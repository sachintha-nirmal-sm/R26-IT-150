using System.Globalization;
using UnityEngine;

public class WavesExperimentManager : MonoBehaviour
{
    public static WavesExperimentManager Instance { get; private set; }

    [SerializeField] private WavesExperimentStep currentStep = WavesExperimentStep.Introduction;
    [SerializeField] private int totalDisplaySteps = 11;
    [SerializeField] private int mistakeCount;
    private bool flutterSent;
    [SerializeField] private int correctScore = 5;
    [SerializeField] private int wrongPenalty = 5;
    [SerializeField] private int majorStepScore = 10;
    [SerializeField] private int maximumAttempts = 3;
    [SerializeField] private bool motionScored;
    [SerializeField] private bool completionScored;
    [SerializeField] private bool identifiedMotion;
    [SerializeField] private bool shakeScored;
    [SerializeField] private bool observeScored;

    public WavesExperimentStep CurrentStep => currentStep;
    public int MistakeCount => mistakeCount;
    public int TotalDisplaySteps => totalDisplaySteps;
    public bool IdentifiedMotion => identifiedMotion;

    private void Awake() => Instance = this;

    private void Start() => ApplyInspectorSettings();

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void ApplyInspectorSettings()
    {
        WavesScoreManager.Instance?.Configure(correctScore, wrongPenalty, majorStepScore);
        WavesAttemptManager.Instance?.Configure(maximumAttempts);
        int required = WavesEquipmentSelectionManager.Instance != null ? WavesEquipmentSelectionManager.Instance.RequiredCount : 3;
        WavesScoreManager.Instance?.ConfigureMaxRaw(required);
    }

    public void RegisterMistake() => mistakeCount++;

