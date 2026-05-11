import "package:flutter/material.dart";
import "dart:math" as math;

class ImagePreviewConfirmationScreen extends StatelessWidget {
  const ImagePreviewConfirmationScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF7F7F7),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: 2,
        selectedItemColor: Colors.black,
        unselectedItemColor: Colors.grey,
        onTap: (index) {
          if (index == 3) {
            Navigator.pushNamed(context, "/profile");
          }
        },
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.home_outlined),
            label: "Home",
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.menu_book_outlined),
            label: "Lessons",
          ),
          BottomNavigationBarItem(
            icon: CircleAvatar(
              radius: 22,
              backgroundColor: Color(0xFFDDE4F2),
              child: Icon(Icons.science_outlined, color: Colors.black),
            ),
            label: "Labs",
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.person_outline),
            label: "Profile",
          ),
        ],
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
          child: Column(
            children: [
              Row(
                children: [
                  IconButton(
                    onPressed: () => Navigator.pop(context),
                    icon: const Icon(
                      Icons.arrow_back_ios_new,
                      color: Colors.blue,
                    ),
                  ),
                  const Expanded(
                    child: Center(
                      child: Text(
                        "Physics Lab",
                        style: TextStyle(
                          fontSize: 28,
                          fontWeight: FontWeight.bold,
                          color: Color(0xFF0057B8),
                        ),
                      ),
                    ),
                  ),
                  Padding(
                    padding: const EdgeInsets.only(right: 16),
                    child: GestureDetector(
                      onTap: () => Navigator.pushNamed(context, "/profile"),
                      child: const CircleAvatar(
                        radius: 18,
                        backgroundColor: Color(0xFFCCCCCC),
                        child: Icon(Icons.person, color: Colors.white, size: 22),
                      ),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 30),
              const Text(
                "CONFIRM CAPTURE",
                style: TextStyle(
                  fontSize: 26,
                  fontWeight: FontWeight.bold,
                  letterSpacing: 1,
                  color: Colors.black54,
                ),
              ),
              const SizedBox(height: 10),
              const Text(
                "Review your handwritten equation before solving.",
                textAlign: TextAlign.center,
                style: TextStyle(
                  fontSize: 17,
                  color: Colors.black87,
                ),
              ),
              const SizedBox(height: 30),
              Container(
                width: double.infinity,
                height: 460,
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(32),
                  border: Border.all(color: const Color(0xFFD3D8E2)),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withOpacity(0.06),
                      blurRadius: 10,
                      offset: const Offset(0, 4),
                    ),
                  ],
                ),
                child: Stack(
                  children: [
                    const Positioned(
                      top: 24,
                      left: 24,
                      child: _Corner(),
                    ),
                    Positioned(
                      top: 24,
                      right: 24,
                      child: Transform.rotate(
                        angle: math.pi / 2,
                        child: const _Corner(),
                      ),
                    ),
                    Positioned(
                      bottom: 24,
                      left: 24,
                      child: Transform.rotate(
                        angle: -math.pi / 2,
                        child: const _Corner(),
                      ),
                    ),
                    Positioned(
                      bottom: 24,
                      right: 24,
                      child: Transform.rotate(
                        angle: math.pi,
                        child: const _Corner(),
                      ),
                    ),
                    Center(
                      child: ClipOval(
                        child: Image.asset(
                          "assets/images/uploaded_answer.png",
                          width: 360,
                          height: 360,
                          fit: BoxFit.cover,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 35),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Expanded(
                    child: OutlinedButton.icon(
                      onPressed: () => Navigator.pushNamed(
                        context,
                        "/upload-image",
                      ),
                      icon: const Icon(Icons.edit_outlined),
                      label: const Text("Edit"),
                      style: OutlinedButton.styleFrom(
                        foregroundColor: Colors.blue,
                        side: const BorderSide(color: Color(0xFF7B8AA0)),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(40),
                        ),
                        padding: const EdgeInsets.symmetric(vertical: 18),
                      ),
                    ),
                  ),
                  const SizedBox(width: 20),
                  Expanded(
                    child: ElevatedButton.icon(
                      onPressed: () => Navigator.pushNamed(
                        context,
                        "/comparison-answer",
                      ),
                      icon: const Icon(Icons.check_circle_outline),
                      label: const Text("OK"),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: const Color(0xFF2F91E8),
                        foregroundColor: Colors.black,
                        elevation: 4,
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(40),
                        ),
                        padding: const EdgeInsets.symmetric(vertical: 18),
                      ),
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _Corner extends StatelessWidget {
  const _Corner();

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: 32,
      height: 32,
      child: CustomPaint(
        painter: CornerPainter(),
      ),
    );
  }
}

class CornerPainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = const Color(0xFF7BA7D9)
      ..strokeWidth = 3
      ..style = PaintingStyle.stroke;

    final path = Path()
      ..moveTo(0, size.height)
      ..lineTo(0, 0)
      ..lineTo(size.width, 0);

    canvas.drawPath(path, paint);
  }

  @override
  bool shouldRepaint(CustomPainter oldDelegate) => false;
}
