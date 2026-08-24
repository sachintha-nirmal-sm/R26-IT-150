import 'dart:io';
import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:path/path.dart' as path;

class CloudinaryService {
  // Cloudinary credentials (from backend .env)
  static const String cloudName = 'qn0ba57q';
  static const String uploadPreset = 'physics_lab'; // Already configured in Cloudinary
  static const String uploadUrl = 'https://api.cloudinary.com/v1_1/$cloudName/raw/upload';

  /// Upload file to Cloudinary (supports both File and bytes)
  /// Returns map with: {success, url, publicId, error}
  static Future<Map<String, dynamic>> uploadFile({
    required dynamic file, // File or Uint8List for web
    required String fileName,
    String? folder,
  }) async {
    try {
      final request = http.MultipartRequest('POST', Uri.parse(uploadUrl));

      // Add file (handle both File and bytes)
      if (file is File) {
        request.files.add(
          await http.MultipartFile.fromPath('file', file.path),
        );
      } else {
        // For web - bytes
        request.files.add(
          http.MultipartFile.fromBytes('file', file, filename: fileName),
        );
      }

      // Add upload preset
      request.fields['upload_preset'] = uploadPreset;

      // Add folder (optional - organizes files in Cloudinary)
      if (folder != null) {
        request.fields['folder'] = 'physics_lab/$folder';
      }

      // Add public ID for easier management
      final publicId = fileName.replaceAll(RegExp(r'[^a-zA-Z0-9_-]'), '_');
      request.fields['public_id'] = publicId;

      // Add tags for filtering
      request.fields['tags'] = 'physics_lab,lesson_material';

      // Send request
      final response = await request.send().timeout(
        const Duration(minutes: 5),
        onTimeout: () {
          throw Exception('Upload timeout - file too large or slow connection');
        },
      );

      final responseBody = await response.stream.bytesToString();
      print('CLOUDINARY RESPONSE [${response.statusCode}]: $responseBody');

      if (response.statusCode == 200) {
        final Map<String, dynamic> data = jsonDecode(responseBody);
        final url = data['secure_url'] ?? data['url'];
        print('CLOUDINARY URL STORED: $url');

        return {
          'success': true,
          'url': url,
          'publicId': data['public_id'],
          'fileSize': data['bytes'],
          'resourceType': data['resource_type'],
        };
      } else {
        final error = jsonDecode(responseBody);
        return {
          'success': false,
          'error': error['error']?['message'] ?? 'Upload failed: ${response.statusCode}',
        };
      }
    } catch (e) {
      return {
        'success': false,
        'error': 'Upload error: $e',
      };
    }
  }

  /// Delete file from Cloudinary
  static Future<bool> deleteFile(String publicId) async {
    try {
      // Note: This requires API key authentication
      // For now, files should be managed via Cloudinary dashboard
      print('Deletion via API requires API key - manage files in Cloudinary dashboard');
      return false;
    } catch (e) {
      print('Error deleting file: $e');
      return false;
    }
  }

  /// Get file type from extension
  static String getFileType(String fileName) {
    final ext = path.extension(fileName).toLowerCase();
    switch (ext) {
      case '.pdf':
        return 'pdf';
      case '.jpg':
      case '.jpeg':
      case '.png':
      case '.gif':
      case '.webp':
        return 'image';
      case '.mp4':
      case '.avi':
      case '.mov':
      case '.mkv':
        return 'video';
      case '.doc':
      case '.docx':
        return 'doc';
      case '.ppt':
      case '.pptx':
        return 'ppt';
      default:
        return 'file';
    }
  }

  /// Check if file is supported
  static bool isSupportedFile(String fileName) {
    final supported = [
      '.pdf', '.jpg', '.jpeg', '.png', '.gif', '.webp',
      '.mp4', '.avi', '.mov', '.mkv',
      '.doc', '.docx', '.ppt', '.pptx',
    ];

    final ext = path.extension(fileName).toLowerCase();
    return supported.contains(ext);
  }

  /// Parse JSON response safely
  static Map<String, dynamic> _parseJsonResponse(String response) {
    try {
      final secureUrlMatch = RegExp(r'"secure_url":"([^"]+)"').firstMatch(response);
      final urlMatch = RegExp(r'"url":"([^"]+)"').firstMatch(response);
      final publicIdMatch = RegExp(r'"public_id":"([^"]+)"').firstMatch(response);
      final bytesMatch = RegExp(r'"bytes":(\d+)').firstMatch(response);
      final typeMatch = RegExp(r'"resource_type":"([^"]+)"').firstMatch(response);

      final resolvedUrl = secureUrlMatch?.group(1) ?? urlMatch?.group(1);

      return {
        'secure_url': resolvedUrl,
        'url': resolvedUrl,
        'public_id': publicIdMatch?.group(1),
        'bytes': int.tryParse(bytesMatch?.group(1) ?? '0') ?? 0,
        'resource_type': typeMatch?.group(1),
      };
    } catch (e) {
      print('Error parsing response: $e');
      return {};
    }
  }
}
