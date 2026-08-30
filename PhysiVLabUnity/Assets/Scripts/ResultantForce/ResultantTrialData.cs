using System;
using System.Collections.Generic;

[Serializable]
public class ResultantTrialData
{
    public int trialNumber;
    public float forceB;
    public float forceC;
    public float forceA;
    public float targetB;
    public float targetC;
    public bool completed;
}

[Serializable]
public class ResultantAttemptRecord
{
    public int attemptNumber;
    public int score;
    public int maxScore = 100;
    public int mistakes;
    public string status;
    public string date;
    public int trialsCompleted;
    public List<ResultantTrialData> trials = new List<ResultantTrialData>();
}

[Serializable]
public class ResultantExperimentSaveData
{
    public string studentId = "Student001";
    public string studentName = "Student";
    public string practicalName = "Two Forces Acting in the Same Direction";
    public string topic = "Resultant Force";
    public string activity = "Two Forces Acting in the Same Direction";
    public int lastScore;
    public int bestScore;
    public int attemptCount;
    public bool completionStatus;
    public int mistakes;
    public int trialsCompleted;
    public string lastCompletedDate;
    public List<ResultantAttemptRecord> attemptHistory = new List<ResultantAttemptRecord>();
}
