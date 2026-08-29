using System;
using System.Collections.Generic;

[Serializable]
public class FrictionTrialData
{
    public int trialNumber;
    public string surfaceName;
    public float length;
    public float width;
    public float contactArea;
    public float weight = 60f;
    public float limitingFriction;
    public float sandpaperRoughness = 1f;
    public bool completed;
}

[Serializable]
public class FrictionForceTimeSample
{
    public float time;
    public float appliedForce;
    public float frictionForce;
    public bool moving;
}

[Serializable]
public class FrictionAttemptRecord
{
    public int attemptNumber;
    public int score;
    public int maxScore = 100;
    public int mistakes;
    public string status;
    public string date;
    public int trialsCompleted;
    public List<FrictionTrialData> trials = new List<FrictionTrialData>();
}

[Serializable]
public class FrictionExperimentSaveData
{
    public string studentId = "Student001";
    public string studentName = "Student";
    public string practicalName = "Investigation of the Influence of Surface Area on Friction";
    public string topic = "Friction";
    public string activity = "Investigation of the Influence of Surface Area on Friction";
    public int lastScore;
    public int bestScore;
    public int attemptCount;
    public bool completionStatus;
    public int mistakes;
    public int trialsCompleted;
    public string lastCompletedDate;
    public List<FrictionAttemptRecord> attemptHistory = new List<FrictionAttemptRecord>();
}
