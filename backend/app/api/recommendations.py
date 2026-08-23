"""
recommendations.py — YouTube video recommendations after a quiz.

POST /student/quiz-feedback
  - Takes lesson info + score
  - Fetches the lesson grade from Firestore
  - Builds a YouTube search query (e.g. "physics grade 9 Newton's Laws explained")
  - Calls YouTube Data API v3 (free tier: 10,000 units/day, 100 searches/day)
  - Caches results in Firestore so the same lesson doesn't burn quota repeatedly
  - Returns video cards: title, channel, thumbnail, URL
"""
import hashlib
import os

import httpx
from fastapi import APIRouter, Depends, HTTPException, status
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from firebase_admin import firestore
from firebase_admin.auth import verify_id_token
from pydantic import BaseModel

from app.core.firebase import db

router = APIRouter(prefix="/student", tags=["Student - Recommendations"])

_bearer = HTTPBearer(auto_error=False)

async def _any_firebase_user(
    creds: HTTPAuthorizationCredentials | None = Depends(_bearer),
) -> str:
    """Accept any valid Firebase token — no role claim required."""
    if not creds or not creds.credentials.strip():
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "Token missing")
    try:
        decoded = verify_id_token(creds.credentials.strip(), check_revoked=True)
        return decoded.get("uid") or decoded.get("user_id") or ""
    except Exception as e:
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, str(e))


class QuizFeedbackRequest(BaseModel):
    lesson_id: str
    lesson_title: str
    score: int          # percentage 0-100
    topic_title: str | None = None  # overrides lesson_title for YouTube search (e.g. sub-lesson title)


@router.post("/quiz-feedback")
async def quiz_feedback(
    body: QuizFeedbackRequest,
    uid: str = Depends(_any_firebase_user),
):
    """Return YouTube recommendations whenever the student got any questions wrong."""
    if body.score >= 100:
        return {"recommendations": []}

    # Look up lesson grade from Firestore
    lesson_doc = db.collection("lessons").document(body.lesson_id).get()
    grade_str = ""
    if lesson_doc.exists:
        grade_raw = lesson_doc.to_dict().get("grade", "")
        grade_digits = "".join(filter(str.isdigit, str(grade_raw)))
        if grade_digits:
            grade_str = f"grade {grade_digits}"

    # Build search query — use topic_title (sub-lesson) when provided, else lesson_title
    search_title = (body.topic_title or body.lesson_title).strip()
    query = f"physics {grade_str} {search_title} explained tutorial".strip()
    query = " ".join(query.split())  # collapse extra spaces

    # Firestore cache — avoids burning API quota for repeated requests
    cache_key = hashlib.md5(query.lower().encode()).hexdigest()
    cache_ref = db.collection("youtubeCache").document(cache_key)
    cache_doc = cache_ref.get()

    if cache_doc.exists:
        videos = cache_doc.to_dict().get("videos", [])
    else:
        videos = await _search_youtube(query)
        if videos:
            cache_ref.set({
                "query": query,
                "videos": videos,
                "cachedAt": firestore.SERVER_TIMESTAMP,
            })

    recommendations = []
    if videos:
        recommendations.append({
            "topic": search_title,
            "videos": videos,
        })

    return {"recommendations": recommendations}


async def _search_youtube(query: str) -> list:
    """Call YouTube Data API v3 search.list and return simplified video objects."""
    api_key = os.getenv("YOUTUBE_API_KEY", "")
    if not api_key:
        print("[YouTube] ERROR: YOUTUBE_API_KEY is not set in .env")
        return []

    print(f"[YouTube] Searching: {query!r}")
    try:
        async with httpx.AsyncClient(timeout=8.0) as client:
            resp = await client.get(
                "https://www.googleapis.com/youtube/v3/search",
                params={
                    "part": "snippet",
                    "q": query,
                    "type": "video",
                    "maxResults": 3,
                    "relevanceLanguage": "en",
                    "videoDuration": "medium",
                    "safeSearch": "strict",
                    "key": api_key,
                },
            )
            print(f"[YouTube] Response status: {resp.status_code}")
            if resp.status_code != 200:
                print(f"[YouTube] Error body: {resp.text}")
                return []

            videos = []
            for item in resp.json().get("items", []):
                vid_id  = item["id"]["videoId"]
                snippet = item["snippet"]
                thumb   = (
                    snippet.get("thumbnails", {})
                           .get("medium", {})
                           .get("url", "")
                )
                videos.append({
                    "video_id": vid_id,
                    "title":    snippet.get("title", ""),
                    "channel":  snippet.get("channelTitle", ""),
                    "thumbnail": thumb,
                    "url":      f"https://www.youtube.com/watch?v={vid_id}",
                })
            print(f"[YouTube] Found {len(videos)} videos")
            return videos
    except Exception as e:
        print(f"[YouTube] Exception: {e}")
        return []
