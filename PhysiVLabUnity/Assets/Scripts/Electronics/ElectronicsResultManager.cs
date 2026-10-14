using TMPro;
using UnityEngine;

public class ElectronicsResultManager : MonoBehaviour
{
    public static ElectronicsResultManager Instance { get; private set; }

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

    public void ShowResult(int score, bool passed, int mistakes, ElectronicsAttemptRecord attempt)
    {
        string perf = ElectronicsProfileManager.Instance != null ? ElectronicsProfileManager.Instance.GetPerformanceLabel(score) : "";
        int best = ElectronicsProfileManager.Instance != null && ElectronicsProfileManager.Instance.ProfileData != null
            ? ElectronicsProfileManager.Instance.ProfileData.bestScore
            : score;
        int attempts = attempt != null ? attempt.attemptNumber : 1;

        if (finalScoreText != null)
        {
            finalScoreText.text =
                "PRACTICAL COMPLETED\n\n" +
                "ELECTRONICS\n" +
                "Forward Bias and Reverse Bias of a Diode\n\n" +
                $"Score:  {score} / 100\n" +
                $"Percentage:  {score}%\n" +
                $"Performance:  {perf}\n" +
                $"Best Score:  {best} / 100\n" +
                $"Attempts:  {attempts} / 3\n\n" +
                "Forward Bias:  " + Tick(attempt != null && attempt.forwardBiasCompleted) + "\n" +
                "Reverse Bias:  " + Tick(attempt != null && attempt.reverseBiasCompleted) + "\n" +
                "Observation:  " + Tick(attempt != null && attempt.observationCompleted) + "\n" +
                "Comparison:  Completed\n" +
                "Questions:  " + Tick(attempt != null && attempt.questionsCompleted) + "\n" +
                "Conclusion:  " + Tick(attempt != null && attempt.conclusionCompleted);
        }

        if (resultDetailsText != null)
            resultDetailsText.text = BuildDetails();

        if (statusText != null)
        {
            statusText.text = passed ? "STATUS: COMPLETED" : "STATUS: NEEDS IMPROVEMENT";
            statusText.color = passed ? new Color(0.1f, 0.55f, 0.2f) : new Color(0.75f, 0.15f, 0.15f);
        }
    }

    private static string Tick(bool ok) => ok ? "✓ Completed" : "—";

    private static string BuildDetails()
    {
        var obs = ElectronicsObservationManager.Instance;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("PRACTICAL RESULTS");
        sb.AppendLine();
        sb.AppendLine("Forward Bias:");
        sb.AppendLine("  Diode: Forward biased");
        sb.AppendLine("  Bulb: Glowing");
        sb.AppendLine("  Current: Flowing");
        if (obs != null && obs.Forward != null && !string.IsNullOrEmpty(obs.Forward.observationText))
            sb.AppendLine("  Observation: " + obs.Forward.observationText);
        sb.AppendLine();
        sb.AppendLine("Reverse Bias:");
        sb.AppendLine("  Diode: Reverse biased");
        sb.AppendLine("  Bulb: Not glowing");
        sb.AppendLine("  Current: Blocked");
        if (obs != null && obs.Reverse != null && !string.IsNullOrEmpty(obs.Reverse.observationText))
            sb.AppendLine("  Observation: " + obs.Reverse.observationText);
        sb.AppendLine();
        sb.AppendLine("Conclusion:");
        sb.AppendLine("The diode allows current to flow mainly in one direction. In forward bias the bulb glows, while in reverse bias the bulb does not glow.");
        return sb.ToString();
    }
}
