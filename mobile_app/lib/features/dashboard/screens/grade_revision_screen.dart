import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../LessonList/lesson_list_page.dart';
import '../../lessons/Lessons_Dashboard.dart';
import '../../admin/models/lesson_material.dart';
import '../../admin/services/materials_service.dart';


class GradeRevisionScreen extends StatefulWidget {
  final int revisionGrade; // always 10 for Grade 11 students

  const GradeRevisionScreen({super.key, this.revisionGrade = 10});

  @override
  State<GradeRevisionScreen> createState() => _GradeRevisionScreenState();
}

class _GradeRevisionScreenState extends State<GradeRevisionScreen>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final gradeLabel = 'Grade ${widget.revisionGrade}';

    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FE),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Color(0xFF7C3AED)),
          onPressed: () => Navigator.pop(context),
        ),
        title: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('Revision Mode',
                style: TextStyle(
                    color: Color(0xFF7C3AED),
                    fontSize: 16,
                    fontWeight: FontWeight.w800)),
            Text(gradeLabel,
                style: const TextStyle(color: Colors.grey, fontSize: 12)),
          ],
        ),
        bottom: TabBar(
          controller: _tabController,
          labelColor: const Color(0xFF7C3AED),
          unselectedLabelColor: Colors.grey,
          indicatorColor: const Color(0xFF7C3AED),
          tabs: const [
            Tab(icon: Icon(Icons.menu_book_outlined), text: 'Lessons'),
            Tab(icon: Icon(Icons.picture_as_pdf_outlined), text: 'Materials'),
          ],
        ),
      ),
      body: Column(
        children: [
          // Revision mode banner
          Container(
            width: double.infinity,
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
            decoration: BoxDecoration(
              gradient: const LinearGradient(
                colors: [Color(0xFF7C3AED), Color(0xFF9F67FA)],
              ),
            ),
            child: Row(
              children: [
                const Icon(Icons.history_edu, color: Colors.white, size: 18),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    'Practising $gradeLabel syllabus for revision',
                    style: const TextStyle(color: Colors.white, fontSize: 12),
                  ),
                ),
                GestureDetector(
                  onTap: () => Navigator.pop(context),
                  child: Container(
                    padding:
                        const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                    decoration: BoxDecoration(
                      color: Colors.white.withOpacity(0.25),
                      borderRadius: BorderRadius.circular(20),
                    ),
                    child: const Text('Exit',
                        style: TextStyle(
                            color: Colors.white,
                            fontSize: 11,
                            fontWeight: FontWeight.w700)),
                  ),
                ),
              ],
            ),
          ),

          Expanded(
            child: TabBarView(
              controller: _tabController,
              children: [
                _RevisionLessonsTab(grade: widget.revisionGrade),
                _RevisionMaterialsTab(grade: widget.revisionGrade),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

// ── Lessons Tab ───────────────────────────────────────────────────────────────
class _RevisionLessonsTab extends StatelessWidget {
  final int grade;
  const _RevisionLessonsTab({required this.grade});

  @override
  Widget build(BuildContext context) {
    final query = FirebaseFirestore.instance
        .collection('lessons')
        .where('grade', isEqualTo: grade)
        .where('status', isEqualTo: 'published')
        .orderBy('order');

    return StreamBuilder<QuerySnapshot>(
      stream: query.snapshots(),
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return const Center(
              child: CircularProgressIndicator(color: Color(0xFF7C3AED)));
        }

        final docs = snapshot.data?.docs ?? [];

        if (docs.isEmpty) {
          return Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(Icons.menu_book_outlined, size: 56, color: Colors.grey[300]),
                const SizedBox(height: 12),
                Text('No Grade $grade lessons available',
                    style: TextStyle(color: Colors.grey[500], fontSize: 14)),
              ],
            ),
          );
        }

        return ListView.builder(
          padding: const EdgeInsets.all(16),
          itemCount: docs.length,
          itemBuilder: (context, index) {
            final doc = docs[index];
            final data = doc.data() as Map<String, dynamic>;
            final title = data['title'] ?? 'Lesson ${index + 1}';
            final description = data['description'] ?? '';
            final topic = data['topic'] ?? '';

            return GestureDetector(
              onTap: () => Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (_) => LessonsDashboard(
                    lessonId: doc.id,
                    lessonTitle: title,
                    grade: 'Grade $grade',
                    lessonDescription: description,
                  ),
                ),
              ),
              child: Container(
                margin: const EdgeInsets.only(bottom: 12),
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(12),
                  boxShadow: [
                    BoxShadow(
                        color: Colors.black.withOpacity(0.05),
                        blurRadius: 8,
                        offset: const Offset(0, 2)),
                  ],
                ),
                child: Row(
                  children: [
                    Container(
                      width: 44,
                      height: 44,
                      decoration: BoxDecoration(
                        color: const Color(0xFF7C3AED).withOpacity(0.1),
                        borderRadius: BorderRadius.circular(10),
                      ),
                      child: Center(
                        child: Text('${index + 1}',
                            style: const TextStyle(
                                color: Color(0xFF7C3AED),
                                fontWeight: FontWeight.w800,
                                fontSize: 16)),
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(title,
                              style: const TextStyle(
                                  fontSize: 14,
                                  fontWeight: FontWeight.w700,
                                  color: Color(0xFF1A1C1E))),
                          if (topic.isNotEmpty) ...[
                            const SizedBox(height: 2),
                            Text(topic,
                                style: TextStyle(
                                    fontSize: 12, color: Colors.grey[600])),
                          ],
                        ],
                      ),
                    ),
                    const Icon(Icons.chevron_right,
                        color: Color(0xFF7C3AED), size: 20),
                  ],
                ),
              ),
            );
          },
        );
      },
    );
  }
}

