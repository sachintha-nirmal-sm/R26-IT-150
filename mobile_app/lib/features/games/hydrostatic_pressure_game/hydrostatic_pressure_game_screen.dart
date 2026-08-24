import '../lesson_id_helper.dart';
import 'package:flame/components.dart';
import 'package:flame/events.dart';
import 'package:flame/game.dart';
import 'package:flutter/material.dart';
import 'dart:math';
import '../../lessons/Lessons_Dashboard.dart';


// ---------------------------------------------------------
// 1. Flutter Overlays
// ---------------------------------------------------------

class PressureOverlay extends StatefulWidget {
  final double depth;
  final double density;
  final double gravity;
  final void Function(bool isCorrect) onSubmit;

  const PressureOverlay({
    super.key,
    required this.depth,
    required this.density,
    required this.gravity,
    required this.onSubmit,
  });

  @override
  State<PressureOverlay> createState() => _PressureOverlayState();
}

class _PressureOverlayState extends State<PressureOverlay>
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
    _controller.dispose();
    _animationController.dispose();
    super.dispose();
  }

  void _submit() {
    final input = double.tryParse(_controller.text.replaceAll(',', ''));
    if (input == null) return;

    final correctPressure = widget.depth * widget.density * widget.gravity;

    if ((input - correctPressure).abs() < 0.1) {
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
    return SingleChildScrollView(
      padding: EdgeInsets.only(
        // Push content up when keyboard appears
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
          child: Center(
            child: Container(
              constraints: const BoxConstraints(maxWidth: 380),
              padding: const EdgeInsets.all(20),
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
                    blurRadius: 20,
                    spreadRadius: 5,
                  )
                ],
              ),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  // Compact Header Row (icon + title side by side to save vertical space)
                  Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.all(8),
                        decoration: BoxDecoration(
                          color: Colors.blueAccent.withAlpha(40),
                          shape: BoxShape.circle,
                        ),
                        child: const Icon(
                          Icons.calculate_outlined,
                          color: Colors.blueAccent,
                          size: 26,
                        ),
                      ),
                      const SizedBox(width: 12),
                      const Expanded(
                        child: Text(
                          'Calculation Checkpoint',
                          style: TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.bold,
                            color: Colors.white,
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 12),
                  // Info box with all given values condensed
                  Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: const Color(0xFF0F172A),
                      borderRadius: BorderRadius.circular(10),
                      border: Border.all(color: Colors.blueAccent.withAlpha(60)),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Sensor placed at depth h = ${widget.depth.toInt()}m',
                          style: const TextStyle(color: Colors.cyanAccent, fontSize: 12, fontWeight: FontWeight.bold),
                        ),
                        const SizedBox(height: 6),
                        Text(
                          '• ρ (density) = ${widget.density.toInt()} kg/m³\n'
                          '• g (gravity) = ${widget.gravity.toInt()} m/s²',
                          style: const TextStyle(color: Color(0xFFCBD5E1), fontSize: 12, height: 1.5),
                        ),
                        const SizedBox(height: 6),
                        const Text(
                          'Use: P = h × ρ × g  →  Find P in Pascals (Pa)',
                          style: TextStyle(
                            color: Colors.greenAccent,
                            fontSize: 12,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 14),
                  TextField(
                    controller: _controller,
                    keyboardType: TextInputType.number,
                    textAlign: TextAlign.center,
                    style: const TextStyle(
                        fontSize: 20, fontWeight: FontWeight.bold, color: Colors.white),
                    decoration: InputDecoration(
                      hintText: 'Enter pressure (Pa)',
                      hintStyle: const TextStyle(color: Colors.white38),
                      filled: true,
                      fillColor: const Color(0xFF0F172A),
                      contentPadding: const EdgeInsets.symmetric(vertical: 12, horizontal: 16),
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                        borderSide: const BorderSide(color: Colors.blueAccent),
                      ),
                      focusedBorder: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                        borderSide: const BorderSide(color: Colors.cyan, width: 2),
                      ),
                      errorText: _showError ? 'Incorrect. Hint: P = h × ρ × g' : null,
                      errorStyle: const TextStyle(
                          color: Colors.redAccent, fontWeight: FontWeight.bold, fontSize: 11),
                    ),
                    onSubmitted: (_) => _submit(),
                  ),
                  const SizedBox(height: 14),
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
                        'Submit',
                        style: TextStyle(fontSize: 15, fontWeight: FontWeight.bold),
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
  final VoidCallback onReset;
  final VoidCallback onExit;

  const VictoryOverlay({
    super.key,
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
            color: const Color(0xFF0F172A), // Premium dark theme
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
                  Icons.verified_user_rounded,
                  color: Colors.greenAccent,
                  size: 56,
                ),
              ),
              const SizedBox(height: 16),
              const Text(
                'Correct! 100,000 Pa!',
                style: TextStyle(
                  fontSize: 22,
                  fontWeight: FontWeight.bold,
                  color: Colors.greenAccent,
                ),
              ),
              const SizedBox(height: 12),
              const Text(
                'Excellent Work!\n\nThe hydrostatic pressure increases linearly with depth (P = hρg). Since the pressure is highest at the bottom, the dam wall experiences immense outward force there. \n\nTo safely withstand this high pressure, dams must be built with a much wider, thicker base at the bottom!',
                textAlign: TextAlign.center,
                style: TextStyle(
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
                        'Try Again',
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

class HydrostaticPressureGame extends FlameGame with DragCallbacks {
  final void Function(double depth) onShowCalculation;
  final void Function(String message) onShowShallowBanner;
  final void Function() onClearShallowBanner;

  HydrostaticPressureGame({
    required this.onShowCalculation,
    required this.onShowShallowBanner,
    required this.onClearShallowBanner,
  });

  late WaterReservoirComponent water;
  late DamWallComponent damWall;
  late DropZoneComponent dropZone1;
  late DropZoneComponent dropZone2;
  late PressureSensorComponent sensor;
  late GroundComponent ground;
  late SensorDockComponent sensorDock;

  double waterLevelY = 0;
  double groundLevelY = 0;
  double damStartX = 0;

  // Real-world factors
  final double density = 1000;
  final double gravity = 10;

  @override
  Color backgroundColor() => const Color(0xFF0F172A);

  @override
  Future<void> onLoad() async {
    super.onLoad();

    // Define coordinates based on canvas size
    _calculateDimensions(size);

    // 1. Sensor Dock Pedestal
    sensorDock = SensorDockComponent(
      position: Vector2(size.x * 0.78, size.y * 0.15),
    );
    add(sensorDock);

    // 2. Water Reservoir
    water = WaterReservoirComponent(
      position: Vector2(0, waterLevelY),
      size: Vector2(damStartX, groundLevelY - waterLevelY),
    );
    add(water);

    // 3. Ground
    ground = GroundComponent(
      position: Vector2(0, groundLevelY),
      size: Vector2(size.x, size.y - groundLevelY),
    );
    add(ground);

    // 4. Dam Wall
    damWall = DamWallComponent(
      position: Vector2(damStartX, waterLevelY - 40), // slightly higher than surface
      wallHeight: groundLevelY - (waterLevelY - 40),
    );
    add(damWall);

    // 5. Drop Zones
    // Zone 1: Shallow (h = 2m). 2m depth maps to 20% of the water depth
    final zone1Y = waterLevelY + (groundLevelY - waterLevelY) * 0.2;
    dropZone1 = DropZoneComponent(
      position: Vector2(damStartX, zone1Y),
      isDeep: false,
    );
    add(dropZone1);

    // Zone 2: Deep (h = 10m). Near the bottom
    final zone2Y = groundLevelY - 40;
    dropZone2 = DropZoneComponent(
      position: Vector2(damStartX, zone2Y),
      isDeep: true,
    );
    add(dropZone2);

    // 6. Pressure Sensor
    sensor = PressureSensorComponent(
      startPosition: Vector2(size.x * 0.78, size.y * 0.15),
      waterTopY: waterLevelY,
      waterBottomY: groundLevelY,
      waterRightX: damStartX,
      onDropped: _handleSensorDropped,
    );
    add(sensor);
  }

  void _calculateDimensions(Vector2 currentSize) {
    waterLevelY = currentSize.y * 0.25;
    groundLevelY = currentSize.y * 0.85;
    damStartX = currentSize.x * 0.42;
  }

  @override
  void onGameResize(Vector2 newSize) {
    super.onGameResize(newSize);

    // Dynamic resize positioning so components render properly on all screen aspect ratios
    if (isLoaded) {
      _calculateDimensions(newSize);

      sensorDock.position = Vector2(newSize.x * 0.78, newSize.y * 0.15);

      water.position = Vector2(0, waterLevelY);
      water.size = Vector2(damStartX, groundLevelY - waterLevelY);

      ground.position = Vector2(0, groundLevelY);
      ground.size = Vector2(newSize.x, newSize.y - groundLevelY);

      damWall.position = Vector2(damStartX, waterLevelY - 40);
      damWall.size = Vector2(damWall.size.x, groundLevelY - (waterLevelY - 40));

      final zone1Y = waterLevelY + (groundLevelY - waterLevelY) * 0.2;
      dropZone1.position = Vector2(damStartX, zone1Y);

      final zone2Y = groundLevelY - 40;
      dropZone2.position = Vector2(damStartX, zone2Y);

      if (!sensor.isDragging && !sensor.isLocked) {
        sensor.startPosition = Vector2(newSize.x * 0.78, newSize.y * 0.15);
        sensor.position = sensor.startPosition.clone();
      }

      sensor.waterTopY = waterLevelY;
      sensor.waterBottomY = groundLevelY;
      sensor.waterRightX = damStartX;
    }
  }

  void resetSensor() {
    sensor.isLocked = false;
    sensor.position = sensor.startPosition.clone();
  }

  void _handleSensorDropped(Vector2 pos) {
    // Check Drop Zone 1 (Shallow)
    if (pos.distanceTo(dropZone1.position) < 55) {
      sensor.position = dropZone1.position.clone();
      onShowShallowBanner("Low Hydrostatic Pressure here (Depth h = 2m).\nP = 2m × 1000kg/m³ × 10m/s² = 20,000 Pa. The dam wall can be thin here.");
      return;
    }

    // Check Drop Zone 2 (Deep)
    if (pos.distanceTo(dropZone2.position) < 55) {
      sensor.position = dropZone2.position.clone() - Vector2(sensor.size.x / 2, 0); // snap slightly to left of wall
      sensor.isLocked = true;
      pauseEngine();
      onShowCalculation(10.0); // 10m depth
      return;
    }

    // Otherwise, check if it's dropped in water generally or out of bounds.
    // If dropped elsewhere, return it to start
    sensor.returnToDock();
    onClearShallowBanner();
  }

  void handleCalculationCorrect() {
    resumeEngine();
    onClearShallowBanner();
    damWall.animateExpansion();
  }

  void resetGame() {
    onClearShallowBanner();
    damWall.reset();
    resetSensor();
    resumeEngine();
  }
}

// ---------------------------------------------------------
// 3. Custom Flame Components
// ---------------------------------------------------------

class SensorDockComponent extends PositionComponent {
  SensorDockComponent({required super.position})
      : super(size: Vector2(70, 50), anchor: Anchor.center);

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    
    // Draw a futuristic pedestal
    final paint = Paint()
      ..color = const Color(0xFF1E293B)
      ..style = PaintingStyle.fill;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(8)), paint);
    
    final borderPaint = Paint()
      ..color = Colors.cyanAccent.withAlpha(100)
      ..strokeWidth = 2
      ..style = PaintingStyle.stroke;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(8)), borderPaint);

    final textPainter = TextPainter(
      text: const TextSpan(
        text: 'SENSOR\nDOCK',
        style: TextStyle(
          color: Colors.cyanAccent,
          fontSize: 9,
          fontWeight: FontWeight.bold,
        ),
      ),
      textDirection: TextDirection.ltr,
      textAlign: TextAlign.center,
    )..layout();

    textPainter.paint(
      canvas,
      Offset((size.x - textPainter.width) / 2, (size.y - textPainter.height) / 2),
    );
  }
}

