import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:firebase_auth/firebase_auth.dart';
import '../../../core/api/api_client.dart';
import '../../games/games_list/game_list_data.dart';
import '../models/search_models.dart';
import 'advanced_ranking_engine.dart';
import 'search_text_utils.dart' as text_utils;

class SimpleSearchService {
  final FirebaseFirestore _firestore = FirebaseFirestore.instance;
  final FirebaseAuth _auth = FirebaseAuth.instance;
  final AdvancedRankingEngine _rankingEngine = AdvancedRankingEngine();
  final ApiClient _apiClient = ApiClient();

  // Cache lessons and sub-lessons in memory
  static Map<int, List<Map<String, dynamic>>> _lessonsCache = {};
  static Map<String, List<Map<String, dynamic>>> _subLessonsCache =
      {}; // key: lessonId
  static DateTime? _lastCacheTime;
  static const Duration _cacheDuration = Duration(hours: 1);

  // Master search-keyword index (approved/auto_approved docs only) — small,
  // curated, read far less often than it's matched against, so one cache
  // for the whole collection instead of per-grade like _lessonsCache.
  static List<Map<String, dynamic>>? _masterKeywordCache;
  static DateTime? _masterKeywordCacheTime;

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
      if (category == 'All' ||
          category == 'Lesson' ||
          category == 'Sub-Lesson') {
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
        results.addAll(await _searchSemanticPdfContent(query, gradeNum));
        results.addAll(
          await _searchMasterKeywordIndex(lowerQuery, gradeNum),
        );
      }

      // Rank all result types consistently while retaining every existing
      // search source (lessons, sub-lessons, games and learning materials).
      results = _rankResults(query, _mergeDuplicateResults(results));

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
        final description =
            (lesson['description'] as String? ?? '').toLowerCase();
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
            print(
                '📥 Fetched ${subLessons.length} sub-lessons for ${lesson['title']}');
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

      print(
          '🎮 Found ${games.length} games for $gradeStr matching "$lowerQuery"');
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

      // Search indexes created from the actual text extracted from uploaded
      // PDFs by the FastAPI RAG ingestion task.
      try {
        // Lessons in this project store grade as an integer. Querying by the
        // signed-in student's exact grade also satisfies the Firestore rule.
        final indexSnapshot = await _firestore
            .collection('pdf_keywords')
            .where('grade', isEqualTo: gradeNum)
            .get();

        final seenIndexes = <String>{};
        for (final doc in indexSnapshot.docs) {
          if (!seenIndexes.add(doc.id)) continue;
          final data = doc.data();
          final keywords =
              List<String>.from(data['keywords'] as List? ?? const []);
          final searchable = [
            data['materialName'],
            data['lessonTitle'],
            data['description'],
            data['category'],
            ...keywords,
          ].whereType<String>().join(' ').toLowerCase();

          if (!searchable.contains(lowerQuery)) continue;
          materials.add({
            'id': doc.id,
            'title':
                data['materialName'] ?? data['lessonTitle'] ?? 'PDF Material',
            'description': data['description'] ??
                'Matches content inside ${data['lessonTitle'] ?? 'this lesson'}',
            'type': 'Learning Materials',
            'subtype': data['materialType'] ?? 'PDF',
            'lessonId': data['lessonId'] ?? '',
            'lessonTitle': data['lessonTitle'] ?? 'Materials',
            'grade': gradeStr,
            'keywords': keywords,
            'source': 'PDF content',
          });
        }
      } catch (e) {
        // Keep legacy material, lesson and game search usable if the new index
        // has not been deployed or the current user cannot read it yet.
        print('Error searching PDF keyword indexes: $e');
      }

