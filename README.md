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
