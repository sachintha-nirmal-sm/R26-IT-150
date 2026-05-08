import 'package:flutter/material.dart';

import '../quizzes/lesson_quizzes_page.dart';
import 'dynamic_assessment_page.dart';

class QuizFailureScreen extends StatelessWidget {
  final int totalQuestions;
  final int incorrectAnswers;
  final String timeTaken;

  const QuizFailureScreen({
    super.key,
    required this.totalQuestions,
    required this.incorrectAnswers,
    required this.timeTaken,
  });

  @override
  Widget build(BuildContext context) {
    // Logic to calculate percentage
    double scorePercentage = ((totalQuestions - incorrectAnswers) / totalQuestions) * 100;

    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FE),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.close, color: Color(0xFF0056D2)),
          onPressed: () => Navigator.pop(context),
        ),
        title: const Text(
          'Result',
          style: TextStyle(color: Color(0xFF1A1C1E), fontWeight: FontWeight.bold),
        ),
        centerTitle: true,
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16.0),
            child: CircleAvatar(
              radius: 18,
              backgroundColor: const Color(0xFFCCCCCC),
              child: const Icon(Icons.person, color: Colors.white, size: 22),
            ),
          )
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Red Score Alert Box
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(25),
              decoration: BoxDecoration(
                color: const Color(0xFFFFEBEB), // Soft red background
                borderRadius: BorderRadius.circular(20),
                border: Border.all(color: const Color(0xFFFFCDCD)),
              ),
              child: Column(
                children: [
                  const Icon(Icons.warning_amber_rounded, color: Color(0xFFC62828), size: 50),
                  const SizedBox(height: 10),
                  Text(
                    '${scorePercentage.toInt()}%',
                    style: const TextStyle(
                      fontSize: 42,
                      fontWeight: FontWeight.bold,
                      color: Color(0xFFC62828),
                    ),
                  ),
                  const Text(
                    'SCORE',
                    style: TextStyle(color: Color(0xFFC62828), fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 15),
                  const Text(
                    'You are weak in this sub-lesson of Force, please learn it deeply.',
                    textAlign: TextAlign.center,
                    style: TextStyle(color: Color(0xFFC62828), fontSize: 16, fontWeight: FontWeight.w500),
                  ),
                ],
              ),
            ),
            
            const SizedBox(height: 20),
            
            // Statistics Row (Time & Incorrect)
            Row(
              children: [
                _buildStatBox(Icons.timer_outlined, timeTaken, "Time Taken"),
                const SizedBox(width: 15),
                _buildStatBox(Icons.cancel_outlined, incorrectAnswers.toString(), "Incorrect", iconColor: Colors.red),
              ],
            ),
            
            const SizedBox(height: 30),
            const Text(
              'Areas for Improvement',
              style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: Color(0xFF1A1C1E)),
            ),
            const SizedBox(height: 15),
            
            // Improvement List
            _buildImprovementTile("Newton’s Second Law", "Calculate the net force required to..."),
            _buildImprovementTile("Frictional Forces", "A block is sliding down an inclined..."),
            _buildImprovementTile("Tension and Pulleys", "Two masses are connected by a..."),
          ],
        ),
      ),
      bottomNavigationBar: Padding(
        padding: const EdgeInsets.all(20.0),
        child: SizedBox(
          width: double.infinity,
          height: 60,
          child: ElevatedButton(
            onPressed: () => Navigator.of(context).push(
              MaterialPageRoute(
                builder: (context) => const DynamicAssessmentPage(),
              ),
            ),
            style: ElevatedButton.styleFrom(
              backgroundColor: const Color(0xFF2196F3),
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(30)),
              elevation: 0,
            ),
            child: const Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(Icons.menu_book, color: Colors.white),
                SizedBox(width: 10),
                Text(
                  'Check my knowledge',
                  style: TextStyle(fontSize: 18, color: Colors.white, fontWeight: FontWeight.bold),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  // Widget for Time and Incorrect boxes
  Widget _buildStatBox(IconData icon, String value, String label, {Color iconColor = const Color(0xFF0056D2)}) {
    return Expanded(
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: const Color(0xFFEEF2FF),
          borderRadius: BorderRadius.circular(16),
        ),
        child: Column(
          children: [
            Icon(icon, color: iconColor),
            const SizedBox(height: 8),
            Text(value, style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
            Text(label, style: const TextStyle(color: Colors.grey, fontSize: 13)),
          ],
        ),
      ),
    );
  }

  // Widget for Improvement List items
  Widget _buildImprovementTile(String title, String subtitle) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.grey.shade200),
      ),
      child: ListTile(
        leading: const Icon(Icons.error_outline, color: Color(0xFFC62828)),
        title: Text(title, style: const TextStyle(fontWeight: FontWeight.bold)),
        subtitle: Text(subtitle, maxLines: 1, overflow: TextOverflow.ellipsis),
        trailing: const Icon(Icons.keyboard_arrow_down, color: Colors.grey),
        onTap: () {}, // Can be used to show more details
      ),
    );
  }
}
