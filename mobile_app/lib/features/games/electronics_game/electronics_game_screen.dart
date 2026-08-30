import '../lesson_id_helper.dart';
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

enum ElectronicsComponentType { resistor, ldr, transistor }

// =========================================================================
// 2. FLAME GAME ENGINE
// =========================================================================

class ElectronicsGame extends FlameGame with DragCallbacks {
  late CircuitBoard circuitBoard;
  late SlotComponent slotA; // Resistor slot
  late SlotComponent slotB; // LDR slot
  late SlotComponent slotC; // Transistor slot

  late DraggableComponent resistor;
  late DraggableComponent ldr;
  late DraggableComponent transistor;

  late ParticleCurrent particleCurrent;

  DraggableComponent? pendingCheckpointComponent;

  bool _isLoaded = false;
  bool isLiveMode = false;
  bool isMissionComplete = false;
  double nightLevel = 0.0; // 0.0 = Day, 1.0 = Night

  // Mobile-responsive circuit layout coordinates
  double leftX = 0.0;
  double midX = 0.0;
  double rightX = 0.0;
  double topY = 0.0;
  double botY = 0.0;
  double centerY = 0.0;

  @override
  Future<void> onLoad() async {
    circuitBoard = CircuitBoard();
    add(circuitBoard);

    slotA = SlotComponent(
      type: ElectronicsComponentType.resistor,
      slotLabel: 'Slot A (Series)',
      position: Vector2(100, 100),
    );
    slotB = SlotComponent(
      type: ElectronicsComponentType.ldr,
      slotLabel: 'Slot B (Sensor)',
      position: Vector2(200, 100),
    );
    slotC = SlotComponent(
      type: ElectronicsComponentType.transistor,
      slotLabel: 'Slot C (Switch)',
      position: Vector2(300, 100),
    );

    resistor = DraggableComponent(
      type: ElectronicsComponentType.resistor,
      label: 'Resistor',
      subText: '700 Ω',
      baseColor: const Color(0xFF2563EB),
      startPosition: Vector2(100, 450),
      size: Vector2(75, 45),
    );

    ldr = DraggableComponent(
      type: ElectronicsComponentType.ldr,
      label: 'LDR',
      subText: 'Sensor',
      baseColor: const Color(0xFFD97706),
      startPosition: Vector2(200, 450),
      size: Vector2(60, 60),
    );

    transistor = DraggableComponent(
      type: ElectronicsComponentType.transistor,
      label: 'Transistor',
      subText: 'NPN',
      baseColor: const Color(0xFF1E293B),
      startPosition: Vector2(300, 450),
      size: Vector2(65, 65),
    );

    particleCurrent = ParticleCurrent();

    add(slotA);
    add(slotB);
    add(slotC);
    add(resistor);
    add(ldr);
    add(transistor);
    add(particleCurrent);

    _isLoaded = true;
  }

  @override
  void onGameResize(Vector2 size) {
    super.onGameResize(size);
    if (!_isLoaded) return;

    final centerX = size.x / 2;
    centerY = size.y / 2 - 25; // Center slightly higher to clear bottom mobile dock

    // Calculate Mobile-Friendly Coordinates
    leftX = max(42.0, size.x * 0.13);
    midX = size.x * 0.44;
    rightX = min(size.x - 42.0, size.x * 0.86);
    topY = max(55.0, centerY - 135.0);
    botY = min(size.y - 165.0, centerY + 125.0);

    // Layout slots cleanly on the circuit diagram
    slotA.position = Vector2((leftX + rightX) / 2 + 15, topY);
    slotB.position = Vector2(midX, (topY + botY) / 2);
    slotC.position = Vector2(rightX, (topY + botY) / 2 + 25);

    // Initial positions for actors at the bottom of the screen (Mobile Dock)
    final spacing = min(size.x / 3.4, 110.0);
    final bottomY = size.y - 60;
    resistor.startPosition.setValues(centerX - spacing, bottomY);
    ldr.startPosition.setValues(centerX, bottomY);
    transistor.startPosition.setValues(centerX + spacing, bottomY);

    if (!resistor.isSnapped) resistor.position.setFrom(resistor.startPosition);
    if (!ldr.isSnapped) ldr.position.setFrom(ldr.startPosition);
    if (!transistor.isSnapped) transistor.position.setFrom(transistor.startPosition);
  }

