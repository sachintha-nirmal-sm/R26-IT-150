import 'package:cloud_firestore/cloud_firestore.dart';
import '../models/search_models.dart';

class FirestoreSearchAdapter {
  static const String _lessonsCollection = 'lessons';
  static const String _gamesCollection = 'games';
  static const String _quizzesCollection = 'quizzes';
  static const String _virtualLabsCollection = 'virtual_labs';

  final FirebaseFirestore _firestore = FirebaseFirestore.instance;

  /// Fetch lessons from Firestore
  Future<List<SearchResult>> fetchLessons(String grade) async {
    try {
      final snapshot = await _firestore
          .collection(_lessonsCollection)
          .where('grade', isEqualTo: grade)
          .limit(100)
          .get();

      return snapshot.docs.map((doc) {
        final data = doc.data();
        return SearchResult(
          id: doc.id,
          title: data['title'] ?? 'Untitled Lesson',
          type: 'Lesson',
          category: data['topic'] ?? data['subject'] ?? 'General',
          description: data['description'] ?? '',
          relevanceScore: 0,
          matchedKeywords: List<String>.from(data['keywords'] ?? []),
          source: 'Firestore',
          lastAccessed: data['lastAccessed'] != null
              ? (data['lastAccessed'] as Timestamp).toDate()
              : null,
        );
      }).toList();
    } catch (e) {
      print('Error fetching lessons: $e');
      return [];
    }
  }

  /// Fetch games from Firestore
  Future<List<SearchResult>> fetchGames(String grade) async {
    try {
      final snapshot = await _firestore
          .collection(_gamesCollection)
          .where('grade', isEqualTo: grade)
          .limit(100)
          .get();

      return snapshot.docs.map((doc) {
        final data = doc.data();
        return SearchResult(
          id: doc.id,
          title: data['title'] ?? 'Untitled Game',
          type: 'Game',
          category: data['topic'] ?? 'Physics',
          description: data['description'] ?? '',
          relevanceScore: 0,
          matchedKeywords: [data['topic'] ?? '', data['difficulty'] ?? ''],
          source: 'Firestore',
          lastAccessed: data['lastPlayed'] != null
              ? (data['lastPlayed'] as Timestamp).toDate()
              : null,
        );
      }).toList();
    } catch (e) {
      print('Error fetching games: $e');
      return [];
    }
  }

  /// Fetch quizzes from Firestore
  Future<List<SearchResult>> fetchQuizzes(String grade) async {
    try {
      final snapshot = await _firestore
          .collection(_quizzesCollection)
          .where('grade', isEqualTo: grade)
          .limit(100)
          .get();

      return snapshot.docs.map((doc) {
        final data = doc.data();
        return SearchResult(
          id: doc.id,
          title: data['title'] ?? 'Untitled Quiz',
          type: 'Quiz',
          category: data['topic'] ?? 'General',
          description: data['description'] ?? '',
          relevanceScore: 0,
          matchedKeywords: [data['topic'] ?? '', data['difficulty'] ?? ''],
          source: 'Firestore',
          lastAccessed: data['lastAttempted'] != null
              ? (data['lastAttempted'] as Timestamp).toDate()
              : null,
        );
      }).toList();
    } catch (e) {
      print('Error fetching quizzes: $e');
      return [];
    }
  }

  /// Fetch virtual labs from Firestore
  Future<List<SearchResult>> fetchVirtualLabs(String grade) async {
    try {
      final snapshot = await _firestore
          .collection(_virtualLabsCollection)
          .where('grade', isEqualTo: grade)
          .limit(100)
          .get();

      return snapshot.docs.map((doc) {
        final data = doc.data();
        return SearchResult(
          id: doc.id,
          title: data['title'] ?? 'Untitled Lab',
          type: 'Lab',
          category: data['topic'] ?? 'General',
          description: data['description'] ?? '',
          relevanceScore: 0,
          matchedKeywords: [data['topic'] ?? '', data['type'] ?? ''],
          source: 'Firestore',
          lastAccessed: data['lastAccessed'] != null
              ? (data['lastAccessed'] as Timestamp).toDate()
              : null,
        );
      }).toList();
    } catch (e) {
      print('Error fetching labs: $e');
      return [];
    }
  }

  /// Fetch all searchable items from Firestore
  Future<List<SearchResult>> fetchAllSearchableItems(String grade) async {
    try {
      final results = <SearchResult>[];

      // Fetch all collections in parallel
      final futures = [
        fetchLessons(grade),
        fetchGames(grade),
        fetchQuizzes(grade),
        fetchVirtualLabs(grade),
      ];

      final allResults = await Future.wait(futures);

      for (final resultList in allResults) {
        results.addAll(resultList);
      }

      return results;
    } catch (e) {
      print('Error fetching all items: $e');
      return [];
    }
  }

  /// Fetch popular search terms for autocomplete
  Future<List<String>> fetchPopularSearchTerms(String grade, {int limit = 10}) async {
    try {
      final snapshot = await _firestore
          .collection('search_analytics')
          .where('grade', isEqualTo: grade)
          .orderBy('count', descending: true)
          .limit(limit)
          .get();

      return snapshot.docs.map((doc) => doc['query'] as String).toList();
    } catch (e) {
      print('Error fetching popular searches: $e');
      return [];
    }
  }

  /// Update last accessed timestamp for an item
  Future<void> updateLastAccessed(String collectionName, String docId) async {
    try {
      await _firestore
          .collection(collectionName)
          .doc(docId)
          .update({'lastAccessed': FieldValue.serverTimestamp()});
    } catch (e) {
      print('Error updating last accessed: $e');
    }
  }

  /// Batch fetch multiple grades' data
  Future<Map<String, List<SearchResult>>> fetchMultipleGrades(
      List<String> grades) async {
    try {
      final results = <String, List<SearchResult>>{};

      for (final grade in grades) {
        results[grade] = await fetchAllSearchableItems(grade);
      }

      return results;
    } catch (e) {
      print('Error fetching multiple grades: $e');
      return {};
    }
  }
}
