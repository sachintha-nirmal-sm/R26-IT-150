import 'dart:async';

import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:firebase_auth/firebase_auth.dart';

import '../../../core/api/api_client.dart';
import 'practical.dart';

class PracticalsRepository {
  PracticalsRepository({
    ApiClient? api,
    FirebaseAuth? auth,
    FirebaseFirestore? firestore,
  })  : _api = api ?? ApiClient(),
        _auth = auth ?? FirebaseAuth.instance,
        _db = firestore ?? FirebaseFirestore.instance;

  final ApiClient _api;
  final FirebaseAuth _auth;
  final FirebaseFirestore _db;

  /// Hub list is read from Firestore so Practical Hub opens without FastAPI.
  /// An empty Firestore result is not treated as success — try API, then local.
  /// Labs (no lessonId) always stay inside the student's current grade.
  Future<List<Practical>> fetchActiveForCurrentStudent({
    String? lessonId,
    int? grade,
  }) async {
    final resolvedGrade = grade ?? await _tryCurrentGrade();
    List<Practical> items = const [];
    try {
      items = await _fetchActiveFromFirestore(
        lessonId: lessonId,
        grade: resolvedGrade,
      );
    } catch (_) {}
    if (items.isEmpty) {
      try {
        items = await _fetchActiveFromApi(lessonId: lessonId);
      } catch (_) {}
    }
    return _withLocalFallbacks(
      items,
      lessonId: lessonId,
      grade: resolvedGrade,
    );
  }

  Future<Practical> fetchById(String practicalId) async {
    try {
      final decoded = await _api.get('/api/practicals/$practicalId');
      return Practical.fromJson(_asMap(decoded));
    } catch (_) {}
    try {
      return await _fetchByIdFromFirestore(practicalId);
    } catch (_) {}
    for (final local in LocalPracticals.all) {
      if (local.id == practicalId) return local;
    }
    throw ApiException('Practical "$practicalId" was not found.');
  }

  Future<PracticalSession> startDemo(String practicalId) async {
    final decoded = await _api.post('/api/practicals/$practicalId/demo/start');
    return PracticalSession.fromJson(_asMap(decoded));
  }

  Future<PracticalResult> finishDemo({
    required String practicalId,
    required String resultId,
    int? score,
    Map<String, dynamic>? measurements,
    Map<String, dynamic>? calculations,
    Map<String, dynamic>? evaluation,
  }) async {
    final decoded = await _api.post(
      '/api/practicals/$practicalId/demo/finish',
      body: {
        'resultId': resultId,
        if (score != null) 'score': score,
        if (measurements != null) 'measurements': measurements,
        if (calculations != null) 'calculations': calculations,
        if (evaluation != null) 'evaluation': evaluation,
      },
    );
    return PracticalResult.fromJson(_asMap(decoded));
  }

  Future<PracticalSession> startPractical(String practicalId) async {
    final decoded = await _api.post('/api/practicals/$practicalId/start');
    return PracticalSession.fromJson(_asMap(decoded));
  }

  Future<PracticalResult> submitPractical({
    required String practicalId,
    required String resultId,
    required int attemptNumber,
    required int score,
    int? durationSeconds,
    Map<String, dynamic>? measurements,
    Map<String, dynamic>? calculations,
    Map<String, dynamic>? evaluation,
  }) async {
    final decoded = await _api.post(
      '/api/practicals/$practicalId/submit',
      body: {
        'resultId': resultId,
        'attemptNumber': attemptNumber,
        'score': score,
        if (durationSeconds != null) 'durationSeconds': durationSeconds,
        if (measurements != null) 'measurements': measurements,
        if (calculations != null) 'calculations': calculations,
        if (evaluation != null) 'evaluation': evaluation,
      },
    );
    return PracticalResult.fromJson(_asMap(decoded));
  }

  /// Official Start saves to the same Firebase project the student signed into.
  Future<PracticalResult> recordOfficialScore({
    required PracticalSession session,
    required int score,
    int? durationSeconds,
    Map<String, dynamic>? measurements,
  }) async {
    final saved = await _saveOfficialToFirestore(
      session: session,
      score: score,
      durationSeconds: durationSeconds,
      measurements: measurements,
    );
    try {
      await _api.post(
        '/api/practicals/${session.practicalId}/complete',
        body: {
          'score': score.clamp(0, 100),
          if (durationSeconds != null)
            'durationSeconds': durationSeconds < 0 ? 0 : durationSeconds,
          if (measurements != null) 'measurements': _jsonSafe(measurements),
        },
      );
    } catch (_) {}
    return saved;
  }

