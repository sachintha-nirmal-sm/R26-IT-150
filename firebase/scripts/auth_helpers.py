"""
auth_helpers.py — Reusable Firebase Auth helper functions.

Import in any Admin SDK script:
    from auth_helpers import set_role_claim, create_auth_user
"""

import sys
import os

# Allow imports from the scripts/ parent directory
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from firebase_init import auth


def set_role_claim(uid: str, role: str) -> None:
    """
    Set the Firebase Auth custom claim { "role": <role> } on a given user.

    Per architecture Section 3.1 and Section 10 (devmini.md):
      - The custom claim is the SINGLE source of truth for Admin vs. Student access.
      - Firestore Security Rules read request.auth.token.role directly from this claim.
      - The claim value must always mirror the users/{uid}.role Firestore field.
      - Valid values: "student" | "admin"

    Args:
        uid  : Firebase Auth UID of the user to update.
        role : "student" or "admin" — any other value raises ValueError.

    Raises:
        ValueError         : if role is not "student" or "admin".
        firebase_admin.auth.UserNotFoundError : if the uid does not exist in Auth.

    Note:
        Custom claim changes take effect on the user's NEXT token refresh (up to 1 hour).
        To force immediate effect during development, sign the user out and back in.
    """
    allowed_roles = {"student", "admin"}
    if role not in allowed_roles:
        raise ValueError(
            f"Invalid role '{role}'. Must be one of: {allowed_roles}"
        )

    auth.set_custom_user_claims(uid, {"role": role})
    print(f"  [auth] Custom claim set -> uid={uid}  role={role}")


def create_auth_user(email: str, password: str, display_name: str) -> str:
    """
    Create a Firebase Auth user and return their UID.

    If a user with the given email already exists, returns the existing UID
    without raising an error (idempotent for seed scripts).

    Args:
        email        : User's email address.
        password     : Initial password (min 6 chars).
        display_name : Display name shown in Firebase Console.

    Returns:
        uid (str) — Firebase Auth UID of the created or existing user.
    """
    try:
        user = auth.create_user(
            email=email,
            password=password,
            display_name=display_name,
        )
        print(f"  [auth] Created user -> uid={user.uid}  email={email}")
        return user.uid
    except auth.EmailAlreadyExistsError:
        # Already exists — fetch and return the existing UID
        existing = auth.get_user_by_email(email)
        print(f"  [auth] User already exists -> uid={existing.uid}  email={email}")
        return existing.uid
