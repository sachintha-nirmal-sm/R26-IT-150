"""
Backfill: mirrors PDFs uploaded from the admin Lessons tab
(lessons/{id}/pdfs) into lesson_materials and indexes them for search, so
students can find them. Safe to re-run — skips PDFs already mirrored.

Run from the backend folder with its venv active:
    python ../firebase/scripts/backfill_lesson_pdfs_to_materials.py
"""
import asyncio
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[2] / "backend"))

import httpx  # noqa: E402

from app.api.admin_lessons import _publish_pdf_to_student_materials  # noqa: E402
from app.core.firebase import db  # noqa: E402


async def main() -> None:
    async with httpx.AsyncClient(timeout=60, follow_redirects=True) as client:
        for lesson in db.collection("lessons").stream():
            for pdf in lesson.reference.collection("pdfs").stream():
                data = pdf.to_dict() or {}
                if db.collection("lesson_materials").document(pdf.id).get().exists:
                    print(f"skip   {lesson.id}/{pdf.id} (already mirrored)")
                    continue
                url = data.get("url")
                if not url:
                    continue
                try:
                    response = await client.get(url)
                    response.raise_for_status()
                    await _publish_pdf_to_student_materials(
                        lesson.id, pdf.id, data.get("fileName"), url, {},
                        response.content, data.get("uploadedBy", ""),
                    )
                    print(f"mirror {lesson.id}/{pdf.id} {data.get('fileName')}")
                except Exception as exc:  # noqa: BLE001
                    print(f"FAILED {lesson.id}/{pdf.id}: {exc}")


asyncio.run(main())
