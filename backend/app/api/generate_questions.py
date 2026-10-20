"""
generate_questions.py — AI-powered quiz question generation + accuracy verification.
  POST /admin/lessons/{lesson_id}/generate-questions

  If the lesson has sub-lessons defined, questions are generated separately for
  each sub-lesson in parallel and tagged with subLessonId / subLessonNumber.
  If no sub-lessons exist the original behaviour is preserved (no tags).
"""

import asyncio
import io
import json
import os
import re
from typing import Literal

import httpx
import pypdf
from fastapi import APIRouter, Depends, HTTPException, status
from google.cloud import firestore
from pydantic import BaseModel, Field

from app.core.dependencies import VerifiedUser, require_admin
from app.core.firebase import db
from app.services.analytics_service import backfill_source_question_performance

router = APIRouter(prefix="/admin/lessons", tags=["Admin - Generate"])


class GenerateRequest(BaseModel):
    model: Literal["gemini", "groq", "mistral", "openrouter"]
    count: int = Field(default=5, ge=0, le=10)
    # Per-sub-lesson counts: {subLessonId: count}. When present, overrides `count` per sub-lesson.
    sub_lesson_counts: dict[str, int] | None = None


# ---------------------------------------------------------------------------
# Prompts
# ---------------------------------------------------------------------------

# Used when lesson has NO sub-lessons (original behaviour)
_PROMPT = """You are a physics teacher creating a quiz.
Based on the content below, generate exactly {count} multiple-choice questions.

Rules:
- Each question has exactly 4 options labeled A, B, C, D
- Exactly one option is correct
- Base questions strictly on the provided content
- Return ONLY a valid JSON array with no markdown fences, no explanation
- Assign a difficulty to every question using these criteria:
    Easy   — basic recall or single-step reasoning
    Medium — multi-step reasoning or formula application
    Hard   — complex problem-solving or synthesis of multiple concepts

Required format:
[
  {{
    "question": "Question text?",
    "options": [
      {{"label": "A", "text": "Option A"}},
      {{"label": "B", "text": "Option B"}},
      {{"label": "C", "text": "Option C"}},
      {{"label": "D", "text": "Option D"}}
    ],
    "correct": "A",
    "explanation": "Why A is correct",
    "difficulty": "Easy"
  }}
]

Content:
{content}
"""

# Used per sub-lesson — keeps questions focused on the specific sub-topic
_SUB_LESSON_PROMPT = """You are a physics teacher creating a quiz.
Based on the content below, generate exactly {count} multiple-choice questions
focused ONLY on the sub-topic: {sub_lesson_number} - {sub_lesson_title}.

Rules:
- Every question must be directly about "{sub_lesson_title}"
- Each question has exactly 4 options labeled A, B, C, D
- Exactly one option is correct
- Base questions strictly on the provided content
- Return ONLY a valid JSON array with no markdown fences, no explanation
- Assign a difficulty to every question using these criteria:
    Easy   — basic recall or single-step reasoning
    Medium — multi-step reasoning or formula application
    Hard   — complex problem-solving or synthesis of multiple concepts

Required format:
[
  {{
    "question": "Question text?",
    "options": [
      {{"label": "A", "text": "Option A"}},
      {{"label": "B", "text": "Option B"}},
      {{"label": "C", "text": "Option C"}},
      {{"label": "D", "text": "Option D"}}
    ],
    "correct": "A",
    "explanation": "Why A is correct",
    "difficulty": "Easy"
  }}
]

Content:
{content}
"""

_VERIFY_PROMPT = """You are a content accuracy auditor for a physics quiz system.
Given the lesson content and generated questions below, score each question 0-100 based on how well it is grounded in the provided content.

Scoring guide:
- 90-100: Question and correct answer are explicitly stated in the content
- 60-89:  Strongly implied by the content
- 30-59:  Loosely related; correct answer may not be verifiable from content
- 0-29:   Not supported by content or potentially incorrect

Return ONLY a JSON array with no markdown:
[{{"index": 0, "score": 85, "reason": "One-line reason"}}]

Lesson Content:
{content}

Questions to verify:
{questions}
"""


