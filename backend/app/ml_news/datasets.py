"""Build labelled CSVs for Model 1 (news) and Model 2 (answers)."""

from __future__ import annotations

import csv
from pathlib import Path

DATA_DIR = Path(__file__).resolve().parent / "data"
DATA_DIR.mkdir(parents=True, exist_ok=True)


def _physics_news() -> list[str]:
    actors = [
        "CERN researchers",
        "NASA engineers",
        "University of Colombo physicists",
        "Japanese scientists",
        "A school laboratory team",
        "European Space Agency staff",
        "Sri Lankan engineers",
        "A crash-investigation unit",
    ]
    actions = [
        "measured",
        "observed",
        "calculated",
        "reported",
        "simulated",
        "detected",
        "compared",
        "recorded",
    ]
    topics = [
        "gravitational waves from colliding black holes",
        "the orbital speed of a new Earth satellite",
        "electron flow through a copper wire at room temperature",
        "the density of seawater after heavy rain near Colombo harbour",
        "friction on a wet bus tyre during emergency braking",
        "the magnetic field around a school electromagnet",
        "voltage drop across a resistor in a simple circuit",
        "kinetic energy of a cricket ball after a fast delivery",
        "pressure at different depths in a swimming pool",
        "the wavelength of sound from a temple bell",
        "refraction of a laser beam through a glass prism",
        "heat transfer through a poorly insulated roof",
        "momentum change in a vehicle collision test",
        "Newton's first law using a hover-puck demonstration",
        "nuclear fusion energy output in a tokamak pulse",
        "ultrasound frequency used in a hospital scanner",
        "the restoring force of a simple pendulum",
        "atmospheric pressure during a monsoon storm",
        "light intensity after passing through polarising filters",
        "the acceleration of a freely falling object",
        "Ohm's law for a torch bulb at two voltages",
        "the work done lifting a sandbag onto a lorry",
        "seismic P-waves arriving after an earthquake",
        "buoyant force on a fishing boat in the Indian Ocean",
        "transformer voltage step-down in a village power line",
        "specific heat capacity of coconut oil in a lab test",
        "the period of a mass-spring system",
        "radioactive decay counts from a sealed source",
        "the speed of a water wave in a school ripple tank",
        "magnetic deflection of a compass near a current-carrying wire",
    ]
    extras = [
        "using school-lab equipment",
        "in a peer-reviewed study",
        "during a public science week",
        "with improved sensors",
        "and compared results with theory",
        "for a Grade 10 physics outreach event",
    ]
    rows = []
    i = 0
    for topic in topics:
        for actor in actors[:4]:
            action = actions[i % len(actions)]
            extra = extras[i % len(extras)]
            rows.append(f"{actor} {action} {topic} {extra}.")
            i += 1
    extra_headlines = [
        "Physicists confirm light travels slower in water than in air.",
        "New battery research explains internal resistance and terminal voltage.",
        "Astronomers estimate mass of a planet from its gravitational pull on a moon.",
        "Engineers reduce air resistance on a train using a more streamlined shape.",
        "Doctors use ultrasound waves to form images of soft tissue.",
        "A roller-coaster design uses conversion between potential and kinetic energy.",
        "Solar-panel study reports energy transferred per second as electrical power.",
        "Students plot a cooling curve to study heat loss to the surroundings.",
        "Bridge designers calculate tension and compression in steel cables.",
        "A lightning report describes a sudden large electric current to the ground.",
        "Ice floating on water is explained using density and upthrust.",
        "Racing cyclists crouch to reduce drag force from air resistance.",
        "A microwave oven heats food by absorbing electromagnetic waves.",
        "GPS satellites must account for orbital motion around Earth.",
        "Firefighters use water pressure in hoses to reach upper floors.",
        "A loudspeaker cone vibrates to produce a sound wave in air.",
        "Scientists map Earth's magnetic field using a network of compass stations.",
        "A skipped stone on a lake shows action and reaction forces.",
        "Lab report: current is the same in series but splits in parallel circuits.",
        "Telescope mirror focuses incoming light by reflection.",
        "Skiers wax skis to change friction with the snow.",
        "A falling coconut hits the ground with greater speed from a taller tree.",
        "Hydroelectric dam converts gravitational potential energy into electricity.",
        "X-ray photons pass through soft tissue more easily than through bone.",
        "A tuning fork experiment measures frequency of a sound wave.",
        "Electric kettle efficiency is compared using energy input and heat gained.",
        "The Hubble telescope orbits Earth because gravity provides centripetal force.",
        "Students use a ticker timer to find average speed and acceleration.",
        "A solenoid becomes an electromagnet only when current flows.",
        "Rainbow colours appear because raindrops refract and disperse sunlight.",
        "A car airbag increases stopping time so the impact force is smaller.",
        "Deep-sea fish survive high pressure caused by the weight of the water column.",
        "A thermal flask keeps tea hot by reducing conduction, convection and radiation.",
        "Particle physicists at CERN collide protons at very high kinetic energy.",
        "A classroom van de Graaff generator demonstrates electrostatic charge.",
        "Waves on the south coast carry energy but not the water itself over long distances.",
        "A bicycle dynamo generates current when a magnet rotates near a coil.",
        "Free-fall tower measurements are close to 10 metres per second squared.",
        "Optical fibre broadband uses total internal reflection of light.",
        "A cricket batsman changes the ball's momentum, producing an impulse on the bat.",
    ]
    rows.extend(extra_headlines)
    return rows


