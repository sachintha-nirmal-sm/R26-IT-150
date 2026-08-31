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
    f"{PROJECT_ID}.firebasestorage.app"
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
