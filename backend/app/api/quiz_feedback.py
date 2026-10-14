"""
api/quiz_feedback.py — student-facing endpoint that feeds the weak-topics
online model (app/rag/topic_performance.py). Fire-and-forget from the
client: a failure here must never surface as an error to a student who
just finished a quiz.
"""

from fastapi import APIRouter, Depends
from pydantic import BaseModel

from app.core.dependencies import VerifiedUser, require_student
from app.rag.topic_performance import record_quiz_attempt

router = APIRouter(prefix="/student", tags=["Quiz Feedback"])


class QuizPerformanceFeedback(BaseModel):
    lessonId: str
    lessonTitle: str | None = None
    grade: str | None = None
    score: float
    correct: int = 0
    total: int = 0


@router.post("/quiz-attempts/performance-feedback")
async def log_quiz_performance(
    body: QuizPerformanceFeedback,
    user: VerifiedUser = Depends(require_student),
) -> dict:
    try:
        record_quiz_attempt(
            body.lessonId, body.lessonTitle, body.grade,
            body.score, body.correct, body.total,
        )
    except Exception as exc:
        print(f"[quiz_feedback] Failed to record quiz performance: {exc}")
    return {"status": "recorded"}
