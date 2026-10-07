import 'package:flutter/material.dart';

import '../lessons/data/lesson_engagement_repository.dart';
import 'data/study_plan_repository.dart';

enum _TodoKind { lesson, quiz }

class _TodoItem {
  _TodoItem({
    required this.key,
    required this.kind,
    required this.label,
    required this.checked,
    this.lessonId,
  });

  final String key;
  final _TodoKind kind;
  final String label;
  bool checked;
  final String? lessonId;
}

/// This week's personalized study plan — generated server-side from the
/// student's real weak topics + lesson engagement (see
/// study_plan_service.py), refreshed automatically every ISO week.
///
/// Plain to-do list, no navigation out of this screen. Tapping a row only
/// toggles its own checkbox — for a lesson item, checking it also records a
/// real "completed" engagement event (see lesson_engagement_service.py), so
/// the checklist itself is a genuine engagement signal, not just decoration.
class WeeklyPlanScreen extends StatefulWidget {
  const WeeklyPlanScreen({super.key});

  @override
  State<WeeklyPlanScreen> createState() => _WeeklyPlanScreenState();
}

class _WeeklyPlanScreenState extends State<WeeklyPlanScreen> {
  static const Color _brand = Color(0xFF2196F3);

  late Future<Map<String, dynamic>> _planFuture;
  final Map<String, List<_TodoItem>> _itemsBySection = {};

  @override
  void initState() {
    super.initState();
    _planFuture = StudyPlanRepository().getCurrentPlan();
  }

  Future<void> _reload() async {
    _itemsBySection.clear();
    setState(() => _planFuture = StudyPlanRepository().getCurrentPlan());
    await _planFuture;
  }

