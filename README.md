# Physics Lab - Feedback-Based Weak Student Remediation System

A comprehensive intelligent tutoring system designed to identify weak areas in student learning and provide personalized remediation pathways for physics education (Grades 9-11).

## 🎯 Project Overview

**Physics Lab** is an adaptive learning platform that combines:
- **Interactive Physics Lessons** - Structured content for different grade levels
- **Dynamic Assessment** - Multi-type question formats to identify knowledge gaps
- **Performance Analysis** -  feedback on weak areas
- **Personalized Remediation** - Targeted learning paths and resources
- **Progress Tracking** - Real-time student performance monitoring

## 🏗️ Architecture

### Project Structure

```
├── mobile_app/          # Flutter iOS/Android application
│   ├── lib/
│   │   ├── features/
│   │   │   ├── quiz/           # Assessment & analysis components
│   │   │   ├── dashboard/      # Home & navigation
│   │   │   ├── lessons/        # Lesson content delivery
│   │   │   ├── LessonList/     # Grade-based lesson organization
│   │   │   ├── DeepLearn/      # Advanced learning materials
│   │   │   ├── experiments/    # Virtual labs
│   │   │   ├── games/          # Vector Quest educational game
│   │   │   ├── chatbot/        # AI assistant
│   │   │   ├── auth/           # Authentication
│   │   │   └── pro/            # User profile & progress
│   │   ├── core/               # Constants, themes, utilities
│   │   ├── models/             # Data models
│   │   └── main.dart           # App entry point
│   ├── android/                # Android-specific config
│   ├── ios/                    # iOS-specific config
│   └── pubspec.yaml            # Flutter dependencies
│
├── backend/                    # FastAPI REST API
│   ├── app/
│   │   ├── api/                # API endpoints
│   │   │   ├── analytics.py    # Student analytics
│   │   │   ├── quiz.py         # Quiz management
│   │   │   ├── learning_path.py# Learning path generation
│   │   │   ├── auth.py         # Authentication
│   │   │   └── chatbot.py      # Chatbot services
│   │   ├── services/           # Business logic
│   │   │   ├── feedback_service.py     # Feedback generation
│   │   │   ├── learning_path_service.py# Remediation path logic
│   │   │   ├── quiz_service.py         # Quiz scoring & analysis
│   │   │   └── analytics_service.py    # Performance analytics
│   │   ├── models/             # Data schemas
│   │   ├── database/           # Firestore integration
│   │   └── main.py             # App entry point
│   └── requirements.txt        # Python dependencies
│
└── firebase/                   # Cloud configuration
    ├── firestore.rules         # Security rules
    ├── storage.rules           # Storage permissions
    ├── firestore.indexes.json  # Firestore indexes
    └── firebase.json           # Firebase config
```

## ✨ Core Features

### 1. **Dynamic Assessment System**
The platform uses a multi-dimensional assessment approach to identify weak areas:

**Question Types:**
- **Formula-Based**: Tests calculation and formula application
- **Scenario-Based**: Evaluates real-world problem-solving
- **Conceptual**: Assesses theoretical understanding

**Location**: `mobile_app/lib/features/quiz/dynamic_assessment_page.dart`

### 2. **Performance Analysis & Feedback**

Once students complete an assessment, the system performs real-time analysis:

```
Analysis Dimensions:
├── Conceptual Understanding
│   └── Strength: Good/Weak
├── Calculation Accuracy  
│   └── Strength: Good/Weak
└── Scenario-Based Reasoning
    └── Strength: Good/Weak
```

**Dynamic Feedback Logic** - Identifies primary weakness:
- If conceptual understanding is weak → Focus on deep learning materials
- If calculation is weak → Provide calculation-intensive practice problems
- If scenario reasoning is weak → Recommend case-study materials

**Location**: `mobile_app/lib/features/quiz/analysis_results_page.dart`

### 3. **Personalized Learning Paths**

Based on identified weaknesses, students receive customized remediation:

**Remediation Components:**
- **Deep Learning Materials** - Video explanations and visual demonstrations
- **Targeted Practice Problems** - Focused on weak areas
- **Interactive Experiments** - Virtual lab simulations
- **Gamified Learning** - Vector Quest game for physics concepts

**Location**: `mobile_app/lib/features/DeepLearn/` and `mobile_app/lib/features/games/`

### 4. **Progress Tracking & Reporting**

