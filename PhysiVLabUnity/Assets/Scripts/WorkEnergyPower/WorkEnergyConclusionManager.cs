using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkEnergyConclusionManager : MonoBehaviour
{
    public static WorkEnergyConclusionManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button optionA;
    [SerializeField] private Button optionB;
    [SerializeField] private Button optionC;
    [SerializeField] private Button optionD;
    [SerializeField] private GameObject explanationPanel;
    [SerializeField] private TextMeshProUGUI explanationText;
    [SerializeField] private Button continueBtn;

    private bool answered;
    private bool conclusionAwarded;
    private WorkEnergyExperimentStep activeQuestion = WorkEnergyExperimentStep.ConclusionQ1;
    private int correctLetterIndex = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(TextMeshProUGUI question, Button a, Button b, Button c, Button d,
        GameObject explainPanel, TextMeshProUGUI explainText, Button continueButton)
    {
        questionText = question;
        optionA = a; optionB = b; optionC = c; optionD = d;
        explanationPanel = explainPanel;
        explanationText = explainText;
        continueBtn = continueButton;
        if (continueBtn != null)
        {
            continueBtn.onClick.RemoveAllListeners();
            continueBtn.onClick.AddListener(Continue);
        }
        if (explanationPanel != null) explanationPanel.SetActive(false);
    }

    public void ShowQuestion(WorkEnergyExperimentStep step)
    {
        answered = false;
        activeQuestion = step;
        if (explanationPanel != null) explanationPanel.SetActive(false);
        SetOptionsInteractable(true);
        ResetOptionColors();

        if (step == WorkEnergyExperimentStep.ConclusionQ1)
        {
            correctLetterIndex = 1;
            SetQuestion("What happens to the gravitational potential energy when the height of the object is increased?",
                "It decreases.",
                "It increases.",
                "It remains zero.",
                "It does not depend on height.");
        }
        else if (step == WorkEnergyExperimentStep.ConclusionQ2)
        {
            correctLetterIndex = 1;
            SetQuestion("Which equation represents gravitational potential energy?",
                "PE = mv",
                "PE = mgh",
                "PE = F/t",
                "PE = V/I");
        }
        else
        {
            correctLetterIndex = 0;
            SetQuestion("What happens to the depression depth when the same weight is released from a greater height?",
                "It generally becomes deeper.",
                "It always becomes zero.",
                "It becomes shallower.",
                "It does not change.");
        }

        WireOptions();
        WorkEnergyUIManager.Instance?.SetNextButtonVisible(true);
        WorkEnergyUIManager.Instance?.SetNextButtonLabel("SKIP QUESTION");
    }

    public void ShowFinalConclusion()
    {
        answered = true;
        if (!conclusionAwarded)
        {
            conclusionAwarded = true;
            WorkEnergyScoreManager.Instance?.AddScore(10);
            WorkEnergyScoreManager.Instance?.AddScore(5);
        }
        if (questionText != null)
        {
            questionText.text =
                "CONCLUSION\n\n" +
                "The gravitational potential energy of an object increases with its height above the reference level.\n\n" +
                "When the same weight is released from greater heights, the impact is greater and the depression produced in the clay generally becomes deeper.\n\n" +
                "PE = mgh\nTherefore PE ∝ h when mass and g are constant.";
        }
        HideOptions(true);
        if (explanationPanel != null) explanationPanel.SetActive(true);
        if (explanationText != null)
            explanationText.text = "Not all of the gravitational potential energy becomes useful deformation energy in the clay. Some energy is transferred to sound, heat, motion and deformation of the object and clay.";
        if (continueBtn != null)
        {
            continueBtn.gameObject.SetActive(true);
            var txt = continueBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = "Finish ▶";
        }
    }

    public void ResetConclusion()
    {
        answered = false;
        conclusionAwarded = false;
        if (explanationPanel != null) explanationPanel.SetActive(false);
        HideOptions(false);
    }

    private void SetQuestion(string q, string a, string b, string c, string d)
    {
        if (questionText != null) questionText.text = q;
        SetOptionLabel(optionA, "A", a);
        SetOptionLabel(optionB, "B", b);
        SetOptionLabel(optionC, "C", c);
        SetOptionLabel(optionD, "D", d);
    }

    private void SetOptionLabel(Button btn, string letter, string body)
    {
        if (btn == null) return;
        var bodyTxt = btn.transform.Find("Body")?.GetComponent<TextMeshProUGUI>();
        if (bodyTxt != null) bodyTxt.text = body;
        var letterTxt = btn.transform.Find("Letter/LetterText")?.GetComponent<TextMeshProUGUI>();
        if (letterTxt != null) letterTxt.text = letter;
    }

    private void WireOptions()
    {
        BindOption(optionA, 0);
        BindOption(optionB, 1);
        BindOption(optionC, 2);
        BindOption(optionD, 3);
    }

    private void BindOption(Button btn, int index)
    {
        if (btn == null) return;
        btn.gameObject.SetActive(true);
        btn.interactable = true;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => Submit(index, btn));
    }

    private void Submit(int index, Button chosen)
    {
        if (answered) return;
        bool correct = index == correctLetterIndex;
        if (correct)
        {
            answered = true;
            Highlight(chosen, new Color(0.55f, 0.92f, 0.65f));
            SetOptionsInteractable(false);
            WorkEnergyScoreManager.Instance?.AddScore(10);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("✓ Correct! +10 marks");
            WorkEnergyPowerExperimentManager.Instance?.AdvanceStep();
        }
        else
        {
            Highlight(chosen, new Color(1f, 0.72f, 0.72f));
            WorkEnergyScoreManager.Instance?.SubtractScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("Not quite. Read the question again and try another option.");
        }
    }

    private void Continue()
    {
        if (WorkEnergyPowerExperimentManager.Instance != null &&
            WorkEnergyPowerExperimentManager.Instance.CurrentStep == WorkEnergyExperimentStep.Conclusion)
        {
            WorkEnergyPowerExperimentManager.Instance.AdvanceStep();
        }
    }

    private void HideOptions(bool hide)
    {
        if (optionA != null) optionA.gameObject.SetActive(!hide);
        if (optionB != null) optionB.gameObject.SetActive(!hide);
        if (optionC != null) optionC.gameObject.SetActive(!hide);
        if (optionD != null) optionD.gameObject.SetActive(!hide);
    }

    private void SetOptionsInteractable(bool value)
    {
        if (optionA != null) optionA.interactable = value;
        if (optionB != null) optionB.interactable = value;
        if (optionC != null) optionC.interactable = value;
        if (optionD != null) optionD.interactable = value;
    }

    private void ResetOptionColors()
    {
        Highlight(optionA, Color.white);
        Highlight(optionB, Color.white);
        Highlight(optionC, Color.white);
        Highlight(optionD, Color.white);
    }

    private void Highlight(Button btn, Color color)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = color;
    }
}
