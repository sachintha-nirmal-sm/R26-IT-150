"""
seed_student_practicals.py — Seed student practical progress and results.

Writes, for the seeded student (student@example.com):
  1. practicalResults/{resultId}     — immutable demo + official completions
  2. studentPracticals/{uid}_{id}    — per-practical progress / state machine
  3. studentProgress/{uid}           — aggregated profile/dashboard rollup

Attempt model (Word doc section 4):
  Try Demo       — no timer, 1 attempt, does NOT count as official score
  Start Practical — timed, 1 attempt, IS the official result

Profile update rule (Word doc section 10):
  bestScore = max(previous, submitted)
  latestScore = submitted
  completed = true after first official submission
  completedPracticals increases only on first official completion
  averagePercentage is from completed official practicals only
  gradeProgress[9|10|11] maintained separately

Requires seed_users.py and seed_practicals.py.

Usage:
    python seed/seed_student_practicals.py
"""

import sys
import os
from datetime import datetime, timedelta, timezone
from google.cloud import firestore

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from firebase_init import db

NEWTONS = "grade10_newtons_laws"
FRICTION = "grade10_friction"
TROLLEY = "grade10_motion_trolley"


def get_student():
    students = db.collection("users").where("role", "==", "student").limit(1).get()
    if not students:
        return None
    snap = students[0]
    return snap.id, snap.to_dict()


def require_practical(practical_id):
    snap = db.collection("practicals").document(practical_id).get()
    if not snap.exists:
        print(f"Practical '{practical_id}' not found. Run seed_practicals.py first.")
        sys.exit(1)
    return snap.to_dict()


