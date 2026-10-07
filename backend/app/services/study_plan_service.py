"""
study_plan_service.py — generates a personalized one-week study plan per
student from their existing weakTopics data + the new lessonEngagement
tracking, and persists it as users/{uid}/studyPlans/{weekId}.

Single orchestrator, same role notification_service.py plays for video
notifications: one function (generate_weekly_plan) used identically by the
scheduled sweep, the admin force-regenerate endpoint, and a lazy-on-read
path for a student's first load of the week.

Deliberately does NOT touch weaknessScore's formula (app/core/utils.py) or
learning_path_service.generate_learning_path (which two existing, unrelated
routes already depend on) — this reimplements the same weakTopics filter in
~10 lines instead of calling that function, specifically to keep
lastUpdated available for the recency tie-break below, without coupling
this service to that function's trimmed return shape.
"""

from __future__ import annotations

import os
from datetime import datetime, timezone

from google.cloud import firestore

from app.core.firebase import db
from app.core.utils import WEAKNESS_THRESHOLD, iso_week_bounds, iso_week_id
from app.services import lesson_engagement_service

MAX_FOCUS_AREAS = 5
MAX_LESSONS_PER_FOCUS_AREA = 2
MAX_EXPLORATION_LESSONS = 5
GENERATION_VERSION = 1


def _user_ref(uid: str):
    return db.collection("users").document(uid)


def _clean_tag_fallback(tag: str) -> str:
    """Non-LLM fallback if Groq is unavailable/fails — never let a naming
    nicety block plan generation."""
    text = tag.replace("phy-g", "Grade ").replace("-", " ").replace("_", " ")
    return " ".join(text.split()).title()


def _friendly_topic_name(lesson_tag: str, fallback_title: str | None) -> str:
    """Human-readable heading for a focus area — e.g. 'Forces & Newton's
    Laws' instead of the raw tag 'phy-g10-forces'. Cached in Firestore per
    tag (topicDisplayNames/{tag}) so Groq is called at most once ever per
    topic, never once per student per week."""
    if not lesson_tag:
        return fallback_title or "Topic"

    cache_ref = db.collection("topicDisplayNames").document(lesson_tag)
    cached = cache_ref.get()
    if cached.exists:
        name = (cached.to_dict() or {}).get("name")
        if name:
            return name

    try:
        from groq import Groq

        api_key = os.getenv("GROQ_API_KEY", "")
        if not api_key:
            raise ValueError("GROQ_API_KEY is not set")
        client = Groq(api_key=api_key)
        user_prompt = f"Identifier: {lesson_tag}"
        if fallback_title:
            user_prompt += f" (lesson: {fallback_title})"
        resp = client.chat.completions.create(
            model="openai/gpt-oss-20b",
            messages=[
                {"role": "system", "content": (
                    "You turn a short physics topic identifier into a clean, "
                    "proper topic name for a student's to-do list heading. "
                    "Max 5 words, title case, no punctuation besides '&' or "
                    "apostrophes. Return ONLY the name, nothing else."
                )},
                {"role": "user", "content": user_prompt},
            ],
            temperature=0.3,
            max_tokens=60,
            # openai/gpt-oss-20b is a reasoning model — without this it burns
            # its token budget on hidden chain-of-thought instead of writing
            # the actual name (see notification_script.py for the same fix).
            reasoning_effort="low",
        )
        name = (resp.choices[0].message.content or "").strip().strip('"')
        if not name:
            raise ValueError("empty name")
    except Exception:
        name = fallback_title or _clean_tag_fallback(lesson_tag)

    cache_ref.set({
        "name": name,
        "lessonTag": lesson_tag,
        "generatedAt": firestore.SERVER_TIMESTAMP,
    })
    return name


def _weak_topics(uid: str) -> list[dict]:
    """Same filter generate_learning_path() applies, reimplemented here so
    lastUpdated survives for the recency tie-break in _select_focus_areas."""
    rows = []
    for doc in _user_ref(uid).collection("weakTopics").stream():
        data = doc.to_dict() or {}
        score = float(data.get("weaknessScore") or 0)
        if score < WEAKNESS_THRESHOLD:
            continue
        by_type = data.get("byQuestionType") or {}
        weak_types = [
            t for t, b in by_type.items()
            if float((b or {}).get("weaknessScore") or 0) >= WEAKNESS_THRESHOLD
        ]
        rows.append({
            "lessonTag": data.get("lessonTag"),
            "lessonId": data.get("lessonId"),
            "weaknessScore": score,
            "weakQuestionTypes": weak_types,
            "incorrectCount": int(data.get("incorrectCount") or 0),
            "totalAttempted": int(data.get("totalAttempted") or 0),
            "lastUpdated": data.get("lastUpdated") or datetime.min.replace(tzinfo=timezone.utc),
        })
    rows.sort(key=lambda r: (r["weaknessScore"], r["lastUpdated"]), reverse=True)
    return rows[:MAX_FOCUS_AREAS]


