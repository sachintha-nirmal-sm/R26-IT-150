import 'package:firebase_auth/firebase_auth.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../notification_permission.dart';
import 'notifications_repository.dart';

/// Requests notification permission (once per device) and keeps the
/// backend's copy of this device's FCM token up to date for whichever
/// student is currently signed in. Call `syncToken()` whenever a student
/// becomes signed in (login, signup, or app cold-start with an existing
/// session).
class FcmTokenSync {
  FcmTokenSync._();

  static const _permissionAskedKey = 'notif_permission_requested';

  static bool _listening = false;

  static Future<void> syncToken() async {
    try {
      final prefs = await SharedPreferences.getInstance();

      if (prefs.getBool(_permissionAskedKey) != true) {
        await requestNotificationPermission();
        await prefs.setBool(_permissionAskedKey, true);
      }

      final token = await FirebaseMessaging.instance.getToken();
      if (token != null) {
        await _registerIfNew(token, prefs);
      }

      if (!_listening) {
        _listening = true;
        FirebaseMessaging.instance.onTokenRefresh.listen((newToken) async {
          final p = await SharedPreferences.getInstance();
          await _registerIfNew(newToken, p);
        });
      }
    } catch (_) {
      // Non-fatal: notifications are a nice-to-have, never block app usage.
    }
  }

  /// The FCM token is tied to this browser/device install, not to whichever
  /// student is signed in — so "already synced" must be scoped per-uid, or a
  /// second student logging into the same device/browser would silently
  /// never get registered (the token value alone looks unchanged).
  static Future<void> _registerIfNew(
    String token,
    SharedPreferences prefs,
  ) async {
    final uid = FirebaseAuth.instance.currentUser?.uid;
    if (uid == null) return;
    final key = 'fcm_token_synced_$uid';
    if (prefs.getString(key) == token) return;
    await NotificationsRepository().registerToken(token, _platform());
    await prefs.setString(key, token);
  }

  static String _platform() {
    if (kIsWeb) return 'web';
    return defaultTargetPlatform == TargetPlatform.iOS ? 'ios' : 'android';
  }
}
