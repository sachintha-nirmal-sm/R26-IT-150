import 'package:flutter/material.dart';
import 'search_data.dart';

class PhysicsLabHomePage extends StatefulWidget {
  const PhysicsLabHomePage({super.key});

  @override
  State<PhysicsLabHomePage> createState() => _PhysicsLabHomePageState();
}

class _PhysicsLabHomePageState extends State<PhysicsLabHomePage> {
  int _selectedIndex = 0;
  static const Color _bg = Color(0xFFF4F6FB);
  static const Color _primaryBlue = Color(0xFF2196F3);
  static const Color _bodyText = Color(0xFF1A1A2E);
  static const Color _subtitleText = Color(0xFF6B7280);
  static const Color _navInactive = Color(0xFFAAAAAA);

  final TextEditingController _searchCtrl = TextEditingController();
  final FocusNode _searchFocus = FocusNode();
  bool _isSearching = false;
  List<SearchItem> _results = [];
  String _grade = 'Grade 10';

  @override
  void initState() {
    super.initState();
    _searchCtrl.addListener(_onSearchChanged);
    _searchFocus.addListener(() {
      if (_searchFocus.hasFocus) setState(() => _isSearching = true);
    });
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    final args = ModalRoute.of(context)?.settings.arguments;
    if (args is Map) {
      setState(() => _grade = args['grade'] ?? 'Grade 10');
    }
  }

  void _onSearchChanged() {
    setState(() {
      _results = searchItems(_searchCtrl.text, _grade);
    });
  }

  void _closeSearch() {
    _searchCtrl.clear();
    _searchFocus.unfocus();
    setState(() { _isSearching = false; _results = []; });
  }

  List<String> get _keywords => gradeKeywords[_grade] ?? [];

