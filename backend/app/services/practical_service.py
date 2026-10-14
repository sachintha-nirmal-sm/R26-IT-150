"""
practical_service.py — Game-Based Measurement practicals business logic.

Enforces the Word-doc attempt model server-side:
  Try Demo        — no timer, optional, attempts from practicals.demoMaxAttempts
  Start Practical — timed, does not require demo, attempts from practicals.practicalMaxAttempts

Client values for attempt availability, max attempts, and elapsed time are never trusted.
All official writes go through the Firebase Admin SDK.
"""

from datetime import datetime, timezone
from uuid import uuid4

from fastapi import HTTPException, status
from google.cloud import firestore

from app.core.firebase import db
from app.models.practical import (
    CompletePracticalRequest,
    DemoFinishRequest,
    OfficialResultBundle,
    PracticalDetail,
    PracticalResultResponse,
    PracticalSummary,
    RecentPracticalItem,
    SessionResponse,
    StudentProgressResponse,
    SubmitPracticalRequest,
)
from app.services.practical_catalogue import CATALOGUE, ensure_catalogue, ensure_practical

TIMER_GRACE_SECONDS = 15
DENSITY_PRACTICAL_ID = "grade9_density_water"
FORCE_PRACTICAL_ID = "grade9_force_basic"
PRESSURE_PRACTICAL_ID = "grade9_pressure_solid"
ALWAYS_AVAILABLE_PRACTICAL_IDS = tuple(CATALOGUE.keys())

STATE_AVAILABLE = "AVAILABLE"
STATE_DEMO_IN_PROGRESS = "DEMO_IN_PROGRESS"
STATE_DEMO_COMPLETED = "DEMO_COMPLETED"
STATE_PRACTICAL_AVAILABLE = "PRACTICAL_AVAILABLE"
STATE_PRACTICAL_IN_PROGRESS = "PRACTICAL_IN_PROGRESS"
STATE_SUBMITTED = "SUBMITTED"
STATE_TIME_EXPIRED = "TIME_EXPIRED"


def _now() -> datetime:
    return datetime.now(timezone.utc)


def _iso(value) -> str | None:
    if value is None:
        return None
    if hasattr(value, "isoformat"):
        return value.isoformat()
    return str(value)


def _percentage(score: int, max_score: int) -> float:
    if max_score <= 0:
        return 0.0
    return round(100.0 * float(score) / float(max_score), 2)


def _configured_max_score(practical: dict) -> int:
    configured = int(practical.get("maxScore", 100) or 100)
    if practical.get("id") in ALWAYS_AVAILABLE_PRACTICAL_IDS:
        return 100
    return configured if configured > 0 else 100


def _official_max_attempts(practical: dict) -> int:
    configured = int(practical.get("practicalMaxAttempts", 3) or 3)
    if practical.get("id") in ALWAYS_AVAILABLE_PRACTICAL_IDS:
        return max(configured, 3)
    return configured


def _time_limit_seconds(practical: dict) -> int:
    configured = int(practical.get("durationSeconds", 600) or 600)
    if configured <= 0:
        return 600
    return configured


def _sp_id(uid: str, practical_id: str) -> str:
    return f"{uid}_{practical_id}"


def _parse_grade(raw) -> int | None:
    if raw is None or isinstance(raw, bool):
        return None
    if isinstance(raw, (int, float)):
        grade = int(raw)
        return grade if grade in (9, 10, 11) else None
    text = str(raw).lower().replace("grade", "").strip()
    try:
        grade = int(text)
    except ValueError:
        return None
    return grade if grade in (9, 10, 11) else None


def _require_active_student(uid: str) -> dict:
    snap = db.collection("users").document(uid).get()
    data = snap.to_dict() or {}
    if not snap.exists:
        data = {
            "role": "student",
            "status": "active",
            "currentGrade": 10,
            "fullName": "",
            "email": "",
        }
        db.collection("users").document(uid).set(data, merge=True)
    if data.get("status") == "suspended":
        raise HTTPException(status.HTTP_403_FORBIDDEN, "Your account has been suspended.")
    if data.get("role") not in (None, "", "student"):
        raise HTTPException(status.HTTP_403_FORBIDDEN, "Student access required.")
    data["role"] = "student"
    grade = _parse_grade(data.get("currentGrade")) or _parse_grade(data.get("grade"))
    if grade is None:
        grade = 10
        db.collection("users").document(uid).set({"currentGrade": 10, "role": "student"}, merge=True)
    data["currentGrade"] = grade
    return data


