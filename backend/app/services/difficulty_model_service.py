"""Runtime inference for the trained question-difficulty classifier."""

from __future__ import annotations

import pickle
from functools import lru_cache
from pathlib import Path
from statistics import stdev

MODEL_PATH = (
    Path(__file__).resolve().parents[3] / "ml" / "models" / "difficulty_model.pkl"
)
MODEL_VERSION = "difficulty-model-v1"
DIFFICULTIES = {"Easy", "Medium", "Hard"}


@lru_cache(maxsize=1)
def _load_model() -> dict:
    if not MODEL_PATH.exists():
        raise FileNotFoundError(f"Difficulty model not found: {MODEL_PATH}")
    with MODEL_PATH.open("rb") as model_file:
        payload = pickle.load(model_file)  # noqa: S301 - trusted local artefact
    required = {"model", "label_encoder", "features", "diff_order"}
    if not required.issubset(payload):
        raise ValueError("Difficulty model payload is incomplete.")
    return payload


def ability_band(overall_accuracy: float) -> str:
    """Map a student's cumulative accuracy to a model ability band."""
    if overall_accuracy < 0.40:
        return "low"
    if overall_accuracy > 0.70:
        return "high"
    return "medium"


def _rate(correct: int, attempts: int, fallback: float) -> float:
    return correct / attempts if attempts else fallback


def feature_vector(
    *,
    attempts: int,
    correct_count: int,
    ability_stats: dict,
    ai_difficulty: str,
    diff_order: dict,
) -> list[float]:
    """Build features in the exact order used by ml/train_models.py."""
    overall = correct_count / attempts if attempts else 0.0
    rates = []
    for band in ("low", "medium", "high"):
        band_data = ability_stats.get(band) or {}
        rates.append(
            _rate(
                int(band_data.get("correct") or 0),
                int(band_data.get("attempts") or 0),
                overall,
            )
        )
    low, medium, high = rates
    return [
        overall,
        low,
        medium,
        high,
        high - low,
        stdev(rates),
        float(attempts),
        float(diff_order.get(ai_difficulty, 1)),
    ]


def predict_difficulty(
    *,
    attempts: int,
    correct_count: int,
    ability_stats: dict,
    ai_difficulty: str,
) -> tuple[str, str]:
    """Return (predicted label, trained model name)."""
    payload = _load_model()
    normalized_ai = ai_difficulty.title()
    features = feature_vector(
        attempts=attempts,
        correct_count=correct_count,
        ability_stats=ability_stats,
        ai_difficulty=normalized_ai,
        diff_order=payload["diff_order"],
    )
    matrix = [features]
    scaler = payload.get("scaler")
    if scaler is not None:
        matrix = scaler.transform(matrix)
    encoded = payload["model"].predict(matrix)
    label = str(payload["label_encoder"].inverse_transform(encoded)[0]).title()
    if label not in DIFFICULTIES:
        raise ValueError(f"Difficulty model returned invalid label: {label}")
    return label, str(payload.get("model_name") or "trained-classifier")
