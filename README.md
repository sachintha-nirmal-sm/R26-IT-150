Game-Based-Measurement-and-Calculation

# Physics Lab - Feedback-Based Weak Student Remediation System

A comprehensive intelligent tutoring system designed to identify weak areas in student learning and provide personalized remediation pathways for physics education (Grades 9-11).

##  Project Overview

**Physics Lab** is an adaptive learning platform that combines:
- **Interactive Physics Lessons** - Structured content for different grade levels
- **Dynamic Assessment** - Multi-type question formats to identify knowledge gaps
- **Performance Analysis** -  feedback on weak areas
- **Personalized Remediation** - Targeted learning paths and resources
- **Progress Tracking** - Real-time student performance monitoring

##  Architecture

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

##  Core Features

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

##  Mobile App Features

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
├──  Learn (Video content + explanations)
├──  Quiz (Attempt lessons with 50%+ unlock requirement)
├──  Experiment (Virtual lab simulations)
└──  Game (Vector Quest - interactive learning)
```

### Quiz System

- **Progressive Unlock**: Complete previous quiz at 50%+ to unlock next
- **Multiple Formats**: Fill-in-blank, MCQ, interactive problems
- **Real-time Scoring**: Immediate feedback with detailed analysis
- **Attempt Tracking**: Record all attempts for analytics

##  Backend API

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

##  Remediation Workflow

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

##  Data Models

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

##  Security & Privacy

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

 **Improved Weak Area Performance** - Targeted practice addresses specific gaps
 **Higher Engagement** - Gamified elements and personalized paths
 **Better Retention** - Multi-modal learning (video, practice, games, simulations)
 **Increased Confidence** - Clear feedback and success tracking
 **Adaptive Learning** - Difficulty adjusts based on performance

##  Getting Started

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

##  Environment Configuration

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

##  Technology Stack

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

##  Monitoring & Analytics

Track key metrics:
- Quiz attempt rates
- Average scores per topic
- Weak area frequency
- Remediation effectiveness
- Student engagement rates

**Dashboard**: Firebase Console or custom analytics dashboard



##  Known Limitations

- Remediation service endpoints are scaffolded (not fully implemented)
- Speech-to-text for voice-based assessments (future)
- Collaborative learning features (future)
- Offline-first architecture (in development)

##  Additional Resources

### Documentation Files
- [Mobile App Architecture](mobile_app/README.md)
- [Backend API Documentation](backend/README.md)
- [Firebase Setup Guide](firebase/README.md)

### Learning Materials
- [Flutter Documentation](https://flutter.dev/docs)
- [FastAPI Guide](https://fastapi.tiangolo.com/)
- [Firebase Documentation](https://firebase.google.com/docs)

##  Version History

**v1.0.0** (Current)
-  Dynamic assessment system
-  Performance analysis & feedback
-  Learning path generation
-  Quiz management
-  Profile & progress tracking



##  Support & Contribution

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

##  Team

**Project**: Physics Lab - Intelligent Tutoring System
**Focus**: Feedback-Based Weak Student Remediation
**Academic Context**: Sri Lankan Physics Curriculum (Grades 9-11)

---

**Last Updated**: May 2026
**Status**: Active Development

**Mobile-Based Virtual Physics Laboratory with AI Assistance**
 main

# Mobile-Based Virtual Physics Laboratory with AI Assistance and Interactive Learning for Rural Students

## Project Overview
This project is a mobile-based virtual physics laboratory designed for Grade 6–11 students, especially students in rural areas with limited access to physical laboratory facilities.

The system combines:
- Interactive physics games
- Virtual experiments
- Real-time measurement generation
- Step-by-step physics calculations
- AI-assisted learning support

The goal is to improve conceptual understanding, engagement, and practical physics learning through an interactive mobile platform.

---

## Problem Statement
Many students struggle to understand physics because:
- Physics concepts are abstract
- Schools lack laboratory facilities
- Learning is mostly theory-based
- Students have limited practical experience

Existing systems mainly focus on:
- Simulations only
- Quizzes only
- Gamification only

Most platforms do not integrate:
- Real-time measurements
- Automatic calculations
- Experiment evaluation
- Game-based interaction

---

## Proposed Solution
The proposed system introduces an integrated physics learning environment where students can:
- Play physics-based games
- Perform virtual laboratory experiments
- Generate measurements such as distance, time, and velocity
- View automatic formula calculations
- Receive step-by-step explanations
- Get experiment performance feedback

---

## Main Features

### Interactive Physics Games
- Physics simulations using Unity 2D
- Motion-based activities
- Real-time interaction

### Measurement Extraction
- Distance, velocity, and time calculation
- Motion tracking using computer vision
- Object detection using YOLOv8 and OpenCV

### Physics Calculation Engine
- Automatic formula application
- Step-by-step calculation explanations
- Concept-based learning support

### Virtual Laboratory
- Digital experiment environment
- Interactive tools and apparatus
- Guided practical activities

### Experiment Evaluation
- Safety checking
- Procedure validation
- Apparatus selection evaluation
- Result accuracy checking

### Cloud Storage
- Firebase integration
- Real-time data synchronization
- User progress tracking

---

## Technologies Used

### Frontend
- Flutter
- Unity 2D

### Backend
- Python

### Database & Cloud
- Firebase
- Cloud Firestore

### AI / Computer Vision
- OpenCV
- YOLOv8

---

## System Architecture
The system contains:
1. Mobile Application Interface
2. Physics Game Module
3. Virtual Experiment Module
4. Measurement Extraction Module
5. Physics Calculation Engine
6. Experiment Evaluation Module
7. Firebase Cloud Database

Workflow:
1. User performs actions in games or experiments
2. System captures measurements
3. Physics formulas are applied
4. Results and explanations are generated
5. Feedback is displayed to the student

---

## Objectives

### Main Objective
Develop an interactive physics learning system that integrates:
- Game-based learning
- Automated calculations
- Virtual experimentation

### Specific Objectives
- Generate real-time physics measurements
- Apply formulas automatically
- Provide step-by-step explanations
- Evaluate practical performance
- Improve student engagement
- Support rural education

---

## Target Users
- Grade 6–11 students
- Teachers
- Schools
- Educational institutions

---

## Functional Requirements
The system should:
- Support interactive games
- Generate real-time measurements
- Display calculations
- Provide virtual experiments
- Evaluate student performance
- Store user progress
- Provide feedback

---

## Non-Functional Requirements
- Fast performance
- User-friendly interface
- Secure data handling
- Cross-platform compatibility
- Reliability and scalability

---

## Research Contribution
This project introduces a unique integration of:
- Game-based physics learning
- Real-time measurement extraction
- Automated calculation support
- Virtual experiment evaluation

Unlike existing systems, all features are combined into one mobile platform.

---

## Commercialization Plan
The platform can be used as:
- A school learning platform
- A freemium educational application
- A subscription-based educational service

Potential users:
- Schools
- Tuition classes
- Educational organizations

---

## Future Improvements
- More physics experiments
- AI tutor/chatbot integration
- Advanced analytics
- Personalized learning recommendations
- Multiplayer collaborative experiments

---

## Expected Outcomes
- Better understanding of physics concepts
- Increased student engagement
- Improved practical learning experience
- Accessible laboratory learning for rural students

---

## Development Tools
- Flutter
- Unity 2D
- Python
- Firebase
- OpenCV
- YOLOv8
- Visual Studio Code

---

## Research Methodology
The project follows:
- Design Science Research (DSR)
- Experimental evaluation methods

Testing includes:
- User interaction testing
- Performance analysis
- Learning outcome evaluation


Game-Based-Measurement-and-Calculation
## Project Structure

### Root Directory
```
R26-IT-150/
├── README.md                 # Project documentation
├── backend/                  # Python backend API
├── mobile_app/              # Flutter mobile application
├── firebase/                # Firebase configuration
└── physics-experiments/     # Unity physics experiments module
```

### Backend Structure
```
backend/
├── requirements.txt         # Python dependencies
└── app/
    ├── main.py             # Main application entry point
    ├── api/                # API endpoints
    │   ├── analytics.py
    │   ├── auth.py
    │   ├── chatbot.py
    │   ├── learning_path.py
    │   └── quiz.py
    ├── config/             # Configuration files
    ├── database/           # Database clients
    │   └── firestore_client.py
    ├── models/             # Data models
    │   ├── performance.py
    │   ├── quiz.py
    │   └── user.py
    ├── services/           # Business logic services
    │   ├── analytics_service.py
    │   ├── feedback_service.py
    │   ├── learning_path_service.py
    │   └── quiz_service.py
    └── utils/              # Utility functions
