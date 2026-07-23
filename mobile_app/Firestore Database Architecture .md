# Firestore Database Architecture 
### AI-Powered Grade-Based Learning Platform (Flutter + FastAPI + RAG) --- 
## 1. Design Principles Behind This Schema 
Five decisions shape everything below — flagged here because they recur 
throughout the document: 
1. **Hierarchy mirrors real containment.** Lesson → Quiz → Question Bank 
Version → Question is a strict one-to-many "owns" relationship with no 
cross-referencing, so it is modeled as **nested subcollections**, not 
top-level collections joined by IDs. Firestore has no joins, so this 
saves reads. 
2. **Student-generated data lives under `users/{uid}`.** Attempts, wrong 
answers, feedback, weak topics, and recommendations are all private, per
student, unbounded-growth data — subcollections under the owning user, 
never arrays inside the user document. 
3. **All writes that affect grading, scores, attempt counts, or 
question/final-quiz content happen server-side (FastAPI + Admin SDK), 
never directly from the Flutter client.** This is a security-critical 
decision, not a style choice — explained fully in Section 10. 
4. **RAG/LLM outputs are treated as generated content with provenance 
fields** (`generatedBy`, `sourceLessonId`/`sourceReference`, `lessonTag`, 
`llmModelUsed`), so every question — bank or final quiz — can be traced 
back to its source material **and** to the specific LLM that generated 
it. The final quiz is generated using **RAG**, the same as the lesson
wise question bank — curriculum content is retrieved first, then given to 
the LLM as grounding context; the LLM is never relied on for its own 
internal knowledge alone. Because `llmModelUsed` is just a field, 
comparing different LLMs (GPT, DeepSeek, Claude, etc.) is a **backend 
configuration choice at generation time**, not a separate data structure 
— generate one round/version per model, and the model that produced it is 
always visible on the resulting document. 
5. **Nothing generated is ever overwritten in place — it's versioned.** 
Question Banks and Final Quizzes are both **append-only, version-stamped, 
and archived** rather than edited destructively, because student attempts 
must forever be traceable to the exact version of content they were 
tested on. --- 
## 2. Collection Hierarchy (Tree View) 
``` 
users (collection)                                       
OR admin] 
└── {uid} 
├── gradeHistory (subcollection) 
│    
└── {historyId} 
├── quizProgress (subcollection) 
│    
└── {quizId} 
├── quizAttempts (subcollection) 
 [role: student 
      │    └── {attemptId} 
      ├── wrongQuestions (subcollection) 
      │    └── {wrongQId} 
      ├── feedback (subcollection) 
      │    └── {feedbackId} 
      ├── weakTopics (subcollection) 
      │    └── {lessonTag} 
      ├── youtubeRecommendations (subcollection) 
      │    └── {recId} 
      ├── finalQuizAttempts (subcollection) 
      │    └── {attemptId} 
      └── performanceSummary (single doc) 
 
lessons (collection) 
 └── {lessonId}                                            [doc: lesson 
metadata, includes grade] 
      ├── materials (subcollection)                        [admin
uploaded PDFs/notes, RAG source] 
      │    └── {materialId} 
      └── quizzes (subcollection) 
           └── {quizId}                                    [doc: quiz 
metadata + active bank pointer] 
                └── questionBankVersions (subcollection)    [versioned 
question banks] 
                     └── {versionId} 
                          └── questions (subcollection) 
                               └── {questionId}             [doc: RAG
generated bank question] 
 
finalQuizzes (collection)                                  [one doc PER 
ROUND/VERSION, per grade] 
 └── {finalQuizId}                                          [e.g. 
"grade10-round2" — doc: version/round metadata] 
      └── questions (subcollection) 
           └── {questionId}                                [doc: LLM
generated, tagged by lesson] 
 
generationJobs (collection)                                 [async job 
tracking] 
 └── {jobId}                                                [question 
bank OR final quiz generation job] 
 
analytics (collection)                                      [admin rollup 
docs, maintained by backend] 
 ├── lessonStats (subcollection) 
 │    └── {lessonId} 
 ├── quizStats (subcollection) 
 │    └── {quizId} 
 └── questionStats (subcollection) 
      └── {questionId} 
``` 
 
