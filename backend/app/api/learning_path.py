from fastapi import APIRouter, Depends
from pydantic import BaseModel

from app.core.dependencies import VerifiedUser, require_student
from app.services.learning_path_service import generate_learning_path

router = APIRouter(prefix="/api/learning-path", tags=["Learning Path"])


class GeneratePathRequest(BaseModel):
    studentId: str | None = None


@router.post("/generate")
async def generate_path(
    body: GeneratePathRequest | None = None,
    user: VerifiedUser = Depends(require_student),
) -> dict:
    return generate_learning_path(user.uid)


@router.get("")
async def get_path(user: VerifiedUser = Depends(require_student)) -> dict:
    return generate_learning_path(user.uid)
