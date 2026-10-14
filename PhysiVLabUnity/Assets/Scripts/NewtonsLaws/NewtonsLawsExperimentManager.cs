using System.Globalization;
using UnityEngine;

public class NewtonsLawsExperimentManager : MonoBehaviour
{
    public static NewtonsLawsExperimentManager Instance { get; private set; }

    [SerializeField] private NewtonExperimentStep currentStep = NewtonExperimentStep.Introduction;
    [SerializeField] private int totalDisplaySteps = 21;
    [SerializeField] private int mistakeCount;
    private bool flutterSent;
    [SerializeField] private int correctScore = 5;
    [SerializeField] private int wrongPenalty = 5;
    [SerializeField] private int majorStepScore = 10;
    [SerializeField] private int maximumAttempts = 3;
    [SerializeField] private float measurementTolerance = 0.05f;
    [SerializeField] private float gravitationalAcceleration = 9.8f;
    [SerializeField] private float maximumForce = 10f;
    [SerializeField] private float minimumMass = 0.5f;
    [SerializeField] private float maximumMass = 5f;

    public NewtonExperimentStep CurrentStep => currentStep;
    public int MistakeCount => mistakeCount;
    public int TotalDisplaySteps => totalDisplaySteps;
    public float MeasurementTolerance => measurementTolerance;
    public float GravitationalAcceleration => gravitationalAcceleration;

    private bool conclusionScored;

    private void Awake() => Instance = this;

    private void Start() => ApplyInspectorSettings();

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void ApplyInspectorSettings()
    {
        NewtonScoreManager.Instance?.Configure(correctScore, wrongPenalty, majorStepScore);
        NewtonAttemptManager.Instance?.Configure(maximumAttempts);
        NewtonForceCalculator.Instance?.Configure(gravitationalAcceleration);
        NewtonForceController.Instance?.Configure(maximumForce);
        NewtonMassController.Instance?.Configure(minimumMass, maximumMass);
        TrolleyController.Instance?.ConfigureLimits(0f, 5f);
    }

    public void RegisterMistake() => mistakeCount++;

    public void StartPractical()
    {
        mistakeCount = 0;
        flutterSent = false;
        conclusionScored = false;
        NewtonScoreManager.Instance?.ResetScore();
        NewtonDataManager.Instance?.ResetReadings();
        NewtonEquipmentSelectionManager.Instance?.EnsureCardsVisible();
        NewtonEquipmentSelectionManager.Instance?.ResetSelection();
        FirstLawExperimentManager.Instance?.FullReset();
        SecondLawExperimentManager.Instance?.FullReset();
        ThirdLawExperimentManager.Instance?.FullReset();
        WeightExperimentManager.Instance?.ResetExperiment();
        NewtonObservationTableManager.Instance?.ResetBonus();
        NewtonGraphController.Instance?.ResetBonus();
        ApplyInspectorSettings();
        int required = NewtonEquipmentSelectionManager.Instance != null ? NewtonEquipmentSelectionManager.Instance.RequiredCount : 12;
        NewtonScoreManager.Instance?.ConfigureMaxRaw(required);
        currentStep = NewtonExperimentStep.Objective;
        NewtonUIManager.Instance?.HideResult();
        NewtonUIManager.Instance?.SetNextButtonVisible(true);
        NewtonUIManager.Instance?.UpdateAttemptsDisplay(NewtonAttemptManager.Instance != null ? NewtonAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateUI();
    }

    public void SetStep(NewtonExperimentStep step)
    {
        currentStep = step;
        EnterStep();
        UpdateUI();
        if (currentStep == NewtonExperimentStep.Complete)
            CompleteExperiment();
    }

    public void AdvanceStep()
    {
        if (currentStep >= NewtonExperimentStep.Complete) return;
        if (!CanLeaveCurrentStep()) return;
        currentStep++;
        EnterStep();
        UpdateUI();
        if (currentStep == NewtonExperimentStep.Complete)
            CompleteExperiment();
    }

    public void TryAdvanceFromEquipment()
    {
        if (currentStep == NewtonExperimentStep.Introduction || currentStep == NewtonExperimentStep.Objective)
            currentStep = NewtonExperimentStep.SelectEquipment;
        if (currentStep != NewtonExperimentStep.SelectEquipment) return;
        if (NewtonEquipmentSelectionManager.Instance != null && NewtonEquipmentSelectionManager.Instance.IsCompleteCheck())
        {
            NewtonScoreManager.Instance?.AddScore(5, false);
            AdvanceStep();
        }
        else
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowInstruction("Select all required equipment before continuing.");
        }
    }

