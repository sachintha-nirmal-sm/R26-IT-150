import 'dart:math';
import 'package:flutter/material.dart';
import 'package:flame/game.dart';
import 'package:flame/components.dart';
import 'package:flame/events.dart';

// =========================================================================
// 1. Enums and Data Models
// =========================================================================

enum SurfaceType {
  ice(
    name: 'Ice',
    color: Color(0xFFE0F7FA),
    description: 'Very smooth surface, low friction.',
    woodStaticMu: 0.15,
    woodKineticMu: 0.08,
    rubberStaticMu: 0.25,
    rubberKineticMu: 0.15,
  ),
  wood(
    name: 'Wood Table',
    color: Color(0xFFD7CCC8),
    description: 'Moderate texture, standard friction.',
    woodStaticMu: 0.45,
    woodKineticMu: 0.30,
    rubberStaticMu: 0.60,
    rubberKineticMu: 0.45,
  ),
  sandpaper(
    name: 'Sandpaper',
    color: Color(0xFF455A64),
    description: 'Rough surface, high friction.',
    woodStaticMu: 0.75,
    woodKineticMu: 0.55,
    rubberStaticMu: 0.90,
    rubberKineticMu: 0.70,
  );

  final String name;
  final Color color;
  final String description;
  final double woodStaticMu;
  final double woodKineticMu;
  final double rubberStaticMu;
  final double rubberKineticMu;

  const SurfaceType({
    required this.name,
    required this.color,
    required this.description,
    required this.woodStaticMu,
    required this.woodKineticMu,
    required this.rubberStaticMu,
    required this.rubberKineticMu,
  });
}

enum BlockMaterial {
  wood(
    name: 'Wood Block',
    color: Color(0xFF5D4037),
    labelColor: Color(0xFFD7CCC8),
  ),
  rubber(
    name: 'Rubber Block',
    color: Color(0xFF212121),
    labelColor: Color(0xFFB0BEC5),
  );

  final String name;
  final Color color;
  final Color labelColor;

  const BlockMaterial({
    required this.name,
    required this.color,
    required this.labelColor,
  });
}

enum FrictionState {
  static(name: 'Static Friction', color: Colors.blueAccent),
  limiting(name: 'Limiting Friction', color: Colors.orangeAccent),
  kinetic(name: 'Kinetic Friction', color: Color(0xFF10B981));

  final String name;
  final Color color;

  const FrictionState({required this.name, required this.color});
}

// =========================================================================
// 2. Flame Game Class
// =========================================================================

class FrictionPhysicsGame extends FlameGame with DragCallbacks {
  // Simulation Variables
  double appliedForce = 0.0; // In Newtons (0 to 100)
  SurfaceType surfaceType = SurfaceType.ice;
  BlockMaterial blockMaterial = BlockMaterial.wood;

  final double blockMass = 10.0; // In kg
  final double gravity = 9.8; // In m/s^2
  
  double velocity = 0.0; // In m/s
  double acceleration = 0.0; // In m/s^2
  double frictionForce = 0.0; // In Newtons
  
  double blockX = 150.0; // Horizontal position
  double elapsedLimitTime = 0.0; // Timer for shaking animation

  // Notifiers for Flutter UI (performance optimized)
  final appliedForceNotifier = ValueNotifier<double>(0.0);
  final frictionForceNotifier = ValueNotifier<double>(0.0);
  final velocityNotifier = ValueNotifier<double>(0.0);
  final accelerationNotifier = ValueNotifier<double>(0.0);
  final frictionStateNotifier = ValueNotifier<FrictionState>(FrictionState.static);

  // Components
  late BlockComponent blockComponent;
  late SurfaceComponent surfaceComponent;

  // Drag State
  bool isDraggingBlock = false;

  // Calculations Helper
  double get normalForce => blockMass * gravity; // R = m * g = 98N
  
  double get muS {
    return blockMaterial == BlockMaterial.wood 
        ? surfaceType.woodStaticMu 
        : surfaceType.rubberStaticMu;
  }

  double get muK {
    return blockMaterial == BlockMaterial.wood 
        ? surfaceType.woodKineticMu 
        : surfaceType.rubberKineticMu;
  }

  double get maxStaticFriction => muS * normalForce; // F_max = mu_s * R
  double get kineticFriction => muK * normalForce; // F_k = mu_k * R

  // Responsive ground Y position based on game height
  double get groundY {
    if (size.y > 750) {
      return size.y * 0.58;
    } else if (size.y > 600) {
      return size.y * 0.50;
    } else {
      return size.y * 0.42;
    }
  }

  @override
  Future<void> onLoad() async {
    super.onLoad();

    // Add Surface
    surfaceComponent = SurfaceComponent();
    add(surfaceComponent);

    // Add Block
    blockComponent = BlockComponent();
    add(blockComponent);
  }

