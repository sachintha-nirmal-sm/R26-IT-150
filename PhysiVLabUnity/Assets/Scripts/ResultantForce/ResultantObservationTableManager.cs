using TMPro;
using UnityEngine;

public class ResultantObservationTableManager : MonoBehaviour
{
    public static ResultantObservationTableManager Instance { get; private set; }

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
        if (!tableScored && ResultantTrialManager.Instance != null && ResultantTrialManager.Instance.AllTrialsComplete())
        {
            tableScored = true;
            ResultantScoreManager.Instance?.AddScore(5, false);
        }
    }

    public void ResetScoring() => tableScored = false;

    public string BuildTable()
    {
        string s =
            "OBSERVATION TABLE — TWO FORCES ACTING IN THE SAME DIRECTION\n\n" +
            "Trial | Force B (N) | Force C (N) | Force A (N) | B + C (N)\n" +
            "-----------------------------------------------------------------\n";
        for (int i = 0; i < 3; i++)
        {
            var t = ResultantTrialManager.Instance != null ? ResultantTrialManager.Instance.GetTrial(i + 1) : null;
            if (t != null && t.completed)
                s += $"{i + 1,-5} | {t.forceB,11:0.0} | {t.forceC,11:0.0} | {t.forceA,11:0.0} | {(t.forceB + t.forceC),9:0.0}\n";
            else
                s += $"{i + 1,-5} |         ___ |         ___ |         ___ |       ___\n";
        }
        s += "-----------------------------------------------------------------\n";
        s += "\nRule:  Force A  =  Force B  +  Force C\n";
        s += "The two forces B and C act on the trolley in the same direction.\n";
        return s;
    }
}
