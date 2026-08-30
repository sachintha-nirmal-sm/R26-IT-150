import 'package:flutter/material.dart';

class OutcomeOverlay extends StatelessWidget {
  const OutcomeOverlay({
    super.key,
    required this.isSuccess,
    required this.pressure,
    required this.onRetry,
    required this.onNext,
  });

  final bool isSuccess;
  final double pressure;
  final VoidCallback onRetry;
  final VoidCallback onNext;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Material(
        color: Colors.transparent,
        child: Container(
          constraints: const BoxConstraints(maxWidth: 400),
          padding: const EdgeInsets.all(32),
          decoration: BoxDecoration(
            color: isSuccess ? Colors.green[50] : Colors.red[50],
            borderRadius: BorderRadius.circular(16),
            border: Border.all(
              color: isSuccess ? Colors.green : Colors.red,
              width: 3,
            ),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withAlpha(128),
                blurRadius: 20,
                spreadRadius: 5,
              )
            ],
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(
                isSuccess ? Icons.check_circle : Icons.warning_rounded,
                color: isSuccess ? Colors.green : Colors.red,
                size: 64,
              ),
              const SizedBox(height: 16),
              Text(
                isSuccess ? 'Success!' : 'Oh no!',
                style: TextStyle(
                  fontSize: 28,
                  fontWeight: FontWeight.bold,
                  color: isSuccess ? Colors.green[800] : Colors.red[800],
                ),
              ),
              const SizedBox(height: 16),
              Text(
                isSuccess 
                    ? 'Excellent! When the contact area (A) increases, the pressure (P = $pressure Pa) on the glass decreases. The glass survived!'
                    : 'The glass broke! With a small contact area (A), the pressure (P = $pressure Pa) was too high for the glass to withstand (> 100 Pa).',
                textAlign: TextAlign.center,
                style: const TextStyle(fontSize: 16, color: Colors.black87),
              ),
              const SizedBox(height: 32),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                children: [
                  ElevatedButton(
                    onPressed: onRetry,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.grey[700],
                      padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
                    ),
                    child: const Text('Retry', style: TextStyle(color: Colors.white, fontSize: 16)),
                  ),
                  if (isSuccess)
                    ElevatedButton(
                      onPressed: onNext,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.green[600],
                        padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
                      ),
                      child: const Text('Next Puzzle', style: TextStyle(color: Colors.white, fontSize: 16)),
                    ),
                ],
              )
            ],
          ),
        ),
      ),
    );
  }
}
