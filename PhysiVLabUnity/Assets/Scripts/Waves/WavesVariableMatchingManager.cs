using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WavesVariableMatchingManager : MonoBehaviour
{
    public static WavesVariableMatchingManager Instance { get; private set; }

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
        WireRow(rows, "Shake");
        WireRow(rows, "RibbonMotion");
        WireRow(rows, "Slinky");
        WireRow(rows, "Table");
        WireRow(rows, "Ribbons");
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
        var choice = t.GetComponent<WavesVariableChoiceButton>() ?? t.gameObject.AddComponent<WavesVariableChoiceButton>();
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
                WavesScoreManager.Instance?.AddScore(5, false);
                WavesFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + Explain(itemId), "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
            }
        }
        else
        {
            WavesScoreManager.Instance?.SubtractScore(5);
            WavesFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + Explain(itemId), "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
        if (independent.Contains("Shake") &&
            dependent.Contains("RibbonMotion") &&
            controlled.Contains("Slinky") && controlled.Contains("Table") &&
            controlled.Contains("Ribbons"))
        {
            completed = true;
            WavesUIManager.Instance?.SetNextButtonVisible(true);
            WavesFeedbackManager.Instance?.ShowInstruction("All variables identified. Press NEXT STEP.");
        }
    }

    public void OnItemDropped(WavesUIDropTarget zone, WavesDragDrop2D item)
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
        if (itemId == "Shake") return "Independent";
        if (itemId == "RibbonMotion") return "Dependent";
        return "Controlled";
    }

    private static string Explain(string itemId)
    {
        switch (itemId)
        {
            case "Shake": return "How you shake the slinky (side to side vs push-pull) is what you change — the independent variable.";
            case "RibbonMotion": return "The motion of the ribbons is what you observe — the dependent variable.";
            case "Slinky": return "The same slinky is used throughout. That is a controlled condition.";
            case "Table": return "The slinky is kept flat on the table in every run. That is controlled.";
            default: return "Ribbons stay tied at the same points on the slinky. That is controlled.";
        }
    }
}
