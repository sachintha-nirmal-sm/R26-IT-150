using UnityEngine;

public class HeatAssemblyManager : MonoBehaviour
{
    public static HeatAssemblyManager Instance { get; private set; }

    private bool testTubePlaced;
    private bool waterFilled;
    private bool stopperPlaced;
    private bool thinTubePlaced;
    private bool levelAMarked;
    private bool tripodPlaced;
    private bool beakerPlaced;
    private bool burnerPlaced;
    private bool standPlaced;
    private bool setupConfirmed;
    private bool tubeScored, waterScored, stopperScored, thinScored, markScored;
    private bool tripodScored, beakerScored, burnerScored, standScored;

    public bool TestTubePlaced => testTubePlaced;
    public bool WaterFilled => waterFilled;
    public bool StopperPlaced => stopperPlaced;
    public bool ThinTubePlaced => thinTubePlaced;
    public bool LevelAMarked => levelAMarked;
    public bool TripodPlaced => tripodPlaced;
    public bool BeakerPlaced => beakerPlaced;
    public bool BurnerPlaced => burnerPlaced;
    public bool StandPlaced => standPlaced;
    public bool SetupConfirmed => setupConfirmed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ResetAssembly()
    {
        testTubePlaced = waterFilled = stopperPlaced = thinTubePlaced = levelAMarked = false;
        tripodPlaced = beakerPlaced = burnerPlaced = standPlaced = setupConfirmed = false;
        tubeScored = waterScored = stopperScored = thinScored = markScored = false;
        tripodScored = beakerScored = burnerScored = standScored = false;
        HeatVisualController.Instance?.ResetVisuals();
        HeatUIManager.Instance?.SetInstruction(NextHint());
    }

    public string SuggestedZone(string itemId)
    {
        if (itemId == "TestTube" && !testTubePlaced) return "TubeZone";
        if (itemId == "ColoredWater" && testTubePlaced && !waterFilled) return "TubeZone";
        if (itemId == "RubberStopper" && waterFilled && !stopperPlaced) return "TubeZone";
        if (itemId == "ThinGlassTube" && stopperPlaced && !thinTubePlaced) return "TubeZone";
        if (itemId == "TripodStand" && !tripodPlaced) return "BathZone";
        if (itemId == "Beaker" && tripodPlaced && !beakerPlaced) return "BathZone";
        if (itemId == "BunsenBurner" && beakerPlaced && !burnerPlaced) return "BathZone";
        if (itemId == "RetortStand" && thinTubePlaced && beakerPlaced && !standPlaced) return "StandZone";
        return null;
    }

    public bool TryPlace(string itemId, string zoneId)
    {
        if (itemId == "TestTube")
            return Place("TestTube", zoneId, "TubeZone|BenchZone",
                () => testTubePlaced,
                v => testTubePlaced = v,
                ref tubeScored,
                () => HeatVisualController.Instance?.ShowTestTube(true),
                "Place the TEST TUBE on the bench first.",
                "✓ CORRECT\nThe test tube is ready to be filled with coloured water.");

        if (itemId == "ColoredWater")
        {
            if (!testTubePlaced)
            {
                Fail("Place the test tube before pouring in the coloured water.");
                return false;
            }
            return Place("ColoredWater", zoneId, "TubeZone|BenchZone",
                () => waterFilled,
                v => waterFilled = v,
                ref waterScored,
                () => HeatVisualController.Instance?.ShowWater(true),
                "Pour the COLOURED WATER into the test tube.",
                "✓ CORRECT\nThe test tube is filled with coloured water.");
        }

        if (itemId == "RubberStopper")
        {
            if (!waterFilled)
            {
                Fail("Fill the test tube with coloured water before fitting the stopper.");
                return false;
            }
            return Place("RubberStopper", zoneId, "TubeZone|BenchZone",
                () => stopperPlaced,
                v => stopperPlaced = v,
                ref stopperScored,
                () => HeatVisualController.Instance?.ShowStopper(true),
                "Fit the RUBBER STOPPER into the mouth of the test tube.",
                "✓ CORRECT\nThe stopper seals the test tube.");
        }

        if (itemId == "ThinGlassTube")
        {
            if (!stopperPlaced)
            {
                Fail("Fit the rubber stopper first, then pass the thin glass tube through it.");
                return false;
            }
            return Place("ThinGlassTube", zoneId, "TubeZone|BenchZone",
                () => thinTubePlaced,
                v => thinTubePlaced = v,
                ref thinScored,
                () => HeatVisualController.Instance?.ShowThinTube(true),
                "Pass the THIN GLASS TUBE through the stopper so the liquid rises a little into it.",
                "✓ CORRECT\nThe liquid has risen a little into the thin tube. Mark this height as A.");
        }

        if (itemId == "TripodStand")
            return Place("TripodStand", zoneId, "BathZone|BenchZone",
                () => tripodPlaced,
                v => tripodPlaced = v,
                ref tripodScored,
                () => HeatVisualController.Instance?.ShowTripod(true),
                "Place the TRIPOD STAND over the burner position.",
                "✓ CORRECT\nThe tripod will support the beaker.");

        if (itemId == "Beaker")
        {
            if (!tripodPlaced)
            {
                Fail("Place the tripod stand first, then the beaker of water on it.");
                return false;
            }
            return Place("Beaker", zoneId, "BathZone|BenchZone",
                () => beakerPlaced,
                v => beakerPlaced = v,
                ref beakerScored,
                () => HeatVisualController.Instance?.ShowBeaker(true),
                "Place the BEAKER of water on the tripod.",
                "✓ CORRECT\nThe beaker of water is the heating bath.");
        }

        if (itemId == "BunsenBurner")
        {
            if (!beakerPlaced)
            {
                Fail("Place the beaker on the tripod before lighting the burner under it.");
                return false;
            }
            return Place("BunsenBurner", zoneId, "BathZone|BenchZone",
                () => burnerPlaced,
                v => burnerPlaced = v,
                ref burnerScored,
                () => HeatVisualController.Instance?.ShowBurner(true),
                "Place the BUNSEN BURNER under the tripod.",
                "✓ CORRECT\nThe burner is under the beaker, ready to heat the water bath.");
        }

        if (itemId == "RetortStand")
        {
            if (!thinTubePlaced || !beakerPlaced)
            {
                Fail("Finish the test-tube apparatus and the water bath, then clamp the tube in the beaker.");
                return false;
            }
            return Place("RetortStand", zoneId, "StandZone|TubeZone|BathZone|BenchZone",
                () => standPlaced,
                v => standPlaced = v,
                ref standScored,
                () => HeatVisualController.Instance?.ShowStand(true),
                "Clamp the TEST TUBE so its lower part sits in the beaker of water.",
                "✓ CORRECT\nThe test tube is held in the water bath.");
        }

        HeatScoreManager.Instance?.SubtractScore(5);
        HeatFeedbackManager.Instance?.ShowInstruction("That item does not belong in this setup.");
        return false;
    }

