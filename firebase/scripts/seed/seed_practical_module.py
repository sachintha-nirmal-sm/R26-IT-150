"""
seed_practical_module.py — Seed the Game-Based Measurement practicals module.

Runs, in order:
  1. seed_topics.py
  2. seed_practicals.py
  3. seed_student_practicals.py

Requires the shared users seed (seed_users.py) so the sample student exists.

Usage:
    python seed/seed_practical_module.py
"""

import sys
import os

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from seed_topics import seed_topics
from seed_practicals import seed_practicals
from seed_student_practicals import seed_student_practicals


def main():
    print("=" * 70)
    print("  Seeding Game-Based Measurement & Calculation practicals module")
    print("=" * 70)

    seed_topics()
    print()
    seed_practicals()
    print()
    seed_student_practicals()

    print()
    print("=" * 70)
    print("  Practicals module seeding completed")
    print("=" * 70)


if __name__ == "__main__":
    try:
        main()
    except FileNotFoundError as e:
        print(e)
        sys.exit(1)
    except Exception as e:
        print(f"\nError during seeding: {e}")
        sys.exit(1)
