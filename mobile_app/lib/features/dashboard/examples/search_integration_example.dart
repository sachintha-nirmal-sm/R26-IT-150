// SMART SEARCH INTEGRATION EXAMPLES
// This file shows how to use the smart search system in your app

import 'package:flutter/material.dart';
import '../widgets/smart_search_widget.dart';
import '../screens/search_results_screen.dart';
import '../models/search_models.dart';
import '../providers/search_data_provider.dart';
import '../services/search_service.dart';
import '../services/pdf_keyword_extractor.dart';

// ────────────────────────────────────────────────────────────
// EXAMPLE 1: Add Search Widget to Dashboard
// ────────────────────────────────────────────────────────────

class DashboardWithSearchExample extends StatefulWidget {
  @override
  State<DashboardWithSearchExample> createState() =>
      _DashboardWithSearchExampleState();
}

class _DashboardWithSearchExampleState extends State<DashboardWithSearchExample> {
  String _studentGrade = 'Grade 9';

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Dashboard')),
      body: SingleChildScrollView(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            children: [
              // Add SmartSearchWidget to your dashboard
              SmartSearchWidget(
                grade: _studentGrade,
                onResultTap: (result) {
                  // Handle tap on search result
                  _handleResultTap(result);
                },
                onSearch: (query, category) {
                  // Navigate to results screen
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (context) => SearchResultsScreen(
                        query: query,
                        category: category,
                        grade: _studentGrade,
                      ),
                    ),
                  );
                },
              ),
              const SizedBox(height: 24),
              // Rest of your dashboard content
            ],
          ),
        ),
      ),
    );
  }

  void _handleResultTap(SearchResult result) {
    // Route based on result type
    switch (result.type) {
      case 'Lesson':
        Navigator.pushNamed(
          context,
          '/lessons',
          arguments: {'lessonId': result.id},
        );
        break;
      case 'Game':
        Navigator.pushNamed(context, result.id);
        break;
      case 'Lab':
        Navigator.pushNamed(
          context,
          '/lab',
          arguments: {'labId': result.id},
        );
        break;
      case 'Quiz':
        Navigator.pushNamed(
          context,
          '/quiz',
          arguments: {'quizId': result.id},
        );
        break;
    }
  }
}

// ────────────────────────────────────────────────────────────
// EXAMPLE 2: Perform Smart Search Programmatically
// ────────────────────────────────────────────────────────────

class ProgrammaticSearchExample {
  final SearchDataProvider _provider = SearchDataProvider();

  Future<void> performSmartSearch() async {
    final results = await _provider.smartSearch(
      query: 'friction',
      grade: 'Grade 9',
      category: 'Lessons',
    );

    // Use results
    for (final result in results) {
      print('${result.title} (Relevance: ${result.relevanceScore}%)');
      print('Keywords: ${result.matchedKeywords.join(", ")}');
    }
  }

  Future<void> getDidYouMeanSuggestions() async {
    final suggestions = await _provider.getDidYouMeanSuggestions(
      'fricton', // typo
      'Grade 9',
    );

    // suggestions = ['friction', 'force', ...]
    print('Did you mean: ${suggestions.first}?');
  }

  Future<void> getTrendingSearches() async {
    final trending = await _provider.getTrendingSearches('Grade 9');

    for (final suggestion in trending) {
      print('${suggestion.text} (Priority: ${suggestion.priority})');
    }
  }
}

// ────────────────────────────────────────────────────────────
// EXAMPLE 3: Index a Lesson PDF
// ────────────────────────────────────────────────────────────

class IndexPDFExample {
  final SearchDataProvider _provider = SearchDataProvider();

  Future<void> indexNewLesson({
    required String lessonId,
    required String lessonTitle,
    required String pdfContent,
    required String category,
    required String grade,
  }) async {
    // When you upload a new lesson, index its content

    print('Indexing PDF for $lessonTitle...');

    await _provider.indexLessonPDF(
      lessonId: lessonId,
      lessonTitle: lessonTitle,
      pdfContent: pdfContent,
      category: category,
      grade: grade,
    );

    print('PDF indexed successfully!');

    // Now the PDF content is searchable
  }

  // Extract keywords from PDF
  Future<void> demonstrateKeywordExtraction() async {
    const pdfContent = '''
    Friction is a force that opposes motion between two surfaces.
    There are two main types of friction: static and kinetic friction.
    The coefficient of friction depends on the materials involved.
    Friction causes energy loss in mechanical systems.
    ''';

    // Extract keywords
    final keywords = PDFKeywordExtractor.extractKeywords(pdfContent);
    print('Keywords: $keywords');

    // Extract n-grams (phrases)
    final phrases = PDFKeywordExtractor.extractNGrams(pdfContent, ngramSize: 2);
    print('Phrases: $phrases');

    // Extract physics concepts
    final concepts = PDFKeywordExtractor.extractPhysicsConcepts(pdfContent);
    print('Physics Concepts: $concepts');
  }
}

