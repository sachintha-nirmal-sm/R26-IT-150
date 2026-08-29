using System.Collections.Generic;
using UnityEngine;

public class WavesAssemblyManager : MonoBehaviour
{
    public static WavesAssemblyManager Instance { get; private set; }

    private bool tablePlaced;
    private bool slinkyPlaced;
    private readonly HashSet<int> tiedRibbons = new HashSet<int>();
    private bool setupConfirmed;
    private bool tableScored;
    private bool slinkyScored;

    public bool TablePlaced => tablePlaced;
    public bool SlinkyPlaced => slinkyPlaced;
    public bool AllRibbonsTied => tiedRibbons.Count >= 5;
    public int RibbonCount => tiedRibbons.Count;
    public bool SetupConfirmed => setupConfirmed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ResetAssembly()
    {
        tablePlaced = false;
        slinkyPlaced = false;
        tiedRibbons.Clear();
        setupConfirmed = false;
        tableScored = false;
        slinkyScored = false;
        WavesVisualController.Instance?.ResetVisuals();
        WavesUIManager.Instance?.SetInstruction(NextHint());
    }

    public string SuggestedZone(string itemId)
    {
        if (itemId == "Table" && !tablePlaced) return "TableZone";
        if (itemId == "Slinky" && tablePlaced && !slinkyPlaced) return "SlinkyZone";
        if (itemId != null && itemId.StartsWith("Ribbon") && slinkyPlaced)
        {
            for (int i = 0; i < 5; i++)
                if (!tiedRibbons.Contains(i)) return "RibbonSlot" + i;
        }
        return null;
    }

    public bool TryPlace(string itemId, string zoneId)
    {
        if (itemId == "Table")
        {
            if (zoneId != "TableZone")
            {
                WavesScoreManager.Instance?.SubtractScore(5);
                WavesFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\nPlace the table in the experiment area first.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
                return false;
            }
            tablePlaced = true;
            if (!tableScored)
            {
                tableScored = true;
                WavesScoreManager.Instance?.AddScore(5, false);
            }
            WavesVisualController.Instance?.ShowTable(true);
            WavesFeedbackManager.Instance?.ShowMessage("✓ CORRECT\nThe slinky will lie flat on this table.", "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
            WavesUIManager.Instance?.SetInstruction(NextHint());
            return true;
        }

        if (itemId == "Slinky")
        {
            if (!tablePlaced)
            {
                WavesScoreManager.Instance?.SubtractScore(5);
                WavesFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\nPlace the table first, then lay the slinky on it.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
                return false;
            }
            if (zoneId != "SlinkyZone" && zoneId != "TableZone")
            {
                WavesScoreManager.Instance?.SubtractScore(5);
                WavesFeedbackManager.Instance?.ShowInstruction("Drop the slinky onto the table.");
                return false;
            }
            slinkyPlaced = true;
            if (!slinkyScored)
            {
                slinkyScored = true;
                WavesScoreManager.Instance?.AddScore(5, false);
            }
            WavesVisualController.Instance?.ShowSlinky(true);
            WavesFeedbackManager.Instance?.ShowMessage("✓ CORRECT\nThe slinky is laid flat on the table.", "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
            WavesUIManager.Instance?.SetInstruction(NextHint());
            return true;
        }

        if (itemId != null && itemId.StartsWith("Ribbon"))
        {
            if (!slinkyPlaced)
            {
                WavesScoreManager.Instance?.SubtractScore(5);
                WavesFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\nLay the slinky on the table before tying ribbons.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
                return false;
            }

            int slot = ParseRibbonSlot(zoneId, itemId);
            if (slot < 0 || slot > 4)
            {
                WavesScoreManager.Instance?.SubtractScore(4);
                WavesFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\nTie each ribbon onto a marked point on the slinky.", "-4 MARKS", new Color(0.75f, 0.12f, 0.12f));
                return false;
            }
            if (tiedRibbons.Contains(slot))
            {
                WavesFeedbackManager.Instance?.ShowInstruction("That point already has a ribbon. Use another mark.");
                return false;
            }
            tiedRibbons.Add(slot);
            WavesScoreManager.Instance?.AddScore(4, false);
            WavesVisualController.Instance?.ShowRibbon(slot, true);
            WavesFeedbackManager.Instance?.ShowMessage(
                $"✓ CORRECT\nRibbon {tiedRibbons.Count} of 5 tied. These mark particles of the medium.",
                "+4 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            WavesUIManager.Instance?.SetInstruction(NextHint());
            return true;
        }

        WavesScoreManager.Instance?.SubtractScore(5);
        WavesFeedbackManager.Instance?.ShowInstruction("That item does not belong in this setup.");
        return false;
    }

    public bool ConfirmSetup()
    {
        if (!tablePlaced || !slinkyPlaced || !AllRibbonsTied)
        {
            WavesScoreManager.Instance?.SubtractScore(5);
            WavesFeedbackManager.Instance?.ShowMessage(
                "✗ INCOMPLETE\n" + NextHint(),
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            return false;
        }
        if (!setupConfirmed)
        {
            setupConfirmed = true;
            WavesScoreManager.Instance?.AddScore(5, false);
        }
        WavesFeedbackManager.Instance?.ShowMessage(
            "✓ SETUP COMPLETE\nHold one end of the slinky. Next you will shake it from side to side.",
            "+5 MARKS",
            new Color(0.08f, 0.52f, 0.22f));
        return true;
    }

    public string NextHint()
    {
        if (!tablePlaced) return "Drag the TABLE into the experiment area.";
        if (!slinkyPlaced) return "Lay the SLINKY flat on the table.";
        if (!AllRibbonsTied) return $"Tie ribbons at several places along the slinky. ({tiedRibbons.Count}/5)";
        return "All apparatus is in place. Press CONFIRM SETUP.";
    }

    private static int ParseRibbonSlot(string zoneId, string itemId)
    {
        if (!string.IsNullOrEmpty(zoneId) && zoneId.StartsWith("RibbonSlot") &&
            int.TryParse(zoneId.Substring("RibbonSlot".Length), out int z))
            return z;
        if (!string.IsNullOrEmpty(itemId) && itemId.StartsWith("Ribbon") &&
            int.TryParse(itemId.Substring("Ribbon".Length), out int i))
            return i;
        if (zoneId == "SlinkyZone")
        {
            var asm = Instance;
            if (asm == null) return 0;
            for (int s = 0; s < 5; s++)
                if (!asm.tiedRibbons.Contains(s)) return s;
        }
        return -1;
    }
}
