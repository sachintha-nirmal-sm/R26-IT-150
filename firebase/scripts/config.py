"""
config.py — Shared configuration re-export for Admin SDK scripts.

Imports from firebase_init.py (which owns SDK initialisation).
Use either this module or firebase_init directly — both expose the same clients.

    from config import db, auth, PROJECT_ID
    # or equivalently:
    from firebase_init import db, auth
"""

import os
from dotenv import load_dotenv

# ── Load .env file if present (optional, for local dev) ───────────────────────
load_dotenv()

# ── Project metadata ──────────────────────────────────────────────────────────
PROJECT_ID: str = os.getenv("FIREBASE_PROJECT_ID", "physics-learning-platform")

# ── Re-export SDK clients from firebase_init ──────────────────────────────────
from firebase_init import db, auth  # noqa: E402  (import after env load is intentional)

__all__ = ["PROJECT_ID", "db", "auth"]
