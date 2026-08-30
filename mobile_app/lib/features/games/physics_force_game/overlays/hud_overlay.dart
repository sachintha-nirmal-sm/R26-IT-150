import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../game/level_config.dart';
import '../game/physics_force_game.dart';

// ---------------------------------------------------------------------------
// HudOverlay — always-on in-game HUD
// ---------------------------------------------------------------------------

/// Renders as a transparent Flutter widget on top of the Flame canvas.
/// Shows:
///   - Level badge (top-left)
///   - Topic chip (top-right)
///   - Step-by-step instruction card (bottom)
class HudOverlay extends StatelessWidget {
  const HudOverlay({super.key, required this.game});

  final PhysicsForceGame game;

  @override
  Widget build(BuildContext context) {
    final cfg = game.currentConfig;
    return SafeArea(
      child: Stack(
        children: [
          // ── Top-left level badge ──────────────────────────────────────
          Positioned(
            top: 8,
            left: 12,
            child: _LevelBadge(levelNumber: cfg.levelNumber),
          ),

          // ── Top-right topic chip ───────────────────────────────────────
          Positioned(
            top: 8,
            right: 12,
            child: _TopicChip(topic: cfg.levelName),
          ),

          // ── Bottom instruction card ───────────────────────────────────
          Positioned(
            bottom: 0,
            left: 0,
            right: 0,
            child: _InstructionCard(config: cfg),
          ),
        ],
      ),
    );
  }
}

// ---------------------------------------------------------------------------
// _LevelBadge
// ---------------------------------------------------------------------------
class _LevelBadge extends StatelessWidget {
  const _LevelBadge({required this.levelNumber});
  final int levelNumber;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [Color(0xFF3A7BFF), Color(0xFF1A3CBA)],
        ),
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            color: const Color(0xFF3A7BFF).withAlpha(100),
            blurRadius: 8,
            spreadRadius: 1,
          ),
        ],
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          const Icon(Icons.videogame_asset_rounded,
              color: Colors.white, size: 14),
          const SizedBox(width: 5),
          Text(
            'LEVEL $levelNumber',
            style: GoogleFonts.poppins(
              fontSize: 12,
              fontWeight: FontWeight.w800,
              color: Colors.white,
              letterSpacing: 1.2,
            ),
          ),
        ],
      ),
    );
  }
}

// ---------------------------------------------------------------------------
// _TopicChip
// ---------------------------------------------------------------------------
class _TopicChip extends StatelessWidget {
  const _TopicChip({required this.topic});
  final String topic;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: const Color(0xFF0F1E35).withAlpha(220),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: const Color(0xFF3A7BFF).withAlpha(80)),
      ),
      child: Text(
        '📚 $topic',
        style: GoogleFonts.poppins(
          fontSize: 11,
          fontWeight: FontWeight.w600,
          color: Colors.white70,
        ),
      ),
    );
  }
}

// ---------------------------------------------------------------------------
// _InstructionCard
// ---------------------------------------------------------------------------
class _InstructionCard extends StatelessWidget {
  const _InstructionCard({required this.config});
  final LevelConfig config;

  @override
  Widget build(BuildContext context) {
    // Level-1 and Level-2 get a richer multi-step breakdown; other levels get single line.
    if (config.levelNumber == 1) {
      return const _Level1Instructions();
    }
    if (config.levelNumber == 2) {
      return const _Level2Instructions();
    }
    return _SimpleInstruction(text: config.instructionText);
  }
}

class _Level1Instructions extends StatelessWidget {
  const _Level1Instructions();

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.fromLTRB(12, 0, 12, 12),
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
      decoration: BoxDecoration(
        color: const Color(0xFF0F1E35).withAlpha(235),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: const Color(0xFF3A7BFF).withAlpha(60)),
        boxShadow: const [
          BoxShadow(color: Color(0x44000000), blurRadius: 12, offset: Offset(0, -3)),
        ],
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            '🤖  How to Push the Crate',
            style: GoogleFonts.poppins(
              fontSize: 13,
              fontWeight: FontWeight.w700,
              color: Colors.white,
            ),
          ),
          const SizedBox(height: 10),
          ...[
            (
              '1️⃣',
              'Touch ON the crate',
              'The green ring shows where to tap',
              const Color(0xFF00FF94),
            ),
            (
              '2️⃣',
              'Drag your finger UP',
              'Your drag direction = force direction',
              const Color(0xFF3A7BFF),
            ),
            (
              '3️⃣',
              'Hold until crate reaches ★',
              'Longer drag = stronger push',
              const Color(0xFFFFD700),
            ),
          ].map(
            (e) => Padding(
              padding: const EdgeInsets.only(bottom: 6),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(e.$1, style: const TextStyle(fontSize: 16)),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          e.$2,
                          style: GoogleFonts.poppins(
                            fontSize: 12,
                            fontWeight: FontWeight.w700,
                            color: e.$4,
                          ),
                        ),
                        Text(
                          e.$3,
                          style: GoogleFonts.poppins(
                            fontSize: 10,
                            color: Colors.white60,
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}

// ---------------------------------------------------------------------------
// _Level2Instructions
// ---------------------------------------------------------------------------
class _Level2Instructions extends StatelessWidget {
  const _Level2Instructions();

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.fromLTRB(12, 0, 12, 12),
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
      decoration: BoxDecoration(
        color: const Color(0xFF0F1E35).withAlpha(235),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: const Color(0xFF3A7BFF).withAlpha(60)),
        boxShadow: const [
          BoxShadow(color: Color(0x44000000), blurRadius: 12, offset: Offset(0, -3)),
        ],
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            '⚡  Effects of Force',
            style: GoogleFonts.poppins(
              fontSize: 13,
              fontWeight: FontWeight.w700,
              color: Colors.white,
            ),
          ),
          const SizedBox(height: 10),
          ...[
            (
              '1️⃣',
              'Crate is sliding — tap & drag LEFT',
              'Apply opposite force to stop it',
              const Color(0xFFFF6B35),
            ),
            (
              '2️⃣',
              'Crate stopped? Now push it UP-RIGHT',
              'Drag toward the ★ target',
              const Color(0xFF3A7BFF),
            ),
            (
              '3️⃣',
              'Hold drag until crate reaches ★',
              'Longer drag = stronger force = faster',
              const Color(0xFFFFD700),
            ),
          ].map(
            (e) => Padding(
              padding: const EdgeInsets.only(bottom: 6),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(e.$1, style: const TextStyle(fontSize: 16)),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          e.$2,
                          style: GoogleFonts.poppins(
                            fontSize: 12,
                            fontWeight: FontWeight.w700,
                            color: e.$4,
                          ),
                        ),
                        Text(
                          e.$3,
                          style: GoogleFonts.poppins(
                            fontSize: 10,
                            color: Colors.white60,
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _SimpleInstruction extends StatelessWidget {
  const _SimpleInstruction({required this.text});
  final String text;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.fromLTRB(12, 0, 12, 12),
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: const Color(0xFF0F1E35).withAlpha(230),
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: const Color(0xFF3A7BFF).withAlpha(60)),
      ),
      child: Row(
        children: [
          const Icon(Icons.info_outline_rounded,
              color: Color(0xFF3A7BFF), size: 18),
          const SizedBox(width: 10),
          Expanded(
            child: Text(
              text,
              style: GoogleFonts.poppins(
                fontSize: 12,
                color: Colors.white70,
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
