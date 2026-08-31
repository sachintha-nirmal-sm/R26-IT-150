"""Load trained Model 1 and Model 2 once, then predict."""

from __future__ import annotations

from pathlib import Path

import joblib
import numpy as np

from app.ml_news.features import extract_features

ARTIFACT_DIR = Path(__file__).resolve().parent / "artifacts"

_model1_tfidf = None
_model1_clf = None
_model2_tfidf = None
_model2_clf = None


def models_ready() -> bool:
    needed = [
        ARTIFACT_DIR / "model1_tfidf.joblib",
        ARTIFACT_DIR / "model1_clf.joblib",
        ARTIFACT_DIR / "model2_tfidf.joblib",
        ARTIFACT_DIR / "model2_clf.joblib",
    ]
    return all(path.exists() for path in needed)


def load_models() -> None:
    global _model1_tfidf, _model1_clf, _model2_tfidf, _model2_clf
    if _model1_clf is not None:
        return
    if not models_ready():
        raise FileNotFoundError(
            "News models are not trained. From backend/ run: python -m app.ml_news.train"
        )
    _model1_tfidf = joblib.load(ARTIFACT_DIR / "model1_tfidf.joblib")
    _model1_clf = joblib.load(ARTIFACT_DIR / "model1_clf.joblib")
    _model2_tfidf = joblib.load(ARTIFACT_DIR / "model2_tfidf.joblib")
    _model2_clf = joblib.load(ARTIFACT_DIR / "model2_clf.joblib")


def predict_news(text: str) -> dict:
    load_models()
    cleaned = (text or "").strip()
    if not cleaned:
        return {"label": "non_physics", "confidence": 0.0, "isPhysics": False}
    probs = _model1_clf.predict_proba(_model1_tfidf.transform([cleaned]))[0]
    labels = list(_model1_clf.classes_)
    best = int(np.argmax(probs))
    label = str(labels[best])
    return {
        "label": label,
        "confidence": round(float(probs[best]), 4),
        "isPhysics": label == "physics",
        "probabilities": {str(name): round(float(p), 4) for name, p in zip(labels, probs)},
    }


def predict_answer(question: str, reference: str, student: str) -> dict:
    load_models()
    features = extract_features(question, reference, student, _model2_tfidf).reshape(1, -1)
    probs = _model2_clf.predict_proba(features)[0]
    labels = list(_model2_clf.classes_)
    best = int(np.argmax(probs))
    label = str(labels[best])
    display = {
        "correct": "Correct",
        "partial": "Partially Correct",
        "incorrect": "Incorrect",
    }.get(label, label)
    return {
        "label": label,
        "displayLabel": display,
        "confidence": round(float(probs[best]), 4),
        "probabilities": {str(name): round(float(p), 4) for name, p in zip(labels, probs)},
    }
