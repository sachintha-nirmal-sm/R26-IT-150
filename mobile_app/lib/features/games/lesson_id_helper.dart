// Helper to generate lesson IDs based on lesson titles
class LessonIdHelper {
  static String generateLessonId(String lessonTitle) {
    return lessonTitle
        .toLowerCase()
        .replaceAll(RegExp(r'[^a-z0-9]'), '_')
        .replaceAll(RegExp(r'_+'), '_')
        .replaceAll(RegExp(r'^_|_$'), '');
  }

  // Predefined lesson IDs for games
  static const Map<String, String> lessonIds = {
    'Current electricity': 'current_electricity',
    'Electromagnetism & Induction': 'electromagnetism_induction',
    'Electronics & Logic Gates': 'electronics_logic_gates',
    'Geometrical Optics': 'geometrical_optics',
    'Heat & Temperature': 'heat_temperature',
    'Hydrostatic pressure': 'hydrostatic_pressure',
    'Power and Energy': 'power_energy',
    'Waves': 'waves',
    'Work, energy and power': 'work_energy_power',
  };

  static String getLessonId(String lessonTitle) {
    return lessonIds[lessonTitle] ?? generateLessonId(lessonTitle);
  }
}
