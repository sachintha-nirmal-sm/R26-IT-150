import 'dart:math';
import 'package:flutter/material.dart';
import 'package:flame/game.dart';
import 'package:flame/components.dart';
import 'package:flame/events.dart';

// =========================================================================
// 1. Equilibrium of Forces Game Class
// =========================================================================

class EquilibriumForcesGame extends FlameGame with DragCallbacks {
  final VoidCallback? onIncorrectDrop;
  final ValueChanged<int>? onCorrectDrop;
  final VoidCallback? onLevelComplete;

  // Track components
  BlockScenarioComponent? blockScenario;
  BeamScenarioComponent? beamScenario;
  HangingSignScenarioComponent? signScenario;

  DropZoneComponent? zoneBlock;
  DropZoneComponent? zoneBeam;
  DropZoneComponent? zoneSign;

  List<DraggableAnswerComponent> draggables = [];

  EquilibriumForcesGame({this.onIncorrectDrop, this.onCorrectDrop, this.onLevelComplete});

  @override
  Future<void> onLoad() async {
    super.onLoad();
    setupGameElements();
  }

  @override
  void onGameResize(Vector2 size) {
    super.onGameResize(size);
    rebuildLayout();
  }

  void setupGameElements() {
    // Clear pre-existing components
    blockScenario?.removeFromParent();
    beamScenario?.removeFromParent();
    signScenario?.removeFromParent();
    zoneBlock?.removeFromParent();
    zoneBeam?.removeFromParent();
    zoneSign?.removeFromParent();
    draggables.forEach((e) => e.removeFromParent());

    draggables.clear();

    // 1. Create Scenarios
    blockScenario = BlockScenarioComponent();
    add(blockScenario!);
    zoneBlock = DropZoneComponent(scenarioId: 1);
    add(zoneBlock!);

    beamScenario = BeamScenarioComponent();
    add(beamScenario!);
    zoneBeam = DropZoneComponent(scenarioId: 2);
    add(zoneBeam!);

    signScenario = HangingSignScenarioComponent();
    add(signScenario!);
    zoneSign = DropZoneComponent(scenarioId: 3);
    add(zoneSign!);

    // 2. Create Draggables (Bottom)
    final answers = [
      {'id': 1, 'label': '30N Left'},
      {'id': 2, 'label': '50N Upwards'},
      {'id': 3, 'label': 'Intersect at one point'},
    ];

    for (final ans in answers) {
      final drag = DraggableAnswerComponent(
        answerId: ans['id'] as int,
        label: ans['label'] as String,
      );
      draggables.add(drag);
      add(drag);
    }

    rebuildLayout();
  }

  void rebuildLayout() {
    if (size.x == 0 || size.y == 0) return;
    if (blockScenario == null || beamScenario == null || signScenario == null || draggables.length < 3) return;

    final double theoryHeight = 115.0;
    final double bottomAreaHeight = 110.0;
    final double availableHeight = size.y - theoryHeight - bottomAreaHeight;

    final double ySpacing = availableHeight / 4;
    final double centerX = size.x / 2;

    // Block Scenario positioning (Scenario 1)
    final double blockY = theoryHeight + ySpacing * 1.0;
    blockScenario!.position = Vector2(centerX, blockY);
    zoneBlock!.position = Vector2(centerX - 80, blockY);

    // Beam Scenario positioning (Scenario 2)
    final double beamY = theoryHeight + ySpacing * 2.0;
    beamScenario!.position = Vector2(centerX, beamY);
    zoneBeam!.position = Vector2(centerX + 110, beamY - 40);

    // Hanging Sign Scenario positioning (Scenario 3)
    final double signY = theoryHeight + ySpacing * 3.0;
    signScenario!.position = Vector2(centerX, signY);
    zoneSign!.position = Vector2(centerX, signY); // intersection is exactly in the center of the circular sign

    // Bottom answers horizontal layout
    final double dragY = size.y - 60.0;
    final double xSpacing = size.x / 4;

    for (int i = 0; i < 3; i++) {
      final double dragX = xSpacing * (i + 1);
      draggables[i].homePosition = Vector2(dragX, dragY);
      
      if (!draggables[i].isLocked) {
        draggables[i].position = Vector2(dragX, dragY);
      } else {
        // If locked, snap exactly to its respective drop zone
        if (draggables[i].answerId == 1) {
          draggables[i].position = zoneBlock!.position.clone();
        } else if (draggables[i].answerId == 2) {
          draggables[i].position = zoneBeam!.position.clone();
        } else {
          draggables[i].position = zoneSign!.position.clone();
        }
      }
    }
  }

