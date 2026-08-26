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
    // Grade 10
    'Motion in a Straight Line': 'motion_straight_line',
    "Newton's Laws of Motion": 'newtons_laws',
    'Friction': 'friction',
    'Resultant Force': 'resultant_force',
    'Turning Effect of a Force': 'turning_effect',
    'Equilibrium of Forces': 'equilibrium_forces',
    'Forces': 'forces',
    'Hydrostatic Pressure': 'hydrostatic_pressure',
    'Hydrostatic pressure': 'hydrostatic_pressure',
    'Pressure': 'pressure',
    'Work, Energy and Power': 'work_energy_power',
    'Work, energy and power': 'work_energy_power',
    'Power and Energy': 'power_energy',
    'Current Electricity': 'current_electricity',
    'Current electricity': 'current_electricity',
    'Vectors': 'vectors',

    // Grade 9
    'Nanotechnology': 'nanotechnology',
    'Simple Machines': 'simple_machines',
    'Density': 'density',

    // Grade 11
    'Waves and Their Applications': 'waves',
    'Waves': 'waves',
    'Light and Optics': 'geometrical_optics',
    'Geometrical Optics': 'geometrical_optics',
    'Heat & Temperature Changes': 'heat_temperature',
    'Heat & Temperature': 'heat_temperature',
    'Electromagnetism & Induction': 'electromagnetism_induction',
    'Electronics & Logic Gates': 'electronics_logic_gates',
  };

  static String getLessonId(String lessonTitle) {
    return lessonIds[lessonTitle] ?? generateLessonId(lessonTitle);
  }
}
