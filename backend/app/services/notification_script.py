"""
notification_script.py — LLM-written personalized notification text via
Groq (ported from the spike's script.py, Groq-only for v1 — matches the
provider already used in app/api/generate_questions.py::_call_groq).
"""

from __future__ import annotations

import json
import os

SYSTEM_PROMPT = """You write short motivational push-notification messages for \
school students (age 13-17) learning physics. Warm, simple, specific to the \
student's data. Never shame, compare, pressure, or mention grades negatively. \
Max 40 characters for title, max 90 characters for body. Return ONLY JSON: \
{"title": "", "body": ""}"""

MOOD_PROMPTS = {
    "struggle": (
        "This student is struggling with the topic '{topic}'. Share one "
        "interesting fact about this physics topic, and invite them to "
        "dive deeper into it."
    ),
    "comeback": (
        "This student hasn't practiced in {idle_days} days. Gently "
        "encourage them to come back and continue learning."
    ),
    "streak": (
        "This student is on a {streak_days}-day learning streak! "
        "Motivate them to keep the streak going today."
    ),
    "welcome": (
        "Nothing specific stands out yet for this student. Send a short, "
        "general motivational nudge encouraging them to keep learning "
        "physics today."
    ),
}


def _build_user_prompt(profile: dict, mood: str) -> str:
    template = MOOD_PROMPTS[mood]
    weak_topic = (profile.get("weakTopic") or {}).get("lessonTag", "physics")
    return (
        f"Student name: {profile.get('fullName', 'there')}\n"
        + template.format(
            topic=weak_topic,
            idle_days=profile.get("daysSinceLastActivity", 0),
            streak_days=profile.get("streakDays", 0),
        )
    )


def _extract_json(text: str) -> dict:
    start = text.find("{")
    end = text.rfind("}")
    if start == -1 or end == -1:
        raise ValueError("No JSON object found in LLM response")
    return json.loads(text[start : end + 1])


async def generate_script(profile: dict, mood: str) -> dict:
    """Returns {"title": str, "body": str}. Raises on any failure — caller
    must catch and fall back to notification_safety.fallback_script."""
    from groq import AsyncGroq

    api_key = os.getenv("GROQ_API_KEY", "")
    if not api_key:
        raise ValueError("GROQ_API_KEY is not set")

    client = AsyncGroq(api_key=api_key)
    resp = await client.chat.completions.create(
        model="openai/gpt-oss-20b",
        messages=[
            {"role": "system", "content": SYSTEM_PROMPT},
            {"role": "user", "content": _build_user_prompt(profile, mood)},
        ],
        temperature=0.7,
        max_tokens=300,
        # openai/gpt-oss-20b is a reasoning model — without this it spends
        # its whole token budget on internal chain-of-thought (exposed
        # separately as resp.choices[0].message.reasoning) and never writes
        # the actual answer into `content` (finish_reason="length", content
        # empty). Low effort is enough for a two-field JSON object.
        reasoning_effort="low",
    )
    content = resp.choices[0].message.content
    result = _extract_json(content)
    title = str(result.get("title", "")).strip()[:40]
    body = str(result.get("body", "")).strip()[:90]
    if not title or not body:
        raise ValueError("LLM returned empty title/body")
    return {"title": title, "body": body}
