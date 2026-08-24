import 'dart:io';
import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:file_picker/file_picker.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:url_launcher/url_launcher.dart';
import 'package:http/http.dart' as http;
// ignore: avoid_web_libraries_in_flutter
import 'dart:html' as html;
import '../models/lesson_material.dart';
import '../services/materials_service.dart';

class MaterialsHubScreenV2 extends StatefulWidget {
  const MaterialsHubScreenV2({Key? key}) : super(key: key);

  @override
  State<MaterialsHubScreenV2> createState() => _MaterialsHubScreenV2State();
}

class _MaterialsHubScreenV2State extends State<MaterialsHubScreenV2> {
  final MaterialsService _materialsService = MaterialsService();
  final FirebaseFirestore _firestore = FirebaseFirestore.instance;
  final _formKey = GlobalKey<FormState>();

  // Upload state
  List<int>? _selectedFileBytes;
  String? _fileName;
  int? _fileSize;
  bool _isUploading = false;
  String _uploadProgress = '';

  String _selectedGrade = 'Grade 9';
  final grades = ['Grade 9', 'Grade 10', 'Grade 11'];
  List<Map<String, dynamic>> _lessonsList = [];
  String? _selectedLessonId;
  Map<String, dynamic>? _selectedLessonData;
  bool _loadingLessons = false;

  // Filter state
  String _filterGrade = 'All';
  final filterGrades = ['All', 'Grade 9', 'Grade 10', 'Grade 11'];
  int _materialsKey = 0; // increment to force FutureBuilder refresh

  @override
  void initState() {
    super.initState();
    _loadLessonsForGrade('Grade 9');
  }

