import 'package:flame/components.dart';
import 'package:flame/events.dart';
import 'package:flame/game.dart';
import 'package:flutter/material.dart';

// ==========================================
// 1. Flutter Overlays
// ==========================================

class CalculationOverlay extends StatefulWidget {
  final void Function(bool isCorrect) onSubmit;

  const CalculationOverlay({
    super.key,
    required this.onSubmit,
  });

  @override
  State<CalculationOverlay> createState() => _CalculationOverlayState();
}

class _CalculationOverlayState extends State<CalculationOverlay>
    with SingleTickerProviderStateMixin {
  final TextEditingController _controller = TextEditingController();
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

    _shakeAnimation = Tween<double>(begin: 0, end: 24).animate(
      CurvedAnimation(
        parent: _animationController,
        curve: const ElasticInCurve(),
      ),
    )..addStatusListener((status) {
        if (status == AnimationStatus.completed) {
          _animationController.reset();
        }
      });
  }

  @override
  void dispose() {
    _controller.dispose();
    _animationController.dispose();
    super.dispose();
  }

  void _submit() {
    final input = double.tryParse(_controller.text);
    if (input == null) return;

    if (input == 4) { // (20 - 0) / 5 = 4
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
      child: AnimatedBuilder(
        animation: _shakeAnimation,
        builder: (context, child) {
          final sinValue = (1 - _animationController.value) *
              20 *
              (_animationController.value % 0.2 > 0.1 ? 1 : -1);

          return Transform.translate(
            offset: Offset(sinValue, 0),
            child: child,
          );
        },
        child: Material(
          color: Colors.transparent,
          child: Container(
            constraints: const BoxConstraints(maxWidth: 420),
            padding: const EdgeInsets.all(24),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(16),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withAlpha(128),
                  blurRadius: 20,
                  spreadRadius: 5,
                )
              ],
            ),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Text(
                  'The Acceleration Zone',
                  style: TextStyle(
                    fontSize: 22,
                    fontWeight: FontWeight.bold,
                    color: Colors.black87,
                  ),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 16),
                const Text(
                  'To enter the Acceleration Zone, calculate the required acceleration.\n\n'
                  'The car starts from rest (u = 0 m/s) and needs to reach a final velocity (v) of 20 m/s in 5 seconds (t = 5 s).\n\n'
                  'What is the acceleration (a) in m/s²?',
                  textAlign: TextAlign.center,
                  style: TextStyle(fontSize: 16, color: Colors.black87),
                ),
                const SizedBox(height: 24),
                TextField(
                  controller: _controller,
                  keyboardType: TextInputType.number,
                  textAlign: TextAlign.center,
                  style: const TextStyle(
                      fontSize: 20, fontWeight: FontWeight.bold),
                  decoration: InputDecoration(
                    hintText: 'Enter acceleration (m/s²)',
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(8),
                    ),
                    errorText: _showError
                        ? 'Hint: Acceleration (a) = (v - u) / t'
                        : null,
                  ),
                  onSubmitted: (_) => _submit(),
                ),
                const SizedBox(height: 24),
                SizedBox(
                  width: double.infinity,
                  height: 48,
                  child: ElevatedButton(
                    onPressed: _submit,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.orange[800],
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(8),
                      ),
                    ),
                    child: const Text(
                      'Submit',
                      style: TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                          color: Colors.white),
                    ),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class ResultOverlay extends StatelessWidget {
  final bool isSuccess;
  final String message;
  final VoidCallback onAction;

  const ResultOverlay({
    super.key,
    required this.isSuccess,
    required this.message,
    required this.onAction,
  });

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Material(
        color: Colors.transparent,
        child: Container(
          constraints: const BoxConstraints(maxWidth: 400),
          padding: const EdgeInsets.all(32),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(16),
            border: Border.all(
              color: isSuccess ? Colors.green : Colors.redAccent,
              width: 3,
            ),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withAlpha(128),
                blurRadius: 20,
                spreadRadius: 5,
              )
            ],
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(
                isSuccess ? Icons.speed : Icons.cancel_outlined,
                color: isSuccess ? Colors.green : Colors.redAccent,
                size: 64,
              ),
              const SizedBox(height: 16),
              Text(
                isSuccess ? 'Speeding Up!' : 'Game Over!',
                style: TextStyle(
                  fontSize: 26,
                  fontWeight: FontWeight.bold,
                  color: isSuccess ? Colors.green[800] : Colors.red[800],
                ),
              ),
              const SizedBox(height: 16),
              Text(
                message,
                textAlign: TextAlign.center,
                style: const TextStyle(fontSize: 16, color: Colors.black87),
              ),
              if (isSuccess) ...[
                const SizedBox(height: 24),
                Container(
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: Colors.blue[50],
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: Colors.blue[200]!),
                  ),
                  child: Column(
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(Icons.lightbulb_outline, color: Colors.blue[800]),
                          const SizedBox(width: 8),
                          Text(
                            'Memory Reminder',
                            style: TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.bold,
                              color: Colors.blue[800],
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 8),
                      const Text(
                        'Acceleration is the rate of change of velocity over time.\nFormula: a = (v - u) / t',
                        textAlign: TextAlign.center,
                        style: TextStyle(fontSize: 14, color: Colors.black87),
                      ),
                    ],
                  ),
                ),
              ],
              const SizedBox(height: 32),
              ElevatedButton(
                onPressed: onAction,
                style: ElevatedButton.styleFrom(
                  backgroundColor: isSuccess ? Colors.green : Colors.redAccent,
                  padding:
                      const EdgeInsets.symmetric(horizontal: 32, vertical: 12),
                ),
                child: Text(
                  isSuccess ? 'Play Again' : 'Try Again',
                  style: const TextStyle(color: Colors.white, fontSize: 18),
                ),
              )
            ],
          ),
        ),
      ),
    );
  }
}

