# System Architecture Extension
### Role-Based Flows, Workflows, and FastAPI Architecture
**Builds on `firestore-database-architecture.md` (v2). No Firestore collections are redefined here — this document describes behavior on top of that schema.**

----

## 1. Role-Based Authentication Flow

1. User opens the single Flutter app and authenticates via **Firebase Authentication** (email/password, matching the existing Student Sign Up flow).
2. On successful sign-in, Flutter sends the Firebase **ID token** to FastAPI (every subsequent request carries this token in the `Authorization` header — never a custom session scheme).
3. FastAPI verifies the token using the Firebase Admin SDK, extracts `uid` and the `role` custom claim from the token.
4. FastAPI reads `users/{uid}` to confirm `role` and (for students) `currentGrade`, `status` (`active`/`suspended`).
5. FastAPI returns a single "bootstrap" response to Flutter: `{ role, fullName, currentGrade? }`.
6. Flutter uses `role` — and only `role` — to decide which interface to render. It never infers role from any other field.

**Why the claim is checked server-side on every request, not just at login:** a custom claim can be stale if changed mid-session (e.g. admin revokes an account); re-validating the token per-request (which Firebase does automatically via signature + claims embedded in the token) prevents a suspended/demoted account from continuing to act on old cached state.

---

## 2. Role-Based Navigation Flow

```
App Launch
   │
   ▼
Firebase Auth Check
   │
   ├── Not signed in ──▶ Login / Sign Up Screen (Sign Up only ever creates role=student)
   │
   └── Signed in ──▶ FastAPI bootstrap call ──▶ role?
                          │
                          ├── admin  ──▶ Admin Shell
                          │              ├── Lesson Management
                          │              ├── Material Upload
                          │              ├── Question Bank Generation & Monitoring
                          │              ├── Question Management (view/edit/delete/regenerate)
                          │              ├── Final Quiz Generation & Monitoring
                          │              └── Admin Analytics Dashboard
                          │
                          └── student ──▶ Student Shell
                                         ├── Lessons (grade-filtered)
                                         ├── Lesson Quizzes → Attempt → Result/Feedback
                                         ├── Wrong Questions / Weak Topics review
                                         ├── YouTube Recommendations
                                         └── Final Quiz → Attempt → Result/Feedback
```

There is **one Flutter codebase**, gated at the shell level by `role` — not two separate apps. Shared widgets (result screens, feedback cards, recommendation lists) are reused between the lesson-quiz and final-quiz flows since their underlying data shapes are close to identical (Section 13).

---

## 3. Admin Workflow

1. **Create/Edit/Delete Lesson** → straightforward CRUD against `lessons/{lessonId}`, gated by `role == admin` at both the FastAPI route and the Firestore Rule level (defense in depth).
2. **Upload Learning Material** → admin selects a lesson, uploads a PDF/notes/formula sheet → FastAPI streams the file to Cloud Storage, writes a `materials/{materialId}` doc with `ingestionStatus: uploaded`, then triggers a background ingestion task (chunk → embed → store in the vector index) that updates `ingestionStatus` through `chunking` → `embedded` (or `failed`).
3. **Generate Question Bank** → admin clicks "Generate Question Bank" for a quiz. FastAPI immediately creates a `generationJobs` doc (`status: queued`) and returns its ID — it does **not** wait for generation to finish. A background worker then runs the RAG pipeline (Section 5), and Flutter shows live progress via a Firestore snapshot listener on that job doc.
4. **Question Management** → admin can view/edit/delete individual questions **within the currently active bank version** (edits do not create a new version by themselves — see Section 6 for what does), or trigger a full **Regenerate**, which always creates a new version.
5. **No "Publish Quiz" step** → the moment a `questionBankVersions` doc successfully completes with `status: active` and the parent `quizzes/{quizId}.status` flips to `bankReady`, the quiz is immediately visible/attemptable to students in that grade. This is enforced by the student-facing query simply filtering on `status == 'bankReady'` — there is no separate visibility flag to forget to toggle.
6. **Generate Final Quiz** → same async-job pattern as Question Bank generation, scoped to a grade instead of a lesson (Section 9).
7. **View Admin Analytics** → reads from the precomputed `analytics/*` rollup collections (never live aggregation) — Section 15.

