import 'package:flutter/material.dart';

import '../../data/practical.dart';
import '../../data/practicals_repository.dart';
import '../../data/unity_lab_service.dart';
import 'experiment_results_screen.dart';

class UnityPlayerScreen extends StatefulWidget {
  const UnityPlayerScreen({super.key, required this.args});

  final PracticalRunArgs args;

  @override
  State<UnityPlayerScreen> createState() => _UnityPlayerScreenState();
}

class _UnityPlayerScreenState extends State<UnityPlayerScreen>
    with WidgetsBindingObserver {
  final _repo = PracticalsRepository();
  bool _starting = true;
  bool _submitting = false;
  bool _handled = false;
  String? _error;
  UnityPracticalResult? _pendingResult;
  bool _saveFailed = false;

  Practical get _practical => widget.args.practical;
  PracticalSession get _session => widget.args.session;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    UnityLabService.listen(
      onResult: _handleUnityResult,
      onCancelled: _handleCancelled,
    );
    _launch();
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    UnityLabService.stopListening();
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) {
      _readPendingResult();
    }
  }

  Future<void> _launch() async {
    final available = await UnityLabService.isAvailable();
    if (!mounted) return;
    if (!available) {
      setState(() {
        _starting = false;
        _error =
            'This practical runs inside this app through Unity as a Library.\n\n'
            'Export it from Unity first:\n'
            'Tools → PhysiVLab → Export Android Library\n\n'
            'Then rebuild the Flutter Android app. Do not install a separate Unity APK.';
      });
      return;
    }

    final payload = UnityLabService.payloadFor(
      practical: _practical,
      session: _session,
    );
    final started = await UnityLabService.startSession(payload);
    if (!mounted) return;
    setState(() {
      _starting = false;
      if (!started) {
        _error = 'Could not open the Unity practical inside this app.';
      }
    });
  }

  Future<void> _readPendingResult() async {
    if (_handled) return;
    final result = await UnityLabService.takePendingResult();
    if (result != null) {
      await _handleUnityResult(result);
    }
  }

  Future<void> _handleUnityResult(UnityPracticalResult result) async {
    if (_handled || !mounted) return;
    _handled = true;
    _pendingResult = result;
    await _submit(result);
  }

  void _handleCancelled() {
    if (_handled || !mounted) return;
    setState(() {
      _error = 'Practical closed without a saved result.';
    });
  }

  Future<void> _submit(UnityPracticalResult result) async {
    setState(() {
      _submitting = true;
      _saveFailed = false;
      _error = null;
    });
    try {
      if (_session.isLocal) {
        if (!mounted) return;
        if (_session.isDemo) {
          Navigator.pop(context);
          return;
        }
        await _openResults(result.score, result.timeUsed);
        return;
      }

      if (_session.isDemo) {
        await _repo.finishDemo(
          practicalId: _session.practicalId,
          resultId: _session.resultId,
          score: result.score,
          measurements: {
            ...result.measurements,
            'mode': 'trial',
            'timeUsed': result.timeUsed,
          },
        );
        if (!mounted) return;
        Navigator.pop(context, result);
        return;
      }

      final saved = await _repo.submitPractical(
        practicalId: _session.practicalId,
        resultId: _session.resultId,
        attemptNumber: _session.attemptNumber,
        score: result.score,
        measurements: {
          ...result.measurements,
          'mode': 'start',
          'timeUsed': result.timeUsed,
          'completed': result.completed,
        },
      );
      if (!mounted) return;
      await _openResults(saved.percentage.round(), saved.durationSeconds ?? result.timeUsed);
    } catch (error) {
      if (!mounted) return;
      setState(() {
        _submitting = false;
        _saveFailed = true;
        _pendingResult = result;
        _error = 'Result could not be saved. Please retry.';
      });
    }
  }

  Future<void> _openResults(int score, int timeUsedSeconds) async {
    final minutes = timeUsedSeconds ~/ 60;
    final seconds = timeUsedSeconds % 60;
    await Navigator.pushReplacement(
      context,
      MaterialPageRoute(
        builder: (context) => ExperimentResultsScreen(
          score: score,
          finalDuration: '$minutes min $seconds sec',
          topicName: _practical.title,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFF101828),
      appBar: AppBar(
        backgroundColor: const Color(0xFF101828),
        foregroundColor: Colors.white,
        title: Text(_practical.title),
      ),
      body: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            const SizedBox(height: 24),
            Icon(
              _error == null ? Icons.science : Icons.error_outline,
              color: Colors.white,
              size: 56,
            ),
            const SizedBox(height: 20),
            Text(
              _submitting
                  ? 'Saving result…'
                  : _starting
                      ? 'Opening ${_practical.title}'
                      : _error ??
                          'Complete the practical in the Unity lab. This screen waits for the score.',
              textAlign: TextAlign.center,
              style: const TextStyle(
                color: Colors.white,
                fontSize: 16,
                height: 1.4,
              ),
            ),
            const SizedBox(height: 24),
            if (_starting || _submitting)
              const Center(child: CircularProgressIndicator(color: Colors.white)),
            if (_saveFailed && _pendingResult != null) ...[
              const SizedBox(height: 16),
              ElevatedButton(
                onPressed: _submitting ? null : () => _submit(_pendingResult!),
                child: const Text('Retry save'),
              ),
            ],
            const Spacer(),
            if (_error != null && !_saveFailed)
              TextButton(
                onPressed: () => Navigator.pop(context),
                child: const Text('Back'),
              ),
          ],
        ),
      ),
    );
  }
}
