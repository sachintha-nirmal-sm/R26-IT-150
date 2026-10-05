import 'package:flutter/foundation.dart';

/// The Android emulator's loopback to the host machine is 10.0.2.2, not
/// localhost — localhost inside the emulator refers to the emulator itself.
/// Every admin screen hardcodes this backend URL (no shared ApiClient here,
/// unlike the rest of the app), so this one helper keeps the platform check
/// in a single place instead of repeating it in each screen.
String get adminBackendUrl {
  if (kIsWeb) return 'http://localhost:9000';
  if (defaultTargetPlatform == TargetPlatform.android) {
    return 'http://10.0.2.2:9000';
  }
  return 'http://localhost:9000';
}
