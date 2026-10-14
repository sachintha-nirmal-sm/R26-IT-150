import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:flutter/material.dart';

import '../experiments/data/practical.dart';
import '../experiments/data/practicals_repository.dart';
import 'lesson_progress.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> with WidgetsBindingObserver {
  int _selectedIndex = 3;
  
  // Lab / Practical Stats
  StudentPracticalProgress? _progress;

  // User profile
  String _name  = '';
  String _email = '';
  String _grade = '';

  // Quiz Stats
  int _completedCount  = 0;
  int _inProgressCount = 0;
  int _totalAttempts   = 0;

  // Sections
  List<Map<String, dynamic>> _recentAttempts = [];
  List<Map<String, dynamic>> _weakAreas      = [];

  bool _loading = true;

  final ScrollController _recentScrollCtrl = ScrollController();
  final ScrollController _weakScrollCtrl   = ScrollController();
  final ScrollController _labScrollCtrl    = ScrollController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    _loadData();
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    _recentScrollCtrl.dispose();
    _weakScrollCtrl.dispose();
    _labScrollCtrl.dispose();
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) {
      if (!_loading) {
        setState(() => _loading = true);
        _loadData();
      }
    }
  }

  Future<void> _loadData() async {
    final uid = FirebaseAuth.instance.currentUser?.uid;
    if (uid == null) {
      if (mounted) setState(() => _loading = false);
      return;
    }

    try {
      try {
        final progress = await PracticalsRepository()
            .fetchMyProgress()
            .timeout(const Duration(seconds: 8));
        if (mounted) _progress = progress;
      } catch (_) {}

      final userDoc = await FirebaseFirestore.instance
          .collection('users')
          .doc(uid)
          .get()
          .timeout(const Duration(seconds: 8));
      final u = userDoc.data() ?? {};

      final gradeRaw = u['currentGrade'] ?? u['grade'];
      final gradeStr = gradeRaw is int
          ? 'Grade $gradeRaw'
          : (gradeRaw?.toString() ?? '');

      QuerySnapshot<Map<String, dynamic>> snap;
      try {
        snap = await FirebaseFirestore.instance
            .collection('users')
            .doc(uid)
            .collection('quizAttempts')
            .orderBy('submittedAt', descending: true)
            .limit(50)
            .get()
            .timeout(const Duration(seconds: 8));
      } catch (_) {
        snap = await FirebaseFirestore.instance
            .collection('users')
            .doc(uid)
            .collection('quizAttempts')
            .limit(50)
            .get()
            .timeout(const Duration(seconds: 8));
      }

      final attempts =
          snap.docs.map((d) => Map<String, dynamic>.from(d.data())).toList();

      final Map<String, int>    bestScores = {};
      final Map<String, String> quizTitles = {};

      for (final a in attempts) {
        final qid   = a['quizId']      as String? ?? '';
        final score = a['score']       as int?    ?? 0;
        final title = (a['subLessonTitle'] as String?)?.isNotEmpty == true
            ? a['subLessonTitle'] as String
            : (a['lessonTitle'] as String? ?? qid);

        if (!bestScores.containsKey(qid) || score > bestScores[qid]!) {
          bestScores[qid] = score;
          quizTitles[qid] = title;
        }
      }

      final completed  = bestScores.values.where((s) => s >= 70).length;
      final inProgress = bestScores.values.where((s) => s < 70).length;

      final seen  = <String>{};
      final recent = <Map<String, dynamic>>[];
      for (final a in attempts) {
        final qid = a['quizId'] as String? ?? '';
        if (seen.contains(qid)) continue;
        seen.add(qid);
        final displayTitle =
            (a['subLessonTitle'] as String?)?.isNotEmpty == true
                ? '${a['subLessonNumber'] ?? ''} ${a['subLessonTitle']}'.trim()
                : (a['lessonTitle'] as String? ?? '');
        recent.add({
          'title': displayTitle,
          'score': a['score'] as int? ?? 0,
          'quizId': qid,
        });
        if (recent.length >= 5) break;
      }

      final weak = bestScores.entries
          .where((e) => e.value < 70)
          .map((e) => {'title': quizTitles[e.key] ?? e.key, 'bestScore': e.value})
          .toList()
        ..sort((a, b) => (a['bestScore'] as int).compareTo(b['bestScore'] as int));

      if (mounted) {
        setState(() {
          _name            = u['fullName'] as String? ??
                             u['displayName'] as String? ?? 'Student';
          _email           = u['email'] as String? ?? '';
          _grade           = gradeStr;
          _completedCount  = completed;
          _inProgressCount = inProgress;
          _totalAttempts   = attempts.length;
          _recentAttempts  = recent;
          _weakAreas       = weak.take(5).toList();
        });
      }
    } catch (_) {
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  // ── Build ───────────────────────────────────────────────────────────────────

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FE),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Color(0xFF2196F3)),
          onPressed: () => Navigator.pop(context),
        ),
        title: const Text('Profile',
            style: TextStyle(
                color: Color(0xFF1A1C1E), fontWeight: FontWeight.bold)),
        centerTitle: true,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh, color: Color(0xFF2196F3)),
            onPressed: () {
              setState(() => _loading = true);
              _loadData();
            },
          ),
          IconButton(
            icon: const Icon(Icons.logout, color: Colors.red),
            onPressed: () async {
              await FirebaseAuth.instance.signOut();
              if (mounted) {
                Navigator.pushNamedAndRemoveUntil(context, '/login', (route) => false);
              }
            },
          ),
        ],
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: () async {
                setState(() => _loading = true);
                await _loadData();
              },
              child: SingleChildScrollView(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.all(20),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    _buildUserCard(),
                    const SizedBox(height: 20),
                    
                    // Quiz Progress
                    _buildSectionTitle('Quiz Performance', icon: Icons.quiz_outlined, iconColor: const Color(0xFF2196F3)),
                    const SizedBox(height: 12),
                    _buildStatsRow(
                      label1: 'Completed', val1: '$_completedCount', icon1: Icons.check_circle, color1: Colors.green,
                      label2: 'Needs Retry', val2: '$_inProgressCount', icon2: Icons.refresh_rounded, color2: Colors.orange,
                      label3: 'Total Quizzes', val3: '$_totalAttempts', icon3: Icons.quiz_outlined, color3: Colors.blue,
                    ),
                    const SizedBox(height: 30),

                    // Practical / Lab Progress
                    _buildSectionTitle('Lab Performance', icon: Icons.science_outlined, iconColor: const Color(0xFF2196F3)),
                    const SizedBox(height: 12),
                    _buildStatsRow(
                      label1: 'Completed', val1: '${_progress?.completedPracticals ?? 0}', icon1: Icons.check_circle, color1: Colors.blue,
                      label2: 'Total Labs', val2: '${_progress?.totalPracticals ?? 0}', icon2: Icons.science, color2: Colors.blue,
                      label3: 'Average', val3: _progress == null ? '-' : '${_progress!.averagePercentage.round()}%', icon3: Icons.insights, color3: Colors.blue,
                    ),
                    const SizedBox(height: 30),
                    
                    // Recent Quiz Progress
                    _buildSectionTitle('Recent Quizzes'),
                    const SizedBox(height: 12),
                    _buildRecentProgress(),
                    const SizedBox(height: 30),

                    // Recent Lab Progress
                    _buildSectionTitle('Recent Labs'),
                    const SizedBox(height: 12),
                    _buildRecentLabs(),
                    const SizedBox(height: 30),

                    // Weak Areas
                    _buildSectionTitle('Areas for Improvement',
                        icon: Icons.warning_amber_rounded,
                        iconColor: Colors.red.shade700),
                    const SizedBox(height: 12),
                    _buildWeakAreas(),
                    const SizedBox(height: 20),
                  ],
                ),
              ),
            ),
      bottomNavigationBar: _buildBottomNav(),
    );
  }

  // ── User card ───────────────────────────────────────────────────────────────

  Widget _buildUserCard() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: Colors.grey.shade200),
        boxShadow: [
          BoxShadow(
              color: Colors.black.withOpacity(0.03),
              blurRadius: 10,
              offset: const Offset(0, 4))
        ],
      ),
      child: Row(children: [
        Container(
          padding: const EdgeInsets.all(3),
          decoration: BoxDecoration(
            shape: BoxShape.circle,
            border: Border.all(
                color: const Color(0xFF2196F3).withOpacity(0.2), width: 2),
          ),
          child: const CircleAvatar(
            radius: 35,
            backgroundColor: Color(0xFFCCCCCC),
            child: Icon(Icons.person, color: Colors.white, size: 35),
          ),
        ),
        const SizedBox(width: 16),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(_name.isEmpty ? 'Student' : _name,
                  style: const TextStyle(
                      fontSize: 20, fontWeight: FontWeight.bold)),
              if (_email.isNotEmpty)
                Text(_email,
                    style:
                        const TextStyle(color: Colors.grey, fontSize: 13)),
              const SizedBox(height: 8),
              if (_grade.isNotEmpty || _progress != null)
                Container(
                  padding: const EdgeInsets.symmetric(
                      horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: const Color(0xFFE8F1FF),
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Text(
                      _grade.isNotEmpty 
                        ? _grade 
                        : 'Grade ${_progress?.grade ?? ""}',
                      style: const TextStyle(
                          color: Color(0xFF2196F3),
                          fontSize: 12,
                          fontWeight: FontWeight.bold)),
                ),
            ],
          ),
        ),
      ]),
    );
  }

  // ── Stats row ───────────────────────────────────────────────────────────────

  Widget _buildStatsRow({
    required String label1, required String val1, required IconData icon1, required Color color1,
    required String label2, required String val2, required IconData icon2, required Color color2,
    required String label3, required String val3, required IconData icon3, required Color color3,
  }) {
    return Row(children: [
      Expanded(
          child: _statCard(label1, val1, icon1, color1)),
      const SizedBox(width: 12),
      Expanded(
          child: _statCard(label2, val2, icon2, color2)),
      const SizedBox(width: 12),
      Expanded(
          child: _statCard(label3, val3, icon3, color3)),
    ]);
  }

  Widget _statCard(
      String label, String value, IconData icon, Color color) {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: Colors.grey.shade200),
        boxShadow: [
          BoxShadow(
              color: Colors.black.withOpacity(0.02),
              blurRadius: 10,
              offset: const Offset(0, 4))
        ],
      ),
      child: Column(children: [
        Icon(icon, color: color, size: 24),
        const SizedBox(height: 8),
        Text(value,
            style: const TextStyle(
                fontSize: 20, fontWeight: FontWeight.bold)),
        const SizedBox(height: 4),
        Text(label,
            textAlign: TextAlign.center,
            style: TextStyle(
                color: Colors.grey.shade600,
                fontSize: 11,
                fontWeight: FontWeight.w500)),
      ]),
    );
  }

  // ── Section title ───────────────────────────────────────────────────────────

  Widget _buildSectionTitle(String title,
      {IconData? icon, Color iconColor = Colors.black}) {
    return Row(children: [
      if (icon != null) ...[
        Icon(icon, color: iconColor, size: 20),
        const SizedBox(width: 8),
      ],
      Text(title,
          style: const TextStyle(
              fontSize: 18, fontWeight: FontWeight.bold)),
    ]);
  }

  // ── Recent Progress (Quizzes) ───────────────────────────────────────────────

  Widget _buildRecentProgress() {
    if (_recentAttempts.isEmpty) {
      return _emptyCard(
          "No quiz attempts yet. Start a lesson quiz to track your progress!");
    }

    final items = _recentAttempts.map((a) {
      final score  = a['score'] as int;
      final title  = a['title'] as String;
      final passed = score >= 70;
      return _listItem(
        icon:     passed ? Icons.check_circle : Icons.refresh_rounded,
        title:    title,
        subtitle: passed ? 'Passed · $score%' : 'Score: $score% · Needs retry',
        color:    passed ? Colors.green : Colors.orange,
      );
    }).toList();

    return _scrollSection(items, _recentScrollCtrl);
  }

  // ── Recent Progress (Labs) ──────────────────────────────────────────────────

  Widget _buildRecentLabs() {
    if (_progress == null) {
      return _emptyCard("Connect to the backend to load saved practical scores.");
    }
    if (_progress!.recentResults.isEmpty) {
      return _emptyCard("Complete a practical with Start to see it here.");
    }

    final items = _progress!.recentResults.map((item) {
      return _listItem(
        icon: Icons.check_circle,
        title: item.title,
        subtitle: 'Completed · ${item.percentage.round()}%',
        color: const Color(0xFF27AE60),
      );
    }).toList();

    return _scrollSection(items, _labScrollCtrl);
  }

  // ── Areas for Improvement ───────────────────────────────────────────────────

  Widget _buildWeakAreas() {
    if (_weakAreas.isEmpty) {
      return _emptyCard("Nothing here — you're passing everything! Keep it up!");
    }

    final items = _weakAreas.map((a) {
      final score = a['bestScore'] as int;
      final title = a['title'] as String;
      final gap   = 70 - score;
      return _listItem(
        icon:     Icons.priority_high,
        title:    title,
        subtitle: 'Best score: $score% · Need $gap% more to pass',
        color:    Colors.red,
      );
    }).toList();

    return _scrollSection(items, _weakScrollCtrl);
  }

  // Scrollable section with a visible scrollbar. Shows ~2.5 items; scroll for more.
  Widget _scrollSection(List<Widget> items, ScrollController ctrl) {
    return SizedBox(
      height: 220,
      child: Scrollbar(
        controller: ctrl,
        child: ListView(
          controller: ctrl,
          padding: const EdgeInsets.only(right: 8), // breathing room for scrollbar
          children: items,
        ),
      ),
    );
  }

  // ── Helpers ─────────────────────────────────────────────────────────────────

  Widget _listItem({
    required IconData icon,
    required String   title,
    required String   subtitle,
    required Color    color,
  }) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.grey.shade200),
      ),
      child: ListTile(
        leading: Container(
          padding: const EdgeInsets.all(8),
          decoration: BoxDecoration(
              color: color.withOpacity(0.1), shape: BoxShape.circle),
          child: Icon(icon, color: color, size: 20),
        ),
        title: Text(title,
            style: const TextStyle(
                fontWeight: FontWeight.bold, fontSize: 15)),
        subtitle: Text(subtitle,
            style: const TextStyle(color: Colors.grey, fontSize: 12)),
      ),
    );
  }

  Widget _emptyCard(String message) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.grey.shade200),
      ),
      child: Text(message,
          style: TextStyle(color: Colors.grey.shade600, fontSize: 13),
          textAlign: TextAlign.center),
    );
  }

  // ── Bottom nav ──────────────────────────────────────────────────────────────

  Widget _buildBottomNav() {
    return BottomNavigationBar(
      currentIndex: _selectedIndex,
      type: BottomNavigationBarType.fixed,
      selectedItemColor: Colors.blue,
      unselectedItemColor: Colors.grey,
      onTap: (index) {
        setState(() => _selectedIndex = index);
        switch (index) {
          case 0:
            Navigator.pushNamed(context, '/home');
            break;
          case 1:
            Navigator.pushNamed(context, '/lesson-list');
            break;
          case 2:
            Navigator.pushNamed(
              context,
              '/practical-home',
              arguments: {'grade': _grade},
            );
            break;
          case 3:
            break;
        }
      },
      items: const [
        BottomNavigationBarItem(
            icon: Icon(Icons.home_outlined), label: 'Home'),
        BottomNavigationBarItem(
            icon: Icon(Icons.menu_book), label: 'Lessons'),
        BottomNavigationBarItem(
            icon: Icon(Icons.biotech_outlined), label: 'Labs'),
        BottomNavigationBarItem(
            icon: Icon(Icons.person), label: 'Profile'),
      ],
    );
  }
}