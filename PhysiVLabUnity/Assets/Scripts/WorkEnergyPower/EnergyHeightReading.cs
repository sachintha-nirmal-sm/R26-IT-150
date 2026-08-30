using System;
using System.Collections.Generic;

[Serializable]
public class EnergyHeightReading
{
    public int instance;
    public float mass;
    public float height;
    public float potentialEnergy;
    public float depressionDepth;
}

[Serializable]
public class WorkEnergyAttemptRecord
{
    public int attemptNumber;
    public int score;
    public int maxScore = 100;
    public int mistakes;
    public string status;
    public string date;
    public List<EnergyHeightReading> readings = new List<EnergyHeightReading>();
}

[Serializable]
public class WorkEnergyExperimentSaveData
{
    public string studentId = "Student001";
    public string studentName = "Student";
    public string practicalName = "Potential Energy and Height";
    public string topic = "Work, Energy and Power";
    public string activity = "Variation of Potential Energy with Height";
    public int lastScore;
    public int bestScore;
    public int attemptCount;
    public bool completionStatus;
    public int mistakes;
    public string lastCompletedDate;
    public List<WorkEnergyAttemptRecord> attemptHistory = new List<WorkEnergyAttemptRecord>();
}
