using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VariableMatchingManager : MonoBehaviour
{
    public static VariableMatchingManager Instance { get; private set; }

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
        WireRow(rows, "ContactArea");
        WireRow(rows, "LimitingFriction");
        WireRow(rows, "Weight");
        WireRow(rows, "Roughness");
        WireRow(rows, "WoodenBlock");
        WireRow(rows, "SurfaceMaterial");
    }

    private static void WireRow(Transform rows, string id)
    {
        var row = rows.Find(id);
        if (row == null) return;
        WireBtn(row.Find("Ind"), id, "Independent");
        WireBtn(row.Find("Dep"), id, "Dependent");
        WireBtn(row.Find("Ctrl"), id, "Controlled");
    }

    private static void WireBtn(Transform t, string itemId, string zone)
    {
        if (t == null) return;
        var choice = t.GetComponent<VariableChoiceButton>() ?? t.gameObject.AddComponent<VariableChoiceButton>();
        choice.Configure(itemId, zone);
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
                FrictionScoreManager.Instance?.AddScore(5, false);
                FrictionFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + Explain(itemId), "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
            }
        }
        else
        {
            FrictionScoreManager.Instance?.SubtractScore(5);
            FrictionFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + Explain(itemId), "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
        if (independent.Contains("ContactArea") && dependent.Contains("LimitingFriction") &&
            controlled.Contains("Weight") && controlled.Contains("Roughness") &&
            controlled.Contains("WoodenBlock") && controlled.Contains("SurfaceMaterial"))
        {
            completed = true;
            FrictionUIManager.Instance?.SetNextButtonVisible(true);
            FrictionFeedbackManager.Instance?.ShowInstruction("All variables identified. Press NEXT STEP.");
        }
    }

    public void OnItemDropped(FrictionUIDropTarget zone, FrictionDragDrop2D item)
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
        if (itemId == "ContactArea") return "Independent";
        if (itemId == "LimitingFriction") return "Dependent";
        return "Controlled";
    }

    private static string Explain(string itemId)
    {
        switch (itemId)
        {
            case "ContactArea": return "Contact area is the independent variable — it is deliberately changed.";
            case "LimitingFriction": return "Limiting frictional force is the dependent variable — it is measured.";
            case "Weight": return "Weight is controlled at 60 N.";
            case "Roughness": return "Sandpaper roughness is kept constant.";
            case "WoodenBlock": return "The same wooden block is used for every trial.";
            default: return "Surface material is kept the same.";
        }
    }
}
