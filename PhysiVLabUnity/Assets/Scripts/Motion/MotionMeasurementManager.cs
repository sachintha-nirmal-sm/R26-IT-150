using UnityEngine;

public class MotionMeasurementManager : MonoBehaviour
{
    public static MotionMeasurementManager Instance { get; private set; }

    [SerializeField] private float sampleInterval = 0.12f;
    private float sampleTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        RefreshLivePanel();
        if (ToyCarController.Instance == null || !ToyCarController.Instance.IsMoving) return;
        sampleTimer += Time.deltaTime;
        if (sampleTimer < sampleInterval) return;
        sampleTimer = 0f;
        float t = StopwatchController.Instance != null ? StopwatchController.Instance.GetElapsedTime() : 0f;
        float d = DistanceCalculator.Instance != null ? DistanceCalculator.Instance.GetDistance() : 0f;
        float v = ToyCarController.Instance.GetVelocity();
        float a = ToyCarController.Instance.Acceleration;
        MotionDataManager.Instance?.AddSample(t, d, v, a);
    }

    public void RefreshLivePanel()
    {
        float time = StopwatchController.Instance != null ? StopwatchController.Instance.GetElapsedTime() : 0f;
        float pos = MotionPositionController.Instance != null ? MotionPositionController.Instance.GetPositionMeters() : 0f;
        float distance = DistanceCalculator.Instance != null ? DistanceCalculator.Instance.GetDistance() : 0f;
        float displacement = DisplacementCalculator.Instance != null ? DisplacementCalculator.Instance.Displacement : pos;
        float speed = time > 0.0001f ? distance / time : Mathf.Abs(ToyCarController.Instance != null ? ToyCarController.Instance.GetVelocity() : 0f);
        float velocity = time > 0.0001f ? displacement / time : (ToyCarController.Instance != null ? ToyCarController.Instance.GetVelocity() : 0f);
        float accel = ToyCarController.Instance != null ? ToyCarController.Instance.Acceleration : 0f;
        MotionUIManager.Instance?.UpdateLiveReadings(time, pos, distance, displacement, speed, velocity, accel);
        MotionUIManager.Instance?.UpdateStopwatchDisplay(time);
    }
}