// ==========================================
// 2. Flame Components
// ==========================================

enum LaneType { constantVelocity, uniformAcceleration }

class LaneComponent extends PositionComponent {
  final LaneType type;
  bool isActivated = false;

  LaneComponent({
    required this.type,
    required super.position,
    required super.size,
  }) : super(anchor: Anchor.topCenter);

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    
    // Draw lane background
    canvas.drawRect(
      rect,
      Paint()..color = type == LaneType.constantVelocity 
          ? Colors.blue[100]! 
          : Colors.orange[100]!,
    );
    
    // Draw border
    canvas.drawRect(
      rect,
      Paint()
        ..color = type == LaneType.constantVelocity 
            ? Colors.blue[800]! 
            : Colors.orange[800]!
        ..strokeWidth = 4
        ..style = PaintingStyle.stroke,
    );

    // Draw dashed center line
    final paint = Paint()
      ..color = Colors.white70
      ..strokeWidth = 4
      ..style = PaintingStyle.stroke;
      
    const dashHeight = 20;
    const dashSpace = 15;
    double startY = 0;
    while (startY < size.y) {
      canvas.drawLine(Offset(size.x / 2, startY), Offset(size.x / 2, startY + dashHeight), paint);
      startY += dashHeight + dashSpace;
    }

    // Draw labels
    final title = type == LaneType.constantVelocity ? 'CONSTANT\nVELOCITY' : 'UNIFORM\nACCELERATION';
    final textColor = type == LaneType.constantVelocity ? Colors.blue[900]! : Colors.orange[900]!;
    _drawText(canvas, title, size.x / 2, size.y / 2, color: textColor, fontSize: 24);
    
