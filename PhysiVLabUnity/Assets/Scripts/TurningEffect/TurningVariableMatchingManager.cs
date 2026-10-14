using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurningVariableMatchingManager : MonoBehaviour
{
    public static TurningVariableMatchingManager Instance { get; private set; }

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
        WireRow(rows, "Tightness");
        WireRow(rows, "Force");
        WireRow(rows, "PointD");
        WireRow(rows, "Angle");
        WireRow(rows, "Stick");
        WireRow(rows, "Pivot");
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
        var choice = t.GetComponent<TurningVariableChoiceButton>() ?? t.gameObject.AddComponent<TurningVariableChoiceButton>();
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
                TurningScoreManager.Instance?.AddScore(5, false);
                TurningFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + Explain(itemId), "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
            }
        }
        else
        {
            TurningScoreManager.Instance?.SubtractScore(5);
            TurningFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + Explain(itemId), "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
        if (independent.Contains("Tightness") &&
            dependent.Contains("Force") &&
            controlled.Contains("PointD") && controlled.Contains("Angle") &&
            controlled.Contains("Stick") && controlled.Contains("Pivot"))
        {
            completed = true;
            TurningUIManager.Instance?.SetNextButtonVisible(true);
            TurningFeedbackManager.Instance?.ShowInstruction("All variables identified. Press NEXT STEP.");
        }
    }

    public void OnItemDropped(TurningUIDropTarget zone, TurningDragDrop2D item)
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
        if (itemId == "Tightness") return "Independent";
        if (itemId == "Force") return "Dependent";
        return "Controlled";
    }

    private static string Explain(string itemId)
    {
        switch (itemId)
        {
            case "Tightness": return "Tightness of the screw is the independent variable — you change it by half turns.";
            case "Force": return "The minimum force on the Newton balance is the dependent variable — it is measured.";
            case "PointD": return "The force is always applied at D (60 cm). That distance is controlled.";
            case "Angle": return "The pull is kept perpendicular (90°) to the stick. That is a controlled condition.";
            case "Stick": return "The same calibrated stick is used in every trial.";
            default: return "The same pivot O (screw nail) is used throughout.";
        }
    }
}
