"""
keyword_aggregate.py — builds/maintains search_keyword_index, the master
keyword set that powers smart search recommendations (see the
"PDF Keyword Aggregation -> Smart Search Recommendations" project plan).

Two entry points, both judging a keyword the same way via
passes_quality_filter():

- aggregate_keywords(): full rescan across every material's current
  keyword list. Backfill/self-healing tool — needed for freshly-seeded
  data, and for catching a keyword that only crosses the document-
  frequency threshold once other materials are added later.
- apply_material_keywords(): incremental update for exactly one material,
  called automatically right after its keywords are extracted
  (app/api/admin_lessons.py's index_lesson_material_for_search), so a
  newly uploaded PDF becomes recommendable immediately with no separate
  admin step.

Both read broadly (every material's keyword list, to compute an accurate
document-frequency ratio) but the incremental path writes narrowly (only
the touched keywords' index docs), keeping its blast radius small.
"""

from __future__ import annotations

from google.cloud import firestore

from app.core.firebase import db
from app.rag.keyword_extract import PHYSICS_CONCEPTS, STOP_WORDS

# Generic words that survive keyword_extract.py's small per-material
# stoplist (tuned for single-document extraction, not cross-material
# aggregation) but carry no lesson-specific signal once pooled across many
# materials. Extends, never replaces, the extractor's own stoplist.
_AGGREGATION_STOP_WORDS = STOP_WORDS | {
    "figure", "example", "examples", "shown", "these", "those", "such",
    "very", "each", "some", "other", "than", "then", "also", "can",
    "may", "like", "used", "use", "using", "given", "where", "when",
    "page", "section", "chapter", "note", "notes",
}

MAX_DOCUMENT_FREQUENCY_RATIO = 0.6
INDEX_COLLECTION = "search_keyword_index"


def passes_quality_filter(
    keyword: str,
    material_count: int,
    total_materials: int,
    max_document_frequency_ratio: float = MAX_DOCUMENT_FREQUENCY_RATIO,
    *,
    ai_selected: bool = False,
) -> bool:
    """A keyword is noise if it's a generic/stop word, or if it shows up in
    more than max_document_frequency_ratio of all materials — that common
    means it isn't specific to any particular lesson's content.

    The stoplist check is never bypassable, including by ai_selected —
    Groq's own picks have been observed padding out to a requested count
    with exactly these generic words (figure, example, note...), so this
    is a real safety net, not just defensive coding. ai_selected only
    overrides the document-frequency ratio, for a genuinely important term
    that happens to recur across most materials."""
    if keyword in _AGGREGATION_STOP_WORDS:
        return False
    if ai_selected:
        return True
    if total_materials <= 0:
        return False
    return (material_count / total_materials) <= max_document_frequency_ratio


def _material_grade(data: dict) -> str | None:
    grade = data.get("grade")
    return grade if isinstance(grade, str) else None


def _iter_all_materials() -> list[dict]:
    """One row per distinct material, across both lesson_materials (the
    real source of truth, written by the live upload flow) and pdf_keywords
    (written by a code path with no current Flutter caller, kept for
    forward-compatibility since simple_search_service.dart already reads
    both)."""
    seen: dict[str, dict] = {}

    for doc in db.collection("lesson_materials").stream():
        data = doc.to_dict() or {}
        keywords = data.get("keywords") or []
        if not keywords:
            continue
        seen[doc.id] = {
            "materialId": doc.id,
            "lessonId": data.get("lessonId"),
            "lessonTitle": data.get("lessonTitle"),
            "grade": _material_grade(data),
            "keywords": keywords,
            "aiSelectedKeywords": set(data.get("aiSelectedKeywords") or []),
        }

    for doc in db.collection("pdf_keywords").stream():
        if doc.id in seen:
            continue
        data = doc.to_dict() or {}
        keywords = data.get("keywords") or []
        if not keywords:
            continue
        seen[doc.id] = {
            "materialId": doc.id,
            "lessonId": data.get("lessonId"),
            "lessonTitle": data.get("lessonTitle"),
            "grade": _material_grade(data),
            "keywords": keywords,
            "aiSelectedKeywords": set(data.get("aiSelectedKeywords") or []),
        }

    return list(seen.values())


def _compute_counts(materials: list[dict]) -> dict[str, int]:
    counts: dict[str, int] = {}
    for material in materials:
        for keyword in set(material["keywords"]):
            counts[keyword] = counts.get(keyword, 0) + 1
    return counts


def _ai_selected_union(materials: list[dict]) -> set[str]:
    """A keyword Groq picked as genuinely search-worthy for at least one
    material bypasses the mechanical document-frequency filter below — that
    judgment is a stronger signal than raw frequency counting (see
    app/rag/keyword_ai_select.py)."""
    selected: set[str] = set()
    for material in materials:
        selected |= material.get("aiSelectedKeywords") or set()
    return selected


