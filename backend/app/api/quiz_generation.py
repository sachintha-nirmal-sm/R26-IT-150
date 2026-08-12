"""
quiz_generation.py — Generate MCQ questions from lesson PDF using Gemini, Groq (Llama), or Mistral.
POST /admin/lessons/{lesson_id}/generate-quiz
"""

import os
import json
import re
import io
import uuid
from typing import Literal
from fastapi import APIRouter, Depends, HTTPException, status
from pydantic import BaseModel, Field
from google.cloud import firestore, storage as gcs

from app.core.dependencies import VerifiedUser, require_admin
from app.core.firebase import db, bucket

router = APIRouter(prefix="/admin/lessons", tags=["Admin - Quiz Generation"])

# ---------------------------------------------------------------------------
# Config — set these in your .env or environment
# ---------------------------------------------------------------------------
GEMINI_API_KEY  = os.getenv("GEMINI_API_KEY", "")
GROQ_API_KEY    = os.getenv("GROQ_API_KEY", "")
MISTRAL_API_KEY = os.getenv("MISTRAL_API_KEY", "")

# ---------------------------------------------------------------------------
# Request / Response
# ---------------------------------------------------------------------------

class QuizGenerateRequest(BaseModel):
    model: Literal["gemini", "llama", "mistral"] = Field(
        ..., description="AI model to use for generation"
    )
    num_questions: int = Field(10, ge=3, le=30, description="Number of MCQ questions")
    material_id: str = Field(..., description="ID of the uploaded PDF material")


class MCQOption(BaseModel):
    label: str   # A, B, C, D
    text: str


class MCQQuestion(BaseModel):
    id: str
    question: str
    options: list[MCQOption]
    correct: str  # A, B, C, or D
    explanation: str


class QuizGenerateResponse(BaseModel):
    lesson_id: str
    model_used: str
    questions_saved: int
    questions: list[MCQQuestion]


# ---------------------------------------------------------------------------
# PDF text extraction
# ---------------------------------------------------------------------------

def extract_text_from_pdf(pdf_bytes: bytes) -> str:
    try:
        import pypdf
        reader = pypdf.PdfReader(io.BytesIO(pdf_bytes))
        text = ""
        for page in reader.pages:
            text += page.extract_text() or ""
        return text.strip()
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to extract PDF text: {e}")


# ---------------------------------------------------------------------------
# Prompt builder
# ---------------------------------------------------------------------------

def build_prompt(text: str, num_questions: int) -> str:
    # Truncate to avoid token limits
    truncated = text[:12000]
    return f"""You are a physics teacher. Read the following lesson content and generate exactly {num_questions} multiple-choice questions (MCQs).

LESSON CONTENT:
{truncated}

RULES:
- Each question must have exactly 4 options: A, B, C, D
- Only one option is correct
- Include a short explanation for the correct answer
- Questions must be directly based on the lesson content
- Vary difficulty: mix easy, medium, and hard questions

Return ONLY a valid JSON array in this exact format, no extra text:
[
  {{
    "question": "Question text here?",
    "options": {{
      "A": "Option A text",
      "B": "Option B text",
      "C": "Option C text",
      "D": "Option D text"
    }},
    "correct": "A",
    "explanation": "Brief explanation why A is correct."
  }}
]"""


# ---------------------------------------------------------------------------
# AI callers
# ---------------------------------------------------------------------------

async def call_gemini(prompt: str) -> str:
    if not GEMINI_API_KEY:
        raise HTTPException(status_code=500, detail="GEMINI_API_KEY not set in environment.")
    import httpx
    url = f"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={GEMINI_API_KEY}"
    payload = {"contents": [{"parts": [{"text": prompt}]}]}
    async with httpx.AsyncClient(timeout=60) as client:
        r = await client.post(url, json=payload)
        if r.status_code != 200:
            raise HTTPException(status_code=502, detail=f"Gemini error: {r.text}")
        data = r.json()
        return data["candidates"][0]["content"]["parts"][0]["text"]


async def call_groq_llama(prompt: str) -> str:
    if not GROQ_API_KEY:
        raise HTTPException(status_code=500, detail="GROQ_API_KEY not set in environment.")
    import httpx
    url = "https://api.groq.com/openai/v1/chat/completions"
    headers = {"Authorization": f"Bearer {GROQ_API_KEY}", "Content-Type": "application/json"}
    payload = {
        "model": "llama-3.3-70b-versatile",
        "messages": [{"role": "user", "content": prompt}],
        "temperature": 0.4,
    }
    async with httpx.AsyncClient(timeout=60) as client:
        r = await client.post(url, json=payload, headers=headers)
        if r.status_code != 200:
            raise HTTPException(status_code=502, detail=f"Groq error: {r.text}")
        return r.json()["choices"][0]["message"]["content"]