  void checkSnapping(DraggableAnswerComponent draggable) {
    DropZoneComponent? targetZone;
    double minDistance = double.infinity;

    for (final zone in [zoneBlock!, zoneBeam!, zoneSign!]) {
      if (zone.isFilled) continue;
      double dist = draggable.position.distanceTo(zone.position);
      if (dist < minDistance) {
        minDistance = dist;
        targetZone = zone;
      }
    }

    if (targetZone != null && minDistance < 50.0) {
      // Validate correct drop
      bool isCorrect = false;
      if (targetZone.scenarioId == 1 && draggable.answerId == 1) isCorrect = true;
      if (targetZone.scenarioId == 2 && draggable.answerId == 2) isCorrect = true;
      if (targetZone.scenarioId == 3 && draggable.answerId == 3) isCorrect = true;

      if (isCorrect) {
        draggable.position = targetZone.position.clone();
        draggable.isLocked = true;
        draggable.color = const Color(0xFF10B981); // Emerald Green
        targetZone.isFilled = true;

        if (targetZone.scenarioId == 1) {
          blockScenario!.triggerReaction();
        } else if (targetZone.scenarioId == 2) {
          beamScenario!.triggerReaction();
        } else if (targetZone.scenarioId == 3) {
          signScenario!.triggerReaction();
        }

        onCorrectDrop?.call(targetZone.scenarioId);
        _checkLevelCompletion();
      } else {
        draggable.isSnappingBack = true;
        onIncorrectDrop?.call();
      }
    } else {
      draggable.isSnappingBack = true;
    }
  }

  void _checkLevelCompletion() {
    final allSolved = [zoneBlock!, zoneBeam!, zoneSign!].every((z) => z.isFilled);
    if (allSolved) {
      Future.delayed(const Duration(milliseconds: 1500), () {
        pauseEngine();
        onLevelComplete?.call();
      });
    }
  }

  void resetGame() {
    setupGameElements();
    resumeEngine();
  }
}

// =========================================================================
// 2. Scenario 1: Shaking Block on Horizontal Line Component
// =========================================================================

class BlockScenarioComponent extends PositionComponent with HasGameRef<EquilibriumForcesGame> {
  bool isShaking = true;
  double shakeTime = 0.0;
  double shakeOffset = 0.0;
  bool showResultantText = false;

  BlockScenarioComponent() {
    size = Vector2(50.0, 50.0);
    anchor = Anchor.center;
  }

  void triggerReaction() {
    isShaking = false;
    shakeOffset = 0.0;
    showResultantText = true;
  }

  @override
  void update(double dt) {
    super.update(dt);
    if (isShaking) {
      shakeTime += dt;
      shakeOffset = sin(shakeTime * 75.0) * 1.8;
    }
  }

