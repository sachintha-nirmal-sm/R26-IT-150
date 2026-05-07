import "package:flutter/material.dart";
import 'features/LessonList/lesson_list_page.dart';
import 'features/auth/presentation/get_started_page.dart';
import 'features/auth/presentation/sign_up.dart';


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
      },
    );
  }
}
