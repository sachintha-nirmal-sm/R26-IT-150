using UnityEngine;

public class FrictionQuestionManager : MonoBehaviour
{
    public static FrictionQuestionManager Instance { get; private set; }

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
        FrictionUIManager.Instance?.ShowQuestionExplanation(q.explanation, correct);
    }

    public void AnswerNumeric(float value)
    {
        if (answered) return;
        var q = CurrentQuestion;
        if (q.kind != QuestionKind.Numeric) return;
        answered = true;
        bool correct = Mathf.Abs(value - q.numericAnswer) <= Mathf.Max(q.tolerance, 0.05f);
        ApplyResult(correct, q.explanation);
        FrictionUIManager.Instance?.ShowQuestionExplanation(q.explanation, correct);
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
            FrictionScoreManager.Instance?.AddScore(5, false);
            FrictionFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + explanation, "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            FrictionScoreManager.Instance?.SubtractScore(5);
            FrictionFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + explanation, "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
    }

    private void ShowCurrent()
    {
        var q = CurrentQuestion;
        if (q.kind == QuestionKind.Numeric)
            FrictionUIManager.Instance?.ShowNumericQuestion(index + 1, quiz.Length, q.prompt);
        else
            FrictionUIManager.Instance?.ShowQuestion(index + 1, quiz.Length, q.prompt, q.a, q.b, q.c, q.d);
    }

    private void BuildQuestions()
    {
        quiz = new[]
        {
            Mcq("What force opposes the motion of the wooden block?",
                "Gravitational force", "Frictional force", "Magnetic force", "Tension in a string",
                1, "Friction is the force that opposes the relative motion or tendency of motion between surfaces in contact."),
            Mcq("What instrument is used to measure the pulling force?",
                "Ammeter", "Thermometer", "Newton balance", "Stopwatch",
                2, "A Newton balance measures force in newtons."),
            Mcq("What is the weight of the wooden block?",
                "10 N", "30 N", "60 N", "100 N",
                2, "The wooden block has a constant weight of 60 N for every orientation."),
            Mcq("What is changed in this experiment?",
                "Weight of the block", "Area of contact", "Sandpaper roughness", "Material of the block",
                1, "Only the contact area is changed by rotating the same wooden block."),
            Mcq("What must remain constant?",
                "The time of pulling", "The colour of the sandpaper", "Weight of the block and surface roughness", "The length of the table",
                2, "Weight and sandpaper roughness are controlled variables."),
            Mcq("What happens to limiting friction when the surface area is changed?",
                "It increases greatly", "It decreases greatly", "It remains approximately the same", "It becomes zero",
                2, "Limiting friction does not depend significantly on contact area when weight and roughness are constant."),
            Mcq("What is the force called just before the block starts moving?",
                "Weight", "Limiting friction", "Air resistance", "Normal reaction only",
                1, "Limiting friction is the maximum frictional force just before motion begins."),
            Mcq("If the block has the same weight and the same surface roughness, does placing it on a larger surface produce much greater limiting friction?",
                "Yes", "No", "Only if the area doubles", "Only if the area is 600 cm²",
                1, "No. A larger contact area does not produce much greater limiting friction under these conditions."),
            Mcq("Why are the sandpapers required to have equal roughness?",
                "To make the block heavier", "To keep surface roughness constant so that only contact area is investigated", "To change the weight", "To reduce the Newton-balance reading to zero",
                1, "Equal roughness keeps the surface condition constant so contact area is the independent variable."),
            Mcq("Which variable is the independent variable?",
                "Limiting frictional force", "Contact area", "Weight", "Time",
                1, "Contact area is deliberately changed; it is the independent variable."),
            Mcq("Which quantity is measured as the dependent variable?",
                "Contact area", "Sandpaper colour", "Limiting frictional force", "Length of the table",
                2, "Limiting frictional force is measured for each contact area."),
            Mcq("Why is the same wooden block used for all trials?",
                "To change the weight each time", "To keep the weight and material conditions constant", "To make the area the same", "To avoid using sandpaper",
                1, "The same block keeps weight and material constant while only orientation (area) changes."),
            Mcq("Surface A has a limiting friction of 18.0 N. Surface B has 18.2 N. Surface C has 17.8 N. What can be concluded?",
                "Friction increases greatly with area", "Friction decreases greatly with area", "The values are approximately the same, so limiting friction does not significantly depend on contact area", "The experiment failed",
                2, "The small differences are experimental variation. Limiting friction does not significantly depend on contact area.")
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
