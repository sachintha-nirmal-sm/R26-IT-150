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
const Map<String, List<String>> gradeKeywords = {
  'Grade 9': [
    'pressure', 'archimedes', 'work & energy',
    'static electricity', 'moments', 'friction',
  ],
  'Grade 10': [
    'distance',
    'displacement',
    'speed',
    'velocity',
    'acceleration',
    'displacement time graph',
    'velocity time graph',
    'gravitational acceleration',

    // Forces
    'force',
    'effects of force',
    'momentum',
    'mass',
    'weight',
    'newton laws',
    'newtons laws of motion',

    // Friction
    'friction',
    'static friction',
    'dynamic friction',
    'limiting friction',
    'frictional force',

    // Resultant forces
    'resultant force',
    'collinear forces',
    'parallel forces',
    'inclined forces',
    'equilibrium',

    // Light / electricity
    'reflection',
    'refraction',
    'current electricity',
    'ohm law',
  ],
  'Grade 11': [
    'specific heat', 'logic gates', 'electromagnetic induction',
    'waves', 'radioactivity', 'electronics',
  ],
};

// ── Keyword → Topic alias map ─────────────────────────────────────────────
// Maps what users type (chip labels / partial words) → actual topic title
// fragments stored in allSearchItems.
const Map<String, String> _keywordAliases = {
  // Grade 9
  'pressure'          : 'force and pressure',
  'archimedes'        : "archimedes' principle",
  'work & energy'     : 'work, energy',
  'work'              : 'work, energy',
  'energy'            : 'work, energy',
  'power'             : 'work, energy',
  'static electricity': 'static electricity',
  'moments'           : 'turning effect',
  'friction'          : 'friction',

  // Grade 10 – Motion
  'distance'                : 'motion in a straight line',
  'displacement'            : 'motion in a straight line',
  'speed'                   : 'motion in a straight line',
  'velocity'                : 'motion in a straight line',
  'acceleration'            : 'motion in a straight line',
  'displacement time graph' : 'motion in a straight line',
  'velocity time graph'     : 'motion in a straight line',
  'gravitational acceleration': 'motion in a straight line',

  // Grade 10 – Forces
  'force'                 : 'resultant force',
  'effects of force'      : 'resultant force',
  'momentum'              : "newton's laws of motion",
  'mass'                  : "newton's laws of motion",
  'weight'                : "newton's laws of motion",
  'newton laws'           : "newton's laws of motion",
  'newtons laws of motion': "newton's laws of motion",

  // Grade 10 – Friction
  'static friction'   : 'friction',
  'dynamic friction'  : 'friction',
  'limiting friction' : 'friction',
  'frictional force'  : 'friction',

  // Grade 10 – Resultant
  'resultant force' : 'resultant force',
  'collinear forces': 'resultant force',
  'parallel forces' : 'resultant force',
  'inclined forces' : 'resultant force',
  'equilibrium'     : 'equilibrium of forces',

  // Grade 10 – Light / Electricity
  'reflection'        : 'current electricity',
  'refraction'        : 'current electricity',
  'current electricity': 'current electricity',
  'ohm law'           : 'current electricity',

  // Grade 11
  'heat'                    : 'heat',
  'temperature'             : 'heat',
  'specific heat capacity'  : 'specific heat',
  'specific heat'           : 'specific heat',
  'waves'                   : 'waves',
  'wave applications'       : 'waves',
  'electronics'             : 'electronics',
  'logic gates'             : 'electronics',
  'electromagnetism'        : 'electromagnetism',
  'electromagnetic induction': 'electromagnetism',
  'radioactivity'           : 'radioactivity',
  'nuclear energy'          : 'radioactivity',
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