class WaterReservoirComponent extends PositionComponent {
  WaterReservoirComponent({required super.position, required super.size});

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    
    // Smooth blue gradient representing depth pressure
    final paint = Paint()
      ..shader = LinearGradient(
        begin: Alignment.topCenter,
        end: Alignment.bottomCenter,
        colors: [
          const Color(0xFF38BDF8).withAlpha(160), // Cyan/light-blue at top
          const Color(0xFF0284C7).withAlpha(220), // Medium blue
          const Color(0xFF0369A1).withAlpha(250), // Dark blue at bottom
        ],
      ).createShader(rect);

    canvas.drawRect(rect, paint);

    // Surface wave highlight line
    final surfacePaint = Paint()
      ..color = Colors.white.withAlpha(120)
      ..strokeWidth = 3
      ..style = PaintingStyle.stroke;
    canvas.drawLine(Offset.zero, Offset(size.x, 0), surfacePaint);
  }
}

class GroundComponent extends PositionComponent {
  GroundComponent({required super.position, required super.size});

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    final paint = Paint()..color = const Color(0xFF334155); // Dark slate ground
    canvas.drawRect(rect, paint);

    // Top soil line
    final linePaint = Paint()
      ..color = const Color(0xFF475569)
      ..strokeWidth = 4;
    canvas.drawLine(Offset.zero, Offset(size.x, 0), linePaint);
  }
}