def _get_practical(practical_id: str) -> dict:
    data = ensure_practical(db, practical_id)
    if not data:
        raise HTTPException(status.HTTP_404_NOT_FOUND, f"Practical '{practical_id}' not found.")
    return _with_canonical_mapping(data)


def _with_canonical_mapping(data: dict) -> dict:
    spec = CATALOGUE.get(str(data.get("id") or ""))
    if not spec:
        return data
    merged = dict(data)
    for field in ("unitySceneId", "lessonId", "topicId", "title"):
        if spec.get(field):
            merged[field] = spec[field]
    return merged


def _assert_grade_allowed(student: dict, practical: dict) -> None:
    if not practical.get("isActive", False):
        raise HTTPException(status.HTTP_404_NOT_FOUND, "This practical is not active.")
    if int(practical.get("grade")) != int(student.get("currentGrade")):
        raise HTTPException(
            status.HTTP_403_FORBIDDEN,
            "This practical is not available for your grade.",
        )


def _default_student_practical(uid: str, practical: dict) -> dict:
    return {
        "studentId": uid,
        "practicalId": practical["id"],
        "grade": practical["grade"],
        "demoAttemptsUsed": 0,
        "practicalAttemptsUsed": 0,
        "demoCompleted": False,
        "bestScore": 0,
        "latestScore": 0,
        "percentage": 0,
        "completed": False,
        "currentState": STATE_AVAILABLE,
        "activeStartedAt": None,
        "activeAttemptType": None,
        "activeAttemptNumber": None,
        "activeResultId": None,
        "lastAttemptAt": None,
    }


def _get_or_create_student_practical(uid: str, practical: dict) -> tuple[str, dict]:
    doc_id = _sp_id(uid, practical["id"])
    ref = db.collection("studentPracticals").document(doc_id)
    snap = ref.get()
    if snap.exists:
        data = snap.to_dict() or {}
        data.setdefault("currentState", STATE_AVAILABLE)
        return doc_id, data
    data = _default_student_practical(uid, practical)
    ref.set(data)
    return doc_id, data


def _to_summary(practical: dict) -> PracticalSummary:
    practical = _with_canonical_mapping(practical)
    return PracticalSummary(
        id=practical["id"],
        title=practical.get("title", ""),
        grade=int(practical.get("grade", 0)),
        lessonId=practical.get("lessonId", ""),
        topicId=practical.get("topicId", ""),
        description=practical.get("description", ""),
        unitySceneId=practical.get("unitySceneId", ""),
        unityBuildUrl=practical.get("unityBuildUrl", ""),
        maxScore=_configured_max_score(practical),
        durationSeconds=_time_limit_seconds(practical),
        demoAllowed=bool(practical.get("demoAllowed", True)),
        demoMaxAttempts=int(practical.get("demoMaxAttempts", 1)),
        practicalMaxAttempts=int(practical.get("practicalMaxAttempts", 1)),
        isActive=bool(practical.get("isActive", False)),
        order=int(practical.get("order", 0)),
    )


def _to_result(result_id: str, data: dict, current_state: str | None = None) -> PracticalResultResponse:
    return PracticalResultResponse(
        resultId=result_id,
        studentId=data.get("studentId", ""),
        practicalId=data.get("practicalId", ""),
        grade=int(data.get("grade", 0)),
        attemptType=data.get("attemptType", "practical"),
        attemptNumber=int(data.get("attemptNumber", 1)),
        score=int(data.get("score", 0)),
        maxScore=int(data.get("maxScore", 0)),
        percentage=float(data.get("percentage", 0)),
        startedAt=_iso(data.get("startedAt")),
        completedAt=_iso(data.get("completedAt")),
        durationSeconds=data.get("durationSeconds"),
        status=data.get("status", "completed"),
        measurements=data.get("measurements"),
        calculations=data.get("calculations"),
        evaluation=data.get("evaluation"),
        currentState=current_state,
    )


