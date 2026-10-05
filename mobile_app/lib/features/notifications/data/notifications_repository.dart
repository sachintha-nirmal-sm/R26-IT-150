import '../../../core/api/api_client.dart';

class NotificationsRepository {
  NotificationsRepository({ApiClient? api}) : _api = api ?? ApiClient();

  final ApiClient _api;

  Future<void> registerToken(String token, String platform) async {
    await _api.post(
      '/api/notifications/register-token',
      body: {'token': token, 'platform': platform},
    );
  }
}
