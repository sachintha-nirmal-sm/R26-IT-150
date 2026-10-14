using TMPro;
using UnityEngine;

public class EquilibriumObservationTableManager : MonoBehaviour
{
    public static EquilibriumObservationTableManager Instance { get; private set; }

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
        if (!tableScored && EquilibriumTrialManager.Instance != null && EquilibriumTrialManager.Instance.AllTrialsComplete())
        {
            tableScored = true;
            EquilibriumScoreManager.Instance?.AddScore(5, false);
        }
    }

    public void ResetScoring() => tableScored = false;

    public string BuildTable()
    {
        float w = EquilibriumForceController.Instance != null && EquilibriumForceController.Instance.WeightRecorded
            ? EquilibriumForceController.Instance.MeasuredW
            : EquilibriumForceController.TrueWeightN;
        string s =
            "OBSERVATION TABLE — EQUILIBRIUM OF FORCES\n\n" +
            "A meter ruler is suspended horizontally by two vertical spring balances.\n" +
            $"Measured weight of the ruler  W = {w:0.00} N\n\n" +
            "Trial |  F1 (N)  |  F2 (N)  |  F1 + F2 (N)  |  W (N)  |  Horizontal?\n" +
            "--------------------------------------------------------------------------------\n";
        for (int i = 0; i < 3; i++)
        {
            var t = EquilibriumTrialManager.Instance != null ? EquilibriumTrialManager.Instance.GetTrial(i + 1) : null;
            if (t != null && t.completed)
                s += $"{i + 1,-5} | {t.force1N,8:0.00} | {t.force2N,8:0.00} | {t.sumN,13:0.00} | {t.weightN,7:0.00} | {(t.horizontal ? "Yes" : "No")}\n";
            else
                s += $"{i + 1,-5} |      ___ |      ___ |           ___ |     ___ |     ___\n";
        }
        s += "--------------------------------------------------------------------------------\n";
        s += "\nFor a horizontal ruler in equilibrium:  F1 + F2 = W\n";
        s += "The two upward forces and the downward weight are three coplanar parallel forces.\n";
        return s;
    }
}
