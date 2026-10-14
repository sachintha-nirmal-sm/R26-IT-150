"""
Quiz attempt lifecycle: start (sanitized questions) and submit (server-side grading).

Question banks and answer keys are never returned to the client.
Attempt limits are enforced with a Firestore transaction on quizProgress/{quizId}.
"""

from __future__ import annotations

import random
import uuid
from typing import Any, Literal

from fastapi import HTTPException, status
from google.cloud import firestore

from app.core.firebase import db
from app.core.utils import normalize_answer

QuizKind = Literal["lessonQuiz", "finalQuiz"]


def _user_ref(uid: str):
    return db.collection("users").document(uid)


def quiz_ref(lesson_id: str, quiz_id: str):
    return db.collection("lessons").document(lesson_id).collection("quizzes").document(quiz_id)


def load_student_profile(uid: str) -> dict:
    snap = _user_ref(uid).get()
    if not snap.exists:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Student profile not found.")
    data = snap.to_dict() or {}
    if data.get("status") == "suspended":
        raise HTTPException(status.HTTP_403_FORBIDDEN, "Account is suspended.")
    return data


def get_quiz_or_404(lesson_id: str, quiz_id: str) -> tuple[Any, dict]:
    ref = quiz_ref(lesson_id, quiz_id)
    snap = ref.get()
    if not snap.exists:
        raise HTTPException(
            status.HTTP_404_NOT_FOUND,
            f"Quiz '{quiz_id}' was not found under lesson '{lesson_id}'.",
        )
    return ref, snap.to_dict() or {}


def get_active_bank_questions(lesson_id: str, quiz_id: str, version_id: str) -> dict[str, dict]:
    questions_ref = (
        quiz_ref(lesson_id, quiz_id)
        .collection("questionBankVersions")
        .document(version_id)
        .collection("questions")
    )
    out: dict[str, dict] = {}
    for doc in questions_ref.stream():
        out[doc.id] = doc.to_dict() or {}
    if not out:
        raise HTTPException(
            status.HTTP_409_CONFLICT,
            "The active question bank has no questions.",
        )
    return out


def sanitize_question(question_id: str, data: dict) -> dict:
    options = data.get("options")
    if isinstance(options, list):
        options = list(options)
        random.shuffle(options)
    return {
        "questionId": question_id,
        "questionText": data.get("questionText", ""),
        "questionType": data.get("questionType"),
        "options": options,
        "difficulty": data.get("difficulty"),
        "marks": data.get("marks", 0),
        "lessonTag": data.get("lessonTag"),
    }


def select_question_ids(
    all_ids: list[str],
    questions: dict[str, dict],
    used_ids: list[str],
    count: int,
) -> list[str]:
    unused = [qid for qid in all_ids if qid not in used_ids]
    pool = unused if len(unused) >= count else all_ids
    if len(pool) <= count:
        chosen = list(pool)
    else:
        chosen = random.sample(pool, count)
    random.shuffle(chosen)
    return chosen


def grade_answer(question: dict, student_answer: str | None) -> tuple[bool, int]:
    marks = int(question.get("marks") or 0)
    correct = normalize_answer(question.get("correctAnswer"))
    given = normalize_answer(student_answer)
    if not given:
        return False, 0
    if given == correct:
        return True, marks
    # Open-response Theory/Calculation: accept if the reference answer is contained
    qtype = question.get("questionType")
    if qtype in ("Theory", "Calculation") and (correct in given or given in correct) and len(given) >= 8:
        return True, marks
    return False, 0


def _load_in_progress(uid: str, progress: dict, kind: QuizKind) -> dict | None:
    attempt_id = progress.get("inProgressAttemptId")
    if not attempt_id:
        return None
    coll_name = "finalQuizAttempts" if kind == "finalQuiz" else "quizAttempts"
    snap = _user_ref(uid).collection(coll_name).document(attempt_id).get()
    if not snap.exists:
        return None
    data = snap.to_dict() or {}
    if data.get("status") != "inProgress":
        return None
    return {"attemptId": snap.id, **data}


