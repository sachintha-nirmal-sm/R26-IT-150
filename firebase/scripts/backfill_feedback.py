"""
backfill_feedback.py — One-time backfill script for Firestore feedback collection group.

Schema update context:
  Existing feedback documents under `users/{uid}/feedback/{feedbackId}` created before
  the schema update may lack `attemptType` and either `finalQuizId` or `quizId`.

This script:
  1. Queries all feedback subcollections using a collection group query ("feedback").
  2. For documents with a non-null `quizId`:
     - Sets `attemptType` to "lessonQuiz"
     - Adds `finalQuizId: None` (null in Firestore)
  3. For documents with a non-null `finalQuizId`:
     - Sets `attemptType` to "finalQuiz"
     - Adds `quizId: None` (null in Firestore)
  4. Preserves all existing fields without overwriting.
  5. Logs total documents scanned and total updated.

Usage:
    python backfill_feedback.py
"""

import os
import sys

# Ensure current script directory is in sys.path for relative imports
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from config import db


def backfill_feedback_documents():
    print("=" * 70)
    print("  FIRESTORE BACKFILL: feedback subcollection schema update")
    print("=" * 70)

    print("Fetching all 'feedback' documents using collection group query...")
    feedback_docs = list(db.collection_group("feedback").stream())
    
    total_scanned = len(feedback_docs)
    print(f"Found {total_scanned} total feedback document(s).\n")

    if total_scanned == 0:
        print("No feedback documents found. Migration complete.")
        return 0

    batch = db.batch()
    batch_ops = 0
    updated_count = 0
    skipped_count = 0

    for doc in feedback_docs:
        doc_data = doc.to_dict() or {}
        updates = {}

        quiz_id = doc_data.get("quizId")
        final_quiz_id = doc_data.get("finalQuizId")

        if quiz_id is not None:
            # Document belongs to a lesson quiz
            if doc_data.get("attemptType") != "lessonQuiz":
                updates["attemptType"] = "lessonQuiz"
            if "finalQuizId" not in doc_data:
                updates["finalQuizId"] = None

        elif final_quiz_id is not None:
            # Document belongs to a final quiz
            if doc_data.get("attemptType") != "finalQuiz":
                updates["attemptType"] = "finalQuiz"
            if "quizId" not in doc_data:
                updates["quizId"] = None
        else:
            print(f"  [WARN] Doc {doc.id} (path: {doc.reference.path}) has neither non-null quizId nor finalQuizId. Skipping.")
            skipped_count += 1
            continue

        if updates:
            print(f"  [UPDATE] Doc: {doc.reference.path}")
            for field, val in updates.items():
                print(f"           + {field}: {val}")
            
            batch.update(doc.reference, updates)
            batch_ops += 1
            updated_count += 1

            if batch_ops >= 500:
                batch.commit()
                print(f"  --> Committed batch of {batch_ops} updates.")
                batch = db.batch()
                batch_ops = 0
        else:
            skipped_count += 1

    if batch_ops > 0:
        batch.commit()
        print(f"  --> Committed final batch of {batch_ops} updates.")

    print("\n" + "=" * 70)
    print(f"  BACKFILL SUMMARY")
    print(f"    - Total documents scanned : {total_scanned}")
    print(f"    - Total documents updated : {updated_count}")
    print(f"    - Total documents skipped : {skipped_count}")
    print("=" * 70)

    return updated_count


if __name__ == "__main__":
    backfill_feedback_documents()
