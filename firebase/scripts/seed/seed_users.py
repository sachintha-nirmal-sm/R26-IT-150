"""
seed_users.py — Seed sample admin and student users in Firebase.

This script:
  1. Creates two users in Firebase Auth:
     - Admin: admin@example.com (Password: admin123)
     - Student: student@example.com (Password: student123)
  2. Sets custom claims on their Auth accounts:
     - Admin claim: { "role": "admin" }
     - Student claim: { "role": "student" }
  3. Writes the matching profiles to the 'users/{uid}' collection in Firestore,
     following the schema defined in Section 3.1 of devmini.md.

Usage:
    python seed/seed_users.py
"""

import sys
import os
from google.cloud import firestore

# Allow imports from the parent directory (firebase/scripts)
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from firebase_init import db
from auth_helpers import create_auth_user, set_role_claim

def seed_users():
    print("Starting User Seeding script...")

    # 1. Define sample user data
    admin_data = {
        "email": "admin@example.com",
        "password": "admin123",
        "fullName": "System Admin",
        "role": "admin",
        "status": "active",
    }

    student_data = {
        "email": "student@example.com",
        "password": "student123",
        "fullName": "Jane Student",
        "role": "student",
        "currentGrade": 10,
        "enrollmentYear": 2026,
        "lastPromotedAt": None,
        "status": "active",
    }

    # --- Seeding Admin User ---
    print(f"\nSetting up Admin User ({admin_data['email']}):")
    admin_uid = create_auth_user(
        email=admin_data["email"],
        password=admin_data["password"],
        display_name=admin_data["fullName"]
    )
    # Set the custom claim
    set_role_claim(admin_uid, admin_data["role"])
    
    # Save/update document in Firestore 'users/{uid}'
    admin_doc_ref = db.collection("users").document(admin_uid)
    admin_doc_ref.set({
        "role": admin_data["role"],
        "fullName": admin_data["fullName"],
        "email": admin_data["email"],
        "status": admin_data["status"],
        "createdAt": firestore.SERVER_TIMESTAMP,
        "updatedAt": firestore.SERVER_TIMESTAMP
    })
    print(f"  [Firestore] Saved admin doc to users/{admin_uid}")

    # --- Seeding Student User ---
    print(f"\nSetting up Student User ({student_data['email']}):")
    student_uid = create_auth_user(
        email=student_data["email"],
        password=student_data["password"],
        display_name=student_data["fullName"]
    )
    # Set the custom claim
    set_role_claim(student_uid, student_data["role"])
    
    # Save/update document in Firestore 'users/{uid}'
    student_doc_ref = db.collection("users").document(student_uid)
    student_doc_ref.set({
        "role": student_data["role"],
        "fullName": student_data["fullName"],
        "email": student_data["email"],
        "currentGrade": student_data["currentGrade"],
        "enrollmentYear": student_data["enrollmentYear"],
        "lastPromotedAt": student_data["lastPromotedAt"],
        "status": student_data["status"],
        "createdAt": firestore.SERVER_TIMESTAMP,
        "updatedAt": firestore.SERVER_TIMESTAMP
    })
    print(f"  [Firestore] Saved student doc to users/{student_uid}")

    print("\nUser seeding completed successfully!")

if __name__ == "__main__":
    try:
        seed_users()
    except FileNotFoundError as e:
        print(e)
        sys.exit(1)
    except Exception as e:
        print(f"\nError during seeding: {e}")
        sys.exit(1)
