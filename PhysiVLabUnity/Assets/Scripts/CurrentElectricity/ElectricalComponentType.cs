public enum ElectricalComponentType
{
    DryCell,
    Bulb,
    Ammeter,
    Voltmeter,
    Wire,
    CircuitBoard
}

public enum ElecEquipmentType
{
    DryCell1,
    DryCell2,
    ConductingWires,
    Bulb,
    Ammeter,
    Voltmeter,
    CircuitBoard,
    NewtonSpringBalance,
    Ruler,
    Thermometer,
    MeasuringCylinder,
    Beaker,
    Magnet,
    Pulley,
    Lever,
    Stopwatch,
    BunsenBurner,
    Compass,
    Clay,
    HeavyWeight,
    Spring,
    Barometer,
    IncorrectAmmeter
}

[System.Serializable]
public class ElecEquipmentDefinition
{
    public ElecEquipmentType type;
    public string displayName;
    public bool isRequired;
    public bool isOptional;
    public string correctReason;
    public string incorrectReason;
}
