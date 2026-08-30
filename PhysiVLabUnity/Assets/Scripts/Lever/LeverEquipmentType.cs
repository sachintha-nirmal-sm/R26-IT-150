public enum LeverEquipmentType
{
    Book,
    NewtonSpringBalance,
    WoodenStrip,
    SupportPivot,
    Ruler,
    Beaker,
    MeasuringCylinder,
    Thermometer,
    Stopwatch,
    Ammeter,
    Voltmeter,
    Magnet,
    BunsenBurner,
    SandBag,
    Pulley
}

[System.Serializable]
public class LeverEquipmentDefinition
{
    public LeverEquipmentType type;
    public string displayName;
    public bool isRequired;
}
