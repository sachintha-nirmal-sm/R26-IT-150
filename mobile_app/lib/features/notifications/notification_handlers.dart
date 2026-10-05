// Wires up all three FCM app states (adapted from the physics-mobile-app
// spike's flutter_integration/notification_handlers.dart, but routed through
// this app's global Navigator key instead of go_router, since this app uses
// plain named routes — see core/app_navigator.dart and how
// features/experiments/data/lab_result_sync.dart already uses the same key):
//   - foreground: FCM doesn't show a system notification automatically, so we
//     show one ourselves with flutter_local_notifications.
//   - background tap: FirebaseMessaging.onMessageOpenedApp
//   - terminated tap: FirebaseMessaging.instance.getInitialMessage()
//
// Call `initNotificationHandlers()` once during app startup, right after
// Firebase.initializeApp(), before runApp().

import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:http/http.dart' as http;

import '../../core/app_navigator.dart';

final FlutterLocalNotificationsPlugin _localNotifications =
    FlutterLocalNotificationsPlugin();

Future<void> initNotificationHandlers() async {
  await _localNotifications.initialize(
    settings: const InitializationSettings(
      android: AndroidInitializationSettings('@mipmap/ic_launcher'),
      iOS: DarwinInitializationSettings(),
    ),
    onDidReceiveNotificationResponse: (response) {
      _routeFromData(_decodePayload(response.payload));
    },
  );

  // Foreground: FCM does not auto-display a notification, show one ourselves.
  // Native only (Android/iOS) — flutter_local_notifications_web 1.0.0 has a
  // bug in its own click-handling code (reads a non-nullable `id` field the
  // browser never provides) that crashes before anything renders. Web relies
  // on the service worker's onBackgroundMessage instead (web/firebase-messaging-sw.js),
  // which doesn't go through this plugin at all.
  if (!kIsWeb) {
    FirebaseMessaging.onMessage.listen((message) async {
      // The FCM notification.image field only auto-displays via Android's
      // native tray when the app is backgrounded — showing it ourselves via
      // flutter_local_notifications (required while foregrounded) needs the
      // image bytes explicitly, as a BigPictureStyleInformation.
      final imageUrl = message.notification?.android?.imageUrl;
      StyleInformation? style;
      if (imageUrl != null) {
        try {
          final response = await http.get(Uri.parse(imageUrl));
          if (response.statusCode == 200) {
            style = BigPictureStyleInformation(
              ByteArrayAndroidBitmap(response.bodyBytes),
              contentTitle: message.notification?.title,
              summaryText: message.notification?.body,
            );
          }
        } catch (_) {
          // Non-fatal: fall back to a plain text notification.
        }
      }

      await _localNotifications.show(
        id: message.hashCode,
        title: message.notification?.title,
        body: message.notification?.body,
        notificationDetails: NotificationDetails(
          android: AndroidNotificationDetails(
            'physics_lab_notifications',
            'PhysicsLab notifications',
            importance: Importance.high,
            priority: Priority.high,
            styleInformation: style,
          ),
          iOS: const DarwinNotificationDetails(),
        ),
        payload: _encodePayload(message.data),
      );
    });
  }

  // Background tap: app was running in the background, user tapped the
  // system notification.
  FirebaseMessaging.onMessageOpenedApp.listen((message) {
    _routeFromData(message.data);
  });

  // Terminated tap: app was launched by tapping the notification.
  final initialMessage = await FirebaseMessaging.instance.getInitialMessage();
  if (initialMessage != null) {
    _routeFromData(initialMessage.data);
  }
}

/// Routes by data['type']: a generated video notification opens the player
/// with its notificationId; anything else (or missing data) lands on the
/// profile screen (same safe landing LabResultSync already uses).
///
/// On a terminated-app cold launch, getInitialMessage() resolves before
/// MyApp's Navigator has been built, so appNavigatorKey.currentState is still
/// null the first time this runs — same timing issue LabResultSync already
/// works around with a delay. Retry briefly instead of silently giving up.
Future<void> _routeFromData(Map<String, dynamic> data) async {
  for (var attempt = 0; attempt < 20; attempt++) {
    final nav = appNavigatorKey.currentState;
    if (nav != null) {
      if (data['type'] == 'motivation_video' && data['notificationId'] != null) {
        nav.pushNamed('/video-notification',
            arguments: {'notificationId': data['notificationId']});
      } else {
        nav.pushNamed('/profile');
      }
      return;
    }
    await Future.delayed(const Duration(milliseconds: 100));
  }
}

String _encodePayload(Map<String, dynamic> data) =>
    data.entries.map((e) => '${e.key}=${e.value}').join('&');

Map<String, dynamic> _decodePayload(String? payload) {
  if (payload == null || payload.isEmpty) return {};
  return {
    for (final pair in payload.split('&'))
      if (pair.contains('='))
        pair.split('=').first: pair.split('=').skip(1).join('=')
  };
}
