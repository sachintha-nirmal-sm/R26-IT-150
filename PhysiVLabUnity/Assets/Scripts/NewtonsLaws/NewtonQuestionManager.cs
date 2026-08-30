using UnityEngine;

public class NewtonQuestionManager : MonoBehaviour
{
    public static NewtonQuestionManager Instance { get; private set; }

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
        NewtonUIManager.Instance?.ShowQuestionExplanation(q.explanation, correct);
    }

    public void AnswerNumeric(float value)
    {
        if (answered) return;
        var q = CurrentQuestion;
        if (q.kind != QuestionKind.Numeric) return;
        answered = true;
        bool correct = NewtonForceCalculator.Instance != null
            ? NewtonForceCalculator.Instance.ValidateStudentAnswer(value, q.numericAnswer, q.tolerance)
            : Mathf.Abs(value - q.numericAnswer) <= Mathf.Max(q.tolerance, 0.05f);
        ApplyResult(correct, q.explanation);
        NewtonUIManager.Instance?.ShowQuestionExplanation(q.explanation, correct);
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
            NewtonScoreManager.Instance?.AddScore(5, false);
            NewtonFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + explanation, "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + explanation, "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
        }
    }

    private void ShowCurrent()
    {
        var q = CurrentQuestion;
        if (q.kind == QuestionKind.Numeric)
            NewtonUIManager.Instance?.ShowNumericQuestion(index + 1, quiz.Length, q.prompt);
        else
            NewtonUIManager.Instance?.ShowQuestion(index + 1, quiz.Length, q.prompt, q.a, q.b, q.c, q.d);
    }

    private void BuildQuestions()
    {
        quiz = new[]
        {
            Mcq("A stationary body remains at rest until an unbalanced force acts on it. Which law explains this?",
                "Newton's First Law", "Newton's Second Law", "Newton's Third Law", "W = mg",
                0, "Newton's First Law: a stationary body remains at rest until an unbalanced force acts on it."),
            Mcq("A moving body continues with uniform velocity when the resultant force is zero. Which law explains this?",
                "Newton's Second Law", "Newton's First Law", "Newton's Third Law", "Hooke's Law",
                1, "Newton's First Law also describes uniform motion when the resultant force is zero."),
            Mcq("Which formula represents Newton's Second Law?",
                "F = ma", "W = mg", "v = d/t", "P = W/t",
                0, "Newton's Second Law is F = ma, so a = F/m."),
            Mcq("If the force acting on an object increases while mass remains constant, what happens to acceleration?",
                "Acceleration decreases.", "Acceleration stays the same.", "Acceleration increases.", "Mass increases.",
                2, "a = F/m. If F increases and m is constant, acceleration increases."),
            Mcq("If mass increases while force remains constant, what happens to acceleration?",
                "Acceleration increases.", "Acceleration decreases.", "Force becomes zero.", "Weight becomes zero.",
                1, "a = F/m. If m increases and F is constant, acceleration decreases."),
            Mcq("For every action there is an equal and opposite reaction. Which law is this?",
                "Newton's First Law", "Newton's Second Law", "Newton's Third Law", "Law of gravitation only",
                2, "Newton's Third Law: action and reaction are equal in magnitude and opposite in direction."),
            Mcq("The balloon moves forward when air moves backward. Which law explains this?",
                "Newton's First Law", "Newton's Second Law", "Newton's Third Law", "W = mg",
                2, "Air pushed backward is the action. The balloon moving forward is the reaction."),
            Mcq("What is the formula for weight?",
                "W = ma", "W = mg", "W = F/m", "W = v/t",
                1, "Weight is the gravitational force: W = mg."),
            Mcq("What is the value of gravitational acceleration used in this practical?",
                "10 m/s²", "9.8 m/s²", "8.9 m/s²", "0 m/s²",
                1, "This practical uses g = 9.8 m/s²."),
            Num("If the mass of an object is 2 kg, calculate its weight. W = mg = 2 × 9.8",
                19.6f, 0.05f, "W = 2 × 9.8 = 19.6 N. Values from 19.55 N to 19.65 N are accepted.")
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

    private static Question Num(string prompt, float answer, float tol, string explanation)
    {
        return new Question
        {
            kind = QuestionKind.Numeric,
            prompt = prompt, numericAnswer = answer, tolerance = tol, explanation = explanation
        };
    }
}
