from pydantic import BaseModel


class Performance(BaseModel):
    user_id: str
    score: float
