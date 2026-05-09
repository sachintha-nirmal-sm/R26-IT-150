import 'package:flutter/material.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  int _selectedIndex = 3;

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
          'Profile',
          style: TextStyle(color: Color(0xFF1A1C1E), fontWeight: FontWeight.bold),
        ),
        centerTitle: true,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // User Info Card with Profile Pic on Left (No Picker)
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(15),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(20),
                border: Border.all(color: Colors.grey.shade200),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.03),
                    blurRadius: 10,
                    offset: const Offset(0, 4),
                  ),
                ],
              ),
              child: Row(
                children: [
                  Container(
                    padding: const EdgeInsets.all(3),
                    decoration: BoxDecoration(
                      shape: BoxShape.circle,
                      border: Border.all(color: const Color(0xFF2196F3).withOpacity(0.2), width: 2),
                    ),
                    child: const CircleAvatar(
                      radius: 35,
                      backgroundColor: Color(0xFFE8F1FF),
                      backgroundImage: NetworkImage('https://i.pravatar.cc/150?u=alex'),
                    ),
                  ),
                  const SizedBox(width: 20),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Text(
                          'Alex Johnson',
                          style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
                        ),
                        const Text(
                          'alex.j@example.edu',
                          style: TextStyle(color: Colors.grey, fontSize: 14),
                        ),
                        const SizedBox(height: 8),
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                          decoration: BoxDecoration(
                            color: const Color(0xFFE8F1FF),
                            borderRadius: BorderRadius.circular(20),
                          ),
                          child: const Text(
                            'Grade 9',
                            style: TextStyle(color: Color(0xFF0056D2), fontSize: 12, fontWeight: FontWeight.bold),
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
            
            const SizedBox(height: 20),

            // Lesson Stats Cards
            Row(
              children: [
                Expanded(child: _buildStatCard('Completed', '1', Icons.check_circle, Colors.green)),
                const SizedBox(width: 12),
                Expanded(child: _buildStatCard('In Progress', '2', Icons.play_circle_filled, Colors.orange)),
                const SizedBox(width: 12),
                Expanded(child: _buildStatCard('Incomplete', '3', Icons.pause_circle_filled, Colors.red)),
              ],
            ),

            const SizedBox(height: 30),
            const Text(
              'Recent Progress',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 15),

            // Progress Items
            
            _buildListItem(Icons.more_horiz, "Linear motion", "Inprogress .60%", Colors.blue),
            _buildListItem(Icons.more_horiz, "Vectors & Scalars", "In Progress • 40%", Colors.blue),

            const SizedBox(height: 30),
            Row(
              children: [
                const Icon(Icons.warning_amber_rounded, color: Color(0xFFC62828), size: 20),
                const SizedBox(width: 8),
                const Text(
                  'Areas for Improvement',
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                ),
              ],
            ),
            const SizedBox(height: 15),

            // Improvement Items
            _buildListItem(Icons.priority_high, "Newton's Second Law", "Difficulty applying formula in..", Colors.red, isAlert: true),
            _buildListItem(Icons.priority_high, "Frictional Forces", "Confusion between static and kinetic", Colors.red, isAlert: true),
          ],
        ),
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: _selectedIndex,
        type: BottomNavigationBarType.fixed,
        selectedItemColor: Colors.blue,
        unselectedItemColor: Colors.grey,
        onTap: (index) {
          setState(() => _selectedIndex = index);
          switch (index) {
            case 0:
              Navigator.pushNamed(context, '/home');
              break;
            case 1:
              Navigator.pushNamed(context, '/lesson-list');
              break;
            case 2:
              Navigator.pushNamed(context, '/deep-learn');
              break;
            case 3:
              // Already on profile
              break;
          }
        },
        items: const [
          BottomNavigationBarItem(icon: Icon(Icons.home_outlined), label: 'Home'),
          BottomNavigationBarItem(icon: Icon(Icons.menu_book), label: 'Lessons'),
          BottomNavigationBarItem(icon: Icon(Icons.biotech_outlined), label: 'Labs'),
          BottomNavigationBarItem(icon: Icon(Icons.person), label: 'Profile'),
        ],
      ),
    );
  }

  Widget _buildListItem(IconData icon, String title, String subtitle, Color color, {bool isAlert = false}) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.grey.shade200),
      ),
      child: ListTile(
        leading: Container(
          padding: const EdgeInsets.all(8),
          decoration: BoxDecoration(
            color: color.withOpacity(0.1),
            shape: BoxShape.circle,
          ),
          child: Icon(icon, color: color, size: 20),
        ),
        title: Text(title, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
        subtitle: Text(subtitle, style: const TextStyle(color: Colors.grey)),
        trailing: const Icon(Icons.chevron_right, color: Colors.grey),
      ),
    );
  }

  Widget _buildStatCard(String label, String count, IconData icon, Color color) {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: Colors.grey.shade200),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.02),
            blurRadius: 10,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        children: [
          Icon(icon, color: color, size: 24),
          const SizedBox(height: 8),
          Text(
            count,
            style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 4),
          Text(
            label,
            textAlign: TextAlign.center,
            style: TextStyle(color: Colors.grey.shade600, fontSize: 11, fontWeight: FontWeight.w500),
          ),
        ],
      ),
    );
  }
}
