import 'dart:math';
import 'package:flame/components.dart';
import 'package:flame/events.dart';
import 'package:flame/game.dart';
import 'package:flutter/material.dart';

// ==========================================
// 1. Particle Component for Rocket Exhaust
// ==========================================
class ExhaustParticle extends PositionComponent {
  final Vector2 velocity;
  final double maxLife = 0.5;
  double life = 0.0;
  final Paint _paint = Paint()..color = Colors.amber;

  ExhaustParticle({
    required Vector2 position,
    required this.velocity,
  }) : super(position: position, size: Vector2.all(8));

  @override
  void update(double dt) {
    super.update(dt);
    life += dt;
    position += velocity * dt;
    _paint.color = Colors.amber.withOpacity(max(0, 1 - (life / maxLife)));
    if (life >= maxLife) {
      removeFromParent();
    }
  }

  @override
  void render(Canvas canvas) {
    canvas.drawCircle(Offset(size.x / 2, size.y / 2), size.x / 2, _paint);
  }
}

// ==========================================
// 2. Law Drop Zone Component
// ==========================================
class LawDropZone extends PositionComponent {
  final int lawIndex;
  final String title;
  final Color baseColor;
  bool isMatched = false;

  late TextPainter _textPainter;

  LawDropZone({
    required this.lawIndex,
    required this.title,
    required this.baseColor,
    required Vector2 position,
    required Vector2 size,
  }) : super(position: position, size: size);

  @override
  void onMount() {
    super.onMount();
    _textPainter = TextPainter(
      textDirection: TextDirection.ltr,
      textAlign: TextAlign.center,
    );
  }

  @override
  void render(Canvas canvas) {
    final paint = Paint()
      ..style = PaintingStyle.stroke
      ..strokeWidth = isMatched ? 4.0 : 2.5;

    if (isMatched) {
      paint.color = Colors.greenAccent;
      // Draw solid filled background on match
      final bgPaint = Paint()..color = Colors.green.withOpacity(0.1);
      canvas.drawRRect(RRect.fromRectAndRadius(size.toRect(), const Radius.circular(12)), bgPaint);
    } else {
      paint.color = baseColor;
    }

    // Draw dashed border or standard rounded rect border
    final rrect = RRect.fromRectAndRadius(size.toRect(), const Radius.circular(12));
    canvas.drawRRect(rrect, paint);

    // Draw Title Text
    _textPainter.text = TextSpan(
      text: title,
      style: TextStyle(
        color: isMatched ? Colors.greenAccent : Colors.white,
        fontWeight: FontWeight.bold,
        fontSize: 14,
      ),
    );
    _textPainter.layout(maxWidth: size.x - 10);
    _textPainter.paint(
      canvas,
      Offset(
        (size.x - _textPainter.width) / 2,
        (size.y - _textPainter.height) / 2,
      ),
    );
  }
}

// ==========================================
// 3. Draggable Scenario Component
// ==========================================
enum ScenarioType { rocket, puck, skateboarder }

class DraggableScenario extends PositionComponent with DragCallbacks, HasGameRef<NewtonGame> {
  final ScenarioType type;
  final int correctZoneIndex;
  final String label;

  final Vector2 originalPos;
  bool isDraggingNow = false;
  bool isMatchingCorrect = false;

  // Shake state variables
  double _shakeTimer = 0.0;
  final double _shakeDuration = 0.5;
  Vector2 _shakeOffset = Vector2.zero();

  // Animation values
  double _animTime = 0.0;
  Vector2 _animVelocity = Vector2.zero();

  // Skateboarder rider sub-component variables
  bool _isAnimatingReaction = false;
  bool _skateboardStopped = false;
  Vector2 _riderOffset = Vector2.zero();
  Vector2 _riderVelocity = Vector2.zero();
  double _riderOpacity = 1.0;

  DraggableScenario({
    required this.type,
    required this.correctZoneIndex,
    required this.label,
    required this.originalPos,
    required Vector2 size,
  }) : super(position: originalPos.clone(), size: size, anchor: Anchor.center);

