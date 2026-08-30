using System;
using System.Collections.Generic;

[Serializable]
public class PowerEnergyAttemptRecord
{
    public int attemptNumber;
    public int score;
    public int maxScore = 100;
    public int mistakes;
    public string status;
    public string date;
    public int appliancesCompleted;
    public int powerCalculations;
    public int energyCalculations;
    public int kwhConversions;
    public bool selectedCorrectEquipment;
    public bool circuitConnected;
    public bool conclusionCompleted;
    public string applianceSummary;
}

[Serializable]
public class PowerEnergyExperimentSaveData
{
    public string studentId = "Student001";
    public string studentName = "Student";
    public string practicalName = "Investigation of the Power and Electrical Energy Consumed by Electric Appliances";
    public string topic = "Power and Energy of Electric Appliances";
    public string activity = "Investigation of the Power and Electrical Energy Consumed by Electric Appliances";
    public int lastScore;
    public int bestScore;
    public int attemptCount;
    public bool completionStatus;
    public int mistakes;
    public int appliancesCompleted;
    public int trialsCompleted;
    public string lastCompletedDate;
    public List<PowerEnergyAttemptRecord> attemptHistory = new List<PowerEnergyAttemptRecord>();
    public List<PowerEnergyApplianceData> applianceResults = new List<PowerEnergyApplianceData>();
}
