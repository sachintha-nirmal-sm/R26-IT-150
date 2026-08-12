"""
api/admin_lessons.py — Admin CRUD endpoints for Lessons.

Implements Admin Workflow (Section 3) and Firestore Schema (Section 3.3):
  - POST /admin/lessons
  - PATCH /admin/lessons/{id}
  - DELETE /admin/lessons/{id}
  - GET /admin/lessons

Restricted to role == "admin" using the require_admin dependency.
"""

import asyncio
import uuid
from typing import Literal
from fastapi import APIRouter, Depends, HTTPException, status, UploadFile, File, Form, BackgroundTasks
from google.cloud import firestore
from pydantic import BaseModel, Field

from app.core.dependencies import VerifiedUser, require_admin
from app.core.firebase import db, bucket

router = APIRouter(prefix="/admin/lessons", tags=["Admin - Lessons"])


# ---------------------------------------------------------------------------
# Request / Response models
# ---------------------------------------------------------------------------

class LessonCreateRequest(BaseModel):
    """Payload to create a new lesson."""
    title: str = Field(..., min_length=1, max_length=200)
    subject: str = Field(..., min_length=1, max_length=50, description="e.g. Physics")
    grade: int = Field(..., ge=9, le=12, description="Grade level (9, 10, 11, or 12)")
    lessonTag: str = Field(..., min_length=1, max_length=50, description="Unique slug e.g. phy-g10-motion")
    description: str = Field(..., max_length=1000)
    order: int = Field(..., description="Display sequence within grade")
    status: Literal["draft", "published"] = Field("draft", description="draft or published")


class LessonUpdateRequest(BaseModel):
    """Payload to update an existing lesson (all fields optional)."""
    title: str | None = Field(None, min_length=1, max_length=200)
    subject: str | None = Field(None, min_length=1, max_length=50)
    grade: int | None = Field(None, ge=9, le=12)
    lessonTag: str | None = Field(None, min_length=1, max_length=50)
    description: str | None = Field(None, max_length=1000)
    order: int | None = Field(None)
    status: Literal["draft", "published"] | None = None


class LessonResponse(BaseModel):
    """Response model for a single lesson."""
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
    materialsCount: int
    createdAt: str
    updatedAt: str


class MaterialResponse(BaseModel):
    """Response model for an uploaded material."""
    id: str
    fileName: str
    materialType: str
    storagePath: str
    fileSizeBytes: int
    ingestionStatus: str
    chunkCount: int
    createdAt: str



# ---------------------------------------------------------------------------
# Endpoints
# ---------------------------------------------------------------------------

@router.post(
    "",
    response_model=LessonResponse,
    status_code=status.HTTP_201_CREATED,
    summary="Create a new lesson",
)
async def create_lesson(
    body: LessonCreateRequest,
    admin: VerifiedUser = Depends(require_admin)
) -> LessonResponse:
    """
    Creates a new lesson document in Firestore (lessons collection).
    Initializes materialsCount to 0 and tracks createdBy.
    """
    # 1. Check for duplicate lessonTag (optional but recommended for uniqueness)
    existing_tags = db.collection("lessons").where(filter=firestore.FieldFilter("lessonTag", "==", body.lessonTag)).limit(1).stream()
    if any(existing_tags):
        raise HTTPException(
            status_code=status.HTTP_409_CONFLICT,
            detail=f"A lesson with tag '{body.lessonTag}' already exists."
        )

    now = firestore.SERVER_TIMESTAMP
    
    lesson_data = {
        "title": body.title,
        "subject": body.subject,
        "grade": body.grade,
        "lessonTag": body.lessonTag,
        "description": body.description,
        "order": body.order,
        "status": body.status,
        
        # Server-managed fields
        "createdBy": admin.uid,
        "lastEditedBy": admin.uid,
        "materialsCount": 0,
        "createdAt": now,
        "updatedAt": now,
    }
    
    # Let Firestore generate a random ID
    doc_ref = db.collection("lessons").document()
    doc_ref.set(lesson_data)
    
    # Read back to get resolved timestamps
    created_doc = doc_ref.get()
    data = created_doc.to_dict()
    
    return LessonResponse(
        id=doc_ref.id,
        title=data["title"],
        subject=data["subject"],
        grade=data["grade"],
        lessonTag=data["lessonTag"],
        description=data["description"],
        order=data["order"],
        status=data["status"],
        createdBy=data["createdBy"],
        lastEditedBy=data["lastEditedBy"],
        materialsCount=data["materialsCount"],
        createdAt=data["createdAt"].isoformat(),
        updatedAt=data["updatedAt"].isoformat(),
    )


