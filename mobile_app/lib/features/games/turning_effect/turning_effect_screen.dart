import 'dart:math';
import 'package:flutter/material.dart';
import 'package:flame/game.dart';
import 'package:flame/components.dart';
import 'package:flame/events.dart';

// =========================================================================
// 1. Turning Effect Game Class
// =========================================================================

class TurningEffectGame extends FlameGame with DragCallbacks {
  final VoidCallback? onIncorrectDrop;
  final ValueChanged<int>? onCorrectDrop;
  final VoidCallback? onLevelComplete;

  // Track components
  SpannerComponent? spanner;
  ValveComponent? valve;
  SeesawComponent? seesaw;

  DropZoneComponent? zoneSpanner;
  DropZoneComponent? zoneValve;
  DropZoneComponent? zoneSeesaw;

  List<DraggableAnswerComponent> draggables = [];

  TurningEffectGame({this.onIncorrectDrop, this.onCorrectDrop, this.onLevelComplete});

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
    spanner?.removeFromParent();
    valve?.removeFromParent();
    seesaw?.removeFromParent();
    zoneSpanner?.removeFromParent();
    zoneValve?.removeFromParent();
    zoneSeesaw?.removeFromParent();
    draggables.forEach((e) => e.removeFromParent());

    draggables.clear();

    // 1. Create Scenarios
    spanner = SpannerComponent();
    add(spanner!);
    zoneSpanner = DropZoneComponent(scenarioId: 1);
    add(zoneSpanner!);

    valve = ValveComponent();
    add(valve!);
    zoneValve = DropZoneComponent(scenarioId: 2);
    add(zoneValve!);

    seesaw = SeesawComponent();
    add(seesaw!);
    // Note: The seesaw drop zone is attached to the seesaw board child hierarchy so it rotates with it!
    zoneSeesaw = DropZoneComponent(scenarioId: 3);
    seesaw!.board.add(zoneSeesaw!);

    // 2. Create Draggables (Bottom)
    final answers = [
      {'id': 1, 'label': '50 Nm Clockwise'},
      {'id': 2, 'label': 'Anti-clockwise Moment'},
      {'id': 3, 'label': '50N'},
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
    if (spanner == null || valve == null || seesaw == null || draggables.length < 3) return;

    final double theoryHeight = 110.0;
    final double bottomAreaHeight = 110.0;
    final double availableHeight = size.y - theoryHeight - bottomAreaHeight;

    final double ySpacing = availableHeight / 4;
    final double centerX = size.x / 2;

    // Spanner scenario positioning (Scenario 1)
    final double spannerY = theoryHeight + ySpacing * 1.0;
    spanner!.position = Vector2(centerX - 40, spannerY);
    zoneSpanner!.position = Vector2(centerX, spannerY + 45);

    // Valve scenario positioning (Scenario 2)
    final double valveY = theoryHeight + ySpacing * 2.0;
    valve!.position = Vector2(centerX, valveY);
    zoneValve!.position = Vector2(centerX, valveY + 55);

    // Seesaw scenario positioning (Scenario 3)
    final double seesawY = theoryHeight + ySpacing * 3.0;
    seesaw!.position = Vector2(centerX, seesawY);
    // Seesaw drop zone layout: Placed on the right side of the seesaw board locally
    zoneSeesaw!.position = Vector2(100.0, 0.0); // center-right of 240px width seesaw board

    // Bottom answers horizontal layout
    final double dragY = size.y - 60.0;
    final double xSpacing = size.x / 4;

    for (int i = 0; i < 3; i++) {
      final double dragX = xSpacing * (i + 1);
      draggables[i].homePosition = Vector2(dragX, dragY);
      
      if (!draggables[i].isLocked) {
        draggables[i].position = Vector2(dragX, dragY);
      } else {
        // If locked, follow its respective drop zone
        if (draggables[i].answerId == 3) {
          // Seesaw child tracking
          draggables[i].position = zoneSeesaw!.absolutePosition.clone();
        } else if (draggables[i].answerId == 1) {
          draggables[i].position = zoneSpanner!.position.clone();
        } else {
          draggables[i].position = zoneValve!.position.clone();
        }
      }
    }
  }

