import 'package:flutter/material.dart';
import 'dart:async';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:firebase_auth/firebase_auth.dart';
import '../services/simple_search_service.dart';
import '../../lessons/Lessons_Dashboard.dart';
import '../../lessons/learning_materials_page.dart';

class SimpleSearchPage extends StatefulWidget {
  final String grade;

  const SimpleSearchPage({
    Key? key,
    required this.grade,
  }) : super(key: key);

  @override
  State<SimpleSearchPage> createState() => _SimpleSearchPageState();
}

class _SimpleSearchPageState extends State<SimpleSearchPage> {
  final SimpleSearchService _searchService = SimpleSearchService();
  final TextEditingController _controller = TextEditingController();

  List<Map<String, dynamic>> _allResults = []; // All unfiltered results
  List<Map<String, dynamic>> _results = []; // Filtered results
  List<String> _suggestions = [];
  String? _correctedSpelling; // For typo suggestions
  bool _isLoading = false;
  bool _showSuggestions = false;
  String _selectedCategory = 'All';
  Timer? _debounceTimer;
  int _queryVersion = 0;
  int _searchRequestId = 0;

  final List<String> _categories = [
    'All',
    'Lesson',
    'Sub-Lesson',
    'Games',
    'Learning Materials',
  ];

  @override
  void initState() {
    super.initState();
    _controller.addListener(_onQueryChanged);

    // Check if a search query was passed (from keyword chips)
    WidgetsBinding.instance.addPostFrameCallback((_) {
      final args = ModalRoute.of(context)?.settings.arguments
          as Map<String, dynamic>?;
      if (args != null && args['searchQuery'] != null) {
        _controller.text = args['searchQuery'];
        Future.delayed(const Duration(milliseconds: 100), () {
          _performSearch();
        });
      }
    });
  }

  Future<void> _onQueryChanged() async {
    _debounceTimer?.cancel();
    final query = _controller.text;
    final version = ++_queryVersion;

    if (query.isEmpty) {
      setState(() {
        _showSuggestions = false;
        _suggestions = [];
        _results = [];
        _allResults = [];
      });
      return;
    }

    // Schedule the search immediately. Autocomplete is asynchronous and must
    // not create an additional timer after a newer query has been entered.
    _debounceTimer = Timer(const Duration(milliseconds: 500), () {
      if (version == _queryVersion) _performSearch();
    });

    final suggestions = await _searchService.getAutocompleteSuggestions(query);
    if (!mounted || version != _queryVersion || _controller.text != query) {
      return;
    }
    setState(() {
      _suggestions = suggestions;
      _showSuggestions = suggestions.isNotEmpty;
    });
  }

