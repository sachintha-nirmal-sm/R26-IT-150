"""
seed_generation_job.py — Seed a sample completed generation job document.

This script:
  1. Locates the seeded admin user (admin@example.com) to populate requestedBy.
  2. Seeds one generationJobs document with:
     - jobType: "questionBankGeneration"
     - status: "completed"
     - progressPercent: 100
     - llmModelUsed: "claude-sonnet-5"
     - targetLessonId: "phy-g10-motion-doc"
     - targetQuizId: "phy-g10-motion-quiz" (referencing Task 6 quiz)
     - resultVersionId: "v1"
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
    """
    try:
        admins = db.collection("users").where("role", "==", "admin").limit(1).get()
        if admins:
            return admins[0].id
    except Exception as e:
        print(f"Could not query admin user: {e}")
    
    return None

def seed_generation_job():
    print("Starting Generation Job Seeding script...")
    
    admin_uid = get_admin_uid()
    if not admin_uid:
        print("Admin user not found! Please run seed_users.py first.")
        sys.exit(1)
        
    print(f"Using Admin UID: {admin_uid}")
    
    # Define generation job document details
    job_id = "job-seed-v1-001"
    job_data = {
        "jobType": "questionBankGeneration",
        "targetLessonId": "phy-g10-motion-doc",
        "targetQuizId": "phy-g10-motion-quiz",
        "targetGrade": None,
        "llmModelUsed": "claude-sonnet-5",
        "status": "completed",
        "progressPercent": 100,
        "requestedBy": admin_uid,
        "resultVersionId": "v1",
        "createdAt": firestore.SERVER_TIMESTAMP,
        "updatedAt": firestore.SERVER_TIMESTAMP
    }
    
    # Save/update document in Firestore 'generationJobs/{jobId}'
    job_ref = db.collection("generationJobs").document(job_id)
    job_ref.set(job_data)
    print(f"  [Firestore] Saved generation job to generationJobs/{job_id}")
    print("\nGeneration Job seeding completed successfully!")

if __name__ == "__main__":
    try:
        seed_generation_job()
    except FileNotFoundError as e:
        print(e)
        sys.exit(1)
    except Exception as e:
        print(f"\nError during seeding: {e}")
        sys.exit(1)
