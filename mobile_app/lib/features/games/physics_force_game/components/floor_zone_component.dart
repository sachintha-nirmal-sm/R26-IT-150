
import 'package:flame/components.dart';
import 'package:flutter/painting.dart';

import '../game/level_config.dart';

// ---------------------------------------------------------------------------
// FloorZoneComponent — v2
// ---------------------------------------------------------------------------

/// Canvas-drawn surface tile. Purely visual — friction is handled by
/// [CrateComponent._updateFrictionZone].
class FloorZoneComponent extends PositionComponent {
  FloorZoneComponent({required this.data})
      : super(
          position: data.position,
          size: data.size,
          anchor: Anchor.topLeft,
          priority: 0,
        );

  final FrictionZoneData data;
  double _animTime = 0.0;

  @override
  void update(double dt) {
    super.update(dt);
    _animTime += dt;
  }

  @override
  void render(Canvas canvas) {
    switch (data.surfaceType) {
      case SurfaceType.normal:
        _renderNormal(canvas);
      case SurfaceType.rough:
        _renderRough(canvas);
      case SurfaceType.ice:
        _renderIce(canvas);
    }
  }

  void _renderNormal(Canvas canvas) {
    final w = size.x;
    final h = size.y;

    canvas.drawRect(
      Rect.fromLTWH(0, 0, w, h),
      Paint()..color = const Color(0xFF1A2535),
    );

    final gridPaint = Paint()
      ..color = const Color(0xFF253348)
      ..strokeWidth = 1.0;
    for (double x = 0; x <= w; x += 40) {
      canvas.drawLine(Offset(x, 0), Offset(x, h), gridPaint);
    }
    for (double y = 0; y <= h; y += 40) {
      canvas.drawLine(Offset(0, y), Offset(w, y), gridPaint);
    }
  }

  void _renderRough(Canvas canvas) {
    final w = size.x;
    final h = size.y;

    canvas.drawRect(
      Rect.fromLTWH(0, 0, w, h),
      Paint()
        ..shader = const LinearGradient(
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
          colors: [Color(0xFF4A3520), Color(0xFF3A2810)],
        ).createShader(Rect.fromLTWH(0, 0, w, h)),
    );

    final dotPaint = Paint()..color = const Color(0xFF5C4530).withAlpha(160);
    for (double x = 6; x < w; x += 12) {
      for (double y = 6; y < h; y += 12) {
        canvas.drawCircle(Offset(x, y), 2.0, dotPaint);
      }
    }

    final gridPaint = Paint()
      ..color = const Color(0xFF6B5040).withAlpha(100)
      ..strokeWidth = 1.0;
    for (double x = 0; x <= w; x += 36) {
      canvas.drawLine(Offset(x, 0), Offset(x, h), gridPaint);
    }
    for (double y = 0; y <= h; y += 36) {
      canvas.drawLine(Offset(0, y), Offset(w, y), gridPaint);
    }

    _drawLabel(canvas, w, h, 'ROUGH FLOOR  🪨 High Friction', const Color(0xFFFF9800));
  }

  void _renderIce(Canvas canvas) {
    final w = size.x;
    final h = size.y;

    canvas.drawRect(
      Rect.fromLTWH(0, 0, w, h),
      Paint()
        ..shader = const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFFCCEEFF), Color(0xFF88CCEE)],
        ).createShader(Rect.fromLTWH(0, 0, w, h)),
    );

    // Animated shimmer.
    final shimmerPaint = Paint()
      ..color = const Color(0xFFFFFFFF).withAlpha(70)
      ..strokeWidth = 1.5;
    final offset = (_animTime * 30) % 40;
    for (double x = -w + offset; x < w * 2; x += 40) {
      canvas.drawLine(Offset(x, 0), Offset(x + h, h), shimmerPaint);
    }

    // Crack lines.
    final crackPaint = Paint()
      ..color = const Color(0xFF90D5F0).withAlpha(140)
      ..strokeWidth = 0.8;
    canvas.drawLine(const Offset(50, 20), const Offset(130, 90), crackPaint);
    canvas.drawLine(const Offset(200, 10), const Offset(260, 70), crackPaint);
    canvas.drawLine(const Offset(300, 30), const Offset(350, 100), crackPaint);

    _drawLabel(canvas, w, h, 'ICE FLOOR  🧊 Low Friction', const Color(0xFF00BCD4));
  }

  void _drawLabel(Canvas canvas, double w, double h, String text, Color color) {
    const fs = 11.0;
    final tp = TextPainter(
      text: TextSpan(
        text: text,
        style: TextStyle(
          color: color.withAlpha(210),
          fontSize: fs,
          fontWeight: FontWeight.w800,
          letterSpacing: 1.5,
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    tp.paint(canvas, Offset(w / 2 - tp.width / 2, h - fs - 10));
  }
}
