import '../../../core/api/api_client.dart';

/// Records whether a student has opened/completed a lesson — backend has
/// no other way to know this (quiz performance was already tracked, lesson
/// viewing was not). study_plan_service.py uses this to avoid
/// re-recommending lessons a student has already finished.
class LessonEngagementRepository {
  LessonEngagementRepository({ApiClient? api}) : _api = api ?? ApiClient();

  final ApiClient _api;

  Future<void> recordOpened(String lessonId) => _record(lessonId, 'opened');

  Future<void> recordCompleted(String lessonId) =>
      _record(lessonId, 'completed');

  Future<void> _record(String lessonId, String event) async {
    await _api.post(
      '/student/lessons/$lessonId/engagement',
      body: {'event': event},
    );
  }
}
