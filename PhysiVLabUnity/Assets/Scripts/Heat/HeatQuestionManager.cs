using UnityEngine;

public class HeatQuestionManager : MonoBehaviour
{
    public static HeatQuestionManager Instance { get; private set; }

    public enum QuestionKind { MultipleChoice }

    public struct Question
    {
        public QuestionKind kind;
        public string prompt;
        public string a, b, c, d;
        public int correctIndex;
        public string explanation;
    }

    private Question[] quiz;
    private int index;
    private bool answered;

    private void Awake()
    {
        Instance = this;
        BuildQuestions();
    }

    public int QuizCount => quiz.Length;
    public bool HasAnswered => answered;
    public bool IsFinished => quiz != null && index >= quiz.Length;
    public Question CurrentQuestion => quiz[Mathf.Clamp(index, 0, quiz.Length - 1)];

    public void StartQuiz()
    {
        index = 0;
        answered = false;
        ShowCurrent();
    }

    public void Answer(int choice)
    {
        if (answered) return;
        var q = CurrentQuestion;
        answered = true;
        bool correct = choice == q.correctIndex;
        ApplyResult(correct, q.explanation);
        HeatUIManager.Instance?.ShowQuestionExplanation(q.explanation, correct);
    }

    public bool Advance()
    {
        if (!answered) return false;
        index++;
        answered = false;
        if (index >= quiz.Length) return true;
        ShowCurrent();
        return false;
    }

    private void ApplyResult(bool correct, string explanation)
    {
        if (correct)
        {
            HeatScoreManager.Instance?.AddScore(5, false);
            HeatFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + explanation, "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            HeatScoreManager.Instance?.SubtractScore(5);
            HeatFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + explanation, "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
    }

    private void ShowCurrent()
    {
        var q = CurrentQuestion;
        HeatUIManager.Instance?.ShowQuestion(index + 1, quiz.Length, q.prompt, q.a, q.b, q.c, q.d);
    }

    private void BuildQuestions()
    {
        quiz = new[]
        {
            Mcq("Why is a thin glass tube used instead of watching the test tube itself?",
                "The thin tube cools the liquid",
                "A small volume change makes a large, easy-to-see height change in a narrow tube",
                "The thin tube prevents expansion",
                "Glass does not expand in a narrow tube",
                1, "The same small increase in volume produces a much larger rise in a narrow tube, so expansion is easy to see."),
            Mcq("Just after heating starts, the liquid level falls from A to B. Why?",
                "The liquid leaks through the stopper",
                "The liquid contracts when heated",
                "The glass container expands first, so its volume increases before the liquid has heated",
                "The burner sucks the liquid down",
                2, "Heat reaches the glass first. The test tube expands, its internal volume increases, and the liquid level drops slightly to B."),
            Mcq("The liquid then rises from B, past A, to C. This shows that",
                "solids expand more than liquids",
                "the liquid expands more than the glass",
                "the liquid has evaporated",
                "the stopper has come out",
                1, "Liquids generally expand more than solids for the same temperature rise, so the level ends higher at C."),
            Mcq("Level A on the thin tube is",
                "the highest level after heating",
                "the lowest level during heating",
                "the starting liquid level before heating",
                "the boiling point of water",
                2, "A is marked before the test tube is heated. It is the original height of the coloured liquid."),
            Mcq("Which statement is true?",
                "Only the liquid expands; glass never expands",
                "Only the glass expands; liquids never expand",
                "Both the glass and the liquid expand when heated, but the liquid expands more",
                "Neither the glass nor the liquid expands",
                2, "Both expand. The brief fall to B shows the glass expanding first; the rise to C shows the greater expansion of the liquid."),
            Mcq("The beaker of water is used to",
                "cool the test tube quickly",
                "heat the test tube evenly as a water bath",
                "measure the volume of coloured water",
                "hold the Bunsen burner",
                1, "A water bath heats the test tube more evenly than a bare flame would."),
            Mcq("If the stopper were left out, the liquid would",
                "still rise clearly in a thin tube",
                "not be forced up a narrow tube, so the expansion would be hard to see",
                "expand more than with a stopper",
                "freeze",
                1, "The stopper and thin tube together make the small volume change visible as a large height change."),
            Mcq("Which apparatus is essential for this practical?",
                "Newton balance and wooden block",
                "Test tube, coloured water, stopper, thin tube, beaker, burner, tripod and clamp stand",
                "Concave mirror and white screen",
                "Ammeter and voltmeter",
                1, "Those items match Figure 9.22: the tube apparatus plus a heated water bath.")
        };
    }

    private static Question Mcq(string prompt, string a, string b, string c, string d, int correct, string explanation)
    {
        return new Question
        {
            kind = QuestionKind.MultipleChoice,
            prompt = prompt, a = a, b = b, c = c, d = d,
            correctIndex = correct, explanation = explanation
        };
    }
}
