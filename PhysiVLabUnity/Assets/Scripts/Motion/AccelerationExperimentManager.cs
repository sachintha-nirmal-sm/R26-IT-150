using UnityEngine;

public class AccelerationExperimentManager : MonoBehaviour
{
    public static AccelerationExperimentManager Instance { get; private set; }

    public struct MotionCondition
    {
        public string name;
        public float initialVelocity;
        public float acceleration;
        public float duration;
    }

    [SerializeField] private int conditionIndex;
    [SerializeField] private bool runComplete;
    [SerializeField] private bool recorded;

    private readonly MotionCondition[] conditions =
    {
        new MotionCondition { name = "LOW SPEED", initialVelocity = 0f, acceleration = 0.75f, duration = 2f },
        new MotionCondition { name = "MEDIUM SPEED", initialVelocity = 0f, acceleration = 1.25f, duration = 2f },
        new MotionCondition { name = "HIGH SPEED", initialVelocity = 0f, acceleration = 2f, duration = 2f }
    };

    private float recordedU;
    private float recordedV;
    private float recordedT;

    public bool Recorded => recorded;
    public MotionCondition Current => conditions[Mathf.Clamp(conditionIndex, 0, conditions.Length - 1)];

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SelectCondition(int index)
    {
        conditionIndex = Mathf.Clamp(index, 0, conditions.Length - 1);
        runComplete = false;
        recorded = false;
        MotionUIManager.Instance?.SetInstruction($"{Current.name}: u = {Current.initialVelocity:0.00} m/s, a = {Current.acceleration:0.00} m/s². Press START EXPERIMENT.");
    }

    public void StartRun()
    {
        runComplete = false;
        recorded = false;
        var c = Current;
        if (ToyCarController.Instance != null)
        {
            ToyCarController.Instance.OnStopped -= OnStopped;
            ToyCarController.Instance.OnStopped += OnStopped;
            ToyCarController.Instance.StartAccelerationRun(c.initialVelocity, c.acceleration, c.duration);
        }
        MotionUIManager.Instance?.SetInstruction($"{Current.name} run started. Watch how velocity increases.");
    }

    public void Record()
    {
        if (!runComplete)
        {
            MotionScoreManager.Instance?.SubtractScore(5);
            MotionFeedbackManager.Instance?.ShowMessage("✗ Complete the acceleration run before recording.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
            return;
        }
        var c = Current;
        recordedU = c.initialVelocity;
        recordedT = StopwatchController.Instance != null ? StopwatchController.Instance.GetElapsedTime() : c.duration;
        recordedV = recordedU + c.acceleration * recordedT;
        float a = AccelerationCalculator.Instance != null
            ? AccelerationCalculator.Instance.Calculate(recordedV, recordedU, recordedT)
            : 0f;
        MotionDataManager.Instance?.AddAccelerationTrial(new AccelerationTrialData
        {
            trialNumber = conditionIndex + 1,
            initialVelocity = recordedU,
            finalVelocity = recordedV,
            time = recordedT,
            acceleration = a,
            condition = c.name
        });
        recorded = true;
        if (!MotionExperimentManager.Instance.AccelerationScored)
        {
            MotionExperimentManager.Instance.AccelerationScored = true;
            MotionScoreManager.Instance?.AddScore(10, false);
        }
        else MotionScoreManager.Instance?.AddScore(5, false);
        MotionFeedbackManager.Instance?.ShowMessage(
            "✓ Acceleration recorded.\n" +
            $"u = {recordedU:0.00} m/s    v = {recordedV:0.00} m/s    t = {recordedT:0.00} s\n" +
            $"a = (v − u) / t = {a:0.00} m/s²",
            "+10 Marks",
            new Color(0.08f, 0.52f, 0.22f));
        MotionObservationTableManager.Instance?.Refresh();
        MotionUIManager.Instance?.SetNextButtonVisible(true);
    }

    public bool CheckStudentAcceleration(float studentValue, float tolerance)
    {
        if (!recorded)
        {
            MotionFeedbackManager.Instance?.ShowInstruction("Record the acceleration run first.");
            return false;
        }
        float expected = AccelerationCalculator.Instance != null
            ? AccelerationCalculator.Instance.Calculate(recordedV, recordedU, recordedT)
            : 0f;
        bool ok = AccelerationCalculator.Instance != null &&
                  AccelerationCalculator.Instance.ValidateStudentAnswer(studentValue, expected, tolerance);
        if (ok)
        {
            MotionScoreManager.Instance?.AddScore(5);
            MotionFeedbackManager.Instance?.ShowMessage("✓ Correct. a = (v − u) / t", "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            MotionScoreManager.Instance?.SubtractScore(5);
            MotionFeedbackManager.Instance?.ShowMessage($"✗ Incorrect. Expected about {expected:0.00} m/s².", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
        }
        return ok;
    }

    public void ResetRun()
    {
        runComplete = false;
        recorded = false;
        if (ToyCarController.Instance != null) ToyCarController.Instance.OnStopped -= OnStopped;
        ToyCarController.Instance?.ResetPosition();
        StopwatchController.Instance?.ResetTimer();
    }

    private void OnStopped()
    {
        if (MotionExperimentManager.Instance == null ||
            MotionExperimentManager.Instance.CurrentStep != MotionExperimentStep.AccelerationExperiment)
            return;
        runComplete = true;
        if (ToyCarController.Instance != null) ToyCarController.Instance.OnStopped -= OnStopped;
        MotionUIManager.Instance?.SetInstruction("Acceleration run finished. Press RECORD, then calculate a = (v − u) / t.");
    }
}
