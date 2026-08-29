using TMPro;
using UnityEngine;

public class TurningObservationTableManager : MonoBehaviour
{
    public static TurningObservationTableManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI tableText;
    private bool tableScored;

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
        if (!tableScored && TurningTrialManager.Instance != null && TurningTrialManager.Instance.AllTrialsComplete())
        {
            tableScored = true;
            TurningScoreManager.Instance?.AddScore(5, false);
        }
    }

    public void ResetScoring() => tableScored = false;

    public string BuildTable()
    {
        string s =
            "OBSERVATION TABLE — TURNING EFFECT OF A FORCE\n\n" +
            "Point D = 60 cm from pivot O. Force is pulled perpendicular to the stick.\n\n" +
            "Trial | Tightness | Force (N) | Angle | Distance (cm) | Moment F × d (N m)\n" +
            "--------------------------------------------------------------------------------\n";
        for (int i = 0; i < 3; i++)
        {
            var t = TurningTrialManager.Instance != null ? TurningTrialManager.Instance.GetTrial(i + 1) : null;
            if (t != null && t.completed)
                s += $"{i + 1,-5} |     {t.tightnessLevel,-5} | {t.forceN,8:0.0} | {t.angleDeg,5:0}° | {t.distanceCm,13:0} | {t.momentNm,16:0.00}\n";
            else
                s += $"{i + 1,-5} |     ___   |      ___ |   ___ |           ___ |              ___\n";
        }
        s += "--------------------------------------------------------------------------------\n";
        s += "\nTurning effect (moment) = Force × perpendicular distance from the pivot.\n";
        s += "Tightening the screw increases friction at O, so a larger force (and a larger moment) is needed to turn the stick.\n";
        return s;
    }
}
