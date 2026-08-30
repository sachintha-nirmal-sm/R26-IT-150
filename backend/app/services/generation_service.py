"""Async question-bank / final-quiz generation using the RAG pipeline."""

from __future__ import annotations

import asyncio
import uuid

from fastapi import HTTPException, status
from google.cloud import firestore

from app.core.config import OLLAMA_MODEL, RAG_QUESTIONS_PER_BANK
from app.core.firebase import db
from app.rag.question_gen import generate_final_quiz_questions, generate_question_bank


def enqueue_question_bank_job(
    admin_uid: str,
    lesson_id: str,
    quiz_id: str,
    llm_model: str | None = None,
    question_count: int | None = None,
) -> str:
    lesson = db.collection("lessons").document(lesson_id).get()
    if not lesson.exists:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Lesson not found.")
    quiz_ref = db.collection("lessons").document(lesson_id).collection("quizzes").document(quiz_id)
    if not quiz_ref.get().exists:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Quiz not found.")

    quiz_ref.update({"status": "regenerating", "updatedAt": firestore.SERVER_TIMESTAMP})
    job_id = str(uuid.uuid4())
    db.collection("generationJobs").document(job_id).set({
        "jobType": "questionBankGeneration",
        "targetLessonId": lesson_id,
        "targetQuizId": quiz_id,
        "targetGrade": None,
        "targetQuestionCount": question_count or RAG_QUESTIONS_PER_BANK,
        "llmModelUsed": llm_model or OLLAMA_MODEL,
        "status": "queued",
        "progressPercent": 0,
        "requestedBy": admin_uid,
        "resultVersionId": None,
        "errorMessage": None,
        "startedAt": firestore.SERVER_TIMESTAMP,
        "completedAt": None,
    })
    return job_id


def enqueue_final_quiz_job(admin_uid: str, grade: int, llm_model: str | None = None) -> str:
    job_id = str(uuid.uuid4())
    db.collection("generationJobs").document(job_id).set({
        "jobType": "finalQuizGeneration",
        "targetLessonId": None,
        "targetQuizId": None,
        "targetGrade": grade,
        "llmModelUsed": llm_model or OLLAMA_MODEL,
        "status": "queued",
        "progressPercent": 0,
        "requestedBy": admin_uid,
        "resultVersionId": None,
        "errorMessage": None,
        "startedAt": firestore.SERVER_TIMESTAMP,
        "completedAt": None,
    })
    return job_id


def _fail_job(job_ref, quiz_ref, message: str) -> None:
    job_ref.update({
        "status": "failed",
        "errorMessage": message[:500],
        "completedAt": firestore.SERVER_TIMESTAMP,
    })
    if quiz_ref is not None:
        quiz = quiz_ref.get().to_dict() or {}
        # Restore attemptable status if a previous bank still exists.
        new_status = "bankReady" if quiz.get("activeQuestionBankVersionId") else "noBankGenerated"
        quiz_ref.update({"status": new_status, "updatedAt": firestore.SERVER_TIMESTAMP})


async def run_question_bank_job(job_id: str) -> None:
    job_ref = db.collection("generationJobs").document(job_id)
    job = job_ref.get().to_dict() or {}
    lesson_id = job.get("targetLessonId")
    quiz_id = job.get("targetQuizId")
    quiz_ref = db.collection("lessons").document(lesson_id).collection("quizzes").document(quiz_id)
    job_ref.update({"status": "processing", "progressPercent": 5})

    try:
        materials = [
            m.id
            for m in db.collection("lessons").document(lesson_id).collection("materials").stream()
            if (m.to_dict() or {}).get("ingestionStatus") == "embedded"
        ]
        if not materials:
            _fail_job(job_ref, quiz_ref, "No embedded materials. Upload notes/PDF and wait for ingestion.")
            return

        lesson = db.collection("lessons").document(lesson_id).get().to_dict() or {}
        lesson_tag = lesson.get("lessonTag", "unknown")
        lesson_title = lesson.get("title", lesson_tag)
        llm_model = job.get("llmModelUsed") or OLLAMA_MODEL
        total = int(job.get("targetQuestionCount") or RAG_QUESTIONS_PER_BANK)

        def on_progress(pct: int) -> None:
            job_ref.update({"progressPercent": min(85, 10 + int(pct * 0.75))})

        questions = await asyncio.to_thread(
            generate_question_bank,
            lesson_id,
            lesson_tag,
            lesson_title,
            llm_model,
            total,
            on_progress,
        )
        if not questions:
            _fail_job(job_ref, quiz_ref, "RAG produced no valid questions. Check vector store and Ollama.")
            return

        versions = list(quiz_ref.collection("questionBankVersions").stream())
        next_number = 1
        previous_active = None
        for v in versions:
            data = v.to_dict() or {}
            next_number = max(next_number, int(data.get("versionNumber") or 0) + 1)
            if data.get("status") == "active":
                previous_active = v.reference

        version_id = f"v{next_number}"
        version_ref = quiz_ref.collection("questionBankVersions").document(version_id)
        version_ref.set({
            "versionNumber": next_number,
            "status": "active",
            "totalQuestions": len(questions),
            "generatedBy": "RAG",
            "generationJobId": job_id,
            "sourceMaterialIds": materials,
            "createdAt": firestore.SERVER_TIMESTAMP,
            "archivedAt": None,
        })

        for i, q in enumerate(questions, start=1):
            version_ref.collection("questions").document(f"gen-{i}").set({
                **q,
                "createdAt": firestore.SERVER_TIMESTAMP,
            })

        if previous_active is not None:
            previous_active.update({
                "status": "archived",
                "archivedAt": firestore.SERVER_TIMESTAMP,
            })

        quiz_ref.update({
            "activeQuestionBankVersionId": version_id,
            "status": "bankReady",
            "updatedAt": firestore.SERVER_TIMESTAMP,
        })
        job_ref.update({
            "status": "completed",
            "progressPercent": 100,
            "resultVersionId": version_id,
            "completedAt": firestore.SERVER_TIMESTAMP,
        })
    except Exception as exc:
        _fail_job(job_ref, quiz_ref, str(exc))