  Future<PracticalResult> _saveOfficialToFirestore({
    required PracticalSession session,
    required int score,
    int? durationSeconds,
    Map<String, dynamic>? measurements,
  }) async {
    final uid = _auth.currentUser?.uid;
    if (uid == null) {
      throw ApiException('Sign in required', statusCode: 401);
    }
    final practicalId = session.practicalId;
    final local = LocalPracticals.byId(practicalId);
    final clamped = score.clamp(0, 100);
    final maxScore = local?.maxScore ?? 100;
    final percentage = maxScore <= 0 ? 0.0 : (100.0 * clamped / maxScore);
    final resultId =
        (session.resultId.isEmpty || session.resultId.startsWith('local-'))
            ? 'pr_${DateTime.now().microsecondsSinceEpoch}'
            : session.resultId;
    final now = DateTime.now().toUtc().toIso8601String();
    final spId = '${uid}_$practicalId';
    final spSnap = await _db.collection('studentPracticals').doc(spId).get();
    final prev = spSnap.data() ?? {};
    final previousBest = (prev['bestScore'] as num?)?.toInt() ?? 0;
    final best = clamped > previousBest ? clamped : previousBest;
    final firstCompletion = prev['completed'] != true;

    await _db.collection('practicalResults').doc(resultId).set({
      'studentId': uid,
      'practicalId': practicalId,
      'lessonId': local?.lessonId ?? '',
      'grade': local?.grade ?? 0,
      'attemptType': 'practical',
      'mode': 'start',
      'attemptNumber': session.attemptNumber,
      'score': clamped,
      'maxScore': maxScore,
      'percentage': percentage,
      'startedAt': prev['activeStartedAt'] ?? now,
      'completedAt': now,
      'durationSeconds': durationSeconds ?? 0,
      'status': 'completed',
      if (measurements != null) 'measurements': _jsonSafe(measurements),
    });

    await _db.collection('studentPracticals').doc(spId).set({
      'studentId': uid,
      'practicalId': practicalId,
      'grade': local?.grade ?? 0,
      'completed': true,
      'bestScore': best,
      'latestScore': clamped,
      'percentage': maxScore <= 0 ? 0.0 : (100.0 * best / maxScore),
      'practicalAttemptsUsed':
          ((prev['practicalAttemptsUsed'] as num?)?.toInt() ?? 0) + 1,
      'currentState': 'SUBMITTED',
      'lastAttemptAt': now,
      'activeResultId': null,
    }, SetOptions(merge: true));

    await _bumpStudentProgress(
      uid: uid,
      grade: local?.grade ?? 0,
      firstCompletion: firstCompletion,
      score: clamped,
    );

    return PracticalResult(
      resultId: resultId,
      practicalId: practicalId,
      attemptType: 'practical',
      attemptNumber: session.attemptNumber,
      score: clamped,
      maxScore: maxScore,
      percentage: percentage,
      status: 'completed',
      durationSeconds: durationSeconds,
      currentState: 'SUBMITTED',
      title: local?.title,
    );
  }

  Future<void> _bumpStudentProgress({
    required String uid,
    required int grade,
    required bool firstCompletion,
    required int score,
  }) async {
    final ref = _db.collection('studentProgress').doc(uid);
    await _db.runTransaction((tx) async {
      final snap = await tx.get(ref);
      final data = snap.data() ?? {};
      final completed = (data['completedPracticals'] as num?)?.toInt() ?? 0;
      final totalScore = (data['totalScore'] as num?)?.toInt() ?? 0;
      final nextCompleted = firstCompletion ? completed + 1 : completed;
      final nextTotal = firstCompletion ? totalScore + score : totalScore;
      final average =
          nextCompleted <= 0 ? 0.0 : nextTotal / nextCompleted;
      tx.set(ref, {
        'studentId': uid,
        'grade': data['grade'] ?? grade,
        'totalPracticals': data['totalPracticals'] ?? LocalPracticals.all.length,
        'completedPracticals': nextCompleted,
        'totalScore': nextTotal,
        'averagePercentage': average,
        'updatedAt': DateTime.now().toUtc().toIso8601String(),
      }, SetOptions(merge: true));
    });
  }

  Map<String, dynamic> _jsonSafe(Map<String, dynamic> input) {
    final out = <String, dynamic>{};
    input.forEach((key, value) {
      if (value == null || value is num || value is bool || value is String) {
        out[key] = value;
      } else {
        out[key] = value.toString();
      }
    });
    return out;
  }

  Future<PracticalResult> fetchOfficialResult(String practicalId) async {
    final decoded = await _api.get('/api/practicals/$practicalId/result');
    final map = _asMap(decoded);
    final latest = map['latest'];
    final best = map['best'];
    final source = latest is Map
        ? latest
        : best is Map
            ? best
            : null;
    if (source == null) {
      throw ApiException('No official result found for this practical.');
    }
    return PracticalResult.fromJson(Map<String, dynamic>.from(source));
  }

  Future<StudentPracticalProgress> fetchMyProgress() async {
    try {
      return await _progressFromFirestore();
    } catch (_) {}
    final decoded = await _api.get('/api/students/me/progress');
    return StudentPracticalProgress.fromJson(_asMap(decoded));
  }

