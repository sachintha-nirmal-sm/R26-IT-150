from pydantic import BaseModel


class Quiz(BaseModel):
    id: str
    title: str
