import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:string_similarity/string_similarity.dart';
import 'package:firebase_auth/firebase_auth.dart';
import '../../games/games_list/game_list_data.dart';

class SimpleSearchService {
  final FirebaseFirestore _firestore = FirebaseFirestore.instance;
  final FirebaseAuth _auth = FirebaseAuth.instance;

  // Cache lessons and sub-lessons in memory
  static Map<int, List<Map<String, dynamic>>> _lessonsCache = {};
  static Map<String, List<Map<String, dynamic>>> _subLessonsCache = {}; // key: lessonId
  static DateTime? _lastCacheTime;
  static const Duration _cacheDuration = Duration(hours: 1);

  /// Get user-specific search history key
  /// Format: search_history_[userId] for logged-in users
  /// Format: search_history_guest for guests
  String _getSearchHistoryKey() {
    final userId = _auth.currentUser?.uid;
    if (userId != null) {
      return 'search_history_$userId';
    }
    return 'search_history_guest'; // Fallback for guest users
  }

  /// Fast search - queries lessons, sub-lessons, games, and learning materials
  Future<List<Map<String, dynamic>>> search(
    String query,
    String grade, {
    String category = 'All',
  }) async {
    if (query.trim().isEmpty) return [];

    final lowerQuery = query.toLowerCase();
    final gradeNum = int.tryParse(grade.replaceAll('Grade ', '')) ?? 10;

    try {
      print('🔍 Searching for: "$query" (Category: $category)');

      List<Map<String, dynamic>> results = [];

      // Search Lessons & Sub-Lessons
      if (category == 'All' || category == 'Lesson' || category == 'Sub-Lesson') {
        results.addAll(await _searchLessonsAndSubLessons(
          lowerQuery,
          gradeNum,
          category,
        ));
      }

      // Search Games (filtered by grade)
      if (category == 'All' || category == 'Games') {
        results.addAll(await _searchGames(lowerQuery, gradeNum));
      }

      // Search Learning Materials
      if (category == 'All' || category == 'Learning Materials') {
        results.addAll(await _searchLearningMaterials(lowerQuery, gradeNum));
      }

      print('✅ Total results: ${results.length}');

      // Save to search history (async, don't wait)
      _saveSearchHistory(query);

      return results;
    } catch (e) {
      print('❌ Search error: $e');
      rethrow;
    }
  }

  /// Search lessons and sub-lessons
  Future<List<Map<String, dynamic>>> _searchLessonsAndSubLessons(
    String lowerQuery,
    int gradeNum,
    String category,
  ) async {
    List<Map<String, dynamic>> results = [];

    // Get lessons from cache
    final lessons = await _getCachedLessons(gradeNum);
    print('📚 Found ${lessons.length} lessons');

    // Search lessons in-memory (FAST)
    if (category == 'All' || category == 'Lesson') {
      for (var lesson in lessons) {
        final title = (lesson['title'] as String? ?? '').toLowerCase();
        final description = (lesson['description'] as String? ?? '').toLowerCase();
        final tag = (lesson['tag'] as String? ?? '').toLowerCase();

        if (title.contains(lowerQuery) ||
            description.contains(lowerQuery) ||
            tag.contains(lowerQuery)) {
          results.add({
            ...lesson,
            'type': 'Lesson',
          });
        }
      }
      print('✅ Found ${results.length} matching lessons');
    }

    // Search sub-lessons for EACH lesson
    if (category == 'All' || category == 'Sub-Lesson') {
      for (var lesson in lessons) {
        final lessonId = lesson['id'] as String;

        // Get or fetch sub-lessons for this lesson
        List<Map<String, dynamic>> subLessons = [];

        if (_subLessonsCache.containsKey(lessonId)) {
          subLessons = _subLessonsCache[lessonId]!;
        } else {
          // Fetch fresh
          try {
            final subLessonsSnap = await _firestore
                .collection('lessons')
                .doc(lessonId)
                .collection('subLessons')
                .get();

            subLessons = subLessonsSnap.docs.map((doc) {
              final data = doc.data();
              return {
                'id': lessonId,
                'subLessonId': doc.id,
                'title': data['title'] ?? 'Untitled Sub-Lesson',
                'description': 'Part of ${lesson['title']}',
                'type': 'Sub-Lesson',
                'grade': lesson['grade'],
              };
            }).toList();

            _subLessonsCache[lessonId] = subLessons;
            print('📥 Fetched ${subLessons.length} sub-lessons for ${lesson['title']}');
          } catch (e) {
            print('⚠️ Error fetching sub-lessons for ${lesson['title']}: $e');
          }
        }

        // Search sub-lessons
        for (var subLesson in subLessons) {
          final subTitle = (subLesson['title'] as String? ?? '').toLowerCase();
          if (subTitle.contains(lowerQuery)) {
            results.add(subLesson);
          }
        }
      }
    }

    return results;
  }

