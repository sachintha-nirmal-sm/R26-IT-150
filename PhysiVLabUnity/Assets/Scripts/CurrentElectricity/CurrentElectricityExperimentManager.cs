using System.Globalization;
using UnityEngine;

public class CurrentElectricityExperimentManager : MonoBehaviour
{
    public static CurrentElectricityExperimentManager Instance { get; private set; }

    [SerializeField] private ElecExperimentStep currentStep = ElecExperimentStep.Introduction;
    [SerializeField] private int totalDisplaySteps = 14;
    [SerializeField] private int mistakeCount;
    private bool flutterSent;
    [SerializeField] private float cellVoltage = 1.5f;
    [SerializeField] private float bulbResistance = 10f;
    [SerializeField] private float internalResistance;
    [SerializeField] private float opposingResidualVoltage;
    [SerializeField] private float voltageTolerance = 0.08f;
    [SerializeField] private float currentTolerance = 0.03f;

    public ElecExperimentStep CurrentStep => currentStep;
    public int MistakeCount => mistakeCount;
    public int TotalDisplaySteps => totalDisplaySteps;
    public int CurrentConnectionNumber
    {
        get
        {
            if (currentStep == ElecExperimentStep.Connection2) return 2;
            if (currentStep == ElecExperimentStep.Connection3) return 3;
            return 1;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ApplyInspectorSettings();
    }

    private void ApplyInspectorSettings()
    {
        CircuitCalculationManager.Instance?.Configure(cellVoltage, bulbResistance, internalResistance, opposingResidualVoltage);
        VoltageMeasurementManager.Instance?.SetTolerance(voltageTolerance);
        CurrentMeasurementManager.Instance?.SetTolerance(currentTolerance);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void RegisterMistake() => mistakeCount++;

    public void StartPractical()
    {
        mistakeCount = 0;
        ElecScoreManager.Instance?.ResetScore();
        ElecExperimentDataManager.Instance?.ResetReadings();
        ElecEquipmentSelectionManager.Instance?.EnsureCardsVisible();
        ElecEquipmentSelectionManager.Instance?.ResetSelection();
        CircuitBuilder.Instance?.ResetCurrentCircuit();
        ApplyInspectorSettings();
        int required = ElecEquipmentSelectionManager.Instance != null ? ElecEquipmentSelectionManager.Instance.RequiredCount : 6;
        ElecScoreManager.Instance?.ConfigureMaxRaw(required);
        currentStep = ElecExperimentStep.Objective;
        ElecUIManager.Instance?.HideResult();
        ElecUIManager.Instance?.SetNextButtonVisible(true);
        ElecUIManager.Instance?.UpdateAttemptsDisplay(ElecAttemptManager.Instance != null ? ElecAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateUI();
    }

    public void SetStep(ElecExperimentStep step)
    {
        currentStep = step;
        UpdateUI();
        if (currentStep == ElecExperimentStep.Complete)
            CompleteExperiment();
    }

    public void AdvanceStep()
    {
        if (currentStep >= ElecExperimentStep.Complete) return;
        switch (currentStep)
        {
            case ElecExperimentStep.Introduction: currentStep = ElecExperimentStep.Objective; break;
            case ElecExperimentStep.Objective: currentStep = ElecExperimentStep.SelectEquipment; break;
            case ElecExperimentStep.SelectEquipment: currentStep = ElecExperimentStep.CircuitTutorial; break;
            case ElecExperimentStep.CircuitTutorial:
                currentStep = ElecExperimentStep.Connection1;
                CircuitBuilder.Instance?.StartConnection(1);
                break;
            case ElecExperimentStep.Connection1:
                if (!EnsureRecorded(1)) return;
                currentStep = ElecExperimentStep.Connection2;
                CircuitBuilder.Instance?.StartConnection(2);
                break;
            case ElecExperimentStep.Connection2:
                if (!EnsureRecorded(2)) return;
                currentStep = ElecExperimentStep.Connection3;
                CircuitBuilder.Instance?.StartConnection(3);
                break;
            case ElecExperimentStep.Connection3:
                if (!EnsureRecorded(3)) return;
                currentStep = ElecExperimentStep.ComparisonTable;
                ElecObservationTableManager.Instance?.Refresh();
                break;
            case ElecExperimentStep.ComparisonTable:
                currentStep = ElecExperimentStep.ComparisonQuestions;
                ElecQuestionManager.Instance?.StartCompare();
                break;
            case ElecExperimentStep.ComparisonQuestions:
                currentStep = ElecExperimentStep.ViewGraph;
                break;
            case ElecExperimentStep.ViewGraph: currentStep = ElecExperimentStep.Education; break;
            case ElecExperimentStep.Education:
                currentStep = ElecExperimentStep.Questions;
                ElecQuestionManager.Instance?.StartQuiz();
                break;
            case ElecExperimentStep.Questions: currentStep = ElecExperimentStep.Conclusion; break;
            case ElecExperimentStep.Conclusion: currentStep = ElecExperimentStep.Complete; break;
            default: currentStep++; break;
        }
        UpdateUI();
        if (currentStep == ElecExperimentStep.Complete)
            CompleteExperiment();
    }

    public void TryAdvanceFromEquipment()
    {
        if (currentStep == ElecExperimentStep.Introduction || currentStep == ElecExperimentStep.Objective)
            currentStep = ElecExperimentStep.SelectEquipment;
        if (currentStep != ElecExperimentStep.SelectEquipment) return;
        if (ElecEquipmentSelectionManager.Instance != null && ElecEquipmentSelectionManager.Instance.IsCompleteCheck())
        {
            ElecScoreManager.Instance?.AddScore(5, false);
            AdvanceStep();
        }
        else
        {
            ElecScoreManager.Instance?.SubtractScore(5);
            ElecFeedbackManager.Instance?.ShowInstruction("Select all required equipment before continuing.");
        }
    }

    public void OnTutorialContinue()
    {
        if (currentStep != ElecExperimentStep.CircuitTutorial) currentStep = ElecExperimentStep.CircuitTutorial;
        ElecScoreManager.Instance?.AddScore(5, false);
        AdvanceStep();
    }

    public void CompleteConnectionRecord()
    {
        if (CircuitBuilder.Instance == null) return;
        if (!CircuitBuilder.Instance.TryRecord()) return;
        ElecUIManager.Instance?.SetNextButtonVisible(true);
        ElecUIManager.Instance?.SetLabButtons(false, false, false, false, false, false);
    }

    public void CompleteExperiment()
    {
        int finalScore = ElecScoreManager.Instance != null ? ElecScoreManager.Instance.FinalizeScore() : 0;
        bool passed = finalScore >= 50;
        var readings = ElecExperimentDataManager.Instance != null
            ? new System.Collections.Generic.List<CircuitReading>(ElecExperimentDataManager.Instance.Readings)
            : new System.Collections.Generic.List<CircuitReading>();

        ElecAttemptRecord attempt;
        if (ElecAttemptManager.Instance != null)
        {
            attempt = ElecAttemptManager.Instance.RegisterAttempt(finalScore, mistakeCount, readings, passed ? "COMPLETED" : "TRY AGAIN");
            ElecProfileManager.Instance?.UpdatePracticalResult(finalScore, mistakeCount, passed, attempt);
        }
        else
        {
            attempt = new ElecAttemptRecord
            {
                attemptNumber = 1,
                score = finalScore,
                mistakes = mistakeCount,
                status = passed ? "COMPLETED" : "TRY AGAIN",
                date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                connectionsCompleted = ElecExperimentDataManager.Instance != null ? ElecExperimentDataManager.Instance.CompletedCount() : 0,
                readings = readings
            };
            ElecProfileManager.Instance?.UpdatePracticalResult(finalScore, mistakeCount, passed, attempt);
        }

        ElecResultManager.Instance?.ShowResult(finalScore, passed, mistakeCount, attempt);
        ElecUIManager.Instance?.ShowResult(finalScore, passed, mistakeCount, attempt);
        SendToFlutter(finalScore, passed);
    }

    private void SendToFlutter(int finalScore, bool passed)
    {
        if (flutterSent)
        {
            return;
        }

        flutterSent = true;
        TimerManager.Instance?.Stop();
        int timeUsed = TimerManager.Instance != null ? TimerManager.Instance.TimeUsedSeconds : 0;
        int readings = ElecExperimentDataManager.Instance != null
            ? ElecExperimentDataManager.Instance.Readings.Count
            : 0;
        string measurements =
            "{\"readings\":" + readings.ToString(CultureInfo.InvariantCulture)
            + ",\"mistakes\":" + mistakeCount.ToString(CultureInfo.InvariantCulture)
            + "}";
        FlutterBridge.EnsureInstance()?.NotifyCompleted(
            finalScore,
            passed,
            mistakeCount,
            timeUsed,
            true,
            measurements);
    }

    public void ResetExperiment()
    {
        mistakeCount = 0;
        flutterSent = false;
        ElecScoreManager.Instance?.ResetScore();
        ElecExperimentDataManager.Instance?.ResetReadings();
        ElecEquipmentSelectionManager.Instance?.ResetSelection();
        CircuitBuilder.Instance?.ResetCurrentCircuit();
        currentStep = ElecExperimentStep.Introduction;
        ElecUIManager.Instance?.HideResult();
        ElecUIManager.Instance?.UpdateScoreDisplay(0);
        UpdateUI();
    }

    public void RetryExperiment()
    {
        if (ElecAttemptManager.Instance != null && !ElecAttemptManager.Instance.CanRetry())
        {
            ElecFeedbackManager.Instance?.ShowInstruction("No attempts remaining. Your best score has been saved to the student profile.");
            return;
        }
        mistakeCount = 0;
        flutterSent = false;
        ElecScoreManager.Instance?.ResetScore();
        ElecExperimentDataManager.Instance?.ResetReadings();
        ElecEquipmentSelectionManager.Instance?.ResetSelection();
        CircuitBuilder.Instance?.ResetCurrentCircuit();
        currentStep = ElecExperimentStep.Introduction;
        ElecUIManager.Instance?.HideResult();
        UpdateUI();
    }

    private bool EnsureRecorded(int connection)
    {
        var data = ElecExperimentDataManager.Instance?.Get(connection);
        if (data != null && data.connectionNumber == connection) return true;
        if (CircuitBuilder.Instance != null && CircuitBuilder.Instance.Phase == CircuitLabPhase.Recorded) return true;
        ElecFeedbackManager.Instance?.ShowInstruction("Record the voltage, current and brightness for this connection first.");
        return false;
    }

    public void UpdateUI()
    {
        ElecUIManager.Instance?.ShowStep(currentStep);
        int display = DisplayIndex(currentStep);
        ElecUIManager.Instance?.UpdateProgress(display, totalDisplaySteps);
        ElecUIManager.Instance?.UpdateScoreDisplay(ElecScoreManager.Instance != null ? ElecScoreManager.Instance.GetScore() : 0);
        ElecUIManager.Instance?.UpdateConnectionLabel(currentStep);
    }

    private static int DisplayIndex(ElecExperimentStep step)
    {
        switch (step)
        {
            case ElecExperimentStep.Introduction: return 1;
            case ElecExperimentStep.Objective: return 2;
            case ElecExperimentStep.SelectEquipment: return 3;
            case ElecExperimentStep.CircuitTutorial: return 4;
            case ElecExperimentStep.Connection1: return 5;
            case ElecExperimentStep.Connection2: return 6;
            case ElecExperimentStep.Connection3: return 7;
            case ElecExperimentStep.ComparisonTable: return 8;
            case ElecExperimentStep.ComparisonQuestions: return 9;
            case ElecExperimentStep.ViewGraph: return 10;
            case ElecExperimentStep.Education: return 11;
            case ElecExperimentStep.Questions: return 12;
            case ElecExperimentStep.Conclusion: return 13;
            default: return 14;
        }
    }
}
