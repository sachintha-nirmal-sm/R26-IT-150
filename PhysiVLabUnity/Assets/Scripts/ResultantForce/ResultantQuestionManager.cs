using UnityEngine;

public class ResultantQuestionManager : MonoBehaviour
{
    public static ResultantQuestionManager Instance { get; private set; }

    public enum QuestionKind { MultipleChoice, Numeric }

    public struct Question
    {
        public QuestionKind kind;
        public string prompt;
        public string a, b, c, d;
        public int correctIndex;
        public float numericAnswer;
        public float tolerance;
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
        if (q.kind != QuestionKind.MultipleChoice) return;
        answered = true;
        bool correct = choice == q.correctIndex;
        ApplyResult(correct, q.explanation);
        ResultantUIManager.Instance?.ShowQuestionExplanation(q.explanation, correct);
    }

    public void AnswerNumeric(float value)
    {
        if (answered) return;
        var q = CurrentQuestion;
        if (q.kind != QuestionKind.Numeric) return;
        answered = true;
        bool correct = Mathf.Abs(value - q.numericAnswer) <= Mathf.Max(q.tolerance, 0.05f);
        ApplyResult(correct, q.explanation);
        ResultantUIManager.Instance?.ShowQuestionExplanation(q.explanation, correct);
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
            ResultantScoreManager.Instance?.AddScore(5, false);
            ResultantFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + explanation, "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            ResultantScoreManager.Instance?.SubtractScore(5);
            ResultantFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + explanation, "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
    }

    private void ShowCurrent()
    {
        var q = CurrentQuestion;
        if (q.kind == QuestionKind.Numeric)
            ResultantUIManager.Instance?.ShowNumericQuestion(index + 1, quiz.Length, q.prompt);
        else
            ResultantUIManager.Instance?.ShowQuestion(index + 1, quiz.Length, q.prompt, q.a, q.b, q.c, q.d);
    }

    private void BuildQuestions()
    {
        quiz = new[]
        {
            Mcq("What is a resultant force?",
                "Any force that is larger than 10 N",
                "The single force that has the same effect as two or more forces acting together",
                "The force of gravity only",
                "The force measured by a thermometer",
                1, "The resultant is one force that replaces two or more forces and produces the same effect."),
            Mcq("Which instrument is used to measure force in this practical?",
                "Ammeter", "Thermometer", "Newton balance", "Stopwatch",
                2, "A Newton balance measures force in newtons."),
            Mcq("Why are two pulleys used?",
                "To heat the strings",
                "To change the direction of the strings so that forces B and C pull the trolley the same way",
                "To measure time",
                "To make the trolley heavier",
                1, "Each string passes over a pulley so both hanging balances pull the trolley in the same direction."),
            Mcq("Newton balance A is attached between the wall and the trolley. What does it measure?",
                "Temperature", "The resultant force that balances B and C", "Electric current", "The mass of the table",
                1, "In equilibrium, A equals the resultant of B and C acting in the same direction."),
            Mcq("If Force B = 5 N and Force C = 3 N, and they act in the same direction, what is Force A?",
                "2 N", "8 N", "15 N", "0 N",
                1, "A = B + C = 5 N + 3 N = 8 N."),
            Mcq("The ring is fixed to which part of the trolley?",
                "The wheels", "The front of the trolley", "The wall", "The ceiling",
                1, "The ring is fixed to the front of the trolley so two strings can be tied to it."),
            Mcq("When two forces act in the same direction, the resultant is",
                "their difference", "their product", "their sum", "always zero",
                2, "Forces in the same direction add: resultant = B + C."),
            Mcq("Which two forces act on the trolley in the same direction?",
                "A and B only", "B and C", "A and gravity only", "The table and the wall",
                1, "Balances B and C, through the pulleys, pull the trolley in the same direction."),
            Mcq("Which quantity is the dependent variable in this experiment?",
                "The colour of the trolley", "The resultant force on balance A", "The length of the table", "Room temperature",
                1, "You change B and C and then measure the resultant A."),
            Mcq("A student records B = 4.0 N and C = 6.0 N. Balance A shows 10.0 N. What can be concluded?",
                "The experiment failed", "A is not related to B and C", "A = B + C, so the two forces add when they act in the same direction", "A is always 10 N",
                2, "10.0 N = 4.0 N + 6.0 N. The resultant equals the sum of the two forces.")
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
