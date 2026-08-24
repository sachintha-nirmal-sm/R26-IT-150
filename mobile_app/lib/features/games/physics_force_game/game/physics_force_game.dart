import 'dart:math' as math;

import 'package:flame/events.dart';
import 'package:flame/game.dart';
import 'package:flutter/painting.dart';

import '../components/crate_component.dart';
import '../components/floor_zone_component.dart';
import '../components/force_arrow_component.dart';
import '../components/friction_arrow_component.dart';
import '../components/robot_component.dart';
import '../components/target_zone_component.dart';
import 'game_state.dart';
import 'level_config.dart';

// ---------------------------------------------------------------------------
// PhysicsForceGame — Fully rewritten for reliability and clarity
// ---------------------------------------------------------------------------

/// KEY ARCHITECTURE DECISIONS (v2):
///
/// 1. NO CameraComponent — all components are added directly to the game
///    (not `world`). This means touch event coordinates (`event.localPosition`)
///    map 1:1 with component positions. No scaling/offset issues.
///
/// 2. DRAG-TO-PUSH mechanic — the player touches the crate and drags in the
///    direction they want to push. The drag vector becomes the force direction.
///    This is intuitive: drag UP = push crate UP.
///
/// 3. Game canvas is sized from `size` getter (real screen size). Positions
///    in [LevelConfig] are defined for a 400×700 reference canvas and are
///    rescaled at load time to fit the actual device screen.
///
/// 4. Theory popup fires after the crate has moved ≥ 40 px from its start,
///    not immediately — so students see the movement first.
class PhysicsForceGame extends FlameGame with DragCallbacks, TapCallbacks {
  PhysicsForceGame({required this.stateController});

  final GameStateController stateController;

  // ── Component references ────────────────────────────────────────────────
  late CrateComponent _crate;
  late RobotComponent _robot;
  late TargetZoneComponent _targetZone;
  late ForceArrowComponent _forceArrow;
  FrictionArrowComponent? _frictionArrow;

  // ── Input / drag state ──────────────────────────────────────────────────
  bool _isDragging = false;

  /// Current drag start position (where finger first touched the crate).
  Vector2 _dragStart = Vector2.zero();

  /// Current drag position (updated as finger moves).
  Vector2 _dragCurrent = Vector2.zero();

  /// Set to true for this frame when the player tries to drag in the
  /// forbidden direction during Level-2 Phase-1 (dragging forward/rightward
  /// instead of left to stop the crate). Used to flash a warning hint.
  bool _level2ForbiddenDragWarning = false;
  double _level2WarningTimer = 0.0;

  // ── Level config ────────────────────────────────────────────────────────
  /// Current level config (null before first resize).
  LevelConfig? _configOrNull;
  LevelConfig get _config => _configOrNull!;

  /// Scale factor applied to convert 400×700 reference positions to actual screen.
  double _scaleX = 1.0;
  double _scaleY = 1.0;

  // ── Level-1 state ───────────────────────────────────────────────────────
  bool _level1TheoryShown = false;
  Vector2 _crateStartPos = Vector2.zero();

  // ── Level-2 state ───────────────────────────────────────────────────────
  /// Phase 1: crate is still sliding — player must stop it.
  bool _level2CrateStopped = false;
  /// Theory popup has fired after the crate first stopped.
  bool _level2TheoryShown = false;
  /// Phase 2: crate has been re-pushed after stopping.
  bool _level2RepushStarted = false;
  Vector2 _level2OriginalDir = Vector2.zero();

  // ── Level-3 state ───────────────────────────────────────────────────────
  bool _level3TheoryShown = false;

  // ── Overlay keys ────────────────────────────────────────────────────────
  static const String overlayHud = 'hud';
  static const String overlayTheory = 'theoryPopup';
  static const String overlayQuiz = 'quizDialog';
  static const String overlayLevelComplete = 'levelComplete';

  // ── Touch detection radius ──────────────────────────────────────────────
  /// Radius around the crate centre that counts as "touching the crate".
  static const double _touchRadius = 70.0;

  // ===========================================================================
  // Flame Lifecycle
  // ===========================================================================

  @override
  Color backgroundColor() => const Color(0xFF0D1B2A);

  @override
  Future<void> onLoad() async {
    await super.onLoad();
    // onLoad is called after the game has a size — but size may be zero here.
    // We defer level loading to onGameResize which fires when size is known.
  }

