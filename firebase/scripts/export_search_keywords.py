"""
export_search_keywords.py — downloads a fresh CSV of the live
search_keyword_index (see keyword_aggregate.py) to
firebase/scripts/search_keywords_export.csv.

This is a point-in-time snapshot, not a live file — the actual search
feature always reads search_keyword_index directly from Firestore, which
updates automatically the moment a new PDF is uploaded (see
admin_lessons.py's index_lesson_material_for_search, which calls
apply_material_keywords() right after extracting a new material's
keywords). Re-run this script whenever you want an up-to-date CSV copy;
there's no need to run it for the live feature itself to stay current.

Usage:
    python export_search_keywords.py [admin_email] [admin_password]

Defaults to the project's seeded admin account if no args are given.
"""

import json
import os
import sys
import urllib.request

API_KEY = os.getenv("FIREBASE_WEB_API_KEY", "AIzaSyC4FbGuVggiEsn-UAl5TTTvaUr-g2toKHc")
BACKEND_URL = os.getenv("BACKEND_URL", "http://localhost:9000")
OUT_PATH = os.path.join(os.path.dirname(os.path.abspath(__file__)), "search_keywords_export.csv")


def get_admin_token(email: str, password: str) -> str:
    url = f"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={API_KEY}"
    body = json.dumps({"email": email, "password": password, "returnSecureToken": True}).encode()
    req = urllib.request.Request(url, data=body, headers={"Content-Type": "application/json"})
    return json.loads(urllib.request.urlopen(req, timeout=10).read())["idToken"]


def main() -> None:
    email = sys.argv[1] if len(sys.argv) > 1 else "superadmin@physics.com"
    password = sys.argv[2] if len(sys.argv) > 2 else "PhysicsAdmin2026!"

    token = get_admin_token(email, password)
    req = urllib.request.Request(
        f"{BACKEND_URL}/admin/search/keywords/export.csv",
        headers={"Authorization": f"Bearer {token}"},
    )
    csv_bytes = urllib.request.urlopen(req, timeout=15).read()

    with open(OUT_PATH, "wb") as f:
        f.write(csv_bytes)

    row_count = csv_bytes.decode().count("\n") - 1  # minus header
    print(f"Wrote {row_count} keyword(s) to {OUT_PATH}")


if __name__ == "__main__":
    main()
