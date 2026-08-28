class Practical {
  const Practical({
    required this.id,
    required this.title,
    required this.description,
    required this.grade,
    required this.lessonId,
    required this.topicId,
    required this.unitySceneId,
    required this.unityBuildUrl,
    required this.maxScore,
    required this.durationSeconds,
    required this.demoAllowed,
    required this.demoMaxAttempts,
    required this.practicalMaxAttempts,
    required this.order,
    required this.isActive,
    this.currentState = 'AVAILABLE',
    this.demoAttemptsUsed = 0,
    this.practicalAttemptsUsed = 0,
    this.demoCompleted = false,
    this.completed = false,
    this.bestScore = 0,
    this.latestScore = 0,
    this.percentage = 0,
  });

  final String id;
  final String title;
  final String description;
  final int grade;
  final String lessonId;
  final String topicId;
  final String unitySceneId;
  final String unityBuildUrl;
  final int maxScore;
  final int durationSeconds;
  final bool demoAllowed;
  final int demoMaxAttempts;
  final int practicalMaxAttempts;
  final int order;
  final bool isActive;
  final String currentState;
  final int demoAttemptsUsed;
  final int practicalAttemptsUsed;
  final bool demoCompleted;
  final bool completed;
  final int bestScore;
  final int latestScore;
  final double percentage;

  String get durationLabel {
    final minutes = durationSeconds ~/ 60;
    final seconds = durationSeconds % 60;
    return '${minutes.toString().padLeft(2, '0')}:${seconds.toString().padLeft(2, '0')}';
  }

  bool get canStartDemo {
    if (!demoAllowed) return false;
    if (currentState == 'DEMO_IN_PROGRESS') return true;
    if (demoAttemptsUsed >= demoMaxAttempts) return false;
    return const {
      'AVAILABLE',
      'DEMO_COMPLETED',
      'PRACTICAL_AVAILABLE',
    }.contains(currentState);
  }

  bool get canRetryOfficial =>
      completed ||
      currentState == 'SUBMITTED' ||
      currentState == 'TIME_EXPIRED';

  bool get canStartPractical {
    if (currentState == 'PRACTICAL_IN_PROGRESS') return true;
    if (canRetryOfficial) return true;
    if (practicalAttemptsUsed >= practicalMaxAttempts) return false;
    return const {
      'AVAILABLE',
      'DEMO_COMPLETED',
      'PRACTICAL_AVAILABLE',
      'DEMO_IN_PROGRESS',
    }.contains(currentState);
  }

  bool get canViewResult =>
      completed ||
      currentState == 'SUBMITTED' ||
      currentState == 'TIME_EXPIRED';

  factory Practical.fromJson(Map<String, dynamic> json) {
    return Practical(
      id: json['id'] as String? ?? '',
      title: json['title'] as String? ?? 'Untitled practical',
      description: json['description'] as String? ?? '',
      grade: _asInt(json['grade']),
      lessonId: json['lessonId'] as String? ?? '',
      topicId: json['topicId'] as String? ?? '',
      unitySceneId: json['unitySceneId'] as String? ?? '',
      unityBuildUrl: json['unityBuildUrl'] as String? ?? '',
      maxScore: _asInt(json['maxScore'], fallback: 100),
      durationSeconds: _asInt(json['durationSeconds']),
      demoAllowed: json['demoAllowed'] as bool? ?? true,
      demoMaxAttempts: _asInt(json['demoMaxAttempts'], fallback: 1),
      practicalMaxAttempts: _asInt(json['practicalMaxAttempts'], fallback: 1),
      order: _asInt(json['order']),
      isActive: json['isActive'] as bool? ?? false,
      currentState: json['currentState'] as String? ?? 'AVAILABLE',
      demoAttemptsUsed: _asInt(json['demoAttemptsUsed']),
      practicalAttemptsUsed: _asInt(json['practicalAttemptsUsed']),
      demoCompleted: json['demoCompleted'] as bool? ?? false,
      completed: json['completed'] as bool? ?? false,
      bestScore: _asInt(json['bestScore']),
      latestScore: _asInt(json['latestScore']),
      percentage: _asDouble(json['percentage']),
    );
  }

  static int _asInt(dynamic value, {int fallback = 0}) {
    if (value is int) return value;
    if (value is num) return value.toInt();
    return fallback;
  }

  static double _asDouble(dynamic value, {double fallback = 0}) {
    if (value is double) return value;
    if (value is num) return value.toDouble();
    return fallback;
  }
}

class PracticalSession {
  const PracticalSession({
    required this.practicalId,
    required this.resultId,
    required this.mode,
    required this.attemptNumber,
    required this.currentState,
    required this.unitySceneId,
    required this.unityBuildUrl,
    this.durationSeconds,
    this.startedAt,
  });

  final String practicalId;
  final String resultId;
  final String mode;
  final int attemptNumber;
  final String currentState;
  final int? durationSeconds;
  final String unitySceneId;
  final String unityBuildUrl;
  final String? startedAt;

  bool get isDemo => mode == 'demo';

  bool get isLocal => resultId.startsWith('local-');

  factory PracticalSession.local({
    required Practical practical,
    required String mode,
  }) {
    final isDemo = mode == 'demo';
    return PracticalSession(
      practicalId: practical.id,
      resultId: 'local-${DateTime.now().millisecondsSinceEpoch}',
      mode: isDemo ? 'demo' : 'practical',
      attemptNumber: 1,
      currentState: isDemo ? 'DEMO_IN_PROGRESS' : 'PRACTICAL_IN_PROGRESS',
      unitySceneId: practical.unitySceneId,
      unityBuildUrl: practical.unityBuildUrl,
      durationSeconds: practical.durationSeconds,
      startedAt: DateTime.now().toIso8601String(),
    );
  }

