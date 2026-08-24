import 'package:cloud_firestore/cloud_firestore.dart';
import '../models/search_models.dart';

class PDFKeywordExtractor {
  static const List<String> _commonStopWords = [
    'the', 'a', 'an', 'and', 'or', 'but', 'in', 'on', 'at', 'to', 'for',
    'of', 'with', 'by', 'from', 'is', 'are', 'was', 'were', 'be', 'been',
    'being', 'have', 'has', 'had', 'do', 'does', 'did', 'will', 'would',
    'should', 'could', 'may', 'might', 'must', 'can', 'this', 'that',
    'these', 'those', 'i', 'you', 'he', 'she', 'it', 'we', 'they', 'what',
    'which', 'who', 'when', 'where', 'why', 'how', 'as', 'if', 'so', 'no',
    'not', 'yes', 'all', 'each', 'every', 'both', 'few', 'more', 'most',
    'other', 'some', 'such', 'no', 'nor', 'than', 'very', 'just', 'only',
    'own', 'same', 'so', 'then', 'too', 'very', 'well', 'about', 'into',
    'through', 'under', 'over', 'up', 'down', 'out', 'off', 'because',
  ];

  /// Extract keywords from PDF text content
  static List<String> extractKeywords(String text) {
    // Split text into words and clean
    final words = text
        .toLowerCase()
        .replaceAll(RegExp(r'[^\w\s]'), '')
        .split(RegExp(r'\s+'))
        .where((word) => word.length > 3 && !_commonStopWords.contains(word))
        .toList();

    // Get frequency of each word
    final frequency = <String, int>{};
    for (final word in words) {
      frequency[word] = (frequency[word] ?? 0) + 1;
    }

    // Sort by frequency and take top keywords
    final sortedKeywords = frequency.entries.toList();
    sortedKeywords.sort((a, b) => b.value.compareTo(a.value));

    return sortedKeywords
        .take(50)
        .map((entry) => entry.key)
        .toList();
  }

  /// Extract n-grams (phrases) for better matching
  static List<String> extractNGrams(String text, {int ngramSize = 2}) {
    final words = text
        .toLowerCase()
        .replaceAll(RegExp(r'[^\w\s]'), '')
        .split(RegExp(r'\s+'))
        .where((word) => word.length > 2)
        .toList();

    final ngrams = <String>[];
    for (int i = 0; i <= words.length - ngramSize; i++) {
      final ngram = words.sublist(i, i + ngramSize).join(' ');
      if (!ngram.split(' ').any((w) => _commonStopWords.contains(w))) {
        ngrams.add(ngram);
      }
    }

    // Remove duplicates and return most frequent
    final frequency = <String, int>{};
    for (final ngram in ngrams) {
      frequency[ngram] = (frequency[ngram] ?? 0) + 1;
    }

    final sortedNGrams = frequency.entries.toList();
    sortedNGrams.sort((a, b) => b.value.compareTo(a.value));

    return sortedNGrams.take(30).map((entry) => entry.key).toList();
  }

  /// Create searchable index from PDF content
  static Future<PDFKeywordIndex> createIndexFromPDF({
    required String lessonId,
    required String lessonTitle,
    required String pdfContent,
    required String category,
    required String grade,
  }) async {
    final keywords = extractKeywords(pdfContent);
    final phrases = extractNGrams(pdfContent, ngramSize: 2);

    final allKeywords = <String>[
      ...keywords,
      ...phrases,
      ...lessonTitle.toLowerCase().split(RegExp(r'\s+')),
    ];

    return PDFKeywordIndex(
      lessonId: lessonId,
      lessonTitle: lessonTitle,
      keywords: allKeywords.toSet().toList(), // Remove duplicates
      category: category,
      grade: grade,
    );
  }

  /// Search through indexed keywords with fuzzy matching
  static List<String> searchKeywords(
    String query,
    List<String> keywords, {
    int maxDistance = 2,
  }) {
    final queryLower = query.toLowerCase();
    final matchedKeywords = <String>[];

    for (final keyword in keywords) {
      final keywordLower = keyword.toLowerCase();

      // Exact match
      if (keywordLower == queryLower) {
        matchedKeywords.add(keyword);
        continue;
      }

      // Contains match
      if (keywordLower.contains(queryLower) || queryLower.contains(keywordLower)) {
        matchedKeywords.add(keyword);
        continue;
      }

      // Fuzzy match using Levenshtein distance
      final distance = _levenshteinDistance(queryLower, keywordLower);
      if (distance <= maxDistance) {
        matchedKeywords.add(keyword);
      }
    }

    return matchedKeywords;
  }

  /// Levenshtein distance for fuzzy matching
  static int _levenshteinDistance(String s1, String s2) {
    final len1 = s1.length;
    final len2 = s2.length;

    if (len1 == 0) return len2;
    if (len2 == 0) return len1;

    final d = List.generate(len1 + 1, (i) => List.filled(len2 + 1, 0));

    for (var i = 0; i <= len1; i++) d[i][0] = i;
    for (var j = 0; j <= len2; j++) d[0][j] = j;

    for (var i = 1; i <= len1; i++) {
      for (var j = 1; j <= len2; j++) {
        final cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
        d[i][j] = [
          d[i - 1][j] + 1,
          d[i][j - 1] + 1,
          d[i - 1][j - 1] + cost,
        ].reduce((a, b) => a < b ? a : b);
      }
    }

    return d[len1][len2];
  }

  /// Extract technical terms and physics concepts
  static List<String> extractPhysicsConcepts(String text) {
    final physicsTerm = RegExp(
      r'\b(force|velocity|acceleration|momentum|energy|power|work|friction|gravity|mass|weight|motion|distance|displacement|speed|kinetic|potential|torque|pressure|temperature|heat|frequency|wavelength|amplitude|resonance|optics|refraction|reflection|electricity|magnetism|circuit|resistance|current|voltage|induction|field|vector|scalar|equilibrium|friction|elastic|plastic|deformation|strain|stress)\b',
      caseSensitive: false,
    );

    final matches = <String>{};
    for (final match in physicsTerm.allMatches(text)) {
      matches.add(match.group(0)?.toLowerCase() ?? '');
    }

    return matches.toList();
  }

  /// Save indexed keywords to Firestore
  static Future<void> saveIndexToFirestore(PDFKeywordIndex index) async {
    try {
      await FirebaseFirestore.instance
          .collection('pdf_keywords')
          .doc(index.lessonId)
          .set(index.toMap());
    } catch (e) {
      print('Error saving PDF index to Firestore: $e');
    }
  }

  /// Get indexed keywords from Firestore
  static Future<PDFKeywordIndex?> getIndexFromFirestore(String lessonId) async {
    try {
      final doc = await FirebaseFirestore.instance
          .collection('pdf_keywords')
          .doc(lessonId)
          .get();

      if (doc.exists) {
        return PDFKeywordIndex.fromMap(doc.data() ?? {});
      }
      return null;
    } catch (e) {
      print('Error fetching PDF index from Firestore: $e');
      return null;
    }
  }

  /// Batch index multiple PDFs
  static Future<void> batchIndexPDFs(List<PDFKeywordIndex> indices) async {
    try {
      final batch = FirebaseFirestore.instance.batch();

      for (final index in indices) {
        final docRef = FirebaseFirestore.instance
            .collection('pdf_keywords')
            .doc(index.lessonId);
        batch.set(docRef, index.toMap());
      }

      await batch.commit();
    } catch (e) {
      print('Error batch indexing PDFs: $e');
    }
  }
}
