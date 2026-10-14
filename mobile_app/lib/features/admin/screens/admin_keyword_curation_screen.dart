import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:http/http.dart' as http;

import '../admin_backend_url.dart';

/// Post-hoc moderation of search_keyword_index: new PDF uploads already
/// auto-approve their own good keywords on the spot (see
/// backend/app/rag/keyword_aggregate.py), so this screen's job is reviewing
/// what's already live and demoting anything that slipped through — not
/// gating keywords before they go live.
class AdminKeywordCurationScreen extends StatefulWidget {
  const AdminKeywordCurationScreen({super.key});

  @override
  State<AdminKeywordCurationScreen> createState() =>
      _AdminKeywordCurationScreenState();
}

class _AdminKeywordCurationScreenState
    extends State<AdminKeywordCurationScreen> with SingleTickerProviderStateMixin {
  static final _backendUrl = adminBackendUrl;
  static const _primaryBlue = Color(0xFF1A3CBA);

  static const _statuses = ['auto_approved', 'approved', 'rejected'];
  static const _tabLabels = ['Auto-Approved', 'Approved', 'Rejected'];

  late TabController _tabController;

  Map<String, List<Map<String, dynamic>>> _byStatus = {
    'auto_approved': [],
    'approved': [],
    'rejected': [],
  };
  bool _loading = true;
  String? _error;
  bool _sweeping = false;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: _statuses.length, vsync: this);
    _loadKeywords();
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<String?> _authToken() async =>
      await FirebaseAuth.instance.currentUser?.getIdToken();

  Future<void> _loadKeywords() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final token = await _authToken();
      final response = await http.get(
        Uri.parse('$_backendUrl/admin/search/keywords'),
        headers: {'Authorization': 'Bearer $token'},
      ).timeout(const Duration(seconds: 15));

      if (response.statusCode != 200) {
        throw Exception('Backend returned ${response.statusCode}');
      }

      final data = (jsonDecode(response.body) as List).cast<Map<String, dynamic>>();
      final grouped = <String, List<Map<String, dynamic>>>{
        'auto_approved': [],
        'approved': [],
        'rejected': [],
      };
      for (final row in data) {
        final status = row['status'] as String? ?? 'auto_approved';
        (grouped[status] ??= []).add(row);
      }
      if (!mounted) return;
      setState(() {
        _byStatus = grouped;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _error = 'Could not load keywords: $e';
        _loading = false;
      });
    }
  }

  Future<void> _setStatus(String keyword, String status) async {
    try {
      final token = await _authToken();
      final response = await http.patch(
        Uri.parse('$_backendUrl/admin/search/keywords/$keyword'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode({'status': status}),
      ).timeout(const Duration(seconds: 10));

      if (response.statusCode != 200) {
        throw Exception('Backend returned ${response.statusCode}');
      }
      await _loadKeywords();
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('"$keyword" marked $status.')),
      );
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to update "$keyword": $e')),
      );
    }
  }

  Future<void> _runSweep() async {
    setState(() => _sweeping = true);
    try {
      final token = await _authToken();
      final response = await http.post(
        Uri.parse('$_backendUrl/admin/search/keywords/sweep'),
        headers: {'Authorization': 'Bearer $token'},
      ).timeout(const Duration(seconds: 30));

      if (response.statusCode != 200) {
        throw Exception('Backend returned ${response.statusCode}');
      }
      final body = jsonDecode(response.body) as Map<String, dynamic>;
      await _loadKeywords();
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Sweep complete — ${body['keywordsIndexed']} keywords indexed.'),
        ),
      );
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Sweep failed: $e')),
      );
    } finally {
      if (mounted) setState(() => _sweeping = false);
    }
  }

  Future<void> _exportCsv() async {
    try {
      final token = await _authToken();
      final response = await http.get(
        Uri.parse('$_backendUrl/admin/search/keywords/export.csv'),
        headers: {'Authorization': 'Bearer $token'},
      ).timeout(const Duration(seconds: 20));

      if (response.statusCode != 200) {
        throw Exception('Backend returned ${response.statusCode}');
      }
      if (!mounted) return;
      await showDialog<void>(
        context: context,
        builder: (dialogContext) => AlertDialog(
          title: const Text('Keyword CSV'),
          content: SizedBox(
            width: double.maxFinite,
            height: 400,
            child: SingleChildScrollView(
              child: SelectableText(
                response.body,
                style: const TextStyle(fontFamily: 'monospace', fontSize: 12),
              ),
            ),
          ),
          actions: [
            TextButton(
              onPressed: () {
                Clipboard.setData(ClipboardData(text: response.body));
                ScaffoldMessenger.of(dialogContext).showSnackBar(
                  const SnackBar(content: Text('CSV copied to clipboard.')),
                );
              },
              child: const Text('Copy to Clipboard'),
            ),
            TextButton(
              onPressed: () => Navigator.of(dialogContext).pop(),
              child: const Text('Close'),
            ),
          ],
        ),
      );
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Export failed: $e')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Keyword Curation'),
        backgroundColor: _primaryBlue,
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: _sweeping
                ? const SizedBox(
                    width: 20,
                    height: 20,
                    child: CircularProgressIndicator(
                      strokeWidth: 2,
                      color: Colors.white,
                    ),
                  )
                : const Icon(Icons.sync),
            tooltip: 'Re-run sweep',
            onPressed: _sweeping ? null : _runSweep,
          ),
          IconButton(
            icon: const Icon(Icons.download),
            tooltip: 'Export CSV',
            onPressed: _exportCsv,
          ),
        ],
        bottom: TabBar(
          controller: _tabController,
          indicatorColor: Colors.white,
          labelColor: Colors.white,
          unselectedLabelColor: Colors.white70,
          tabs: List.generate(_statuses.length, (i) {
            final count = _byStatus[_statuses[i]]?.length ?? 0;
            return Tab(text: '${_tabLabels[i]} ($count)');
          }),
        ),
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
              : RefreshIndicator(
                  onRefresh: _loadKeywords,
                  child: TabBarView(
                    controller: _tabController,
                    children: _statuses.map(_buildList).toList(),
                  ),
                ),
    );
  }

  Widget _buildList(String status) {
    final rows = _byStatus[status] ?? [];
    if (rows.isEmpty) {
      return const Center(child: Text('No keywords here.'));
    }
    return ListView.builder(
      padding: const EdgeInsets.all(12),
      itemCount: rows.length,
      itemBuilder: (context, index) {
        final row = rows[index];
        final keyword = (row['keyword'] ?? row['id'] ?? '') as String;
        final materialCount = row['materialCount'] ?? 0;
        final isPhysicsConcept = row['isPhysicsConcept'] == true;
        final aiSelected = row['aiSelected'] == true;

        return Card(
          margin: const EdgeInsets.only(bottom: 8),
          child: ListTile(
            title: Text(keyword, style: const TextStyle(fontWeight: FontWeight.bold)),
            subtitle: Padding(
              padding: const EdgeInsets.only(top: 4),
              child: Wrap(
                spacing: 6,
                runSpacing: 4,
                crossAxisAlignment: WrapCrossAlignment.center,
                children: [
                  Text('$materialCount material${materialCount == 1 ? '' : 's'}'),
                  if (isPhysicsConcept) _tag('Physics concept', Colors.blue),
                  if (aiSelected) _tag('AI-picked', Colors.purple),
                ],
              ),
            ),
            trailing: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                if (status != 'approved')
                  IconButton(
                    icon: const Icon(Icons.check_circle_outline, color: Colors.green),
                    tooltip: 'Approve',
                    onPressed: () => _setStatus(keyword, 'approved'),
                  ),
                if (status != 'rejected')
                  IconButton(
                    icon: const Icon(Icons.cancel_outlined, color: Colors.red),
                    tooltip: 'Reject',
                    onPressed: () => _setStatus(keyword, 'rejected'),
                  ),
              ],
            ),
          ),
        );
      },
    );
  }

  Widget _tag(String label, Color color) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(4),
        border: Border.all(color: color.withOpacity(0.3)),
      ),
      child: Text(
        label,
        style: TextStyle(fontSize: 10, color: color, fontWeight: FontWeight.w600),
      ),
    );
  }
}
