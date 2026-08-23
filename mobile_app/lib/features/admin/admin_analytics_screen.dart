import 'dart:math' as math;

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';

String _clean(String? s) {
  if (s == null) return '';
  return s
      .replaceAll('â€¢', '') // corrupted bullet (3-char UTF-8 mojibake)
      .replaceAll('â€”', '-') // corrupted em-dash
      .replaceAll('•', '')              // actual bullet point
      .trim();
}

class AdminAnalyticsScreen extends StatefulWidget {
  const AdminAnalyticsScreen({super.key});

  @override
  State<AdminAnalyticsScreen> createState() => _AdminAnalyticsScreenState();
}

class _AdminAnalyticsScreenState extends State<AdminAnalyticsScreen> {
  static final String _base =
      'http://localhost:9000';

  // Static cache "” survives tab switches, cleared on pull-to-refresh only
  static Map<String, dynamic>? _cachedMlReport;
  static Map<String, dynamic>? _cachedDiffAnalytics;

  Map<String, dynamic>? _mlReport;
  Map<String, dynamic>? _diffAnalytics;
  bool _loadingMl   = true;
  bool _loadingDiff = true;

  final _lessonScrollCtrl  = ScrollController();
  final _attemptsScrollCtrl = ScrollController();

  @override
  void initState() {
    super.initState();
    // Show cached data immediately "” no spinner on revisit
    if (_cachedMlReport != null) {
      _mlReport   = _cachedMlReport;
      _loadingMl  = false;
    }
    if (_cachedDiffAnalytics != null) {
      _diffAnalytics  = _cachedDiffAnalytics;
      _loadingDiff    = false;
    }
    // Always refresh in background (silently)
    _loadMlReport();
    _loadDifficultyAnalytics();
  }

  @override
  void dispose() {
    _lessonScrollCtrl.dispose();
    _attemptsScrollCtrl.dispose();
    super.dispose();
  }

  Future<String?> _token() async =>
      FirebaseAuth.instance.currentUser?.getIdToken();

  Future<void> _loadMlReport() async {
    try {
      final t = await _token();
      final res = await http.get(
        Uri.parse('$_base/admin/ml-report'),
        headers: {'Authorization': 'Bearer $t'},
      );
      if (res.statusCode == 200) {
        final data = jsonDecode(res.body) as Map<String, dynamic>;
        _cachedMlReport = data;
        if (mounted) setState(() { _mlReport = data; _loadingMl = false; });
        return;
      }
    } catch (_) {}
    if (mounted) setState(() => _loadingMl = false);
  }

  Future<void> _loadDifficultyAnalytics() async {
    try {
      final t = await _token();
      final res = await http.get(
        Uri.parse('$_base/admin/difficulty-analytics'),
        headers: {'Authorization': 'Bearer $t'},
      );
      if (res.statusCode == 200) {
        final data = jsonDecode(res.body) as Map<String, dynamic>;
        _cachedDiffAnalytics = data;
        if (mounted) setState(() { _diffAnalytics = data; _loadingDiff = false; });
        return;
      }
    } catch (_) {}
    if (mounted) setState(() => _loadingDiff = false);
  }

