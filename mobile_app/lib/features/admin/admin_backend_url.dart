import 'package:flutter/foundation.dart';

/// The Android emulator's loopback to the host machine is 10.0.2.2, not
/// localhost — localhost inside the emulator refers to the emulator itself.
/// Every admin screen on this branch was hardcoding 'http://localhost:9000'
/// directly (confirmed via grep across admin_materials_screen.dart,
/// admin_lessons_screen.dart, admin_quiz_generator_screen.dart,
/// admin_analytics_screen.dart), which works on web/desktop but silently
/// fails every admin HTTP call on the Android emulator. This one helper
/// keeps the platform check in a single place instead of repeating it.
String get adminBackendUrl {
  if (kIsWeb) return 'http://localhost:9000';
  if (defaultTargetPlatform == TargetPlatform.android) {
    return 'http://10.0.2.2:9000';
  }
  return 'http://localhost:9000';
}
