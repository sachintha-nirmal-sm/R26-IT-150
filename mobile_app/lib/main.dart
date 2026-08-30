import 'package:firebase_auth/firebase_auth.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:flutter/material.dart';

import 'core/app_navigator.dart';
import 'firebase_options.dart';
import 'features/experiments/experiments.dart';
import 'features/experiments/data/lab_result_sync.dart';
import 'features/experiments/data/practical.dart';
import 'features/experiments/presentation/screens/practical_home_page.dart';

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
import 'features/games/lesson_games_screen.dart';
import 'features/admin/admin_dashboard.dart';
import 'features/dashboard/screens/simple_search_page.dart';
import 'features/games/games_list/games_list_screen.dart';
import 'features/games/nano_shield_game/nano_shield_screen.dart';
import 'features/games/simple_machines_game/simple_machines_screen.dart';
import 'features/games/density_puzzle_game/density_puzzle_screen.dart';
import 'features/games/motion_quest_game/motion_quest_screen.dart';
import 'features/games/newton_game/newton_game_screen.dart';
import 'features/games/friction_game/friction_game_screen.dart';
import 'features/games/resultant_force/resultant_force_screen.dart';
import 'features/games/turning_effect/turning_effect_screen.dart';
import 'features/games/equilibrium_forces/equilibrium_forces_screen.dart';
import 'features/games/hydrostatic_pressure_game/hydrostatic_pressure_game_screen.dart';
import 'features/games/work_power_game/work_power_game_screen.dart';
import 'features/games/power_energy_game/power_energy_game_screen.dart';
import 'features/games/current_electricity_game/current_electricity_game_screen.dart';
import 'features/games/physics_force_game/presentation/physics_game_wrapper.dart';
import 'features/games/pressure_puzzle_game/presentation/pressure_puzzle_screen.dart';
import 'features/games/waves_game/waves_game_screen.dart';
import 'features/games/geometrical_optics_game/geometrical_optics_game_screen.dart';
import 'features/games/heat_game/heat_game_screen.dart';
import 'features/games/electromagnetism_game/electromagnetism_game_screen.dart';
import 'features/games/electronics_game/electronics_game_screen.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  await Firebase.initializeApp(
    options: DefaultFirebaseOptions.currentPlatform,
  );

  runApp(const MyApp());
}

class MyApp extends StatefulWidget {
  const MyApp({super.key});

  @override
  State<MyApp> createState() => _MyAppState();
}

class _MyAppState extends State<MyApp> {
  @override
  void initState() {
    super.initState();
    LabResultSync.start();
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Physics Lab',
      navigatorKey: appNavigatorKey,
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

      // Preserve the authenticated startup flow from develop_main.
      initialRoute: FirebaseAuth.instance.currentUser == null
          ? '/get-started'
          : '/home',

      routes: {
        // Preserve the other branch's "/" navigation entry as an alias.
        "/": (context) => const GetStartedPage(),

        "/get-started": (context) => const GetStartedPage(),
        "/login": (context) => const LoginPage(),
        "/home": (context) => const PhysicsLabHomePage(),
        "/sign-up": (context) => const SignupScreen(),
        "/lesson-list": (context) => const PhysicsLessonsScreen(),
        "/force-motion": (context) => const ForceLinearMotionPage(),
        "/lesson-quizzes": (context) => const LessonQuizzesPage(),
        "/lessonDBoard": (context) => const LessonsDashboard(),

        "/search": (context) {
          final args =
              ModalRoute.of(context)?.settings.arguments as Map<String, dynamic>?;
          final grade = args?['grade'] as String? ?? 'Grade 10';
          return SimpleSearchPage(grade: grade);
        },

        "/deep-learn": (context) => const DeepLearningScreen(),
        "/profile": (context) => const ProfileScreen(),

        "/practical-home": (context) => const PracticalHomePage(),

        "/experiment-results": (context) =>
            const ExperimentResultsScreen(),

        "/experiment-execution": (context) =>
            const ExperimentExecutionScreen(),

        "/experiment-in-progress": (context) {
          final args = ModalRoute.of(context)?.settings.arguments;

          if (args is PracticalRunArgs) {
            return ExperimentInProgressScreen(args: args);
          }

          return const Scaffold(
            body: Center(
              child: Text('Start this practical from Practical Hub.'),
            ),
          );
        },

        "/practice-experience": (context) {
          final args = ModalRoute.of(context)?.settings.arguments;

          if (args is PracticalRunArgs) {
            return PracticeExperienceScreen(args: args);
          }

          return const Scaffold(
            body: Center(
              child: Text('Start the trial from Practical Hub.'),
            ),
          );
        },

        "/scenario-question": (context) =>
            const ScenarioQuestionScreen(),

        "/quiz-complete": (context) => const QuizCompleteScreen(
              completionSeconds: 0,
              overtimeSeconds: 0,
            ),

        "/step-by-step": (context) =>
            const StepByStepSolutionScreen(),

        "/upload-image": (context) => const UploadImageScreen(),

        "/image-preview": (context) =>
            const ImagePreviewConfirmationScreen(),

        "/comparison-answer": (context) =>
            const ComparisonFinalReturnScreen(),

        "/chatbot": (context) => const ChatbotScreen(),

        "/game-intro": (context) =>
            const VectorQuestGameScreen(),

        "/admin-dashboard": (context) =>
            const AdminDashboard(),

        "/games": (context) =>
            const GamesListScreen(),

        // Game routes
        "/nano-shield": (context) =>
            const NanoShieldScreen(),

        "/simple-machines-game": (context) =>
            const SimpleMachinesScreen(),

        "/density-puzzle": (context) =>
            const DensityPuzzleScreen(),

        "/motion-quest": (context) =>
            const MotionQuestScreen(),

        "/newton-game": (context) =>
            const NewtonGameScreen(),

        "/friction-game": (context) =>
            const FrictionGameScreen(),

        "/resultant-force": (context) =>
            const ResultantForceScreen(),

        "/turning-effect": (context) =>
            const TurningEffectScreen(),

        "/equilibrium-forces": (context) =>
            const EquilibriumForcesScreen(),

        "/hydrostatic-pressure": (context) =>
            const HydrostaticPressureGameScreen(),

        "/work-power-game": (context) =>
            const WorkPowerGameScreen(),

        "/power-energy-game": (context) =>
            const PowerEnergyGameScreen(),

        "/current-electricity-game": (context) =>
            const CurrentElectricityGameScreen(),

        "/force-game": (context) =>
            const PhysicsGameWrapper(),

        "/pressure-puzzle": (context) =>
            const PressurePuzzleScreen(),

        "/waves-game": (context) =>
            const WavesGameScreen(),

        "/geometrical-optics-game": (context) =>
            const GeometricalOpticsGameScreen(),

        "/heat-game": (context) =>
            const HeatGameScreen(),

        "/electromagnetism-game": (context) =>
            const ElectromagnetismGameScreen(),

        "/electronics-game": (context) =>
            const ElectronicsGameScreen(),
      },
    );
  }
}
