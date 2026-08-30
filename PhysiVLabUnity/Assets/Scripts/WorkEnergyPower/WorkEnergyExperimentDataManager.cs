using System.Collections.Generic;
using UnityEngine;

public class WorkEnergyExperimentDataManager : MonoBehaviour
{
    public static WorkEnergyExperimentDataManager Instance { get; private set; }

    [SerializeField] private float weightMass = 1.0f;
    [SerializeField] private float[] experimentHeights = { 0.20f, 0.30f, 0.40f, 0.50f, 0.60f };

    private readonly List<EnergyHeightReading> readings = new List<EnergyHeightReading>();
    private float measuredMass = -1f;

    public float WeightMass => weightMass;
    public float[] ExperimentHeights => experimentHeights;
    public IReadOnlyList<EnergyHeightReading> Readings => readings;
    public float StoredMass => measuredMass > 0f ? measuredMass : weightMass;
    public bool MassWasMeasured => measuredMass > 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetMass(float mass)
    {
        if (mass <= 0f) return;
        weightMass = mass;
        measuredMass = mass;
    }

    public void StoreMeasuredMass(float mass)
    {
        measuredMass = mass;
        weightMass = mass;
    }

    public EnergyHeightReading RecordReading(int instance, float mass, float height, float potentialEnergy, float depressionDepth)
    {
        var reading = new EnergyHeightReading
        {
            instance = instance,
            mass = mass,
            height = height,
            potentialEnergy = potentialEnergy,
            depressionDepth = depressionDepth
        };

        for (int i = 0; i < readings.Count; i++)
        {
            if (readings[i].instance == instance)
            {
                readings[i] = reading;
                return reading;
            }
        }

        readings.Add(reading);
        return reading;
    }

    public bool HasReadingForHeight(float height, float tolerance = 0.001f)
    {
        for (int i = 0; i < readings.Count; i++)
        {
            if (Mathf.Abs(readings[i].height - height) <= tolerance) return true;
        }
        return false;
    }

    public bool AllHeightsRecorded()
    {
        if (experimentHeights == null) return false;
        return readings.Count >= experimentHeights.Length;
    }

    public float GetNextUnrecordedHeight()
    {
        if (experimentHeights == null) return 0.5f;
        for (int i = 0; i < experimentHeights.Length; i++)
        {
            if (!HasReadingForHeight(experimentHeights[i])) return experimentHeights[i];
        }
        return experimentHeights[experimentHeights.Length - 1];
    }

    public void ResetReadings()
    {
        readings.Clear();
        measuredMass = -1f;
    }
}
