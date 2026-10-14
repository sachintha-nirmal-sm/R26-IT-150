"""
api/admin_search_insights.py — admin-facing read endpoint for the
struggling-search / content-gap detector (app/rag/search_quality.py).
"""

from fastapi import APIRouter, Depends

from app.core.dependencies import VerifiedUser, require_admin
from app.rag.search_quality import list_struggling_terms

router = APIRouter(prefix="/admin/search", tags=["Admin - Search Insights"])


@router.get("/struggling-terms")
def struggling_terms(admin: VerifiedUser = Depends(require_admin)) -> list[dict]:
    return list_struggling_terms()
