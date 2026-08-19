import 'package:flutter/material.dart';

void main() => runApp(const MaterialApp(home: LessonDetailScreen(title: 'Preview', description: 'Preview description')));

class LessonDetailScreen extends StatelessWidget {
  final String title;
  final String description;

  const LessonDetailScreen({
    super.key,
    required this.title,
    required this.description,
  });

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
            // Video Section
            Stack(
              alignment: Alignment.center,
              children: [
                Container(
                  height: 300,
                  width: double.infinity,
                  color: const Color(0xFF0D1B2A), // Dark background for video area
                  child: Center(
                    child: Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 20.0),
                      child: ClipRRect(
                        borderRadius: BorderRadius.circular(15),
                        child: Image.asset(
                          'assets/images/video.jpg', // Updated to match actual asset extension
                          fit: BoxFit.cover,
                          height: 200,
                          width: double.infinity,
                        ),
                      ),
                    ),
                  ),
                ),
                // Play Button Overlay
                CircleAvatar(
                  radius: 30,
                  backgroundColor: const Color(0xFF0056D2).withOpacity(0.9),
                  child: const Icon(Icons.play_arrow, color: Colors.white, size: 40),
                ),
                // Custom Carousel Indicator at bottom of video
                Positioned(
                  bottom: 15,
                  child: Row(
                    children: [
                      Container(width: 40, height: 3, color: Colors.white.withOpacity(0.5)),
                      const SizedBox(width: 5),
                      Container(width: 40, height: 3, color: Colors.white.withOpacity(0.2)),
                    ],
                  ),
                ),
              ],
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
                          title,
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
                    description,
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