  @override
  void render(Canvas canvas) {
    // 1. Horizontal ground line
    final linePaint = Paint()
      ..color = Colors.blueGrey.shade600
      ..strokeWidth = 2.0;
    canvas.drawLine(Offset(-size.x * 2.5, size.y / 2), Offset(size.x * 2.5, size.y / 2), linePaint);

    // Apply shaking offset to the block drawing
    canvas.save();
    canvas.translate(shakeOffset, 0);

    // 2. Block Shape
    final rect = Rect.fromCenter(center: Offset(0, 0), width: size.x, height: size.y);
    final blockPaint = Paint()
      ..color = const Color(0xFF475569)
      ..style = PaintingStyle.fill;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(6.0)), blockPaint);

    final borderPaint = Paint()
      ..color = isSolved() ? Colors.greenAccent : Colors.blueGrey.shade300
      ..strokeWidth = 2.0
      ..style = PaintingStyle.stroke;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(6.0)), borderPaint);

    // Force labels & arrows
    // Applied Force = 30N pointing Right
    _drawVectorArrow(canvas, Offset(size.x / 2, 0), Offset(size.x / 2 + 50.0, 0), "Applied = 30N", Colors.redAccent);

    // If solved, draw the balancing force vector pointing Left (30N Left)
    if (isSolved()) {
      _drawVectorArrow(canvas, Offset(-size.x / 2, 0), Offset(-size.x / 2 - 50.0, 0), "Friction = 30N", Colors.greenAccent);
    }

    canvas.restore();

    // Resultant force text indicator
    if (showResultantText) {
      final painter = TextPainter(
        text: const TextSpan(
          text: 'Resultant = 0 N',
          style: TextStyle(color: Colors.greenAccent, fontSize: 10.0, fontWeight: FontWeight.bold, fontFamily: 'Poppins'),
        ),
        textDirection: TextDirection.ltr,
      );
      painter.layout();
      painter.paint(canvas, Offset(-painter.width / 2, -size.y / 2 - 16.0));
    }

    // Title label
    final textPainter = TextPainter(
      text: const TextSpan(
        text: 'Horizontal Equilibrium',
        style: TextStyle(color: Colors.white, fontSize: 10.0, fontWeight: FontWeight.bold, fontFamily: 'Poppins'),
      ),
      textDirection: TextDirection.ltr,
    );
    textPainter.layout();
    textPainter.paint(canvas, Offset(-textPainter.width / 2, -size.y / 2 - 30.0));
  }

  bool isSolved() {
    return game.zoneBlock?.isFilled ?? false;
  }

  void _drawVectorArrow(Canvas canvas, Offset start, Offset end, String forceLabel, Color color) {
    final double headSize = 8.0;
    final paint = Paint()
      ..color = color
      ..strokeWidth = 2.5
      ..style = PaintingStyle.stroke;

    canvas.drawLine(start, end, paint);

    final headPaint = Paint()
      ..color = color
      ..style = PaintingStyle.fill;
    
    final path = Path();
    path.moveTo(end.dx, end.dy);
    
    // Calculate unit vector direction to draw arrowhead correctly
    final double dx = end.dx - start.dx;
    final double ux = dx > 0 ? 1 : -1;
    
    path.lineTo(end.dx - headSize * ux, end.dy - 4);
    path.lineTo(end.dx - headSize * ux, end.dy + 4);
    path.close();
    canvas.drawPath(path, headPaint);

    final textPainter = TextPainter(
      text: TextSpan(text: forceLabel, style: TextStyle(color: color, fontSize: 9.0, fontWeight: FontWeight.bold, fontFamily: 'Poppins')),
      textDirection: TextDirection.ltr,
    );
    textPainter.layout();
    
    // Text placement
    final double labelX = dx > 0 ? (start.dx + 5) : (end.dx + 5);
    textPainter.paint(canvas, Offset(labelX, start.dy - 13));
  }
}

// =========================================================================
// 3. Scenario 2: Tilted Support Beam Component
// =========================================================================

class BeamScenarioComponent extends PositionComponent with HasGameRef<EquilibriumForcesGame> {
  bool isBalancing = false;
  double angleSpeed = 0.3; // radians per sec

  BeamScenarioComponent() {
    size = Vector2(200.0, 14.0);
    anchor = Anchor.center;
    angle = 0.12; // Start tilted right
  }

  void triggerReaction() {
    isBalancing = true;
  }

  @override
  void update(double dt) {
    super.update(dt);
    if (isBalancing) {
      if (angle > 0.0) {
        angle -= angleSpeed * dt;
        if (angle <= 0.0) {
          angle = 0.0;
          isBalancing = false;
        }
      }
    }
  }

  @override
  void render(Canvas canvas) {
    final rect = size.toRect();
    
    // Beam shaft
    final beamPaint = Paint()
      ..color = const Color(0xFF64748B)
      ..style = PaintingStyle.fill;
    canvas.drawRect(rect, beamPaint);

    final borderPaint = Paint()
      ..color = isSolved() ? Colors.greenAccent : Colors.blueGrey.shade300
      ..strokeWidth = 2.0
      ..style = PaintingStyle.stroke;
    canvas.drawRect(rect, borderPaint);

    // Forces Arrows (Relative to rotated beam coordinate)
    // 1. Weight pointing downwards in the center
    _drawVectorArrow(canvas, Offset(0, size.y / 2), Offset(0, size.y / 2 + 35.0), "Weight = 120N", Colors.redAccent, false);

    // 2. Support A pointing upwards on left edge
    _drawVectorArrow(canvas, Offset(-size.x / 2, 0), Offset(-size.x / 2, -35.0), "Support A = 70N", Colors.blueAccent, true);

    // 3. If solved, show support B arrow pointing upwards on the far right
    if (isSolved()) {
      _drawVectorArrow(canvas, Offset(size.x / 2, 0), Offset(size.x / 2, -35.0), "Support B = 50N", Colors.greenAccent, true);
    }

    // Label Text
    final textPainter = TextPainter(
      text: const TextSpan(
        text: 'Parallel Vertical Equilibrium',
        style: TextStyle(color: Colors.white, fontSize: 10.0, fontWeight: FontWeight.bold, fontFamily: 'Poppins'),
      ),
      textDirection: TextDirection.ltr,
    );
    textPainter.layout();
    textPainter.paint(canvas, Offset(-textPainter.width / 2, -size.y / 2 - 50.0));
  }

