using UnityEngine;

public class HeatAttemptManager : MonoBehaviour
{
    public static HeatAttemptManager Instance { get; private set; }

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

    public HeatAttemptRecord RegisterAttempt(int score, int mistakes, string status)
    {
        attemptsUsed++;
        var asm = HeatAssemblyManager.Instance;
        var vis = HeatVisualController.Instance;
        var mgr = HeatExperimentManager.Instance;
        var record = new HeatAttemptRecord
        {
            attemptNumber = attemptsUsed,
            score = score,
            mistakes = mistakes,
            status = status,
            date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            trialsCompleted = vis != null && vis.ReachedLevelC ? 1 : 0,
            selectedCorrectEquipment = HeatEquipmentSelectionManager.Instance != null && HeatEquipmentSelectionManager.Instance.IsEquipmentComplete,
            apparatusAssembled = asm != null && asm.SetupConfirmed,
            markedLevelA = asm != null && asm.LevelAMarked,
            observedDropToB = vis != null && vis.ReachedLevelB,
            observedRiseToC = vis != null && vis.ReachedLevelC,
            identifiedLevels = mgr != null && mgr.IdentifiedLevels
        };
        HeatUIManager.Instance?.UpdateAttemptsDisplay(AttemptsRemaining);
        return record;
    }

    public void ResetSessionAttempts()
    {
        attemptsUsed = 0;
        HeatUIManager.Instance?.UpdateAttemptsDisplay(AttemptsRemaining);
    }
}
