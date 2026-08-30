import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../game/physics_force_game.dart';

// ---------------------------------------------------------------------------
// TheoryPopupOverlay
// ---------------------------------------------------------------------------

/// Glassmorphism theory popup that pauses the game and teaches a concept.
///
/// Used in:
/// - Level 1: Force definition (triggered when crate first moves)
/// - Level 3: Friction definition (triggered when entering rough zone)
///
/// The popup slides in from the bottom with a spring animation.
class TheoryPopupOverlay extends StatefulWidget {
  const TheoryPopupOverlay({
    super.key,
    required this.game,
  });

  final PhysicsForceGame game;

  @override
  State<TheoryPopupOverlay> createState() => _TheoryPopupOverlayState();
}

class _TheoryPopupOverlayState extends State<TheoryPopupOverlay>
    with SingleTickerProviderStateMixin {
  late final AnimationController _ctrl;
  late final Animation<Offset> _slideAnim;
  late final Animation<double> _fadeAnim;

  @override
  void initState() {
    super.initState();
    _ctrl = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 450),
    );
    _slideAnim = Tween<Offset>(
      begin: const Offset(0, 1),
      end: Offset.zero,
    ).animate(CurvedAnimation(parent: _ctrl, curve: Curves.easeOutBack));
    _fadeAnim = CurvedAnimation(parent: _ctrl, curve: Curves.easeIn);
    _ctrl.forward();
  }

  @override
  void dispose() {
    _ctrl.dispose();
    super.dispose();
  }

  String get _theoryText {
    final level = widget.game.currentLevelNumber;
    if (level == 3) {
      return '💡 Note: The force that opposes motion when two surfaces are in contact is called \'Frictional Force\'';
    }
    if (level == 2) {
      return '💡 Note: A force can stop a moving object, start a stationary object, or change the direction of a moving object. Force changes motion!';
    }
    // Level 1
    return '💡 Note: A push or pull applied to move a stationary object or stop a moving object is called a \'Force\'';
  }

  /// True for Level 1 — show the F=ma equation block.
  /// True for Level 2 — show the direction-change concept block.
  bool get _showFormula => widget.game.currentLevelNumber == 1;
  bool get _showEffectsBlock => widget.game.currentLevelNumber == 2;

  String get _levelLabel {
    final level = widget.game.currentLevelNumber;
    return 'Level $level — Physics Insight';
  }

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.transparent,
      child: FadeTransition(
        opacity: _fadeAnim,
        child: Container(
          color: Colors.black.withValues(alpha: 0.55),
          alignment: Alignment.bottomCenter,
          child: SlideTransition(
            position: _slideAnim,
            child: Container(
              margin: const EdgeInsets.all(20),
              padding: const EdgeInsets.all(24),
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(24),
                gradient: const LinearGradient(
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                  colors: [Color(0xFF1A2540), Color(0xFF0D1B30)],
                ),
                border: Border.all(
                  color: const Color(0xFF3A7BFF).withValues(alpha: 0.6),
                  width: 1.5,
                ),
                boxShadow: [
                  BoxShadow(
                    color: const Color(0xFF3A7BFF).withValues(alpha: 0.25),
                    blurRadius: 32,
                    spreadRadius: 4,
                  ),
                ],
              ),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // ── Header ───────────────────────────────────────────────
                  Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.all(10),
                        decoration: BoxDecoration(
                          shape: BoxShape.circle,
                          color: const Color(0xFF3A7BFF).withValues(alpha: 0.2),
                          border: Border.all(
                            color: const Color(0xFF3A7BFF).withValues(alpha: 0.5),
                          ),
                        ),
                        child: const Icon(
                          Icons.lightbulb_outline,
                          color: Color(0xFFFFD700),
                          size: 22,
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Text(
                          _levelLabel,
                          style: GoogleFonts.poppins(
                            fontSize: 13,
                            fontWeight: FontWeight.w600,
                            color: const Color(0xFF90CAF9),
                            letterSpacing: 0.3,
                          ),
                        ),
                      ),
                    ],
                  ),

                  const SizedBox(height: 18),

                  // ── Theory text ───────────────────────────────────────────
                  Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      borderRadius: BorderRadius.circular(14),
                      color: const Color(0xFF3A7BFF).withValues(alpha: 0.08),
                      border: Border.all(
                        color: const Color(0xFF3A7BFF).withValues(alpha: 0.2),
                      ),
                    ),
                    child: Text(
                      _theoryText,
                      style: GoogleFonts.poppins(
                        fontSize: 15,
                        fontWeight: FontWeight.w500,
                        color: Colors.white.withValues(alpha: 0.92),
                        height: 1.7,
                      ),
                    ),
                  ),

                  // ── F = ma Equation Block (Level 1 only) ────────────────
                  if (_showFormula) ...[
                    const SizedBox(height: 16),
                    Container(
                      width: double.infinity,
                      padding: const EdgeInsets.symmetric(
                          horizontal: 20, vertical: 18),
                      decoration: BoxDecoration(
                        borderRadius: BorderRadius.circular(16),
                        gradient: const LinearGradient(
                          begin: Alignment.topLeft,
                          end: Alignment.bottomRight,
                          colors: [
                            Color(0xFF003D1F),
                            Color(0xFF00261A),
                          ],
                        ),
                        border: Border.all(
                          color: const Color(0xFF00FF94).withValues(alpha: 0.45),
                          width: 1.5,
                        ),
                        boxShadow: [
                          BoxShadow(
                            color: const Color(0xFF00FF94).withValues(alpha: 0.15),
                            blurRadius: 18,
                            spreadRadius: 2,
                          ),
                        ],
                      ),
                      child: Column(
                        children: [
                          // ── Equation label ───────────────────────────
                          Text(
                            'Equation',
                            style: GoogleFonts.poppins(
                              fontSize: 11,
                              fontWeight: FontWeight.w600,
                              color: const Color(0xFF00FF94).withValues(alpha: 0.7),
                              letterSpacing: 2.0,
                            ),
                          ),
                          const SizedBox(height: 8),
                          // ── F = ma  (large, bold, green) ─────────
                          Text(
                            'F = ma',
                            style: GoogleFonts.jetBrainsMono(
                              fontSize: 36,
                              fontWeight: FontWeight.w900,
                              color: const Color(0xFF00FF94),
                              letterSpacing: 4,
                            ),
                          ),
                          const SizedBox(height: 10),
                          // ── Full form ────────────────────────────────
                          Text(
                            'Force = Mass × Acceleration',
                            style: GoogleFonts.poppins(
                              fontSize: 13,
                              fontWeight: FontWeight.w600,
                              color: Colors.white.withValues(alpha: 0.85),
                            ),
                          ),
                          const SizedBox(height: 14),
                          // ── Symbol breakdown ─────────────────────
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                            children: [
                              const _FormulaSymbol(
                                symbol: 'F',
                                label: 'Force (N)',
                                color: Color(0xFF00FF94),
                              ),
                              Text(
                                '=',
                                style: GoogleFonts.jetBrainsMono(
                                  fontSize: 20,
                                  color: Colors.white54,
                                  fontWeight: FontWeight.w700,
                                ),
                              ),
                              const _FormulaSymbol(
                                symbol: 'm',
                                label: 'Mass (kg)',
                                color: Color(0xFF3A7BFF),
                              ),
                              Text(
                                '×',
                                style: GoogleFonts.jetBrainsMono(
                                  fontSize: 20,
                                  color: Colors.white54,
                                  fontWeight: FontWeight.w700,
                                ),
                              ),
                              const _FormulaSymbol(
                                symbol: 'a',
                                label: 'Accel (m/s²)',
                                color: Color(0xFFFFD700),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                  ],

                  // ── Effects of Force Block (Level 2 only) ───────────────
                  if (_showEffectsBlock) ...[
                    const SizedBox(height: 16),
                    Container(
                      width: double.infinity,
                      padding: const EdgeInsets.symmetric(
                          horizontal: 20, vertical: 18),
                      decoration: BoxDecoration(
                        borderRadius: BorderRadius.circular(16),
                        gradient: const LinearGradient(
                          begin: Alignment.topLeft,
                          end: Alignment.bottomRight,
                          colors: [
                            Color(0xFF1A0040),
                            Color(0xFF0D0028),
                          ],
                        ),
                        border: Border.all(
                          color: const Color(0xFF7C3AED).withValues(alpha: 0.5),
                          width: 1.5,
                        ),
                        boxShadow: [
                          BoxShadow(
                            color: const Color(0xFF7C3AED).withValues(alpha: 0.2),
                            blurRadius: 18,
                            spreadRadius: 2,
                          ),
                        ],
                      ),
                      child: Column(
                        children: [
                          Text(
                            'Force Can…',
                            style: GoogleFonts.poppins(
                              fontSize: 11,
                              fontWeight: FontWeight.w600,
                              color: const Color(0xFFA78BFA).withValues(alpha: 0.8),
                              letterSpacing: 2.0,
                            ),
                          ),
                          const SizedBox(height: 12),
                          ...[  
                            ('🛑', 'Stop', 'a moving object', const Color(0xFFFF6B35)),
                            ('🚀', 'Start', 'a stationary object', const Color(0xFF00FF94)),
                            ('↩️', 'Change', 'direction of motion', const Color(0xFF3A7BFF)),
                          ].map(
                            (e) => Padding(
                              padding: const EdgeInsets.only(bottom: 8),
                              child: Row(
                                children: [
                                  Text(e.$1, style: const TextStyle(fontSize: 20)),
                                  const SizedBox(width: 12),
                                  Text(
                                    e.$2,
                                    style: GoogleFonts.poppins(
                                      fontSize: 15,
                                      fontWeight: FontWeight.w800,
                                      color: e.$4,
                                    ),
                                  ),
                                  const SizedBox(width: 6),
                                  Expanded(
                                    child: Text(
                                      e.$3,
                                      style: GoogleFonts.poppins(
                                        fontSize: 12,
                                        color: Colors.white70,
                                      ),
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],

                  const SizedBox(height: 22),

                  // ── Continue button ───────────────────────────────────────
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: () {
                        _ctrl.reverse().then((_) {
                          widget.game.resumeAfterOverlay();
                        });
                      },
                      style: ElevatedButton.styleFrom(
                        backgroundColor: const Color(0xFF3A7BFF),
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(vertical: 16),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(16),
                        ),
                        elevation: 0,
                      ),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Text(
                            'Continue',
                            style: GoogleFonts.poppins(
                              fontWeight: FontWeight.w700,
                              fontSize: 16,
                            ),
                          ),
                          const SizedBox(width: 8),
                          const Icon(Icons.arrow_forward_rounded, size: 18),
                        ],
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

// ---------------------------------------------------------------------------
// _FormulaSymbol — small colour-coded symbol + label for the F=ma breakdown
// ---------------------------------------------------------------------------

class _FormulaSymbol extends StatelessWidget {
  const _FormulaSymbol({
    required this.symbol,
    required this.label,
    required this.color,
  });

  final String symbol;
  final String label;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          width: 42,
          height: 42,
          decoration: BoxDecoration(
            color: color.withValues(alpha: 0.12),
            borderRadius: BorderRadius.circular(10),
            border: Border.all(color: color.withValues(alpha: 0.5)),
          ),
          alignment: Alignment.center,
          child: Text(
            symbol,
            style: GoogleFonts.jetBrainsMono(
              fontSize: 22,
              fontWeight: FontWeight.w900,
              color: color,
            ),
          ),
        ),
        const SizedBox(height: 5),
        Text(
          label,
          style: GoogleFonts.poppins(
            fontSize: 9,
            color: Colors.white54,
            fontWeight: FontWeight.w500,
          ),
        ),
      ],
    );
  }
}
