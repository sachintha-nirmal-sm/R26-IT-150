/// Shared text-matching helpers for search — pulled out of
/// [AdvancedRankingEngine] so the master-keyword typo-tolerant lookup in
/// [SimpleSearchService] can reuse the exact same algorithm instead of a
/// second copy drifting out of sync with it.
library;

/// Shared typo-tolerance threshold — a query within this many single-
/// character edits of a known term counts as a fuzzy match. Used by both
/// [AdvancedRankingEngine]'s "did you mean" suggestions and the master-
/// keyword lookup, so the two stay consistent.
const int fuzzyMaxDistance = 2;

/// Edit distance between two strings (case-sensitive — callers should
/// lowercase both inputs first, matching existing call sites).
int levenshteinDistance(String s1, String s2) {
  final len1 = s1.length;
  final len2 = s2.length;

  if (len1 == 0) return len2;
  if (len2 == 0) return len1;

  final d = List.generate(
    len1 + 1,
    (_) => List<int>.filled(len2 + 1, 0),
  );

  for (var i = 0; i <= len1; i++) {
    d[i][0] = i;
  }

  for (var j = 0; j <= len2; j++) {
    d[0][j] = j;
  }

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
