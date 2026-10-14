public enum NewtonEquipmentType
{
    DynamicsTrolley,
    StraightTrack,
    NewtonSpringBalance,
    MassBlocks,
    WeightHanger,
    Stopwatch,
    Ruler,
    Balloon,
    String,
    Pulley,
    RecordingTable,
    Calculator,
    Straw,
    Ammeter,
    Voltmeter,
    Bulb,
    DryCell,
    Beaker,
    MeasuringCylinder,
    Thermometer,
    Magnet,
    BunsenBurner,
    Compass,
    Microscope,
    Pipette
}

[System.Serializable]
public class NewtonEquipmentDefinition
{
    public NewtonEquipmentType type;
    public string displayName;
    public bool isRequired;
    public bool isOptional;
    public string correctReason;
    public string incorrectReason;
}
