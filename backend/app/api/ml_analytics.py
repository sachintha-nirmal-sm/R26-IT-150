"""
ml_analytics.py — ML results + difficulty analytics endpoints.
"""
import json
from pathlib import Path

from fastapi import APIRouter, Depends

from app.core.dependencies import VerifiedUser, require_admin
from app.core.firebase import db

router = APIRouter(prefix="/admin", tags=["Admin - ML Analytics"])

_ML_DIR = Path(__file__).resolve().parent.parent.parent.parent / "ml"


@router.get("/ml-report")
async def get_ml_report(admin: VerifiedUser = Depends(require_admin)):
    """Serve the model training comparison report."""
    path = _ML_DIR / "models" / "model_report.json"
    if not path.exists():
        return {"error": "Model not trained yet. Run ml/train_models.py first."}
    return json.loads(path.read_text(encoding="utf-8"))


@router.get("/difficulty-analytics")
async def get_difficulty_analytics(admin: VerifiedUser = Depends(require_admin)):
    """
    Aggregated difficulty stats from Firestore across all lessons:
    - overall Easy/Medium/Hard distribution
    - per-lesson AI difficulty accuracy (AI label vs actual from student data)
    """
    lessons = list(db.collection("lessons").stream())

    overall = {"easy": 0, "medium": 0, "hard": 0, "matched": 0, "withActual": 0}
    lesson_rows = []

    for lesson in lessons:
        ld = lesson.to_dict()
        qdocs = list(
            db.collection("lessons").document(lesson.id)
              .collection("questions").stream()
        )
        if not qdocs:
            continue

        easy = medium = hard = matched = with_actual = 0
        total_attempts = 0

        for doc in qdocs:
            qd = doc.to_dict() or {}
            diff   = qd.get("difficulty", "Medium")
            actual = qd.get("actualDifficulty")
            match  = qd.get("difficultyMatch")
            attempts = qd.get("attempts", 0)

            if diff == "Easy":    easy   += 1
            elif diff == "Hard":  hard   += 1
            else:                 medium += 1

            total_attempts += attempts

            if actual is not None:
                with_actual += 1
                if match:
                    matched += 1

        total = len(qdocs)
        acc = round(matched / with_actual * 100, 1) if with_actual else None

        lesson_rows.append({
            "lessonId":          lesson.id,
            "title":             ld.get("title", ""),
            "grade":             ld.get("grade", ""),
            "total":             total,
            "easy":              easy,
            "medium":            medium,
            "hard":              hard,
            "matched":           matched,
            "withActual":        with_actual,
            "difficultyAccuracy": acc,
            "totalAttempts":     total_attempts,
        })

        overall["easy"]       += easy
        overall["medium"]     += medium
        overall["hard"]       += hard
        overall["matched"]    += matched
        overall["withActual"] += with_actual

    wa = overall["withActual"]
    overall["matchRate"] = round(overall["matched"] / wa * 100, 1) if wa else None

    # Sort lessons by grade then title
    lesson_rows.sort(key=lambda x: (x["grade"], x["title"]))

    return {"overall": overall, "lessons": lesson_rows}
