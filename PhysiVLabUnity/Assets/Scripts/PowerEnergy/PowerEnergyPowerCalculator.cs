using UnityEngine;

public class PowerEnergyPowerCalculator : MonoBehaviour
{
    public static PowerEnergyPowerCalculator Instance { get; private set; }

    public const string Formula = "P = VI";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public float CalculatePower(float voltage, float current)
    {
        return voltage * current;
    }

    public string FormatWorkedExample(float voltage, float current)
    {
        float p = CalculatePower(voltage, current);
        return
            $"{Formula}\n" +
            $"P = {FormatNumber(voltage)} × {FormatNumber(current)}\n" +
            $"P = {FormatNumber(p)} W";
    }

    public bool IsCorrect(float voltage, float current, float studentPower, float roundedDisplay)
    {
        float exact = CalculatePower(voltage, current);
        if (Approximately(studentPower, exact, 0.08f, 0.5f)) return true;
        if (Approximately(studentPower, roundedDisplay, 0.08f, 0.6f)) return true;
        if (Approximately(studentPower, Mathf.Round(exact), 0.02f, 0.6f)) return true;
        return false;
    }

    private static bool Approximately(float a, float b, float rel, float abs)
    {
        return Mathf.Abs(a - b) <= Mathf.Max(abs, rel * Mathf.Max(Mathf.Abs(a), Mathf.Abs(b)));
    }

    private static string FormatNumber(float value)
    {
        if (Mathf.Abs(value) >= 100f) return value.ToString("0");
        if (Mathf.Abs(value) >= 10f) return value.ToString("0.##");
        return value.ToString("0.###");
    }
}
