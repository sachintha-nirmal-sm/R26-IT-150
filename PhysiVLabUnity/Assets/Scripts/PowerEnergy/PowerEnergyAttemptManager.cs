using UnityEngine;

public class PowerEnergyAttemptManager : MonoBehaviour
{
    public static PowerEnergyAttemptManager Instance { get; private set; }

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

    public PowerEnergyAttemptRecord RegisterAttempt(int score, int mistakes, string status)
    {
        attemptsUsed++;
        var appliances = PowerEnergyApplianceController.Instance;
        var record = new PowerEnergyAttemptRecord
        {
            attemptNumber = attemptsUsed,
            score = score,
            mistakes = mistakes,
            status = status,
            date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            appliancesCompleted = appliances != null ? appliances.CompletedCount : 0,
            powerCalculations = appliances != null ? appliances.PowerCount : 0,
            energyCalculations = appliances != null ? appliances.EnergyCount : 0,
            kwhConversions = appliances != null ? appliances.KwhCount : 0,
            selectedCorrectEquipment = PowerEnergyEquipmentSelectionManager.Instance != null && PowerEnergyEquipmentSelectionManager.Instance.IsEquipmentComplete,
            circuitConnected = PowerEnergyCircuitConnectionManager.Instance != null && PowerEnergyCircuitConnectionManager.Instance.IsComplete,
            conclusionCompleted = PowerEnergyConclusionManager.Instance != null && PowerEnergyConclusionManager.Instance.IsCorrect,
            applianceSummary = appliances != null ? appliances.BuildSummary() : ""
        };
        PowerEnergyUIManager.Instance?.UpdateAttemptsDisplay(AttemptsRemaining);
        return record;
    }

    public void ResetSessionAttempts()
    {
        attemptsUsed = 0;
        PowerEnergyUIManager.Instance?.UpdateAttemptsDisplay(AttemptsRemaining);
    }
}
