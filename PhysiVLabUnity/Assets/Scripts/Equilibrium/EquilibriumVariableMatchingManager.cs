using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquilibriumVariableMatchingManager : MonoBehaviour
{
    public static EquilibriumVariableMatchingManager Instance { get; private set; }

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
        WireRow(rows, "Repeat");
        WireRow(rows, "Readings");
        WireRow(rows, "Ruler");
        WireRow(rows, "Vertical");
        WireRow(rows, "Coplanar");
        WireRow(rows, "Horizontal");
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
        var choice = t.GetComponent<EquilibriumVariableChoiceButton>() ?? t.gameObject.AddComponent<EquilibriumVariableChoiceButton>();
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
                EquilibriumScoreManager.Instance?.AddScore(5, false);
                EquilibriumFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + Explain(itemId), "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
            }
        }
        else
        {
            EquilibriumScoreManager.Instance?.SubtractScore(5);
            EquilibriumFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + Explain(itemId), "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
        if (independent.Contains("Repeat") &&
            dependent.Contains("Readings") &&
            controlled.Contains("Ruler") && controlled.Contains("Vertical") &&
            controlled.Contains("Coplanar") && controlled.Contains("Horizontal"))
        {
            completed = true;
            EquilibriumUIManager.Instance?.SetNextButtonVisible(true);
            EquilibriumFeedbackManager.Instance?.ShowInstruction("All variables identified. Press NEXT STEP.");
        }
    }

    public void OnItemDropped(EquilibriumUIDropTarget zone, EquilibriumDragDrop2D item)
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
        if (itemId == "Repeat") return "Independent";
        if (itemId == "Readings") return "Dependent";
        return "Controlled";
    }

    private static string Explain(string itemId)
    {
        switch (itemId)
        {
            case "Repeat": return "Repeating the trial is what you change (you take three separate readings).";
            case "Readings": return "F1 and F2 are the dependent variables — they are the measured spring-balance readings.";
            case "Ruler": return "The same meter ruler (same weight W) is used in every trial.";
            case "Vertical": return "The spring balances are kept vertical. That is a controlled condition.";
            case "Coplanar": return "The three forces are kept in the same plane so the ruler does not twist.";
            default: return "The ruler is kept horizontal in every recorded trial.";
        }
    }
}
