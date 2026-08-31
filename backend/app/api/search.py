"""Authenticated hybrid-search API backed by the local RAG vector index."""

from fastapi import APIRouter, Depends
from pydantic import BaseModel, Field

from app.core.dependencies import VerifiedUser, require_auth
from app.core.firebase import db
from app.rag.retrieve import retrieve_for_chat


router = APIRouter(prefix="/search", tags=["Search"])


class SemanticSearchRequest(BaseModel):
    query: str = Field(..., min_length=2, max_length=300)
    grade: int | None = Field(None, ge=9, le=11)
    limit: int = Field(10, ge=1, le=20)


def _grade_number(value: object, fallback: int | None = None) -> int | None:
    if isinstance(value, int):
        return value
    digits = "".join(character for character in str(value or "") if character.isdigit())
    return int(digits) if digits else fallback


@router.post("/semantic")
def semantic_search(
    body: SemanticSearchRequest,
    user: VerifiedUser = Depends(require_auth),
) -> dict:
    grade = body.grade
    if user.role == "student":
        profile = db.collection("users").document(user.uid).get()
        profile_data = profile.to_dict() if profile.exists else {}
        grade = _grade_number(profile_data.get("currentGrade"), grade)

    hits = retrieve_for_chat(query=body.query, grade=grade, k=body.limit * 3)
    print(
        f"[semantic-search] uid={user.uid} grade={grade} "
        f"query={body.query!r} vector_hits={len(hits)}"
    )
    best_by_material: dict[str, dict] = {}
    for hit in hits:
        score = float(hit.get("score") or 0)
        # Low cosine scores tend to be generic/noisy matches.
        if score < 0.25:
            continue
        material_id = str(hit.get("materialId") or "")
        lesson_id = str(hit.get("lessonId") or "")
        if not material_id or not lesson_id:
            continue
        file_name = str(hit.get("fileName") or "PDF Material")
        dedupe_key = f"{lesson_id}:{file_name.strip().lower()}"
        current = best_by_material.get(dedupe_key)
        if current is None or score > current["semanticScore"]:
            best_by_material[dedupe_key] = {
                "materialId": material_id,
                "lessonId": lesson_id,
                "fileName": file_name,
                "topic": hit.get("topic") or "",
                "semanticScore": score,
            }

    results = []
    for match in sorted(
        best_by_material.values(),
        key=lambda item: item["semanticScore"],
        reverse=True,
    )[: body.limit]:
        material = (
            db.collection("lesson_materials")
            .document(match["materialId"])
            .get()
        )
        material_data = material.to_dict() if material.exists else {}
        lesson = db.collection("lessons").document(match["lessonId"]).get()
        lesson_data = lesson.to_dict() if lesson.exists else {}
        results.append({
            "id": match["materialId"],
            "title": material_data.get("materialName") or match["fileName"],
            "description": material_data.get("description")
            or f"Semantic match in {lesson_data.get('title') or match['topic']}",
            "type": "Learning Materials",
            "subtype": material_data.get("materialType") or "PDF",
            "lessonId": match["lessonId"],
            "lessonTitle": material_data.get("lessonTitle")
            or lesson_data.get("title")
            or match["topic"],
            "grade": f"Grade {grade}" if grade else "",
            "keywords": material_data.get("keywords") or [],
            "source": "Semantic PDF content",
            "semanticScore": round(match["semanticScore"], 6),
        })

    print(f"[semantic-search] returned={len(results)}")
    return {"query": body.query, "grade": grade, "results": results}
