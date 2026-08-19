import 'dart:math';
import 'dart:ui';
import 'package:flame/components.dart';
import 'package:flame/events.dart';
import 'package:flame/game.dart';
import 'package:flutter/material.dart';
import '../../lessons/Lessons_Dashboard.dart';

// =========================================================================
// 1. ENUMS & DATA MODELS
// =========================================================================

enum CoilType { basic, industrial }

enum PhysicsPhase { idle, lowering, testing, lifting, finished }

// =========================================================================
// 2. FLAME GAME ENGINE
// =========================================================================

class ElectromagnetismGame extends FlameGame with DragCallbacks {
  late ScrapyardEnvironment environment;
  late CraneHead craneHead;
  late ScrapCar scrapCar;

  late CoilKitComponent basicCoil;
  late CoilKitComponent industrialCoil;

  CoilKitComponent? attachedCoil;

  bool _isLoaded = false;

  // Animation & Telemetry State
  PhysicsPhase animationPhase = PhysicsPhase.idle;
  double craneOffsetY = 0.0;
  double carOffsetY = 0.0;
  double calculatedForce = 0.0;
  String telemetryText = 'Drag a Coil Kit to the Crane Core';
  bool isOutcomeSuccess = false;

  // Responsive Coordinates
  double craneX = 0.0;
  double topY = 0.0;
  double groundY = 0.0;

  @override
  Future<void> onLoad() async {
    environment = ScrapyardEnvironment();
    add(environment);

    craneHead = CraneHead(position: Vector2(200, 150));
    scrapCar = ScrapCar(position: Vector2(200, 300));

    basicCoil = CoilKitComponent(
      type: CoilType.basic,
      name: 'Basic Coil Kit',
      turnsN: 100,
      currentI: 2.0,
      baseColor: const Color(0xFF2563EB), // Blue
      startPosition: Vector2(100, 450),
      size: Vector2(65, 65),
    );

    industrialCoil = CoilKitComponent(
      type: CoilType.industrial,
      name: 'Industrial Coil Kit',
      turnsN: 200,
      currentI: 5.0,
      baseColor: const Color(0xFFEA580C), // Orange
      startPosition: Vector2(300, 450),
      size: Vector2(65, 65),
    );

    add(craneHead);
    add(scrapCar);
    add(basicCoil);
    add(industrialCoil);

    _isLoaded = true;
  }

  @override
  void onGameResize(Vector2 size) {
    super.onGameResize(size);
    if (!_isLoaded) return;

    final centerX = size.x / 2;

    // Mobile-friendly coordinates
    craneX = min(size.x - 75.0, centerX + 50.0);
    topY = max(70.0, size.y * 0.16);
    groundY = size.y - 130.0;

    craneHead.position = Vector2(craneX, topY + craneOffsetY);
    scrapCar.position = Vector2(craneX, groundY - 32.0 + carOffsetY);

    final spacing = min(size.x / 3.2, 110.0);
    final bottomY = size.y - 55.0;

    basicCoil.startPosition.setValues(centerX - spacing, bottomY);
    industrialCoil.startPosition.setValues(centerX + spacing, bottomY);

    if (!basicCoil.isSnapped) basicCoil.position.setFrom(basicCoil.startPosition);
    if (!industrialCoil.isSnapped) industrialCoil.position.setFrom(industrialCoil.startPosition);
  }

  void onCoilSnapped(CoilKitComponent coil) {
    attachedCoil = coil;
    // Hide the unselected coil
    if (coil == basicCoil) {
      industrialCoil.position.setValues(-1000, -1000);
    } else {
      basicCoil.position.setValues(-1000, -1000);
    }

    pauseEngine();
    overlays.add('CalculationOverlay');
  }

  void startPhysicsReaction() {
    if (attachedCoil == null) return;

    calculatedForce = 0.5 * attachedCoil!.turnsN * attachedCoil!.currentI;
    animationPhase = PhysicsPhase.lowering;
    craneOffsetY = 0.0;
    carOffsetY = 0.0;
  }

  void resetGame() {
    animationPhase = PhysicsPhase.idle;
    craneOffsetY = 0.0;
    carOffsetY = 0.0;
    calculatedForce = 0.0;
    attachedCoil = null;
    telemetryText = 'Drag a Coil Kit to the Crane Core';
    isOutcomeSuccess = false;

    basicCoil.reset();
    industrialCoil.reset();

    onGameResize(size);

    overlays.remove('CalculationOverlay');
    overlays.remove('ResultOverlay');

    resumeEngine();
  }