    public void NotifySetupChanged()
    {
        switch (currentStep)
        {
            case NewtonExperimentStep.FirstLawSetup:
                if (FirstLawExperimentManager.Instance != null && FirstLawExperimentManager.Instance.SetupComplete)
                    NewtonUIManager.Instance?.SetNextButtonVisible(true);
                break;
            case NewtonExperimentStep.FirstLawStationary:
                if (FirstLawExperimentManager.Instance != null && FirstLawExperimentManager.Instance.StationaryObserved)
                    NewtonUIManager.Instance?.SetNextButtonVisible(true);
                break;
            case NewtonExperimentStep.FirstLawMoving:
                if (FirstLawExperimentManager.Instance != null && FirstLawExperimentManager.Instance.MovingObserved)
                    NewtonUIManager.Instance?.SetNextButtonVisible(true);
                break;
            case NewtonExperimentStep.FirstLawFriction:
            case NewtonExperimentStep.FirstLawObservation:
                if (FirstLawExperimentManager.Instance != null && FirstLawExperimentManager.Instance.ObservationComplete)
                    NewtonUIManager.Instance?.SetNextButtonVisible(true);
                break;
            case NewtonExperimentStep.SecondLawSetup:
                if (SecondLawExperimentManager.Instance != null && SecondLawExperimentManager.Instance.SetupComplete)
                    NewtonUIManager.Instance?.SetNextButtonVisible(true);
                break;
            case NewtonExperimentStep.SecondLawConstantMass:
                if (SecondLawExperimentManager.Instance != null && SecondLawExperimentManager.Instance.HasForceSeries)
                    NewtonUIManager.Instance?.SetNextButtonVisible(true);
                break;
            case NewtonExperimentStep.SecondLawConstantForce:
                if (SecondLawExperimentManager.Instance != null && SecondLawExperimentManager.Instance.HasMassSeries)
                {
                    if (NewtonDataManager.Instance != null) NewtonDataManager.Instance.SecondLawComplete = true;
                    NewtonUIManager.Instance?.SetNextButtonVisible(true);
                }
                break;
            case NewtonExperimentStep.ThirdLawSetup:
                if (ThirdLawExperimentManager.Instance != null && ThirdLawExperimentManager.Instance.SetupComplete)
                    NewtonUIManager.Instance?.SetNextButtonVisible(true);
                break;
            case NewtonExperimentStep.ThirdLawExperiment:
                if (ThirdLawExperimentManager.Instance != null && ThirdLawExperimentManager.Instance.ExperimentComplete)
                    NewtonUIManager.Instance?.SetNextButtonVisible(true);
                break;
            case NewtonExperimentStep.ThirdLawObservation:
                if (ThirdLawExperimentManager.Instance != null && ThirdLawExperimentManager.Instance.ObservationComplete)
                    NewtonUIManager.Instance?.SetNextButtonVisible(true);
                break;
            case NewtonExperimentStep.WeightExperiment:
                if (WeightExperimentManager.Instance != null && WeightExperimentManager.Instance.HasMeasurement)
                    NewtonUIManager.Instance?.SetNextButtonVisible(true);
                break;
        }
    }