// ────────────────────────────────────────────────────────────
// EXAMPLE 4: Manage Recent Searches
// ────────────────────────────────────────────────────────────

class RecentSearchesExample {
  final SearchService _searchService = SearchService();

  Future<void> demonstrateRecentSearches() async {
    // Save a search
    await _searchService.saveRecentSearch(
      query: 'Newton Laws',
      category: 'Lessons',
    );

    // Get all recent searches
    final recentSearches = await _searchService.getRecentSearches();
    print('Recent searches: ${recentSearches.map((s) => s.query).toList()}');

    // Delete specific search
    await _searchService.deleteRecentSearch('Newton Laws');

    // Clear all searches
    await _searchService.clearAllRecentSearches();
  }
}

// ────────────────────────────────────────────────────────────
// EXAMPLE 5: Use Search Widget in a Dialog
// ────────────────────────────────────────────────────────────

class SearchDialogExample {
  static void showSearchDialog(BuildContext context, String grade) {
    showDialog(
      context: context,
      builder: (context) => Dialog(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: SmartSearchWidget(
            grade: grade,
            onResultTap: (result) {
              Navigator.pop(context, result);
            },
            onSearch: (query, category) {
              Navigator.pop(context);
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (context) => SearchResultsScreen(
                    query: query,
                    category: category,
                    grade: grade,
                  ),
                ),
              );
            },
          ),
        ),
      ),
    );
  }
}

// ────────────────────────────────────────────────────────────
// EXAMPLE 6: Implement Search Analytics
// ────────────────────────────────────────────────────────────

class SearchAnalyticsExample {
  final SearchService _searchService = SearchService();

  Future<void> demonstrateAnalytics() async {
    // When user searches, it's automatically tracked
    await _searchService.saveRecentSearch(
      query: 'momentum',
      category: 'Lessons',
    );

    // Get trending searches to display to users
    final trending = await _searchService.getTrendingSearches('Grade 9');

    print('Trending searches this week:');
    for (final trend in trending) {
      print('  - ${trend.text}');
    }
  }
}

// ────────────────────────────────────────────────────────────
// EXAMPLE 7: Fuzzy Matching and Typo Correction
// ────────────────────────────────────────────────────────────

class FuzzyMatchingExample {
  final SearchService _searchService = SearchService();

  void demonstrateFuzzyMatching() {
    const availableOptions = [
      'Newton\'s Laws',
      'Friction Force',
      'Acceleration',
      'Velocity',
      'Momentum',
    ];

    // User types "newtonz" (typo)
    final suggestions = _searchService.getDidYouMeanSuggestions(
      'newtonz',
      availableOptions,
    );

    print('Did you mean: $suggestions');
    // Output: Did you mean: [Newton's Laws]
  }
}

// ────────────────────────────────────────────────────────────
// EXAMPLE 8: Full Integration in Navigation
// ────────────────────────────────────────────────────────────

class FullSearchIntegrationExample extends StatefulWidget {
  @override
  State<FullSearchIntegrationExample> createState() =>
      _FullSearchIntegrationExampleState();
}

class _FullSearchIntegrationExampleState
    extends State<FullSearchIntegrationExample> {
  final SearchDataProvider _provider = SearchDataProvider();
  String _studentGrade = 'Grade 9';
  bool _isSearching = false;
  List<SearchResult> _searchResults = [];

  void _performSearch(String query, String category) async {
    setState(() => _isSearching = true);

    try {
      final results = await _provider.smartSearch(
        query: query,
        grade: _studentGrade,
        category: category,
      );

      setState(() {
        _searchResults = results;
        _isSearching = false;
      });

      // Navigate to results screen
      if (mounted) {
        Navigator.push(
          context,
          MaterialPageRoute(
            builder: (context) => SearchResultsScreen(
              query: query,
              category: category,
              grade: _studentGrade,
            ),
          ),
        );
      }
    } catch (e) {
      setState(() => _isSearching = false);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Search failed: $e')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Full Search Integration')),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            SmartSearchWidget(
              grade: _studentGrade,
              onResultTap: (result) {
                // Handle direct tap
                Navigator.pushNamed(
                  context,
                  '/view-item',
                  arguments: result,
                );
              },
              onSearch: _performSearch,
            ),
            const SizedBox(height: 24),
            if (_isSearching)
              const CircularProgressIndicator()
            else if (_searchResults.isEmpty)
              const Text('No results yet')
            else
              Expanded(
                child: ListView.builder(
                  itemCount: _searchResults.length,
                  itemBuilder: (context, index) {
                    final result = _searchResults[index];
                    return ListTile(
                      title: Text(result.title),
                      subtitle: Text(result.type),
                      onTap: () {
                        // Navigate to result
                      },
                    );
                  },
                ),
              ),
          ],
        ),
      ),
    );
  }
}
