"""Admin quiz containers, question-bank generation, and in-place question edits."""

from fastapi import APIRouter, BackgroundTasks, Depends, HTTPException, Query, status
from google.cloud import firestore
from pydantic import BaseModel, Field

from app.core.dependencies import VerifiedUser, require_admin
from app.core.firebase import db
from app.core.utils import iso
from app.services import generation_service
from app.services.quiz_service import get_quiz_or_404, quiz_ref

router = APIRouter(tags=["Admin - Quizzes"])


class QuizCreateRequest(BaseModel):
    title: str = Field(..., min_length=1, max_length=200)
    maxAttempts: int = Field(3, ge=1, le=10)
    questionsPerAttempt: int = Field(20, ge=1, le=100)


class GenerateBankRequest(BaseModel):
    lessonId: str
    llmModelUsed: str | None = None
    questionsPerBank: int | None = Field(None, ge=3, le=50)


class QuestionPatchRequest(BaseModel):
    lessonId: str
    quizId: str
    questionText: str | None = None
    questionType: str | None = None
    options: list[str] | None = None
    correctAnswer: str | None = None
    explanation: str | None = None
    difficulty: str | None = None
    marks: int | None = Field(None, ge=1)


@router.post(
    "/admin/lessons/{lesson_id}/quizzes",
    status_code=status.HTTP_201_CREATED,
    summary="Create a quiz container under a lesson",
)
async def create_quiz(
    lesson_id: str,
    body: QuizCreateRequest,
    admin: VerifiedUser = Depends(require_admin),
) -> dict:
    lesson = db.collection("lessons").document(lesson_id).get()
    if not lesson.exists:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Lesson not found.")
    ref = db.collection("lessons").document(lesson_id).collection("quizzes").document()
    ref.set({
        "title": body.title,
        "quizId": ref.id,
        "lessonId": lesson_id,
        "maxAttempts": body.maxAttempts,
        "questionsPerAttempt": body.questionsPerAttempt,
        "activeQuestionBankVersionId": None,
        "status": "noBankGenerated",
        "createdAt": firestore.SERVER_TIMESTAMP,
        "updatedAt": firestore.SERVER_TIMESTAMP,
        "createdBy": admin.uid,
    })
    return {"id": ref.id, "lessonId": lesson_id, "status": "noBankGenerated", "title": body.title}


@router.post(
    "/admin/quizzes/{quiz_id}/generate-question-bank",
    status_code=status.HTTP_202_ACCEPTED,
    summary="Enqueue RAG question-bank generation",
)
async def generate_bank(
    quiz_id: str,
    body: GenerateBankRequest,
    background_tasks: BackgroundTasks,
    admin: VerifiedUser = Depends(require_admin),
) -> dict:
    job_id = generation_service.enqueue_question_bank_job(
        admin.uid,
        body.lessonId,
        quiz_id,
        body.llmModelUsed,
        body.questionsPerBank,
    )
    background_tasks.add_task(generation_service.run_question_bank_job, job_id)
    return {"jobId": job_id, "status": "queued"}


@router.get("/admin/generation-jobs/{job_id}")
async def get_job(job_id: str, admin: VerifiedUser = Depends(require_admin)) -> dict:
    snap = db.collection("generationJobs").document(job_id).get()
    if not snap.exists:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Job not found.")
    data = snap.to_dict() or {}
    data["id"] = snap.id
    data["startedAt"] = iso(data.get("startedAt"))
    data["completedAt"] = iso(data.get("completedAt"))
    return data


@router.get("/admin/quizzes/{quiz_id}/questions")
async def list_questions(
    quiz_id: str,
    lessonId: str = Query(...),
    admin: VerifiedUser = Depends(require_admin),
) -> list[dict]:
    _, quiz = get_quiz_or_404(lessonId, quiz_id)
    version_id = quiz.get("activeQuestionBankVersionId")
    if not version_id:
        return []
    rows = []
    qref = quiz_ref(lessonId, quiz_id).collection("questionBankVersions").document(version_id).collection("questions")
    for doc in qref.stream():
        data = doc.to_dict() or {}
        data["id"] = doc.id
        data["createdAt"] = iso(data.get("createdAt"))
        rows.append(data)
    return rows


@router.patch("/admin/questions/{question_id}")
async def patch_question(
    question_id: str,
    body: QuestionPatchRequest,
    admin: VerifiedUser = Depends(require_admin),
) -> dict:
    _, quiz = get_quiz_or_404(body.lessonId, body.quizId)
    version_id = quiz.get("activeQuestionBankVersionId")
    if not version_id:
        raise HTTPException(status.HTTP_409_CONFLICT, "No active question bank.")
    ref = (
        quiz_ref(body.lessonId, body.quizId)
        .collection("questionBankVersions")
        .document(version_id)
        .collection("questions")
        .document(question_id)
    )
    if not ref.get().exists:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Question not found in the active bank.")
    updates = body.model_dump(exclude_unset=True, exclude={"lessonId", "quizId"})
    if not updates:
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "No fields to update.")
    ref.update(updates)
    data = ref.get().to_dict() or {}
    data["id"] = question_id
    return data


@router.delete("/admin/questions/{question_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_question(
    question_id: str,
    lessonId: str = Query(...),
    quizId: str = Query(...),
    admin: VerifiedUser = Depends(require_admin),
):
    _, quiz = get_quiz_or_404(lessonId, quizId)
    version_id = quiz.get("activeQuestionBankVersionId")
    if not version_id:
        raise HTTPException(status.HTTP_409_CONFLICT, "No active question bank.")
    ref = (
        quiz_ref(lessonId, quizId)
        .collection("questionBankVersions")
        .document(version_id)
        .collection("questions")
        .document(question_id)
    )
    if not ref.get().exists:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Question not found.")
    ref.delete()
    version_ref = quiz_ref(lessonId, quizId).collection("questionBankVersions").document(version_id)
    version_ref.update({"totalQuestions": firestore.Increment(-1)})
