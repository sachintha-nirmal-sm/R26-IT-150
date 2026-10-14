using UnityEngine;

public class CircuitCalculationManager : MonoBehaviour
{
    public static CircuitCalculationManager Instance { get; private set; }

    [SerializeField] private float cellVoltage = 1.5f;
    [SerializeField] private float bulbResistance = 10f;
    [SerializeField] private float internalResistance;
    [SerializeField] private float opposingResidualVoltage;
    [SerializeField] private float highPowerReference = 0.9f;

    public float CellVoltage => cellVoltage;
    public float BulbResistance => bulbResistance;
    public float InternalResistance => internalResistance;
    public float HighPowerReference => highPowerReference;

    public float LastVoltage { get; private set; }
    public float LastCurrent { get; private set; }
    public float LastPower { get; private set; }
    public float LastBrightness { get; private set; }
    public ConnectionType LastArrangement { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Configure(float voltage, float resistance, float rInternal, float residual)
    {
        cellVoltage = voltage;
        bulbResistance = Mathf.Max(0.01f, resistance);
        internalResistance = Mathf.Max(0f, rInternal);
        opposingResidualVoltage = Mathf.Max(0f, residual);
    }

    public float CalculateSeriesAidingVoltage()
    {
        return cellVoltage + cellVoltage;
    }

    public float CalculateParallelVoltage()
    {
        return cellVoltage;
    }

    public float CalculateSeriesOpposingVoltage()
    {
        return Mathf.Abs(cellVoltage - cellVoltage) + opposingResidualVoltage;
    }

    public float VoltageFor(ConnectionType type)
    {
        switch (type)
        {
            case ConnectionType.SeriesAiding: return CalculateSeriesAidingVoltage();
            case ConnectionType.Parallel: return CalculateParallelVoltage();
            case ConnectionType.SeriesOpposing: return CalculateSeriesOpposingVoltage();
            default: return 0f;
        }
    }

    public float CalculateCurrent(float voltage)
    {
        float r = bulbResistance + internalResistance;
        if (r < 0.0001f) return 0f;
        return voltage / r;
    }

    public float CalculatePower(float voltage, float current)
    {
        return voltage * current;
    }

    public float CalculateBrightness(float power)
    {
        if (power <= 0.02f) return 0f;
        return Mathf.Clamp01(power / Mathf.Max(0.05f, highPowerReference));
    }

    public CircuitReading Evaluate(ConnectionType type, int connectionNumber, string arrangement)
    {
        float v = VoltageFor(type);
        float i = CalculateCurrent(v);
        float p = CalculatePower(v, i);
        float b = CalculateBrightness(p);
        LastVoltage = v;
        LastCurrent = i;
        LastPower = p;
        LastBrightness = b;
        LastArrangement = type;

        var reading = new CircuitReading
        {
            connectionNumber = connectionNumber,
            arrangement = arrangement,
            voltage = v,
            current = i,
            power = p,
            brightness = b,
            brightnessLabel = BrightnessLabel(b, v)
        };
        return reading;
    }

    public static string BrightnessLabel(float brightness, float voltage)
    {
        if (voltage <= 0.05f || brightness < 0.05f) return "OFF";
        if (brightness < 0.35f) return "Dim";
        if (brightness < 0.7f) return "Medium";
        return "High";
    }
}
