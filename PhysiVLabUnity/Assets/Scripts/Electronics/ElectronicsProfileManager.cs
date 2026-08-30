using UnityEngine;

public class ElectronicsProfileManager : MonoBehaviour
{
    public static ElectronicsProfileManager Instance { get; private set; }

    [SerializeField] private string studentId = "Student001";
    [SerializeField] private string studentName = "Student";
    [SerializeField] private ElectronicsExperimentSaveData profileData;

    public string StudentName => studentName;
    public ElectronicsExperimentSaveData ProfileData => profileData;

    private const string Topic = "Electronics";
    private const string Practical = "Investigation of Forward Bias and Reverse Bias of a Diode";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        profileData = ElectronicsSaveManager.Instance != null ? ElectronicsSaveManager.Instance.Load() : new ElectronicsExperimentSaveData();
        profileData.studentId = studentId;
        profileData.studentName = studentName;
        profileData.topic = Topic;
        profileData.practicalName = Practical;
        profileData.activity = Practical;
    }

    public void UpdatePracticalResult(int score, int mistakes, bool passed, ElectronicsAttemptRecord attempt)
    {
        if (profileData == null) profileData = new ElectronicsExperimentSaveData();
        profileData.practicalName = Practical;
        profileData.topic = Topic;
        profileData.activity = Practical;
        profileData.lastScore = score;
        profileData.bestScore = Mathf.Max(profileData.bestScore, score);
        profileData.attemptCount = attempt != null ? attempt.attemptNumber : profileData.attemptCount + 1;
        profileData.completionStatus = passed;
        profileData.mistakes = mistakes;
        profileData.lastCompletedDate = attempt != null ? attempt.date : System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        profileData.forwardBiasCompleted = attempt != null && attempt.forwardBiasCompleted;
        profileData.reverseBiasCompleted = attempt != null && attempt.reverseBiasCompleted;
        profileData.observationCompleted = attempt != null && attempt.observationCompleted;
        profileData.questionsCompleted = attempt != null && attempt.questionsCompleted;
        profileData.conclusionCompleted = attempt != null && attempt.conclusionCompleted;

        var obs = ElectronicsObservationManager.Instance;
        if (obs != null)
        {
            profileData.forwardObservation = obs.Forward;
            profileData.reverseObservation = obs.Reverse;
        }

        if (profileData.attemptHistory == null)
            profileData.attemptHistory = new System.Collections.Generic.List<ElectronicsAttemptRecord>();
        if (attempt != null) profileData.attemptHistory.Add(attempt);
        ElectronicsSaveManager.Instance?.Save(profileData);
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
        if (profileData == null) profileData = new ElectronicsExperimentSaveData();
        return
            "STUDENT PROFILE\n\n" +
            $"Student: {profileData.studentName}\n" +
            $"Topic: {profileData.topic}\n" +
            $"Practical: {profileData.practicalName}\n" +
            $"Score: {profileData.lastScore}/100\n" +
            $"Percentage: {profileData.lastScore}%\n" +
            $"Best Score: {profileData.bestScore}/100\n" +
            $"Attempts: {profileData.attemptCount}\n" +
            $"Forward Bias: {(profileData.forwardBiasCompleted ? "Completed" : "—")}\n" +
            $"Reverse Bias: {(profileData.reverseBiasCompleted ? "Completed" : "—")}\n" +
            $"Observation: {(profileData.observationCompleted ? "Completed" : "—")}\n" +
            $"Questions: {(profileData.questionsCompleted ? "Completed" : "—")}\n" +
            $"Conclusion: {(profileData.conclusionCompleted ? "Completed" : "—")}\n" +
            $"Status: {(profileData.completionStatus ? "Completed" : "In Progress")}\n" +
            $"Date: {profileData.lastCompletedDate}";
    }
}