  @override
  void update(double dt) {
    super.update(dt);

    if (isDraggingBlock) {
      // If manually dragging, hold velocity and update friction to match dragging direction visually
      velocity = 0;
      acceleration = 0;
      frictionForce = appliedForce.clamp(0, maxStaticFriction);
      frictionStateNotifier.value = FrictionState.static;
      
      // Update Notifiers
      velocityNotifier.value = velocity;
      accelerationNotifier.value = acceleration;
      frictionForceNotifier.value = frictionForce;
      return;
    }

    final fMax = maxStaticFriction;
    final fK = kineticFriction;

    // Check states
    if (velocity == 0.0) {
      // The block is stationary
      // Check if applied force is near F_max (within 0.5N tolerance for the limiting state visualization)
      if ((appliedForce - fMax).abs() <= 0.5) {
        frictionForce = fMax;
        acceleration = 0;
        frictionStateNotifier.value = FrictionState.limiting;
        elapsedLimitTime += dt;
      } else if (appliedForce < fMax) {
        // Static friction matches applied force exactly
        frictionForce = appliedForce;
        acceleration = 0;
        frictionStateNotifier.value = FrictionState.static;
        elapsedLimitTime = 0;
      } else {
        // Applied force breaks static friction! Transitions to Kinetic
        frictionForce = fK;
        acceleration = (appliedForce - fK) / blockMass;
        velocity += acceleration * dt;
        frictionStateNotifier.value = FrictionState.kinetic;
        elapsedLimitTime = 0;
      }
    } else {
      // The block is in motion (Kinetic state)
      frictionForce = fK;
      frictionStateNotifier.value = FrictionState.kinetic;
      elapsedLimitTime = 0;

      // Net Force = Applied Force - Friction Force
      // (Applied force points right (+), kinetic friction points left (-))
      double netForce = appliedForce - fK;
      acceleration = netForce / blockMass;
      
      velocity += acceleration * dt;

      // If velocity drops below zero, block stops and resets to static/limiting state
      if (velocity <= 0.0) {
        velocity = 0.0;
        acceleration = 0.0;
      }
    }

    // Update block horizontal position if moving
    if (velocity > 0.0) {
      blockX += velocity * dt * 100.0; // scale factor to translate m/s to pixels/s

      // Reset block position if it goes off-screen
      if (blockX > size.x) {
        resetBlockPosition();
      }
    }

    // Update block position component coordinates
    blockComponent.position.x = blockX;
    
    // Notify UI variables
    appliedForceNotifier.value = appliedForce;
    frictionForceNotifier.value = frictionForce;
    velocityNotifier.value = velocity;
    accelerationNotifier.value = acceleration;
  }

  void resetBlockPosition() {
    blockX = 150.0;
    velocity = 0.0;
    acceleration = 0.0;
  }

  void changeSurface(SurfaceType type) {
    surfaceType = type;
    resetBlockPosition();
  }

  void changeBlockMaterial(BlockMaterial material) {
    blockMaterial = material;
    resetBlockPosition();
  }

  void resetSimulation() {
    appliedForce = 0.0;
    resetBlockPosition();
    frictionStateNotifier.value = FrictionState.static;
  }
}

// =========================================================================
// 3. Surface Flame Component
// =========================================================================

class SurfaceComponent extends PositionComponent with HasGameRef<FrictionPhysicsGame> {
  @override
  void onGameResize(Vector2 size) {
    super.onGameResize(size);
    // Dynamically position based on responsive groundY
    position = Vector2(0, game.groundY);
    this.size = Vector2(size.x, size.y - game.groundY);
  }

  @override
  void render(Canvas canvas) {
    final rect = size.toRect();
    final surfacePaint = Paint()..color = game.surfaceType.color;
    canvas.drawRect(rect, surfacePaint);

    // Draw grid table lines or pattern to make surface look detailed and scientific
    final linePaint = Paint()
      ..color = Colors.black.withOpacity(0.15)
      ..strokeWidth = 2.0;

    // Draw horizontal texture highlights
    if (game.surfaceType == SurfaceType.ice) {
      // Ice shine lines
      final shinePaint = Paint()
        ..color = Colors.white.withOpacity(0.6)
        ..strokeWidth = 3.0;
      canvas.drawLine(Offset(0, 10), Offset(size.x, 15), shinePaint);
      canvas.drawLine(Offset(0, 45), Offset(size.x, 50), shinePaint);
      canvas.drawLine(Offset(0, 80), Offset(size.x, 83), shinePaint);
    } else if (game.surfaceType == SurfaceType.wood) {
      // Wood grain lines
      final grainPaint = Paint()
        ..color = Color(0xFF8D6E63).withOpacity(0.3)
        ..strokeWidth = 4.0;
      for (double y = 15; y < size.y; y += 30) {
        canvas.drawLine(Offset(0, y), Offset(size.x, y + 2), grainPaint);
      }
    } else if (game.surfaceType == SurfaceType.sandpaper) {
      // Sandpaper texture noise (random grains)
      final grainPaint = Paint()..color = Colors.black.withOpacity(0.25);
      final r = Random(42); // Seeded random for static noise
      for (int i = 0; i < 400; i++) {
        final x = r.nextDouble() * size.x;
        final y = r.nextDouble() * size.y;
        canvas.drawCircle(Offset(x, y), 1.5, grainPaint);
      }
    }

    // Draw table top boundary line (the sliding surface)
    final topBorderPaint = Paint()
      ..color = Colors.blueGrey.shade800
      ..strokeWidth = 4.0;
    canvas.drawLine(Offset(0, 0), Offset(size.x, 0), topBorderPaint);

    // Draw measurement marks (ticks and distance meters)
    final tickPaint = Paint()
      ..color = Colors.blueGrey.shade700
      ..strokeWidth = 2.0;
    
    for (double x = 100.0; x < size.x; x += 150.0) {
      canvas.drawLine(Offset(x, 0), Offset(x, 15), tickPaint);
      final meterVal = (x - 150.0) / 100.0;
      if (meterVal >= 0) {
        _drawTickText(canvas, '${meterVal.toStringAsFixed(1)} m', Offset(x, 20));
      }
    }
  }

