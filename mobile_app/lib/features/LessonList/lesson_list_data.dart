// ── Grade-wise Lesson Data for Sri Lankan Physics Curriculum ──

class LessonItem {
  final String title;
  final String grade;
  final String duration;
  final String subtitle;
  final String? practicalId;
  final String? lessonId;

  const LessonItem({
    required this.title,
    required this.grade,
    required this.duration,
    this.subtitle = 'Start Lesson',
    this.practicalId,
    this.lessonId,
  });
}

const Map<String, List<LessonItem>> gradeLessons = {
  'Grade 9': [
    LessonItem(
      title: 'Basic Concepts Associated with Force',
      grade: 'Grade 9',
      duration: '20 mins',
      subtitle: 'Start Lesson',
      lessonId: 'phy-g9-force-doc',
      practicalId: 'grade9_force_basic',
    ),
    LessonItem(
      title: 'Pressure Exerted by Solid',
      grade: 'Grade 9',
      duration: '18 mins',
      subtitle: 'Start Lesson',
      lessonId: 'phy-g9-pressure-solid-doc',
      practicalId: 'grade9_pressure_solid',
    ),
    LessonItem(
      title: 'Density',
      grade: 'Grade 9',
      duration: '15 mins',
      subtitle: 'Start Lesson',
      lessonId: 'phy-g9-density-doc',
      practicalId: 'grade9_density_water',
    ),
    LessonItem(
      title: 'Reflection and Refraction of Waves',
      grade: 'Grade 9',
      duration: '22 mins',
      subtitle: 'Start Lesson',
      lessonId: 'phy-g9-reflection-doc',
      practicalId: 'grade9_reflection_prism',
    ),
    LessonItem(
      title: 'Simple Machines',
      grade: 'Grade 9',
      duration: '20 mins',
      subtitle: 'Start Lesson',
      lessonId: 'phy-g9-lever-doc',
      practicalId: 'grade9_lever_15_1',
    ),
    LessonItem(
      title: 'Nanotechnology and its Applications',
      grade: 'Grade 9',
      duration: '18 mins',
      subtitle: 'Start Lesson',
    ),
  ],
  'Grade 10': [
    LessonItem(
      title: 'Density',
      grade: 'Grade 10',
      duration: '15 mins',
      subtitle: 'Start Lesson',
    ),
    LessonItem(
      title: 'Motion in a straight line',
      grade: 'Grade 10',
      duration: '25 mins',
      subtitle: 'Start Lesson',
      lessonId: 'phy-g10-motion-straight-doc',
      practicalId: 'grade10_motion_straight_line',
    ),
    LessonItem(
      title: "Newton's laws of motion",
      grade: 'Grade 10',
      duration: '22 mins',
      subtitle: 'Start Lesson',
      lessonId: 'phy-g10-newtons-laws-doc',
      practicalId: 'grade10_newtons_laws',
    ),
    LessonItem(
      title: 'Friction',
      grade: 'Grade 10',
      duration: '20 mins',
      subtitle: 'Start Lesson',
      lessonId: 'phy-g10-friction-doc',
      practicalId: 'grade10_friction',
    ),
    LessonItem(
      title: 'Resultant force',
      grade: 'Grade 10',
      duration: '18 mins',
      subtitle: 'Start Lesson',
    ),
    LessonItem(
      title: 'Turning effect of a force',
      grade: 'Grade 10',
      duration: '20 mins',
      subtitle: 'Start Lesson',
    ),
    LessonItem(
      title: 'Equilibrium of Forces',
      grade: 'Grade 10',
      duration: '18 mins',
      subtitle: 'Start Lesson',
    ),
    LessonItem(
      title: 'Hydrostatic pressure and its applications',
      grade: 'Grade 10',
      duration: '22 mins',
      subtitle: 'Start Lesson',
      lessonId: 'phy-g10-hydrostatic-doc',
      practicalId: 'grade10_hydrostatic_pressure',
    ),
    LessonItem(
      title: 'Work, energy and power',
      grade: 'Grade 10',
      duration: '20 mins',
      subtitle: 'Start Lesson',
      lessonId: 'phy-g10-work-energy-doc',
      practicalId: 'grade10_work_energy_power',
    ),
    LessonItem(
      title: 'Current electricity',
      grade: 'Grade 10',
      duration: '25 mins',
      subtitle: 'Start Lesson',
      lessonId: 'phy-g10-current-electricity-doc',
      practicalId: 'grade10_current_electricity',
    ),
  ],
  'Grade 11': [
    LessonItem(
      title: 'Waves and their applications',
      grade: 'Grade 11',
      duration: '22 mins',
      subtitle: 'Start Lesson',
    ),
    LessonItem(
      title: 'Geometrical Optics',
      grade: 'Grade 11',
      duration: '20 mins',
      subtitle: 'Start Lesson',
    ),
    LessonItem(
      title: 'Heat',
      grade: 'Grade 11',
      duration: '18 mins',
      subtitle: 'Start Lesson',
    ),
    LessonItem(
      title: 'Power and Energy of Electric Appliances',
      grade: 'Grade 11',
      duration: '20 mins',
      subtitle: 'Start Lesson',
    ),
    LessonItem(
      title: 'Electronics',
      grade: 'Grade 11',
      duration: '22 mins',
      subtitle: 'Start Lesson',
    ),
    LessonItem(
      title: 'Electromagnetism and Electromagnetic Induction',
      grade: 'Grade 11',
      duration: '25 mins',
      subtitle: 'Start Lesson',
    ),
  ],
};

/// Returns the lessons for a given grade. Falls back to Grade 10 if not found.
List<LessonItem> getLessonsForGrade(String grade) {
  return gradeLessons[grade] ?? gradeLessons['Grade 10']!;
}

/// Grade subtitle labels
const Map<String, String> gradeSubtitles = {
  'Grade 9': 'Core Concepts & Foundations',
  'Grade 10': 'Motion, Forces & Energy',
  'Grade 11': 'Waves, Optics & Electronics',
};
