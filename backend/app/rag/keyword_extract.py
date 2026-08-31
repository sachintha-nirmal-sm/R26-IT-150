"""Deterministic keyword extraction for lesson-material search indexes."""

from __future__ import annotations

import re
from collections import Counter


STOP_WORDS = {
    "a", "an", "and", "are", "as", "at", "be", "been", "between", "by",
    "for", "from", "has", "have", "in", "into", "is", "it", "its", "of",
    "on", "or", "that", "the", "their", "this", "to", "was", "were", "will",
    "with",
}

PHYSICS_CONCEPTS = {
    "acceleration", "ampere", "amplitude", "buoyancy", "charge", "circuit",
    "current", "density", "displacement", "electricity", "electromagnetism",
    "energy", "equilibrium", "force", "frequency", "friction", "gravity",
    "heat", "inertia", "kinetic", "lens", "light", "mass", "momentum",
    "motion", "optics", "power", "pressure", "resistance", "speed",
    "temperature", "thermal", "velocity", "voltage", "wave", "waves",
    "weight", "work",
}


def extract_keywords(text: str, limit: int = 80) -> list[str]:
    """Return frequent searchable terms, with physics concepts preferred."""
    words = re.findall(r"[a-z0-9]+(?:'[a-z0-9]+)?", (text or "").lower())
    counts = Counter(
        word for word in words if len(word) >= 3 and word not in STOP_WORDS
    )
    ranked = sorted(
        counts,
        key=lambda word: (-counts[word], -int(word in PHYSICS_CONCEPTS), word),
    )
    return ranked[: max(0, limit)]
