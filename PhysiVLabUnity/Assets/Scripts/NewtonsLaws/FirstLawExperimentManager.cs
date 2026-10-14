using UnityEngine;

public class FirstLawExperimentManager : MonoBehaviour
{
    public static FirstLawExperimentManager Instance { get; private set; }

    [SerializeField] private bool trackPlaced;
    [SerializeField] private bool rulerPlaced;
    [SerializeField] private bool trolleyPlaced;
    [SerializeField] private bool frictionSet;
    [SerializeField] private bool stationaryObserved;
    [SerializeField] private bool movingObserved;
    [SerializeField] private bool frictionCompared;
    [SerializeField] private bool observationScored;

    public bool TrackPlaced => trackPlaced;
    public bool RulerPlaced => rulerPlaced;
    public bool TrolleyPlaced => trolleyPlaced;
    public bool SetupComplete => trackPlaced && trolleyPlaced;
    public bool StationaryObserved => stationaryObserved;
    public bool MovingObserved => movingObserved;
    public bool FrictionCompared => frictionCompared;
    public bool ObservationComplete => observationScored;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartFirstLawExperiment()
    {
        FirstLawMotionController.Instance?.PrepareStationary();
        TrolleyController.Instance?.SetForce(0f);
        TrolleyController.Instance?.SetVelocity(0f);
        TrolleyController.Instance?.SetPosition(0f);
        TrolleyController.Instance?.StartMotion();
        TrolleyController.Instance?.PulseHighlight();
        NewtonUIManager.Instance?.UpdateLiveNewtonReadings(1f, 0f, 0f, 0f, 0f, 9.8f, true);
        var step = NewtonsLawsExperimentManager.Instance != null
            ? NewtonsLawsExperimentManager.Instance.CurrentStep
            : NewtonExperimentStep.FirstLawStationary;
        if (step == NewtonExperimentStep.FirstLawStationary)
        {
            NewtonUIManager.Instance?.ShowTrackResult(
                "RUNNING — no push\nTrolley stays at 0 m    Fnet = 0 N    v = 0 m/s",
                true);
            NewtonFeedbackManager.Instance?.ShowMessage(
                "START is working. There is no push, so the trolley stays at rest.\nNet force = 0 N. Velocity = 0 m/s.\nNow tap: Remains at rest.",
                "", new Color(0.08f, 0.52f, 0.22f));
            NewtonUIManager.Instance?.SetInstruction("Watch the stopwatch: the trolley does NOT move. That is the result. Then tap: Remains at rest.");
        }
        else if (step == NewtonExperimentStep.FirstLawFriction)
        {
            NewtonUIManager.Instance?.ShowTrackResult("Select LOW FRICTION or HIGH FRICTION, then press PUSH.", true);
            NewtonFeedbackManager.Instance?.ShowInstruction("Select LOW FRICTION or HIGH FRICTION, then press PUSH.");
        }
    }

    public void StopExperiment()
    {
        TrolleyController.Instance?.Stop();
    }

    public void ResetExperiment()
    {
        TrolleyController.Instance?.ResetTrolley();
        FirstLawMotionController.Instance?.PrepareStationary();
        frictionSet = false;
        stationaryObserved = false;
        movingObserved = false;
        frictionCompared = false;
        observationScored = false;
        NewtonFrictionController.Instance?.ResetFriction();
    }

    public void ResetKeepSetup()
    {
        TrolleyController.Instance?.ResetTrolley();
        FirstLawMotionController.Instance?.PrepareStationary();
    }

