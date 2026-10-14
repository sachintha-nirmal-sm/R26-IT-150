using System;
using System.Collections.Generic;

[Serializable]
public class WavesAttemptRecord
{
    public int attemptNumber;
    public int score;
    public int maxScore = 100;
    public int mistakes;
    public string status;
    public string date;
    public int trialsCompleted;
    public bool selectedCorrectEquipment;
    public bool tiedRibbons;
    public bool shookTransverse;
    public bool identifiedPerpendicularMotion;
}

[Serializable]
public class WavesExperimentSaveData
{
    public string studentId = "Student001";
    public string studentName = "Student";
    public string practicalName = "4.5 — Demonstration of the formation of transverse waves using a slinky";
    public string topic = "Waves and their applications";
    public string activity = "4.5 — Demonstration of the formation of transverse waves using a slinky";
    public int lastScore;
    public int bestScore;
    public int attemptCount;
    public bool completionStatus;
    public int mistakes;
    public int trialsCompleted;
    public string lastCompletedDate;
    public List<WavesAttemptRecord> attemptHistory = new List<WavesAttemptRecord>();
}