    public bool CanLeaveCurrentStep()
    {
        switch (currentStep)
        {
            case NewtonExperimentStep.SelectEquipment:
                if (NewtonEquipmentSelectionManager.Instance == null || !NewtonEquipmentSelectionManager.Instance.IsCompleteCheck())
                {
                    NewtonScoreManager.Instance?.SubtractScore(5);
                    NewtonFeedbackManager.Instance?.ShowInstruction("Select all required equipment first.");
                    return false;
                }
                return true;
            case NewtonExperimentStep.FirstLawSetup:
                if (FirstLawExperimentManager.Instance == null || !FirstLawExperimentManager.Instance.SetupComplete)
                {
                    NewtonScoreManager.Instance?.SubtractScore(5);
                    NewtonFeedbackManager.Instance?.ShowInstruction("Place the track and trolley at 0 m.");
                    return false;
                }
                return true;
            case NewtonExperimentStep.SecondLawSetup:
                if (SecondLawExperimentManager.Instance == null || !SecondLawExperimentManager.Instance.SetupComplete)
                {
                    SecondLawExperimentManager.Instance?.MarkIncorrectSetup();
                    return false;
                }
                return true;
            case NewtonExperimentStep.SecondLawConstantMass:
                if (SecondLawExperimentManager.Instance == null || !SecondLawExperimentManager.Instance.HasForceSeries)
                {
                    NewtonScoreManager.Instance?.SubtractScore(5);
                    NewtonFeedbackManager.Instance?.ShowInstruction("Record at least three constant-mass trials.");
                    return false;
                }
                return true;
            case NewtonExperimentStep.SecondLawConstantForce:
                if (SecondLawExperimentManager.Instance == null || !SecondLawExperimentManager.Instance.HasMassSeries)
                {
                    NewtonScoreManager.Instance?.SubtractScore(5);
                    NewtonFeedbackManager.Instance?.ShowInstruction("Record at least three constant-force trials.");
                    return false;
                }
                return true;
            case NewtonExperimentStep.FirstLawObservation:
                if (FirstLawExperimentManager.Instance == null || !FirstLawExperimentManager.Instance.ObservationComplete)
                {
                    NewtonFeedbackManager.Instance?.ShowInstruction("Record both observations and confirm the explanation.");
                    return false;
                }
                return true;
            case NewtonExperimentStep.ThirdLawSetup:
                if (ThirdLawExperimentManager.Instance == null || !ThirdLawExperimentManager.Instance.SetupComplete)
                {
                    NewtonScoreManager.Instance?.SubtractScore(5);
                    NewtonFeedbackManager.Instance?.ShowInstruction("Place string, straw and balloon in order.");
                    return false;
                }
                return true;
            case NewtonExperimentStep.ThirdLawObservation:
                if (ThirdLawExperimentManager.Instance == null || !ThirdLawExperimentManager.Instance.ObservationComplete)
                {
                    NewtonFeedbackManager.Instance?.ShowInstruction("Answer both action-reaction questions first.");
                    return false;
                }
                return true;
            case NewtonExperimentStep.WeightExperiment:
                if (WeightExperimentManager.Instance == null || !WeightExperimentManager.Instance.HasMeasurement)
                {
                    NewtonFeedbackManager.Instance?.ShowInstruction("Hang an object, read the spring balance and record the weight.");
                    return false;
                }
                return true;
            case NewtonExperimentStep.ConceptMatching:
                if (ConceptMatchingManager.Instance == null || !ConceptMatchingManager.Instance.IsComplete)
                {
                    NewtonFeedbackManager.Instance?.ShowInstruction("Match all four concepts first.");
                    return false;
                }
                return true;
            case NewtonExperimentStep.Questions:
                NewtonFeedbackManager.Instance?.ShowInstruction("Answer the current question first.");
                return false;
            default:
                return true;
        }
    }

    private void EnterStep()
    {
        FirstLawExperimentManager.Instance?.StopExperiment();
        SecondLawExperimentManager.Instance?.StopExperiment();
        switch (currentStep)
        {
            case NewtonExperimentStep.FirstLawSetup:
            case NewtonExperimentStep.FirstLawStationary:
                FirstLawExperimentManager.Instance?.StartFirstLawExperiment();
                break;
            case NewtonExperimentStep.FirstLawMoving:
                break;
            case NewtonExperimentStep.SecondLawSetup:
            case NewtonExperimentStep.SecondLawConstantMass:
                SecondLawExperimentManager.Instance?.SetConstantMassMode(true);
                break;
            case NewtonExperimentStep.SecondLawConstantForce:
                SecondLawExperimentManager.Instance?.SetConstantMassMode(false);
                break;
            case NewtonExperimentStep.SecondLawGraphs:
                NewtonGraphController.Instance?.ShowGraphs();
                break;
            case NewtonExperimentStep.ThirdLawSetup:
                ThirdLawExperimentManager.Instance?.ResetExperiment();
                break;
            case NewtonExperimentStep.WeightExperiment:
                TrolleyController.Instance?.Stop();
                TrolleyController.Instance?.ResetTrolley();
                WeightExperimentManager.Instance?.ResetExperiment();
                break;
            case NewtonExperimentStep.ObservationTables:
                NewtonObservationTableManager.Instance?.Refresh();
                break;
            case NewtonExperimentStep.Questions:
                NewtonQuestionManager.Instance?.StartQuiz();
                break;
            case NewtonExperimentStep.ConceptMatching:
                ConceptMatchingManager.Instance?.StartMatching();
                break;
            case NewtonExperimentStep.Conclusion:
                if (!conclusionScored)
                {
                    conclusionScored = true;
                    NewtonScoreManager.Instance?.AddScore(5, false);
                }
                break;
        }
    }

