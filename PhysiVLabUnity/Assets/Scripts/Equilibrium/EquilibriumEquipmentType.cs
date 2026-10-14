public enum EquilibriumEquipmentType
{
    TwoSpringBalances,
    MeterRuler,
    TwoRubberBands,
    RetortStand,
    Trolley,
    Pulley,
    WoodenBlock,
    Sandpaper,
    Drill,
    ScrewNail,
    WoodenStick,
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
    Wire,
    LooseSpring
}

[System.Serializable]
public class EquilibriumEquipmentDefinition
{
    public EquilibriumEquipmentType type;
    public string displayName;
    public bool isRequired;
    public bool isOptional;
    public string correctReason;
    public string incorrectReason;
}
