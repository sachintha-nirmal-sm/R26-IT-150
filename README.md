# 🔬 Intelligent Keyword-Based Content Retrieval & Curriculum-Aligned 2D Game Generation

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Flutter](https://img.shields.io/badge/Flutter-3.0+-blue?logo=flutter)](https://flutter.dev)
[![Python](https://img.shields.io/badge/Python-3.9+-green?logo=python)](https://www.python.org)
[![Unity](https://img.shields.io/badge/Unity-2022+--green?logo=unity)](https://unity.com)
[![Firebase](https://img.shields.io/badge/Firebase-Realtime%20Database-orange?logo=firebase)](https://firebase.google.com)
[![Status](https://img.shields.io/badge/Status-In%20Development-yellow)]()

> A mobile-based virtual physics laboratory with AI-powered semantic search and curriculum-aligned 2D educational games for Grade 9–11 students in Sri Lanka.

**University**: Sri Lanka Institute of Information Technology  
**Research Project Code**: R26-IT-150  
**Module**: Module 2 – Intelligent Keyword-Based Content Retrieval & Curriculum-Aligned 2D Game Generation

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [System Architecture](#system-architecture)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Installation & Setup](#installation--setup)
- [API Documentation](#api-documentation)
- [Usage Guide](#usage-guide)
- [Game Templates](#game-templates)
- [Evaluation Metrics](#evaluation-metrics)
- [Future Improvements](#future-improvements)
- [Contributing](#contributing)
- [License](#license)

---

## 🎯 Overview

**PhysiV-Lab** is a comprehensive physics learning platform designed specifically for the Sri Lankan curriculum (NIE syllabus). It combines:

✅ **Intelligent Physics Search** - AI-powered semantic search with BM25 + SBERT hybrid retrieval  
✅ **Adaptive 2D Games** - Curriculum-aligned mini-games with mastery-based progression  
✅ **Low-Device Optimization** - Designed for low-end Android devices  
✅ **Real-Time Analytics** - Firebase-driven progress tracking and scoring  

The platform serves **Grade 6–11 students** with personalized, grade-adaptive learning experiences.

---

## 🚀 Key Features

### 🔍 Intelligent Physics Search System

| Feature | Description |
|---------|-------------|
| **Keyword-Based Search** | Find physics topics, definitions, formulas, and examples |
| **Semantic Understanding** | Natural language queries processed using SBERT |
| **Hybrid Retrieval** | BM25 for fast keyword matching + SBERT for semantic similarity |
| **Grade Filtering** | Content filtered by student grade level (9-11) |
| **NIE Alignment** | Results aligned with Sri Lankan curriculum standards |
| **Content Types** | Returns definitions, formulas, examples, and explanations |

**Example Query**: "How does gravity affect falling objects?" → Returns curated physics content + relevant formulas

### 🎮 Adaptive 2D Physics Game System

| Feature | Description |
|---------|-------------|
| **Unity 2D Games** | Educational mini-games embedded in Flutter app |
| **Firebase Config** | Game difficulty and parameters managed dynamically |
| **Grade Scaling** | Difficulty automatically adjusts to student grade |
| **Mastery Progression** | Stage unlocking based on student performance |
| **Physics Validation** | Formulas validated at runtime during gameplay |
| **Real-Time Scoring** | Instant feedback and score tracking |
| **Offline Support** | Games work offline with local caching |

---

## 🏗️ System Architecture

### Two-Tier Search Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Flutter Mobile App                        │
│                   (Search Interface)                         │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
        ┌─────────────────────┐
        │   FastAPI Backend   │
        └─────────────────────┘
                  │
        ┌─────────┴─────────┐
        ▼                   ▼
    ┌────────┐          ┌────────┐
    │  BM25  │          │ SBERT  │
    │ (Tier1)│          │(Tier 2)│
    └─────┬──┘          └──┬─────┘
          │ Fast Keyword   │ Semantic
          │ Matching       │ Similarity
          └─────┬──────────┘
                ▼
    ┌─────────────────────────┐
    │ Grade Filtering         │
    │ Result Ranking          │
    └────────┬────────────────┘
             ▼
    ┌──────────────────────┐
    │ Search Results       │
    │ (Top K Results)      │
    └──────────────────────┘
```

### Game Generation Architecture

```
┌────────────────────────────┐
│   Student Selects Game     │
└───────────┬────────────────┘
            │
            ▼
┌────────────────────────────┐
│  Fetch Game Config from    │
│  Firebase (Grade-Based)    │
└───────────┬────────────────┘
            │
            ▼
┌────────────────────────────┐
│  Launch Unity 2D Scene     │
│  Inside Flutter Widget     │
└───────────┬────────────────┘
            │
            ▼
┌────────────────────────────┐
│  Runtime Parameter         │
│  Injection                 │
└───────────┬────────────────┘
            │
            ▼
┌────────────────────────────┐
│  Student Plays Game        │
│  Physics Validation        │
└───────────┬────────────────┘
            │
            ▼
┌────────────────────────────┐
│  Score → Firebase          │
│  Progress → Update         │
└────────────────────────────┘
```

---

## 💻 Technology Stack

### Frontend
```
┌─ Flutter (Dart 3.0+)
│  ├─ Provider / Riverpod (State Management)
│  ├─ Go Router (Navigation)
│  ├─ flutter_unity_widget (Unity Integration)
│  ├─ Shimmer (Loading Animations)
│  └─ Image Picker (Asset Management)
└─ Target: Android 8.0+ (API 26+)
```

### Backend
```
┌─ Python 3.9+
│  ├─ FastAPI (REST API Framework)
│  ├─ rank_bm25 (BM25 Retrieval)
│  ├─ sentence-transformers (SBERT)
│  │  └─ all-MiniLM-L6-v2 (Pre-trained Model)
│  ├─ Uvicorn (ASGI Server)
│  └─ Firebase Admin SDK (Authentication)
```

### Database & Cloud
```
┌─ Firebase
│  ├─ Firestore (Game Config, Progress Data)
│  ├─ Realtime Database (Score Sync)
│  ├─ Cloud Storage (Assets)
│  └─ Authentication (Firebase Auth)
```

### Game Engine
```
┌─ Unity 2D (2022 LTS)
│  └─ C# Scripting
│  └─ Physics2D Engine
```

### AI/ML
```
┌─ Sentence-BERT (SBERT)
│  ├─ Cosine Similarity for Semantic Matching
│  └─ Fine-tuned for Physics Domain
```

---

## 📁 Project Structure

```
R26-IT-150/
├── 📱 mobile_app/                          # Flutter Frontend
│   ├── lib/
│   │   ├── main.dart
│   │   ├── features/
│   │   │   ├── dashboard/                  # Physics Lab Home
│   │   │   ├── search/                     # Search Interface
│   │   │   ├── games/                      # Game Launcher
│   │   │   ├── chatbot/                    # AI Assistant
│   │   │   ├── learning_path/              # Curriculum Navigation
│   │   │   ├── lessons/                    # Content Display
│   │   │   ├── quiz/                       # Assessment
│   │   │   └── analytics/                  # Progress Tracking
│   │   ├── models/                         # Data Models
│   │   ├── providers/                      # State Management
│   │   ├── routes/                         # Navigation Routes
│   │   ├── core/                           # Core Utilities
│   │   └── bloc/                           # BLoC Pattern
│   ├── assets/
│   │   ├── images/                         # UI Images
│   │   ├── animations/                     # Lottie Animations
│   │   ├── videos/                         # Educational Videos
│   │   └── games/                          # Game Assets
│   ├── android/                            # Android Config
│   ├── ios/                                # iOS Config
│   ├── pubspec.yaml                        # Dependencies
│   └── README.md
│
├── 🐍 backend/                             # Python FastAPI
│   ├── app/
│   │   ├── main.py                         # Entry Point
│   │   ├── api/
│   │   │   ├── search.py                   # Search Endpoints
│   │   │   ├── auth.py                     # Auth Endpoints
│   │   │   ├── chatbot.py                  # Chatbot API
│   │   │   ├── quiz.py                     # Quiz API
│   │   │   ├── analytics.py                # Analytics API
│   │   │   └── learning_path.py            # Learning Path API
│   │   ├── models/
│   │   │   ├── user.py                     # User Model
│   │   │   ├── quiz.py                     # Quiz Model
│   │   │   └── performance.py              # Performance Model
│   │   ├── services/
│   │   │   ├── search_service.py           # Search Logic
│   │   │   ├── sbert_service.py            # SBERT Integration
│   │   │   ├── bm25_service.py             # BM25 Retrieval
│   │   │   ├── analytics_service.py        # Analytics
│   │   │   ├── feedback_service.py         # User Feedback
│   │   │   └── learning_path_service.py    # Path Recommendations
│   │   ├── database/
│   │   │   ├── firestore_client.py         # Firestore Connection
│   │   │   └── config/                     # DB Config
│   │   ├── utils/                          # Helper Functions
│   │   └── config/                         # App Config
│   ├── requirements.txt
│   ├── .env.example
│   └── README.md
│
├── 🎮 unity_games/                         # Unity 2D Games
│   ├── Assets/
│   │   ├── Scenes/
│   │   │   ├── CarRace.unity
│   │   │   ├── FallingObject.unity
│   │   │   ├── CircuitBuilder.unity
│   │   │   ├── PendulumSim.unity
│   │   │   └── PressureLever.unity
│   │   ├── Scripts/
│   │   │   ├── GameController.cs
│   │   │   ├── PhysicsValidator.cs
│   │   │   ├── ScoreManager.cs
│   │   │   └── UIController.cs
│   │   ├── Prefabs/
│   │   └── Resources/
│   ├── ProjectSettings/
│   └── README.md
│
├── 🔥 firebase/                            # Firebase Config
│   ├── firebase.json
│   ├── firestore.rules
│   ├── firestore.indexes.json
│   ├── storage.rules
│   └── functions/
│       └── search_index.js
│
├── 📊 ml_models/                           # AI Models
│   ├── sbert/
│   │   └── all-MiniLM-L6-v2/
│   ├── bm25/
│   │   └── index/
│   ├── training/
│   │   └── fine_tune.py
│   └── README.md
│
├── 📚 datasets/                            # Training Data
│   ├── physics_curriculum.json
│   ├── ni_syllabus.json
│   ├── game_configs.json
│   └── README.md
│
├── 📖 docs/                                # Documentation
│   ├── ARCHITECTURE.md
│   ├── API_REFERENCE.md
│   ├── DEPLOYMENT.md
│   ├── CONTRIBUTING.md
│   └── TROUBLESHOOTING.md
│
└── README.md                               # Main README (this file)
```

---

## ⚙️ Installation & Setup

### Prerequisites

- **Flutter**: 3.0+
- **Dart**: 3.0+
- **Python**: 3.9+
- **Node.js**: 16+ (for Firebase CLI)
- **Unity**: 2022 LTS
- **Android SDK**: API 26+

### 1️⃣ Flutter Setup

```bash
# Clone repository
git clone https://github.com/sliit-foss/physiv-lab.git
cd R26-IT-150/mobile_app

# Get dependencies
flutter pub get

# Configure Firebase
flutterfire configure

# Run on connected device
flutter run

# Build APK for distribution
flutter build apk --release
```

### 2️⃣ Python Backend Setup

```bash
# Navigate to backend
cd ../backend

# Create virtual environment
python -m venv venv

# Activate virtual environment
# On Windows:
venv\Scripts\activate
# On macOS/Linux:
source venv/bin/activate

# Install dependencies
pip install -r requirements.txt

# Download SBERT model
python -c "from sentence_transformers import SentenceTransformer; SentenceTransformer('all-MiniLM-L6-v2')"

# Create .env file
cp .env.example .env

# Start FastAPI server
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000
```

### 3️⃣ Unity Game Setup

```bash
# Open Unity Hub
# Add project: R26-IT-150/unity_games

# Open Unity Editor
# Configure build settings:
# - Platform: Android
# - Minimum API Level: 26
# - Target API Level: 34

# Build as Library (for Flutter integration)
# File > Build Settings > Android > Build as Library
```

### 4️⃣ Firebase Configuration

```bash
# Install Firebase CLI
npm install -g firebase-tools

# Login to Firebase
firebase login

# Navigate to firebase folder
cd firebase

# Deploy Firestore rules
firebase deploy --only firestore:rules

# Deploy storage rules
firebase deploy --only storage

# Create indexes
firebase firestore:indexes

# Initialize Realtime Database
# Done through Firebase Console
```

### 5️⃣ Environment Variables

Create `.env` file in `backend/`:

```env
# Firebase
FIREBASE_PROJECT_ID=your_project_id
FIREBASE_PRIVATE_KEY=your_private_key
FIREBASE_CLIENT_EMAIL=your_client_email

# API
API_HOST=0.0.0.0
API_PORT=8000
DEBUG=True

# Models
SBERT_MODEL=all-MiniLM-L6-v2
BM25_INDEX_PATH=./models/bm25_index

# Database
FIRESTORE_DATABASE=default
```

---

## 📡 API Documentation

### Base URL
```
http://localhost:8000/api
```

### 🔍 Search Endpoints

#### Get Search Results
```http
GET /search?query=string&grade=integer
```

**Query Parameters:**
- `query` (string): Physics topic or keyword
- `grade` (integer): Student grade (9-11)

**Response:**
```json
{
  "status": "success",
  "results": [
    {
      "id": "123",
      "title": "Newton's Laws of Motion",
      "content": "...",
      "formula": "F = ma",
      "examples": [...],
      "difficulty": "intermediate",
      "relevance_score": 0.95
    }
  ],
  "total": 5
}
```

#### Semantic Search
```http
POST /search/semantic
Content-Type: application/json

{
  "query": "How does gravity affect falling objects?",
  "grade": 10,
  "top_k": 5
}
```

---

### 🎮 Game Endpoints

#### Get Game Configuration
```http
GET /game/config?grade=integer&topic=string
```

**Response:**
```json
{
  "game_id": "car_race_001",
  "game_type": "CarRace",
  "difficulty_level": 3,
  "parameters": {
    "gravity": 9.8,
    "friction": 0.5,
    "target_speed": 50
  },
  "max_score": 100,
  "time_limit": 300
}
```

#### Submit Game Score
```http
POST /game/score
Content-Type: application/json

{
  "student_id": "user_123",
  "game_id": "car_race_001",
  "score": 85,
  "time_taken": 120,
  "completion_percentage": 95
}
```

**Response:**
```json
{
  "status": "success",
  "mastery_level": "advanced",
  "next_level_unlocked": true,
  "total_points": 250
}
```

---

### 📊 Analytics Endpoints

#### Get Student Progress
```http
GET /analytics/progress?student_id=string&grade=integer
```

#### Get Game Statistics
```http
GET /analytics/games?student_id=string&time_period=string
```

---

## 💡 Usage Guide

### For Students

1. **Open PhysiV-Lab App**
   - Login with credentials
   - Select or confirm grade level

2. **Search Physics Topics**
   - Enter keyword or natural language query
   - Browse results (definitions, formulas, examples)
   - Tap on topic to view detailed content

3. **Play Educational Games**
   - Select game from recommendations
   - Complete levels based on mastery
   - Earn points and unlock achievements

4. **Track Progress**
   - View learning analytics
   - Check game scores and statistics
   - Get personalized recommendations

### For Teachers

1. **Monitor Student Progress**
   - Dashboard shows class analytics
   - Track individual student performance
   - Identify learning gaps

2. **Assign Content**
   - Create learning paths
   - Set game targets
   - Monitor completion rates

---

## 🎮 Game Templates

| Game | Physics Concept | Grade | Duration |
|------|-----------------|-------|----------|
| 🏎️ **Car Race** | Velocity, acceleration, friction | 9-11 | 3-5 min |
| 📉 **Falling Object** | Gravity, terminal velocity | 9-10 | 2-3 min |
| ⚡ **Circuit Builder** | Electricity, circuits, resistance | 10-11 | 5-7 min |
| 🔄 **Pendulum Sim** | Simple harmonic motion, period | 10-11 | 4-6 min |
| 🔧 **Pressure & Lever** | Pressure, mechanical advantage, levers | 9-10 | 3-4 min |

---

## 📊 Evaluation Metrics

The system is evaluated using:

| Metric | Target | Description |
|--------|--------|-------------|
| **Precision@3** | >85% | Relevance of top 3 search results |
| **Recall@3** | >80% | Coverage of relevant results in top 3 |
| **MRR** | >0.85 | Mean Reciprocal Rank of results |
| **Grade Filter Accuracy** | >95% | Correct grade-level filtering |
| **SUS Score** | >70 | System Usability Scale (User Experience) |
| **Game Accuracy** | >90% | Physics calculation accuracy |
| **Student Engagement** | >75% | Active usage and game completion |

---

## 🔮 Future Improvements

- [ ] **Language Support** - English language support
- [ ] **Offline Mode** - Complete offline content access
- [ ] **Voice Search** - Speech-to-text query input
- [ ] **Collaborative Learning** - Multiplayer games and peer study
- [ ] **Advanced Analytics** - ML-based learning recommendations
- [ ] **Smart Tutoring** - AI chatbot for physics doubts
- [ ] **Gamification** - Badges, leaderboards, achievements
- [ ] **Content Expansion** - More game templates and topics
- [ ] **Parent Portal** - Guardian tracking and insights

---

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Contribution Guidelines
- Follow Flutter and Python style guides
- Add tests for new features
- Update documentation
- Ensure code quality with linting

---

## 📝 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 👥 Contributors

**Research Team - R26-IT-150**
- Module 2 Development Team
- Sri Lanka Institute of Information Technology

---

## 📞 Support & Contact

For issues, questions, or suggestions:

- **Issues**: Open an issue on GitHub
- **Email**: research@sliit.edu.lk
- **Documentation**: See `/docs` folder

---

## 🙏 Acknowledgments

- Sri Lankan NIE Curriculum advisors
- Flutter and Firebase communities
- Unity game engine
- SBERT and NLP contributors

---

<div align="center">

**Built with ❤️ for Physics Education in Sri Lanka**

⭐ If this project helps you, please consider giving it a star!

</div>
