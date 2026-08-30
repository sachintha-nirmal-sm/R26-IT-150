import 'package:flame/game.dart';
import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../game/game_state.dart';
import '../game/physics_force_game.dart';
import '../overlays/hud_overlay.dart';
import '../overlays/level_complete_overlay.dart';
import '../overlays/quiz_dialog_overlay.dart';
import '../overlays/theory_popup_overlay.dart';

// ---------------------------------------------------------------------------
// PhysicsGameWrapper
// ---------------------------------------------------------------------------

/// The Flutter widget that hosts the [PhysicsForceGame] via [GameWidget].
///
/// Responsibilities:
/// - Creates the [GameStateController] and passes it to the game.
/// - Registers all named overlays in [GameWidget.overlayBuilderMap].
/// - Wraps the game in a Scaffold with an app-bar and safe-area padding.
///
/// Navigate here via: `Navigator.pushNamed(context, '/force-game')`
class PhysicsGameWrapper extends StatefulWidget {
  const PhysicsGameWrapper({super.key});

  @override
  State<PhysicsGameWrapper> createState() => _PhysicsGameWrapperState();
}

class _PhysicsGameWrapperState extends State<PhysicsGameWrapper> {
  late final GameStateController _stateController;
  late final PhysicsForceGame _game;

  @override
  void initState() {
    super.initState();
    _stateController = GameStateController();
    _game = PhysicsForceGame(stateController: _stateController);
  }

  @override
  void dispose() {
    _stateController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.black,
      appBar: _buildAppBar(context),
      body: SafeArea(
        child: GameWidget<PhysicsForceGame>(
          game: _game,
          // ── Named overlay builders ──────────────────────────────────────
          overlayBuilderMap: {
            PhysicsForceGame.overlayHud: (context, game) =>
                HudOverlay(game: game),
            PhysicsForceGame.overlayTheory: (context, game) =>
                TheoryPopupOverlay(game: game),
            PhysicsForceGame.overlayQuiz: (context, game) =>
                QuizDialogOverlay(game: game),
            PhysicsForceGame.overlayLevelComplete: (context, game) =>
                LevelCompleteOverlay(game: game),
          },
          // ── Loading screen ────────────────────────────────────────────
          loadingBuilder: (context) => _buildLoadingScreen(),
        ),
      ),
    );
  }

  // ── AppBar ──────────────────────────────────────────────────────────────
  PreferredSizeWidget _buildAppBar(BuildContext context) {
    return AppBar(
      backgroundColor: const Color(0xFF0D1B2A),
      elevation: 0,
      leading: IconButton(
        icon: const Icon(Icons.arrow_back_ios_new_rounded,
            color: Colors.white70, size: 20),
        onPressed: () => Navigator.of(context).maybePop(),
      ),
      title: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Container(
            width: 28,
            height: 28,
            decoration: const BoxDecoration(
              shape: BoxShape.circle,
              gradient: LinearGradient(
                colors: [Color(0xFF3A7BFF), Color(0xFF1A3CBA)],
              ),
            ),
            child: const Icon(Icons.science, color: Colors.white, size: 16),
          ),
          const SizedBox(width: 8),
          Text(
            'Force Lab',
            style: GoogleFonts.poppins(
              fontSize: 18,
              fontWeight: FontWeight.w700,
              color: Colors.white,
            ),
          ),
        ],
      ),
      centerTitle: true,
      actions: [
        // Info/instructions button.
        IconButton(
          icon: const Icon(Icons.help_outline_rounded,
              color: Colors.white60, size: 22),
          onPressed: () => _showInstructionsSheet(context),
        ),
      ],
    );
  }

  // ── Loading screen ──────────────────────────────────────────────────────
  Widget _buildLoadingScreen() {
    return Container(
      color: const Color(0xFF0D1B2A),
      child: Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const CircularProgressIndicator(
              color: Color(0xFF3A7BFF),
              strokeWidth: 2,
            ),
            const SizedBox(height: 20),
            Text(
              'Loading Force Lab…',
              style: GoogleFonts.poppins(
                color: Colors.white60,
                fontSize: 14,
              ),
            ),
          ],
        ),
      ),
    );
  }

  // ── Instructions bottom sheet ───────────────────────────────────────────
  void _showInstructionsSheet(BuildContext context) {
    showModalBottomSheet(
      context: context,
      backgroundColor: const Color(0xFF0F1E35),
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      builder: (_) => Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              '🎮 How to Play',
              style: GoogleFonts.poppins(
                fontSize: 20,
                fontWeight: FontWeight.w700,
                color: Colors.white,
              ),
            ),
            const SizedBox(height: 16),
            ...[
              ('👆 Tap & Hold', 'Touch near the crate and hold to apply force'),
              ('🎯 Goal', 'Push the crate onto the glowing green target'),
              ('📚 Theory', 'Theory popups pause the game — read and continue'),
              ('❓ Quiz', 'Answer correctly to unlock the next level'),
              ('🧊 Friction', 'Different surfaces = different friction forces'),
            ].map((e) => Padding(
                  padding: const EdgeInsets.only(bottom: 12),
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(e.$1,
                          style: GoogleFonts.poppins(
                              fontWeight: FontWeight.w600,
                              color: const Color(0xFF90CAF9),
                              fontSize: 13)),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Text(e.$2,
                            style: GoogleFonts.poppins(
                                color: Colors.white70, fontSize: 13)),
                      ),
                    ],
                  ),
                )),
            const SizedBox(height: 8),
          ],
        ),
      ),
    );
  }
}
