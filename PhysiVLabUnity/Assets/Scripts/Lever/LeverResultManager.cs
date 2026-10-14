using UnityEngine;

/// <summary>
/// Thin wrapper that shows the final result panel via LeverUIManager / profile summary.
/// </summary>
public class LeverResultManager : MonoBehaviour
{
    public static LeverResultManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void ShowFinalResult(int score, bool passed, int mistakes, LeverAttemptRecord attempt)
    {
        LeverUIManager.Instance?.ShowResult(score, passed, mistakes, attempt);
    }

    public void ShowProfile()
    {
        string summary = LeverProfileManager.Instance?.GetProfileSummary() ?? "No profile data.";
        LeverFeedbackManager.Instance?.ShowInstruction(summary);
    }

    public void HideResult()
    {
        LeverUIManager.Instance?.HideResult();
    }
}