  void checkSnapping(DraggableAnswerComponent draggable) {
    DropZoneComponent? targetZone;
    double minDistance = double.infinity;

    // Evaluate proximity for global drop zones first
    for (final zone in [zoneSpanner!, zoneValve!]) {
      if (zone.isFilled) continue;
      double dist = draggable.position.distanceTo(zone.position);
      if (dist < minDistance) {
        minDistance = dist;
        targetZone = zone;
      }
    }

    // Evaluate proximity for seesaw drop zone in global coordinates
    if (!zoneSeesaw!.isFilled) {
      double dist = draggable.position.distanceTo(zoneSeesaw!.absolutePosition);
      if (dist < minDistance) {
        minDistance = dist;
        targetZone = zoneSeesaw;
      }
    }

    if (targetZone != null && minDistance < 50.0) {
      // Validate correct drop
      bool isCorrect = false;
      if (targetZone.scenarioId == 1 && draggable.answerId == 1) isCorrect = true;
      if (targetZone.scenarioId == 2 && draggable.answerId == 2) isCorrect = true;
      if (targetZone.scenarioId == 3 && draggable.answerId == 3) isCorrect = true;

      if (isCorrect) {
        draggable.isLocked = true;
        draggable.color = const Color(0xFF10B981); // Emerald Green
        targetZone.isFilled = true;

        if (targetZone.scenarioId == 3) {
          // Lock draggable into seesaw child system so it moves/balances with the seesaw board
          draggable.removeFromParent();
          zoneSeesaw!.add(draggable);
          draggable.position = Vector2(zoneSeesaw!.size.x / 2, zoneSeesaw!.size.y / 2);
          draggable.anchor = Anchor.center;
          seesaw!.triggerReaction();
        } else {
          draggable.position = targetZone.position.clone();
          if (targetZone.scenarioId == 1) {
            spanner!.triggerReaction();
          } else {
            valve!.triggerReaction();
          }
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
    final allSolved = [zoneSpanner!, zoneValve!, zoneSeesaw!].every((z) => z.isFilled);
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
// 2. Scenario 1: The Spanner Component
// =========================================================================

class SpannerComponent extends PositionComponent with HasGameRef<TurningEffectGame> {
  bool isRotating = false;
  double rotationSpeed = 0.8; // rad per sec

  SpannerComponent() {
    size = Vector2(110.0, 18.0);
    anchor = Anchor.centerLeft; // Rotate around the pivot on the far left
  }

  void triggerReaction() {
    isRotating = true;
  }

  @override
  void update(double dt) {
    super.update(dt);
    if (isRotating) {
      angle += rotationSpeed * dt;
      if (angle >= 0.52) { // rotate clockwise by ~30 degrees
        angle = 0.52;
        isRotating = false;
      }
    }
  }

  @override
  void render(Canvas canvas) {
    // 1. Spanner Shaft
    final rect = size.toRect();
    final shaftPaint = Paint()
      ..color = const Color(0xFF64748B)
      ..style = PaintingStyle.fill;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(4.0)), shaftPaint);

    final borderPaint = Paint()
      ..color = isSolved() ? Colors.greenAccent : Colors.blueGrey.shade300
      ..strokeWidth = 1.8
      ..style = PaintingStyle.stroke;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(4.0)), borderPaint);

    // 2. Left Bolt/Pivot Circle
    final pivotPaint = Paint()
      ..color = const Color(0xFF1E293B)
      ..style = PaintingStyle.fill;
    canvas.drawCircle(Offset(0, size.y / 2), 12.0, pivotPaint);
    canvas.drawCircle(Offset(0, size.y / 2), 12.0, borderPaint);

    // Inner bolt ridges (hex nut shape)
    final boltPaint = Paint()
      ..color = Colors.yellow.shade700
      ..style = PaintingStyle.fill;
    canvas.drawCircle(Offset(0, size.y / 2), 5.0, boltPaint);

    // 3. Right Force Vector (10N Down)
    final double rightX = size.x;
    final double centerY = size.y / 2;
    _drawVectorArrow(canvas, Offset(rightX, centerY), Offset(rightX, centerY + 45), "Force = 10N", Colors.redAccent);

    // Text specs
    final textPainter = TextPainter(
      text: const TextSpan(
        text: 'Spanner: Dist = 5m',
        style: TextStyle(color: Colors.white, fontSize: 10.0, fontWeight: FontWeight.bold, fontFamily: 'Poppins'),
      ),
      textDirection: TextDirection.ltr,
    );
    textPainter.layout();
    textPainter.paint(canvas, Offset(size.x * 0.15, -14.0));
  }

  bool isSolved() {
    return game.zoneSpanner?.isFilled ?? false;
  }

  void _drawVectorArrow(Canvas canvas, Offset start, Offset end, String forceLabel, Color color) {
    final double headSize = 8.0;
    final paint = Paint()
      ..color = color
      ..strokeWidth = 2.5
      ..style = PaintingStyle.stroke;

    canvas.drawLine(start, end, paint);

    // Arrow pointing down
    final headPaint = Paint()
      ..color = color
      ..style = PaintingStyle.fill;
    
    final path = Path();
    path.moveTo(end.dx, end.dy);
    path.lineTo(end.dx - 5, end.dy - headSize);
    path.lineTo(end.dx + 5, end.dy - headSize);
    path.close();
    canvas.drawPath(path, headPaint);

    final textPainter = TextPainter(
      text: TextSpan(text: forceLabel, style: TextStyle(color: color, fontSize: 9.0, fontWeight: FontWeight.bold, fontFamily: 'Poppins')),
      textDirection: TextDirection.ltr,
    );
    textPainter.layout();
    textPainter.paint(canvas, Offset(end.dx + 8, end.dy - 25));
  }
}

// =========================================================================
// 3. Scenario 2: The Valve Component
// =========================================================================

class ValveComponent extends PositionComponent with HasGameRef<TurningEffectGame> {
  bool isSpinning = false;
  double rotationSpeed = 1.6; // spin rate

