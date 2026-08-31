import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../game/physics_force_game.dart';

// ---------------------------------------------------------------------------
// QuizDialogOverlay
// ---------------------------------------------------------------------------

/// MCQ quiz checkpoint overlay for Level 2 — "Effects of Force".
///
/// Shows a multiple-choice question. Wrong answers shake the option and
/// show an error state. Correct answer shows green confirmation and
/// enables the "Next Level" button.
///
/// Only the correct answer ("A Force") unlocks progression.
class QuizDialogOverlay extends StatefulWidget {
  const QuizDialogOverlay({
    super.key,
    required this.game,
  });

  final PhysicsForceGame game;

  @override
  State<QuizDialogOverlay> createState() => _QuizDialogOverlayState();
}

class _QuizDialogOverlayState extends State<QuizDialogOverlay>
    with TickerProviderStateMixin {
  late final AnimationController _enterCtrl;
  late final Animation<double> _scaleFade;

  late final AnimationController _shakeCtrl;
  late final Animation<double> _shakeAnim;

  int? _selectedIndex;
  bool _answered = false;
  bool _isCorrect = false;

  /// Index of the correct answer in [_options].
  static const int _correctIndex = 0;

  final List<String> _options = [
    'A Force',
    'A Mass',
    'Time',
  ];

  @override
  void initState() {
    super.initState();

    _enterCtrl = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 400),
    );
    _scaleFade = CurvedAnimation(parent: _enterCtrl, curve: Curves.easeOutBack);
    _enterCtrl.forward();

    _shakeCtrl = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 400),
    );
    _shakeAnim = Tween<double>(begin: 0, end: 1).animate(_shakeCtrl);
  }

  @override
  void dispose() {
    _enterCtrl.dispose();
    _shakeCtrl.dispose();
    super.dispose();
  }

  void _onOptionSelected(int index) {
    if (_answered) return;

    setState(() {
      _selectedIndex = index;
    });

    if (index == _correctIndex) {
      setState(() {
        _answered = true;
        _isCorrect = true;
      });
    } else {
      // Shake and mark incorrect.
      _shakeCtrl.forward(from: 0).then((_) => _shakeCtrl.reset());
      setState(() {
        _answered = false; // allow retry
      });
    }
  }

  Color _optionColor(int index) {
    if (_selectedIndex != index) return const Color(0xFF1E2D45);
    if (!_answered) return const Color(0xFFB71C1C); // wrong = red
    return const Color(0xFF1B5E20); // correct = green
  }

  Color _optionBorderColor(int index) {
    if (_selectedIndex != index) return const Color(0xFF2D4060);
    if (!_answered) return const Color(0xFFE53935);
    return const Color(0xFF43A047);
  }

  IconData _optionIcon(int index) {
    if (_selectedIndex != index) return Icons.radio_button_unchecked;
    if (!_answered) return Icons.cancel_outlined;
    return Icons.check_circle_outline;
  }

  Color _optionIconColor(int index) {
    if (_selectedIndex != index) return const Color(0xFF5A7BAA);
    if (!_answered) return const Color(0xFFEF5350);
    return const Color(0xFF66BB6A);
  }

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.transparent,
      child: Container(
        color: Colors.black.withValues(alpha: 0.7),
        alignment: Alignment.center,
        child: ScaleTransition(
          scale: _scaleFade,
          child: FadeTransition(
            opacity: _scaleFade,
            child: Container(
              margin: const EdgeInsets.symmetric(horizontal: 20),
              padding: const EdgeInsets.all(24),
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(24),
                gradient: const LinearGradient(
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                  colors: [Color(0xFF0F1E35), Color(0xFF162440)],
                ),
                border: Border.all(
                  color: const Color(0xFF3A7BFF).withValues(alpha: 0.5),
                  width: 1.5,
                ),
                boxShadow: [
                  BoxShadow(
                    color: const Color(0xFF3A7BFF).withValues(alpha: 0.2),
                    blurRadius: 40,
                    spreadRadius: 8,
                  ),
                ],
              ),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  // ── Quiz badge ──────────────────────────────────────────
                  Container(
                    padding: const EdgeInsets.symmetric(
                        horizontal: 14, vertical: 6),
                    decoration: BoxDecoration(
                      borderRadius: BorderRadius.circular(20),
                      color: const Color(0xFF7C3AED).withValues(alpha: 0.25),
                      border: Border.all(
                        color: const Color(0xFF7C3AED).withValues(alpha: 0.5),
                      ),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Icon(Icons.quiz_outlined,
                            color: Color(0xFFA78BFA), size: 14),
                        const SizedBox(width: 6),
                        Text(
                          'Checkpoint Quiz',
                          style: GoogleFonts.poppins(
                            fontSize: 12,
                            fontWeight: FontWeight.w600,
                            color: const Color(0xFFA78BFA),
                          ),
                        ),
                      ],
                    ),
                  ),

                  const SizedBox(height: 18),

                  // ── Question ──────────────────────────────────────────────
                  Text(
                    '❓ What must be applied to change the direction of a moving object?',
                    textAlign: TextAlign.center,
                    style: GoogleFonts.poppins(
                      fontSize: 16,
                      fontWeight: FontWeight.w600,
                      color: Colors.white,
                      height: 1.5,
                    ),
                  ),

                  const SizedBox(height: 20),

                  // ── Options ───────────────────────────────────────────────
                  ..._options.asMap().entries.map((entry) {
                    final idx = entry.key;
                    final text = entry.value;
                    final isSelected = _selectedIndex == idx;

                    Widget optionWidget = GestureDetector(
                      onTap: () => _onOptionSelected(idx),
                      child: AnimatedContainer(
                        duration: const Duration(milliseconds: 200),
                        margin: const EdgeInsets.only(bottom: 10),
                        padding: const EdgeInsets.symmetric(
                            horizontal: 16, vertical: 14),
                        decoration: BoxDecoration(
                          borderRadius: BorderRadius.circular(14),
                          color: _optionColor(idx),
                          border: Border.all(
                            color: _optionBorderColor(idx),
                            width: 1.5,
                          ),
                        ),
                        child: Row(
                          children: [
                            Icon(
                              _optionIcon(idx),
                              color: _optionIconColor(idx),
                              size: 20,
                            ),
                            const SizedBox(width: 12),
                            Expanded(
                              child: Text(
                                text,
                                style: GoogleFonts.poppins(
                                  fontSize: 14,
                                  fontWeight: FontWeight.w500,
                                  color: isSelected
                                      ? Colors.white
                                      : const Color(0xFFCDD5E0),
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    );

                    // Apply shake animation only to incorrectly selected option.
                    if (isSelected && !_answered) {
                      return AnimatedBuilder(
                        animation: _shakeAnim,
                        builder: (_, child) {
                          final offset =
                              (8 * _shakeAnim.value * (1 - _shakeAnim.value) * 4)
                                  .clamp(-8.0, 8.0) *
                                  (1 - _shakeAnim.value > 0.5 ? -1 : 1);
                          return Transform.translate(
                            offset: Offset(offset, 0),
                            child: child,
                          );
                        },
                        child: optionWidget,
                      );
                    }
                    return optionWidget;
                  }),

                  const SizedBox(height: 8),

                  // ── Success message ───────────────────────────────────────
                  if (_isCorrect) ...[
                    Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        borderRadius: BorderRadius.circular(12),
                        color: const Color(0xFF1B5E20).withValues(alpha: 0.3),
                        border: Border.all(
                          color: const Color(0xFF43A047).withValues(alpha: 0.5),
                        ),
                      ),
                      child: Row(
                        children: [
                          const Icon(Icons.check_circle,
                              color: Color(0xFF66BB6A), size: 20),
                          const SizedBox(width: 8),
                          Expanded(
                            child: Text(
                              'Correct! A force is needed to change direction.',
                              style: GoogleFonts.poppins(
                                fontSize: 12,
                                color: const Color(0xFF81C784),
                                height: 1.4,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 14),
                    SizedBox(
                      width: double.infinity,
                      child: ElevatedButton(
                        onPressed: () {
                          _enterCtrl.reverse().then((_) {
                            widget.game.loadNextLevel();
                          });
                        },
                        style: ElevatedButton.styleFrom(
                          backgroundColor: const Color(0xFF43A047),
                          foregroundColor: Colors.white,
                          padding: const EdgeInsets.symmetric(vertical: 16),
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
                            const Icon(Icons.arrow_forward_rounded, size: 18),
                          ],
                        ),
                      ),
                    ),
                  ] else ...[
                    Text(
                      'Select the correct answer to proceed.',
                      style: GoogleFonts.poppins(
                        fontSize: 12,
                        color: const Color(0xFF5A7BAA),
                      ),
                    ),
                  ],
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
