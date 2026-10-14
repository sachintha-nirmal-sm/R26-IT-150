"""
keyword_ai_select.py — uses Groq to pick the most search-worthy keywords
for a newly uploaded PDF, out of the raw, frequency-ranked candidates
keyword_extract.extract_keywords() already produced. A word that only
scores well on raw frequency isn't necessarily what a student would
actually search for; an LLM reading the material's own text can judge
relevance in a way word-counting can't.

Deliberately constrained to PICKING FROM the existing candidate list,
never inventing new words — keeps this from ever hallucinating a keyword
that isn't actually in the material's text, which matters since these
keywords directly drive "smart recommendation" search hits.

Pure enhancement: any failure (no API key, network error, bad response)
falls back to the existing candidates unchanged, so an upload can never
fail because of this.
"""

from __future__ import annotations

import json
import os

MAX_SELECTED = 20


async def select_best_keywords(
    candidates: list[str],
    lesson_title: str | None,
    limit: int = MAX_SELECTED,
) -> list[str]:
    """Returns up to `limit` of `candidates` — Groq's pick of the most
    search-worthy ones for this specific material. Falls back to the top
    `limit` candidates unchanged (their existing frequency order) on any
    failure, so this is never a point of failure for an upload."""
    fallback = candidates[:limit]
    if not candidates:
        return []

    try:
        from groq import AsyncGroq

        api_key = os.getenv("GROQ_API_KEY", "")
        if not api_key:
            raise ValueError("GROQ_API_KEY is not set")

        client = AsyncGroq(api_key=api_key)
        topic = f' about "{lesson_title}"' if lesson_title else ""
        prompt = (
            f"These are candidate search keywords extracted from a physics "
            f"lesson material{topic}:\n{', '.join(candidates)}\n\n"
            f"Pick up to {limit} that a student would genuinely search for to "
            "find this specific material again — physics concepts, named "
            "laws/quantities, and specific terms. Exclude generic/structural "
            "words even if they're in the list (e.g. figure, example, shown, "
            "page, chapter, note, these, which) — they are never useful "
            "search terms. It's fine to return fewer than "
            f"{limit} if fewer candidates are genuinely good; do not pad the "
            "list with weak choices just to reach the count. Only choose "
            "from the list above, never invent new words. Return ONLY a "
            "JSON array of strings, most useful first, nothing else."
        )
        resp = await client.chat.completions.create(
            model="openai/gpt-oss-20b",
            messages=[{"role": "user", "content": prompt}],
            temperature=0.2,
            max_tokens=400,
            # openai/gpt-oss-20b is a reasoning model — without this it can
            # burn its whole token budget on hidden chain-of-thought and
            # return empty content instead of the answer (the existing
            # _call_groq() in generate_questions.py doesn't set this either,
            # but its much larger max_tokens=2500 likely papers over the
            # same risk; this call's budget is deliberately small, so the
            # fix is applied explicitly here rather than assumed safe).
            reasoning_effort="low",
        )
        content = resp.choices[0].message.content or ""
        start = content.find("[")
        end = content.rfind("]")
        if start == -1 or end == -1:
            raise ValueError("No JSON array found in Groq response")
        selected = json.loads(content[start : end + 1])

        candidate_set = set(candidates)
        cleaned = [kw for kw in selected if isinstance(kw, str) and kw in candidate_set]
        return cleaned[:limit] if cleaned else fallback
    except Exception as exc:
        print(f"[keyword_ai_select] Falling back to raw candidates: {exc}")
        return fallback
