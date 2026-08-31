import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:flutter/material.dart';
import '../experiments/data/practical.dart';
import '../lessons/Lessons_Dashboard.dart';

class PhysicsLessonsScreen extends StatefulWidget {
  final String? grade; // e.g. "Grade 9" — optional override from nav args

  const PhysicsLessonsScreen({super.key, this.grade});

  @override
  State<PhysicsLessonsScreen> createState() => _PhysicsLessonsScreenState();
}

class _PhysicsLessonsScreenState extends State<PhysicsLessonsScreen> {
  static const Color _primaryBlue = Color(0xFF2196F3);
  static const Color _navInactive = Color(0xFFB0BEC5);

  int _currentIndex = 1;
  int? _studentGrade; // loaded from Firestore users/{uid}.grade
  bool _loadingGrade = true;
  String _selectedLessonTitle = '';

  @override
  void initState() {
    super.initState();
    _loadStudentGrade();
  }

  Future<void> _loadStudentGrade() async {
    final user = FirebaseAuth.instance.currentUser;
    if (user == null) {
      setState(() => _loadingGrade = false);
      return;
    }
    final doc = await FirebaseFirestore.instance.collection('users').doc(user.uid).get();
    final grade = doc.data()?['currentGrade'] ?? doc.data()?['grade'];
    final parsed = (grade is int) ? grade : int.tryParse(grade?.toString() ?? '');

    if (parsed == null) {
      // Grade missing — show picker and save it
      final picked = await _pickGradeDialog();
      if (picked != null) {
        await FirebaseFirestore.instance.collection('users').doc(user.uid).set(
          {'grade': picked, 'role': 'student'},
          SetOptions(merge: true),
        );
        setState(() { _studentGrade = picked; _loadingGrade = false; });
      } else {
        setState(() => _loadingGrade = false);
      }
    } else {
      setState(() { _studentGrade = parsed; _loadingGrade = false; });
    }
  }

  Future<int?> _pickGradeDialog() async {
    return showDialog<int>(
      context: context,
      barrierDismissible: false,
      builder: (ctx) => AlertDialog(
        title: const Text('Select Your Grade'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [9, 10, 11].map((g) => ListTile(
            title: Text('Grade $g'),
            leading: const Icon(Icons.school, color: Color(0xFF2196F3)),
            onTap: () => Navigator.of(ctx).pop(g),
          )).toList(),
        ),
      ),
    );
  }

  void _onNavTap(int index) {
    setState(() => _currentIndex = index);
    if (index == 0) Navigator.pushReplacementNamed(context, '/home');
    if (index == 3) Navigator.pushNamed(context, '/profile');
  }

  @override
  Widget build(BuildContext context) {
    if (_loadingGrade) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    final gradeInt = _studentGrade;
    final gradeLabel = gradeInt != null ? 'Grade $gradeInt' : 'My Lessons';

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
          style: TextStyle(color: Color(0xFF1A1C1E), fontWeight: FontWeight.bold),
        ),
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16),
            child: GestureDetector(
              onTap: () => Navigator.pushNamed(context, '/profile'),
              child: const CircleAvatar(
                backgroundColor: Color(0xFFCCCCCC),
                child: Icon(Icons.person, color: Colors.white, size: 22),
              ),
            ),
          ),
        ],
      ),
      body: gradeInt == null
          ? const Center(child: CircularProgressIndicator())
          : _LessonListBody(
              grade: gradeInt, 
              gradeLabel: gradeLabel, 
              selectedLessonTitle: _selectedLessonTitle,
              onLessonSelected: (title) => setState(() => _selectedLessonTitle = title),
            ),
      bottomNavigationBar: _buildBottomNav(),
    );
  }

  Widget _buildBottomNav() {
    return Container(
      decoration: const BoxDecoration(
        color: Colors.white,
        boxShadow: [BoxShadow(color: Colors.black12, blurRadius: 8, offset: Offset(0, -2))],
      ),
      child: BottomNavigationBar(
        type: BottomNavigationBarType.fixed,
        currentIndex: _currentIndex,
        onTap: _onNavTap,
        selectedItemColor: _primaryBlue,
        unselectedItemColor: _navInactive,
        backgroundColor: Colors.transparent,
        elevation: 0,
        selectedLabelStyle: const TextStyle(fontWeight: FontWeight.w700, fontSize: 12),
        unselectedLabelStyle: const TextStyle(fontWeight: FontWeight.w500, fontSize: 12),
        items: const [
          BottomNavigationBarItem(icon: Icon(Icons.home_outlined), activeIcon: Icon(Icons.home), label: 'Home'),
          BottomNavigationBarItem(icon: Icon(Icons.menu_book_outlined), activeIcon: Icon(Icons.menu_book), label: 'Lessons'),
          BottomNavigationBarItem(icon: Icon(Icons.science_outlined), activeIcon: Icon(Icons.science), label: 'Labs'),
          BottomNavigationBarItem(icon: Icon(Icons.person_outline), activeIcon: Icon(Icons.person), label: 'Profile'),
        ],
      ),
    );
  }
}