  factory PracticalSession.fromJson(Map<String, dynamic> json) {
    return PracticalSession(
      practicalId: json['practicalId'] as String? ?? '',
      resultId: json['resultId'] as String? ?? '',
      mode: json['mode'] as String? ?? 'demo',
      attemptNumber: Practical._asInt(json['attemptNumber'], fallback: 1),
      currentState: json['currentState'] as String? ?? '',
      durationSeconds: json['durationSeconds'] == null
          ? null
          : Practical._asInt(json['durationSeconds']),
      unitySceneId: json['unitySceneId'] as String? ?? '',
      unityBuildUrl: json['unityBuildUrl'] as String? ?? '',
      startedAt: json['startedAt'] as String?,
    );
  }
}

class PracticalResult {
  const PracticalResult({
    required this.resultId,
    required this.practicalId,
    required this.attemptType,
    required this.attemptNumber,
    required this.score,
    required this.maxScore,
    required this.percentage,
    required this.status,
    this.durationSeconds,
    this.currentState,
    this.title,
  });

  final String resultId;
  final String practicalId;
  final String attemptType;
  final int attemptNumber;
  final int score;
  final int maxScore;
  final double percentage;
  final String status;
  final int? durationSeconds;
  final String? currentState;
  final String? title;

  String get durationLabel {
    final seconds = durationSeconds ?? 0;
    final minutes = seconds ~/ 60;
    final remain = seconds % 60;
    return '$minutes min $remain sec';
  }

  factory PracticalResult.fromJson(Map<String, dynamic> json) {
    return PracticalResult(
      resultId: json['resultId'] as String? ?? '',
      practicalId: json['practicalId'] as String? ?? '',
      attemptType: json['attemptType'] as String? ?? 'practical',
      attemptNumber: Practical._asInt(json['attemptNumber'], fallback: 1),
      score: Practical._asInt(json['score']),
      maxScore: Practical._asInt(json['maxScore'], fallback: 10),
      percentage: Practical._asDouble(json['percentage']),
      status: json['status'] as String? ?? 'completed',
      durationSeconds: json['durationSeconds'] == null
          ? null
          : Practical._asInt(json['durationSeconds']),
      currentState: json['currentState'] as String?,
      title: json['title'] as String?,
    );
  }
}

class StudentPracticalProgress {
  const StudentPracticalProgress({
    required this.studentId,
    required this.grade,
    required this.totalPracticals,
    required this.completedPracticals,
    required this.totalScore,
    required this.averagePercentage,
    required this.gradeProgress,
  });

  final String studentId;
  final int grade;
  final int totalPracticals;
  final int completedPracticals;
  final int totalScore;
  final double averagePercentage;
  final Map<String, Map<String, num>> gradeProgress;

  factory StudentPracticalProgress.fromJson(Map<String, dynamic> json) {
    final rawGrades = json['gradeProgress'];
    final grades = <String, Map<String, num>>{};
    if (rawGrades is Map) {
      rawGrades.forEach((key, value) {
        if (value is Map) {
          grades[key.toString()] = {
            'totalPracticals': Practical._asInt(value['totalPracticals']),
            'completedPracticals': Practical._asInt(value['completedPracticals']),
            'totalScore': Practical._asInt(value['totalScore']),
            'averagePercentage': Practical._asDouble(value['averagePercentage']),
          };
        }
      });
    }
    return StudentPracticalProgress(
      studentId: json['studentId'] as String? ?? '',
      grade: Practical._asInt(json['grade']),
      totalPracticals: Practical._asInt(json['totalPracticals']),
      completedPracticals: Practical._asInt(json['completedPracticals']),
      totalScore: Practical._asInt(json['totalScore']),
      averagePercentage: Practical._asDouble(json['averagePercentage']),
      gradeProgress: grades,
    );
  }
}

class PracticalRunArgs {
  const PracticalRunArgs({
    required this.practical,
    required this.session,
  });

  final Practical practical;
  final PracticalSession session;
}

/// Built-in catalogue so Practical Hub is never empty when Firestore/API
/// have not been seeded yet. Matches Unity scene names in PracticalManager.
class LocalPracticals {
  static const forceBasic = Practical(
    id: 'grade9_force_basic',
    title: 'Basic Concepts Associated with Force',
    description:
        'Use a spring balance to measure weight and mass of given objects (Activity 4.1).',
    grade: 9,
    lessonId: 'phy-g9-force-doc',
    topicId: 'topic-g9-force',
    unitySceneId: 'ForceBasicConcepts',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 1,
    isActive: true,
  );

  static const densityWater = Practical(
    id: 'grade9_density_water',
    title: 'Density of Water 1',
    description: 'Measure mass and volume of water and calculate density.',
    grade: 9,
    lessonId: 'phy-g9-density-doc',
    topicId: 'topic-g9-density',
    unitySceneId: 'DensityWaterExperiment',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 1,
    isActive: true,
  );

  static const all = [forceBasic, densityWater];

  static List<Practical> forLesson(String? lessonId) {
    if (lessonId == null || lessonId.isEmpty) return List<Practical>.from(all);
    return all.where((item) => item.lessonId == lessonId).toList();
  }
}
