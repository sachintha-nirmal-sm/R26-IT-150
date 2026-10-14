using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ElecUIManager : MonoBehaviour
{
    public static ElecUIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI attemptsText;
    [SerializeField] private TextMeshProUGUI connectionText;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private GameObject introPanel;
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private GameObject instructionBar;
    [SerializeField] private GameObject equipmentPanel;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject laboratoryPanel;
    [SerializeField] private GameObject dataTablePanel;
    [SerializeField] private GameObject comparePanel;
    [SerializeField] private GameObject graphPanel;
    [SerializeField] private GameObject educationPanel;
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private GameObject conclusionPanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI introText;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private TextMeshProUGUI educationText;
    [SerializeField] private TextMeshProUGUI conclusionText;
    [SerializeField] private TextMeshProUGUI compareText;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI resultDetailsText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button startBtn;
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button resetBtn;
    [SerializeField] private Button retryBtn;
    [SerializeField] private Button viewProfileBtn;
    [SerializeField] private Button viewResultsBtn;
    [SerializeField] private GameObject resetConfirmPanel;
    [SerializeField] private Button resetYes;
    [SerializeField] private Button resetNo;
    [SerializeField] private Button equipContinueBtn;
    [SerializeField] private Button checkCircuitBtn;
    [SerializeField] private Button testCircuitBtn;
    [SerializeField] private Button measureVBtn;
    [SerializeField] private Button measureIBtn;
    [SerializeField] private Button recordBtn;
    [SerializeField] private Button undoWireBtn;
    [SerializeField] private Button rotateBtn;
    [SerializeField] private Button brightHigh;
    [SerializeField] private Button brightMed;
    [SerializeField] private Button brightOff;
    [SerializeField] private Button questionA, questionB, questionC, questionD, questionContinue;
    [SerializeField] private GameObject questionExplanationPanel;
    [SerializeField] private TextMeshProUGUI questionExplanationText;
    [SerializeField] private TextMeshProUGUI optAText, optBText, optCText, optDText;

    private bool practicalRunning;

    private void Awake() => Instance = this;

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void BindAll(ElecUIRefsHolder refs, bool showWelcome)
    {
        titleText = refs.Title; scoreText = refs.Score; progressText = refs.Progress;
        attemptsText = refs.Attempts; connectionText = refs.ConnectionLabel;
        progressBarFill = refs.ProgressBar;
        instructionText = refs.Instruction;
        introPanel = refs.IntroPanel; objectivePanel = refs.ObjectivePanel;
        instructionBar = refs.InstructionBar; equipmentPanel = refs.EquipmentPanel;
        tutorialPanel = refs.TutorialPanel; laboratoryPanel = refs.LaboratoryPanel;
        dataTablePanel = refs.DataTablePanel; comparePanel = refs.ComparePanel;
        graphPanel = refs.GraphPanel; educationPanel = refs.EducationPanel;
        questionPanel = refs.QuestionPanel; conclusionPanel = refs.ConclusionPanel;
        resultPanel = refs.ResultPanel;
        introText = refs.IntroText; objectiveText = refs.ObjectiveText;
        tutorialText = refs.TutorialText; educationText = refs.EducationText;
        conclusionText = refs.ConclusionText; compareText = refs.CompareText;
        questionText = refs.QuestionText;
        finalScoreText = refs.FinalScore; resultDetailsText = refs.ResultDetails; statusText = refs.StatusText;
        startBtn = refs.StartBtn; nextBtn = refs.Next; resetBtn = refs.Reset;
        retryBtn = refs.Retry; viewProfileBtn = refs.ViewProfileBtn; viewResultsBtn = refs.ViewResultsBtn;
        equipContinueBtn = refs.EquipContinueBtn;
        resetConfirmPanel = refs.ResetConfirm; resetYes = refs.ResetYes; resetNo = refs.ResetNo;
        checkCircuitBtn = refs.CheckCircuitBtn; testCircuitBtn = refs.TestCircuitBtn;
        measureVBtn = refs.MeasureVBtn; measureIBtn = refs.MeasureIBtn; recordBtn = refs.RecordBtn;
        undoWireBtn = refs.UndoWireBtn; rotateBtn = refs.RotateBtn;
        brightHigh = refs.BrightHigh; brightMed = refs.BrightMed; brightOff = refs.BrightOff;
        questionA = refs.QuestionA; questionB = refs.QuestionB; questionC = refs.QuestionC; questionD = refs.QuestionD;
        questionContinue = refs.QuestionContinue;
        questionExplanationPanel = refs.QuestionExplanationPanel;
        questionExplanationText = refs.QuestionExplanationText;
        optAText = refs.OptAText; optBText = refs.OptBText; optCText = refs.OptCText; optDText = refs.OptDText;
        WireButtons();
        if (showWelcome) ShowIntro();
    }

    private void WireButtons()
    {
        WireBtn(startBtn, StartPractical);
        WireBtn(nextBtn, OnNextPressed);
        if (equipContinueBtn != null) equipContinueBtn.gameObject.SetActive(false);
        WireBtn(resetBtn, () => { if (resetConfirmPanel != null) resetConfirmPanel.SetActive(true); });
        WireBtn(resetYes, () =>
        {
            resetConfirmPanel?.SetActive(false);
            CurrentElectricityExperimentManager.Instance?.ResetExperiment();
            ShowIntro();
        });
        WireBtn(resetNo, () => resetConfirmPanel?.SetActive(false));
        WireBtn(retryBtn, () => CurrentElectricityExperimentManager.Instance?.RetryExperiment());
        WireBtn(viewProfileBtn, () =>
        {
            string summary = ElecProfileManager.Instance != null ? ElecProfileManager.Instance.GetProfileSummary() : "No profile data.";
            ElecFeedbackManager.Instance?.ShowInstruction(summary);
        });
        WireBtn(viewResultsBtn, () => resultPanel?.SetActive(true));
        WireBtn(checkCircuitBtn, () => CircuitBuilder.Instance?.CheckCircuit());
        WireBtn(testCircuitBtn, () => CircuitBuilder.Instance?.TestCircuit());
        WireBtn(measureVBtn, () => CircuitBuilder.Instance?.MeasureVoltage());
        WireBtn(measureIBtn, () => CircuitBuilder.Instance?.MeasureCurrent());
        WireBtn(recordBtn, () => CurrentElectricityExperimentManager.Instance?.CompleteConnectionRecord());
        WireBtn(undoWireBtn, () => CircuitBuilder.Instance?.UndoLastWire());
        WireBtn(rotateBtn, () => CircuitBuilder.Instance?.RotateSelectedCell());
        WireBtn(brightHigh, () => CircuitBuilder.Instance?.SetBrightnessChoice("High"));
        WireBtn(brightMed, () => CircuitBuilder.Instance?.SetBrightnessChoice("Medium"));
        WireBtn(brightOff, () => CircuitBuilder.Instance?.SetBrightnessChoice("OFF"));
        WireBtn(questionA, () => ElecQuestionManager.Instance?.Answer(0));
        WireBtn(questionB, () => ElecQuestionManager.Instance?.Answer(1));
        WireBtn(questionC, () => ElecQuestionManager.Instance?.Answer(2));
        WireBtn(questionD, () => ElecQuestionManager.Instance?.Answer(3));
        WireBtn(questionContinue, OnQuestionContinue);
    }

    private void WireBtn(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    public void StartPractical()
    {
        practicalRunning = true;
        CurrentElectricityExperimentManager.Instance?.StartPractical();
    }

    public void GoNextFromEquipment()
    {
        CurrentElectricityExperimentManager.Instance?.TryAdvanceFromEquipment();
    }

    private void OnNextPressed()
    {
        var step = CurrentElectricityExperimentManager.Instance != null
            ? CurrentElectricityExperimentManager.Instance.CurrentStep
            : ElecExperimentStep.Introduction;
        if (step == ElecExperimentStep.Introduction) { StartPractical(); return; }
        if (step == ElecExperimentStep.SelectEquipment) { GoNextFromEquipment(); return; }
        if (step == ElecExperimentStep.CircuitTutorial)
        {
            CurrentElectricityExperimentManager.Instance?.OnTutorialContinue();
            return;
        }
        if (step == ElecExperimentStep.ComparisonQuestions || step == ElecExperimentStep.Questions)
        {
            OnQuestionContinue();
            return;
        }
        CurrentElectricityExperimentManager.Instance?.AdvanceStep();
    }

    private void OnQuestionContinue()
    {
        bool done = ElecQuestionManager.Instance != null && ElecQuestionManager.Instance.Advance();
        if (done) CurrentElectricityExperimentManager.Instance?.AdvanceStep();
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(false);
    }

    public void ShowIntro()
    {
        practicalRunning = false;
        HideAllContent();
        if (introPanel != null) introPanel.SetActive(true);
        if (instructionBar != null) instructionBar.SetActive(false);
        SetNextButtonVisible(false);
        if (introText != null)
        {
            introText.text =
                "CURRENT ELECTRICITY\n\n" +
                "Activity:\nInvestigating Different Connections of Two Dry Cells\n\n" +
                "In this practical you will connect two dry cells to a bulb in three different ways. " +
                "You will observe the brightness of the bulb and measure the potential difference and current for each connection.";
        }
        UpdateAttemptsDisplay(ElecAttemptManager.Instance != null ? ElecAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateScoreDisplay(0);
        UpdateProgress(1, 14);
        if (connectionText != null) connectionText.text = "Connection: —";
    }

    public void ShowStep(ElecExperimentStep step)
    {
        HideAllContent();
        if (instructionBar != null) instructionBar.SetActive(step != ElecExperimentStep.Introduction && step != ElecExperimentStep.Complete);
        SetNextButtonVisible(step != ElecExperimentStep.Connection1 && step != ElecExperimentStep.Connection2 && step != ElecExperimentStep.Connection3);

        switch (step)
        {
            case ElecExperimentStep.Introduction:
                ShowIntro();
                break;
            case ElecExperimentStep.Objective:
                objectivePanel?.SetActive(true);
                if (objectiveText != null)
                    objectiveText.text =
                        "OBJECTIVE\n\n" +
                        "To investigate how different connections of two dry cells affect the potential difference across a bulb, " +
                        "the current through the bulb, and the brightness of the bulb.\n\n" +
                        "You will measure:\nVoltage (V)\nCurrent (A)\n\nand observe:\nBulb brightness.";
                SetInstruction("Read the objective, then press CONTINUE.");
                SetNextButtonVisible(true);
                if (nextBtn != null) SetButtonLabel(nextBtn, "CONTINUE");
                break;
            case ElecExperimentStep.SelectEquipment:
                equipmentPanel?.SetActive(true);
                ElecEquipmentSelectionManager.Instance?.EnsureCardsVisible();
                if (equipContinueBtn != null) equipContinueBtn.gameObject.SetActive(false);
                SetInstruction("Tap the equipment you need. Correct items move to the blue tray at the top. Wrong items lose marks.");
                SetNextButtonVisible(true);
                if (nextBtn != null) SetButtonLabel(nextBtn, "NEXT STEP");
                break;
            case ElecExperimentStep.CircuitTutorial:
                tutorialPanel?.SetActive(true);
                if (tutorialText != null)
                    tutorialText.text =
                        "HOW TO BUILD THE CIRCUIT\n\n" +
                        "You will make 3 circuits with the same two dry cells.\n\n" +
                        "RULES\n" +
                        "• Ammeter — in series (in the main loop with the bulb)\n" +
                        "• Voltmeter — in parallel (across the two bulb terminals only)\n\n" +
                        "THE THREE CONNECTIONS\n" +
                        "1. Series aiding — Cell1 [+][−] joined to Cell2 [+][−]  (voltages ADD)\n" +
                        "2. Parallel — both + together, both − together  (voltage stays 1.5 V)\n" +
                        "3. Series opposing — like poles together  (voltages CANCEL)\n\n" +
                        "ON THE BOARD\n" +
                        "1. Drag each part from the tray onto the green board (or tap it).\n" +
                        "2. Tap one terminal, then tap another terminal to add a wire.\n" +
                        "3. Use ↻ on a cell if you need to reverse + and −.\n" +
                        "4. Press CHECK CIRCUIT. If it is correct, press TEST CIRCUIT.\n" +
                        "5. Then measure voltage, measure current, and record.";
                SetInstruction("Read these steps. Then press BUILD CONNECTION 1.");
                SetNextButtonVisible(true);
                if (nextBtn != null) SetButtonLabel(nextBtn, "BUILD CONNECTION 1");
                break;
            case ElecExperimentStep.Connection1:
            case ElecExperimentStep.Connection2:
            case ElecExperimentStep.Connection3:
                laboratoryPanel?.SetActive(true);
                SetInstruction(InstructionForConnection(step));
                if (nextBtn != null) SetButtonLabel(nextBtn, "NEXT CONNECTION");
                SetNextButtonVisible(CircuitBuilder.Instance != null && CircuitBuilder.Instance.Phase == CircuitLabPhase.Recorded);
                break;
            case ElecExperimentStep.ComparisonTable:
                dataTablePanel?.SetActive(true);
                ElecObservationTableManager.Instance?.Refresh();
                SetInstruction("Compare your three sets of readings in the observation table.");
                if (nextBtn != null) SetButtonLabel(nextBtn, "COMPARE THE RESULTS");
                break;
            case ElecExperimentStep.ComparisonQuestions:
                questionPanel?.SetActive(true);
                SetInstruction("Answer the comparison questions using your results.");
                SetNextButtonVisible(false);
                break;
            case ElecExperimentStep.ViewGraph:
                graphPanel?.SetActive(true);
                ElecGraphController.Instance?.ShowGraphs();
                SetInstruction("The graphs are generated from your recorded experiment data.");
                if (nextBtn != null) SetButtonLabel(nextBtn, "CONTINUE");
                break;
            case ElecExperimentStep.Education:
                educationPanel?.SetActive(true);
                if (educationText != null) educationText.text = EducationCopy();
                SetInstruction("Read the explanation of series, parallel and opposing connections.");
                if (nextBtn != null) SetButtonLabel(nextBtn, "QUESTIONS");
                break;
            case ElecExperimentStep.Questions:
                questionPanel?.SetActive(true);
                SetInstruction("Answer the questions. Each correct answer scores +10. An incorrect answer scores −5.");
                SetNextButtonVisible(false);
                break;
            case ElecExperimentStep.Conclusion:
                conclusionPanel?.SetActive(true);
                if (conclusionText != null) conclusionText.text = ConclusionCopy();
                SetInstruction("Read the conclusion, then view your final score.");
                if (nextBtn != null) SetButtonLabel(nextBtn, "VIEW SCORE");
                break;
            case ElecExperimentStep.Complete:
                resultPanel?.SetActive(true);
                SetNextButtonVisible(false);
                break;
        }
    }

    public void ShowQuestion(int number, int total, string prompt, string a, string b, string c, string d)
    {
        questionPanel?.SetActive(true);
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(false);
        if (questionText != null) questionText.text = $"QUESTION {number} / {total}\n\n{prompt}";
        SetOption(optAText, "A. " + a);
        SetOption(optBText, "B. " + b);
        SetOption(optCText, "C. " + c);
        SetOption(optDText, "D. " + d);
        EnableQuestionButtons(true);
    }

    public void ShowQuestionExplanation(string explanation, bool correct)
    {
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(true);
        if (questionExplanationText != null)
        {
            questionExplanationText.text = (correct ? "✓ CORRECT\n" : "✗ INCORRECT\n") + explanation;
            questionExplanationText.color = correct ? new Color(0.9f, 1f, 0.9f) : Color.white;
        }
        EnableQuestionButtons(false);
    }

    private void EnableQuestionButtons(bool on)
    {
        if (questionA != null) questionA.interactable = on;
        if (questionB != null) questionB.interactable = on;
        if (questionC != null) questionC.interactable = on;
        if (questionD != null) questionD.interactable = on;
    }

    private static void SetOption(TextMeshProUGUI tmp, string text)
    {
        if (tmp != null) tmp.text = text;
    }

    public void SetLabButtons(bool check, bool test, bool measureV, bool measureI, bool record, bool tools)
    {
        if (checkCircuitBtn != null) checkCircuitBtn.gameObject.SetActive(check);
        if (testCircuitBtn != null) testCircuitBtn.gameObject.SetActive(test);
        if (measureVBtn != null) measureVBtn.gameObject.SetActive(measureV);
        if (measureIBtn != null) measureIBtn.gameObject.SetActive(measureI);
        if (recordBtn != null) recordBtn.gameObject.SetActive(record);
        if (undoWireBtn != null) undoWireBtn.gameObject.SetActive(tools);
        if (rotateBtn != null) rotateBtn.gameObject.SetActive(tools);
        if (brightHigh != null) brightHigh.gameObject.SetActive(record);
        if (brightMed != null) brightMed.gameObject.SetActive(record);
        if (brightOff != null) brightOff.gameObject.SetActive(record);
    }

    public void SetNextButtonVisible(bool visible)
    {
        if (nextBtn != null) nextBtn.gameObject.SetActive(visible);
    }

    public void UpdateScoreDisplay(int score)
    {
        if (scoreText != null) scoreText.text = $"Score: {score} / 100";
    }

    public void UpdateProgress(int step, int total)
    {
        if (progressText != null) progressText.text = $"Step: {step} / {total}";
        if (progressBarFill != null) progressBarFill.fillAmount = total > 0 ? step / (float)total : 0f;
    }

    public void UpdateAttemptsDisplay(int remaining)
    {
        if (attemptsText != null) attemptsText.text = $"Attempts Remaining: {remaining}";
    }

    public void UpdateConnectionLabel(ElecExperimentStep step)
    {
        if (connectionText == null) return;
        if (step == ElecExperimentStep.Connection1) connectionText.text = "Connection: 1 / 3";
        else if (step == ElecExperimentStep.Connection2) connectionText.text = "Connection: 2 / 3";
        else if (step == ElecExperimentStep.Connection3) connectionText.text = "Connection: 3 / 3";
        else connectionText.text = "Connection: —";
    }

    public void ShowResult(int score, bool passed, int mistakes, ElecAttemptRecord attempt)
    {
        HideAllContent();
        resultPanel?.SetActive(true);
        instructionBar?.SetActive(false);
        SetNextButtonVisible(false);
        if (retryBtn != null)
            retryBtn.gameObject.SetActive(ElecAttemptManager.Instance == null || ElecAttemptManager.Instance.CanRetry());
        ElecResultManager.Instance?.ShowResult(score, passed, mistakes, attempt);
    }

    public void HideResult()
    {
        resultPanel?.SetActive(false);
        if (retryBtn != null) retryBtn.gameObject.SetActive(false);
    }

    private void HideAllContent()
    {
        introPanel?.SetActive(false);
        objectivePanel?.SetActive(false);
        equipmentPanel?.SetActive(false);
        tutorialPanel?.SetActive(false);
        laboratoryPanel?.SetActive(false);
        dataTablePanel?.SetActive(false);
        comparePanel?.SetActive(false);
        graphPanel?.SetActive(false);
        educationPanel?.SetActive(false);
        questionPanel?.SetActive(false);
        conclusionPanel?.SetActive(false);
        resultPanel?.SetActive(false);
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(false);
    }

    private void SetInstruction(string text)
    {
        if (instructionText != null) instructionText.text = text;
    }

    private static void SetButtonLabel(Button btn, string label)
    {
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = label;
    }

    private static string InstructionForConnection(ElecExperimentStep step)
    {
        if (step == ElecExperimentStep.Connection2)
            return "STEP 6 — CONNECTION 2 (Parallel): Join both + terminals together and both − terminals together. Ammeter in the loop. Voltmeter across the bulb. Then CHECK CIRCUIT.";
        if (step == ElecExperimentStep.Connection3)
            return "STEP 7 — CONNECTION 3 (Series opposing): Reverse one cell with ↻ so like poles face each other. Ammeter in the loop. Voltmeter across the bulb. Then CHECK CIRCUIT.";
        return "STEP 5 — CONNECTION 1 (Series aiding): Drag the 5 parts onto the board. Line the cells [+][−] [+][−] and join Cell 1 (−) to Cell 2 (+). Ammeter in the loop. Voltmeter across the bulb. Then press CHECK CIRCUIT.";
    }

    private static string EducationCopy()
    {
        var calc = CircuitCalculationManager.Instance;
        float v = calc != null ? calc.CellVoltage : 1.5f;
        var r1 = ElecExperimentDataManager.Instance != null ? ElecExperimentDataManager.Instance.Get(1) : null;
        var r2 = ElecExperimentDataManager.Instance != null ? ElecExperimentDataManager.Instance.Get(2) : null;
        var r3 = ElecExperimentDataManager.Instance != null ? ElecExperimentDataManager.Instance.Get(3) : null;
        return
            "SERIES CONNECTION\n" +
            "When cells are connected in series aiding, their potential differences add.\n" +
            $"For two identical cells: Vtotal ≈ V1 + V2 ≈ {2f * v:0.0} V\n" +
            (r1 != null && r1.connectionNumber == 1 ? $"Your Connection 1 power P = VI = {r1.power:0.00} W\n\n" : "\n") +
            "PARALLEL CONNECTION\n" +
            "When identical cells are connected in parallel, the potential difference remains approximately equal to the potential difference of one cell, while the arrangement can provide greater capacity.\n" +
            (r2 != null && r2.connectionNumber == 2 ? $"Your Connection 2 power P = {r2.power:0.00} W\n\n" : "\n") +
            "SERIES OPPOSING\n" +
            "When equal cells are connected in series opposition, their potential differences oppose each other.\n" +
            "Vnet ≈ V1 − V2  therefore  Vnet ≈ 0 V for identical ideal cells.\n" +
            (r3 != null && r3.connectionNumber == 3 ? $"Your Connection 3 power P = {r3.power:0.00} W\n\n" : "\n") +
            "CURRENT\n" +
            "Current is the rate of flow of electric charge.\nI = Q / t\nFor the simplified circuit: I = V / R\n" +
            "If resistance is kept constant, increasing the potential difference increases the current.\n\n" +
            "POTENTIAL DIFFERENCE\n" +
            "Potential difference is the energy transferred per unit charge.\nV = W / Q\nUnit: Volt (V)\n\n" +
            "ELECTRICAL POWER\nP = VI\nUnit: Watt (W)\n\n" +
            "Simulation values are based on a simplified circuit model.";
    }

    private static string ConclusionCopy()
    {
        return
            "CONCLUSION\n\n" +
            "The potential difference and current in a circuit depend on how the cells are connected.\n\n" +
            "For identical cells connected in series aiding, the potential differences add.\n\n" +
            "For identical cells connected in parallel, the potential difference is approximately the same as the potential difference of one cell.\n\n" +
            "For identical cells connected in series opposition, the potential differences oppose each other and the net potential difference is approximately zero.\n\n" +
            "The ammeter must be connected in series, while the voltmeter must be connected in parallel across the component being measured.";
    }
}

public class ElecIntroClickToStart : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (ElecUIManager.Instance != null) ElecUIManager.Instance.StartPractical();
    }
}
