using UnityEngine;

public class TurningAssemblyManager : MonoBehaviour
{
    public static TurningAssemblyManager Instance { get; private set; }

    [SerializeField] private bool tablePlaced;
    [SerializeField] private bool stickPlaced;
    [SerializeField] private bool holeO;
    [SerializeField] private bool washer1;
    [SerializeField] private bool screwNail;
    [SerializeField] private bool washer2;
    [SerializeField] private bool holeA, holeB, holeC, holeD;
    [SerializeField] private bool loopsPlaced;
    [SerializeField] private bool setupConfirmed;

    private bool tableScored, stickScored, oScored, w1Scored, nailScored, w2Scored;
    private bool aScored, bScored, cScored, dScored, loopScored, confirmScored;

    public bool IsComplete => tablePlaced && stickPlaced && holeO && washer1 && screwNail && washer2 &&
                              holeA && holeB && holeC && holeD && loopsPlaced && setupConfirmed;

    public bool TablePlaced => tablePlaced;
    public bool StickPlaced => stickPlaced;
    public bool HoleODrilled => holeO;
    public bool Washer1Placed => washer1;
    public bool ScrewPlaced => screwNail;
    public bool Washer2Placed => washer2;
    public bool HoleADrilled => holeA;
    public bool HoleBDrilled => holeB;
    public bool HoleCDrilled => holeC;
    public bool HoleDDrilled => holeD;
    public bool LoopsPlaced => loopsPlaced;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ResetAssembly()
    {
        tablePlaced = stickPlaced = holeO = washer1 = screwNail = washer2 = false;
        holeA = holeB = holeC = holeD = loopsPlaced = setupConfirmed = false;
        tableScored = stickScored = oScored = w1Scored = nailScored = w2Scored = false;
        aScored = bScored = cScored = dScored = loopScored = confirmScored = false;
        TurningVisualController.Instance?.RefreshVisuals();
        TurningUIManager.Instance?.SetInstruction(NextHint());
    }

    public bool TryPlace(string itemId, string zoneId)
    {
        switch (itemId)
        {
            case "Table":
                if (zoneId != "TableZone" && zoneId != "Table") return Wrong("Place the table or plank in the experiment area.");
                return Place(ref tablePlaced, ref tableScored, "Table (or plank) set in the experiment area.");
            case "Stick":
                if (!tablePlaced) return Wrong("Place the table first, then lay the calibrated stick on it.");
                if (zoneId != "StickZone" && zoneId != "Table" && zoneId != "TableZone") return Wrong("Lay the calibrated stick on the table.");
                return Place(ref stickPlaced, ref stickScored, "Calibrated stick placed on the table. Marks A–D are 15 cm apart.");
            case "Drill":
                return TryDrill(zoneId);
            case "Washer1":
                if (!holeO) return Wrong("Drill hole O first, then place a rubber washer at the pivot.");
                if (zoneId != "PivotO" && zoneId != "StickZone") return Wrong("Place washer 1 at point O.");
                return Place(ref washer1, ref w1Scored, "First rubber washer placed at O.");
            case "ScrewNail":
                if (!washer1) return Wrong("Place a rubber washer at O before driving the screw nail.");
                if (zoneId != "PivotO" && zoneId != "StickZone") return Wrong("Drive the screw nail through hole O.");
                return Place(ref screwNail, ref nailScored, "Screw nail driven through O — this is the pivot.");
            case "Washer2":
                if (!screwNail) return Wrong("Drive the screw nail first, then add the second washer.");
                if (zoneId != "PivotO" && zoneId != "StickZone") return Wrong("Place washer 2 at point O.");
                return Place(ref washer2, ref w2Scored, "Second rubber washer clamps the stick at the pivot.");
            case "Wire":
                if (!holeA || !holeB || !holeC || !holeD) return Wrong("Drill holes A, B, C and D before attaching wire loops.");
                if (zoneId != "LoopA" && zoneId != "LoopB" && zoneId != "LoopC" && zoneId != "LoopD" &&
                    zoneId != "HoleA" && zoneId != "HoleB" && zoneId != "HoleC" && zoneId != "HoleD" && zoneId != "StickZone")
                    return Wrong("Attach a wire loop to each hole at A, B, C and D.");
                return Place(ref loopsPlaced, ref loopScored, "Wire loops attached at A, B, C and D.");
            case "NewtonBalance":
                return Wrong("Hook the Newton balance after you confirm the setup.");
            default:
                return Wrong("That item does not belong in this position.");
        }
    }

