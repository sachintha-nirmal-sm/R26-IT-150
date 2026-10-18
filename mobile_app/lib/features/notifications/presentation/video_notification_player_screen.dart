import 'dart:io';

import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:video_player/video_player.dart';

import '../../../core/backend_url.dart';

/// Plays a generated personalized video notification. Fetches the
/// notifications/{id} doc for its videoUrl, marks it opened on first play.
class VideoNotificationPlayerScreen extends StatefulWidget {
  const VideoNotificationPlayerScreen(
      {super.key, required this.notificationId});

  final String notificationId;

  @override
  State<VideoNotificationPlayerScreen> createState() =>
      _VideoNotificationPlayerScreenState();
}

/// The video link is saved when the admin sends the notification, with
/// whatever host the sender's backend was configured with (localhost for
/// Chrome, 10.0.2.2 for the Android emulator). The device playing it may be
/// a different one, so swap a loopback host for the one this device uses
/// to reach the backend. Real hostnames are left alone.
Uri _reachableUri(String raw) {
  final uri = Uri.parse(raw);
  const loopback = {'localhost', '127.0.0.1', '10.0.2.2'};
  if (!loopback.contains(uri.host)) return uri;
  final base = Uri.parse(backendUrl);
  return uri.replace(scheme: base.scheme, host: base.host, port: base.port);
}

Future<({VideoPlayerController? controller, Object? error})> _playFromDownload(
    Uri uri) async {
  try {
    final response = await http.get(uri).timeout(const Duration(seconds: 60));
    if (response.statusCode != 200) {
      return (controller: null, error: 'HTTP ${response.statusCode} for $uri');
    }
    final file = File(
        '${Directory.systemTemp.path}/notification_${uri.pathSegments.last}');
    await file.writeAsBytes(response.bodyBytes, flush: true);
    final controller = VideoPlayerController.file(file);
    await controller.initialize();
    return (controller: controller, error: null);
  } catch (e) {
    return (controller: null, error: e);
  }
}

class _VideoNotificationPlayerScreenState
    extends State<VideoNotificationPlayerScreen> {
  VideoPlayerController? _controller;
  String? _title;
  String? _body;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final snap = await FirebaseFirestore.instance
          .collection('notifications')
          .doc(widget.notificationId)
          .get();
      final data = snap.data();
      if (data == null || data['videoUrl'] == null) {
        setState(() => _error = 'Video not ready yet — try again shortly.');
        return;
      }
      _title = data['title'] as String?;
      _body = data['body'] as String?;

      // Try the address this device can reach first, then the stored one,
      // and report which address failed so a network problem is obvious.
      final stored = data['videoUrl'] as String;
      final candidates = <Uri>[_reachableUri(stored), Uri.parse(stored)];
      VideoPlayerController? controller;
      Object? lastError;
      Uri? lastTried;
      final seen = <String>{};
      for (final uri in candidates) {
        if (!seen.add(uri.toString())) continue;
        lastTried = uri;
        final attempt = VideoPlayerController.networkUrl(uri);
        try {
          await attempt.initialize();
          controller = attempt;
          break;
        } catch (e) {
          lastError = e;
          await attempt.dispose();
        }
      }
      // Last resort on Android/iOS: fetch the file with Dart's own HTTP
      // client (the same one the API calls use) and play it from disk, in
      // case the native player's streaming can't open the URL.
      if (controller == null && !kIsWeb) {
        final fileAttempt = await _playFromDownload(candidates.first);
        if (fileAttempt.controller != null) {
          controller = fileAttempt.controller;
        } else {
          lastError = '$lastError; download fallback: ${fileAttempt.error}';
        }
      }
      if (controller == null) {
        throw 'could not load $lastTried — $lastError';
      }
      await controller.setVolume(1.0);
      await controller.play();
      if (!mounted) {
        await controller.dispose();
        return;
      }
      setState(() => _controller = controller);
    } catch (e) {
      if (!mounted) return;
      setState(() => _error = 'Could not play this video: $e');
    }
  }

  @override
  void dispose() {
    _controller?.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.black,
      appBar: AppBar(
        backgroundColor: Colors.black,
        foregroundColor: Colors.white,
        elevation: 0,
      ),
      body: _error != null
          ? Center(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Text(_error!,
                    style: const TextStyle(color: Colors.white),
                    textAlign: TextAlign.center),
              ),
            )
          : _controller == null
              ? const Center(
                  child: CircularProgressIndicator(color: Colors.white))
              : SafeArea(
                  child: Column(
                    children: [
                      // Expanded bounds the video's height to whatever space
                      // is actually left after the text/button below take
                      // their natural size — without this, AspectRatio (the
                      // videos are tall 9:16) sizes purely from width inside
                      // the unbounded Center/Column(min) this used to be in,
                      // computing a height taller than the screen and
                      // pushing everything else off the bottom.
                      Expanded(
                        child: Center(
                          child: AspectRatio(
                            aspectRatio: _controller!.value.aspectRatio,
                            child: VideoPlayer(_controller!),
                          ),
                        ),
                      ),
                      const SizedBox(height: 12),
                      if (_title != null)
                        Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 24),
                          child: Text(_title!,
                              style: const TextStyle(
                                  color: Colors.white,
                                  fontSize: 20,
                                  fontWeight: FontWeight.bold),
                              textAlign: TextAlign.center),
                        ),
                      const SizedBox(height: 8),
                      if (_body != null)
                        Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 24),
                          child: Text(_body!,
                              style: TextStyle(color: Colors.grey.shade300),
                              textAlign: TextAlign.center),
                        ),
                      const SizedBox(height: 12),
                      IconButton(
                        iconSize: 48,
                        color: Colors.white,
                        icon: Icon(_controller!.value.isPlaying
                            ? Icons.pause_circle
                            : Icons.play_circle),
                        onPressed: () => setState(() {
                          _controller!.value.isPlaying
                              ? _controller!.pause()
                              : _controller!.play();
                        }),
                      ),
                      const SizedBox(height: 8),
                    ],
                  ),
                ),
    );
  }
}
