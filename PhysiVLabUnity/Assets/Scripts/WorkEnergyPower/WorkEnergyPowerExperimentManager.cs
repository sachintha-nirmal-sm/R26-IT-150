using System.Globalization;
using UnityEngine;

public class WorkEnergyPowerExperimentManager : MonoBehaviour
{
    public static WorkEnergyPowerExperimentManager Instance { get; private set; }

    [SerializeField] private WorkEnergyExperimentStep currentStep = WorkEnergyExperimentStep.Introduction;
    [SerializeField] private int totalDisplaySteps = 15;
    [SerializeField] private int mistakeCount;
    private bool flutterSent;

    public WorkEnergyExperimentStep CurrentStep => currentStep;
    public int MistakeCount => mistakeCount;
    public int TotalDisplaySteps => totalDisplaySteps;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void RegisterMistake() => mistakeCount++;

    public void StartPractical()
    {
        mistakeCount = 0;
        WorkEnergyScoreManager.Instance?.ResetScore();
        WorkEnergyExperimentDataManager.Instance?.ResetReadings();
        WorkEnergyLabWorkbench.Instance?.ResetWorkbench();
        WorkEnergyConclusionManager.Instance?.ResetConclusion();
        WorkEnergyPowerChallengeManager.Instance?.ResetChallenge();
        WorkEnergyEquipmentSelectionManager.Instance?.EnsureCardsVisible();
        WorkEnergyEquipmentSelectionManager.Instance?.ResetSelection();

        int required = WorkEnergyEquipmentSelectionManager.Instance != null ? WorkEnergyEquipmentSelectionManager.Instance.RequiredCount : 7;
        int heights = WorkEnergyExperimentDataManager.Instance != null && WorkEnergyExperimentDataManager.Instance.ExperimentHeights != null
            ? WorkEnergyExperimentDataManager.Instance.ExperimentHeights.Length
            : 5;
        WorkEnergyScoreManager.Instance?.ConfigureMaxRaw(required, heights);

        currentStep = WorkEnergyExperimentStep.SelectEquipment;
        WorkEnergyScoreManager.Instance?.AddScore(5, false);
        WorkEnergyUIManager.Instance?.UpdateScoreDisplay(WorkEnergyScoreManager.Instance != null ? WorkEnergyScoreManager.Instance.GetScore() : 0);
        WorkEnergyUIManager.Instance?.UpdateAttemptsDisplay(WorkEnergyAttemptManager.Instance != null ? WorkEnergyAttemptManager.Instance.AttemptsRemaining : 3);
        WorkEnergyUIManager.Instance?.HideResult();
        WorkEnergyUIManager.Instance?.SetNextButtonVisible(true);
        UpdateUI();
    }

    public void SetStep(WorkEnergyExperimentStep step)
    {
        currentStep = step;
        UpdateUI();
        if (currentStep == WorkEnergyExperimentStep.Complete)
            CompleteExperiment();
    }

    public void AdvanceStep()
    {
        if (currentStep >= WorkEnergyExperimentStep.Complete) return;
        switch (currentStep)
        {
            case WorkEnergyExperimentStep.SelectEquipment: currentStep = WorkEnergyExperimentStep.PrepareClay; break;
            case WorkEnergyExperimentStep.PrepareClay: currentStep = WorkEnergyExperimentStep.PlaceStand; break;
            case WorkEnergyExperimentStep.PlaceStand: currentStep = WorkEnergyExperimentStep.PlaceWeight; break;
            case WorkEnergyExperimentStep.PlaceWeight: currentStep = WorkEnergyExperimentStep.MeasureMass; break;
            case WorkEnergyExperimentStep.MeasureMass: currentStep = WorkEnergyExperimentStep.SetHeight; break;
            case WorkEnergyExperimentStep.SetHeight: currentStep = WorkEnergyExperimentStep.MeasureHeight; break;
            case WorkEnergyExperimentStep.MeasureHeight: currentStep = WorkEnergyExperimentStep.ReleaseWeight; break;
            case WorkEnergyExperimentStep.ReleaseWeight: currentStep = WorkEnergyExperimentStep.ObserveImpact; break;
            case WorkEnergyExperimentStep.ObserveImpact: currentStep = WorkEnergyExperimentStep.MeasureDepression; break;
            case WorkEnergyExperimentStep.MeasureDepression: currentStep = WorkEnergyExperimentStep.RecordResult; break;
            case WorkEnergyExperimentStep.RecordResult: currentStep = WorkEnergyExperimentStep.CompareResults; break;
            case WorkEnergyExperimentStep.CompareResults: currentStep = WorkEnergyExperimentStep.ViewGraph; break;
            case WorkEnergyExperimentStep.ViewGraph: currentStep = WorkEnergyExperimentStep.WorkEnergy; break;
            case WorkEnergyExperimentStep.WorkEnergy: currentStep = WorkEnergyExperimentStep.PowerChallenge; break;
            case WorkEnergyExperimentStep.PowerChallenge: currentStep = WorkEnergyExperimentStep.ConclusionQ1; break;
            case WorkEnergyExperimentStep.ConclusionQ1: currentStep = WorkEnergyExperimentStep.ConclusionQ2; break;
            case WorkEnergyExperimentStep.ConclusionQ2: currentStep = WorkEnergyExperimentStep.ConclusionQ3; break;
            case WorkEnergyExperimentStep.ConclusionQ3: currentStep = WorkEnergyExperimentStep.Conclusion; break;
            case WorkEnergyExperimentStep.Conclusion: currentStep = WorkEnergyExperimentStep.Complete; break;
            default: currentStep++; break;
        }
        UpdateUI();
        if (currentStep == WorkEnergyExperimentStep.Complete)
            CompleteExperiment();
    }

