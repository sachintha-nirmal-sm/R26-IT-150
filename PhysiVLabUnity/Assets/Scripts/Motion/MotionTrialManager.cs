using UnityEngine;

public class MotionTrialManager : MonoBehaviour
{
    public static MotionTrialManager Instance { get; private set; }

    [SerializeField] private int maximumTrials = 5;
    [SerializeField] private float cruiseSpeed = 1.25f;
    [SerializeField] private float selectedTarget = 1f;
    [SerializeField] private bool runActive;
    [SerializeField] private bool awaitingRecord;

    public int MaximumTrials => maximumTrials;
    public float SelectedTarget => selectedTarget;
    public bool RunActive => runActive;
    public bool AwaitingRecord => awaitingRecord;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Configure(int maxTrials, float speed)
    {
        maximumTrials = Mathf.Clamp(maxTrials, 1, 5);
        cruiseSpeed = Mathf.Max(0.25f, speed);
    }

    public void SelectTarget(float meters)
    {
        selectedTarget = Mathf.Clamp(meters, 1f, 5f);
        MotionUIManager.Instance?.SetInstruction($"Target distance selected: {selectedTarget:0} m. Press START EXPERIMENT.");
    }

    public void StartTrial()
    {
        if (MotionTrackController.Instance == null || !MotionTrackController.Instance.SetupComplete)
        {
            MotionScoreManager.Instance?.SubtractScore(5);
            MotionFeedbackManager.Instance?.ShowMessage("✗ Set up the track, ruler, car, markers and stopwatch before starting.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
            return;
        }
        if (ToyCarController.Instance != null && ToyCarController.Instance.IsMoving)
        {
            MotionFeedbackManager.Instance?.ShowInstruction("The car is already moving. Press STOP first.");
            return;
        }

        runActive = true;
        awaitingRecord = false;
        if (ToyCarController.Instance != null)
        {
            ToyCarController.Instance.OnReachedTarget -= OnReachedTarget;
            ToyCarController.Instance.OnReachedTarget += OnReachedTarget;
            ToyCarController.Instance.StartConstantRun(0f, selectedTarget, cruiseSpeed);
        }
        MotionUIManager.Instance?.SetInstruction($"The car is moving toward {selectedTarget:0} m. Watch the stopwatch and live readings.");
    }

    public void StopTrial()
    {
        if (!runActive)
        {
            MotionFeedbackManager.Instance?.ShowInstruction("Start the experiment before pressing STOP.");
            return;
        }
        ToyCarController.Instance?.Stop();
        FinishRun();
    }

    public void RecordTrial()
    {
        if (runActive)
        {
            MotionScoreManager.Instance?.SubtractScore(5);
            MotionFeedbackManager.Instance?.ShowMessage("✗ Stop the car or wait until it reaches the target before recording.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
            return;
        }
        if (!awaitingRecord)
        {
            MotionScoreManager.Instance?.SubtractScore(5);
            MotionFeedbackManager.Instance?.ShowMessage("✗ Run the experiment first, then record the measurement.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
            return;
        }

        float time = StopwatchController.Instance != null ? StopwatchController.Instance.GetElapsedTime() : 0f;
        float initial = 0f;
        float final = MotionPositionController.Instance != null ? MotionPositionController.Instance.GetPositionMeters() : selectedTarget;
        float distance = DistanceCalculator.Instance != null ? DistanceCalculator.Instance.GetDistance() : Mathf.Abs(final - initial);
        float displacement = DisplacementCalculator.Instance != null
            ? DisplacementCalculator.Instance.Calculate(initial, final)
            : final - initial;
        float speed = SpeedCalculator.Instance != null ? SpeedCalculator.Instance.Calculate(distance, time) : 0f;
        float velocity = VelocityCalculator.Instance != null ? VelocityCalculator.Instance.Calculate(displacement, time) : 0f;

        int number = Mathf.Clamp(Mathf.RoundToInt(selectedTarget), 1, maximumTrials);
        var data = new MotionTrialData
        {
            trialNumber = number,
            initialPosition = initial,
            finalPosition = final,
            distance = distance,
            displacement = displacement,
            time = time,
            speed = speed,
            velocity = velocity,
            acceleration = 0f
        };
        MotionDataManager.Instance?.AddOrReplaceTrial(data);
        awaitingRecord = false;
        MotionScoreManager.Instance?.AddScore(5, false);
        MotionFeedbackManager.Instance?.ShowMessage(
            "✓ Measurement recorded.\n" +
            $"Distance = {distance:0.00} m    Time = {time:0.00} s\n" +
            $"Speed = distance / time = {speed:0.00} m/s\n" +
            $"Velocity = displacement / time = {velocity:0.00} m/s →",
            "+5 Marks",
            new Color(0.08f, 0.52f, 0.22f));
        MotionObservationTableManager.Instance?.Refresh();
        MotionUIManager.Instance?.RefreshTrialStatus();
        if (MotionDataManager.Instance != null && MotionDataManager.Instance.CompletedTrialCount() >= maximumTrials)
            MotionUIManager.Instance?.SetNextButtonVisible(true);
    }

    public void ResetCurrentRun()
    {
        runActive = false;
        awaitingRecord = false;
        ToyCarController.Instance?.ResetPosition();
        StopwatchController.Instance?.ResetTimer();
        DistanceCalculator.Instance?.ResetDistance();
        DisplacementCalculator.Instance?.ResetDisplacement();
        MotionMeasurementManager.Instance?.RefreshLivePanel();
    }

    private void OnReachedTarget()
    {
        if (ToyCarController.Instance != null)
            ToyCarController.Instance.OnReachedTarget -= OnReachedTarget;
        FinishRun();
    }

    private void FinishRun()
    {
        runActive = false;
        awaitingRecord = true;
        MotionUIManager.Instance?.SetInstruction("Run complete. Press RECORD to store this trial, then choose another target.");
    }
}
