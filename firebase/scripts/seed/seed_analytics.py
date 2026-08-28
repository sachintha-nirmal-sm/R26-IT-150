"""
seed_analytics.py — Seed sample analytics documents.

This script creates one sample document each in:
  1. analytics/global/lessonStats/phy-g10-motion-doc
  2. analytics/global/quizStats/phy-g10-motion-quiz
  3. analytics/global/questionStats/q1

This matches Sections 3.19 and 3.20 in devmini.md.
"""

import sys
import os
from google.cloud import firestore

# Allow imports from the parent directory (firebase/scripts)
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from firebase_init import db

def seed_analytics():
    print("Starting Analytics Seeding script...")

    # Define IDs from earlier tasks
    lesson_id = "phy-g10-motion-doc"
    quiz_id = "phy-g10-motion-quiz"
    question_id = "q1"

    # Base reference under analytics/global
    global_analytics_ref = db.collection("analytics").document("global")

    # 1. Seed analytics/global/lessonStats/{lessonId} (Section 3.19)
    print(f"\nSeeding lessonStats for {lesson_id}...")
    lesson_stats_data = {
        "lessonId": lesson_id,
        "totalAttempts": 150,
        "averageScorePercent": 72.5,
        "weakStudentCount": 12,
        "mostMissedQuestionIds": ["q2", "q5"],
        "lastUpdated": firestore.SERVER_TIMESTAMP
    }
    global_analytics_ref.collection("lessonStats").document(lesson_id).set(lesson_stats_data)
    print(f"  [Firestore] Saved to analytics/global/lessonStats/{lesson_id}")

    # 2. Seed analytics/global/quizStats/{quizId} (Section 3.20)
    print(f"\nSeeding quizStats for {quiz_id}...")
    quiz_stats_data = {
        "totalAttempts": 85,
        "averageScorePercent": 68.4,
        "lastUpdated": firestore.SERVER_TIMESTAMP
    }
    global_analytics_ref.collection("quizStats").document(quiz_id).set(quiz_stats_data)
    print(f"  [Firestore] Saved to analytics/global/quizStats/{quiz_id}")

    # 3. Seed analytics/global/questionStats/{questionId} (Section 3.20)
    print(f"\nSeeding questionStats for {question_id}...")
    question_stats_data = {
        "totalAnswered": 220,
        "incorrectRate": 0.35,  # 35% incorrect
        "lastUpdated": firestore.SERVER_TIMESTAMP
    }
    global_analytics_ref.collection("questionStats").document(question_id).set(question_stats_data)
    print(f"  [Firestore] Saved to analytics/global/questionStats/{question_id}")

    print("\nAnalytics seeding completed successfully!")

if __name__ == "__main__":
    try:
        seed_analytics()
    except FileNotFoundError as e:
        print(e)
        sys.exit(1)
    except Exception as e:
        print(f"\nError during seeding: {e}")
        sys.exit(1)
