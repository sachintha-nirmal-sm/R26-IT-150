using UnityEngine;

public class PowerEnergyQuestionManager : MonoBehaviour
{
    public static PowerEnergyQuestionManager Instance { get; private set; }

    public struct Question
    {
        public string prompt;
        public string a, b, c, d;
        public int correctIndex;
        public string explanation;
        public bool isNumeric;
        public float numericAnswer;
        public string numericHint;
    }

    private Question[] quiz;
    private int index;
    private bool answered;
    private bool inVariables;

    public int QuizCount => quiz.Length;
    public bool HasAnswered => answered;
    public bool IsFinished => quiz != null && index >= quiz.Length;
    public Question CurrentQuestion => quiz[Mathf.Clamp(index, 0, quiz.Length - 1)];

    private void Awake()
    {
        Instance = this;
        BuildQuestions();
    }

    public void StartQuiz()
    {
        index = 0;
        answered = false;
        inVariables = false;
        quiz = MainQuestions();
        ShowCurrent();
    }

    public void StartVariables()
    {
        inVariables = true;
        index = 0;
        answered = false;
        quiz = VariableQuestions();
        ShowCurrent();
    }

    public void Answer(int choice)
    {
        if (answered) return;
        var q = CurrentQuestion;
        if (q.isNumeric) return;
        answered = true;
        bool correct = choice == q.correctIndex;
        ApplyResult(correct, q.explanation);
        PowerEnergyUIManager.Instance?.ShowQuestionExplanation(q.explanation, correct);
    }

    public bool SubmitNumeric(float value)
    {
        if (answered) return false;
        var q = CurrentQuestion;
        if (!q.isNumeric) return false;
        answered = true;
        bool correct = Mathf.Abs(value - q.numericAnswer) <= Mathf.Max(0.05f, 0.02f * Mathf.Abs(q.numericAnswer));
        ApplyResult(correct, q.explanation);
        PowerEnergyUIManager.Instance?.ShowQuestionExplanation(q.explanation, correct);
        return correct;
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
        var cat = inVariables || !IsNumericNow() ? PowerEnergyScoreCategory.Questions : PowerEnergyScoreCategory.Questions;
        if (correct)
        {
            PowerEnergyScoreManager.Instance?.AddToCategory(cat, 5, false);
            PowerEnergyFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + explanation, "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + explanation, "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
    }

    private bool IsNumericNow()
    {
        return quiz != null && index < quiz.Length && quiz[index].isNumeric;
    }

    private void ShowCurrent()
    {
        var q = CurrentQuestion;
        if (q.isNumeric)
            PowerEnergyUIManager.Instance?.ShowNumericQuestion(index + 1, quiz.Length, q.prompt, q.numericHint);
        else
            PowerEnergyUIManager.Instance?.ShowQuestion(index + 1, quiz.Length, q.prompt, q.a, q.b, q.c, q.d);
    }

    private void BuildQuestions()
    {
        quiz = MainQuestions();
    }

    private static Question[] MainQuestions()
    {
        return new[]
        {
            Mcq("What is the formula for electrical power?", "P = VI", "E = Pt", "P = E/V", "I = VR", 0, "Electrical power is P = VI."),
            Mcq("What is the SI unit of power?", "Joule (J)", "Watt (W)", "Volt (V)", "Ampere (A)", 1, "The SI unit of power is the watt (W). 1 W = 1 J/s."),
            Mcq("What is the formula for electrical energy?", "P = VI", "E = V/I", "E = Pt", "E = I/t", 2, "Electrical energy consumed is E = Pt."),
            Mcq("What is the SI unit of electrical energy?", "Watt (W)", "kilowatt hour only", "Joule (J)", "Volt (V)", 2, "The SI unit of energy is the joule (J)."),
            Mcq("What unit is commonly used by domestic electricity meters?", "joule", "watt", "ampere", "kilowatt hour (kWh)", 3, "A domestic electricity meter measures energy in kilowatt hours (kWh)."),
            Mcq("How many joules are equal to 1 kWh?", "3600 J", "36,000 J", "3,600,000 J", "360 J", 2, "1 kWh = 3,600,000 J."),
            Mcq("If power increases while time remains constant, what happens to the energy consumed?", "Energy consumed decreases.", "Energy consumed stays the same.", "Energy consumed increases.", "Power has no effect on energy.", 2, "E = Pt, so if P increases and t is constant, E increases."),
            Mcq("If the same appliance operates for twice as long, what happens to the energy consumed?", "It halves.", "It doubles.", "It stays the same.", "It becomes zero.", 1, "E = Pt. If t doubles and P is unchanged, E doubles."),
            Mcq("Which instrument measures potential difference?", "Ammeter", "Voltmeter", "Timer", "Newton balance", 1, "A voltmeter measures potential difference."),
            Mcq("Which instrument measures current?", "Voltmeter", "Thermometer", "Ammeter", "Ruler", 2, "An ammeter measures current."),
            Mcq("Which appliance has the highest power in this practical?", "Electric bulb", "Electric fan", "Electric iron", "Electric kettle", 3, "The electric kettle has the highest power, about 2000 W."),
            Mcq("Which appliance consumes more energy during the same time?", "The appliance with higher power.", "The appliance with lower current only.", "Always the bulb.", "The one that is switched off.", 0, "For the same time, the appliance with higher power consumes more energy."),
            Numeric("An electric appliance operates at V = 230 V and I = 2 A. Calculate the power.\nP = VI", 460f, "P = 230 × 2 = 460 W", "Enter power in watts."),
            Numeric("An appliance has a power of 500 W and operates for 60 seconds. Calculate the energy consumed.\nE = Pt", 30000f, "E = 500 × 60 = 30,000 J", "Enter energy in joules."),
            Numeric("An appliance consumes 3,600,000 J. Convert this to kWh.", 1f, "3,600,000 / 3,600,000 = 1 kWh", "Enter energy in kWh.")
        };
    }

    private static Question[] VariableQuestions()
    {
        return new[]
        {
            Mcq("In E = Pt, what does P represent?", "Pressure", "Power", "Potential only", "Period", 1, "P stands for power."),
            Mcq("In E = Pt, what does t represent?", "Temperature", "Thickness", "Time", "Tension", 2, "t stands for time."),
            Mcq("What is the SI unit of power?", "Joule", "Watt", "kWh", "Volt", 1, "The SI unit of power is the watt."),
            Mcq("What is the common domestic electrical energy unit?", "Watt", "Ampere", "kWh", "Newton", 2, "Domestic meters use the kilowatt hour (kWh).")
        };
    }

    private static Question Mcq(string prompt, string a, string b, string c, string d, int correct, string explanation)
    {
        return new Question
        {
            prompt = prompt, a = a, b = b, c = c, d = d,
            correctIndex = correct, explanation = explanation
        };
    }

    private static Question Numeric(string prompt, float answer, string explanation, string hint)
    {
        return new Question
        {
            prompt = prompt,
            isNumeric = true,
            numericAnswer = answer,
            explanation = explanation,
            numericHint = hint,
            a = "", b = "", c = "", d = ""
        };
    }
}
