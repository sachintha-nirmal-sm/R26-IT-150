import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:url_launcher/url_launcher.dart';
import '../admin/models/lesson_material.dart';
import '../admin/services/materials_service.dart';
// ignore: avoid_web_libraries_in_flutter
import 'dart:html' as html;

class LearningMaterialsPage extends StatefulWidget {
  final String lessonId;
  final String lessonTitle;
  final String grade;

  const LearningMaterialsPage({
    super.key,
    required this.lessonId,
    this.lessonTitle = 'Lesson',
    this.grade = 'Grade 10',
  });

  @override
  State<LearningMaterialsPage> createState() => _LearningMaterialsPageState();
}

class _LearningMaterialsPageState extends State<LearningMaterialsPage> {
  static const Color _primaryBlue = Color(0xFF2196F3);
  final MaterialsService _materialsService = MaterialsService();
  late Future<List<LessonMaterial>> _materialsFuture;

  @override
  void initState() {
    super.initState();
    _materialsFuture = _materialsService.getMaterialsForLesson(widget.lessonId);
  }

  void _downloadFile(String url, String fileName) {
    if (kIsWeb) {
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
        html.window.open(url, '_blank');
      });
    } else {
      launchUrl(Uri.parse(url), mode: LaunchMode.externalApplication);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F6FA),
      appBar: AppBar(
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: _primaryBlue),
          onPressed: () => Navigator.pop(context),
        ),
        title: const Text('Learning Materials',
            style: TextStyle(color: Colors.black87, fontWeight: FontWeight.bold)),
        centerTitle: true,
        backgroundColor: Colors.white,
        elevation: 0,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header
            Container(
              decoration: BoxDecoration(
                color: const Color(0xFFE8F1FB),
                borderRadius: BorderRadius.circular(16),
              ),
              padding: const EdgeInsets.all(20),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Chapter: ${widget.lessonTitle}',
                      style: const TextStyle(
                          fontSize: 14, color: _primaryBlue, fontWeight: FontWeight.w600)),
                  const SizedBox(height: 8),
                  const Text('Complete Study Material',
                      style: TextStyle(
                          fontSize: 20, fontWeight: FontWeight.bold, color: Colors.black87)),
                  const SizedBox(height: 8),
                  Text('Grade: ${widget.grade}',
                      style: const TextStyle(fontSize: 13, color: Colors.grey)),
                ],
              ),
            ),
            const SizedBox(height: 24),

            const Text('Available Documents',
                style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.black87)),
            const SizedBox(height: 12),

            FutureBuilder<List<LessonMaterial>>(
              future: _materialsFuture,
              builder: (context, snapshot) {
                if (snapshot.connectionState == ConnectionState.waiting) {
                  return const Center(
                      child: Padding(
                          padding: EdgeInsets.all(32),
                          child: CircularProgressIndicator(color: _primaryBlue)));
                }

                final materials = snapshot.data ?? [];

                if (materials.isEmpty) {
                  return Center(
                    child: Padding(
                      padding: const EdgeInsets.all(32),
                      child: Column(
                        children: [
                          Icon(Icons.folder_open_outlined, size: 48, color: Colors.grey[400]),
                          const SizedBox(height: 12),
                          Text('No materials uploaded yet',
                              style: TextStyle(color: Colors.grey[600], fontSize: 14)),
                        ],
                      ),
                    ),
                  );
                }

                return GridView.builder(
                  gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                    crossAxisCount: 2,
                    crossAxisSpacing: 12,
                    mainAxisSpacing: 12,
                    childAspectRatio: 0.85,
                  ),
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  itemCount: materials.length,
                  itemBuilder: (context, index) => _buildMaterialCard(materials[index]),
                );
              },
            ),

            const SizedBox(height: 32),
          ],
        ),
      ),
    );
  }

  Widget _buildMaterialCard(LessonMaterial material) {
    return GestureDetector(
      onTap: () {
        _materialsService.incrementDownloadCount(material.id);
        _downloadFile(material.cloudinaryUrl, '${material.lessonTitle}.pdf');
      },
      child: Container(
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
          boxShadow: [
            BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 8, offset: const Offset(0, 2)),
          ],
        ),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: Colors.red.withOpacity(0.1),
                shape: BoxShape.circle,
              ),
              child: const Icon(Icons.picture_as_pdf, color: Colors.red, size: 36),
            ),
            const SizedBox(height: 12),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 8),
              child: Text(
                material.lessonTitle,
                textAlign: TextAlign.center,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(
                    fontSize: 13, fontWeight: FontWeight.w600, color: Colors.black87, height: 1.3),
              ),
            ),
            const SizedBox(height: 6),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 8),
              child: Text(
                'PDF • ${material.getFileSizeString()}',
                textAlign: TextAlign.center,
                style: const TextStyle(fontSize: 11, color: Colors.grey),
              ),
            ),
            const SizedBox(height: 8),
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(Icons.download, size: 14, color: Colors.grey[500]),
                const SizedBox(width: 4),
                Text('Download', style: TextStyle(fontSize: 11, color: Colors.grey[600])),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
