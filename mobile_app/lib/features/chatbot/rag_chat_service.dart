import '../../core/api/api_client.dart';

class RagChatResponse {
  const RagChatResponse({
    required this.sessionId,
    required this.answer,
    required this.usedLlm,
    this.grade,
  });

  final String sessionId;
  final String answer;
  final bool usedLlm;
  final int? grade;

  factory RagChatResponse.fromJson(Map<String, dynamic> json) {
    return RagChatResponse(
      sessionId: json['sessionId'] as String? ?? '',
      answer: json['answer'] as String? ?? '',
      usedLlm: json['usedLlm'] == true,
      grade: json['grade'] is int ? json['grade'] as int : int.tryParse('${json['grade']}'),
    );
  }
}

class RagChatService {
  RagChatService({ApiClient? api}) : _api = api ?? ApiClient();

  final ApiClient _api;

  Future<RagChatResponse> send({
    required String message,
    String? lessonId,
    String? sessionId,
  }) async {
    final decoded = await _api.post(
      '/chat/rag',
      body: {
        'message': message,
        if (lessonId != null) 'lesson_id': lessonId,
        if (sessionId != null) 'session_id': sessionId,
      },
      timeout: const Duration(seconds: 90),
    );
    if (decoded is! Map) {
      throw ApiException('Unexpected chat response');
    }
    return RagChatResponse.fromJson(Map<String, dynamic>.from(decoded));
  }
}
