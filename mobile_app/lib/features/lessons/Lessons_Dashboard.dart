import 'package:flutter/material.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import '../LessonList/lesson_list_page.dart';
import '../quizzes/student_quiz_screen.dart';
import '../experiments/presentation/screens/experiment_execution_screen.dart';
import '../games/vector_quest/presentation/pages/vector_quest_game_screen.dart';
import 'sub_lessons_screen.dart';

class LessonsDashboard extends StatefulWidget {
  final String lessonId;
  final String lessonTitle;
  final String grade;
  final String? lessonDescription;

  const LessonsDashboard({
    super.key,
    required this.lessonId,
    this.lessonTitle = 'Linear Motion',
    this.grade = 'Grade 9 Physics',
    this.lessonDescription,
  });

  @override
  State<LessonsDashboard> createState() => _LessonsDashboardState();
}

class _LessonsDashboardState extends State<LessonsDashboard> {
  static const Color _primaryBlue = Color(0xFF2196F3);
  static const Color _navInactive = Color(0xFFB0BEC5);
  
  int _selectedIndex = 1; // Lessons tab selected by default

  late String _currentLessonDescription;
  bool? _hasSubLessons; // null = still checking

  @override
  void initState() {
    super.initState();
    _currentLessonDescription = widget.lessonDescription ??
        _getDescriptionForLesson(widget.lessonTitle);
    _checkSubLessons();
  }

  Future<void> _checkSubLessons() async {
    try {
      final snap = await FirebaseFirestore.instance
          .collection('lessons')
          .doc(widget.lessonId)
          .collection('subLessons')
          .limit(1)
          .get();
      if (mounted) setState(() => _hasSubLessons = snap.docs.isNotEmpty);
    } catch (_) {
      if (mounted) setState(() => _hasSubLessons = false);
    }
  }

  void _navigateToQuiz() {
    if (_hasSubLessons == true) {
      Navigator.push(
        context,
        MaterialPageRoute(
          builder: (_) => SubLessonsScreen(
            lessonId: widget.lessonId,
            lessonTitle: widget.lessonTitle,
            grade: widget.grade,
          ),
        ),
      );
    } else {
      Navigator.push(
        context,
        MaterialPageRoute(
          builder: (_) => StudentQuizScreen(
            lessonId: widget.lessonId,
            lessonTitle: widget.lessonTitle,
          ),
        ),
      );
    }
  }

  String _getDescriptionForLesson(String title) {
    final descriptions = {
      'Introduction to Physics': 'Learn the basics of physics and explore fundamental principles that govern the universe.',
      'Linear Motion': 'Master the fundamental concepts of push, pull, and the laws governing motion.',
      "Forces and Newton's Laws": 'Understand the three laws of motion and how forces affect objects.',
      'Work, Energy, and Power': 'Discover the concepts of work, energy transformation, and power in physical systems.',
      'Waves and Sound': 'Explore the properties of waves and how sound travels through different media.',
    };
    return descriptions[title] ?? 'Master the fundamental concepts of this lesson.';
  }

