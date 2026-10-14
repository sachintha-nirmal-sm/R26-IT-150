"""
topic_performance.py — online learning for the "weak topics by quiz
performance" admin feature. See the "Online Learning: Weak Topics by Quiz
Performance" project plan.

Every quiz a student submits is one atomic Firestore increment onto
topic_performance_index/{lessonId} — running counters (attemptCount,
totalScoreSum, lowScoreCount) that are the model's parameters. The
increment itself IS the training step: no batch job, no retraining run,
no external dataset.

Mirrors app/rag/search_quality.py's architecture exactly (same online
counter pattern, same CSV mirror for presentation) so both of Chamath's
ML features share one consistent story.
"""

from __future__ import annotations

import csv
from datetime import datetime, timezone
from pathlib import Path

from google.cloud import firestore

from app.core.firebase import db

INDEX_COLLECTION = "topic_performance_index"

MIN_SAMPLES = 3
PASSING_SCORE = 70  # matches admin_analytics_screen.dart's existing pass/fail threshold

DATA_DIR = Path(__file__).resolve().parents[2] / "data"
EVENTS_LOG_CSV = DATA_DIR / "quiz_performance_events_log.csv"
MODEL_SNAPSHOT_CSV = DATA_DIR / "topic_performance_model.csv"


def _append_event_log(
    lesson_id: str,
    lesson_title: str | None,
    grade: str | None,
    score: float,
    correct: int,
    total: int,
) -> None:
    """Appends one row to the raw dataset CSV — one row per real quiz
    submission, in the order it happened."""
    DATA_DIR.mkdir(parents=True, exist_ok=True)
    is_new = not EVENTS_LOG_CSV.exists()
    with open(EVENTS_LOG_CSV, "a", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)
        if is_new:
            writer.writerow(["timestamp", "lesson_id", "lesson_title", "grade", "score", "correct", "total"])
        writer.writerow([
            datetime.now(timezone.utc).isoformat(),
            lesson_id,
            lesson_title or "",
            grade or "",
            score,
            correct,
            total,
        ])


def _write_model_snapshot() -> None:
    """Rewrites the full model-state CSV from Firestore's current counters
    — the model's trained parameters, as they stand the instant this is
    called."""
    DATA_DIR.mkdir(parents=True, exist_ok=True)
    rows = []
    for doc in db.collection(INDEX_COLLECTION).stream():
        data = doc.to_dict() or {}
        attempt_count = data.get("attemptCount", 0)
        total_score_sum = data.get("totalScoreSum", 0)
        low_score_count = data.get("lowScoreCount", 0)
        average_score = (total_score_sum / attempt_count) if attempt_count else 0.0
        low_score_rate = (low_score_count / attempt_count) if attempt_count else 0.0
        flagged = attempt_count >= MIN_SAMPLES and average_score < PASSING_SCORE
        rows.append({
            "lesson_id": doc.id,
            "lesson_title": data.get("lessonTitle", doc.id),
            "attempt_count": attempt_count,
            "average_score": round(average_score, 2),
            "low_score_count": low_score_count,
            "low_score_rate": round(low_score_rate, 4),
            "grades": ",".join(sorted(data.get("grades", []))),
            "flagged_as_weak": flagged,
        })
    rows.sort(key=lambda r: (-r["attempt_count"], r["average_score"]))

    with open(MODEL_SNAPSHOT_CSV, "w", newline="", encoding="utf-8") as f:
        fieldnames = [
            "lesson_id", "lesson_title", "attempt_count", "average_score",
            "low_score_count", "low_score_rate", "grades", "flagged_as_weak",
        ]
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)


def record_quiz_attempt(
    lesson_id: str,
    lesson_title: str | None,
    grade: str | None,
    score: float,
    correct: int,
    total: int,
) -> None:
    """Training step for a quiz submission — increments this topic's
    attempt count, score sum, and (when below PASSING_SCORE) low-score
    count, the instant the quiz is submitted."""
    if not lesson_id:
        return

    ref = db.collection(INDEX_COLLECTION).document(lesson_id)
    update = {
        "lessonTitle": lesson_title or lesson_id,
        "attemptCount": firestore.Increment(1),
        "totalScoreSum": firestore.Increment(score),
        "lastAttemptAt": firestore.SERVER_TIMESTAMP,
    }
    if grade:
        update["grades"] = firestore.ArrayUnion([grade])
    if score < PASSING_SCORE:
        update["lowScoreCount"] = firestore.Increment(1)
    ref.set(update, merge=True)

    _append_event_log(lesson_id, lesson_title, grade, score, correct, total)
    _write_model_snapshot()


def list_weak_topics(
    min_samples: int = MIN_SAMPLES,
    passing_score: float = PASSING_SCORE,
) -> list[dict]:
    """Inference step: derives average score and low-score rate from the
    stored counters, and flags topics with enough attempts and a low
    enough average score."""
    rows = []
    for doc in db.collection(INDEX_COLLECTION).stream():
        data = doc.to_dict() or {}
        attempt_count = data.get("attemptCount", 0)
        if attempt_count < min_samples:
            continue

        total_score_sum = data.get("totalScoreSum", 0)
        low_score_count = data.get("lowScoreCount", 0)
        average_score = total_score_sum / attempt_count
        if average_score >= passing_score:
            continue

        rows.append({
            "lessonId": doc.id,
            "lessonTitle": data.get("lessonTitle", doc.id),
            "attemptCount": attempt_count,
            "averageScore": round(average_score, 2),
            "lowScoreCount": low_score_count,
            "lowScoreRate": round(low_score_count / attempt_count, 4),
            "grades": sorted(data.get("grades", [])),
        })

    rows.sort(key=lambda r: (-r["attemptCount"], r["averageScore"]))
    return rows
