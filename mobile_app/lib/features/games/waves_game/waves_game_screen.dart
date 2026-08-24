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

class CalculationOverlay extends StatefulWidget {
  final String generatorName;
  final double frequency;
  final double wavelength;
  final void Function(bool isCorrect) onSubmit;

  const CalculationOverlay({
    super.key,
    required this.generatorName,
    required this.frequency,
    required this.wavelength,
    required this.onSubmit,
  });

  @override
  State<CalculationOverlay> createState() => _CalculationOverlayState();
}

class _CalculationOverlayState extends State<CalculationOverlay>
    with SingleTickerProviderStateMixin {
  final TextEditingController _velocityController = TextEditingController();
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
    _velocityController.dispose();
    _animationController.dispose();
    super.dispose();
  }

  void _submit() {
    final velocityInput = double.tryParse(_velocityController.text.replaceAll(',', ''));

    if (velocityInput == null) return;

    final correctVelocity = widget.frequency * widget.wavelength;

    final isCorrect = (velocityInput - correctVelocity).abs() < 0.1;

    if (isCorrect) {
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
                          Icons.radar_outlined,
                          color: Colors.cyanAccent,
                          size: 28,
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Text(
                          '${widget.generatorName} Setup',
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
                          'Wave Characteristics:',
                          style: TextStyle(
                            color: Colors.cyanAccent[100],
                            fontWeight: FontWeight.bold,
                            fontSize: 12,
                          ),
                        ),
                        const SizedBox(height: 6),
                        Text(
                          '• Frequency (f) = ${widget.frequency.toInt()} Hz\n'
                          '• Wavelength (λ) = ${widget.wavelength.toInt()} m',
                          style: const TextStyle(color: Color(0xFFCBD5E1), fontSize: 13, height: 1.4),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 16),
                  TextField(
                    controller: _velocityController,
                    keyboardType: TextInputType.number,
                    style: const TextStyle(color: Colors.white, fontSize: 16),
                    decoration: InputDecoration(
                      labelText: 'Wave Velocity (v) in m/s',
                      labelStyle: const TextStyle(color: Colors.white70),
                      hintText: 'v = f × λ',
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
                      'Hint: Velocity (v) = Frequency (f) * Wavelength (λ)',
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
                        'Transmit Signal',
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
  final String generatorName;
  final double velocity;
  final VoidCallback onReset;
  final VoidCallback onExit;

  const VictoryOverlay({
    super.key,
    required this.generatorName,
    required this.velocity,
    required this.onReset,
    required this.onExit,
  });

  @override
  Widget build(BuildContext context) {
    final bool isTransverse = generatorName.contains('Water');
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
                  Icons.sensors_rounded,
                  color: Colors.greenAccent,
                  size: 56,
                ),
              ),
              const SizedBox(height: 16),
              const Text(
                'Transmission Successful!',
                style: TextStyle(
                  fontSize: 22,
                  fontWeight: FontWeight.bold,
                  color: Colors.greenAccent,
                ),
              ),
              const SizedBox(height: 14),
              Text(
                'The $generatorName signal successfully reached the Receiver Station!\n\n'
                'Key Physics Concepts:\n'
                '• Wave Equation: The velocity (\$v = f \\cdot \\lambda\$) was calculated correctly as ${velocity.toInt()} m/s.\n\n'
                '• Wave Types:\n'
                '  - ${isTransverse ? "Transverse waves (like water waves) oscillate particles perpendicular to the direction of propagation (forming crests and troughs)." : "Longitudinal waves (like sound waves) oscillate particles parallel to the direction of propagation (forming compressions and rarefactions)."}\n\n'
                '  - Notice how the Sound Wave traveled at 340 m/s, which is much faster than the Water Wave (20 m/s)!',
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

class WavesGame extends FlameGame with DragCallbacks {
  final void Function(String name, double frequency, double wavelength) onShowCalculation;
  final void Function(String name, double velocity) onShowVictory;

  WavesGame({
    required this.onShowCalculation,
    required this.onShowVictory,
  });

  late TransmitterTowerComponent transmitter;
  late ReceiverStationComponent receiver;
  late WaveGeneratorComponent waterGenerator;
  late WaveGeneratorComponent soundGenerator;
  late SensorDockComponent dock;

  double transmitterX = 0;
  double receiverX = 0;
  double baseLineY = 0;

  WaveGeneratorComponent? activeGenerator;
  bool isTransmitting = false;
  double waveProgressX = 0.0;
  double calculatedVelocity = 0.0;
  double waveTime = 0.0;

  @override
  Color backgroundColor() => const Color(0xFF070B19); // Dark blue night sky

  @override
  Future<void> onLoad() async {
    super.onLoad();

    _calculateDimensions(size);

    // 1. Bottom Docking Pedestal
    dock = SensorDockComponent(
      position: Vector2(size.x * 0.5, size.y * 0.88),
    );
    add(dock);

    // 2. Transmitter Tower (Left side)
    transmitter = TransmitterTowerComponent(
      position: Vector2(transmitterX, baseLineY),
    );
    add(transmitter);

    // 3. Receiver Station (Right side)
    receiver = ReceiverStationComponent(
      position: Vector2(receiverX, baseLineY),
    );
    add(receiver);

    // 4. Wave Generators
    _loadGenerators();
  }

  void _calculateDimensions(Vector2 currentSize) {
    transmitterX = currentSize.x * 0.15;
    receiverX = currentSize.x * 0.85;
    baseLineY = currentSize.y * 0.65;
  }

  void _loadGenerators() {
    if (activeGenerator != null) {
      remove(activeGenerator!);
      activeGenerator = null;
    }
    isTransmitting = false;
    waveProgressX = transmitterX;
    calculatedVelocity = 0.0;
    waveTime = 0.0;
    receiver.deactivate();

    // Water Wave: Cyan, 5 Hz, Wavelength = 4m (Transverse)
    waterGenerator = WaveGeneratorComponent(
      name: 'Water Wave Generator',
      frequency: 5.0,
      wavelength: 4.0,
      color: Colors.cyan,
      isTransverse: true,
      startPosition: Vector2(size.x * 0.38, size.y * 0.88),
      onDropped: _handleGeneratorDropped,
    );

    // Sound Wave: Orange, 170 Hz, Wavelength = 2m (Longitudinal)
    soundGenerator = WaveGeneratorComponent(
      name: 'Sound Wave Generator',
      frequency: 170.0,
      wavelength: 2.0,
      color: Colors.orangeAccent,
      isTransverse: false,
      startPosition: Vector2(size.x * 0.62, size.y * 0.88),
      onDropped: _handleGeneratorDropped,
    );

    add(waterGenerator);
    add(soundGenerator);
  }

  void _handleGeneratorDropped(WaveGeneratorComponent generator, Vector2 dropPos) {
    if (isTransmitting) return;

    // Check collision with the Transmitter Tower
    final distToTower = dropPos.distanceTo(transmitter.position - Vector2(0, transmitter.size.y / 2));

    if (distToTower < 80) {
      // Snap to the top of the tower
      generator.position = Vector2(
        transmitter.position.x,
        transmitter.position.y - transmitter.size.y + generator.size.y / 2,
      );
      generator.isLocked = true;
      activeGenerator = generator;

      // Hide the other generator
      final other = generator == waterGenerator ? soundGenerator : waterGenerator;
      other.position = Vector2(-200, -200);

      pauseEngine();
      onShowCalculation(generator.name, generator.frequency, generator.wavelength);
    } else {
      generator.returnToStart();
    }
  }

  void handleCalculationCorrect() {
    resumeEngine();
    if (activeGenerator == null) return;
    
    isTransmitting = true;
    calculatedVelocity = activeGenerator!.frequency * activeGenerator!.wavelength;
    waveProgressX = transmitterX;
    waveTime = 0.0;
  }

  @override
  void update(double dt) {
    super.update(dt);

    if (isTransmitting && activeGenerator != null) {
      waveTime += dt;
      // Normalizing visual speeds so:
      // Water Wave (20 m/s) -> takes ~4 seconds to travel across the screen
      // Sound Wave (340 m/s) -> takes ~1 second to travel across the screen
      final travelSpeed = calculatedVelocity == 340.0
          ? (receiverX - transmitterX) * 1.0 * dt
          : (receiverX - transmitterX) * 0.25 * dt;

      waveProgressX += travelSpeed;

      if (waveProgressX >= receiverX) {
        waveProgressX = receiverX;
        isTransmitting = false;
        receiver.activate();

        // Trigger success dialog
        Future.delayed(const Duration(milliseconds: 800), () {
          if (!isTransmitting) {
            onShowVictory(activeGenerator!.name, calculatedVelocity);
          }
        });
      }
    }
  }

  @override
  void render(Canvas canvas) {
    super.render(canvas);

    // Draw connecting visual wire/path between Transmitter and Receiver
    final pathPaint = Paint()
      ..color = Colors.white.withAlpha(30)
      ..strokeWidth = 2
      ..style = PaintingStyle.stroke;
    canvas.drawLine(Offset(transmitterX, baseLineY), Offset(receiverX, baseLineY), pathPaint);

    if (isTransmitting && activeGenerator != null) {
      final bool transverse = activeGenerator!.isTransverse;
      final double width = waveProgressX - transmitterX;

      if (transverse) {
        // Render Transverse Sine Wave
        final wavePaint = Paint()
          ..color = Colors.cyan.withAlpha(180)
          ..strokeWidth = 3
          ..style = PaintingStyle.stroke;

        final path = Path()..moveTo(transmitterX, baseLineY);
        for (double x = transmitterX; x <= waveProgressX; x += 2) {
          const double waveAmplitude = 30.0;
          const double waveFrequency = 0.08;
          final double y = baseLineY + waveAmplitude * sin((x - transmitterX) * waveFrequency - waveTime * 15.0);
          path.lineTo(x, y);
        }
        canvas.drawPath(path, wavePaint);
      } else {
        // Render Longitudinal Compression Wave
        final linePaint = Paint()
          ..color = Colors.orangeAccent.withAlpha(180)
          ..strokeWidth = 2;

        const double segmentSpacing = 16.0;
        final int segments = (width / segmentSpacing).ceil();

        for (int i = 0; i <= segments; i++) {
          final double equilibriumX = transmitterX + i * segmentSpacing;
          if (equilibriumX > waveProgressX) continue;

          // Push particles horizontally based on a cosine wave (compressions & rarefactions)
          final double displacement = 10.0 * cos((equilibriumX - transmitterX) * 0.08 - waveTime * 18.0);
          final double x = equilibriumX + displacement;

          canvas.drawLine(
            Offset(x, baseLineY - 25),
            Offset(x, baseLineY + 25),
            linePaint,
          );
        }
      }
    }
  }

  void resetGame() {
    _loadGenerators();
    resumeEngine();
  }

  @override
  void onGameResize(Vector2 newSize) {
    super.onGameResize(newSize);
    if (isLoaded) {
      _calculateDimensions(newSize);

      dock.position = Vector2(newSize.x * 0.5, newSize.y * 0.88);
      transmitter.position = Vector2(transmitterX, baseLineY);
      receiver.position = Vector2(receiverX, baseLineY);

      if (!isTransmitting) {
        if (activeGenerator == null) {
          waterGenerator.startPosition = Vector2(newSize.x * 0.38, newSize.y * 0.88);
          waterGenerator.position = waterGenerator.startPosition.clone();

          soundGenerator.startPosition = Vector2(newSize.x * 0.62, newSize.y * 0.88);
          soundGenerator.position = soundGenerator.startPosition.clone();
        } else {
          activeGenerator!.position = Vector2(
            transmitter.position.x,
            transmitter.position.y - transmitter.size.y + activeGenerator!.size.y / 2,
          );
        }
      }
    }
  }
}

// ---------------------------------------------------------
// 3. Custom Flame Components
// ---------------------------------------------------------

class SensorDockComponent extends PositionComponent {
  SensorDockComponent({required super.position})
      : super(size: Vector2(250, 60), anchor: Anchor.center);

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
  }
}

class TransmitterTowerComponent extends PositionComponent {
  TransmitterTowerComponent({
    required super.position,
  }) : super(size: Vector2(30, 100), anchor: Anchor.bottomCenter);

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(-size.x / 2, -size.y, size.x, size.y);

    final fillPaint = Paint()..color = const Color(0xFF475569);
    canvas.drawRect(rect, fillPaint);

    final borderPaint = Paint()
      ..color = const Color(0xFF64748B)
      ..strokeWidth = 2
      ..style = PaintingStyle.stroke;
    canvas.drawRect(rect, borderPaint);

    // Cross brace grid designs
    canvas.drawLine(Offset(-size.x / 2, -size.y), Offset(size.x / 2, -size.y * 0.66), borderPaint);
    canvas.drawLine(Offset(size.x / 2, -size.y), Offset(-size.x / 2, -size.y * 0.66), borderPaint);
    canvas.drawLine(Offset(-size.x / 2, -size.y * 0.66), Offset(size.x / 2, -size.y * 0.33), borderPaint);
    canvas.drawLine(Offset(size.x / 2, -size.y * 0.66), Offset(-size.x / 2, -size.y * 0.33), borderPaint);
    canvas.drawLine(Offset(-size.x / 2, -size.y * 0.33), Offset(size.x / 2, 0), borderPaint);
    canvas.drawLine(Offset(size.x / 2, -size.y * 0.33), Offset(-size.x / 2, 0), borderPaint);

    // Blink beacon on top
    final double blinkTime = DateTime.now().millisecondsSinceEpoch / 500.0;
    final blinkPaint = Paint()
      ..color = blinkTime.floor() % 2 == 0 ? Colors.red : Colors.red.withAlpha(50)
      ..style = PaintingStyle.fill;
    canvas.drawCircle(Offset(0, -size.y - 5), 5, blinkPaint);
  }
}

class ReceiverStationComponent extends PositionComponent {
  bool isActive = false;

  ReceiverStationComponent({
    required super.position,
  }) : super(size: Vector2(40, 50), anchor: Anchor.bottomCenter);

  void activate() => isActive = true;
  void deactivate() => isActive = false;

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(-size.x / 2, -size.y, size.x, size.y);

    // Renders Receiver Station building base
    final fillPaint = Paint()..color = const Color(0xFF334155);
    canvas.drawRect(rect, fillPaint);

    final borderPaint = Paint()
      ..color = const Color(0xFF475569)
      ..strokeWidth = 2
      ..style = PaintingStyle.stroke;
    canvas.drawRect(rect, borderPaint);

    // Renders Radar Dish Dome structure on top
    final domePaint = Paint()
      ..color = isActive ? Colors.cyanAccent : const Color(0xFF64748B)
      ..style = PaintingStyle.fill;
    canvas.drawArc(
      Rect.fromLTWH(-15, -size.y - 12, 30, 24),
      pi,
      pi,
      true,
      domePaint,
    );

    // Renders blinking green/cyan beacon when signal hits
    if (isActive) {
      final double blinkTime = DateTime.now().millisecondsSinceEpoch / 300.0;
      final signalPaint = Paint()
        ..color = blinkTime.floor() % 2 == 0 ? Colors.cyanAccent : Colors.cyanAccent.withAlpha(50)
        ..style = PaintingStyle.fill;
      canvas.drawCircle(Offset(0, -size.y - 15), 4, signalPaint);
    }
  }
}

class WaveGeneratorComponent extends PositionComponent with DragCallbacks {
  final String name;
  final double frequency;
  final double wavelength;
  final Color color;
  final bool isTransverse;
  Vector2 startPosition;
  final void Function(WaveGeneratorComponent generator, Vector2 finalPosition) onDropped;

  bool isDragging = false;
  bool isLocked = false;

  WaveGeneratorComponent({
    required this.name,
    required this.frequency,
    required this.wavelength,
    required this.color,
    required this.isTransverse,
    required this.startPosition,
    required this.onDropped,
  }) : super(
          position: startPosition.clone(),
          size: Vector2(45, 45),
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

    final fillPaint = Paint()..color = color;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(8)), fillPaint);

    final borderPaint = Paint()
      ..color = Colors.black87
      ..strokeWidth = 2
      ..style = PaintingStyle.stroke;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, const Radius.circular(8)), borderPaint);

    // Inner symbol depicting wave nature (sine wave for transverse, bars for longitudinal)
    final symbolPaint = Paint()
      ..color = Colors.white70
      ..strokeWidth = 2
      ..style = PaintingStyle.stroke;

    if (isTransverse) {
      // Sine wave symbol
      final path = Path()
        ..moveTo(8, size.y / 2)
        ..quadraticBezierTo(size.x * 0.35, 10, size.x * 0.5, size.y / 2)
        ..quadraticBezierTo(size.x * 0.65, size.y - 10, size.x - 8, size.y / 2);
      canvas.drawPath(path, symbolPaint);
    } else {
      // Bar wave symbol
      canvas.drawLine(const Offset(10, 10), const Offset(10, 35), symbolPaint);
      canvas.drawLine(const Offset(14, 10), const Offset(14, 35), symbolPaint);
      canvas.drawLine(const Offset(22, 10), const Offset(22, 35), symbolPaint);
      canvas.drawLine(const Offset(30, 10), const Offset(30, 35), symbolPaint);
      canvas.drawLine(const Offset(34, 10), const Offset(34, 35), symbolPaint);
    }

    // Label Text
    final text = isTransverse ? 'WATER' : 'SOUND';
    final tp = TextPainter(
      text: TextSpan(
        text: '$text\n${frequency.toInt()}Hz',
        style: const TextStyle(
          color: Colors.white,
          fontSize: 7.5,
          fontWeight: FontWeight.bold,
        ),
      ),
      textDirection: TextDirection.ltr,
      textAlign: TextAlign.center,
    )..layout();
    tp.paint(canvas, Offset((size.x - tp.width) / 2, size.y - tp.height - 4));
  }
}

