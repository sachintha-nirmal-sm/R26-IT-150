using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultantVariableMatchingManager : MonoBehaviour
{
    public static ResultantVariableMatchingManager Instance { get; private set; }

    private readonly HashSet<string> independent = new HashSet<string>();
    private readonly HashSet<string> dependent = new HashSet<string>();
    private readonly HashSet<string> controlled = new HashSet<string>();
    private readonly HashSet<string> scored = new HashSet<string>();
    private bool completed;

    public bool IsComplete => completed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ResetMatching()
    {
        independent.Clear();
        dependent.Clear();
        controlled.Clear();
        scored.Clear();
        completed = false;
    }

    public void Bind(Transform panel)
    {
        if (panel == null) return;
        var rows = panel.Find("Rows");
        if (rows == null) return;
        WireRow(rows, "ForceB");
        WireRow(rows, "ForceC");
        WireRow(rows, "ResultantA");
        WireRow(rows, "Direction");
        WireRow(rows, "Trolley");
        WireRow(rows, "PulleySetup");
    }

    private static void WireRow(Transform rows, string id)
    {
        var row = rows.Find(id);
        if (row == null) return;
        WireBtn(row.Find("Ind"), id, "Independent");
        WireBtn(row.Find("Dep"), id, "Dependent");
        WireBtn(row.Find("Ctrl"), id, "Controlled");
    }

    private static void WireBtn(Transform t, string id, string zone)
    {
        if (t == null) return;
        var choice = t.GetComponent<ResultantVariableChoiceButton>() ?? t.gameObject.AddComponent<ResultantVariableChoiceButton>();
        choice.Configure(id, zone);
        var btn = t.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(choice.Activate);
        }
    }

    public void Assign(string itemId, string zoneId)
    {
        if (completed) return;
        string expected = ExpectedZone(itemId);
        if (expected == zoneId)
        {
            AddTo(zoneId, itemId);
            if (!scored.Contains(itemId))
            {
                scored.Add(itemId);
                ResultantScoreManager.Instance?.AddScore(5, false);
                ResultantFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + Explain(itemId), "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
            }
        }
        else
        {
            ResultantScoreManager.Instance?.SubtractScore(5);
            ResultantFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + Explain(itemId), "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
        if (independent.Contains("ForceB") && independent.Contains("ForceC") &&
            dependent.Contains("ResultantA") &&
            controlled.Contains("Direction") && controlled.Contains("Trolley") && controlled.Contains("PulleySetup"))
        {
            completed = true;
            ResultantUIManager.Instance?.SetNextButtonVisible(true);
            ResultantFeedbackManager.Instance?.ShowInstruction("All variables identified. Press NEXT STEP.");
        }
    }

    public void OnItemDropped(ResultantUIDropTarget zone, ResultantDragDrop2D item)
    {
        if (zone == null || item == null) return;
        if (zone.ZoneId == "Independent" || zone.ZoneId == "Dependent" || zone.ZoneId == "Controlled")
            Assign(item.ItemId, zone.ZoneId);
    }

    public void TapAssign(string itemId, string zoneId) => Assign(itemId, zoneId);

    private void AddTo(string zone, string item)
    {
        if (zone == "Independent") independent.Add(item);
        else if (zone == "Dependent") dependent.Add(item);
        else controlled.Add(item);
    }

    private static string ExpectedZone(string itemId)
    {
        if (itemId == "ForceB" || itemId == "ForceC") return "Independent";
        if (itemId == "ResultantA") return "Dependent";
        return "Controlled";
    }

    private static string Explain(string itemId)
    {
        switch (itemId)
        {
            case "ForceB": return "Force B is an independent variable — you change it with Newton balance B.";
            case "ForceC": return "Force C is an independent variable — you change it with Newton balance C.";
            case "ResultantA": return "The resultant on balance A is the dependent variable — it is measured.";
            case "Direction": return "The two forces are kept in the same direction. That is a controlled condition.";
            case "Trolley": return "The same trolley is used in every trial.";
            default: return "The pulley arrangement is kept the same so both forces still act in the same direction.";
        }
    }
}
