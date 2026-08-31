"""News-to-question and student-answer evaluation APIs (one-day checklist)."""

from __future__ import annotations

import json
import re
import time
from pathlib import Path

from fastapi import APIRouter, HTTPException
from pydantic import BaseModel, Field

from app.ml_news.features import _tokens, jaccard
from app.ml_news.generate import generate_scenario_question
from app.ml_news.runtime import models_ready, predict_answer, predict_news

router = APIRouter(prefix="/news", tags=["News Scenario Pipeline"])
ARTIFACT_DIR = Path(__file__).resolve().parent.parent / "ml_news" / "artifacts"

SAMPLE_NEWS = [
    {
        "id": "density-harbour",
        "title": "Harbour water density after rain",
        "text": "Sri Lankan researchers measured the density of seawater near Colombo harbour after heavy rain mixed freshwater into the ocean.",
        "expected": "physics",
    },
    {
        "id": "wet-road-friction",
        "title": "Wet-road bus crash",
        "text": "A crash-investigation unit reported that a bus could not stop in time because friction was low on a wet road.",
        "expected": "physics",
    },
    {
        "id": "satellite-orbit",
        "title": "New Earth satellite",
        "text": "NASA engineers calculated the orbital speed of a new Earth satellite using school-lab equipment comparisons with theory.",
        "expected": "physics",
    },
    {
        "id": "football-final",
        "title": "Football final",
        "text": "Colombo FC celebrated a 3-1 football final victory after a late penalty in a packed stadium.",
        "expected": "non_physics",
    },
    {
        "id": "celebrity-wedding",
        "title": "Celebrity wedding",
        "text": "A film studio announced a celebrity wedding date in Mumbai according to a press release.",
        "expected": "non_physics",
    },
]


class NewsIn(BaseModel):
    title: str | None = None
    text: str = Field(..., min_length=1, max_length=4000)
    grade: int | None = Field(default=None, ge=6, le=13)


class AnswerIn(BaseModel):
    question: str = Field(..., min_length=1)
    referenceAnswer: str = Field(..., min_length=1)
    studentAnswer: str = Field(..., min_length=1, max_length=4000)
    scenario: str | None = None


def _combine(title: str | None, text: str) -> str:
    parts = [p.strip() for p in (title or "", text or "") if p and p.strip()]
    return " ".join(parts).strip()


def _rubric(question: str, reference: str, student: str) -> dict:
    q_tok, r_tok, s_tok = _tokens(question), _tokens(reference), _tokens(student)
    relevance = round(100 * jaccard(s_tok, q_tok | r_tok))
    completeness = round(100 * (len(s_tok & r_tok) / max(len(r_tok), 1)))
    copied = jaccard(s_tok, r_tok) > 0.92 and len(student.split()) > 8
    has_example = bool(re.search(r"\b(for example|because|therefore|in this news|such as)\b", student, re.I))
    if copied:
        creativity = 20
        creativity_note = "The answer is very close to the marking scheme, so creativity is low."
    elif has_example:
        creativity = 80
        creativity_note = "The answer adds a reason or example, which meets the creativity criterion."
    else:
        creativity = 50
        creativity_note = "The answer uses own words but no extra example or real-world link."
    return {
        "relevance": relevance,
        "completeness": completeness,
        "creativity": creativity,
        "notes": {
            "relevance": "Share of student words that also appear in the question or reference answer.",
            "completeness": "Share of marking-scheme keywords covered by the student.",
            "creativity": creativity_note,
        },
    }


def _feedback(label: str, rubric: dict, topic: str | None) -> str:
    if label == "correct":
        return (
            f"Strong answer. You covered the key {topic or 'physics'} idea. "
            f"Completeness {rubric['completeness']}%. "
            + rubric["notes"]["creativity"]
        )
    if label == "partial":
        return (
            "Partially correct. Name the concept clearly, include the formula or definition, "
            "and link it back to the news event."
        )
    return (
        "This does not match the marking-scheme physics. Re-read the scenario, "
        "state the concept, and use the relevant formula."
    )


@router.get("/health")
def news_health() -> dict:
    metrics_path = ARTIFACT_DIR / "metrics.json"
    metrics = {}
    if metrics_path.exists():
        metrics = json.loads(metrics_path.read_text(encoding="utf-8"))
    return {
        "modelsReady": models_ready(),
        "model1Accuracy": (metrics.get("model1") or {}).get("accuracy"),
        "model2Accuracy": (metrics.get("model2") or {}).get("accuracy"),
    }


@router.get("/samples")
def news_samples() -> dict:
    return {"samples": SAMPLE_NEWS}


@router.get("/metrics")
def news_metrics() -> dict:
    path = ARTIFACT_DIR / "metrics.json"
    if not path.exists():
        raise HTTPException(503, "Train models first: python -m app.ml_news.train")
    return json.loads(path.read_text(encoding="utf-8"))


@router.post("/scenario")
def create_scenario(body: NewsIn) -> dict:
    started = time.perf_counter()
    news = _combine(body.title, body.text)
    if len(news) < 12:
        raise HTTPException(400, "Please paste a news title or short description.")
    if not models_ready():
        raise HTTPException(503, "Model 1 is not loaded. Train it with: python -m app.ml_news.train")
    relevance = predict_news(news)
    if not relevance["isPhysics"]:
        return {
            "accepted": False,
            "relevance": relevance,
            "message": "Model 1 classified this as Non-Physics, so no question was generated.",
            "elapsedMs": round((time.perf_counter() - started) * 1000),
        }
    generated = generate_scenario_question(news, body.grade)
    return {
        "accepted": True,
        "relevance": relevance,
        "concept": generated["concept"],
        "scenario": generated["scenario"],
        "question": generated["question"],
        "referenceAnswer": generated["referenceAnswer"],
        "usedLlm": generated["usedLlm"],
        "elapsedMs": round((time.perf_counter() - started) * 1000),
    }


@router.post("/evaluate")
def evaluate_answer(body: AnswerIn) -> dict:
    started = time.perf_counter()
    student = body.studentAnswer.strip()
    if not student:
        raise HTTPException(400, "Please type an answer before submitting.")
    if not models_ready():
        raise HTTPException(503, "Model 2 is not loaded. Train it with: python -m app.ml_news.train")
    correctness = predict_answer(body.question, body.referenceAnswer, student)
    rubric = _rubric(body.question, body.referenceAnswer, student)
    return {
        "correctness": correctness,
        "rubric": rubric,
        "feedback": _feedback(correctness["label"], rubric, None),
        "elapsedMs": round((time.perf_counter() - started) * 1000),
    }
