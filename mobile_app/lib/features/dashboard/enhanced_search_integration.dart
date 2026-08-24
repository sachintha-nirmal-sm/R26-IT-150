// ENHANCED DASHBOARD INTEGRATION FOR SMART SEARCH
// This file shows the updated dashboard code with integrated smart search

import 'package:flutter/material.dart';
import 'models/search_models.dart';
import 'services/integrated_search_service.dart';
import 'widgets/voice_search_widget.dart';
import 'screens/search_results_screen.dart';

/// Usage example in the dashboard state:
///
/// class _PhysicsLabHomePageState extends State<PhysicsLabHomePage> {
///   late IntegratedSearchService _searchService;
///   String _grade = 'Grade 10';
///   bool _isSearching = false;
///   List<SearchResult> _searchResults = [];
///   String _currentQuery = '';
///
///   @override
///   void initState() {
///     super.initState();
///     _searchService = IntegratedSearchService();
///   }
/// }

/// UPDATED SEARCH BAR WIDGET WITH VOICE SEARCH
class EnhancedSearchBarWidget extends StatefulWidget {
  final IntegratedSearchService searchService;
  final String grade;
  final Function(String, String) onSearch;

  const EnhancedSearchBarWidget({
    Key? key,
    required this.searchService,
    required this.grade,
    required this.onSearch,
  }) : super(key: key);

  @override
  State<EnhancedSearchBarWidget> createState() =>
      _EnhancedSearchBarWidgetState();
}

class _EnhancedSearchBarWidgetState extends State<EnhancedSearchBarWidget> {
  final TextEditingController _controller = TextEditingController();
  bool _isSearching = false;
  List<SearchSuggestion> _suggestions = [];
  String _selectedCategory = 'All';

  @override
  void initState() {
    super.initState();
    _controller.addListener(_onQueryChanged);
  }

  Future<void> _onQueryChanged() async {
    final query = _controller.text;

    if (query.isEmpty) {
      setState(() {
        _isSearching = false;
        _suggestions = [];
      });
      return;
    }

    // Get autocomplete suggestions
    final suggestions =
        await widget.searchService.getAutocompleteSuggestions(
      query: query,
      grade: widget.grade,
    );

    setState(() {
      _isSearching = true;
      _suggestions = suggestions;
    });
  }

  void _performSearch() {
    final query = _controller.text.trim();
    if (query.isNotEmpty) {
      widget.onSearch(query, _selectedCategory);
    }
  }

  void _showVoiceSearch() {
    showDialog(
      context: context,
      builder: (context) => VoiceSearchDialog(
        onSearch: (result) {
          _controller.text = result;
          _performSearch();
        },
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        // Search input with voice button
        Container(
          height: 56,
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(30),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withOpacity(0.08),
                blurRadius: 8,
                offset: const Offset(0, 2),
              ),
            ],
          ),
          padding: const EdgeInsets.symmetric(horizontal: 16),
          child: Row(
            children: [
              const Icon(Icons.search, color: Color(0xFF2196F3)),
              const SizedBox(width: 12),
              Expanded(
                child: TextField(
                  controller: _controller,
                  decoration: const InputDecoration(
                    border: InputBorder.none,
                    hintText: 'Search lessons, games, labs...',
                    hintStyle: TextStyle(color: Colors.grey),
                  ),
                  onSubmitted: (_) => _performSearch(),
                ),
              ),
              // Voice search button
              GestureDetector(
                onTap: _showVoiceSearch,
                child: const Icon(
                  Icons.mic_none,
                  color: Color(0xFF2196F3),
                ),
              ),
            ],
          ),
        ),
        const SizedBox(height: 12),

        // Category filter chips
        SingleChildScrollView(
          scrollDirection: Axis.horizontal,
          child: Row(
            children: ['All', 'Lessons', 'Games', 'Labs', 'Quizzes']
                .map((category) {
              final isSelected = _selectedCategory == category;
              return Padding(
                padding: const EdgeInsets.symmetric(horizontal: 4),
                child: FilterChip(
                  label: Text(category),
                  selected: isSelected,
                  onSelected: (_) {
                    setState(() => _selectedCategory = category);
                  },
                  backgroundColor: isSelected
                      ? const Color(0xFF2196F3)
                      : Colors.white,
                  labelStyle: TextStyle(
                    color: isSelected
                        ? Colors.white
                        : const Color(0xFF2196F3),
                  ),
                ),
              );
            }).toList(),
          ),
        ),
        const SizedBox(height: 12),