def seed_student_practicals():
    print("Starting Student Practicals Seeding script...")

    student = get_student()
    if not student:
        print("Student user not found! Please run seed_users.py first.")
        sys.exit(1)

    student_uid, student_data = student
    student_grade = student_data.get("currentGrade", 10)
    print(f"Using Student UID: {student_uid} (grade {student_grade})")

    newtons = require_practical(NEWTONS)
    friction = require_practical(FRICTION)
    require_practical(TROLLEY)

    now = datetime.now(timezone.utc)
    demo_started = now - timedelta(minutes=20)
    demo_completed = demo_started + timedelta(seconds=180)
    official_started = now - timedelta(minutes=12)
    official_completed = official_started + timedelta(seconds=372)
    friction_demo_started = now - timedelta(minutes=8)
    friction_demo_completed = friction_demo_started + timedelta(seconds=95)

    # ------------------------------------------------------------------
    # 1. practicalResults — immutable attempt records
    # ------------------------------------------------------------------
    print("\nSeeding practicalResults...")

    results = [
        {
            "id": "result-newtons-demo-1",
            "data": {
                "studentId": student_uid,
                "practicalId": NEWTONS,
                "grade": 10,
                "attemptType": "demo",
                "attemptNumber": 1,
                "score": 6,
                "maxScore": newtons["maxScore"],
                "percentage": 60,
                "startedAt": demo_started,
                "completedAt": demo_completed,
                "durationSeconds": 180,
                "status": "completed",
                "measurements": {
                    "mass": 2.0,
                    "initialVelocity": 5.0,
                    "force": 10.0,
                    "time": 2.0,
                },
                "calculations": {
                    "acceleration": 5.0,
                    "finalVelocity": 15.0,
                },
                "evaluation": {
                    "apparatus": 2,
                    "procedure": 2,
                    "accuracy": 2,
                },
            },
        },
        {
            "id": "result-newtons-practical-1",
            "data": {
                "studentId": student_uid,
                "practicalId": NEWTONS,
                "grade": 10,
                "attemptType": "practical",
                "attemptNumber": 1,
                "score": 8,
                "maxScore": newtons["maxScore"],
                "percentage": 80,
                "startedAt": official_started,
                "completedAt": official_completed,
                "durationSeconds": 372,
                "status": "completed",
                "measurements": {
                    "mass": 2.0,
                    "initialVelocity": 5.0,
                    "force": 10.0,
                    "time": 3.0,
                    "distance": 22.5,
                },
                "calculations": {
                    "acceleration": 5.0,
                    "finalVelocity": 20.0,
                },
                "evaluation": {
                    "apparatus": 2,
                    "procedure": 3,
                    "accuracy": 3,
                },
            },
        },
        {
            "id": "result-friction-demo-1",
            "data": {
                "studentId": student_uid,
                "practicalId": FRICTION,
                "grade": 10,
                "attemptType": "demo",
                "attemptNumber": 1,
                "score": 5,
                "maxScore": friction["maxScore"],
                "percentage": 50,
                "startedAt": friction_demo_started,
                "completedAt": friction_demo_completed,
                "durationSeconds": 95,
                "status": "completed",
                "measurements": {
                    "mass": 1.0,
                    "appliedForce": 4.0,
                    "surfaceType": "wood",
                },
                "calculations": {
                    "frictionForce": 2.0,
                    "coefficient": 0.2,
                },
                "evaluation": {
                    "apparatus": 2,
                    "procedure": 2,
                    "accuracy": 1,
                },
            },
        },
    ]

    results_ref = db.collection("practicalResults")
    for result in results:
        results_ref.document(result["id"]).set(result["data"])
        print(
            f"  [Firestore] Saved practicalResults/{result['id']} "
            f"({result['data']['attemptType']}, {result['data']['percentage']}%)"
        )

    # ------------------------------------------------------------------
    # 2. studentPracticals — one doc per student+practical (composite ID)
    # ------------------------------------------------------------------
    print("\nSeeding studentPracticals...")

    student_practicals = [
        {
            "practicalId": NEWTONS,
            "grade": 10,
            "demoAttemptsUsed": 1,
            "practicalAttemptsUsed": 1,
            "demoCompleted": True,
            "bestScore": 8,
            "latestScore": 8,
            "percentage": 80,
            "completed": True,
            "currentState": "SUBMITTED",
            "activeStartedAt": None,
            "lastAttemptAt": official_completed,
        },
        {
            "practicalId": FRICTION,
            "grade": 10,
            "demoAttemptsUsed": 1,
            "practicalAttemptsUsed": 0,
            "demoCompleted": True,
            "bestScore": 0,
            "latestScore": 0,
            "percentage": 0,
            "completed": False,
            "currentState": "PRACTICAL_AVAILABLE",
            "activeStartedAt": None,
            "lastAttemptAt": friction_demo_completed,
        },
        {
            "practicalId": TROLLEY,
            "grade": 10,
            "demoAttemptsUsed": 0,
            "practicalAttemptsUsed": 0,
            "demoCompleted": False,
            "bestScore": 0,
            "latestScore": 0,
            "percentage": 0,
            "completed": False,
            "currentState": "AVAILABLE",
            "activeStartedAt": None,
            "lastAttemptAt": None,
        },
    ]

    sp_ref = db.collection("studentPracticals")
    for record in student_practicals:
        doc_id = f"{student_uid}_{record['practicalId']}"
        payload = {
            "studentId": student_uid,
            **record,
        }
        sp_ref.document(doc_id).set(payload)
        print(
            f"  [Firestore] Saved studentPracticals/{doc_id} "
            f"(state={record['currentState']})"
        )

    # ------------------------------------------------------------------
    # 3. studentProgress — profile rollup (doc ID = student uid)
    # Official Newton's Laws 8/10 is the only completed official practical.
    # Demo scores are excluded from averages (Word doc section 4 + 10).
    # ------------------------------------------------------------------
    print("\nSeeding studentProgress...")

    progress_data = {
        "studentId": student_uid,
        "grade": student_grade,
        "totalPracticals": 6,
        "completedPracticals": 1,
        "totalScore": 8,
        "averagePercentage": 80,
        "gradeProgress": {
            "9": {
                "totalPracticals": 2,
                "completedPracticals": 0,
                "totalScore": 0,
                "averagePercentage": 0,
            },
            "10": {
                "totalPracticals": 3,
                "completedPracticals": 1,
                "totalScore": 8,
                "averagePercentage": 80,
            },
            "11": {
                "totalPracticals": 1,
                "completedPracticals": 0,
                "totalScore": 0,
                "averagePercentage": 0,
            },
        },
        "lessonProgress": {
            "phy-g10-forces-doc": {
                "totalPracticals": 2,
                "completedPracticals": 1,
                "averagePercentage": 80,
            },
            "phy-g10-motion-doc": {
                "totalPracticals": 1,
                "completedPracticals": 0,
                "averagePercentage": 0,
            },
        },
        "updatedAt": firestore.SERVER_TIMESTAMP,
    }
    db.collection("studentProgress").document(student_uid).set(progress_data)
    print(f"  [Firestore] Saved studentProgress/{student_uid}")

    print("\nStudent practicals seeding completed successfully!")


if __name__ == "__main__":
    try:
        seed_student_practicals()
    except FileNotFoundError as e:
        print(e)
        sys.exit(1)
    except Exception as e:
        print(f"\nError during seeding: {e}")
        sys.exit(1)
