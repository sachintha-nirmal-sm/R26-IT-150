using System.Globalization;
using UnityEngine;

public class OpticsExperimentManager : MonoBehaviour
{
    public static OpticsExperimentManager Instance { get; private set; }

    [SerializeField] private OpticsExperimentStep currentStep = OpticsExperimentStep.Introduction;
    [SerializeField] private int totalDisplaySteps = 11;
    [SerializeField] private int mistakeCount;
    private bool flutterSent;
    [SerializeField] private int correctScore = 5;
    [SerializeField] private int wrongPenalty = 5;
    [SerializeField] private int majorStepScore = 10;
    [SerializeField] private int maximumAttempts = 3;
    [SerializeField] private bool focusScored;
    [SerializeField] private bool identifyScored;
    [SerializeField] private bool measureScored;
    [SerializeField] private bool completionScored;
    [SerializeField] private bool identifiedImage;
    [SerializeField] private bool measuredFocalLength;

    public OpticsExperimentStep CurrentStep => currentStep;
    public int MistakeCount => mistakeCount;
    public int TotalDisplaySteps => totalDisplaySteps;
    public bool IdentifiedImage => identifiedImage;
    public bool MeasuredFocalLength => measuredFocalLength;

    private void Awake() => Instance = this;

    private void Start() => ApplyInspectorSettings();

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void ApplyInspectorSettings()
    {
        OpticsScoreManager.Instance?.Configure(correctScore, wrongPenalty, majorStepScore);
        OpticsAttemptManager.Instance?.Configure(maximumAttempts);
        int required = OpticsEquipmentSelectionManager.Instance != null ? OpticsEquipmentSelectionManager.Instance.RequiredCount : 3;
        OpticsScoreManager.Instance?.ConfigureMaxRaw(required);
    }

    public void RegisterMistake() => mistakeCount++;

