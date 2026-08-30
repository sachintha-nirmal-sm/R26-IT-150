import 'package:flame/components.dart';
import 'package:flame/effects.dart';
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

    if (input == 20) {
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
                  'The Lotus Effect: Scale Conversion',
                  style: TextStyle(
                    fontSize: 22,
                    fontWeight: FontWeight.bold,
                    color: Colors.black87,
                  ),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 16),
                const Text(
                  'To create a hydrophobic lotus effect, the surface bumps must be smaller than 50 nm.\n\n'
                  'Your coating particles have a size of 2 x 10^-8 meters.\n\n'
                  'What is this size in nanometers (nm)?',
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
                    hintText: 'Enter size in nm',
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(8),
                    ),
                    errorText: _showError
                        ? 'Hint: Multiply the size in meters by 10^9 to convert to nm.'
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
                isSuccess ? 'Stain-Proof Success!' : 'Game Over!',
                style: TextStyle(
                  fontSize: 26,
                  fontWeight: FontWeight.bold,
                  color: isSuccess ? Colors.green[800] : Colors.red[800],
                ),
              ),
              const SizedBox(height: 16),
              Text(
                isSuccess
                    ? 'Correct! 20 nm is less than 50 nm.\n\nThe Lotus Effect works and repels the mud completely!'
                    : 'Oops! Something went wrong.',
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

enum FabricType { normal, nano }

class FabricComponent extends PositionComponent {
  final FabricType type;
  bool isStained = false;

  FabricComponent({
    required this.type,
    required super.position,
    required super.size,
  }) : super(anchor: Anchor.center);

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    
    // Determine color based on state and type
    Color fabricColor;
    if (type == FabricType.normal) {
      fabricColor = isStained ? const Color(0xFF5D4037) : Colors.white; // Brown if stained
    } else {
      fabricColor = const Color(0xFFC8E6C9); // Light green for Nano
    }

    // Draw main fabric
    canvas.drawRect(
      rect,
      Paint()..color = fabricColor,
    );
    
    // Draw border
    canvas.drawRect(
      rect,
      Paint()
        ..color = type == FabricType.normal ? Colors.grey[400]! : Colors.green[800]!
        ..strokeWidth = 4
        ..style = PaintingStyle.stroke,
    );

    // Draw labels
    if (isStained && type == FabricType.normal) {
      _drawText(canvas, 'STAINED!', size.x / 2, size.y / 2 - 10, color: Colors.white, fontSize: 24);
      _drawText(canvas, 'Normal fabric absorbs water/mud.', size.x / 2, size.y / 2 + 20, color: Colors.white70, fontSize: 14);
    } else {
      final label = type == FabricType.normal ? 'Normal Cotton Fabric' : 'Nano-Coated Fabric';
      final textColor = type == FabricType.normal ? Colors.black87 : Colors.green[900]!;
      _drawText(canvas, label, size.x / 2, size.y / 2, color: textColor, fontSize: 18);
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
      textDirection: TextDirection.ltr,
    )..layout();
    tp.paint(canvas, Offset(x - tp.width / 2, y - tp.height / 2));
  }
}

class DraggableMudDrop extends PositionComponent with DragCallbacks {
  final Vector2 startPosition;
  final void Function(DraggableMudDrop) onDropped;

  bool isDragging = false;
  bool isAnimating = false;
  bool isVisible = true;

  DraggableMudDrop({
    required this.startPosition,
    required this.onDropped,
  }) : super(
          position: startPosition,
          size: Vector2(40, 40),
          anchor: Anchor.center,
        );

  @override
  void onDragStart(DragStartEvent event) {
    if (isAnimating) return;
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
  }

  @override
  void render(Canvas canvas) {
    if (!isVisible) return;

    if (isDragging) {
      final shadowPaint = Paint()
        ..color = Colors.black.withAlpha(80)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 8);
      canvas.drawCircle(Offset(size.x/2 + 5, size.y/2 + 5), size.x/2, shadowPaint);
    }

    // Draw mud drop (brown circle)
    canvas.drawCircle(Offset(size.x/2, size.y/2), size.x/2, Paint()..color = const Color(0xFF6D4C41));
    
    // Draw slight highlight for liquid effect
    canvas.drawCircle(Offset(size.x * 0.3, size.y * 0.3), size.x * 0.15, Paint()..color = Colors.white30);
  }
}

// ==========================================
// 3. Flame Game Logic
// ==========================================

class NanoShieldGame extends FlameGame {
  NanoShieldGame({
    required this.onShowCalculationDialog,
    required this.onShowResultDialog,
  });

  final void Function() onShowCalculationDialog;
  final void Function(bool isSuccess) onShowResultDialog;

