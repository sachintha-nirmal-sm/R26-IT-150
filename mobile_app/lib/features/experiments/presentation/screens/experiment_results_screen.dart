import 'package:flutter/material.dart';

class ExperimentResultsScreen extends StatelessWidget {
  final int? score; // The score passed from the practical (0-100)
  final String? finalDuration; // e.g., "12 min 30 sec"
  final String? topicName; // e.g., "Force"

  const ExperimentResultsScreen({
    super.key,
    this.score,
    this.finalDuration,
    this.topicName = "Force",
  });

  // Logic for Performance Level based on score
  String _getPerformanceLabel(int score) {
    if (score == 0) return "-";
    if (score < 35) return 'Low';
    if (score < 65) return 'Medium';
    return 'High';
  }

  Color _getPerformanceColor(int score) {
    if (score == 0) return Colors.grey;
    if (score < 35) return const Color(0xFFEB5757);
    return const Color(0xFF27AE60);
  }

  @override
  Widget build(BuildContext context) {
    // Extract arguments if passed via named route
    final args = ModalRoute.of(context)?.settings.arguments as Map<String, dynamic>?;
    final displayScore = score ?? args?['score'] ?? 0;
    final displayDuration = finalDuration ?? args?['finalDuration'] ?? "0 min 0 sec";
    final displayTopic = topicName ?? args?['topicName'] ?? "Force";

    final label = _getPerformanceLabel(displayScore);
    final color = _getPerformanceColor(displayScore);

    return Scaffold(
      backgroundColor: Colors.white,
      appBar: AppBar(
        title: const Text('Experiment Results'),
        centerTitle: true,
        backgroundColor: Colors.white,
        foregroundColor: const Color(0xFF2F80ED),
        elevation: 0,
      ),
      body: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 24.0),
        child: Column(
          children: [
            const Spacer(),
            
            // Final Duration Card
            _buildInfoCard("FINAL DURATION", displayDuration),
            const SizedBox(height: 24),

            // Performance Score Card (Progress Bar)
            Container(
              padding: const EdgeInsets.all(24),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(20),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.05),
                    blurRadius: 15,
                    offset: const Offset(0, 5),
                  )
                ],
              ),
              child: Column(
                children: [
                  const Text(
                    "PERFORMANCE SCORE",
                    style: TextStyle(
                      color: Colors.grey, 
                      fontSize: 12, 
                      fontWeight: FontWeight.bold, 
                      letterSpacing: 1.2
                    ),
                  ),
                  const SizedBox(height: 16),
                  Text(
                    "$displayScore%",
                    style: const TextStyle(
                      fontSize: 48, 
                      fontWeight: FontWeight.bold, 
                      color: Color(0xFF2F80ED)
                    ),
                  ),
                  const SizedBox(height: 20),
                  ClipRRect(
                    borderRadius: BorderRadius.circular(10),
                    child: LinearProgressIndicator(
                      value: displayScore / 100,
                      backgroundColor: const Color(0xFFF0F4F8),
                      color: const Color(0xFF2F80ED),
                      minHeight: 12,
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 24),

            // Performance Level Tile
            _buildPerformanceTile(label, color),

            const Spacer(),

            // Save Result Button
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: () {
                  Navigator.pushNamed(
                    context, 
                    '/profile', 
                    arguments: {
                      'view': 'recent_progress',
                      'topic': displayTopic,
                      'status': 'Completed',
                      'score': displayScore,
                    },
                  );
                },
                style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xFF2F80ED),
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 18),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                  elevation: 5,
                ),
                child: const Text(
                  "Save Result", 
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)
                ),
              ),
            ),
            const SizedBox(height: 40),
          ],
        ),
      ),
    );
  }

  Widget _buildInfoCard(String title, String value) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: const Color(0xFFF5F9FF),
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        children: [
          Text(
            title, 
            style: const TextStyle(
              color: Colors.grey, 
              fontSize: 11, 
              fontWeight: FontWeight.bold, 
              letterSpacing: 1.1
            )
          ),
          const SizedBox(height: 8),
          Text(
            value, 
            style: const TextStyle(
              fontSize: 24, 
              fontWeight: FontWeight.bold, 
              color: Color(0xFF2F80ED)
            )
          ),
        ],
      ),
    );
  }

  Widget _buildPerformanceTile(String label, Color color) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        border: Border.all(color: const Color(0xFFE0E7FF)),
        borderRadius: BorderRadius.circular(16),
      ),
      child: Row(
        children: [
          Icon(Icons.speed, color: color, size: 28),
          const SizedBox(width: 16),
          const Text(
            "Performance", 
            style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600, color: Color(0xFF1E293B))
          ),
          const Spacer(),
          Text(
            label,
            style: TextStyle(color: color, fontSize: 18, fontWeight: FontWeight.bold),
          ),
        ],
      ),
    );
  }
}