**Why not put `quizAttempts` etc. as top-level collections with a 
`studentId` field?** 
Both are viable in Firestore. Subcollections win here because: (a) 
security rules become a one-line `request.auth.uid == uid` check instead 
of a filtered query, (b) all of a student's data is naturally colocated 
for "load my dashboard" reads, and (c) you can still run cross-student 
analytics using **Collection Group Queries** — so you lose nothing on the 
admin/analytics side, and the `analytics` collection removes the need to 
run those expensive queries live anyway (Section 9). --- 
## 3. Detailed Document Schemas 
### 3.1 `users/{uid}` 
Document ID = Firebase Auth UID (never auto-generated — this is one of 
the deliberate cases where you choose the ID so `request.auth.uid` maps 
1:1). **This single collection holds both roles** — do not create a 
separate `admins` collection; distinguish by `role`. 
| Field | Type | Notes | 
|---|---|---| 
| `role` | string | `student` \| `admin`. Mirrored from the Firebase Auth 
custom claim so Firestore Security Rules can read it directly without a 
network round-trip. | 
| `fullName` | string | | 
| `email` | string | mirrors Firebase Auth, kept for querying/display 
without extra Auth calls | 
| `currentGrade` | number (9/10/11) | student only; null/absent for admin 
docs. **Client cannot write this field** — see Section 10 | 
| `enrollmentYear` | number | student only; e.g. 2026, anchor for auto
promotion logic | 
| `lastPromotedAt` | timestamp \| null | student only; prevents double
promotion in the same cycle | 
| `status` | string | `active` / `graduated` / `suspended` | 
| `createdAt` | timestamp | | 
| `updatedAt` | timestamp | | 
>     
**Admin accounts are never created through the public Student Sign 
Up flow.** They are provisioned out-of-band (Firebase Console, a seed 
script, or a super-admin-only endpoint) and given the `admin` custom 
claim explicitly. Public self-signup can only ever produce `role: 
student`. This is restated in Section 10 because it's the single most 
important access-control decision in the whole system. 
### 3.2 `users/{uid}/gradeHistory/{historyId}` 
Audit trail so grade promotions are traceable (support tickets, disputes, 
analytics). 
| Field | Type | Notes | 
|---|---|---| 
| `fromGrade` | number | | 
| `toGrade` | number | | 
| `promotedAt` | timestamp | | 
| `triggeredBy` | string | `scheduledJob` / `adminOverride` | 
### 3.3 `lessons/{lessonId}` 
| Field | Type | Notes | 
|---|---|---| 
| `title` | string | | 
| `subject` | string | e.g. "Physics" | 
| `grade` | number (9/10/11) | **primary access-control field** | 
| `lessonTag` | string | short unique slug, e.g. `phy-g10-motion` — 
reused everywhere for tagging | 
| `description` | string | | 
| `order` | number | display sequence within grade | 
| `status` | string | `draft` / `published` | 
| `createdBy` | string | admin `uid` who created the lesson | 
| `lastEditedBy` | string | admin `uid` of most recent edit | 
| `materialsCount` | number | denormalized count of uploaded materials, 
for admin UI badges | 
| `createdAt` | timestamp | | 
| `updatedAt` | timestamp | | 
### 3.4 `lessons/{lessonId}/materials/{materialId}` 
Admin-uploaded source documents that feed the RAG pipeline for this 
lesson. The actual file lives in Firebase Cloud Storage — this document 
is metadata plus a pointer. 
| Field | Type | Notes | 
|---|---|---| 
| `fileName` | string | original uploaded file name | 
| `materialType` | string | `pdf` \| `theoryNotes` \| `formulaSheet` \| 
`calculationSheet` \| `other` | 
| `storagePath` | string | Firebase Cloud Storage path | 
| `fileSizeBytes` | number | | 
| `ingestionStatus` | string | `uploaded` → `chunking` → `embedded` → 
`failed` — tracks RAG preprocessing | 
| `chunkCount` | number | how many chunks this material produced once 
embedded | 
| `uploadedBy` | string | admin `uid` | 
| `uploadedAt` | timestamp | | 
| `lastProcessedAt` | timestamp \| null | | 
### 3.5 `lessons/{lessonId}/quizzes/{quizId}` 
A quiz is a **container for versioned question banks** — it does not 
itself hold questions. 
| Field | Type | Notes | 
|---|---|---| 
| `title` | string | | 
| `lessonId` | string | denormalized back-reference (cheap, avoids a 
parent lookup in the API layer) | 
| `maxAttempts` | number | fixed at `3` per your requirement, kept as a 
field (not hardcoded) so it's configurable later | 
| `questionsPerAttempt` | number | how many questions the backend 
randomly draws from the active bank per attempt (e.g. 20) | 
| `activeQuestionBankVersionId` | string | points to the currently live 
version in `questionBankVersions`; this is the *only* version new 
attempts are drawn from | 
| `status` | string | `noBankGenerated` \| `bankReady` \| `regenerating` 
— there is no manual "Publish Quiz" step; a quiz becomes attemptable 
automatically the moment `status` flips to `bankReady` | 
| `createdAt` | timestamp | | 
| `updatedAt` | timestamp | | 
### 3.6 
`lessons/{lessonId}/quizzes/{quizId}/questionBankVersions/{versionId}` 
One document per generation/regeneration event. **Never deleted, never 
edited after archival** — this is what makes old student attempts 
permanently traceable. 
| Field | Type | Notes | 
|---|---|---| 
| `versionNumber` | number | 1, 2, 3 … monotonically increasing per quiz 
| 
| `status` | string | `active` (exactly one per quiz at any time) \| 
`archived` | 
| `totalQuestions` | number | e.g. 150–300 | 
| `generatedBy` | string | `RAG` | 
| `generationJobId` | string | back-reference to the `generationJobs` doc 
that produced it | 
| `sourceMaterialIds` | array\<string\> | which `materials` docs were 
used as RAG input for this generation run | 
| `createdAt` | timestamp | | 
| `archivedAt` | timestamp \| null | | 
### 3.7 `.../questionBankVersions/{versionId}/questions/{questionId}` 
| Field | Type | Notes | 
|---|---|---| 
| `questionText` | string | | 
| `questionType` | string | `Theory` \| `Formula` \| `Calculation` | 
| `options` | array\<string\> \| null | null for open-response 
Theory/Calculation questions | 
| `correctAnswer` | string | reference answer (used for grading, incl. 
LLM-assisted grading of Theory answers) | 
| `explanation` | string | shown after submission | 
| `lessonTag` | string | denormalized from parent lesson | 
| `difficulty` | string | `easy` / `medium` / `hard` | 
| `marks` | number | | 
| `generatedBy` | string | `RAG` (traceability) | 
| `sourceReference` | string | pointer(s) into the RAG vector store — 
**not the embeddings themselves** (see Section 9) | 
| `createdAt` | timestamp | | 
>     
**This subcollection should never be read directly by the Flutter 
client.** See Section 10 — the client must call a FastAPI endpoint that 
fetches these server-side and strips `correctAnswer`/`explanation` before 
returning the quiz. This is the raw, unshuffled, answer-included bank. 
### 3.8 `users/{uid}/quizProgress/{quizId}` 
Doc ID is deliberately the `quizId` itself → O(1) lookup, no query 
needed, and it's the perfect place to **atomically enforce the 3-attempt 
limit** via a Firestore transaction. 
| Field | Type | Notes | 
|---|---|---| 
| `quizId` | string | | 
| `lessonId` | string | | 
| `attemptsUsed` | number | 0–3 | 
| `isLocked` | boolean | true once `attemptsUsed == maxAttempts` | 
| `bestScore` | number | | 
| `bestAttemptId` | string | | 
| `usedQuestionIds` | array\<string\> | union of question IDs served 
across this student's previous attempts on this quiz, used by the random
selection algorithm (Section 8, System Architecture Extension) to 
minimize repeats | 
| `lastAttemptAt` | timestamp | | 
### 3.9 `users/{uid}/quizAttempts/{attemptId}` 
| Field | Type | Notes | 
|---|---|---| 
| `quizId` | string | | 
| `lessonId` | string | | 
| `questionBankVersionId` | string | exactly which bank version this 
attempt's questions were drawn from — critical for audit if the bank is 
later regenerated | 
| `attemptNumber` | number | 1, 2, or 3 | 
| `answers` | array\<map\> | `{questionId, studentAnswer, isCorrect, 
marksAwarded, lessonTag, difficulty, questionType}` — denormalized per
answer so wrong-answer/weak-topic aggregation doesn't require re-fetching 
the question doc | 
| `score` | number | | 
| `totalMarks` | number | | 
| `timeTakenSeconds` | number | | 
| `startedAt` | timestamp | | 
| `submittedAt` | timestamp | | 
| `status` | string | `completed` / `abandoned` | 
### 3.10 `users/{uid}/wrongQuestions/{wrongQId}` 
One doc per missed question, kept **separately** per your requirement 
(not just a filter on attempts) so revision screens can query this 
directly without scanning attempts. 
| Field | Type | Notes | 
|---|---|---| 
| `questionId` | string | ref to source question | 
| `lessonId` | string | | 
| `quizId` | string | | 
| `attemptId` | string | | 
| `questionBankVersionId` | string | which bank version this question 
came from | 
| `questionText` | string | snapshot, in case the source question is 
later edited or its version archived | 
| `studentAnswer` | string | | 
| `correctAnswer` | string | | 
| `explanation` | string | | 
| `questionType` | string | | 
| `lessonTag` | string | | 
| `difficulty` | string | | 
| `reviewed` | boolean | | 
| `createdAt` | timestamp | | 
### 3.11 `users/{uid}/feedback/{feedbackId}` 
| Field | Type | Notes | 
|---|---|---| 
| `attemptId` | string | | 
| `quizId` | string | | 
| `feedbackText` | string | LLM-generated | 
| `strengths` | array\<string\> | | 
| `weaknesses` | array\<string\> | | 
| `recommendedTopics` | array\<string\> | `lessonTag`s the LLM suggests 
revising — the "learning suggestions" part of feedback | 
| `llmModelUsed` | string | which model generated this feedback (e.g. 
`gpt-5`, `deepseek-v4`, `claude-sonnet-5`) — lets you swap the feedback
generation model per request and know afterward which one produced which 
feedback, without needing a separate comparison system | 
| `generatedAt` | timestamp | | 
### 3.12 `users/{uid}/weakTopics/{lessonTag}` 
Doc ID = `lessonTag` itself → natural upsert target; a Cloud 
Function/background task increments this on every wrong answer instead of 
you running aggregation queries. 
**Weakness is tracked on two dimensions, not one** — which *lesson* the 
student is weak in, and which *question type* 
(Theory/Formula/Calculation) they're weak in *within* that lesson. A 
lesson quiz only ever touches one `lessonTag`, so for a lesson quiz the 
"weak lesson" question is trivial (there's only one) and the interesting 
signal is the question-type breakdown. The final quiz spans every lesson, 
so there the "which lessons are weak" question becomes meaningful too — 
answered by comparing `weaknessScore` across every `weakTopics` doc 
touched by that attempt. 
| Field | Type | Notes | 
|---|---|---| 
| `lessonTag` | string | | 
| `lessonId` | string | | 
| `incorrectCount` | number | total wrong, across all quiz types | 
| `totalAttempted` | number | total answered, across all quiz types | 
| `weaknessScore` | number | `incorrectCount / totalAttempted` — ranks 
weak **lessons** (final-quiz view) | 
| `byQuestionType` | map | `{ Theory: {incorrectCount, totalAttempted, 
weaknessScore}, Formula: {...}, Calculation: {...}}` — ranks weak 
**question types** within this lesson (lesson-quiz view, and drill-down 
on final-quiz view) | 
| `contributingQuizTypes` | array\<string\> | `["lessonQuiz", 
"finalQuiz"]` — which sources have fed this doc, purely informational | 
| `lastUpdated` | timestamp | | 
> A Cloud Function (or FastAPI background task) triggered on **every** 
attempt submission — lesson quiz or final quiz — walks `answers[]`, and 
for each wrong answer does an upsert on `weakTopics/{lessonTag}`: 
increments `incorrectCount`/`totalAttempted` at the top level, **and** 
increments the matching bucket inside `byQuestionType`. Same function, 
same trigger, same target doc — the only difference is which quiz type 
called it, recorded in `contributingQuizTypes` for traceability. 
### 3.13 `users/{uid}/youtubeRecommendations/{recId}` 
| Field | Type | Notes | 
|---|---|---| 
| `lessonTag` | string | | 
| `videoId` | string | | 
| `title` | string | | 
| `channelName` | string | | 
| `thumbnailUrl` | string | | 
| `videoUrl` | string | | 
| `relevanceScore` | number | | 
| `generatedAt` | timestamp | | 
### 3.14 `finalQuizzes/{finalQuizId}` 
**One document per generated round**, not one per grade — this is what 
preserves final-quiz history exactly like question banks preserve theirs. 
Doc ID convention: `grade{N}-round{M}` (e.g. `grade10-round2`) — human
readable and collision-proof. 
The final quiz is generated using **RAG** — curriculum content is 
retrieved from every lesson in the grade first, and only that retrieved 
context is given to the LLM to write questions from. It is **not** 
generated from the LLM's own internal knowledge alone. 
| Field | Type | Notes | 
|---|---|---| 
| `grade` | number | | 
| `roundNumber` | number | 1, 2, 3 … per grade | 
| `status` | string | `active` (exactly one per grade at any time) \| 
`archived` | 
| `title` | string | | 
| `totalMarks` | number | | 
| `maxAttempts` | number | can differ from lesson quizzes if you want | 
| `coveredLessonIds` | array\<string\> | which lessons this round was 
generated from | 
| `generatedBy` | string | `RAG` — retrieval-augmented, not the LLM's 
internal knowledge alone | 
| `llmModelUsed` | string | which LLM actually generated this round (e.g. 
`gpt-5`, `deepseek-v4`, `claude-sonnet-5`). Since you plan to try 
different LLMs and compare results, this is the field that makes each 
round self-describing about which model produced it — **you don't need a 
separate comparison database for this: generate Round 1 with one model, 
Round 2 with another, and this field alone lets you tell them apart and 
compare quality directly in the app.** | 
| `generationJobId` | string | back-reference to `generationJobs` | 
| `createdAt` | timestamp | | 
| `archivedAt` | timestamp \| null | | 
### 3.15 `finalQuizzes/{finalQuizId}/questions/{questionId}` 
Same shape as 3.7, **with `lessonTag` mandatory** (your requirement) plus 
two extra fields: 
| Field | Type | Notes | 
|---|---|---| 
| `questionText` | string | | 
| `questionType` | string | `Theory` \| `Formula` \| `Calculation` | 
| `options` | array\<string\> \| null | | 
| `correctAnswer` | string | | 
| `explanation` | string | | 
| `lessonTag` | string | **required, not nullable** | 
| `sourceLessonId` | string | which lesson this question was pulled from 
| 
| `difficulty` | string | | 
| `marks` | number | | 
| `generatedBy` | string | `RAG` | 
| `sourceReference` | string | which retrieved chunk(s) this question was 
grounded in | 
| `createdAt` | timestamp | | 
`llmModelUsed` is **not** repeated on every question — it's recorded once 
on the parent `finalQuizzes/{finalQuizId}` doc (3.14), since every 
question in a round was generated by the same model. Repeating it per 
question would be redundant denormalization with no query benefit here. 
### 3.16 `users/{uid}/finalQuizAttempts/{attemptId}` 
Identical shape to 3.9 (`quizAttempts`), kept as a **separate 
subcollection** rather than mixed into `quizAttempts`, because final 
exams have different downstream logic (grade promotion eligibility, 
transcripts, certificates) and you don't want to filter a shared 
collection by type every time. 
| Field | Type | Notes | 
|---|---|---| 
| `finalQuizId` | string | exactly which round/version was attempted | 
| `roundNumber` | number | denormalized for display ("You attempted Round 
2") without a lookup | 
| `attemptNumber` | number | 1, 2, or 3 | 
| `answers` | array\<map\> | same shape as `quizAttempts.answers` | 
| `answerCheckingModel` | string | which LLM checked/graded the answers 
for this attempt (relevant for open-response `Theory` answers, which need 
LLM-assisted grading rather than exact string matching) — recorded per 
attempt so you can compare grading quality across models too | 
| `score` | number | | 
| `totalMarks` | number | | 
| `timeTakenSeconds` | number | | 
| `startedAt` | timestamp | | 
| `submittedAt` | timestamp | | 
| `status` | string | `completed` / `abandoned` | 
### 3.17 `users/{uid}/performanceSummary` (single doc, optional but 
recommended) 
One flat doc (not a subcollection — it's a single running summary) giving 
a lesson-independent view: *"is this student generally weak at 
Calculation questions, regardless of which lesson they're in?"* This is 
the cross-cutting counterpart to `weakTopics`, which is always scoped to 
one lesson. 
| Field | Type | Notes | 
|---|---|---| 
| `byQuestionType` | map | same shape as `weakTopics.byQuestionType`, but 
aggregated across **every** lesson and the final quiz combined | 
| `overallAccuracy` | number | | 
| `lastUpdated` | timestamp | | 
Updated by the same Cloud Function/task described in 3.12, in the same 
write pass — no extra trigger needed. 
### 3.18 `generationJobs/{jobId}` 
Question bank generation (150–300 questions via RAG) and final quiz 
generation (also via RAG, across every lesson in the grade) are both 
slow, async operations — they must never block an admin's HTTP request. 
This collection is how Flutter observes/polls progress. 
| Field | Type | Notes | 
|---|---|---| 
| `jobType` | string | `questionBankGeneration` \| `finalQuizGeneration` 
| 
| `targetLessonId` | string \| null | set for `questionBankGeneration` | 
| `targetQuizId` | string \| null | set for `questionBankGeneration` | 
| `targetGrade` | number \| null | set for `finalQuizGeneration` | 
| `llmModelUsed` | string | which LLM this generation run called (e.g. 
`gpt-5`, `deepseek-v4`, `claude-sonnet-5`) — since this is a config 
choice made by your backend at request time, comparing models is simply a 
matter of re-running the same job with a different model and comparing 
the resulting `questionBankVersions`/`finalQuizzes` round afterward. No 
separate comparison collection is needed for this. | 
| `status` | string | `queued` → `processing` → `completed` \| `failed` | 
| `progressPercent` | number | 0–100, updated incrementally so the Admin 
UI can show a progress bar | 
| `requestedBy` | string | admin `uid` | 
| `resultVersionId` | string \| null | the `questionBankVersions` or 
`finalQuizzes` doc ID produced on success | 
| `errorMessage` | string \| null | | 
| `startedAt` | timestamp | | 
| `completedAt` | timestamp \| null | | 
### 3.19 `analytics/lessonStats/{lessonId}` 
| Field | Type | Notes | 
|---|---|---| 
| `lessonId` | string | | 
| `totalAttempts` | number | across all students, all quizzes under this 
lesson | 
| `averageScorePercent` | number | | 
| `weakStudentCount` | number | students whose 
`weakTopics/{thisLessonTag}.weaknessScore` exceeds your threshold | 
| `mostMissedQuestionIds` | array\<string\> | top N by incorrect rate, 
refreshed periodically | 
| `lastUpdated` | timestamp | | 
### 3.20 `analytics/quizStats/{quizId}` and 
`analytics/questionStats/{questionId}` 
Same rollup pattern as 3.19, scoped one level finer — `quizStats` holds 
per-quiz averages and attempt counts, `questionStats` holds per-question 
incorrect-rate (this is what powers "Most Difficult Questions" in Admin 
Analytics). Both maintained the same way — updated incrementally in the 
same background task described in Section 3.12. 
| Field (both) | Type | Notes | 
|---|---|---| 
| `totalAttempts` / `totalAnswered` | number | | 
| `averageScorePercent` / `incorrectRate` | number | | 
| `lastUpdated` | timestamp | | --- 
## 4. Post-Submission Analytics Workflow (Lesson Quiz vs. Final Quiz) 
Both flows use the **same underlying trigger and the same target 
collections** — only what gets *displayed* in Flutter differs, and the 
final quiz additionally feeds the admin `analytics/*` rollups at a 
broader scope. 
**On any `quizAttempts` (lesson quiz) submission:** 
1. Grade the attempt server-side → write `score`, `answers[]` (with 
`isCorrect`, `lessonTag`, `questionType` per answer) and 
`questionBankVersionId` to `quizAttempts/{attemptId}`. 
2. For each wrong answer → upsert `wrongQuestions` and 
`weakTopics/{lessonTag}` (both the top-level counters and the 
`byQuestionType` bucket) + `performanceSummary` + 
`analytics/lessonStats`, `quizStats`, `questionStats`. 
3. **Flutter display for a lesson quiz result:** since only one 
`lessonTag` is involved, pull `weakTopics/{thatLessonTag}.byQuestionType` 
and show *"You're weak in: Formula questions (2/3 wrong), Calculation 
(1/4 wrong)"* — a question-type breakdown for that single lesson. 
4. Optionally fetch `youtubeRecommendations` where `lessonTag == 
thatLessonTag` if `weaknessScore` crosses your chosen threshold (e.g. > 
0.4). 
**On a `finalQuizAttempts` submission:** 
1. Same grading + same upserts as above, but now `answers[]` spans 
**every** `lessonTag` in the syllabus, so the upsert loop touches 
multiple `weakTopics` docs, not just one. 
2. **Flutter display for the final quiz result:** - **Weak lessons:** query all `weakTopics` docs the attempt touched 
(you already have the list of `lessonTag`s from `answers[]`), sort by 
`weaknessScore` descending → *"Your weakest topics: Thermodynamics, 
Vectors, Circular Motion."* - **Weak question types (per weak lesson):** drill into each weak 
lesson's `byQuestionType` map → *"In Thermodynamics, you're weak 
specifically in Calculation-type questions."* - **YouTube recommendations:** for each lesson in the "weak lessons" 
list, fetch/generate a `youtubeRecommendations` entry keyed by that 
`lessonTag` and display it alongside the topic. 
3. This is also the natural place to refresh `performanceSummary` since 
the final quiz gives the broadest sample of question types across the 
whole syllabus. 
**Why this works with the schema as designed:** every `answers[]` entry 
already carries `lessonTag` **and** `questionType` (Section 3.9), so no 
extra read-back to the `questions` subcollection is needed to do this 
analysis — the attempt document is self-sufficient. This is exactly why 
those two fields were denormalized into `answers[]` in the first place. --- 
## 5. Relationships Map 
``` 
users.role                
navigation) 
users.currentGrade        
lessons.lessonTag         
quiz questions, 
 ──gates──▶  Admin UI vs. Student UI (Flutter 
 ──filters──▶  lessons.grade 
 ──denormalized into──▶  bank questions, final 
wrongQuestions, 
weakTopics, youtubeRecommendations 
lessons ──1:N──▶ materials 
lessons ──1:N──▶ quizzes ──1:N──▶ questionBankVersions ──1:N──▶ 
questions 
quizzes.activeQuestionBankVersionId ──points to──▶ 
questionBankVersions/{versionId}  (exactly one active) 
quizAttempts.questionBankVersionId ──pins the attempt to──▶ the exact 
version drawn from 
generationJobs.resultVersionId ──produces──▶ questionBankVersions/{id} 
OR finalQuizzes/{id} 
finalQuizzes (one doc per round) ──status: active──▶ the only round new 
attempts can start 
finalQuizAttempts.finalQuizId ──pins the attempt to──▶ the exact round 
attempted 
weakTopics ◀──aggregated from── wrongQuestions (from both quizAttempts 
and finalQuizAttempts) 
youtubeRecommendations.lessonTag ◀──generated from── weakTopics 
analytics/* ◀──rolled up from── quizAttempts, finalQuizAttempts, 
wrongQuestions (background tasks, batched) 
``` 
**Note on reference type:** store all cross-document links (`lessonId`, 
`quizId`, `questionId`, `attemptId`, `versionId`, `finalQuizId`) as 
**plain string IDs**, not Firestore `DocumentReference` fields. Since 
FastAPI (via Pydantic models) is your access layer, plain strings 
serialize cleanly to JSON without special handling, and you avoid 
Firestore's reference-type quirks in queries. --- 
## 6. Recommended Indexes 
**Single-field indexes** are automatic in Firestore — you only need to 
declare **composite** and **collection group** indexes explicitly. 
| Query pattern | Index type | Fields | 
|---|---|---| 
| List published lessons for a grade, in order | Composite | `lessons`: 
`grade` ASC, `status` ASC, `order` ASC | 
| Get the active question bank version for a quiz | Composite | 
`questionBankVersions`: `status` ASC, `versionNumber` DESC | 
| Pull questions by tag+difficulty for adaptive review | Composite | 
`questions` (subcollection): `lessonTag` ASC, `difficulty` ASC | 
| Get the active final quiz round for a grade | Composite | 
`finalQuizzes`: `grade` ASC, `status` ASC, `roundNumber` DESC | 
| Filter final quiz questions by tag | Composite | 
`finalQuizzes/*/questions`: `lessonTag` ASC, `difficulty` ASC | 
| Student's wrong questions by topic, most recent first | Composite | 
`wrongQuestions` (subcollection): `lessonTag` ASC, `createdAt` DESC | 
| Admin: most-missed topics across all students | Collection Group | 
`wrongQuestions`: `lessonTag` ASC, `createdAt` DESC | 
| Admin: attempt analytics across all students for one quiz | Collection 
Group | `quizAttempts`: `quizId` ASC, `submittedAt` DESC | 
| Admin: identify widespread weak topics | Collection Group | 
`weakTopics`: `lessonTag` ASC, `weaknessScore` DESC | 
| Admin: poll pending/active generation jobs | Composite | 
`generationJobs`: `status` ASC, `startedAt` DESC | 
| Admin: jobs requested by a specific admin | Composite | 
`generationJobs`: `requestedBy` ASC, `startedAt` DESC | 
| Admin: rank most difficult lessons | Single-field (auto) | 
`analytics/lessonStats`: `averageScorePercent` ASC | 
| Admin: rank most difficult questions globally | Single-field (auto) | 
`analytics/questionStats`: `incorrectRate` DESC | 
| Student dashboard: recent recommendations | Single-field (auto) | 
`youtubeRecommendations`: `generatedAt` DESC | 
Collection Group indexes are what let you keep the "subcollection under 
user" model (good for security rules and per-student reads) while still 
running platform-wide analytics later — enable them proactively even if 
you don't need the admin dashboard on day one. 
--- 
## 7. Naming Conventions - **Collections:** plural, `lowerCamelCase` — `users`, `lessons`, 
`materials`, `quizzes`, `questionBankVersions`, `questions`, 
`quizAttempts`, `wrongQuestions`, `weakTopics`, `youtubeRecommendations`, 
`finalQuizzes`, `generationJobs`, `analytics`. - **Fields:** `lowerCamelCase` throughout — matches both Dart (Flutter) 
and Python (via Pydantic aliasing) conventions. - **Timestamps:** always suffixed `At` (`createdAt`, `updatedAt`, 
`startedAt`, `submittedAt`, `generatedAt`, `archivedAt`) — never mix 
`date`, `time`, `at` styles. - **Foreign keys:** always suffixed `Id` (`lessonId`, `quizId`, 
`questionId`, `attemptId`, `versionId`, `finalQuizId`, `jobId`) and 
stored as plain strings. - **Booleans:** prefixed `is`/`has` (`isLocked`, `isCorrect`, 
`hasReviewed`) for readability in security rules and code. - **Document IDs:** use Firestore auto-generated IDs everywhere 
**except** the deliberate cases — `users/{uid}` (= Firebase Auth UID), 
`quizProgress/{quizId}` (= quiz ID, for O(1) lookup), 
`weakTopics/{lessonTag}` (= tag, for upsert), `finalQuizzes/{grade}
round{N}` (human-readable and collision-proof). - **`lessonTag`:** a short, stable, human-readable slug (e.g. `phy-g10
motion`), not a random ID — it's your cross-cutting index key, so treat 
it like a controlled vocabulary, ideally generated/validated centrally 
rather than free-typed per lesson. - **Versioned collections** (`questionBankVersions`, `finalQuizzes`-as
rounds) always carry a `status` field with exactly the two values 
`active`/`archived`, and a monotonically increasing 
`versionNumber`/`roundNumber` — never reuse or decrement these. - **Job tracking docs** (`generationJobs`) use `status` values from a 
fixed enum (`queued`/`processing`/`completed`/`failed`) — Flutter polling 
logic and FastAPI both branch on these exact strings, so treat them as a 
contract, not free text. - **Analytics rollup collections** are named as the plural of what they 
summarize plus `Stats` (`lessonStats`, `quizStats`, `questionStats`) to 
keep them visually distinct from the transactional collections they're 
derived from. --- 
## 8. Best Practices - **No unbounded arrays.** The only arrays in this schema are `answers[]` 
inside a single attempt (bounded by question count, ~10–20 items) and 
small tag/string lists. Everything that grows without bound (attempts, 
wrong questions, feedback, recommendations, bank versions) is a 
subcollection. - **Denormalize deliberately, not accidentally.** `lessonTag`, 
`difficulty`, `questionType` are copied onto `wrongQuestions` and 
`answers[]` entries specifically so revision/analytics screens never need 
a second read to the source question. Document *why* a field is 
denormalized so future-you doesn't "normalize it away" by mistake. 
- **Use transactions for attempt-limit enforcement.** Creating a new 
`quizAttempts` doc must happen inside a Firestore transaction that reads 
`quizProgress/{quizId}`, checks `attemptsUsed < 3`, then increments — 
otherwise two rapid submissions (double-tap, retry logic) can create a 
4th attempt. - **Use Cloud Functions (or FastAPI background tasks) for derived data.** 
When an attempt is submitted, updating `wrongQuestions`, `weakTopics`, 
`feedback`, `youtubeRecommendations`, and `analytics/*` should happen 
asynchronously after the score is returned to the student — not block the 
submission response on an LLM call plus a YouTube API call. - **Snapshot question text into `wrongQuestions`.** Don't rely on always 
joining back to the live question doc — bank versions get archived and 
questions can be edited, and a student's review history should reflect 
what they were actually asked. - **Never generate synchronously inside an HTTP request.** Both question 
bank generation (RAG, 150–300 questions) and final quiz generation (LLM, 
cross-lesson) are long-running — model them as a `generationJobs` doc 
created immediately (`status: queued`), processed by a background worker, 
updated in place as it progresses, with Flutter listening to that doc in 
real time via a Firestore snapshot listener rather than polling an HTTP 
endpoint. - **Archive, never delete or edit, generated content.** When an admin 
regenerates a bank or a final quiz, the old 
`questionBankVersions`/`finalQuizzes` doc flips to `status: archived` and 
a new one is created — this is what guarantees "preserve all previous 
student attempts" holds structurally, not just by convention. - **Roll up analytics asynchronously, not live.** Don't have the Admin 
dashboard run a collection-group aggregation query across every student's 
`quizAttempts` on page load. Update `analytics/lessonStats`, `quizStats`, 
`questionStats` incrementally in the same background task that processes 
each attempt submission, or via a nightly scheduled job if near-real-time 
isn't required. --- 
## 9. Scalability Considerations - **Auto-generated document IDs everywhere except the noted exceptions** 
— this avoids write hotspots on sequential keys, which matters once you 
have thousands of students submitting `quizAttempts` concurrently at exam 
time. - **RAG embeddings do not belong in Firestore.** Store only 
`sourceReference` (pointers/IDs) in the question documents; keep the 
actual vector embeddings in a dedicated vector store (e.g., pgvector, 
Pinecone, Weaviate, or a FAISS index managed by FastAPI). Firestore is a 
document DB, not a vector DB — mixing concerns here will hurt both cost 
and retrieval quality. - **Paginate lesson/question lists** (`limit()` + `startAfter()`) rather 
than fetching entire subcollections, especially once question banks grow 
per lesson. - **Lean on Flutter's Firestore offline cache** for `lessons` and 
`quizzes` (relatively static, grade-scoped) to cut down repeated reads — 
but never cache `questions` with answers client-side beyond the active 
session (see Section 10). 
- **Collection Group queries scale independently of nesting depth**, so 
the "subcollection under user" choice does not become a scalability tax 
on admin analytics later. - **Question bank generation and final quiz generation should run on a 
task queue** (Cloud Tasks, or a FastAPI background worker backed by a 
queue), not inline — both can involve dozens of LLM calls (chunk 
retrieval + generation per question) and would otherwise time out an HTTP 
request or block a server worker for minutes. - **For heavy, ad-hoc Admin Analytics** (cohort trends, historical 
comparisons across grades/terms) that go beyond what the lightweight 
`analytics/*` rollup docs cover, use the **Firestore → BigQuery 
Extension** rather than building increasingly complex Firestore queries — 
this is the standard enterprise pattern once analytics needs outgrow 
simple rollups. - **Question banks of 150–300 docs per version, per quiz, across many 
quizzes and versions, add up.** Random selection at attempt time should 
use a bounded query (e.g. fetch question IDs via a lightweight document
ID-only projection and sample server-side) rather than reading the entire 
bank into memory per attempt if banks grow very large. --- 
## 10. Security Considerations 
This is the part most learning-platform projects get wrong, so it's worth 
being explicit. 
**Principle: the Flutter client should never be the source of truth for 
anything that affects a grade, a score, an attempt count, or generated 
content.** All of that must be computed and written server-side by 
FastAPI using the Firebase Admin SDK (which bypasses Firestore Security 
Rules entirely). Security Rules then exist purely to lock down what the 
*client* SDK can directly read/write. 
Concretely: - **`role` custom claim is the only source of truth for Admin vs. Student 
access**, mirrored into `users/{uid}.role` purely so Security Rules can 
read it without a token round-trip. **Public sign-up must be hardcoded to 
only ever set `role: student`** — there must be no client-reachable path, 
request parameter, or Firestore write that lets a user set their own 
`role` to `admin`. Admin provisioning is a manual/offline operation. - **`lessons`:** `allow read: if resource.data.grade == 
getUserGrade(request.auth.uid) && resource.data.status == 'published'; 
allow write: if false` (client never writes lessons). - **`materials` (subcollection):** `allow read, write: if false` for the 
client entirely — raw source PDFs and notes are admin/backend-only; 
Flutter never fetches these directly (they're for RAG ingestion, not 
display). Uploads go through a FastAPI endpoint that also writes to Cloud 
Storage and kicks off ingestion. - **`quizzes` (subcollection):** same grade-check pattern via the parent 
lesson; `write: false`. - **`questionBankVersions` and their nested `questions`:** `allow read, 
write: if false` for the client SDK entirely. The Flutter app should 
**never** query these directly — it must call a FastAPI endpoint (e.g. 
`GET /quizzes/{id}/start`) that reads the questions server-side with the 
Admin SDK and returns a **sanitized payload with `correctAnswer` and 
`explanation` stripped out**. This is the only reliable way to stop a 
student from reading Firestore directly and seeing the answer key before 
submitting. Only `activeQuestionBankVersionId` (a plain string field on 
the parent `quiz` doc) is ever read by the client, and even that only to 
display quiz status. - **`finalQuizzes` and their nested `questions`:** same pattern — client 
never reads these directly; FastAPI serves a sanitized, shuffled, answer
stripped payload at attempt-start. - **`users/{uid}`:** `allow read, update: if request.auth.uid == uid`, 
but explicitly **deny updates that touch `currentGrade` or `role`** from 
the client (`!(["currentGrade", 
"role"].hasAny(request.resource.data.diff(resource.data).affectedKeys()))
`), since promotion and role assignment must only happen via server-side 
processes. - **`quizProgress`, `quizAttempts`, `wrongQuestions`, `feedback`, 
`weakTopics`, `youtubeRecommendations`, `finalQuizAttempts`, 
`performanceSummary`:** `allow read: if request.auth.uid == uid; allow 
write: if false` for the client. All writes to these happen only through 
FastAPI (Admin SDK), because these are exactly the documents a malicious 
client could otherwise use to fake a perfect score or unlock a 4th 
attempt by writing directly to Firestore. - **`generationJobs`:** `allow read: if request.auth.token.role == 
'admin'; allow write: if false` — students should never see generation 
jobs, and even admins only read progress (writes are backend-only). - **`analytics/*`:** `allow read: if request.auth.token.role == 'admin'; 
allow write: if false` — these are admin-dashboard-only aggregates, never 
relevant to a student, and never client-writable. - **Grade promotion job:** run as a Cloud Scheduler–triggered Cloud 
Function (or FastAPI cron endpoint) using the Admin SDK, guarded by 
`lastPromotedAt` to make it idempotent if it accidentally runs twice. --- 
## Summary 
| Concern | Decision | 
|---|---| 
| Roles | `role` field on `users/{uid}` + Firebase custom claim; admin 
provisioned out-of-band, never via public signup | 
| RAG source material | `lessons/{id}/materials` subcollection, Cloud
Storage-backed, backend/admin-only access | 
| Lesson → Quiz → Question | Nested subcollections (strict containment), 
with a versioning layer between quiz and question | 
| Question content | Versioned via 
`questionBankVersions/{versionId}/questions` — old versions archived, 
never deleted or edited | 
| Final quiz | One doc **per round** (`grade{N}-round{M}`), `status: 
active/archived`, same versioning philosophy as question banks; generated 
via **RAG**, not the LLM's internal knowledge alone | 
| Async generation | `generationJobs` collection, task-queue driven, 
tracked with live progress | 
| Comparing different LLMs (GPT, DeepSeek, Claude, etc.) | No separate 
database structure needed — `llmModelUsed` on `generationJobs`, 
`finalQuizzes`, and `feedback`, plus `answerCheckingModel` on 
`finalQuizAttempts`, record which model did the work. Switching models is 
a backend config choice; each generated round/attempt/feedback is simply 
tagged with which model produced it. | 
| Student-generated data | Subcollections under `users/{uid}` (privacy + 
simple rules) | 
| Cross-student analytics | Precomputed `analytics/lessonStats` / 
`quizStats` / `questionStats` rollups, backed by Collection Group queries 
and optionally BigQuery for deeper analysis | 
| Attempt-limit enforcement | `quizProgress/{quizId}` doc + Firestore 
transaction | 
| Answer-key protection | Never expose question/bank/final-quiz 
subcollections to client reads — mediate entirely via FastAPI | 
| Score/attempt integrity | All writes to attempts/progress/generated
content via Admin SDK only, client write access = `false` | 
| Grade promotion | Server-side scheduled job, client cannot write 
`currentGrade` or `role` | 
| RAG embeddings | Kept out of Firestore; only source references stored | 
This schema is ready for the next step — API endpoint implementation and 
Security Rules code — whenever you want to move forward. The companion 
document **System Architecture Extension** covers the role-based flows, 
admin/student workflows, randomization strategy, and FastAPI endpoint 
architecture built on top of this schema. 