async def run_final_quiz_job(job_id: str) -> None:
    job_ref = db.collection("generationJobs").document(job_id)
    job = job_ref.get().to_dict() or {}
    grade = int(job.get("targetGrade"))
    job_ref.update({"status": "processing", "progressPercent": 8})

    try:
        lesson_docs = list(
            db.collection("lessons")
            .where(filter=firestore.FieldFilter("grade", "==", grade))
            .where(filter=firestore.FieldFilter("status", "==", "published"))
            .stream()
        )
        lessons = [{"id": d.id, **(d.to_dict() or {})} for d in lesson_docs]
        covered = [item["id"] for item in lessons]
        if not lessons:
            job_ref.update({
                "status": "failed",
                "errorMessage": f"No published lessons for grade {grade}.",
                "completedAt": firestore.SERVER_TIMESTAMP,
            })
            return

        llm_model = job.get("llmModelUsed") or OLLAMA_MODEL

        def on_progress(pct: int) -> None:
            job_ref.update({"progressPercent": min(85, 15 + int(pct * 0.7))})

        questions = await asyncio.to_thread(
            generate_final_quiz_questions,
            lessons,
            llm_model,
            3,
            on_progress,
        )
        questions = [q for q in questions if q.get("lessonTag")]
        if not questions:
            job_ref.update({
                "status": "failed",
                "errorMessage": "RAG produced no tagged final-quiz questions.",
                "completedAt": firestore.SERVER_TIMESTAMP,
            })
            return

        previous = list(
            db.collection("finalQuizzes")
            .where(filter=firestore.FieldFilter("grade", "==", grade))
            .where(filter=firestore.FieldFilter("status", "==", "active"))
            .stream()
        )
        next_round = 1
        all_rounds = list(
            db.collection("finalQuizzes").where(filter=firestore.FieldFilter("grade", "==", grade)).stream()
        )
        for r in all_rounds:
            next_round = max(next_round, int((r.to_dict() or {}).get("roundNumber") or 0) + 1)

        final_id = f"grade{grade}-round{next_round}"
        fq_ref = db.collection("finalQuizzes").document(final_id)
        total_marks = sum(int(q.get("marks") or 0) for q in questions)
        fq_ref.set({
            "grade": grade,
            "roundNumber": next_round,
            "status": "active",
            "title": f"Grade {grade} Physics Final Quiz - Round {next_round}",
            "totalMarks": total_marks,
            "maxAttempts": 3,
            "coveredLessonIds": covered,
            "generatedBy": "RAG",
            "llmModelUsed": llm_model,
            "generationJobId": job_id,
            "createdAt": firestore.SERVER_TIMESTAMP,
            "archivedAt": None,
        })

        for i, q in enumerate(questions, start=1):
            fq_ref.collection("questions").document(f"fq-gen-{i}").set({
                **q,
                "createdAt": firestore.SERVER_TIMESTAMP,
            })

        for prev in previous:
            prev.reference.update({
                "status": "archived",
                "archivedAt": firestore.SERVER_TIMESTAMP,
            })

        job_ref.update({
            "status": "completed",
            "progressPercent": 100,
            "resultVersionId": final_id,
            "completedAt": firestore.SERVER_TIMESTAMP,
        })
    except Exception as exc:
        job_ref.update({
            "status": "failed",
            "errorMessage": str(exc)[:500],
            "completedAt": firestore.SERVER_TIMESTAMP,
        })