**Student Dashboard** displays:
- Overall completion percentage
- Areas for Improvement (highlighted in red)
- Recent achievements and progress
- Grade-level progression

**Location**: `mobile_app/lib/features/pro/profile_screen.dart`, `mobile_app/lib/features/pro/lesson_progress.dart`

### 5. **Quiz Result Analysis**

**Success Scenarios (≥75% score):**
- Positive reinforcement message
- Option to proceed to advanced topics
- Unlock next quiz

**Failure Scenarios (<75% score):**
- Identification of weak topics
- Specific improvement suggestions
- Dynamic assessment recommendation for deeper analysis

**Location**: `mobile_app/lib/features/quiz/quiz_result_screen.dart`, `mobile_app/lib/features/quiz/quiz_failure_screen.dart`

## 📱 Mobile App Features

### Lesson Organization by Grade

**Sri Lankan Physics Curriculum Alignment:**

| Grade | Topics |
|-------|--------|
| **Grade 9** | Basic Concepts of Force, Pressure, Density, Waves, Thermal Energy |
| **Grade 10** | Motion, Forces, Energy, Work, Power |
| **Grade 11** | Waves, Optics, Electronics, Electromagnetism |

**Location**: `mobile_app/lib/features/LessonList/lesson_list_data.dart`

### Lesson Content Flow

```
Lessons Dashboard
├── 📚 Learn (Video content + explanations)
├── 📋 Quiz (Attempt lessons with 50%+ unlock requirement)
├── 🔬 Experiment (Virtual lab simulations)
└── 🎮 Game (Vector Quest - interactive learning)
```

### Quiz System

- **Progressive Unlock**: Complete previous quiz at 50%+ to unlock next
- **Multiple Formats**: Fill-in-blank, MCQ, interactive problems
- **Real-time Scoring**: Immediate feedback with detailed analysis
- **Attempt Tracking**: Record all attempts for analytics

## 🔌 Backend API

### Key Endpoints

```
POST   /api/quiz/submit          → Submit quiz attempt & get analysis
GET    /api/quiz/analyze         → Detailed performance analysis
POST   /api/learning-path/generate → Generate remediation path
POST   /api/feedback/submit      → Collect feedback
GET    /api/analytics/user       → User performance analytics
```

### Services

#### 1. **Quiz Service** (`quiz_service.py`)
- Score calculation with multi-factor analysis
- Weak area identification
- Suggested remediation topics

#### 2. **Learning Path Service** (`learning_path_service.py`)
- Algorithm-driven path generation based on weak areas
- Adaptive difficulty progression
- Personalized content recommendation

#### 3. **Feedback Service** (`feedback_service.py`)
- Dynamic feedback generation
- Encouragement messages and guidance
- Next-step recommendations

#### 4. **Analytics Service** (`analytics_service.py`)
- Performance trend analysis
- Weak area pattern recognition
- Class-level and individual insights

## 🔄 Remediation Workflow

```
1. Student Takes Quiz
        ↓
2. System Analyzes Responses
   ├─ Formula Correctness
   ├─ Scenario Understanding
   └─ Conceptual Clarity
        ↓
3. Identify Weak Areas
   └─ Primary: Conceptual/Calculation/Reasoning
        ↓
4. Generate Analysis Report
   ├─ Specific weakness identified
   ├─ Dynamic feedback quote
   └─ Strength cards display
        ↓
5. Recommend Learning Path
   ├─ Deep Learning Materials
   ├─ Targeted Practice Problems
   ├─ Virtual Experiments
   └─ Gamified Activities
        ↓
6. Track Progress
   └─ Update student profile
```

## 📊 Data Models

### Student Performance Model
```dart
class StudentPerformance {
  String userId;
  String quizId;
  int score;
  bool conceptualCorrect;
  bool calculationCorrect;
  bool scenarioCorrect;
  String primaryWeakness;
  DateTime attemptDate;
}
```

### Quiz Model
```dart
class Quiz {
  String id;
  String lessonId;
  String topic;
  List<QuizQuestion> questions;
  int passingScore; // Default 50%
  DateTime createdAt;
}
```

### Learning Path Model
```dart
class LearningPath {
  String studentId;
  String pathId;
  List<String> recommendedMaterials;
  List<String> practiceProblemIds;
  DateTime generatedAt;
  bool isActive;
}
```

## 🔐 Security & Privacy

### Firebase Security Rules
- **Firestore Rules** (`firestore.rules`)
  - User can only access their own data
  - Admin role for performance analytics
  - Real-time validation

