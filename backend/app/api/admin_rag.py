from fastapi import APIRouter, Depends, HTTPException, status
from pydantic import BaseModel, Field

from app.core.dependencies import VerifiedUser, require_admin
from app.core.firebase import db
from app.rag.retrieve import retrieve_for_lesson
from app.rag.vector_store import stats as vector_stats

router = APIRouter(prefix="/admin/lessons", tags=["Admin - RAG"])


class RagQueryRequest(BaseModel):
    query: str = Field(..., min_length=2, max_length=500)
    k: int = Field(4, ge=1, le=12)


@router.get("/{lesson_id}/rag/stats")
async def rag_stats(lesson_id: str, admin: VerifiedUser = Depends(require_admin)) -> dict:
    lesson = db.collection("lessons").document(lesson_id).get()
    if not lesson.exists:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Lesson not found.")
    return vector_stats(lesson_id)


@router.post("/{lesson_id}/rag/query")
async def rag_query(
    lesson_id: str,
    body: RagQueryRequest,
    admin: VerifiedUser = Depends(require_admin),
) -> dict:
    lesson = db.collection("lessons").document(lesson_id).get()
    if not lesson.exists:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Lesson not found.")
    hits = retrieve_for_lesson(lesson_id, body.query, k=body.k)
    return {
        "lessonId": lesson_id,
        "query": body.query,
        "hits": [
            {
                "chunkId": h.get("chunkId"),
                "materialId": h.get("materialId"),
                "sourceReference": h.get("sourceReference"),
                "score": h.get("score"),
                "text": (h.get("text") or "")[:500],
            }
            for h in hits
        ],
    }