  /// Search games from static game list (filtered by grade)
  Future<List<Map<String, dynamic>>> _searchGames(
      String lowerQuery, int gradeNum) async {
    try {
      final gradeStr = 'Grade $gradeNum';

      final games = allGames.where((game) {
        // Filter by grade
        if (game.grade != gradeStr) return false;

        // Search by title, topic, or description
        final title = game.title.toLowerCase();
        final topic = game.topic.toLowerCase();
        final description = game.description.toLowerCase();
        return title.contains(lowerQuery) ||
            topic.contains(lowerQuery) ||
            description.contains(lowerQuery);
      }).map((game) {
        return {
          'id': game.title,
          'title': game.title,
          'description': game.description,
          'topic': game.topic,
          'grade': game.grade,
          'duration': game.duration,
          'route': game.route,
          'icon': game.icon,
          'type': 'Games',
        };
      }).toList();

      print('🎮 Found ${games.length} games for $gradeStr matching "$lowerQuery"');
      return games;
    } catch (e) {
      print('⚠️ Error searching games: $e');
      return [];
    }
  }

  /// Search learning materials (experiments, learning paths, etc.)
  Future<List<Map<String, dynamic>>> _searchLearningMaterials(
    String lowerQuery,
    int gradeNum,
  ) async {
    try {
      List<Map<String, dynamic>> materials = [];
      final String gradeStr = 'Grade $gradeNum';

      // Search lesson_materials
      try {
        final materialsSnap = await _firestore
            .collection('lesson_materials')
            .where('grade', isEqualTo: gradeStr)
            .get();

        final lessonMaterials = materialsSnap.docs.where((doc) {
          final data = doc.data();
          final title = (data['materialName'] as String? ?? '').toLowerCase();
          final description = (data['description'] as String? ?? '').toLowerCase();
          final lessonTitle = (data['lessonTitle'] as String? ?? '').toLowerCase();
          return title.contains(lowerQuery) || 
                 description.contains(lowerQuery) ||
                 lessonTitle.contains(lowerQuery);
        }).map((doc) {
          final data = doc.data();
          return {
            'id': doc.id,
            'title': data['materialName'] ?? 'Untitled Material',
            'description': data['description'] ?? 'Learning material',
            'type': 'Learning Materials',
            'subtype': data['materialType'] ?? 'Document',
            'fileUrl': data['fileUrl'],
            'lessonId': data['lessonId'] ?? '',
            'lessonTitle': data['lessonTitle'] ?? '',
            'grade': gradeStr,
          };
        }).toList();

        materials.addAll(lessonMaterials);
      } catch (e) {
        print('⚠️ Error searching lesson_materials: $e');
      }

      // Search experiments
      try {
        final experimentsSnap =
            await _firestore.collection('experiments').get();

        final experiments = experimentsSnap.docs.where((doc) {
          final data = doc.data();
          final title = (data['title'] as String? ?? '').toLowerCase();
          final description = (data['description'] as String? ?? '').toLowerCase();
          return title.contains(lowerQuery) || description.contains(lowerQuery);
        }).map((doc) {
          final data = doc.data();
          return {
            'id': doc.id,
            'title': data['title'] ?? 'Untitled Experiment',
            'description': data['description'] ?? 'A physics experiment',
            'type': 'Learning Materials',
            'subtype': 'Experiment',
          };
        }).toList();

        materials.addAll(experiments);
      } catch (e) {
        print('⚠️ Error searching experiments: $e');
      }

      // Search learning paths
      try {
        final pathsSnap =
            await _firestore.collection('learning_paths').get();

        final paths = pathsSnap.docs.where((doc) {
          final data = doc.data();
          final title = (data['title'] as String? ?? '').toLowerCase();
          final description = (data['description'] as String? ?? '').toLowerCase();
          return title.contains(lowerQuery) || description.contains(lowerQuery);
        }).map((doc) {
          final data = doc.data();
          return {
            'id': doc.id,
            'title': data['title'] ?? 'Untitled Learning Path',
            'description': data['description'] ?? 'A learning path',
            'type': 'Learning Materials',
            'subtype': 'Learning Path',
          };
        }).toList();

        materials.addAll(paths);
      } catch (e) {
        print('⚠️ Error searching learning paths: $e');
      }

      print('📚 Found ${materials.length} learning materials');
      return materials;
    } catch (e) {
      print('❌ Error searching learning materials: $e');
      return [];
    }
  }