  @override
  void update(double dt) {
    super.update(dt);

    // 1. Shake animation update
    if (_shakeTimer > 0) {
      _shakeTimer -= dt;
      if (_shakeTimer <= 0) {
        _shakeOffset = Vector2.zero();
        position.setFrom(originalPos);
      } else {
        final randomVal = (sin(_shakeTimer * 50) * 10);
        _shakeOffset = Vector2(randomVal, 0);
        position.setFrom(originalPos + _shakeOffset);
      }
    }

    // 2. Physics animation update
    if (_isAnimatingReaction) {
      _animTime += dt;
      if (type == ScenarioType.rocket) {
        // Rocket Reaction: Move rapidly upwards
        const double acceleration = 600.0;
        _animVelocity.y -= acceleration * dt;
        position += _animVelocity * dt;

        // Spawn exhaust particles
        final exhaustPos = position + Vector2(0, size.y / 2);
        final randomAngle = (Random().nextDouble() - 0.5) * 0.4;
        final particleVel = Vector2(sin(randomAngle), 1).normalized() * (100.0 + Random().nextDouble() * 100);
        gameRef.add(ExhaustParticle(position: exhaustPos, velocity: particleVel));

        // Finish check
        if (position.y < -size.y) {
          _isAnimatingReaction = false;
          gameRef.onReactionComplete(this);
        }
      } else if (type == ScenarioType.puck) {
        // Puck Reaction: Move horizontally with constant acceleration
        const double acceleration = 400.0; // F = ma concept
        _animVelocity.x += acceleration * dt;
        position += _animVelocity * dt;

        // Finish check
        if (position.x > gameRef.size.x + size.x) {
          _isAnimatingReaction = false;
          gameRef.onReactionComplete(this);
        }
      } else if (type == ScenarioType.skateboarder) {
        // Skateboarder Reaction
        if (!_skateboardStopped) {
          // Both move forward together
          const double initialSpeed = 250.0;
          position.x += initialSpeed * dt;

          // Stop at the center of the screen
          if (position.x >= gameRef.size.x / 2) {
            position.x = gameRef.size.x / 2;
            _skateboardStopped = true;
            // Launch rider forward with inertial speed
            _riderVelocity = Vector2(initialSpeed * 1.5, -200); // Projectile launch
          }
        } else {
          // Skateboard stopped. Animate rider flying forward with gravity
          _riderVelocity.y += 600 * dt; // gravity force pulls rider down
          _riderOffset += _riderVelocity * dt;
          _riderOpacity = max(0.0, _riderOpacity - dt * 1.2); // fade out

          if (_riderOpacity <= 0.0) {
            _isAnimatingReaction = false;
            gameRef.onReactionComplete(this);
          }
        }
      }
    }
  }

  void startShake() {
    _shakeTimer = _shakeDuration;
  }

  void startReactionAnimation() {
    _isAnimatingReaction = true;
    _animTime = 0.0;
    _animVelocity = Vector2.zero();
    _skateboardStopped = false;
    _riderOffset = Vector2.zero();
    _riderOpacity = 1.0;
    position.setFrom(gameRef.size / 2); // Center of the screen
  }

  @override
  void onDragStart(DragStartEvent event) {
    if (isMatchingCorrect || _isAnimatingReaction) return;
    super.onDragStart(event);
    isDraggingNow = true;
    priority = 100; // Bring to front
  }

  @override
  void onDragUpdate(DragUpdateEvent event) {
    if (!isDraggingNow) return;
    position += event.localDelta;
  }

  @override
  void onDragEnd(DragEndEvent event) {
    if (!isDraggingNow) return;
    isDraggingNow = false;
    priority = 1;

    // Check overlap with correct and incorrect zones
    LawDropZone? matchedZone;
    double maxOverlap = 0.0;

    for (final child in gameRef.children) {
      if (child is LawDropZone) {
        // Simple bounding box intersection
        final rectA = toRect();
        final rectB = child.toRect();
        final overlapRect = rectA.intersect(rectB);
        if (overlapRect.width > 0 && overlapRect.height > 0) {
          final area = overlapRect.width * overlapRect.height;
          if (area > maxOverlap) {
            maxOverlap = area;
            matchedZone = child;
          }
        }
      }
    }

    if (matchedZone != null) {
      if (matchedZone.lawIndex == correctZoneIndex) {
        // Success
        isMatchingCorrect = true;
        position.setFrom(matchedZone.position + matchedZone.size / 2);
        matchedZone.isMatched = true;
        gameRef.onSuccessfulDrop(this);
      } else {
        // Wrong zone
        startShake();
        gameRef.onFailedDrop();
      }
    } else {
      // Return smoothly to original position
      position.setFrom(originalPos);
    }
  }

