from pydantic import BaseModel, EmailStr, Field


class User(BaseModel):
    uid: str
    role: str
    fullName: str
    email: EmailStr | None = None
    currentGrade: int | None = Field(None, ge=9, le=12)
    status: str = "active"
