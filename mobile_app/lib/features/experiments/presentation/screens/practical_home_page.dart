import 'package:flutter/material.dart';

import '../../data/practical.dart';
import '../../data/practical_guide.dart';
import '../../data/practicals_repository.dart';
import '../widgets/practical_hero.dart';

class PracticalHomePage extends StatefulWidget {
  const PracticalHomePage({
    super.key,
    this.lessonId,
    this.lessonTitle,
    this.practicalId,
  });

  final String? lessonId;
  final String? lessonTitle;
  final String? practicalId;

  @override
  State<PracticalHomePage> createState() => _PracticalHomePageState();
}

class _PracticalHomePageState extends State<PracticalHomePage> {
  final _repo = PracticalsRepository();
  late Future<List<Practical>> _practicalsFuture;
  String? _lessonId;
  String? _lessonTitle;
  String? _practicalId;
  int? _grade;
  bool _readRouteArgs = false;

  static const Color _primaryBlue = Color(0xFF2196F3);
  static const Color _navInactive = Color(0xFFB0BEC5);
  int _selectedIndex = 2; // Labs tab selected by default

  @override
  void initState() {
    super.initState();
    _lessonId = widget.lessonId;
    _lessonTitle = widget.lessonTitle;
    _practicalId = widget.practicalId;
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_readRouteArgs) return;
    _readRouteArgs = true;
    final args = ModalRoute.of(context)?.settings.arguments;
    if (args is Map) {
      _lessonId = args['lessonId'] as String? ?? _lessonId;
      _lessonTitle = args['lessonTitle'] as String? ?? _lessonTitle;
      _practicalId = args['practicalId'] as String? ?? _practicalId;
      _grade = LocalPracticals.parseGrade(args['grade']) ?? _grade;
    }
    _practicalsFuture = _load();
  }

  Future<List<Practical>> _load() async {
    _grade ??= await _repo.currentStudentGrade();
    final wanted = LocalPracticals.forTopic(
      practicalId: _practicalId,
      lessonId: _lessonId,
      title: _lessonTitle,
    );
    final items = await _repo.fetchActiveForCurrentStudent(
      lessonId: _lessonId,
      grade: _grade,
    );
    if (wanted != null) {
      for (final item in items) {
        if (item.id == wanted.id) return [LocalPracticals.align(item)];
      }
      return [wanted];
    }
    if (items.isNotEmpty) {
      _grade ??= items.first.grade;
      return items;
    }
    if (_grade != null) return LocalPracticals.forGrade(_grade!);
    return const [];
  }

  void _reload() {
    setState(() {
      _practicalsFuture = _load();
    });
  }

  void _onItemTapped(int index) {
    setState(() {
      _selectedIndex = index;
    });
    if (index == 0) Navigator.pushNamed(context, '/home');
    if (index == 1) Navigator.pushNamed(context, '/lesson-list');
    if (index == 3) Navigator.pushNamed(context, '/profile');
  }

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
        title: Text(
          _lessonTitle == null || _lessonTitle!.isEmpty
              ? 'Practical Hub'
              : '$_lessonTitle practicals',
          style: const TextStyle(color: Color(0xFF1A1C1E), fontWeight: FontWeight.bold),
        ),
        centerTitle: true,
      ),
      body: FutureBuilder<List<Practical>>(
        future: _practicalsFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snapshot.hasError) {
            final message = snapshot.error.toString();
            final notSignedIn = message.contains('Sign in required');
            return _MessageState(
              icon: notSignedIn ? Icons.lock_outline : Icons.cloud_off,
              title: notSignedIn ? 'Sign in required' : 'Could not load practicals',
              subtitle: notSignedIn
                  ? 'Log in so the backend can load practicals for your grade.'
                  : message,
              actionLabel: notSignedIn ? 'Go to login' : 'Retry',
              onAction: notSignedIn
                  ? () => Navigator.pushReplacementNamed(context, '/login')
                  : _reload,
            );
          }
          final practicals = snapshot.data ?? const <Practical>[];
          if (practicals.isEmpty) {
            return _MessageState(
              icon: Icons.science_outlined,
              title: 'No practicals yet',
              subtitle: 'No active practicals are published for your grade.',
              actionLabel: 'Retry',
              onAction: _reload,
            );
          }
          return ListView(
            padding: const EdgeInsets.all(20),
            children: [
              const Text(
                'Interactive Experiments',
                style: TextStyle(
                  fontSize: 24,
                  fontWeight: FontWeight.bold,
                  color: Color(0xFF1A1C1E),
                ),
              ),
              const SizedBox(height: 8),
              Text(
                _lessonId == null
                    ? (_grade == null
                        ? 'Select an experiment to start your virtual lab'
                        : 'Grade $_grade experiments for your lessons')
                    : 'Related practicals for this lesson',
                style: const TextStyle(fontSize: 14, color: Colors.grey),
              ),
              const SizedBox(height: 24),
              for (final practical in practicals)
                _PracticalCard(
                  practical: practical,
                  onStart: () => Navigator.pushNamed(
                    context,
                    '/experiment-execution',
                    arguments: practical,
                  ),
                ),
            ],
          );
        },
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
}

