"""
generate_questions.py — AI-powered quiz question generation + accuracy verification.
  POST /admin/lessons/{lesson_id}/generate-questions
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

router = APIRouter(prefix="/admin/lessons", tags=["Admin - Generate"])


class GenerateRequest(BaseModel):
    model: Literal["gemini", "groq", "mistral", "openrouter"]
    count: int = Field(default=10, ge=5, le=30)


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


def _sanitize(text: str) -> str:
    """Remove corrupted UTF-8 mojibake characters that some LLMs output."""
    return (text
            .replace("â€¢", "")   # â€¢ → corrupted bullet
            .replace("â€”", "-")   # â€" → corrupted em-dash
            .replace("â€·", "")    # â€· → corrupted middle dot
            .replace("•", "")                     # plain bullet
            .strip())


def _sanitize_question(q: dict) -> dict:
    q["question"] = _sanitize(q.get("question", ""))
    q["explanation"] = _sanitize(q.get("explanation", ""))
    for opt in q.get("options", []):
        opt["text"] = _sanitize(opt.get("text", ""))
    return q


def _extract_json(raw: str) -> list[dict]:
    raw = re.sub(r"```(?:json)?\s*", "", raw).strip()
    start = raw.find("[")
    end = raw.rfind("]")
    if start == -1 or end == -1:
        raise ValueError("No JSON array found")
    questions = json.loads(raw[start : end + 1])
    return [_sanitize_question(q) for q in questions]


# ---------------------------------------------------------------------------
# AI model callers
# ---------------------------------------------------------------------------

async def _call_gemini(prompt: str) -> str:
    api_key = os.getenv("GEMINI_API_KEY", "")
    async with httpx.AsyncClient(timeout=60) as client:
        resp = await client.post(
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent",
            headers={"x-goog-api-key": api_key, "Content-Type": "application/json"},
            json={"contents": [{"parts": [{"text": prompt}]}]},
        )
        resp.raise_for_status()
    return resp.json()["candidates"][0]["content"]["parts"][0]["text"]


async def _call_groq(prompt: str) -> str:
    from groq import Groq
    client = Groq(api_key=os.getenv("GROQ_API_KEY", ""))
    loop = asyncio.get_event_loop()
    resp = await loop.run_in_executor(
        None,
        lambda: client.chat.completions.create(
            model="openai/gpt-oss-120b",
            messages=[{"role": "user", "content": prompt}],
            temperature=0.3,
            max_tokens=8000,
        ),
    )
    return resp.choices[0].message.content


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
                "model": "mistral-large-latest",
                "messages": [{"role": "user", "content": prompt}],
                "temperature": 0.3,
            },
        )
        resp.raise_for_status()
    return resp.json()["choices"][0]["message"]["content"]


# ---------------------------------------------------------------------------
# Accuracy verification (always uses OpenRouter as independent judge)
# ---------------------------------------------------------------------------

async def _verify_accuracy(content: str, questions: list[dict]) -> list[dict]:
    if not content.strip() or not questions:
        for q in questions:
            q["accuracyScore"] = None
            q["accuracyReason"] = "No content to verify against"
            q["accuracyVerified"] = False
        return questions

    q_lines = []
    for i, q in enumerate(questions):
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
        raw = await _call_groq(prompt)
        scores = _extract_json(raw)
        score_map = {
            item.get("index", -1): item
            for item in scores
            if isinstance(item, dict)
        }
        for i, q in enumerate(questions):
            item = score_map.get(i, {})
            q["accuracyScore"] = min(100, max(0, int(item.get("score", 0))))
            q["accuracyReason"] = str(item.get("reason", ""))[:300]
            q["accuracyVerified"] = True
    except Exception:
        for q in questions:
            q["accuracyScore"] = None
            q["accuracyReason"] = "Verification unavailable"
            q["accuracyVerified"] = False

    return questions


# ---------------------------------------------------------------------------
# Endpoint
# ---------------------------------------------------------------------------

@router.post("/{lesson_id}/generate-questions", status_code=status.HTTP_201_CREATED)
async def generate_questions(
    lesson_id: str,
    body: GenerateRequest,
    admin: VerifiedUser = Depends(require_admin),
):
    content = await _build_content(lesson_id)
    prompt = _PROMPT.format(count=body.count, content=content[:8000])

    try:
        if body.model == "gemini":
            raw = await _call_gemini(prompt)
        elif body.model == "groq":
            raw = await _call_groq(prompt)
        elif body.model == "openrouter":
            raw = await _call_openrouter(prompt)
        else:
            raw = await _call_mistral(prompt)
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"{body.model} error: {e}")

    try:
        questions = _extract_json(raw)
    except Exception:
        raise HTTPException(
            status_code=500,
            detail=f"Could not parse AI response: {raw[:300]}",
        )

    # Verify accuracy against source content
    questions = await _verify_accuracy(content, questions)

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

    # Save new questions with accuracy scores and difficulty
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
        "avgAccuracyScore": avg_score,
        "verifiedCount": len(verified),
        "flaggedCount": flagged,
        "generatedBy": admin.uid,
        "createdAt": firestore.SERVER_TIMESTAMP,
    })

    return {
        "generated": count,
        "avgAccuracyScore": avg_score,
        "verifiedCount": len(verified),
        "flaggedCount": flagged,
    }