---

## 4. Student Workflow

1. Sign up (`fullName`, `email`, `password`, `grade`) → Firebase Auth account created → `users/{uid}` doc created with `role: student`, `currentGrade`, `enrollmentYear`.
2. Log in → bootstrap call → Student Shell.
3. Browse lessons → FastAPI (or a directly-secured Firestore query, since this is non-sensitive) returns `lessons` where `grade == currentGrade && status == 'published'`.
4. Open a lesson → see its quizzes where `status == 'bankReady'`.
5. Start a quiz attempt → Section 7 (Lesson Quiz Workflow).
6. Submit → see score, weak-area breakdown, YouTube recommendations → Section 4 of the Firestore doc + Sections 12–14 below.
7. Once all/most lessons for the grade are complete, attempt the grade's **active** Final Quiz round → Section 9.
8. Grade promotion happens automatically on the scheduled server-side job (unchanged from v1) — not tied to final quiz completion unless you later decide to gate promotion on a passing final quiz score (currently out of scope per your stated flow, flagged here only as a natural future extension).

---

## 5. RAG Question Bank Generation Workflow

Triggered by the `generationJobs` doc created in Admin Workflow step 3.

1. **Retrieve context:** pull all `materials` docs for the lesson with `ingestionStatus == embedded`; retrieve their chunks/embeddings from the vector store.
2. **Plan the bank composition:** decide the target mix up front (e.g. for 200 questions: a spread across `Theory`/`Formula`/`Calculation` and `easy`/`medium`/`hard`) so the generation loop has a concrete quota to fill rather than generating unboundedly.
3. **Generate in batches:** for each quota slot, retrieve the most relevant chunks (RAG retrieval step) and prompt the LLM to produce one question conforming to the required shape (`questionText`, `correctAnswer`, `explanation`, `difficulty`, `marks`, `questionType`, `lessonTag`, `sourceReference`).
4. **Validate before storing:** reject/retry any generated question missing a required field, with `marks <= 0`, or with a `questionType` outside the allowed enum — do not let malformed LLM output reach Firestore.
5. **Write the version:** create the new `questionBankVersions/{versionId}` doc (`status: active`, incrementing `versionNumber`), write all validated questions into its `questions` subcollection, then archive the previous active version (`status: archived`) in the same operation.
6. **Update pointers:** set `quizzes/{quizId}.activeQuestionBankVersionId` to the new version and `status: bankReady`.
7. **Close the job:** update `generationJobs/{jobId}` to `status: completed`, `resultVersionId` set, `progressPercent: 100`.

Update `progressPercent` on the job doc after each batch (step 3) so the Admin UI's live listener shows real movement rather than jumping from 0% to 100%.

---

## 6. Question Bank Versioning Workflow

- **What creates a new version:** only a full **Regenerate**. In-place edits/deletes of individual questions (Admin Workflow step 4) modify documents *within* the current active version and do **not** bump `versionNumber` — they're corrections, not regenerations.
- **What happens on Regenerate:**
  1. New `questionBankVersions` doc created, `status: active`, `versionNumber = previous + 1`.
  2. Previous active version's `status` flips to `archived`, `archivedAt` set — **its `questions` subcollection is left completely untouched.**
  3. `quizzes/{quizId}.activeQuestionBankVersionId` repointed to the new version.
- **What is never touched:** any `quizAttempts` doc that references an old `questionBankVersionId` continues to resolve correctly forever, because that version's questions are archived, not deleted. A student's historical attempt always shows the exact question they were actually asked, even years after ten regenerations.
- **New attempts only ever draw from the current active version** — old versions are read-only history from that point on.

---

## 7. Lesson Quiz Workflow (Attempt Time)

