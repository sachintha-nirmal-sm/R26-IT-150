using System.Collections.Generic;
using UnityEngine;

public class WorkEnergyAttemptManager : MonoBehaviour
{
    public static WorkEnergyAttemptManager Instance { get; private set; }

    [SerializeField] private int maxAttempts = 3;
    [SerializeField] private int attemptsUsed;

    public int MaxAttempts => maxAttempts;
    public int AttemptsRemaining => Mathf.Max(0, maxAttempts - attemptsUsed);
    public int CurrentAttemptNumber => Mathf.Min(attemptsUsed + 1, maxAttempts);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool CanRetry() => AttemptsRemaining > 0;

    public WorkEnergyAttemptRecord RegisterAttempt(int score, int mistakes, List<EnergyHeightReading> readings, string status)
    {
        attemptsUsed++;
        var record = new WorkEnergyAttemptRecord
        {
            attemptNumber = attemptsUsed,
            score = score,
            mistakes = mistakes,
            status = status,
            date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            readings = readings != null ? new List<EnergyHeightReading>(readings) : new List<EnergyHeightReading>()
        };
        WorkEnergyUIManager.Instance?.UpdateAttemptsDisplay(AttemptsRemaining);
        return record;
    }

    public void ResetSessionAttempts()
    {
        attemptsUsed = 0;
        WorkEnergyUIManager.Instance?.UpdateAttemptsDisplay(AttemptsRemaining);
    }
}
