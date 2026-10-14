"""
seed_search_quality_demo.py — simulates realistic search traffic through
the real app.rag.search_quality functions (not a parallel write path), so
the struggling-search admin screen has something to show before real
student traffic has accumulated.

Covers three distinct cases on purpose:
  - "force"     : healthy term, high click-through -> correctly excluded.
  - "capacitor" : soft content gap, results exist but barely get clicked.
  - "quantum"   : hard content gap, zero results every single time.

Usage:
    cd backend && source venv/bin/activate
    PYTHONPATH=<repo>/backend python3 <repo>/firebase/scripts/seed_search_quality_demo.py
"""

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[2] / "backend"))

from app.rag.search_quality import (  # noqa: E402
    INDEX_COLLECTION,
    list_struggling_terms,
    record_search_click,
    record_search_event,
)
from app.core.firebase import db  # noqa: E402

SCENARIOS = [
    {
        "term": "force",
        "grade": "Grade 10",
        "searches": 10,
        "result_count": 5,
        "clicks": 8,
    },
    {
        "term": "capacitor",
        "grade": "Grade 11",
        "searches": 6,
        "result_count": 3,
        "clicks": 1,
    },
    {
        "term": "quantum",
        "grade": "Grade 11",
        "searches": 4,
        "result_count": 0,
        "clicks": 0,
    },
]


def main() -> None:
    for scenario in SCENARIOS:
        term = scenario["term"]
        grade = scenario["grade"]
        for _ in range(scenario["searches"]):
            record_search_event(term, grade, scenario["result_count"])
        for _ in range(scenario["clicks"]):
            record_search_click(term, grade)

    print(f"Seeded {len(SCENARIOS)} search-quality scenarios into "
          f"'{INDEX_COLLECTION}':\n")
    print(f"{'term':<12}{'searches':<10}{'clicks':<8}{'ctr':<8}{'flagged?':<10}")

    struggling = {row["term"] for row in list_struggling_terms()}
    for scenario in SCENARIOS:
        term = scenario["term"]
        doc = db.collection(INDEX_COLLECTION).document(term).get().to_dict() or {}
        search_count = doc.get("searchCount", 0)
        click_count = doc.get("clickCount", 0)
        ctr = click_count / search_count if search_count else 0.0
        flagged = "yes" if term in struggling else "no"
        print(f"{term:<12}{search_count:<10}{click_count:<8}{ctr:<8.2f}{flagged:<10}")

    print(
        "\nThis is demo-only seed data (simulated traffic, not tied to a "
        "specific student account) — re-run the admin screen's "
        "'/admin/search/struggling-terms' endpoint to see it reflected live."
    )


if __name__ == "__main__":
    main()
