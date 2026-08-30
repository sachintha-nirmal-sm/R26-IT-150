import '../lesson_id_helper.dart';
import 'dart:math';
import 'dart:ui';
import 'package:flame/components.dart';
import 'package:flame/events.dart';
import 'package:flame/game.dart';
import 'package:flutter/material.dart';
import '../../lessons/Lessons_Dashboard.dart';

// =========================================================================
// 1. GAME WIDGET OVERLAY: CALCULATION CHECKPOINT
// =========================================================================

class CalculationOverlay extends StatefulWidget {
  final SmartMeterGame game;

  const CalculationOverlay({super.key, required this.game});

  @override
  State<CalculationOverlay> createState() => _CalculationOverlayState();
}

class _CalculationOverlayState extends State<CalculationOverlay>
    with SingleTickerProviderStateMixin {
  final TextEditingController _kWController = TextEditingController();
  final TextEditingController _kWhController = TextEditingController();
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

    _shakeAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(
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
    _kWController.dispose();
    _kWhController.dispose();
    _animationController.dispose();
    super.dispose();
  }

  void _submit() {
    final kWInput = double.tryParse(_kWController.text.trim());
    final kWhInput = double.tryParse(_kWhController.text.trim());

    if (kWInput == null || kWhInput == null) {
      setState(() {
        _showError = true;
      });
      _animationController.forward(from: 0);
      return;
    }

    final appliance = widget.game.selectedAppliance;
    if (appliance == null) return;

    final double correctKW = appliance.powerW / 1000.0;
    final double correctKWh = correctKW * appliance.usageTimeHours;

    // Validate inputs with 0.01 tolerance
    final bool isKWCorrect = (kWInput - correctKW).abs() < 0.01;
    final bool isKWhCorrect = (kWhInput - correctKWh).abs() < 0.01;

    if (isKWCorrect && isKWhCorrect) {
      widget.game.overlays.remove('CalculationOverlay');
      widget.game.resumeEngine();
      widget.game.startSimulation();
    } else {
      setState(() {
        _showError = true;
      });
      _animationController.forward(from: 0);
    }
  }

  @override
  Widget build(BuildContext context) {
    final appliance = widget.game.selectedAppliance;
    if (appliance == null) return const SizedBox.shrink();

    return Material(
      color: Colors.black.withOpacity(0.65),
      child: Center(
        child: BackdropFilter(
          filter: ImageFilter.blur(sigmaX: 5, sigmaY: 5),
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: AnimatedBuilder(
              animation: _shakeAnimation,
              builder: (context, child) {
                final double offset = sin(_animationController.value * pi * 8) * 16 * (1 - _animationController.value);
                return Transform.translate(
                  offset: Offset(offset, 0),
                  child: child,
                );
              },
              child: Container(
                constraints: const BoxConstraints(maxWidth: 380),
                padding: const EdgeInsets.all(24),
                decoration: BoxDecoration(
                  color: const Color(0xFF1E293B),
                  borderRadius: BorderRadius.circular(20),
                  border: Border.all(
                    color: Colors.cyanAccent.withOpacity(0.5),
                    width: 2,
                  ),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.cyanAccent.withOpacity(0.1),
                      blurRadius: 20,
                      spreadRadius: 2,
                    ),
                  ],
                ),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Container(
                          padding: const EdgeInsets.all(8),
                          decoration: BoxDecoration(
                            color: Colors.cyanAccent.withOpacity(0.15),
                            shape: BoxShape.circle,
                          ),
                          child: Icon(
                            appliance.applianceName.contains('Iron')
                                ? Icons.iron
                                : Icons.toys,
                            color: Colors.cyanAccent,
                            size: 28,
                          ),
                        ),
                        const SizedBox(width: 12),
                        const Expanded(
                          child: Text(
                            'Calculation Checkpoint',
                            style: TextStyle(
                              fontSize: 18,
                              fontWeight: FontWeight.bold,
                              color: Colors.white,
                            ),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 16),
                    Text(
                      'You plugged in the ${appliance.applianceName}.\n'
                      '• Power Rating = ${appliance.powerW.toInt()} W\n'
                      '• Usage Time = ${appliance.usageTimeHours.toInt()} Hours',
                      style: const TextStyle(
                        color: Color(0xFFCBD5E1),
                        fontSize: 14,
                        height: 1.5,
                      ),
                    ),
                    const SizedBox(height: 20),
                    const Text(
                      'Convert Power to Kilowatts (kW):',
                      style: TextStyle(
                        color: Colors.white70,
                        fontSize: 12,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 6),
                    TextField(
                      controller: _kWController,
                      keyboardType: const TextInputType.numberWithOptions(decimal: true),
                      style: const TextStyle(color: Colors.white),
                      decoration: InputDecoration(
                        hintText: 'Power (kW) = Watts / 1000',
                        hintStyle: TextStyle(color: Colors.white.withOpacity(0.3)),
                        filled: true,
                        fillColor: const Color(0xFF0F172A),
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(10),
                        ),
                        focusedBorder: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(10),
                          borderSide: const BorderSide(color: Colors.cyanAccent, width: 2),
                        ),
                      ),
                    ),
                    const SizedBox(height: 16),
                    const Text(
                      'Calculate Total Energy (kWh):',
                      style: TextStyle(
                        color: Colors.white70,
                        fontSize: 12,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 6),
                    TextField(
                      controller: _kWhController,
                      keyboardType: const TextInputType.numberWithOptions(decimal: true),
                      style: const TextStyle(color: Colors.white),
                      decoration: InputDecoration(
                        hintText: 'Energy (kWh) = Power (kW) * Time (hours)',
                        hintStyle: TextStyle(color: Colors.white.withOpacity(0.3)),
                        filled: true,
                        fillColor: const Color(0xFF0F172A),
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(10),
                        ),
                        focusedBorder: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(10),
                          borderSide: const BorderSide(color: Colors.cyanAccent, width: 2),
                        ),
                      ),
                    ),
                    if (_showError) ...[
                      const SizedBox(height: 14),
                      Container(
                        padding: const EdgeInsets.all(10),
                        decoration: BoxDecoration(
                          color: Colors.redAccent.withOpacity(0.1),
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(color: Colors.redAccent.withOpacity(0.3)),
                        ),
                        child: const Row(
                          children: [
                            Icon(Icons.lightbulb_outline, color: Colors.redAccent, size: 16),
                            SizedBox(width: 8),
                            Expanded(
                              child: Text(
                                'Hint: 1 kW = 1000 W.\nEnergy (kWh) = Power (kW) * time (hours)',
                                style: TextStyle(
                                  color: Colors.redAccent,
                                  fontSize: 12,
                                  fontWeight: FontWeight.bold,
                                  height: 1.4,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                    const SizedBox(height: 20),
                    SizedBox(
                      width: double.infinity,
                      height: 48,
                      child: ElevatedButton(
                        onPressed: _submit,
                        style: ElevatedButton.styleFrom(
                          backgroundColor: Colors.cyanAccent,
                          foregroundColor: const Color(0xFF0F172A),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(10),
                          ),
                          elevation: 4,
                        ),
                        child: const Text(
                          'Submit Answers',
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
      ),
    );
  }
}

// =========================================================================
// 2. GAME WIDGET OVERLAY: SUCCESS SCREEN
// =========================================================================

class SuccessOverlay extends StatelessWidget {
  final SmartMeterGame game;

  const SuccessOverlay({super.key, required this.game});

  @override
  Widget build(BuildContext context) {
    const emeraldColor = Color(0xFF10B981);
    return Material(
      color: Colors.black.withOpacity(0.7),
      child: Center(
        child: BackdropFilter(
          filter: ImageFilter.blur(sigmaX: 5, sigmaY: 5),
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: Container(
              constraints: const BoxConstraints(maxWidth: 400),
              padding: const EdgeInsets.all(24),
              decoration: BoxDecoration(
                color: const Color(0xFF1E293B),
                borderRadius: BorderRadius.circular(20),
                border: Border.all(
                  color: emeraldColor.withOpacity(0.5),
                  width: 2.5,
                ),
                boxShadow: [
                  BoxShadow(
                    color: emeraldColor.withOpacity(0.15),
                    blurRadius: 25,
                    spreadRadius: 2,
                  ),
                ],
              ),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: emeraldColor.withOpacity(0.1),
                      shape: BoxShape.circle,
                    ),
                    child: const Icon(
                      Icons.check_circle_outline,
                      color: emeraldColor,
                      size: 64,
                    ),
                  ),
                  const SizedBox(height: 16),
                  const Text(
                    'Mission Complete!',
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 22,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 14),
                  const Text(
                    'Both appliances consumed exactly 5 kWh (5 Units) of electricity!\n\n'
                    'The Iron used energy at a very fast rate (High Power of 1.0 kW for 5 hours), '
                    'while the Fan used it slowly over a long time (Low Power of 0.05 kW for 100 hours).',
                    textAlign: TextAlign.center,
                    style: TextStyle(
                      color: Color(0xFFCBD5E1),
                      fontSize: 14,
                      height: 1.5,
                    ),
                  ),
                  const SizedBox(height: 24),
                  SizedBox(
                    width: double.infinity,
                    height: 48,
                    child: ElevatedButton(
                      onPressed: () {
                        game.resetGame();
                      },
                      style: ElevatedButton.styleFrom(
                        backgroundColor: emeraldColor,
                        foregroundColor: const Color(0xFF0F172A),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(10),
                        ),
                        elevation: 4,
                      ),
                      child: const Text(
                        'Reset / Try Another',
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

// =========================================================================
// 3. FLAME ENGINE COMPONENTS
// =========================================================================

class WallBackground extends PositionComponent with HasGameReference<SmartMeterGame> {
  WallBackground() : super(priority: -10);

  @override
  void onGameResize(Vector2 size) {
    super.onGameResize(size);
    this.size = size;
  }

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    final paint = Paint()
      ..shader = const LinearGradient(
        begin: Alignment.topCenter,
        end: Alignment.bottomCenter,
        colors: [
          Color(0xFF0F172A),
          Color(0xFF1E293B),
        ],
      ).createShader(rect);
    canvas.drawRect(rect, paint);

    final gridPaint = Paint()
      ..color = Colors.cyanAccent.withOpacity(0.04)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 1.0;
    const spacing = 40.0;
    for (double x = 0; x < size.x; x += spacing) {
      canvas.drawLine(Offset(x, 0), Offset(x, size.y), gridPaint);
    }
    for (double y = 0; y < size.y; y += spacing) {
      canvas.drawLine(Offset(0, y), Offset(size.x, y), gridPaint);
    }
  }
}

class SmartMeter extends PositionComponent with HasGameReference<SmartMeterGame> {
  SmartMeter({required Vector2 position})
      : super(position: position, size: Vector2(180, 130), anchor: Anchor.center);

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    final boxPaint = Paint()
      ..color = const Color(0xFF475569)
      ..style = PaintingStyle.fill;
    final borderPaint = Paint()
      ..color = Colors.cyanAccent.withOpacity(0.8)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 3;

    final rrect = RRect.fromRectAndRadius(rect, const Radius.circular(16));
    canvas.drawRRect(rrect, boxPaint);
    canvas.drawRRect(rrect, borderPaint);

    final labelPainter = TextPainter(
      text: const TextSpan(
        text: 'SMART METER',
        style: TextStyle(
          color: Colors.white70,
          fontSize: 10,
          fontWeight: FontWeight.bold,
          letterSpacing: 1.5,
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    labelPainter.paint(canvas, Offset((size.x - labelPainter.width) / 2, 8));

    final lcdRect = Rect.fromLTWH(15, 30, size.x - 30, 36);
    final lcdRRect = RRect.fromRectAndRadius(lcdRect, const Radius.circular(6));
    canvas.drawRRect(lcdRRect, Paint()..color = const Color(0xFF022C22));
    canvas.drawRRect(lcdRRect, Paint()..color = const Color(0xFF10B981).withOpacity(0.2)..style = PaintingStyle.stroke);

    final double currentEnergy = game.simulationActive || game.simulationCompleted
        ? game.powerkW * game.elapsedHours
        : 0.00;

    final valuePainter = TextPainter(
      text: TextSpan(
        text: '${currentEnergy.toStringAsFixed(2)} kWh',
        style: const TextStyle(
          color: Color(0xFF10B981),
          fontSize: 16,
          fontWeight: FontWeight.bold,
          fontFamily: 'Courier',
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    valuePainter.paint(canvas, Offset(25, 38));

    final diskCenter = Offset(size.x / 2, 92);
    const diskRadius = 20.0;

    canvas.drawCircle(diskCenter, diskRadius, Paint()..color = const Color(0xFF1E293B));
    canvas.drawCircle(diskCenter, diskRadius, Paint()..color = Colors.white54..style = PaintingStyle.stroke..strokeWidth = 1.5);

    final angle = game.diskAngle;
    final markerOffset = Offset(
      diskCenter.dx + diskRadius * cos(angle),
      diskCenter.dy + diskRadius * sin(angle),
    );
    canvas.drawLine(
      diskCenter,
      markerOffset,
      Paint()
        ..color = Colors.redAccent
        ..strokeWidth = 3
        ..style = PaintingStyle.stroke,
    );
  }
}

class ClockComponent extends PositionComponent with HasGameReference<SmartMeterGame> {
  ClockComponent({required Vector2 position})
      : super(position: position, size: Vector2(100, 100), anchor: Anchor.center);

  @override
  void render(Canvas canvas) {
    final center = Offset(size.x / 2, size.y / 2);
    final radius = size.x / 2 - 4;

    canvas.drawCircle(center, radius, Paint()..color = const Color(0xFF1E293B));
    canvas.drawCircle(
      center,
      radius,
      Paint()
        ..color = Colors.amberAccent.withOpacity(0.8)
        ..style = PaintingStyle.stroke
        ..strokeWidth = 3,
    );

    final tickPaint = Paint()
      ..color = Colors.white70
      ..strokeWidth = 1.5;
    for (int i = 0; i < 12; i++) {
      final tickAngle = i * (2 * pi / 12);
      final outerPt = Offset(center.dx + radius * cos(tickAngle), center.dy + radius * sin(tickAngle));
      final innerPt = Offset(center.dx + (radius - 6) * cos(tickAngle), center.dy + (radius - 6) * sin(tickAngle));
      canvas.drawLine(innerPt, outerPt, tickPaint);
    }

    final handAngle = game.clockAngle - pi / 2;
    final handLength = radius * 0.7;
    final handPt = Offset(
      center.dx + handLength * cos(handAngle),
      center.dy + handLength * sin(handAngle),
    );
    canvas.drawLine(
      center,
      handPt,
      Paint()
        ..color = Colors.amberAccent
        ..strokeWidth = 3.5
        ..strokeCap = StrokeCap.round,
    );

    canvas.drawCircle(center, 4, Paint()..color = Colors.white);
  }
}

class WallSocket extends PositionComponent {
  WallSocket({required Vector2 position})
      : super(position: position, size: Vector2(70, 70), anchor: Anchor.center);

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    final rrect = RRect.fromRectAndRadius(rect, const Radius.circular(12));

    canvas.drawRRect(rrect, Paint()..color = const Color(0xFFF1F5F9));
    canvas.drawRRect(
      rrect,
      Paint()
        ..color = Colors.grey[400]!
        ..style = PaintingStyle.stroke
        ..strokeWidth = 2,
    );

    final pinPaint = Paint()..color = const Color(0xFF334155);
    canvas.drawCircle(Offset(size.x * 0.3, size.y * 0.45), 6, pinPaint);
    canvas.drawCircle(Offset(size.x * 0.7, size.y * 0.45), 6, pinPaint);
    canvas.drawRect(
      Rect.fromCenter(center: Offset(size.x * 0.5, size.y * 0.7), width: 6, height: 12),
      pinPaint,
    );

    final labelPainter = TextPainter(
      text: const TextSpan(
        text: 'SOCKET',
        style: TextStyle(
          color: Color(0xFF64748B),
          fontSize: 8,
          fontWeight: FontWeight.bold,
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    labelPainter.paint(canvas, Offset((size.x - labelPainter.width) / 2, 8));
  }
}

class Appliance extends PositionComponent with DragCallbacks, HasGameReference<SmartMeterGame> {
  final String applianceName;
  final double powerW;
  final double usageTimeHours;
  final Color baseColor;
  final Vector2 startPosition;

  bool isDragging = false;
  bool isSnapped = false;

  Appliance({
    required this.applianceName,
    required this.powerW,
    required this.usageTimeHours,
    required this.baseColor,
    required this.startPosition,
    required Vector2 size,
  }) : super(position: startPosition.clone(), size: size, anchor: Anchor.center);

  @override
  void onDragStart(DragStartEvent event) {
    if (isSnapped || game.simulationActive || game.simulationCompleted) return;
    super.onDragStart(event);
    isDragging = true;
    priority = 10;
  }

  @override
  void onDragUpdate(DragUpdateEvent event) {
    if (isDragging) {
      position += event.localDelta;
      position.clamp(Vector2(size.x / 2, size.y / 2), game.size - Vector2(size.x / 2, size.y / 2));
    }
  }

  @override
  void onDragEnd(DragEndEvent event) {
    if (!isDragging) return;
    super.onDragEnd(event);
    isDragging = false;
    priority = 1;

    final socket = game.socket;
    final distance = position.distanceTo(socket.position);
    if (distance < 60) {
      isSnapped = true;
      position.setFrom(socket.position);
      game.snapAppliance(this);
    } else {
      position.setFrom(startPosition);
    }
  }

  @override
  void onDragCancel(DragCancelEvent event) {
    super.onDragCancel(event);
    isDragging = false;
    priority = 1;
    position.setFrom(startPosition);
  }

  void reset() {
    isSnapped = false;
    isDragging = false;
    priority = 1;
    position.setFrom(startPosition);
  }

  @override
  void render(Canvas canvas) {
    if (isDragging) {
      final shadowPaint = Paint()
        ..color = Colors.black.withOpacity(0.4)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 6);
      canvas.drawRRect(
        RRect.fromRectAndRadius(Rect.fromLTWH(6, 6, size.x, size.y), const Radius.circular(8)),
        shadowPaint,
      );
    }

    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    final rrect = RRect.fromRectAndRadius(rect, const Radius.circular(8));

    canvas.drawRRect(rrect, Paint()..color = baseColor);

    if (!isSnapped && !game.simulationActive && !game.simulationCompleted) {
      canvas.drawRRect(
        rrect,
        Paint()
          ..color = Colors.yellowAccent.withOpacity(0.6)
          ..style = PaintingStyle.stroke
          ..strokeWidth = 2,
      );
    }

    if (applianceName == 'Electric Iron') {
      canvas.drawRect(
        Rect.fromLTWH(2, size.y - 8, size.x - 4, 6),
        Paint()..color = const Color(0xFFCBD5E1),
      );
      final path = Path()
        ..moveTo(10, size.y * 0.6)
        ..quadraticBezierTo(size.x / 2, 5, size.x - 10, size.y * 0.6)
        ..lineTo(size.x - 15, size.y * 0.65)
        ..quadraticBezierTo(size.x / 2, 15, 15, size.y * 0.65)
        ..close();
      canvas.drawPath(path, Paint()..color = Colors.white70);
    } else {
      final center = Offset(size.x / 2, size.y / 2 - 10);
      final grilleRadius = size.y / 2 - 16;
      canvas.drawCircle(center, grilleRadius, Paint()..color = Colors.white24);
      canvas.drawCircle(center, grilleRadius, Paint()..color = Colors.white54..style = PaintingStyle.stroke..strokeWidth = 1.5);

      final bladePaint = Paint()..color = Colors.cyanAccent..style = PaintingStyle.fill;
      double bladeAngle = 0.0;
      if (game.simulationActive && isSnapped) {
        bladeAngle = game.elapsedHours * 10.0 * (powerW == 1000 ? 5.0 : 1.0);
      }

      for (int i = 0; i < 3; i++) {
        final angle = bladeAngle + i * (2 * pi / 3);
        final bx = center.dx + grilleRadius * 0.8 * cos(angle);
        final by = center.dy + grilleRadius * 0.8 * sin(angle);
        canvas.drawCircle(Offset(bx, by), 5, bladePaint);
        canvas.drawLine(center, Offset(bx, by), Paint()..color = Colors.cyanAccent..strokeWidth = 2);
      }

      canvas.drawLine(
        Offset(size.x / 2, center.dy + grilleRadius),
        Offset(size.x / 2, size.y - 12),
        Paint()..color = Colors.white..strokeWidth = 4,
      );

      canvas.drawRect(
        Rect.fromLTWH(size.x * 0.25, size.y - 12, size.x * 0.5, 6),
        Paint()..color = Colors.white70,
      );
    }

    final textPainter = TextPainter(
      text: TextSpan(
        text: '$applianceName\n${powerW.toInt()} W',
        style: const TextStyle(
          color: Colors.white,
          fontSize: 9,
          fontWeight: FontWeight.bold,
          height: 1.2,
        ),
      ),
      textDirection: TextDirection.ltr,
      textAlign: TextAlign.center,
    )..layout();

    final bgRect = Rect.fromCenter(
      center: Offset(size.x / 2, size.y / 2),
      width: textPainter.width + 8,
      height: textPainter.height + 4,
    );
    canvas.drawRRect(
      RRect.fromRectAndRadius(bgRect, const Radius.circular(4)),
      Paint()..color = Colors.black.withOpacity(0.5),
    );

    textPainter.paint(
      canvas,
      Offset((size.x - textPainter.width) / 2, (size.y - textPainter.height) / 2),
    );
  }
}

class HUDPanel extends PositionComponent with HasGameReference<SmartMeterGame> {
  HUDPanel({required Vector2 position})
      : super(position: position, size: Vector2(280, 75), anchor: Anchor.center);

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    final rrect = RRect.fromRectAndRadius(rect, const Radius.circular(12));
    canvas.drawRRect(rrect, Paint()..color = const Color(0xFF1E293B).withOpacity(0.85));
    canvas.drawRRect(
      rrect,
      Paint()
        ..color = Colors.cyanAccent.withOpacity(0.3)
        ..style = PaintingStyle.stroke
        ..strokeWidth = 1.5,
    );

    final specLabel = game.selectedAppliance != null
        ? '${game.selectedAppliance!.applianceName} Active'
        : 'Select & Plug in an Appliance';

    final labelPainter = TextPainter(
      text: TextSpan(
        text: specLabel,
        style: TextStyle(
          color: game.selectedAppliance != null ? Colors.cyanAccent : Colors.white60,
          fontSize: 10,
          fontWeight: FontWeight.bold,
          letterSpacing: 1.0,
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    labelPainter.paint(canvas, Offset(16, 8));

    final currentHrs = game.simulationActive || game.simulationCompleted
        ? game.elapsedHours
        : 0.0;
    final currentKWh = game.simulationActive || game.simulationCompleted
        ? game.powerkW * game.elapsedHours
        : 0.0;

    final statsText = 'Time Passed: ${currentHrs.toStringAsFixed(1)} Hours\n'
        'Energy Consumed: ${currentKWh.toStringAsFixed(2)} kWh';

    final statsPainter = TextPainter(
      text: TextSpan(
        text: statsText,
        style: const TextStyle(
          color: Colors.white,
          fontSize: 12,
          fontWeight: FontWeight.w600,
          height: 1.5,
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    statsPainter.paint(canvas, Offset(16, 26));
  }
}

class SmartMeterGame extends FlameGame with DragCallbacks {
  late WallBackground wallBackground;
  late SmartMeter smartMeter;
  late ClockComponent clock;
  late WallSocket socket;
  late Appliance iron;
  late Appliance fan;
  late HUDPanel hudPanel;

  Appliance? selectedAppliance;

  bool _isLoaded = false;
  bool simulationActive = false;
  bool simulationCompleted = false;
  double elapsedHours = 0.0;
  double targetTime = 0.0;
  double powerkW = 0.0;
  double diskAngle = 0.0;
  double clockAngle = 0.0;

  @override
  Future<void> onLoad() async {
    wallBackground = WallBackground();
    add(wallBackground);

    smartMeter = SmartMeter(position: Vector2(100, 100));
    clock = ClockComponent(position: Vector2(300, 100));
    socket = WallSocket(position: Vector2(200, 300));

    iron = Appliance(
      applianceName: 'Electric Iron',
      powerW: 1000.0,
      usageTimeHours: 5.0,
      baseColor: const Color(0xFFEF4444),
      startPosition: Vector2(100, 450),
      size: Vector2(90, 60),
    );

    fan = Appliance(
      applianceName: 'Pedestal Fan',
      powerW: 50.0,
      usageTimeHours: 100.0,
      baseColor: const Color(0xFF3B82F6),
      startPosition: Vector2(300, 450),
      size: Vector2(90, 90),
    );

    hudPanel = HUDPanel(position: Vector2(200, 210));

    add(smartMeter);
    add(clock);
    add(socket);
    add(iron);
    add(fan);
    add(hudPanel);
    _isLoaded = true;
  }

  @override
  void onGameResize(Vector2 size) {
    super.onGameResize(size);
    // Guard: components are not yet initialized before onLoad completes
    if (!_isLoaded) return;

    final centerX = size.x / 2;

    smartMeter.position = Vector2(centerX - 100, 90);
    clock.position = Vector2(centerX + 110, 90);
    hudPanel.position = Vector2(centerX, 195);
    socket.position = Vector2(centerX, size.y - 180);

    iron.startPosition.setValues(centerX - 110, size.y - 75);
    fan.startPosition.setValues(centerX + 110, size.y - 75);

    if (!iron.isSnapped) iron.position.setFrom(iron.startPosition);
    if (!fan.isSnapped) fan.position.setFrom(fan.startPosition);
  }

  void snapAppliance(Appliance appliance) {
    selectedAppliance = appliance;
    if (appliance == iron) {
      fan.position.setValues(-1000, -1000);
    } else {
      iron.position.setValues(-1000, -1000);
    }

    pauseEngine();
    overlays.add('CalculationOverlay');
  }

  void startSimulation() {
    simulationActive = true;
    elapsedHours = 0.0;
    targetTime = selectedAppliance!.usageTimeHours;
    powerkW = selectedAppliance!.powerW / 1000.0;
  }

  void resetGame() {
    simulationActive = false;
    simulationCompleted = false;
    elapsedHours = 0.0;
    targetTime = 0.0;
    powerkW = 0.0;
    diskAngle = 0.0;
    clockAngle = 0.0;

    selectedAppliance = null;

    iron.reset();
    fan.reset();

    onGameResize(size);

    overlays.remove('SuccessOverlay');
    overlays.remove('CalculationOverlay');

    resumeEngine();
  }

  @override
  void update(double dt) {
    super.update(dt);

    if (simulationActive && selectedAppliance != null) {
      final double virtualTimeRate = targetTime / 5.0;
      elapsedHours += dt * virtualTimeRate;

      diskAngle += dt * 15.0 * powerkW;
      clockAngle = (elapsedHours / 12.0) * 2.0 * pi;

      if (elapsedHours >= targetTime) {
        elapsedHours = targetTime;
        simulationActive = false;
        simulationCompleted = true;
        overlays.add('SuccessOverlay');
      }
    }
  }
}

// =========================================================================
// 4. MAIN FLUTTER SCREEN SCAFFOLD & ENTRY WIDGET
// =========================================================================

class PowerEnergyGameScreen extends StatefulWidget {
  const PowerEnergyGameScreen({super.key});

  @override
  State<PowerEnergyGameScreen> createState() => _PowerEnergyGameScreenState();
}

class _PowerEnergyGameScreenState extends State<PowerEnergyGameScreen> {
  late final SmartMeterGame _game;

  @override
  void initState() {
    super.initState();
    _game = SmartMeterGame();
  }

  void _onReset() {
    _game.resetGame();
  }

  void _exitToLessonDashboard() {
    _game.resetGame();
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(
        builder: (_) => LessonsDashboard(
          lessonId: LessonIdHelper.getLessonId('Power and Energy'),
          lessonTitle: 'Power and Energy of Electric Appliances',
          grade: 'Grade 11 Physics',
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
          'Puzzle 4: Smart Meter Challenge',
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
            tooltip: 'Reset Game',
            onPressed: _onReset,
          ),
        ],
      ),
      body: SafeArea(
        child: Column(
          children: [
            Container(
              width: double.infinity,
              margin: const EdgeInsets.fromLTRB(10, 8, 10, 0),
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
              decoration: BoxDecoration(
                color: const Color(0xFF1E293B),
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: Colors.cyanAccent.withOpacity(0.4), width: 1),
              ),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Flexible(
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: const [
                        Icon(Icons.info_outline, color: Colors.cyanAccent, size: 14),
                        SizedBox(width: 6),
                        Flexible(
                          child: Text(
                            'Drag Appliance to Wall Socket',
                            overflow: TextOverflow.ellipsis,
                            style: TextStyle(
                              color: Colors.white70,
                              fontWeight: FontWeight.w600,
                              fontSize: 11,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(width: 8),
                  const Flexible(
                    child: Text(
                      'E(kWh) = P(kW) × T(h)',
                      overflow: TextOverflow.ellipsis,
                      style: TextStyle(
                        color: Colors.cyanAccent,
                        fontSize: 11,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ],
              ),
            ),
            Expanded(
              child: Container(
                margin: const EdgeInsets.fromLTRB(10, 8, 10, 10),
                clipBehavior: Clip.antiAlias,
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(14),
                  border: Border.all(color: const Color(0xFF334155), width: 2),
                ),
                child: GameWidget<SmartMeterGame>(
                  game: _game,
                  overlayBuilderMap: {
                    'CalculationOverlay': (context, game) => CalculationOverlay(game: game),
                    'SuccessOverlay': (context, game) => SuccessOverlay(game: game),
                  },
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
