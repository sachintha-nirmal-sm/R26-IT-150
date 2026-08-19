import 'package:flutter/material.dart';

class AnalysisResultsPage extends StatelessWidget {
  final bool formulaCorrect;
  final bool scenarioCorrect;
  final bool conceptualCorrect;

  const AnalysisResultsPage({
    super.key,
    required this.formulaCorrect,
    required this.scenarioCorrect,
    required this.conceptualCorrect,
  });

  @override
  Widget build(BuildContext context) {
    // Logic for dynamic feedback
    String mainWeakness = !formulaCorrect 
        ? "Calculation & Formula Application" 
        : (!scenarioCorrect ? "Scenario-based Reasoning" : "None! Great Job");

    String feedbackQuote = formulaCorrect 
        ? "You have a strong grasp of formulas. Keep applying them to complex scenarios."
        : "You understand the concepts well, but tend to make errors when applying Newton's Second Law in multi-step problems.";

    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FE),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Color(0xFF2196F3)),
          onPressed: () => Navigator.pop(context),
        ),
        title: const Text('Analysis Results', style: TextStyle(color: Colors.black, fontWeight: FontWeight.bold)),
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
          )
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20.0),
        child: Column(
          children: [
            // Analysis Main Card
            Container(
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(20),
                border: Border.all(color: Colors.grey.shade200),
              ),
              child: Column(
                children: [
                  const CircleAvatar(
                    backgroundColor: Color(0xFFFFEBEB),
                    child: Icon(Icons.warning_rounded, color: Colors.red),
                  ),
                  const SizedBox(height: 15),
                  const Text("Analysis Complete!", style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold)),
                  const SizedBox(height: 15),
                  Text.rich(
                    TextSpan(
                      text: "You're doing great, but we noticed a weakness in ",
                      style: const TextStyle(color: Colors.grey, fontSize: 16),
                      children: [
                        TextSpan(text: mainWeakness, style: const TextStyle(color: Colors.black, fontWeight: FontWeight.bold)),
                        const TextSpan(text: "."),
                      ],
                    ),
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: 20),
                  // Quote Box
                  Container(
                    padding: const EdgeInsets.all(15),
                    decoration: BoxDecoration(
                      color: const Color(0xFFF0F4FF),
                      borderRadius: BorderRadius.circular(12),
                      border: const Border(left: BorderSide(color: Colors.blue, width: 4)),
                    ),
                    child: Text(
                      '"$feedbackQuote"',
                      style: const TextStyle(color: Color(0xFF444444), fontSize: 15, height: 1.4),
                    ),
                  ),
                ],
              ),
            ),

            const SizedBox(height: 20),

            // Dynamic Status Row
            Row(
              children: [
                _buildStatusCard(
                  Icons.bolt, 
                  "Conceptual Strength", 
                  conceptualCorrect ? "Good" : "Weak", 
                  conceptualCorrect ? Colors.blue : Colors.red
                ),
                const SizedBox(width: 15),
                _buildStatusCard(
                  Icons.calculate, 
                  "Calculation Accuracy", 
                  formulaCorrect ? "Good" : "Weak", 
                  formulaCorrect ? Colors.blue : Colors.red
                ),
              ],
            ),

            const SizedBox(height: 30),

            // Action Button
            SizedBox(
              width: double.infinity,
              height: 60,
              child: ElevatedButton(
                onPressed: () {
                  Navigator.pushNamed(context, '/deep-learn');
                },
                style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xFF2196F3),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(15)),
                ),
                child: const Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(Icons.school, color: Colors.white),
                    SizedBox(width: 10),
                    Text("Start Deep Learning", style: TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.bold)),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: 1,
        selectedItemColor: Colors.blue,
        unselectedItemColor: Colors.grey,
        type: BottomNavigationBarType.fixed,
        onTap: (index) {
          if (index == 3) {
            Navigator.pushNamed(context, '/profile');
          }
        },
        items: const [
          BottomNavigationBarItem(icon: Icon(Icons.home_outlined), label: 'Home'),
          BottomNavigationBarItem(icon: Icon(Icons.book_outlined), label: 'Lessons'),
          BottomNavigationBarItem(icon: Icon(Icons.science_outlined), label: 'Labs'),
          BottomNavigationBarItem(icon: Icon(Icons.person_outline), label: 'Profile'),
        ],
      ),
    );
  }

  Widget _buildStatusCard(IconData icon, String title, String status, Color color) {
    return Expanded(
      child: Container(
        padding: const EdgeInsets.all(15),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(15),
          border: Border.all(color: Colors.grey.shade200),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Icon(icon, color: color),
            const SizedBox(height: 10),
            Text(title, style: const TextStyle(color: Colors.grey, fontSize: 13, fontWeight: FontWeight.w600)),
            const SizedBox(height: 5),
            Text(
              status, 
              style: TextStyle(color: color, fontSize: 24, fontWeight: FontWeight.bold)
            ),
          ],
        ),
      ),
    );
  }
}
