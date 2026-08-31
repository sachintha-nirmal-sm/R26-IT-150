"""Grade-adaptive RAG chatbot API (Part 01)."""

from fastapi import APIRouter, Depends, HTTPException, status
from pydantic import BaseModel, Field

from app.core.auth_users import profile_grade
from app.core.dependencies import VerifiedUser, require_auth
from app.core.firebase import db
from app.core.grade import parse_grade
from app.services.chat_service import answer_chat

router = APIRouter(prefix="/chat", tags=["RAG Chatbot"])


class ChatRagRequest(BaseModel):
    message: str = Field(..., min_length=1, max_length=2000)
    lesson_id: str | None = None
    topic: str | None = None
    session_id: str | None = None


def _profile_grade(uid: str, fallback: int | None = None) -> int | None:
    """Grade from the logged-in user's Firestore profile (not a client picker)."""
    if fallback is not None:
        return fallback
    snap = db.collection("users").document(uid).get()
    if not snap.exists:
        return None
    data = snap.to_dict() or {}
    return parse_grade(data.get("currentGrade")) or parse_grade(data.get("grade")) or profile_grade(data)


@router.post("/rag")
async def chat_rag(
    body: ChatRagRequest,
    user: VerifiedUser = Depends(require_auth),
) -> dict:
    """Uses the logged-in student's grade from Firestore. Client cannot pick another grade."""
    if user.role != "student":
        raise HTTPException(
            status.HTTP_403_FORBIDDEN,
            "Student login is required for the grade-adaptive chatbot.",
        )
    grade = _profile_grade(user.uid, user.current_grade)
    if grade is None:
        raise HTTPException(
            status.HTTP_400_BAD_REQUEST,
            "Your profile has no grade. Sign up again or ask an admin to set currentGrade.",
        )
    return answer_chat(
        student_id=user.uid,
        message=body.message,
        grade=grade,
        lesson_id=body.lesson_id,
        topic=body.topic,
        session_id=body.session_id,
    )
