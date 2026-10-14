import 'package:firebase_auth/firebase_auth.dart';

import '../../../core/app_navigator.dart';
import 'practical.dart';
import 'practicals_repository.dart';
import 'unity_lab_service.dart';

/// Saves a Unity lab result even if Flutter restarted back at Get Started.
class LabResultSync {
  LabResultSync._();

  static bool _busy = false;

  static void start() {
    UnityLabService.listenApp(_handle);
    Future<void>.delayed(const Duration(milliseconds: 800), flush);
  }

  static Future<void> flush() async {
    final result = await UnityLabService.takePendingResult();
    if (result != null) {
      await _handle(result);
    }
  }

  static Future<void> _handle(UnityPracticalResult result) async {
    if (_busy) return;
    if (FirebaseAuth.instance.currentUser == null) return;
    if (result.practicalId.isEmpty) return;
    _busy = true;
    try {
      if (result.isOfficial) {
        await PracticalsRepository().recordOfficialScore(
          session: PracticalSession(
            practicalId: result.practicalId,
            resultId: result.resultId.isEmpty
                ? 'local-sync'
                : result.resultId,
            mode: 'practical',
            attemptNumber: result.attempt,
            currentState: 'PRACTICAL_IN_PROGRESS',
            unitySceneId: '',
            unityBuildUrl: '',
          ),
          score: result.score,
          durationSeconds: result.timeUsed,
          measurements: {
            ...result.measurements,
            'mode': 'start',
            'timeUsed': result.timeUsed,
            'completed': result.completed,
          },
        );
        final nav = appNavigatorKey.currentState;
        if (nav != null) {
          nav.pushNamedAndRemoveUntil('/profile', (route) => false);
        }
      }
    } catch (_) {
    } finally {
      _busy = false;
    }
  }
}