  void onComponentSnapped(DraggableComponent component) {
    pendingCheckpointComponent = component;
    pauseEngine();
    overlays.add('TheoryOverlay');
  }

  void checkAllPlaced() {
    if (resistor.isCorrectlyPlaced && ldr.isCorrectlyPlaced && transistor.isCorrectlyPlaced) {
      activateLiveMode();
    }
  }

  void activateLiveMode() {
    isLiveMode = true;
    overlays.add('LiveControlOverlay');
  }

  void resetGame() {
    isLiveMode = false;
    isMissionComplete = false;
    nightLevel = 0.0;
    pendingCheckpointComponent = null;

    resistor.reset();
    ldr.reset();
    transistor.reset();

    onGameResize(size);

    overlays.remove('TheoryOverlay');
    overlays.remove('LiveControlOverlay');
    overlays.remove('SuccessOverlay');

    resumeEngine();
  }

  @override
  void update(double dt) {
    super.update(dt);
  }
}

// =========================================================================
// 3. FLAME CANVAS COMPONENTS
// =========================================================================

class CircuitBoard extends PositionComponent with HasGameReference<ElectronicsGame> {
  CircuitBoard() : super(priority: -10);

  @override
  void onGameResize(Vector2 size) {
    super.onGameResize(size);
    this.size = size;
  }

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);

    // 1. Dynamic Background Color: Day (Dark Slate) to Night (Deep Midnight)
    final dayColor = const Color(0xFF0F172A);
    final nightColor = const Color(0xFF020617);
    final currentBg = Color.lerp(dayColor, nightColor, game.nightLevel)!;
    canvas.drawRect(rect, Paint()..color = currentBg);

    // Background Grid
    final gridPaint = Paint()
      ..color = Colors.cyanAccent.withOpacity(0.03)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 1.0;
    for (double x = 0; x < size.x; x += 36) {
      canvas.drawLine(Offset(x, 0), Offset(x, size.y), gridPaint);
    }
    for (double y = 0; y < size.y; y += 36) {
      canvas.drawLine(Offset(0, y), Offset(size.x, y), gridPaint);
    }

    final leftX = game.leftX;
    final midX = game.midX;
    final rightX = game.rightX;
    final topY = game.topY;
    final botY = game.botY;
    final centerY = game.centerY;

    // Circuit Line Wireframe Paint
    final wirePaint = Paint()
      ..color = Colors.cyanAccent.withOpacity(0.75)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 3.0;

    // 2. Draw Wireframe Circuit Lines
    // Top Power Line (+9V)
    const batH = 76.0;
    final batTopY = centerY - batH / 2;
    final batBotY = centerY + batH / 2;

    // (+) Wire from Battery top to top line
    canvas.drawLine(Offset(leftX, batTopY - 8), Offset(leftX, topY), wirePaint);
    canvas.drawLine(Offset(leftX, topY), Offset(rightX, topY), wirePaint);

    // LDR Sensor Left Branch
    canvas.drawLine(Offset(midX, topY), Offset(midX, centerY - 25), wirePaint);
    canvas.drawLine(Offset(midX, centerY + 25), Offset(midX, botY), wirePaint);

    // Transistor Base Connection
    canvas.drawLine(Offset(midX, centerY), Offset(rightX - 35, centerY), wirePaint);

    // Right Branch (Series Resistor + LED + Transistor C/E)
    canvas.drawLine(Offset(rightX, topY), Offset(rightX, botY), wirePaint);

    // Bottom Return Line (0V Ground)
    canvas.drawLine(Offset(leftX, botY), Offset(rightX, botY), wirePaint);
    // (-) Wire from Battery bottom to bottom line
    canvas.drawLine(Offset(leftX, batBotY), Offset(leftX, botY), wirePaint);

    // 3. Render High-Detail Mobile-Optimized 9V Battery
    const batW = 46.0;
    final batRect = Rect.fromCenter(center: Offset(leftX, centerY), width: batW, height: batH);
    final batRRect = RRect.fromRectAndRadius(batRect, const Radius.circular(6));

    // Battery Main Dark Body
    canvas.drawRRect(batRRect, Paint()..color = const Color(0xFF1E293B));

    // Metallic Gold Top Header Band
    final goldRect = Rect.fromLTWH(leftX - batW / 2, batTopY, batW, 20);
    final goldGradient = const LinearGradient(
      colors: [Color(0xFFF59E0B), Color(0xFFD97706), Color(0xFFB45309)],
    ).createShader(goldRect);
    canvas.drawRRect(
      RRect.fromRectAndCorners(
        goldRect,
        topLeft: const Radius.circular(6),
        topRight: const Radius.circular(6),
      ),
      Paint()..shader = goldGradient,
    );

    // Metallic Silver Bottom Foot Strip
    final silverRect = Rect.fromLTWH(leftX - batW / 2, batBotY - 8, batW, 8);
    canvas.drawRRect(
      RRect.fromRectAndCorners(
        silverRect,
        bottomLeft: const Radius.circular(6),
        bottomRight: const Radius.circular(6),
      ),
      Paint()..color = const Color(0xFF94A3B8),
    );

    // Outer Chrome Border
    canvas.drawRRect(
      batRRect,
      Paint()
        ..color = const Color(0xFFCBD5E1)
        ..style = PaintingStyle.stroke
        ..strokeWidth = 1.5,
    );

    // Top Snap Terminals (+ and -)
    // Positive (+) Male Snap Terminal (Left Top)
    final posTermCenter = Offset(leftX - 11, batTopY - 4);
    canvas.drawCircle(posTermCenter, 5.5, Paint()..color = const Color(0xFFEF4444));
    canvas.drawCircle(posTermCenter, 2.5, Paint()..color = Colors.white);

    // Negative (-) Female Snap Terminal (Right Top)
    final negTermCenter = Offset(leftX + 11, batTopY - 4);
    canvas.drawCircle(negTermCenter, 6.0, Paint()..color = const Color(0xFF64748B));
    canvas.drawCircle(negTermCenter, 3.5, Paint()..color = const Color(0xFF0F172A));

    // Polarity Signs (+ / -)
    final posSign = TextPainter(
      text: const TextSpan(text: '+', style: TextStyle(color: Colors.redAccent, fontSize: 10, fontWeight: FontWeight.bold)),
      textDirection: TextDirection.ltr,
    )..layout();
    posSign.paint(canvas, Offset(leftX - 14, batTopY + 2));

    final negSign = TextPainter(
      text: const TextSpan(text: '-', style: TextStyle(color: Colors.white70, fontSize: 12, fontWeight: FontWeight.bold)),
      textDirection: TextDirection.ltr,
    )..layout();
    negSign.paint(canvas, Offset(leftX + 8, batTopY + 1));

    // Battery Text Label
    final batText = TextPainter(
      text: const TextSpan(
        text: '9V\nBATTERY',
        style: TextStyle(color: Colors.white, fontSize: 9, fontWeight: FontWeight.bold, height: 1.15),
      ),
      textDirection: TextDirection.ltr,
      textAlign: TextAlign.center,
    )..layout();
    batText.paint(canvas, Offset(leftX - batText.width / 2, centerY + 4));

    // 4. Render LED (Light Emitting Diode)
    final ledCenter = Offset(rightX, topY + 42);
    const ledRadius = 20.0;

    // LED Glow Aura in Live Mode when night falls
    if (game.isLiveMode && game.nightLevel > 0.0) {
      final glowRadius = ledRadius + (game.nightLevel * 24.0);
      final glowPaint = Paint()
        ..color = const Color(0xFFFFEA00).withOpacity(game.nightLevel * 0.65)
        ..maskFilter = MaskFilter.blur(BlurStyle.normal, 12.0 * game.nightLevel);
      canvas.drawCircle(ledCenter, glowRadius, glowPaint);
    }

    // LED Body
    final baseLedColor = Color.lerp(const Color(0xFF64748B), const Color(0xFFFFEA00), game.nightLevel)!;
    canvas.drawCircle(ledCenter, ledRadius, Paint()..color = baseLedColor);
    canvas.drawCircle(
      ledCenter,
      ledRadius,
      Paint()
        ..color = Colors.white.withOpacity(0.85)
        ..style = PaintingStyle.stroke
        ..strokeWidth = 2.0,
    );

    // Diode Symbol inside LED
    final symbolPaint = Paint()
      ..color = Colors.black87
      ..style = PaintingStyle.stroke
      ..strokeWidth = 2.0;
    final path = Path()
      ..moveTo(ledCenter.dx - 7, ledCenter.dy - 7)
      ..lineTo(ledCenter.dx - 7, ledCenter.dy + 7)
      ..lineTo(ledCenter.dx + 5, ledCenter.dy)
      ..close();
    canvas.drawPath(path, symbolPaint);
    canvas.drawLine(Offset(ledCenter.dx + 5, ledCenter.dy - 7), Offset(ledCenter.dx + 5, ledCenter.dy + 7), symbolPaint);

    final ledLabel = TextPainter(
      text: const TextSpan(
        text: 'LED',
        style: TextStyle(color: Colors.white70, fontSize: 9, fontWeight: FontWeight.bold),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    ledLabel.paint(canvas, Offset(ledCenter.dx + 25, ledCenter.dy - 6));
  }
}

