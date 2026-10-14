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
    if (currentState == 'PRACTICAL_IN_PROGRESS') return false;
    return true;
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
    final parsed = Practical(
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
    return LocalPracticals.align(parsed);
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
      unitySceneId: LocalPracticals.sceneFor(practical.id, practical.unitySceneId),
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
      unitySceneId: LocalPracticals.sceneFor(
        json['practicalId'] as String? ?? '',
        json['unitySceneId'] as String? ?? '',
      ),
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

class RecentPracticalItem {
  const RecentPracticalItem({
    required this.practicalId,
    required this.title,
    required this.score,
    required this.percentage,
    this.completedAt,
    this.attemptType = 'practical',
  });

  final String practicalId;
  final String title;
  final int score;
  final double percentage;
  final String? completedAt;
  final String attemptType;

  factory RecentPracticalItem.fromJson(Map<String, dynamic> json) {
    return RecentPracticalItem(
      practicalId: json['practicalId'] as String? ?? '',
      title: json['title'] as String? ?? 'Practical',
      score: Practical._asInt(json['score']),
      percentage: Practical._asDouble(json['percentage']),
      completedAt: json['completedAt'] as String?,
      attemptType: json['attemptType'] as String? ?? 'practical',
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
    this.recentResults = const [],
  });

  final String studentId;
  final int grade;
  final int totalPracticals;
  final int completedPracticals;
  final int totalScore;
  final double averagePercentage;
  final Map<String, Map<String, num>> gradeProgress;
  final List<RecentPracticalItem> recentResults;

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
    final rawRecent = json['recentResults'];
    final recent = <RecentPracticalItem>[];
    if (rawRecent is List) {
      for (final item in rawRecent) {
        if (item is Map) {
          recent.add(RecentPracticalItem.fromJson(Map<String, dynamic>.from(item)));
        }
      }
    }
    return StudentPracticalProgress(
      studentId: json['studentId'] as String? ?? '',
      grade: Practical._asInt(json['grade']),
      totalPracticals: Practical._asInt(json['totalPracticals']),
      completedPracticals: Practical._asInt(json['completedPracticals']),
      totalScore: Practical._asInt(json['totalScore']),
      averagePercentage: Practical._asDouble(json['averagePercentage']),
      gradeProgress: grades,
      recentResults: recent,
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

  static const pressureSolid = Practical(
    id: 'grade9_pressure_solid',
    title: 'Pressure Exerted by Solids',
    description:
        'Use a thin wire and sandbags to cut through soap and record Table 5.1.',
    grade: 9,
    lessonId: 'phy-g9-pressure-solid-doc',
    topicId: 'topic-g9-pressure-solid',
    unitySceneId: 'PressureExertedBySolid',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 1,
    isActive: true,
  );

  static const reflectionPrism = Practical(
    id: 'grade9_reflection_prism',
    title: 'Dispersion of White Light through a Glass Prism',
    description:
        'Select the apparatus, send a thin white beam through a glass prism, and record the ROYGBIV spectrum.',
    grade: 9,
    lessonId: 'phy-g9-reflection-doc',
    topicId: 'topic-g9-reflection',
    unitySceneId: 'ReflectionPrismExperiment',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 4,
    isActive: true,
  );

  static const leverActivity = Practical(
    id: 'grade9_lever_15_1',
    title: 'Lever — Activity 15.1',
    description:
        'Set up a first-class lever, measure effort for different load distances, and record the relationship.',
    grade: 9,
    lessonId: 'phy-g9-lever-doc',
    topicId: 'topic-g9-lever',
    unitySceneId: 'LeverActivity15_1',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 5,
    isActive: true,
  );

  static const hydrostaticPressure = Practical(
    id: 'grade10_hydrostatic_pressure',
    title: 'Hydrostatic pressure and its applications',
    description:
        'Measure upthrust with a Eureka can and spring balance, then complete the observation table.',
    grade: 10,
    lessonId: 'phy-g10-hydrostatic-doc',
    topicId: 'topic-g10-hydrostatic',
    unitySceneId: 'HydrostaticPressureExperiment',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 6,
    isActive: true,
  );

  static const workEnergyPower = Practical(
    id: 'grade10_work_energy_power',
    title: 'Work, energy and power',
    description:
        'Drop a weight onto clay from different heights and relate potential energy, work and power.',
    grade: 10,
    lessonId: 'phy-g10-work-energy-doc',
    topicId: 'topic-g10-work-energy',
    unitySceneId: 'WorkEnergyPowerExperiment',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 7,
    isActive: true,
  );

  static const currentElectricity = Practical(
    id: 'grade10_current_electricity',
    title: 'Current electricity',
    description:
        'Build circuits with two dry cells and compare series, parallel and opposing connections.',
    grade: 10,
    lessonId: 'phy-g10-current-electricity-doc',
    topicId: 'topic-g10-current-electricity',
    unitySceneId: 'CurrentElectricityExperiment',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 8,
    isActive: true,
  );

  static const motionStraightLine = Practical(
    id: 'grade10_motion_straight_line',
    title: 'Motion in a straight line',
    description:
        'Time a toy car on a track and compare distance, displacement, speed, velocity and acceleration.',
    grade: 10,
    lessonId: 'phy-g10-motion-straight-doc',
    topicId: 'topic-g10-motion-straight',
    unitySceneId: 'MotionStraightLineExperiment',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 9,
    isActive: true,
  );

  static const newtonsLaws = Practical(
    id: 'grade10_newtons_laws',
    title: "Newton's laws of motion",
    description:
        "Investigate Newton's first, second and third laws with a trolley, pulley and spring balance.",
    grade: 10,
    lessonId: 'phy-g10-newtons-laws-doc',
    topicId: 'topic-g10-newtons-laws',
    unitySceneId: 'NewtonsLawsExperiment',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 10,
    isActive: true,
  );

  static const friction = Practical(
    id: 'grade10_friction',
    title: 'Friction',
    description:
        'Pull a wooden block on sandpaper and compare limiting friction for three contact areas.',
    grade: 10,
    lessonId: 'phy-g10-friction-doc',
    topicId: 'topic-g10-friction',
    unitySceneId: 'FrictionExperiment',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 11,
    isActive: true,
  );

  static const resultantForce = Practical(
    id: 'grade10_resultant_force',
    title: 'Resultant force',
    description:
        'Find the resultant of two forces with spring balances, a trolley and pulleys.',
    grade: 10,
    lessonId: 'phy-g10-resultant-force-doc',
    topicId: 'topic-g10-resultant-force',
    unitySceneId: 'ResultantForceExperiment',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 12,
    isActive: true,
  );

  static const turningEffect = Practical(
    id: 'grade10_turning_effect',
    title: 'Turning effect of a force',
    description:
        'Investigate the moment of a force with a clamped stick, newton balance and pivot screw.',
    grade: 10,
    lessonId: 'phy-g10-turning-effect-doc',
    topicId: 'topic-g10-turning-effect',
    unitySceneId: 'TurningEffectExperiment',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 13,
    isActive: true,
  );

  static const equilibriumOfForces = Practical(
    id: 'grade10_equilibrium',
    title: 'Equilibrium of Forces',
    description:
        'Balance a meter ruler under three coplanar parallel forces and compare F1 + F2 with weight W.',
    grade: 10,
    lessonId: 'phy-g10-equilibrium-doc',
    topicId: 'topic-g10-equilibrium',
    unitySceneId: 'EquilibriumOfForcesExperiment',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 14,
    isActive: true,
  );

  static const wavesApplications = Practical(
    id: 'grade11_waves',
    title: 'Waves and their applications',
    description:
        'Demonstrate formation of a transverse wave with a slinky and observe ribbon motion.',
    grade: 11,
    lessonId: 'phy-g11-waves-doc',
    topicId: 'topic-g11-waves',
    unitySceneId: 'WavesApplicationsExperiment',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 15,
    isActive: true,
  );

  static const geometricalOptics = Practical(
    id: 'grade11_geometrical_optics',
    title: 'Geometrical Optics',
    description:
        'Find the focal length of a concave mirror using a distant object and a white screen.',
    grade: 11,
    lessonId: 'phy-g11-optics-doc',
    topicId: 'topic-g11-optics',
    unitySceneId: 'GeometricalOpticsExperiment',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 16,
    isActive: true,
  );

  static const heatExpansion = Practical(
    id: 'grade11_heat',
    title: 'Heat',
    description:
        'Illustrate expansion of liquids with a test tube, thin glass tube and a water bath.',
    grade: 11,
    lessonId: 'phy-g11-heat-doc',
    topicId: 'topic-g11-heat',
    unitySceneId: 'HeatExpansionExperiment',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 17,
    isActive: true,
  );

  static const powerEnergyAppliances = Practical(
    id: 'grade11_power_appliances',
    title: 'Power and Energy of Electric Appliances',
    description:
        'Measure voltage and current of electric appliances, then calculate power, energy and kilowatt-hours.',
    grade: 11,
    lessonId: 'phy-g11-power-appliances-doc',
    topicId: 'topic-g11-power-appliances',
    unitySceneId: 'PowerEnergyAppliancesExperiment',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 18,
    isActive: true,
  );

  static const electronicsDiode = Practical(
    id: 'grade11_electronics',
    title: 'Electronics',
    description:
        'Investigate forward bias and reverse bias of a diode using a battery, switch and bulb.',
    grade: 11,
    lessonId: 'phy-g11-electronics-doc',
    topicId: 'topic-g11-electronics',
    unitySceneId: 'ElectronicsDiodeExperiment',
    unityBuildUrl: '',
    maxScore: 100,
    durationSeconds: 600,
    demoAllowed: true,
    demoMaxAttempts: 10,
    practicalMaxAttempts: 3,
    order: 19,
    isActive: true,
  );

  static const all = [
    forceBasic,
    pressureSolid,
    densityWater,
    reflectionPrism,
    leverActivity,
    hydrostaticPressure,
    workEnergyPower,
    currentElectricity,
    motionStraightLine,
    newtonsLaws,
    friction,
    resultantForce,
    turningEffect,
    equilibriumOfForces,
    wavesApplications,
    geometricalOptics,
    heatExpansion,
    powerEnergyAppliances,
    electronicsDiode,
  ];

  static int? parseGrade(dynamic raw) {
    if (raw == null) return null;
    if (raw is num) {
      final n = raw.toInt();
      return (n == 9 || n == 10 || n == 11) ? n : null;
    }
    final match = RegExp(r'(\d{1,2})').firstMatch('$raw');
    final n = int.tryParse(match?.group(1) ?? '');
    return (n == 9 || n == 10 || n == 11) ? n : null;
  }

  static List<Practical> forGrade(int grade) {
    return all.where((item) => item.grade == grade).toList();
  }

  static List<Practical> forLesson(String? lessonId) {
    if (lessonId == null || lessonId.isEmpty) {
      return const [];
    }
    return all.where((item) => item.lessonId == lessonId).toList();
  }

  static Practical? byId(String id) {
    final canonical = canonicalId(id);
    for (final item in all) {
      if (item.id == canonical) return item;
    }
    return null;
  }

  static const sceneById = <String, String>{
    'grade9_force_basic': 'ForceBasicConcepts',
    'grade9_density_water': 'DensityWaterExperiment',
    'grade9_pressure_solid': 'PressureExertedBySolid',
    'grade9_reflection_prism': 'ReflectionPrismExperiment',
    'grade9_lever_15_1': 'LeverActivity15_1',
    'grade10_hydrostatic_pressure': 'HydrostaticPressureExperiment',
    'grade10_work_energy_power': 'WorkEnergyPowerExperiment',
    'grade10_current_electricity': 'CurrentElectricityExperiment',
    'grade10_motion_straight_line': 'MotionStraightLineExperiment',
    'grade10_newtons_laws': 'NewtonsLawsExperiment',
    'grade10_friction': 'FrictionExperiment',
    'grade10_resultant_force': 'ResultantForceExperiment',
    'grade10_turning_effect': 'TurningEffectExperiment',
    'grade10_equilibrium': 'EquilibriumOfForcesExperiment',
    'grade11_waves': 'WavesApplicationsExperiment',
    'grade11_geometrical_optics': 'GeometricalOpticsExperiment',
    'grade11_heat': 'HeatExpansionExperiment',
    'grade11_power_appliances': 'PowerEnergyAppliancesExperiment',
    'grade11_electronics': 'ElectronicsDiodeExperiment',
    'grade11_electronics_diode': 'ElectronicsDiodeExperiment',
  };

  static const _idAliases = <String, String>{
    'grade11_electronics_diode': 'grade11_electronics',
  };

  static String canonicalId(String id) => _idAliases[id] ?? id;

  static String sceneFor(String practicalId, [String fallback = '']) {
    return sceneById[practicalId] ?? sceneById[canonicalId(practicalId)] ?? fallback;
  }

  static Practical align(Practical live) {
    final local = byId(live.id);
    if (local == null) {
      final scene = sceneFor(live.id, live.unitySceneId);
      if (scene == live.unitySceneId) return live;
      return _copy(live, unitySceneId: scene);
    }
    return Practical(
      id: local.id,
      title: local.title,
      description: local.description,
      grade: local.grade,
      lessonId: local.lessonId,
      topicId: local.topicId,
      unitySceneId: local.unitySceneId,
      unityBuildUrl: local.unityBuildUrl,
      maxScore: local.maxScore,
      durationSeconds: local.durationSeconds,
      demoAllowed: local.demoAllowed,
      demoMaxAttempts: local.demoMaxAttempts,
      practicalMaxAttempts: local.practicalMaxAttempts,
      order: local.order,
      isActive: local.isActive,
      currentState: live.currentState,
      demoAttemptsUsed: live.demoAttemptsUsed,
      practicalAttemptsUsed: live.practicalAttemptsUsed,
      demoCompleted: live.demoCompleted,
      completed: live.completed,
      bestScore: live.bestScore,
      latestScore: live.latestScore,
      percentage: live.percentage,
    );
  }

  static Practical _copy(Practical live, {required String unitySceneId}) {
    return Practical(
      id: live.id,
      title: live.title,
      description: live.description,
      grade: live.grade,
      lessonId: live.lessonId,
      topicId: live.topicId,
      unitySceneId: unitySceneId,
      unityBuildUrl: live.unityBuildUrl,
      maxScore: live.maxScore,
      durationSeconds: live.durationSeconds,
      demoAllowed: live.demoAllowed,
      demoMaxAttempts: live.demoMaxAttempts,
      practicalMaxAttempts: live.practicalMaxAttempts,
      order: live.order,
      isActive: live.isActive,
      currentState: live.currentState,
      demoAttemptsUsed: live.demoAttemptsUsed,
      practicalAttemptsUsed: live.practicalAttemptsUsed,
      demoCompleted: live.demoCompleted,
      completed: live.completed,
      bestScore: live.bestScore,
      latestScore: live.latestScore,
      percentage: live.percentage,
    );
  }

  static String _norm(String value) {
    return value
        .toLowerCase()
        .replaceAll("'", '')
        .replaceAll('&', ' ')
        .replaceAll(RegExp(r'[^a-z0-9]+'), ' ')
        .trim()
        .replaceAll(RegExp(r'\s+'), ' ');
  }

  /// Extra titles used in Lessons / search that must not share another topic's lab.
  static const _titleAliases = <String, String>{
    'basic concepts associated with force': 'grade9_force_basic',
    'force basic concepts': 'grade9_force_basic',
    'pressure exerted by solid': 'grade9_pressure_solid',
    'pressure exerted by solids': 'grade9_pressure_solid',
    'density': 'grade9_density_water',
    'density of water': 'grade9_density_water',
    'density of water 1': 'grade9_density_water',
    'reflection and refraction of waves': 'grade9_reflection_prism',
    'dispersion of white light through a glass prism': 'grade9_reflection_prism',
    'lever activity 15 1': 'grade9_lever_15_1',
    'simple machines': 'grade9_lever_15_1',
    'motion in a straight line': 'grade10_motion_straight_line',
    'linear motion': 'grade10_motion_straight_line',
    'newtons laws of motion': 'grade10_newtons_laws',
    'forces and newtons laws': 'grade10_newtons_laws',
    'friction': 'grade10_friction',
    'resultant force': 'grade10_resultant_force',
    'turning effect of a force': 'grade10_turning_effect',
    'equilibrium of forces': 'grade10_equilibrium',
    'hydrostatic pressure and its applications': 'grade10_hydrostatic_pressure',
    'hydrostatic pressure': 'grade10_hydrostatic_pressure',
    'work energy and power': 'grade10_work_energy_power',
    'current electricity': 'grade10_current_electricity',
    'waves and their applications': 'grade11_waves',
    'waves': 'grade11_waves',
    'geometrical optics': 'grade11_geometrical_optics',
    'light and optics': 'grade11_geometrical_optics',
    'heat': 'grade11_heat',
    'heat temperature changes': 'grade11_heat',
    'heat temperature': 'grade11_heat',
    'power and energy of electric appliances': 'grade11_power_appliances',
    'electronics': 'grade11_electronics',
    'electronics logic gates': 'grade11_electronics',
    'electronics diode properties circuit behavior': 'grade11_electronics',
    'electronics diode properties': 'grade11_electronics',
    'investigation of forward bias and reverse bias of a diode': 'grade11_electronics',
  };

  static const _lessonIdAliases = <String, String>{
    'phy-g10-motion-doc': 'grade10_motion_straight_line',
    'phy-g9-force': 'grade9_force_basic',
    'phy-g9-pressure': 'grade9_pressure_solid',
    'phy-g9-density': 'grade9_density_water',
    'phy-g11-electronics': 'grade11_electronics',
  };

  static Practical? _fromLessonId(String? lessonId) {
    if (lessonId == null || lessonId.isEmpty) return null;
    final exact = forLesson(lessonId);
    if (exact.length == 1) return exact.first;
    return byId(_lessonIdAliases[lessonId] ?? '');
  }

  static Practical? _fromTitle(String? title, {String extra = '', int? grade}) {
    if (title == null || title.trim().isEmpty) {
      final id = _keywordMatch(_norm(extra), grade);
      return id == null ? null : byId(id);
    }
    final key = _norm(title);
    final combined = _norm('$title $extra');
    var id = _titleAliases[key];
    if (id == null) {
      for (final item in all) {
        if (_norm(item.title) == key) {
          id = item.id;
          break;
        }
      }
    }
    if (id != null) {
      final matched = byId(id);
      if (matched == null) return null;
      if (grade != null && matched.grade != grade) return null;
      return matched;
    }
    final keywordId = _keywordMatch(combined, grade);
    return keywordId == null ? null : byId(keywordId);
  }

  /// Resolve exactly one practical for a lesson/topic. Returns null when the
  /// topic has no lab of its own — never reuses another topic's practical.
  static Practical? forTopic({
    String? practicalId,
    String? lessonId,
    String? title,
    int? grade,
  }) {
    // Title / lessonId win over a stale Firestore practicalId (often wrong or shared).
    final fromTitle = _fromTitle(title, extra: lessonId ?? '', grade: grade);
    final fromLesson = _fromLessonId(lessonId);
    if (fromTitle != null && fromLesson != null && fromTitle.id != fromLesson.id) {
      return fromTitle;
    }
    if (fromTitle != null) return fromTitle;
    if (fromLesson != null) return fromLesson;

    final fromId = byId(practicalId ?? '');
    if (fromId == null) return null;
    if (grade != null && fromId.grade != grade) return null;
    // Only accept practicalId alone when we have no title to verify against.
    if (title == null || title.trim().isEmpty) return fromId;
    return null;
  }

  static String? matchTopicId(String title, [String extra = '', int? grade]) {
    return _fromTitle(title, extra: extra, grade: grade)?.id;
  }

  /// Exclusive keyword rules. Broad words such as force / pressure / wave
  /// must not steal another topic's lab.
  static String? _keywordMatch(String t, int? grade) {
    if (t.isEmpty) return null;
    bool has(String s) => t.contains(s);

    if (has('newton')) return 'grade10_newtons_laws';
    if (has('friction')) return 'grade10_friction';
    if (has('resultant')) return 'grade10_resultant_force';
    if (has('turning') || (has('moment') && has('force'))) {
      return 'grade10_turning_effect';
    }
    if (has('equilibrium')) return 'grade10_equilibrium';
    if (has('hydrostatic') || has('upthrust') || has('archimedes')) {
      return 'grade10_hydrostatic_pressure';
    }
    if (has('appliance')) return 'grade11_power_appliances';
    if (has('electronics') || has('diode') || has('logic gate')) {
      return 'grade11_electronics';
    }
    if (has('current') && has('electric') && !has('electronics')) {
      return 'grade10_current_electricity';
    }
    if ((has('geometrical') ||
            (has('optic') && !has('prism')) ||
            has('concave mirror') ||
            has('focal length')) &&
        !has('prism')) {
      return 'grade11_geometrical_optics';
    }
    if (has('heat') && !has('work')) return 'grade11_heat';
    if (has('wave') && (has('application') || has('slinky') || has('transverse'))) {
      return 'grade11_waves';
    }
    if (has('prism') || has('dispersion') || has('roygbiv')) {
      return 'grade9_reflection_prism';
    }
    if ((has('reflect') || has('refract')) && has('wave') && !has('application')) {
      return 'grade9_reflection_prism';
    }
    if (has('lever') || has('simple machine')) return 'grade9_lever_15_1';
    if (has('density') && !has('hydrostatic')) {
      if (grade == 10) return null;
      if (grade == 9 || grade == null) return 'grade9_density_water';
      return null;
    }
    if (has('work') && has('energy') && !has('appliance')) {
      return 'grade10_work_energy_power';
    }
    if ((has('straight') && has('line')) || has('linear motion')) {
      return 'grade10_motion_straight_line';
    }
    if (has('pressure') && (has('solid') || has('soap') || has('wire'))) {
      return 'grade9_pressure_solid';
    }
    if (has('force') &&
        (has('basic') || has('weight') || has('mass') || has('spring'))) {
      return 'grade9_force_basic';
    }
    return null;
  }
}
