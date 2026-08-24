import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';
import 'package:firebase_auth/firebase_auth.dart';
import '../../admin/models/lesson_material.dart';
import '../../admin/services/materials_service.dart';

class StudentMaterialsScreen extends StatefulWidget {
  final String grade;
  final String? lessonId;

  const StudentMaterialsScreen({
    Key? key,
    required this.grade,
    this.lessonId,
  }) : super(key: key);

  @override
  State<StudentMaterialsScreen> createState() => _StudentMaterialsScreenState();
}

class _StudentMaterialsScreenState extends State<StudentMaterialsScreen> {
  late MaterialsService _materialsService;
  late Future<List<LessonMaterial>> _materialsFuture;

  String _selectedFilter = 'All';
  final filters = ['All', 'PDF', 'Image', 'Video', 'Doc'];

  @override
  void initState() {
    super.initState();
    _materialsService = MaterialsService();
    _loadMaterials();
  }

  void _loadMaterials() {
    if (widget.lessonId != null) {
      _materialsFuture = _materialsService.getMaterialsForLesson(widget.lessonId!);
    } else {
      _materialsFuture = _materialsService.getMaterialsForGrade(widget.grade);
    }
  }

  Future<void> _openMaterial(LessonMaterial material) async {
    try {
      // Increment download count
      await _materialsService.incrementDownloadCount(material.id);

      // Open URL
      if (await canLaunchUrl(Uri.parse(material.cloudinaryUrl))) {
        await launchUrl(
          Uri.parse(material.cloudinaryUrl),
          mode: LaunchMode.externalApplication,
        );
      } else {
        _showError('Could not open material');
      }
    } catch (e) {
      _showError('Error: $e');
    }
  }

  bool _matchesFilter(LessonMaterial material) {
    if (_selectedFilter == 'All') return true;
    return material.materialType.toLowerCase() == _selectedFilter.toLowerCase();
  }

  void _showError(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message), backgroundColor: Colors.red),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.lessonId != null ? 'Lesson Materials' : '${widget.grade} Materials'),
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: Color(0xFF1A1A2E)),
          onPressed: () => Navigator.pop(context),
        ),
      ),
      body: SafeArea(
        child: Column(
          children: [
            // Filter chips
            SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              padding: const EdgeInsets.all(16),
              child: Row(
                children: filters.map((filter) {
                  final isSelected = _selectedFilter == filter;
                  return Padding(
                    padding: const EdgeInsets.only(right: 8),
                    child: FilterChip(
                      label: Text(filter),
                      selected: isSelected,
                      onSelected: (_) {
                        setState(() => _selectedFilter = filter);
                      },
                      backgroundColor: isSelected
                          ? const Color(0xFF2196F3)
                          : Colors.white,
                      labelStyle: TextStyle(
                        color: isSelected
                            ? Colors.white
                            : const Color(0xFF2196F3),
                      ),
                      side: BorderSide(
                        color: isSelected
                            ? const Color(0xFF2196F3)
                            : Colors.grey[300]!,
                      ),
                    ),
                  );
                }).toList(),
              ),
            ),

            // Materials list
            Expanded(
              child: FutureBuilder<List<LessonMaterial>>(
                future: _materialsFuture,
                builder: (context, snapshot) {
                  if (snapshot.connectionState == ConnectionState.waiting) {
                    return const Center(
                      child: CircularProgressIndicator(
                        color: Color(0xFF2196F3),
                      ),
                    );
                  }

                  if (snapshot.hasError) {
                    return Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(Icons.error_outline, size: 48,
                              color: Colors.grey[400]),
                          const SizedBox(height: 12),
                          Text('Error loading materials',
                              style: TextStyle(color: Colors.grey[600])),
                        ],
                      ),
                    );
                  }

                  final allMaterials = snapshot.data ?? [];
                  final filteredMaterials = allMaterials
                      .where(_matchesFilter)
                      .toList();

                  if (filteredMaterials.isEmpty) {
                    return Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(Icons.folder_open_outlined, size: 48,
                              color: Colors.grey[400]),
                          const SizedBox(height: 12),
                          Text('No materials available',
                              style: TextStyle(color: Colors.grey[600])),
                        ],
                      ),
                    );
                  }

                  return ListView.builder(
                    padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
                    itemCount: filteredMaterials.length,
                    itemBuilder: (context, index) {
                      final material = filteredMaterials[index];
                      return _buildMaterialCard(material);
                    },
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildMaterialCard(LessonMaterial material) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.08),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          borderRadius: BorderRadius.circular(12),
          onTap: () => _openMaterial(material),
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Header with icon and type badge
                Row(
                  children: [
                    Text(material.getIcon(), style: const TextStyle(fontSize: 32)),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            material.materialName,
                            style: const TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.w600,
                              color: Color(0xFF1A1A2E),
                            ),
                            maxLines: 2,
                            overflow: TextOverflow.ellipsis,
                          ),
                          const SizedBox(height: 4),
                          Text(
                            material.lessonTitle,
                            style: TextStyle(
                              fontSize: 12,
                              color: Colors.grey[600],
                            ),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ],
                      ),
                    ),
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 8,
                        vertical: 4,
                      ),
                      decoration: BoxDecoration(
                        color: const Color(0xFFE8F1FF),
                        borderRadius: BorderRadius.circular(6),
                      ),
                      child: Text(
                        material.materialType.toUpperCase(),
                        style: const TextStyle(
                          fontSize: 10,
                          fontWeight: FontWeight.w600,
                          color: Color(0xFF2196F3),
                        ),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 12),

                // Description
                if (material.description.isNotEmpty)
                  Column(
                    children: [
                      Text(
                        material.description,
                        style: TextStyle(
                          fontSize: 13,
                          color: Colors.grey[600],
                        ),
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                      ),
                      const SizedBox(height: 12),
                    ],
                  ),

                // Info row: File size, Downloads, Upload date
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Row(
                      children: [
                        Icon(Icons.storage, size: 16,
                            color: Colors.grey[500]),
                        const SizedBox(width: 4),
                        Text(
                          material.getFileSizeString(),
                          style: TextStyle(
                            fontSize: 12,
                            color: Colors.grey[600],
                          ),
                        ),
                      ],
                    ),
                    Row(
                      children: [
                        Icon(Icons.download, size: 16,
                            color: Colors.grey[500]),
                        const SizedBox(width: 4),
                        Text(
                          '${material.downloadCount} downloads',
                          style: TextStyle(
                            fontSize: 12,
                            color: Colors.grey[600],
                          ),
                        ),
                      ],
                    ),
                    Icon(Icons.arrow_forward, size: 18,
                        color: const Color(0xFF2196F3)),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
