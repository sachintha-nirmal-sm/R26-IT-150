"""
notification_video.py — Phase 2: TTS voice + FFmpeg composition + Firebase
Storage upload for personalized notification videos. Ported from the
physics-mobile-app spike's tts.py/compose.py/storage.py — English only for
v1 (matches notification_mood.py's scope), and uploads via the already-
initialized Firebase Storage bucket instead of the spike's unwired re-init.

All ffmpeg invocations build argument lists, never shell strings.
"""

from __future__ import annotations

import json
import os
import re
import subprocess
from pathlib import Path

import edge_tts

ASSETS_DIR = Path(__file__).resolve().parent.parent / "assets"
FONT_PATH = ASSETS_DIR / "fonts" / "NotoSans-Regular.ttf"

# Served directly by FastAPI (see main.py's StaticFiles mount at /media) —
# Firebase Storage's current tier requires the paid Blaze plan even within
# free-tier usage, which isn't worth blocking on for local/dev delivery.
# NOTIFICATION_MEDIA_BASE_URL defaults to the Android emulator's alias for
# the host machine's localhost (10.0.2.2); override for a physical device
# on the same LAN or a real deployment.
MEDIA_DIR = Path(__file__).resolve().parent.parent / "media" / "notifications"
MEDIA_BASE_URL = os.getenv("NOTIFICATION_MEDIA_BASE_URL", "http://10.0.2.2:9000")

WORK_DIR = Path("/tmp/notification_video")
CLIPS_DIR = WORK_DIR / "clips"
MUSIC_DIR = WORK_DIR / "music"
PLACEHOLDER_MUSIC = MUSIC_DIR / "placeholder.mp3"

VOICE = "en-US-JennyNeural"
WIDTH, HEIGHT = 1080, 1920
MUSIC_VOLUME = 0.22
# amix's default normalize=1 silently divides the combined signal by the
# number of inputs (2) to avoid clipping, which was quietly halving the
# voice on top of the music attenuation below — the actual cause of videos
# sounding too quiet. A flat volume multiply alone isn't enough to fix it
# though: edge-tts speech has a high crest factor (loud words, quiet gaps),
# so a big enough multiply to raise the *average* level pushes word peaks
# past true 0 dBFS (measured +2.2 dBTP at volume=1.8) well before the
# average catches up — and that clipping-level peak then forces loudnorm's
# limiter to claw the whole mix back down, capping loudness far below the
# target regardless of how it's requested. VOICE_COMPRESSOR evens out that
# dynamic range first (ffmpeg's acompressor) so loudnorm has real headroom
# to push the average up to LOUDNESS_TARGET_LUFS while still respecting the
# -1 dBTP ceiling.
VOICE_VOLUME = 1.3
VOICE_COMPRESSOR = "acompressor=threshold=-20dB:ratio=6:attack=5:release=80:makeup=4,alimiter=limit=0.9"
# -13 LUFS is the realistic ceiling for this voice+chain, not an arbitrary
# pick: edge-tts speech has a ~14dB crest factor even after the compressor
# above, and a -1dBTP safety ceiling only leaves that much headroom above
# whatever integrated loudness the content can reach without clipping.
# Asking loudnorm for something louder (e.g. -9) doesn't get louder output —
# it just hits the same TP ceiling, so this is already the real maximum.
LOUDNESS_TARGET_LUFS = -13

# One gradient background per mood so videos look different depending on
# what's going on for the student. Free, local, no AI video generation cost.
MOOD_BACKGROUND_SPECS: dict[str, dict] = {
    "streak": dict(type="linear", speed=0.035, colors=["0xd68910", "0x7d5109", "0xffe8b0"]),
    "comeback": dict(type="radial", speed=0.015, colors=["0xffb199", "0x2b5876", "0xfff3ee"]),
    "struggle": dict(type="spiral", speed=0.012, colors=["0x3a3a52", "0x6c5ce7", "0x2a2a3d"]),
    "welcome": dict(type="linear", speed=0.02, colors=["0x1a3cba", "0x4a5a7a", "0x7bdff2"]),
}


class VideoError(RuntimeError):
    pass


def _run(cmd: list[str]) -> None:
    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        raise VideoError(f"command failed: {' '.join(cmd[:3])}...\n{result.stderr[-4000:]}")


def _probe_duration_sec(path: Path) -> float:
    cmd = ["ffprobe", "-v", "error", "-show_entries", "format=duration", "-of", "json", str(path)]
    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        raise VideoError(f"ffprobe failed: {result.stderr[-2000:]}")
    return float(json.loads(result.stdout)["format"]["duration"])


