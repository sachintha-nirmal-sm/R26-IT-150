using TMPro;
using UnityEngine;

public class ElecObservationTableManager : MonoBehaviour
{
    public static ElecObservationTableManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI tableText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(TextMeshProUGUI text)
    {
        tableText = text;
        Refresh();
    }

    public void Refresh()
    {
        if (tableText == null) return;
        var data = ElecExperimentDataManager.Instance;
        tableText.text = BuildTable(data != null ? data.Readings : null);
    }

    public string BuildTable(System.Collections.Generic.IReadOnlyList<CircuitReading> readings)
    {
        string s =
            "OBSERVATION TABLE\n\n" +
            "Conn | Arrangement      | V (V) | I (A) | Brightness | P (W)\n" +
            "--------------------------------------------------------------\n";
        for (int i = 1; i <= 3; i++)
        {
            CircuitReading r = null;
            if (readings != null && readings.Count >= i) r = readings[i - 1];
            if (r == null || r.connectionNumber != i)
            {
                s += $"{i}    | —                 |  —    |  —    |  —         |  —\n";
                continue;
            }
            s += $"{r.connectionNumber}    | {Pad(r.arrangement, 17)} | {r.voltage,5:0.00} | {r.current,5:0.00} | {Pad(r.brightnessLabel, 10)} | {r.power,5:0.00}\n";
        }
        s += "\nSimulation values are based on a simplified circuit model.";
        return s;
    }

    private static string Pad(string value, int width)
    {
        if (string.IsNullOrEmpty(value)) value = "—";
        if (value.Length >= width) return value.Substring(0, width);
        return value + new string(' ', width - value.Length);
    }
}
