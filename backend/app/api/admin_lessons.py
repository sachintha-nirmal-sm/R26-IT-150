"""
admin_lessons.py — Admin CRUD endpoints for Lessons.
  - POST   /admin/lessons
  - PATCH  /admin/lessons/{id}
  - DELETE /admin/lessons/{id}
  - GET    /admin/lessons
"""

import asyncio
import os
from functools import partial
from typing import Literal

import cloudinary
import cloudinary.uploader
from fastapi import APIRouter, Depends, File, HTTPException, UploadFile, status
from google.cloud import firestore
from pydantic import BaseModel, Field

import uuid

from app.core.dependencies import VerifiedUser, require_admin
from app.core.firebase import db

cloudinary.config(
    cloud_name=os.getenv("CLOUDINARY_CLOUD_NAME"),
    api_key=os.getenv("CLOUDINARY_API_KEY"),
    api_secret=os.getenv("CLOUDINARY_API_SECRET"),
    secure=True,
)

router = APIRouter(prefix="/admin/lessons", tags=["Admin - Lessons"])


# ---------------------------------------------------------------------------
# Models
# ---------------------------------------------------------------------------

class LessonCreateRequest(BaseModel):
    title: str = Field(..., min_length=1, max_length=200)
    subject: str = Field(..., min_length=1, max_length=50)
    grade: int = Field(..., ge=9, le=12)
    lessonTag: str = Field(..., min_length=1, max_length=50)
    description: str = Field(..., max_length=1000)
    order: int
    status: Literal["draft", "published"] = "draft"


class LessonUpdateRequest(BaseModel):
    title: str | None = Field(None, min_length=1, max_length=200)
    subject: str | None = Field(None, min_length=1, max_length=50)
    grade: int | None = Field(None, ge=9, le=12)
    lessonTag: str | None = Field(None, min_length=1, max_length=50)
    description: str | None = Field(None, max_length=1000)
    order: int | None = None
    status: Literal["draft", "published"] | None = None


class LessonResponse(BaseModel):
    id: str
    title: str
    subject: str
    grade: int
    lessonTag: str
    description: str
    order: int
    status: str
    createdBy: str
    lastEditedBy: str | None = None
    createdAt: str
    updatedAt: str


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def _to_response(doc_id: str, data: dict) -> LessonResponse:
    return LessonResponse(
        id=doc_id,
        title=data.get("title", ""),
        subject=data.get("subject", ""),
        grade=data.get("grade", 0),
        lessonTag=data.get("lessonTag", ""),
        description=data.get("description", ""),
        order=data.get("order", 0),
        status=data.get("status", "draft"),
        createdBy=data.get("createdBy", ""),
        lastEditedBy=data.get("lastEditedBy"),
        createdAt=data["createdAt"].isoformat() if data.get("createdAt") else "",
        updatedAt=data["updatedAt"].isoformat() if data.get("updatedAt") else "",
    )


# ---------------------------------------------------------------------------
# Endpoints
# ---------------------------------------------------------------------------

@router.post("", response_model=LessonResponse, status_code=status.HTTP_201_CREATED)
async def create_lesson(body: LessonCreateRequest, admin: VerifiedUser = Depends(require_admin)):
    existing = db.collection("lessons").where(
        filter=firestore.FieldFilter("lessonTag", "==", body.lessonTag)
    ).limit(1).stream()
    if any(existing):
        raise HTTPException(status_code=409, detail=f"Lesson tag '{body.lessonTag}' already exists.")

    now = firestore.SERVER_TIMESTAMP
    doc_ref = db.collection("lessons").document()
    doc_ref.set({
        "title": body.title,
        "subject": body.subject,
        "grade": body.grade,
        "lessonTag": body.lessonTag,
        "description": body.description,
        "order": body.order,
        "status": body.status,
        "createdBy": admin.uid,
        "lastEditedBy": admin.uid,
        "createdAt": now,
        "updatedAt": now,
    })
    data = doc_ref.get().to_dict()
    return _to_response(doc_ref.id, data)


@router.get("", response_model=list[LessonResponse])
async def list_lessons(admin: VerifiedUser = Depends(require_admin)):
    docs = db.collection("lessons").order_by("grade").stream()
    results = [_to_response(doc.id, doc.to_dict()) for doc in docs]
    results.sort(key=lambda x: (x.grade, x.order))
    return results


