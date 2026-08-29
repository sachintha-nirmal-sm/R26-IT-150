using System.Globalization;
using UnityEngine;

public class MotionExperimentManager : MonoBehaviour
{
    public static MotionExperimentManager Instance { get; private set; }

    [SerializeField] private MotionExperimentStep currentStep = MotionExperimentStep.Introduction;
    [SerializeField] private int totalDisplaySteps = 20;
    [SerializeField] private int mistakeCount;
    private bool flutterSent;
    [SerializeField] private float trackLengthMeters = 5f;
    [SerializeField] private int maximumTrials = 5;
    [SerializeField] private int correctScore = 5;
    [SerializeField] private int wrongPenalty = 5;
    [SerializeField] private int majorStepScore = 10;
    [SerializeField] private int maximumAttempts = 3;
    [SerializeField] private float measurementTolerance = 0.05f;
    [SerializeField] private float cruiseSpeed = 1.25f;
    [SerializeField] private float initialVelocity;
    [SerializeField] private float acceleration = 1.25f;
    [SerializeField] private float deceleration = -2f;

    public MotionExperimentStep CurrentStep => currentStep;
    public int MistakeCount => mistakeCount;
    public int TotalDisplaySteps => totalDisplaySteps;
    public float MeasurementTolerance => measurementTolerance;
    public bool SpeedCalcScored { get; set; }
    public bool VelocityCalcScored { get; set; }
    public bool PathTaskScored { get; set; }
    public bool CompareScored { get; set; }
    public bool AccelerationScored { get; set; }
    public bool DecelerationScored { get; set; }
    public bool CompletionScored { get; set; }

    private void Awake() => Instance = this;

    private void Start() => ApplyInspectorSettings();

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void ApplyInspectorSettings()
    {
        MotionPositionController.Instance?.Configure(trackLengthMeters);
        MotionTrackController.Instance?.Configure(trackLengthMeters);
        MotionTrialManager.Instance?.Configure(maximumTrials, cruiseSpeed);
        MotionScoreManager.Instance?.Configure(correctScore, wrongPenalty, majorStepScore);
        MotionAttemptManager.Instance?.Configure(maximumAttempts);
        DecelerationController.Instance?.Configure(Mathf.Max(0.5f, initialVelocity > 0f ? initialVelocity : 5f), deceleration);
        ToyCarController.Instance?.ConfigureLimits(0f, trackLengthMeters);
    }

    public void RegisterMistake() => mistakeCount++;

