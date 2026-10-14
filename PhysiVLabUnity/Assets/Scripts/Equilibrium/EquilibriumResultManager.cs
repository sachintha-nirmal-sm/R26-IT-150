using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EquilibriumResultManager : MonoBehaviour
{
    public static EquilibriumResultManager Instance { get; private set; }

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

    public void ShowResult(int score, bool passed, int mistakes, EquilibriumAttemptRecord attempt)
    {
        string perf = EquilibriumProfileManager.Instance != null ? EquilibriumProfileManager.Instance.GetPerformanceLabel(score) : "";
        int best = EquilibriumProfileManager.Instance != null && EquilibriumProfileManager.Instance.ProfileData != null
            ? EquilibriumProfileManager.Instance.ProfileData.bestScore
            : score;
        int attempts = attempt != null ? attempt.attemptNumber : 1;
        var trials = EquilibriumTrialManager.Instance != null ? EquilibriumTrialManager.Instance.GetAllTrials() : null;

        if (finalScoreText != null)
        {
            finalScoreText.text =
                "PRACTICAL COMPLETED\n\n" +
                "EQUILIBRIUM OF FORCES\n" +
                "Activity — Equilibrium of a meter ruler under three coplanar parallel forces\n\n" +
                $"Score: {score} / 100\n" +
                $"Percentage: {score}%\n" +
                $"Performance: {perf}\n" +
                $"Best Score: {best} / 100\n" +
                $"Attempts: {attempts}\n" +
                $"Mistakes: {mistakes}";
        }

        if (resultDetailsText != null)
        {
            string details = "EQUILIBRIUM OF FORCES — RESULTS\n\n";
            float w = EquilibriumForceController.TrueWeightN;
            details += $"Measured weight W = {w:0.00} N\n\n";
            if (trials != null)
            {
                foreach (var t in trials)
                {
                    if (t == null) continue;
                    details += t.completed
                        ? $"Trial {t.trialNumber}:  F1 = {t.force1N:0.00} N,  F2 = {t.force2N:0.00} N,  F1+F2 = {t.sumN:0.00} N,  W = {t.weightN:0.00} N\n"
                        : $"Trial {t.trialNumber}: not completed\n";
                }
            }
            details +=
                "\nTrial 1: Completed " + Mark(trials, 1) + "\n" +
                "Trial 2: Completed " + Mark(trials, 2) + "\n" +
                "Trial 3: Completed " + Mark(trials, 3) + "\n\n" +
                "CONCLUSION\n" +
                "For a meter ruler in equilibrium under three coplanar parallel forces, F1 + F2 = W.";
            resultDetailsText.text = details;
        }

        if (statusText != null)
        {
            statusText.text = passed ? "STATUS: COMPLETED" : "STATUS: NEEDS IMPROVEMENT";
            statusText.color = passed ? new Color(0.1f, 0.55f, 0.2f) : new Color(0.75f, 0.15f, 0.15f);
        }
    }

    private static string Mark(List<EquilibriumTrialData> trials, int n)
    {
        if (trials == null || n < 1 || n > trials.Count) return "✗";
        return trials[n - 1] != null && trials[n - 1].completed ? "✓" : "✗";
    }
}
