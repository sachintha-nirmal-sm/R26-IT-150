using UnityEngine;

public class NewtonScoreManager : MonoBehaviour
{
    public static NewtonScoreManager Instance { get; private set; }

    [SerializeField] private int rawScore;
    [SerializeField] private int maxRawScore = 280;
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

    public void ConfigureMaxRaw(int requiredEquipmentCount = 12)
    {
        maxRawScore =
            equipmentScore * requiredEquipmentCount +
            correctScore * 5 +
            majorStepScore +
            correctScore * 8 +
            correctScore * 6 +
            correctScore * 2 +
            correctScore * 3 +
            correctScore +
            correctScore * 10 +
            correctScore * 4 +
            correctScore;
        if (maxRawScore < 1) maxRawScore = 1;
    }

    public void AddScore(int amount, bool showFeedback = true)
    {
        if (amount <= 0) return;
        rawScore += amount;
        NewtonUIManager.Instance?.UpdateScoreDisplay(GetScore());
        if (showFeedback) NewtonFeedbackManager.Instance?.ShowCorrect($"+{amount} Marks");
    }

    public void SubtractScore(int amount)
    {
        if (amount <= 0) return;
        rawScore = Mathf.Max(0, rawScore - amount);
        NewtonUIManager.Instance?.UpdateScoreDisplay(GetScore());
        NewtonFeedbackManager.Instance?.ShowIncorrect($"-{amount} Marks");
        NewtonsLawsExperimentManager.Instance?.RegisterMistake();
    }

    public int GetScore()
    {
        float normalized = maxRawScore > 0 ? (rawScore / (float)maxRawScore) * maxScore : 0f;
        return Mathf.Clamp(Mathf.RoundToInt(normalized), 0, maxScore);
    }

    public void ResetScore()
    {
        rawScore = 0;
        NewtonUIManager.Instance?.UpdateScoreDisplay(0);
    }

    public int FinalizeScore()
    {
        int finalScore = GetScore();
        NewtonUIManager.Instance?.UpdateScoreDisplay(finalScore);
        return finalScore;
    }
}
