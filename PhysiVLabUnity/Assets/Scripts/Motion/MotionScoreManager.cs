using UnityEngine;

public class MotionScoreManager : MonoBehaviour
{
    public static MotionScoreManager Instance { get; private set; }

    [SerializeField] private int rawScore;
    [SerializeField] private int maxRawScore = 205;
    [SerializeField] private int maxScore = 100;
    [SerializeField] private int correctScore = 5;
    [SerializeField] private int wrongPenalty = 5;
    [SerializeField] private int majorStepScore = 10;

    public int RawScore => rawScore;
    public int MaxScore => maxScore;
    public int CorrectScore => correctScore;
    public int WrongPenalty => wrongPenalty;
    public int MajorStepScore => majorStepScore;

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

    public void ConfigureMaxRaw(int requiredEquipmentCount = 7)
    {
        maxRawScore =
            correctScore * requiredEquipmentCount +
            correctScore * 5 +
            3 * 5 +
            correctScore * 5 +
            correctScore * 4 +
            majorStepScore +
            correctScore * 4 +
            correctScore * 10 +
            correctScore;
        if (maxRawScore < 1) maxRawScore = 1;
    }

    public void AddScore(int amount, bool showFeedback = true)
    {
        if (amount <= 0) return;
        rawScore += amount;
        MotionUIManager.Instance?.UpdateScoreDisplay(GetScore());
        if (showFeedback) MotionFeedbackManager.Instance?.ShowCorrect($"+{amount} Marks");
    }

    public void SubtractScore(int amount)
    {
        if (amount <= 0) return;
        rawScore = Mathf.Max(0, rawScore - amount);
        MotionUIManager.Instance?.UpdateScoreDisplay(GetScore());
        MotionFeedbackManager.Instance?.ShowIncorrect($"-{amount} Marks");
        MotionExperimentManager.Instance?.RegisterMistake();
    }

    public int GetScore()
    {
        float normalized = maxRawScore > 0 ? (rawScore / (float)maxRawScore) * maxScore : 0f;
        return Mathf.Clamp(Mathf.RoundToInt(normalized), 0, maxScore);
    }

    public void ResetScore()
    {
        rawScore = 0;
        MotionUIManager.Instance?.UpdateScoreDisplay(0);
    }

    public int FinalizeScore()
    {
        int finalScore = GetScore();
        MotionUIManager.Instance?.UpdateScoreDisplay(finalScore);
        return finalScore;
    }
}
