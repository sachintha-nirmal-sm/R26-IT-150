using UnityEngine;

public class TurningQuestionManager : MonoBehaviour
{
    public static TurningQuestionManager Instance { get; private set; }

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
        TurningUIManager.Instance?.ShowQuestionExplanation(q.explanation, correct);
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
            TurningScoreManager.Instance?.AddScore(5, false);
            TurningFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + explanation, "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            TurningScoreManager.Instance?.SubtractScore(5);
            TurningFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + explanation, "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
    }

    private void ShowCurrent()
    {
        var q = CurrentQuestion;
        TurningUIManager.Instance?.ShowQuestion(index + 1, quiz.Length, q.prompt, q.a, q.b, q.c, q.d);
    }

    private void BuildQuestions()
    {
        quiz = new[]
        {
            Mcq("What is the turning effect of a force also called?",
                "Pressure", "Moment of a force", "Mass", "Temperature",
                1, "The turning effect of a force about a pivot is called the moment of the force."),
            Mcq("Which instrument measures the force in this practical?",
                "Ammeter", "Thermometer", "Newton balance", "Stopwatch",
                2, "A Newton balance measures force in newtons."),
            Mcq("Why must the Newton balance be pulled perpendicular to the stick?",
                "To make the stick heavier",
                "Because only the perpendicular component of the force produces a turning effect",
                "To measure time",
                "To heat the wood",
                1, "Moment = force × perpendicular distance. A force along the stick has no turning effect."),
            Mcq("Point O is the",
                "load", "pivot (axis of rotation)", "Newton balance", "table leg",
                1, "The screw nail through O is the pivot about which the stick turns."),
            Mcq("Points A, B, C and D are marked at",
                "5 cm intervals", "15 cm intervals", "1 m intervals", "random distances",
                1, "The holes are drilled 15 cm apart: A = 15 cm, B = 30 cm, C = 45 cm, D = 60 cm from O."),
            Mcq("If the force is 2.5 N and the perpendicular distance is 0.60 m, the moment is",
                "3.10 N m", "1.50 N m", "0.25 N m", "2.50 N m",
                1, "Moment = F × d = 2.5 N × 0.60 m = 1.50 N m."),
            Mcq("What happens when the screw nail is tightened?",
                "Friction at the pivot decreases", "The stick becomes longer", "Friction at the pivot increases, so a larger force is needed to turn the stick", "The Newton balance reads zero",
                2, "Tightening clamps the stick more firmly. More frictional torque must be overcome, so the minimum force increases."),
            Mcq("Why are two rubber washers used?",
                "To measure current", "To protect the stick and help the screw clamp it at O", "To increase the length of the stick", "To heat the nail",
                1, "One washer sits below and one above so the screw nail can clamp the stick without damaging it."),
            Mcq("Which quantity is the dependent variable in this experiment?",
                "The colour of the stick", "The minimum force needed to just turn the stick", "Room temperature", "The length of the table",
                1, "You change the tightness of the screw and then measure the force that just starts the stick turning."),
            Mcq("A student pulls at D with the balance almost parallel to the stick. The stick does not turn. Why?",
                "The force is too large", "The perpendicular component of the force is almost zero, so the moment is too small", "The table is too short", "Newton balances cannot measure force",
                1, "A force along the stick has almost no perpendicular component, so its turning effect is nearly zero.")
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