    public void TryAdvanceFromEquipment()
    {
        if (currentStep == WorkEnergyExperimentStep.Introduction)
            currentStep = WorkEnergyExperimentStep.SelectEquipment;
        if (currentStep != WorkEnergyExperimentStep.SelectEquipment) return;
        if (WorkEnergyEquipmentSelectionManager.Instance != null && WorkEnergyEquipmentSelectionManager.Instance.IsCompleteCheck())
            AdvanceStep();
        else
        {
            WorkEnergyScoreManager.Instance?.SubtractScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("Select all required equipment before continuing.");
        }
    }

    public void CompleteExperiment()
    {
        int finalScore = WorkEnergyScoreManager.Instance != null ? WorkEnergyScoreManager.Instance.FinalizeScore() : 0;
        bool passed = finalScore >= 50;
        var readings = WorkEnergyExperimentDataManager.Instance != null
            ? new System.Collections.Generic.List<EnergyHeightReading>(WorkEnergyExperimentDataManager.Instance.Readings)
            : new System.Collections.Generic.List<EnergyHeightReading>();

        WorkEnergyAttemptRecord attempt;
        if (WorkEnergyAttemptManager.Instance != null)
        {
            attempt = WorkEnergyAttemptManager.Instance.RegisterAttempt(finalScore, mistakeCount, readings, passed ? "PASSED" : "TRY AGAIN");
            WorkEnergyProfileManager.Instance?.UpdatePracticalResult(finalScore, mistakeCount, passed, attempt);
        }
        else
        {
            attempt = new WorkEnergyAttemptRecord
            {
                attemptNumber = 1,
                score = finalScore,
                mistakes = mistakeCount,
                status = passed ? "PASSED" : "TRY AGAIN",
                date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                readings = readings
            };
            WorkEnergyProfileManager.Instance?.UpdatePracticalResult(finalScore, mistakeCount, passed, attempt);
        }

        WorkEnergyResultManager.Instance?.ShowResult(finalScore, passed, mistakeCount, attempt);
        WorkEnergyUIManager.Instance?.ShowResult(finalScore, passed, mistakeCount, attempt);
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
        int readings = WorkEnergyExperimentDataManager.Instance != null
            ? WorkEnergyExperimentDataManager.Instance.Readings.Count
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
        WorkEnergyScoreManager.Instance?.ResetScore();
        WorkEnergyExperimentDataManager.Instance?.ResetReadings();
        WorkEnergyEquipmentSelectionManager.Instance?.ResetSelection();
        WorkEnergyLabWorkbench.Instance?.ResetWorkbench();
        WorkEnergyConclusionManager.Instance?.ResetConclusion();
        WorkEnergyPowerChallengeManager.Instance?.ResetChallenge();
        currentStep = WorkEnergyExperimentStep.SelectEquipment;
        WorkEnergyUIManager.Instance?.HideResult();
        WorkEnergyUIManager.Instance?.UpdateScoreDisplay(0);
        UpdateUI();
    }

