import 'dart:async';
import 'dart:convert';

import 'package:firebase_auth/firebase_auth.dart';
import 'package:http/http.dart' as http;

import 'api_config.dart';

class ApiException implements Exception {
  ApiException(this.message, {this.statusCode});

  final String message;
  final int? statusCode;

  @override
  String toString() => message;
}

class ApiClient {
  ApiClient({
    http.Client? httpClient,
    FirebaseAuth? auth,
    String? baseUrl,
  })  : _http = httpClient ?? http.Client(),
        _auth = auth ?? FirebaseAuth.instance,
        _forcedBaseUrl = baseUrl;

  final http.Client _http;
  final FirebaseAuth _auth;
  final String? _forcedBaseUrl;

  Future<dynamic> get(String path, {Map<String, String>? query}) {
    return _send('GET', path, query: query);
  }

  Future<dynamic> post(String path, {Map<String, dynamic>? body}) {
    return _send('POST', path, body: body);
  }

  Future<dynamic> postPublic(String path, {Map<String, dynamic>? body}) {
    return _send('POST', path, body: body, requireAuth: false);
  }

  Future<dynamic> _send(
    String method,
    String path, {
    Map<String, String>? query,
    Map<String, dynamic>? body,
    bool retried = false,
    bool requireAuth = true,
  }) async {
    final bases = _forcedBaseUrl != null
        ? [_forcedBaseUrl!]
        : ApiConfig.candidateBaseUrls;
    Object? lastError;

    for (final base in bases) {
      try {
        return await _sendTo(
          base,
          method,
          path,
          query: query,
          body: body,
          retried: retried,
          requireAuth: requireAuth,
        );
      } on ApiException catch (error) {
        if (!_isUnreachable(error)) rethrow;
        lastError = error;
      }
    }

    throw lastError ??
        ApiException(
          'Cannot reach the backend. Start FastAPI with: '
          'uvicorn app.main:app --reload --host 0.0.0.0 --port 8000',
        );
  }

  Future<dynamic> _sendTo(
    String baseUrl,
    String method,
    String path, {
    Map<String, String>? query,
    Map<String, dynamic>? body,
    bool retried = false,
    bool requireAuth = true,
  }) async {
    final uri = Uri.parse('$baseUrl$path').replace(queryParameters: query);
    final headers = <String, String>{
      'Accept': 'application/json',
      'Content-Type': 'application/json',
    };

    try {
      if (requireAuth) {
        final token = await _idToken(forceRefresh: retried);
        headers['Authorization'] = 'Bearer $token';
      }

      late http.Response response;
      if (method == 'GET') {
        response = await _http.get(uri, headers: headers).timeout(
              const Duration(seconds: 6),
            );
      } else {
        response = await _http
            .post(
              uri,
              headers: headers,
              body: body == null ? null : jsonEncode(body),
            )
            .timeout(const Duration(seconds: 6));
      }

      if (requireAuth && response.statusCode == 401 && !retried) {
        return _sendTo(
          baseUrl,
          method,
          path,
          query: query,
          body: body,
          retried: true,
          requireAuth: requireAuth,
        );
      }

      final decoded = _decode(response);
      ApiConfig.rememberWorkingUrl(baseUrl);
      return decoded;
    } on ApiException {
      rethrow;
    } on TimeoutException {
      throw ApiException(
        'The backend at $baseUrl timed out.',
        statusCode: 503,
      );
    } on http.ClientException {
      throw ApiException(
        'Cannot reach the backend at $baseUrl.',
        statusCode: 503,
      );
    } catch (error) {
      final text = error.toString().toLowerCase();
      if (text.contains('socket') ||
          text.contains('connection') ||
          text.contains('failed host lookup') ||
          text.contains('os error') ||
          text.contains('timed out')) {
        throw ApiException(
          'Cannot reach the backend at $baseUrl.',
          statusCode: 503,
        );
      }
      throw ApiException(error.toString());
    }
  }

  bool _isUnreachable(ApiException error) {
    return error.statusCode == 503;
  }

  Future<String> _idToken({required bool forceRefresh}) async {
    final user = _auth.currentUser;
    if (user == null) {
      throw ApiException('Sign in required', statusCode: 401);
    }
    final token = await user.getIdToken(forceRefresh);
    if (token == null || token.isEmpty) {
      throw ApiException('Could not get a Firebase ID token. Sign in again.');
    }
    return token;
  }

  dynamic _decode(http.Response response) {
    dynamic decoded;
    if (response.body.isNotEmpty) {
      try {
        decoded = jsonDecode(response.body);
      } catch (_) {
        decoded = response.body;
      }
    }

    if (response.statusCode >= 200 && response.statusCode < 300) {
      return decoded;
    }

    throw ApiException(
      _detail(decoded) ?? 'Request failed (${response.statusCode}).',
      statusCode: response.statusCode,
    );
  }

  String? _detail(dynamic decoded) {
    if (decoded is Map && decoded['detail'] != null) {
      final detail = decoded['detail'];
      if (detail is String) return detail;
      if (detail is List) {
        return detail.map((item) {
          if (item is Map && item['msg'] != null) return item['msg'].toString();
          return item.toString();
        }).join('\n');
      }
      return detail.toString();
    }
    if (decoded is String && decoded.trim().isNotEmpty) return decoded;
    return null;
  }
}