  ValveComponent() {
    size = Vector2(75.0, 75.0);
    anchor = Anchor.center;
  }

  void triggerReaction() {
    isSpinning = true;
  }

  @override
  void update(double dt) {
    super.update(dt);
    if (isSpinning) {
      angle -= rotationSpeed * dt; // spin anti-clockwise continuously
    }
  }

  @override
  void render(Canvas canvas) {
    final double center = size.x / 2;

    // 1. Valve Outer Ring
    final wheelPaint = Paint()
      ..color = const Color(0xFF64748B)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 6.0;
    canvas.drawCircle(Offset(center, center), center - 3, wheelPaint);

    final borderPaint = Paint()
      ..color = isSolved() ? Colors.greenAccent : Colors.blueGrey.shade300
      ..strokeWidth = 1.5
      ..style = PaintingStyle.stroke;
    canvas.drawCircle(Offset(center, center), center, borderPaint);

    // 2. Inner Spokes
    final spokePaint = Paint()
      ..color = const Color(0xFF475569)
      ..strokeWidth = 4.0;
    canvas.drawLine(Offset(center, 0), Offset(center, size.y), spokePaint);
    canvas.drawLine(Offset(0, center), Offset(size.x, center), spokePaint);

    // Center pivot shaft
    final shaftPaint = Paint()
      ..color = const Color(0xFF1E293B)
      ..style = PaintingStyle.fill;
    canvas.drawCircle(Offset(center, center), 8.0, shaftPaint);
    canvas.drawCircle(Offset(center, center), 8.0, borderPaint);

    // 3. Force Vector on Left Edge pointing down
    _drawVectorArrow(canvas, Offset(0, center), Offset(0, center + 40), "Force", Colors.redAccent);

    // Text label
    final textPainter = TextPainter(
      text: const TextSpan(
        text: 'Valve Wheel',
        style: TextStyle(color: Colors.white, fontSize: 10.0, fontWeight: FontWeight.bold, fontFamily: 'Poppins'),
      ),
      textDirection: TextDirection.ltr,
    );
    textPainter.layout();
    textPainter.paint(canvas, Offset(center - textPainter.width / 2, -18.0));
  }

