import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:http/http.dart' as http;
import 'package:intl/intl.dart';

import 'admin_backend_url.dart';

class AdminNotificationsScreen extends StatefulWidget {
  const AdminNotificationsScreen({super.key});

  @override
  State<AdminNotificationsScreen> createState() =>
      _AdminNotificationsScreenState();
}

class _AdminNotificationsScreenState extends State<AdminNotificationsScreen>
    with SingleTickerProviderStateMixin {
  static final String _backendUrl = adminBackendUrl;

  late final TabController _tabController;
  String _searchQuery = '';
  String _selectedGrade = 'All';
  final Set<String> _sendingUids = {};

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<String?> _getToken() async =>
      await FirebaseAuth.instance.currentUser?.getIdToken();

  /// Mirrors the backend's notification_profile_service._most_active_hour:
  /// argmax of users/{uid}.activityHourCounts, default 19 (7 PM) if empty.
  /// Hours are UTC (same clock the backend's hourly sweep runs on).
  String _nextSendLabel(Map<String, dynamic> d) {
    final counts = d['activityHourCounts'] as Map<String, dynamic>?;
    int hour = 19;
    if (counts != null && counts.isNotEmpty) {
      var best = -1;
      counts.forEach((k, v) {
        final c = (v is num) ? v.toInt() : 0;
        if (c > best) {
          best = c;
          hour = int.tryParse(k) ?? hour;
        }
      });
    }
    final period = hour >= 12 ? 'PM' : 'AM';
    final hour12 = hour % 12 == 0 ? 12 : hour % 12;
    return 'Next auto-send: ~$hour12:00 $period UTC';
  }

  Future<void> _sendPersonalized(String uid, String name) async {
    setState(() => _sendingUids.add(uid));
    try {
      final token = await _getToken();
      final response = await http
          .post(
            Uri.parse('$_backendUrl/admin/notifications/send'),
            headers: {
              'Authorization': 'Bearer $token',
              'Content-Type': 'application/json',
            },
            body: jsonEncode({'uid': uid}),
          )
          .timeout(const Duration(seconds: 15));

      if (!mounted) return;
      if (response.statusCode == 202) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Personalized notification queued for $name')),
        );
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Failed (${response.statusCode}).')),
        );
      }
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text('Error: $e')));
    } finally {
      if (mounted) setState(() => _sendingUids.remove(uid));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Container(
          color: Colors.white,
          child: TabBar(
            controller: _tabController,
            labelColor: const Color(0xFF1A3CBA),
            indicatorColor: const Color(0xFF1A3CBA),
            tabs: const [
              Tab(text: 'Send', icon: Icon(Icons.send_outlined)),
              Tab(text: 'History', icon: Icon(Icons.history)),
            ],
          ),
        ),
        Expanded(
          child: TabBarView(
            controller: _tabController,
            children: [
              _buildSendTab(),
              const _NotificationHistoryTab(),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildSendTab() {
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
                  border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(10)),
                  filled: true,
                  fillColor: Colors.white,
                  contentPadding: const EdgeInsets.symmetric(vertical: 10),
                ),
                onChanged: (v) =>
                    setState(() => _searchQuery = v.toLowerCase()),
              ),
              const SizedBox(height: 10),
              Row(
                children: [
                  const Text('Grade: ',
                      style: TextStyle(fontWeight: FontWeight.w600)),
                  const SizedBox(width: 8),
                  DropdownButton<String>(
                    value: _selectedGrade,
                    items: ['All', 'Grade 9', 'Grade 10', 'Grade 11']
                        .map((g) =>
                            DropdownMenuItem(value: g, child: Text(g)))
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
                final name = (d['fullName'] ?? '').toString().toLowerCase();
                final email = (d['email'] ?? '').toString().toLowerCase();
                final grade = (d['grade'] ?? '').toString();
                final matchSearch = _searchQuery.isEmpty ||
                    name.contains(_searchQuery) ||
                    email.contains(_searchQuery);
                final matchGrade =
                    _selectedGrade == 'All' || grade == _selectedGrade;
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
                  final nameStr = (d['fullName'] ?? '').toString();
                  final name = nameStr.isNotEmpty ? nameStr : 'this student';
                  final initials =
                      (nameStr.isNotEmpty ? nameStr[0] : 'S').toUpperCase();
                  final sending = _sendingUids.contains(uid);
                  // Plain Row/Column instead of ListTile: ListTile's
                  // title+subtitle layout reserves a fixed height even with
                  // isThreeLine, which doesn't actually fit a 2-line
                  // subtitle plus a long email reliably — causes a few
                  // pixels of bottom overflow. This sizes to its content.
                  return Card(
                    margin: const EdgeInsets.only(bottom: 10),
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12)),
                    child: Padding(
                      padding:
                          const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                      child: Row(
                        crossAxisAlignment: CrossAxisAlignment.center,
                        children: [
                          CircleAvatar(
                            backgroundColor:
                                const Color(0xFF1A3CBA).withOpacity(0.1),
                            child: Text(initials,
                                style: const TextStyle(
                                    color: Color(0xFF1A3CBA),
                                    fontWeight: FontWeight.bold)),
                          ),
                          const SizedBox(width: 12),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                Text(name,
                                    overflow: TextOverflow.ellipsis,
                                    style: const TextStyle(
                                        fontWeight: FontWeight.bold)),
                                const SizedBox(height: 2),
                                Text('${d['email'] ?? '-'}',
                                    overflow: TextOverflow.ellipsis,
                                    style: const TextStyle(fontSize: 13)),
                                const SizedBox(height: 2),
                                Text(
                                  _nextSendLabel(d),
                                  overflow: TextOverflow.ellipsis,
                                  style: TextStyle(
                                      fontSize: 11,
                                      color: Colors.grey.shade600,
                                      fontStyle: FontStyle.italic),
                                ),
                              ],
                            ),
                          ),
                          const SizedBox(width: 8),
                          sending
                              ? const SizedBox(
                                  width: 20,
                                  height: 20,
                                  child:
                                      CircularProgressIndicator(strokeWidth: 2),
                                )
                              : IconButton(
                                  onPressed: () =>
                                      _sendPersonalized(uid, name),
                                  icon: const Icon(Icons.send,
                                      color: Color(0xFF1A3CBA)),
                                  tooltip: 'Send personalized notification',
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
}

class _NotificationHistoryTab extends StatelessWidget {
  const _NotificationHistoryTab();

  Color _statusColor(String status) {
    switch (status) {
      case 'sent':
        return Colors.green;
      case 'failed':
        return Colors.red;
      case 'skipped':
        return Colors.grey;
      default:
        return Colors.orange;
    }
  }

  @override
  Widget build(BuildContext context) {
    return StreamBuilder<QuerySnapshot>(
      stream: FirebaseFirestore.instance
          .collection('notifications')
          .orderBy('createdAt', descending: true)
          .limit(100)
          .snapshots(),
      builder: (context, snapshot) {
        if (!snapshot.hasData) {
          return const Center(child: CircularProgressIndicator());
        }
        final docs = snapshot.data!.docs;
        if (docs.isEmpty) {
          return const Center(child: Text('No notifications sent yet.'));
        }
        return ListView.builder(
          padding: const EdgeInsets.all(16),
          itemCount: docs.length,
          itemBuilder: (ctx, i) {
            final d = docs[i].data() as Map<String, dynamic>;
            final status = (d['status'] ?? 'queued').toString();
            final createdAt = d['createdAt'] as Timestamp?;
            final timeLabel = createdAt != null
                ? DateFormat('MMM d, h:mm a').format(createdAt.toDate())
                : '-';
            // Plain Row/Column instead of ListTile — same reasoning as the
            // Send tab: ListTile's isThreeLine budget doesn't reliably fit
            // a wrapped AI-generated body plus the meta line underneath.
            return Card(
              margin: const EdgeInsets.only(bottom: 10),
              shape:
                  RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              child: Padding(
                padding:
                    const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Text(d['title'] ?? '(generating...)',
                              overflow: TextOverflow.ellipsis,
                              style:
                                  const TextStyle(fontWeight: FontWeight.bold)),
                          if (d['body'] != null) ...[
                            const SizedBox(height: 4),
                            Text(d['body'],
                                maxLines: 2, overflow: TextOverflow.ellipsis),
                          ],
                          const SizedBox(height: 4),
                          Text(
                            '${d['mood'] ?? '-'} · ${d['triggeredBy'] ?? '-'} · $timeLabel',
                            overflow: TextOverflow.ellipsis,
                            style: TextStyle(
                                color: Colors.grey.shade600, fontSize: 11),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(width: 8),
                    Container(
                      padding:
                          const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                      decoration: BoxDecoration(
                        color: _statusColor(status).withOpacity(0.1),
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: Text(status,
                          style: TextStyle(
                              color: _statusColor(status), fontSize: 11)),
                    ),
                  ],
                ),
              ),
            );
          },
        );
      },
    );
  }
}
