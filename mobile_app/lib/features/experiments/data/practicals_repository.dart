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
  Future<List<Practical>> fetchActiveForCurrentStudent({String? lessonId}) async {
    try {
      final items = await _fetchActiveFromFirestore(lessonId: lessonId);
      if (items.isNotEmpty) return items;
    } catch (_) {}
    try {
      final items = await _fetchActiveFromApi(lessonId: lessonId);
      if (items.isNotEmpty) return items;
    } catch (_) {}
    return LocalPracticals.forLesson(lessonId);
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
        if (measurements != null) 'measurements': measurements,
        if (calculations != null) 'calculations': calculations,
        if (evaluation != null) 'evaluation': evaluation,
      },
    );
    return PracticalResult.fromJson(_asMap(decoded));
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
    final decoded = await _api.get('/api/students/me/progress');
    return StudentPracticalProgress.fromJson(_asMap(decoded));
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

  Future<List<Practical>> _fetchActiveFromFirestore({String? lessonId}) async {
    final grade = await _currentGrade();
    Query<Map<String, dynamic>> query = _db.collection('practicals');
    if (lessonId != null && lessonId.isNotEmpty) {
      query = query.where('lessonId', isEqualTo: lessonId);
    } else {
      query = query.where('grade', isEqualTo: grade);
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

  Future<int> _currentGrade() async {
    final user = _auth.currentUser;
    if (user == null) {
      throw ApiException('Sign in required', statusCode: 401);
    }
    final snap = await _db.collection('users').doc(user.uid).get();
    final raw = snap.data()?['currentGrade'];
    final grade = raw is num ? raw.toInt() : int.tryParse('$raw');
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