        // Suggestions dropdown
        if (_isSearching && _suggestions.isNotEmpty)
          Container(
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(12),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withOpacity(0.08),
                  blurRadius: 8,
                ),
              ],
            ),
            child: ListView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              itemCount: _suggestions.length,
              itemBuilder: (context, index) {
                final suggestion = _suggestions[index];
                return ListTile(
                  leading: Icon(
                    Icons.search,
                    size: 18,
                    color: Colors.grey,
                  ),
                  title: Text(suggestion.text),
                  onTap: () {
                    _controller.text = suggestion.text;
                    _performSearch();
                  },
                );
              },
            ),
          ),
      ],
    );
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }
}

/// SEARCH RESULTS DISPLAY WIDGET
class SearchResultsWidget extends StatelessWidget {
  final List<SearchResult> results;
  final bool isLoading;
  final String query;
  final Function(SearchResult) onResultTap;

  const SearchResultsWidget({
    Key? key,
    required this.results,
    required this.isLoading,
    required this.query,
    required this.onResultTap,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    if (isLoading) {
      return const Center(
        child: CircularProgressIndicator(
          color: Color(0xFF2196F3),
        ),
      );
    }

    if (results.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.search_off,
              size: 48,
              color: Colors.grey[300],
            ),
            const SizedBox(height: 16),
            Text(
              'No results found for "$query"',
              style: const TextStyle(
                fontSize: 16,
                color: Colors.grey,
              ),
            ),
          ],
        ),
      );
    }

    return ListView.builder(
      itemCount: results.length,
      itemBuilder: (context, index) {
        final result = results[index];
        return _buildResultCard(result, onResultTap);
      },
    );
  }

  Widget _buildResultCard(
    SearchResult result,
    Function(SearchResult) onTap,
  ) {
    final typeColor = _getTypeColor(result.type);

    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Container(
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.08),
              blurRadius: 8,
            ),
          ],
        ),
        child: Material(
          color: Colors.transparent,
          child: InkWell(
            borderRadius: BorderRadius.circular(12),
            onTap: () => onTap(result),
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 8,
                          vertical: 4,
                        ),
                        decoration: BoxDecoration(
                          color: typeColor.withOpacity(0.2),
                          borderRadius: BorderRadius.circular(6),
                        ),
                        child: Text(
                          result.type,
                          style: TextStyle(
                            fontSize: 12,
                            fontWeight: FontWeight.w600,
                            color: typeColor,
                          ),
                        ),
                      ),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          result.title,
                          style: const TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.w600,
                          ),
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  Text(
                    result.description,
                    style: TextStyle(
                      fontSize: 13,
                      color: Colors.grey[600],
                    ),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 12),
                  // Relevance indicator
                  ClipRRect(
                    borderRadius: BorderRadius.circular(4),
                    child: LinearProgressIndicator(
                      value: result.relevanceScore / 100,
                      minHeight: 4,
                      backgroundColor:
                          const Color(0xFFE0E0E0),
                      valueColor: AlwaysStoppedAnimation<Color>(
                        typeColor,
                      ),
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    'Relevance: ${result.relevanceScore.toStringAsFixed(0)}%',
                    style: TextStyle(
                      fontSize: 12,
                      color: Colors.grey[500],
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  Color _getTypeColor(String type) {
    switch (type.toLowerCase()) {
      case 'lesson':
        return const Color(0xFF2196F3);
      case 'game':
        return const Color(0xFF4CAF50);
      case 'lab':
        return const Color(0xFFFF9800);
      case 'quiz':
        return const Color(0xFFF44336);
      default:
        return Colors.grey;
    }
  }
}

/// INTEGRATION PATTERN FOR DASHBOARD
///
/// In your _PhysicsLabHomePageState:
///
/// Future<void> _performSearch(String query, String category) async {
///   setState(() => _isSearching = true);
///
///   try {
///     final results = await _searchService.search(
///       query: query,
///       grade: _grade,
///       category: category,
///       useCache: true,
///     );
///
///     setState(() {
///       _searchResults = results;
///       _currentQuery = query;
///       _isSearching = false;
///     });
///   } catch (e) {
///     print('Error: $e');
///     setState(() => _isSearching = false);
///   }
/// }
///
/// void _handleResultTap(SearchResult result) {
///   // Route based on result type and ID
///   switch (result.type) {
///     case 'Lesson':
///       Navigator.pushNamed(context, '/lesson-list',
///           arguments: {'lessonId': result.id});
///       break;
///     case 'Game':
///       Navigator.pushNamed(context, '/${result.id}');
///       break;
///     case 'Lab':
///       Navigator.pushNamed(context, '/practical-home',
///           arguments: {'labId': result.id});
///       break;
///     case 'Quiz':
///       Navigator.pushNamed(context, '/lesson-quizzes',
///           arguments: {'quizId': result.id});
///       break;
///   }
/// }
