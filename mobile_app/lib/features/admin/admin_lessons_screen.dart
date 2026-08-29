import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:http/http.dart' as http;
import 'dart:async';
import 'dart:convert';
import 'admin_lesson_detail_screen.dart';

class AdminLessonsScreen extends StatefulWidget {
  const AdminLessonsScreen({super.key});

  @override
  State<AdminLessonsScreen> createState() => _AdminLessonsScreenState();
}

class _AdminLessonsScreenState extends State<AdminLessonsScreen> {
  static final String _backendUrl =
      'http://localhost:9000';

  String? _selectedGrade;

  final List<Map<String, dynamic>> _grades = [
    {
      'label': 'Grade 9',
      'subtitle': 'Core Concepts & Foundations',
      'icon': Icons.looks_one,
      'color': Colors.blue,
    },
    {
      'label': 'Grade 10',
      'subtitle': 'Motion, Forces & Energy',
      'icon': Icons.looks_two,
      'color': Colors.green,
    },
    {
      'label': 'Grade 11',
      'subtitle': 'Waves, Optics & Electronics',
      'icon': Icons.looks_3,
      'color': Colors.orange,
    },
  ];

  @override
  Widget build(BuildContext context) {
    if (_selectedGrade == null) {
      return _buildGradeSelector();
    }
    return _buildLessonsForGrade(_selectedGrade!);
  }

  Widget _buildGradeSelector() {
    return Padding(
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Lessons by Grade',
              style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold)),
          const SizedBox(height: 6),
          Text('Select a grade to manage its lessons.',
              style: TextStyle(color: Colors.grey.shade600, fontSize: 13)),
          const SizedBox(height: 24),
          ..._grades.map((g) => _buildGradeCard(g)),
        ],
      ),
    );
  }

  Widget _buildGradeCard(Map<String, dynamic> g) {
    final color = g['color'] as Color;
    return GestureDetector(
      onTap: () => setState(() => _selectedGrade = g['label']),
      child: Container(
        margin: const EdgeInsets.only(bottom: 16),
        padding: const EdgeInsets.all(20),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(16),
          boxShadow: [BoxShadow(color: Colors.black.withOpacity(0.06), blurRadius: 10)],
        ),
        child: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(14),
              decoration: BoxDecoration(
                color: color.withOpacity(0.1),
                borderRadius: BorderRadius.circular(14),
              ),
              child: Icon(g['icon'] as IconData, color: color, size: 28),
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(g['label'],
                      style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                  const SizedBox(height: 4),
                  Text(g['subtitle'],
                      style: TextStyle(color: Colors.grey.shade600, fontSize: 13)),
                ],
              ),
            ),
            Icon(Icons.chevron_right, color: Colors.grey.shade400),
          ],
        ),
      ),
    );
  }

  Widget _buildLessonsForGrade(String grade) {
    return _GradeLessonsView(
      grade: grade,
      backendUrl: _backendUrl,
      onBack: () => setState(() => _selectedGrade = null),
    );
  }
}

class _GradeLessonsView extends StatefulWidget {
  final String grade;
  final String backendUrl;
  final VoidCallback onBack;

  const _GradeLessonsView({
    required this.grade,
    required this.backendUrl,
    required this.onBack,
  });

  @override
  State<_GradeLessonsView> createState() => _GradeLessonsViewState();
}

class _GradeLessonsViewState extends State<_GradeLessonsView> {
  // Cache keyed by grade so switching grades is also instant
  static final Map<String, List<Map<String, dynamic>>> _cache = {};

