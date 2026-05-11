import 'package:flutter/material.dart';
import 'package:video_player/video_player.dart';

void main() => runApp(const MaterialApp(home: LessonDetailScreen(title: 'Preview', description: 'Preview description')));

class LessonDetailScreen extends StatefulWidget {
  final String title;
  final String description;

  const LessonDetailScreen({
    super.key,
    required this.title,
    required this.description,
  });

  @override
  State<LessonDetailScreen> createState() => _LessonDetailScreenState();
}

class _LessonDetailScreenState extends State<LessonDetailScreen> {
  late VideoPlayerController _videoController;
  bool _isInitialized = false;
  bool _hasError = false;
  String _errorMessage = '';

  @override
  void initState() {
    super.initState();
    _initializeVideo();
  }

  Future<void> _initializeVideo() async {
    try {
      _videoController = VideoPlayerController.asset('assets/videos/force 1.mp4');
      await _videoController.initialize();
      setState(() {
        _isInitialized = true;
      });
    } catch (e) {
      setState(() {
        _hasError = true;
        _errorMessage = e.toString();
      });
    }
  }

  @override
  void dispose() {
    _videoController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Color(0xFF2196F3)),
          onPressed: () => Navigator.pop(context),
        ),
        title: const Text(
          'Physics Lab',
          style: TextStyle(color: Color(0xFF1A1C1E), fontWeight: FontWeight.bold),
        ),
        centerTitle: true,
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16.0),
            child: GestureDetector(
              onTap: () => Navigator.pushNamed(context, '/profile'),
              child: const CircleAvatar(
                radius: 18,
                backgroundColor: Color(0xFFCCCCCC),
                child: Icon(Icons.person, color: Colors.white, size: 22),
              ),
            ),
          )
        ],
      ),
      body: SingleChildScrollView(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Video Section with Understanding Force & Motion
            Padding(
              padding: const EdgeInsets.all(20.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'Understanding Force & Motion',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 12),
                  ClipRRect(
                    borderRadius: BorderRadius.circular(15),
                    child: _hasError
                        ? Container(
                            height: 200,
                            color: Colors.grey.shade300,
                            child: Center(
                              child: Column(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: [
                                  const Icon(Icons.error_outline, size: 40, color: Colors.red),
                                  const SizedBox(height: 10),
                                  Padding(
                                    padding: const EdgeInsets.all(16.0),
                                    child: Text(
                                      'Error loading video: $_errorMessage',
                                      textAlign: TextAlign.center,
                                      style: const TextStyle(fontSize: 12),
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          )
                        : _isInitialized
                            ? GestureDetector(
                                onTap: () {
                                  setState(() {
                                    _videoController.value.isPlaying
                                        ? _videoController.pause()
                                        : _videoController.play();
                                  });
                                },
                                child: Stack(
                                  alignment: Alignment.center,
                                  children: [
                                    Container(
                                      color: Colors.black,
                                      child: AspectRatio(
                                        aspectRatio: _videoController.value.aspectRatio,
                                        child: VideoPlayer(_videoController),
                                      ),
                                    ),
                                    if (!_videoController.value.isPlaying)
                                      CircleAvatar(
                                        radius: 30,
                                        backgroundColor: const Color(0xFF0056D2).withOpacity(0.9),
                                        child: const Icon(Icons.play_arrow, color: Colors.white, size: 40),
                                      ),
                                  ],
                                ),
                              )
                            : Container(
                                height: 200,
                                color: Colors.grey.shade300,
                                child: const Center(
                                  child: CircularProgressIndicator(),
                                ),
                              ),
                  ),
                ],
              ),
            ),
            
            const SizedBox(height: 20),

            // Lesson Header
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Expanded(
                        child: Text(
                          widget.title,
                          style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold),
                        ),
                      ),
                      const SizedBox(width: 10),
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                        decoration: BoxDecoration(
                          color: const Color(0xFF2196F3),
                          borderRadius: BorderRadius.circular(5),
                        ),
                        child: const Text(
                          'Lesson 1.1',
                          style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 10),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 15),
                  Text(
                    widget.description,
                    style: const TextStyle(color: Colors.grey, fontSize: 15, height: 1.5),
                  ),
                  
                  const SizedBox(height: 40),

                  // Reattempt Quiz Card
                  Container(
                    width: double.infinity,
                    padding: const EdgeInsets.all(25),
                    decoration: BoxDecoration(
                      color: const Color(0xFFF8F9FE),
                      borderRadius: BorderRadius.circular(15),
                      border: Border.all(color: const Color(0xFFE8F1FF)),
                    ),
                    child: Column(
                      children: [
                        CircleAvatar(
                          radius: 25,
                          backgroundColor: const Color(0xFFE8F1FF),
                          child: Icon(Icons.school_outlined, color: Colors.blue.shade800),
                        ),
                        const SizedBox(height: 15),
                        const Text(
                          'You can now reattempt the quiz',
                          style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                        ),
                        const SizedBox(height: 5),
                        const Text(
                          'Force 1.1',
                          style: TextStyle(color: Colors.grey),
                        ),
                        const SizedBox(height: 25),
                        SizedBox(
                          width: double.infinity,
                          height: 55,
                          child: ElevatedButton(
                            onPressed: () {
                              Navigator.pushNamed(context, '/lesson-quizzes');
                            },
                            style: ElevatedButton.styleFrom(
                              backgroundColor: const Color(0xFF2196F3),
                              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                              elevation: 0,
                            ),
                            child: const Row(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                Icon(Icons.refresh, color: Colors.white),
                                SizedBox(width: 10),
                                Text(
                                  'Reattempt Quiz',
                                  style: TextStyle(color: Colors.white, fontSize: 16, fontWeight: FontWeight.bold),
                                ),
                              ],
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 30),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
