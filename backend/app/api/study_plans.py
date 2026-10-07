"""
api/study_plans.py — admin controls for the weekly study plan sweep. The
student-facing GET lives in api/student.py next to /learning-path, since
that file already owns this student's-own-data scope.
"""

from fastapi import APIRouter, Depends, HTTPException, status

from app.core.dependencies import VerifiedUser, require_admin
from app.core.firebase import db
from app.services import study_plan_service

router = APIRouter(prefix="/admin/study-plans", tags=["Study Plans"])


@router.post("/sweep")
async def trigger_sweep(admin: VerifiedUser = Depends(require_admin)) -> dict:
    """Manually runs the weekly sweep immediately (the same job the Monday
    scheduler runs) — lets an admin test/demo it without waiting for the
    clock. Pure Firestore reads + arithmetic (no LLM/video), so unlike the
    notification sweep's per-student send this stays a plain blocking call
    here too, matching how /admin/notifications/sweep is itself implemented."""
    return await study_plan_service.run_weekly_sweep(triggered_by="admin:manual")


@router.post("/{uid}/generate")
def force_generate(uid: str, admin: VerifiedUser = Depends(require_admin)) -> dict:
    student = db.collection("users").document(uid).get()
    if not student.exists or (student.to_dict() or {}).get("role") != "student":
        raise HTTPException(status.HTTP_404_NOT_FOUND, "Student not found.")
    return study_plan_service.generate_weekly_plan(
        uid, force=True, triggered_by=f"admin:{admin.uid}"
    )
