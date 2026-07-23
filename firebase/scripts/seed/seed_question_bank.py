"""
seed_question_bank.py — Seed question bank version and sample questions.

This script:
  1. Creates a 'questionBankVersions' document (v1, active) under the quiz.
  2. Adds 5 sample questions to the nested 'questions' subcollection.
  3. Updates the parent quiz with the active version ID and sets status to 'bankReady'.

Usage:
    python seed/seed_question_bank.py
"""

import sys
import os
from google.cloud import firestore

# Allow imports from the parent directory (firebase/scripts)
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from firebase_init import db

def seed_question_bank():
    print("Starting Question Bank Seeding script...")
    
    lesson_id = "phy-g10-motion-doc"
    quiz_id = "phy-g10-motion-quiz"
    version_id = "v1"
    
    # References
    lesson_ref = db.collection("lessons").document(lesson_id)
    quiz_ref = lesson_ref.collection("quizzes").document(quiz_id)
    
    # Check if target quiz exists
    quiz_snap = quiz_ref.get()
    if not quiz_snap.exists:
        print(f"Target quiz '{quiz_id}' not found! Please run seed_quizzes.py first.")
        sys.exit(1)

    # 1. Define the Version document data
    version_data = {
        "versionNumber": 1,
        "status": "active",
        "totalQuestions": 5,
        "generatedBy": "RAG",
        "generationJobId": "job-seed-v1-001",
        "sourceMaterialIds": ["kinematics-notes-pdf", "motion-formula-sheet"],
        "createdAt": firestore.SERVER_TIMESTAMP,
        "archivedAt": None
    }

    # 2. Define 5 sample questions
    questions_to_seed = [
        {
            "id": "q1",
            "data": {
                "questionText": "What is the SI unit of speed?",
                "questionType": "Theory",
                "options": ["meters per second (m/s)", "kilometers per hour (km/h)", "meters per second squared (m/s^2)", "miles per hour (mph)"],
                "correctAnswer": "meters per second (m/s)",
                "explanation": "Speed is defined as distance divided by time. The SI unit of distance is meters (m) and time is seconds (s), hence m/s.",
                "lessonTag": "phy-g10-motion",
                "difficulty": "easy",
                "marks": 2,
                "generatedBy": "RAG",
                "sourceReference": "kinematics-notes-pdf#page=1",
                "createdAt": firestore.SERVER_TIMESTAMP
            }
        },
        {
            "id": "q2",
            "data": {
                "questionText": "Which of the following equations is a valid kinematic equation for constant acceleration?",
                "questionType": "Formula",
                "options": ["v = u + at", "v^2 = u^2 - 2as", "s = ut - 0.5*at^2", "v = u + a*t^2"],
                "correctAnswer": "v = u + at",
                "explanation": "v = u + at represents the definition of acceleration: rate of change of velocity over time.",
                "lessonTag": "phy-g10-motion",
                "difficulty": "easy",
                "marks": 2,
                "generatedBy": "RAG",
                "sourceReference": "motion-formula-sheet#line=5",
                "createdAt": firestore.SERVER_TIMESTAMP
            }
        },
        {
            "id": "q3",
            "data": {
                "questionText": "A car accelerates from rest at a constant rate of 2 m/s^2. How far will it travel in 5 seconds?",
                "questionType": "Calculation",
                "options": ["5 meters", "10 meters", "25 meters", "50 meters"],
                "correctAnswer": "25 meters",
                "explanation": "Using the kinematic formula: s = ut + 0.5*at^2. Since it starts from rest, u = 0. Therefore, s = 0.5 * 2 * (5^2) = 25 meters.",
                "lessonTag": "phy-g10-motion",
                "difficulty": "medium",
                "marks": 5,
                "generatedBy": "RAG",
                "sourceReference": "kinematics-notes-pdf#page=3",
                "createdAt": firestore.SERVER_TIMESTAMP
            }
        },
        {
            "id": "q4",
            "data": {
                "questionText": "Explain the key difference between distance and displacement.",
                "questionType": "Theory",
                "options": None,  # Open-ended theory question
                "correctAnswer": "Distance is a scalar quantity representing the total path length traveled, while displacement is a vector quantity representing the straight-line distance and direction from the start point to the end point.",
                "explanation": "Distance does not specify direction (scalar), whereas displacement always specifies a direction relative to the origin (vector).",
                "lessonTag": "phy-g10-motion",
                "difficulty": "medium",
                "marks": 4,
                "generatedBy": "RAG",
                "sourceReference": "kinematics-notes-pdf#page=2",
                "createdAt": firestore.SERVER_TIMESTAMP
            }
        },
        {
            "id": "q5",
            "data": {
                "questionText": "A stone is dropped from a cliff and takes 4 seconds to reach the ground. Assuming acceleration due to gravity is 10 m/s^2, what is the height of the cliff?",
                "questionType": "Calculation",
                "options": ["40 meters", "80 meters", "160 meters", "20 meters"],
                "correctAnswer": "80 meters",
                "explanation": "Using s = ut + 0.5*gt^2. Initial velocity u = 0, g = 10, t = 4. s = 0.5 * 10 * 16 = 80 meters.",
                "lessonTag": "phy-g10-motion",
                "difficulty": "hard",
                "marks": 5,
                "generatedBy": "RAG",
                "sourceReference": "kinematics-notes-pdf#page=4",
                "createdAt": firestore.SERVER_TIMESTAMP
            }
        }
    ]

    # 3. Create the version document
    version_ref = quiz_ref.collection("questionBankVersions").document(version_id)
    print(f"Creating question bank version '{version_id}'...")
    version_ref.set(version_data)
    print(f"  [Firestore] Saved to .../questionBankVersions/{version_id}")

    # 4. Create the questions under the version
    questions_subcoll = version_ref.collection("questions")
    for q in questions_to_seed:
        q_id = q["id"]
        q_data = q["data"]
        print(f"Adding question {q_id}: {q_data['questionText'][:40]}...")
        questions_subcoll.document(q_id).set(q_data)
        print(f"  [Firestore] Saved to .../questions/{q_id}")

    # 5. Update parent quiz
    print(f"\nUpdating parent quiz '{quiz_id}' to point to active version '{version_id}'...")
    quiz_ref.update({
        "activeQuestionBankVersionId": version_id,
        "status": "bankReady",
        "updatedAt": firestore.SERVER_TIMESTAMP
    })
    print("  [Firestore] Quiz status updated to 'bankReady'")

    print("\nQuestion Bank and Questions seeding completed successfully!")

if __name__ == "__main__":
    try:
        seed_question_bank()
    except FileNotFoundError as e:
        print(e)
        sys.exit(1)
    except Exception as e:
        print(f"\nError during seeding: {e}")
        sys.exit(1)
