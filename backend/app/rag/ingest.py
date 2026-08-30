"""Material ingestion: Storage → text → chunks → embeddings → local vector store."""

from pathlib import Path

from google.cloud import firestore

from app.core.config import LOCAL_UPLOAD_DIR
from app.core.firebase import bucket, db
from app.rag.chunking import chunk_text
from app.rag.embeddings import embed_texts
from app.rag.text_extract import extract_text
from app.rag.vector_store import replace_material


def _read_file_bytes(storage_path: str) -> bytes:
    if storage_path.startswith("local:"):
        path = Path(storage_path.removeprefix("local:"))
        return path.read_bytes()
    try:
        return bucket.blob(storage_path).download_as_bytes()
    except Exception:
        # Fallback: same relative path under local uploads
        local = LOCAL_UPLOAD_DIR / storage_path
        if local.exists():
            return local.read_bytes()
        raise


def ingest_material(lesson_id: str, material_id: str) -> int:
    material_ref = (
        db.collection("lessons").document(lesson_id)
        .collection("materials").document(material_id)
    )
    snap = material_ref.get()
    if not snap.exists:
        raise FileNotFoundError(f"Material {material_id} not found.")
    data = snap.to_dict() or {}
    storage_path = data.get("storagePath")
    file_name = data.get("fileName") or ""

    try:
        material_ref.update({
            "ingestionStatus": "chunking",
            "ingestionError": None,
        })

        file_bytes = _read_file_bytes(storage_path)
        text = extract_text(file_bytes, file_name)
        pieces = chunk_text(text)
        if not pieces:
            raise ValueError(
                "No text could be extracted. If this is a scanned PDF, OCR is required."
            )

        vectors = embed_texts(pieces)
        lesson = db.collection("lessons").document(lesson_id).get().to_dict() or {}
        grade_level = lesson.get("grade")
        topic = lesson.get("lessonTag") or lesson.get("title") or lesson_id
        material_type = data.get("materialType")
        difficulty_tier = {
            "theoryNotes": "easy",
            "formulaSheet": "medium",
            "calculationSheet": "hard",
        }.get(material_type, "medium")

        chunks = []
        for i, piece in enumerate(pieces):
            chunks.append({
                "chunkId": f"{material_id}:{i}",
                "lessonId": lesson_id,
                "materialId": material_id,
                "materialType": material_type,
                "fileName": file_name,
                "text": piece,
                "sourceReference": f"{material_id}#chunk={i}",
                "grade_level": grade_level,
                "topic": topic,
                "difficulty_tier": difficulty_tier,
            })

        count = replace_material(lesson_id, material_id, chunks, vectors)
        material_ref.update({
            "ingestionStatus": "embedded",
            "chunkCount": count,
            "lastProcessedAt": firestore.SERVER_TIMESTAMP,
            "ingestionError": None,
        })
        return count
    except Exception as exc:
        material_ref.update({
            "ingestionStatus": "failed",
            "ingestionError": str(exc)[:500],
            "lastProcessedAt": firestore.SERVER_TIMESTAMP,
        })
        raise
