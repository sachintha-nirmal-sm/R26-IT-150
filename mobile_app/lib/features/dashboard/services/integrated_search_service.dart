import 'package:cloud_firestore/cloud_firestore.dart';
import 'dart:async';
import '../models/search_models.dart';
import 'search_service.dart';
import 'firestore_search_adapter.dart';
import 'advanced_ranking_engine.dart';
import 'pdf_keyword_extractor.dart';

class IntegratedSearchService {
  final SearchService _searchService = SearchService();
  final FirestoreSearchAdapter _firestoreAdapter = FirestoreSearchAdapter();
  final AdvancedRankingEngine _rankingEngine = AdvancedRankingEngine();

  // Cache for search results (grade-based)
  final Map<String, List<SearchResult>> _searchCache = {};
  final Map<String, DateTime> _cacheTimestamp = {};
  static const Duration _cacheDuration = Duration(hours: 1);

  // Search history
  final Map<String, List<String>> _searchHistory = {};
  static const int _maxHistoryItems = 10;

  // ────────────────────────────────────────────────────────────
  // INTEGRATED SEARCH
  // ────────────────────────────────────────────────────────────

  /// Perform comprehensive search with all features
  Future<List<SearchResult>> search({
    required String query,
    required String grade,
    String category = 'All',
    bool useCache = true,
  }) async {
    if (query.trim().isEmpty) return [];

    // Save to search history
    _addToSearchHistory(grade, query);

    // Fetch items (with caching)
    final items = await _getSearchItems(grade, useCache);

    // Filter by category if needed
    var filteredItems = items;
    if (category != 'All') {
      filteredItems = items
          .where((item) => item.type.toLowerCase() == category.toLowerCase())
          .toList();
    }

    // Get analytics for ranking
    final analytics = await _getSearchAnalytics(grade);
    final totalSearches = analytics.values.fold<int>(0, (a, b) => a + b);

    // Apply ranking algorithm
    final rankedResults = _rankingEngine.sortByRelevance(
      filteredItems.map((item) {
        final score = _rankingEngine.calculateRelevanceScore(
          query: query,
          item: item,
          searchCount: analytics[item.title] ?? 0,
          totalSearches: totalSearches,
        );

        return SearchResult(
          id: item.id,
          title: item.title,
          type: item.type,
          category: item.category,
          description: item.description,
          relevanceScore: score,
          matchedKeywords: item.matchedKeywords,
          source: item.source,
          lastAccessed: item.lastAccessed,
        );
      }).toList(),
    );

    // Filter out zero-score results
    final finalResults =
        rankedResults.where((r) => r.relevanceScore > 0).toList();

    // Apply diversity filter
    final diverseResults =
        _rankingEngine.applyDiversityFilter(finalResults, maxPerType: 5);

    // Save search analytics (optional - don't block if fails)
    try {
      await _searchService.saveRecentSearch(
        query: query,
        category: category,
      );
    } catch (e) {
      print('Note: Could not save search to history');
    }

    return diverseResults;
  }

