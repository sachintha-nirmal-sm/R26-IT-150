import 'dart:math' as math;

import 'package:flame/components.dart';
import 'package:flutter/material.dart' show Colors;
import 'package:flutter/painting.dart';

// ---------------------------------------------------------------------------
// TargetZoneComponent — v2
// ---------------------------------------------------------------------------

/// Animated glowing target ring. Overlap is checked manually each frame
/// by calling [checkCrateOverlap] from the game's update loop (no collision
/// system needed, avoids world/hitbox issues).
class TargetZoneComponent extends PositionComponent {
  TargetZoneComponent({
    required Vector2 center,
    required this.onCrateReached,
  }) : super(
          position: center,
          size: Vector2(70, 70),
          anchor: Anchor.center,
          priority: 2,
        );

  final VoidCallback onCrateReached;

  double _animTime = 0.0;
  bool _triggered = false;

  /// Call this every frame with the crate's centre position.
  /// Fires [onCrateReached] once when the crate is close enough.
  void checkCrateOverlap(Vector2 crateCenter) {
    if (_triggered) return;
    final dist = (crateCenter - position).length;
    if (dist < 34) {
      _triggered = true;
      onCrateReached();
    }
  }

  @override
  void update(double dt) {
    super.update(dt);
    _animTime += dt;
  }

  @override
  void render(Canvas canvas) {
    final cx = size.x / 2;
    final cy = size.y / 2;
    final pulse = (math.sin(_animTime * 2.5) + 1) / 2;

    // ── Outer pulsing glow ────────────────────────────────────────────────
    final glowR = 36.0 + 5.0 * pulse;
    canvas.drawCircle(
      Offset(cx, cy),
      glowR,
      Paint()
        ..shader = RadialGradient(
          colors: [
            const Color(0xFF00FF94).withAlpha((70 * pulse).toInt()),
            const Color(0x0000FF94),
          ],
        ).createShader(Rect.fromCircle(center: Offset(cx, cy), radius: glowR)),
    );

    // ── Rotating dashed ring ──────────────────────────────────────────────
    final dashPaint = Paint()
      ..color = Color.lerp(
        const Color(0xFF00FF94),
        Colors.white,
        pulse * 0.35,
      )!
      ..style = PaintingStyle.stroke
      ..strokeWidth = 2.8
      ..strokeCap = StrokeCap.round;

    const dashCount = 12;
    const r = 30.0;
    const dashArc = (2 * math.pi / dashCount) * 0.6;
    for (int i = 0; i < dashCount; i++) {
      final startAngle =
          (2 * math.pi / dashCount) * i + _animTime * 0.9;
      canvas.drawArc(
        Rect.fromCircle(center: Offset(cx, cy), radius: r),
        startAngle,
        dashArc,
        false,
        dashPaint,
      );
    }

    // ── Inner star icon ────────────────────────────────────────────────────
    _drawStar(canvas, Offset(cx, cy), 11, 5, pulse);

    // ── "TARGET" label ────────────────────────────────────────────────────
    final labelColor = Color.lerp(
      const Color(0xFF00FF94), Colors.white, pulse * 0.4)!;
    final tp = TextPainter(
      text: TextSpan(
        text: '★ TARGET',
        style: TextStyle(
          color: labelColor,
          fontSize: 10,
          fontWeight: FontWeight.w800,
          letterSpacing: 1.5,
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    tp.paint(canvas, Offset(cx - tp.width / 2, cy + 20));
  }

  void _drawStar(Canvas canvas, Offset center, double outer, int points, double pulse) {
    final inner = outer * 0.45;
    final path = Path();
    final alpha = (0.7 + 0.3 * pulse).clamp(0.0, 1.0);
    final paint = Paint()
      ..color = const Color(0xFF00FF94).withAlpha((alpha * 255).toInt())
      ..style = PaintingStyle.fill;

    for (int i = 0; i < points * 2; i++) {
      final angle = (math.pi / points) * i - math.pi / 2;
      final r = i.isEven ? outer : inner;
      final x = center.dx + r * math.cos(angle);
      final y = center.dy + r * math.sin(angle);
      if (i == 0) {
        path.moveTo(x, y);
      } else {
        path.lineTo(x, y);
      }
    }
    path.close();
    canvas.drawPath(path, paint);
  }
}
