using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpticsVariableMatchingManager : MonoBehaviour
{
    public static OpticsVariableMatchingManager Instance { get; private set; }

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
        WireRow(rows, "ScreenDistance");
        WireRow(rows, "ImageSharpness");
        WireRow(rows, "Mirror");
        WireRow(rows, "DistantObject");
        WireRow(rows, "Screen");
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
        var choice = t.GetComponent<OpticsVariableChoiceButton>() ?? t.gameObject.AddComponent<OpticsVariableChoiceButton>();
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
                OpticsScoreManager.Instance?.AddScore(5, false);
                OpticsFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + Explain(itemId), "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
            }
        }
        else
        {
            OpticsScoreManager.Instance?.SubtractScore(5);
            OpticsFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + Explain(itemId), "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
        if (independent.Contains("ScreenDistance") &&
            dependent.Contains("ImageSharpness") &&
            controlled.Contains("Mirror") && controlled.Contains("DistantObject") &&
            controlled.Contains("Screen"))
        {
            completed = true;
            OpticsUIManager.Instance?.SetNextButtonVisible(true);
            OpticsFeedbackManager.Instance?.ShowInstruction("All variables identified. Press NEXT STEP.");
        }
    }

    public void OnItemDropped(OpticsUIDropTarget zone, OpticsDragDrop2D item)
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
        if (itemId == "ScreenDistance") return "Independent";
        if (itemId == "ImageSharpness") return "Dependent";
        return "Controlled";
    }

    private static string Explain(string itemId)
    {
        switch (itemId)
        {
            case "ScreenDistance": return "You change the distance between the mirror and the screen — the independent variable.";
            case "ImageSharpness": return "How clear the image is on the screen is what you observe — the dependent variable.";
            case "Mirror": return "The same concave mirror is used throughout. That is a controlled condition.";
            case "DistantObject": return "The outdoor scene stays far away (object at infinity). That is controlled.";
            default: return "The same white screen is used to catch the image. That is controlled.";
        }
    }
}
