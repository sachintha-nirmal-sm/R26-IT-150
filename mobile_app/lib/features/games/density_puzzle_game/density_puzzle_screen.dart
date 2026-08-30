import 'package:flame/components.dart';
import 'package:flame/events.dart';
import 'package:flame/game.dart';
import 'package:flutter/material.dart';
import 'dart:math';

// ---------------------------------------------------------
// 1. Flutter Overlays
// ---------------------------------------------------------

class CalculationOverlay extends StatefulWidget {
  final String itemName;
  final double mass;
  final double volume;
  final void Function(bool isCorrect, double calculatedDensity) onSubmit;

  const CalculationOverlay({
    super.key,
    required this.itemName,
    required this.mass,
    required this.volume,
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

    final correctDensity = widget.mass / widget.volume;

    // Small epsilon for floating point issues
    if ((input - correctDensity).abs() < 0.01) {
      widget.onSubmit(true, correctDensity);
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
            constraints: const BoxConstraints(maxWidth: 400),
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
                  'Calculate Density',
                  style: TextStyle(
                    fontSize: 22,
                    fontWeight: FontWeight.bold,
                    color: Colors.black87,
                  ),
                ),
                const SizedBox(height: 16),
                Text(
                  'You dropped the ${widget.itemName}.\n'
                  'Its Mass is ${widget.mass.toInt()}g and Volume is ${widget.volume.toInt()}cm³.\n\n'
                  'What is its Density?',
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
                    hintText: 'Enter value',
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(8),
                    ),
                    errorText: _showError
                        ? 'Hint: Density (d) = Mass (m) / Volume (v)'
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

class OutcomeOverlay extends StatelessWidget {
  final bool sinks;
  final String itemName;
  final VoidCallback onRetry;
  final VoidCallback onNext;

  const OutcomeOverlay({
    super.key,
    required this.sinks,
    required this.itemName,
    required this.onRetry,
    required this.onNext,
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
              color: Colors.blueAccent,
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
                sinks ? Icons.arrow_downward : Icons.waves,
                color: Colors.blueAccent,
                size: 64,
              ),
              const SizedBox(height: 16),
              Text(
                'Correct!',
                style: TextStyle(
                  fontSize: 28,
                  fontWeight: FontWeight.bold,
                  color: Colors.blue[800],
                ),
              ),
              const SizedBox(height: 16),
              Text(
                sinks
                    ? 'The density is greater than 1.0, so the $itemName SINKS!'
                    : 'The density is less than 1.0, so the $itemName FLOATS!',
                textAlign: TextAlign.center,
                style: const TextStyle(fontSize: 16, color: Colors.black87),
              ),
              const SizedBox(height: 32),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                children: [
                  ElevatedButton(
                    onPressed: onRetry,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.grey[700],
                      padding: const EdgeInsets.symmetric(
                          horizontal: 24, vertical: 12),
                    ),
                    child: const Text('Reset',
                        style: TextStyle(color: Colors.white, fontSize: 16)),
                  ),
                  ElevatedButton(
                    onPressed: onNext,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.green[600],
                      padding: const EdgeInsets.symmetric(
                          horizontal: 24, vertical: 12),
                    ),
                    child: const Text('Next Puzzle',
                        style: TextStyle(color: Colors.white, fontSize: 16)),
                  ),
                ],
              )
            ],
          ),
        ),
      ),
    );
  }
}

// ---------------------------------------------------------
// 2. Flame Components
// ---------------------------------------------------------

class ScaleComponent extends PositionComponent {
  ScaleComponent({required super.position})
      : super(size: Vector2(120, 80), anchor: Anchor.center);

  double currentMass = 0.0;

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    canvas.drawRRect(
      RRect.fromRectAndRadius(rect, const Radius.circular(8)),
      Paint()..color = Colors.grey[800]!,
    );
    canvas.drawRect(
      Rect.fromLTWH(10, 10, size.x - 20, 30),
      Paint()..color = Colors.green[100]!,
    );

