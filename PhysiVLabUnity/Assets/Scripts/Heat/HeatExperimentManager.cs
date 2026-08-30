using System.Globalization;
using UnityEngine;

public class HeatExperimentManager : MonoBehaviour
{
    public static HeatExperimentManager Instance { get; private set; }

    [SerializeField] private HeatExperimentStep currentStep = HeatExperimentStep.Introduction;
    [SerializeField] private int totalDisplaySteps = 11;
    [SerializeField] private int mistakeCount;
    private bool flutterSent;
    [SerializeField] private int correctScore = 5;
    [SerializeField] private int wrongPenalty = 5;
    [SerializeField] private int majorStepScore = 10;
    [SerializeField] private int maximumAttempts = 3;
    [SerializeField] private bool heatScored;
    [SerializeField] private bool identifyScored;
    [SerializeField] private bool completionScored;
    [SerializeField] private bool identifiedLevels;

    public HeatExperimentStep CurrentStep => currentStep;
    public int MistakeCount => mistakeCount;
    public int TotalDisplaySteps => totalDisplaySteps;
    public bool IdentifiedLevels => identifiedLevels;

    private void Awake() => Instance = this;

    private void Start() => ApplyInspectorSettings();

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void ApplyInspectorSettings()
    {
        HeatScoreManager.Instance?.Configure(correctScore, wrongPenalty, majorStepScore);
        HeatAttemptManager.Instance?.Configure(maximumAttempts);
        int required = HeatEquipmentSelectionManager.Instance != null ? HeatEquipmentSelectionManager.Instance.RequiredCount : 8;
        HeatScoreManager.Instance?.ConfigureMaxRaw(required);
    }

    public void RegisterMistake() => mistakeCount++;

