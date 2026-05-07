import 'package:flutter/material.dart';
import '../quiz/quiz_details_page.dart';


class LessonQuizzesPage extends StatefulWidget {
  const LessonQuizzesPage({super.key, this.lessonTitle = 'Force'});

  final String lessonTitle;

  @override
  State<LessonQuizzesPage> createState() => _LessonQuizzesPageState();
}

class _LessonQuizzesPageState extends State<LessonQuizzesPage> {
  String _selectedQuizTitle = 'Force 1.1';

  void _selectQuiz(String title, bool isLocked) {
    if (isLocked) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: const Text('Complete the previous quiz to unlock this one.'),
          behavior: SnackBarBehavior.floating,
          shape:
              RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
        ),
      );
      return;
    }

    setState(() => _selectedQuizTitle = title);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FE),
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        leading: IconButton(
          onPressed: () => Navigator.of(context).maybePop(),
          icon: const Icon(Icons.arrow_back, color: Color(0xFF2196F3)),
        ),
        title: Text(
          'Quizzes - ${widget.lessonTitle}',
          style: const TextStyle(
            color: Color(0xFF1A1C1E),
            fontWeight: FontWeight.bold,
            fontSize: 18,
          ),
        ),
        centerTitle: true,
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16),
            child: CircleAvatar(
              radius: 18,
              backgroundColor: const Color(0xFFE8F1FF),
              child: Icon(
                Icons.person_outline,
                size: 20,
                color: const Color.fromARGB(255, 90, 162, 245),
              ),
            ),
          ),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                _buildStatCard('MODULE\nPROGRESS', '10%', hasProgress: true),
                const SizedBox(width: 15),
                _buildStatCard(
                  'MASTERY',
                  '--/100',
                  subtitle: 'Complete quizzes to unlock',
                ),
              ],
            ),
            const SizedBox(height: 30),
            const Text(
              'Required Assessments',
              style: TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.bold,
                color: Color(0xFF1A1C1E),
              ),
            ),
            const SizedBox(height: 15),
            _buildAssessmentCard(
              status: 'UNLOCKED',
              title: 'Force 1.1',
              subtitle: "Newton's First Law",
              isLocked: false,
              details: '15 mins  -  10 Questions',
            ),
            _buildAssessmentCard(
              status: 'LOCKED',
              title: 'Force 1.2',
              subtitle: 'F = ma (Second Law)',
            ),
            _buildAssessmentCard(
              status: 'LOCKED',
              title: 'Force 1.3',
              subtitle: 'Action & Reaction',
            ),
            _buildAssessmentCard(
              status: 'LOCKED',
              title: 'Force 1.4',
              subtitle: 'Friction Fundamentals',
            ),
            _buildAssessmentCard(
              status: 'LOCKED - MODULE EXAM',
              title: 'Force Unit Mastery',
              subtitle: 'Comprehensive Review',
              isExam: true,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildStatCard(
    String title,
    String value, {
    bool hasProgress = false,
    String? subtitle,
  }) {
    return Expanded(
      child: Container(
        padding: const EdgeInsets.all(16),
        height: 140,
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(color: Colors.grey.shade200),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              title,
              style: const TextStyle(
                color: Colors.grey,
                fontWeight: FontWeight.bold,
                fontSize: 12,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              value,
              style: const TextStyle(
                color: Color(0xFF2196F3),
                fontWeight: FontWeight.bold,
                fontSize: 20,
              ),
            ),
            if (hasProgress) ...[
              const Spacer(),
              LinearProgressIndicator(
                value: 0.1,
                backgroundColor: Colors.blue.shade50,
                color: Colors.blue,
                minHeight: 6,
                borderRadius: BorderRadius.circular(10),
              ),
            ],
            if (subtitle != null) ...[
              const SizedBox(height: 4),
              Text(
                subtitle,
                style: const TextStyle(color: Colors.grey, fontSize: 11),
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _buildAssessmentCard({
    required String status,
    required String title,
    required String subtitle,
    bool isLocked = true,
    bool isExam = false,
    String? details,
  }) {
    final isSelected = title == _selectedQuizTitle;

    return GestureDetector(
      onTap: () => _selectQuiz(title, isLocked),
      child: Container(
        margin: const EdgeInsets.only(bottom: 12),
        decoration: BoxDecoration(
          color: isLocked ? const Color(0xFFFBFBFF) : Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: isSelected ? const Color(0xFF2196F3) : Colors.grey.shade200,
            width: isSelected ? 2 : 1,
          ),
        ),
        child: IntrinsicHeight(
          child: Row(
            children: [
              if (!isLocked)
                Container(
                  width: 5,
                  decoration: const BoxDecoration(
                    color: Color(0xFF2196F3),
                    borderRadius: BorderRadius.only(
                      topLeft: Radius.circular(12),
                      bottomLeft: Radius.circular(12),
                    ),
                  ),
                ),
              Expanded(
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.all(10),
                        decoration: BoxDecoration(
                          color: const Color(0xFFE8F1FF),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Icon(
                          isLocked
                              ? Icons.lock_outline
                              : isExam
                                  ? Icons.workspace_premium_outlined
                                  : Icons.assignment_outlined,
                          color: const Color(0xFF2196F3),
                        ),
                      ),
                      const SizedBox(width: 15),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              status,
                              style: TextStyle(
                                color: isLocked
                                    ? Colors.grey
                                    : const Color(0xFF2196F3),
                                fontWeight: FontWeight.bold,
                                fontSize: 12,
                              ),
                            ),
                            Text(
                              title,
                              style: const TextStyle(
                                fontWeight: FontWeight.bold,
                                fontSize: 15,
                              ),
                            ),
                            Text(
                              subtitle,
                              style: const TextStyle(
                                color: Colors.grey,
                                fontSize: 14,
                              ),
                            ),
                            if (details != null) ...[
                              const SizedBox(height: 8),
                              Text(
                                details,
                                style: const TextStyle(
                                  color: Colors.grey,
                                  fontSize: 12,
                                ),
                              ),
                            ],
                          ],
                        ),
                      ),
                      if (!isLocked)
                        ElevatedButton(
                          onPressed: () {
                            Navigator.of(context).push(
                              MaterialPageRoute(
                                builder: (context) => QuizDetailScreen(
                                  quizTitle: title,
                                  chapterTitle: subtitle,
                                ),
                              ),
                            );
                          },
                          style: ElevatedButton.styleFrom(
                            backgroundColor: const Color(0xFF2196F3),
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(8),
                            ),
                          ),
                          child: const Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              Text(
                                'Start',
                                style: TextStyle(color: Colors.white),
                              ),
                              SizedBox(width: 4),
                              Icon(
                                Icons.arrow_forward,
                                size: 16,
                                color: Colors.white,
                              ),
                            ],
                          ),
                        ),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