  late FabricComponent normalFabric;
  late FabricComponent nanoFabric;
  late DraggableMudDrop mudDrop;

  @override
  Color backgroundColor() => const Color(0xFFE2E8F0);

  @override
  Future<void> onLoad() async {
    await super.onLoad();

    final fabricWidth = size.x * 0.4;
    final fabricHeight = size.y * 0.4;

    normalFabric = FabricComponent(
      type: FabricType.normal,
      position: Vector2(size.x * 0.25, size.y * 0.7),
      size: Vector2(fabricWidth, fabricHeight),
    );
    add(normalFabric);

    nanoFabric = FabricComponent(
      type: FabricType.nano,
      position: Vector2(size.x * 0.75, size.y * 0.7),
      size: Vector2(fabricWidth, fabricHeight),
    );
    // Slight angle to represent a smooth tilted surface for the lotus effect
    nanoFabric.angle = 0.1; 
    add(nanoFabric);

    mudDrop = DraggableMudDrop(
      startPosition: Vector2(size.x * 0.5, size.y * 0.2),
      onDropped: _onMudDropped,
    );
    add(mudDrop);
  }

  void _onMudDropped(DraggableMudDrop mud) {
    // Check collision with normal fabric
    if (_isDroppedOnFabric(mud, normalFabric)) {
      // Normal fabric absorbs mud
      normalFabric.isStained = true;
      mud.isVisible = false; // Disappears
      mud.position = mud.startPosition; // Reset position secretly
      
      // Auto-reset normal fabric stain after 3 seconds for replayability
      Future.delayed(const Duration(seconds: 3), () {
        if (normalFabric.isMounted) {
          normalFabric.isStained = false;
          mud.isVisible = true;
        }
      });
      return;
    }

    // Check collision with nano fabric
    if (_isDroppedOnFabric(mud, nanoFabric)) {
      // Snap to top edge of nano fabric
      mud.position = Vector2(nanoFabric.position.x - nanoFabric.size.x * 0.2, nanoFabric.position.y - nanoFabric.size.y / 2 - mud.size.y / 2);
      
      pauseEngine();
      onShowCalculationDialog();
      return;
    }

    // Dropped elsewhere
    mud.returnToStart();
  }

  bool _isDroppedOnFabric(DraggableMudDrop mud, FabricComponent fabric) {
    // Simple bounding box collision
    final rect = Rect.fromCenter(
      center: Offset(fabric.position.x, fabric.position.y),
      width: fabric.size.x,
      height: fabric.size.y,
    );
    return rect.contains(Offset(mud.position.x, mud.position.y));
  }

  void handleCalculationSuccess() {
    resumeEngine();
    
    mudDrop.isAnimating = true;

    // Roll off animation (Lotus Effect)
    // Move diagonally down-right and fall off screen
    final rollEffect = MoveEffect.by(
      Vector2(nanoFabric.size.x * 0.8, nanoFabric.size.y * 1.5),
      EffectController(duration: 1.5, curve: Curves.easeIn),
      onComplete: () {
        onShowResultDialog(true);
      }
    );
    
    // Bounce effect while rolling
    final bounceEffect = SequenceEffect([
      MoveEffect.by(Vector2(0, -20), EffectController(duration: 0.2, curve: Curves.easeOut)),
      MoveEffect.by(Vector2(0, 20), EffectController(duration: 0.2, curve: Curves.easeIn)),
      MoveEffect.by(Vector2(0, -10), EffectController(duration: 0.15, curve: Curves.easeOut)),
      MoveEffect.by(Vector2(0, 10), EffectController(duration: 0.15, curve: Curves.easeIn)),
    ]);

    mudDrop.add(rollEffect);
    mudDrop.add(bounceEffect);
  }

  void resetGame() {
    mudDrop.removeAll(mudDrop.children.whereType<Effect>());
    mudDrop.isAnimating = false;
    mudDrop.isVisible = true;
    mudDrop.returnToStart();
    normalFabric.isStained = false;
    resumeEngine();
  }
}

// ==========================================
// 4. Flutter Screen
// ==========================================

class NanoShieldScreen extends StatefulWidget {
  const NanoShieldScreen({super.key});

  @override
  State<NanoShieldScreen> createState() => _NanoShieldScreenState();
}

class _NanoShieldScreenState extends State<NanoShieldScreen> {
  late final NanoShieldGame _game;

  bool _showCalculation = false;
  bool _showResult = false;
  bool _isSuccess = false;

  @override
  void initState() {
    super.initState();
    _game = NanoShieldGame(
      onShowCalculationDialog: () {
        setState(() {
          _showCalculation = true;
        });
      },
      onShowResultDialog: (success) {
        setState(() {
          _isSuccess = success;
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
        title: const Text('The Nano Shield: Lotus Effect'),
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
