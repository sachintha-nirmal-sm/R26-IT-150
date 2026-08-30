"""
Seed the Firestore schema from:
  mobile_app/Firestore Database Architecture .md
  firebase/firestore.rules (Section 10)

Writes to physics-learning-platform as the admin user. Requires the repo
firestore.rules file to be Published in Firebase Console (the live default
deny-all rules block every client write).

Usage:
    python seed_architecture_live.py
"""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from seed_firestore_live import (
    ADMIN_EMAIL,
    ADMIN_PASSWORD,
    STUDENT_EMAIL,
    STUDENT_PASSWORD,
    auth_user,
    now,
    upsert,
    upsert_path,
)


LESSON_ID = "phy-g10-motion-doc"
QUIZ_ID = "phy-g10-motion-quiz"
VERSION_ID = "v1"


def seed_architecture():
    print("Seeding Firestore Database Architecture schema...")
    student_uid = auth_user(
        STUDENT_EMAIL, STUDENT_PASSWORD, "Jane Student"
    )[1]
    admin_token, admin_uid = auth_user(
        ADMIN_EMAIL, ADMIN_PASSWORD, "System Admin"
    )
    token = admin_token
    ts = now()

    print("\n[3.1] users/{uid}")
    upsert(token, "users", admin_uid, {
        "role": "admin",
        "fullName": "System Admin",
        "email": ADMIN_EMAIL,
        "status": "active",
        "createdAt": ts,
        "updatedAt": ts,
    })
    upsert(token, "users", student_uid, {
        "role": "student",
        "fullName": "Jane Student",
        "email": STUDENT_EMAIL,
        "currentGrade": 10,
        "enrollmentYear": 2026,
        "lastPromotedAt": None,
        "status": "active",
        "createdAt": ts,
        "updatedAt": ts,
    })

    print("\n[3.2] users/{uid}/gradeHistory")
    upsert_path(token, f"users/{student_uid}/gradeHistory/enroll-2026", {
        "fromGrade": 9,
        "toGrade": 10,
        "promotedAt": ts,
        "triggeredBy": "adminOverride",
    })

    print("\n[3.3] lessons/{lessonId}")
    lessons = [
        ("phy-g10-motion-doc", {
            "title": "Introduction to Motion",
            "subject": "Physics",
            "grade": 10,
            "lessonTag": "phy-g10-motion",
            "description": "Kinematics, speed, velocity, acceleration, and distance-time graphs.",
            "order": 1,
        }),
        ("phy-g10-forces-doc", {
            "title": "Forces and Newton's Laws",
            "subject": "Physics",
            "grade": 10,
            "lessonTag": "phy-g10-forces",
            "description": "Force types, Newton's three laws of motion, inertia, and friction.",
            "order": 2,
        }),
        ("phy-g10-work-energy-doc", {
            "title": "Work, Energy, and Power",
            "subject": "Physics",
            "grade": 10,
            "lessonTag": "phy-g10-work-energy",
            "description": "Work done, kinetic and potential energy, conservation of energy.",
            "order": 3,
        }),
    ]
    for lesson_id, data in lessons:
        upsert(token, "lessons", lesson_id, {
            **data,
            "status": "published",
            "createdBy": admin_uid,
            "lastEditedBy": admin_uid,
            "materialsCount": 0,
            "createdAt": ts,
            "updatedAt": ts,
        })

    print("\n[3.4] lessons/{id}/materials")
    upsert_path(token, f"lessons/{LESSON_ID}/materials/kinematics-notes-pdf", {
        "fileName": "kinematics_lecture_notes_v1.pdf",
        "materialType": "pdf",
        "storagePath": f"materials/{LESSON_ID}/kinematics_lecture_notes_v1.pdf",
        "fileSizeBytes": 2048576,
        "ingestionStatus": "embedded",
        "chunkCount": 42,
        "uploadedBy": admin_uid,
        "uploadedAt": ts,
        "lastProcessedAt": ts,
    })
    upsert_path(token, f"lessons/{LESSON_ID}/materials/motion-formula-sheet", {
        "fileName": "motion_formulas_quick_ref.pdf",
        "materialType": "formulaSheet",
        "storagePath": f"materials/{LESSON_ID}/motion_formulas_quick_ref.pdf",
        "fileSizeBytes": 512400,
        "ingestionStatus": "uploaded",
        "chunkCount": 0,
        "uploadedBy": admin_uid,
        "uploadedAt": ts,
        "lastProcessedAt": None,
    })
    upsert(token, "lessons", LESSON_ID, {
        **lessons[0][1],
        "status": "published",
        "createdBy": admin_uid,
        "lastEditedBy": admin_uid,
        "materialsCount": 2,
        "createdAt": ts,
        "updatedAt": ts,
    })

    print("\n[3.5] lessons/{id}/quizzes")
    upsert_path(token, f"lessons/{LESSON_ID}/quizzes/{QUIZ_ID}", {
        "title": "Introduction to Motion Quiz",
        "lessonId": LESSON_ID,
        "maxAttempts": 3,
        "questionsPerAttempt": 20,
        "activeQuestionBankVersionId": VERSION_ID,
        "status": "bankReady",
        "createdAt": ts,
        "updatedAt": ts,
    })

    print("\n[3.6-3.7] questionBankVersions + questions")
    upsert_path(
        token,
        f"lessons/{LESSON_ID}/quizzes/{QUIZ_ID}/questionBankVersions/{VERSION_ID}",
        {
            "versionNumber": 1,
            "status": "active",
            "totalQuestions": 5,
            "generatedBy": "RAG",
            "generationJobId": "job-seed-v1-001",
            "sourceMaterialIds": ["kinematics-notes-pdf", "motion-formula-sheet"],
            "createdAt": ts,
            "archivedAt": None,
        },
    )
    questions = [
        ("q1", {
            "questionText": "What is the SI unit of speed?",
            "questionType": "Theory",
            "options": ["meters per second (m/s)", "kilometers per hour (km/h)", "meters per second squared (m/s^2)", "miles per hour (mph)"],
            "correctAnswer": "meters per second (m/s)",
            "explanation": "Speed is distance divided by time. SI unit is m/s.",
            "difficulty": "easy",
            "marks": 2,
        }),
        ("q2", {
            "questionText": "Which equation is valid for constant acceleration?",
            "questionType": "Formula",
            "options": ["v = u + at", "v^2 = u^2 - 2as", "s = ut - 0.5*at^2", "v = u + a*t^2"],
            "correctAnswer": "v = u + at",
            "explanation": "v = u + at is the definition of constant acceleration.",
            "difficulty": "easy",
            "marks": 2,
        }),
        ("q3", {
            "questionText": "A car accelerates from rest at 2 m/s^2. How far in 5 seconds?",
            "questionType": "Calculation",
            "options": ["5 meters", "10 meters", "25 meters", "50 meters"],
            "correctAnswer": "25 meters",
            "explanation": "s = ut + 0.5*at^2 = 0.5*2*25 = 25 meters.",
            "difficulty": "medium",
            "marks": 5,
        }),
        ("q4", {
            "questionText": "Explain the key difference between distance and displacement.",
            "questionType": "Theory",
            "options": None,
            "correctAnswer": "Distance is a scalar path length; displacement is a vector from start to end.",
            "explanation": "Distance has no direction; displacement does.",
            "difficulty": "medium",
            "marks": 4,
        }),
        ("q5", {
            "questionText": "A stone dropped from a cliff takes 4 s to fall. Height if g=10 m/s^2?",
            "questionType": "Calculation",
            "options": ["40 meters", "80 meters", "160 meters", "20 meters"],
            "correctAnswer": "80 meters",
            "explanation": "s = 0.5*g*t^2 = 0.5*10*16 = 80 meters.",
            "difficulty": "hard",
            "marks": 5,
        }),
    ]
    for qid, q in questions:
        upsert_path(
            token,
            f"lessons/{LESSON_ID}/quizzes/{QUIZ_ID}/questionBankVersions/{VERSION_ID}/questions/{qid}",
            {
                **q,
                "lessonTag": "phy-g10-motion",
                "generatedBy": "RAG",
                "sourceReference": "kinematics-notes-pdf#page=1",
                "createdAt": ts,
            },
        )

    print("\n[3.18] generationJobs")
    upsert(token, "generationJobs", "job-seed-v1-001", {
        "jobType": "questionBankGeneration",
        "targetLessonId": LESSON_ID,
        "targetQuizId": QUIZ_ID,
        "targetGrade": None,
        "llmModelUsed": "claude-sonnet-5",
        "status": "completed",
        "progressPercent": 100,
        "requestedBy": admin_uid,
        "resultVersionId": VERSION_ID,
        "errorMessage": None,
        "startedAt": ts,
        "completedAt": ts,
    })

    print("\n[3.14-3.15] finalQuizzes")
    upsert(token, "finalQuizzes", "grade10-round1", {
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
        "createdAt": ts,
        "archivedAt": None,
    })
    upsert_path(token, "finalQuizzes/grade10-round1/questions/fq-q1", {
        "questionText": "What does a horizontal line on a distance-time graph represent?",
        "questionType": "Theory",
        "options": ["Constant speed", "Acceleration", "Deceleration", "The object is at rest"],
        "correctAnswer": "The object is at rest",
        "explanation": "Distance is not changing, so the object is at rest.",
        "lessonTag": "phy-g10-motion",
        "sourceLessonId": "phy-g10-motion-doc",
        "difficulty": "easy",
        "marks": 3,
        "generatedBy": "RAG",
        "sourceReference": "kinematics-notes-pdf#page=1",
        "createdAt": ts,
    })

    print("\n[3.8-3.13, 3.16-3.17] student subcollections")
    upsert_path(token, f"users/{student_uid}/quizProgress/{QUIZ_ID}", {
        "quizId": QUIZ_ID,
        "lessonId": LESSON_ID,
        "attemptsUsed": 1,
        "isLocked": False,
        "bestScore": 11,
        "bestAttemptId": "attempt-1",
        "usedQuestionIds": ["q1", "q2", "q3", "q4", "q5"],
        "lastAttemptAt": ts,
    })
    upsert_path(token, f"users/{student_uid}/quizAttempts/attempt-1", {
        "quizId": QUIZ_ID,
        "lessonId": LESSON_ID,
        "questionBankVersionId": VERSION_ID,
        "attemptNumber": 1,
        "answers": [
            {"questionId": "q1", "studentAnswer": "meters per second (m/s)", "isCorrect": True, "marksAwarded": 2, "lessonTag": "phy-g10-motion", "difficulty": "easy", "questionType": "Theory"},
            {"questionId": "q2", "studentAnswer": "v = u + a*t^2", "isCorrect": False, "marksAwarded": 0, "lessonTag": "phy-g10-motion", "difficulty": "easy", "questionType": "Formula"},
        ],
        "score": 11,
        "totalMarks": 18,
        "timeTakenSeconds": 320,
        "startedAt": ts,
        "submittedAt": ts,
        "status": "completed",
    })
    upsert_path(token, f"users/{student_uid}/wrongQuestions/wrong-q2", {
        "questionId": "q2",
        "lessonId": LESSON_ID,
        "quizId": QUIZ_ID,
        "attemptId": "attempt-1",
        "questionBankVersionId": VERSION_ID,
        "questionText": "Which of the following equations is a valid kinematic equation for constant acceleration?",
        "studentAnswer": "v = u + a*t^2",
        "correctAnswer": "v = u + at",
        "explanation": "v = u + at represents constant acceleration.",
        "questionType": "Formula",
        "lessonTag": "phy-g10-motion",
        "difficulty": "easy",
        "reviewed": False,
        "createdAt": ts,
    })
    upsert_path(token, f"users/{student_uid}/feedback/feedback-attempt-1", {
        "attemptId": "attempt-1",
        "quizId": QUIZ_ID,
        "feedbackText": "Solid grasp of SI units. Revise kinematic formulas.",
        "strengths": ["Good grasp of SI units"],
        "weaknesses": ["Recalling kinematic formulas correctly"],
        "recommendedTopics": ["phy-g10-motion"],
        "llmModelUsed": "claude-sonnet-5",
        "generatedAt": ts,
    })
    upsert_path(token, f"users/{student_uid}/weakTopics/phy-g10-motion", {
        "lessonTag": "phy-g10-motion",
        "lessonId": LESSON_ID,
        "incorrectCount": 2,
        "totalAttempted": 5,
        "weaknessScore": 0.4,
        "byQuestionType": {
            "Theory": {"incorrectCount": 0, "totalAttempted": 2, "weaknessScore": 0.0},
            "Formula": {"incorrectCount": 1, "totalAttempted": 1, "weaknessScore": 1.0},
            "Calculation": {"incorrectCount": 1, "totalAttempted": 2, "weaknessScore": 0.5},
        },
        "contributingQuizTypes": ["lessonQuiz"],
        "lastUpdated": ts,
    })
    upsert_path(token, f"users/{student_uid}/youtubeRecommendations/rec-motion-1", {
        "lessonTag": "phy-g10-motion",
        "videoId": "K2R56zU-Y3k",
        "title": "Physics Kinematics Equations Made Easy",
        "channelName": "The Organic Chemistry Tutor",
        "thumbnailUrl": "https://img.youtube.com/vi/K2R56zU-Y3k/0.jpg",
        "videoUrl": "https://www.youtube.com/watch?v=K2R56zU-Y3k",
        "relevanceScore": 0.95,
        "generatedAt": ts,
    })
    upsert_path(token, f"users/{student_uid}/performanceSummary/summary", {
        "byQuestionType": {
            "Theory": {"incorrectCount": 0, "totalAttempted": 2, "weaknessScore": 0.0},
            "Formula": {"incorrectCount": 1, "totalAttempted": 1, "weaknessScore": 1.0},
            "Calculation": {"incorrectCount": 1, "totalAttempted": 2, "weaknessScore": 0.5},
        },
        "overallAccuracy": 0.6,
        "lastUpdated": ts,
    })
    upsert_path(token, f"users/{student_uid}/finalQuizAttempts/final-attempt-1", {
        "finalQuizId": "grade10-round1",
        "roundNumber": 1,
        "attemptNumber": 1,
        "answerCheckingModel": "gpt-4o",
        "answers": [
            {"questionId": "fq-q1", "studentAnswer": "The object is at rest", "isCorrect": True, "marksAwarded": 3, "lessonTag": "phy-g10-motion", "difficulty": "easy", "questionType": "Theory"},
        ],
        "score": 12,
        "totalMarks": 12,
        "timeTakenSeconds": 450,
        "startedAt": ts,
        "submittedAt": ts,
        "status": "completed",
    })

    print("\n[3.19-3.20] analytics")
    upsert_path(token, f"analytics/global/lessonStats/{LESSON_ID}", {
        "lessonId": LESSON_ID,
        "totalAttempts": 150,
        "averageScorePercent": 72.5,
        "weakStudentCount": 12,
        "mostMissedQuestionIds": ["q2", "q5"],
        "lastUpdated": ts,
    })
    upsert_path(token, f"analytics/global/quizStats/{QUIZ_ID}", {
        "totalAttempts": 85,
        "averageScorePercent": 68.4,
        "lastUpdated": ts,
    })
    upsert_path(token, "analytics/global/questionStats/q1", {
        "totalAnswered": 220,
        "incorrectRate": 0.35,
        "lastUpdated": ts,
    })

    print("\nArchitecture schema seeding completed.")
    print("Console: https://console.firebase.google.com/project/physics-learning-platform/firestore")


if __name__ == "__main__":
    try:
        seed_architecture()
    except Exception as e:
        print(f"\nERROR: {e}")
        sys.exit(1)
