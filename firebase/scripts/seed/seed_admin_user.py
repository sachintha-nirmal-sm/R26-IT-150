"""
seed_admin_user.py — Provision the first admin account.

What this script does (when written):
  1. Create a Firebase Auth user with email + password.
  2. Set a custom claim: { "role": "admin" }
  3. Write the corresponding users/{uid} document in Firestore with:
       role: "admin"
       fullName: <configured>
       email: <configured>
       status: "active"
       createdAt: <server timestamp>

Per architecture (Section 10, devmini.md):
  - Admin accounts are NEVER created through the public Student Sign-Up flow.
  - The role custom claim is the single source of truth for Admin vs. Student.
  - Public self-signup can only ever produce role: "student".

Usage:
    python seed/seed_admin_user.py

TODO: Implement this script.
"""

# Seed logic not yet written — scaffolded only.
raise NotImplementedError("seed_admin_user.py is not yet implemented.")
