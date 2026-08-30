import 'package:flutter/material.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'quiz_result_screen.dart';

class StudentQuizScreen extends StatefulWidget {
  final String lessonId;
  final String lessonTitle;

  // Optional sub-lesson context — set when navigating from SubLessonsScreen.
  final String? subLessonId;
  final String? subLessonNumber;
  final String? subLessonTitle;
  final int?    subLessonOrder;    // 0-indexed position in the sub-lesson list
  final int?    totalSubLessons;
  final int?    quizCount;         // question limit from admin's sub-lesson setting

  const StudentQuizScreen({
    super.key,
    required this.lessonId,
    required this.lessonTitle,
    this.subLessonId,
    this.subLessonNumber,
    this.subLessonTitle,
    this.subLessonOrder,
    this.totalSubLessons,
    this.quizCount,
  });

  @override
  State<StudentQuizScreen> createState() => _StudentQuizScreenState();
}

class _StudentQuizScreenState extends State<StudentQuizScreen> {
  List<Map<String, dynamic>> _questions = [];
  Map<int, String> _selectedAnswers = {};
  bool _isLoading    = true;
  bool _submitted    = false;
  int  _currentIndex = 0;
  bool _notPublished = false;
  bool _isAdaptive   = false;

  bool get _isSubLesson => widget.subLessonId != null;

  // quizId used for attempt tracking: lesson-level or sub-lesson-specific
  String get _quizId => _isSubLesson
      ? '${widget.lessonId}_${widget.subLessonId}'
      : widget.lessonId;

  @override
  void initState() {
    super.initState();
    _loadQuestions();
  }

  // ── Load questions ──────────────────────────────────────────────────────────
  Future<void> _loadQuestions() async {
    setState(() { _isLoading = true; _isAdaptive = false; });
    try {
      final snap = await FirebaseFirestore.instance
          .collection('lessons')
          .doc(widget.lessonId)
          .collection('questions')
          .orderBy('createdAt')
          .get();

      List<Map<String, dynamic>> all = snap.docs.map((d) {
        final data = Map<String, dynamic>.from(d.data());
        data['id'] = d.id;
        return data;
      }).toList();

      List<Map<String, dynamic>> result;

      if (_isSubLesson) {
        // ── Sub-lesson mode: filter by subLessonId, skip quiz settings ──────
        all = all
            .where((q) => q['subLessonId'] == widget.subLessonId)
            .toList();

        final uid = FirebaseAuth.instance.currentUser?.uid;
        List<String> previousWrongIds = [];

        if (uid != null) {
          try {
            final prevSnap = await FirebaseFirestore.instance
                .collection('users')
                .doc(uid)
                .collection('quizAttempts')
                .where('quizId', isEqualTo: _quizId)
                .orderBy('submittedAt', descending: true)
                .limit(1)
                .get();
            if (prevSnap.docs.isNotEmpty) {
              previousWrongIds = List<String>.from(
                prevSnap.docs.first.data()['wrongQuestionIds'] ?? [],
              );
            }
          } catch (_) {}
        }

        final limit = widget.quizCount ?? 10;

        if (previousWrongIds.isNotEmpty) {
          final weakList  = all.where((q) => previousWrongIds.contains(q['id'])).toList();
          final otherList = all.where((q) => !previousWrongIds.contains(q['id'])).toList();
          final weakCount  = (limit * 0.6).ceil().clamp(0, weakList.length);
          final otherCount = (limit - weakCount).clamp(0, otherList.length);
          weakList.shuffle();
          otherList.shuffle();
          result = [...weakList.take(weakCount), ...otherList.take(otherCount)];
          result.shuffle();
          _isAdaptive = true;
        } else {
          const order = {'Easy': 0, 'Medium': 1, 'Hard': 2};
          all.sort((a, b) {
            final aR = order[a['difficulty'] ?? 'Medium'] ?? 1;
            final bR = order[b['difficulty'] ?? 'Medium'] ?? 1;
            return aR.compareTo(bR);
          });
          result = all.take(limit).toList();
        }
      } else {
        // ── Lesson mode: original logic with quiz settings ────────────────
        final settingsDoc = await FirebaseFirestore.instance
            .collection('lessons')
            .doc(widget.lessonId)
            .collection('quizSettings')
            .doc('config')
            .get();

        int    questionCount = 10;
        String mode          = 'random';
        List<String> selectedIds = [];

        if (settingsDoc.exists) {
          final settings    = settingsDoc.data()!;
          final isPublished = settings['isPublished'] ?? false;
          if (!isPublished) {
            setState(() { _notPublished = true; _isLoading = false; });
            return;
          }
          mode          = settings['mode']          ?? 'random';
          questionCount = settings['questionCount'] ?? 10;
          selectedIds   = List<String>.from(settings['selectedQuestionIds'] ?? []);
        }

        if (mode == 'manual' && selectedIds.isNotEmpty) {
          result = all.where((q) => selectedIds.contains(q['id'])).toList();
        } else {
          final uid = FirebaseAuth.instance.currentUser?.uid;
          List<String> previousWrongIds = [];
          if (uid != null) {
            try {
              final prevSnap = await FirebaseFirestore.instance
                  .collection('users')
                  .doc(uid)
                  .collection('quizAttempts')
                  .where('quizId', isEqualTo: _quizId)
                  .orderBy('submittedAt', descending: true)
                  .limit(1)
                  .get();
              if (prevSnap.docs.isNotEmpty) {
                previousWrongIds = List<String>.from(
                  prevSnap.docs.first.data()['wrongQuestionIds'] ?? [],
                );
              }
            } catch (_) {}
          }

          if (previousWrongIds.isNotEmpty) {
            final weakList  = all.where((q) => previousWrongIds.contains(q['id'])).toList();
            final otherList = all.where((q) => !previousWrongIds.contains(q['id'])).toList();
            final weakCount  = (questionCount * 0.6).ceil().clamp(0, weakList.length);
            final otherCount = (questionCount - weakCount).clamp(0, otherList.length);
            weakList.shuffle();
            otherList.shuffle();
            result = [...weakList.take(weakCount), ...otherList.take(otherCount)];
            result.shuffle();
            _isAdaptive = true;
          } else {
            const order = {'Easy': 0, 'Medium': 1, 'Hard': 2};
            all.sort((a, b) {
              final aR = order[a['difficulty'] ?? 'Medium'] ?? 1;
              final bR = order[b['difficulty'] ?? 'Medium'] ?? 1;
              return aR.compareTo(bR);
            });
            result = all.take(questionCount).toList();
          }
        }
      }

      setState(() { _questions = result; _isLoading = false; });
    } catch (_) {
      setState(() => _isLoading = false);
    }
  }

