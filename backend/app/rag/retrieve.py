from app.core.config import RAG_TOP_K
from app.rag.embeddings import embed_query
from app.rag.vector_store import search, search_adaptive


def retrieve_for_lesson(lesson_id: str, query: str, k: int = RAG_TOP_K) -> list[dict]:
    vector = embed_query(query)
    return search(lesson_id, vector, k=k)


def retrieve_for_lessons(
    lesson_ids: list[str],
    query: str,
    k_per_lesson: int = RAG_TOP_K,
) -> list[dict]:
    hits: list[dict] = []
    for lesson_id in lesson_ids:
        hits.extend(retrieve_for_lesson(lesson_id, query, k=k_per_lesson))
    hits.sort(key=lambda h: h.get("score") or 0, reverse=True)
    return hits


def retrieve_for_chat(
    query: str,
    grade: int | None = None,
    lesson_id: str | None = None,
    topic: str | None = None,
    k: int = RAG_TOP_K,
) -> list[dict]:
    """Grade-adaptive retrieval for the chatbot (admin lessons tagged by grade)."""
    vector = embed_query(query)
    return search_adaptive(
        vector,
        k=k,
        grade=grade,
        lesson_id=lesson_id,
        topic=topic,
    )


def retrieve_across_grades(
    query: str,
    lesson_id: str | None = None,
    topic: str | None = None,
    k: int = RAG_TOP_K,
) -> list[dict]:
    """Search all ingested lesson PDFs, any grade (to detect higher/lower syllabus)."""
    vector = embed_query(query)
    return search_adaptive(
        vector,
        k=k,
        grade=None,
        lesson_id=lesson_id,
        topic=topic,
    )
