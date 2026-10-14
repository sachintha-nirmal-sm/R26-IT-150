using System.Globalization;
using UnityEngine;

public class FrictionExperimentManager : MonoBehaviour
{
    public static FrictionExperimentManager Instance { get; private set; }

    [SerializeField] private FrictionExperimentStep currentStep = FrictionExperimentStep.Introduction;
    [SerializeField] private int totalDisplaySteps = 12;
    [SerializeField] private int mistakeCount;
    private bool flutterSent;
    [SerializeField] private int correctScore = 5;
    [SerializeField] private int wrongPenalty = 5;
    [SerializeField] private int majorStepScore = 10;
    [SerializeField] private int maximumAttempts = 3;
    [SerializeField] private bool compareScored;
    [SerializeField] private bool completionScored;

    public FrictionExperimentStep CurrentStep => currentStep;
    public int MistakeCount => mistakeCount;
    public int TotalDisplaySteps => totalDisplaySteps;

    private void Awake() => Instance = this;

    private void Start() => ApplyInspectorSettings();

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void ApplyInspectorSettings()
    {
        FrictionScoreManager.Instance?.Configure(correctScore, wrongPenalty, majorStepScore);
        FrictionAttemptManager.Instance?.Configure(maximumAttempts);
        int required = FrictionEquipmentSelectionManager.Instance != null ? FrictionEquipmentSelectionManager.Instance.RequiredCount : 7;
        FrictionScoreManager.Instance?.ConfigureMaxRaw(required);
    }

    public void RegisterMistake() => mistakeCount++;

