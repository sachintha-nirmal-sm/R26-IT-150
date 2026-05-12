import 'package:flutter/material.dart';

class VectorQuestGameScreen extends StatefulWidget {
  const VectorQuestGameScreen({super.key});

  @override
  State<VectorQuestGameScreen> createState() => _VectorQuestGameScreenState();
}

class _VectorQuestGameScreenState extends State<VectorQuestGameScreen>
    with TickerProviderStateMixin {
  int _selectedBottomNavIndex = 2;
  bool _howToPlayExpanded = true;
  bool _controlsExpanded = false;
  bool _rulesExpanded = false;

  late final AnimationController _pulseController;
  late final Animation<double> _pulseAnim;

  static const Color _blue = Color(0xFF2F80ED);
  static const Color _purple = Color(0xFF7C3AED);
  static const Color _green = Color(0xFF16A34A);
  static const Color _orange = Color(0xFFEA580C);

  final List<Map<String, dynamic>> _howToPlaySteps = [
    {
      'number': '1',
      'icon': Icons.touch_app,
      'color': _blue,
      'title': 'Choose a Level',
      'description':
          'Select from Beginner, Intermediate, or Advanced. Each level introduces more complex vector scenarios.',
    },
    {
      'number': '2',
      'icon': Icons.gamepad,
      'color': _purple,
      'title': 'Drag & Apply Vectors',
      'description':
          'Use the on-screen joystick or drag arrows to apply force vectors to the physics object on the field.',
    },
    {
      'number': '3',
      'icon': Icons.flag,
      'color': _green,
      'title': 'Reach the Target',
      'description':
          'Guide the object to the target zone using resultant vectors. Fewer moves = higher star rating.',
    },
    {
      'number': '4',
      'icon': Icons.star,
      'color': _orange,
      'title': 'Earn Stars & Progress',
      'description':
          'Score 3 stars to unlock the next level and earn bonus XP for your Physics Lab profile.',
    },
  ];

  final List<Map<String, dynamic>> _controls = [
    {'icon': Icons.swipe, 'label': 'Swipe', 'desc': 'Set vector direction'},
    {'icon': Icons.pinch, 'label': 'Pinch', 'desc': 'Adjust magnitude'},
    {'icon': Icons.touch_app, 'label': 'Tap arrow', 'desc': 'Lock vector'},
    {'icon': Icons.play_arrow, 'label': 'Launch', 'desc': 'Apply the force'},
  ];

  final List<String> _rules = [
    'You have a limited number of vector moves per level.',
    'Exceeding the move limit will reduce your star score.',
    'Passing through red zones deducts points.',
    'Complete all 3 objectives to unlock boss levels.',
    'Hint tokens can be used once per level — use wisely!',
  ];

  @override
  void initState() {
    super.initState();
    _pulseController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1200),
    )..repeat(reverse: true);
    _pulseAnim = Tween<double>(begin: 0.92, end: 1.0).animate(
      CurvedAnimation(parent: _pulseController, curve: Curves.easeInOut),
    );
  }

  @override
  void dispose() {
    _pulseController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      appBar: _buildAppBar(),
      body: SingleChildScrollView(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _buildGamePreview(),
            const SizedBox(height: 24),
            _buildInfoChips(),
            const SizedBox(height: 20),
            _buildActionButtons(),
            const SizedBox(height: 28),
            _buildExpandableSection(
              icon: Icons.help_outline,
              title: 'How to Play',
              isExpanded: _howToPlayExpanded,
              onToggle: () =>
                  setState(() => _howToPlayExpanded = !_howToPlayExpanded),
              content: _buildHowToPlay(),
            ),
            const SizedBox(height: 12),
            _buildExpandableSection(
              icon: Icons.gamepad_outlined,
              title: 'Controls',
              isExpanded: _controlsExpanded,
              onToggle: () =>
                  setState(() => _controlsExpanded = !_controlsExpanded),
              content: _buildControls(),
            ),
            const SizedBox(height: 12),
            _buildExpandableSection(
              icon: Icons.gavel,
              title: 'Rules',
              isExpanded: _rulesExpanded,
              onToggle: () =>
                  setState(() => _rulesExpanded = !_rulesExpanded),
              content: _buildRules(),
            ),
            const SizedBox(height: 32),
          ],
        ),
      ),
      bottomNavigationBar: _buildBottomNav(),
    );
  }

  // ── AppBar ────────────────────────────────────────────────────────────────
  PreferredSizeWidget _buildAppBar() {
    return AppBar(
      elevation: 0,
      backgroundColor: Colors.white,
      leading: Container(
        margin: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: const Color(0xFFF5F7FB),
          borderRadius: BorderRadius.circular(8),
        ),
        child: IconButton(
          icon: const Icon(Icons.arrow_back, color: Colors.black87, size: 22),
          onPressed: () => Navigator.of(context).maybePop(),
        ),
      ),
      title: const Text(
        'Physics Lab',
        style: TextStyle(
          color: _blue,
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

  // ── Game Preview Section ──────────────────────────────────────────────────
  Widget _buildGamePreview() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.symmetric(vertical: 36, horizontal: 24),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(25),
        gradient: const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFF1A1035), Color(0xFF2F1B6B), Color(0xFF1C5ED6)],
        ),
      ),
      child: Column(
        children: [
          // Animated game icon
          ScaleTransition(
            scale: _pulseAnim,
            child: Container(
              width: 90,
              height: 90,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                color: Colors.white.withValues(alpha: 0.12),
                border: Border.all(
                  color: Colors.white.withValues(alpha: 0.3),
                  width: 2,
                ),
                boxShadow: [
                  BoxShadow(
                    color: _purple.withValues(alpha: 0.5),
                    blurRadius: 24,
                    spreadRadius: 4,
                  ),
                ],
              ),
              child: const Icon(
                Icons.sports_esports,
                color: Colors.white,
                size: 44,
              ),
            ),
          ),
          const SizedBox(height: 18),
          const Text(
            'Vector Quest',
            style: TextStyle(
              fontSize: 26,
              fontWeight: FontWeight.w800,
              color: Colors.white,
              letterSpacing: 0.5,
            ),
          ),
          const SizedBox(height: 6),
          Text(
            'Master vectors & forces through gameplay',
            textAlign: TextAlign.center,
            style: TextStyle(
              fontSize: 13,
              color: Colors.white.withValues(alpha: 0.75),
              fontFamily: 'Poppins',
            ),
          ),
          const SizedBox(height: 18),
          // Star rating row
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Icon(Icons.star, color: Color(0xFFFBBF24), size: 22),
              const Icon(Icons.star, color: Color(0xFFFBBF24), size: 22),
              const Icon(Icons.star, color: Color(0xFFFBBF24), size: 22),
              const Icon(Icons.star_half, color: Color(0xFFFBBF24), size: 22),
              const Icon(Icons.star_border, color: Color(0xFFFBBF24), size: 22),
              const SizedBox(width: 8),
              Text(
                '4.5 / 5',
                style: TextStyle(
                  color: Colors.white.withValues(alpha: 0.85),
                  fontSize: 13,
                  fontWeight: FontWeight.w600,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  // ── Info chips ────────────────────────────────────────────────────────────
  Widget _buildInfoChips() {
    return Wrap(
      spacing: 10,
      runSpacing: 8,
      children: [
        _chip(Icons.school_outlined, 'Grade 10–11', _blue),
        _chip(Icons.timer_outlined, '10–20 mins', _purple),
        _chip(Icons.bar_chart, 'Beginner–Adv.', _green),
        _chip(Icons.bolt, 'Physics Vectors', _orange),
      ],
    );
  }

  Widget _chip(IconData icon, String label, Color color) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: color.withValues(alpha: 0.3)),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 14, color: color),
          const SizedBox(width: 5),
          Text(
            label,
            style: TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.w600,
              color: color,
            ),
          ),
        ],
      ),
    );
  }

  // ── Action buttons ────────────────────────────────────────────────────────
  Widget _buildActionButtons() {
    return Row(
      children: [
        Expanded(
          child: Container(
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(24),
              boxShadow: [
                BoxShadow(
                  color: _blue.withValues(alpha: 0.35),
                  blurRadius: 14,
                  offset: const Offset(0, 4),
                ),
              ],
            ),
            child: ElevatedButton.icon(
              onPressed: () {
                // TODO: Navigate to actual game play screen
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Starting Vector Quest…')),
                );
              },
              icon: const Icon(Icons.play_arrow, size: 22),
              label: const Text(
                'Play Now',
                style: TextStyle(fontWeight: FontWeight.w700, fontSize: 15),
              ),
              style: ElevatedButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 14),
                backgroundColor: _blue,
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
              // TODO: Navigate to demo/tutorial level
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Loading tutorial level…')),
              );
            },
            icon: const Icon(Icons.school_outlined, size: 20),
            label: const Text(
              'Try Tutorial',
              style: TextStyle(fontWeight: FontWeight.w700, fontSize: 15),
            ),
            style: OutlinedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 14),
              side: const BorderSide(color: _blue, width: 1.5),
              foregroundColor: _blue,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(24),
              ),
            ),
          ),
        ),
      ],
    );
  }

  // ── Expandable section wrapper ────────────────────────────────────────────
  Widget _buildExpandableSection({
    required IconData icon,
    required String title,
    required bool isExpanded,
    required VoidCallback onToggle,
    required Widget content,
  }) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.07),
            blurRadius: 12,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        children: [
          GestureDetector(
            onTap: onToggle,
            behavior: HitTestBehavior.opaque,
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Row(
                children: [
                  Icon(icon, color: _blue, size: 24),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Text(
                      title,
                      style: const TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.w600,
                        color: Colors.black87,
                        fontFamily: 'Poppins',
                      ),
                    ),
                  ),
                  Icon(
                    isExpanded ? Icons.expand_less : Icons.expand_more,
                    color: Colors.grey,
                  ),
                ],
              ),
            ),
          ),
          if (isExpanded)
            Padding(
              padding:
                  const EdgeInsets.symmetric(horizontal: 16).copyWith(bottom: 16),
              child: content,
            ),
        ],
      ),
    );
  }

  // ── How to Play content ───────────────────────────────────────────────────
  Widget _buildHowToPlay() {
    return Column(
      children: _howToPlaySteps.map((step) {
        return Padding(
          padding: const EdgeInsets.only(bottom: 16),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                width: 40,
                height: 40,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: (step['color'] as Color).withValues(alpha: 0.12),
                  border: Border.all(
                    color: step['color'] as Color,
                    width: 2,
                  ),
                ),
                child: Center(
                  child: Icon(
                    step['icon'] as IconData,
                    color: step['color'] as Color,
                    size: 18,
                  ),
                ),
              ),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      '${step['number']}. ${step['title']}',
                      style: const TextStyle(
                        fontSize: 14,
                        fontWeight: FontWeight.w600,
                        color: Colors.black87,
                        fontFamily: 'Poppins',
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      step['description'] as String,
                      style: const TextStyle(
                        fontSize: 13,
                        color: Color(0xFF666666),
                        height: 1.5,
                        fontFamily: 'Poppins',
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        );
      }).toList(),
    );
  }

  // ── Controls content ──────────────────────────────────────────────────────
  Widget _buildControls() {
    return GridView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 2,
        mainAxisExtent: 72, // Using a fixed height instead of childAspectRatio to prevent overflow
        crossAxisSpacing: 12,
        mainAxisSpacing: 12,
      ),
      itemCount: _controls.length,
      itemBuilder: (context, index) {
        final c = _controls[index];
        return Container(
          padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
          decoration: BoxDecoration(
            color: const Color(0xFFF5F7FB),
            borderRadius: BorderRadius.circular(12),
          ),
          child: Row(
            children: [
              Container(
                width: 36,
                height: 36,
                decoration: BoxDecoration(
                  color: _blue.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Icon(c['icon'] as IconData, color: _blue, size: 18),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      c['label'] as String,
                      style: const TextStyle(
                        fontWeight: FontWeight.w700,
                        fontSize: 12,
                        color: Colors.black87,
                      ),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    Text(
                      c['desc'] as String,
                      style: const TextStyle(
                        fontSize: 10,
                        color: Color(0xFF888888),
                      ),
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ],
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  // ── Rules content ─────────────────────────────────────────────────────────
  Widget _buildRules() {
    return Column(
      children: _rules.asMap().entries.map((entry) {
        final i = entry.key;
        final rule = entry.value;
        return Padding(
          padding: const EdgeInsets.only(bottom: 10),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                width: 24,
                height: 24,
                decoration: BoxDecoration(
                  color: _orange.withValues(alpha: 0.12),
                  shape: BoxShape.circle,
                ),
                child: Center(
                  child: Text(
                    '${i + 1}',
                    style: const TextStyle(
                      color: _orange,
                      fontSize: 12,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Text(
                  rule,
                  style: const TextStyle(
                    fontSize: 13,
                    color: Color(0xFF444444),
                    height: 1.5,
                    fontFamily: 'Poppins',
                  ),
                ),
              ),
            ],
          ),
        );
      }).toList(),
    );
  }

  // ── Bottom Nav ────────────────────────────────────────────────────────────
  Widget _buildBottomNav() {
    return BottomNavigationBar(
      currentIndex: _selectedBottomNavIndex,
      onTap: (index) {
        setState(() => _selectedBottomNavIndex = index);
        switch (index) {
          case 0:
            Navigator.of(context)
                .pushNamedAndRemoveUntil('/home', (r) => false);
            break;
          case 1:
            Navigator.of(context).pushNamed('/lesson-list');
            break;
          case 2:
            Navigator.of(context).pushNamed('/practical-home');
            break;
          case 3:
            Navigator.of(context).pushNamed('/profile');
            break;
        }
      },
      type: BottomNavigationBarType.fixed,
      backgroundColor: Colors.white,
      selectedItemColor: _blue,
      unselectedItemColor: Colors.grey,
      elevation: 8,
      items: const [
        BottomNavigationBarItem(icon: Icon(Icons.home_outlined), activeIcon: Icon(Icons.home), label: 'Home'),
        BottomNavigationBarItem(
            icon: Icon(Icons.menu_book_outlined), activeIcon: Icon(Icons.menu_book), label: 'Lessons'),
        BottomNavigationBarItem(
            icon: Icon(Icons.science_outlined), activeIcon: Icon(Icons.science), label: 'Labs'),
        BottomNavigationBarItem(icon: Icon(Icons.person), label: 'Profile'),
      ],
    );
  }
}
