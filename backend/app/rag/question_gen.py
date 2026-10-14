"""Grounded question generation: retrieve chunks, then LLM (Ollama) or extractive fallback."""

from __future__ import annotations

from typing import Callable

from app.core.config import RAG_QUESTIONS_PER_BANK
from app.core.utils import QUESTION_TYPES
from app.rag.llm import generate_question_json
from app.rag.retrieve import retrieve_for_lesson, retrieve_for_lessons

DIFFICULTIES = ("easy", "medium", "hard")
MARKS = {"easy": 2, "medium": 4, "hard": 5}


def plan_quota(total: int) -> list[tuple[str, str]]:
    slots: list[tuple[str, str]] = []
    combo = [(t, d) for t in QUESTION_TYPES for d in DIFFICULTIES]
    i = 0
    while len(slots) < max(1, total):
        slots.append(combo[i % len(combo)])
        i += 1
    return slots[:total]


def _context_blob(hits: list[dict]) -> tuple[str, str]:
    if not hits:
        return "", ""
    parts = []
    refs = []
    for h in hits:
        parts.append(h.get("text") or "")
        ref = h.get("sourceReference")
        if ref:
            refs.append(ref)
    return "\n\n".join(parts), ",".join(refs[:4])


def _validate(raw: dict, lesson_tag: str, qtype: str, difficulty: str) -> dict | None:
    text = (raw.get("questionText") or "").strip()
    answer = (raw.get("correctAnswer") or "").strip()
    explanation = (raw.get("explanation") or "").strip()
    if not text or not answer:
        return None
    marks = raw.get("marks")
    try:
        marks = int(marks)
    except (TypeError, ValueError):
        marks = MARKS[difficulty]
    if marks <= 0:
        return None
    options = raw.get("options")
    if options is not None and not isinstance(options, list):
        options = None
    if isinstance(options, list) and len(options) < 2:
        options = None
    qtype_out = raw.get("questionType") or qtype
    if qtype_out not in QUESTION_TYPES:
        return None
    return {
        "questionText": text,
        "questionType": qtype_out,
        "options": options,
        "correctAnswer": answer,
        "explanation": explanation or f"From the lesson notes: {answer}",
        "lessonTag": lesson_tag,
        "difficulty": difficulty if difficulty in DIFFICULTIES else "medium",
        "marks": marks,
        "generatedBy": "RAG",
    }


def _fallback_question(hits: list[dict], lesson_tag: str, qtype: str, difficulty: str) -> dict | None:
    if not hits:
        return None
    chunk = (hits[0].get("text") or "").strip()
    if len(chunk) < 40:
        return None
    snippet = chunk[:280]
    if qtype == "Formula":
        question = "Which statement correctly reflects the relationship described in the notes?"
        correct = snippet[:120]
        options = [correct, "Force equals mass divided by velocity.", "Energy is measured only in newtons.", "Time is a vector quantity."]
    elif qtype == "Calculation":
        question = "Using the method in the notes, which conclusion is supported by the given relations?"
        correct = snippet[:120]
        options = [correct, "The result must be zero.", "Units can be ignored in this calculation.", "Acceleration is always 10 m/s^2."]
    else:
        question = "According to the lesson notes, which of the following is correct?"
        correct = snippet[:120]
        options = [correct, "The notes do not define this quantity.", "This concept does not apply to physics.", "Only experimental error matters here."]
    return {
        "questionText": question,
        "questionType": qtype,
        "options": options,
        "correctAnswer": correct,
        "explanation": snippet,
        "lessonTag": lesson_tag,
        "difficulty": difficulty,
        "marks": MARKS[difficulty],
        "generatedBy": "RAG",
    }


def _one_question(
    lesson_id: str,
    lesson_tag: str,
    lesson_title: str,
    qtype: str,
    difficulty: str,
    llm_model: str,
) -> dict | None:
    query = f"{lesson_title} {lesson_tag} {qtype} {difficulty} physics"
    hits = retrieve_for_lesson(lesson_id, query)
    context, refs = _context_blob(hits)
    raw = generate_question_json(context, lesson_tag, qtype, difficulty, model=llm_model) if context else None
    valid = _validate(raw, lesson_tag, qtype, difficulty) if raw else None
    if valid is None:
        valid = _fallback_question(hits, lesson_tag, qtype, difficulty)
    if valid is None:
        return None
    valid["sourceReference"] = refs or (hits[0].get("sourceReference") if hits else "none")
    return valid


def generate_question_bank(
    lesson_id: str,
    lesson_tag: str,
    lesson_title: str,
    llm_model: str,
    total: int | None = None,
    on_progress: Callable[[int], None] | None = None,
) -> list[dict]:
    quota = plan_quota(total or RAG_QUESTIONS_PER_BANK)
    questions: list[dict] = []
    for i, (qtype, difficulty) in enumerate(quota, start=1):
        q = _one_question(lesson_id, lesson_tag, lesson_title, qtype, difficulty, llm_model)
        if q:
            questions.append(q)
        if on_progress:
            on_progress(int(100 * i / len(quota)))
    return questions


def generate_final_quiz_questions(
    lessons: list[dict],
    llm_model: str,
    per_lesson: int = 3,
    on_progress: Callable[[int], None] | None = None,
) -> list[dict]:
    questions: list[dict] = []
    total_slots = max(1, len(lessons) * per_lesson)
    done = 0
    for lesson in lessons:
        lesson_id = lesson["id"]
        tag = lesson.get("lessonTag") or "unknown"
        title = lesson.get("title") or tag
        quota = plan_quota(per_lesson)
        for qtype, difficulty in quota:
            query = f"{title} {tag} {qtype} physics final quiz"
            hits = retrieve_for_lessons([lesson_id], query)
            context, refs = _context_blob(hits)
            raw = generate_question_json(context, tag, qtype, difficulty, model=llm_model) if context else None
            valid = _validate(raw, tag, qtype, difficulty) if raw else None
            if valid is None:
                valid = _fallback_question(hits, tag, qtype, difficulty)
            if valid:
                valid["sourceLessonId"] = lesson_id
                valid["sourceReference"] = refs or "none"
                questions.append(valid)
            done += 1
            if on_progress:
                on_progress(int(100 * done / total_slots))
    return questions
