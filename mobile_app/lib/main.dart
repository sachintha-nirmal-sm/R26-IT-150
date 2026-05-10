import "package:flutter/material.dart";

import "features/Step-by-Step Solution/step_by_step_solution_screen.dart";
import "features/comparison-answer/comparison_final_return_screen.dart";
import "features/image-preview/image_preview_confirmation_screen.dart";
import "features/quiz-complete/quiz_complete_screen.dart";
import "features/scenario-Based Question/scenario_question_screen.dart";
import "features/upload-image/upload_image_screen.dart";

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: "Mobile App",
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color(0xFF1A3CBA),
          brightness: Brightness.light,
        ),
        scaffoldBackgroundColor: const Color(0xFFF4F6FB),
        appBarTheme: const AppBarTheme(
          backgroundColor: Colors.white,
          elevation: 0,
          iconTheme: IconThemeData(color: Color(0xFF1A1A2E)),
        ),
      ),
      home: const ScenarioQuestionScreen(),
      initialRoute: "/scenario-question",
      routes: {
        "/scenario-question": (context) => const ScenarioQuestionScreen(),
        "/quiz-complete": (context) => const QuizCompleteScreen(),
        "/step-by-step": (context) => const StepByStepSolutionScreen(),
        "/upload-image": (context) => const UploadImageScreen(),
        "/image-preview": (context) => const ImagePreviewConfirmationScreen(),
        "/comparison-answer": (context) => const ComparisonFinalReturnScreen(),
      },
    );
  }
}
