from contextlib import asynccontextmanager
from fastapi import FastAPI
from app.core import db

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup verification
    print("[FastAPI] App starting up...")
    try:
        # Verify Firestore connection by listing top-level collections or checking client
        _ = db.project
        print(f"[FastAPI] Firebase Admin SDK initialized successfully (Project: {db.project}).")
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

@app.get("/health", tags=["Health"])
def health_check():
    """Health check endpoint to verify backend operational status and Firebase Admin connection."""
    return {
        "status": "online",
        "firebase_connected": db is not None,
        "firebase_project": db.project if db else None,
    }
