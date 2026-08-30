using UnityEngine;

public class ElecScoreManager : MonoBehaviour
{
    public static ElecScoreManager Instance { get; private set; }

    [SerializeField] private int rawScore;
    [SerializeField] private int maxRawScore = 250;
    [SerializeField] private int maxScore = 100;
    [SerializeField] private int correctScore = 5;
    [SerializeField] private int wrongPenalty = 5;
    [SerializeField] private int connectionScore = 10;

    public int RawScore => rawScore;
    public int MaxScore => maxScore;
    public int CorrectScore => correctScore;
    public int WrongPenalty => wrongPenalty;
    public int ConnectionScore => connectionScore;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ConfigureMaxRaw(int requiredEquipmentCount)
    {
        maxRawScore =
            correctScore * requiredEquipmentCount +
            5 +
            3 * (connectionScore + correctScore + correctScore + correctScore) +
            4 * 10 +
            5 +
            6 * 10;
        if (maxRawScore < 1) maxRawScore = 1;
    }

    public void AddScore(int amount, bool showFeedback = true)
    {
        if (amount <= 0) return;
        rawScore += amount;
        ElecUIManager.Instance?.UpdateScoreDisplay(GetScore());
        if (showFeedback) ElecFeedbackManager.Instance?.ShowCorrect($"+{amount} Marks");
    }

    public void SubtractScore(int amount)
    {
        if (amount <= 0) return;
        rawScore = Mathf.Max(0, rawScore - amount);
        ElecUIManager.Instance?.UpdateScoreDisplay(GetScore());
        ElecFeedbackManager.Instance?.ShowIncorrect($"-{amount} Marks");
        CurrentElectricityExperimentManager.Instance?.RegisterMistake();
    }

    public int GetScore()
    {
        float normalized = maxRawScore > 0 ? (rawScore / (float)maxRawScore) * maxScore : 0f;
        return Mathf.Clamp(Mathf.RoundToInt(normalized), 0, maxScore);
    }

    public void ResetScore()
    {
        rawScore = 0;
        ElecUIManager.Instance?.UpdateScoreDisplay(0);
    }

    public int FinalizeScore()
    {
        int finalScore = GetScore();
        ElecUIManager.Instance?.UpdateScoreDisplay(finalScore);
        return finalScore;
    }
}
