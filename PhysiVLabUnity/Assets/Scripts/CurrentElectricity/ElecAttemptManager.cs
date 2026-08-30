using System.Collections.Generic;
using UnityEngine;

public class ElecAttemptManager : MonoBehaviour
{
    public static ElecAttemptManager Instance { get; private set; }

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

    public ElecAttemptRecord RegisterAttempt(int score, int mistakes, List<CircuitReading> readings, string status)
    {
        attemptsUsed++;
        int completed = 0;
        if (readings != null)
        {
            foreach (var r in readings)
                if (r != null && r.connectionNumber > 0) completed++;
        }

        var record = new ElecAttemptRecord
        {
            attemptNumber = attemptsUsed,
            score = score,
            mistakes = mistakes,
            status = status,
            date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            connectionsCompleted = completed,
            readings = readings != null ? new List<CircuitReading>(readings) : new List<CircuitReading>()
        };
        ElecUIManager.Instance?.UpdateAttemptsDisplay(AttemptsRemaining);
        return record;
    }

    public void ResetSessionAttempts()
    {
        attemptsUsed = 0;
        ElecUIManager.Instance?.UpdateAttemptsDisplay(AttemptsRemaining);
    }
}
