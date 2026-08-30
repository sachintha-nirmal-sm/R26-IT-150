using UnityEngine;

public class PowerEnergyProfileManager : MonoBehaviour
{
    public static PowerEnergyProfileManager Instance { get; private set; }

    [SerializeField] private string studentId = "Student001";
    [SerializeField] private string studentName = "Student";
    [SerializeField] private PowerEnergyExperimentSaveData profileData;

    public string StudentName => studentName;
    public PowerEnergyExperimentSaveData ProfileData => profileData;

    private const string Topic = "Power and Energy of Electric Appliances";
    private const string Practical = "Investigation of the Power and Electrical Energy Consumed by Electric Appliances";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        profileData = PowerEnergySaveManager.Instance != null ? PowerEnergySaveManager.Instance.Load() : new PowerEnergyExperimentSaveData();
        profileData.studentId = studentId;
        profileData.studentName = studentName;
        profileData.topic = Topic;
        profileData.practicalName = Practical;
        profileData.activity = Practical;
    }

    public void UpdatePracticalResult(int score, int mistakes, bool passed, PowerEnergyAttemptRecord attempt)
    {
        if (profileData == null) profileData = new PowerEnergyExperimentSaveData();
        profileData.practicalName = Practical;
        profileData.topic = Topic;
        profileData.activity = Practical;
        profileData.lastScore = score;
        profileData.bestScore = Mathf.Max(profileData.bestScore, score);
        profileData.attemptCount = attempt != null ? attempt.attemptNumber : profileData.attemptCount + 1;
        profileData.completionStatus = passed;
        profileData.mistakes = mistakes;
        profileData.appliancesCompleted = attempt != null ? attempt.appliancesCompleted : profileData.appliancesCompleted;
        profileData.trialsCompleted = profileData.appliancesCompleted;
        profileData.lastCompletedDate = attempt != null ? attempt.date : System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        if (profileData.attemptHistory == null)
            profileData.attemptHistory = new System.Collections.Generic.List<PowerEnergyAttemptRecord>();
        if (attempt != null) profileData.attemptHistory.Add(attempt);
        var results = PowerEnergyApplianceController.Instance != null ? PowerEnergyApplianceController.Instance.Results : null;
        if (results != null)
            profileData.applianceResults = new System.Collections.Generic.List<PowerEnergyApplianceData>(results);
        PowerEnergySaveManager.Instance?.Save(profileData);
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
        if (profileData == null) profileData = new PowerEnergyExperimentSaveData();
        return
            "STUDENT PROFILE\n\n" +
            $"Student: {profileData.studentName}\n" +
            $"Topic: {profileData.topic}\n" +
            $"Practical: {profileData.practicalName}\n" +
            $"Score: {profileData.lastScore}/100\n" +
            $"Percentage: {profileData.lastScore}%\n" +
            $"Best Score: {profileData.bestScore}/100\n" +
            $"Attempts: {profileData.attemptCount}\n" +
            $"Mistakes: {profileData.mistakes}\n" +
            $"Appliances Completed: {profileData.appliancesCompleted}/4\n" +
            $"Status: {(profileData.completionStatus ? "Completed" : "In Progress")}\n" +
            $"Date: {profileData.lastCompletedDate}";
    }
}
