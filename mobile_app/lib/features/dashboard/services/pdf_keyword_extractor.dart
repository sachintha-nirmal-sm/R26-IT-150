/// Lightweight keyword extraction for already-extracted PDF text.
///
/// This does not parse PDF bytes; it converts plain text into searchable terms
/// and identifies common secondary-school physics concepts.
class PDFKeywordExtractor {
  static const Set<String> _stopWords = {
    'a',
    'an',
    'and',
    'are',
    'as',
    'at',
    'be',
    'been',
    'between',
    'by',
    'for',
    'from',
    'has',
    'have',
    'in',
    'into',
    'is',
    'it',
    'its',
    'of',
    'on',
    'or',
    'that',
    'the',
    'their',
    'this',
    'to',
    'was',
    'were',
    'will',
    'with',
  };

  static const Set<String> _physicsConcepts = {
    'acceleration',
    'ampere',
    'amplitude',
    'buoyancy',
    'charge',
    'circuit',
    'current',
    'density',
    'displacement',
    'electricity',
    'electromagnetism',
    'energy',
    'equilibrium',
    'force',
    'frequency',
    'friction',
    'gravity',
    'heat',
    'inertia',
    'kinetic',
    'lens',
    'light',
    'mass',
    'momentum',
    'motion',
    'optics',
    'power',
    'pressure',
    'resistance',
    'speed',
    'temperature',
    'thermal',
    'velocity',
    'voltage',
    'wave',
    'waves',
    'weight',
    'work',
  };

  static List<String> extractKeywords(String text, {int maxKeywords = 50}) {
    if (maxKeywords < 1) return const [];

    final frequencies = <String, int>{};
    for (final word in _words(text.toLowerCase())) {
      if (word.length < 3 || _stopWords.contains(word)) continue;
      frequencies[word] = (frequencies[word] ?? 0) + 1;
    }

    final ranked = frequencies.entries.toList()
      ..sort((a, b) {
        final frequency = b.value.compareTo(a.value);
        if (frequency != 0) return frequency;
        final conceptPriority = (_physicsConcepts.contains(b.key) ? 1 : 0)
            .compareTo(_physicsConcepts.contains(a.key) ? 1 : 0);
        return conceptPriority != 0 ? conceptPriority : a.key.compareTo(b.key);
      });

    return ranked.take(maxKeywords).map((entry) => entry.key).toList();
  }

  static List<String> extractNGrams(String text, {int ngramSize = 2}) {
    if (ngramSize < 1) return const [];
    final words = _words(text);
    if (words.length < ngramSize) return const [];

    return [
      for (var i = 0; i <= words.length - ngramSize; i++)
        words.sublist(i, i + ngramSize).join(' '),
    ];
  }

  static List<String> extractPhysicsConcepts(String text) {
    final found =
        _words(text.toLowerCase()).where(_physicsConcepts.contains).toSet();
    final concepts = found.toList()..sort();
    return concepts;
  }

  static List<String> _words(String text) =>
      RegExp(r"[A-Za-z0-9]+(?:'[A-Za-z0-9]+)?")
          .allMatches(text)
          .map((match) => match.group(0)!)
          .toList();
}
