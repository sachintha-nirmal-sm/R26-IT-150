import 'package:flutter/foundation.dart';

// ---------------------------------------------------------------------------
// Game State Enum
// ---------------------------------------------------------------------------

/// Represents every discrete state the physics game can be in.
enum GameState {
  /// Assets are loading / level is being set up.
  loading,

  /// The simulation is actively running.
  playing,

  /// Simulation is paused (e.g., overlay is open).
  paused,

  /// Theory popup is visible (pauses game loop).
  showTheory,

  /// MCQ quiz dialog is visible (pauses game loop).
  showQuiz,

  /// The current level has been completed successfully.
  levelComplete,

  /// All three levels have been completed.
  gameOver,
}

// ---------------------------------------------------------------------------
// Level Enum
// ---------------------------------------------------------------------------

enum GameLevel {
  level1PushPull,
  level2EffectsOfForce,
  level3Friction,
}

extension GameLevelExt on GameLevel {
  int get index1Based => index + 1;

  String get displayName {
    switch (this) {
      case GameLevel.level1PushPull:
        return 'Level 1 — Push & Pull';
      case GameLevel.level2EffectsOfForce:
        return 'Level 2 — Effects of Force';
      case GameLevel.level3Friction:
        return 'Level 3 — Friction';
    }
  }

  GameLevel? get next {
    final idx = index + 1;
    if (idx < GameLevel.values.length) return GameLevel.values[idx];
    return null;
  }
}

// ---------------------------------------------------------------------------
// Game State Controller
// ---------------------------------------------------------------------------

/// A `ChangeNotifier` that owns the mutable game state and broadcasts changes
/// to Flutter widgets (overlays) listening via [ValueListenableBuilder].
class GameStateController extends ChangeNotifier {
  GameState _state = GameState.loading;
  GameLevel _currentLevel = GameLevel.level1PushPull;

  // ── Getters ───────────────────────────────────────────────────────────────

  GameState get state => _state;
  GameLevel get currentLevel => _currentLevel;

  bool get isPlaying => _state == GameState.playing;
  bool get isPaused =>
      _state == GameState.paused ||
      _state == GameState.showTheory ||
      _state == GameState.showQuiz ||
      _state == GameState.levelComplete;

  // ── Mutators ──────────────────────────────────────────────────────────────

  void setPlaying() => _update(GameState.playing);

  void setPaused() => _update(GameState.paused);

  void showTheory() => _update(GameState.showTheory);

  void showQuiz() => _update(GameState.showQuiz);

  void setLevelComplete() => _update(GameState.levelComplete);

  void setGameOver() => _update(GameState.gameOver);

  void setLoading() => _update(GameState.loading);

  void advanceLevel() {
    final next = _currentLevel.next;
    if (next != null) {
      _currentLevel = next;
      _update(GameState.loading);
    } else {
      _update(GameState.gameOver);
    }
  }

  void restartLevel() {
    _update(GameState.loading);
  }

  void resetToLevel1() {
    _currentLevel = GameLevel.level1PushPull;
    _update(GameState.loading);
  }

  // ── Private ───────────────────────────────────────────────────────────────

  void _update(GameState newState) {
    _state = newState;
    notifyListeners();
  }
}