@router.get(
    "",
    response_model=list[LessonResponse],
    summary="List all lessons (Admin)",
)
async def list_lessons(admin: VerifiedUser = Depends(require_admin)) -> list[LessonResponse]:
    """Returns all lessons across all grades (admin view)."""
    # Note: Firestore requires a composite index if we use multiple order_by clauses.
    # To avoid crashing before the index is built, we fetch ordered by grade and sort by 'order' in Python.
    lessons_ref = db.collection("lessons").order_by("grade").stream()
    results = []
    
    for doc in lessons_ref:
        data = doc.to_dict()
        results.append(LessonResponse(
            id=doc.id,
            title=data.get("title", ""),
            subject=data.get("subject", ""),
            grade=data.get("grade", 0),
            lessonTag=data.get("lessonTag", ""),
            description=data.get("description", ""),
            order=data.get("order", 0),
            status=data.get("status", "draft"),
            createdBy=data.get("createdBy", ""),
            lastEditedBy=data.get("lastEditedBy"),
            materialsCount=data.get("materialsCount", 0),
            createdAt=data.get("createdAt").isoformat() if data.get("createdAt") else "",
            updatedAt=data.get("updatedAt").isoformat() if data.get("updatedAt") else "",
        ))
        
    # Sort in memory by 'order'
    results.sort(key=lambda x: (x.grade, x.order))
    return results


@router.patch(
    "/{lesson_id}",
    response_model=LessonResponse,
    summary="Update a lesson",
)
async def update_lesson(
    lesson_id: str,
    body: LessonUpdateRequest,
    admin: VerifiedUser = Depends(require_admin)
) -> LessonResponse:
    """
    Updates an existing lesson document.
    Only provided fields are modified. `lastEditedBy` is automatically updated.
    """
    doc_ref = db.collection("lessons").document(lesson_id)
    doc = doc_ref.get()
    
    if not doc.exists:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Lesson '{lesson_id}' not found."
        )
        
    update_data = body.model_dump(exclude_unset=True)
    
    if not update_data:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="No fields provided for update."
        )

    # If lessonTag is changing, check for duplicates
    if "lessonTag" in update_data:
        existing_tags = db.collection("lessons").where(
            filter=firestore.FieldFilter("lessonTag", "==", update_data["lessonTag"])
        ).stream()
        
        for t in existing_tags:
            if t.id != lesson_id:
                raise HTTPException(
                    status_code=status.HTTP_409_CONFLICT,
                    detail=f"Another lesson with tag '{update_data['lessonTag']}' already exists."
                )

    update_data["updatedAt"] = firestore.SERVER_TIMESTAMP
    update_data["lastEditedBy"] = admin.uid
    
    doc_ref.update(update_data)
    
    # Return updated doc
    updated_doc = doc_ref.get()
    data = updated_doc.to_dict()
    
    return LessonResponse(
        id=doc_ref.id,
        title=data["title"],
        subject=data["subject"],
        grade=data["grade"],
        lessonTag=data["lessonTag"],
        description=data["description"],
        order=data["order"],
        status=data["status"],
        createdBy=data["createdBy"],
        lastEditedBy=data["lastEditedBy"],
        materialsCount=data.get("materialsCount", 0),
        createdAt=data["createdAt"].isoformat(),
        updatedAt=data["updatedAt"].isoformat(),
    )


@router.delete(
    "/{lesson_id}",
    status_code=status.HTTP_204_NO_CONTENT,
    summary="Delete a lesson",
)
async def delete_lesson(
    lesson_id: str,
    admin: VerifiedUser = Depends(require_admin)
):
    """
    Deletes a lesson. 
    Note: In a full production system, this might also require cleaning up
    subcollections (materials) and deleting associated Storage files.
    """
    doc_ref = db.collection("lessons").document(lesson_id)
    doc = doc_ref.get()
    
    if not doc.exists:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Lesson '{lesson_id}' not found."
        )
        
    # Strictly speaking, deleting a document doesn't delete its subcollections in Firestore.
    # A true recursive delete would require a background job or multi-batch delete.
    # For now, we perform a simple document delete.
    doc_ref.delete()