    if (isActivated && type == LaneType.constantVelocity) {
      _drawText(canvas, 'a = 0 m/s²', size.x / 2, size.y * 0.8, color: Colors.blue[900]!, fontSize: 20);
    }
  }

  void _drawText(Canvas canvas, String text, double x, double y, {required Color color, double fontSize = 14}) {
    final tp = TextPainter(
      text: TextSpan(
        text: text,
        style: TextStyle(
          color: color,
          fontSize: fontSize,
          fontWeight: FontWeight.bold,
        ),
      ),
      textAlign: TextAlign.center,
      textDirection: TextDirection.ltr,
    )..layout();
    tp.paint(canvas, Offset(x - tp.width / 2, y - tp.height / 2));
  }
}

enum CarState { idle, constantVelocity, accelerating }

class DraggableCar extends PositionComponent with DragCallbacks {
  final Vector2 startPosition;
  final void Function(DraggableCar) onDropped;
  final void Function(String message) onShowMessage;

  bool isDragging = false;
  CarState state = CarState.idle;
  
  double velocityY = 0.0;
  final double accelerationRate = 200.0; // Visual scale of 4 m/s^2

  DraggableCar({
    required this.startPosition,
    required this.onDropped,
    required this.onShowMessage,
  }) : super(
          position: startPosition,
          size: Vector2(60, 100),
          anchor: Anchor.center,
        );

  @override
  void onDragStart(DragStartEvent event) {
    if (state != CarState.idle) return;
    super.onDragStart(event);
    isDragging = true;
    priority = 10;
  }

  @override
  void onDragUpdate(DragUpdateEvent event) {
    if (!isDragging) return;
    position += event.localDelta;
  }

  @override
  void onDragEnd(DragEndEvent event) {
    if (!isDragging) return;
    super.onDragEnd(event);
    isDragging = false;
    priority = 1;
    onDropped(this);
  }

  @override
  void onDragCancel(DragCancelEvent event) {
    if (!isDragging) return;
    super.onDragCancel(event);
    isDragging = false;
    priority = 1;
    returnToStart();
  }

  void returnToStart() {
    position = startPosition;
    state = CarState.idle;
    velocityY = 0.0;
  }

  void startConstantVelocity() {
    state = CarState.constantVelocity;
    velocityY = 150.0; // Steady visual speed
  }
  
  void startAcceleration() {
    state = CarState.accelerating;
    velocityY = 0.0; // Starts from rest (u=0)
  }

  @override
  void update(double dt) {
    super.update(dt);
    
    if (state == CarState.constantVelocity) {
      // position = speed * time
      position.y += velocityY * dt;
    } else if (state == CarState.accelerating) {
      // v = u + at
      velocityY += accelerationRate * dt;
      // s = ut + 0.5at^2 -> position changes exponentially based on increasing velocity
      position.y += velocityY * dt;
    }
  }

  @override
  void render(Canvas canvas) {
    if (isDragging) {
      final shadowPaint = Paint()
        ..color = Colors.black.withAlpha(80)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 8);
      canvas.drawRRect(
        RRect.fromRectAndRadius(Rect.fromLTWH(5, 5, size.x, size.y), const Radius.circular(8)), 
        shadowPaint
      );
    }

    // Draw Car Body
    canvas.drawRRect(
      RRect.fromRectAndRadius(Rect.fromLTWH(0, 0, size.x, size.y), const Radius.circular(12)),
      Paint()..color = Colors.yellow[700]!,
    );
    
    // Windshield
    canvas.drawRect(
      Rect.fromLTWH(size.x * 0.15, size.y * 0.2, size.x * 0.7, size.y * 0.25),
      Paint()..color = Colors.lightBlue[300]!,
    );
    
    // Rear Window
    canvas.drawRect(
      Rect.fromLTWH(size.x * 0.15, size.y * 0.65, size.x * 0.7, size.y * 0.15),
      Paint()..color = Colors.lightBlue[300]!,
    );
    
