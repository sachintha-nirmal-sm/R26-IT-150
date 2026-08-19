import 'package:flutter/material.dart';
import 'video_view_screen.dart';

void main() => runApp(const MaterialApp(home: DeepLearningScreen()));

class DeepLearningScreen extends StatelessWidget {
  const DeepLearningScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FE),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Color(0xFF2196F3)),
          onPressed: () => Navigator.pop(context),
        ),
        title: const Text(
          'Deep Learning - Force',
          style: TextStyle(color: Color(0xFF1A1C1E), fontWeight: FontWeight.bold),
        ),
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
      body: ListView(
        padding: const EdgeInsets.all(16.0),
        children: const [
          VideoLessonCard(
            title: "Understanding Force & Motion",
            description: "An in-depth introduction to Newton's laws, exploring how unbalanced forces create...",
            duration: "12:45",
          ),
          VideoLessonCard(
            title: "Friction and Surface Resistance",
            description: "Examine the forces that oppose motion. Learn the difference between static and kinetic...",
            duration: "08:30",
          ),
          VideoLessonCard(
            title: "Gravity: The Invisible Pull",
            description: "Dive deep into gravitational force. Calculate weight, understand mass vs. gravity, and...",
            duration: "15:10",
          ),
        ],
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: 1,
        type: BottomNavigationBarType.fixed,
        selectedItemColor: Colors.blue,
        unselectedItemColor: Colors.grey,
        onTap: (index) {
          if (index == 3) {
            Navigator.pushNamed(context, '/profile');
          }
        },
        items: const [
          BottomNavigationBarItem(icon: Icon(Icons.home_outlined), label: 'Home'),
          BottomNavigationBarItem(icon: Icon(Icons.menu_book), label: 'Lessons'),
          BottomNavigationBarItem(icon: Icon(Icons.biotech_outlined), label: 'Labs'),
          BottomNavigationBarItem(icon: Icon(Icons.person_outline), label: 'Profile'),
        ],
      ),
    );
  }
}

class VideoLessonCard extends StatelessWidget {
  final String title;
  final String description;
  final String duration;

  const VideoLessonCard({
    super.key,
    required this.title,
    required this.description,
    required this.duration,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(15),
        border: Border.all(color: Colors.grey.shade200),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Thumbnail with Play Button & Timer
          Stack(
            alignment: Alignment.center,
            children: [
              ClipRRect(
                borderRadius: const BorderRadius.vertical(top: Radius.circular(15)),
                child: Image.asset(
                  'assets/images/video.jpg', // Using your specific asset path
                  height: 200,
                  width: double.infinity,
                  fit: BoxFit.cover,
                ),
              ),
              // Play Button Icon
              CircleAvatar(
                radius: 28,
                backgroundColor: Colors.white.withOpacity(0.9),
                child: const Icon(Icons.play_arrow, color: Color(0xFF0056D2), size: 35),
              ),
              // Duration Badge
              Positioned(
                bottom: 10,
                right: 10,
                child: Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(
                    color: Colors.black.withOpacity(0.7),
                    borderRadius: BorderRadius.circular(4),
                  ),
                  child: Text(
                    duration,
                    style: const TextStyle(color: Colors.white, fontSize: 12, fontWeight: FontWeight.bold),
                  ),
                ),
              ),
            ],
          ),
          
          // Content Section
          Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 8),
                Text(
                  description,
                  style: const TextStyle(color: Colors.grey, fontSize: 14, height: 1.4),
                ),
                const SizedBox(height: 15),
                // Start Video Button
                Align(
                  alignment: Alignment.centerRight,
                  child: ElevatedButton(
                    onPressed: () {
                      Navigator.push(
                        context,
                        MaterialPageRoute(
                          builder: (context) => LessonDetailScreen(
                            title: title,
                            description: description,
                          ),
                        ),
                      );
                    },
                    style: ElevatedButton.styleFrom(
                      backgroundColor: const Color(0xFF2196F3),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                      padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
                    ),
                    child: const Text(
                      'Start Video',
                      style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
