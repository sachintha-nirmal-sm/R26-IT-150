"""
notification_topic_visuals.py — topic-specific animated backgrounds, ported
from the physics-mobile-app spike's topic_visuals.py. Free, local, no AI
video generation cost (matplotlib, headless/Agg backend).

Covers a starter set of 5 physics topics. ensure_topic_background() returns
None for anything outside that set, so the caller (notification_video.py)
falls back to the mood-colored gradient.
"""

from __future__ import annotations

import re
from pathlib import Path
from typing import Callable

import matplotlib

matplotlib.use("Agg")  # headless — no display available on a server
import matplotlib.pyplot as plt  # noqa: E402
import numpy as np  # noqa: E402
from matplotlib.animation import FFMpegWriter, FuncAnimation  # noqa: E402
from matplotlib.patches import Circle, Rectangle  # noqa: E402

from app.services.notification_video import CLIPS_DIR

FIGSIZE = (4.5, 8)  # 9:16-ish; compose_video's scale+crop handles exact sizing
DPI = 240  # higher than the target video's effective resolution needs, so
# the later scale-to-1080x1920 step in compose_video never has to upscale
# (upscaling a low-res source is what caused visible blur before)
FPS = 20
DURATION_SEC = 6

BG_COLOR = "#0d1b2a"  # matches the dark mood-gradient backgrounds for consistency


def _new_axes():
    fig, ax = plt.subplots(figsize=FIGSIZE, dpi=DPI)
    fig.patch.set_facecolor(BG_COLOR)
    ax.set_facecolor(BG_COLOR)
    ax.set_xlim(-1, 1)
    ax.set_ylim(-1, 1)
    ax.set_aspect("equal", adjustable="box")
    ax.axis("off")
    fig.subplots_adjust(left=0, right=1, top=1, bottom=0)
    return fig, ax


def _save(fig, update: Callable[[int], tuple], frames: int, out_path: Path) -> None:
    anim = FuncAnimation(fig, update, frames=frames, blit=True)
    writer = FFMpegWriter(fps=FPS, bitrate=1200)
    out_path.parent.mkdir(parents=True, exist_ok=True)
    anim.save(str(out_path), writer=writer)
    plt.close(fig)


def _gen_reflection_of_light(out_path: Path) -> None:
    fig, ax = _new_axes()
    ax.plot([-1, 1], [0, 0], color="#9fb4c7", linewidth=3)

    incoming, = ax.plot([], [], color="#ffd166", linewidth=4)
    reflected, = ax.plot([], [], color="#ffd166", linewidth=4)
    a = (-0.75, 0.85)
    hit = (0.0, 0.0)
    b = (0.75, 0.85)

    frames = FPS * DURATION_SEC
    half = frames // 2

    def update(frame):
        f = frame % frames
        if f <= half:
            t = f / half
            incoming.set_data([a[0], a[0] + (hit[0] - a[0]) * t], [a[1], a[1] + (hit[1] - a[1]) * t])
            reflected.set_data([], [])
        else:
            incoming.set_data([a[0], hit[0]], [a[1], hit[1]])
            t = (f - half) / (frames - half)
            reflected.set_data([hit[0], hit[0] + (b[0] - hit[0]) * t], [hit[1], hit[1] + (b[1] - hit[1]) * t])
        return incoming, reflected

    _save(fig, update, frames, out_path)


def _gen_circular_motion(out_path: Path) -> None:
    fig, ax = _new_axes()
    radius = 0.6
    theta = np.linspace(0, 2 * np.pi, 200)
    ax.plot(radius * np.cos(theta), radius * np.sin(theta), color="#4a5a7a", linewidth=2)

    dot, = ax.plot([], [], "o", color="#ff9f45", markersize=18)
    velocity_line, = ax.plot([], [], color="#7bdff2", linewidth=3)

    frames = FPS * DURATION_SEC

    def update(frame):
        t = 2 * np.pi * frame / frames
        x, y = radius * np.cos(t), radius * np.sin(t)
        dot.set_data([x], [y])
        vx, vy = -np.sin(t) * 0.35, np.cos(t) * 0.35
        velocity_line.set_data([x, x + vx], [y, y + vy])
        return dot, velocity_line

    _save(fig, update, frames, out_path)


