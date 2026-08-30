import 'dart:math' as math;

import 'package:flame/components.dart';
import 'package:flutter/painting.dart';

// ---------------------------------------------------------------------------
// ForceArrowComponent
// ---------------------------------------------------------------------------

/// An animated vector arrow visualising the applied force direction/magnitude.
/// Color shifts gold → red with increasing force. Magnitude label shown as %.
class ForceArrowComponent extends PositionComponent {
  ForceArrowComponent()
      : super(
          position: Vector2.zero(),
          size: Vector2(200, 200),
          priority: 20,
        );

  bool _visible = false;
  Vector2 _origin = Vector2.zero();
  Vector2 _direction = Vector2(0, -1);
  double _magnitude = 0.0;
  double _animTime = 0.0;

  static const double _maxMag = 400.0;
  static const double _minLength = 20.0;
  static const double _maxLength = 90.0;
  static const double _headSize = 14.0;
  static const double _shaftWidth = 5.0;

  void show({
    required Vector2 origin,
    required Vector2 direction,
    required double magnitude,
  }) {
    _visible = true;
    _origin = origin;
    _direction = direction;
    _magnitude = magnitude;
    position = _origin - Vector2(100, 100);
  }

  void hide() => _visible = false;

  @override
  void update(double dt) {
    super.update(dt);
    _animTime += dt;
  }

  @override
  void render(Canvas canvas) {
    if (!_visible || _magnitude < 5.0) return;

    final t = (_magnitude / _maxMag).clamp(0.0, 1.0);
    final pulse = (math.sin(_animTime * 8) + 1) / 2;

    final arrowColor = Color.lerp(
      const Color(0xFFFFD700),
      const Color(0xFFFF2200),
      t,
    )!;

    final arrowLength = _minLength + (_maxLength - _minLength) * t;
    const local = Offset(100, 100);
    final tip = Offset(
      local.dx + _direction.x * arrowLength,
      local.dy + _direction.y * arrowLength,
    );

    // Glow.
    final glowAlpha = ((0.15 + 0.15 * pulse) * 255).toInt();
    _drawArrow(
      canvas,
      local,
      tip,
      _headSize + 4,
      _shaftWidth + 4,
      Paint()
        ..color = arrowColor.withAlpha(glowAlpha)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 10),
    );

    // Main arrow.
    _drawArrow(
      canvas,
      local,
      tip,
      _headSize,
      _shaftWidth,
      Paint()..color = arrowColor,
    );

    // Magnitude label.
    final magPercent = (t * 100).toInt();
    final tp = TextPainter(
      text: TextSpan(
        text: 'F: $magPercent%',
        style: TextStyle(
          color: arrowColor,
          fontSize: 10,
          fontWeight: FontWeight.w700,
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    final perpX = -_direction.y;
    final perpY = _direction.x;
    tp.paint(
      canvas,
      tip + Offset(perpX * 14 - tp.width / 2, perpY * 14 - tp.height / 2),
    );
  }

  void _drawArrow(
    Canvas canvas,
    Offset from,
    Offset to,
    double headSize,
    double shaftWidth,
    Paint paint,
  ) {
    final angle = math.atan2(to.dy - from.dy, to.dx - from.dx);
    final shaftEnd = to - Offset(
      math.cos(angle) * headSize,
      math.sin(angle) * headSize,
    );

    canvas.drawLine(
      from,
      shaftEnd,
      paint
        ..style = PaintingStyle.stroke
        ..strokeWidth = shaftWidth
        ..strokeCap = StrokeCap.round,
    );

    final headPath = Path()
      ..moveTo(to.dx, to.dy)
      ..lineTo(
        to.dx - headSize * math.cos(angle - math.pi / 6),
        to.dy - headSize * math.sin(angle - math.pi / 6),
      )
      ..lineTo(
        to.dx - headSize * math.cos(angle + math.pi / 6),
        to.dy - headSize * math.sin(angle + math.pi / 6),
      )
      ..close();
    canvas.drawPath(headPath, paint..style = PaintingStyle.fill);
  }
}