  @override
  void update(double dt) {
    super.update(dt);

    if (animationPhase == PhysicsPhase.idle || attachedCoil == null) return;

    final maxLowerOffset = (groundY - 32.0 - 40.0) - topY; // Drop to touch top of car

    if (animationPhase == PhysicsPhase.lowering) {
      craneOffsetY += dt * 140.0;
      if (craneOffsetY >= maxLowerOffset) {
        craneOffsetY = maxLowerOffset;
        animationPhase = PhysicsPhase.testing;
      }
      craneHead.position.y = topY + craneOffsetY;
    } else if (animationPhase == PhysicsPhase.testing) {
      if (calculatedForce >= 500.0) {
        // Success: 500 N lifts the heavy car
        isOutcomeSuccess = true;
        telemetryText = 'Force = 500 N. Success! The electromagnet is strong enough.';
        animationPhase = PhysicsPhase.lifting;
      } else {
        // Fail: 100 N is too weak
        isOutcomeSuccess = false;
        telemetryText = 'Force = 100 N. Too weak! The Scrap Car needs 500 N to be lifted.';
        animationPhase = PhysicsPhase.lifting;
      }
    } else if (animationPhase == PhysicsPhase.lifting) {
      craneOffsetY -= dt * 100.0;
      if (craneOffsetY <= 0.0) {
        craneOffsetY = 0.0;
        animationPhase = PhysicsPhase.finished;
        overlays.add('ResultOverlay');
      }

      craneHead.position.y = topY + craneOffsetY;

      // If success, lift the car up attached to the magnet
      if (isOutcomeSuccess) {
        carOffsetY = craneOffsetY - maxLowerOffset;
        scrapCar.position.y = (groundY - 32.0) + carOffsetY;
      }
    }
  }
}

// =========================================================================
// 3. FLAME CANVAS COMPONENTS
// =========================================================================

class ScrapyardEnvironment extends PositionComponent with HasGameReference<ElectromagnetismGame> {
  ScrapyardEnvironment() : super(priority: -10);

  @override
  void onGameResize(Vector2 size) {
    super.onGameResize(size);
    this.size = size;
  }

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);

    // Dark Scrapyard Gradient
    final bgPaint = Paint()
      ..shader = const LinearGradient(
        begin: Alignment.topCenter,
        end: Alignment.bottomCenter,
        colors: [Color(0xFF0F172A), Color(0xFF1E293B)],
      ).createShader(rect);
    canvas.drawRect(rect, bgPaint);

    // Industrial Grid lines
    final gridPaint = Paint()
      ..color = Colors.cyanAccent.withOpacity(0.03)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 1.0;
    for (double x = 0; x < size.x; x += 40) {
      canvas.drawLine(Offset(x, 0), Offset(x, size.y), gridPaint);
    }
    for (double y = 0; y < size.y; y += 40) {
      canvas.drawLine(Offset(0, y), Offset(size.x, y), gridPaint);
    }

    final craneX = game.craneX;
    final groundY = game.groundY;

    // Heavy Industrial Ground Line & Platform
    final groundPaint = Paint()..color = const Color(0xFF334155);
    canvas.drawRect(Rect.fromLTWH(0, groundY, size.x, size.y - groundY), groundPaint);
    canvas.drawLine(
      Offset(0, groundY),
      Offset(size.x, groundY),
      Paint()
        ..color = Colors.amberAccent.withOpacity(0.6)
        ..strokeWidth = 2.0,
    );

    // Overhead Crane Gantry Support Beam
    final gantryPaint = Paint()
      ..color = const Color(0xFF64748B)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 6.0;
    canvas.drawLine(Offset(craneX - 60, 35), Offset(craneX + 60, 35), gantryPaint);
    canvas.drawLine(Offset(craneX, 35), Offset(craneX, game.craneHead.position.y - 20), Paint()..color = Colors.amberAccent..strokeWidth = 2.5);
  }
}