@router.patch("/{lesson_id}", response_model=LessonResponse)
async def update_lesson(lesson_id: str, body: LessonUpdateRequest, admin: VerifiedUser = Depends(require_admin)):
    doc_ref = db.collection("lessons").document(lesson_id)
    if not doc_ref.get().exists:
        raise HTTPException(status_code=404, detail="Lesson not found.")

    update_data = body.model_dump(exclude_unset=True)
    if not update_data:
        raise HTTPException(status_code=400, detail="No fields provided.")

    if "lessonTag" in update_data:
        for t in db.collection("lessons").where(
            filter=firestore.FieldFilter("lessonTag", "==", update_data["lessonTag"])
        ).stream():
            if t.id != lesson_id:
                raise HTTPException(status_code=409, detail="Another lesson with that tag exists.")

    update_data["updatedAt"] = firestore.SERVER_TIMESTAMP
    update_data["lastEditedBy"] = admin.uid
    doc_ref.update(update_data)
    data = doc_ref.get().to_dict()
    return _to_response(doc_ref.id, data)


@router.delete("/{lesson_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_lesson(lesson_id: str, admin: VerifiedUser = Depends(require_admin)):
    doc_ref = db.collection("lessons").document(lesson_id)
    if not doc_ref.get().exists:
        raise HTTPException(status_code=404, detail="Lesson not found.")
    doc_ref.delete()


# ---------------------------------------------------------------------------
# PDF Links — store URL + title in Firestore (no file upload needed)
# ---------------------------------------------------------------------------

class PdfLinkRequest(BaseModel):
    fileName: str = Field(..., min_length=1, max_length=200)
    url: str = Field(..., min_length=1, max_length=2000)


@router.post("/{lesson_id}/pdfs", status_code=status.HTTP_201_CREATED)
async def add_lesson_pdf_link(
    lesson_id: str,
    body: PdfLinkRequest,
    admin: VerifiedUser = Depends(require_admin),
):
    pdf_id = str(uuid.uuid4())
    db.collection("lessons").document(lesson_id).collection("pdfs").document(pdf_id).set({
        "fileName": body.fileName,
        "url": body.url,
        "uploadedBy": admin.uid,
        "uploadedAt": firestore.SERVER_TIMESTAMP,
    })
    return {"id": pdf_id, "fileName": body.fileName, "url": body.url}


@router.get("/{lesson_id}/pdfs")
async def list_lesson_pdfs(lesson_id: str, admin: VerifiedUser = Depends(require_admin)):
    docs = db.collection("lessons").document(lesson_id).collection("pdfs").stream()
    results = []
    for doc in docs:
        d = doc.to_dict()
        results.append({
            "id": doc.id,
            "fileName": d.get("fileName", ""),
            "url": d.get("url", ""),
            "uploadedAt": d["uploadedAt"].isoformat() if d.get("uploadedAt") else "",
        })
    results.sort(key=lambda x: x["uploadedAt"], reverse=True)
    return results


@router.delete("/{lesson_id}/pdfs/{pdf_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_lesson_pdf(
    lesson_id: str,
    pdf_id: str,
    admin: VerifiedUser = Depends(require_admin),
):
    doc_ref = db.collection("lessons").document(lesson_id).collection("pdfs").document(pdf_id)
    if not doc_ref.get().exists:
        raise HTTPException(status_code=404, detail="PDF not found.")
    doc_ref.delete()


@router.post("/{lesson_id}/pdfs/upload", status_code=status.HTTP_201_CREATED)
async def upload_lesson_pdf(
    lesson_id: str,
    file: UploadFile = File(...),
    admin: VerifiedUser = Depends(require_admin),
):
    contents = await file.read()
    loop = asyncio.get_event_loop()
    try:
        result = await loop.run_in_executor(
            None,
            partial(
                cloudinary.uploader.upload,
                contents,
                resource_type="raw",
                folder=f"lessons/{lesson_id}",
                use_filename=True,
                unique_filename=True,
            ),
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Cloudinary upload failed: {e}")

    url = result["secure_url"]
    pdf_id = str(uuid.uuid4())
    db.collection("lessons").document(lesson_id).collection("pdfs").document(pdf_id).set({
        "fileName": file.filename,
        "url": url,
        "uploadedBy": admin.uid,
        "uploadedAt": firestore.SERVER_TIMESTAMP,
    })
    return {"id": pdf_id, "fileName": file.filename, "url": url}
