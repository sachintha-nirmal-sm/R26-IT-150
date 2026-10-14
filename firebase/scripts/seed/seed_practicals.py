"""
seed_practicals.py — Seed Unity practical master configuration.

Writes practicals/{practicalId} documents. Attempt limits, timer duration,
and maxScore live here so the backend — not the Flutter/Unity client — is
the source of truth (Word doc sections 4 and 11).

Requires topics (and their parent lessons) from seed_topics.py.

Usage:
    python seed/seed_practicals.py
"""

import sys
import os
from google.cloud import firestore

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from firebase_init import db

STORAGE_BUCKET = os.getenv(
    "STORAGE_BUCKET",
    "physics-learning-platform.firebasestorage.app",
)

# One practical per Unity scene. Doc IDs follow the Word-doc convention
# grade{N}_{slug} so they stay human-readable and collision-proof.
PRACTICALS = [
    {
        "id": "grade9_force_basic",
        "data": {
            "title": "Basic Concepts Associated with Force",
            "grade": 9,
            "lessonId": "phy-g9-force-doc",
            "topicId": "topic-g9-force",
            "description": "Apply a push to a block and check that acceleration is a = F / m.",
            "unitySceneId": "ForceBasicConcepts",
            "unityBuildUrl": "",
            "maxScore": 100,
            "durationSeconds": 600,
            "demoAllowed": True,
            "demoMaxAttempts": 10,
            "practicalMaxAttempts": 3,
            "isActive": True,
            "order": 1,
        },
    },
    {
        "id": "grade9_pressure_solid",
        "data": {
            "title": "Pressure Exerted by Solids",
            "grade": 9,
            "lessonId": "phy-g9-pressure-solid-doc",
            "topicId": "topic-g9-pressure-solid",
            "description": "Hang sandbags on a thin wire and time how fast it cuts through a cake of soap.",
            "unitySceneId": "PressureExertedBySolid",
            "unityBuildUrl": "",
            "maxScore": 100,
            "durationSeconds": 600,
            "demoAllowed": True,
            "demoMaxAttempts": 10,
            "practicalMaxAttempts": 3,
            "isActive": True,
            "order": 1,
        },
    },
    {
        "id": "grade9_density_water",
        "data": {
            "title": "Density of Water 1",
            "grade": 9,
            "lessonId": "phy-g9-density-doc",
            "topicId": "topic-g9-density",
            "description": "Measure mass and volume of water and calculate density.",
            "unitySceneId": "DensityWaterExperiment",
            "unityBuildUrl": "",
            "maxScore": 100,
            "durationSeconds": 600,
            "demoAllowed": True,
            "demoMaxAttempts": 10,
            "practicalMaxAttempts": 3,
            "isActive": True,
            "order": 1,
        },
    },
    {
        "id": "grade9_pendulum",
        "data": {
            "title": "Pendulum Oscillations",
            "grade": 9,
            "lessonId": "phy-g9-oscillations-doc",
            "topicId": "topic-g9-oscillations",
            "description": "Study the relationship between pendulum length and time period.",
            "unitySceneId": "PendulumOscillationsScene",
            "unityBuildUrl": f"gs://{STORAGE_BUCKET}/practicals/grade9_pendulum/build",
            "maxScore": 10,
            "durationSeconds": 600,
            "demoAllowed": True,
            "demoMaxAttempts": 1,
            "practicalMaxAttempts": 1,
            "isActive": True,
            "order": 2,
        },
    },
    {
        "id": "grade10_newtons_laws",
        "data": {
            "title": "Newton's Laws of Motion",
            "grade": 10,
            "lessonId": "phy-g10-forces-doc",
            "topicId": "topic-g10-forces",
            "description": "Explore the fundamental laws governing the motion of objects.",
            "unitySceneId": "NewtonsLawsScene",
            "unityBuildUrl": f"gs://{STORAGE_BUCKET}/practicals/grade10_newtons_laws/build",
            "maxScore": 10,
            "durationSeconds": 600,
            "demoAllowed": True,
            "demoMaxAttempts": 1,
            "practicalMaxAttempts": 1,
            "isActive": True,
            "order": 1,
        },
    },
    {
        "id": "grade10_friction",
        "data": {
            "title": "Friction & Surfaces",
            "grade": 10,
            "lessonId": "phy-g10-forces-doc",
            "topicId": "topic-g10-forces",
            "description": "Analyze how different surfaces affect the movement of blocks.",
            "unitySceneId": "FrictionSurfacesScene",
            "unityBuildUrl": f"gs://{STORAGE_BUCKET}/practicals/grade10_friction/build",
            "maxScore": 10,
            "durationSeconds": 480,
            "demoAllowed": True,
            "demoMaxAttempts": 1,
            "practicalMaxAttempts": 1,
            "isActive": True,
            "order": 2,
        },
    },
    {
        "id": "grade10_motion_trolley",
        "data": {
            "title": "Motion of a Trolley",
            "grade": 10,
            "lessonId": "phy-g10-motion-doc",
            "topicId": "topic-g10-motion",
            "description": "Measure distance, time, and velocity of a trolley under constant force.",
            "unitySceneId": "MotionTrolleyScene",
            "unityBuildUrl": f"gs://{STORAGE_BUCKET}/practicals/grade10_motion_trolley/build",
            "maxScore": 10,
            "durationSeconds": 600,
            "demoAllowed": True,
            "demoMaxAttempts": 1,
            "practicalMaxAttempts": 1,
            "isActive": True,
            "order": 1,
        },
    },
    {
        "id": "grade11_wave_ripple",
        "data": {
            "title": "Ripple Tank Waves",
            "grade": 11,
            "lessonId": "phy-g11-waves-doc",
            "topicId": "topic-g11-waves",
            "description": "Measure wavelength and frequency in a virtual ripple tank.",
            "unitySceneId": "RippleTankScene",
            "unityBuildUrl": f"gs://{STORAGE_BUCKET}/practicals/grade11_wave_ripple/build",
            "maxScore": 10,
            "durationSeconds": 720,
            "demoAllowed": True,
            "demoMaxAttempts": 1,
            "practicalMaxAttempts": 1,
            "isActive": True,
            "order": 1,
        },
    },
    {
        "id": "grade11_electronics",
        "data": {
            "title": "Electronics",
            "grade": 11,
            "lessonId": "phy-g11-electronics-doc",
            "topicId": "topic-g11-electronics",
            "description": "Study diode characteristics, forward and reverse bias, and circuit applications in a virtual electronics lab.",
            "unitySceneId": "ElectronicsDiodeExperiment",
            "unityBuildUrl": "",
            "maxScore": 100,
            "durationSeconds": 600,
            "demoAllowed": True,
            "demoMaxAttempts": 10,
            "practicalMaxAttempts": 3,
            "isActive": True,
            "order": 19,
        },
    },
]


def seed_practicals():
    print("Starting Practicals Seeding script...")

    missing_topics = []
    for practical in PRACTICALS:
        topic_id = practical["data"]["topicId"]
        if not db.collection("topics").document(topic_id).get().exists:
            missing_topics.append(topic_id)
    if missing_topics:
        print("Missing topics: " + ", ".join(sorted(set(missing_topics))))
        print("Please run seed_topics.py first.")
        sys.exit(1)

    practicals_ref = db.collection("practicals")
    for practical in PRACTICALS:
        payload = {
            **practical["data"],
            "createdAt": firestore.SERVER_TIMESTAMP,
            "updatedAt": firestore.SERVER_TIMESTAMP,
        }
        practicals_ref.document(practical["id"]).set(payload)
        print(
            f"  [Firestore] Saved practicals/{practical['id']} "
            f"(Grade {payload['grade']}, demo={payload['demoMaxAttempts']}, "
            f"official={payload['practicalMaxAttempts']})"
        )

    print("\nPracticals seeding completed successfully!")


if __name__ == "__main__":
    try:
        seed_practicals()
    except FileNotFoundError as e:
        print(e)
        sys.exit(1)
    except Exception as e:
        print(f"\nError during seeding: {e}")
        sys.exit(1)
