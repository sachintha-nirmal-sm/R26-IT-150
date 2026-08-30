import 'dart:convert';

import 'package:http/http.dart' as http;

import '../../core/api_config.dart';

class RagChatResponse {
  const RagChatResponse({
    required this.sessionId,
    required this.answer,
    required this.usedLlm,
  });

  final String sessionId;
  final String answer;
  final bool usedLlm;

  factory RagChatResponse.fromJson(Map<String, dynamic> json) {
    return RagChatResponse(
      sessionId: json['sessionId'] as String? ?? '',
      answer: json['answer'] as String? ?? '',
      usedLlm: json['usedLlm'] == true,
    );
  }
}

class RagChatService {
  Future<RagChatResponse> send({
    required String message,
    required int grade,
    String? lessonId,
    String? sessionId,
  }) async {
    final uri = Uri.parse('${ApiConfig.baseUrl}/chat/rag');
    final response = await http
        .post(
          uri,
          headers: {'Content-Type': 'application/json'},
          body: jsonEncode({
            'message': message,
            'grade': grade,
            if (lessonId != null) 'lesson_id': lessonId,
            if (sessionId != null) 'session_id': sessionId,
          }),
        )
        .timeout(const Duration(seconds: 90));

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception(
        'Chat API ${response.statusCode}: ${response.body}',
      );
    }
    final decoded = jsonDecode(response.body);
    if (decoded is! Map<String, dynamic>) {
      throw Exception('Unexpected chat response');
    }
    return RagChatResponse.fromJson(decoded);
  }
}
