import 'package:flutter/material.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:firebase_auth/firebase_auth.dart';

class StudentQuizScreen extends StatefulWidget {
  final String lessonId;
  final String lessonTitle;

  const StudentQuizScreen({
    super.key,
    required this.lessonId,
    required this.lessonTitle,
  });

  @override
  State<StudentQuizScreen> createState() => _StudentQuizScreenState();
}

class _StudentQuizScreenState extends State<StudentQuizScreen> {
  List<Map<String, dynamic>> _questions = [];
  Map<int, String> _selectedAnswers = {};
  bool _isLoading = true;
  bool _submitted = false;
  int _currentIndex = 0;
  bool _notPublished = false;

  @override
  void initState() {
    super.initState();
    _loadQuestions();
  }

  Future<void> _loadQuestions() async {
    try {
      // 1. Read quiz settings
      final settingsDoc = await FirebaseFirestore.instance
          .collection('lessons')
          .doc(widget.lessonId)
          .collection('quizSettings')
          .doc('config')
          .get();

      if (settingsDoc.exists) {
        final settings = settingsDoc.data()!;
        final isPublished = settings['isPublished'] ?? false;

        if (!isPublished) {
          setState(() { _notPublished = true; _isLoading = false; });
          return;
        }

        final mode = settings['mode'] ?? 'random';
        final questionCount = settings['questionCount'] ?? 10;
        final selectedIds = List<String>.from(settings['selectedQuestionIds'] ?? []);

        // 2. Fetch questions based on mode
        final snap = await FirebaseFirestore.instance
            .collection('lessons')
            .doc(widget.lessonId)
            .collection('questions')
            .orderBy('createdAt')
            .get();

        List<Map<String, dynamic>> all = snap.docs.map((d) {
          final data = d.data();
          data['id'] = d.id;
          return data;
        }).toList();

        List<Map<String, dynamic>> result;

        if (mode == 'manual' && selectedIds.isNotEmpty) {
          result = all.where((q) => selectedIds.contains(q['id'])).toList();
        } else {
          // Random shuffle
          all.shuffle();
          result = all.take(questionCount).toList();
        }

        setState(() { _questions = result; _isLoading = false; });
      } else {
        // No settings saved yet — show all questions
        final snap = await FirebaseFirestore.instance
            .collection('lessons')
            .doc(widget.lessonId)
            .collection('questions')
            .orderBy('createdAt')
            .get();

        setState(() {
          _questions = snap.docs.map((d) {
            final data = d.data();
            data['id'] = d.id;
            return data;
          }).toList();
          _isLoading = false;
        });
      }
    } catch (e) {
      setState(() => _isLoading = false);
    }
  }

  void _selectAnswer(String label) {
    if (_submitted) return;
    setState(() => _selectedAnswers[_currentIndex] = label);
  }

  void _next() {
    if (_currentIndex < _questions.length - 1) {
      setState(() => _currentIndex++);
    }
  }

  void _previous() {
    if (_currentIndex > 0) {
      setState(() => _currentIndex--);
    }
  }

  Future<void> _submit() async {
    setState(() => _submitted = true);

    // Calculate score
    int correct = 0;
    for (int i = 0; i < _questions.length; i++) {
      if (_selectedAnswers[i] == _questions[i]['correct']) correct++;
    }

    // Save attempt to Firestore
    final uid = FirebaseAuth.instance.currentUser?.uid;
    if (uid != null) {
      await FirebaseFirestore.instance
          .collection('users')
          .doc(uid)
          .collection('quizAttempts')
          .add({
        'quizId': widget.lessonId,
        'lessonTitle': widget.lessonTitle,
        'score': ((correct / _questions.length) * 100).round(),
        'correct': correct,
        'total': _questions.length,
        'submittedAt': FieldValue.serverTimestamp(),
      });
    }

    // Show result screen
    _showResults(correct);
  }