  bool isSolved() {
    return game.zoneBeam?.isFilled ?? false;
  }

  void _drawVectorArrow(Canvas canvas, Offset start, Offset end, String forceLabel, Color color, bool arrowPointsUp) {
    final double headSize = 8.0;
    final paint = Paint()
      ..color = color
      ..strokeWidth = 2.5
      ..style = PaintingStyle.stroke;

    canvas.drawLine(start, end, paint);

    final headPaint = Paint()
      ..color = color
      ..style = PaintingStyle.fill;
    
    final path = Path();
    path.moveTo(end.dx, end.dy);
    
    if (arrowPointsUp) {
      path.lineTo(end.dx - 5, end.dy + headSize);
      path.lineTo(end.dx + 5, end.dy + headSize);
    } else {
      path.lineTo(end.dx - 5, end.dy - headSize);
      path.lineTo(end.dx + 5, end.dy - headSize);
    }
    path.close();
    canvas.drawPath(path, headPaint);

    final textPainter = TextPainter(
      text: TextSpan(text: forceLabel, style: TextStyle(color: color, fontSize: 8.5, fontWeight: FontWeight.bold, fontFamily: 'Poppins')),
      textDirection: TextDirection.ltr,
    );
    textPainter.layout();
    
    // Position text
    final double textY = arrowPointsUp ? (end.dy - 12) : (end.dy + 3);
    textPainter.paint(canvas, Offset(end.dx - textPainter.width / 2, textY));
  }
}

// =========================================================================
// 4. Scenario 3: Hanging Sign with Concurrent Intersecting Forces
// =========================================================================

class HangingSignScenarioComponent extends PositionComponent with HasGameRef<EquilibriumForcesGame> {
  bool isPulsing = false;
  double pulseTime = 0.0;
  double pulseScale = 1.0;

  HangingSignScenarioComponent() {
    size = Vector2(85.0, 85.0);
    anchor = Anchor.center;
  }

  void triggerReaction() {
    isPulsing = true;
  }

  @override
  void update(double dt) {
    super.update(dt);
    if (isPulsing) {
      pulseTime += dt;
      pulseScale = 1.0 + sin(pulseTime * 8.0) * 0.25; // Pulsing effect
    }
  }