class SlotComponent extends PositionComponent {
  final ElectronicsComponentType type;
  final String slotLabel;

  SlotComponent({
    required this.type,
    required this.slotLabel,
    required Vector2 position,
  }) : super(position: position, size: Vector2(75, 45), anchor: Anchor.center);

  @override
  void render(Canvas canvas) {
    final rect = Rect.fromLTWH(0, 0, size.x, size.y);
    final rrect = RRect.fromRectAndRadius(rect, const Radius.circular(8));

    final borderPaint = Paint()
      ..color = Colors.cyanAccent.withOpacity(0.5)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 1.5;

    canvas.drawRRect(rrect, Paint()..color = const Color(0xFF1E293B).withOpacity(0.6));
    canvas.drawRRect(rrect, borderPaint);

    final textPainter = TextPainter(
      text: TextSpan(
        text: slotLabel,
        style: const TextStyle(color: Colors.white70, fontSize: 8, fontWeight: FontWeight.bold),
      ),
      textDirection: TextDirection.ltr,
      textAlign: TextAlign.center,
    )..layout();
    textPainter.paint(canvas, Offset((size.x - textPainter.width) / 2, (size.y - textPainter.height) / 2));
  }
}

class DraggableComponent extends PositionComponent
    with DragCallbacks, HasGameReference<ElectronicsGame> {
  final ElectronicsComponentType type;
  final String label;
  final String subText;
  final Color baseColor;
  final Vector2 startPosition;

  bool isDragging = false;
  bool isSnapped = false;
  bool isCorrectlyPlaced = false;

  DraggableComponent({
    required this.type,
    required this.label,
    required this.subText,
    required this.baseColor,
    required this.startPosition,
    required Vector2 size,
  }) : super(position: startPosition.clone(), size: size, anchor: Anchor.center);

  @override
  void onDragStart(DragStartEvent event) {
    if (isCorrectlyPlaced || game.isLiveMode) return;
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

    SlotComponent targetSlot;
    if (type == ElectronicsComponentType.resistor) {
      targetSlot = game.slotA;
    } else if (type == ElectronicsComponentType.ldr) {
      targetSlot = game.slotB;
    } else {
      targetSlot = game.slotC;
    }

    final distance = position.distanceTo(targetSlot.position);
    if (distance < 50) {
      isSnapped = true;
      position.setFrom(targetSlot.position);
      game.onComponentSnapped(this);
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
    isDragging = false;
    isSnapped = false;
    isCorrectlyPlaced = false;
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
    final rrect = RRect.fromRectAndRadius(rect, const Radius.circular(8));

    canvas.drawRRect(rrect, Paint()..color = baseColor);

    if (!isCorrectlyPlaced) {
      canvas.drawRRect(
        rrect,
        Paint()
          ..color = Colors.amberAccent.withOpacity(0.7)
          ..style = PaintingStyle.stroke
          ..strokeWidth = 2.0,
      );
    }

    if (type == ElectronicsComponentType.resistor) {
      canvas.drawRect(Rect.fromLTWH(14, 0, 5, size.y), Paint()..color = Colors.brown);
      canvas.drawRect(Rect.fromLTWH(26, 0, 5, size.y), Paint()..color = Colors.black);
      canvas.drawRect(Rect.fromLTWH(38, 0, 5, size.y), Paint()..color = Colors.red);
      canvas.drawRect(Rect.fromLTWH(56, 0, 5, size.y), Paint()..color = Colors.amber);
    } else if (type == ElectronicsComponentType.ldr) {
      final trackPaint = Paint()
        ..color = Colors.redAccent
        ..style = PaintingStyle.stroke
        ..strokeWidth = 2.0;
      final path = Path()
        ..moveTo(12, size.y / 2)
        ..lineTo(20, size.y / 2 - 8)
        ..lineTo(30, size.y / 2 + 8)
        ..lineTo(40, size.y / 2 - 8)
        ..lineTo(48, size.y / 2);
      canvas.drawPath(path, trackPaint);
    } else if (type == ElectronicsComponentType.transistor) {
      final leadPaint = Paint()..color = Colors.white70;
      canvas.drawRect(Rect.fromLTWH(8, size.y - 5, 7, 3), leadPaint);
      canvas.drawRect(Rect.fromLTWH(size.x / 2 - 3.5, size.y - 5, 7, 3), leadPaint);
      canvas.drawRect(Rect.fromLTWH(size.x - 15, size.y - 5, 7, 3), leadPaint);
    }

    final textPainter = TextPainter(
      text: TextSpan(
        text: '$label\n$subText',
        style: const TextStyle(
          color: Colors.white,
          fontSize: 8.5,
          fontWeight: FontWeight.bold,
          height: 1.15,
        ),
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
      Paint()..color = Colors.black.withOpacity(0.55),
    );

    textPainter.paint(
      canvas,
      Offset((size.x - textPainter.width) / 2, (size.y - textPainter.height) / 2),
    );
  }
}

class ParticleCurrent extends PositionComponent with HasGameReference<ElectronicsGame> {
  double particleTimer = 0.0;

  @override
  void update(double dt) {
    super.update(dt);
    if (game.isLiveMode && game.nightLevel > 0.0) {
      particleTimer += dt * (3.0 + game.nightLevel * 7.0);
    }
  }

  @override
  void render(Canvas canvas) {
    if (!game.isLiveMode || game.nightLevel <= 0.0) return;

    final leftX = game.leftX;
    final rightX = game.rightX;
    final topY = game.topY;
    final botY = game.botY;

    final particlePaint = Paint()..color = Colors.amberAccent;

    for (int i = 0; i < 12; i++) {
      final progress = ((particleTimer + i * 0.8) % 10.0) / 10.0;
      Offset pt;
      if (progress < 0.25) {
        final t = progress / 0.25;
        pt = Offset(lerpDouble(leftX, rightX, t)!, topY);
      } else if (progress < 0.50) {
        final t = (progress - 0.25) / 0.25;
        pt = Offset(rightX, lerpDouble(topY, botY, t)!);
      } else if (progress < 0.75) {
        final t = (progress - 0.50) / 0.25;
        pt = Offset(lerpDouble(rightX, leftX, t)!, botY);
      } else {
        final t = (progress - 0.75) / 0.25;
        pt = Offset(leftX, lerpDouble(botY, topY, t)!);
      }
      canvas.drawCircle(pt, 3.5, particlePaint);
    }
  }
}

// =========================================================================
// 4. FLUTTER OVERLAYS (THEORY CHECKPOINT & SUCCESS)
// =========================================================================

class TheoryOverlay extends StatefulWidget {
  final ElectronicsGame game;

  const TheoryOverlay({super.key, required this.game});

  @override
  State<TheoryOverlay> createState() => _TheoryOverlayState();
}

class _TheoryOverlayState extends State<TheoryOverlay>
    with SingleTickerProviderStateMixin {
  final TextEditingController _numericController = TextEditingController();
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
    _numericController.dispose();
    _animationController.dispose();
    super.dispose();
  }

  void _onAnswerSubmitted(bool isCorrect) {
    if (isCorrect) {
      final comp = widget.game.pendingCheckpointComponent;
      if (comp != null) {
        comp.isCorrectlyPlaced = true;
      }
      widget.game.overlays.remove('TheoryOverlay');
      widget.game.resumeEngine();
      widget.game.checkAllPlaced();
    } else {
      setState(() {
        _showError = true;
      });
      _animationController.forward(from: 0);
    }
  }

  @override
  Widget build(BuildContext context) {
    final comp = widget.game.pendingCheckpointComponent;
    if (comp == null) return const SizedBox.shrink();

    Widget questionWidget;
    String hintText = '';

    if (comp.type == ElectronicsComponentType.resistor) {
      hintText = 'Hint: R = V / I. Use voltage difference: (9V - 2V) / 0.01A = 700 Ω';
      questionWidget = Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Battery is 9V. LED needs 2V and 0.01A (10mA) to light up safely. Calculate the required series resistance in Ohms (Ω):',
            style: TextStyle(color: Color(0xFFCBD5E1), fontSize: 13, height: 1.4),
          ),
          const SizedBox(height: 14),
          TextField(
            controller: _numericController,
            keyboardType: TextInputType.number,
            style: const TextStyle(color: Colors.white),
            decoration: InputDecoration(
              hintText: 'Enter resistance in Ω (e.g. 700)',
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
          const SizedBox(height: 16),
          SizedBox(
            width: double.infinity,
            height: 44,
            child: ElevatedButton(
              onPressed: () {
                final val = double.tryParse(_numericController.text.trim());
                _onAnswerSubmitted(val != null && (val - 700).abs() < 5);
              },
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.cyanAccent,
                foregroundColor: const Color(0xFF0F172A),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
              ),
              child: const Text('Submit Calculation', style: TextStyle(fontWeight: FontWeight.bold)),
            ),
          ),
        ],
      );
    } else if (comp.type == ElectronicsComponentType.ldr) {
      hintText = 'Hint: LDR (Light Dependent Resistor) resistance INCREASES in dark environments.';
      questionWidget = Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'When it gets dark (light intensity decreases), what happens to the LDR\'s resistance?',
            style: TextStyle(color: Color(0xFFCBD5E1), fontSize: 13, height: 1.4),
          ),
          const SizedBox(height: 18),
          Row(
            children: [
              Expanded(
                child: ElevatedButton(
                  onPressed: () => _onAnswerSubmitted(true),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.amberAccent,
                    foregroundColor: const Color(0xFF0F172A),
                    padding: const EdgeInsets.symmetric(vertical: 12),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                  ),
                  child: const Text('Increases', style: TextStyle(fontWeight: FontWeight.bold)),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: ElevatedButton(
                  onPressed: () => _onAnswerSubmitted(false),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: const Color(0xFF334155),
                    foregroundColor: Colors.white,
                    padding: const EdgeInsets.symmetric(vertical: 12),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                  ),
                  child: const Text('Decreases', style: TextStyle(fontWeight: FontWeight.bold)),
                ),
              ),
            ],
          ),
        ],
      );
    } else {
      hintText = 'Hint: The Transistor turns ON when current enters its BASE terminal.';
      questionWidget = Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'To turn this NPN Transistor ON (acting as a closed switch), which terminal needs a small positive voltage/current?',
            style: TextStyle(color: Color(0xFFCBD5E1), fontSize: 13, height: 1.4),
          ),
          const SizedBox(height: 18),
          Row(
            children: [
              Expanded(
                child: ElevatedButton(
                  onPressed: () => _onAnswerSubmitted(true),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.cyanAccent,
                    foregroundColor: const Color(0xFF0F172A),
                    padding: const EdgeInsets.symmetric(vertical: 12),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                  ),
                  child: const Text('Base', style: TextStyle(fontWeight: FontWeight.bold)),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: ElevatedButton(
                  onPressed: () => _onAnswerSubmitted(false),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: const Color(0xFF334155),
                    foregroundColor: Colors.white,
                    padding: const EdgeInsets.symmetric(vertical: 12),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                  ),
                  child: const Text('Collector', style: TextStyle(fontWeight: FontWeight.bold)),
                ),
              ),
            ],
          ),
        ],
      );
    }

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
                          child: const Icon(Icons.memory, color: Colors.cyanAccent, size: 26),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Text(
                            '${comp.label} Theory Checkpoint',
                            style: const TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: Colors.white),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 16),
                    questionWidget,
                    if (_showError) ...[
                      const SizedBox(height: 14),
                      Container(
                        padding: const EdgeInsets.all(10),
                        decoration: BoxDecoration(
                          color: Colors.redAccent.withOpacity(0.1),
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(color: Colors.redAccent.withOpacity(0.3)),
                        ),
                        child: Row(
                          children: [
                            const Icon(Icons.lightbulb_outline, color: Colors.redAccent, size: 16),
                            const SizedBox(width: 8),
                            Expanded(
                              child: Text(
                                hintText,
                                style: const TextStyle(
                                  color: Colors.redAccent,
                                  fontSize: 11,
                                  fontWeight: FontWeight.bold,
                                  height: 1.3,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
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

class LiveControlOverlay extends StatefulWidget {
  final ElectronicsGame game;

  const LiveControlOverlay({super.key, required this.game});

  @override
  State<LiveControlOverlay> createState() => _LiveControlOverlayState();
}

class _LiveControlOverlayState extends State<LiveControlOverlay> {
  double _currentSliderVal = 0.0;

  @override
  Widget build(BuildContext context) {
    return Align(
      alignment: Alignment.bottomCenter,
      child: Container(
        margin: const EdgeInsets.fromLTRB(16, 0, 16, 20),
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
        decoration: BoxDecoration(
          color: const Color(0xFF1E293B).withOpacity(0.92),
          borderRadius: BorderRadius.circular(16),
          border: Border.all(color: Colors.amberAccent.withOpacity(0.5), width: 1.5),
          boxShadow: [
            BoxShadow(color: Colors.black.withOpacity(0.4), blurRadius: 15),
          ],
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Row(
                  children: const [
                    Icon(Icons.wb_sunny, color: Colors.amberAccent, size: 18),
                    SizedBox(width: 6),
                    Text('Day (Light)', style: TextStyle(color: Colors.white70, fontSize: 11, fontWeight: FontWeight.bold)),
                  ],
                ),
                const Text(
                  'Circuit Live Mode: Adjust Light',
                  style: TextStyle(color: Colors.amberAccent, fontSize: 11, fontWeight: FontWeight.bold),
                ),
                Row(
                  children: const [
                    Text('Night (Dark)', style: TextStyle(color: Colors.white70, fontSize: 11, fontWeight: FontWeight.bold)),
                    SizedBox(width: 6),
                    Icon(Icons.nights_stay, color: Colors.cyanAccent, size: 18),
                  ],
                ),
              ],
            ),
            Slider(
              value: _currentSliderVal,
              min: 0.0,
              max: 1.0,
              activeColor: Colors.amberAccent,
              inactiveColor: const Color(0xFF334155),
              onChanged: (val) {
                setState(() {
                  _currentSliderVal = val;
                });
                widget.game.nightLevel = val;
                if (val >= 0.98 && !widget.game.isMissionComplete) {
                  widget.game.isMissionComplete = true;
                  widget.game.overlays.add('SuccessOverlay');
                }
              },
            ),
          ],
        ),
      ),
    );
  }
}

class SuccessOverlay extends StatelessWidget {
  final ElectronicsGame game;

  const SuccessOverlay({super.key, required this.game});

  @override
  Widget build(BuildContext context) {
    const amberColor = Color(0xFFF59E0B);
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
                border: Border.all(color: amberColor.withOpacity(0.6), width: 2.5),
                boxShadow: [
                  BoxShadow(color: amberColor.withOpacity(0.2), blurRadius: 25, spreadRadius: 2),
                ],
              ),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: amberColor.withOpacity(0.15),
                      shape: BoxShape.circle,
                    ),
                    child: const Icon(Icons.lightbulb_sharp, color: amberColor, size: 64),
                  ),
                  const SizedBox(height: 16),
                  const Text(
                    'Mission Complete!',
                    style: TextStyle(color: Colors.white, fontSize: 22, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 14),
                  const Text(
                    'In the dark, LDR resistance increases, sending current to the Transistor\'s Base. '
                    'The Transistor switches ON, completing the circuit and lighting the LED!',
                    textAlign: TextAlign.center,
                    style: TextStyle(color: Color(0xFFCBD5E1), fontSize: 14, height: 1.5),
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
                        backgroundColor: amberColor,
                        foregroundColor: const Color(0xFF0F172A),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                        elevation: 4,
                      ),
                      child: const Text('Restart Lab', style: TextStyle(fontSize: 15, fontWeight: FontWeight.bold)),
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

class ElectronicsGameScreen extends StatefulWidget {
  const ElectronicsGameScreen({super.key});

  @override
  State<ElectronicsGameScreen> createState() => _ElectronicsGameScreenState();
}

class _ElectronicsGameScreenState extends State<ElectronicsGameScreen> {
  late final ElectronicsGame _game;

  @override
  void initState() {
    super.initState();
    _game = ElectronicsGame();
  }

  void _onReset() {
    _game.resetGame();
  }

  void _exitToLessonDashboard() {
    _game.resetGame();
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(
        builder: (_) => LessonsDashboard(
          lessonId: LessonIdHelper.getLessonId('Electronics & Logic Gates'),
          lessonTitle: 'Electronics',
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
          'Puzzle 4: The Smart Night Light',
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
                            'Drag Components to Circuit Slots',
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
                      'V = I·R | Transistor Switch',
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
                child: GameWidget<ElectronicsGame>(
                  game: _game,
                  overlayBuilderMap: {
                    'TheoryOverlay': (context, game) => TheoryOverlay(game: game),
                    'LiveControlOverlay': (context, game) => LiveControlOverlay(game: game),
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
