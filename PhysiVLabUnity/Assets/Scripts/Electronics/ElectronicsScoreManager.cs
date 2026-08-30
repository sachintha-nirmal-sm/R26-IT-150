using System.Collections.Generic;
using UnityEngine;

public class ElectronicsScoreManager : MonoBehaviour
{
    public static ElectronicsScoreManager Instance { get; private set; }

    [SerializeField] private int displayedScore;
    [SerializeField] private int maxScore = 100;
    [SerializeField] private int penalties;

    private readonly Dictionary<ElectronicsScoreCategory, int> earned = new Dictionary<ElectronicsScoreCategory, int>();
    private readonly Dictionary<ElectronicsScoreCategory, int> caps = new Dictionary<ElectronicsScoreCategory, int>
    {
        { ElectronicsScoreCategory.Equipment, 15 },
        { ElectronicsScoreCategory.Placement, 5 },
        { ElectronicsScoreCategory.ForwardCircuit, 15 },
        { ElectronicsScoreCategory.ForwardObservation, 10 },
        { ElectronicsScoreCategory.BatteryReverse, 10 },
        { ElectronicsScoreCategory.ReverseCircuit, 15 },
        { ElectronicsScoreCategory.ReverseObservation, 10 },
        { ElectronicsScoreCategory.Comparison, 10 },
        { ElectronicsScoreCategory.Questions, 5 },
        { ElectronicsScoreCategory.Conclusion, 5 }
    };

    public int MaxScore => maxScore;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        ResetScore();
    }

    public void AddScore(int amount, bool showFeedback = true)
    {
        AddToCategory(ElectronicsScoreCategory.Questions, amount, showFeedback);
    }

    public void AddToCategory(ElectronicsScoreCategory category, int amount, bool showFeedback = true)
    {
        if (amount <= 0) return;
        if (!earned.ContainsKey(category)) earned[category] = 0;
        int room = GetCap(category) - earned[category];
        int applied = Mathf.Min(amount, Mathf.Max(0, room));
        earned[category] += applied;
        Recalculate();
        if (showFeedback) ElectronicsFeedbackManager.Instance?.ShowCorrect($"+{amount} MARKS");
    }

    public void SubtractScore(int amount)
    {
        if (amount <= 0) return;
        penalties += amount;
        Recalculate();
        ElectronicsFeedbackManager.Instance?.ShowIncorrect($"-{amount} MARKS");
        ElectronicsPracticalManager.Instance?.RegisterMistake();
    }

    public int GetScore() => Mathf.Clamp(displayedScore, 0, maxScore);

    public void ResetScore()
    {
        earned.Clear();
        foreach (ElectronicsScoreCategory cat in System.Enum.GetValues(typeof(ElectronicsScoreCategory)))
            earned[cat] = 0;
        penalties = 0;
        displayedScore = 0;
        ElectronicsUIManager.Instance?.UpdateScoreDisplay(0);
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
        ElectronicsUIManager.Instance?.UpdateScoreDisplay(GetScore());
    }

    private int GetCap(ElectronicsScoreCategory category)
    {
        return caps.TryGetValue(category, out int cap) ? cap : 5;
    }
}
