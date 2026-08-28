import 'package:flutter/material.dart';

import '../../data/practical.dart';
import 'unity_player_screen.dart';

/// Trial / demo — Unity runs inside this Flutter app, not a separate APK.
class PracticeExperienceScreen extends StatelessWidget {
  const PracticeExperienceScreen({super.key, required this.args});

  final PracticalRunArgs args;

  @override
  Widget build(BuildContext context) {
    return UnityPlayerScreen(args: args);
  }
}