def start_lesson_quiz(uid: str, lesson_id: str, quiz_id: str) -> dict:
    profile = load_student_profile(uid)
    lesson_snap = db.collection("lessons").document(lesson_id).get()
    if not lesson_snap.exists:
        raise HTTPException(status.HTTP_404_NOT_FOUND, f"Lesson '{lesson_id}' not found.")
    lesson = lesson_snap.to_dict() or {}
    if lesson.get("status") != "published":
        raise HTTPException(status.HTTP_403_FORBIDDEN, "This lesson is not published.")
    if lesson.get("grade") != profile.get("currentGrade"):
        raise HTTPException(status.HTTP_403_FORBIDDEN, "This lesson is not in your grade.")

    _, quiz = get_quiz_or_404(lesson_id, quiz_id)
    if quiz.get("status") != "bankReady":
        raise HTTPException(status.HTTP_409_CONFLICT, "This quiz is not ready to attempt yet.")

    version_id = quiz.get("activeQuestionBankVersionId")
    if not version_id:
        raise HTTPException(status.HTTP_409_CONFLICT, "No active question bank is assigned.")

    max_attempts = int(quiz.get("maxAttempts") or 3)
    per_attempt = int(quiz.get("questionsPerAttempt") or 20)

    progress_ref = _user_ref(uid).collection("quizProgress").document(quiz_id)
    existing = _load_in_progress(uid, progress_ref.get().to_dict() or {}, "lessonQuiz")
    if existing:
        questions = get_active_bank_questions(lesson_id, quiz_id, existing["questionBankVersionId"])
        served = existing.get("servedQuestionIds") or []
        return {
            "attemptId": existing["attemptId"],
            "quizId": quiz_id,
            "lessonId": lesson_id,
            "attemptNumber": existing.get("attemptNumber"),
            "resumed": True,
            "questions": [sanitize_question(qid, questions[qid]) for qid in served if qid in questions],
        }

    @firestore.transactional
    def _check_limit(transaction):
        snap = progress_ref.get(transaction=transaction)
        data = snap.to_dict() if snap.exists else {}
        used = int(data.get("attemptsUsed") or 0)
        if data.get("isLocked") or used >= max_attempts:
            raise HTTPException(
                status.HTTP_403_FORBIDDEN,
                f"Attempt limit reached ({max_attempts}). This quiz is locked.",
            )
        return data

    progress = _check_limit(db.transaction())
    used_ids = list(progress.get("usedQuestionIds") or [])
    questions = get_active_bank_questions(lesson_id, quiz_id, version_id)
    chosen = select_question_ids(list(questions.keys()), questions, used_ids, per_attempt)
    attempt_id = str(uuid.uuid4())
    attempt_number = int(progress.get("attemptsUsed") or 0) + 1

    attempt_doc = {
        "quizId": quiz_id,
        "lessonId": lesson_id,
        "questionBankVersionId": version_id,
        "attemptNumber": attempt_number,
        "servedQuestionIds": chosen,
        "answers": [],
        "score": 0,
        "totalMarks": 0,
        "timeTakenSeconds": 0,
        "startedAt": firestore.SERVER_TIMESTAMP,
        "submittedAt": None,
        "status": "inProgress",
    }
    _user_ref(uid).collection("quizAttempts").document(attempt_id).set(attempt_doc)
    progress_ref.set(
        {
            "quizId": quiz_id,
            "lessonId": lesson_id,
            "inProgressAttemptId": attempt_id,
            "attemptsUsed": progress.get("attemptsUsed") or 0,
            "isLocked": False,
            "bestScore": progress.get("bestScore") or 0,
            "bestAttemptId": progress.get("bestAttemptId"),
            "usedQuestionIds": used_ids,
        },
        merge=True,
    )

    return {
        "attemptId": attempt_id,
        "quizId": quiz_id,
        "lessonId": lesson_id,
        "attemptNumber": attempt_number,
        "resumed": False,
        "questions": [sanitize_question(qid, questions[qid]) for qid in chosen],
    }


