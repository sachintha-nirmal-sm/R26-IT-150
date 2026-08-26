// ── Updated Search Data for Sri Lankan Physics Curriculum (English) ──

class SearchItem {
  final String title;
  final String path;
  final String duration;
  final String type; // Lesson | Lab | Quiz | Game | Other
  final String grade; // 'Grade 9' | 'Grade 10' | 'Grade 11'

  const SearchItem({
    required this.title,
    required this.path,
    required this.duration,
    required this.type,
    required this.grade,
  });
}

const List<SearchItem> allSearchItems = [
  // ── Grade 9: Force and Energy ──
  SearchItem(title: 'Force and Pressure', path: 'Grade 9 > Physics > Lessons', duration: '20 mins', type: 'Lesson', grade: 'Grade 9'),
  SearchItem(title: 'Force and Pressure', path: 'Grade 9 > Physics > Labs', duration: '45 mins', type: 'Lab', grade: 'Grade 9'),
  SearchItem(title: 'Force and Pressure', path: 'Grade 9 > Physics > Quizzes', duration: '10 mins', type: 'Quiz', grade: 'Grade 9'),
  SearchItem(title: 'Force and Pressure', path: 'Grade 9 > Physics > Games', duration: '20 mins', type: 'Game', grade: 'Grade 9'),
  
  SearchItem(title: 'Work, Energy, and Power', path: 'Grade 9 > Physics > Lessons', duration: '18 mins', type: 'Lesson', grade: 'Grade 9'),
  
  
  
  
  SearchItem(title: 'Static Electricity', path: 'Grade 9 > Physics > Lessons', duration: '15 mins', type: 'Lesson', grade: 'Grade 9'),
  SearchItem(title: 'Archimedes\' Principle Lab', path: 'Grade 9 > Physics > Labs', duration: '35 mins', type: 'Lab', grade: 'Grade 9'),
  SearchItem(title: 'Pressure in Liquids Quiz', path: 'Grade 9 > Physics > Quizzes', duration: '10 mins', type: 'Quiz', grade: 'Grade 9'),

  // ── Grade 10: Motion and Light ──
  SearchItem(title: 'Motion in a straight line', path: 'Grade 10 > Physics > Lessons', duration: '25 mins', type: 'Lesson', grade: 'Grade 10'),
  SearchItem(title: 'Motion in a straight line', path: 'Grade 10 > Physics > Labs', duration: '45 mins', type: 'Lab', grade: 'Grade 10'),
  SearchItem(title: 'Motion in a straight line', path: 'Grade 10 > Physics > Quizzes', duration: '10 mins', type: 'Quiz', grade: 'Grade 10'),
  SearchItem(title: 'Motion in a straight line', path: 'Grade 10 > Physics > Games', duration: '20 mins', type: 'Game', grade: 'Grade 10'),

  SearchItem(title: "Newton's laws of motion", path: 'Grade 10 > Physics > Lessons', duration: '22 mins', type: 'Lesson', grade: 'Grade 10'),
  SearchItem(title: "Newton's laws of motion", path: 'Grade 10 > Physics > Labs', duration: '45 mins', type: 'Lab', grade: 'Grade 10'),
  SearchItem(title: "Newton's laws of motion", path: 'Grade 10 > Physics > Quizzes', duration: '10 mins', type: 'Quiz', grade: 'Grade 10'),
  SearchItem(title: "Newton's laws of motion", path: 'Grade 10 > Physics > Games', duration: '20 mins', type: 'Game', grade: 'Grade 10'),

  SearchItem(title: 'Friction', path: 'Grade 10 > Physics > Lessons', duration: '20 mins', type: 'Lesson', grade: 'Grade 10'),
  SearchItem(title: 'Friction', path: 'Grade 10 > Physics > Labs', duration: '45 mins', type: 'Lab', grade: 'Grade 10'),
  SearchItem(title: 'Friction', path: 'Grade 10 > Physics > Quizzes', duration: '10 mins', type: 'Quiz', grade: 'Grade 10'),
  SearchItem(title: 'Friction', path: 'Grade 10 > Physics > Games', duration: '20 mins', type: 'Game', grade: 'Grade 10'),

  SearchItem(title: 'Resultant force', path: 'Grade 10 > Physics > Lessons', duration: '18 mins', type: 'Lesson', grade: 'Grade 10'),
  SearchItem(title: 'Resultant force', path: 'Grade 10 > Physics > Labs', duration: '45 mins', type: 'Lab', grade: 'Grade 10'),
  SearchItem(title: 'Resultant force', path: 'Grade 10 > Physics > Quizzes', duration: '10 mins', type: 'Quiz', grade: 'Grade 10'),
  SearchItem(title: 'Resultant force', path: 'Grade 10 > Physics > Games', duration: '20 mins', type: 'Game', grade: 'Grade 10'),

  SearchItem(title: 'Turning effect of a force', path: 'Grade 10 > Physics > Labs', duration: '40 mins', type: 'Lab', grade: 'Grade 10'),
  SearchItem(title: 'Turning effect of a force', path: 'Grade 10 > Physics > Quizzes', duration: '10 mins', type: 'Quiz', grade: 'Grade 10'),
  SearchItem(title: 'Turning effect of a force', path: 'Grade 10 > Physics > Games', duration: '20 mins', type: 'Game', grade: 'Grade 10'),
  SearchItem(title: 'Turning effect of a force', path: 'Grade 10 > Physics > Lessons', duration: '15 mins', type: 'Lesson', grade: 'Grade 10'),

  SearchItem(title: 'Equilibrium of forces', path: 'Grade 10 > Physics > Lessons', duration: '15 mins', type: 'Lesson', grade: 'Grade 10'),
  SearchItem(title: 'Equilibrium of forces', path: 'Grade 10 > Physics > Labs', duration: '40 mins', type: 'Lab', grade: 'Grade 10'),
  SearchItem(title: 'Equilibrium of forces', path: 'Grade 10 > Physics > Quizzes', duration: '10 mins', type: 'Quiz', grade: 'Grade 10'),
  SearchItem(title: 'Equilibrium of forces', path: 'Grade 10 > Physics > Games', duration: '20 mins', type: 'Game', grade: 'Grade 10'),

  SearchItem(title: 'Hydrostatic pressure and its applications', path: 'Grade 10 > Physics > Labs', duration: '40 mins', type: 'Lab', grade: 'Grade 10'),
  SearchItem(title: 'Hydrostatic pressure and its applications', path: 'Grade 10 > Physics > Quizzes', duration: '10 mins', type: 'Quiz', grade: 'Grade 10'),
  SearchItem(title: 'Hydrostatic pressure and its applications', path: 'Grade 10 > Physics > Games', duration: '20 mins', type: 'Game', grade: 'Grade 10'),
  SearchItem(title: 'Hydrostatic pressure and its applications', path: 'Grade 10 > Physics > Lessons', duration: '15 mins', type: 'Lesson', grade: 'Grade 10'),

  SearchItem(title: 'Work, energy and power', path: 'Grade 10 > Physics > Labs', duration: '40 mins', type: 'Lab', grade: 'Grade 10'),
  SearchItem(title: 'Work, energy and power', path: 'Grade 10 > Physics > Quizzes', duration: '10 mins', type: 'Quiz', grade: 'Grade 10'),
  SearchItem(title: 'Work, energy and power', path: 'Grade 10 > Physics > Games', duration: '20 mins', type: 'Game', grade: 'Grade 10'),
  SearchItem(title: 'Work, energy and power', path: 'Grade 10 > Physics > Lessons', duration: '15 mins', type: 'Lesson', grade: 'Grade 10'),  

  SearchItem(title: 'Current electricity', path: 'Grade 10 > Physics > Labs', duration: '40 mins', type: 'Lab', grade: 'Grade 10'),
  SearchItem(title: 'Current electricity', path: 'Grade 10 > Physics > Quizzes', duration: '10 mins', type: 'Quiz', grade: 'Grade 10'),
  SearchItem(title: 'Current electricity', path: 'Grade 10 > Physics > Games', duration: '20 mins', type: 'Game', grade: 'Grade 10'),
  SearchItem(title: 'Current electricity', path: 'Grade 10 > Physics > Lessons', duration: '15 mins', type: 'Lesson', grade: 'Grade 10'), 


  // ── Grade 11: Waves, Heat, and Electronics ──
  SearchItem(title: 'Heat & Temperature Changes', path: 'Grade 11 > Physics > Lessons', duration: '20 mins', type: 'Lesson', grade: 'Grade 11'),
  SearchItem(title: 'Waves and their Applications', path: 'Grade 11 > Physics > Lessons', duration: '22 mins', type: 'Lesson', grade: 'Grade 11'),
  SearchItem(title: 'Electromagnetism & Induction', path: 'Grade 11 > Physics > Lessons', duration: '25 mins', type: 'Lesson', grade: 'Grade 11'),
  SearchItem(title: 'Electronics & Logic Gates', path: 'Grade 11 > Physics > Lessons', duration: '18 mins', type: 'Lesson', grade: 'Grade 11'),
  SearchItem(title: 'Specific Heat Capacity Lab', path: 'Grade 11 > Physics > Labs', duration: '45 mins', type: 'Lab', grade: 'Grade 11'),
  SearchItem(title: 'Radioactivity & Nuclear Energy', path: 'Grade 11 > Physics > Lessons', duration: '15 mins', type: 'Lesson', grade: 'Grade 11'),
];

