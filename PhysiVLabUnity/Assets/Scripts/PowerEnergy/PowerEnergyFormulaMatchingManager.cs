using UnityEngine;

public class PowerEnergyFormulaMatchingManager : MonoBehaviour
{
    public static PowerEnergyFormulaMatchingManager Instance { get; private set; }

    private readonly System.Collections.Generic.HashSet<string> matched = new System.Collections.Generic.HashSet<string>();
    private bool completed;

    public bool IsComplete => completed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ResetMatching()
    {
        matched.Clear();
        completed = false;
    }

    public void OnItemDropped(PowerEnergyUIDropTarget zone, PowerEnergyDragDrop2D item)
    {
        if (zone == null || item == null) return;
        Assign(item.ItemId, zone.ZoneId);
    }

    public void Assign(string itemId, string zoneId)
    {
        if (completed) return;
        string expected = Expected(itemId);
        if (string.IsNullOrEmpty(expected)) return;
        if (expected == zoneId)
        {
            if (!matched.Contains(itemId))
            {
                matched.Add(itemId);
                PowerEnergyScoreManager.Instance?.AddToCategory(PowerEnergyScoreCategory.Questions, 5, false);
                PowerEnergyFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + Explain(itemId), "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
            }
        }
        else
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + Explain(itemId), "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }

        if (matched.Contains("Power") && matched.Contains("Energy") && matched.Contains("Kwh"))
        {
            completed = true;
            PowerEnergyUIManager.Instance?.SetNextButtonVisible(true);
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("All formulas matched. Press NEXT STEP.");
        }
    }

    public void TapMatch(string itemId, string zoneId) => Assign(itemId, zoneId);

    private static string Expected(string itemId)
    {
        switch (itemId)
        {
            case "Power": return "FormulaPower";
            case "Energy": return "FormulaEnergy";
            case "Kwh": return "FormulaKwh";
            default: return null;
        }
    }

    private static string Explain(string itemId)
    {
        switch (itemId)
        {
            case "Power": return "Power is calculated using P = VI.";
            case "Energy": return "Electrical energy is calculated using E = Pt.";
            default: return "Energy in kWh = Energy in J / 3,600,000.";
        }
    }
}
