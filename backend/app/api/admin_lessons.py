"""
admin_lessons.py — Admin CRUD endpoints for Lessons.
  - POST   /admin/lessons
  - PATCH  /admin/lessons/{id}
  - DELETE /admin/lessons/{id}
  - GET    /admin/lessons
"""

import asyncio
import os
import uuid
from functools import partial
from typing import Literal

import cloudinary
import cloudinary.uploader
from fastapi import (
    APIRouter,
    BackgroundTasks,
    Depends,
    File,
    Form,
    HTTPException,
    UploadFile,
    status,
)
from google.cloud import firestore
from pydantic import BaseModel, Field

from app.core.config import LOCAL_UPLOAD_DIR
from app.core.dependencies import VerifiedUser, require_admin
from app.core.firebase import db
from app.rag.ingest import ingest_material
from app.rag.keyword_extract import extract_keywords
from app.rag.text_extract import extract_text
from app.rag.chunking import chunk_text
from app.rag.embeddings import embed_texts
from app.rag.vector_store import replace_material


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


class PdfLinkRequest(BaseModel):
    fileName: str = Field(..., min_length=1, max_length=200)
    url: str = Field(..., min_length=1, max_length=2000)


class MaterialResponse(BaseModel):
    id: str
    fileName: str
    materialType: str
    storagePath: str
    fileSizeBytes: int
    ingestionStatus: str
    chunkCount: int
    createdAt: str


class SearchIndexResponse(BaseModel):
    materialId: str
    keywordCount: int
    status: str


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
# Lesson CRUD Endpoints
# ---------------------------------------------------------------------------

@router.post("", response_model=LessonResponse, status_code=status.HTTP_201_CREATED)
async def create_lesson(
    body: LessonCreateRequest,
    admin: VerifiedUser = Depends(require_admin),
):
    existing = (
        db.collection("lessons")
        .where(filter=firestore.FieldFilter("lessonTag", "==", body.lessonTag))
        .limit(1)
        .stream()
    )
    if any(existing):
        raise HTTPException(
            status_code=status.HTTP_409_CONFLICT,
            detail=f"Lesson tag '{body.lessonTag}' already exists.",
        )

    now = firestore.SERVER_TIMESTAMP
    doc_ref = db.collection("lessons").document()
    doc_ref.set(
        {
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
        }
    )

    data = doc_ref.get().to_dict() or {}
    return _to_response(doc_ref.id, data)


@router.get("", response_model=list[LessonResponse])
async def list_lessons(admin: VerifiedUser = Depends(require_admin)):
    docs = db.collection("lessons").order_by("grade").stream()
    results = [_to_response(doc.id, doc.to_dict() or {}) for doc in docs]
    results.sort(key=lambda item: (item.grade, item.order))
    return results


@router.patch("/{lesson_id}", response_model=LessonResponse)
async def update_lesson(
    lesson_id: str,
    body: LessonUpdateRequest,
    admin: VerifiedUser = Depends(require_admin),
):
    doc_ref = db.collection("lessons").document(lesson_id)
    if not doc_ref.get().exists:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Lesson not found.",
        )

    update_data = body.model_dump(exclude_unset=True)
    if not update_data:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="No fields provided.",
        )

    if "lessonTag" in update_data:
        matching_lessons = (
            db.collection("lessons")
            .where(
                filter=firestore.FieldFilter(
                    "lessonTag",
                    "==",
                    update_data["lessonTag"],
                )
            )
            .stream()
        )
        for lesson in matching_lessons:
            if lesson.id != lesson_id:
                raise HTTPException(
                    status_code=status.HTTP_409_CONFLICT,
                    detail="Another lesson with that tag exists.",
                )

    update_data["updatedAt"] = firestore.SERVER_TIMESTAMP
    update_data["lastEditedBy"] = admin.uid
    doc_ref.update(update_data)

    data = doc_ref.get().to_dict() or {}
    return _to_response(doc_ref.id, data)


@router.delete("/{lesson_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_lesson(
    lesson_id: str,
    admin: VerifiedUser = Depends(require_admin),
):
    doc_ref = db.collection("lessons").document(lesson_id)
    if not doc_ref.get().exists:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Lesson not found.",
        )

    doc_ref.delete()


# ---------------------------------------------------------------------------
# PDF Links — store URL + title in Firestore
# ---------------------------------------------------------------------------