def _unattempted_lessons(uid: str, grade: int, engagement: dict[str, dict]) -> list[dict]:
    """Published lessons for this grade the student has no engagement doc
    for at all yet (never opened) — catalogue order. Shared by the
    no-weakness fallback and the always-present 'explore' list below."""
    docs = (
        db.collection("lessons")
        .where(filter=firestore.FieldFilter("grade", "==", grade))
        .where(filter=firestore.FieldFilter("status", "==", "published"))
        .stream()
    )
    candidates = []
    for doc in docs:
        data = doc.to_dict() or {}
        if doc.id in engagement:
            continue
        candidates.append({
            "lessonId": doc.id,
            "lessonTag": data.get("lessonTag"),
            "title": data.get("title"),
            "order": data.get("order", 0),
        })
    candidates.sort(key=lambda c: c["order"])
    return candidates


def _fallback_focus_areas(uid: str, grade: int, engagement: dict[str, dict]) -> list[dict]:
    """No qualifying weakness yet (new student, or genuinely strong) — a
    plan must never be empty, so recommend the next not-yet-touched lessons
    in catalogue order instead of a hollow 'nothing to show' screen."""
    candidates = _unattempted_lessons(uid, grade, engagement)
    out = []
    for c in candidates[:MAX_FOCUS_AREAS]:
        out.append({
            "lessonTag": c["lessonTag"],
            "topicName": c["title"] or _clean_tag_fallback(c["lessonTag"] or ""),
            "lessonId": c["lessonId"],
            "weaknessScore": 0.0,
            "weakQuestionTypes": [],
            "reason": "Keep moving forward — here's what's next in your grade.",
            "recommendedLessons": [
                {"lessonId": c["lessonId"], "title": c["title"], "alreadyViewed": False}
            ],
            "suggestedQuiz": None,
        })
    return out


def _exploration_lessons(
    uid: str, grade: int, engagement: dict[str, dict], exclude_lesson_ids: set[str]
) -> list[dict]:
    """Lessons for this grade the student hasn't attempted yet and that
    aren't already covered by a weakness-based focus area — a separate
    'go explore this subject' section so the plan always points at
    something new, not just remediation."""
    candidates = _unattempted_lessons(uid, grade, engagement)
    out = []
    for c in candidates:
        if c["lessonId"] in exclude_lesson_ids:
            continue
        out.append({
            "lessonId": c["lessonId"],
            "lessonTag": c["lessonTag"],
            "title": c["title"],
        })
        if len(out) >= MAX_EXPLORATION_LESSONS:
            break
    return out


def _lessons_for_tag(lesson_tag: str, grade: int) -> list[dict]:
    docs = (
        db.collection("lessons")
        .where(filter=firestore.FieldFilter("lessonTag", "==", lesson_tag))
        .where(filter=firestore.FieldFilter("grade", "==", grade))
        .where(filter=firestore.FieldFilter("status", "==", "published"))
        .stream()
    )
    return [{"id": d.id, **(d.to_dict() or {})} for d in docs]


def _suggested_quiz(uid: str, lesson_id: str) -> dict | None:
    quizzes = list(db.collection("lessons").document(lesson_id).collection("quizzes").stream())
    if not quizzes:
        return None
    progress_coll = _user_ref(uid).collection("quizProgress")
    best_candidate = None
    for doc in quizzes:
        quiz = doc.to_dict() or {}
        max_attempts = int(quiz.get("maxAttempts") or 3)
        progress_snap = progress_coll.document(doc.id).get()
        progress = progress_snap.to_dict() if progress_snap.exists else {}
        attempts_used = int(progress.get("attemptsUsed") or 0)
        best_score = int(progress.get("bestScore") or 0)
        attempts_remaining = max(max_attempts - attempts_used, 0)
        if attempts_remaining <= 0:
            continue
        candidate = {
            "quizId": doc.id,
            "lessonId": lesson_id,
            "bestScore": best_score,
            "attemptsRemaining": attempts_remaining,
        }
        # Prefer the quiz this student has scored lowest on so far.
        if best_candidate is None or best_score < best_candidate["bestScore"]:
            best_candidate = candidate
    return best_candidate


def _build_focus_area(uid: str, topic: dict, grade: int, engagement: dict[str, dict]) -> dict:
    lesson_tag = topic["lessonTag"]
    candidates = _lessons_for_tag(lesson_tag, grade) if lesson_tag else []

    not_completed = [c for c in candidates if not engagement.get(c["id"], {}).get("completed")]
    pool = not_completed or candidates
    chosen = pool[:MAX_LESSONS_PER_FOCUS_AREA]
    recommended_lessons = [
        {
            "lessonId": c["id"],
            "title": c.get("title"),
            "alreadyViewed": bool(engagement.get(c["id"], {}).get("completed")),
        }
        for c in chosen
    ]

    # Only suggest a quiz retry once the student has actually gone through
    # at least one lecture note for this topic — recommending a quiz before
    # any lesson has been opened/completed puts the test before the study.
    has_studied_any = any(
        engagement.get(c["id"], {}).get("completed") for c in candidates
    )
    suggested_quiz = None
    if has_studied_any:
        for lesson in chosen:
            suggested_quiz = _suggested_quiz(uid, lesson["id"])
            if suggested_quiz:
                break

    incorrect = topic["incorrectCount"]
    total = topic["totalAttempted"]
    types = topic["weakQuestionTypes"]
    reason = f"{incorrect} of {total} incorrect so far"
    if types:
        reason += f", mostly {', '.join(types)}"

    fallback_title = chosen[0].get("title") if chosen else None
    topic_name = _friendly_topic_name(lesson_tag, fallback_title)

    return {
        "lessonTag": lesson_tag,
        "topicName": topic_name,
        "lessonId": topic.get("lessonId"),
        "weaknessScore": topic["weaknessScore"],
        "weakQuestionTypes": types,
        "reason": reason,
        "recommendedLessons": recommended_lessons,
        "suggestedQuiz": suggested_quiz,
    }