  @override
  void render(Canvas canvas) {
    if (type == ScenarioType.rocket) {
      _renderRocket(canvas);
    } else if (type == ScenarioType.puck) {
      _renderPuck(canvas);
    } else if (type == ScenarioType.skateboarder) {
      _renderSkateboarder(canvas);
    }
  }

  // Draw Space Rocket
  void _renderRocket(Canvas canvas) {
    final paintBody = Paint()..color = Colors.redAccent;
    final paintFin = Paint()..color = Colors.blueGrey;
    final paintWindow = Paint()..color = Colors.cyan;

    // Draw Rocket Body (Rectangle)
    final bodyRect = Rect.fromLTWH(-15, -10, 30, 40);
    canvas.drawRect(bodyRect, paintBody);

    // Draw Rocket Nose (Triangle)
    final nosePath = Path()
      ..moveTo(0, -35)
      ..lineTo(-15, -10)
      ..lineTo(15, -10)
      ..close();
    canvas.drawPath(nosePath, paintBody);

    // Draw Rocket Fins
    final leftFin = Path()
      ..moveTo(-15, 15)
      ..lineTo(-25, 30)
      ..lineTo(-15, 30)
      ..close();
    canvas.drawPath(leftFin, paintFin);

    final rightFin = Path()
      ..moveTo(15, 15)
      ..lineTo(25, 30)
      ..lineTo(15, 30)
      ..close();
    canvas.drawPath(rightFin, paintFin);

    // Draw Rocket Window
    canvas.drawCircle(const Offset(0, 5), 6, paintWindow);

    // Draw Label Text
    _renderLabelText(canvas, Offset(0, 45));
  }

  // Draw Physics Puck
  void _renderPuck(Canvas canvas) {
    final paintCircle = Paint()..color = Colors.green;
    final paintInner = Paint()..color = Colors.greenAccent;

    // Draw puck main body
    canvas.drawCircle(Offset.zero, 25, paintCircle);
    canvas.drawCircle(Offset.zero, 18, paintInner);

    // Label Text
    _renderLabelText(canvas, Offset(0, 38));
  }

  // Draw Skateboarder
  void _renderSkateboarder(Canvas canvas) {
    // 1. Skateboard base rendering
    final paintBoard = Paint()..color = Colors.grey[700]!..strokeWidth = 5;
    final paintWheel = Paint()..color = Colors.grey[400]!;

    if (!_skateboardStopped) {
      // Skateboard
      canvas.drawLine(const Offset(-20, 15), const Offset(20, 15), paintBoard);
      // Wheels
      canvas.drawCircle(const Offset(-12, 22), 5, paintWheel);
      canvas.drawCircle(const Offset(12, 22), 5, paintWheel);

      // Rider (Blue square)
      final paintRider = Paint()..color = Colors.blueAccent;
      canvas.drawRect(Rect.fromCenter(center: const Offset(0, -10), width: 24, height: 32), paintRider);
      // Head
      canvas.drawCircle(const Offset(0, -32), 6, paintRider);
    } else {
      // Draw board only (Stopped)
      canvas.drawLine(const Offset(-20, 15), const Offset(20, 15), paintBoard);
      canvas.drawCircle(const Offset(-12, 22), 5, paintWheel);
      canvas.drawCircle(const Offset(12, 22), 5, paintWheel);

      // Draw flying rider with applied offset and fade opacity
      final paintRider = Paint()..color = Colors.blueAccent.withOpacity(_riderOpacity);
      canvas.save();
      canvas.translate(_riderOffset.x, _riderOffset.y);
      canvas.drawRect(Rect.fromCenter(center: const Offset(0, -10), width: 24, height: 32), paintRider);
      canvas.drawCircle(const Offset(0, -32), 6, paintRider);
      canvas.restore();
    }

    _renderLabelText(canvas, Offset(0, 42));
  }

  void _renderLabelText(Canvas canvas, Offset offset) {
    final textPainter = TextPainter(
      text: TextSpan(
        text: label,
        style: const TextStyle(
          color: Colors.white,
          fontSize: 10,
          fontWeight: FontWeight.bold,
          shadows: [Shadow(color: Colors.black, blurRadius: 4, offset: Offset(1, 1))],
        ),
      ),
      textDirection: TextDirection.ltr,
      textAlign: TextAlign.center,
    );
    textPainter.layout(maxWidth: 90);
    textPainter.paint(canvas, Offset(offset.dx - textPainter.width / 2, offset.dy));
  }
}

