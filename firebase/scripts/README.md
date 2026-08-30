# Firebase Admin SDK Python Scripts
# PhysiV-Lab — shared Firestore (quiz/RAG + practicals modules)

## Purpose
This folder contains Python scripts that use the **Firebase Admin SDK** to
perform privileged Firestore operations that cannot be done from the Flutter
client.

All scripts here bypass Firestore Security Rules (Admin SDK privilege),
so they must be run in a controlled environment — **never expose these
scripts or the service account key to the Flutter client or a public server
without proper authentication.**

---

## Folder Structure

```
scripts/
├── README.md
├── requirements.txt
├── config.py
├── firebase_init.py
├── auth_helpers.py
├── verify_seed_data.py
│
└── seed/
    ├── seed_users.py
    ├── seed_admin_user.py
    ├── seed_sample_lessons.py
    ├── seed_materials.py
    ├── seed_quizzes.py
    ├── seed_question_bank.py
    ├── seed_generation_job.py
    ├── seed_final_quiz.py
    ├── seed_final_quiz_attempt.py
    ├── seed_student_progress.py          ← quiz attempts / weak topics
    ├── seed_analytics.py
    ├── seed_topics.py                    ← practicals: topics + parent lessons
    ├── seed_practicals.py                ← practicals: Unity practical config
    ├── seed_student_practicals.py        ← practicals: results + progress
    └── seed_practical_module.py          ← runs the three practicals seeds
```

---

## Game-Based Measurement practicals module

Live Firebase project: **physics-learning-platform**. Documents are visible
in Console only after they are written:

```bash
cd firebase/scripts
python seed_firestore_live.py
```

Collections added for the Unity practicals workflow. Existing `users` and
`lessons` documents are reused — they are **not** duplicated.

| Collection | Document ID | Written by |
|---|---|---|
| `topics` | `topicId` | `seed_topics.py` |
| `practicals` | `practicalId` (e.g. `grade10_newtons_laws`) | `seed_practicals.py` |
| `practicalResults` | auto / seed id | FastAPI later; sample via `seed_student_practicals.py` |
| `studentPracticals` | `{studentId}_{practicalId}` | FastAPI later; sample via `seed_student_practicals.py` |
| `studentProgress` | `{studentId}` (= Auth UID) | FastAPI later; sample via `seed_student_practicals.py` |

Attempt limits (`demoMaxAttempts`, `practicalMaxAttempts`) live on
`practicals/{id}` and must be enforced by the backend, never the client.

Word-doc names `practical_results`, `student_practicals`, and
`student_progress` map to the camelCase names above so this module matches
the rest of the shared Firestore.

### Seed order

```bash
cd firebase/scripts
pip install -r requirements.txt
python seed/seed_users.py
python seed/seed_practical_module.py
python verify_seed_data.py
```

---

## Setup

1. **Install dependencies:**
   ```bash
   pip install -r requirements.txt
   ```

2. **Set up a service account key:**
   - Go to Firebase Console → Project Settings → Service Accounts
   - Click "Generate new private key" → download the JSON file
   - Save it as `serviceAccountKey.json` in this folder (already in `.gitignore`)

3. **Run a script:**
   ```bash
   python seed/seed_practical_module.py
   ```

> ⚠️ **Never commit `serviceAccountKey.json` to version control.**
> It grants full Admin SDK access to your Firebase project.
