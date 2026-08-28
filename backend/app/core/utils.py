from datetime import datetime, timezone
from typing import Any


QUESTION_TYPES = ("Theory", "Formula", "Calculation")
WEAKNESS_THRESHOLD = 0.4


def iso(value: Any) -> str | None:
    """Convert a Firestore timestamp / datetime to ISO-8601, or None."""
    if value is None:
        return None
    if hasattr(value, "isoformat"):
        return value.isoformat()
    return str(value)


def utcnow() -> datetime:
    return datetime.now(timezone.utc)


def normalize_answer(text: str | None) -> str:
    if text is None:
        return ""
    return " ".join(str(text).strip().lower().split())


def empty_type_bucket() -> dict:
    return {
        qtype: {"incorrectCount": 0, "totalAttempted": 0, "weaknessScore": 0.0}
        for qtype in QUESTION_TYPES
    }


def weakness_score(incorrect: int, total: int) -> float:
    if total <= 0:
        return 0.0
    return round(incorrect / total, 4)
