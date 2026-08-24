import 'dart:math';
import '../models/search_models.dart';

class AdvancedRankingEngine {
  // ────────────────────────────────────────────────────────────
  // RANKING WEIGHTS (Fine-tuned based on UX research)
  // ────────────────────────────────────────────────────────────

  // Title matching weights
  static const double exactTitleMatch = 100.0;
  static const double titleStartsWith = 85.0;
  static const double titleWholeWordMatch = 65.0;
  static const double titlePartialMatch = 40.0;

  // Category matching weights
  static const double categoryMatch = 30.0;

  // Keyword matching weights
  static const double keywordExactMatch = 50.0;
  static const double keywordPartialMatch = 20.0;
  static const double keywordCount = 5.0; // per keyword

  // Popularity & recency weights
  static const double popularityBoost = 25.0; // max boost
  static const double recencyBoost = 15.0; // max boost
  static const double typePreference = 20.0; // by result type

  // Penalty weights
  static const double fuzzyMatchPenalty = -5.0;

  /// Calculate comprehensive relevance score
  double calculateRelevanceScore({
    required String query,
    required SearchResult item,
    required int searchCount,
    required int totalSearches,
  }) {
    double score = 0;

    final queryLower = query.toLowerCase().trim();
    final titleLower = item.title.toLowerCase();

    // ─────────────────────────────────────────────────────────
    // TITLE MATCHING (40% weight)
    // ─────────────────────────────────────────────────────────

    // Exact match
    if (titleLower == queryLower) {
      score += exactTitleMatch;
    }
    // Title starts with query
    else if (titleLower.startsWith(queryLower)) {
      score += titleStartsWith;
    }
    // Whole word match in title
    else if (_isWholeWordMatch(queryLower, titleLower)) {
      score += titleWholeWordMatch;
    }
    // Partial match
    else if (titleLower.contains(queryLower)) {
      score += titlePartialMatch;
    }

    // ─────────────────────────────────────────────────────────
    // CATEGORY MATCHING (10% weight)
    // ─────────────────────────────────────────────────────────

    if (item.category.toLowerCase().contains(queryLower)) {
      score += categoryMatch;
    }

    // ─────────────────────────────────────────────────────────
    // KEYWORD MATCHING (30% weight)
    // ─────────────────────────────────────────────────────────

    final keywordScore = _calculateKeywordScore(queryLower, item.matchedKeywords);
    score += keywordScore;

    // ─────────────────────────────────────────────────────────
    // POPULARITY SCORING (10% weight)
    // ─────────────────────────────────────────────────────────

    if (totalSearches > 0) {
      final popularityRatio = searchCount / totalSearches;
      score += (popularityRatio * popularityBoost).clamp(0, popularityBoost);
    }

    // ─────────────────────────────────────────────────────────
    // RECENCY SCORING (5% weight)
    // ─────────────────────────────────────────────────────────

    if (item.lastAccessed != null) {
      final daysAgo = DateTime.now().difference(item.lastAccessed!).inDays;
      if (daysAgo <= 30) {
        // Decay function: recent items get higher boost
        final recencyScore = recencyBoost * (1 - (daysAgo / 30));
        score += recencyScore.clamp(0, recencyBoost);
      }
    }

    // ─────────────────────────────────────────────────────────
    // TYPE PREFERENCE (5% weight)
    // ─────────────────────────────────────────────────────────

    final typeBoost = _getTypePreferenceBoost(item.type);
    score += typeBoost;

    // Ensure score is never negative
    return score.clamp(0, 999);
  }

  /// Calculate keyword matching score
  double _calculateKeywordScore(String query, List<String> keywords) {
    if (keywords.isEmpty) return 0;

    double score = 0;
    var matchedKeywords = 0;

    for (final keyword in keywords) {
      final keywordLower = keyword.toLowerCase();

      if (keywordLower == query) {
        score += keywordExactMatch;
        matchedKeywords++;
      } else if (keywordLower.contains(query) || query.contains(keywordLower)) {
        score += keywordPartialMatch;
        matchedKeywords++;
      }
    }

    // Bonus for multiple keyword matches
    if (matchedKeywords > 1) {
      score += (matchedKeywords - 1) * keywordCount;
    }

    return score;
  }

