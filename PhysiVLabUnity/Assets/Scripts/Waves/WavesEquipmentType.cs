[System.Serializable]
public class WavesEquipmentDefinition
{
    public WavesEquipmentType type;
    public string displayName;
    public bool isRequired;
    public bool isOptional;
    public string correctReason;
    public string incorrectReason;
}

public enum WavesEquipmentType
{
    Slinky,
    Ribbons,
    Table,
    NewtonBalance,
    WoodenBlock,
    MeterRuler,
    LooseSpring,
    Beaker,
    Ammeter,
    Voltmeter,
    DryCell,
    Bulb,
    MeasuringCylinder,
    Thermometer,
    Magnet,
    Stopwatch,
    MassHanger,
    Compass,
    BunsenBurner,
    Pulley,
    Trolley,
    Sandpaper
}