// ── Grade-specific Keywords for SL Syllabus ────────────────────────────
// Optimized for matching lessons, sub-lessons, games, and materials
const Map<String, List<String>> gradeKeywords = {
  'Grade 9': [
    'simple machines', 'pressure', 'density', 'archimedes',
    'work & energy', 'static electricity', 'moments', 'friction',
  ],
  'Grade 10': [
    // Motion - broad keywords with high match count
    'motion', 'kinematics', 'graphs',

    // Forces - high match content
    'force', 'newton', 'friction', 'equilibrium',

    // High-match keywords
    'energy', 'work', 'power', 'electricity',
    'pressure', 'vectors', 'moments',
  ],
  'Grade 11': [
    // High-match keywords covering all Grade 11 content
    'waves', 'heat', 'temperature', 'optics',
    'light', 'electromagnetism', 'induction',
    'electronics', 'logic gates', 'radioactivity',
  ],
};

// ── Keyword → Topic alias map ─────────────────────────────────────────────
// Maps what users type (chip labels / partial words) → actual topic title
// Optimized for high-match keywords
const Map<String, String> _keywordAliases = {
  // Grade 9
  'simple machines'   : 'simple machines',
  'pressure'          : 'pressure',
  'density'           : 'density',
  'archimedes'        : 'archimedes',
  'work & energy'     : 'work, energy',
  'work'              : 'work, energy',
  'energy'            : 'work, energy',
  'power'             : 'work, energy',
  'static electricity': 'static electricity',
  'moments'           : 'turning effect',
  'friction'          : 'friction',

  // Grade 10 – Motion (high-match keywords)
  'motion'            : 'motion in a straight line',
  'kinematics'        : 'motion in a straight line',
  'graphs'            : 'motion in a straight line',

  // Grade 10 – Forces (high-match keywords)
  'force'             : 'resultant force',
  'newton'            : "newton's laws of motion",
  'friction'          : 'friction',
  'equilibrium'       : 'equilibrium of forces',

  // Grade 10 – Energy & Work
  'energy'            : 'work, energy and power',
  'work'              : 'work, energy and power',
  'power'             : 'work, energy and power',

  // Grade 10 – Electricity & Others
  'electricity'       : 'current electricity',
  'pressure'          : 'hydrostatic pressure and its applications',
  'vectors'           : 'vectors',
  'moments'           : 'turning effect of a force',

  // Grade 11 – High-match keywords
  'waves'             : 'waves and their applications',
  'heat'              : 'heat & temperature changes',
  'temperature'       : 'heat & temperature changes',
  'optics'            : 'light and optics',
  'light'             : 'light and optics',
  'electromagnetism'  : 'electromagnetism & induction',
  'induction'         : 'electromagnetism & induction',
  'electronics'       : 'electronics & logic gates',
  'logic gates'       : 'electronics & logic gates',
  'radioactivity'     : 'radioactivity',
};

List<SearchItem> searchItems(String query, String grade) {
  final q = query.trim().toLowerCase();
  if (q.isEmpty) return [];

  return allSearchItems.where((item) {
    // Must match the user's grade
    if (item.grade != grade) return false;

    final titleLower = item.title.toLowerCase();

    // 1. Direct title / path / type match
    if (titleLower.contains(q) ||
        item.path.toLowerCase().contains(q) ||
        item.type.toLowerCase().contains(q)) {
      return true;
    }

    // 2. Alias match: look up the query (or each word of it) in the alias map
    //    and check if the resolved topic fragment is in the item's title.
    final resolvedTopic = _keywordAliases[q];
    if (resolvedTopic != null && titleLower.contains(resolvedTopic)) {
      return true;
    }

    // 3. Word-level partial alias match for multi-word queries
    final words = q.split(RegExp(r'\s+'));
    for (final word in words) {
      if (word.length < 3) continue; // skip tiny words
      if (titleLower.contains(word)) return true;
      final resolved = _keywordAliases[word];
      if (resolved != null && titleLower.contains(resolved)) return true;
    }

    return false;
  }).toList();
}