def _non_physics_news() -> list[str]:
    actors = [
        "A film studio",
        "The finance ministry",
        "Colombo FC",
        "A fashion brand",
        "Pop star Maya",
        "A high court",
        "A restaurant chain",
        "Tourism officials",
    ]
    actions = [
        "announced",
        "celebrated",
        "denied",
        "promoted",
        "delayed",
        "criticised",
        "confirmed",
        "cancelled",
    ]
    topics = [
        "a celebrity wedding date in Mumbai",
        "new tax rules for small shops",
        "a 3-1 football final victory",
        "a spring clothing collection",
        "concert tickets selling out in one hour",
        "a lengthy corruption trial",
        "a spicy new rice-and-curry menu",
        "record hotel bookings for the holiday season",
        "a reality-TV singing contest winner",
        "share prices rising after a bank merger",
        "a royal visit to a tea plantation",
        "school exam timetable changes for next year",
        "a social-media influencer brand deal",
        "the opening of a shopping mall food court",
        "a political party leadership vote",
        "box-office numbers for a comedy movie",
        "a cricket board selection controversy",
        "a new smartphone camera marketing campaign",
        "a charity gala dinner in Colombo",
        "a cookbook of traditional sweets",
        "an art exhibition of landscape paintings",
        "a strike by bus drivers over wages",
        "a beauty pageant in Kandy",
        "stock-market rumours about a hotel group",
        "a viral dance challenge on TikTok",
        "a court ban on a newspaper cartoon",
        "festival fireworks permission in the city",
        "a grocery discount week at supermarket chains",
        "an award for best supporting actor",
        "a tourism slogan for the east coast",
    ]
    extras = [
        "according to a press release",
        "after weeks of speculation",
        "during a live television interview",
        "on social media",
        "in a packed stadium",
        "before next month's budget",
    ]
    rows = []
    i = 0
    for topic in topics:
        for actor in actors[:4]:
            action = actions[i % len(actions)]
            extra = extras[i % len(extras)]
            rows.append(f"{actor} {action} {topic} {extra}.")
            i += 1
    extra_headlines = [
        "Voters queue early for local council elections in Galle.",
        "A new cafe offers free wifi and cinnamon buns.",
        "Fashion week closes with a gold embroidered sari collection.",
        "The stock exchange pauses trading after a software glitch.",
        "A novelist wins a prize for a historical love story.",
        "Football fans celebrate a last-minute penalty in extra time.",
        "A supermarket chain recalls a batch of expired yoghurt.",
        "Travel agents advertise cheap flights to Dubai.",
        "A court hears arguments in a land-dispute case.",
        "Streaming service releases a new mystery series tonight.",
        "The central bank comments on inflation and interest rates.",
        "A chef shares a recipe for hopper batter on television.",
        "University arts faculty hosts a poetry reading night.",
        "A mobile game adds new cartoon characters in an update.",
        "Wedding planners report a rise in beach ceremonies.",
        "Opposition parties criticise a new traffic fine proposal.",
        "A pop concert is postponed because the singer has a cold.",
        "Jewellery sales jump during the festival shopping week.",
        "A radio station launches a morning gossip show.",
        "Hotel owners complain about a new service charge rule.",
        "A painting by a local artist sells at auction.",
        "Social media users argue about a celebrity interview clip.",
        "A bakery wins an award for best chocolate cake.",
        "City hall approves a new public park playground.",
        "A language app adds Sinhala slang lessons for tourists.",
        "The film board rates a new thriller for adult audiences.",
        "A cricket commentator signs a deal with a sports channel.",
        "Shopkeepers decorate streets for a religious procession.",
        "A banking app adds fingerprint login for customers.",
        "Food bloggers review a new kottu stall in Pettah.",
        "A talent show judge praises a contestant's costume.",
        "Export earnings from tea are discussed in parliament.",
        "A soap opera cliffhanger trends on Facebook overnight.",
        "Gym memberships are discounted for the new year.",
        "A historian publishes a book on colonial-era forts.",
        "Online shoppers complain about late parcel deliveries.",
        "A mayor opens a flower show in the town hall.",
        "Record labels argue over music streaming royalties.",
        "A cooking contest finalist uses jackfruit in a dessert.",
        "Youth groups organise a beach clean-up and music night.",
    ]
    rows.extend(extra_headlines)
    return rows


