"""
seed_final_quiz_attempt.py — Seed finalQuizAttempts data under the student user.

This script:
  1. Locates the seeded student user (student@example.com).
  2. Seeds one sample document in users/{uid}/finalQuizAttempts/final-attempt-1
     referencing the finalQuizzes doc from Task 9 (grade10-round1, roundNumber: 1).
     Includes the answerCheckingModel field.
"""

import sys
import os
from google.cloud import firestore

# Allow imports from the parent directory (firebase/scripts)
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from firebase_init import db

def get_student_uid():
    """
    Attempts to fetch the UID of the first student user in the users collection.
    """
    try:
        students = db.collection("users").where("role", "==", "student").limit(1).get()
        if students:
            return students[0].id
    except Exception as e:
        print(f"Could not query student user: {e}")
    
    return None

def seed_final_quiz_attempt():
    print("Starting Final Quiz Attempt Seeding script...")
    
    student_uid = get_student_uid()
    if not student_uid:
        print("Student user not found! Please run seed_users.py first.")
        sys.exit(1)
        
    print(f"Using Student UID: {student_uid}")
    
    # Reference the finalQuizzes doc from Task 9
    final_quiz_id = "grade10-round1"
    round_number = 1
    attempt_id = "final-attempt-1"
    
    # Base reference to the student's user document
    student_ref = db.collection("users").document(student_uid)
    
    # Define attempt data matching Section 3.16 of devmini.md
    attempt_data = {
        "finalQuizId": final_quiz_id,
        "roundNumber": round_number,
        "attemptNumber": 1,
        "answerCheckingModel": "gpt-4o",  # answerCheckingModel field
        "answers": [
            {
                "questionId": "fq-q1",
                "studentAnswer": "The object is at rest",
                "isCorrect": True,
                "marksAwarded": 3,
                "lessonTag": "phy-g10-motion",
                "difficulty": "easy",
                "questionType": "Theory"
            },
            {
                "questionId": "fq-q2",
                "studentAnswer": "F = m*a",
                "isCorrect": True,
                "marksAwarded": 3,
                "lessonTag": "phy-g10-forces",
                "difficulty": "easy",
                "questionType": "Formula"
            },
            {
                "questionId": "fq-q3",
                "studentAnswer": "60 Joules",
                "isCorrect": True,
                "marksAwarded": 6,
                "lessonTag": "phy-g10-work-energy",
                "difficulty": "medium",
                "questionType": "Calculation"
            }
        ],
        "score": 12,
        "totalMarks": 12,
        "timeTakenSeconds": 450,
        "startedAt": firestore.SERVER_TIMESTAMP,
        "submittedAt": firestore.SERVER_TIMESTAMP,
        "status": "completed"
    }
    
    # Save to Firestore
    student_ref.collection("finalQuizAttempts").document(attempt_id).set(attempt_data)
    print(f"  [Firestore] Saved to users/{student_uid}/finalQuizAttempts/{attempt_id}")
    print("\nFinal Quiz Attempt seeding completed successfully!")

if __name__ == "__main__":
    try:
        seed_final_quiz_attempt()
    except FileNotFoundError as e:
        print(e)
        sys.exit(1)
    except Exception as e:
        print(f"\nError during seeding: {e}")
        sys.exit(1)
