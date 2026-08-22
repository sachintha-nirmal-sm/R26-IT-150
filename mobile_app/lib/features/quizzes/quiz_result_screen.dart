import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:http/http.dart' as http;
import 'package:url_launcher/url_launcher.dart';

class QuizResultScreen extends StatefulWidget {
  final String lessonId;
  final String lessonTitle;
  final int correct;
  final int total;
  final VoidCallback onRetry;

  const QuizResultScreen({
    super.key,
    required this.lessonId,
    required this.lessonTitle,
    required this.correct,
    required this.total,
    required this.onRetry,
  });

  @override
  State<QuizResultScreen> createState() => _QuizResultScreenState();
}

class _QuizResultScreenState extends State<QuizResultScreen> {
  static final String _base =
      kIsWeb ? 'http://localhost:9000' : 'http://10.0.2.2:9000';

  List<Map<String, dynamic>> _recommendations = [];
  bool _loadingRec = false;

  late final int _score;
  late final bool _passed;

  @override
  void initState() {
    super.initState();
    _score  = widget.total > 0
        ? ((widget.correct / widget.total) * 100).round()
        : 0;
    _passed = _score >= 60;

    // Show recommendations whenever the student got any question wrong
    if (widget.correct < widget.total) {
      _loadingRec = true;
      _fetchRecommendations();
    }
  }

