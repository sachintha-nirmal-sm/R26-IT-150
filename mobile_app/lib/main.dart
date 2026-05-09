import 'package:flutter/material.dart';
import 'features/experiments/experiments.dart';

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
      ),

      // ── Initial screen (change when auth is integrated) ──────────────────
      home: const ExperimentResultsScreen(),
      initialRoute: '/experiment-execution',



      // ── Named Routes ────────────────────────────────────────────────────────
      routes: {
        '/experiment-results':    (context) => const ExperimentResultsScreen(),
        '/experiment-execution':  (context) => const ExperimentExecutionScreen(),
        '/experiment-in-progress':(context) => const ExperimentInProgressScreen(),
        '/practice-experience':   (context) => const PracticeExperienceScreen(),

      },
    );
  }
}
