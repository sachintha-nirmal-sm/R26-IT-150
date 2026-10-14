using System;
using System.Collections.Generic;

[Serializable]
public class EquilibriumTrialData
{
    public int trialNumber;
    public float weightN;
    public float force1N;
    public float force2N;
    public float sumN;
    public float tiltDeg;
    public bool horizontal;
    public bool completed;
}

[Serializable]
public class EquilibriumAttemptRecord
{
    public int attemptNumber;
    public int score;
    public int maxScore = 100;
    public int mistakes;
    public string status;
    public string date;
    public int trialsCompleted;
    public List<EquilibriumTrialData> trials = new List<EquilibriumTrialData>();
}

[Serializable]
public class EquilibriumExperimentSaveData
{
    public string studentId = "Student001";
    public string studentName = "Student";
    public string practicalName = "Activity — Equilibrium of a meter ruler under three coplanar parallel forces";
    public string topic = "Equilibrium of Forces";
    public string activity = "Activity — Equilibrium of a meter ruler under three coplanar parallel forces";
    public int lastScore;
    public int bestScore;
    public int attemptCount;
    public bool completionStatus;
    public int mistakes;
    public int trialsCompleted;
    public string lastCompletedDate;
    public List<EquilibriumAttemptRecord> attemptHistory = new List<EquilibriumAttemptRecord>();
}
