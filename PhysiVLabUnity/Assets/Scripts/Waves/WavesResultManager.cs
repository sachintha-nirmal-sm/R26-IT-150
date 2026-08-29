using TMPro;
using UnityEngine;

public class WavesResultManager : MonoBehaviour
{
    public static WavesResultManager Instance { get; private set; }

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

    public void ShowResult(int score, bool passed, int mistakes, WavesAttemptRecord attempt)
    {
        string perf = WavesProfileManager.Instance != null ? WavesProfileManager.Instance.GetPerformanceLabel(score) : "";
        int best = WavesProfileManager.Instance != null && WavesProfileManager.Instance.ProfileData != null
            ? WavesProfileManager.Instance.ProfileData.bestScore
            : score;
        int attempts = attempt != null ? attempt.attemptNumber : 1;

        if (finalScoreText != null)
        {
            finalScoreText.text =
                "PRACTICAL COMPLETED\n\n" +
                "WAVES AND THEIR APPLICATIONS\n" +
                "4.5 — Demonstration of the formation of transverse waves using a slinky\n\n" +
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
            bool ribbons = attempt != null && attempt.tiedRibbons;
            bool trans = attempt != null && attempt.shookTransverse;
            bool perp = attempt != null && attempt.identifiedPerpendicularMotion;
            resultDetailsText.text =
                "WAVES — RESULTS\n\n" +
                "Apparatus selected: " + (eq ? "✓" : "✗") + "\n" +
                "Ribbons tied along slinky: " + (ribbons ? "✓" : "✗") + "\n" +
                "Side-to-side (transverse) shake: " + (trans ? "✓" : "✗") + "\n" +
                "Particle motion identified: " + (perp ? "✓" : "✗") + "\n\n" +
                "CONCLUSION\n" +
                "When a slinky is shaken from side to side, a transverse wave travels along the slinky, " +
                "while the ribbons (particles of the medium) move perpendicular to the direction of the wave.";
        }

        if (statusText != null)
        {
            statusText.text = passed ? "STATUS: COMPLETED" : "STATUS: NEEDS IMPROVEMENT";
            statusText.color = passed ? new Color(0.1f, 0.55f, 0.2f) : new Color(0.75f, 0.15f, 0.15f);
        }
    }
}