// ==========================================
// 4. Flame Game Engine Instance
// ==========================================
class NewtonGame extends FlameGame with DragCallbacks {
  final void Function(DraggableScenario item) onTriggerCheckpoint;
  final void Function(String message, Color color) onShowMessage;

  NewtonGame({
    required this.onTriggerCheckpoint,
    required this.onShowMessage,
  });

  Set<ScenarioType> completedScenarios = {};

  @override
  Color backgroundColor() => const Color(0xFF0F172A); // Premium Slate Dark Background

  @override
  void onLoad() {
    super.onLoad();

    // Responsive positioning of zones
    final double zoneWidth = size.x * 0.28;
    final double zoneHeight = 110.0;
    final double gap = (size.x - (zoneWidth * 3)) / 4;
    final double topMargin = 50.0;

    // Add 3 Target Drop Zones
    add(LawDropZone(
      lawIndex: 1,
      title: "1st Law\n(Inertia)",
      baseColor: Colors.blueAccent,
      position: Vector2(gap, topMargin),
      size: Vector2(zoneWidth, zoneHeight),
    ));

    add(LawDropZone(
      lawIndex: 2,
      title: "2nd Law\n(F = ma)",
      baseColor: Colors.greenAccent,
      position: Vector2(gap * 2 + zoneWidth, topMargin),
      size: Vector2(zoneWidth, zoneHeight),
    ));

    add(LawDropZone(
      lawIndex: 3,
      title: "3rd Law\n(Action/React)",
      baseColor: Colors.orangeAccent,
      position: Vector2(gap * 3 + zoneWidth * 2, topMargin),
      size: Vector2(zoneWidth, zoneHeight),
    ));

    // Spawn 3 Draggable Scenarios initially at bottom
    final double startY = size.y - 120.0;
    final double itemGap = size.x / 4;

    add(DraggableScenario(
      type: ScenarioType.skateboarder,
      correctZoneIndex: 1,
      label: "Skateboarder",
      originalPos: Vector2(itemGap, startY),
      size: Vector2(80, 80),
    ));

    add(DraggableScenario(
      type: ScenarioType.puck,
      correctZoneIndex: 2,
      label: "Physics Puck\n(F=20N, M=4kg)",
      originalPos: Vector2(itemGap * 2, startY),
      size: Vector2(80, 80),
    ));

    add(DraggableScenario(
      type: ScenarioType.rocket,
      correctZoneIndex: 3,
      label: "Space Rocket",
      originalPos: Vector2(itemGap * 3, startY),
      size: Vector2(80, 80),
    ));
  }

  void onSuccessfulDrop(DraggableScenario item) {
    onTriggerCheckpoint(item);
  }

  void onFailedDrop() {
    onShowMessage("Try Again! Wrong Zone Match.", Colors.redAccent);
  }

  void handleCheckpointSuccess(DraggableScenario item) {
    // Hide item from the top zone
    item.removeFromParent();

    // Recreate a duplicate in the center of the screen to animate the physical reaction
    final animItem = DraggableScenario(
      type: item.type,
      correctZoneIndex: item.correctZoneIndex,
      label: item.label,
      originalPos: size / 2,
      size: Vector2(90, 90),
    );
    add(animItem);

    // Start reaction
    animItem.startReactionAnimation();
  }

  void onReactionComplete(DraggableScenario animItem) {
    animItem.removeFromParent();
    completedScenarios.add(animItem.type);

    String successMessage = "";
    if (animItem.type == ScenarioType.rocket) {
      successMessage = "Correct! Equal and opposite reaction!";
    } else if (animItem.type == ScenarioType.puck) {
      successMessage = "Correct! Acceleration = 5 m/s²";
    } else if (animItem.type == ScenarioType.skateboarder) {
      successMessage = "Correct! That's Inertia in action!";
    }

    onShowMessage(successMessage, Colors.greenAccent);

    // Check Victory
    if (completedScenarios.length == 3) {
      onShowMessage("VICTORY! All three laws demonstrated!", Colors.yellowAccent);
    }
  }