class CraneHead extends PositionComponent with HasGameReference<ElectromagnetismGame> {
  CraneHead({required Vector2 position})
      : super(position: position, size: Vector2(90, 50), anchor: Anchor.center);

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);

    final isAttached = game.attachedCoil != null;
    final isSuccessGlow = game.isOutcomeSuccess && game.animationPhase == PhysicsPhase.lifting;

    // Magnetic Glow Effect when Powered and Lifting
    if (isSuccessGlow || (isAttached && game.calculatedForce >= 500)) {
      final glowPaint = Paint()
        ..color = Colors.amberAccent.withOpacity(0.4)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 15.0);
      canvas.drawCircle(Offset(size.x / 2, size.y / 2), 50.0, glowPaint);
    }

    // Bare Iron Core Box (or Coiled Electromagnet)
    final coreColor = isAttached ? game.attachedCoil!.baseColor : const Color(0xFF64748B);
    final coreRRect = RRect.fromRectAndRadius(rect, const Radius.circular(10));
    canvas.drawRRect(coreRRect, Paint()..color = coreColor);
    canvas.drawRRect(
      coreRRect,
      Paint()
        ..color = isAttached ? Colors.amberAccent : Colors.cyanAccent.withOpacity(0.8)
        ..style = PaintingStyle.stroke
        ..strokeWidth = 2.5,
    );

    // If attached, draw copper windings / coils
    if (isAttached) {
      final coilPaint = Paint()
        ..color = const Color(0xFFD97706) // Copper color
        ..strokeWidth = 3.0;
      for (double x = 12; x < size.x - 10; x += 10) {
        canvas.drawLine(Offset(x, 4), Offset(x, size.y - 4), coilPaint);
      }
    }

    // Iron Core Label
    final labelText = isAttached ? '${game.attachedCoil!.name}\n${game.calculatedForce.toInt()} N' : 'BARE IRON CORE\n(DROP ZONE)';
    final textPainter = TextPainter(
      text: TextSpan(
        text: labelText,
        style: const TextStyle(color: Colors.white, fontSize: 8.5, fontWeight: FontWeight.bold, height: 1.15),
      ),
      textDirection: TextDirection.ltr,
      textAlign: TextAlign.center,
    )..layout();
    textPainter.paint(canvas, Offset((size.x - textPainter.width) / 2, (size.y - textPainter.height) / 2));
  }
}

class ScrapCar extends PositionComponent with HasGameReference<ElectromagnetismGame> {
  ScrapCar({required Vector2 position})
      : super(position: position, size: Vector2(110, 60), anchor: Anchor.center);

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    final rrect = RRect.fromRectAndRadius(rect, const Radius.circular(12));

    // Car Body (Red Heavy Scrap)
    canvas.drawRRect(rrect, Paint()..color = const Color(0xFFDC2626));
    canvas.drawRRect(
      rrect,
      Paint()
        ..color = Colors.white70
        ..style = PaintingStyle.stroke
        ..strokeWidth = 2.0,
    );

    // Windshield & Windows
    canvas.drawRect(Rect.fromLTWH(20, 10, 30, 18), Paint()..color = const Color(0xFF94A3B8));
    canvas.drawRect(Rect.fromLTWH(55, 10, 30, 18), Paint()..color = const Color(0xFF94A3B8));

    // Wheels
    canvas.drawCircle(Offset(25, size.y - 4), 10, Paint()..color = Colors.black87);
    canvas.drawCircle(Offset(85, size.y - 4), 10, Paint()..color = Colors.black87);
    canvas.drawCircle(Offset(25, size.y - 4), 4, Paint()..color = Colors.grey);
    canvas.drawCircle(Offset(85, size.y - 4), 4, Paint()..color = Colors.grey);

    // Text Label
    final textPainter = TextPainter(
      text: const TextSpan(
        text: 'SCRAP CAR\nReq: 500 N',
        style: TextStyle(color: Colors.white, fontSize: 9, fontWeight: FontWeight.bold, height: 1.15),
      ),
      textDirection: TextDirection.ltr,
      textAlign: TextAlign.center,
    )..layout();
    textPainter.paint(canvas, Offset((size.x - textPainter.width) / 2, size.y / 2 - 4));
  }
}