  void _drawTickText(Canvas canvas, String text, Offset pos) {
    final painter = TextPainter(
      text: TextSpan(
        text: text,
        style: TextStyle(
          color: Colors.blueGrey.shade800,
          fontSize: 12,
          fontWeight: FontWeight.bold,
        ),
      ),
      textDirection: TextDirection.ltr,
    );
    painter.layout();
    painter.paint(canvas, Offset(pos.dx - painter.width / 2, pos.dy));
  }
}

// =========================================================================
// 4. Block Flame Component
// =========================================================================

class BlockComponent extends PositionComponent with DragCallbacks, HasGameRef<FrictionPhysicsGame> {
  double spawnTimer = 0.0;

  BlockComponent() {
    size = Vector2(110.0, 70.0);
    anchor = Anchor.bottomLeft; // align sitting on surface easily
  }

  @override
  void onGameResize(Vector2 size) {
    super.onGameResize(size);
    // Align sitting exactly on the dynamic sliding surface
    position = Vector2(game.blockX, game.groundY);
  }

  @override
  void update(double dt) {
    super.update(dt);

    // Particles effect in kinetic state
    if (game.frictionStateNotifier.value == FrictionState.kinetic && game.velocity > 0) {
      spawnTimer += dt;
      if (spawnTimer >= 0.04) {
        spawnTimer = 0;
        final pColor = game.surfaceType == SurfaceType.ice 
            ? Colors.cyan.shade100 
            : game.surfaceType == SurfaceType.wood 
                ? Color(0xFFA1887F) 
                : Colors.grey.shade400;

        // Spawn a dust particle behind the block's sliding bottom edge
        game.add(
          FrictionParticle(
            position: Vector2(position.x + 5, position.y - 4),
            velocity: Vector2(
              -game.velocity * 40 - (15.0 + Random().nextDouble() * 35.0),
              -(10.0 + Random().nextDouble() * 20.0),
            ),
            maxLife: 0.4 + Random().nextDouble() * 0.3,
            color: pColor,
          ),
        );
      }
    }
  }

  // Handle Dragging
  @override
  void onDragStart(DragStartEvent event) {
    super.onDragStart(event);
    game.isDraggingBlock = true;
  }

  @override
  void onDragUpdate(DragUpdateEvent event) {
    // Move block horizontally within boundaries
    game.blockX = (game.blockX + event.localDelta.x).clamp(20.0, game.size.x - size.x - 20.0);
  }

  @override
  void onDragEnd(DragEndEvent event) {
    super.onDragEnd(event);
    game.isDraggingBlock = false;
  }

