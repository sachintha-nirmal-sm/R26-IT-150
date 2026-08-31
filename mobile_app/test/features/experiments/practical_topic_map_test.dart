import 'package:flutter_test/flutter_test.dart';
import 'package:mobile_app/features/LessonList/lesson_list_data.dart';
import 'package:mobile_app/features/experiments/data/practical.dart';

void main() {
  test('each catalogue practical has a unique lessonId and scene', () {
    final lessonIds = <String>{};
    final scenes = <String>{};
    for (final item in LocalPracticals.all) {
      expect(lessonIds.add(item.lessonId), isTrue, reason: item.lessonId);
      expect(scenes.add(item.unitySceneId), isTrue, reason: item.unitySceneId);
      expect(LocalPracticals.sceneById[item.id], item.unitySceneId);
    }
  });

  test('each practical title maps only to itself', () {
    for (final item in LocalPracticals.all) {
      final mapped = LocalPracticals.forTopic(
        title: item.title,
        grade: item.grade,
      );
      expect(mapped?.id, item.id, reason: item.title);
    }
  });

  test('each lesson with a practical opens only that practical', () {
    final used = <String, String>{};
    for (final entry in gradeLessons.entries) {
      final grade = LocalPracticals.parseGrade(entry.key);
      for (final lesson in entry.value) {
        final mapped = LocalPracticals.forTopic(
          practicalId: lesson.practicalId,
          lessonId: lesson.lessonId,
          title: lesson.title,
          grade: grade,
        );
        if (lesson.practicalId == null) {
          expect(
            mapped,
            isNull,
            reason: '${lesson.title} should not reuse another topic lab',
          );
          continue;
        }
        expect(mapped?.id, lesson.practicalId, reason: lesson.title);
        final owner = used[mapped!.id];
        expect(
          owner,
          isNull,
          reason: '${mapped.id} already used by $owner, also ${lesson.title}',
        );
        used[mapped.id] = lesson.title;
      }
    }
  });

  test('wrong Firestore practicalId does not steal another topic', () {
    final friction = LocalPracticals.forTopic(
      practicalId: 'grade9_force_basic',
      lessonId: 'random-firestore-id',
      title: 'Friction',
      grade: 10,
    );
    expect(friction?.id, 'grade10_friction');

    final nano = LocalPracticals.forTopic(
      practicalId: 'grade9_force_basic',
      title: 'Nanotechnology and its Applications',
      grade: 9,
    );
    expect(nano, isNull);
  });

  test('overlapping words do not share one practical', () {
    expect(
      LocalPracticals.forTopic(
        title: 'Reflection and Refraction of Waves',
        grade: 9,
      )?.id,
      'grade9_reflection_prism',
    );
    expect(
      LocalPracticals.forTopic(title: 'Waves and their applications', grade: 11)
          ?.id,
      'grade11_waves',
    );
    expect(
      LocalPracticals.forTopic(title: 'Geometrical Optics', grade: 11)?.id,
      'grade11_geometrical_optics',
    );
    expect(
      LocalPracticals.forTopic(title: 'Work, energy and power', grade: 10)?.id,
      'grade10_work_energy_power',
    );
    expect(
      LocalPracticals.forTopic(
        title: 'Power and Energy of Electric Appliances',
        grade: 11,
      )?.id,
      'grade11_power_appliances',
    );
    expect(
      LocalPracticals.forTopic(title: 'Turning effect of a force', grade: 10)
          ?.id,
      'grade10_turning_effect',
    );
    expect(
      LocalPracticals.forTopic(
        title: 'Basic Concepts Associated with Force',
        grade: 9,
      )?.id,
      'grade9_force_basic',
    );
    expect(
      LocalPracticals.forTopic(title: 'Hydrostatic pressure', grade: 10)?.id,
      'grade10_hydrostatic_pressure',
    );
    expect(
      LocalPracticals.forTopic(title: 'Pressure Exerted by Solid', grade: 9)?.id,
      'grade9_pressure_solid',
    );
    expect(LocalPracticals.forTopic(title: 'Density', grade: 10), isNull);
    expect(
      LocalPracticals.forTopic(title: 'Density', grade: 9)?.id,
      'grade9_density_water',
    );
  });
}
