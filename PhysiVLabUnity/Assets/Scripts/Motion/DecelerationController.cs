using UnityEngine;

public class DecelerationController : MonoBehaviour
{
    public static DecelerationController Instance { get; private set; }

    [SerializeField] private float initialVelocity = 5f;
    [SerializeField] private float deceleration = -2f;
    [SerializeField] private bool completed;
    [SerializeField] private float recordedInitial;
    [SerializeField] private float recordedFinal;
    [SerializeField] private float recordedTime;

    public bool Completed => completed;
    public float RecordedAcceleration { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Configure(float startVelocity, float accel)
    {
        initialVelocity = Mathf.Abs(startVelocity);
        deceleration = accel < 0f ? accel : -Mathf.Abs(accel);
    }

    public void BeginDemonstration()
    {
        completed = false;
        recordedInitial = initialVelocity;
        recordedFinal = 0f;
        recordedTime = 0f;
        if (ToyCarController.Instance != null)
        {
            ToyCarController.Instance.OnStopped = OnCarStopped;
            ToyCarController.Instance.StartDecelerationRun(initialVelocity, deceleration);
        }
        MotionFeedbackManager.Instance?.ShowInstruction("Observe what happens when the moving car slows down.");
    }

    private void OnCarStopped()
    {
        if (MotionExperimentManager.Instance == null ||
            MotionExperimentManager.Instance.CurrentStep != MotionExperimentStep.Deceleration)
            return;
        recordedTime = StopwatchController.Instance != null ? StopwatchController.Instance.GetElapsedTime() : 0f;
        recordedFinal = ToyCarController.Instance != null ? ToyCarController.Instance.GetVelocity() : 0f;
        RecordedAcceleration = AccelerationCalculator.Instance != null
            ? AccelerationCalculator.Instance.Calculate(recordedFinal, recordedInitial, recordedTime)
            : 0f;
        completed = true;
        if (ToyCarController.Instance != null) ToyCarController.Instance.OnStopped = null;
        if (!MotionExperimentManager.Instance.DecelerationScored)
        {
            MotionExperimentManager.Instance.DecelerationScored = true;
            MotionScoreManager.Instance?.AddScore(5);
        }
        MotionFeedbackManager.Instance?.ShowMessage(
            "✓ Negative acceleration represents deceleration.\n" +
            $"u = {recordedInitial:0.00} m/s    v = {recordedFinal:0.00} m/s    t = {recordedTime:0.00} s\n" +
            $"a = (v − u) / t = {RecordedAcceleration:0.00} m/s²",
            "+5 Marks",
            new Color(0.08f, 0.52f, 0.22f));
        MotionUIManager.Instance?.SetNextButtonVisible(true);
        MotionDataManager.Instance?.SetDeceleration(recordedInitial, recordedFinal, recordedTime, RecordedAcceleration);
    }

    public void ResetDemo()
    {
        completed = false;
        recordedTime = 0f;
        RecordedAcceleration = 0f;
        if (ToyCarController.Instance != null) ToyCarController.Instance.OnStopped = null;
        ToyCarController.Instance?.ResetPosition();
        StopwatchController.Instance?.ResetTimer();
    }
}
