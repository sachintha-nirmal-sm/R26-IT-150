using System.Globalization;
using UnityEngine;

public class ElectronicsPracticalManager : MonoBehaviour
{
    public static ElectronicsPracticalManager Instance { get; private set; }

    [SerializeField] private ElectronicsPracticalStep currentStep = ElectronicsPracticalStep.Introduction;
    [SerializeField] private int mistakeCount;
    [SerializeField] private int maximumAttempts = 3;
    [SerializeField] private bool completionScored;
    private bool flutterSent;

    private bool batteryDisconnected;
    private bool batteryReversedThisAttempt;
    private bool batteryReconnected;

    public ElectronicsPracticalStep CurrentStep => currentStep;
    public int MistakeCount => mistakeCount;

    private void Awake() => Instance = this;
    private void Start() => ElectronicsAttemptManager.Instance?.Configure(maximumAttempts);

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void RegisterMistake() => mistakeCount++;

    public void StartPractical()
    {
        mistakeCount = 0;
        flutterSent = false;
        completionScored = false;
        batteryDisconnected = false;
        batteryReversedThisAttempt = false;
        batteryReconnected = false;
        ElectronicsScoreManager.Instance?.ResetScore();
        ElectronicsEquipmentSelectionManager.Instance?.EnsureCardsVisible();
        ElectronicsEquipmentSelectionManager.Instance?.ResetSelection();
        ElectronicsObservationManager.Instance?.ResetScoring();
        ElectronicsConclusionManager.Instance?.ResetBuilder();
        ElectronicsFormulaMatchingManager.Instance?.ResetMatching();
        ElectronicsCircuitConnectionManager.Instance?.ResetCircuit();
        ElectronicsForwardBiasController.Instance?.ResetState();
        ElectronicsReverseBiasController.Instance?.ResetState();
        ElectronicsMiniChallengeManager.Instance?.ResetState();
        ElectronicsAttemptManager.Instance?.Configure(maximumAttempts);
        currentStep = ElectronicsPracticalStep.Theory;
        ElectronicsUIManager.Instance?.HideResult();
        ElectronicsUIManager.Instance?.SetNextButtonVisible(true);
        int attempt = ElectronicsAttemptManager.Instance != null ? ElectronicsAttemptManager.Instance.CurrentAttemptNumber : 1;
        ElectronicsUIManager.Instance?.UpdateAttemptsDisplay(attempt, maximumAttempts);
        UpdateUI();
    }

    public void SetStep(ElectronicsPracticalStep step)
    {
        currentStep = step;
        EnterStep();
        UpdateUI();
        if (currentStep == ElectronicsPracticalStep.Result)
            CompletePractical();
    }

    public void AdvanceStep()
    {
        if (currentStep >= ElectronicsPracticalStep.Result) return;
        if (!CanLeaveCurrentStep()) return;
        currentStep++;
        EnterStep();
        UpdateUI();
        if (currentStep == ElectronicsPracticalStep.Result)
            CompletePractical();
    }

    public void TryAdvanceFromEquipment()
    {
        if (currentStep != ElectronicsPracticalStep.EquipmentSelection) return;
        if (ElectronicsEquipmentSelectionManager.Instance != null && ElectronicsEquipmentSelectionManager.Instance.IsCompleteCheck())
            AdvanceStep();
        else
        {
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            ElectronicsFeedbackManager.Instance?.ShowInstruction("Select all required equipment before continuing.");
        }
    }

    public void CompleteQuestions()
    {
        currentStep = ElectronicsPracticalStep.Conclusion;
        EnterStep();
        UpdateUI();
    }

    public void CompleteConclusion()
    {
        if (ElectronicsConclusionManager.Instance != null && !ElectronicsConclusionManager.Instance.IsCorrect)
        {
            ElectronicsFeedbackManager.Instance?.ShowInstruction("Arrange the three conclusion sentences first.");
            return;
        }
        currentStep = ElectronicsPracticalStep.Result;
        EnterStep();
        UpdateUI();
        CompletePractical();
    }

