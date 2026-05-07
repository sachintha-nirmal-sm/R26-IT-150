import "package:flutter/material.dart";
import 'features/experiments/experiments.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: "Physics Lab - Experiment Results",
      theme: ThemeData(
        useMaterial3: true,
        fontFamily: 'Poppins',
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color(0xFF2F80ED),
        ),
      ),
      home: const ExperimentResultsScreen(),
    );
  }
}
