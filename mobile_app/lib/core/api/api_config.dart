import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';

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
    defaultValue: '172.28.4.53',
  );

  static const _prefsKey = 'api_base_url';
  static String? _resolved;
  static Future<void>? _prefsLoad;

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
      return const ['http://127.0.0.1:9000', 'http://127.0.0.1:8000'];
    }
    return [
      'http://127.0.0.1:9000',
      'http://10.0.2.2:9000',
      'http://$_lanHost:9000',
      'http://127.0.0.1:8000',
      'http://10.0.2.2:8000',
      'http://$_lanHost:8000',
    ];
  }

  static Future<List<String>> liveCandidateBaseUrls() async {
    await ensureLoaded();
    final urls = <String>[];
    void add(String raw) {
      final url = _trimSlash(raw);
      if (url.isEmpty || urls.contains(url)) return;
      urls.add(url);
    }

    final override = _fromDefine.trim();
    if (override.isNotEmpty) {
      add(override);
      return urls;
    }

    if (_resolved != null) add(_resolved!);
    for (final item in candidateBaseUrls) {
      add(item);
    }
    return urls;
  }

  static Future<void> ensureLoaded() {
    return _prefsLoad ??= _loadPrefs();
  }

  static Future<void> _loadPrefs() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final saved = prefs.getString(_prefsKey);
      if (saved != null && saved.trim().isNotEmpty) {
        _resolved = _trimSlash(saved);
      }
    } catch (_) {}
  }

  static void rememberWorkingUrl(String url) {
    _resolved = _trimSlash(url);
    SharedPreferences.getInstance()
        .then((prefs) => prefs.setString(_prefsKey, _resolved!))
        .catchError((_) => false);
  }

  static String _trimSlash(String value) {
    return value.endsWith('/') ? value.substring(0, value.length - 1) : value;
  }
}
