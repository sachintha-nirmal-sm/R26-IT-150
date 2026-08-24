"""
api/auth.py — Authentication & bootstrap endpoints.

Implements (per Section 17 and Section 1 of System Architecture Extension):
  - POST /auth/signup  — student-only public registration. Creates a Firebase Auth user
                         and a users/{uid} Firestore document with role HARDCODED to "student".
                         No request parameter can ever produce role "admin".
  - GET  /auth/bootstrap — verifies the Firebase ID token, reads users/{uid} to confirm
                           role and account status, and returns the Flutter boot payload.
"""

from datetime import datetime, timezone

import firebase_admin.auth as firebase_auth
from fastapi import APIRouter, Depends, HTTPException, status
from google.cloud import firestore
from pydantic import BaseModel, EmailStr, Field, field_validator

from app.core.dependencies import VerifiedUser, require_auth
from app.core.firebase import auth, db

router = APIRouter(prefix="/auth", tags=["Authentication"])


# ---------------------------------------------------------------------------
# Request / Response models
# ---------------------------------------------------------------------------

class SignupRequest(BaseModel):
    """
    Body for POST /auth/signup.
    
    'role' is intentionally absent — the endpoint always creates a student account.
    No caller can elevate the role to 'admin' via any field or bypass.
    """
    email: EmailStr = Field(..., description="Student's email address.")
    password: str = Field(..., min_length=6, description="Password (minimum 6 characters).")
    fullName: str = Field(..., min_length=1, max_length=100, description="Student's full name.")
    currentGrade: int = Field(..., ge=9, le=12, description="Student's grade (9, 10, 11, or 12).")
    enrollmentYear: int = Field(..., ge=2000, le=2100, description="Year of enrollment.")

    @field_validator("fullName")
    @classmethod
    def name_must_not_be_blank(cls, v: str) -> str:
        if not v.strip():
            raise ValueError("fullName must not be blank or whitespace only.")
        return v.strip()


class SignupResponse(BaseModel):
    """Returned after successful student registration."""
    uid: str
    role: str           # Always "student" — returned so Flutter can route immediately
    fullName: str
    email: str
    currentGrade: int


class BootstrapResponse(BaseModel):
    """
    Bootstrap payload returned to Flutter after login.
    Flutter uses 'role' — and ONLY 'role' — to decide which interface to render.
    currentGrade is omitted for admin accounts.
    """
    uid: str
    role: str
    fullName: str
    email: str | None
    currentGrade: int | None = None


# ---------------------------------------------------------------------------
# POST /auth/signup  —  student-only registration
# ---------------------------------------------------------------------------

@router.post(
    "/signup",
    response_model=SignupResponse,
    status_code=status.HTTP_201_CREATED,
    summary="Register a new student account",
    description=(
        "Public endpoint — no token required. "
        "Creates a Firebase Auth user and a Firestore users/{uid} document. "
        "**Role is always set to 'student' server-side — no request parameter can override this.** "
        "Admin accounts are never created through this endpoint."
    ),
)
async def signup(body: SignupRequest) -> SignupResponse:
    """
    Student-only sign-up flow (Section 17 + Section 1 of System Architecture Extension,
    and Section 3.1 of Firestore Database Architecture).

    Steps:
      1. Create Firebase Auth user with email + password.
      2. Set custom claim { "role": "student" } on the new user.
         Role is HARDCODED — the claim can never be "admin" from this endpoint.
      3. Write users/{uid} Firestore document with all Section 3.1 fields.
      4. Return the minimal payload so Flutter can navigate immediately.
    """
    # --- 1. Create Firebase Auth user ---
    try:
        new_user = auth.create_user(
            email=body.email,
            password=body.password,
            display_name=body.fullName,
        )
    except firebase_auth.EmailAlreadyExistsError:
        raise HTTPException(
            status_code=status.HTTP_409_CONFLICT,
            detail="An account with this email address already exists.",
        )
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to create Firebase Auth user: {e}",
        )

    uid = new_user.uid

    # --- 2. Set custom claim { role: "student" } — HARDCODED, never "admin" ---
    try:
        auth.set_custom_user_claims(uid, {"role": "student"})
    except Exception as e:
        # Auth user was created — attempt to delete it to avoid orphaned Auth accounts
        try:
            auth.delete_user(uid)
        except Exception:
            pass
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to set role claim on Auth user: {e}",
        )

    # --- 3. Write users/{uid} Firestore document (Section 3.1 schema) ---
    now = firestore.SERVER_TIMESTAMP
    user_doc = {
        # Security-critical fields — set server-side, never from client input
        "role": "student",                  # HARDCODED — cannot be overridden
        "status": "active",

        # Identity fields from request body
        "fullName": body.fullName,
        "email": body.email,

        # Student-specific fields
        "currentGrade": body.currentGrade,
        "enrollmentYear": body.enrollmentYear,
        "lastPromotedAt": None,

        # Timestamps
        "createdAt": now,
        "updatedAt": now,
    }

    try:
        db.collection("users").document(uid).set(user_doc)
    except Exception as e:
        # Firestore write failed — clean up the Auth user to avoid an orphaned account
        try:
            auth.delete_user(uid)
        except Exception:
            pass
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to write user profile to Firestore: {e}",
        )

    return SignupResponse(
        uid=uid,
        role="student",     # Always literal "student" — never derived from request body
        fullName=body.fullName,
        email=body.email,
        currentGrade=body.currentGrade,
    )


# ---------------------------------------------------------------------------
# GET /auth/bootstrap  —  post-login user context fetch
# ---------------------------------------------------------------------------

@router.get(
    "/bootstrap",
    response_model=BootstrapResponse,
    summary="Bootstrap user session after login",
    description=(
        "Called once after Firebase sign-in. Requires a valid Firebase ID token in the "
        "Authorization header (Bearer scheme). Reads users/{uid} to confirm role and "
        "account status, then returns the minimal user info Flutter needs to render the "
        "correct interface."
    ),
)
async def bootstrap(user: VerifiedUser = Depends(require_auth)) -> BootstrapResponse:
    """
    Section 1, Steps 3–5 of System Architecture Extension.

    Token is already verified by the require_auth dependency (uid + role extracted).
    This handler then reads users/{uid} to:
      - Reject suspended accounts with 403.
      - Confirm the Firestore role matches the token claim (defence-in-depth).
      - Return { uid, role, fullName, currentGrade? } to Flutter.
    """
    user_snap = db.collection("users").document(user.uid).get()

    if not user_snap.exists:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"User profile not found in Firestore for uid={user.uid}. "
                   "The account may not have been fully provisioned.",
        )

    data = user_snap.to_dict()

    # Reject suspended accounts
    account_status = data.get("status", "active")
    if account_status == "suspended":
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Your account has been suspended. Please contact an administrator.",
        )

    # Confirm Firestore role matches the token claim (defence-in-depth check)
    firestore_role = data.get("role")
    if firestore_role != user.role:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail=(
                "Role mismatch: the Auth token claim does not match the Firestore profile. "
                "Please contact an administrator."
            ),
        )

    return BootstrapResponse(
        uid=user.uid,
        role=user.role,
        fullName=data.get("fullName", ""),
        email=data.get("email", user.email),
        # currentGrade only returned for students; absent for admin accounts
        currentGrade=data.get("currentGrade") if user.role == "student" else None,
    )