  Future<void> _fetchRecommendations() async {
    try {
      final token = await FirebaseAuth.instance.currentUser?.getIdToken();
      final res = await http.post(
        Uri.parse('$_base/student/quiz-feedback'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode({
          'lesson_id':    widget.lessonId,
          'lesson_title': widget.lessonTitle,
          'score':        _score,
        }),
      );
      debugPrint('[Recommendations] status=${res.statusCode} body=${res.body}');
      if (res.statusCode == 200) {
        final data = jsonDecode(res.body) as Map<String, dynamic>;
        if (mounted) {
          setState(() {
            _recommendations = List<Map<String, dynamic>>.from(
              data['recommendations'] ?? [],
            );
          });
        }
      }
    } catch (e) {
      debugPrint('[Recommendations] error: $e');
    }
    if (mounted) setState(() => _loadingRec = false);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F6FA),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        automaticallyImplyLeading: false,
        title: const Text('Quiz Results',
            style: TextStyle(fontWeight: FontWeight.bold,
                fontSize: 16, color: Color(0xFF1A1C1E))),
        centerTitle: true,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _buildScoreCard(),
            const SizedBox(height: 16),
            _buildStatsRow(),

            if (widget.correct < widget.total) ...[
              const SizedBox(height: 24),
              _buildRecommendationsSection(),
            ],

            const SizedBox(height: 28),
            _buildButtons(),
            const SizedBox(height: 20),
          ],
        ),
      ),
    );
  }

  // ── Score card ──────────────────────────────────────────────────────────────
  Widget _buildScoreCard() {
    final color = _passed ? Colors.green : Colors.orange;
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.symmetric(vertical: 32, horizontal: 24),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(color: Colors.black.withOpacity(0.06),
              blurRadius: 14, offset: const Offset(0, 3))
        ],
      ),
      child: Column(children: [
        Container(
          width: 68, height: 68,
          decoration: BoxDecoration(
              color: color.withOpacity(0.12), shape: BoxShape.circle),
          child: Icon(
            _passed ? Icons.emoji_events_rounded : Icons.refresh_rounded,
            color: color, size: 34,
          ),
        ),
        const SizedBox(height: 12),
        Text(
          _passed ? 'Great Job!' : 'Keep Practicing!',
          style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold),
        ),
        const SizedBox(height: 4),
        Text(widget.lessonTitle,
            style: TextStyle(color: Colors.grey.shade600, fontSize: 13),
            textAlign: TextAlign.center),
        const SizedBox(height: 22),

        // Score circle
        Container(
          width: 110, height: 110,
          decoration: BoxDecoration(
            shape: BoxShape.circle,
            border: Border.all(color: color, width: 6),
          ),
          child: Center(
            child: Text('$_score%',
                style: TextStyle(fontSize: 28,
                    fontWeight: FontWeight.bold, color: color)),
          ),
        ),
        const SizedBox(height: 14),
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 6),
          decoration: BoxDecoration(
            color: color.withOpacity(0.1),
            borderRadius: BorderRadius.circular(20),
          ),
          child: Text(
            _passed ? 'Passed' : 'Not Passed — Review recommended',
            style: TextStyle(color: color,
                fontWeight: FontWeight.bold, fontSize: 13),
          ),
        ),
      ]),
    );
  }

  // ── Stats row ───────────────────────────────────────────────────────────────
  Widget _buildStatsRow() {
    return Row(children: [
      _statCard('Correct', '${widget.correct}',
          Colors.green, Icons.check_circle_outline),
      const SizedBox(width: 12),
      _statCard('Wrong', '${widget.total - widget.correct}',
          Colors.red, Icons.cancel_outlined),
      const SizedBox(width: 12),
      _statCard('Total', '${widget.total}',
          Colors.blue, Icons.quiz_outlined),
    ]);
  }

  Widget _statCard(String label, String value, Color color, IconData icon) {
    return Expanded(
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 16),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(14),
          boxShadow: [
            BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 8)
          ],
        ),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Icon(icon, color: color, size: 22),
          const SizedBox(height: 6),
          Text(value,
              style: TextStyle(fontSize: 20,
                  fontWeight: FontWeight.bold, color: color)),
          Text(label,
              style: const TextStyle(fontSize: 11, color: Colors.grey)),
        ]),
      ),
    );
  }

  // ── Recommendations ─────────────────────────────────────────────────────────
  Widget _buildRecommendationsSection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(children: [
          const Icon(Icons.lightbulb_rounded, color: Colors.amber, size: 20),
          const SizedBox(width: 8),
          const Text('Recommended for You',
              style: TextStyle(fontSize: 15, fontWeight: FontWeight.bold)),
        ]),
        const SizedBox(height: 4),
        Text(
          'Watch these to strengthen your understanding of ${widget.lessonTitle}',
          style: TextStyle(color: Colors.grey.shade600, fontSize: 12),
        ),
        const SizedBox(height: 14),

        if (_loadingRec)
          Container(
            height: 120,
            decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(14)),
            child: const Center(
                child: CircularProgressIndicator(strokeWidth: 2)),
          )
        else if (_recommendations.isEmpty)
          _noVideosBox()
        else
          ..._recommendations.expand((rec) {
            final videos = (rec['videos'] as List? ?? [])
                .cast<Map<String, dynamic>>();
            return videos.map(_videoCard);
          }),
      ],
    );
  }

  Widget _noVideosBox() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
      ),
      child: Row(children: [
        Icon(Icons.youtube_searched_for, color: Colors.grey.shade400, size: 28),
        const SizedBox(width: 12),
        Expanded(
          child: Text(
            'No videos found right now. Try searching '
            '"${widget.lessonTitle} physics" on YouTube.',
            style: TextStyle(color: Colors.grey.shade600, fontSize: 13),
          ),
        ),
      ]),
    );
  }

  Widget _videoCard(Map<String, dynamic> video) {
    return GestureDetector(
      onTap: () async {
        final uri = Uri.tryParse(video['url'] ?? '');
        if (uri != null && await canLaunchUrl(uri)) {
          await launchUrl(uri, mode: LaunchMode.externalApplication);
        }
      },
      child: Container(
        margin: const EdgeInsets.only(bottom: 12),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(14),
          boxShadow: [
            BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 8)
          ],
        ),
        child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
          // Thumbnail
          ClipRRect(
            borderRadius: const BorderRadius.horizontal(
                left: Radius.circular(14)),
            child: Image.network(
              video['thumbnail'] ?? '',
              width: 120,
              height: 82,
              fit: BoxFit.cover,
              errorBuilder: (_, __, ___) => Container(
                width: 120, height: 82,
                color: Colors.grey.shade100,
                child: const Icon(Icons.play_circle_outline,
                    color: Colors.grey, size: 34),
              ),
            ),
          ),

          // Info
          Expanded(
            child: Padding(
              padding: const EdgeInsets.fromLTRB(12, 10, 12, 10),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    video['title'] ?? '',
                    style: const TextStyle(
                        fontWeight: FontWeight.w600, fontSize: 13),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 5),
                  Row(children: [
                    const Icon(Icons.play_circle_fill,
                        color: Colors.red, size: 13),
                    const SizedBox(width: 4),
                    Expanded(
                      child: Text(video['channel'] ?? '',
                          style: TextStyle(
                              color: Colors.grey.shade500, fontSize: 11),
                          overflow: TextOverflow.ellipsis),
                    ),
                  ]),
                  const SizedBox(height: 8),
                  Container(
                    padding: const EdgeInsets.symmetric(
                        horizontal: 8, vertical: 3),
                    decoration: BoxDecoration(
                      color: Colors.red.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(6),
                    ),
                    child: const Row(mainAxisSize: MainAxisSize.min, children: [
                      Icon(Icons.open_in_new, color: Colors.red, size: 11),
                      SizedBox(width: 4),
                      Text('Watch on YouTube',
                          style: TextStyle(
                              color: Colors.red,
                              fontSize: 10,
                              fontWeight: FontWeight.w600)),
                    ]),
                  ),
                ],
              ),
            ),
          ),
        ]),
      ),
    );
  }

  // ── Buttons ─────────────────────────────────────────────────────────────────
  Widget _buildButtons() {
    return Row(children: [
      Expanded(
        child: OutlinedButton.icon(
          icon: const Icon(Icons.refresh),
          label: const Text('Retry Quiz'),
          onPressed: () {
            Navigator.pop(context);
            widget.onRetry();
          },
          style: OutlinedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 14)),
        ),
      ),
      const SizedBox(width: 12),
      Expanded(
        child: ElevatedButton.icon(
          icon: const Icon(Icons.check, color: Colors.white),
          label: const Text('Done',
              style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
          onPressed: () {
            Navigator.pop(context); // close result screen
            Navigator.pop(context); // close quiz screen
          },
          style: ElevatedButton.styleFrom(
            backgroundColor: const Color(0xFF2196F3),
            padding: const EdgeInsets.symmetric(vertical: 14),
          ),
        ),
      ),
    ]);
  }
}
