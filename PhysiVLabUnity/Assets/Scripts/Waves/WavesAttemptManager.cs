using UnityEngine;

public class WavesAttemptManager : MonoBehaviour
{
    public static WavesAttemptManager Instance { get; private set; }

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

    public WavesAttemptRecord RegisterAttempt(int score, int mistakes, string status)
    {
        attemptsUsed++;
        var assembly = WavesAssemblyManager.Instance;
        var wave = WavesWaveController.Instance;
        var record = new WavesAttemptRecord
        {
            attemptNumber = attemptsUsed,
            score = score,
            mistakes = mistakes,
            status = status,
            date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            trialsCompleted = wave != null && wave.HasTransverseWave ? 1 : 0,
            selectedCorrectEquipment = WavesEquipmentSelectionManager.Instance != null && WavesEquipmentSelectionManager.Instance.IsEquipmentComplete,
            tiedRibbons = assembly != null && assembly.AllRibbonsTied,
            shookTransverse = wave != null && wave.HasTransverseWave,
            identifiedPerpendicularMotion = WavesExperimentManager.Instance != null && WavesExperimentManager.Instance.IdentifiedMotion
        };
        WavesUIManager.Instance?.UpdateAttemptsDisplay(AttemptsRemaining);
        return record;
    }

    public void ResetSessionAttempts()
    {
        attemptsUsed = 0;
        WavesUIManager.Instance?.UpdateAttemptsDisplay(AttemptsRemaining);
    }
}
