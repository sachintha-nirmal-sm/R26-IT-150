using UnityEngine;

public class OpticsProfileManager : MonoBehaviour
{
    public static OpticsProfileManager Instance { get; private set; }

    [SerializeField] private string studentId = "Student001";
    [SerializeField] private string studentName = "Student";
    [SerializeField] private OpticsExperimentSaveData profileData;

    public string StudentName => studentName;
    public OpticsExperimentSaveData ProfileData => profileData;

    private const string Topic = "Geometrical Optics";
    private const string Practical = "Finding the approximate focal length of a concave mirror using a distant object";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        profileData = OpticsSaveManager.Instance != null ? OpticsSaveManager.Instance.Load() : new OpticsExperimentSaveData();
        profileData.studentId = studentId;
        profileData.studentName = studentName;
        profileData.topic = Topic;
        profileData.practicalName = Practical;
        profileData.activity = Practical;
    }

    public void UpdatePracticalResult(int score, int mistakes, bool passed, OpticsAttemptRecord attempt)
    {
        if (profileData == null) profileData = new OpticsExperimentSaveData();
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
            profileData.attemptHistory = new System.Collections.Generic.List<OpticsAttemptRecord>();
        if (attempt != null) profileData.attemptHistory.Add(attempt);
        OpticsSaveManager.Instance?.Save(profileData);
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
        if (profileData == null) profileData = new OpticsExperimentSaveData();
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