  /// Get lessons from cache, or fetch from Firestore if expired
  Future<List<Map<String, dynamic>>> _getCachedLessons(int grade) async {
    // Check if cache is valid
    if (_lessonsCache.containsKey(grade) && _lastCacheTime != null) {
      if (DateTime.now().difference(_lastCacheTime!) < _cacheDuration) {
        print('✅ Using cached lessons for grade $grade');
        return _lessonsCache[grade]!;
      }
    }

    // Cache expired or not found - fetch fresh
    print('📥 Fetching lessons from Firestore for grade $grade');
    try {
      final lessonsSnap = await _firestore
          .collection('lessons')
          .where('grade', isEqualTo: grade)
          .get();

      final lessons = lessonsSnap.docs.map((doc) {
        final data = doc.data();
        return {
          'id': doc.id,
          'title': data['title'] ?? 'Untitled',
          'description': data['description'] ?? 'No description',
          'grade': data['grade'],
          'tag': data['lessonTag'] ?? '',
        };
      }).toList();

      // Cache the results
      _lessonsCache[grade] = lessons;
      _lastCacheTime = DateTime.now();

      return lessons;
    } catch (e) {
      print('Error fetching lessons: $e');
      rethrow;
    }
  }

  /// Get sub-lessons from cache, or fetch from Firestore if not cached
  Future<List<Map<String, dynamic>>> _getCachedSubLessons(
    List<Map<String, dynamic>> lessons,
  ) async {
    List<Map<String, dynamic>> allSubLessons = [];

    // Fetch sub-lessons for each lesson
    for (var lesson in lessons) {
      final lessonId = lesson['id'] as String;
      final lessonTitle = lesson['title'] as String;

      // Check if already cached
      if (_subLessonsCache.containsKey(lessonId)) {
        print('✅ Using cached sub-lessons for $lessonTitle');
        allSubLessons.addAll(_subLessonsCache[lessonId]!);
        continue;
      }

      // Fetch from Firestore
      print('📥 Fetching sub-lessons from Firestore for $lessonTitle');
      try {
        final subLessonsSnap = await _firestore
            .collection('lessons')
            .doc(lessonId)
            .collection('subLessons')
            .get();

        final subLessons = subLessonsSnap.docs.map((doc) {
          final data = doc.data();
          return {
            'id': lessonId, // Parent lesson ID
            'subLessonId': doc.id,
            'title': data['title'] ?? 'Untitled Sub-Lesson',
            'description': 'Part of $lessonTitle',
            'type': 'Sub-Lesson',
            'grade': lesson['grade'],
          };
        }).toList();

        // Cache the sub-lessons
        _subLessonsCache[lessonId] = subLessons;
        allSubLessons.addAll(subLessons);
      } catch (e) {
        print('Error fetching sub-lessons for $lessonId: $e');
        // Continue with other lessons
      }
    }

    return allSubLessons;
  }

  /// Clear cache (call this when lessons are updated)
  static void clearCache() {
    _lessonsCache.clear();
    _subLessonsCache.clear();
    _lastCacheTime = null;
    print('🗑️ Cache cleared');
  }

  /// Get autocomplete suggestions from recent searches (user-specific)
  Future<List<String>> getAutocompleteSuggestions(String query) async {
    if (query.isEmpty) return [];

    try {
      final prefs = await SharedPreferences.getInstance();
      final historyKey = _getSearchHistoryKey();
      final history = prefs.getStringList(historyKey) ?? [];

      final lowerQuery = query.toLowerCase();
      return history
          .where((item) => item.toLowerCase().startsWith(lowerQuery))
          .take(5)
          .toList();
    } catch (e) {
      print('Error getting suggestions: $e');
      return [];
    }
  }