class CoilKitComponent extends PositionComponent
    with DragCallbacks, HasGameReference<ElectromagnetismGame> {
  final CoilType type;
  final String name;
  final int turnsN;
  final double currentI;
  final Color baseColor;
  final Vector2 startPosition;

  bool isDragging = false;
  bool isSnapped = false;

  CoilKitComponent({
    required this.type,
    required this.name,
    required this.turnsN,
    required this.currentI,
    required this.baseColor,
    required this.startPosition,
    required Vector2 size,
  }) : super(position: startPosition.clone(), size: size, anchor: Anchor.center);

  @override
  void onDragStart(DragStartEvent event) {
    if (isSnapped || game.animationPhase != PhysicsPhase.idle) return;
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

    final targetCore = game.craneHead;
    final distance = position.distanceTo(targetCore.position);

    if (distance < 60) {
      // Snapped to Crane Core
      isSnapped = true;
      position.setFrom(targetCore.position);
      game.onCoilSnapped(this);
    } else {
      // Missed -> Bounce back to start
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
    isDragging = false;
    isSnapped = false;
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
        RRect.fromRectAndRadius(Rect.fromLTWH(4, 4, size.x, size.y), const Radius.circular(8)),
        shadowPaint,
      );
    }

    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    final rrect = RRect.fromRectAndRadius(rect, const Radius.circular(10));

    canvas.drawRRect(rrect, Paint()..color = baseColor);

    if (!isSnapped) {
      canvas.drawRRect(
        rrect,
        Paint()
          ..color = Colors.amberAccent.withOpacity(0.7)
          ..style = PaintingStyle.stroke
          ..strokeWidth = 2.0,
      );
    }

    // Copper winding lines on coil kit
    final coilPaint = Paint()
      ..color = const Color(0xFFF59E0B)
      ..strokeWidth = 2.5;
    for (double x = 10; x < size.x - 8; x += 8) {
      canvas.drawLine(Offset(x, 4), Offset(x, size.y - 4), coilPaint);
    }

    // Label Text
    final textPainter = TextPainter(
      text: TextSpan(
        text: '$name\nN=$turnsN, I=${currentI.toInt()}A',
        style: const TextStyle(color: Colors.white, fontSize: 8.5, fontWeight: FontWeight.bold, height: 1.15),
      ),
      textDirection: TextDirection.ltr,
      textAlign: TextAlign.center,
    )..layout();

    final bgRect = Rect.fromCenter(
      center: Offset(size.x / 2, size.y / 2),
      width: textPainter.width + 6,
      height: textPainter.height + 4,
    );
    canvas.drawRRect(
      RRect.fromRectAndRadius(bgRect, const Radius.circular(4)),
      Paint()..color = Colors.black.withOpacity(0.65),
    );

    textPainter.paint(
      canvas,
      Offset((size.x - textPainter.width) / 2, (size.y - textPainter.height) / 2),
    );
  }
}

// =========================================================================
// 4. FLUTTER OVERLAYS (CALCULATION & RESULT)
// =========================================================================

class CalculationOverlay extends StatefulWidget {
  final ElectromagnetismGame game;

  const CalculationOverlay({super.key, required this.game});

  @override
  State<CalculationOverlay> createState() => _CalculationOverlayState();
}

