using UnityEngine;

public class OpticsAssemblyManager : MonoBehaviour
{
    public static OpticsAssemblyManager Instance { get; private set; }

    private bool windowOpened;
    private bool mirrorPlaced;
    private bool screenPlaced;
    private bool rulerPlaced;
    private bool setupConfirmed;
    private bool windowScored;
    private bool mirrorScored;
    private bool screenScored;
    private bool rulerScored;

    public bool WindowOpened => windowOpened;
    public bool MirrorPlaced => mirrorPlaced;
    public bool ScreenPlaced => screenPlaced;
    public bool RulerPlaced => rulerPlaced;
    public bool SetupConfirmed => setupConfirmed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ResetAssembly()
    {
        windowOpened = false;
        mirrorPlaced = false;
        screenPlaced = false;
        rulerPlaced = false;
        setupConfirmed = false;
        windowScored = false;
        mirrorScored = false;
        screenScored = false;
        rulerScored = false;
        OpticsVisualController.Instance?.ResetVisuals();
        OpticsUIManager.Instance?.SetInstruction(NextHint());
    }

    public string SuggestedZone(string itemId)
    {
        if (itemId == "ConcaveMirror" && windowOpened && !mirrorPlaced) return "MirrorZone";
        if (itemId == "WhiteScreen" && mirrorPlaced && !screenPlaced) return "ScreenZone";
        if (itemId == "MeterRuler" && screenPlaced && !rulerPlaced) return "RulerZone";
        return null;
    }

    public bool OpenWindow()
    {
        if (windowOpened) return true;
        windowOpened = true;
        if (!windowScored)
        {
            windowScored = true;
            OpticsScoreManager.Instance?.AddScore(5, false);
        }
        OpticsVisualController.Instance?.ShowWindowOpen(true);
        OpticsFeedbackManager.Instance?.ShowMessage(
            "✓ CORRECT\nThe window is open. Light from the distant outdoor scene can now enter the room.",
            "+5 MARKS",
            new Color(0.08f, 0.52f, 0.22f));
        OpticsUIManager.Instance?.SetInstruction(NextHint());
        OpticsUIManager.Instance?.UpdateLiveReadings();
        return true;
    }

    public bool TryPlace(string itemId, string zoneId)
    {
        if (itemId == "ConcaveMirror")
        {
            if (!windowOpened)
            {
                OpticsScoreManager.Instance?.SubtractScore(5);
                OpticsFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\nOpen the window first so that light from a distant object can enter.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
                return false;
            }
            if (zoneId != "MirrorZone" && zoneId != "RoomZone")
            {
                OpticsScoreManager.Instance?.SubtractScore(5);
                OpticsFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\nHold the concave mirror turned towards the open window.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
                return false;
            }
            mirrorPlaced = true;
            if (!mirrorScored)
            {
                mirrorScored = true;
                OpticsScoreManager.Instance?.AddScore(5, false);
            }
            OpticsVisualController.Instance?.ShowMirror(true);
            OpticsFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT\nThe concave mirror faces the window, so parallel rays from the distant scene strike it.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            OpticsUIManager.Instance?.SetInstruction(NextHint());
            OpticsUIManager.Instance?.UpdateLiveReadings();
            return true;
        }

        if (itemId == "WhiteScreen")
        {
            if (!mirrorPlaced)
            {
                OpticsScoreManager.Instance?.SubtractScore(5);
                OpticsFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\nPlace the concave mirror facing the window before holding the screen.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
                return false;
            }
            if (zoneId != "ScreenZone" && zoneId != "RoomZone" && zoneId != "MirrorZone")
            {
                OpticsScoreManager.Instance?.SubtractScore(5);
                OpticsFeedbackManager.Instance?.ShowInstruction("Hold the white screen in front of the concave mirror.");
                return false;
            }
            screenPlaced = true;
            if (!screenScored)
            {
                screenScored = true;
                OpticsScoreManager.Instance?.AddScore(5, false);
            }
            OpticsVisualController.Instance?.ShowScreen(true);
            OpticsFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT\nThe white screen is in front of the mirror. A real image can form on it.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            OpticsUIManager.Instance?.SetInstruction(NextHint());
            OpticsUIManager.Instance?.UpdateLiveReadings();
            return true;
        }

        if (itemId == "MeterRuler")
        {
            if (!screenPlaced)
            {
                OpticsScoreManager.Instance?.SubtractScore(5);
                OpticsFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\nPlace the white screen first, then the ruler to measure mirror–screen distance.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
                return false;
            }
            rulerPlaced = true;
            if (!rulerScored)
            {
                rulerScored = true;
                OpticsScoreManager.Instance?.AddScore(5, false);
            }
            OpticsVisualController.Instance?.ShowRuler(true);
            OpticsFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT\nThe meter ruler will measure the distance from the mirror to the screen.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            OpticsUIManager.Instance?.SetInstruction(NextHint());
            OpticsUIManager.Instance?.UpdateLiveReadings();
            return true;
        }

        OpticsScoreManager.Instance?.SubtractScore(5);
        OpticsFeedbackManager.Instance?.ShowInstruction("That item does not belong in this setup.");
        return false;
    }

    public bool ConfirmSetup()
    {
        if (!windowOpened || !mirrorPlaced || !screenPlaced || !rulerPlaced)
        {
            OpticsScoreManager.Instance?.SubtractScore(5);
            OpticsFeedbackManager.Instance?.ShowMessage(
                "✗ INCOMPLETE\n" + NextHint(),
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            return false;
        }
        if (!setupConfirmed)
        {
            setupConfirmed = true;
            OpticsScoreManager.Instance?.AddScore(5, false);
        }
        OpticsFeedbackManager.Instance?.ShowMessage(
            "✓ SETUP COMPLETE\nNow move the screen until a clear, upside-down image of the outdoor scene is formed.",
            "+5 MARKS",
            new Color(0.08f, 0.52f, 0.22f));
        return true;
    }

    public string NextHint()
    {
        if (!windowOpened) return "Press OPEN WINDOW so light from a distant object can enter the room.";
        if (!mirrorPlaced) return "Place the CONCAVE MIRROR turned towards the open window.";
        if (!screenPlaced) return "Hold the WHITE SCREEN in front of the concave mirror.";
        if (!rulerPlaced) return "Place the METER RULER to measure the distance from the mirror to the screen.";
        return "All apparatus is in place. Press CONFIRM SETUP.";
    }
}
