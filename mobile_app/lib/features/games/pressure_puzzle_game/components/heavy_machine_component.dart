import 'package:flame/components.dart';
import 'package:flutter/material.dart';

import 'draggable_item.dart';

class HeavyMachineComponent extends DraggableItem {
  HeavyMachineComponent({
    required super.startPosition,
    required this.onDroppedCallback,
  }) : super(
          size: Vector2(100, 100),
        );

  final void Function(HeavyMachineComponent) onDroppedCallback;

  // The fixed force of this machine
  final double force = 400.0; 

  @override
  void onDropped() {
    onDroppedCallback(this);
  }

  @override
  void render(Canvas canvas) {
    renderDragShadow(canvas);

    final w = size.x;
    final h = size.y;
    final rect = Rect.fromLTWH(0, 0, w, h);

    // Draw heavy machine body
    final paint = Paint()
      ..color = Colors.blueGrey[800]!
      ..style = PaintingStyle.fill;
    
    canvas.drawRRect(
      RRect.fromRectAndRadius(rect, const Radius.circular(12)),
      paint,
    );

    // Inner detail
    final innerPaint = Paint()
      ..color = Colors.blueGrey[600]!
      ..style = PaintingStyle.fill;
    
    canvas.drawRect(
      Rect.fromLTWH(10, 10, w - 20, h - 40),
      innerPaint,
    );

    // Add hazard stripes at the bottom
    final stripePaint = Paint()
      ..color = Colors.yellow[700]!
      ..style = PaintingStyle.fill;
    
    for (int i = 0; i < 5; i++) {
      canvas.drawRect(
        Rect.fromLTWH(10 + i * 16.0, h - 25, 8, 15),
        stripePaint,
      );
    }

    _drawText(canvas, 'F=400N', w / 2, h / 2 - 5);
    _drawText(canvas, 'HEAVY', w / 2, h / 2 + 15, fontSize: 10, color: Colors.red[300]!);
  }
  
  void _drawText(
    Canvas canvas, 
    String text, 
    double x, 
    double y, {
    double fontSize = 16,
    Color color = Colors.white,
  }) {
    final tp = TextPainter(
      text: TextSpan(
        text: text,
        style: TextStyle(
          color: color,
          fontSize: fontSize,
          fontWeight: FontWeight.bold,
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    tp.paint(canvas, Offset(x - tp.width / 2, y - tp.height / 2));
  }
}
