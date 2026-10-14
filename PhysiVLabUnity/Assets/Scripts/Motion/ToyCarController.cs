using UnityEngine;

public class ToyCarController : MonoBehaviour
{
    public static ToyCarController Instance { get; private set; }

    public enum DriveMode
    {
        Stopped,
        ConstantVelocity,
        Accelerating,
        Decelerating,
        PathSequence
    }

    [SerializeField] private float velocity;
    [SerializeField] private float acceleration;
    [SerializeField] private float targetPosition = 5f;
    [SerializeField] private DriveMode mode = DriveMode.Stopped;

    private float[] pathPoints;
    private int pathIndex;
    private float pathSpeed = 1.5f;
    private float minMeters;
    private float maxMeters = 5f;

    public float Velocity => velocity;
    public float Acceleration => acceleration;
    public bool IsMoving => mode != DriveMode.Stopped;
    public DriveMode Mode => mode;

    public System.Action OnReachedTarget;
    public System.Action OnPathComplete;
    public System.Action OnStopped;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (mode == DriveMode.Stopped) return;
        float dt = Time.deltaTime;
        float pos = MotionPositionController.Instance != null
            ? MotionPositionController.Instance.GetPositionMeters()
            : 0f;

        if (mode == DriveMode.Accelerating || mode == DriveMode.Decelerating)
            velocity += acceleration * dt;

        if (mode == DriveMode.Decelerating && velocity <= 0f)
        {
            velocity = 0f;
            Stop();
            return;
        }

        if (mode == DriveMode.PathSequence)
        {
            if (pathPoints == null || pathIndex >= pathPoints.Length)
            {
                Stop();
                OnPathComplete?.Invoke();
                return;
            }
            float dest = pathPoints[pathIndex];
            float dir = dest >= pos ? 1f : -1f;
            velocity = dir * pathSpeed;
            float next = pos + velocity * dt;
            if ((dir > 0f && next >= dest) || (dir < 0f && next <= dest))
            {
                MotionPositionController.Instance?.SetPositionMeters(dest);
                DistanceCalculator.Instance?.AddSample(dest);
                DisplacementCalculator.Instance?.Calculate(dest);
                pathIndex++;
                if (pathIndex >= pathPoints.Length)
                {
                    Stop();
                    OnPathComplete?.Invoke();
                }
                return;
            }
            pos = next;
        }
        else
        {
            pos += velocity * dt;
            if (mode == DriveMode.ConstantVelocity)
            {
                if ((velocity >= 0f && pos >= targetPosition) || (velocity < 0f && pos <= targetPosition))
                {
                    pos = targetPosition;
                    MotionPositionController.Instance?.SetPositionMeters(pos);
                    DistanceCalculator.Instance?.AddSample(pos);
                    DisplacementCalculator.Instance?.Calculate(pos);
                    Stop();
                    OnReachedTarget?.Invoke();
                    return;
                }
            }
        }

        pos = Mathf.Clamp(pos, minMeters, maxMeters);
        MotionPositionController.Instance?.SetPositionMeters(pos);
        DistanceCalculator.Instance?.AddSample(pos);
        DisplacementCalculator.Instance?.Calculate(pos);

        if (pos <= minMeters && velocity < 0f)
        {
            velocity = 0f;
            Stop();
        }
        else if (pos >= maxMeters && velocity > 0f)
        {
            velocity = 0f;
            Stop();
        }
    }

    public void MoveForward()
    {
        if (velocity <= 0f) velocity = 1.25f;
        mode = DriveMode.ConstantVelocity;
        targetPosition = maxMeters;
    }

    public void MoveBackward()
    {
        if (velocity >= 0f) velocity = -1.25f;
        mode = DriveMode.ConstantVelocity;
        targetPosition = minMeters;
    }

    public void Stop()
    {
        mode = DriveMode.Stopped;
        StopwatchController.Instance?.StopTimer();
        OnStopped?.Invoke();
    }

    public void ResetPosition()
    {
        Stop();
        velocity = 0f;
        acceleration = 0f;
        MotionPositionController.Instance?.ResetPosition();
        DistanceCalculator.Instance?.ResetDistance();
        DisplacementCalculator.Instance?.ResetDisplacement();
        DisplacementCalculator.Instance?.SetInitialPosition(0f);
    }

    public void SetVelocity(float value) => velocity = value;
    public float GetVelocity() => velocity;
    public float GetPosition() => MotionPositionController.Instance != null ? MotionPositionController.Instance.GetPositionMeters() : 0f;

    public void StartConstantRun(float startMeters, float endMeters, float speedMetersPerSecond)
    {
        maxMeters = MotionPositionController.Instance != null ? MotionPositionController.Instance.TrackLengthMeters : 5f;
        startMeters = Mathf.Clamp(startMeters, 0f, maxMeters);
        endMeters = Mathf.Clamp(endMeters, 0f, maxMeters);
        MotionPositionController.Instance?.SetPositionMeters(startMeters);
        DistanceCalculator.Instance?.BeginTracking(startMeters);
        DisplacementCalculator.Instance?.SetInitialPosition(startMeters);
        targetPosition = endMeters;
        velocity = endMeters >= startMeters ? Mathf.Abs(speedMetersPerSecond) : -Mathf.Abs(speedMetersPerSecond);
        acceleration = 0f;
        mode = DriveMode.ConstantVelocity;
        StopwatchController.Instance?.ResetTimer();
        StopwatchController.Instance?.StartTimer();
    }

    public void StartAccelerationRun(float initialVelocity, float accel, float durationHint)
    {
        CancelInvoke();
        maxMeters = MotionPositionController.Instance != null ? MotionPositionController.Instance.TrackLengthMeters : 5f;
        MotionPositionController.Instance?.ResetPosition();
        DistanceCalculator.Instance?.BeginTracking(0f);
        DisplacementCalculator.Instance?.SetInitialPosition(0f);
        velocity = initialVelocity;
        acceleration = accel;
        mode = DriveMode.Accelerating;
        StopwatchController.Instance?.ResetTimer();
        StopwatchController.Instance?.StartTimer();
        if (durationHint > 0f)
            Invoke(nameof(Stop), durationHint);
    }

    public void StartDecelerationRun(float initialVelocity, float accel)
    {
        CancelInvoke();
        maxMeters = MotionPositionController.Instance != null ? MotionPositionController.Instance.TrackLengthMeters : 5f;
        MotionPositionController.Instance?.SetPositionMeters(0.2f);
        DistanceCalculator.Instance?.BeginTracking(0.2f);
        DisplacementCalculator.Instance?.SetInitialPosition(0.2f);
        velocity = Mathf.Abs(initialVelocity);
        acceleration = accel;
        mode = DriveMode.Decelerating;
        StopwatchController.Instance?.ResetTimer();
        StopwatchController.Instance?.StartTimer();
    }

    public void StartPath(float[] points, float speed)
    {
        CancelInvoke();
        pathPoints = points;
        pathIndex = 0;
        pathSpeed = Mathf.Max(0.4f, speed);
        if (points == null || points.Length == 0) return;
        MotionPositionController.Instance?.SetPositionMeters(points[0]);
        DistanceCalculator.Instance?.BeginTracking(points[0]);
        DisplacementCalculator.Instance?.SetInitialPosition(points[0]);
        pathIndex = 1;
        mode = DriveMode.PathSequence;
        StopwatchController.Instance?.ResetTimer();
        StopwatchController.Instance?.StartTimer();
    }

    public void ConfigureLimits(float min, float max)
    {
        minMeters = min;
        maxMeters = max;
    }
}
