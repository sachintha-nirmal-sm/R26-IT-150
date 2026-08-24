import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:file_picker/file_picker.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:url_launcher/url_launcher.dart';
import '../models/lesson_material.dart';
import '../services/materials_service.dart';
import '../services/cloudinary_service.dart';

class MaterialsHubScreen extends StatefulWidget {
  const MaterialsHubScreen({Key? key}) : super(key: key);

  @override
  State<MaterialsHubScreen> createState() => _MaterialsHubScreenState();
}

class _MaterialsHubScreenState extends State<MaterialsHubScreen> {
  final MaterialsService _materialsService = MaterialsService();
  final FirebaseFirestore _firestore = FirebaseFirestore.instance;
  final _formKey = GlobalKey<FormState>();

  // Upload state
  List<int>? _selectedFileBytes;
  String? _fileName;
  int? _fileSize;
  bool _isUploading = false;
  String _uploadProgress = '';

  final TextEditingController _lessonIdCtrl = TextEditingController();
  final TextEditingController _materialNameCtrl = TextEditingController();

  String _selectedGrade = 'Grade 9';
  final grades = ['Grade 9', 'Grade 10', 'Grade 11'];

  Map<String, dynamic>? _lessonData;
  bool _loadingLesson = false;

  // Filter state
  String _filterGrade = 'All';
  String _filterTopic = 'All';
  final filterGrades = ['All', 'Grade 9', 'Grade 10', 'Grade 11'];
  final filterTopics = ['All', 'Mechanics', 'Electricity', 'Magnetism', 'Waves', 'Optics', 'Thermodynamics', 'General'];

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
        _showSuccess('Lesson found!');
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

      final saved = await _materialsService.saveMaterial(material);

      if (saved) {
        _showSuccess('Material uploaded successfully!');
        _resetForm();
        setState(() {}); // Refresh materials list
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
                      // Grade
                      DropdownButtonFormField<String>(
                        value: _selectedGrade,
                        items: grades
                            .map((g) => DropdownMenuItem(value: g, child: Text(g)))
                            .toList(),
                        onChanged: (v) => setState(() => _selectedGrade = v!),
                        decoration: InputDecoration(
                          labelText: 'Grade',
                          border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(8)),
                        ),
                      ),
                      const SizedBox(height: 12),

                      // Lesson ID
                      Row(
                        children: [
                          Expanded(
                            child: TextFormField(
                              controller: _lessonIdCtrl,
                              decoration: InputDecoration(
                                labelText: 'Lesson ID',
                                hintText: 'e.g., lesson_001',
                                border: OutlineInputBorder(
                                    borderRadius: BorderRadius.circular(8)),
                              ),
                              validator: (v) =>
                                  v?.isEmpty ?? true ? 'Required' : null,
                              onChanged: (_) =>
                                  setState(() => _lessonData = null),
                            ),
                          ),
                          const SizedBox(width: 8),
                          ElevatedButton(
                            onPressed: _fetchLessonDetails,
                            style: ElevatedButton.styleFrom(
                              backgroundColor: const Color(0xFF2196F3),
                              padding: const EdgeInsets.symmetric(
                                  horizontal: 16, vertical: 16),
                            ),
                            child: _loadingLesson
                                ? const SizedBox(
                                    width: 20,
                                    height: 20,
                                    child: CircularProgressIndicator(
                                      strokeWidth: 2,
                                      valueColor: AlwaysStoppedAnimation<Color>(
                                          Colors.white),
                                    ),
                                  )
                                : const Text('Fetch',
                                    style: TextStyle(color: Colors.white)),
                          ),
                        ],
                      ),
                      const SizedBox(height: 12),

