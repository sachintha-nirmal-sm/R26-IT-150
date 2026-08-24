import 'package:flutter/material.dart';
import '../models/search_models.dart';
import '../services/integrated_search_service.dart';
import '../enhanced_search_integration.dart';

class SearchPage extends StatefulWidget {
  final String? grade;

  const SearchPage({
    Key? key,
    this.grade,
  }) : super(key: key);

  @override
  State<SearchPage> createState() => _SearchPageState();
}

class _SearchPageState extends State<SearchPage> {
  late IntegratedSearchService _searchService;
  late String _userGrade;
  bool _isLoading = false;
  List<SearchResult> _results = [];
  String _currentQuery = '';
  String _selectedCategory = 'All';

  @override
  void initState() {
    super.initState();
    _searchService = IntegratedSearchService();
    _userGrade = grade ?? 'Grade 10';
  }

  Future<void> _performSearch(String query, String category) async {
    setState(() {
      _isLoading = true;
      _currentQuery = query;
      _selectedCategory = category;
    });

    try {
      final results = await _searchService.search(
        query: query,
        grade: _userGrade,
        category: category,
        useCache: true,
      );

      setState(() {
        _results = results;
        _isLoading = false;
      });
    } catch (e) {
      print('Search error: $e');
      setState(() => _isLoading = false);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Search error: $e'),
            backgroundColor: Colors.red,
          ),
        );
      }
    }
  }

  void _handleResultTap(SearchResult result) {
    print('Tapped: ${result.title} (${result.type})');

    // Route based on result type
    switch (result.type.toLowerCase()) {
      case 'lesson':
        Navigator.pushNamed(
          context,
          '/lesson-list',
          arguments: {'grade': _userGrade},
        );
        break;
      case 'game':
        // Route to game using result ID
        Navigator.pushNamed(context, '/${result.id}');
        break;
      case 'lab':
        Navigator.pushNamed(
          context,
          '/practical-home',
          arguments: {'grade': _userGrade},
        );
        break;
      case 'quiz':
        Navigator.pushNamed(
          context,
          '/lesson-quizzes',
          arguments: {'grade': _userGrade},
        );
        break;
      default:
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Opening ${result.title}')),
        );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4F6FB),
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Color(0xFF1A1A2E)),
          onPressed: () => Navigator.pop(context),
        ),
        title: const Text(
          'Search',
          style: TextStyle(
            color: Color(0xFF1A1A2E),
            fontWeight: FontWeight.w600,
            fontSize: 18,
          ),
        ),
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(16),
          child: Column(
            children: [
              // Enhanced search bar with voice
              EnhancedSearchBarWidget(
                searchService: _searchService,
                grade: _userGrade,
                onSearch: _performSearch,
              ),
              const SizedBox(height: 20),

              // Results or empty state
              if (_currentQuery.isEmpty)
                Center(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Icon(
                        Icons.search,
                        size: 64,
                        color: Colors.grey[300],
                      ),
                      const SizedBox(height: 16),
                      Text(
                        'Search for lessons, games, labs...',
                        style: TextStyle(
                          fontSize: 16,
                          color: Colors.grey[600],
                        ),
                      ),
                    ],
                  ),
                )
              else
                SearchResultsWidget(
                  results: _results,
                  isLoading: _isLoading,
                  query: _currentQuery,
                  onResultTap: _handleResultTap,
                ),
            ],
          ),
        ),
      ),
    );
  }
}
