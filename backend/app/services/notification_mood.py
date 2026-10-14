"""
notification_mood.py — deterministic mood selection, ported from the
physics-mobile-app spike's video_service/app/mood.py. No LLM involved;
same priority order and thresholds as the spike.
"""

from __future__ import annotations

from typing import Literal

Mood = Literal["struggle", "comeback", "streak", "welcome"]

WEAKNESS_MOOD_THRESHOLD = 0.4
STREAK_MIN_DAYS = 3
COMEBACK_MIN_IDLE_DAYS = 2
COMEBACK_MAX_IDLE_DAYS = 7


def select_mood(profile: dict) -> Mood:
    """Priority: struggle > comeback > streak > welcome. "welcome" is the
    catch-all generic motivational mood for any student whose profile
    doesn't (yet) match a more specific one — every student should get a
    personalized send, never a silent skip."""
    weak_topic = profile.get("weakTopic")
    if weak_topic and weak_topic.get("weaknessScore", 0) >= WEAKNESS_MOOD_THRESHOLD:
        return "struggle"

    idle_days = profile.get("daysSinceLastActivity")
    if idle_days is not None and COMEBACK_MIN_IDLE_DAYS <= idle_days <= COMEBACK_MAX_IDLE_DAYS:
        return "comeback"

    streak_days = profile.get("streakDays", 0)
    if streak_days >= STREAK_MIN_DAYS:
        return "streak"

    return "welcome"


def is_eligible_to_send(now_hour: int, most_active_hour: int) -> bool:
    """Safety floor: never send during a hard-coded quiet window (22:00-06:00)
    even if that happens to be the student's computed most-active hour."""
    if 22 <= now_hour or now_hour < 6:
        return False
    return now_hour == most_active_hour