# ---------------------------------------------------------------------------
# Background Task Stub
# ---------------------------------------------------------------------------

async def stub_rag_ingestion_pipeline(lesson_id: str, material_id: str):
    """
    Stub background task that mimics the RAG ingestion pipeline.
    Later, this will actually chunk the PDF, call the embedding model, and store vectors.
    """
    material_ref = db.collection("lessons").document(lesson_id).collection("materials").document(material_id)
    
    # Simulate processing time
    await asyncio.sleep(2)
    material_ref.update({"ingestionStatus": "chunking"})
    print(f"[RAG Stub] Material {material_id} status -> chunking")
    
    await asyncio.sleep(3)
    material_ref.update({
        "ingestionStatus": "embedded",
        "chunkCount": 12  # Stub chunk count
    })
    print(f"[RAG Stub] Material {material_id} status -> embedded")


# ---------------------------------------------------------------------------
# Materials Endpoint
# ---------------------------------------------------------------------------

@router.post(
    "/{lesson_id}/upload-pdf",
    response_model=MaterialResponse,
    status_code=status.HTTP_201_CREATED,
    summary="Upload PDF for a lesson (alias)",
)
@router.post(
    "/{lesson_id}/materials",
    response_model=MaterialResponse,
    status_code=status.HTTP_201_CREATED,
    summary="Upload learning material for a lesson",
)
async def upload_material(
    lesson_id: str,
    background_tasks: BackgroundTasks,
    file: UploadFile = File(...),
    materialType: Literal["pdf", "theoryNotes", "formulaSheet", "calculationSheet", "other"] = Form("pdf"),
    admin: VerifiedUser = Depends(require_admin)
) -> MaterialResponse:
    """
    Uploads a learning material (e.g. PDF) for a specific lesson to Firebase Storage,
    creates a metadata document in Firestore, and triggers the RAG ingestion pipeline.
    """
    lesson_ref = db.collection("lessons").document(lesson_id)
    lesson_doc = lesson_ref.get()
    
    if not lesson_doc.exists:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Lesson '{lesson_id}' not found."
        )

    # 1. Read file to determine size and upload to Cloud Storage
    file_bytes = await file.read()
    file_size_bytes = len(file_bytes)
    
    if file_size_bytes == 0:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Uploaded file is empty."
        )

    material_id = str(uuid.uuid4())

    # Firebase Storage requires Blaze plan — save to local disk instead
    import pathlib
    upload_dir = pathlib.Path(__file__).parent.parent.parent / "uploads"
    upload_dir.mkdir(parents=True, exist_ok=True)
    local_filename = f"{lesson_id}_{material_id}_{file.filename}"
    local_path = upload_dir / local_filename
    local_path.write_bytes(file_bytes)
    storage_path = str(local_path)

    # 2. Write metadata document to Firestore
    now = firestore.SERVER_TIMESTAMP
    material_data = {
        "fileName": file.filename,
        "materialType": materialType,
        "storagePath": storage_path,
        "fileSizeBytes": file_size_bytes,
        "ingestionStatus": "uploaded",
        "chunkCount": 0,
        "createdAt": now,
    }
    
    material_ref = lesson_ref.collection("materials").document(material_id)
    material_ref.set(material_data)
    
    # Update denormalized count on the parent lesson
    lesson_ref.update({
        "materialsCount": firestore.Increment(1)
    })
    
    # 3. Queue the background RAG ingestion task
    background_tasks.add_task(stub_rag_ingestion_pipeline, lesson_id, material_id)
    
    # Read back to get timestamp
    created_doc = material_ref.get().to_dict()
    
    return MaterialResponse(
        id=material_id,
        fileName=created_doc["fileName"],
        materialType=created_doc["materialType"],
        storagePath=created_doc["storagePath"],
        fileSizeBytes=created_doc["fileSizeBytes"],
        ingestionStatus=created_doc["ingestionStatus"],
        chunkCount=created_doc["chunkCount"],
        createdAt=created_doc["createdAt"].isoformat(),
    )
