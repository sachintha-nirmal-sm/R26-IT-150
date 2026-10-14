public enum PowerEnergyExperimentStep
{
    Introduction,
    Objective,
    SelectEquipment,
    CircuitSetup,
    Experiment,
    ObservationTable,
    Compare,
    Graph,
    FormulaMatch,
    IdentifyVariables,
    Questions,
    Conclusion,
    Complete
}

public enum PowerEnergyScoreCategory
{
    Equipment,
    Circuit,
    Voltage,
    Current,
    Power,
    Energy,
    Kwh,
    Observation,
    Questions,
    Conclusion
}

public enum PowerEnergyEquipmentType
{
    ElectricalAppliance,
    PowerSupply,
    Voltmeter,
    Ammeter,
    Timer,
    Calculator,
    ObservationSheet,
    NewtonBalance,
    Spring,
    WoodenBlock,
    Thermometer,
    MeasuringCylinder,
    Beaker,
    Magnet,
    Ruler,
    Stopwatch,
    Pulley,
    Switch,
    Wire,
    Bulb,
    Fan,
    Iron,
    Kettle
}

[System.Serializable]
public class PowerEnergyEquipmentDefinition
{
    public PowerEnergyEquipmentType type;
    public string displayName;
    public bool isRequired;
    public string correctReason;
    public string incorrectReason;
}