    public void DisconnectBattery()
    {
        if (currentStep != ElectronicsPracticalStep.BatteryDisconnect && currentStep != ElectronicsPracticalStep.BatteryReverse)
        {
            ElectronicsFeedbackManager.Instance?.ShowInstruction("Disconnect only the battery at the reverse-bias step.");
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            return;
        }
        if (ElectronicsSwitchController.Instance != null && ElectronicsSwitchController.Instance.IsOn())
        {
            ElectronicsFeedbackManager.Instance?.ShowInstruction("Turn OFF the switch before disconnecting the battery.");
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            return;
        }
        ElectronicsBatteryController.Instance?.Disconnect();
        if (!batteryDisconnected)
        {
            batteryDisconnected = true;
            ElectronicsScoreManager.Instance?.AddToCategory(ElectronicsScoreCategory.BatteryReverse, 5, false);
            ElectronicsFeedbackManager.Instance?.ShowMessage("✓ Disconnect only the battery.", "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
    }

    public void ReverseBattery()
    {
        if (!batteryDisconnected)
        {
            ElectronicsFeedbackManager.Instance?.ShowInstruction("Disconnect the battery first, then reverse it.");
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            return;
        }
        ElectronicsBatteryController.Instance?.ReversePolarity();
        if (!batteryReversedThisAttempt)
        {
            batteryReversedThisAttempt = true;
            ElectronicsScoreManager.Instance?.AddToCategory(ElectronicsScoreCategory.BatteryReverse, 3, false);
            ElectronicsFeedbackManager.Instance?.ShowMessage("✓ Battery polarity reversed.", "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
    }

    public void ReconnectBattery()
    {
        if (!batteryReversedThisAttempt)
        {
            ElectronicsFeedbackManager.Instance?.ShowInstruction("Reverse the battery before reconnecting.");
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            return;
        }
        ElectronicsBatteryController.Instance?.Reconnect();
        if (!batteryReconnected)
        {
            batteryReconnected = true;
            ElectronicsScoreManager.Instance?.AddToCategory(ElectronicsScoreCategory.BatteryReverse, 2, false);
            ElectronicsFeedbackManager.Instance?.ShowMessage("✓ Battery reconnected in reverse polarity.", "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
            ElectronicsUIManager.Instance?.SetNextButtonVisible(true);
        }
        ElectronicsCircuitConnectionManager.Instance?.RefreshState();
    }

    public void CompletePractical()
    {
        if (completionScored) return;
        completionScored = true;
        int score = ElectronicsScoreManager.Instance != null ? ElectronicsScoreManager.Instance.FinalizeScore() : 0;
        bool passed = score >= 50;
        var attempt = ElectronicsAttemptManager.Instance != null
            ? ElectronicsAttemptManager.Instance.RegisterAttempt(score, mistakeCount, passed ? "Completed" : "Needs Improvement")
            : null;
        ElectronicsProfileManager.Instance?.UpdatePracticalResult(score, mistakeCount, passed, attempt);
        ElectronicsResultManager.Instance?.ShowResult(score, passed, mistakeCount, attempt);
        ElectronicsUIManager.Instance?.ShowResult();
        ElectronicsSaveManager.Instance?.Save(ElectronicsProfileManager.Instance != null ? ElectronicsProfileManager.Instance.ProfileData : null);
        SendToFlutter(score, passed);
    }

    public void CompleteExperiment()
    {
        if (!completionScored)
        {
            CompletePractical();
            return;
        }

        int score = ElectronicsScoreManager.Instance != null ? ElectronicsScoreManager.Instance.GetScore() : 0;
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
            "{\"mistakes\":" + mistakeCount.ToString(CultureInfo.InvariantCulture) + "}";
        FlutterBridge.EnsureInstance()?.NotifyCompleted(
            score,
            passed,
            mistakeCount,
            timeUsed,
            true,
            measurements);
    }

    public void ResetCurrentExperiment()
    {
        batteryDisconnected = false;
        batteryReversedThisAttempt = false;
        batteryReconnected = false;
        ElectronicsBatteryController.Instance?.ResetPolarity();
        ElectronicsBatteryController.Instance?.Reconnect();
        ElectronicsDiodeController.Instance?.ResetDiode();
        if (ElectronicsCircuitConnectionManager.Instance != null && ElectronicsCircuitConnectionManager.Instance.HasComponent("Diode"))
            ElectronicsDiodeController.Instance?.SetPlaced(true);
        if (ElectronicsCircuitConnectionManager.Instance != null && ElectronicsCircuitConnectionManager.Instance.HasComponent("Battery"))
            ElectronicsBatteryController.Instance?.SetPlaced(true);
        ElectronicsBulbController.Instance?.TurnOff();
        ElectronicsSwitchController.Instance?.ForceOff();
        ElectronicsCircuitConnectionManager.Instance?.RefreshState();
        ElectronicsObservationManager.Instance?.ResetScoring();
        ElectronicsUIManager.Instance?.HideResult();
        UpdateUI();
    }

    public void ResetEntirePractical()
    {
        mistakeCount = 0;
        flutterSent = false;
        completionScored = false;
        batteryDisconnected = false;
        batteryReversedThisAttempt = false;
        batteryReconnected = false;
        currentStep = ElectronicsPracticalStep.Introduction;
        ElectronicsScoreManager.Instance?.ResetScore();
        ElectronicsProgressManager.Instance?.ResetProgress();
        ElectronicsEquipmentSelectionManager.Instance?.ResetSelection();
        ElectronicsConclusionManager.Instance?.ResetBuilder();
        ElectronicsFormulaMatchingManager.Instance?.ResetMatching();
        ElectronicsObservationManager.Instance?.ResetScoring();
        ElectronicsCircuitConnectionManager.Instance?.ResetCircuit();
        ElectronicsForwardBiasController.Instance?.ResetState();
        ElectronicsReverseBiasController.Instance?.ResetState();
        ElectronicsMiniChallengeManager.Instance?.ResetState();
        UpdateUI();
    }

    public void RetryExperiment()
    {
        if (ElectronicsAttemptManager.Instance != null && !ElectronicsAttemptManager.Instance.CanRetry())
        {
            ElectronicsFeedbackManager.Instance?.ShowInstruction("No attempts remaining. Best score has been saved.");
            return;
        }
        ResetEntirePractical();
        StartPractical();
    }

    private bool CanLeaveCurrentStep()
    {
        switch (currentStep)
        {
            case ElectronicsPracticalStep.EquipmentSelection:
                if (ElectronicsEquipmentSelectionManager.Instance == null || !ElectronicsEquipmentSelectionManager.Instance.IsCompleteCheck())
                {
                    ElectronicsScoreManager.Instance?.SubtractScore(3);
                    ElectronicsFeedbackManager.Instance?.ShowInstruction("Select all required equipment first.");
                    return false;
                }
                return true;
            case ElectronicsPracticalStep.CircuitSetup:
                if (ElectronicsCircuitConnectionManager.Instance != null && ElectronicsCircuitConnectionManager.Instance.IsCircuitComplete())
                    return true;
                ElectronicsScoreManager.Instance?.SubtractScore(3);
                ElectronicsFeedbackManager.Instance?.ShowInstruction("Place the board, battery, switch, diode, bulb and connect all four wires.");
                return false;
            case ElectronicsPracticalStep.ForwardBias:
                if (ElectronicsForwardBiasController.Instance != null && ElectronicsForwardBiasController.Instance.GlowScored) return true;
                ElectronicsFeedbackManager.Instance?.ShowInstruction("Turn ON the switch and observe the glowing bulb first.");
                return false;
            case ElectronicsPracticalStep.ForwardObservation:
                if (ElectronicsObservationManager.Instance != null && ElectronicsObservationManager.Instance.Forward != null &&
                    !string.IsNullOrEmpty(ElectronicsObservationManager.Instance.Forward.observationText))
                    return true;
                ElectronicsFeedbackManager.Instance?.ShowInstruction("Select the bulb observation for forward bias.");
                return false;
            case ElectronicsPracticalStep.ReverseObservation:
                if (ElectronicsObservationManager.Instance != null && ElectronicsObservationManager.Instance.Reverse != null &&
                    !string.IsNullOrEmpty(ElectronicsObservationManager.Instance.Reverse.observationText))
                    return true;
                ElectronicsFeedbackManager.Instance?.ShowInstruction("Select the bulb observation for reverse bias.");
                return false;
            case ElectronicsPracticalStep.BatteryDisconnect:
                if (batteryDisconnected) return true;
                ElectronicsScoreManager.Instance?.SubtractScore(3);
                ElectronicsFeedbackManager.Instance?.ShowInstruction("Disconnect only the battery.");
                return false;
            case ElectronicsPracticalStep.BatteryReverse:
                if (batteryReconnected && ElectronicsBatteryController.Instance != null && ElectronicsBatteryController.Instance.IsReversedPolarity())
                    return true;
                ElectronicsFeedbackManager.Instance?.ShowInstruction("Reverse the battery and reconnect it.");
                return false;
            case ElectronicsPracticalStep.ReverseBias:
                if (ElectronicsReverseBiasController.Instance != null && ElectronicsReverseBiasController.Instance.DarkScored) return true;
                ElectronicsFeedbackManager.Instance?.ShowInstruction("Turn ON the switch and observe that the bulb does not glow.");
                return false;
            case ElectronicsPracticalStep.Comparison:
                if (ElectronicsComparisonManager.Instance != null && ElectronicsComparisonManager.Instance.IsFinished) return true;
                ElectronicsFeedbackManager.Instance?.ShowInstruction("Complete the comparison table first.");
                return false;
            case ElectronicsPracticalStep.Matching:
                if (ElectronicsFormulaMatchingManager.Instance != null && ElectronicsFormulaMatchingManager.Instance.IsComplete) return true;
                ElectronicsFeedbackManager.Instance?.ShowInstruction("Match all six ideas first.");
                return false;
            case ElectronicsPracticalStep.Challenge:
                if (ElectronicsMiniChallengeManager.Instance != null && ElectronicsMiniChallengeManager.Instance.IsComplete) return true;
                ElectronicsFeedbackManager.Instance?.ShowInstruction("Make the bulb glow with a forward-biased circuit.");
                return false;
            case ElectronicsPracticalStep.Questions:
                if (ElectronicsQuestionManager.Instance != null && ElectronicsQuestionManager.Instance.IsFinished) return true;
                ElectronicsFeedbackManager.Instance?.ShowInstruction("Answer the question, then press CONTINUE.");
                return false;
            default:
                return true;
        }
    }

    private void EnterStep()
    {
        switch (currentStep)
        {
            case ElectronicsPracticalStep.CircuitSetup:
                ElectronicsCircuitConnectionManager.Instance?.ResetCircuit();
                break;
            case ElectronicsPracticalStep.ForwardBias:
                ElectronicsForwardBiasController.Instance?.Begin();
                break;
            case ElectronicsPracticalStep.ForwardObservation:
                ElectronicsSwitchController.Instance?.ForceOff();
                ElectronicsObservationManager.Instance?.StartForwardObservation();
                break;
            case ElectronicsPracticalStep.BatteryDisconnect:
                ElectronicsSwitchController.Instance?.ForceOff();
                ElectronicsFeedbackManager.Instance?.ShowInstruction("Disconnect only the battery.");
                break;
            case ElectronicsPracticalStep.BatteryReverse:
                ElectronicsFeedbackManager.Instance?.ShowInstruction("Rotate the battery 180 degrees.");
                break;
            case ElectronicsPracticalStep.ReverseBias:
                ElectronicsReverseBiasController.Instance?.Begin();
                break;
            case ElectronicsPracticalStep.ReverseObservation:
                ElectronicsSwitchController.Instance?.ForceOff();
                ElectronicsObservationManager.Instance?.StartReverseObservation();
                break;
            case ElectronicsPracticalStep.Comparison:
                ElectronicsComparisonManager.Instance?.StartComparison();
                break;
            case ElectronicsPracticalStep.Matching:
                ElectronicsFormulaMatchingManager.Instance?.ResetMatching();
                break;
            case ElectronicsPracticalStep.Challenge:
                ElectronicsMiniChallengeManager.Instance?.Begin();
                break;
            case ElectronicsPracticalStep.Questions:
                ElectronicsQuestionManager.Instance?.StartQuiz();
                break;
            case ElectronicsPracticalStep.Conclusion:
                ElectronicsConclusionManager.Instance?.ResetBuilder();
                ElectronicsConclusionManager.Instance?.BindPhrases(Object.FindAnyObjectByType<ElectronicsUIRefs>()?.PhraseButtons);
                break;
        }
    }

    public void UpdateUI() => ElectronicsUIManager.Instance?.ShowStep(currentStep);
}
