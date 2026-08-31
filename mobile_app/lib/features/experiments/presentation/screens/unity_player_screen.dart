import 'package:flutter/material.dart';

import '../../data/practical.dart';
import '../../data/practicals_repository.dart';
import '../../data/unity_lab_service.dart';

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
    await UnityLabService.takePendingResult();
    await _submit(result);
  }

  void _handleCancelled() {
    if (_handled || _submitting || !mounted) return;
    _recoverResultOrLeave();
  }

  Future<void> _recoverResultOrLeave() async {
    await Future<void>.delayed(const Duration(milliseconds: 400));
    if (_handled || !mounted) return;
    final result = await UnityLabService.takePendingResult();
    if (result != null) {
      await _handleUnityResult(result);
      return;
    }
    if (_handled || !mounted) return;
    _handled = true;
    Navigator.pop(context);
  }

  Future<void> _submit(UnityPracticalResult result) async {
    setState(() {
      _submitting = true;
      _saveFailed = false;
      _error = null;
    });
    try {
      await UnityLabService.unloadUnity();
      await Future<void>.delayed(const Duration(milliseconds: 600));
      if (_session.isDemo) {
        await _finishTrial(result);
      } else {
        Object? lastError;
        for (var attempt = 0; attempt < 2; attempt++) {
          try {
            await _repo.recordOfficialScore(
              session: _session,
              score: result.score,
              durationSeconds: result.timeUsed,
              measurements: {
                ...result.measurements,
                'mode': 'start',
                'timeUsed': result.timeUsed,
                'completed': result.completed,
              },
            );
            lastError = null;
            break;
          } catch (error) {
            lastError = error;
            if (attempt < 1) {
              await Future<void>.delayed(const Duration(seconds: 2));
            }
          }
        }
        if (lastError != null) throw lastError;
      }
      if (!mounted) return;
      if (_session.isDemo) {
        Navigator.pop(context, result);
        return;
      }
      Navigator.of(context, rootNavigator: true).pushNamedAndRemoveUntil(
        '/profile',
        (route) => false,
      );
    } catch (error) {
      if (!mounted) return;
      _handled = false;
      setState(() {
        _submitting = false;
        _saveFailed = true;
        _pendingResult = result;
        _error =
            'Result could not be saved.\n\n$error\n\n'
            'Keep the backend running and USB debugging connected, then tap Retry save.';
      });
    }
  }

  Future<void> _finishTrial(UnityPracticalResult result) async {
    try {
      var resultId = _session.resultId;
      if (_session.isLocal) {
        final started = await _repo.startDemo(_session.practicalId);
        resultId = started.resultId;
      }
      await _repo.finishDemo(
        practicalId: _session.practicalId,
        resultId: resultId,
        score: result.score,
        measurements: {
          ...result.measurements,
          'mode': 'trial',
          'timeUsed': result.timeUsed,
        },
      );
    } catch (_) {
      // Trial is practice. Always return to Start / Trial even if the
      // backend is briefly unreachable.
    }
  }

  Future<void> _returnToStartTrial() async {
    if (!mounted) return;
    Navigator.pop(context, _pendingResult);
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
                  ? 'Saving result to your profile…'
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
                onPressed: _submitting
                    ? null
                    : () {
                        _handled = false;
                        _submit(_pendingResult!);
                      },
                child: const Text('Retry save'),
              ),
              TextButton(
                onPressed: _returnToStartTrial,
                child: const Text('Back to Start / Trial'),
              ),
            ],
            const Spacer(),
            if (_error != null && !_saveFailed)
              TextButton(
                onPressed: () => Navigator.pop(context),
                child: const Text('Back to Start / Trial'),
              ),
          ],
        ),
      ),
    );
  }
}
