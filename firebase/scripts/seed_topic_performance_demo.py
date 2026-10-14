"""
seed_topic_performance_demo.py — simulates realistic quiz submissions
through the real app.rag.topic_performance functions (not a parallel
write path), so the Weak Topics admin screen has something real to show
before enough live student quiz data has accumulated.

Uses two real lessons already in the `lessons` collection:
  - "Forces and Newton's Laws" (Grade 10) : healthy, mostly passing scores
    -> correctly excluded from the weak-topics list.
  - "Waves" (Grade 11)                    : weak, mostly below passing
    -> correctly flagged.

Usage:
    cd backend && source venv/bin/activate
    PYTHONPATH=<repo>/backend python3 <repo>/firebase/scripts/seed_topic_performance_demo.py
"""

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[2] / "backend"))

from app.rag.topic_performance import (  # noqa: E402
    INDEX_COLLECTION,
    list_weak_topics,
    record_quiz_attempt,
)
from app.core.firebase import db  # noqa: E402

SCENARIOS = [
    {
        "lesson_id": "phy-g10-forces-doc",
        "lesson_title": "Forces and Newton's Laws",
        "grade": "Grade 10",
        "scores": [85, 90, 78, 92],
    },
    {
        "lesson_id": "phy-g11-waves-doc",
        "lesson_title": "Waves",
        "grade": "Grade 11",
        "scores": [40, 55, 60, 45, 65],
    },
]


def main() -> None:
    for scenario in SCENARIOS:
        for score in scenario["scores"]:
            correct = round(score / 100 * 10)
            record_quiz_attempt(
                scenario["lesson_id"], scenario["lesson_title"], scenario["grade"],
                score, correct, 10,
            )

    print(f"Seeded {len(SCENARIOS)} topic-performance scenarios into "
          f"'{INDEX_COLLECTION}':\n")
    print(f"{'lesson':<28}{'attempts':<10}{'avg score':<12}{'flagged?':<10}")

    weak = {row["lessonId"] for row in list_weak_topics()}
    for scenario in SCENARIOS:
        lesson_id = scenario["lesson_id"]
        doc = db.collection(INDEX_COLLECTION).document(lesson_id).get().to_dict() or {}
        attempt_count = doc.get("attemptCount", 0)
        total_score_sum = doc.get("totalScoreSum", 0)
        avg = total_score_sum / attempt_count if attempt_count else 0.0
        flagged = "yes" if lesson_id in weak else "no"
        print(f"{scenario['lesson_title']:<28}{attempt_count:<10}{avg:<12.1f}{flagged:<10}")

    print(
        "\nThis is demo-only seed data (simulated quiz submissions) — "
        "re-run the admin screen's '/admin/topics/weak-performance' "
        "endpoint to see it reflected live."
    )


if __name__ == "__main__":
    main()