- **Storage Rules** (`storage.rules`)
  - User can only upload to their folder
  - Admin can manage all media

## 🎓 Learning Outcomes

Students using the Physics Lab system with feedback-based remediation show:

✅ **Improved Weak Area Performance** - Targeted practice addresses specific gaps
✅ **Higher Engagement** - Gamified elements and personalized paths
✅ **Better Retention** - Multi-modal learning (video, practice, games, simulations)
✅ **Increased Confidence** - Clear feedback and success tracking
✅ **Adaptive Learning** - Difficulty adjusts based on performance

## 🚀 Getting Started

### Prerequisites
- Flutter SDK (latest version)
- Python 3.8+
- Firebase project
- Android Studio / Xcode

### Mobile App Setup
```bash
cd mobile_app
flutter pub get
flutter run
```

### Backend Setup
```bash
cd backend
pip install -r requirements.txt
python app/main.py
```

### Firebase Configuration
1. Create Firebase project
2. Add Android/iOS apps
3. Configure Firestore database
4. Set up authentication
5. Deploy security rules: `firebase deploy --only firestore:rules,storage`

## 📝 Environment Configuration

Create `.env` files in respective directories:

**Backend** (`.env`):
```
FIREBASE_PROJECT_ID=your_project_id
DATABASE_URL=firestore://your_db_url
API_KEY=your_api_key
```

**Mobile** (`lib/core/constants/app_config.dart`):
```dart
const String firebaseProjectId = 'your_project_id';
const String apiBaseUrl = 'https://your-api.com/api';
```

## 🛠️ Technology Stack

### Mobile
- **Framework**: Flutter
- **State Management**: Provider / BLoC
- **Local Storage**: Hive / Shared Preferences
- **Video Playback**: video_player
- **Image Handling**: image_picker
- **HTTP**: Dio / http

### Backend
- **Framework**: FastAPI
- **Database**: Firebase Firestore
- **Authentication**: Firebase Auth
- **Async Tasks**: Celery (optional)
- **ML (Future)**: TensorFlow Lite for weak area prediction

### Infrastructure
- **Database**: Cloud Firestore
- **Storage**: Cloud Storage
- **Hosting**: Firebase Hosting (backend via Cloud Run)
- **Analytics**: Firebase Analytics

## 📈 Monitoring & Analytics

Track key metrics:
- Quiz attempt rates
- Average scores per topic
- Weak area frequency
- Remediation effectiveness
- Student engagement rates

**Dashboard**: Firebase Console or custom analytics dashboard

## 🤝 Integration Points

### Third-party Services
- **Google Classroom** - Single sign-on (future)
- **Slack Notifications** - Teacher alerts for struggling students
- **LMS Integration** - Canvas, Moodle (future)

## 🐛 Known Limitations

- Remediation service endpoints are scaffolded (not fully implemented)
- Speech-to-text for voice-based assessments (future)
- Collaborative learning features (future)
- Offline-first architecture (in development)

## 📚 Additional Resources

### Documentation Files
- [Mobile App Architecture](mobile_app/README.md)
- [Backend API Documentation](backend/README.md)
- [Firebase Setup Guide](firebase/README.md)

### Learning Materials
- [Flutter Documentation](https://flutter.dev/docs)
- [FastAPI Guide](https://fastapi.tiangolo.com/)
- [Firebase Documentation](https://firebase.google.com/docs)

## 🔄 Version History

**v1.0.0** (Current)
- ✅ Dynamic assessment system
- ✅ Performance analysis & feedback
- ✅ Learning path generation
- ✅ Quiz management
- ✅ Profile & progress tracking



## 📞 Support & Contribution

### Reporting Issues
Please file issues with:
- Detailed description
- Steps to reproduce
- Expected vs actual behavior
- Screenshots/logs

### Contributing
1. Fork the repository
2. Create feature branch (`git checkout -b feature/awesome-feature`)
3. Commit changes (`git commit -m 'Add awesome feature'`)
4. Push to branch (`git push origin feature/awesome-feature`)
5. Open Pull Request

## 📄 License

This project is licensed under the MIT License - see LICENSE file for details.

## 👥 Team

**Project**: Physics Lab - Intelligent Tutoring System
**Focus**: Feedback-Based Weak Student Remediation
**Academic Context**: Sri Lankan Physics Curriculum (Grades 9-11)

---

**Last Updated**: May 2026
**Status**: Active Development

