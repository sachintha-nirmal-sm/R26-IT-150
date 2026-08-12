import 'package:flutter/material.dart';
import 'package:cloud_firestore/cloud_firestore.dart';

class AdminAnalyticsScreen extends StatelessWidget {
  const AdminAnalyticsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Analytics', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold)),
          const SizedBox(height: 4),
          Text('Student performance overview across all grades.',
              style: TextStyle(color: Colors.grey.shade600, fontSize: 13)),
          const SizedBox(height: 20),
          _buildGradeBreakdown(),
          const SizedBox(height: 16),
          _buildWeakTopicsCard(),
          const SizedBox(height: 16),
          _buildRecentAttemptsCard(),
        ],
      ),
    );
  }

  Widget _buildGradeBreakdown() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 8)],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Students by Grade', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
          const Divider(height: 20),
          ...['Grade 9', 'Grade 10', 'Grade 11'].map((grade) =>
              FutureBuilder<AggregateQuerySnapshot>(
                future: FirebaseFirestore.instance
                    .collection('users')
                    .where('role', isEqualTo: 'student')
                    .where('grade', isEqualTo: grade)
                    .count().get(),
                builder: (context, snap) {
                  final count = snap.data?.count ?? 0;
                  final color = grade == 'Grade 9' ? Colors.blue
                      : grade == 'Grade 10' ? Colors.green : Colors.orange;
                  return Padding(
                    padding: const EdgeInsets.symmetric(vertical: 8),
                    child: Row(
                      children: [
                        SizedBox(width: 80, child: Text(grade, style: const TextStyle(fontSize: 13))),
                        Expanded(
                          child: ClipRRect(
                            borderRadius: BorderRadius.circular(4),
                            child: LinearProgressIndicator(
                              value: count == 0 ? 0 : (count / 50).clamp(0.0, 1.0),
                              minHeight: 10,
                              backgroundColor: Colors.grey.shade200,
                              valueColor: AlwaysStoppedAnimation<Color>(color),
                            ),
                          ),
                        ),
                        const SizedBox(width: 10),
                        SizedBox(
                          width: 30,
                          child: Text('$count',
                              style: const TextStyle(fontWeight: FontWeight.bold),
                              textAlign: TextAlign.right),
                        ),
                      ],
                    ),
                  );
                },
              ),
          ),
        ],
      ),
    );
  }

  Widget _buildWeakTopicsCard() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 8)],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Common Weak Topics', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
          const Divider(height: 20),
          StreamBuilder<QuerySnapshot>(
            stream: FirebaseFirestore.instance
                .collectionGroup('weakTopics')
                .limit(50).snapshots(),
            builder: (context, snap) {
              if (!snap.hasData) return const Center(child: CircularProgressIndicator());
              if (snap.data!.docs.isEmpty) return const Text('No weak topic data yet.');

              final Map<String, int> tagCount = {};
              for (final doc in snap.data!.docs) {
                final data = doc.data() as Map<String, dynamic>;
                final tag = data['lessonTag'] ?? 'Unknown';
                tagCount[tag] = (tagCount[tag] ?? 0) + 1;
              }
              final sorted = tagCount.entries.toList()
                ..sort((a, b) => b.value.compareTo(a.value));

              return Column(
                children: sorted.take(5).map((entry) => Padding(
                  padding: const EdgeInsets.symmetric(vertical: 6),
                  child: Row(
                    children: [
                      const Icon(Icons.warning_amber, color: Colors.orange, size: 18),
                      const SizedBox(width: 8),
                      Expanded(child: Text(entry.key, style: const TextStyle(fontSize: 14))),
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 3),
                        decoration: BoxDecoration(
                          color: Colors.red.withOpacity(0.1),
                          borderRadius: BorderRadius.circular(20),
                        ),
                        child: Text('${entry.value} students',
                            style: const TextStyle(color: Colors.red, fontSize: 12)),
                      ),
                    ],
                  ),
                )).toList(),
              );
            },
          ),
        ],
      ),
    );
  }

  Widget _buildRecentAttemptsCard() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 8)],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Recent Quiz Attempts', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
          const Divider(height: 20),
          StreamBuilder<QuerySnapshot>(
            stream: FirebaseFirestore.instance
                .collectionGroup('quizAttempts')
                .limit(10).snapshots(),
            builder: (context, snap) {
              if (!snap.hasData) return const Center(child: CircularProgressIndicator());
              if (snap.data!.docs.isEmpty) return const Text('No quiz attempts yet.');

              return Column(
                children: snap.data!.docs.map((doc) {
                  final data = doc.data() as Map<String, dynamic>;
                  final score = data['score'] ?? 0;
                  final passed = score >= 70;
                  return Padding(
                    padding: const EdgeInsets.only(bottom: 8),
                    child: Row(children: [
                      Container(
                        width: 48, height: 48,
                        decoration: BoxDecoration(
                          color: (passed ? Colors.green : Colors.red).withOpacity(0.1),
                          borderRadius: BorderRadius.circular(10),
                        ),
                        child: Center(
                          child: Text('$score%',
                              style: TextStyle(
                                  color: passed ? Colors.green : Colors.red,
                                  fontWeight: FontWeight.bold,
                                  fontSize: 13)),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                          Text('Quiz: ${data['quizId'] ?? '-'}',
                              style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                          Text(passed ? 'Pass' : 'Fail',
                              style: TextStyle(
                                  color: passed ? Colors.green : Colors.red, fontSize: 12)),
                        ]),
                      ),
                    ]),
                  );
                }).toList(),
              );
            },
          ),
        ],
      ),
    );
  }
}
