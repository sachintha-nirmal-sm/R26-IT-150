"""
api/admin_search_keywords.py — admin controls for the master search
keyword index (search_keyword_index). New uploads auto-populate this index
via app/rag/keyword_aggregate.py's apply_material_keywords(), called from
index_lesson_material_for_search in admin_lessons.py — the endpoints here
are for backfill (sweep), moderation (approve/reject), and CSV export, not
the primary way a keyword becomes live.
"""

import csv
import io

from fastapi import APIRouter, Depends, HTTPException, Query, status
from fastapi.responses import Response
from google.cloud import firestore
from pydantic import BaseModel, Field

from app.core.dependencies import VerifiedUser, require_admin
from app.core.firebase import db
from app.rag.keyword_aggregate import INDEX_COLLECTION, aggregate_keywords

router = APIRouter(prefix="/admin/search/keywords", tags=["Admin - Search Keywords"])


class KeywordStatusUpdate(BaseModel):
    status: str = Field(..., pattern="^(approved|rejected)$")


@router.post("/sweep")
def sweep(admin: VerifiedUser = Depends(require_admin)) -> dict:
    """Full rescan — backfill/self-healing tool, not the primary trigger
    (new uploads already auto-index via apply_material_keywords)."""
    written = aggregate_keywords()
    return {"keywordsIndexed": len(written)}


@router.get("")
def list_keywords(
    status_filter: str | None = Query(None, alias="status"),
    admin: VerifiedUser = Depends(require_admin),
) -> list[dict]:
    query = db.collection(INDEX_COLLECTION)
    if status_filter:
        query = query.where(filter=firestore.FieldFilter("status", "==", status_filter))
    rows = []
    for doc in query.stream():
        data = doc.to_dict() or {}
        data["id"] = doc.id
        rows.append(data)
    rows.sort(key=lambda r: r.get("materialCount", 0), reverse=True)
    return rows


@router.patch("/{keyword}")
def update_status(
    keyword: str,
    body: KeywordStatusUpdate,
    admin: VerifiedUser = Depends(require_admin),
) -> dict:
    ref = db.collection(INDEX_COLLECTION).document(keyword)
    if not ref.get().exists:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Keyword not found.")
    ref.update({
        "status": body.status,
        "reviewedBy": admin.uid,
        "reviewedAt": firestore.SERVER_TIMESTAMP,
    })
    return {"keyword": keyword, "status": body.status}


@router.get("/export.csv")
def export_csv(admin: VerifiedUser = Depends(require_admin)) -> Response:
    buffer = io.StringIO()
    writer = csv.writer(buffer)
    writer.writerow(["keyword", "materialCount", "isPhysicsConcept", "status", "materialTitles"])
    for doc in db.collection(INDEX_COLLECTION).order_by(
        "materialCount", direction=firestore.Query.DESCENDING
    ).stream():
        data = doc.to_dict() or {}
        titles = "; ".join(
            sorted({
                (ref.get("lessonTitle") or "")
                for ref in (data.get("materialRefs") or [])
                if ref.get("lessonTitle")
            })
        )
        writer.writerow([
            doc.id,
            data.get("materialCount", 0),
            data.get("isPhysicsConcept", False),
            data.get("status", ""),
            titles,
        ])
    return Response(
        content=buffer.getvalue(),
        media_type="text/csv",
        headers={"Content-Disposition": "attachment; filename=search_keywords.csv"},
    )