def _gen_newtons_laws(out_path: Path) -> None:
    """Newton's cradle: momentum transfer / action-reaction."""
    fig, ax = _new_axes()
    pivot_y = 0.7
    ax.plot([-0.9, 0.9], [pivot_y, pivot_y], color="#9fb4c7", linewidth=3)

    n_balls = 5
    xs = np.linspace(-0.6, 0.6, n_balls)
    string_length = pivot_y
    ball_radius = 0.09
    strings = [ax.plot([], [], color="#5a6a8a", linewidth=1.5)[0] for _ in range(n_balls)]
    balls = [Circle((x, 0.0), ball_radius, color="#dbe4ee") for x in xs]
    for c in balls:
        ax.add_patch(c)

    def pendulum_xy(x_pivot: float, theta: float) -> tuple[float, float]:
        return (
            x_pivot + string_length * np.sin(theta),
            pivot_y - string_length * np.cos(theta),
        )

    frames = FPS * DURATION_SEC
    swing_frames = frames // 2
    max_angle = 0.7

    def update(frame):
        f = frame % frames
        positions = [(x, 0.0) for x in xs]

        if f < swing_frames:
            t = f / swing_frames
            angle = (1 - t) * -max_angle
            positions[0] = pendulum_xy(xs[0], angle)
        else:
            t = (f - swing_frames) / (frames - swing_frames)
            angle = np.sin(t * np.pi) * max_angle
            positions[-1] = pendulum_xy(xs[-1], angle)

        for i, c in enumerate(balls):
            c.center = positions[i]
            strings[i].set_data([xs[i], positions[i][0]], [pivot_y, positions[i][1]])

        return (*balls, *strings)

    _save(fig, update, frames, out_path)


def _gen_thermodynamics(out_path: Path) -> None:
    fig, ax = _new_axes()
    ax.plot([-0.8, 0.8, 0.8, -0.8, -0.8], [-0.8, -0.8, 0.8, 0.8, -0.8], color="#9fb4c7", linewidth=2)

    rng = np.random.default_rng(7)
    n = 14
    pos = rng.uniform(-0.7, 0.7, size=(n, 2))
    vel = rng.uniform(-0.02, 0.02, size=(n, 2))
    scatter = ax.scatter(pos[:, 0], pos[:, 1], s=60, c=["#5aa9ff"] * n)

    frames = FPS * DURATION_SEC
    bound = 0.75

    def update(frame):
        nonlocal pos, vel
        heat_factor = 1 + 2.0 * (frame / frames)
        pos = pos + vel * heat_factor
        out_of_bounds = np.abs(pos) > bound
        vel[out_of_bounds] *= -1
        pos = np.clip(pos, -bound, bound)
        scatter.set_offsets(pos)
        speeds = np.linalg.norm(vel, axis=1) * heat_factor
        colors = plt.cm.coolwarm(np.clip(speeds / speeds.max(), 0, 1))
        scatter.set_color(colors)
        return (scatter,)

    _save(fig, update, frames, out_path)


def _gen_projectile_motion(out_path: Path) -> None:
    fig, ax = _new_axes()
    ax.plot([-0.9, 0.9], [-0.8, -0.8], color="#9fb4c7", linewidth=3)

    trail, = ax.plot([], [], color="#7bdff2", linewidth=2, alpha=0.6)
    ball, = ax.plot([], [], "o", color="#ff9f45", markersize=16)

    v0, angle, g = 1.6, np.radians(55), 2.6
    vx, vy0 = v0 * np.cos(angle), v0 * np.sin(angle)
    t_flight = 2 * vy0 / g
    frames = FPS * DURATION_SEC

    xs_trail, ys_trail = [], []

    def update(frame):
        t = (frame / frames) * t_flight
        x = -0.85 + vx * t
        y = -0.8 + vy0 * t - 0.5 * g * t * t
        if frame == 0:
            xs_trail.clear()
            ys_trail.clear()
        xs_trail.append(x)
        ys_trail.append(y)
        trail.set_data(xs_trail, ys_trail)
        ball.set_data([x], [y])
        return trail, ball

    _save(fig, update, frames, out_path)


