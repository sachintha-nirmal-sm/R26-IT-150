import '../models/search_models.dart';

/// Pure, deterministic ranking utilities used by dashboard search.
///
/// Keeping this class independent from Firebase makes ranking fast and easy to
/// test. Data retrieval and analytics persistence remain in [SearchService].
class AdvancedRankingEngine {
  double calculateRelevanceScore({
    required String query,
    required SearchResult item,
    required int searchCount,
    required int totalSearches,
  }) {
    final normalizedQuery = _normalize(query);
    if (normalizedQuery.isEmpty) return 0;

    final title = _normalize(item.title);
    final description = _normalize(item.description);
    final category = _normalize(item.category);
    final queryWords = normalizedQuery.split(' ');

    double score = 0;

    // Title relevance is intentionally the strongest signal.
    if (title == normalizedQuery) {
      score += 100;
    } else if (title.startsWith(normalizedQuery)) {
      score += 70;
    } else if (_containsWholePhrase(title, normalizedQuery)) {
      score += 55;
    } else if (queryWords.every(title.contains)) {
      score += 45;
    } else if (title.contains(normalizedQuery)) {
      score += 35;
    }

    var keywordMatches = 0;
    for (final keyword in item.matchedKeywords) {
      final normalizedKeyword = _normalize(keyword);
      if (normalizedKeyword == normalizedQuery) {
        score += 20;
        keywordMatches++;
      } else if (normalizedKeyword.contains(normalizedQuery) ||
          normalizedQuery.contains(normalizedKeyword)) {
        score += 8;
        keywordMatches++;
      }
    }
    if (keywordMatches > 1) score += (keywordMatches - 1) * 3;

    if (description.contains(normalizedQuery)) score += 8;
    if (category.contains(normalizedQuery)) score += 10;

    if (totalSearches > 0 && searchCount > 0) {
      score += (searchCount / totalSearches).clamp(0, 1) * 10;
    }

    final lastAccessed = item.lastAccessed;
    if (lastAccessed != null) {
      final age = DateTime.now().difference(lastAccessed).inDays.clamp(0, 30);
      score += 5 * (1 - age / 30);
    }

    score += switch (item.type.toLowerCase()) {
      'lesson' => 5,
      'lab' || 'practical' => 4,
      'game' => 3,
      'quiz' => 2,
      _ => 0,
    };

    return score;
  }

  List<SearchResult> sortByRelevance(List<SearchResult> results) {
    final sorted = List<SearchResult>.of(results);
    sorted.sort((a, b) => b.relevanceScore.compareTo(a.relevanceScore));
    return sorted;
  }

  List<SearchResult> applyDiversityFilter(
    List<SearchResult> results, {
    int maxPerType = 5,
  }) {
    if (maxPerType < 1) return const [];

    final counts = <String, int>{};
    final filtered = <SearchResult>[];
    for (final result in results) {
      final type = result.type.toLowerCase();
      final count = counts[type] ?? 0;
      if (count < maxPerType) {
        filtered.add(result);
        counts[type] = count + 1;
      }
    }
    return filtered;
  }

  Map<String, num> getResultStats(List<SearchResult> results) {
    if (results.isEmpty) {
      return const {
        'total': 0,
        'maxScore': 0,
        'minScore': 0,
        'averageScore': 0
      };
    }

    final scores = results.map((result) => result.relevanceScore);
    final totalScore = scores.reduce((a, b) => a + b);
    return {
      'total': results.length,
      'maxScore': scores.reduce((a, b) => a > b ? a : b),
      'minScore': scores.reduce((a, b) => a < b ? a : b),
      'averageScore': totalScore / results.length,
    };
  }

  List<String> getDidYouMeanSuggestions(
    String query,
    List<String> availableOptions, {
    int maxSuggestions = 3,
  }) {
    final normalizedQuery = _normalize(query);
    if (normalizedQuery.isEmpty) return const [];

    final candidates = availableOptions
        .map((option) =>
            MapEntry(option, _levenshtein(normalizedQuery, _normalize(option))))
        .where(
            (entry) => entry.value <= _allowedDistance(normalizedQuery.length))
        .toList()
      ..sort((a, b) {
        final distance = a.value.compareTo(b.value);
        return distance != 0 ? distance : a.key.compareTo(b.key);
      });

    return candidates.take(maxSuggestions).map((entry) => entry.key).toList();
  }

  static String _normalize(String value) => value
      .toLowerCase()
      .replaceAll(RegExp(r"[^a-z0-9\s]"), ' ')
      .replaceAll(RegExp(r'\s+'), ' ')
      .trim();

  static bool _containsWholePhrase(String text, String phrase) =>
      RegExp('(?:^|\\s)${RegExp.escape(phrase)}(?:\\s|\$)').hasMatch(text);

  static int _allowedDistance(int length) =>
      length <= 4 ? 1 : (length <= 9 ? 2 : 3);

  static int _levenshtein(String left, String right) {
    if (left.isEmpty) return right.length;
    if (right.isEmpty) return left.length;

    var previous = List<int>.generate(right.length + 1, (index) => index);
    for (var i = 1; i <= left.length; i++) {
      final current = List<int>.filled(right.length + 1, 0)..[0] = i;
      for (var j = 1; j <= right.length; j++) {
        final substitutionCost = left[i - 1] == right[j - 1] ? 0 : 1;
        current[j] = [
          current[j - 1] + 1,
          previous[j] + 1,
          previous[j - 1] + substitutionCost,
        ].reduce((a, b) => a < b ? a : b);
      }
      previous = current;
    }
    return previous.last;
  }
}
