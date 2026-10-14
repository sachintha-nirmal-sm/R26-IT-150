using System;
using System.Collections.Generic;

[Serializable]
public class TurningTrialData
{
    public int trialNumber;
    public int tightnessLevel;
    public string point = "D";
    public float distanceCm = 60f;
    public float angleDeg = 90f;
    public float forceN;
    public float momentNm;
    public float targetForceN;
    public bool completed;
    public bool stickMoved;
}

[Serializable]
public class TurningAttemptRecord
{
    public int attemptNumber;
    public int score;
    public int maxScore = 100;
    public int mistakes;
    public string status;
    public string date;
    public int trialsCompleted;
    public List<TurningTrialData> trials = new List<TurningTrialData>();
}

[Serializable]
public class TurningExperimentSaveData
{
    public string studentId = "Student001";
    public string studentName = "Student";
    public string practicalName = "Activity 2 — Investigating the turning effect of a force";
    public string topic = "Turning effect of a force";
    public string activity = "Activity 2 — Investigating the turning effect of a force";
    public int lastScore;
    public int bestScore;
    public int attemptCount;
    public bool completionStatus;
    public int mistakes;
    public int trialsCompleted;
    public string lastCompletedDate;
    public List<TurningAttemptRecord> attemptHistory = new List<TurningAttemptRecord>();
}
