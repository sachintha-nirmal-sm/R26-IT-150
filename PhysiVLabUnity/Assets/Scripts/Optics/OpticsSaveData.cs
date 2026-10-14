using System;
using System.Collections.Generic;

[Serializable]
public class OpticsAttemptRecord
{
    public int attemptNumber;
    public int score;
    public int maxScore = 100;
    public int mistakes;
    public string status;
    public string date;
    public int trialsCompleted;
    public bool selectedCorrectEquipment;
    public bool windowOpened;
    public bool mirrorFacingWindow;
    public bool foundSharpImage;
    public bool measuredFocalLength;
}

[Serializable]
public class OpticsExperimentSaveData
{
    public string studentId = "Student001";
    public string studentName = "Student";
    public string practicalName = "Finding the approximate focal length of a concave mirror using a distant object";
    public string topic = "Geometrical Optics";
    public string activity = "Finding the approximate focal length of a concave mirror using a distant object";
    public int lastScore;
    public int bestScore;
    public int attemptCount;
    public bool completionStatus;
    public int mistakes;
    public int trialsCompleted;
    public string lastCompletedDate;
    public List<OpticsAttemptRecord> attemptHistory = new List<OpticsAttemptRecord>();
}