```

### Mobile App Structure
```
mobile_app/
├── pubspec.yaml            # Flutter dependencies
├── lib/
│   ├── main.dart          # Main application entry point
│   ├── bloc/              # BLoC pattern implementations
│   ├── core/              # Core utilities
│   │   ├── constants/
│   │   ├── services/
│   │   ├── theme/
│   │   └── utils/
│   ├── features/          # Feature modules
│   │   ├── analytics/
│   │   ├── auth/
│   │   ├── chatbot/
│   │   ├── dashboard/
│   │   ├── games/
│   │   ├── learning_path/
│   │   ├── lessons/
│   │   ├── quiz/
│   │   └── upload-image/
│   ├── models/            # Data models
│   ├── providers/         # State management providers
│   └── routes/            # Navigation routes
├── android/               # Android native code
├── ios/                   # iOS native code
├── web/                   # Web version
├── windows/               # Windows desktop version
├── linux/                 # Linux desktop version
├── macos/                 # macOS desktop version
└── test/                  # Flutter tests
```

### Firebase Structure
```
firebase/
├── firebase.json          # Firebase configuration
├── firestore.indexes.json # Firestore indexes
├── firestore.rules        # Firestore security rules
└── storage.rules          # Cloud Storage security rules
```

### Physics Experiments Module (Unity)
```
physics-experiments/
├── Assets/
│   ├── Scenes/                    # Unity scenes for different experiments
│   │   ├── MotionExperiment.unity
│   │   ├── ForceExperiment.unity
│   │   ├── EnergyExperiment.unity
│   │   └── WavesExperiment.unity
│   ├── Scripts/                   # C# scripts
│   │   ├── Experiments/
│   │   │   ├── BaseExperiment.cs
│   │   │   ├── MotionExperiment.cs
│   │   │   ├── ForceExperiment.cs
│   │   │   ├── EnergyExperiment.cs
│   │   │   └── WavesExperiment.cs
│   │   ├── Measurements/
│   │   │   ├── MeasurementCalculator.cs
│   │   │   ├── VelocityCalculator.cs
│   │   │   ├── AccelerationCalculator.cs
│   │   │   └── ForceCalculator.cs
│   │   ├── UI/
│   │   │   ├── MeasurementDisplay.cs
│   │   │   ├── FormulaDisplay.cs
│   │   │   └── ResultsPanel.cs
│   │   ├── Physics/
│   │   │   ├── PhysicsSimulator.cs
│   │   │   ├── ObjectController.cs
│   │   │   └── EnvironmentController.cs
│   │   └── API/
│   │       ├── FirebaseManager.cs
│   │       ├── APIClient.cs
│   │       └── DataUploader.cs
│   ├── Prefabs/                   # Reusable prefabs
│   │   ├── Apparatus/
│   │   │   ├── Pendulum.prefab
│   │   │   ├── Incline.prefab
│   │   │   ├── Spring.prefab
│   │   │   └── Pulley.prefab
│   │   ├── UI/
│   │   │   ├── MeasurementPanel.prefab
│   │   │   └── FormulaPanel.prefab
│   │   └── Objects/
│   │       ├── Ball.prefab
│   │       ├── Box.prefab
│   │       └── Cylinder.prefab
│   ├── Materials/                 # Unity materials
│   ├── Animations/                # Animation clips
│   ├── Images/                    # Sprites and textures
│   └── Resources/                 # Resources folder
├── ProjectSettings/               # Unity project settings
├── Packages/                      # Package manifest
├── Assets.meta                    # Meta file
└── Assembly definitions/          # Assembly definitions

