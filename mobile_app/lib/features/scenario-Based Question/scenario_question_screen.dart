import "package:cloud_firestore/cloud_firestore.dart";
import "package:firebase_auth/firebase_auth.dart";
import "package:flutter/material.dart";

import "news_scenario_service.dart";

class ScenarioQuestionScreen extends StatefulWidget {
  const ScenarioQuestionScreen({super.key});

  @override
  State<ScenarioQuestionScreen> createState() => _ScenarioQuestionScreenState();
}

class _ScenarioQuestionScreenState extends State<ScenarioQuestionScreen> {
  final NewsScenarioService _service = NewsScenarioService();
  final TextEditingController _newsController = TextEditingController();
  final TextEditingController _answerController = TextEditingController();

  List<NewsSample> _samples = [];
  int? _grade;
  bool _loading = false;
  String? _error;
  NewsScenarioResult? _scenario;
  NewsEvalResult? _eval;

  @override
  void initState() {
    super.initState();
    _bootstrap();
  }

  @override
  void dispose() {
    _newsController.dispose();
    _answerController.dispose();
    super.dispose();
  }

  Future<void> _bootstrap() async {
    try {
      final samples = await _service.samples();
      final uid = FirebaseAuth.instance.currentUser?.uid;
      int? grade;
      if (uid != null) {
        final snap = await FirebaseFirestore.instance.collection("users").doc(uid).get();
        final data = snap.data() ?? {};
        final raw = data["currentGrade"] ?? data["grade"];
        if (raw is int) {
          grade = raw;
        } else {
          grade = int.tryParse(RegExp(r"\d{1,2}").firstMatch("$raw")?.group(0) ?? "");
        }
      }
      if (!mounted) {
        return;
      }
      setState(() {
        _samples = samples;
        _grade = grade;
        if (samples.isNotEmpty) {
          _newsController.text = samples.first.text;
        }
      });
    } catch (error) {
      if (mounted) {
        setState(() => _error = error.toString());
      }
    }
  }

  Future<void> _generate() async {
    final text = _newsController.text.trim();
    if (text.isEmpty || _loading) {
      return;
    }
    setState(() {
      _loading = true;
      _error = null;
      _eval = null;
      _scenario = null;
      _answerController.clear();
    });
    try {
      final result = await _service.generate(text: text, grade: _grade);
      if (!mounted) {
        return;
      }
      setState(() {
        _scenario = result;
        _loading = false;
      });
    } catch (error) {
      if (!mounted) {
        return;
      }
      setState(() {
        _loading = false;
        _error = error.toString();
      });
    }
  }