  @override
  void onGameResize(Vector2 size) {
    super.onGameResize(size);
    if (size.x > 0 && size.y > 0) {
      _computeScale(size);
      if (_configOrNull == null) {
        _loadLevel();
      }
    }
  }

  void _computeScale(Vector2 gameSize) {
    _scaleX = gameSize.x / 400.0;
    _scaleY = gameSize.y / 700.0;
  }

  /// Converts a reference-canvas position (400×700) to actual screen position.
  Vector2 _scale(Vector2 ref) =>
      Vector2(ref.x * _scaleX, ref.y * _scaleY);

  // ===========================================================================
  // Level Management
  // ===========================================================================

  void _loadLevel() {
    removeAll(children);
    overlays.remove(overlayHud);
    overlays.remove(overlayTheory);
    overlays.remove(overlayQuiz);
    overlays.remove(overlayLevelComplete);

    final levelNum = stateController.currentLevel.index + 1;
    _configOrNull = LevelConfigs.forLevel(levelNum);

    // Reset flags.
    _level1TheoryShown = false;
    _level2CrateStopped = false;
    _level2TheoryShown = false;
    _level2RepushStarted = false;
    _level3TheoryShown = false;
    _level2OriginalDir = Vector2.zero();
    _level2ForbiddenDragWarning = false;
    _level2WarningTimer = 0.0;
    _isDragging = false;

    // Reset drag vectors so there are no stale positions from the previous session.
    _dragStart = Vector2.zero();
    _dragCurrent = Vector2.zero();

    // Reset the animated hint arrow timer so it restarts from the beginning.
    _hintAnimTime = 0.0;

    // Scaled positions.
    final cratePos = _scale(_config.crateStartPosition);
    final targetPos = _scale(_config.targetPosition);
    final robotPos = _scale(_config.robotStartPosition);
    _crateStartPos = cratePos.clone();

    // ── Background floor zones ────────────────────────────────────────────
    if (_config.frictionZones.isEmpty) {
      add(FloorZoneComponent(
        data: FrictionZoneData(
          position: Vector2.zero(),
          size: size.clone(),
          surfaceType: _config.defaultSurface,
        ),
      ));
    } else {
      for (final zone in _config.frictionZones) {
        add(FloorZoneComponent(
          data: FrictionZoneData(
            position: _scale(zone.position),
            size: _scale(zone.size),
            surfaceType: zone.surfaceType,
          ),
        ));
      }
    }

    // ── Target zone ───────────────────────────────────────────────────────
    _targetZone = TargetZoneComponent(
      center: targetPos,
      onCrateReached: _handleCrateReachedTarget,
    );
    add(_targetZone);

    // ── Crate ─────────────────────────────────────────────────────────────
    _crate = CrateComponent(
      config: _config,
      startPosition: cratePos,
      onStartedMoving: _handleCrateStartedMoving,
      onEnteredFrictionZone: _handleCrateEnteredFrictionZone,
    );
    if (_config.crateInitialVelocity != null) {
      // Scale velocity proportionally.
      _crate.velocity = Vector2(
        _config.crateInitialVelocity!.x * _scaleX,
        _config.crateInitialVelocity!.y * _scaleY,
      );
      _level2OriginalDir = _crate.velocity.normalized();
    }
    add(_crate);

    // ── Robot ─────────────────────────────────────────────────────────────
    _robot = RobotComponent(startPosition: robotPos);
    add(_robot);

    // ── Force arrow ───────────────────────────────────────────────────────
    _forceArrow = ForceArrowComponent();
    add(_forceArrow);

    // ── Friction arrow (Level 3) ──────────────────────────────────────────
    if (_config.showFrictionArrow) {
      _frictionArrow = FrictionArrowComponent();
      add(_frictionArrow!);
    } else {
      _frictionArrow = null;
    }

    // Show HUD and start playing.
    overlays.add(overlayHud);
    stateController.setPlaying();
  }


  // ── External control methods called from overlay buttons ─────────────────

  void resumeAfterOverlay() {
    overlays.remove(overlayTheory);
    overlays.remove(overlayQuiz);
    overlays.remove(overlayLevelComplete);
    stateController.setPlaying();
  }

  void loadNextLevel() {
    overlays.remove(overlayLevelComplete);
    overlays.remove(overlayTheory);
    overlays.remove(overlayQuiz);
    stateController.advanceLevel();

    // Same microtask deferral: let Flutter finish unmounting the overlay widget
    // before we call removeAll(children) and rebuild the component tree.
    Future.microtask(() {
      if (size.x > 0) _loadLevel();
    });
  }

