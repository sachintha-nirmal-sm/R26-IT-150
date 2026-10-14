using TMPro;
using UnityEngine;

public class MotionResultManager : MonoBehaviour
{
    public static MotionResultManager Instance { get; private set; }

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

    public void ShowResult(int score, bool passed, int mistakes, MotionAttemptRecord attempt)
    {
        string perf = MotionProfileManager.Instance != null ? MotionProfileManager.Instance.GetPerformanceLabel(score) : "";
        int best = MotionProfileManager.Instance != null && MotionProfileManager.Instance.ProfileData != null
            ? MotionProfileManager.Instance.ProfileData.bestScore
            : score;
        int attempts = attempt != null ? attempt.attemptNumber : 1;

        if (finalScoreText != null)
        {
            finalScoreText.text =
                "PRACTICAL COMPLETED\n\n" +
                "MOTION\n" +
                "Investigating Distance, Displacement, Speed, Velocity and Acceleration\n\n" +
                $"Score: {score} / 100\n" +
                $"Percentage: {score}%\n" +
                $"Performance: {perf}\n" +
                $"Best Score: {best} / 100\n" +
                $"Attempts: {attempts}\n" +
                $"Mistakes: {mistakes}";
        }

        if (resultDetailsText != null)
        {
            var data = MotionDataManager.Instance;
            string details =
                "EXPERIMENT RESULTS\n\n" +
                $"Distance (last path): {(data != null ? data.PathDistance : 0f):0.00} m\n" +
                $"Displacement (last path): {(data != null ? data.PathDisplacement : 0f):+0.00;-0.00} m\n" +
                $"Average Speed: {(data != null ? data.AverageSpeed() : 0f):0.00} m/s\n" +
                $"Average Velocity: {(data != null ? data.AverageVelocity() : 0f):0.00} m/s\n" +
                $"Acceleration: {(data != null ? data.LatestAcceleration() : 0f):0.00} m/s²\n" +
                $"Deceleration: {(data != null ? data.DecelerationA : 0f):0.00} m/s²\n" +
                $"Trials Completed: {(data != null ? data.CompletedTrialCount() : 0)}/5\n\n" +
                "CONCLUSION\n" +
                "Distance is the total path travelled by an object.\n" +
                "Displacement depends only on the initial and final positions and has a direction.\n" +
                "Speed is the rate of change of distance.\n" +
                "Velocity is the rate of change of displacement.\n" +
                "Acceleration is the rate of change of velocity.\n" +
                "Negative acceleration represents deceleration.";
            resultDetailsText.text = details;
        }

        if (statusText != null)
        {
            statusText.text = passed ? "STATUS: COMPLETED" : "STATUS: NEEDS IMPROVEMENT";
            statusText.color = passed ? new Color(0.1f, 0.55f, 0.2f) : new Color(0.75f, 0.15f, 0.15f);
        }
    }
}
