import 'dart:async';
import 'dart:convert';
import 'dart:typed_data';

import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:http/http.dart' as http;

import '../models/lesson_material.dart';
import '../services/materials_service.dart';
import '../services/cloudinary_service.dart';

class AdminMaterialsScreen extends StatefulWidget {
  const AdminMaterialsScreen({super.key});

  @override
  State<AdminMaterialsScreen> createState() => _AdminMaterialsScreenState();
}

class _AdminMaterialsScreenState extends State<AdminMaterialsScreen>
    with SingleTickerProviderStateMixin {
  final MaterialsService _service = MaterialsService();
  late TabController _tabController;

  static const _grades = ['Grade 9', 'Grade 10', 'Grade 11'];
  static const _primaryBlue = Color(0xFF1A3CBA);

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: _grades.length, vsync: this);
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  // ─── File picker ─────────────────────────────────────────────────────────────
  Future<_PickedFile?> _pickFile() async {
    try {
      final result = await FilePicker.platform.pickFiles(
        type: FileType.custom,
        allowedExtensions: const [
          'pdf',
          'jpg',
          'jpeg',
          'png',
          'mp4',
          'doc',
          'docx',
          'ppt',
          'pptx',
        ],
        withData: true,
      );

      if (result == null || result.files.isEmpty) return null;

      final file = result.files.first;
      final bytes = file.bytes;

      if (bytes == null) return null;

      return _PickedFile(
        name: file.name,
        bytes: bytes,
        size: file.size,
      );
    } catch (e) {
      debugPrint('Error picking file: $e');
      return null;
    }
  }

  // ─── Load lessons (Backend API with Firestore fallback) ─────────────────────
  static const _backendUrl = 'http://localhost:9000';

  Future<bool> _indexPdfForSearch({
    required String lessonId,
    required String materialId,
    required _PickedFile file,
  }) async {
    if (!file.name.toLowerCase().endsWith('.pdf')) return true;

    try {
      final token =
          await FirebaseAuth.instance.currentUser?.getIdToken();

      final request = http.MultipartRequest(
        'POST',
        Uri.parse(
          '$_backendUrl/admin/lessons/$lessonId/materials/$materialId/search-index',
        ),
      )
        ..headers['Authorization'] = 'Bearer $token'
        ..files.add(
          http.MultipartFile.fromBytes(
            'file',
            file.bytes,
            filename: file.name,
          ),
        );

      final response =
          await request.send().timeout(const Duration(seconds: 120));

      return response.statusCode == 200;
    } catch (error) {
      debugPrint('PDF search indexing failed: $error');
      return false;
    }
  }

  Future<List<Map<String, dynamic>>> _fetchLessonsForGrade(
      String grade) async {
    final gradeNum =
        int.tryParse(grade.replaceAll(RegExp(r'[^0-9]'), '')) ?? 0;

    List<Map<String, dynamic>> lessons = [];

    // 1. Try Backend API (if running)
    try {
      final token =
          await FirebaseAuth.instance.currentUser?.getIdToken();

      final response = await http.get(
        Uri.parse('$_backendUrl/admin/lessons'),
        headers: {
          'Authorization': 'Bearer $token',
        },
      ).timeout(const Duration(seconds: 3));

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body) as List;
        final all = data.cast<Map<String, dynamic>>();

        lessons = all
            .where(
              (l) =>
                  l['grade'] == gradeNum ||
                  l['grade'] == grade ||
                  '${l['grade']}' == '$gradeNum',
            )
            .toList();
      }
    } catch (_) {
      // Backend not running (e.g. ERR_CONNECTION_REFUSED)
      // — will fallback to Firestore
    }

    if (lessons.isNotEmpty) {
      lessons.sort(
        (a, b) => (a['title'] as String? ?? '')
            .compareTo(b['title'] as String? ?? ''),
      );

      return lessons;
    }

    // 2. Fallback to Firestore collection('lessons')
    try {
      final snap1 = await FirebaseFirestore.instance
          .collection('lessons')
          .where('grade', isEqualTo: gradeNum)
          .get();

      final snap2 = await FirebaseFirestore.instance
          .collection('lessons')
          .where('grade', isEqualTo: grade)
          .get();

      final docsMap = <String, Map<String, dynamic>>{};

      for (final doc in [...snap1.docs, ...snap2.docs]) {
        final data = doc.data();

        docsMap[doc.id] = {
          'id': doc.id,
          'title': data['title'] as String? ?? doc.id,
          'grade': data['grade'],
          'lessonTag': data['lessonTag'] ?? doc.id,
        };
      }

      lessons = docsMap.values.toList();

      lessons.sort(
        (a, b) => (a['title'] as String? ?? '')
            .compareTo(b['title'] as String? ?? ''),
      );
    } catch (e) {
      print('Error fetching lessons from Firestore: $e');
    }

    return lessons;
  }

  // ─── Upload dialog ────────────────────────────────────────────────────────────
  Future<void> _showUploadDialog(String grade) async {
    // Show loading dialog while fetching lessons
    List<Map<String, dynamic>> lessons = [];
    bool loadingLessons = true;
    String? lessonLoadError;

    String? selectedLessonId;
    String? selectedLessonTitle;
    String description = '';
    bool uploading = false;
    _PickedFile? pickedFile;
    String? uploadError;

    // Fetch lessons in background then update dialog
    _fetchLessonsForGrade(grade).then((result) {
      lessons = result;
      loadingLessons = false;

      if (result.isEmpty) {
        lessonLoadError =
            'No lessons found for $grade. Add lessons in the Lessons tab first.';
      }
    }).catchError((e) {
      loadingLessons = false;
      lessonLoadError = 'Failed to load lessons: $e';
    });

    await showDialog(
      context: context,
      barrierDismissible: false,
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setDialogState) {
          // Kick off lesson fetch once dialog is open
          // — uses setDialogState to rebuild
          if (loadingLessons) {
            _fetchLessonsForGrade(grade).then((result) {
              lessons = result;
              loadingLessons = false;

              if (result.isEmpty) {
                lessonLoadError =
                    'No lessons found for $grade. Add lessons in the Lessons tab first.';
              }

              if (ctx.mounted) {
                setDialogState(() {});
              }
            }).catchError((e) {
              loadingLessons = false;
              lessonLoadError = 'Failed to load lessons: $e';

              if (ctx.mounted) {
                setDialogState(() {});
              }
            });
          }

          return AlertDialog(
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(16),
            ),
            titlePadding: const EdgeInsets.fromLTRB(20, 20, 20, 0),
            contentPadding: const EdgeInsets.fromLTRB(20, 16, 20, 0),
            title: Row(
              children: [
                Container(
                  padding: const EdgeInsets.all(8),
                  decoration: BoxDecoration(
                    color: _primaryBlue.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: const Icon(
                    Icons.upload_file,
                    color: _primaryBlue,
                    size: 20,
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: Text(
                    'Upload Material — $grade',
                    style: const TextStyle(
                      fontSize: 15,
                      fontWeight: FontWeight.bold,
                    ),
                    overflow: TextOverflow.ellipsis,
                  ),
                ),
              ],
            ),
            content: SizedBox(
              width: 420,
              child: SingleChildScrollView(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const SizedBox(height: 4),

                    // ── Lesson dropdown ──────────────────────────
                    _label('Select Lesson *'),
                    const SizedBox(height: 6),

                    if (loadingLessons)
                      const SizedBox(
                        height: 48,
                        child: Center(
                          child: Row(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              SizedBox(
                                width: 16,
                                height: 16,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2,
                                  color: _primaryBlue,
                                ),
                              ),
                              SizedBox(width: 10),
                              Text(
                                'Loading lessons…',
                                style: TextStyle(
                                  fontSize: 13,
                                  color: Colors.grey,
                                ),
                              ),
                            ],
                          ),
                        ),
                      )
                    else if (lessonLoadError != null)
                      Container(
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          border:
                              Border.all(color: Colors.orange.shade300),
                          borderRadius: BorderRadius.circular(10),
                          color: Colors.orange.shade50,
                        ),
                        child: Row(
                          children: [
                            Icon(
                              Icons.warning_amber,
                              color: Colors.orange.shade700,
                              size: 18,
                            ),
                            const SizedBox(width: 8),
                            Expanded(
                              child: Text(
                                lessonLoadError!,
                                style: TextStyle(
                                  fontSize: 12,
                                  color: Colors.orange.shade800,
                                ),
                              ),
                            ),
                          ],
                        ),
                      )
                    else
                      DropdownButtonFormField<String>(
                        value: selectedLessonId,
                        isExpanded: true,
                        decoration: InputDecoration(
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(10),
                          ),
                          contentPadding: const EdgeInsets.symmetric(
                            horizontal: 12,
                            vertical: 12,
                          ),
                          hintText: 'Choose a lesson',
                          hintStyle:
                              TextStyle(color: Colors.grey.shade400),
                        ),
                        items: lessons.map((lesson) {
                          final id = lesson['id'] as String? ??
                              lesson['lessonTag'] as String? ??
                              '';

                          final title =
                              lesson['title'] as String? ?? id;

                          return DropdownMenuItem<String>(
                            value: id,
                            child: Text(
                              title,
                              overflow: TextOverflow.ellipsis,
                              style: const TextStyle(fontSize: 13),
                            ),
                          );
                        }).toList(),
                        onChanged: uploading
                            ? null
                            : (val) {
                                setDialogState(() {
                                  selectedLessonId = val;

                                  selectedLessonTitle =
                                      lessons.firstWhere(
                                    (l) =>
                                        (l['id'] as String? ??
                                            l['lessonTag']) ==
                                        val,
                                    orElse: () => {},
                                  )['title'] as String?;
                                });
                              },
                      ),

                    const SizedBox(height: 16),

                    // ── File picker ──────────────────────────────
                    _label('PDF / File *'),
                    const SizedBox(height: 6),

                    GestureDetector(
                      onTap: uploading
                          ? null
                          : () async {
                              final picked = await _pickFile();

                              if (picked != null) {
                                setDialogState(() {
                                  pickedFile = picked;
                                  uploadError = null;
                                });
                              }
                            },
                      child: AnimatedContainer(
                        duration: const Duration(milliseconds: 200),
                        width: double.infinity,
                        padding: const EdgeInsets.symmetric(
                          horizontal: 16,
                          vertical: 14,
                        ),
                        decoration: BoxDecoration(
                          border: Border.all(
                            color: pickedFile != null
                                ? Colors.green.shade400
                                : _primaryBlue.withOpacity(0.35),
                            width: 1.5,
                          ),
                          borderRadius: BorderRadius.circular(10),
                          color: pickedFile != null
                              ? Colors.green.shade50
                              : _primaryBlue.withOpacity(0.03),
                        ),
                        child: Row(
                          children: [
                            Icon(
                              pickedFile != null
                                  ? Icons.check_circle_rounded
                                  : Icons.attach_file_rounded,
                              color: pickedFile != null
                                  ? Colors.green.shade600
                                  : _primaryBlue,
                              size: 22,
                            ),
                            const SizedBox(width: 10),
                            Expanded(
                              child: Column(
                                crossAxisAlignment:
                                    CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    pickedFile != null
                                        ? pickedFile!.name
                                        : 'Click to select file',
                                    style: TextStyle(
                                      fontWeight: pickedFile != null
                                          ? FontWeight.w600
                                          : FontWeight.normal,
                                      color: pickedFile != null
                                          ? Colors.green.shade700
                                          : Colors.grey.shade600,
                                      fontSize: 13,
                                    ),
                                    overflow: TextOverflow.ellipsis,
                                  ),
                                  if (pickedFile != null)
                                    Text(
                                      _formatBytes(pickedFile!.size),
                                      style: TextStyle(
                                        fontSize: 11,
                                        color: Colors.grey.shade500,
                                      ),
                                    ),
                                  if (pickedFile == null)
                                    Text(
                                      'PDF, Image, Video, Doc, PPT',
                                      style: TextStyle(
                                        fontSize: 11,
                                        color: Colors.grey.shade400,
                                      ),
                                    ),
                                ],
                              ),
                            ),
                            if (pickedFile != null)
                              IconButton(
                                icon: Icon(
                                  Icons.close,
                                  size: 18,
                                  color: Colors.grey.shade400,
                                ),
                                padding: EdgeInsets.zero,
                                constraints:
                                    const BoxConstraints(),
                                onPressed: uploading
                                    ? null
                                    : () => setDialogState(
                                          () => pickedFile = null,
                                        ),
                              ),
                          ],
                        ),
                      ),
                    ),

                    const SizedBox(height: 16),

                    // ── Description ──────────────────────────────
                    _label('Description (optional)'),
                    const SizedBox(height: 6),

                    TextField(
                      enabled: !uploading,
                      decoration: InputDecoration(
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(10),
                        ),
                        hintText:
                            'Brief note about this material…',
                        hintStyle:
                            TextStyle(color: Colors.grey.shade400),
                        contentPadding:
                            const EdgeInsets.symmetric(
                          horizontal: 12,
                          vertical: 10,
                        ),
                      ),
                      style: const TextStyle(fontSize: 13),
                      maxLines: 2,
                      onChanged: (v) => description = v,
                    ),

                    // ── Upload progress ──────────────────────────
                    if (uploading) ...[
                      const SizedBox(height: 16),
                      const LinearProgressIndicator(
                        color: _primaryBlue,
                      ),
                      const SizedBox(height: 6),
                      const Text(
                        'Uploading to Cloudinary…',
                        style: TextStyle(
                          fontSize: 12,
                          color: Colors.grey,
                        ),
                      ),
                    ],

                    // ── Error message ────────────────────────────
                    if (uploadError != null) ...[
                      const SizedBox(height: 12),
                      Container(
                        padding: const EdgeInsets.all(10),
                        decoration: BoxDecoration(
                          color: Colors.red.shade50,
                          border: Border.all(
                            color: Colors.red.shade200,
                          ),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Row(
                          children: [
                            Icon(
                              Icons.error_outline,
                              color: Colors.red.shade600,
                              size: 16,
                            ),
                            const SizedBox(width: 6),
                            Expanded(
                              child: Text(
                                uploadError!,
                                style: TextStyle(
                                  fontSize: 12,
                                  color: Colors.red.shade700,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],

                    const SizedBox(height: 4),
                  ],
                ),
              ),
            ),
            actions: [
              TextButton(
                onPressed:
                    uploading ? null : () => Navigator.pop(ctx),
                child: const Text('Cancel'),
              ),
              ElevatedButton.icon(
                icon: uploading
                    ? const SizedBox(
                        width: 16,
                        height: 16,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          color: Colors.white,
                        ),
                      )
                    : const Icon(
                        Icons.cloud_upload,
                        size: 18,
                      ),
                label: Text(
                  uploading ? 'Uploading…' : 'Upload',
                ),
                style: ElevatedButton.styleFrom(
                  backgroundColor: _primaryBlue,
                  foregroundColor: Colors.white,
                  disabledBackgroundColor:
                      _primaryBlue.withOpacity(0.5),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
                onPressed: uploading
                    ? null
                    : () async {
                        // Validation
                        if (selectedLessonId == null) {
                          setDialogState(
                            () => uploadError =
                                'Please select a lesson first.',
                          );
                          return;
                        }

                        if (pickedFile == null) {
                          setDialogState(
                            () => uploadError =
                                'Please select a file.',
                          );
                          return;
                        }

                        setDialogState(() => uploading = true);

                        try {
                          final fileType =
                              CloudinaryService.getFileType(
                            pickedFile!.name,
                          );

                          // Upload to Cloudinary
                          final result =
                              await CloudinaryService.uploadFile(
                            file: pickedFile!.bytes,
                            fileName: pickedFile!.name,
                            folder: grade.replaceAll(' ', '_'),
                          );

                          if (!result['success']) {
                            setDialogState(() {
                              uploading = false;
                              uploadError =
                                  'Upload failed: ${result['error']}';
                            });
                            return;
                          }

                          // Save metadata to Firestore
                          final material = LessonMaterial(
                            id: '',
                            lessonId: selectedLessonId!,
                            lessonTitle:
                                selectedLessonTitle ??
                                    selectedLessonId!,
                            materialName: pickedFile!.name,
                            materialType: fileType,
                            grade: grade,
                            topic: selectedLessonTitle ?? '',
                            cloudinaryUrl:
                                result['url'] as String? ?? '',
                            cloudinaryPublicId:
                                result['publicId'] as String? ?? '',
                            fileSizeBytes:
                                (result['fileSize'] as num?)
                                        ?.toInt() ??
                                    pickedFile!.size,
                            uploadedBy:
                                FirebaseAuth.instance.currentUser
                                        ?.uid ??
                                    '',
                            uploadedAt: DateTime.now(),
                            description: description,
                          );

                          final materialId =
                              await _service.saveMaterial(material);

                          if (materialId == null) {
                            throw Exception(
                              'Could not save material metadata.',
                            );
                          }

                          final indexed = await _indexPdfForSearch(
                            lessonId: selectedLessonId!,
                            materialId: materialId,
                            file: pickedFile!,
                          );

                          if (ctx.mounted) {
                            Navigator.pop(ctx);
                          }

                          if (mounted) {
                            ScaffoldMessenger.of(context).showSnackBar(
                              SnackBar(
                                content: Row(
                                  children: [
                                    const Icon(
                                      Icons.check_circle,
                                      color: Colors.white,
                                      size: 18,
                                    ),
                                    const SizedBox(width: 8),
                                    Expanded(
                                      child: Text(
                                        indexed
                                            ? '${pickedFile!.name} uploaded and indexed for search!'
                                            : '${pickedFile!.name} uploaded. Search indexing will need a retry.',
                                      ),
                                    ),
                                  ],
                                ),
                                backgroundColor:
                                    Colors.green.shade600,
                              ),
                            );
                          }
                        } catch (e) {
                          setDialogState(() {
                            uploading = false;
                            uploadError = 'Error: $e';
                          });
                        }
                      },
              ),
            ],
          );
        },
      ),
    );
  }

  // ─── Delete confirm ───────────────────────────────────────────────────────────
  Future<void> _deleteMaterial(LessonMaterial m) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Delete Material?'),
        content: Text(
          'Are you sure you want to delete "${m.materialName}"?',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Cancel'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            style: TextButton.styleFrom(
              foregroundColor: Colors.red,
            ),
            child: const Text('Delete'),
          ),
        ],
      ),
    );

    if (confirm == true) {
      await _service.deleteMaterial(m.id);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Material deleted.'),
            backgroundColor: Colors.red,
          ),
        );
      }
    }
  }

  // ─── Build ────────────────────────────────────────────────────────────────────
  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        // Grade tabs
        Container(
          color: Colors.white,
          child: TabBar(
            controller: _tabController,
            labelColor: _primaryBlue,
            unselectedLabelColor: Colors.grey,
            indicatorColor: _primaryBlue,
            tabs: _grades.map((g) => Tab(text: g)).toList(),
          ),
        ),

        // Tab pages
        Expanded(
          child: TabBarView(
            controller: _tabController,
            children: _grades
                .map(
                  (grade) => _GradeMaterialsTab(
                    grade: grade,
                    service: _service,
                    onUpload: () => _showUploadDialog(grade),
                    onDelete: _deleteMaterial,
                  ),
                )
                .toList(),
          ),
        ),
      ],
    );
  }

  // ─── Helpers ──────────────────────────────────────────────────────────────────
  static Widget _label(String text) => Text(
        text,
        style: const TextStyle(
          fontWeight: FontWeight.w600,
          fontSize: 13,
        ),
      );

  static String _formatBytes(int bytes) {
    if (bytes <= 0) return '';

    if (bytes >= 1024 * 1024) {
      return '${(bytes / (1024 * 1024)).toStringAsFixed(1)} MB';
    }

    return '${(bytes / 1024).toStringAsFixed(0)} KB';
  }
}