  // ── Answer selection ────────────────────────────────────────────────────────
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

  // ── Submit ──────────────────────────────────────────────────────────────────
  Future<void> _submit() async {
    setState(() => _submitted = true);

    int correct = 0;
    final List<String> wrongIds = [];

    for (int i = 0; i < _questions.length; i++) {
      final isCorrect = _selectedAnswers[i] == _questions[i]['correct'];
      if (isCorrect) {
        correct++;
      } else {
        final id = _questions[i]['id'] as String?;
        if (id != null) wrongIds.add(id);
      }
    }

    final uid = FirebaseAuth.instance.currentUser?.uid;
    if (uid != null) {
      final userDoc = await FirebaseFirestore.instance
          .collection('users')
          .doc(uid)
          .get();
      final userData = userDoc.data() ?? {};
      await FirebaseFirestore.instance
          .collection('users')
          .doc(uid)
          .collection('quizAttempts')
          .add({
        'quizId':           _quizId,
        'lessonId':         widget.lessonId,
        'lessonTitle':      widget.lessonTitle,
        'subLessonId':      widget.subLessonId,
        'subLessonTitle':   widget.subLessonTitle,
        'score':            (_questions.isEmpty ? 0 : (correct / _questions.length * 100)).round(),
        'correct':          correct,
        'total':            _questions.length,
        'wrongQuestionIds': wrongIds,
        'isAdaptive':       _isAdaptive,
        'studentName':      ((userData['name'] as String?)?.trim().isNotEmpty == true
                              ? userData['name']
                              : (userData['fullName'] as String?)?.trim().isNotEmpty == true
                                  ? userData['fullName']
                                  : (userData['displayName'] as String?)?.trim().isNotEmpty == true
                                      ? userData['displayName']
                                      : FirebaseAuth.instance.currentUser?.displayName)
                            ?.trim() ?? '',
        'grade':            userData['grade']?.toString() ?? '',
        'submittedAt':      FieldValue.serverTimestamp(),
      });
    }

    _recordAttempts();

    if (mounted) {
      Navigator.push(
        context,
        MaterialPageRoute(
          builder: (_) => QuizResultScreen(
            lessonId:        widget.lessonId,
            lessonTitle:     widget.lessonTitle,
            correct:         correct,
            total:           _questions.length,
            subLessonId:     widget.subLessonId,
            subLessonNumber: widget.subLessonNumber,
            subLessonTitle:  widget.subLessonTitle,
            onRetry: () {
              setState(() {
                _submitted       = false;
                _selectedAnswers = {};
                _currentIndex    = 0;
              });
              _loadQuestions();
            },
          ),
        ),
      );
    }
  }