    public void NotifyTrackPlaced()
    {
        if (trackPlaced) return;
        trackPlaced = true;
        NewtonScoreManager.Instance?.AddScore(5, false);
        NewtonFeedbackManager.Instance?.ShowMessage("✓ Track correctly placed.", "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
        NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
    }

    public void NotifyRulerPlaced()
    {
        if (rulerPlaced) return;
        rulerPlaced = true;
        NewtonScoreManager.Instance?.AddScore(5, false);
        NewtonFeedbackManager.Instance?.ShowMessage("✓ Ruler placed beside the track.", "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
        NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
    }

    public void NotifyTrolleyPlaced(bool atStart)
    {
        if (!atStart)
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowMessage("✗ Place the trolley at 0 m.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
            return;
        }
        if (trolleyPlaced) return;
        trolleyPlaced = true;
        TrolleyController.Instance?.SetPosition(0f);
        NewtonScoreManager.Instance?.AddScore(5, false);
        NewtonFeedbackManager.Instance?.ShowMessage("✓ Trolley correctly positioned at 0 m.", "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
        NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
    }

    public void SetFriction(bool low)
    {
        NewtonFrictionController.Instance?.SetFriction(low);
        if (!frictionSet)
        {
            frictionSet = true;
            if (low)
            {
                NewtonScoreManager.Instance?.AddScore(5, false);
                NewtonFeedbackManager.Instance?.ShowMessage(
                    "✓ LOW FRICTION selected. Newton's First Law is observed most clearly when the resultant force is nearly zero.",
                    "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
            }
            else
            {
                NewtonScoreManager.Instance?.SubtractScore(5);
                NewtonFeedbackManager.Instance?.ShowMessage(
                    "✗ Select LOW FRICTION to see uniform motion clearly. High friction is an unbalanced force.",
                    "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
            }
        }
        frictionCompared = true;
        NewtonFeedbackManager.Instance?.ShowInstruction(
            low
                ? "Low friction: the trolley moves farther at nearly constant velocity."
                : "High friction: the trolley slows down faster because friction is an external force.");
    }

    public void ApplyInitialPush()
    {
        if (!SetupComplete)
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowInstruction("Place the track and trolley before giving a push.");
            return;
        }
        FirstLawMotionController.Instance?.ApplyInitialPush();
        NewtonUIManager.Instance?.ShowTrackResult(
            "PUSH given — then F = 0 N. On a low-friction track the trolley keeps moving.",
            true);
        NewtonFeedbackManager.Instance?.ShowMessage(
            "Applied force after the push: 0 N. Net force is approximately 0 N on a low-friction track. The trolley continues moving.",
            "", new Color(0.12f, 0.32f, 0.62f));
    }

    public float GetNetForce() => FirstLawMotionController.Instance != null ? FirstLawMotionController.Instance.GetNetForce() : 0f;
    public float GetVelocity() => FirstLawMotionController.Instance != null ? FirstLawMotionController.Instance.GetVelocity() : 0f;

    public void RecordStationaryObservation(bool remainsAtRest)
    {
        if (stationaryObserved) return;
        if (remainsAtRest)
        {
            stationaryObserved = true;
            NewtonDataManager.Instance.StationaryObservation = "Remains at rest";
            NewtonUIManager.Instance?.ShowTrackResult("RESULT: trolley remains at rest. Fnet = 0 N.  Tap NEXT STEP.", true);
            NewtonScoreManager.Instance?.AddScore(5, false);
            NewtonFeedbackManager.Instance?.ShowMessage("✓ The stationary trolley remains at rest. Net force = 0 N. Velocity = 0 m/s.", "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowMessage("✗ With no unbalanced force a stationary body remains at rest.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
        }
        NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
    }

    public void RecordMovingObservation(bool uniformVelocity)
    {
        if (movingObserved) return;
        if (uniformVelocity)
        {
            movingObserved = true;
            NewtonDataManager.Instance.MovingObservation = "Continues moving with uniform velocity";
            NewtonScoreManager.Instance?.AddScore(5, false);
            NewtonFeedbackManager.Instance?.ShowMessage(
                "✓ After the push, with net force ≈ 0 N, the trolley continues with uniform velocity.",
                "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowMessage("✗ A moving body continues with uniform velocity when the resultant force is zero.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
        }
        NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
    }

    public void ConfirmExplanation()
    {
        if (observationScored) return;
        if (!stationaryObserved || !movingObserved)
        {
            NewtonFeedbackManager.Instance?.ShowInstruction("Record both observations first.");
            return;
        }
        observationScored = true;
        NewtonDataManager.Instance.FirstLawComplete = true;
        NewtonScoreManager.Instance?.AddScore(5, false);
        NewtonFeedbackManager.Instance?.ShowMessage(
            "✓ Until an unbalanced force acts, a stationary body remains at rest and a moving body continues at uniform velocity.",
            "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
        NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
    }

    public void FullReset()
    {
        trackPlaced = rulerPlaced = trolleyPlaced = false;
        ResetExperiment();
    }
}
