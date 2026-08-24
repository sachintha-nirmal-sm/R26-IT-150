import 'package:flutter/material.dart';
import '../models/search_models.dart';
import '../services/search_service.dart';

class SmartSearchWidget extends StatefulWidget {
  final String grade;
  final Function(SearchResult) onResultTap;
  final Function(String, String)? onSearch;

  const SmartSearchWidget({
    Key? key,
    required this.grade,
    required this.onResultTap,
    this.onSearch,
  }) : super(key: key);

  @override
  State<SmartSearchWidget> createState() => _SmartSearchWidgetState();
}

class _SmartSearchWidgetState extends State<SmartSearchWidget> {
  final SearchService _searchService = SearchService();
  final TextEditingController _controller = TextEditingController();

  List<SearchSuggestion> _suggestions = [];
  List<RecentSearch> _recentSearches = [];
  bool _showSuggestions = false;
  String _selectedCategory = 'All';

  final categories = ['All', 'Lessons', 'Games', 'Labs', 'Quizzes'];

  @override
  void initState() {
    super.initState();
    _loadRecentSearches();
  }

  Future<void> _loadRecentSearches() async {
    final recent = await _searchService.getRecentSearches();
    setState(() {
      _recentSearches = recent;
    });
  }

  Future<void> _updateSuggestions(String query) async {
    if (query.isEmpty) {
      setState(() => _showSuggestions = false);
      return;
    }

    final suggestions = await _searchService.getAutocompleteSuggestions(
      query,
      widget.grade,
    );

    setState(() {
      _suggestions = suggestions;
      _showSuggestions = true;
    });
  }

  Future<void> _deleteRecentSearch(String query) async {
    await _searchService.deleteRecentSearch(query);
    await _loadRecentSearches();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        _buildSearchField(),
        const SizedBox(height: 12),
        _buildCategoryFilter(),
        const SizedBox(height: 16),
        if (_controller.text.isEmpty)
          _buildRecentSearches()
        else if (_showSuggestions)
          _buildSuggestionsDropdown()
        else
          const SizedBox.shrink(),
      ],
    );
  }

  Widget _buildSearchField() {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
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
        onChanged: _updateSuggestions,
        onSubmitted: (query) {
          if (query.isNotEmpty) {
            _searchService.saveRecentSearch(
              query: query,
              category: _selectedCategory,
            );
            widget.onSearch?.call(query, _selectedCategory);
          }
        },
        decoration: InputDecoration(
          hintText: 'Search lessons, games, labs...',
          hintStyle: TextStyle(color: Colors.grey[400]),
          prefixIcon: const Icon(Icons.search, color: Color(0xFF2196F3)),
          suffixIcon: _controller.text.isNotEmpty
              ? GestureDetector(
                  onTap: () {
                    _controller.clear();
                    setState(() => _showSuggestions = false);
                  },
                  child: const Icon(Icons.close, color: Colors.grey),
                )
              : null,
          border: InputBorder.none,
          contentPadding: const EdgeInsets.symmetric(
            horizontal: 16,
            vertical: 14,
          ),
        ),
        style: const TextStyle(fontSize: 16),
      ),
    );
  }

  Widget _buildCategoryFilter() {
    return SingleChildScrollView(
      scrollDirection: Axis.horizontal,
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 4),
        child: Row(
          children: categories.map((category) {
            final isSelected = _selectedCategory == category;
            return Padding(
              padding: const EdgeInsets.symmetric(horizontal: 4),
              child: FilterChip(
                label: Text(
                  category,
                  style: TextStyle(
                    color: isSelected
                        ? Colors.white
                        : const Color(0xFF2196F3),
                    fontWeight: FontWeight.w500,
                  ),
                ),
                onSelected: (_) {
                  setState(() => _selectedCategory = category);
                },
                backgroundColor: isSelected
                    ? const Color(0xFF2196F3)
                    : const Color(0xFFE8F1FF),
                side: BorderSide.none,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(8),
                ),
              ),
            );
          }).toList(),
        ),
      ),
    );
  }

  Widget _buildRecentSearches() {
    if (_recentSearches.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Text(
            'No recent searches',
            style: TextStyle(color: Colors.grey[400]),
          ),
        ),
      );
    }

    return Container(
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
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Padding(
            padding: const EdgeInsets.all(16),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Text(
                  'Recent Searches',
                  style: TextStyle(
                    fontSize: 14,
                    fontWeight: FontWeight.w600,
                    color: Colors.grey,
                  ),
                ),
                TextButton(
                  onPressed: () async {
                    await _searchService.clearAllRecentSearches();
                    await _loadRecentSearches();
                  },
                  child: const Text(
                    'Clear',
                    style: TextStyle(fontSize: 12, color: Color(0xFF2196F3)),
                  ),
                ),
              ],
            ),
          ),
          ...List.generate(_recentSearches.length, (index) {
            final search = _recentSearches[index];
            return ListTile(
              dense: true,
              leading: const Icon(
                Icons.history,
                size: 18,
                color: Colors.grey,
              ),
              title: Text(
                search.query,
                style: const TextStyle(fontSize: 14),
              ),
              trailing: GestureDetector(
                onTap: () => _deleteRecentSearch(search.query),
                child: const Icon(
                  Icons.close,
                  size: 18,
                  color: Colors.grey,
                ),
              ),
              onTap: () {
                _controller.text = search.query;
                _updateSuggestions(search.query);
              },
            );
          }),
        ],
      ),
    );
  }

  Widget _buildSuggestionsDropdown() {
    if (_suggestions.isEmpty) {
      return Container(
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
          boxShadow: [
            BoxShadow(color: Colors.black.withOpacity(0.08), blurRadius: 8),
          ],
        ),
        padding: const EdgeInsets.all(16),
        child: Text(
          'No suggestions found',
          style: TextStyle(color: Colors.grey[400]),
        ),
      );
    }

    return Container(
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
            dense: true,
            leading: Icon(
              suggestion.type == 'autocomplete'
                  ? Icons.search
                  : Icons.trending_up,
              size: 18,
              color: Colors.grey,
            ),
            title: Text(
              suggestion.text,
              style: const TextStyle(fontSize: 14),
            ),
            onTap: () {
              _controller.text = suggestion.text;
              _searchService.saveRecentSearch(
                query: suggestion.text,
                category: _selectedCategory,
              );
              widget.onSearch?.call(suggestion.text, _selectedCategory);
              setState(() => _showSuggestions = false);
            },
          );
        },
      ),
    );
  }
}
