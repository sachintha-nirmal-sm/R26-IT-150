using UnityEngine;

public class ThirdLawExperimentManager : MonoBehaviour
{
    public static ThirdLawExperimentManager Instance { get; private set; }

    [SerializeField] private bool stringPlaced;
    [SerializeField] private bool strawPlaced;
    [SerializeField] private bool balloonAttached;
    [SerializeField] private bool inflated;
    [SerializeField] private bool released;
    [SerializeField] private bool observed;
    [SerializeField] private bool q1Done;
    [SerializeField] private bool q2Done;

    public bool StringReady => stringPlaced;
    public bool StrawReady => strawPlaced;
    public bool BalloonReady => balloonAttached;
    public bool SetupComplete => stringPlaced && strawPlaced && balloonAttached;
    public bool ExperimentComplete => inflated && released && observed;
    public bool ObservationComplete => q1Done && q2Done;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void PlaceString()
    {
        if (stringPlaced) return;
        stringPlaced = true;
        ScoreStep("✓ String placed between the two fixed points.");
        NewtonUIManager.Instance?.SetThirdLawVisual("string", true);
        NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
    }

    public void PlaceStraw()
    {
        if (!stringPlaced)
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowMessage("✗ Place the string first.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
            return;
        }
        if (strawPlaced) return;
        strawPlaced = true;
        ScoreStep("✓ Straw attached to the string.");
        NewtonUIManager.Instance?.SetThirdLawVisual("straw", true);
        NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
    }

    public void AttachBalloon()
    {
        if (!strawPlaced)
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowMessage("✗ Attach the straw before the balloon.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
            return;
        }
        if (balloonAttached) return;
        balloonAttached = true;
        BalloonController.Instance?.PrepareBalloon();
        ScoreStep("✓ Balloon attached to the straw.");
        NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
    }

    public void PrepareBalloon() => AttachBalloon();

    public void InflateBalloon()
    {
        if (!balloonAttached)
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowInstruction("Attach the balloon first.");
            return;
        }
        if (inflated) return;
        inflated = true;
        BalloonController.Instance?.InflateBalloon();
        ScoreStep("✓ Balloon inflated. Hold it, then release.");
        NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
    }

    public void ReleaseBalloon()
    {
        if (!inflated)
        {
            BalloonController.Instance?.ReleaseBalloon();
            return;
        }
        if (released) return;
        released = true;
        BalloonController.Instance?.ReleaseBalloon();
        CalculateActionReaction();
        ScoreStep("✓ Balloon released. Air moves backward. Balloon moves forward.");
        NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
    }

    public void CalculateActionReaction() => ActionReactionController.Instance?.CalculateActionReaction();
    public void ShowForceArrows() => ActionReactionController.Instance?.ShowForceArrows();

    public void NotifyFlightComplete()
    {
        observed = true;
        NewtonDataManager.Instance.ThirdLawComplete = true;
        NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
    }

    public void AnswerForwardQuestion(int index)
    {
        if (q1Done) return;
        bool ok = index == 0;
        q1Done = true;
        if (ok)
        {
            NewtonScoreManager.Instance?.AddScore(5, false);
            NewtonFeedbackManager.Instance?.ShowMessage("✓ The balloon moves forward when air moves backward.", "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowMessage("✗ The balloon moves forward — opposite to the escaping air.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
        }
        NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
    }

    public void AnswerLawQuestion(int index)
    {
        if (q2Done) return;
        bool ok = index == 2;
        q2Done = true;
        if (ok)
        {
            NewtonScoreManager.Instance?.AddScore(5, false);
            NewtonFeedbackManager.Instance?.ShowMessage("✓ Newton's Third Law: for every action there is an equal and opposite reaction.", "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowMessage("✗ This is Newton's Third Law.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
        }
        NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
    }

    public void ResetExperiment()
    {
        inflated = false;
        released = false;
        observed = false;
        BalloonController.Instance?.ResetBalloon();
        ActionReactionController.Instance?.Hide();
    }

    public void FullReset()
    {
        stringPlaced = strawPlaced = balloonAttached = false;
        q1Done = q2Done = false;
        ResetExperiment();
        NewtonUIManager.Instance?.SetThirdLawVisual("string", false);
        NewtonUIManager.Instance?.SetThirdLawVisual("straw", false);
    }

    private static void ScoreStep(string message)
    {
        NewtonScoreManager.Instance?.AddScore(5, false);
        NewtonFeedbackManager.Instance?.ShowMessage(message, "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
    }
}
