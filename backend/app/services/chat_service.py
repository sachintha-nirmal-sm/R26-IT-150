"""Grade-adaptive RAG chatbot: retrieve → prompt → LLM → Firestore log."""

from __future__ import annotations

import uuid

import re

from google.cloud import firestore

from app.core.config import OLLAMA_MODEL
from app.core.firebase import db
from app.core.grade import parse_grade
from app.rag.chat_llm import chat_complete
from app.rag.prompts import system_prompt_for_grade
from app.rag.retrieve import retrieve_across_grades, retrieve_for_chat


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


def _sentences(text: str) -> list[str]:
    cleaned = re.sub(r"\s+", " ", text or "").strip()
    if not cleaned:
        return []
    parts = re.split(r"(?<=[.!?])\s+", cleaned)
    return [p.strip() for p in parts if 40 < len(p.strip()) < 320]


MIN_HIT_SCORE = 0.30

_SIMPLIFY = re.compile(
    r"\b(simpl(e|y|er|ify)|easier|too hard|don't understand|do not understand|"
    r"more basic|explain more simply|in simple(r)? (words|terms)|like grade\s*\d+)\b",
    re.I,
)


def _hit_grades(hits: list[dict]) -> list[int]:
    grades = []
    for hit in hits:
        parsed = parse_grade(hit.get("grade_level"))
        if parsed is not None:
            grades.append(parsed)
    return grades


def _strong_hits(hits: list[dict]) -> list[dict]:
    return [h for h in hits if float(h.get("score") or 0) >= MIN_HIT_SCORE]


def _wants_simpler(message: str) -> bool:
    return bool(_SIMPLIFY.search(message or ""))


def _last_user_question(history: list[dict]) -> str:
    for item in reversed(history):
        if item.get("role") == "user" and item.get("content"):
            return str(item["content"])
    return ""


def _retrieval_query(message: str, history: list[dict], simplify: bool) -> str:
    if not simplify:
        return message
    previous = _last_user_question(history)
    content_words = re.findall(r"[a-zA-Z]{4,}", message or "")
    if previous and len(content_words) <= 6:
        return previous
    return f"{previous} {message}".strip()


def _cross_grade_notice(student_grade: int, other_hits: list[dict]) -> str | None:
    others = [g for g in _hit_grades(_strong_hits(other_hits)) if g != student_grade]
    higher = sorted({g for g in others if g > student_grade})
    lower = sorted({g for g in others if g < student_grade}, reverse=True)
    if higher:
        g = higher[0]
        return (
            f"This topic is covered in the Grade {g} lesson notes. "
            f"You are logged in as Grade {student_grade}, so I cannot teach it from the Grade {g} PDFs. "
            f"Ask about topics in your Grade {student_grade} lessons, or wait until this lesson is in your syllabus."
        )
    if lower:
        g = lower[0]
        return (
            f"I could not find this in your Grade {student_grade} lesson PDFs. "
            f"It is covered in the Grade {g} notes. Ask me to explain it more simply "
            f"and I will use the Grade {g} PDFs."
        )
    return None


