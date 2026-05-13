import "package:flutter/material.dart";

import "../upload-image/upload_image_screen.dart";

class StepByStepSolutionScreen extends StatelessWidget {
  const StepByStepSolutionScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F5F7),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 20),
          child: Column(
            children: [
              const _HeaderBar(),
              const SizedBox(height: 20),
              Expanded(
                child: SingleChildScrollView(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text(
                        "Kinetic Energy Derivation",
                        style: TextStyle(
                          fontSize: 34,
                          fontWeight: FontWeight.bold,
                          color: Colors.black87,
                        ),
                      ),
                      const SizedBox(height: 12),
                      const Text(
                        "Deriving the relationship between work and energy from first principles.",
                        style: TextStyle(
                          fontSize: 18,
                          height: 1.5,
                          color: Colors.black54,
                        ),
                      ),
                      const SizedBox(height: 28),
                      buildStepCard(
                        step: "Step 1",
                        description:
                            "Start with the definition of work done by a constant force:",
                        formula: "W = F \\cdot d",
                        showInfo: true,
                      ),
                      const SizedBox(height: 18),
                      buildStepCard(
                        step: "Step 2",
                        description:
                            "Substitute Newton's Second Law (F = ma) into the work equation:",
                        formula: "W = (ma) \\cdot d",
                      ),
                      const SizedBox(height: 18),
                      buildStepCard(
                        step: "Step 3",
                        description:
                            "Use the kinematic equation for displacement under constant acceleration:",
                        formula:
                            "v_f^2 = v_i^2 + 2ad \\implies d = \\frac{v_f^2 - v_i^2}{2a}",
                      ),
                      const SizedBox(height: 18),
                      buildStepCard(
                        step: "Step 4",
                        description:
                            "Substitute the expression for d back into the work equation:",
                        formula:
                            "W = m \\cdot a \\cdot \\left( \\frac{v_f^2 - v_i^2}{2a} \\right)",
                      ),
                      const SizedBox(height: 18),
                      buildStepCard(
                        step: "Step 5",
                        description: "Simplify by canceling a and distributing m:",
                        formula: "W = \\frac{1}{2}mv_f^2 - \\frac{1}{2}mv_i^2",
                      ),
                      const SizedBox(height: 40),
                      SizedBox(
                        width: double.infinity,
                        height: 60,
                        child: ElevatedButton(
                            onPressed: () {
                              Navigator.of(context).push(
                                MaterialPageRoute(
                                  builder: (context) =>
                                      const UploadImageScreen(),
                                ),
                              );
                            },
                          style: ElevatedButton.styleFrom(
                            backgroundColor: Colors.blue,
                            elevation: 5,
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(30),
                            ),
                          ),
                          child: const Row(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              Icon(Icons.compare_arrows, color: Colors.white),
                              SizedBox(width: 10),
                              Text(
                                "Compare",
                                style: TextStyle(
                                  color: Colors.white,
                                  fontSize: 20,
                                  fontWeight: FontWeight.w600,
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
                      const SizedBox(height: 30),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: 1,
        type: BottomNavigationBarType.fixed,
        selectedItemColor: Colors.blue,
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
            icon: Icon(Icons.science_outlined),
            label: "Labs",
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.person_outline),
            label: "Profile",
          ),
        ],
      ),
    );
  }
}

class _HeaderBar extends StatelessWidget {
  const _HeaderBar();

  @override
  Widget build(BuildContext context) {
    return Row(
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
              "Step-by-Step Solution",
              style: TextStyle(
                fontSize: 24,
                fontWeight: FontWeight.bold,
                color: Colors.blue,
              ),
            ),
          ),
        ),
        const SizedBox(width: 48),
      ],
    );
  }
}

Widget buildStepCard({
  required String step,
  required String description,
  required String formula,
  bool showInfo = false,
}) {
  return Container(
    padding: const EdgeInsets.all(18),
    decoration: BoxDecoration(
      color: Colors.white,
      borderRadius: BorderRadius.circular(18),
      border: Border.all(color: const Color(0xFFD5DBE7)),
      boxShadow: [
        BoxShadow(
          blurRadius: 8,
          offset: const Offset(0, 4),
          color: Colors.black.withOpacity(0.03),
        ),
      ],
    ),
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              step,
              style: const TextStyle(
                fontSize: 22,
                fontWeight: FontWeight.bold,
                color: Colors.blue,
              ),
            ),
            if (showInfo)
              const Icon(
                Icons.info_outline,
                color: Colors.grey,
              ),
          ],
        ),
        const SizedBox(height: 16),
        Text(
          description,
          style: const TextStyle(
            fontSize: 18,
            height: 1.5,
            color: Colors.black87,
          ),
        ),
        const SizedBox(height: 18),
        Container(
          width: double.infinity,
          padding: const EdgeInsets.symmetric(vertical: 18, horizontal: 12),
          decoration: BoxDecoration(
            color: const Color(0xFFF4F4F6),
            borderRadius: BorderRadius.circular(14),
          ),
          child: Center(
            child: Text(
              formula,
              textAlign: TextAlign.center,
              style: const TextStyle(
                fontSize: 26,
                fontStyle: FontStyle.italic,
                color: Colors.black87,
              ),
            ),
          ),
        ),
      ],
    ),
  );
}
