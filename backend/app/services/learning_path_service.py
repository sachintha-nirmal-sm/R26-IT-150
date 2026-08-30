"""Remediation path + stub YouTube recommendations from weakTopics."""

from datetime import datetime, timedelta, timezone

from google.cloud import firestore

from app.core.firebase import db
from app.core.utils import WEAKNESS_THRESHOLD

# Placeholder videos used until YouTube Data API v3 is configured.
STUB_VIDEOS = {
    "phy-g10-motion": {
        "videoId": "K2R56zU-Y3k",
        "title": "Physics Kinematics Equations Made Easy",
        "channelName": "The Organic Chemistry Tutor",
    },
    "default": {
        "videoId": "ZM8HBE2T5sI",
        "title": "Introduction to Physics",
        "channelName": "Khan Academy",
    },
}

REMEDIATION_BY_TYPE = {
    "Theory": ["deepLearningMaterials", "lessonVideo"],
    "Formula": ["formulaPractice", "targetedPracticeProblems"],
    "Calculation": ["calculationPractice", "virtualExperiments"],
}


def maybe_write_youtube_recommendation(uid: str, lesson_tag: str) -> None:
    coll = db.collection("users").document(uid).collection("youtubeRecommendations")
    existing = list(
        coll.where(filter=firestore.FieldFilter("lessonTag", "==", lesson_tag))
        .limit(5)
        .stream()
    )
    cutoff = datetime.now(timezone.utc) - timedelta(days=30)
    for doc in existing:
        generated = (doc.to_dict() or {}).get("generatedAt")
        if generated is None:
            return
        if hasattr(generated, "tzinfo") and generated >= cutoff:
            return
        # Firestore timestamp
        if hasattr(generated, "timestamp"):
            if datetime.fromtimestamp(generated.timestamp(), tz=timezone.utc) >= cutoff:
                return

    meta = STUB_VIDEOS.get(lesson_tag, STUB_VIDEOS["default"])
    vid = meta["videoId"]
    coll.document().set({
        "lessonTag": lesson_tag,
        "videoId": vid,
        "title": meta["title"],
        "channelName": meta["channelName"],
        "thumbnailUrl": f"https://img.youtube.com/vi/{vid}/0.jpg",
        "videoUrl": f"https://www.youtube.com/watch?v={vid}",
        "relevanceScore": 0.8,
        "generatedAt": firestore.SERVER_TIMESTAMP,
    })


def generate_learning_path(uid: str) -> dict:
    weak_docs = list(db.collection("users").document(uid).collection("weakTopics").stream())
    topics = []
    resources: list[str] = []
    for doc in weak_docs:
        data = doc.to_dict() or {}
        score = float(data.get("weaknessScore") or 0)
        if score < WEAKNESS_THRESHOLD:
            continue
        by_type = data.get("byQuestionType") or {}
        weak_types = [
            t for t, b in by_type.items()
            if float((b or {}).get("weaknessScore") or 0) >= WEAKNESS_THRESHOLD
        ]
        for t in weak_types:
            resources.extend(REMEDIATION_BY_TYPE.get(t, []))
        topics.append({
            "lessonTag": data.get("lessonTag"),
            "lessonId": data.get("lessonId"),
            "weaknessScore": score,
            "weakQuestionTypes": weak_types,
        })

    topics.sort(key=lambda x: x["weaknessScore"], reverse=True)
    recs = []
    for doc in db.collection("users").document(uid).collection("youtubeRecommendations").stream():
        recs.append({"id": doc.id, **(doc.to_dict() or {})})

    return {
        "studentId": uid,
        "weakTopics": topics,
        "recommendedMaterials": list(dict.fromkeys(resources)) or ["deepLearningMaterials"],
        "youtubeRecommendations": recs,
        "isActive": True,
    }
