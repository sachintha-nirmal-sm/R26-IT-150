import 'package:flutter/material.dart';
import 'package:file_picker/file_picker.dart';
import 'package:firebase_auth/firebase_auth.dart';
import '../models/lesson_material.dart';
import '../services/materials_service.dart';
import '../services/cloudinary_service.dart';

class UploadMaterialsScreen extends StatefulWidget {
  const UploadMaterialsScreen({Key? key}) : super(key: key);

  @override
  State<UploadMaterialsScreen> createState() => _UploadMaterialsScreenState();
}

class _UploadMaterialsScreenState extends State<UploadMaterialsScreen> {
  final MaterialsService _materialsService = MaterialsService();
  final _formKey = GlobalKey<FormState>();

  String? _selectedFile;
  String? _fileName;
  int? _fileSize;
  bool _isUploading = false;
  String _uploadProgress = '';

  // Form fields
  final TextEditingController _lessonIdCtrl = TextEditingController();
  final TextEditingController _lessonTitleCtrl = TextEditingController();
  final TextEditingController _materialNameCtrl = TextEditingController();
  final TextEditingController _descriptionCtrl = TextEditingController();

  String _selectedGrade = 'Grade 10';
  String _selectedTopic = 'Mechanics';

  final grades = ['Grade 9', 'Grade 10', 'Grade 11'];
  final topics = [
    'Mechanics', 'Electricity', 'Magnetism', 'Waves', 'Optics',
    'Thermodynamics', 'Modern Physics', 'General'
  ];

