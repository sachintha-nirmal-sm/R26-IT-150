"""Grade-adaptive RAG chatbot: retrieve → prompt → LLM → Firestore log."""

from __future__ import annotations

import uuid

from google.cloud import firestore

from app.core.config import OLLAMA_MODEL
from app.core.firebase import db
from app.rag.chat_llm import chat_complete
from app.rag.prompts import system_prompt_for_grade
from app.rag.retrieve import retrieve_for_chat


def _sessions():
    return db.collection("chat_sessions")


def _load_history(session_id: str, limit: int = 8) -> list[dict]:
    docs = list(
        _sessions()
        .document(session_id)
        .collection("messages")
        .order_by("createdAt")
        .stream()
    )
    history = []
    for doc in docs[-limit:]:
        data = doc.to_dict() or {}
        role = data.get("role")
        text = data.get("text")
        if role in ("user", "assistant") and text:
            history.append({"role": role, "content": text})
    return history


def _fallback_answer(hits: list[dict], grade: int | None) -> str:
    if not hits:
        return (
            "I could not find this in your uploaded syllabus notes for this grade. "
            "Ask your teacher to upload the lesson PDF, or try a more specific topic."
        )
    snippets = "\n\n".join((h.get("text") or "")[:400] for h in hits[:3])
    return (
        f"From your Grade {grade or '?'} notes:\n\n{snippets}\n\n"
        "(Ollama is not running, so this is the retrieved text rather than a rewritten explanation. "
        "Start Ollama for a full tutor-style answer.)"
    )


def answer_chat(
    *,
    student_id: str,
    message: str,
    grade: int | None,
    lesson_id: str | None,
    topic: str | None,
    session_id: str | None,
) -> dict:
    session_id = session_id or str(uuid.uuid4())
    session_ref = _sessions().document(session_id)
    snap = session_ref.get()
    if not snap.exists:
        session_ref.set({
            "studentId": student_id,
            "grade": grade,
            "lessonId": lesson_id,
            "topic": topic,
            "createdAt": firestore.SERVER_TIMESTAMP,
            "updatedAt": firestore.SERVER_TIMESTAMP,
        })
    else:
        session_ref.update({
            "grade": grade,
            "lessonId": lesson_id or (snap.to_dict() or {}).get("lessonId"),
            "topic": topic or (snap.to_dict() or {}).get("topic"),
            "updatedAt": firestore.SERVER_TIMESTAMP,
        })

    history = _load_history(session_id)
    hits = retrieve_for_chat(
        query=message,
        grade=grade,
        lesson_id=lesson_id,
        topic=topic,
    )
    context = "\n\n".join(
        f"[{h.get('sourceReference')}] (grade={h.get('grade_level')}, topic={h.get('topic')})\n{h.get('text')}"
        for h in hits
    ) or "(no matching curriculum chunks)"

    llm_messages = [
        {"role": "system", "content": system_prompt_for_grade(grade)},
        {"role": "system", "content": f"Retrieved curriculum context:\n{context[:6000]}"},
        *history,
        {"role": "user", "content": message},
    ]
    reply = chat_complete(llm_messages, model=OLLAMA_MODEL)
    used_llm = bool(reply)
    if not reply:
        reply = _fallback_answer(hits, grade)

    messages_ref = session_ref.collection("messages")
    messages_ref.document().set({
        "role": "user",
        "text": message,
        "createdAt": firestore.SERVER_TIMESTAMP,
    })
    messages_ref.document().set({
        "role": "assistant",
        "text": reply,
        "retrievedChunkIds": [h.get("chunkId") for h in hits],
        "sourceReferences": [h.get("sourceReference") for h in hits],
        "usedLlm": used_llm,
        "createdAt": firestore.SERVER_TIMESTAMP,
    })

    return {
        "sessionId": session_id,
        "answer": reply,
        "grade": grade,
        "lessonId": lesson_id,
        "usedLlm": used_llm,
        "sources": [
            {
                "chunkId": h.get("chunkId"),
                "sourceReference": h.get("sourceReference"),
                "topic": h.get("topic"),
                "grade_level": h.get("grade_level"),
                "score": h.get("score"),
            }
            for h in hits
        ],
    }