def list_practicals(uid: str, grade: int | None, lesson_id: str | None = None) -> list[PracticalSummary]:
    ensure_catalogue(db)
    student = _require_active_student(uid)
    student_grade = int(student["currentGrade"])
    if grade is not None and int(grade) != student_grade:
        raise HTTPException(
            status.HTTP_403_FORBIDDEN,
            "You may only list practicals for your own grade.",
        )

    query = db.collection("practicals").where(
        filter=firestore.FieldFilter("grade", "==", student_grade)
    )
    items = []
    seen = set()
    for snap in query.stream():
        data = snap.to_dict() or {}
        if not data.get("isActive", False):
            continue
        data["id"] = snap.id
        items.append(_to_summary(data))
        seen.add(snap.id)

    for extra_id, spec in CATALOGUE.items():
        if extra_id in seen:
            continue
        if int(spec.get("grade", 0)) != student_grade:
            continue
        extra_snap = db.collection("practicals").document(extra_id).get()
        if extra_snap.exists:
            extra = extra_snap.to_dict() or {}
            extra["id"] = extra_id
            extra_grade = extra.get("grade", spec.get("grade"))
            if extra.get("isActive", False) is False:
                continue
            if int(extra_grade) != student_grade:
                continue
            items.append(_to_summary(extra))
        else:
            extra = dict(spec)
            extra["id"] = extra_id
            items.append(_to_summary(extra))
        seen.add(extra_id)

    items.sort(
        key=lambda item: (0 if item.id in ALWAYS_AVAILABLE_PRACTICAL_IDS else 1, item.order)
    )
    if lesson_id:
        items = [item for item in items if item.lessonId == lesson_id]
    return items


def get_practical(uid: str, practical_id: str) -> PracticalDetail:
    student = _require_active_student(uid)
    practical = _get_practical(practical_id)
    _assert_grade_allowed(student, practical)
    _, progress = _get_or_create_student_practical(uid, practical)
    summary = _to_summary(practical)
    return PracticalDetail(
        **summary.model_dump(),
        currentState=progress.get("currentState", STATE_AVAILABLE),
        demoAttemptsUsed=int(progress.get("demoAttemptsUsed", 0)),
        practicalAttemptsUsed=int(progress.get("practicalAttemptsUsed", 0)),
        demoCompleted=bool(progress.get("demoCompleted", False)),
        completed=bool(progress.get("completed", False)),
        bestScore=int(progress.get("bestScore", 0)),
        latestScore=int(progress.get("latestScore", 0)),
        percentage=float(progress.get("percentage", 0)),
    )


