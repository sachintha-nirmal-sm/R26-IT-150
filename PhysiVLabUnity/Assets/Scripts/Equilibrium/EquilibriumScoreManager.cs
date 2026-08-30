using UnityEngine;

public class EquilibriumScoreManager : MonoBehaviour
{
    public static EquilibriumScoreManager Instance { get; private set; }

    [SerializeField] private int rawScore;
    [SerializeField] private int maxRawScore = 200;
    [SerializeField] private int maxScore = 100;
    [SerializeField] private int correctScore = 5;
    [SerializeField] private int wrongPenalty = 5;
    [SerializeField] private int majorStepScore = 10;
    [SerializeField] private int equipmentScore = 4;

    public int RawScore => rawScore;
    public int MaxScore => maxScore;
    public int CorrectScore => correctScore;
    public int WrongPenalty => wrongPenalty;
    public int MajorStepScore => majorStepScore;
    public int EquipmentScore => equipmentScore;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Configure(int correct, int wrong, int major)
    {
        correctScore = Mathf.Max(1, correct);
        wrongPenalty = Mathf.Max(1, wrong);
        majorStepScore = Mathf.Max(1, major);
        ConfigureMaxRaw();
    }

    public void ConfigureMaxRaw(int requiredEquipmentCount = 4)
    {
        maxRawScore =
            equipmentScore * requiredEquipmentCount +
            correctScore * 6 +
            correctScore +
            correctScore +
            correctScore * 3 +
            correctScore +
            correctScore +
            correctScore +
            correctScore * 10 +
            correctScore * 6 +
            majorStepScore;
        if (maxRawScore < 1) maxRawScore = 1;
    }

    public void AddScore(int amount, bool showFeedback = true)
    {
        if (amount <= 0) return;
        rawScore += amount;
        EquilibriumUIManager.Instance?.UpdateScoreDisplay(GetScore());
        if (showFeedback) EquilibriumFeedbackManager.Instance?.ShowCorrect($"+{amount} MARKS");
    }

    public void SubtractScore(int amount)
    {
        if (amount <= 0) return;
        rawScore = Mathf.Max(0, rawScore - amount);
        EquilibriumUIManager.Instance?.UpdateScoreDisplay(GetScore());
        EquilibriumFeedbackManager.Instance?.ShowIncorrect($"-{amount} MARKS");
        EquilibriumExperimentManager.Instance?.RegisterMistake();
    }

    public int GetScore()
    {
        float normalized = maxRawScore > 0 ? (rawScore / (float)maxRawScore) * maxScore : 0f;
        return Mathf.Clamp(Mathf.RoundToInt(normalized), 0, maxScore);
    }

    public void ResetScore()
    {
        rawScore = 0;
        EquilibriumUIManager.Instance?.UpdateScoreDisplay(0);
    }

    public int FinalizeScore()
    {
        int finalScore = GetScore();
        EquilibriumUIManager.Instance?.UpdateScoreDisplay(finalScore);
        return finalScore;
    }
}
