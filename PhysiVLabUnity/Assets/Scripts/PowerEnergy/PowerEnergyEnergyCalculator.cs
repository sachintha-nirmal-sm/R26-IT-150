using UnityEngine;

public class PowerEnergyEnergyCalculator : MonoBehaviour
{
    public static PowerEnergyEnergyCalculator Instance { get; private set; }

    public const string Formula = "E = Pt";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public float CalculateEnergy(float power, float time)
    {
        return power * time;
    }

    public string FormatWorkedExample(float power, float time)
    {
        float e = CalculateEnergy(power, time);
        return
            $"{Formula}\n" +
            $"E = {FormatNumber(power)} × {FormatNumber(time)}\n" +
            $"E = {FormatNumber(e)} J";
    }

    public bool IsCorrect(float power, float displayPower, float time, float studentEnergy)
    {
        float exact = CalculateEnergy(power, time);
        float rounded = CalculateEnergy(displayPower, time);
        if (Approximately(studentEnergy, exact, 0.08f, 2f)) return true;
        if (Approximately(studentEnergy, rounded, 0.08f, 2f)) return true;
        return false;
    }

    private static bool Approximately(float a, float b, float rel, float abs)
    {
        return Mathf.Abs(a - b) <= Mathf.Max(abs, rel * Mathf.Max(Mathf.Abs(a), Mathf.Abs(b)));
    }

    private static string FormatNumber(float value)
    {
        if (Mathf.Abs(value) >= 1000f) return value.ToString("N0");
        if (Mathf.Abs(value) >= 10f) return value.ToString("0.##");
        return value.ToString("0.###");
    }
}