    public void RetryExperiment()
    {
        if (WorkEnergyAttemptManager.Instance != null && !WorkEnergyAttemptManager.Instance.CanRetry())
        {
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("No attempts remaining.");
            return;
        }
        ResetExperiment();
        WorkEnergyUIManager.Instance?.RestartPractical();
    }

    private void UpdateUI()
    {
        WorkEnergyUIManager.Instance?.UpdateProgress(currentStep, totalDisplaySteps);
        WorkEnergyUIManager.Instance?.ShowStagePanels(currentStep);
        WorkEnergyUIManager.Instance?.UpdateInstruction(GetInstruction(currentStep));
        WorkEnergyLabWorkbench.Instance?.UpdateForStep(currentStep);

        if (currentStep == WorkEnergyExperimentStep.CompareResults) WorkEnergyUIManager.Instance?.ShowCompareResults();
        if (currentStep == WorkEnergyExperimentStep.ViewGraph) WorkEnergyGraphController.Instance?.ShowGraphs();
        if (currentStep == WorkEnergyExperimentStep.WorkEnergy) WorkEnergyUIManager.Instance?.ShowWorkEnergy();
        if (currentStep == WorkEnergyExperimentStep.PowerChallenge) WorkEnergyPowerChallengeManager.Instance?.ShowChallenge();
        if (currentStep == WorkEnergyExperimentStep.ConclusionQ1 || currentStep == WorkEnergyExperimentStep.ConclusionQ2 || currentStep == WorkEnergyExperimentStep.ConclusionQ3)
            WorkEnergyConclusionManager.Instance?.ShowQuestion(currentStep);
        if (currentStep == WorkEnergyExperimentStep.Conclusion) WorkEnergyConclusionManager.Instance?.ShowFinalConclusion();
    }

    private string GetInstruction(WorkEnergyExperimentStep step)
    {
        switch (step)
        {
            case WorkEnergyExperimentStep.SelectEquipment: return "STEP 1: Tap the required equipment, then press NEXT STEP at the bottom.";
            case WorkEnergyExperimentStep.PrepareClay: return "STEP 2: Press PLACE CLAY at the bottom to prepare the clay surface.";
            case WorkEnergyExperimentStep.PlaceStand: return "STEP 3: Press PLACE STAND at the bottom to position the release stand.";
            case WorkEnergyExperimentStep.PlaceWeight: return "STEP 3: Press PLACE WEIGHT at the bottom to put the weight in the stand.";
            case WorkEnergyExperimentStep.MeasureMass: return "STEP 4: Press MEASURE MASS at the bottom (optional).";
            case WorkEnergyExperimentStep.SetHeight: return "STEP 5: Press SET HEIGHT at the bottom to set the next release height.";
            case WorkEnergyExperimentStep.MeasureHeight: return "STEP 5: Press CONFIRM HEIGHT at the bottom to measure h.";
            case WorkEnergyExperimentStep.ReleaseWeight: return "STEP 6: Press RELEASE WEIGHT at the bottom. The weight falls from rest.";
            case WorkEnergyExperimentStep.ObserveImpact: return "STEP 6: Watch the impact, then press CONTINUE.";
            case WorkEnergyExperimentStep.MeasureDepression: return "STEP 7: Press CONFIRM DEPTH at the bottom to measure the depression.";
            case WorkEnergyExperimentStep.RecordResult: return "STEP 7: Press RECORD READING. The next height is prepared automatically.";
            case WorkEnergyExperimentStep.CompareResults: return "Compare your results. Depression depth is an indicator of the impact effect.";
            case WorkEnergyExperimentStep.ViewGraph: return "Study the graphs of height against potential energy and depression depth.";
            case WorkEnergyExperimentStep.WorkEnergy: return "Read how work, energy and power are connected in this experiment.";
            case WorkEnergyExperimentStep.PowerChallenge: return "Optional challenge: calculate power using P = W / t.";
            case WorkEnergyExperimentStep.ConclusionQ1: return "Question 1 of 3 — choose the correct answer.";
            case WorkEnergyExperimentStep.ConclusionQ2: return "Question 2 of 3 — choose the correct answer.";
            case WorkEnergyExperimentStep.ConclusionQ3: return "Question 3 of 3 — choose the correct answer.";
            case WorkEnergyExperimentStep.Conclusion: return "Read the conclusion, then continue to your final score.";
            case WorkEnergyExperimentStep.Complete: return "Practical completed.";
            default: return "";
        }
    }
}