    public void ResetCurrentActivity()
    {
        switch (currentStep)
        {
            case NewtonExperimentStep.FirstLawSetup:
            case NewtonExperimentStep.FirstLawStationary:
            case NewtonExperimentStep.FirstLawMoving:
            case NewtonExperimentStep.FirstLawFriction:
            case NewtonExperimentStep.FirstLawObservation:
                FirstLawExperimentManager.Instance?.ResetKeepSetup();
                break;
            case NewtonExperimentStep.SecondLawSetup:
            case NewtonExperimentStep.SecondLawConstantMass:
            case NewtonExperimentStep.SecondLawConstantForce:
                SecondLawExperimentManager.Instance?.ResetExperiment();
                break;
            case NewtonExperimentStep.ThirdLawSetup:
            case NewtonExperimentStep.ThirdLawExperiment:
                ThirdLawExperimentManager.Instance?.ResetExperiment();
                break;
            case NewtonExperimentStep.WeightExperiment:
                WeightExperimentManager.Instance?.ResetExperiment();
                break;
        }
        NewtonFeedbackManager.Instance?.ShowInstruction("Current activity reset. Completed trial data and score are kept.");
        UpdateUI();
    }

    public void ResetExperiment()
    {
        StartPractical();
        currentStep = NewtonExperimentStep.Introduction;
        UpdateUI();
        NewtonUIManager.Instance?.ShowIntro();
    }

    public void RetryExperiment()
    {
        if (NewtonAttemptManager.Instance != null && !NewtonAttemptManager.Instance.CanRetry())
        {
            NewtonFeedbackManager.Instance?.ShowInstruction("Maximum of 3 attempts reached. Your best score is saved.");
            return;
        }
        mistakeCount = 0;
        flutterSent = false;
        conclusionScored = false;
        NewtonScoreManager.Instance?.ResetScore();
        NewtonDataManager.Instance?.ResetReadings();
        FirstLawExperimentManager.Instance?.FullReset();
        SecondLawExperimentManager.Instance?.FullReset();
        ThirdLawExperimentManager.Instance?.FullReset();
        WeightExperimentManager.Instance?.ResetExperiment();
        NewtonEquipmentSelectionManager.Instance?.ResetSelection();
        NewtonEquipmentSnapController.Instance?.ResetVisuals();
        currentStep = NewtonExperimentStep.Objective;
        NewtonUIManager.Instance?.HideResult();
        NewtonUIManager.Instance?.SetNextButtonVisible(true);
        UpdateUI();
    }

    public void CompleteExperiment()
    {
        int score = NewtonScoreManager.Instance != null ? NewtonScoreManager.Instance.FinalizeScore() : 0;
        bool passed = score >= 50;
        var attempt = NewtonAttemptManager.Instance != null
            ? NewtonAttemptManager.Instance.RegisterAttempt(score, mistakeCount, NewtonDataManager.Instance != null ? NewtonDataManager.Instance.AllTrials() : null, passed ? "Completed" : "Needs Improvement")
            : null;
        NewtonProfileManager.Instance?.UpdatePracticalResult(score, mistakeCount, passed, attempt);
        NewtonUIManager.Instance?.ShowResult(score, passed, mistakeCount, attempt);
        SendToFlutter(score, passed);
    }

    private void SendToFlutter(int score, bool passed)
    {
        if (flutterSent)
        {
            return;
        }

        flutterSent = true;
        TimerManager.Instance?.Stop();
        int timeUsed = TimerManager.Instance != null ? TimerManager.Instance.TimeUsedSeconds : 0;
        int trials = 0;
        if (NewtonDataManager.Instance != null)
        {
            var list = NewtonDataManager.Instance.AllTrials();
            trials = list != null ? list.Count : 0;
        }

        string measurements =
            "{\"trials\":" + trials.ToString(CultureInfo.InvariantCulture)
            + ",\"mistakes\":" + mistakeCount.ToString(CultureInfo.InvariantCulture)
            + "}";
        FlutterBridge.EnsureInstance()?.NotifyCompleted(
            score,
            passed,
            mistakeCount,
            timeUsed,
            true,
            measurements);
    }

    public void UpdateUI()
    {
        NewtonUIManager.Instance?.ShowStep(currentStep);
        NewtonUIManager.Instance?.UpdateProgress((int)currentStep + 1, totalDisplaySteps);
        NewtonUIManager.Instance?.UpdateScoreDisplay(NewtonScoreManager.Instance != null ? NewtonScoreManager.Instance.GetScore() : 0);
    }
}