def build_news_rows() -> list[dict]:
    physics = _physics_news()
    other = _non_physics_news()
    seen = set()
    rows = []
    for text in physics:
        key = text.strip().lower()
        if key in seen:
            continue
        seen.add(key)
        rows.append({"text": text.strip(), "label": "physics"})
    for text in other:
        key = text.strip().lower()
        if key in seen:
            continue
        seen.add(key)
        rows.append({"text": text.strip(), "label": "non_physics"})
    return rows


def _qa_bank() -> list[dict]:
    return [
        {
            "question": "Define density and state its SI unit.",
            "reference": "Density is mass per unit volume. Its SI unit is kilogram per cubic metre (kg/m^3).",
            "topic": "Density",
        },
        {
            "question": "State Newton's first law of motion.",
            "reference": "A body remains at rest or continues to move at constant velocity in a straight line unless acted on by an unbalanced force.",
            "topic": "Newton's laws",
        },
        {
            "question": "Write the formula for kinetic energy and name the quantities.",
            "reference": "Kinetic energy KE = 1/2 mv^2, where m is mass in kilograms and v is speed in metres per second.",
            "topic": "Energy",
        },
        {
            "question": "State Ohm's law.",
            "reference": "Ohm's law states that the current through a conductor is directly proportional to the potential difference across it, provided temperature is constant. V = IR.",
            "topic": "Electricity",
        },
        {
            "question": "What is pressure? Give the formula and SI unit.",
            "reference": "Pressure is force per unit area. P = F/A. The SI unit is the pascal (Pa), which is N/m^2.",
            "topic": "Pressure",
        },
        {
            "question": "Define momentum and give its formula.",
            "reference": "Momentum is the product of mass and velocity. p = mv. It is a vector and its SI unit is kg m/s.",
            "topic": "Momentum",
        },
        {
            "question": "State the relationship between speed, frequency and wavelength of a wave.",
            "reference": "Wave speed equals frequency times wavelength, v = f λ.",
            "topic": "Waves",
        },
        {
            "question": "What is friction and how does it affect motion?",
            "reference": "Friction is a force that opposes the relative motion of surfaces in contact. It can slow an object down or help it start moving without slipping.",
            "topic": "Forces",
        },
        {
            "question": "Explain why a small force on a syringe piston can produce a large force at the other piston in a hydraulic system.",
            "reference": "Pressure is transmitted equally through the liquid. A small force on a small area creates pressure that acts on a larger area, producing a larger force.",
            "topic": "Pressure",
        },
        {
            "question": "State the law of reflection of light.",
            "reference": "The angle of incidence equals the angle of reflection, and the incident ray, reflected ray and normal all lie in the same plane.",
            "topic": "Optics",
        },
        {
            "question": "What is gravitational potential energy of an object near Earth?",
            "reference": "Gravitational potential energy is m g h, the energy an object has because of its height in a gravitational field.",
            "topic": "Energy",
        },
        {
            "question": "Define electric current.",
            "reference": "Electric current is the rate of flow of charge. I = Q/t. The SI unit is the ampere.",
            "topic": "Electricity",
        },
        {
            "question": "Why does ice float on water?",
            "reference": "Ice is less dense than liquid water, so the upthrust when it is partly submerged equals its weight and it floats.",
            "topic": "Density",
        },
        {
            "question": "State Newton's second law in terms of force, mass and acceleration.",
            "reference": "The unbalanced force on a body equals mass times acceleration, F = ma, in the direction of the force.",
            "topic": "Newton's laws",
        },
        {
            "question": "What is power in physics?",
            "reference": "Power is the rate of doing work or transferring energy. P = W/t. The SI unit is the watt (J/s).",
            "topic": "Energy",
        },
        {
            "question": "Describe how a transformer works at school level.",
            "reference": "An alternating current in the primary coil produces a changing magnetic field, which induces an emf in the secondary coil. The voltage ratio equals the turns ratio.",
            "topic": "Magnetism",
        },
        {
            "question": "What is acceleration?",
            "reference": "Acceleration is the rate of change of velocity. a = (v - u)/t. It is a vector and its SI unit is m/s^2.",
            "topic": "Motion",
        },
        {
            "question": "State the difference between heat and temperature.",
            "reference": "Heat is energy transferred because of a temperature difference. Temperature measures how hot or cold a body is, related to average kinetic energy of particles.",
            "topic": "Heat",
        },
        {
            "question": "What is work done by a force?",
            "reference": "Work is done when a force moves an object in the direction of the force. W = F s when the force and displacement are in the same direction.",
            "topic": "Energy",
        },
        {
            "question": "Why does a bus passenger lurch forward when the bus stops suddenly?",
            "reference": "By Newton's first law the passenger continues moving forward at constant velocity until an unbalanced force, such as a seatbelt or friction, slows them down.",
            "topic": "Newton's laws",
        },
    ]