def start_demo(uid: str, practical_id: str) -> SessionResponse:
    student = _require_active_student(uid)
    practical = _get_practical(practical_id)
    _assert_grade_allowed(student, practical)

    if not practical.get("demoAllowed", True):
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "Demo is not allowed for this practical.")

    demo_max = int(practical.get("demoMaxAttempts", 1))
    sp_ref = db.collection("studentPracticals").document(_sp_id(uid, practical_id))
    result_id = str(uuid4())
    started = _now()

    @firestore.transactional
    def _tx(transaction: firestore.Transaction) -> dict:
        snap = sp_ref.get(transaction=transaction)
        progress = snap.to_dict() if snap.exists else _default_student_practical(uid, practical)
        used = int(progress.get("demoAttemptsUsed", 0))
        state = progress.get("currentState", STATE_AVAILABLE)
        active_result_id = progress.get("activeResultId")
        if state == STATE_DEMO_IN_PROGRESS and active_result_id:
            return {
                "resultId": active_result_id,
                "attemptNumber": int(progress.get("activeAttemptNumber") or used or 1),
                "startedAt": progress.get("activeStartedAt") or started,
                "resume": True,
            }
        if state == STATE_PRACTICAL_IN_PROGRESS and active_result_id:
            raise HTTPException(
                status.HTTP_409_CONFLICT,
                "Finish the official practical before starting a new trial.",
            )
        if used >= demo_max and practical.get("id") not in CATALOGUE:
            raise HTTPException(
                status.HTTP_409_CONFLICT,
                "Demo attempt limit reached. Attempt limits come from practical configuration.",
            )

        result_ref = db.collection("practicalResults").document(result_id)
        transaction.set(result_ref, {
            "studentId": uid,
            "practicalId": practical_id,
            "lessonId": practical.get("lessonId", ""),
            "grade": practical["grade"],
            "attemptType": "demo",
            "mode": "trial",
            "attemptNumber": used + 1,
            "score": 0,
            "maxScore": _configured_max_score(practical),
            "percentage": 0,
            "startedAt": started,
            "completedAt": None,
            "durationSeconds": None,
            "status": "inProgress",
            "measurements": None,
            "calculations": None,
            "evaluation": None,
        })
        transaction.set(sp_ref, {
            **progress,
            "currentState": STATE_DEMO_IN_PROGRESS,
            "activeStartedAt": started,
            "activeAttemptType": "demo",
            "activeAttemptNumber": used + 1,
            "activeResultId": result_id,
        }, merge=True)
        return {
            "resultId": result_id,
            "attemptNumber": used + 1,
            "startedAt": started,
            "resume": False,
        }

    info = _tx(db.transaction())
    return SessionResponse(
        practicalId=practical_id,
        resultId=info["resultId"],
        mode="demo",
        attemptNumber=info["attemptNumber"],
        currentState=STATE_DEMO_IN_PROGRESS,
        durationSeconds=_time_limit_seconds(practical),
        unitySceneId=practical.get("unitySceneId", ""),
        unityBuildUrl=practical.get("unityBuildUrl", ""),
        startedAt=_iso(info["startedAt"]),
    )


def finish_demo(uid: str, practical_id: str, body: DemoFinishRequest) -> PracticalResultResponse:
    student = _require_active_student(uid)
    practical = _get_practical(practical_id)
    _assert_grade_allowed(student, practical)

    sp_ref = db.collection("studentPracticals").document(_sp_id(uid, practical_id))
    result_ref = db.collection("practicalResults").document(body.resultId)
    completed = _now()
    max_score = _configured_max_score(practical)
    score = 0 if body.score is None else int(body.score)
    if score < 0 or score > max_score:
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "score must be between 0 and maxScore.")

    @firestore.transactional
    def _tx(transaction: firestore.Transaction) -> dict:
        sp_snap = sp_ref.get(transaction=transaction)
        result_snap = result_ref.get(transaction=transaction)
        if not sp_snap.exists or not result_snap.exists:
            raise HTTPException(status.HTTP_404_NOT_FOUND, "Demo session not found.")
        progress = sp_snap.to_dict() or {}
        result = result_snap.to_dict() or {}
        if progress.get("currentState") != STATE_DEMO_IN_PROGRESS:
            raise HTTPException(status.HTTP_409_CONFLICT, "No demo session is in progress.")
        if progress.get("activeResultId") != body.resultId:
            raise HTTPException(status.HTTP_409_CONFLICT, "resultId does not match the active demo session.")
        if result.get("studentId") != uid or result.get("practicalId") != practical_id:
            raise HTTPException(status.HTTP_403_FORBIDDEN, "This demo result does not belong to you.")
        if result.get("attemptType") != "demo":
            raise HTTPException(status.HTTP_400_BAD_REQUEST, "This session is not a demo attempt.")

        started = result.get("startedAt")
        elapsed = None
        if started is not None:
            started_dt = started if isinstance(started, datetime) else completed
            if getattr(started_dt, "tzinfo", None) is None:
                started_dt = started_dt.replace(tzinfo=timezone.utc)
            elapsed = int((completed - started_dt).total_seconds())

        result_update = {
            "score": score,
            "percentage": _percentage(score, max_score),
            "completedAt": completed,
            "durationSeconds": elapsed,
            "status": "completed",
            "measurements": body.measurements,
            "calculations": body.calculations,
            "evaluation": body.evaluation,
        }
        transaction.update(result_ref, result_update)
        used = int(progress.get("demoAttemptsUsed", 0)) + 1
        if progress.get("completed"):
            next_state = progress.get("currentState") or STATE_SUBMITTED
            if next_state in (STATE_DEMO_IN_PROGRESS, STATE_AVAILABLE, STATE_DEMO_COMPLETED):
                next_state = STATE_SUBMITTED
        else:
            next_state = STATE_PRACTICAL_AVAILABLE
        transaction.update(sp_ref, {
            "demoAttemptsUsed": used,
            "demoCompleted": True,
            "currentState": next_state,
            "activeStartedAt": None,
            "activeAttemptType": None,
            "activeAttemptNumber": None,
            "activeResultId": None,
            "lastAttemptAt": completed,
        })
        result.update(result_update)
        result["_finalState"] = next_state
        return result

    data = _tx(db.transaction())
    return _to_result(body.resultId, data, data.get("_finalState") or STATE_PRACTICAL_AVAILABLE)