    public void StartPractical()
    {
        mistakeCount = 0;
        flutterSent = false;
        focusScored = false;
        identifyScored = false;
        measureScored = false;
        completionScored = false;
        identifiedImage = false;
        measuredFocalLength = false;
        OpticsScoreManager.Instance?.ResetScore();
        OpticsEquipmentSelectionManager.Instance?.EnsureCardsVisible();
        OpticsEquipmentSelectionManager.Instance?.ResetSelection();
        OpticsObservationTableManager.Instance?.ResetScoring();
        OpticsConclusionManager.Instance?.ResetBuilder();
        OpticsVariableMatchingManager.Instance?.ResetMatching();
        OpticsAssemblyManager.Instance?.ResetAssembly();
        OpticsEquipmentManager.Instance?.ResetTray();
        ApplyInspectorSettings();
        currentStep = OpticsExperimentStep.Objective;
        OpticsUIManager.Instance?.HideResult();
        OpticsUIManager.Instance?.SetNextButtonVisible(true);
        OpticsUIManager.Instance?.UpdateAttemptsDisplay(OpticsAttemptManager.Instance != null ? OpticsAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateUI();
    }

    public void SetStep(OpticsExperimentStep step)
    {
        currentStep = step;
        EnterStep();
        UpdateUI();
        if (currentStep == OpticsExperimentStep.Complete)
            CompletePractical();
    }

    public void AdvanceStep()
    {
        if (currentStep >= OpticsExperimentStep.Complete) return;
        if (!CanLeaveCurrentStep()) return;
        currentStep++;
        EnterStep();
        UpdateUI();
        if (currentStep == OpticsExperimentStep.Complete)
            CompletePractical();
    }

    public void CompleteQuestions()
    {
        currentStep = OpticsExperimentStep.VariableMatching;
        EnterStep();
        UpdateUI();
    }

    public void CompleteVariableMatching()
    {
        currentStep = OpticsExperimentStep.Conclusion;
        EnterStep();
        UpdateUI();
    }

    public void CompleteConclusion()
    {
        currentStep = OpticsExperimentStep.Complete;
        EnterStep();
        UpdateUI();
        CompletePractical();
    }

    public void TryAdvanceFromEquipment()
    {
        if (currentStep == OpticsExperimentStep.Introduction || currentStep == OpticsExperimentStep.Objective)
            currentStep = OpticsExperimentStep.SelectEquipment;
        if (currentStep != OpticsExperimentStep.SelectEquipment) return;
        if (OpticsEquipmentSelectionManager.Instance != null && OpticsEquipmentSelectionManager.Instance.IsCompleteCheck())
        {
            OpticsScoreManager.Instance?.AddScore(5, false);
            AdvanceStep();
        }
        else
        {
            OpticsScoreManager.Instance?.SubtractScore(5);
            OpticsFeedbackManager.Instance?.ShowInstruction("Select all required equipment before continuing.");
        }
    }

    public void OpenWindow()
    {
        if (currentStep != OpticsExperimentStep.Assembly)
        {
            OpticsFeedbackManager.Instance?.ShowInstruction("Open the window during the assembly step.");
            OpticsScoreManager.Instance?.SubtractScore(5);
            return;
        }
        OpticsAssemblyManager.Instance?.OpenWindow();
    }

    public void ConfirmSetup()
    {
        if (currentStep != OpticsExperimentStep.Assembly)
        {
            OpticsFeedbackManager.Instance?.ShowInstruction("Confirm setup during the assembly step.");
            OpticsScoreManager.Instance?.SubtractScore(5);
            return;
        }
        if (OpticsAssemblyManager.Instance != null && OpticsAssemblyManager.Instance.ConfirmSetup())
        {
            currentStep = OpticsExperimentStep.AdjustFocus;
            EnterStep();
            UpdateUI();
        }
    }

    public void ConfirmSharpImage()
    {
        if (currentStep != OpticsExperimentStep.AdjustFocus)
        {
            OpticsFeedbackManager.Instance?.ShowInstruction("Adjust the screen until the image is sharp in this step.");
            return;
        }
        if (OpticsVisualController.Instance != null && OpticsVisualController.Instance.TryConfirmFocus())
        {
            if (!focusScored)
            {
                focusScored = true;
                OpticsScoreManager.Instance?.AddScore(10, false);
            }
            OpticsFeedbackManager.Instance?.ShowMessage(
                "✓ CLEAR IMAGE\nThe image is formed on the screen, so it is a real image. It is upside down. Parallel rays from a distant object meet at the focus.",
                "+10 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            OpticsUIManager.Instance?.SetNextButtonVisible(true);
            OpticsUIManager.Instance?.UpdateLiveReadings();
        }
    }

    public void AnswerImage(int choice)
    {
        if (currentStep != OpticsExperimentStep.IdentifyImage) return;
        if (choice == 2)
        {
            identifiedImage = true;
            if (!identifyScored)
            {
                identifyScored = true;
                OpticsScoreManager.Instance?.AddScore(5, false);
            }
            OpticsFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT\nBecause the image is formed on a screen it is real, and the distant scene appears upside down.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            OpticsUIManager.Instance?.SetNextButtonVisible(true);
        }
        else
        {
            OpticsScoreManager.Instance?.SubtractScore(5);
            OpticsFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nThe image is captured on the white screen, so it is real. Look: the outdoor scene is inverted.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
        }
    }

    public void RecordMeasurement()
    {
        if (currentStep != OpticsExperimentStep.MeasureFocalLength)
        {
            OpticsFeedbackManager.Instance?.ShowInstruction("Record the focal length in the measurement step.");
            return;
        }
        var vis = OpticsVisualController.Instance;
        if (vis == null || !vis.IsInFocus)
        {
            OpticsScoreManager.Instance?.SubtractScore(5);
            OpticsFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nGet a very clear image first. Then the mirror–screen distance is approximately the focal length.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            return;
        }

        measuredFocalLength = true;
        if (!measureScored)
        {
            measureScored = true;
            OpticsScoreManager.Instance?.AddScore(5, false);
        }
        OpticsFeedbackManager.Instance?.ShowMessage(
            $"✓ MEASURED\nDistance = {vis.ScreenDistanceCm:0.0} cm ≈ focal length f. Rays from a far object are parallel, so they meet at F.",
            "+5 MARKS",
            new Color(0.08f, 0.52f, 0.22f));
        OpticsUIManager.Instance?.SetNextButtonVisible(true);
        OpticsUIManager.Instance?.UpdateLiveReadings();
    }

    public void CompletePractical()
    {
        if (completionScored) return;
        completionScored = true;
        int score = OpticsScoreManager.Instance != null ? OpticsScoreManager.Instance.FinalizeScore() : 0;
        bool passed = score >= 50;
        var attempt = OpticsAttemptManager.Instance != null
            ? OpticsAttemptManager.Instance.RegisterAttempt(score, mistakeCount, passed ? "Completed" : "Needs Improvement")
            : null;
        OpticsProfileManager.Instance?.UpdatePracticalResult(score, mistakeCount, passed, attempt);
        OpticsResultManager.Instance?.ShowResult(score, passed, mistakeCount, attempt);
        OpticsUIManager.Instance?.ShowResult();
        OpticsSaveManager.Instance?.Save(OpticsProfileManager.Instance != null ? OpticsProfileManager.Instance.ProfileData : null);
        SendToFlutter(score, passed);
    }

    public void CompleteExperiment()
    {
        if (!completionScored)
        {
            CompletePractical();
            return;
        }

        int score = OpticsScoreManager.Instance != null ? OpticsScoreManager.Instance.GetScore() : 0;
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
            + ",\"identifiedImage\":" + (identifiedImage ? "true" : "false")
            + ",\"measuredFocalLength\":" + (measuredFocalLength ? "true" : "false")
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
        focusScored = false;
        identifyScored = false;
        measureScored = false;
        completionScored = false;
        identifiedImage = false;
        measuredFocalLength = false;
        currentStep = OpticsExperimentStep.Introduction;
        OpticsScoreManager.Instance?.ResetScore();
        OpticsEquipmentSelectionManager.Instance?.ResetSelection();
        OpticsConclusionManager.Instance?.ResetBuilder();
        OpticsVariableMatchingManager.Instance?.ResetMatching();
        OpticsObservationTableManager.Instance?.ResetScoring();
        OpticsAssemblyManager.Instance?.ResetAssembly();
        OpticsEquipmentManager.Instance?.ResetTray();
        UpdateUI();
    }

    public void ResetExperiment() => ResetPractical();

    public void RetryExperiment()
    {
        if (OpticsAttemptManager.Instance != null && !OpticsAttemptManager.Instance.CanRetry())
        {
            OpticsFeedbackManager.Instance?.ShowInstruction("No attempts remaining.");
            return;
        }
        ResetPractical();
        StartPractical();
    }

    private bool CanLeaveCurrentStep()
    {
        switch (currentStep)
        {
            case OpticsExperimentStep.SelectEquipment:
                if (OpticsEquipmentSelectionManager.Instance == null || !OpticsEquipmentSelectionManager.Instance.IsCompleteCheck())
                {
                    OpticsScoreManager.Instance?.SubtractScore(5);
                    OpticsFeedbackManager.Instance?.ShowInstruction("Select all required equipment first.");
                    return false;
                }
                return true;
            case OpticsExperimentStep.Assembly:
                OpticsFeedbackManager.Instance?.ShowInstruction("Place all apparatus, then press CONFIRM SETUP.");
                return false;
            case OpticsExperimentStep.AdjustFocus:
                if (focusScored) return true;
                OpticsScoreManager.Instance?.SubtractScore(5);
                OpticsFeedbackManager.Instance?.ShowInstruction("Move the screen until the image is clear, then press IMAGE IS SHARP.");
                return false;
            case OpticsExperimentStep.IdentifyImage:
                if (identifiedImage) return true;
                OpticsScoreManager.Instance?.SubtractScore(5);
                OpticsFeedbackManager.Instance?.ShowInstruction("Choose the correct description of the image.");
                return false;
            case OpticsExperimentStep.MeasureFocalLength:
                if (measuredFocalLength) return true;
                OpticsScoreManager.Instance?.SubtractScore(5);
                OpticsFeedbackManager.Instance?.ShowInstruction("Press RECORD f to take the mirror–screen distance as the focal length.");
                return false;
            case OpticsExperimentStep.Questions:
                if (OpticsQuestionManager.Instance != null && OpticsQuestionManager.Instance.IsFinished) return true;
                OpticsFeedbackManager.Instance?.ShowInstruction("Select an answer, then press CONTINUE.");
                return false;
            default:
                return true;
        }
    }

    private void EnterStep()
    {
        switch (currentStep)
        {
            case OpticsExperimentStep.Assembly:
                OpticsEquipmentManager.Instance?.ResetTray();
                OpticsAssemblyManager.Instance?.ResetAssembly();
                break;
            case OpticsExperimentStep.ObservationTable:
                OpticsObservationTableManager.Instance?.Refresh();
                break;
            case OpticsExperimentStep.Questions:
                OpticsQuestionManager.Instance?.StartQuiz();
                break;
            case OpticsExperimentStep.VariableMatching:
                OpticsVariableMatchingManager.Instance?.ResetMatching();
                break;
            case OpticsExperimentStep.Conclusion:
                OpticsConclusionManager.Instance?.ResetBuilder();
                break;
        }
    }

    public void UpdateUI() => OpticsUIManager.Instance?.ShowStep(currentStep);
}
