using UnityEngine;

public class TurningProfileManager : MonoBehaviour
{
    public static TurningProfileManager Instance { get; private set; }

    [SerializeField] private string studentId = "Student001";
    [SerializeField] private string studentName = "Student";
    [SerializeField] private TurningExperimentSaveData profileData;

    public string StudentName => studentName;
    public TurningExperimentSaveData ProfileData => profileData;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        profileData = TurningSaveManager.Instance != null ? TurningSaveManager.Instance.Load() : new TurningExperimentSaveData();
        profileData.studentId = studentId;
        profileData.studentName = studentName;
        profileData.topic = "Turning effect of a force";
        profileData.practicalName = "Activity 2 — Investigating the turning effect of a force";
        profileData.activity = profileData.practicalName;
    }

    public void UpdatePracticalResult(int score, int mistakes, bool passed, TurningAttemptRecord attempt)
    {
        if (profileData == null) profileData = new TurningExperimentSaveData();
        profileData.practicalName = "Activity 2 — Investigating the turning effect of a force";
        profileData.topic = "Turning effect of a force";
        profileData.activity = profileData.practicalName;
        profileData.lastScore = score;
        profileData.bestScore = Mathf.Max(profileData.bestScore, score);
        profileData.attemptCount = attempt != null ? attempt.attemptNumber : profileData.attemptCount + 1;
        profileData.completionStatus = passed;
        profileData.mistakes = mistakes;
        profileData.trialsCompleted = attempt != null ? attempt.trialsCompleted : 3;
        profileData.lastCompletedDate = attempt != null ? attempt.date : System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        if (profileData.attemptHistory == null)
            profileData.attemptHistory = new System.Collections.Generic.List<TurningAttemptRecord>();
        if (attempt != null) profileData.attemptHistory.Add(attempt);
        TurningSaveManager.Instance?.Save(profileData);
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
        if (profileData == null) profileData = new TurningExperimentSaveData();
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
            $"Trials Completed: {profileData.trialsCompleted}/3\n" +
            $"Status: {(profileData.completionStatus ? "Completed" : "In Progress")}\n" +
            $"Date: {profileData.lastCompletedDate}";
    }
}