    public void StartPractical()
    {
        mistakeCount = 0;
        flutterSent = false;
        motionScored = false;
        completionScored = false;
        identifiedMotion = false;
        shakeScored = false;
        observeScored = false;
        WavesScoreManager.Instance?.ResetScore();
        WavesEquipmentSelectionManager.Instance?.EnsureCardsVisible();
        WavesEquipmentSelectionManager.Instance?.ResetSelection();
        WavesObservationTableManager.Instance?.ResetScoring();
        WavesConclusionManager.Instance?.ResetBuilder();
        WavesVariableMatchingManager.Instance?.ResetMatching();
        WavesAssemblyManager.Instance?.ResetAssembly();
        WavesWaveController.Instance?.ResetAll();
        WavesEquipmentManager.Instance?.ResetTray();
        ApplyInspectorSettings();
        currentStep = WavesExperimentStep.Objective;
        WavesUIManager.Instance?.HideResult();
        WavesUIManager.Instance?.SetNextButtonVisible(true);
        WavesUIManager.Instance?.UpdateAttemptsDisplay(WavesAttemptManager.Instance != null ? WavesAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateUI();
    }

    public void SetStep(WavesExperimentStep step)
    {
        currentStep = step;
        EnterStep();
        UpdateUI();
        if (currentStep == WavesExperimentStep.Complete)
            CompletePractical();
    }

    public void AdvanceStep()
    {
        if (currentStep >= WavesExperimentStep.Complete) return;
        if (!CanLeaveCurrentStep()) return;
        currentStep++;
        EnterStep();
        UpdateUI();
        if (currentStep == WavesExperimentStep.Complete)
            CompletePractical();
    }

    public void CompleteQuestions()
    {
        currentStep = WavesExperimentStep.VariableMatching;
        EnterStep();
        UpdateUI();
    }

    public void CompleteVariableMatching()
    {
        currentStep = WavesExperimentStep.Conclusion;
        EnterStep();
        UpdateUI();
    }

    public void CompleteConclusion()
    {
        currentStep = WavesExperimentStep.Complete;
        EnterStep();
        UpdateUI();
        CompletePractical();
    }

    public void TryAdvanceFromEquipment()
    {
        if (currentStep == WavesExperimentStep.Introduction || currentStep == WavesExperimentStep.Objective)
            currentStep = WavesExperimentStep.SelectEquipment;
        if (currentStep != WavesExperimentStep.SelectEquipment) return;
        if (WavesEquipmentSelectionManager.Instance != null && WavesEquipmentSelectionManager.Instance.IsCompleteCheck())
        {
            WavesScoreManager.Instance?.AddScore(5, false);
            AdvanceStep();
        }
        else
        {
            WavesScoreManager.Instance?.SubtractScore(5);
            WavesFeedbackManager.Instance?.ShowInstruction("Select all required equipment before continuing.");
        }
    }

    public void ConfirmSetup()
    {
        if (currentStep != WavesExperimentStep.Assembly)
        {
            WavesFeedbackManager.Instance?.ShowInstruction("Confirm setup during the assembly step.");
            WavesScoreManager.Instance?.SubtractScore(5);
            return;
        }
        if (WavesAssemblyManager.Instance != null && WavesAssemblyManager.Instance.ConfirmSetup())
        {
            currentStep = WavesExperimentStep.GenerateWave;
            EnterStep();
            UpdateUI();
        }
    }

    public void ShakeSideToSide()
    {
        if (currentStep != WavesExperimentStep.GenerateWave)
        {
            WavesFeedbackManager.Instance?.ShowInstruction("Shake the slinky in the generate-wave step.");
            return;
        }
        if (WavesWaveController.Instance != null && WavesWaveController.Instance.TryShakeTransverse())
        {
            if (!shakeScored)
            {
                shakeScored = true;
                WavesScoreManager.Instance?.AddScore(5, false);
            }
            WavesVisualController.Instance?.ShowWaveTravel(true);
            WavesFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT\nSide-to-side shaking on the table produces a transverse wave. Energy travels along the slinky.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            WavesUIManager.Instance?.SetNextButtonVisible(true);
            WavesUIManager.Instance?.UpdateLiveReadings();
        }
    }

    public void ShakePushPull()
    {
        if (currentStep != WavesExperimentStep.GenerateWave)
        {
            WavesFeedbackManager.Instance?.ShowInstruction("Choose the shaking method in the generate-wave step.");
            return;
        }
        WavesWaveController.Instance?.TryShakeLongitudinal();
        WavesScoreManager.Instance?.SubtractScore(5);
        WavesFeedbackManager.Instance?.ShowMessage(
            "✗ INCORRECT\nPushing and pulling along the slinky would make a longitudinal wave. Shake from side to side.",
            "-5 MARKS",
            new Color(0.75f, 0.12f, 0.12f));
        WavesUIManager.Instance?.UpdateLiveReadings();
    }

    public void ObserveRibbons()
    {
        if (currentStep != WavesExperimentStep.ObserveRibbons && currentStep != WavesExperimentStep.GenerateWave)
        {
            WavesFeedbackManager.Instance?.ShowInstruction("Observe the ribbons after the wave is generated.");
            return;
        }
        if (WavesWaveController.Instance == null || !WavesWaveController.Instance.HasTransverseWave)
        {
            WavesScoreManager.Instance?.SubtractScore(5);
            WavesFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nGenerate a transverse wave first by shaking the slinky side to side.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            return;
        }
        if (!observeScored)
        {
            observeScored = true;
            WavesScoreManager.Instance?.AddScore(5, false);
        }
        WavesFeedbackManager.Instance?.ShowMessage(
            "✓ OBSERVED\nThe ribbons move left and right (across the table) while the wave travels along the slinky.",
            "+5 MARKS",
            new Color(0.08f, 0.52f, 0.22f));
        currentStep = WavesExperimentStep.IdentifyMotion;
        EnterStep();
        UpdateUI();
    }

    public void AnswerMotion(int choice)
    {
        if (currentStep != WavesExperimentStep.IdentifyMotion) return;
        if (choice == 2)
        {
            identifiedMotion = true;
            if (!motionScored)
            {
                motionScored = true;
                WavesScoreManager.Instance?.AddScore(5, false);
            }
            WavesFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT\nIn a transverse wave the particles (ribbons) move perpendicular to the direction of the wave.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            WavesUIManager.Instance?.SetNextButtonVisible(true);
        }
        else
        {
            WavesScoreManager.Instance?.SubtractScore(5);
            WavesFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nWatch the ribbons. They move across the table, not along the slinky with the wave.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
        }
    }

    public void CompletePractical()
    {
        if (completionScored) return;
        completionScored = true;
        int score = WavesScoreManager.Instance != null ? WavesScoreManager.Instance.FinalizeScore() : 0;
        bool passed = score >= 50;
        var attempt = WavesAttemptManager.Instance != null
            ? WavesAttemptManager.Instance.RegisterAttempt(score, mistakeCount, passed ? "Completed" : "Needs Improvement")
            : null;
        WavesProfileManager.Instance?.UpdatePracticalResult(score, mistakeCount, passed, attempt);
        WavesResultManager.Instance?.ShowResult(score, passed, mistakeCount, attempt);
        WavesUIManager.Instance?.ShowResult();
        WavesSaveManager.Instance?.Save(WavesProfileManager.Instance != null ? WavesProfileManager.Instance.ProfileData : null);
        SendToFlutter(score, passed);
    }

    public void CompleteExperiment()
    {
        if (!completionScored)
        {
            CompletePractical();
            return;
        }

        int score = WavesScoreManager.Instance != null ? WavesScoreManager.Instance.GetScore() : 0;
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
            + ",\"identifiedMotion\":" + (identifiedMotion ? "true" : "false")
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
        motionScored = false;
        completionScored = false;
        identifiedMotion = false;
        shakeScored = false;
        observeScored = false;
        currentStep = WavesExperimentStep.Introduction;
        WavesScoreManager.Instance?.ResetScore();
        WavesEquipmentSelectionManager.Instance?.ResetSelection();
        WavesConclusionManager.Instance?.ResetBuilder();
        WavesVariableMatchingManager.Instance?.ResetMatching();
        WavesObservationTableManager.Instance?.ResetScoring();
        WavesAssemblyManager.Instance?.ResetAssembly();
        WavesWaveController.Instance?.ResetAll();
        WavesEquipmentManager.Instance?.ResetTray();
        UpdateUI();
    }

    public void ResetExperiment() => ResetPractical();

    public void RetryExperiment()
    {
        if (WavesAttemptManager.Instance != null && !WavesAttemptManager.Instance.CanRetry())
        {
            WavesFeedbackManager.Instance?.ShowInstruction("No attempts remaining.");
            return;
        }
        ResetPractical();
        StartPractical();
    }

    private bool CanLeaveCurrentStep()
    {
        switch (currentStep)
        {
            case WavesExperimentStep.SelectEquipment:
                if (WavesEquipmentSelectionManager.Instance == null || !WavesEquipmentSelectionManager.Instance.IsCompleteCheck())
                {
                    WavesScoreManager.Instance?.SubtractScore(5);
                    WavesFeedbackManager.Instance?.ShowInstruction("Select all required equipment first.");
                    return false;
                }
                return true;
            case WavesExperimentStep.Assembly:
                WavesFeedbackManager.Instance?.ShowInstruction("Place all apparatus, then press CONFIRM SETUP.");
                return false;
            case WavesExperimentStep.GenerateWave:
                if (WavesWaveController.Instance != null && WavesWaveController.Instance.HasTransverseWave)
                    return true;
                WavesScoreManager.Instance?.SubtractScore(5);
                WavesFeedbackManager.Instance?.ShowInstruction("Shake the slinky from side to side to form a transverse wave.");
                return false;
            case WavesExperimentStep.ObserveRibbons:
                if (observeScored) return true;
                WavesScoreManager.Instance?.SubtractScore(5);
                WavesFeedbackManager.Instance?.ShowInstruction("Watch the ribbons, then press OBSERVE RIBBONS.");
                return false;
            case WavesExperimentStep.IdentifyMotion:
                if (identifiedMotion) return true;
                WavesScoreManager.Instance?.SubtractScore(5);
                WavesFeedbackManager.Instance?.ShowInstruction("Choose how the ribbons move relative to the wave.");
                return false;
            case WavesExperimentStep.Questions:
                if (WavesQuestionManager.Instance != null && WavesQuestionManager.Instance.IsFinished) return true;
                WavesFeedbackManager.Instance?.ShowInstruction("Select an answer, then press CONTINUE.");
                return false;
            default:
                return true;
        }
    }

    private void EnterStep()
    {
        switch (currentStep)
        {
            case WavesExperimentStep.Assembly:
                WavesEquipmentManager.Instance?.ResetTray();
                WavesAssemblyManager.Instance?.ResetAssembly();
                WavesWaveController.Instance?.ResetAll();
                break;
            case WavesExperimentStep.GenerateWave:
                if (WavesWaveController.Instance != null && !WavesWaveController.Instance.HasTransverseWave)
                    WavesVisualController.Instance?.ShowWaveTravel(false);
                break;
            case WavesExperimentStep.ObserveRibbons:
                if (WavesWaveController.Instance != null && WavesWaveController.Instance.HasTransverseWave)
                    WavesVisualController.Instance?.ShowWaveTravel(true);
                break;
            case WavesExperimentStep.ObservationTable:
                WavesObservationTableManager.Instance?.Refresh();
                break;
            case WavesExperimentStep.Questions:
                WavesQuestionManager.Instance?.StartQuiz();
                break;
            case WavesExperimentStep.VariableMatching:
                WavesVariableMatchingManager.Instance?.ResetMatching();
                break;
            case WavesExperimentStep.Conclusion:
                WavesConclusionManager.Instance?.ResetBuilder();
                break;
        }
    }

    public void UpdateUI() => WavesUIManager.Instance?.ShowStep(currentStep);
}
