"""Ollama chat completion for the RAG tutor. Returns None if Ollama is down."""

from __future__ import annotations

import httpx

from app.core.config import OLLAMA_BASE_URL, OLLAMA_MODEL

_base = OLLAMA_BASE_URL.rstrip("/")


def _available_model(preferred: str) -> str | None:
    try:
        response = httpx.get(f"{_base}/api/tags", timeout=5.0)
        response.raise_for_status()
        names = [m.get("name") or "" for m in (response.json() or {}).get("models") or []]
        names = [n for n in names if n]
        if not names:
            return None
        for name in names:
            if preferred in name or name.startswith(preferred):
                return name
        for name in names:
            if "llama" in name.lower():
                return name
        return names[0]
    except Exception as exc:
        print(f"[Ollama] cannot list models: {exc}")
        return None


def chat_complete(messages: list[dict], model: str | None = None) -> str | None:
    chosen = _available_model(model or OLLAMA_MODEL)
    if not chosen:
        print("[Ollama] no running model found. Is Ollama started?")
        return None

    payload = {
        "model": chosen,
        "messages": messages,
        "stream": False,
        "options": {"temperature": 0.2, "num_predict": 180},
    }
    try:
        with httpx.Client(timeout=90.0) as client:
            response = client.post(f"{_base}/api/chat", json=payload)
            response.raise_for_status()
            data = response.json() or {}
            content = ((data.get("message") or {}).get("content") or "").strip()
            if content:
                return content
    except Exception as exc:
        print(f"[Ollama] /api/chat failed ({chosen}): {exc}")

    # Fallback: generate API with a flattened prompt
    prompt = "\n\n".join(f"{m.get('role', 'user').upper()}: {m.get('content', '')}" for m in messages)
    try:
        with httpx.Client(timeout=90.0) as client:
            response = client.post(
                f"{_base}/api/generate",
                json={"model": chosen, "prompt": prompt, "stream": False},
            )
            response.raise_for_status()
            content = ((response.json() or {}).get("response") or "").strip()
            return content or None
    except Exception as exc:
        print(f"[Ollama] /api/generate failed ({chosen}): {exc}")
        return None
