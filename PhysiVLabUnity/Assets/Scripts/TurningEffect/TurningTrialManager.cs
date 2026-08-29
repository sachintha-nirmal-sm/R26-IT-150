using System.Collections.Generic;
using UnityEngine;

public class TurningTrialManager : MonoBehaviour
{
    public static TurningTrialManager Instance { get; private set; }

    [SerializeField] private int currentTrial = 1;
    private readonly List<TurningTrialData> trials = new List<TurningTrialData>();

    public int CurrentTrial => currentTrial;
    public int RequiredTightness => currentTrial;
    public float TargetForceN => TurningMomentController.TorqueOf(currentTrial) / 0.60f;

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
        TurningMomentController.Instance?.PrepareTrial(currentTrial);
        TurningUIManager.Instance?.RefreshTrialLabels();
    }

    public void RecordCurrentReading(float force, float angle, float moment, string point, bool moved)
    {
        EnsureTrials();
        var data = trials[currentTrial - 1];
        data.trialNumber = currentTrial;
        data.tightnessLevel = currentTrial;
        data.point = point;
        data.distanceCm = TurningMomentController.DistanceOf(point) * 100f;
        data.angleDeg = angle;
        data.forceN = force;
        data.momentNm = moment;
        data.targetForceN = TargetForceN;
        data.stickMoved = moved;
        data.completed = true;
    }

    public TurningTrialData GetTrial(int number)
    {
        EnsureTrials();
        if (number < 1 || number > 3) return null;
        return trials[number - 1];
    }

    public List<TurningTrialData> GetAllTrials()
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
            trials.Add(new TurningTrialData
            {
                trialNumber = n,
                tightnessLevel = n,
                point = "D",
                distanceCm = 60f,
                targetForceN = TurningMomentController.TorqueOf(n) / 0.60f
            });
        }
    }
}
