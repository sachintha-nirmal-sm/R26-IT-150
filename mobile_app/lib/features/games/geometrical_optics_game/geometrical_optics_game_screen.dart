import 'package:flame/components.dart';
import 'package:flame/events.dart';
import 'package:flame/game.dart';
import 'package:flutter/material.dart';
import 'dart:math';
import '../../lessons/Lessons_Dashboard.dart';

// ---------------------------------------------------------
// 1. Flutter Overlays
// ---------------------------------------------------------

class OpticsOverlay extends StatefulWidget {
  final String positionName;
  final void Function(String type, String orientation, String size) onSubmit;

  const OpticsOverlay({
    super.key,
    required this.positionName,
    required this.onSubmit,
  });

  @override
  State<OpticsOverlay> createState() => _OpticsOverlayState();
}

class _OpticsOverlayState extends State<OpticsOverlay>
    with SingleTickerProviderStateMixin {
  String? _selectedType;
  String? _selectedOrientation;
  String? _selectedSize;
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
    _animationController.dispose();
    super.dispose();
  }

  void _submit() {
    if (_selectedType == null || _selectedOrientation == null || _selectedSize == null) return;

    // Validate inputs based on object position
    bool isCorrect = false;
    final pos = widget.positionName;

    if (pos == 'Beyond 2F1') {
      isCorrect = _selectedType == 'Real' &&
          _selectedOrientation == 'Inverted' &&
          _selectedSize == 'Diminished';
    } else if (pos == 'At 2F1') {
      isCorrect = _selectedType == 'Real' &&
          _selectedOrientation == 'Inverted' &&
          _selectedSize == 'Same Size';
    } else if (pos == 'Between F1 and 2F1') {
      isCorrect = _selectedType == 'Real' &&
          _selectedOrientation == 'Inverted' &&
          _selectedSize == 'Magnified';
    }

    if (isCorrect) {
      widget.onSubmit(_selectedType!, _selectedOrientation!, _selectedSize!);
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
        padding: const EdgeInsets.all(16),
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
                color: const Color(0xFF1E293B),
                borderRadius: BorderRadius.circular(20),
                border: Border.all(
                  color: Colors.blueAccent.withAlpha(150),
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
                          color: Colors.blueAccent.withAlpha(30),
                          shape: BoxShape.circle,
                        ),
                        child: const Icon(
                          Icons.visibility_outlined,
                          color: Colors.blueAccent,
                          size: 28,
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Text(
                          'Image Theory Checkpoint',
                          style: TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.bold,
                            color: Colors.blueAccent[100],
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 12),
                  Text(
                    'You placed the Candle ${widget.positionName}. Predict the nature of the image formed:',
                    style: const TextStyle(color: Colors.white70, fontSize: 13, height: 1.4),
                  ),
                  const SizedBox(height: 16),
                  
                  // Dropdown 1: Image Type
                  _buildDropdown(
                    label: 'Image Type',
                    value: _selectedType,
                    items: ['Real', 'Virtual'],
                    onChanged: (val) => setState(() => _selectedType = val),
                  ),
                  const SizedBox(height: 12),
                  
                  // Dropdown 2: Orientation
                  _buildDropdown(
                    label: 'Orientation',
                    value: _selectedOrientation,
                    items: ['Upright', 'Inverted'],
                    onChanged: (val) => setState(() => _selectedOrientation = val),
                  ),
                  const SizedBox(height: 12),
                  
                  // Dropdown 3: Size
                  _buildDropdown(
                    label: 'Size',
                    value: _selectedSize,
                    items: ['Diminished', 'Same Size', 'Magnified'],
                    onChanged: (val) => setState(() => _selectedSize = val),
                  ),

                  if (_showError) ...[
                    const SizedBox(height: 12),
                    const Text(
                      'Hint: Review your convex lens rules! Real images are always inverted.',
                      textAlign: TextAlign.center,
                      style: TextStyle(
                        color: Colors.redAccent,
                        fontSize: 12,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ],
                  const SizedBox(height: 20),
                  SizedBox(
                    width: double.infinity,
                    height: 46,
                    child: ElevatedButton(
                      onPressed: _submit,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.blueAccent,
                        foregroundColor: Colors.white,
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                        elevation: 4,
                      ),
                      child: const Text(
                        'Draw Light Rays',
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

  Widget _buildDropdown({
    required String label,
    required String? value,
    required List<String> items,
    required ValueChanged<String?> onChanged,
  }) {
    return DropdownButtonFormField<String>(
      value: value,
      dropdownColor: const Color(0xFF0F172A),
      style: const TextStyle(color: Colors.white, fontSize: 14),
      decoration: InputDecoration(
        labelText: label,
        labelStyle: const TextStyle(color: Colors.white70, fontSize: 13),
        filled: true,
        fillColor: const Color(0xFF0F172A),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
        ),
        contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
      ),
      items: items.map((item) {
        return DropdownMenuItem<String>(
          value: item,
          child: Text(item),
        );
      }).toList(),
      onChanged: onChanged,
    );
  }
}

class VictoryOverlay extends StatelessWidget {
  final String positionName;
  final String sizeName;
  final VoidCallback onReset;
  final VoidCallback onExit;

  const VictoryOverlay({
    super.key,
    required this.positionName,
    required this.sizeName,
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
                  Icons.auto_awesome,
                  color: Colors.greenAccent,
                  size: 56,
                ),
              ),
              const SizedBox(height: 16),
              const Text(
                'Ray Tracing Success!',
                style: TextStyle(
                  fontSize: 22,
                  fontWeight: FontWeight.bold,
                  color: Colors.greenAccent,
                ),
              ),
              const SizedBox(height: 14),
              Text(
                'By placing the Candle $positionName, the light rays refracted through the lens to form a Real, Inverted, and $sizeName image on the other side.\n\n'
                'Key Physics Concept:\n'
                '• Real images are formed by the actual intersection of light rays.\n\n'
                '• As the object moves closer to the focal point (F1) of a convex lens, the image is formed further away from the lens and increases in size!',
                textAlign: TextAlign.left,
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
                        side: const BorderSide(color: Colors.blueAccent, width: 2),
                        padding: const EdgeInsets.symmetric(vertical: 12),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                      ),
                      child: const Text(
                        'Try Another',
                        style: TextStyle(
                            color: Colors.blueAccent,
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

class GeometricalOpticsGame extends FlameGame with DragCallbacks {
  final void Function(String positionName) onShowTheory;
  final void Function(String positionName, String sizeName) onShowVictory;

  GeometricalOpticsGame({
    required this.onShowTheory,
    required this.onShowVictory,
  });

  late CandleComponent candle;
  late ImageCandleComponent imageCandle;

  // Axis and Lens dimensions
  double axisY = 0;
  double lensX = 0;
  double f = 0; // Focal distance

  // Object variables
  String snappedPositionName = '';
  double objectDistanceU = 0.0;
  double imageDistanceV = 0.0;
  double imageScale = 1.0;
  bool showRays = false;

  // Ray Animations
  double rayAnimationProgress = 0.0;
  bool isDrawingRays = false;

  @override
  Color backgroundColor() => const Color(0xFF0F172A);

  @override
  Future<void> onLoad() async {
    super.onLoad();

    _calculateDimensions(size);

    // Candle object
    candle = CandleComponent(
      startPosition: Vector2(size.x * 0.5, size.y * 0.82),
      onDropped: _handleCandleDropped,
    );
    add(candle);

    // Inverted image candle
    imageCandle = ImageCandleComponent();
    add(imageCandle);
  }

  void _calculateDimensions(Vector2 currentSize) {
    axisY = currentSize.y * 0.5;
    lensX = currentSize.x * 0.5;
    f = currentSize.x * 0.12; // Dynamic focal length
  }

  void _handleCandleDropped(Vector2 pos) {
    if (isDrawingRays) return;

    final double c1 = lensX - 2.8 * f; // Beyond 2F1
    final double c2 = lensX - 2.0 * f; // At 2F1
    final double c3 = lensX - 1.4 * f; // Between F1 and 2F1

    if (pos.x < lensX - 0.5 * f) {
      // Find closest snap zone
      final d1 = (pos.x - c1).abs();
      final d2 = (pos.x - c2).abs();
      final d3 = (pos.x - c3).abs();

      double snapX = c1;
      snappedPositionName = 'Beyond 2F1';
      objectDistanceU = 2.8 * f;

      if (d2 < d1 && d2 < d3) {
        snapX = c2;
        snappedPositionName = 'At 2F1';
        objectDistanceU = 2.0 * f;
      } else if (d3 < d1 && d3 < d2) {
        snapX = c3;
        snappedPositionName = 'Between F1 and 2F1';
        objectDistanceU = 1.4 * f;
      }

      // Snap to axis
      candle.position = Vector2(snapX, axisY - candle.size.y);
      candle.isLocked = true;

      // Pause and trigger overlay questions
      pauseEngine();
      onShowTheory(snappedPositionName);
    } else {
      candle.returnToStart();
    }
  }

  void handleTheoryCorrect(String type, String orientation, String sizeName) {
    resumeEngine();
    
    // Lens formula: 1/v - 1/u = 1/f -> v = u*f/(u - f)
    final u = objectDistanceU;
    imageDistanceV = (u * f) / (u - f);
    
    // Scale factor
    imageScale = imageDistanceV / u;

    isDrawingRays = true;
    rayAnimationProgress = 0.0;
    showRays = true;
  }

  @override
  void update(double dt) {
    super.update(dt);

    if (isDrawingRays) {
      rayAnimationProgress += dt * 0.8; // moderated speed
      if (rayAnimationProgress >= 1.0) {
        rayAnimationProgress = 1.0;
        isDrawingRays = false;

        // Position and reveal the inverted image candle at the intersection point
        final imgX = lensX + imageDistanceV;
        final imgH = candle.size.y * imageScale;
        
        imageCandle.position = Vector2(imgX, axisY);
        imageCandle.size = Vector2(candle.size.x * imageScale, imgH);
        imageCandle.reveal();

        // Trigger success dialog after image fully appears
        Future.delayed(const Duration(milliseconds: 1000), () {
          if (showRays) {
            String sizeName = 'Same Size';
            if (imageScale > 1.1) sizeName = 'Magnified';
            if (imageScale < 0.9) sizeName = 'Diminished';
            onShowVictory(snappedPositionName, sizeName);
          }
        });
      }
    }
  }

  @override
  void render(Canvas canvas) {
    super.render(canvas);

    // 1. Draw Principal Axis
    final axisPaint = Paint()
      ..color = const Color(0xFF64748B)
      ..strokeWidth = 3;
    canvas.drawLine(Offset(0, axisY), Offset(size.x, axisY), axisPaint);

    // Draw focus/center ticks and labels
    _drawTick(canvas, lensX - 2 * f, '2F1');
    _drawTick(canvas, lensX - f, 'F1');
    _drawTick(canvas, lensX + f, 'F2');
    _drawTick(canvas, lensX + 2 * f, '2F2');

    // 2. Draw Convex Lens
    final lensPaint = Paint()
      ..color = Colors.lightBlueAccent.withAlpha(70)
      ..style = PaintingStyle.fill;
    final lensOutline = Paint()
      ..color = Colors.lightBlueAccent
      ..strokeWidth = 2.5
      ..style = PaintingStyle.stroke;

    final lensRect = Rect.fromCenter(center: Offset(lensX, axisY), width: 20, height: 160);
    canvas.drawOval(lensRect, lensPaint);
    canvas.drawOval(lensRect, lensOutline);

    // 3. Draw Rays
    if (showRays) {
      final rayPaint = Paint()
        ..color = Colors.yellowAccent
        ..strokeWidth = 2.5
        ..style = PaintingStyle.stroke;

      // Candle Flame top point
      final start = Offset(candle.position.x, candle.position.y);
      
      // Points for Ray 1 (Parallel -> Lens -> Focus F2 -> Image Point)
      final r1Lens = Offset(lensX, start.dy);
      final imgPoint = Offset(lensX + imageDistanceV, axisY + (candle.size.y * imageScale));

      // Draw Ray 1
      final p1 = _interpolate(start, r1Lens, min(rayAnimationProgress * 2, 1.0));
      canvas.drawLine(start, p1, rayPaint);

      if (rayAnimationProgress > 0.5) {
        final p2 = _interpolate(r1Lens, imgPoint, min((rayAnimationProgress - 0.5) * 2, 1.0));
        canvas.drawLine(r1Lens, p2, rayPaint);
      }

      // Draw Ray 2 (Directly through optical center to Image Point)
      final p3 = _interpolate(start, imgPoint, rayAnimationProgress);
      canvas.drawLine(start, p3, rayPaint);
    }
  }

  Offset _interpolate(Offset a, Offset b, double t) {
    return Offset(a.dx + (b.dx - a.dx) * t, a.dy + (b.dy - a.dy) * t);
  }

  void _drawTick(Canvas canvas, double x, String label) {
    final tickPaint = Paint()
      ..color = Colors.white70
      ..strokeWidth = 2.5;
    canvas.drawLine(Offset(x, axisY - 6), Offset(x, axisY + 6), tickPaint);

    final tp = TextPainter(
      text: TextSpan(
        text: label,
        style: const TextStyle(
          color: Colors.white70,
          fontSize: 10,
          fontWeight: FontWeight.bold,
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    tp.paint(canvas, Offset(x - tp.width / 2, axisY + 12));
  }

  void resetGame() {
    candle.reset();
    imageCandle.reset();
    showRays = false;
    isDrawingRays = false;
    rayAnimationProgress = 0.0;
    resumeEngine();
  }

  @override
  void onGameResize(Vector2 newSize) {
    super.onGameResize(newSize);
    if (isLoaded) {
      _calculateDimensions(newSize);
      if (!isDrawingRays && !showRays) {
        candle.startPosition = Vector2(newSize.x * 0.5, newSize.y * 0.82);
        candle.position = candle.startPosition.clone();
      }
    }
  }
}

// ---------------------------------------------------------
// 3. Custom Flame Components
// ---------------------------------------------------------

class CandleComponent extends PositionComponent with DragCallbacks {
  Vector2 startPosition;
  final void Function(Vector2 finalPosition) onDropped;

  bool isDragging = false;
  bool isLocked = false;

  CandleComponent({
    required this.startPosition,
    required this.onDropped,
  }) : super(
          position: startPosition.clone(),
          size: Vector2(30, 60),
          anchor: Anchor.topCenter,
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
    onDropped(position);
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

  void reset() {
    returnToStart();
  }

  @override
  void render(Canvas canvas) {
    // 1. Draw Candle wax body (orange rectangle)
    final waxRect = Rect.fromLTWH(-size.x / 2, size.y * 0.3, size.x, size.y * 0.7);
    canvas.drawRRect(
      RRect.fromRectAndRadius(waxRect, const Radius.circular(3)),
      Paint()..color = const Color(0xFFF97316),
    );
    canvas.drawRRect(
      RRect.fromRectAndRadius(waxRect, const Radius.circular(3)),
      Paint()
        ..color = const Color(0xFFEA580C)
        ..strokeWidth = 1.5
        ..style = PaintingStyle.stroke,
    );

    // 2. Draw wick
    canvas.drawLine(
      Offset(0, size.y * 0.3),
      Offset(0, size.y * 0.2),
      Paint()
        ..color = Colors.black87
        ..strokeWidth = 2,
    );

    // 3. Draw Flame (Yellow triangle)
    final flamePath = Path()
      ..moveTo(0, size.y * 0.05)
      ..lineTo(-5, size.y * 0.2)
      ..lineTo(5, size.y * 0.2)
      ..close();
    canvas.drawPath(flamePath, Paint()..color = Colors.yellowAccent);
  }
}

class ImageCandleComponent extends PositionComponent {
  double opacity = 0.0;

  ImageCandleComponent()
      : super(
          size: Vector2(30, 60),
          anchor: Anchor.topCenter,
        );

  void reveal() {
    opacity = 1.0;
  }

  void reset() {
    opacity = 0.0;
  }

  @override
  void render(Canvas canvas) {
    if (opacity <= 0.0) return;

    // Draw upside down (Inverted image)
    canvas.save();
    canvas.scale(1, -1);

    // 1. Candle Wax body (semi-transparent)
    final waxRect = Rect.fromLTWH(-size.x / 2, size.y * 0.3, size.x, size.y * 0.7);
    canvas.drawRRect(
      RRect.fromRectAndRadius(waxRect, const Radius.circular(3)),
      Paint()..color = const Color(0xFFF97316).withAlpha(180),
    );
    canvas.drawRRect(
      RRect.fromRectAndRadius(waxRect, const Radius.circular(3)),
      Paint()
        ..color = const Color(0xFFEA580C).withAlpha(180)
        ..strokeWidth = 1.5
        ..style = PaintingStyle.stroke,
    );

    // 2. Wick
    canvas.drawLine(
      Offset(0, size.y * 0.3),
      Offset(0, size.y * 0.2),
      Paint()
        ..color = Colors.black54
        ..strokeWidth = 2,
    );

    // 3. Flame
    final flamePath = Path()
      ..moveTo(0, size.y * 0.05)
      ..lineTo(-5, size.y * 0.2)
      ..lineTo(5, size.y * 0.2)
      ..close();
    canvas.drawPath(flamePath, Paint()..color = Colors.yellowAccent.withAlpha(180));

    canvas.restore();
  }
}

// ---------------------------------------------------------
// 4. Flutter Screen Wrapper
// ---------------------------------------------------------

class GeometricalOpticsGameScreen extends StatefulWidget {
  const GeometricalOpticsGameScreen({super.key});

  @override
  State<GeometricalOpticsGameScreen> createState() => _GeometricalOpticsGameScreenState();
}

class _GeometricalOpticsGameScreenState extends State<GeometricalOpticsGameScreen> {
  late final GeometricalOpticsGame _game;

  bool _showTheory = false;
  String _activePositionName = '';

  bool _showVictory = false;
  String _victorySizeName = '';

  @override
  void initState() {
    super.initState();
    _game = GeometricalOpticsGame(
      onShowTheory: (posName) {
        setState(() {
          _activePositionName = posName;
          _showTheory = true;
        });
      },
      onShowVictory: (posName, sizeName) {
        setState(() {
          _victorySizeName = sizeName;
          _showVictory = true;
        });
      },
    );
  }

  void _onTheorySubmit(String type, String orientation, String sizeName) {
    setState(() {
      _showTheory = false;
    });
    _game.handleTheoryCorrect(type, orientation, sizeName);
  }

  void _onReset() {
    setState(() {
      _showVictory = false;
      _showTheory = false;
    });
    _game.resetGame();
  }

  void _exitToLessonDashboard() {
    _game.resetGame();
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(
        builder: (_) => const LessonsDashboard(
          lessonTitle: 'Geometrical Optics',
          grade: 'Grade 11 Physics',
        ),
      ),
      (route) => false,
    );
  }

  @override
  Widget build(BuildContext context) {
    // Dynamic metric labels
    double objectDistU = 0.0;
    double imageDistV = 0.0;
    if (_game.showRays) {
      objectDistU = _game.objectDistanceU;
      imageDistV = _game.imageDistanceV;
    }

    return Scaffold(
      backgroundColor: const Color(0xFF0F172A),
      resizeToAvoidBottomInset: false,
      appBar: AppBar(
        title: const Text(
          'Puzzle 4: Optics Lab',
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
            tooltip: 'Reset Lens Lab',
            onPressed: _onReset,
          ),
        ],
      ),
      body: SafeArea(
        child: Column(
          children: [
            // Top HUD Status bar
            Container(
              width: double.infinity,
              margin: const EdgeInsets.fromLTRB(10, 8, 10, 0),
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
              decoration: BoxDecoration(
                color: const Color(0xFF1E293B),
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: Colors.blueAccent.withAlpha(100), width: 1),
              ),
              child: const Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Row(
                    children: [
                      Icon(Icons.info_outline, color: Colors.blueAccent, size: 14),
                      SizedBox(width: 6),
                      Text(
                        'Drag Candle to snap position',
                        style: TextStyle(
                          color: Colors.white70,
                          fontWeight: FontWeight.w600,
                          fontSize: 12,
                        ),
                      ),
                    ],
                  ),
                  Text(
                    '1/f = 1/v - 1/u',
                    style: TextStyle(
                      color: Colors.blueAccent,
                      fontSize: 12,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
            ),

            // Live HUD Readouts
            if (_game.showRays)
              Container(
                margin: const EdgeInsets.fromLTRB(10, 6, 10, 0),
                padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                decoration: BoxDecoration(
                  color: const Color(0xFF1E293B),
                  borderRadius: BorderRadius.circular(10),
                  border: Border.all(color: Colors.yellowAccent.withAlpha(100), width: 1.5),
                ),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceAround,
                  children: [
                    _buildHUDStat('Focal Length (f)', '${_game.f.toStringAsFixed(0)} px'),
                    _buildHUDStat('Object Dist (u)', '${objectDistU.toStringAsFixed(0)} px'),
                    _buildHUDStat('Image Dist (v)', '${imageDistV.toStringAsFixed(0)} px'),
                    _buildHUDStat('Magnification (m)', '${_game.imageScale.toStringAsFixed(2)}x'),
                  ],
                ),
              ),

            // Game view frame
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
                    if (_showTheory)
                      Positioned.fill(
                        child: ColoredBox(
                          color: Colors.black87,
                          child: OpticsOverlay(
                            positionName: _activePositionName,
                            onSubmit: _onTheorySubmit,
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
                              positionName: _activePositionName,
                              sizeName: _victorySizeName,
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
          style: const TextStyle(color: Colors.yellowAccent, fontSize: 13, fontWeight: FontWeight.bold),
        ),
      ],
    );
  }
}
