using UnityEngine;

public class WorkEnergyScoreManager : MonoBehaviour
{
    public static WorkEnergyScoreManager Instance { get; private set; }

    [SerializeField] private int rawScore;
    [SerializeField] private int maxRawScore = 270;
    [SerializeField] private int maxScore = 100;
    [SerializeField] private int correctScore = 5;
    [SerializeField] private int wrongPenalty = 5;

    public int RawScore => rawScore;
    public int MaxScore => maxScore;
    public int CorrectScore => correctScore;
    public int WrongPenalty => wrongPenalty;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ConfigureMaxRaw(int requiredEquipmentCount, int heightCount)
    {
        maxRawScore =
            5 +
            correctScore * requiredEquipmentCount +
            5 + 5 + 5 +
            heightCount * (5 + 5 + 5 + 5 + 5 + 5) +
            5 + 5 +
            10 + 10 + 10 +
            5 + 10 + 5;
        if (maxRawScore < 1) maxRawScore = 1;
    }

    public void AddScore(int amount, bool showFeedback = true)
    {
        if (amount <= 0) return;
        rawScore += amount;
        WorkEnergyUIManager.Instance?.UpdateScoreDisplay(GetScore());
        if (showFeedback) WorkEnergyFeedbackManager.Instance?.ShowCorrect($"+{amount} Marks");
    }

    public void SubtractScore(int amount)
    {
        if (amount <= 0) return;
        rawScore = Mathf.Max(0, rawScore - amount);
        WorkEnergyUIManager.Instance?.UpdateScoreDisplay(GetScore());
        WorkEnergyFeedbackManager.Instance?.ShowIncorrect($"-{amount} Marks");
        WorkEnergyPowerExperimentManager.Instance?.RegisterMistake();
    }

    public int GetScore()
    {
        float normalized = maxRawScore > 0 ? (rawScore / (float)maxRawScore) * maxScore : 0f;
        return Mathf.Clamp(Mathf.RoundToInt(normalized), 0, maxScore);
    }

    public void ResetScore()
    {
        rawScore = 0;
        WorkEnergyUIManager.Instance?.UpdateScoreDisplay(0);
    }

    public int FinalizeScore()
    {
        int finalScore = GetScore();
        WorkEnergyUIManager.Instance?.UpdateScoreDisplay(finalScore);
        return finalScore;
    }
}
