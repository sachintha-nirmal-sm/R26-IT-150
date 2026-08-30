using System;
using System.Collections.Generic;

[Serializable]
public class NewtonLawTrialData
{
    public int trialNumber;
    public float force;
    public float mass;
    public float acceleration;
    public float velocity;
    public float time;
    public float weight;
    public string observation;
    public string series;
}

[Serializable]
public class NewtonAttemptRecord
{
    public int attemptNumber;
    public int score;
    public int maxScore = 100;
    public int mistakes;
    public string status;
    public string date;
    public int trialsCompleted;
    public int activitiesCompleted;
    public List<NewtonLawTrialData> trials = new List<NewtonLawTrialData>();
}

[Serializable]
public class NewtonExperimentSaveData
{
    public string studentId = "Student001";
    public string studentName = "Student";
    public string practicalName = "Investigating Newton's Laws of Motion";
    public string topic = "Newton's Laws of Motion";
    public string activity = "Investigating Newton's First, Second and Third Laws";
    public int lastScore;
    public int bestScore;
    public int attemptCount;
    public bool completionStatus;
    public int mistakes;
    public int trialsCompleted;
    public int activitiesCompleted;
    public string lastCompletedDate;
    public List<NewtonAttemptRecord> attemptHistory = new List<NewtonAttemptRecord>();
}
