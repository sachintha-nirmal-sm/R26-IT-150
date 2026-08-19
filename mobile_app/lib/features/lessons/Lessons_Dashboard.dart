import 'package:flutter/material.dart';
import '../LessonList/lesson_list_page.dart';
import '../quizzes/lesson_quizzes_page.dart';
import '../experiments/presentation/screens/experiment_execution_screen.dart';

class LessonsDashboard extends StatefulWidget {
  final String lessonTitle;
  final String grade;
  final String? lessonDescription;

  const LessonsDashboard({
    super.key,
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

  @override
  void initState() {
    super.initState();
    _currentLessonDescription = widget.lessonDescription ??
        _getDescriptionForLesson(widget.lessonTitle);
  }

  String _getDescriptionForLesson(String title) {
    final descriptions = {
      'Introduction to Physics': 'Learn the basics of physics and explore fundamental principles that govern the universe.',
      'Basic Concepts Associated with Force': 'Understand the fundamental concepts of force, types of forces, and their effects.',
      'Pressure Exerted by Solid': 'Learn about pressure in solids, thrust, and practical applications of pressure in everyday life.',
      'Density': 'Explore mass, volume, and density calculations for various solids and liquids.',
      'Reflection and Refraction of Waves': 'Study wave properties, behavior, reflection, and refraction through different media.',
      'Simple Machines': 'Discover levers, pulleys, inclined planes, mechanical advantage, and efficiency of simple machines.',
      'Nanotechnology and its Applications': 'An introduction to nanoscience, nanomaterials, and futuristic applications of nanotechnology.',
      'Linear Motion': 'Master the fundamental concepts of push, pull, and the laws governing motion.',
      'Motion in a straight line': 'Master the fundamental concepts of displacement, velocity, acceleration, and motion graphs.',
      "Forces and Newton's Laws": 'Understand the three laws of motion and how forces affect objects.',
      "Newton's laws of motion": 'Understand the three laws of motion and how forces affect objects.',
      'Friction': 'Explore static and dynamic friction, advantages, disadvantages, and ways to modify friction.',
      'Resultant force': 'Learn how to determine the resultant of concurrent forces acting on a body.',
      'Turning effect of a force': 'Understand moments, torque, principle of moments, and equilibrium in rotation.',
      'Turning Effect of Forces': 'Understand moments, torque, principle of moments, and equilibrium in rotation.',
      'Equilibrium of Forces': 'Study stable, unstable, and neutral equilibrium under coplanar forces.',
      'Hydrostatic pressure and its applications': 'Understand liquid pressure, Pascal\'s law, hydraulic systems, and atmospheric pressure.',
      'Work, Energy, and Power': 'Discover the concepts of work, energy transformation, and power in physical systems.',
      'Work, energy and power': 'Discover the concepts of work, energy transformation, and power in physical systems.',
      'Current electricity': 'Learn about electric current, voltage, resistance, Ohm\'s law, and simple circuits.',
      'Waves and Sound': 'Explore the properties of waves and how sound travels through different media.',
      'Waves and their applications': 'Explore wave characteristics, sound waves, electromagnetic waves, and applications.',
      'Geometrical Optics': 'Study reflection, refraction, lenses, mirrors, and optical instruments.',
      'Heat': 'Understand temperature, thermal expansion, specific heat capacity, and heat transfer.',
      'Power and Energy of Electric Appliances': 'Calculate electrical power, energy consumption, and electrical safety in appliances.',
      'Electronics': 'Discover semiconductors, diodes, transistors, and logic gates in electronic circuits.',
      'Electromagnetism and Electromagnetic Induction': 'Understand magnetic fields, electromagnetic induction, transformers, and generators.',
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
          icon: const Icon(Icons.arrow_back, color: Color(0xFF2196F3)),
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
  onTap: () {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => const LessonQuizzesPage(),
      ),
    );
  },

  child: _buildGridCard(
    icon: Icons.quiz_outlined,
    label: 'Quizzes',
    iconColor: const Color(0xFF2196F3),
    bgColor: const Color.fromARGB(255, 210, 235, 255),
  ),
),
                  GestureDetector(
                    onTap: () {
                      // ── Grade 9 Topics ──────────────────────────────────
                      if (widget.lessonTitle == 'Basic Concepts Associated with Force') {
                        Navigator.pushNamed(context, '/force-game');
                      } else if (widget.lessonTitle == 'Pressure Exerted by Solid') {
                        Navigator.pushNamed(context, '/pressure-puzzle');
                      } else if (widget.lessonTitle == 'Density') {
                        Navigator.pushNamed(context, '/density-puzzle');
                      } else if (widget.lessonTitle == 'Simple Machines' ||
                          widget.lessonTitle == 'Turning Effect of Forces') {
                        Navigator.pushNamed(context, '/simple-machines-game');
                      } else if (widget.lessonTitle == 'Nanotechnology and its Applications') {
                        Navigator.pushNamed(context, '/nano-shield');
                      } else if (widget.lessonTitle == 'Reflection and Refraction of Waves') {
                        Navigator.pushNamed(context, '/waves-game');

                      // ── Grade 10 Topics ─────────────────────────────────
                      } else if (widget.lessonTitle == 'Motion in a straight line' ||
                          widget.lessonTitle == 'Linear Motion') {
                        Navigator.pushNamed(context, '/motion-quest');
                      } else if (widget.lessonTitle == "Newton's laws of motion" ||
                          widget.lessonTitle == "Forces and Newton's Laws") {
                        Navigator.pushNamed(context, '/newton-game');
                      } else if (widget.lessonTitle == 'Friction') {
                        Navigator.pushNamed(context, '/friction-game');
                      } else if (widget.lessonTitle == 'Resultant force') {
                        Navigator.pushNamed(context, '/resultant-force');
                      } else if (widget.lessonTitle == 'Turning effect of a force') {
                        Navigator.pushNamed(context, '/turning-effect');
                      } else if (widget.lessonTitle == 'Equilibrium of Forces') {
                        Navigator.pushNamed(context, '/equilibrium-forces');
                      } else if (widget.lessonTitle == 'Hydrostatic pressure and its applications') {
                        Navigator.pushNamed(context, '/hydrostatic-pressure');
                      } else if (widget.lessonTitle == 'Work, energy and power' ||
                          widget.lessonTitle == 'Work, Energy, and Power') {
                        Navigator.pushNamed(context, '/work-power-game');
                      } else if (widget.lessonTitle == 'Current electricity') {
                        Navigator.pushNamed(context, '/current-electricity-game');

                      // ── Grade 11 Topics ─────────────────────────────────
                      } else if (widget.lessonTitle == 'Waves and their applications' ||
                          widget.lessonTitle == 'Waves and Sound') {
                        Navigator.pushNamed(context, '/waves-game');
                      } else if (widget.lessonTitle == 'Geometrical Optics') {
                        Navigator.pushNamed(context, '/geometrical-optics-game');
                      } else if (widget.lessonTitle == 'Heat') {
                        Navigator.pushNamed(context, '/heat-game');
                      } else if (widget.lessonTitle == 'Power and Energy of Electric Appliances') {
                        Navigator.pushNamed(context, '/power-energy-game');
                      } else if (widget.lessonTitle == 'Electronics') {
                        Navigator.pushNamed(context, '/electronics-game');
                      } else if (widget.lessonTitle == 'Electromagnetism and Electromagnetic Induction') {
                        Navigator.pushNamed(context, '/electromagnetism-game');

                      // ── Fallback ─────────────────────────────────────────
                      } else {
                        Navigator.pushNamed(context, '/game-intro');
                      }
                    },
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
                            color: Color(0xFF2196F3),
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
