using System.Globalization;
using UnityEngine;

public class TurningExperimentManager : MonoBehaviour
{
    public static TurningExperimentManager Instance { get; private set; }

    [SerializeField] private TurningExperimentStep currentStep = TurningExperimentStep.Introduction;
    [SerializeField] private int totalDisplaySteps = 12;
    [SerializeField] private int mistakeCount;
    private bool flutterSent;
    [SerializeField] private int correctScore = 5;
    [SerializeField] private int wrongPenalty = 5;
    [SerializeField] private int majorStepScore = 10;
    [SerializeField] private int maximumAttempts = 3;
    [SerializeField] private bool compareScored;
    [SerializeField] private bool completionScored;

    public TurningExperimentStep CurrentStep => currentStep;
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
        TurningScoreManager.Instance?.Configure(correctScore, wrongPenalty, majorStepScore);
        TurningAttemptManager.Instance?.Configure(maximumAttempts);
        int required = TurningEquipmentSelectionManager.Instance != null ? TurningEquipmentSelectionManager.Instance.RequiredCount : 7;
        TurningScoreManager.Instance?.ConfigureMaxRaw(required);
    }

    public void RegisterMistake() => mistakeCount++;

    public void StartPractical()
    {
        mistakeCount = 0;
        flutterSent = false;
        compareScored = false;
        completionScored = false;
        TurningScoreManager.Instance?.ResetScore();
        TurningEquipmentSelectionManager.Instance?.EnsureCardsVisible();
        TurningEquipmentSelectionManager.Instance?.ResetSelection();
        TurningTrialManager.Instance?.ResetAllTrials();
        TurningObservationTableManager.Instance?.ResetScoring();
        TurningGraphController.Instance?.ResetScoring();
        TurningConclusionManager.Instance?.ResetBuilder();
        TurningVariableMatchingManager.Instance?.ResetMatching();
        TurningAssemblyManager.Instance?.ResetAssembly();
        TurningMomentController.Instance?.ResetAll();
        ApplyInspectorSettings();
        currentStep = TurningExperimentStep.Objective;
        TurningUIManager.Instance?.HideResult();
        TurningUIManager.Instance?.SetNextButtonVisible(true);
        TurningUIManager.Instance?.UpdateAttemptsDisplay(TurningAttemptManager.Instance != null ? TurningAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateUI();
    }

    public void SetStep(TurningExperimentStep step)
    {
        currentStep = step;
        EnterStep();
        UpdateUI();
        if (currentStep == TurningExperimentStep.Complete)
            CompletePractical();
    }

    public void AdvanceStep()
    {
        if (currentStep >= TurningExperimentStep.Complete) return;
        if (!CanLeaveCurrentStep()) return;

        if (currentStep == TurningExperimentStep.ApplyForce)
        {
            if (!TurningTrialManager.Instance.AllTrialsComplete())
            {
                int next = TurningTrialManager.Instance.CurrentTrial + 1;
                StartTrial(next);
                currentStep = TurningExperimentStep.ApplyForce;
                EnterStep();
                UpdateUI();
                return;
            }
        }

        currentStep++;
        EnterStep();
        UpdateUI();
        if (currentStep == TurningExperimentStep.Complete)
            CompletePractical();
    }

    public void CompleteQuestions()
    {
        currentStep = TurningExperimentStep.VariableMatching;
        EnterStep();
        UpdateUI();
    }

    public void CompleteVariableMatching()
    {
        currentStep = TurningExperimentStep.Conclusion;
        EnterStep();
        UpdateUI();
    }

    public void CompleteConclusion()
    {
        currentStep = TurningExperimentStep.Complete;
        EnterStep();
        UpdateUI();
        CompletePractical();
    }

    public void TryAdvanceFromEquipment()
    {
        if (currentStep == TurningExperimentStep.Introduction || currentStep == TurningExperimentStep.Objective)
            currentStep = TurningExperimentStep.SelectEquipment;
        if (currentStep != TurningExperimentStep.SelectEquipment) return;
        if (TurningEquipmentSelectionManager.Instance != null && TurningEquipmentSelectionManager.Instance.IsCompleteCheck())
        {
            TurningScoreManager.Instance?.AddScore(5, false);
            AdvanceStep();
        }
        else
        {
            TurningScoreManager.Instance?.SubtractScore(5);
            TurningFeedbackManager.Instance?.ShowInstruction("Select all required equipment before continuing.");
        }
    }

    public void StartTrial(int trial)
    {
        TurningTrialManager.Instance?.BeginTrial(trial);
        currentStep = TurningExperimentStep.ApplyForce;
        TurningMomentController.Instance?.PrepareTrial(trial);
        EnterStep();
        UpdateUI();
    }

    public void ConfirmSetup()
    {
        if (currentStep != TurningExperimentStep.Assembly)
        {
            TurningFeedbackManager.Instance?.ShowInstruction("Confirm setup during the assembly step.");
            TurningScoreManager.Instance?.SubtractScore(5);
            return;
        }
        if (TurningAssemblyManager.Instance != null && TurningAssemblyManager.Instance.ConfirmSetup())
        {
            StartTrial(1);
        }
    }

    public void ChangeForce(float delta) => TurningMomentController.Instance?.AddForce(delta);
    public void ChangeAngle(float delta) => TurningMomentController.Instance?.AddAngle(delta);

    public void TightenScrew()
    {
        if (currentStep != TurningExperimentStep.ApplyForce)
        {
            TurningFeedbackManager.Instance?.ShowInstruction("Tighten the screw during the measurement trials.");
            return;
        }
        var trial = TurningTrialManager.Instance;
        if (trial == null) return;
        var current = trial.GetTrial(trial.CurrentTrial);
        if (current != null && current.completed)
        {
            TurningFeedbackManager.Instance?.ShowInstruction("This trial is already recorded. Press NEXT STEP.");
            return;
        }
        if (trial.CurrentTrial <= 1)
        {
            TurningScoreManager.Instance?.SubtractScore(5);
            TurningFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nFirst measure the force at the initial tightness. Tighten the screw after Trial 1.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            return;
        }
        var mom = TurningMomentController.Instance;
        if (mom == null) return;
        if (mom.TightnessLevel >= trial.CurrentTrial)
        {
            TurningFeedbackManager.Instance?.ShowInstruction("The screw is already at the tightness for this trial. Pull the Newton balance.");
            return;
        }
        mom.TightenScrew();
    }

    public void RecordReading()
    {
        if (currentStep != TurningExperimentStep.ApplyForce)
        {
            TurningFeedbackManager.Instance?.ShowInstruction("Record the force while pulling the Newton balance.");
            return;
        }

        var mom = TurningMomentController.Instance;
        if (mom == null || !mom.BalanceAttached)
        {
            TurningScoreManager.Instance?.SubtractScore(5);
            TurningFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nHook the Newton balance onto the wire loop at D first.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            return;
        }

        var trial = TurningTrialManager.Instance;
        if (trial != null && mom.TightnessLevel != trial.CurrentTrial)
        {
            TurningScoreManager.Instance?.SubtractScore(5);
            TurningFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nRotate the screw nail half a turn to tighten it, then pull again.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            return;
        }

        if (!mom.IsPerpendicular)
        {
            TurningScoreManager.Instance?.SubtractScore(5);
            TurningFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nPull the Newton balance perpendicular to the stick (about 90°).",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            return;
        }

        if (!mom.StickJustMoves)
        {
            TurningScoreManager.Instance?.SubtractScore(5);
            TurningFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nIncrease the force until the stick just begins to turn.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            return;
        }

        float target = mom.TargetForceAtD;
        bool nearMin = mom.ForceN <= target + 1.0f;
        TurningTrialManager.Instance?.RecordCurrentReading(mom.ForceN, mom.AngleDeg, mom.MomentNm, mom.AttachedPoint, mom.StickJustMoves);
        if (nearMin)
        {
            TurningScoreManager.Instance?.AddScore(5, false);
            TurningFeedbackManager.Instance?.ShowMessage(
                $"✓ CORRECT\nMinimum force at D ≈ {mom.ForceN:0.0} N\nMoment = {mom.ForceN:0.0} N × 0.60 m = {mom.MomentNm:0.00} N m",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            TurningScoreManager.Instance?.AddScore(5, false);
            TurningFeedbackManager.Instance?.ShowMessage(
                $"✓ Reading recorded, but the force is larger than the minimum.\nThe stick just moves near {target:0.0} N at D.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
        }
        CompleteTrial();
    }

    public void CompleteTrial()
    {
        if (TurningTrialManager.Instance != null && TurningTrialManager.Instance.AllTrialsComplete())
        {
            currentStep = TurningExperimentStep.ObservationTable;
            EnterStep();
            UpdateUI();
            return;
        }
        TurningUIManager.Instance?.SetNextButtonVisible(true);
        TurningFeedbackManager.Instance?.ShowInstruction("Reading recorded. Press NEXT STEP, then tighten the screw half a turn for the next trial.");
    }

    public void AnswerCompare(int choice)
    {
        if (currentStep != TurningExperimentStep.CompareResults) return;
        if (choice == 2)
        {
            if (!compareScored)
            {
                compareScored = true;
                TurningScoreManager.Instance?.AddScore(5, false);
            }
            TurningFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT\nAs the screw is tightened, friction at the pivot increases, so a larger force is needed to turn the stick.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            TurningUIManager.Instance?.SetNextButtonVisible(true);
        }
        else
        {
            TurningScoreManager.Instance?.SubtractScore(5);
            TurningFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nLook at the table. The force at D increases as the screw is tightened.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
        }
    }

    public void CompletePractical()
    {
        if (completionScored) return;
        completionScored = true;
        int score = TurningScoreManager.Instance != null ? TurningScoreManager.Instance.FinalizeScore() : 0;
        bool passed = score >= 50;
        var attempt = TurningAttemptManager.Instance != null
            ? TurningAttemptManager.Instance.RegisterAttempt(score, mistakeCount, TurningTrialManager.Instance != null ? TurningTrialManager.Instance.GetAllTrials() : null, passed ? "Completed" : "Needs Improvement")
            : null;
        TurningProfileManager.Instance?.UpdatePracticalResult(score, mistakeCount, passed, attempt);
        TurningResultManager.Instance?.ShowResult(score, passed, mistakeCount, attempt);
        TurningUIManager.Instance?.ShowResult();
        TurningSaveManager.Instance?.Save(TurningProfileManager.Instance != null ? TurningProfileManager.Instance.ProfileData : null);
        SendToFlutter(score, passed);
    }

    public void CompleteExperiment()
    {
        if (!completionScored)
        {
            CompletePractical();
            return;
        }

        int score = TurningScoreManager.Instance != null ? TurningScoreManager.Instance.GetScore() : 0;
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
        if (TurningTrialManager.Instance != null)
        {
            var list = TurningTrialManager.Instance.GetAllTrials();
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
        currentStep = TurningExperimentStep.Introduction;
        TurningScoreManager.Instance?.ResetScore();
        TurningTrialManager.Instance?.ResetAllTrials();
        TurningEquipmentSelectionManager.Instance?.ResetSelection();
        TurningConclusionManager.Instance?.ResetBuilder();
        TurningVariableMatchingManager.Instance?.ResetMatching();
        TurningObservationTableManager.Instance?.ResetScoring();
        TurningGraphController.Instance?.ResetScoring();
        TurningAssemblyManager.Instance?.ResetAssembly();
        TurningMomentController.Instance?.ResetAll();
        TurningEquipmentManager.Instance?.ResetTray();
        UpdateUI();
    }

    public void ResetCurrentTrial()
    {
        TurningTrialManager.Instance?.ResetCurrentTrialKeepData();
        TurningMomentController.Instance?.ResetForces();
        TurningUIManager.Instance?.UpdateLiveReadings();
    }

    public void ResetExperiment() => ResetPractical();

    public void RetryExperiment()
    {
        if (TurningAttemptManager.Instance != null && !TurningAttemptManager.Instance.CanRetry())
        {
            TurningFeedbackManager.Instance?.ShowInstruction("No attempts remaining.");
            return;
        }
        ResetPractical();
        StartPractical();
    }

    private bool CanLeaveCurrentStep()
    {
        switch (currentStep)
        {
            case TurningExperimentStep.SelectEquipment:
                if (TurningEquipmentSelectionManager.Instance == null || !TurningEquipmentSelectionManager.Instance.IsCompleteCheck())
                {
                    TurningScoreManager.Instance?.SubtractScore(5);
                    TurningFeedbackManager.Instance?.ShowInstruction("Select all required equipment first.");
                    return false;
                }
                return true;
            case TurningExperimentStep.Assembly:
                TurningFeedbackManager.Instance?.ShowInstruction("Place all apparatus, then press CONFIRM SETUP.");
                return false;
            case TurningExperimentStep.ApplyForce:
                if (TurningTrialManager.Instance != null && TurningTrialManager.Instance.GetTrial(TurningTrialManager.Instance.CurrentTrial) != null &&
                    TurningTrialManager.Instance.GetTrial(TurningTrialManager.Instance.CurrentTrial).completed)
                    return true;
                TurningScoreManager.Instance?.SubtractScore(5);
                TurningFeedbackManager.Instance?.ShowInstruction("Record the minimum force before continuing.");
                return false;
            case TurningExperimentStep.Questions:
                if (TurningQuestionManager.Instance != null && TurningQuestionManager.Instance.IsFinished) return true;
                TurningFeedbackManager.Instance?.ShowInstruction("Select an answer, then press CONTINUE.");
                return false;
            default:
                return true;
        }
    }

    private void EnterStep()
    {
        switch (currentStep)
        {
            case TurningExperimentStep.Assembly:
                TurningEquipmentManager.Instance?.ResetTray();
                TurningAssemblyManager.Instance?.ResetAssembly();
                TurningMomentController.Instance?.ResetAll();
                break;
            case TurningExperimentStep.ApplyForce:
                if (TurningTrialManager.Instance != null && TurningTrialManager.Instance.CurrentTrial < 1)
                    StartTrial(1);
                break;
            case TurningExperimentStep.ObservationTable:
                TurningObservationTableManager.Instance?.Refresh();
                break;
            case TurningExperimentStep.Graph:
                TurningGraphController.Instance?.ShowGraphs();
                break;
            case TurningExperimentStep.Questions:
                TurningQuestionManager.Instance?.StartQuiz();
                break;
            case TurningExperimentStep.VariableMatching:
                TurningVariableMatchingManager.Instance?.ResetMatching();
                break;
            case TurningExperimentStep.Conclusion:
                TurningConclusionManager.Instance?.ResetBuilder();
                break;
        }
    }

    public void UpdateUI() => TurningUIManager.Instance?.ShowStep(currentStep);
}
