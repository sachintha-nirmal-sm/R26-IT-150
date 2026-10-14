using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurningResultManager : MonoBehaviour
{
    public static TurningResultManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI resultDetailsText;
    [SerializeField] private TextMeshProUGUI statusText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(TextMeshProUGUI score, TextMeshProUGUI details, TextMeshProUGUI status)
    {
        finalScoreText = score;
        resultDetailsText = details;
        statusText = status;
    }

    public void ShowResult(int score, bool passed, int mistakes, TurningAttemptRecord attempt)
    {
        string perf = TurningProfileManager.Instance != null ? TurningProfileManager.Instance.GetPerformanceLabel(score) : "";
        int best = TurningProfileManager.Instance != null && TurningProfileManager.Instance.ProfileData != null
            ? TurningProfileManager.Instance.ProfileData.bestScore
            : score;
        int attempts = attempt != null ? attempt.attemptNumber : 1;
        var trials = TurningTrialManager.Instance != null ? TurningTrialManager.Instance.GetAllTrials() : null;

        if (finalScoreText != null)
        {
            finalScoreText.text =
                "PRACTICAL COMPLETED\n\n" +
                "TURNING EFFECT OF A FORCE\n" +
                "Activity 2 — Investigating the turning effect of a force\n\n" +
                $"Score: {score} / 100\n" +
                $"Percentage: {score}%\n" +
                $"Performance: {perf}\n" +
                $"Best Score: {best} / 100\n" +
                $"Attempts: {attempts}\n" +
                $"Mistakes: {mistakes}";
        }

        if (resultDetailsText != null)
        {
            string details = "TURNING EFFECT PRACTICAL RESULTS\n\n";
            if (trials != null)
            {
                foreach (var t in trials)
                {
                    if (t == null) continue;
                    details += t.completed
                        ? $"Trial {t.trialNumber}: tightness {t.tightnessLevel},  F = {t.forceN:0.0} N at {t.angleDeg:0}°,  d = {t.distanceCm:0} cm,  moment = {t.momentNm:0.00} N m\n"
                        : $"Trial {t.trialNumber}: not completed\n";
                }
            }
            details +=
                "\nTrial 1: Completed " + Mark(trials, 1) + "\n" +
                "Trial 2: Completed " + Mark(trials, 2) + "\n" +
                "Trial 3: Completed " + Mark(trials, 3) + "\n\n" +
                "CONCLUSION\n" +
                "The turning effect of a force is the moment, equal to force times the perpendicular distance from the pivot.\n" +
                "Tightening the screw increases friction at O, so a larger force is needed to turn the stick.";
            resultDetailsText.text = details;
        }

        if (statusText != null)
        {
            statusText.text = passed ? "STATUS: COMPLETED" : "STATUS: NEEDS IMPROVEMENT";
            statusText.color = passed ? new Color(0.1f, 0.55f, 0.2f) : new Color(0.75f, 0.15f, 0.15f);
        }
    }

    private static string Mark(List<TurningTrialData> trials, int n)
    {
        if (trials == null || n < 1 || n > trials.Count) return "✗";
        return trials[n - 1] != null && trials[n - 1].completed ? "✓" : "✗";
    }
}
