using UnityEngine;

public class OpticsAttemptManager : MonoBehaviour
{
    public static OpticsAttemptManager Instance { get; private set; }

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

    public OpticsAttemptRecord RegisterAttempt(int score, int mistakes, string status)
    {
        attemptsUsed++;
        var asm = OpticsAssemblyManager.Instance;
        var vis = OpticsVisualController.Instance;
        var record = new OpticsAttemptRecord
        {
            attemptNumber = attemptsUsed,
            score = score,
            mistakes = mistakes,
            status = status,
            date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            trialsCompleted = vis != null && vis.IsInFocus ? 1 : 0,
            selectedCorrectEquipment = OpticsEquipmentSelectionManager.Instance != null && OpticsEquipmentSelectionManager.Instance.IsEquipmentComplete,
            windowOpened = asm != null && asm.WindowOpened,
            mirrorFacingWindow = asm != null && asm.MirrorPlaced,
            foundSharpImage = vis != null && vis.IsInFocus,
            measuredFocalLength = OpticsExperimentManager.Instance != null && OpticsExperimentManager.Instance.MeasuredFocalLength
        };
        OpticsUIManager.Instance?.UpdateAttemptsDisplay(AttemptsRemaining);
        return record;
    }

    public void ResetSessionAttempts()
    {
        attemptsUsed = 0;
        OpticsUIManager.Instance?.UpdateAttemptsDisplay(AttemptsRemaining);
    }
}
