import 'package:flutter/material.dart';
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
import 'features/games/physics_force_game/presentation/physics_game_wrapper.dart';
import 'features/games/pressure_puzzle_game/presentation/pressure_puzzle_screen.dart';
import 'features/games/density_puzzle_game/density_puzzle_screen.dart';
import 'features/games/simple_machines_game/simple_machines_screen.dart';
import 'features/games/nano_shield_game/nano_shield_screen.dart';
import 'features/games/motion_quest_game/motion_quest_screen.dart';
import 'features/games/newton_game/newton_game_screen.dart';
import 'features/games/friction_game/friction_game_screen.dart';
import 'features/games/resultant_force/resultant_force_screen.dart';
import 'features/games/turning_effect/turning_effect_screen.dart';
import 'features/games/equilibrium_forces/equilibrium_forces_screen.dart';
import 'features/games/hydrostatic_pressure_game/hydrostatic_pressure_game_screen.dart';
import 'features/games/work_power_game/work_power_game_screen.dart';
import 'features/games/current_electricity_game/current_electricity_game_screen.dart';
import 'features/games/waves_game/waves_game_screen.dart';
import 'features/games/geometrical_optics_game/geometrical_optics_game_screen.dart';
import 'features/games/heat_game/heat_game_screen.dart';
import 'features/games/power_energy_game/power_energy_game_screen.dart';
import 'features/games/electronics_game/electronics_game_screen.dart';
import 'features/games/electromagnetism_game/electromagnetism_game_screen.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
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
          iconTheme: IconThemeData(
            color: Color(0xFF1A1A2E),
          ),
        ),
      ),
    initialRoute: "/get-started",
    routes: {
    "/get-started": (context) => const GetStartedPage(),
    "/login": (context) => const LoginPage(),
    "/home": (context) => const PhysicsLabHomePage(),
    "/sign-up": (context) => const SignupScreen(),
    "/lesson-list": (context) => const PhysicsLessonsScreen(),
    "/force-motion": (context) => const ForceLinearMotionPage(),
    "/lesson-quizzes": (context) => const LessonQuizzesPage(),
    "/lessonDBoard": (context) => const LessonsDashboard(),
    "/deep-learn": (context) => const DeepLearningScreen(),
    "/profile": (context) => const ProfileScreen(),
    "/experiment-results": (context) =>
      const ExperimentResultsScreen(),
    "/experiment-execution": (context) =>
      const ExperimentExecutionScreen(),
    "/experiment-in-progress": (context) =>
      const ExperimentInProgressScreen(),
    "/practice-experience": (context) =>
      const PracticeExperienceScreen(),
    "/scenario-question": (context) => const ScenarioQuestionScreen(),
    "/quiz-complete": (context) => const QuizCompleteScreen(
      completionSeconds: 0,
      overtimeSeconds: 0,
    ),
    "/step-by-step": (context) => const StepByStepSolutionScreen(),
    "/upload-image": (context) => const UploadImageScreen(),
    "/image-preview": (context) =>
      const ImagePreviewConfirmationScreen(),
    "/comparison-answer": (context) =>
      const ComparisonFinalReturnScreen(),
    "/chatbot": (context) => const ChatbotScreen(),
    "/game-intro": (context) => const VectorQuestGameScreen(),
    "/force-game": (context) => const PhysicsGameWrapper(),
    "/pressure-puzzle": (context) => const PressurePuzzleScreen(),
    "/density-puzzle": (context) => const DensityPuzzleScreen(),
    "/simple-machines-game": (context) => const SimpleMachinesScreen(),
    "/nano-shield": (context) => const NanoShieldScreen(),
    "/motion-quest": (context) => const MotionQuestScreen(),
    "/newton-game": (context) => const NewtonGameScreen(),
    "/friction-game": (context) => const FrictionGameScreen(),
    "/resultant-force": (context) => const ResultantForceScreen(),
    "/turning-effect": (context) => const TurningEffectScreen(),
    "/equilibrium-forces": (context) => const EquilibriumForcesScreen(),
    "/hydrostatic-pressure": (context) => const HydrostaticPressureGameScreen(),
    "/work-power-game": (context) => const WorkPowerGameScreen(),
    "/current-electricity-game": (context) => const CurrentElectricityGameScreen(),
    "/waves-game": (context) => const WavesGameScreen(),
    "/geometrical-optics-game": (context) => const GeometricalOpticsGameScreen(),
    "/heat-game": (context) => const HeatGameScreen(),
    "/power-energy-game": (context) => const PowerEnergyGameScreen(),
    "/electronics-game": (context) => const ElectronicsGameScreen(),
    "/electromagnetism-game": (context) => const ElectromagnetismGameScreen(),
    },
    );
  }
}