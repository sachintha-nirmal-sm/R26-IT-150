"""Train Model 1 (news) and Model 2 (answers). Run from backend/: python -m app.ml_news.train"""

from __future__ import annotations

import json
from collections import Counter
from pathlib import Path

import joblib
import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import (
    accuracy_score,
    classification_report,
    confusion_matrix,
    f1_score,
    precision_score,
    recall_score,
)
from sklearn.model_selection import train_test_split

from app.ml_news.datasets import export_datasets
from app.ml_news.features import FEATURE_NAMES, extract_features

ARTIFACT_DIR = Path(__file__).resolve().parent / "artifacts"
ARTIFACT_DIR.mkdir(parents=True, exist_ok=True)


def _save_confusion(labels, y_true, y_pred, title: str, path: Path) -> None:
    matrix = confusion_matrix(y_true, y_pred, labels=labels)
    fig, ax = plt.subplots(figsize=(5.5, 4.5))
    image = ax.imshow(matrix, cmap="Blues")
    fig.colorbar(image, ax=ax, fraction=0.046)
    ax.set_xticks(range(len(labels)), labels=labels, rotation=20)
    ax.set_yticks(range(len(labels)), labels=labels)
    ax.set_xlabel("Predicted")
    ax.set_ylabel("True")
    ax.set_title(title)
    for i in range(matrix.shape[0]):
        for j in range(matrix.shape[1]):
            ax.text(j, i, str(matrix[i, j]), ha="center", va="center")
    fig.tight_layout()
    fig.savefig(path, dpi=140)
    plt.close(fig)


def _metrics(y_true, y_pred, labels) -> dict:
    return {
        "accuracy": round(float(accuracy_score(y_true, y_pred)), 4),
        "precision_weighted": round(
            float(precision_score(y_true, y_pred, average="weighted", zero_division=0)), 4
        ),
        "recall_weighted": round(
            float(recall_score(y_true, y_pred, average="weighted", zero_division=0)), 4
        ),
        "f1_weighted": round(
            float(f1_score(y_true, y_pred, average="weighted", zero_division=0)), 4
        ),
        "report": classification_report(y_true, y_pred, labels=labels, zero_division=0),
        "confusion_matrix": confusion_matrix(y_true, y_pred, labels=labels).tolist(),
        "labels": list(labels),
    }


def train_model1(news_csv: Path) -> dict:
    df = pd.read_csv(news_csv)
    df["text"] = df["text"].astype(str).str.strip()
    df = df[df["text"].str.len() > 8].drop_duplicates(subset=["text"])
    labels = ["non_physics", "physics"]
    x_train, x_test, y_train, y_test = train_test_split(
        df["text"],
        df["label"],
        test_size=0.2,
        random_state=42,
        stratify=df["label"],
    )
    vectorizer = TfidfVectorizer(ngram_range=(1, 2), min_df=1, max_features=8000)
    x_train_vec = vectorizer.fit_transform(x_train)
    x_test_vec = vectorizer.transform(x_test)
    clf = LogisticRegression(max_iter=2000, class_weight="balanced", random_state=42)
    clf.fit(x_train_vec, y_train)
    pred = clf.predict(x_test_vec)
    joblib.dump(vectorizer, ARTIFACT_DIR / "model1_tfidf.joblib")
    joblib.dump(clf, ARTIFACT_DIR / "model1_clf.joblib")
    _save_confusion(labels, y_test, pred, "Model 1 — Physics vs Non-Physics", ARTIFACT_DIR / "model1_cm.png")
    examples = []
    for text, true, guessed in zip(x_test.head(8), y_test.head(8), pred[:8]):
        examples.append({"text": text, "true": true, "predicted": guessed})
    stats = _metrics(y_test, pred, labels)
    stats.update(
        {
            "dataset_size": int(len(df)),
            "class_distribution": dict(Counter(df["label"])),
            "train_size": int(len(x_train)),
            "test_size": int(len(x_test)),
            "split": "80/20 stratified",
            "model": "TF-IDF + Logistic Regression",
            "examples": examples,
        }
    )
    return stats


def train_model2(answers_csv: Path) -> dict:
    df = pd.read_csv(answers_csv)
    df["student"] = df["student"].astype(str).str.strip()
    df = df[df["student"].str.len() > 3]
    labels = ["incorrect", "partial", "correct"]
    train_df, test_df = train_test_split(
        df,
        test_size=0.2,
        random_state=42,
        stratify=df["label"],
    )
    corpus = list(train_df["student"]) + list(train_df["reference"]) + list(train_df["question"])
    vectorizer = TfidfVectorizer(ngram_range=(1, 2), min_df=1, max_features=6000)
    vectorizer.fit(corpus)
    x_train = np.vstack(
        [
            extract_features(row.question, row.reference, row.student, vectorizer)
            for row in train_df.itertuples()
        ]
    )
    x_test = np.vstack(
        [
            extract_features(row.question, row.reference, row.student, vectorizer)
            for row in test_df.itertuples()
        ]
    )
    clf = LogisticRegression(max_iter=2000, class_weight="balanced", random_state=42)
    clf.fit(x_train, train_df["label"])
    pred = clf.predict(x_test)
    joblib.dump(vectorizer, ARTIFACT_DIR / "model2_tfidf.joblib")
    joblib.dump(clf, ARTIFACT_DIR / "model2_clf.joblib")
    _save_confusion(labels, test_df["label"], pred, "Model 2 — Answer correctness", ARTIFACT_DIR / "model2_cm.png")
    examples = []
    for row, guessed in zip(test_df.head(8).itertuples(), pred[:8]):
        examples.append(
            {
                "question": row.question,
                "student": row.student[:180],
                "true": row.label,
                "predicted": guessed,
            }
        )
    stats = _metrics(test_df["label"], pred, labels)
    stats.update(
        {
            "dataset_size": int(len(df)),
            "class_distribution": dict(Counter(df["label"])),
            "train_size": int(len(train_df)),
            "test_size": int(len(test_df)),
            "split": "80/20 stratified",
            "model": "TF-IDF cosine + overlap features + Logistic Regression",
            "feature_names": FEATURE_NAMES,
            "examples": examples,
        }
    )
    return stats


def main() -> None:
    paths = export_datasets()
    model1 = train_model1(paths["news"])
    model2 = train_model2(paths["answers"])
    payload = {"model1": model1, "model2": model2}
    (ARTIFACT_DIR / "metrics.json").write_text(json.dumps(payload, indent=2), encoding="utf-8")
    print("Model 1", {k: model1[k] for k in ("dataset_size", "class_distribution", "accuracy", "f1_weighted")})
    print(model1["report"])
    print("Model 2", {k: model2[k] for k in ("dataset_size", "class_distribution", "accuracy", "f1_weighted")})
    print(model2["report"])
    print(f"Saved artifacts to {ARTIFACT_DIR}")


if __name__ == "__main__":
    main()
