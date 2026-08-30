using UnityEngine;

public class ResultantProfileManager : MonoBehaviour
{
    public static ResultantProfileManager Instance { get; private set; }

    [SerializeField] private string studentId = "Student001";
    [SerializeField] private string studentName = "Student";
    [SerializeField] private ResultantExperimentSaveData profileData;

    public string StudentName => studentName;
    public ResultantExperimentSaveData ProfileData => profileData;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        profileData = ResultantSaveManager.Instance != null ? ResultantSaveManager.Instance.Load() : new ResultantExperimentSaveData();
        profileData.studentId = studentId;
        profileData.studentName = studentName;
        profileData.topic = "Resultant Force";
        profileData.practicalName = "Two Forces Acting in the Same Direction";
        profileData.activity = profileData.practicalName;
    }

    public void UpdatePracticalResult(int score, int mistakes, bool passed, ResultantAttemptRecord attempt)
    {
        if (profileData == null) profileData = new ResultantExperimentSaveData();
        profileData.practicalName = "Two Forces Acting in the Same Direction";
        profileData.topic = "Resultant Force";
        profileData.activity = profileData.practicalName;
        profileData.lastScore = score;
        profileData.bestScore = Mathf.Max(profileData.bestScore, score);
        profileData.attemptCount = attempt != null ? attempt.attemptNumber : profileData.attemptCount + 1;
        profileData.completionStatus = passed;
        profileData.mistakes = mistakes;
        profileData.trialsCompleted = attempt != null ? attempt.trialsCompleted : 3;
        profileData.lastCompletedDate = attempt != null ? attempt.date : System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        if (profileData.attemptHistory == null)
            profileData.attemptHistory = new System.Collections.Generic.List<ResultantAttemptRecord>();
        if (attempt != null) profileData.attemptHistory.Add(attempt);
        ResultantSaveManager.Instance?.Save(profileData);
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
        if (profileData == null) profileData = new ResultantExperimentSaveData();
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
