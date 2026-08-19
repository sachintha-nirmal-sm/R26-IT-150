"""
seed_student_progress.py — Seed student progress, attempts, feedback, and performance summary data.

This script:
  1. Locates the seeded student user (student@example.com).
  2. Seeds one sample document in each of the following collections:
     - users/{uid}/quizProgress/phy-g10-motion-quiz
     - users/{uid}/quizAttempts/attempt-1
     - users/{uid}/wrongQuestions/wrong-q2 and users/{uid}/wrongQuestions/wrong-q5
     - users/{uid}/feedback/feedback-attempt-1
     - users/{uid}/weakTopics/phy-g10-motion
     - users/{uid}/youtubeRecommendations/rec-motion-1
     - users/{uid}/performanceSummary (single doc)
  These items map exactly to Sections 3.8 to 3.13 and 3.17 in devmini.md.

Usage:
    python seed/seed_student_progress.py
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

def seed_student_progress():
    print("Starting Student Progress Seeding script...")
    
    student_uid = get_student_uid()
    if not student_uid:
        print("Student user not found! Please run seed_users.py first.")
        sys.exit(1)
        
    print(f"Using Student UID: {student_uid}")
    
    # Common variables
    quiz_id = "phy-g10-motion-quiz"
    lesson_id = "phy-g10-motion-doc"
    lesson_tag = "phy-g10-motion"
    attempt_id = "attempt-1"

    # Base reference to the student's user document
    student_ref = db.collection("users").document(student_uid)

    # 1. Seed quizProgress (Section 3.8)
    print("\nSeeding quizProgress...")
    progress_data = {
        "quizId": quiz_id,
        "lessonId": lesson_id,
        "attemptsUsed": 1,
        "isLocked": False,
        "bestScore": 11,
        "bestAttemptId": attempt_id,
        "usedQuestionIds": ["q1", "q2", "q3", "q4", "q5"],
        "lastAttemptAt": firestore.SERVER_TIMESTAMP
    }
    student_ref.collection("quizProgress").document(quiz_id).set(progress_data)
    print(f"  [Firestore] Saved to users/{student_uid}/quizProgress/{quiz_id}")

    # 2. Seed quizAttempts (Section 3.9)
    print("\nSeeding quizAttempts...")
    attempt_data = {
        "quizId": quiz_id,
        "lessonId": lesson_id,
        "questionBankVersionId": "v1",
        "attemptNumber": 1,
        "answers": [
            {
                "questionId": "q1",
                "studentAnswer": "meters per second (m/s)",
                "isCorrect": True,
                "marksAwarded": 2,
                "lessonTag": lesson_tag,
                "difficulty": "easy",
                "questionType": "Theory"
            },
            {
                "questionId": "q2",
                "studentAnswer": "v = u + a*t^2",
                "isCorrect": False,
                "marksAwarded": 0,
                "lessonTag": lesson_tag,
                "difficulty": "easy",
                "questionType": "Formula"
            },
            {
                "questionId": "q3",
                "studentAnswer": "25 meters",
                "isCorrect": True,
                "marksAwarded": 5,
                "lessonTag": lesson_tag,
                "difficulty": "medium",
                "questionType": "Calculation"
            },
            {
                "questionId": "q4",
                "studentAnswer": "Distance is scalar, displacement is vector.",
                "isCorrect": True,
                "marksAwarded": 4,
                "lessonTag": lesson_tag,
                "difficulty": "medium",
                "questionType": "Theory"
            },
            {
                "questionId": "q5",
                "studentAnswer": "40 meters",
                "isCorrect": False,
                "marksAwarded": 0,
                "lessonTag": lesson_tag,
                "difficulty": "hard",
                "questionType": "Calculation"
            }
        ],
        "score": 11,
        "totalMarks": 18,
        "timeTakenSeconds": 320,
        "startedAt": firestore.SERVER_TIMESTAMP,
        "submittedAt": firestore.SERVER_TIMESTAMP,
        "status": "completed"
    }
    student_ref.collection("quizAttempts").document(attempt_id).set(attempt_data)
    print(f"  [Firestore] Saved to users/{student_uid}/quizAttempts/{attempt_id}")

    # 3. Seed wrongQuestions (Section 3.10)
    print("\nSeeding wrongQuestions...")
    wrong_questions = [
        {
            "id": "wrong-q2",
            "data": {
                "questionId": "q2",
                "lessonId": lesson_id,
                "quizId": quiz_id,
                "attemptId": attempt_id,
                "questionBankVersionId": "v1",
                "questionText": "Which of the following equations is a valid kinematic equation for constant acceleration?",
                "studentAnswer": "v = u + a*t^2",
                "correctAnswer": "v = u + at",
                "explanation": "v = u + at represents the definition of acceleration: rate of change of velocity over time.",
                "questionType": "Formula",
                "lessonTag": lesson_tag,
                "difficulty": "easy",
                "reviewed": False,
                "createdAt": firestore.SERVER_TIMESTAMP
            }
        },
        {
            "id": "wrong-q5",
            "data": {
                "questionId": "q5",
                "lessonId": lesson_id,
                "quizId": quiz_id,
                "attemptId": attempt_id,
                "questionBankVersionId": "v1",
                "questionText": "A stone is dropped from a cliff and takes 4 seconds to reach the ground. Assuming acceleration due to gravity is 10 m/s^2, what is the height of the cliff?",
                "studentAnswer": "40 meters",
                "correctAnswer": "80 meters",
                "explanation": "Using s = ut + 0.5*gt^2. Initial velocity u = 0, g = 10, t = 4. s = 0.5 * 10 * 16 = 80 meters.",
                "questionType": "Calculation",
                "lessonTag": lesson_tag,
                "difficulty": "hard",
                "reviewed": False,
                "createdAt": firestore.SERVER_TIMESTAMP
            }
        }
    ]
    for wq in wrong_questions:
        student_ref.collection("wrongQuestions").document(wq["id"]).set(wq["data"])
        print(f"  [Firestore] Saved to users/{student_uid}/wrongQuestions/{wq['id']}")

    # 4. Seed feedback (Section 3.11)
    print("\nSeeding feedback...")
    feedback_data = {
        "attemptId": attempt_id,
        "quizId": quiz_id,
        "feedbackText": "Great effort! You show a solid understanding of basic speed concepts and kinematic calculations. However, you need to revise key kinematics formulas and practice more challenging cliff-drop calculation problems.",
        "strengths": [
            "Good grasp of SI units",
            "Correctly calculated displacement for constant acceleration problems"
        ],
        "weaknesses": [
            "Recalling kinematic formulas correctly",
            "Solving multi-step vertical motion calculation problems"
        ],
        "recommendedTopics": [lesson_tag],
        "llmModelUsed": "claude-sonnet-5",
        "generatedAt": firestore.SERVER_TIMESTAMP
    }
    student_ref.collection("feedback").document(f"feedback-{attempt_id}").set(feedback_data)
    print(f"  [Firestore] Saved to users/{student_uid}/feedback/feedback-{attempt_id}")

    # 5. Seed weakTopics (Section 3.12)
    print("\nSeeding weakTopics...")
    weak_topic_data = {
        "lessonTag": lesson_tag,
        "lessonId": lesson_id,
        "incorrectCount": 2,
        "totalAttempted": 5,
        "weaknessScore": 0.4,  # 2/5 incorrect
        "byQuestionType": {
            "Theory": {
                "incorrectCount": 0,
                "totalAttempted": 2,
                "weaknessScore": 0.0
            },
            "Formula": {
                "incorrectCount": 1,
                "totalAttempted": 1,
                "weaknessScore": 1.0
            },
            "Calculation": {
                "incorrectCount": 1,
                "totalAttempted": 2,
                "weaknessScore": 0.5
            }
        },
        "contributingQuizTypes": ["lessonQuiz"],
        "lastUpdated": firestore.SERVER_TIMESTAMP
    }
    student_ref.collection("weakTopics").document(lesson_tag).set(weak_topic_data)
    print(f"  [Firestore] Saved to users/{student_uid}/weakTopics/{lesson_tag}")

    # 6. Seed youtubeRecommendations (Section 3.13)
    print("\nSeeding youtubeRecommendations...")
    youtube_rec_data = {
        "lessonTag": lesson_tag,
        "videoId": "K2R56zU-Y3k",
        "title": "Physics Kinematics Equations Made Easy",
        "channelName": "The Organic Chemistry Tutor",
        "thumbnailUrl": "https://img.youtube.com/vi/K2R56zU-Y3k/0.jpg",
        "videoUrl": "https://www.youtube.com/watch?v=K2R56zU-Y3k",
        "relevanceScore": 0.95,
        "generatedAt": firestore.SERVER_TIMESTAMP
    }
    student_ref.collection("youtubeRecommendations").document("rec-motion-1").set(youtube_rec_data)
    print(f"  [Firestore] Saved to users/{student_uid}/youtubeRecommendations/rec-motion-1")

    # 7. Seed performanceSummary (Section 3.17 - single document, not subcollection)
    print("\nSeeding performanceSummary...")
    perf_summary_data = {
        "byQuestionType": {
            "Theory": {
                "incorrectCount": 0,
                "totalAttempted": 2,
                "weaknessScore": 0.0
            },
            "Formula": {
                "incorrectCount": 1,
                "totalAttempted": 1,
                "weaknessScore": 1.0
            },
            "Calculation": {
                "incorrectCount": 1,
                "totalAttempted": 2,
                "weaknessScore": 0.5
            }
        },
        "overallAccuracy": 0.6,  # 3/5 correct
        "lastUpdated": firestore.SERVER_TIMESTAMP
    }
    student_ref.collection("performanceSummary").document("summary").set(perf_summary_data)
    print(f"  [Firestore] Saved to users/{student_uid}/performanceSummary/summary")

    print("\nStudent Progress seeding completed successfully!")

if __name__ == "__main__":
    try:
        seed_student_progress()
    except FileNotFoundError as e:
        print(e)
        sys.exit(1)
    except Exception as e:
        print(f"\nError during seeding: {e}")
        sys.exit(1)
