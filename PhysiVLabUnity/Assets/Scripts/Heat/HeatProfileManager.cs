using UnityEngine;

public class HeatProfileManager : MonoBehaviour
{
    public static HeatProfileManager Instance { get; private set; }

    [SerializeField] private string studentId = "Student001";
    [SerializeField] private string studentName = "Student";
    [SerializeField] private HeatExperimentSaveData profileData;

    public string StudentName => studentName;
    public HeatExperimentSaveData ProfileData => profileData;

    private const string Topic = "Heat";
    private const string Practical = "Illustrating expansion of liquids";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        profileData = HeatSaveManager.Instance != null ? HeatSaveManager.Instance.Load() : new HeatExperimentSaveData();
        profileData.studentId = studentId;
        profileData.studentName = studentName;
        profileData.topic = Topic;
        profileData.practicalName = Practical;
        profileData.activity = Practical;
    }

    public void UpdatePracticalResult(int score, int mistakes, bool passed, HeatAttemptRecord attempt)
    {
        if (profileData == null) profileData = new HeatExperimentSaveData();
        profileData.practicalName = Practical;
        profileData.topic = Topic;
        profileData.activity = Practical;
        profileData.lastScore = score;
        profileData.bestScore = Mathf.Max(profileData.bestScore, score);
        profileData.attemptCount = attempt != null ? attempt.attemptNumber : profileData.attemptCount + 1;
        profileData.completionStatus = passed;
        profileData.mistakes = mistakes;
        profileData.trialsCompleted = attempt != null ? attempt.trialsCompleted : 1;
        profileData.lastCompletedDate = attempt != null ? attempt.date : System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        if (profileData.attemptHistory == null)
            profileData.attemptHistory = new System.Collections.Generic.List<HeatAttemptRecord>();
        if (attempt != null) profileData.attemptHistory.Add(attempt);
        HeatSaveManager.Instance?.Save(profileData);
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
        if (profileData == null) profileData = new HeatExperimentSaveData();
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
            $"Status: {(profileData.completionStatus ? "Completed" : "In Progress")}\n" +
            $"Date: {profileData.lastCompletedDate}";
    }
}
