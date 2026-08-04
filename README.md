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

A mobile-based virtual physics laboratory designed for Grade 6-11 students, especially students in rural areas with limited access to physical laboratory facilities.

This project combines interactive physics games, virtual experiments, real-time measurement generation, automated physics calculations, and AI-assisted learning support to improve students' conceptual understanding and practical learning experience.

**Project Overview**
Physics is often difficult for students because many concepts are abstract and schools may not have enough laboratory resources. This system provides a mobile learning platform where students can perform virtual experiments, play physics-based games, view measurements, and understand calculations step by step.

The platform supports:

- Interactive physics games
- Virtual laboratory experiments
- Real-time distance, time, and velocity measurements
- Automated physics formula calculations
- Step-by-step explanations
- Experiment performance feedback
- Student progress tracking

**Problem Statement**

Many students struggle to understand physics due to:

- Lack of physical laboratory facilities
- Theory-based learning methods
- Limited practical experience
- Difficulty understanding abstract concepts

Existing learning systems often focus only on simulations, quizzes, or gamification. Most do not combine real-time measurements, automatic calculations, experiment evaluation, and game-based interaction in one platform.

**Proposed Solution**

This project introduces an integrated mobile learning environment where students can:

- Perform virtual physics experiments
- Play interactive physics-based games
- Generate measurements such as distance, time, and velocity
- View automatic formula-based calculations
- Receive step-by-step explanations
- Get feedback based on experiment performance

**Main Features**
Interactive Physics Games
- Physics simulations using Unity 2D
- Motion-based activities
- Real-time user interaction
- Game-based concept learning

**Measurement Extraction**
- Distance calculation
- Time measurement
- Velocity calculation
- Motion tracking using computer vision
- Object detection using YOLOv8 and OpenCV

**Physics Calculation Engine**
- Automatic formula selection
- Step-by-step calculation explanations
- Concept-based learning support
- Real-time result generation

**Virtual Laboratory**
- Digital experiment environment
- Interactive apparatus and tools
- Guided practical activities
- Student-friendly experiment flow

**Experiment Evaluation**
- Procedure validation
- Apparatus selection checking
- Safety rule checking
- Result accuracy evaluation
- Performance feedback

**Cloud Storage**

- Firebase integration
- Cloud Firestore database
- Real-time data synchronization
- User progress tracking

**Technologies Used**

Frontend
- Flutter
- Unity 2D

Backend
- Python

Database 
- Firebase
- Cloud Firestore

AI and Computer Vision
- OpenCV
- YOLOv8

Development Tools
- Visual Studio Code
- Android Studio
- Firebase Console
- Unity Editor

**System Architecture**

The system consists of the following main modules:

1. Mobile Application Interface
2. Physics Game Module
3. Virtual Experiment Module
4. Measurement Extraction Module
5. Physics Calculation Engine
6. Experiment Evaluation Module
7. Firebase Cloud Database

**System Workflow**

1. The student performs an activity in a game or virtual experiment.
2. The system captures motion and experiment data.
3. Measurements such as distance, time, and velocity are generated.
4. Physics formulas are applied automatically.
5. Results and step-by-step explanations are displayed.
6. Experiment feedback is provided to the student.
7. Student progress is stored in Firebase.

**Objectives**

Main Objective

- To develop an interactive mobile-based physics learning system that integrates game-based learning, automated calculations, and virtual experimentation.

Specific Objectives

- Generate real-time physics measurements
- Apply physics formulas automatically
- Provide step-by-step explanations
- Evaluate practical experiment performance
- Improve student engagement
- Support rural education through mobile learning
  
**Target Users**

- Grade 6-11 students
- Physics teachers
- Schools
- Tuition classes
- Educational institutions

**Functional Requirements**
The system should be able to:

- Support interactive physics games
- Provide virtual experiments
- Generate real-time measurements
- Display physics calculations
- Provide step-by-step explanations
- Evaluate student experiment performance
- Store user progress
- Provide learning feedback
 
**Non-Functional Requirements**

- Fast performance
- User-friendly interface
- Secure data handling
- Cross-platform compatibility
- Reliable operation
- Scalable cloud storage

**Research Contribution**

This project contributes an integrated physics learning platform that combines:

- Game-based physics learning
- Real-time measurement extraction
- Automated physics calculation support
- Virtual experiment evaluation
- Mobile accessibility for rural students
- Unlike many existing systems, this project combines simulations, calculations, feedback, and experiment evaluation into one mobile platform.

**Commercialization Plan**
The platform can be used as:

- A school-based learning platform
- A freemium educational mobile application
- A subscription-based educational service
- A digital tool for tuition classes and learning centers

Potential customers include:

- Schools
- Tuition classes
- Educational organizations
- Individual students and teachers

**Future Improvements**

- Add more physics experiments
- Integrate an AI tutor or chatbot
- Add advanced student analytics
- Provide personalized learning recommendations
- Support multiplayer collaborative experiments
- Improve computer vision-based measurement accuracy

**Expected Outcomes**

The expected outcomes of this project are:

- Better understanding of physics concepts
- Increased student engagement
- Improved practical learning experience
- Accessible laboratory learning for rural students
- Better support for teachers and educational institutions

**Research Methodology**

This project follows:

- Design Science Research methodology
- Experimental evaluation methods

Testing includes:

- User interaction testing
- Performance analysis
- Learning outcome evaluation
- Sytem usability testing

**Set Up Unity Games**

1.Open Unity Hub.

2.Select Open Project.

3.Choose the unity_games/ folder.

4.Open the project in Unity Editor.

5.Build or export the Unity game module for mobile integration.

**Example 1: Velocity Calculation**

A student performs a motion experiment.

Input measurements:

Distance = 10 meters
Time = 5 seconds

Formula:

Velocity = Distance / Time

Calculation:

Velocity = 10 / 5
Velocity = 2 m/s

Output shown to student:

The object moved with a velocity of 2 m/s.

Step-by-step explanation:
1. Distance travelled by the object is 10 meters.
2. Time taken is 5 seconds.
3. Velocity is calculated using the formula: Velocity = Distance / Time.
4. Therefore, Velocity = 10 / 5 = 2 m/s.
   
**Example 2: Virtual Experiment Flow**

1. Student selects a motion experiment.
2. Student starts the virtual activity.
3. The system tracks object movement.
4. Distance and time are recorded.
5. Velocity is calculated automatically.
6. The student receives explanation and feedback.
7. Progress is saved to Firebase.
   
**Example 3: Experiment Evaluation**

If a student selects the correct apparatus and follows the correct procedure:

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
