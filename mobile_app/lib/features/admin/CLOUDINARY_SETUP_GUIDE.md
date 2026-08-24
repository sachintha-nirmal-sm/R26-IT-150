# Cloudinary Setup Guide - Lesson Materials Upload

## Overview
This guide explains how to set up Cloudinary for uploading and serving lesson materials to students.

## Step 1: Create Cloudinary Account

1. Go to [cloudinary.com](https://cloudinary.com)
2. Sign up for a free account
3. Verify your email
4. Log in to your dashboard

## Step 2: Get Your Credentials

In the Cloudinary Dashboard:

1. **Cloud Name**: Copy from the top of your dashboard
   - Format: `your-cloud-name`
   - You'll see it in the API environment variable

2. **API Key & Secret**: Found in Settings → API Keys
   - **API Key**: Used for authentication
   - **API Secret**: Keep this private!

3. **Upload Preset**: Create one for unsigned uploads
   - Go to Settings → Upload
   - Under "Upload presets", click "Add upload preset"
   - Set name: `physics_lab_materials`
   - Set Mode: `Unsigned`
   - Save

## Step 3: Configure Flutter App

Update `cloudinary_service.dart` with your credentials:

```dart
class CloudinaryService {
  static const String cloudName = 'YOUR_CLOUD_NAME';
  static const String uploadPreset = 'physics_lab_materials';
  static const String uploadUrl = 'https://api.cloudinary.com/v1_1/$cloudName/auto/upload';
  
  // ... rest of code
}
```

**Example:**
```dart
static const String cloudName = 'demo-physics-lab';
static const String uploadPreset = 'physics_lab_materials';
```

## Step 4: Add to main.dart Routes

```dart
routes: {
  // ... existing routes ...
  "/admin/upload-materials": (context) => const UploadMaterialsScreen(),
  "/student/materials": (context) => const StudentMaterialsScreen(
    grade: 'Grade 10',
  ),
},
```

## Step 5: Firestore Setup

Create a collection called `lesson_materials` with this structure:

```
lesson_materials/
├── docId1/
│   ├── lessonId: "lesson_001"
│   ├── lessonTitle: "Newton's Laws"
│   ├── materialName: "Lesson Slides"
│   ├── materialType: "pdf"
│   ├── grade: "Grade 10"
│   ├── topic: "Mechanics"
│   ├── cloudinaryUrl: "https://res.cloudinary.com/..."
│   ├── cloudinaryPublicId: "physics_lab/material_001"
│   ├── fileSizeBytes: 2048576
│   ├── uploadedBy: "admin_uid_123"
│   ├── uploadedAt: timestamp
│   ├── description: "Complete slides for Newton's Laws"
│   └── downloadCount: 45
```

## Step 6: Security Rules (Firestore)

Add these rules for materials collection:

```
match /lesson_materials/{document=**} {
  allow read: if request.auth != null;
  allow create, update, delete: if request.auth.token.admin == true;
}
```

## Features Supported

### File Types:
- 📄 **PDF**: `.pdf`
- 🖼️ **Images**: `.jpg, .jpeg, .png, .gif, .webp`
- 🎥 **Videos**: `.mp4, .avi, .mov, .mkv`
- 📝 **Documents**: `.doc, .docx`
- 📊 **Presentations**: `.ppt, .pptx`

### Upload Limits:
- Free tier: 10GB/month storage
- Max file size: Limited by Cloudinary plan
- Recommended: Keep files under 100MB

## Admin Panel Usage

### Upload Materials:
1. Navigate to `/admin/upload-materials`
2. Fill in lesson details
3. Select file
4. Choose grade and topic
5. Add description
6. Click "Upload Material"

### View uploaded materials:
- Access Cloudinary dashboard
- Go to Media Library → All files
- Files organized in `physics_lab/{grade}` folders

## Student Access

### View Materials:
1. Students navigate to materials section
2. See all materials for their grade
3. Filter by type (PDF, Images, Videos, etc.)
4. Click to download/view
5. File opens in browser or download starts

### Features:
- ✅ Search by name or description
- ✅ Filter by material type
- ✅ Download count tracking
- ✅ Upload date display
- ✅ Lesson association

## API Integration

### Upload Flow:
```
File Selection
    ↓
File Picker
    ↓
Cloudinary Upload (multipart/form-data)
    ↓
Get Cloudinary URL & Public ID
    ↓
Save Metadata to Firestore
    ↓
Student Access
```

### Download Flow:
```
Student clicks material
    ↓
Increment download count
    ↓
Open Cloudinary URL in browser
    ↓
File downloads/displays
```

## Testing

### Test Upload:
```dart
// Test with a small PDF file
File testFile = File('path/to/test.pdf');
final result = await CloudinaryService.uploadFile(
  file: testFile,
  fileName: 'test_material.pdf',
  folder: 'Grade 10',
);

print('URL: ${result['url']}');
print('Success: ${result['success']}');
```

### Test Retrieval:
```dart
final materials = await MaterialsService().getMaterialsForGrade('Grade 10');
for (final material in materials) {
  print('${material.materialName}: ${material.cloudinaryUrl}');
}
```

## Troubleshooting

### "Upload failed" error:
- Check internet connection
- Verify Cloudinary credentials
- Check file size (< 100MB recommended)
- Verify file format is supported

### File not appearing in student view:
- Check Firestore permissions
- Verify material was saved to database
- Check grade field matches student's grade

### Cloudinary URL returns 404:
- Verify public ID is correct
- Check file was successfully uploaded
- Try accessing URL directly in browser

### Out of storage:
- Upgrade Cloudinary plan
- Delete old materials (Cloudinary dashboard)
- Compress large files before upload

## Best Practices

1. **File Organization**:
   - Use meaningful names: `Grade_10_Mechanics_Slides.pdf`
   - Organize in Cloudinary by grade/topic
   - Keep descriptions detailed

2. **File Formats**:
   - Compress PDFs before upload
   - Use WebP for images
   - MP4 recommended for videos

3. **Metadata**:
   - Always add lesson association
   - Include description for search
   - Set correct grade/topic

4. **Performance**:
   - Limit to reasonable file sizes
   - Use appropriate format for content
   - Consider compression for large files

## Limits & Quotas

**Free Tier:**
- 10GB total storage
- 20GB/month bandwidth
- Unlimited uploads per month
- Images auto-optimized

**Paid Tiers:**
- Additional storage and bandwidth
- Advanced transformations
- Priority support

## Support

- 📖 [Cloudinary Docs](https://cloudinary.com/documentation)
- 🐛 [Cloudinary Support](https://support.cloudinary.com)
- 💬 [Community Forum](https://support.cloudinary.com/hc/en-us)

## Security Notes

⚠️ **Important:**
- Never commit Cloudinary credentials to git
- Use environment variables for production
- Keep API Secret private
- Review Firestore security rules
- Use unsigned upload presets for client-side uploads
