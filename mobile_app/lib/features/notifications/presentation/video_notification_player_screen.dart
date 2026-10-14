import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:flutter/material.dart';
import 'package:video_player/video_player.dart';

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

      final controller =
          VideoPlayerController.networkUrl(Uri.parse(data['videoUrl']));
      await controller.initialize();
      controller.play();
      if (!mounted) return;
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
