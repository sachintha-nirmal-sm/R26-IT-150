using System.Collections.Generic;
using UnityEngine;

public class LeverExperimentDataManager : MonoBehaviour
{
    public static LeverExperimentDataManager Instance { get; private set; }

    public float bookWeight = 10f;
    public float distanceA = 20f;
    public float[] distanceXValues = { 10f, 20f, 30f, 40f };
    public float distanceTolerance = 0.5f;

    [SerializeField] private int currentXIndex;

    private readonly List<LeverReading> readings = new List<LeverReading>();

    public int MaxInstances => distanceXValues != null ? distanceXValues.Length : 0;
    public int CurrentXIndex => currentXIndex;
    public IReadOnlyList<LeverReading> Readings => readings;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public float GetXValue(int index)
    {
        if (distanceXValues == null || distanceXValues.Length == 0) return 0f;
        return distanceXValues[Mathf.Clamp(index, 0, distanceXValues.Length - 1)];
    }

    public float GetCurrentX() => GetXValue(currentXIndex);

    public bool HasMoreXValues() => currentXIndex < MaxInstances - 1;

    public bool AdvanceXIndex()
    {
        if (!HasMoreXValues()) return false;
        currentXIndex++;
        return true;
    }

    public void SetCurrentXIndex(int index)
    {
        currentXIndex = Mathf.Clamp(index, 0, Mathf.Max(0, MaxInstances - 1));
    }

    public float GetRequiredEffort(float x)
    {
        if (LeverPhysicsController.Instance != null)
            return LeverPhysicsController.Instance.CalculateRequiredEffort(bookWeight, distanceA, x);
        return (bookWeight * distanceA) / Mathf.Max(0.01f, x);
    }

    public float GetRequiredEffortForCurrentX() => GetRequiredEffort(GetCurrentX());

    public void RecordReading(int instance, float a, float x, float load, float requiredEffort, float measuredEffort, bool bookLifted)
    {
        var reading = new LeverReading
        {
            instance = instance,
            distanceA = a,
            distanceX = x,
            bookWeight = load,
            requiredEffort = requiredEffort,
            measuredEffort = measuredEffort,
            bookLifted = bookLifted
        };

        for (int i = 0; i < readings.Count; i++)
        {
            if (readings[i].instance == instance)
            {
                readings[i] = reading;
                return;
            }
        }
        readings.Add(reading);
    }

    public void RecordReadingForCurrentX(float measuredEffort, bool bookLifted)
    {
        float x = GetCurrentX();
        float required = GetRequiredEffort(x);
        RecordReading(currentXIndex + 1, distanceA, x, bookWeight, required, measuredEffort, bookLifted);
    }

    public bool IsDistanceACorrect(float measuredA) =>
        Mathf.Abs(measuredA - distanceA) <= distanceTolerance;

    public bool IsDistanceXCorrect(float measuredX, float expectedX) =>
        Mathf.Abs(measuredX - expectedX) <= distanceTolerance;

    public bool AllInstancesRecorded() => readings.Count >= MaxInstances;

    public void ResetReadings()
    {
        readings.Clear();
        currentXIndex = 0;
    }
}
