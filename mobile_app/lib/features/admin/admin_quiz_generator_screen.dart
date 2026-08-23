import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';

class AdminQuizGeneratorScreen extends StatefulWidget {
  const AdminQuizGeneratorScreen({super.key});

  @override
  State<AdminQuizGeneratorScreen> createState() => _AdminQuizGeneratorScreenState();
}

class _AdminQuizGeneratorScreenState extends State<AdminQuizGeneratorScreen>
    with SingleTickerProviderStateMixin {
  static final String _backendUrl =
      'http://localhost:9000';
  late TabController _tabController;

  List<Map<String, dynamic>> _lessons = [];
  String? _selectedLessonId;
  String _selectedLlm = 'gpt-4';
  String _selectedGrade = 'Grade 9';
  bool _isGenerating = false;
  String _log = '';

  final List<String> _llmOptions = ['gpt-4', 'gpt-3.5-turbo', 'deepseek', 'claude'];

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
    _loadLessons();
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<String?> _getToken() async =>
      await FirebaseAuth.instance.currentUser?.getIdToken();

  Future<void> _loadLessons() async {
    try {
      final token = await _getToken();
      final response = await http.get(
        Uri.parse('$_backendUrl/admin/lessons'),
        headers: {'Authorization': 'Bearer $token'},
      );
      if (response.statusCode == 200) {
        final data = jsonDecode(response.body) as List;
        setState(() => _lessons = data.cast<Map<String, dynamic>>());
      }
    } catch (_) {}
  }

  Future<void> _generate(String endpoint, Map<String, dynamic> body) async {
    setState(() { _isGenerating = true; _log = ''; });
    _appendLog('Starting generation...');
    _appendLog('Model: $_selectedLlm');
    try {
      final token = await _getToken();
      final response = await http.post(
        Uri.parse('$_backendUrl$endpoint'),
        headers: {'Authorization': 'Bearer $token', 'Content-Type': 'application/json'},
        body: jsonEncode(body),
      );
      if (response.statusCode == 200 || response.statusCode == 202) {
        _appendLog('âœ… Generation started successfully!');
      } else {
        _appendLog('âŒ Error ${response.statusCode}: ${response.body}');
      }
    } catch (e) {
      _appendLog('âŒ Error: $e');
    } finally {
      setState(() => _isGenerating = false);
    }
  }

  void _appendLog(String msg) => setState(() =>
      _log += '${DateTime.now().toString().substring(11, 19)} "” $msg\n');

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        TabBar(
          controller: _tabController,
          tabs: const [
            Tab(icon: Icon(Icons.menu_book), text: 'Lesson Quiz'),
            Tab(icon: Icon(Icons.assignment), text: 'Final Quiz'),
          ],
        ),
        Expanded(
          child: TabBarView(
            controller: _tabController,
            children: [_buildLessonTab(), _buildFinalTab()],
          ),
        ),
      ],
    );
  }

  Widget _buildLessonTab() {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Generate Lesson Quiz',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
          const SizedBox(height: 16),
          const Text('Select Lesson:', style: TextStyle(fontWeight: FontWeight.w600)),
          const SizedBox(height: 8),
          DropdownButtonFormField<String>(
            value: _selectedLessonId,
            hint: const Text('Choose a lesson'),
            decoration: const InputDecoration(border: OutlineInputBorder()),
            items: _lessons.map((l) => DropdownMenuItem(
              value: l['id'] as String,
              child: Text(l['title'] ?? ''),
            )).toList(),
            onChanged: (v) => setState(() => _selectedLessonId = v),
          ),
          const SizedBox(height: 16),
          const Text('AI Model:', style: TextStyle(fontWeight: FontWeight.w600)),
          const SizedBox(height: 8),
          DropdownButtonFormField<String>(
            value: _selectedLlm,
            decoration: const InputDecoration(border: OutlineInputBorder()),
            items: _llmOptions.map((m) => DropdownMenuItem(
              value: m, child: Text(m.toUpperCase()),
            )).toList(),
            onChanged: (v) => setState(() => _selectedLlm = v!),
          ),
          const SizedBox(height: 24),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton.icon(
              onPressed: _isGenerating || _selectedLessonId == null ? null : () =>
                  _generate('/admin/lessons/$_selectedLessonId/generate-quiz',
                      {'llm': _selectedLlm}),
              icon: const Icon(Icons.auto_awesome, color: Colors.white),
              label: Text(_isGenerating ? 'Generating...' : 'Generate Quiz',
                  style: const TextStyle(color: Colors.white)),
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFF1A3CBA),
                padding: const EdgeInsets.symmetric(vertical: 14),
              ),
            ),
          ),
          if (_log.isNotEmpty) ...[
            const SizedBox(height: 16),
            _buildLog(),
          ],
        ],
      ),
    );
  }

  Widget _buildFinalTab() {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Generate Final Quiz',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
          const SizedBox(height: 4),
          Text('Covers all lessons in a grade fairly using stratified retrieval.',
              style: TextStyle(color: Colors.grey.shade600, fontSize: 13)),
          const SizedBox(height: 16),
          const Text('Grade:', style: TextStyle(fontWeight: FontWeight.w600)),
          const SizedBox(height: 8),
          DropdownButtonFormField<String>(
            value: _selectedGrade,
            decoration: const InputDecoration(border: OutlineInputBorder()),
            items: ['Grade 9', 'Grade 10', 'Grade 11'].map((g) =>
                DropdownMenuItem(value: g, child: Text(g))).toList(),
            onChanged: (v) => setState(() => _selectedGrade = v!),
          ),
          const SizedBox(height: 16),
          const Text('AI Model:', style: TextStyle(fontWeight: FontWeight.w600)),
          const SizedBox(height: 8),
          DropdownButtonFormField<String>(
            value: _selectedLlm,
            decoration: const InputDecoration(border: OutlineInputBorder()),
            items: _llmOptions.map((m) => DropdownMenuItem(
              value: m, child: Text(m.toUpperCase()),
            )).toList(),
            onChanged: (v) => setState(() => _selectedLlm = v!),
          ),
          const SizedBox(height: 16),
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: Colors.orange.withOpacity(0.1),
              borderRadius: BorderRadius.circular(8),
            ),
            child: const Row(children: [
              Icon(Icons.info_outline, color: Colors.orange, size: 18),
              SizedBox(width: 8),
              Expanded(child: Text(
                'Each LLM run is recorded for comparison research.',
                style: TextStyle(color: Colors.orange, fontSize: 13),
              )),
            ]),
          ),
          const SizedBox(height: 24),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton.icon(
              onPressed: _isGenerating ? null : () =>
                  _generate('/admin/final-quiz/generate',
                      {'grade': _selectedGrade, 'llm': _selectedLlm}),
              icon: const Icon(Icons.auto_awesome, color: Colors.white),
              label: Text(_isGenerating ? 'Generating...' : 'Generate Final Quiz',
                  style: const TextStyle(color: Colors.white)),
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.orange,
                padding: const EdgeInsets.symmetric(vertical: 14),
              ),
            ),
          ),
          if (_log.isNotEmpty) ...[
            const SizedBox(height: 16),
            _buildLog(),
          ],
        ],
      ),
    );
  }

  Widget _buildLog() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: const Color(0xFF1A1A2E),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Text(_log,
          style: const TextStyle(color: Colors.greenAccent,
              fontFamily: 'monospace', fontSize: 12)),
    );
  }
}



