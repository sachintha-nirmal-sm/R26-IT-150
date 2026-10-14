import 'dart:convert';

import 'package:firebase_auth/firebase_auth.dart';
import 'package:http/http.dart' as http;

import '../../../core/backend_url.dart';

/// Fire-and-forget signal that feeds the online "weak topics by quiz
/// performance" model (backend/app/rag/topic_performance.py) — every call
/// here is one atomic training step on the backend. Errors are swallowed:
/// a logging failure must never interrupt a student's quiz submission.
class QuizFeedbackService {
  Future<void> logQuizPerformance({
    required String lessonId,
    String? lessonTitle,
    String? grade,
    required num score,
    required int correct,
    required int total,
  }) async {
    try {
      final token = await FirebaseAuth.instance.currentUser?.getIdToken();
      if (token == null) return;
      await http.post(
        Uri.parse('$backendUrl/student/quiz-attempts/performance-feedback'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode({
          'lessonId': lessonId,
          'lessonTitle': lessonTitle,
          'grade': grade,
          'score': score,
          'correct': correct,
          'total': total,
        }),
      ).timeout(const Duration(seconds: 5));
    } catch (_) {
      // Best-effort only — never let this affect quiz submission.
    }
  }
}