class DamWallComponent extends PositionComponent {
  final double wallHeight;
  DamWallComponent({required super.position, required this.wallHeight})
      : super(size: Vector2(50, wallHeight)); // initial top width 50

  double currentBottomWidth = 50;
  final double targetBottomWidth = 120; // Widens towards the right side
  bool isExpanding = false;
  double expansionProgress = 0.0;

  void animateExpansion() {
    isExpanding = true;
    expansionProgress = 0.0;
  }

  void reset() {
    currentBottomWidth = 50;
    isExpanding = false;
    expansionProgress = 0.0;
  }

  @override
  void update(double dt) {
    super.update(dt);
    if (isExpanding && currentBottomWidth < targetBottomWidth) {
      expansionProgress += dt * 0.8; // Takes about 1.25s
      if (expansionProgress > 1.0) expansionProgress = 1.0;
      currentBottomWidth = 50 + (targetBottomWidth - 50) * expansionProgress;
    }
  }

  @override
  void render(Canvas canvas) {
    final path = Path()
      ..moveTo(0, 0) // top left
      ..lineTo(50, 0) // top right
      ..lineTo(currentBottomWidth, size.y) // bottom right (expands right)
      ..lineTo(0, size.y) // bottom left
      ..close();

    // Draw concrete block
    final wallPaint = Paint()
      ..color = const Color(0xFF94A3B8)
      ..style = PaintingStyle.fill;
    canvas.drawPath(path, wallPaint);

    // Draw concrete borders/shadows for 3D depth effect
    final borderPaint = Paint()
      ..color = const Color(0xFF64748B)
      ..strokeWidth = 3
      ..style = PaintingStyle.stroke;
    canvas.drawPath(path, borderPaint);

    // Subtle rebar joint lines to make it look like engineered concrete
    final linePaint = Paint()
      ..color = const Color(0xFF475569).withAlpha(100)
      ..strokeWidth = 1.5;
    
    // Draw horizontal sections
    for (int i = 1; i < 6; i++) {
      final ratio = i / 6.0;
      final y = size.y * ratio;
      final widthAtY = 50 + (currentBottomWidth - 50) * ratio;
      canvas.drawLine(Offset(0, y), Offset(widthAtY, y), linePaint);
    }
  }
}

