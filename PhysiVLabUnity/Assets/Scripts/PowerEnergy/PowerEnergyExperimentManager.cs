using System.Globalization;
using UnityEngine;

public enum PowerEnergyExperimentPhase
{
    SelectAppliance,
    TakeReadings,
    CalculatePower,
    SelectTime,
    RunTimer,
    CalculateEnergy,
    ConvertKwh,
    Recorded
}

public class PowerEnergyExperimentManager : MonoBehaviour
{
    public static PowerEnergyExperimentManager Instance { get; private set; }

    [SerializeField] private PowerEnergyExperimentStep currentStep = PowerEnergyExperimentStep.Introduction;
    [SerializeField] private PowerEnergyExperimentPhase experimentPhase = PowerEnergyExperimentPhase.SelectAppliance;
    [SerializeField] private int totalDisplaySteps = 13;
    [SerializeField] private int mistakeCount;
    private bool flutterSent;
    [SerializeField] private int maximumAttempts = 3;
    [SerializeField] private bool completionScored;

    public PowerEnergyExperimentStep CurrentStep => currentStep;
    public PowerEnergyExperimentPhase Phase => experimentPhase;
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
        PowerEnergyAttemptManager.Instance?.Configure(maximumAttempts);
    }

    public void RegisterMistake() => mistakeCount++;

    public void StartPractical()
    {
        mistakeCount = 0;
        flutterSent = false;
        completionScored = false;
        experimentPhase = PowerEnergyExperimentPhase.SelectAppliance;
        PowerEnergyScoreManager.Instance?.ResetScore();
        PowerEnergyEquipmentSelectionManager.Instance?.EnsureCardsVisible();
        PowerEnergyEquipmentSelectionManager.Instance?.ResetSelection();
        PowerEnergyObservationTableManager.Instance?.ResetScoring();
        PowerEnergyConclusionManager.Instance?.ResetBuilder();
        PowerEnergyFormulaMatchingManager.Instance?.ResetMatching();
        PowerEnergyCircuitConnectionManager.Instance?.ResetCircuit();
        PowerEnergyApplianceController.Instance?.ResetAll();
        ApplyInspectorSettings();
        currentStep = PowerEnergyExperimentStep.Objective;
        PowerEnergyUIManager.Instance?.HideResult();
        PowerEnergyUIManager.Instance?.SetNextButtonVisible(true);
        PowerEnergyUIManager.Instance?.UpdateAttemptsDisplay(PowerEnergyAttemptManager.Instance != null ? PowerEnergyAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateUI();
    }

    public void SetStep(PowerEnergyExperimentStep step)
    {
        currentStep = step;
        EnterStep();
        UpdateUI();
        if (currentStep == PowerEnergyExperimentStep.Complete)
            CompletePractical();
    }

    public void AdvanceStep()
    {
        if (currentStep >= PowerEnergyExperimentStep.Complete) return;
        if (!CanLeaveCurrentStep()) return;
        currentStep++;
        EnterStep();
        UpdateUI();
        if (currentStep == PowerEnergyExperimentStep.Complete)
            CompletePractical();
    }

    public void TryAdvanceFromEquipment()
    {
        if (currentStep == PowerEnergyExperimentStep.Introduction || currentStep == PowerEnergyExperimentStep.Objective)
            currentStep = PowerEnergyExperimentStep.SelectEquipment;
        if (currentStep != PowerEnergyExperimentStep.SelectEquipment) return;
        if (PowerEnergyEquipmentSelectionManager.Instance != null && PowerEnergyEquipmentSelectionManager.Instance.IsCompleteCheck())
            AdvanceStep();
        else
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("Select all required equipment before continuing.");
        }
    }

    public void CompleteQuestions()
    {
        currentStep = PowerEnergyExperimentStep.Conclusion;
        EnterStep();
        UpdateUI();
    }

    public void CompleteVariables()
    {
        currentStep = PowerEnergyExperimentStep.Questions;
        EnterStep();
        UpdateUI();
    }

    public void CompleteConclusion()
    {
        currentStep = PowerEnergyExperimentStep.Complete;
        EnterStep();
        UpdateUI();
        CompletePractical();
    }

    public void SetPhase(PowerEnergyExperimentPhase phase)
    {
        experimentPhase = phase;
        UpdateUI();
        PowerEnergyUIManager.Instance?.ShowPowerEnergyExperimentPhase(phase);
    }

    public void SubmitPower(float value)
    {
        var app = PowerEnergyApplianceController.Instance != null ? PowerEnergyApplianceController.Instance.Current : null;
        if (app == null)
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("Select an appliance first.");
            return;
        }
        if (PowerEnergyVoltmeterController.Instance == null || !PowerEnergyVoltmeterController.Instance.ReadingTaken ||
            PowerEnergyAmmeterController.Instance == null || !PowerEnergyAmmeterController.Instance.ReadingTaken)
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("Take voltage and current readings first.");
            return;
        }
        if (app.powerCalculated)
        {
            SetPhase(PowerEnergyExperimentPhase.SelectTime);
            return;
        }
        bool ok = PowerEnergyPowerCalculator.Instance != null &&
                  PowerEnergyPowerCalculator.Instance.IsCorrect(app.voltage, app.current, value, app.displayPower);
        if (ok)
        {
            app.studentPower = value;
            app.powerCalculated = true;
            app.power = app.voltage * app.current;
            PowerEnergyScoreManager.Instance?.AddToCategory(PowerEnergyScoreCategory.Power, 10, false);
            string work = PowerEnergyPowerCalculator.Instance.FormatWorkedExample(app.voltage, app.current);
            PowerEnergyFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT\nPower is the rate at which electrical energy is consumed.\n" + work + "\nUnit: Watt (W).  1 W = 1 J/s",
                "+10 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            SetPhase(PowerEnergyExperimentPhase.SelectTime);
        }
        else
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nUse P = VI. Multiply the voltage by the current.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
        }
    }

    public void SubmitEnergy(float value)
    {
        var app = PowerEnergyApplianceController.Instance != null ? PowerEnergyApplianceController.Instance.Current : null;
        if (app == null || !app.powerCalculated)
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("Calculate power first.");
            return;
        }
        if (PowerEnergyTimerController.Instance == null || (!PowerEnergyTimerController.Instance.IsFinished && app.operatingTime < 1f))
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("Choose a time and let the timer finish first.");
            return;
        }
        float t = app.operatingTime > 0 ? app.operatingTime : PowerEnergyTimerController.Instance.TargetDuration;
        if (app.energyCalculated)
        {
            SetPhase(PowerEnergyExperimentPhase.ConvertKwh);
            return;
        }
        bool ok = PowerEnergyEnergyCalculator.Instance != null &&
                  PowerEnergyEnergyCalculator.Instance.IsCorrect(app.power, app.displayPower, t, value);
        if (ok)
        {
            app.studentEnergyJoules = value;
            app.energyJoules = (app.studentPower > 0 ? app.studentPower : app.displayPower) * t;
            app.energyCalculated = true;
            PowerEnergyScoreManager.Instance?.AddToCategory(PowerEnergyScoreCategory.Energy, 10, false);
            PowerEnergyFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT\n" + PowerEnergyEnergyCalculator.Instance.FormatWorkedExample(app.displayPower, t),
                "+10 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            SetPhase(PowerEnergyExperimentPhase.ConvertKwh);
        }
        else
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nElectrical energy is E = Pt. Multiply power by time in seconds.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
        }
    }

    public void SubmitKwh(float value)
    {
        var app = PowerEnergyApplianceController.Instance != null ? PowerEnergyApplianceController.Instance.Current : null;
        if (app == null || !app.energyCalculated)
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("Calculate energy in joules first.");
            return;
        }
        float joules = app.studentEnergyJoules > 0 ? app.studentEnergyJoules : app.energyJoules;
        if (app.kwhConverted)
        {
            SetPhase(PowerEnergyExperimentPhase.Recorded);
            return;
        }
        bool ok = PowerEnergyKwhConverter.Instance != null && PowerEnergyKwhConverter.Instance.IsCorrect(joules, value);
        if (ok)
        {
            app.studentEnergyKwh = value;
            app.energyKwh = PowerEnergyKwhConverter.Instance.ConvertJoulesToKwh(joules);
            app.kwhConverted = true;
            app.completed = true;
            PowerEnergyScoreManager.Instance?.AddToCategory(PowerEnergyScoreCategory.Kwh, 10, false);
            PowerEnergyFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT\n" + PowerEnergyKwhConverter.Instance.FormatWorkedExample(joules),
                "+10 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            PowerEnergyObservationTableManager.Instance?.Refresh();
            SetPhase(PowerEnergyExperimentPhase.Recorded);
        }
        else
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nEnergy in kWh = Energy in J / 3,600,000.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
        }
    }

    public void InvestigateAnother()
    {
        PowerEnergyApplianceController.Instance?.ResetCurrentAppliance();
        experimentPhase = PowerEnergyExperimentPhase.SelectAppliance;
        UpdateUI();
        PowerEnergyUIManager.Instance?.ShowPowerEnergyExperimentPhase(experimentPhase);
    }

    public void CompletePractical()
    {
        if (completionScored) return;
        completionScored = true;
        int score = PowerEnergyScoreManager.Instance != null ? PowerEnergyScoreManager.Instance.FinalizeScore() : 0;
        bool passed = score >= 50;
        var attempt = PowerEnergyAttemptManager.Instance != null
            ? PowerEnergyAttemptManager.Instance.RegisterAttempt(score, mistakeCount, passed ? "Completed" : "Needs Improvement")
            : null;
        PowerEnergyProfileManager.Instance?.UpdatePracticalResult(score, mistakeCount, passed, attempt);
        PowerEnergyResultManager.Instance?.ShowResult(score, passed, mistakeCount, attempt);
        PowerEnergyUIManager.Instance?.ShowResult();
        PowerEnergySaveManager.Instance?.Save(PowerEnergyProfileManager.Instance != null ? PowerEnergyProfileManager.Instance.ProfileData : null);
        SendToFlutter(score, passed);
    }

    public void CompleteExperiment()
    {
        if (!completionScored)
        {
            CompletePractical();
            return;
        }

        int score = PowerEnergyScoreManager.Instance != null ? PowerEnergyScoreManager.Instance.GetScore() : 0;
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

    public void ResetPractical()
    {
        mistakeCount = 0;
        flutterSent = false;
        completionScored = false;
        experimentPhase = PowerEnergyExperimentPhase.SelectAppliance;
        currentStep = PowerEnergyExperimentStep.Introduction;
        PowerEnergyScoreManager.Instance?.ResetScore();
        PowerEnergyEquipmentSelectionManager.Instance?.ResetSelection();
        PowerEnergyConclusionManager.Instance?.ResetBuilder();
        PowerEnergyFormulaMatchingManager.Instance?.ResetMatching();
        PowerEnergyObservationTableManager.Instance?.ResetScoring();
        PowerEnergyCircuitConnectionManager.Instance?.ResetCircuit();
        PowerEnergyApplianceController.Instance?.ResetAll();
        PowerEnergyTimerController.Instance?.ResetTimer();
        PowerEnergyVoltmeterController.Instance?.ResetMeter();
        PowerEnergyAmmeterController.Instance?.ResetMeter();
        UpdateUI();
    }

    public void ResetExperiment() => ResetPractical();

    public void ResetCurrentAppliance() => PowerEnergyApplianceController.Instance?.ResetCurrentAppliance();

    public void RetryExperiment()
    {
        if (PowerEnergyAttemptManager.Instance != null && !PowerEnergyAttemptManager.Instance.CanRetry())
        {
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("No attempts remaining. Best score has been saved.");
            return;
        }
        ResetPractical();
        StartPractical();
    }

    private bool CanLeaveCurrentStep()
    {
        switch (currentStep)
        {
            case PowerEnergyExperimentStep.SelectEquipment:
                if (PowerEnergyEquipmentSelectionManager.Instance == null || !PowerEnergyEquipmentSelectionManager.Instance.IsCompleteCheck())
                {
                    PowerEnergyScoreManager.Instance?.SubtractScore(5);
                    PowerEnergyFeedbackManager.Instance?.ShowInstruction("Select all required equipment first.");
                    return false;
                }
                return true;
            case PowerEnergyExperimentStep.CircuitSetup:
                if (PowerEnergyCircuitConnectionManager.Instance != null && PowerEnergyCircuitConnectionManager.Instance.IsComplete)
                    return true;
                PowerEnergyScoreManager.Instance?.SubtractScore(5);
                PowerEnergyFeedbackManager.Instance?.ShowInstruction("Connect power supply, ammeter in series, voltmeter in parallel, appliance, switch and wires.");
                return false;
            case PowerEnergyExperimentStep.Experiment:
                if (PowerEnergyApplianceController.Instance != null && PowerEnergyApplianceController.Instance.CompletedCount >= 3)
                    return true;
                PowerEnergyScoreManager.Instance?.SubtractScore(5);
                PowerEnergyFeedbackManager.Instance?.ShowInstruction("Investigate at least THREE appliances before continuing.");
                return false;
            case PowerEnergyExperimentStep.IdentifyVariables:
                if (PowerEnergyQuestionManager.Instance != null && PowerEnergyQuestionManager.Instance.IsFinished) return true;
                PowerEnergyFeedbackManager.Instance?.ShowInstruction("Answer the variable questions, then press CONTINUE.");
                return false;
            case PowerEnergyExperimentStep.Questions:
                if (PowerEnergyQuestionManager.Instance != null && PowerEnergyQuestionManager.Instance.IsFinished) return true;
                PowerEnergyFeedbackManager.Instance?.ShowInstruction("Select an answer, then press CONTINUE.");
                return false;
            case PowerEnergyExperimentStep.FormulaMatch:
                if (PowerEnergyFormulaMatchingManager.Instance != null && PowerEnergyFormulaMatchingManager.Instance.IsComplete) return true;
                PowerEnergyFeedbackManager.Instance?.ShowInstruction("Match all three formulas first.");
                return false;
            default:
                return true;
        }
    }

    private void EnterStep()
    {
        switch (currentStep)
        {
            case PowerEnergyExperimentStep.CircuitSetup:
                PowerEnergyCircuitConnectionManager.Instance?.ResetCircuit();
                break;
            case PowerEnergyExperimentStep.Experiment:
                experimentPhase = PowerEnergyExperimentPhase.SelectAppliance;
                break;
            case PowerEnergyExperimentStep.ObservationTable:
                PowerEnergyObservationTableManager.Instance?.Refresh();
                break;
            case PowerEnergyExperimentStep.Compare:
                PowerEnergyComparisonManager.Instance?.StartComparison();
                break;
            case PowerEnergyExperimentStep.Graph:
                PowerEnergyGraphController.Instance?.Refresh();
                break;
            case PowerEnergyExperimentStep.FormulaMatch:
                PowerEnergyFormulaMatchingManager.Instance?.ResetMatching();
                break;
            case PowerEnergyExperimentStep.IdentifyVariables:
                PowerEnergyQuestionManager.Instance?.StartVariables();
                break;
            case PowerEnergyExperimentStep.Questions:
                PowerEnergyQuestionManager.Instance?.StartQuiz();
                break;
            case PowerEnergyExperimentStep.Conclusion:
                PowerEnergyConclusionManager.Instance?.ResetBuilder();
                break;
        }
    }

    public void UpdateUI() => PowerEnergyUIManager.Instance?.ShowStep(currentStep);
}