  @override
  void dispose() {
    _searchCtrl.dispose();
    _searchFocus.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: _bg,
      appBar: _appBar(),
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
                  _searchBarWidget(),
                  const SizedBox(height: 10),
                  _keywordChips(),
                  const SizedBox(height: 16),
                  _progressCard(),
                  const SizedBox(height: 16),
                  _continueButton(),
                  const SizedBox(height: 24),
                  _sectionTitle('Recommended for you'),
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
            if (_isSearching) _searchOverlay(),
          ],
        ),
      ),
      bottomNavigationBar: _bottomNav(),
    );
  }

  // ── AppBar ────────────────────────────────────────────────────────────────
  AppBar _appBar() => AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        titleSpacing: 16,
        title: Row(children: [
          const Icon(Icons.science, color: _primaryBlue, size: 26),
          const SizedBox(width: 8),
          const Text('Physics Lab',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.w800, color: _bodyText)),
          const SizedBox(width: 8),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
            decoration: BoxDecoration(
              color: _primaryBlue.withValues(alpha: 0.12),
              borderRadius: BorderRadius.circular(20),
            ),
            child: Text(_grade,
                style: const TextStyle(fontSize: 11, color: _primaryBlue, fontWeight: FontWeight.w700)),
          ),
        ]),
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16),
            child: CircleAvatar(
              radius: 18,
              backgroundColor: const Color(0xFFCCCCCC),
              child: const Icon(Icons.person, color: Colors.white, size: 22),
            ),
          ),
        ],
      );

  // ── Greeting ──────────────────────────────────────────────────────────────
  Widget _greetingCard() => _card(child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Hello, Alex',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.w800, color: _bodyText)),
          const SizedBox(height: 4),
          Text('You are on $_grade · Let\'s keep learning!',
              style: const TextStyle(fontSize: 13, color: _subtitleText)),
        ],
      ));

  // ── Search bar (static) ───────────────────────────────────────────────────
  Widget _searchBarWidget() => GestureDetector(
        onTap: () {
          setState(() => _isSearching = true);
          Future.delayed(const Duration(milliseconds: 50), () => _searchFocus.requestFocus());
        },
        child: Container(
          height: 50,
          decoration: BoxDecoration(
            color: const Color(0xFFEEEFF4),
            borderRadius: BorderRadius.circular(30),
          ),
          padding: const EdgeInsets.symmetric(horizontal: 16),
          child: Row(children: [
            const Icon(Icons.search, color: _subtitleText, size: 20),
            const SizedBox(width: 10),
            const Expanded(
              child: Text('Search lessons, quizzes, labs...',
                  style: TextStyle(fontSize: 14, color: _subtitleText)),
            ),
            const Icon(Icons.mic_none, color: _subtitleText, size: 20),
          ]),
        ),
      );

  // ── Keyword chips ─────────────────────────────────────────────────────────
  Widget _keywordChips() => SingleChildScrollView(
        scrollDirection: Axis.horizontal,
        child: Row(
          children: _keywords.map((kw) {
            return GestureDetector(
              onTap: () {
                setState(() => _isSearching = true);
                _searchCtrl.text = kw;
                _onSearchChanged();
                Future.delayed(const Duration(milliseconds: 50), () => _searchFocus.requestFocus());
              },
              child: Container(
                margin: const EdgeInsets.only(right: 8),
                padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(20),
                  border: Border.all(color: const Color(0xFFDDE0EA)),
                  boxShadow: const [BoxShadow(color: Colors.black12, blurRadius: 4, offset: Offset(0, 1))],
                ),
                child: Text(kw,
                    style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w500, color: _bodyText)),
              ),
            );
          }).toList(),
        ),
      );

  // ── Progress card ─────────────────────────────────────────────────────────
  Widget _progressCard() => _card(child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
            const Text('YOUR PROGRESS',
                style: TextStyle(fontSize: 11, fontWeight: FontWeight.w800,
                    color: _primaryBlue, letterSpacing: 1.2)),
            const Icon(Icons.bar_chart, color: _subtitleText, size: 20),
          ]),
          const SizedBox(height: 10),
          const Text('12 Completed Lessons',
              style: TextStyle(fontSize: 16, fontWeight: FontWeight.w700, color: _bodyText)),
          const SizedBox(height: 10),
          Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
            const Text('Current: Linear Motion',
                style: TextStyle(fontSize: 13, color: _subtitleText)),
            const Text('60%',
                style: TextStyle(fontSize: 13, fontWeight: FontWeight.w700, color: _primaryBlue)),
          ]),
          const SizedBox(height: 8),
          ClipRRect(
            borderRadius: BorderRadius.circular(6),
            child: const LinearProgressIndicator(
              value: 0.60, minHeight: 6,
              backgroundColor: Color(0xFFDDE3F8),
              valueColor: AlwaysStoppedAnimation<Color>(_primaryBlue),
            ),
          ),
        ],
      ));

  // ── Continue button ───────────────────────────────────────────────────────
  Widget _continueButton() => SizedBox(
        width: double.infinity, height: 52,
        child: ElevatedButton.icon(
          onPressed: () => Navigator.of(context).pushNamed('/lesson-list'),
          icon: const Icon(Icons.play_arrow, size: 20, color: Colors.white),
          label: const Text('Continue Learning',
              style: TextStyle(fontSize: 16, fontWeight: FontWeight.w700, color: Colors.white)),
          style: ElevatedButton.styleFrom(
            backgroundColor: _primaryBlue, elevation: 0,
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
          ),
        ),
      );

  // ── Recommended ───────────────────────────────────────────────────────────
  Widget _sectionTitle(String t) => Text(t,
      style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w800, color: _bodyText));

  Widget _recommendedRow() => Row(children: [
        Expanded(child: _recCard(const Color(0xFFFDE8D8), Icons.local_fire_department,
            const Color(0xFFEA580C), 'Thermodynamics Basics', '15 mins • Quiz')),
        const SizedBox(width: 12),
        Expanded(child: _recCard(const Color(0xFFEDE9FE), Icons.bolt,
            const Color(0xFF7C3AED), 'Circuit Fundamentals', '22 mins • Lab')),
      ]);

  Widget _recCard(Color bg, IconData icon, Color iconColor, String title, String sub) =>
      Container(
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(
          color: Colors.white, borderRadius: BorderRadius.circular(12),
          boxShadow: const [BoxShadow(color: Colors.black12, blurRadius: 8, offset: Offset(0, 2))],
        ),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Container(
            width: 44, height: 44,
            decoration: BoxDecoration(color: bg, borderRadius: BorderRadius.circular(10)),
            child: Icon(icon, color: iconColor, size: 26),
          ),
          const SizedBox(height: 12),
          Text(title,
              style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w700,
                  color: _bodyText, height: 1.3)),
          const SizedBox(height: 6),
          Text(sub, style: const TextStyle(fontSize: 12, color: _subtitleText)),
        ]),
      );

  // ── Virtual labs banner ───────────────────────────────────────────────────
  Widget _virtualLabsBanner() => Container(
        height: 160, width: double.infinity,
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(16),
          gradient: const LinearGradient(
            begin: Alignment.topRight, end: Alignment.bottomLeft,
            colors: [Color(0xFF64B5F6), Color(0xFF2196F3), Color(0xFF1976D2)],
          ),
          boxShadow: const [BoxShadow(color: Colors.black12, blurRadius: 8, offset: Offset(0, 2))],
        ),
        child: Stack(children: [
          Positioned(top: -20, right: -20,
              child: Container(width: 120, height: 120,
                  decoration: BoxDecoration(shape: BoxShape.circle,
                      color: Colors.white.withValues(alpha: 0.07)))),
          Positioned(top: 20, right: 50,
              child: Container(width: 70, height: 70,
                  decoration: BoxDecoration(shape: BoxShape.circle,
                      color: Colors.white.withValues(alpha: 0.07)))),
          Padding(
            padding: const EdgeInsets.all(20),
            child: Column(crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisAlignment: MainAxisAlignment.end,
                children: const [
                  Text('Explore Virtual Labs',
                      style: TextStyle(fontSize: 17, fontWeight: FontWeight.w800, color: Colors.white)),
                  SizedBox(height: 4),
                  Text('Interactive simulations for your grade',
                      style: TextStyle(fontSize: 13, color: Colors.white70)),
                ]),
          ),
        ]),
      );

  // ── Search overlay ────────────────────────────────────────────────────────
  Widget _searchOverlay() => Positioned.fill(
        child: GestureDetector(
          onTap: _closeSearch,
          child: Container(
            color: const Color(0xFF1A1A1A).withValues(alpha: 0.96),
            child: Column(children: [
              Padding(
                padding: const EdgeInsets.fromLTRB(16, 12, 16, 8),
                child: _searchBarActive(),
              ),
              Expanded(
                child: _searchCtrl.text.trim().isEmpty
                    ? _searchHintView()
                    : _results.isEmpty
                        ? _noResultsView()
                        : _resultsList(),
              ),
            ]),
          ),
        ),
      );

  Widget _searchBarActive() => Container(
        height: 52,
        decoration: BoxDecoration(
          color: const Color(0xFF2C2C2C), borderRadius: BorderRadius.circular(30),
        ),
        padding: const EdgeInsets.symmetric(horizontal: 16),
        child: Row(children: [
          const Icon(Icons.search, color: Colors.white54, size: 20),
          const SizedBox(width: 10),
          Expanded(
            child: TextField(
              controller: _searchCtrl,
              focusNode: _searchFocus,
              style: const TextStyle(color: Colors.white, fontSize: 15),
              cursorColor: _primaryBlue,
              decoration: const InputDecoration(
                border: InputBorder.none,
                hintText: 'Search lessons, quizzes, labs...',
                hintStyle: TextStyle(color: Colors.white38, fontSize: 14),
              ),
            ),
          ),
          GestureDetector(
            onTap: _closeSearch,
            child: const Icon(Icons.close, color: Colors.white54, size: 20),
          ),
        ]),
      );

  Widget _searchHintView() => Center(
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          const Icon(Icons.search, color: Colors.white24, size: 52),
          const SizedBox(height: 12),
          Text('Search $_grade content...',
              style: const TextStyle(color: Colors.white38, fontSize: 14)),
        ]),
      );

  Widget _noResultsView() => Center(
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          const Icon(Icons.search_off, color: Colors.white24, size: 52),
          const SizedBox(height: 12),
          Text('No results for "${_searchCtrl.text}"',
              style: const TextStyle(color: Colors.white38, fontSize: 14)),
        ]),
      );

  Widget _resultsList() {
    final Map<String, List<SearchItem>> groups = {};
    for (final item in _results) {
      groups.putIfAbsent(_groupKey(item.type), () => []).add(item);
    }
    final order = ['LESSONS', 'LABS', 'QUIZZES', 'GAMES', 'OTHERS'];
    final keys = order.where((k) => groups.containsKey(k)).toList();

    // Flatten to widgets
    final List<Widget> widgets = [];
    for (final key in keys) {
      widgets.add(_sectionHeader(key));
      for (final item in groups[key]!) {
        widgets.add(_resultTile(item));
      }
    }

    return GestureDetector(
      onTap: () {},
      child: ListView(
        padding: const EdgeInsets.fromLTRB(16, 4, 16, 16),
        children: widgets,
      ),
    );
  }

  String _groupKey(String type) {
    switch (type) {
      case 'Lesson': return 'LESSONS';
      case 'Lab':    return 'LABS';
      case 'Quiz':   return 'QUIZZES';
      case 'Game':   return 'GAMES';
      default:       return 'OTHERS';
    }
  }

  Widget _sectionHeader(String label) => Padding(
        padding: const EdgeInsets.only(top: 16, bottom: 6),
        child: Text(label,
            style: const TextStyle(color: Colors.white54, fontSize: 11,
                fontWeight: FontWeight.w700, letterSpacing: 1.4)),
      );

  Widget _resultTile(SearchItem item) {
    // badge colours
    Color bg; Color fg;
    switch (item.type) {
      case 'Lesson': bg = const Color(0xFF1E3A5F); fg = const Color(0xFF64B5F6); break;
      case 'Lab':    bg = const Color(0xFF2E1A4A); fg = const Color(0xFFCE93D8); break;
      case 'Quiz':   bg = const Color(0xFF3B2A1A); fg = const Color(0xFFFFB74D); break;
      case 'Game':   bg = const Color(0xFF1A3B1A); fg = const Color(0xFF81C784); break;
      default:       bg = const Color(0xFF2A2A3A); fg = const Color(0xFF90CAF9); break;
    }
    // icon per type
    IconData icon;
    switch (item.type) {
      case 'Lesson': icon = Icons.menu_book; break;
      case 'Lab':    icon = Icons.science; break;
      case 'Quiz':   icon = Icons.quiz; break;
      case 'Game':   icon = Icons.sports_esports; break;
      default:       icon = Icons.folder_open; break;
    }

    return Container(
      margin: const EdgeInsets.only(bottom: 8),
      decoration: BoxDecoration(
        color: const Color(0xFF2A2A2A), borderRadius: BorderRadius.circular(12),
      ),
      child: ListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
        leading: Container(
          width: 44, height: 44,
          decoration: BoxDecoration(color: bg, borderRadius: BorderRadius.circular(10)),
          child: Icon(icon, color: fg, size: 22),
        ),
        title: Text(item.title,
            style: const TextStyle(color: Colors.white, fontSize: 14, fontWeight: FontWeight.w600)),
        subtitle: Padding(
          padding: const EdgeInsets.only(top: 2),
          child: Text('${item.path} · ${item.duration}',
              style: const TextStyle(color: Colors.white38, fontSize: 11)),
        ),
        trailing: Container(
          padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
          decoration: BoxDecoration(color: bg, borderRadius: BorderRadius.circular(20)),
          child: Text(item.type, style: TextStyle(color: fg, fontSize: 11, fontWeight: FontWeight.w600)),
        ),
      ),
    );
  }

  // ── Bottom nav ────────────────────────────────────────────────────────────
  Widget _bottomNav() => Container(
        decoration: const BoxDecoration(
          color: Colors.white,
          boxShadow: [BoxShadow(color: Colors.black12, blurRadius: 8, offset: Offset(0, -2))],
        ),
        child: BottomNavigationBar(
          type: BottomNavigationBarType.fixed,
          currentIndex: _selectedIndex,
          onTap: (i) => setState(() => _selectedIndex = i),
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

  // ── Card wrapper ──────────────────────────────────────────────────────────
  Widget _card({required Widget child}) => Container(
        width: double.infinity,
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: Colors.white, borderRadius: BorderRadius.circular(12),
          boxShadow: const [BoxShadow(color: Colors.black12, blurRadius: 8, offset: Offset(0, 2))],
        ),
        child: child,
      );
}
