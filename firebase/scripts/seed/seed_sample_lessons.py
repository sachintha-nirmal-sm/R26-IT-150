"""
seed_sample_lessons.py — Seed sample lesson documents for local dev.

Writes a set of sample lessons/{lessonId} documents to Firestore
for Grade 10, following the schema defined in Section 3.3 of devmini.md.

Usage:
    python seed/seed_sample_lessons.py
"""

import sys
import os
from google.cloud import firestore

# Allow imports from the parent directory (firebase/scripts)
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from firebase_init import db

def get_admin_uid():
    """
    Attempts to fetch the UID of the first admin user in the users collection.
    Falls back to a placeholder string if no admin exists yet.
    """
    try:
        admins = db.collection("users").where("role", "==", "admin").limit(1).get()
        if admins:
            return admins[0].id
    except Exception as e:
        print(f"Could not query admin user: {e}")
    
    return "system_seed_admin"

def seed_lessons():
    print("Starting Lessons Seeding script...")
    
    admin_uid = get_admin_uid()
    print(f"Using admin UID for creation fields: {admin_uid}")

    # 1. Define sample lessons for Grade 10 Physics
    lessons_to_seed = [
        {
            "id": "phy-g10-motion-doc", # Custom ID or let Firestore generate. Here we use stable custom IDs.
            "data": {
                "title": "Introduction to Motion",
                "subject": "Physics",
                "grade": 10,
                "lessonTag": "phy-g10-motion",
                "description": "Learn about kinematics, speed, velocity, acceleration, and distance-time graphs.",
                "order": 1,
                "status": "published",
                "createdBy": admin_uid,
                "lastEditedBy": admin_uid,
                "materialsCount": 0,
                "createdAt": firestore.SERVER_TIMESTAMP,
                "updatedAt": firestore.SERVER_TIMESTAMP,
            }
        },
        {
            "id": "phy-g10-forces-doc",
            "data": {
                "title": "Forces and Newton's Laws",
                "subject": "Physics",
                "grade": 10,
                "lessonTag": "phy-g10-forces",
                "description": "Explore force types, Newton's three laws of motion, inertia, and friction.",
                "order": 2,
                "status": "published",
                "createdBy": admin_uid,
                "lastEditedBy": admin_uid,
                "materialsCount": 0,
                "createdAt": firestore.SERVER_TIMESTAMP,
                "updatedAt": firestore.SERVER_TIMESTAMP,
            }
        },
        {
            "id": "phy-g10-work-energy-doc",
            "data": {
                "title": "Work, Energy, and Power",
                "subject": "Physics",
                "grade": 10,
                "lessonTag": "phy-g10-work-energy",
                "description": "Understand work done, kinetic and potential energy, conservation of energy, and power calculation.",
                "order": 3,
                "status": "published",
                "createdBy": admin_uid,
                "lastEditedBy": admin_uid,
                "materialsCount": 0,
                "createdAt": firestore.SERVER_TIMESTAMP,
                "updatedAt": firestore.SERVER_TIMESTAMP,
            }
        }
    ]

    # 2. Write to Firestore
    lessons_ref = db.collection("lessons")
    for lesson in lessons_to_seed:
        doc_id = lesson["id"]
        doc_data = lesson["data"]
        
        print(f"Seeding lesson: {doc_data['title']} (tag: {doc_data['lessonTag']})...")
        lessons_ref.document(doc_id).set(doc_data)
        print(f"  [Firestore] Saved to lessons/{doc_id}")

    print("\nLessons seeding completed successfully!")

if __name__ == "__main__":
    try:
        seed_lessons()
    except FileNotFoundError as e:
        print(e)
        sys.exit(1)
    except Exception as e:
        print(f"\nError during seeding: {e}")
        sys.exit(1)
