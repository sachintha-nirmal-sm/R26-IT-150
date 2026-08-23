import 'package:flutter/material.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:http/http.dart' as http;
import 'package:file_picker/file_picker.dart';
import 'dart:convert';

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
  late TabController _tabController;

  static const String _backendUrl = 'http://localhost:9000';

  // Settings state
  String _quizMode = 'random';
  int _questionCount = 10;
  bool _isPublished = false;
  Set<String> _selectedQuestionIds = {};
  bool _settingsSaving       = false; // main Save Settings button
  bool _subLessonSaving      = false; // Save Sub-lesson Settings button

  // Scroll controllers for Settings tab sections
  final ScrollController _subLessonSettingsScroll = ScrollController();
  final ScrollController _manualQScroll           = ScrollController();

  // PDF state
  List<Map<String, dynamic>> _pdfs = [];
  bool _pdfsLoading = true;
  bool _pdfSaving = false;

  // Generate state
  String _selectedModel = 'openrouter';
  int _generateCount = 10; // used only when no sub-lessons
  bool _isGenerating = false;
  Map<String, dynamic>? _lastGenerationStats;
  List<Map<String, dynamic>> _sessionHistory = [];
  bool _sessionsLoading = false;

  // Sub-lessons state
  List<Map<String, dynamic>> _subLessons = [];
  bool _subLessonsLoading = true; // true until first load completes — prevents flash

  // Per-sub-lesson generate counts (subLessonId -> count, default 10)
  Map<String, int> _subLessonGenerateCounts = {};

  // Per-sub-lesson settings (subLessonId -> value)
  Map<String, int> _subLessonQuizCounts = {};
  Map<String, bool> _subLessonPublished = {};

  // Questions filter state — null means "All"
  String? _selectedSubLessonFilter;

  // Difficulty accuracy state (computed live from question docs)
  Map<String, Map<String, int>> _difficultyStats = {};

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 4, vsync: this);
    _loadSettings();
    _loadPdfs();
    _loadSessionHistory();
    _loadSubLessons();
  }

  @override
  void dispose() {
    _tabController.dispose();
    _subLessonSettingsScroll.dispose();
    _manualQScroll.dispose();
    super.dispose();
  }

  Future<String?> _getToken() async =>
      await FirebaseAuth.instance.currentUser?.getIdToken();

  // ---------------------------------------------------------------------------
  // PDF helpers
  // ---------------------------------------------------------------------------

  Future<void> _loadPdfs() async {
    setState(() => _pdfsLoading = true);
    try {
      final token = await _getToken();
      final res = await http.get(
        Uri.parse('$_backendUrl/admin/lessons/${widget.lessonId}/pdfs'),
        headers: {'Authorization': 'Bearer $token'},
      );
      if (res.statusCode == 200) {
        final list = jsonDecode(res.body) as List;
        setState(() => _pdfs = list.cast<Map<String, dynamic>>());
      }
    } catch (_) {}
    setState(() => _pdfsLoading = false);
  }

  Future<void> _savePdfLink(String fileName, String url) async {
    final placeholder = {'id': '__pending__', 'fileName': fileName, 'url': url};
    setState(() {
      _pdfs = [placeholder, ..._pdfs];
      _pdfSaving = true;
    });
    try {
      final token = await _getToken();
      final res = await http.post(
        Uri.parse('$_backendUrl/admin/lessons/${widget.lessonId}/pdfs'),
        headers: {'Authorization': 'Bearer $token', 'Content-Type': 'application/json'},
        body: jsonEncode({'fileName': fileName, 'url': url}),
      );
      if (res.statusCode == 201) {
        final saved = jsonDecode(res.body) as Map<String, dynamic>;
        setState(() => _pdfs = [saved, ..._pdfs.where((p) => p['id'] != '__pending__')]);
        if (mounted) _showSnack('Link saved!', Colors.green);
      } else {
        setState(() => _pdfs = _pdfs.where((p) => p['id'] != '__pending__').toList());
        if (mounted) _showSnack('Failed: ${res.body}', Colors.red);
      }
    } catch (e) {
      setState(() => _pdfs = _pdfs.where((p) => p['id'] != '__pending__').toList());
      if (mounted) _showSnack('Error: $e', Colors.red);
    }
    setState(() => _pdfSaving = false);
  }

  Future<void> _uploadPdfFile() async {
    final result = await FilePicker.platform.pickFiles(
      type: FileType.custom,
      allowedExtensions: ['pdf'],
      withData: true,
    );
    if (result == null || result.files.isEmpty) return;
    final file = result.files.first;
    final bytes = file.bytes;
    if (bytes == null) return;

    final placeholder = {'id': '__pending__', 'fileName': file.name, 'url': ''};
    setState(() {
      _pdfs = [placeholder, ..._pdfs];
      _pdfSaving = true;
    });

    try {
      final token = await _getToken();
      final request = http.MultipartRequest(
        'POST',
        Uri.parse('$_backendUrl/admin/lessons/${widget.lessonId}/pdfs/upload'),
      );
      request.headers['Authorization'] = 'Bearer $token';
      request.files.add(http.MultipartFile.fromBytes('file', bytes, filename: file.name));
      final streamed = await request.send();
      final body = await streamed.stream.bytesToString();
      if (streamed.statusCode == 201) {
        final saved = jsonDecode(body) as Map<String, dynamic>;
        setState(() => _pdfs = [saved, ..._pdfs.where((p) => p['id'] != '__pending__')]);
        if (mounted) _showSnack('PDF uploaded!', Colors.green);
      } else {
        setState(() => _pdfs = _pdfs.where((p) => p['id'] != '__pending__').toList());
        if (mounted) _showSnack('Upload failed: $body', Colors.red);
      }
    } catch (e) {
      setState(() => _pdfs = _pdfs.where((p) => p['id'] != '__pending__').toList());
      if (mounted) _showSnack('Error: $e', Colors.red);
    }
    setState(() => _pdfSaving = false);
  }

  Future<void> _deletePdf(String pdfId) async {
    setState(() => _pdfs = _pdfs.where((p) => p['id'] != pdfId).toList());
    try {
      final token = await _getToken();
      await http.delete(
        Uri.parse('$_backendUrl/admin/lessons/${widget.lessonId}/pdfs/$pdfId'),
        headers: {'Authorization': 'Bearer $token'},
      );
    } catch (_) {
      _loadPdfs();
    }
  }

  // ---------------------------------------------------------------------------
  // Sub-lesson helpers
  // ---------------------------------------------------------------------------

  Future<void> _loadSubLessons() async {
    setState(() => _subLessonsLoading = true);
    try {
      final token = await _getToken();
      final res = await http.get(
        Uri.parse('$_backendUrl/admin/lessons/${widget.lessonId}/sub-lessons'),
        headers: {'Authorization': 'Bearer $token'},
      );
      if (res.statusCode == 200) {
        final list = (jsonDecode(res.body) as List).cast<Map<String, dynamic>>();
        if (mounted) {
          setState(() {
            _subLessons = list;
            // Initialise per-sub-lesson maps with stored or default values
            for (final sl in list) {
              final id = sl['id'] as String;
              _subLessonGenerateCounts.putIfAbsent(id, () => ((sl['generateCount'] as int? ?? 10)).clamp(0, 10));
              _subLessonQuizCounts.putIfAbsent(id, () => ((sl['quizCount'] as int? ?? 5)).clamp(0, 10));
              _subLessonPublished.putIfAbsent(id, () => sl['isPublished'] as bool? ?? false);
            }
          });
        }
      }
    } catch (_) {}
    if (mounted) setState(() => _subLessonsLoading = false);
  }

  Future<void> _createSubLesson(String number, String title) async {
    final order = _subLessons.length + 1;
    try {
      final token = await _getToken();
      final res = await http.post(
        Uri.parse('$_backendUrl/admin/lessons/${widget.lessonId}/sub-lessons'),
        headers: {'Authorization': 'Bearer $token', 'Content-Type': 'application/json'},
        body: jsonEncode({'number': number, 'title': title, 'order': order}),
      );
      if (res.statusCode == 201) {
        await _loadSubLessons();
        if (mounted) _showSnack('Sub-lesson created!', Colors.green);
      } else {
        if (mounted) _showSnack('Failed: ${res.body}', Colors.red);
      }
    } catch (e) {
      if (mounted) _showSnack('Error: $e', Colors.red);
    }
  }

  Future<void> _deleteSubLesson(String subId) async {
    try {
      final token = await _getToken();
      await http.delete(
        Uri.parse('$_backendUrl/admin/lessons/${widget.lessonId}/sub-lessons/$subId'),
        headers: {'Authorization': 'Bearer $token'},
      );
      await _loadSubLessons();
      if (mounted) _showSnack('Sub-lesson deleted.', Colors.orange);
    } catch (e) {
      if (mounted) _showSnack('Error: $e', Colors.red);
    }
  }

  // ---------------------------------------------------------------------------
  // Session / stats helpers
  // ---------------------------------------------------------------------------

  Future<void> _loadSessionHistory() async {
    setState(() => _sessionsLoading = true);
    try {
      final snap = await FirebaseFirestore.instance
          .collection('lessons')
          .doc(widget.lessonId)
          .collection('generationSessions')
          .orderBy('createdAt', descending: true)
          .limit(20)
          .get();
      if (mounted) {
        setState(() =>
            _sessionHistory = snap.docs.map((d) => {'id': d.id, ...d.data()}).toList());
      }
    } catch (_) {}
    if (mounted) setState(() => _sessionsLoading = false);
    _loadDifficultyStats();
  }

  Future<void> _loadDifficultyStats() async {
    try {
      final snap = await FirebaseFirestore.instance
          .collection('lessons')
          .doc(widget.lessonId)
          .collection('questions')
          .get();
      final stats = <String, Map<String, int>>{};
      for (final doc in snap.docs) {
        final d = doc.data();
        final model = d['generatedBy'] as String? ?? 'unknown';
        if (d['actualDifficulty'] == null) continue;
        stats.putIfAbsent(model, () => {'evaluated': 0, 'matched': 0});
        stats[model]!['evaluated'] = stats[model]!['evaluated']! + 1;
        if (d['difficultyMatch'] == true) {
          stats[model]!['matched'] = stats[model]!['matched']! + 1;
        }
      }
      if (mounted) setState(() => _difficultyStats = stats);
    } catch (_) {}
  }

  // ---------------------------------------------------------------------------
  // Question generation
  // ---------------------------------------------------------------------------

  Future<void> _generateQuestions() async {
    setState(() => _isGenerating = true);
    try {
      final token = await _getToken();

      final Map<String, dynamic> body = {'model': _selectedModel, 'count': _generateCount};
      if (_subLessons.isNotEmpty) {
        body['sub_lesson_counts'] = {
          for (final sl in _subLessons)
            sl['id'] as String: _subLessonGenerateCounts[sl['id']] ?? 10,
        };
      }

      final res = await http.post(
        Uri.parse('$_backendUrl/admin/lessons/${widget.lessonId}/generate-questions'),
        headers: {'Authorization': 'Bearer $token', 'Content-Type': 'application/json'},
        body: jsonEncode(body),
      ).timeout(const Duration(seconds: 300));

      if (res.statusCode == 201) {
        final data = jsonDecode(res.body) as Map<String, dynamic>;
        final count = data['generated'] as int? ?? 0;
        if (mounted) {
          setState(() {
            _lastGenerationStats = data;
            _difficultyStats = {};
            _selectedSubLessonFilter = null; // reset filter to show all
          });
          _loadSessionHistory();
          _showSnack('Generated $count questions — accuracy verified!', Colors.green);
          _tabController.animateTo(1); // go to Questions tab
        }
      } else {
        if (mounted) _showSnack('Failed: ${res.body}', Colors.red);
      }
    } catch (e) {
      if (mounted) _showSnack('Error: $e', Colors.red);
    }
    if (mounted) setState(() => _isGenerating = false);
  }

  // ---------------------------------------------------------------------------
  // Settings helpers
  // ---------------------------------------------------------------------------

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
          _questionCount = ((d['questionCount'] ?? 10) as num).clamp(0, 10).toInt();
          _isPublished = d['isPublished'] ?? false;
          _selectedQuestionIds = Set<String>.from(d['selectedQuestionIds'] ?? []);
        });
      }
    } catch (_) {}
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
      if (mounted) {
        _showSnack('Settings saved!', Colors.green);
      }
    } catch (e) {
      if (mounted) _showSnack('Error: $e', Colors.red);
    }
    setState(() => _settingsSaving = false);
  }

  void _showSnack(String msg, Color color) => ScaffoldMessenger.of(context)
      .showSnackBar(SnackBar(content: Text(msg), backgroundColor: color));

  Future<void> _saveSubLessonSettings() async {
    setState(() => _subLessonSaving = true);
    try {
      final batch = FirebaseFirestore.instance.batch();
      for (final sl in _subLessons) {
        final id = sl['id'] as String;
        final ref = FirebaseFirestore.instance
            .collection('lessons')
            .doc(widget.lessonId)
            .collection('subLessons')
            .doc(id);
        batch.update(ref, {
          'quizCount': _subLessonQuizCounts[id] ?? 5,
          'isPublished': _subLessonPublished[id] ?? false,
          'updatedAt': FieldValue.serverTimestamp(),
        });
      }
      await batch.commit();
      if (mounted) _showSnack('Sub-lesson settings saved!', Colors.green);
    } catch (e) {
      if (mounted) _showSnack('Error: $e', Colors.red);
    }
    setState(() => _subLessonSaving = false);
  }

  // ---------------------------------------------------------------------------
  // Dialogs
  // ---------------------------------------------------------------------------

  void _showAddLinkDialog() {
    final nameCtrl = TextEditingController();
    final urlCtrl = TextEditingController();

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
            const Text('Add PDF Link',
                style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold)),
            const SizedBox(height: 6),
            Text('Paste a Google Drive, Dropbox, or any public PDF link.',
                style: TextStyle(color: Colors.grey.shade600, fontSize: 13)),
            const SizedBox(height: 16),
            TextField(
              controller: nameCtrl,
              decoration: const InputDecoration(
                labelText: 'Display Name',
                hintText: 'e.g. Chapter 4 Notes',
                border: OutlineInputBorder(),
                contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 10),
              ),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: urlCtrl,
              keyboardType: TextInputType.url,
              decoration: const InputDecoration(
                labelText: 'PDF URL',
                hintText: 'https://drive.google.com/...',
                border: OutlineInputBorder(),
                contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 10),
              ),
            ),
            const SizedBox(height: 20),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: () {
                  final name = nameCtrl.text.trim();
                  final url = urlCtrl.text.trim();
                  if (name.isEmpty || url.isEmpty) return;
                  Navigator.pop(ctx);
                  _savePdfLink(name, url);
                },
                icon: const Icon(Icons.link, color: Colors.white),
                label: const Text('Save Link', style: TextStyle(color: Colors.white)),
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

  void _showAddSubLessonDialog() {
    final numberCtrl = TextEditingController();
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
            const Text('Add Sub-lesson',
                style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold)),
            const SizedBox(height: 6),
            Text('Define a sub-section of this lesson (e.g. 4.1, 4.2).',
                style: TextStyle(color: Colors.grey.shade600, fontSize: 13)),
            const SizedBox(height: 16),
            TextField(
              controller: numberCtrl,
              decoration: const InputDecoration(
                labelText: 'Sub-lesson Number',
                hintText: 'e.g. 4.1',
                border: OutlineInputBorder(),
                contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 10),
              ),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: titleCtrl,
              decoration: const InputDecoration(
                labelText: 'Sub-lesson Title',
                hintText: 'e.g. Velocity and Acceleration',
                border: OutlineInputBorder(),
                contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 10),
              ),
            ),
            const SizedBox(height: 20),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: () {
                  final number = numberCtrl.text.trim();
                  final title = titleCtrl.text.trim();
                  if (number.isEmpty || title.isEmpty) return;
                  Navigator.pop(ctx);
                  _createSubLesson(number, title);
                },
                icon: const Icon(Icons.add, color: Colors.white),
                label: const Text('Create Sub-lesson', style: TextStyle(color: Colors.white)),
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

  // ---------------------------------------------------------------------------
  // Build
  // ---------------------------------------------------------------------------

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
            Tab(icon: Icon(Icons.auto_awesome), text: 'Generate'),
            Tab(icon: Icon(Icons.quiz), text: 'Questions'),
            Tab(icon: Icon(Icons.layers), text: 'Sub-lessons'),
            Tab(icon: Icon(Icons.settings), text: 'Settings'),
          ],
        ),
      ),
      body: TabBarView(
        controller: _tabController,
        children: [
          _buildGenerateTab(),
          _buildQuestionsTab(),
          _buildSubLessonsTab(),
          _buildSettingsTab(),
        ],
      ),
    );
  }

  // ---------------------------------------------------------------------------
  // Generate Tab
  // ---------------------------------------------------------------------------

  Widget _buildGenerateTab() {
    final hasSubLessons = !_subLessonsLoading && _subLessons.isNotEmpty;
    final totalToGenerate = hasSubLessons
        ? _subLessons.fold<int>(
            0, (sum, sl) => sum + (_subLessonGenerateCounts[sl['id']] ?? 10))
        : _generateCount;

    return SingleChildScrollView(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // PDF upload bar
          Container(
            color: Colors.white,
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
            child: Row(
              children: [
                Expanded(
                  child: ElevatedButton.icon(
                    onPressed: _pdfSaving ? null : _uploadPdfFile,
                    icon: _pdfSaving
                        ? const SizedBox(
                            width: 18, height: 18,
                            child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                        : const Icon(Icons.upload_file, color: Colors.white),
                    label: Text(
                      _pdfSaving ? 'Uploading...' : 'Upload PDF',
                      style: const TextStyle(color: Colors.white, fontSize: 15),
                    ),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: const Color(0xFF1A3CBA),
                      padding: const EdgeInsets.symmetric(vertical: 13),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                    ),
                  ),
                ),
                const SizedBox(width: 10),
                OutlinedButton.icon(
                  onPressed: _pdfSaving ? null : _showAddLinkDialog,
                  icon: const Icon(Icons.add_link, size: 18),
                  label: const Text('Add Link'),
                  style: OutlinedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 13, horizontal: 12),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                  ),
                ),
              ],
            ),
          ),

          // PDF list
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 16, 16, 4),
            child: Text('Lesson PDFs',
                style: TextStyle(
                    fontWeight: FontWeight.bold, fontSize: 13, color: Colors.grey.shade500)),
          ),
          if (_pdfsLoading)
            const Padding(
              padding: EdgeInsets.all(24),
              child: Center(child: CircularProgressIndicator()),
            )
          else if (_pdfs.isEmpty)
            Padding(
              padding: const EdgeInsets.symmetric(vertical: 20, horizontal: 16),
              child: Row(children: [
                Icon(Icons.picture_as_pdf_outlined, color: Colors.grey.shade300, size: 32),
                const SizedBox(width: 12),
                Text('No PDFs yet — upload one above.',
                    style: TextStyle(color: Colors.grey.shade400, fontSize: 13)),
              ]),
            )
          else
            ListView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              padding: const EdgeInsets.symmetric(horizontal: 16),
              itemCount: _pdfs.length,
              itemBuilder: (ctx, i) {
                final pdf = _pdfs[i];
                final url = pdf['url'] as String? ?? '';
                return Card(
                  margin: const EdgeInsets.only(bottom: 10),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                  child: ListTile(
                    contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                    leading: Container(
                      padding: const EdgeInsets.all(10),
                      decoration: BoxDecoration(
                        color: Colors.red.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(10),
                      ),
                      child: const Icon(Icons.picture_as_pdf, color: Colors.red, size: 26),
                    ),
                    title: Text(
                      pdf['fileName'] ?? 'Unknown',
                      style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 14),
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                    subtitle: Text(url,
                        style: TextStyle(color: Colors.grey.shade500, fontSize: 11),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis),
                    trailing: IconButton(
                      icon: const Icon(Icons.delete_outline, color: Colors.red),
                      onPressed: () => showDialog(
                        context: context,
                        builder: (ctx) => AlertDialog(
                          title: const Text('Delete PDF'),
                          content: Text('Delete "${pdf['fileName']}"?'),
                          actions: [
                            TextButton(
                                onPressed: () => Navigator.pop(ctx),
                                child: const Text('Cancel')),
                            TextButton(
                              onPressed: () {
                                Navigator.pop(ctx);
                                _deletePdf(pdf['id']);
                              },
                              child: const Text('Delete',
                                  style: TextStyle(color: Colors.red)),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                );
              },
            ),

          const Padding(
            padding: EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            child: Divider(),
          ),

          // Generate Questions section
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 4, 16, 32),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(children: [
                  const Icon(Icons.auto_awesome, color: Color(0xFF1A3CBA), size: 20),
                  const SizedBox(width: 8),
                  const Text('Generate Questions',
                      style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                ]),
                const SizedBox(height: 4),
                Text('AI reads your PDFs and writes quiz questions automatically.',
                    style: TextStyle(color: Colors.grey.shade600, fontSize: 13)),
                const SizedBox(height: 16),

                // Sub-lessons info banner — wait for load before showing
                if (_subLessonsLoading) ...[
                  Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: Colors.grey.shade100,
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: Row(children: [
                      const SizedBox(
                        width: 14, height: 14,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      ),
                      const SizedBox(width: 10),
                      Text('Loading sub-lessons...',
                          style: TextStyle(color: Colors.grey.shade500, fontSize: 12)),
                    ]),
                  ),
                  const SizedBox(height: 16),
                ] else if (hasSubLessons) ...[
                  Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: Colors.indigo.withOpacity(0.08),
                      borderRadius: BorderRadius.circular(10),
                      border: Border.all(color: Colors.indigo.withOpacity(0.3)),
                    ),
                    child: Row(children: [
                      const Icon(Icons.layers, color: Colors.indigo, size: 18),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          '${_subLessons.length} sub-lessons — '
                          '$totalToGenerate questions total across all sub-lessons',
                          style: TextStyle(color: Colors.indigo.shade700, fontSize: 12),
                        ),
                      ),
                    ]),
                  ),
                  const SizedBox(height: 16),
                ] else ...[
                  Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: Colors.orange.withOpacity(0.07),
                      borderRadius: BorderRadius.circular(10),
                      border: Border.all(color: Colors.orange.withOpacity(0.3)),
                    ),
                    child: Row(children: [
                      const Icon(Icons.info_outline, color: Colors.orange, size: 18),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          'No sub-lessons added yet. Questions will be generated without sub-lesson tags. '
                          'Add sub-lessons in the Sub-lessons tab first.',
                          style: TextStyle(color: Colors.orange.shade800, fontSize: 12),
                        ),
                      ),
                    ]),
                  ),
                  const SizedBox(height: 16),
                ],

                // Model selector
                const Text('AI Model',
                    style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                const SizedBox(height: 10),
                Wrap(spacing: 8, runSpacing: 8, children: [
                  _modelChip('openrouter', 'DeepSeek', Colors.teal),
                  _modelChip('groq', 'Groq', Colors.orange),
                  _modelChip('mistral', 'Mistral', Colors.purple),
                ]),
                const SizedBox(height: 20),

                // Per-sub-lesson sliders OR single global slider (hidden while loading)
                if (_subLessonsLoading) ...[
                  const SizedBox(height: 8),
                ] else if (hasSubLessons) ...[
                  const Text('Questions per Sub-lesson',
                      style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                  const SizedBox(height: 12),
                  ..._subLessons.map((sl) {
                    final id = sl['id'] as String;
                    final count = _subLessonGenerateCounts[id] ?? 10;
                    return Container(
                      margin: const EdgeInsets.only(bottom: 12),
                      padding: const EdgeInsets.fromLTRB(14, 12, 14, 8),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: Colors.grey.shade200),
                      ),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(children: [
                            _subLessonBadge(sl['number'] as String),
                            const SizedBox(width: 8),
                            Expanded(
                              child: Text(sl['title'] as String,
                                  style: const TextStyle(
                                      fontWeight: FontWeight.w600, fontSize: 13),
                                  overflow: TextOverflow.ellipsis),
                            ),
                            Container(
                              padding: const EdgeInsets.symmetric(
                                  horizontal: 10, vertical: 3),
                              decoration: BoxDecoration(
                                color: const Color(0xFF1A3CBA).withOpacity(0.1),
                                borderRadius: BorderRadius.circular(14),
                              ),
                              child: Text('$count',
                                  style: const TextStyle(
                                      fontSize: 15,
                                      fontWeight: FontWeight.bold,
                                      color: Color(0xFF1A3CBA))),
                            ),
                          ]),
                          Slider(
                            value: count.toDouble(),
                            min: 0,
                            max: 10,
                            divisions: 10,
                            activeColor: const Color(0xFF1A3CBA),
                            onChanged: (v) => setState(
                                () => _subLessonGenerateCounts[id] = v.round()),
                          ),
                          if (count == 0)
                            Padding(
                              padding: const EdgeInsets.only(bottom: 4),
                              child: Text('Skip — no questions for this sub-lesson',
                                  style: TextStyle(
                                      color: Colors.orange.shade700,
                                      fontSize: 11)),
                            ),
                        ],
                      ),
                    );
                  }),
                  const SizedBox(height: 4),
                ] else ...[
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Text('Questions to Generate',
                          style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 4),
                        decoration: BoxDecoration(
                          color: const Color(0xFF1A3CBA).withOpacity(0.1),
                          borderRadius: BorderRadius.circular(20),
                        ),
                        child: Text('$_generateCount',
                            style: const TextStyle(
                                fontSize: 18,
                                fontWeight: FontWeight.bold,
                                color: Color(0xFF1A3CBA))),
                      ),
                    ],
                  ),
                  Slider(
                    value: _generateCount.toDouble(),
                    min: 0,
                    max: 10,
                    divisions: 10,
                    activeColor: const Color(0xFF1A3CBA),
                    onChanged: (v) => setState(() => _generateCount = v.round()),
                  ),
                  const SizedBox(height: 12),
                ],

                // Last generation accuracy summary
                if (_lastGenerationStats != null) _buildAccuracySummary(),
                if (_lastGenerationStats != null) const SizedBox(height: 16),

                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton.icon(
                    onPressed: _isGenerating ? null : _generateQuestions,
                    icon: _isGenerating
                        ? const SizedBox(
                            width: 18, height: 18,
                            child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                        : const Icon(Icons.auto_awesome, color: Colors.white),
                    label: Text(
                      _isGenerating ? 'Generating...' : 'Generate Questions',
                      style: const TextStyle(color: Colors.white, fontSize: 15),
                    ),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: const Color(0xFF1A3CBA),
                      padding: const EdgeInsets.symmetric(vertical: 14),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                    ),
                  ),
                ),
                const SizedBox(height: 24),

                _buildModelComparisonTable(),
              ],
            ),
          ),
        ],
      ),
    );
  }

  // ---------------------------------------------------------------------------
  // Questions Tab
  // ---------------------------------------------------------------------------

  Widget _buildQuestionsTab() {
    return Column(
      children: [
        // Sub-lesson filter chips (only shown when sub-lessons exist)
        if (_subLessons.isNotEmpty)
          Container(
            color: Colors.white,
            padding: const EdgeInsets.symmetric(vertical: 10),
            child: SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              padding: const EdgeInsets.symmetric(horizontal: 16),
              child: Row(
                children: [
                  _filterChip('All', null),
                  ..._subLessons.map((sl) => Padding(
                        padding: const EdgeInsets.only(left: 8),
                        child: _filterChip(
                          '${sl['number']} ${sl['title']}',
                          sl['id'] as String,
                        ),
                      )),
                ],
              ),
            ),
          ),

        // Questions stream
        Expanded(
          child: StreamBuilder<QuerySnapshot>(
            stream: FirebaseFirestore.instance
                .collection('lessons')
                .doc(widget.lessonId)
                .collection('questions')
                .orderBy('createdAt', descending: true)
                .snapshots(),
            builder: (context, snap) {
              if (!snap.hasData) {
                return Center(
                  child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
                    Icon(Icons.quiz_outlined, size: 64, color: Colors.grey.shade300),
                    const SizedBox(height: 12),
                    Text('Loading questions...',
                        style: TextStyle(color: Colors.grey.shade400)),
                  ]),
                );
              }

              // Apply sub-lesson filter client-side
              final allDocs = snap.data!.docs;
              final docs = _selectedSubLessonFilter == null
                  ? allDocs
                  : allDocs.where((d) {
                      final data = d.data() as Map<String, dynamic>;
                      return data['subLessonId'] == _selectedSubLessonFilter;
                    }).toList();

              if (docs.isEmpty) {
                return Center(
                  child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
                    Icon(Icons.quiz_outlined, size: 64, color: Colors.grey.shade300),
                    const SizedBox(height: 12),
                    Text(
                      _selectedSubLessonFilter != null
                          ? 'No questions for this sub-lesson yet.'
                          : 'No questions yet.',
                      style: TextStyle(color: Colors.grey.shade500),
                    ),
                  ]),
                );
              }

              return ListView.builder(
                padding: const EdgeInsets.all(16),
                itemCount: docs.length,
                itemBuilder: (ctx, i) {
                  final d = docs[i].data() as Map<String, dynamic>;
                  final options = (d['options'] as List?) ?? [];
                  final subNumber = d['subLessonNumber'] as String?;

                  return Card(
                    margin: const EdgeInsets.only(bottom: 14),
                    shape:
                        RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          // Top row: Q number, accuracy badge, sub-lesson badge, delete
                          Row(children: [
                            Container(
                              padding: const EdgeInsets.symmetric(
                                  horizontal: 8, vertical: 3),
                              decoration: BoxDecoration(
                                color: const Color(0xFF1A3CBA).withOpacity(0.1),
                                borderRadius: BorderRadius.circular(6),
                              ),
                              child: Text('Q${i + 1}',
                                  style: const TextStyle(
                                      color: Color(0xFF1A3CBA),
                                      fontWeight: FontWeight.bold,
                                      fontSize: 12)),
                            ),
                            const SizedBox(width: 8),
                            _accuracyBadge(d['accuracyScore'], d['accuracyVerified']),
                            if (subNumber != null) ...[
                              const SizedBox(width: 8),
                              _subLessonBadge(subNumber),
                            ],
                            const Spacer(),
                            IconButton(
                              icon: const Icon(Icons.delete_outline,
                                  color: Colors.red, size: 20),
                              onPressed: () => docs[i].reference.delete(),
                              padding: EdgeInsets.zero,
                              constraints: const BoxConstraints(),
                            ),
                          ]),
                          const SizedBox(height: 6),

                          // Difficulty badge + attempts
                          Row(children: [
                            _difficultyBadge(
                              d['difficulty'] as String?,
                              d['actualDifficulty'] as String?,
                              d['difficultyMatch'] as bool?,
                            ),
                            const SizedBox(width: 8),
                            Text(
                              '${d['attempts'] ?? 0} attempt${(d['attempts'] ?? 0) == 1 ? '' : 's'}',
                              style: TextStyle(
                                  color: Colors.grey.shade400, fontSize: 10),
                            ),
                          ]),
                          const SizedBox(height: 10),

                          // Question text
                          Text(d['question'] ?? '',
                              style: const TextStyle(
                                  fontWeight: FontWeight.w600, fontSize: 15)),
                          const SizedBox(height: 10),

                          // Options
                          ...options.map((opt) {
                            final label = opt['label'] ?? '';
                            final isCorrect = label == d['correct'];
                            return Container(
                              margin: const EdgeInsets.only(bottom: 6),
                              padding: const EdgeInsets.symmetric(
                                  horizontal: 12, vertical: 8),
                              decoration: BoxDecoration(
                                color: isCorrect
                                    ? Colors.green.withOpacity(0.08)
                                    : Colors.grey.shade50,
                                borderRadius: BorderRadius.circular(8),
                                border: Border.all(
                                  color: isCorrect
                                      ? Colors.green
                                      : Colors.grey.shade200,
                                ),
                              ),
                              child: Row(children: [
                                Text(label,
                                    style: TextStyle(
                                        fontWeight: FontWeight.bold,
                                        color: isCorrect
                                            ? Colors.green
                                            : Colors.grey.shade600)),
                                const SizedBox(width: 10),
                                Expanded(child: Text(opt['text'] ?? '')),
                                if (isCorrect)
                                  const Icon(Icons.check,
                                      color: Colors.green, size: 16),
                              ]),
                            );
                          }),

                          // Explanation
                          if ((d['explanation'] ?? '').isNotEmpty) ...[
                            const SizedBox(height: 8),
                            Container(
                              padding: const EdgeInsets.all(10),
                              decoration: BoxDecoration(
                                color: Colors.blue.withOpacity(0.05),
                                borderRadius: BorderRadius.circular(8),
                              ),
                              child: Row(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    const Icon(Icons.info_outline,
                                        color: Colors.blue, size: 16),
                                    const SizedBox(width: 6),
                                    Expanded(
                                      child: Text(d['explanation'] ?? '',
                                          style: const TextStyle(
                                              color: Colors.blue, fontSize: 13)),
                                    ),
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
          ),
        ),
      ],
    );
  }

  // ---------------------------------------------------------------------------
  // Sub-lessons Tab
  // ---------------------------------------------------------------------------

  Widget _buildSubLessonsTab() {
    return Column(
      children: [
        // Header
        Container(
          color: Colors.white,
          padding: const EdgeInsets.fromLTRB(16, 14, 16, 14),
          child: Row(children: [
            const Icon(Icons.layers, color: Color(0xFF1A3CBA), size: 20),
            const SizedBox(width: 8),
            const Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Sub-lessons',
                      style: TextStyle(fontWeight: FontWeight.bold, fontSize: 15)),
                  Text('Add sub-lessons before generating questions.',
                      style: TextStyle(color: Colors.grey, fontSize: 12)),
                ],
              ),
            ),
            ElevatedButton.icon(
              onPressed: _showAddSubLessonDialog,
              icon: const Icon(Icons.add, color: Colors.white, size: 18),
              label: const Text('Add', style: TextStyle(color: Colors.white)),
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFF1A3CBA),
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
              ),
            ),
          ]),
        ),

        // List
        Expanded(
          child: _subLessonsLoading
              ? const Center(child: CircularProgressIndicator())
              : _subLessons.isEmpty
                  ? Center(
                      child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                          Icon(Icons.layers_outlined,
                              size: 64, color: Colors.grey.shade300),
                          const SizedBox(height: 12),
                          Text('No sub-lessons yet.',
                              style: TextStyle(
                                  color: Colors.grey.shade500, fontSize: 15)),
                          const SizedBox(height: 6),
                          Text(
                            'Tap "Add" to create sub-lessons like 4.1, 4.2...',
                            style: TextStyle(
                                color: Colors.grey.shade400, fontSize: 13),
                          ),
                        ]))
                  : ListView.builder(
                      padding: const EdgeInsets.all(16),
                      itemCount: _subLessons.length,
                      itemBuilder: (ctx, i) =>
                          _buildSubLessonCard(_subLessons[i]),
                    ),
        ),
      ],
    );
  }

  Widget _buildSubLessonCard(Map<String, dynamic> sl) {
    final id = sl['id'] as String;
    final number = sl['number'] as String;
    final title = sl['title'] as String;

    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Row(children: [
          // Number badge
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
            decoration: BoxDecoration(
              color: const Color(0xFF1A3CBA).withOpacity(0.1),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Text(number,
                style: const TextStyle(
                    color: Color(0xFF1A3CBA),
                    fontWeight: FontWeight.bold,
                    fontSize: 16)),
          ),
          const SizedBox(width: 14),

          // Title + question count
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(title,
                    style: const TextStyle(
                        fontWeight: FontWeight.w600, fontSize: 14)),
                const SizedBox(height: 4),
                StreamBuilder<QuerySnapshot>(
                  stream: FirebaseFirestore.instance
                      .collection('lessons')
                      .doc(widget.lessonId)
                      .collection('questions')
                      .where('subLessonId', isEqualTo: id)
                      .snapshots(),
                  builder: (ctx, snap) {
                    final count = snap.data?.docs.length ?? 0;
                    return Text('$count question${count == 1 ? '' : 's'}',
                        style: TextStyle(
                            color: Colors.grey.shade500, fontSize: 12));
                  },
                ),
              ],
            ),
          ),

          // View questions button
          TextButton(
            onPressed: () {
              setState(() => _selectedSubLessonFilter = id);
              _tabController.animateTo(1); // go to Questions tab
            },
            style: TextButton.styleFrom(foregroundColor: const Color(0xFF1A3CBA)),
            child: const Text('View', style: TextStyle(fontSize: 13)),
          ),

          // Delete
          IconButton(
            icon: const Icon(Icons.delete_outline, color: Colors.red, size: 20),
            onPressed: () => showDialog(
              context: context,
              builder: (ctx) => AlertDialog(
                title: const Text('Delete Sub-lesson'),
                content: Text('Delete "$number - $title"? '
                    'Questions tagged to this sub-lesson will remain but lose their tag.'),
                actions: [
                  TextButton(
                      onPressed: () => Navigator.pop(ctx),
                      child: const Text('Cancel')),
                  TextButton(
                    onPressed: () {
                      Navigator.pop(ctx);
                      _deleteSubLesson(id);
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
    );
  }

  // ---------------------------------------------------------------------------
  // Settings Tab
  // ---------------------------------------------------------------------------

  Widget _buildSettingsTab() {
    final hasSubLessons = !_subLessonsLoading && _subLessons.isNotEmpty;

    return SingleChildScrollView(
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Show spinner while sub-lessons are loading
          if (_subLessonsLoading) ...[
            const Center(child: Padding(
              padding: EdgeInsets.only(bottom: 20),
              child: CircularProgressIndicator(strokeWidth: 2),
            )),
          ],

          // ── Sub-lesson settings ──────────────────────────────────────────
          if (hasSubLessons) ...[
            _sectionHeader('Sub-lesson Quiz Settings', Icons.layers, Colors.indigo),
            const SizedBox(height: 6),
            Text('Set how many questions to show and whether each sub-lesson is published.',
                style: TextStyle(color: Colors.grey.shade600, fontSize: 12)),
            const SizedBox(height: 14),
            SizedBox(
              height: 300,
              child: Scrollbar(
                controller: _subLessonSettingsScroll,
                child: ListView(
                  controller: _subLessonSettingsScroll,
                  padding: const EdgeInsets.only(right: 8),
                  children: _subLessons.map((sl) {
                    final id = sl['id'] as String;
                    final quizCount = _subLessonQuizCounts[id] ?? 5;
                    final published = _subLessonPublished[id] ?? false;
                    return Container(
                      margin: const EdgeInsets.only(bottom: 14),
                      padding: const EdgeInsets.all(16),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(
                          color: published ? Colors.green.shade300 : Colors.grey.shade200,
                        ),
                      ),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(children: [
                            _subLessonBadge(sl['number'] as String),
                            const SizedBox(width: 8),
                            Expanded(
                              child: Text(sl['title'] as String,
                                  style: const TextStyle(
                                      fontWeight: FontWeight.w600, fontSize: 13),
                                  overflow: TextOverflow.ellipsis),
                            ),
                            Switch(
                              value: published,
                              activeColor: Colors.green,
                              onChanged: (v) =>
                                  setState(() => _subLessonPublished[id] = v),
                            ),
                          ]),
                          const SizedBox(height: 4),
                          Text(
                            published ? 'Published — students can take this quiz' : 'Unpublished',
                            style: TextStyle(
                                fontSize: 11,
                                color: published ? Colors.green : Colors.grey.shade500),
                          ),
                          const SizedBox(height: 12),
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Text('Questions per attempt',
                                  style: TextStyle(
                                      fontSize: 12, color: Colors.grey.shade700)),
                              Container(
                                padding: const EdgeInsets.symmetric(
                                    horizontal: 10, vertical: 3),
                                decoration: BoxDecoration(
                                  color: const Color(0xFF1A3CBA).withOpacity(0.1),
                                  borderRadius: BorderRadius.circular(14),
                                ),
                                child: Text('$quizCount',
                                    style: const TextStyle(
                                        fontSize: 15,
                                        fontWeight: FontWeight.bold,
                                        color: Color(0xFF1A3CBA))),
                              ),
                            ],
                          ),
                          Slider(
                            value: quizCount.toDouble(),
                            min: 0,
                            max: 10,
                            divisions: 10,
                            activeColor: Colors.orange,
                            onChanged: (v) =>
                                setState(() => _subLessonQuizCounts[id] = v.round()),
                          ),
                        ],
                      ),
                    );
                  }).toList(),
                ),
              ),
            ),
            const SizedBox(height: 8),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: _subLessonSaving ? null : _saveSubLessonSettings,
                icon: _subLessonSaving
                    ? const SizedBox(
                        width: 18, height: 18,
                        child: CircularProgressIndicator(
                            color: Colors.white, strokeWidth: 2))
                    : const Icon(Icons.save, color: Colors.white),
                label: Text(
                    _subLessonSaving ? 'Saving...' : 'Save Sub-lesson Settings',
                    style: const TextStyle(color: Colors.white, fontSize: 15)),
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.indigo,
                  padding: const EdgeInsets.symmetric(vertical: 14),
                  shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12)),
                ),
              ),
            ),
            const SizedBox(height: 30),
            const Divider(),
            const SizedBox(height: 20),
          ],

          // ── Global lesson settings ────────────────────────────────────────
          if (!hasSubLessons) ...[
            // Publish toggle (only shown when no sub-lessons)
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: _isPublished
                    ? Colors.green.withOpacity(0.08)
                    : Colors.grey.shade50,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(
                  color: _isPublished ? Colors.green : Colors.grey.shade300,
                ),
              ),
              child: Row(children: [
                Icon(
                  _isPublished ? Icons.visibility : Icons.visibility_off,
                  color: _isPublished ? Colors.green : Colors.grey,
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          _isPublished ? 'Quiz Published' : 'Quiz Unpublished',
                          style: TextStyle(
                              fontWeight: FontWeight.bold,
                              color: _isPublished
                                  ? Colors.green
                                  : Colors.grey.shade700),
                        ),
                        Text(
                          _isPublished
                              ? 'Students can see and take this quiz.'
                              : 'Students cannot see this quiz yet.',
                          style: TextStyle(
                              fontSize: 12, color: Colors.grey.shade600),
                        ),
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
          ],

          _sectionHeader('Quiz Mode', Icons.tune, Colors.purple),
          const SizedBox(height: 12),
          _modeCard(
            label: 'Random Shuffle',
            subtitle: 'Pick N random questions each time student takes quiz.',
            value: 'random',
            color: Colors.purple,
          ),
          _modeCard(
            label: 'Manual Selection',
            subtitle: 'Handpick exactly which questions students will see.',
            value: 'manual',
            color: Colors.blue,
          ),
          const SizedBox(height: 24),

          if (_quizMode == 'random' && !hasSubLessons) ...[
            _sectionHeader(
                'Questions per Attempt', Icons.format_list_numbered, Colors.orange),
            const SizedBox(height: 12),
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                  color: Colors.white, borderRadius: BorderRadius.circular(12)),
              child: Column(children: [
                Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
                  const Text('Questions:',
                      style: TextStyle(fontWeight: FontWeight.w600)),
                  Text('$_questionCount',
                      style: const TextStyle(
                          fontSize: 22,
                          fontWeight: FontWeight.bold,
                          color: Color(0xFF1A3CBA))),
                ]),
                Slider(
                  value: _questionCount.toDouble(),
                  min: 0,
                  max: 10,
                  divisions: 10,
                  activeColor: Colors.orange,
                  onChanged: (v) => setState(() => _questionCount = v.round()),
                ),
              ]),
            ),
          ],

          if (_quizMode == 'manual') ...[
            _sectionHeader('Select Questions', Icons.checklist, Colors.blue),
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
                if (docs.isEmpty) return const Text('No questions yet.');
                final items = docs.asMap().entries.map((entry) {
                  final i   = entry.key;
                  final doc = entry.value;
                  final d   = doc.data() as Map<String, dynamic>;
                  final isSelected = _selectedQuestionIds.contains(doc.id);
                  final subNum = d['subLessonNumber'] as String?;
                  return Container(
                    margin: const EdgeInsets.only(bottom: 8),
                    decoration: BoxDecoration(
                      color: isSelected
                          ? Colors.blue.withOpacity(0.06)
                          : Colors.white,
                      borderRadius: BorderRadius.circular(10),
                      border: Border.all(
                          color: isSelected
                              ? Colors.blue
                              : Colors.grey.shade200),
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
                      title: Row(children: [
                        if (subNum != null) ...[
                          _subLessonBadge(subNum),
                          const SizedBox(width: 6),
                        ],
                        Expanded(
                          child: Text('Q${i + 1}: ${d['question'] ?? ''}',
                              style: const TextStyle(fontSize: 13),
                              maxLines: 2,
                              overflow: TextOverflow.ellipsis),
                        ),
                      ]),
                      subtitle: Text('Correct: ${d['correct']}',
                          style: TextStyle(
                              fontSize: 11, color: Colors.grey.shade500)),
                      controlAffinity: ListTileControlAffinity.leading,
                    ),
                  );
                }).toList();
                return SizedBox(
                  height: 320,
                  child: Scrollbar(
                    controller: _manualQScroll,
                    child: ListView(
                      controller: _manualQScroll,
                      padding: const EdgeInsets.only(right: 8),
                      children: items,
                    ),
                  ),
                );
              },
            ),
            const SizedBox(height: 8),
            Text('${_selectedQuestionIds.length} questions selected',
                style: const TextStyle(
                    fontWeight: FontWeight.w600, color: Colors.blue)),
          ],

          const SizedBox(height: 28),

          SizedBox(
            width: double.infinity,
            child: ElevatedButton.icon(
              onPressed: _settingsSaving ? null : _saveSettings,
              icon: _settingsSaving
                  ? const SizedBox(
                      width: 18, height: 18,
                      child: CircularProgressIndicator(
                          color: Colors.white, strokeWidth: 2))
                  : const Icon(Icons.save, color: Colors.white),
              label: Text(_settingsSaving ? 'Saving...' : 'Save Settings',
                  style: const TextStyle(color: Colors.white, fontSize: 16)),
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFF1A3CBA),
                padding: const EdgeInsets.symmetric(vertical: 14),
                shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12)),
              ),
            ),
          ),
        ],
      ),
    );
  }

  // ---------------------------------------------------------------------------
  // Shared widgets
  // ---------------------------------------------------------------------------

  Widget _filterChip(String label, String? subLessonId) {
    final selected = _selectedSubLessonFilter == subLessonId;
    return GestureDetector(
      onTap: () => setState(() => _selectedSubLessonFilter = subLessonId),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 6),
        decoration: BoxDecoration(
          color: selected
              ? const Color(0xFF1A3CBA).withOpacity(0.12)
              : Colors.grey.shade100,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(
            color: selected ? const Color(0xFF1A3CBA) : Colors.grey.shade300,
            width: selected ? 2 : 1,
          ),
        ),
        child: Text(
          label,
          style: TextStyle(
            color: selected ? const Color(0xFF1A3CBA) : Colors.grey.shade600,
            fontWeight: selected ? FontWeight.bold : FontWeight.normal,
            fontSize: 12,
          ),
        ),
      ),
    );
  }

  Widget _subLessonBadge(String number) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
      decoration: BoxDecoration(
        color: Colors.indigo.withOpacity(0.1),
        borderRadius: BorderRadius.circular(6),
        border: Border.all(color: Colors.indigo.withOpacity(0.35)),
      ),
      child: Text(number,
          style: const TextStyle(
              color: Colors.indigo, fontWeight: FontWeight.bold, fontSize: 11)),
    );
  }

  Widget _accuracyBadge(dynamic score, dynamic verified) {
    if (score == null || verified != true) {
      return Container(
        padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
        decoration: BoxDecoration(
          color: Colors.grey.shade200,
          borderRadius: BorderRadius.circular(8),
        ),
        child: Text('—', style: TextStyle(color: Colors.grey.shade500, fontSize: 11)),
      );
    }
    final s = (score as num).toInt();
    final color = s >= 80 ? Colors.green : s >= 50 ? Colors.orange : Colors.red;
    final icon = s >= 80 ? '✓' : s >= 50 ? '~' : '!';
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: color.withOpacity(0.3)),
      ),
      child: Text('$icon $s%',
          style: TextStyle(color: color, fontSize: 11, fontWeight: FontWeight.bold)),
    );
  }

  Widget _difficultyBadge(String? predicted, String? actual, bool? match) {
    if (predicted == null) return const SizedBox.shrink();
    Color diffColor(String d) =>
        d == 'Easy' ? Colors.green : d == 'Hard' ? Colors.red : Colors.orange;

    if (actual == null) {
      final c = diffColor(predicted);
      return Container(
        padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
        decoration: BoxDecoration(
          color: c.withOpacity(0.08),
          borderRadius: BorderRadius.circular(8),
          border: Border.all(color: c.withOpacity(0.25)),
        ),
        child: Text(predicted,
            style: TextStyle(
                color: c.withOpacity(0.75),
                fontSize: 10,
                fontWeight: FontWeight.w600)),
      );
    }

    final matched = match == true;
    final badgeColor = matched ? Colors.green : Colors.red;
    final icon = matched ? '✓' : '≠';
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
      decoration: BoxDecoration(
        color: badgeColor.withOpacity(0.1),
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: badgeColor.withOpacity(0.4)),
      ),
      child: Text('$predicted $icon $actual',
          style: TextStyle(
              color: matched ? Colors.green.shade700 : Colors.red.shade700,
              fontSize: 10,
              fontWeight: FontWeight.bold)),
    );
  }

  Widget _buildAccuracySummary() {
    final stats = _lastGenerationStats!;
    final avg = stats['avgAccuracyScore'] as int? ?? 0;
    final verified = stats['verifiedCount'] as int? ?? 0;
    final flagged = stats['flaggedCount'] as int? ?? 0;
    final generated = stats['generated'] as int? ?? 0;
    final subCount = stats['subLessonCount'] as int? ?? 0;
    final color = avg >= 80 ? Colors.green : avg >= 50 ? Colors.orange : Colors.red;
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: color.withOpacity(0.08),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: color.withOpacity(0.3)),
      ),
      child: Row(children: [
        Icon(Icons.verified, color: color, size: 22),
        const SizedBox(width: 10),
        Expanded(
          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Text('Last Generation Accuracy',
                style: TextStyle(
                    fontWeight: FontWeight.bold, color: color, fontSize: 13)),
            const SizedBox(height: 2),
            Text(
              '$generated questions'
              '${subCount > 0 ? ' across $subCount sub-lessons' : ''}'
              '  ·  Avg: $avg%  ·  $verified verified  ·  $flagged flagged',
              style: TextStyle(color: color.withOpacity(0.8), fontSize: 12),
            ),
          ]),
        ),
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
          decoration: BoxDecoration(
            color: color.withOpacity(0.15),
            borderRadius: BorderRadius.circular(20),
          ),
          child: Text('$avg%',
              style: TextStyle(
                  color: color, fontWeight: FontWeight.bold, fontSize: 16)),
        ),
      ]),
    );
  }

  Widget _buildModelComparisonTable() {
    if (_sessionsLoading) {
      return const Center(
          child: Padding(
              padding: EdgeInsets.all(16),
              child: CircularProgressIndicator(strokeWidth: 2)));
    }
    if (_sessionHistory.isEmpty) return const SizedBox.shrink();

    final grouped = <String, List<Map<String, dynamic>>>{};
    for (final s in _sessionHistory) {
      final model = s['model'] as String? ?? 'unknown';
      grouped.putIfAbsent(model, () => []).add(s);
    }

    return Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      Row(children: [
        const Icon(Icons.bar_chart, color: Color(0xFF1A3CBA), size: 18),
        const SizedBox(width: 6),
        const Text('Model Accuracy Comparison',
            style: TextStyle(fontWeight: FontWeight.bold, fontSize: 14)),
        const Spacer(),
        GestureDetector(
          onTap: _loadSessionHistory,
          child: Icon(Icons.refresh, size: 18, color: Colors.grey.shade500),
        ),
      ]),
      const SizedBox(height: 10),
      ...grouped.entries.map((entry) {
        final model = entry.key;
        final sessions = entry.value;
        final scores = sessions
            .where((s) => s['avgAccuracyScore'] != null)
            .map((s) => (s['avgAccuracyScore'] as num).toInt())
            .toList();
        final avg = scores.isEmpty
            ? 0
            : scores.reduce((a, b) => a + b) ~/ scores.length;
        final best =
            scores.isEmpty ? 0 : scores.reduce((a, b) => a > b ? a : b);
        final totalQ = sessions.fold<int>(
            0, (sum, s) => sum + ((s['questionCount'] as int?) ?? 0));
        final color = _modelColor(model);
        final barColor =
            avg >= 80 ? Colors.green : avg >= 50 ? Colors.orange : Colors.red;

        return Container(
          margin: const EdgeInsets.only(bottom: 10),
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(10),
            border: Border.all(color: Colors.grey.shade200),
          ),
          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Row(children: [
              Container(
                padding:
                    const EdgeInsets.symmetric(horizontal: 10, vertical: 3),
                decoration: BoxDecoration(
                  color: color.withOpacity(0.1),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Text(_modelDisplayName(model),
                    style: TextStyle(
                        color: color,
                        fontWeight: FontWeight.bold,
                        fontSize: 12)),
              ),
              const Spacer(),
              Text(
                  '${sessions.length} session${sessions.length > 1 ? 's' : ''}  ·  $totalQ questions',
                  style:
                      TextStyle(color: Colors.grey.shade500, fontSize: 11)),
            ]),
            const SizedBox(height: 8),
            Row(children: [
              Expanded(
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(4),
                  child: LinearProgressIndicator(
                    value: avg / 100,
                    backgroundColor: Colors.grey.shade200,
                    color: barColor,
                    minHeight: 8,
                  ),
                ),
              ),
              const SizedBox(width: 10),
              Text('$avg%',
                  style: TextStyle(
                      fontWeight: FontWeight.bold,
                      color: barColor,
                      fontSize: 14)),
            ]),
            const SizedBox(height: 4),
            Text('Best: $best%',
                style: TextStyle(color: Colors.grey.shade500, fontSize: 11)),
            Builder(builder: (_) {
              final ds = _difficultyStats[model];
              if (ds == null || (ds['evaluated'] ?? 0) == 0) {
                return const SizedBox.shrink();
              }
              final evaluated = ds['evaluated']!;
              final matched = ds['matched']!;
              final diffPct = (matched / evaluated * 100).round();
              final diffColor = diffPct >= 70
                  ? Colors.green.shade700
                  : diffPct >= 40
                      ? Colors.orange.shade700
                      : Colors.red.shade700;
              return Padding(
                padding: const EdgeInsets.only(top: 4),
                child: Row(children: [
                  Icon(Icons.psychology, size: 12, color: Colors.grey.shade500),
                  const SizedBox(width: 4),
                  Text(
                    'Difficulty accuracy: $diffPct% ($matched/$evaluated evaluated)',
                    style: TextStyle(
                        color: diffColor,
                        fontSize: 11,
                        fontWeight: FontWeight.w600),
                  ),
                ]),
              );
            }),
          ]),
        );
      }),
    ]);
  }

  String _modelDisplayName(String model) {
    switch (model) {
      case 'openrouter':
        return 'DeepSeek';
      case 'groq':
        return 'Groq';
      case 'mistral':
        return 'Mistral';
      default:
        return model;
    }
  }

  Color _modelColor(String model) {
    switch (model) {
      case 'openrouter':
        return Colors.teal;
      case 'groq':
        return Colors.orange;
      case 'mistral':
        return Colors.purple;
      default:
        return Colors.blue;
    }
  }

  Widget _modelChip(String value, String label, Color color) {
    final selected = _selectedModel == value;
    return GestureDetector(
      onTap: () => setState(() => _selectedModel = value),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        decoration: BoxDecoration(
          color: selected ? color.withOpacity(0.1) : Colors.grey.shade100,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(
              color: selected ? color : Colors.grey.shade300,
              width: selected ? 2 : 1),
        ),
        child: Row(mainAxisSize: MainAxisSize.min, children: [
          if (selected) ...[
            Icon(Icons.check_circle, color: color, size: 14),
            const SizedBox(width: 4),
          ],
          Text(label,
              style: TextStyle(
                  color: selected ? color : Colors.grey.shade600,
                  fontWeight: selected ? FontWeight.bold : FontWeight.normal,
                  fontSize: 13)),
        ]),
      ),
    );
  }

  Widget _modeCard({
    required String label,
    required String subtitle,
    required String value,
    required Color color,
  }) {
    final isSelected = _quizMode == value;
    return GestureDetector(
      onTap: () => setState(() => _quizMode = value),
      child: Container(
        padding: const EdgeInsets.all(16),
        margin: const EdgeInsets.only(bottom: 10),
        decoration: BoxDecoration(
          color: isSelected ? color.withOpacity(0.08) : Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
              color: isSelected ? color : Colors.grey.shade200,
              width: isSelected ? 2 : 1),
        ),
        child: Row(children: [
          Radio<String>(
            value: value,
            groupValue: _quizMode,
            activeColor: color,
            onChanged: (v) => setState(() => _quizMode = v!),
          ),
          const SizedBox(width: 8),
          Expanded(
            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text(label,
                  style: const TextStyle(fontWeight: FontWeight.bold)),
              Text(subtitle,
                  style:
                      TextStyle(fontSize: 12, color: Colors.grey.shade600)),
            ]),
          ),
        ]),
      ),
    );
  }

  Widget _sectionHeader(String title, IconData icon, Color color) {
    return Row(children: [
      Icon(icon, color: color, size: 20),
      const SizedBox(width: 8),
      Text(title,
          style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15)),
    ]);
  }
}