      // Search lesson_materials
      try {
        final materialsSnap = await _firestore
            .collection('lesson_materials')
            .where('grade', isEqualTo: gradeStr)
            .get();

        final lessonMaterials = materialsSnap.docs.where((doc) {
          final data = doc.data();
          final title = (data['materialName'] as String? ?? '').toLowerCase();
          final description =
              (data['description'] as String? ?? '').toLowerCase();
          final lessonTitle =
              (data['lessonTitle'] as String? ?? '').toLowerCase();
          final keywords =
              List<String>.from(data['keywords'] as List? ?? const []);
          return title.contains(lowerQuery) ||
              description.contains(lowerQuery) ||
              lessonTitle.contains(lowerQuery) ||
              keywords.any(
                (keyword) => keyword.toLowerCase().contains(lowerQuery),
              );
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
            'keywords':
                List<String>.from(data['keywords'] as List? ?? const []),
            'source': data['keywordIndexStatus'] == 'indexed'
                ? 'PDF content'
                : 'Material metadata',
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
          final description =
              (data['description'] as String? ?? '').toLowerCase();
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
        final pathsSnap = await _firestore.collection('learning_paths').get();

        final paths = pathsSnap.docs.where((doc) {
          final data = doc.data();
          final title = (data['title'] as String? ?? '').toLowerCase();
          final description =
              (data['description'] as String? ?? '').toLowerCase();
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

  Future<List<Map<String, dynamic>>> _searchSemanticPdfContent(
    String query,
    int gradeNum,
  ) async {
    try {
      final response = await _apiClient.post(
        '/search/semantic',
        body: {'query': query, 'grade': gradeNum, 'limit': 10},
      );
      if (response is! Map || response['results'] is! List) {
        print('Semantic PDF search returned an invalid response.');
        return [];
      }
      final results = (response['results'] as List)
          .whereType<Map>()
          .map((item) => Map<String, dynamic>.from(item))
          .toList();
      print(
        'Semantic PDF search returned ${results.length} result(s) '
        '(backend grade: ${response['grade']}).',
      );
      return results;
    } catch (error) {
      // Semantic search is an accuracy enhancement. Exact lesson, keyword,
      // game and material search remain available while FastAPI is offline.
      print('Semantic PDF search unavailable: $error');
      return [];
    }
  }

  /// Loads the live, moderated master keyword set (search_keyword_index) —
  /// see backend/app/rag/keyword_aggregate.py. Approved/auto_approved docs
  /// only; a rejected keyword must never recommend anything.
  Future<List<Map<String, dynamic>>> _getCachedMasterKeywords() async {
    if (_masterKeywordCache != null && _masterKeywordCacheTime != null) {
      if (DateTime.now().difference(_masterKeywordCacheTime!) <
          _cacheDuration) {
        return _masterKeywordCache!;
      }
    }

    try {
      final snap = await _firestore
          .collection('search_keyword_index')
          .where('status', whereIn: ['approved', 'auto_approved']).get();

      final keywords = snap.docs.map((doc) {
        final data = doc.data();
        return {
          'keyword': data['keyword'] ?? doc.id,
          'materialRefs':
              List<Map<String, dynamic>>.from(data['materialRefs'] ?? []),
        };
      }).toList();

      _masterKeywordCache = keywords;
      _masterKeywordCacheTime = DateTime.now();
      return keywords;
    } catch (e) {
      print('⚠️ Error loading master keyword index: $e');
      return _masterKeywordCache ?? [];
    }
  }

  /// Smart recommendation lookup: if the query (or a word within it) exactly
  /// matches an approved keyword, or is a 1-2 character typo away from one,
  /// surface everything that keyword is linked to — filtered to the
  /// student's own grade, so a Grade 10 student never sees a Grade 9 PDF
  /// recommended just because the keyword happens to be shared.
  Future<List<Map<String, dynamic>>> _searchMasterKeywordIndex(
    String lowerQuery,
    int gradeNum,
  ) async {
    final trimmed = lowerQuery.trim();
    if (trimmed.isEmpty) return [];

    final masterKeywords = await _getCachedMasterKeywords();
    if (masterKeywords.isEmpty) return [];

    final queryWords = [
      trimmed,
      ...trimmed.split(RegExp(r'\s+')).where((w) => w.length >= 3),
    ].toSet();

    Map<String, dynamic>? matched;
    var isFuzzyMatch = false;

    // Exact match first — cheap, and always preferred over a fuzzy guess.
    for (final entry in masterKeywords) {
      final keyword = (entry['keyword'] as String).toLowerCase();
      if (queryWords.contains(keyword)) {
        matched = entry;
        break;
      }
    }

    // Typo-tolerant fallback, only if nothing matched exactly. Skipped for
    // long queries — a short curated keyword is never a near-miss of a long
    // free-text search, so it would just waste cycles.
    if (matched == null && trimmed.length <= 20) {
      var bestDistance = text_utils.fuzzyMaxDistance + 1;
      for (final entry in masterKeywords) {
        final keyword = (entry['keyword'] as String).toLowerCase();
        for (final word in queryWords) {
          final distance = text_utils.levenshteinDistance(word, keyword);
          if (distance > 0 &&
              distance <= text_utils.fuzzyMaxDistance &&
              distance < bestDistance) {
            bestDistance = distance;
            matched = entry;
            isFuzzyMatch = true;
          }
        }
      }
    }

    if (matched == null) return [];

    final gradeStr = 'Grade $gradeNum';
    final matchedKeyword = matched['keyword'] as String;
    final refs = List<Map<String, dynamic>>.from(matched['materialRefs']);

    return refs
        .where((ref) => ref['grade'] == gradeStr)
        .map((ref) => {
              'id': ref['materialId'],
              'title': ref['lessonTitle'] ?? matchedKeyword,
              'description': isFuzzyMatch
                  ? 'Matched "$trimmed" to "$matchedKeyword"'
                  : 'Recommended based on "$matchedKeyword"',
              'type': 'Learning Materials',
              'lessonId': ref['lessonId'] ?? '',
              'lessonTitle': ref['lessonTitle'] ?? '',
              'grade': gradeStr,
              'source': 'Smart recommendation',
              'isMasterKeywordMatch': true,
              'matchedMasterKeyword': matchedKeyword,
              'isFuzzyMatch': isFuzzyMatch,
            })
        .toList();
  }

  List<Map<String, dynamic>> _mergeDuplicateResults(
    List<Map<String, dynamic>> results,
  ) {
    final merged = <String, Map<String, dynamic>>{};
    for (final result in results) {
      final type = result['type']?.toString() ?? '';
      final id = result['id']?.toString() ?? '';
      final lessonId = result['lessonId']?.toString() ?? '';
      final title = result['title']?.toString().toLowerCase() ?? '';
      final key = type == 'Learning Materials'
          ? 'material:${id.isNotEmpty ? id : '$lessonId:$title'}'
          : '$type:$id:$title';

      final existing = merged[key];
      if (existing == null) {
        merged[key] = Map<String, dynamic>.from(result);
        continue;
      }

      final currentSemantic =
          (existing['semanticScore'] as num?)?.toDouble() ?? 0;
      final newSemantic = (result['semanticScore'] as num?)?.toDouble() ?? 0;
      existing['semanticScore'] =
          currentSemantic > newSemantic ? currentSemantic : newSemantic;
      final keywords = <String>{
        ...List<String>.from(existing['keywords'] as List? ?? const []),
        ...List<String>.from(result['keywords'] as List? ?? const []),
      };
      existing['keywords'] = keywords.toList();
      if (newSemantic > 0) existing['source'] = 'Hybrid PDF search';

      if (result['isMasterKeywordMatch'] == true) {
        existing['isMasterKeywordMatch'] = true;
        existing['matchedMasterKeyword'] = result['matchedMasterKeyword'];
        existing['isFuzzyMatch'] = result['isFuzzyMatch'] ?? false;
      }
    }
    return merged.values.toList();
  }

  List<Map<String, dynamic>> _rankResults(
    String query,
    List<Map<String, dynamic>> results,
  ) {
    final scored = results.map((result) {
      final searchResult = SearchResult(
        id: result['id']?.toString() ?? '',
        title: result['title']?.toString() ?? '',
        type: result['type']?.toString() ?? '',
        category:
            result['topic']?.toString() ?? result['subtype']?.toString() ?? '',
        description: result['description']?.toString() ?? '',
        relevanceScore: 0,
        matchedKeywords: List<String>.from(
          result['keywords'] as List? ?? const <String>[],
        ),
        source: result['source']?.toString() ?? 'Search',
      );
      // Exact keyword-index match is a stronger signal than a corrected
      // (fuzzy) one, so it scores slightly higher — both still rank well
      // above a plain single-keyword overlap (+3 in AdvancedRankingEngine)
      // but below an exact title match (+100).
      final masterKeywordBonus = result['isMasterKeywordMatch'] == true
          ? (result['isFuzzyMatch'] == true ? 8.0 : 12.0)
          : 0.0;
      final score = _rankingEngine.calculateRelevanceScore(
            query: query,
            item: searchResult,
            searchCount: 0,
            totalSearches: 0,
          ) +
          ((result['semanticScore'] as num?)?.toDouble() ?? 0) * 25 +
          masterKeywordBonus;
      return MapEntry({...result, 'relevanceScore': score}, score);
    }).toList()
      ..sort((a, b) => b.value.compareTo(a.value));

    return scored.map((entry) => entry.key).toList();
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
          .where('status', isEqualTo: 'published')
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
        final maxLen =
            lowerQuery.length > term.length ? lowerQuery.length : term.length;
        final score = 1 - (distance / maxLen);

        print(
            '   "$lowerQuery" vs "$term" = ${(score * 100).toStringAsFixed(0)}%');

        if (score > bestScore && score > 0.5) {
          bestScore = score;
          bestMatch = term;
        }
      }

      if (bestMatch != null) {
        print(
            '✅ Typo detected (dictionary)! "$query" → "$bestMatch" (similarity: ${(bestScore * 100).toStringAsFixed(0)}%)');
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
