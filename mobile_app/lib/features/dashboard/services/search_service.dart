import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:firebase_auth/firebase_auth.dart';
import '../models/search_models.dart';

class SearchService {
  static const String _recentSearchesCollection = 'recent_searches';
  static const String _pdfKeywordsCollection = 'pdf_keywords';
  static const int _maxRecentSearches = 15;

  final FirebaseFirestore _firestore = FirebaseFirestore.instance;
  final FirebaseAuth _auth = FirebaseAuth.instance;

  // ────────────────────────────────────────────────────────────
  // FUZZY MATCHING (Typo Correction)
  // ────────────────────────────────────────────────────────────

  /// Levenshtein distance for fuzzy matching
  int _levenshteinDistance(String s1, String s2) {
    final len1 = s1.length;
    final len2 = s2.length;

    if (len1 == 0) return len2;
    if (len2 == 0) return len1;

    final d = List.generate(len1 + 1, (i) => List.filled(len2 + 1, 0));

    for (var i = 0; i <= len1; i++) d[i][0] = i;
    for (var j = 0; j <= len2; j++) d[0][j] = j;

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

  /// Check if query might be a typo and return suggestions
  List<String> getDidYouMeanSuggestions(
    String query,
    List<String> availableOptions,
  ) {
    const maxDistance = 2;
    final suggestions = <String>[];

    for (final option in availableOptions) {
      final distance = _levenshteinDistance(query.toLowerCase(), option.toLowerCase());
      if (distance <= maxDistance && distance > 0) {
        suggestions.add(option);
      }
    }

    suggestions.sort((a, b) {
      final distA = _levenshteinDistance(query.toLowerCase(), a.toLowerCase());
      final distB = _levenshteinDistance(query.toLowerCase(), b.toLowerCase());
      return distA.compareTo(distB);
    });

    return suggestions.take(3).toList();
  }

  // ────────────────────────────────────────────────────────────
  // SMART SEARCH RANKING
  // ────────────────────────────────────────────────────────────

  /// Calculate relevance score based on multiple factors
  double _calculateRelevanceScore({
    required String query,
    required String title,
    required List<String> keywords,
    required int searchCount,
    required bool isRecent,
  }) {
    double score = 0;

    final queryLower = query.toLowerCase();
    final titleLower = title.toLowerCase();

    // Exact match (highest priority)
    if (titleLower == queryLower) {
      score += 100;
    }
    // Title starts with query
    else if (titleLower.startsWith(queryLower)) {
      score += 80;
    }
    // Title contains query as whole word
    else if (titleLower.contains(RegExp(r'\b' + RegExp.escape(queryLower) + r'\b'))) {
      score += 60;
    }
    // Title contains query substring
    else if (titleLower.contains(queryLower)) {
      score += 40;
    }

    // Keywords match
    for (final keyword in keywords) {
      if (keyword.toLowerCase().contains(queryLower)) {
        score += 25;
      }
    }

    // Popularity boost (based on search count)
    score += (searchCount * 2).toDouble().clamp(0, 20);

    // Recency boost
    if (isRecent) {
      score += 10;
    }

    return score;
  }

  // ────────────────────────────────────────────────────────────
  // SMART SEARCH
  // ────────────────────────────────────────────────────────────

  /// Perform smart search with all features
  Future<List<SearchResult>> smartSearch({
    required String query,
    required String grade,
    required List<SearchResult> allItems,
    required Map<String, int> searchAnalytics,
  }) async {
    if (query.trim().isEmpty) return [];

    final results = <SearchResult>[];

    // Score each item
    final scoredItems = allItems.map((item) {
      final relevance = _calculateRelevanceScore(
        query: query,
        title: item.title,
        keywords: item.matchedKeywords,
        searchCount: searchAnalytics[item.title] ?? 0,
        isRecent: item.lastAccessed != null &&
            DateTime.now().difference(item.lastAccessed!).inDays <= 7,
      );

      return MapEntry(
        item,
        relevance,
      );
    }).toList();

    // Sort by relevance score
    scoredItems.sort((a, b) => b.value.compareTo(a.value));

    // Filter items with score > 0 and add
    for (final entry in scoredItems) {
      if (entry.value > 0) {
        results.add(entry.key);
      }
    }

    return results;
  }

  // ────────────────────────────────────────────────────────────
  // AUTO-COMPLETE & SUGGESTIONS
  // ────────────────────────────────────────────────────────────

  /// Get autocomplete suggestions
  Future<List<SearchSuggestion>> getAutocompleteSuggestions(
    String query,
    String grade,
  ) async {
    if (query.isEmpty) return [];

    final suggestions = <SearchSuggestion>[];
    final queryLower = query.toLowerCase();

    try {
      // Get popular searches from analytics
      final analyticsQuery = await _firestore
          .collection('search_analytics')
          .where('grade', isEqualTo: grade)
          .limit(20)
          .get();
      analyticsQuery.docs.sort((a, b) =>
          ((b['count'] ?? 0) as num).compareTo((a['count'] ?? 0) as num));

      for (final doc in analyticsQuery.docs) {
        final searchTerm = doc['query'] as String;
        if (searchTerm.toLowerCase().startsWith(queryLower)) {
          suggestions.add(
            SearchSuggestion(
              text: searchTerm,
              type: 'autocomplete',
              priority: 80 - suggestions.length,
              icon: '🔍',
            ),
          );
        }
      }

      // Get from all items (lessons, games, etc)
      // This would need to be populated from your search data
      return suggestions;
    } catch (e) {
      print('Error getting autocomplete suggestions: $e');
      return [];
    }
  }

  // ────────────────────────────────────────────────────────────
  // RECENT SEARCHES
  // ────────────────────────────────────────────────────────────

  /// Save search to recent searches
  Future<void> saveRecentSearch({
    required String query,
    required String category,
  }) async {
    try {
      final user = _auth.currentUser;
      if (user == null) return;

      final recentSearch = RecentSearch(
        query: query,
        timestamp: DateTime.now(),
        category: category,
        userId: user.uid,
      );

      await _firestore
          .collection(_recentSearchesCollection)
          .add(recentSearch.toMap());

      // Update search analytics
      await _updateSearchAnalytics(query, category);
    } catch (e) {
      print('Error saving recent search: $e');
    }
  }

  /// Get recent searches for current user
  Future<List<RecentSearch>> getRecentSearches() async {
    try {
      final user = _auth.currentUser;
      if (user == null) return [];

      final snapshot = await _firestore
          .collection(_recentSearchesCollection)
          .where('userId', isEqualTo: user.uid)
          .limit(_maxRecentSearches)
          .get();

      final results = snapshot.docs
          .map((doc) => RecentSearch.fromMap(doc.data()))
          .toList();
      results.sort((a, b) => b.timestamp.compareTo(a.timestamp));
      return results;
    } catch (e) {
      print('Error getting recent searches: $e');
      return [];
    }
  }

  /// Delete a recent search
  Future<void> deleteRecentSearch(String query) async {
    try {
      final user = _auth.currentUser;
      if (user == null) return;

      final snapshot = await _firestore
          .collection(_recentSearchesCollection)
          .where('userId', isEqualTo: user.uid)
          .where('query', isEqualTo: query)
          .get();

      for (final doc in snapshot.docs) {
        await doc.reference.delete();
      }
    } catch (e) {
      print('Error deleting recent search: $e');
    }
  }

  /// Clear all recent searches
  Future<void> clearAllRecentSearches() async {
    try {
      final user = _auth.currentUser;
      if (user == null) return;

      final snapshot = await _firestore
          .collection(_recentSearchesCollection)
          .where('userId', isEqualTo: user.uid)
          .get();

      for (final doc in snapshot.docs) {
        await doc.reference.delete();
      }
    } catch (e) {
      print('Error clearing recent searches: $e');
    }
  }

  // ────────────────────────────────────────────────────────────
  // SEARCH ANALYTICS
  // ────────────────────────────────────────────────────────────

  /// Update search analytics
  Future<void> _updateSearchAnalytics(String query, String category) async {
    try {
      final analyticsRef = _firestore.collection('search_analytics');

      final snapshot = await analyticsRef
          .where('query', isEqualTo: query)
          .where('category', isEqualTo: category)
          .get();

      if (snapshot.docs.isEmpty) {
        await analyticsRef.add({
          'query': query,
          'category': category,
          'count': 1,
          'lastSearched': DateTime.now(),
        });
      } else {
        await snapshot.docs.first.reference.update({
          'count': FieldValue.increment(1),
          'lastSearched': DateTime.now(),
        });
      }
    } catch (e) {
      print('Error updating search analytics: $e');
    }
  }

  /// Get trending searches for a grade
  Future<List<SearchSuggestion>> getTrendingSearches(String grade) async {
    try {
      final snapshot = await _firestore
          .collection('search_analytics')
          .where('grade', isEqualTo: grade)
          .limit(10)
          .get();
      snapshot.docs.sort((a, b) =>
          ((b['count'] ?? 0) as num).compareTo((a['count'] ?? 0) as num));

      return snapshot.docs
          .map((doc) {
            return SearchSuggestion(
              text: doc['query'] as String,
              type: 'popular',
              priority: 100 - snapshot.docs.indexOf(doc),
              icon: '📈',
            );
          })
          .toList();
    } catch (e) {
      print('Error getting trending searches: $e');
      return [];
    }
  }
}
