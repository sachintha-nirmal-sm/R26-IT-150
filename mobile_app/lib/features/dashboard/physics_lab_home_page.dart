import 'package:flutter/material.dart';

/// ────────────────────────────────────────────────────────────────────────────
/// Physics Lab – Home Page
/// ────────────────────────────────────────────────────────────────────────────
class PhysicsLabHomePage extends StatefulWidget {
  const PhysicsLabHomePage({super.key});

  @override
  State<PhysicsLabHomePage> createState() => _PhysicsLabHomePageState();
}

class _PhysicsLabHomePageState extends State<PhysicsLabHomePage> {
  int _selectedIndex = 0;

  // ── colour tokens ────────────────────────────────────────────────────────────
  static const Color _bg = Color(0xFFF4F6FB);
  static const Color _primaryBlue = Color(0xFF2196F3);
  static const Color _bodyText = Color(0xFF1A1A2E);
  static const Color _subtitleText = Color(0xFF6B7280);
  static const Color _searchBg = Color(0xFFEEEFF4);
  static const Color _navInactive = Color(0xFFAAAAAA);

  void _onNavTap(int index) {
    setState(() => _selectedIndex = index);
    if (index == 3) {
      Navigator.pushNamed(context, '/profile');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: _bg,
      appBar: _buildAppBar(),
      body: SafeArea(
        child: Stack(
          children: [
            SingleChildScrollView(
              physics: const BouncingScrollPhysics(),
              padding: const EdgeInsets.fromLTRB(16, 12, 16, 50),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  _greetingCard(),
                  const SizedBox(height: 16),
                  _searchBar(),
                  const SizedBox(height: 16),
                  _progressCard(),
                  const SizedBox(height: 16),
                  _continueButton(context),
                  const SizedBox(height: 24),
                  _recommendedLabel(),
                  const SizedBox(height: 12),
                  _recommendedRow(),
                  const SizedBox(height: 16),
                  _virtualLabsBanner(),
                ],
              ),
            ),
            Positioned(
              right: 0,
              bottom: 0,
              child: GestureDetector(
                onTap: () => Navigator.pushNamed(context, "/chatbot"),
                child: SizedBox(
                  width: 100,
                  height: 100,
                  child: Image.asset(
                    'assets/animations/Chatbot.gif',
                    fit: BoxFit.contain,
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
      bottomNavigationBar: _buildBottomNav(),
    );
  }

  // ── AppBar ───────────────────────────────────────────────────────────────────
  AppBar _buildAppBar() {
    return AppBar(
      backgroundColor: Colors.white,
      elevation: 0,
      titleSpacing: 16,
      title: Row(
        children: [
          const Icon(Icons.science, color: _primaryBlue, size: 26),
          const SizedBox(width: 8),
          Text(
            'Physics Lab',
            style: const TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.w800,
              color: _bodyText,
            ),
          ),
        ],
      ),
      actions: [
        Padding(
          padding: const EdgeInsets.only(right: 16),
          child: GestureDetector(
            onTap: () => Navigator.pushNamed(context, '/profile'),
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

  // ── Greeting card ────────────────────────────────────────────────────────────
  Widget _greetingCard() {
    return _card(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Hello, Alex',
            style: const TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.w800,
              color: _bodyText,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            'Let\'s continue your physics learning',
            style: const TextStyle(
              fontSize: 14,
              color: _subtitleText,
            ),
          ),
        ],
      ),
    );
  }

  // ── Search bar ───────────────────────────────────────────────────────────────
  Widget _searchBar() {
    return Container(
      height: 48,
      decoration: BoxDecoration(
        color: _searchBg,
        borderRadius: BorderRadius.circular(30),
      ),
      padding: const EdgeInsets.symmetric(horizontal: 16),
      child: Row(
        children: [
          const Icon(Icons.search, color: _subtitleText, size: 20),
          const SizedBox(width: 10),
          Text(
            'Search lessons, quizzes...',
            style: const TextStyle(
              fontSize: 14,
              color: _subtitleText,
            ),
          ),
        ],
      ),
    );
  }

  // ── Progress card ────────────────────────────────────────────────────────────
  Widget _progressCard() {
    return _card(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'YOUR PROGRESS',
                style: const TextStyle(
                  fontSize: 11,
                  fontWeight: FontWeight.w800,
                  color: _primaryBlue,
                  letterSpacing: 1.2,
                ),
              ),
              const Icon(Icons.bar_chart, color: _subtitleText, size: 20),
            ],
          ),
          const SizedBox(height: 10),
          Text(
            '12 Completed Lessons',
            style: const TextStyle(
              fontSize: 16,
              fontWeight: FontWeight.w700,
              color: _bodyText,
            ),
          ),
          const SizedBox(height: 10),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'Current: Linear Motion',
                style: const TextStyle(fontSize: 13, color: _subtitleText),
              ),
              Text(
                '60%',
                style: const TextStyle(
                  fontSize: 13,
                  fontWeight: FontWeight.w700,
                  color: _primaryBlue,
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          ClipRRect(
            borderRadius: BorderRadius.circular(6),
            child: LinearProgressIndicator(
              value: 0.60,
              minHeight: 6,
              backgroundColor: const Color(0xFFDDE3F8),
              valueColor: const AlwaysStoppedAnimation<Color>(_primaryBlue),
            ),
          ),
        ],
      ),
    );
  }

  // ── Continue Learning button ──────────────────────────────────────────────────
  Widget _continueButton(BuildContext context) {
    return SizedBox(
      width: double.infinity,
      height: 52,
      child: ElevatedButton.icon(
        onPressed: () {
          Navigator.of(context).pushNamed('/lesson-list');
        },
        icon: const Icon(Icons.play_arrow, size: 20, color: Colors.white),
        label: const Text(
          'Continue Learning',
          style: TextStyle(
            fontSize: 16,
            fontWeight: FontWeight.w700,
            color: Colors.white,
          ),
        ),
        style: ElevatedButton.styleFrom(
          backgroundColor: _primaryBlue,
          elevation: 0,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
        ),
      ),
    );
  }

  // ── "Recommended for you" heading ─────────────────────────────────────────────
  Widget _recommendedLabel() {
    return Text(
      'Recommended for you',
      style: const TextStyle(
        fontSize: 16,
        fontWeight: FontWeight.w800,
        color: _bodyText,
      ),
    );
  }

  // ── Recommended cards row ─────────────────────────────────────────────────────
  Widget _recommendedRow() {
    return Row(
      children: [
        Expanded(
          child: _recommendedCard(
            iconBg: const Color(0xFFFDE8D8),
            icon: Icons.local_fire_department,
            iconColor: const Color(0xFFEA580C),
            title: 'Thermodynamics Basics',
            subtitle: '15 mins • Quiz',
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: _recommendedCard(
            iconBg: const Color(0xFFEDE9FE),
            icon: Icons.bolt,
            iconColor: const Color(0xFF7C3AED),
            title: 'Circuit Fundamentals',
            subtitle: '22 mins • Lab',
          ),
        ),
      ],
    );
  }

  Widget _recommendedCard({
    required Color iconBg,
    required IconData icon,
    required Color iconColor,
    required String title,
    required String subtitle,
  }) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: const [
          BoxShadow(
            color: Colors.black12,
            blurRadius: 8,
            offset: Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            width: 44,
            height: 44,
            decoration: BoxDecoration(
              color: iconBg,
              borderRadius: BorderRadius.circular(10),
            ),
            child: Icon(icon, color: iconColor, size: 26),
          ),
          const SizedBox(height: 12),
          Text(
            title,
            style: const TextStyle(
              fontSize: 14,
              fontWeight: FontWeight.w700,
              color: _bodyText,
              height: 1.3,
            ),
          ),
          const SizedBox(height: 6),
          Text(
            subtitle,
            style: const TextStyle(
              fontSize: 12,
              color: _subtitleText,
            ),
          ),
        ],
      ),
    );
  }