    // Wheels (just tiny side markers)
    final wheelPaint = Paint()..color = Colors.black87;
    canvas.drawRRect(RRect.fromRectAndRadius(const Rect.fromLTWH(-5, 15, 10, 20), const Radius.circular(2)), wheelPaint);
    canvas.drawRRect(RRect.fromRectAndRadius(Rect.fromLTWH(-5, size.y - 35, 10, 20), const Radius.circular(2)), wheelPaint);
    canvas.drawRRect(RRect.fromRectAndRadius(Rect.fromLTWH(size.x - 5, 15, 10, 20), const Radius.circular(2)), wheelPaint);
    canvas.drawRRect(RRect.fromRectAndRadius(Rect.fromLTWH(size.x - 5, size.y - 35, 10, 20), const Radius.circular(2)), wheelPaint);

    if (state == CarState.accelerating) {
      // Draw fire/exhaust if accelerating
      final firePaint = Paint()..color = Colors.orangeAccent;
      final path = Path()
        ..moveTo(size.x * 0.3, 0)
        ..lineTo(size.x * 0.5, -30)
        ..lineTo(size.x * 0.7, 0)
        ..close();
      canvas.drawPath(path, firePaint);
    }
  }
}

// ==========================================
// 3. Flame Game Logic
// ==========================================

class MotionQuestGame extends FlameGame {
  MotionQuestGame({
    required this.onShowCalculationDialog,
    required this.onShowResultDialog,
  });

  final void Function() onShowCalculationDialog;
  final void Function(bool isSuccess, String message) onShowResultDialog;

  late LaneComponent constVelLane;
  late LaneComponent accelLane;
  late DraggableCar testCar;
  
  bool isSimulationRunning = false;

  @override
  Color backgroundColor() => const Color(0xFFF1F5F9);

  @override
  Future<void> onLoad() async {
    await super.onLoad();

    final laneWidth = size.x * 0.45;
    final laneHeight = size.y * 0.8;

    constVelLane = LaneComponent(
      type: LaneType.constantVelocity,
      position: Vector2(size.x * 0.25, size.y * 0.2),
      size: Vector2(laneWidth, laneHeight),
    );
    add(constVelLane);

    accelLane = LaneComponent(
      type: LaneType.uniformAcceleration,
      position: Vector2(size.x * 0.75, size.y * 0.2),
      size: Vector2(laneWidth, laneHeight),
    );
    add(accelLane);

    testCar = DraggableCar(
      startPosition: Vector2(size.x * 0.5, size.y * 0.1),
      onDropped: _onCarDropped,
      onShowMessage: (msg) => onShowResultDialog(true, msg),
    );
    add(testCar);
  }

  void _onCarDropped(DraggableCar car) {
    if (_isDroppedOnLane(car, constVelLane)) {
      // Snap to top of const velocity lane
      car.position = Vector2(constVelLane.position.x, constVelLane.position.y + car.size.y / 2);
      constVelLane.isActivated = true;
      car.startConstantVelocity();
      isSimulationRunning = true;
      return;
    }

    if (_isDroppedOnLane(car, accelLane)) {
      // Snap to top of acceleration lane
      car.position = Vector2(accelLane.position.x, accelLane.position.y + car.size.y / 2);
      
      pauseEngine();
      onShowCalculationDialog();
      return;
    }

    // Dropped elsewhere
    car.returnToStart();
  }

  bool _isDroppedOnLane(DraggableCar car, LaneComponent lane) {
    // Check if the car is within the lane's X boundaries
    final minX = lane.position.x - lane.size.x / 2;
    final maxX = lane.position.x + lane.size.x / 2;
    return car.position.x >= minX && car.position.x <= maxX && car.position.y > lane.position.y - 50;
  }

  void handleCalculationSuccess() {
    resumeEngine();
    testCar.startAcceleration();
    isSimulationRunning = true;
    onShowResultDialog(true, 'Correct! The car accelerates uniformly at 4 m/s²!');
  }

  @override
  void update(double dt) {
    super.update(dt);
    
    // Check if car fell off screen to reset
    if (isSimulationRunning && testCar.position.y > size.y + 100) {
      isSimulationRunning = false;
      // Auto reset after falling
      Future.delayed(const Duration(seconds: 1), () {
        if (isMounted) resetGame();
      });
    }
  }

