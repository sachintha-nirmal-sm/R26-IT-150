import 'package:flame/components.dart';
import 'package:flame/events.dart';
import 'package:flutter/material.dart';

/// Base class for draggable items in the Pressure Puzzle Game.
abstract class DraggableItem extends PositionComponent with DragCallbacks {
  DraggableItem({
    required this.startPosition,
    required Vector2 size,
  }) : super(position: startPosition, size: size, anchor: Anchor.center);

  final Vector2 startPosition;
  bool isDragging = false;
  
  /// The position this item snaps to when dropped successfully.
  Vector2? snappedPosition;
  
  @override
  void onDragStart(DragStartEvent event) {
    super.onDragStart(event);
    isDragging = true;
    priority = 10; // Bring to front while dragging
  }

  @override
  void onDragUpdate(DragUpdateEvent event) {
    if (isDragging) {
      position += event.localDelta;
    }
  }

  @override
  void onDragEnd(DragEndEvent event) {
    super.onDragEnd(event);
    isDragging = false;
    priority = 1; // Reset priority
    
    // Check if we dropped on a target. 
    // This will be overridden or handled by the game logic, but we provide a hook.
    onDropped();
  }
  
  @override
  void onDragCancel(DragCancelEvent event) {
    super.onDragCancel(event);
    isDragging = false;
    priority = 1;
    resetPosition();
  }

  /// Called when the drag ends. The subclass or game should check if it's over a target.
  void onDropped();

  /// Snaps back to the current safe location (start or snapped position).
  void resetPosition() {
    position = snappedPosition ?? startPosition;
  }
  
  /// Completely resets the item to its original starting position and clears snap state.
  void returnToStart() {
    snappedPosition = null;
    position = startPosition;
  }
  
  /// Snaps to a specific target position.
  void snapTo(Vector2 target) {
    snappedPosition = target;
    position = target;
  }

  /// Utility to paint a generic shadow when dragging
  void renderDragShadow(Canvas canvas) {
    if (isDragging) {
      final shadowPaint = Paint()
        ..color = Colors.black.withAlpha(80)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 8);
      canvas.drawRect(
        Rect.fromLTWH(10, 10, size.x, size.y), 
        shadowPaint,
      );
    }
  }
}
