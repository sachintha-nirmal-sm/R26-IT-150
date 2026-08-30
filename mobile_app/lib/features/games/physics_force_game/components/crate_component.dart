import 'dart:math' as math;

import 'package:flame/components.dart';
import 'package:flame/game.dart' show FlameGame;
import 'package:flutter/painting.dart';

import '../game/level_config.dart';

// ---------------------------------------------------------------------------
// CrateComponent — v2
// ---------------------------------------------------------------------------

/// The pushable crate.
///
/// Physics:
///   velocity += dragForce/mass * dt   (applied externally each frame)
///   velocity -= frictionDecel * normalized(velocity) * dt  (always)
///
/// Position is in raw screen coordinates (no CameraComponent).
class CrateComponent extends PositionComponent {
  CrateComponent({
    required this.config,
    required Vector2 startPosition,
    required this.onStartedMoving,
    required this.onEnteredFrictionZone,
  }) : super(
          position: startPosition,
          size: Vector2(54, 54),
          anchor: Anchor.center,
          priority: 8,
        );

  final LevelConfig config;
  final VoidCallback onStartedMoving;
  final void Function(SurfaceType surface) onEnteredFrictionZone;

  // ── Physics ────────────────────────────────────────────────────────────
  Vector2 velocity = Vector2.zero();
  double currentFrictionDecel = SurfaceType.normal.frictionDecel;

  /// The current applied Force vector set each frame by [PhysicsForceGame].
  /// Components:
  ///   direction = normalised drag direction
  ///   length    = F magnitude  (so longer arrow = stronger push)
  ///
  /// This is exposed so [render] can draw the green F = ma arrow
  /// directly on the crate without needing a separate component.
  Vector2 appliedForceVec = Vector2.zero();

  SurfaceType _currentSurface = SurfaceType.normal;
  bool _hasMovedOnce = false;
  bool _lastSurfaceTriggered = false;

  // ── Visuals ────────────────────────────────────────────────────────────
  double _animTime = 0.0;

  /// Pulsing green ring drawn around the crate before first touch.
  bool showPulse = true;

  @override
  Future<void> onLoad() async {
    currentFrictionDecel = config.defaultSurface.frictionDecel;
    _currentSurface = config.defaultSurface;
  }

  @override
  void update(double dt) {
    super.update(dt);
    _animTime += dt;

    if (velocity.length < 0.8) {
      velocity = Vector2.zero();
      return;
    }

    if (!_hasMovedOnce && velocity.length > 5.0) {
      _hasMovedOnce = true;
      showPulse = false;
      onStartedMoving();
    }

    // Apply friction.
    final decel = velocity.normalized() * currentFrictionDecel * dt;
    if (decel.length >= velocity.length) {
      velocity = Vector2.zero();
    } else {
      velocity -= decel;
    }

    position += velocity * dt;

    // Clamp to screen bounds via parent.
    double maxW = 380, maxH = 680;
    final p = parent;
    if (p is FlameGame) {
      maxW = p.size.x - 27;
      maxH = p.size.y - 27;
    }
    position.x = position.x.clamp(27.0, maxW);
    position.y = position.y.clamp(27.0, maxH);

    _updateFrictionZone();
  }

  void _updateFrictionZone() {
    SurfaceType detected = config.defaultSurface;
    for (final zone in config.frictionZones) {
      final zoneRect = Rect.fromLTWH(
        zone.position.x, zone.position.y, zone.size.x, zone.size.y,
      );
      if (zoneRect.contains(Offset(position.x, position.y))) {
        detected = zone.surfaceType;
        break;
      }
    }
    if (detected != _currentSurface) {
      _currentSurface = detected;
      currentFrictionDecel = detected.frictionDecel;
      if (!_lastSurfaceTriggered || detected == SurfaceType.rough) {
        _lastSurfaceTriggered = true;
        onEnteredFrictionZone(detected);
      }
    }
  }

