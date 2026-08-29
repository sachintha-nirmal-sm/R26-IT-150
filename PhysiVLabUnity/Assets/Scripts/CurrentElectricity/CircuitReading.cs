using System;
using System.Collections.Generic;

[Serializable]
public class CircuitReading
{
    public int connectionNumber;
    public string arrangement;
    public float voltage;
    public float current;
    public float power;
    public float brightness;
    public string brightnessLabel;
    public float recordedVoltage;
    public float recordedCurrent;
    public string recordedBrightness;
}

[Serializable]
public class ElecAttemptRecord
{
    public int attemptNumber;
    public int score;
    public int maxScore = 100;
    public int mistakes;
    public string status;
    public string date;
    public int connectionsCompleted;
    public List<CircuitReading> readings = new List<CircuitReading>();
}

[Serializable]
public class ElecExperimentSaveData
{
    public string studentId = "Student001";
    public string studentName = "Student";
    public string practicalName = "Investigating Different Connections of Two Dry Cells";
    public string topic = "Current Electricity";
    public string activity = "Investigating Different Connections of Two Dry Cells";
    public int lastScore;
    public int bestScore;
    public int attemptCount;
    public bool completionStatus;
    public int mistakes;
    public int connectionsCompleted;
    public string lastCompletedDate;
    public List<ElecAttemptRecord> attemptHistory = new List<ElecAttemptRecord>();
}