// ─── Simple picked file model ─────────────────────────────────────────────────
class _PickedFile {
  final String name;
  final Uint8List bytes;
  final int size;

  const _PickedFile({
    required this.name,
    required this.bytes,
    required this.size,
  });
}

// ─── Per-grade materials tab ──────────────────────────────────────────────────
class _GradeMaterialsTab extends StatefulWidget {
  final String grade;
  final MaterialsService service;
  final Future<void> Function() onUpload;
  final Future<void> Function(LessonMaterial) onDelete;

  const _GradeMaterialsTab({
    required this.grade,
    required this.service,
    required this.onUpload,
    required this.onDelete,
  });

  @override
  State<_GradeMaterialsTab> createState() =>
      _GradeMaterialsTabState();
}

class _GradeMaterialsTabState extends State<_GradeMaterialsTab>
    with AutomaticKeepAliveClientMixin {
  late Future<List<LessonMaterial>> _future;

  @override
  bool get wantKeepAlive => true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  void _load() {
    _future = widget.service.getMaterialsForGrade(widget.grade);
  }

  static const _primaryBlue = Color(0xFF1A3CBA);

  static final _typeColors = <String, Color>{
    'pdf': Colors.red,
    'image': Colors.purple,
    'video': Colors.orange,
    'doc': Colors.blue,
    'ppt': Colors.deepOrange,
    'file': Colors.teal,
  };

  @override
  Widget build(BuildContext context) {
    super.build(context);

    return FutureBuilder<List<LessonMaterial>>(
      future: _future,
      builder: (context, snap) {
        return Scaffold(
          backgroundColor: const Color(0xFFF5F7FA),
          floatingActionButton: FloatingActionButton.extended(
            heroTag: null,
            onPressed: () async {
              await widget.onUpload();
              setState(_load);
            },
            backgroundColor: _primaryBlue,
            icon: const Icon(
              Icons.upload_file,
              color: Colors.white,
            ),
            label: const Text(
              'Upload',
              style: TextStyle(color: Colors.white),
            ),
          ),
          body: _buildBody(snap),
        );
      },
    );
  }

  Widget _buildBody(
    AsyncSnapshot<List<LessonMaterial>> snap,
  ) {
    if (snap.connectionState == ConnectionState.waiting) {
      return const Center(
        child: CircularProgressIndicator(
          color: _primaryBlue,
        ),
      );
    }

    if (snap.hasError) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.error_outline,
              size: 48,
              color: Colors.red.shade300,
            ),
            const SizedBox(height: 12),
            Text(
              'Error: ${snap.error}',
              textAlign: TextAlign.center,
              style: TextStyle(
                color: Colors.grey.shade600,
              ),
            ),
          ],
        ),
      );
    }

    final materials = snap.data ?? [];

    if (materials.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.folder_open_outlined,
              size: 72,
              color: Colors.grey.shade300,
            ),
            const SizedBox(height: 16),
            Text(
              'No materials for ${widget.grade} yet.',
              style: TextStyle(
                color: Colors.grey.shade500,
                fontSize: 15,
              ),
            ),
            const SizedBox(height: 6),
            Text(
              'Tap Upload to add materials.',
              style: TextStyle(
                color: Colors.grey.shade400,
                fontSize: 13,
              ),
            ),
            const SizedBox(height: 100),
          ],
        ),
      );
    }

    // Group by lesson title
    final grouped = <String, List<LessonMaterial>>{};

    for (final m in materials) {
      grouped.putIfAbsent(m.lessonTitle, () => []).add(m);
    }

    return RefreshIndicator(
      onRefresh: () async => setState(_load),
      child: ListView.builder(
        padding: const EdgeInsets.fromLTRB(
          16,
          16,
          16,
          100,
        ),
        itemCount: grouped.length,
        itemBuilder: (_, i) {
          final lessonTitle = grouped.keys.elementAt(i);
          final items = grouped[lessonTitle]!;

          return Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Lesson header
              Padding(
                padding: const EdgeInsets.only(
                  top: 12,
                  bottom: 8,
                ),
                child: Row(
                  children: [
                    Container(
                      width: 4,
                      height: 20,
                      decoration: BoxDecoration(
                        color: _primaryBlue,
                        borderRadius: BorderRadius.circular(2),
                      ),
                    ),
                    const SizedBox(width: 8),
                    Expanded(
                      child: Text(
                        lessonTitle,
                        style: const TextStyle(
                          fontSize: 14,
                          fontWeight: FontWeight.w700,
                          color: _primaryBlue,
                        ),
                      ),
                    ),
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 8,
                        vertical: 2,
                      ),
                      decoration: BoxDecoration(
                        color: _primaryBlue.withOpacity(0.08),
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: Text(
                        '${items.length} file${items.length != 1 ? 's' : ''}',
                        style: const TextStyle(
                          fontSize: 11,
                          fontWeight: FontWeight.w600,
                          color: _primaryBlue,
                        ),
                      ),
                    ),
                  ],
                ),
              ),

              // Material cards
              ...items.map((m) => _buildMaterialCard(m)),
              const SizedBox(height: 4),
            ],
          );
        },
      ),
    );
  }

  Widget _buildMaterialCard(LessonMaterial m) {
    final color = _typeColors[m.materialType] ?? Colors.grey;

    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
      ),
      elevation: 0,
      child: Padding(
        padding: const EdgeInsets.symmetric(
          horizontal: 12,
          vertical: 10,
        ),
        child: Row(
          children: [
            // File type icon
            Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                color: color.withOpacity(0.12),
                borderRadius: BorderRadius.circular(10),
              ),
              child: Center(
                child: Text(
                  m.getIcon(),
                  style: const TextStyle(fontSize: 22),
                ),
              ),
            ),
            const SizedBox(width: 12),

            // File info
            Expanded(
              child: Column(
                crossAxisAlignment:
                    CrossAxisAlignment.start,
                children: [
                  Text(
                    m.materialName,
                    style: const TextStyle(
                      fontSize: 13,
                      fontWeight: FontWeight.w600,
                    ),
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 3),
                  Row(
                    children: [
                      // Type chip
                      Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 6,
                          vertical: 2,
                        ),
                        decoration: BoxDecoration(
                          color: color.withOpacity(0.12),
                          borderRadius: BorderRadius.circular(4),
                        ),
                        child: Text(
                          m.materialType.toUpperCase(),
                          style: TextStyle(
                            fontSize: 9,
                            fontWeight: FontWeight.w700,
                            color: color,
                          ),
                        ),
                      ),
                      const SizedBox(width: 6),
                      Text(
                        m.getFileSizeString(),
                        style: TextStyle(
                          fontSize: 11,
                          color: Colors.grey.shade500,
                        ),
                      ),
                      const SizedBox(width: 6),
                      Icon(
                        Icons.download,
                        size: 11,
                        color: Colors.grey.shade400,
                      ),
                      Text(
                        ' ${m.downloadCount}',
                        style: TextStyle(
                          fontSize: 11,
                          color: Colors.grey.shade500,
                        ),
                      ),
                      const SizedBox(width: 6),
                      Text(
                        _fmtDate(m.uploadedAt),
                        style: TextStyle(
                          fontSize: 11,
                          color: Colors.grey.shade400,
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),

            // Delete button
            IconButton(
              icon: const Icon(
                Icons.delete_outline,
                color: Colors.red,
                size: 20,
              ),
              tooltip: 'Delete',
              onPressed: () async {
                await widget.onDelete(m);
                setState(_load);
              },
            ),
          ],
        ),
      ),
    );
  }

  String _fmtDate(DateTime dt) =>
      '${dt.day}/${dt.month}/${dt.year}';
}