# ---------------------------------------------------------------------------
# PDF + content helpers
# ---------------------------------------------------------------------------

async def _fetch_pdf_text(url: str) -> str:
    async with httpx.AsyncClient(timeout=30) as client:
        r = await client.get(url)
        r.raise_for_status()
    reader = pypdf.PdfReader(io.BytesIO(r.content))
    return "\n".join(page.extract_text() or "" for page in reader.pages)


async def _build_content(lesson_id: str) -> str:
    pdf_docs = list(
        db.collection("lessons").document(lesson_id).collection("pdfs").stream()
    )
    if pdf_docs:
        texts = []
        for doc in pdf_docs:
            d = doc.to_dict()
            url = d.get("url", "")
            name = d.get("fileName", "PDF")
            if url:
                try:
                    text = await _fetch_pdf_text(url)
                    if text.strip():
                        texts.append(f"=== {name} ===\n{text}")
                except Exception:
                    pass
        if texts:
            return "\n\n".join(texts)

    lesson = db.collection("lessons").document(lesson_id).get()
    if lesson.exists:
        d = lesson.to_dict()
        return (
            f"Physics lesson: {d.get('title', '')} "
            f"(Grade {d.get('grade', '')}). "
            f"Topic: {d.get('lessonTag', '')}. "
            f"{d.get('description', '')}"
        )
    return "General high school physics"


def _fetch_sub_lessons(lesson_id: str) -> list[dict]:
    docs = (
        db.collection("lessons")
        .document(lesson_id)
        .collection("subLessons")
        .order_by("order")
        .stream()
    )
    result = []
    for doc in docs:
        d = doc.to_dict()
        result.append({
            "id": doc.id,
            "number": d.get("number", ""),
            "title": d.get("title", ""),
            "order": d.get("order", 0),
        })
    return result


def _extract_json(raw: str | None) -> list[dict]:
    if not raw or not raw.strip():
        raise ValueError("the model returned an empty response")
    cleaned = re.sub(r"```(?:json)?\s*", "", raw).strip()
    start = cleaned.find("[")
    end = cleaned.rfind("]")
    if start == -1 or end == -1 or end < start:
        raise ValueError(f"no JSON array in the model response: {cleaned[:150]!r}")
    return json.loads(cleaned[start : end + 1])


async def _ask_for_questions(call_model, prompt: str, attempts: int = 3) -> list[dict]:
    """Calls the model and parses its JSON, retrying when the reply is empty
    or isn't valid JSON — reasoning models occasionally spend their whole
    token budget thinking, or wrap the answer in prose."""
    last_error: Exception | None = None
    for attempt in range(attempts):
        try:
            questions = _extract_json(await call_model(prompt))
            if questions:
                return questions
            last_error = ValueError(
                "the model returned no questions — the lesson's PDF may not "
                "cover this sub-topic, check the sub-lessons match the PDF"
            )
        except (ValueError, json.JSONDecodeError) as exc:
            last_error = exc
        await asyncio.sleep(1.0 + attempt)
    raise ValueError(f"{last_error} (after {attempts} attempts)")