  @override
  void render(Canvas canvas) {
    final double center = size.x / 2;

    // 1. Strings hanging from top corners
    final stringPaint = Paint()
      ..color = Colors.white30
      ..strokeWidth = 1.5;
    
    // Draw hanging lines from imaginary suspension points
    canvas.drawLine(Offset(-60, -70), Offset(0, 0), stringPaint);
    canvas.drawLine(Offset(60, -70), Offset(0, 0), stringPaint);

    // 2. Circular Sign
    final bgPaint = Paint()
      ..color = const Color(0xFF1E293B)
      ..style = PaintingStyle.fill;
    canvas.drawCircle(Offset(0, 0), center, bgPaint);

    final borderPaint = Paint()
      ..color = isSolved() ? Colors.greenAccent : Colors.blueGrey.shade300
      ..strokeWidth = 2.0
      ..style = PaintingStyle.stroke;
    canvas.drawCircle(Offset(0, 0), center, borderPaint);

    // Sign text
    final text1 = TextPainter(
      text: const TextSpan(text: "HANGING\nSIGN", style: TextStyle(color: Colors.white60, fontSize: 8.5, fontWeight: FontWeight.bold, fontFamily: 'Poppins')),
      textDirection: TextDirection.ltr,
      textAlign: TextAlign.center,
    );
    text1.layout();
    text1.paint(canvas, Offset(-text1.width / 2, -28.0));

    // 3. Dashed lines extending from the force vectors to show intersection in the center
    final dashPaint = Paint()
      ..color = isSolved() ? Colors.yellowAccent.withOpacity(0.5) : Colors.white24
      ..strokeWidth = 1.5;
    
    // Up-Left Tension line
    _drawDashedLine(canvas, Offset(-60, -60), Offset(0, 0), dashPaint);
    // Up-Right Tension line
    _drawDashedLine(canvas, Offset(60, -60), Offset(0, 0), dashPaint);
    // Down Weight line
    _drawDashedLine(canvas, Offset(0, 70), Offset(0, 0), dashPaint);

    // Vectors arrows
    _drawVectorArrow(canvas, Offset(0, 0), Offset(0, 50), "W (Weight)", Colors.orange.shade300, false);
    _drawVectorArrow(canvas, Offset(0, 0), Offset(-40, -40), "T1", Colors.orange.shade300, true);
    _drawVectorArrow(canvas, Offset(0, 0), Offset(40, -40), "T2", Colors.orange.shade300, true);

    // 4. Pulsing Intersection Equilibrium Point
    if (isSolved()) {
      final pulsePaint = Paint()
        ..color = Colors.yellowAccent
        ..style = PaintingStyle.fill;
      canvas.drawCircle(Offset(0, 0), 6.5 * pulseScale, pulsePaint);
      
      final glowPaint = Paint()
        ..color = Colors.yellowAccent.withOpacity(0.3)
        ..style = PaintingStyle.fill;
      canvas.drawCircle(Offset(0, 0), 12.0 * pulseScale, glowPaint);
    }

    // Label Text
    final textPainter = TextPainter(
      text: const TextSpan(
        text: '3 Non-Parallel Forces',
        style: TextStyle(color: Colors.white, fontSize: 10.0, fontWeight: FontWeight.bold, fontFamily: 'Poppins'),
      ),
      textDirection: TextDirection.ltr,
    );
    textPainter.layout();
    textPainter.paint(canvas, Offset(-textPainter.width / 2, -center - 18.0));
  }

  bool isSolved() {
    return game.zoneSign?.isFilled ?? false;
  }

  void _drawDashedLine(Canvas canvas, Offset p1, Offset p2, Paint paint) {
    final double dx = p2.dx - p1.dx;
    final double dy = p2.dy - p1.dy;
    final double len = sqrt(dx * dx + dy * dy);
    final double dashLen = 6.0;
    final double gapLen = 4.0;
    
    double dist = 0.0;
    final double ux = dx / len;
    final double uy = dy / len;

    while (dist < len) {
      final double endX = p1.dx + (dist + dashLen).clamp(0, len) * ux;
      final double endY = p1.dy + (dist + dashLen).clamp(0, len) * uy;
      canvas.drawLine(Offset(p1.dx + dist * ux, p1.dy + dist * uy), Offset(endX, endY), paint);
      dist += dashLen + gapLen;
    }
  }

  void _drawVectorArrow(Canvas canvas, Offset start, Offset end, String forceLabel, Color color, bool offsetLabelUp) {
    final double headSize = 7.0;
    final paint = Paint()
      ..color = color
      ..strokeWidth = 2.0
      ..style = PaintingStyle.stroke;

    canvas.drawLine(start, end, paint);

    final headPaint = Paint()
      ..color = color
      ..style = PaintingStyle.fill;

    final double dx = end.dx - start.dx;
    final double dy = end.dy - start.dy;
    final double len = sqrt(dx * dx + dy * dy);
    
    if (len > 0.5) {
      final double ux = dx / len;
      final double uy = dy / len;
      final double px = -uy;
      final double py = ux;

      final path = Path();
      path.moveTo(end.dx, end.dy);
      path.lineTo(
        end.dx - headSize * ux + (headSize / 1.7) * px,
        end.dy - headSize * uy + (headSize / 1.7) * py,
      );
      path.lineTo(
        end.dx - headSize * ux - (headSize / 1.7) * px,
        end.dy - headSize * uy - (headSize / 1.7) * py,
      );
      path.close();
      canvas.drawPath(path, headPaint);
    }

    final textPainter = TextPainter(
      text: TextSpan(text: forceLabel, style: TextStyle(color: color, fontSize: 8.5, fontWeight: FontWeight.bold, fontFamily: 'Poppins')),
      textDirection: TextDirection.ltr,
    );
    textPainter.layout();
    
    final double textX = end.dx - textPainter.width / 2;
    final double textY = offsetLabelUp ? (end.dy - 12) : (end.dy + 3);
    textPainter.paint(canvas, Offset(textX, textY));
  }
}

