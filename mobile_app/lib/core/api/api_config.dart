import 'package:flutter/foundation.dart';

/// FastAPI base URL for the practicals backend.
///
/// Override at build time:
///   flutter run --dart-define=API_BASE_URL=http://192.168.128.196:8000
///
/// Android tries several hosts so a USB phone, emulator, and same-Wi-Fi
/// device can all reach the PC without a rebuild:
///   127.0.0.1  — physical phone with `adb reverse tcp:8000 tcp:8000`
///   10.0.2.2   — Android emulator
///   LAN IP     — physical phone on the same Wi-Fi
class ApiConfig {
  static const _fromDefine = String.fromEnvironment('API_BASE_URL');
  static const _lanHost = String.fromEnvironment(
    'API_HOST',
    defaultValue: '172.28.10.87',
  );

  static String? _resolved;

  static String get baseUrl => _resolved ?? candidateBaseUrls.first;

  static List<String> get candidateBaseUrls {
    final override = _fromDefine.trim();
    if (override.isNotEmpty) {
      return [_trimSlash(override)];
    }
    if (kIsWeb ||
        defaultTargetPlatform == TargetPlatform.iOS ||
        defaultTargetPlatform == TargetPlatform.windows ||
        defaultTargetPlatform == TargetPlatform.macOS ||
        defaultTargetPlatform == TargetPlatform.linux) {
      return const ['http://127.0.0.1:8000'];
    }
    return [
      'http://127.0.0.1:8000',
      'http://10.0.2.2:8000',
      'http://$_lanHost:8000',
    ];
  }

  static void rememberWorkingUrl(String url) {
    _resolved = _trimSlash(url);
  }

  static String _trimSlash(String value) {
    return value.endsWith('/') ? value.substring(0, value.length - 1) : value;
  }
}
