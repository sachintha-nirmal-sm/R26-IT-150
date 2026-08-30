import "dart:async";

import "package:flutter/material.dart";
import "../quiz-complete/quiz_complete_screen.dart";

class ScenarioQuestionScreen extends StatefulWidget {
  const ScenarioQuestionScreen({super.key});

  @override
  State<ScenarioQuestionScreen> createState() =>
      _ScenarioQuestionScreenState();
}

class _ScenarioQuestionScreenState extends State<ScenarioQuestionScreen> {
  static const int _startSeconds = -60;
  static const int _maxSeconds = 600;
  Timer? _timer;
  int _seconds = _startSeconds;
  bool _navigated = false;

  @override
  void initState() {
    super.initState();
    _startTimer();
  }

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }

  void _startTimer() {
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (!mounted) {
        timer.cancel();
        return;
      }

      setState(() {
        _seconds += 1;
      });

    });
  }

  void _navigateToQuizComplete() {
    if (_navigated || !mounted) {
      return;
    }

    _navigated = true;
    final completionSeconds = _seconds < 0 ? 0 : _seconds;
    final overtimeSeconds = completionSeconds > _maxSeconds
        ? completionSeconds - _maxSeconds
        : 0;
    Navigator.of(context).pushReplacement(
      MaterialPageRoute(
        builder: (context) => QuizCompleteScreen(
          completionSeconds: completionSeconds,
          overtimeSeconds: overtimeSeconds,
        ),
      ),
    );
  }

  String _formatTimer(int totalSeconds) {
    final isNegative = totalSeconds < 0;
    final absSeconds = totalSeconds.abs();
    final minutes = absSeconds ~/ 60;
    final seconds = absSeconds % 60;
    final sign = isNegative ? "-" : "";
    return "$sign${minutes.toString().padLeft(2, "0")}:${seconds.toString().padLeft(2, "0")}";
  }

  @override
  Widget build(BuildContext context) {
    final isReadingTime = _seconds < 0;
    final isOvertime = _seconds >= _maxSeconds;
    final timerColor = isReadingTime || isOvertime ? Colors.red : Colors.blue;

    return Scaffold(
      backgroundColor: const Color(0xFFF5F5F7),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.symmetric(horizontal: 20),
          child: Column(
            children: [
              const _TopBar(),
              const SizedBox(height: 20),
              _TimerRow(
                timeText: _formatTimer(_seconds),
                timeColor: timerColor,
                showReadingLabel: isReadingTime,
              ),
              const SizedBox(height: 30),
              _QuestionCard(onFinish: _navigateToQuizComplete),
              const SizedBox(height: 20),
              const _CalculatorShortcut(),
              const SizedBox(height: 20),
            ],
          ),
        ),
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: 2,
        type: BottomNavigationBarType.fixed,
        onTap: (index) {
          if (index == 3) {
            Navigator.pushNamed(context, "/profile");
          }
        },
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.home_outlined),
            label: "Home",
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.menu_book_outlined),
            label: "Lessons",
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.science),
            label: "Labs",
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.person_outline),
            label: "Profile",
          ),
        ],
      ),
    );
  }
}

class _TopBar extends StatelessWidget {
  const _TopBar();

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        IconButton(
          onPressed: () => Navigator.pop(context),
          icon: const Icon(
            Icons.arrow_back_ios_new,
            color: Colors.blue,
          ),
        ),
        const Expanded(
          child: Center(
            child: Text(
              "Scenario Question",
              style: TextStyle(
                fontSize: 24,
                fontWeight: FontWeight.bold,
                color: Colors.blue,
              ),
            ),
          ),
        ),
        Padding(
          padding: const EdgeInsets.only(right: 16),
          child: GestureDetector(
            onTap: () => Navigator.pushNamed(context, "/profile"),
            child: const CircleAvatar(
              radius: 18,
              backgroundColor: Color(0xFFCCCCCC),
              child: Icon(Icons.person, color: Colors.white, size: 22),
            ),
          ),
        ),
      ],
    );
  }
}

class _TimerRow extends StatelessWidget {
  const _TimerRow({
    required this.timeText,
    required this.timeColor,
    required this.showReadingLabel,
  });