// =========================================================================
// 5. Drop Zone Component
// =========================================================================

class DropZoneComponent extends PositionComponent with HasGameRef<EquilibriumForcesGame> {
  final int scenarioId;
  bool isFilled = false;

  DropZoneComponent({required this.scenarioId}) {
    size = Vector2(105.0, 36.0);
    anchor = Anchor.center;
  }

  @override
  void render(Canvas canvas) {
    final rect = size.toRect();
    
    final bgPaint = Paint()
      ..color = isFilled ? Colors.green.withOpacity(0.08) : Colors.white.withOpacity(0.04)
      ..style = PaintingStyle.fill;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(8.0)), bgPaint);

    final borderPaint = Paint()
      ..color = isFilled ? Colors.greenAccent : Colors.white24
      ..strokeWidth = 1.8
      ..style = PaintingStyle.stroke;
    
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(8.0)), borderPaint);

    if (!isFilled) {
      final painter = TextPainter(
        text: TextSpan(
          text: 'Drop Ans $scenarioId',
          style: const TextStyle(
            color: Colors.white30,
            fontSize: 9.0,
            fontWeight: FontWeight.w600,
            fontFamily: 'Poppins',
          ),
        ),
        textDirection: TextDirection.ltr,
      );
      painter.layout();
      painter.paint(
        canvas,
        Offset((size.x - painter.width) / 2, (size.y - painter.height) / 2),
      );
    }
  }
}

// =========================================================================
// 6. Draggable Answer Component
// =========================================================================

class DraggableAnswerComponent extends PositionComponent with DragCallbacks, HasGameRef<EquilibriumForcesGame> {
  final int answerId;
  final String label;
  
  Vector2 homePosition = Vector2.zero();
  bool isLocked = false;
  bool isSnappingBack = false;
  Color color = const Color(0xFF1E293B);

  DraggableAnswerComponent({
    required this.answerId,
    required this.label,
  }) : super(size: Vector2(110.0, 38.0), anchor: Anchor.center);

  @override
  void update(double dt) {
    super.update(dt);
    if (isSnappingBack) {
      position.lerp(homePosition, 0.16);
      if (position.distanceTo(homePosition) < 1.0) {
        position = homePosition.clone();
        isSnappingBack = false;
      }
    }
  }

  @override
  void render(Canvas canvas) {
    final rect = size.toRect();
    
    final fillPaint = Paint()
      ..color = color
      ..style = PaintingStyle.fill;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(8.0)), fillPaint);

    final borderPaint = Paint()
      ..color = isLocked ? Colors.greenAccent : Colors.blueAccent.withOpacity(0.3)
      ..strokeWidth = 2.0
      ..style = PaintingStyle.stroke;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(8.0)), borderPaint);

    // Label text
    final painter = TextPainter(
      text: TextSpan(
        text: label,
        style: const TextStyle(
          color: Colors.white,
          fontSize: 10.0,
          fontWeight: FontWeight.bold,
          fontFamily: 'Poppins',
        ),
      ),
      textDirection: TextDirection.ltr,
    );
    painter.layout();
    painter.paint(
      canvas,
      Offset((size.x - painter.width) / 2, (size.y - painter.height) / 2),
    );
  }

  @override
  void onDragStart(DragStartEvent event) {
    if (isLocked) return;
    super.onDragStart(event);
    isSnappingBack = false;
    priority = 100;
  }

  @override
  void onDragUpdate(DragUpdateEvent event) {
    if (isLocked) return;
    position += event.localDelta;
  }

  @override
  void onDragEnd(DragEndEvent event) {
    if (isLocked) return;
    super.onDragEnd(event);
    priority = 1;
    game.checkSnapping(this);
  }
}

// =========================================================================
// 7. Flutter Main Screen Widget
// =========================================================================

class EquilibriumForcesScreen extends StatefulWidget {
  const EquilibriumForcesScreen({super.key});

