public enum MotionEquipmentType
{
    ToyCar,
    StraightTrack,
    MetreRuler,
    Stopwatch,
    DistanceMarkers,
    StartingMarker,
    RecordingTable,
    Calculator,
    NewtonSpringBalance,
    Ammeter,
    Voltmeter,
    Bulb,
    DryCell,
    Beaker,
    MeasuringCylinder,
    Thermometer,
    Magnet,
    Pulley,
    Lever,
    Clay,
    BunsenBurner,
    Compass
}

[System.Serializable]
public class MotionEquipmentDefinition
{
    public MotionEquipmentType type;
    public string displayName;
    public bool isRequired;
    public bool isOptional;
    public string correctReason;
    public string incorrectReason;
}
