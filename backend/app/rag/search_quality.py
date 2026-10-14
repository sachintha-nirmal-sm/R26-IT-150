"""
search_quality.py — online learning for the "struggling search terms" /
content-gap detector. See the "Online Learning: Struggling-Search /
Content-Gap Detector" project plan.

Every search a student runs, and every result they click afterward, is one
atomic Firestore increment onto search_quality_index/{term} — a per-term
Beta-Bernoulli click-through-rate estimator (the same counting idea behind
multi-armed bandits). The increment itself IS the training step: there is
no batch job, no retraining run, no external dataset — the counters are
the model's parameters, updated the instant each event happens.

Firestore remains the real operational store (atomic, safe under many
concurrent students) — the two CSVs below are a human-readable mirror of
it, rewritten on every event purely so the dataset and the model's current
parameters can be pointed to as literal files, not just described as
"rows in a database."
"""

from __future__ import annotations

import csv
from datetime import datetime, timezone
from pathlib import Path

from google.cloud import firestore

from app.core.firebase import db

INDEX_COLLECTION = "search_quality_index"

MIN_SAMPLES = 3
STRUGGLING_CTR_THRESHOLD = 0.3

DATA_DIR = Path(__file__).resolve().parents[2] / "data"
EVENTS_LOG_CSV = DATA_DIR / "search_events_log.csv"
MODEL_SNAPSHOT_CSV = DATA_DIR / "search_quality_model.csv"


def _normalize(term: str) -> str:
    return term.strip().lower()


def _append_event_log(
    term: str,
    grade: str | None,
    event_type: str,
    result_count: int | None,
) -> None:
    """Appends one row to the raw dataset CSV — one row per real search or
    click event, in the order it happened. This is the literal training
    data: nothing here is derived or aggregated."""
    DATA_DIR.mkdir(parents=True, exist_ok=True)
    is_new = not EVENTS_LOG_CSV.exists()
    with open(EVENTS_LOG_CSV, "a", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)
        if is_new:
            writer.writerow(["timestamp", "term", "grade", "event_type", "result_count"])
        writer.writerow([
            datetime.now(timezone.utc).isoformat(),
            term,
            grade or "",
            event_type,
            result_count if result_count is not None else "",
        ])


def _write_model_snapshot() -> None:
    """Rewrites the full model-state CSV from Firestore's current counters
    — the model's trained parameters, as they stand the instant this is
    called. Every term is included (not just flagged ones) so a healthy
    term is visible alongside a struggling one."""
    DATA_DIR.mkdir(parents=True, exist_ok=True)
    rows = []
    for doc in db.collection(INDEX_COLLECTION).stream():
        data = doc.to_dict() or {}
        search_count = data.get("searchCount", 0)
        click_count = data.get("clickCount", 0)
        zero_result_count = data.get("zeroResultSearchCount", 0)
        ctr = (click_count / search_count) if search_count else 0.0
        zero_result_rate = (zero_result_count / search_count) if search_count else 0.0
        flagged = search_count >= MIN_SAMPLES and ctr <= STRUGGLING_CTR_THRESHOLD
        rows.append({
            "term": data.get("term", doc.id),
            "sample_query": data.get("sampleQuery", doc.id),
            "search_count": search_count,
            "click_count": click_count,
            "zero_result_search_count": zero_result_count,
            "ctr": round(ctr, 4),
            "zero_result_rate": round(zero_result_rate, 4),
            "grades": ",".join(sorted(data.get("grades", []))),
            "flagged_as_struggling": flagged,
        })
    rows.sort(key=lambda r: (-r["search_count"], r["ctr"]))

    with open(MODEL_SNAPSHOT_CSV, "w", newline="", encoding="utf-8") as f:
        fieldnames = [
            "term", "sample_query", "search_count", "click_count",
            "zero_result_search_count", "ctr", "zero_result_rate",
            "grades", "flagged_as_struggling",
        ]
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)


def record_search_event(term: str, grade: str | None, result_count: int) -> None:
    """Training step for a settled search (called once per debounced query,
    not per keystroke) — increments the trial count, and the zero-result
    count when nothing came back."""
    normalized = _normalize(term)
    if not normalized:
        return

    ref = db.collection(INDEX_COLLECTION).document(normalized)
    update = {
        "term": normalized,
        "sampleQuery": term.strip(),
        "searchCount": firestore.Increment(1),
        "lastSearchedAt": firestore.SERVER_TIMESTAMP,
    }
    if grade:
        update["grades"] = firestore.ArrayUnion([grade])
    if result_count == 0:
        update["zeroResultSearchCount"] = firestore.Increment(1)
    ref.set(update, merge=True)

    _append_event_log(normalized, grade, "search", result_count)
    _write_model_snapshot()


def record_search_click(term: str, grade: str | None) -> None:
    """Training step for a click — increments the success count."""
    normalized = _normalize(term)
    if not normalized:
        return

    ref = db.collection(INDEX_COLLECTION).document(normalized)
    update = {
        "term": normalized,
        "clickCount": firestore.Increment(1),
    }
    if grade:
        update["grades"] = firestore.ArrayUnion([grade])
    ref.set(update, merge=True)

    _append_event_log(normalized, grade, "click", None)
    _write_model_snapshot()


def list_struggling_terms(
    min_samples: int = MIN_SAMPLES,
    max_ctr: float = STRUGGLING_CTR_THRESHOLD,
) -> list[dict]:
    """Inference step: derives click-through rate and zero-result rate from
    the stored counters, and flags terms with enough traffic and a low
    enough success rate to likely be a content gap."""
    rows = []
    for doc in db.collection(INDEX_COLLECTION).stream():
        data = doc.to_dict() or {}
        search_count = data.get("searchCount", 0)
        if search_count < min_samples:
            continue

        click_count = data.get("clickCount", 0)
        zero_result_count = data.get("zeroResultSearchCount", 0)
        ctr = click_count / search_count
        if ctr > max_ctr:
            continue

        rows.append({
            "term": data.get("term", doc.id),
            "sampleQuery": data.get("sampleQuery", doc.id),
            "searchCount": search_count,
            "clickCount": click_count,
            "zeroResultSearchCount": zero_result_count,
            "ctr": round(ctr, 4),
            "zeroResultRate": round(zero_result_count / search_count, 4),
            "grades": sorted(data.get("grades", [])),
        })

    rows.sort(key=lambda r: (-r["searchCount"], r["ctr"]))
    return rows