#### Key Directories Explained:
- **Assets/Scenes/**: Each scene represents a virtual experiment
- **Assets/Scripts/Experiments/**: Core experiment logic
- **Assets/Scripts/Measurements/**: Calculation engines for physics quantities
- **Assets/Scripts/UI/**: Display components for measurements and formulas
- **Assets/Scripts/Physics/**: Physics simulation logic
- **Assets/Scripts/API/**: Backend communication and data sync
- **Assets/Prefabs/**: Reusable components (apparatus, UI elements, objects)
```

---

## Installation and Setup

### Prerequisites
- Flutter SDK (>=3.0.0)
- Python 3.8+
- Node.js (for Firebase CLI)
- Unity 2021 LTS or newer
- Android Studio / Xcode (for mobile development)

### Backend Setup

#### 1. Install Python Dependencies
```bash
cd backend
python -m venv venv
# On Windows
venv\Scripts\activate
# On macOS/Linux
source venv/bin/activate
pip install -r requirements.txt
```

#### 2. Configure Environment Variables
Create a `.env` file in the `backend/` directory:
```
FIREBASE_PROJECT_ID=your_firebase_project_id
FIREBASE_API_KEY=your_firebase_api_key
FIREBASE_DATABASE_URL=your_firestore_url
ALLOWED_ORIGINS=http://localhost:8000,your_production_url
```

#### 3. Run Backend Server
```bash
cd backend
python -m uvicorn app.main:app --reload
```
The API will be available at `http://localhost:8000`

### Mobile App Setup

#### 1. Install Flutter Dependencies
```bash
cd mobile_app
flutter pub get
```

#### 2. Configure Firebase
```bash
# Install Firebase CLI
npm install -g firebase-tools

# Login to Firebase
firebase login

# Connect to your Firebase project
firebase init
```

#### 3. Run Mobile App
```bash
# For development
flutter run

# For specific device
flutter run -d <device_id>

# Build APK (Android)
flutter build apk --release

# Build IPA (iOS)
flutter build ios --release
```

### Unity Physics Experiments Setup

#### 1. Create Unity Project
```bash
# Create a new 2D project
unity -createProject physics-experiments -quit
```

#### 2. Install Required Packages
In Unity Editor:
- Go to Window > TextMesh Pro > Import TMP Essential Resources
- Window > Package Manager
- Add packages:
  - Firebase SDK for Unity
  - OpenCV for Unity
  - DOTween (for animations)

#### 3. Clone/Import Scripts
```bash
# Copy Assets folder to physics-experiments/Assets/
cp -r physics-experiments/Assets/* <your-unity-project>/Assets/
```

#### 4. Configure Firebase in Unity
1. Download `google-services.json` from Firebase Console
2. Place it in `Assets/Resources/`
3. In Unity: Window > Firebase > Firebase Unity SDK > Check Health
4. Authorize Firebase

#### 5. Build Experiment Module
```bash
# Build for Android
unity -projectPath physics-experiments -executeMethod BuildScript.BuildAndroid -quit

# Build for iOS
unity -projectPath physics-experiments -executeMethod BuildScript.BuildiOS -quit
```

### Firebase Setup

#### 1. Initialize Firebase
```bash
cd firebase
firebase init
```

#### 2. Deploy Firestore Rules
```bash
firebase deploy --only firestore:rules
```

#### 3. Deploy Storage Rules
```bash
firebase deploy --only storage
```

#### 4. Create Firestore Collections
Run the following in Firebase Console:
- Create `users` collection
- Create `experiments` collection
- Create `measurements` collection
- Create `quiz_attempts` collection

---

## Usage Examples

### Example 1: Running a Motion Experiment

#### In Unity:
```csharp
// Create instance of motion experiment
MotionExperiment experiment = new MotionExperiment();
experiment.Initialize();

// Set up parameters
experiment.SetObjectMass(2f);  // 2 kg
experiment.SetInitialVelocity(5f);  // 5 m/s
experiment.SetForce(10f);  // 10 N

// Run simulation
experiment.StartSimulation();

// Get measurements
float distance = experiment.GetDistance();
float time = experiment.GetTime();
float velocity = experiment.GetFinalVelocity();
```

#### In Flutter Mobile App:
```dart
// Connect to experiment
final experiment = PhysicsExperiment(
  type: ExperimentType.motion,
  topic: 'Kinematics',
);

// Perform experiment
await experiment.initialize();
await experiment.runSimulation();

// Display results
final results = experiment.getResults();
showMeasurements(results);
```

### Example 2: Physics Calculation with Step-by-Step Explanation

#### Backend API Request:
```bash
curl -X POST http://localhost:8000/api/calculate \
  -H "Content-Type: application/json" \
  -d '{
    "formula": "v = u + at",
    "parameters": {
      "u": 5,
      "a": 2,
      "t": 3
    }
  }'
```

#### Response:
```json
{
  "formula": "v = u + at",
  "result": 11,
  "steps": [
    {
      "step": 1,
      "description": "Identify known values",
      "values": {"u": 5, "a": 2, "t": 3}
    },
    {
      "step": 2,
      "description": "Apply formula v = u + at",
      "calculation": "v = 5 + (2 × 3)"
    },
    {
      "step": 3,
      "description": "Calculate result",
      "calculation": "v = 5 + 6 = 11"
    }
  ],
  "unit": "m/s"
}
```

### Example 3: Uploading Experiment Results

#### From Unity to Firebase:
```csharp
// Create experiment result
var experimentResult = new ExperimentResult
{
    userId = currentUser.uid,
    experimentType = "Force and Motion",
    measurements = new Dictionary<string, float>
    {
        {"distance", 25.5f},
        {"time", 5.2f},
        {"velocity", 4.9f},
        {"acceleration", 0.95f}
    },
    timestamp = System.DateTime.Now,
    performanceScore = 85
};
=======
- Apparatus Selection: Correct
- Procedure: Valid
- Result Accuracy: Good
- Feedback: Excellent work. Your experiment was completed successfully.
=======
# 🚀 Project Workspace

This repository contains the source code and configuration for:

- 📱 **Mobile App** (Flutter/Dart)
- 🐍 **Backend API** (Python)
- ☁️ **Firebase Configuration**

---

## 📁 Repository Structure

```
.
├── 📱 mobile_app/              # Flutter mobile application
├── 🐍 backend/                 # Backend API (Python)
├── ☁️ firebase/                # Firebase configuration and rules
├── README.md
└── ... (other files)
```

---

## 🧑‍💻 Language Composition

| Language  | Percentage |
|-----------|-----------|
| <img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/python/python-original.svg" alt="Python" width="20"/> Python      | (update %)  |
| <img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/flutter/flutter-original.svg" alt="Dart" width="20"/> Dart        | 2.7%        |
| <img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/swift/swift-original.svg" alt="Swift" width="20"/> Swift         | 3.6%        |
| <img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/html5/html5-original.svg" alt="HTML" width="20"/> HTML           | 2.9%        |
| 🗂️ Other   | ...%         |

*Replace or update percentages & rows above to match your actual Python usage. C++, CMake, and C rows have been omitted.*

---

## 🌟 Components

### 📱 Mobile App
- **Directory:** `/mobile_app`
- **Tech Stack:** [Flutter](https://flutter.dev/) <img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/flutter/flutter-original.svg" width="100px" /> & Dart
- **Platforms:** Android & iOS
- **Info:** See [`mobile_app/README.md`](./mobile_app/README.md) for setup instructions.

### 🐍 Backend API
- **Directory:** `/backend` (or update as needed)
- **Language:** Python <img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/python/python-original.svg" width="100px" />
- **Purpose:** Provides REST API and backend services

### ☁️ Firebase Configuration
- **Directory:** `/firebase`
- **Purpose:** Cloud backend (Authentication, Database, Storage, etc.)

---

## 🚦 Getting Started

1. **Clone the repository:**  
   ```bash
   git clone https://github.com/sachintha-nirmal-sm/R26-IT-150.git
   ```

2. **Set up the mobile app:**  
   - Navigate to `/mobile_app`
   - Follow the instructions in [`mobile_app/README.md`](./mobile_app/README.md)

3. **Set up the backend API:**  
   - Navigate to `/backend`
   - Create a virtual environment and install dependencies:
     ```bash
     python3 -m venv venv
     source venv/bin/activate
     pip install -r requirements.txt
     ```
   - Start the backend server as described in project docs

4. **Configure Firebase:**  
   - Review files under `/firebase`
   - Set up your [Firebase](https://firebase.google.com/) project accordingly

---

## 📜 License

This project is for educational/demo purposes.  
See the repository's license file for more information.

---

**🤝 Contributions welcome!**  
Open an issue or a pull request for feedback or suggestions.
main
