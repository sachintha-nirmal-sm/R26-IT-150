"""
seed_quizzes.py — Seed sample quizzes for lessons.

Adds a quiz document to the 'lessons/phy-g10-motion-doc/quizzes' subcollection,
following the schema defined in Section 3.5 of devmini.md.

Usage:
    python seed/seed_quizzes.py
"""

import sys
import os
from google.cloud import firestore

# Allow imports from the parent directory (firebase/scripts)
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from firebase_init import db

def seed_quiz():
    print("Starting Quiz Seeding script...")
    
    lesson_id = "phy-g10-motion-doc"
    lesson_ref = db.collection("lessons").document(lesson_id)

    # Check if target lesson exists
    if not lesson_ref.get().exists:
        print(f"Target lesson '{lesson_id}' not found! Please run seed_sample_lessons.py first.")
        sys.exit(1)

    # Define quiz data
    quiz_id = "phy-g10-motion-quiz"
    quiz_data = {
        "title": "Introduction to Motion Quiz",
        "lessonId": lesson_id,
        "maxAttempts": 3,
        "questionsPerAttempt": 20,
        "activeQuestionBankVersionId": None,  # None/null since no bank version exists yet
        "status": "noBankGenerated",
        "createdAt": firestore.SERVER_TIMESTAMP,
        "updatedAt": firestore.SERVER_TIMESTAMP
    }

    # Write to lessons/{lessonId}/quizzes/{quizId}
    quiz_ref = lesson_ref.collection("quizzes").document(quiz_id)
    print(f"Seeding quiz: {quiz_data['title']}...")
    quiz_ref.set(quiz_data)
    print(f"  [Firestore] Saved to lessons/{lesson_id}/quizzes/{quiz_id}")

    print("\nQuiz seeding completed successfully!")

if __name__ == "__main__":
    try:
        seed_quiz()
    except FileNotFoundError as e:
        print(e)
        sys.exit(1)
    except Exception as e:
        print(f"\nError during seeding: {e}")
        sys.exit(1)