def _gen_work_energy(out_path: Path) -> None:
    """Simple pendulum — visualizes kinetic/potential energy trading back
    and forth as it swings, for Work/Energy/Power topics."""
    fig, ax = _new_axes()
    pivot = (0.0, 0.85)
    length = 1.1
    ax.plot([pivot[0]], [pivot[1]], "o", color="#9fb4c7", markersize=6)

    string, = ax.plot([], [], color="#5a6a8a", linewidth=2)
    bob, = ax.plot([], [], "o", color="#ffd166", markersize=20)

    frames = FPS * DURATION_SEC
    max_angle = 0.9

    def update(frame):
        t = 2 * np.pi * frame / frames
        angle = max_angle * np.sin(t)
        x = pivot[0] + length * np.sin(angle)
        y = pivot[1] - length * np.cos(angle)
        string.set_data([pivot[0], x], [pivot[1], y])
        bob.set_data([x], [y])
        return string, bob

    _save(fig, update, frames, out_path)


def _gen_waves(out_path: Path) -> None:
    """A traveling sine wave — covers Waves and sound/loudness topics."""
    fig, ax = _new_axes()
    x = np.linspace(-1, 1, 200)
    line, = ax.plot(x, np.zeros_like(x), color="#7bdff2", linewidth=3)
    dot, = ax.plot([], [], "o", color="#ff9f45", markersize=14)

    frames = FPS * DURATION_SEC

    def update(frame):
        phase = 2 * np.pi * frame / (frames / 2)
        y = 0.5 * np.sin(4 * np.pi * x + phase)
        line.set_data(x, y)
        dot.set_data([0], [0.5 * np.sin(phase)])
        return line, dot

    _save(fig, update, frames, out_path)


def _gen_density(out_path: Path) -> None:
    """Two containers, same size, different particle packing — a dense
    cluster vs. a sparse one — for the Density topic."""
    fig, ax = _new_axes()
    ax.plot([-0.95, -0.95, -0.05, -0.05, -0.95], [-0.8, 0.5, 0.5, -0.8, -0.8], color="#9fb4c7", linewidth=2)
    ax.plot([0.05, 0.05, 0.95, 0.95, 0.05], [-0.8, 0.5, 0.5, -0.8, -0.8], color="#9fb4c7", linewidth=2)

    rng = np.random.default_rng(3)
    dense_pos = rng.uniform([-0.9, -0.75], [-0.1, 0.45], size=(18, 2))
    sparse_pos = rng.uniform([0.1, -0.75], [0.9, 0.45], size=(6, 2))
    dense_scatter = ax.scatter(dense_pos[:, 0], dense_pos[:, 1], s=50, c="#6c5ce7")
    sparse_scatter = ax.scatter(sparse_pos[:, 0], sparse_pos[:, 1], s=50, c="#ffd166")

    frames = FPS * DURATION_SEC

    def update(frame):
        wobble = 0.01 * np.sin(2 * np.pi * frame / frames * 3)
        dense_scatter.set_offsets(dense_pos + [0, wobble])
        sparse_scatter.set_offsets(sparse_pos + [0, -wobble])
        return dense_scatter, sparse_scatter

    _save(fig, update, frames, out_path)


def _gen_oscillations(out_path: Path) -> None:
    """Mass on a spring in simple harmonic motion, for the Oscillations topic."""
    fig, ax = _new_axes()
    anchor_y = 0.9
    ax.plot([-0.3, 0.3], [anchor_y, anchor_y], color="#9fb4c7", linewidth=4)

    spring, = ax.plot([], [], color="#5a6a8a", linewidth=2)
    mass, = ax.plot([], [], "s", color="#ff9f45", markersize=24)

    frames = FPS * DURATION_SEC

    def update(frame):
        t = 2 * np.pi * frame / (frames / 2)
        y = 0.1 - 0.55 * np.cos(t)
        spring.set_data([0, 0], [anchor_y, y])
        mass.set_data([0], [y])
        return spring, mass

    _save(fig, update, frames, out_path)


