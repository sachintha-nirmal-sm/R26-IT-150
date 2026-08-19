import 'package:flame/components.dart';
import 'package:flutter/material.dart';

import 'draggable_item.dart';

enum SurfaceType {
  wheels, // Small Area
  planks, // Large Area
}

class SurfaceModifierComponent extends DraggableItem {
  SurfaceModifierComponent({
    required this.type,
    required super.startPosition,
    required this.onDroppedCallback,
  }) : super(
          size: type == SurfaceType.wheels ? Vector2(60, 40) : Vector2(160, 40),
        );

  final SurfaceType type;
  final void Function(SurfaceModifierComponent) onDroppedCallback;

  double get area => type == SurfaceType.wheels ? 2.0 : 10.0;
  String get name => type == SurfaceType.wheels ? 'Small Iron Wheels' : 'Wide Wooden Planks';

  @override
  void onDropped() {
    // Notify the game that this was dropped
    onDroppedCallback(this);
  }

  @override
  void render(Canvas canvas) {
    renderDragShadow(canvas);
    
    final w = size.x;
    final h = size.y;
    final rect = Rect.fromLTWH(0, 0, w, h);

    if (type == SurfaceType.wheels) {
      // Draw Wheels
      final paint = Paint()..color = Colors.grey[700]!;
      canvas.drawRRect(
        RRect.fromRectAndRadius(rect, const Radius.circular(8)),
        paint,
      );
      
      // Add wheel details
      final innerPaint = Paint()..color = Colors.black87;
      canvas.drawCircle(Offset(15, h / 2), 12, innerPaint);
      canvas.drawCircle(Offset(w - 15, h / 2), 12, innerPaint);
      
      _drawText(canvas, 'A=2m²', w / 2, h / 2);
    } else {
      // Draw Planks
      final paint = Paint()..color = Colors.brown[600]!;
      canvas.drawRRect(
        RRect.fromRectAndRadius(rect, const Radius.circular(4)),
        paint,
      );
      
      // Plank lines
      final linePaint = Paint()
        ..color = Colors.brown[900]!
        ..strokeWidth = 2;
      canvas.drawLine(Offset(0, h / 3), Offset(w, h / 3), linePaint);
      canvas.drawLine(Offset(0, 2 * h / 3), Offset(w, 2 * h / 3), linePaint);
      
      _drawText(canvas, 'A=10m²', w / 2, h / 2);
    }
  }
  
  void _drawText(Canvas canvas, String text, double x, double y) {
    final tp = TextPainter(
      text: TextSpan(
        text: text,
        style: const TextStyle(
          color: Colors.white,
          fontSize: 14,
          fontWeight: FontWeight.bold,
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    tp.paint(canvas, Offset(x - tp.width / 2, y - tp.height / 2));
  }
}
