using UnityEngine;
using UnityEngine.UI;

public class ElecQuestionManager : MonoBehaviour
{
    public static ElecQuestionManager Instance { get; private set; }

    public struct Question
    {
        public string prompt;
        public string a, b, c, d;
        public int correctIndex;
        public string explanation;
    }

    private Question[] quiz;
    private Question[] compare;
    private int index;
    private bool answered;
    private bool compareMode;

    private void Awake()
    {
        Instance = this;
        BuildQuestions();
    }

    public int QuizCount => quiz.Length;
    public int CompareCount => compare.Length;

    public void StartCompare()
    {
        compareMode = true;
        index = 0;
        answered = false;
        ShowCurrent();
    }

    public void StartQuiz()
    {
        compareMode = false;
        index = 0;
        answered = false;
        ShowCurrent();
    }

    public void Answer(int choice)
    {
        if (answered) return;
        answered = true;
        var q = Current();
        bool correct = choice == q.correctIndex;
        if (correct)
        {
            ElecScoreManager.Instance?.AddScore(10);
            ElecFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + q.explanation, "+10 Marks", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            ElecScoreManager.Instance?.SubtractScore(5);
            ElecFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + q.explanation, "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
        }
        ElecUIManager.Instance?.ShowQuestionExplanation(q.explanation, correct);
    }

    public bool Advance()
    {
        if (!answered) return false;
        index++;
        answered = false;
        if (index >= CurrentList().Length)
            return true;
        ShowCurrent();
        return false;
    }

    private void ShowCurrent()
    {
        var q = Current();
        int total = CurrentList().Length;
        ElecUIManager.Instance?.ShowQuestion(index + 1, total, q.prompt, q.a, q.b, q.c, q.d);
    }

    private Question Current() => CurrentList()[Mathf.Clamp(index, 0, CurrentList().Length - 1)];

    private Question[] CurrentList() => compareMode ? compare : quiz;

    private void BuildQuestions()
    {
        compare = new[]
        {
            Q("Which connection produced the greatest potential difference?",
                "Connection 1 – series aiding", "Connection 2 – parallel", "Connection 3 – series opposing", "All were equal",
                0, "Series aiding adds the cell voltages, so Connection 1 has the greatest potential difference."),
            Q("Which connection produced the greatest current in the simplified model?",
                "Connection 2", "Connection 1", "Connection 3", "None",
                1, "With resistance constant, the largest voltage (Connection 1) produces the largest current."),
            Q("Which connection produced approximately zero potential difference?",
                "Connection 1 – series aiding", "Connection 2 – parallel", "Connection 3 – series opposing", "None",
                2, "In series opposition the equal cell voltages cancel, so the net potential difference is approximately zero."),
            Q("Which connection keeps the voltage approximately equal to one cell voltage?",
                "Connection 1 – series aiding", "Connection 2 – parallel", "Connection 3 – series opposing", "An open circuit",
                1, "Identical cells in parallel keep the potential difference approximately equal to one cell.")
        };

        quiz = new[]
        {
            Q("How should an ammeter be connected?",
                "In parallel", "In series", "Across the cell", "Across the bulb only",
                1, "An ammeter is connected in series so the same current that passes through the bulb also passes through the meter."),
            Q("How should a voltmeter be connected to measure the potential difference across the bulb?",
                "In series", "In parallel across the bulb", "Across the ammeter", "In series with the cells",
                1, "A voltmeter must be connected in parallel across the bulb."),
            Q("When two identical cells are connected in series aiding, what happens to the total potential difference?",
                "It approximately doubles", "It becomes zero", "It becomes half", "It remains exactly one cell voltage",
                0, "In series aiding the potential differences add, so two 1.5 V cells give about 3.0 V."),
            Q("When two identical cells are connected in series opposition, what is the ideal net potential difference?",
                "3.0 V", "1.5 V", "0 V", "6 V",
                2, "Equal opposing voltages cancel, so the ideal net potential difference is 0 V."),
            Q("Which quantity is measured using an ammeter?",
                "Voltage", "Current", "Resistance", "Power",
                1, "An ammeter measures current."),
            Q("Which quantity is measured using a voltmeter?",
                "Current", "Charge", "Potential difference", "Resistance",
                2, "A voltmeter measures potential difference.")
        };
    }

    private static Question Q(string prompt, string a, string b, string c, string d, int correct, string explanation)
    {
        return new Question { prompt = prompt, a = a, b = b, c = c, d = d, correctIndex = correct, explanation = explanation };
    }
}