  Future<void> _submit() async {
    final scenario = _scenario;
    final answer = _answerController.text.trim();
    if (scenario == null || !scenario.accepted || answer.isEmpty || _loading) {
      return;
    }
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final result = await _service.evaluate(
        question: scenario.question ?? "",
        referenceAnswer: scenario.referenceAnswer ?? "",
        studentAnswer: answer,
        scenario: scenario.scenario,
      );
      if (!mounted) {
        return;
      }
      setState(() {
        _eval = result;
        _loading = false;
      });
    } catch (error) {
      if (!mounted) {
        return;
      }
      setState(() {
        _loading = false;
        _error = error.toString();
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F5F7),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.symmetric(horizontal: 20),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const _TopBar(),
              if (_grade != null)
                Padding(
                  padding: const EdgeInsets.only(bottom: 8),
                  child: Chip(label: Text("Answering as Grade $_grade")),
                ),
              const Text(
                "News-based physics question",
                style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold, color: Colors.blue),
              ),
              const SizedBox(height: 8),
              const Text(
                "Paste a news item. Model 1 checks if it is physics. If it is, you get a scenario and question. Model 2 then marks your answer.",
              ),
              const SizedBox(height: 16),
              Wrap(
                spacing: 8,
                runSpacing: 8,
                children: _samples
                    .map(
                      (sample) => ActionChip(
                        label: Text(sample.title),
                        onPressed: _loading
                            ? null
                            : () {
                                _newsController.text = sample.text;
                                setState(() {
                                  _scenario = null;
                                  _eval = null;
                                });
                              },
                      ),
                    )
                    .toList(),
              ),
              const SizedBox(height: 16),
              TextField(
                controller: _newsController,
                minLines: 3,
                maxLines: 6,
                enabled: !_loading,
                decoration: InputDecoration(
                  hintText: "Paste a news headline or short description...",
                  filled: true,
                  fillColor: Colors.white,
                  border: OutlineInputBorder(borderRadius: BorderRadius.circular(16)),
                ),
              ),
              const SizedBox(height: 12),
              SizedBox(
                width: double.infinity,
                height: 52,
                child: ElevatedButton(
                  onPressed: _loading ? null : _generate,
                  child: Text(_loading && _scenario == null ? "Checking news..." : "Generate question"),
                ),
              ),
              if (_error != null) ...[
                const SizedBox(height: 12),
                Text(_error!, style: const TextStyle(color: Colors.red)),
              ],
              if (_scenario != null) ...[
                const SizedBox(height: 20),
                _InfoCard(
                  title: _scenario!.accepted
                      ? "Model 1: Physics (${(_scenario!.confidence * 100).round()}%)"
                      : "Model 1: Non-Physics (${(_scenario!.confidence * 100).round()}%)",
                  body: _scenario!.accepted
                      ? "${_scenario!.topic ?? 'Physics topic'}\n${_scenario!.gradeNote ?? ''}"
                      : (_scenario!.message ?? "This news is not physics."),
                  color: _scenario!.accepted ? Colors.green : Colors.orange,
                ),
              ],
              if (_scenario?.accepted == true) ...[
                const SizedBox(height: 16),
                _InfoCard(title: "Scenario", body: _scenario!.scenario ?? ""),
                const SizedBox(height: 12),
                _InfoCard(title: "Question", body: _scenario!.question ?? ""),
                const SizedBox(height: 16),
                TextField(
                  controller: _answerController,
                  minLines: 4,
                  maxLines: 8,
                  enabled: !_loading,
                  decoration: InputDecoration(
                    hintText: "Type your answer...",
                    filled: true,
                    fillColor: Colors.white,
                    border: OutlineInputBorder(borderRadius: BorderRadius.circular(16)),
                  ),
                ),
                const SizedBox(height: 12),
                SizedBox(
                  width: double.infinity,
                  height: 52,
                  child: ElevatedButton(
                    onPressed: _loading ? null : _submit,
                    child: Text(_loading && _eval == null ? "Marking..." : "Submit answer"),
                  ),
                ),
              ],
              if (_eval != null) ...[
                const SizedBox(height: 20),
                _ResultCard(result: _eval!),
              ],
              const SizedBox(height: 28),
            ],
          ),
        ),
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: 1,
        type: BottomNavigationBarType.fixed,
        onTap: (index) {
          if (index == 0) {
            Navigator.pushNamed(context, "/home");
          } else if (index == 1) {
            Navigator.pushNamed(context, "/lesson-list");
          } else if (index == 2) {
            Navigator.pushNamed(context, "/practical-home");
          } else if (index == 3) {
            Navigator.pushNamed(context, "/profile");
          }
        },
        items: const [
          BottomNavigationBarItem(icon: Icon(Icons.home_outlined), label: "Home"),
          BottomNavigationBarItem(icon: Icon(Icons.menu_book_outlined), label: "Lessons"),
          BottomNavigationBarItem(icon: Icon(Icons.science), label: "Labs"),
          BottomNavigationBarItem(icon: Icon(Icons.person_outline), label: "Profile"),
        ],
      ),
    );
  }
}

class _TopBar extends StatelessWidget {
  const _TopBar();

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        IconButton(
          onPressed: () => Navigator.pop(context),
          icon: const Icon(Icons.arrow_back_ios_new, color: Colors.blue),
        ),
        const Expanded(
          child: Center(
            child: Text(
              "Scenario Question",
              style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold, color: Colors.blue),
            ),
          ),
        ),
        const SizedBox(width: 48),
      ],
    );
  }
}

class _InfoCard extends StatelessWidget {
  const _InfoCard({
    required this.title,
    required this.body,
    this.color = Colors.blue,
  });

  final String title;
  final String body;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(18),
        border: Border(left: BorderSide(color: color, width: 4)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(title, style: TextStyle(color: color, fontWeight: FontWeight.bold)),
          const SizedBox(height: 8),
          Text(body, style: const TextStyle(height: 1.45)),
        ],
      ),
    );
  }
}

class _ResultCard extends StatelessWidget {
  const _ResultCard({required this.result});

  final NewsEvalResult result;

  Color get _color {
    switch (result.label) {
      case "correct":
        return Colors.green;
      case "partial":
        return Colors.orange;
      default:
        return Colors.red;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: _color.withOpacity(0.4)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            "Model 2: ${result.displayLabel}",
            style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: _color),
          ),
          const SizedBox(height: 8),
          Text("Confidence ${(result.confidence * 100).round()}%"),
          const SizedBox(height: 12),
          Text(result.feedback, style: const TextStyle(height: 1.45)),
          const SizedBox(height: 16),
          Text("Relevance ${result.relevance}%   Completeness ${result.completeness}%   Creativity ${result.creativity}%"),
          if (result.elapsedMs != null)
            Text("Marked in ${result.elapsedMs} ms", style: const TextStyle(color: Colors.black54)),
        ],
      ),
    );
  }
}
