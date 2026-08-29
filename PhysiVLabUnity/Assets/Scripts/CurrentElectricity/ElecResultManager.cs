using TMPro;
using UnityEngine;

public class ElecResultManager : MonoBehaviour
{
    public static ElecResultManager Instance { get; private set; }

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

    public void ShowResult(int score, bool passed, int mistakes, ElecAttemptRecord attempt)
    {
        string perf = ElecProfileManager.Instance != null ? ElecProfileManager.Instance.GetPerformanceLabel(score) : "";
        int best = ElecProfileManager.Instance != null && ElecProfileManager.Instance.ProfileData != null
            ? ElecProfileManager.Instance.ProfileData.bestScore
            : score;

        if (finalScoreText != null)
        {
            finalScoreText.text =
                "PRACTICAL COMPLETED\n\n" +
                "CURRENT ELECTRICITY\n" +
                "Investigating Different Connections of Two Dry Cells\n\n" +
                $"Score: {score} / 100\n" +
                $"Percentage: {score}%\n" +
                $"Performance: {perf}\n" +
                $"Best Score: {best} / 100\n" +
                $"Attempts: {(attempt != null ? attempt.attemptNumber : 1)}\n" +
                $"Mistakes: {mistakes}";
        }

        if (resultDetailsText != null)
        {
            string details = "RESULTS\n\n";
            var readings = attempt != null ? attempt.readings : (ElecExperimentDataManager.Instance != null ? ElecExperimentDataManager.Instance.Readings : null);
            if (readings != null)
            {
                foreach (var r in readings)
                {
                    if (r == null || r.connectionNumber < 1) continue;
                    details +=
                        $"Connection {r.connectionNumber} ({r.arrangement}):\n" +
                        $"Voltage = {r.voltage:0.00} V    Current = {r.current:0.00} A\n" +
                        $"Brightness = {r.brightnessLabel}    Power = {r.power:0.00} W\n\n";
                }
            }
            details +=
                "CONCLUSION\n" +
                "The arrangement of cells affects the potential difference and current in the circuit.\n" +
                "When identical cells are connected in series aiding, their potential differences add.\n" +
                "When identical cells are connected in parallel, the potential difference is approximately equal to that of one cell.\n" +
                "When identical cells are connected in series opposition, their potential differences oppose each other and the net potential difference is approximately zero.";
            resultDetailsText.text = details;
        }

        if (statusText != null)
        {
            statusText.text = passed ? "STATUS: COMPLETED" : "STATUS: NEEDS IMPROVEMENT";
            statusText.color = passed ? new Color(0.1f, 0.55f, 0.2f) : new Color(0.75f, 0.15f, 0.15f);
        }
    }
}
