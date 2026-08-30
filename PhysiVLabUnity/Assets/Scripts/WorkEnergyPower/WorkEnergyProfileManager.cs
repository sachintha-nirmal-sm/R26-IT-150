using UnityEngine;

public class WorkEnergyProfileManager : MonoBehaviour
{
    public static WorkEnergyProfileManager Instance { get; private set; }

    [SerializeField] private string studentId = "Student001";
    [SerializeField] private string studentName = "Student";
    [SerializeField] private WorkEnergyExperimentSaveData profileData;

    public string StudentName => studentName;
    public WorkEnergyExperimentSaveData ProfileData => profileData;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        profileData = WorkEnergySaveManager.Instance != null ? WorkEnergySaveManager.Instance.Load() : new WorkEnergyExperimentSaveData();
        profileData.studentId = studentId;
        profileData.studentName = studentName;
    }

    public void UpdatePracticalResult(int score, int mistakes, bool passed, WorkEnergyAttemptRecord attempt)
    {
        if (profileData == null) profileData = new WorkEnergyExperimentSaveData();
        profileData.practicalName = "Potential Energy and Height";
        profileData.topic = "Work, Energy and Power";
        profileData.activity = "Variation of Potential Energy with Height";
        profileData.lastScore = score;
        profileData.bestScore = Mathf.Max(profileData.bestScore, score);
        profileData.attemptCount = attempt != null ? attempt.attemptNumber : profileData.attemptCount + 1;
        profileData.completionStatus = passed;
        profileData.mistakes = mistakes;
        profileData.lastCompletedDate = attempt != null ? attempt.date : System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        if (profileData.attemptHistory == null)
            profileData.attemptHistory = new System.Collections.Generic.List<WorkEnergyAttemptRecord>();
        if (attempt != null) profileData.attemptHistory.Add(attempt);
        WorkEnergySaveManager.Instance?.Save(profileData);
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
        if (profileData == null) profileData = new WorkEnergyExperimentSaveData();
        return
            "STUDENT PROFILE\n\n" +
            $"Student: {profileData.studentName}\n" +
            $"Practical: {profileData.practicalName}\n" +
            $"Topic: {profileData.topic}\n" +
            $"Activity: {profileData.activity}\n" +
            $"Last Score: {profileData.lastScore}/100\n" +
            $"Percentage: {profileData.lastScore}%\n" +
            $"Best Score: {profileData.bestScore}/100\n" +
            $"Attempt: {profileData.attemptCount}\n" +
            $"Mistakes: {profileData.mistakes}\n" +
            $"Status: {(profileData.completionStatus ? "Completed" : "In Progress")}\n" +
            $"Date: {profileData.lastCompletedDate}";
    }
}
