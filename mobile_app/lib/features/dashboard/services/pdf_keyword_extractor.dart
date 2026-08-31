/// Extracts searchable keywords, phrases, and physics terms from lesson text.
class PDFKeywordExtractor {
  static const Set<String> _stopWords = {
    'a', 'an', 'the', 'and', 'or', 'but', 'if', 'then', 'else', 'when',
    'at', 'by', 'for', 'from', 'in', 'into', 'of', 'on', 'off', 'to',
    'up', 'with', 'as', 'is', 'are', 'was', 'were', 'be', 'been', 'being',
    'it', 'its', 'this', 'that', 'these', 'those', 'they', 'them', 'their',
    'we', 'you', 'he', 'she', 'his', 'her', 'our', 'your',
    'not', 'no', 'nor', 'so', 'too', 'very', 'can', 'could', 'should',
    'would', 'will', 'just', 'about', 'also', 'than',
    'between', 'over', 'under', 'after', 'before', 'because',
  };

  static const Set<String> _physicsConcepts = {
    'force', 'friction', 'motion', 'velocity', 'acceleration', 'speed',
    'mass', 'weight', 'gravity', 'energy', 'kinetic', 'potential', 'power',
    'work', 'pressure', 'density', 'volume', 'equilibrium', 'momentum',
    'newton', 'torque', 'moment', 'lever', 'upthrust', 'buoyancy',
    'current', 'voltage', 'resistance', 'circuit', 'charge', 'magnet',
    'wave', 'frequency', 'amplitude', 'reflection', 'refraction', 'lens',
    'heat', 'temperature', 'expansion', 'conduction', 'convection',
    'diode', 'electronics', 'optics',
  };

  static List<String> extractKeywords(String text) {
    final keywords = <String>[];
    final seen = <String>{};

    for (final token in _tokenize(text.toLowerCase())) {
      if (token.length < 3) continue;
      if (_stopWords.contains(token)) continue;
      if (seen.add(token)) {
        keywords.add(token);
      }
    }

    return keywords;
  }

  static List<String> extractNGrams(String text, {int ngramSize = 2}) {
    if (ngramSize <= 0) return [];

    final tokens = _tokenize(text);
    if (tokens.length < ngramSize) return [];

    final ngrams = <String>[];
    for (var i = 0; i <= tokens.length - ngramSize; i++) {
      ngrams.add(tokens.sublist(i, i + ngramSize).join(' '));
    }
    return ngrams;
  }

  static List<String> extractPhysicsConcepts(String text) {
    final lower = text.toLowerCase();
    final concepts = <String>[];

    for (final concept in _physicsConcepts) {
      if (RegExp('\\b${RegExp.escape(concept)}\\b').hasMatch(lower)) {
        concepts.add(concept);
      }
    }

    return concepts;
  }

  static List<String> _tokenize(String text) {
    return text
        .split(RegExp(r'[^A-Za-z]+'))
        .where((token) => token.isNotEmpty)
        .toList();
  }
}