// ── Materials Tab ─────────────────────────────────────────────────────────────
class _RevisionMaterialsTab extends StatefulWidget {
  final int grade;
  const _RevisionMaterialsTab({required this.grade});

  @override
  State<_RevisionMaterialsTab> createState() => _RevisionMaterialsTabState();
}

class _RevisionMaterialsTabState extends State<_RevisionMaterialsTab> {
  final MaterialsService _service = MaterialsService();
  late Future<List<LessonMaterial>> _future;

  @override
  void initState() {
    super.initState();
    _future = _service.getMaterialsForGrade('Grade ${widget.grade}');
  }

  void _download(String url, String name) {
    launchUrl(Uri.parse(url), mode: LaunchMode.externalApplication);
  }

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<List<LessonMaterial>>(
      future: _future,
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return const Center(
              child: CircularProgressIndicator(color: Color(0xFF7C3AED)));
        }

        final materials = snapshot.data ?? [];

        if (materials.isEmpty) {
          return Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(Icons.folder_open_outlined,
                    size: 56, color: Colors.grey[300]),
                const SizedBox(height: 12),
                Text('No Grade ${widget.grade} materials yet',
                    style: TextStyle(color: Colors.grey[500], fontSize: 14)),
              ],
            ),
          );
        }

        // Group by lesson title
        final grouped = <String, List<LessonMaterial>>{};
        for (final m in materials) {
          grouped.putIfAbsent(m.lessonTitle, () => []).add(m);
        }

        return ListView.builder(
          padding: const EdgeInsets.all(16),
          itemCount: grouped.length,
          itemBuilder: (context, i) {
            final lessonTitle = grouped.keys.elementAt(i);
            final items = grouped[lessonTitle]!;

            return Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Padding(
                  padding: const EdgeInsets.only(bottom: 8, top: 4),
                  child: Text(lessonTitle,
                      style: const TextStyle(
                          fontSize: 13,
                          fontWeight: FontWeight.w700,
                          color: Color(0xFF7C3AED))),
                ),
                SizedBox(
                  height: 130,
                  child: ListView.separated(
                    scrollDirection: Axis.horizontal,
                    itemCount: items.length,
                    separatorBuilder: (_, __) => const SizedBox(width: 8),
                    itemBuilder: (_, j) {
                      final m = items[j];
                      return GestureDetector(
                        onTap: () {
                          _service.incrementDownloadCount(m.id);
                          _download(m.cloudinaryUrl, m.lessonTitle);
                        },
                        child: Container(
                          width: 120,
                          decoration: BoxDecoration(
                            color: Colors.white,
                            borderRadius: BorderRadius.circular(10),
                            boxShadow: [
                              BoxShadow(
                                  color: Colors.black.withOpacity(0.06),
                                  blurRadius: 4)
                            ],
                          ),
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              Container(
                                padding: const EdgeInsets.all(10),
                                decoration: BoxDecoration(
                                  color:
                                      const Color(0xFF7C3AED).withOpacity(0.08),
                                  shape: BoxShape.circle,
                                ),
                                child: const Icon(Icons.picture_as_pdf,
                                    color: Color(0xFF7C3AED), size: 28),
                              ),
                              const SizedBox(height: 8),
                              Padding(
                                padding:
                                    const EdgeInsets.symmetric(horizontal: 8),
                                child: Text(m.lessonTitle,
                                    maxLines: 2,
                                    overflow: TextOverflow.ellipsis,
                                    textAlign: TextAlign.center,
                                    style: const TextStyle(
                                        fontSize: 10,
                                        fontWeight: FontWeight.w600)),
                              ),
                              const SizedBox(height: 4),
                              Row(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: [
                                  Icon(Icons.download,
                                      size: 11, color: Colors.grey[500]),
                                  const SizedBox(width: 2),
                                  Text(m.getFileSizeString(),
                                      style: TextStyle(
                                          fontSize: 9,
                                          color: Colors.grey[500])),
                                ],
                              ),
                            ],
                          ),
                        ),
                      );
                    },
                  ),
                ),
                const SizedBox(height: 16),
              ],
            );
          },
        );
      },
    );
  }
}