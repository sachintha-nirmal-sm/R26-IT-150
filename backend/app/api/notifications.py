"""
api/notifications.py — FCM token registration, admin test send, and the
Phase 1 personalized-notification send/history endpoints.
"""

from pydantic import BaseModel, Field

from fastapi import APIRouter, BackgroundTasks, Depends, HTTPException, status

from app.core.dependencies import VerifiedUser, require_admin, require_student
from app.core.firebase import db
from app.core.utils import iso
from app.services import notification_service

router = APIRouter(tags=["Notifications"])


class RegisterTokenRequest(BaseModel):
    token: str = Field(..., min_length=1)
    platform: str = Field(..., pattern="^(android|ios|web)$")


class TestNotificationRequest(BaseModel):
    uid: str = Field(..., min_length=1)
    title: str = Field(..., min_length=1, max_length=100)
    body: str = Field(..., min_length=1, max_length=200)


class SendNotificationRequest(BaseModel):
    uid: str = Field(..., min_length=1)


@router.post("/api/notifications/register-token", status_code=status.HTTP_204_NO_CONTENT)
def register_token(
    body: RegisterTokenRequest,
    user: VerifiedUser = Depends(require_student),
) -> None:
    notification_service.register_token(user.uid, body.token, body.platform)


@router.post("/admin/notifications/test")
def send_test_notification(
    body: TestNotificationRequest,
    admin: VerifiedUser = Depends(require_admin),
) -> dict:
    result = notification_service.send_to_uid(body.uid, body.title, body.body)
    if result.get("error") == "no_fcm_tokens":
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="This student has no registered device tokens.",
        )
    return result


@router.post("/admin/notifications/send", status_code=status.HTTP_202_ACCEPTED)
def send_personalized_notification(
    body: SendNotificationRequest,
    background_tasks: BackgroundTasks,
    admin: VerifiedUser = Depends(require_admin),
) -> dict:
    student = db.collection("users").document(body.uid).get()
    if not student.exists or (student.to_dict() or {}).get("role") != "student":
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Student not found.")
    doc_id = notification_service.enqueue_personalized_notification(body.uid, admin.uid)
    background_tasks.add_task(notification_service.run_personalized_notification, doc_id)
    return {"notificationId": doc_id, "status": "queued"}


@router.post("/admin/notifications/sweep")
async def trigger_sweep(admin: VerifiedUser = Depends(require_admin)) -> dict:
    """Manually runs the auto-sweep immediately (the same job the hourly
    scheduler runs) — lets an admin test/demo it without waiting for the
    clock, and is a legitimate ops action for a real deployment too."""
    return await notification_service.run_auto_sweep()


@router.get("/admin/notifications")
def list_notifications(admin: VerifiedUser = Depends(require_admin)) -> list[dict]:
    docs = (
        db.collection("notifications")
        .order_by("createdAt", direction="DESCENDING")
        .limit(100)
        .stream()
    )
    out = []
    for snap in docs:
        data = snap.to_dict() or {}
        data["id"] = snap.id
        data["createdAt"] = iso(data.get("createdAt"))
        data["sentAt"] = iso(data.get("sentAt"))
        out.append(data)
    return out
