using System.Collections.Generic;
using UnityEngine;

public class ResultantTrialManager : MonoBehaviour
{
    public static ResultantTrialManager Instance { get; private set; }

    [SerializeField] private int currentTrial = 1;
    private readonly List<ResultantTrialData> trials = new List<ResultantTrialData>();

    public int CurrentTrial => currentTrial;
    public float TargetB => currentTrial == 1 ? 5f : currentTrial == 2 ? 4f : 2f;
    public float TargetC => currentTrial == 1 ? 3f : currentTrial == 2 ? 6f : 7f;
    public float TargetA => TargetB + TargetC;

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
        ResultantForceController.Instance?.ResetForces();
        ResultantUIManager.Instance?.RefreshTrialLabels();
    }

    public void RecordCurrentReading(float b, float c, float a)
    {
        EnsureTrials();
        var data = trials[currentTrial - 1];
        data.trialNumber = currentTrial;
        data.forceB = b;
        data.forceC = c;
        data.forceA = a;
        data.targetB = TargetB;
        data.targetC = TargetC;
        data.completed = true;
    }

    public ResultantTrialData GetTrial(int number)
    {
        EnsureTrials();
        if (number < 1 || number > 3) return null;
        return trials[number - 1];
    }

    public List<ResultantTrialData> GetAllTrials()
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
            float tb = n == 1 ? 5f : n == 2 ? 4f : 2f;
            float tc = n == 1 ? 3f : n == 2 ? 6f : 7f;
            trials.Add(new ResultantTrialData
            {
                trialNumber = n,
                targetB = tb,
                targetC = tc
            });
        }
    }
}
