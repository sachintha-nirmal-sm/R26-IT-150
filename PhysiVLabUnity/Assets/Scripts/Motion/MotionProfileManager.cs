using UnityEngine;

public class MotionProfileManager : MonoBehaviour
{
    public static MotionProfileManager Instance { get; private set; }

    [SerializeField] private string studentId = "Student001";
    [SerializeField] private string studentName = "Student";
    [SerializeField] private MotionExperimentSaveData profileData;

    public string StudentName => studentName;
    public MotionExperimentSaveData ProfileData => profileData;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        profileData = MotionSaveManager.Instance != null ? MotionSaveManager.Instance.Load() : new MotionExperimentSaveData();
        profileData.studentId = studentId;
        profileData.studentName = studentName;
    }

    public void UpdatePracticalResult(int score, int mistakes, bool passed, MotionAttemptRecord attempt)
    {
        if (profileData == null) profileData = new MotionExperimentSaveData();
        profileData.practicalName = "Investigating Distance, Displacement, Speed, Velocity and Acceleration";
        profileData.topic = "Motion";
        profileData.activity = "Investigating Distance, Displacement, Speed, Velocity and Acceleration";
        profileData.lastScore = score;
        profileData.bestScore = Mathf.Max(profileData.bestScore, score);
        profileData.attemptCount = attempt != null ? attempt.attemptNumber : profileData.attemptCount + 1;
        profileData.completionStatus = passed;
        profileData.mistakes = mistakes;
        profileData.trialsCompleted = attempt != null ? attempt.trialsCompleted : 5;
        profileData.lastCompletedDate = attempt != null ? attempt.date : System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        if (profileData.attemptHistory == null)
            profileData.attemptHistory = new System.Collections.Generic.List<MotionAttemptRecord>();
        if (attempt != null) profileData.attemptHistory.Add(attempt);
        MotionSaveManager.Instance?.Save(profileData);
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
        if (profileData == null) profileData = new MotionExperimentSaveData();
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
            $"Trials Completed: {profileData.trialsCompleted}/5\n" +
            $"Status: {(profileData.completionStatus ? "Completed" : "In Progress")}\n" +
            $"Date: {profileData.lastCompletedDate}";
    }
}
