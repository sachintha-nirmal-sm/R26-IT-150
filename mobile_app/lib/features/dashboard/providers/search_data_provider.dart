import 'package:cloud_firestore/cloud_firestore.dart';
import '../models/search_models.dart';
import '../services/search_service.dart';
import '../services/pdf_keyword_extractor.dart';

class SearchDataProvider {
  static const String _lessonsCollection = 'lessons';
  static const String _gamesCollection = 'games';
  static const String _labsCollection = 'labs';
  static const String _quizzesCollection = 'quizzes';

  final FirebaseFirestore _firestore = FirebaseFirestore.instance;
  final SearchService _searchService = SearchService();

  /// Fetch all searchable items for a grade
  Future<List<SearchResult>> getAllSearchableItems(String grade) async {
    final results = <SearchResult>[];

    try {
      // Fetch lessons
      results.addAll(await _fetchLessons(grade));

      // Fetch games
      results.addAll(await _fetchGames(grade));

      // Fetch labs
      results.addAll(await _fetchLabs(grade));

      // Fetch quizzes
      results.addAll(await _fetchQuizzes(grade));
    } catch (e) {
      print('Error fetching searchable items: $e');
    }

    return results;
  }

  /// Fetch lessons for a grade
  Future<List<SearchResult>> _fetchLessons(String grade) async {
    final results = <SearchResult>[];

    try {
      final snapshot = await _firestore
          .collection(_lessonsCollection)
          .where('grade', isEqualTo: grade)
          .get();

      for (final doc in snapshot.docs) {
        final data = doc.data();

        // Get PDF keywords if available
        final keywordIndex = await PDFKeywordExtractor.getIndexFromFirestore(doc.id);
        final keywords = keywordIndex?.keywords ?? [];

        results.add(
          SearchResult(
            id: doc.id,
            title: data['title'] ?? 'Untitled',
            type: 'Lesson',
            category: data['topic'] ?? 'General',
            description: data['description'] ?? '',
            relevanceScore: 0,
            matchedKeywords: keywords,
            source: data['source'] ?? 'PDF',
            lastAccessed: data['lastAccessed'] != null
                ? (data['lastAccessed'] as Timestamp).toDate()
                : null,
          ),
        );
      }
    } catch (e) {
      print('Error fetching lessons: $e');
    }

    return results;
  }

  /// Fetch games for a grade
  Future<List<SearchResult>> _fetchGames(String grade) async {
    final results = <SearchResult>[];

    try {
      final snapshot = await _firestore
          .collection(_gamesCollection)
          .where('grade', isEqualTo: grade)
          .get();

      for (final doc in snapshot.docs) {
        final data = doc.data();

        results.add(
          SearchResult(
            id: doc.id,
            title: data['title'] ?? 'Untitled',
            type: 'Game',
            category: data['topic'] ?? 'General',
            description: data['description'] ?? '',
            relevanceScore: 0,
            matchedKeywords: [
              data['topic'] ?? '',
              data['difficulty'] ?? '',
            ],
            source: 'Game',
            lastAccessed: data['lastPlayed'] != null
                ? (data['lastPlayed'] as Timestamp).toDate()
                : null,
          ),
        );
      }
    } catch (e) {
      print('Error fetching games: $e');
    }

    return results;
  }

  /// Fetch labs for a grade
  Future<List<SearchResult>> _fetchLabs(String grade) async {
    final results = <SearchResult>[];

    try {
      final snapshot = await _firestore
          .collection(_labsCollection)
          .where('grade', isEqualTo: grade)
          .get();

      for (final doc in snapshot.docs) {
        final data = doc.data();

        results.add(
          SearchResult(
            id: doc.id,
            title: data['title'] ?? 'Untitled',
            type: 'Lab',
            category: data['topic'] ?? 'General',
            description: data['description'] ?? '',
            relevanceScore: 0,
            matchedKeywords: [
              data['topic'] ?? '',
              data['type'] ?? '',
            ],
            source: 'Lab',
            lastAccessed: data['lastAccessed'] != null
                ? (data['lastAccessed'] as Timestamp).toDate()
                : null,
          ),
        );
      }
    } catch (e) {
      print('Error fetching labs: $e');
    }

    return results;
  }

  /// Fetch quizzes for a grade
  Future<List<SearchResult>> _fetchQuizzes(String grade) async {
    final results = <SearchResult>[];

    try {
      final snapshot = await _firestore
          .collection(_quizzesCollection)
          .where('grade', isEqualTo: grade)
          .get();

      for (final doc in snapshot.docs) {
        final data = doc.data();

        results.add(
          SearchResult(
            id: doc.id,
            title: data['title'] ?? 'Untitled',
            type: 'Quiz',
            category: data['topic'] ?? 'General',
            description: data['description'] ?? '',
            relevanceScore: 0,
            matchedKeywords: [
              data['topic'] ?? '',
              data['difficulty'] ?? '',
            ],
            source: 'Quiz',
            lastAccessed: data['lastAttempted'] != null
                ? (data['lastAttempted'] as Timestamp).toDate()
                : null,
          ),
        );
      }
    } catch (e) {
      print('Error fetching quizzes: $e');
    }

    return results;
  }

  /// Perform smart search across all items
  Future<List<SearchResult>> smartSearch({
    required String query,
    required String grade,
    String category = 'All',
  }) async {
    try {
      // Get all items
      var items = await getAllSearchableItems(grade);

      // Filter by category if needed
      if (category != 'All') {
        items = items
            .where((item) => item.type.toLowerCase() == category.toLowerCase())
            .toList();
      }

      // Get search analytics
      final analyticsMap = await _getSearchAnalytics(grade);

      // Perform smart search
      final results = await _searchService.smartSearch(
        query: query,
        grade: grade,
        allItems: items,
        searchAnalytics: analyticsMap,
      );

      return results;
    } catch (e) {
      print('Error performing smart search: $e');
      return [];
    }
  }

  /// Get search analytics data
  Future<Map<String, int>> _getSearchAnalytics(String grade) async {
    try {
      final snapshot = await _firestore
          .collection('search_analytics')
          .where('grade', isEqualTo: grade)
          .get();

      final analytics = <String, int>{};
      for (final doc in snapshot.docs) {
        analytics[doc['query'] as String] = (doc['count'] as num).toInt();
      }

      return analytics;
    } catch (e) {
      print('Error getting search analytics: $e');
      return {};
    }
  }

  /// Index a lesson's PDF keywords
  Future<void> indexLessonPDF({
    required String lessonId,
    required String lessonTitle,
    required String pdfContent,
    required String category,
    required String grade,
  }) async {
    try {
      final index = await PDFKeywordExtractor.createIndexFromPDF(
        lessonId: lessonId,
        lessonTitle: lessonTitle,
        pdfContent: pdfContent,
        category: category,
        grade: grade,
      );

      await PDFKeywordExtractor.saveIndexToFirestore(index);
    } catch (e) {
      print('Error indexing lesson PDF: $e');
    }
  }

  /// Get trending searches
  Future<List<SearchSuggestion>> getTrendingSearches(String grade) async {
    return await _searchService.getTrendingSearches(grade);
  }

  /// Get "Did You Mean" suggestions
  Future<List<String>> getDidYouMeanSuggestions(
    String query,
    String grade,
  ) async {
    try {
      final items = await getAllSearchableItems(grade);
      final titles = items.map((item) => item.title).toList();

      return _searchService.getDidYouMeanSuggestions(query, titles);
    } catch (e) {
      print('Error getting did you mean suggestions: $e');
      return [];
    }
  }
}
