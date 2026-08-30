using System;
using System.Collections.Generic;

[Serializable]
public class LeverReading
{
    public int instance;
    public float distanceA;
    public float distanceX;
    public float bookWeight;
    public float requiredEffort;
    public float measuredEffort;
    public bool bookLifted;
}

[Serializable]
public class LeverAttemptRecord
{
    public int attemptNumber;
    public int score;
    public int maxScore = 100;
    public int mistakes;
    public string status;
    public string date;
    public List<LeverReading> readings = new List<LeverReading>();
}

[Serializable]
public class LeverExperimentSaveData
{
    public string studentId = "Student001";
    public string studentName = "Student";
    public string practicalName = "Lever – Activity 15.1";
    public int lastScore;
    public int bestScore;
    public int attemptCount;
    public bool completionStatus;
    public int mistakes;
    public string lastCompletedDate;
    public List<LeverAttemptRecord> attemptHistory = new List<LeverAttemptRecord>();
}
