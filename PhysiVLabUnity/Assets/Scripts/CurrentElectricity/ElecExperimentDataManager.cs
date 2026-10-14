using System.Collections.Generic;
using UnityEngine;

public class ElecExperimentDataManager : MonoBehaviour
{
    public static ElecExperimentDataManager Instance { get; private set; }

    [SerializeField] private List<CircuitReading> readings = new List<CircuitReading>();

    public IReadOnlyList<CircuitReading> Readings => readings;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        EnsureSlots();
    }

    public void ResetReadings()
    {
        readings.Clear();
        EnsureSlots();
    }

    public void Store(CircuitReading reading)
    {
        if (reading == null) return;
        EnsureSlots();
        int i = Mathf.Clamp(reading.connectionNumber - 1, 0, 2);
        readings[i] = reading;
    }

    public CircuitReading Get(int connectionNumber)
    {
        EnsureSlots();
        int i = Mathf.Clamp(connectionNumber - 1, 0, 2);
        return readings[i];
    }

    public bool AllRecorded()
    {
        EnsureSlots();
        for (int i = 0; i < 3; i++)
            if (readings[i] == null || readings[i].connectionNumber != i + 1) return false;
        return true;
    }

    public int CompletedCount()
    {
        int n = 0;
        if (readings == null) return 0;
        foreach (var r in readings)
            if (r != null && r.connectionNumber > 0) n++;
        return n;
    }

    private void EnsureSlots()
    {
        if (readings == null) readings = new List<CircuitReading>();
        while (readings.Count < 3) readings.Add(new CircuitReading());
    }
}
