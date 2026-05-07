import 'package:flutter/material.dart';

class PhysicsLessonsScreen extends StatefulWidget {
  const PhysicsLessonsScreen({super.key});

  @override
  State<PhysicsLessonsScreen> createState() => _PhysicsLessonsScreenState();
}

class _PhysicsLessonsScreenState extends State<PhysicsLessonsScreen> {
  int _currentIndex = 1;
  String _selectedLessonTitle = 'Linear Motion';

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
              backgroundColor: const Color(0xFFE8F1FF),
              child: Icon(Icons.person_outline, color: Colors.blue.shade800),
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
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: _currentIndex,
        type: BottomNavigationBarType.fixed,
        selectedItemColor: Colors.blue,
        unselectedItemColor: Colors.grey,
        onTap: (index) => setState(() => _currentIndex = index),
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.home_outlined),
            label: 'Home',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.menu_book),
            label: 'Lessons',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.biotech_outlined),
            label: 'Labs',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.person_outline),
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
      onTap: () => setState(() => _selectedLessonTitle = title),
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