def _fallback_answer(hits: list[dict], grade: int | None, question: str = "") -> str:
    if not hits:
        return (
            "I could not find this in your uploaded syllabus notes for this grade. "
            "Ask your teacher to upload the lesson PDF, or try a more specific topic."
        )

    q_words = {w for w in re.findall(r"[a-zA-Z]{4,}", (question or "").lower())}
    ranked: list[tuple[int, str]] = []
    for hit in hits:
        for sentence in _sentences(hit.get("text") or ""):
            low = sentence.lower()
            if "figure" in low or "page " in low:
                continue
            score = sum(1 for w in q_words if w in low)
            if "law" in low or "states that" in low or "is " in low:
                score += 1
            ranked.append((score, sentence))

    ranked.sort(key=lambda item: item[0], reverse=True)
    if not ranked:
        snippet = (hits[0].get("text") or "").strip()
        snippet = re.sub(r"\s+", " ", snippet)[:280]
        return snippet

    best = ranked[0][1].rstrip(".")
    return f"{best}."


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
    simplify = _wants_simpler(message)
    source_grade = grade
    hits: list[dict] = []
    query = _retrieval_query(message, history, simplify)

    if simplify and grade and grade > 6:
        source_grade = grade - 1
        hits = retrieve_for_chat(
            query=query,
            grade=source_grade,
            lesson_id=None,
            topic=topic,
        )
        if not _strong_hits(hits):
            reply = (
                f"I could not find a simpler Grade {source_grade} version of this topic "
                f"in the uploaded lesson PDFs. I can still explain from your Grade {grade} notes if you ask again."
            )
            messages_ref = session_ref.collection("messages")
            messages_ref.document().set({
                "role": "user",
                "text": message,
                "createdAt": firestore.SERVER_TIMESTAMP,
            })
            messages_ref.document().set({
                "role": "assistant",
                "text": reply,
                "retrievedChunkIds": [],
                "sourceReferences": [],
                "usedLlm": False,
                "createdAt": firestore.SERVER_TIMESTAMP,
            })
            return {
                "sessionId": session_id,
                "answer": reply,
                "grade": grade,
                "lessonId": lesson_id,
                "usedLlm": False,
                "mode": "simplify-missing",
                "sourceGrade": source_grade,
                "sources": [],
            }
    else:
        hits = retrieve_for_chat(
            query=query,
            grade=grade,
            lesson_id=lesson_id,
            topic=topic,
        )

    if not simplify and (not _strong_hits(hits)):
        other = retrieve_across_grades(query=query, lesson_id=None, topic=topic)
        notice = _cross_grade_notice(int(grade), other) if grade else None
        if notice:
            reply = notice
            used_llm = False
            messages_ref = session_ref.collection("messages")
            messages_ref.document().set({
                "role": "user",
                "text": message,
                "createdAt": firestore.SERVER_TIMESTAMP,
            })
            messages_ref.document().set({
                "role": "assistant",
                "text": reply,
                "retrievedChunkIds": [h.get("chunkId") for h in other],
                "sourceReferences": [h.get("sourceReference") for h in other],
                "usedLlm": False,
                "createdAt": firestore.SERVER_TIMESTAMP,
            })
            return {
                "sessionId": session_id,
                "answer": reply,
                "grade": grade,
                "lessonId": lesson_id,
                "usedLlm": False,
                "mode": "higher-grade-notice",
                "sources": [
                    {
                        "chunkId": h.get("chunkId"),
                        "sourceReference": h.get("sourceReference"),
                        "topic": h.get("topic"),
                        "grade_level": h.get("grade_level"),
                        "score": h.get("score"),
                    }
                    for h in other
                ],
            }

    context = "\n\n".join(
        f"[{h.get('sourceReference')}] (grade={h.get('grade_level')}, topic={h.get('topic')})\n{h.get('text')}"
        for h in hits
    ) or "(no matching curriculum chunks)"

    llm_messages = [
        {
            "role": "system",
            "content": system_prompt_for_grade(
                grade,
                simplify=simplify,
                source_grade=source_grade,
            ),
        },
        {"role": "system", "content": f"Retrieved curriculum context:\n{context[:6000]}"},
        *history,
        {"role": "user", "content": message},
    ]
    reply = chat_complete(llm_messages, model=OLLAMA_MODEL)
    used_llm = bool(reply)
    if not reply:
        if simplify and hits:
            reply = (
                f"Here is a simpler explanation from the Grade {source_grade} notes. "
                + _fallback_answer(hits, source_grade, message)
            )
        else:
            reply = _fallback_answer(hits, grade, message)

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
        "mode": "simplify" if simplify else "grade",
        "sourceGrade": source_grade,
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
