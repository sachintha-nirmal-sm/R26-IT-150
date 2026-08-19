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
  final String materialName;
  final double mass;
  final double specificHeat;
  final double deltaTemp;
  final void Function(bool isCorrect) onSubmit;

  const CalculationOverlay({
    super.key,
    required this.materialName,
    required this.mass,
    required this.specificHeat,
    required this.deltaTemp,
    required this.onSubmit,
  });

  @override
  State<CalculationOverlay> createState() => _CalculationOverlayState();
}

class _CalculationOverlayState extends State<CalculationOverlay>
    with SingleTickerProviderStateMixin {
  final TextEditingController _energyController = TextEditingController();
  late AnimationController _animController;
  bool _showError = false;

  @override
  void initState() {
    super.initState();
    _animController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 500),
    )..addStatusListener((status) {
        if (status == AnimationStatus.completed) _animController.reset();
      });
  }

  @override
  void dispose() {
    _energyController.dispose();
    _animController.dispose();
    super.dispose();
  }

  void _submit() {
    final energyInput =
        double.tryParse(_energyController.text.replaceAll(',', ''));
    if (energyInput == null) return;

    final correct = widget.mass * widget.specificHeat * widget.deltaTemp;
    if ((energyInput - correct).abs() < 1.0) {
      widget.onSubmit(true);
    } else {
      setState(() => _showError = true);
      _animController.forward(from: 0.0);
    }
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _animController,
      builder: (ctx, child) {
        final shake =
            sin(_animController.value * pi * 6) * 14 * (1 - _animController.value);
        return Transform.translate(offset: Offset(shake, 0), child: child);
      },
      child: Center(
        child: SingleChildScrollView(
          padding: EdgeInsets.only(
            left: 20,
            right: 20,
            top: 20,
            bottom: MediaQuery.of(context).viewInsets.bottom + 20,
          ),
          child: Material(
            color: Colors.transparent,
            child: Container(
              constraints: const BoxConstraints(maxWidth: 400),
              padding: const EdgeInsets.all(22),
              decoration: BoxDecoration(
                color: const Color(0xFF1E293B),
                borderRadius: BorderRadius.circular(20),
                border: Border.all(
                    color: Colors.orangeAccent.withAlpha(180), width: 2),
                boxShadow: [
                  BoxShadow(
                      color: Colors.black.withAlpha(190),
                      blurRadius: 24,
                      spreadRadius: 6)
                ],
              ),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  // Header
                  Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.all(8),
                        decoration: BoxDecoration(
                          color: Colors.orangeAccent.withAlpha(28),
                          shape: BoxShape.circle,
                        ),
                        child: const Icon(Icons.thermostat,
                            color: Colors.orangeAccent, size: 26),
                      ),
                      const SizedBox(width: 10),
                      Expanded(
                        child: Text(
                          'Thermal Math Checkpoint',
                          style: TextStyle(
                            fontSize: 17,
                            fontWeight: FontWeight.bold,
                            color: Colors.orangeAccent[100],
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 14),

                  // Material info box
                  Container(
                    width: double.infinity,
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: const Color(0xFF0F172A),
                      borderRadius: BorderRadius.circular(10),
                      border:
                          Border.all(color: Colors.orangeAccent.withAlpha(50)),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text('Material Specs:',
                            style: TextStyle(
                                color: Colors.orangeAccent[100],
                                fontWeight: FontWeight.bold,
                                fontSize: 12)),
                        const SizedBox(height: 6),
                        Text(
                          '• Object: ${widget.materialName}\n'
                          '• Mass (m) = ${widget.mass.toInt()} kg\n'
                          '• Specific Heat (c) = ${widget.specificHeat.toInt()} J/kg°C\n'
                          '• Temp Rise (Δθ) = ${widget.deltaTemp.toInt()}°C',
                          style: const TextStyle(
                              color: Color(0xFFCBD5E1),
                              fontSize: 13,
                              height: 1.5),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 14),

                  // Question label
                  const Align(
                    alignment: Alignment.centerLeft,
                    child: Text(
                      'Calculate the Heat Energy required:',
                      style:
                          TextStyle(color: Colors.white70, fontSize: 13),
                    ),
                  ),
                  const SizedBox(height: 8),

                  // Input field
                  TextField(
                    controller: _energyController,
                    keyboardType: TextInputType.number,
                    style: const TextStyle(color: Colors.white, fontSize: 16),
                    decoration: InputDecoration(
                      labelText: 'Heat Energy Q (Joules)',
                      labelStyle: const TextStyle(color: Colors.white60),
                      hintText: 'Q = m × c × Δθ',
                      hintStyle: const TextStyle(color: Colors.white30),
                      filled: true,
                      fillColor: const Color(0xFF0F172A),
                      enabledBorder: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(10),
                        borderSide: BorderSide(
                            color: Colors.orangeAccent.withAlpha(80)),
                      ),
                      focusedBorder: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(10),
                        borderSide: const BorderSide(
                            color: Colors.orangeAccent, width: 2),
                      ),
                    ),
                  ),

                  // Error hint
                  if (_showError) ...[
                    const SizedBox(height: 10),
                    Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 12, vertical: 8),
                      decoration: BoxDecoration(
                        color: Colors.red.withAlpha(30),
                        borderRadius: BorderRadius.circular(8),
                        border: Border.all(color: Colors.redAccent),
                      ),
                      child: const Text(
                        'Hint: Q = Mass (m) × Specific Heat (c) × Temp Change (Δθ)',
                        style: TextStyle(
                            color: Colors.redAccent,
                            fontSize: 12,
                            fontWeight: FontWeight.bold),
                        textAlign: TextAlign.center,
                      ),
                    ),
                  ],
                  const SizedBox(height: 18),

                  // Submit button
                  SizedBox(
                    width: double.infinity,
                    height: 48,
                    child: ElevatedButton(
                      onPressed: _submit,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.orange[700],
                        foregroundColor: Colors.white,
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                        elevation: 4,
                      ),
                      child: const Text('Submit',
                          style: TextStyle(
                              fontSize: 16, fontWeight: FontWeight.bold)),
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
  final String materialName;
  final double energy;
  final VoidCallback onReset;
  final VoidCallback onExit;

  const VictoryOverlay({
    super.key,
    required this.materialName,
    required this.energy,
    required this.onReset,
    required this.onExit,
  });

  @override
  Widget build(BuildContext context) {
    return Center(
      child: SingleChildScrollView(
        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 20),
        child: Material(
          color: Colors.transparent,
          child: Container(
            constraints: const BoxConstraints(maxWidth: 400),
            padding: const EdgeInsets.all(24),
            decoration: BoxDecoration(
              color: const Color(0xFF0F172A),
              borderRadius: BorderRadius.circular(20),
              border: Border.all(color: Colors.greenAccent.withAlpha(180), width: 2),
              boxShadow: [
                BoxShadow(
                    color: Colors.black.withAlpha(200),
                    blurRadius: 30,
                    spreadRadius: 8)
              ],
            ),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Container(
                  padding: const EdgeInsets.all(14),
                  decoration: BoxDecoration(
                    color: Colors.greenAccent.withAlpha(28),
                    shape: BoxShape.circle,
                  ),
                  child: const Icon(Icons.local_fire_department,
                      color: Colors.greenAccent, size: 50),
                ),
                const SizedBox(height: 16),
                const Text('Mission Complete!',
                    style: TextStyle(
                        fontSize: 22,
                        fontWeight: FontWeight.bold,
                        color: Colors.greenAccent)),
                const SizedBox(height: 14),
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: const Color(0xFF1E293B),
                    borderRadius: BorderRadius.circular(10),
                  ),
                  child: Text(
                    'The $materialName reached the +10°C target!\n\n'
                    '• Water requires 42,000 J and takes much longer to heat due to its high Specific Heat Capacity (4200 J/kg°C).\n\n'
                    '• Copper only requires 4,000 J and heats up 10× faster due to its low capacity (400 J/kg°C).\n\n'
                    'This is why metals like copper are used in cookware — they heat up quickly!',
                    style: const TextStyle(
                        color: Color(0xFFE2E8F0), fontSize: 13, height: 1.55),
                  ),
                ),
                const SizedBox(height: 22),
                Row(
                  children: [
                    Expanded(
                      child: OutlinedButton(
                        onPressed: onReset,
                        style: OutlinedButton.styleFrom(
                          side: const BorderSide(
                              color: Colors.orangeAccent, width: 2),
                          padding: const EdgeInsets.symmetric(vertical: 14),
                          shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(12)),
                        ),
                        child: const Text('Try Another',
                            style: TextStyle(
                                color: Colors.orangeAccent,
                                fontSize: 14,
                                fontWeight: FontWeight.bold)),
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: ElevatedButton(
                        onPressed: onExit,
                        style: ElevatedButton.styleFrom(
                          backgroundColor: Colors.green[600],
                          foregroundColor: Colors.white,
                          padding: const EdgeInsets.symmetric(vertical: 14),
                          shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(12)),
                        ),
                        child: const Text('Exit Lab',
                            style: TextStyle(
                                fontSize: 14,
                                fontWeight: FontWeight.bold)),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

// ---------------------------------------------------------
// 2. Flame Game Engine
// ---------------------------------------------------------

class HeatGame extends FlameGame with DragCallbacks {
  final void Function(String name, double mass, double specificHeat, double deltaTemp)
      onShowCalculation;
  final void Function(String name, double energy) onShowVictory;
  final VoidCallback onHUDUpdate;

  HeatGame({
    required this.onShowCalculation,
    required this.onShowVictory,
    required this.onHUDUpdate,
  });

  late HeaterComponent heater;
  late ThermometerComponent thermometer;
  MaterialBlockComponent? waterBeaker;
  MaterialBlockComponent? copperBlock;

  final double deltaTemp = 10.0;

  MaterialBlockComponent? activeBlock;
  bool isHeating = false;
  double heatTimeElapsed = 0.0;
  double calculatedEnergy = 0.0;
  bool _victoryFired = false;
  double _hudUpdateTimer = 0.0;
  static const double _hudUpdateInterval = 0.1; // update HUD every 100ms

  @override
  Color backgroundColor() => const Color(0xFF0F172A);

  @override
  Future<void> onLoad() async {
    await super.onLoad();
    _rebuildComponents();
  }

  void _rebuildComponents() {
    // Remove existing components
    final toRemove = children.whereType<PositionComponent>().toList();
    for (final c in toRemove) {
      c.removeFromParent();
    }

    // Heater — placed at 40% x, 70% y (on tabletop)
    heater = HeaterComponent(
      position: Vector2(size.x * 0.40, size.y * 0.65),
    );
    add(heater);

    // Thermometer — right side
    thermometer = ThermometerComponent(
      position: Vector2(size.x * 0.82, size.y * 0.12),
    );
    add(thermometer);

    // Material blocks at bottom
    waterBeaker = MaterialBlockComponent(
      name: 'Water Beaker',
      mass: 1.0,
      specificHeat: 4200.0,
      blockColor: Colors.blueAccent,
      isBeaker: true,
      startPosition: Vector2(size.x * 0.25, size.y * 0.85),
      onDropped: _handleBlockDropped,
    );

    copperBlock = MaterialBlockComponent(
      name: 'Copper Block',
      mass: 1.0,
      specificHeat: 400.0,
      blockColor: Colors.orange[700]!,
      isBeaker: false,
      startPosition: Vector2(size.x * 0.60, size.y * 0.85),
      onDropped: _handleBlockDropped,
    );

    add(waterBeaker!);
    add(copperBlock!);
  }

  void _handleBlockDropped(MaterialBlockComponent block, Vector2 dropPos) {
    if (isHeating) {
      block.returnToStart();
      return;
    }

    // Distance check in world space
    final dist = dropPos.distanceTo(heater.position);
    if (dist < 90) {
      // Snap block ON TOP of heater
      block.position = Vector2(
        heater.position.x,
        heater.position.y - (heater.size.y / 2) - (block.size.y / 2) - 2,
      );
      block.isLocked = true;
      activeBlock = block;

      // Hide the other block
      final other =
          block == waterBeaker ? copperBlock : waterBeaker;
      if (other != null) {
        other.position = Vector2(-300, -300);
      }

      pauseEngine();
      onShowCalculation(
          block.name, block.mass, block.specificHeat, deltaTemp);
    } else {
      block.returnToStart();
    }
  }

  void handleCalculationCorrect() {
    resumeEngine();
    if (activeBlock == null) return;
    isHeating = true;
    _victoryFired = false;
    heatTimeElapsed = 0.0;
    calculatedEnergy = activeBlock!.mass * activeBlock!.specificHeat * deltaTemp;
    heater.activate();
  }

  @override
  void update(double dt) {
    super.update(dt);

    if (isHeating && activeBlock != null) {
      // Copper ≈ 1.5s, Water ≈ 15s (realistic proportional feel)
      final double targetDuration = calculatedEnergy >= 40000 ? 12.0 : 1.2;
      heatTimeElapsed += dt;
      final progress = (heatTimeElapsed / targetDuration).clamp(0.0, 1.0);

      thermometer.currentProgress = progress;
      // Throttle HUD setState — only rebuild Flutter widget every 100ms
      _hudUpdateTimer += dt;
      if (_hudUpdateTimer >= _hudUpdateInterval) {
        _hudUpdateTimer = 0.0;
        onHUDUpdate();
      }

      if (progress >= 1.0 && !_victoryFired) {
        _victoryFired = true;
        isHeating = false;
        heater.deactivate();
        Future.delayed(const Duration(milliseconds: 700), () {
          onShowVictory(activeBlock!.name, calculatedEnergy);
        });
      }
    }
  }

  @override
  void render(Canvas canvas) {
    // Draw tabletop
    final tablePaint = Paint()..color = const Color(0xFF334155);
    final tableTop = size.y * 0.73;
    canvas.drawRect(
        Rect.fromLTWH(0, tableTop, size.x, size.y - tableTop), tablePaint);

    // Table edge highlight line
    final edgePaint = Paint()
      ..color = const Color(0xFF475569)
      ..strokeWidth = 3;
    canvas.drawLine(
        Offset(0, tableTop), Offset(size.x, tableTop), edgePaint);

    // Lab background details: shelf on back wall
    final shelfPaint = Paint()..color = const Color(0xFF1E293B);
    canvas.drawRect(Rect.fromLTWH(0, size.y * 0.08, size.x * 0.62, 6), shelfPaint);

    // Draw thermometer label text
    final labelPainter = TextPainter(
      text: const TextSpan(
        text: 'TEMP',
        style: TextStyle(
            color: Color(0xFF94A3B8), fontSize: 11, fontWeight: FontWeight.bold),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    labelPainter.paint(
        canvas,
        Offset(size.x * 0.82 - labelPainter.width / 2,
            size.y * 0.12 + 185));

    super.render(canvas);
  }

  void resetGame() {
    isHeating = false;
    heatTimeElapsed = 0.0;
    calculatedEnergy = 0.0;
    _victoryFired = false;
    activeBlock = null;
    heater.deactivate();
    thermometer.reset();
    resumeEngine();
    _rebuildComponents();
  }

  @override
  void onGameResize(Vector2 size) {
    super.onGameResize(size);
    if (isLoaded && !isHeating) {
      heater.position = Vector2(size.x * 0.40, size.y * 0.65);
      thermometer.position = Vector2(size.x * 0.82, size.y * 0.12);
      waterBeaker?.startPosition = Vector2(size.x * 0.25, size.y * 0.85);
      copperBlock?.startPosition = Vector2(size.x * 0.60, size.y * 0.85);
      if (activeBlock == null) {
        waterBeaker?.position = waterBeaker!.startPosition.clone();
        copperBlock?.position = copperBlock!.startPosition.clone();
      }
    }
  }
}

// ---------------------------------------------------------
// 3. Custom Flame Components
// ---------------------------------------------------------

class HeaterComponent extends PositionComponent {
  bool isActive = false;
  double _pulse = 0.0;
  double _elapsed = 0.0;

  HeaterComponent({required super.position})
      : super(size: Vector2(80, 45), anchor: Anchor.center);

  void activate() => isActive = true;
  void deactivate() {
    isActive = false;
    _pulse = 0.0;
  }

  @override
  void update(double dt) {
    super.update(dt);
    if (isActive) {
      _elapsed += dt;
      _pulse = sin(_elapsed * 8.0);
    }
  }

  @override
  void render(Canvas canvas) {
    final w = size.x;
    final h = size.y;

    // Base plate — dark metallic
    final basePaint = Paint()..color = const Color(0xFF1E293B);
    final baseRect = Rect.fromLTWH(-w / 2, -h / 2, w, h);
    canvas.drawRRect(
        RRect.fromRectAndRadius(baseRect, const Radius.circular(8)), basePaint);

    // Border
    final borderPaint = Paint()
      ..color = const Color(0xFF475569)
      ..strokeWidth = 2
      ..style = PaintingStyle.stroke;
    canvas.drawRRect(
        RRect.fromRectAndRadius(baseRect, const Radius.circular(8)),
        borderPaint);

    // Heating coils — 3 horizontal lines
    final coilPaint = Paint()
      ..color = isActive ? Colors.deepOrangeAccent : const Color(0xFF64748B)
      ..strokeWidth = 3
      ..style = PaintingStyle.stroke;
    for (int i = 0; i < 3; i++) {
      final y = -h / 2 + 8 + i * (h - 16) / 2;
      canvas.drawLine(
          Offset(-w / 2 + 10, y), Offset(w / 2 - 10, y), coilPaint);
    }

    // Red LED indicator dot
    final ledPaint = Paint()
      ..color = isActive ? Colors.red : const Color(0xFF374151);
    canvas.drawCircle(Offset(w / 2 - 8, -h / 2 + 7), 4, ledPaint);

    // Heat glow when active — layered rects instead of MaskFilter.blur
    // (MaskFilter.blur causes GraphicBuffer allocation failures on many Android GPUs)
    if (isActive) {
      final int baseAlpha = ((_pulse * 0.5 + 0.5) * 60).round().clamp(20, 80);
      // Outer glow ring — largest, most transparent
      canvas.drawRRect(
          RRect.fromRectAndRadius(
              Rect.fromLTWH(-w / 2 - 8, -h / 2 - 8, w + 16, h + 16),
              const Radius.circular(14)),
          Paint()..color = Colors.orangeAccent.withAlpha(baseAlpha ~/ 3));
      // Middle glow ring
      canvas.drawRRect(
          RRect.fromRectAndRadius(
              Rect.fromLTWH(-w / 2 - 4, -h / 2 - 4, w + 8, h + 8),
              const Radius.circular(11)),
          Paint()..color = Colors.orangeAccent.withAlpha(baseAlpha ~/ 2));
      // Inner glow ring — smallest, most opaque
      canvas.drawRRect(
          RRect.fromRectAndRadius(
              Rect.fromLTWH(-w / 2 - 2, -h / 2 - 2, w + 4, h + 4),
              const Radius.circular(9)),
          Paint()..color = Colors.orangeAccent.withAlpha(baseAlpha));
    }

    // Label
    final lp = TextPainter(
      text: const TextSpan(
          text: 'HEATER',
          style: TextStyle(
              color: Colors.white54,
              fontSize: 9,
              fontWeight: FontWeight.bold)),
      textDirection: TextDirection.ltr,
    )..layout();
    lp.paint(canvas, Offset(-lp.width / 2, h / 2 - 14));
  }
}

class ThermometerComponent extends PositionComponent {
  double currentProgress = 0.0;

  ThermometerComponent({required super.position})
      : super(size: Vector2(30, 175), anchor: Anchor.topCenter);

  void reset() => currentProgress = 0.0;

  @override
  void render(Canvas canvas) {
    const double bulbRadius = 13.0;
    const double stemW = 9.0;
    final double tubeH = size.y - bulbRadius * 2 - 8;
    const double tubeTop = 4.0;
    final double tubeLeft = (size.x - stemW) / 2;

    // White tube background
    canvas.drawRRect(
      RRect.fromLTRBAndCorners(
        tubeLeft, tubeTop, tubeLeft + stemW, tubeTop + tubeH,
        topLeft: const Radius.circular(4),
        topRight: const Radius.circular(4),
      ),
      Paint()..color = Colors.white,
    );

    // Red mercury fill (grows from bottom of tube upward)
    final double fillH = (tubeH - 4) * currentProgress;
    if (fillH > 0) {
      canvas.drawRRect(
        RRect.fromLTRBAndCorners(
          tubeLeft + 1,
          tubeTop + tubeH - fillH,
          tubeLeft + stemW - 1,
          tubeTop + tubeH,
          bottomLeft: const Radius.circular(2),
          bottomRight: const Radius.circular(2),
        ),
        Paint()..color = Colors.redAccent,
      );
    }

    // Tube border
    canvas.drawRRect(
      RRect.fromLTRBAndCorners(
        tubeLeft, tubeTop, tubeLeft + stemW, tubeTop + tubeH,
        topLeft: const Radius.circular(4),
        topRight: const Radius.circular(4),
      ),
      Paint()
        ..color = const Color(0xFF94A3B8)
        ..strokeWidth = 1.5
        ..style = PaintingStyle.stroke,
    );

    // Bulb
    final bulbCenter = Offset(size.x / 2, tubeTop + tubeH + bulbRadius + 1);
    canvas.drawCircle(bulbCenter, bulbRadius, Paint()..color = Colors.redAccent);
    canvas.drawCircle(
        bulbCenter,
        bulbRadius,
        Paint()
          ..color = const Color(0xFF94A3B8)
          ..strokeWidth = 1.5
          ..style = PaintingStyle.stroke);

    // Graduation ticks (5 marks)
    final tickPaint = Paint()
      ..color = const Color(0xFF64748B)
      ..strokeWidth = 1;
    for (int i = 0; i <= 5; i++) {
      final ty = tubeTop + tubeH - (tubeH - 4) * (i / 5.0);
      canvas.drawLine(
          Offset(tubeLeft + stemW + 1, ty),
          Offset(tubeLeft + stemW + 7, ty),
          tickPaint);
    }

    // Percentage label at top
    final pct = (currentProgress * 100).round();
    final pctPainter = TextPainter(
      text: TextSpan(
        text: '$pct%',
        style: const TextStyle(
            color: Colors.white70, fontSize: 10, fontWeight: FontWeight.bold),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    pctPainter.paint(canvas, Offset((size.x - pctPainter.width) / 2, tubeTop - 14));
  }
}

class MaterialBlockComponent extends PositionComponent with DragCallbacks {
  final String name;
  final double mass;
  final double specificHeat;
  final Color blockColor;
  final bool isBeaker;
  Vector2 startPosition;
  final void Function(MaterialBlockComponent block, Vector2 worldPos) onDropped;

  bool isDragging = false;
  bool isLocked = false;

  MaterialBlockComponent({
    required this.name,
    required this.mass,
    required this.specificHeat,
    required this.blockColor,
    required this.isBeaker,
    required this.startPosition,
    required this.onDropped,
  }) : super(
          position: startPosition.clone(),
          size: isBeaker ? Vector2(44, 56) : Vector2(50, 50),
          anchor: Anchor.center,
        );

  @override
  bool containsLocalPoint(Vector2 point) {
    return point.x >= -size.x / 2 &&
        point.x <= size.x / 2 &&
        point.y >= -size.y / 2 &&
        point.y <= size.y / 2;
  }

  @override
  void onDragStart(DragStartEvent event) {
    super.onDragStart(event);
    if (isLocked) return;
    isDragging = true;
    priority = 100;
    event.handled = true;
  }

  @override
  void onDragUpdate(DragUpdateEvent event) {
    if (!isDragging || isLocked) return;
    // Use world delta directly
    position.x += event.localDelta.x;
    position.y += event.localDelta.y;
    event.handled = true;
  }

  @override
  void onDragEnd(DragEndEvent event) {
    super.onDragEnd(event);
    if (isLocked) return;
    isDragging = false;
    priority = 1;
    onDropped(this, position.clone());
    event.handled = true;
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
    final w = size.x;
    final h = size.y;

    if (isBeaker) {
      // Beaker glass body — transparent blue
      final glassFill = Paint()
        ..color = Colors.lightBlue.withAlpha(50)
        ..style = PaintingStyle.fill;
      canvas.drawRect(
          Rect.fromLTWH(-w / 2, -h / 2, w, h), glassFill);

      // Water fill inside beaker
      final waterPaint = Paint()
        ..color = blockColor.withAlpha(200)
        ..style = PaintingStyle.fill;
      canvas.drawRect(
          Rect.fromLTWH(-w / 2 + 3, h * 0.0, w - 6, h / 2 - 4), waterPaint);

      // Glass border — 3 sides (open top)
      final glassBorder = Paint()
        ..color = Colors.white70
        ..strokeWidth = 2.5
        ..style = PaintingStyle.stroke;
      final path = Path()
        ..moveTo(-w / 2, -h / 2)
        ..lineTo(-w / 2, h / 2)
        ..lineTo(w / 2, h / 2)
        ..lineTo(w / 2, -h / 2);
      canvas.drawPath(path, glassBorder);

      // Steam wisps when being heated (just visual dots)
    } else {
      // Copper block
      final blockPaint = Paint()..color = blockColor;
      canvas.drawRRect(
          RRect.fromRectAndRadius(
              Rect.fromLTWH(-w / 2, -h / 2, w, h), const Radius.circular(5)),
          blockPaint);

      // Metallic sheen
      final sheenPaint = Paint()
        ..color = Colors.white.withAlpha(40)
        ..style = PaintingStyle.fill;
      canvas.drawRRect(
          RRect.fromRectAndRadius(
              Rect.fromLTWH(-w / 2, -h / 2, w, h / 3), const Radius.circular(5)),
          sheenPaint);

      final border = Paint()
        ..color = Colors.black54
        ..strokeWidth = 2
        ..style = PaintingStyle.stroke;
      canvas.drawRRect(
          RRect.fromRectAndRadius(
              Rect.fromLTWH(-w / 2, -h / 2, w, h), const Radius.circular(5)),
          border);
    }

    // Label
    final label = isBeaker ? 'WATER\n1 kg\nc=4200' : 'COPPER\n1 kg\nc=400';
    final tp = TextPainter(
      text: TextSpan(
        text: label,
        style: const TextStyle(
            color: Colors.white,
            fontSize: 8,
            fontWeight: FontWeight.bold,
            shadows: [Shadow(color: Colors.black, blurRadius: 3)]),
      ),
      textDirection: TextDirection.ltr,
      textAlign: TextAlign.center,
    )..layout(maxWidth: w);
    tp.paint(canvas, Offset(-tp.width / 2, -tp.height / 2));
  }
}

// ---------------------------------------------------------
// 4. Flutter Screen Wrapper
// ---------------------------------------------------------

class HeatGameScreen extends StatefulWidget {
  const HeatGameScreen({super.key});

  @override
  State<HeatGameScreen> createState() => _HeatGameScreenState();
}

class _HeatGameScreenState extends State<HeatGameScreen> {
  late final HeatGame _game;

  bool _showCalculation = false;
  String _activeMaterialName = '';
  double _activeMass = 0;
  double _activeSpecificHeat = 0;
  double _activeDeltaTemp = 0;

  bool _showVictory = false;
  double _victoryEnergy = 0.0;

  @override
  void initState() {
    super.initState();
    _game = HeatGame(
      onShowCalculation: (name, mass, specificHeat, deltaTemp) {
        if (!mounted) return;
        setState(() {
          _activeMaterialName = name;
          _activeMass = mass;
          _activeSpecificHeat = specificHeat;
          _activeDeltaTemp = deltaTemp;
          _showCalculation = true;
          _showVictory = false;
        });
      },
      onShowVictory: (name, energy) {
        if (!mounted) return;
        setState(() {
          _victoryEnergy = energy;
          _showVictory = true;
        });
      },
      onHUDUpdate: () {
        if (mounted) setState(() {});
      },
    );
  }

  void _onCalculationSubmit(bool isCorrect) {
    if (!isCorrect) return;
    setState(() => _showCalculation = false);
    _game.handleCalculationCorrect();
  }

  void _onReset() {
    setState(() {
      _showVictory = false;
      _showCalculation = false;
      _activeMaterialName = '';
    });
    _game.resetGame();
  }

  void _exitToLessonDashboard() {
    _game.resetGame();
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(
        builder: (_) => const LessonsDashboard(
          lessonTitle: 'Heat',
          grade: 'Grade 11 Physics',
        ),
      ),
      (route) => false,
    );
  }

  @override
  Widget build(BuildContext context) {
    final isHeating = _game.isHeating;
    double liveEnergy = 0.0;
    double liveTempRise = 0.0;

    if (isHeating) {
      final targetDuration = _game.calculatedEnergy >= 40000 ? 12.0 : 1.2;
      final progress = (_game.heatTimeElapsed / targetDuration).clamp(0.0, 1.0);
      liveEnergy = progress * _game.calculatedEnergy;
      liveTempRise = progress * _game.deltaTemp;
    }

    return Scaffold(
      backgroundColor: const Color(0xFF0F172A),
      resizeToAvoidBottomInset: true,
      appBar: AppBar(
        title: const Text(
          'The Heat Lab — Q = mcΔθ',
          style: TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
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
            tooltip: 'Reset',
            onPressed: _onReset,
          ),
        ],
      ),
      body: SafeArea(
        child: Column(
          children: [
            // Instruction / formula bar
            Container(
              margin: const EdgeInsets.fromLTRB(10, 8, 10, 0),
              padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
              decoration: BoxDecoration(
                color: const Color(0xFF1E293B),
                borderRadius: BorderRadius.circular(10),
                border: Border.all(
                    color: Colors.orangeAccent.withAlpha(90), width: 1),
              ),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  const Row(
                    children: [
                      Icon(Icons.info_outline,
                          color: Colors.orangeAccent, size: 15),
                      SizedBox(width: 6),
                      Text(
                        'Drag a block onto the heater!',
                        style:
                            TextStyle(color: Colors.white70, fontSize: 12),
                      ),
                    ],
                  ),
                  Container(
                    padding: const EdgeInsets.symmetric(
                        horizontal: 8, vertical: 4),
                    decoration: BoxDecoration(
                      color: Colors.orangeAccent.withAlpha(22),
                      borderRadius: BorderRadius.circular(6),
                    ),
                    child: const Text(
                      'Q = m × c × Δθ',
                      style: TextStyle(
                          color: Colors.orangeAccent,
                          fontSize: 12,
                          fontWeight: FontWeight.bold),
                    ),
                  ),
                ],
              ),
            ),

            // Live HUD — only shown during heating
            if (isHeating)
              AnimatedContainer(
                duration: const Duration(milliseconds: 300),
                margin: const EdgeInsets.fromLTRB(10, 6, 10, 0),
                padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                decoration: BoxDecoration(
                  color: const Color(0xFF1E293B),
                  borderRadius: BorderRadius.circular(10),
                  border: Border.all(
                      color: Colors.yellowAccent.withAlpha(120), width: 1.5),
                ),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceAround,
                  children: [
                    _buildHUDStat(
                        'Material',
                        _activeMaterialName.contains('Water')
                            ? 'Water'
                            : 'Copper'),
                    _buildHUDStat(
                        'Temp Rise',
                        '+${liveTempRise.toStringAsFixed(1)}°C'),
                    _buildHUDStat(
                        'Heat Absorbed',
                        '${liveEnergy.toStringAsFixed(0)} J'),
                  ],
                ),
              ),

            // Game canvas
            Expanded(
              child: Container(
                margin: const EdgeInsets.fromLTRB(10, 8, 10, 10),
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(14),
                  border:
                      Border.all(color: const Color(0xFF334155), width: 2),
                ),
                clipBehavior: Clip.antiAlias,
                child: Stack(
                  fit: StackFit.expand,
                  children: [
                    GameWidget(game: _game),

                    // Calculation checkpoint overlay
                    if (_showCalculation)
                      GestureDetector(
                        onTap: () {}, // absorb taps
                        child: ColoredBox(
                          color: Colors.black.withAlpha(200),
                          child: CalculationOverlay(
                            materialName: _activeMaterialName,
                            mass: _activeMass,
                            specificHeat: _activeSpecificHeat,
                            deltaTemp: _activeDeltaTemp,
                            onSubmit: _onCalculationSubmit,
                          ),
                        ),
                      ),

                    // Victory overlay
                    if (_showVictory)
                      GestureDetector(
                        onTap: () {}, // absorb taps
                        child: ColoredBox(
                          color: Colors.black.withAlpha(210),
                          child: VictoryOverlay(
                            materialName: _activeMaterialName,
                            energy: _victoryEnergy,
                            onReset: _onReset,
                            onExit: _exitToLessonDashboard,
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
        Text(label.toUpperCase(),
            style: const TextStyle(
                color: Colors.white54,
                fontSize: 9,
                fontWeight: FontWeight.bold,
                letterSpacing: 0.5)),
        const SizedBox(height: 3),
        Text(value,
            style: const TextStyle(
                color: Colors.yellowAccent,
                fontSize: 13,
                fontWeight: FontWeight.bold)),
      ],
    );
  }
}