  @override
  void render(Canvas canvas) {
    canvas.save();

    // 1. Vibration Shaking when in Limiting State
    if (game.frictionStateNotifier.value == FrictionState.limiting) {
      final shake = sin(game.elapsedLimitTime * 70.0) * 2.2;
      canvas.translate(shake, 0);
    }

    // 2. Draw Block Rect
    final rect = size.toRect();
    final blockPaint = Paint()
      ..color = game.blockMaterial.color
      ..style = PaintingStyle.fill;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, Radius.circular(8.0)), blockPaint);

    // Draw Block Border
    final borderPaint = Paint()
      ..color = Colors.black.withOpacity(0.7)
      ..strokeWidth = 3.0
      ..style = PaintingStyle.stroke;
    canvas.drawRRect(RRect.fromRectAndRadius(rect, Radius.circular(8.0)), borderPaint);

    // 3. Draw Block Details (texture or grain)
    final labelStyle = TextStyle(
      color: game.blockMaterial.labelColor,
      fontSize: 12.0,
      fontWeight: FontWeight.bold,
      fontFamily: 'Poppins',
    );
    
    // Draw wood lines or rubber grid inside block
    if (game.blockMaterial == BlockMaterial.wood) {
      final detailPaint = Paint()
        ..color = Colors.black.withOpacity(0.18)
        ..strokeWidth = 2.0;
      canvas.drawLine(Offset(10, 15), Offset(size.x - 10, 15), detailPaint);
      canvas.drawLine(Offset(15, 35), Offset(size.x - 15, 35), detailPaint);
      canvas.drawLine(Offset(10, 55), Offset(size.x - 10, 55), detailPaint);
    } else {
      // Rubber cross pattern
      final detailPaint = Paint()
        ..color = Colors.white.withOpacity(0.1)
        ..strokeWidth = 1.5;
      for (double x = 10; x < size.x; x += 20) {
        canvas.drawLine(Offset(x, 5), Offset(x + 10, size.y - 5), detailPaint);
      }
    }

    // Draw Block Material Name inside the block
    final textPainter = TextPainter(
      text: TextSpan(text: game.blockMaterial.name, style: labelStyle),
      textDirection: TextDirection.ltr,
    );
    textPainter.layout();
    textPainter.paint(
      canvas,
      Offset((size.x - textPainter.width) / 2, (size.y - textPainter.height) / 2),
    );

    // Draw a small steel loop/hook on the right side of the block
    final hookPaint = Paint()
      ..color = Colors.blueGrey.shade300
      ..strokeWidth = 3.0
      ..style = PaintingStyle.stroke;
    canvas.drawArc(
      Rect.fromCenter(center: Offset(size.x + 3.0, size.y / 2), width: 12.0, height: 16.0),
      -pi / 2,
      pi,
      false,
      hookPaint,
    );

    // 4. Force Vectors (Applied Force Green, Friction Force Red)
    // Scale length based on force values
    final double maxArrowLength = size.x * 1.5;
    final double forceScale = maxArrowLength / 100.0; // 100N maps to max arrow length

    // Draw Applied Force Arrow (Right)
    if (game.appliedForce > 0) {
      final double appArrowLen = (game.appliedForce * forceScale).clamp(15.0, maxArrowLength);
      final start = Offset(size.x + 9, size.y / 2);
      final end = Offset(start.dx + appArrowLen, start.dy);
      
      _drawArrow(canvas, start, end, Colors.greenAccent.shade700);
      _drawText(
        canvas,
        'Fa: ${game.appliedForce.toStringAsFixed(1)} N',
        Offset(start.dx + appArrowLen / 2, start.dy - 20),
        Colors.greenAccent.shade700,
        alignCenter: true,
      );
    }

    // Draw Friction Force Arrow (Left)
    if (game.frictionForce > 0) {
      final double fricArrowLen = (game.frictionForce * forceScale).clamp(15.0, maxArrowLength);
      final start = Offset(-3, size.y / 2);
      final end = Offset(start.dx - fricArrowLen, start.dy);

      _drawArrow(canvas, start, end, Colors.redAccent.shade700);
      _drawText(
        canvas,
        'Ff: ${game.frictionForce.toStringAsFixed(1)} N',
        Offset(start.dx - fricArrowLen / 2, start.dy - 20),
        Colors.redAccent.shade700,
        alignCenter: true,
      );
    }

    canvas.restore();
  }

  // Draw arrow shaft and a solid arrowhead triangle
  void _drawArrow(Canvas canvas, Offset start, Offset end, Color color) {
    final double headSize = 12.0;
    
    final paint = Paint()
      ..color = color
      ..strokeWidth = 4.5
      ..style = PaintingStyle.stroke
      ..strokeCap = StrokeCap.round;

    // Draw shaft line
    canvas.drawLine(start, end, paint);

    // Calculate normal / direction vector
    final dx = end.dx - start.dx;
    final dy = end.dy - start.dy;
    final len = sqrt(dx * dx + dy * dy);
    if (len < 0.5) return;
    
    final ux = dx / len;
    final uy = dy / len;
    
    final px = -uy;
    final py = ux;

    // Draw arrowhead triangle path
    final headPaint = Paint()
      ..color = color
      ..style = PaintingStyle.fill;
    
    final path = Path();
    path.moveTo(end.dx, end.dy);
    path.lineTo(
      end.dx - headSize * ux + (headSize / 1.7) * px,
      end.dy - headSize * uy + (headSize / 1.7) * py,
    );
    path.lineTo(
      end.dx - headSize * ux - (headSize / 1.7) * px,
      end.dy - headSize * uy - (headSize / 1.7) * py,
    );
    path.close();
    canvas.drawPath(path, headPaint);
  }

  void _drawText(Canvas canvas, String text, Offset position, Color color, {bool alignCenter = false}) {
    final textPainter = TextPainter(
      text: TextSpan(
        text: text,
        style: TextStyle(
          color: color,
          fontSize: 13,
          fontWeight: FontWeight.bold,
          fontFamily: 'Poppins',
          shadows: [
            Shadow(
              color: Colors.black.withOpacity(0.7),
              offset: const Offset(1, 1.5),
              blurRadius: 3,
            ),
          ],
        ),
      ),
      textDirection: TextDirection.ltr,
    );
    textPainter.layout();
    final offset = alignCenter 
        ? Offset(position.dx - textPainter.width / 2, position.dy - textPainter.height / 2)
        : position;
    textPainter.paint(canvas, offset);
  }
}

// =========================================================================
// 5. Sliding Dust Particle Component
// =========================================================================

class FrictionParticle extends PositionComponent {
  final Vector2 velocity;
  final double maxLife;
  double life = 0.0;
  final Color color;

  FrictionParticle({
    required Vector2 position,
    required this.velocity,
    required this.maxLife,
    required this.color,
  }) : super(position: position, size: Vector2.all(3.0 + Random().nextDouble() * 5.0));

  @override
  void update(double dt) {
    super.update(dt);
    life += dt;
    // Move particle
    position += velocity * dt;

    if (life >= maxLife) {
      removeFromParent();
    }
  }

