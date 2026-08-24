import 'package:cloud_firestore/cloud_firestore.dart';

class LessonMaterial {
  final String id;
  final String lessonId;
  final String lessonTitle;
  final String materialName;
  final String materialType; // pdf, image, video, doc, etc
  final String grade; // Grade 9, Grade 10, Grade 11
  final String topic;
  final String cloudinaryUrl;
  final String cloudinaryPublicId;
  final int fileSizeBytes;
  final String uploadedBy; // admin user ID
  final DateTime uploadedAt;
  final String description;
  final int downloadCount;

  LessonMaterial({
    required this.id,
    required this.lessonId,
    required this.lessonTitle,
    required this.materialName,
    required this.materialType,
    required this.grade,
    required this.topic,
    required this.cloudinaryUrl,
    required this.cloudinaryPublicId,
    required this.fileSizeBytes,
    required this.uploadedBy,
    required this.uploadedAt,
    required this.description,
    this.downloadCount = 0,
  });

  // Convert to Firestore document
  Map<String, dynamic> toMap() {
    return {
      'lessonId': lessonId,
      'lessonTitle': lessonTitle,
      'materialName': materialName,
      'materialType': materialType,
      'grade': grade,
      'topic': topic,
      'cloudinaryUrl': cloudinaryUrl,
      'cloudinaryPublicId': cloudinaryPublicId,
      'fileSizeBytes': fileSizeBytes,
      'uploadedBy': uploadedBy,
      'uploadedAt': uploadedAt,
      'description': description,
      'downloadCount': downloadCount,
    };
  }

  // Create from Firestore document
  factory LessonMaterial.fromMap(String id, Map<String, dynamic> data) {
    return LessonMaterial(
      id: id,
      lessonId: data['lessonId'] ?? '',
      lessonTitle: data['lessonTitle'] ?? '',
      materialName: data['materialName'] ?? '',
      materialType: data['materialType'] ?? 'pdf',
      grade: data['grade'] ?? 'Grade 10',
      topic: data['topic'] ?? '',
      cloudinaryUrl: data['cloudinaryUrl'] ?? '',
      cloudinaryPublicId: data['cloudinaryPublicId'] ?? '',
      fileSizeBytes: data['fileSizeBytes'] ?? 0,
      uploadedBy: data['uploadedBy'] ?? '',
      uploadedAt: data['uploadedAt'] is Timestamp
          ? (data['uploadedAt'] as Timestamp).toDate()
          : DateTime.now(),
      description: data['description'] ?? '',
      downloadCount: data['downloadCount'] ?? 0,
    );
  }

  // Get file size in readable format
  String getFileSizeString() {
    if (fileSizeBytes < 1024) return '$fileSizeBytes B';
    if (fileSizeBytes < 1024 * 1024) return '${(fileSizeBytes / 1024).toStringAsFixed(2)} KB';
    return '${(fileSizeBytes / (1024 * 1024)).toStringAsFixed(2)} MB';
  }

  // Get icon based on material type
  String getIcon() {
    switch (materialType.toLowerCase()) {
      case 'pdf':
        return '📄';
      case 'image':
      case 'jpg':
      case 'png':
        return '🖼️';
      case 'video':
        return '🎥';
      case 'doc':
      case 'docx':
        return '📝';
      case 'ppt':
      case 'pptx':
        return '📊';
      default:
        return '📁';
    }
  }
}
