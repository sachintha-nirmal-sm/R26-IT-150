"""RAG pipeline: ingest materials, retrieve chunks, generate grounded questions.

Embeddings and chunk text live in a local vector store under `data/vector_store/`.
Firestore only stores `sourceReference` pointers and ingestion status.
"""

from app.rag.ingest import ingest_material
from app.rag.retrieve import retrieve_for_chat, retrieve_for_lesson, retrieve_for_lessons
from app.rag.question_gen import generate_question_bank, generate_final_quiz_questions

__all__ = [
    "ingest_material",
    "retrieve_for_chat",
    "retrieve_for_lesson",
    "retrieve_for_lessons",
    "generate_question_bank",
    "generate_final_quiz_questions",
]