  @override
  void render(Canvas canvas) {
    final double progress = (life / maxLife).clamp(0.0, 1.0);
    final double opacity = 1.0 - progress;
    
    final paint = Paint()
      ..color = color.withOpacity(opacity)
      ..style = PaintingStyle.fill;
    
    canvas.drawCircle(Offset(size.x / 2, size.y / 2), size.x / 2, paint);
  }
}

// =========================================================================
// 6. Flutter Screen Widget with Interactive Controls
// =========================================================================

class FrictionGameScreen extends StatefulWidget {
  const FrictionGameScreen({super.key});

  @override
  State<FrictionGameScreen> createState() => _FrictionGameScreenState();
}

class _FrictionGameScreenState extends State<FrictionGameScreen> {
  late FrictionPhysicsGame game;

  @override
  void initState() {
    super.initState();
    game = FrictionPhysicsGame();
  }

  @override
  void dispose() {
    game.appliedForceNotifier.dispose();
    game.frictionForceNotifier.dispose();
    game.velocityNotifier.dispose();
    game.accelerationNotifier.dispose();
    game.frictionStateNotifier.dispose();
    super.dispose();
  }

  void _showTheoryDialog() {
    final double screenWidth = MediaQuery.of(context).size.width;
    final bool isSmallScreen = screenWidth < 380;

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        backgroundColor: const Color(0xFF1E293B),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16.0)),
        insetPadding: EdgeInsets.symmetric(
          horizontal: isSmallScreen ? 16.0 : 24.0,
          vertical: 24.0,
        ),
        title: Row(
          children: [
            Icon(Icons.menu_book, color: Colors.blueAccent, size: isSmallScreen ? 22 : 26),
            const SizedBox(width: 8),
            Expanded(
              child: Text(
                "Friction Physics Guide",
                style: TextStyle(
                  color: Colors.white,
                  fontWeight: FontWeight.bold,
                  fontSize: isSmallScreen ? 15.0 : 18.0,
                ),
              ),
            ),
          ],
        ),
        content: SizedBox(
          width: screenWidth - (isSmallScreen ? 32 : 48),
          child: SingleChildScrollView(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisSize: MainAxisSize.min,
              children: [
                _buildTheorySection(
                  title: "1. Static Friction (Fs)",
                  desc: "The friction that opposes initial motion. As you apply force, the friction force matches it exactly (Ff = Fapplied), keeping the block stationary.",
                  isSmallScreen: isSmallScreen,
                ),
                _buildTheorySection(
                  title: "2. Limiting Friction (Fmax)",
                  desc: "The absolute maximum value of static friction (Fmax = μs · R). This is the threshold point where the block is on the verge of sliding. You will see the block shake visually at this limit!",
                  isSmallScreen: isSmallScreen,
                ),
                _buildTheorySection(
                  title: "3. Kinetic Friction (Fk)",
                  desc: "Once the applied force exceeds Limiting Friction (Fapplied > Fmax), the block breaks free. The friction instantly drops to kinetic friction (Fk = μk · R), which is constant and lower than static friction. The net force accelerates the block.",
                  isSmallScreen: isSmallScreen,
                ),
                Divider(color: Colors.white24, height: isSmallScreen ? 16 : 24),
                Text(
                  "Key Formulas:",
                  style: TextStyle(
                    color: Colors.white,
                    fontWeight: FontWeight.bold,
                    fontSize: isSmallScreen ? 13.0 : 14.0,
                  ),
                ),
                const SizedBox(height: 6),
                Text(
                  "• Normal Reaction (R) = m · g = 10kg · 9.8m/s² = 98 N\n"
                  "• Max Static Friction (Fmax) = μs · R\n"
                  "• Kinetic Friction (Fk) = μk · R\n"
                  "• Net Force (Fnet) = Fapplied - Fk\n"
                  "• Acceleration (a) = Fnet / m",
                  style: TextStyle(
                    color: const Color(0xFFCBD5E1),
                    fontSize: isSmallScreen ? 11.5 : 12.5,
                    height: 1.45,
                  ),
                ),
              ],
            ),
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: Text(
              "Got it!",
              style: TextStyle(
                color: Colors.blueAccent,
                fontWeight: FontWeight.bold,
                fontSize: isSmallScreen ? 13.0 : 14.0,
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildTheorySection({
    required String title,
    required String desc,
    required bool isSmallScreen,
  }) {
    return Padding(
      padding: EdgeInsets.only(bottom: isSmallScreen ? 8.0 : 12.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            title,
            style: TextStyle(
              color: Colors.white,
              fontWeight: FontWeight.bold,
              fontSize: isSmallScreen ? 12.5 : 13.5,
            ),
          ),
          const SizedBox(height: 3),
          Text(
            desc,
            style: TextStyle(
              color: const Color(0xFF94A3B8),
              fontSize: isSmallScreen ? 11.5 : 12.5,
              height: 1.35,
            ),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final double screenWidth = MediaQuery.of(context).size.width;
    final bool isCompact = screenWidth < 550;
    final double maxStatic = game.maxStaticFriction;

    return Scaffold(
      backgroundColor: const Color(0xFF0F172A),
      body: Stack(
        children: [
          // 1. Flame Game Widget
          Positioned.fill(
            child: GameWidget(game: game),
          ),

          // 2. Top Header Overlay (Glassmorphism design)
          Positioned(
            top: 0,
            left: 16.0,
            right: 16.0,
            child: SafeArea(
              bottom: false,
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 12.0),
                decoration: BoxDecoration(
                  color: const Color(0xFF1E293B).withOpacity(0.85),
                  borderRadius: BorderRadius.circular(16.0),
                  border: Border.all(color: Colors.white.withOpacity(0.12), width: 1.5),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withOpacity(0.2),
                      blurRadius: 10,
                      offset: const Offset(0, 4),
                    )
                  ],
                ),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    IconButton(
                      onPressed: () => Navigator.maybePop(context),
                      icon: const Icon(Icons.arrow_back_ios_new, color: Colors.white, size: 20),
                    ),
                    Expanded(
                      child: Column(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          const Text(
                            "THE FRICTION LAB",
                            style: TextStyle(
                              color: Colors.white,
                              fontSize: 16.0,
                              fontWeight: FontWeight.w800,
                              letterSpacing: 1.2,
                            ),
                          ),
                          Text(
                            "Drag block or slide force to experiment",
                            textAlign: TextAlign.center,
                            style: TextStyle(
                              color: Colors.grey.shade400,
                              fontSize: 10.0,
                            ),
                          ),
                        ],
                      ),
                    ),
                    IconButton(
                      onPressed: _showTheoryDialog,
                      icon: const Icon(Icons.info_outline, color: Colors.blueAccent, size: 24),
                    ),
                  ],
                ),
              ),
            ),
          ),

          // 3. Floating Bottom Controls & Dashboard Panel
          Positioned(
            bottom: 0,
            left: 16.0,
            right: 16.0,
            child: SafeArea(
              top: false,
              child: Padding(
                padding: const EdgeInsets.only(bottom: 16.0),
                child: Container(
                  padding: const EdgeInsets.all(16.0),
                  decoration: BoxDecoration(
                    color: const Color(0xFF1E293B).withOpacity(0.92),
                    borderRadius: BorderRadius.circular(20.0),
                    border: Border.all(color: Colors.white.withOpacity(0.15), width: 1.5),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withOpacity(0.4),
                        blurRadius: 15,
                        offset: const Offset(0, -5),
                      )
                    ],
                  ),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      // Selectors (Material & Surface)
                      if (isCompact) ...[
                        _buildCompactSelectorRow(
                          label: "BLOCK:",
                          chips: BlockMaterial.values.map((mat) {
                            final isSelected = game.blockMaterial == mat;
                            return ChoiceChip(
                              label: Text(mat.name.split(' ')[0]),
                              selected: isSelected,
                              backgroundColor: const Color(0xFF0F172A),
                              selectedColor: Colors.blueAccent.shade700,
                              labelStyle: TextStyle(
                                color: isSelected ? Colors.white : Colors.blueGrey.shade200,
                                fontSize: 10.5,
                                fontWeight: FontWeight.bold,
                              ),
                              padding: const EdgeInsets.symmetric(horizontal: 8.0, vertical: 4.0),
                              materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
                              onSelected: (selected) {
                                if (selected) {
                                  setState(() {
                                    game.changeBlockMaterial(mat);
                                  });
                                }
                              },
                            );
                          }).toList(),
                        ),
                        const SizedBox(height: 6),
                        _buildCompactSelectorRow(
                          label: "SURFACE:",
                          chips: SurfaceType.values.map((surf) {
                            final isSelected = game.surfaceType == surf;
                            return ChoiceChip(
                              label: Text(surf.name.split(' ')[0]),
                              selected: isSelected,
                              backgroundColor: const Color(0xFF0F172A),
                              selectedColor: Colors.blueAccent.shade700,
                              labelStyle: TextStyle(
                                color: isSelected ? Colors.white : Colors.blueGrey.shade200,
                                fontSize: 10.5,
                                fontWeight: FontWeight.bold,
                              ),
                              padding: const EdgeInsets.symmetric(horizontal: 8.0, vertical: 4.0),
                              materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
                              onSelected: (selected) {
                                if (selected) {
                                  setState(() {
                                    game.changeSurface(surf);
                                  });
                                }
                              },
                            );
                          }).toList(),
                        ),
                      ] else ...[
                        Row(
                          children: [
                            // Material Selector
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  const Text(
                                    "BLOCK MATERIAL",
                                    style: TextStyle(color: Colors.grey, fontSize: 10.0, fontWeight: FontWeight.bold),
                                  ),
                                  const SizedBox(height: 6),
                                  Row(
                                    children: BlockMaterial.values.map((mat) {
                                      final isSelected = game.blockMaterial == mat;
                                      return Expanded(
                                        child: Padding(
                                          padding: const EdgeInsets.symmetric(horizontal: 2.0),
                                          child: ChoiceChip(
                                            label: Text(mat.name.split(' ')[0]),
                                            selected: isSelected,
                                            backgroundColor: const Color(0xFF0F172A),
                                            selectedColor: Colors.blueAccent.shade700,
                                            labelStyle: TextStyle(
                                              color: isSelected ? Colors.white : Colors.blueGrey.shade200,
                                              fontSize: 11.0,
                                              fontWeight: FontWeight.bold,
                                            ),
                                            padding: EdgeInsets.zero,
                                            materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
                                            onSelected: (selected) {
                                              if (selected) {
                                                setState(() {
                                                  game.changeBlockMaterial(mat);
                                                });
                                              }
                                            },
                                          ),
                                        ),
                                      );
                                    }).toList(),
                                  )
                                ],
                              ),
                            ),
                            const SizedBox(width: 12),
                            // Surface Selector
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  const Text(
                                    "SURFACE AREA",
                                    style: TextStyle(color: Colors.grey, fontSize: 10.0, fontWeight: FontWeight.bold),
                                  ),
                                  const SizedBox(height: 6),
                                  Row(
                                    children: SurfaceType.values.map((surf) {
                                      final isSelected = game.surfaceType == surf;
                                      return Expanded(
                                        child: Padding(
                                          padding: const EdgeInsets.symmetric(horizontal: 2.0),
                                          child: ChoiceChip(
                                            label: Text(surf.name.split(' ')[0]),
                                            selected: isSelected,
                                            backgroundColor: const Color(0xFF0F172A),
                                            selectedColor: Colors.blueAccent.shade700,
                                            labelStyle: TextStyle(
                                              color: isSelected ? Colors.white : Colors.blueGrey.shade200,
                                              fontSize: 11.0,
                                              fontWeight: FontWeight.bold,
                                            ),
                                            padding: EdgeInsets.zero,
                                            materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
                                            onSelected: (selected) {
                                              if (selected) {
                                                setState(() {
                                                  game.changeSurface(surf);
                                                });
                                              }
                                            },
                                          ),
                                        ),
                                      );
                                    }).toList(),
                                  )
                                ],
                              ),
                            ),
                          ],
                        ),
                      ],
                      const SizedBox(height: 12),

                      // Applied Force Slider & Custom Threshold Tick
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              const Text(
                                "APPLIED FORCE",
                                style: TextStyle(color: Colors.grey, fontSize: 10.0, fontWeight: FontWeight.bold),
                              ),
                              ValueListenableBuilder<double>(
                                valueListenable: game.appliedForceNotifier,
                                builder: (context, value, _) {
                                  return Text(
                                    "${value.toStringAsFixed(1)} N",
                                    style: const TextStyle(
                                      color: Colors.greenAccent,
                                      fontSize: 14.0,
                                      fontWeight: FontWeight.bold,
                                    ),
                                  );
                                },
                              ),
                            ],
                          ),
                          LayoutBuilder(
                            builder: (context, constraints) {
                              final width = constraints.maxWidth;
                              // Calculate position percentage for F_max line
                              final ratio = maxStatic / 100.0;
                              final double paddingOffset = 22.0; // padding inside slider component
                              final linePosition = (width - paddingOffset * 2) * ratio + paddingOffset;

                              return Stack(
                                clipBehavior: Clip.none,
                                alignment: Alignment.center,
                                children: [
                                  // Slider
                                  ValueListenableBuilder<double>(
                                    valueListenable: game.appliedForceNotifier,
                                    builder: (context, value, _) {
                                      return SliderTheme(
                                        data: SliderTheme.of(context).copyWith(
                                          activeTrackColor: Colors.greenAccent,
                                          inactiveTrackColor: const Color(0xFF0F172A),
                                          thumbColor: Colors.white,
                                          overlayColor: Colors.greenAccent.withOpacity(0.12),
                                          trackHeight: 6.0,
                                          thumbShape: const RoundSliderThumbShape(enabledThumbRadius: 9.0),
                                        ),
                                        child: Slider(
                                          value: value,
                                          min: 0.0,
                                          max: 100.0,
                                          onChanged: (newVal) {
                                            double snappedVal = newVal;
                                            if ((newVal - maxStatic).abs() <= 1.5) {
                                              snappedVal = maxStatic;
                                            }
                                            setState(() {
                                              game.appliedForce = snappedVal;
                                            });
                                          },
                                        ),
                                      );
                                    },
                                  ),

                                  // Threshold vertical mark indicating F_max
                                  Positioned(
                                    left: linePosition,
                                    top: -8.0,
                                    child: Column(
                                      mainAxisSize: MainAxisSize.min,
                                      children: [
                                        Container(
                                          width: 2.2,
                                          height: 22.0,
                                          color: Colors.orangeAccent,
                                        ),
                                        const SizedBox(height: 2),
                                        Text(
                                          "Limit: ${maxStatic.toStringAsFixed(1)}N",
                                          style: const TextStyle(
                                            color: Colors.orangeAccent,
                                            fontSize: 9.0,
                                            fontWeight: FontWeight.w800,
                                          ),
                                        )
                                      ],
                                    ),
                                  ),
                                ],
                              );
                            },
                          ),
                        ],
                      ),
                      const SizedBox(height: 10),

                      // Real-time HUD Dashboard
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 12.0, vertical: 10.0),
                        decoration: BoxDecoration(
                          color: const Color(0xFF0F172A),
                          borderRadius: BorderRadius.circular(12.0),
                        ),
                        child: Row(
                          mainAxisAlignment: MainAxisAlignment.spaceAround,
                          children: [
                            _buildHudItem(
                              label: isCompact ? "FRICTION" : "FRICTION FORCE",
                              valueStream: game.frictionForceNotifier,
                              suffix: " N",
                              valueColor: Colors.redAccent,
                              compact: isCompact,
                            ),
                            _buildHudItem(
                              label: "VELOCITY",
                              valueStream: game.velocityNotifier,
                              suffix: " m/s",
                              valueColor: Colors.cyanAccent,
                              compact: isCompact,
                            ),
                            _buildHudItem(
                              label: isCompact ? "ACCEL." : "ACCELERATION",
                              valueStream: game.accelerationNotifier,
                              suffix: " m/s²",
                              valueColor: Colors.purpleAccent,
                              compact: isCompact,
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: 12),

                      // Bottom State Badge & Action Buttons
                      if (isCompact) ...[
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            _buildStateBadge(),
                            _buildLimitForceButton(compact: true),
                          ],
                        ),
                        const SizedBox(height: 8),
                        SizedBox(
                          width: double.infinity,
                          child: _buildResetButton(compact: true),
                        ),
                      ] else ...[
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            _buildStateBadge(),
                            Row(
                              children: [
                                _buildLimitForceButton(compact: false),
                                const SizedBox(width: 8),
                                _buildResetButton(compact: false),
                              ],
                            ),
                          ],
                        ),
                      ],
                    ],
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildCompactSelectorRow({required String label, required List<Widget> chips}) {
    return Row(
      children: [
        SizedBox(
          width: 70,
          child: Text(
            label,
            style: const TextStyle(
              color: Colors.grey,
              fontSize: 10.0,
              fontWeight: FontWeight.bold,
              letterSpacing: 0.5,
            ),
          ),
        ),
        Expanded(
          child: Wrap(
            spacing: 8.0,
            runSpacing: 4.0,
            children: chips,
          ),
        ),
      ],
    );
  }

  Widget _buildStateBadge() {
    return ValueListenableBuilder<FrictionState>(
      valueListenable: game.frictionStateNotifier,
      builder: (context, state, _) {
        return Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              width: 9,
              height: 9,
              decoration: BoxDecoration(
                color: state.color,
                shape: BoxShape.circle,
                boxShadow: [
                  BoxShadow(color: state.color.withOpacity(0.5), blurRadius: 5, spreadRadius: 1)
                ],
              ),
            ),
            const SizedBox(width: 6),
            Text(
              state.name.toUpperCase(),
              style: TextStyle(
                color: state.color,
                fontWeight: FontWeight.w900,
                fontSize: 11.0,
                letterSpacing: 0.5,
              ),
            ),
          ],
        );
      },
    );
  }

  Widget _buildLimitForceButton({required bool compact}) {
    final double maxStatic = game.maxStaticFriction;
    return TextButton.icon(
      style: TextButton.styleFrom(
        foregroundColor: Colors.orangeAccent,
        padding: EdgeInsets.symmetric(horizontal: compact ? 8 : 12, vertical: 4),
        tapTargetSize: MaterialTapTargetSize.shrinkWrap,
      ),
      onPressed: () {
        setState(() {
          game.appliedForce = maxStatic;
        });
      },
      icon: const Icon(Icons.flash_on, size: 14),
      label: Text(
        "Limit Force",
        style: TextStyle(
          fontSize: compact ? 10.5 : 12.0,
          fontWeight: FontWeight.bold,
        ),
      ),
    );
  }

  Widget _buildResetButton({required bool compact}) {
    return ElevatedButton.icon(
      style: ElevatedButton.styleFrom(
        backgroundColor: Colors.red.shade900,
        foregroundColor: Colors.white,
        padding: EdgeInsets.symmetric(horizontal: compact ? 12 : 16, vertical: compact ? 8 : 10),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10.0)),
        elevation: 2,
      ),
      onPressed: () {
        setState(() {
          game.resetSimulation();
        });
      },
      icon: const Icon(Icons.refresh, size: 14),
      label: Text(
        "Reset Lab",
        style: TextStyle(
          fontSize: compact ? 11.5 : 13.0,
          fontWeight: FontWeight.bold,
        ),
      ),
    );
  }

  Widget _buildHudItem({
    required String label,
    required ValueNotifier<double> valueStream,
    required String suffix,
    required Color valueColor,
    required bool compact,
  }) {
    return Column(
      children: [
        Text(
          label,
          style: TextStyle(
            color: Colors.blueGrey,
            fontSize: compact ? 8.0 : 9.5,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 4),
        ValueListenableBuilder<double>(
          valueListenable: valueStream,
          builder: (context, value, _) {
            return Text(
              "${value.toStringAsFixed(2)}$suffix",
              style: TextStyle(
                color: valueColor,
                fontSize: compact ? 12.0 : 13.5,
                fontWeight: FontWeight.bold,
                fontFamily: 'monospace',
              ),
            );
          },
        ),
      ],
    );
  }
}
