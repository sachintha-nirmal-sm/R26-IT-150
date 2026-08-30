using UnityEngine;

public class EquilibriumAssemblyManager : MonoBehaviour
{
    public static EquilibriumAssemblyManager Instance { get; private set; }

    [SerializeField] private bool standPlaced;
    [SerializeField] private bool balance1Placed;
    [SerializeField] private bool balance2Placed;
    [SerializeField] private bool rulerPlaced;
    [SerializeField] private bool bandLeft;
    [SerializeField] private bool bandRight;
    [SerializeField] private bool setupConfirmed;

    private bool standScored, b1Scored, b2Scored, rulerScored, leftScored, rightScored, confirmScored;

    public bool IsComplete => standPlaced && balance1Placed && balance2Placed && rulerPlaced && bandLeft && bandRight && setupConfirmed;
    public bool StandPlaced => standPlaced;
    public bool Balance1Placed => balance1Placed;
    public bool Balance2Placed => balance2Placed;
    public bool RulerPlaced => rulerPlaced;
    public bool BandLeft => bandLeft;
    public bool BandRight => bandRight;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ResetAssembly()
    {
        standPlaced = balance1Placed = balance2Placed = rulerPlaced = bandLeft = bandRight = setupConfirmed = false;
        standScored = b1Scored = b2Scored = rulerScored = leftScored = rightScored = confirmScored = false;
        EquilibriumVisualController.Instance?.RefreshVisuals();
        EquilibriumUIManager.Instance?.SetInstruction(NextHint());
    }

    public bool TryPlace(string itemId, string zoneId)
    {
        switch (itemId)
        {
            case "Stand":
                if (zoneId != "StandZone" && zoneId != "ExperimentHost") return Wrong("Place the retort stand in the experiment area.");
                return Place(ref standPlaced, ref standScored, "Retort stand set up. The spring balances will hang from it.");
            case "Balance1":
                if (!standPlaced) return Wrong("Place the retort stand first, then hang spring balance F1 on the left.");
                if (zoneId != "LeftHang" && zoneId != "StandZone") return Wrong("Hang spring balance F1 from the left support.");
                return Place(ref balance1Placed, ref b1Scored, "Spring balance F1 hung vertically on the left.");
            case "Balance2":
                if (!standPlaced) return Wrong("Place the retort stand first, then hang spring balance F2 on the right.");
                if (zoneId != "RightHang" && zoneId != "StandZone") return Wrong("Hang spring balance F2 from the right support.");
                return Place(ref balance2Placed, ref b2Scored, "Spring balance F2 hung vertically on the right.");
            case "Ruler":
                if (!balance1Placed || !balance2Placed) return Wrong("Hang both spring balances before placing the meter ruler.");
                if (zoneId != "RulerZone" && zoneId != "StandZone") return Wrong("Place the meter ruler in the experiment area, ready to hang.");
                return Place(ref rulerPlaced, ref rulerScored, "Meter ruler placed. Its weight W will act at the centre of gravity.");
            case "BandLeft":
                if (!rulerPlaced) return Wrong("Place the meter ruler first, then loop a rubber band around the left end.");
                if (zoneId != "LeftEnd" && zoneId != "RulerZone") return Wrong("Loop a rubber band around the left end of the ruler.");
                return Place(ref bandLeft, ref leftScored, "Rubber band looped around the left end (to hook onto F1).");
            case "BandRight":
                if (!rulerPlaced) return Wrong("Place the meter ruler first, then loop a rubber band around the right end.");
                if (zoneId != "RightEnd" && zoneId != "RulerZone") return Wrong("Loop a rubber band around the right end of the ruler.");
                return Place(ref bandRight, ref rightScored, "Rubber band looped around the right end (to hook onto F2).");
            default:
                return Wrong("That item does not belong in this position.");
        }
    }

    public bool ConfirmSetup()
    {
        if (!standPlaced || !balance1Placed || !balance2Placed || !rulerPlaced || !bandLeft || !bandRight)
        {
            EquilibriumScoreManager.Instance?.SubtractScore(5);
            EquilibriumFeedbackManager.Instance?.ShowInstruction(NextHint());
            return false;
        }
        setupConfirmed = true;
        if (!confirmScored)
        {
            confirmScored = true;
            EquilibriumScoreManager.Instance?.AddScore(5, false);
            EquilibriumFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT SETUP\nNext, hang the ruler from one spring balance and measure its weight W.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
        }
        return true;
    }

    public string NextHint()
    {
        if (!standPlaced) return "ASSEMBLY — Place the retort stand / support in the experiment area.";
        if (!balance1Placed) return "ASSEMBLY — Hang spring balance F1 from the left support.";
        if (!balance2Placed) return "ASSEMBLY — Hang spring balance F2 from the right support.";
        if (!rulerPlaced) return "ASSEMBLY — Place the meter ruler in the experiment area.";
        if (!bandLeft) return "ASSEMBLY — Loop a rubber band around the left end of the ruler.";
        if (!bandRight) return "ASSEMBLY — Loop a rubber band around the right end of the ruler.";
        return "ASSEMBLY — All apparatus is in place. Press CONFIRM SETUP.";
    }

    private bool Place(ref bool flag, ref bool scored, string message)
    {
        if (flag)
        {
            EquilibriumFeedbackManager.Instance?.ShowInstruction("That part is already in place.");
            return true;
        }
        flag = true;
        if (!scored)
        {
            scored = true;
            EquilibriumScoreManager.Instance?.AddScore(5, false);
            EquilibriumFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + message, "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
        EquilibriumVisualController.Instance?.RefreshVisuals();
        EquilibriumUIManager.Instance?.SetInstruction(NextHint());
        return true;
    }

    private static bool Wrong(string message)
    {
        EquilibriumScoreManager.Instance?.SubtractScore(5);
        EquilibriumFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + message, "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        return false;
    }
}