  void restartCurrentLevel() {
    // Remove overlay immediately so the Flutter widget unmounts cleanly.
    overlays.remove(overlayLevelComplete);
    overlays.remove(overlayTheory);
    overlays.remove(overlayQuiz);
    stateController.restartLevel();

    // Defer _loadLevel() to the next microtask so the overlay widget is fully
    // disposed before we call removeAll(children) and re-add components.
    // Without this, the Level Complete overlay may still be reading
    // widget.game.currentConfig while _loadLevel() is resetting _configOrNull.
    Future.microtask(() {
      if (size.x > 0) _loadLevel();
    });
  }

  // ===========================================================================
  // Update Loop
  // ===========================================================================

  @override
  void update(double dt) {
    // Pause the physics tick when any overlay is open.
    if (stateController.isPaused) return;
    super.update(dt);

    // ── Apply drag force while finger is held ─────────────────────────────
    if (_isDragging) {
      _applyDragForce();
    } else {
      _forceArrow.hide();
    }

    // ── Friction arrow (Level 3) ──────────────────────────────────────────
    if (_frictionArrow != null) {
      if (_crate.velocity.length > 2.0) {
        _frictionArrow!.setData(
          crateCenter: _crate.position,
          crateVelocity: _crate.velocity,
          frictionDecel: _crate.currentFrictionDecel,
        );
        _frictionArrow!.isVisible = true;
      } else {
        _frictionArrow!.isVisible = false;
      }
    }

    // ── Level-2 forbidden-drag warning timer ─────────────────────────────
    if (_level2ForbiddenDragWarning) {
      _level2WarningTimer -= dt;
      if (_level2WarningTimer <= 0) {
        _level2ForbiddenDragWarning = false;
        _level2WarningTimer = 0;
      }
    }

    // ── Check Level-2 stop + theory trigger ──────────────────────────────
    if (stateController.currentLevel == GameLevel.level2EffectsOfForce) {
      _updateLevel2State();
    }

    // ── Check Level-1 theory trigger (after 40px of movement) ────────────
    if (stateController.currentLevel == GameLevel.level1PushPull &&
        !_level1TheoryShown) {
      final dist = (_crate.position - _crateStartPos).length;
      if (dist > 40) {
        _level1TheoryShown = true;
        _showTheoryOverlay();
      }
    }

    // ── Check target reached ──────────────────────────────────────────────
    _targetZone.checkCrateOverlap(_crate.position);
  }

  // ===========================================================================
  // Physics — F = ma Drag Force
  // ===========================================================================