  // ── Per-question difficulty tracking ───────────────────────────────────────
  Future<void> _recordAttempts() async {
    try {
      await Future.wait(_questions.asMap().entries.map((entry) async {
        final i   = entry.key;
        final q   = entry.value;
        final qId = q['id'] as String?;
        if (qId == null) return;
        final isCorrect = _selectedAnswers[i] == q['correct'];
        final qRef = FirebaseFirestore.instance
            .collection('lessons')
            .doc(widget.lessonId)
            .collection('questions')
            .doc(qId);
        await FirebaseFirestore.instance.runTransaction((tx) async {
          final snap = await tx.get(qRef);
          if (!snap.exists) return;
          final data        = snap.data()!;
          final newAttempts = (data['attempts']    as int? ?? 0) + 1;
          final newCorrect  = (data['correctCount'] as int? ?? 0) + (isCorrect ? 1 : 0);
          final updates = <String, dynamic>{
            'attempts':     newAttempts,
            'correctCount': newCorrect,
          };
          if (newAttempts >= 10) {
            final rate   = newCorrect / newAttempts;
            final actual = rate > 0.70 ? 'Easy' : rate > 0.40 ? 'Medium' : 'Hard';
            updates['actualDifficulty'] = actual;
            updates['difficultyMatch']  = actual == (data['difficulty'] ?? 'Medium');
          }
          tx.update(qRef, updates);
        });
      }).toList());
    } catch (_) {}
  }

