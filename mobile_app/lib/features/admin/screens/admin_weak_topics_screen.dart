import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:http/http.dart' as http;

import '../admin_backend_url.dart';

/// Reads the online "weak topics by quiz performance" model
/// (topic_performance_index, built up live from real student quiz
/// submissions — see backend/app/rag/topic_performance.py) and shows
/// admins which lessons/topics students are consistently scoring poorly
/// on.
class AdminWeakTopicsScreen extends StatefulWidget {
  const AdminWeakTopicsScreen({super.key});

  @override
  State<AdminWeakTopicsScreen> createState() => _AdminWeakTopicsScreenState();
}

class _AdminWeakTopicsScreenState extends State<AdminWeakTopicsScreen> {
  static final _backendUrl = adminBackendUrl;
  static const _primaryBlue = Color(0xFF1A3CBA);

  bool _loading = true;
  String? _error;
  List<Map<String, dynamic>> _rows = [];

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final token = await FirebaseAuth.instance.currentUser?.getIdToken();
      final response = await http.get(
        Uri.parse('$_backendUrl/admin/topics/weak-performance'),
        headers: {'Authorization': 'Bearer $token'},
      ).timeout(const Duration(seconds: 15));

      if (response.statusCode != 200) {
        throw Exception('Backend returned ${response.statusCode}');
      }

      final data = (jsonDecode(response.body) as List).cast<Map<String, dynamic>>();
      if (!mounted) return;
      setState(() {
        _rows = data;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _error = 'Could not load weak topics: $e';
        _loading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Weak Topics'),
        backgroundColor: _primaryBlue,
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Refresh',
            onPressed: _load,
          ),
        ],
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? Center(
                  child: Padding(
                    padding: const EdgeInsets.all(24),
                    child: Text(_error!, textAlign: TextAlign.center),
                  ),
                )
              : _rows.isEmpty
                  ? const Center(
                      child: Padding(
                        padding: EdgeInsets.all(24),
                        child: Text(
                          'No weak topics right now — every well-attempted '
                          'topic is averaging a passing score.',
                          textAlign: TextAlign.center,
                        ),
                      ),
                    )
                  : RefreshIndicator(
                      onRefresh: _load,
                      child: ListView.builder(
                        padding: const EdgeInsets.all(12),
                        itemCount: _rows.length,
                        itemBuilder: (context, index) => _buildCard(_rows[index]),
                      ),
                    ),
    );
  }

  Widget _buildCard(Map<String, dynamic> row) {
    final lessonTitle = (row['lessonTitle'] ?? row['lessonId'] ?? '') as String;
    final attemptCount = row['attemptCount'] ?? 0;
    final averageScore = ((row['averageScore'] ?? 0.0) as num).toDouble();
    final lowScoreRate = ((row['lowScoreRate'] ?? 0.0) as num).toDouble();
    final grades = (row['grades'] as List?)?.cast<String>() ?? [];
    final isSevere = averageScore < 50;

    return Card(
      margin: const EdgeInsets.only(bottom: 10),
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    lessonTitle,
                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                  ),
                ),
                _tag(
                  isSevere ? 'Severe' : 'Below passing',
                  isSevere ? Colors.red : Colors.orange,
                ),
              ],
            ),
            const SizedBox(height: 8),
            Wrap(
              spacing: 12,
              runSpacing: 4,
              children: [
                Text('$attemptCount attempts'),
                Text('${averageScore.toStringAsFixed(0)}% average score'),
                Text('${(lowScoreRate * 100).toStringAsFixed(0)}% below passing'),
              ],
            ),
            if (grades.isNotEmpty) ...[
              const SizedBox(height: 6),
              Text(
                'Grades: ${grades.join(', ')}',
                style: TextStyle(color: Colors.grey.shade600, fontSize: 12),
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _tag(String label, Color color) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(6),
        border: Border.all(color: color.withOpacity(0.3)),
      ),
      child: Text(
        label,
        style: TextStyle(fontSize: 11, color: color, fontWeight: FontWeight.w600),
      ),
    );
  }
}
