using TMPro;
using UnityEngine;

public class PowerEnergyComparisonManager : MonoBehaviour
{
    public static PowerEnergyComparisonManager Instance { get; private set; }

    private int questionIndex;
    private bool answered;
    private int correctCount;
    private TextMeshProUGUI promptText;

    private readonly string[] prompts =
    {
        "Which appliance has the highest power?",
        "Which appliance consumes the most energy during the same operating time?",
        "Which appliance consumes the least energy during the same operating time?"
    };

    private readonly int[] answers = { 3, 3, 0 };
    private readonly string[] explanations =
    {
        "The electric kettle has the highest power (about 2000 W).",
        "For the SAME time, higher power means more electrical energy: E = Pt. The kettle consumes the most.",
        "The electric bulb has the lowest power, so it consumes the least energy in the same time."
    };

    public bool IsFinished => questionIndex >= prompts.Length;
    public bool HasAnswered => answered;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(TextMeshProUGUI prompt)
    {
        promptText = prompt;
    }

    public void StartComparison()
    {
        questionIndex = 0;
        answered = false;
        correctCount = 0;
        ShowCurrent();
    }

    public void Answer(int choice)
    {
        if (answered || IsFinished) return;
        answered = true;
        bool ok = choice == answers[questionIndex];
        if (ok)
        {
            correctCount++;
            PowerEnergyScoreManager.Instance?.AddToCategory(PowerEnergyScoreCategory.Observation, 5, false);
            PowerEnergyFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + explanations[questionIndex], "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + explanations[questionIndex], "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
        PowerEnergyUIManager.Instance?.ShowQuestionExplanation(explanations[questionIndex], ok);
    }

    public bool Advance()
    {
        if (!answered) return false;
        questionIndex++;
        answered = false;
        if (questionIndex >= prompts.Length) return true;
        ShowCurrent();
        return false;
    }

    private void ShowCurrent()
    {
        string extra =
            "COMPARE THE APPLIANCES\n\n" +
            "For the SAME time: higher power → more electrical energy consumed.\n\n";
        if (promptText != null)
            promptText.text = extra + $"QUESTION {questionIndex + 1} / {prompts.Length}\n\n{prompts[questionIndex]}";
        PowerEnergyUIManager.Instance?.ShowQuestion(
            questionIndex + 1,
            prompts.Length,
            prompts[questionIndex],
            "Electric bulb",
            "Electric fan",
            "Electric iron",
            "Electric kettle");
    }
}
