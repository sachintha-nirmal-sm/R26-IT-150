# Optimization and Testing Guide (Phases 5-7)

## Phase 5: Fine-Tuned Ranking Algorithm

### Scoring Weights

The ranking engine uses the following weighted scoring system:

```
Title Matching (40% weight)
├─ Exact match: 100 points
├─ Title starts with: 85 points
├─ Whole word match: 65 points
└─ Partial match: 40 points

Keyword Matching (30% weight)
├─ Exact keyword match: 50 points per keyword
├─ Partial keyword match: 20 points per keyword
└─ Multiple matches bonus: 5 points per extra match

Popularity Scoring (10% weight)
└─ Search count / total searches × 25 (max)

Recency Scoring (5% weight)
├─ Within 30 days: decay function
├─ 1 day old: 15 points
└─ 30 days old: 0 points

Type Preference (5% weight)
├─ Lesson: 24 points (1.2× multiplier)
├─ Lab: 20 points
├─ Game: 18 points (0.9× multiplier)
└─ Quiz: 16 points (0.8× multiplier)

Category Matching (10% weight)
└─ Category contains query: 30 points
```

### Ranking Algorithm Flow

```
1. Calculate Title Match Score
2. Add Keyword Matches
3. Apply Popularity Boost
4. Apply Recency Boost
5. Apply Type Preference
6. Sort Results (descending by score)
7. Apply Diversity Filter (max 5 per type)
8. Return Top 20 Results
```

### Performance Considerations

- **Caching**: Results cached for 1 hour per grade
- **Batch Operations**: Fetch all data types in parallel
- **Lazy Loading**: Don't fetch data until needed
- **Index Optimization**: Use Firestore composite indexes

### Testing the Ranking

```dart
// Test exact match
final score = rankingEngine.calculateRelevanceScore(
  query: 'Newton\'s Laws of Motion',
  item: newtonLesson,
  searchCount: 10,
  totalSearches: 100,
);
// Expected: > 90

// Test partial match
final partialScore = rankingEngine.calculateRelevanceScore(
  query: 'Newton',
  item: newtonLesson,
  searchCount: 10,
  totalSearches: 100,
);
// Expected: 40-85 (depends on query)

// Test popularity
final popularScore = rankingEngine.calculateRelevanceScore(
  query: 'friction',
  item: frictionLesson,
  searchCount: 50,
  totalSearches: 100,
);
// Expected: highPopularity > lowPopularity
```

## Phase 6: Voice Search Implementation

### Architecture

```
User Speech
    ↓
speech_to_text package
    ↓
Audio Processing
    ↓
Text Recognition
    ↓
Search Query
    ↓
Integrated Search Service
    ↓
Results Display
```

### Features

- **Real-time Feedback**: Shows recognized text as user speaks
- **Confidence Level**: Displays confidence percentage (0-100%)
- **Audio Visualization**: Visual feedback during recording
- **Error Handling**: Graceful fallback if speech fails
- **Multi-language**: Can be extended for Sinhala/Tamil

### Usage

```dart
// Simple voice search
showDialog(
  context: context,
  builder: (context) => VoiceSearchDialog(
    onSearch: (result) {
      // result = recognized text
      // Perform search with result
    },
  ),
);

// Or use the widget directly
VoiceSearchWidget(
  onSpeechResult: (text) {
    // Handle recognized speech
  },
  onStartListening: () {
    print('Started listening');
  },
  onStopListening: () {
    print('Stopped listening');
  },
  onError: () {
    print('Error during speech recognition');
  },
)
```

### Permissions Required

**Android (AndroidManifest.xml)**
```xml
<uses-permission android:name="android.permission.RECORD_AUDIO" />
<uses-permission android:name="android.permission.INTERNET" />
```

**iOS (Info.plist)**
```xml
<key>NSMicrophoneUsageDescription</key>
<string>This app needs microphone access for voice search</string>
<key>NSSpeechRecognitionUsageDescription</key>
<string>This app needs speech recognition for voice search</string>
```

### Handling Errors

```dart
// Handle when speech recognition is not available
if (!isInitialized) {
  ScaffoldMessenger.of(context).showSnackBar(
    const SnackBar(
      content: Text('Voice search not available on this device'),
    ),
  );
  return;
}

// Handle speech recognition errors
onError: () {
  ScaffoldMessenger.of(context).showSnackBar(
    const SnackBar(
      content: Text('Error during voice search. Try typing instead.'),
    ),
  );
}
```

## Phase 7: Testing & Optimization

### Unit Tests

**Run tests:**
```bash
flutter test test/features/dashboard/search_service_test.dart
```

**Test Coverage:**
- Relevance scoring algorithm (10 tests)
- Keyword extraction (4 tests)
- Fuzzy matching (2 tests)
- Sorting and filtering (3 tests)

### Widget Tests

