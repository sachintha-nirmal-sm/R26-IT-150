/// <summary>
/// Apparatus identities for the Upthrust / Archimedes practical.
/// </summary>
public enum UpthrustApparatusType
{
    MetalCube,
    SpringBalance,
    EurekaCan,
    SmallBeaker,
    RetortStand,
    Thermometer,
    Voltmeter,
    ConvexLens,
    FrictionWoodenBlock
}

public static class UpthrustApparatusTypeUtil
{
    public static bool IsCorrect(UpthrustApparatusType type)
    {
        switch (type)
        {
            case UpthrustApparatusType.MetalCube:
            case UpthrustApparatusType.SpringBalance:
            case UpthrustApparatusType.EurekaCan:
            case UpthrustApparatusType.SmallBeaker:
            case UpthrustApparatusType.RetortStand:
                return true;
            default:
                return false;
        }
    }

    public static string DisplayName(UpthrustApparatusType type)
    {
        switch (type)
        {
            case UpthrustApparatusType.MetalCube: return "Metal Cube";
            case UpthrustApparatusType.SpringBalance: return "Spring Balance (0–5 N)";
            case UpthrustApparatusType.EurekaCan: return "Eureka Can / Overflow Vessel";
            case UpthrustApparatusType.SmallBeaker: return "Small Empty Beaker";
            case UpthrustApparatusType.RetortStand: return "Retort Stand / Support";
            case UpthrustApparatusType.Thermometer: return "Thermometer";
            case UpthrustApparatusType.Voltmeter: return "Voltmeter";
            case UpthrustApparatusType.ConvexLens: return "Convex Lens";
            case UpthrustApparatusType.FrictionWoodenBlock: return "Friction Wooden Block";
            default: return type.ToString();
        }
    }
}