  void resetGame() {
    completedScenarios.clear();
    // Remove all current components and reload
    removeAll(children);
    onLoad();
  }
}

// ==========================================
// 5. Theory Challenge Custom Dialog Overlay
// ==========================================
class TheoryOverlay extends StatefulWidget {
  final DraggableScenario item;
  final void Function(bool isCorrect, String? errorMsg) onValidate;

  const TheoryOverlay({
    super.key,
    required this.item,
    required this.onValidate,
  });

  @override
  State<TheoryOverlay> createState() => _TheoryOverlayState();
}

class _TheoryOverlayState extends State<TheoryOverlay> with SingleTickerProviderStateMixin {
  final TextEditingController _puckInputController = TextEditingController();
  late AnimationController _shakeController;
  late Animation<double> _shakeAnimation;
  String? _errorMessage;

  @override
  void initState() {
    super.initState();
    _shakeController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 500),
    );
    _shakeAnimation = Tween<double>(begin: 0.0, end: 20.0)
        .chain(CurveTween(curve: Curves.elasticIn))
        .animate(_shakeController)
      ..addStatusListener((status) {
        if (status == AnimationStatus.completed) {
          _shakeController.reset();
        }
      });
  }

  @override
  void dispose() {
    _puckInputController.dispose();
    _shakeController.dispose();
    super.dispose();
  }

  void _triggerShake(String errorMsg) {
    setState(() {
      _errorMessage = errorMsg;
    });
    _shakeController.forward();
  }

  void _submitChoice(String selectedChoice) {
    if (widget.item.type == ScenarioType.rocket) {
      if (selectedChoice == "Gas pushes rocket UP") {
        widget.onValidate(true, null);
      } else {
        _triggerShake("Think about opposite forces. The gas goes down, so...");
      }
    } else if (widget.item.type == ScenarioType.skateboarder) {
      if (selectedChoice == "Inertia") {
        widget.onValidate(true, null);
      } else {
        _triggerShake("Think about Newton's 1st Law. Objects in motion tend to...");
      }
    }
  }

  void _submitPuckCalculation() {
    final value = double.tryParse(_puckInputController.text.trim());
    if (value == 5) {
      widget.onValidate(true, null);
    } else {
      _triggerShake("Check your math! F = m * a  =>  20 = 4 * a. Find a.");
    }
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _shakeAnimation,
      builder: (context, child) {
        final double offset = sin(_shakeAnimation.value * pi * 2) * 8.0;
        return Transform.translate(
          offset: Offset(offset, 0),
          child: child,
        );
      },
      child: Center(
        child: Material(
          color: Colors.transparent,
          child: Container(
            margin: const EdgeInsets.symmetric(horizontal: 20),
            padding: const EdgeInsets.all(24),
            decoration: BoxDecoration(
              color: const Color(0xFF1E293B),
              borderRadius: BorderRadius.circular(20),
              border: Border.all(color: Colors.indigoAccent, width: 2),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withOpacity(0.5),
                  blurRadius: 15,
                  spreadRadius: 2,
                )
              ],
            ),
            constraints: const BoxConstraints(maxWidth: 400),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                // Header Icon
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: const BoxDecoration(
                    color: Colors.indigo,
                    shape: BoxShape.circle,
                  ),
                  child: Icon(
                    widget.item.type == ScenarioType.rocket
                        ? Icons.rocket_launch
                        : widget.item.type == ScenarioType.puck
                            ? Icons.calculate
                            : Icons.directions_run,
                    color: Colors.white,
                    size: 32,
                  ),
                ),
                const SizedBox(height: 16),
                // Title
                const Text(
                  "Theory Checkpoint",
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 20,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 12),
                // Question Statement
                Text(
                  _getQuestionText(),
                  textAlign: TextAlign.center,
                  style: const TextStyle(
                    color: Color(0xFFE2E8F0),
                    fontSize: 15,
                    height: 1.4,
                  ),
                ),
                const SizedBox(height: 20),
                // Form / Interactive element
                _buildFormWidget(),
                // Error Hint
                if (_errorMessage != null) ...[
                  const SizedBox(height: 12),
                  Text(
                    _errorMessage!,
                    textAlign: TextAlign.center,
                    style: const TextStyle(
                      color: Colors.redAccent,
                      fontSize: 13,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ]
              ],
            ),
          ),
        ),
      ),
    );
  }

  String _getQuestionText() {
    switch (widget.item.type) {
      case ScenarioType.rocket:
        return "Action: The space rocket pushes burning gas downward.\n\nWhat is the equal and opposite reaction?";
      case ScenarioType.puck:
        return "Given data:\nForce (F) = 20 N\nMass (m) = 4 kg\n\nUsing F = ma, calculate the resulting acceleration (a) in m/s².";
      case ScenarioType.skateboarder:
        return "The skateboard hits a curb and stops suddenly. Why does the rider continue to fly forward?";
    }
  }

  Widget _buildFormWidget() {
    switch (widget.item.type) {
      case ScenarioType.rocket:
        return Column(
          children: [
            ElevatedButton(
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFF0F172A),
                foregroundColor: Colors.white,
                minimumSize: const Size(double.infinity, 48),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
              ),
              onPressed: () => _submitChoice("Gas pushes rocket UP"),
              child: const Text("Gas pushes rocket UP"),
            ),
            const SizedBox(height: 10),
            ElevatedButton(
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFF0F172A),
                foregroundColor: Colors.white,
                minimumSize: const Size(double.infinity, 48),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
              ),
              onPressed: () => _submitChoice("Rocket gravity increases"),
              child: const Text("Rocket gravity increases"),
            ),
          ],
        );
      case ScenarioType.puck:
        return Column(
          children: [
            TextField(
              controller: _puckInputController,
              keyboardType: TextInputType.number,
              textAlign: TextAlign.center,
              style: const TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.bold),
              decoration: InputDecoration(
                hintText: "Enter acceleration (a)",
                hintStyle: const TextStyle(color: Colors.grey),
                filled: true,
                fillColor: const Color(0xFF0F172A),
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(10),
                  borderSide: BorderSide.none,
                ),
              ),
            ),
            const SizedBox(height: 16),
            ElevatedButton(
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.green,
                foregroundColor: Colors.white,
                minimumSize: const Size(double.infinity, 48),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
              ),
              onPressed: _submitPuckCalculation,
              child: const Text("Verify Answer"),
            ),
          ],
        );
      case ScenarioType.skateboarder:
        return Column(
          children: [
            ElevatedButton(
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFF0F172A),
                foregroundColor: Colors.white,
                minimumSize: const Size(double.infinity, 48),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
              ),
              onPressed: () => _submitChoice("Friction"),
              child: const Text("Friction"),
            ),
            const SizedBox(height: 10),
            ElevatedButton(
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFF0F172A),
                foregroundColor: Colors.white,
                minimumSize: const Size(double.infinity, 48),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
              ),
              onPressed: () => _submitChoice("Inertia"),
              child: const Text("Inertia"),
            ),
          ],
        );
    }
  }
}

