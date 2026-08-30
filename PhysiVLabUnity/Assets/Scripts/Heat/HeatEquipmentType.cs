[System.Serializable]
public class HeatEquipmentDefinition
{
    public HeatEquipmentType type;
    public string displayName;
    public bool isRequired;
    public bool isOptional;
    public string correctReason;
    public string incorrectReason;
}

public enum HeatEquipmentType
{
    TestTube,
    ColoredWater,
    RubberStopper,
    ThinGlassTube,
    Beaker,
    BunsenBurner,
    RetortStand,
    TripodStand,
    Thermometer,
    MeasuringCylinder,
    NewtonBalance,
    WoodenBlock,
    Slinky,
    Ammeter,
    Voltmeter,
    DryCell,
    Bulb,
    Magnet,
    Stopwatch,
    MassHanger,
    Compass,
    Pulley,
    Trolley,
    ConcaveMirror,
    ConvexLens,
    GlassPrism,
    MeterRuler,
    WhiteScreen,
    GlassSlab,
    ConcaveLens
}
