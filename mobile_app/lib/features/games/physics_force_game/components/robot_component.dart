import 'dart:math' as math;

import 'package:flame/components.dart';
import 'package:flutter/painting.dart';

// ---------------------------------------------------------------------------
// RobotComponent — v2 (screen-space, no CameraComponent)
// ---------------------------------------------------------------------------

/// A canvas-drawn top-down space robot that smoothly follows the drag point.
class RobotComponent extends PositionComponent {
  RobotComponent({required Vector2 startPosition})
      : super(
          position: startPosition,
          size: Vector2(44, 44),
          anchor: Anchor.center,
          priority: 10,
        );

  Vector2 _targetPosition = Vector2.zero();
  double _animTime = 0.0;

  @override
  Future<void> onLoad() async {
    _targetPosition = position.clone();
  }

  void moveTo(Vector2 destination) {
    _targetPosition = destination.clone();
  }

  @override
  void update(double dt) {
    super.update(dt);
    _animTime += dt;
    final diff = _targetPosition - position;
    if (diff.length > 2.0) {
      position += diff * (dt * 5.0);
    }
  }

  @override
  void render(Canvas canvas) {
    final cx = size.x / 2;
    final cy = size.y / 2;
    final pulse = (math.sin(_animTime * 4) + 1) / 2;

    // Body.
    final bodyRect = Rect.fromCenter(center: Offset(cx, cy + 4), width: 28, height: 22);
    canvas.drawRRect(
      RRect.fromRectAndRadius(bodyRect, const Radius.circular(6)),
      Paint()
        ..shader = const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFF3A7BFF), Color(0xFF1A3CBA)],
        ).createShader(bodyRect),
    );

    // Head.
    final headRect = Rect.fromCenter(center: Offset(cx, cy - 10), width: 20, height: 14);
    canvas.drawRRect(
      RRect.fromRectAndRadius(headRect, const Radius.circular(4)),
      Paint()
        ..shader = const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFF4A90FF), Color(0xFF2060D0)],
        ).createShader(headRect),
    );

    // Visor.
    canvas.drawRRect(
      RRect.fromRectAndRadius(
        Rect.fromCenter(center: Offset(cx, cy - 10), width: 14, height: 4),
        const Radius.circular(2),
      ),
      Paint()
        ..color = Color.lerp(
          const Color(0xFF00E5FF),
          const Color(0xFF80FFFF),
          pulse,
        )!,
    );

    // Antenna.
    canvas.drawLine(
      Offset(cx, cy - 17),
      Offset(cx, cy - 23),
      Paint()
        ..color = const Color(0xFF9DC8FF)
        ..strokeWidth = 1.5,
    );
    canvas.drawCircle(
      Offset(cx, cy - 24),
      3.0,
      Paint()
        ..color = Color.lerp(const Color(0xFFFF6B35), const Color(0xFFFFD700), pulse)!
            .withAlpha(230)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 3),
    );
    canvas.drawCircle(
      Offset(cx, cy - 24),
      2.0,
      Paint()..color = const Color(0xFFFFFFFF),
    );

    // Arms.
    final armPaint = Paint()..color = const Color(0xFF2060D0);
    for (final dx in [-17.0, 17.0]) {
      canvas.drawRRect(
        RRect.fromRectAndRadius(
          Rect.fromCenter(center: Offset(cx + dx, cy + 4), width: 6, height: 10),
          const Radius.circular(3),
        ),
        armPaint,
      );
    }

    // Thruster glow.
    final thrusterAlpha = (0.7 * pulse * 255).toInt();
    canvas.drawCircle(
      Offset(cx, cy + 16),
      12,
      Paint()
        ..shader = RadialGradient(
          colors: [
            const Color(0xFF00E5FF).withAlpha(thrusterAlpha),
            const Color(0x0000E5FF),
          ],
        ).createShader(
            Rect.fromCircle(center: Offset(cx, cy + 16), radius: 12)),
    );
  }
}
