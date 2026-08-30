using UnityEngine;

public class LeverProfileManager : MonoBehaviour
{
    public static LeverProfileManager Instance { get; private set; }

    [SerializeField] private string studentId = "Student001";
    [SerializeField] private string studentName = "Student";
    [SerializeField] private LeverExperimentSaveData profileData;

    public string StudentName => studentName;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        profileData = LeverSaveManager.Instance != null ? LeverSaveManager.Instance.Load() : new LeverExperimentSaveData();
        profileData.studentId = studentId;
        profileData.studentName = studentName;
    }

    public void UpdatePracticalResult(int score, int mistakes, bool passed, LeverAttemptRecord attempt)
    {
        profileData.practicalName = "Lever – Activity 15.1";
        profileData.lastScore = score;
        profileData.bestScore = Mathf.Max(profileData.bestScore, score);
        profileData.attemptCount = attempt.attemptNumber;
        profileData.completionStatus = passed;
        profileData.mistakes = mistakes;
        profileData.lastCompletedDate = attempt.date;
        if (profileData.attemptHistory == null) profileData.attemptHistory = new System.Collections.Generic.List<LeverAttemptRecord>();
        profileData.attemptHistory.Add(attempt);
        LeverSaveManager.Instance?.Save(profileData);
    }

    public string GetPerformanceLabel(int score)
    {
        if (score >= 90) return "Excellent";
        if (score >= 75) return "Very Good";
        if (score >= 50) return "Good";
        return "Needs Improvement";
    }

    public string GetProfileSummary()
    {
        if (profileData == null) profileData = new LeverExperimentSaveData();
        return
            $"PROFILE\n\n" +
            $"Student: {profileData.studentName}\n" +
            $"Practical: {profileData.practicalName}\n" +
            $"Last Score: {profileData.lastScore}/100\n" +
            $"Best Score: {profileData.bestScore}/100\n" +
            $"Attempts: {profileData.attemptCount}\n" +
            $"Mistakes: {profileData.mistakes}\n" +
            $"Status: {(profileData.completionStatus ? "Completed" : "In Progress")}\n" +
            $"Date: {profileData.lastCompletedDate}";
    }
}
