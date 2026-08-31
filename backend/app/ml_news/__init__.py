"""Grade/topic matching for news-to-question generation."""

from __future__ import annotations

CONCEPTS = [
    {
        "topic": "Density",
        "grade": 9,
        "keywords": ("density", "float", "sink", "buoyan", "mass and volume", "seawater", "iceberg"),
        "formula": "density = mass / volume",
    },
    {
        "topic": "Newton's laws of motion",
        "grade": 10,
        "keywords": ("newton", "force", "inertia", "acceleration", "crash", "friction", "brake", "collision"),
        "formula": "F = m a",
    },
    {
        "topic": "Momentum",
        "grade": 10,
        "keywords": ("momentum", "collision", "impulse", "airbag"),
        "formula": "p = m v",
    },
    {
        "topic": "Work, power and energy",
        "grade": 10,
        "keywords": ("kinetic energy", "potential energy", "power plant", "work done", "joule", "watt"),
        "formula": "KE = 1/2 m v^2",
    },
    {
        "topic": "Pressure",
        "grade": 9,
        "keywords": ("pressure", "atmosphere", "hydraulic", "pascal", "tyre", "depth"),
        "formula": "P = F / A",
    },
    {
        "topic": "Heat and temperature",
        "grade": 9,
        "keywords": ("heat", "temperature", "thermal", "specific heat", "climate warming", "insulation"),
        "formula": "Q = m c Δθ",
    },
    {
        "topic": "Waves and sound",
        "grade": 10,
        "keywords": ("wave", "frequency", "wavelength", "sound", "ultrasound", "earthquake", "seismic"),
        "formula": "v = f λ",
    },
    {
        "topic": "Light and optics",
        "grade": 10,
        "keywords": ("light", "lens", "refraction", "reflection", "laser", "prism", "mirror", "optical"),
        "formula": "n = c / v",
    },
    {
        "topic": "Electricity",
        "grade": 10,
        "keywords": ("electric", "current", "voltage", "resistance", "ohm", "circuit", "battery", "power grid"),
        "formula": "V = I R",
    },
    {
        "topic": "Magnetism",
        "grade": 10,
        "keywords": ("magnet", "magnetic field", "compass", "electromagnet", "transformer"),
        "formula": "a changing magnetic field can induce an emf",
    },
    {
        "topic": "Gravity and orbits",
        "grade": 10,
        "keywords": ("gravity", "orbit", "satellite", "planet", "black hole", "gravitational"),
        "formula": "g = 10 m/s^2 (O/L approximation)",
    },
    {
        "topic": "Nuclear and particle physics",
        "grade": 11,
        "keywords": ("nuclear", "fusion", "fission", "cern", "higgs", "radioactive", "atom", "particle"),
        "formula": "E = mc^2 (qualitative at school level)",
    },
]


def match_concept(text: str, student_grade: int | None = None) -> dict:
    blob = (text or "").lower()
    scored: list[tuple[int, dict]] = []
    for concept in CONCEPTS:
        hits = sum(1 for key in concept["keywords"] if key in blob)
        if hits:
            scored.append((hits, concept))
    if scored:
        scored.sort(key=lambda item: item[0], reverse=True)
        chosen = dict(scored[0][1])
    else:
        chosen = {
            "topic": "Forces and motion",
            "grade": 10,
            "keywords": (),
            "formula": "F = m a",
        }
    chosen["studentGrade"] = student_grade
    chosen["curriculumGrade"] = int(chosen["grade"])
    if student_grade and int(student_grade) < int(chosen["grade"]):
        chosen["gradeNote"] = (
            f"This news maps to a Grade {chosen['grade']} topic. "
            f"The question is simplified for Grade {student_grade}."
        )
        chosen["questionGrade"] = int(student_grade)
    else:
        chosen["gradeNote"] = f"Matched to Grade {chosen['grade']} {chosen['topic']}."
        chosen["questionGrade"] = int(student_grade or chosen["grade"])
    return chosen
