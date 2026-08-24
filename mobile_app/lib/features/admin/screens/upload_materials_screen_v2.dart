import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:file_picker/file_picker.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import '../models/lesson_material.dart';
import '../services/materials_service.dart';
import '../services/cloudinary_service.dart';

class UploadMaterialsScreenV2 extends StatefulWidget {
  const UploadMaterialsScreenV2({Key? key}) : super(key: key);

  @override
  State<UploadMaterialsScreenV2> createState() => _UploadMaterialsScreenV2State();
}

class _UploadMaterialsScreenV2State extends State<UploadMaterialsScreenV2> {
  final MaterialsService _materialsService = MaterialsService();
  final FirebaseFirestore _firestore = FirebaseFirestore.instance;
  final _formKey = GlobalKey<FormState>();

  List<int>? _selectedFileBytes;
  String? _fileName;
  int? _fileSize;
  bool _isUploading = false;
  String _uploadProgress = '';

  // Form fields
  final TextEditingController _lessonIdCtrl = TextEditingController();
  final TextEditingController _materialNameCtrl = TextEditingController();

  String _selectedGrade = 'Grade 9';
  final grades = ['Grade 9', 'Grade 10', 'Grade 11'];

  // Lesson cache
  Map<String, dynamic>? _lessonData;
  bool _loadingLesson = false;

  @override
  void dispose() {
    _lessonIdCtrl.dispose();
    _materialNameCtrl.dispose();
    super.dispose();
  }

