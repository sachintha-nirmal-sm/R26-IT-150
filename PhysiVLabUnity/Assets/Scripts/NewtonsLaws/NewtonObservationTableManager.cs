using TMPro;
using UnityEngine;

public class NewtonObservationTableManager : MonoBehaviour
{
    public static NewtonObservationTableManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI tableText;
    private bool tableBonusAwarded;

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
        if (!tableBonusAwarded && NewtonDataManager.Instance != null && NewtonDataManager.Instance.CompletedTrialCount() > 0)
        {
            tableBonusAwarded = true;
            NewtonScoreManager.Instance?.AddScore(5, false);
        }
    }

    public string BuildTable()
    {
        var data = NewtonDataManager.Instance;
        string s =
            "NEWTON'S SECOND LAW — CONSTANT MASS\n\n" +
            "Trial | Mass (kg) | Force (N) | Acceleration (m/s²)\n" +
            "---------------------------------------------------------\n";
        if (data == null || data.ForceSeries.Count == 0)
            s += "1     |  —        |  —        |  —\n";
        else
        {
            foreach (var t in data.ForceSeries)
                s += $"{t.trialNumber}     | {t.mass,8:0.00} | {t.force,8:0.00} | {t.acceleration,12:0.00}\n";
        }

        s += "\nNEWTON'S SECOND LAW — CONSTANT FORCE\n\n";
        s += "Trial | Force (N) | Mass (kg) | Acceleration (m/s²)\n";
        s += "---------------------------------------------------------\n";
        if (data == null || data.MassSeries.Count == 0)
            s += "1     |  —        |  —        |  —\n";
        else
        {
            foreach (var t in data.MassSeries)
                s += $"{t.trialNumber}     | {t.force,8:0.00} | {t.mass,8:0.00} | {t.acceleration,12:0.00}\n";
        }

        s += "\nWEIGHT  W = mg    g = 9.8 m/s²\n\n";
        s += "Object | Mass (kg) | Spring Balance (N) | Calculated Weight (N)\n";
        s += "----------------------------------------------------------------\n";
        if (data == null || data.WeightSeries.Count == 0)
            s += "1      |  —        |  —                 |  —\n";
        else
        {
            foreach (var t in data.WeightSeries)
                s += $"{t.trialNumber}      | {t.mass,8:0.00} | {t.force,17:0.00} | {t.weight,20:0.00}\n";
        }

        s += "\nFIRST LAW OBSERVATIONS\n";
        s += $"Stationary trolley: {(data != null ? data.StationaryObservation : "")}\n";
        s += $"Moving trolley: {(data != null ? data.MovingObservation : "")}\n";
        return s;
    }

    public void ResetBonus() => tableBonusAwarded = false;
}
