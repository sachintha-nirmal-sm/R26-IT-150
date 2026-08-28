import 'dart:io' show Platform;

class ApiConfig {
  /// Android emulator reaches the host machine at 10.0.2.2.
  static String get baseUrl {
    if (Platform.isAndroid) {
      return 'http://10.0.2.2:8000';
    }
    return 'http://127.0.0.1:8000';
  }
}
