"""
lesson_engagement_service.py — tracks whether a student has actually opened
or completed a lesson. This did not exist anywhere before: quiz performance
was already tracked in detail, but lesson *viewing* had no server-side
record at all (the closest thing, subLessonProgress, is client-written and
only covers sub-lesson quiz gating). study_plan_service.py depends on this
to know what a student has already covered.
"""

from __future__ import annotations

from datetime import datetime

from fastapi import HTTPException, status
from google.cloud import firestore

from app.core.firebase import db

VALID_EVENTS = ("opened", "completed")


def _user_ref(uid: str):
    return db.collection("users").document(uid)


def record_event(uid: str, lesson_id: str, event: str) -> dict:
    if event not in VALID_EVENTS:
        raise HTTPException(status.HTTP_400_BAD_REQUEST, f"Invalid event '{event}'.")

    lesson_snap = db.collection("lessons").document(lesson_id).get()
    if not lesson_snap.exists:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Lesson not found.")
    lesson = lesson_snap.to_dict() or {}

    ref = _user_ref(uid).collection("lessonEngagement").document(lesson_id)
    snap = ref.get()
    data = snap.to_dict() if snap.exists else {
        "lessonId": lesson_id,
        "lessonTag": lesson.get("lessonTag"),
        "grade": lesson.get("grade"),
        "firstOpenedAt": firestore.SERVER_TIMESTAMP,
        "openCount": 0,
        "completed": False,
        "completedAt": None,
    }

    if event == "opened":
        data["openCount"] = firestore.Increment(1)
        data["lastOpenedAt"] = firestore.SERVER_TIMESTAMP
    else:
        # Idempotent — a lesson already marked complete stays complete even
        # if a stale "completed" event arrives again later.
        data["completed"] = True
        data["completedAt"] = data.get("completedAt") or firestore.SERVER_TIMESTAMP

    ref.set(data, merge=True)
    return {"lessonId": lesson_id, "event": event, "status": "recorded"}


def get_engagement_map(uid: str) -> dict[str, dict]:
    """lessonId -> engagement doc, for the study-plan generator to avoid
    re-recommending lessons the student has already completed."""
    out: dict[str, dict] = {}
    for doc in _user_ref(uid).collection("lessonEngagement").stream():
        out[doc.id] = doc.to_dict() or {}
    return out


def week_counts(uid: str, start: datetime, end: datetime) -> dict:
    """Lessons opened/completed within [start, end) — feeds a study plan's
    engagementSummary. Firestore has no OR-across-fields query, so this
    scans the (typically small, per-student) engagement collection in
    Python rather than issuing two separate range queries."""
    opened = 0
    completed = 0
    for doc in _user_ref(uid).collection("lessonEngagement").stream():
        data = doc.to_dict() or {}
        last_opened = data.get("lastOpenedAt")
        if last_opened is not None and start <= last_opened < end:
            opened += 1
        completed_at = data.get("completedAt")
        if completed_at is not None and start <= completed_at < end:
            completed += 1
    return {"lessonsOpenedThisWeek": opened, "lessonsCompletedThisWeek": completed}