def submit_lesson_quiz(
    uid: str,
    lesson_id: str,
    quiz_id: str,
    attempt_id: str,
    answers: list[dict],
    time_taken_seconds: int = 0,
) -> dict:
    attempt_ref = _user_ref(uid).collection("quizAttempts").document(attempt_id)
    attempt_snap = attempt_ref.get()
    if not attempt_snap.exists:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Attempt not found.")
    attempt = attempt_snap.to_dict() or {}
    if attempt.get("status") != "inProgress":
        raise HTTPException(status.HTTP_409_CONFLICT, "This attempt has already been submitted.")
    if attempt.get("quizId") != quiz_id:
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "Attempt does not belong to this quiz.")

    version_id = attempt["questionBankVersionId"]
    questions = get_active_bank_questions(lesson_id, quiz_id, version_id)
    served = attempt.get("servedQuestionIds") or []
    answer_map = {a.get("questionId"): a.get("studentAnswer") for a in answers}

    graded = []
    score = 0
    total_marks = 0
    for qid in served:
        q = questions.get(qid)
        if not q:
            continue
        student_answer = answer_map.get(qid)
        is_correct, awarded = grade_answer(q, student_answer)
        marks = int(q.get("marks") or 0)
        total_marks += marks
        score += awarded
        graded.append({
            "questionId": qid,
            "studentAnswer": student_answer,
            "isCorrect": is_correct,
            "marksAwarded": awarded,
            "lessonTag": q.get("lessonTag"),
            "difficulty": q.get("difficulty"),
            "questionType": q.get("questionType"),
        })

    _, quiz = get_quiz_or_404(lesson_id, quiz_id)
    max_attempts = int(quiz.get("maxAttempts") or 3)
    progress_ref = _user_ref(uid).collection("quizProgress").document(quiz_id)

    @firestore.transactional
    def _commit(transaction):
        snap = progress_ref.get(transaction=transaction)
        data = snap.to_dict() if snap.exists else {
            "quizId": quiz_id,
            "lessonId": lesson_id,
            "attemptsUsed": 0,
            "isLocked": False,
            "bestScore": 0,
            "bestAttemptId": None,
            "usedQuestionIds": [],
        }
        used = int(data.get("attemptsUsed") or 0) + 1
        used_ids = list(dict.fromkeys((data.get("usedQuestionIds") or []) + served))
        best = int(data.get("bestScore") or 0)
        best_id = data.get("bestAttemptId")
        if score >= best:
            best = score
            best_id = attempt_id
        locked = used >= max_attempts
        transaction.set(progress_ref, {
            "quizId": quiz_id,
            "lessonId": lesson_id,
            "attemptsUsed": used,
            "isLocked": locked,
            "bestScore": best,
            "bestAttemptId": best_id,
            "usedQuestionIds": used_ids,
            "inProgressAttemptId": None,
            "lastAttemptAt": firestore.SERVER_TIMESTAMP,
        })
        transaction.update(attempt_ref, {
            "answers": graded,
            "score": score,
            "totalMarks": total_marks,
            "timeTakenSeconds": time_taken_seconds,
            "submittedAt": firestore.SERVER_TIMESTAMP,
            "status": "completed",
        })
        return used, locked, best

    attempts_used, is_locked, best_score = _commit(db.transaction())

    percent = round((score / total_marks) * 100, 2) if total_marks else 0.0
    return {
        "attemptId": attempt_id,
        "quizId": quiz_id,
        "lessonId": lesson_id,
        "score": score,
        "totalMarks": total_marks,
        "scorePercent": percent,
        "answers": graded,
        "questionBankVersionId": version_id,
        "attemptsUsed": attempts_used,
        "isLocked": is_locked,
        "bestScore": best_score,
        "quizKind": "lessonQuiz",
    }