def _human_status(existing: dict | None) -> str | None:
    """Only a status set via the admin curation screen (PATCH .../{keyword})
    counts as human-reviewed; auto_approved/auto_rejected are always safe
    for either aggregation path to overwrite as material counts change."""
    if not existing or not existing.get("reviewedBy"):
        return None
    return existing.get("status")


def aggregate_keywords() -> list[dict]:
    """Full rescan: rebuilds search_keyword_index from every material's
    current keyword list. Preserves human-reviewed status; deletes index
    docs no material backs any more."""
    materials = _iter_all_materials()
    total_materials = len(materials)
    counts = _compute_counts(materials)
    ai_selected = _ai_selected_union(materials)

    refs_by_keyword: dict[str, list[dict]] = {}
    for material in materials:
        for keyword in set(material["keywords"]):
            refs_by_keyword.setdefault(keyword, []).append({
                "materialId": material["materialId"],
                "lessonId": material["lessonId"],
                "lessonTitle": material["lessonTitle"],
                "grade": material["grade"],
            })

    existing_docs = {
        doc.id: doc.to_dict() or {}
        for doc in db.collection(INDEX_COLLECTION).stream()
    }

    batch = db.batch()
    written: list[dict] = []
    for keyword, material_count in counts.items():
        is_ai_selected = keyword in ai_selected
        passes = passes_quality_filter(
            keyword, material_count, total_materials, ai_selected=is_ai_selected
        )
        existing = existing_docs.get(keyword)
        human_status = _human_status(existing)
        ref = db.collection(INDEX_COLLECTION).document(keyword)

        if not passes and human_status is None:
            if existing is not None:
                batch.delete(ref)
            continue

        status = human_status or ("auto_approved" if passes else "auto_rejected")
        doc = {
            "keyword": keyword,
            "materialCount": material_count,
            "totalMaterials": total_materials,
            "isPhysicsConcept": keyword in PHYSICS_CONCEPTS,
            "aiSelected": is_ai_selected,
            "materialRefs": refs_by_keyword.get(keyword, []),
            "status": status,
            "updatedAt": firestore.SERVER_TIMESTAMP,
        }
        batch.set(ref, doc, merge=True)
        written.append(doc)

    live_keywords = set(counts.keys())
    for keyword in existing_docs:
        if keyword not in live_keywords:
            batch.delete(db.collection(INDEX_COLLECTION).document(keyword))

    batch.commit()
    return written


def apply_material_keywords(
    material_id: str,
    lesson_id: str | None,
    lesson_title: str | None,
    grade: str | None,
    keywords: list[str],
    ai_selected_keywords: list[str] | None = None,
) -> None:
    """Incremental update for exactly one material — called automatically
    right after its keywords are extracted, so it's recommendable without a
    separate admin sweep. Reads every material's keyword list to compute an
    accurate document-frequency ratio, but only writes index docs for this
    material's own keywords.

    ai_selected_keywords (optional): Groq's pick of this material's best
    keywords (see keyword_ai_select.select_best_keywords) — these bypass
    the document-frequency filter, since that judgment is a stronger
    signal than raw frequency counting."""
    if not keywords:
        return

    ai_selected = set(ai_selected_keywords or [])
    materials = _iter_all_materials()
    total_materials = len(materials)
    counts = _compute_counts(materials)

    batch = db.batch()
    for keyword in set(keywords):
        material_count = counts.get(keyword, 1)
        is_ai_selected = keyword in ai_selected
        passes = passes_quality_filter(
            keyword, material_count, total_materials, ai_selected=is_ai_selected
        )

        ref = db.collection(INDEX_COLLECTION).document(keyword)
        snap = ref.get()
        existing = snap.to_dict() if snap.exists else None
        human_status = _human_status(existing)

        if not passes and human_status is None:
            continue

        status = human_status or "auto_approved"
        material_refs = list(existing.get("materialRefs", [])) if existing else []
        if not any(r.get("materialId") == material_id for r in material_refs):
            material_refs.append({
                "materialId": material_id,
                "lessonId": lesson_id,
                "lessonTitle": lesson_title,
                "grade": grade,
            })

        batch.set(ref, {
            "keyword": keyword,
            "materialCount": material_count,
            "totalMaterials": total_materials,
            "isPhysicsConcept": keyword in PHYSICS_CONCEPTS,
            "aiSelected": is_ai_selected,
            "materialRefs": material_refs,
            "status": status,
            "updatedAt": firestore.SERVER_TIMESTAMP,
        }, merge=True)

    batch.commit()