  /// Check if query is a whole word match
  bool _isWholeWordMatch(String query, String text) {
    final pattern = RegExp(r'\b' + RegExp.escape(query) + r'\b');
    return pattern.hasMatch(text);
  }

  /// Get type-based preference boost
  double _getTypePreferenceBoost(String type) {
    switch (type.toLowerCase()) {
      case 'lesson':
        return typePreference * 1.2; // Lessons preferred
      case 'lab':
        return typePreference * 1.0;
      case 'game':
        return typePreference * 0.9;
      case 'quiz':
        return typePreference * 0.8;
      default:
        return 0;
    }
  }

  /// Sort results by relevance with secondary sorting
  List<SearchResult> sortByRelevance(List<SearchResult> results) {
    results.sort((a, b) {
      // Primary: relevance score
      final scoreComparison = b.relevanceScore.compareTo(a.relevanceScore);
      if (scoreComparison != 0) return scoreComparison;

      // Secondary: type (Lesson > Lab > Game > Quiz)
      final typeOrder = {'Lesson': 0, 'Lab': 1, 'Game': 2, 'Quiz': 3};
      final typeA = typeOrder[a.type] ?? 4;
      final typeB = typeOrder[b.type] ?? 4;
      final typeComparison = typeA.compareTo(typeB);
      if (typeComparison != 0) return typeComparison;

      // Tertiary: alphabetical by title
      return a.title.compareTo(b.title);
    });

    return results;
  }

  /// Apply diversity filter to avoid too many same-type results
  List<SearchResult> applyDiversityFilter(
    List<SearchResult> results, {
    int maxPerType = 3,
  }) {
    final typeCount = <String, int>{};
    final filtered = <SearchResult>[];

    for (final result in results) {
      final count = (typeCount[result.type] ?? 0);

      if (count < maxPerType) {
        filtered.add(result);
        typeCount[result.type] = count + 1;
      }
    }

    return filtered;
  }

  /// Get result statistics
  Map<String, dynamic> getResultStats(List<SearchResult> results) {
    final stats = <String, dynamic>{
      'total': results.length,
      'byType': <String, int>{},
      'averageScore': 0.0,
      'maxScore': 0.0,
      'minScore': 0.0,
    };

    if (results.isEmpty) return stats;

    // Count by type
    for (final result in results) {
      stats['byType'][result.type] = (stats['byType'][result.type] ?? 0) + 1;
    }

    // Calculate score statistics
    final scores = results.map((r) => r.relevanceScore).toList();
    scores.sort();

    stats['maxScore'] = scores.last;
    stats['minScore'] = scores.first;
    stats['averageScore'] = scores.reduce((a, b) => a + b) / scores.length;

    return stats;
  }
}

/// Enhanced search result with ranking metadata
class RankedSearchResult extends SearchResult {
  final double rawScore;
  final double normalizedScore; // 0-100
  final List<String> scoringBreakdown;

  RankedSearchResult({
    required String id,
    required String title,
    required String type,
    required String category,
    required String description,
    required double relevanceScore,
    required List<String> matchedKeywords,
    required String source,
    DateTime? lastAccessed,
    required this.rawScore,
    required this.normalizedScore,
    required this.scoringBreakdown,
  }) : super(
    id: id,
    title: title,
    type: type,
    category: category,
    description: description,
    relevanceScore: relevanceScore,
    matchedKeywords: matchedKeywords,
    source: source,
    lastAccessed: lastAccessed,
  );

  /// Create from SearchResult
  factory RankedSearchResult.fromSearchResult(
    SearchResult result, {
    required double rawScore,
    required double normalizedScore,
    List<String>? scoringBreakdown,
  }) {
    return RankedSearchResult(
      id: result.id,
      title: result.title,
      type: result.type,
      category: result.category,
      description: result.description,
      relevanceScore: result.relevanceScore,
      matchedKeywords: result.matchedKeywords,
      source: result.source,
      lastAccessed: result.lastAccessed,
      rawScore: rawScore,
      normalizedScore: normalizedScore,
      scoringBreakdown: scoringBreakdown ?? [],
    );
  }
}
