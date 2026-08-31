import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:firebase_auth/firebase_auth.dart';
import '../models/lesson_material.dart';

class MaterialsService {
  static const String materialsCollection = 'lesson_materials';
  final FirebaseFirestore _firestore = FirebaseFirestore.instance;
  final FirebaseAuth _auth = FirebaseAuth.instance;

  /// Save material metadata to Firestore
  Future<String?> saveMaterial(LessonMaterial material) async {
    try {
      final document = await _firestore
          .collection(materialsCollection)
          .add(material.toMap());
      return document.id;
    } catch (e) {
      print('Error saving material: $e');
      return null;
    }
  }

  /// Get all materials for a lesson (with grade and title fallback for Grades 9, 10, 11)
  Future<List<LessonMaterial>> getMaterialsForLesson(
    String lessonId, {
    String? grade,
    String? lessonTitle,
  }) async {
    try {
      final docsMap = <String, LessonMaterial>{};

      // 1. Check by exact lessonId
      if (lessonId.isNotEmpty) {
        final snap1 = await _firestore
            .collection(materialsCollection)
            .where('lessonId', isEqualTo: lessonId)
            .get();
        for (final doc in snap1.docs) {
          docsMap[doc.id] = LessonMaterial.fromMap(doc.id, doc.data());
        }
      }

      // 2. Check by lessonTitle if provided
      if (lessonTitle != null && lessonTitle.isNotEmpty) {
        final snap2 = await _firestore
            .collection(materialsCollection)
            .where('lessonTitle', isEqualTo: lessonTitle)
            .get();
        for (final doc in snap2.docs) {
          docsMap[doc.id] = LessonMaterial.fromMap(doc.id, doc.data());
        }
      }

      // 3. Fallback: if no materials found for this specific lesson, retrieve all materials for this grade
      if (docsMap.isEmpty && grade != null && grade.isNotEmpty) {
        String cleanGrade = grade;
        if (grade.contains('Grade 9')) {
          cleanGrade = 'Grade 9';
        } else if (grade.contains('Grade 10')) {
          cleanGrade = 'Grade 10';
        } else if (grade.contains('Grade 11')) {
          cleanGrade = 'Grade 11';
        }

        final snap3 = await _firestore
            .collection(materialsCollection)
            .where('grade', isEqualTo: cleanGrade)
            .get();
        for (final doc in snap3.docs) {
          docsMap[doc.id] = LessonMaterial.fromMap(doc.id, doc.data());
        }
      }

      final results = docsMap.values.toList();
      results.sort((a, b) => b.uploadedAt.compareTo(a.uploadedAt));
      return results;
    } catch (e) {
      print('Error getting lesson materials: $e');
      return [];
    }
  }

  /// Get all materials for a grade
  Future<List<LessonMaterial>> getMaterialsForGrade(String grade) async {
    try {
      final snapshot = await _firestore
          .collection(materialsCollection)
          .where('grade', isEqualTo: grade)
          .limit(100)
          .get();

      final results = snapshot.docs
          .map((doc) => LessonMaterial.fromMap(doc.id, doc.data()))
          .toList();
      results.sort((a, b) => b.uploadedAt.compareTo(a.uploadedAt));
      return results;
    } catch (e) {
      print('Error getting grade materials: $e');
      return [];
    }
  }

  /// Get materials by topic
  Future<List<LessonMaterial>> getMaterialsByTopic(
    String grade,
    String topic,
  ) async {
    try {
      final snapshot = await _firestore
          .collection(materialsCollection)
          .where('grade', isEqualTo: grade)
          .where('topic', isEqualTo: topic)
          .orderBy('uploadedAt', descending: true)
          .get();

      return snapshot.docs
          .map((doc) => LessonMaterial.fromMap(doc.id, doc.data()))
          .toList();
    } catch (e) {
      print('Error getting topic materials: $e');
      return [];
    }
  }

  /// Get all materials (admin view)
  Future<List<LessonMaterial>> getAllMaterials({int limit = 50}) async {
    try {
      final snapshot = await _firestore
          .collection(materialsCollection)
          .orderBy('uploadedAt', descending: true)
          .limit(limit)
          .get();

      return snapshot.docs
          .map((doc) => LessonMaterial.fromMap(doc.id, doc.data()))
          .toList();
    } catch (e) {
      print('Error getting all materials: $e');
      return [];
    }
  }

  /// Update download count
  Future<void> incrementDownloadCount(String materialId) async {
    try {
      await _firestore.collection(materialsCollection).doc(materialId).update({
        'downloadCount': FieldValue.increment(1),
      });
    } catch (e) {
      print('Error updating download count: $e');
    }
  }

  /// Delete material record (Cloudinary file should be deleted separately)
  Future<bool> deleteMaterial(String materialId) async {
    try {
      await _firestore.collection(materialsCollection).doc(materialId).delete();
      return true;
    } catch (e) {
      print('Error deleting material: $e');
      return false;
    }
  }

  /// Search materials by name or description
  Future<List<LessonMaterial>> searchMaterials(
    String query,
    String grade,
  ) async {
    try {
      // Note: Firestore doesn't support full-text search
      // This fetches all materials and filters locally
      final snapshot = await _firestore
          .collection(materialsCollection)
          .where('grade', isEqualTo: grade)
          .limit(100)
          .get();

      final queryLower = query.toLowerCase();
      return snapshot.docs
          .map((doc) => LessonMaterial.fromMap(doc.id, doc.data()))
          .where((material) =>
              material.materialName.toLowerCase().contains(queryLower) ||
              material.description.toLowerCase().contains(queryLower) ||
              material.lessonTitle.toLowerCase().contains(queryLower))
          .toList();
    } catch (e) {
      print('Error searching materials: $e');
      return [];
    }
  }

  /// Get material by ID
  Future<LessonMaterial?> getMaterialById(String materialId) async {
    try {
      final doc = await _firestore
          .collection(materialsCollection)
          .doc(materialId)
          .get();

      if (doc.exists) {
        return LessonMaterial.fromMap(doc.id, doc.data() ?? {});
      }
      return null;
    } catch (e) {
      print('Error getting material: $e');
      return null;
    }
  }

  /// Get materials uploaded by specific admin
  Future<List<LessonMaterial>> getMaterialsByUploader(String uploaderId) async {
    try {
      final snapshot = await _firestore
          .collection(materialsCollection)
          .where('uploadedBy', isEqualTo: uploaderId)
          .orderBy('uploadedAt', descending: true)
          .get();

      return snapshot.docs
          .map((doc) => LessonMaterial.fromMap(doc.id, doc.data()))
          .toList();
    } catch (e) {
      print('Error getting uploader materials: $e');
      return [];
    }
  }

  /// Get material statistics
  Future<Map<String, int>> getMaterialStats(String grade) async {
    try {
      final snapshot = await _firestore
          .collection(materialsCollection)
          .where('grade', isEqualTo: grade)
          .get();

      final stats = <String, int>{};
      for (final doc in snapshot.docs) {
        final type = doc['materialType'] as String? ?? 'other';
        stats[type] = (stats[type] ?? 0) + 1;
      }

      return stats;
    } catch (e) {
      print('Error getting material stats: $e');
      return {};
    }
  }
}
