import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:http/http.dart' as http;

import '../admin_backend_url.dart';

/// Reads the online "struggling search terms" model (search_quality_index,
/// built up live from real student search/click traffic — see
/// backend/app/rag/search_quality.py) and shows admins which search terms
/// are likely content gaps: searched often but rarely lead to a click, or
/// searched and never return any results at all.
class AdminStrugglingSearchesScreen extends StatefulWidget {
  const AdminStrugglingSearchesScreen({super.key});

  @override
  State<AdminStrugglingSearchesScreen> createState() =>
      _AdminStrugglingSearchesScreenState();
}

class _AdminStrugglingSearchesScreenState
    extends State<AdminStrugglingSearchesScreen> {
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
        Uri.parse('$_backendUrl/admin/search/struggling-terms'),
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
        _error = 'Could not load search insights: $e';
        _loading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Search Insights'),
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
                          'No struggling search terms right now — every '
                          'well-searched term is getting clicked.',
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
    final term = (row['sampleQuery'] ?? row['term'] ?? '') as String;
    final searchCount = row['searchCount'] ?? 0;
    final clickCount = row['clickCount'] ?? 0;
    final ctr = ((row['ctr'] ?? 0.0) as num).toDouble();
    final zeroResultRate = ((row['zeroResultRate'] ?? 0.0) as num).toDouble();
    final grades = (row['grades'] as List?)?.cast<String>() ?? [];
    final isHardGap = zeroResultRate >= 0.5;

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
                    '"$term"',
                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                  ),
                ),
                _tag(
                  isHardGap ? 'No results' : 'Low click-through',
                  isHardGap ? Colors.red : Colors.orange,
                ),
              ],
            ),
            const SizedBox(height: 8),
            Wrap(
              spacing: 12,
              runSpacing: 4,
              children: [
                Text('$searchCount searches'),
                Text('$clickCount clicks'),
                Text('${(ctr * 100).toStringAsFixed(0)}% click-through'),
                if (zeroResultRate > 0)
                  Text('${(zeroResultRate * 100).toStringAsFixed(0)}% returned nothing'),
              ],
            ),
            if (grades.isNotEmpty) ...[
              const SizedBox(height: 6),
              Text(
                'Searched by: ${grades.join(', ')}',
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
