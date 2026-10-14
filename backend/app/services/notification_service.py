"""
notification_service.py — FCM token storage, push-send primitives, and the
Phase 1 personalization orchestrator (profile -> mood -> script -> send).

Reuses the firebase_admin app already initialized in app.core.firebase — never
call firebase_admin.initialize_app() again here.
"""

from __future__ import annotations

import uuid
from datetime import datetime, timezone

from firebase_admin import firestore, messaging
from google.cloud.firestore import FieldFilter

from app.core.firebase import db
from app.services import notification_mood as mood_service
from app.services import notification_profile_service
from app.services import notification_safety
from app.services import notification_script

Platform = str  # "android" | "ios" | "web"


def register_token(uid: str, token: str, platform: Platform) -> None:
    """Save/refresh one FCM token for a student under users/{uid}.fcmTokens."""
    db.collection("users").document(uid).set(
        {
            "fcmTokens": {
                token: {
                    "platform": platform,
                    "updatedAt": firestore.SERVER_TIMESTAMP,
                }
            }
        },
        merge=True,
    )


def unregister_token(uid: str, token: str) -> None:
    """Remove one FCM token (e.g. on logout, or after FCM reports it dead)."""
    db.collection("users").document(uid).update(
        {f"fcmTokens.{token}": firestore.DELETE_FIELD}
    )


def _tokens_for_uid(uid: str) -> list[str]:
    snap = db.collection("users").document(uid).get()
    data = snap.to_dict() or {}
    tokens = data.get("fcmTokens") or {}
    return list(tokens.keys())


def _send_one(
    token: str,
    title: str,
    body: str,
    data: dict[str, str],
    image_url: str | None,
) -> bool:
    message = messaging.Message(
        token=token,
        notification=messaging.Notification(title=title, body=body, image=image_url),
        data=data,
        android=messaging.AndroidConfig(priority="high"),
        apns=messaging.APNSConfig(
            payload=messaging.APNSPayload(aps=messaging.Aps(mutable_content=True))
        ),
    )
    messaging.send(message)
    return True


def send_to_uid(
    uid: str,
    title: str,
    body: str,
    data: dict[str, str] | None = None,
    image_url: str | None = None,
) -> dict:
    """
    Send a push to every device registered for this student.

    Prunes any token FCM reports as unregistered. Returns
    {"sent": int, "failed": int, "tokens": int, "error"?: str}.
    """
    tokens = _tokens_for_uid(uid)
    if not tokens:
        return {"sent": 0, "failed": 0, "tokens": 0, "error": "no_fcm_tokens"}

    sent = 0
    failed = 0
    for token in tokens:
        try:
            _send_one(token, title, body, data or {}, image_url)
            sent += 1
        except messaging.UnregisteredError:
            unregister_token(uid, token)
            failed += 1
        except Exception:
            failed += 1
    return {"sent": sent, "failed": failed, "tokens": len(tokens)}


def enqueue_personalized_notification(uid: str, admin_uid: str) -> str:
    """Creates the notifications/{id} doc in 'queued' state and returns its
    id. The actual generation+send happens in run_personalized_notification,
    run via BackgroundTasks so the admin request returns immediately — same
    pattern as generation_service.enqueue_question_bank_job."""
    doc_id = str(uuid.uuid4())
    db.collection("notifications").document(doc_id).set({
        "uid": uid,
        "mood": None,
        "title": None,
        "body": None,
        "usedFallback": None,
        "status": "queued",
        "triggeredBy": f"admin:{admin_uid}",
        "createdAt": firestore.SERVER_TIMESTAMP,
        "sentAt": None,
        "errorMessage": None,
    })
    return doc_id


