import 'package:flutter/material.dart';
import 'dart:async';
import 'quiz_result_screen.dart';
import 'quiz_failure_screen.dart';

class QuizScreenPage extends StatefulWidget {
  const QuizScreenPage({
    super.key,
    this.quizTitle = 'Force 1.1',
    this.totalQuestions = 10,
    this.timeLimit = 600, // 10 minutes in seconds
  });

  final String quizTitle;
  final int totalQuestions;
  final int timeLimit;

  @override
  State<QuizScreenPage> createState() => _QuizScreenPageState();
}

class _QuizScreenPageState extends State<QuizScreenPage> {
  // Timer Logic
  late int _secondsRemaining;
  Timer? _timer;
  late int _currentQuestionIndex;

  // State for MCQ selections (6 questions)
  late List<int?> selectedMcqOptions;
  
  // State for space filling questions
  late List<TextEditingController> spaceFillAnswers;

  // Mock questions data
  late List<Map<String, dynamic>> quizQuestions;

  @override
  void initState() {
    super.initState();
    _secondsRemaining = widget.timeLimit;
    _currentQuestionIndex = 0;
    selectedMcqOptions = List.filled(6, null);
    spaceFillAnswers = List.generate(4, (_) => TextEditingController());
    
    // Initialize quiz questions
    _initializeQuestions();
    _startTimer();
  }

  void _initializeQuestions() {
    quizQuestions = [
      {
        'type': 'mcq',
        'question': 'A block of mass 5 kg rests on a frictionless inclined plane angled at 30° to the horizontal. What is the magnitude of the normal force exerted on the block? (Assume g = 9.8 m/s²)',
        'options': ['24.5 N', '42.4 N', '49.0 N', '84.8 N'],
        'correctOption': 1,
        'hasImage': true,
        'imageUrl': 'https://i.ytimg.com/vi/v7ffr0_WRxc/hq720.jpg?sqp=-oaymwEhCK4FEIIDSFryq4qpAxMIARUAAAAAGAElAADIQj0AgKJD&rs=AOn4CLBZE08gx81q7xDN-UyClS2El_nVeA',
      },
      {
        'type': 'mcq',
        'question': 'What is the SI unit of force?',
        'options': ['Newton', 'Pascal', 'Joule', 'Watt'],
        'correctOption': 0,
        'hasImage': false,
      },
      {
        'type': 'mcq',
        'question': 'Which of Newton\'s Laws states that F = ma?',
        'options': ['First Law', 'Second Law', 'Third Law', 'Law of Gravitation'],
        'correctOption': 1,
        'hasImage': false,
      },
      {
        'type': 'mcq',
        'question': 'What is the magnitude of acceleration due to gravity?',
        'options': ['8.8 m/s²', '9.8 m/s²', '10.8 m/s²', '11.8 m/s²'],
        'correctOption': 1,
        'hasImage': false,
      },
      {
        'type': 'mcq',
        'question': 'Friction acts in which direction?',
        'options': ['Same as motion', 'Opposite to motion', 'Perpendicular to motion', 'Random direction'],
        'correctOption': 1,
        'hasImage': false,
      },
      {
        'type': 'mcq',
        'question': 'What does Newton\'s Third Law state?',
        'options': ['Every action has equal and\n opposite reaction', 'F=ma', 'Objects at rest stay at rest', 'None of the above'],
        'correctOption': 0,
        'hasImage': false,
      },
      {
        'type': 'fillblank',
        'question': 'The force of gravity on an object is called its ___________.',
        'answerLength': 6,
        'correctAnswer': 'weight',
      },
      {
        'type': 'fillblank',
        'question': 'A ___________ force is a push or pull on an object.',
        'answerLength': 5,
        'correctAnswer': 'force',
      },
      {
        'type': 'fillblank',
        'question': 'The tendency of an object to resist changes in motion is called ___________.',
        'answerLength': 7,
        'correctAnswer': 'inertia',
      },
      {
        'type': 'fillblank',
        'question': 'When two objects rub against each other, ___________ is produced.',
        'answerLength': 8,
        'correctAnswer': 'friction',
      },
    ];
  }

