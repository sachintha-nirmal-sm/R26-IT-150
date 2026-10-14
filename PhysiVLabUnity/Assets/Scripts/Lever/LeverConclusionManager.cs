using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeverConclusionManager : MonoBehaviour
{
    public static LeverConclusionManager Instance { get; private set; }

    [SerializeField] private Button optionA;
    [SerializeField] private Button optionB;
    [SerializeField] private Button optionC;
    [SerializeField] private Button optionD;
    [SerializeField] private GameObject explanationPanel;
    [SerializeField] private TextMeshProUGUI explanationText;
    [SerializeField] private TextMeshProUGUI resultsReminder;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button continueBtn;

    private bool answered;
    private bool readyToContinue;
    private Button[] allOptions;
    private LeverExperimentStep activePhase = LeverExperimentStep.Conclusion;

    private const string SharedExplanation =
        "A lever helps us to lift a load more easily by increasing the distance from the pivot to the point where the effort is applied. " +
        "Moment = Force × Distance. Load × a = Effort × x. Effort = Load × a / x. " +
        "When x becomes larger, required effort decreases.";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Bind(Button a, Button b, Button c, Button d,
        GameObject explainPanel = null, TextMeshProUGUI explainText = null, TextMeshProUGUI reminder = null,
        Button continueButton = null, TextMeshProUGUI question = null)
    {
        optionA = a;
        optionB = b;
        optionC = c;
        optionD = d;
        explanationPanel = explainPanel;
        explanationText = explainText;
        resultsReminder = reminder;
        continueBtn = continueButton;
        questionText = question;
        allOptions = new[] { optionA, optionB, optionC, optionD };

        if (continueBtn != null)
        {
            continueBtn.onClick.RemoveAllListeners();
            continueBtn.onClick.AddListener(ContinueToNext);
        }

        if (explanationPanel != null) explanationPanel.SetActive(false);
    }

    public void ShowConclusion()
    {
        var step = LeverExperimentManager.Instance != null
            ? LeverExperimentManager.Instance.CurrentStep
            : LeverExperimentStep.Conclusion;

        activePhase = step;
        answered = false;
        readyToContinue = false;
        if (explanationPanel != null) explanationPanel.SetActive(false);

        UpdateResultsReminder();
        ConfigureForPhase(step);
        LeverUIManager.Instance?.SetNextButtonVisible(false);
    }

    public void ResetConclusion()
    {
        answered = false;
        readyToContinue = false;
        activePhase = LeverExperimentStep.Conclusion;
        if (explanationPanel != null) explanationPanel.SetActive(false);
        SetOptionsInteractable(true);
        ResetOptionColors();
    }

    private void ConfigureForPhase(LeverExperimentStep step)
    {
        switch (step)
        {
            case LeverExperimentStep.Conclusion:
                SetQuestion("What can be concluded from this experiment?");
                SetOptionTexts(
                    "When x increases, the effort required increases.",
                    "When x increases, the effort required decreases.",
                    "Changing x has no effect.",
                    "The book becomes heavier when x increases.");
                WireOptions(correctIndex: 1); // B
                LeverUIManager.Instance?.UpdateInstruction(
                    "Conclusion Q1: Choose how effort changes with distance x. Tap A, B, C or D.");
                break;

            case LeverExperimentStep.Conclusion2:
                SetQuestion("When x is greater than a, what happens?");
                SetOptionTexts(
                    "The required effort can be less than the weight of the book.",
                    "The required effort is always greater than the weight of the book.",
                    "The lever cannot lift the book.",
                    "The book becomes heavier.");
                WireOptions(correctIndex: 0); // A
                LeverUIManager.Instance?.UpdateInstruction(
                    "Conclusion Q2: When x > a, choose the best statement. Tap A, B, C or D.");
                break;

            case LeverExperimentStep.Challenge:
                SetQuestion("If the book weight is 10 N, a = 20 cm and x = 50 cm, approximately what effort is required?");
                SetOptionTexts("2 N", "4 N", "10 N", "20 N");
                WireOptions(correctIndex: 1); // 4 N
                LeverUIManager.Instance?.UpdateInstruction(
                    "Challenge: Predict the effort using Effort = Load × a / x.");
                break;

            default:
                WireOptions(correctIndex: 1);
                break;
        }
    }

    private void WireOptions(int correctIndex)
    {
        if (allOptions == null) allOptions = new[] { optionA, optionB, optionC, optionD };
        for (int i = 0; i < allOptions.Length; i++)
            BindOption(allOptions[i], i == correctIndex);
        SetOptionsInteractable(true);
        ResetOptionColors();
    }

    private void BindOption(Button btn, bool correct)
    {
        if (btn == null) return;
        btn.interactable = true;
        btn.onClick.RemoveAllListeners();
        bool isCorrect = correct;
        btn.onClick.AddListener(() => SubmitAnswer(btn, isCorrect));
    }

    private void SubmitAnswer(Button chosen, bool correct)
    {
        var step = LeverExperimentManager.Instance != null
            ? LeverExperimentManager.Instance.CurrentStep
            : activePhase;

        if (step != LeverExperimentStep.Conclusion &&
            step != LeverExperimentStep.Conclusion2 &&
            step != LeverExperimentStep.Challenge)
        {
            LeverScoreManager.Instance?.SubtractScore(5);
            return;
        }

        // After a correct answer, ignore further option clicks until Continue.
        if (answered) return;

        if (correct)
        {
            answered = true;
            readyToContinue = true;
            Highlight(chosen, new Color(0.55f, 0.92f, 0.65f));
            SetOptionsInteractable(false);

            int points = step == LeverExperimentStep.Challenge ? 5 : 10;
            LeverScoreManager.Instance?.AddScore(points);

            string correctLetter = GetCorrectLetter(step);
            string msg =
                $"✓ Correct answer: {correctLetter}\n\n{SharedExplanation}";

            if (step == LeverExperimentStep.Challenge)
            {
                msg =
                    "✓ Correct: 4 N\n\n" +
                    "Effort = Load × a / x = 10 × 20 / 50 = 4 N\n\n" +
                    SharedExplanation;
            }

            if (explanationText != null) explanationText.text = msg;
            if (explanationPanel != null) explanationPanel.SetActive(true);

            LeverFeedbackManager.Instance?.ShowInstruction(
                $"Correct! +{points} marks — read the explanation, then Continue.");
            LeverUIManager.Instance?.UpdateInstruction("Great! Read the explanation, then tap Continue.");
        }
        else
        {
            Highlight(chosen, new Color(1f, 0.72f, 0.72f));
            LeverScoreManager.Instance?.SubtractScore(5);
            LeverGameManager.Instance?.RegisterMistake();

            string hint = step == LeverExperimentStep.Challenge
                ? "Use Effort = Load × a / x. Try again!"
                : step == LeverExperimentStep.Conclusion2
                    ? "When x > a, effort can be smaller than the load. Try again!"
                    : "Look at your results — as x increased, effort decreased. Try again!";

            LeverFeedbackManager.Instance?.ShowInstruction(hint);
            // Wrong answers can be retried (answered stays false).
        }
    }

    private void ContinueToNext()
    {
        if (!readyToContinue) return;
        if (explanationPanel != null) explanationPanel.SetActive(false);
        readyToContinue = false;
        answered = false;
        LeverExperimentManager.Instance?.AdvanceStep();
    }

    private string GetCorrectLetter(LeverExperimentStep step)
    {
        switch (step)
        {
            case LeverExperimentStep.Conclusion: return "B";
            case LeverExperimentStep.Conclusion2: return "A";
            case LeverExperimentStep.Challenge: return "4 N";
            default: return "?";
        }
    }

    private void SetQuestion(string text)
    {
        if (questionText != null) questionText.text = text;
    }

    private void SetOptionTexts(string a, string b, string c, string d)
    {
        SetButtonLabel(optionA, "A. " + a);
        SetButtonLabel(optionB, "B. " + b);
        SetButtonLabel(optionC, "C. " + c);
        SetButtonLabel(optionD, "D. " + d);
    }

    private void SetButtonLabel(Button btn, string text)
    {
        if (btn == null) return;
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = text;
    }

    private void UpdateResultsReminder()
    {
        if (resultsReminder == null) return;
        var readings = LeverExperimentDataManager.Instance?.Readings;
        if (readings == null || readings.Count == 0)
        {
            resultsReminder.text = "Hint: As distance x increased, the required effort decreased.";
            return;
        }

        string line = "Your results: ";
        for (int i = 0; i < readings.Count; i++)
        {
            var r = readings[i];
            float effort = r.measuredEffort > 0f ? r.measuredEffort : r.requiredEffort;
            line += $"x={r.distanceX:0} cm → {effort:0.0} N";
            if (i < readings.Count - 1) line += "   |   ";
        }
        resultsReminder.text = line;
    }

    private void SetOptionsInteractable(bool value)
    {
        if (allOptions == null) return;
        foreach (var btn in allOptions)
            if (btn != null) btn.interactable = value;
    }

    private void ResetOptionColors()
    {
        if (allOptions == null) return;
        foreach (var btn in allOptions)
            SetButtonColor(btn, Color.white);
    }

    private void Highlight(Button btn, Color color) => SetButtonColor(btn, color);

    private void SetButtonColor(Button btn, Color color)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = color;
    }
}