class DropZoneComponent extends PositionComponent {
  final bool isDeep;

  DropZoneComponent({
    required super.position,
    required this.isDeep,
  }) : super(size: Vector2(75, 45), anchor: Anchor.center);

  @override
  void render(Canvas canvas) {
    // Draw dotted boundary
    final paint = Paint()
      ..color = isDeep ? Colors.redAccent.withAlpha(180) : Colors.greenAccent.withAlpha(180)
      ..strokeWidth = 1.5
      ..style = PaintingStyle.stroke;

    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    _drawDashedRect(canvas, rect, paint);

    // Draw Label text
    final textPainter = TextPainter(
      text: TextSpan(
        text: isDeep ? 'Zone 2 (Deep)\nh = 10m' : 'Zone 1 (Shallow)\nh = 2m',
        style: TextStyle(
          color: isDeep ? Colors.redAccent[100] : Colors.greenAccent[100],
          fontSize: 9,
          fontWeight: FontWeight.bold,
        ),
      ),
      textDirection: TextDirection.ltr,
      textAlign: TextAlign.center,
    )..layout();

    textPainter.paint(
      canvas,
      Offset((size.x - textPainter.width) / 2, (size.y - textPainter.height) / 2),
    );
  }

  void _drawDashedRect(Canvas canvas, Rect rect, Paint paint) {
    const dashWidth = 4.0;
    const dashSpace = 3.0;

    // Draw top
    double curX = rect.left;
    while (curX < rect.right) {
      canvas.drawLine(Offset(curX, rect.top), Offset(curX + dashWidth, rect.top), paint);
      curX += dashWidth + dashSpace;
    }
    // Draw bottom
    curX = rect.left;
    while (curX < rect.right) {
      canvas.drawLine(Offset(curX, rect.bottom), Offset(curX + dashWidth, rect.bottom), paint);
      curX += dashWidth + dashSpace;
    }
    // Draw left
    double curY = rect.top;
    while (curY < rect.bottom) {
      canvas.drawLine(Offset(rect.left, curY), Offset(rect.left, curY + dashWidth), paint);
      curY += dashWidth + dashSpace;
    }
    // Draw right
    curY = rect.top;
    while (curY < rect.bottom) {
      canvas.drawLine(Offset(rect.right, curY), Offset(rect.right, curY + dashWidth), paint);
      curY += dashWidth + dashSpace;
    }
  }
}

