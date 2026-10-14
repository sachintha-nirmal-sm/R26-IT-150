"""On-disk cosine vector store per lesson (not Firestore)."""

from __future__ import annotations

import json
from pathlib import Path
from typing import Any

import numpy as np

from app.core.config import VECTOR_STORE_DIR


def _lesson_dir(lesson_id: str) -> Path:
    path = VECTOR_STORE_DIR / lesson_id
    path.mkdir(parents=True, exist_ok=True)
    return path


def _meta_path(lesson_id: str) -> Path:
    return _lesson_dir(lesson_id) / "chunks.json"


def _vec_path(lesson_id: str) -> Path:
    return _lesson_dir(lesson_id) / "vectors.npy"


def load(lesson_id: str) -> tuple[list[dict[str, Any]], np.ndarray]:
    meta_file = _meta_path(lesson_id)
    vec_file = _vec_path(lesson_id)
    if not meta_file.exists() or not vec_file.exists():
        return [], np.zeros((0, 384), dtype=np.float32)
    chunks = json.loads(meta_file.read_text(encoding="utf-8"))
    vectors = np.load(vec_file)
    if vectors.ndim == 1:
        vectors = vectors.reshape(1, -1)
    return chunks, vectors.astype(np.float32)


def save(lesson_id: str, chunks: list[dict[str, Any]], vectors: np.ndarray) -> None:
    _meta_path(lesson_id).write_text(
        json.dumps(chunks, ensure_ascii=False, indent=2),
        encoding="utf-8",
    )
    np.save(_vec_path(lesson_id), vectors.astype(np.float32))


def replace_material(
    lesson_id: str,
    material_id: str,
    new_chunks: list[dict[str, Any]],
    new_vectors: np.ndarray,
) -> int:
    chunks, vectors = load(lesson_id)
    keep_idx = [i for i, c in enumerate(chunks) if c.get("materialId") != material_id]
    kept_chunks = [chunks[i] for i in keep_idx]
    kept_vectors = vectors[keep_idx] if keep_idx else np.zeros((0, new_vectors.shape[1] if new_vectors.size else 384), dtype=np.float32)

    if new_vectors.size:
        if kept_vectors.size:
            vectors_out = np.vstack([kept_vectors, new_vectors])
        else:
            vectors_out = new_vectors
    else:
        vectors_out = kept_vectors

    chunks_out = kept_chunks + new_chunks
    save(lesson_id, chunks_out, vectors_out)
    return len(new_chunks)


def list_lesson_ids() -> list[str]:
    if not VECTOR_STORE_DIR.exists():
        return []
    return [p.name for p in VECTOR_STORE_DIR.iterdir() if p.is_dir() and not p.name.startswith("_")]


def _matches_filters(chunk: dict[str, Any], grade: int | None, lesson_id: str | None, topic: str | None) -> bool:
    if lesson_id and chunk.get("lessonId") and chunk.get("lessonId") != lesson_id:
        return False
    if grade is not None:
        chunk_grade = chunk.get("grade_level")
        if chunk_grade is not None and int(chunk_grade) != int(grade):
            return False
    if topic:
        topic_l = topic.lower()
        hay = " ".join(
            str(chunk.get(k) or "") for k in ("topic", "lessonTag", "fileName")
        ).lower()
        if topic_l not in hay and hay not in topic_l:
            # soft filter: keep if topic metadata missing
            if chunk.get("topic"):
                return False
    return True


def search(
    lesson_id: str,
    query_vector: np.ndarray,
    k: int = 4,
    grade: int | None = None,
    topic: str | None = None,
) -> list[dict[str, Any]]:
    chunks, vectors = load(lesson_id)
    return _rank(chunks, vectors, query_vector, k, grade=grade, lesson_id=lesson_id, topic=topic)


def search_adaptive(
    query_vector: np.ndarray,
    k: int = 4,
    grade: int | None = None,
    lesson_id: str | None = None,
    topic: str | None = None,
) -> list[dict[str, Any]]:
    """Grade/topic-filtered search across one lesson or the whole index."""
    if lesson_id:
        return search(lesson_id, query_vector, k=k, grade=grade, topic=topic)

    all_chunks: list[dict[str, Any]] = []
    vec_rows: list[np.ndarray] = []
    for lid in list_lesson_ids():
        chunks, vectors = load(lid)
        if not chunks or vectors.size == 0:
            continue
        for i, chunk in enumerate(chunks):
            if not _matches_filters(chunk, grade, lid, topic):
                continue
            all_chunks.append(chunk)
            vec_rows.append(vectors[i])
    if not vec_rows:
        return []
    stacked = np.vstack(vec_rows)
    return _rank(all_chunks, stacked, query_vector, k, grade=None, lesson_id=None, topic=None)


def _rank(
    chunks: list[dict[str, Any]],
    vectors: np.ndarray,
    query_vector: np.ndarray,
    k: int,
    grade: int | None,
    lesson_id: str | None,
    topic: str | None,
) -> list[dict[str, Any]]:
    if not chunks or vectors.size == 0:
        return []
    keep_chunks = []
    keep_vecs = []
    for i, chunk in enumerate(chunks):
        if not _matches_filters(chunk, grade, lesson_id, topic):
            continue
        keep_chunks.append(chunk)
        keep_vecs.append(vectors[i])
    if not keep_chunks:
        return []
    matrix = np.vstack(keep_vecs)
    q = query_vector.reshape(1, -1)
    scores = (matrix @ q.T).ravel()
    k = min(k, len(keep_chunks))
    top = np.argsort(scores)[::-1][:k]
    results = []
    for i in top:
        item = dict(keep_chunks[int(i)])
        item["score"] = float(scores[int(i)])
        results.append(item)
    return results


def stats(lesson_id: str) -> dict[str, Any]:
    chunks, vectors = load(lesson_id)
    materials = sorted({c.get("materialId") for c in chunks if c.get("materialId")})
    return {
        "lessonId": lesson_id,
        "chunkCount": len(chunks),
        "materialIds": materials,
        "embeddingDim": int(vectors.shape[1]) if vectors.size else 0,
    }