  List<Map<String, dynamic>> _lessons = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    // Show cached data instantly, then refresh in background
    if (_cache.containsKey(widget.grade)) {
      _lessons = _cache[widget.grade]!;
      _isLoading = false;
    }
    _loadLessons(silent: _cache.containsKey(widget.grade));
  }

  Future<String?> _getToken() async =>
      await FirebaseAuth.instance.currentUser?.getIdToken();

  Future<void> _loadLessons({bool silent = false}) async {
    if (!silent) setState(() => _isLoading = true);
    try {
      final token = await _getToken().timeout(const Duration(seconds: 10));
      final response = await http.get(
        Uri.parse('${widget.backendUrl}/admin/lessons'),
        headers: {'Authorization': 'Bearer $token'},
      ).timeout(const Duration(seconds: 15));
      if (response.statusCode == 200) {
        final data = jsonDecode(response.body) as List;
        final all = data.cast<Map<String, dynamic>>();
        final gradeNum = int.tryParse(widget.grade.replaceAll(RegExp(r'[^0-9]'), '')) ?? 10;
        final filtered = all.where((l) => l['grade'] == gradeNum).toList();
        _cache[widget.grade] = filtered;
        if (mounted) setState(() => _lessons = filtered);
      } else {
        if (!silent) _showError('Backend error ${response.statusCode}: ${response.body}');
      }
    } on TimeoutException {
      if (!silent) _showError('Timed out "” backend not reachable at ${widget.backendUrl}');
    } catch (e) {
      if (!silent) _showError('Error: $e');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Future<void> _createLesson(Map<String, dynamic> data) async {
    // Optimistic: add a placeholder immediately so the UI updates before the server responds
    final placeholder = {...data, 'id': '__pending__'};
    setState(() => _lessons = [..._lessons, placeholder]);

    try {
      final token = await _getToken();
      final response = await http.post(
        Uri.parse('${widget.backendUrl}/admin/lessons'),
        headers: {'Authorization': 'Bearer $token', 'Content-Type': 'application/json'},
        body: jsonEncode(data),
      );
      if (response.statusCode == 200 || response.statusCode == 201) {
        final newLesson = jsonDecode(response.body) as Map<String, dynamic>;
        // Replace placeholder with real server data
        setState(() => _lessons = [
          ..._lessons.where((l) => l['id'] != '__pending__'),
          newLesson,
        ]);
        _cache.remove(widget.grade);
        if (mounted) _showSuccess('Lesson created!');
      } else {
        // Rollback on failure
        setState(() => _lessons = _lessons.where((l) => l['id'] != '__pending__').toList());
        if (mounted) _showError('Failed: ${response.body}');
      }
    } catch (e) {
      setState(() => _lessons = _lessons.where((l) => l['id'] != '__pending__').toList());
      if (mounted) _showError('Error: $e');
    }
  }

  Future<void> _deleteLesson(String id) async {
    // Optimistic: remove immediately, restore if server fails
    final removed = _lessons.firstWhere((l) => l['id'] == id, orElse: () => {});
    setState(() => _lessons = _lessons.where((l) => l['id'] != id).toList());

    try {
      final token = await _getToken();
      final response = await http.delete(
        Uri.parse('${widget.backendUrl}/admin/lessons/$id'),
        headers: {'Authorization': 'Bearer $token'},
      );
      if (response.statusCode == 200 || response.statusCode == 204) {
        _cache.remove(widget.grade);
        if (mounted) _showSuccess('Lesson deleted.');
      } else {
        // Rollback
        if (removed.isNotEmpty) setState(() => _lessons = [..._lessons, removed]);
        if (mounted) _showError('Failed to delete lesson.');
      }
    } catch (_) {
      if (removed.isNotEmpty) setState(() => _lessons = [..._lessons, removed]);
      if (mounted) _showError('Error deleting lesson.');
    }
  }

  void _showCreateDialog() {
    final titleCtrl = TextEditingController();

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
          borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => Padding(
        padding: EdgeInsets.only(
          left: 20, right: 20, top: 20,
          bottom: MediaQuery.of(ctx).viewInsets.bottom + 20,
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(children: [
              Container(
                width: 4, height: 24,
                decoration: BoxDecoration(
                  color: const Color(0xFF1A3CBA),
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
              const SizedBox(width: 10),
              Text('New Lesson "” ${widget.grade}',
                  style: const TextStyle(fontSize: 17, fontWeight: FontWeight.bold)),
            ]),
            const SizedBox(height: 16),
            _field(titleCtrl, 'Lesson Title'),
            const SizedBox(height: 20),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: () {
                  final title = titleCtrl.text.trim();
                  if (title.isEmpty) return;
                  // Auto-generate lessonTag from title
                  final tag = title.toLowerCase().replaceAll(RegExp(r'[^a-z0-9]+'), '-');
                  final gradeNum = int.tryParse(widget.grade.replaceAll(RegExp(r'[^0-9]'), '')) ?? 10;
                  Navigator.pop(ctx);
                  _createLesson({
                    'title': title,
                    'subject': 'Physics',
                    'grade': gradeNum,
                    'lessonTag': tag,
                    'description': '',
                    'order': _lessons.length + 1,
                    'status': 'published',
                  });
                },
                icon: const Icon(Icons.add, color: Colors.white),
                label: const Text('Create Lesson', style: TextStyle(color: Colors.white)),
                style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xFF1A3CBA),
                  padding: const EdgeInsets.symmetric(vertical: 14),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _showError(String msg) => ScaffoldMessenger.of(context)
      .showSnackBar(SnackBar(content: Text(msg), backgroundColor: Colors.red));
  void _showSuccess(String msg) => ScaffoldMessenger.of(context)
      .showSnackBar(SnackBar(content: Text(msg), backgroundColor: Colors.green));

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4F6FB),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Color(0xFF1A3CBA)),
          onPressed: widget.onBack,
        ),
        title: Text(widget.grade,
            style: const TextStyle(fontWeight: FontWeight.bold, color: Color(0xFF1A1A2E))),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh, color: Color(0xFF1A3CBA)),
            onPressed: _loadLessons,
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton.extended(
        heroTag: null,
        onPressed: _showCreateDialog,
        backgroundColor: const Color(0xFF1A3CBA),
        icon: const Icon(Icons.add, color: Colors.white),
        label: const Text('Add Lesson', style: TextStyle(color: Colors.white)),
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _lessons.isEmpty
              ? Center(
                  child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
                    Icon(Icons.menu_book_outlined, size: 64, color: Colors.grey.shade300),
                    const SizedBox(height: 12),
                    Text('No lessons yet for ${widget.grade}.',
                        style: TextStyle(color: Colors.grey.shade500)),
                    const SizedBox(height: 6),
                    const Text('Tap + Add Lesson to get started.',
                        style: TextStyle(color: Colors.grey)),
                  ]),
                )
              : RefreshIndicator(
                  onRefresh: _loadLessons,
                  child: ListView.builder(
                    padding: const EdgeInsets.fromLTRB(16, 16, 16, 100),
                    itemCount: _lessons.length,
                    itemBuilder: (ctx, i) {
                      final lesson = _lessons[i];
                      return GestureDetector(
                        onTap: () => Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (_) => AdminLessonDetailScreen(
                              lessonId: lesson['id'] ?? '',
                              lessonTitle: lesson['title'] ?? '-',
                              grade: widget.grade,
                            ),
                          ),
                        ),
                        child: Card(
                          margin: const EdgeInsets.only(bottom: 12),
                          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                          child: Padding(
                            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                            child: Row(children: [
                              Container(
                                width: 42, height: 42,
                                decoration: BoxDecoration(
                                  color: const Color(0xFF1A3CBA).withOpacity(0.1),
                                  borderRadius: BorderRadius.circular(10),
                                ),
                                child: Center(
                                  child: Text('${i + 1}',
                                      style: const TextStyle(
                                          color: Color(0xFF1A3CBA),
                                          fontWeight: FontWeight.bold, fontSize: 16)),
                                ),
                              ),
                              const SizedBox(width: 14),
                              Expanded(
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Text(lesson['title'] ?? '-',
                                        style: const TextStyle(fontWeight: FontWeight.bold)),
                                    Text(lesson['lessonTag'] ?? '',
                                        style: TextStyle(color: Colors.grey.shade500, fontSize: 12)),
                                  ],
                                ),
                              ),
                              IconButton(
                                icon: const Icon(Icons.delete_outline, color: Colors.red),
                                onPressed: () => showDialog(
                                  context: context,
                                  builder: (ctx) => AlertDialog(
                                    title: const Text('Delete Lesson'),
                                    content: Text('Delete "${lesson['title']}"?'),
                                    actions: [
                                      TextButton(onPressed: () => Navigator.pop(ctx),
                                          child: const Text('Cancel')),
                                      TextButton(
                                        onPressed: () {
                                          Navigator.pop(ctx);
                                          _deleteLesson(lesson['id']);
                                        },
                                        child: const Text('Delete',
                                            style: TextStyle(color: Colors.red)),
                                      ),
                                    ],
                                  ),
                                ),
                              ),
                            ]),
                          ),
                        ),
                      );
                    },
                  ),
                ),
    );
  }

  Widget _field(TextEditingController ctrl, String label,
      {int maxLines = 1, TextInputType keyboardType = TextInputType.text}) {
    return TextField(
      controller: ctrl,
      maxLines: maxLines,
      keyboardType: keyboardType,
      decoration: InputDecoration(
        labelText: label,
        border: const OutlineInputBorder(),
        contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
      ),
    );
  }
}


