using UnityEngine;

public class ElectronicsQuestionManager : MonoBehaviour
{
    public static ElectronicsQuestionManager Instance { get; private set; }

    public struct Question
    {
        public string prompt;
        public string a, b, c, d;
        public int correctIndex;
        public string explanation;
        public bool diodeDiagram;
        public bool anodeQuestion;
    }

    private Question[] quiz;
    private int index;
    private bool answered;

    public int QuizCount => quiz != null ? quiz.Length : 0;
    public bool HasAnswered => answered;
    public bool IsFinished => quiz != null && index >= quiz.Length;
    public Question CurrentQuestion => quiz[Mathf.Clamp(index, 0, quiz.Length - 1)];

    private void Awake()
    {
        Instance = this;
        quiz = MainQuestions();
    }

    public void StartQuiz()
    {
        index = 0;
        answered = false;
        quiz = MainQuestions();
        ShowCurrent();
    }

    public void Answer(int choice)
    {
        if (answered) return;
        var q = CurrentQuestion;
        answered = true;
        bool correct = choice == q.correctIndex;
        if (correct)
        {
            ElectronicsScoreManager.Instance?.AddToCategory(ElectronicsScoreCategory.Questions, 2, false);
            ElectronicsFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + q.explanation, "+2 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            ElectronicsScoreManager.Instance?.SubtractScore(1);
            ElectronicsFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + q.explanation, "-1 MARK", new Color(0.75f, 0.12f, 0.12f));
        }
        ElectronicsUIManager.Instance?.ShowQuestionExplanation(q.explanation, correct);
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

    private void ShowCurrent()
    {
        var q = CurrentQuestion;
        ElectronicsUIManager.Instance?.ShowQuestion(index + 1, quiz.Length, q.prompt, q.a, q.b, q.c, q.d);
        ElectronicsUIManager.Instance?.SetDiodeDiagramVisible(q.diodeDiagram);
        ElectronicsUIManager.Instance?.SetQuestionOptionsVisible(4);
    }

    private static Question[] MainQuestions()
    {
        return new[]
        {
            Mcq("What is the main function of a diode?",
                "To allow current to flow mainly in one direction.",
                "To store electric charge.",
                "To measure voltage.",
                "To increase resistance only.",
                0, "A diode allows electric current to flow mainly in one direction."),
            Mcq("What happens to the bulb during forward bias?",
                "The bulb glows.",
                "The bulb explodes.",
                "The bulb does not glow.",
                "The switch melts.",
                0, "In forward bias the diode allows current, so the bulb glows."),
            Mcq("What happens to the bulb during reverse bias?",
                "The bulb glows brightly.",
                "The bulb does not glow.",
                "The battery reverses itself.",
                "The wires disconnect.",
                1, "In reverse bias the diode blocks current, so the bulb does not glow."),
            Mcq("What is the voltage of two 1.5 V cells connected together?",
                "1.5 V",
                "2.5 V",
                "3 V",
                "4.5 V",
                2, "1.5 V + 1.5 V = 3 V."),
            Mcq("Which component controls the opening and closing of the circuit?",
                "Diode",
                "Bulb",
                "Switch",
                "Breadboard",
                2, "The switch opens and closes the circuit."),
            Mcq("Which component allows current mainly in one direction?",
                "Switch",
                "Bulb",
                "Battery",
                "Diode",
                3, "The diode allows current mainly in one direction."),
            Mcq("What happens when the battery is connected in the opposite direction while the diode remains unchanged?",
                "The bulb always glows brighter.",
                "The diode becomes reverse biased.",
                "The switch is destroyed.",
                "Nothing changes.",
                1, "Reversing only the battery makes the diode reverse biased."),
            Mcq("Why does the bulb not glow during reverse bias?",
                "Because the switch is missing.",
                "Because the battery has no voltage.",
                "Because the diode blocks the current.",
                "Because the bulb is 230 V.",
                2, "The diode blocks the current, so the bulb stays dark."),
            Diagram("Which side of a diode is called the anode?",
                "The arrow side (the side current enters).",
                "The bar / cathode band side.",
                "The battery negative terminal.",
                "The switch lever.",
                0, "The anode is the side current enters. Symbol: Anode → |>| → Cathode.", true),
            Diagram("Which side of a diode is called the cathode?",
                "The arrow / anode side.",
                "The bar side (the band on an IN4001).",
                "The bulb glass.",
                "The breadboard rail.",
                1, "The cathode is the bar side of the diode symbol and the banded end of an IN4001.", true)
        };
    }

    private static Question Mcq(string prompt, string a, string b, string c, string d, int correct, string explanation)
    {
        return new Question { prompt = prompt, a = a, b = b, c = c, d = d, correctIndex = correct, explanation = explanation };
    }

    private static Question Diagram(string prompt, string a, string b, string c, string d, int correct, string explanation, bool anode)
    {
        return new Question
        {
            prompt = prompt, a = a, b = b, c = c, d = d, correctIndex = correct,
            explanation = explanation, diodeDiagram = true, anodeQuestion = anode
        };
    }
}
