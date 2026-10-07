import '../../../core/api/api_client.dart';

class StudyPlanRepository {
  StudyPlanRepository({ApiClient? api}) : _api = api ?? ApiClient();

  final ApiClient _api;

  /// Generates this week's plan on first call if it doesn't exist yet —
  /// see study_plan_service.get_current_plan on the backend.
  Future<Map<String, dynamic>> getCurrentPlan() async {
    final result = await _api.get('/student/study-plan/current');
    return Map<String, dynamic>.from(result as Map);
  }
}
