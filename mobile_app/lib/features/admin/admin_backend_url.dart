import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;

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

/// POST with one automatic retry on a timeout or network-level failure —
/// a transient network hiccup (e.g. this dev machine's occasionally flaky
/// connection) shouldn't surface as a hard error on a single admin button
/// tap when a second attempt a moment later would just work. Does NOT
/// retry on a real HTTP error response (4xx/5xx with a body) — only on
/// the request failing to complete at all.
Future<http.Response> adminHttpPost(
  Uri uri, {
  required Map<String, String> headers,
  String? body,
  // 25s, not 15s: sending a notification kicks off a genuinely CPU/IO
  // heavy background job (Groq script + TTS + FFmpeg video) on the same
  // single-process dev backend, which can briefly slow down any other
  // concurrent admin request (e.g. a study-plan regenerate fired right
  // after) — this margin covers that overlap instead of cutting it off.
  Duration timeout = const Duration(seconds: 25),
}) async {
  Object? lastError;
  for (var attempt = 0; attempt < 2; attempt++) {
    try {
      return await http
          .post(uri, headers: headers, body: body)
          .timeout(timeout);
    } on TimeoutException catch (e) {
      lastError = e;
    } on http.ClientException catch (e) {
      lastError = e;
    }
    if (attempt == 0) await Future.delayed(const Duration(milliseconds: 400));
  }
  throw lastError!;
}
