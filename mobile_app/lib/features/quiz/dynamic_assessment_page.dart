import 'package:flutter/material.dart';
import 'analysis_results_page.dart';

class DynamicAssessmentPage extends StatefulWidget {
  const DynamicAssessmentPage({super.key});

  @override
  State<DynamicAssessmentPage> createState() => _DynamicAssessmentPageState();
}

class _DynamicAssessmentPageState extends State<DynamicAssessmentPage> {
  // Track answers: 0 = Force Definition, 1 = Effects of Force, 2 = Calculation
  List<int?> userAnswers = List.filled(3, null);
  final List<int> correctAnswers = [1, 2, 1]; // B, C, B
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
            padding: const EdgeInsets.only(right: 16.0),
            child: GestureDetector(
              onTap: () => Navigator.pushNamed(context, '/profile'),
              child: const CircleAvatar(
                radius: 18,
                backgroundColor: const Color(0xFFCCCCCC),
                child: Icon(Icons.person, color: Colors.white, size: 22),
              ),
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
                // 1. Function / Definition Question
                _buildQuestionCard(
                  0,
                  "Topic: Force - Definition",
                  "What is force?",
                  ["A type of energy", "A push or a pull", "A form of speed", "A type of mass"],
                  showInteractive: true,
                ),

                // 2. Content Related Question
                _buildQuestionCard(
                  1,
                  "Topic: Force - Effects",
                  "Which of the following is NOT an effect of force?",
                  ["Changing the shape of an object", "Stopping a moving object", "Increasing the mass of\n an object", "Changing the direction of motion"],
                ),

                // 3. Calculation Related Question
                _buildQuestionCard(
                  2,
                  "Topic: Force - Calculation",
                  "A force of 10 N is applied to a box and another force of 5 N is applied to another box. How many times is the first force greater than the second?",
                  ["1 time", "2 times", "5 times", "15 times"],
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
    );
  }

  // Analysis Logic to identify weak areas
  void _analyzePerformance() {
    // Q1 and Q2 are conceptual questions (Definition + Effects)
    bool q1Correct = userAnswers[0] == correctAnswers[0]; // Definition
    bool q2Correct = userAnswers[1] == correctAnswers[1]; // Effects
    bool conceptualCorrect = q1Correct && q2Correct;
    
    // Q3 is a calculation question
    bool calculationCorrect = userAnswers[2] == correctAnswers[2]; // Calculation

    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => AnalysisResultsPage(
          calculationCorrect: calculationCorrect,
          scenarioCorrect: q2Correct,
          conceptualCorrect: conceptualCorrect,
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
          image: AssetImage('assets/images/dynamic quize.png'),
          fit: BoxFit.cover,
        ),
      ),
    );
  }
}
