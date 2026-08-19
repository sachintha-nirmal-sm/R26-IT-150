import 'package:flutter/material.dart';
import 'dart:async';
import 'experiment_in_progress_screen.dart';

class PracticeExperienceScreen extends StatefulWidget {
  const PracticeExperienceScreen({super.key});

  @override
  State<PracticeExperienceScreen> createState() =>
      _PracticeExperienceScreenState();
}

class _PracticeExperienceScreenState extends State<PracticeExperienceScreen> {
  late Timer _sessionTimer;
  late Timer _practiceTimer;
  int _sessionSeconds = 45;
  int _practiceSeconds = 0;
  int _practiceMilliseconds = 0;
  int _attemptsRemaining = 2;
  bool _practiceTimerStarted = false;

  final List<Map<String, String>> _parameters = [
    {'label': 'Mass', 'value': '2kg', 'icon': '⚖️'},
    {'label': 'Initial V', 'value': '0m/s', 'icon': '→'},
    {'label': 'Distance', 'value': '5m', 'icon': '📏'},
  ];

  @override
  void initState() {
    super.initState();
    _startSessionTimer();
  }

  void _startSessionTimer() {
    _sessionTimer = Timer.periodic(const Duration(seconds: 1), (timer) {
      setState(() {
        _sessionSeconds++;
      });
    });
  }

  void _startPracticeTimer() {
    if (!_practiceTimerStarted) {
      _practiceTimerStarted = true;
      _practiceTimer = Timer.periodic(const Duration(milliseconds: 10), (timer) {
        setState(() {
          _practiceMilliseconds += 10;
          if (_practiceMilliseconds >= 1000) {
            _practiceSeconds += 1;
            _practiceMilliseconds = 0;
          }
        });
      });
    }
  }

  void _resetPracticeTimer() {
    setState(() {
      _practiceSeconds = 0;
      _practiceMilliseconds = 0;
    });
    if (_practiceTimerStarted) {
      _practiceTimer.cancel();
      _practiceTimerStarted = false;
    }
  }

  void _finishPractice() {
    if (_attemptsRemaining > 0) {
      setState(() {
        _attemptsRemaining--;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content:
              Text('Attempt completed! $_attemptsRemaining attempts remaining'),
          duration: const Duration(seconds: 2),
        ),
      );
    } else {
      _showCompletionDialog();
    }
  }