async def call_mistral(prompt: str) -> str:
    if not MISTRAL_API_KEY:
        raise HTTPException(status_code=500, detail="MISTRAL_API_KEY not set in environment.")
    import httpx
    url = "https://api.mistral.ai/v1/chat/completions"
    headers = {"Authorization": f"Bearer {MISTRAL_API_KEY}", "Content-Type": "application/json"}
    payload = {
        "model": "mistral-small-latest",
        "messages": [{"role": "user", "content": prompt}],
        "temperature": 0.4,
    }
    async with httpx.AsyncClient(timeout=60) as client:
        r = await client.post(url, json=payload, headers=headers)
        if r.status_code != 200:
            raise HTTPException(status_code=502, detail=f"Mistral error: {r.text}")
        return r.json()["choices"][0]["message"]["content"]


# ---------------------------------------------------------------------------
# JSON parser
# ---------------------------------------------------------------------------

def parse_questions(raw: str) -> list[dict]:
    # Extract JSON array from response (handles markdown code blocks)
    match = re.search(r'\[.*\]', raw, re.DOTALL)
    if not match:
        raise HTTPException(status_code=502, detail="AI response did not contain valid JSON array.")
    try:
        questions = json.loads(match.group())
        return questions
    except json.JSONDecodeError as e:
        raise HTTPException(status_code=502, detail=f"Failed to parse AI JSON: {e}")


# ---------------------------------------------------------------------------
# Endpoint
# ---------------------------------------------------------------------------

@router.post(
    "/{lesson_id}/generate-quiz",
    response_model=QuizGenerateResponse,
    summary="Generate MCQ quiz from lesson PDF using AI",
)
async def generate_quiz(
    lesson_id: str,
    body: QuizGenerateRequest,
    admin: VerifiedUser = Depends(require_admin),
) -> QuizGenerateResponse:
    # 1. Get lesson
    lesson_ref = db.collection("lessons").document(lesson_id)
    lesson_doc = lesson_ref.get()
    if not lesson_doc.exists:
        raise HTTPException(status_code=404, detail="Lesson not found.")

    # 2. Get material (PDF) from Firestore
    material_ref = lesson_ref.collection("materials").document(body.material_id)
    material_doc = material_ref.get()
    if not material_doc.exists:
        raise HTTPException(status_code=404, detail="Material not found.")

    import pathlib
    material_data = material_doc.to_dict()
    local_path = material_data.get("storagePath", "")
    if not local_path or not pathlib.Path(local_path).exists():
        raise HTTPException(status_code=404, detail="PDF file not found. Please re-upload the PDF.")

    # 3. Read PDF from local disk
    try:
        pdf_bytes = pathlib.Path(local_path).read_bytes()
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to read PDF: {e}")

    # 4. Extract text
    text = extract_text_from_pdf(pdf_bytes)
    if len(text) < 50:
        raise HTTPException(status_code=422, detail="PDF has too little text to generate questions.")

    # 5. Build prompt and call AI
    prompt = build_prompt(text, body.num_questions)

    if body.model == "gemini":
        raw = await call_gemini(prompt)
        model_label = "Gemini 1.5 Flash"
    elif body.model == "llama":
        raw = await call_groq_llama(prompt)
        model_label = "Llama 3.3 70B (Groq)"
    else:
        raw = await call_mistral(prompt)
        model_label = "Mistral Small"

    # 6. Parse response and enforce exact count
    raw_questions = parse_questions(raw)
    raw_questions = raw_questions[:body.num_questions]

    # 7. Reset quiz settings selectedQuestionIds (old IDs no longer valid after regeneration)
    settings_ref = lesson_ref.collection("quizSettings").document("config")
    if settings_ref.get().exists:
        settings_ref.update({"selectedQuestionIds": [], "updatedAt": firestore.SERVER_TIMESTAMP})

    # Delete existing questions first (replace, not append)
    existing = lesson_ref.collection("questions").stream()
    del_batch = db.batch()
    for doc in existing:
        del_batch.delete(doc.reference)
    del_batch.commit()

    # 8. Save new questions to Firestore: lessons/{id}/questions/
    saved = []
    batch = db.batch()
    for q in raw_questions:
        q_id = str(uuid.uuid4())
        q_ref = lesson_ref.collection("questions").document(q_id)
        options = q.get("options", {})
        doc = {
            "question": q.get("question", ""),
            "options": [
                {"label": "A", "text": options.get("A", "")},
                {"label": "B", "text": options.get("B", "")},
                {"label": "C", "text": options.get("C", "")},
                {"label": "D", "text": options.get("D", "")},
            ],
            "correct": q.get("correct", "A"),
            "explanation": q.get("explanation", ""),
            "generatedBy": body.model,
            "modelLabel": model_label,
            "createdAt": firestore.SERVER_TIMESTAMP,
        }
        batch.set(q_ref, doc)
        saved.append(MCQQuestion(
            id=q_id,
            question=doc["question"],
            options=[MCQOption(**o) for o in doc["options"]],
            correct=doc["correct"],
            explanation=doc["explanation"],
        ))

    batch.commit()

    return QuizGenerateResponse(
        lesson_id=lesson_id,
        model_used=model_label,
        questions_saved=len(saved),
        questions=saved,
    )