  void _onItemTapped(int index) {
    setState(() {
      _selectedIndex = index;
    });
    if (index == 3) {
      Navigator.pushNamed(context, '/profile');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F6FA),
      appBar: AppBar(
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color:  const Color(0xFF2196F3)),
          onPressed: () => Navigator.pushReplacement(
            context,
            MaterialPageRoute(
              builder: (context) => const PhysicsLessonsScreen(),
            ),
          ),
        ),
        title: Text(
          widget.grade,
          style: const TextStyle(
            color: Color.fromARGB(255, 0, 0, 0),
            fontWeight: FontWeight.bold,
          ),
        ),
        centerTitle: true,
        backgroundColor: Colors.white,
        elevation: 0,
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16.0),
            child: GestureDetector(
              onTap: () => Navigator.pushNamed(context, '/profile'),
              child: const CircleAvatar(
                radius: 20,
                backgroundColor: Color.fromARGB(255, 190, 190, 191),
                child: Icon(
                  Icons.person,
                  color: Color.fromARGB(255, 246, 250, 253),
                ),
              ),
            ),
          ),
        ],
      ),
      body: SingleChildScrollView(
        child: Padding(
          padding: const EdgeInsets.all(20.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Hero Card
              Container(
                decoration: BoxDecoration(
                  color: const Color(0xFFE8F1FB),
                  borderRadius: BorderRadius.circular(16),
                ),
                padding: const EdgeInsets.all(24),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                
                    const SizedBox(height: 8),
                    Text(
                      widget.lessonTitle,
                      style: const TextStyle(
                        fontSize: 28,
                        fontWeight: FontWeight.bold,
                        color: Color.fromARGB(255, 0, 0, 0),
                      ),
                    ),
                    const SizedBox(height: 12),
                    Text(
                      _currentLessonDescription,
                      style: const TextStyle(
                        fontSize: 14,
                        color: Colors.grey,
                        height: 1.5,
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 32),

              // 2x2 Icon Grid
              GridView.count(
                crossAxisCount: 2,
                mainAxisSpacing: 16,
                crossAxisSpacing: 16,
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                children: [
                  GestureDetector(
                    onTap: _hasSubLessons == null ? null : _navigateToQuiz,
                    child: _buildGridCard(
                      icon: Icons.quiz_outlined,
                      label: 'Quizzes',
                      iconColor: _hasSubLessons == null
                          ? Colors.grey
                          : const Color(0xFF2196F3),
                      bgColor: const Color.fromARGB(255, 210, 235, 255),
                    ),
                  ),
                  GestureDetector(
                    onTap: () => Navigator.pushNamed(context, '/game-intro'),
                    child: _buildGridCard(
                      icon: Icons.sports_esports_outlined,
                      label: 'Games',
                      iconColor: const Color(0xFF2196F3),
                      bgColor: const Color.fromARGB(255, 210, 235, 255),
                    ),
                  ),
                  GestureDetector(
                    onTap: () {
                      Navigator.push(
                        context,
                        MaterialPageRoute(
                          builder: (context) =>
                              const ExperimentExecutionScreen(),
                        ),
                      );
                    },
                    child: _buildGridCard(
                      icon: Icons.science_outlined,
                      label: 'Practicals',
                      iconColor: const Color(0xFF2196F3),
                      bgColor: const Color.fromARGB(255, 210, 235, 255),
                    ),
                  ),
                  _buildGridCard(
                    icon: Icons.menu_book_outlined,
                    label: 'Learning Materials',
                    iconColor: const Color(0xFF2196F3),
                    bgColor: const Color.fromARGB(255, 210, 235, 255),
                  ),
                ],
              ),
              const SizedBox(height: 32),

              // Centered Scenario Card
              Center(
                child: GestureDetector(
                  onTap: () =>
                      Navigator.pushNamed(context, "/scenario-question"),
                  child: Container(
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(16),
                      border: Border.all(color: const Color(0xFFE0E0E0)),
                    ),
                    padding: const EdgeInsets.symmetric(
                      horizontal: 32,
                      vertical: 24,
                    ),
                    child: Column(
                      children: [
                        Container(
                          decoration: BoxDecoration(
                            color: const Color(0xFFE8F1FB),
                            shape: BoxShape.circle,
                          ),
                          padding: const EdgeInsets.all(12),
                          child: const Icon(
                            Icons.menu_book_outlined,
                            color: const Color(0xFF2196F3),
                            size: 28,
                          ),
                        ),
                        const SizedBox(height: 12),
                        const Text(
                          "Scenario Based\nQuestion",
                          textAlign: TextAlign.center,
                          style: TextStyle(
                            fontSize: 14,
                            fontWeight: FontWeight.w600,
                            color: Color.fromARGB(255, 0, 0, 0),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ],
          ),
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
        currentIndex: _selectedIndex,
        onTap: _onItemTapped,
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

  Widget _buildGridCard({
    required IconData icon,
    required String label,
    required Color iconColor,
    required Color bgColor,
  }) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: const Color(0xFFE0E0E0)),
      ),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Container(
            decoration: BoxDecoration(
              color: bgColor,
              shape: BoxShape.circle,
            ),
            padding: const EdgeInsets.all(12),
            child: Icon(
              icon,
              color: iconColor,
              size: 32,
            ),
          ),
          const SizedBox(height: 12),
          Text(
            label,
            textAlign: TextAlign.center,
            style: const TextStyle(
              fontSize: 14,
              fontWeight: FontWeight.w600,
              color: Colors.black,
            ),
          ),
        ],
      ),
    );
  }
}
