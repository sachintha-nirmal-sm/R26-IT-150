"""
train_models.py
===============
Loads simulated student response CSVs, engineers per-question features,
trains 4 classifiers, compares Accuracy + weighted F1, selects the best,
and saves it as models/difficulty_model.pkl.

Target label: irt_true_difficulty (derived from IRT b-parameter)
  NOT the observed correct_rate threshold -- this makes the task genuinely
  non-trivial because borderline questions produce ambiguous response patterns.

Models compared:
  1. Logistic Regression
  2. Random Forest
  3. SVM (RBF kernel)
  4. Gradient Boosting

Usage:
  pip install -r requirements.txt
  python train_models.py
"""

import json
import pickle
import warnings
from pathlib import Path

import numpy as np
import pandas as pd
from sklearn.ensemble import GradientBoostingClassifier, RandomForestClassifier
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import (accuracy_score, classification_report,
                             confusion_matrix, f1_score)
from sklearn.model_selection import StratifiedKFold, cross_val_score, train_test_split
from sklearn.preprocessing import LabelEncoder, StandardScaler
from sklearn.svm import SVC

warnings.filterwarnings("ignore")

DATA_DIR  = Path(__file__).parent / "data"
MODEL_DIR = Path(__file__).parent / "models"
MODEL_DIR.mkdir(exist_ok=True)

# ── 1. Load datasets ──────────────────────────────────────────────────────────
csv_files = sorted(DATA_DIR.glob("student_responses_seed*.csv"))
if not csv_files:
    raise FileNotFoundError("No CSVs found. Run python run_simulation.py first.")

print(f"Loading {len(csv_files)} seed dataset(s):")
frames = []
for f in csv_files:
    df_seed = pd.read_csv(f)
    frames.append(df_seed)
    print(f"  {f.name}: {len(df_seed):,} rows")

df = pd.concat(frames, ignore_index=True)
print(f"\nCombined: {len(df):,} responses | "
      f"{df['question_id'].nunique()} questions | "
      f"{df['student_id'].nunique()} unique student IDs")

# ── 2. Per-question feature engineering ───────────────────────────────────────
# One row per (question_id, run_seed) so each seed is an independent sample.
# 210 questions x 5 seeds = 1,050 training samples.
print("\nEngineering features per (question, seed)...")

agg = (
    df.groupby(["question_id", "run_seed"])
    .agg(
        grade             = ("grade",               "first"),
        ai_difficulty     = ("ai_difficulty",        "first"),
        irt_true_difficulty = ("irt_true_difficulty", "first"),
        irt_b_param       = ("irt_b_param",          "first"),
        correct_rate      = ("correct",              "mean"),
        total_attempts    = ("correct",              "count"),
    )
    .reset_index()
)

# Correct rate by student ability band
for band in ["Low", "Medium", "High"]:
    rate = (
        df[df["ability_band"] == band]
        .groupby(["question_id", "run_seed"])["correct"]
        .mean()
        .rename(f"correct_rate_{band.lower()}")
    )
    agg = agg.join(rate, on=["question_id", "run_seed"])

band_cols = ["correct_rate_low", "correct_rate_medium", "correct_rate_high"]
agg[band_cols] = agg[band_cols].fillna(agg["correct_rate"])

# Discrimination index: separability between top and bottom performers
agg["discrimination_index"] = agg["correct_rate_high"] - agg["correct_rate_low"]

# Biserial-like spread: std of correct rate across ability bands
agg["band_std"] = agg[band_cols].std(axis=1).fillna(0)

# Encode AI difficulty label (ordinal)
DIFF_ORDER = {"Easy": 0, "Medium": 1, "Hard": 2}
agg["ai_difficulty_enc"] = agg["ai_difficulty"].map(DIFF_ORDER).fillna(1)

print(f"Feature matrix shape: {agg.shape[0]} samples")

# ── 3. Target: IRT true difficulty (NOT correct_rate threshold) ───────────────
# This is the key design choice -- the b-parameter defines ground truth,
# not the observed correct rate. Borderline questions create genuine errors.
y = agg["irt_true_difficulty"].values

le = LabelEncoder()
y_enc = le.fit_transform(y)

print(f"\nClass distribution (IRT b-param based labels):")
for cls, cnt in zip(*np.unique(y, return_counts=True)):
    pct = cnt / len(y) * 100
    print(f"  {cls:<8}: {cnt:>4} samples ({pct:.1f}%)")

ai_match = (agg["ai_difficulty"] == agg["irt_true_difficulty"]).mean()
print(f"\nAI label agreement with IRT true label: {ai_match:.1%} "
      f"(~25% mismatch is intentional -- keeps ML task non-trivial)")

# ── 4. Feature matrix and split ───────────────────────────────────────────────
FEATURES = [
    "correct_rate",
    "correct_rate_low",
    "correct_rate_medium",
    "correct_rate_high",
    "discrimination_index",
    "band_std",
    "total_attempts",
    "ai_difficulty_enc",
]
X = agg[FEATURES].values