  void _toggle(_TodoItem item) {
    setState(() => item.checked = !item.checked);
    if (item.kind == _TodoKind.lesson &&
        item.checked &&
        item.lessonId != null) {
      // Fire-and-forget — a flaky network shouldn't block the checkbox from
      // responding; the real completion gets recorded next time the
      // student actually opens the lesson if this call happens to fail.
      LessonEngagementRepository()
          .recordCompleted(item.lessonId!)
          .catchError((_) {});
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FE),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: _brand),
          onPressed: () => Navigator.pop(context),
        ),
        title: const Text("This Week's Plan",
            style: TextStyle(
                color: Color(0xFF1A1C1E), fontWeight: FontWeight.bold)),
      ),
      body: FutureBuilder<Map<String, dynamic>>(
        future: _planFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState != ConnectionState.done) {
            return const Center(
                child: CircularProgressIndicator(color: _brand));
          }
          if (snapshot.hasError) {
            return _errorState(snapshot.error.toString());
          }
          final plan = snapshot.data ?? {};
          return RefreshIndicator(
            onRefresh: _reload,
            child: ListView(
              padding: const EdgeInsets.all(20),
              children: [
                _overviewCard(plan),
                const SizedBox(height: 20),
                ..._todoSections(plan),
                ..._explorationSection(plan),
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _errorState(String message) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.cloud_off, size: 40, color: Colors.grey),
            const SizedBox(height: 12),
            Text("Couldn't load your plan right now.\n$message",
                textAlign: TextAlign.center,
                style: const TextStyle(color: Colors.grey)),
            const SizedBox(height: 16),
            ElevatedButton(onPressed: _reload, child: const Text('Try again')),
          ],
        ),
      ),
    );
  }

  Widget _overviewCard(Map<String, dynamic> plan) {
    final fallbackMode = plan['fallbackMode'] == true;
    final summary = Map<String, dynamic>.from(plan['engagementSummary'] ?? {});
    final opened = (summary['lessonsOpenedThisWeek'] ?? 0) as int;
    final completed = (summary['lessonsCompletedThisWeek'] ?? 0) as int;
    final quizzes = (summary['quizzesAttemptedThisWeek'] ?? 0) as int;
    final progress = opened > 0 ? (completed / opened).clamp(0.0, 1.0) : 0.0;

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(15),
        border: Border.all(color: const Color(0xFFE8F1FF)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                padding: const EdgeInsets.all(10),
                decoration: BoxDecoration(
                  color: const Color(0xFFE8F1FF),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: const Icon(Icons.calendar_today, color: _brand),
              ),
              const SizedBox(width: 15),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      fallbackMode
                          ? "No weak spots yet — here's what's next"
                          : 'Your focus areas this week',
                      style: const TextStyle(
                          fontSize: 16, fontWeight: FontWeight.bold),
                    ),
                    Text(
                      '$completed of $opened lessons completed · $quizzes quiz attempts',
                      style: const TextStyle(color: Colors.grey, fontSize: 13),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 15),
          ClipRRect(
            borderRadius: BorderRadius.circular(10),
            child: LinearProgressIndicator(
              value: progress,
              minHeight: 10,
              backgroundColor: const Color(0xFFE8F1FF),
              valueColor: const AlwaysStoppedAnimation<Color>(_brand),
            ),
          ),
        ],
      ),
    );
  }

  /// Builds (once per focus area, cached across rebuilds in
  /// _itemsBySection) one "Go through <lesson>" item per recommended
  /// lesson and one "Do the quiz for <topic>" item when a retry is
  /// suggested.
  List<Widget> _todoSections(Map<String, dynamic> plan) {
    final focusAreas = (plan['focusAreas'] as List? ?? [])
        .map((e) => Map<String, dynamic>.from(e as Map))
        .toList();

    if (focusAreas.isEmpty) {
      return [
        Container(
          width: double.infinity,
          padding: const EdgeInsets.all(18),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(12),
            border: Border.all(color: Colors.grey.shade200),
          ),
          child: const Text(
            'Nothing scheduled yet — check back after your next quiz or lesson.',
            style: TextStyle(color: Colors.grey),
          ),
        ),
      ];
    }

    final widgets = <Widget>[];
    for (final area in focusAreas) {
      // lessonTag is the stable internal key (dedup/cache key); topicName
      // is the human-readable heading actually shown to the student.
      final lessonTag = (area['lessonTag'] as String?) ?? 'topic';
      final topicName = (area['topicName'] as String?) ??
          (area['lessonTag'] as String?) ??
          'Topic';
      final reason = (area['reason'] as String?) ?? '';
      widgets.add(_sectionLabel(topicName, reason));

      final items = _itemsBySection.putIfAbsent(lessonTag, () {
        final built = <_TodoItem>[];
        final recommendedLessons = (area['recommendedLessons'] as List? ?? [])
            .map((e) => Map<String, dynamic>.from(e as Map))
            .toList();
        for (final lesson in recommendedLessons) {
          final lessonId = lesson['lessonId'] as String?;
          final title = (lesson['title'] as String?) ?? 'Lesson';
          built.add(_TodoItem(
            key: 'lesson:$lessonId',
            kind: _TodoKind.lesson,
            label: 'Go through: $title',
            checked: lesson['alreadyViewed'] == true,
            lessonId: lessonId,
          ));
        }
        if (area['suggestedQuiz'] != null) {
          built.add(_TodoItem(
            key: 'quiz:$lessonTag',
            kind: _TodoKind.quiz,
            label: 'Do the quiz for: $topicName',
            checked: false,
          ));
        }
        return built;
      });

      for (final item in items) {
        widgets.add(_todoRow(item));
      }
      widgets.add(const SizedBox(height: 10));
    }
    return widgets;
  }

  /// Lessons for the student's grade they haven't attempted at all yet —
  /// shown separately from the weakness-based items above, so the plan
  /// always points at something new to try, not just remediation.
  List<Widget> _explorationSection(Map<String, dynamic> plan) {
    final lessons = (plan['explorationLessons'] as List? ?? [])
        .map((e) => Map<String, dynamic>.from(e as Map))
        .toList();
    if (lessons.isEmpty) return [];

    final items = _itemsBySection.putIfAbsent('__explore__', () {
      return lessons.map((lesson) {
        final lessonId = lesson['lessonId'] as String?;
        final title = (lesson['title'] as String?) ?? 'Lesson';
        return _TodoItem(
          key: 'lesson:$lessonId',
          kind: _TodoKind.lesson,
          label: 'Go explore: $title',
          checked: false,
          lessonId: lessonId,
        );
      }).toList();
    });

    return [
      _sectionLabel('Explore something new', ''),
      ...items.map(_todoRow),
      const SizedBox(height: 10),
    ];
  }

  Widget _sectionLabel(String lessonTag, String reason) {
    return Padding(
      padding: const EdgeInsets.only(top: 6, bottom: 6),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(lessonTag,
              style:
                  const TextStyle(fontWeight: FontWeight.bold, fontSize: 14)),
          if (reason.isNotEmpty)
            Text(reason,
                style: TextStyle(color: Colors.grey.shade600, fontSize: 12)),
        ],
      ),
    );
  }

  Widget _todoRow(_TodoItem item) {
    return InkWell(
      borderRadius: BorderRadius.circular(10),
      onTap: () => _toggle(item),
      child: Container(
        margin: const EdgeInsets.only(bottom: 8),
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(10),
          border: Border.all(color: Colors.grey.shade200),
        ),
        child: Row(
          children: [
            Icon(
              item.checked ? Icons.check_box : Icons.check_box_outline_blank,
              size: 20,
              color: item.checked ? Colors.green : Colors.grey.shade400,
            ),
            const SizedBox(width: 10),
            Expanded(
              child: Text(
                item.label,
                style: TextStyle(
                  fontSize: 13.5,
                  color: item.checked ? Colors.grey.shade500 : Colors.black87,
                  decoration: item.checked ? TextDecoration.lineThrough : null,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
