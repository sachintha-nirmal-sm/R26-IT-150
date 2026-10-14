using System;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Scoring for the Upthrust practical. Raw marks are converted to a 0–100 display score.
/// Score never drops below 0.
/// </summary>
public class UpthrustScoreManager : MonoBehaviour
{
    public static UpthrustScoreManager Instance { get; private set; }

    [Header("Phase 1 — Apparatus")]
    [SerializeField] private int correctApparatusMarks = 10;
    [SerializeField] private int wrongApparatusPenalty = 5;
    [SerializeField] private int maxCorrectApparatus = UpthrustPracticalData.CorrectApparatusCount;

    [Header("Phase 2 — Steps")]
    [SerializeField] private int correctStepMarks = 15;
    [SerializeField] private int stepMistakePenalty = 5;
    [SerializeField] private int maxPracticalSteps = UpthrustPracticalData.PracticalStepCount;

    [Header("Phase 3 — Observation Table")]
    [SerializeField] private int correctCellMarks = 5;
    [SerializeField] private int wrongCellPenalty = 2;
    [SerializeField] private int maxTableCells = UpthrustPracticalData.ObservationCellCount;

    private int rawScore;
    private int correctChoicesCount;
    private int incorrectMistakesCount;
    private int maxPossibleScore;
    [NonSerialized] private float lockedDisplayScore = -1f;
    private bool flutterSent;

    public int RawScore => rawScore;
    public int CorrectChoicesCount => correctChoicesCount;
    public int IncorrectMistakesCount => incorrectMistakesCount;
    public int MaxPossibleScore => maxPossibleScore;

    public float ScoreOutOf100
    {
        get
        {
            if (lockedDisplayScore >= 0f) return lockedDisplayScore;
            if (maxPossibleScore <= 0) return 0f;
            return Mathf.Clamp(Mathf.Round((rawScore / (float)maxPossibleScore) * 100f), 0f, 100f);
        }
    }

    public float PercentageScore => ScoreOutOf100;

    public event Action<int, float> OnScoreChanged;
    public event Action OnScoreFinalized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RecalculateMaxPossible();
        ResetScore();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void RecalculateMaxPossible()
    {
        maxPossibleScore = (maxCorrectApparatus * correctApparatusMarks)
                         + (maxPracticalSteps * correctStepMarks)
                         + (maxTableCells * correctCellMarks);

        if (maxPossibleScore <= 0)
            maxPossibleScore = 220;
    }

    public void ResetScore()
    {
        rawScore = 0;
        correctChoicesCount = 0;
        incorrectMistakesCount = 0;
        lockedDisplayScore = -1f;
        flutterSent = false;
        RecalculateMaxPossible();
        NotifyScoreChanged();
    }

    public void RegisterCorrectApparatus()
    {
        correctChoicesCount++;
        AddScore(correctApparatusMarks);
    }

    public void RegisterWrongApparatus()
    {
        incorrectMistakesCount++;
        SubtractScore(wrongApparatusPenalty);
    }

    public void RegisterStepSuccess(bool firstTry)
    {
        correctChoicesCount++;
        AddScore(correctStepMarks);
    }

    public void RegisterStepMistake()
    {
        incorrectMistakesCount++;
        SubtractScore(stepMistakePenalty);
    }

    public void RegisterCorrectTableCell()
    {
        correctChoicesCount++;
        AddScore(correctCellMarks);
    }

    public void RegisterWrongTableCell()
    {
        incorrectMistakesCount++;
        SubtractScore(wrongCellPenalty);
    }

    private void AddScore(int amount)
    {
        rawScore += Mathf.Max(0, amount);
        NotifyScoreChanged();
    }

    private void SubtractScore(int amount)
    {
        rawScore = Mathf.Max(0, rawScore - Mathf.Max(0, amount));
        NotifyScoreChanged();
    }

    private void NotifyScoreChanged()
    {
        OnScoreChanged?.Invoke(rawScore, ScoreOutOf100);
    }

    public void FinalizeScore()
    {
        RecalculateMaxPossible();
        lockedDisplayScore = ScoreOutOf100;

        if (UpthrustProfileManager.Instance != null)
            UpthrustProfileManager.Instance.SavePracticalResult(lockedDisplayScore, true);

        SendToFlutter();
        OnScoreFinalized?.Invoke();
        Debug.Log($"[UpthrustScoreManager] Final {lockedDisplayScore}/100 (raw {rawScore}/{maxPossibleScore})");
    }

    private void SendToFlutter()
    {
        if (flutterSent)
        {
            return;
        }

        flutterSent = true;
        TimerManager.Instance?.Stop();
        int score = Mathf.Clamp(Mathf.RoundToInt(lockedDisplayScore), 0, 100);
        bool passed = score >= 50;
        int timeUsed = TimerManager.Instance != null ? TimerManager.Instance.TimeUsedSeconds : 0;
        string measurements =
            "{\"correct\":" + correctChoicesCount.ToString(CultureInfo.InvariantCulture)
            + ",\"mistakes\":" + incorrectMistakesCount.ToString(CultureInfo.InvariantCulture)
            + "}";
        FlutterBridge.EnsureInstance()?.NotifyCompleted(
            score,
            passed,
            incorrectMistakesCount,
            timeUsed,
            true,
            measurements);
    }

    /// <summary>3 stars ≥ 80%, 2 stars ≥ 50%, 1 star otherwise.</summary>
    public int GetStarRating()
    {
        float p = ScoreOutOf100;
        if (p >= 80f) return 3;
        if (p >= 50f) return 2;
        return 1;
    }

    public string GetPerformanceGrade()
    {
        float p = ScoreOutOf100;
        if (p >= 80f) return "Excellent";
        if (p >= 50f) return "Good";
        return "Needs Improvement";
    }
}
