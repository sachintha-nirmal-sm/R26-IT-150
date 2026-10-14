"""
Push Game-Based Measurement collections to physics-learning-platform.

Uses the Firebase Auth REST API + Firestore REST API as the admin user
(role custom claim). Requires firestore.rules that allow admin writes
on the practicals collections.

Usage:
    python seed_firestore_live.py
"""

import json
import os
import sys
import urllib.error
import urllib.request
from datetime import datetime, timedelta, timezone

API_KEY = os.getenv(
    "FIREBASE_WEB_API_KEY",
    "AIzaSyB9PuIPzqUAFxa0N3OQ6te5p33RkL2aaY0",
)
PROJECT_ID = os.getenv("FIREBASE_PROJECT_ID", "physics-learning-platform")
STORAGE_BUCKET = f"{PROJECT_ID}.firebasestorage.app"
BASE = f"https://firestore.googleapis.com/v1/projects/{PROJECT_ID}/databases/(default)/documents"

STUDENT_EMAIL = "student@example.com"
STUDENT_PASSWORD = "student123"
ADMIN_EMAIL = "admin@example.com"
ADMIN_PASSWORD = "admin123"


def rfc3339(dt):
    return dt.astimezone(timezone.utc).isoformat().replace("+00:00", "Z")


def encode_value(value):
    if value is None:
        return {"nullValue": None}
    if isinstance(value, bool):
        return {"booleanValue": value}
    if isinstance(value, int) and not isinstance(value, bool):
        return {"integerValue": str(value)}
    if isinstance(value, float):
        return {"doubleValue": value}
    if isinstance(value, datetime):
        return {"timestampValue": rfc3339(value)}
    if isinstance(value, dict):
        return {
            "mapValue": {
                "fields": {k: encode_value(v) for k, v in value.items()}
            }
        }
    if isinstance(value, list):
        return {"arrayValue": {"values": [encode_value(v) for v in value]}}
    return {"stringValue": str(value)}


def to_doc(data):
    return {"fields": {k: encode_value(v) for k, v in data.items()}}


def http(method, url, payload=None, token=None):
    body = None if payload is None else json.dumps(payload).encode()
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    request = urllib.request.Request(url, data=body, headers=headers, method=method)
    try:
        with urllib.request.urlopen(request, timeout=30) as resp:
            raw = resp.read().decode()
            return resp.status, json.loads(raw) if raw else {}
    except urllib.error.HTTPError as e:
        raw = e.read().decode()
        try:
            parsed = json.loads(raw)
        except Exception:
            parsed = raw
        return e.code, parsed


def auth_user(email, password, display_name):
    status, res = http(
        "POST",
        f"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={API_KEY}",
        {"email": email, "password": password, "returnSecureToken": True},
    )
    if status == 200:
        print(f"  signed in {email} ({res['localId']})")
        return res["idToken"], res["localId"]

    message = ""
    if isinstance(res, dict):
        message = res.get("error", {}).get("message", str(res))
    if message != "EMAIL_NOT_FOUND":
        raise RuntimeError(f"Login failed for {email}: {status} {res}")

    status, res = http(
        "POST",
        f"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={API_KEY}",
        {"email": email, "password": password, "returnSecureToken": True},
    )
    if status != 200:
        raise RuntimeError(f"Signup failed for {email}: {status} {res}")
    http(
        "POST",
        f"https://identitytoolkit.googleapis.com/v1/accounts:update?key={API_KEY}",
        {"idToken": res["idToken"], "displayName": display_name, "returnSecureToken": True},
    )
    print(f"  created {email} ({res['localId']})")
    return res["idToken"], res["localId"]


def upsert(token, collection, doc_id, data):
    return upsert_path(token, f"{collection}/{doc_id}", data)


def upsert_path(token, path, data):
    status, res = http(
        "PATCH",
        f"{BASE}/{path}",
        to_doc(data),
        token,
    )
    if status not in (200, 201):
        raise RuntimeError(f"Failed writing {path}: {status} {res}")
    print(f"  [ok] {path}")


def delete_doc(token, collection, doc_id):
    status, res = http(
        "DELETE",
        f"{BASE}/{collection}/{doc_id}",
        token=token,
    )
    if status in (200, 201, 404):
        print(f"  [del] {collection}/{doc_id}")
        return
    print(f"  [warn] could not delete {collection}/{doc_id}: {status} {res}")


def now():
    return datetime.now(timezone.utc)