  final String timeText;
  final Color timeColor;
  final bool showReadingLabel;

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.info, color: timeColor),
            const SizedBox(width: 8),
            Text(
              timeText,
              style: TextStyle(
                fontSize: 22,
                color: timeColor,
                fontWeight: FontWeight.w500,
              ),
            ),
          ],
        ),
        if (showReadingLabel) ...[
          const SizedBox(height: 6),
          const Text(
            "Reading Time",
            style: TextStyle(
              fontSize: 14,
              color: Colors.red,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ],
    );
  }
}

class _QuestionCard extends StatelessWidget {
  const _QuestionCard({required this.onFinish});

  final VoidCallback onFinish;

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(24),
        border: Border.all(color: Colors.grey.shade300),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          ClipRRect(
            borderRadius: const BorderRadius.vertical(
              top: Radius.circular(24),
            ),
            child: Image.asset(
              "assets/images/projectile_motion.png",
              height: 200,
              width: double.infinity,
              fit: BoxFit.cover,
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(18),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const _QuestionTitle(),
                const SizedBox(height: 16),
                const _QuestionDescription(),
                const SizedBox(height: 24),
                const _FormulaBox(),
                const SizedBox(height: 28),
                SizedBox(
                  width: double.infinity,
                  height: 58,
                  child: ElevatedButton(
                    onPressed: onFinish,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.blue,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(30),
                      ),
                    ),
                    child: const Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Text(
                          "Finish",
                          style: TextStyle(
                            fontSize: 22,
                            color: Colors.white,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        SizedBox(width: 8),
                        Icon(
                          Icons.check_circle_outline,
                          color: Colors.white,
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _QuestionTitle extends StatelessWidget {
  const _QuestionTitle();

  @override
  Widget build(BuildContext context) {
    return const Row(
      children: [
        Icon(Icons.science, color: Colors.blue),
        SizedBox(width: 8),
        Text(
          "Projectile Motion",
          style: TextStyle(
            fontSize: 20,
            fontWeight: FontWeight.bold,
          ),
        ),
      ],
    );
  }
}

class _QuestionDescription extends StatelessWidget {
  const _QuestionDescription();

  @override
  Widget build(BuildContext context) {
    return RichText(
      text: TextSpan(
        style: TextStyle(
          color: Colors.black87,
          fontSize: 16,
          height: 1.6,
        ),
        children: [
          TextSpan(
            text:
                "A particle is projected from the ground with an initial velocity ",
          ),
          TextSpan(
            text: "v = 25 m/s",
            style: TextStyle(fontStyle: FontStyle.italic),
          ),
          TextSpan(
            text: " at an angle of theta = 30 deg above the horizontal. ",
          ),
          TextSpan(
            text: "Calculate the maximum height ",
          ),
          TextSpan(
            text: "H",
            style: TextStyle(fontStyle: FontStyle.italic),
          ),
          TextSpan(text: " reached by the particle."),
        ],
      ),
    );
  }
}

class _FormulaBox extends StatelessWidget {
  const _FormulaBox();

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: const Color(0xFFF1F2F6),
        borderRadius: BorderRadius.circular(18),
        border: const Border(
          left: BorderSide(color: Colors.blue, width: 4),
        ),
      ),
      child: const Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            "RELEVANT FORMULA",
            style: TextStyle(
              color: Colors.blue,
              fontWeight: FontWeight.bold,
              letterSpacing: 1,
            ),
          ),
          SizedBox(height: 16),
          Center(
            child: Text(
              "H = (v^2 * sin^2(theta)) / (2g)",
              style: TextStyle(
                fontSize: 24,
                fontStyle: FontStyle.italic,
              ),
              textAlign: TextAlign.center,
            ),
          ),
        ],
      ),
    );
  }
}

class _CalculatorShortcut extends StatelessWidget {
  const _CalculatorShortcut();

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 170,
      padding: const EdgeInsets.symmetric(vertical: 20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: Colors.grey.shade300),
      ),
      child: Column(
        children: [
          CircleAvatar(
            radius: 24,
            backgroundColor: const Color(0xFFE8ECF7),
            child: Icon(
              Icons.calculate,
              color: Colors.grey.shade700,
            ),
          ),
          const SizedBox(height: 12),
          const Text(
            "Calculator",
            style: TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }
}
