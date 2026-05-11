import 'package:flutter/material.dart';
import '../lessons/Lessons_Dashboard.dart';
import 'lesson_list_data.dart';

class PhysicsLessonsScreen extends StatefulWidget {
  const PhysicsLessonsScreen({super.key});

  @override
  State<PhysicsLessonsScreen> createState() => _PhysicsLessonsScreenState();
}

class _PhysicsLessonsScreenState extends State<PhysicsLessonsScreen> {
  static const Color _primaryBlue = Color(0xFF2196F3);
  static const Color _navInactive = Color(0xFFB0BEC5);

  int _currentIndex = 1;
  String _selectedLessonTitle = '';
  String _grade = 'Grade 10';

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    final args = ModalRoute.of(context)?.settings.arguments;
    if (args is Map) {
      final incoming = args['grade'] as String? ?? 'Grade 10';
      if (_grade != incoming) {
        setState(() {
          _grade = incoming;
          _selectedLessonTitle = '';
        });
      }
    }
  }

  void _onNavTap(int index) {
    setState(() => _currentIndex = index);
  }

  List<LessonItem> get _lessons => getLessonsForGrade(_grade);
  String get _subtitle => gradeSubtitles[_grade] ?? 'Physics Lessons';

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FE),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          onPressed: () => Navigator.of(context).maybePop(),
          icon: const Icon(Icons.arrow_back, color: Color(0xFF2196F3)),
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
            // Grade badge
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
              decoration: BoxDecoration(
                color: _primaryBlue.withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(20),
              ),
              child: Text(
                _grade,
                style: const TextStyle(
                  fontSize: 12,
                  color: _primaryBlue,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ),
            const SizedBox(height: 8),
            Text(
              '$_grade Physics',
              style: const TextStyle(
                fontSize: 28,
                fontWeight: FontWeight.bold,
                color: Color(0xFF1A1C1E),
              ),
            ),
            const SizedBox(height: 4),
            Text(
              _subtitle,
              style: const TextStyle(fontSize: 16, color: Colors.grey),
            ),
            const SizedBox(height: 8),
            Text(
              '${_lessons.length} lessons available',
              style: TextStyle(
                fontSize: 13,
                color: _primaryBlue,
                fontWeight: FontWeight.w600,
              ),
            ),
            const SizedBox(height: 20),
            Expanded(
              child: ListView.builder(
                itemCount: _lessons.length,
                itemBuilder: (context, index) {
                  final lesson = _lessons[index];
                  return _buildLessonCard(
                    index: index,
                    title: lesson.title,
                    subtitle: lesson.subtitle,
                    duration: lesson.duration,
                  );
                },
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
    required int index,
    required String title,
    required String subtitle,
    required String duration,
  }) {
    final isSelected = title == _selectedLessonTitle;

    // Icon per index cycling through meaningful icons
    final icons = [
      Icons.science,
      Icons.speed,
      Icons.fitness_center,
      Icons.bolt,
      Icons.graphic_eq,
      Icons.thermostat,
      Icons.lightbulb_outline,
      Icons.electric_bolt,
      Icons.waves,
      Icons.memory,
    ];
    final icon = icons[index % icons.length];

    return GestureDetector(
      onTap: () {
        setState(() => _selectedLessonTitle = title);
        Navigator.push(
          context,
          MaterialPageRoute(
            builder: (context) => LessonsDashboard(
              lessonTitle: title,
              grade: '$_grade Physics',
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
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.04),
              blurRadius: 6,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: ListTile(
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
            child: Icon(icon, color: const Color(0xFF2196F3)),
          ),
          title: Text(
            title,
            style: const TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 16,
            ),
          ),
          subtitle: Row(
            children: [
              const Icon(Icons.access_time, size: 13, color: Colors.grey),
              const SizedBox(width: 4),
              Text(
                duration,
                style: const TextStyle(color: Colors.grey, fontSize: 12),
              ),
            ],
          ),
          trailing: const Icon(Icons.chevron_right, color: Colors.grey),
        ),
      ),
    );
  }
}