    final displayMass = currentMass > 0 ? '${currentMass.toInt()}g' : '---';
    _drawText(canvas, displayMass, size.x / 2, 25, color: Colors.black);
    _drawText(canvas, 'SCALE', size.x / 2, size.y - 15, fontSize: 14);
  }

  void _drawText(Canvas canvas, String text, double x, double y,
      {Color color = Colors.white, double fontSize = 18}) {
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

class CylinderComponent extends PositionComponent {
  CylinderComponent({required super.position})
      : super(size: Vector2(100, 150), anchor: Anchor.center);

  double currentVolume = 0.0;

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    // Draw Glass Cylinder
    canvas.drawRRect(
      RRect.fromRectAndRadius(rect, const Radius.circular(12)),
      Paint()
        ..color = Colors.blueAccent.withAlpha(50)
        ..style = PaintingStyle.fill,
    );
    canvas.drawRRect(
      RRect.fromRectAndRadius(rect, const Radius.circular(12)),
      Paint()
        ..color = Colors.blueAccent
        ..strokeWidth = 3
        ..style = PaintingStyle.stroke,
    );

    final displayVol = currentVolume > 0 ? '${currentVolume.toInt()}cm³' : '---';
    _drawText(canvas, displayVol, size.x / 2, size.y / 2, color: Colors.blue[900]!);
    _drawText(canvas, 'CYLINDER', size.x / 2, size.y - 20,
        fontSize: 12, color: Colors.blue[900]!);
  }

  void _drawText(Canvas canvas, String text, double x, double y,
      {Color color = Colors.white, double fontSize = 18}) {
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

class WaterTankComponent extends PositionComponent {
  WaterTankComponent({required super.position})
      : super(size: Vector2(250, 200), anchor: Anchor.center);

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    // Water Fill
    canvas.drawRect(
      rect,
      Paint()..color = Colors.cyan.withAlpha(100),
    );
    // Tank Outline
    canvas.drawRect(
      rect,
      Paint()
        ..color = Colors.cyan[800]!
        ..strokeWidth = 4
        ..style = PaintingStyle.stroke,
    );
    
    // Water surface line
    canvas.drawLine(
      const Offset(0, 5),
      Offset(size.x, 5),
      Paint()
        ..color = Colors.white.withAlpha(150)
        ..strokeWidth = 2,
    );

    _drawText(canvas, 'WATER TANK', size.x / 2, size.y - 20, fontSize: 16);
    _drawText(canvas, 'Density = 1.0 g/cm³', size.x / 2, size.y - 40,
        fontSize: 14);
  }

  void _drawText(Canvas canvas, String text, double x, double y,
      {double fontSize = 18}) {
    final tp = TextPainter(
      text: TextSpan(
        text: text,
        style: TextStyle(
          color: Colors.cyan[900],
          fontSize: fontSize,
          fontWeight: FontWeight.bold,
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    tp.paint(canvas, Offset(x - tp.width / 2, y - tp.height / 2));
  }
}

enum ItemType { wood, iron }

enum PhysicsState { idle, sinking, floating }

class DraggableItem extends PositionComponent with DragCallbacks {
  DraggableItem({
    required this.type,
    required this.startPosition,
    required this.onDroppedCallback,
  }) : super(
          position: startPosition,
          size: type == ItemType.wood ? Vector2(70, 70) : Vector2(40, 40),
          anchor: Anchor.center,
        );

  final ItemType type;
  final Vector2 startPosition;
  final void Function(DraggableItem) onDroppedCallback;

  bool isDragging = false;
  PhysicsState physicsState = PhysicsState.idle;
  
  bool hasMeasuredMass = false;
  bool hasMeasuredVolume = false;
  
  // Animation variables
  double _time = 0;
  double _targetY = 0;

  double get mass => type == ItemType.wood ? 40.0 : 78.0;
  double get volume => type == ItemType.wood ? 50.0 : 10.0;
  double get density => mass / volume;
  String get name => type == ItemType.wood ? 'Wooden Block' : 'Iron Key';

  @override
  void update(double dt) {
    super.update(dt);
    
    if (physicsState == PhysicsState.sinking) {
      if (position.y < _targetY) {
        position.y += 100 * dt; // Sink speed
      }
    } else if (physicsState == PhysicsState.floating) {
      _time += dt;
      // Gentle sine-wave bobbing at targetY
      position.y = _targetY + sin(_time * 3) * 5; 
    }
  }

  @override
  void onDragStart(DragStartEvent event) {
    super.onDragStart(event);
    isDragging = true;
    physicsState = PhysicsState.idle; // Stop physics if dragged
    priority = 10;
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
    priority = 1;
    onDroppedCallback(this);
  }

  @override
  void onDragCancel(DragCancelEvent event) {
    super.onDragCancel(event);
    isDragging = false;
    priority = 1;
    returnToStart();
  }

  void returnToStart() {
    physicsState = PhysicsState.idle;
    hasMeasuredMass = false;
    hasMeasuredVolume = false;
    position = startPosition;
  }
  
  void triggerSinking(double tankBottomY) {
    _targetY = tankBottomY - size.y / 2 - 10; // Rest at bottom
    physicsState = PhysicsState.sinking;
  }
  
  void triggerFloating(double tankTopY) {
    _targetY = tankTopY + size.y / 2; // Bob at surface
    physicsState = PhysicsState.floating;
  }

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    
    if (isDragging) {
      final shadowPaint = Paint()
        ..color = Colors.black.withAlpha(80)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 8);
      canvas.drawRect(Rect.fromLTWH(10, 10, size.x, size.y), shadowPaint);
    }

    if (type == ItemType.wood) {
      canvas.drawRRect(
        RRect.fromRectAndRadius(rect, const Radius.circular(8)),
        Paint()..color = Colors.brown[600]!,
      );
      _drawText(canvas, 'WOOD', size.x / 2, size.y / 2);
    } else {
      canvas.drawRRect(
        RRect.fromRectAndRadius(rect, const Radius.circular(4)),
        Paint()..color = Colors.grey[600]!,
      );
      // Key visual elements
      canvas.drawCircle(Offset(size.x / 2, size.y * 0.3), 8, Paint()..color = Colors.black45);
      _drawText(canvas, 'IRON', size.x / 2, size.y * 0.7, fontSize: 10);
    }
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

// ---------------------------------------------------------
// 3. Flame Game Logic
// ---------------------------------------------------------

class DensityPuzzleGame extends FlameGame {
  DensityPuzzleGame({
    required this.onShowCalculationDialog,
    required this.onShowOutcomeDialog,
    required this.onShowErrorDialog,
  });

  final void Function(DraggableItem item) onShowCalculationDialog;
  final void Function(bool sinks, String itemName) onShowOutcomeDialog;
  final void Function(String message) onShowErrorDialog;

  late ScaleComponent scale;
  late CylinderComponent cylinder;
  late WaterTankComponent waterTank;
  
  late DraggableItem woodItem;
  late DraggableItem ironItem;
  
  DraggableItem? currentTestingItem;

  @override
  Color backgroundColor() => const Color(0xFFF4F6FB);

  @override
  Future<void> onLoad() async {
    await super.onLoad();

    // Setup Zones
    scale = ScaleComponent(position: Vector2(size.x * 0.2, size.y * 0.8));
    add(scale);

    cylinder = CylinderComponent(position: Vector2(size.x * 0.5, size.y * 0.8));
    add(cylinder);

    waterTank = WaterTankComponent(position: Vector2(size.x * 0.85, size.y * 0.6));
    add(waterTank);

    // Setup Draggable Items
    woodItem = DraggableItem(
      type: ItemType.wood,
      startPosition: Vector2(size.x * 0.2, size.y * 0.2),
      onDroppedCallback: _onItemDropped,
    );
    add(woodItem);

    ironItem = DraggableItem(
      type: ItemType.iron,
      startPosition: Vector2(size.x * 0.5, size.y * 0.2),
      onDroppedCallback: _onItemDropped,
    );
    add(ironItem);
  }

  void _onItemDropped(DraggableItem item) {
    // Reset measuring tools
    scale.currentMass = 0;
    cylinder.currentVolume = 0;

    // Check Scale
    if ((item.position - scale.position).length < 80) {
      item.position = scale.position - Vector2(0, scale.size.y / 2 + item.size.y / 2);
      scale.currentMass = item.mass;
      item.hasMeasuredMass = true;
      return;
    }

    // Check Cylinder
    if ((item.position - cylinder.position).length < 80) {
      item.position = cylinder.position - Vector2(0, cylinder.size.y / 2 + item.size.y / 2);
      cylinder.currentVolume = item.volume;
      item.hasMeasuredVolume = true;
      return;
    }

    // Check Water Tank
    if ((item.position - waterTank.position).length < 150) {
      if (!item.hasMeasuredMass || !item.hasMeasuredVolume) {
        onShowErrorDialog('You must measure both Mass and Volume first!');
        item.returnToStart();
        return;
      }
      
      // Snap to top center of tank
      item.position = Vector2(waterTank.position.x, waterTank.position.y - waterTank.size.y / 2 + item.size.y / 2);
      currentTestingItem = item;
      
      pauseEngine();
      onShowCalculationDialog(item);
      return;
    }

    // Dropped elsewhere
    item.returnToStart();
  }

  void handleCalculationSuccess() {
    resumeEngine();
    
    if (currentTestingItem == null) return;
    
    final item = currentTestingItem!;
    final bool sinks = item.density > 1.0;
    
    if (sinks) {
      item.triggerSinking(waterTank.position.y + waterTank.size.y / 2);
    } else {
      item.triggerFloating(waterTank.position.y - waterTank.size.y / 2);
    }
    
    // Show outcome after a brief delay so they see the animation start
    Future.delayed(const Duration(seconds: 1), () {
      onShowOutcomeDialog(sinks, item.name);
    });
  }

  void resetGame() {
    woodItem.returnToStart();
    ironItem.returnToStart();
    scale.currentMass = 0;
    cylinder.currentVolume = 0;
    currentTestingItem = null;
    resumeEngine();
  }
}

// ---------------------------------------------------------
// 4. Flutter Screen
// ---------------------------------------------------------

class DensityPuzzleScreen extends StatefulWidget {
  const DensityPuzzleScreen({super.key});

  @override
  State<DensityPuzzleScreen> createState() => _DensityPuzzleScreenState();
}

class _DensityPuzzleScreenState extends State<DensityPuzzleScreen> {
  late final DensityPuzzleGame _game;

  bool _showCalculation = false;
  DraggableItem? _activeItem;

  bool _showOutcome = false;
  bool _outcomeSinks = false;
  String _outcomeItemName = '';

  @override
  void initState() {
    super.initState();
    _game = DensityPuzzleGame(
      onShowCalculationDialog: (item) {
        setState(() {
          _activeItem = item;
          _showCalculation = true;
        });
      },
      onShowOutcomeDialog: (sinks, itemName) {
        setState(() {
          _outcomeSinks = sinks;
          _outcomeItemName = itemName;
          _showOutcome = true;
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

  void _onCalculationSubmit(bool isCorrect, double calculatedDensity) {
    if (isCorrect) {
      setState(() {
        _showCalculation = false;
      });
      _game.handleCalculationSuccess();
    }
  }

  void _onRetry() {
    setState(() {
      _showOutcome = false;
      _showCalculation = false;
    });
    _game.resetGame();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Puzzle 2: The Density Lab'),
        backgroundColor: Colors.blueAccent,
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Reset Lab',
            onPressed: _onRetry,
          ),
        ],
      ),
      body: Stack(
        children: [
          GameWidget(game: _game),
          if (_showCalculation && _activeItem != null)
            Positioned.fill(
              child: Container(
                color: Colors.black54,
                child: CalculationOverlay(
                  itemName: _activeItem!.name,
                  mass: _activeItem!.mass,
                  volume: _activeItem!.volume,
                  onSubmit: _onCalculationSubmit,
                ),
              ),
            ),
          if (_showOutcome)
            Positioned.fill(
              child: Container(
                color: Colors.black54,
                child: OutcomeOverlay(
                  sinks: _outcomeSinks,
                  itemName: _outcomeItemName,
                  onRetry: _onRetry,
                  onNext: () {
                    Navigator.of(context).pop();
                  },
                ),
              ),
            ),
        ],
      ),
    );
  }
}
