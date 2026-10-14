using UnityEngine;

public class EquilibriumQuestionManager : MonoBehaviour
{
    public static EquilibriumQuestionManager Instance { get; private set; }

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
        EquilibriumUIManager.Instance?.ShowQuestionExplanation(q.explanation, correct);
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
            EquilibriumScoreManager.Instance?.AddScore(5, false);
            EquilibriumFeedbackManager.Instance?.ShowMessage("✓ CORRECT\n" + explanation, "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            EquilibriumScoreManager.Instance?.SubtractScore(5);
            EquilibriumFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + explanation, "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
    }

    private void ShowCurrent()
    {
        var q = CurrentQuestion;
        EquilibriumUIManager.Instance?.ShowQuestion(index + 1, quiz.Length, q.prompt, q.a, q.b, q.c, q.d);
    }

    private void BuildQuestions()
    {
        quiz = new[]
        {
            Mcq("What condition must be true for the hanging meter ruler to be in equilibrium?",
                "F1 = F2 only", "F1 + F2 = W, and the ruler is horizontal", "F1 × F2 = W", "W is larger than F1 + F2",
                1, "For equilibrium, the two upward forces together equal the downward weight, and the net turning effect is zero (ruler horizontal)."),
            Mcq("Which instruments measure F1 and F2?",
                "Thermometers", "Two spring balances", "A stopwatch and a ruler", "An ammeter",
                1, "The two spring balances give the upward forces F1 and F2 in newtons."),
            Mcq("Where does the weight W of the meter ruler act?",
                "At the left end", "At the right end", "At the centre of gravity (the middle of a uniform ruler)", "At the stand",
                2, "For a uniform meter ruler the centre of gravity is at the 50 cm mark, so W acts downward there."),
            Mcq("Why are rubber bands used?",
                "To measure current", "To loop around the ends of the ruler and hook onto the spring balances", "To heat the ruler", "To make the ruler heavier",
                1, "Each rubber band connects one end of the ruler to a spring balance."),
            Mcq("The three forces F1, F2 and W are",
                "non-parallel and in different planes", "coplanar parallel forces", "magnetic forces", "electrical forces",
                1, "They all act in the same vertical plane and are parallel (two up, one down)."),
            Mcq("If F1 = 0.60 N and F2 = 0.60 N, the weight of the ruler is",
                "0.30 N", "0.60 N", "1.20 N", "1.80 N",
                2, "F1 + F2 = W, so W = 0.60 + 0.60 = 1.20 N."),
            Mcq("A student leaves the ruler tilted. Why is that not equilibrium of a horizontal object?",
                "The spring balances cannot read force", "There is a net turning effect, so the ruler is not in a balanced horizontal position", "W becomes zero", "Rubber bands measure time",
                1, "Equilibrium in this practical means the ruler stays horizontal: net force and net moment are both zero."),
            Mcq("If the forces are pushed out of the same plane, what happens?",
                "The readings still prove F1 + F2 = W easily", "The ruler may twist and equilibrium in one plane is lost", "W doubles", "The stand disappears",
                1, "The forces must remain coplanar. If they are not in one plane the ruler can twist."),
            Mcq("Which quantity is measured first in this practical?",
                "The length of the stand", "The weight W of the meter ruler alone", "Room temperature", "The colour of the rubber bands",
                1, "W is measured with a spring balance before the ruler is hung from both ends."),
            Mcq("The rule tested by this experiment is",
                "F1 − F2 = W", "F1 + F2 = W for a ruler in equilibrium under three coplanar parallel forces", "F1 × F2 = W", "W = F1 only",
                1, "The sum of the two upward forces equals the downward weight of the ruler.")
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
