using System.Collections.Generic;
using UnityEngine;

public class PowerEnergyScoreManager : MonoBehaviour
{
    public static PowerEnergyScoreManager Instance { get; private set; }

    [SerializeField] private int displayedScore;
    [SerializeField] private int maxScore = 100;
    [SerializeField] private int penalties;

    private readonly Dictionary<PowerEnergyScoreCategory, int> earned = new Dictionary<PowerEnergyScoreCategory, int>();
    private readonly Dictionary<PowerEnergyScoreCategory, int> caps = new Dictionary<PowerEnergyScoreCategory, int>
    {
        { PowerEnergyScoreCategory.Equipment, 10 },
        { PowerEnergyScoreCategory.Circuit, 15 },
        { PowerEnergyScoreCategory.Voltage, 10 },
        { PowerEnergyScoreCategory.Current, 10 },
        { PowerEnergyScoreCategory.Power, 15 },
        { PowerEnergyScoreCategory.Energy, 15 },
        { PowerEnergyScoreCategory.Kwh, 10 },
        { PowerEnergyScoreCategory.Observation, 5 },
        { PowerEnergyScoreCategory.Questions, 5 },
        { PowerEnergyScoreCategory.Conclusion, 5 }
    };

    public int MaxScore => maxScore;
    public int EquipmentScore => 4;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        ResetScore();
    }

    public void Configure(int correct, int wrong, int major) { }

    public void ConfigureMaxRaw(int requiredEquipmentCount = 7) { }

    public void AddScore(int amount, bool showFeedback = true)
    {
        AddToCategory(PowerEnergyScoreCategory.Questions, amount, showFeedback);
    }

    public void AddToCategory(PowerEnergyScoreCategory category, int amount, bool showFeedback = true)
    {
        if (amount <= 0) return;
        if (!earned.ContainsKey(category)) earned[category] = 0;
        int room = GetCap(category) - earned[category];
        int applied = Mathf.Min(amount, Mathf.Max(0, room));
        earned[category] += applied;
        Recalculate();
        if (showFeedback) PowerEnergyFeedbackManager.Instance?.ShowCorrect($"+{amount} MARKS");
    }

    public void SubtractScore(int amount)
    {
        if (amount <= 0) return;
        penalties += amount;
        Recalculate();
        PowerEnergyFeedbackManager.Instance?.ShowIncorrect($"-{amount} MARKS");
        PowerEnergyExperimentManager.Instance?.RegisterMistake();
    }

    public int GetScore() => Mathf.Clamp(displayedScore, 0, maxScore);

    public void ResetScore()
    {
        earned.Clear();
        foreach (PowerEnergyScoreCategory cat in System.Enum.GetValues(typeof(PowerEnergyScoreCategory)))
            earned[cat] = 0;
        penalties = 0;
        displayedScore = 0;
        PowerEnergyUIManager.Instance?.UpdateScoreDisplay(0);
    }

    public int FinalizeScore()
    {
        Recalculate();
        return GetScore();
    }

    private void Recalculate()
    {
        int total = 0;
        foreach (var pair in earned) total += pair.Value;
        displayedScore = Mathf.Clamp(total - penalties, 0, maxScore);
        PowerEnergyUIManager.Instance?.UpdateScoreDisplay(GetScore());
    }

    private int GetCap(PowerEnergyScoreCategory category)
    {
        return caps.TryGetValue(category, out int cap) ? cap : 5;
    }
}