  /// Get autocomplete suggestions
  Future<List<SearchSuggestion>> getAutocompleteSuggestions({
    required String query,
    required String grade,
  }) async {
    if (query.isEmpty) return [];

    final suggestions = <SearchSuggestion>[];

    // Get from recent searches (with error handling)
    List<RecentSearch> recentSearches = [];
    try {
      recentSearches = await _searchService.getRecentSearches();
    } catch (e) {
      print('Note: Recent searches unavailable');
    }
    for (final recent in recentSearches.take(5)) {
      if (recent.query.toLowerCase().startsWith(query.toLowerCase())) {
        suggestions.add(
          SearchSuggestion(
            text: recent.query,
            type: 'history',
            priority: 90,
            icon: '🕐',
          ),
        );
      }
    }

    // Get from popular searches (with error handling)
    List<SearchSuggestion> trending = [];
    try {
      trending = await _searchService.getTrendingSearches(grade);
    } catch (e) {
      print('Note: Trending searches unavailable');
    }
    for (final trend in trending.take(3)) {
      if (trend.text.toLowerCase().startsWith(query.toLowerCase())) {
        suggestions.add(
          SearchSuggestion(
            text: trend.text,
            type: 'popular',
            priority: 80,
            icon: '📈',
          ),
        );
      }
    }

    // Get from available items
    final items = await _getSearchItems(grade, true);
    for (final item in items) {
      if (item.title.toLowerCase().startsWith(query.toLowerCase())) {
        suggestions.add(
          SearchSuggestion(
            text: item.title,
            type: 'autocomplete',
            priority: 70,
            icon: '🔍',
          ),
        );
      }

      if (suggestions.length >= 10) break;
    }

    // Remove duplicates
    final uniqueSuggestions = <SearchSuggestion>[];
    final seen = <String>{};
    for (final s in suggestions) {
      if (!seen.contains(s.text)) {
        uniqueSuggestions.add(s);
        seen.add(s.text);
      }
    }

    // Sort by priority
    uniqueSuggestions.sort((a, b) => b.priority.compareTo(a.priority));

    return uniqueSuggestions.take(10).toList();
  }

  /// Get "Did you mean" suggestions for typos
  Future<List<String>> getDidYouMeanSuggestions({
    required String query,
    required String grade,
  }) async {
    final items = await _getSearchItems(grade, true);
    final titles = items.map((item) => item.title).toList();

    return _searchService.getDidYouMeanSuggestions(query, titles);
  }

  /// Refresh search cache
  Future<void> refreshCache(String grade) async {
    _searchCache.remove(grade);
    _cacheTimestamp.remove(grade);
  }

  /// Get search history for grade
  List<String> getSearchHistory(String grade) {
    return _searchHistory[grade] ?? [];
  }

  /// Clear search history for grade
  void clearSearchHistory(String grade) {
    _searchHistory[grade] = [];
  }

  // ────────────────────────────────────────────────────────────
  // PRIVATE HELPERS
  // ────────────────────────────────────────────────────────────

  /// Get search items with caching
  Future<List<SearchResult>> _getSearchItems(String grade, bool useCache) async {
    // Check cache
    if (useCache) {
      final cached = _searchCache[grade];
      final timestamp = _cacheTimestamp[grade];

      if (cached != null &&
          timestamp != null &&
          DateTime.now().difference(timestamp) < _cacheDuration) {
        return cached;
      }
    }

    // Fetch from Firestore
    final items = await _firestoreAdapter.fetchAllSearchableItems(grade);

    // Cache results
    _searchCache[grade] = items;
    _cacheTimestamp[grade] = DateTime.now();

    return items;
  }

  /// Get search analytics
  Future<Map<String, int>> _getSearchAnalytics(String grade) async {
    try {
      final snapshot = await FirebaseFirestore.instance
          .collection('search_analytics')
          .where('grade', isEqualTo: grade)
          .get();

      final analytics = <String, int>{};
      for (final doc in snapshot.docs) {
        analytics[doc['query'] as String] = (doc['count'] as num).toInt();
      }

      return analytics;
    } catch (e) {
      print('Note: Search analytics unavailable (needs Firestore index). Basic search will still work.');
      return {};
    }
  }

  /// Add to search history
  void _addToSearchHistory(String grade, String query) {
    final history = _searchHistory.putIfAbsent(grade, () => []);

    // Remove if already exists (to put at top)
    history.remove(query);

    // Add to top
    history.insert(0, query);

    // Keep only recent items
    if (history.length > _maxHistoryItems) {
      history.removeLast();
    }
  }

  /// Get result statistics for debugging/analytics
  Map<String, dynamic> getResultStats(List<SearchResult> results) {
    return _rankingEngine.getResultStats(results);
  }
}
