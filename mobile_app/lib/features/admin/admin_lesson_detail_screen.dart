import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';
import 'dart:typed_data';
import 'package:file_picker/file_picker.dart';

class AdminLessonDetailScreen extends StatefulWidget {
  final String lessonId;
  final String lessonTitle;
  final String grade;

  const AdminLessonDetailScreen({
    super.key,
    required this.lessonId,
    required this.lessonTitle,
    required this.grade,
  });

  @override
  State<AdminLessonDetailScreen> createState() => _AdminLessonDetailScreenState();
}

class _AdminLessonDetailScreenState extends State<AdminLessonDetailScreen>
    with SingleTickerProviderStateMixin {
  static final String _backendUrl =
      kIsWeb ? 'http://localhost:8000' : 'http://10.0.2.2:8000';

  // Quiz settings state
  String _quizMode = 'random';      // 'random' or 'manual'
  int _questionCount = 10;
  bool _isPublished = false;
  Set<String> _selectedQuestionIds = {};
  bool _settingsLoading = true;
  bool _settingsSaving = false;

  late TabController _tabController;
  bool _isUploading = false;
  bool _isGenerating = false;
  String _selectedModel = 'gemini';
  int _numQuestions = 10;
  String? _selectedMaterialId;
  String _statusLog = '';

  final List<Map<String, dynamic>> _models = [
    {'value': 'gemini', 'label': 'Gemini 1.5 Flash', 'icon': '🔵', 'color': Colors.blue},
    {'value': 'llama',  'label': 'Llama 3.3 70B',    'icon': '🟣', 'color': Colors.purple},
    {'value': 'mistral','label': 'Mistral Small',     'icon': '🟠', 'color': Colors.orange},
  ];

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
    _loadSettings();
  }

  Future<void> _loadSettings() async {
    try {
      final doc = await FirebaseFirestore.instance
          .collection('lessons')
          .doc(widget.lessonId)
          .collection('quizSettings')
          .doc('config')
          .get();
      if (doc.exists) {
        final d = doc.data()!;
        setState(() {
          _quizMode = d['mode'] ?? 'random';
          _questionCount = d['questionCount'] ?? 10;
          _isPublished = d['isPublished'] ?? false;
          _selectedQuestionIds = Set<String>.from(d['selectedQuestionIds'] ?? []);
        });
      }
    } catch (_) {}
    setState(() => _settingsLoading = false);
  }

  Future<void> _saveSettings() async {
    setState(() => _settingsSaving = true);
    try {
      await FirebaseFirestore.instance
          .collection('lessons')
          .doc(widget.lessonId)
          .collection('quizSettings')
          .doc('config')
          .set({
        'mode': _quizMode,
        'questionCount': _questionCount,
        'isPublished': _isPublished,
        'selectedQuestionIds': _selectedQuestionIds.toList(),
        'updatedAt': FieldValue.serverTimestamp(),
      });
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Settings saved!'), backgroundColor: Colors.green),
      );
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Error: $e'), backgroundColor: Colors.red),
      );
    }
    setState(() => _settingsSaving = false);
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<String?> _getToken() async =>
      await FirebaseAuth.instance.currentUser?.getIdToken();

  Future<void> _uploadPdf() async {
    final result = await FilePicker.platform.pickFiles(
      type: FileType.custom,
      allowedExtensions: ['pdf'],
      withData: true,
    );
    if (result == null || result.files.isEmpty) return;

    final file = result.files.first;
    final bytes = file.bytes;
    if (bytes == null) return;

    setState(() { _isUploading = true; _statusLog = ''; });
    _log('Uploading ${file.name}...');

    try {
      final token = await _getToken();
      final request = http.MultipartRequest(
        'POST',
        Uri.parse('$_backendUrl/admin/lessons/${widget.lessonId}/materials'),
      );
      request.headers['Authorization'] = 'Bearer $token';
      request.files.add(http.MultipartFile.fromBytes(
        'file', bytes, filename: file.name,
      ));
      request.fields['materialType'] = 'pdf';

      final response = await request.send();
      final body = await response.stream.bytesToString();

      if (response.statusCode >= 400) {
        _log('❌ Upload failed: $body');
        return;
      }
      final data = jsonDecode(body) as Map<String, dynamic>;
      final materialId = data['id'] ?? data['materialId'];
      if (materialId != null) {
        setState(() => _selectedMaterialId = materialId as String);
        _log('✅ PDF uploaded successfully!');
        _log('You can now generate quiz questions.');
      } else {
        _log('❌ Upload failed: $body');
      }
    } catch (e) {
      _log('❌ Error: $e');
    } finally {
      setState(() => _isUploading = false);
    }
  }

  Future<void> _generateQuiz() async {
    if (_selectedMaterialId == null) {
      _showError('Upload a PDF first.');
      return;
    }

    setState(() { _isGenerating = true; _statusLog = ''; });
    _log('Starting quiz generation...');
    _log('Model: ${_models.firstWhere((m) => m['value'] == _selectedModel)['label']}');
    _log('Questions: $_numQuestions');

    try {
      final token = await _getToken();
      final response = await http.post(
        Uri.parse('$_backendUrl/admin/lessons/${widget.lessonId}/generate-quiz'),
        headers: {'Authorization': 'Bearer $token', 'Content-Type': 'application/json'},
        body: jsonEncode({
          'model': _selectedModel,
          'num_questions': _numQuestions,
          'material_id': _selectedMaterialId,
        }),
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        _log('✅ Generated ${data['questions_saved']} questions using ${data['model_used']}!');
        _log('Questions are now saved and visible to students.');
        setState(() => _tabController.index = 1);
      } else {
        _log('❌ Error ${response.statusCode}: ${response.body}');
      }
    } catch (e) {
      _log('❌ Error: $e');
    } finally {
      setState(() => _isGenerating = false);
    }
  }

  void _log(String msg) => setState(() =>
      _statusLog += '${DateTime.now().toString().substring(11, 19)} — $msg\n');

  void _showError(String msg) => ScaffoldMessenger.of(context)
      .showSnackBar(SnackBar(content: Text(msg), backgroundColor: Colors.red));

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4F6FB),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        title: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(widget.lessonTitle,
                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15)),
            Text(widget.grade,
                style: TextStyle(color: Colors.grey.shade500, fontSize: 12)),
          ],
        ),
        bottom: TabBar(
          controller: _tabController,
          tabs: const [
            Tab(icon: Icon(Icons.upload_file), text: 'Generate'),
            Tab(icon: Icon(Icons.quiz), text: 'Questions'),
            Tab(icon: Icon(Icons.settings), text: 'Settings'),
          ],
        ),
      ),
      body: TabBarView(
        controller: _tabController,
        children: [_buildUploadTab(), _buildQuestionsTab(), _buildSettingsTab()],
      ),
    );
  }

  Widget _buildUploadTab() {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // PDF Upload Section
          _sectionHeader('Step 1: Upload Lesson PDF', Icons.upload_file, Colors.blue),
          const SizedBox(height: 12),
          GestureDetector(
            onTap: _isUploading ? null : _uploadPdf,
            child: Container(
              width: double.infinity,
              padding: const EdgeInsets.all(24),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(
                  color: _selectedMaterialId != null ? Colors.green : Colors.blue.withOpacity(0.3),
                  width: 2,
                  style: BorderStyle.solid,
                ),
              ),
              child: Column(
                children: [
                  _isUploading
                      ? const CircularProgressIndicator()
                      : Icon(
                          _selectedMaterialId != null ? Icons.check_circle : Icons.picture_as_pdf,
                          size: 48,
                          color: _selectedMaterialId != null ? Colors.green : Colors.blue,
                        ),
                  const SizedBox(height: 12),
                  Text(
                    _selectedMaterialId != null
                        ? 'PDF Uploaded ✓\nTap to upload a different PDF'
                        : _isUploading ? 'Uploading...' : 'Tap to select PDF file',
                    textAlign: TextAlign.center,
                    style: TextStyle(
                      color: _selectedMaterialId != null ? Colors.green : Colors.blue,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ),
            ),
          ),

          const SizedBox(height: 28),

          // AI Model Selection
          _sectionHeader('Step 2: Choose AI Model', Icons.smart_toy, Colors.purple),
          const SizedBox(height: 12),
          ..._models.map((m) => _buildModelCard(m)),

          const SizedBox(height: 20),

          // Number of questions
          _sectionHeader('Step 3: Number of Questions', Icons.format_list_numbered, Colors.orange),
          const SizedBox(height: 12),
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(12),
            ),
            child: Column(
              children: [
                Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
                  const Text('Questions:', style: TextStyle(fontWeight: FontWeight.w600)),
                  Text('$_numQuestions',
                      style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold,
                          color: Color(0xFF1A3CBA))),
                ]),
                Slider(
                  value: _numQuestions.toDouble(),
                  min: 5, max: 20, divisions: 15,
                  activeColor: const Color(0xFF1A3CBA),
                  onChanged: (v) => setState(() => _numQuestions = v.round()),
                ),
              ],
            ),
          ),

          const SizedBox(height: 28),

          // Generate Button
          SizedBox(
            width: double.infinity,
            child: ElevatedButton.icon(
              onPressed: _isGenerating || _selectedMaterialId == null ? null : _generateQuiz,
              icon: _isGenerating
                  ? const SizedBox(width: 18, height: 18,
                      child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                  : const Icon(Icons.auto_awesome, color: Colors.white),
              label: Text(
                _isGenerating ? 'Generating...' : 'Generate Quiz Questions',
                style: const TextStyle(color: Colors.white, fontSize: 16),
              ),
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFF1A3CBA),
                padding: const EdgeInsets.symmetric(vertical: 16),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              ),
            ),
          ),

          // Status log
          if (_statusLog.isNotEmpty) ...[
            const SizedBox(height: 20),
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: const Color(0xFF1A1A2E),
                borderRadius: BorderRadius.circular(10),
              ),
              child: Text(_statusLog,
                  style: const TextStyle(color: Colors.greenAccent,
                      fontFamily: 'monospace', fontSize: 12)),
            ),
          ],
        ],
      ),
    );
  }

  Widget _buildModelCard(Map<String, dynamic> m) {
    final isSelected = _selectedModel == m['value'];
    final color = m['color'] as Color;
    return GestureDetector(
      onTap: () => setState(() => _selectedModel = m['value']),
      child: Container(
        margin: const EdgeInsets.only(bottom: 10),
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(
          color: isSelected ? color.withOpacity(0.08) : Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: isSelected ? color : Colors.grey.shade200,
            width: isSelected ? 2 : 1,
          ),
        ),
        child: Row(children: [
          Text(m['icon'], style: const TextStyle(fontSize: 24)),
          const SizedBox(width: 12),
          Expanded(
            child: Text(m['label'],
                style: TextStyle(fontWeight: FontWeight.w600,
                    color: isSelected ? color : Colors.black87)),
          ),
          if (isSelected) Icon(Icons.check_circle, color: color),
        ]),
      ),
    );
  }

  Widget _buildQuestionsTab() {
    return StreamBuilder<QuerySnapshot>(
      stream: FirebaseFirestore.instance
          .collection('lessons')
          .doc(widget.lessonId)
          .collection('questions')
          .orderBy('createdAt', descending: true)
          .snapshots(),
      builder: (context, snap) {
        if (!snap.hasData) return const Center(child: CircularProgressIndicator());
        final docs = snap.data!.docs;

        if (docs.isEmpty) {
          return Center(
            child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
              Icon(Icons.quiz_outlined, size: 64, color: Colors.grey.shade300),
              const SizedBox(height: 12),
              Text('No questions yet.', style: TextStyle(color: Colors.grey.shade500)),
              const SizedBox(height: 6),
              const Text('Upload a PDF and generate questions.',
                  style: TextStyle(color: Colors.grey)),
            ]),
          );
        }

        return ListView.builder(
          padding: const EdgeInsets.all(16),
          itemCount: docs.length,
          itemBuilder: (ctx, i) {
            final d = docs[i].data() as Map<String, dynamic>;
            final options = (d['options'] as List?) ?? [];
            return Card(
              margin: const EdgeInsets.only(bottom: 14),
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(children: [
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                        decoration: BoxDecoration(
                          color: const Color(0xFF1A3CBA).withOpacity(0.1),
                          borderRadius: BorderRadius.circular(6),
                        ),
                        child: Text('Q${i + 1}',
                            style: const TextStyle(color: Color(0xFF1A3CBA),
                                fontWeight: FontWeight.bold, fontSize: 12)),
                      ),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(d['modelLabel'] ?? '',
                            style: TextStyle(color: Colors.grey.shade500, fontSize: 11)),
                      ),
                      IconButton(
                        icon: const Icon(Icons.delete_outline, color: Colors.red, size: 20),
                        onPressed: () => docs[i].reference.delete(),
                        padding: EdgeInsets.zero,
                        constraints: const BoxConstraints(),
                      ),
                    ]),
                    const SizedBox(height: 10),
                    Text(d['question'] ?? '',
                        style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 15)),
                    const SizedBox(height: 10),
                    ...options.map((opt) {
                      final label = opt['label'] ?? '';
                      final isCorrect = label == d['correct'];
                      return Container(
                        margin: const EdgeInsets.only(bottom: 6),
                        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                        decoration: BoxDecoration(
                          color: isCorrect ? Colors.green.withOpacity(0.08) : Colors.grey.shade50,
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(
                            color: isCorrect ? Colors.green : Colors.grey.shade200,
                          ),
                        ),
                        child: Row(children: [
                          Text(label,
                              style: TextStyle(
                                  fontWeight: FontWeight.bold,
                                  color: isCorrect ? Colors.green : Colors.grey.shade600)),
                          const SizedBox(width: 10),
                          Expanded(child: Text(opt['text'] ?? '')),
                          if (isCorrect)
                            const Icon(Icons.check, color: Colors.green, size: 16),
                        ]),
                      );
                    }),
                    if ((d['explanation'] ?? '').isNotEmpty) ...[
                      const SizedBox(height: 8),
                      Container(
                        padding: const EdgeInsets.all(10),
                        decoration: BoxDecoration(
                          color: Colors.blue.withOpacity(0.05),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
                          const Icon(Icons.info_outline, color: Colors.blue, size: 16),
                          const SizedBox(width: 6),
                          Expanded(child: Text(d['explanation'] ?? '',
                              style: const TextStyle(color: Colors.blue, fontSize: 13))),
                        ]),
                      ),
                    ],
                  ],
                ),
              ),
            );
          },
        );
      },
    );
  }

  Widget _buildSettingsTab() {
    if (_settingsLoading) return const Center(child: CircularProgressIndicator());

    return SingleChildScrollView(
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Publish toggle
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: _isPublished ? Colors.green.withOpacity(0.08) : Colors.grey.shade50,
              borderRadius: BorderRadius.circular(12),
              border: Border.all(
                color: _isPublished ? Colors.green : Colors.grey.shade300,
              ),
            ),
            child: Row(children: [
              Icon(_isPublished ? Icons.visibility : Icons.visibility_off,
                  color: _isPublished ? Colors.green : Colors.grey),
              const SizedBox(width: 12),
              Expanded(
                child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text(_isPublished ? 'Quiz Published' : 'Quiz Unpublished',
                      style: TextStyle(
                          fontWeight: FontWeight.bold,
                          color: _isPublished ? Colors.green : Colors.grey.shade700)),
                  Text(_isPublished
                      ? 'Students can see and take this quiz.'
                      : 'Students cannot see this quiz yet.',
                      style: TextStyle(fontSize: 12, color: Colors.grey.shade600)),
                ]),
              ),
              Switch(
                value: _isPublished,
                activeColor: Colors.green,
                onChanged: (v) => setState(() => _isPublished = v),
              ),
            ]),
          ),
          const SizedBox(height: 24),

          _sectionHeader('Quiz Mode', Icons.tune, Colors.purple),
          const SizedBox(height: 12),

          // Random mode
          GestureDetector(
            onTap: () => setState(() => _quizMode = 'random'),
            child: Container(
              padding: const EdgeInsets.all(16),
              margin: const EdgeInsets.only(bottom: 10),
              decoration: BoxDecoration(
                color: _quizMode == 'random' ? Colors.purple.withOpacity(0.08) : Colors.white,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(
                  color: _quizMode == 'random' ? Colors.purple : Colors.grey.shade200,
                  width: _quizMode == 'random' ? 2 : 1,
                ),
              ),
              child: Row(children: [
                Radio<String>(
                  value: 'random',
                  groupValue: _quizMode,
                  activeColor: Colors.purple,
                  onChanged: (v) => setState(() => _quizMode = v!),
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    const Text('Random Shuffle', style: TextStyle(fontWeight: FontWeight.bold)),
                    Text('Pick N random questions each time student takes quiz.',
                        style: TextStyle(fontSize: 12, color: Colors.grey.shade600)),
                  ]),
                ),
              ]),
            ),
          ),

          // Manual mode
          GestureDetector(
            onTap: () => setState(() => _quizMode = 'manual'),
            child: Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: _quizMode == 'manual' ? Colors.blue.withOpacity(0.08) : Colors.white,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(
                  color: _quizMode == 'manual' ? Colors.blue : Colors.grey.shade200,
                  width: _quizMode == 'manual' ? 2 : 1,
                ),
              ),
              child: Row(children: [
                Radio<String>(
                  value: 'manual',
                  groupValue: _quizMode,
                  activeColor: Colors.blue,
                  onChanged: (v) => setState(() => _quizMode = v!),
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    const Text('Manual Selection', style: TextStyle(fontWeight: FontWeight.bold)),
                    Text('Handpick exactly which questions students will see.',
                        style: TextStyle(fontSize: 12, color: Colors.grey.shade600)),
                  ]),
                ),
              ]),
            ),
          ),

          const SizedBox(height: 24),

          // Random: question count slider
          if (_quizMode == 'random') ...[
            _sectionHeader('Number of Questions for Students', Icons.format_list_numbered, Colors.orange),
            const SizedBox(height: 12),
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(12),
              ),
              child: Column(children: [
                Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
                  const Text('Questions per attempt:', style: TextStyle(fontWeight: FontWeight.w600)),
                  Text('$_questionCount',
                      style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold,
                          color: Color(0xFF1A3CBA))),
                ]),
                Slider(
                  value: _questionCount.toDouble(),
                  min: 1, max: 30, divisions: 29,
                  activeColor: Colors.orange,
                  onChanged: (v) => setState(() => _questionCount = v.round()),
                ),
                Text('Students will get $_questionCount random questions from the full question bank.',
                    style: TextStyle(fontSize: 12, color: Colors.grey.shade600),
                    textAlign: TextAlign.center),
              ]),
            ),
          ],

          // Manual: checklist of questions
          if (_quizMode == 'manual') ...[
            _sectionHeader('Select Questions for Students', Icons.checklist, Colors.blue),
            const SizedBox(height: 4),
            Text('Tick the questions you want students to answer.',
                style: TextStyle(fontSize: 12, color: Colors.grey.shade600)),
            const SizedBox(height: 12),
            StreamBuilder<QuerySnapshot>(
              stream: FirebaseFirestore.instance
                  .collection('lessons')
                  .doc(widget.lessonId)
                  .collection('questions')
                  .orderBy('createdAt')
                  .snapshots(),
              builder: (context, snap) {
                if (!snap.hasData) return const CircularProgressIndicator();
                final docs = snap.data!.docs;
                if (docs.isEmpty) {
                  return const Text('No questions generated yet. Go to Generate tab first.');
                }
                return Column(
                  children: docs.asMap().entries.map((entry) {
                    final i = entry.key;
                    final doc = entry.value;
                    final d = doc.data() as Map<String, dynamic>;
                    final isSelected = _selectedQuestionIds.contains(doc.id);
                    return Container(
                      margin: const EdgeInsets.only(bottom: 8),
                      decoration: BoxDecoration(
                        color: isSelected ? Colors.blue.withOpacity(0.06) : Colors.white,
                        borderRadius: BorderRadius.circular(10),
                        border: Border.all(
                          color: isSelected ? Colors.blue : Colors.grey.shade200,
                        ),
                      ),
                      child: CheckboxListTile(
                        value: isSelected,
                        activeColor: Colors.blue,
                        onChanged: (v) {
                          setState(() {
                            if (v == true) {
                              _selectedQuestionIds.add(doc.id);
                            } else {
                              _selectedQuestionIds.remove(doc.id);
                            }
                          });
                        },
                        title: Text('Q${i + 1}: ${d['question'] ?? ''}',
                            style: const TextStyle(fontSize: 13),
                            maxLines: 2,
                            overflow: TextOverflow.ellipsis),
                        subtitle: Text('Correct: ${d['correct']}  •  ${d['modelLabel'] ?? ''}',
                            style: TextStyle(fontSize: 11, color: Colors.grey.shade500)),
                        controlAffinity: ListTileControlAffinity.leading,
                      ),
                    );
                  }).toList(),
                );
              },
            ),
            const SizedBox(height: 8),
            Text('${_selectedQuestionIds.length} questions selected',
                style: const TextStyle(fontWeight: FontWeight.w600, color: Colors.blue)),
          ],

          const SizedBox(height: 28),

          SizedBox(
            width: double.infinity,
            child: ElevatedButton.icon(
              onPressed: _settingsSaving ? null : _saveSettings,
              icon: _settingsSaving
                  ? const SizedBox(width: 18, height: 18,
                      child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                  : const Icon(Icons.save, color: Colors.white),
              label: Text(_settingsSaving ? 'Saving...' : 'Save Settings',
                  style: const TextStyle(color: Colors.white, fontSize: 16)),
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFF1A3CBA),
                padding: const EdgeInsets.symmetric(vertical: 14),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _sectionHeader(String title, IconData icon, Color color) {
    return Row(children: [
      Icon(icon, color: color, size: 20),
      const SizedBox(width: 8),
      Text(title, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15)),
    ]);
  }
}