    public void StartPractical()
    {
        mistakeCount = 0;
        flutterSent = false;
        heatScored = false;
        identifyScored = false;
        completionScored = false;
        identifiedLevels = false;
        HeatScoreManager.Instance?.ResetScore();
        HeatEquipmentSelectionManager.Instance?.EnsureCardsVisible();
        HeatEquipmentSelectionManager.Instance?.ResetSelection();
        HeatObservationTableManager.Instance?.ResetScoring();
        HeatConclusionManager.Instance?.ResetBuilder();
        HeatVariableMatchingManager.Instance?.ResetMatching();
        HeatAssemblyManager.Instance?.ResetAssembly();
        HeatEquipmentManager.Instance?.ResetTray();
        ApplyInspectorSettings();
        currentStep = HeatExperimentStep.Objective;
        HeatUIManager.Instance?.HideResult();
        HeatUIManager.Instance?.SetNextButtonVisible(true);
        HeatUIManager.Instance?.UpdateAttemptsDisplay(HeatAttemptManager.Instance != null ? HeatAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateUI();
    }

    public void SetStep(HeatExperimentStep step)
    {
        currentStep = step;
        EnterStep();
        UpdateUI();
        if (currentStep == HeatExperimentStep.Complete)
            CompletePractical();
    }

    public void AdvanceStep()
    {
        if (currentStep >= HeatExperimentStep.Complete) return;
        if (!CanLeaveCurrentStep()) return;
        currentStep++;
        EnterStep();
        UpdateUI();
        if (currentStep == HeatExperimentStep.Complete)
            CompletePractical();
    }

    public void CompleteQuestions()
    {
        currentStep = HeatExperimentStep.VariableMatching;
        EnterStep();
        UpdateUI();
    }

    public void CompleteVariableMatching()
    {
        currentStep = HeatExperimentStep.Conclusion;
        EnterStep();
        UpdateUI();
    }

    public void CompleteConclusion()
    {
        currentStep = HeatExperimentStep.Complete;
        EnterStep();
        UpdateUI();
        CompletePractical();
    }

    public void TryAdvanceFromEquipment()
    {
        if (currentStep == HeatExperimentStep.Introduction || currentStep == HeatExperimentStep.Objective)
            currentStep = HeatExperimentStep.SelectEquipment;
        if (currentStep != HeatExperimentStep.SelectEquipment) return;
        if (HeatEquipmentSelectionManager.Instance != null && HeatEquipmentSelectionManager.Instance.IsCompleteCheck())
        {
            HeatScoreManager.Instance?.AddScore(5, false);
            AdvanceStep();
        }
        else
        {
            HeatScoreManager.Instance?.SubtractScore(5);
            HeatFeedbackManager.Instance?.ShowInstruction("Select all required equipment before continuing.");
        }
    }

    public void MarkLevelA()
    {
        if (currentStep != HeatExperimentStep.Assembly)
        {
            HeatFeedbackManager.Instance?.ShowInstruction("Mark level A during the assembly step.");
            HeatScoreManager.Instance?.SubtractScore(5);
            return;
        }
        HeatAssemblyManager.Instance?.MarkLevelA();
    }

    public void ConfirmSetup()
    {
        if (currentStep != HeatExperimentStep.Assembly)
        {
            HeatFeedbackManager.Instance?.ShowInstruction("Confirm setup during the assembly step.");
            HeatScoreManager.Instance?.SubtractScore(5);
            return;
        }
        if (HeatAssemblyManager.Instance != null && HeatAssemblyManager.Instance.ConfirmSetup())
        {
            currentStep = HeatExperimentStep.HeatObserve;
            EnterStep();
            UpdateUI();
        }
    }

    public void StartHeating()
    {
        if (currentStep != HeatExperimentStep.HeatObserve)
        {
            HeatFeedbackManager.Instance?.ShowInstruction("Heat the water bath in the heating step.");
            return;
        }
        HeatVisualController.Instance?.StartHeating();
    }

    public void ConfirmLevelsObserved()
    {
        if (currentStep != HeatExperimentStep.HeatObserve)
        {
            HeatFeedbackManager.Instance?.ShowInstruction("Watch the liquid fall to B then rise to C in this step.");
            return;
        }
        var vis = HeatVisualController.Instance;
        if (vis == null || !vis.ReachedLevelC)
        {
            HeatScoreManager.Instance?.SubtractScore(5);
            HeatFeedbackManager.Instance?.ShowMessage(
                "✗ NOT YET\nPress START HEATING and wait until the liquid drops to B and then rises to C.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            return;
        }
        heatScored = true;
        HeatFeedbackManager.Instance?.ShowMessage(
            "✓ LEVELS OBSERVED\nThe liquid first fell from A to B (glass expands first), then rose past A to C (liquid expands more than glass).",
            "",
            new Color(0.08f, 0.52f, 0.22f));
        HeatUIManager.Instance?.SetNextButtonVisible(true);
        HeatUIManager.Instance?.UpdateLiveReadings();
    }

    public void AnswerLevels(int choice)
    {
        if (currentStep != HeatExperimentStep.IdentifyLevels) return;
        if (choice == 1)
        {
            identifiedLevels = true;
            if (!identifyScored)
            {
                identifyScored = true;
                HeatScoreManager.Instance?.AddScore(5, false);
            }
            HeatFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT\nThe glass container expands first, so the liquid level falls from A to B. Then the liquid expands more than the glass, so the level rises to C.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            HeatUIManager.Instance?.SetNextButtonVisible(true);
        }
        else
        {
            HeatScoreManager.Instance?.SubtractScore(5);
            HeatFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nHeat reaches the glass first, so the test tube expands and the level drops to B. Liquids expand more than solids, so the level then rises to C.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
        }
    }

    public void CompletePractical()
    {
        if (completionScored) return;
        completionScored = true;
        int score = HeatScoreManager.Instance != null ? HeatScoreManager.Instance.FinalizeScore() : 0;
        bool passed = score >= 50;
        var attempt = HeatAttemptManager.Instance != null
            ? HeatAttemptManager.Instance.RegisterAttempt(score, mistakeCount, passed ? "Completed" : "Needs Improvement")
            : null;
        HeatProfileManager.Instance?.UpdatePracticalResult(score, mistakeCount, passed, attempt);
        HeatResultManager.Instance?.ShowResult(score, passed, mistakeCount, attempt);
        HeatUIManager.Instance?.ShowResult();
        HeatSaveManager.Instance?.Save(HeatProfileManager.Instance != null ? HeatProfileManager.Instance.ProfileData : null);
        SendToFlutter(score, passed);
    }

    public void CompleteExperiment()
    {
        if (!completionScored)
        {
            CompletePractical();
            return;
        }

        int score = HeatScoreManager.Instance != null ? HeatScoreManager.Instance.GetScore() : 0;
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
        string measurements =
            "{\"mistakes\":" + mistakeCount.ToString(CultureInfo.InvariantCulture)
            + ",\"identifiedLevels\":" + (identifiedLevels ? "true" : "false")
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
        heatScored = false;
        identifyScored = false;
        completionScored = false;
        identifiedLevels = false;
        currentStep = HeatExperimentStep.Introduction;
        HeatScoreManager.Instance?.ResetScore();
        HeatEquipmentSelectionManager.Instance?.ResetSelection();
        HeatConclusionManager.Instance?.ResetBuilder();
        HeatVariableMatchingManager.Instance?.ResetMatching();
        HeatObservationTableManager.Instance?.ResetScoring();
        HeatAssemblyManager.Instance?.ResetAssembly();
        HeatEquipmentManager.Instance?.ResetTray();
        UpdateUI();
    }

    public void ResetExperiment() => ResetPractical();

    public void RetryExperiment()
    {
        if (HeatAttemptManager.Instance != null && !HeatAttemptManager.Instance.CanRetry())
        {
            HeatFeedbackManager.Instance?.ShowInstruction("No attempts remaining.");
            return;
        }
        ResetPractical();
        StartPractical();
    }

    private bool CanLeaveCurrentStep()
    {
        switch (currentStep)
        {
            case HeatExperimentStep.SelectEquipment:
                if (HeatEquipmentSelectionManager.Instance == null || !HeatEquipmentSelectionManager.Instance.IsCompleteCheck())
                {
                    HeatScoreManager.Instance?.SubtractScore(5);
                    HeatFeedbackManager.Instance?.ShowInstruction("Select all required equipment first.");
                    return false;
                }
                return true;
            case HeatExperimentStep.Assembly:
                HeatFeedbackManager.Instance?.ShowInstruction("Assemble the apparatus in order, mark A, then press CONFIRM SETUP.");
                return false;
            case HeatExperimentStep.HeatObserve:
                if (heatScored || (HeatVisualController.Instance != null && HeatVisualController.Instance.ReachedLevelC))
                {
                    heatScored = true;
                    return true;
                }
                HeatScoreManager.Instance?.SubtractScore(5);
                HeatFeedbackManager.Instance?.ShowInstruction("Heat the bath, watch A → B → C, then press LEVELS REACHED C.");
                return false;
            case HeatExperimentStep.IdentifyLevels:
                if (identifiedLevels) return true;
                HeatScoreManager.Instance?.SubtractScore(5);
                HeatFeedbackManager.Instance?.ShowInstruction("Choose the correct explanation of levels A, B and C.");
                return false;
            case HeatExperimentStep.Questions:
                if (HeatQuestionManager.Instance != null && HeatQuestionManager.Instance.IsFinished) return true;
                HeatFeedbackManager.Instance?.ShowInstruction("Select an answer, then press CONTINUE.");
                return false;
            default:
                return true;
        }
    }

    private void EnterStep()
    {
        switch (currentStep)
        {
            case HeatExperimentStep.Assembly:
                HeatEquipmentManager.Instance?.ResetTray();
                HeatAssemblyManager.Instance?.ResetAssembly();
                break;
            case HeatExperimentStep.ObservationTable:
                HeatObservationTableManager.Instance?.Refresh();
                break;
            case HeatExperimentStep.Questions:
                HeatQuestionManager.Instance?.StartQuiz();
                break;
            case HeatExperimentStep.VariableMatching:
                HeatVariableMatchingManager.Instance?.ResetMatching();
                break;
            case HeatExperimentStep.Conclusion:
                HeatConclusionManager.Instance?.ResetBuilder();
                break;
        }
    }

    public void UpdateUI() => HeatUIManager.Instance?.ShowStep(currentStep);
}
