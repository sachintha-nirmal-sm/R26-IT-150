from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from app.core import db
from app.api import auth as auth_router
from app.api import admin_lessons
from app.api import admin_quizzes
from app.api import admin_final_quiz
from app.api import student
from app.api import analytics
from app.api import learning_path
from app.api import admin_rag
from app.api import chatbot


@asynccontextmanager
async def lifespan(app: FastAPI):
    print("[FastAPI] App starting up...")
    try:
        _ = db.project
        print(f"[FastAPI] Firebase Admin SDK initialized (Project: {db.project}).")
    except Exception as e:
        print(f"[FastAPI Error] Firebase Admin SDK initialization check failed: {e}")
    yield
    print("[FastAPI] App shutting down...")


app = FastAPI(
    title="Physics Learning Platform API",
    description=(
        "FastAPI + Firestore backend for the grade-based physics learning platform: "
        "auth, lesson/material admin, quiz start/submit (server-side grading), "
        "weak-topic analytics, feedback, and RAG generation jobs."
    ),
    version="1.0.0",
    lifespan=lifespan,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=False,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(auth_router.router)
app.include_router(admin_lessons.router)
app.include_router(admin_quizzes.router)
app.include_router(admin_final_quiz.router)
app.include_router(student.router)
app.include_router(analytics.router)
app.include_router(learning_path.router)
app.include_router(admin_rag.router)
app.include_router(chatbot.router)


@app.get("/", tags=["Health"])
def root():
    return {
        "status": "online",
        "docs": "/docs",
        "health": "/health",
        "chat": "POST /chat/rag",
    }


@app.get("/health", tags=["Health"])
def health_check():
    return {
        "status": "online",
        "firebase_connected": db is not None,
        "firebase_project": db.project if db else None,
    }
