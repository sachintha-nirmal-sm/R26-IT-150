from pydantic import BaseModel, Field


class Quiz(BaseModel):
    id: str
    title: str
    lessonId: str
    maxAttempts: int = 3
    questionsPerAttempt: int = 20
    status: str = "noBankGenerated"
    activeQuestionBankVersionId: str | None = None


class SanitizedQuestion(BaseModel):
    questionId: str
    questionText: str
    questionType: str | None = None
    options: list[str] | None = None
    difficulty: str | None = None
    marks: int = 0
    lessonTag: str | None = None


class QuizAnswer(BaseModel):
    questionId: str
    studentAnswer: str | None = None
    isCorrect: bool | None = None
    marksAwarded: int | None = None
    lessonTag: str | None = None
    difficulty: str | None = None
    questionType: str | None = None