def start_practical(
    uid: str, practical_id: str, *, ignore_attempt_limit: bool = False
) -> SessionResponse:
    student = _require_active_student(uid)
    practical = _get_practical(practical_id)
    _assert_grade_allowed(student, practical)

    practical_max = _official_max_attempts(practical)
    sp_ref = db.collection("studentPracticals").document(_sp_id(uid, practical_id))
    result_id = str(uuid4())
    started = _now()

    @firestore.transactional
    def _tx(transaction: firestore.Transaction) -> dict:
        snap = sp_ref.get(transaction=transaction)
        progress = snap.to_dict() if snap.exists else _default_student_practical(uid, practical)
        used = int(progress.get("practicalAttemptsUsed", 0))
        state = progress.get("currentState", STATE_AVAILABLE)
        already_completed = bool(progress.get("completed", False))
        active_result_id = progress.get("activeResultId")
        if state == STATE_PRACTICAL_IN_PROGRESS and active_result_id:
            return {
                "resultId": active_result_id,
                "attemptNumber": int(progress.get("activeAttemptNumber") or used or 1),
                "startedAt": progress.get("activeStartedAt") or started,
                "resume": True,
            }
        # Demo is optional. Students may start the official practical immediately.
        # First official attempt is limited by config. After an official
        # completion, every practical can be retried; bestScore is kept.
        if (
            not ignore_attempt_limit
            and not already_completed
            and used >= practical_max
        ):
            raise HTTPException(
                status.HTTP_409_CONFLICT,
                "Official practical attempt limit reached. Attempt limits come from practical configuration.",
            )

        result_ref = db.collection("practicalResults").document(result_id)
        transaction.set(result_ref, {
            "studentId": uid,
            "practicalId": practical_id,
            "lessonId": practical.get("lessonId", ""),
            "grade": practical["grade"],
            "attemptType": "practical",
            "mode": "start",
            "attemptNumber": used + 1,
            "score": 0,
            "maxScore": _configured_max_score(practical),
            "percentage": 0,
            "startedAt": started,
            "completedAt": None,
            "durationSeconds": None,
            "status": "inProgress",
            "measurements": None,
            "calculations": None,
            "evaluation": None,
        })
        if not snap.exists:
            transaction.set(sp_ref, progress)
        transaction.update(sp_ref, {
            "currentState": STATE_PRACTICAL_IN_PROGRESS,
            "activeStartedAt": started,
            "activeAttemptType": "practical",
            "activeAttemptNumber": used + 1,
            "activeResultId": result_id,
        })
        return {
            "resultId": result_id,
            "attemptNumber": used + 1,
            "startedAt": started,
            "resume": False,
        }

    info = _tx(db.transaction())
    return SessionResponse(
        practicalId=practical_id,
        resultId=info["resultId"],
        mode="practical",
        attemptNumber=info["attemptNumber"],
        currentState=STATE_PRACTICAL_IN_PROGRESS,
        durationSeconds=_time_limit_seconds(practical),
        unitySceneId=practical.get("unitySceneId", ""),
        unityBuildUrl=practical.get("unityBuildUrl", ""),
        startedAt=_iso(info["startedAt"]),
    )