  void _startTimer() {
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (_secondsRemaining > 0) {
        setState(() => _secondsRemaining--);
      } else {
        _timer?.cancel();
        _showTimeUpDialog();
      }
    });
  }

  String _formatTime(int seconds) {
    int mins = seconds ~/ 60;
    int secs = seconds % 60;
    return '${mins.toString().padLeft(2, '0')}:${secs.toString().padLeft(2, '0')}';
  }

  void _showTimeUpDialog() {
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => AlertDialog(
        title: const Text('Time\'s Up'),
        content: const Text('Your quiz time has ended.'),
        actions: [
          ElevatedButton(
            onPressed: () {
              Navigator.pop(context);
              Navigator.pop(context);
            },
            child: const Text('OK'),
          ),
        ],
      ),
    );
  }

  void _submitQuiz() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Submit Quiz?'),
        content: const Text('Are you sure you want to submit your answers?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(context); // Close the dialog
              
              final timeTaken = _formatTime(widget.timeLimit - _secondsRemaining);
              
              int correctAnswers = 0;
              for (int i = 0; i < quizQuestions.length; i++) {
                var q = quizQuestions[i];
                if (q['type'] == 'mcq') {
                  if (selectedMcqOptions[i] == q['correctOption']) {
                    correctAnswers++;
                  }
                } else if (q['type'] == 'fillblank') {
                  int fillIndex = i - 6;
                  if (fillIndex >= 0 && fillIndex < spaceFillAnswers.length) {
                    String userAnswer = spaceFillAnswers[fillIndex].text.trim().toLowerCase();
                    if (userAnswer == q['correctAnswer'].toString().toLowerCase()) {
                      correctAnswers++;
                    }
                  }
                }
              }
              
              double scorePercentage = (correctAnswers / widget.totalQuestions) * 100;
              
              if (scorePercentage < 45) {
                Navigator.pushReplacement(
                  context,
                  MaterialPageRoute(
                    builder: (context) => QuizFailureScreen(
                      totalQuestions: widget.totalQuestions,
                      incorrectAnswers: widget.totalQuestions - correctAnswers,
                      timeTaken: timeTaken,
                    ),
                  ),
                );
              } else {
                Navigator.pushReplacement(
                  context,
                  MaterialPageRoute(
                    builder: (context) => QuizResultScreen(
                      totalQuestions: widget.totalQuestions,
                      correctAnswers: correctAnswers,
                      timeTaken: timeTaken,
                    ),
                  ),
                );
              }
            },
            child: const Text('Submit'),
          ),
        ],
      ),
    );
  }

  @override
  void dispose() {
    _timer?.cancel();
    for (var controller in spaceFillAnswers) {
      controller.dispose();
    }
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final currentQuestion = quizQuestions[_currentQuestionIndex];
    final isFirstQuestion = _currentQuestionIndex == 0;
    final isLastQuestion = _currentQuestionIndex == quizQuestions.length - 1;

    return WillPopScope(
      onWillPop: () async {
        showDialog(
          context: context,
          builder: (context) => AlertDialog(
            title: const Text('Exit Quiz?'),
            content: const Text('Your progress will be lost if you exit.'),
            actions: [
              TextButton(
                onPressed: () => Navigator.pop(context),
                child: const Text('Cancel'),
              ),
              ElevatedButton(
                onPressed: () {
                  Navigator.pop(context);
                  Navigator.pop(context);
                },
                style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
                child: const Text('Exit'),
              ),
            ],
          ),
        );
        return false;
      },
      child: Scaffold(
        backgroundColor: const Color(0xFFF8F9FE),
        appBar: AppBar(
          backgroundColor: Colors.white,
          elevation: 0,
          leading: IconButton(
            icon: const Icon(Icons.close, color: Color(0xFF2196F3)),
            onPressed: () {
              showDialog(
                context: context,
                builder: (context) => AlertDialog(
                  title: const Text('Exit Quiz?'),
                  content: const Text('Your progress will be lost if you exit.'),
                  actions: [
                    TextButton(
                      onPressed: () => Navigator.pop(context),
                      child: const Text('Cancel'),
                    ),
                    ElevatedButton(
                      onPressed: () {
                        Navigator.pop(context);
                        Navigator.pop(context);
                      },
                      style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
                      child: const Text('Exit'),
                    ),
                  ],
                ),
              );
            },
          ),
          title: Text(
            widget.quizTitle,
            style: const TextStyle(color: Color(0xFF1A1C1E), fontWeight: FontWeight.bold),
          ),
          centerTitle: true,
        ),
        body: Column(
          children: [
            // Header with Question Count and Timer
            Container(
              color: Colors.white,
              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 10),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text.rich(
                    TextSpan(
                      text: 'Question ',
                      style: const TextStyle(color: Colors.grey, fontSize: 16),
                      children: [
                        TextSpan(
                          text: '${_currentQuestionIndex + 1} ',
                          style: const TextStyle(color: Colors.black, fontWeight: FontWeight.bold),
                        ),
                        TextSpan(text: '/ ${widget.totalQuestions}'),
                      ],
                    ),
                  ),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                    decoration: BoxDecoration(
                      color: _secondsRemaining < 60 ? const Color(0xFFFFEBEE) : const Color(0xFFEEF2FF),
                      borderRadius: BorderRadius.circular(20),
                    ),
                    child: Row(
                      children: [
                        Icon(
                          Icons.timer_outlined,
                          size: 18,
                          color: _secondsRemaining < 60 ? Colors.red : const Color(0xFF2196F3),
                        ),
                        const SizedBox(width: 8),
                        Text(
                          _formatTime(_secondsRemaining),
                          style: TextStyle(
                            fontWeight: FontWeight.bold,
                            color: _secondsRemaining < 60 ? Colors.red : const Color(0xFF1A1C1E),
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
            const Divider(height: 1),

            // Question Content
            Expanded(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(20),
                child: currentQuestion['type'] == 'mcq'
                    ? _buildMcqQuestion(currentQuestion)
                    : _buildSpaceFillingQuestion(currentQuestion),
              ),
            ),

            // Navigation Buttons
            Container(
              color: Colors.white,
              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 20),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  // Previous Button
                  if (!isFirstQuestion)
                    ElevatedButton.icon(
                      onPressed: () {
                        setState(() => _currentQuestionIndex--);
                      },
                      icon: const Icon(Icons.arrow_back),
                      label: const Text('Previous'),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.grey.shade300,
                        foregroundColor: Colors.black,
                      ),
                    )
                  else
                    const SizedBox(width: 100),

                  // Next or Submit Button
                  if (!isLastQuestion)
                    ElevatedButton.icon(
                      onPressed: () {
                        setState(() => _currentQuestionIndex++);
                      },
                      icon: const Icon(Icons.arrow_forward),
                      label: const Text('Next'),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: const Color(0xFF2196F3),
                      ),
                    )
                  else
                    ElevatedButton(
                      onPressed: _submitQuiz,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: const Color(0xFF2196F3),
                        padding: const EdgeInsets.symmetric(horizontal: 40, vertical: 12),
                      ),
                      child: const Text('Submit Quiz'),
                    ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildMcqQuestion(Map<String, dynamic> question) {
    final int qIndex = quizQuestions.indexOf(question);
    final String questionText = question['question'];
    final List<String> options = question['options'];
    final bool hasImage = question['hasImage'] ?? false;
    final String imageUrl = question['imageUrl'] ?? '';

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        if (hasImage && imageUrl.isNotEmpty)
          Container(
            margin: const EdgeInsets.only(bottom: 20),
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(16),
              border: Border.all(color: Colors.blue.shade100),
            ),
            child: ClipRRect(
              borderRadius: BorderRadius.circular(12),
              child: imageUrl.startsWith('assets/')
                  ? Image.asset(
                      imageUrl,
                      fit: BoxFit.contain,
                      height: 200,
                      errorBuilder: (context, error, stackTrace) {
                        return Container(
                          height: 200,
                          decoration: BoxDecoration(
                            color: Colors.grey.shade200,
                            borderRadius: BorderRadius.circular(12),
                          ),
                          child: const Center(
                            child: Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                Icon(Icons.image_not_supported, size: 50),
                                SizedBox(height: 8),
                                Text('Image not found'),
                              ],
                            ),
                          ),
                        );
                      },
                    )
                  : Image.network(
                      imageUrl,
                      fit: BoxFit.contain,
                      height: 200,
                      errorBuilder: (context, error, stackTrace) {
                        return Container(
                          height: 200,
                          decoration: BoxDecoration(
                            color: Colors.grey.shade200,
                            borderRadius: BorderRadius.circular(12),
                          ),
                          child: const Center(
                            child: Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                Icon(Icons.image_not_supported, size: 50),
                                SizedBox(height: 8),
                                Text('Failed to load image'),
                              ],
                            ),
                          ),
                        );
                      },
                    ),
            ),
          ),
        Text(
          questionText,
          style: const TextStyle(fontSize: 17, fontWeight: FontWeight.bold, height: 1.4),
        ),
        const SizedBox(height: 20),
        ...List.generate(options.length, (oIndex) {
          bool isSelected = selectedMcqOptions[qIndex] == oIndex;
          return GestureDetector(
            onTap: () => setState(() => selectedMcqOptions[qIndex] = oIndex),
            child: Container(
              margin: const EdgeInsets.only(bottom: 12),
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
              decoration: BoxDecoration(
                color: isSelected ? const Color(0xFFEEF2FF) : Colors.white,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(
                  color: isSelected ? const Color(0xFF2196F3) : Colors.grey.shade300,
                  width: isSelected ? 2 : 1,
                ),
              ),
              child: Row(
                children: [
                  Icon(
                    isSelected ? Icons.radio_button_checked : Icons.radio_button_off,
                    color: isSelected ? const Color(0xFF2196F3) : Colors.grey,
                  ),
                  const SizedBox(width: 15),
                  Text(options[oIndex], style: const TextStyle(fontSize: 16)),
                ],
              ),
            ),
          );
        }),
      ],
    );
  }

  Widget _buildSpaceFillingQuestion(Map<String, dynamic> question) {
    final String questionText = question['question'];
    final int answerIndex = _currentQuestionIndex - 6;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          questionText,
          style: const TextStyle(fontSize: 17, fontWeight: FontWeight.bold, height: 1.4),
        ),
        const SizedBox(height: 30),
        TextField(
          controller: spaceFillAnswers[answerIndex],
          decoration: InputDecoration(
            hintText: "Type your answer here...",
            fillColor: Colors.white,
            filled: true,
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: BorderSide(color: Colors.grey.shade300),
            ),
            enabledBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: BorderSide(color: Colors.grey.shade300),
            ),
            focusedBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: Color(0xFF2196F3)),
            ),
          ),
          maxLines: 3,
        ),
      ],
    );
  }
}