  // ── UI ──────────────────────────────────────────────────────────────────────
  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    if (_notPublished) {
      return Scaffold(
        appBar: AppBar(title: Text(widget.lessonTitle),
            backgroundColor: Colors.white, elevation: 0),
        body: Center(child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.lock_outline, size: 64, color: Colors.grey.shade300),
            const SizedBox(height: 16),
            const Text('Quiz not available yet.',
                style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold,
                    color: Colors.grey)),
            const SizedBox(height: 8),
            const Text('Your teacher hasn\'t published this quiz yet.',
                style: TextStyle(color: Colors.grey)),
          ],
        )),
      );
    }

    if (_questions.isEmpty) {
      return Scaffold(
        appBar: AppBar(title: Text(widget.subLessonTitle ?? widget.lessonTitle),
            backgroundColor: Colors.white, elevation: 0),
        body: Center(child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.quiz_outlined, size: 64, color: Colors.grey.shade300),
            const SizedBox(height: 16),
            const Text('No quiz questions yet.',
                style: TextStyle(fontSize: 16, color: Colors.grey)),
            const SizedBox(height: 8),
            const Text('Check back later!', style: TextStyle(color: Colors.grey)),
          ],
        )),
      );
    }

    final question = _questions[_currentIndex];
    final options  = (question['options'] as List?) ?? [];
    final selected = _selectedAnswers[_currentIndex];
    final correct  = question['correct'];

    final String appBarTitle = _isSubLesson
        ? '${widget.subLessonNumber} · ${widget.subLessonTitle}'
        : widget.lessonTitle;

    return Scaffold(
      backgroundColor: const Color(0xFFF5F6FA),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Color(0xFF2196F3)),
          onPressed: () => Navigator.pop(context),
        ),
        title: Text(appBarTitle,
            style: const TextStyle(fontWeight: FontWeight.bold,
                fontSize: 14, color: Color(0xFF1A1C1E))),
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
          LinearProgressIndicator(
            value: (_currentIndex + 1) / _questions.length,
            backgroundColor: Colors.grey.shade200,
            valueColor: const AlwaysStoppedAnimation<Color>(Color(0xFF2196F3)),
            minHeight: 4,
          ),

          if (_isAdaptive)
            Container(
              width: double.infinity,
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
              color: Colors.orange.withOpacity(0.1),
              child: const Row(children: [
                Icon(Icons.auto_fix_high, color: Colors.orange, size: 15),
                SizedBox(width: 8),
                Expanded(
                  child: Text(
                    'Adaptive quiz — focused on your previous weak areas',
                    style: TextStyle(color: Colors.orange,
                        fontSize: 12, fontWeight: FontWeight.w500),
                  ),
                ),
              ]),
            ),

          Expanded(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(20),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Container(
                    padding: const EdgeInsets.symmetric(
                        horizontal: 12, vertical: 4),
                    decoration: BoxDecoration(
                      color: const Color(0xFF2196F3).withOpacity(0.1),
                      borderRadius: BorderRadius.circular(20),
                    ),
                    child: Text('Question ${_currentIndex + 1}',
                        style: const TextStyle(color: Color(0xFF2196F3),
                            fontWeight: FontWeight.w600, fontSize: 13)),
                  ),
                  const SizedBox(height: 16),

                  Text(
                    question['question'] ?? '',
                    style: const TextStyle(fontSize: 18,
                        fontWeight: FontWeight.bold,
                        color: Color(0xFF1A1C1E), height: 1.4),
                  ),
                  const SizedBox(height: 24),

                  ...options.map((opt) {
                    final label     = opt['label'] as String;
                    final text      = opt['text']  as String;
                    final isSelected = selected == label;
                    final isCorrect  = label == correct;

                    Color  borderColor = Colors.grey.shade200;
                    Color  bgColor     = Colors.white;
                    Color  textColor   = Colors.black87;
                    Widget? trailingIcon;

                    if (_submitted) {
                      if (isCorrect) {
                        borderColor  = Colors.green;
                        bgColor      = Colors.green.withOpacity(0.08);
                        textColor    = Colors.green.shade700;
                        trailingIcon = const Icon(Icons.check_circle,
                            color: Colors.green);
                      } else if (isSelected && !isCorrect) {
                        borderColor  = Colors.red;
                        bgColor      = Colors.red.withOpacity(0.08);
                        textColor    = Colors.red.shade700;
                        trailingIcon = const Icon(Icons.cancel,
                            color: Colors.red);
                      }
                    } else if (isSelected) {
                      borderColor = const Color(0xFF2196F3);
                      bgColor     = const Color(0xFF2196F3).withOpacity(0.08);
                      textColor   = const Color(0xFF2196F3);
                    }

                    return GestureDetector(
                      onTap: () => _selectAnswer(label),
                      child: Container(
                        margin: const EdgeInsets.only(bottom: 12),
                        padding: const EdgeInsets.all(16),
                        decoration: BoxDecoration(
                          color: bgColor,
                          borderRadius: BorderRadius.circular(12),
                          border: Border.all(
                              color: borderColor, width: 1.5),
                        ),
                        child: Row(children: [
                          Container(
                            width: 32, height: 32,
                            decoration: BoxDecoration(
                              color: isSelected ||
                                      (_submitted && isCorrect)
                                  ? borderColor
                                  : Colors.grey.shade100,
                              shape: BoxShape.circle,
                            ),
                            child: Center(
                              child: Text(label,
                                  style: TextStyle(
                                    fontWeight: FontWeight.bold,
                                    color: isSelected ||
                                            (_submitted && isCorrect)
                                        ? Colors.white
                                        : Colors.grey.shade600,
                                  )),
                            ),
                          ),
                          const SizedBox(width: 12),
                          Expanded(child: Text(text,
                              style: TextStyle(
                                  fontSize: 15, color: textColor))),
                          if (trailingIcon != null) trailingIcon,
                        ]),
                      ),
                    );
                  }),

                  if (_submitted &&
                      (question['explanation'] ?? '').isNotEmpty) ...[
                    const SizedBox(height: 8),
                    Container(
                      padding: const EdgeInsets.all(14),
                      decoration: BoxDecoration(
                        color: Colors.blue.withOpacity(0.05),
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(
                            color: Colors.blue.withOpacity(0.2)),
                      ),
                      child: Row(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            const Icon(Icons.lightbulb_outline,
                                color: Colors.blue, size: 18),
                            const SizedBox(width: 8),
                            Expanded(
                              child: Text(question['explanation'],
                                  style: const TextStyle(
                                      color: Colors.blue,
                                      fontSize: 14)),
                            ),
                          ]),
                    ),
                  ],
                ],
              ),
            ),
          ),

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
                          padding: const EdgeInsets.symmetric(
                              vertical: 14),
                        ),
                        child: const Text('Next',
                            style: TextStyle(color: Colors.white)),
                      )
                    : ElevatedButton(
                        onPressed:
                            !_submitted && selected != null
                                ? _submit
                                : null,
                        style: ElevatedButton.styleFrom(
                          backgroundColor: Colors.green,
                          padding: const EdgeInsets.symmetric(
                              vertical: 14),
                        ),
                        child: Text(
                          _submitted
                              ? 'Submitted ✓'
                              : 'Submit Quiz',
                          style: const TextStyle(
                              color: Colors.white,
                              fontWeight: FontWeight.bold),
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
