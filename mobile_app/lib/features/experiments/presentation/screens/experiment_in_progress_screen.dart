import 'package:flutter/material.dart';

import '../../data/practical.dart';
import 'unity_player_screen.dart';

/// Official attempt — Unity runs inside this Flutter app, not a separate APK.
class ExperimentInProgressScreen extends StatelessWidget {
  const ExperimentInProgressScreen({super.key, required this.args});

  final PracticalRunArgs args;

  @override
  Widget build(BuildContext context) {
    return UnityPlayerScreen(args: args);
  }
}