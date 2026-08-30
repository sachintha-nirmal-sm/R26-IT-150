using System.Collections.Generic;
using UnityEngine;

public class FrictionTrialManager : MonoBehaviour
{
    public static FrictionTrialManager Instance { get; private set; }

    [SerializeField] private int currentTrial = 1;
    [SerializeField] private int expectedSurface;
    [SerializeField] private bool sandpaperPlaced;
    [SerializeField] private bool blockPlaced;
    [SerializeField] private bool balanceAttached;
    [SerializeField] private bool setupConfirmed;
    [SerializeField] private bool orientationScored;
    [SerializeField] private bool sandpaperScored;
    [SerializeField] private bool balanceScored;
    [SerializeField] private bool confirmScored;

    private readonly List<FrictionTrialData> trials = new List<FrictionTrialData>();

    public int CurrentTrial => currentTrial;
    public int ExpectedSurface => expectedSurface;
    public bool SetupComplete => sandpaperPlaced && blockPlaced && balanceAttached && setupConfirmed && IsCorrectOrientation();
    public IReadOnlyList<FrictionTrialData> Trials => trials;
    public int CompletedCount
    {
        get
        {
            int n = 0;
            foreach (var t in trials) if (t != null && t.completed) n++;
            return n;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        EnsureTrials();
    }

    public void Configure()
    {
        currentTrial = 1;
        EnsureTrials();
        BeginTrial(1);
    }

    public void ResetAllTrials()
    {
        trials.Clear();
        EnsureTrials();
        BeginTrial(1);
    }

    public void BeginTrial(int trial)
    {
        currentTrial = Mathf.Clamp(trial, 1, 3);
        expectedSurface = currentTrial - 1;
        sandpaperPlaced = false;
        blockPlaced = false;
        balanceAttached = false;
        setupConfirmed = false;
        orientationScored = false;
        sandpaperScored = false;
        balanceScored = false;
        confirmScored = false;
        FrictionForceController.Instance?.PrepareTrial(currentTrial);
        FrictionMeasurementManager.Instance?.ResetTrialSamples();
        SandpaperController.Instance?.ResetPlacement();
        NewtonBalanceController.Instance?.Detach();
        WoodenBlockController.Instance?.ResetPosition();
        WoodenBlockController.Instance?.RotateToSurface(expectedSurface);
        FrictionAppliedForceController.Instance?.ResetForce();
        PullController.Instance?.ResetPull();
        FrictionUIManager.Instance?.RefreshTrialLabels();
    }

    public bool IsCorrectOrientation()
    {
        return WoodenBlockController.Instance != null &&
               WoodenBlockController.Instance.SurfaceIndex == expectedSurface;
    }

    public void NotifySandpaperPlaced()
    {
        sandpaperPlaced = true;
        if (!sandpaperScored)
        {
            sandpaperScored = true;
            FrictionScoreManager.Instance?.AddScore(5, false);
            FrictionFeedbackManager.Instance?.ShowMessage("✓ CORRECT SETUP\nSandpaper placed under the block. Roughness is CONSTANT.", "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
    }

    public void NotifyBlockPlaced()
    {
        blockPlaced = true;
        if (IsCorrectOrientation())
        {
            if (!orientationScored)
            {
                orientationScored = true;
                FrictionScoreManager.Instance?.AddScore(5, false);
                string name = WoodenBlockController.Instance.GetSurfaceName();
                FrictionFeedbackManager.Instance?.ShowMessage($"✓ CORRECT SETUP\nYou placed surface {name} on the sandpaper.", "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
            }
        }
        else
        {
            FrictionScoreManager.Instance?.SubtractScore(5);
            FrictionFeedbackManager.Instance?.ShowMessage("✗ INCORRECT SETUP\nRotate the block so the required surface is in contact with the table.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
    }

    public void NotifyBalanceAttached()
    {
        balanceAttached = true;
        NewtonBalanceController.Instance?.Attach();
        if (!balanceScored)
        {
            balanceScored = true;
            FrictionScoreManager.Instance?.AddScore(5, false);
            FrictionFeedbackManager.Instance?.ShowMessage("✓ CORRECT SETUP\nNewton balance attached to the wooden block.", "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
    }

    public void NotifyWrongPlacement(string reason)
    {
        FrictionScoreManager.Instance?.SubtractScore(5);
        FrictionFeedbackManager.Instance?.ShowMessage("✗ INCORRECT SETUP\n" + reason, "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
    }

    public bool ConfirmSetup()
    {
        if (!sandpaperPlaced || !blockPlaced || !balanceAttached)
        {
            FrictionScoreManager.Instance?.SubtractScore(5);
            FrictionFeedbackManager.Instance?.ShowInstruction("Complete the setup: sandpaper, block on the correct surface, and Newton balance.");
            return false;
        }
        if (!IsCorrectOrientation())
        {
            FrictionScoreManager.Instance?.SubtractScore(5);
            FrictionFeedbackManager.Instance?.ShowInstruction("The block is on the wrong surface for this trial.");
            return false;
        }
        setupConfirmed = true;
        if (!confirmScored)
        {
            confirmScored = true;
            FrictionScoreManager.Instance?.AddScore(5, false);
            FrictionFeedbackManager.Instance?.ShowMessage("✓ CORRECT SETUP\nYou may now pull the block slowly.", "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
        return true;
    }

    public void RecordCurrentReading(float value)
    {
        EnsureTrials();
        var data = trials[currentTrial - 1];
        data.trialNumber = currentTrial;
        data.surfaceName = WoodenBlockController.Instance != null ? WoodenBlockController.Instance.GetSurfaceName() : "?";
        data.contactArea = WoodenBlockController.Instance != null ? WoodenBlockController.Instance.GetContactArea() : 0f;
        data.weight = 60f;
        data.sandpaperRoughness = SandpaperController.Roughness;
        data.limitingFriction = value;
        data.completed = true;
        if (data.surfaceName == "A") { data.length = 30f; data.width = 20f; }
        else if (data.surfaceName == "B") { data.length = 30f; data.width = 10f; }
        else { data.length = 20f; data.width = 10f; }
    }

    public FrictionTrialData GetTrial(int number)
    {
        EnsureTrials();
        if (number < 1 || number > 3) return null;
        return trials[number - 1];
    }

    public List<FrictionTrialData> GetAllTrials()
    {
        EnsureTrials();
        return trials;
    }

    public bool AllTrialsComplete()
    {
        EnsureTrials();
        foreach (var t in trials) if (t == null || !t.completed) return false;
        return true;
    }

    public void ResetCurrentTrialKeepData()
    {
        BeginTrial(currentTrial);
    }

    private void EnsureTrials()
    {
        while (trials.Count < 3)
        {
            int n = trials.Count + 1;
            string name = n == 1 ? "A" : n == 2 ? "B" : "C";
            float area = n == 1 ? 600f : n == 2 ? 300f : 200f;
            float len = n == 3 ? 20f : 30f;
            float wid = n == 1 ? 20f : 10f;
            trials.Add(new FrictionTrialData
            {
                trialNumber = n,
                surfaceName = name,
                length = len,
                width = wid,
                contactArea = area,
                weight = 60f,
                sandpaperRoughness = 1f
            });
        }
    }
}
