"""
api/practicals.py — Game-Based Measurement practicals endpoints.

Word-doc API map:
  GET  /api/practicals?grade=9
  GET  /api/practicals?lessonId=phy-g9-density-doc
  GET  /api/practicals/{practicalId}
  POST /api/practicals/{id}/demo/start
  POST /api/practicals/{id}/demo/finish
  POST /api/practicals/{id}/start
  POST /api/practicals/{id}/submit
  GET  /api/practicals/{id}/result
  GET  /api/students/me/progress
"""

from fastapi import APIRouter, Depends, Query

from app.core.dependencies import VerifiedUser, require_student
from app.models.practical import (
    CompletePracticalRequest,
    DemoFinishRequest,
    OfficialResultBundle,
    PracticalDetail,
    PracticalResultResponse,
    PracticalSummary,
    SessionResponse,
    StudentProgressResponse,
    SubmitPracticalRequest,
)
from app.services import practical_service as service

router = APIRouter(tags=["Practicals"])


@router.get(
    "/api/practicals",
    response_model=list[PracticalSummary],
    summary="List active practicals for the student's grade",
)
def list_practicals(
    grade: int | None = Query(None, ge=9, le=11),
    lesson_id: str | None = Query(None, alias="lessonId"),
    user: VerifiedUser = Depends(require_student),
) -> list[PracticalSummary]:
    return service.list_practicals(user.uid, grade, lesson_id)


@router.get(
    "/api/practicals/{practical_id}",
    response_model=PracticalDetail,
    summary="Load practical home/execution metadata",
)
def get_practical(
    practical_id: str,
    user: VerifiedUser = Depends(require_student),
) -> PracticalDetail:
    return service.get_practical(user.uid, practical_id)


@router.post(
    "/api/practicals/{practical_id}/demo/start",
    response_model=SessionResponse,
    summary="Start the single demo attempt",
)
def start_demo(
    practical_id: str,
    user: VerifiedUser = Depends(require_student),
) -> SessionResponse:
    return service.start_demo(user.uid, practical_id)


@router.post(
    "/api/practicals/{practical_id}/demo/finish",
    response_model=PracticalResultResponse,
    summary="Finish demo without updating the official score",
)
def finish_demo(
    practical_id: str,
    body: DemoFinishRequest,
    user: VerifiedUser = Depends(require_student),
) -> PracticalResultResponse:
    return service.finish_demo(user.uid, practical_id, body)


@router.post(
    "/api/practicals/{practical_id}/start",
    response_model=SessionResponse,
    summary="Start the official timed practical",
)
def start_practical(
    practical_id: str,
    user: VerifiedUser = Depends(require_student),
) -> SessionResponse:
    return service.start_practical(user.uid, practical_id)


@router.post(
    "/api/practicals/{practical_id}/submit",
    response_model=PracticalResultResponse,
    summary="Submit Unity official result",
)
def submit_practical(
    practical_id: str,
    body: SubmitPracticalRequest,
    user: VerifiedUser = Depends(require_student),
) -> PracticalResultResponse:
    return service.submit_practical(user.uid, practical_id, body)


@router.post(
    "/api/practicals/{practical_id}/complete",
    response_model=PracticalResultResponse,
    summary="Save Unity official score and update the student profile",
)
def complete_practical(
    practical_id: str,
    body: CompletePracticalRequest,
    user: VerifiedUser = Depends(require_student),
) -> PracticalResultResponse:
    return service.complete_official(user.uid, practical_id, body)


@router.get(
    "/api/practicals/{practical_id}/result",
    response_model=OfficialResultBundle,
    summary="Return the student's latest and best official result",
)
def get_result(
    practical_id: str,
    user: VerifiedUser = Depends(require_student),
) -> OfficialResultBundle:
    return service.get_official_result(user.uid, practical_id)


@router.get(
    "/api/students/me/progress",
    response_model=StudentProgressResponse,
    summary="Load the authenticated student's aggregated practical progress",
)
def get_my_progress(
    user: VerifiedUser = Depends(require_student),
) -> StudentProgressResponse:
    return service.get_my_progress(user.uid)