@router.post("/{lesson_id}/pdfs", status_code=status.HTTP_201_CREATED)
async def add_lesson_pdf_link(
    lesson_id: str,
    body: PdfLinkRequest,
    admin: VerifiedUser = Depends(require_admin),
):
    lesson_ref = db.collection("lessons").document(lesson_id)
    if not lesson_ref.get().exists:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Lesson not found.",
        )

    pdf_id = str(uuid.uuid4())
    lesson_ref.collection("pdfs").document(pdf_id).set(
        {
            "fileName": body.fileName,
            "url": body.url,
            "uploadedBy": admin.uid,
            "uploadedAt": firestore.SERVER_TIMESTAMP,
        }
    )

    return {
        "id": pdf_id,
        "fileName": body.fileName,
        "url": body.url,
    }


@router.get("/{lesson_id}/pdfs")
async def list_lesson_pdfs(
    lesson_id: str,
    admin: VerifiedUser = Depends(require_admin),
):
    docs = (
        db.collection("lessons")
        .document(lesson_id)
        .collection("pdfs")
        .stream()
    )

    results = []
    for doc in docs:
        data = doc.to_dict() or {}
        results.append(
            {
                "id": doc.id,
                "fileName": data.get("fileName", ""),
                "url": data.get("url", ""),
                "uploadedAt": (
                    data["uploadedAt"].isoformat()
                    if data.get("uploadedAt")
                    else ""
                ),
            }
        )

    results.sort(key=lambda item: item["uploadedAt"], reverse=True)
    return results


@router.delete(
    "/{lesson_id}/pdfs/{pdf_id}",
    status_code=status.HTTP_204_NO_CONTENT,
)
async def delete_lesson_pdf(
    lesson_id: str,
    pdf_id: str,
    admin: VerifiedUser = Depends(require_admin),
):
    doc_ref = (
        db.collection("lessons")
        .document(lesson_id)
        .collection("pdfs")
        .document(pdf_id)
    )

    if not doc_ref.get().exists:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="PDF not found.",
        )

    doc_ref.delete()


# ---------------------------------------------------------------------------
# PDF Upload — Cloudinary
# ---------------------------------------------------------------------------

@router.post(
    "/{lesson_id}/pdfs/upload",
    status_code=status.HTTP_201_CREATED,
)
async def upload_lesson_pdf(
    lesson_id: str,
    file: UploadFile = File(...),
    admin: VerifiedUser = Depends(require_admin),
):
    lesson_ref = db.collection("lessons").document(lesson_id)
    if not lesson_ref.get().exists:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Lesson not found.",
        )

    contents = await file.read()

    try:
        loop = asyncio.get_running_loop()
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
    except Exception as exc:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Cloudinary upload failed: {exc}",
        ) from exc

    url = result["secure_url"]
    pdf_id = str(uuid.uuid4())

    lesson_ref.collection("pdfs").document(pdf_id).set(
        {
            "fileName": file.filename,
            "url": url,
            "uploadedBy": admin.uid,
            "uploadedAt": firestore.SERVER_TIMESTAMP,
        }
    )

    return {
        "id": pdf_id,
        "fileName": file.filename,
        "url": url,
    }


# ---------------------------------------------------------------------------
# Materials — local storage + background RAG ingestion
# ---------------------------------------------------------------------------

@router.post(
    "/{lesson_id}/materials/{material_id}/search-index",
    response_model=SearchIndexResponse,
    summary="Extract PDF keywords into an existing lesson_materials document",
)
async def index_lesson_material_for_search(
    lesson_id: str,
    material_id: str,
    file: UploadFile = File(...),
    admin: VerifiedUser = Depends(require_admin),
) -> SearchIndexResponse:
    material_ref = db.collection("lesson_materials").document(material_id)
    material_snapshot = material_ref.get()
    if not material_snapshot.exists:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Learning material not found.",
        )

    material = material_snapshot.to_dict() or {}
    if material.get("lessonId") != lesson_id:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Material does not belong to this lesson.",
        )

    try:
        file_bytes = await file.read()
        text = extract_text(file_bytes, file.filename)
        keywords = extract_keywords(text)
        if not keywords:
            raise ValueError("No searchable text could be extracted from this PDF.")

        pieces = chunk_text(text)
        vectors = embed_texts(pieces)
        lesson = db.collection("lessons").document(lesson_id).get().to_dict() or {}
        chunks = [
            {
                "chunkId": f"{material_id}:{index}",
                "lessonId": lesson_id,
                "materialId": material_id,
                "materialType": material.get("materialType") or "pdf",
                "fileName": material.get("materialName") or file.filename or "",
                "text": piece,
                "sourceReference": f"{material_id}#chunk={index}",
                "grade_level": lesson.get("grade"),
                "topic": lesson.get("lessonTag") or lesson.get("title") or lesson_id,
                "difficulty_tier": "medium",
            }
            for index, piece in enumerate(pieces)
        ]
        replace_material(lesson_id, material_id, chunks, vectors)

        material_ref.update({
            "keywords": keywords,
            "keywordCount": len(keywords),
            "keywordIndexStatus": "indexed",
            "keywordIndexedAt": firestore.SERVER_TIMESTAMP,
            "keywordIndexError": None,
            "semanticIndexStatus": "indexed",
            "semanticChunkCount": len(chunks),
        })
        return SearchIndexResponse(
            materialId=material_id,
            keywordCount=len(keywords),
            status="indexed",
        )
    except Exception as exc:
        material_ref.update({
            "keywordIndexStatus": "failed",
            "keywordIndexError": str(exc)[:500],
        })
        raise HTTPException(
            status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
            detail=str(exc),
        ) from exc

