"""
run_simulation.py
=================
Reads all real physics questions from Firestore, simulates realistic student
responses using IRT 2-PL model, updates Firestore, and saves CSV datasets.

Key design choices for research validity:
  - b-parameters drawn from OVERLAPPING Normal distributions, so ~25% of
    questions land near a boundary (e.g., an AI-labeled "Easy" question may
    have a true IRT difficulty of "Medium").
  - True difficulty is defined by the b-parameter, NOT the observed correct
    rate -- this makes the ML task genuinely non-trivial.
  - 5 student populations x 300 students = 1,500 responses per question.

Five student populations simulated:
  seed=42  average      N(0.0,  1.0)  <- updates Firestore
  seed=7   weaker       N(-0.3, 1.1)  <- CSV only
  seed=99  stronger     N(+0.3, 0.9)  <- CSV only
  seed=13  high-variance N(0.0, 1.3)  <- CSV only
  seed=77  low-variance  N(0.0, 0.8)  <- CSV only

Usage:
  python run_simulation.py              # simulate + update Firestore
  python run_simulation.py --dry-run   # simulate only, no Firestore writes
"""

import argparse
import csv
import json
import math
import random
import sys
from pathlib import Path

# ── Firebase setup ────────────────────────────────────────────────────────────
ROOT = Path(__file__).resolve().parent.parent
KEY  = ROOT / "firebase" / "scripts" / "serviceAccountKey.json"

import firebase_admin
from firebase_admin import credentials, firestore as fs

if not firebase_admin._apps:
    firebase_admin.initialize_app(credentials.Certificate(str(KEY)))

db = fs.client()

# ── IRT 2-PL parameters ───────────────────────────────────────────────────────
_A = 1.2   # discrimination (fixed, typical value in physics assessments)

# b-parameter as Normal distribution per AI-label.
# Using OVERLAPPING distributions so ~25% of questions are near a boundary.
# This means the ML task is genuinely hard -- response patterns alone won't
# perfectly reveal the true difficulty.
_B_MU_SIGMA = {
    "Easy":   (-0.70, 0.45),   # ~26% will have b > -0.30  (true: Medium)
    "Medium": ( 0.00, 0.40),   # ~23% will have |b| > 0.30 (true: Easy or Hard)
    "Hard":   ( 0.80, 0.45),   # ~24% will have b <  0.30  (true: Medium)
}

# True difficulty boundaries based on b-parameter (NOT correct_rate)
_B_EASY_THRESHOLD  = -0.30
_B_HARD_THRESHOLD  =  0.30

# ── Student population configs ────────────────────────────────────────────────
_RUNS = [
    {"seed": 42,  "n": 300, "mu":  0.0, "sigma": 1.0, "label": "average",       "update_firestore": True},
    {"seed": 7,   "n": 300, "mu": -0.3, "sigma": 1.1, "label": "weaker",        "update_firestore": False},
    {"seed": 99,  "n": 300, "mu":  0.3, "sigma": 0.9, "label": "stronger",      "update_firestore": False},
    {"seed": 13,  "n": 300, "mu":  0.0, "sigma": 1.3, "label": "high_variance", "update_firestore": False},
    {"seed": 77,  "n": 300, "mu":  0.0, "sigma": 0.8, "label": "low_variance",  "update_firestore": False},
]

# ── Helpers ───────────────────────────────────────────────────────────────────

def _irt(theta: float, a: float, b: float) -> float:
    """P(correct) per IRT 2-PL model."""
    return 1.0 / (1.0 + math.exp(-a * (theta - b)))


def _b_to_true_difficulty(b: float) -> str:
    """Convert IRT b-parameter to true difficulty label."""
    if b < _B_EASY_THRESHOLD:
        return "Easy"
    if b <= _B_HARD_THRESHOLD:
        return "Medium"
    return "Hard"


def _rate_to_difficulty(rate: float) -> str:
    """Threshold-based difficulty from observed correct rate."""
    if rate > 0.70:
        return "Easy"
    if rate >= 0.40:
        return "Medium"
    return "Hard"


def _ability_band(theta: float) -> str:
    if theta < -0.5:
        return "Low"
    if theta > 0.5:
        return "High"
    return "Medium"


# ── Main ──────────────────────────────────────────────────────────────────────