def seed():
    print(f"Connecting to Firestore project: {PROJECT_ID}")
    student_token, student_uid = auth_user(STUDENT_EMAIL, STUDENT_PASSWORD, "Jane Student")
    admin_token, admin_uid = auth_user(ADMIN_EMAIL, ADMIN_PASSWORD, "System Admin")
    token = admin_token
    ts = now()

    print("\nCleaning connect-test leftover...")
    delete_doc(token, "practicals", "connect-test")

    print("\nSeeding users...")
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

    print("\nSeeding lessons (parent docs for practicals)...")
    lessons = [
        ("phy-g9-force-doc", {
            "title": "Basic Concepts Associated with Force", "subject": "Physics", "grade": 9,
            "lessonTag": "phy-g9-force",
            "description": "Force as a push or pull, and how force changes the motion of an object.",
            "order": 1, "status": "published",
        }),
        ("phy-g9-pressure-solid-doc", {
            "title": "Pressure Exerted by Solid", "subject": "Physics", "grade": 9,
            "lessonTag": "phy-g9-pressure-solid",
            "description": "Pressure of solids, a wire cutting soap, and how force changes cutting time.",
            "order": 2, "status": "published",
        }),
        ("phy-g9-density-doc", {
            "title": "Density", "subject": "Physics", "grade": 9,
            "lessonTag": "phy-g9-density",
            "description": "Mass, volume, and density of solids and liquids.",
            "order": 1, "status": "published",
        }),
        ("phy-g9-oscillations-doc", {
            "title": "Oscillations", "subject": "Physics", "grade": 9,
            "lessonTag": "phy-g9-oscillations",
            "description": "Simple pendulum motion, time period, and length.",
            "order": 2, "status": "published",
        }),
        ("phy-g10-motion-doc", {
            "title": "Introduction to Motion", "subject": "Physics", "grade": 10,
            "lessonTag": "phy-g10-motion",
            "description": "Kinematics, speed, velocity, acceleration, and distance-time graphs.",
            "order": 1, "status": "published",
        }),
        ("phy-g10-forces-doc", {
            "title": "Forces and Newton's Laws", "subject": "Physics", "grade": 10,
            "lessonTag": "phy-g10-forces",
            "description": "Force types, Newton's three laws of motion, inertia, and friction.",
            "order": 2, "status": "published",
        }),
        ("phy-g10-work-energy-doc", {
            "title": "Work, Energy, and Power", "subject": "Physics", "grade": 10,
            "lessonTag": "phy-g10-work-energy",
            "description": "Work done, kinetic and potential energy, conservation of energy.",
            "order": 3, "status": "published",
        }),
        ("phy-g11-waves-doc", {
            "title": "Waves", "subject": "Physics", "grade": 11,
            "lessonTag": "phy-g11-waves",
            "description": "Wave properties, ripple tanks, frequency and wavelength.",
            "order": 1, "status": "published",
        }),
    ]
    for lesson_id, data in lessons:
        upsert(token, "lessons", lesson_id, {
            **data,
            "createdBy": admin_uid,
            "lastEditedBy": admin_uid,
            "materialsCount": 0,
            "createdAt": ts,
            "updatedAt": ts,
        })

    print("\nSeeding topics...")
    topics = [
        ("topic-g9-force", {
            "grade": 9, "lessonId": "phy-g9-force-doc", "name": "Basic Concepts Associated with Force",
            "description": "Apply a force to a block and relate force, mass, and acceleration.",
            "order": 1, "isActive": True,
        }),
        ("topic-g9-pressure-solid", {
            "grade": 9, "lessonId": "phy-g9-pressure-solid-doc", "name": "Pressure Exerted by Solids",
            "description": "Hang sandbags on a thin wire and relate force to the time to cut through soap.",
            "order": 2, "isActive": True,
        }),
        ("topic-g9-density", {
            "grade": 9, "lessonId": "phy-g9-density-doc", "name": "Density",
            "description": "Measure mass and volume to calculate density of water.",
            "order": 1, "isActive": True,
        }),
        ("topic-g9-oscillations", {
            "grade": 9, "lessonId": "phy-g9-oscillations-doc", "name": "Pendulum Oscillations",
            "description": "Study the relationship between pendulum length and time period.",
            "order": 2, "isActive": True,
        }),
        ("topic-g10-motion", {
            "grade": 10, "lessonId": "phy-g10-motion-doc", "name": "Motion",
            "description": "Kinematics practicals: distance, velocity, and acceleration.",
            "order": 1, "isActive": True,
        }),
        ("topic-g10-forces", {
            "grade": 10, "lessonId": "phy-g10-forces-doc", "name": "Forces",
            "description": "Newton's laws and friction practicals.",
            "order": 2, "isActive": True,
        }),
        ("topic-g11-waves", {
            "grade": 11, "lessonId": "phy-g11-waves-doc", "name": "Waves",
            "description": "Ripple-tank wave measurement practicals.",
            "order": 1, "isActive": True,
        }),
    ]
    for topic_id, data in topics:
        upsert(token, "topics", topic_id, {**data, "createdAt": ts, "updatedAt": ts})

    print("\nSeeding practicals...")
    practicals = [
        ("grade9_force_basic", {
            "title": "Basic Concepts Associated with Force", "grade": 9,
            "lessonId": "phy-g9-force-doc", "topicId": "topic-g9-force",
            "description": "Apply a push to a block and check that acceleration is a = F / m.",
            "unitySceneId": "ForceBasicConcepts",
            "unityBuildUrl": "",
            "maxScore": 100, "durationSeconds": 600, "order": 1,
        }),
        ("grade9_pressure_solid", {
            "title": "Pressure Exerted by Solids", "grade": 9,
            "lessonId": "phy-g9-pressure-solid-doc", "topicId": "topic-g9-pressure-solid",
            "description": "Hang sandbags on a thin wire and time how fast it cuts through a cake of soap.",
            "unitySceneId": "PressureExertedBySolid",
            "unityBuildUrl": "",
            "maxScore": 100, "durationSeconds": 600, "order": 2,
        }),
        ("grade9_density_water", {
            "title": "Density of Water 1", "grade": 9,
            "lessonId": "phy-g9-density-doc", "topicId": "topic-g9-density",
            "description": "Measure mass and volume of water and calculate density.",
            "unitySceneId": "DensityWaterExperiment",
            "unityBuildUrl": "",
            "maxScore": 100, "durationSeconds": 600, "order": 1,
        }),
        ("grade9_pendulum", {
            "title": "Pendulum Oscillations", "grade": 9,
            "lessonId": "phy-g9-oscillations-doc", "topicId": "topic-g9-oscillations",
            "description": "Study the relationship between pendulum length and time period.",
            "unitySceneId": "PendulumOscillationsScene",
            "maxScore": 10, "durationSeconds": 600, "order": 2,
        }),
        ("grade10_newtons_laws", {
            "title": "Newton's Laws of Motion", "grade": 10,
            "lessonId": "phy-g10-forces-doc", "topicId": "topic-g10-forces",
            "description": "Explore the fundamental laws governing the motion of objects.",
            "unitySceneId": "NewtonsLawsScene",
            "maxScore": 10, "durationSeconds": 600, "order": 1,
        }),
        ("grade10_friction", {
            "title": "Friction & Surfaces", "grade": 10,
            "lessonId": "phy-g10-forces-doc", "topicId": "topic-g10-forces",
            "description": "Analyze how different surfaces affect the movement of blocks.",
            "unitySceneId": "FrictionSurfacesScene",
            "maxScore": 10, "durationSeconds": 480, "order": 2,
        }),
        ("grade10_motion_trolley", {
            "title": "Motion of a Trolley", "grade": 10,
            "lessonId": "phy-g10-motion-doc", "topicId": "topic-g10-motion",
            "description": "Measure distance, time, and velocity of a trolley under constant force.",
            "unitySceneId": "MotionTrolleyScene",
            "maxScore": 10, "durationSeconds": 600, "order": 1,
        }),
        ("grade11_wave_ripple", {
            "title": "Ripple Tank Waves", "grade": 11,
            "lessonId": "phy-g11-waves-doc", "topicId": "topic-g11-waves",
            "description": "Measure wavelength and frequency in a virtual ripple tank.",
            "unitySceneId": "RippleTankScene",
            "maxScore": 10, "durationSeconds": 720, "order": 1,
        }),
    ]
    for practical_id, data in practicals:
        upsert(token, "practicals", practical_id, {
            **data,
            "unityBuildUrl": data.get("unityBuildUrl")
            or f"gs://{STORAGE_BUCKET}/practicals/{practical_id}/build",
            "demoAllowed": True,
            "demoMaxAttempts": 10 if practical_id in ("grade9_density_water", "grade9_force_basic", "grade9_pressure_solid") else 1,
            "practicalMaxAttempts": 3 if practical_id in ("grade9_density_water", "grade9_force_basic", "grade9_pressure_solid") else 1,
            "isActive": True,
            "createdAt": ts,
            "updatedAt": ts,
        })

    print("\nSeeding practicalResults...")
    demo_started = ts - timedelta(minutes=20)
    demo_completed = demo_started + timedelta(seconds=180)
    official_started = ts - timedelta(minutes=12)
    official_completed = official_started + timedelta(seconds=372)
    friction_demo_started = ts - timedelta(minutes=8)
    friction_demo_completed = friction_demo_started + timedelta(seconds=95)

    upsert(token, "practicalResults", "result-newtons-demo-1", {
        "studentId": student_uid,
        "practicalId": "grade10_newtons_laws",
        "grade": 10,
        "attemptType": "demo",
        "attemptNumber": 1,
        "score": 6,
        "maxScore": 10,
        "percentage": 60,
        "startedAt": demo_started,
        "completedAt": demo_completed,
        "durationSeconds": 180,
        "status": "completed",
        "measurements": {"mass": 2.0, "initialVelocity": 5.0, "force": 10.0, "time": 2.0},
        "calculations": {"acceleration": 5.0, "finalVelocity": 15.0},
        "evaluation": {"apparatus": 2, "procedure": 2, "accuracy": 2},
    })
    upsert(token, "practicalResults", "result-newtons-practical-1", {
        "studentId": student_uid,
        "practicalId": "grade10_newtons_laws",
        "grade": 10,
        "attemptType": "practical",
        "attemptNumber": 1,
        "score": 8,
        "maxScore": 10,
        "percentage": 80,
        "startedAt": official_started,
        "completedAt": official_completed,
        "durationSeconds": 372,
        "status": "completed",
        "measurements": {
            "mass": 2.0, "initialVelocity": 5.0, "force": 10.0,
            "time": 3.0, "distance": 22.5,
        },
        "calculations": {"acceleration": 5.0, "finalVelocity": 20.0},
        "evaluation": {"apparatus": 2, "procedure": 3, "accuracy": 3},
    })
    upsert(token, "practicalResults", "result-friction-demo-1", {
        "studentId": student_uid,
        "practicalId": "grade10_friction",
        "grade": 10,
        "attemptType": "demo",
        "attemptNumber": 1,
        "score": 5,
        "maxScore": 10,
        "percentage": 50,
        "startedAt": friction_demo_started,
        "completedAt": friction_demo_completed,
        "durationSeconds": 95,
        "status": "completed",
        "measurements": {"mass": 1.0, "appliedForce": 4.0, "surfaceType": "wood"},
        "calculations": {"frictionForce": 2.0, "coefficient": 0.2},
        "evaluation": {"apparatus": 2, "procedure": 2, "accuracy": 1},
    })

    print("\nSeeding studentPracticals...")
    records = [
        ("grade10_newtons_laws", {
            "demoAttemptsUsed": 1, "practicalAttemptsUsed": 1, "demoCompleted": True,
            "bestScore": 8, "latestScore": 8, "percentage": 80, "completed": True,
            "currentState": "SUBMITTED", "activeStartedAt": None,
            "lastAttemptAt": official_completed,
        }),
        ("grade10_friction", {
            "demoAttemptsUsed": 1, "practicalAttemptsUsed": 0, "demoCompleted": True,
            "bestScore": 0, "latestScore": 0, "percentage": 0, "completed": False,
            "currentState": "PRACTICAL_AVAILABLE", "activeStartedAt": None,
            "lastAttemptAt": friction_demo_completed,
        }),
        ("grade10_motion_trolley", {
            "demoAttemptsUsed": 0, "practicalAttemptsUsed": 0, "demoCompleted": False,
            "bestScore": 0, "latestScore": 0, "percentage": 0, "completed": False,
            "currentState": "AVAILABLE", "activeStartedAt": None,
            "lastAttemptAt": None,
        }),
    ]
    for practical_id, data in records:
        upsert(token, "studentPracticals", f"{student_uid}_{practical_id}", {
            "studentId": student_uid,
            "practicalId": practical_id,
            "grade": 10,
            **data,
        })

    print("\nSeeding studentProgress...")
    upsert(token, "studentProgress", student_uid, {
        "studentId": student_uid,
        "grade": 10,
        "totalPracticals": 6,
        "completedPracticals": 1,
        "totalScore": 8,
        "averagePercentage": 80,
        "gradeProgress": {
            "9": {"totalPracticals": 2, "completedPracticals": 0, "totalScore": 0, "averagePercentage": 0},
            "10": {"totalPracticals": 3, "completedPracticals": 1, "totalScore": 8, "averagePercentage": 80},
            "11": {"totalPracticals": 1, "completedPracticals": 0, "totalScore": 0, "averagePercentage": 0},
        },
        "lessonProgress": {
            "phy-g10-forces-doc": {
                "totalPracticals": 2, "completedPracticals": 1, "averagePercentage": 80,
            },
            "phy-g10-motion-doc": {
                "totalPracticals": 1, "completedPracticals": 0, "averagePercentage": 0,
            },
        },
        "updatedAt": ts,
    })

    print("\nDone. Open Firebase Console:")
    print(f"  https://console.firebase.google.com/project/{PROJECT_ID}/firestore")
    print("Collections to check: topics, practicals, practicalResults, studentPracticals, studentProgress")


if __name__ == "__main__":
    try:
        seed()
    except Exception as e:
        print(f"\nERROR: {e}")
        sys.exit(1)
