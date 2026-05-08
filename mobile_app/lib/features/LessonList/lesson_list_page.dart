import 'package:flutter/material.dart';
import '../lessons/Lessons_Dashboard.dart';

class PhysicsLessonsScreen extends StatefulWidget {
  const PhysicsLessonsScreen({super.key});

  @override
  State<PhysicsLessonsScreen> createState() => _PhysicsLessonsScreenState();
}

class _PhysicsLessonsScreenState extends State<PhysicsLessonsScreen> {
  static const Color _primaryBlue = Color(0xFF2196F3);
  static const Color _navInactive = Color(0xFFB0BEC5);
  
  int _currentIndex = 1;
  String _selectedLessonTitle = 'Linear Motion';

  void _onNavTap(int index) {
    setState(() => _currentIndex = index);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FE),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          onPressed: () => Navigator.of(context).maybePop(),
          icon: const Icon(Icons.arrow_back, color: Color(0xFF0056D2)),
        ),
        title: const Text(
          'Physics Lab',
          style: TextStyle(
            color: Color(0xFF1A1C1E),
            fontWeight: FontWeight.bold,
          ),
        ),
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16),
            child: CircleAvatar(
              backgroundColor: const Color(0xFFCCCCCC),
              child: const Icon(Icons.person, color: Colors.white, size: 22),
            ),
          ),
        ],
      ),
      body: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Grade 9 Physics',
              style: TextStyle(
                fontSize: 28,
                fontWeight: FontWeight.bold,
                color: Color(0xFF1A1C1E),
              ),
            ),
            const SizedBox(height: 4),
            const Text(
              'Core Concepts & Foundations',
              style: TextStyle(fontSize: 16, color: Colors.grey),
            ),
            const SizedBox(height: 25),
            Expanded(
              child: ListView(
                children: [
                  _buildLessonCard(
                    title: 'Introduction to Physics',
                    subtitle: 'Completed',
                    icon: Icons.check_circle_outline,
                    isCompleted: true,
                  ),
                  _buildLessonCard(
                    title: 'Linear Motion',
                    subtitle: 'In Progress (60%)',
                    icon: Icons.speed,
                    isInProgress: true,
                    progress: 0.6,
                  ),
                  _buildLessonCard(
                    title: "Forces and Newton's Laws",
                    subtitle: 'Start Lesson',
                    icon: Icons.fitness_center,
                  ),
                  _buildLessonCard(
                    title: 'Work, Energy, and Power',
                    subtitle: 'Start Lesson',
                    icon: Icons.bolt,
                  ),
                  _buildLessonCard(
                    title: 'Waves and Sound',
                    subtitle: 'Start Lesson',
                    icon: Icons.graphic_eq,
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
      bottomNavigationBar: _buildBottomNav(),
    );
  }

  Widget _buildBottomNav() {
    return Container(
      decoration: const BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.black12,
            blurRadius: 8,
            offset: Offset(0, -2),
          ),
        ],
      ),
      child: BottomNavigationBar(
        type: BottomNavigationBarType.fixed,
        currentIndex: _currentIndex,
        onTap: _onNavTap,
        selectedItemColor: _primaryBlue,
        unselectedItemColor: _navInactive,
        backgroundColor: Colors.transparent,
        elevation: 0,
        selectedLabelStyle: const TextStyle(
          fontWeight: FontWeight.w700,
          fontSize: 12,
        ),
        unselectedLabelStyle: const TextStyle(
          fontWeight: FontWeight.w500,
          fontSize: 12,
        ),
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.home_outlined),
            activeIcon: Icon(Icons.home),
            label: 'Home',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.menu_book_outlined),
            activeIcon: Icon(Icons.menu_book),
            label: 'Lessons',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.science_outlined),
            activeIcon: Icon(Icons.science),
            label: 'Labs',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.person_outline),
            activeIcon: Icon(Icons.person),
            label: 'Profile',
          ),
        ],
      ),
    );
  }

  Widget _buildLessonCard({
    required String title,
    required String subtitle,
    required IconData icon,
    bool isCompleted = false,
    bool isInProgress = false,
    double progress = 0.0,
  }) {
    final isSelected = title == _selectedLessonTitle;

    return GestureDetector(
      onTap: () {
        setState(() => _selectedLessonTitle = title);
        Navigator.of(context).pushNamed('/force-motion');

        Navigator.push(
          context,
          MaterialPageRoute(
            builder: (context) => LessonsDashboard(
              lessonTitle: title,
              grade: 'Grade 9 Physics',
            ),
          ),
        );

      },
      child: Container(
        margin: const EdgeInsets.only(bottom: 15),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: isSelected ? Colors.blue : Colors.grey.shade200,
            width: isSelected ? 2 : 1,
          ),
        ),
        child: ClipRRect(
          borderRadius: BorderRadius.circular(12),
          child: Column(
            children: [
              ListTile(
                contentPadding: const EdgeInsets.symmetric(
                  horizontal: 16,
                  vertical: 8,
                ),
                leading: Container(
                  padding: const EdgeInsets.all(10),
                  decoration: BoxDecoration(
                    color: const Color(0xFFE8F1FF),
                    borderRadius: BorderRadius.circular(50),
                  ),
                  child: Icon(icon, color: const Color(0xFF0056D2)),
                ),
                title: Text(
                  title,
                  style: const TextStyle(
                    fontWeight: FontWeight.bold,
                    fontSize: 18,
                  ),
                ),
                subtitle: Row(
                  children: [
                    if (isCompleted)
                      const Icon(Icons.circle, size: 8, color: Colors.blue),
                    if (isInProgress)
                      const Icon(
                        Icons.play_circle_outline,
                        size: 14,
                        color: Colors.blue,
                      ),
                    const SizedBox(width: 5),
                    Text(
                      subtitle,
                      style: TextStyle(
                        color: isInProgress || isCompleted
                            ? Colors.blue
                            : Colors.grey,
                        fontWeight: isInProgress || isCompleted
                            ? FontWeight.bold
                            : FontWeight.normal,
                      ),
                    ),
                  ],
                ),
                trailing: const Icon(Icons.chevron_right, color: Colors.grey),
              ),
              if (isInProgress)
                Align(
                  alignment: Alignment.centerLeft,
                  child: Container(
                    height: 4,
                    width: double.infinity,
                    color: Colors.grey.shade200,
                    child: FractionallySizedBox(
                      alignment: Alignment.bottomLeft,
                      widthFactor: progress,
                      child: Container(color: Colors.blue),
                    ),
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }
}
