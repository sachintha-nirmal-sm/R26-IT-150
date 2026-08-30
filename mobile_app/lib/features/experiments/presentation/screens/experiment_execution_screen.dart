import 'package:flutter/material.dart';
import '../../../../core/api/api_client.dart';
import '../../data/practical.dart';
import '../../data/practicals_repository.dart';
import '../../data/unity_lab_service.dart';
import '../widgets/practical_hero.dart';
import 'experiment_results_screen.dart';
import 'unity_player_screen.dart';

class ExperimentExecutionScreen extends StatefulWidget {
  const ExperimentExecutionScreen({super.key});

  @override
  State<ExperimentExecutionScreen> createState() =>
      _ExperimentExecutionScreenState();
}

class _ExperimentExecutionScreenState extends State<ExperimentExecutionScreen> {
  final _repo = PracticalsRepository();
  int _selectedBottomNavIndex = 1;
  bool _busy = false;
  bool _startedLoad = false;
  String? _error;
  Practical? _practical;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_startedLoad) return;
    final args = ModalRoute.of(context)?.settings.arguments;
    if (args is Practical) {
      _practical = args;
      _startedLoad = true;
      _refresh();
      return;
    }
    _startedLoad = true;
    _error = 'Open this practical from Practical Hub.';
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) setState(() {});
    });
  }

  Future<void> _refresh() async {
    final id = _practical?.id;
    if (id == null) return;
    try {
      final detail = await _repo.fetchById(id);
      if (!mounted) return;
      setState(() {
        _practical = detail;
        _error = null;
      });
    } catch (error) {
      if (!mounted) return;
      setState(() => _error = error.toString());
    }
  }

  Future<void> _runBusy(Future<void> Function() action) async {
    if (_busy) return;
    setState(() => _busy = true);
    try {
      await action();
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(error.toString()), backgroundColor: Colors.red),
      );
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _startDemo() async {
    final practical = _practical;
    if (practical == null) return;
    if (!practical.canStartDemo) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Demo is not available for this practical.')),
      );
      return;
    }
    await _runBusy(() async {
      final session = await _beginSession(practical, demo: true);
      if (!mounted) return;
      final popped = await Navigator.push(
        context,
        MaterialPageRoute(builder: (context) => _playerFor(practical, session)),
      );
      if (!mounted) return;
      await _refresh();
      if (!mounted) return;
      if (popped is UnityPracticalResult) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(
              'Trial complete: ${popped.score}%. Use Start to save a score to your profile.',
            ),
          ),
        );
      }
    });
  }

  Future<void> _startOfficial() async {
    final practical = _practical;
    if (practical == null) return;
    if (!practical.canStartPractical) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('The official practical is not available.')),
      );
      return;
    }
    await _runBusy(() async {
      final session = await _beginSession(practical, demo: false);
      if (!mounted) return;
      final popped = await Navigator.push(
        context,
        MaterialPageRoute(builder: (context) => _playerFor(practical, session)),
      );
      if (!mounted) return;
      await _refresh();
      if (!mounted) return;
      if (popped is UnityPracticalResult) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Saved ${popped.score}% to your profile.')),
        );
      }
    });
  }

  Future<PracticalSession> _beginSession(
    Practical practical, {
    required bool demo,
  }) async {
    try {
      return demo
          ? await _repo.startDemo(practical.id)
          : await _repo.startPractical(practical.id);
    } catch (error) {
      final localIds = LocalPracticals.all.map((item) => item.id).toSet();
      if (localIds.contains(practical.id)) {
        return PracticalSession.local(
          practical: practical,
          mode: demo ? 'demo' : 'practical',
        );
      }
      if (error is ApiException &&
          (error.statusCode == 503 || error.statusCode == 404)) {
        return PracticalSession.local(
          practical: practical,
          mode: demo ? 'demo' : 'practical',
        );
      }
      rethrow;
    }
  }

  Widget _playerFor(Practical practical, PracticalSession session) {
    return UnityPlayerScreen(
      args: PracticalRunArgs(practical: practical, session: session),
    );
  }

  Future<void> _viewResult() async {
    final practical = _practical;
    if (practical == null) return;
    await _runBusy(() async {
      final result = await _repo.fetchOfficialResult(practical.id);
      if (!mounted) return;
      await Navigator.push(
        context,
        MaterialPageRoute(
          builder: (context) => ExperimentResultsScreen(
            score: result.percentage.round(),
            finalDuration: result.durationLabel,
            topicName: practical.title,
          ),
        ),
      );
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      appBar: _buildAppBar(),
      body: Stack(
        children: [
          SingleChildScrollView(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  if (_error != null) ...[
                    Text(_error!, style: const TextStyle(color: Colors.red)),
                    const SizedBox(height: 12),
                    OutlinedButton(onPressed: _refresh, child: const Text('Retry')),
                    const SizedBox(height: 16),
                  ],
                  _buildTopicPreview(),
                  const SizedBox(height: 24),
                  _buildActionButtons(),
                  if (_practical != null) ...[
                    const SizedBox(height: 20),
                    PracticalInstructionsCard(practical: _practical!),
                  ],
                  const SizedBox(height: 32),
                ],
              ),
            ),
          ),
          if (_busy)
            const ColoredBox(
              color: Color(0x66FFFFFF),
              child: Center(child: CircularProgressIndicator()),
            ),
        ],
      ),
      bottomNavigationBar: _buildBottomNavigationBar(),
    );
  }

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
          icon: const Icon(Icons.arrow_back, color: Color.fromARGB(255, 42, 128, 241), size: 24),
          onPressed: () => Navigator.pop(context),
        ),
      ),
      title: Text(
        _practical?.title ?? 'Physics Lab',
        style: const TextStyle(
          color: Color(0xFF2F80ED),
          fontSize: 20,
          fontWeight: FontWeight.w700,
          fontFamily: 'Poppins',
        ),
      ),
      centerTitle: false,
      actions: [
        Container(
          margin: const EdgeInsets.all(12),
          width: 44,
          height: 44,
          decoration: BoxDecoration(
            shape: BoxShape.circle,
            gradient: const LinearGradient(
              colors: [Color(0xFF2F80ED), Color(0xFF1C5ED6)],
              begin: Alignment.topLeft,
              end: Alignment.bottomRight,
            ),
            boxShadow: [
              BoxShadow(
                color: const Color(0xFF2F80ED).withValues(alpha: 0.3),
                blurRadius: 12,
                offset: const Offset(0, 4),
              ),
            ],
          ),
          child: const Center(
            child: Text(
              'A',
              style: TextStyle(
                color: Colors.white,
                fontSize: 18,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildTopicPreview() {
    final practical = _practical;
    if (practical == null) {
      return Container(
        padding: const EdgeInsets.symmetric(vertical: 48, horizontal: 24),
        decoration: BoxDecoration(
          color: const Color(0xFFF0F2F5),
          borderRadius: BorderRadius.circular(25),
        ),
        child: const Center(
          child: Text(
            'Open a practical from Interactive Experiments.',
            style: TextStyle(color: Color(0xFF999999)),
          ),
        ),
      );
    }
    return PracticalHeroCard(practical: practical);
  }

  Widget _buildActionButtons() {
    final practical = _practical;
    final canDemo = practical?.canStartDemo ?? false;
    final canOfficial = practical?.canStartPractical ?? false;
    final canResult = practical?.canViewResult ?? false;

    return Column(
      children: [
        if (practical != null) ...[
          if (practical.completed) ...[
            _CompletedBanner(
              bestScore: practical.bestScore,
              maxScore: practical.maxScore,
            ),
            const SizedBox(height: 12),
          ],
          Align(
            alignment: Alignment.centerLeft,
            child: Text(
              practical.completed
                  ? 'You already completed this practical. You can retry it anytime. Your best official score is kept.'
                  : practical.demoCompleted
                      ? 'Trial complete. Start is timed and counts as the official score.'
                      : 'Trial is practice and does not count. Start is timed, scored /100, and saved to your profile.',
              style: const TextStyle(fontSize: 13, color: Color(0xFF666666)),
            ),
          ),
          const SizedBox(height: 12),
        ],
        if (practical != null)
          PracticalStartTrialBar(
            practical: practical,
            busy: _busy,
            onStart: canOfficial ? _startOfficial : null,
            onTrial: canDemo ? _startDemo : null,
          )
        else
          const SizedBox.shrink(),
        if (canResult) ...[
          const SizedBox(height: 12),
          SizedBox(
            width: double.infinity,
            child: TextButton(
              onPressed: _busy ? null : _viewResult,
              child: const Text('View official result'),
            ),
          ),
        ],
      ],
    );
  }

  Widget _buildBottomNavigationBar() {
    return BottomNavigationBar(
      currentIndex: _selectedBottomNavIndex,
      onTap: (index) {
        setState(() {
          _selectedBottomNavIndex = index;
        });
      },
      type: BottomNavigationBarType.fixed,
      backgroundColor: Colors.white,
      selectedItemColor: const Color(0xFF2F80ED),
      unselectedItemColor: Colors.grey,
      elevation: 8,
      items: const [
        BottomNavigationBarItem(
          icon: Icon(Icons.home),
          label: 'Home',
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.science),
          label: 'Experiment',
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.menu_book),
          label: 'Library',
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.person),
          label: 'Profile',
        ),
      ],
    );
  }
}

class _CompletedBanner extends StatelessWidget {
  const _CompletedBanner({
    required this.bestScore,
    required this.maxScore,
  });

  final int bestScore;
  final int maxScore;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: const Color(0xFFE8F5E9),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: const Color(0xFF81C784)),
      ),
      child: Row(
        children: [
          const Icon(Icons.check_circle, color: Color(0xFF2E7D32)),
          const SizedBox(width: 10),
          Expanded(
            child: Text(
              maxScore > 0
                  ? 'Completed. Best official score: $bestScore / $maxScore'
                  : 'Completed',
              style: const TextStyle(
                fontSize: 13,
                fontWeight: FontWeight.w600,
                color: Color(0xFF2E7D32),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
