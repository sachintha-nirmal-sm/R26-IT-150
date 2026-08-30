import 'package:flutter_test/flutter_test.dart';
import 'package:mobile_app/features/dashboard/models/search_models.dart';
import 'package:mobile_app/features/dashboard/services/advanced_ranking_engine.dart';
import 'package:mobile_app/features/dashboard/services/pdf_keyword_extractor.dart';

void main() {
  group('Advanced Ranking Engine', () {
    late AdvancedRankingEngine rankingEngine;

    setUp(() {
      rankingEngine = AdvancedRankingEngine();
    });

    // ─────────────────────────────────────────────────────────
    // RELEVANCE SCORING TESTS
    // ─────────────────────────────────────────────────────────

    test('Exact title match should have highest score', () {
      final result = SearchResult(
        id: '1',
        title: 'Newton\'s Laws of Motion',
        type: 'Lesson',
        category: 'Mechanics',
        description: 'Learn about Newton\'s laws',
        relevanceScore: 0,
        matchedKeywords: ['Newton', 'Motion', 'Force'],
        source: 'Firestore',
      );

      final score = rankingEngine.calculateRelevanceScore(
        query: 'Newton\'s Laws of Motion',
        item: result,
        searchCount: 0,
        totalSearches: 0,
      );

      expect(score, greaterThan(90)); // Exact match should be high
    });

    test('Partial match should score lower than exact match', () {
      final result = SearchResult(
        id: '1',
        title: 'Newton\'s Laws of Motion',
        type: 'Lesson',
        category: 'Mechanics',
        description: 'Learn about Newton\'s laws',
        relevanceScore: 0,
        matchedKeywords: ['Newton', 'Motion', 'Force'],
        source: 'Firestore',
      );

      final exactScore = rankingEngine.calculateRelevanceScore(
        query: 'Newton\'s Laws of Motion',
        item: result,
        searchCount: 0,
        totalSearches: 0,
      );

      final partialScore = rankingEngine.calculateRelevanceScore(
        query: 'Newton',
        item: result,
        searchCount: 0,
        totalSearches: 0,
      );

      expect(exactScore, greaterThan(partialScore));
    });

    test('Popular items should score higher', () {
      final result = SearchResult(
        id: '1',
        title: 'Friction',
        type: 'Lesson',
        category: 'Mechanics',
        description: 'Learn about friction',
        relevanceScore: 0,
        matchedKeywords: ['Friction', 'Force'],
        source: 'Firestore',
      );

      final lowPopularityScore = rankingEngine.calculateRelevanceScore(
        query: 'Friction',
        item: result,
        searchCount: 1,
        totalSearches: 100,
      );

      final highPopularityScore = rankingEngine.calculateRelevanceScore(
        query: 'Friction',
        item: result,
        searchCount: 50,
        totalSearches: 100,
      );

      expect(highPopularityScore, greaterThan(lowPopularityScore));
    });

    test('Recently accessed items should score higher', () {
      final recentResult = SearchResult(
        id: '1',
        title: 'Friction',
        type: 'Lesson',
        category: 'Mechanics',
        description: 'Learn about friction',
        relevanceScore: 0,
        matchedKeywords: ['Friction'],
        source: 'Firestore',
        lastAccessed: DateTime.now().subtract(const Duration(days: 1)),
      );

      final oldResult = SearchResult(
        id: '2',
        title: 'Friction',
        type: 'Lesson',
        category: 'Mechanics',
        description: 'Learn about friction',
        relevanceScore: 0,
        matchedKeywords: ['Friction'],
        source: 'Firestore',
        lastAccessed: DateTime.now().subtract(const Duration(days: 60)),
      );

      final recentScore = rankingEngine.calculateRelevanceScore(
        query: 'Friction',
        item: recentResult,
        searchCount: 0,
        totalSearches: 0,
      );

      final oldScore = rankingEngine.calculateRelevanceScore(
        query: 'Friction',
        item: oldResult,
        searchCount: 0,
        totalSearches: 0,
      );

      expect(recentScore, greaterThan(oldScore));
    });

    test('Lessons should score higher than games for same query', () {
      final lessonResult = SearchResult(
        id: '1',
        title: 'Force',
        type: 'Lesson',
        category: 'Mechanics',
        description: 'Learn about force',
        relevanceScore: 0,
        matchedKeywords: ['Force'],
        source: 'Firestore',
      );

      final gameResult = SearchResult(
        id: '2',
        title: 'Force',
        type: 'Game',
        category: 'Mechanics',
        description: 'Play a game about force',
        relevanceScore: 0,
        matchedKeywords: ['Force'],
        source: 'Firestore',
      );

      final lessonScore = rankingEngine.calculateRelevanceScore(
        query: 'Force',
        item: lessonResult,
        searchCount: 0,
        totalSearches: 0,
      );

      final gameScore = rankingEngine.calculateRelevanceScore(
        query: 'Force',
        item: gameResult,
        searchCount: 0,
        totalSearches: 0,
      );

      expect(lessonScore, greaterThan(gameScore));
    });

    // ─────────────────────────────────────────────────────────
    // SORTING TESTS
    // ─────────────────────────────────────────────────────────

    test('Results should be sorted by relevance score', () {
      final results = [
        SearchResult(
          id: '1',
          title: 'Friction',
          type: 'Lesson',
          category: 'Mechanics',
          description: 'Low relevance',
          relevanceScore: 30,
          matchedKeywords: [],
          source: 'Firestore',
        ),
        SearchResult(
          id: '2',
          title: 'Friction',
          type: 'Lesson',
          category: 'Mechanics',
          description: 'High relevance',
          relevanceScore: 80,
          matchedKeywords: [],
          source: 'Firestore',
        ),
        SearchResult(
          id: '3',
          title: 'Friction',
          type: 'Lesson',
          category: 'Mechanics',
          description: 'Medium relevance',
          relevanceScore: 50,
          matchedKeywords: [],
          source: 'Firestore',
        ),
      ];

      final sorted = rankingEngine.sortByRelevance(results);

      expect(sorted[0].relevanceScore, 80);
      expect(sorted[1].relevanceScore, 50);
      expect(sorted[2].relevanceScore, 30);
    });

    // ─────────────────────────────────────────────────────────
    // DIVERSITY FILTER TESTS
    // ─────────────────────────────────────────────────────────

    test('Diversity filter should limit results per type', () {
      final results = [
        SearchResult(
          id: '1',
          title: 'Friction Lesson',
          type: 'Lesson',
          category: 'Mechanics',
          description: 'desc',
          relevanceScore: 80,
          matchedKeywords: [],
          source: 'Firestore',
        ),
        SearchResult(
          id: '2',
          title: 'Friction Lesson 2',
          type: 'Lesson',
          category: 'Mechanics',
          description: 'desc',
          relevanceScore: 75,
          matchedKeywords: [],
          source: 'Firestore',
        ),
        SearchResult(
          id: '3',
          title: 'Friction Lesson 3',
          type: 'Lesson',
          category: 'Mechanics',
          description: 'desc',
          relevanceScore: 70,
          matchedKeywords: [],
          source: 'Firestore',
        ),
        SearchResult(
          id: '4',
          title: 'Friction Lab',
          type: 'Lab',
          category: 'Mechanics',
          description: 'desc',
          relevanceScore: 85,
          matchedKeywords: [],
          source: 'Firestore',
        ),
      ];

      final filtered = rankingEngine.applyDiversityFilter(results, maxPerType: 2);

      final lessonCount = filtered.where((r) => r.type == 'Lesson').length;
      final labCount = filtered.where((r) => r.type == 'Lab').length;

      expect(lessonCount, lessThanOrEqualTo(2));
      expect(labCount, lessThanOrEqualTo(2));
    });

    // ─────────────────────────────────────────────────────────
    // STATS TESTS
    // ─────────────────────────────────────────────────────────

    test('Result stats should calculate correctly', () {
      final results = [
        SearchResult(
          id: '1',
          title: 'Item 1',
          type: 'Lesson',
          category: 'Mechanics',
          description: 'desc',
          relevanceScore: 80,
          matchedKeywords: [],
          source: 'Firestore',
        ),
        SearchResult(
          id: '2',
          title: 'Item 2',
          type: 'Lab',
          category: 'Mechanics',
          description: 'desc',
          relevanceScore: 60,
          matchedKeywords: [],
          source: 'Firestore',
        ),
      ];

      final stats = rankingEngine.getResultStats(results);

      expect(stats['total'], 2);
      expect(stats['maxScore'], 80);
      expect(stats['minScore'], 60);
      expect(stats['averageScore'], 70);
    });
  });

  group('PDF Keyword Extractor', () {
    test('Should extract keywords from text', () {
      const text =
          'Friction is a force that opposes motion between surfaces. The coefficient of friction depends on the materials involved.';

      final keywords = PDFKeywordExtractor.extractKeywords(text);

      expect(keywords.isNotEmpty, true);
      expect(keywords.contains('friction'), true);
      expect(keywords.contains('force'), true);
    });

    test('Should extract n-grams correctly', () {
      const text = 'Newton laws of motion are fundamental principles';

      final ngrams = PDFKeywordExtractor.extractNGrams(text, ngramSize: 2);

      expect(ngrams.isNotEmpty, true);
      expect(ngrams.any((ng) => ng.contains('Newton')), true);
    });

    test('Should extract physics concepts', () {
      const text =
          'Velocity and acceleration are kinetic properties. Friction and force determine equilibrium.';

      final concepts = PDFKeywordExtractor.extractPhysicsConcepts(text);

      expect(concepts.isNotEmpty, true);
      expect(concepts.contains('velocity'), true);
      expect(concepts.contains('friction'), true);
      expect(concepts.contains('force'), true);
    });

    test('Should remove common stop words', () {
      const text = 'The force is applied to the object and it moves';

      final keywords = PDFKeywordExtractor.extractKeywords(text);

      // Stop words should not be present
      expect(keywords.contains('the'), false);
      expect(keywords.contains('is'), false);
      expect(keywords.contains('and'), false);
    });
  });

  group('Fuzzy Matching', () {
    late AdvancedRankingEngine rankingEngine;

    setUp(() {
      rankingEngine = AdvancedRankingEngine();
    });

    test('Should match similar terms for typos', () {
      const query = 'fricton'; // typo
      final availableOptions = ['friction', 'force', 'motion'];

      final suggestions = rankingEngine.getDidYouMeanSuggestions(
        query,
        availableOptions,
      );

      expect(suggestions.contains('friction'), true);
      expect(suggestions.isNotEmpty, true);
    });
  });
}
