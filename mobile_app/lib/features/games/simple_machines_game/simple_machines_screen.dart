import 'package:flame/components.dart';
import 'package:flame/effects.dart';
import 'package:flame/events.dart';
import 'package:flame/game.dart';
import 'package:flutter/material.dart';
import 'dart:math';

// ==========================================
// 1. Flutter Overlays
// ==========================================

class CalculationOverlay extends StatefulWidget {
  final double force;
  final double distance;
  final void Function(bool isCorrect, double calculatedMoment) onSubmit;

  const CalculationOverlay({
    super.key,
    required this.force,
    required this.distance,
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

    final correctMoment = widget.force * widget.distance;

    if ((input - correctMoment).abs() < 0.1) {
      widget.onSubmit(true, correctMoment);
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
                  'Calculate the Turning Moment',
                  style: TextStyle(
                    fontSize: 22,
                    fontWeight: FontWeight.bold,
                    color: Colors.black87,
                  ),
                ),
                const SizedBox(height: 16),
                Text(
                  'You applied a downward force of ${widget.force.toInt()} N.\n'
                  'The perpendicular distance from the pivot is ${widget.distance.toInt()} m.\n\n'
                  'Calculate the turning moment (M = F * d):',
                  textAlign: TextAlign.center,
                  style: const TextStyle(fontSize: 16, color: Colors.black87),
                ),
                const SizedBox(height: 24),
                TextField(
                  controller: _controller,
                  keyboardType: TextInputType.number,
                  textAlign: TextAlign.center,
                  style: const TextStyle(
                      fontSize: 20, fontWeight: FontWeight.bold),
                  decoration: InputDecoration(
                    hintText: 'Enter Moment (Nm)',
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(8),
                    ),
                    errorText: _showError
                        ? 'Hint: Multiply Force (${widget.force.toInt()}) by Distance (${widget.distance.toInt()})'
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
                      backgroundColor: Colors.blueAccent,
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
  final VoidCallback onAction;

  const ResultOverlay({
    super.key,
    required this.isSuccess,
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
                isSuccess ? Icons.check_circle_outline : Icons.cancel_outlined,
                color: isSuccess ? Colors.green : Colors.redAccent,
                size: 64,
              ),
              const SizedBox(height: 16),
              Text(
                isSuccess ? 'Nut Loosened!' : 'Not Enough Moment!',
                style: TextStyle(
                  fontSize: 26,
                  fontWeight: FontWeight.bold,
                  color: isSuccess ? Colors.green[800] : Colors.red[800],
                ),
              ),
              const SizedBox(height: 16),
              Text(
                isSuccess
                    ? 'Great job! A longer distance creates a larger turning moment with the same force.'
                    : 'The moment was too small to break the rust. Try a tool that provides a larger perpendicular distance.',
                textAlign: TextAlign.center,
                style: const TextStyle(fontSize: 16, color: Colors.black87),
              ),
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

class RustedNutComponent extends PositionComponent {
  RustedNutComponent({required super.position})
      : super(size: Vector2(80, 80), anchor: Anchor.center);

  @override
  void render(Canvas canvas) {
    // Draw hexagon nut
    final paint = Paint()..color = const Color(0xFFB85D19); // Rusted orange/brown
    final path = Path();
    final center = Offset(size.x / 2, size.y / 2);
    final radius = size.x / 2;
    for (int i = 0; i < 6; i++) {
      final angle = (i * 60) * pi / 180;
      final point = Offset(
          center.dx + radius * cos(angle), center.dy + radius * sin(angle));
      if (i == 0) {
        path.moveTo(point.dx, point.dy);
      } else {
        path.lineTo(point.dx, point.dy);
      }
    }
    path.close();
    canvas.drawPath(path, paint);

    // Inner hole
    canvas.drawCircle(center, radius * 0.4, Paint()..color = Colors.black87);
    
    // Label
    _drawText(canvas, 'PIVOT', size.x / 2, size.y + 15, fontSize: 12);
  }
  
  void _drawText(Canvas canvas, String text, double x, double y,
      {double fontSize = 14}) {
    final tp = TextPainter(
      text: TextSpan(
        text: text,
        style: TextStyle(
          color: Colors.white,
          fontSize: fontSize,
          fontWeight: FontWeight.bold,
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    tp.paint(canvas, Offset(x - tp.width / 2, y - tp.height / 2));
  }
}

class DraggableSpanner extends PositionComponent with DragCallbacks {
  final double spannerDistance; // 2.0 or 4.0
  final Vector2 startPosition;
  final void Function(DraggableSpanner) onDroppedOnNut;

  bool isDragging = false;
  bool isAttached = false;

  DraggableSpanner({
    required this.spannerDistance,
    required this.startPosition,
    required this.onDroppedOnNut,
  }) : super(
          position: startPosition,
          size: Vector2(spannerDistance * 50, 30), // Length visually proportional to distance
          anchor: Anchor.centerLeft,
        );

  @override
  void onDragStart(DragStartEvent event) {
    if (isAttached) return;
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
    onDroppedOnNut(this);
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
    isAttached = false;
    position = startPosition;
    angle = 0;
  }

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);

    if (isDragging) {
      final shadowPaint = Paint()
        ..color = Colors.black.withAlpha(80)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 8);
      canvas.drawRect(Rect.fromLTWH(5, 5, size.x, size.y), shadowPaint);
    }

    // Draw Spanner handle
    canvas.drawRRect(
      RRect.fromRectAndRadius(rect, const Radius.circular(8)),
      Paint()..color = Colors.grey[400]!,
    );
    
    // Draw Spanner Head
    canvas.drawCircle(Offset(10, size.y/2), 20, Paint()..color = Colors.grey[400]!);
    canvas.drawCircle(Offset(10, size.y/2), 12, Paint()..color = Colors.black54);

    _drawText(canvas, '${spannerDistance.toInt()}m', size.x / 2, size.y / 2);
  }

  void _drawText(Canvas canvas, String text, double x, double y) {
    final tp = TextPainter(
      text: TextSpan(
        text: text,
        style: const TextStyle(
          color: Colors.black87,
          fontSize: 16,
          fontWeight: FontWeight.bold,
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    tp.paint(canvas, Offset(x - tp.width / 2, y - tp.height / 2));
  }
}

class DraggableHand extends PositionComponent with DragCallbacks {
  final Vector2 startPosition;
  final void Function(DraggableHand) onDroppedOnHand;
  
  final double forceApplied = 50.0;

  bool isDragging = false;
  bool isApplyingForce = false;

  DraggableHand({
    required this.startPosition,
    required this.onDroppedOnHand,
  }) : super(
          position: startPosition,
          size: Vector2(60, 60),
          anchor: Anchor.bottomCenter,
        );

  @override
  void onDragStart(DragStartEvent event) {
    if (isApplyingForce) return;
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
    onDroppedOnHand(this);
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
    isApplyingForce = false;
    position = startPosition;
  }

  @override
  void render(Canvas canvas) {
    if (isDragging) {
      final shadowPaint = Paint()
        ..color = Colors.black.withAlpha(80)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 8);
      canvas.drawCircle(Offset(size.x/2 + 5, size.y/2 + 5), size.x/2, shadowPaint);
    }

    // Draw hand as a generic symbol (circle with arrow)
    canvas.drawCircle(Offset(size.x/2, size.y/2), size.x/2, Paint()..color = Colors.amber[700]!);
    
    // Draw downward arrow
    final path = Path()
      ..moveTo(size.x/2, size.y * 0.2)
      ..lineTo(size.x/2, size.y * 0.7)
      ..moveTo(size.x * 0.3, size.y * 0.5)
      ..lineTo(size.x/2, size.y * 0.7)
      ..lineTo(size.x * 0.7, size.y * 0.5);
      
    canvas.drawPath(path, Paint()
      ..color = Colors.white
      ..strokeWidth = 4
      ..style = PaintingStyle.stroke
      ..strokeCap = StrokeCap.round
    );
    
    _drawText(canvas, '${forceApplied.toInt()}N', size.x / 2, size.y + 15);
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

// ==========================================
// 3. Flame Game Logic
// ==========================================

class SimpleMachinesGame extends FlameGame {
  SimpleMachinesGame({
    required this.onShowCalculationDialog,
    required this.onShowResultDialog,
    required this.onShowErrorDialog,
  });

  final void Function(double force, double distance) onShowCalculationDialog;
  final void Function(bool isSuccess) onShowResultDialog;
  final void Function(String message) onShowErrorDialog;

  late RustedNutComponent nut;
  late DraggableSpanner shortSpanner;
  late DraggableSpanner longSpanner;
  late DraggableHand hand;

  final double minMoment = 150.0;
  
  DraggableSpanner? attachedSpanner;

  @override
  Color backgroundColor() => const Color(0xFF1E293B);

  @override
  Future<void> onLoad() async {
    await super.onLoad();

    nut = RustedNutComponent(position: Vector2(size.x * 0.3, size.y * 0.5));
    add(nut);

    shortSpanner = DraggableSpanner(
      spannerDistance: 2.0,
      startPosition: Vector2(size.x * 0.6, size.y * 0.2),
      onDroppedOnNut: _onSpannerDropped,
    );
    add(shortSpanner);

    longSpanner = DraggableSpanner(
      spannerDistance: 4.0,
      startPosition: Vector2(size.x * 0.6, size.y * 0.4),
      onDroppedOnNut: _onSpannerDropped,
    );
    add(longSpanner);

    hand = DraggableHand(
      startPosition: Vector2(size.x * 0.8, size.y * 0.8),
      onDroppedOnHand: _onHandDropped,
    );
    add(hand);
  }

  void _onSpannerDropped(DraggableSpanner spanner) {
    if (attachedSpanner != null) {
      onShowErrorDialog('A spanner is already attached!');
      spanner.returnToStart();
      return;
    }

    final globalNutPos = nut.position;
    
    if ((spanner.position - globalNutPos).length < 100) {
      spanner.isAttached = true;
      attachedSpanner = spanner;
      
      // Snap position directly to nut
      spanner.position = globalNutPos;
    } else {
      spanner.returnToStart();
    }
  }

  void _onHandDropped(DraggableHand handComp) {
    if (attachedSpanner == null) {
      onShowErrorDialog('Attach a spanner to the nut first!');
      handComp.returnToStart();
      return;
    }

    // Get the global position of the far end of the attached spanner
    final endOfSpannerGlobal = nut.position + Vector2(attachedSpanner!.size.x, 0);

    if ((handComp.position - endOfSpannerGlobal).length < 80) {
      handComp.isApplyingForce = true;
      handComp.position = endOfSpannerGlobal;
      
      pauseEngine();
      onShowCalculationDialog(handComp.forceApplied, attachedSpanner!.spannerDistance);
    } else {
      handComp.returnToStart();
    }
  }

  void handleCalculationSuccess(double calculatedMoment) {
    resumeEngine();
    
    final bool isSuccess = calculatedMoment >= minMoment;
    
    if (isSuccess) {
      // Rotate effect! Nut is successfully broken loose.
      nut.add(
        RotateEffect.by(
          pi / 2, // 90 degrees
          EffectController(duration: 1.0, curve: Curves.easeInOut),
          onComplete: () {
            onShowResultDialog(true);
          }
        )
      );
      
      attachedSpanner!.add(
        RotateEffect.by(
          pi / 2,
          EffectController(duration: 1.0, curve: Curves.easeInOut)
        )
      );
      
      // Animate hand down with the spanner
      hand.add(
        MoveEffect.by(
          Vector2(0, attachedSpanner!.size.x), // Moving down proportionally
          EffectController(duration: 1.0, curve: Curves.easeInOut)
        )
      );
      
    } else {
      // Shake effect! Not enough moment.
      final shakeController = EffectController(duration: 0.1);
      final sequence = SequenceEffect([
        RotateEffect.by(0.1, shakeController),
        RotateEffect.by(-0.2, shakeController),
        RotateEffect.by(0.2, shakeController),
        RotateEffect.by(-0.1, shakeController),
      ], onComplete: () {
        onShowResultDialog(false);
      });
      
      nut.add(sequence);
      
      attachedSpanner!.add(SequenceEffect([
        RotateEffect.by(0.1, shakeController),
        RotateEffect.by(-0.2, shakeController),
        RotateEffect.by(0.2, shakeController),
        RotateEffect.by(-0.1, shakeController),
      ]));
    }
  }

  void resetGame() {
    // Reset nut rotation
    nut.angle = 0;
    
    // Unattach spanner
    if (attachedSpanner != null) {
      attachedSpanner!.returnToStart();
      attachedSpanner = null;
    }
    
    // Reset unattached spanner just in case
    shortSpanner.returnToStart();
    longSpanner.returnToStart();
    
    hand.returnToStart();
    
    // Remove all ongoing effects
    nut.removeAll(nut.children.whereType<Effect>());
    shortSpanner.removeAll(shortSpanner.children.whereType<Effect>());
    longSpanner.removeAll(longSpanner.children.whereType<Effect>());
    hand.removeAll(hand.children.whereType<Effect>());
    
    resumeEngine();
  }
}

// ==========================================
// 4. Flutter Screen
// ==========================================

class SimpleMachinesScreen extends StatefulWidget {
  const SimpleMachinesScreen({super.key});

  @override
  State<SimpleMachinesScreen> createState() => _SimpleMachinesScreenState();
}

class _SimpleMachinesScreenState extends State<SimpleMachinesScreen> {
  late final SimpleMachinesGame _game;

  bool _showCalculation = false;
  double _currentForce = 0;
  double _currentDistance = 0;

  bool _showResult = false;
  bool _isSuccess = false;

  @override
  void initState() {
    super.initState();
    _game = SimpleMachinesGame(
      onShowCalculationDialog: (force, distance) {
        setState(() {
          _currentForce = force;
          _currentDistance = distance;
          _showCalculation = true;
        });
      },
      onShowResultDialog: (success) {
        setState(() {
          _isSuccess = success;
          _showResult = true;
        });
      },
      onShowErrorDialog: (message) {
        if (!mounted) return;
        ScaffoldMessenger.of(context).clearSnackBars();
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(message),
            backgroundColor: Colors.orange[800],
            duration: const Duration(seconds: 2),
          ),
        );
      },
    );
  }

  void _onCalculationSubmit(bool isCorrect, double calculatedMoment) {
    if (isCorrect) {
      setState(() {
        _showCalculation = false;
      });
      _game.handleCalculationSuccess(calculatedMoment);
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
        title: const Text('Puzzle: The Stubborn Rusted Nut'),
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
                  force: _currentForce,
                  distance: _currentDistance,
                  onSubmit: _onCalculationSubmit,
                ),
              ),
            ),
          if (_showResult)
            Positioned.fill(
              child: Container(
                color: Colors.black54,
                child: ResultOverlay(
                  isSuccess: _isSuccess,
                  onAction: _onReset,
                ),
              ),
            ),
        ],
      ),
    );
  }
}
