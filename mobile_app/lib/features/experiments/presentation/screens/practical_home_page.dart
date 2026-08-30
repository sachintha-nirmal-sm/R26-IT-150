import 'package:flutter/material.dart';

import '../../data/practical.dart';
import '../../data/practical_guide.dart';
import '../../data/practicals_repository.dart';
import '../widgets/practical_hero.dart';

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
    if (title.contains('heat')) {
      return const [LocalPracticals.heatExpansion];
    }
    if (title.contains('appliance') ||
        (title.contains('power') &&
            title.contains('energy') &&
            title.contains('electric'))) {
      return const [LocalPracticals.powerEnergyAppliances];
    }
    if (title.contains('electronics') || title.contains('diode')) {
      return const [LocalPracticals.electronicsDiode];
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
            color: guide.color.withValues(alpha: 0.12),
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
                              color: guide.accent.withValues(alpha: 0.35),
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