  Future<void> _pickFile() async {
    try {
      final result = await FilePicker.platform.pickFiles(
        type: FileType.custom,
        allowedExtensions: [
          'pdf', 'jpg', 'jpeg', 'png', 'gif', 'webp',
          'mp4', 'avi', 'mov', 'mkv',
          'doc', 'docx', 'ppt', 'pptx'
        ],
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

  /// Fetch lesson details to get topic
  Future<void> _fetchLessonDetails() async {
    if (_lessonIdCtrl.text.isEmpty) {
      _showError('Enter lesson ID');
      return;
    }

    setState(() => _loadingLesson = true);

    try {
      final doc = await _firestore
          .collection('lessons')
          .doc(_lessonIdCtrl.text.trim())
          .get();

      if (doc.exists) {
        setState(() {
          _lessonData = doc.data();
          _loadingLesson = false;
        });
      } else {
        _showError('Lesson not found');
        setState(() => _loadingLesson = false);
      }
    } catch (e) {
      _showError('Error fetching lesson: $e');
      setState(() => _loadingLesson = false);
    }
  }

  Future<void> _uploadMaterial() async {
    if (!_formKey.currentState!.validate() || _selectedFileBytes == null) {
      _showError('Please select a file');
      return;
    }

    if (_lessonData == null) {
      _showError('Please fetch lesson details first');
      return;
    }

    setState(() => _isUploading = true);

    try {
      setState(() => _uploadProgress = 'Uploading to Cloudinary...');

      // Upload to Cloudinary
      final uploadResult = await CloudinaryService.uploadFile(
        file: _selectedFileBytes,
        fileName: _fileName ?? 'material',
        folder: _selectedGrade,
      );

      if (!uploadResult['success']) {
        _showError(uploadResult['error'] ?? 'Upload failed');
        setState(() => _isUploading = false);
        return;
      }

      setState(() => _uploadProgress = 'Saving to database...');

      // Create material object
      final material = LessonMaterial(
        id: '',
        lessonId: _lessonIdCtrl.text.trim(),
        lessonTitle: _lessonData?['title'] ?? 'Unknown',
        materialName: _materialNameCtrl.text.trim(),
        materialType: CloudinaryService.getFileType(_fileName ?? ''),
        grade: _selectedGrade,
        topic: _lessonData?['topic'] ?? _lessonData?['subject'] ?? 'General',
        cloudinaryUrl: uploadResult['url'],
        cloudinaryPublicId: uploadResult['publicId'],
        fileSizeBytes: uploadResult['fileSize'] ?? _fileSize ?? 0,
        uploadedBy: FirebaseAuth.instance.currentUser?.uid ?? 'unknown',
        uploadedAt: DateTime.now(),
        description: '',
      );

      // Save to Firestore
      final saved = await _materialsService.saveMaterial(material);

      if (saved) {
        _showSuccess('Material uploaded successfully!');
        _resetForm();
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
      _lessonIdCtrl.clear();
      _materialNameCtrl.clear();
      _lessonData = null;
      _selectedGrade = 'Grade 9';
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
      padding: const EdgeInsets.all(16),
      child: Form(
        key: _formKey,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Grade selector
            const Text('Select Grade', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
            const SizedBox(height: 12),
            DropdownButtonFormField<String>(
              value: _selectedGrade,
              items: grades.map((g) => DropdownMenuItem(value: g, child: Text(g))).toList(),
              onChanged: (v) => setState(() => _selectedGrade = v!),
              decoration: InputDecoration(
                border: OutlineInputBorder(borderRadius: BorderRadius.circular(8)),
              ),
            ),
            const SizedBox(height: 24),

            // Lesson ID
            const Text('Lesson ID', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
            const SizedBox(height: 12),
            TextFormField(
              controller: _lessonIdCtrl,
              decoration: InputDecoration(
                hintText: 'e.g., lesson_001',
                border: OutlineInputBorder(borderRadius: BorderRadius.circular(8)),
                suffixIcon: _loadingLesson
                    ? const SizedBox(
                        width: 20,
                        height: 20,
                        child: Padding(
                          padding: EdgeInsets.all(12),
                          child: CircularProgressIndicator(strokeWidth: 2),
                        ),
                      )
                    : null,
              ),
              validator: (v) => v?.isEmpty ?? true ? 'Required' : null,
              onChanged: (_) => setState(() => _lessonData = null),
            ),
            const SizedBox(height: 8),
            ElevatedButton(
              onPressed: _fetchLessonDetails,
              style: ElevatedButton.styleFrom(backgroundColor: const Color(0xFF2196F3)),
              child: const Text('Fetch Lesson Details', style: TextStyle(color: Colors.white)),
            ),
            const SizedBox(height: 16),

            // Show lesson info if loaded
            if (_lessonData != null)
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: Colors.green[50],
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.green),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      '✓ Lesson Found',
                      style: TextStyle(color: Colors.green[700], fontWeight: FontWeight.w600),
                    ),
                    Text('Title: ${_lessonData?['title']}', style: const TextStyle(fontSize: 12)),
                    Text('Topic: ${_lessonData?['topic'] ?? 'General'}', style: const TextStyle(fontSize: 12)),
                  ],
                ),
              ),
            const SizedBox(height: 24),

            // Material name
            const Text('Material Name', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
            const SizedBox(height: 12),
            TextFormField(
              controller: _materialNameCtrl,
              decoration: InputDecoration(
                hintText: 'e.g., Lesson Slides, Worksheet',
                border: OutlineInputBorder(borderRadius: BorderRadius.circular(8)),
              ),
              validator: (v) => v?.isEmpty ?? true ? 'Required' : null,
            ),
            const SizedBox(height: 24),

            // File picker
            const Text('Select File', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
            const SizedBox(height: 12),
            Container(
              decoration: BoxDecoration(
                border: Border.all(color: Colors.grey[300]!),
                borderRadius: BorderRadius.circular(12),
                color: Colors.grey[50],
              ),
              padding: const EdgeInsets.all(20),
              child: Column(
                children: [
                  if (_selectedFileBytes == null)
                    Column(
                      children: [
                        Icon(Icons.cloud_upload_outlined, size: 48, color: Colors.grey[400]),
                        const SizedBox(height: 12),
                        Text('No file selected', style: TextStyle(color: Colors.grey[600])),
                      ],
                    )
                  else
                    Column(
                      children: [
                        Icon(Icons.check_circle, size: 48, color: Colors.green),
                        const SizedBox(height: 8),
                        Text(_fileName ?? '', style: const TextStyle(fontWeight: FontWeight.w600)),
                        Text('${_fileSize} bytes', style: TextStyle(color: Colors.grey[600])),
                      ],
                    ),
                  const SizedBox(height: 16),
                  ElevatedButton.icon(
                    onPressed: _isUploading ? null : _pickFile,
                    icon: const Icon(Icons.attach_file),
                    label: const Text('Select File'),
                    style: ElevatedButton.styleFrom(backgroundColor: const Color(0xFF2196F3)),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 24),

            // Upload progress
            if (_isUploading)
              Column(
                children: [
                  LinearProgressIndicator(
                    minHeight: 6,
                    backgroundColor: Colors.grey[300],
                    valueColor: const AlwaysStoppedAnimation<Color>(Color(0xFF2196F3)),
                  ),
                  const SizedBox(height: 8),
                  Text(_uploadProgress, style: const TextStyle(color: Color(0xFF2196F3))),
                  const SizedBox(height: 24),
                ],
              ),

            // Upload button
            SizedBox(
              width: double.infinity,
              height: 50,
              child: ElevatedButton(
                onPressed: _isUploading ? null : _uploadMaterial,
                style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xFF2196F3),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                ),
                child: _isUploading
                    ? const SizedBox(
                        width: 20,
                        height: 20,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          valueColor: AlwaysStoppedAnimation<Color>(Colors.white),
                        ),
                      )
                    : const Text('Upload Material',
                        style: TextStyle(fontSize: 16, color: Colors.white)),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