  Future<StudentPracticalProgress> _progressFromFirestore() async {
    final uid = _auth.currentUser?.uid;
    if (uid == null) {
      throw ApiException('Sign in required', statusCode: 401);
    }
    final snap = await _db.collection('studentProgress').doc(uid).get()
        .timeout(const Duration(seconds: 5));
    final data = Map<String, dynamic>.from(snap.data() ?? {});
    data['studentId'] = uid;

    final results = await _db
        .collection('practicalResults')
        .where('studentId', isEqualTo: uid)
        .get()
        .timeout(const Duration(seconds: 5));
    final rows = <Map<String, dynamic>>[];
    for (final doc in results.docs) {
      final item = doc.data();
      if (item['attemptType'] != 'practical') continue;
      final status = item['status'] as String? ?? '';
      if (status != 'completed' && status != 'timeExpired') continue;
      final practicalId = item['practicalId'] as String? ?? '';
      rows.add({
        'practicalId': practicalId,
        'title': LocalPracticals.byId(practicalId)?.title ?? practicalId,
        'score': item['score'] ?? 0,
        'percentage': item['percentage'] ?? 0,
        'completedAt': _asIso(item['completedAt'] ?? item['startedAt']),
        'attemptType': 'practical',
      });
    }
    rows.sort((a, b) {
      final left = a['completedAt'] as String? ?? '';
      final right = b['completedAt'] as String? ?? '';
      return right.compareTo(left);
    });
    data['recentResults'] = rows.take(12).toList();
    return StudentPracticalProgress.fromJson(data);
  }

  String? _asIso(dynamic value) {
    if (value == null) return null;
    if (value is Timestamp) return value.toDate().toUtc().toIso8601String();
    if (value is DateTime) return value.toUtc().toIso8601String();
    return value.toString();
  }

  Future<List<Practical>> _fetchActiveFromApi({String? lessonId}) async {
    final decoded = await _api.get(
      '/api/practicals',
      query: {
        if (lessonId != null && lessonId.isNotEmpty) 'lessonId': lessonId,
      },
    );
    if (decoded is! List) return const [];
    return decoded
        .whereType<Map>()
        .map((item) => Practical.fromJson(Map<String, dynamic>.from(item)))
        .toList();
  }

  Future<List<Practical>> _fetchActiveFromFirestore({
    String? lessonId,
    int? grade,
  }) async {
    Query<Map<String, dynamic>> query = _db.collection('practicals');
    if (lessonId != null && lessonId.isNotEmpty) {
      query = query.where('lessonId', isEqualTo: lessonId);
    } else if (grade != null) {
      query = query.where('grade', isEqualTo: grade);
    } else {
      return const [];
    }
    QuerySnapshot<Map<String, dynamic>> snap;
    try {
      snap = await query.where('isActive', isEqualTo: true).get();
    } on FirebaseException {
      snap = await query.get();
    }

    final items = snap.docs
        .map(_fromDoc)
        .where((practical) => practical.isActive)
        .toList()
      ..sort((a, b) => a.order.compareTo(b.order));
    return items;
  }

  Future<Practical> _fetchByIdFromFirestore(String practicalId) async {
    final snap = await _db.collection('practicals').doc(practicalId).get();
    if (!snap.exists || snap.data() == null) {
      throw ApiException('Practical "$practicalId" was not found in Firestore.');
    }
    return _fromDoc(snap);
  }

  List<Practical> _withLocalFallbacks(
    List<Practical> items, {
    String? lessonId,
    int? grade,
  }) {
    final scopedLesson = lessonId != null && lessonId.isNotEmpty;
    final locals = scopedLesson
        ? LocalPracticals.forLesson(lessonId)
        : (grade == null ? const <Practical>[] : LocalPracticals.forGrade(grade));

    var filtered = items.map(LocalPracticals.align).toList();
    if (scopedLesson) {
      filtered = filtered.where((item) => item.lessonId == lessonId).toList();
    } else if (grade != null) {
      filtered = filtered.where((item) => item.grade == grade).toList();
    } else {
      filtered = const [];
    }

    if (filtered.isEmpty) return locals;
    final ids = filtered.map((item) => item.id).toSet();
    final extra = locals.where((item) => !ids.contains(item.id)).toList();
    if (extra.isEmpty) return filtered;
    return [...filtered, ...extra]..sort((a, b) => a.order.compareTo(b.order));
  }

  Future<int?> currentStudentGrade() => _tryCurrentGrade();

  Future<int?> _tryCurrentGrade() async {
    try {
      return await _currentGrade();
    } catch (_) {
      return null;
    }
  }

  Future<int> _currentGrade() async {
    final user = _auth.currentUser;
    if (user == null) {
      throw ApiException('Sign in required', statusCode: 401);
    }
    final snap = await _db.collection('users').doc(user.uid).get();
    final data = snap.data() ?? {};
    final grade = LocalPracticals.parseGrade(data['currentGrade']) ??
        LocalPracticals.parseGrade(data['grade']);
    if (grade == null) {
      throw ApiException('Student grade is not set.');
    }
    return grade;
  }

  Practical _fromDoc(DocumentSnapshot<Map<String, dynamic>> doc) {
    final data = Map<String, dynamic>.from(doc.data() ?? {});
    data['id'] = doc.id;
    return Practical.fromJson(data);
  }

  Map<String, dynamic> _asMap(dynamic decoded) {
    if (decoded is Map<String, dynamic>) return decoded;
    if (decoded is Map) return Map<String, dynamic>.from(decoded);
    throw ApiException('Unexpected response from the practicals API.');
  }
}
