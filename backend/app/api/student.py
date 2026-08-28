"""Student lessons, quizzes, final quiz, revision, and recommendations."""

from fastapi import APIRouter, Depends, HTTPException, Query, status
from google.cloud import firestore
from pydantic import BaseModel, Field

from app.core.dependencies import VerifiedUser, require_student
from app.core.firebase import db
from app.core.utils import iso
from app.services import analytics_service, quiz_service
from app.services.learning_path_service import generate_learning_path

router = APIRouter(prefix="/student", tags=["Student"])


class AnswerItem(BaseModel):
    questionId: str
    studentAnswer: str | None = None


class SubmitRequest(BaseModel):
    attemptId: str
    answers: list[AnswerItem]
    timeTakenSeconds: int = Field(0, ge=0)


def _run_post_submit(uid: str, result: dict) -> dict:
    return analytics_service.process_submission(uid, result)


@router.get("/lessons", summary="List published lessons for the student's grade")
async def list_lessons(user: VerifiedUser = Depends(require_student)) -> list[dict]:
    profile = quiz_service.load_student_profile(user.uid)
    grade = profile.get("currentGrade")
    docs = (
        db.collection("lessons")
        .where(filter=firestore.FieldFilter("grade", "==", grade))
        .where(filter=firestore.FieldFilter("status", "==", "published"))
        .stream()
    )
    results = []
    for doc in docs:
        data = doc.to_dict() or {}
        results.append({
            "id": doc.id,
            "title": data.get("title"),
            "subject": data.get("subject"),
            "grade": data.get("grade"),
            "lessonTag": data.get("lessonTag"),
            "description": data.get("description"),
            "order": data.get("order", 0),
            "status": data.get("status"),
        })
    results.sort(key=lambda x: x.get("order") or 0)
    return results


@router.get("/lessons/{lesson_id}/quizzes", summary="List attemptable quizzes for a lesson")
async def list_lesson_quizzes(
    lesson_id: str,
    user: VerifiedUser = Depends(require_student),
) -> list[dict]:
    quiz_service.load_student_profile(user.uid)
    lesson = db.collection("lessons").document(lesson_id).get()
    if not lesson.exists:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Lesson not found.")
    out = []
    for doc in db.collection("lessons").document(lesson_id).collection("quizzes").stream():
        data = doc.to_dict() or {}
        progress = (
            db.collection("users").document(user.uid)
            .collection("quizProgress").document(doc.id).get()
        )
        pdata = progress.to_dict() if progress.exists else {}
        out.append({
            "id": doc.id,
            "title": data.get("title"),
            "lessonId": lesson_id,
            "status": data.get("status"),
            "maxAttempts": data.get("maxAttempts", 3),
            "questionsPerAttempt": data.get("questionsPerAttempt"),
            "attemptsUsed": pdata.get("attemptsUsed", 0),
            "isLocked": pdata.get("isLocked", False),
            "bestScore": pdata.get("bestScore", 0),
        })
    return out


@router.post("/quizzes/{quiz_id}/start", summary="Start a lesson quiz (sanitized questions)")
async def start_quiz(
    quiz_id: str,
    lessonId: str = Query(..., description="Parent lesson ID"),
    user: VerifiedUser = Depends(require_student),
) -> dict:
    return quiz_service.start_lesson_quiz(user.uid, lessonId, quiz_id)


@router.post("/quizzes/{quiz_id}/submit", summary="Submit and grade a lesson quiz")
async def submit_quiz(
    quiz_id: str,
    body: SubmitRequest,
    lessonId: str = Query(...),
    user: VerifiedUser = Depends(require_student),
) -> dict:
    result = quiz_service.submit_lesson_quiz(
        user.uid,
        lessonId,
        quiz_id,
        body.attemptId,
        [a.model_dump() for a in body.answers],
        body.timeTakenSeconds,
    )
    extra = _run_post_submit(user.uid, result)
    result.update(extra)
    return result


@router.post("/final-quiz/start", summary="Start the active final quiz for the student's grade")
async def start_final(user: VerifiedUser = Depends(require_student)) -> dict:
    return quiz_service.start_final_quiz(user.uid)


@router.post("/final-quiz/submit", summary="Submit and grade the final quiz")
async def submit_final(
    body: SubmitRequest,
    user: VerifiedUser = Depends(require_student),
) -> dict:
    result = quiz_service.submit_final_quiz(
        user.uid,
        body.attemptId,
        [a.model_dump() for a in body.answers],
        body.timeTakenSeconds,
    )
    extra = _run_post_submit(user.uid, result)
    result.update(extra)
    return result


@router.get("/quizzes/{quiz_id}/wrong-questions")
async def wrong_questions(
    quiz_id: str,
    user: VerifiedUser = Depends(require_student),
) -> list[dict]:
    docs = (
        db.collection("users").document(user.uid)
        .collection("wrongQuestions")
        .where(filter=firestore.FieldFilter("quizId", "==", quiz_id))
        .stream()
    )
    rows = []
    for doc in docs:
        data = doc.to_dict() or {}
        data["id"] = doc.id
        data["createdAt"] = iso(data.get("createdAt"))
        rows.append(data)
    return rows


@router.get("/weak-topics")
async def weak_topics(user: VerifiedUser = Depends(require_student)) -> list[dict]:
    rows = []
    for doc in db.collection("users").document(user.uid).collection("weakTopics").stream():
        data = doc.to_dict() or {}
        data["id"] = doc.id
        data["lastUpdated"] = iso(data.get("lastUpdated"))
        rows.append(data)
    rows.sort(key=lambda x: x.get("weaknessScore") or 0, reverse=True)
    return rows


@router.get("/recommendations")
async def recommendations(user: VerifiedUser = Depends(require_student)) -> list[dict]:
    rows = []
    for doc in db.collection("users").document(user.uid).collection("youtubeRecommendations").stream():
        data = doc.to_dict() or {}
        data["id"] = doc.id
        data["generatedAt"] = iso(data.get("generatedAt"))
        rows.append(data)
    return rows


@router.get("/feedback")
async def list_feedback(user: VerifiedUser = Depends(require_student)) -> list[dict]:
    rows = []
    for doc in db.collection("users").document(user.uid).collection("feedback").stream():
        data = doc.to_dict() or {}
        data["id"] = doc.id
        data["generatedAt"] = iso(data.get("generatedAt"))
        rows.append(data)
    return rows


@router.get("/learning-path")
async def learning_path(user: VerifiedUser = Depends(require_student)) -> dict:
    return generate_learning_path(user.uid)
