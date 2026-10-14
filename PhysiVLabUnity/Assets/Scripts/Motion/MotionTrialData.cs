using System;
using System.Collections.Generic;

[Serializable]
public class MotionTrialData
{
    public int trialNumber;
    public float initialPosition;
    public float finalPosition;
    public float distance;
    public float displacement;
    public float time;
    public float speed;
    public float velocity;
    public float acceleration;
}

[Serializable]
public class AccelerationTrialData
{
    public int trialNumber;
    public float initialVelocity;
    public float finalVelocity;
    public float time;
    public float acceleration;
    public string condition;
}

[Serializable]
public class MotionGraphSample
{
    public float time;
    public float distance;
    public float velocity;
    public float acceleration;
}

[Serializable]
public class MotionAttemptRecord
{
    public int attemptNumber;
    public int score;
    public int maxScore = 100;
    public int mistakes;
    public string status;
    public string date;
    public int trialsCompleted;
    public List<MotionTrialData> trials = new List<MotionTrialData>();
}

[Serializable]
public class MotionExperimentSaveData
{
    public string studentId = "Student001";
    public string studentName = "Student";
    public string practicalName = "Investigating Distance, Displacement, Speed, Velocity and Acceleration";
    public string topic = "Motion";
    public string activity = "Investigating Distance, Displacement, Speed, Velocity and Acceleration";
    public int lastScore;
    public int bestScore;
    public int attemptCount;
    public bool completionStatus;
    public int mistakes;
    public int trialsCompleted;
    public string lastCompletedDate;
    public List<MotionAttemptRecord> attemptHistory = new List<MotionAttemptRecord>();
}