  /// Converts the player's finger-drag into a physical Force vector, then
  /// applies the resulting acceleration to the crate each frame.
  ///
  /// ─── F = ma  step-by-step ────────────────────────────────────────────────
  ///
  ///  Step 1 — Measure the drag vector.
  ///    dragVec = dragCurrent - dragStart    (a Vector2 in screen pixels)
  ///    dragLen = |dragVec|                  (scalar length in pixels)
  ///
  ///  Step 2 — Convert drag length to Force magnitude  (F).
  ///    F = clamp(dragLen / 80, 0, 1) × maxForceMagnitude
  ///    • Dividing by 80 maps 80 px of drag → full force.
  ///    • clamped to [0 … maxForceMagnitude] so the crate can't over-accelerate.
  ///
  ///  Step 3 — Calculate acceleration  (a = F / m).
  ///    a = F / mass                        (Newton's 2nd Law: F = ma  → a = F/m)
  ///    • mass (m) comes from LevelConfig.crateMass (kg equivalent).
  ///    • Heavier crates need more force to produce the same acceleration.
  ///
  ///  Step 4 — Integrate velocity.
  ///    Δv = direction × a × dt            (per-frame velocity increment)
  ///    velocity += Δv
  ///    where dt ≈ 1/60 s  (one physics frame at 60 fps)
  ///
  ///  Summary:  F = m × a   ⟹   a = F/m   ⟹   v += a × dt
  /// ─────────────────────────────────────────────────────────────────────────
  void _applyDragForce() {
    // Step 1: Drag vector (direction + raw magnitude in px).
    final dragVec = _dragCurrent - _dragStart;
    final dragLen = dragVec.length;
    if (dragLen < 2.0) {
      _crate.appliedForceVec = Vector2.zero(); // no force — hide arrow
      return;
    }

    // Unit vector pointing in the push direction.
    final direction = dragVec.normalized();

    // ── Level-2 Phase-1: Block drags in the forward (slide) direction ──────
    //
    // During Phase 1 the crate is sliding RIGHT. The player MUST drag LEFT
    // (opposite direction) to stop it. Dragging rightward (same direction as
    // the slide) is forbidden so the student cannot skip the lesson.
    //
    // Gate: ONLY active while _level2CrateStopped is false (Phase 1).
    // Once the crate stops (_level2CrateStopped = true), this block is
    // completely bypassed and all drag directions are allowed again.
    //
    // Threshold: dot > 0.5 means the drag has a strong rightward component
    // (more than 60° aligned with the original slide). Pure UP or UP-RIGHT
    // drags score ≈ 0.5 or less so they are NOT blocked — this prevents
    // accidentally blocking the Phase-2 push-toward-target direction.
    if (stateController.currentLevel == GameLevel.level2EffectsOfForce &&
        !_level2CrateStopped &&
        _level2OriginalDir.length > 0.1) {
      final dotWithSlide = direction.dot(_level2OriginalDir);
      if (dotWithSlide > 0.5) {
        // Forbidden drag — cancel force, trigger warning flash.
        _crate.appliedForceVec = Vector2.zero();
        _forceArrow.hide();
        _level2ForbiddenDragWarning = true;
        _level2WarningTimer = 1.2; // show warning for 1.2 s
        return;
      }
    }

    // Step 2: F = clamp(dragLen / 80, 0, 1) × maxForceMagnitude
    //   This maps 80 px of drag → maximum configured force.
    final F = (dragLen / 80.0).clamp(0.0, 1.0) * _config.maxForceMagnitude;

    // Step 3: a = F / m   (Newton's Second Law)
    final m = _config.crateMass;   // mass in kg-equivalent units
    final a = F / m;               // acceleration in px/s²

    // Step 4: Δv = direction × a × dt   (integrate over one frame)
    const dt = 1 / 60.0;           // fixed 60 fps physics step
    _crate.velocity += direction * a * dt;

    // Cap speed so the crate doesn't fly off screen.
    const maxSpeed = 400.0;
    if (_crate.velocity.length > maxSpeed) {
      _crate.velocity = _crate.velocity.normalized() * maxSpeed;
    }

    // Expose the live Force vector to the crate so it can draw
    // its own green F=ma vector arrow in its render() method.
    // Arrow length ∝ F magnitude; direction = push direction.
    _crate.appliedForceVec = direction * F;

    // Show force arrow at crate centre pointing in drag direction.
    _forceArrow.show(
      origin: _crate.position,
      direction: direction,
      magnitude: F,
    );

    // Robot walks toward the touch origin (where the player is pushing from).
    _robot.moveTo(_dragStart);
  }

  // ===========================================================================
  // Input Handlers — Drag-to-Push
  // ===========================================================================

  @override
  void onDragStart(DragStartEvent event) {
    super.onDragStart(event);
    if (stateController.isPaused) return;

    final pos = event.localPosition;
    // Only start a drag if the player touches near the crate.
    if (_isTouchNearCrate(pos)) {
      _isDragging = true;
      _dragStart = pos.clone();
      _dragCurrent = pos.clone();
    }
  }

  @override
  void onDragUpdate(DragUpdateEvent event) {
    super.onDragUpdate(event);
    if (stateController.isPaused || !_isDragging) return;
    _dragCurrent = event.localStartPosition + event.localDelta;
  }

  @override
  void onDragEnd(DragEndEvent event) {
    super.onDragEnd(event);
    _stopDrag();
  }

  @override
  void onDragCancel(DragCancelEvent event) {
    super.onDragCancel(event);
    _stopDrag();
  }

  @override
  void onTapDown(TapDownEvent event) {
    // Tap-and-hold: treat tap as a drag starting at the same position.
    if (stateController.isPaused) return;
    final pos = event.localPosition;
    if (_isTouchNearCrate(pos)) {
      _isDragging = true;
      _dragStart = pos.clone();
      _dragCurrent = pos.clone();
    }
  }

  @override
  void onTapUp(TapUpEvent event) {
    _stopDrag();
  }