class PressureSensorComponent extends PositionComponent with DragCallbacks {
  Vector2 startPosition;
  double waterTopY;
  double waterBottomY;
  double waterRightX;
  final void Function(Vector2 finalPosition) onDropped;

  bool isDragging = false;
  bool isLocked = false;
  double displayedPressure = 0.0;
  double displayedDepth = 0.0;

  PressureSensorComponent({
    required this.startPosition,
    required this.waterTopY,
    required this.waterBottomY,
    required this.waterRightX,
    required this.onDropped,
  }) : super(
          position: startPosition.clone(),
          size: Vector2(40, 40),
          anchor: Anchor.center,
        );

  @override
  void update(double dt) {
    super.update(dt);

    if (isLocked) return;

    // Calculate depth and pressure based on sensor position inside the water bounds
    if (position.x <= waterRightX && position.y >= waterTopY && position.y <= waterBottomY) {
      final fraction = (position.y - waterTopY) / (waterBottomY - waterTopY);
      displayedDepth = fraction * 10.0; // scales to 10m depth max
      displayedPressure = displayedDepth * 1000 * 10; // P = h * rho * g
    } else {
      displayedDepth = 0.0;
      displayedPressure = 0.0;
    }
  }

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
    returnToDock();
  }

  void returnToDock() {
    isLocked = false;
    position = startPosition.clone();
  }

  @override
  void render(Canvas canvas) {
    final center = Offset(size.x / 2, size.y / 2);
    final radius = size.x / 2;

    // Glowing shadow if dragging
    if (isDragging) {
      canvas.drawCircle(
        center,
        radius + 5,
        Paint()
          ..color = Colors.cyanAccent.withAlpha(80)
          ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 5),
      );
    }

    // Outer metallic ring
    canvas.drawCircle(
      center,
      radius,
      Paint()
        ..color = const Color(0xFF475569)
        ..style = PaintingStyle.fill,
    );
    canvas.drawCircle(
      center,
      radius - 2,
      Paint()
        ..color = const Color(0xFF94A3B8)
        ..style = PaintingStyle.fill,
    );

    // Inner glowing sensor eye
    canvas.drawCircle(
      center,
      radius - 6,
      Paint()
        ..color = isLocked
            ? Colors.greenAccent
            : (displayedDepth > 0 ? Colors.cyanAccent : Colors.redAccent)
        ..style = PaintingStyle.fill,
    );

    // Crosshairs
    final crosshairPaint = Paint()
      ..color = Colors.black38
      ..strokeWidth = 1.2;
    canvas.drawLine(Offset(size.x / 2, 4), Offset(size.x / 2, size.y - 4), crosshairPaint);
    canvas.drawLine(Offset(4, size.y / 2), Offset(size.x - 4, size.y / 2), crosshairPaint);

    // Display pressure read-out badge above the sensor while dragging
    if (isDragging || displayedDepth > 0) {
      final valueText = '${displayedDepth.toStringAsFixed(1)}m\n${(displayedPressure / 1000).toStringAsFixed(0)} kPa';
      final textPainter = TextPainter(
        text: TextSpan(
          text: valueText,
          style: const TextStyle(
            color: Colors.white,
            fontSize: 9,
            fontWeight: FontWeight.bold,
            backgroundColor: Colors.black87,
          ),
        ),
        textDirection: TextDirection.ltr,
        textAlign: TextAlign.center,
      )..layout();

      textPainter.paint(
        canvas,
        Offset((size.x - textPainter.width) / 2, -26),
      );
    }
  }
}

