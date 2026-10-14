using UnityEngine;

public class NewtonProfileManager : MonoBehaviour
{
    public static NewtonProfileManager Instance { get; private set; }

    [SerializeField] private string studentId = "Student001";
    [SerializeField] private string studentName = "Student";
    [SerializeField] private NewtonExperimentSaveData profileData;

    public string StudentName => studentName;
    public NewtonExperimentSaveData ProfileData => profileData;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        profileData = NewtonSaveManager.Instance != null ? NewtonSaveManager.Instance.Load() : new NewtonExperimentSaveData();
        profileData.studentId = studentId;
        profileData.studentName = studentName;
    }

    public void UpdatePracticalResult(int score, int mistakes, bool passed, NewtonAttemptRecord attempt)
    {
        if (profileData == null) profileData = new NewtonExperimentSaveData();
        profileData.practicalName = "Investigating Newton's Laws of Motion";
        profileData.topic = "Newton's Laws of Motion";
        profileData.activity = "Investigating Newton's First, Second and Third Laws";
        profileData.lastScore = score;
        profileData.bestScore = Mathf.Max(profileData.bestScore, score);
        profileData.attemptCount = attempt != null ? attempt.attemptNumber : profileData.attemptCount + 1;
        profileData.completionStatus = passed;
        profileData.mistakes = mistakes;
        profileData.trialsCompleted = attempt != null ? attempt.trialsCompleted : 0;
        profileData.activitiesCompleted = NewtonDataManager.Instance != null ? NewtonDataManager.Instance.ActivitiesCompleted() : 4;
        profileData.lastCompletedDate = attempt != null ? attempt.date : System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        if (profileData.attemptHistory == null)
            profileData.attemptHistory = new System.Collections.Generic.List<NewtonAttemptRecord>();
        if (attempt != null) profileData.attemptHistory.Add(attempt);
        NewtonSaveManager.Instance?.Save(profileData);
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
        if (profileData == null) profileData = new NewtonExperimentSaveData();
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
            $"Activities Completed: {profileData.activitiesCompleted}/4\n" +
            $"Status: {(profileData.completionStatus ? "Completed" : "In Progress")}\n" +
            $"Date: {profileData.lastCompletedDate}";
    }
}
