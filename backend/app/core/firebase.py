import os
import firebase_admin
from firebase_admin import credentials, firestore, auth as firebase_auth
from app.core.config import get_service_account_path

_service_account_path = get_service_account_path()

if not firebase_admin._apps:
    if not os.path.exists(_service_account_path):
        raise FileNotFoundError(
            f"\n\n[Firebase Init Error] serviceAccountKey.json not found at:\n"
            f"  {_service_account_path}\n\n"
            "Please ensure serviceAccountKey.json is placed in firebase/scripts/ or backend/ folder."
        )
    _cred = credentials.Certificate(_service_account_path)
    firebase_admin.initialize_app(_cred)

# Expose Firestore client and Firebase Auth module
db: firestore.Client = firestore.client()
auth = firebase_auth

def get_db() -> firestore.Client:
    """Dependency helper to return Firestore DB client."""
    return db

def get_auth():
    """Dependency helper to return Firebase Auth module."""
    return auth