// ---------------------------------------------------------
// 4. Flutter Screen Wrapper
// ---------------------------------------------------------

class WavesGameScreen extends StatefulWidget {
  const WavesGameScreen({super.key});

  @override
  State<WavesGameScreen> createState() => _WavesGameScreenState();
}

class _WavesGameScreenState extends State<WavesGameScreen> {
  late final WavesGame _game;

  bool _showCalculation = false;
  String _activeGeneratorName = '';
  double _activeFrequency = 0;
  double _activeWavelength = 0;

  bool _showVictory = false;
  double _victoryVelocity = 0.0;

  @override
  void initState() {
    super.initState();
    _game = WavesGame(
      onShowCalculation: (name, frequency, wavelength) {
        setState(() {
          _activeGeneratorName = name;
          _activeFrequency = frequency;
          _activeWavelength = wavelength;
          _showCalculation = true;
        });
      },
      onShowVictory: (name, velocity) {
        setState(() {
          _victoryVelocity = velocity;
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
        builder: (_) => LessonsDashboard(
          lessonId: LessonIdHelper.getLessonId('Waves'),
          lessonTitle: 'Reflection and Refraction of Waves',
          grade: 'Grade 9 Physics',
        ),
      ),
      (route) => false,
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFF070B19),
      resizeToAvoidBottomInset: false,
      appBar: AppBar(
        title: const Text(
          'Puzzle: Wave Transmission Station',
          style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
        ),
        backgroundColor: const Color(0xFF0F172A),
        foregroundColor: Colors.white,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: _exitToLessonDashboard,
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh_rounded),
            tooltip: 'Reset Generator',
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
                color: const Color(0xFF0F172A),
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
                        'Drag Generator to Transmitter Tower',
                        style: TextStyle(
                          color: Colors.white70,
                          fontWeight: FontWeight.w600,
                          fontSize: 12,
                        ),
                      ),
                    ],
                  ),
                  Text(
                    'v = f · λ',
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
            if (_game.isTransmitting)
              Container(
                margin: const EdgeInsets.fromLTRB(10, 6, 10, 0),
                padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
                decoration: BoxDecoration(
                  color: const Color(0xFF0F172A),
                  borderRadius: BorderRadius.circular(10),
                  border: Border.all(color: Colors.yellowAccent.withAlpha(100), width: 1.5),
                ),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceAround,
                  children: [
                    _buildHUDStat('Wave Type', _activeGeneratorName.contains('Water') ? 'Transverse' : 'Longitudinal'),
                    _buildHUDStat('Frequency', '${_activeFrequency.toInt()} Hz'),
                    _buildHUDStat('Wavelength', '${_activeWavelength.toInt()} m'),
                    _buildHUDStat('Velocity', '${_game.calculatedVelocity.toStringAsFixed(0)} m/s'),
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
                            generatorName: _activeGeneratorName,
                            frequency: _activeFrequency,
                            wavelength: _activeWavelength,
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
                              generatorName: _activeGeneratorName,
                              velocity: _victoryVelocity,
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