  // ── Explore Virtual Labs wide banner ─────────────────────────────────────────
  Widget _virtualLabsBanner() {
    return Container(
      height: 160,
      width: double.infinity,
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(16),
        gradient: const LinearGradient(
          begin: Alignment.topRight,
          end: Alignment.bottomLeft,
          colors: [
            Color(0xFF64B5F6),
            Color(0xFF2196F3),
            Color(0xFF1976D2),
          ],
        ),
        boxShadow: const [
          BoxShadow(
            color: Colors.black12,
            blurRadius: 8,
            offset: Offset(0, 2),
          ),
        ],
      ),
      child: Stack(
        children: [
          // Decorative circles for depth
          Positioned(
            top: -20,
            right: -20,
            child: Container(
              width: 120,
              height: 120,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                color: Colors.white.withValues(alpha: 0.07),
              ),
            ),
          ),
          Positioned(
            top: 20,
            right: 50,
            child: Container(
              width: 70,
              height: 70,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                color: Colors.white.withValues(alpha: 0.07),
              ),
            ),
          ),
          // Content
          Padding(
            padding: const EdgeInsets.all(20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                const Text(
                  'Explore Virtual Labs',
                  style: TextStyle(
                    fontSize: 17,
                    fontWeight: FontWeight.w800,
                    color: Colors.white,
                  ),
                ),
                const SizedBox(height: 4),
                const Text(
                  'Interactive simulations for Grade 10',
                  style: TextStyle(
                    fontSize: 13,
                    color: Colors.white70,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  // ── Bottom Navigation Bar ─────────────────────────────────────────────────────
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

  // ── Shared card wrapper ───────────────────────────────────────────────────────
  Widget _card({required Widget child}) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: const [
          BoxShadow(
            color: Colors.black12,
            blurRadius: 8,
            offset: Offset(0, 2),
          ),
        ],
      ),
      child: child,
    );
  }
}