  Future<void> _performSearch() async {
    _debounceTimer?.cancel();
    final query = _controller.text.trim();
    if (query.isEmpty) return;
    final requestId = ++_searchRequestId;

    setState(() {
      _isLoading = true;
      _showSuggestions = false;
      _correctedSpelling = null;
    });

    try {
      final results = await _searchService.search(
        query,
        widget.grade,
        category: _selectedCategory,
      );

      // If no results, try to find similar query (typo correction)
      String? suggestion;
      if (results.isEmpty) {
        suggestion = await _searchService.findSimilarQuery(query, widget.grade);
      }

      if (!mounted ||
          requestId != _searchRequestId ||
          _controller.text.trim() != query) {
        return;
      }
      setState(() {
        _correctedSpelling = suggestion;
        _allResults = results;
      });
      _filterResults();
    } catch (e) {
      print('Error: $e');
      if (mounted && requestId == _searchRequestId) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Search failed: $e')),
        );
      }
    } finally {
      if (mounted && requestId == _searchRequestId) {
        setState(() => _isLoading = false);
      }
    }
  }

  void _onSuggestionTap(String suggestion) {
    _controller.text = suggestion;
    _performSearch();
  }

  void _applyCorrectedSpelling(String corrected) {
    _controller.text = corrected;
    _performSearch();
  }

  Future<void> _deleteRecentSearch(String query) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final historyKey = _getSearchHistoryKey();
      final history = prefs.getStringList(historyKey) ?? [];
      history.removeWhere((item) => item.toLowerCase() == query.toLowerCase());
      await prefs.setStringList(historyKey, history);
      setState(() => _suggestions = history.take(5).toList());
      print('❌ Deleted search: "$query"');
    } catch (e) {
      print('Error deleting search: $e');
    }
  }

  Future<void> _clearAllSearchHistory() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final historyKey = _getSearchHistoryKey();
      await prefs.remove(historyKey);
      setState(() => _suggestions = []);
      print('🗑️ Cleared all search history');
    } catch (e) {
      print('Error clearing history: $e');
    }
  }

  /// Get user-specific search history key
  String _getSearchHistoryKey() {
    final userId = FirebaseAuth.instance.currentUser?.uid;
    if (userId != null) {
      return 'search_history_$userId';
    }
    return 'search_history_guest';
  }

  void _filterResults() {
    // Filter ALL results based on selected category
    if (_allResults.isEmpty) {
      print('📊 No results to filter');
      setState(() => _results = []);
      return;
    }

    List<Map<String, dynamic>> filtered = _allResults;
    print('📊 Filtering results. Category: $_selectedCategory');
    print('📊 All results: ${_allResults.length}');

    if (_selectedCategory != 'All') {
      filtered = _allResults
          .where((r) {
            final type = r['type'] as String?;
            print('  - Checking: ${r['title']} (type: $type)');
            return type == _selectedCategory;
          })
          .toList();
    }

    print('✅ Filtered results: ${filtered.length}');
    setState(() => _results = filtered);
  }

  void _handleResultTap(Map<String, dynamic> result) {
    final type = result['type'] as String?;

    // Handle Games
    if (type == 'Games') {
      final route = result['route'] as String?;
      if (route != null) {
        print('🎮 Navigating to game: ${result['title']} ($route)');
        Navigator.pushNamed(context, route);
      }
      return;
    }

    // Handle Learning Materials
    if (type == 'Learning Materials') {
      final subtype = result['subtype'] as String?;
      print('📚 Navigating to learning material: ${result['title']} ($subtype)');
      
      final String lessonId = result['lessonId'] ?? '';
      final String lessonTitle = result['lessonTitle'] ?? 'Materials';
      final String grade = result['grade'] ?? widget.grade;

      Navigator.push(
        context,
        MaterialPageRoute(
          builder: (context) => LearningMaterialsPage(
            lessonId: lessonId,
            lessonTitle: lessonTitle,
            grade: grade,
          ),
        ),
      );
      return;
    }

    // Handle Lessons and Sub-Lessons (default)
    print('📖 Navigating to lesson: ${result['title']}');
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => LessonsDashboard(
          lessonId: result['id'],
          lessonTitle: result['title'],
          grade: widget.grade,
          lessonDescription: result['description'],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Search'),
        elevation: 0,
        backgroundColor: Colors.white,
        foregroundColor: const Color(0xFF1A1A2E),
      ),
      body: SingleChildScrollView(
        child: Column(
          children: [
            // Search input area
            Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Search bar
                  Row(
                    children: [
                      Expanded(
                        child: Container(
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
                          child: TextField(
                            controller: _controller,
                            decoration: InputDecoration(
                              hintText: 'Search lessons...',
                              border: InputBorder.none,
                              prefixIcon: const Icon(
                                Icons.search,
                                color: Color(0xFF2196F3),
                              ),
                              contentPadding: const EdgeInsets.symmetric(vertical: 12),
                            ),
                            onSubmitted: (_) => _performSearch(),
                          ),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Container(
                        decoration: BoxDecoration(
                          color: const Color(0xFF2196F3),
                          borderRadius: BorderRadius.circular(30),
                        ),
                        child: IconButton(
                          icon: const Icon(Icons.search, color: Colors.white),
                          onPressed: _performSearch,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 12),

                  // Category filter
                  SingleChildScrollView(
                    scrollDirection: Axis.horizontal,
                    child: Row(
                      children: _categories.map((category) {
                        final isSelected = _selectedCategory == category;
                        return Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 4),
                          child: FilterChip(
                            label: Text(category),
                            selected: isSelected,
                            onSelected: (_) {
                              setState(() => _selectedCategory = category);
                              _filterResults(); // Re-filter when category changes
                            },
                            backgroundColor: Colors.white,
                            selectedColor: const Color(0xFF2196F3),
                            labelStyle: TextStyle(
                              color: isSelected
                                  ? Colors.white
                                  : const Color(0xFF2196F3),
                            ),
                            side: BorderSide(
                              color: isSelected
                                  ? const Color(0xFF2196F3)
                                  : Colors.grey[300]!,
                            ),
                          ),
                        );
                      }).toList(),
                    ),
                  ),
                ],
              ),
            ),

            // Autocomplete suggestions + Recent searches
            if (_showSuggestions && _suggestions.isNotEmpty)
              Container(
                margin: const EdgeInsets.symmetric(horizontal: 16),
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
                    return ListTile(
                      leading: const Icon(Icons.history, size: 18),
                      title: Text(_suggestions[index]),
                      trailing: IconButton(
                        icon: const Icon(Icons.close, size: 18),
                        onPressed: () => _deleteRecentSearch(_suggestions[index]),
                        padding: EdgeInsets.zero,
                        constraints: const BoxConstraints(),
                      ),
                      onTap: () => _onSuggestionTap(_suggestions[index]),
                    );
                  },
                ),
              ),

            // Recent searches (when search box is empty)
            if (_controller.text.isEmpty)
              FutureBuilder<List<String>>(
                future: _searchService.getRecentSearches(),
                builder: (context, snapshot) {
                  final recentSearches = snapshot.data ?? [];
                  if (recentSearches.isEmpty) return const SizedBox.shrink();

                  return Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              'Recent Searches',
                              style: TextStyle(
                                fontSize: 14,
                                fontWeight: FontWeight.w600,
                                color: Colors.grey[700],
                              ),
                            ),
                            GestureDetector(
                              onTap: _clearAllSearchHistory,
                              child: Text(
                                'Clear All',
                                style: TextStyle(
                                  fontSize: 12,
                                  color: Colors.red[400],
                                  fontWeight: FontWeight.w500,
                                ),
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 8),
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
                            itemCount: recentSearches.length,
                            itemBuilder: (context, index) {
                              return ListTile(
                                leading: const Icon(Icons.history, size: 18, color: Colors.orange),
                                title: Text(recentSearches[index]),
                                trailing: IconButton(
                                  icon: const Icon(Icons.close, size: 18),
                                  onPressed: () => _deleteRecentSearch(recentSearches[index]),
                                  padding: EdgeInsets.zero,
                                  constraints: const BoxConstraints(),
                                ),
                                onTap: () => _onSuggestionTap(recentSearches[index]),
                              );
                            },
                          ),
                        ),
                      ],
                    ),
                  );
                },
              ),

            const SizedBox(height: 16),

            // Results area
            if (_isLoading)
              const Center(child: CircularProgressIndicator())
            else if (_results.isEmpty && _controller.text.isNotEmpty)
              Center(
                child: SingleChildScrollView(
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
                                'No results found',
                                style: TextStyle(
                                  fontSize: 16,
                                  color: Colors.grey[600],
                                ),
                              ),
                              const SizedBox(height: 8),
                              // "Did you mean?" suggestion
                              if (_correctedSpelling != null)
                                Padding(
                                  padding: const EdgeInsets.symmetric(
                                    horizontal: 24,
                                    vertical: 16,
                                  ),
                                  child: Container(
                                    decoration: BoxDecoration(
                                      color: const Color(0xFFFFF3E0),
                                      borderRadius: BorderRadius.circular(12),
                                      border: Border.all(
                                        color: const Color(0xFFFFB74D),
                                        width: 1.5,
                                      ),
                                    ),
                                    padding: const EdgeInsets.all(12),
                                    child: Column(
                                      children: [
                                        Text(
                                          'Did you mean?',
                                          style: TextStyle(
                                            fontSize: 13,
                                            color: Colors.orange[700],
                                            fontWeight: FontWeight.w500,
                                          ),
                                        ),
                                        const SizedBox(height: 8),
                                        GestureDetector(
                                          onTap: () => _applyCorrectedSpelling(
                                              _correctedSpelling!),
                                          child: Container(
                                            decoration: BoxDecoration(
                                              color: Colors.white,
                                              borderRadius:
                                                  BorderRadius.circular(8),
                                            ),
                                            padding: const EdgeInsets.symmetric(
                                              horizontal: 12,
                                              vertical: 8,
                                            ),
                                            child: Text(
                                              _correctedSpelling!,
                                              style: const TextStyle(
                                                fontSize: 14,
                                                fontWeight: FontWeight.w600,
                                                color: Color(0xFF2196F3),
                                              ),
                                            ),
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                )
                              else
                                Text(
                                  'Try a different search term',
                                  style: TextStyle(
                                    fontSize: 14,
                                    color: Colors.grey[400],
                                  ),
                                ),
                    ],
                  ),
                ),
              )
            else if (_results.isEmpty)
              Center(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(
                      Icons.search,
                      size: 48,
                      color: Colors.grey[300],
                    ),
                    const SizedBox(height: 16),
                    Text(
                      'Search for lessons',
                      style: TextStyle(
                        fontSize: 16,
                        color: Colors.grey[600],
                      ),
                    ),
                  ],
                ),
              )
            else
              ListView.builder(
                padding: const EdgeInsets.symmetric(horizontal: 16),
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                itemCount: _results.length,
                itemBuilder: (context, index) {
                  final result = _results[index];
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
                          onTap: () => _handleResultTap(result),
                          child: Padding(
                            padding: const EdgeInsets.all(16),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                // Type badge and title
                                Row(
                                  children: [
                                    Container(
                                      padding:
                                          const EdgeInsets.symmetric(
                                        horizontal: 8,
                                        vertical: 4,
                                      ),
                                      decoration: BoxDecoration(
                                        color: const Color(0xFF2196F3)
                                            .withOpacity(0.2),
                                        borderRadius:
                                            BorderRadius.circular(6),
                                      ),
                                      child: Text(
                                        result['type'],
                                        style: const TextStyle(
                                          fontSize: 12,
                                          fontWeight: FontWeight.w600,
                                          color: Color(0xFF2196F3),
                                        ),
                                      ),
                                    ),
                                    const SizedBox(width: 8),
                                    Expanded(
                                      child: Text(
                                        result['title'],
                                        style: const TextStyle(
                                          fontSize: 16,
                                          fontWeight: FontWeight.w600,
                                          color: Color(0xFF1A1A2E),
                                        ),
                                        maxLines: 2,
                                        overflow: TextOverflow.ellipsis,
                                      ),
                                    ),
                                  ],
                                ),
                                const SizedBox(height: 8),
                                // Description
                                Text(
                                  result['description'],
                                  style: TextStyle(
                                    fontSize: 13,
                                    color: Colors.grey[600],
                                  ),
                                  maxLines: 2,
                                  overflow: TextOverflow.ellipsis,
                                ),
                              ],
                            ),
                          ),
                        ),
                      ),
                    ),
                  );
                },
              ),
          ],
        ),
      ),
    );
  }

  @override
  void dispose() {
    _debounceTimer?.cancel();
    _controller.dispose();
    super.dispose();
  }
}
