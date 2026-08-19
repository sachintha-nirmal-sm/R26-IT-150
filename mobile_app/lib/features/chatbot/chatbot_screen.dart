import "package:flutter/material.dart";

class ChatbotScreen extends StatefulWidget {
  const ChatbotScreen({super.key});

  @override
  State<ChatbotScreen> createState() => _ChatbotScreenState();
}

class _ChatbotScreenState extends State<ChatbotScreen> {
  final List<_ChatMessage> _messages = [
    const _ChatMessage(
      text: "Hello, Alex! How can I help you today?",
      isUser: false,
    ),
  ];

  final List<String> _quickQuestions = [
    "Explain Newton's First Law",
    "What is the formula for momentum?",
    "How does friction affect motion?",
    "Define kinetic energy",
  ];

  void _sendQuickQuestion(String question) {
    setState(() {
      _messages.add(_ChatMessage(text: question, isUser: true));
      _messages.add(
        _ChatMessage(
          text: "Got it. Here's a quick explanation for: $question",
          isUser: false,
        ),
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
        actions: const [
          Padding(
            padding: EdgeInsets.only(right: 12),
            child: _ProfileAvatar(),
          ),
        ],
      ),
      body: Column(
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
                        onPressed: () => _sendQuickQuestion(question),
                      ),
                    )
                    .toList(),
              ),
            ),
          ),
          Expanded(
            child: ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: _messages.length,
              itemBuilder: (context, index) {
                final message = _messages[index];
                return _ChatBubble(message: message);
              },
            ),
          ),
          const _InputBar(),
        ],
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: 0,
        type: BottomNavigationBarType.fixed,
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
  const _InputBar();

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      color: Colors.white,
      child: Row(
        children: [
          const Icon(Icons.add),
          const SizedBox(width: 8),
          const Expanded(
            child: TextField(
              decoration: InputDecoration(
                hintText: "TYPE YOUR PHYSICS QUERY...",
                border: InputBorder.none,
              ),
            ),
          ),
          Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: Colors.blue,
              borderRadius: BorderRadius.circular(8),
            ),
            child: const Icon(Icons.send, color: Colors.white),
          ),
        ],
      ),
    );
  }
}
