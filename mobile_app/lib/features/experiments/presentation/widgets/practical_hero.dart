import 'package:flutter/material.dart';

import '../../data/practical.dart';
import '../../data/practical_guide.dart';

class PracticalHeroCard extends StatelessWidget {
  const PracticalHeroCard({
    super.key,
    required this.practical,
    this.compact = false,
  });

  final Practical practical;
  final bool compact;

  @override
  Widget build(BuildContext context) {
    final guide = PracticalGuide.forPractical(practical.id, title: practical.title);
    final radius = compact
        ? const BorderRadius.vertical(top: Radius.circular(24))
        : BorderRadius.circular(24);

    return Container(
      width: double.infinity,
      constraints: BoxConstraints(minHeight: compact ? 156 : 200),
      decoration: BoxDecoration(
        borderRadius: radius,
        gradient: LinearGradient(
          colors: [
            guide.color,
            Color.lerp(guide.color, guide.accent, 0.45) ?? guide.accent,
          ],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        boxShadow: compact
            ? null
            : [
                BoxShadow(
                  color: guide.color.withValues(alpha: 0.28),
                  blurRadius: 18,
                  offset: const Offset(0, 8),
                ),
              ],
      ),
      child: ClipRRect(
        borderRadius: radius,
        child: Stack(
          children: [
            Positioned(
              right: -28,
              top: -24,
              child: Icon(
                guide.icon,
                size: compact ? 140 : 180,
                color: Colors.white.withValues(alpha: 0.12),
              ),
            ),
            Positioned(
              left: -20,
              bottom: -30,
              child: Container(
                width: 110,
                height: 110,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: Colors.white.withValues(alpha: 0.08),
                ),
              ),
            ),
            Padding(
              padding: EdgeInsets.fromLTRB(18, compact ? 14 : 18, 18, 16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisSize: MainAxisSize.min,
                children: [
                  Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.all(10),
                        decoration: BoxDecoration(
                          color: Colors.white.withValues(alpha: 0.18),
                          borderRadius: BorderRadius.circular(14),
                        ),
                        child: Icon(guide.icon, color: Colors.white, size: 26),
                      ),
                      const Spacer(),
                      _Pill(text: 'Grade ${practical.grade}'),
                      const SizedBox(width: 8),
                      _Pill(text: practical.durationLabel),
                    ],
                  ),
                  SizedBox(height: compact ? 18 : 22),
                  Text(
                    guide.headline,
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: compact ? 20 : 24,
                      fontWeight: FontWeight.w800,
                      height: 1.15,
                    ),
                  ),
                  const SizedBox(height: 6),
                  Text(
                    guide.goal,
                    maxLines: compact ? 2 : 3,
                    overflow: TextOverflow.ellipsis,
                    style: TextStyle(
                      color: Colors.white.withValues(alpha: 0.92),
                      fontSize: compact ? 12.5 : 14,
                      height: 1.35,
                    ),
                  ),
                  if (!compact) ...[
                    const SizedBox(height: 12),
                    Wrap(
                      spacing: 8,
                      runSpacing: 8,
                      children: [
                        for (final item in guide.kit.take(4))
                          _Pill(text: item, filled: true),
                      ],
                    ),
                  ],
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class PracticalStartTrialBar extends StatelessWidget {
  const PracticalStartTrialBar({
    super.key,
    required this.practical,
    required this.busy,
    required this.onStart,
    required this.onTrial,
  });

  final Practical practical;
  final bool busy;
  final VoidCallback? onStart;
  final VoidCallback? onTrial;

  @override
  Widget build(BuildContext context) {
    final guide = PracticalGuide.forPractical(
      practical.id,
      title: practical.title,
    );
    final retry = practical.canRetryOfficial;

    return Column(
      children: [
        _LabCta(
          enabled: onStart != null && !busy,
          onTap: onStart,
          filled: true,
          color: guide.color,
          accent: guide.accent,
          icon: retry ? Icons.refresh_rounded : Icons.play_arrow_rounded,
          title: retry ? 'Retry Start' : 'Start',
          subtitle: 'Official run · timed · saves to profile',
          height: 64,
        ),
        const SizedBox(height: 12),
        _LabCta(
          enabled: onTrial != null && !busy,
          onTap: onTrial,
          filled: false,
          color: guide.color,
          accent: guide.accent,
          icon: Icons.science_outlined,
          title: 'Trial',
          subtitle: 'Practice only · score is not saved',
          height: 56,
        ),
      ],
    );
  }
}

class _LabCta extends StatelessWidget {
  const _LabCta({
    required this.enabled,
    required this.onTap,
    required this.filled,
    required this.color,
    required this.accent,
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.height,
  });

  final bool enabled;
  final VoidCallback? onTap;
  final bool filled;
  final Color color;
  final Color accent;
  final IconData icon;
  final String title;
  final String subtitle;
  final double height;

  @override
  Widget build(BuildContext context) {
    final bg = !enabled
        ? const Color(0xFFE8ECF1)
        : filled
            ? color
            : accent.withValues(alpha: 0.28);
    final fg = !enabled
        ? const Color(0xFF98A2B3)
        : filled
            ? Colors.white
            : color;
    final border = filled
        ? null
        : Border.all(
            color: enabled ? color.withValues(alpha: 0.45) : const Color(0xFFD0D5DD),
            width: 1.6,
          );

    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: enabled ? onTap : null,
        borderRadius: BorderRadius.circular(18),
        child: Ink(
          height: height,
          width: double.infinity,
          decoration: BoxDecoration(
            color: bg,
            borderRadius: BorderRadius.circular(18),
            border: border,
            boxShadow: enabled && filled
                ? [
                    BoxShadow(
                      color: color.withValues(alpha: 0.32),
                      blurRadius: 14,
                      offset: const Offset(0, 6),
                    ),
                  ]
                : null,
          ),
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 18),
            child: Row(
              children: [
                Container(
                  width: 40,
                  height: 40,
                  decoration: BoxDecoration(
                    color: filled
                        ? Colors.white.withValues(alpha: enabled ? 0.18 : 0.4)
                        : color.withValues(alpha: enabled ? 0.12 : 0.06),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Icon(icon, color: fg, size: filled ? 26 : 22),
                ),
                const SizedBox(width: 14),
                Expanded(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        title,
                        style: TextStyle(
                          color: fg,
                          fontSize: filled ? 18 : 16,
                          fontWeight: FontWeight.w800,
                          height: 1.1,
                        ),
                      ),
                      const SizedBox(height: 3),
                      Text(
                        subtitle,
                        style: TextStyle(
                          color: fg.withValues(alpha: enabled ? 0.85 : 0.7),
                          fontSize: 12,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                    ],
                  ),
                ),
                Icon(
                  Icons.arrow_forward_rounded,
                  color: fg.withValues(alpha: 0.85),
                  size: filled ? 22 : 20,
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class PracticalInstructionsCard extends StatelessWidget {
  const PracticalInstructionsCard({super.key, required this.practical});

  final Practical practical;

  @override
  Widget build(BuildContext context) {
    final guide = PracticalGuide.forPractical(practical.id, title: practical.title);

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: const Color(0xFFF7FAFF),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: guide.color.withValues(alpha: 0.18)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(Icons.menu_book_rounded, color: guide.color, size: 22),
              const SizedBox(width: 8),
              const Text(
                'How to do this lab',
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.w800,
                  color: Color(0xFF1A1C1E),
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          Text(
            'Trial is a free practice run. Start is timed, scored /100, and saved to your profile.',
            style: TextStyle(fontSize: 13, height: 1.4, color: Colors.grey.shade700),
          ),
          const SizedBox(height: 14),
          for (var i = 0; i < guide.steps.length; i++)
            Padding(
              padding: const EdgeInsets.only(bottom: 10),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Container(
                    width: 26,
                    height: 26,
                    alignment: Alignment.center,
                    decoration: BoxDecoration(
                      color: guide.color,
                      shape: BoxShape.circle,
                    ),
                    child: Text(
                      '${i + 1}',
                      style: const TextStyle(
                        color: Colors.white,
                        fontWeight: FontWeight.w700,
                        fontSize: 13,
                      ),
                    ),
                  ),
                  const SizedBox(width: 10),
                  Expanded(
                    child: Padding(
                      padding: const EdgeInsets.only(top: 3),
                      child: Text(
                        guide.steps[i],
                        style: const TextStyle(
                          fontSize: 14,
                          height: 1.4,
                          color: Color(0xFF334155),
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: guide.accent.withValues(alpha: 0.35),
              borderRadius: BorderRadius.circular(14),
            ),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Icon(Icons.lightbulb_outline, color: guide.color, size: 20),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    guide.tip,
                    style: TextStyle(
                      fontSize: 13,
                      height: 1.4,
                      color: guide.color.withValues(alpha: 0.95),
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _Pill extends StatelessWidget {
  const _Pill({required this.text, this.filled = false});

  final String text;
  final bool filled;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
      decoration: BoxDecoration(
        color: filled ? Colors.white.withValues(alpha: 0.2) : Colors.black.withValues(alpha: 0.22),
        borderRadius: BorderRadius.circular(20),
      ),
      child: Text(
        text,
        style: const TextStyle(
          color: Colors.white,
          fontSize: 11,
          fontWeight: FontWeight.w700,
        ),
      ),
    );
  }
}
