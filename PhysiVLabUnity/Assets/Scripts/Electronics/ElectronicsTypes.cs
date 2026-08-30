using System;
using System.Collections.Generic;

public enum ElectronicsPracticalStep
{
    Introduction,
    Theory,
    EquipmentSelection,
    CircuitSetup,
    ForwardBias,
    ForwardObservation,
    BatteryDisconnect,
    BatteryReverse,
    ReverseBias,
    ReverseObservation,
    Comparison,
    Matching,
    Challenge,
    Questions,
    Conclusion,
    Result
}

public enum ElectronicsScoreCategory
{
    Equipment,
    Placement,
    ForwardCircuit,
    ForwardObservation,
    BatteryReverse,
    ReverseCircuit,
    ReverseObservation,
    Comparison,
    Questions,
    Conclusion
}

public enum ElectronicsEquipmentType
{
    Diode,
    Bulb,
    DryCells,
    Switch,
    Breadboard,
    Wires,
    NewtonBalance,
    Spring,
    WoodenBlock,
    Beaker,
    MeasuringCylinder,
    Thermometer,
    Ruler,
    Ammeter,
    Voltmeter,
    Magnet,
    Stopwatch,
    Pulley
}

[Serializable]
public class ElectronicsEquipmentDefinition
{
    public ElectronicsEquipmentType type;
    public string displayName;
    public bool isRequired;
    public string correctReason;
    public string incorrectReason;
}

[Serializable]
public class ElectronicsDiodeObservation
{
    public string connectionType;
    public bool bulbGlowing;
    public bool currentFlowing;
    public string observationText;
}

[Serializable]
public class ElectronicsAttemptRecord
{
    public int attemptNumber;
    public int score;
    public int maxScore = 100;
    public int mistakes;
    public string status;
    public string date;
    public bool selectedCorrectEquipment;
    public bool circuitConnected;
    public bool forwardBiasCompleted;
    public bool reverseBiasCompleted;
    public bool observationCompleted;
    public bool questionsCompleted;
    public bool conclusionCompleted;
    public string summary;
}

[Serializable]
public class ElectronicsExperimentSaveData
{
    public string studentId = "Student001";
    public string studentName = "Student";
    public string practicalName = "Investigation of Forward Bias and Reverse Bias of a Diode";
    public string topic = "Electronics";
    public string activity = "Investigation of Forward Bias and Reverse Bias of a Diode";
    public int lastScore;
    public int bestScore;
    public int attemptCount;
    public bool completionStatus;
    public int mistakes;
    public bool forwardBiasCompleted;
    public bool reverseBiasCompleted;
    public bool observationCompleted;
    public bool questionsCompleted;
    public bool conclusionCompleted;
    public string lastCompletedDate;
    public List<ElectronicsAttemptRecord> attemptHistory = new List<ElectronicsAttemptRecord>();
    public ElectronicsDiodeObservation forwardObservation = new ElectronicsDiodeObservation();
    public ElectronicsDiodeObservation reverseObservation = new ElectronicsDiodeObservation();
}
