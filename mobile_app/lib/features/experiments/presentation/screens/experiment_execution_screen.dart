import 'package:flutter/material.dart';
import 'experiment_in_progress_screen.dart';
import 'practice_experience_screen.dart';

class ExperimentExecutionScreen extends StatefulWidget {
  const ExperimentExecutionScreen({super.key});

  @override
  State<ExperimentExecutionScreen> createState() =>
      _ExperimentExecutionScreenState();
}

class _ExperimentExecutionScreenState extends State<ExperimentExecutionScreen> {
  int _selectedBottomNavIndex = 1;
  bool _instructionsExpanded = true;

  final List<Map<String, String>> _steps = [
    {
      'number': '1',
      'title': 'Calibrate the motion sensor',
      'description':
          'Calibrate the motion sensor by clicking the "Reset" button while the object is at the starting position.'
    },
    {
      'number': '2',
      'title': 'Press Start Experiment',
      'description':
          'Press "Start Experiment" and observe the object\'s acceleration as it descends the incline.'
    },
    {
      'number': '3',
      'title': 'Stop the timer',
      'description':
          'Stop the timer when the object reaches the 50cm mark and record the "Data Log".'
    },
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      appBar: _buildAppBar(),
      body: SingleChildScrollView(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Video Section
              _buildVideoSection(),
              const SizedBox(height: 24),

              // Action Buttons
              _buildActionButtons(),
              const SizedBox(height: 28),

              // Instructions Section
              _buildInstructionsSection(),
              const SizedBox(height: 32),
            ],
          ),
        ),
      ),
      bottomNavigationBar: _buildBottomNavigationBar(),
    );
  }

  PreferredSizeWidget _buildAppBar() {
    return AppBar(
      elevation: 0,
      backgroundColor: Colors.white,
      leading: IconButton(
        icon: const Icon(Icons.arrow_back, color: Color(0xFF2F80ED), size: 28),
        onPressed: () => Navigator.pop(context),

      leading: Container(
        margin: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: const Color(0xFFF5F7FB),
          borderRadius: BorderRadius.circular(8),
        ),
        child: IconButton(
          icon: const Icon(Icons.arrow_back, color: Color.fromARGB(255, 42, 128, 241), size: 24),
          onPressed: () => Navigator.pop(context),
        ),
      ),
      title: const Text(
        'Physics Lab',
        style: TextStyle(
          color: Color(0xFF2F80ED),
          fontSize: 20,
          fontWeight: FontWeight.w700,
          fontFamily: 'Poppins',
        ),
      ),
      centerTitle: false,
      actions: [
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

  Widget _buildVideoSection() {
    return Container(
      padding: const EdgeInsets.symmetric(vertical: 48, horizontal: 24),
      decoration: BoxDecoration(
        color: const Color(0xFFF0F2F5),
        borderRadius: BorderRadius.circular(25),
      ),
      child: Column(
        children: [
          Container(
            width: 64,
            height: 64,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              color: Colors.white,
              boxShadow: [
                BoxShadow(
                  color: const Color(0xFF2F80ED).withValues(alpha: 0.2),
                  blurRadius: 16,
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
          const SizedBox(height: 16),
          const Text(
            'Experiment video will appear here',
            style: TextStyle(
              fontSize: 14,
              color: Color(0xFF999999),
              fontFamily: 'Poppins',
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildActionButtons() {
    return Row(
      children: [
        Expanded(
          child: Container(
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(24),
              boxShadow: [
                BoxShadow(
                  color: const Color(0xFF2F80ED).withValues(alpha: 0.3),
                  blurRadius: 12,
                  offset: const Offset(0, 4),
                ),
              ],
            ),
            child: ElevatedButton.icon(
              onPressed: () {
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => const ExperimentInProgressScreen(),
                  ),
                );
              },
              icon: const Icon(Icons.play_arrow, size: 20),
              label: const Text('Start Experiment'),
              style: ElevatedButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 14),
                backgroundColor: const Color(0xFF2F80ED),
                foregroundColor: Colors.white,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(24),
                ),
              ),
            ),
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: OutlinedButton.icon(
            onPressed: () {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (context) => const PracticeExperienceScreen(),
                ),
              );
            },
            icon: const Icon(Icons.science, size: 20),
            label: const Text('Try Demo'),
            style: OutlinedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 14),
              side: const BorderSide(
                color: Color(0xFF2F80ED),
                width: 1.5,
              ),
              foregroundColor: const Color(0xFF2F80ED),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(24),
              ),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildInstructionsSection() {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.08),
            blurRadius: 12,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        children: [
          // Header
          GestureDetector(
            onTap: () {
              setState(() {
                _instructionsExpanded = !_instructionsExpanded;
              });
            },
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Row(
                children: [
                  const Icon(
                    Icons.assignment,
                    color: Color(0xFF2F80ED),
                    size: 24,
                  ),
                  const SizedBox(width: 12),
                  const Expanded(
                    child: Text(
                      'Instructions',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.w600,
                        color: Colors.black87,
                        fontFamily: 'Poppins',
                      ),
                    ),
                  ),
                  Icon(
                    _instructionsExpanded
                        ? Icons.expand_less
                        : Icons.expand_more,
                    color: Colors.grey,
                  ),
                ],
              ),
            ),
          ),

          // Expandable Content
          if (_instructionsExpanded)
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16)
                  .copyWith(bottom: 16),
              child: Column(
                children: List.generate(
                  _steps.length,
                  (index) => Padding(
                    padding: const EdgeInsets.only(bottom: 16),
                    child: _buildStepItem(
                      _steps[index]['number']!,
                      _steps[index]['title']!,
                      _steps[index]['description']!,
                    ),
                  ),
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildStepItem(String number, String title, String description) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Container(
          width: 36,
          height: 36,
          decoration: BoxDecoration(
            shape: BoxShape.circle,
            color: const Color(0xFFE3F2FD),
            border: Border.all(
              color: const Color(0xFF2F80ED),
              width: 2,
            ),
          ),
          child: Center(
            child: Text(
              number,
              style: const TextStyle(
                color: Color(0xFF2F80ED),
                fontSize: 14,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                title,
                style: const TextStyle(
                  fontSize: 14,
                  fontWeight: FontWeight.w600,
                  color: Colors.black87,
                  fontFamily: 'Poppins',
                ),
              ),
              const SizedBox(height: 4),
              Text(
                description,
                style: const TextStyle(
                  fontSize: 13,
                  color: Color(0xFF666666),
                  height: 1.4,
                  fontFamily: 'Poppins',
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildBottomNavigationBar() {
    return BottomNavigationBar(
      currentIndex: _selectedBottomNavIndex,
      onTap: (index) {
        setState(() {
          _selectedBottomNavIndex = index;
        });
        if (index == 3) {
          Navigator.pushNamed(context, '/profile');
        }
      },
      type: BottomNavigationBarType.fixed,
      backgroundColor: Colors.white,
      selectedItemColor: const Color(0xFF2F80ED),
      unselectedItemColor: Colors.grey,
      elevation: 8,
      items: const [
        BottomNavigationBarItem(
          icon: Icon(Icons.home),
          label: 'Home',
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.science),
          label: 'Experiment',
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.menu_book),
          label: 'Library',
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.person),
          label: 'Profile',
        ),
      ],
    );
  }
}
