import 'package:flutter/material.dart';
import 'package:mobile_app/features/games/games_list/game_list_data.dart';

class LessonGamesScreen extends StatefulWidget {
  final String lessonTitle;
  final String lessonTopic;
  final String grade;

  const LessonGamesScreen({
    super.key,
    required this.lessonTitle,
    required this.lessonTopic,
    required this.grade,
  });

  @override
  State<LessonGamesScreen> createState() => _LessonGamesScreenState();
}

class _LessonGamesScreenState extends State<LessonGamesScreen> {
  late List<GameItem> _relevantGames;

  @override
  void initState() {
    super.initState();
    print('🔍 LessonGamesScreen - lessonTitle: "${widget.lessonTitle}"');
    print('🔍 LessonGamesScreen - lessonTopic: "${widget.lessonTopic}"');
    print('🔍 LessonGamesScreen - grade: "${widget.grade}"');
    _relevantGames = _filterGamesByTopic();
    print('🔍 Filtered games count: ${_relevantGames.length}');
    for (var game in _relevantGames) {
      print('  ✓ ${game.title} (${game.topic})');
    }
    print('🔍 All available games for grade: ${widget.grade}');
    final gradeGames = allGames.where((g) => g.grade == widget.grade).toList();
    for (var game in gradeGames) {
      print('  - ${game.title} (topic: "${game.topic}")');
    }
  }

  List<GameItem> _filterGamesByTopic() {
    // Extract grade number from "Grade 10" or "Grade 10 Physics" format
    String _extractGradeNumber(String grade) {
      // Extract "10" from "Grade 10" or "Grade 10 Physics"
      final match = RegExp(r'Grade\s*(\d+)').firstMatch(grade);
      return match?.group(1) ?? grade;
    }

    final lessonGradeNum = _extractGradeNumber(widget.grade);

    return allGames
        .where((game) {
          // Match grade by number only
          final gameGradeNum = _extractGradeNumber(game.grade);

          if (gameGradeNum != lessonGradeNum) {
            print('🔍 Grade mismatch: game="${game.grade}" ($gameGradeNum) vs lesson="${widget.grade}" ($lessonGradeNum)');
            return false;
          }

          final gameTopic = game.topic.toLowerCase();
          final lessonTopic = widget.lessonTopic.toLowerCase();

          // Match if: exact match OR game topic is contained in lesson title OR lesson title is contained in game topic
          final topicMatch = gameTopic == lessonTopic ||
              gameTopic.contains(lessonTopic) ||
              lessonTopic.contains(gameTopic);

          if (topicMatch) {
            print('🔍 ✓ MATCH: "${game.title}" (topic: "${game.topic}") matches lesson "${widget.lessonTopic}"');
          } else {
            print('🔍 Topic mismatch: game topic="${game.topic}" vs lesson topic="${widget.lessonTopic}"');
          }

          return topicMatch;
        })
        .toList();
  }

  void _navigateToGame(GameItem game) {
    // Navigate based on game route
    Navigator.pushNamed(context, game.route);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F6FA),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Color(0xFF2196F3)),
          onPressed: () => Navigator.pop(context),
        ),
        title: Text(
          '${widget.lessonTitle} - Games',
          style: const TextStyle(
            fontWeight: FontWeight.bold,
            fontSize: 16,
            color: Color(0xFF1A1C1E),
          ),
        ),
        centerTitle: true,
      ),
      body: _relevantGames.isEmpty
          ? _buildEmptyState()
          : _buildGamesList(),
    );
  }

  Widget _buildEmptyState() {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.sports_esports_outlined,
              size: 64, color: Colors.grey.shade300),
          const SizedBox(height: 16),
          Text(
            'No games available for ${widget.lessonTitle}',
            style: const TextStyle(fontSize: 16, color: Colors.grey),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 8),
          const Text(
            'Check back later!',
            style: TextStyle(color: Colors.grey),
          ),
        ],
      ),
    );
  }

  Widget _buildGamesList() {
    return ListView.builder(
      padding: const EdgeInsets.all(16),
      itemCount: _relevantGames.length,
      itemBuilder: (context, index) {
        final game = _relevantGames[index];
        return _buildGameCard(game);
      },
    );
  }

  Widget _buildGameCard(GameItem game) {
    return GestureDetector(
      onTap: () => _navigateToGame(game),
      child: Container(
        margin: const EdgeInsets.only(bottom: 12),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(14),
          border: Border.all(
            color: const Color(0xFF2196F3).withOpacity(0.25),
            width: 1.5,
          ),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.04),
              blurRadius: 8,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            children: [
              // Game icon
              Container(
                width: 60,
                height: 60,
                decoration: BoxDecoration(
                  color: const Color(0xFFE8F1FB),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Center(
                  child: Text(
                    game.icon,
                    style: const TextStyle(fontSize: 32),
                  ),
                ),
              ),
              const SizedBox(width: 14),
              // Game info
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      game.title,
                      style: const TextStyle(
                        fontWeight: FontWeight.w600,
                        fontSize: 15,
                        color: Color(0xFF1A1C1E),
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      game.topic,
                      style: const TextStyle(
                        fontSize: 12,
                        color: Colors.grey,
                      ),
                    ),
                    const SizedBox(height: 8),
                    Row(
                      children: [
                        const Icon(Icons.timer_outlined,
                            size: 14, color: Colors.grey),
                        const SizedBox(width: 4),
                        Text(
                          game.duration,
                          style: const TextStyle(
                            fontSize: 12,
                            color: Colors.grey,
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 12),
              // Right arrow
              const Icon(
                Icons.arrow_forward_ios,
                size: 16,
                color: Color(0xFF2196F3),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
