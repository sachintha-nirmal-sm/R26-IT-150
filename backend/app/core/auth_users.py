"""Verify Flutter ID tokens and load the signed-in student's profile."""

from __future__ import annotations

import json
from pathlib import Path

import httpx
from google.auth.exceptions import GoogleAuthError
from google.auth.transport import requests as google_requests
from google.oauth2 import id_token as google_id_token
from firebase_admin.auth import (
    CertificateFetchError,
    ExpiredIdTokenError,
    InvalidIdTokenError,
    RevokedIdTokenError,
    verify_id_token,
)

from app.core.config import FIREBASE_AUTH_PROJECT_ID, get_service_account_path
from app.core.firebase import db
from app.core.grade import parse_grade

_google_request = google_requests.Request()


def service_account_project_id() -> str | None:
    path = get_service_account_path()
    if not path or not Path(path).exists():
        return None
    try:
        data = json.loads(Path(path).read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError):
        return None
    project = data.get("project_id")
    return str(project) if project else None


def verify_flutter_id_token(id_token: str) -> dict:
    """
    Accept tokens from the Flutter Firebase project first.

    The Admin SDK key may belong to a different project, which would reject
    Flutter tokens with an 'aud' mismatch. Google public-key verification
    uses FIREBASE_AUTH_PROJECT_ID (physicslab-eaa8a) as the audience.
    """
    try:
        decoded = google_id_token.verify_firebase_token(
            id_token,
            _google_request,
            audience=FIREBASE_AUTH_PROJECT_ID,
        )
        decoded["_auth_project"] = FIREBASE_AUTH_PROJECT_ID
        if not decoded.get("uid"):
            decoded["uid"] = decoded.get("user_id") or decoded.get("sub")
        return decoded
    except (ValueError, GoogleAuthError) as exc:
        text = str(exc)
        # Fall back to Admin SDK (same project as the service account).
        try:
            decoded = verify_id_token(id_token, check_revoked=True)
            decoded["_auth_project"] = service_account_project_id() or ""
            return decoded
        except ExpiredIdTokenError:
            raise
        except RevokedIdTokenError:
            raise
        except CertificateFetchError:
            raise
        except InvalidIdTokenError as admin_exc:
            raise InvalidIdTokenError(
                f'{text} Admin SDK also rejected the token: {admin_exc}. '
                f'The Flutter app uses project "{FIREBASE_AUTH_PROJECT_ID}". '
                "If this keeps failing, download a service account key for that "
                "project and save it as backend/serviceAccountKey.json."
            ) from admin_exc


def _decode_firestore_value(node):
    if not isinstance(node, dict):
        return node
    if "stringValue" in node:
        return node["stringValue"]
    if "integerValue" in node:
        return int(node["integerValue"])
    if "doubleValue" in node:
        return float(node["doubleValue"])
    if "booleanValue" in node:
        return node["booleanValue"]
    if "nullValue" in node:
        return None
    if "mapValue" in node:
        fields = (node.get("mapValue") or {}).get("fields") or {}
        return {key: _decode_firestore_value(val) for key, val in fields.items()}
    if "arrayValue" in node:
        values = (node.get("arrayValue") or {}).get("values") or []
        return [_decode_firestore_value(val) for val in values]
    return None


def _profile_via_rest(uid: str, id_token: str, project_id: str) -> dict:
    url = (
        f"https://firestore.googleapis.com/v1/projects/{project_id}"
        f"/databases/(default)/documents/users/{uid}"
    )
    response = httpx.get(
        url,
        headers={"Authorization": f"Bearer {id_token}"},
        timeout=15.0,
    )
    if response.status_code == 404:
        return {}
    response.raise_for_status()
    payload = response.json()
    fields = payload.get("fields") or {}
    return {key: _decode_firestore_value(val) for key, val in fields.items()}


def load_user_profile(uid: str, id_token: str, auth_project: str) -> dict:
    """Read users/{uid} from the project the student actually logged into."""
    admin_project = service_account_project_id()
    if auth_project and admin_project and auth_project == admin_project:
        snap = db.collection("users").document(uid).get()
        return snap.to_dict() if snap.exists else {}

    try:
        profile = _profile_via_rest(uid, id_token, auth_project or FIREBASE_AUTH_PROJECT_ID)
        if profile:
            return profile
    except Exception:
        pass

    snap = db.collection("users").document(uid).get()
    return snap.to_dict() if snap.exists else {}


def profile_grade(profile: dict | None) -> int | None:
    data = profile or {}
    return parse_grade(data.get("currentGrade")) or parse_grade(data.get("grade"))
