using System.Collections.Generic;
using UnityEngine;

public class TurningAttemptManager : MonoBehaviour
{
    public static TurningAttemptManager Instance { get; private set; }

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

    public void Configure(int max)
    {
        maxAttempts = Mathf.Max(1, max);
    }

    public bool CanRetry() => AttemptsRemaining > 0;

    public TurningAttemptRecord RegisterAttempt(int score, int mistakes, List<TurningTrialData> trials, string status)
    {
        attemptsUsed++;
        int completed = 0;
        if (trials != null)
        {
            foreach (var t in trials)
                if (t != null && t.completed) completed++;
        }

        var record = new TurningAttemptRecord
        {
            attemptNumber = attemptsUsed,
            score = score,
            mistakes = mistakes,
            status = status,
            date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            trialsCompleted = completed,
            trials = trials != null ? new List<TurningTrialData>(trials) : new List<TurningTrialData>()
        };
        TurningUIManager.Instance?.UpdateAttemptsDisplay(AttemptsRemaining);
        return record;
    }

    public void ResetSessionAttempts()
    {
        attemptsUsed = 0;
        TurningUIManager.Instance?.UpdateAttemptsDisplay(AttemptsRemaining);
    }
}
