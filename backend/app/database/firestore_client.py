"""Compatibility shim — prefer `app.core.firebase.db` in new code."""

from app.core.firebase import db, get_db


def get_firestore_client():
    return db


__all__ = ["db", "get_db", "get_firestore_client"]
