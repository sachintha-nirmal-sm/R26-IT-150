using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FrictionResultManager : MonoBehaviour
{
    public static FrictionResultManager Instance { get; private set; }

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

    public void ShowResult(int score, bool passed, int mistakes, FrictionAttemptRecord attempt)
    {
        string perf = FrictionProfileManager.Instance != null ? FrictionProfileManager.Instance.GetPerformanceLabel(score) : "";
        int best = FrictionProfileManager.Instance != null && FrictionProfileManager.Instance.ProfileData != null
            ? FrictionProfileManager.Instance.ProfileData.bestScore
            : score;
        int attempts = attempt != null ? attempt.attemptNumber : 1;
        var trials = FrictionTrialManager.Instance != null ? FrictionTrialManager.Instance.GetAllTrials() : null;

        if (finalScoreText != null)
        {
            finalScoreText.text =
                "PRACTICAL COMPLETED\n\n" +
                "FRICTION\n" +
                "Investigation of the Influence of Surface Area on Friction\n\n" +
                $"Score: {score} / 100\n" +
                $"Percentage: {score}%\n" +
                $"Performance: {perf}\n" +
                $"Best Score: {best} / 100\n" +
                $"Attempts: {attempts}\n" +
                $"Mistakes: {mistakes}";
        }

        if (resultDetailsText != null)
        {
            string details =
                "FRICTION PRACTICAL RESULTS\n\n";
            if (trials != null)
            {
                foreach (var t in trials)
                {
                    if (t == null) continue;
                    details += $"Surface {t.surfaceName}: Area = {t.contactArea:0} cm²    Limiting friction = {(t.completed ? t.limitingFriction.ToString("0.0") : "—")} N\n";
                }
            }
            details +=
                "\nTrial 1: Completed " + Mark(trials, 1) + "\n" +
                "Trial 2: Completed " + Mark(trials, 2) + "\n" +
                "Trial 3: Completed " + Mark(trials, 3) + "\n\n" +
                "CONCLUSION\n" +
                "The limiting frictional force does not significantly depend on the area of contact when weight and surface roughness remain constant.\n\n" +
                "The small differences between readings are experimental/measurement variations.";
            resultDetailsText.text = details;
        }

        if (statusText != null)
        {
            statusText.text = passed ? "STATUS: COMPLETED" : "STATUS: NEEDS IMPROVEMENT";
            statusText.color = passed ? new Color(0.1f, 0.55f, 0.2f) : new Color(0.75f, 0.15f, 0.15f);
        }
    }

    private static string Mark(List<FrictionTrialData> trials, int n)
    {
        if (trials == null || n < 1 || n > trials.Count) return "✗";
        return trials[n - 1] != null && trials[n - 1].completed ? "✓" : "✗";
    }
}