def get_active_final_quiz(grade: int) -> tuple[str, dict]:
    query = (
        db.collection("finalQuizzes")
        .where(filter=firestore.FieldFilter("grade", "==", grade))
        .where(filter=firestore.FieldFilter("status", "==", "active"))
        .limit(1)
    )
    docs = list(query.stream())
    if not docs:
        raise HTTPException(
            status.HTTP_404_NOT_FOUND,
            f"No active final quiz for grade {grade}.",
        )
    return docs[0].id, docs[0].to_dict() or {}


def get_final_questions(final_quiz_id: str) -> dict[str, dict]:
    out: dict[str, dict] = {}
    for doc in db.collection("finalQuizzes").document(final_quiz_id).collection("questions").stream():
        out[doc.id] = doc.to_dict() or {}
    if not out:
        raise HTTPException(status.HTTP_409_CONFLICT, "The final quiz has no questions.")
    return out


def start_final_quiz(uid: str) -> dict:
    profile = load_student_profile(uid)
    grade = profile.get("currentGrade")
    if grade is None:
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "Student grade is not set.")

    final_quiz_id, quiz = get_active_final_quiz(int(grade))
    max_attempts = int(quiz.get("maxAttempts") or 3)

    progress_ref = _user_ref(uid).collection("quizProgress").document(final_quiz_id)
    existing = _load_in_progress(uid, progress_ref.get().to_dict() or {}, "finalQuiz")
    questions = get_final_questions(final_quiz_id)
    if existing:
        served = existing.get("servedQuestionIds") or list(questions.keys())
        return {
            "attemptId": existing["attemptId"],
            "finalQuizId": final_quiz_id,
            "roundNumber": existing.get("roundNumber"),
            "resumed": True,
            "questions": [sanitize_question(qid, questions[qid]) for qid in served if qid in questions],
        }

    @firestore.transactional
    def _check(transaction):
        snap = progress_ref.get(transaction=transaction)
        data = snap.to_dict() if snap.exists else {}
        used = int(data.get("attemptsUsed") or 0)
        if data.get("isLocked") or used >= max_attempts:
            raise HTTPException(
                status.HTTP_403_FORBIDDEN,
                f"Final quiz attempt limit reached ({max_attempts}).",
            )
        return data

    progress = _check(db.transaction())
    served = list(questions.keys())
    random.shuffle(served)
    attempt_id = str(uuid.uuid4())
    attempt_number = int(progress.get("attemptsUsed") or 0) + 1

    _user_ref(uid).collection("finalQuizAttempts").document(attempt_id).set({
        "finalQuizId": final_quiz_id,
        "roundNumber": quiz.get("roundNumber"),
        "attemptNumber": attempt_number,
        "servedQuestionIds": served,
        "answers": [],
        "answerCheckingModel": "rule-based",
        "score": 0,
        "totalMarks": 0,
        "timeTakenSeconds": 0,
        "startedAt": firestore.SERVER_TIMESTAMP,
        "submittedAt": None,
        "status": "inProgress",
    })
    progress_ref.set(
        {
            "quizId": final_quiz_id,
            "inProgressAttemptId": attempt_id,
            "attemptsUsed": progress.get("attemptsUsed") or 0,
            "isLocked": False,
            "bestScore": progress.get("bestScore") or 0,
            "bestAttemptId": progress.get("bestAttemptId"),
            "usedQuestionIds": progress.get("usedQuestionIds") or [],
        },
        merge=True,
    )

    return {
        "attemptId": attempt_id,
        "finalQuizId": final_quiz_id,
        "roundNumber": quiz.get("roundNumber"),
        "attemptNumber": attempt_number,
        "resumed": False,
        "questions": [sanitize_question(qid, questions[qid]) for qid in served],
    }


