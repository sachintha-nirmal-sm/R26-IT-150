import "package:flutter/material.dart";
import 'features/LessonList/lesson_list_page.dart';
import 'features/auth/presentation/get_started_page.dart';
import 'features/auth/presentation/sign_up.dart';
import 'package:flutter/material.dart';
import 'package:mobile_app/features/dashboard/physics_lab_home_page.dart';
import 'package:mobile_app/features/lessons/force_linear_motion_page.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: "Mobile App",
      theme: ThemeData(
        useMaterial3: true,
        primarySwatch: Colors.blue,
      ),
      home: const GetStartedPage(),
      routes: {
        '/sign-up': (context) => const SignupScreen(),
        '/lessons': (context) => const PhysicsLessonsScreen(),
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
      initialRoute: '/',
      routes: {
        '/': (context) => const PhysicsLabHomePage(),
        '/force-motion': (context) => const ForceLinearMotionPage(),
      },
    );
  }
}
