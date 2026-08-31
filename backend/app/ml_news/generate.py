"""Scenario + question generation: LLM if available, otherwise a syllabus template."""

from __future__ import annotations

import json
import os
import re

import httpx

from app.core.config import OLLAMA_BASE_URL, OLLAMA_MODEL
from app.ml_news import match_concept


def _parse_json(raw: str) -> dict | None:
    text = (raw or "").strip()
    match = re.search(r"\{.*\}", text, re.DOTALL)
    if not match:
        return None
    try:
        data = json.loads(match.group(0))
    except json.JSONDecodeError:
        return None
    return data if isinstance(data, dict) else None


def _template_pack(news: str, concept: dict) -> dict:
    topic = concept["topic"]
    grade = concept["questionGrade"]
    formula = concept["formula"]
    scenario = (
        f"A Grade {grade} class reads this news: {news.strip()} "
        f"The teacher links it to {topic} using {formula}."
    )
    question = (
        f"Using the news above, explain the physics of {topic} in 2 or 3 sentences "
        f"and include {formula} if it applies."
    )
    reference = (
        f"{topic} is the physics idea in this news. "
        f"A correct answer names the concept, uses {formula}, and connects it to the event. "
        f"{concept.get('gradeNote') or ''}"
    ).strip()
    return {
        "scenario": scenario,
        "question": question,
        "referenceAnswer": reference,
        "usedLlm": False,
    }


def _ollama_generate(prompt: str) -> str | None:
    try:
        with httpx.Client(timeout=45.0) as client:
            tags = client.get(f"{OLLAMA_BASE_URL.rstrip('/')}/api/tags")
            tags.raise_for_status()
            names = [m.get("name") or "" for m in (tags.json() or {}).get("models") or []]
            model = next((n for n in names if OLLAMA_MODEL in n), names[0] if names else None)
            if not model:
                return None
            response = client.post(
                f"{OLLAMA_BASE_URL.rstrip('/')}/api/generate",
                json={"model": model, "prompt": prompt, "stream": False, "format": "json"},
            )
            response.raise_for_status()
            return (response.json() or {}).get("response") or ""
    except Exception:
        return None


def _groq_generate(prompt: str) -> str | None:
    api_key = os.getenv("GROQ_API_KEY", "").strip()
    if not api_key:
        return None
    try:
        with httpx.Client(timeout=45.0) as client:
            response = client.post(
                "https://api.groq.com/openai/v1/chat/completions",
                headers={"Authorization": f"Bearer {api_key}"},
                json={
                    "model": "llama-3.1-8b-instant",
                    "temperature": 0.3,
                    "messages": [{"role": "user", "content": prompt}],
                },
            )
            response.raise_for_status()
            return response.json()["choices"][0]["message"]["content"]
    except Exception:
        return None


def generate_scenario_question(news: str, student_grade: int | None) -> dict:
    concept = match_concept(news, student_grade)
    prompt = f"""You are a Sri Lankan O/L physics teacher.
Turn this news into ONE short realistic classroom scenario and ONE open question.
Student grade: {concept['questionGrade']}. Topic: {concept['topic']}. Formula: {concept['formula']}.
Return JSON only with keys: scenario, question, referenceAnswer.
- scenario: 2-4 sentences, real-world, no markdown
- question: one answerable physics question for that grade
- referenceAnswer: a marking-scheme style answer, 2-4 sentences
News: {news[:1200]}
"""
    raw = _groq_generate(prompt) or _ollama_generate(prompt)
    parsed = _parse_json(raw or "")
    if parsed and parsed.get("question") and parsed.get("referenceAnswer"):
        return {
            "scenario": str(parsed.get("scenario") or "").strip(),
            "question": str(parsed["question"]).strip(),
            "referenceAnswer": str(parsed["referenceAnswer"]).strip(),
            "usedLlm": True,
            "concept": concept,
        }
    pack = _template_pack(news, concept)
    pack["concept"] = concept
    return pack
