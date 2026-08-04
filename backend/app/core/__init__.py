from app.core.config import PROJECT_ID, get_service_account_path
from app.core.firebase import db, auth, get_db, get_auth
from app.core.dependencies import VerifiedUser, require_auth, require_admin, require_student

__all__ = [
    "PROJECT_ID", "get_service_account_path",
    "db", "auth", "get_db", "get_auth",
    "VerifiedUser", "require_auth", "require_admin", "require_student",
]
