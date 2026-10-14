using UnityEngine;

public class ElectronicsFormulaMatchingManager : MonoBehaviour
{
    public static ElectronicsFormulaMatchingManager Instance { get; private set; }

    private readonly System.Collections.Generic.HashSet<string> matched = new System.Collections.Generic.HashSet<string>();
    private bool completed;
    private string pendingLeft;

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
        pendingLeft = null;
    }

    public void OnItemDropped(ElectronicsUIDropTarget zone, ElectronicsDragDrop2D item)
    {
        if (zone == null || item == null) return;
        Assign(item.ItemId, zone.ZoneId);
    }

    public void TapLeft(string id)
    {
        pendingLeft = id;
        ElectronicsFeedbackManager.Instance?.ShowInstruction("Selected: " + Label(id) + ". Now tap the matching statement.");
    }

    public void TapRight(string zoneId)
    {
        if (string.IsNullOrEmpty(pendingLeft))
        {
            ElectronicsFeedbackManager.Instance?.ShowInstruction("Tap a term on the left first, then match it.");
            return;
        }
        Assign(pendingLeft, zoneId);
        pendingLeft = null;
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
                ElectronicsScoreManager.Instance?.AddToCategory(ElectronicsScoreCategory.Comparison, 2, false);
                ElectronicsFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + Explain(itemId), "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
            }
        }
        else
        {
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            ElectronicsFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + Explain(itemId), "-3 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }

        if (matched.Count >= 6)
        {
            completed = true;
            ElectronicsUIManager.Instance?.SetNextButtonVisible(true);
            ElectronicsFeedbackManager.Instance?.ShowInstruction("All concepts matched. Press NEXT STEP.");
        }
        ElectronicsUIManager.Instance?.UpdateMatchProgress(matched.Count, 6);
    }

    private static string Expected(string itemId)
    {
        switch (itemId)
        {
            case "ForwardBias": return "MatchFlow";
            case "ReverseBias": return "MatchBlocked";
            case "CorrectDirection": return "MatchForward";
            case "OppositeDirection": return "MatchReverse";
            case "BulbGlows": return "MatchForward";
            case "BulbDark": return "MatchReverse";
            default: return null;
        }
    }

    private static string Explain(string itemId)
    {
        switch (itemId)
        {
            case "ForwardBias": return "Forward bias → current can flow.";
            case "ReverseBias": return "Reverse bias → current is blocked.";
            case "CorrectDirection": return "Correct diode direction → forward bias.";
            case "OppositeDirection": return "Opposite diode direction → reverse bias.";
            case "BulbGlows": return "Bulb glows → forward bias.";
            default: return "Bulb does not glow → reverse bias.";
        }
    }

    private static string Label(string id)
    {
        switch (id)
        {
            case "ForwardBias": return "Forward Bias";
            case "ReverseBias": return "Reverse Bias";
            case "CorrectDirection": return "Correct diode direction";
            case "OppositeDirection": return "Opposite diode direction";
            case "BulbGlows": return "Bulb glows";
            default: return "Bulb does not glow";
        }
    }
}
