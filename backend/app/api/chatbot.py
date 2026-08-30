"""Grade-adaptive RAG chatbot API (Part 01)."""

from fastapi import APIRouter, Depends, HTTPException, status
from fastapi.security import HTTPAuthorizationCredentials
from pydantic import BaseModel, Field

from app.core.dependencies import VerifiedUser, require_auth, security_scheme
from app.core.firebase import db
from app.services.chat_service import answer_chat

router = APIRouter(prefix="/chat", tags=["RAG Chatbot"])


class ChatRagRequest(BaseModel):
    message: str = Field(..., min_length=1, max_length=2000)
    user_id: str | None = None
    grade: int | None = Field(None, ge=6, le=12)
    lesson_id: str | None = None
    topic: str | None = None
    session_id: str | None = None


async def optional_student(
    credentials: HTTPAuthorizationCredentials | None = Depends(security_scheme),
) -> VerifiedUser | None:
    if not credentials or not credentials.credentials:
        return None
    return await require_auth(credentials)


@router.post("/rag")
async def chat_rag(
    body: ChatRagRequest,
    user: VerifiedUser | None = Depends(optional_student),
) -> dict:
    student_id = user.uid if user else (body.user_id or "anonymous")
    grade = body.grade
    if user and user.role == "student":
        profile = db.collection("users").document(user.uid).get()
        if profile.exists:
            grade = (profile.to_dict() or {}).get("currentGrade") or grade
    if grade is None:
        raise HTTPException(
            status.HTTP_400_BAD_REQUEST,
            "grade is required when the student profile has no currentGrade.",
        )
    return answer_chat(
        student_id=student_id,
        message=body.message,
        grade=int(grade),
        lesson_id=body.lesson_id,
        topic=body.topic,
        session_id=body.session_id,
    )