  bool isSolved() {
    return game.zoneValve?.isFilled ?? false;
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
    path.lineTo(end.dx - 5, end.dy - headSize);
    path.lineTo(end.dx + 5, end.dy - headSize);
    path.close();
    canvas.drawPath(path, headPaint);

    final textPainter = TextPainter(
      text: TextSpan(text: forceLabel, style: TextStyle(color: color, fontSize: 9.0, fontWeight: FontWeight.bold, fontFamily: 'Poppins')),
      textDirection: TextDirection.ltr,
    );
    textPainter.layout();
    textPainter.paint(canvas, Offset(end.dx - 35, end.dy - 20));
  }
}

// =========================================================================
// 4. Scenario 3: The Seesaw Component
// =========================================================================

class SeesawComponent extends PositionComponent with HasGameRef<TurningEffectGame> {
  late SeesawPlankComponent board;
  late SeesawPivotComponent pivot;

  SeesawComponent() {
    size = Vector2(240.0, 60.0);
    anchor = Anchor.center;

    // Pivot support triangle
    pivot = SeesawPivotComponent();
    add(pivot);

    // Balanced seesaw plank board
    board = SeesawPlankComponent();
    add(board);
  }

  void triggerReaction() {
    board.isBalancing = true;
  }

  @override
  void onGameResize(Vector2 size) {
    super.onGameResize(size);
    // Align seesaw board to pivot apex
    board.position = Vector2(size.x / 2, size.y / 2);
    pivot.position = Vector2(size.x / 2, size.y / 2);
  }
}

class SeesawPivotComponent extends PositionComponent {
  SeesawPivotComponent() {
    size = Vector2(30.0, 30.0);
    anchor = Anchor.topCenter;
  }

  @override
  void render(Canvas canvas) {
    final path = Path();
    path.moveTo(size.x / 2, 0); // Apex
    path.lineTo(0, size.y); // bottom left
    path.lineTo(size.x, size.y); // bottom right
    path.close();

    final paint = Paint()
      ..color = const Color(0xFF475569)
      ..style = PaintingStyle.fill;
    canvas.drawPath(path, paint);

    final border = Paint()
      ..color = Colors.blueGrey.shade300
      ..strokeWidth = 1.8
      ..style = PaintingStyle.stroke;
    canvas.drawPath(path, border);
  }
}

class SeesawPlankComponent extends PositionComponent with HasGameRef<TurningEffectGame> {
  bool isBalancing = false;
  double balanceSpeed = 0.3; // radians per sec

  SeesawPlankComponent() {
    size = Vector2(240.0, 10.0);
    anchor = Anchor.center;
    angle = -0.15; // Start tilted left due to unequal moments
  }

  @override
  void update(double dt) {
    super.update(dt);
    if (isBalancing) {
      if (angle < 0.0) {
        angle += balanceSpeed * dt;
        if (angle >= 0.0) {
          angle = 0.0;
          isBalancing = false;
        }
      }
    }
  }

  @override
  void render(Canvas canvas) {
    final rect = size.toRect();
    final boardPaint = Paint()
      ..color = const Color(0xFF64748B)
      ..style = PaintingStyle.fill;
    canvas.drawRect(rect, boardPaint);

    final borderPaint = Paint()
      ..color = isSolved() ? Colors.greenAccent : Colors.blueGrey.shade300
      ..strokeWidth = 2.0
      ..style = PaintingStyle.stroke;
    canvas.drawRect(rect, borderPaint);

    // 1. Left weight box (100N at 2m)
    final double leftX = -size.x / 2 + 30;
    final boxPaint = Paint()
      ..color = Colors.blue.shade900
      ..style = PaintingStyle.fill;
    canvas.drawRect(Rect.fromCenter(center: Offset(30, -10), width: 30, height: 20), boxPaint);
    canvas.drawRect(Rect.fromCenter(center: Offset(30, -10), width: 30, height: 20), borderPaint);

    final text1 = TextPainter(
      text: const TextSpan(text: "100N\n(2m)", style: TextStyle(color: Colors.white, fontSize: 7.0, fontWeight: FontWeight.bold, fontFamily: 'Poppins')),
      textDirection: TextDirection.ltr,
      textAlign: TextAlign.center,
    );
    text1.layout();
    text1.paint(canvas, Offset(18, -17));

    // 2. Right distance marker (4m)
    final text2 = TextPainter(
      text: const TextSpan(text: "Dist: 4m", style: TextStyle(color: Colors.white, fontSize: 9.0, fontWeight: FontWeight.bold, fontFamily: 'Poppins')),
      textDirection: TextDirection.ltr,
    );
    text2.layout();
    text2.paint(canvas, Offset(size.x - 60, -22));
  }