X_train, X_test, y_train, y_test = train_test_split(
    X, y_enc, test_size=0.25, random_state=42, stratify=y_enc
)

scaler = StandardScaler()
X_train_sc = scaler.fit_transform(X_train)
X_test_sc  = scaler.transform(X_test)

print(f"\nTrain: {len(X_train)} samples | Test: {len(X_test)} samples")

# ── 5. Define 4 candidate models ─────────────────────────────────────────────
candidates = {
    "Logistic Regression": {
        "model": LogisticRegression(C=0.5, max_iter=1000, random_state=42),
        "scale": True,
    },
    "Random Forest": {
        "model": RandomForestClassifier(
            n_estimators=200, max_depth=6, min_samples_leaf=4,
            random_state=42
        ),
        "scale": False,
    },
    "SVM (RBF)": {
        "model": SVC(kernel="rbf", C=0.8, probability=True, random_state=42),
        "scale": True,
    },
    "Gradient Boosting": {
        "model": GradientBoostingClassifier(
            n_estimators=150, learning_rate=0.08, max_depth=3,
            subsample=0.8, random_state=42
        ),
        "scale": False,
    },
}

# ── 6. Train, evaluate, compare ───────────────────────────────────────────────
cv = StratifiedKFold(n_splits=5, shuffle=True, random_state=42)

print(f"\n{'-'*68}")
print(f"{'Model':<25} {'Accuracy':>10} {'F1 (weighted)':>15} {'CV F1 (5-fold)':>14}")
print(f"{'-'*68}")

results  = {}
best_name, best_f1, best_payload = None, -1.0, None

for name, cfg in candidates.items():
    model = cfg["model"]
    Xtr = X_train_sc if cfg["scale"] else X_train
    Xte = X_test_sc  if cfg["scale"] else X_test
    Xcv = X_train_sc if cfg["scale"] else X_train

    model.fit(Xtr, y_train)
    preds  = model.predict(Xte)
    acc    = accuracy_score(y_test, preds)
    f1     = f1_score(y_test, preds, average="weighted")
    cv_f1  = cross_val_score(model, Xcv, y_train, cv=cv,
                             scoring="f1_weighted").mean()
    cr     = classification_report(y_test, preds,
                                   target_names=le.classes_, output_dict=True)
    cm     = confusion_matrix(y_test, preds).tolist()

    print(f"{name:<25} {acc:>10.4f} {f1:>15.4f} {cv_f1:>14.4f}")
    results[name] = {
        "accuracy":         round(float(acc),   4),
        "f1_weighted":      round(float(f1),    4),
        "cv_f1_weighted":   round(float(cv_f1), 4),
        "per_class":        cr,
        "confusion_matrix": cm,
    }

    if f1 > best_f1:
        best_f1, best_name = f1, name
        best_payload = {
            "model_name":    name,
            "model":         model,
            "scaler":        scaler if cfg["scale"] else None,
            "label_encoder": le,
            "features":      FEATURES,
            "diff_order":    DIFF_ORDER,
            "train_samples": int(len(X_train)),
            "test_samples":  int(len(X_test)),
        }

print(f"{'-'*68}")
print(f"\nBest model: {best_name}  (F1 = {best_f1:.4f})")

# ── 7. Feature importance ─────────────────────────────────────────────────────
bm = best_payload["model"]
if hasattr(bm, "feature_importances_"):
    print("\nFeature importances:")
    pairs = sorted(zip(FEATURES, bm.feature_importances_), key=lambda x: -x[1])
    for feat, imp in pairs:
        bar = "#" * int(imp * 40)
        print(f"  {feat:<30} {imp:.4f}  {bar}")

# Confusion matrix for best model
best_preds = bm.predict(X_test_sc if best_payload["scaler"] else X_test)
print(f"\nConfusion matrix ({best_name}):")
print(f"  Classes: {list(le.classes_)}")
cm = confusion_matrix(y_test, best_preds)
for row_label, row in zip(le.classes_, cm):
    print(f"  {row_label:<8}: {list(row)}")

# ── 8. Save artefacts ─────────────────────────────────────────────────────────
with open(MODEL_DIR / "difficulty_model.pkl", "wb") as f:
    pickle.dump(best_payload, f)

report = {
    "best_model":    best_name,
    "best_f1":       round(float(best_f1), 4),
    "questions":     int(agg["question_id"].nunique()),
    "total_samples": int(len(agg)),
    "train_samples": int(len(X_train)),
    "test_samples":  int(len(X_test)),
    "features":      FEATURES,
    "classes":       list(le.classes_),
    "ai_label_agreement": round(float(ai_match), 4),
    "results":       results,
}
with open(MODEL_DIR / "model_report.json", "w") as f:
    json.dump(report, f, indent=2)

print(f"\nSaved -> {MODEL_DIR / 'difficulty_model.pkl'}")
print(f"Saved -> {MODEL_DIR / 'model_report.json'}")
print("Done.")