def main(dry_run: bool = False) -> None:
    # 1. Fetch all lessons that have questions
    active = []
    for lesson in db.collection("lessons").stream():
        qdocs = list(
            db.collection("lessons").document(lesson.id)
              .collection("questions").stream()
        )
        if qdocs:
            active.append((lesson, qdocs))

    total_q = sum(len(q) for _, q in active)

    print(f"\nFound {len(active)} lessons with questions:")
    for lesson, qdocs in active:
        ld = lesson.to_dict()
        print(f"  Grade {ld.get('grade','?')} | {ld.get('title','?')} | {len(qdocs)}q")

    total_students = sum(r["n"] for r in _RUNS)
    print(f"\nTotal questions   : {total_q}")
    print(f"Students per run  : {_RUNS[0]['n']}")
    print(f"Runs (seeds)      : {[r['seed'] for r in _RUNS]}")
    print(f"Total responses   : {total_q * total_students:,}\n")

    # 2. Assign a FIXED b-value per question using Normal distribution.
    #    Same b across all runs so different student populations all face the
    #    same question difficulty.
    b_rng = random.Random(0)   # deterministic — same b every time script runs
    question_meta: dict[str, dict] = {}

    for _, qdocs in active:
        for doc in qdocs:
            qd = doc.to_dict() or {}
            ai_diff = qd.get("difficulty", "Medium")
            if ai_diff not in _B_MU_SIGMA:
                ai_diff = "Medium"
            mu, sigma = _B_MU_SIGMA[ai_diff]
            b = b_rng.gauss(mu, sigma)
            question_meta[doc.id] = {
                "ai_difficulty":    ai_diff,
                "irt_b_param":      round(b, 4),
                "irt_true_difficulty": _b_to_true_difficulty(b),
            }

    # Report how many questions are borderline (AI label != true IRT label)
    mismatches = sum(
        1 for m in question_meta.values()
        if m["ai_difficulty"] != m["irt_true_difficulty"]
    )
    print(f"Questions where AI label != IRT true label: {mismatches}/{total_q} "
          f"({mismatches/total_q:.0%}) -- these create realistic ML errors\n")

    # 3. Simulate responses
    all_records: list[dict] = []

    for run in _RUNS:
        rng = random.Random(run["seed"])
        abilities = [rng.gauss(run["mu"], run["sigma"]) for _ in range(run["n"])]

        # Per-question tallies for this run (for Firestore update)
        run_tallies: dict[str, tuple[int, int]] = {}

        for lesson, qdocs in active:
            ld = lesson.to_dict()
            for doc in qdocs:
                meta     = question_meta[doc.id]
                ai_diff  = meta["ai_difficulty"]
                b        = meta["irt_b_param"]
                true_diff = meta["irt_true_difficulty"]
                n_correct = 0

                for sid, theta in enumerate(abilities):
                    p = _irt(theta, _A, b)
                    c = 1 if rng.random() < p else 0
                    n_correct += c

                    all_records.append({
                        "lesson_id":          lesson.id,
                        "lesson_title":        ld.get("title", ""),
                        "grade":              ld.get("grade", ""),
                        "question_id":         doc.id,
                        "ai_difficulty":       ai_diff,
                        "irt_b_param":         b,
                        "irt_true_difficulty": true_diff,
                        "student_id":          f"SIM_{run['label']}_S{sid:04d}",
                        "student_ability":     round(theta, 4),
                        "ability_band":        _ability_band(theta),
                        "correct":             c,
                        "run_seed":            run["seed"],
                        "population":          run["label"],
                    })

                run_tallies[doc.id] = (n_correct, run["n"])

        print(f"  Seed {run['seed']:>3} ({run['label']:<14}): {run['n']} students")

        # 4. Update Firestore from the "average population" run only
        if run["update_firestore"] and not dry_run:
            print("              Writing to Firestore...", end=" ", flush=True)
            batch = db.batch()
            written = 0
            for lesson, qdocs in active:
                for doc in qdocs:
                    n_correct, n_total = run_tallies[doc.id]
                    rate  = n_correct / n_total
                    meta  = question_meta[doc.id]

                    # Set values directly (overwrite previous simulation)
                    batch.update(doc.reference, {
                        "attempts":            n_total,
                        "correctCount":        n_correct,
                        "actualDifficulty":    _rate_to_difficulty(rate),
                        "difficultyMatch":     meta["ai_difficulty"] == _rate_to_difficulty(rate),
                        "irtBParam":           meta["irt_b_param"],
                        "irtTrueDifficulty":   meta["irt_true_difficulty"],
                    })
                    written += 1
                    if written % 450 == 0:
                        batch.commit()
                        batch = db.batch()
            batch.commit()
            print(f"done ({written} questions updated)")

    # 5. Save CSV datasets
    data_dir = Path(__file__).parent / "data"
    data_dir.mkdir(exist_ok=True)

    FIELDS = [
        "lesson_id", "lesson_title", "grade", "question_id",
        "ai_difficulty", "irt_b_param", "irt_true_difficulty",
        "student_id", "student_ability", "ability_band",
        "correct", "run_seed", "population",
    ]

    # Full combined dataset
    with open(data_dir / "student_responses_full.csv", "w", newline="", encoding="utf-8") as f:
        w = csv.DictWriter(f, fieldnames=FIELDS)
        w.writeheader()
        w.writerows(all_records)

    # Per-seed datasets
    for run in _RUNS:
        seed_records = [r for r in all_records if r["run_seed"] == run["seed"]]
        with open(data_dir / f"student_responses_seed{run['seed']}.csv",
                  "w", newline="", encoding="utf-8") as f:
            w = csv.DictWriter(f, fieldnames=FIELDS)
            w.writeheader()
            w.writerows(seed_records)

    # Summary
    summary = {
        "total_responses":  len(all_records),
        "lessons":          len(active),
        "questions":        total_q,
        "runs":             [r["seed"] for r in _RUNS],
        "students_per_run": _RUNS[0]["n"],
        "ai_label_mismatches": mismatches,
        "firestore_updated": not dry_run,
        "files": [
            "student_responses_full.csv",
            *[f"student_responses_seed{r['seed']}.csv" for r in _RUNS],
        ],
    }
    with open(data_dir / "simulation_summary.json", "w") as f:
        json.dump(summary, f, indent=2)

    print(f"\n{'-'*55}")
    print(f"Total responses   : {len(all_records):,}")
    print(f"CSVs saved to     : {data_dir}")
    print(f"Firestore updated : {'NO (dry-run)' if dry_run else 'YES (seed=42 run)'}")
    print(f"{'-'*55}")
    print("Next step: python train_models.py")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--dry-run", action="store_true",
                        help="Simulate without writing to Firestore")
    args = parser.parse_args()
    main(dry_run=args.dry_run)
