import 'package:flame/components.dart';
import 'package:flame/events.dart';
import 'package:flame/game.dart';
import 'package:flutter/material.dart';
import 'dart:math';
import '../../lessons/Lessons_Dashboard.dart';

// ---------------------------------------------------------
// 1. Flutter Overlays
// ---------------------------------------------------------

class CalculationOverlay extends StatefulWidget {
  final String actorName;
  final double force;
  final double distance;
  final double time;
  final void Function(bool isCorrect) onSubmit;

  const CalculationOverlay({
    super.key,
    required this.actorName,
    required this.force,
    required this.distance,
    required this.time,
    required this.onSubmit,
  });

  @override
  State<CalculationOverlay> createState() => _CalculationOverlayState();
}

class _CalculationOverlayState extends State<CalculationOverlay>
    with SingleTickerProviderStateMixin {
  final TextEditingController _workController = TextEditingController();
  final TextEditingController _powerController = TextEditingController();
  late AnimationController _animationController;
  late Animation<double> _shakeAnimation;
  bool _showError = false;

  @override
  void initState() {
    super.initState();
    _animationController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 500),
    );

    _shakeAnimation = Tween<double>(begin: 0, end: 1).animate(
      CurvedAnimation(
        parent: _animationController,
        curve: Curves.decelerate,
      ),
    )..addStatusListener((status) {
        if (status == AnimationStatus.completed) {
          _animationController.reset();
        }
      });
  }

  @override
  void dispose() {
    _workController.dispose();
    _powerController.dispose();
    _animationController.dispose();
    super.dispose();
  }

  void _submit() {
    final workInput = double.tryParse(_workController.text.replaceAll(',', ''));
    final powerInput = double.tryParse(_powerController.text.replaceAll(',', ''));

    if (workInput == null || powerInput == null) return;

    final correctWork = widget.force * widget.distance;
    final correctPower = correctWork / widget.time;

    // Validate inputs
    final isWorkCorrect = (workInput - correctWork).abs() < 0.1;
    final isPowerCorrect = (powerInput - correctPower).abs() < 0.1;

    if (isWorkCorrect && isPowerCorrect) {
      widget.onSubmit(true);
    } else {
      setState(() {
        _showError = true;
      });
      _animationController.forward();
    }
  }

  @override
  Widget build(BuildContext context) {
    return Center(
      child: SingleChildScrollView(
        padding: EdgeInsets.only(
          bottom: MediaQuery.of(context).viewInsets.bottom + 16,
          top: 16,
          left: 16,
          right: 16,
        ),
        child: AnimatedBuilder(
          animation: _shakeAnimation,
          builder: (context, child) {
            final double offset = sin(_animationController.value * pi * 6) * 15 * (1 - _animationController.value);
            return Transform.translate(
              offset: Offset(offset, 0),
              child: child,
            );
          },
          child: Material(
            color: Colors.transparent,
            child: Container(
              constraints: const BoxConstraints(maxWidth: 380),
              padding: const EdgeInsets.all(24),
              decoration: BoxDecoration(
                color: const Color(0xFF1E293B), // Dark slate
                borderRadius: BorderRadius.circular(20),
                border: Border.all(
                  color: Colors.yellowAccent.withAlpha(150),
                  width: 2,
                ),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withAlpha(180),
                    blurRadius: 25,
                    spreadRadius: 8,
                  )
                ],
              ),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.all(8),
                        decoration: BoxDecoration(
                          color: Colors.yellowAccent.withAlpha(30),
                          shape: BoxShape.circle,
                        ),
                        child: Icon(
                          widget.actorName.contains('Bulldozer')
                              ? Icons.agriculture_outlined
                              : Icons.engineering_outlined,
                          color: Colors.yellowAccent,
                          size: 28,
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Text(
                          '${widget.actorName} Checkpoint',
                          style: const TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                            color: Colors.white,
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 14),
                  // Given info box
                  Container(
                    width: double.infinity,
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: const Color(0xFF0F172A),
                      borderRadius: BorderRadius.circular(10),
                      border: Border.all(color: Colors.yellowAccent.withAlpha(55)),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Specifications:',
                          style: TextStyle(
                            color: Colors.yellowAccent[100],
                            fontWeight: FontWeight.bold,
                            fontSize: 12,
                          ),
                        ),
                        const SizedBox(height: 6),
                        Text(
                          '• Applied Force (F) = ${widget.force.toInt()} N\n'
                          '• Distance (d) = ${widget.distance.toInt()} m\n'
                          '• Time Taken (t) = ${widget.time.toInt()} seconds',
                          style: const TextStyle(color: Color(0xFFCBD5E1), fontSize: 13, height: 1.4),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 16),
                  // Work Done Input
                  TextField(
                    controller: _workController,
                    keyboardType: TextInputType.number,
                    style: const TextStyle(color: Colors.white, fontSize: 16),
                    decoration: InputDecoration(
                      labelText: 'Work Done (W) in Joules',
                      labelStyle: const TextStyle(color: Colors.white70),
                      hintText: 'W = F × d',
                      hintStyle: const TextStyle(color: Colors.white38),
                      filled: true,
                      fillColor: const Color(0xFF0F172A),
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                    ),
                  ),
                  const SizedBox(height: 14),
                  // Power Input
                  TextField(
                    controller: _powerController,
                    keyboardType: TextInputType.number,
                    style: const TextStyle(color: Colors.white, fontSize: 16),
                    decoration: InputDecoration(
                      labelText: 'Power Output (P) in Watts',
                      labelStyle: const TextStyle(color: Colors.white70),
                      hintText: 'P = W / t',
                      hintStyle: const TextStyle(color: Colors.white38),
                      filled: true,
                      fillColor: const Color(0xFF0F172A),
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                    ),
                  ),
                  if (_showError) ...[
                    const SizedBox(height: 12),
                    const Text(
                      'Hint: Work (W) = Force (F) * distance (d)\nPower (P) = Work (W) / time (t)',
                      textAlign: TextAlign.center,
                      style: TextStyle(
                        color: Colors.redAccent,
                        fontSize: 12,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ],
                  const SizedBox(height: 18),
                  SizedBox(
                    width: double.infinity,
                    height: 48,
                    child: ElevatedButton(
                      onPressed: _submit,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.yellow[600],
                        foregroundColor: Colors.black,
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                        elevation: 4,
                      ),
                      child: const Text(
                        'Submit Calculations',
                        style: TextStyle(
                          fontSize: 15,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class VictoryOverlay extends StatelessWidget {
  final String actorName;
  final double power;
  final VoidCallback onReset;
  final VoidCallback onExit;

  const VictoryOverlay({
    super.key,
    required this.actorName,
    required this.power,
    required this.onReset,
    required this.onExit,
  });

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Material(
        color: Colors.transparent,
        child: Container(
          constraints: const BoxConstraints(maxWidth: 380),
          margin: const EdgeInsets.symmetric(horizontal: 16),
          padding: const EdgeInsets.all(24),
          decoration: BoxDecoration(
            color: const Color(0xFF0F172A),
            borderRadius: BorderRadius.circular(20),
            border: Border.all(
              color: Colors.greenAccent.withAlpha(150),
              width: 3,
            ),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withAlpha(200),
                blurRadius: 30,
                spreadRadius: 10,
              )
            ],
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: Colors.greenAccent.withAlpha(30),
                  shape: BoxShape.circle,
                ),
                child: const Icon(
                  Icons.done_all_rounded,
                  color: Colors.greenAccent,
                  size: 56,
                ),
              ),
              const SizedBox(height: 16),
              const Text(
                'Mission Complete!',
                style: TextStyle(
                  fontSize: 24,
                  fontWeight: FontWeight.bold,
                  color: Colors.greenAccent,
                ),
              ),
              const SizedBox(height: 14),
              Text(
                'The $actorName pushed the crate to the finish line in ${actorName.contains('Bulldozer') ? '2' : '10'} seconds!\n\n'
                'Key Physics Concept:\n'
                '• Both machines performed the same amount of Work (500 J) because they applied the same Force (50 N) over the same Distance (10 m).\n\n'
                '• However, the Bulldozer generated more Power (${power.toInt()} W vs 50 W) because it performed the work 5 times faster!',
                textAlign: TextAlign.center,
                style: const TextStyle(
                  fontSize: 13,
                  color: Color(0xFFE2E8F0),
                  height: 1.5,
                ),
              ),
              const SizedBox(height: 24),
              Row(
                children: [
                  Expanded(
                    child: OutlinedButton(
                      onPressed: onReset,
                      style: OutlinedButton.styleFrom(
                        side: const BorderSide(color: Colors.yellowAccent, width: 2),
                        padding: const EdgeInsets.symmetric(vertical: 12),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                      ),
                      child: const Text(
                        'Try Another',
                        style: TextStyle(
                            color: Colors.yellowAccent,
                            fontSize: 14,
                            fontWeight: FontWeight.bold),
                      ),
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: ElevatedButton(
                      onPressed: onExit,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.green[600],
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(vertical: 12),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                      ),
                      child: const Text(
                        'Exit Lab',
                        style: TextStyle(
                            fontSize: 14,
                            fontWeight: FontWeight.bold),
                      ),
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}

// ---------------------------------------------------------
// 2. Flame Game Engine
// ---------------------------------------------------------

class WorkPowerGame extends FlameGame with DragCallbacks {
  final void Function(String actorName, double force, double distance, double time) onShowCalculation;
  final void Function(String actorName, double power) onShowVictory;

  WorkPowerGame({
    required this.onShowCalculation,
    required this.onShowVictory,
  });

  late ConstructionGroundComponent ground;
  late HeavyCrateComponent crate;
  late ActorComponent robot;
  late ActorComponent bulldozer;

  double groundLevelY = 0;
  double startLineX = 0;
  final double distanceMeters = 10.0;
  double get finishLineX => size.x * 0.82;

  // Active push state
  ActorComponent? activeActor;
  bool isPushing = false;
  double pushDurationElapsed = 0.0;
  double pushStartX = 0.0;

  @override
  Color backgroundColor() => const Color(0xFF0F172A);

  @override
  Future<void> onLoad() async {
    super.onLoad();

    groundLevelY = size.y * 0.8;
    startLineX = size.x * 0.15;

    // 1. Ground Component
    ground = ConstructionGroundComponent(
      groundLevelY: groundLevelY,
      startLineX: startLineX,
      finishLineX: finishLineX,
      distanceMeters: distanceMeters,
    );
    add(ground);

    // 2. Heavy Crate Component
    crate = HeavyCrateComponent(
      startPosition: Vector2(startLineX, groundLevelY - 60),
      size: Vector2(60, 60),
    );
    add(crate);

    // 3. Actors (Draggable items)
    _loadActors();
  }

  void _loadActors() {
    if (activeActor != null) {
      remove(activeActor!);
      activeActor = null;
    }
    isPushing = false;
    pushDurationElapsed = 0.0;

    // Worker Robot: Blue, applied force 50 N, time 10s
    robot = ActorComponent(
      name: 'Worker Robot',
      force: 50.0,
      time: 10.0,
      color: Colors.blueAccent,
      startPosition: Vector2(size.x * 0.72, size.y * 0.25),
      size: Vector2(40, 60),
      onDropped: _handleActorDropped,
    );

    // Bulldozer: Yellow, applied force 50 N, time 2s
    bulldozer = ActorComponent(
      name: 'Bulldozer',
      force: 50.0,
      time: 2.0,
      color: Colors.yellow[600]!,
      startPosition: Vector2(size.x * 0.88, size.y * 0.25),
      size: Vector2(70, 50),
      onDropped: _handleActorDropped,
    );

    add(robot);
    add(bulldozer);
  }

  void _handleActorDropped(ActorComponent actor, Vector2 dropPos) {
    if (isPushing) return;

    // Check collision with the Heavy Crate (Drop zone)
    final distanceToCrate = dropPos.distanceTo(crate.position + crate.size / 2);
    
    if (distanceToCrate < 90) {
      // Snap to the left side of the crate
      actor.position = Vector2(
        crate.position.x - actor.size.x - 2,
        groundLevelY - actor.size.y,
      );
      actor.isLocked = true;
      activeActor = actor;

      // Disable dragging on the other actor
      final other = actor == robot ? bulldozer : robot;
      other.position = Vector2(-200, -200); // move off-screen
      
      // Pause engine and trigger checkpoint dialog
      pauseEngine();
      onShowCalculation(
        actor.name,
        actor.force,
        distanceMeters,
        actor.time,
      );
    } else {
      actor.returnToStart();
    }
  }

  void handleCalculationCorrect() {
    resumeEngine();
    if (activeActor == null) return;
    isPushing = true;
    pushStartX = activeActor!.position.x;
  }

  @override
  void update(double dt) {
    super.update(dt);

    if (isPushing && activeActor != null) {
      pushDurationElapsed += dt;
      final progress = min(pushDurationElapsed / activeActor!.time, 1.0);

      // Travel from start x to finish x
      final totalTravelDist = finishLineX - crate.size.x - pushStartX;
      final newActorX = pushStartX + totalTravelDist * progress;

      activeActor!.position.x = newActorX;
      crate.position.x = newActorX + activeActor!.size.x + 2;

      // Check if finished
      if (progress >= 1.0) {
        isPushing = false;
        onShowVictory(activeActor!.name, (activeActor!.force * distanceMeters) / activeActor!.time);
      }
    }
  }

  void resetGame() {
    crate.position = Vector2(startLineX, groundLevelY - crate.size.y);
    _loadActors();
    resumeEngine();
  }

  @override
  void onGameResize(Vector2 newSize) {
    super.onGameResize(newSize);
    if (isLoaded) {
      groundLevelY = newSize.y * 0.8;
      startLineX = newSize.x * 0.15;

      ground.groundLevelY = groundLevelY;
      ground.startLineX = startLineX;
      ground.finishLineX = finishLineX;

      if (!isPushing) {
        crate.position = Vector2(startLineX, groundLevelY - crate.size.y);
        
        if (activeActor == null) {
          robot.startPosition = Vector2(newSize.x * 0.72, newSize.y * 0.25);
          robot.position = robot.startPosition.clone();

          bulldozer.startPosition = Vector2(newSize.x * 0.88, newSize.y * 0.25);
          bulldozer.position = bulldozer.startPosition.clone();
        } else {
          activeActor!.position = Vector2(
            crate.position.x - activeActor!.size.x - 2,
            groundLevelY - activeActor!.size.y,
          );
        }
      }
    }
  }
}

// ---------------------------------------------------------
// 3. Custom Flame Components
// ---------------------------------------------------------

class ConstructionGroundComponent extends PositionComponent {
  double groundLevelY;
  double startLineX;
  double finishLineX;
  final double distanceMeters;

  ConstructionGroundComponent({
    required this.groundLevelY,
    required this.startLineX,
    required this.finishLineX,
    required this.distanceMeters,
  });

  @override
  void render(Canvas canvas) {
    // Draw sky/ambient background
    final skyRect = Rect.fromLTWH(0, 0, size.x, groundLevelY);
    final skyPaint = Paint()
      ..color = const Color(0xFF0F172A);
    canvas.drawRect(skyRect, skyPaint);

    // Draw grey ground stretching across bottom
    final groundRect = Rect.fromLTWH(0, groundLevelY, size.x, size.y - groundLevelY);
    final groundPaint = Paint()..color = const Color(0xFF475569); // Dark grey concrete ground
    canvas.drawRect(groundRect, groundPaint);

    // Ground edge line
    final linePaint = Paint()
      ..color = const Color(0xFF64748B)
      ..strokeWidth = 4;
    canvas.drawLine(Offset(0, groundLevelY), Offset(size.x, groundLevelY), linePaint);

    // Start Line (Dashed)
    _drawDashedVerticalLine(canvas, startLineX, 0, groundLevelY, Colors.yellowAccent.withAlpha(130));
    _drawText(canvas, 'START', startLineX, groundLevelY + 20, color: Colors.yellowAccent);

    // Finish Line (Dashed)
    _drawDashedVerticalLine(canvas, finishLineX, 0, groundLevelY, Colors.greenAccent.withAlpha(130));
    _drawText(canvas, 'FINISH', finishLineX, groundLevelY + 20, color: Colors.greenAccent);

    // Distance Label Bracket
    final bracketPaint = Paint()
      ..color = Colors.white24
      ..strokeWidth = 2
      ..style = PaintingStyle.stroke;
    final bracketY = groundLevelY - 140;
    canvas.drawLine(Offset(startLineX, bracketY), Offset(finishLineX, bracketY), bracketPaint);
    canvas.drawLine(Offset(startLineX, bracketY - 8), Offset(startLineX, bracketY + 8), bracketPaint);
    canvas.drawLine(Offset(finishLineX, bracketY - 8), Offset(finishLineX, bracketY + 8), bracketPaint);
    _drawText(canvas, 'Total Distance D = ${distanceMeters.toInt()} meters', (startLineX + finishLineX) / 2, bracketY - 20, fontSize: 13);
  }

  void _drawDashedVerticalLine(Canvas canvas, double x, double startY, double endY, Color color) {
    final paint = Paint()
      ..color = color
      ..strokeWidth = 3
      ..style = PaintingStyle.stroke;

    double curY = startY;
    const dashHeight = 8.0;
    const dashSpace = 6.0;

    while (curY < endY) {
      canvas.drawLine(Offset(x, curY), Offset(x, curY + dashHeight), paint);
      curY += dashHeight + dashSpace;
    }
  }

  void _drawText(Canvas canvas, String text, double x, double y,
      {Color color = Colors.white70, double fontSize = 12}) {
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

class HeavyCrateComponent extends PositionComponent {
  final Vector2 startPosition;

  HeavyCrateComponent({
    required this.startPosition,
    required super.size,
  }) : super(position: startPosition.clone());

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);

    // Brown crate color
    final fillPaint = Paint()..color = const Color(0xFF8B5A2B);
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(6)), fillPaint);

    // Crate frame/border
    final borderPaint = Paint()
      ..color = const Color(0xFF5C3A21)
      ..strokeWidth = 3
      ..style = PaintingStyle.stroke;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(6)), borderPaint);

    // Dynamic diagonal cross plank for visual aesthetic
    canvas.drawLine(const Offset(4, 4), Offset(size.x - 4, size.y - 4), borderPaint);
    canvas.drawLine(Offset(4, size.y - 4), Offset(size.x - 4, 4), borderPaint);

    // Crate Label
    final tp = TextPainter(
      text: const TextSpan(
        text: '50kg',
        style: TextStyle(
          color: Colors.white,
          fontSize: 10,
          fontWeight: FontWeight.bold,
          backgroundColor: Colors.black54,
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    tp.paint(canvas, Offset((size.x - tp.width) / 2, (size.y - tp.height) / 2));
  }
}

class ActorComponent extends PositionComponent with DragCallbacks {
  final String name;
  final double force;
  final double time;
  final Color color;
  Vector2 startPosition;
  final void Function(ActorComponent actor, Vector2 finalPosition) onDropped;

  bool isDragging = false;
  bool isLocked = false;

  ActorComponent({
    required this.name,
    required this.force,
    required this.time,
    required this.color,
    required this.startPosition,
    required super.size,
    required this.onDropped,
  }) : super(
          position: startPosition.clone(),
          anchor: Anchor.topLeft,
        );

  @override
  void onDragStart(DragStartEvent event) {
    super.onDragStart(event);
    if (isLocked) return;
    isDragging = true;
    priority = 100;
  }

  @override
  void onDragUpdate(DragUpdateEvent event) {
    if (!isDragging || isLocked) return;
    position += event.localDelta;
  }

  @override
  void onDragEnd(DragEndEvent event) {
    super.onDragEnd(event);
    if (isLocked) return;
    isDragging = false;
    priority = 1;
    onDropped(this, position + size / 2);
  }

  @override
  void onDragCancel(DragCancelEvent event) {
    super.onDragCancel(event);
    if (isLocked) return;
    isDragging = false;
    priority = 1;
    returnToStart();
  }

  void returnToStart() {
    isLocked = false;
    position = startPosition.clone();
  }

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);

    // Body paint
    final fillPaint = Paint()..color = color;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(8)), fillPaint);

    final borderPaint = Paint()
      ..color = Colors.black87
      ..strokeWidth = 2
      ..style = PaintingStyle.stroke;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(8)), borderPaint);

    // Eyes/Visor to look like robot/bulldozer components
    final visorPaint = Paint()..color = Colors.cyanAccent;
    canvas.drawRect(Rect.fromLTWH(size.x * 0.15, size.y * 0.2, size.x * 0.7, size.y * 0.15), visorPaint);

    // Tread/wheels at bottom
    final wheelPaint = Paint()..color = Colors.black87;
    canvas.drawCircle(Offset(size.x * 0.25, size.y - 4), 6, wheelPaint);
    canvas.drawCircle(Offset(size.x * 0.75, size.y - 4), 6, wheelPaint);

    // Label
    final text = name.contains('Bulldozer') ? 'DOZER' : 'ROBOT';
    final tp = TextPainter(
      text: TextSpan(
        text: '$text\n${force.toInt()}N',
        style: const TextStyle(
          color: Colors.white,
          fontSize: 8,
          fontWeight: FontWeight.bold,
        ),
      ),
      textDirection: TextDirection.ltr,
      textAlign: TextAlign.center,
    )..layout();
    tp.paint(canvas, Offset((size.x - tp.width) / 2, (size.y - tp.height) / 2 + 5));
  }
}

// ---------------------------------------------------------
// 4. Flutter Screen
// ---------------------------------------------------------

class WorkPowerGameScreen extends StatefulWidget {
  const WorkPowerGameScreen({super.key});

  @override
  State<WorkPowerGameScreen> createState() => _WorkPowerGameScreenState();
}

class _WorkPowerGameScreenState extends State<WorkPowerGameScreen> {
  late final WorkPowerGame _game;

  bool _showCalculation = false;
  String _activeActorName = '';
  double _activeForce = 0;
  double _activeDistance = 0;
  double _activeTime = 0;

  bool _showVictory = false;
  double _calculatedPower = 0.0;

  // Real-time animation states mapped to Flutter HUD overlay
  double _liveDistance = 0.0;
  double _liveWork = 0.0;
  double _liveTime = 0.0;
  double _livePower = 0.0;
  bool _hudVisible = false;

  @override
  void initState() {
    super.initState();
    _game = WorkPowerGame(
      onShowCalculation: (name, force, distance, time) {
        setState(() {
          _activeActorName = name;
          _activeForce = force;
          _activeDistance = distance;
          _activeTime = time;
          _showCalculation = true;
        });
      },
      onShowVictory: (name, power) {
        setState(() {
          _calculatedPower = power;
          _showVictory = true;
        });
      },
    );
  }

  void _onCalculationSubmit(bool isCorrect) {
    if (isCorrect) {
      setState(() {
        _showCalculation = false;
        _hudVisible = true;
        _livePower = _activeForce * _activeDistance / _activeTime;
      });
      _game.handleCalculationCorrect();
    }
  }

  void _onReset() {
    setState(() {
      _showVictory = false;
      _showCalculation = false;
      _hudVisible = false;
      _liveDistance = 0.0;
      _liveWork = 0.0;
      _liveTime = 0.0;
      _livePower = 0.0;
    });
    _game.resetGame();
  }

  void _exitToLessonDashboard() {
    _game.resetGame();
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(
        builder: (_) => const LessonsDashboard(
          lessonTitle: 'Work, energy and power',
          grade: 'Grade 10 Physics',
        ),
      ),
      (route) => false,
    );
  }

  @override
  Widget build(BuildContext context) {
    // Listen to tick changes to feed the HUD with fluid real-time updates
    if (_game.isPushing && _game.activeActor != null) {
      final progress = min(_game.pushDurationElapsed / _game.activeActor!.time, 1.0);
      _liveDistance = progress * _game.distanceMeters;
      _liveWork = _liveDistance * _activeForce;
      _liveTime = progress * _activeTime;
    }

    return Scaffold(
      backgroundColor: const Color(0xFF0F172A),
      resizeToAvoidBottomInset: false,
      appBar: AppBar(
        title: const Text(
          'Puzzle 3: Work & Power',
          style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
        ),
        backgroundColor: const Color(0xFF1E293B),
        foregroundColor: Colors.white,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: _exitToLessonDashboard,
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh_rounded),
            tooltip: 'Reset Lab',
            onPressed: _onReset,
          ),
        ],
      ),
      body: SafeArea(
        child: Column(
          children: [
            // Top HUD Banner
            Container(
              width: double.infinity,
              margin: const EdgeInsets.fromLTRB(10, 8, 10, 0),
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
              decoration: BoxDecoration(
                color: const Color(0xFF1E293B),
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: Colors.yellowAccent.withAlpha(100), width: 1),
              ),
              child: const Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Row(
                    children: [
                      Icon(Icons.info_outline, color: Colors.yellowAccent, size: 14),
                      SizedBox(width: 6),
                      Text(
                        'Drag Robot/Dozer to Crate',
                        style: TextStyle(
                          color: Colors.white70,
                          fontWeight: FontWeight.w600,
                          fontSize: 12,
                        ),
                      ),
                    ],
                  ),
                  Text(
                    'W = F·d  |  P = W/t',
                    style: TextStyle(
                      color: Colors.yellowAccent,
                      fontSize: 12,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
            ),

            // Live Telemetry Readout HUD when pushes occur
            if (_hudVisible)
              Container(
                margin: const EdgeInsets.fromLTRB(10, 6, 10, 0),
                padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                decoration: BoxDecoration(
                  color: const Color(0xFF1E293B),
                  borderRadius: BorderRadius.circular(10),
                  border: Border.all(color: Colors.cyanAccent.withAlpha(100), width: 1.5),
                ),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceAround,
                  children: [
                    _buildHUDStat('Time', '${_liveTime.toStringAsFixed(1)}s'),
                    _buildHUDStat('Distance', '${_liveDistance.toStringAsFixed(1)}m'),
                    _buildHUDStat('Work Done', '${_liveWork.toStringAsFixed(0)} J'),
                    _buildHUDStat('Power Output', '${_livePower.toStringAsFixed(0)} W'),
                  ],
                ),
              ),

            // Main Game viewport
            Expanded(
              child: Container(
                margin: const EdgeInsets.fromLTRB(10, 8, 10, 10),
                clipBehavior: Clip.antiAlias,
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(14),
                  border: Border.all(color: const Color(0xFF334155), width: 2),
                ),
                child: Stack(
                  fit: StackFit.expand,
                  children: [
                    GameWidget(game: _game),

                    // Calculations
                    if (_showCalculation)
                      Positioned.fill(
                        child: ColoredBox(
                          color: Colors.black87,
                          child: CalculationOverlay(
                            actorName: _activeActorName,
                            force: _activeForce,
                            distance: _activeDistance,
                            time: _activeTime,
                            onSubmit: _onCalculationSubmit,
                          ),
                        ),
                      ),

                    // Victory Overlay
                    if (_showVictory)
                      Positioned.fill(
                        child: ColoredBox(
                          color: Colors.black87,
                          child: SingleChildScrollView(
                            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 20),
                            child: VictoryOverlay(
                              actorName: _activeActorName,
                              power: _calculatedPower,
                              onReset: _onReset,
                              onExit: _exitToLessonDashboard,
                            ),
                          ),
                        ),
                      ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildHUDStat(String label, String value) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Text(
          label.toUpperCase(),
          style: const TextStyle(color: Colors.white54, fontSize: 9, fontWeight: FontWeight.bold),
        ),
        const SizedBox(height: 3),
        Text(
          value,
          style: const TextStyle(color: Colors.cyanAccent, fontSize: 14, fontWeight: FontWeight.bold),
        ),
      ],
    );
  }
}
