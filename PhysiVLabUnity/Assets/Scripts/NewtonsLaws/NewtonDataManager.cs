using System.Collections.Generic;
using UnityEngine;

public class NewtonDataManager : MonoBehaviour
{
    public static NewtonDataManager Instance { get; private set; }

    private readonly List<NewtonLawTrialData> forceSeries = new List<NewtonLawTrialData>();
    private readonly List<NewtonLawTrialData> massSeries = new List<NewtonLawTrialData>();
    private readonly List<NewtonLawTrialData> weightSeries = new List<NewtonLawTrialData>();

    public IReadOnlyList<NewtonLawTrialData> ForceSeries => forceSeries;
    public IReadOnlyList<NewtonLawTrialData> MassSeries => massSeries;
    public IReadOnlyList<NewtonLawTrialData> WeightSeries => weightSeries;

    public bool FirstLawComplete { get; set; }
    public bool SecondLawComplete { get; set; }
    public bool ThirdLawComplete { get; set; }
    public bool WeightComplete { get; set; }

    public string StationaryObservation { get; set; }
    public string MovingObservation { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ResetReadings()
    {
        forceSeries.Clear();
        massSeries.Clear();
        weightSeries.Clear();
        FirstLawComplete = SecondLawComplete = ThirdLawComplete = WeightComplete = false;
        StationaryObservation = MovingObservation = "";
    }

    public void AddForceTrial(NewtonLawTrialData data)
    {
        if (data == null) return;
        data.series = "Force";
        data.trialNumber = forceSeries.Count + 1;
        forceSeries.Add(data);
    }

    public void AddMassTrial(NewtonLawTrialData data)
    {
        if (data == null) return;
        data.series = "Mass";
        data.trialNumber = massSeries.Count + 1;
        massSeries.Add(data);
    }

    public void AddWeightTrial(NewtonLawTrialData data)
    {
        if (data == null) return;
        data.series = "Weight";
        data.trialNumber = weightSeries.Count + 1;
        weightSeries.Add(data);
    }

    public int CompletedTrialCount() => forceSeries.Count + massSeries.Count + weightSeries.Count;

    public int ActivitiesCompleted()
    {
        int n = 0;
        if (FirstLawComplete) n++;
        if (SecondLawComplete) n++;
        if (ThirdLawComplete) n++;
        if (WeightComplete) n++;
        return n;
    }

    public float HighestForce()
    {
        float max = 0f;
        foreach (var t in forceSeries) if (t.force > max) max = t.force;
        foreach (var t in massSeries) if (t.force > max) max = t.force;
        return max;
    }

    public float LowestMass()
    {
        float min = float.MaxValue;
        foreach (var t in forceSeries) if (t.mass > 0f && t.mass < min) min = t.mass;
        foreach (var t in massSeries) if (t.mass > 0f && t.mass < min) min = t.mass;
        foreach (var t in weightSeries) if (t.mass > 0f && t.mass < min) min = t.mass;
        return min == float.MaxValue ? 0f : min;
    }

    public float HighestAcceleration()
    {
        float max = 0f;
        foreach (var t in forceSeries) if (t.acceleration > max) max = t.acceleration;
        foreach (var t in massSeries) if (t.acceleration > max) max = t.acceleration;
        return max;
    }

    public List<NewtonLawTrialData> AllTrials()
    {
        var list = new List<NewtonLawTrialData>();
        list.AddRange(forceSeries);
        list.AddRange(massSeries);
        list.AddRange(weightSeries);
        return list;
    }
}
