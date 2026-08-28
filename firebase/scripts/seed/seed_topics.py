"""
seed_topics.py — Seed curriculum topics for Unity practicals.

Creates:
  1. Parent lessons/{lessonId} docs for Grade 9 / 11 if they do not already
     exist (Grade 10 lessons from seed_sample_lessons.py are reused, never
     overwritten).
  2. topics/{topicId} documents that group practicals under a lesson.

Does NOT replace the existing lessons schema used by the quiz/RAG module.
New lessons use the same field shape (title, subject, grade, lessonTag,
status, …) so both components share one lessons collection.

Usage:
    python seed/seed_topics.py
"""

import sys
import os
from google.cloud import firestore

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from firebase_init import db


def get_admin_uid():
    try:
        admins = db.collection("users").where("role", "==", "admin").limit(1).get()
        if admins:
            return admins[0].id
    except Exception as e:
        print(f"Could not query admin user: {e}")
    return "system_seed_admin"


# Lessons that the practicals module needs but the quiz seed may not have.
# Existing Grade 10 docs (phy-g10-motion-doc, phy-g10-forces-doc) are skipped.
PRACTICAL_LESSONS = [
    {
        "id": "phy-g9-force-doc",
        "data": {
            "title": "Basic Concepts Associated with Force",
            "subject": "Physics",
            "grade": 9,
            "lessonTag": "phy-g9-force",
            "description": "Force as a push or pull, and how force changes the motion of an object.",
            "order": 1,
            "status": "published",
        },
    },
    {
        "id": "phy-g9-density-doc",
        "data": {
            "title": "Density",
            "subject": "Physics",
            "grade": 9,
            "lessonTag": "phy-g9-density",
            "description": "Mass, volume, and density of solids and liquids.",
            "order": 1,
            "status": "published",
        },
    },
    {
        "id": "phy-g9-oscillations-doc",
        "data": {
            "title": "Oscillations",
            "subject": "Physics",
            "grade": 9,
            "lessonTag": "phy-g9-oscillations",
            "description": "Simple pendulum motion, time period, and length.",
            "order": 2,
            "status": "published",
        },
    },
    {
        "id": "phy-g11-waves-doc",
        "data": {
            "title": "Waves",
            "subject": "Physics",
            "grade": 11,
            "lessonTag": "phy-g11-waves",
            "description": "Wave properties, ripple tanks, frequency and wavelength.",
            "order": 1,
            "status": "published",
        },
    },
]

TOPICS = [
    {
        "id": "topic-g9-force",
        "data": {
            "grade": 9,
            "lessonId": "phy-g9-force-doc",
            "name": "Basic Concepts Associated with Force",
            "description": "Apply a force to a block and relate force, mass, and acceleration.",
            "order": 1,
            "isActive": True,
        },
    },
    {
        "id": "topic-g9-density",
        "data": {
            "grade": 9,
            "lessonId": "phy-g9-density-doc",
            "name": "Density",
            "description": "Measure mass and volume to calculate density of water.",
            "order": 1,
            "isActive": True,
        },
    },
    {
        "id": "topic-g9-oscillations",
        "data": {
            "grade": 9,
            "lessonId": "phy-g9-oscillations-doc",
            "name": "Pendulum Oscillations",
            "description": "Study the relationship between pendulum length and time period.",
            "order": 2,
            "isActive": True,
        },
    },
    {
        "id": "topic-g10-motion",
        "data": {
            "grade": 10,
            "lessonId": "phy-g10-motion-doc",
            "name": "Motion",
            "description": "Kinematics practicals: distance, velocity, and acceleration.",
            "order": 1,
            "isActive": True,
        },
    },
    {
        "id": "topic-g10-forces",
        "data": {
            "grade": 10,
            "lessonId": "phy-g10-forces-doc",
            "name": "Forces",
            "description": "Newton's laws and friction practicals.",
            "order": 2,
            "isActive": True,
        },
    },
    {
        "id": "topic-g11-waves",
        "data": {
            "grade": 11,
            "lessonId": "phy-g11-waves-doc",
            "name": "Waves",
            "description": "Ripple-tank wave measurement practicals.",
            "order": 1,
            "isActive": True,
        },
    },
]


def ensure_lessons(admin_uid):
    print("Ensuring parent lessons exist (will not overwrite existing docs)...")
    lessons_ref = db.collection("lessons")
    for lesson in PRACTICAL_LESSONS:
        doc_ref = lessons_ref.document(lesson["id"])
        if doc_ref.get().exists:
            print(f"  [skip] lessons/{lesson['id']} already exists")
            continue
        payload = {
            **lesson["data"],
            "createdBy": admin_uid,
            "lastEditedBy": admin_uid,
            "materialsCount": 0,
            "createdAt": firestore.SERVER_TIMESTAMP,
            "updatedAt": firestore.SERVER_TIMESTAMP,
        }
        doc_ref.set(payload)
        print(f"  [Firestore] Created lessons/{lesson['id']}")


def seed_topics():
    print("Starting Topics Seeding script...")
    admin_uid = get_admin_uid()
    print(f"Using admin UID: {admin_uid}")

    ensure_lessons(admin_uid)

    print("\nSeeding topics...")
    topics_ref = db.collection("topics")
    for topic in TOPICS:
        payload = {
            **topic["data"],
            "createdAt": firestore.SERVER_TIMESTAMP,
            "updatedAt": firestore.SERVER_TIMESTAMP,
        }
        topics_ref.document(topic["id"]).set(payload)
        print(f"  [Firestore] Saved topics/{topic['id']} ({payload['name']})")

    print("\nTopics seeding completed successfully!")


if __name__ == "__main__":
    try:
        seed_topics()
    except FileNotFoundError as e:
        print(e)
        sys.exit(1)
    except Exception as e:
        print(f"\nError during seeding: {e}")
        sys.exit(1)
