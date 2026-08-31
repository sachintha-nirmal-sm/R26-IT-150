import '../../core/api/api_client.dart';

class NewsSample {
  const NewsSample({
    required this.id,
    required this.title,
    required this.text,
    required this.expected,
  });

  final String id;
  final String title;
  final String text;
  final String expected;

  factory NewsSample.fromJson(Map<String, dynamic> json) {
    return NewsSample(
      id: json['id'] as String? ?? '',
      title: json['title'] as String? ?? '',
      text: json['text'] as String? ?? '',
      expected: json['expected'] as String? ?? '',
    );
  }
}

class NewsScenarioResult {
  const NewsScenarioResult({
    required this.accepted,
    required this.isPhysics,
    required this.confidence,
    this.message,
    this.scenario,
    this.question,
    this.referenceAnswer,
    this.topic,
    this.gradeNote,
    this.usedLlm = false,
    this.elapsedMs,
  });

  final bool accepted;
  final bool isPhysics;
  final double confidence;
  final String? message;
  final String? scenario;
  final String? question;
  final String? referenceAnswer;
  final String? topic;
  final String? gradeNote;
  final bool usedLlm;
  final int? elapsedMs;

  factory NewsScenarioResult.fromJson(Map<String, dynamic> json) {
    final relevance = json['relevance'] is Map
        ? Map<String, dynamic>.from(json['relevance'] as Map)
        : <String, dynamic>{};
    final concept = json['concept'] is Map
        ? Map<String, dynamic>.from(json['concept'] as Map)
        : <String, dynamic>{};
    return NewsScenarioResult(
      accepted: json['accepted'] == true,
      isPhysics: relevance['isPhysics'] == true,
      confidence: (relevance['confidence'] as num?)?.toDouble() ?? 0,
      message: json['message'] as String?,
      scenario: json['scenario'] as String?,
      question: json['question'] as String?,
      referenceAnswer: json['referenceAnswer'] as String?,
      topic: concept['topic'] as String?,
      gradeNote: concept['gradeNote'] as String?,
      usedLlm: json['usedLlm'] == true,
      elapsedMs: json['elapsedMs'] as int?,
    );
  }
}

class NewsEvalResult {
  const NewsEvalResult({
    required this.displayLabel,
    required this.label,
    required this.confidence,
    required this.feedback,
    required this.relevance,
    required this.completeness,
    required this.creativity,
    this.elapsedMs,
  });

  final String displayLabel;
  final String label;
  final double confidence;
  final String feedback;
  final int relevance;
  final int completeness;
  final int creativity;
  final int? elapsedMs;

  factory NewsEvalResult.fromJson(Map<String, dynamic> json) {
    final correctness = json['correctness'] is Map
        ? Map<String, dynamic>.from(json['correctness'] as Map)
        : <String, dynamic>{};
    final rubric = json['rubric'] is Map
        ? Map<String, dynamic>.from(json['rubric'] as Map)
        : <String, dynamic>{};
    return NewsEvalResult(
      displayLabel: correctness['displayLabel'] as String? ?? 'Incorrect',
      label: correctness['label'] as String? ?? 'incorrect',
      confidence: (correctness['confidence'] as num?)?.toDouble() ?? 0,
      feedback: json['feedback'] as String? ?? '',
      relevance: (rubric['relevance'] as num?)?.toInt() ?? 0,
      completeness: (rubric['completeness'] as num?)?.toInt() ?? 0,
      creativity: (rubric['creativity'] as num?)?.toInt() ?? 0,
      elapsedMs: json['elapsedMs'] as int?,
    );
  }
}

class NewsScenarioService {
  NewsScenarioService({ApiClient? api}) : _api = api ?? ApiClient();

  final ApiClient _api;

  Future<List<NewsSample>> samples() async {
    final decoded = await _api.get('/news/samples', requireAuth: false);
    if (decoded is! Map || decoded['samples'] is! List) {
      return const [];
    }
    return (decoded['samples'] as List)
        .whereType<Map>()
        .map((item) => NewsSample.fromJson(Map<String, dynamic>.from(item)))
        .toList();
  }

  Future<NewsScenarioResult> generate({
    required String text,
    String? title,
    int? grade,
  }) async {
    final decoded = await _api.postPublic(
      '/news/scenario',
      body: {
        'text': text,
        if (title != null && title.isNotEmpty) 'title': title,
        if (grade != null) 'grade': grade,
      },
      timeout: const Duration(seconds: 90),
    );
    if (decoded is! Map) {
      throw ApiException('Unexpected news response');
    }
    return NewsScenarioResult.fromJson(Map<String, dynamic>.from(decoded));
  }

  Future<NewsEvalResult> evaluate({
    required String question,
    required String referenceAnswer,
    required String studentAnswer,
    String? scenario,
  }) async {
    final decoded = await _api.postPublic(
      '/news/evaluate',
      body: {
        'question': question,
        'referenceAnswer': referenceAnswer,
        'studentAnswer': studentAnswer,
        if (scenario != null) 'scenario': scenario,
      },
      timeout: const Duration(seconds: 40),
    );
    if (decoded is! Map) {
      throw ApiException('Unexpected evaluation response');
    }
    return NewsEvalResult.fromJson(Map<String, dynamic>.from(decoded));
  }
}
