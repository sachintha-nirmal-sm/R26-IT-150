import 'package:flutter/material.dart';

class DynamicAssessmentPage extends StatefulWidget {
  const DynamicAssessmentPage({super.key});

  @override
  State<DynamicAssessmentPage> createState() => _DynamicAssessmentPageState();
}

class _DynamicAssessmentPageState extends State<DynamicAssessmentPage> {
  // Track answers: 0 = Formula, 1 = Scenario, 2 = Conceptual
  List<int?> userAnswers = List.filled(3, null);
  final List<int> correctAnswers = [2, 1, 0]; // Example answer key
  int currentQuestion = 0;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FE),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Color(0xFF2196F3)),
          onPressed: () => Navigator.pop(context),
        ),
        title: const Text('Physics Lab', style: TextStyle(color: Colors.black, fontWeight: FontWeight.bold)),
        actions: [
          Padding(
            padding: const EdgeInsets.all(8.0),
            child: CircleAvatar(
              backgroundColor: const Color(0xFF2196F3),
              child: const Text("SP", style: TextStyle(color: Colors.white, fontSize: 12)),
            ),
          )
        ],
      ),
      body: Column(
        children: [
          // Header Section
          _buildDottedHeader(),
          
          Expanded(
            child: ListView(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              children: [
                // 1. Formula Based Question
                _buildQuestionCard(
                  0,
                  "Topic: Newton's Second Law",
                  "A force of 10N is applied to a 2kg mass. What is the acceleration? (a = F/m)",
                  ["2 m/s²", "5 m/s²", "20 m/s²", "10 m/s²"],
                  showInteractive: true,
                ),

                // 2. Scenario Based Question
                _buildQuestionCard(
                  1,
                  "Topic: Friction",
                  "If a block is sliding on a rough surface and the pushing force stops, what happens to the block's motion?",
                  ["Accelerates", "Stops immediately", "Slows down gradually", "Constant speed"],
                ),

                // 3. Conceptual Question
                _buildQuestionCard(
                  2,
                  "Topic: Inertia",
                  "Which object has the most inertia?",
                  ["A 10kg rock at rest", "A 2kg ball rolling fast", "A 5kg feather", "Inertia depends only on speed"],
                ),

                const SizedBox(height: 20),
                ElevatedButton(
                  onPressed: _analyzePerformance,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: const Color(0xFF2196F3),
                    minimumSize: const Size(double.infinity, 55),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                  ),
                  child: const Text("Submit", style: TextStyle(color: Colors.white, fontSize: 18)),
                ),
                const SizedBox(height: 40),
              ],
            ),
          ),
        ],
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: 1,
        selectedItemColor: Colors.blue,
        unselectedItemColor: Colors.grey,
        type: BottomNavigationBarType.fixed,
        items: const [
          BottomNavigationBarItem(icon: Icon(Icons.home_outlined), label: 'Home'),
          BottomNavigationBarItem(icon: Icon(Icons.menu_book), label: 'Lessons'),
          BottomNavigationBarItem(icon: Icon(Icons.biotech_outlined), label: 'Labs'),
          BottomNavigationBarItem(icon: Icon(Icons.person_outline), label: 'Profile'),
        ],
      ),
    );
  }

  // Analysis Logic to identify weak areas
  void _analyzePerformance() {
    List<String> weakAreas = [];
    if (userAnswers[0] != correctAnswers[0]) weakAreas.add("Mathematical Application (F=ma)");
    if (userAnswers[1] != correctAnswers[1]) weakAreas.add("Frictional Forces & Real-world motion");
    if (userAnswers[2] != correctAnswers[2]) weakAreas.add("Conceptual understanding of Inertia/Mass");

    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(25))),
      builder: (context) => Padding(
        padding: const EdgeInsets.all(25.0),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text("Knowledge Analysis", style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold)),
            const SizedBox(height: 15),
            if (weakAreas.isEmpty)
              const Text("Excellent! You have a strong grasp of basics.")
            else ...[
              const Text("You should focus on these areas:"),
              const SizedBox(height: 10),
              ...weakAreas.map((area) => ListTile(
                leading: const Icon(Icons.warning_amber_rounded, color: Colors.orange),
                title: Text(area),
              )),
            ],
            const SizedBox(height: 20),
            ElevatedButton(
              onPressed: () => Navigator.pop(context),
              child: const Text("Got it"),
            )
          ],
        ),
      ),
    );
  }

  Widget _buildDottedHeader() {
    return Container(
      margin: const EdgeInsets.all(20),
      padding: const EdgeInsets.all(15),
      decoration: BoxDecoration(
        border: Border.all(color: Colors.blue.withOpacity(0.5), style: BorderStyle.none),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          const Text("Dynamics Assessment", style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold)),
          ],
      ),
    );
  }

  Widget _buildQuestionCard(int index, String topic, String question, List<String> options, {bool showInteractive = false}) {
    return Column(
      children: [
        Container(
          padding: const EdgeInsets.all(15),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(12),
            border: Border.all(color: Colors.blue.shade100),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Icon(Icons.science_outlined, color: Colors.blue.shade300, size: 20),
                  const SizedBox(width: 8),
                  Text(topic, style: const TextStyle(color: Colors.grey, fontWeight: FontWeight.w600)),
                ],
              ),
              const SizedBox(height: 15),
              Text(question, style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
              if (showInteractive) ...[
                const SizedBox(height: 15),
                _buildInteractivePlaceholder(),
              ]
            ],
          ),
        ),
        const SizedBox(height: 15),
        ...List.generate(options.length, (optIndex) {
          bool isSelected = userAnswers[index] == optIndex;
          return GestureDetector(
            onTap: () => setState(() => userAnswers[index] = optIndex),
            child: Container(
              margin: const EdgeInsets.only(bottom: 10),
              padding: const EdgeInsets.symmetric(horizontal: 15, vertical: 12),
              decoration: BoxDecoration(
                color: isSelected ? const Color(0xFFE8F1FF) : Colors.white,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: isSelected ? const Color(0xFF2196F3) : Colors.grey.shade200, width: isSelected ? 2 : 1),
              ),
              child: Row(
                children: [
                  CircleAvatar(
                    radius: 14,
                    backgroundColor: isSelected ? const Color(0xFF2196F3) : Colors.grey.shade100,
                    child: Text(String.fromCharCode(65 + optIndex), style: TextStyle(color: isSelected ? Colors.white : Colors.black, fontSize: 12)),
                  ),
                  const SizedBox(width: 15),
                  Text(options[optIndex], style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w500)),
                  const Spacer(),
                  if (isSelected) const Icon(Icons.check_circle, color: Color(0xFF2196F3), size: 20),
                ],
              ),
            ),
          );
        }),
        const SizedBox(height: 30),
      ],
    );
  }

  Widget _buildInteractivePlaceholder() {
    return Container(
      height: 180,
      width: double.infinity,
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(12),
        image: const DecorationImage(
          image: NetworkImage('https://via.placeholder.com/400x200'),
          fit: BoxFit.cover,
        ),
      ),
      child: Center(
        child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 10),
          decoration: BoxDecoration(color: Colors.white.withOpacity(0.9), borderRadius: BorderRadius.circular(25)),
          child: const Text("Interactive Diagram", style: TextStyle(color: Color(0xFF2196F3), fontWeight: FontWeight.bold)),
        ),
      ),
    );
  }
}