1. Student taps "Start Quiz" → FastAPI checks `quizProgress/{quizId}` inside a **transaction**: if `attemptsUsed >= maxAttempts`, reject with "attempts exhausted"; otherwise proceed.
2. FastAPI reads the quiz's `activeQuestionBankVersionId`, then randomly selects `questionsPerAttempt` questions from that version's `questions` subcollection (Section 8 — Randomization Strategy).
3. FastAPI strips `correctAnswer` and `explanation` from each selected question, shuffles each question's `options` array, and returns the sanitized set to Flutter along with a server-generated `attemptId` placeholder (the actual `quizAttempts` doc is written on submission, not on start, to avoid orphaned "started but never touched" docs cluttering the collection — alternatively write it on start with `status: inProgress` if you want abandonment tracking; either is valid, pick one and be consistent).
4. Student answers, submits → FastAPI grades server-side against the *unstripped* question docs (fetched again server-side, never trusted from the client), computes `score`, builds the `answers[]` array (with `isCorrect`, `lessonTag`, `questionType`, `difficulty` denormalized per answer), and writes the final `quizAttempts/{attemptId}` doc with `questionBankVersionId` set to the version used.
5. In the same transaction/batch: increment `quizProgress/{quizId}.attemptsUsed`, extend `usedQuestionIds` with this attempt's question IDs, update `bestScore`/`bestAttemptId` if improved, and lock (`isLocked: true`) if `attemptsUsed` now equals `maxAttempts`.
6. Kick off the async post-submission pipeline (Section 4 of the Firestore doc: `wrongQuestions`, `weakTopics`, `feedback`, `youtubeRecommendations`, plus the new `analytics/*` rollups — Section 15).

---

## 8. Question Randomization Strategy

Goal: different questions, different order, different option order, minimal repeats — across up to 3 attempts, drawn from a bank of 150–300.

1. **Question selection:** fetch the full list of question IDs in the active bank version (a lightweight document-ID-only read). Exclude IDs already in `quizProgress/{quizId}.usedQuestionIds` where possible; if the remaining pool is smaller than `questionsPerAttempt` (likely by attempt 3, since 3 × 20 = 60 questions against a 150–300 bank is usually fine — but flag this explicitly for small banks), allow controlled repeats rather than failing the attempt, prioritizing least-recently-used questions first.
2. **Difficulty/type balance (recommended, optional):** rather than pure random sampling, sample within the same `easy`/`medium`/`hard` and `Theory`/`Formula`/`Calculation` proportions the bank was generated with, so no attempt is accidentally all-hard or all-Theory.
3. **Order shuffle:** shuffle the selected question list's presentation order per attempt — this is ephemeral (computed at serve time), never persisted, since grading only needs to know *which question* was answered, not its on-screen position.
4. **Option shuffle:** for each question with an `options` array, shuffle it independently per attempt before sending to the client. Also ephemeral — the student's submitted answer is graded by comparing the *content* of the selected option to `correctAnswer`, never by comparing a position/letter, so shuffling never needs to be reversed or stored.

---

## 9. Final Quiz Workflow

1. Admin triggers **Generate Final Quiz** for a grade → `generationJobs` doc created (`jobType: finalQuizGeneration`, `targetGrade`).
2. Background worker gathers **all published lessons for that grade** (`coveredLessonIds`), retrieves RAG context across all of them, and prompts the LLM to generate a cross-lesson question set where **every question is mandatorily tagged with its `lessonTag` and `sourceLessonId`** — this is validated exactly like bank generation (Section 5, step 4): reject/retry any question missing `lessonTag`.
3. Writes a new `finalQuizzes/{grade}-round{N}` doc (`status: active`, `roundNumber = previous + 1` for that grade) and its `questions` subcollection; archives the previous active round for that grade (`status: archived`).
4. Student starts the final quiz → FastAPI fetches the **current active round** for the student's grade, sanitizes (strips answers, shuffles options — same as lesson quizzes), serves it.
5. Submission, grading, and the `finalQuizAttempts` write follow the same pattern as lesson quizzes (Section 7, steps 4–6), writing `finalQuizId` and `roundNumber` onto the attempt doc so it's permanently pinned to the exact round attempted.

---

## 10. Final Quiz Versioning Strategy (Rounds)

Identical philosophy to Question Bank versioning (Section 6), applied per grade instead of per quiz:

