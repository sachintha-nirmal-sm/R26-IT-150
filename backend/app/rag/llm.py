"""Optional Ollama JSON generator. Returns None if Ollama is not running."""

from __future__ import annotations

import json
import re

import httpx

from app.core.config import OLLAMA_BASE_URL, OLLAMA_MODEL

QUESTION_TYPES = ("Theory", "Formula", "Calculation")
DIFFICULTIES = ("easy", "medium", "hard")


def generate_question_json(
    context: str,
    lesson_tag: str,
    question_type: str,
    difficulty: str,
    model: str | None = None,
) -> dict | None:
    prompt = f"""You are a Sri Lankan O/L–A/L physics teacher.
Using ONLY the context below, write ONE {difficulty} {question_type} question.
Return a single JSON object with keys:
questionText, questionType, options, correctAnswer, explanation, difficulty, marks.
- questionType must be exactly "{question_type}"
- difficulty must be exactly "{difficulty}"
- options: array of 4 strings for Formula/Calculation/Theory MCQ, or null for open Theory
- correctAnswer must match one option when options are present
- marks: integer 2 (easy), 4 (medium), or 5 (hard)
- Do not invent facts that are not in the context.

CONTEXT:
{context[:3500]}
"""
    payload = {
        "model": model or OLLAMA_MODEL,
        "prompt": prompt,
        "stream": False,
        "format": "json",
        "options": {"temperature": 0.4},
    }
    try:
        with httpx.Client(timeout=90.0) as client:
            response = client.post(f"{OLLAMA_BASE_URL.rstrip('/')}/api/generate", json=payload)
            response.raise_for_status()
            raw = (response.json() or {}).get("response") or ""
    except Exception:
        return None

    data = _parse_json(raw)
    if not data:
        return None
    data["lessonTag"] = lesson_tag
    data["questionType"] = question_type
    data["difficulty"] = difficulty
    return data


def _parse_json(raw: str) -> dict | None:
    text = (raw or "").strip()
    match = re.search(r"\{.*\}", text, re.DOTALL)
    if match:
        text = match.group(0)
    try:
        data = json.loads(text)
    except json.JSONDecodeError:
        return None
    return data if isinstance(data, dict) else None
