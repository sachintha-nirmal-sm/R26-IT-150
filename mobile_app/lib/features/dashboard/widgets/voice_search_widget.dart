import 'package:flutter/material.dart';
import 'package:speech_to_text/speech_to_text.dart' as stt;

class VoiceSearchWidget extends StatefulWidget {
  final Function(String) onSpeechResult;
  final VoidCallback? onStartListening;
  final VoidCallback? onStopListening;
  final VoidCallback? onError;

  const VoiceSearchWidget({
    Key? key,
    required this.onSpeechResult,
    this.onStartListening,
    this.onStopListening,
    this.onError,
  }) : super(key: key);

  @override
  State<VoiceSearchWidget> createState() => _VoiceSearchWidgetState();
}

class _VoiceSearchWidgetState extends State<VoiceSearchWidget> {
  late stt.SpeechToText _speechToText;
  bool _isListening = false;
  bool _isInitialized = false;
  String _currentWords = '';
  double _confidence = 0.0;

  @override
  void initState() {
    super.initState();
    _initializeSpeechToText();
  }

  Future<void> _initializeSpeechToText() async {
    _speechToText = stt.SpeechToText();
    final available = await _speechToText.initialize(
      onError: (error) {
        print('Error: ${error.errorMsg}');
        widget.onError?.call();
        setState(() => _isListening = false);
      },
      onStatus: (status) {
        print('Status: $status');
      },
    );

    setState(() => _isInitialized = available);

    if (!available) {
      print('Speech to text not available on this device');
    }
  }

  void _startListening() async {
    if (!_isInitialized) {
      print('Speech to text not initialized');
      return;
    }

    if (!_isListening) {
      widget.onStartListening?.call();

      bool available = await _speechToText.initialize();
      if (available) {
        setState(() {
          _isListening = true;
          _currentWords = '';
          _confidence = 0.0;
        });

        _speechToText.listen(
          onResult: (result) {
            setState(() {
              _currentWords = result.recognizedWords;
              _confidence = result.confidence;
            });

            if (result.finalResult) {
              _stopListening();
              widget.onSpeechResult(_currentWords);
            }
          },
          listenFor: const Duration(seconds: 30),
          pauseFor: const Duration(seconds: 3),
          partialResults: true,
          cancelOnError: true,
          listenMode: stt.ListenMode.search,
        );
      }
    }
  }

  void _stopListening() async {
    if (_isListening) {
      widget.onStopListening?.call();
      await _speechToText.stop();
      setState(() => _isListening = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        // Mic button
        GestureDetector(
          onTap: _isListening ? _stopListening : _startListening,
          child: Container(
            width: 64,
            height: 64,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              color: _isListening
                  ? const Color(0xFFF44336).withOpacity(0.2)
                  : const Color(0xFF2196F3).withOpacity(0.2),
              border: Border.all(
                color: _isListening ? const Color(0xFFF44336) : const Color(0xFF2196F3),
                width: 2,
              ),
            ),
            child: Icon(
              _isListening ? Icons.mic : Icons.mic_none,
              color: _isListening ? const Color(0xFFF44336) : const Color(0xFF2196F3),
              size: 28,
            ),
          ),
        ),
        if (_isListening) ...[
          const SizedBox(height: 16),
          // Live feedback during listening
          Column(
            children: [
              Text(
                _currentWords.isEmpty ? 'Listening...' : _currentWords,
                style: const TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.w500,
                  color: Color(0xFF1A1A2E),
                ),
                textAlign: TextAlign.center,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
              ),
              const SizedBox(height: 8),
              // Confidence indicator
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(
                    'Confidence: ${(_confidence * 100).toStringAsFixed(0)}%',
                    style: TextStyle(
                      fontSize: 12,
                      color: Colors.grey[600],
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 8),
              // Audio level visualization
              _buildAudioLevelVisualizer(),
            ],
          ),
          const SizedBox(height: 16),
          // Stop button
          ElevatedButton.icon(
            onPressed: _stopListening,
            icon: const Icon(Icons.stop_circle, size: 20),
            label: const Text('Stop Listening'),
            style: ElevatedButton.styleFrom(
              backgroundColor: const Color(0xFFF44336),
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(8),
              ),
            ),
          ),
        ] else if (!_isInitialized) ...[
          const SizedBox(height: 12),
          Text(
            'Speech recognition not available',
            style: TextStyle(
              fontSize: 12,
              color: Colors.grey[600],
            ),
          ),
        ] else if (_currentWords.isNotEmpty) ...[
          const SizedBox(height: 12),
          Text(
            _currentWords,
            style: const TextStyle(
              fontSize: 14,
              fontWeight: FontWeight.w500,
              color: Color(0xFF1A1A2E),
            ),
            textAlign: TextAlign.center,
            maxLines: 3,
            overflow: TextOverflow.ellipsis,
          ),
        ],
      ],
    );
  }

  Widget _buildAudioLevelVisualizer() {
    // Simulate audio level bars
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: List.generate(
        5,
        (index) {
          final height = 8.0 + (index * 8.0) * (_confidence);
          return Padding(
            padding: const EdgeInsets.symmetric(horizontal: 3),
            child: Container(
              width: 4,
              height: height,
              decoration: BoxDecoration(
                color: const Color(0xFF2196F3),
                borderRadius: BorderRadius.circular(2),
              ),
            ),
          );
        },
      ),
    );
  }

  @override
  void dispose() {
    _speechToText.stop();
    super.dispose();
  }
}

/// Simple voice search dialog
class VoiceSearchDialog extends StatefulWidget {
  final Function(String) onSearch;

  const VoiceSearchDialog({
    Key? key,
    required this.onSearch,
  }) : super(key: key);

  @override
  State<VoiceSearchDialog> createState() => _VoiceSearchDialogState();
}

class _VoiceSearchDialogState extends State<VoiceSearchDialog> {
  String _searchResult = '';

  @override
  Widget build(BuildContext context) {
    return Dialog(
      child: Container(
        padding: const EdgeInsets.all(24),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(16),
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text(
              'Voice Search',
              style: TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.w600,
                color: Color(0xFF1A1A2E),
              ),
            ),
            const SizedBox(height: 24),
            VoiceSearchWidget(
              onSpeechResult: (result) {
                setState(() => _searchResult = result);

                // Auto-search after a short delay
                Future.delayed(const Duration(milliseconds: 500), () {
                  widget.onSearch(result);
                  if (mounted) Navigator.pop(context);
                });
              },
              onError: () {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(
                    content: Text('Error during voice search'),
                    backgroundColor: Colors.red,
                  ),
                );
              },
            ),
            const SizedBox(height: 16),
            TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text('Cancel'),
            ),
          ],
        ),
      ),
    );
  }
}
