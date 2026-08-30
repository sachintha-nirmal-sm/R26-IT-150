"""
verify_seed_data.py — Read back and verify seed data across Tasks 3 to 16.

This script uses the Firebase Admin SDK to inspect all Firestore collections
and Auth records created by the seed scripts (Tasks 3–12 quiz/RAG module,
Tasks 13–16 practicals module) to confirm data structure integrity before
backend development begins.

Usage:
    python verify_seed_data.py
"""

import sys
import os

# Allow imports from parent directory (firebase/scripts)
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from firebase_init import db, auth

def run_verification():
    print("=" * 70)
    print("  FIREBASE SEED DATA VERIFICATION (TASKS 3 - 16)")
    print("=" * 70)

    passed_tests = 0
    failed_tests = 0

    def report_status(task_num, task_name, success, details=""):
        nonlocal passed_tests, failed_tests
        if success:
            passed_tests += 1
            print(f"  [PASS] Task {task_num:2d} | {task_name:<40} | {details}")
        else:
            failed_tests += 1
            print(f"  [FAIL] Task {task_num:2d} | {task_name:<40} | {details}")

    # -------------------------------------------------------------------------
    # Task 3: Users (users/{uid}) & Auth Custom Claims
    # -------------------------------------------------------------------------
    try:
        admins = db.collection("users").where("role", "==", "admin").limit(1).get()
        students = db.collection("users").where("role", "==", "student").limit(1).get()
        if admins and students:
            admin_doc = admins[0].to_dict()
            student_doc = students[0].to_dict()
            admin_uid = admins[0].id
            student_uid = students[0].id

            # Verify custom claims in Auth
            admin_auth = auth.get_user(admin_uid)
            student_auth = auth.get_user(student_uid)
            admin_claim_ok = admin_auth.custom_claims.get("role") == "admin"
            student_claim_ok = student_auth.custom_claims.get("role") == "student"

            if admin_claim_ok and student_claim_ok:
                report_status(3, "Users & Auth Custom Claims", True, f"Admin ({admin_doc['email']}), Student ({student_doc['email']})")
            else:
                report_status(3, "Users & Auth Custom Claims", False, "Custom claims mismatch in Auth")
        else:
            report_status(3, "Users & Auth Custom Claims", False, "Missing Admin or Student user document")
    except Exception as e:
        report_status(3, "Users & Auth Custom Claims", False, str(e))

    # -------------------------------------------------------------------------
    # Task 4: Sample Lessons (lessons/{lessonId})
    # -------------------------------------------------------------------------
    lesson_id = "phy-g10-motion-doc"
    try:
        lesson_snap = db.collection("lessons").document(lesson_id).get()
        if lesson_snap.exists:
            data = lesson_snap.to_dict()
            report_status(4, "Sample Lessons", True, f"Lesson ID: {lesson_id} (Grade {data.get('grade')})")
        else:
            report_status(4, "Sample Lessons", False, f"Document lessons/{lesson_id} not found")
    except Exception as e:
        report_status(4, "Sample Lessons", False, str(e))

    # -------------------------------------------------------------------------
    # Task 5: Materials (lessons/{lessonId}/materials/{materialId})
    # -------------------------------------------------------------------------
    try:
        mats = db.collection("lessons").document(lesson_id).collection("materials").limit(1).get()
        if mats:
            data = mats[0].to_dict()
            report_status(5, "Lesson Materials", True, f"Material ID: {mats[0].id} (Type: {data.get('fileType')})")
        else:
            report_status(5, "Lesson Materials", False, f"No materials found under lessons/{lesson_id}")
    except Exception as e:
        report_status(5, "Lesson Materials", False, str(e))

    # -------------------------------------------------------------------------
    # Task 6: Quizzes (lessons/{lessonId}/quizzes/{quizId})
    # -------------------------------------------------------------------------
    quiz_id = "phy-g10-motion-quiz"
    try:
        quiz_snap = db.collection("lessons").document(lesson_id).collection("quizzes").document(quiz_id).get()
        if quiz_snap.exists:
            data = quiz_snap.to_dict()
            report_status(6, "Quizzes", True, f"Quiz ID: {quiz_id} (Status: {data.get('status')})")
        else:
            report_status(6, "Quizzes", False, f"Document quizzes/{quiz_id} not found")
    except Exception as e:
        report_status(6, "Quizzes", False, str(e))

    # -------------------------------------------------------------------------
    # Task 7: Question Bank & Questions
    # -------------------------------------------------------------------------
    try:
        versions = (
            db.collection("lessons")
            .document(lesson_id)
            .collection("quizzes")
            .document(quiz_id)
            .collection("questionBankVersions")
            .limit(1)
            .get()
        )
        if versions:
            ver_id = versions[0].id
            questions = (
                db.collection("lessons")
                .document(lesson_id)
                .collection("quizzes")
                .document(quiz_id)
                .collection("questionBankVersions")
                .document(ver_id)
                .collection("questions")
                .get()
            )
            if questions:
                report_status(7, "Question Bank & Questions", True, f"Bank version {ver_id} with {len(questions)} questions")
            else:
                report_status(7, "Question Bank & Questions", False, f"No questions in version {ver_id}")
        else:
            report_status(7, "Question Bank & Questions", False, "No questionBankVersions found")
    except Exception as e:
        report_status(7, "Question Bank & Questions", False, str(e))

    # -------------------------------------------------------------------------
    # Task 8: Generation Jobs (generationJobs/{jobId})
    # -------------------------------------------------------------------------
    job_id = "job-seed-v1-001"
    try:
        job_snap = db.collection("generationJobs").document(job_id).get()
        if job_snap.exists:
            data = job_snap.to_dict()
            report_status(8, "Generation Jobs", True, f"Job ID: {job_id} (Status: {data.get('status')}, Model: {data.get('llmModelUsed')})")
        else:
            report_status(8, "Generation Jobs", False, f"Document generationJobs/{job_id} not found")
    except Exception as e:
        report_status(8, "Generation Jobs", False, str(e))

    # -------------------------------------------------------------------------
    # Task 9: Final Quizzes & Questions (finalQuizzes/{finalQuizId})
    # -------------------------------------------------------------------------
    final_quiz_id = "grade10-round1"
    try:
        fq_snap = db.collection("finalQuizzes").document(final_quiz_id).get()
        if fq_snap.exists:
            fq_qs = db.collection("finalQuizzes").document(final_quiz_id).collection("questions").get()
            report_status(9, "Final Quizzes & Questions", True, f"Final Quiz: {final_quiz_id} with {len(fq_qs)} nested questions")
        else:
            report_status(9, "Final Quizzes & Questions", False, f"Document finalQuizzes/{final_quiz_id} not found")
    except Exception as e:
        report_status(9, "Final Quizzes & Questions", False, str(e))

    # -------------------------------------------------------------------------
    # Task 10: Student Progress Subcollections
    # -------------------------------------------------------------------------
    try:
        students = db.collection("users").where("role", "==", "student").limit(1).get()
        if students:
            st_uid = students[0].id
            st_ref = db.collection("users").document(st_uid)

            qp = st_ref.collection("quizProgress").document(quiz_id).get().exists
            qa = st_ref.collection("quizAttempts").document("attempt-1").get().exists
            wq = len(st_ref.collection("wrongQuestions").get()) > 0
            fb = st_ref.collection("feedback").document("feedback-attempt-1").get().exists
            wt = len(st_ref.collection("weakTopics").get()) > 0
            yr = st_ref.collection("youtubeRecommendations").document("rec-motion-1").get().exists
            ps = st_ref.collection("performanceSummary").document("summary").get().exists

            all_subcolls_ok = qp and qa and wq and fb and wt and yr and ps
            if all_subcolls_ok:
                report_status(10, "Student Progress & Attempts", True, f"All 7 subcollections verified under users/{st_uid}")
            else:
                missing = []
                if not qp: missing.append("quizProgress")
                if not qa: missing.append("quizAttempts")
                if not wq: missing.append("wrongQuestions")
                if not fb: missing.append("feedback")
                if not wt: missing.append("wrongQuestions")
                if not fb: missing.append("feedback")
                if not wt: missing.append("weakTopics")
                if not yr: missing.append("youtubeRecommendations")
                if not ps: missing.append("performanceSummary")
                report_status(10, "Student Progress & Attempts", False, f"Missing subcollections: {', '.join(missing)}")
        else:
            report_status(10, "Student Progress & Attempts", False, "No student user found to inspect progress")
    except Exception as e:
        report_status(10, "Student Progress & Attempts", False, str(e))

    # -------------------------------------------------------------------------
    # Task 11: Final Quiz Attempts (users/{uid}/finalQuizAttempts/{attemptId})
    # -------------------------------------------------------------------------
    try:
        students = db.collection("users").where("role", "==", "student").limit(1).get()
        if students:
            st_uid = students[0].id
            fq_att = db.collection("users").document(st_uid).collection("finalQuizAttempts").document("final-attempt-1").get()
            if fq_att.exists:
                data = fq_att.to_dict()
                report_status(11, "Final Quiz Attempts", True, f"Attempt: final-attempt-1 (Score: {data.get('score')}%)")
            else:
                report_status(11, "Final Quiz Attempts", False, f"Document users/{st_uid}/finalQuizAttempts/final-attempt-1 not found")
        else:
            report_status(11, "Final Quiz Attempts", False, "No student user found")
    except Exception as e:
        report_status(11, "Final Quiz Attempts", False, str(e))

    # -------------------------------------------------------------------------
    # Task 12: Analytics Rollups (analytics/global/...)
    # -------------------------------------------------------------------------
    try:
        ls = db.collection("analytics").document("global").collection("lessonStats").document(lesson_id).get().exists
        qs = db.collection("analytics").document("global").collection("quizStats").document(quiz_id).get().exists
        qns = db.collection("analytics").document("global").collection("questionStats").document("q1").get().exists

        if ls and qs and qns:
            report_status(12, "Analytics Rollups", True, f"Verified lessonStats, quizStats, & questionStats under analytics/global")
        else:
            missing = []
            if not ls: missing.append("lessonStats")
            if not qs: missing.append("quizStats")
            if not qns: missing.append("questionStats")
            report_status(12, "Analytics Rollups", False, f"Missing stats subcollections: {', '.join(missing)}")
    except Exception as e:
        report_status(12, "Analytics Rollups", False, str(e))

    # -------------------------------------------------------------------------
    # Task 13: Topics (topics/{topicId})
    # -------------------------------------------------------------------------
    try:
        topic_ids = [
            "topic-g9-force",
            "topic-g9-density",
            "topic-g10-forces",
            "topic-g11-waves",
        ]
        missing = [tid for tid in topic_ids if not db.collection("topics").document(tid).get().exists]
        if not missing:
            report_status(13, "Practical Topics", True, f"Verified {len(topic_ids)} topics across grades 9–11")
        else:
            report_status(13, "Practical Topics", False, f"Missing topics: {', '.join(missing)}")
    except Exception as e:
        report_status(13, "Practical Topics", False, str(e))

    # -------------------------------------------------------------------------
    # Task 14: Practicals (practicals/{practicalId})
    # -------------------------------------------------------------------------
    try:
        practical_id = "grade10_newtons_laws"
        snap = db.collection("practicals").document(practical_id).get()
        if snap.exists:
            data = snap.to_dict()
            limits_ok = (
                data.get("demoMaxAttempts") == 1
                and data.get("practicalMaxAttempts") == 1
                and data.get("demoAllowed") is True
                and data.get("isActive") is True
            )
            if limits_ok:
                report_status(14, "Practicals Config", True, f"{practical_id} (Grade {data.get('grade')}, maxScore={data.get('maxScore')})")
            else:
                report_status(14, "Practicals Config", False, f"{practical_id} has incorrect attempt/timer config")
        else:
            report_status(14, "Practicals Config", False, f"Document practicals/{practical_id} not found")
    except Exception as e:
        report_status(14, "Practicals Config", False, str(e))

    # -------------------------------------------------------------------------
    # Task 15: Practical Results (practicalResults/{resultId})
    # -------------------------------------------------------------------------
    try:
        demo = db.collection("practicalResults").document("result-newtons-demo-1").get()
        official = db.collection("practicalResults").document("result-newtons-practical-1").get()
        if demo.exists and official.exists:
            demo_data = demo.to_dict()
            official_data = official.to_dict()
            shape_ok = (
                demo_data.get("attemptType") == "demo"
                and official_data.get("attemptType") == "practical"
                and official_data.get("score") == 8
                and official_data.get("percentage") == 80
                and isinstance(official_data.get("measurements"), dict)
                and isinstance(official_data.get("calculations"), dict)
                and isinstance(official_data.get("evaluation"), dict)
            )
            if shape_ok:
                report_status(15, "Practical Results", True, "Demo + official Newton's Laws results stored")
            else:
                report_status(15, "Practical Results", False, "Result documents exist but fields do not match schema")
        else:
            report_status(15, "Practical Results", False, "Missing result-newtons-demo-1 or result-newtons-practical-1")
    except Exception as e:
        report_status(15, "Practical Results", False, str(e))

    # -------------------------------------------------------------------------
    # Task 16: Student Practical Progress (studentPracticals + studentProgress)
    # -------------------------------------------------------------------------
    try:
        students = db.collection("users").where("role", "==", "student").limit(1).get()
        if students:
            st_uid = students[0].id
            sp_id = f"{st_uid}_grade10_newtons_laws"
            sp_snap = db.collection("studentPracticals").document(sp_id).get()
            progress_snap = db.collection("studentProgress").document(st_uid).get()
            if sp_snap.exists and progress_snap.exists:
                sp = sp_snap.to_dict()
                progress = progress_snap.to_dict()
                ok = (
                    sp.get("completed") is True
                    and sp.get("currentState") == "SUBMITTED"
                    and sp.get("demoCompleted") is True
                    and sp.get("bestScore") == 8
                    and progress.get("completedPracticals") == 1
                    and progress.get("averagePercentage") == 80
                    and "10" in (progress.get("gradeProgress") or {})
                )
                if ok:
                    report_status(16, "Student Practical Progress", True, f"studentPracticals + studentProgress verified for {st_uid}")
                else:
                    report_status(16, "Student Practical Progress", False, "Progress docs exist but rollup fields are incorrect")
            else:
                missing = []
                if not sp_snap.exists:
                    missing.append(f"studentPracticals/{sp_id}")
                if not progress_snap.exists:
                    missing.append(f"studentProgress/{st_uid}")
                report_status(16, "Student Practical Progress", False, f"Missing: {', '.join(missing)}")
        else:
            report_status(16, "Student Practical Progress", False, "No student user found")
    except Exception as e:
        report_status(16, "Student Practical Progress", False, str(e))

    print("=" * 70)
    print(f"  VERIFICATION SUMMARY: {passed_tests} PASSED | {failed_tests} FAILED")
    print("=" * 70)

    if failed_tests > 0:
        sys.exit(1)
    else:
        sys.exit(0)

if __name__ == "__main__":
    run_verification()
