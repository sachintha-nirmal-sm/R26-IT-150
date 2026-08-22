"""
simulate_responses.py — Automated student response simulation.

Reads real AI-generated questions from Firestore, simulates N students
answering them using IRT 2PL model, updates attempts/correctCount on each
question doc, and saves a CSV for ML training.

IRT 2PL: P(correct | theta, a, b) = 1 / (1 + exp(-a * (theta - b)))
  theta = student ability drawn from N(0, 1)
  a     = fixed discrimination = 1.2
  b     = item difficulty: Easy=-0.7, Medium=0.0, Hard=0.8
"""

import csv
import io
import math
import os
import random
import time
from pathlib import Path
from typing import Literal

from fastapi import APIRouter, Depends, HTTPException, status
from fastapi.responses import StreamingResponse
from google.cloud import firestore as google_firestore
from pydantic import BaseModel, Field

from app.core.dependencies import VerifiedUser, require_admin
from app.core.firebase import db

router = APIRouter(prefix="/admin/lessons", tags=["Admin - Simulate"])

# ---------------------------------------------------------------------------
# IRT parameters per difficulty label
# ---------------------------------------------------------------------------
_IRT_A = 1.2          # discrimination (fixed)
_IRT_B = {            # difficulty (item difficulty parameter)
    "Easy":   -0.70,
    "Medium":  0.00,
    "Hard":    0.80,
}
# Expected correct rates with average student (theta=0):
#   Easy  : 1/(1+exp( 0.84)) ≈ 70%
#   Medium: 1/(1+exp( 0.00)) = 50%
#   Hard  : 1/(1+exp(-0.96)) ≈ 28%


# ---------------------------------------------------------------------------
# Request model
# ---------------------------------------------------------------------------
class SimulateRequest(BaseModel):
    num_students: int = Field(default=100, ge=10, le=500,
                              description="Number of students to simulate (10-500)")
    seed: int = Field(default=42, description="Random seed for reproducibility")


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def _irt_2pl(theta: float, a: float, b: float) -> float:
    """2-parameter logistic IRT: P(correct) for one student-item pair."""
    return 1.0 / (1.0 + math.exp(-a * (theta - b)))


def _gauss(rng: random.Random) -> float:
    """Box-Muller normal sample (no numpy needed)."""
    return rng.gauss(0.0, 1.0)


def _actual_difficulty(correct_rate: float) -> str:
    if correct_rate > 0.70:
        return "Easy"
    if correct_rate >= 0.40:
        return "Medium"
    return "Hard"


# ---------------------------------------------------------------------------
# Endpoint: simulate responses
# ---------------------------------------------------------------------------

@router.post(
    "/{lesson_id}/simulate-responses",
    status_code=status.HTTP_200_OK,
)
async def simulate_student_responses(
    lesson_id: str,
    body: SimulateRequest,
    admin: VerifiedUser = Depends(require_admin),
):
    """
    Simulate student responses for all questions in a lesson.
    - Updates attempts + correctCount on each question doc.
    - Returns summary stats and per-question breakdown.
    - Does NOT affect quiz settings, PDFs, or session history.
    """
    docs = list(
        db.collection("lessons").document(lesson_id)
          .collection("questions").stream()
    )
    if not docs:
        raise HTTPException(status_code=404, detail="No questions found for this lesson.")

    rng = random.Random(body.seed)

    # Generate student ability scores once (same cohort for all questions)
    abilities = [_gauss(rng) for _ in range(body.num_students)]

    records = []          # for CSV export
    question_summaries = []

    batch = db.batch()

    for doc in docs:
        data = doc.to_dict() or {}
        difficulty = data.get("difficulty", "Medium")
        if difficulty not in _IRT_B:
            difficulty = "Medium"

        b = _IRT_B[difficulty]
        a = _IRT_A

        # Simulate each student answering this question
        total_correct = 0
        for sid, theta in enumerate(abilities):
            p = _irt_2pl(theta, a, b)
            correct = 1 if rng.random() < p else 0
            total_correct += correct

            ability_band = (
                "Low"    if theta < -0.5 else
                "High"   if theta >  0.5 else
                "Medium"
            )
            records.append({
                "lesson_id":       lesson_id,
                "question_id":     doc.id,
                "ai_difficulty":   difficulty,
                "student_id":      f"SIM_S{sid:04d}_seed{body.seed}",
                "student_ability": round(theta, 4),
                "ability_band":    ability_band,
                "correct":         correct,
            })

        correct_rate    = total_correct / body.num_students
        actual_diff     = _actual_difficulty(correct_rate)
        difficulty_match = actual_diff == difficulty

        # Update Firestore — same fields the real student flow updates
        batch.update(doc.reference, {
            "attempts":          google_firestore.Increment(body.num_students),
            "correctCount":      google_firestore.Increment(total_correct),
            "actualDifficulty":  actual_diff,
            "difficultyMatch":   difficulty_match,
        })

        question_summaries.append({
            "questionId":      doc.id,
            "aiDifficulty":    difficulty,
            "actualDifficulty": actual_diff,
            "difficultyMatch": difficulty_match,
            "correctRate":     round(correct_rate, 3),
            "totalStudents":   body.num_students,
        })

    batch.commit()

    # Save CSV to ml/data/
    ml_data_dir = (
        Path(__file__).resolve()
        .parent.parent.parent.parent  # → R26-IT-150/
        / "ml" / "data"
    )
    ml_data_dir.mkdir(parents=True, exist_ok=True)
    csv_path = ml_data_dir / f"responses_{lesson_id}_seed{body.seed}.csv"

    fieldnames = [
        "lesson_id", "question_id", "ai_difficulty",
        "student_id", "student_ability", "ability_band", "correct",
    ]
    with open(csv_path, "w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(records)

    matched = sum(1 for q in question_summaries if q["difficultyMatch"])

    return {
        "questionsSimulated":  len(docs),
        "studentsPerQuestion": body.num_students,
        "totalResponses":      len(records),
        "difficultyAccuracy":  round(matched / len(docs) * 100, 1),
        "csvPath":             str(csv_path),
        "questions":           question_summaries,
        "seed":                body.seed,
    }


# ---------------------------------------------------------------------------
# Endpoint: download CSV dataset
# ---------------------------------------------------------------------------

@router.get("/{lesson_id}/download-dataset")
async def download_dataset(
    lesson_id: str,
    seed: int = 42,
    admin: VerifiedUser = Depends(require_admin),
):
    """Download the simulated response CSV for a lesson."""
    ml_data_dir = (
        Path(__file__).resolve()
        .parent.parent.parent.parent
        / "ml" / "data"
    )
    csv_path = ml_data_dir / f"responses_{lesson_id}_seed{seed}.csv"

    if not csv_path.exists():
        raise HTTPException(
            status_code=404,
            detail="Dataset not found. Run simulate-responses first.",
        )

    content = csv_path.read_bytes()
    return StreamingResponse(
        io.BytesIO(content),
        media_type="text/csv",
        headers={
            "Content-Disposition": f'attachment; filename="responses_{lesson_id}_seed{seed}.csv"'
        },
    )