def _gen_pressure(out_path: Path) -> None:
    """Force arrows pressing down onto a solid block, for the Pressure
    Exerted by Solid topic."""
    fig, ax = _new_axes()
    ax.add_patch(Rectangle((-0.5, -0.6), 1.0, 0.6, color="#3a3a52"))
    arrows = [ax.plot([], [], color="#ffd166", linewidth=4)[0] for _ in range(3)]
    arrow_xs = [-0.3, 0.0, 0.3]

    frames = FPS * DURATION_SEC
    half = max(frames // 2, 1)

    def update(frame):
        t = (frame % half) / half
        press = 0.25 * np.sin(np.pi * t)
        for x, arrow in zip(arrow_xs, arrows):
            arrow.set_data([x, x], [0.75, 0.05 - press])
        return (*arrows,)

    _save(fig, update, frames, out_path)


def _gen_linear_motion(out_path: Path) -> None:
    """Constant-velocity motion along a straight track with a trailing
    path and a velocity vector — catch-all for basic Motion/kinematics."""
    fig, ax = _new_axes()
    ax.plot([-0.9, 0.9], [-0.1, -0.1], color="#9fb4c7", linewidth=3)
    trail, = ax.plot([], [], color="#7bdff2", linewidth=2, alpha=0.5)
    ball, = ax.plot([], [], "o", color="#ff9f45", markersize=16)
    velocity_line, = ax.plot([], [], color="#ffd166", linewidth=3)

    frames = FPS * DURATION_SEC
    xs_trail: list[float] = []

    def update(frame):
        f = frame % frames
        if f == 0:
            xs_trail.clear()
        t = f / frames
        x = -0.85 + 1.7 * t
        y = -0.1
        xs_trail.append(x)
        trail.set_data(xs_trail, [y] * len(xs_trail))
        ball.set_data([x], [y])
        velocity_line.set_data([x, min(x + 0.25, 0.95)], [y, y])
        return trail, ball, velocity_line

    _save(fig, update, frames, out_path)


_TEMPLATES: list[tuple[str, list[str], Callable[[Path], None]]] = [
    # Order matters: normalize_topic returns the FIRST keyword match, so
    # narrower/more specific topics must come before broad catch-alls like
    # "motion" — otherwise e.g. "Circular Motion" would never reach its own
    # dedicated animation.
    ("reflection_of_light", ["reflect", "light", "mirror"], _gen_reflection_of_light),
    ("circular_motion", ["circular", "orbit", "centripetal"], _gen_circular_motion),
    ("newtons_laws", ["newton", "force"], _gen_newtons_laws),
    ("thermodynamics", ["thermo", "heat", "temperature"], _gen_thermodynamics),
    ("projectile_motion", ["projectile", "trajectory"], _gen_projectile_motion),
    ("work_energy", ["energy", "work", "power"], _gen_work_energy),
    ("waves", ["wave", "sound", "loud"], _gen_waves),
    ("density", ["density"], _gen_density),
    ("oscillations", ["oscillat", "spring", "shm"], _gen_oscillations),
    ("pressure", ["pressure"], _gen_pressure),
    # Catch-all for the "Introduction to Motion" lesson's real lessonTag
    # ("phy-g10-motion") — must stay last, "motion" would otherwise swallow
    # circular_motion/projectile_motion matches above.
    ("linear_motion", ["motion", "kinematics", "velocity", "displacement"], _gen_linear_motion),
]


def normalize_topic(topic: str) -> str | None:
    text = topic.lower()
    text = re.sub(r"[^a-z0-9\s]", " ", text)
    for key, keywords, _ in _TEMPLATES:
        if any(kw in text for kw in keywords):
            return key
    return None


def ensure_topic_background(topic: str) -> Path | None:
    """Returns a cached background clip for this topic, generating it on
    first use. Returns None if the topic doesn't match any known template —
    caller falls back to the mood-based gradient in that case."""
    key = normalize_topic(topic)
    if key is None:
        return None

    path = CLIPS_DIR / f"topic_{key}.mp4"
    if path.exists():
        return path

    generator = next(gen for template_key, _, gen in _TEMPLATES if template_key == key)
    generator(path)
    return path
