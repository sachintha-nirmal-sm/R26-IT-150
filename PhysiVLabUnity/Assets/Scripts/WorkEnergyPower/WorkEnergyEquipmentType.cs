public enum WorkEnergyEquipmentType
{
    Clay,
    HeavyWeight,
    ReleaseStand,
    ReleaseMechanism,
    Ruler,
    DepthRuler,
    ClayTray,
    Balance,
    Beaker,
    MeasuringCylinder,
    Thermometer,
    NewtonSpringBalance,
    Ammeter,
    Voltmeter,
    Magnet,
    Pulley,
    Lever,
    Stopwatch,
    BunsenBurner,
    Compass
}

[System.Serializable]
public class EquipmentDefinition
{
    public WorkEnergyEquipmentType type;
    public string displayName;
    public bool isRequired;
    public bool isOptional;
    public string correctReason;
    public string incorrectReason;
}
