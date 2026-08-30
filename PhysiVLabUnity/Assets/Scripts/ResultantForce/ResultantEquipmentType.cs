public enum ResultantEquipmentType
{
    Trolley,
    NewtonBalance,
    Pulley,
    Ring,
    String,
    LabTable,
    RecordingSheet,
    WoodenBlock,
    Sandpaper,
    Spring,
    Ammeter,
    Voltmeter,
    DryCell,
    Bulb,
    Beaker,
    MeasuringCylinder,
    Thermometer,
    Magnet,
    Stopwatch,
    MassHanger,
    Compass,
    BunsenBurner
}

[System.Serializable]
public class ResultantEquipmentDefinition
{
    public ResultantEquipmentType type;
    public string displayName;
    public bool isRequired;
    public bool isOptional;
    public string correctReason;
    public string incorrectReason;
}
