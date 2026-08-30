using UnityEngine;

public class ElectronicsComparisonManager : MonoBehaviour
{
    public static ElectronicsComparisonManager Instance { get; private set; }

    private int index;
    private bool answered;
    private int correctCount;
    private bool finished;

    private readonly string[] prompts =
    {
        "In forward bias, the diode direction is:",
        "In reverse bias, current is:",
        "In forward bias, the bulb:",
        "In reverse bias, the bulb:"
    };

    private readonly string[] optionA =
    {
        "Correct — the diode allows current to flow",
        "Flowing through the circuit",
        "Glows",
        "Glows brightly"
    };
    private readonly string[] optionB =
    {
        "Reversed — the diode blocks current",
        "Blocked by the diode",
        "Does not glow",
        "Does not glow"
    };
    private readonly string[] optionC =
    {
        "The diode is removed from the circuit",
        "Stored inside the battery",
        "Explodes",
        "Measures the voltage"
    };
    private readonly string[] optionD =
    {
        "The switch stays permanently OFF",
        "Doubled because there are two cells",
        "Becomes a second diode",
        "Reverses the battery by itself"
    };

    private readonly int[] answers = { 0, 1, 0, 1 };
    private readonly string[] explanations =
    {
        "Forward bias uses the correct diode direction so current can flow.",
        "In reverse bias the diode blocks current.",
        "The bulb glows in forward bias.",
        "The bulb does not glow in reverse bias."
    };
    private readonly int[] highlightRows = { 0, 1, 2, 2 };

    public bool IsFinished => finished;
    public bool HasAnswered => answered;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartComparison()
    {
        index = 0;
        answered = false;
        correctCount = 0;
        finished = false;
        ShowCurrent();
    }

    public void Answer(int choice)
    {
        if (answered || finished) return;
        answered = true;
        bool ok = choice == answers[index];
        if (ok)
        {
            correctCount++;
            ElectronicsScoreManager.Instance?.AddToCategory(ElectronicsScoreCategory.Comparison, 3, false);
            ElectronicsFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + explanations[index], "+10 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            ElectronicsScoreManager.Instance?.SubtractScore(5);
            ElectronicsFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + explanations[index], "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
        ElectronicsUIManager.Instance?.ShowQuestionExplanation(explanations[index], ok);
        if (index == prompts.Length - 1 && ok)
            ElectronicsScoreManager.Instance?.AddToCategory(ElectronicsScoreCategory.Comparison, 4, false);
    }

    public bool Advance()
    {
        if (!answered) return false;
        index++;
        answered = false;
        if (index >= prompts.Length)
        {
            finished = true;
            return true;
        }
        ShowCurrent();
        return false;
    }

    private void ShowCurrent()
    {
        ElectronicsUIManager.Instance?.HighlightCompareRow(highlightRows[index]);
        ElectronicsUIManager.Instance?.SetQuestionPanelWide(false);
        ElectronicsUIManager.Instance?.ShowQuestion(
            index + 1,
            prompts.Length,
            prompts[index],
            optionA[index],
            optionB[index],
            optionC[index],
            optionD[index]);
        ElectronicsUIManager.Instance?.SetQuestionOptionsVisible(4);
    }
}