    public void StartPractical()
    {
        mistakeCount = 0;
        flutterSent = false;
        compareScored = false;
        completionScored = false;
        FrictionScoreManager.Instance?.ResetScore();
        FrictionEquipmentSelectionManager.Instance?.EnsureCardsVisible();
        FrictionEquipmentSelectionManager.Instance?.ResetSelection();
        FrictionTrialManager.Instance?.ResetAllTrials();
        FrictionObservationTableManager.Instance?.ResetScoring();
        FrictionGraphController.Instance?.ResetScoring();
        ConclusionManager.Instance?.ResetBuilder();
        VariableMatchingManager.Instance?.ResetMatching();
        ApplyInspectorSettings();
        currentStep = FrictionExperimentStep.Objective;
        FrictionUIManager.Instance?.HideResult();
        FrictionUIManager.Instance?.SetNextButtonVisible(true);
        FrictionUIManager.Instance?.UpdateAttemptsDisplay(FrictionAttemptManager.Instance != null ? FrictionAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateUI();
    }

    public void SetStep(FrictionExperimentStep step)
    {
        currentStep = step;
        EnterStep();
        UpdateUI();
        if (currentStep == FrictionExperimentStep.Complete)
            CompletePractical();
    }

    public void AdvanceStep()
    {
        if (currentStep >= FrictionExperimentStep.Complete) return;
        if (!CanLeaveCurrentStep()) return;

        if (currentStep == FrictionExperimentStep.Pulling)
        {
            if (!FrictionTrialManager.Instance.AllTrialsComplete())
            {
                int next = FrictionTrialManager.Instance.CurrentTrial + 1;
                StartTrial(next);
                currentStep = FrictionExperimentStep.Setup;
                EnterStep();
                UpdateUI();
                return;
            }
        }

        currentStep++;
        EnterStep();
        UpdateUI();
        if (currentStep == FrictionExperimentStep.Complete)
            CompletePractical();
    }

    public void CompleteQuestions()
    {
        currentStep = FrictionExperimentStep.VariableMatching;
        EnterStep();
        UpdateUI();
    }

    public void CompleteVariableMatching()
    {
        currentStep = FrictionExperimentStep.Conclusion;
        EnterStep();
        UpdateUI();
    }

    public void CompleteConclusion()
    {
        currentStep = FrictionExperimentStep.Complete;
        EnterStep();
        UpdateUI();
        CompletePractical();
    }

    public void TryAdvanceFromEquipment()
    {
        if (currentStep == FrictionExperimentStep.Introduction || currentStep == FrictionExperimentStep.Objective)
            currentStep = FrictionExperimentStep.SelectEquipment;
        if (currentStep != FrictionExperimentStep.SelectEquipment) return;
        if (FrictionEquipmentSelectionManager.Instance != null && FrictionEquipmentSelectionManager.Instance.IsCompleteCheck())
        {
            FrictionScoreManager.Instance?.AddScore(5, false);
            AdvanceStep();
        }
        else
        {
            FrictionScoreManager.Instance?.SubtractScore(5);
            FrictionFeedbackManager.Instance?.ShowInstruction("Select all required equipment before continuing.");
        }
    }

    public void StartTrial(int trial)
    {
        FrictionTrialManager.Instance?.BeginTrial(trial);
        currentStep = FrictionExperimentStep.Setup;
        FrictionEquipmentManager.Instance?.ResetTray();
        EnterStep();
        UpdateUI();
    }

    public void SelectSurface(int surfaceIndex)
    {
        if (currentStep != FrictionExperimentStep.Setup)
        {
            FrictionFeedbackManager.Instance?.ShowInstruction("Select the surface during setup.");
            return;
        }
        WoodenBlockController.Instance?.RotateToSurface(surfaceIndex);
        int expected = FrictionTrialManager.Instance != null ? FrictionTrialManager.Instance.ExpectedSurface : 0;
        if (surfaceIndex == expected)
        {
            FrictionFeedbackManager.Instance?.ShowMessage(
                $"✓ Correct. Rotate the block onto surface {(char)('A' + surfaceIndex)}.",
                "",
                new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            FrictionScoreManager.Instance?.SubtractScore(5);
            string need = expected == 0 ? "A (largest, 600 cm²)" : expected == 1 ? "B (300 cm²)" : "C (smallest, 200 cm²)";
            FrictionFeedbackManager.Instance?.ShowMessage($"✗ Incorrect orientation. This trial needs surface {need}.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
        FrictionUIManager.Instance?.UpdateLiveReadings();
    }

    public void ConfirmSetup()
    {
        if (currentStep != FrictionExperimentStep.Setup)
        {
            FrictionFeedbackManager.Instance?.ShowInstruction("Confirm setup before pulling.");
            FrictionScoreManager.Instance?.SubtractScore(5);
            return;
        }
        if (FrictionTrialManager.Instance != null && FrictionTrialManager.Instance.ConfirmSetup())
        {
            currentStep = FrictionExperimentStep.Pulling;
            EnterStep();
            UpdateUI();
        }
    }

    public void StartPulling()
    {
        if (currentStep != FrictionExperimentStep.Pulling)
        {
            FrictionFeedbackManager.Instance?.ShowInstruction("Finish the setup and confirm it before pulling.");
            FrictionScoreManager.Instance?.SubtractScore(5);
            return;
        }
        NewtonBalanceController.Instance?.StartPull();
    }

    public void StopPulling()
    {
        NewtonBalanceController.Instance?.StopPull();
    }

    public void RecordReading()
    {
        if (currentStep != FrictionExperimentStep.Pulling)
        {
            FrictionFeedbackManager.Instance?.ShowInstruction("Record the reading during the pull trial.");
            return;
        }

        var detector = LimitingFrictionDetector.Instance;
        float applied = FrictionAppliedForceController.Instance != null ? FrictionAppliedForceController.Instance.AppliedForce : 0f;
        if (detector == null || !detector.LimitingFrictionDetected)
        {
            FrictionScoreManager.Instance?.SubtractScore(5);
            FrictionFeedbackManager.Instance?.ShowMessage("✗ The block has not started moving yet.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
            return;
        }

        if (applied > detector.DetectedReading + 2.5f)
        {
            FrictionScoreManager.Instance?.SubtractScore(5);
            FrictionFeedbackManager.Instance?.ShowMessage("✗ Record the force at the moment the block JUST begins to move.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
            return;
        }

        float value = detector.DetectedReading;
        FrictionTrialManager.Instance?.RecordCurrentReading(value);
        FrictionScoreManager.Instance?.AddScore(5, false);
        FrictionFeedbackManager.Instance?.ShowMessage(
            $"✓ Correct. The block began moving at approximately {value:0.0} N.",
            "+5 MARKS",
            new Color(0.08f, 0.52f, 0.22f));
        CompleteTrial();
    }

    public void CompleteTrial()
    {
        NewtonBalanceController.Instance?.StopPull();
        if (FrictionTrialManager.Instance != null && FrictionTrialManager.Instance.AllTrialsComplete())
        {
            currentStep = FrictionExperimentStep.ObservationTable;
            EnterStep();
            UpdateUI();
            return;
        }
        FrictionUIManager.Instance?.SetNextButtonVisible(true);
        FrictionFeedbackManager.Instance?.ShowInstruction("Reading recorded. Press NEXT STEP to set up the next surface.");
    }

    public void CompareResults()
    {
        currentStep = FrictionExperimentStep.CompareResults;
        EnterStep();
        UpdateUI();
    }

    public void AnswerCompare(int choice)
    {
        if (currentStep != FrictionExperimentStep.CompareResults) return;
        if (choice == 2)
        {
            if (!compareScored)
            {
                compareScored = true;
                FrictionScoreManager.Instance?.AddScore(5, false);
            }
            FrictionFeedbackManager.Instance?.ShowMessage(
                "✓ Correct. Limiting friction remains approximately the same. Small differences are experimental variation.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            FrictionUIManager.Instance?.SetNextButtonVisible(true);
        }
        else
        {
            FrictionScoreManager.Instance?.SubtractScore(5);
            FrictionFeedbackManager.Instance?.ShowMessage(
                "✗ Incorrect. The three limiting-friction values are approximately the same.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
        }
    }

    public void CompletePractical()
    {
        if (completionScored) return;
        completionScored = true;
        int score = FrictionScoreManager.Instance != null ? FrictionScoreManager.Instance.FinalizeScore() : 0;
        bool passed = score >= 50;
        var attempt = FrictionAttemptManager.Instance != null
            ? FrictionAttemptManager.Instance.RegisterAttempt(score, mistakeCount, FrictionTrialManager.Instance != null ? FrictionTrialManager.Instance.GetAllTrials() : null, passed ? "Completed" : "Needs Improvement")
            : null;
        FrictionProfileManager.Instance?.UpdatePracticalResult(score, mistakeCount, passed, attempt);
        FrictionResultManager.Instance?.ShowResult(score, passed, mistakeCount, attempt);
        FrictionUIManager.Instance?.ShowResult();
        FrictionSaveManager.Instance?.Save(FrictionProfileManager.Instance != null ? FrictionProfileManager.Instance.ProfileData : null);
        SendToFlutter(score, passed);
    }

    public void CompleteExperiment()
    {
        if (!completionScored)
        {
            CompletePractical();
            return;
        }

        int score = FrictionScoreManager.Instance != null ? FrictionScoreManager.Instance.GetScore() : 0;
        SendToFlutter(score, score >= 50);
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
        if (FrictionTrialManager.Instance != null)
        {
            var list = FrictionTrialManager.Instance.GetAllTrials();
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

    public void ResetPractical()
    {
        mistakeCount = 0;
        flutterSent = false;
        compareScored = false;
        completionScored = false;
        currentStep = FrictionExperimentStep.Introduction;
        FrictionScoreManager.Instance?.ResetScore();
        FrictionTrialManager.Instance?.ResetAllTrials();
        FrictionEquipmentSelectionManager.Instance?.ResetSelection();
        ConclusionManager.Instance?.ResetBuilder();
        VariableMatchingManager.Instance?.ResetMatching();
        FrictionObservationTableManager.Instance?.ResetScoring();
        FrictionGraphController.Instance?.ResetScoring();
        ResetCurrentTrial();
        UpdateUI();
    }

    public void ResetCurrentTrial()
    {
        FrictionTrialManager.Instance?.ResetCurrentTrialKeepData();
        FrictionEquipmentManager.Instance?.ResetTray();
        FrictionAppliedForceController.Instance?.ResetForce();
        NewtonBalanceController.Instance?.ResetBalance();
        NewtonBalancePointer.Instance?.ResetPointer();
        WoodenBlockController.Instance?.ResetPosition();
        SandpaperController.Instance?.ResetPlacement();
        PullController.Instance?.ResetPull();
        FrictionForceController.Instance?.ResetForces();
        FrictionUIManager.Instance?.UpdateLiveReadings();
    }

    public void ResetExperiment() => ResetPractical();

    public void RetryExperiment()
    {
        if (FrictionAttemptManager.Instance != null && !FrictionAttemptManager.Instance.CanRetry())
        {
            FrictionFeedbackManager.Instance?.ShowInstruction("No attempts remaining.");
            return;
        }
        ResetPractical();
        StartPractical();
    }

    private bool CanLeaveCurrentStep()
    {
        switch (currentStep)
        {
            case FrictionExperimentStep.SelectEquipment:
                if (FrictionEquipmentSelectionManager.Instance == null || !FrictionEquipmentSelectionManager.Instance.IsCompleteCheck())
                {
                    FrictionScoreManager.Instance?.SubtractScore(5);
                    FrictionFeedbackManager.Instance?.ShowInstruction("Select all required equipment first.");
                    return false;
                }
                return true;
            case FrictionExperimentStep.Setup:
                FrictionFeedbackManager.Instance?.ShowInstruction("Confirm the setup with the CONFIRM SETUP button.");
                return false;
            case FrictionExperimentStep.Pulling:
                if (FrictionTrialManager.Instance != null && FrictionTrialManager.Instance.GetTrial(FrictionTrialManager.Instance.CurrentTrial) != null &&
                    FrictionTrialManager.Instance.GetTrial(FrictionTrialManager.Instance.CurrentTrial).completed)
                    return true;
                FrictionScoreManager.Instance?.SubtractScore(5);
                FrictionFeedbackManager.Instance?.ShowInstruction("Record the limiting friction before continuing.");
                return false;
            case FrictionExperimentStep.Questions:
                if (FrictionQuestionManager.Instance != null && FrictionQuestionManager.Instance.IsFinished) return true;
                FrictionFeedbackManager.Instance?.ShowInstruction("Select an answer, then press CONTINUE.");
                return false;
            case FrictionExperimentStep.VariableMatching:
                return true;
            default:
                return true;
        }
    }

    private void EnterStep()
    {
        switch (currentStep)
        {
            case FrictionExperimentStep.Setup:
                if (FrictionTrialManager.Instance != null && FrictionTrialManager.Instance.CurrentTrial < 1)
                    StartTrial(1);
                FrictionEquipmentManager.Instance?.ResetTray();
                break;
            case FrictionExperimentStep.ObservationTable:
                FrictionObservationTableManager.Instance?.Refresh();
                break;
            case FrictionExperimentStep.Graph:
                FrictionGraphController.Instance?.ShowGraphs();
                break;
            case FrictionExperimentStep.Questions:
                FrictionQuestionManager.Instance?.StartQuiz();
                break;
            case FrictionExperimentStep.VariableMatching:
                VariableMatchingManager.Instance?.ResetMatching();
                break;
            case FrictionExperimentStep.Conclusion:
                ConclusionManager.Instance?.ResetBuilder();
                break;
        }
    }

    public void UpdateUI() => FrictionUIManager.Instance?.ShowStep(currentStep);
}
