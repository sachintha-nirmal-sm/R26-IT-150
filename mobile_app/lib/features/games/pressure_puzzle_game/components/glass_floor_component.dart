import 'package:flame/components.dart';
import 'package:flutter/material.dart';
import 'dart:math';

enum GlassState {
  intact,
  cracked, // Optional intermediate state
  broken
}

class GlassFloorComponent extends PositionComponent {
  GlassFloorComponent({
    required Vector2 position,
  }) : super(
          position: position,
          size: Vector2(250, 60),
          anchor: Anchor.center,
        );

  final double maxPressure = 100.0;
  GlassState state = GlassState.intact;

  // Cracks coordinates for breaking animation/drawing
  final List<List<Offset>> _cracks = [];

  @override
  void onLoad() {
    super.onLoad();
    _generateCracks();
  }

  void _generateCracks() {
    final rand = Random();
    for (int i = 0; i < 5; i++) {
      List<Offset> crack = [];
      double x = rand.nextDouble() * size.x;
      double y = rand.nextDouble() * size.y;
      crack.add(Offset(x, y));
      
      for (int j = 0; j < 3; j++) {
        x += (rand.nextDouble() - 0.5) * 40;
        y += (rand.nextDouble() - 0.5) * 40;
        crack.add(Offset(x, y));
      }
      _cracks.add(crack);
    }
  }

  void breakGlass() {
    state = GlassState.broken;
  }

  void repairGlass() {
    state = GlassState.intact;
  }

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);

    if (state == GlassState.broken) {
      // Draw shattered glass
      final paint = Paint()
        ..color = Colors.lightBlueAccent.withAlpha(100)
        ..style = PaintingStyle.fill;
      
      canvas.drawRect(rect, paint);
      
      final crackPaint = Paint()
        ..color = Colors.white
        ..style = PaintingStyle.stroke
        ..strokeWidth = 2;
        
      for (var crack in _cracks) {
        for (int i = 0; i < crack.length - 1; i++) {
          canvas.drawLine(crack[i], crack[i + 1], crackPaint);
        }
      }
      
      _drawText(canvas, 'SHATTERED!', size.x / 2, size.y / 2, color: Colors.red[900]!);
    } else {
      // Draw intact glass
      final paint = Paint()
        ..color = Colors.lightBlueAccent.withAlpha(150)
        ..style = PaintingStyle.fill;
      
      canvas.drawRRect(
        RRect.fromRectAndRadius(rect, const Radius.circular(8)),
        paint,
      );

      // Glass reflections
      final highlightPaint = Paint()
        ..color = Colors.white.withAlpha(100)
        ..style = PaintingStyle.stroke
        ..strokeWidth = 4;
      
      canvas.drawLine(const Offset(20, 10), const Offset(100, 10), highlightPaint);
      canvas.drawLine(Offset(size.x - 80, 20), Offset(size.x - 20, 20), highlightPaint);

      _drawText(canvas, 'Max Pressure: 100 Pa', size.x / 2, size.y / 2);
    }
  }
  
  void _drawText(Canvas canvas, String text, double x, double y, {Color color = Colors.white}) {
    final tp = TextPainter(
      text: TextSpan(
        text: text,
        style: TextStyle(
          color: color,
          fontSize: 16,
          fontWeight: FontWeight.bold,
          shadows: const [Shadow(color: Colors.black, blurRadius: 4)],
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    tp.paint(canvas, Offset(x - tp.width / 2, y - tp.height / 2));
  }
}
