// Search data models

class SearchResult {
  final String id;
  final String title;
  final String type; // Lesson, Game, Lab, Quiz
  final String category;
  final String description;
  final double relevanceScore;
  final List<String> matchedKeywords;
  final String source; // PDF, Database, Game, etc
  final DateTime? lastAccessed;

  const SearchResult({
    required this.id,
    required this.title,
    required this.type,
    required this.category,
    required this.description,
    required this.relevanceScore,
    required this.matchedKeywords,
    required this.source,
    this.lastAccessed,
  });
}

class SearchSuggestion {
  final String text;
  final String type; // history, autocomplete, did-you-mean, popular
  final int priority; // Higher = show first
  final String? icon;

  const SearchSuggestion({
    required this.text,
    required this.type,
    required this.priority,
    this.icon,
  });
}

class RecentSearch {
  final String query;
  final DateTime timestamp;
  final String category;
  final String userId;

  const RecentSearch({
    required this.query,
    required this.timestamp,
    required this.category,
    required this.userId,
  });

  Map<String, dynamic> toMap() {
    return {
      'query': query,
      'timestamp': timestamp,
      'category': category,
      'userId': userId,
    };
  }

  factory RecentSearch.fromMap(Map<String, dynamic> map) {
    return RecentSearch(
      query: map['query'] as String,
      timestamp: (map['timestamp'] as DateTime),
      category: map['category'] as String,
      userId: map['userId'] as String,
    );
  }
}

class PDFKeywordIndex {
  final String lessonId;
  final String lessonTitle;
  final List<String> keywords;
  final String category;
  final String grade;

  const PDFKeywordIndex({
    required this.lessonId,
    required this.lessonTitle,
    required this.keywords,
    required this.category,
    required this.grade,
  });

  Map<String, dynamic> toMap() {
    return {
      'lessonId': lessonId,
      'lessonTitle': lessonTitle,
      'keywords': keywords,
      'category': category,
      'grade': grade,
    };
  }

  factory PDFKeywordIndex.fromMap(Map<String, dynamic> map) {
    return PDFKeywordIndex(
      lessonId: map['lessonId'] as String,
      lessonTitle: map['lessonTitle'] as String,
      keywords: List<String>.from(map['keywords'] as List),
      category: map['category'] as String,
      grade: map['grade'] as String,
    );
  }
}
