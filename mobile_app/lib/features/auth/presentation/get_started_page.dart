import 'package:firebase_auth/firebase_auth.dart';
import 'package:flutter/material.dart';

class GetStartedPage extends StatefulWidget {
  const GetStartedPage({Key? key}) : super(key: key);

  @override
  State<GetStartedPage> createState() => _GetStartedPageState();
}

class _GetStartedPageState extends State<GetStartedPage> {
  @override
  void initState() {
    super.initState();
    if (FirebaseAuth.instance.currentUser != null) {
      WidgetsBinding.instance.addPostFrameCallback((_) {
        if (!mounted) return;
        Navigator.of(context).pushReplacementNamed('/home');
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      body: SafeArea(
        child: SingleChildScrollView(
          child: Column(
            children: [
              // Top decorative icons
              Stack(
                children: [
                  SizedBox(height: 60),
                  Positioned(
                    top: 10,
                    right: 20,
                    child: Icon(
                      Icons.all_inclusive,
                      color: Colors.blue.shade300,
                      size: 32,
                    ),
                  ),
                  Positioned(
                    top: 5,
                    left: 30,
                    child: Icon(
                      Icons.science_outlined,
                      color: Colors.blue.shade400,
                      size: 28,
                    ),
                  ),
                ],
              ),
              // Main content
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 24.0),
                child: Column(
                  children: [
                    // Title
                    Text(
                      'Learn\nPhysics Anytime,\nAnywhere',
                      style: Theme.of(context).textTheme.headlineLarge?.copyWith(
                            fontSize: 40,
                            fontWeight: FontWeight.bold,
                            color: Colors.blue.shade600,
                            height: 1.3,
                          ),
                      textAlign: TextAlign.left,
                    ),
                    SizedBox(height: 7),

                    // Subtitle
                    Text(
                      'Perform virtual experiments, test your knowledge, and improve with personalized AI guidance',
                      style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                            fontSize: 18,
                            color: Colors.grey.shade700,
                            height: 1.5,
                          ),
                      textAlign: TextAlign.left,
                    ),
                    SizedBox(height:5),
                  ],
                ),
              ),

              // Decorative section with icons and illustration
              Stack(
                children: [
                  // Background circle decoration
                  Positioned(
                    right: -50,
                    top: 50,
                    child: Container(
                      width: 200,
                      height: 200,
                      decoration: BoxDecoration(
                        shape: BoxShape.circle,
                        color: Colors.blue.shade50,
                      ),
                    ),
                  ),

                  // Main content area
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 24.0),
                    child: Column(
                      children: [
                        // Decorative icons row 1
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            _buildDecorativeIcon(
                              icon: Icons.bubble_chart_outlined,
                              color: Colors.blue.shade400,
                              size: 40,
                            ),
                            Spacer(),
                            _buildDecorativeIcon(
                              icon: Icons.power_settings_new,
                              color: Colors.blue.shade300,
                              size: 36,
                            ),
                          ],
                        ),
                        SizedBox(height: 5),

                        // Central illustration area - Display image
                        Container(
                          width: 280,
                          height: 280,
                          decoration: BoxDecoration(
                            borderRadius: BorderRadius.circular(20),
                            color: Colors.blue.shade50,
                          ),
                          child: ClipRRect(
                            borderRadius: BorderRadius.circular(20),
                            child: Image.asset(
                              'assets/images/profile_illustration.png',
                              fit: BoxFit.cover,
                              errorBuilder: (context, error, stackTrace) {
                                return Center(
                                  child: Icon(
                                    Icons.image_not_supported,
                                    size: 60,
                                    color: Colors.grey.shade400,
                                  ),
                                );
                              },
                            ),
                          ),
                        ),
                        SizedBox(height: 5),

                        // Decorative icons row 2
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            _buildDecorativeIcon(
                              icon: Icons.science,
                              color: Colors.blue.shade400,
                              size: 32,
                            ),
                            Spacer(),
                            _buildDecorativeIcon(
                              icon: Icons.psychology_outlined,
                              color: const Color.fromARGB(255, 203, 96, 209),
                              size: 36,
                            ),
                          ],
                        ),
                        SizedBox(height: 5),
                      ],
                    ),
                  ),
                ],
              ),

              // Get Started Button
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 24.0, vertical: 10),
                child: SizedBox(
                  width: double.infinity,
                  child: ElevatedButton.icon(
                    onPressed: () {
                      Navigator.of(context).pushNamed('/login');
                    },
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.blue.shade500,
                      padding: const EdgeInsets.symmetric(
                        horizontal: 32,
                        vertical: 16,
                      ),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(30),
                      ),
                      elevation: 4,
                    ),
                    icon: const Icon(
                      Icons.arrow_forward,
                      color: Colors.white,
                      size: 20,
                    ),
                    label: Text(
                      'Click to get started',
                      style: Theme.of(context).textTheme.labelLarge?.copyWith(
                            color: Colors.white,
                            fontSize: 16,
                            fontWeight: FontWeight.w600,
                          ),
                    ),
                  ),
                ),
              ),
              SizedBox(height: 20),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildDecorativeIcon({
    required IconData icon,
    required Color color,
    required double size,
  }) {
    return Container(
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        color: color.withOpacity(0.15),
      ),
      padding: const EdgeInsets.all(8),
      child: Icon(
        icon,
        color: color,
        size: size,
      ),
    );
  }
}