class _PracticalCard extends StatelessWidget {
  const _PracticalCard({required this.practical, required this.onStart});

  final Practical practical;
  final VoidCallback onStart;

  @override
  Widget build(BuildContext context) {
    final guide = PracticalGuide.forPractical(
      practical.id,
      title: practical.title,
    );

    return Container(
      margin: const EdgeInsets.only(bottom: 24),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(24),
        boxShadow: [
          BoxShadow(
            color: guide.color.withOpacity(0.12),
            blurRadius: 18,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(24),
        child: Material(
          color: Colors.transparent,
          child: InkWell(
            onTap: onStart,
            borderRadius: BorderRadius.circular(24),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                PracticalHeroCard(practical: practical, compact: true),
                Padding(
                  padding: const EdgeInsets.fromLTRB(16, 14, 16, 16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        practical.title,
                        style: const TextStyle(
                          fontSize: 17,
                          fontWeight: FontWeight.bold,
                          color: Color(0xFF1A1C1E),
                        ),
                      ),
                      const SizedBox(height: 6),
                      Text(
                        practical.description,
                        style: const TextStyle(
                          fontSize: 13,
                          height: 1.35,
                          color: Colors.grey,
                        ),
                      ),
                      const SizedBox(height: 12),
                      Wrap(
                        spacing: 6,
                        runSpacing: 6,
                        children: [
                          for (final item in guide.kit.take(3))
                            Container(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 10,
                                vertical: 5,
                              ),
                              decoration: BoxDecoration(
                                color: guide.accent.withOpacity(0.35),
                                borderRadius: BorderRadius.circular(20),
                              ),
                              child: Text(
                                item,
                                style: TextStyle(
                                  fontSize: 11,
                                  fontWeight: FontWeight.w700,
                                  color: guide.color,
                                ),
                              ),
                            ),
                        ],
                      ),
                      const SizedBox(height: 14),
                      Align(
                        alignment: Alignment.centerRight,
                        child: Container(
                          padding: const EdgeInsets.symmetric(
                            horizontal: 18,
                            vertical: 9,
                          ),
                          decoration: BoxDecoration(
                            color: guide.color,
                            borderRadius: BorderRadius.circular(12),
                          ),
                          child: const Text(
                            'Open lab',
                            style: TextStyle(
                              color: Colors.white,
                              fontWeight: FontWeight.bold,
                              fontSize: 14,
                            ),
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _MessageState extends StatelessWidget {
  const _MessageState({
    required this.icon,
    required this.title,
    required this.subtitle,
    this.actionLabel,
    this.onAction,
  });

  final IconData icon;
  final String title;
  final String subtitle;
  final String? actionLabel;
  final VoidCallback? onAction;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, size: 48, color: Colors.grey),
            const SizedBox(height: 16),
            Text(
              title,
              style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 8),
            Text(
              subtitle,
              textAlign: TextAlign.center,
              style: const TextStyle(color: Colors.grey),
            ),
            if (actionLabel != null && onAction != null) ...[
              const SizedBox(height: 20),
              ElevatedButton(onPressed: onAction, child: Text(actionLabel!)),
            ],
          ],
        ),
      ),
    );
  }
}