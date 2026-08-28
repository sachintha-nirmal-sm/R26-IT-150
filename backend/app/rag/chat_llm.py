"""Ollama chat completion for the RAG tutor. Returns None if Ollama is down."""

from __future__ import annotations

import httpx

from app.core.config import OLLAMA_BASE_URL, OLLAMA_MODEL


def chat_complete(messages: list[dict], model: str | None = None) -> str | None:
    payload = {
        "model": model or OLLAMA_MODEL,
        "messages": messages,
        "stream": False,
        "options": {"temperature": 0.3},
    }
    try:
        with httpx.Client(timeout=90.0) as client:
            response = client.post(f"{OLLAMA_BASE_URL.rstrip('/')}/api/chat", json=payload)
            response.raise_for_status()
            data = response.json() or {}
            message = data.get("message") or {}
            content = (message.get("content") or "").strip()
            return content or None
    except Exception:
        return None