async def run_personalized_notification(doc_id: str) -> None:
    """Background task: build profile, pick mood, generate (or fall back to)
    a script, send the push, and record the outcome on the notifications doc."""
    doc_ref = db.collection("notifications").document(doc_id)
    doc = doc_ref.get().to_dict() or {}
    uid = doc.get("uid")
    if not uid:
        doc_ref.update({"status": "failed", "errorMessage": "missing uid"})
        return

    try:
        profile = notification_profile_service.build_profile(uid)
        selected_mood = mood_service.select_mood(profile)

        used_fallback = False
        try:
            script = await notification_script.generate_script(profile, selected_mood)
            ok, reason = notification_safety.check_script(script["title"], script["body"])
            if not ok:
                raise ValueError(f"safety check failed: {reason}")
        except Exception:
            script = notification_safety.fallback_script(selected_mood)
            used_fallback = True

        doc_ref.update({
            "mood": selected_mood,
            "title": script["title"],
            "body": script["body"],
            "usedFallback": used_fallback,
            "status": "generatingVideo",
        })

        video_url: str | None = None
        thumb_url: str | None = None
        try:
            from app.services import notification_video

            weak_topic = (profile.get("weakTopic") or {}).get("lessonTag")
            video = await notification_video.generate_video(
                uid, doc_id, script["title"], script["body"], selected_mood,
                topic=weak_topic, student_name=profile.get("fullName"),
            )
            video_url = video["videoUrl"]
            thumb_url = video["thumbUrl"]
            doc_ref.update({"videoUrl": video_url, "thumbUrl": thumb_url})
        except Exception as exc:
            # Video is a nice-to-have on top of the already-proven text push —
            # never let a video failure stop the notification from sending.
            doc_ref.update({"videoError": str(exc)[:500]})

        data = {"notificationId": doc_id}
        data["type"] = "motivation_video" if video_url else "personalized_notification"
        if video_url:
            data["videoId"] = doc_id

        result = send_to_uid(
            uid,
            script["title"],
            script["body"],
            data=data,
            image_url=thumb_url,
        )
        if result.get("sent", 0) > 0:
            doc_ref.update({"status": "sent", "sentAt": firestore.SERVER_TIMESTAMP})
        else:
            doc_ref.update({
                "status": "failed",
                "errorMessage": result.get("error", "no devices reached"),
            })
    except Exception as exc:
        doc_ref.update({"status": "failed", "errorMessage": str(exc)[:500]})


def _sent_today(uid: str) -> bool:
    """True if this student already has a notification created today
    (any status) — the one-per-day guard for the auto sweep."""
    docs = (
        db.collection("notifications")
        .where(filter=FieldFilter("uid", "==", uid))
        .order_by("createdAt", direction="DESCENDING")
        .limit(1)
        .stream()
    )
    today = datetime.now(timezone.utc).date()
    for snap in docs:
        created = (snap.to_dict() or {}).get("createdAt")
        if created and hasattr(created, "date") and created.date() == today:
            return True
    return False


async def run_auto_sweep() -> dict:
    """Hourly scheduled sweep (see main.py's lifespan): for every active
    student whose computed most-active-hour matches the current hour (and
    who hasn't already been sent one today, and who isn't in the quiet-hours
    floor), auto-generate and send a personalized notification — every
    student gets at least a "welcome" generic nudge if nothing more specific
    matches (see notification_mood.select_mood). Same pipeline as the
    admin-manual send, just self-triggered with triggeredBy='auto'."""
    now_hour = datetime.now(timezone.utc).hour
    candidates = 0
    enqueued = 0

    students = (
        db.collection("users")
        .where(filter=FieldFilter("role", "==", "student"))
        .where(filter=FieldFilter("status", "==", "active"))
        .stream()
    )
    for snap in students:
        uid = snap.id
        candidates += 1
        try:
            profile = notification_profile_service.build_profile(uid)
            if not mood_service.is_eligible_to_send(now_hour, profile["mostActiveHour"]):
                continue
            if _sent_today(uid):
                continue

            doc_id = str(uuid.uuid4())
            db.collection("notifications").document(doc_id).set({
                "uid": uid,
                "mood": None,
                "title": None,
                "body": None,
                "usedFallback": None,
                "status": "queued",
                "triggeredBy": "auto",
                "createdAt": firestore.SERVER_TIMESTAMP,
                "sentAt": None,
                "errorMessage": None,
            })
            await run_personalized_notification(doc_id)
            enqueued += 1
        except Exception:
            continue

    return {"candidates": candidates, "enqueued": enqueued, "hour": now_hour}
