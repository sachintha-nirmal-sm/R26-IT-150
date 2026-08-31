"""Parse syllabus grade from Firestore, tokens, or lesson metadata."""

from __future__ import annotations

import re

_GRADE_RE = re.compile(r"(\d{1,2})")


def parse_grade(raw) -> int | None:
    """Accept 9, '9', '09', 'Grade 9', or 'Grade 9 Physics'."""
    if raw is None or isinstance(raw, bool):
        return None
    if isinstance(raw, int):
        value = raw
    elif isinstance(raw, float) and raw == int(raw):
        value = int(raw)
    else:
        match = _GRADE_RE.search(str(raw).strip())
        if not match:
            return None
        value = int(match.group(1))
    if 6 <= value <= 13:
        return value
    return None