```dart
// Test search widget rendering
testWidgets('SmartSearchWidget renders correctly', (WidgetTester tester) async {
  await tester.pumpWidget(
    MaterialApp(
      home: Scaffold(
        body: SmartSearchWidget(
          grade: 'Grade 10',
          onResultTap: (_) {},
        ),
      ),
    ),
  );

  // Verify search field exists
  expect(find.byType(TextField), findsOneWidget);
  
  // Type in search
  await tester.enterText(find.byType(TextField), 'friction');
  await tester.pumpWidget(const SizedBox.expand());
  
  // Verify results appear
  expect(find.byType(ListView), findsWidgets);
});
```

### Performance Benchmarks

**Target Metrics:**
- Search latency: < 500ms
- Voice recognition time: < 3 seconds
- Ranking algorithm: < 100ms for 100 items
- Suggestions display: < 200ms

**Measuring Performance:**
```dart
final stopwatch = Stopwatch()..start();

final results = await searchService.search(
  query: 'friction',
  grade: 'Grade 10',
);

stopwatch.stop();
print('Search took ${stopwatch.elapsedMilliseconds}ms');
```

### Optimization Checklist

- [ ] Cache search results (1 hour TTL)
- [ ] Parallel data fetching for different types
- [ ] Lazy load heavy computations
- [ ] Compress Firestore queries
- [ ] Use pagination for large result sets
- [ ] Implement result limiting (top 20)
- [ ] Debounce autocomplete queries
- [ ] Store voice recognition cache

### Caching Strategy

```dart
// Cache hit example
class SearchCache {
  final Map<String, List<SearchResult>> _cache = {};
  final Map<String, DateTime> _timestamps = {};
  static const Duration cacheTTL = Duration(hours: 1);

  bool isCacheValid(String grade) {
    final timestamp = _timestamps[grade];
    if (timestamp == null) return false;
    
    return DateTime.now().difference(timestamp) < cacheTTL;
  }

  List<SearchResult>? getCache(String grade) {
    if (!isCacheValid(grade)) {
      _cache.remove(grade);
      _timestamps.remove(grade);
      return null;
    }
    return _cache[grade];
  }

  void setCache(String grade, List<SearchResult> results) {
    _cache[grade] = results;
    _timestamps[grade] = DateTime.now();
  }
}
```

### User Testing Recommendations

**Scenario 1: Exact Match**
- User: "Search for Newton's Laws"
- Expected: First result is "Newton's Laws of Motion"
- Metric: Precision = 1.0

**Scenario 2: Typo Correction**
- User: "Search for fricton" (typo)
- Expected: Shows "Did you mean friction?"
- Metric: Recall = 0.9+

**Scenario 3: Voice Search**
- User: Speaks "friction"
- Expected: Recognized text appears, search performs
- Metric: Voice accuracy = 95%+

**Scenario 4: Category Filter**
- User: Filters to "Games" category
- Expected: Only game results shown
- Metric: Filter accuracy = 100%

### Analytics to Track

- **Search Metrics**
  - Total searches per day
  - Queries without results (0 results rate)
  - Average results per query
  - Voice search adoption rate

- **Performance Metrics**
  - Search latency (p50, p95, p99)
  - Voice recognition success rate
  - Cache hit rate

- **User Behavior**
  - Most searched terms
  - Click-through rate (CTR)
  - Time to click result
  - Bounce rate (searcher leaves without clicking)

### Debugging

**Enable debug logging:**
```dart
// In search_service.dart
const bool _enableDebugLogging = true;

void _log(String message) {
  if (_enableDebugLogging) {
    print('[SearchService] $message');
  }
}
```

**Monitor Firestore queries:**
```
Firebase Console → Firestore Database → Monitoring
- Query count
- Read operations
- Network latency
```

## Deployment Checklist

- [ ] All unit tests passing (100% coverage for search services)
- [ ] Integration tests for voice search
- [ ] Performance benchmarks met
- [ ] Analytics instrumentation added
- [ ] Error tracking (Sentry/Firebase Crashlytics)
- [ ] A/B testing infrastructure (Firebase Remote Config)
- [ ] Security rules validated
- [ ] Backup strategy for search history

## Post-Launch Monitoring

1. **First Week**: Monitor crash rates and latency
2. **First Month**: Analyze user search patterns
3. **Ongoing**: Weekly performance review
4. **Quarterly**: Algorithm tuning based on user data

## Future Enhancements

1. **Machine Learning**
   - User preference learning
   - Personalized ranking per student

2. **Advanced Filtering**
   - Difficulty level filtering
   - Time duration filtering
   - Topic-specific search

3. **Trending Insights**
   - Weekly trending topics
   - Difficulty distribution analytics

4. **Multi-language Support**
   - Sinhala search
   - Tamil search
   - English-to-Sinhala translation