def _variants(item: dict) -> list[dict]:
    q, ref, topic = item["question"], item["reference"], item["topic"]
    correct = [
        ref,
        ref.replace("SI unit", "unit in SI").replace("equals", "is"),
        "According to the syllabus, " + ref,
    ]
    partial = [
        f"It is about {topic.lower()}, but I cannot write the full definition.",
        f"I remember {topic.lower()} from class, yet I forgot the formula and the SI unit.",
        "Part of the idea is: " + " ".join(ref.split()[:5]) + ". I cannot complete the rest.",
    ]
    incorrect = [
        "This is caused by photosynthesis in green plants.",
        "The answer is the number of people voting in an election.",
        "Energy is created from nothing and density is the same as weight.",
        "Newton said objects stop by themselves because they get tired.",
    ]
    rows = []
    for text in correct:
        rows.append({"question": q, "reference": ref, "student": text, "label": "correct", "topic": topic})
    for text in partial:
        rows.append({"question": q, "reference": ref, "student": text, "label": "partial", "topic": topic})
    for text in incorrect[:3]:
        rows.append({"question": q, "reference": ref, "student": text, "label": "incorrect", "topic": topic})
    return rows


def build_answer_rows() -> list[dict]:
    rows: list[dict] = []
    for item in _qa_bank():
        rows.extend(_variants(item))
    return rows


def write_csv(path: Path, rows: list[dict], fieldnames: list[str]) -> Path:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        for row in rows:
            writer.writerow({key: row.get(key, "") for key in fieldnames})
    return path


def export_datasets() -> dict[str, Path]:
    news_path = DATA_DIR / "news_classifier.csv"
    answers_path = DATA_DIR / "answer_classifier.csv"
    write_csv(news_path, build_news_rows(), ["text", "label"])
    write_csv(answers_path, build_answer_rows(), ["question", "reference", "student", "label", "topic"])
    return {"news": news_path, "answers": answers_path}
