import 'dart:async';
import 'dart:convert';

import 'package:firebase_auth/firebase_auth.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/services.dart';

import 'practical.dart';

class UnitySessionPayload {
  const UnitySessionPayload({
    required this.studentId,
    required this.practicalId,
    required this.lessonId,
    required this.mode,
    required this.attempt,
    required this.timeLimitSeconds,
    required this.resultId,
    required this.unitySceneId,
  });

  final String studentId;
  final String practicalId;
  final String lessonId;
  final String mode;
  final int attempt;
  final int timeLimitSeconds;
  final String resultId;
  final String unitySceneId;

  Map<String, dynamic> toJson() => {
        'studentId': studentId,
        'practicalId': practicalId,
        'lessonId': lessonId,
        'mode': mode,
        'attempt': attempt,
        'attemptNumber': attempt,
        'timeLimitSeconds': timeLimitSeconds,
        'durationSeconds': timeLimitSeconds,
        'resultId': resultId,
        'unitySceneId': unitySceneId,
        'unityScene': unitySceneId,
      };

  String encode() => jsonEncode(toJson());
}

class UnityPracticalResult {
  const UnityPracticalResult({
    required this.practicalId,
    required this.mode,
    required this.attempt,
    required this.score,
    required this.timeUsed,
    required this.completed,
    this.resultId = '',
    this.lessonId = '',
    this.measurements = const {},
  });

  final String practicalId;
  final String resultId;
  final String lessonId;
  final String mode;
  final int attempt;
  final int score;
  final int timeUsed;
  final bool completed;
  final Map<String, dynamic> measurements;

  bool get isOfficial => mode == 'start' || mode == 'practical';

  factory UnityPracticalResult.fromJson(Map<String, dynamic> json) {
    final measurements = json['measurements'];
    return UnityPracticalResult(
      practicalId: json['practicalId'] as String? ?? '',
      resultId: json['resultId'] as String? ?? '',
      lessonId: json['lessonId'] as String? ?? '',
      mode: json['mode'] as String? ?? 'trial',
      attempt: _asInt(json['attempt'] ?? json['attemptNumber'], fallback: 1),
      score: _asInt(json['score'] ?? json['unityScore']).clamp(0, 100),
      timeUsed: _asInt(json['timeUsed']),
      completed: json['completed'] == true,
      measurements: measurements is Map
          ? Map<String, dynamic>.from(measurements)
          : const {},
    );
  }

  static int _asInt(dynamic value, {int fallback = 0}) {
    if (value is int) return value;
    if (value is num) return value.toInt();
    return int.tryParse('$value') ?? fallback;
  }
}

/// Starts Unity inside this Flutter Android app (Unity as a Library).
/// Does not launch a separate APK.
class UnityLabService {
  UnityLabService._();

  static const _channel = MethodChannel('com.example.mobile_app/unity_lab');
  static bool _listening = false;
  static void Function(UnityPracticalResult result)? _onResult;
  static void Function()? _onCancelled;

  static bool get isAndroid =>
      !kIsWeb && defaultTargetPlatform == TargetPlatform.android;

  static Future<bool> isAvailable() async {
    if (!isAndroid) return false;
    final available = await _channel.invokeMethod<bool>('isUnityAvailable');
    return available == true;
  }

  static UnitySessionPayload payloadFor({
    required Practical practical,
    required PracticalSession session,
  }) {
    final uid = FirebaseAuth.instance.currentUser?.uid ?? '';
    final official = !session.isDemo;
    final limit = session.durationSeconds ?? practical.durationSeconds;
    return UnitySessionPayload(
      studentId: uid,
      practicalId: practical.id,
      lessonId: practical.lessonId,
      mode: official ? 'start' : 'trial',
      attempt: session.attemptNumber,
      timeLimitSeconds: limit > 0 ? limit : 600,
      resultId: session.resultId,
      unitySceneId: session.unitySceneId.isNotEmpty
          ? session.unitySceneId
          : practical.unitySceneId,
    );
  }

  static Future<bool> startSession(UnitySessionPayload payload) async {
    _ensureListener();
    if (!isAndroid) return false;
    final started = await _channel.invokeMethod<bool>('startPractical', {
      'sessionJson': payload.encode(),
    });
    return started == true;
  }

  static Future<UnityPracticalResult?> takePendingResult() async {
    if (!isAndroid) return null;
    final raw = await _channel.invokeMethod<String>('takePendingResult');
    if (raw == null || raw.isEmpty) return null;
    return _parse(raw);
  }

  static void listen({
    required void Function(UnityPracticalResult result) onResult,
    void Function()? onCancelled,
  }) {
    _onResult = onResult;
    _onCancelled = onCancelled;
    _ensureListener();
  }

  static void stopListening() {
    _onResult = null;
    _onCancelled = null;
  }

  static void _ensureListener() {
    if (_listening) return;
    _listening = true;
    _channel.setMethodCallHandler((call) async {
      if (call.method == 'onPracticalCompleted') {
        final result = _parse(call.arguments);
        if (result != null) _onResult?.call(result);
      } else if (call.method == 'onPracticalCancelled') {
        _onCancelled?.call();
      }
    });
  }

  static UnityPracticalResult? _parse(dynamic raw) {
    if (raw is! String || raw.isEmpty) return null;
    try {
      final decoded = jsonDecode(raw);
      if (decoded is Map<String, dynamic>) {
        return UnityPracticalResult.fromJson(decoded);
      }
      if (decoded is Map) {
        return UnityPracticalResult.fromJson(Map<String, dynamic>.from(decoded));
      }
    } catch (_) {}
    return null;
  }
}
