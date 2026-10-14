public enum FrictionEquipmentType
{
    WoodenBlock,
    NewtonBalance,
    Sandpaper,
    LabTable,
    MeasuringRuler,
    RecordingSheet,
    ForceDisplay,
    Ammeter,
    Voltmeter,
    DryCell,
    Bulb,
    Beaker,
    MeasuringCylinder,
    Thermometer,
    Magnet,
    Stopwatch,
    Pulley,
    Spring,
    MassHanger,
    Compass,
    BunsenBurner
}

[System.Serializable]
public class FrictionEquipmentDefinition
{
    public FrictionEquipmentType type;
    public string displayName;
    public bool isRequired;
    public bool isOptional;
    public string correctReason;
    public string incorrectReason;
}
