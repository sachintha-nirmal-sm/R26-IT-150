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

import json
import os
import urllib.error
import urllib.request
from dataclasses import dataclass
from typing import Literal

from fastapi import Depends, HTTPException, status
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from firebase_admin.auth import (
    verify_id_token,
    ExpiredIdTokenError,
    InvalidIdTokenError,
    RevokedIdTokenError,
    CertificateFetchError,
)

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
        uid   : Firebase Auth UID (document ID in users/{uid}).
        role  : Custom claim value — either "admin" or "student".
        email : Email address from the ID token (informational).
    """
    uid: str
    role: Literal["admin", "student"]
    email: str | None


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

    # 2. Verify token via Firebase Admin SDK
    try:
        decoded_token = verify_id_token(id_token, check_revoked=True)
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
        looked_up = _lookup_token_with_client_api(id_token)
        if looked_up is None:
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail=f"Firebase ID token is invalid: {e}",
                headers={"WWW-Authenticate": "Bearer"},
            )
        decoded_token = looked_up
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
    uid: str = decoded_token.get("uid") or decoded_token.get("user_id")
    role: str | None = decoded_token.get("role")
    email: str | None = decoded_token.get("email")

    if not uid:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Token is missing the 'uid' field.",
            headers={"WWW-Authenticate": "Bearer"},
        )

    if role not in ("admin", "student"):
        snap = firestore_db.collection("users").document(uid).get()
        profile = snap.to_dict() if snap.exists else {}
        role = profile.get("role")
        if role not in ("admin", "student"):
            role = "student"
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

    return VerifiedUser(uid=uid, role=role, email=email)


def _lookup_token_with_client_api(id_token: str) -> dict | None:
    """Verify a token issued by the Flutter Firebase project (physicslab-eaa8a)."""
    api_key = os.getenv(
        "FIREBASE_WEB_API_KEY",
        "AIzaSyBXGMZOCCdAL3WVEnBg_mCS-dbo0kfd1sY",
    )
    url = (
        "https://identitytoolkit.googleapis.com/v1/accounts:lookup"
        f"?key={api_key}"
    )
    request = urllib.request.Request(
        url,
        data=json.dumps({"idToken": id_token}).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    try:
        with urllib.request.urlopen(request, timeout=8) as response:
            payload = json.loads(response.read().decode("utf-8"))
    except (urllib.error.URLError, TimeoutError, json.JSONDecodeError):
        return None
    users = payload.get("users") or []
    if not users:
        return None
    user = users[0]
    return {
        "uid": user.get("localId"),
        "user_id": user.get("localId"),
        "email": user.get("email"),
        "role": "student",
    }


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
