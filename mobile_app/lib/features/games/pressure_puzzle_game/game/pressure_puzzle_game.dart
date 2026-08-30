import 'package:flame/game.dart';
import 'package:flutter/material.dart';

import '../components/glass_floor_component.dart';
import '../components/heavy_machine_component.dart';
import '../components/surface_modifier_component.dart';

enum GamePhase {
  awaitingSurface,
  awaitingMachine,
  calculating,
  resolved
}

class PressurePuzzleGame extends FlameGame {
  PressurePuzzleGame({
    required this.onShowCalculationDialog,
    required this.onHideCalculationDialog,
    required this.onShowOutcomeDialog,
  });

  final void Function(double force, double area) onShowCalculationDialog;
  final VoidCallback onHideCalculationDialog;
  final void Function(bool success, double pressure) onShowOutcomeDialog;

  late GlassFloorComponent glassFloor;
  late HeavyMachineComponent machine;
  late SurfaceModifierComponent wheels;
  late SurfaceModifierComponent planks;

  GamePhase phase = GamePhase.awaitingSurface;
  SurfaceModifierComponent? activeSurface;

  // Constants
  static const String overlayCalculation = 'calculationOverlay';
  static const String overlayOutcome = 'outcomeOverlay';

  @override
  Color backgroundColor() => const Color(0xFF1E293B); // Dark slate background

  @override
  Future<void> onLoad() async {
    await super.onLoad();

    final centerX = size.x / 2;
    
    // 1. Add Glass Floor (Drop Zone)
    glassFloor = GlassFloorComponent(
      position: Vector2(centerX, size.y * 0.7),
    );
    add(glassFloor);

    // 2. Add Surface Modifiers
    wheels = SurfaceModifierComponent(
      type: SurfaceType.wheels,
      startPosition: Vector2(size.x * 0.25, size.y * 0.2),
      onDroppedCallback: _onSurfaceDropped,
    );
    add(wheels);

    planks = SurfaceModifierComponent(
      type: SurfaceType.planks,
      startPosition: Vector2(size.x * 0.75, size.y * 0.2),
      onDroppedCallback: _onSurfaceDropped,
    );
    add(planks);

    // 3. Add Heavy Machine
    machine = HeavyMachineComponent(
      startPosition: Vector2(centerX, size.y * 0.4),
      onDroppedCallback: _onMachineDropped,
    );
    add(machine);
  }

  void _onSurfaceDropped(SurfaceModifierComponent surface) {
    if (phase != GamePhase.awaitingSurface) {
      surface.resetPosition();
      return;
    }

    final distance = (surface.position - glassFloor.position).length;
    
    // Check if dropped near the glass floor
    if (distance < 100) {
      // Snap to glass floor
      surface.snapTo(glassFloor.position - Vector2(0, glassFloor.size.y / 2 + surface.size.y / 2));
      activeSurface = surface;
      phase = GamePhase.awaitingMachine;
      
      // Send the other modifier back just in case
      final other = surface.type == SurfaceType.wheels ? planks : wheels;
      other.resetPosition();
    } else {
      surface.resetPosition();
    }
  }

  void _onMachineDropped(HeavyMachineComponent mac) {
    if (phase != GamePhase.awaitingMachine) {
      mac.resetPosition();
      // Optional: Add some visual hint that a surface needs to be placed first
      return;
    }

    // Check if dropped on the active surface
    if (activeSurface == null) return;
    
    final distance = (mac.position - activeSurface!.position).length;
    if (distance < 100) {
      // Snap machine on top of surface
      mac.snapTo(activeSurface!.position - Vector2(0, activeSurface!.size.y / 2 + mac.size.y / 2));
      phase = GamePhase.calculating;
      
      // Pause engine and show calculation overlay
      pauseEngine();
      onShowCalculationDialog(mac.force, activeSurface!.area);
    } else {
      mac.resetPosition();
    }
  }

  void handleCalculationResult(bool isCorrect, double calculatedPressure) {
    if (isCorrect) {
      resumeEngine();
      phase = GamePhase.resolved;
      
      // Check physics condition
      bool success = calculatedPressure <= glassFloor.maxPressure;
      
      if (!success) {
        glassFloor.breakGlass();
        // Maybe make the machine and surface drop down slightly
        activeSurface?.position.y += 20;
        machine.position.y += 20;
      }
      
      onShowOutcomeDialog(success, calculatedPressure);
    }
    // If incorrect, the overlay handles showing a hint/shake and doesn't call this.
  }

  void resetGame() {
    phase = GamePhase.awaitingSurface;
    activeSurface = null;
    glassFloor.repairGlass();
    
    machine.returnToStart();
    wheels.returnToStart();
    planks.returnToStart();
    
    resumeEngine();
  }

  void undoLastMove() {
    if (phase == GamePhase.calculating) {
      // Undo machine placement
      machine.returnToStart();
      phase = GamePhase.awaitingMachine;
      resumeEngine();
      onHideCalculationDialog();
    } else if (phase == GamePhase.awaitingMachine) {
      // Undo surface placement
      activeSurface?.returnToStart();
      activeSurface = null;
      phase = GamePhase.awaitingSurface;
    }
  }
}
