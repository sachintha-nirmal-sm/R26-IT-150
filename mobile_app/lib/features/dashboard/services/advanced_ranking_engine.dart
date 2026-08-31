import '../models/search_models.dart';

/// Weighted ranking for dashboard search results.
class AdvancedRankingEngine {
  static const double _maxPopularityScore = 20;
  static const double _maxRecencyScore = 15;
  static const int _recencyWindowDays = 30;
  static const int _fuzzyMaxDistance = 2;

  double calculateRelevanceScore({
    required String query,
    required SearchResult item,
    required int searchCount,
    required int totalSearches,
  }) {
    final queryLower = query.toLowerCase().trim();
    if (queryLower.isEmpty) return 0;

    return _titleScore(queryLower, item.title.toLowerCase()) +
        _keywordScore(queryLower, item.matchedKeywords) +
        _popularityScore(searchCount, totalSearches) +
        _recencyScore(item.lastAccessed) +
        _typePreference(item.type) +
        _categoryScore(queryLower, item.category);
  }

  List<SearchResult> sortByRelevance(List<SearchResult> results) {
    final sorted = List<SearchResult>.from(results);
    sorted.sort((a, b) => b.relevanceScore.compareTo(a.relevanceScore));
    return sorted;
  }

  List<SearchResult> applyDiversityFilter(
    List<SearchResult> results, {
    int maxPerType = 5,
  }) {
    final filtered = <SearchResult>[];
    final counts = <String, int>{};

    for (final result in sortByRelevance(results)) {
      final count = counts[result.type] ?? 0;
      if (count >= maxPerType) continue;
      filtered.add(result);
      counts[result.type] = count + 1;
    }

    return filtered;
  }

  Map<String, num> getResultStats(List<SearchResult> results) {
    if (results.isEmpty) {
      return {
        'total': 0,
        'maxScore': 0,
        'minScore': 0,
        'averageScore': 0,
      };
    }

    final scores = results.map((r) => r.relevanceScore);
    final maxScore = scores.reduce((a, b) => a > b ? a : b);
    final minScore = scores.reduce((a, b) => a < b ? a : b);
    final averageScore = scores.reduce((a, b) => a + b) / results.length;

    return {
      'total': results.length,
      'maxScore': maxScore,
      'minScore': minScore,
      'averageScore': averageScore,
    };
  }

  List<String> getDidYouMeanSuggestions(
    String query,
    List<String> availableOptions,
  ) {
    final queryLower = query.toLowerCase().trim();
    if (queryLower.isEmpty) return [];

    final suggestions = <String>[];
    for (final option in availableOptions) {
      final distance = _levenshteinDistance(queryLower, option.toLowerCase());
      if (distance > 0 && distance <= _fuzzyMaxDistance) {
        suggestions.add(option);
      }
    }

    suggestions.sort((a, b) {
      final distA = _levenshteinDistance(queryLower, a.toLowerCase());
      final distB = _levenshteinDistance(queryLower, b.toLowerCase());
      return distA.compareTo(distB);
    });

    return suggestions.take(3).toList();
  }

  double _titleScore(String queryLower, String titleLower) {
    if (titleLower == queryLower) return 100;
    if (titleLower.startsWith(queryLower)) return 70;
    if (RegExp('\\b${RegExp.escape(queryLower)}\\b').hasMatch(titleLower)) {
      return 50;
    }
    if (titleLower.contains(queryLower)) return 30;
    return 0;
  }

  double _keywordScore(String queryLower, List<String> keywords) {
    double score = 0;
    for (final keyword in keywords) {
      final key = keyword.toLowerCase();
      if (key == queryLower) {
        score += 8;
      } else if (key.contains(queryLower) || queryLower.contains(key)) {
        score += 3;
      }
    }
    return score;
  }

  double _popularityScore(int searchCount, int totalSearches) {
    if (totalSearches <= 0 || searchCount <= 0) return 0;
    return (searchCount / totalSearches) * _maxPopularityScore;
  }

  double _recencyScore(DateTime? lastAccessed) {
    if (lastAccessed == null) return 0;
    final days = DateTime.now().difference(lastAccessed).inDays;
    if (days >= _recencyWindowDays) return 0;
    if (days <= 0) return _maxRecencyScore;
    return _maxRecencyScore * (1 - days / _recencyWindowDays);
  }

  double _typePreference(String type) {
    switch (type.toLowerCase()) {
      case 'lesson':
        return 12;
      case 'lab':
        return 10;
      case 'game':
        return 6;
      case 'quiz':
        return 4;
      default:
        return 5;
    }
  }

  double _categoryScore(String queryLower, String category) {
    final categoryLower = category.toLowerCase();
    if (categoryLower == queryLower || categoryLower.contains(queryLower)) {
      return 10;
    }
    return 0;
  }

  int _levenshteinDistance(String s1, String s2) {
    final len1 = s1.length;
    final len2 = s2.length;
    if (len1 == 0) return len2;
    if (len2 == 0) return len1;

    final d = List.generate(len1 + 1, (_) => List<int>.filled(len2 + 1, 0));
    for (var i = 0; i <= len1; i++) {
      d[i][0] = i;
    }
    for (var j = 0; j <= len2; j++) {
      d[0][j] = j;
    }

    for (var i = 1; i <= len1; i++) {
      for (var j = 1; j <= len2; j++) {
        final cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
        d[i][j] = [
          d[i - 1][j] + 1,
          d[i][j - 1] + 1,
          d[i - 1][j - 1] + cost,
        ].reduce((a, b) => a < b ? a : b);
      }
    }

    return d[len1][len2];
  }
}
