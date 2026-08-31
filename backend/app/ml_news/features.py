"""Shared text features for Model 2 (answer correctness)."""

from __future__ import annotations

import re

import numpy as np
from sklearn.metrics.pairwise import cosine_similarity


_WORD = re.compile(r"[a-zA-Z]{3,}")
_NUM = re.compile(r"\d+(?:\.\d+)?")


def _tokens(text: str) -> set[str]:
    return set(_WORD.findall((text or "").lower()))


def _numbers(text: str) -> set[str]:
    return set(_NUM.findall(text or ""))


def jaccard(a: set[str], b: set[str]) -> float:
    if not a and not b:
        return 0.0
    return len(a & b) / max(len(a | b), 1)


def extract_features(question: str, reference: str, student: str, vectorizer) -> np.ndarray:
    q = question or ""
    r = reference or ""
    s = student or ""
    matrix = vectorizer.transform([s, r, q])
    sim_ref = float(cosine_similarity(matrix[0], matrix[1])[0, 0])
    sim_q = float(cosine_similarity(matrix[0], matrix[2])[0, 0])
    stu_tok, ref_tok, q_tok = _tokens(s), _tokens(r), _tokens(q)
    stu_len = max(len(s.split()), 1)
    ref_len = max(len(r.split()), 1)
    coverage = len(stu_tok & ref_tok) / max(len(ref_tok), 1)
    num_overlap = jaccard(_numbers(s), _numbers(r))
    return np.array(
        [
            sim_ref,
            sim_q,
            jaccard(stu_tok, ref_tok),
            jaccard(stu_tok, q_tok),
            coverage,
            min(stu_len / ref_len, 3.0),
            num_overlap,
            min(len(s) / 400.0, 1.5),
        ],
        dtype=np.float32,
    )


FEATURE_NAMES = [
    "sim_reference",
    "sim_question",
    "jaccard_reference",
    "jaccard_question",
    "ref_token_coverage",
    "length_ratio",
    "number_overlap",
    "char_norm",
]
