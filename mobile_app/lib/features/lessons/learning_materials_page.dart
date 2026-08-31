import 'dart:async';
import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';
import '../admin/models/lesson_material.dart';
import '../admin/services/materials_service.dart';

class LearningMaterialsPage extends StatefulWidget {
  final String lessonId;
  final String lessonTitle;
  final String grade;

  const LearningMaterialsPage({
    super.key,
    required this.lessonId,
    this.lessonTitle = 'Lesson',
    this.grade = 'Grade 9',
  });

  @override
  State<LearningMaterialsPage> createState() => _LearningMaterialsPageState();
}

class _LearningMaterialsPageState extends State<LearningMaterialsPage> {
  static const Color _primaryBlue = Color(0xFF2196F3);
  final MaterialsService _service = MaterialsService();
  late Future<List<LessonMaterial>> _future;

  @override
  void initState() {
    super.initState();
    _future = _service.getMaterialsForLesson(
      widget.lessonId,
      grade: widget.grade,
      lessonTitle: widget.lessonTitle,
    );
  }

  // ─── Download PDF File ────────────────────────────────────────────────────────
  void _downloadMaterial(LessonMaterial m) {
    _service.incrementDownloadCount(m.id);

    // Format file name to ensure it has .pdf extension
    String fileName = m.materialName.trim();
    if (!fileName.toLowerCase().endsWith('.pdf')) {
      fileName = '$fileName.pdf';
    }

    if (mounted) {
      ScaffoldMessenger.of(context).hideCurrentSnackBar();
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Row(
            children: [
              const Icon(Icons.download_done_rounded, color: Colors.white, size: 18),
              const SizedBox(width: 10),
              Expanded(
                child: Text(
                  'Downloading $fileName...',
                  overflow: TextOverflow.ellipsis,
                ),
              ),
            ],
          ),
          duration: const Duration(seconds: 3),
          backgroundColor: _primaryBlue,
        ),
      );
    }

    // Transform Cloudinary URL to include fl_attachment to force direct browser attachment download
    // This avoids XHR CORS 401 Unauthorized errors when fetching raw files via JS http.get
    String downloadUrl = m.cloudinaryUrl;
    if (downloadUrl.contains('/upload/') && !downloadUrl.contains('fl_attachment')) {
      downloadUrl = downloadUrl.replaceAll('/upload/', '/upload/fl_attachment/');
    }

    final uri = Uri.tryParse(downloadUrl);
    if (uri != null) {
      launchUrl(uri, mode: LaunchMode.externalApplication);
    }
  }

  // ─── Build ───────────────────────────────────────────────────────────────────
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F6FA),
      appBar: AppBar(
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: _primaryBlue),
          onPressed: () => Navigator.pop(context),
        ),
        title: const Text(
          'Learning Materials',
          style: TextStyle(
              color: Colors.black87, fontWeight: FontWeight.bold, fontSize: 17),
        ),
        centerTitle: true,
        backgroundColor: Colors.white,
        elevation: 0,
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16.0),
            child: GestureDetector(
              onTap: () => Navigator.pushNamed(context, '/profile'),
              child: const CircleAvatar(
                radius: 18,
                backgroundColor: Color(0xFFCCCCCC),
                child: Icon(Icons.person, color: Colors.white, size: 22),
              ),
            ),
          ),
        ],
      ),
      body: FutureBuilder<List<LessonMaterial>>(
        future: _future,
        builder: (context, snap) {
          // Loading
          if (snap.connectionState == ConnectionState.waiting) {
            return const Center(
              child: CircularProgressIndicator(color: _primaryBlue),
            );
          }

          // Error
          if (snap.hasError) {
            return _buildEmpty(
              icon: Icons.error_outline,
              color: Colors.red.shade300,
              title: 'Could not load materials',
              subtitle: snap.error.toString(),
            );
          }

          final materials = snap.data ?? [];

          return SingleChildScrollView(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // ── Lesson header card ──────────────────────────────────
                Container(
                  width: double.infinity,
                  decoration: BoxDecoration(
                    color: const Color(0xFFE8F1FB),
                    borderRadius: BorderRadius.circular(16),
                  ),
                  padding: const EdgeInsets.all(20),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        widget.grade,
                        style: const TextStyle(
                          fontSize: 12,
                          color: _primaryBlue,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                      const SizedBox(height: 6),
                      Text(
                        widget.lessonTitle,
                        style: const TextStyle(
                          fontSize: 20,
                          fontWeight: FontWeight.bold,
                          color: Colors.black87,
                        ),
                      ),
                      const SizedBox(height: 6),
                      Row(
                        children: [
                          const Icon(Icons.folder_open,
                              size: 14, color: Colors.grey),
                          const SizedBox(width: 4),
                          Text(
                            materials.isEmpty
                                ? 'No materials uploaded yet'
                                : '${materials.length} PDF material${materials.length != 1 ? 's' : ''} available',
                            style: const TextStyle(
                                fontSize: 12, color: Colors.grey),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 24),

                // ── Empty state ─────────────────────────────────────────
                if (materials.isEmpty)
                  _buildEmpty(
                    icon: Icons.folder_open_outlined,
                    color: Colors.grey.shade300,
                    title: 'No materials yet',
                    subtitle:
                        'Your teacher hasn\'t uploaded any PDF materials for this lesson yet. Check back later.',
                  )
                else ...[
                  Text(
                    'Available PDFs (${materials.length})',
                    style: const TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                      color: Colors.black87,
                    ),
                  ),
                  const SizedBox(height: 12),

                  // ── Material grid ─────────────────────────────────────
                  GridView.builder(
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    gridDelegate:
                        const SliverGridDelegateWithFixedCrossAxisCount(
                      crossAxisCount: 2,
                      crossAxisSpacing: 12,
                      mainAxisSpacing: 12,
                      childAspectRatio: 0.80,
                    ),
                    itemCount: materials.length,
                    itemBuilder: (_, i) => _buildMaterialCard(materials[i]),
                  ),
                ],

                const SizedBox(height: 32),
              ],
            ),
          );
        },
      ),
    );
  }

  // ─── Material card ────────────────────────────────────────────────────────────
  Widget _buildMaterialCard(LessonMaterial m) {
    const color = Color(0xFFE53E3E); // Red for PDF
    const icon = Icons.picture_as_pdf;

    return GestureDetector(
      onTap: () => _downloadMaterial(m),
      child: Container(
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(14),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.06),
              blurRadius: 8,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            // Icon circle
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: color.withOpacity(0.1),
                shape: BoxShape.circle,
              ),
              child: const Icon(icon, color: color, size: 34),
            ),
            const SizedBox(height: 10),

            // File name
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 10),
              child: Text(
                m.materialName,
                textAlign: TextAlign.center,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(
                  fontSize: 12,
                  fontWeight: FontWeight.w600,
                  color: Colors.black87,
                  height: 1.3,
                ),
              ),
            ),
            const SizedBox(height: 6),

            // File size + type chip
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                  decoration: BoxDecoration(
                    color: color.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(4),
                  ),
                  child: const Text(
                    'PDF',
                    style: TextStyle(
                        fontSize: 9,
                        fontWeight: FontWeight.w700,
                        color: color),
                  ),
                ),
                const SizedBox(width: 4),
                Text(
                  m.getFileSizeString(),
                  style:
                      TextStyle(fontSize: 10, color: Colors.grey.shade500),
                ),
              ],
            ),
            const SizedBox(height: 10),

            // Single Download PDF Button
            Container(
              padding:
                  const EdgeInsets.symmetric(horizontal: 14, vertical: 6),
              decoration: BoxDecoration(
                color: _primaryBlue.withOpacity(0.1),
                borderRadius: BorderRadius.circular(20),
              ),
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: const [
                  Icon(Icons.download_rounded, size: 14, color: _primaryBlue),
                  SizedBox(width: 4),
                  Text(
                    'Download PDF',
                    style: TextStyle(
                      fontSize: 11,
                      color: _primaryBlue,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  // ─── Empty / error widget ─────────────────────────────────────────────────────
  Widget _buildEmpty({
    required IconData icon,
    required Color color,
    required String title,
    required String subtitle,
  }) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 48),
      child: Center(
        child: Column(
          children: [
            Icon(icon, size: 72, color: color),
            const SizedBox(height: 16),
            Text(title,
                style: const TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.bold,
                    color: Colors.black54)),
            const SizedBox(height: 8),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 32),
              child: Text(subtitle,
                  textAlign: TextAlign.center,
                  style:
                      TextStyle(fontSize: 13, color: Colors.grey.shade500)),
            ),
          ],
        ),
      ),
    );
  }
}