  @override
  void dispose() {
    _lessonIdCtrl.dispose();
    _lessonTitleCtrl.dispose();
    _materialNameCtrl.dispose();
    _descriptionCtrl.dispose();
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
          _selectedFile = result.files.single.path;
          _fileName = result.files.single.name;
          _fileSize = result.files.single.size;
          _materialNameCtrl.text = _fileName ?? '';
        });
      }
    } catch (e) {
      _showError('Error picking file: $e');
    }
  }

  Future<void> _uploadMaterial() async {
    if (!_formKey.currentState!.validate() || _selectedFile == null) {
      _showError('Please fill all fields and select a file');
      return;
    }

    setState(() => _isUploading = true);

    try {
      setState(() => _uploadProgress = 'Uploading to Cloudinary...');

      // Upload to Cloudinary
      final uploadResult = await CloudinaryService.uploadFile(
        file: (await _getFile(_selectedFile!)),
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
        lessonTitle: _lessonTitleCtrl.text.trim(),
        materialName: _materialNameCtrl.text.trim(),
        materialType: CloudinaryService.getFileType(_fileName ?? ''),
        grade: _selectedGrade,
        topic: _selectedTopic,
        cloudinaryUrl: uploadResult['url'],
        cloudinaryPublicId: uploadResult['publicId'],
        fileSizeBytes: uploadResult['fileSize'] ?? _fileSize ?? 0,
        uploadedBy: FirebaseAuth.instance.currentUser?.uid ?? 'unknown',
        uploadedAt: DateTime.now(),
        description: _descriptionCtrl.text.trim(),
      );

      // Save to Firestore
      final saved = await _materialsService.saveMaterial(material);

      if (saved) {
        _showSuccess('Material uploaded successfully!');
        _resetForm();
      } else {
        _showError('Failed to save material metadata');
      }
    } catch (e) {
      _showError('Error: $e');
    } finally {
      setState(() => _isUploading = false);
    }
  }

  Future<Object> _getFile(String path) async {
    // Convert file path to File object
    // This is a placeholder - actual implementation depends on your file handling
    return Future.value(path);
  }

  void _resetForm() {
    _formKey.currentState?.reset();
    setState(() {
      _selectedFile = null;
      _fileName = null;
      _fileSize = null;
      _lessonIdCtrl.clear();
      _lessonTitleCtrl.clear();
      _materialNameCtrl.clear();
      _descriptionCtrl.clear();
      _selectedGrade = 'Grade 10';
      _selectedTopic = 'Mechanics';
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
    return Scaffold(
      appBar: AppBar(
        title: const Text('Upload Lesson Materials'),
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Color(0xFF1A1A2E)),
          onPressed: () => Navigator.pop(context),
        ),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // File picker
              Container(
                decoration: BoxDecoration(
                  border: Border.all(color: Colors.grey[300]!),
                  borderRadius: BorderRadius.circular(12),
                  color: Colors.grey[50],
                ),
                padding: const EdgeInsets.all(20),
                child: Column(
                  children: [
                    if (_selectedFile == null)
                      Column(
                        children: [
                          Icon(Icons.cloud_upload_outlined,
                              size: 48, color: Colors.grey[400]),
                          const SizedBox(height: 12),
                          Text(
                            'No file selected',
                            style: TextStyle(color: Colors.grey[600]),
                          ),
                        ],
                      )
                    else
                      Column(
                        children: [
                          Icon(Icons.check_circle, size: 48,
                              color: Colors.green),
                          const SizedBox(height: 8),
                          Text(_fileName ?? '', style: const TextStyle(
                              fontWeight: FontWeight.w600)),
                          Text('${_fileSize} bytes',
                              style: TextStyle(color: Colors.grey[600])),
                        ],
                      ),
                    const SizedBox(height: 16),
                    ElevatedButton.icon(
                      onPressed: _isUploading ? null : _pickFile,
                      icon: const Icon(Icons.attach_file),
                      label: const Text('Select File'),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: const Color(0xFF2196F3),
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 24),

              // Lesson info
              const Text('Lesson Information',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
              const SizedBox(height: 12),

              TextFormField(
                controller: _lessonIdCtrl,
                decoration: InputDecoration(
                  labelText: 'Lesson ID',
                  border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(8)),
                  hintText: 'e.g., lesson_001',
                ),
                validator: (v) => v?.isEmpty ?? true ? 'Required' : null,
              ),
              const SizedBox(height: 12),

              TextFormField(
                controller: _lessonTitleCtrl,
                decoration: InputDecoration(
                  labelText: 'Lesson Title',
                  border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(8)),
                  hintText: 'e.g., Newton\'s Laws',
                ),
                validator: (v) => v?.isEmpty ?? true ? 'Required' : null,
              ),
              const SizedBox(height: 24),

              // Material info
              const Text('Material Information',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
              const SizedBox(height: 12),

              TextFormField(
                controller: _materialNameCtrl,
                decoration: InputDecoration(
                  labelText: 'Material Name',
                  border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(8)),
                  hintText: 'e.g., Lesson Slides',
                ),
                validator: (v) => v?.isEmpty ?? true ? 'Required' : null,
              ),
              const SizedBox(height: 12),

              TextFormField(
                controller: _descriptionCtrl,
                maxLines: 3,
                decoration: InputDecoration(
                  labelText: 'Description',
                  border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(8)),
                  hintText: 'Describe the material...',
                ),
              ),
              const SizedBox(height: 24),

              // Grade & Topic
              const Text('Grade & Topic',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
              const SizedBox(height: 12),

              DropdownButtonFormField<String>(
                value: _selectedGrade,
                items: grades.map((g) => DropdownMenuItem(value: g, child: Text(g))).toList(),
                onChanged: (v) => setState(() => _selectedGrade = v!),
                decoration: InputDecoration(
                  labelText: 'Grade',
                  border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(8)),
                ),
              ),
              const SizedBox(height: 12),

              DropdownButtonFormField<String>(
                value: _selectedTopic,
                items: topics.map((t) => DropdownMenuItem(value: t, child: Text(t))).toList(),
                onChanged: (v) => setState(() => _selectedTopic = v!),
                decoration: InputDecoration(
                  labelText: 'Topic',
                  border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(8)),
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
                      valueColor: const AlwaysStoppedAnimation<Color>(
                          Color(0xFF2196F3)),
                    ),
                    const SizedBox(height: 8),
                    Text(_uploadProgress,
                        style: const TextStyle(color: Color(0xFF2196F3))),
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
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(8)),
                  ),
                  child: _isUploading
                      ? const SizedBox(
                          width: 20,
                          height: 20,
                          child: CircularProgressIndicator(
                            strokeWidth: 2,
                            valueColor:
                                AlwaysStoppedAnimation<Color>(Colors.white),
                          ),
                        )
                      : const Text('Upload Material',
                          style: TextStyle(fontSize: 16, color: Colors.white)),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
