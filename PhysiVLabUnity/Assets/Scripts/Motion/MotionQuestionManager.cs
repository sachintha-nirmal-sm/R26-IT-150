using UnityEngine;

public class MotionQuestionManager : MonoBehaviour
{
    public static MotionQuestionManager Instance { get; private set; }

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
        MotionUIManager.Instance?.ShowQuestionExplanation(q.explanation, correct);
    }

    public void AnswerNumeric(float value)
    {
        if (answered) return;
        var q = CurrentQuestion;
        if (q.kind != QuestionKind.Numeric) return;
        answered = true;
        bool correct = Mathf.Abs(value - q.numericAnswer) <= Mathf.Max(q.tolerance, 0.05f);
        ApplyResult(correct, q.explanation);
        MotionUIManager.Instance?.ShowQuestionExplanation(q.explanation, correct);
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
            MotionScoreManager.Instance?.AddScore(5, false);
            MotionFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + explanation, "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            MotionScoreManager.Instance?.SubtractScore(5);
            MotionFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + explanation, "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
        }
    }

    private void ShowCurrent()
    {
        var q = CurrentQuestion;
        if (q.kind == QuestionKind.Numeric)
            MotionUIManager.Instance?.ShowNumericQuestion(index + 1, quiz.Length, q.prompt);
        else
            MotionUIManager.Instance?.ShowQuestion(index + 1, quiz.Length, q.prompt, q.a, q.b, q.c, q.d);
    }

    private void BuildQuestions()
    {
        quiz = new[]
        {
            Mcq("What is distance?",
                "Straight-line change from initial to final position",
                "Total path length travelled",
                "Rate of change of velocity",
                "Force",
                1, "Distance is the total path length travelled by an object."),
            Mcq("What type of quantity is displacement?",
                "Scalar", "Vector", "Unitless", "Constant",
                1, "Displacement is a vector. It has magnitude and direction."),
            Mcq("Which formula is used to calculate speed?",
                "speed = time / distance",
                "speed = distance / time",
                "speed = distance × time",
                "speed = displacement × time",
                1, "Speed = distance / time."),
            Mcq("Which formula is used to calculate velocity?",
                "velocity = displacement / time",
                "velocity = distance / time",
                "velocity = time / displacement",
                "velocity = force / mass",
                0, "Velocity = displacement / time."),
            Mcq("What is acceleration?",
                "Rate of change of distance",
                "Rate of change of displacement",
                "Rate of change of velocity",
                "Total distance travelled",
                2, "Acceleration is the rate of change of velocity."),
            Mcq("If velocity decreases with time, what is this called?",
                "Speed", "Acceleration", "Deceleration", "Distance",
                2, "A decrease in velocity with time is deceleration (negative acceleration)."),
            Num("A car travels 20 m in 5 s. What is its average speed in m/s?\nSpeed = distance / time",
                4f, 0.05f, "speed = 20 / 5 = 4 m/s"),
            Num("A car moves from 2 m to 7 m. What is its displacement in metres?",
                5f, 0.05f, "Displacement = 7 − 2 = +5 m, in the positive direction."),
            Num("A car moves from 5 m to 2 m. What is its displacement in metres? (include the sign)",
                -3f, 0.05f, "Displacement = 2 − 5 = −3 m. Direction: ←"),
            Num("If velocity changes from 2 m/s to 6 m/s in 2 s, what is the acceleration in m/s²?\na = (v − u) / t",
                2f, 0.05f, "a = (6 − 2) / 2 = 2 m/s²")
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

    private static Question Num(string prompt, float answer, float tolerance, string explanation)
    {
        return new Question
        {
            kind = QuestionKind.Numeric,
            prompt = prompt,
            numericAnswer = answer,
            tolerance = tolerance,
            explanation = explanation
        };
    }
}