def generate_weekly_plan(uid: str, *, force: bool = False, triggered_by: str = "auto") -> dict:
    now = datetime.now(timezone.utc)
    week_id = iso_week_id(now)
    week_start, week_end = iso_week_bounds(now)
    ref = _user_ref(uid).collection("studyPlans").document(week_id)

    existing = ref.get()
    if existing.exists and not force:
        return {"id": existing.id, **(existing.to_dict() or {})}

    try:
        profile_snap = _user_ref(uid).get()
        if not profile_snap.exists:
            raise ValueError("Student profile not found.")
        grade = (profile_snap.to_dict() or {}).get("currentGrade")

        engagement = lesson_engagement_service.get_engagement_map(uid)
        weak_topics = _weak_topics(uid)
        fallback_mode = not weak_topics
        if fallback_mode:
            focus_areas = _fallback_focus_areas(uid, grade, engagement)
            # Fallback focus areas already *are* "lessons you haven't
            # attempted yet" — a separate explore section would just repeat
            # the same content.
            exploration_lessons = []
        else:
            focus_areas = [_build_focus_area(uid, t, grade, engagement) for t in weak_topics]
            already_recommended = {
                lesson["lessonId"]
                for area in focus_areas
                for lesson in area["recommendedLessons"]
            }
            exploration_lessons = _exploration_lessons(
                uid, grade, engagement, already_recommended
            )

        engagement_counts = lesson_engagement_service.week_counts(uid, week_start, week_end)
        quiz_attempts_this_week = 0
        for coll_name in ("quizAttempts", "finalQuizAttempts"):
            docs = (
                _user_ref(uid).collection(coll_name)
                .where(filter=firestore.FieldFilter("submittedAt", ">=", week_start))
                .where(filter=firestore.FieldFilter("submittedAt", "<", week_end))
                .stream()
            )
            quiz_attempts_this_week += sum(1 for _ in docs)

        doc = {
            "weekId": week_id,
            "uid": uid,
            "weekStartDate": week_start,
            "weekEndDate": week_end,
            "status": "generated",
            "generationVersion": GENERATION_VERSION,
            "weaknessThresholdUsed": WEAKNESS_THRESHOLD,
            "fallbackMode": fallback_mode,
            "focusAreas": focus_areas,
            "explorationLessons": exploration_lessons,
            "engagementSummary": {
                **engagement_counts,
                "quizzesAttemptedThisWeek": quiz_attempts_this_week,
                "weakTopicsTracked": len(weak_topics),
            },
            "triggeredBy": triggered_by,
            "createdAt": firestore.SERVER_TIMESTAMP,
            "errorMessage": None,
        }
        ref.set(doc)
        # doc["createdAt"] is still the unresolved firestore.SERVER_TIMESTAMP
        # sentinel after .set() — Firestore only resolves it server-side, it
        # never mutates the local dict. Returning that sentinel straight to
        # an HTTP caller crashes FastAPI's JSON encoder (it isn't
        # serializable), so the response substitutes the already-computed
        # local `now` instead; the persisted Firestore doc still has the
        # accurate, clock-skew-proof server timestamp.
        return {"id": week_id, **doc, "createdAt": now}
    except Exception as exc:
        failure = {
            "weekId": week_id,
            "uid": uid,
            "status": "failed",
            "triggeredBy": triggered_by,
            "createdAt": firestore.SERVER_TIMESTAMP,
            "errorMessage": str(exc)[:500],
        }
        ref.set(failure, merge=True)
        return {"id": week_id, **failure, "createdAt": now}


def get_current_plan(uid: str) -> dict:
    return generate_weekly_plan(uid, force=False, triggered_by="system:lazy-get")


async def run_weekly_sweep(triggered_by: str = "auto") -> dict:
    candidates = 0
    generated = 0
    failed = 0
    students = (
        db.collection("users")
        .where(filter=firestore.FieldFilter("role", "==", "student"))
        .where(filter=firestore.FieldFilter("status", "==", "active"))
        .stream()
    )
    for snap in students:
        candidates += 1
        try:
            result = generate_weekly_plan(snap.id, force=False, triggered_by=triggered_by)
            if result.get("status") == "failed":
                failed += 1
            else:
                generated += 1
        except Exception:
            failed += 1
    return {"candidates": candidates, "generated": generated, "failed": failed}
