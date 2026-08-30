using UnityEngine;

public class ElecProfileManager : MonoBehaviour
{
    public static ElecProfileManager Instance { get; private set; }

    [SerializeField] private string studentId = "Student001";
    [SerializeField] private string studentName = "Student";
    [SerializeField] private ElecExperimentSaveData profileData;

    public string StudentName => studentName;
    public ElecExperimentSaveData ProfileData => profileData;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        profileData = ElecSaveManager.Instance != null ? ElecSaveManager.Instance.Load() : new ElecExperimentSaveData();
        profileData.studentId = studentId;
        profileData.studentName = studentName;
    }

    public void UpdatePracticalResult(int score, int mistakes, bool passed, ElecAttemptRecord attempt)
    {
        if (profileData == null) profileData = new ElecExperimentSaveData();
        profileData.practicalName = "Investigating Different Connections of Two Dry Cells";
        profileData.topic = "Current Electricity";
        profileData.activity = "Investigating Different Connections of Two Dry Cells";
        profileData.lastScore = score;
        profileData.bestScore = Mathf.Max(profileData.bestScore, score);
        profileData.attemptCount = attempt != null ? attempt.attemptNumber : profileData.attemptCount + 1;
        profileData.completionStatus = passed;
        profileData.mistakes = mistakes;
        profileData.connectionsCompleted = attempt != null ? attempt.connectionsCompleted : 3;
        profileData.lastCompletedDate = attempt != null ? attempt.date : System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        if (profileData.attemptHistory == null)
            profileData.attemptHistory = new System.Collections.Generic.List<ElecAttemptRecord>();
        if (attempt != null) profileData.attemptHistory.Add(attempt);
        ElecSaveManager.Instance?.Save(profileData);
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
        if (profileData == null) profileData = new ElecExperimentSaveData();
        return
            "STUDENT PROFILE\n\n" +
            $"Student: {profileData.studentName}\n" +
            $"Topic: {profileData.topic}\n" +
            $"Practical: {profileData.practicalName}\n" +
            $"Score: {profileData.lastScore}/100\n" +
            $"Percentage: {profileData.lastScore}%\n" +
            $"Attempt: {profileData.attemptCount}\n" +
            $"Best Score: {profileData.bestScore}/100\n" +
            $"Mistakes: {profileData.mistakes}\n" +
            $"Connections Completed: {profileData.connectionsCompleted}/3\n" +
            $"Status: {(profileData.completionStatus ? "Completed" : "In Progress")}\n" +
            $"Date: {profileData.lastCompletedDate}";
    }
}
