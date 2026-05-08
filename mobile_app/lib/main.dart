import "package:flutter/material.dart";
import "features/scenario-Based Question/scenario_question_screen.dart";

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: "Mobile App",
      home: const ScenarioQuestionScreen(),
    );
  }
}