    public void StartPractical()
    {
        mistakeCount = 0;
        SpeedCalcScored = VelocityCalcScored = PathTaskScored = CompareScored = false;
        AccelerationScored = DecelerationScored = CompletionScored = false;
        MotionScoreManager.Instance?.ResetScore();
        MotionDataManager.Instance?.ResetReadings();
        MotionEquipmentSelectionManager.Instance?.EnsureCardsVisible();
        MotionEquipmentSelectionManager.Instance?.ResetSelection();
        MotionTrackController.Instance?.FullReset();
        ApplyInspectorSettings();
        int required = MotionEquipmentSelectionManager.Instance != null ? MotionEquipmentSelectionManager.Instance.RequiredCount : 7;
        MotionScoreManager.Instance?.ConfigureMaxRaw(required);
        currentStep = MotionExperimentStep.Objective;
        MotionUIManager.Instance?.HideResult();
        MotionUIManager.Instance?.SetNextButtonVisible(true);
        MotionUIManager.Instance?.UpdateAttemptsDisplay(MotionAttemptManager.Instance != null ? MotionAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateUI();
    }

    public void SetStep(MotionExperimentStep step)
    {
        currentStep = step;
        EnterStep();
        UpdateUI();
        if (currentStep == MotionExperimentStep.Complete)
            CompleteExperiment();
    }

    public void AdvanceStep()
    {
        if (currentStep >= MotionExperimentStep.Complete) return;
        if (!CanLeaveCurrentStep()) return;
        currentStep++;
        EnterStep();
        UpdateUI();
        if (currentStep == MotionExperimentStep.Complete)
            CompleteExperiment();
    }

    public void TryAdvanceFromEquipment()
    {
        if (currentStep == MotionExperimentStep.Introduction || currentStep == MotionExperimentStep.Objective)
            currentStep = MotionExperimentStep.SelectEquipment;
        if (currentStep != MotionExperimentStep.SelectEquipment) return;
        if (MotionEquipmentSelectionManager.Instance != null && MotionEquipmentSelectionManager.Instance.IsCompleteCheck())
        {
            MotionScoreManager.Instance?.AddScore(5, false);
            AdvanceStep();
        }
        else
        {
            MotionScoreManager.Instance?.SubtractScore(5);
            MotionFeedbackManager.Instance?.ShowInstruction("Select all required equipment before continuing.");
        }
    }

    public void NotifySetupChanged()
    {
        if (currentStep < MotionExperimentStep.SetupTrack || currentStep > MotionExperimentStep.SetDirection) return;
        var setup = MotionTrackController.Instance;
        if (setup == null) return;
        if (currentStep == MotionExperimentStep.SetupTrack && setup.TrackPlaced) MotionUIManager.Instance?.SetNextButtonVisible(true);
        if (currentStep == MotionExperimentStep.SetupRuler && setup.RulerPlaced) MotionUIManager.Instance?.SetNextButtonVisible(true);
        if (currentStep == MotionExperimentStep.PlaceCar && setup.CarPlaced) MotionUIManager.Instance?.SetNextButtonVisible(true);
        if (currentStep == MotionExperimentStep.PlaceMarkers && setup.MarkersPlaced >= 5) MotionUIManager.Instance?.SetNextButtonVisible(true);
        if (currentStep == MotionExperimentStep.SetDirection && setup.DirectionSet) MotionUIManager.Instance?.SetNextButtonVisible(true);
    }

    public void CheckSpeedAnswer(float studentValue)
    {
        var trial = BestTrialForCalc();
        if (trial == null)
        {
            MotionFeedbackManager.Instance?.ShowInstruction("Complete at least one motion trial first.");
            return;
        }
        float expected = SpeedCalculator.Instance != null
            ? SpeedCalculator.Instance.Calculate(trial.distance, trial.time)
            : trial.speed;
        bool ok = SpeedCalculator.Instance != null &&
                  SpeedCalculator.Instance.ValidateStudentAnswer(studentValue, expected, measurementTolerance);
        if (ok && !SpeedCalcScored)
        {
            SpeedCalcScored = true;
            MotionScoreManager.Instance?.AddScore(5, false);
            MotionFeedbackManager.Instance?.ShowMessage("✓ Correct. Speed = distance / time.", "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
            MotionUIManager.Instance?.SetNextButtonVisible(true);
        }
        else if (ok)
        {
            MotionFeedbackManager.Instance?.ShowCorrect("+5 Marks");
            MotionUIManager.Instance?.SetNextButtonVisible(true);
        }
        else
        {
            MotionScoreManager.Instance?.SubtractScore(5);
            MotionFeedbackManager.Instance?.ShowMessage($"✗ Incorrect. Expected about {expected:0.00} m/s.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
        }
    }

    public void CheckVelocityAnswer(float studentValue)
    {
        var trial = BestTrialForCalc();
        if (trial == null)
        {
            MotionFeedbackManager.Instance?.ShowInstruction("Complete at least one motion trial first.");
            return;
        }
        float expected = VelocityCalculator.Instance != null
            ? VelocityCalculator.Instance.Calculate(trial.displacement, trial.time)
            : trial.velocity;
        bool ok = VelocityCalculator.Instance != null &&
                  VelocityCalculator.Instance.ValidateStudentAnswer(studentValue, expected, measurementTolerance);
        if (ok && !VelocityCalcScored)
        {
            VelocityCalcScored = true;
            MotionScoreManager.Instance?.AddScore(5, false);
            MotionFeedbackManager.Instance?.ShowMessage("✓ Correct. Velocity = displacement / time. Direction →", "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
            MotionUIManager.Instance?.SetNextButtonVisible(true);
        }
        else if (!ok)
        {
            MotionScoreManager.Instance?.SubtractScore(5);
            MotionFeedbackManager.Instance?.ShowMessage($"✗ Incorrect. Expected about {expected:0.00} m/s.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
        }
        else MotionUIManager.Instance?.SetNextButtonVisible(true);
    }

    public void CheckCompare(float distance, float displacement)
    {
        bool ok = Mathf.Abs(distance - 6f) <= 0.05f && Mathf.Abs(displacement - 2f) <= 0.05f;
        if (ok && !CompareScored)
        {
            CompareScored = true;
            MotionScoreManager.Instance?.AddScore(5, false);
            MotionFeedbackManager.Instance?.ShowMessage(
                "✓ Distance = 6 m (4 m + 2 m). Displacement = +2 m (final − initial).\nDistance depends on the total path. Displacement depends only on start and finish.",
                "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
            MotionUIManager.Instance?.SetNextButtonVisible(true);
        }
        else if (!ok)
        {
            MotionScoreManager.Instance?.SubtractScore(5);
            MotionFeedbackManager.Instance?.ShowMessage("✗ Distance should be 6 m and displacement +2 m.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
        }
        else MotionUIManager.Instance?.SetNextButtonVisible(true);
    }

    public void StartPathTask()
    {
        if (ToyCarController.Instance == null) return;
        ToyCarController.Instance.OnPathComplete -= OnPathComplete;
        ToyCarController.Instance.OnPathComplete += OnPathComplete;
        ToyCarController.Instance.StartPath(new[] { 0f, 3f, 1f }, 1.6f);
        MotionUIManager.Instance?.SetInstruction("Watch the car move 0 m → 3 m → 1 m. Compare distance and displacement.");
    }

    private void OnPathComplete()
    {
        if (ToyCarController.Instance != null) ToyCarController.Instance.OnPathComplete -= OnPathComplete;
        float distance = DistanceCalculator.Instance != null ? DistanceCalculator.Instance.GetDistance() : 5f;
        float displacement = DisplacementCalculator.Instance != null ? DisplacementCalculator.Instance.Displacement : 1f;
        MotionDataManager.Instance?.SetPathResult(distance, displacement);
        if (!PathTaskScored)
        {
            PathTaskScored = true;
            MotionScoreManager.Instance?.AddScore(5, false);
        }
        MotionFeedbackManager.Instance?.ShowMessage(
            $"Distance = {distance:0.00} m (3 m + 2 m).\nDisplacement = {displacement:+0.00;-0.00} m (final − initial).\nWhy different? Distance uses the whole path. Displacement uses only start and finish, with direction.",
            "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
        MotionUIManager.Instance?.SetNextButtonVisible(true);
        MotionObservationTableManager.Instance?.Refresh();
    }

    public void ResetExperiment()
    {
        mistakeCount = 0;
        flutterSent = false;
        SpeedCalcScored = VelocityCalcScored = PathTaskScored = CompareScored = false;
        AccelerationScored = DecelerationScored = CompletionScored = false;
        MotionScoreManager.Instance?.ResetScore();
        MotionDataManager.Instance?.ResetReadings();
        MotionEquipmentSelectionManager.Instance?.ResetSelection();
        MotionTrackController.Instance?.FullReset();
        AccelerationExperimentManager.Instance?.ResetRun();
        DecelerationController.Instance?.ResetDemo();
        currentStep = MotionExperimentStep.Introduction;
        MotionUIManager.Instance?.HideResult();
        MotionUIManager.Instance?.UpdateScoreDisplay(0);
        UpdateUI();
    }

    public void ResetKeepTrials()
    {
        MotionTrackController.Instance?.ResetSetupKeepScore();
        MotionTrialManager.Instance?.ResetCurrentRun();
        MotionUIManager.Instance?.SetInstruction("Current run reset. Previous recorded trials and score are kept.");
    }

    public void RetryExperiment()
    {
        if (MotionAttemptManager.Instance != null && !MotionAttemptManager.Instance.CanRetry())
        {
            MotionFeedbackManager.Instance?.ShowInstruction("No attempts remaining. Your best score has been saved to the student profile.");
            return;
        }
        ResetExperiment();
    }

    public void CompleteExperiment()
    {
        if (!CompletionScored)
        {
            CompletionScored = true;
            MotionScoreManager.Instance?.AddScore(5, false);
        }
        int finalScore = MotionScoreManager.Instance != null ? MotionScoreManager.Instance.FinalizeScore() : 0;
        bool passed = finalScore >= 50;
        var trials = MotionDataManager.Instance != null
            ? new System.Collections.Generic.List<MotionTrialData>(MotionDataManager.Instance.Trials)
            : new System.Collections.Generic.List<MotionTrialData>();

        MotionAttemptRecord attempt;
        if (MotionAttemptManager.Instance != null)
        {
            attempt = MotionAttemptManager.Instance.RegisterAttempt(finalScore, mistakeCount, trials, passed ? "COMPLETED" : "TRY AGAIN");
            MotionProfileManager.Instance?.UpdatePracticalResult(finalScore, mistakeCount, passed, attempt);
        }
        else
        {
            attempt = new MotionAttemptRecord
            {
                attemptNumber = 1,
                score = finalScore,
                mistakes = mistakeCount,
                status = passed ? "COMPLETED" : "TRY AGAIN",
                date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                trialsCompleted = MotionDataManager.Instance != null ? MotionDataManager.Instance.CompletedTrialCount() : 0,
                trials = trials
            };
            MotionProfileManager.Instance?.UpdatePracticalResult(finalScore, mistakeCount, passed, attempt);
        }

        MotionResultManager.Instance?.ShowResult(finalScore, passed, mistakeCount, attempt);
        MotionUIManager.Instance?.ShowResult(finalScore, passed, mistakeCount, attempt);
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
        int trialsCount = MotionDataManager.Instance != null
            ? MotionDataManager.Instance.Trials.Count
            : 0;
        string measurements =
            "{\"trials\":" + trialsCount.ToString(CultureInfo.InvariantCulture)
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

    public void UpdateUI()
    {
        MotionUIManager.Instance?.ShowStep(currentStep);
        MotionUIManager.Instance?.UpdateProgress(DisplayIndex(currentStep), totalDisplaySteps);
        MotionUIManager.Instance?.UpdateScoreDisplay(MotionScoreManager.Instance != null ? MotionScoreManager.Instance.GetScore() : 0);
        MotionUIManager.Instance?.UpdateStepLabel(currentStep);
        NotifySetupChanged();
    }

    private void EnterStep()
    {
        switch (currentStep)
        {
            case MotionExperimentStep.SetupTrack:
                MotionTrackController.Instance?.BuildTrayItems();
                break;
            case MotionExperimentStep.MotionTrials:
                ToyCarController.Instance?.ResetPosition();
                break;
            case MotionExperimentStep.DistanceVsDisplacement:
                StartPathTask();
                break;
            case MotionExperimentStep.AccelerationExperiment:
                AccelerationExperimentManager.Instance?.SelectCondition(acceleration < 1f ? 0 : acceleration < 1.6f ? 1 : 2);
                break;
            case MotionExperimentStep.Deceleration:
                DecelerationController.Instance?.BeginDemonstration();
                break;
            case MotionExperimentStep.ObservationTable:
                MotionObservationTableManager.Instance?.Refresh();
                break;
            case MotionExperimentStep.Graphs:
                MotionGraphController.Instance?.ShowGraphs();
                break;
            case MotionExperimentStep.Questions:
                MotionQuestionManager.Instance?.StartQuiz();
                break;
        }
    }

    private bool CanLeaveCurrentStep()
    {
        var setup = MotionTrackController.Instance;
        switch (currentStep)
        {
            case MotionExperimentStep.SetupTrack:
                if (setup != null && !setup.TrackPlaced)
                {
                    MotionFeedbackManager.Instance?.ShowInstruction("Place the straight track first.");
                    return false;
                }
                break;
            case MotionExperimentStep.SetupRuler:
                if (setup != null && !setup.RulerPlaced)
                {
                    MotionFeedbackManager.Instance?.ShowInstruction("Place the metre ruler along the track.");
                    return false;
                }
                break;
            case MotionExperimentStep.PlaceCar:
                if (setup != null && !setup.CarPlaced)
                {
                    MotionFeedbackManager.Instance?.ShowInstruction("Place the car at the starting position 0 m.");
                    return false;
                }
                break;
            case MotionExperimentStep.PlaceMarkers:
                if (setup != null && setup.MarkersPlaced < 5)
                {
                    MotionFeedbackManager.Instance?.ShowInstruction("Place markers at 1 m, 2 m, 3 m, 4 m and 5 m.");
                    return false;
                }
                break;
            case MotionExperimentStep.SetDirection:
                if (setup != null && !setup.DirectionSet)
                {
                    MotionFeedbackManager.Instance?.ShowInstruction("Confirm the positive direction START → FINISH.");
                    return false;
                }
                if (setup != null && !setup.StopwatchPlaced)
                {
                    MotionFeedbackManager.Instance?.ShowInstruction("Place the stopwatch on the right-hand panel.");
                    return false;
                }
                break;
            case MotionExperimentStep.MotionTrials:
                if (MotionDataManager.Instance == null || MotionDataManager.Instance.CompletedTrialCount() < 1)
                {
                    MotionFeedbackManager.Instance?.ShowInstruction("Complete and record at least one trial. Aim for all five distances.");
                    return false;
                }
                break;
            case MotionExperimentStep.SpeedCalculation:
                if (!SpeedCalcScored)
                {
                    MotionFeedbackManager.Instance?.ShowInstruction("Enter the calculated speed first.");
                    return false;
                }
                break;
            case MotionExperimentStep.VelocityCalculation:
                if (!VelocityCalcScored)
                {
                    MotionFeedbackManager.Instance?.ShowInstruction("Enter the calculated velocity first.");
                    return false;
                }
                break;
            case MotionExperimentStep.AccelerationExperiment:
                if (AccelerationExperimentManager.Instance != null && !AccelerationExperimentManager.Instance.Recorded)
                {
                    MotionFeedbackManager.Instance?.ShowInstruction("Run and record the acceleration experiment first.");
                    return false;
                }
                break;
            case MotionExperimentStep.CompareDistanceDisplacement:
                if (!CompareScored)
                {
                    MotionFeedbackManager.Instance?.ShowInstruction("Enter distance and displacement for the comparison task.");
                    return false;
                }
                break;
        }
        return true;
    }

    private MotionTrialData BestTrialForCalc()
    {
        var data = MotionDataManager.Instance;
        if (data == null) return null;
        var t3 = data.GetTrial(3);
        if (t3 != null && t3.time > 0f) return t3;
        foreach (var t in data.Trials)
            if (t != null && t.time > 0f) return t;
        return null;
    }

    private static int DisplayIndex(MotionExperimentStep step)
    {
        return Mathf.Clamp((int)step + 1, 1, 20);
    }
}