// ---------------------------------------------------------
// 4. Flutter Screen
// ---------------------------------------------------------

class HydrostaticPressureGameScreen extends StatefulWidget {
  const HydrostaticPressureGameScreen({super.key});

  @override
  State<HydrostaticPressureGameScreen> createState() => _HydrostaticPressureGameScreenState();
}

class _HydrostaticPressureGameScreenState extends State<HydrostaticPressureGameScreen> {
  late final HydrostaticPressureGame _game;

  bool _showCalculation = false;
  double _activeDepth = 10.0;

  bool _showVictory = false;

  String? _shallowBannerText;

  @override
  void initState() {
    super.initState();
    _game = HydrostaticPressureGame(
      onShowCalculation: (depth) {
        setState(() {
          _activeDepth = depth;
          _showCalculation = true;
        });
      },
      onShowShallowBanner: (msg) {
        setState(() {
          _shallowBannerText = msg;
        });
      },
      onClearShallowBanner: () {
        setState(() {
          _shallowBannerText = null;
        });
      },
    );
  }

  void _onCalculationSubmit(bool isCorrect) {
    if (isCorrect) {
      setState(() {
        _showCalculation = false;
      });
      _game.handleCalculationCorrect();
      
      // Delay victory overlay slightly so student sees the dam wall animate widening
      Future.delayed(const Duration(milliseconds: 1500), () {
        if (mounted) {
          setState(() {
            _showVictory = true;
          });
        }
      });
    }
  }