  Future<void> _loadLessonsForGrade(String grade) async {
    setState(() => _loadingLessons = true);

    try {
      final token = await FirebaseAuth.instance.currentUser?.getIdToken();
      if (token == null) {
        _showError('Not authenticated');
        setState(() => _loadingLessons = false);
        return;
      }

      final response = await http.get(
        Uri.parse('http://localhost:9000/admin/lessons'),
        headers: {'Authorization': 'Bearer $token'},
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body) as List;
        final allLessons = data.cast<Map<String, dynamic>>();

        final gradeNum = int.tryParse(grade.replaceAll(RegExp(r'[^0-9]'), '')) ?? 9;
        final filtered = allLessons
            .where((l) => l['grade'] == gradeNum)
            .toList();

        setState(() {
          _lessonsList = filtered;
          _selectedLessonId = null;
          _selectedLessonData = null;
          _loadingLessons = false;
        });
      } else {
        _showError('Failed to load lessons');
        setState(() => _loadingLessons = false);
      }
    } catch (e) {
      _showError('Error loading lessons: $e');
      setState(() => _loadingLessons = false);
    }
  }

  Future<void> _pickFile() async {
    try {
      final result = await FilePicker.platform.pickFiles(
        type: FileType.custom,
        allowedExtensions: ['pdf', 'jpg', 'jpeg', 'png', 'doc', 'docx'],
      );

      if (result != null && result.files.isNotEmpty) {
        setState(() {
          _selectedFileBytes = result.files.single.bytes;
          _fileName = result.files.single.name;
          _fileSize = result.files.single.size;
        });
      }
    } catch (e) {
      _showError('Error picking file: $e');
    }
  }

  Future<void> _uploadMaterial() async {
    if (!_formKey.currentState!.validate() ||
        _selectedFileBytes == null ||
        _selectedLessonData == null) {
      _showError('Please select lesson and file');
      return;
    }

    setState(() => _isUploading = true);

    try {
      setState(() => _uploadProgress = 'Uploading file...');

      final token = await FirebaseAuth.instance.currentUser?.getIdToken();
      if (token == null) {
        _showError('Not authenticated');
        setState(() => _isUploading = false);
        return;
      }

      final request = http.MultipartRequest(
        'POST',
        Uri.parse('http://localhost:9000/admin/upload-material'),
      );
      request.headers['Authorization'] = 'Bearer $token';
      request.fields['grade'] = _selectedGrade;
      request.fields['lesson_id'] = _selectedLessonId ?? '';
      request.fields['lesson_title'] = _selectedLessonData?['title']?.toString() ?? '';
      request.fields['topic'] = _selectedLessonData?['topic']?.toString() ?? 'General';
      request.files.add(http.MultipartFile.fromBytes(
        'file',
        _selectedFileBytes!,
        filename: _fileName ?? 'material.pdf',
      ));

      final response = await request.send();
      final body = await response.stream.bytesToString();

      if (response.statusCode != 200) {
        _showError('Upload failed: $body');
        setState(() => _isUploading = false);
        return;
      }

      final uploadResult = jsonDecode(body) as Map<String, dynamic>;
      if (uploadResult['success'] != true) {
        _showError(uploadResult['error'] ?? 'Upload failed');
        setState(() => _isUploading = false);
        return;
      }

      setState(() => _uploadProgress = 'Saving to database...');

      final lessonTitle = _selectedLessonData?['title']?.toString() ?? 'Unknown';
      final topic = _selectedLessonData?['topic']?.toString() ?? 'General';
      final ext = (_fileName ?? '').contains('.') ? (_fileName!).split('.').last.toLowerCase() : 'pdf';

      final material = LessonMaterial(
        id: '',
        lessonId: _selectedLessonId ?? '',
        lessonTitle: lessonTitle,
        materialName: lessonTitle,
        materialType: ext,
        grade: _selectedGrade,
        topic: topic,
        cloudinaryUrl: uploadResult['url'] ?? '',
        cloudinaryPublicId: uploadResult['publicId'] ?? '',
        fileSizeBytes: (uploadResult['fileSize'] as num?)?.toInt() ?? _fileSize ?? 0,
        uploadedBy: FirebaseAuth.instance.currentUser?.uid ?? 'unknown',
        uploadedAt: DateTime.now(),
        description: '',
      );

      final saved = await _materialsService.saveMaterial(material);

      if (saved) {
        _showSuccess('PDF uploaded!');
        _resetForm();
        setState(() => _materialsKey++);
      } else {
        _showError('Failed to save material');
      }
    } catch (e) {
      _showError('Error: $e');
    } finally {
      setState(() => _isUploading = false);
    }
  }

  void _resetForm() {
    _formKey.currentState?.reset();
    setState(() {
      _selectedFileBytes = null;
      _fileName = null;
      _fileSize = null;
      _selectedLessonId = null;
      _selectedLessonData = null;
    });
  }

  void _showError(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message), backgroundColor: Colors.red),
    );
  }

  void _showSuccess(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message), backgroundColor: Colors.green),
    );
  }

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      child: Column(
        children: [
          // UPLOAD SECTION
          Container(
            color: Colors.blue[50],
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text('Upload Material',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.w700)),
                const SizedBox(height: 16),
                Form(
                  key: _formKey,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Grade dropdown
                      DropdownButtonFormField<String>(
                        value: _selectedGrade,
                        items: grades
                            .map((g) => DropdownMenuItem(value: g, child: Text(g)))
                            .toList(),
                        onChanged: (v) {
                          setState(() => _selectedGrade = v!);
                          _loadLessonsForGrade(v!);
                        },
                        decoration: InputDecoration(
                          labelText: 'Grade',
                          border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(8)),
                        ),
                      ),
                      const SizedBox(height: 12),

                      // Lessons dropdown
                      if (_loadingLessons)
                        const Padding(
                          padding: EdgeInsets.symmetric(vertical: 12),
                          child: SizedBox(
                            height: 20,
                            width: 20,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          ),
                        )
                      else if (_lessonsList.isEmpty)
                        Container(
                          padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 16),
                          decoration: BoxDecoration(
                            border: Border.all(color: Colors.grey[300]!),
                            borderRadius: BorderRadius.circular(8),
                            color: Colors.grey[50],
                          ),
                          child: Text('No lessons for this grade',
                              style: TextStyle(color: Colors.grey[600], fontSize: 13)),
                        )
                      else
                        DropdownButtonFormField<String>(
                          value: _selectedLessonId,
                          isExpanded: true,
                          items: _lessonsList
                              .map((lesson) => DropdownMenuItem(
                                    value: lesson['id'].toString(),
                                    child: Text(
                                      lesson['title']?.toString() ?? 'Unknown',
                                      maxLines: 1,
                                      overflow: TextOverflow.ellipsis,
                                    ),
                                  ))
                              .toList(),
                          onChanged: (v) {
                            setState(() {
                              _selectedLessonId = v;
                              _selectedLessonData = _lessonsList
                                  .firstWhere((l) => l['id'].toString() == v, orElse: () => {});
                            });
                          },
                          decoration: InputDecoration(
                            labelText: 'Select Lesson',
                            border: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(8)),
                          ),
                          validator: (v) => v == null ? 'Required' : null,
                        ),
                      const SizedBox(height: 12),

                      // File picker
                      Container(
                        decoration: BoxDecoration(
                          border: Border.all(color: Colors.grey[300]!),
                          borderRadius: BorderRadius.circular(8),
                          color: Colors.white,
                        ),
                        padding: const EdgeInsets.all(12),
                        child: Column(
                          children: [
                            if (_selectedFileBytes == null)
                              Column(
                                children: [
                                  Icon(Icons.picture_as_pdf, size: 32, color: Colors.red[400]),
                                  const SizedBox(height: 6),
                                  Text('No file selected',
                                      style: TextStyle(color: Colors.grey[600], fontSize: 12)),
                                ],
                              )
                            else
                              Column(
                                children: [
                                  Icon(Icons.check_circle, size: 32, color: Colors.green),
                                  const SizedBox(height: 6),
                                  Text(_fileName ?? '',
                                      style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w600)),
                                ],
                              ),
                            const SizedBox(height: 10),
                            ElevatedButton.icon(
                              onPressed: _isUploading ? null : _pickFile,
                              icon: const Icon(Icons.attach_file, size: 18),
                              label: const Text('Pick PDF'),
                              style: ElevatedButton.styleFrom(
                                  backgroundColor: const Color(0xFF2196F3),
                                  padding: const EdgeInsets.symmetric(
                                      horizontal: 16, vertical: 8)),
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: 12),

                      // Progress
                      if (_isUploading)
                        Column(
                          children: [
                            LinearProgressIndicator(
                              minHeight: 4,
                              backgroundColor: Colors.grey[300],
                              valueColor:
                                  const AlwaysStoppedAnimation<Color>(Color(0xFF2196F3)),
                            ),
                            const SizedBox(height: 6),
                            Text(_uploadProgress,
                                style: const TextStyle(
                                    color: Color(0xFF2196F3), fontSize: 11)),
                            const SizedBox(height: 10),
                          ],
                        ),

                      // Upload button
                      SizedBox(
                        width: double.infinity,
                        height: 44,
                        child: ElevatedButton(
                          onPressed: _isUploading ? null : _uploadMaterial,
                          style: ElevatedButton.styleFrom(
                            backgroundColor: const Color(0xFF2196F3),
                            shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(8)),
                          ),
                          child: _isUploading
                              ? const SizedBox(
                                  width: 18,
                                  height: 18,
                                  child: CircularProgressIndicator(
                                    strokeWidth: 2,
                                    valueColor: AlwaysStoppedAnimation<Color>(
                                        Colors.white),
                                  ),
                                )
                              : const Text('Upload PDF',
                                  style: TextStyle(fontSize: 14, color: Colors.white)),
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),

          const SizedBox(height: 20),

          // MATERIALS LIST SECTION
          Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text('All Materials',
                        style: TextStyle(fontSize: 18, fontWeight: FontWeight.w700)),
                    DropdownButton<String>(
                      value: _filterGrade,
                      items: filterGrades
                          .map((g) => DropdownMenuItem(value: g, child: Text(g)))
                          .toList(),
                      onChanged: (v) => setState(() => _filterGrade = v!),
                    ),
                  ],
                ),
                const SizedBox(height: 12),

                // Materials list
                FutureBuilder<List<LessonMaterial>>(
                  key: ValueKey(_materialsKey),
                  future: _materialsService.getAllMaterials(limit: 200),
                  builder: (context, snapshot) {
                    if (snapshot.connectionState == ConnectionState.waiting) {
                      return const Center(
                          child: Padding(
                        padding: EdgeInsets.all(20),
                        child: CircularProgressIndicator(strokeWidth: 2),
                      ));
                    }

                    final materials = snapshot.data ?? [];
                    final filtered = materials.where((m) {
                      if (_filterGrade != 'All' && m.grade != _filterGrade)
                        return false;
                      return true;
                    }).toList();

                    if (filtered.isEmpty) {
                      return Center(
                        child: Padding(
                          padding: const EdgeInsets.all(20),
                          child: Text('No materials',
                              style: TextStyle(color: Colors.grey[600], fontSize: 13)),
                        ),
                      );
                    }

                    // Group by grade
                    final groupedByGrade = <String, List<LessonMaterial>>{};
                    for (final m in filtered) {
                      groupedByGrade.putIfAbsent(m.grade, () => []).add(m);
                    }

                    return ListView.builder(
                      shrinkWrap: true,
                      physics: const NeverScrollableScrollPhysics(),
                      itemCount: groupedByGrade.length,
                      itemBuilder: (context, index) {
                        final grade = groupedByGrade.keys.elementAt(index);
                        final materials = groupedByGrade[grade]!;

                        return Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(grade,
                                style: const TextStyle(
                                    fontSize: 13, fontWeight: FontWeight.w600)),
                            const SizedBox(height: 8),
                            SizedBox(
                              height: 165,
                              child: Scrollbar(
                                thumbVisibility: true,
                                child: ListView.separated(
                                  scrollDirection: Axis.horizontal,
                                  itemCount: materials.length,
                                  separatorBuilder: (_, __) => const SizedBox(width: 8),
                                  itemBuilder: (_, i) => _buildCompactMaterialCard(materials[i]),
                                ),
                              ),
                            ),
                            const SizedBox(height: 16),
                          ],
                        );
                      },
                    );
                  },
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  void _downloadFile(String url, String fileName) {
    if (kIsWeb) {
      // Fetch as blob first so the browser uses our filename (cross-origin download attribute is ignored)
      html.HttpRequest.request(url, responseType: 'blob').then((req) {
        final blob = req.response as html.Blob;
        final blobUrl = html.Url.createObjectUrlFromBlob(blob);
        final anchor = html.AnchorElement(href: blobUrl)
          ..setAttribute('download', fileName.endsWith('.pdf') ? fileName : '$fileName.pdf');
        html.document.body?.append(anchor);
        anchor.click();
        anchor.remove();
        html.Url.revokeObjectUrl(blobUrl);
      }).catchError((_) {
        // Fallback: open in new tab if blob fetch fails
        html.window.open(url, '_blank');
      });
    } else {
      launchUrl(Uri.parse(url), mode: LaunchMode.externalApplication);
    }
  }

  Future<void> _deleteMaterial(LessonMaterial material) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Delete Material'),
        content: Text('Delete "${material.lessonTitle}"?'),
        actions: [
          TextButton(
              onPressed: () => Navigator.pop(ctx, false),
              child: const Text('Cancel')),
          TextButton(
              onPressed: () => Navigator.pop(ctx, true),
              child: const Text('Delete', style: TextStyle(color: Colors.red))),
        ],
      ),
    );

    if (confirmed == true) {
      final deleted = await _materialsService.deleteMaterial(material.id);
      if (deleted) {
        _showSuccess('Deleted');
        setState(() => _materialsKey++);
      } else {
        _showError('Failed to delete');
      }
    }
  }

  Widget _buildCompactMaterialCard(LessonMaterial material) {
    return Container(
      width: 140,
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(8),
        boxShadow: [
          BoxShadow(color: Colors.black.withOpacity(0.06), blurRadius: 4),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Thumbnail with delete button overlay
          Stack(
            children: [
              GestureDetector(
                onTap: () => _downloadFile(material.cloudinaryUrl, '${material.lessonTitle}.pdf'),
                child: Container(
                  height: 80,
                  width: double.infinity,
                  decoration: BoxDecoration(
                    color: Colors.red[50],
                    borderRadius:
                        const BorderRadius.vertical(top: Radius.circular(8)),
                  ),
                  child: Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(Icons.picture_as_pdf, size: 32, color: Colors.red[400]),
                        const SizedBox(height: 4),
                        Icon(Icons.download, size: 14, color: Colors.grey[500]),
                      ],
                    ),
                  ),
                ),
              ),
              // Delete button
              Positioned(
                top: 4,
                right: 4,
                child: GestureDetector(
                  onTap: () => _deleteMaterial(material),
                  child: Container(
                    padding: const EdgeInsets.all(3),
                    decoration: BoxDecoration(
                      color: Colors.white,
                      shape: BoxShape.circle,
                      boxShadow: [
                        BoxShadow(
                            color: Colors.black.withOpacity(0.15),
                            blurRadius: 3),
                      ],
                    ),
                    child: Icon(Icons.delete_outline,
                        size: 14, color: Colors.red[400]),
                  ),
                ),
              ),
            ],
          ),
          // Info
          GestureDetector(
            onTap: () => _downloadFile(material.cloudinaryUrl, '${material.lessonTitle}.pdf'),
            child: Padding(
              padding: const EdgeInsets.all(8),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(material.lessonTitle,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(
                          fontSize: 11, fontWeight: FontWeight.w600)),
                  const SizedBox(height: 4),
                  Text(material.topic,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style:
                          TextStyle(fontSize: 9, color: Colors.grey[600])),
                  const SizedBox(height: 4),
                  Text(material.getFileSizeString(),
                      style:
                          TextStyle(fontSize: 8, color: Colors.grey[500])),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}
