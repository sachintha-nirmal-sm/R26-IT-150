using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeatVariableMatchingManager : MonoBehaviour
{
    public static HeatVariableMatchingManager Instance { get; private set; }

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
        WireRow(rows, "Temperature");
        WireRow(rows, "LiquidLevel");
        WireRow(rows, "LiquidType");
        WireRow(rows, "GlassTube");
        WireRow(rows, "ThinTube");
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
        var choice = t.GetComponent<HeatVariableChoiceButton>() ?? t.gameObject.AddComponent<HeatVariableChoiceButton>();
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
                HeatScoreManager.Instance?.AddScore(5, false);
                HeatFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + Explain(itemId), "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
            }
        }
        else
        {
            HeatScoreManager.Instance?.SubtractScore(5);
            HeatFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + Explain(itemId), "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
        if (independent.Contains("Temperature") &&
            dependent.Contains("LiquidLevel") &&
            controlled.Contains("LiquidType") && controlled.Contains("GlassTube") &&
            controlled.Contains("ThinTube"))
        {
            completed = true;
            HeatUIManager.Instance?.SetNextButtonVisible(true);
            HeatFeedbackManager.Instance?.ShowInstruction("All variables identified. Press NEXT STEP.");
        }
    }

    public void OnItemDropped(HeatUIDropTarget zone, HeatDragDrop2D item)
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
        if (itemId == "Temperature") return "Independent";
        if (itemId == "LiquidLevel") return "Dependent";
        return "Controlled";
    }

    private static string Explain(string itemId)
    {
        switch (itemId)
        {
            case "Temperature": return "You change the temperature of the water bath by heating — the independent variable.";
            case "LiquidLevel": return "The height of liquid in the thin tube is what you observe — the dependent variable.";
            case "LiquidType": return "The same coloured water is used throughout. That is a controlled condition.";
            case "GlassTube": return "The same glass test tube is used. That is controlled.";
            default: return "The same thin glass tube is used to show the level. That is controlled.";
        }
    }
}