  bool isSolved() {
    return game.zoneSeesaw?.isFilled ?? false;
  }
}

// =========================================================================
// 5. Drop Zone Component
// =========================================================================

class DropZoneComponent extends PositionComponent with HasGameRef<TurningEffectGame> {
  final int scenarioId;
  bool isFilled = false;

  DropZoneComponent({required this.scenarioId}) {
    size = Vector2(105.0, 36.0);
    anchor = Anchor.center;
  }

  @override
  void render(Canvas canvas) {
    // Hidden target overlay on seesaw board if rotating, visible box for spanner/valve
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

class DraggableAnswerComponent extends PositionComponent with DragCallbacks, HasGameRef<TurningEffectGame> {
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
          fontSize: 10.5,
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

class TurningEffectScreen extends StatefulWidget {
  const TurningEffectScreen({super.key});

  @override
  State<TurningEffectScreen> createState() => _TurningEffectScreenState();
}

class _TurningEffectScreenState extends State<TurningEffectScreen> {
  late TurningEffectGame game;
  bool _showBanner = false;

  @override
  void initState() {
    super.initState();
    game = TurningEffectGame(
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
      title = "Spanner Moment Calculation";
      content = "To calculate the moment (turning effect), multiply the force by the perpendicular distance from the pivot.\n\n"
          "Formula:\n"
          "Moment = Force × Distance\n"
          "Moment = 10 N × 5 m = 50 Nm\n\n"
          "Since the force pulls downward on the right side of the pivot, it rotates in a Clockwise direction!";
    } else if (scenarioId == 2) {
      title = "Valve Rotational Direction";
      content = "Look at the direction of the force vector on the left edge.\n\n"
          "A downward force pulling on the left side of a circular valve wheel causes the wheel to turn towards the left (Anti-clockwise direction).";
    } else if (scenarioId == 3) {
      title = "Principle of Moments";
      content = "For an object to balance in rotational equilibrium, the sum of clockwise moments must equal the sum of anti-clockwise moments.\n\n"
          "Equation:\n"
          "Clockwise Moment = Anti-clockwise Moment\n"
          "F_right × d_right = F_left × d_left\n"
          "Force × 4m = 100N × 2m\n"
          "Force = 200 / 4 = 50 N\n\n"
          "Placing a 50N force on the right side brings the seesaw back to a horizontal balance!";
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
                "Theory Mastered!",
                style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
              ),
            ),
          ],
        ),
        content: const Text(
          "Congratulations! You solved all moments calculations:\n\n"
          "• Spanner: 10N × 5m = 50 Nm Clockwise\n"
          "• Valve: Downward left edge = Anti-clockwise\n"
          "• Seesaw: 100N × 2m = 50N × 4m Balanced",
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

          // 2. Header Box
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
                          "TURNING EFFECT LAB",
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
                        _buildRuleBadge(isSmallScreen ? "Moment = F × d" : "Moment = Force × Distance", Colors.blue.shade100, isSmallScreen),
                        _buildRuleBadge(isSmallScreen ? "Equil: CW = ACW" : "Equilibrium: Clockwise = Anti-Clockwise", Colors.orange.shade100, isSmallScreen),
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
                          "Incorrect! Hint: Multiply Force by Perpendicular Distance, or check the Principle of Moments (100x2 = Fx4).",
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