  void _stopDrag() {
    _isDragging = false;
    _forceArrow.hide();
    // Clear the force vector on the crate so its green F=ma arrow disappears.
    _crate.appliedForceVec = Vector2.zero();
  }

  /// Returns true if [pos] is within [_touchRadius] of the crate centre.
  bool _isTouchNearCrate(Vector2 pos) {
    return (pos - _crate.position).length < _touchRadius;
  }

  // ===========================================================================
  // Level Callbacks
  // ===========================================================================

  void _handleCrateStartedMoving() {
    // Level-2: track moving state.
    if (stateController.currentLevel == GameLevel.level2EffectsOfForce) {
      // nothing extra needed here — handled in _updateLevel2State
    }
  }

  void _handleCrateEnteredFrictionZone(SurfaceType surface) {
    if (stateController.currentLevel == GameLevel.level3Friction &&
        surface == SurfaceType.rough &&
        !_level3TheoryShown) {
      _level3TheoryShown = true;
      _showTheoryOverlay();
    }
  }

  void _handleCrateReachedTarget() {
    if (stateController.currentLevel == GameLevel.level2EffectsOfForce) {
      _showQuizOverlay();
    } else {
      _showLevelComplete();
    }
  }

  void _updateLevel2State() {
    final speed = _crate.velocity.length;

    // Phase 1: Wait for the crate to stop (speed < 4 px/s).
    if (!_level2CrateStopped && speed < 4.0) {
      _level2CrateStopped = true;
      // Fire theory popup once the crate first comes to rest.
      if (!_level2TheoryShown) {
        _level2TheoryShown = true;
        _showTheoryOverlay();
      }
    }

    // Phase 2: Detect when the player has successfully re-pushed the crate
    // in a meaningfully different direction from its original slide.
    if (_level2CrateStopped && !_level2RepushStarted && speed > 20.0) {
      final newDir = _crate.velocity.normalized();
      // Original dir was horizontal (RIGHT). New push should be different.
      if (newDir.dot(_level2OriginalDir) < 0.7) {
        _level2RepushStarted = true;
      }
    }
  }


  // ===========================================================================
  // Overlay Helpers
  // ===========================================================================

  void _showTheoryOverlay() {
    stateController.showTheory();
    overlays.add(overlayTheory);
  }

  void _showQuizOverlay() {
    stateController.showQuiz();
    overlays.add(overlayQuiz);
  }

  void _showLevelComplete() {
    stateController.setLevelComplete();
    overlays.add(overlayLevelComplete);
  }

  // ===========================================================================
  // Canvas-level render — draw the guide hint arrow for Level 1
  // ===========================================================================

  /// Draws an animated dashed guide arrow from crate → target on Level 1
  /// before the player touches, so they know which direction to push.
  double _hintAnimTime = 0.0;

  @override
  void render(Canvas canvas) {
    super.render(canvas);

    if (stateController.currentLevel == GameLevel.level1PushPull &&
        !_level1TheoryShown &&
        !_isDragging &&
        _crate.velocity.length < 5.0) {
      _hintAnimTime += 0.016;
      _drawGuideHint(canvas);
    }

    // Level 2 Phase 1: Show "drag LEFT" hint while crate is still sliding.
    if (stateController.currentLevel == GameLevel.level2EffectsOfForce &&
        !_level2CrateStopped &&
        !_isDragging &&
        _crate.velocity.length > 8.0) {
      _hintAnimTime += 0.016;
      _drawLevel2Phase1Hint(canvas);
    }

    // Level 2 Phase 1: Forbidden-drag warning flash.
    if (_level2ForbiddenDragWarning) {
      _drawForbiddenDragWarning(canvas);
    }

    // Level 2 Phase 2: Show guide arrow to target after crate has stopped,
    // so the player knows where to push next.
    if (stateController.currentLevel == GameLevel.level2EffectsOfForce &&
        _level2CrateStopped &&
        !_isDragging &&
        _crate.velocity.length < 5.0) {
      _hintAnimTime += 0.016;
      _drawLevel2GuideHint(canvas);
    }
  }

  void _drawGuideHint(Canvas canvas) {
    _drawAnimatedArrow(
      canvas,
      from: _crate.position,
      to: _targetZone.position,
      label: '👆 Drag UP to push',
      color: const Color(0xFF00FF94),
    );
  }