  void _showResults(int correct) {
    final total = _questions.length;
    final percent = ((correct / total) * 100).round();
    final passed = percent >= 60;

    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (ctx) => Dialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        child: Padding(
          padding: const EdgeInsets.all(28),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              // Icon
              Container(
                width: 80, height: 80,
                decoration: BoxDecoration(
                  color: (passed ? Colors.green : Colors.orange).withOpacity(0.1),
                  shape: BoxShape.circle,
                ),
                child: Icon(
                  passed ? Icons.emoji_events : Icons.refresh,
                  color: passed ? Colors.green : Colors.orange,
                  size: 40,
                ),
              ),
              const SizedBox(height: 16),
              Text(
                passed ? 'Great Job!' : 'Keep Practicing!',
                style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 8),
              Text(
                widget.lessonTitle,
                style: TextStyle(color: Colors.grey.shade600, fontSize: 14),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 24),

              // Score circle
              Container(
                width: 100, height: 100,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  border: Border.all(
                    color: passed ? Colors.green : Colors.orange,
                    width: 5,
                  ),
                ),
                child: Center(
                  child: Text(
                    '$percent%',
                    style: TextStyle(
                      fontSize: 26,
                      fontWeight: FontWeight.bold,
                      color: passed ? Colors.green : Colors.orange,
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 20),

              // Stats row
              Row(mainAxisAlignment: MainAxisAlignment.spaceEvenly, children: [
                _statChip('✅ Correct', '$correct', Colors.green),
                _statChip('❌ Wrong', '${total - correct}', Colors.red),
                _statChip('📝 Total', '$total', Colors.blue),
              ]),
              const SizedBox(height: 24),

              Row(children: [
                Expanded(
                  child: OutlinedButton(
                    onPressed: () {
                      Navigator.pop(ctx);
                      setState(() {
                        _submitted = false;
                        _selectedAnswers = {};
                        _currentIndex = 0;
                      });
                    },
                    child: const Text('Retry'),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: ElevatedButton(
                    onPressed: () {
                      Navigator.pop(ctx);
                      Navigator.pop(context);
                    },
                    style: ElevatedButton.styleFrom(
                      backgroundColor: const Color(0xFF2196F3),
                    ),
                    child: const Text('Done', style: TextStyle(color: Colors.white)),
                  ),
                ),
              ]),
            ],
          ),
        ),
      ),
    );
  }

  Widget _statChip(String label, String value, Color color) {
    return Column(children: [
      Text(value, style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: color)),
      Text(label, style: const TextStyle(fontSize: 11, color: Colors.grey)),
    ]);
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    if (_notPublished) {
      return Scaffold(
        appBar: AppBar(title: Text(widget.lessonTitle),
            backgroundColor: Colors.white, elevation: 0),
        body: Center(
          child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
            Icon(Icons.lock_outline, size: 64, color: Colors.grey.shade300),
            const SizedBox(height: 16),
            const Text('Quiz not available yet.',
                style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.grey)),
            const SizedBox(height: 8),
            const Text('Your teacher hasn\'t published this quiz yet.',
                style: TextStyle(color: Colors.grey)),
          ]),
        ),
      );
    }

    if (_questions.isEmpty) {
      return Scaffold(
        appBar: AppBar(title: Text(widget.lessonTitle),
            backgroundColor: Colors.white, elevation: 0),
        body: Center(
          child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
            Icon(Icons.quiz_outlined, size: 64, color: Colors.grey.shade300),
            const SizedBox(height: 16),
            const Text('No quiz questions yet.',
                style: TextStyle(fontSize: 16, color: Colors.grey)),
            const SizedBox(height: 8),
            const Text('Check back later!', style: TextStyle(color: Colors.grey)),
          ]),
        ),
      );
    }

    final question = _questions[_currentIndex];
    final options = (question['options'] as List?) ?? [];
    final selected = _selectedAnswers[_currentIndex];
    final correct = question['correct'];

    return Scaffold(
      backgroundColor: const Color(0xFFF5F6FA),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Color(0xFF2196F3)),
          onPressed: () => Navigator.pop(context),
        ),
        title: Text(widget.lessonTitle,
            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15,
                color: Color(0xFF1A1C1E))),
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16),
            child: Center(
              child: Text('${_currentIndex + 1} / ${_questions.length}',
                  style: const TextStyle(fontWeight: FontWeight.w600,
                      color: Color(0xFF2196F3))),
            ),
          ),
        ],
      ),
      body: Column(
        children: [
          // Progress bar
          LinearProgressIndicator(
            value: (_currentIndex + 1) / _questions.length,
            backgroundColor: Colors.grey.shade200,
            valueColor: const AlwaysStoppedAnimation<Color>(Color(0xFF2196F3)),
            minHeight: 4,
          ),

          Expanded(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(20),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Question number badge
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                    decoration: BoxDecoration(
                      color: const Color(0xFF2196F3).withOpacity(0.1),
                      borderRadius: BorderRadius.circular(20),
                    ),
                    child: Text('Question ${_currentIndex + 1}',
                        style: const TextStyle(color: Color(0xFF2196F3),
                            fontWeight: FontWeight.w600, fontSize: 13)),
                  ),
                  const SizedBox(height: 16),

                  // Question text
                  Text(
                    question['question'] ?? '',
                    style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold,
                        color: Color(0xFF1A1C1E), height: 1.4),
                  ),
                  const SizedBox(height: 24),

                  // Options
                  ...options.map((opt) {
                    final label = opt['label'] as String;
                    final text = opt['text'] as String;
                    final isSelected = selected == label;
                    final isCorrect = label == correct;

                    Color borderColor = Colors.grey.shade200;
                    Color bgColor = Colors.white;
                    Color textColor = Colors.black87;
                    Widget? trailingIcon;

                    if (_submitted) {
                      if (isCorrect) {
                        borderColor = Colors.green;
                        bgColor = Colors.green.withOpacity(0.08);
                        textColor = Colors.green.shade700;
                        trailingIcon = const Icon(Icons.check_circle, color: Colors.green);
                      } else if (isSelected && !isCorrect) {
                        borderColor = Colors.red;
                        bgColor = Colors.red.withOpacity(0.08);
                        textColor = Colors.red.shade700;
                        trailingIcon = const Icon(Icons.cancel, color: Colors.red);
                      }
                    } else if (isSelected) {
                      borderColor = const Color(0xFF2196F3);
                      bgColor = const Color(0xFF2196F3).withOpacity(0.08);
                      textColor = const Color(0xFF2196F3);
                    }

                    return GestureDetector(
                      onTap: () => _selectAnswer(label),
                      child: Container(
                        margin: const EdgeInsets.only(bottom: 12),
                        padding: const EdgeInsets.all(16),
                        decoration: BoxDecoration(
                          color: bgColor,
                          borderRadius: BorderRadius.circular(12),
                          border: Border.all(color: borderColor, width: 1.5),
                        ),
                        child: Row(children: [
                          Container(
                            width: 32, height: 32,
                            decoration: BoxDecoration(
                              color: isSelected || (_submitted && isCorrect)
                                  ? borderColor : Colors.grey.shade100,
                              shape: BoxShape.circle,
                            ),
                            child: Center(
                              child: Text(label,
                                  style: TextStyle(
                                    fontWeight: FontWeight.bold,
                                    color: isSelected || (_submitted && isCorrect)
                                        ? Colors.white : Colors.grey.shade600,
                                  )),
                            ),
                          ),
                          const SizedBox(width: 12),
                          Expanded(child: Text(text,
                              style: TextStyle(fontSize: 15, color: textColor))),
                          if (trailingIcon != null) trailingIcon,
                        ]),
                      ),
                    );
                  }),

                  // Explanation (shown after submit)
                  if (_submitted && (question['explanation'] ?? '').isNotEmpty) ...[
                    const SizedBox(height: 8),
                    Container(
                      padding: const EdgeInsets.all(14),
                      decoration: BoxDecoration(
                        color: Colors.blue.withOpacity(0.05),
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: Colors.blue.withOpacity(0.2)),
                      ),
                      child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
                        const Icon(Icons.lightbulb_outline, color: Colors.blue, size: 18),
                        const SizedBox(width: 8),
                        Expanded(child: Text(question['explanation'],
                            style: const TextStyle(color: Colors.blue, fontSize: 14))),
                      ]),
                    ),
                  ],
                ],
              ),
            ),
          ),

          // Bottom navigation
          Container(
            padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
            color: Colors.white,
            child: Row(children: [
              if (_currentIndex > 0)
                Expanded(
                  child: OutlinedButton(
                    onPressed: _previous,
                    child: const Text('Previous'),
                  ),
                ),
              if (_currentIndex > 0) const SizedBox(width: 12),
              Expanded(
                flex: 2,
                child: _currentIndex < _questions.length - 1
                    ? ElevatedButton(
                        onPressed: selected != null ? _next : null,
                        style: ElevatedButton.styleFrom(
                          backgroundColor: const Color(0xFF2196F3),
                          padding: const EdgeInsets.symmetric(vertical: 14),
                        ),
                        child: const Text('Next', style: TextStyle(color: Colors.white)),
                      )
                    : ElevatedButton(
                        onPressed: !_submitted && selected != null ? _submit : null,
                        style: ElevatedButton.styleFrom(
                          backgroundColor: Colors.green,
                          padding: const EdgeInsets.symmetric(vertical: 14),
                        ),
                        child: Text(
                          _submitted ? 'Submitted ✓' : 'Submit Quiz',
                          style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
                        ),
                      ),
              ),
            ]),
          ),
        ],
      ),
    );
  }
}
