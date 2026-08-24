import 'package:flame/game.dart';
import 'package:flutter/material.dart';

import '../game/pressure_puzzle_game.dart';
import '../overlays/calculation_overlay.dart';
import '../overlays/outcome_overlay.dart';

class PressurePuzzleScreen extends StatefulWidget {
  const PressurePuzzleScreen({super.key});

  @override
  State<PressurePuzzleScreen> createState() => _PressurePuzzleScreenState();
}

class _PressurePuzzleScreenState extends State<PressurePuzzleScreen> {
  late final PressurePuzzleGame _game;

  // We need to pass these values to the overlay via state because 
  // Flame's overlay system expects widget builders, not dynamic values per-call 
  // if not using custom overlay logic. We will use simple setState.
  
  bool _showCalculation = false;
  double _force = 0;
  double _area = 0;
  
  bool _showOutcome = false;
  bool _isSuccess = false;
  double _pressure = 0;

  @override
  void initState() {
    super.initState();
    _game = PressurePuzzleGame(
      onShowCalculationDialog: (force, area) {
        setState(() {
          _force = force;
          _area = area;
          _showCalculation = true;
        });
      },
      onHideCalculationDialog: () {
        setState(() {
          _showCalculation = false;
        });
      },
      onShowOutcomeDialog: (success, pressure) {
        setState(() {
          _isSuccess = success;
          _pressure = pressure;
          _showOutcome = true;
        });
      },
    );
  }

  void _onCalculationSubmit(bool isCorrect, double calculatedPressure) {
    if (isCorrect) {
      setState(() {
        _showCalculation = false;
      });
      _game.handleCalculationResult(isCorrect, calculatedPressure);
    }
  }

  void _onRetry() {
    setState(() {
      _showOutcome = false;
    });
    _game.resetGame();
  }

  void _onNext() {
    // Handle navigation to the next puzzle
    Navigator.of(context).pop();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Puzzle 1: The Fragile Glass Floor'),
        backgroundColor: const Color(0xFF0F172A),
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.undo),
            tooltip: 'Undo Last Move',
            onPressed: () {
              _game.undoLastMove();
            },
          ),
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Reset All',
            onPressed: () {
              _onRetry();
            },
          ),
        ],
      ),
      body: Stack(
        children: [
          GameWidget(game: _game),
          
          if (_showCalculation)
            Positioned.fill(
              child: Container(
                color: Colors.black54,
                child: CalculationOverlay(
                  force: _force,
                  area: _area,
                  onSubmit: _onCalculationSubmit,
                ),
              ),
            ),
            
          if (_showOutcome)
            Positioned.fill(
              child: Container(
                color: Colors.black54,
                child: OutcomeOverlay(
                  isSuccess: _isSuccess,
                  pressure: _pressure,
                  onRetry: _onRetry,
                  onNext: _onNext,
                ),
              ),
            ),
        ],
      ),
    );
  }
}
