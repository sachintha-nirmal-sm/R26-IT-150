"""Post-submission Firestore writes: wrongQuestions, weakTopics, performanceSummary, analytics."""

from __future__ import annotations

from google.cloud import firestore

from app.core.firebase import db
from app.core.utils import QUESTION_TYPES, WEAKNESS_THRESHOLD, empty_type_bucket, weakness_score
from app.services.feedback_service import build_feedback
from app.services.learning_path_service import maybe_write_youtube_recommendation

ANALYTICS_ROOT = db.collection("analytics").document("global")


def _user_ref(uid: str):
    return db.collection("users").document(uid)


def _question_snapshot(submit_result: dict, answer: dict) -> dict:
    """Load source question text/answer for wrongQuestions snapshotting."""
    qid = answer.get("questionId")
    kind = submit_result.get("quizKind")
    if kind == "finalQuiz":
        ref = (
            db.collection("finalQuizzes")
            .document(submit_result["finalQuizId"])
            .collection("questions")
            .document(qid)
        )
    else:
        ref = (
            db.collection("lessons")
            .document(submit_result["lessonId"])
            .collection("quizzes")
            .document(submit_result["quizId"])
            .collection("questionBankVersions")
            .document(submit_result["questionBankVersionId"])
            .collection("questions")
            .document(qid)
        )
    snap = ref.get()
    return snap.to_dict() or {}


def _upsert_weak_topic(uid: str, lesson_tag: str, lesson_id: str | None, answers_for_tag: list[dict], quiz_kind: str):
    ref = _user_ref(uid).collection("weakTopics").document(lesson_tag)
    snap = ref.get()
    data = snap.to_dict() if snap.exists else {
        "lessonTag": lesson_tag,
        "lessonId": lesson_id,
        "incorrectCount": 0,
        "totalAttempted": 0,
        "weaknessScore": 0.0,
        "byQuestionType": empty_type_bucket(),
        "contributingQuizTypes": [],
    }
    by_type = data.get("byQuestionType") or empty_type_bucket()
    for t in QUESTION_TYPES:
        by_type.setdefault(t, {"incorrectCount": 0, "totalAttempted": 0, "weaknessScore": 0.0})

    for ans in answers_for_tag:
        qtype = ans.get("questionType") or "Theory"
        if qtype not in by_type:
            by_type[qtype] = {"incorrectCount": 0, "totalAttempted": 0, "weaknessScore": 0.0}
        by_type[qtype]["totalAttempted"] += 1
        data["totalAttempted"] = int(data.get("totalAttempted") or 0) + 1
        if not ans.get("isCorrect"):
            by_type[qtype]["incorrectCount"] += 1
            data["incorrectCount"] = int(data.get("incorrectCount") or 0) + 1

    for t, bucket in by_type.items():
        bucket["weaknessScore"] = weakness_score(bucket["incorrectCount"], bucket["totalAttempted"])

    data["byQuestionType"] = by_type
    data["weaknessScore"] = weakness_score(int(data["incorrectCount"]), int(data["totalAttempted"]))
    data["lessonId"] = data.get("lessonId") or lesson_id
    contrib = list(data.get("contributingQuizTypes") or [])
    if quiz_kind not in contrib:
        contrib.append(quiz_kind)
    data["contributingQuizTypes"] = contrib
    data["lastUpdated"] = firestore.SERVER_TIMESTAMP
    ref.set(data)
    return data


def _update_performance_summary(uid: str, answers: list[dict]):
    ref = _user_ref(uid).collection("performanceSummary").document("summary")
    snap = ref.get()
    data = snap.to_dict() if snap.exists else {
        "byQuestionType": empty_type_bucket(),
        "overallAccuracy": 0.0,
    }
    by_type = data.get("byQuestionType") or empty_type_bucket()
    for t in QUESTION_TYPES:
        by_type.setdefault(t, {"incorrectCount": 0, "totalAttempted": 0, "weaknessScore": 0.0})

    correct = 0
    total = 0
    # Include historical totals already stored
    hist_total = sum(int(b.get("totalAttempted") or 0) for b in by_type.values())
    hist_incorrect = sum(int(b.get("incorrectCount") or 0) for b in by_type.values())

    for ans in answers:
        qtype = ans.get("questionType") or "Theory"
        if qtype not in by_type:
            by_type[qtype] = {"incorrectCount": 0, "totalAttempted": 0, "weaknessScore": 0.0}
        by_type[qtype]["totalAttempted"] += 1
        total += 1
        if ans.get("isCorrect"):
            correct += 1
        else:
            by_type[qtype]["incorrectCount"] += 1

    for bucket in by_type.values():
        bucket["weaknessScore"] = weakness_score(bucket["incorrectCount"], bucket["totalAttempted"])

    new_total = hist_total + total
    new_correct = (hist_total - hist_incorrect) + correct
    data["byQuestionType"] = by_type
    data["overallAccuracy"] = round(new_correct / new_total, 4) if new_total else 0.0
    data["lastUpdated"] = firestore.SERVER_TIMESTAMP
    ref.set(data)
    return data