- Exactly one `finalQuizzes` doc per grade has `status: active` at any time; all others for that grade are `archived`.
- Regeneration never edits or deletes a prior round — it always creates `round N+1` and archives `round N`.
- `finalQuizAttempts.finalQuizId` + `roundNumber` mean a student's final exam history is permanently auditable, even after the grade's final quiz has been regenerated multiple times since.
- If you later want "the final quiz changes each academic year," this is already the mechanism — a new round *is* a new year's final quiz; no separate "academic year" collection is needed unless you want to explicitly group rounds by year for reporting.

---

## 11. Student Attempt Workflow (Cross-Cutting Summary)

Both lesson-quiz and final-quiz attempts follow the same shape end-to-end: **start → serve sanitized questions → submit → server-side grade → write attempt → async post-processing.** The only structural differences are (a) which collection the attempt is written to (`quizAttempts` vs `finalQuizAttempts`), and (b) that a final-quiz attempt's `answers[]` spans every `lessonTag` in the grade rather than just one. This symmetry is intentional — it's what let the post-submission analytics (Section 4 of the Firestore doc) reuse one pipeline for both.

---

## 12. Feedback Workflow

1. Triggered as part of the async post-submission pipeline for both quiz types.
2. Backend assembles a compact summary of the attempt — score, list of wrong `lessonTag`/`questionType` pairs, and (for final quiz) which lessons scored worst — and sends it to the LLM with a prompt to produce encouraging, specific, actionable feedback (not a generic "study more").
3. Result stored in `users/{uid}/feedback/{feedbackId}` with `strengths`/`weaknesses` arrays plus a natural-language `feedbackText`, linked back to the `attemptId`.
4. Flutter displays this immediately after the score screen, before the weak-topic/recommendation breakdown, so the emotional framing lands before the "here's what to fix" detail.

---

## 13. Weak Topic Detection Workflow

*(Full mechanics already specified in the Firestore doc, Section 4 — summarized here for completeness of the numbered list you asked for.)*

- Every wrong answer, from either quiz type, upserts `weakTopics/{lessonTag}` on two axes: overall `weaknessScore` (ranks **which lessons** are weak — meaningful mainly post-final-quiz, since a lesson quiz only ever has one lesson tag) and `byQuestionType` (ranks **which of Theory/Formula/Calculation** is weak within that lesson — meaningful for both quiz types).
- `performanceSummary` aggregates `byQuestionType` across the whole syllabus, answering "is this student generally weak at Calculation regardless of topic."

---

## 14. YouTube Recommendation Workflow

