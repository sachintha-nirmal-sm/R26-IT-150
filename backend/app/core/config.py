import os
from pathlib import Path
from dotenv import load_dotenv

# Load environment variables from .env if present
load_dotenv()

# Path definitions
APP_DIR = Path(__file__).resolve().parent.parent
BACKEND_DIR = APP_DIR.parent
ROOT_DIR = BACKEND_DIR.parent

PROJECT_ID: str = os.getenv("FIREBASE_PROJECT_ID", "physicslab-eaa8a")

# New Firebase projects (created after Oct 2024) use firebasestorage.app
# Old projects used appspot.com. Override via STORAGE_BUCKET env var if needed.
STORAGE_BUCKET: str = os.getenv(
    "STORAGE_BUCKET",
    f"{PROJECT_ID}.appspot.com",
)

# Candidate paths for serviceAccountKey.json
DEFAULT_SERVICE_ACCOUNT_PATHS = [
    BACKEND_DIR / "serviceAccountKey.json",
    ROOT_DIR / "firebase" / "scripts" / "serviceAccountKey.json",
    APP_DIR / "serviceAccountKey.json",
]

def get_service_account_path() -> str:
    env_path = os.getenv("GOOGLE_APPLICATION_CREDENTIALS")
    if env_path and os.path.exists(env_path):
        return env_path

    for candidate in DEFAULT_SERVICE_ACCOUNT_PATHS:
        if candidate.exists():
            return str(candidate)

    # Return default expected path if none found (will raise clear error on init)
    return str(DEFAULT_SERVICE_ACCOUNT_PATHS[1])


# --- RAG (embeddings stay on disk, not in Firestore) ---
VECTOR_STORE_DIR: Path = Path(
    os.getenv("VECTOR_STORE_DIR", str(BACKEND_DIR / "data" / "vector_store"))
)
VECTOR_STORE_DIR.mkdir(parents=True, exist_ok=True)
LOCAL_UPLOAD_DIR: Path = Path(
    os.getenv("LOCAL_UPLOAD_DIR", str(BACKEND_DIR / "data" / "uploads"))
)
LOCAL_UPLOAD_DIR.mkdir(parents=True, exist_ok=True)
EMBEDDING_MODEL: str = os.getenv("EMBEDDING_MODEL", "all-MiniLM-L6-v2")
RAG_CHUNK_SIZE: int = int(os.getenv("RAG_CHUNK_SIZE", "800"))
RAG_CHUNK_OVERLAP: int = int(os.getenv("RAG_CHUNK_OVERLAP", "120"))
RAG_TOP_K: int = int(os.getenv("RAG_TOP_K", "4"))
RAG_QUESTIONS_PER_BANK: int = int(os.getenv("RAG_QUESTIONS_PER_BANK", "9"))
OLLAMA_BASE_URL: str = os.getenv("OLLAMA_BASE_URL", "http://127.0.0.1:11434")
OLLAMA_MODEL: str = os.getenv("OLLAMA_MODEL", "llama3.1")