def _bump_analytics(submit_result: dict, answers: list[dict]):
    percent = float(submit_result.get("scorePercent") or 0)
    lesson_id = submit_result.get("lessonId")
    quiz_id = submit_result.get("quizId")

    if lesson_id:
        ref = ANALYTICS_ROOT.collection("lessonStats").document(lesson_id)
        snap = ref.get()
        data = snap.to_dict() if snap.exists else {
            "lessonId": lesson_id,
            "totalAttempts": 0,
            "averageScorePercent": 0.0,
            "weakStudentCount": 0,
            "mostMissedQuestionIds": [],
        }
        n = int(data.get("totalAttempts") or 0)
        avg = float(data.get("averageScorePercent") or 0)
        data["averageScorePercent"] = round(((avg * n) + percent) / (n + 1), 2)
        data["totalAttempts"] = n + 1
        data["lastUpdated"] = firestore.SERVER_TIMESTAMP
        ref.set(data)

    if quiz_id:
        ref = ANALYTICS_ROOT.collection("quizStats").document(quiz_id)
        snap = ref.get()
        data = snap.to_dict() if snap.exists else {
            "totalAttempts": 0,
            "averageScorePercent": 0.0,
        }
        n = int(data.get("totalAttempts") or 0)
        avg = float(data.get("averageScorePercent") or 0)
        data["averageScorePercent"] = round(((avg * n) + percent) / (n + 1), 2)
        data["totalAttempts"] = n + 1
        data["lastUpdated"] = firestore.SERVER_TIMESTAMP
        ref.set(data)

    for ans in answers:
        qid = ans.get("questionId")
        if not qid:
            continue
        ref = ANALYTICS_ROOT.collection("questionStats").document(qid)
        snap = ref.get()
        data = snap.to_dict() if snap.exists else {
            "totalAnswered": 0,
            "incorrectRate": 0.0,
        }
        n = int(data.get("totalAnswered") or 0)
        prev_incorrect = float(data.get("incorrectRate") or 0) * n
        n += 1
        if not ans.get("isCorrect"):
            prev_incorrect += 1
        data["totalAnswered"] = n
        data["incorrectRate"] = round(prev_incorrect / n, 4)
        data["lastUpdated"] = firestore.SERVER_TIMESTAMP
        ref.set(data)


def process_submission(uid: str, submit_result: dict) -> dict:
    answers = submit_result.get("answers") or []
    quiz_kind = submit_result.get("quizKind") or "lessonQuiz"
    attempt_id = submit_result["attemptId"]
    quiz_id = submit_result.get("quizId")
    lesson_id = submit_result.get("lessonId")
    version_id = submit_result.get("questionBankVersionId")

    # wrongQuestions snapshots
    for ans in answers:
        if ans.get("isCorrect"):
            continue
        source = _question_snapshot(submit_result, ans)
        _user_ref(uid).collection("wrongQuestions").document().set({
            "questionId": ans.get("questionId"),
            "lessonId": lesson_id or ans.get("sourceLessonId"),
            "quizId": quiz_id,
            "attemptId": attempt_id,
            "questionBankVersionId": version_id,
            "questionText": source.get("questionText", ""),
            "studentAnswer": ans.get("studentAnswer"),
            "correctAnswer": source.get("correctAnswer"),
            "explanation": source.get("explanation"),
            "questionType": ans.get("questionType"),
            "lessonTag": ans.get("lessonTag"),
            "difficulty": ans.get("difficulty"),
            "reviewed": False,
            "createdAt": firestore.SERVER_TIMESTAMP,
        })

    # weakTopics per lessonTag
    by_tag: dict[str, list[dict]] = {}
    for ans in answers:
        tag = ans.get("lessonTag")
        if not tag:
            continue
        by_tag.setdefault(tag, []).append(ans)

    weak_docs = []
    for tag, group in by_tag.items():
        lid = lesson_id
        if not lid:
            lid = next((a.get("sourceLessonId") for a in group if a.get("sourceLessonId")), None)
        doc = _upsert_weak_topic(uid, tag, lid, group, quiz_kind)
        weak_docs.append(doc)
        if float(doc.get("weaknessScore") or 0) >= WEAKNESS_THRESHOLD:
            maybe_write_youtube_recommendation(uid, tag)

    summary = _update_performance_summary(uid, answers)
    _bump_analytics(submit_result, answers)

    feedback_payload = build_feedback(submit_result)
    feedback_ref = _user_ref(uid).collection("feedback").document()
    feedback_ref.set({
        "attemptId": attempt_id,
        "quizId": quiz_id,
        "feedbackText": feedback_payload["feedbackText"],
        "strengths": feedback_payload["strengths"],
        "weaknesses": feedback_payload["weaknesses"],
        "recommendedTopics": feedback_payload["recommendedTopics"],
        "llmModelUsed": feedback_payload["llmModelUsed"],
        "generatedAt": firestore.SERVER_TIMESTAMP,
    })

    learning_path = {
        "primaryWeakness": feedback_payload.get("primaryWeakness"),
        "recommendedTopics": feedback_payload.get("recommendedTopics"),
        "byQuestionType": summary.get("byQuestionType"),
    }

    return {
        "feedbackId": feedback_ref.id,
        "feedback": feedback_payload,
        "weakTopics": [
            {
                "lessonTag": d.get("lessonTag"),
                "weaknessScore": d.get("weaknessScore"),
                "byQuestionType": d.get("byQuestionType"),
            }
            for d in weak_docs
        ],
        "performanceSummary": {
            "overallAccuracy": summary.get("overallAccuracy"),
            "byQuestionType": summary.get("byQuestionType"),
        },
        "learningPath": learning_path,
    }


def list_admin_stats(kind: str) -> list[dict]:
    coll = ANALYTICS_ROOT.collection(kind)
    rows = []
    for doc in coll.stream():
        data = doc.to_dict() or {}
        data["id"] = doc.id
        rows.append(data)
    return rows