def submit_practical(uid: str, practical_id: str, body: SubmitPracticalRequest) -> PracticalResultResponse:
    student = _require_active_student(uid)
    practical = _get_practical(practical_id)
    _assert_grade_allowed(student, practical)

    max_score = _configured_max_score(practical)
    duration_limit = _time_limit_seconds(practical)
    score = max(0, min(int(body.score), max_score))

    sp_ref = db.collection("studentPracticals").document(_sp_id(uid, practical_id))
    result_ref = db.collection("practicalResults").document(body.resultId)
    completed = _now()

    @firestore.transactional
    def _tx(transaction: firestore.Transaction) -> dict:
        sp_snap = sp_ref.get(transaction=transaction)
        result_snap = result_ref.get(transaction=transaction)
        if not sp_snap.exists or not result_snap.exists:
            raise HTTPException(status.HTTP_404_NOT_FOUND, "Official practical session not found.")
        progress = sp_snap.to_dict() or {}
        result = result_snap.to_dict() or {}
        if result.get("status") in ("completed", "timeExpired"):
            return result

        if progress.get("currentState") != STATE_PRACTICAL_IN_PROGRESS:
            raise HTTPException(
                status.HTTP_409_CONFLICT,
                "No official practical is in progress. Duplicate submissions are rejected.",
            )
        if progress.get("activeResultId") != body.resultId:
            raise HTTPException(status.HTTP_409_CONFLICT, "resultId does not match the active official session.")
        if result.get("studentId") != uid or result.get("practicalId") != practical_id:
            raise HTTPException(status.HTTP_403_FORBIDDEN, "This result does not belong to you.")
        if result.get("attemptType") != "practical":
            raise HTTPException(status.HTTP_400_BAD_REQUEST, "This session is not an official practical.")
        if int(result.get("attemptNumber", 0)) != int(body.attemptNumber):
            raise HTTPException(status.HTTP_400_BAD_REQUEST, "attemptNumber does not match the active session.")

        started = result.get("startedAt")
        if started is None:
            raise HTTPException(status.HTTP_400_BAD_REQUEST, "Official session is missing server startedAt.")
        started_dt = started
        if getattr(started_dt, "tzinfo", None) is None:
            started_dt = started_dt.replace(tzinfo=timezone.utc)
        elapsed = int((completed - started_dt).total_seconds())
        if elapsed < 0:
            elapsed = 0
        client_time = body.durationSeconds
        if client_time is None and isinstance(body.measurements, dict):
            raw_time = body.measurements.get("timeUsed")
            if isinstance(raw_time, (int, float)):
                client_time = int(raw_time)
        if client_time is not None and client_time >= 0:
            elapsed = client_time
        result_status = "timeExpired" if duration_limit > 0 and elapsed > duration_limit else "completed"
        final_state = STATE_TIME_EXPIRED if result_status == "timeExpired" else STATE_SUBMITTED
        percentage = _percentage(score, max_score)
        first_completion = not bool(progress.get("completed", False))
        previous_best = int(progress.get("bestScore", 0))
        best_score = max(previous_best, score)

        result_update = {
            "score": score,
            "percentage": percentage,
            "completedAt": completed,
            "durationSeconds": elapsed,
            "status": result_status,
            "measurements": body.measurements,
            "calculations": body.calculations,
            "evaluation": body.evaluation,
        }
        transaction.update(result_ref, result_update)
        transaction.update(sp_ref, {
            "practicalAttemptsUsed": int(progress.get("practicalAttemptsUsed", 0)) + 1,
            "bestScore": best_score,
            "latestScore": score,
            "percentage": _percentage(best_score, max_score),
            "completed": True,
            "currentState": final_state,
            "activeStartedAt": None,
            "activeAttemptType": None,
            "activeAttemptNumber": None,
            "activeResultId": None,
            "lastAttemptAt": completed,
        })
        result.update(result_update)
        result["_finalState"] = final_state
        result["_firstCompletion"] = first_completion
        return result

    data = _tx(db.transaction())
    if data.get("status") in ("completed", "timeExpired") and not data.get("_finalState"):
        _, progress = _get_or_create_student_practical(uid, practical)
        return _to_result(body.resultId, data, progress.get("currentState"))
    _refresh_student_progress(uid, int(student["currentGrade"]))
    return _to_result(body.resultId, data, data.get("_finalState"))


