import 'dart:math';
import 'package:flutter/material.dart';
import 'package:flame/game.dart';
import 'package:flame/components.dart';
import 'package:flame/events.dart';

// =========================================================================
// 1. Resultant Force Game Class
// =========================================================================

class ResultantForceGame extends FlameGame with DragCallbacks {
  final VoidCallback? onIncorrectDrop;
  final ValueChanged<int>? onCorrectDrop;
  final VoidCallback? onLevelComplete;

  // Track state
  List<ObjectBlockComponent> objectBlocks = [];
  List<DropZoneComponent> dropZones = [];
  List<DraggableAnswerComponent> draggables = [];

  ResultantForceGame({this.onIncorrectDrop, this.onCorrectDrop, this.onLevelComplete});

  @override
  Future<void> onLoad() async {
    super.onLoad();
    setupGameElements();
  }

  @override
  void onGameResize(Vector2 size) {
    super.onGameResize(size);
    // Reposition components dynamically when layout size changes
    rebuildLayout();
  }

  void setupGameElements() {
    // Clear pre-existing components
    objectBlocks.forEach((e) => e.removeFromParent());
    dropZones.forEach((e) => e.removeFromParent());
    draggables.forEach((e) => e.removeFromParent());
    
    objectBlocks.clear();
    dropZones.clear();
    draggables.clear();

    // 1. Create Target Scenarios (Blocks and Drop Zones)
    for (int i = 1; i <= 3; i++) {
      final block = ObjectBlockComponent(scenarioId: i);
      objectBlocks.add(block);
      add(block);

      final zone = DropZoneComponent(scenarioId: i);
      dropZones.add(zone);
      add(zone);
    }

    // 2. Create Draggables (Bottom)
    final answers = [
      {'id': 1, 'label': '15N Right'},
      {'id': 2, 'label': '15N Left'},
      {'id': 3, 'label': '0N (Balanced)'},
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
    if (objectBlocks.length < 3 || dropZones.length < 3 || draggables.length < 3) return;

    final double theoryHeight = 110.0;
    final double bottomAreaHeight = 110.0;
    final double availableHeight = size.y - theoryHeight - bottomAreaHeight;

    // Responsive Object Block positions stacked vertically
    final double ySpacing = availableHeight / 4;

    for (int i = 0; i < 3; i++) {
      final double blockY = theoryHeight + ySpacing * (i + 1);
      final double centerX = size.x / 2;

      // Position Block
      objectBlocks[i].position = Vector2(centerX, blockY);
      objectBlocks[i].initialX = centerX;

      // Position Drop Zone directly below the Block
      dropZones[i].position = Vector2(centerX, blockY + 54);
    }

    // Position Draggables Horizontally at the bottom
    final double dragY = size.y - 60.0;
    final double xSpacing = size.x / 4;

    for (int i = 0; i < 3; i++) {
      final double dragX = xSpacing * (i + 1);
      draggables[i].homePosition = Vector2(dragX, dragY);
      
      if (!draggables[i].isLocked) {
        draggables[i].position = Vector2(dragX, dragY);
      } else {
        // If locked, snap exactly to its filled drop zone position
        final zone = dropZones.firstWhere((z) => z.scenarioId == draggables[i].answerId);
        draggables[i].position = zone.position.clone();
      }
    }
  }

  void checkSnapping(DraggableAnswerComponent draggable) {
    DropZoneComponent? targetZone;
    double minDistance = double.infinity;

    for (final zone in dropZones) {
      if (zone.isFilled) continue;
      double dist = draggable.position.distanceTo(zone.position);
      if (dist < minDistance) {
        minDistance = dist;
        targetZone = zone;
      }
    }

    // Snapping radius of 50 pixels
    if (targetZone != null && minDistance < 50.0) {
      // Validate correct match
      bool isCorrect = false;
      if (targetZone.scenarioId == 1 && draggable.answerId == 1) isCorrect = true;
      if (targetZone.scenarioId == 2 && draggable.answerId == 2) isCorrect = true;
      if (targetZone.scenarioId == 3 && draggable.answerId == 3) isCorrect = true;

      if (isCorrect) {
        // Lock answer inside Drop Zone
        draggable.position = targetZone.position.clone();
        draggable.isLocked = true;
        draggable.color = const Color(0xFF10B981); // Emerald Green
        targetZone.isFilled = true;

        // Trigger corresponding physics reactions
        final block = objectBlocks.firstWhere((b) => b.scenarioId == targetZone!.scenarioId);
        block.triggerReaction();

        onCorrectDrop?.call(targetZone.scenarioId);

        _checkLevelCompletion();
      } else {
        // Snap back to home and trigger hint banner in UI
        draggable.isSnappingBack = true;
        onIncorrectDrop?.call();
      }
    } else {
      // Dragged in wild, snap back
      draggable.isSnappingBack = true;
    }
  }

  void _checkLevelCompletion() {
    final allSolved = dropZones.every((zone) => zone.isFilled);
    if (allSolved) {
      // Delay complete popup briefly to let physics reaction complete
      Future.delayed(const Duration(milliseconds: 1200), () {
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
// 2. Object Mass Component (Target Zones)
// =========================================================================

class ObjectBlockComponent extends PositionComponent with HasGameRef<ResultantForceGame> {
  final int scenarioId;

  // Animation values
  double initialX = 0.0;
  bool isMovingRight = false;
  bool isMovingLeft = false;
  bool isShaking = false;
  double shakeTime = 0.0;
  double shakeOffset = 0.0;
  double animationSpeed = 260.0; // pixels per sec

  ObjectBlockComponent({required this.scenarioId}) {
    size = Vector2(85.0, 42.0);
    anchor = Anchor.center;
  }

  void triggerReaction() {
    if (scenarioId == 1) {
      isMovingRight = true;
    } else if (scenarioId == 2) {
      isMovingLeft = true;
    } else if (scenarioId == 3) {
      isShaking = true;
    }
  }

  @override
  void update(double dt) {
    super.update(dt);

    if (isMovingRight) {
      position.x += animationSpeed * dt;
      // Wrap-around or just let it slide off-screen
    } else if (isMovingLeft) {
      position.x -= animationSpeed * dt;
    } else if (isShaking) {
      shakeTime += dt;
      shakeOffset = sin(shakeTime * 70.0) * 2.5; // vibrations
    }
  }

  @override
  void render(Canvas canvas) {
    canvas.save();
    
    // Apply shake translation if active
    if (isShaking) {
      canvas.translate(shakeOffset, 0);
    }

    final rect = size.toRect();
    final blockPaint = Paint()
      ..color = const Color(0xFF334155) // Slate Dark
      ..style = PaintingStyle.fill;
    
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(8.0)), blockPaint);

    final borderPaint = Paint()
      ..color = isSolved() ? Colors.greenAccent : Colors.blueGrey.shade300
      ..strokeWidth = 2.5
      ..style = PaintingStyle.stroke;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(8.0)), borderPaint);

    // Draw Mass Label
    final painter = TextPainter(
      text: TextSpan(
        text: 'Object $scenarioId',
        style: const TextStyle(
          color: Colors.white,
          fontSize: 12.0,
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

    // 4. Force Vectors Arrows Drawing
    _drawArrows(canvas);

    canvas.restore();
  }

  bool isSolved() {
    if (game.dropZones.length < scenarioId) return false;
    return game.dropZones[scenarioId - 1].isFilled;
  }

  void _drawArrows(Canvas canvas) {
    // Left edge and right edge offsets
    final double leftX = 0;
    final double rightX = size.x;
    final double centerY = size.y / 2;

    final double baseScale = game.size.x < 380 ? 0.8 : 1.0;

    if (scenarioId == 1) {
      // Scenario 1: Two forces pointing Right (10N and 5N)
      // Stack arrows vertically
      _drawVectorArrow(canvas, Offset(rightX, centerY - 10), Offset(rightX + 65.0 * baseScale, centerY - 10), "10N", Colors.redAccent);
      _drawVectorArrow(canvas, Offset(rightX, centerY + 10), Offset(rightX + 45.0 * baseScale, centerY + 10), "5N", Colors.redAccent);
    } else if (scenarioId == 2) {
      // Scenario 2: One force Left (20N), one Right (5N)
      _drawVectorArrow(canvas, Offset(leftX, centerY), Offset(leftX - 85.0 * baseScale, centerY), "20N", Colors.redAccent);
      _drawVectorArrow(canvas, Offset(rightX, centerY), Offset(rightX + 45.0 * baseScale, centerY), "5N", Colors.redAccent);
    } else if (scenarioId == 3) {
      // Scenario 3: Balanced forces Left (10N) and Right (10N)
      _drawVectorArrow(canvas, Offset(leftX, centerY), Offset(leftX - 65.0 * baseScale, centerY), "10N", Colors.redAccent);
      _drawVectorArrow(canvas, Offset(rightX, centerY), Offset(rightX + 65.0 * baseScale, centerY), "10N", Colors.redAccent);
    }
  }

  void _drawVectorArrow(Canvas canvas, Offset start, Offset end, String forceLabel, Color color) {
    final double headSize = 10.0;
    final paint = Paint()
      ..color = color
      ..strokeWidth = 3.5
      ..style = PaintingStyle.stroke
      ..strokeCap = StrokeCap.round;

    // Draw shaft
    canvas.drawLine(start, end, paint);

    // Calculate heading vector
    final dx = end.dx - start.dx;
    final dy = end.dy - start.dy;
    final len = sqrt(dx * dx + dy * dy);
    if (len < 0.5) return;
    
    final ux = dx / len;
    final uy = dy / len;
    
    final px = -uy;
    final py = ux;

    // Draw arrowhead
    final headPaint = Paint()
      ..color = color
      ..style = PaintingStyle.fill;
    
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

    // Render N Label Text above arrow shaft
    final textPainter = TextPainter(
      text: TextSpan(
        text: forceLabel,
        style: TextStyle(
          color: color,
          fontSize: 10.5,
          fontWeight: FontWeight.bold,
          fontFamily: 'Poppins',
        ),
      ),
      textDirection: TextDirection.ltr,
    );
    textPainter.layout();
    
    // Position text in center of shaft, slightly raised
    final textX = start.dx + dx / 2 - textPainter.width / 2;
    final textY = start.dy + dy / 2 - 15;
    textPainter.paint(canvas, Offset(textX, textY));
  }
}

// =========================================================================
// 3. Drop Zone Flame Component
// =========================================================================

class DropZoneComponent extends PositionComponent with HasGameRef<ResultantForceGame> {
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

    // Draw small target text helper if empty
    if (!isFilled) {
      final painter = TextPainter(
        text: TextSpan(
          text: 'Drop Answer $scenarioId',
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
// 4. Draggable Answer Component
// =========================================================================

class DraggableAnswerComponent extends PositionComponent with DragCallbacks, HasGameRef<ResultantForceGame> {
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
      position.lerp(homePosition, 0.16); // smooth elastic snapping
      if (position.distanceTo(homePosition) < 1.0) {
        position = homePosition.clone();
        isSnappingBack = false;
      }
    }
  }

  @override
  void render(Canvas canvas) {
    final rect = size.toRect();
    
    // Shadow / Background Glow on locking
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
          fontSize: 11.5,
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
    priority = 100; // top layer
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
// 5. Flutter Main Screen Screen Widget
// =========================================================================

class ResultantForceScreen extends StatefulWidget {
  const ResultantForceScreen({super.key});

  @override
  State<ResultantForceScreen> createState() => _ResultantForceScreenState();
}

class _ResultantForceScreenState extends State<ResultantForceScreen> {
  late ResultantForceGame game;
  bool _showBanner = false;

  @override
  void initState() {
    super.initState();
    game = ResultantForceGame(
      onIncorrectDrop: _handleIncorrectDrop,
      onCorrectDrop: _showCorrectTheoryDialog,
      onLevelComplete: _handleLevelComplete,
    );
  }

  void _handleIncorrectDrop() {
    setState(() {
      _showBanner = true;
    });

    // Auto-dismiss hint banner after 3.5 seconds
    Future.delayed(const Duration(milliseconds: 3500), () {
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
      title = "Same Direction: Add Forces";
      content = "When multiple forces act on an object in the same direction, their magnitudes add together.\n\n"
          "Formula:\n"
          "F_net = F1 + F2\n"
          "F_net = 10 N + 5 N = 15 N (Right)\n\n"
          "The net force accelerates the object to the right!";
    } else if (scenarioId == 2) {
      title = "Opposite Directions: Subtract Forces";
      content = "When forces act in opposite directions, the resultant force is calculated by subtracting the smaller force from the larger force. The direction matches the larger force.\n\n"
          "Formula:\n"
          "F_net = F_left - F_right\n"
          "F_net = 20 N - 5 N = 15 N (Left)\n\n"
          "The net force accelerates the object to the left!";
    } else if (scenarioId == 3) {
      title = "Equal & Opposite: Balanced Forces";
      content = "When two forces of equal magnitude act in opposite directions, they cancel each other out. The net resultant force is exactly 0 N.\n\n"
          "Formula:\n"
          "F_net = F_left - F_right\n"
          "F_net = 10 N - 10 N = 0 N (Balanced)\n\n"
          "The object is in equilibrium and remains stationary!";
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
            Icon(Icons.emoji_events, color: Colors.yellowAccent, size: 30),
            SizedBox(width: 10),
            Text(
              "Level Complete!",
              style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
            ),
          ],
        ),
        content: const Text(
          "Great job! You have calculated all the resultant forces correctly:\n\n"
          "• Scenario 1: 10N + 5N = 15N Right\n"
          "• Scenario 2: 20N - 5N = 15N Left\n"
          "• Scenario 3: 10N - 10N = 0N Balanced",
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

          // 2. Top Header and Back Navigation
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
                          "RESULTANT FORCE LAB",
                          style: TextStyle(
                            color: Colors.white,
                            fontSize: isSmallScreen ? 14.0 : 16.0,
                            fontWeight: FontWeight.w800,
                            letterSpacing: 1.0,
                          ),
                        ),
                      ),
                    ),
                    const SizedBox(width: 48), // spacer balance
                  ],
                ),
              ),
            ),
          ),

          // 3. Theory Board (Dynamic alignment below Header)
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
                        _buildRuleBadge(isSmallScreen ? "Same: Add" : "Same Dir: Add (F1+F2)", Colors.blue.shade100, isSmallScreen),
                        _buildRuleBadge(isSmallScreen ? "Opposite: Sub" : "Opposite: Sub (F1-F2)", Colors.orange.shade100, isSmallScreen),
                        _buildRuleBadge(isSmallScreen ? "Balanced: 0N" : "Balanced: (0N)", Colors.green.shade100, isSmallScreen),
                      ],
                    ),
                  ],
                ),
              ),
            ),
          ),

          // 4. Temporary Validation Warning Banner (Snackbar overlay)
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
                          "Incorrect! Hint: Check the direction of the arrows and refer to the Theory Board.",
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
