# Phases 3-4: Integration and Firestore Connection

## Phase 3: Dashboard Integration

### Step 1: Update pubspec.yaml

Add required dependencies:

```yaml
dependencies:
  flutter:
    sdk: flutter
  firebase_core: ^latest
  cloud_firestore: ^latest
  firebase_auth: ^latest
  speech_to_text: ^6.0.0  # For voice search
```

### Step 2: Update main.dart Routes

Add the new search routes:

```dart
routes: {
  // ... existing routes ...
  "/search": (context) => const EnhancedSearchPage(),
  "/search-results": (context) => const SearchResultsScreen(
    query: '',
    category: 'All',
    grade: 'Grade 9',
  ),
},
```

### Step 3: Create Enhanced Search Page

```dart
// lib/features/dashboard/screens/search_page.dart
import 'package:flutter/material.dart';
import '../services/integrated_search_service.dart';
import '../enhanced_search_integration.dart';
import '../models/search_models.dart';

class EnhancedSearchPage extends StatefulWidget {
  final String? initialGrade;

  const EnhancedSearchPage({
    Key? key,
    this.initialGrade,
  }) : super(key: key);

  @override
  State<EnhancedSearchPage> createState() => _EnhancedSearchPageState();
}

class _EnhancedSearchPageState extends State<EnhancedSearchPage> {
  late IntegratedSearchService _searchService;
  late String _grade;
  bool _isLoading = false;
  List<SearchResult> _results = [];
  String _currentQuery = '';
  String _selectedCategory = 'All';

  @override
  void initState() {
    super.initState();
    _searchService = IntegratedSearchService();
    _grade = widget.initialGrade ?? 'Grade 10';
  }

  Future<void> _performSearch(String query, String category) async {
    setState(() {
      _isLoading = true;
      _currentQuery = query;
      _selectedCategory = category;
    });

    try {
      final results = await _searchService.search(
        query: query,
        grade: _grade,
        category: category,
        useCache: true,
      );

      setState(() {
        _results = results;
        _isLoading = false;
      });
    } catch (e) {
      print('Error: $e');
      setState(() => _isLoading = false);
      
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Error: $e')),
        );
      }
    }
  }

  void _handleResultTap(SearchResult result) {
    // Route based on result type
    switch (result.type) {
      case 'Lesson':
        Navigator.pushNamed(
          context,
          '/lesson-list',
          arguments: {'lessonId': result.id, 'grade': _grade},
        );
        break;
      case 'Game':
        Navigator.pushNamed(context, '/${result.id}');
        break;
      case 'Lab':
        Navigator.pushNamed(
          context,
          '/practical-home',
          arguments: {'labId': result.id},
        );
        break;
      case 'Quiz':
        Navigator.pushNamed(
          context,
          '/lesson-quizzes',
          arguments: {'quizId': result.id},
        );
        break;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          color: const Color(0xFF1A1A2E),
          onPressed: () => Navigator.pop(context),
        ),
        title: const Text(
          'Search',
          style: TextStyle(
            color: Color(0xFF1A1A2E),
            fontWeight: FontWeight.w600,
          ),
        ),
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            children: [
              // Search bar with voice
              EnhancedSearchBarWidget(
                searchService: _searchService,
                grade: _grade,
                onSearch: _performSearch,
              ),
              const SizedBox(height: 16),

              // Results or empty state
              Expanded(
                child: SearchResultsWidget(
                  results: _results,
                  isLoading: _isLoading,
                  query: _currentQuery,
                  onResultTap: _handleResultTap,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
```

### Step 4: Integrate Search into Dashboard

Update `physics_lab_home_page.dart`:

```dart
// Add this to the imports
import 'services/integrated_search_service.dart';
import 'enhanced_search_integration.dart';

// In _PhysicsLabHomePageState class
class _PhysicsLabHomePageState extends State<PhysicsLabHomePage> {
  late IntegratedSearchService _searchService;
  
  @override
  void initState() {
    super.initState();
    _searchService = IntegratedSearchService();
    // ... other initialization
  }

  // Replace the old _searchBarWidget() with:
  Widget _searchBarWidget() => GestureDetector(
    onTap: () {
      Navigator.pushNamed(
        context,
        '/search',
        arguments: {'grade': _grade},
      );
    },
    child: Container(
      height: 50,
      decoration: BoxDecoration(
        color: const Color(0xFFEEEFF4),
        borderRadius: BorderRadius.circular(30),
      ),
      padding: const EdgeInsets.symmetric(horizontal: 16),
      child: Row(children: [
        const Icon(Icons.search, color: Color(0xFF6B7280), size: 20),
        const SizedBox(width: 10),
        const Expanded(
          child: Text('Search lessons, quizzes, labs...',
              style: TextStyle(fontSize: 14, color: Color(0xFF6B7280))),
        ),
        const Icon(Icons.mic_none, color: Color(0xFF6B7280), size: 20),
      ]),
    ),
  );
}
```

## Phase 4: Firestore Data Connection

### Step 1: Set Up Firestore Collections

**lessons**
```json
{
  "id": "lesson_001",
  "title": "Newton's Laws of Motion",
  "grade": "Grade 10",
  "topic": "Mechanics",
  "description": "Learn about the three laws of motion",
  "keywords": ["Newton", "force", "motion", "acceleration"],
  "duration": 25,
  "source": "PDF",
  "lastAccessed": timestamp,
  "url": "gs://bucket/lesson_001.pdf"
}
```

