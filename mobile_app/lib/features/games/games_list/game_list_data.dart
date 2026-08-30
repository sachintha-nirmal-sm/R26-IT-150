class GameItem {
  final String title;
  final String topic;
  final String grade;
  final String duration;
  final String description;
  final String route;
  final String icon;

  const GameItem({
    required this.title,
    required this.topic,
    required this.grade,
    required this.duration,
    required this.description,
    required this.route,
    required this.icon,
  });
}

const List<GameItem> allGames = [
  // ── Grade 9 Games ──
  GameItem(
    title: 'Nano Shield',
    topic: 'Nanotechnology',
    grade: 'Grade 9',
    duration: '15 mins',
    description: 'Protect against nano-scale particles',
    route: '/nano-shield',
    icon: '🛡️',
  ),
  GameItem(
    title: 'Simple Machines Quest',
    topic: 'Simple Machines',
    grade: 'Grade 9',
    duration: '20 mins',
    description: 'Solve puzzles using levers and pulleys',
    route: '/simple-machines-game',
    icon: '⚙️',
  ),
  GameItem(
    title: 'Density Puzzle',
    topic: 'Density',
    grade: 'Grade 9',
    duration: '18 mins',
    description: 'Calculate and match objects by density',
    route: '/density-puzzle',
    icon: '🧊',
  ),

  // ── Grade 10 Games ──
  GameItem(
    title: 'Motion Quest',
    topic: 'Motion in a Straight Line',
    grade: 'Grade 10',
    duration: '20 mins',
    description: 'Navigate through motion scenarios',
    route: '/motion-quest',
    icon: '🚀',
  ),
  GameItem(
    title: "Newton's Laws Challenge",
    topic: "Newton's Laws of Motion",
    grade: 'Grade 10',
    duration: '22 mins',
    description: 'Master Newton\'s three laws',
    route: '/newton-game',
    icon: '⚡',
  ),
  GameItem(
    title: 'Friction Force Game',
    topic: 'Friction',
    grade: 'Grade 10',
    duration: '18 mins',
    description: 'Explore static and dynamic friction',
    route: '/friction-game',
    icon: '🔥',
  ),
  GameItem(
    title: 'Resultant Force Solver',
    topic: 'Resultant Force',
    grade: 'Grade 10',
    duration: '20 mins',
    description: 'Calculate resultant forces',
    route: '/resultant-force',
    icon: '📐',
  ),
  GameItem(
    title: 'Turning Effect Simulator',
    topic: 'Turning Effect of a Force',
    grade: 'Grade 10',
    duration: '20 mins',
    description: 'Balance moments and rotations',
    route: '/turning-effect',
    icon: '🔄',
  ),
  GameItem(
    title: 'Equilibrium Forces',
    topic: 'Equilibrium of Forces',
    grade: 'Grade 10',
    duration: '18 mins',
    description: 'Find forces for equilibrium',
    route: '/equilibrium-forces',
    icon: '⚖️',
  ),
  GameItem(
    title: 'Hydrostatic Pressure',
    topic: 'Hydrostatic Pressure',
    grade: 'Grade 10',
    duration: '20 mins',
    description: 'Explore pressure in liquids',
    route: '/hydrostatic-pressure',
    icon: '💧',
  ),
  GameItem(
    title: 'Work & Power Game',
    topic: 'Work, Energy and Power',
    grade: 'Grade 10',
    duration: '22 mins',
    description: 'Calculate work and power',
    route: '/work-power-game',
    icon: '⚙️',
  ),
  GameItem(
    title: 'Power & Energy Quest',
    topic: 'Power and Energy',
    grade: 'Grade 10',
    duration: '20 mins',
    description: 'Master energy conversion',
    route: '/power-energy-game',
    icon: '🔋',
  ),
  GameItem(
    title: 'Current Electricity Lab',
    topic: 'Current Electricity',
    grade: 'Grade 10',
    duration: '25 mins',
    description: 'Build circuits with Ohm\'s Law',
    route: '/current-electricity-game',
    icon: '⚡',
  ),
  GameItem(
    title: 'Physics Force Simulator',
    topic: 'Forces',
    grade: 'Grade 10',
    duration: '20 mins',
    description: 'Apply forces and observe physics',
    route: '/force-game',
    icon: '💪',
  ),
  GameItem(
    title: 'Pressure Puzzle',
    topic: 'Pressure',
    grade: 'Grade 10',
    duration: '18 mins',
    description: 'Solve pressure puzzles',
    route: '/pressure-puzzle',
    icon: '📊',
  ),
  GameItem(
    title: 'Vector Quest',
    topic: 'Vectors',
    grade: 'Grade 10',
    duration: '20 mins',
    description: 'Solve vector challenges',
    route: '/game-intro',
    icon: '➡️',
  ),

  // ── Grade 11 Games ──
  GameItem(
    title: 'Waves Explorer',
    topic: 'Waves and Their Applications',
    grade: 'Grade 11',
    duration: '22 mins',
    description: 'Understand wave properties',
    route: '/waves-game',
    icon: '〰️',
  ),
  GameItem(
    title: 'Geometrical Optics',
    topic: 'Light and Optics',
    grade: 'Grade 11',
    duration: '20 mins',
    description: 'Master reflection and refraction',
    route: '/geometrical-optics-game',
    icon: '💡',
  ),
  GameItem(
    title: 'Heat & Temperature',
    topic: 'Heat & Temperature Changes',
    grade: 'Grade 11',
    duration: '18 mins',
    description: 'Explore heat transfer',
    route: '/heat-game',
    icon: '🔥',
  ),
  GameItem(
    title: 'Electromagnetism Quest',
    topic: 'Electromagnetism & Induction',
    grade: 'Grade 11',
    duration: '20 mins',
    description: 'Learn about magnetic fields',
    route: '/electromagnetism-game',
    icon: '🧲',
  ),
  GameItem(
    title: 'Electronics & Logic Gates',
    topic: 'Electronics & Logic Gates',
    grade: 'Grade 11',
    duration: '20 mins',
    description: 'Build circuits with logic gates',
    route: '/electronics-game',
    icon: '🔌',
  ),
];

List<GameItem> getGamesForGrade(String grade) {
  return allGames.where((game) => game.grade == grade).toList();
}

const Map<String, String> gradeSubtitles = {
  'Grade 9': 'Build fundamentals with games',
  'Grade 10': 'Master forces and motion',
  'Grade 11': 'Explore advanced physics',
};