                      // Lesson found
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
                                '✓ ${_lessonData?['title']}',
                                style: TextStyle(
                                    color: Colors.green[700],
                                    fontWeight: FontWeight.w600),
                              ),
                              Text(
                                'Topic: ${_lessonData?['topic'] ?? 'General'}',
                                style: const TextStyle(fontSize: 12),
                              ),
                            ],
                          ),
                        ),
                      if (_lessonData != null) const SizedBox(height: 12),

                      // Material name
                      TextFormField(
                        controller: _materialNameCtrl,
                        decoration: InputDecoration(
                          labelText: 'Material Name',
                          hintText: 'e.g., Lesson Slides',
                          border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(8)),
                        ),
                        validator: (v) =>
                            v?.isEmpty ?? true ? 'Required' : null,
                      ),
                      const SizedBox(height: 12),

                      // File picker
                      Container(
                        decoration: BoxDecoration(
                          border: Border.all(color: Colors.grey[300]!),
                          borderRadius: BorderRadius.circular(8),
                          color: Colors.white,
                        ),
                        padding: const EdgeInsets.all(16),
                        child: Column(
                          children: [
                            if (_selectedFileBytes == null)
                              Column(
                                children: [
                                  Icon(Icons.cloud_upload_outlined, size: 40,
                                      color: Colors.grey[400]),
                                  const SizedBox(height: 8),
                                  Text('No file selected',
                                      style: TextStyle(
                                          color: Colors.grey[600])),
                                ],
                              )
                            else
                              Column(
                                children: [
                                  Icon(Icons.check_circle, size: 40,
                                      color: Colors.green),
                                  const SizedBox(height: 8),
                                  Text(_fileName ?? '',
                                      style: const TextStyle(
                                          fontWeight: FontWeight.w600)),
                                  Text('${_fileSize} bytes',
                                      style: TextStyle(
                                          color: Colors.grey[600],
                                          fontSize: 12)),
                                ],
                              ),
                            const SizedBox(height: 12),
                            ElevatedButton.icon(
                              onPressed: _isUploading ? null : _pickFile,
                              icon: const Icon(Icons.attach_file),
                              label: const Text('Select File'),
                              style: ElevatedButton.styleFrom(
                                  backgroundColor: const Color(0xFF2196F3)),
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
                              minHeight: 6,
                              backgroundColor: Colors.grey[300],
                              valueColor: const AlwaysStoppedAnimation<Color>(
                                  Color(0xFF2196F3)),
                            ),
                            const SizedBox(height: 8),
                            Text(_uploadProgress,
                                style: const TextStyle(
                                    color: Color(0xFF2196F3), fontSize: 12)),
                            const SizedBox(height: 12),
                          ],
                        ),

                      // Upload button
                      SizedBox(
                        width: double.infinity,
                        height: 48,
                        child: ElevatedButton(
                          onPressed: _isUploading ? null : _uploadMaterial,
                          style: ElevatedButton.styleFrom(
                            backgroundColor: const Color(0xFF2196F3),
                            shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(8)),
                          ),
                          child: _isUploading
                              ? const SizedBox(
                                  width: 20,
                                  height: 20,
                                  child: CircularProgressIndicator(
                                    strokeWidth: 2,
                                    valueColor: AlwaysStoppedAnimation<Color>(
                                        Colors.white),
                                  ),
                                )
                              : const Text('Upload Material',
                                  style: TextStyle(
                                      fontSize: 16, color: Colors.white)),
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),

          const SizedBox(height: 24),

          // MATERIALS LIST SECTION
          Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text('All Materials',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.w700)),
                const SizedBox(height: 12),

                // Filters
                Row(
                  children: [
                    Expanded(
                      child: DropdownButton<String>(
                        value: _filterGrade,
                        isExpanded: true,
                        items: filterGrades
                            .map((g) => DropdownMenuItem(value: g, child: Text(g)))
                            .toList(),
                        onChanged: (v) => setState(() => _filterGrade = v!),
                      ),
                    ),
                    const SizedBox(width: 8),
                    Expanded(
                      child: DropdownButton<String>(
                        value: _filterTopic,
                        isExpanded: true,
                        items: filterTopics
                            .map((t) => DropdownMenuItem(value: t, child: Text(t)))
                            .toList(),
                        onChanged: (v) => setState(() => _filterTopic = v!),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 16),

                // Materials list
                FutureBuilder<List<LessonMaterial>>(
                  future: _materialsService.getAllMaterials(limit: 200),
                  builder: (context, snapshot) {
                    if (snapshot.connectionState == ConnectionState.waiting) {
                      return const Center(
                          child: Padding(
                        padding: EdgeInsets.all(32),
                        child: CircularProgressIndicator(),
                      ));
                    }

                    final materials = snapshot.data ?? [];
                    final filtered = materials.where((m) {
                      if (_filterGrade != 'All' && m.grade != _filterGrade)
                        return false;
                      if (_filterTopic != 'All' && m.topic != _filterTopic)
                        return false;
                      return true;
                    }).toList();

                    if (filtered.isEmpty) {
                      return Center(
                        child: Padding(
                          padding: const EdgeInsets.all(32),
                          child: Text('No materials found',
                              style: TextStyle(color: Colors.grey[600])),
                        ),
                      );
                    }

                    return ListView.builder(
                      shrinkWrap: true,
                      physics: const NeverScrollableScrollPhysics(),
                      itemCount: filtered.length,
                      itemBuilder: (context, index) {
                        final material = filtered[index];
                        return _buildMaterialCard(material);
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

  Widget _buildMaterialCard(LessonMaterial material) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(color: Colors.black.withOpacity(0.08), blurRadius: 8),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Preview
          if (material.materialType.toLowerCase() == 'pdf')
            Container(
              height: 150,
              width: double.infinity,
              decoration: BoxDecoration(
                color: Colors.grey[200],
                borderRadius:
                    const BorderRadius.vertical(top: Radius.circular(12)),
              ),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(Icons.picture_as_pdf, size: 48, color: Colors.red[400]),
                  const SizedBox(height: 4),
                  Text('PDF', style: TextStyle(color: Colors.grey[600], fontSize: 12)),
                ],
              ),
            )
          else if (['jpg', 'jpeg', 'png', 'gif', 'webp']
              .contains(material.materialType.toLowerCase()))
            Container(
              height: 150,
              width: double.infinity,
              decoration: BoxDecoration(
                color: Colors.grey[200],
                borderRadius:
                    const BorderRadius.vertical(top: Radius.circular(12)),
              ),
              child: Image.network(
                material.cloudinaryUrl,
                fit: BoxFit.cover,
                errorBuilder: (c, e, st) => Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(Icons.image, size: 48, color: Colors.grey[400]),
                    Text('Image', style: TextStyle(color: Colors.grey[600], fontSize: 12)),
                  ],
                ),
              ),
            )
          else
            Container(
              height: 150,
              width: double.infinity,
              decoration: BoxDecoration(
                color: Colors.grey[200],
                borderRadius:
                    const BorderRadius.vertical(top: Radius.circular(12)),
              ),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(Icons.description, size: 48, color: Colors.grey[400]),
                  Text(material.materialType.toUpperCase(),
                      style: TextStyle(color: Colors.grey[600], fontSize: 12)),
                ],
              ),
            ),

          // Info
          Padding(
            padding: const EdgeInsets.all(12),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(material.materialName,
                              style: const TextStyle(
                                  fontSize: 14, fontWeight: FontWeight.w600)),
                          Text(material.lessonTitle,
                              style: TextStyle(
                                  fontSize: 11, color: Colors.grey[600])),
                        ],
                      ),
                    ),
                    Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 6, vertical: 3),
                      decoration: BoxDecoration(
                        color: const Color(0xFFE8F1FF),
                        borderRadius: BorderRadius.circular(4),
                      ),
                      child: Text(material.materialType.toUpperCase(),
                          style: const TextStyle(
                              fontSize: 9,
                              fontWeight: FontWeight.w600,
                              color: Color(0xFF2196F3))),
                    ),
                  ],
                ),
                const SizedBox(height: 6),
                Text(
                    '${material.grade} • ${material.topic} • ${material.getFileSizeString()}',
                    style: TextStyle(fontSize: 11, color: Colors.grey[600])),
                const SizedBox(height: 8),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text('⬇️ ${material.downloadCount} downloads',
                        style: TextStyle(fontSize: 10, color: Colors.grey[500])),
                    GestureDetector(
                      onTap: () async {
                        if (await canLaunchUrl(
                            Uri.parse(material.cloudinaryUrl))) {
                          await launchUrl(Uri.parse(material.cloudinaryUrl),
                              mode: LaunchMode.externalApplication);
                        }
                      },
                      child: const Text('View →',
                          style: TextStyle(
                              fontSize: 10,
                              color: Color(0xFF2196F3),
                              fontWeight: FontWeight.w600)),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
