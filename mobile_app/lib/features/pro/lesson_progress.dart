import 'package:flutter/material.dart';

class NewtonModuleScreen extends StatelessWidget {
  final String lessonName;

  const NewtonModuleScreen({super.key, required this.lessonName});

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
        title: Text(
          lessonName, // Displays "Newton's Laws" or clicked lesson
          style: const TextStyle(color: Color(0xFF1A1C1E), fontWeight: FontWeight.bold),
        ),
        
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16),
            child: GestureDetector(
              onTap: () => Navigator.pushNamed(context, '/profile'),
              child: const CircleAvatar(
                backgroundColor: Color(0xFFCCCCCC),
                child: Icon(Icons.person, color: Colors.white, size: 22),
              ),
            ),
          ),
        ],
      ),
      body: ListView(
        padding: const EdgeInsets.all(20.0),
        children: [
          // 1. Module Overview Card
          _buildOverviewCard(),

          const SizedBox(height: 25),
          
          // 2. Quizzes Section
          _buildSectionHeader(Icons.assignment_outlined, "Quizzes"),
          const SizedBox(height: 10),
          _buildTaskCard(
            title: "First Law of Motion",
            status: "Completed",
            trailing: "95%\nMarks",
            icon: Icons.check_circle,
            iconColor: const Color(0xFF2196F3),
            statusColor: Colors.blue,
          ),
          _buildTaskCard(
            title: "Second Law Calculations",
            status: "Pending",
            trailing: "--\nMarks",
            icon: Icons.more_horiz,
            iconColor: Colors.blue,
            statusColor: Colors.grey,
          ),

          const SizedBox(height: 25),

          // 3. Practicals Section
          _buildSectionHeader(Icons.science_outlined, "Practicals", headerColor: Colors.brown.shade700),
          const SizedBox(height: 10),
          _buildTaskCard(
            title: "Pendulum Motion",
            status: "Completed",
            trailing: "A-\nGrade",
            icon: Icons.check_circle,
            iconColor: Colors.orange.shade800,
            statusColor: Colors.orange.shade800,
          ),
          
          // Specialized Progress Card for Friction Lab
          _buildContinueCard(),

          const SizedBox(height: 25),

          // 4. Areas for Improvement
          _buildSectionHeader(Icons.warning_amber_rounded, "Areas for Improvement", headerColor: Colors.red.shade900),
          const SizedBox(height: 10),
          _buildImprovementItem("Newton's Second Law", "Difficulty applying formula in.."),
          _buildImprovementItem("Frictional Forces", "Confusion between static and.."),
        ],
      ),
    );
  }

  Widget _buildOverviewCard() {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(15),
        border: Border.all(color: const Color(0xFFE8F1FF)),
      ),
      child: Column(
        children: [
          Row(
            children: [
              Container(
                padding: const EdgeInsets.all(10),
                decoration: BoxDecoration(
                  color: const Color(0xFFE8F1FF),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: const Icon(Icons.science_outlined, color: Color(0xFF0056D2)),
              ),
              const SizedBox(width: 15),
              const Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Module Overview', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                  Text('Completed 3 of 5 tasks', style: TextStyle(color: Colors.grey)),
                ],
              ),
            ],
          ),
          const SizedBox(height: 15),
          ClipRRect(
            borderRadius: BorderRadius.circular(10),
            child: const LinearProgressIndicator(
              value: 0.6,
              minHeight: 10,
              backgroundColor: Color(0xFFE8F1FF),
              valueColor: AlwaysStoppedAnimation<Color>(Color(0xFF2196F3)),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSectionHeader(IconData icon, String title, {Color headerColor = Colors.black}) {
    return Row(
      children: [
        Icon(icon, size: 22, color: headerColor),
        const SizedBox(width: 8),
        Text(title, style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: headerColor)),
      ],
    );
  }

  Widget _buildTaskCard({required String title, required String status, required String trailing, required IconData icon, required Color iconColor, Color statusColor = const Color(0xFF0056D2)}) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.grey.shade200),
      ),
      child: Row(
        children: [
          Icon(icon, color: iconColor, size: 30),
          const SizedBox(width: 15),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(title, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                Text('Status: $status', style: TextStyle(color: statusColor, fontWeight: FontWeight.w500)),
              ],
            ),
          ),
          Text(trailing, textAlign: TextAlign.right, style: const TextStyle(color: Colors.grey, fontWeight: FontWeight.bold)),
        ],
      ),
    );
  }

  Widget _buildContinueCard() {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.grey.shade200),
        boxShadow: [BoxShadow(color: const Color(0xFF0056D2).withOpacity(0.1), blurRadius: 4, offset: const Offset(-4, 0))],
      ),
      child: Row(
        children: [
          const Icon(Icons.timelapse, color: Color(0xFF2196F3), size: 30),
          const SizedBox(width: 15),
          const Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Friction Lab', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                Text('Status: pending', style: TextStyle(color: Color(0xFF2196F3))),
              ],
            ),
          ),
          ElevatedButton(
            onPressed: () {},
            style: ElevatedButton.styleFrom(
              backgroundColor: const Color(0xFF2196F3),
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
            ),
            child: const Text('Continue', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
  }

  Widget _buildImprovementItem(String title, String subtitle) {
    return Container(
      margin: const EdgeInsets.only(bottom: 10),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.grey.shade200),
      ),
      child: ListTile(
        leading: CircleAvatar(
          backgroundColor: Colors.red.shade50,
          child: const Icon(Icons.priority_high, color: Colors.red),
        ),
        title: Text(title, style: const TextStyle(fontWeight: FontWeight.bold)),
        subtitle: Text(subtitle, style: const TextStyle(color: Colors.grey)),
        trailing: const Icon(Icons.chevron_right),
      ),
    );
  }
}