  @override
  void render(Canvas canvas) {
    final w = size.x;
    final h = size.y;
    final cx = w / 2;
    final cy = h / 2;

    // ── Pulse glow ring ────────────────────────────────────────────────────
    if (showPulse) {
      final pulse = (math.sin(_animTime * 3) + 1) / 2;
      final pulseR = 32.0 + 6.0 * pulse;
      canvas.drawCircle(
        Offset(cx, cy),
        pulseR,
        Paint()
          ..color = const Color(0xFF00FF94).withAlpha((80 + 60 * pulse).toInt())
          ..style = PaintingStyle.stroke
          ..strokeWidth = 2.5
          ..maskFilter = MaskFilter.blur(BlurStyle.normal, 4 + 4 * pulse),
      );
    }

    // ── Body ───────────────────────────────────────────────────────────────
    final bodyRect = Rect.fromLTWH(0, 0, w, h);
    canvas.drawRRect(
      RRect.fromRectAndRadius(bodyRect, const Radius.circular(6)),
      Paint()
        ..shader = const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFFC8922A), Color(0xFF8B5E1A)],
        ).createShader(bodyRect),
    );

    // ── Wood grain ─────────────────────────────────────────────────────────
    final grain = Paint()
      ..color = const Color(0xFF7A4F14).withAlpha(100)
      ..strokeWidth = 1.2;
    for (double y = 8; y < h; y += 10) {
      canvas.drawLine(Offset(4, y), Offset(w - 4, y), grain);
    }

    // ── Metal corners ──────────────────────────────────────────────────────
    final metal = Paint()
      ..color = const Color(0xFFB0BEC5)
      ..strokeWidth = 2.5
      ..style = PaintingStyle.stroke;
    const cs = 9.0;
    canvas.drawPath(Path()..moveTo(0, cs)..lineTo(0, 0)..lineTo(cs, 0), metal);
    canvas.drawPath(Path()..moveTo(w - cs, 0)..lineTo(w, 0)..lineTo(w, cs), metal);
    canvas.drawPath(Path()..moveTo(0, h - cs)..lineTo(0, h)..lineTo(cs, h), metal);
    canvas.drawPath(Path()..moveTo(w - cs, h)..lineTo(w, h)..lineTo(w, h - cs), metal);

    // ── X mark ────────────────────────────────────────────────────────────
    final xp = Paint()
      ..color = const Color(0xFF4A3200).withAlpha(70)
      ..strokeWidth = 2.0;
    canvas.drawLine(Offset(w * .25, h * .25), Offset(w * .75, h * .75), xp);
    canvas.drawLine(Offset(w * .75, h * .25), Offset(w * .25, h * .75), xp);

    // ── Speed glow ─────────────────────────────────────────────────────────
    final spd = velocity.length;
    if (spd > 20) {
      final alpha = ((spd - 20) / 250).clamp(0.0, 0.55);
      canvas.drawRRect(
        RRect.fromRectAndRadius(
          Rect.fromLTWH(-5, -5, w + 10, h + 10),
          const Radius.circular(10),
        ),
        Paint()
          ..color = const Color(0xFFFF9800).withAlpha((alpha * 255).toInt())
          ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 8),
      );
    }

    // ── Green F = ma Force Vector Arrow ───────────────────────────────────
    //
    // Draws a bright green arrow from the crate centre to show:
    //   • Direction  = direction of the applied Force (F)
    //   • Length     ∝ F magnitude  (short = small force, long = large force)
    //
    // This makes F = ma tangible:
    //   longer drag → bigger F → longer arrow → faster acceleration (a = F/m)
    final forceLen = appliedForceVec.length;
    if (forceLen > 2.0) {
      // Scale arrow: max force → 80 px arrow. Clamp so it stays readable.
      final arrowLen = (forceLen / config.maxForceMagnitude * 80).clamp(8.0, 80.0);
      final dir = appliedForceVec.normalized();

      final tailX = cx + dir.x * 4;   // start just outside crate centre
      final tailY = cy + dir.y * 4;
      final tipX  = cx + dir.x * (4 + arrowLen);
      final tipY  = cy + dir.y * (4 + arrowLen);

      // Glow outline (wide, translucent).
      canvas.drawLine(
        Offset(tailX, tailY),
        Offset(tipX, tipY),
        Paint()
          ..color = const Color(0xFF00FF94).withAlpha(60)
          ..strokeWidth = 8
          ..strokeCap = StrokeCap.round
          ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 4),
      );

      // Arrow shaft (bright green).
      canvas.drawLine(
        Offset(tailX, tailY),
        Offset(tipX, tipY),
        Paint()
          ..color = const Color(0xFF00FF94)
          ..strokeWidth = 2.5
          ..strokeCap = StrokeCap.round,
      );

      // Arrowhead triangle at the tip.
      final angle = math.atan2(dir.y, dir.x);
      const hs = 9.0; // half-span of arrowhead
      final arrowHead = Path()
        ..moveTo(
          tipX + math.cos(angle) * 6,
          tipY + math.sin(angle) * 6,
        )
        ..lineTo(
          tipX + math.cos(angle - 2.4) * hs,
          tipY + math.sin(angle - 2.4) * hs,
        )
        ..lineTo(
          tipX + math.cos(angle + 2.4) * hs,
          tipY + math.sin(angle + 2.4) * hs,
        )
        ..close();
      canvas.drawPath(
        arrowHead,
        Paint()..color = const Color(0xFF00FF94),
      );

      // "F" label at the tip of the arrow.
      final ftp = TextPainter(
        text: const TextSpan(
          text: 'F',
          style: TextStyle(
            color: Color(0xFF00FF94),
            fontSize: 11,
            fontWeight: FontWeight.w900,
          ),
        ),
        textDirection: TextDirection.ltr,
      )..layout();
      ftp.paint(
        canvas,
        Offset(tipX + dir.x * 8 - ftp.width / 2, tipY + dir.y * 8 - ftp.height / 2),
      );
    }
  }
}
