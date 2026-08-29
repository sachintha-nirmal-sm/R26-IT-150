[System.Serializable]
public class OpticsEquipmentDefinition
{
    public OpticsEquipmentType type;
    public string displayName;
    public bool isRequired;
    public bool isOptional;
    public string correctReason;
    public string incorrectReason;
}

public enum OpticsEquipmentType
{
    ConcaveMirror,
    WhiteScreen,
    MeterRuler,
    ConvexMirror,
    PlaneMirror,
    ConvexLens,
    ConcaveLens,
    GlassPrism,
    GlassSlab,
    Thermometer,
    NewtonBalance,
    WoodenBlock,
    Slinky,
    Ammeter,
    Voltmeter,
    DryCell,
    Bulb,
    Beaker,
    MeasuringCylinder,
    Magnet,
    Stopwatch,
    MassHanger,
    Compass,
    BunsenBurner,
    Pulley,
    Trolley
}
