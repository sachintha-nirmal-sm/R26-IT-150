from fastapi import APIRouter, Depends, HTTPException, status

from app.core.dependencies import VerifiedUser, require_admin
from app.core.firebase import db
from app.core.utils import iso
from app.services.analytics_service import list_admin_stats

router = APIRouter(prefix="/admin/analytics", tags=["Admin - Analytics"])


def _serialize(rows: list[dict]) -> list[dict]:
    for row in rows:
        if "lastUpdated" in row:
            row["lastUpdated"] = iso(row.get("lastUpdated"))
    return rows


@router.get("/lessons")
async def lesson_stats(admin: VerifiedUser = Depends(require_admin)) -> list[dict]:
    rows = _serialize(list_admin_stats("lessonStats"))
    rows.sort(key=lambda x: x.get("averageScorePercent") or 0)
    return rows


@router.get("/quizzes")
async def quiz_stats(admin: VerifiedUser = Depends(require_admin)) -> list[dict]:
    return _serialize(list_admin_stats("quizStats"))


@router.get("/questions")
async def question_stats(admin: VerifiedUser = Depends(require_admin)) -> list[dict]:
    rows = _serialize(list_admin_stats("questionStats"))
    rows.sort(key=lambda x: x.get("incorrectRate") or 0, reverse=True)
    return rows


@router.get("/students/{uid}")
async def student_progress(uid: str, admin: VerifiedUser = Depends(require_admin)) -> dict:
    user = db.collection("users").document(uid).get()
    if not user.exists:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Student not found.")
    profile = user.to_dict() or {}
    if profile.get("role") != "student":
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "User is not a student.")

    def _coll(name: str) -> list[dict]:
        out = []
        for doc in db.collection("users").document(uid).collection(name).stream():
            data = doc.to_dict() or {}
            data["id"] = doc.id
            out.append(data)
        return out

    summary = db.collection("users").document(uid).collection("performanceSummary").document("summary").get()
    return {
        "uid": uid,
        "fullName": profile.get("fullName"),
        "currentGrade": profile.get("currentGrade"),
        "quizProgress": _coll("quizProgress"),
        "quizAttempts": _coll("quizAttempts"),
        "finalQuizAttempts": _coll("finalQuizAttempts"),
        "weakTopics": _coll("weakTopics"),
        "performanceSummary": summary.to_dict() if summary.exists else None,
    }