// ==========================================
// 6. Complete Flutter Game Screen Wrapper
// ==========================================
class NewtonGameScreen extends StatefulWidget {
  const NewtonGameScreen({super.key});

  @override
  State<NewtonGameScreen> createState() => _NewtonGameScreenState();
}

class _NewtonGameScreenState extends State<NewtonGameScreen> {
  late NewtonGame _game;

  // Game UI State Control
  DraggableScenario? _activeDialogItem;
  bool _showNotification = false;
  String _notificationMessage = "";
  Color _notificationColor = Colors.white;

  @override
  void initState() {
    super.initState();
    _game = NewtonGame(
      onTriggerCheckpoint: (item) {
        // Match detected: Pause engine and trigger Flutter Overlay
        _game.pauseEngine();
        setState(() {
          _activeDialogItem = item;
        });
      },
      onShowMessage: (message, color) {
        setState(() {
          _notificationMessage = message;
          _notificationColor = color;
          _showNotification = true;
        });
        // Auto-dismiss standard message after 3 seconds, except Victory
        if (!message.contains("VICTORY")) {
          Future.delayed(const Duration(seconds: 3), () {
            if (mounted) {
              setState(() {
                _showNotification = false;
              });
            }
          });
        }
      },
    );
  }

  void _onValidate(bool isCorrect, String? errorMsg) {
    if (isCorrect && _activeDialogItem != null) {
      final matchedItem = _activeDialogItem!;
      setState(() {
        _activeDialogItem = null;
      });
      // Close overlay, resume game engine, play physics animation
      _game.resumeEngine();
      _game.handleCheckpointSuccess(matchedItem);
    }
  }

