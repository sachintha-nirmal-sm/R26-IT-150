using System.Collections.Generic;
using UnityEngine;

public class LeverAttemptManager : MonoBehaviour
{
    public static LeverAttemptManager Instance { get; private set; }

    [SerializeField] private int maxAttempts = 3;
    [SerializeField] private int attemptsUsed;

    public int AttemptsRemaining => Mathf.Max(0, maxAttempts - attemptsUsed);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool CanRetry() => AttemptsRemaining > 0;

    public LeverAttemptRecord RegisterAttempt(int score, int mistakes, List<LeverReading> readings, string status)
    {
        attemptsUsed++;
        var record = new LeverAttemptRecord
        {
            attemptNumber = attemptsUsed,
            score = score,
            mistakes = mistakes,
            status = status,
            date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            readings = readings != null ? new List<LeverReading>(readings) : new List<LeverReading>()
        };
        LeverUIManager.Instance?.UpdateAttemptsDisplay(AttemptsRemaining);
        return record;
    }

    public void ResetSessionAttempts()
    {
        attemptsUsed = 0;
        LeverUIManager.Instance?.UpdateAttemptsDisplay(AttemptsRemaining);
    }
}
