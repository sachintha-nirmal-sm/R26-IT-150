import 'package:flutter/material.dart';

import '../../core/api/api_client.dart';
import 'data/quiz_repository.dart';
import 'quiz_result_screen.dart';

/// Takes a lesson-level quiz through the real backend API — questions are
/// fetched sanitized (no answer keys) via /student/quizzes/{id}/start, and
/// grading happens server-side via /student/quizzes/{id}/submit. This
/// replaces a previous version that read lessons/{id}/questions directly
/// from Firestore, which the security rules correctly block (answer keys
/// must never reach the client) and which had no real question data behind
/// it anyway.
///
/// Sub-lesson quizzes (subLessonId set) have no backend support yet — the
/// quiz data model only covers lesson-level and final quizzes — so that
/// mode shows a clear "not available" state instead of attempting a call
/// that cannot succeed.
class StudentQuizScreen extends StatefulWidget {
  final String lessonId;
  final String lessonTitle;

  // Sub-lesson context, kept only to preserve the existing call signature
  // from sub_lessons_screen.dart — see the class doc above.
  final String? subLessonId;
  final String? subLessonNumber;
  final String? subLessonTitle;
  final int? subLessonOrder;
  final int? totalSubLessons;
  final int? quizCount;

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
  static const Color _brand = Color(0xFF2196F3);

  final _repo = QuizRepository();

  bool _isLoading = true;
  String? _errorMessage;

  List<Map<String, dynamic>> _questions = [];
  String? _quizId;
  String? _attemptId;
  final Map<int, String> _selectedAnswers = {};
  final Map<String, bool> _gradedCorrect = {};
  bool _submitted = false;
  int _currentIndex = 0;
  DateTime? _startedAt;

  bool get _isSubLesson => widget.subLessonId != null;

  @override
  void initState() {
    super.initState();
    _start();
  }

