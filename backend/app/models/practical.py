"""
Pydantic models for the Game-Based Measurement practicals API.

Matches PhysiV-Lab Backend, Firebase Database & System Architecture
(attempt model, result document, and profile progress).
"""

from typing import Any, Literal

from pydantic import BaseModel, Field


AttemptType = Literal["demo", "practical"]
ResultStatus = Literal["inProgress", "completed", "timeExpired"]


class PracticalSummary(BaseModel):
    id: str
    title: str
    grade: int
    lessonId: str
    topicId: str
    description: str
    unitySceneId: str
    unityBuildUrl: str
    maxScore: int
    durationSeconds: int
    demoAllowed: bool
    demoMaxAttempts: int
    practicalMaxAttempts: int
    isActive: bool
    order: int = 0


class PracticalDetail(PracticalSummary):
    currentState: str
    demoAttemptsUsed: int
    practicalAttemptsUsed: int
    demoCompleted: bool
    completed: bool
    bestScore: int
    latestScore: int
    percentage: float


class SessionResponse(BaseModel):
    practicalId: str
    resultId: str
    mode: AttemptType
    attemptNumber: int
    currentState: str
    durationSeconds: int | None
    unitySceneId: str
    unityBuildUrl: str
    startedAt: str | None


class DemoFinishRequest(BaseModel):
    resultId: str
    score: int | None = Field(None, ge=0)
    measurements: dict[str, Any] | None = None
    calculations: dict[str, Any] | None = None
    evaluation: dict[str, Any] | None = None


class SubmitPracticalRequest(BaseModel):
    resultId: str
    attemptNumber: int = Field(..., ge=1)
    score: int = Field(..., ge=0)
    durationSeconds: int | None = Field(None, ge=0)
    measurements: dict[str, Any] | None = None
    calculations: dict[str, Any] | None = None
    evaluation: dict[str, Any] | None = None


class CompletePracticalRequest(BaseModel):
    score: int = Field(..., ge=0)
    durationSeconds: int | None = Field(None, ge=0)
    measurements: dict[str, Any] | None = None


class PracticalResultResponse(BaseModel):
    resultId: str
    studentId: str
    practicalId: str
    grade: int
    attemptType: AttemptType
    attemptNumber: int
    score: int
    maxScore: int
    percentage: float
    startedAt: str | None
    completedAt: str | None
    durationSeconds: int | None
    status: str
    measurements: dict[str, Any] | None = None
    calculations: dict[str, Any] | None = None
    evaluation: dict[str, Any] | None = None
    currentState: str | None = None


class OfficialResultBundle(BaseModel):
    latest: PracticalResultResponse | None = None
    best: PracticalResultResponse | None = None


class GradeProgress(BaseModel):
    totalPracticals: int = 0
    completedPracticals: int = 0
    totalScore: int = 0
    averagePercentage: float = 0


class RecentPracticalItem(BaseModel):
    practicalId: str
    title: str
    score: int
    percentage: float
    completedAt: str | None = None
    attemptType: str = "practical"


class StudentProgressResponse(BaseModel):
    studentId: str
    grade: int
    totalPracticals: int
    completedPracticals: int
    totalScore: int
    averagePercentage: float
    gradeProgress: dict[str, GradeProgress]
    lessonProgress: dict[str, GradeProgress]
    recentResults: list[RecentPracticalItem] = Field(default_factory=list)
    updatedAt: str | None = None
