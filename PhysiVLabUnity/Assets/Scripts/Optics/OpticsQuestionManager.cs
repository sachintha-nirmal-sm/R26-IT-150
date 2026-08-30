using UnityEngine;

public class OpticsQuestionManager : MonoBehaviour
{
    public static OpticsQuestionManager Instance { get; private set; }

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
        OpticsUIManager.Instance?.ShowQuestionExplanation(q.explanation, correct);
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
            OpticsScoreManager.Instance?.AddScore(5, false);
            OpticsFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + explanation, "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            OpticsScoreManager.Instance?.SubtractScore(5);
            OpticsFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + explanation, "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
    }

    private void ShowCurrent()
    {
        var q = CurrentQuestion;
        OpticsUIManager.Instance?.ShowQuestion(index + 1, quiz.Length, q.prompt, q.a, q.b, q.c, q.d);
    }

    private void BuildQuestions()
    {
        quiz = new[]
        {
            Mcq("Why can rays from a far-away outdoor scene be treated as parallel?",
                "Because they travel through glass",
                "Because a distant object is effectively at infinity",
                "Because the screen is white",
                "Because the room is dark",
                1, "A very distant object is treated as being at infinity, so the rays that reach the mirror are nearly parallel."),
            Mcq("Where do parallel rays meet after reflection from a concave mirror?",
                "At the centre of curvature C",
                "At the pole P",
                "At the principal focus F",
                "Behind the mirror",
                2, "A concave mirror converges parallel incident rays to its principal focus F."),
            Mcq("The image of the distant scene is formed on the white screen. What kind of image is this?",
                "Virtual and erect",
                "Real and inverted",
                "Virtual and inverted",
                "Real and erect",
                1, "An image that can be caught on a screen is real. The distant scene appears upside down, so it is inverted."),
            Mcq("When the image on the screen is very clear, the mirror–screen distance is approximately",
                "the radius of curvature",
                "twice the wavelength",
                "the focal length of the mirror",
                "the height of the window",
                2, "Parallel rays meet at F, so the sharp-image distance is approximately the focal length f."),
            Mcq("Which apparatus is essential for this practical?",
                "Convex mirror and thermometer",
                "Concave mirror, white screen and meter ruler",
                "Glass prism and glass slab",
                "Newton balance and mass hanger",
                1, "The concave mirror forms the image, the screen shows it, and the ruler measures f."),
            Mcq("Why is a convex mirror unsuitable here?",
                "It is too heavy",
                "It forms only a virtual, diminished image and cannot throw a real image on a screen",
                "It has no reflecting surface",
                "It absorbs all the light",
                1, "A convex mirror diverges rays. The image is virtual, so it cannot be obtained on a screen."),
            Mcq("You open the window because",
                "you need a nearby lamp as the object",
                "you need a distant object so that incident rays are parallel",
                "you need to cool the mirror",
                "you need to measure wind speed",
                1, "The outdoor scene is far away, so its rays arriving at the mirror are treated as parallel."),
            Mcq("If the screen is moved far from the focus, the image on the screen becomes",
                "sharper and larger",
                "blurred (out of focus)",
                "virtual",
                "coloured like a spectrum",
                1, "Only near F do the reflected rays meet on the screen. Away from F the patch of light is blurred.")
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