  void _showCompletionDialog() {
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (BuildContext context) {
        return AlertDialog(
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
          ),
          title: const Text(
            'Practice Complete',
            style: TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.w700,
              color: Color(0xFF2F80ED),
              fontFamily: 'Poppins',
            ),
          ),
          content: const Text(
            'Now you can Start Experiment',
            style: TextStyle(
              fontSize: 14,
              color: Colors.black87,
              height: 1.5,
              fontFamily: 'Poppins',
            ),
          ),
          actions: [
            ElevatedButton(
              onPressed: () {
                Navigator.pop(context);
                // Navigate to Experiment in Progress page
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) =>
                        const ExperimentInProgressScreen(showTimer: true),
                  ),
                );
              },
              style: ElevatedButton.styleFrom(
                padding: const EdgeInsets.symmetric(
                  horizontal: 24,
                  vertical: 12,
                ),
                backgroundColor: const Color(0xFF2F80ED),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(8),
                ),
              ),
              child: const Text(
                'OK',
                style: TextStyle(
                  fontSize: 14,
                  fontWeight: FontWeight.w600,
                  color: Colors.white,
                  fontFamily: 'Poppins',
                ),
              ),
            ),
          ],
        );
      },
    );
  }

  String _getSessionTime() {
    int minutes = _sessionSeconds ~/ 60;
    int seconds = _sessionSeconds % 60;
    return '${minutes.toString().padLeft(2, '0')}:${seconds.toString().padLeft(2, '0')}';
  }

  String _getPracticeTime() {
    int minutes = _practiceSeconds ~/ 60;
    int seconds = _practiceSeconds % 60;
    int centiseconds = _practiceMilliseconds ~/ 10;

    return '${minutes.toString().padLeft(2, '0')}:${seconds.toString().padLeft(2, '0')}.${centiseconds.toString().padLeft(2, '0')}';
  }

  @override
  void dispose() {
    _sessionTimer.cancel();
    if (_practiceTimerStarted) {
      _practiceTimer.cancel();
    }
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      appBar: _buildAppBar(),
      body: SingleChildScrollView(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Practice Status Card
              _buildPracticeStatusCard(),
              const SizedBox(height: 24),

              // Simulation Preview Window
              _buildSimulationPreview(),
              const SizedBox(height: 20),

              // Parameter Summary
              _buildParameterSummary(),
              const SizedBox(height: 24),

              // Current Session Timer
              _buildCurrentSessionTimer(),
              const SizedBox(height: 32),

              // Bottom Action Row
              _buildActionButtons(),
              const SizedBox(height: 32),
            ],
          ),
        ),
      ),
    );
  }

  PreferredSizeWidget _buildAppBar() {
    return AppBar(
      elevation: 0,
      backgroundColor: Colors.white,
      leading: IconButton(
        icon: const Icon(Icons.arrow_back, color: Color(0xFF2F80ED), size: 28),
        onPressed: () => Navigator.pop(context),
      ),
      title: const Text(
        'Level 04: Kinematics',
        style: TextStyle(
          color: Color(0xFF2F80ED),
          fontSize: 18,
          fontWeight: FontWeight.w700,
          fontFamily: 'Poppins',
        ),
      ),
      centerTitle: true,
      actions: [
        Padding(
          padding: const EdgeInsets.all(16),
          child: Center(
            child: Text(
              _getSessionTime(),
              style: const TextStyle(
                color: Color(0xFF2F80ED),
                fontSize: 14,
                fontWeight: FontWeight.w600,
                fontFamily: 'Courier New',
              ),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildPracticeStatusCard() {
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
      ),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Row(
          children: [
            const Icon(
              Icons.history,
              color: Color(0xFF2F80ED),
              size: 24,
            ),
            const SizedBox(width: 12),
            const Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Practice Attempts',
                    style: TextStyle(
                      fontSize: 14,
                      fontWeight: FontWeight.w600,
                      color: Colors.black87,
                      fontFamily: 'Poppins',
                    ),
                  ),
                  SizedBox(height: 4),
                  Text(
                    'Your current progress state',
                    style: TextStyle(
                      fontSize: 12,
                      color: Color(0xFF999999),
                      fontFamily: 'Poppins',
                    ),
                  ),
                ],
              ),
            ),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
              decoration: BoxDecoration(
                color: const Color(0xFF2F80ED),
                borderRadius: BorderRadius.circular(20),
              ),
              child: Text(
                '$_attemptsRemaining Attempts Remaining',
                style: const TextStyle(
                  fontSize: 12,
                  fontWeight: FontWeight.w600,
                  color: Colors.white,
                  fontFamily: 'Poppins',
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSimulationPreview() {
    return Container(
      height: 200,
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(16),
        color: const Color(0xFF2C3E50),
        image: const DecorationImage(
          image: AssetImage('assets/images/practice_simulation.png'),
          fit: BoxFit.cover,
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.1),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Container(
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(16),
          color: Colors.black.withValues(alpha: 0.4),
        ),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              width: 64,
              height: 64,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                color: Colors.white.withValues(alpha: 0.9),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withValues(alpha: 0.2),
                    blurRadius: 12,
                    offset: const Offset(0, 4),
                  ),
                ],
              ),
              child: const Icon(
                Icons.play_arrow,
                color: Color(0xFF2F80ED),
                size: 32,
              ),
            ),
            const SizedBox(height: 12),
            Text(
              'Practice simulation will appear here',
              style: TextStyle(
                fontSize: 13,
                color: Colors.white.withValues(alpha: 0.8),
                fontFamily: 'Poppins',
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildParameterSummary() {
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
      ),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: List.generate(
            _parameters.length,
            (index) => Padding(
              padding: EdgeInsets.only(
                bottom: index < _parameters.length - 1 ? 12 : 0,
              ),
              child: Row(
                children: [
                  Text(
                    _parameters[index]['icon']!,
                    style: const TextStyle(fontSize: 20),
                  ),
                  const SizedBox(width: 12),
                  Text(
                    _parameters[index]['label']!,
                    style: const TextStyle(
                      fontSize: 13,
                      color: Color(0xFF666666),
                      fontFamily: 'Poppins',
                    ),
                  ),
                  const Spacer(),
                  Text(
                    _parameters[index]['value']!,
                    style: const TextStyle(
                      fontSize: 14,
                      fontWeight: FontWeight.w600,
                      color: Colors.black87,
                      fontFamily: 'Poppins',
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildCurrentSessionTimer() {
    return Center(
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
        decoration: BoxDecoration(
          color: const Color(0xFFE3F2FD),
          borderRadius: BorderRadius.circular(24),
          boxShadow: [
            BoxShadow(
              color: const Color(0xFF2F80ED).withValues(alpha: 0.1),
              blurRadius: 8,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Column(
          children: [
            const Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(
                  Icons.schedule,
                  color: Color(0xFF2F80ED),
                  size: 18,
                ),
                SizedBox(width: 8),
                Text(
                  'CURRENT SESSION',
                  style: TextStyle(
                    fontSize: 10,
                    fontWeight: FontWeight.w600,
                    color: Color(0xFF2F80ED),
                    letterSpacing: 0.5,
                    fontFamily: 'Poppins',
                  ),
                ),
              ],
            ),
            const SizedBox(height: 8),
            Text(
              _getPracticeTime(),
              style: const TextStyle(
                fontSize: 32,
                fontWeight: FontWeight.w700,
                color: Color(0xFF2F80ED),
                fontFamily: 'Courier New',
                letterSpacing: 1,
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildActionButtons() {
    return Column(
      children: [
        // Row 1: Start Practice and Reset
        Row(
          children: [
            Expanded(
              flex: 2,
              child: ElevatedButton.icon(
                onPressed: _startPracticeTimer,
                icon: const Icon(Icons.play_arrow, size: 20),
                label: const Text('Start Practice'),
                style: ElevatedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 12),
                  backgroundColor: const Color(0xFF1F3A7D),
                  foregroundColor: Colors.white,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
              ),
            ),
            const SizedBox(width: 12),
            SizedBox(
              width: 48,
              height: 48,
              child: ElevatedButton(
                onPressed: _resetPracticeTimer,
                style: ElevatedButton.styleFrom(
                  padding: EdgeInsets.zero,
                  backgroundColor: const Color(0xFFE0E5EC),
                  foregroundColor: Colors.black87,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
                child: const Icon(Icons.refresh, size: 20),
              ),
            ),
          ],
        ),
        const SizedBox(height: 12),

        // Row 2: Finish Practice
        SizedBox(
          width: double.infinity,
          child: OutlinedButton(
            onPressed: _finishPractice,
            style: OutlinedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 12),
              side: const BorderSide(
                color: Color(0xFF2F80ED),
                width: 1.5,
              ),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(8),
              ),
            ),
            child: const Text(
              'Finish Practice',
              style: TextStyle(
                fontSize: 14,
                fontWeight: FontWeight.w600,
                color: Color(0xFF2F80ED),
                fontFamily: 'Poppins',
              ),
            ),
          ),
        ),
      ],
    );
  }
}
