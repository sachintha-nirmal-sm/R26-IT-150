import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';
import '../models/lesson_material.dart';
import '../services/materials_service.dart';

class AdminMaterialsScreen extends StatefulWidget {
  const AdminMaterialsScreen({Key? key}) : super(key: key);

  @override
  State<AdminMaterialsScreen> createState() => _AdminMaterialsScreenState();
}

class _AdminMaterialsScreenState extends State<AdminMaterialsScreen> {
  late MaterialsService _materialsService;
  String _filterGrade = 'All';
  String _filterTopic = 'All';

  final grades = ['All', 'Grade 9', 'Grade 10', 'Grade 11'];
  final topics = ['All', 'Mechanics', 'Electricity', 'Magnetism', 'Waves', 'Optics', 'Thermodynamics', 'General'];

  @override
  void initState() {
    super.initState();
    _materialsService = MaterialsService();
  }

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Filters
          Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text('Filter Materials', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
                const SizedBox(height: 12),
                Row(
                  children: [
                    Expanded(
                      child: DropdownButton<String>(
                        value: _filterGrade,
                        isExpanded: true,
                        items: grades.map((g) => DropdownMenuItem(value: g, child: Text(g))).toList(),
                        onChanged: (v) => setState(() => _filterGrade = v!),
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: DropdownButton<String>(
                        value: _filterTopic,
                        isExpanded: true,
                        items: topics.map((t) => DropdownMenuItem(value: t, child: Text(t))).toList(),
                        onChanged: (v) => setState(() => _filterTopic = v!),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),

          // Materials list
          FutureBuilder<List<LessonMaterial>>(
            future: _getMaterials(),
            builder: (context, snapshot) {
              if (snapshot.connectionState == ConnectionState.waiting) {
                return const Center(
                  child: Padding(
                    padding: EdgeInsets.all(32),
                    child: CircularProgressIndicator(),
                  ),
                );
              }

              if (snapshot.hasError) {
                return Center(
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Text('Error: ${snapshot.error}'),
                  ),
                );
              }

              final materials = snapshot.data ?? [];
              final filtered = materials.where((m) {
                if (_filterGrade != 'All' && m.grade != _filterGrade) return false;
                if (_filterTopic != 'All' && m.topic != _filterTopic) return false;
                return true;
              }).toList();

              if (filtered.isEmpty) {
                return Center(
                  child: Padding(
                    padding: const EdgeInsets.all(32),
                    child: Text('No materials found', style: TextStyle(color: Colors.grey[600])),
                  ),
                );
              }

              return ListView.builder(
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                padding: const EdgeInsets.all(16),
                itemCount: filtered.length,
                itemBuilder: (context, index) {
                  final material = filtered[index];
                  return _buildMaterialCard(material);
                },
              );
            },
          ),
        ],
      ),
    );
  }

  Future<List<LessonMaterial>> _getMaterials() async {
    return await _materialsService.getAllMaterials(limit: 200);
  }

  Widget _buildMaterialCard(LessonMaterial material) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(color: Colors.black.withOpacity(0.08), blurRadius: 8),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Preview area
          if (material.materialType.toLowerCase() == 'pdf')
            Container(
              height: 200,
              width: double.infinity,
              decoration: BoxDecoration(
                color: Colors.grey[200],
                borderRadius: const BorderRadius.vertical(top: Radius.circular(12)),
              ),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(Icons.picture_as_pdf, size: 64, color: Colors.red[400]),
                  const SizedBox(height: 8),
                  Text('PDF Document', style: TextStyle(color: Colors.grey[600])),
                ],
              ),
            )
          else if (['jpg', 'jpeg', 'png', 'gif', 'webp'].contains(material.materialType.toLowerCase()))
            Container(
              height: 200,
              width: double.infinity,
              decoration: BoxDecoration(
                color: Colors.grey[200],
                borderRadius: const BorderRadius.vertical(top: Radius.circular(12)),
              ),
              child: Image.network(
                material.cloudinaryUrl,
                fit: BoxFit.cover,
                errorBuilder: (c, e, st) => Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(Icons.image_not_supported, size: 48, color: Colors.grey[400]),
                    const SizedBox(height: 8),
                    Text('Image Preview', style: TextStyle(color: Colors.grey[600])),
                  ],
                ),
              ),
            )
          else if (['mp4', 'avi', 'mov', 'mkv'].contains(material.materialType.toLowerCase()))
            Container(
              height: 200,
              width: double.infinity,
              decoration: BoxDecoration(
                color: Colors.black,
                borderRadius: const BorderRadius.vertical(top: Radius.circular(12)),
              ),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(Icons.play_circle_outline, size: 64, color: Colors.grey[400]),
                  const SizedBox(height: 8),
                  Text('Video', style: TextStyle(color: Colors.grey[400])),
                ],
              ),
            )
          else
            Container(
              height: 200,
              width: double.infinity,
              decoration: BoxDecoration(
                color: Colors.grey[200],
                borderRadius: const BorderRadius.vertical(top: Radius.circular(12)),
              ),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(Icons.description_outlined, size: 64, color: Colors.grey[400]),
                  const SizedBox(height: 8),
                  Text('File', style: TextStyle(color: Colors.grey[600])),
                ],
              ),
            ),

          // Material info
          Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
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
                          ),
                          Text(
                            material.lessonTitle,
                            style: TextStyle(fontSize: 12, color: Colors.grey[600]),
                          ),
                        ],
                      ),
                    ),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
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
                const SizedBox(height: 8),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(
                      '${material.grade} • ${material.topic}',
                      style: TextStyle(fontSize: 12, color: Colors.grey[600]),
                    ),
                    Text(
                      material.getFileSizeString(),
                      style: TextStyle(fontSize: 12, color: Colors.grey[600]),
                    ),
                  ],
                ),
                const SizedBox(height: 12),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Row(
                      children: [
                        Icon(Icons.download, size: 16, color: Colors.grey[500]),
                        const SizedBox(width: 4),
                        Text(
                          '${material.downloadCount} downloads',
                          style: TextStyle(fontSize: 11, color: Colors.grey[600]),
                        ),
                      ],
                    ),
                    ElevatedButton.icon(
                      onPressed: () async {
                        if (await canLaunchUrl(Uri.parse(material.cloudinaryUrl))) {
                          await launchUrl(Uri.parse(material.cloudinaryUrl),
                              mode: LaunchMode.externalApplication);
                        }
                      },
                      icon: const Icon(Icons.open_in_new, size: 16),
                      label: const Text('View'),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: const Color(0xFF2196F3),
                        foregroundColor: Colors.white,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
