import 'package:flutter/material.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:firebase_auth/firebase_auth.dart';
import '../quizzes/student_quiz_screen.dart';

class SubLessonsScreen extends StatefulWidget {
  final String lessonId;
  final String lessonTitle;
  final String grade;

  const SubLessonsScreen({
    super.key,
    required this.lessonId,
    required this.lessonTitle,
    required this.grade,
  });

  @override
  State<SubLessonsScreen> createState() => _SubLessonsScreenState();
}

class _SubLessonsScreenState extends State<SubLessonsScreen> {
  List<Map<String, dynamic>> _subLessons = [];
  // subLessonId → progress doc
  Map<String, Map<String, dynamic>> _progress = {};
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final snap = await FirebaseFirestore.instance
          .collection('lessons')
          .doc(widget.lessonId)
          .collection('subLessons')
          .orderBy('order')
          .get();

      final subLessons = snap.docs.map((d) {
        final data = Map<String, dynamic>.from(d.data());
        data['id'] = d.id;
        return data;
      }).toList();

      final uid = FirebaseAuth.instance.currentUser?.uid;
      final Map<String, Map<String, dynamic>> progress = {};

      if (uid != null) {
        final progSnap = await FirebaseFirestore.instance
            .collection('users')
            .doc(uid)
            .collection('subLessonProgress')
            .where('lessonId', isEqualTo: widget.lessonId)
            .get();

        for (final doc in progSnap.docs) {
          final data = doc.data();
          final sid = data['subLessonId'] as String?;
          if (sid != null) progress[sid] = data;
        }
      }