**games**
```json
{
  "id": "game_friction",
  "title": "Friction Force Game",
  "grade": "Grade 10",
  "topic": "Mechanics",
  "description": "Interactive game about friction",
  "difficulty": "Medium",
  "duration": 20,
  "lastPlayed": timestamp,
  "route": "/friction-game"
}
```

**virtual_labs**
```json
{
  "id": "lab_pendulum",
  "title": "Simple Pendulum Lab",
  "grade": "Grade 10",
  "topic": "Oscillation",
  "description": "Virtual simulation of pendulum motion",
  "type": "Simulation",
  "lastAccessed": timestamp
}
```

**quizzes**
```json
{
  "id": "quiz_forces",
  "title": "Forces Quiz",
  "grade": "Grade 10",
  "topic": "Mechanics",
  "description": "Test your knowledge on forces",
  "difficulty": "Hard",
  "questions": 20,
  "lastAttempted": timestamp
}
```

### Step 2: Update Search Analytics

**search_analytics**
```json
{
  "id": "auto",
  "query": "friction",
  "grade": "Grade 10",
  "count": 45,
  "category": "Lessons",
  "lastSearched": timestamp
}
```

### Step 3: Add Firestore Security Rules

```
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    // Public read access for content
    match /lessons/{document=**} {
      allow read: if request.auth != null;
    }
    match /games/{document=**} {
      allow read: if request.auth != null;
    }
    match /virtual_labs/{document=**} {
      allow read: if request.auth != null;
    }
    match /quizzes/{document=**} {
      allow read: if request.auth != null;
    }

    // User-specific collections
    match /users/{userId}/recent_searches/{document=**} {
      allow read, write: if request.auth.uid == userId;
    }

    // Analytics - allow reads and writes
    match /search_analytics/{document=**} {
      allow read: if request.auth != null;
      allow write: if request.auth != null;
    }

    // PDF keywords - read-only for students
    match /pdf_keywords/{document=**} {
      allow read: if request.auth != null;
      allow write: if request.auth.token.admin == true;
    }
  }
}
```

### Step 4: Create Firestore Helper Class

```dart
// lib/features/dashboard/services/firestore_init.dart
import 'package:cloud_firestore/cloud_firestore.dart';

class FirestoreInitializer {
  static Future<void> initializeSearchData() async {
    final firestore = FirebaseFirestore.instance;
    
    // Create indexes if needed
    // Note: Composite indexes are created via Firebase Console

    print('Firestore initialized for search');
  }

  static Future<void> seedTestData() async {
    final firestore = FirebaseFirestore.instance;
    final batch = firestore.batch();

    // Example lesson
    batch.set(firestore.collection('lessons').doc('lesson_001'), {
      'title': 'Newton\'s Laws of Motion',
      'grade': 'Grade 10',
      'topic': 'Mechanics',
      'description': 'Learn about the three laws of motion',
      'keywords': ['Newton', 'force', 'motion', 'acceleration'],
      'duration': 25,
      'source': 'PDF',
      'lastAccessed': FieldValue.serverTimestamp(),
    });

    await batch.commit();
    print('Test data seeded');
  }
}
```

### Step 5: Load Data on App Start

```dart
// In main.dart or app initialization
@override
void initState() {
  super.initState();
  _initializeApp();
}

Future<void> _initializeApp() async {
  try {
    // Initialize Firestore search data
    await FirestoreInitializer.initializeSearchData();

    // Pre-warm cache for current user's grade
    final currentUser = FirebaseAuth.instance.currentUser;
    if (currentUser != null) {
      final userDoc = await FirebaseFirestore.instance
          .collection('users')
          .doc(currentUser.uid)
          .get();

      final grade = userDoc.get('grade') ?? 'Grade 10';
      // Cache will be populated on first search
    }
  } catch (e) {
    print('Error initializing: $e');
  }
}
```

### Step 6: Handle Pagination (for large result sets)

```dart
// Add to IntegratedSearchService
Future<List<SearchResult>> searchPaginated({
  required String query,
  required String grade,
  int pageSize = 20,
  int pageNumber = 0,
}) async {
  final allResults = await search(
    query: query,
    grade: grade,
  );

  final start = pageNumber * pageSize;
  final end = (start + pageSize).clamp(0, allResults.length);

  return allResults.sublist(start, end);
}
```

## Deployment Checklist

- [ ] All Firestore collections created
- [ ] Security rules deployed
- [ ] Test data seeded
- [ ] Search routes added to main.dart
- [ ] Dependencies updated in pubspec.yaml
- [ ] Voice search permissions added (Android/iOS)
- [ ] Cache invalidation strategy implemented
- [ ] Error handling in place
- [ ] Analytics tracking enabled
- [ ] Performance tested

## Troubleshooting

**Issue: Search returns no results**
- Check Firestore collections exist and have data
- Verify grade format matches (e.g., "Grade 10" not "10")
- Check security rules allow reads

**Issue: Voice search not working**
- Verify microphone permissions granted
- Check speech_to_text package initialization
- Test on device (not simulator)

**Issue: Slow search performance**
- Enable caching
- Check Firestore indexes
- Reduce result limit if needed
- Profile with Dart DevTools

## Next Steps

1. Deploy to Firebase
2. Enable Firestore indexes
3. Test with real data
4. Monitor performance metrics
5. Gather user feedback
6. Iterate on ranking algorithm
