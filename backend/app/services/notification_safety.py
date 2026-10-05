"""
notification_safety.py — rule-based content check + hand-written fallback
messages, ported from the spike's safety.py. English only for v1.
"""

from __future__ import annotations

import re

_BANNED_SUBSTRINGS = [
    "stupid", "loser", "worst", "failure", "fat", "ugly",
    "hate you", "kill", "die", "suicide", "self harm", "self-harm",
]


def check_script(title: str, body: str) -> tuple[bool, str | None]:
    if len(title) > 40 or len(body) > 90:
        return False, "title/body exceeds length limit"
    combined = f"{title} {body}".lower()
    for word in _BANNED_SUBSTRINGS:
        if re.search(rf"\b{re.escape(word)}\b", combined):
            return False, f"banned phrase: {word}"
    return True, None


_FALLBACKS: dict[str, dict[str, str]] = {
    "struggle": {
        "title": "Let's dive deep! 🔬",
        "body": "You've got a topic worth exploring further — tap to review it.",
    },
    "comeback": {
        "title": "We miss you! 👋",
        "body": "Jump back in — your physics journey is waiting for you.",
    },
    "streak": {
        "title": "Keep the streak alive! 🔥",
        "body": "You're on a roll — don't break it now, practice today.",
    },
    "welcome": {
        "title": "Keep going! ⚡",
        "body": "Every bit of practice adds up — jump into a lesson today.",
    },
}


def fallback_script(mood: str) -> dict:
    return dict(_FALLBACKS.get(mood, _FALLBACKS["welcome"]))