class _CalculationOverlayState extends State<CalculationOverlay>
    with SingleTickerProviderStateMixin {
  final TextEditingController _forceController = TextEditingController();
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
      CurvedAnimation(parent: _animationController, curve: Curves.decelerate),
    )..addStatusListener((status) {
        if (status == AnimationStatus.completed) {
          _animationController.reset();
        }
      });
  }

  @override
  void dispose() {
    _forceController.dispose();
    _animationController.dispose();
    super.dispose();
  }

  void _submit() {
    final val = double.tryParse(_forceController.text.trim());
    final attached = widget.game.attachedCoil;
    if (attached == null || val == null) {
      setState(() {
        _showError = true;
      });
      _animationController.forward(from: 0);
      return;
    }

    final double expectedForce = 0.5 * attached.turnsN * attached.currentI;
    if ((val - expectedForce).abs() < 2.0) {
      widget.game.overlays.remove('CalculationOverlay');
      widget.game.resumeEngine();
      widget.game.startPhysicsReaction();
    } else {
      setState(() {
        _showError = true;
      });
      _animationController.forward(from: 0);
    }
  }

  @override
  Widget build(BuildContext context) {
    final attached = widget.game.attachedCoil;
    if (attached == null) return const SizedBox.shrink();

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
                return Transform.translate(offset: Offset(offset, 0), child: child);
              },
              child: Container(
                constraints: const BoxConstraints(maxWidth: 380),
                padding: const EdgeInsets.all(24),
                decoration: BoxDecoration(
                  color: const Color(0xFF1E293B),
                  borderRadius: BorderRadius.circular(20),
                  border: Border.all(color: Colors.cyanAccent.withOpacity(0.5), width: 2),
                  boxShadow: [
                    BoxShadow(color: Colors.cyanAccent.withOpacity(0.1), blurRadius: 20, spreadRadius: 2),
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
                          child: const Icon(Icons.bolt, color: Colors.cyanAccent, size: 26),
                        ),
                        const SizedBox(width: 12),
                        const Expanded(
                          child: Text(
                            'Electromagnet Checkpoint',
                            style: TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: Colors.white),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 14),
                    Text(
                      'You attached the ${attached.name}!\n'
                      'The lifting force of an electromagnet depends on the number of turns (N) and current (I).\n\n'
                      'Formula: Force (F) = 0.5 × N × I\n'
                      '• Number of turns (N) = ${attached.turnsN}\n'
                      '• Current (I) = ${attached.currentI.toInt()} A',
                      style: const TextStyle(color: Color(0xFFCBD5E1), fontSize: 13, height: 1.45),
                    ),
                    const SizedBox(height: 18),
                    const Text(
                      'Calculate Lifting Force in Newtons (N):',
                      style: TextStyle(color: Colors.white70, fontSize: 12, fontWeight: FontWeight.bold),
                    ),
                    const SizedBox(height: 6),
                    TextField(
                      controller: _forceController,
                      keyboardType: TextInputType.number,
                      style: const TextStyle(color: Colors.white),
                      decoration: InputDecoration(
                        hintText: 'Enter Force (F) in N',
                        hintStyle: TextStyle(color: Colors.white.withOpacity(0.3)),
                        filled: true,
                        fillColor: const Color(0xFF0F172A),
                        border: OutlineInputBorder(borderRadius: BorderRadius.circular(10)),
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
                                'Hint: Multiply 0.5 by N, then multiply by I.',
                                style: TextStyle(color: Colors.redAccent, fontSize: 11, fontWeight: FontWeight.bold),
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
                          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                        ),
                        child: const Text('Submit Calculation', style: TextStyle(fontSize: 15, fontWeight: FontWeight.bold)),
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

class ResultOverlay extends StatelessWidget {
  final ElectromagnetismGame game;

  const ResultOverlay({super.key, required this.game});

  @override
  Widget build(BuildContext context) {
    final isSuccess = game.isOutcomeSuccess;
    final primaryColor = isSuccess ? const Color(0xFF10B981) : Colors.redAccent;

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
                border: Border.all(color: primaryColor.withOpacity(0.6), width: 2.5),
                boxShadow: [
                  BoxShadow(color: primaryColor.withOpacity(0.2), blurRadius: 25, spreadRadius: 2),
                ],
              ),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: primaryColor.withOpacity(0.15),
                      shape: BoxShape.circle,
                    ),
                    child: Icon(
                      isSuccess ? Icons.check_circle_outline : Icons.warning_amber_rounded,
                      color: primaryColor,
                      size: 60,
                    ),
                  ),
                  const SizedBox(height: 16),
                  Text(
                    isSuccess ? 'Mission Complete!' : 'Lifting Failed!',
                    style: const TextStyle(color: Colors.white, fontSize: 22, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 14),
                  Text(
                    isSuccess
                        ? 'Force = 500 N. Success! The Industrial Coil generated high magnetic force (N=200, I=5A), lifting the 500 N heavy Scrap Car.'
                        : 'Force = 100 N. Too weak! The Basic Coil (N=100, I=2A) only generated 100 N of force, which is not enough to lift the 500 N Scrap Car.',
                    textAlign: TextAlign.center,
                    style: const TextStyle(color: Color(0xFFCBD5E1), fontSize: 14, height: 1.5),
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
                        backgroundColor: primaryColor,
                        foregroundColor: const Color(0xFF0F172A),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                        elevation: 4,
                      ),
                      child: Text(
                        isSuccess ? 'Reset / Try Again' : 'Try Industrial Coil',
                        style: const TextStyle(fontSize: 15, fontWeight: FontWeight.bold),
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
// 5. MAIN FLUTTER SCREEN SCAFFOLD & ENTRY WIDGET
// =========================================================================

class ElectromagnetismGameScreen extends StatefulWidget {
  const ElectromagnetismGameScreen({super.key});

  @override
  State<ElectromagnetismGameScreen> createState() => _ElectromagnetismGameScreenState();
}

class _ElectromagnetismGameScreenState extends State<ElectromagnetismGameScreen> {
  late final ElectromagnetismGame _game;

  @override
  void initState() {
    super.initState();
    _game = ElectromagnetismGame();
  }

  void _onReset() {
    _game.resetGame();
  }

  void _exitToLessonDashboard() {
    _game.resetGame();
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(
        builder: (_) => const LessonsDashboard(
          lessonTitle: 'Electromagnetism and Electromagnetic Induction',
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
          'Puzzle 5: Electromagnet Scrapyard',
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
                            'Drag Coil Kit to Crane Core',
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
                      'F = 0.5 × N × I',
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
                child: GameWidget<ElectromagnetismGame>(
                  game: _game,
                  overlayBuilderMap: {
                    'CalculationOverlay': (context, game) => CalculationOverlay(game: game),
                    'ResultOverlay': (context, game) => ResultOverlay(game: game),
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
