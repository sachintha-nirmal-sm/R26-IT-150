"""
seed_materials.py — Seed sample lesson materials for RAG ingestion testing.

Adds sample materials to the 'lessons/phy-g10-motion-doc/materials' subcollection,
following the schema defined in Section 3.4 of devmini.md.

Usage:
    python seed/seed_materials.py
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

def seed_materials():
    print("Starting Materials Seeding script...")
    
    admin_uid = get_admin_uid()
    print(f"Using admin UID for uploader fields: {admin_uid}")

    lesson_id = "phy-g10-motion-doc"
    lesson_ref = db.collection("lessons").document(lesson_id)

    # Check if target lesson exists
    if not lesson_ref.get().exists:
        print(f"Target lesson '{lesson_id}' not found! Please run seed_sample_lessons.py first.")
        sys.exit(1)

    # Define sample materials
    materials_to_seed = [
        {
            "id": "kinematics-notes-pdf",
            "data": {
                "fileName": "kinematics_lecture_notes_v1.pdf",
                "materialType": "pdf",
                "storagePath": f"materials/{lesson_id}/kinematics_lecture_notes_v1.pdf",
                "fileSizeBytes": 2048576,  # ~2 MB
                "ingestionStatus": "embedded",  # fully processed for testing
                "chunkCount": 42,
                "uploadedBy": admin_uid,
                "uploadedAt": firestore.SERVER_TIMESTAMP,
                "lastProcessedAt": firestore.SERVER_TIMESTAMP,
            }
        },
        {
            "id": "motion-formula-sheet",
            "data": {
                "fileName": "motion_formulas_quick_ref.pdf",
                "materialType": "formulaSheet",
                "storagePath": f"materials/{lesson_id}/motion_formulas_quick_ref.pdf",
                "fileSizeBytes": 512400,  # ~500 KB
                "ingestionStatus": "uploaded",  # simulating a freshly uploaded file not yet embedded
                "chunkCount": 0,
                "uploadedBy": admin_uid,
                "uploadedAt": firestore.SERVER_TIMESTAMP,
                "lastProcessedAt": None,
            }
        }
    ]

    # Write to the subcollection lessons/{lessonId}/materials/{materialId}
    materials_subcoll_ref = lesson_ref.collection("materials")
    
    for mat in materials_to_seed:
        doc_id = mat["id"]
        doc_data = mat["data"]
        
        print(f"Seeding material '{doc_data['fileName']}'...")
        materials_subcoll_ref.document(doc_id).set(doc_data)
        print(f"  [Firestore] Saved to lessons/{lesson_id}/materials/{doc_id}")

    # Also update the denormalized materialsCount on the parent lesson document
    # as described in Section 3.3 ("denormalized count of uploaded materials")
    print(f"\nUpdating materialsCount on parent lesson '{lesson_id}'...")
    lesson_ref.update({
        "materialsCount": len(materials_to_seed),
        "updatedAt": firestore.SERVER_TIMESTAMP
    })
    print("  [Firestore] Updated parent lesson materialsCount to", len(materials_to_seed))

    print("\nMaterials seeding completed successfully!")

if __name__ == "__main__":
    try:
        seed_materials()
    except FileNotFoundError as e:
        print(e)
        sys.exit(1)
    except Exception as e:
        print(f"\nError during seeding: {e}")
        sys.exit(1)
