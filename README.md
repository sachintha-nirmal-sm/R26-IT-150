# Mobile-Based Virtual Physics Laboratory with AI Assistance

## Project Overview

This project is part of the PhysiV-Lab mobile virtual physics laboratory system
designed for rural Sri Lankan students (Grades 6-11). The research focuses on
improving physics education accessibility using:

- AI-powered learning assistance
- Real-world contextual learning
- Scenario-based assessments
- Multilingual support (English + Sinhala)

The system aims to bridge the gap between theoretical physics concepts and
real-world applications.

## Main Features

### 1) Real-World Scenario Question Generator

Automatically generates physics MCQs from current world news.

Workflow:

- Collect news articles using RSS feeds
- Detect physics-related content using NLP
- Map content to Sri Lankan physics curriculum topics
- Generate grade-adaptive MCQs using LLMs
- Deliver questions after lesson completion

Technologies:

- Python FastAPI
- feedparser
- HuggingFace facebook/bart-large-mnli
- Ollama + Llama 3.1
- Firebase Firestore

### 2) AI-Powered Post-Lesson Q&A Agent

Students can ask physics questions after completing lesson MCQs.

Features:

- English and Sinhala support
- Automatic language detection
- Sinhala <-> English translation
- Physics keyword extraction
- BM25-based retrieval from knowledge base
- LLM fallback responses when retrieval fails

Technologies:

- spaCy
- rank_bm25
- langdetect
- Helsinki-NLP translation models
- Ollama LLMs

## Target Users

- Grade 6-11 rural students
- Physics teachers
- Educational institutions
- Sri Lankan schools with limited laboratory access

## System Architecture

Frontend:

- Flutter mobile application

Backend:

- Python FastAPI server

Database:

- Firebase Firestore

AI/NLP Stack:

- Ollama (local LLM inference)
- HuggingFace transformers
- BM25 retrieval
- spaCy NLP pipeline

## Key Research Contributions

Novel features:

- News-driven contextual physics question generation
- Sinhala-supported AI learning assistant
- Curriculum-aligned RAG-based learning support
- Offline/local AI processing without paid APIs

Educational goals:

- SDG 4 - Quality Education
- SDG 10 - Reduced Inequalities

## Functional Modules

News pipeline:

- RSS ingestion
- Physics relevance classification
- Curriculum mapping
- MCQ generation
- Firestore storage

Q&A pipeline:

- Student question input
- Language detection
- Translation
- Keyword extraction
- BM25 retrieval
- LLM fallback answer generation

## API Endpoints

### POST /qa-agent

Handles student question answering.

Input:

```json
{
	"question": "What is velocity?",
	"grade": 8
}
```

Output:

```json
{
	"answer": "Velocity is the rate of change of displacement..."
}
```

### GET /scenario-question

Returns a lesson-end scenario question.

Parameters:

- grade
- lesson_topic

## Knowledge Base Structure

Each entry contains:

- concept name
- grade range
- explanation
- formulas
- keyword tags

Example:

```json
{
	"concept": "Velocity",
	"grade_min": 6,
	"grade_max": 7,
	"explanation": "Velocity is how fast something moves in a specific direction."
}
```

## Core Technologies

| Category | Technology |
| --- | --- |
| Mobile App | Flutter |
| Backend | FastAPI |
| Database | Firebase Firestore |
| NLP | spaCy |
| Translation | Helsinki-NLP |
| Classification | BART MNLI |
| Retrieval | BM25 |
| LLM Runtime | Ollama |
| Models | Llama 3.1 |

## Non-Functional Requirements

- Fully local/offline AI processing
- No cloud API dependency
- Response time under 8 seconds
- Sinhala language support
- Modular scalable architecture

## Research Objectives

- Generate curriculum-aligned physics MCQs from live news
- Provide multilingual AI-assisted learning support
- Improve contextual understanding of physics
- Support rural education accessibility

## Expected Outcomes

- Improved student engagement
- Better conceptual understanding
- Real-world contextual learning
- Accessible AI tutoring for rural students

## Suggested Folder Structure

```
project/
|
|-- backend/
|   |-- api/
|   |-- qa_agent/
|   |-- scenario_pipeline/
|   |-- knowledge_base/
|
|-- frontend/
|   |-- flutter_app/
|
|-- models/
|
|-- firestore/
|
|-- docs/
```

## Repository Structure (Current)

- `mobile_app/`: Flutter application
- `backend/`: FastAPI service and supporting modules
- `firebase/`: Firebase rules, indexes, and project config

## Getting Started

### Backend (FastAPI)

1) Create and activate a virtual environment (optional but recommended).
2) Install dependencies:

```bash
pip install -r backend/requirements.txt
```

3) Run the API locally:

```bash
uvicorn app.main:app --reload
```

The API should respond at `http://127.0.0.1:8000/` with `{ "status": "ok" }`.

### Mobile App (Flutter)

1) Install Flutter and ensure it is on your PATH.
2) Fetch dependencies:

```bash
cd mobile_app
flutter pub get
```

3) Run the app on a connected device or emulator:

```bash
flutter run
```

## Firebase

Firebase configuration and Firestore rules/indexes are stored in the `firebase/`
directory. Update rules and indexes as needed, then deploy using the Firebase
CLI from that directory.

## Done So Far

- Defined project vision and research goals for Physics-Lab
- Designed core features for news-driven MCQ generation and AI Q&A assistance
- Selected AI stack and offline-first approach
- Outlined workflows for scenario-question and Q&A pipelines
- Documented API contract for `/qa-agent` and `/scenario-question`

## Next Steps

- Build MCQ generation module and Firestore storage layer
- Implement Q&A pipeline (language detection, translation, retrieval, fallback)
- Populate and validate the physics knowledge base
- Integrate backend APIs with the Flutter app

## Notes

- Backend routes are defined under `backend/app/api/` and wired in
	`backend/app/main.py`.
- If environment variables or secrets are required, add them locally and do not
	commit them to version control.
