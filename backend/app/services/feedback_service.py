"""Rule-based feedback after a quiz attempt.

LLM generation can replace `build_feedback` later; the stored document shape
matches Firestore schema 3.11 so the client contract stays stable.
"""

from app.core.utils import QUESTION_TYPES


def build_feedback(submit_result: dict) -> dict:
    answers = submit_result.get("answers") or []
    score = submit_result.get("score") or 0
    total = submit_result.get("totalMarks") or 0
    percent = submit_result.get("scorePercent") or 0

    by_type: dict[str, dict[str, int]] = {
        t: {"correct": 0, "total": 0} for t in QUESTION_TYPES
    }
    wrong_tags: list[str] = []
    for ans in answers:
        qtype = ans.get("questionType") or "Theory"
        if qtype not in by_type:
            by_type[qtype] = {"correct": 0, "total": 0}
        by_type[qtype]["total"] += 1
        if ans.get("isCorrect"):
            by_type[qtype]["correct"] += 1
        else:
            tag = ans.get("lessonTag")
            if tag:
                wrong_tags.append(tag)

    strengths = []
    weaknesses = []
    for qtype, stats in by_type.items():
        if stats["total"] == 0:
            continue
        acc = stats["correct"] / stats["total"]
        if acc >= 0.7:
            strengths.append(f"Strong {qtype} performance ({stats['correct']}/{stats['total']})")
        elif acc <= 0.5:
            weaknesses.append(f"Weak {qtype} questions ({stats['correct']}/{stats['total']} correct)")

    if percent >= 75:
        text = (
            f"Well done — you scored {score}/{total} ({percent}%). "
            "Keep practising the remaining gaps so they don't carry into the final quiz."
        )
    elif percent >= 50:
        text = (
            f"You scored {score}/{total} ({percent}%). You have a workable foundation, "
            "but the weak areas below need targeted revision before your next attempt."
        )
    else:
        text = (
            f"You scored {score}/{total} ({percent}%). Focus on the recommended topics "
            "and question types before retrying — the next attempt will draw different questions."
        )

    recommended = list(dict.fromkeys(wrong_tags))
    primary = None
    worst = 1.1
    for qtype, stats in by_type.items():
        if stats["total"] == 0:
            continue
        acc = stats["correct"] / stats["total"]
        if acc < worst:
            worst = acc
            primary = qtype

    return {
        "feedbackText": text,
        "strengths": strengths or ["You completed the attempt — that's the first step."],
        "weaknesses": weaknesses,
        "recommendedTopics": recommended,
        "primaryWeakness": primary,
        "llmModelUsed": "rule-based",
    }
