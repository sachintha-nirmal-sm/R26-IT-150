import 'package:flutter/material.dart';

import 'features/LessonList/lesson_list_page.dart';
import 'features/auth/presentation/get_started_page.dart';
import 'features/auth/presentation/login_page.dart';
import 'features/auth/presentation/sign_up.dart';
import 'features/dashboard/physics_lab_home_page.dart';
import 'features/lessons/force_linear_motion_page.dart';
import 'features/lessons/lessons_dashboard.dart';

void main() {
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
      home: const GetStartedPage(),
      initialRoute: '/get-started',
      routes: {
        '/get-started': (context) => const GetStartedPage(),
        '/login': (context) => const LoginPage(),
        '/home': (context) => const PhysicsLabHomePage(),
        '/sign-up': (context) => const SignupScreen(),
        '/lesson-list': (context) => const PhysicsLessonsScreen(),
        '/force-motion': (context) => const ForceLinearMotionPage(),
        '/lessonDBoard': (context) => const LessonsDashboard(),
      },
    );
  }
}
