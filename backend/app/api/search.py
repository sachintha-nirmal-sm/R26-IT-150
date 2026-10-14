"""Authenticated hybrid-search API backed by the local RAG vector index."""

import re

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
    query_normalized = " ".join(re.findall(r"[a-z0-9]+", body.query.lower()))
    query_tokens = query_normalized.split()
    for hit in hits:
        score = float(hit.get("score") or 0)
        # Low cosine scores tend to be generic/noisy matches.
        if score < 0.30:
            continue
        material_id = str(hit.get("materialId") or "")
        lesson_id = str(hit.get("lessonId") or "")
        if not material_id or not lesson_id:
            continue
        file_name = str(hit.get("fileName") or "PDF Material")
        text = str(hit.get("text") or "").lower()
        metadata = " ".join(
            str(hit.get(key) or "")
            for key in ("topic", "lessonTag", "fileName")
        ).lower()
        haystack = f"{metadata} {text}"
        matched_tokens = sum(
            1
            for token in query_tokens
            if re.search(rf"\b{re.escape(token)}\b", haystack)
        )
        lexical_coverage = (
            matched_tokens / len(query_tokens) if query_tokens else 0.0
        )
        phrase_match = bool(
            query_normalized
            and re.search(rf"\b{re.escape(query_normalized)}\b", haystack)
        )
        metadata_match = bool(
            query_normalized
            and re.search(rf"\b{re.escape(query_normalized)}\b", metadata)
        )

        # A single unsupported word must not produce a generic semantic hit.
        if len(query_tokens) == 1 and matched_tokens == 0:
            continue

        ranking_score = (
            score
            + lexical_coverage * 0.15
            + (0.10 if phrase_match else 0.0)
            + (0.15 if metadata_match else 0.0)
        )
        dedupe_key = f"{lesson_id}:{file_name.strip().lower()}"
        current = best_by_material.get(dedupe_key)
        if current is None or ranking_score > current["rankingScore"]:
            best_by_material[dedupe_key] = {
                "materialId": material_id,
                "lessonId": lesson_id,
                "fileName": file_name,
                "topic": hit.get("topic") or "",
                "semanticScore": score,
                "rankingScore": ranking_score,
            }

    ranked_matches = sorted(
        best_by_material.values(),
        key=lambda item: item["rankingScore"],
        reverse=True,
    )
    # An absolute threshold alone lets generic passages from another physics
    # lesson leak into the results. Keep candidates close to the best match;
    # genuinely ambiguous queries can still return multiple near-equal hits.
    if ranked_matches:
        relative_floor = max(0.30, ranked_matches[0]["rankingScore"] * 0.90)
        ranked_matches = [
            match
            for match in ranked_matches
            if match["rankingScore"] >= relative_floor
        ]

    results = []
    for match in ranked_matches[: body.limit]:
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