// ── Body: streams lessons from Firestore for the student's grade ─────────────

class _LessonListBody extends StatelessWidget {
  final int grade;
  final String gradeLabel;
  final String selectedLessonTitle;
  final Function(String) onLessonSelected;

  const _LessonListBody({
    required this.grade, 
    required this.gradeLabel,
    required this.selectedLessonTitle,
    required this.onLessonSelected,
  });

  static const Color _primaryBlue = Color(0xFF2196F3);

  @override
  Widget build(BuildContext context) {
    final query = FirebaseFirestore.instance
        .collection('lessons')
        .where('grade', isEqualTo: grade)
        .where('status', isEqualTo: 'published')
        .orderBy('order');

    return StreamBuilder<QuerySnapshot>(
      stream: query.snapshots(),
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return const Center(child: CircularProgressIndicator());
        }
        if (snapshot.hasError) {
          return Center(child: Text('Error: ${snapshot.error}'));
        }

        final docs = snapshot.data?.docs ?? [];

        return Padding(
          padding: const EdgeInsets.all(20),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                decoration: BoxDecoration(
                  color: _primaryBlue.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Text(gradeLabel,
                    style: const TextStyle(fontSize: 12, color: _primaryBlue, fontWeight: FontWeight.w700)),
              ),
              const SizedBox(height: 8),
              Text('$gradeLabel Physics',
                  style: const TextStyle(fontSize: 28, fontWeight: FontWeight.bold, color: Color(0xFF1A1C1E))),
              const SizedBox(height: 4),
              Text('${docs.length} lesson${docs.length == 1 ? '' : 's'} available',
                  style: const TextStyle(fontSize: 13, color: _primaryBlue, fontWeight: FontWeight.w600)),
              const SizedBox(height: 20),
              Expanded(
                child: docs.isEmpty
                    ? const Center(
                        child: Text('No lessons published yet.\nCheck back soon!',
                            textAlign: TextAlign.center,
                            style: TextStyle(color: Colors.grey, fontSize: 16)))
                    : ListView.builder(
                        itemCount: docs.length,
                        itemBuilder: (context, index) {
                          final doc = docs[index];
                          final data = doc.data() as Map<String, dynamic>;
                          final title = data['title'] as String? ?? 'Lesson ${index + 1}';
                          final mapped = LocalPracticals.forTopic(
                            practicalId: data['practicalId'] as String?,
                            lessonId: doc.id,
                            title: title,
                            grade: grade,
                          );
                          return _LessonCard(
                            index: index,
                            lessonId: doc.id,
                            title: title,
                            gradeLabel: gradeLabel,
                            practicalId: mapped?.id,
                            isSelected: title == selectedLessonTitle,
                            onTap: () => onLessonSelected(title),
                          );
                        },
                      ),
              ),
            ],
          ),
        );
      },
    );
  }
}

class _LessonCard extends StatelessWidget {
  final int index;
  final String lessonId;
  final String title;
  final String gradeLabel;
  final String? practicalId;
  final bool isSelected;
  final VoidCallback onTap;

  const _LessonCard({
    required this.index,
    required this.lessonId,
    required this.title,
    required this.gradeLabel,
    this.practicalId,
    required this.isSelected,
    required this.onTap,
  });

  static const _icons = [
    Icons.science, Icons.speed, Icons.fitness_center, Icons.bolt,
    Icons.graphic_eq, Icons.thermostat, Icons.lightbulb_outline,
    Icons.electric_bolt, Icons.waves, Icons.memory,
  ];

  @override
  Widget build(BuildContext context) {
    final icon = _icons[index % _icons.length];
    return GestureDetector(
      onTap: () {
        onTap();
        Navigator.push(
          context,
          MaterialPageRoute(
            builder: (_) => LessonsDashboard(
              lessonId: lessonId,
              lessonTitle: title,
              grade: '$gradeLabel Physics',
              practicalId: practicalId,
            ),
          ),
        );
      },
      child: Container(
        margin: const EdgeInsets.only(bottom: 15),
        decoration: BoxDecoration(
          color: isSelected ? const Color(0xFFE8F1FF) : Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(color: isSelected ? const Color(0xFF2196F3) : Colors.grey.shade200),
          boxShadow: [BoxShadow(color: Colors.black.withValues(alpha: 0.04), blurRadius: 6, offset: const Offset(0, 2))],
        ),
        child: ListTile(
          contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
          leading: Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(color: const Color(0xFFE8F1FF), borderRadius: BorderRadius.circular(50)),
            child: Icon(icon, color: const Color(0xFF2196F3)),
          ),
          title: Text(title, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
          trailing: const Icon(Icons.chevron_right, color: Colors.grey),
        ),
      ),
    );
  }
}