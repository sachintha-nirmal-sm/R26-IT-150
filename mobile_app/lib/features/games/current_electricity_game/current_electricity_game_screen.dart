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
  final String resistorName;
  final double resistance;
  final double voltage;
  final void Function(bool isCorrect) onSubmit;

  const CalculationOverlay({
    super.key,
    required this.resistorName,
    required this.resistance,
    required this.voltage,
    required this.onSubmit,
  });

  @override
  State<CalculationOverlay> createState() => _CalculationOverlayState();
}

class _CalculationOverlayState extends State<CalculationOverlay>
    with SingleTickerProviderStateMixin {
  final TextEditingController _currentController = TextEditingController();
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
    _currentController.dispose();
    _powerController.dispose();
    _animationController.dispose();
    super.dispose();
  }

  void _submit() {
    final currentInput = double.tryParse(_currentController.text.replaceAll(',', ''));
    final powerInput = double.tryParse(_powerController.text.replaceAll(',', ''));

    if (currentInput == null || powerInput == null) return;

    final correctCurrent = widget.voltage / widget.resistance;
    final correctPower = widget.voltage * correctCurrent;

    final isCurrentCorrect = (currentInput - correctCurrent).abs() < 0.1;
    final isPowerCorrect = (powerInput - correctPower).abs() < 0.1;

    if (isCurrentCorrect && isPowerCorrect) {
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
                color: const Color(0xFF1E293B),
                borderRadius: BorderRadius.circular(20),
                border: Border.all(
                  color: Colors.cyanAccent.withAlpha(150),
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
                          color: Colors.cyanAccent.withAlpha(30),
                          shape: BoxShape.circle,
                        ),
                        child: const Icon(
                          Icons.electrical_services_outlined,
                          color: Colors.cyanAccent,
                          size: 28,
                        ),
                      ),
                      const SizedBox(width: 12),
                      const Expanded(
                        child: Text(
                          'Ohm\'s Law Checkpoint',
                          style: TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                            color: Colors.white,
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 14),
                  Container(
                    width: double.infinity,
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: const Color(0xFF0F172A),
                      borderRadius: BorderRadius.circular(10),
                      border: Border.all(color: Colors.cyanAccent.withAlpha(55)),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Circuit Configuration:',
                          style: TextStyle(
                            color: Colors.cyanAccent[100],
                            fontWeight: FontWeight.bold,
                            fontSize: 12,
                          ),
                        ),
                        const SizedBox(height: 6),
                        Text(
                          '• Voltage source (V) = ${widget.voltage.toInt()} V\n'
                          '• Connected Resistance (R) = ${widget.resistance.toInt()} Ω',
                          style: const TextStyle(color: Color(0xFFCBD5E1), fontSize: 13, height: 1.4),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 16),
                  TextField(
                    controller: _currentController,
                    keyboardType: TextInputType.number,
                    style: const TextStyle(color: Colors.white, fontSize: 16),
                    decoration: InputDecoration(
                      labelText: 'Current (I) in Amperes',
                      labelStyle: const TextStyle(color: Colors.white70),
                      hintText: 'I = V / R',
                      hintStyle: const TextStyle(color: Colors.white38),
                      filled: true,
                      fillColor: const Color(0xFF0F172A),
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                    ),
                  ),
                  const SizedBox(height: 14),
                  TextField(
                    controller: _powerController,
                    keyboardType: TextInputType.number,
                    style: const TextStyle(color: Colors.white, fontSize: 16),
                    decoration: InputDecoration(
                      labelText: 'Power Output (P) in Watts',
                      labelStyle: const TextStyle(color: Colors.white70),
                      hintText: 'P = V × I',
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
                      'Hint: Current (I) = Voltage (V) / Resistance (R)\nPower (P) = Voltage (V) * Current (I)',
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
                        backgroundColor: Colors.cyan[600],
                        foregroundColor: Colors.white,
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
  final String resistorName;
  final double current;
  final double power;
  final VoidCallback onReset;
  final VoidCallback onExit;

  const VictoryOverlay({
    super.key,
    required this.resistorName,
    required this.current,
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
                  Icons.electric_bolt,
                  color: Colors.greenAccent,
                  size: 56,
                ),
              ),
              const SizedBox(height: 16),
              const Text(
                'Circuit Activated!',
                style: TextStyle(
                  fontSize: 24,
                  fontWeight: FontWeight.bold,
                  color: Colors.greenAccent,
                ),
              ),
              const SizedBox(height: 14),
              Text(
                'You inserted the $resistorName (${resistorName.contains('Resistor A') ? '6' : '2'} Ω) into the loop.\n\n'
                'Key Physics Concept:\n'
                '• Ohm\'s Law (\$I = V/R\$) proves that lower resistance allows more current (\$I\$) to flow.\n\n'
                '• Power output (\$P = V \\times I\$) scales directly with current. Lowering the resistance to 2 Ω boosted current to ${current.toInt()} A and bulb power to ${power.toInt()} W, resulting in a much brighter light!',
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
                        side: const BorderSide(color: Colors.cyanAccent, width: 2),
                        padding: const EdgeInsets.symmetric(vertical: 12),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                      ),
                      child: const Text(
                        'Try Another',
                        style: TextStyle(
                            color: Colors.cyanAccent,
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

class CurrentElectricityGame extends FlameGame with DragCallbacks {
  final void Function(String name, double resistance, double voltage) onShowCalculation;
  final void Function(String name, double current, double power) onShowVictory;

  CurrentElectricityGame({
    required this.onShowCalculation,
    required this.onShowVictory,
  });

  late BatteryComponent battery;
  late LightBulbComponent bulb;
  late CircuitGapComponent gap;
  late ResistorComponent resistorA;
  late ResistorComponent resistorB;
  late SensorDockComponent resistorDock;

  // Perimeter Wire dimensions
  double wireLeft = 0;
  double wireRight = 0;
  double wireTop = 0;
  double wireBottom = 0;

  final double voltage = 12.0;

  ResistorComponent? activeResistor;
  bool isCircuitActive = false;
  double calculatedCurrent = 0.0;
  double calculatedPower = 0.0;

  // Electron Animation variables
  final List<double> electronPositions = [0.0, 0.15, 0.3, 0.45, 0.6, 0.75, 0.9];
  final List<Vector2> electronCoords = [];

  @override
  Color backgroundColor() => const Color(0xFF0F172A);

  @override
  Future<void> onLoad() async {
    super.onLoad();

    _calculateDimensions(size);

    // 1. Resistor Dock pedestal
    resistorDock = SensorDockComponent(
      position: Vector2(size.x * 0.81, size.y * 0.15),
    );
    add(resistorDock);

    // 2. Battery Component (Left wire center)
    battery = BatteryComponent(
      position: Vector2(wireLeft, (wireTop + wireBottom) / 2),
      voltage: voltage,
    );
    add(battery);

    // 3. Light Bulb Component (Top wire center)
    bulb = LightBulbComponent(
      position: Vector2((wireLeft + wireRight) / 2, wireTop),
    );
    add(bulb);

    // 4. Circuit Gap Component (Bottom wire center)
    gap = CircuitGapComponent(
      position: Vector2((wireLeft + wireRight) / 2, wireBottom),
    );
    add(gap);

    // 5. Load Resistors
    _loadResistors();
  }

  void _calculateDimensions(Vector2 currentSize) {
    wireLeft = currentSize.x * 0.16;
    wireRight = currentSize.x * 0.66;
    wireTop = currentSize.y * 0.28;
    wireBottom = currentSize.y * 0.82;
  }

  void _loadResistors() {
    if (activeResistor != null) {
      remove(activeResistor!);
      activeResistor = null;
    }
    isCircuitActive = false;
    calculatedCurrent = 0.0;
    calculatedPower = 0.0;
    bulb.reset();

    // Resistor A: Red, 6 Ohms
    resistorA = ResistorComponent(
      name: 'Resistor A',
      resistance: 6.0,
      color: Colors.redAccent,
      startPosition: Vector2(size.x * 0.75, size.y * 0.15),
      onDropped: _handleResistorDropped,
    );

    // Resistor B: Blue, 2 Ohms
    resistorB = ResistorComponent(
      name: 'Resistor B',
      resistance: 2.0,
      color: Colors.blueAccent,
      startPosition: Vector2(size.x * 0.88, size.y * 0.15),
      onDropped: _handleResistorDropped,
    );

    add(resistorA);
    add(resistorB);
  }

  void _handleResistorDropped(ResistorComponent resistor, Vector2 dropPos) {
    if (isCircuitActive) return;

    final distToGap = dropPos.distanceTo(gap.position);

    if (distToGap < 70) {
      // Snap directly into circuit gap
      resistor.position = gap.position.clone();
      resistor.isLocked = true;
      activeResistor = resistor;

      // Move the other resistor off-screen
      final other = resistor == resistorA ? resistorB : resistorA;
      other.position = Vector2(-200, -200);

      pauseEngine();
      onShowCalculation(resistor.name, resistor.resistance, voltage);
    } else {
      resistor.returnToStart();
    }
  }

  void handleCalculationCorrect() {
    resumeEngine();
    if (activeResistor == null) return;
    
    isCircuitActive = true;
    calculatedCurrent = voltage / activeResistor!.resistance;
    calculatedPower = voltage * calculatedCurrent;

    // Trigger bulb glow
    bulb.activate(calculatedPower);

    // Trigger overlay victory dialog after 3 seconds
    Future.delayed(const Duration(seconds: 3), () {
      if (isCircuitActive) {
        onShowVictory(activeResistor!.name, calculatedCurrent, calculatedPower);
      }
    });
  }

  @override
  void update(double dt) {
    super.update(dt);

    if (isCircuitActive && calculatedCurrent > 0) {
      // Loop electrons clockwise along circuit perimeter
      // Perimeter = 2 * (Width + Height)
      final w = wireRight - wireLeft;
      final h = wireBottom - wireTop;
      final perimeter = 2 * (w + h);

      // Speed of electrons based on Current (I)
      final step = calculatedCurrent * 90.0 * dt;

      for (int i = 0; i < electronPositions.length; i++) {
        // Increment visual progress offset
        electronPositions[i] = (electronPositions[i] + step / perimeter) % 1.0;
      }
    }
  }

  @override
  void render(Canvas canvas) {
    super.render(canvas);

    // Render Circuit Loop wires (excluding the circuit gap segment at the bottom)
    final wirePaint = Paint()
      ..color = const Color(0xFF475569)
      ..strokeWidth = 4
      ..style = PaintingStyle.stroke;

    final path = Path()
      ..moveTo(wireLeft, wireTop)
      ..lineTo(wireRight, wireTop)
      ..lineTo(wireRight, wireBottom)
      ..lineTo(gap.position.x + gap.size.x / 2, wireBottom) // right of gap
      ..moveTo(gap.position.x - gap.size.x / 2, wireBottom) // left of gap
      ..lineTo(wireLeft, wireBottom)
      ..lineTo(wireLeft, wireTop);
    
    canvas.drawPath(path, wirePaint);

    // Draw active circuit flowing electrons
    if (isCircuitActive) {
      final electronPaint = Paint()
        ..color = Colors.cyanAccent
        ..style = PaintingStyle.fill;

      final w = wireRight - wireLeft;
      final h = wireBottom - wireTop;
      final perimeter = 2 * (w + h);

      for (double pos in electronPositions) {
        final currentDistance = pos * perimeter;
        Vector2 coord;

        if (currentDistance < w) {
          // Top wire (flowing Left to Right)
          coord = Vector2(wireLeft + currentDistance, wireTop);
        } else if (currentDistance < w + h) {
          // Right wire (flowing Top to Bottom)
          coord = Vector2(wireRight, wireTop + (currentDistance - w));
        } else if (currentDistance < 2 * w + h) {
          // Bottom wire (flowing Right to Left)
          coord = Vector2(wireRight - (currentDistance - (w + h)), wireBottom);
        } else {
          // Left wire (flowing Bottom to Top)
          coord = Vector2(wireLeft, wireBottom - (currentDistance - (2 * w + h)));
        }

        canvas.drawCircle(Offset(coord.x, coord.y), 4.5, electronPaint);
      }
    }
  }

  void resetGame() {
    _loadResistors();
    resumeEngine();
  }

  @override
  void onGameResize(Vector2 newSize) {
    super.onGameResize(newSize);
    if (isLoaded) {
      _calculateDimensions(newSize);

      resistorDock.position = Vector2(newSize.x * 0.81, newSize.y * 0.15);
      battery.position = Vector2(wireLeft, (wireTop + wireBottom) / 2);
      bulb.position = Vector2((wireLeft + wireRight) / 2, wireTop);
      gap.position = Vector2((wireLeft + wireRight) / 2, wireBottom);

      if (!isCircuitActive) {
        if (activeResistor == null) {
          resistorA.startPosition = Vector2(newSize.x * 0.73, newSize.y * 0.15);
          resistorA.position = resistorA.startPosition.clone();

          resistorB.startPosition = Vector2(newSize.x * 0.89, newSize.y * 0.15);
          resistorB.position = resistorB.startPosition.clone();
        } else {
          activeResistor!.position = gap.position.clone();
        }
      } else if (activeResistor != null) {
        activeResistor!.position = gap.position.clone();
      }
    }
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
        text: 'RESISTOR\nDOCK',
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

class BatteryComponent extends PositionComponent {
  final double voltage;

  BatteryComponent({
    required super.position,
    required this.voltage,
  }) : super(size: Vector2(40, 70), anchor: Anchor.center);

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);

    // Green Battery body
    final fillPaint = Paint()..color = const Color(0xFF10B981);
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(6)), fillPaint);

    // Battery Cap
    final capRect = Rect.fromLTWH(size.x * 0.3, -4, size.x * 0.4, 4);
    canvas.drawRect(capRect, Paint()..color = const Color(0xFF047857));

    // Outline
    final borderPaint = Paint()
      ..color = const Color(0xFF047857)
      ..strokeWidth = 2.5
      ..style = PaintingStyle.stroke;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(6)), borderPaint);

    // Battery Labels (+ / -)
    _drawText(canvas, '+', size.x / 2, 14, fontSize: 16);
    _drawText(canvas, '12V', size.x / 2, size.y / 2, fontSize: 10);
    _drawText(canvas, '-', size.x / 2, size.y - 12, fontSize: 16);
  }

  void _drawText(Canvas canvas, String text, double x, double y, {double fontSize = 12}) {
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

class LightBulbComponent extends PositionComponent {
  bool isOn = false;
  double power = 0.0;
  double glowAnimationOffset = 0.0;

  LightBulbComponent({
    required super.position,
  }) : super(size: Vector2(60, 60), anchor: Anchor.center);

  void activate(double calculatedPower) {
    isOn = true;
    power = calculatedPower;
  }

  void reset() {
    isOn = false;
    power = 0.0;
  }

  @override
  void update(double dt) {
    super.update(dt);
    if (isOn) {
      // Continuous slight glow oscillation to look dynamic
      glowAnimationOffset = sin(DateTime.now().millisecondsSinceEpoch / 150.0) * 3.5;
    }
  }

  @override
  void render(Canvas canvas) {
    final center = Offset(size.x / 2, size.y / 2);
    final radius = size.x * 0.35;

    // Render large glowing halo if bulb is active (scales based on P = 24W or P = 72W)
    if (isOn) {
      final glowRadius = (power == 72.0 ? 55.0 : 25.0) + glowAnimationOffset;
      final glowPaint = Paint()
        ..color = Colors.yellowAccent.withAlpha(80)
        ..maskFilter = MaskFilter.blur(BlurStyle.normal, glowRadius / 2);
      canvas.drawCircle(center, radius + glowRadius, glowPaint);
    }

    // Metal Base (threaded plug)
    final basePaint = Paint()..color = const Color(0xFF64748B);
    canvas.drawRect(Rect.fromLTWH(size.x * 0.35, size.y * 0.72, size.x * 0.3, size.y * 0.16), basePaint);
    canvas.drawCircle(Offset(size.x / 2, size.y * 0.88), 6, Paint()..color = const Color(0xFF334155));

    // Glass Bulb body
    final bulbPaint = Paint()
      ..color = isOn ? Colors.yellowAccent : const Color(0xFF475569)
      ..style = PaintingStyle.fill;
    canvas.drawCircle(center, radius, bulbPaint);

    final borderPaint = Paint()
      ..color = isOn ? Colors.yellowAccent[700]! : const Color(0xFF1E293B)
      ..strokeWidth = 2.5
      ..style = PaintingStyle.stroke;
    canvas.drawCircle(center, radius, borderPaint);

    // Inner Filament
    final filamentPaint = Paint()
      ..color = isOn ? Colors.orangeAccent : const Color(0xFF334155)
      ..strokeWidth = 2
      ..style = PaintingStyle.stroke;

    final path = Path()
      ..moveTo(size.x * 0.4, size.y * 0.72)
      ..lineTo(size.x * 0.45, size.y * 0.48)
      ..lineTo(size.x * 0.5, size.y * 0.38)
      ..lineTo(size.x * 0.55, size.y * 0.48)
      ..lineTo(size.x * 0.6, size.y * 0.72);
    canvas.drawPath(path, filamentPaint);
  }
}

class CircuitGapComponent extends PositionComponent {
  CircuitGapComponent({
    required super.position,
  }) : super(size: Vector2(60, 30), anchor: Anchor.center);

  @override
  void render(Canvas canvas) {
    // Draw dashed empty slot indicator
    final paint = Paint()
      ..color = Colors.white30
      ..strokeWidth = 1.5
      ..style = PaintingStyle.stroke;

    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    _drawDashedRect(canvas, rect, paint);

    final tp = TextPainter(
      text: const TextSpan(
        text: 'GAP',
        style: TextStyle(
          color: Colors.white24,
          fontSize: 8,
          fontWeight: FontWeight.bold,
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    tp.paint(canvas, Offset((size.x - tp.width) / 2, (size.y - tp.height) / 2));
  }

  void _drawDashedRect(Canvas canvas, Rect rect, Paint paint) {
    const dashWidth = 4.0;
    const dashSpace = 3.0;

    double curX = rect.left;
    while (curX < rect.right) {
      canvas.drawLine(Offset(curX, rect.top), Offset(curX + dashWidth, rect.top), paint);
      canvas.drawLine(Offset(curX, rect.bottom), Offset(curX + dashWidth, rect.bottom), paint);
      curX += dashWidth + dashSpace;
    }
    double curY = rect.top;
    while (curY < rect.bottom) {
      canvas.drawLine(Offset(rect.left, curY), Offset(rect.left, curY + dashWidth), paint);
      canvas.drawLine(Offset(rect.right, curY), Offset(rect.right, curY + dashWidth), paint);
      curY += dashWidth + dashSpace;
    }
  }
}

class ResistorComponent extends PositionComponent with DragCallbacks {
  final String name;
  final double resistance;
  final Color color;
  Vector2 startPosition;
  final void Function(ResistorComponent resistor, Vector2 finalPosition) onDropped;

  bool isDragging = false;
  bool isLocked = false;

  ResistorComponent({
    required this.name,
    required this.resistance,
    required this.color,
    required this.startPosition,
    required this.onDropped,
  }) : super(
          position: startPosition.clone(),
          size: Vector2(60, 30),
          anchor: Anchor.center,
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
    onDropped(this, position);
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

    // Resistor body
    final fillPaint = Paint()..color = color;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(4)), fillPaint);

    final borderPaint = Paint()
      ..color = Colors.black87
      ..strokeWidth = 2
      ..style = PaintingStyle.stroke;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(4)), borderPaint);

    // Classic resistor color band rings
    final bandPaint = Paint()..color = Colors.white70;
    canvas.drawRect(Rect.fromLTWH(size.x * 0.2, 0, 4, size.y), bandPaint);
    canvas.drawRect(Rect.fromLTWH(size.x * 0.4, 0, 4, size.y), bandPaint);
    canvas.drawRect(Rect.fromLTWH(size.x * 0.6, 0, 4, size.y), bandPaint);

    // Resistor Label Text
    final tp = TextPainter(
      text: TextSpan(
        text: '${resistance.toInt()} Ω',
        style: const TextStyle(
          color: Colors.white,
          fontSize: 9,
          fontWeight: FontWeight.bold,
          backgroundColor: Colors.black45,
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    tp.paint(canvas, Offset((size.x - tp.width) / 2, (size.y - tp.height) / 2));
  }
}

// ---------------------------------------------------------
// 4. Flutter Screen Wrapper
// ---------------------------------------------------------

class CurrentElectricityGameScreen extends StatefulWidget {
  const CurrentElectricityGameScreen({super.key});

  @override
  State<CurrentElectricityGameScreen> createState() => _CurrentElectricityGameScreenState();
}

class _CurrentElectricityGameScreenState extends State<CurrentElectricityGameScreen> {
  late final CurrentElectricityGame _game;

  bool _showCalculation = false;
  String _activeResistorName = '';
  double _activeResistance = 0;
  double _activeVoltage = 0;

  bool _showVictory = false;
  double _victoryCurrent = 0.0;
  double _victoryPower = 0.0;

  @override
  void initState() {
    super.initState();
    _game = CurrentElectricityGame(
      onShowCalculation: (name, resistance, voltage) {
        setState(() {
          _activeResistorName = name;
          _activeResistance = resistance;
          _activeVoltage = voltage;
          _showCalculation = true;
        });
      },
      onShowVictory: (name, current, power) {
        setState(() {
          _victoryCurrent = current;
          _victoryPower = power;
          _showVictory = true;
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
    }
  }

  void _onReset() {
    setState(() {
      _showVictory = false;
      _showCalculation = false;
    });
    _game.resetGame();
  }

  void _exitToLessonDashboard() {
    _game.resetGame();
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(
        builder: (_) => const LessonsDashboard(
          lessonTitle: 'Current electricity',
          grade: 'Grade 10 Physics',
        ),
      ),
      (route) => false,
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFF0F172A),
      resizeToAvoidBottomInset: false,
      appBar: AppBar(
        title: const Text(
          'Puzzle 4: Circuit Challenge',
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
            tooltip: 'Reset Circuit',
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
                border: Border.all(color: Colors.cyanAccent.withAlpha(100), width: 1),
              ),
              child: const Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Row(
                    children: [
                      Icon(Icons.info_outline, color: Colors.cyanAccent, size: 14),
                      SizedBox(width: 6),
                      Text(
                        'Drag Resistor to completing gap',
                        style: TextStyle(
                          color: Colors.white70,
                          fontWeight: FontWeight.w600,
                          fontSize: 12,
                        ),
                      ),
                    ],
                  ),
                  Text(
                    'V = I·R  |  P = V·I',
                    style: TextStyle(
                      color: Colors.cyanAccent,
                      fontSize: 12,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
            ),

            // Live HUD Readouts
            if (_game.isCircuitActive)
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
                    _buildHUDStat('Voltage', '12 V'),
                    _buildHUDStat('Resistance', '${_activeResistance.toInt()} Ω'),
                    _buildHUDStat('Current Flow', '${_game.calculatedCurrent.toStringAsFixed(1)} A'),
                    _buildHUDStat('Power Output', '${_game.calculatedPower.toStringAsFixed(0)} W'),
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
                    if (_showCalculation)
                      Positioned.fill(
                        child: ColoredBox(
                          color: Colors.black87,
                          child: CalculationOverlay(
                            resistorName: _activeResistorName,
                            resistance: _activeResistance,
                            voltage: _activeVoltage,
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
                              resistorName: _activeResistorName,
                              current: _victoryCurrent,
                              power: _victoryPower,
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
          style: const TextStyle(color: Colors.yellowAccent, fontSize: 14, fontWeight: FontWeight.bold),
        ),
      ],
    );
  }
}