def complete_official(uid: str, practical_id: str, body: CompletePracticalRequest) -> PracticalResultResponse:
    """One-shot official save used after Unity finishes.

    Opens or resumes an official session, then writes the score to
    practicalResults, studentPracticals, and studentProgress.
    """
    session = start_practical(uid, practical_id, ignore_attempt_limit=True)
    return submit_practical(
        uid,
        practical_id,
        SubmitPracticalRequest(
            resultId=session.resultId,
            attemptNumber=session.attemptNumber,
            score=body.score,
            durationSeconds=body.durationSeconds,
            measurements=body.measurements,
        ),
    )


def get_official_result(uid: str, practical_id: str) -> OfficialResultBundle:
    student = _require_active_student(uid)
    practical = _get_practical(practical_id)
    _assert_grade_allowed(student, practical)

    query = db.collection("practicalResults").where(
        filter=firestore.FieldFilter("studentId", "==", uid)
    )
    official = []
    for snap in query.stream():
        item = snap.to_dict() or {}
        if (
            item.get("practicalId") == practical_id
            and item.get("attemptType") == "practical"
            and item.get("status") in ("completed", "timeExpired")
        ):
            official.append((snap.id, item))
    if not official:
        raise HTTPException(status.HTTP_404_NOT_FOUND, "No official result found for this practical.")

    def _completed_at(item: dict):
        return item.get("completedAt") or item.get("startedAt") or datetime.min.replace(tzinfo=timezone.utc)

    latest_id, latest = max(official, key=lambda pair: _completed_at(pair[1]))
    best_id, best = max(official, key=lambda pair: int(pair[1].get("score", 0)))
    _, progress = _get_or_create_student_practical(uid, practical)
    state = progress.get("currentState")
    return OfficialResultBundle(
        latest=_to_result(latest_id, latest, state),
        best=_to_result(best_id, best, state),
    )


def get_my_progress(uid: str) -> StudentProgressResponse:
    ensure_catalogue(db)
    student = _require_active_student(uid)
    snap = db.collection("studentProgress").document(uid).get()
    if not snap.exists:
        _refresh_student_progress(uid, int(student["currentGrade"]))
        snap = db.collection("studentProgress").document(uid).get()
    data = snap.to_dict() or {}
    return StudentProgressResponse(
        studentId=uid,
        grade=int(data.get("grade", student["currentGrade"])),
        totalPracticals=int(data.get("totalPracticals", 0)),
        completedPracticals=int(data.get("completedPracticals", 0)),
        totalScore=int(data.get("totalScore", 0)),
        averagePercentage=float(data.get("averagePercentage", 0)),
        gradeProgress=data.get("gradeProgress") or {"9": {}, "10": {}, "11": {}},
        lessonProgress=data.get("lessonProgress") or {},
        recentResults=_recent_official_results(uid),
        updatedAt=_iso(data.get("updatedAt")),
    )


def _recent_official_results(uid: str, limit: int = 12) -> list[RecentPracticalItem]:
    titles = {
        pid: spec.get("title", pid)
        for pid, spec in CATALOGUE.items()
    }
    for snap in db.collection("practicals").stream():
        item = snap.to_dict() or {}
        titles[snap.id] = item.get("title") or titles.get(snap.id) or snap.id

    rows = []
    query = db.collection("practicalResults").where(
        filter=firestore.FieldFilter("studentId", "==", uid)
    )
    for snap in query.stream():
        item = snap.to_dict() or {}
        if item.get("attemptType") != "practical":
            continue
        if item.get("status") not in ("completed", "timeExpired"):
            continue
        completed_at = item.get("completedAt") or item.get("startedAt")
        practical_id = str(item.get("practicalId") or "")
        rows.append(
            RecentPracticalItem(
                practicalId=practical_id,
                title=titles.get(practical_id, practical_id),
                score=int(item.get("score", 0)),
                percentage=float(item.get("percentage", 0)),
                completedAt=_iso(completed_at),
                attemptType="practical",
            )
        )
    rows.sort(key=lambda row: row.completedAt or "", reverse=True)
    return rows[:limit]


