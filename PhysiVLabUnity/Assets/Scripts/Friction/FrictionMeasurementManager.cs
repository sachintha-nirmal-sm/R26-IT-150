using System.Collections.Generic;
using UnityEngine;

public class FrictionMeasurementManager : MonoBehaviour
{
    public static FrictionMeasurementManager Instance { get; private set; }

    [SerializeField] private float frozenLimitingReading;
    private readonly List<FrictionForceTimeSample> samples = new List<FrictionForceTimeSample>();
    private float sampleClock;

    public float FrozenLimitingReading => frozenLimitingReading;
    public IReadOnlyList<FrictionForceTimeSample> Samples => samples;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ResetTrialSamples()
    {
        frozenLimitingReading = 0f;
        samples.Clear();
        sampleClock = 0f;
    }

    public void SampleForce(float applied)
    {
        sampleClock += Time.deltaTime;
        samples.Add(new FrictionForceTimeSample
        {
            time = sampleClock,
            appliedForce = applied,
            frictionForce = FrictionForceController.Instance != null ? FrictionForceController.Instance.FrictionForce : 0f,
            moving = FrictionForceController.Instance != null && FrictionForceController.Instance.BlockMoving
        });
        if (samples.Count > 400) samples.RemoveAt(0);
    }

    public void FreezeLimitingReading(float value)
    {
        frozenLimitingReading = value;
    }

    public void ClearAll()
    {
        ResetTrialSamples();
    }
}
