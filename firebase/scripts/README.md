# Firebase Admin SDK Python Scripts
# AI-Powered Grade-Based Learning Platform

## Purpose
This folder contains Python scripts that use the **Firebase Admin SDK** to
perform privileged Firestore operations that cannot be done from the Flutter
client (see Section 10 of `devmini.md`).

All scripts here bypass Firestore Security Rules (Admin SDK privilege),
so they must be run in a controlled environment — **never expose these
scripts or the service account key to the Flutter client or a public server
without proper authentication.**

---

## Folder Structure

```
scripts/
├── README.md                   ← this file
├── requirements.txt            ← Python dependencies (firebase-admin, etc.)
├── config.py                   ← shared config (project ID, credential path)
│
├── seed/                       ← one-time seed / setup scripts
│   ├── seed_admin_user.py      ← provision the first admin account + custom claim
│   └── seed_sample_lessons.py  ← (optional) seed sample lesson documents for dev
│
└── jobs/                       ← recurring / scheduled job scripts
    └── (to be added later)     ← e.g. grade promotion job, analytics rollup
```

---

## Scripts (Planned — Not Yet Written)

| Script | Purpose |
|---|---|
| `seed/seed_admin_user.py` | Creates the first admin user in Firebase Auth, sets the `role: admin` custom claim, and writes the corresponding `users/{uid}` doc |
| `seed/seed_sample_lessons.py` | Seeds sample `lessons/` documents for Grade 9/10/11 so the Flutter app has data to display immediately |
| `jobs/grade_promotion.py` | Promotes eligible students to the next grade — reads `users` where `currentGrade` meets the threshold, checks `lastPromotedAt`, and updates atomically |

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
   python seed/seed_admin_user.py
   ```

> ⚠️ **Never commit `serviceAccountKey.json` to version control.**
> It grants full Admin SDK access to your Firebase project.
