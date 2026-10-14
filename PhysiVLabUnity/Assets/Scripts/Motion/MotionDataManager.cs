using System.Collections.Generic;
using UnityEngine;

public class MotionDataManager : MonoBehaviour
{
    public static MotionDataManager Instance { get; private set; }

    [SerializeField] private List<MotionTrialData> trials = new List<MotionTrialData>();
    [SerializeField] private List<AccelerationTrialData> accelerationTrials = new List<AccelerationTrialData>();
    [SerializeField] private List<MotionGraphSample> samples = new List<MotionGraphSample>();

    public float DecelerationU { get; private set; }
    public float DecelerationV { get; private set; }
    public float DecelerationT { get; private set; }
    public float DecelerationA { get; private set; }
    public float PathDistance { get; private set; }
    public float PathDisplacement { get; private set; }

    public IReadOnlyList<MotionTrialData> Trials => trials;
    public IReadOnlyList<AccelerationTrialData> AccelerationTrials => accelerationTrials;
    public IReadOnlyList<MotionGraphSample> Samples => samples;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddOrReplaceTrial(MotionTrialData data)
    {
        if (data == null) return;
        for (int i = 0; i < trials.Count; i++)
        {
            if (trials[i] != null && trials[i].trialNumber == data.trialNumber)
            {
                trials[i] = data;
                return;
            }
        }
        trials.Add(data);
        trials.Sort((a, b) => a.trialNumber.CompareTo(b.trialNumber));
    }

    public MotionTrialData GetTrial(int number)
    {
        foreach (var t in trials)
            if (t != null && t.trialNumber == number) return t;
        return null;
    }

    public int CompletedTrialCount()
    {
        int n = 0;
        foreach (var t in trials)
            if (t != null && t.trialNumber > 0 && t.time > 0f) n++;
        return n;
    }

    public void AddAccelerationTrial(AccelerationTrialData data)
    {
        if (data == null) return;
        for (int i = 0; i < accelerationTrials.Count; i++)
        {
            if (accelerationTrials[i] != null && accelerationTrials[i].trialNumber == data.trialNumber)
            {
                accelerationTrials[i] = data;
                return;
            }
        }
        accelerationTrials.Add(data);
    }

    public void AddSample(float time, float distance, float velocity, float acceleration)
    {
        samples.Add(new MotionGraphSample
        {
            time = time,
            distance = distance,
            velocity = velocity,
            acceleration = acceleration
        });
        if (samples.Count > 400) samples.RemoveAt(0);
    }

    public void ClearSamples() => samples.Clear();

    public void SetPathResult(float distance, float displacement)
    {
        PathDistance = distance;
        PathDisplacement = displacement;
    }

    public void SetDeceleration(float u, float v, float t, float a)
    {
        DecelerationU = u;
        DecelerationV = v;
        DecelerationT = t;
        DecelerationA = a;
    }

    public float AverageSpeed()
    {
        if (trials.Count == 0) return 0f;
        float sum = 0f;
        int n = 0;
        foreach (var t in trials)
        {
            if (t == null || t.time <= 0f) continue;
            sum += t.speed;
            n++;
        }
        return n > 0 ? sum / n : 0f;
    }

    public float AverageVelocity()
    {
        if (trials.Count == 0) return 0f;
        float sum = 0f;
        int n = 0;
        foreach (var t in trials)
        {
            if (t == null || t.time <= 0f) continue;
            sum += t.velocity;
            n++;
        }
        return n > 0 ? sum / n : 0f;
    }

    public float LatestAcceleration()
    {
        if (accelerationTrials.Count == 0) return 0f;
        return accelerationTrials[accelerationTrials.Count - 1].acceleration;
    }

    public void ResetReadings()
    {
        trials.Clear();
        accelerationTrials.Clear();
        samples.Clear();
        PathDistance = 0f;
        PathDisplacement = 0f;
        DecelerationU = DecelerationV = DecelerationT = DecelerationA = 0f;
    }
}
