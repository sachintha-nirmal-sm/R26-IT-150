import 'package:flutter/foundation.dart';

/// Platform-aware base URL for the FastAPI backend (port 9000) — the
/// Android emulator's loopback to the host machine is 10.0.2.2, not
/// localhost. Same logic as the admin-side helper
/// (lib/features/admin/admin_backend_url.dart), duplicated here rather than
/// shared across features since this is called from student-facing code.
String get backendUrl {
  if (kIsWeb) return 'http://localhost:9000';
  if (defaultTargetPlatform == TargetPlatform.android) {
    return 'http://10.0.2.2:9000';
  }
  return 'http://localhost:9000';
}