  // ── Start ────────────────────────────────────────────────────────────────
  Future<void> _start() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
      _submitted = false;
      _selectedAnswers.clear();
      _gradedCorrect.clear();
      _currentIndex = 0;
    });

    if (_isSubLesson) {
      setState(() {
        _isLoading = false;
        _errorMessage =
            "Quizzes for individual sections aren't available yet — "
            "try the full lesson quiz instead.";
      });
      return;
    }

    try {
      final quizzes = await _repo.listQuizzesForLesson(widget.lessonId);
      final quiz = quizzes.cast<Map<String, dynamic>?>().firstWhere(
            (q) => q?['status'] == 'bankReady',
            orElse: () => null,
          );
      if (quiz == null) {
        setState(() {
          _isLoading = false;
          _errorMessage = 'No quiz is available for this lesson yet.';
        });
        return;
      }

      final quizId = quiz['id'] as String;
      final result = await _repo.startQuiz(widget.lessonId, quizId);
      final questions = (result['questions'] as List)
          .map((e) => Map<String, dynamic>.from(e as Map))
          .toList();

      setState(() {
        _quizId = quizId;
        _attemptId = result['attemptId'] as String?;
        _questions = questions;
        _startedAt = DateTime.now();
        _isLoading = false;
      });
    } on ApiException catch (e) {
      setState(() {
        _isLoading = false;
        _errorMessage = e.message;
      });
    } catch (e) {
      setState(() {
        _isLoading = false;
        _errorMessage = 'Could not load this quiz: $e';
      });
    }
  }

  // ── Answer selection ────────────────────────────────────────────────────────
  void _selectAnswer(String optionText) {
    if (_submitted) return;
    setState(() => _selectedAnswers[_currentIndex] = optionText);
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
    if (_attemptId == null || _quizId == null) return;
    setState(() => _submitted = true);

    final answers = _questions.asMap().entries.map((entry) {
      final i = entry.key;
      final q = entry.value;
      return {
        'questionId': q['questionId'] as String?,
        'studentAnswer': _selectedAnswers[i],
      };
    }).toList();

    final timeTaken = _startedAt == null
        ? 0
        : DateTime.now().difference(_startedAt!).inSeconds;

    try {
      final result = await _repo.submitQuiz(
        lessonId: widget.lessonId,
        quizId: _quizId!,
        attemptId: _attemptId!,
        answers: answers,
        timeTakenSeconds: timeTaken,
      );

      final graded = (result['answers'] as List? ?? [])
          .map((e) => Map<String, dynamic>.from(e as Map));
      var correct = 0;
      for (final g in graded) {
        final qId = g['questionId'] as String?;
        final isCorrect = g['isCorrect'] == true;
        if (qId != null) _gradedCorrect[qId] = isCorrect;
        if (isCorrect) correct++;
      }

      if (!mounted) return;
      Navigator.push(
        context,
        MaterialPageRoute(
          builder: (_) => QuizResultScreen(
            lessonId: widget.lessonId,
            lessonTitle: widget.lessonTitle,
            correct: correct,
            total: _questions.length,
            onRetry: _start,
          ),
        ),
      );
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() => _submitted = false);
      ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Could not submit: ${e.message}')));
    } catch (e) {
      if (!mounted) return;
      setState(() => _submitted = false);
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text('Could not submit: $e')));
    }
  }

  // ── UI ──────────────────────────────────────────────────────────────────────
  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    if (_errorMessage != null) {
      return Scaffold(
        appBar: AppBar(
            title: Text(widget.lessonTitle),
            backgroundColor: Colors.white,
            elevation: 0),
        body: Center(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(Icons.quiz_outlined,
                    size: 64, color: Colors.grey.shade300),
                const SizedBox(height: 16),
                Text(_errorMessage!,
                    textAlign: TextAlign.center,
                    style: const TextStyle(
                        fontSize: 15, color: Colors.grey, height: 1.4)),
              ],
            ),
          ),
        ),
      );
    }

    if (_questions.isEmpty) {
      return Scaffold(
        appBar: AppBar(
            title: Text(widget.lessonTitle),
            backgroundColor: Colors.white,
            elevation: 0),
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(Icons.quiz_outlined, size: 64, color: Colors.grey.shade300),
              const SizedBox(height: 16),
              const Text('No quiz questions yet.',
                  style: TextStyle(fontSize: 16, color: Colors.grey)),
            ],
          ),
        ),
      );
    }

    final question = _questions[_currentIndex];
    final options = (question['options'] as List?)?.cast<String>() ?? [];
    final selected = _selectedAnswers[_currentIndex];
    final questionId = question['questionId'] as String?;
    final isCorrect = questionId != null ? _gradedCorrect[questionId] : null;

    return Scaffold(
      backgroundColor: const Color(0xFFF5F6FA),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: _brand),
          onPressed: () => Navigator.pop(context),
        ),
        title: Text(widget.lessonTitle,
            style: const TextStyle(
                fontWeight: FontWeight.bold,
                fontSize: 14,
                color: Color(0xFF1A1C1E))),
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16),
            child: Center(
              child: Text('${_currentIndex + 1} / ${_questions.length}',
                  style: const TextStyle(
                      fontWeight: FontWeight.w600, color: _brand)),
            ),
          ),
        ],
      ),
      body: Column(
        children: [
          LinearProgressIndicator(
            value: (_currentIndex + 1) / _questions.length,
            backgroundColor: Colors.grey.shade200,
            valueColor: const AlwaysStoppedAnimation<Color>(_brand),
            minHeight: 4,
          ),
          Expanded(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(20),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Container(
                    padding:
                        const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                    decoration: BoxDecoration(
                      color: _brand.withValues(alpha: 0.1),
                      borderRadius: BorderRadius.circular(20),
                    ),
                    child: Text('Question ${_currentIndex + 1}',
                        style: const TextStyle(
                            color: _brand,
                            fontWeight: FontWeight.w600,
                            fontSize: 13)),
                  ),
                  const SizedBox(height: 16),
                  Text(
                    question['questionText'] ?? '',
                    style: const TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                        color: Color(0xFF1A1C1E),
                        height: 1.4),
                  ),
                  const SizedBox(height: 24),
                  ...options.asMap().entries.map((entry) {
                    final label =
                        String.fromCharCode(65 + entry.key); // A, B, C...
                    final text = entry.value;
                    final isSelected = selected == text;

                    Color borderColor = Colors.grey.shade200;
                    Color bgColor = Colors.white;
                    Color textColor = Colors.black87;
                    Widget? trailingIcon;

                    if (_submitted && isSelected) {
                      if (isCorrect == true) {
                        borderColor = Colors.green;
                        bgColor = Colors.green.withValues(alpha: 0.08);
                        textColor = Colors.green.shade700;
                        trailingIcon =
                            const Icon(Icons.check_circle, color: Colors.green);
                      } else if (isCorrect == false) {
                        borderColor = Colors.red;
                        bgColor = Colors.red.withValues(alpha: 0.08);
                        textColor = Colors.red.shade700;
                        trailingIcon =
                            const Icon(Icons.cancel, color: Colors.red);
                      }
                    } else if (isSelected) {
                      borderColor = _brand;
                      bgColor = _brand.withValues(alpha: 0.08);
                      textColor = _brand;
                    }

                    return GestureDetector(
                      onTap: () => _selectAnswer(text),
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
                            width: 32,
                            height: 32,
                            decoration: BoxDecoration(
                              color: isSelected
                                  ? borderColor
                                  : Colors.grey.shade100,
                              shape: BoxShape.circle,
                            ),
                            child: Center(
                              child: Text(label,
                                  style: TextStyle(
                                      fontWeight: FontWeight.bold,
                                      color: isSelected
                                          ? Colors.white
                                          : Colors.grey.shade600)),
                            ),
                          ),
                          const SizedBox(width: 12),
                          Expanded(
                              child: Text(text,
                                  style: TextStyle(
                                      fontSize: 15, color: textColor))),
                          if (trailingIcon != null) trailingIcon,
                        ]),
                      ),
                    );
                  }),
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
                            backgroundColor: _brand,
                            padding: const EdgeInsets.symmetric(vertical: 14)),
                        child: const Text('Next',
                            style: TextStyle(color: Colors.white)),
                      )
                    : ElevatedButton(
                        onPressed:
                            !_submitted && selected != null ? _submit : null,
                        style: ElevatedButton.styleFrom(
                            backgroundColor: Colors.green,
                            padding: const EdgeInsets.symmetric(vertical: 14)),
                        child: Text(
                          _submitted ? 'Submitted ✓' : 'Submit Quiz',
                          style: const TextStyle(
                              color: Colors.white, fontWeight: FontWeight.bold),
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