  @override
  State<EquilibriumForcesScreen> createState() => _EquilibriumForcesScreenState();
}

class _EquilibriumForcesScreenState extends State<EquilibriumForcesScreen> {
  late EquilibriumForcesGame game;
  bool _showBanner = false;

  @override
  void initState() {
    super.initState();
    game = EquilibriumForcesGame(
      onIncorrectDrop: _handleIncorrectDrop,
      onCorrectDrop: _showCorrectTheoryDialog,
      onLevelComplete: _handleLevelComplete,
    );
  }

  void _handleIncorrectDrop() {
    setState(() {
      _showBanner = true;
    });

    // Auto-dismiss hint banner after 4 seconds
    Future.delayed(const Duration(milliseconds: 4000), () {
      if (mounted) {
        setState(() {
          _showBanner = false;
        });
      }
    });
  }

  void _showCorrectTheoryDialog(int scenarioId) {
    game.pauseEngine();

    String title = "";
    String content = "";
    if (scenarioId == 1) {
      title = "Translational Equilibrium (Horizontal)";
      content = "For an object to remain stationary (in static equilibrium), the resultant force acting on it must be zero.\n\n"
          "If a 30 N horizontal force is applied to the right, a matching 30 N force must act to the left (e.g. friction or tension):\n"
          "Resultant Force = 30 N - 30 N = 0 N.";
    } else if (scenarioId == 2) {
      title = "Parallel Forces in Equilibrium";
      content = "For vertical equilibrium, the sum of all upward forces must equal the sum of all downward forces.\n\n"
          "Equation:\n"
          "Support A + Support B = Weight\n"
          "70 N + Support B = 120 N\n"
          "Support B = 120 N - 70 N = 50 N (Upwards).";
    } else if (scenarioId == 3) {
      title = "Three Non-Parallel Forces";
      content = "When three non-parallel forces act on a body in equilibrium, their lines of action must intersect at a single concurrent point.\n\n"
          "This ensures that their vector sum equals zero without causing any turning moment or rotation (torque). The lines of Tension 1, Tension 2, and Weight meet exactly at the sign's center of gravity.";
    }

    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => AlertDialog(
        backgroundColor: const Color(0xFF1E293B),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16.0)),
        title: Row(
          children: [
            const Icon(Icons.check_circle, color: Colors.greenAccent, size: 28),
            const SizedBox(width: 10),
            Expanded(
              child: Text(
                title,
                style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 16.0),
              ),
            ),
          ],
        ),
        content: Text(
          content,
          style: const TextStyle(color: Color(0xFFCBD5E1), fontSize: 13.5, height: 1.5),
        ),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.pop(context);
              game.resumeEngine();
            },
            child: const Text("Got it!", style: TextStyle(color: Colors.blueAccent, fontWeight: FontWeight.bold)),
          ),
        ],
      ),
    );
  }

  void _handleLevelComplete() {
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => AlertDialog(
        backgroundColor: const Color(0xFF1E293B),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16.0)),
        title: const Row(
          children: [
            Icon(Icons.stars, color: Colors.yellowAccent, size: 30),
            SizedBox(width: 10),
            Expanded(
              child: Text(
                "Equilibrium Mastered!",
                style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
              ),
            ),
          ],
        ),
        content: const Text(
          "Congratulations! You solved all equilibrium conditions:\n\n"
          "• Block: 30N Right = 30N Left (Resultant 0N)\n"
          "• Beam: 70N + 50N Upwards = 120N Downwards\n"
          "• Hanging Sign: Forces intersect concurrent at center",
          style: TextStyle(color: Color(0xFFCBD5E1), fontSize: 13.5, height: 1.5),
        ),
        actions: [
          ElevatedButton.icon(
            style: ElevatedButton.styleFrom(
              backgroundColor: Colors.green.shade800,
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10.0)),
            ),
            onPressed: () {
              Navigator.pop(context);
              setState(() {
                _showBanner = false;
                game.resetGame();
              });
            },
            icon: const Icon(Icons.refresh),
            label: const Text("Restart Game", style: TextStyle(fontWeight: FontWeight.bold)),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final double screenWidth = MediaQuery.of(context).size.width;
    final bool isSmallScreen = screenWidth < 380;

    return Scaffold(
      backgroundColor: const Color(0xFF0F172A),
      body: Stack(
        children: [
          // 1. Flame Game Canvas
          Positioned.fill(
            child: GameWidget(game: game),
          ),

          // 2. Header overlay
          Positioned(
            top: 0,
            left: 16.0,
            right: 16.0,
            child: SafeArea(
              bottom: false,
              child: Padding(
                padding: const EdgeInsets.only(top: 8.0),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    IconButton(
                      style: IconButton.styleFrom(backgroundColor: const Color(0xFF1E293B)),
                      onPressed: () => Navigator.maybePop(context),
                      icon: const Icon(Icons.arrow_back_ios_new, color: Colors.white, size: 18),
                    ),
                    Expanded(
                      child: Center(
                        child: Text(
                          "EQUILIBRIUM LAB",
                          style: TextStyle(
                            color: Colors.white,
                            fontSize: isSmallScreen ? 14.0 : 16.0,
                            fontWeight: FontWeight.w800,
                            letterSpacing: 1.0,
                          ),
                        ),
                      ),
                    ),
                    const SizedBox(width: 48),
                  ],
                ),
              ),
            ),
          ),

          // 3. Theory Board
          Positioned(
            top: 65,
            left: 16.0,
            right: 16.0,
            child: SafeArea(
              bottom: false,
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 14.0, vertical: 10.0),
                decoration: BoxDecoration(
                  color: const Color(0xFF1E293B).withOpacity(0.85),
                  borderRadius: BorderRadius.circular(14.0),
                  border: Border.all(color: Colors.white.withOpacity(0.12), width: 1.2),
                ),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Text(
                      "THEORY BOARD",
                      style: TextStyle(color: Colors.grey, fontSize: 9.5, fontWeight: FontWeight.bold, letterSpacing: 0.8),
                    ),
                    const SizedBox(height: 6),
                    Wrap(
                      spacing: 8.0,
                      runSpacing: 6.0,
                      alignment: WrapAlignment.center,
                      children: [
                        _buildRuleBadge(isSmallScreen ? "Equil: Resultant = 0" : "Equilibrium: Resultant Force = 0", Colors.blue.shade100, isSmallScreen),
                        _buildRuleBadge(isSmallScreen ? "Upward F = Downward F" : "Upward Forces = Downward Forces", Colors.orange.shade100, isSmallScreen),
                        _buildRuleBadge(isSmallScreen ? "3 Forces: Intersect Point" : "3 Non-parallel forces intersect at one point", Colors.green.shade100, isSmallScreen),
                      ],
                    ),
                  ],
                ),
              ),
            ),
          ),

          // 4. Temporary Incorrect Drop Warning overlay
          if (_showBanner)
            Positioned(
              bottom: 120.0,
              left: 16.0,
              right: 16.0,
              child: SafeArea(
                top: false,
                bottom: false,
                child: Container(
                  padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 12.0),
                  decoration: BoxDecoration(
                    color: Colors.red.shade900.withOpacity(0.95),
                    borderRadius: BorderRadius.circular(12.0),
                    border: Border.all(color: Colors.redAccent.shade100, width: 1.2),
                    boxShadow: const [
                      BoxShadow(color: Colors.black38, blurRadius: 8.0, offset: Offset(0, 3)),
                    ],
                  ),
                  child: Row(
                    children: [
                      const Icon(Icons.info_outline, color: Colors.white, size: 20),
                      const SizedBox(width: 10),
                      Expanded(
                        child: Text(
                          "Incorrect! Hint: Check if total upward forces equal total downward forces or lines meet.",
                          style: TextStyle(
                            color: Colors.white,
                            fontSize: isSmallScreen ? 11.0 : 12.0,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                      GestureDetector(
                        onTap: () {
                          setState(() {
                            _showBanner = false;
                          });
                        },
                        child: const Icon(Icons.close, color: Colors.white70, size: 18),
                      ),
                    ],
                  ),
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildRuleBadge(String text, Color textColor, bool compact) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8.0, vertical: 4.5),
      decoration: BoxDecoration(
        color: Colors.white.withOpacity(0.05),
        borderRadius: BorderRadius.circular(8.0),
      ),
      child: Text(
        text,
        style: TextStyle(
          color: textColor,
          fontSize: compact ? 8.5 : 10.0,
          fontWeight: FontWeight.bold,
        ),
      ),
    );
  }
}