async def synthesize_voice(text: str, out_path: Path) -> Path:
    out_path.parent.mkdir(parents=True, exist_ok=True)
    communicate = edge_tts.Communicate(text, VOICE)
    await communicate.save(str(out_path))
    return out_path


def _ensure_mood_background(mood: str) -> Path:
    CLIPS_DIR.mkdir(parents=True, exist_ok=True)
    path = CLIPS_DIR / f"placeholder_{mood}.mp4"
    if path.exists():
        return path

    spec = MOOD_BACKGROUND_SPECS.get(mood, MOOD_BACKGROUND_SPECS["comeback"])
    colors = spec["colors"]
    color_args = ":".join(f"c{i}={c}" for i, c in enumerate(colors))
    _run([
        "ffmpeg", "-y",
        "-f", "lavfi",
        "-i", (
            f"gradients=size={WIDTH}x{HEIGHT}:duration=15:rate=30:speed={spec['speed']}"
            f":type={spec['type']}:nb_colors={len(colors)}:{color_args}"
        ),
        "-c:v", "libx264", "-pix_fmt", "yuv420p", "-t", "15",
        str(path),
    ])
    return path


def _ensure_placeholder_music() -> Path:
    MUSIC_DIR.mkdir(parents=True, exist_ok=True)
    if not PLACEHOLDER_MUSIC.exists():
        _run([
            "ffmpeg", "-y",
            "-f", "lavfi", "-i", "sine=frequency=220:duration=30",
            "-f", "lavfi", "-i", "sine=frequency=330:duration=30",
            "-filter_complex", "[0:a][1:a]amix=inputs=2:duration=first,volume=0.5[aout]",
            "-map", "[aout]",
            str(PLACEHOLDER_MUSIC),
        ])
    return PLACEHOLDER_MUSIC


def _measure_premix_loudness(voice_path: Path, music_path: Path, total_duration: float) -> dict | None:
    """First pass of a two-pass loudnorm: single-pass 'dynamic' loudnorm
    alone tends to undershoot its target on a short clip (measured ~-14
    LUFS when asked for -9), so measure the real voice+music mix here and
    feed those exact numbers into a second, linear-gain loudnorm pass in
    compose_video for an accurate result. Returns None on any failure —
    caller falls back to plain single-pass loudnorm."""
    filter_complex = (
        f"[0:a]volume={VOICE_VOLUME},{VOICE_COMPRESSOR}[voice];"
        f"[1:a]volume={MUSIC_VOLUME}[music];"
        "[voice][music]amix=inputs=2:duration=first:dropout_transition=2:normalize=0[premix];"
        f"[premix]loudnorm=I={LOUDNESS_TARGET_LUFS}:TP=-1:LRA=11:print_format=json[out]"
    )
    cmd = [
        "ffmpeg", "-y",
        "-i", str(voice_path),
        "-stream_loop", "-1", "-i", str(music_path),
        "-filter_complex", filter_complex,
        "-map", "[out]", "-t", str(total_duration),
        "-f", "null", "-",
    ]
    result = subprocess.run(cmd, capture_output=True, text=True)
    match = re.search(r"\{[^{}]*\"input_i\"[^{}]*\}", result.stderr)
    if not match:
        return None
    try:
        return json.loads(match.group(0))
    except json.JSONDecodeError:
        return None


def _escape_drawtext(text: str) -> str:
    text = text.replace("\\", "\\\\")
    text = text.replace(":", "\\:")
    text = text.replace("'", "’")
    text = text.replace("%", "\\%")
    return text


