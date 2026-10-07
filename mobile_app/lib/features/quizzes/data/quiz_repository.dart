import '../../../core/api/api_client.dart';

/// Wraps the real, secure quiz API (backend/app/services/quiz_service.py) —
/// question banks and answer keys are never read directly from Firestore by
/// the client (the security rules explicitly forbid it), so this is the
/// only correct way for a student to take a quiz.
class QuizRepository {
  QuizRepository({ApiClient? api}) : _api = api ?? ApiClient();

  final ApiClient _api;

  Future<List<Map<String, dynamic>>> listQuizzesForLesson(
      String lessonId) async {
    final result = await _api.get('/student/lessons/$lessonId/quizzes');
    return (result as List)
        .map((e) => Map<String, dynamic>.from(e as Map))
        .toList();
  }

  Future<Map<String, dynamic>> startQuiz(String lessonId, String quizId) async {
    final result = await _api.post(
      '/student/quizzes/$quizId/start',
      query: {'lessonId': lessonId},
    );
    return Map<String, dynamic>.from(result as Map);
  }

  Future<Map<String, dynamic>> submitQuiz({
    required String lessonId,
    required String quizId,
    required String attemptId,
    required List<Map<String, String?>> answers,
    required int timeTakenSeconds,
  }) async {
    final result = await _api.post(
      '/student/quizzes/$quizId/submit',
      query: {'lessonId': lessonId},
      body: {
        'attemptId': attemptId,
        'answers': answers,
        'timeTakenSeconds': timeTakenSeconds,
      },
    );
    return Map<String, dynamic>.from(result as Map);
  }
}