  void _onReset() {
    setState(() {
      _showVictory = false;
      _showCalculation = false;
      _shallowBannerText = null;
    });
    _game.resetGame();
  }

  /// Exits the game and returns to the Lesson Dashboard for
  /// 'Hydrostatic pressure and its applications', clearing any intermediate
  /// routes so the lesson board is always the destination.
  void _exitToLessonDashboard() {
    _game.resetGame();
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(
        builder: (_) => LessonsDashboard(
          lessonId: LessonIdHelper.getLessonId('Hydrostatic pressure'),
          lessonTitle: 'Hydrostatic pressure and its applications',
          grade: 'Grade 10 Physics',
        ),
      ),
      (route) => false, // clear the entire navigation stack
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFF0F172A),
      // Prevent keyboard from resizing the game canvas
      resizeToAvoidBottomInset: false,
      appBar: AppBar(
        title: const Text(
          'Puzzle 4: Hydrostatic Dam',
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
            // Compact Top HUD
            Container(
              width: double.infinity,
              margin: const EdgeInsets.fromLTRB(10, 8, 10, 0),
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
              decoration: BoxDecoration(
                color: const Color(0xFF1E293B),
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: Colors.blueAccent.withAlpha(100), width: 1),
              ),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  const Row(
                    children: [
                      Icon(Icons.info_outline, color: Colors.cyanAccent, size: 14),
                      SizedBox(width: 6),
                      Text(
                        'Drag sensor → Drop in Zones',
                        style: TextStyle(
                          color: Colors.white70,
                          fontWeight: FontWeight.w600,
                          fontSize: 12,
                        ),
                      ),
                    ],
                  ),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                    decoration: BoxDecoration(
                      color: Colors.greenAccent.withAlpha(40),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: const Text(
                      'P = h·ρ·g',
                      style: TextStyle(
                        color: Colors.greenAccent,
                        fontSize: 12,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ],
              ),
            ),

            // Shallow Zone Banner
            if (_shallowBannerText != null)
              Container(
                margin: const EdgeInsets.fromLTRB(10, 6, 10, 0),
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                decoration: BoxDecoration(
                  color: Colors.green[900]!.withAlpha(220),
                  borderRadius: BorderRadius.circular(10),
                  border: Border.all(color: Colors.greenAccent, width: 1),
                ),
                child: Row(
                  children: [
                    const Icon(Icons.waves, color: Colors.greenAccent, size: 16),
                    const SizedBox(width: 8),
                    Expanded(
                      child: Text(
                        _shallowBannerText!,
                        style: const TextStyle(
                          color: Colors.white,
                          fontSize: 11,
                          height: 1.3,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ),
                    GestureDetector(
                      onTap: () => setState(() => _shallowBannerText = null),
                      child: const Icon(Icons.close, color: Colors.white54, size: 16),
                    ),
                  ],
                ),
              ),

            // Game Canvas — takes all remaining vertical space
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

                    // Calculation Overlay — scrollable so it's not clipped by keyboard
                    if (_showCalculation)
                      Positioned.fill(
                        child: ColoredBox(
                          color: Colors.black87,
                          child: PressureOverlay(
                            depth: _activeDepth,
                            density: _game.density,
                            gravity: _game.gravity,
                            onSubmit: _onCalculationSubmit,
                          ),
                        ),
                      ),

                    // Victory Overlay — scrollable
                    if (_showVictory)
                      Positioned.fill(
                        child: ColoredBox(
                          color: Colors.black87,
                          child: SingleChildScrollView(
                            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 20),
                            child: Center(
                              child: VictoryOverlay(
                                onReset: _onReset,
                                onExit: _exitToLessonDashboard,
                              ),
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
}
