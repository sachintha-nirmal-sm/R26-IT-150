import io
import sys
import traceback
from contextlib import asynccontextmanager

from fastapi import FastAPI, Request
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse

from app.core import db

from app.api.auth import router as auth_router
from app.api.admin_lessons import router as admin_lessons_router
from app.api.admin_sub_lessons import router as sub_lessons_router
from app.api.generate_questions import router as generate_router
from app.api.ml_analytics import router as ml_analytics_router
from app.api.recommendations import router as recommendations_router
from app.api.practicals import router as practicals_router

from app.api.admin_quizzes import router as admin_quizzes_router
from app.api.admin_final_quiz import router as admin_final_quiz_router
from app.api.student import router as student_router
from app.api.analytics import router as analytics_router
from app.api.learning_path import router as learning_path_router
from app.api.admin_rag import router as admin_rag_router
from app.api.chatbot import router as chatbot_router


# Force UTF-8 on Windows so binary data in tracebacks never crashes the process.
if hasattr(sys.stdout, "buffer"):
    sys.stdout = io.TextIOWrapper(
        sys.stdout.buffer,
        encoding="utf-8",
        errors="replace",
    )

if hasattr(sys.stderr, "buffer"):
    sys.stderr = io.TextIOWrapper(
        sys.stderr.buffer,
        encoding="utf-8",
        errors="replace",
    )


@asynccontextmanager
async def lifespan(app: FastAPI):
    print("[FastAPI] App starting up...")

    try:
        _ = db.project
        print(
            "[FastAPI] Firebase Admin SDK initialized successfully "
            f"(Project: {db.project})."
        )

        # Preserve develop_main startup behaviour.
        from app.core.firebase import db as firestore_db
        from app.services.practical_catalogue import ensure_catalogue

        ensure_catalogue(firestore_db)
        print("[FastAPI] Practical catalogue ready in Firestore.")

    except Exception as exc:
        print(
            "[FastAPI Error] Firebase Admin SDK initialization "
            f"check failed: {exc}"
        )

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


# Use standard CORSMiddleware for robust preflight and header handling.
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=False,
    allow_methods=["*"],
    allow_headers=["*"],
)


# Custom error handling middleware to safely catch and print tracebacks.
@app.middleware("http")
async def error_handling_middleware(request: Request, call_next):
    try:
        return await call_next(request)
    except Exception as exc:
        try:
            traceback.print_exc()
        except Exception:
            print(
                f"[error] {type(exc).__name__}: {repr(exc)[:200]}",
                file=sys.__stderr__,
            )

        return JSONResponse(
            status_code=500,
            content={"detail": "Internal server error"},
        )


# ---------------------------------------------------------------------------
# Routers from develop_main
# ---------------------------------------------------------------------------

app.include_router(auth_router)
app.include_router(admin_lessons_router)
app.include_router(sub_lessons_router)
app.include_router(generate_router)
app.include_router(ml_analytics_router)
app.include_router(recommendations_router)
app.include_router(practicals_router)


# ---------------------------------------------------------------------------
# Routers from AI-Learning-Support-System
# ---------------------------------------------------------------------------

app.include_router(admin_quizzes_router)
app.include_router(admin_final_quiz_router)
app.include_router(student_router)
app.include_router(analytics_router)
app.include_router(learning_path_router)
app.include_router(admin_rag_router)
app.include_router(chatbot_router)


# ---------------------------------------------------------------------------
# Health / root endpoints
# ---------------------------------------------------------------------------

@app.get("/", tags=["Health"])
def root():
    """
    Combined root response.

    The two branches both defined GET /, which cannot remain as two separate
    handlers without creating ambiguous routing. This single handler preserves
    the useful root information from both versions.
    """
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
