using System;
using System.Collections.Generic;

[Serializable]
public class HeatAttemptRecord
{
    public int attemptNumber;
    public int score;
    public int maxScore = 100;
    public int mistakes;
    public string status;
    public string date;
    public int trialsCompleted;
    public bool selectedCorrectEquipment;
    public bool apparatusAssembled;
    public bool markedLevelA;
    public bool observedDropToB;
    public bool observedRiseToC;
    public bool identifiedLevels;
}

[Serializable]
public class HeatExperimentSaveData
{
    public string studentId = "Student001";
    public string studentName = "Student";
    public string practicalName = "Illustrating expansion of liquids";
    public string topic = "Heat";
    public string activity = "Illustrating expansion of liquids";
    public int lastScore;
    public int bestScore;
    public int attemptCount;
    public bool completionStatus;
    public int mistakes;
    public int trialsCompleted;
    public string lastCompletedDate;
    public List<HeatAttemptRecord> attemptHistory = new List<HeatAttemptRecord>();
}
