"""
seed_final_quiz.py — Seed Grade 10 final quiz and its nested questions.

This script:
  1. Creates a 'finalQuizzes' document with ID 'grade10-round1' (status: active, roundNumber: 1).
  2. Adds 3 sample questions to the nested 'questions' subcollection,
     each representing a different lesson in Grade 10 (Motion, Forces, Work/Energy),
     with mandatory 'lessonTag' fields.

Usage:
    python seed/seed_final_quiz.py
"""

import sys
import os
from google.cloud import firestore

# Allow imports from the parent directory (firebase/scripts)
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from firebase_init import db

def seed_final_quiz():
    print("Starting Final Quiz Seeding script...")
    
    final_quiz_id = "grade10-round1"
    
    # 1. Define the finalQuizzes document data (Section 3.14)
    final_quiz_data = {
        "grade": 10,
        "roundNumber": 1,
        "status": "active",
        "title": "Grade 10 Physics Final Quiz - Round 1",
        "totalMarks": 12,
        "maxAttempts": 3,
        "coveredLessonIds": ["phy-g10-motion-doc", "phy-g10-forces-doc", "phy-g10-work-energy-doc"],
        "generatedBy": "RAG",
        "llmModelUsed": "gpt-5",
        "generationJobId": "job-seed-fq-001",
        "createdAt": firestore.SERVER_TIMESTAMP,
        "archivedAt": None
    }
    
    # 2. Define 3 questions, each under a different lessonTag (Section 3.15)
    questions_to_seed = [
        {
            "id": "fq-q1",
            "data": {
                "questionText": "What does a horizontal line on a distance-time graph represent?",
                "questionType": "Theory",
                "options": ["Constant speed", "Acceleration", "Deceleration", "The object is at rest"],
                "correctAnswer": "The object is at rest",
                "explanation": "On a distance-time graph, a horizontal line shows that the distance is not changing over time, meaning the object is stationary.",
                "lessonTag": "phy-g10-motion",
                "sourceLessonId": "phy-g10-motion-doc",
                "difficulty": "easy",
                "marks": 3,
                "generatedBy": "RAG",
                "sourceReference": "kinematics-notes-pdf#page=1",
                "createdAt": firestore.SERVER_TIMESTAMP
            }
        },
        {
            "id": "fq-q2",
            "data": {
                "questionText": "Which formula corresponds to Newton's Second Law of Motion?",
                "questionType": "Formula",
                "options": ["F = m/a", "F = m*a", "F = w*g", "F = m*v"],
                "correctAnswer": "F = m*a",
                "explanation": "Newton's second law states that acceleration is directly proportional to net force and inversely proportional to mass, represented as F = ma.",
                "lessonTag": "phy-g10-forces",
                "sourceLessonId": "phy-g10-forces-doc",
                "difficulty": "easy",
                "marks": 3,
                "generatedBy": "RAG",
                "sourceReference": "forces-reference#page=2",
                "createdAt": firestore.SERVER_TIMESTAMP
            }
        },
        {
            "id": "fq-q3",
            "data": {
                "questionText": "An object of mass 2 kg is lifted vertically through a height of 3 meters. Calculate the work done against gravity. (Take g = 10 m/s^2)",
                "questionType": "Calculation",
                "options": ["6 Joules", "20 Joules", "30 Joules", "60 Joules"],
                "correctAnswer": "60 Joules",
                "explanation": "Work Done = Force * Distance = mass * g * height = 2 kg * 10 m/s^2 * 3 m = 60 Joules.",
                "lessonTag": "phy-g10-work-energy",
                "sourceLessonId": "phy-g10-work-energy-doc",
                "difficulty": "medium",
                "marks": 6,
                "generatedBy": "RAG",
                "sourceReference": "energy-ref#page=4",
                "createdAt": firestore.SERVER_TIMESTAMP
            }
        }
    ]

    # Write the main final quiz document
    final_quiz_ref = db.collection("finalQuizzes").document(final_quiz_id)
    print(f"Creating final quiz '{final_quiz_id}'...")
    final_quiz_ref.set(final_quiz_data)
    print(f"  [Firestore] Saved to finalQuizzes/{final_quiz_id}")

    # Write the nested questions
    questions_subcoll_ref = final_quiz_ref.collection("questions")
    for q in questions_to_seed:
        q_id = q["id"]
        q_data = q["data"]
        print(f"Adding question {q_id} (tag: {q_data['lessonTag']})...")
        questions_subcoll_ref.document(q_id).set(q_data)
        print(f"  [Firestore] Saved to finalQuizzes/{final_quiz_id}/questions/{q_id}")

    print("\nFinal Quiz seeding completed successfully!")

if __name__ == "__main__":
    try:
        seed_final_quiz()
    except FileNotFoundError as e:
        print(e)
        sys.exit(1)
    except Exception as e:
        print(f"\nError during seeding: {e}")
        sys.exit(1)
