import 'package:flutter/material.dart';
import 'package:firebase_core/firebase_core.dart';
import 'firebase_options.dart';
import 'features/experiments/experiments.dart';
import 'features/LessonList/lesson_list_page.dart';
import 'features/auth/presentation/get_started_page.dart';
import 'features/auth/presentation/login_page.dart';
import 'features/auth/presentation/sign_up.dart';
import 'features/chatbot/chatbot_screen.dart';
import 'features/dashboard/physics_lab_home_page.dart';
import 'features/lessons/force_linear_motion_page.dart';
import 'features/quizzes/lesson_quizzes_page.dart';
import 'features/lessons/lessons_dashboard.dart';
import 'features/DeepLearn/deep_learn_screen.dart';
import 'features/pro/profile_screen.dart';
import 'features/Step-by-Step Solution/step_by_step_solution_screen.dart';
import 'features/comparison-answer/comparison_final_return_screen.dart';
import 'features/image-preview/image_preview_confirmation_screen.dart';
import 'features/quiz-complete/quiz_complete_screen.dart';
import 'features/scenario-Based Question/scenario_question_screen.dart';
import 'features/upload-image/upload_image_screen.dart';
import 'features/games/vector_quest/presentation/pages/vector_quest_game_screen.dart';
import 'features/admin/admin_dashboard.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await Firebase.initializeApp(
    options: DefaultFirebaseOptions.currentPlatform,
  );
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Physics Lab',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        useMaterial3: true,
        fontFamily: 'Poppins',
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
        scrollbarTheme: ScrollbarThemeData(
          thickness: WidgetStateProperty.all(3),
          radius: const Radius.circular(10),
          thumbColor: WidgetStateProperty.all(
            const Color(0xFF2196F3).withOpacity(0.45),
          ),
          trackColor: WidgetStateProperty.all(
            const Color(0xFF2196F3).withOpacity(0.08),
          ),
          trackBorderColor: WidgetStateProperty.all(Colors.transparent),
          thumbVisibility: WidgetStateProperty.all(true),
          trackVisibility: WidgetStateProperty.all(true),
          crossAxisMargin: 2,
          mainAxisMargin: 4,
        ),
      ),
      home: const GetStartedPage(),
      initialRoute: "/get-started",
      routes: {
        "/get-started": (context) => const GetStartedPage(),
        "/login": (context) => const LoginPage(),
        "/home": (context) => const PhysicsLabHomePage(),
        "/sign-up": (context) => const SignupScreen(),
        "/lesson-list": (context) => const PhysicsLessonsScreen(),
        "/force-motion": (context) => const ForceLinearMotionPage(),
        "/lesson-quizzes": (context) => const LessonQuizzesPage(),

        "/deep-learn": (context) => const DeepLearningScreen(),
        "/profile": (context) => const ProfileScreen(),
        "/experiment-results": (context) => const ExperimentResultsScreen(),
        "/experiment-execution": (context) => const ExperimentExecutionScreen(),
        "/experiment-in-progress": (context) => const ExperimentInProgressScreen(),
        "/practice-experience": (context) => const PracticeExperienceScreen(),
        "/scenario-question": (context) => const ScenarioQuestionScreen(),
        "/quiz-complete": (context) => const QuizCompleteScreen(
              completionSeconds: 0,
              overtimeSeconds: 0,
            ),
        "/step-by-step": (context) => const StepByStepSolutionScreen(),
        "/upload-image": (context) => const UploadImageScreen(),
        "/image-preview": (context) => const ImagePreviewConfirmationScreen(),
        "/comparison-answer": (context) => const ComparisonFinalReturnScreen(),
        "/chatbot": (context) => const ChatbotScreen(),
        "/game-intro": (context) => const VectorQuestGameScreen(),
        "/admin-dashboard": (context) => const AdminDashboard(),
      },
    );
  }
}