      setState(() {
        _subLessons = subLessons;
        _progress = progress;
        _loading = false;
      });
    } catch (_) {
      setState(() => _loading = false);
    }
  }

  // First sub-lesson is always unlocked; subsequent ones require previous completed ≥70%.
  bool _isUnlocked(int index) {
    if (index == 0) return true;
    final prev = _subLessons[index - 1];
    final p = _progress[prev['id'] as String];
    if (p == null) return false;
    return (p['isCompleted'] as bool? ?? false) &&
        (p['bestScore'] as int? ?? 0) >= 70;
  }

  bool _isCompleted(String subLessonId) {
    final p = _progress[subLessonId];
    return p != null && (p['isCompleted'] as bool? ?? false);
  }

  int _bestScore(String subLessonId) =>
      (_progress[subLessonId]?['bestScore'] as int?) ?? 0;

  bool _isPublished(Map<String, dynamic> sl) =>
      sl['isPublished'] as bool? ?? false;

  // ── UI ─────────────────────────────────────────────────────────────────────

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F6FA),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Color(0xFF2196F3)),
          onPressed: () => Navigator.pop(context),
        ),
        title: Text(
          widget.lessonTitle,
          style: const TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 16,
              color: Color(0xFF1A1C1E)),
        ),
        centerTitle: true,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh, color: Color(0xFF2196F3)),
            onPressed: _load,
          ),
        ],
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _subLessons.isEmpty
              ? _buildEmpty()
              : _buildList(),
    );
  }

  Widget _buildEmpty() {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.quiz_outlined, size: 64, color: Colors.grey.shade300),
          const SizedBox(height: 16),
          const Text('No sub-lessons available',
              style: TextStyle(fontSize: 16, color: Colors.grey)),
          const SizedBox(height: 8),
          const Text('Check back later!',
              style: TextStyle(color: Colors.grey)),
        ],
      ),
    );
  }

  Widget _buildList() {
    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          // Info banner
          Container(
            padding: const EdgeInsets.all(14),
            margin: const EdgeInsets.only(bottom: 16),
            decoration: BoxDecoration(
              color: const Color(0xFFE8F1FB),
              borderRadius: BorderRadius.circular(12),
            ),
            child: const Row(children: [
              Icon(Icons.info_outline, color: Color(0xFF2196F3), size: 20),
              SizedBox(width: 10),
              Expanded(
                child: Text(
                  'Score 70% or more on each sub-lesson to unlock the next one.',
                  style: TextStyle(color: Color(0xFF2196F3), fontSize: 13),
                ),
              ),
            ]),
          ),

          // Sub-lesson cards
          ...List.generate(_subLessons.length, (i) {
            final sl = _subLessons[i];
            final id = sl['id'] as String;
            final published = _isPublished(sl);
            final unlocked = _isUnlocked(i);
            final completed = _isCompleted(id);
            final score = _bestScore(id);
            return _buildCard(sl, i, published, unlocked, completed, score);
          }),
        ],
      ),
    );
  }

  Widget _buildCard(
    Map<String, dynamic> sl,
    int index,
    bool published,
    bool unlocked,
    bool completed,
    int score,
  ) {
    final id = sl['id'] as String;
    final number = sl['number'] as String? ?? '';
    final title = sl['title'] as String? ?? '';
    final quizCount = sl['quizCount'] as int? ?? 10;

    // Determine visual state
    final bool canTap = published && unlocked;
    final Color accentColor = completed
        ? Colors.green
        : (canTap ? const Color(0xFF2196F3) : Colors.grey);

    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(
          color: completed
              ? Colors.green.withOpacity(0.3)
              : canTap
                  ? const Color(0xFF2196F3).withOpacity(0.25)
                  : Colors.grey.shade200,
          width: 1.5,
        ),
        boxShadow: [
          BoxShadow(
              color: Colors.black.withOpacity(0.04),
              blurRadius: 8,
              offset: const Offset(0, 2))
        ],
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          borderRadius: BorderRadius.circular(14),
          onTap: canTap
              ? () async {
                  await Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (_) => StudentQuizScreen(
                        lessonId: widget.lessonId,
                        lessonTitle: widget.lessonTitle,
                        subLessonId: id,
                        subLessonNumber: number,
                        subLessonTitle: title,
                        subLessonOrder: index,
                        totalSubLessons: _subLessons.length,
                        quizCount: quizCount,
                      ),
                    ),
                  );
                  _load(); // refresh unlock state after returning
                }
              : null,
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Row(children: [
              // Number badge
              Container(
                width: 52,
                height: 52,
                decoration: BoxDecoration(
                  color: accentColor.withOpacity(0.1),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Center(
                  child: Text(
                    number,
                    style: TextStyle(
                        fontWeight: FontWeight.bold,
                        fontSize: 15,
                        color: accentColor),
                  ),
                ),
              ),
              const SizedBox(width: 14),

              // Title + status
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      title,
                      style: TextStyle(
                          fontWeight: FontWeight.w600,
                          fontSize: 15,
                          color: canTap
                              ? const Color(0xFF1A1C1E)
                              : Colors.grey.shade500),
                    ),
                    const SizedBox(height: 4),
                    _statusText(published, unlocked, completed, score),
                  ],
                ),
              ),
              const SizedBox(width: 12),

              // Right indicator
              _rightIcon(published, unlocked, completed, score),
            ]),
          ),
        ),
      ),
    );
  }

  Widget _statusText(
      bool published, bool unlocked, bool completed, int score) {
    if (!published) {
      return const Text('Not available yet',
          style: TextStyle(color: Colors.grey, fontSize: 12));
    }
    if (completed) {
      return Row(children: [
        const Icon(Icons.check_circle_rounded,
            color: Colors.green, size: 13),
        const SizedBox(width: 4),
        Text('Completed · Best: $score%',
            style:
                const TextStyle(color: Colors.green, fontSize: 12)),
      ]);
    }
    if (unlocked) {
      return const Text('Tap to start quiz',
          style:
              TextStyle(color: Color(0xFF2196F3), fontSize: 12));
    }
    return const Text('Complete the previous sub-lesson first',
        style: TextStyle(color: Colors.grey, fontSize: 12));
  }

  Widget _rightIcon(
      bool published, bool unlocked, bool completed, int score) {
    if (!published) {
      return Icon(Icons.schedule_rounded,
          color: Colors.grey.shade400, size: 22);
    }
    if (completed) {
      return Container(
        padding:
            const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
        decoration: BoxDecoration(
          color: Colors.green.withOpacity(0.1),
          borderRadius: BorderRadius.circular(20),
        ),
        child: Text('$score%',
            style: const TextStyle(
                color: Colors.green,
                fontWeight: FontWeight.bold,
                fontSize: 13)),
      );
    }
    if (unlocked) {
      return Container(
        padding: const EdgeInsets.all(8),
        decoration: BoxDecoration(
          color: const Color(0xFF2196F3),
          borderRadius: BorderRadius.circular(10),
        ),
        child: const Icon(Icons.play_arrow_rounded,
            color: Colors.white, size: 20),
      );
    }
    return Icon(Icons.lock_rounded,
        color: Colors.grey.shade400, size: 22);
  }
}