    public bool MarkLevelA()
    {
        if (!thinTubePlaced)
        {
            Fail("Fit the thin glass tube so the liquid rises into it, then mark level A.");
            return false;
        }
        if (levelAMarked) return true;
        levelAMarked = true;
        if (!markScored)
        {
            markScored = true;
            HeatScoreManager.Instance?.AddScore(5, false);
        }
        HeatVisualController.Instance?.ShowMarkA(true);
        HeatFeedbackManager.Instance?.ShowMessage(
            "✓ LEVEL A MARKED\nThis is the starting height of the liquid in the thin tube, before heating.",
            "+5 MARKS",
            new Color(0.08f, 0.52f, 0.22f));
        HeatUIManager.Instance?.SetInstruction(NextHint());
        HeatUIManager.Instance?.UpdateLiveReadings();
        return true;
    }

    public bool ConfirmSetup()
    {
        if (!testTubePlaced || !waterFilled || !stopperPlaced || !thinTubePlaced ||
            !levelAMarked || !tripodPlaced || !beakerPlaced || !burnerPlaced || !standPlaced)
        {
            HeatScoreManager.Instance?.SubtractScore(5);
            HeatFeedbackManager.Instance?.ShowMessage("✗ INCOMPLETE\n" + NextHint(), "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
            return false;
        }
        if (!setupConfirmed)
        {
            setupConfirmed = true;
            HeatScoreManager.Instance?.AddScore(5, false);
        }
        HeatFeedbackManager.Instance?.ShowMessage(
            "✓ SETUP COMPLETE\nNow heat the water bath. Watch the liquid in the thin tube.",
            "+5 MARKS",
            new Color(0.08f, 0.52f, 0.22f));
        return true;
    }

    public string NextHint()
    {
        if (!testTubePlaced) return "Place the TEST TUBE on the bench.";
        if (!waterFilled) return "Fill the test tube with COLOURED WATER.";
        if (!stopperPlaced) return "Fit the RUBBER STOPPER into the test tube.";
        if (!thinTubePlaced) return "Pass the THIN GLASS TUBE through the stopper. The liquid should rise a little into it.";
        if (!levelAMarked) return "Press MARK LEVEL A on the thin tube.";
        if (!tripodPlaced) return "Place the TRIPOD STAND for the water bath.";
        if (!beakerPlaced) return "Place the BEAKER of water on the tripod.";
        if (!burnerPlaced) return "Place the BUNSEN BURNER under the tripod.";
        if (!standPlaced) return "Use the RETORT STAND AND CLAMP to hold the test tube in the beaker.";
        return "All apparatus is in place. Press CONFIRM SETUP.";
    }

    private bool Place(string itemId, string zoneId, string allowedZones, System.Func<bool> get, System.Action<bool> set, ref bool scored,
        System.Action show, string wrongZone, string success)
    {
        if (get()) return true;
        if (!ZoneOk(zoneId, allowedZones))
        {
            Fail(wrongZone);
            return false;
        }
        set(true);
        if (!scored)
        {
            scored = true;
            HeatScoreManager.Instance?.AddScore(5, false);
        }
        show?.Invoke();
        HeatFeedbackManager.Instance?.ShowMessage(success, "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        HeatUIManager.Instance?.SetInstruction(NextHint());
        HeatUIManager.Instance?.UpdateLiveReadings();
        return true;
    }

    private static bool ZoneOk(string zoneId, string allowed)
    {
        var parts = allowed.Split('|');
        foreach (var p in parts)
            if (string.Equals(zoneId, p.Trim())) return true;
        return false;
    }

    private static void Fail(string message)
    {
        HeatScoreManager.Instance?.SubtractScore(5);
        HeatFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + message, "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
    }
}
