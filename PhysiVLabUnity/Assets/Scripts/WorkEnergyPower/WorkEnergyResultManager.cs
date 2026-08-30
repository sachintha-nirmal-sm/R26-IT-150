using TMPro;
using UnityEngine;

public class WorkEnergyResultManager : MonoBehaviour
{
    public static WorkEnergyResultManager Instance { get; private set; }

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

    public void ShowResult(int score, bool passed, int mistakes, WorkEnergyAttemptRecord attempt)
    {
        string perf = WorkEnergyProfileManager.Instance != null ? WorkEnergyProfileManager.Instance.GetPerformanceLabel(score) : "";
        if (finalScoreText != null)
        {
            finalScoreText.text =
                "PRACTICAL COMPLETED\n\n" +
                "WORK, ENERGY AND POWER\n" +
                "Variation of Potential Energy with Height\n\n" +
                $"Score: {score} / 100\n" +
                $"Percentage: {score}%\n" +
                $"Performance: {perf}\n" +
                $"Mistakes: {mistakes}\n" +
                $"Attempt: {(attempt != null ? attempt.attemptNumber : 1)}";
        }

        if (resultDetailsText != null)
        {
            float mass = WorkEnergyExperimentDataManager.Instance != null ? WorkEnergyExperimentDataManager.Instance.StoredMass : 1f;
            string details = "EXPERIMENT RESULTS\n\n";
            details += $"Mass = {mass:0.00} kg\n\n";
            if (attempt != null && attempt.readings != null)
            {
                foreach (var r in attempt.readings)
                    details += $"Height = {r.height:0.00} m    PE = {r.potentialEnergy:0.00} J    Depth = {r.depressionDepth:0.0} cm\n";
            }
            details +=
                "\nCONCLUSION\n" +
                "As height increases, gravitational potential energy increases.\n" +
                "The impact effect also generally increases, producing a deeper depression in the clay.";
            resultDetailsText.text = details;
        }

        if (statusText != null)
        {
            statusText.text = passed ? "STATUS: PASSED" : "STATUS: NEEDS IMPROVEMENT";
            statusText.color = passed ? new Color(0.1f, 0.55f, 0.2f) : new Color(0.75f, 0.15f, 0.15f);
        }
    }
}
