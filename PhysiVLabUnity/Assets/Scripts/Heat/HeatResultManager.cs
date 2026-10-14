using TMPro;
using UnityEngine;

public class HeatResultManager : MonoBehaviour
{
    public static HeatResultManager Instance { get; private set; }

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

    public void ShowResult(int score, bool passed, int mistakes, HeatAttemptRecord attempt)
    {
        string perf = HeatProfileManager.Instance != null ? HeatProfileManager.Instance.GetPerformanceLabel(score) : "";
        int best = HeatProfileManager.Instance != null && HeatProfileManager.Instance.ProfileData != null
            ? HeatProfileManager.Instance.ProfileData.bestScore
            : score;
        int attempts = attempt != null ? attempt.attemptNumber : 1;

        if (finalScoreText != null)
        {
            finalScoreText.text =
                "PRACTICAL COMPLETED\n\n" +
                "HEAT\n" +
                "Illustrating expansion of liquids\n\n" +
                $"Score: {score} / 100\n" +
                $"Percentage: {score}%\n" +
                $"Performance: {perf}\n" +
                $"Best Score: {best} / 100\n" +
                $"Attempts: {attempts}\n" +
                $"Mistakes: {mistakes}";
        }

        if (resultDetailsText != null)
        {
            bool eq = attempt != null && attempt.selectedCorrectEquipment;
            bool assembled = attempt != null && attempt.apparatusAssembled;
            bool marked = attempt != null && attempt.markedLevelA;
            bool drop = attempt != null && attempt.observedDropToB;
            bool rise = attempt != null && attempt.observedRiseToC;
            bool id = attempt != null && attempt.identifiedLevels;
            resultDetailsText.text =
                "HEAT — RESULTS\n\n" +
                "Apparatus selected: " + (eq ? "✓" : "✗") + "\n" +
                "Apparatus assembled: " + (assembled ? "✓" : "✗") + "\n" +
                "Level A marked: " + (marked ? "✓" : "✗") + "\n" +
                "Fall to B observed: " + (drop ? "✓" : "✗") + "\n" +
                "Rise to C observed: " + (rise ? "✓" : "✗") + "\n" +
                "Levels A, B, C explained: " + (id ? "✓" : "✗") + "\n\n" +
                "CONCLUSION\n" +
                "When heat is applied, the glass container expands first, so the liquid level falls slightly from A to B. " +
                "Then the liquid expands more than the glass, so the level rises from B, past A, to C.";
        }

        if (statusText != null)
        {
            statusText.text = passed ? "STATUS: COMPLETED" : "STATUS: NEEDS IMPROVEMENT";
            statusText.color = passed ? new Color(0.1f, 0.55f, 0.2f) : new Color(0.75f, 0.15f, 0.15f);
        }
    }
}
