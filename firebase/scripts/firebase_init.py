"""
firebase_init.py — Firebase Admin SDK initializer.

Usage in any other script:
    from firebase_init import db, auth

This module:
  1. Loads the service account key from scripts/serviceAccountKey.json
  2. Initialises the Firebase Admin SDK (safe to import multiple times — idempotent)
  3. Exposes:
       db   → google.cloud.firestore.Client  (Firestore)
       auth → firebase_admin.auth            (Firebase Authentication)

Service account key location:
  Place your downloaded key at:
      firebase/scripts/serviceAccountKey.json

  To download it:
    Firebase Console → Project Settings → Service Accounts
    → "Generate new private key" → save as serviceAccountKey.json here.

  ⚠️  This file is in .gitignore — NEVER commit it to version control.
"""

import os
import firebase_admin
from firebase_admin import credentials, firestore, auth as _auth

# ── Resolve path to the service account key ───────────────────────────────────
# Expected location: same directory as this file (scripts/serviceAccountKey.json)
_SCRIPTS_DIR = os.path.dirname(os.path.abspath(__file__))
_SERVICE_ACCOUNT_PATH = os.environ.get(
    "GOOGLE_APPLICATION_CREDENTIALS",
    os.path.join(_SCRIPTS_DIR, "serviceAccountKey.json"),
)

# ── Initialize Firebase Admin SDK (idempotent — safe to import multiple times) ─
if not firebase_admin._apps:
    if not os.path.exists(_SERVICE_ACCOUNT_PATH):
        raise FileNotFoundError(
            f"\n\n[firebase_init] serviceAccountKey.json not found at:\n"
            f"  {_SERVICE_ACCOUNT_PATH}\n\n"
            "To fix this:\n"
            "  1. Go to Firebase Console → Project Settings → Service Accounts\n"
            "  2. Click 'Generate new private key'\n"
            "  3. Save the downloaded file as 'serviceAccountKey.json' in the scripts/ folder\n"
        )
    _cred = credentials.Certificate(_SERVICE_ACCOUNT_PATH)
    firebase_admin.initialize_app(_cred)

# ── Public clients (import these in other scripts) ────────────────────────────
db: firestore.Client = firestore.client()
"""Firestore client — use for all database reads/writes."""

auth = _auth
"""Firebase Auth module — use to create users, set custom claims, etc."""
