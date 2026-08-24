import 'package:flutter/material.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import 'admin_lessons_screen.dart';
import 'admin_students_screen.dart';
import 'admin_analytics_screen.dart';
import 'screens/upload_materials_screen.dart';

class AdminDashboard extends StatefulWidget {
  const AdminDashboard({super.key});

  @override
  State<AdminDashboard> createState() => _AdminDashboardState();
}

class _AdminDashboardState extends State<AdminDashboard> {
  int _selectedIndex = 0;

  final List<Widget> _screens = [
    const _AdminHomeOverview(),
    const AdminLessonsScreen(),
    const AdminStudentsScreen(),
    const UploadMaterialsScreen(),
    const AdminAnalyticsScreen(),
  ];

  final List<NavigationDestination> _destinations = const [
    NavigationDestination(icon: Icon(Icons.dashboard_outlined), selectedIcon: Icon(Icons.dashboard), label: 'Dashboard'),
    NavigationDestination(icon: Icon(Icons.menu_book_outlined), selectedIcon: Icon(Icons.menu_book), label: 'Lessons'),
    NavigationDestination(icon: Icon(Icons.people_outline), selectedIcon: Icon(Icons.people), label: 'Students'),
    NavigationDestination(icon: Icon(Icons.cloud_upload_outlined), selectedIcon: Icon(Icons.cloud_upload), label: 'Materials'),
    NavigationDestination(icon: Icon(Icons.bar_chart_outlined), selectedIcon: Icon(Icons.bar_chart), label: 'Analytics'),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Row(
          children: [
            Icon(Icons.admin_panel_settings, color: Color(0xFF1A3CBA)),
            SizedBox(width: 8),
            Text('PhysicsLab Admin', style: TextStyle(fontWeight: FontWeight.bold)),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () async {
              await FirebaseAuth.instance.signOut();
              if (mounted) {
                Navigator.of(context).pushReplacementNamed('/get-started');
              }
            },
            tooltip: 'Logout',
          ),
        ],
      ),
      body: _screens[_selectedIndex],
      bottomNavigationBar: NavigationBar(
        selectedIndex: _selectedIndex,
        onDestinationSelected: (i) => setState(() => _selectedIndex = i),
        destinations: _destinations,
        backgroundColor: Colors.white,
        indicatorColor: const Color(0xFF1A3CBA).withOpacity(0.1),
      ),
    );
  }
}

class _AdminHomeOverview extends StatefulWidget {
  const _AdminHomeOverview();

  @override
  State<_AdminHomeOverview> createState() => _AdminHomeOverviewState();
}

class _AdminHomeOverviewState extends State<_AdminHomeOverview> {
  final _studentsScrollCtrl = ScrollController();

  @override
  void dispose() {
    _studentsScrollCtrl.dispose();
    super.dispose();
  }

  Future<int> _count(String collection, {String? field, String? value}) async {
    Query q = FirebaseFirestore.instance.collection(collection);
    if (field != null && value != null) q = q.where(field, isEqualTo: value);
    final snap = await q.count().get();
    return snap.count ?? 0;
  }

  Future<int> _countGroup(String group) async {
    final snap = await FirebaseFirestore.instance.collectionGroup(group).count().get();
    return snap.count ?? 0;
  }

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Overview', style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold)),
          const SizedBox(height: 20),

          GridView.count(
            crossAxisCount: 2,
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            crossAxisSpacing: 12,
            mainAxisSpacing: 12,
            childAspectRatio: 1.6,
            children: [
              _statCard('Students', Icons.people, Colors.blue,
                  _count('users', field: 'role', value: 'student')),
              _statCard('Lessons', Icons.menu_book, Colors.green,
                  _count('lessons')),
              _statCard('Quiz Attempts', Icons.quiz, Colors.orange,
                  _countGroup('quizAttempts')),
              _statCard('Weak Topics', Icons.warning_amber, Colors.red,
                  _countGroup('weakTopics')),
            ],
          ),
          const SizedBox(height: 24),

          const Text('Recent Students', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
          const SizedBox(height: 12),
          StreamBuilder<QuerySnapshot>(
            stream: FirebaseFirestore.instance
                .collection('users')
                .where('role', isEqualTo: 'student')
                .limit(20)
                .snapshots(),
            builder: (context, snap) {
              if (!snap.hasData) return const CircularProgressIndicator();
              if (snap.data!.docs.isEmpty) return const Text('No students yet.');
              return SizedBox(
                height: 320,
                child: Scrollbar(
                  controller: _studentsScrollCtrl,
                  thumbVisibility: true,
                  child: SingleChildScrollView(
                    controller: _studentsScrollCtrl,
                    child: Column(
                      children: snap.data!.docs.map((doc) {
                        final d = doc.data() as Map<String, dynamic>;
                        return Card(
                          margin: const EdgeInsets.only(bottom: 8),
                          child: ListTile(
                            leading: CircleAvatar(
                              backgroundColor: const Color(0xFF1A3CBA).withOpacity(0.1),
                              child: Text(
                                (d['fullName'] ?? 'S').toString().substring(0, 1).toUpperCase(),
                                style: const TextStyle(color: Color(0xFF1A3CBA), fontWeight: FontWeight.bold),
                              ),
                            ),
                            title: Text(d['fullName'] ?? '-'),
                            subtitle: Text(d['email'] ?? '-'),
                            trailing: Chip(
                              label: Text(d['grade']?.toString() ?? '-', style: const TextStyle(fontSize: 12)),
                              backgroundColor: const Color(0xFF1A3CBA).withOpacity(0.1),
                            ),
                          ),
                        );
                      }).toList(),
                    ),
                  ),
                ),
              );
            },
          ),
        ],
      ),
    );
  }

  Widget _statCard(String title, IconData icon, Color color, Future<int> future) {
    return FutureBuilder<int>(
      future: future,
      builder: (context, snap) => Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
          boxShadow: [BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 8)],
        ),
        child: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(10),
              decoration: BoxDecoration(
                color: color.withOpacity(0.1),
                borderRadius: BorderRadius.circular(10),
              ),
              child: Icon(icon, color: color, size: 22),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(snap.data?.toString() ?? '...',
                      style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold),
                      overflow: TextOverflow.ellipsis),
                  Text(title, style: TextStyle(color: Colors.grey.shade600, fontSize: 12),
                      overflow: TextOverflow.ellipsis),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
