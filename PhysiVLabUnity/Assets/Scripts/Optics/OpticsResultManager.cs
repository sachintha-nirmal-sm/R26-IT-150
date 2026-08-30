using TMPro;
using UnityEngine;

public class OpticsResultManager : MonoBehaviour
{
    public static OpticsResultManager Instance { get; private set; }

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

    public void ShowResult(int score, bool passed, int mistakes, OpticsAttemptRecord attempt)
    {
        string perf = OpticsProfileManager.Instance != null ? OpticsProfileManager.Instance.GetPerformanceLabel(score) : "";
        int best = OpticsProfileManager.Instance != null && OpticsProfileManager.Instance.ProfileData != null
            ? OpticsProfileManager.Instance.ProfileData.bestScore
            : score;
        int attempts = attempt != null ? attempt.attemptNumber : 1;

        if (finalScoreText != null)
        {
            finalScoreText.text =
                "PRACTICAL COMPLETED\n\n" +
                "GEOMETRICAL OPTICS\n" +
                "Finding the approximate focal length of a concave mirror using a distant object\n\n" +
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
            bool win = attempt != null && attempt.windowOpened;
            bool mir = attempt != null && attempt.mirrorFacingWindow;
            bool sharp = attempt != null && attempt.foundSharpImage;
            bool meas = attempt != null && attempt.measuredFocalLength;
            resultDetailsText.text =
                "GEOMETRICAL OPTICS — RESULTS\n\n" +
                "Apparatus selected: " + (eq ? "✓" : "✗") + "\n" +
                "Window opened: " + (win ? "✓" : "✗") + "\n" +
                "Concave mirror facing window: " + (mir ? "✓" : "✗") + "\n" +
                "Clear inverted image found: " + (sharp ? "✓" : "✗") + "\n" +
                "Focal length measured: " + (meas ? "✓" : "✗") + "\n\n" +
                "CONCLUSION\n" +
                "Because light rays from a far-away object can be considered as parallel, " +
                "the distance from the mirror to the sharp real image on the screen is approximately " +
                "equal to the focal length of the concave mirror.";
        }

        if (statusText != null)
        {
            statusText.text = passed ? "STATUS: COMPLETED" : "STATUS: NEEDS IMPROVEMENT";
            statusText.color = passed ? new Color(0.1f, 0.55f, 0.2f) : new Color(0.75f, 0.15f, 0.15f);
        }
    }
}
