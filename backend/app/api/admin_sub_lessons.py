"""
admin_sub_lessons.py — Sub-lesson CRUD under a lesson.
  POST   /admin/lessons/{lesson_id}/sub-lessons
  GET    /admin/lessons/{lesson_id}/sub-lessons
  PATCH  /admin/lessons/{lesson_id}/sub-lessons/{sub_id}
  DELETE /admin/lessons/{lesson_id}/sub-lessons/{sub_id}
"""

from typing import Optional

from fastapi import APIRouter, Depends, HTTPException, status
from google.cloud import firestore
from pydantic import BaseModel, Field

from app.core.dependencies import VerifiedUser, require_admin
from app.core.firebase import db

router = APIRouter(prefix="/admin/lessons", tags=["Admin - Sub-Lessons"])


class SubLessonCreateRequest(BaseModel):
    number: str = Field(..., min_length=1, max_length=20)   # e.g. "4.1"
    title: str = Field(..., min_length=1, max_length=200)
    order: int = Field(default=1)


class SubLessonUpdateRequest(BaseModel):
    number: Optional[str] = Field(None, min_length=1, max_length=20)
    title: Optional[str] = Field(None, min_length=1, max_length=200)
    order: Optional[int] = None


@router.post("/{lesson_id}/sub-lessons", status_code=status.HTTP_201_CREATED)
async def create_sub_lesson(
    lesson_id: str,
    body: SubLessonCreateRequest,
    admin: VerifiedUser = Depends(require_admin),
):
    lesson_ref = db.collection("lessons").document(lesson_id)
    if not lesson_ref.get().exists:
        raise HTTPException(status_code=404, detail="Lesson not found.")

    now = firestore.SERVER_TIMESTAMP
    ref = lesson_ref.collection("subLessons").document()
    ref.set({
        "number": body.number,
        "title": body.title,
        "order": body.order,
        "createdBy": admin.uid,
        "createdAt": now,
        "updatedAt": now,
    })
    data = ref.get().to_dict()
    return {
        "id": ref.id,
        "number": data.get("number", ""),
        "title": data.get("title", ""),
        "order": data.get("order", 0),
        "createdAt": data["createdAt"].isoformat() if data.get("createdAt") else "",
    }


@router.get("/{lesson_id}/sub-lessons")
async def list_sub_lessons(
    lesson_id: str,
    admin: VerifiedUser = Depends(require_admin),
):
    docs = (
        db.collection("lessons")
        .document(lesson_id)
        .collection("subLessons")
        .order_by("order")
        .stream()
    )
    result = []
    for doc in docs:
        d = doc.to_dict()
        result.append({
            "id": doc.id,
            "number": d.get("number", ""),
            "title": d.get("title", ""),
            "order": d.get("order", 0),
            "createdAt": d["createdAt"].isoformat() if d.get("createdAt") else "",
        })
    return result


@router.patch("/{lesson_id}/sub-lessons/{sub_id}")
async def update_sub_lesson(
    lesson_id: str,
    sub_id: str,
    body: SubLessonUpdateRequest,
    admin: VerifiedUser = Depends(require_admin),
):
    ref = (
        db.collection("lessons")
        .document(lesson_id)
        .collection("subLessons")
        .document(sub_id)
    )
    if not ref.get().exists:
        raise HTTPException(status_code=404, detail="Sub-lesson not found.")

    update_data = body.model_dump(exclude_unset=True)
    if not update_data:
        raise HTTPException(status_code=400, detail="No fields provided.")
    update_data["updatedAt"] = firestore.SERVER_TIMESTAMP
    ref.update(update_data)
    data = ref.get().to_dict()
    return {
        "id": sub_id,
        "number": data.get("number", ""),
        "title": data.get("title", ""),
        "order": data.get("order", 0),
    }


@router.delete("/{lesson_id}/sub-lessons/{sub_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_sub_lesson(
    lesson_id: str,
    sub_id: str,
    admin: VerifiedUser = Depends(require_admin),
):
    ref = (
        db.collection("lessons")
        .document(lesson_id)
        .collection("subLessons")
        .document(sub_id)
    )
    if not ref.get().exists:
        raise HTTPException(status_code=404, detail="Sub-lesson not found.")
    ref.delete()
