public enum TurningEquipmentType
{
    WoodenStick,
    RubberWashers,
    Drill,
    NewtonBalance,
    LabTable,
    ScrewNail,
    Wire,
    Trolley,
    Pulley,
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
    BunsenBurner,
    WoodenBlock
}

[System.Serializable]
public class TurningEquipmentDefinition
{
    public TurningEquipmentType type;
    public string displayName;
    public bool isRequired;
    public bool isOptional;
    public string correctReason;
    public string incorrectReason;
}
