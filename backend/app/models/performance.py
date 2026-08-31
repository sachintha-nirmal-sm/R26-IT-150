from pydantic import BaseModel


class Performance(BaseModel):
    overallAccuracy: float = 0.0
    byQuestionType: dict = {}


class WeakTopic(BaseModel):
    lessonTag: str
    lessonId: str | None = None
    incorrectCount: int = 0
    totalAttempted: int = 0
    weaknessScore: float = 0.0