1. Once `weakTopics/{lessonTag}.weaknessScore` crosses a configured threshold (e.g. > 0.4) after an upsert, the same background task queries the **YouTube Data API v3** (`search.list`) using the lesson's title/tag as the query, optionally filtered by relevance/duration/language.
2. Top results are filtered for basic quality signals (view count, channel legitimacy heuristics you choose) and written to `users/{uid}/youtubeRecommendations/{recId}` with `lessonTag`, `videoId`, `title`, `channelName`, `thumbnailUrl`, `videoUrl`, `relevanceScore`.
3. Recommendations are **refreshed, not endlessly appended** — if a recommendation for a given `lessonTag` already exists and is recent (e.g. < 30 days old), skip the API call rather than re-querying YouTube on every single wrong answer; this both saves API quota and avoids a recommendations list that grows unbounded per topic.
4. Flutter surfaces these directly under the corresponding weak topic on both the lesson-quiz result screen and the final-quiz result screen (Section 11's shared-UI point).

---

## 15. Admin Analytics Workflow

1. The same async post-submission task that updates `weakTopics` (Section 13) also updates the three rollup collections defined in the Firestore doc: `analytics/lessonStats/{lessonId}`, `analytics/quizStats/{quizId}`, `analytics/questionStats/{questionId}` — incrementing attempt counts, running averages, and per-question incorrect tallies.
2. **Most Difficult Lessons/Questions** are simply the rollup docs sorted by `averageScorePercent` ascending / incorrect-rate descending — no live aggregation query needed.
3. **Student Progress / Quiz Attempts / Average Marks** views in the Admin dashboard read directly from `quizAttempts`/`finalQuizAttempts` via **Collection Group queries** (e.g. "all attempts for quiz X across all students") when the rollups don't already cover the exact slice needed.
4. For deeper historical/cohort analysis beyond what these rollups support, use the Firestore → BigQuery Extension (Firestore doc, Section 9) rather than expanding the rollup schema indefinitely.

---

## 16. Backend Responsibilities (FastAPI)

FastAPI is the **only** writer for anything that affects grading, scores, attempts, question banks, or final quiz content. Concretely, it owns:

- Firebase ID token verification and role extraction on every request.
- All lesson/material CRUD and file upload handling (Cloud Storage + Firestore metadata).
- Orchestrating RAG ingestion (chunking, embedding, vector store writes) and question/final-quiz generation via the LLM, both as background jobs.
- Serving sanitized (answer-stripped, shuffled) quiz/final-quiz payloads — the client never has a code path to read raw question docs.
- Server-side grading, attempt-limit enforcement (via transactions), and all writes to `quizAttempts`, `finalQuizAttempts`, `quizProgress`, `wrongQuestions`, `weakTopics`, `feedback`, `youtubeRecommendations`, `performanceSummary`, and `analytics/*`.
- The scheduled grade-promotion job.
- Enforcing that `role` can only ever be set to `student` via the public signup endpoint.

---

## 17. FastAPI API Architecture (Endpoint Map — no implementation)

**Auth / Bootstrap**
- `POST /auth/signup` — student-only, creates Firebase Auth user + `users/{uid}` doc with `role: student`.
- `GET /auth/bootstrap` — verifies token, returns `{role, fullName, currentGrade?}`.

**Admin — Content**
- `POST /admin/lessons`, `PATCH /admin/lessons/{id}`, `DELETE /admin/lessons/{id}`
- `POST /admin/lessons/{id}/materials` — upload → Storage + `materials` doc + ingestion trigger
- `GET /admin/lessons/{id}/materials/{materialId}/status` — ingestion status polling (or Flutter listens to the doc directly)

**Admin — Question Bank**
- `POST /admin/quizzes/{quizId}/generate-question-bank` — creates a `generationJobs` doc, enqueues background work, returns `jobId` immediately
- `GET /admin/generation-jobs/{jobId}` — status (or Flutter listens to the doc directly)
- `GET /admin/quizzes/{quizId}/questions` — view current active bank
- `PATCH /admin/questions/{questionId}`, `DELETE /admin/questions/{questionId}` — in-place edits (no new version)

**Admin — Final Quiz**
- `POST /admin/grades/{grade}/generate-final-quiz` — same async-job pattern
- `GET /admin/final-quizzes/{grade}/rounds` — version history

**Admin — Analytics**
- `GET /admin/analytics/lessons`, `GET /admin/analytics/quizzes`, `GET /admin/analytics/questions`
- `GET /admin/analytics/students/{uid}` — individual student progress view

**Student — Lessons & Quizzes**
- `GET /student/lessons` — grade-filtered
- `POST /student/quizzes/{quizId}/start` — attempt-limit check + sanitized question serving
- `POST /student/quizzes/{quizId}/submit` — server-side grading + attempt write + triggers async pipeline
- `GET /student/quizzes/{quizId}/wrong-questions`, `GET /student/weak-topics`, `GET /student/recommendations`

**Student — Final Quiz**
- `POST /student/final-quiz/start`, `POST /student/final-quiz/submit` — mirrors lesson-quiz endpoints exactly (Section 11's symmetry)

---

## 18. Security Recommendations (Cross-Reference)

All rule-level detail lives in the Firestore doc, Section 10. The one point worth restating at the system level: **Firestore Security Rules and FastAPI authorization checks are both required, not either/or** — Rules protect against a compromised or modified Flutter client talking to Firestore directly; FastAPI checks protect the business logic (attempt limits, grading integrity, generation triggers) that Rules alone can't express. Treat them as two independent layers, not a single control.

---

## 19. Scalability Recommendations (Cross-Reference)

All detail lives in the Firestore doc, Section 9. At the system level, the two decisions that matter most as this scales past a single class/cohort: (1) generation work (bank + final quiz) must be queue-driven, never inline, or admin actions will start timing out as banks grow toward the 300-question end of your range; (2) admin analytics must read from precomputed rollups, never live collection-group aggregations, once the student count grows past a few hundred.

