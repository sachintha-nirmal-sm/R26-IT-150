using UnityEngine;

public class DistanceCalculator : MonoBehaviour
{
    public static DistanceCalculator Instance { get; private set; }

    [SerializeField] private float distanceTravelled;
    [SerializeField] private float lastPosition;
    [SerializeField] private bool tracking;

    public float DistanceTravelled => distanceTravelled;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void BeginTracking(float startPositionMeters)
    {
        tracking = true;
        lastPosition = startPositionMeters;
        distanceTravelled = 0f;
    }

    public void AddSample(float currentPositionMeters)
    {
        if (!tracking) return;
        distanceTravelled += Mathf.Abs(currentPositionMeters - lastPosition);
        lastPosition = currentPositionMeters;
    }

    public float CalculateStraightDistance(float initialPosition, float finalPosition)
    {
        return Mathf.Abs(finalPosition - initialPosition);
    }

    public float GetDistance() => distanceTravelled;

    public void ResetDistance()
    {
        distanceTravelled = 0f;
        lastPosition = 0f;
        tracking = false;
    }
}
