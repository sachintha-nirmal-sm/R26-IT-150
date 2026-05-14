<<<<<<< HEAD
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

---

## License
This project is developed for educational and research purposes.

---



=======
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
>>>>>>> 943015b300df0e5385f02d371a7e7b07708fe02d
