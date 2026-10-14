// Request notification permission. Required on Android 13+ (POST_NOTIFICATIONS
// runtime permission) and iOS (provisional or full authorization).

import 'package:firebase_messaging/firebase_messaging.dart';

Future<bool> requestNotificationPermission() async {
  final settings = await FirebaseMessaging.instance.requestPermission(
    alert: true,
    badge: true,
    sound: true,
  );

  return settings.authorizationStatus == AuthorizationStatus.authorized ||
      settings.authorizationStatus == AuthorizationStatus.provisional;
}
