import 'dart:math' as math;
import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../game/physics_force_game.dart';

// ---------------------------------------------------------------------------
// LevelCompleteOverlay
// ---------------------------------------------------------------------------

/// Full-screen level-complete celebration shown after a crate reaches target
/// (Levels 1 and 3, since Level 2 uses the quiz instead).
///
/// Features: particle star burst animation, star rating, Next Level / Retry.
class LevelCompleteOverlay extends StatefulWidget {
  const LevelCompleteOverlay({
    super.key,
    required this.game,
  });

  final PhysicsForceGame game;

  @override
  State<LevelCompleteOverlay> createState() => _LevelCompleteOverlayState();
}

class _LevelCompleteOverlayState extends State<LevelCompleteOverlay>
    with TickerProviderStateMixin {
  late final AnimationController _enterCtrl;
  late final AnimationController _starCtrl;
  late final AnimationController _particleCtrl;

  late final Animation<double> _enterScale;
  late final Animation<double> _enterFade;
  late final List<Animation<double>> _starAnims;

  final _random = math.Random();

  @override
  void initState() {
    super.initState();

    _enterCtrl = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 500),
    );
    _enterScale = CurvedAnimation(
      parent: _enterCtrl,
      curve: Curves.easeOutBack,
    );
    _enterFade = CurvedAnimation(parent: _enterCtrl, curve: Curves.easeIn);

    _starCtrl = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1200),
    );
    _starAnims = List.generate(3, (i) {
      return Tween<double>(begin: 0, end: 1).animate(
        CurvedAnimation(
          parent: _starCtrl,
          curve: Interval(i * 0.2, 0.6 + i * 0.15, curve: Curves.elasticOut),
        ),
      );
    });

    _particleCtrl = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 2500),
    )..repeat();

    _enterCtrl.forward().then((_) => _starCtrl.forward());
  }

  @override
  void dispose() {
    _enterCtrl.dispose();
    _starCtrl.dispose();
    _particleCtrl.dispose();
    super.dispose();
  }

  bool get _isLastLevel => widget.game.currentLevelNumber >= 3;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.transparent,
      child: FadeTransition(
        opacity: _enterFade,
        child: Container(
          color: Colors.black.withValues(alpha: 0.7),
          child: Stack(
            children: [
              // ── Particle background ───────────────────────────────────
              AnimatedBuilder(
                animation: _particleCtrl,
                builder: (_, __) {
                  return CustomPaint(
                    painter: _ParticlePainter(
                      progress: _particleCtrl.value,
                      random: _random,
                    ),
                    size: Size.infinite,
                  );
                },
              ),

              // ── Main card ─────────────────────────────────────────────
              Center(
                child: ScaleTransition(
                  scale: _enterScale,
                  child: Container(
                    margin: const EdgeInsets.symmetric(horizontal: 24),
                    padding: const EdgeInsets.all(28),
                    decoration: BoxDecoration(
                      borderRadius: BorderRadius.circular(28),
                      gradient: const LinearGradient(
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                        colors: [Color(0xFF0F1E35), Color(0xFF122040)],
                      ),
                      border: Border.all(
                        color: const Color(0xFFFFD700).withValues(alpha: 0.5),
                        width: 1.5,
                      ),
                      boxShadow: [
                        BoxShadow(
                          color: const Color(0xFFFFD700).withValues(alpha: 0.2),
                          blurRadius: 40,
                          spreadRadius: 4,
                        ),
                      ],
                    ),
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        // Rocket emoji + title.
                        const Text('🚀', style: TextStyle(fontSize: 52)),
                        const SizedBox(height: 10),
                        Text(
                          'Level Complete!',
                          style: GoogleFonts.poppins(
                            fontSize: 26,
                            fontWeight: FontWeight.w800,
                            color: Colors.white,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          'Level ${widget.game.currentLevelNumber} — ${widget.game.currentConfig.levelName}',
                          style: GoogleFonts.poppins(
                            fontSize: 13,
                            color: const Color(0xFF90CAF9),
                            fontWeight: FontWeight.w500,
                          ),
                        ),

                        const SizedBox(height: 22),

                        // ── Star rating ─────────────────────────────────
                        Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: List.generate(3, (i) {
                            return ScaleTransition(
                              scale: _starAnims[i],
                              child: const Padding(
                                padding:
                                    EdgeInsets.symmetric(horizontal: 6),
                                child: Icon(
                                  Icons.star_rounded,
                                  color: Color(0xFFFFD700),
                                  size: 44,
                                  shadows: [
                                    Shadow(
                                      color: Color(0xFFFF9800),
                                      blurRadius: 12,
                                    ),
                                  ],
                                ),
                              ),
                            );
                          }),
                        ),

                        const SizedBox(height: 24),

                        // ── Buttons ──────────────────────────────────────
                        if (!_isLastLevel)
                          SizedBox(
                            width: double.infinity,
                            child: ElevatedButton(
                              onPressed: () {
                                widget.game.loadNextLevel();
                              },
                              style: ElevatedButton.styleFrom(
                                backgroundColor: const Color(0xFF3A7BFF),
                                foregroundColor: Colors.white,
                                padding:
                                    const EdgeInsets.symmetric(vertical: 15),
                                shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(16),
                                ),
                              ),
                              child: Row(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: [
                                  Text(
                                    'Next Level',
                                    style: GoogleFonts.poppins(
                                      fontWeight: FontWeight.w700,
                                      fontSize: 16,
                                    ),
                                  ),
                                  const SizedBox(width: 8),
                                  const Icon(Icons.arrow_forward_rounded,
                                      size: 18),
                                ],
                              ),
                            ),
                          )
                        else
                          _buildGameCompleteBadge(),

                        const SizedBox(height: 10),

                        TextButton(
                          onPressed: () {
                            widget.game.restartCurrentLevel();
                          },
                          child: Text(
                            'Replay Level',
                            style: GoogleFonts.poppins(
                              color: const Color(0xFF5A7BAA),
                              fontSize: 14,
                            ),
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
    );
  }

  Widget _buildGameCompleteBadge() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(16),
        gradient: const LinearGradient(
          colors: [Color(0xFFFFD700), Color(0xFFFFA000)],
        ),
      ),
      child: Column(
        children: [
          Text(
            '🎓 All Levels Complete!',
            style: GoogleFonts.poppins(
              fontWeight: FontWeight.w800,
              fontSize: 18,
              color: const Color(0xFF1A1A00),
            ),
          ),
          const SizedBox(height: 4),
          Text(
            'You\'ve mastered Push, Pull, Effects of Force & Friction!',
            textAlign: TextAlign.center,
            style: GoogleFonts.poppins(
              fontSize: 12,
              color: const Color(0xFF3D2E00),
              height: 1.4,
            ),
          ),
        ],
      ),
    );
  }
}

// ---------------------------------------------------------------------------
// Particle Painter
// ---------------------------------------------------------------------------

class _ParticlePainter extends CustomPainter {
  _ParticlePainter({required this.progress, required this.random});

  final double progress;
  final math.Random random;

  static final List<_Particle> _particles = List.generate(40, (i) {
    final r = math.Random(i * 37 + 13);
    return _Particle(
      x: r.nextDouble(),
      y: r.nextDouble(),
      speed: 0.05 + r.nextDouble() * 0.1,
      size: 3 + r.nextDouble() * 6,
      color: [
        const Color(0xFFFFD700),
        const Color(0xFF3A7BFF),
        const Color(0xFFFF6B35),
        const Color(0xFF00FF94),
        const Color(0xFFE040FB),
      ][r.nextInt(5)],
      phase: r.nextDouble(),
    );
  });

  @override
  void paint(Canvas canvas, Size size) {
    for (final p in _particles) {
      final t = (progress + p.phase) % 1.0;
      final x = p.x * size.width;
      final y = (p.y - t * p.speed * 8) * size.height;
      if (y < -20 || y > size.height + 20) continue;

      final paint = Paint()
        ..color = p.color.withValues(alpha: (1 - t) * 0.8)
        ..style = PaintingStyle.fill;
      canvas.drawCircle(Offset(x, y), p.size * (1 - t * 0.5), paint);
    }
  }

  @override
  bool shouldRepaint(covariant _ParticlePainter old) =>
      old.progress != progress;
}

class _Particle {
  const _Particle({
    required this.x,
    required this.y,
    required this.speed,
    required this.size,
    required this.color,
    required this.phase,
  });

  final double x, y, speed, size, phase;
  final Color color;
}
