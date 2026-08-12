import 'package:flutter/material.dart';
import 'package:cloud_firestore/cloud_firestore.dart';

class AdminStudentsScreen extends StatefulWidget {
  const AdminStudentsScreen({super.key});

  @override
  State<AdminStudentsScreen> createState() => _AdminStudentsScreenState();
}

class _AdminStudentsScreenState extends State<AdminStudentsScreen> {
  String _searchQuery = '';
  String _selectedGrade = 'All';

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            children: [
              TextField(
                decoration: InputDecoration(
                  hintText: 'Search by name or email...',
                  prefixIcon: const Icon(Icons.search),
                  border: OutlineInputBorder(borderRadius: BorderRadius.circular(10)),
                  filled: true,
                  fillColor: Colors.white,
                  contentPadding: const EdgeInsets.symmetric(vertical: 10),
                ),
                onChanged: (v) => setState(() => _searchQuery = v.toLowerCase()),
              ),
              const SizedBox(height: 10),
              Row(
                children: [
                  const Text('Grade: ', style: TextStyle(fontWeight: FontWeight.w600)),
                  const SizedBox(width: 8),
                  DropdownButton<String>(
                    value: _selectedGrade,
                    items: ['All', 'Grade 9', 'Grade 10', 'Grade 11']
                        .map((g) => DropdownMenuItem(value: g, child: Text(g)))
                        .toList(),
                    onChanged: (v) => setState(() => _selectedGrade = v!),
                  ),
                ],
              ),
            ],
          ),
        ),
        Expanded(
          child: StreamBuilder<QuerySnapshot>(
            stream: FirebaseFirestore.instance
                .collection('users')
                .where('role', isEqualTo: 'student')
                .snapshots(),
            builder: (context, snapshot) {
              if (!snapshot.hasData) {
                return const Center(child: CircularProgressIndicator());
              }
              final docs = snapshot.data!.docs.where((doc) {
                final d = doc.data() as Map<String, dynamic>;
                final name  = (d['fullName'] ?? '').toString().toLowerCase();
                final email = (d['email'] ?? '').toString().toLowerCase();
                final grade = (d['grade'] ?? '').toString();
                final matchSearch = _searchQuery.isEmpty ||
                    name.contains(_searchQuery) || email.contains(_searchQuery);
                final matchGrade  = _selectedGrade == 'All' || grade == _selectedGrade;
                return matchSearch && matchGrade;
              }).toList();

              if (docs.isEmpty) {
                return const Center(child: Text('No students found.'));
              }

              return ListView.builder(
                padding: const EdgeInsets.symmetric(horizontal: 16),
                itemCount: docs.length,
                itemBuilder: (ctx, i) {
                  final d = docs[i].data() as Map<String, dynamic>;
                  final uid = docs[i].id;
                  final initials = (d['fullName'] ?? 'S').toString().substring(0, 1).toUpperCase();
                  return Card(
                    margin: const EdgeInsets.only(bottom: 10),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                    child: ListTile(
                      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                      leading: CircleAvatar(
                        backgroundColor: const Color(0xFF1A3CBA).withOpacity(0.1),
                        child: Text(initials,
                            style: const TextStyle(color: Color(0xFF1A3CBA), fontWeight: FontWeight.bold)),
                      ),
                      title: Text(d['fullName'] ?? '-',
                          style: const TextStyle(fontWeight: FontWeight.bold)),
                      subtitle: Text('${d['email'] ?? '-'}\n${d['grade'] ?? '-'}'),
                      isThreeLine: true,
                      trailing: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                            decoration: BoxDecoration(
                              color: Colors.green.withOpacity(0.1),
                              borderRadius: BorderRadius.circular(20),
                            ),
                            child: Text(d['status'] ?? 'active',
                                style: const TextStyle(color: Colors.green, fontSize: 11)),
                          ),
                          const SizedBox(height: 4),
                          GestureDetector(
                            onTap: () => _showStudentDetail(uid, d),
                            child: const Text('View',
                                style: TextStyle(color: Color(0xFF1A3CBA), fontSize: 12)),
                          ),
                        ],
                      ),
                    ),
                  );
                },
              );
            },
          ),
        ),
      ],
    );
  }

  void _showStudentDetail(String uid, Map<String, dynamic> data) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
          borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => DraggableScrollableSheet(
        expand: false,
        initialChildSize: 0.75,
        maxChildSize: 0.95,
        builder: (_, controller) => SingleChildScrollView(
          controller: controller,
          padding: const EdgeInsets.all(20),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Center(
                child: Container(width: 40, height: 4,
                    margin: const EdgeInsets.only(bottom: 16),
                    decoration: BoxDecoration(color: Colors.grey.shade300,
                        borderRadius: BorderRadius.circular(2))),
              ),
              Row(children: [
                CircleAvatar(
                  radius: 24,
                  backgroundColor: const Color(0xFF1A3CBA).withOpacity(0.1),
                  child: Text(
                    (data['fullName'] ?? 'S').toString().substring(0, 1).toUpperCase(),
                    style: const TextStyle(fontSize: 20, color: Color(0xFF1A3CBA), fontWeight: FontWeight.bold),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    Text(data['fullName'] ?? '-',
                        style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                    Text(data['email'] ?? '-',
                        style: TextStyle(color: Colors.grey.shade600, fontSize: 13)),
                    Text('${data['grade'] ?? '-'} • ${data['status'] ?? 'active'}',
                        style: TextStyle(color: Colors.grey.shade500, fontSize: 12)),
                  ]),
                ),
              ]),
              const Divider(height: 28),

              const Text('Quiz Attempts', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
              const SizedBox(height: 8),
              StreamBuilder<QuerySnapshot>(
                stream: FirebaseFirestore.instance
                    .collection('users').doc(uid)
                    .collection('quizAttempts')
                    .limit(5).snapshots(),
                builder: (_, snap) {
                  if (!snap.hasData) return const CircularProgressIndicator();
                  if (snap.data!.docs.isEmpty) return const Text('No quiz attempts yet.');
                  return Column(
                    children: snap.data!.docs.map((d) {
                      final a = d.data() as Map<String, dynamic>;
                      final score = a['score'] ?? 0;
                      return Card(
                        margin: const EdgeInsets.only(bottom: 6),
                        child: ListTile(
                          leading: CircleAvatar(
                            backgroundColor: score >= 70 ? Colors.green.withOpacity(0.1) : Colors.red.withOpacity(0.1),
                            child: Text('$score%',
                                style: TextStyle(color: score >= 70 ? Colors.green : Colors.red, fontSize: 12)),
                          ),
                          title: Text('Quiz: ${a['quizId'] ?? '-'}'),
                          subtitle: Text(score >= 70 ? 'Pass' : 'Fail'),
                        ),
                      );
                    }).toList(),
                  );
                },
              ),

              const SizedBox(height: 16),
              const Text('Weak Topics', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
              const SizedBox(height: 8),
              StreamBuilder<QuerySnapshot>(
                stream: FirebaseFirestore.instance
                    .collection('users').doc(uid)
                    .collection('weakTopics')
                    .limit(10).snapshots(),
                builder: (_, snap) {
                  if (!snap.hasData) return const CircularProgressIndicator();
                  if (snap.data!.docs.isEmpty) return const Text('No weak topics identified yet.');
                  return Wrap(
                    spacing: 8, runSpacing: 8,
                    children: snap.data!.docs.map((d) {
                      final t = d.data() as Map<String, dynamic>;
                      return Chip(
                        label: Text(t['lessonTag'] ?? '-'),
                        backgroundColor: Colors.red.withOpacity(0.1),
                        labelStyle: const TextStyle(color: Colors.red, fontSize: 12),
                      );
                    }).toList(),
                  );
                },
              ),
              const SizedBox(height: 20),
            ],
          ),
        ),
      ),
    );
  }
}
