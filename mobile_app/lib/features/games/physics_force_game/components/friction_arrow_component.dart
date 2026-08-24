import 'dart:math' as math;

import 'package:flame/components.dart';
import 'package:flutter/painting.dart';

// ---------------------------------------------------------------------------
// FrictionArrowComponent
// ---------------------------------------------------------------------------

/// A counter-vector arrow drawn opposite to crate velocity — visualises
/// frictional force opposing motion. Used only in Level 3.
///
/// Note: We deliberately rename the custom update method to [setData] to avoid
/// conflicting with [Component.update(double dt)].
class FrictionArrowComponent extends Component {
  FrictionArrowComponent() : super(priority: 19);

  bool isVisible = false;

  Vector2 _crateCenter = Vector2.zero();
  Vector2 _frictionDirection = Vector2.zero();
  double _frictionDecel = 0.0;
  double _animTime = 0.0;

  static const double _maxDecel = 250.0;

  /// Called each frame from [PhysicsForceGame.update] with current crate data.
  void setData({
    required Vector2 crateCenter,
    required Vector2 crateVelocity,
    required double frictionDecel,
  }) {
    _crateCenter = crateCenter;
    _frictionDirection = -(crateVelocity.normalized());
    _frictionDecel = frictionDecel;
  }

  @override
  void update(double dt) {
    super.update(dt);
    _animTime += dt;
  }

  @override
  void renderTree(Canvas canvas) {
    _render(canvas);
  }

  void _render(Canvas canvas) {
    if (!isVisible || _frictionDecel < 1.0) return;

    final t = (_frictionDecel / _maxDecel).clamp(0.0, 1.0);
    final pulse = (math.sin(_animTime * 5) + 1) / 2;

    final frictionColor = Color.lerp(
      const Color(0xFFAB47BC),
      const Color(0xFFE040FB),
      t,
    )!;

    const minLen = 14.0;
    const maxLen = 70.0;
    final arrowLen = minLen + (maxLen - minLen) * t;

    final from = Offset(_crateCenter.x, _crateCenter.y);
    final tip = Offset(
      from.dx + _frictionDirection.x * arrowLen,
      from.dy + _frictionDirection.y * arrowLen,
    );

    // Glow.
    final glowAlpha = ((0.2 + 0.15 * pulse) * 255).toInt();
    _drawArrow(
      canvas,
      from,
      tip,
      16,
      6,
      Paint()
        ..color = frictionColor.withAlpha(glowAlpha)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 8),
    );

    // Main arrow.
    _drawArrow(canvas, from, tip, 12, 4, Paint()..color = frictionColor);

    // "Friction" label.
    final tp = TextPainter(
      text: TextSpan(
        text: '← Friction',
        style: TextStyle(
          color: frictionColor,
          fontSize: 10,
          fontWeight: FontWeight.w700,
          shadows: const [Shadow(blurRadius: 4, color: Color(0x88000000))],
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();

    final perpX = -_frictionDirection.y;
    final perpY = _frictionDirection.x;
    tp.paint(
      canvas,
      Offset(
        tip.dx + perpX * 16 - tp.width / 2,
        tip.dy + perpY * 16 - tp.height / 2,
      ),
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