def compose_video(
    voice_path: Path,
    overlay_text: str,
    mood: str,
    out_path: Path,
    thumb_path: Path,
    clip_path: Path | None = None,
    student_name: str | None = None,
) -> float:
    """Compose the final vertical video. Returns its duration in seconds.
    clip_path overrides the mood gradient — used for topic-specific
    backgrounds (see notification_topic_visuals.py). student_name, if given,
    renders as a distinct "Hi {name}!" overlay near the top — so the video
    reads as personalized visually, not just in the spoken voice."""
    clip_path = clip_path or _ensure_mood_background(mood)
    music_path = _ensure_placeholder_music()

    voice_duration = _probe_duration_sec(voice_path)
    total_duration = round(voice_duration + 0.5, 2)

    title_drawtext = (
        f"drawtext=fontfile={FONT_PATH}:text='{_escape_drawtext(overlay_text)}'"
        ":fontcolor=white:fontsize=56:line_spacing=10"
        ":x=(w-text_w)/2:y=h-500:box=1:boxcolor=black@0.45:boxborderw=24"
    )

    name_drawtext = ""
    if student_name:
        greeting = _escape_drawtext(f"Hi {student_name}!")
        name_drawtext = (
            f",drawtext=fontfile={FONT_PATH}:text='{greeting}'"
            ":fontcolor=0xffd166:fontsize=68:y=140:x=(w-text_w)/2"
            ":box=1:boxcolor=black@0.35:boxborderw=20"
        )

    measured = _measure_premix_loudness(voice_path, music_path, total_duration)
    if measured:
        loudnorm_audio = (
            f"loudnorm=I={LOUDNESS_TARGET_LUFS}:TP=-1:LRA=11:linear=true"
            f":measured_I={measured['input_i']}:measured_TP={measured['input_tp']}"
            f":measured_LRA={measured['input_lra']}:measured_thresh={measured['input_thresh']}"
        )
    else:
        loudnorm_audio = f"loudnorm=I={LOUDNESS_TARGET_LUFS}:TP=-1:LRA=11"

    filter_complex = (
        f"[0:v]scale={WIDTH}:{HEIGHT}:force_original_aspect_ratio=increase,"
        f"crop={WIDTH}:{HEIGHT},{title_drawtext}{name_drawtext}[vout];"
        f"[1:a]volume={VOICE_VOLUME},{VOICE_COMPRESSOR}[voice];"
        f"[2:a]volume={MUSIC_VOLUME}[music];"
        "[voice][music]amix=inputs=2:duration=first:dropout_transition=2:normalize=0[premix];"
        f"[premix]{loudnorm_audio}[aout]"
    )

    out_path.parent.mkdir(parents=True, exist_ok=True)
    _run([
        "ffmpeg", "-y",
        "-stream_loop", "-1", "-i", str(clip_path),
        "-i", str(voice_path),
        "-stream_loop", "-1", "-i", str(music_path),
        "-filter_complex", filter_complex,
        "-map", "[vout]", "-map", "[aout]",
        "-t", str(total_duration),
        # crf 18 + preset medium: visually crisp, worth the extra ~1-2s encode
        # time for a one-shot ~6s video (vs. veryfast's default-quality blur).
        "-c:v", "libx264", "-pix_fmt", "yuv420p", "-preset", "medium", "-crf", "18",
        "-c:a", "aac", "-b:a", "128k",
        "-movflags", "+faststart",
        str(out_path),
    ])

    thumb_path.parent.mkdir(parents=True, exist_ok=True)
    _run(["ffmpeg", "-y", "-i", str(out_path), "-ss", "0.5", "-frames:v", "1", str(thumb_path)])

    return total_duration


async def generate_video(
    uid: str,
    notification_id: str,
    title: str,
    body: str,
    mood: str,
    topic: str | None = None,
    student_name: str | None = None,
) -> dict:
    """Full pipeline: voice -> compose -> publish under MEDIA_DIR (served by
    FastAPI's StaticFiles mount). Returns
    {"videoUrl": str, "thumbUrl": str, "durationSec": float}.

    If `topic` matches a known physics-topic template, the background is an
    animated diagram of that concept instead of the generic mood gradient —
    see notification_topic_visuals.py. Falls back to the gradient for any
    other topic (including None) or if generation fails for any reason."""
    work = WORK_DIR / notification_id
    voice_path = work / "voice.mp3"
    video_path = work / "video.mp4"
    thumb_path = work / "thumb.jpg"

    await synthesize_voice(body, voice_path)

    topic_clip = None
    if topic:
        try:
            from app.services import notification_topic_visuals

            topic_clip = notification_topic_visuals.ensure_topic_background(topic)
        except Exception:
            topic_clip = None

    duration = compose_video(
        voice_path, title, mood, video_path, thumb_path,
        clip_path=topic_clip, student_name=student_name,
    )

    dest_dir = MEDIA_DIR / uid
    dest_dir.mkdir(parents=True, exist_ok=True)
    final_video = dest_dir / f"{notification_id}.mp4"
    final_thumb = dest_dir / f"{notification_id}_thumb.jpg"
    final_video.write_bytes(video_path.read_bytes())
    final_thumb.write_bytes(thumb_path.read_bytes())

    video_url = f"{MEDIA_BASE_URL}/media/notifications/{uid}/{notification_id}.mp4"
    thumb_url = f"{MEDIA_BASE_URL}/media/notifications/{uid}/{notification_id}_thumb.jpg"

    return {"videoUrl": video_url, "thumbUrl": thumb_url, "durationSec": duration}