  /// Phase-1 hint: show a leftward arrow from the crate to teach the player
  /// to drag LEFT (opposite to the crate's rightward slide).
  void _drawLevel2Phase1Hint(Canvas canvas) {
    // Draw hint arrow pointing LEFT from crate position.
    final from = _crate.position.clone();
    final to = Vector2(from.x - 90, from.y); // 90 px to the left
    _drawAnimatedArrow(
      canvas,
      from: from,
      to: to,
      label: '← Drag LEFT to stop!',
      color: const Color(0xFFFF6B35),
    );
  }

  /// Forbidden-drag warning: flash a red "NO!" message and an X symbol so the
  /// player understands they cannot push forward while crate is still sliding.
  void _drawForbiddenDragWarning(Canvas canvas) {
    final cx = _crate.position.x;
    final cy = _crate.position.y;

    // Pulsing red glow circle around crate.
    final pulse = (math.sin(_hintAnimTime * 12) + 1) / 2;
    canvas.drawCircle(
      Offset(cx, cy),
      38 + 8 * pulse,
      Paint()
        ..color = const Color(0xFFFF1744).withAlpha((80 + 80 * pulse).toInt())
        ..style = PaintingStyle.stroke
        ..strokeWidth = 3
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 6),
    );

    // "✗ Stop it first!" label above crate.
    final tp = TextPainter(
      text: const TextSpan(
        text: '✗ Stop it first!',
        style: TextStyle(
          color: Color(0xFFFF1744),
          fontSize: 14,
          fontWeight: FontWeight.w900,
          shadows: [
            Shadow(color: Color(0xFF000000), blurRadius: 4),
          ],
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    tp.paint(canvas, Offset(cx - tp.width / 2, cy - 62));
  }

  void _drawLevel2GuideHint(Canvas canvas) {
    _drawAnimatedArrow(
      canvas,
      from: _crate.position,
      to: _targetZone.position,
      label: '👆 Drag toward ★ target',
      color: const Color(0xFF3A7BFF),
    );
  }

  /// Generic animated dashed arrow with arrowhead and label.
  void _drawAnimatedArrow(
    Canvas canvas, {
    required Vector2 from,
    required Vector2 to,
    required String label,
    required Color color,
  }) {
    // Animated dash offset.
    final dashOffset = (_hintAnimTime * 40) % 20;
    final dir = (to - from).normalized();
    final totalLen = (to - from).length;
    final angle = math.atan2(dir.y, dir.x);

    const dashLen = 12.0;
    const gapLen = 8.0;
    const stride = dashLen + gapLen;

    final dashPaint = Paint()
      ..color = color.withAlpha(180)
      ..strokeWidth = 2.5
      ..strokeCap = StrokeCap.round;

    double d = dashOffset;
    while (d < totalLen - 20) {
      final end = math.min(d + dashLen, totalLen - 20);
      final p1 = Offset(from.x + dir.x * d, from.y + dir.y * d);
      final p2 = Offset(from.x + dir.x * end, from.y + dir.y * end);
      canvas.drawLine(p1, p2, dashPaint);
      d += stride;
    }

    // Arrowhead near target.
    final headPos =
        Offset(from.x + dir.x * (totalLen - 22), from.y + dir.y * (totalLen - 22));
    final headPath = Path()
      ..moveTo(
        headPos.dx + math.cos(angle) * 14,
        headPos.dy + math.sin(angle) * 14,
      )
      ..lineTo(
        headPos.dx + math.cos(angle - 2.4) * 8,
        headPos.dy + math.sin(angle - 2.4) * 8,
      )
      ..lineTo(
        headPos.dx + math.cos(angle + 2.4) * 8,
        headPos.dy + math.sin(angle + 2.4) * 8,
      )
      ..close();
    canvas.drawPath(
        headPath, Paint()..color = color.withAlpha(200));

    // Pulsing label near the crate.
    final pulse = (math.sin(_hintAnimTime * 3) + 1) / 2;
    final labelAlpha = (120 + 100 * pulse).toInt();
    final tp = TextPainter(
      text: TextSpan(
        text: label,
        style: TextStyle(
          color: color.withAlpha(labelAlpha),
          fontSize: 13,
          fontWeight: FontWeight.w700,
        ),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    tp.paint(canvas, Offset(from.x - tp.width / 2, from.y + 36));
  }

  // ===========================================================================
  // Getters for overlays
  // ===========================================================================

  LevelConfig get currentConfig => _config;
  int get currentLevelNumber => stateController.currentLevel.index + 1;
}
