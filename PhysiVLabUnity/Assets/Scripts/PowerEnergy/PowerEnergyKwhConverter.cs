using UnityEngine;

public class PowerEnergyKwhConverter : MonoBehaviour
{
    public static PowerEnergyKwhConverter Instance { get; private set; }

    public const float JoulesPerKwh = 3600000f;
    public const string ConversionFact = "1 kWh = 3,600,000 J";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public float ConvertJoulesToKwh(float energyJoules)
    {
        return energyJoules / JoulesPerKwh;
    }

    public string FormatWorkedExample(float energyJoules)
    {
        float kwh = ConvertJoulesToKwh(energyJoules);
        return
            $"{ConversionFact}\n" +
            $"Energy in kWh = Energy in J / 3,600,000\n" +
            $"{FormatJoules(energyJoules)} / 3,600,000\n" +
            $"= {kwh:0.####} kWh";
    }

    public bool IsCorrect(float energyJoules, float studentKwh)
    {
        float exact = ConvertJoulesToKwh(energyJoules);
        if (Mathf.Abs(studentKwh - exact) <= Mathf.Max(0.0002f, 0.08f * Mathf.Abs(exact)))
            return true;
        if (Mathf.Abs(exact - 1f) < 0.0001f && Mathf.Abs(studentKwh - 1f) < 0.02f)
            return true;
        return false;
    }

    private static string FormatJoules(float value)
    {
        return value.ToString("N0");
    }
}