def _relevant_content(content: str, title: str, limit: int = 8000) -> str:
    """The slice of the lesson text to send for one sub-topic: a window
    around where the sub-topic is first mentioned, instead of always the
    first 8000 characters (which may not cover it at all)."""
    if len(content) <= limit:
        return content
    index = content.lower().find((title or "").lower())
    if index == -1:
        return content[:limit]
    start = max(0, index - limit // 4)
    return content[start : start + limit]


# ---------------------------------------------------------------------------
# AI model callers
# ---------------------------------------------------------------------------

async def _call_gemini(prompt: str) -> str:
    api_key = os.getenv("GEMINI_API_KEY", "")
    async with httpx.AsyncClient(timeout=60) as client:
        resp = await client.post(
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent",
            headers={"x-goog-api-key": api_key, "Content-Type": "application/json"},
            json={"contents": [{"parts": [{"text": prompt}]}]},
        )
        resp.raise_for_status()
    return resp.json()["candidates"][0]["content"]["parts"][0]["text"]


async def _call_groq(prompt: str) -> str:
    from groq import AsyncGroq
    api_key = os.getenv("GROQ_API_KEY", "")
    if not api_key:
        raise ValueError("GROQ_API_KEY is not set")
    client = AsyncGroq(api_key=api_key)
    try:
        resp = await client.chat.completions.create(
            model="openai/gpt-oss-20b",
            messages=[{"role": "user", "content": prompt}],
            temperature=0.3,
            # gpt-oss is a reasoning model: its thinking counts against this
            # budget, so a small limit can leave the visible answer empty.
            max_tokens=8000,
            reasoning_effort="low",
        )
        choice = resp.choices[0]
        if not (choice.message.content or "").strip():
            raise ValueError(
                f"Groq returned no answer (finish_reason={choice.finish_reason})"
            )
        return choice.message.content
    except Exception as e:
        print(f"[Groq] API error: {type(e).__name__}: {e}")
        raise


async def _call_openrouter(prompt: str) -> str:
    async with httpx.AsyncClient(timeout=120) as client:
        resp = await client.post(
            "https://openrouter.ai/api/v1/chat/completions",
            headers={
                "Authorization": f"Bearer {os.getenv('OPENROUTER_API_KEY', '')}",
                "Content-Type": "application/json",
                "X-Title": "PhysicsLab",
            },
            json={
                "model": "nvidia/nemotron-3-super-120b-a12b:free",
                "messages": [{"role": "user", "content": prompt}],
                "temperature": 0.3,
            },
        )
        resp.raise_for_status()
    return resp.json()["choices"][0]["message"]["content"]


async def _call_mistral(prompt: str) -> str:
    async with httpx.AsyncClient(timeout=120) as client:
        resp = await client.post(
            "https://api.mistral.ai/v1/chat/completions",
            headers={
                "Authorization": f"Bearer {os.getenv('MISTRAL_API_KEY', '')}",
                "Content-Type": "application/json",
            },
            json={
                # mistral-large-latest is rejected (403) on free-tier keys.
                "model": "open-mistral-nemo",
                "messages": [{"role": "user", "content": prompt}],
                "temperature": 0.3,
            },
        )
        resp.raise_for_status()
    return resp.json()["choices"][0]["message"]["content"]


# ---------------------------------------------------------------------------
# Sub-lesson generation helper
# ---------------------------------------------------------------------------

async def _generate_for_sub_lesson(
    content: str,
    sub_lesson: dict,
    count: int,
    call_model,
) -> list[dict]:
    prompt = _SUB_LESSON_PROMPT.format(
        count=count,
        sub_lesson_number=sub_lesson["number"],
        sub_lesson_title=sub_lesson["title"],
        content=_relevant_content(content, sub_lesson["title"]),
    )
    # Let exceptions propagate — caller surfaces the real error message
    questions = await _ask_for_questions(call_model, prompt)

    for q in questions:
        q["subLessonId"] = sub_lesson["id"]
        q["subLessonNumber"] = sub_lesson["number"]
        q["subLessonTitle"] = sub_lesson["title"]

    return questions


# ---------------------------------------------------------------------------
# Accuracy verification (always uses Groq as independent judge)
# Batched to 10 questions per call so large sub-lesson sets don't overwhelm the API.
# ---------------------------------------------------------------------------

async def _verify_batch(content: str, batch: list[dict], call_model) -> None:
    """Verify one batch of ≤10 questions in-place using the same model that generated them."""
    q_lines = []
    for i, q in enumerate(batch):
        correct_text = next(
            (o.get("text", "") for o in q.get("options", []) if o.get("label") == q.get("correct")),
            "",
        )
        q_lines.append(
            f"{i}. {q.get('question', '')}\n"
            f"   Correct: {q.get('correct', '')} — {correct_text}"
        )

    prompt = _VERIFY_PROMPT.format(
        content=content[:5000],
        questions="\n".join(q_lines),
    )

    try:
        raw = await call_model(prompt)
        scores = _extract_json(raw)
        score_map = {
            item.get("index", -1): item
            for item in scores
            if isinstance(item, dict)
        }
        for i, q in enumerate(batch):
            item = score_map.get(i, {})
            q["accuracyScore"] = min(100, max(0, int(item.get("score", 0))))
            q["accuracyReason"] = str(item.get("reason", ""))[:300]
            q["accuracyVerified"] = True
    except Exception:
        for q in batch:
            q["accuracyScore"] = None
            q["accuracyReason"] = "Verification unavailable"
            q["accuracyVerified"] = False


async def _verify_accuracy(content: str, questions: list[dict], call_model) -> list[dict]:
    if not content.strip() or not questions:
        for q in questions:
            q["accuracyScore"] = None
            q["accuracyReason"] = "No content to verify against"
            q["accuracyVerified"] = False
        return questions

    # Split into batches of 10 and run in parallel
    _BATCH = 10
    batches = [questions[i : i + _BATCH] for i in range(0, len(questions), _BATCH)]
    await asyncio.gather(*[_verify_batch(content, b, call_model) for b in batches])

    return questions


# ---------------------------------------------------------------------------
# Endpoint
# ---------------------------------------------------------------------------


# ---------------------------------------------------------------------------
# Publish admin-generated questions to the student quiz flow
# ---------------------------------------------------------------------------
# The admin "Generate" tab stores questions in lessons/{id}/questions and the
# "Settings" tab in lessons/{id}/quizSettings/config, but students take
# quizzes through quiz_service, which only reads lessons/{id}/quizzes/{quiz}
# with an active question-bank version. This copies one into the other.

ADMIN_QUIZ_ID = "admin-generated"
_DIFFICULTY_MARKS = {"easy": 1, "medium": 1, "hard": 2}


def _to_student_question(data: dict, lesson_tag: str) -> dict | None:
    """Admin format ({label,text} options, correct = "A") -> student format
    (plain option strings, correctAnswer = the option's text)."""
    raw_options = data.get("options") or []
    texts: list[str] = []
    labelled: dict[str, str] = {}
    for option in raw_options:
        if isinstance(option, dict):
            text = str(option.get("text", "")).strip()
            label = str(option.get("label", "")).strip().upper()
            if label:
                labelled[label] = text
        else:
            text = str(option).strip()
        if text:
            texts.append(text)
    if len(texts) < 2:
        return None

    correct = str(data.get("correct", "")).strip()
    answer = labelled.get(correct.upper()) or next(
        (t for t in texts if t.lower() == correct.lower()), None
    )
    question = str(data.get("question", "")).strip()
    if not answer or not question:
        return None

    difficulty = str(data.get("difficulty", "Medium")).lower()
    if difficulty not in _DIFFICULTY_MARKS:
        difficulty = "medium"
    return {
        "questionText": question,
        "questionType": "Theory",
        "options": texts,
        "correctAnswer": answer,
        "explanation": data.get("explanation", ""),
        "lessonTag": data.get("subLessonTitle") or lesson_tag,
        "difficulty": difficulty,
        "marks": _DIFFICULTY_MARKS[difficulty],
        "generatedBy": data.get("generatedBy", "admin"),
    }


SUB_LESSON_QUIZ_PREFIX = "admin-sl-"


def _write_quiz(lesson_ref, quiz_id: str, title: str, items: dict[str, dict],
                per_attempt: int, extra: dict | None = None) -> None:
    """Writes one student-visible quiz (bank version v1 + the quiz doc)."""
    quiz_ref = lesson_ref.collection("quizzes").document(quiz_id)
    version_ref = quiz_ref.collection("questionBankVersions").document("v1")

    writes: list[tuple] = []  # (ref, data | None) — None means delete
    for old in version_ref.collection("questions").stream():
        writes.append((old.reference, None))
    for qid, item in items.items():
        writes.append((version_ref.collection("questions").document(qid),
                       {**item, "createdAt": firestore.SERVER_TIMESTAMP}))
    writes.append((version_ref, {
        "versionNumber": 1,
        "status": "active",
        "totalQuestions": len(items),
        "generatedBy": "admin",
        "createdAt": firestore.SERVER_TIMESTAMP,
        "archivedAt": None,
    }))
    writes.append((quiz_ref, {
        "title": title,
        "quizId": quiz_id,
        "lessonId": lesson_ref.id,
        # Generous: the earlier direct-Firestore quiz had no attempt limit.
        "maxAttempts": 10,
        "questionsPerAttempt": max(1, min(per_attempt, len(items))),
        "activeQuestionBankVersionId": "v1",
        "status": "bankReady",
        "updatedAt": firestore.SERVER_TIMESTAMP,
        **(extra or {}),
    }))

    # Firestore batches hold at most 500 writes.
    for i in range(0, len(writes), 400):
        batch = db.batch()
        for ref, data in writes[i : i + 400]:
            if data is None:
                batch.delete(ref)
            else:
                batch.set(ref, data, merge=ref.parent.id == "quizzes")
        batch.commit()


def _publish_student_quiz(lesson_id: str) -> dict:
    """Publishes the lesson-level quiz plus one quiz per sub-lesson (the app
    opens a sub-lesson's quiz from the lesson's sections list)."""
    lesson_ref = db.collection("lessons").document(lesson_id)
    lesson = lesson_ref.get().to_dict() or {}
    settings = lesson_ref.collection("quizSettings").document("config").get().to_dict() or {}
    quizzes = lesson_ref.collection("quizzes")

    admin_quiz_ids = [
        d.id for d in quizzes.stream()
        if d.id == ADMIN_QUIZ_ID or d.id.startswith(SUB_LESSON_QUIZ_PREFIX)
    ]

    # The Settings tab hides its Publish switch when a lesson has sections —
    # each section then has its own isPublished flag — so for those lessons
    # a published section is what makes the quizzes live.
    sub_docs = [(d.id, d.to_dict() or {}) for d in lesson_ref.collection("subLessons").stream()]
    if sub_docs:
        is_live = any(data.get("isPublished") for _, data in sub_docs)
    else:
        is_live = bool(settings.get("isPublished"))

    if not is_live:
        for quiz_id in admin_quiz_ids:
            quizzes.document(quiz_id).update({
                "status": "unpublished", "updatedAt": firestore.SERVER_TIMESTAMP,
            })
        return {"published": False, "questions": 0, "subLessonQuizzes": 0}

    docs = {d.id: d.to_dict() or {} for d in lesson_ref.collection("questions").stream()}
    selected = set(settings.get("selectedQuestionIds") or [])
    if settings.get("mode") == "manual" and selected:
        ids = [qid for qid in docs if qid in selected]
    else:
        ids = list(docs)

    lesson_tag = lesson.get("lessonTag") or lesson.get("title") or lesson_id
    converted: dict[str, dict] = {}
    for qid in ids:
        item = _to_student_question(docs[qid], lesson_tag)
        if item:
            # Keep the link back to the document shown in the Admin Questions
            # tab so real student responses can update its ML statistics.
            item["sourceQuestionId"] = qid
            item["subLessonId"] = docs[qid].get("subLessonId")
            converted[qid] = item
    if not converted:
        raise HTTPException(
            status_code=409,
            detail="There are no usable questions to publish. Generate questions first.",
        )

    lesson_count = int(settings.get("questionCount") or 10)
    _write_quiz(lesson_ref, ADMIN_QUIZ_ID, f"{lesson.get('title', 'Lesson')} Quiz",
                converted, lesson_count)

    live_ids = {ADMIN_QUIZ_ID}
    sub_quizzes = 0
    for sub_id, data in sub_docs:
        if not data.get("isPublished"):
            continue
        items = {qid: q for qid, q in converted.items() if q.get("subLessonId") == sub_id}
        if not items:
            continue
        quiz_id = f"{SUB_LESSON_QUIZ_PREFIX}{sub_id}"
        _write_quiz(
            lesson_ref, quiz_id,
            f"{data.get('number', '')} {data.get('title', 'Section')}".strip(),
            items, int(data.get("quizCount") or lesson_count),
            extra={"subLessonId": sub_id},
        )
        live_ids.add(quiz_id)
        sub_quizzes += 1

    # Sections that no longer have questions shouldn't keep an old quiz.
    for quiz_id in admin_quiz_ids:
        if quiz_id not in live_ids:
            quizzes.document(quiz_id).update({
                "status": "unpublished", "updatedAt": firestore.SERVER_TIMESTAMP,
            })

    return {
        "published": True,
        "questions": len(converted),
        "questionsPerAttempt": max(1, min(lesson_count, len(converted))),
        "subLessonQuizzes": sub_quizzes,
    }


@router.post("/{lesson_id}/publish-quiz")
def publish_quiz(lesson_id: str, admin: VerifiedUser = Depends(require_admin)) -> dict:
    """Make the generated questions + saved settings visible to students."""
    if not db.collection("lessons").document(lesson_id).get().exists:
        raise HTTPException(status_code=404, detail="Lesson not found.")
    return _publish_student_quiz(lesson_id)


@router.post("/{lesson_id}/sync-question-performance")
def sync_question_performance(
    lesson_id: str, admin: VerifiedUser = Depends(require_admin)
) -> dict:
    """Backfill source-question stats from attempts made before live syncing."""
    return backfill_source_question_performance(lesson_id)


@router.post("/{lesson_id}/generate-questions", status_code=status.HTTP_201_CREATED)
async def generate_questions(
    lesson_id: str,
    body: GenerateRequest,
    admin: VerifiedUser = Depends(require_admin),
):
    content = await _build_content(lesson_id)
    sub_lessons = _fetch_sub_lessons(lesson_id)

    # Wrap model selection into a single callable for reuse
    async def call_model(prompt: str) -> str:
        if body.model == "gemini":
            return await _call_gemini(prompt)
        elif body.model == "groq":
            return await _call_groq(prompt)
        elif body.model == "openrouter":
            return await _call_openrouter(prompt)
        else:
            return await _call_mistral(prompt)

    skipped: list[dict] = []
    try:
        if sub_lessons:
            sub_lesson_tasks = [
                (sl, (body.sub_lesson_counts.get(sl["id"], body.count) if body.sub_lesson_counts else body.count))
                for sl in sub_lessons
                if (body.sub_lesson_counts.get(sl["id"], body.count) if body.sub_lesson_counts else body.count) > 0
            ]
            questions: list[dict] = []
            # A sub-lesson the model can't write questions for (e.g. its title
            # doesn't match the PDF) is skipped and reported, instead of
            # failing the sub-lessons that did work.

            def _note_skip(sl: dict, reason: object) -> None:
                skipped.append({
                    "subLesson": f"{sl['number']} {sl['title']}",
                    "reason": str(reason),
                })

            if body.model == "groq":
                # Groq free tier has strict rate limits — run sequentially with a delay
                for sl, sl_count in sub_lesson_tasks:
                    try:
                        result = await _generate_for_sub_lesson(content, sl, sl_count, call_model)
                        questions.extend(result)
                    except Exception as exc:  # noqa: BLE001
                        _note_skip(sl, exc)
                    await asyncio.sleep(1.0)
            else:
                # Other models handle parallel calls fine
                tasks = [_generate_for_sub_lesson(content, sl, sl_count, call_model) for sl, sl_count in sub_lesson_tasks]
                results = await asyncio.gather(*tasks, return_exceptions=True)
                for (sl, _), r in zip(sub_lesson_tasks, results):
                    if isinstance(r, list):
                        questions.extend(r)
                    else:
                        _note_skip(sl, r)
            if not questions and skipped:
                raise ValueError("; ".join(f"{x['subLesson']}: {x['reason']}" for x in skipped))
        else:
            # Original behaviour — single call, no sub-lesson tags
            prompt = _PROMPT.format(count=body.count, content=content[:8000])
            questions = await _ask_for_questions(call_model, prompt)

    except Exception as e:
        raise HTTPException(status_code=500, detail=f"{body.model} error: {e}")

    if not questions:
        raise HTTPException(status_code=500, detail="AI returned no questions.")

    # Verify accuracy against source content (uses the same model that generated the questions)
    questions = await _verify_accuracy(content, questions, call_model)

    # Compute accuracy stats
    verified = [q for q in questions if q.get("accuracyVerified") and q.get("accuracyScore") is not None]
    avg_score = round(sum(q["accuracyScore"] for q in verified) / len(verified)) if verified else 0
    flagged = len([q for q in verified if q["accuracyScore"] < 50])

    questions_ref = db.collection("lessons").document(lesson_id).collection("questions")

    # Delete all existing questions
    existing_docs = list(questions_ref.stream())
    if existing_docs:
        delete_batch = db.batch()
        for doc in existing_docs:
            delete_batch.delete(doc.reference)
        delete_batch.commit()

    # Save new questions with accuracy scores, difficulty, and sub-lesson tags
    _valid_difficulties = {"Easy", "Medium", "Hard"}
    save_batch = db.batch()
    count = 0
    for q in questions:
        difficulty = q.get("difficulty", "Medium")
        if difficulty not in _valid_difficulties:
            difficulty = "Medium"
        ref = questions_ref.document()
        save_batch.set(ref, {
            "question": q.get("question", ""),
            "options": q.get("options", []),
            "correct": q.get("correct", ""),
            "explanation": q.get("explanation", ""),
            "difficulty": difficulty,
            "generatedBy": body.model,
            "accuracyScore": q.get("accuracyScore"),
            "accuracyReason": q.get("accuracyReason", ""),
            "accuracyVerified": q.get("accuracyVerified", False),
            # Sub-lesson fields (None when no sub-lessons are defined)
            "subLessonId": q.get("subLessonId"),
            "subLessonNumber": q.get("subLessonNumber"),
            "subLessonTitle": q.get("subLessonTitle"),
            "attempts": 0,
            "correctCount": 0,
            "actualDifficulty": None,
            "difficultyMatch": None,
            "createdAt": firestore.SERVER_TIMESTAMP,
        })
        count += 1
    save_batch.commit()

    # Save generation session record for model comparison
    db.collection("lessons").document(lesson_id).collection("generationSessions").document().set({
        "model": body.model,
        "questionCount": count,
        "subLessonCount": len(sub_lessons),
        "countPerSubLesson": body.count if sub_lessons else None,
        "avgAccuracyScore": avg_score,
        "verifiedCount": len(verified),
        "flaggedCount": flagged,
        "generatedBy": admin.uid,
        "createdAt": firestore.SERVER_TIMESTAMP,
    })

    # If the quiz is already published to students, refresh what they see.
    republished = None
    try:
        settings = db.collection("lessons").document(lesson_id).collection("quizSettings").document("config").get().to_dict() or {}
        if settings.get("isPublished"):
            republished = _publish_student_quiz(lesson_id)
    except Exception as exc:  # noqa: BLE001
        print(f"[generate_questions] republish failed: {exc}")

    return {
        "republished": republished,
        "generated": count,
        "subLessonCount": len(sub_lessons),
        "avgAccuracyScore": avg_score,
        "verifiedCount": len(verified),
        "flaggedCount": flagged,
        "skipped": skipped,
    }