  /// Get recent searches (user-specific)
  Future<List<String>> getRecentSearches() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final historyKey = _getSearchHistoryKey();
      return prefs.getStringList(historyKey) ?? [];
    } catch (e) {
      print('Error getting recent searches: $e');
      return [];
    }
  }

  /// Save search to history (user-specific)
  Future<void> _saveSearchHistory(String query) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final historyKey = _getSearchHistoryKey();
      final userId = _auth.currentUser?.uid;

      final history = prefs.getStringList(historyKey) ?? [];

      // Remove if already exists
      history.removeWhere((item) => item.toLowerCase() == query.toLowerCase());

      // Add to front
      history.insert(0, query);

      // Keep only last 20
      if (history.length > 20) {
        history.removeRange(20, history.length);
      }

      await prefs.setStringList(historyKey, history);

      final userTag = userId != null ? userId : 'guest';
      print('✅ Search history saved for user: $userTag');
    } catch (e) {
      print('Error saving search history: $e');
    }
  }

  /// Clear search history (user-specific)
  Future<void> clearSearchHistory() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final historyKey = _getSearchHistoryKey();
      final userId = _auth.currentUser?.uid;

      await prefs.remove(historyKey);

      final userTag = userId != null ? userId : 'guest';
      print('🗑️ Search history cleared for user: $userTag');
    } catch (e) {
      print('Error clearing search history: $e');
    }
  }

  /// Calculate Levenshtein distance (edit distance) for typo detection
  int _levenshteinDistance(String s1, String s2) {
    final List<List<int>> matrix =
        List.generate(s1.length + 1, (i) => List.filled(s2.length + 1, 0));

    for (int i = 0; i <= s1.length; i++) {
      matrix[i][0] = i;
    }
    for (int j = 0; j <= s2.length; j++) {
      matrix[0][j] = j;
    }

    for (int i = 1; i <= s1.length; i++) {
      for (int j = 1; j <= s2.length; j++) {
        final cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
        matrix[i][j] = [
          matrix[i - 1][j] + 1, // deletion
          matrix[i][j - 1] + 1, // insertion
          matrix[i - 1][j - 1] + cost, // substitution
        ].reduce((a, b) => a < b ? a : b);
      }
    }

    return matrix[s1.length][s2.length];
  }

  /// Physics terms dictionary for typo correction
  static const List<String> _physicsTerms = [
    'force',
    'acceleration',
    'velocity',
    'distance',
    'mass',
    'weight',
    'pressure',
    'density',
    'momentum',
    'energy',
    'power',
    'work',
    'heat',
    'temperature',
    'friction',
    'gravity',
    'motion',
    'speed',
    'inertia',
    'impulse',
    'torque',
    'equilibrium',
    'force and motion',
    'waves',
    'sound',
    'light',
    'electricity',
    'magnetism',
    'current',
    'voltage',
    'resistance',
    'capacitance',
    'inductance',
    'frequency',
    'wavelength',
    'amplitude',
    'quantum',
    'atom',
    'molecule',
    'element',
    'compound',
    'reaction',
    'viscosity',
  ];

  /// Find similar queries using fuzzy matching (for typo correction)
  Future<String?> findSimilarQuery(String query, String grade) async {
    if (query.trim().isEmpty) return null;

    try {
      final lowerQuery = query.toLowerCase();

      // First try database terms
      final dbMatch = await _findDatabaseMatch(lowerQuery);
      if (dbMatch != null) return dbMatch;

      // Fallback: check physics dictionary
      double bestScore = 0;
      String? bestMatch;

      for (var term in _physicsTerms) {
        final distance = _levenshteinDistance(lowerQuery, term);
        final maxLen = lowerQuery.length > term.length
            ? lowerQuery.length
            : term.length;
        final score = 1 - (distance / maxLen);

        print('   "$lowerQuery" vs "$term" = ${(score * 100).toStringAsFixed(0)}%');

        if (score > bestScore && score > 0.5) {
          bestScore = score;
          bestMatch = term;
        }
      }

      if (bestMatch != null) {
        print('✅ Typo detected (dictionary)! "$query" → "$bestMatch" (similarity: ${(bestScore * 100).toStringAsFixed(0)}%)');
        return bestMatch;
      }

      return null;
    } catch (e) {
      print('❌ Error finding similar query: $e');
      return null;
    }
  }

  /// Try to find match in database
  Future<String?> _findDatabaseMatch(String lowerQuery) async {
    try {
      // Try different grades
      for (int grade = 9; grade <= 12; grade++) {
        final lessons = await _getCachedLessons(grade);

        for (var lesson in lessons) {
          final title = (lesson['title'] as String).toLowerCase();
          final distance = _levenshteinDistance(lowerQuery, title);
          if (distance <= 2) {
            // Allow up to 2 edits
            print('✅ Typo detected (database)! "$lowerQuery" → "$title"');
            return title;
          }
        }
      }
      return null;
    } catch (e) {
      print('⚠️ Error searching database: $e');
      return null;
    }
  }
}