def submit_final_quiz(uid: str, attempt_id: str, answers: list[dict], time_taken_seconds: int = 0) -> dict:
    attempt_ref = _user_ref(uid).collection("finalQuizAttempts").document(attempt_id)
    snap = attempt_ref.get()
    if not snap.exists:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Final quiz attempt not found.")
    attempt = snap.to_dict() or {}
    if attempt.get("status") != "inProgress":
        raise HTTPException(status.HTTP_409_CONFLICT, "This attempt has already been submitted.")

    final_quiz_id = attempt["finalQuizId"]
    fq_snap = db.collection("finalQuizzes").document(final_quiz_id).get()
    quiz = fq_snap.to_dict() or {}
    max_attempts = int(quiz.get("maxAttempts") or 3)
    questions = get_final_questions(final_quiz_id)
    served = attempt.get("servedQuestionIds") or list(questions.keys())
    answer_map = {a.get("questionId"): a.get("studentAnswer") for a in answers}

    graded = []
    score = 0
    total_marks = 0
    for qid in served:
        q = questions.get(qid)
        if not q:
            continue
        student_answer = answer_map.get(qid)
        is_correct, awarded = grade_answer(q, student_answer)
        marks = int(q.get("marks") or 0)
        total_marks += marks
        score += awarded
        graded.append({
            "questionId": qid,
            "studentAnswer": student_answer,
            "isCorrect": is_correct,
            "marksAwarded": awarded,
            "lessonTag": q.get("lessonTag"),
            "difficulty": q.get("difficulty"),
            "questionType": q.get("questionType"),
            "sourceLessonId": q.get("sourceLessonId"),
        })

    progress_ref = _user_ref(uid).collection("quizProgress").document(final_quiz_id)

    @firestore.transactional
    def _commit(transaction):
        psnap = progress_ref.get(transaction=transaction)
        data = psnap.to_dict() if psnap.exists else {
            "quizId": final_quiz_id,
            "lessonId": None,
            "attemptsUsed": 0,
            "isLocked": False,
            "bestScore": 0,
            "bestAttemptId": None,
            "usedQuestionIds": [],
        }
        used = int(data.get("attemptsUsed") or 0) + 1
        best = int(data.get("bestScore") or 0)
        best_id = data.get("bestAttemptId")
        if score >= best:
            best = score
            best_id = attempt_id
        transaction.set(progress_ref, {
            **data,
            "quizId": final_quiz_id,
            "attemptsUsed": used,
            "isLocked": used >= max_attempts,
            "bestScore": best,
            "bestAttemptId": best_id,
            "usedQuestionIds": list(dict.fromkeys((data.get("usedQuestionIds") or []) + served)),
            "inProgressAttemptId": None,
            "lastAttemptAt": firestore.SERVER_TIMESTAMP,
        })
        transaction.update(attempt_ref, {
            "answers": graded,
            "score": score,
            "totalMarks": total_marks,
            "timeTakenSeconds": time_taken_seconds,
            "submittedAt": firestore.SERVER_TIMESTAMP,
            "status": "completed",
            "answerCheckingModel": "rule-based",
        })
        return used, used >= max_attempts, best

    attempts_used, is_locked, best_score = _commit(db.transaction())
    percent = round((score / total_marks) * 100, 2) if total_marks else 0.0
    return {
        "attemptId": attempt_id,
        "finalQuizId": final_quiz_id,
        "roundNumber": attempt.get("roundNumber"),
        "score": score,
        "totalMarks": total_marks,
        "scorePercent": percent,
        "answers": graded,
        "attemptsUsed": attempts_used,
        "isLocked": is_locked,
        "bestScore": best_score,
        "quizKind": "finalQuiz",
        "lessonId": None,
        "quizId": final_quiz_id,
        "questionBankVersionId": final_quiz_id,
    }
