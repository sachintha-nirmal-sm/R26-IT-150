import "package:cloud_firestore/cloud_firestore.dart";
import "package:firebase_auth/firebase_auth.dart";
import "package:flutter/material.dart";

import "rag_chat_service.dart";

class ChatbotScreen extends StatefulWidget {
  const ChatbotScreen({super.key});

  @override
  State<ChatbotScreen> createState() => _ChatbotScreenState();
}

class _ChatbotScreenState extends State<ChatbotScreen> {
  final RagChatService _chatService = RagChatService();
  final TextEditingController _inputController = TextEditingController();
  final ScrollController _scrollController = ScrollController();

  final List<_ChatMessage> _messages = [];

  int? _grade;
  String? _sessionId;
  bool _sending = false;
  bool _loadingGrade = true;
  String? _gradeError;

  List<String> get _quickQuestions {
    final items = [
      "Explain Newton's First Law",
      "What is the formula for momentum?",
      "How does friction affect motion?",
      "Define kinetic energy",
    ];
    if (_grade != null && _grade! > 6) {
      items.add("Explain more simply");
    }
    return items;
  }

  @override
  void initState() {
    super.initState();
    _loadStudentGrade();
  }

  @override
  void dispose() {
    _inputController.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  int? _parseGrade(dynamic raw) {
    if (raw is int) {
      return raw;
    }
    final match = RegExp(r"(\d{1,2})").firstMatch(raw?.toString() ?? "");
    if (match == null) {
      return null;
    }
    final value = int.tryParse(match.group(1)!);
    if (value == null || value < 6 || value > 13) {
      return null;
    }
    return value;
  }

  Future<void> _loadStudentGrade() async {
    final user = FirebaseAuth.instance.currentUser;
    if (user == null) {
      setState(() {
        _loadingGrade = false;
        _gradeError = "Sign in as a student to use the grade-adaptive chatbot.";
      });
      return;
    }

    try {
      final snap = await FirebaseFirestore.instance
          .collection("users")
          .doc(user.uid)
          .get();
      final data = snap.data() ?? {};
      final grade = _parseGrade(data["currentGrade"]) ?? _parseGrade(data["grade"]);
      if (!mounted) {
        return;
      }
      setState(() {
        _grade = grade;
        _loadingGrade = false;
        _gradeError = grade == null
            ? "Your profile has no grade. Ask an admin to set currentGrade."
            : null;
        _messages.add(
          _ChatMessage(
            text: grade == null
                ? "I cannot answer yet because your account has no grade."
                : "Hi! You are signed in as Grade $grade. "
                    "I will answer from the Grade $grade lesson PDFs your teacher uploaded. "
                    "If a topic is only in a higher grade, I will tell you. "
                    "If you need it simpler, ask me to explain more simply and I will use Grade ${grade - 1} notes.",
            isUser: false,
          ),
        );
      });
    } catch (error) {
      if (!mounted) {
        return;
      }
      setState(() {
        _loadingGrade = false;
        _gradeError = "Could not load your grade.\n$error";
      });
    }
  }

  Future<void> _send(String question) async {
    final text = question.trim();
    if (text.isEmpty || _sending || _grade == null) {
      return;
    }

    setState(() {
      _sending = true;
      _messages.add(_ChatMessage(text: text, isUser: true));
      _inputController.clear();
    });
    _scrollToEnd();

    try {
      final result = await _chatService.send(
        message: text,
        sessionId: _sessionId,
      );
      if (!mounted) {
        return;
      }
      setState(() {
        _sessionId = result.sessionId;
        if (result.grade != null) {
          _grade = result.grade;
        }
        _messages.add(_ChatMessage(text: result.answer, isUser: false));
        _sending = false;
      });
    } catch (error) {
      if (!mounted) {
        return;
      }
      setState(() {
        _messages.add(
          _ChatMessage(
            text:
                "Could not reach the RAG chatbot. Start the backend (uvicorn) and check your connection.\n$error",
            isUser: false,
          ),
        );
        _sending = false;
      });
    }
    _scrollToEnd();
  }

  void _scrollToEnd() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!_scrollController.hasClients) {
        return;
      }
      _scrollController.animateTo(
        _scrollController.position.maxScrollExtent,
        duration: const Duration(milliseconds: 250),
        curve: Curves.easeOut,
      );
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F6FA),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        title: const Row(
          children: [
            Icon(Icons.science, color: Colors.blue),
            SizedBox(width: 8),
            Text(
              "Physics Lab",
              style: TextStyle(color: Colors.black),
            ),
          ],
        ),
        actions: [
          if (_grade != null)
            Padding(
              padding: const EdgeInsets.only(right: 8),
              child: Center(
                child: Chip(
                  label: Text("Grade $_grade notes"),
                  backgroundColor: const Color(0xFFE3F2FD),
                  visualDensity: VisualDensity.compact,
                ),
              ),
            ),
          Padding(
            padding: const EdgeInsets.only(right: 12),
            child: GestureDetector(
              onTap: () => Navigator.pushNamed(context, "/profile"),
              child: const _ProfileAvatar(),
            ),
          ),
        ],
      ),
      body: _loadingGrade
          ? const Center(child: CircularProgressIndicator())
          : _gradeError != null && _messages.isEmpty
              ? Center(
                  child: Padding(
                    padding: const EdgeInsets.all(24),
                    child: Text(
                      _gradeError!,
                      textAlign: TextAlign.center,
                    ),
                  ),
                )
              : Column(
                  children: [
                    Padding(
                      padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
                      child: Align(
                        alignment: Alignment.centerLeft,
                        child: Wrap(
                          spacing: 8,
                          runSpacing: 8,
                          children: _quickQuestions
                              .map(
                                (question) => ActionChip(
                                  label: Text(question),
                                  onPressed:
                                      _sending ? null : () => _send(question),
                                ),
                              )
                              .toList(),
                        ),
                      ),
                    ),
                    Expanded(
                      child: ListView.builder(
                        controller: _scrollController,
                        padding: const EdgeInsets.all(16),
                        itemCount: _messages.length + (_sending ? 1 : 0),
                        itemBuilder: (context, index) {
                          if (index >= _messages.length) {
                            return const _ChatBubble(
                              message: _ChatMessage(
                                text: "Thinking…",
                                isUser: false,
                              ),
                            );
                          }
                          return _ChatBubble(message: _messages[index]);
                        },
                      ),
                    ),
                    _InputBar(
                      controller: _inputController,
                      enabled: !_sending && _grade != null,
                      onSend: () => _send(_inputController.text),
                    ),
                  ],
                ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: 0,
        type: BottomNavigationBarType.fixed,
        onTap: (index) {
          if (index == 0) {
            Navigator.pushNamed(context, '/home');
          } else if (index == 1) {
            Navigator.pushNamed(context, '/lesson-list');
          } else if (index == 2) {
            Navigator.pushNamed(context, '/practical-home');
          } else if (index == 3) {
            Navigator.pushNamed(context, "/profile");
          }
        },
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.home),
            label: "Home",
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.menu_book),
            label: "Lessons",
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.science),
            label: "Labs",
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.person),
            label: "Profile",
          ),
        ],
      ),
    );
  }
}

