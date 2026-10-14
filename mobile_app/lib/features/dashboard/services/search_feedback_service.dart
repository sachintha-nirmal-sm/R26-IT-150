import 'dart:convert';

import 'package:firebase_auth/firebase_auth.dart';
import 'package:http/http.dart' as http;

import '../../../core/backend_url.dart';

/// Fire-and-forget signals that feed the online "struggling search terms"
/// model (backend/app/rag/search_quality.py) — every call here is one
/// atomic training step on the backend. Both methods swallow all errors:
/// a logging failure must never interrupt a student's search.
class SearchFeedbackService {
  Future<void> logSearchEvent(String query, String? grade, int resultCount) async {
    try {
      final token = await FirebaseAuth.instance.currentUser?.getIdToken();
      if (token == null) return;
      await http.post(
        Uri.parse('$backendUrl/search/event'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode({
          'query': query,
          'grade': grade,
          'resultCount': resultCount,
        }),
      ).timeout(const Duration(seconds: 5));
    } catch (_) {
      // Best-effort only — never let this affect the search experience.
    }
  }

  Future<void> logSearchClick(String query, String? grade) async {
    try {
      final token = await FirebaseAuth.instance.currentUser?.getIdToken();
      if (token == null) return;
      await http.post(
        Uri.parse('$backendUrl/search/click'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode({
          'query': query,
          'grade': grade,
        }),
      ).timeout(const Duration(seconds: 5));
    } catch (_) {
      // Best-effort only — never let this affect navigation.
    }
  }
}
