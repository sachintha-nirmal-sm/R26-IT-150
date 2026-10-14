using UnityEngine;

public class ResultantAssemblyManager : MonoBehaviour
{
    public static ResultantAssemblyManager Instance { get; private set; }

    [SerializeField] private bool trolleyPlaced;
    [SerializeField] private bool ringPlaced;
    [SerializeField] private bool stringsPlaced;
    [SerializeField] private bool pulley1Placed;
    [SerializeField] private bool pulley2Placed;
    [SerializeField] private bool balanceBPlaced;
    [SerializeField] private bool balanceCPlaced;
    [SerializeField] private bool balanceAPlaced;
    [SerializeField] private bool setupConfirmed;

    private bool trolleyScored, ringScored, stringsScored, pulley1Scored, pulley2Scored;
    private bool balanceBScored, balanceCScored, balanceAScored, confirmScored;

    public bool IsComplete => trolleyPlaced && ringPlaced && stringsPlaced && pulley1Placed && pulley2Placed &&
                              balanceBPlaced && balanceCPlaced && balanceAPlaced && setupConfirmed;

    public bool TrolleyPlaced => trolleyPlaced;
    public bool RingPlaced => ringPlaced;
    public bool StringsPlaced => stringsPlaced;
    public bool Pulley1Placed => pulley1Placed;
    public bool Pulley2Placed => pulley2Placed;
    public bool BalanceBPlaced => balanceBPlaced;
    public bool BalanceCPlaced => balanceCPlaced;
    public bool BalanceAPlaced => balanceAPlaced;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ResetAssembly()
    {
        trolleyPlaced = ringPlaced = stringsPlaced = pulley1Placed = pulley2Placed = false;
        balanceBPlaced = balanceCPlaced = balanceAPlaced = setupConfirmed = false;
        trolleyScored = ringScored = stringsScored = pulley1Scored = pulley2Scored = false;
        balanceBScored = balanceCScored = balanceAScored = confirmScored = false;
        ResultantVisualController.Instance?.RefreshVisuals();
        ResultantUIManager.Instance?.SetInstruction(NextHint());
    }

    public bool TryPlace(string itemId, string zoneId)
    {
        switch (itemId)
        {
            case "Trolley":
                if (zoneId != "Table" && zoneId != "TrolleyZone") return Wrong("Place the trolley on the table.");
                return Place(ref trolleyPlaced, ref trolleyScored, "Trolley placed on the table.");
            case "Ring":
                if (!trolleyPlaced) return Wrong("Fix the ring after the trolley is on the table.");
                if (zoneId != "TrolleyFront" && zoneId != "RingZone" && zoneId != "TrolleyZone") return Wrong("Fix the ring to the front of the trolley.");
                return Place(ref ringPlaced, ref ringScored, "Ring fixed to the front of the trolley.");
            case "Strings":
                if (!ringPlaced) return Wrong("Attach the two strings to the ring first.");
                if (zoneId != "RingZone" && zoneId != "TrolleyFront" && zoneId != "StringZone") return Wrong("Tie the two strings to the ring.");
                return Place(ref stringsPlaced, ref stringsScored, "Two strings attached to the ring.");
            case "Pulley1":
                if (zoneId != "PulleyZone1" && zoneId != "TableEdge" && zoneId != "PulleyZone") return Wrong("Place pulley 1 at the table edge.");
                return Place(ref pulley1Placed, ref pulley1Scored, "Pulley 1 placed at the table edge.");
            case "Pulley2":
                if (zoneId != "PulleyZone2" && zoneId != "TableEdge" && zoneId != "PulleyZone") return Wrong("Place pulley 2 at the table edge.");
                return Place(ref pulley2Placed, ref pulley2Scored, "Pulley 2 placed at the table edge.");
            case "BalanceB":
                if (!stringsPlaced || !pulley1Placed) return Wrong("Pass a string over pulley 1, then attach Newton balance B.");
                if (zoneId != "HangB" && zoneId != "PulleyZone1" && zoneId != "BalanceBZone") return Wrong("Attach Newton balance B to the hanging string.");
                return Place(ref balanceBPlaced, ref balanceBScored, "Newton balance B attached to the string.");
            case "BalanceC":
                if (!stringsPlaced || !pulley2Placed) return Wrong("Pass a string over pulley 2, then attach Newton balance C.");
                if (zoneId != "HangC" && zoneId != "PulleyZone2" && zoneId != "BalanceCZone") return Wrong("Attach Newton balance C to the hanging string.");
                return Place(ref balanceCPlaced, ref balanceCScored, "Newton balance C attached to the string.");
            case "BalanceA":
                if (!trolleyPlaced) return Wrong("Place the trolley first, then attach balance A between the wall and the trolley.");
                if (zoneId != "WallZone" && zoneId != "BalanceAZone" && zoneId != "TrolleyBack") return Wrong("Attach Newton balance A between the wall and the back of the trolley.");
                return Place(ref balanceAPlaced, ref balanceAScored, "Newton balance A attached between the wall and the trolley.");
            default:
                return Wrong("That item does not belong in this position.");
        }
    }

    public bool ConfirmSetup()
    {
        if (!trolleyPlaced || !ringPlaced || !stringsPlaced || !pulley1Placed || !pulley2Placed ||
            !balanceBPlaced || !balanceCPlaced || !balanceAPlaced)
        {
            ResultantScoreManager.Instance?.SubtractScore(5);
            ResultantFeedbackManager.Instance?.ShowInstruction(NextHint());
            return false;
        }
        setupConfirmed = true;
        if (!confirmScored)
        {
            confirmScored = true;
            ResultantScoreManager.Instance?.AddScore(5, false);
            ResultantFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT SETUP\nTwo forces will now act on the trolley in the same direction. You may pull balances B and C.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
        }
        return true;
    }

    public string NextHint()
    {
        if (!trolleyPlaced) return "ASSEMBLY — Place the trolley on the table.";
        if (!ringPlaced) return "ASSEMBLY — Fix the ring to the front of the trolley.";
        if (!stringsPlaced) return "ASSEMBLY — Attach two strings to the ring.";
        if (!pulley1Placed) return "ASSEMBLY — Place pulley 1 at the table edge.";
        if (!pulley2Placed) return "ASSEMBLY — Place pulley 2 at the table edge.";
        if (!balanceBPlaced) return "ASSEMBLY — Attach Newton balance B to the string over pulley 1.";
        if (!balanceCPlaced) return "ASSEMBLY — Attach Newton balance C to the string over pulley 2.";
        if (!balanceAPlaced) return "ASSEMBLY — Attach Newton balance A between the wall and the back of the trolley.";
        return "ASSEMBLY complete. Press CONFIRM SETUP.";
    }

    private bool Place(ref bool flag, ref bool scored, string message)
    {
        flag = true;
        ResultantVisualController.Instance?.RefreshVisuals();
        ResultantUIManager.Instance?.SetInstruction(NextHint());
        if (!scored)
        {
            scored = true;
            ResultantScoreManager.Instance?.AddScore(5, false);
            ResultantFeedbackManager.Instance?.ShowMessage("✓ CORRECT SETUP\n" + message, "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
        return true;
    }

    private bool Wrong(string reason)
    {
        ResultantScoreManager.Instance?.SubtractScore(5);
        ResultantFeedbackManager.Instance?.ShowMessage("✗ INCORRECT SETUP\n" + reason, "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        return false;
    }
}
