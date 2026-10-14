from fastapi import APIRouter, BackgroundTasks, Depends, HTTPException, status
from google.cloud import firestore
from pydantic import BaseModel

from app.core.config import OLLAMA_MODEL
from app.core.dependencies import VerifiedUser, require_admin
from app.core.firebase import db
from app.core.utils import iso
from app.services import generation_service

router = APIRouter(prefix="/admin", tags=["Admin - Final Quiz"])


class GenerateFinalRequest(BaseModel):
    llmModelUsed: str | None = None


@router.post(
    "/grades/{grade}/generate-final-quiz",
    status_code=status.HTTP_202_ACCEPTED,
)
async def generate_final(
    grade: int,
    background_tasks: BackgroundTasks,
    body: GenerateFinalRequest | None = None,
    admin: VerifiedUser = Depends(require_admin),
) -> dict:
    if grade not in (9, 10, 11, 12):
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "Grade must be 9–12.")
    model = (body.llmModelUsed if body else None) or OLLAMA_MODEL
    job_id = generation_service.enqueue_final_quiz_job(admin.uid, grade, model)
    background_tasks.add_task(generation_service.run_final_quiz_job, job_id)
    return {"jobId": job_id, "status": "queued", "grade": grade}


@router.get("/final-quizzes/{grade}/rounds")
async def list_rounds(grade: int, admin: VerifiedUser = Depends(require_admin)) -> list[dict]:
    rows = []
    for doc in (
        db.collection("finalQuizzes")
        .where(filter=firestore.FieldFilter("grade", "==", grade))
        .stream()
    ):
        data = doc.to_dict() or {}
        data["id"] = doc.id
        data["createdAt"] = iso(data.get("createdAt"))
        data["archivedAt"] = iso(data.get("archivedAt"))
        rows.append(data)
    rows.sort(key=lambda x: x.get("roundNumber") or 0, reverse=True)
    return rows
