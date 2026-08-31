"""
dependencies.py — FastAPI reusable dependencies for Firebase token verification.

Per Section 1 of System Architecture Extension:
  - Every request carries a Firebase ID token in the Authorization header (Bearer scheme).
  - FastAPI verifies the token using the Admin SDK, extracts uid + role custom claim.
  - If the token is missing, invalid, or expired, the request is rejected with 401.
  - The verified uid and role are attached to the request via a VerifiedUser dataclass
    so all downstream route handlers can access them without re-verifying.

Usage in a route:
    from app.core.dependencies import require_auth, require_admin, require_student
    
    @router.get("/my-route")
    def my_route(user: VerifiedUser = Depends(require_auth)):
        return {"uid": user.uid, "role": user.role}
"""

from dataclasses import dataclass
from typing import Literal

from fastapi import Depends, HTTPException, status
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from firebase_admin.auth import (
    ExpiredIdTokenError,
    InvalidIdTokenError,
    RevokedIdTokenError,
    CertificateFetchError,
)

from app.core.auth_users import (
    load_user_profile,
    profile_grade,
    service_account_project_id,
    verify_flutter_id_token,
)
from app.core.config import FIREBASE_AUTH_PROJECT_ID
from app.core.firebase import auth
from app.core.firebase import db as firestore_db

# Enable OpenAPI HTTPBearer security scheme (renders "Authorize" button in Swagger UI)
security_scheme = HTTPBearer(auto_error=False)


# ---------------------------------------------------------------------------
# Verified user dataclass — attached to request context after token check
# ---------------------------------------------------------------------------

@dataclass
class VerifiedUser:
    """
    Carries the verified identity of the authenticated caller.

    Attributes:
        uid           : Firebase Auth UID (document ID in users/{uid}).
        role          : Custom claim value — either "admin" or "student".
        email         : Email address from the ID token (informational).
        current_grade : Syllabus grade from the login project's user profile.
    """
    uid: str
    role: Literal["admin", "student"]
    email: str | None
    current_grade: int | None = None


# ---------------------------------------------------------------------------
# Core token verification dependency
# ---------------------------------------------------------------------------

async def require_auth(
    credentials: HTTPAuthorizationCredentials | None = Depends(security_scheme)
) -> VerifiedUser:
    """
    FastAPI dependency — verifies the Firebase ID token from the Authorization header.

    Expected header format:
        Authorization: Bearer <firebase_id_token>

    Steps:
      1. Extract the token from the Bearer security scheme.
      2. Verify the token signature, expiry, and revocation via Firebase Admin SDK.
      3. Extract uid and the 'role' custom claim from the decoded token.
      4. Reject with 401 if:
           - Header is missing or empty
           - Token is expired or revoked
           - Token signature is invalid
           - 'role' custom claim is absent or has an unrecognised value

    Returns:
        VerifiedUser with uid, role, and email.

    Raises:
        HTTPException(401) on any verification failure.
    """
    # 1. Verify token is present
    if not credentials or not credentials.credentials or not credentials.credentials.strip():
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Authorization token is missing. Please provide 'Authorization: Bearer <token>'.",
            headers={"WWW-Authenticate": "Bearer"},
        )
    
    id_token = credentials.credentials.strip()

    # 2. Verify token against the Flutter Firebase project (physicslab-eaa8a),
    #    not only the Admin SDK service-account project.
    try:
        decoded_token = verify_flutter_id_token(id_token)
    except ExpiredIdTokenError:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Firebase ID token has expired. Please sign in again.",
            headers={"WWW-Authenticate": "Bearer"},
        )
    except RevokedIdTokenError:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Firebase ID token has been revoked. Please sign in again.",
            headers={"WWW-Authenticate": "Bearer"},
        )
    except InvalidIdTokenError as e:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail=f"Firebase ID token is invalid: {e}",
            headers={"WWW-Authenticate": "Bearer"},
        )
    except CertificateFetchError:
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="Could not fetch Firebase public keys to verify token. Try again shortly.",
        )
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail=f"Token verification failed: {e}",
            headers={"WWW-Authenticate": "Bearer"},
        )

    # 3. Extract uid and role custom claim
    uid: str = decoded_token.get("uid") or decoded_token.get("user_id") or decoded_token.get("sub")
    role: str | None = decoded_token.get("role")
    email: str | None = decoded_token.get("email")
    auth_project = decoded_token.get("_auth_project") or FIREBASE_AUTH_PROJECT_ID

    if not uid:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Token is missing the 'uid' field.",
            headers={"WWW-Authenticate": "Bearer"},
        )

    profile = load_user_profile(uid, id_token, auth_project)
    if role not in ("admin", "student"):
        role = profile.get("role")
        if role not in ("admin", "student"):
            role = "student"
            admin_project = service_account_project_id()
            # Only write a stub profile when Auth and Admin share the same project.
            # Otherwise we would create a Grade-10 user in the wrong Firebase project.
            if admin_project and auth_project == admin_project:
                try:
                    auth.set_custom_user_claims(uid, {"role": "student"})
                except Exception:
                    pass
                firestore_db.collection("users").document(uid).set(
                    {
                        "role": "student",
                        "status": profile.get("status") or "active",
                        "currentGrade": profile.get("currentGrade") or 10,
                        "email": profile.get("email") or email or "",
                        "fullName": profile.get("fullName") or "",
                    },
                    merge=True,
                )

    return VerifiedUser(
        uid=uid,
        role=role,
        email=email,
        current_grade=profile_grade(profile),
    )


# ---------------------------------------------------------------------------
# Role-scoped convenience dependencies
# ---------------------------------------------------------------------------

async def require_admin(user: VerifiedUser = Depends(require_auth)) -> VerifiedUser:
    """
    Dependency that allows only admin users through.
    Raises 403 Forbidden for student role or any unexpected role.
    """
    if user.role != "admin":
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Admin access required.",
        )
    return user


async def require_student(user: VerifiedUser = Depends(require_auth)) -> VerifiedUser:
    """
    Dependency that allows only student users through.
    Raises 403 Forbidden for admin role or any unexpected role.
    """
    if user.role != "student":
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Student access required.",
        )
    return user
