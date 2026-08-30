using UnityEngine;

public class LeverScoreManager : MonoBehaviour
{
    public static LeverScoreManager Instance { get; private set; }

    [SerializeField] private int rawScore;
    // Perfect-play total of awards across start, equipment, setup, trials, and conclusion.
    [SerializeField] private int maxRawScore = 175;
    [SerializeField] private int maxScore = 100;

    public int RawScore => rawScore;
    public int MaxScore => maxScore;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddScore(int amount, bool showFeedback = true)
    {
        if (amount <= 0) return;
        rawScore += amount;
        LeverUIManager.Instance?.UpdateScoreDisplay(GetScore());
        if (showFeedback) LeverFeedbackManager.Instance?.ShowCorrect($"+{amount} Marks");
    }

    public void SubtractScore(int amount)
    {
        if (amount <= 0) return;
        rawScore = Mathf.Max(0, rawScore - amount);
        LeverUIManager.Instance?.UpdateScoreDisplay(GetScore());
        LeverFeedbackManager.Instance?.ShowIncorrect($"-{amount} Marks");
    }

    public int GetScore()
    {
        float normalized = maxRawScore > 0 ? (rawScore / (float)maxRawScore) * maxScore : 0f;
        return Mathf.Clamp(Mathf.RoundToInt(normalized), 0, maxScore);
    }

    public void ResetScore()
    {
        rawScore = 0;
        LeverUIManager.Instance?.UpdateScoreDisplay(0);
    }

    public int FinalizeScore()
    {
        int finalScore = GetScore();
        LeverUIManager.Instance?.UpdateScoreDisplay(finalScore);
        return finalScore;
    }
}
