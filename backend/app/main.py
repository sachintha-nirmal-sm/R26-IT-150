import sys
import io
import traceback
from contextlib import asynccontextmanager
from fastapi import FastAPI, Request
from fastapi.responses import Response, JSONResponse
from fastapi.middleware.cors import CORSMiddleware
from app.core import db

# Force UTF-8 on Windows so binary data in tracebacks never crashes the process
if hasattr(sys.stdout, 'buffer'):
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
if hasattr(sys.stderr, 'buffer'):
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup verification
    print("[FastAPI] App starting up...")
    try:
        _ = db.project
        print(f"[FastAPI] Firebase Admin SDK initialized successfully (Project: {db.project}).")
        from app.core.firebase import db as firestore_db
        from app.services.practical_catalogue import ensure_catalogue
        ensure_catalogue(firestore_db)
        print("[FastAPI] Practical catalogue ready in Firestore.")
    except Exception as e:
        print(f"[FastAPI Error] Firebase Admin SDK initialization check failed: {e}")
    yield
    print("[FastAPI] App shutting down...")

app = FastAPI(
    title="Physics Learning Platform API",
    description="FastAPI Backend for Physics Learning Platform (Role-Based Auth, RAG, Quiz Grading, Analytics)",
    version="1.0.0",
    lifespan=lifespan,
)

# Use standard CORSMiddleware for robust preflight and header handling
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=False,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Custom error handling middleware to safely catch and print tracebacks
@app.middleware("http")
async def error_handling_middleware(request: Request, call_next):
    try:
        response = await call_next(request)
        return response
    except Exception as exc:
        try:
            traceback.print_exc()
        except Exception:
            print(f"[error] {type(exc).__name__}: {repr(exc)[:200]}", file=sys.__stderr__)
        return JSONResponse(status_code=500, content={"detail": "Internal server error"})

# --- Routers ---
from app.api.auth import router as auth_router
from app.api.admin_lessons import router as admin_lessons_router
from app.api.admin_sub_lessons import router as sub_lessons_router
from app.api.generate_questions import router as generate_router
from app.api.ml_analytics import router as ml_analytics_router
from app.api.recommendations import router as recommendations_router
from app.api import practicals

app.include_router(auth_router)
app.include_router(admin_lessons_router)
app.include_router(sub_lessons_router)
app.include_router(generate_router)
app.include_router(ml_analytics_router)
app.include_router(recommendations_router)
app.include_router(practicals.router)

@app.get("/health", tags=["Health"])
def health_check():
    """Health check endpoint to verify backend operational status and Firebase Admin connection."""
    return {
        "status": "online",
        "firebase_connected": db is not None,
        "firebase_project": db.project if db else None,
    }

@app.get("/")
def health():
    return {"status": "ok"}