using TMPro;
using UnityEngine;

public class NewtonResultManager : MonoBehaviour
{
    public static NewtonResultManager Instance { get; private set; }

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

    public void ShowResult(int score, bool passed, int mistakes, NewtonAttemptRecord attempt)
    {
        string perf = NewtonProfileManager.Instance != null ? NewtonProfileManager.Instance.GetPerformanceLabel(score) : "";
        int best = NewtonProfileManager.Instance != null && NewtonProfileManager.Instance.ProfileData != null
            ? NewtonProfileManager.Instance.ProfileData.bestScore
            : score;
        int attempts = attempt != null ? attempt.attemptNumber : 1;
        var data = NewtonDataManager.Instance;
        int activities = data != null ? data.ActivitiesCompleted() : 4;

        if (finalScoreText != null)
        {
            finalScoreText.text =
                "PRACTICAL COMPLETED\n\n" +
                "NEWTON'S LAWS OF MOTION\n" +
                "Investigating Newton's First, Second and Third Laws\n\n" +
                $"Score: {score} / 100\n" +
                $"Percentage: {score}%\n" +
                $"Performance: {perf}\n" +
                $"Best Score: {best} / 100\n" +
                $"Attempts: {attempts}\n" +
                $"Activities Completed: {activities}/4\n" +
                $"Mistakes: {mistakes}";
        }

        if (resultDetailsText != null)
        {
            resultDetailsText.text =
                "NEWTON'S LAWS RESULTS\n\n" +
                $"First Law: {(data != null && data.FirstLawComplete ? "Completed ✓" : "Incomplete")}\n" +
                $"Second Law: {(data != null && data.SecondLawComplete ? "Completed ✓" : "Incomplete")}\n" +
                $"Third Law: {(data != null && data.ThirdLawComplete ? "Completed ✓" : "Incomplete")}\n" +
                $"Weight: {(data != null && data.WeightComplete ? "Completed ✓" : "Incomplete")}\n\n" +
                $"Highest Force Tested: {(data != null ? data.HighestForce() : 0f):0.0} N\n" +
                $"Lowest Mass Tested: {(data != null ? data.LowestMass() : 0f):0.00} kg\n" +
                $"Highest Acceleration: {(data != null ? data.HighestAcceleration() : 0f):0.00} m/s²\n" +
                $"Weight Measurements: {(data != null ? data.WeightSeries.Count : 0)}\n" +
                $"Final Score: {score}/100\n\n" +
                "CONCLUSION\n" +
                "Newton's First Law explains that an object remains at rest or continues with uniform velocity when no unbalanced force acts on it.\n" +
                "Newton's Second Law shows that acceleration depends on the force and mass of an object.\n" +
                "Newton's Third Law states that forces occur in equal and opposite pairs.\n" +
                "The weight of an object is the gravitational force acting on it and is calculated using W = mg.";
        }

        if (statusText != null)
        {
            statusText.text = passed ? "STATUS: COMPLETED" : "STATUS: NEEDS IMPROVEMENT";
            statusText.color = passed ? new Color(0.1f, 0.55f, 0.2f) : new Color(0.75f, 0.15f, 0.15f);
        }
    }
}