def _empty_bucket() -> dict:
    return {
        "totalPracticals": 0,
        "completedPracticals": 0,
        "totalScore": 0,
        "averagePercentage": 0,
    }


def _refresh_student_progress(uid: str, student_grade: int) -> None:
    """
    Profile update rule (Word doc section 10):
      bestScore already stored per practical
      completedPracticals increases only on first official completion
      averagePercentage recalculated from completed official practicals
      gradeProgress[9|10|11] maintained separately
    """
    ensure_catalogue(db)
    practicals = {
        snap.id: (snap.to_dict() or {})
        for snap in db.collection("practicals").where(
            filter=firestore.FieldFilter("isActive", "==", True)
        ).stream()
    }
    progress_docs = {
        snap.id: (snap.to_dict() or {})
        for snap in db.collection("studentPracticals").where(
            filter=firestore.FieldFilter("studentId", "==", uid)
        ).stream()
    }

    grade_progress = {"9": _empty_bucket(), "10": _empty_bucket(), "11": _empty_bucket()}
    lesson_progress: dict[str, dict] = {}
    percentages_by_grade: dict[str, list[float]] = {"9": [], "10": [], "11": []}
    lesson_percentages: dict[str, list[float]] = {}

    for practical_id, practical in practicals.items():
        grade_key = str(int(practical.get("grade", 0)))
        if grade_key not in grade_progress:
            grade_progress[grade_key] = _empty_bucket()
            percentages_by_grade[grade_key] = []
        grade_progress[grade_key]["totalPracticals"] += 1
        lesson_id = practical.get("lessonId") or "unknown"
        lesson_progress.setdefault(lesson_id, _empty_bucket())
        lesson_progress[lesson_id]["totalPracticals"] += 1
        lesson_percentages.setdefault(lesson_id, [])

        sp = progress_docs.get(_sp_id(uid, practical_id), {})
        if sp.get("completed"):
            score = int(sp.get("bestScore", 0))
            pct = float(sp.get("percentage", 0))
            grade_progress[grade_key]["completedPracticals"] += 1
            grade_progress[grade_key]["totalScore"] += score
            percentages_by_grade[grade_key].append(pct)
            lesson_progress[lesson_id]["completedPracticals"] += 1
            lesson_progress[lesson_id]["totalScore"] += score
            lesson_percentages[lesson_id].append(pct)

    overall = _empty_bucket()
    overall_percentages: list[float] = []
    for key, bucket in grade_progress.items():
        scores = percentages_by_grade.get(key) or []
        bucket["averagePercentage"] = round(sum(scores) / len(scores), 2) if scores else 0
        overall["totalPracticals"] += bucket["totalPracticals"]
        overall["completedPracticals"] += bucket["completedPracticals"]
        overall["totalScore"] += bucket["totalScore"]
        overall_percentages.extend(scores)
    overall["averagePercentage"] = (
        round(sum(overall_percentages) / len(overall_percentages), 2) if overall_percentages else 0
    )
    for lesson_id, bucket in lesson_progress.items():
        scores = lesson_percentages.get(lesson_id) or []
        bucket["averagePercentage"] = round(sum(scores) / len(scores), 2) if scores else 0

    db.collection("studentProgress").document(uid).set({
        "studentId": uid,
        "grade": student_grade,
        "totalPracticals": overall["totalPracticals"],
        "completedPracticals": overall["completedPracticals"],
        "totalScore": overall["totalScore"],
        "averagePercentage": overall["averagePercentage"],
        "gradeProgress": grade_progress,
        "lessonProgress": lesson_progress,
        "updatedAt": firestore.SERVER_TIMESTAMP,
    })