  @override
  Widget build(BuildContext context) {
    final bool isVictory = _game.completedScenarios.length == 3;

    return Scaffold(
      backgroundColor: const Color(0xFF0F172A),
      appBar: AppBar(
        title: const Text(
          "Newton's Laws Arena",
          style: TextStyle(fontFamily: 'Poppins', fontWeight: FontWeight.bold, fontSize: 18),
        ),
        centerTitle: true,
        backgroundColor: const Color(0xFF1E293B),
        elevation: 4,
        foregroundColor: Colors.white,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () {
            Navigator.pop(context);
          },
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: "Restart Game",
            onPressed: () {
              setState(() {
                _activeDialogItem = null;
                _showNotification = false;
                _game.resetGame();
              });
            },
          )
        ],
      ),
      body: SafeArea(
        child: Stack(
          children: [
            // Flame game canvas
            GameWidget(game: _game),

            // Top Instructions HUD
            if (!isVictory && _activeDialogItem == null)
              Positioned(
                top: 15,
                left: 15,
                right: 15,
                child: Container(
                  padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                  decoration: BoxDecoration(
                    color: Colors.black.withOpacity(0.6),
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: Colors.white24),
                  ),
                  child: const Row(
                    children: [
                      Icon(Icons.info_outline, color: Colors.blueAccent, size: 20),
                      SizedBox(width: 10),
                      Expanded(
                        child: Text(
                          "Drag and match each scenario to its corresponding Newton's Law drop zone above.",
                          style: TextStyle(color: Colors.white, fontSize: 12),
                        ),
                      ),
                    ],
                  ),
                ),
              ),

            // Active Dialog / Checkpoint Overlay
            if (_activeDialogItem != null)
              Positioned.fill(
                child: Container(
                  color: Colors.black87,
                  child: TheoryOverlay(
                    item: _activeDialogItem!,
                    onValidate: _onValidate,
                  ),
                ),
              ),

            // Notification / Success Banner Overlay
            if (_showNotification)
              Positioned(
                bottom: 140,
                left: 20,
                right: 20,
                child: Center(
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 14),
                    decoration: BoxDecoration(
                      color: const Color(0xFF1E293B),
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: _notificationColor, width: 1.5),
                      boxShadow: const [
                        BoxShadow(
                          color: Colors.black45,
                          blurRadius: 8,
                          offset: Offset(0, 4),
                        )
                      ],
                    ),
                    child: Text(
                      _notificationMessage,
                      textAlign: TextAlign.center,
                      style: TextStyle(
                        color: _notificationColor,
                        fontSize: 15,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ),
              ),

            // Victory Screen Overlay
            if (isVictory)
              Positioned.fill(
                child: Container(
                  color: Colors.black.withOpacity(0.9),
                  child: Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        const Icon(
                          Icons.emoji_events,
                          color: Colors.yellowAccent,
                          size: 100,
                        ),
                        const SizedBox(height: 16),
                        const Text(
                          "CONGRATULATIONS!",
                          style: TextStyle(
                            color: Colors.white,
                            fontSize: 28,
                            fontWeight: FontWeight.bold,
                            letterSpacing: 2.0,
                          ),
                        ),
                        const SizedBox(height: 12),
                        const Padding(
                          padding: EdgeInsets.symmetric(horizontal: 40.0),
                          child: Text(
                            "You have successfully mapped and simulated Newton's Three Laws of Motion!",
                            textAlign: TextAlign.center,
                            style: TextStyle(
                              color: Color(0xFFCBD5E1),
                              fontSize: 16,
                            ),
                          ),
                        ),
                        const SizedBox(height: 32),
                        ElevatedButton.icon(
                          style: ElevatedButton.styleFrom(
                            backgroundColor: Colors.green,
                            foregroundColor: Colors.white,
                            padding: const EdgeInsets.symmetric(horizontal: 28, vertical: 14),
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(30),
                            ),
                            elevation: 8,
                          ),
                          onPressed: () {
                            setState(() {
                              _activeDialogItem = null;
                              _showNotification = false;
                              _game.resetGame();
                            });
                          },
                          icon: const Icon(Icons.replay),
                          label: const Text(
                            "Play Again",
                            style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
          ],
        ),
      ),
    );
  }
}