@router.post(
    "/{lesson_id}/materials",
    response_model=MaterialResponse,
    status_code=status.HTTP_201_CREATED,
    summary="Upload lesson material and queue RAG ingestion",
)
async def upload_lesson_material(
    lesson_id: str,
    background_tasks: BackgroundTasks,
    file: UploadFile = File(...),
    materialType: str = Form(...),
    admin: VerifiedUser = Depends(require_admin),
) -> MaterialResponse:
    lesson_ref = db.collection("lessons").document(lesson_id)
    if not lesson_ref.get().exists:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Lesson not found.",
        )

    file_bytes = await file.read()
    file_size_bytes = len(file_bytes)

    material_id = str(uuid.uuid4())
    safe_name = file.filename or "upload.pdf"
    local_file = LOCAL_UPLOAD_DIR / lesson_id / f"{material_id}_{safe_name}"
    local_file.parent.mkdir(parents=True, exist_ok=True)
    local_file.write_bytes(file_bytes)

    # Local disk is the source for RAG. Cloud Storage is optional.
    storage_path = f"local:{local_file.as_posix()}"
    now = firestore.SERVER_TIMESTAMP

    material_data = {
        "fileName": file.filename or safe_name,
        "materialType": materialType,
        "storagePath": storage_path,
        "fileSizeBytes": file_size_bytes,
        "ingestionStatus": "uploaded",
        "chunkCount": 0,
        "uploadedBy": admin.uid,
        "uploadedAt": now,
        "lastProcessedAt": None,
        "createdAt": now,
    }

    material_ref = lesson_ref.collection("materials").document(material_id)
    material_ref.set(material_data)

    lesson_ref.update(
        {
            "materialsCount": firestore.Increment(1),
        }
    )

    background_tasks.add_task(
        ingest_material,
        lesson_id,
        material_id,
    )

    created_doc = material_ref.get().to_dict() or {}

    return MaterialResponse(
        id=material_id,
        fileName=created_doc.get("fileName", ""),
        materialType=created_doc.get("materialType", ""),
        storagePath=created_doc.get("storagePath", ""),
        fileSizeBytes=created_doc.get("fileSizeBytes", 0),
        ingestionStatus=created_doc.get("ingestionStatus", "uploaded"),
        chunkCount=created_doc.get("chunkCount", 0),
        createdAt=(
            created_doc["createdAt"].isoformat()
            if created_doc.get("createdAt")
            else ""
        ),
    )


@router.get(
    "/{lesson_id}/materials/{material_id}/status",
    response_model=MaterialResponse,
    summary="Poll RAG ingestion status for a material",
)
async def get_material_status(
    lesson_id: str,
    material_id: str,
    admin: VerifiedUser = Depends(require_admin),
) -> MaterialResponse:
    material_ref = (
        db.collection("lessons")
        .document(lesson_id)
        .collection("materials")
        .document(material_id)
    )

    snap = material_ref.get()
    if not snap.exists:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Material not found.",
        )

    data = snap.to_dict() or {}

    return MaterialResponse(
        id=material_id,
        fileName=data.get("fileName", ""),
        materialType=data.get("materialType", ""),
        storagePath=data.get("storagePath", ""),
        fileSizeBytes=data.get("fileSizeBytes", 0),
        ingestionStatus=data.get("ingestionStatus", "uploaded"),
        chunkCount=data.get("chunkCount", 0),
        createdAt=(
            data["createdAt"].isoformat()
            if data.get("createdAt")
            else ""
        ),
    )
