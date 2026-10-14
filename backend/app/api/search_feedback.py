"""
api/search_feedback.py — student-facing endpoints that feed the
struggling-search online model (app/rag/search_quality.py). Fire-and-forget
from the client: a failure here must never surface as an error to a
student who's just trying to search.
"""

from fastapi import APIRouter, Depends
from pydantic import BaseModel

from app.core.dependencies import VerifiedUser, require_student
from app.rag.search_quality import record_search_click, record_search_event

router = APIRouter(prefix="/search", tags=["Search Feedback"])


class SearchEvent(BaseModel):
    query: str
    grade: str | None = None
    resultCount: int = 0


class SearchClick(BaseModel):
    query: str
    grade: str | None = None


@router.post("/event")
async def log_search_event(
    body: SearchEvent,
    user: VerifiedUser = Depends(require_student),
) -> dict:
    try:
        record_search_event(body.query, body.grade, body.resultCount)
    except Exception as exc:
        print(f"[search_feedback] Failed to record search event: {exc}")
    return {"status": "recorded"}


@router.post("/click")
async def log_search_click(
    body: SearchClick,
    user: VerifiedUser = Depends(require_student),
) -> dict:
    try:
        record_search_click(body.query, body.grade)
    except Exception as exc:
        print(f"[search_feedback] Failed to record search click: {exc}")
    return {"status": "recorded"}
