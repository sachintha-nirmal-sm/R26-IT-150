using System.Globalization;
using UnityEngine;

public class EquilibriumExperimentManager : MonoBehaviour
{
    public static EquilibriumExperimentManager Instance { get; private set; }

    [SerializeField] private EquilibriumExperimentStep currentStep = EquilibriumExperimentStep.Introduction;
    [SerializeField] private int totalDisplaySteps = 12;
    [SerializeField] private int mistakeCount;
    private bool flutterSent;
    [SerializeField] private int correctScore = 5;
    [SerializeField] private int wrongPenalty = 5;
    [SerializeField] private int majorStepScore = 10;
    [SerializeField] private int maximumAttempts = 3;
    [SerializeField] private bool compareScored;
    [SerializeField] private bool completionScored;

    public EquilibriumExperimentStep CurrentStep => currentStep;
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
        EquilibriumScoreManager.Instance?.Configure(correctScore, wrongPenalty, majorStepScore);
        EquilibriumAttemptManager.Instance?.Configure(maximumAttempts);
        int required = EquilibriumEquipmentSelectionManager.Instance != null ? EquilibriumEquipmentSelectionManager.Instance.RequiredCount : 4;
        EquilibriumScoreManager.Instance?.ConfigureMaxRaw(required);
    }

    public void RegisterMistake() => mistakeCount++;

    public void StartPractical()
    {
        mistakeCount = 0;
        flutterSent = false;
        compareScored = false;
        completionScored = false;
        EquilibriumScoreManager.Instance?.ResetScore();
        EquilibriumEquipmentSelectionManager.Instance?.EnsureCardsVisible();
        EquilibriumEquipmentSelectionManager.Instance?.ResetSelection();
        EquilibriumTrialManager.Instance?.ResetAllTrials();
        EquilibriumObservationTableManager.Instance?.ResetScoring();
        EquilibriumGraphController.Instance?.ResetScoring();
        EquilibriumConclusionManager.Instance?.ResetBuilder();
        EquilibriumVariableMatchingManager.Instance?.ResetMatching();
        EquilibriumAssemblyManager.Instance?.ResetAssembly();
        EquilibriumForceController.Instance?.ResetAll();
        ApplyInspectorSettings();
        currentStep = EquilibriumExperimentStep.Objective;
        EquilibriumUIManager.Instance?.HideResult();
        EquilibriumUIManager.Instance?.SetNextButtonVisible(true);
        EquilibriumUIManager.Instance?.UpdateAttemptsDisplay(EquilibriumAttemptManager.Instance != null ? EquilibriumAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateUI();
    }

    public void SetStep(EquilibriumExperimentStep step)
    {
        currentStep = step;
        EnterStep();
        UpdateUI();
        if (currentStep == EquilibriumExperimentStep.Complete)
            CompletePractical();
    }

    public void AdvanceStep()
    {
        if (currentStep >= EquilibriumExperimentStep.Complete) return;
        if (!CanLeaveCurrentStep()) return;

        if (currentStep == EquilibriumExperimentStep.Equilibrium)
        {
            if (!EquilibriumTrialManager.Instance.AllTrialsComplete())
            {
                int next = EquilibriumTrialManager.Instance.CurrentTrial + 1;
                StartTrial(next);
                currentStep = EquilibriumExperimentStep.Equilibrium;
                EnterStep();
                UpdateUI();
                return;
            }
        }

        currentStep++;
        EnterStep();
        UpdateUI();
        if (currentStep == EquilibriumExperimentStep.Complete)
            CompletePractical();
    }

    public void CompleteQuestions()
    {
        currentStep = EquilibriumExperimentStep.VariableMatching;
        EnterStep();
        UpdateUI();
    }

    public void CompleteVariableMatching()
    {
        currentStep = EquilibriumExperimentStep.Conclusion;
        EnterStep();
        UpdateUI();
    }

    public void CompleteConclusion()
    {
        currentStep = EquilibriumExperimentStep.Complete;
        EnterStep();
        UpdateUI();
        CompletePractical();
    }

    public void TryAdvanceFromEquipment()
    {
        if (currentStep == EquilibriumExperimentStep.Introduction || currentStep == EquilibriumExperimentStep.Objective)
            currentStep = EquilibriumExperimentStep.SelectEquipment;
        if (currentStep != EquilibriumExperimentStep.SelectEquipment) return;
        if (EquilibriumEquipmentSelectionManager.Instance != null && EquilibriumEquipmentSelectionManager.Instance.IsCompleteCheck())
        {
            EquilibriumScoreManager.Instance?.AddScore(5, false);
            AdvanceStep();
        }
        else
        {
            EquilibriumScoreManager.Instance?.SubtractScore(5);
            EquilibriumFeedbackManager.Instance?.ShowInstruction("Select all required equipment before continuing.");
        }
    }

    public void StartTrial(int trial)
    {
        EquilibriumTrialManager.Instance?.BeginTrial(trial);
        currentStep = EquilibriumExperimentStep.Equilibrium;
        EnterStep();
        UpdateUI();
    }

    public void ConfirmSetup()
    {
        if (currentStep != EquilibriumExperimentStep.Assembly)
        {
            EquilibriumFeedbackManager.Instance?.ShowInstruction("Confirm setup during the assembly step.");
            EquilibriumScoreManager.Instance?.SubtractScore(5);
            return;
        }
        if (EquilibriumAssemblyManager.Instance != null && EquilibriumAssemblyManager.Instance.ConfirmSetup())
        {
            currentStep = EquilibriumExperimentStep.MeasureWeight;
            EquilibriumForceController.Instance?.PrepareWeighing();
            EnterStep();
            UpdateUI();
        }
    }

    public void HangLeft() => EquilibriumForceController.Instance?.TryHangLeft();
    public void HangRight() => EquilibriumForceController.Instance?.TryHangRight();
    public void ChangeTilt(float delta) => EquilibriumForceController.Instance?.ChangeTilt(delta);

    public void WeighRuler()
    {
        if (currentStep != EquilibriumExperimentStep.MeasureWeight)
        {
            EquilibriumFeedbackManager.Instance?.ShowInstruction("Weigh the ruler in the weight-measurement step.");
            return;
        }
        EquilibriumForceController.Instance?.TryWeighRuler();
    }

    public void RecordReading()
    {
        if (currentStep == EquilibriumExperimentStep.MeasureWeight)
        {
            var force = EquilibriumForceController.Instance;
            if (force == null) return;
            if (!force.WeighingAttached)
            {
                EquilibriumScoreManager.Instance?.SubtractScore(5);
                EquilibriumFeedbackManager.Instance?.ShowMessage(
                    "✗ INCORRECT\nHang the meter ruler from spring balance F1 first.",
                    "-5 MARKS",
                    new Color(0.75f, 0.12f, 0.12f));
                return;
            }
            force.ConfirmWeightReading();
            EquilibriumUIManager.Instance?.SetNextButtonVisible(true);
            return;
        }

        if (currentStep != EquilibriumExperimentStep.Equilibrium)
        {
            EquilibriumFeedbackManager.Instance?.ShowInstruction("Record F1 and F2 after the ruler is hanging horizontally.");
            return;
        }

        var ctrl = EquilibriumForceController.Instance;
        if (ctrl == null || !ctrl.CanRecordEquilibrium()) return;

        EquilibriumTrialManager.Instance?.RecordCurrentReading(ctrl.Force1N, ctrl.Force2N, ctrl.MeasuredW, ctrl.TiltDeg, ctrl.IsHorizontal);
        bool sumOk = Mathf.Abs(ctrl.SumN - ctrl.MeasuredW) <= 0.05f;
        if (sumOk)
        {
            EquilibriumScoreManager.Instance?.AddScore(5, false);
            EquilibriumFeedbackManager.Instance?.ShowMessage(
                $"✓ CORRECT\nF1 = {ctrl.Force1N:0.00} N,  F2 = {ctrl.Force2N:0.00} N\nF1 + F2 = {ctrl.SumN:0.00} N  =  W = {ctrl.MeasuredW:0.00} N",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            EquilibriumScoreManager.Instance?.AddScore(5, false);
            EquilibriumFeedbackManager.Instance?.ShowMessage(
                $"✓ Reading recorded.\nF1 + F2 = {ctrl.SumN:0.00} N,  W = {ctrl.MeasuredW:0.00} N",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
        }
        CompleteTrial();
    }

    public void CompleteTrial()
    {
        if (EquilibriumTrialManager.Instance != null && EquilibriumTrialManager.Instance.AllTrialsComplete())
        {
            currentStep = EquilibriumExperimentStep.ObservationTable;
            EnterStep();
            UpdateUI();
            return;
        }
        EquilibriumUIManager.Instance?.SetNextButtonVisible(true);
        EquilibriumFeedbackManager.Instance?.ShowInstruction("Reading recorded. Press NEXT STEP, then hang the ruler again for the next trial.");
    }

    public void AnswerCompare(int choice)
    {
        if (currentStep != EquilibriumExperimentStep.CompareResults) return;
        if (choice == 2)
        {
            if (!compareScored)
            {
                compareScored = true;
                EquilibriumScoreManager.Instance?.AddScore(5, false);
            }
            EquilibriumFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT\nWhen the ruler is horizontal, F1 + F2 equals W. The two upward forces balance the weight.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            EquilibriumUIManager.Instance?.SetNextButtonVisible(true);
        }
        else
        {
            EquilibriumScoreManager.Instance?.SubtractScore(5);
            EquilibriumFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nLook at the table. F1 + F2 should equal the measured weight W.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
        }
    }

    public void CompletePractical()
    {
        if (completionScored) return;
        completionScored = true;
        int score = EquilibriumScoreManager.Instance != null ? EquilibriumScoreManager.Instance.FinalizeScore() : 0;
        bool passed = score >= 50;
        var attempt = EquilibriumAttemptManager.Instance != null
            ? EquilibriumAttemptManager.Instance.RegisterAttempt(score, mistakeCount, EquilibriumTrialManager.Instance != null ? EquilibriumTrialManager.Instance.GetAllTrials() : null, passed ? "Completed" : "Needs Improvement")
            : null;
        EquilibriumProfileManager.Instance?.UpdatePracticalResult(score, mistakeCount, passed, attempt);
        EquilibriumResultManager.Instance?.ShowResult(score, passed, mistakeCount, attempt);
        EquilibriumUIManager.Instance?.ShowResult();
        EquilibriumSaveManager.Instance?.Save(EquilibriumProfileManager.Instance != null ? EquilibriumProfileManager.Instance.ProfileData : null);
        SendToFlutter(score, passed);
    }

    public void CompleteExperiment()
    {
        if (!completionScored)
        {
            CompletePractical();
            return;
        }

        int score = EquilibriumScoreManager.Instance != null ? EquilibriumScoreManager.Instance.GetScore() : 0;
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
        if (EquilibriumTrialManager.Instance != null)
        {
            var list = EquilibriumTrialManager.Instance.GetAllTrials();
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
        currentStep = EquilibriumExperimentStep.Introduction;
        EquilibriumScoreManager.Instance?.ResetScore();
        EquilibriumTrialManager.Instance?.ResetAllTrials();
        EquilibriumEquipmentSelectionManager.Instance?.ResetSelection();
        EquilibriumConclusionManager.Instance?.ResetBuilder();
        EquilibriumVariableMatchingManager.Instance?.ResetMatching();
        EquilibriumObservationTableManager.Instance?.ResetScoring();
        EquilibriumGraphController.Instance?.ResetScoring();
        EquilibriumAssemblyManager.Instance?.ResetAssembly();
        EquilibriumForceController.Instance?.ResetAll();
        EquilibriumEquipmentManager.Instance?.ResetTray();
        UpdateUI();
    }

    public void ResetCurrentTrial()
    {
        EquilibriumTrialManager.Instance?.ResetCurrentTrialKeepData();
        EquilibriumUIManager.Instance?.UpdateLiveReadings();
    }

    public void ResetExperiment() => ResetPractical();

    public void RetryExperiment()
    {
        if (EquilibriumAttemptManager.Instance != null && !EquilibriumAttemptManager.Instance.CanRetry())
        {
            EquilibriumFeedbackManager.Instance?.ShowInstruction("No attempts remaining.");
            return;
        }
        ResetPractical();
        StartPractical();
    }

    private bool CanLeaveCurrentStep()
    {
        switch (currentStep)
        {
            case EquilibriumExperimentStep.SelectEquipment:
                if (EquilibriumEquipmentSelectionManager.Instance == null || !EquilibriumEquipmentSelectionManager.Instance.IsCompleteCheck())
                {
                    EquilibriumScoreManager.Instance?.SubtractScore(5);
                    EquilibriumFeedbackManager.Instance?.ShowInstruction("Select all required equipment first.");
                    return false;
                }
                return true;
            case EquilibriumExperimentStep.Assembly:
                EquilibriumFeedbackManager.Instance?.ShowInstruction("Place all apparatus, then press CONFIRM SETUP.");
                return false;
            case EquilibriumExperimentStep.MeasureWeight:
                if (EquilibriumForceController.Instance != null && EquilibriumForceController.Instance.WeightRecorded)
                    return true;
                EquilibriumScoreManager.Instance?.SubtractScore(5);
                EquilibriumFeedbackManager.Instance?.ShowInstruction("Hang the ruler from F1 and record its weight W first.");
                return false;
            case EquilibriumExperimentStep.Equilibrium:
                if (EquilibriumTrialManager.Instance != null && EquilibriumTrialManager.Instance.GetTrial(EquilibriumTrialManager.Instance.CurrentTrial) != null &&
                    EquilibriumTrialManager.Instance.GetTrial(EquilibriumTrialManager.Instance.CurrentTrial).completed)
                    return true;
                EquilibriumScoreManager.Instance?.SubtractScore(5);
                EquilibriumFeedbackManager.Instance?.ShowInstruction("Level the ruler and record F1 and F2 before continuing.");
                return false;
            case EquilibriumExperimentStep.Questions:
                if (EquilibriumQuestionManager.Instance != null && EquilibriumQuestionManager.Instance.IsFinished) return true;
                EquilibriumFeedbackManager.Instance?.ShowInstruction("Select an answer, then press CONTINUE.");
                return false;
            default:
                return true;
        }
    }

    private void EnterStep()
    {
        switch (currentStep)
        {
            case EquilibriumExperimentStep.Assembly:
                EquilibriumEquipmentManager.Instance?.ResetTray();
                EquilibriumAssemblyManager.Instance?.ResetAssembly();
                EquilibriumForceController.Instance?.ResetAll();
                break;
            case EquilibriumExperimentStep.MeasureWeight:
                EquilibriumForceController.Instance?.PrepareWeighing();
                break;
            case EquilibriumExperimentStep.Equilibrium:
            {
                int t = EquilibriumTrialManager.Instance != null ? Mathf.Max(1, EquilibriumTrialManager.Instance.CurrentTrial) : 1;
                var data = EquilibriumTrialManager.Instance != null ? EquilibriumTrialManager.Instance.GetTrial(t) : null;
                if (data == null || !data.completed)
                    EquilibriumForceController.Instance?.PrepareTrial(t);
                break;
            }
            case EquilibriumExperimentStep.ObservationTable:
                EquilibriumObservationTableManager.Instance?.Refresh();
                break;
            case EquilibriumExperimentStep.Graph:
                EquilibriumGraphController.Instance?.ShowGraphs();
                break;
            case EquilibriumExperimentStep.Questions:
                EquilibriumQuestionManager.Instance?.StartQuiz();
                break;
            case EquilibriumExperimentStep.VariableMatching:
                EquilibriumVariableMatchingManager.Instance?.ResetMatching();
                break;
            case EquilibriumExperimentStep.Conclusion:
                EquilibriumConclusionManager.Instance?.ResetBuilder();
                break;
        }
    }

    public void UpdateUI() => EquilibriumUIManager.Instance?.ShowStep(currentStep);
}