  void resetGame() {
    testCar.returnToStart();
    constVelLane.isActivated = false;
    isSimulationRunning = false;
    resumeEngine();
  }
}

// ==========================================
// 4. Flutter Screen
// ==========================================

class MotionQuestScreen extends StatefulWidget {
  const MotionQuestScreen({super.key});

  @override
  State<MotionQuestScreen> createState() => _MotionQuestScreenState();
}

class _MotionQuestScreenState extends State<MotionQuestScreen> {
  late final MotionQuestGame _game;

  bool _showCalculation = false;
  bool _showResult = false;
  bool _isSuccess = false;
  String _resultMessage = '';

  @override
  void initState() {
    super.initState();
    _game = MotionQuestGame(
      onShowCalculationDialog: () {
        setState(() {
          _showCalculation = true;
        });
      },
      onShowResultDialog: (success, message) {
        setState(() {
          _isSuccess = success;
          _resultMessage = message;
          _showResult = true;
        });
      },
    );
  }

  void _onCalculationSubmit(bool isCorrect) {
    if (isCorrect) {
      setState(() {
        _showCalculation = false;
      });
      _game.handleCalculationSuccess();
    }
  }

  void _onReset() {
    setState(() {
      _showResult = false;
      _showCalculation = false;
    });
    _game.resetGame();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Puzzle 3: Motion Quest'),
        backgroundColor: const Color(0xFF0F172A),
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Reset Puzzle',
            onPressed: _onReset,
          ),
        ],
      ),
      body: Stack(
        children: [
          GameWidget(game: _game),
          if (_showCalculation)
            Positioned.fill(
              child: Container(
                color: Colors.black54,
                child: CalculationOverlay(
                  onSubmit: _onCalculationSubmit,
                ),
              ),
            ),
          // We show result overlay on success, but keep it small if it's a notification,
          // Actually, based on requirements, on success: "Show a Flutter overlay message".
          // The ResultOverlay blocks the screen, but we want the user to SEE the car accelerating!
          // So let's use a non-blocking overlay (like a Snackbar or a positioned banner) for success messages.
          if (_showResult)
            Positioned(
              bottom: 40,
              left: 20,
              right: 20,
              child: Material(
                color: Colors.transparent,
                child: Container(
                  padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 24),
                  decoration: BoxDecoration(
                    color: _isSuccess ? Colors.green[800] : Colors.red[800],
                    borderRadius: BorderRadius.circular(12),
                    boxShadow: const [BoxShadow(color: Colors.black26, blurRadius: 10, offset: Offset(0, 4))],
                  ),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Row(
                        children: [
                          Icon(_isSuccess ? Icons.check_circle : Icons.error, color: Colors.white),
                          const SizedBox(width: 16),
                          Expanded(
                            child: Text(
                              _resultMessage,
                              style: const TextStyle(color: Colors.white, fontSize: 16, fontWeight: FontWeight.bold),
                            ),
                          ),
                          IconButton(
                            icon: const Icon(Icons.close, color: Colors.white),
                            onPressed: () {
                              setState(() {
                                _showResult = false;
                              });
                            },
                          ),
                        ],
                      ),
                      if (_isSuccess) ...[
                        const SizedBox(height: 12),
                        Container(
                          padding: const EdgeInsets.all(12),
                          decoration: BoxDecoration(
                            color: Colors.white.withAlpha(50),
                            borderRadius: BorderRadius.circular(8),
                          ),
                          child: const Row(
                            children: [
                              Icon(Icons.lightbulb_outline, color: Colors.white),
                              SizedBox(width: 12),
                              Expanded(
                                child: Text(
                                  'Memory Reminder: Acceleration is the rate of change of velocity over time. Formula: a = (v - u) / t',
                                  style: TextStyle(color: Colors.white, fontSize: 14),
                                ),
                              ),
                            ],
                          ),
                        ),
                      ],
                    ],
                  ),
                ),
              ),
            ),
        ],
      ),
    );
  }
}