  @override
  Widget build(BuildContext context) {
    return RefreshIndicator(
      onRefresh: () async {
        _cachedMlReport      = null;
        _cachedDiffAnalytics = null;
        setState(() { _loadingMl = true; _loadingDiff = true; });
        await Future.wait([_loadMlReport(), _loadDifficultyAnalytics()]);
      },
      child: SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _sectionHeader('ML Model Comparison', Icons.psychology,
                const Color(0xFF1A3CBA)),
            const SizedBox(height: 12),
            _buildMlCard(),
            const SizedBox(height: 20),

            _sectionHeader('Difficulty Distribution', Icons.pie_chart,
                Colors.teal),
            const SizedBox(height: 12),
            _buildDiffDistributionCard(),
            const SizedBox(height: 20),

            _sectionHeader('Difficulty Accuracy by Lesson', Icons.fact_check,
                Colors.orange),
            const SizedBox(height: 12),
            _buildLessonAccuracyCard(),
            const SizedBox(height: 20),

            _sectionHeader('Students by Grade', Icons.school, Colors.purple),
            const SizedBox(height: 12),
            _buildGradeBreakdown(),
            const SizedBox(height: 20),

            _sectionHeader('Recent Quiz Attempts', Icons.history,
                Colors.green),
            const SizedBox(height: 12),
            _buildRecentAttemptsCard(),
            const SizedBox(height: 24),
          ],
        ),
      ),
    );
  }

  // â”€â”€ Section header â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  Widget _sectionHeader(String title, IconData icon, Color color) {
    return Row(children: [
      Icon(icon, color: color, size: 20),
      const SizedBox(width: 8),
      Text(title,
          style: TextStyle(
              fontSize: 15, fontWeight: FontWeight.bold, color: color)),
    ]);
  }

  // â”€â”€ ML model comparison card â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  Widget _buildMlCard() {
    if (_loadingMl) return _loadingBox();
    if (_mlReport == null || _mlReport!.containsKey('error')) {
      return _infoBox(
        Icons.info_outline, Colors.grey,
        'ML model not trained yet.',
        'Run python ml/train_models.py to generate results.',
      );
    }

    final results     = _mlReport!['results'] as Map<String, dynamic>;
    final bestModel   = _mlReport!['best_model'] as String? ?? '';
    final totalSamples = (_mlReport!['total_samples'] as num? ?? 0).toInt();
    final questions   = (_mlReport!['questions'] as num? ?? 0).toInt();

    return _card(Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Wrap(spacing: 8, runSpacing: 6, children: [
          _chip('$questions questions', Icons.quiz, Colors.blue),
          _chip('$totalSamples samples', Icons.dataset, Colors.teal),
          _chip('5 populations', Icons.group, Colors.purple),
          _chip('Best: $bestModel', Icons.emoji_events, Colors.amber.shade700),
        ]),
        const SizedBox(height: 16),
        const Text('Accuracy & F1 Score per Model',
            style: TextStyle(fontSize: 12, color: Colors.grey)),
        const SizedBox(height: 10),
        ...results.entries.map((e) {
          final name  = e.key;
          final acc   = ((e.value as Map)['accuracy']       as num?)?.toDouble() ?? 0;
          final f1    = ((e.value as Map)['f1_weighted']    as num?)?.toDouble() ?? 0;
          final cvF1  = ((e.value as Map)['cv_f1_weighted'] as num?)?.toDouble() ?? 0;
          final isBest = name == bestModel;
          final color  = isBest ? const Color(0xFF1A3CBA) : Colors.grey.shade500;

          return Container(
            margin: const EdgeInsets.only(bottom: 12),
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: isBest
                  ? const Color(0xFF1A3CBA).withOpacity(0.05)
                  : Colors.grey.shade50,
              borderRadius: BorderRadius.circular(10),
              border: Border.all(
                color: isBest
                    ? const Color(0xFF1A3CBA).withOpacity(0.3)
                    : Colors.grey.shade200,
              ),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(children: [
                  if (isBest)
                    const Padding(
                      padding: EdgeInsets.only(right: 6),
                      child: Icon(Icons.emoji_events,
                          color: Colors.amber, size: 16),
                    ),
                  Expanded(
                    child: Text(name,
                        style: TextStyle(
                            fontWeight: FontWeight.bold,
                            fontSize: 13,
                            color: color)),
                  ),
                  Text('CV F1: ${(cvF1 * 100).toStringAsFixed(1)}%',
                      style: TextStyle(
                          color: Colors.grey.shade500, fontSize: 11)),
                ]),
                const SizedBox(height: 8),
                _metricBar('Accuracy', acc, Colors.green),
                const SizedBox(height: 6),
                _metricBar('F1 Score', f1, const Color(0xFF1A3CBA)),
              ],
            ),
          );
        }),

        if (_mlReport!['ai_label_agreement'] != null) ...[
          const Divider(),
          Row(children: [
            const Icon(Icons.info_outline, size: 14, color: Colors.grey),
            const SizedBox(width: 6),
            Expanded(
              child: Text(
                'AI label agreement with IRT ground truth: '
                '${((_mlReport!['ai_label_agreement'] as num) * 100).toStringAsFixed(1)}%'
                '  "” ~28% mismatch intentionally models real AI labeling errors.',
                style: TextStyle(color: Colors.grey.shade600, fontSize: 11),
              ),
            ),
          ]),
        ],
      ],
    ));
  }

  Widget _metricBar(String label, double value, Color color) {
    return Row(children: [
      SizedBox(width: 68,
          child: Text(label,
              style: const TextStyle(fontSize: 11, color: Colors.grey))),
      Expanded(
        child: ClipRRect(
          borderRadius: BorderRadius.circular(4),
          child: LinearProgressIndicator(
            value: value,
            minHeight: 10,
            backgroundColor: Colors.grey.shade200,
            valueColor: AlwaysStoppedAnimation<Color>(color),
          ),
        ),
      ),
      const SizedBox(width: 8),
      SizedBox(
        width: 42,
        child: Text('${(value * 100).toStringAsFixed(1)}%',
            style: TextStyle(
                fontSize: 12, fontWeight: FontWeight.bold, color: color),
            textAlign: TextAlign.right),
      ),
    ]);
  }

  // â”€â”€ Difficulty distribution "” PIE CHART â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  Widget _buildDiffDistributionCard() {
    if (_loadingDiff) return _loadingBox();
    if (_diffAnalytics == null) {
      return _infoBox(Icons.error_outline, Colors.red, 'Failed to load.', '');
    }

    final overall = _diffAnalytics!['overall'] as Map<String, dynamic>;
    final easy   = (overall['easy']   as num? ?? 0).toInt();
    final medium = (overall['medium'] as num? ?? 0).toInt();
    final hard   = (overall['hard']   as num? ?? 0).toInt();
    final total  = easy + medium + hard;
    if (total == 0) {
      return _infoBox(Icons.bar_chart, Colors.grey, 'No questions yet.', '');
    }

    return _card(Row(
      crossAxisAlignment: CrossAxisAlignment.center,
      children: [
        // Pie chart
        SizedBox(
          width: 140,
          height: 140,
          child: CustomPaint(
            painter: _PieChartPainter(slices: [
              _PieSlice(easy   / total, Colors.green),
              _PieSlice(medium / total, Colors.orange),
              _PieSlice(hard   / total, Colors.red),
            ]),
            child: Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text('$total',
                      style: const TextStyle(
                          fontSize: 22, fontWeight: FontWeight.bold)),
                  const Text('questions',
                      style: TextStyle(fontSize: 10, color: Colors.grey)),
                ],
              ),
            ),
          ),
        ),
        const SizedBox(width: 20),

        // Legend + bars
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              if (overall['matchRate'] != null)
                Container(
                  margin: const EdgeInsets.only(bottom: 12),
                  padding: const EdgeInsets.symmetric(
                      horizontal: 10, vertical: 5),
                  decoration: BoxDecoration(
                    color: Colors.green.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Row(mainAxisSize: MainAxisSize.min, children: [
                    const Icon(Icons.verified, size: 13, color: Colors.green),
                    const SizedBox(width: 4),
                    Text(
                      '${overall['matchRate']}% AI accuracy',
                      style: const TextStyle(
                          fontSize: 11,
                          color: Colors.green,
                          fontWeight: FontWeight.w600),
                    ),
                  ]),
                ),
              _diffLegendRow('Easy',   easy,   total, Colors.green),
              const SizedBox(height: 10),
              _diffLegendRow('Medium', medium, total, Colors.orange),
              const SizedBox(height: 10),
              _diffLegendRow('Hard',   hard,   total, Colors.red),
            ],
          ),
        ),
      ],
    ));
  }

  Widget _diffLegendRow(String label, int count, int total, Color color) {
    final pct = total > 0 ? count / total : 0.0;
    return Row(children: [
      Container(width: 12, height: 12,
          decoration: BoxDecoration(color: color, shape: BoxShape.circle)),
      const SizedBox(width: 6),
      SizedBox(width: 50,
          child: Text(label, style: const TextStyle(fontSize: 12))),
      Expanded(
        child: ClipRRect(
          borderRadius: BorderRadius.circular(4),
          child: LinearProgressIndicator(
            value: pct,
            minHeight: 8,
            backgroundColor: Colors.grey.shade200,
            valueColor: AlwaysStoppedAnimation<Color>(color),
          ),
        ),
      ),
      const SizedBox(width: 8),
      Text('${(pct * 100).toStringAsFixed(0)}%',
          style: TextStyle(fontSize: 11, color: color,
              fontWeight: FontWeight.bold)),
    ]);
  }

  // â”€â”€ Per-lesson difficulty accuracy bars â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  Widget _buildLessonAccuracyCard() {
    if (_loadingDiff) return _loadingBox();
    if (_diffAnalytics == null) return const SizedBox.shrink();

    final lessons = (_diffAnalytics!['lessons'] as List)
        .cast<Map<String, dynamic>>()
        .where((l) => (l['withActual'] as num? ?? 0) > 0)
        .toList();

    if (lessons.isEmpty) {
      return _infoBox(Icons.info_outline, Colors.grey,
          'No difficulty evaluations yet.',
          'Simulated responses are being processed.');
    }

    return _card(SizedBox(
      height: 320,
      child: Scrollbar(
        controller: _lessonScrollCtrl,
        thumbVisibility: true,
        child: SingleChildScrollView(
          controller: _lessonScrollCtrl,
          child: Column(
            children: lessons.map((l) {
        final acc      = (l['difficultyAccuracy'] as num?)?.toDouble() ?? 0;
        final gradeRaw = l['grade'];
        final grade    = gradeRaw is int
            ? 'Grade $gradeRaw'
            : gradeRaw?.toString() ?? '';
        final title    = l['title']?.toString() ?? '';
        final total    = (l['total']        as num? ?? 0).toInt();
        final attempts = (l['totalAttempts'] as num? ?? 0).toInt();

        final color = acc >= 75
            ? Colors.green
            : acc >= 50 ? Colors.orange : Colors.red;
        final gradeNum = gradeRaw is int
            ? gradeRaw
            : int.tryParse(grade.replaceAll(RegExp(r'[^0-9]'), '')) ?? 0;
        final gradeColor = gradeNum == 9
            ? Colors.blue
            : gradeNum == 10 ? Colors.green : Colors.orange;

        return Container(
          margin: const EdgeInsets.only(bottom: 10),
          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
          decoration: BoxDecoration(
            color: Colors.grey.shade50,
            borderRadius: BorderRadius.circular(10),
            border: Border.all(color: Colors.grey.shade200),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(children: [
                Container(
                  padding: const EdgeInsets.symmetric(
                      horizontal: 7, vertical: 2),
                  decoration: BoxDecoration(
                    color: gradeColor.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(6),
                  ),
                  child: Text(grade,
                      style: TextStyle(
                          fontSize: 10,
                          color: gradeColor,
                          fontWeight: FontWeight.bold)),
                ),
                const SizedBox(width: 8),
                Expanded(
                    child: Text(_clean(title),
                        style: const TextStyle(
                            fontWeight: FontWeight.w600, fontSize: 13),
                        overflow: TextOverflow.ellipsis)),
                Container(
                  padding: const EdgeInsets.symmetric(
                      horizontal: 8, vertical: 2),
                  decoration: BoxDecoration(
                    color: color.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Text('${acc.toStringAsFixed(0)}%',
                      style: TextStyle(
                          color: color,
                          fontWeight: FontWeight.bold,
                          fontSize: 13)),
                ),
              ]),
              const SizedBox(height: 6),
              ClipRRect(
                borderRadius: BorderRadius.circular(4),
                child: LinearProgressIndicator(
                  value: acc / 100,
                  minHeight: 6,
                  backgroundColor: Colors.grey.shade200,
                  valueColor: AlwaysStoppedAnimation<Color>(color),
                ),
              ),
              const SizedBox(height: 4),
              Text('$total questions  ·  $attempts simulated attempts',
                  style: TextStyle(
                      color: Colors.grey.shade500, fontSize: 10)),
            ],
          ),
        );
            }).toList(),
          ),
        ),
      ),
    ));
  }

  // â”€â”€ Grade breakdown â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  Widget _buildGradeBreakdown() {
    return _card(Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        ...['Grade 9', 'Grade 10', 'Grade 11'].map((grade) =>
            FutureBuilder<AggregateQuerySnapshot>(
              future: FirebaseFirestore.instance
                  .collection('users')
                  .where('role', isEqualTo: 'student')
                  .where('grade', isEqualTo: grade)
                  .count().get(),
              builder: (context, snap) {
                final count = snap.data?.count ?? 0;
                final color = grade == 'Grade 9'
                    ? Colors.blue
                    : grade == 'Grade 10' ? Colors.green : Colors.orange;
                return Padding(
                  padding: const EdgeInsets.symmetric(vertical: 8),
                  child: Row(children: [
                    SizedBox(width: 76,
                        child: Text(grade,
                            style: const TextStyle(fontSize: 13))),
                    Expanded(
                      child: ClipRRect(
                        borderRadius: BorderRadius.circular(4),
                        child: LinearProgressIndicator(
                          value: count == 0 ? 0 : (count / 50).clamp(0.0, 1.0),
                          minHeight: 10,
                          backgroundColor: Colors.grey.shade200,
                          valueColor: AlwaysStoppedAnimation<Color>(color),
                        ),
                      ),
                    ),
                    const SizedBox(width: 10),
                    SizedBox(width: 28,
                        child: Text('$count',
                            style: const TextStyle(
                                fontWeight: FontWeight.bold),
                            textAlign: TextAlign.right)),
                  ]),
                );
              },
            )),
      ],
    ));
  }

  // â”€â”€ Recent attempts â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  Widget _buildRecentAttemptsCard() {
    return _card(StreamBuilder<QuerySnapshot>(
      stream: FirebaseFirestore.instance
          .collectionGroup('quizAttempts')
          .limit(10).snapshots(),
      builder: (context, snap) {
        if (!snap.hasData) {
          return const Center(child: CircularProgressIndicator());
        }
        if (snap.data!.docs.isEmpty) {
          return const Text('No quiz attempts yet.',
              style: TextStyle(color: Colors.grey));
        }
        return SizedBox(
          height: 320,
          child: Scrollbar(
            controller: _attemptsScrollCtrl,
            thumbVisibility: true,
            child: SingleChildScrollView(
              controller: _attemptsScrollCtrl,
              child: Column(
                children: snap.data!.docs.map((doc) {
            final data   = doc.data() as Map<String, dynamic>;
            final score  = (data['score'] as num? ?? 0).toInt();
            final passed = score >= 60;
            return Padding(
              padding: const EdgeInsets.only(bottom: 8),
              child: Row(children: [
                Container(
                  width: 48, height: 48,
                  decoration: BoxDecoration(
                    color: (passed ? Colors.green : Colors.red)
                        .withOpacity(0.1),
                    borderRadius: BorderRadius.circular(10),
                  ),
                  child: Center(
                    child: Text('$score%',
                        style: TextStyle(
                            color: passed ? Colors.green : Colors.red,
                            fontWeight: FontWeight.bold,
                            fontSize: 13)),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                        data['lessonTitle'] ?? data['quizId'] ?? '-',
                        style: const TextStyle(
                            fontWeight: FontWeight.w600,
                            fontSize: 13),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis),
                    Text(
                        '${data['studentName'] ?? 'Unknown'} · Grade ${data['grade'] ?? '-'}',
                        style: TextStyle(color: Colors.grey.shade600, fontSize: 11),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis),
                    Text(passed ? 'Pass' : 'Fail',
                        style: TextStyle(
                            color: passed ? Colors.green : Colors.red,
                            fontSize: 12)),
                  ],
                )),
                Text('$score%',
                    style: TextStyle(
                        fontWeight: FontWeight.bold,
                        color: passed ? Colors.green : Colors.red)),
              ]),
            );
                }).toList(),
              ),
            ),
          ),
        );
      },
    ));
  }

  // â”€â”€ Shared helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  Widget _card(Widget child) => Container(
    width: double.infinity,
    padding: const EdgeInsets.all(16),
    decoration: BoxDecoration(
      color: Colors.white,
      borderRadius: BorderRadius.circular(14),
      boxShadow: [
        BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 10,
            offset: const Offset(0, 2)),
      ],
    ),
    child: child,
  );

  Widget _loadingBox() => _card(
    const Padding(
      padding: EdgeInsets.symmetric(vertical: 24),
      child: Center(child: CircularProgressIndicator(strokeWidth: 2)),
    ),
  );

  Widget _infoBox(IconData icon, Color color, String title, String sub) =>
      _card(Row(children: [
        Icon(icon, color: color, size: 28),
        const SizedBox(width: 12),
        Expanded(child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(title,
                style: TextStyle(
                    fontWeight: FontWeight.w600, color: color)),
            if (sub.isNotEmpty)
              Text(sub,
                  style: TextStyle(
                      color: Colors.grey.shade500, fontSize: 12)),
          ],
        )),
      ]));

  Widget _chip(String label, IconData icon, Color color) => Container(
    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
    decoration: BoxDecoration(
      color: color.withOpacity(0.1),
      borderRadius: BorderRadius.circular(20),
    ),
    child: Row(mainAxisSize: MainAxisSize.min, children: [
      Icon(icon, size: 13, color: color),
      const SizedBox(width: 4),
      Text(label,
          style: TextStyle(
              fontSize: 11,
              color: color,
              fontWeight: FontWeight.w600)),
    ]),
  );
}

// â”€â”€ Pie chart painter â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
class _PieSlice {
  final double fraction;
  final Color color;
  const _PieSlice(this.fraction, this.color);
}

class _PieChartPainter extends CustomPainter {
  final List<_PieSlice> slices;
  const _PieChartPainter({required this.slices});

  @override
  void paint(Canvas canvas, Size size) {
    final center = Offset(size.width / 2, size.height / 2);
    final radius = math.min(size.width, size.height) / 2;
    double startAngle = -math.pi / 2;

    for (final slice in slices) {
      if (slice.fraction <= 0) continue;
      final sweepAngle = 2 * math.pi * slice.fraction;
      canvas.drawArc(
        Rect.fromCircle(center: center, radius: radius),
        startAngle,
        sweepAngle,
        true,
        Paint()..color = slice.color,
      );
      startAngle += sweepAngle;
    }

    // White donut hole
    canvas.drawCircle(
      center,
      radius * 0.58,
      Paint()..color = Colors.white,
    );
  }

  @override
  bool shouldRepaint(covariant _PieChartPainter old) =>
      old.slices != slices;
}



