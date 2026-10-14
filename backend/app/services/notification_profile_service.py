"""
notification_profile_service.py — builds the per-student profile dict that
notification_mood.select_mood() and notification_script.generate_script()
consume. Replaces the spike's local-fixture _load_profile_fixture with a
real Firestore-backed equivalent.
"""

from __future__ import annotations

from datetime import date, datetime, timedelta, timezone

from google.cloud.firestore import FieldFilter

from app.core.firebase import db


def _to_date(value) -> date | None:
    if value is None:
        return None
    if hasattr(value, "date"):
        return value.date()
    return None


def _compute_streak_days(uid: str) -> int:
    """Consecutive days (ending today or yesterday) with at least one quiz
    or practical attempt. Computed on-demand from existing timestamps rather
    than maintained as a counter."""
    activity_dates: set[date] = set()

    for snap in db.collection("users").document(uid).collection("quizProgress").stream():
        d = _to_date((snap.to_dict() or {}).get("lastAttemptAt"))
        if d:
            activity_dates.add(d)

    for snap in db.collection("studentPracticals").where(
        filter=FieldFilter("studentId", "==", uid)
    ).stream():
        d = _to_date((snap.to_dict() or {}).get("lastAttemptAt"))
        if d:
            activity_dates.add(d)

    if not activity_dates:
        return 0

    today = datetime.now(timezone.utc).date()
    anchor = today if today in activity_dates else today - timedelta(days=1)
    if anchor not in activity_dates:
        return 0

    streak = 0
    cursor = anchor
    while cursor in activity_dates:
        streak += 1
        cursor -= timedelta(days=1)
    return streak


def _most_active_hour(uid: str) -> int:
    snap = db.collection("users").document(uid).get()
    counts = (snap.to_dict() or {}).get("activityHourCounts") or {}
    if not counts:
        return 19
    best_hour = max(counts.items(), key=lambda kv: kv[1])[0]
    try:
        return int(best_hour)
    except (TypeError, ValueError):
        return 19


def _top_weak_topic(uid: str) -> dict | None:
    docs = list(
        db.collection("users")
        .document(uid)
        .collection("weakTopics")
        .order_by("weaknessScore", direction="DESCENDING")
        .limit(1)
        .stream()
    )
    if not docs:
        return None
    data = docs[0].to_dict() or {}
    return {
        "lessonTag": data.get("lessonTag", ""),
        "weaknessScore": float(data.get("weaknessScore", 0)),
    }


def _days_since_last_activity(uid: str) -> int | None:
    latest: date | None = None
    for snap in db.collection("users").document(uid).collection("quizProgress").stream():
        d = _to_date((snap.to_dict() or {}).get("lastAttemptAt"))
        if d and (latest is None or d > latest):
            latest = d
    if latest is None:
        return None
    return (datetime.now(timezone.utc).date() - latest).days


def build_profile(uid: str) -> dict:
    """Assemble the real-data equivalent of the spike's StudentProfile."""
    user_snap = db.collection("users").document(uid).get()
    user_data = user_snap.to_dict() or {}

    weak_topic = _top_weak_topic(uid)
    streak_days = _compute_streak_days(uid)
    days_idle = _days_since_last_activity(uid)

    return {
        "uid": uid,
        "fullName": user_data.get("fullName") or "there",
        "currentGrade": user_data.get("currentGrade", 10),
        "streakDays": streak_days,
        "daysSinceLastActivity": days_idle,
        "mostActiveHour": _most_active_hour(uid),
        "weakTopic": weak_topic,
        "accountCreatedAt": user_data.get("createdAt"),
    }
