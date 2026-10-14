using UnityEngine;

public class ElectronicsAttemptManager : MonoBehaviour
{
    public static ElectronicsAttemptManager Instance { get; private set; }

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

    public ElectronicsAttemptRecord RegisterAttempt(int score, int mistakes, string status)
    {
        attemptsUsed++;
        var circuit = ElectronicsCircuitConnectionManager.Instance;
        var obs = ElectronicsObservationManager.Instance;
        var record = new ElectronicsAttemptRecord
        {
            attemptNumber = attemptsUsed,
            score = score,
            mistakes = mistakes,
            status = status,
            date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            selectedCorrectEquipment = ElectronicsEquipmentSelectionManager.Instance != null && ElectronicsEquipmentSelectionManager.Instance.IsEquipmentComplete,
            circuitConnected = circuit != null && circuit.IsCircuitComplete(),
            forwardBiasCompleted = ElectronicsForwardBiasController.Instance != null && ElectronicsForwardBiasController.Instance.IsCompleted,
            reverseBiasCompleted = ElectronicsReverseBiasController.Instance != null && ElectronicsReverseBiasController.Instance.IsCompleted,
            observationCompleted = obs != null && obs.IsComplete,
            questionsCompleted = ElectronicsQuestionManager.Instance != null && ElectronicsQuestionManager.Instance.IsFinished,
            conclusionCompleted = ElectronicsConclusionManager.Instance != null && ElectronicsConclusionManager.Instance.IsCorrect,
            summary = BuildSummary()
        };
        ElectronicsUIManager.Instance?.UpdateAttemptsDisplay(CurrentAttemptNumber, maxAttempts);
        ElectronicsProgressManager.Instance?.RefreshHeader();
        return record;
    }

    public void ResetSessionAttempts()
    {
        attemptsUsed = 0;
        ElectronicsUIManager.Instance?.UpdateAttemptsDisplay(CurrentAttemptNumber, maxAttempts);
    }

    private static string BuildSummary()
    {
        var obs = ElectronicsObservationManager.Instance;
        string forward = obs != null && obs.Forward != null ? obs.Forward.observationText : "";
        string reverse = obs != null && obs.Reverse != null ? obs.Reverse.observationText : "";
        return $"Forward: {forward} | Reverse: {reverse}";
    }
}