    private bool TryDrill(string zoneId)
    {
        if (!stickPlaced) return Wrong("Place the calibrated stick on the table before drilling.");
        if (zoneId == "PivotO" || zoneId == "StickZone")
        {
            if (holeO) return Wrong("Hole O is already drilled. Drill A, B, C and D next, 15 cm apart.");
            return Place(ref holeO, ref oScored, "Hole drilled at O (the pivot), 0 cm.");
        }
        if (!holeO) return Wrong("Drill hole O (the pivot) first.");
        if (zoneId == "HoleA" || zoneId == "LoopA")
        {
            if (holeA) return Wrong("Hole A is already drilled.");
            return Place(ref holeA, ref aScored, "Hole drilled at A, 15 cm from O.");
        }
        if (zoneId == "HoleB" || zoneId == "LoopB")
        {
            if (holeB) return Wrong("Hole B is already drilled.");
            return Place(ref holeB, ref bScored, "Hole drilled at B, 30 cm from O.");
        }
        if (zoneId == "HoleC" || zoneId == "LoopC")
        {
            if (holeC) return Wrong("Hole C is already drilled.");
            return Place(ref holeC, ref cScored, "Hole drilled at C, 45 cm from O.");
        }
        if (zoneId == "HoleD" || zoneId == "LoopD")
        {
            if (holeD) return Wrong("Hole D is already drilled.");
            return Place(ref holeD, ref dScored, "Hole drilled at D, 60 cm from O.");
        }
        return Wrong("Use the drill on O, A, B, C or D. The holes are 15 cm apart.");
    }

    public bool ConfirmSetup()
    {
        if (!tablePlaced || !stickPlaced || !holeO || !washer1 || !screwNail || !washer2 ||
            !holeA || !holeB || !holeC || !holeD || !loopsPlaced)
        {
            TurningScoreManager.Instance?.SubtractScore(5);
            TurningFeedbackManager.Instance?.ShowInstruction(NextHint());
            return false;
        }
        setupConfirmed = true;
        if (!confirmScored)
        {
            confirmScored = true;
            TurningScoreManager.Instance?.AddScore(5, false);
            TurningFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT SETUP\nThe stick can turn about O. Hook the Newton balance at D and pull perpendicular to the stick.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
        }
        return true;
    }

    public string NextHint()
    {
        if (!tablePlaced) return "ASSEMBLY — Place the table or plank in the experiment area.";
        if (!stickPlaced) return "ASSEMBLY — Lay the calibrated wooden stick on the table.";
        if (!holeO) return "ASSEMBLY — Drill a hole at O. This will be the pivot.";
        if (!washer1) return "ASSEMBLY — Place the first rubber washer at O.";
        if (!screwNail) return "ASSEMBLY — Drive the screw nail through O to clamp the stick.";
        if (!washer2) return "ASSEMBLY — Place the second rubber washer at O.";
        if (!holeA) return "ASSEMBLY — Drill a hole at A, 15 cm from O.";
        if (!holeB) return "ASSEMBLY — Drill a hole at B, 30 cm from O.";
        if (!holeC) return "ASSEMBLY — Drill a hole at C, 45 cm from O.";
        if (!holeD) return "ASSEMBLY — Drill a hole at D, 60 cm from O.";
        if (!loopsPlaced) return "ASSEMBLY — Attach wire loops at A, B, C and D.";
        return "ASSEMBLY complete. Press CONFIRM SETUP.";
    }

    private bool Place(ref bool flag, ref bool scored, string message)
    {
        flag = true;
        TurningVisualController.Instance?.RefreshVisuals();
        TurningUIManager.Instance?.SetInstruction(NextHint());
        if (!scored)
        {
            scored = true;
            TurningScoreManager.Instance?.AddScore(5, false);
            TurningFeedbackManager.Instance?.ShowMessage("✓ CORRECT SETUP\n" + message, "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
        return true;
    }

    private bool Wrong(string reason)
    {
        TurningScoreManager.Instance?.SubtractScore(5);
        TurningFeedbackManager.Instance?.ShowMessage("✗ INCORRECT SETUP\n" + reason, "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        return false;
    }
}
