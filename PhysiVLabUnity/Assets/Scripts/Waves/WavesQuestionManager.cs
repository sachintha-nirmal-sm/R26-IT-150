using UnityEngine;

public class WavesQuestionManager : MonoBehaviour
{
    public static WavesQuestionManager Instance { get; private set; }

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
        WavesUIManager.Instance?.ShowQuestionExplanation(q.explanation, correct);
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
            WavesScoreManager.Instance?.AddScore(5, false);
            WavesFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + explanation, "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            WavesScoreManager.Instance?.SubtractScore(5);
            WavesFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + explanation, "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
    }

    private void ShowCurrent()
    {
        var q = CurrentQuestion;
        WavesUIManager.Instance?.ShowQuestion(index + 1, quiz.Length, q.prompt, q.a, q.b, q.c, q.d);
    }

    private void BuildQuestions()
    {
        quiz = new[]
        {
            Mcq("What kind of wave is formed when one end of the slinky is shaken from side to side on the table?",
                "Longitudinal wave", "Transverse wave", "Sound wave in air only", "Electromagnetic wave",
                1, "Side-to-side shaking makes the coils move at right angles to the length of the slinky, so the wave is transverse."),
            Mcq("In this demonstration, what do the pieces of ribbon represent?",
                "The energy source", "Particles of the medium", "The direction of the wave", "The wavelength only",
                1, "Each ribbon is tied to a coil, so it shows how that part of the medium moves as the wave passes."),
            Mcq("The wave travels along the slinky. How do the ribbons move?",
                "Along the slinky with the wave", "Perpendicular to the direction of the wave", "They stay completely still", "In circles around the table legs",
                1, "In a transverse wave, particles vibrate at right angles to the direction of energy transfer."),
            Mcq("Why is the slinky placed flat on the table?",
                "To heat the coils", "So the pulse can travel along its length in a plane", "To measure its weight", "To make a sound wave",
                1, "The table supports the slinky so you can shake it from side to side and watch the pulse travel."),
            Mcq("If a student pushes and pulls the slinky along its length instead of shaking it sideways, the wave would be",
                "transverse", "longitudinal", "stationary only", "electromagnetic",
                1, "Push-pull along the slinky compresses and stretches the coils, which is a longitudinal wave — not this practical."),
            Mcq("Energy is transferred by the wave. What happens to a ribbon after the pulse has passed?",
                "It is carried to the far end of the table", "It returns to its original place, oscillating about rest", "It disappears", "It moves only in the wave direction",
                1, "The medium does not travel with the wave. Ribbons oscillate about their rest positions."),
            Mcq("Which apparatus is essential for this practical?",
                "Ammeter and voltmeter", "Slinky, pieces of ribbon and a table", "Bunsen burner and beaker", "Newton balance and mass hanger",
                1, "The slinky is the medium, ribbons mark particles, and the table keeps the slinky flat."),
            Mcq("The large arrow along the slinky in the diagram shows",
                "the motion of each ribbon", "the direction of the wave (energy travel)", "the weight of the slinky", "the temperature of the room",
                1, "Wave direction is along the slinky, away from the hand that shakes it.")
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
