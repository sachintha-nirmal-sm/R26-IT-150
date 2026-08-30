import 'package:flame/components.dart' show Vector2;

// ---------------------------------------------------------------------------
// Surface Type
// ---------------------------------------------------------------------------

enum SurfaceType {
  normal,
  rough,
  ice,
}

extension SurfaceTypeExt on SurfaceType {
  String get label {
    switch (this) {
      case SurfaceType.normal:
        return 'Normal';
      case SurfaceType.rough:
        return 'Rough Floor';
      case SurfaceType.ice:
        return 'Ice Floor';
    }
  }

  /// Friction deceleration (pixels/sec²). Higher value = stops faster.
  double get frictionDecel {
    switch (this) {
      case SurfaceType.normal:
        return 60.0;
      case SurfaceType.rough:
        return 200.0;
      case SurfaceType.ice:
        return 10.0;
    }
  }
}

// ---------------------------------------------------------------------------
// Friction Zone Data
// ---------------------------------------------------------------------------

class FrictionZoneData {
  const FrictionZoneData({
    required this.position,
    required this.size,
    required this.surfaceType,
  });

  final Vector2 position;
  final Vector2 size;
  final SurfaceType surfaceType;
}

// ---------------------------------------------------------------------------
// Level Configuration
// ---------------------------------------------------------------------------

class LevelConfig {
  const LevelConfig({
    required this.levelName,
    required this.levelNumber,
    required this.crateStartPosition,
    required this.targetPosition,
    required this.robotStartPosition,
    this.crateInitialVelocity,
    this.frictionZones = const [],
    this.defaultSurface = SurfaceType.normal,
    this.maxForceMagnitude = 500.0,
    this.crateMass = 8.0,
    this.instructionText = '',
    this.showFrictionArrow = false,
  });

  final String levelName;
  final int levelNumber;
  final Vector2 crateStartPosition;
  final Vector2 targetPosition;
  final Vector2 robotStartPosition;
  final Vector2? crateInitialVelocity;
  final List<FrictionZoneData> frictionZones;
  final SurfaceType defaultSurface;
  final double maxForceMagnitude;
  final double crateMass;
  final String instructionText;
  final bool showFrictionArrow;
}

// ---------------------------------------------------------------------------
// Level Definitions
// ---------------------------------------------------------------------------

class LevelConfigs {
  LevelConfigs._();

  static LevelConfig get level1 => LevelConfig(
        levelName: 'Push & Pull',
        levelNumber: 1,
        crateStartPosition: Vector2(200, 430),
        targetPosition: Vector2(200, 130),
        robotStartPosition: Vector2(200, 540),
        defaultSurface: SurfaceType.normal,
        maxForceMagnitude: 500.0,
        crateMass: 8.0,
        instructionText: 'Drag on the crate to push it to the ★ target!',
        showFrictionArrow: false,
      );

  static LevelConfig get level2 => LevelConfig(
        levelName: 'Effects of Force',
        levelNumber: 2,
        // Crate starts centre-left, moving RIGHT horizontally
        crateStartPosition: Vector2(60, 350),
        targetPosition: Vector2(320, 200),
        robotStartPosition: Vector2(60, 490),
        // Initial velocity: sliding RIGHT across the screen
        crateInitialVelocity: Vector2(120, 0),
        defaultSurface: SurfaceType.normal,
        maxForceMagnitude: 600.0,
        crateMass: 10.0,
        instructionText: 'Crate is sliding → Stop it! Then push UP-RIGHT to ★',
        showFrictionArrow: false,
      );

  static LevelConfig get level3 {
    return LevelConfig(
      levelName: 'Friction',
      levelNumber: 3,
      crateStartPosition: Vector2(200, 470),
      targetPosition: Vector2(200, 80),
      robotStartPosition: Vector2(200, 570),
      defaultSurface: SurfaceType.ice,
      maxForceMagnitude: 550.0,
      crateMass: 8.0,
      instructionText: 'Watch how friction changes on different surfaces!',
      showFrictionArrow: true,
      frictionZones: [
        FrictionZoneData(
          position: Vector2(0, 0),
          size: Vector2(400, 335),
          surfaceType: SurfaceType.rough,
        ),
        FrictionZoneData(
          position: Vector2(0, 335),
          size: Vector2(400, 365),
          surfaceType: SurfaceType.ice,
        ),
      ],
    );
  }

  static LevelConfig forLevel(int levelNumber) {
    switch (levelNumber) {
      case 1:
        return level1;
      case 2:
        return level2;
      case 3:
        return level3;
      default:
        return level1;
    }
  }
}
