using TMPro;
using UnityEngine;

public class MotionObservationTableManager : MonoBehaviour
{
    public static MotionObservationTableManager Instance { get; private set; }

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
        tableText.text = BuildTable();
    }

    public string BuildTable()
    {
        var data = MotionDataManager.Instance;
        string s =
            "OBSERVATION TABLE — MOTION TRIALS\n\n" +
            "Trial | Initial (m) | Final (m) | Distance (m) | Displacement (m) | Time (s) | Speed (m/s) | Velocity (m/s)\n" +
            "----------------------------------------------------------------------------------------------------------------\n";
        for (int i = 1; i <= 5; i++)
        {
            var t = data != null ? data.GetTrial(i) : null;
            if (t == null || t.time <= 0f)
            {
                s += $"{i}     | 0           | {i}         |  —           |  —               |  —       |  —          |  —\n";
                continue;
            }
            s += $"{t.trialNumber}     | {t.initialPosition,7:0.00}   | {t.finalPosition,7:0.00} | {t.distance,10:0.00}   | {t.displacement,13:0.00}    | {t.time,6:0.00}  | {t.speed,8:0.00}    | {t.velocity,8:0.00}\n";
        }

        s += "\nACCELERATION TABLE    a = (v − u) / t\n";
        s += "Trial | Condition     | u (m/s) | v (m/s) | Time (s) | Acceleration (m/s²)\n";
        s += "----------------------------------------------------------------------------\n";
        if (data != null && data.AccelerationTrials.Count > 0)
        {
            foreach (var a in data.AccelerationTrials)
            {
                if (a == null) continue;
                s += $"{a.trialNumber}     | {Pad(a.condition, 13)} | {a.initialVelocity,7:0.00} | {a.finalVelocity,7:0.00} | {a.time,8:0.00} | {a.acceleration,12:0.00}\n";
            }
        }
        else s += "—     | —             |  —      |  —      |  —       |  —\n";

        if (data != null && data.PathDistance > 0f)
        {
            s += "\nDISTANCE vs DISPLACEMENT (0 m → 3 m → 1 m)\n";
            s += $"Distance = {data.PathDistance:0.00} m     Displacement = {data.PathDisplacement:+0.00;-0.00} m\n";
        }
        return s;
    }

    private static string Pad(string value, int width)
    {
        if (string.IsNullOrEmpty(value)) value = "—";
        if (value.Length >= width) return value.Substring(0, width);
        return value + new string(' ', width - value.Length);
    }
}
