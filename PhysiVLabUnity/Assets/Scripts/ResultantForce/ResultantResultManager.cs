using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultantResultManager : MonoBehaviour
{
    public static ResultantResultManager Instance { get; private set; }

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

    public void ShowResult(int score, bool passed, int mistakes, ResultantAttemptRecord attempt)
    {
        string perf = ResultantProfileManager.Instance != null ? ResultantProfileManager.Instance.GetPerformanceLabel(score) : "";
        int best = ResultantProfileManager.Instance != null && ResultantProfileManager.Instance.ProfileData != null
            ? ResultantProfileManager.Instance.ProfileData.bestScore
            : score;
        int attempts = attempt != null ? attempt.attemptNumber : 1;
        var trials = ResultantTrialManager.Instance != null ? ResultantTrialManager.Instance.GetAllTrials() : null;

        if (finalScoreText != null)
        {
            finalScoreText.text =
                "PRACTICAL COMPLETED\n\n" +
                "RESULTANT FORCE\n" +
                "Two forces acting in the same direction\n\n" +
                $"Score: {score} / 100\n" +
                $"Percentage: {score}%\n" +
                $"Performance: {perf}\n" +
                $"Best Score: {best} / 100\n" +
                $"Attempts: {attempts}\n" +
                $"Mistakes: {mistakes}";
        }

        if (resultDetailsText != null)
        {
            string details = "RESULTANT FORCE PRACTICAL RESULTS\n\n";
            if (trials != null)
            {
                foreach (var t in trials)
                {
                    if (t == null) continue;
                    details += t.completed
                        ? $"Trial {t.trialNumber}: A = {t.forceA:0.0} N,  B = {t.forceB:0.0} N,  C = {t.forceC:0.0} N    (B+C = {(t.forceB + t.forceC):0.0} N)\n"
                        : $"Trial {t.trialNumber}: not completed\n";
                }
            }
            details +=
                "\nTrial 1: Completed " + Mark(trials, 1) + "\n" +
                "Trial 2: Completed " + Mark(trials, 2) + "\n" +
                "Trial 3: Completed " + Mark(trials, 3) + "\n\n" +
                "CONCLUSION\n" +
                "The resultant force is equal to the sum of the two forces acting in the same direction.\n" +
                "Force A = Force B + Force C";
            resultDetailsText.text = details;
        }

        if (statusText != null)
        {
            statusText.text = passed ? "STATUS: COMPLETED" : "STATUS: NEEDS IMPROVEMENT";
            statusText.color = passed ? new Color(0.1f, 0.55f, 0.2f) : new Color(0.75f, 0.15f, 0.15f);
        }
    }

    private static string Mark(List<ResultantTrialData> trials, int n)
    {
        if (trials == null || n < 1 || n > trials.Count) return "✗";
        return trials[n - 1] != null && trials[n - 1].completed ? "✓" : "✗";
    }
}
