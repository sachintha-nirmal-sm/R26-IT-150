"""
seed_keyword_aggregation_fixture.py — writes synthetic lesson_materials docs
with hand-authored keyword lists so keyword_aggregate.py's document-frequency
filter has something real to prove itself against before any real PDF has
been uploaded (pdf_keywords/lesson_materials are both empty in this
environment as of this writing).

Mixes real physics terms (should survive the filter) with deliberately
frequent generic words (should be rejected as noise once aggregated across
several "materials" — some materials repeat them, matching what a bag-of-
words extractor realistically produces across many real documents).

Reuses the backend's already-working Firebase Admin SDK client directly
(there's no serviceAccountKey.json under firebase/scripts/, only under
backend/) rather than firebase_init.py's separate credential path.

Usage:
    cd backend && source venv/bin/activate
    PYTHONPATH=<repo>/backend python3 <repo>/firebase/scripts/seed_keyword_aggregation_fixture.py
"""

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[2] / "backend"))

from app.core.firebase import db  # noqa: E402

# Generic words that realistically survive extract_keywords()'s 25-word
# stoplist (length >= 3, not an exact stopword) but are not lesson-specific
# signal — deliberately repeated across most fixture materials below so the
# >60% document-frequency filter has a real reason to reject them.
_NOISE = ["figure", "example", "shown", "these", "which"]

FIXTURES = [
    {
        "id": "fixture-material-1",
        "lessonId": "fixture-lesson-forces",
        "lessonTitle": "Forces and Newton's Laws",
        "materialName": "Forces Chapter Notes",
        "grade": "Grade 10",
        "keywords": [
            "force", "newton", "acceleration", "mass", "inertia",
            "action", "reaction", *_NOISE,
        ],
    },
    {
        "id": "fixture-material-2",
        "lessonId": "fixture-lesson-forces",
        "lessonTitle": "Forces and Newton's Laws",
        "materialName": "Friction Worksheet",
        "grade": "Grade 10",
        "keywords": ["friction", "force", "equilibrium", "surface", *_NOISE],
    },
    {
        "id": "fixture-material-3",
        "lessonId": "fixture-lesson-motion",
        "lessonTitle": "Introduction to Motion",
        "materialName": "Kinematics Notes",
        "grade": "Grade 10",
        "keywords": ["velocity", "displacement", "acceleration", "speed", *_NOISE],
    },
    {
        "id": "fixture-material-4",
        "lessonId": "fixture-lesson-motion",
        "lessonTitle": "Introduction to Motion",
        "materialName": "Motion Graphs Handout",
        "grade": "Grade 10",
        "keywords": ["velocity", "graph", "slope", "displacement", *_NOISE],
    },
    {
        "id": "fixture-material-5",
        "lessonId": "fixture-lesson-energy",
        "lessonTitle": "Work, Energy and Power",
        "materialName": "Energy Chapter Notes",
        "grade": "Grade 10",
        "keywords": ["energy", "work", "power", "momentum", "kinetic", *_NOISE],
    },
    {
        "id": "fixture-material-6",
        "lessonId": "fixture-lesson-density",
        "lessonTitle": "Density",
        "materialName": "Density Lab Guide",
        "grade": "Grade 9",
        "keywords": ["density", "mass", "volume", "buoyancy", *_NOISE],
    },
    {
        "id": "fixture-material-7",
        "lessonId": "fixture-lesson-waves",
        "lessonTitle": "Waves",
        "materialName": "Waves Introduction",
        "grade": "Grade 11",
        "keywords": ["wave", "frequency", "amplitude", "wavelength", *_NOISE],
    },
    {
        "id": "fixture-material-8",
        "lessonId": "fixture-lesson-waves",
        "lessonTitle": "Waves",
        "materialName": "Sound Waves Supplement",
        "grade": "Grade 11",
        # Deliberately no noise words here, to prove the filter works on
        # per-material ratios, not just "always present" words.
        "keywords": ["wave", "sound", "resonance", "pitch"],
    },
]


def seed() -> None:
    for fixture in FIXTURES:
        doc_id = fixture["id"]
        ref = db.collection("lesson_materials").document(doc_id)
        ref.set({
            "lessonId": fixture["lessonId"],
            "lessonTitle": fixture["lessonTitle"],
            "materialName": fixture["materialName"],
            "materialType": "pdf",
            "grade": fixture["grade"],
            "topic": fixture["lessonTitle"],
            "description": f"Seed fixture for {fixture['materialName']}",
            "keywords": fixture["keywords"],
            "keywordCount": len(fixture["keywords"]),
            "keywordIndexStatus": "indexed",
        })
        print(f"seeded {doc_id} ({len(fixture['keywords'])} keywords)")

    print(f"\nDone — {len(FIXTURES)} fixture materials written to lesson_materials.")
    print("Run POST /admin/search/keywords/sweep next to build search_keyword_index.")


if __name__ == "__main__":
    seed()
