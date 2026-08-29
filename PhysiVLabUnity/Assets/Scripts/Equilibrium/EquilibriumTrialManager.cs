using System.Collections.Generic;
using UnityEngine;

public class EquilibriumTrialManager : MonoBehaviour
{
    public static EquilibriumTrialManager Instance { get; private set; }

    [SerializeField] private int currentTrial = 1;
    private readonly List<EquilibriumTrialData> trials = new List<EquilibriumTrialData>();

    public int CurrentTrial => currentTrial;

    public int CompletedCount
    {
        get
        {
            int n = 0;
            foreach (var t in trials) if (t != null && t.completed) n++;
            return n;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        EnsureTrials();
    }

    public void ResetAllTrials()
    {
        trials.Clear();
        EnsureTrials();
        BeginTrial(1);
    }

    public void BeginTrial(int trial)
    {
        currentTrial = Mathf.Clamp(trial, 1, 3);
        EquilibriumForceController.Instance?.PrepareTrial(currentTrial);
        EquilibriumUIManager.Instance?.RefreshTrialLabels();
    }

    public void RecordCurrentReading(float f1, float f2, float w, float tilt, bool horizontal)
    {
        EnsureTrials();
        var data = trials[currentTrial - 1];
        data.trialNumber = currentTrial;
        data.force1N = f1;
        data.force2N = f2;
        data.weightN = w;
        data.sumN = f1 + f2;
        data.tiltDeg = tilt;
        data.horizontal = horizontal;
        data.completed = true;
    }

    public EquilibriumTrialData GetTrial(int number)
    {
        EnsureTrials();
        if (number < 1 || number > 3) return null;
        return trials[number - 1];
    }

    public List<EquilibriumTrialData> GetAllTrials()
    {
        EnsureTrials();
        return trials;
    }

    public bool AllTrialsComplete()
    {
        EnsureTrials();
        foreach (var t in trials) if (t == null || !t.completed) return false;
        return true;
    }

    public void ResetCurrentTrialKeepData()
    {
        BeginTrial(currentTrial);
    }

    private void EnsureTrials()
    {
        while (trials.Count < 3)
        {
            int n = trials.Count + 1;
            trials.Add(new EquilibriumTrialData { trialNumber = n, weightN = EquilibriumForceController.TrueWeightN });
        }
    }
}
