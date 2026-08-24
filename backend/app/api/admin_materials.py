"""
admin_materials.py — Upload lesson materials to Cloudinary via backend.
  POST /admin/upload-material  — upload PDF/file, returns Cloudinary URL
"""

import os
import cloudinary
import cloudinary.uploader
from fastapi import APIRouter, Depends, File, Form, HTTPException, UploadFile
from app.core.dependencies import VerifiedUser, require_admin

router = APIRouter()

cloudinary.config(
    cloud_name=os.getenv("CLOUDINARY_CLOUD_NAME"),
    api_key=os.getenv("CLOUDINARY_API_KEY"),
    api_secret=os.getenv("CLOUDINARY_API_SECRET"),
    secure=True,
)


@router.post("/admin/upload-material")
async def upload_material(
    file: UploadFile = File(...),
    grade: str = Form(...),
    lesson_id: str = Form(...),
    lesson_title: str = Form(...),
    topic: str = Form(default="General"),
    user: VerifiedUser = Depends(require_admin),
):
    try:
        contents = await file.read()
        filename = file.filename or "material"

        # Determine resource type
        ext = filename.rsplit(".", 1)[-1].lower() if "." in filename else ""
        resource_type = "raw" if ext == "pdf" else "image" if ext in ("jpg", "jpeg", "png", "gif", "webp") else "raw"

        folder = f"physics_lab/{grade}"

        result = cloudinary.uploader.upload(
            contents,
            resource_type=resource_type,
            folder=folder,
            use_filename=True,
            unique_filename=True,
            overwrite=False,
            type="upload",         # ensures public access
            access_mode="public",
        )

        return {
            "success": True,
            "url": result.get("secure_url"),
            "publicId": result.get("public_id"),
            "fileSize": result.get("bytes", 0),
            "resourceType": result.get("resource_type"),
        }

    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Upload failed: {str(e)}")