class _ChatMessage {
  const _ChatMessage({required this.text, required this.isUser});

  final String text;
  final bool isUser;
}

class _ProfileAvatar extends StatelessWidget {
  const _ProfileAvatar();

  @override
  Widget build(BuildContext context) {
    return CircleAvatar(
      backgroundColor: const Color(0xFFE9ECF5),
      child: ClipOval(
        child: Image.asset(
          "assets/profile.jpg",
          width: 36,
          height: 36,
          fit: BoxFit.cover,
          errorBuilder: (context, error, stackTrace) {
            return const Icon(Icons.person, color: Colors.blueGrey);
          },
        ),
      ),
    );
  }
}

class _ChatBubble extends StatelessWidget {
  const _ChatBubble({required this.message});

  final _ChatMessage message;

  @override
  Widget build(BuildContext context) {
    final alignment =
        message.isUser ? Alignment.centerRight : Alignment.centerLeft;
    final bubbleColor =
        message.isUser ? const Color(0xFFECE6D8) : const Color(0xFF6E6B67);
    final textColor = message.isUser ? Colors.black : Colors.white;
    final radius = message.isUser
        ? const BorderRadius.only(
            topLeft: Radius.circular(16),
            topRight: Radius.circular(16),
            bottomLeft: Radius.circular(16),
          )
        : const BorderRadius.only(
            topLeft: Radius.circular(16),
            topRight: Radius.circular(16),
            bottomRight: Radius.circular(16),
          );

    return Align(
      alignment: alignment,
      child: Container(
        margin: const EdgeInsets.only(bottom: 12),
        padding: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: bubbleColor,
          borderRadius: radius,
        ),
        child: Text(
          message.text,
          style: TextStyle(color: textColor),
        ),
      ),
    );
  }
}

class _InputBar extends StatelessWidget {
  const _InputBar({
    required this.controller,
    required this.onSend,
    required this.enabled,
  });

  final TextEditingController controller;
  final VoidCallback onSend;
  final bool enabled;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      color: Colors.white,
      child: Row(
        children: [
          const Icon(Icons.add),
          const SizedBox(width: 8),
          Expanded(
            child: TextField(
              controller: controller,
              enabled: enabled,
              textInputAction: TextInputAction.send,
              onSubmitted: enabled ? (_) => onSend() : null,
              decoration: const InputDecoration(
                hintText: "TYPE YOUR PHYSICS QUERY...",
                border: InputBorder.none,
              ),
            ),
          ),
          GestureDetector(
            onTap: enabled ? onSend : null,
            child: Container(
              padding: const EdgeInsets.all(10),
              decoration: BoxDecoration(
                color: enabled ? Colors.blue : Colors.blue.shade200,
                borderRadius: BorderRadius.circular(8),
              ),
              child: const Icon(Icons.send, color: Colors.white),
            ),
          ),
        ],
      ),
    );
  }
}
