import 'package:flutter/material.dart';

import '../../data/practical.dart';
import '../../data/practicals_repository.dart';

class PracticalHomePage extends StatefulWidget {
  const PracticalHomePage({super.key, this.lessonId, this.lessonTitle});

  final String? lessonId;
  final String? lessonTitle;

  @override
  State<PracticalHomePage> createState() => _PracticalHomePageState();
}

class _PracticalHomePageState extends State<PracticalHomePage> {
  final _repo = PracticalsRepository();
  late Future<List<Practical>> _practicalsFuture;
  String? _lessonId;
  String? _lessonTitle;
  bool _readRouteArgs = false;

  @override
  void initState() {
    super.initState();
    _lessonId = widget.lessonId;
    _lessonTitle = widget.lessonTitle;
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
    }
    _practicalsFuture = _load();
  }

  Future<List<Practical>> _load() async {
    final items = await _repo.fetchActiveForCurrentStudent(lessonId: _lessonId);
    if (items.isNotEmpty) return items;
    final title = (_lessonTitle ?? '').toLowerCase();
    if (title.contains('newton')) {
      return const [LocalPracticals.newtonsLaws];
    }
    if (title.contains('friction')) {
      return const [LocalPracticals.friction];
    }
    if (title.contains('resultant')) {
      return const [LocalPracticals.resultantForce];
    }
    if (title.contains('turning')) {
      return const [LocalPracticals.turningEffect];
    }
    if (title.contains('equilibrium')) {
      return const [LocalPracticals.equilibriumOfForces];
    }
    if (title.contains('wave') && title.contains('application')) {
      return const [LocalPracticals.wavesApplications];
    }
    if (title.contains('geometrical') || title.contains('optic')) {
      return const [LocalPracticals.geometricalOptics];
    }
    if (title.contains('straight') && title.contains('line')) {
      return const [LocalPracticals.motionStraightLine];
    }
    if (title.contains('current') && title.contains('electricity')) {
      return const [LocalPracticals.currentElectricity];
    }
    if (title.contains('density')) return const [LocalPracticals.densityWater];
    if (title.contains('force')) return const [LocalPracticals.forceBasic];
    if (title.contains('work') && title.contains('energy')) {
      return const [LocalPracticals.workEnergyPower];
    }
    if (title.contains('hydrostatic') ||
        title.contains('upthrust') ||
        title.contains('archimedes')) {
      return const [LocalPracticals.hydrostaticPressure];
    }
    if (title.contains('pressure')) return const [LocalPracticals.pressureSolid];
    if (title.contains('reflection') ||
        title.contains('refract') ||
        title.contains('prism')) {
      return const [LocalPracticals.reflectionPrism];
    }
    if (title.contains('lever') || title.contains('simple machine')) {
      return const [LocalPracticals.leverActivity];
    }
    return LocalPracticals.forLesson(_lessonId);
  }

  void _reload() {
    setState(() {
      _practicalsFuture = _load();
    });
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
          style: TextStyle(color: Color(0xFF1A1C1E), fontWeight: FontWeight.bold),
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
                    ? 'Select an experiment to start your virtual lab'
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
    );
  }
}

class _PracticalCard extends StatelessWidget {
  const _PracticalCard({required this.practical, required this.onStart});

  final Practical practical;
  final VoidCallback onStart;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 24),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.05),
            blurRadius: 15,
            offset: const Offset(0, 5),
          ),
        ],
      ),
      child: InkWell(
        onTap: onStart,
        borderRadius: BorderRadius.circular(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Stack(
              children: [
                ClipRRect(
                  borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
                  child: Container(
                    height: 180,
                    width: double.infinity,
                    color: const Color(0xFF2C3E50),
                    child: const Center(
                      child: Icon(Icons.science, color: Colors.white30, size: 60),
                    ),
                  ),
                ),
                Positioned(
                  bottom: 12,
                  right: 12,
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                    decoration: BoxDecoration(
                      color: Colors.black.withValues(alpha: 0.7),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Text(
                      practical.durationLabel,
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 12,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ),
              ],
            ),
            Padding(
              padding: const EdgeInsets.all(16),
              child: Row(
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          practical.title,
                          style: const TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                            color: Color(0xFF1A1C1E),
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          practical.description,
                          style: const TextStyle(fontSize: 13, color: Colors.grey),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(width: 12),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                    decoration: BoxDecoration(
                      color: const Color(0xFF2196F3),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: const Text(
                      'Start',
                      style: TextStyle(
                        color: Colors.white,
                        fontWeight: FontWeight.bold,
                        fontSize: 14,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
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
