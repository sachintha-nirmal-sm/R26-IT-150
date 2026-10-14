using System.Globalization;
using UnityEngine;

public class LeverGameManager : MonoBehaviour
{
    public static LeverGameManager Instance { get; private set; }

    [SerializeField] private int mistakeCount;
    public int MistakeCount => mistakeCount;
    private bool flutterSent;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void RegisterMistake() => mistakeCount++;

    public void StartPractical()
    {
        mistakeCount = 0;
        flutterSent = false;
        LeverScoreManager.Instance?.ResetScore();
        LeverExperimentDataManager.Instance?.ResetReadings();
        LeverLabWorkbench.Instance?.ResetWorkbench();
        LeverConclusionManager.Instance?.ResetConclusion();
        LeverEquipmentSelectionManager.Instance?.EnsureCardsVisible();
        LeverEquipmentSelectionManager.Instance?.ResetSelection();
        LeverExperimentManager.Instance?.StartExperiment();
        LeverScoreManager.Instance?.AddScore(5, false);
        LeverUIManager.Instance?.UpdateScoreDisplay(LeverScoreManager.Instance?.GetScore() ?? 0);
        LeverUIManager.Instance?.UpdateAttemptsDisplay(LeverAttemptManager.Instance?.AttemptsRemaining ?? 3);
        LeverUIManager.Instance?.HideResult();
        LeverUIManager.Instance?.SetNextButtonVisible(false);
        Debug.Log("Lever: practical started → Select Equipment");
    }

    public void CompleteExperiment()
    {
        int finalScore = LeverScoreManager.Instance?.FinalizeScore() ?? 0;
        bool passed = finalScore >= 50;
        var readings = LeverExperimentDataManager.Instance?.Readings;
        var list = readings != null
            ? new System.Collections.Generic.List<LeverReading>(readings)
            : new System.Collections.Generic.List<LeverReading>();

        LeverAttemptRecord attempt;
        if (LeverAttemptManager.Instance != null)
        {
            attempt = LeverAttemptManager.Instance.RegisterAttempt(finalScore, mistakeCount, list, passed ? "PASSED" : "TRY AGAIN");
            LeverProfileManager.Instance?.UpdatePracticalResult(finalScore, mistakeCount, passed, attempt);
        }
        else
        {
            attempt = new LeverAttemptRecord
            {
                attemptNumber = 1,
                score = finalScore,
                mistakes = mistakeCount,
                status = passed ? "PASSED" : "TRY AGAIN",
                date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                readings = list
            };
        }

        LeverUIManager.Instance?.ShowResult(finalScore, passed, mistakeCount, attempt);
        SendToFlutter(finalScore, passed);
    }

    private void SendToFlutter(int finalScore, bool passed)
    {
        if (flutterSent)
        {
            return;
        }

        flutterSent = true;
        TimerManager.Instance?.Stop();
        int timeUsed = TimerManager.Instance != null ? TimerManager.Instance.TimeUsedSeconds : 0;
        int readings = LeverExperimentDataManager.Instance != null
            ? LeverExperimentDataManager.Instance.Readings.Count
            : 0;
        string measurements =
            "{\"readings\":" + readings.ToString(CultureInfo.InvariantCulture)
            + ",\"mistakes\":" + mistakeCount.ToString(CultureInfo.InvariantCulture)
            + "}";
        FlutterBridge.EnsureInstance()?.NotifyCompleted(
            finalScore,
            passed,
            mistakeCount,
            timeUsed,
            true,
            measurements);
    }

    public void ResetExperiment()
    {
        mistakeCount = 0;
        flutterSent = false;
        LeverScoreManager.Instance?.ResetScore();
        LeverExperimentDataManager.Instance?.ResetReadings();
        LeverEquipmentSelectionManager.Instance?.ResetSelection();
        LeverLabWorkbench.Instance?.ResetWorkbench();
        LeverConclusionManager.Instance?.ResetConclusion();
        LeverExperimentManager.Instance?.ResetExperiment();
        LeverUIManager.Instance?.HideResult();
        LeverUIManager.Instance?.UpdateScoreDisplay(0);
    }

    public void RetryExperiment()
    {
        if (LeverAttemptManager.Instance != null && !LeverAttemptManager.Instance.CanRetry())
        {
            LeverFeedbackManager.Instance?.ShowInstruction("No attempts remaining.");
            return;
        }
        ResetExperiment();
        LeverUIManager.Instance?.StartPractical();
    }
}
