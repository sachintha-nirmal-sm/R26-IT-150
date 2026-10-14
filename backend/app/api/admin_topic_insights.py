"""
api/admin_topic_insights.py — admin-facing read endpoint for the
weak-topics-by-quiz-performance detector (app/rag/topic_performance.py).
"""

from fastapi import APIRouter, Depends

from app.core.dependencies import VerifiedUser, require_admin
from app.rag.topic_performance import list_weak_topics

router = APIRouter(prefix="/admin/topics", tags=["Admin - Topic Insights"])


@router.get("/weak-performance")
def weak_performance(admin: VerifiedUser = Depends(require_admin)) -> list[dict]:
    return list_weak_topics()
