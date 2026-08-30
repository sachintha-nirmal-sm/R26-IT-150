using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElectronicsUIManager : MonoBehaviour
{
    public static ElectronicsUIManager Instance { get; private set; }

    private TextMeshProUGUI titleText, scoreText, progressText, attemptsText, stepText, instructionText;
    private Image progressBarFill;
    private GameObject introPanel, theoryPanel, instructionBar, equipmentPanel, laboratoryPanel;
    private GameObject observationPanel, comparePanel, matchPanel, challengePanel, questionPanel, conclusionPanel, resultPanel;
    private GameObject batteryTools, circuitTools;
    private TextMeshProUGUI introText, theoryText, conclusionPreview, compareText, questionText, observationTable;
    private TextMeshProUGUI finalScoreText, resultDetailsText, statusText, circuitStatus, matchProgress, bulbStatus;
    private Button startBtn, nextBtn, resetBtn, retryBtn, viewProfileBtn, viewResultsBtn, theoryContinue;
    private Button questionA, questionB, questionC, questionD, questionContinue;
    private Button obsGlow, obsDark;
    private Button disconnectBtn, reverseBtn, reconnectBtn, flipDiodeBtn, switchBtn;
    private Button challengeDiodeBtn, challengeBatteryBtn;
    private GameObject questionExplanationPanel, resetConfirmPanel, retryConfirmPanel, optionsGroup, diodeDiagram;
    private TextMeshProUGUI questionExplanationText;
    private Button resetYes, resetNo, retryYes, retryNo, conclusionContinueBtn;
    private Button[] phraseButtons;
    private Button[] matchLeft;
    private Button[] matchRight;

    private void Awake() => Instance = this;
    private void OnDestroy() { if (Instance == this) Instance = null; }

    public void BindAll(ElectronicsUIRefs refs, bool showWelcome)
    {
        titleText = refs.Title; scoreText = refs.Score; progressText = refs.Progress;
        attemptsText = refs.Attempts; stepText = refs.StepLabel;
        progressBarFill = refs.ProgressBar;
        instructionText = refs.Instruction;
        introPanel = refs.IntroPanel; theoryPanel = refs.TheoryPanel;
        instructionBar = refs.InstructionBar; equipmentPanel = refs.EquipmentPanel;
        laboratoryPanel = refs.LaboratoryPanel;
        observationPanel = refs.ObservationPanel; comparePanel = refs.ComparePanel;
        matchPanel = refs.MatchPanel; challengePanel = refs.ChallengePanel;
        questionPanel = refs.QuestionPanel; conclusionPanel = refs.ConclusionPanel; resultPanel = refs.ResultPanel;
        introText = refs.IntroText; theoryText = refs.TheoryText;
        conclusionPreview = refs.ConclusionPreview; compareText = refs.CompareText;
        questionText = refs.QuestionText; observationTable = refs.ObservationTable;
        finalScoreText = refs.FinalScore; resultDetailsText = refs.ResultDetails; statusText = refs.StatusText;
        circuitStatus = refs.CircuitStatus; matchProgress = refs.MatchProgress; bulbStatus = refs.BulbStatus;
        startBtn = refs.StartBtn; nextBtn = refs.Next; resetBtn = refs.Reset;
        retryBtn = refs.Retry; viewProfileBtn = refs.ViewProfileBtn; viewResultsBtn = refs.ViewResultsBtn;
        theoryContinue = refs.TheoryContinue;
        questionA = refs.QuestionA; questionB = refs.QuestionB; questionC = refs.QuestionC; questionD = refs.QuestionD;
        questionContinue = refs.QuestionContinue;
        obsGlow = refs.ObsGlow; obsDark = refs.ObsDark;
        disconnectBtn = refs.DisconnectBtn; reverseBtn = refs.ReverseBtn; reconnectBtn = refs.ReconnectBtn;
        flipDiodeBtn = refs.FlipDiodeBtn; switchBtn = refs.SwitchBtn;
        challengeDiodeBtn = refs.ChallengeDiodeBtn; challengeBatteryBtn = refs.ChallengeBatteryBtn;
        questionExplanationPanel = refs.QuestionExplanationPanel;
        questionExplanationText = refs.QuestionExplanationText;
        optionsGroup = refs.OptionsGroup; diodeDiagram = refs.DiodeDiagram;
        batteryTools = refs.BatteryTools; circuitTools = refs.CircuitTools;
        resetConfirmPanel = refs.ResetConfirm; resetYes = refs.ResetYes; resetNo = refs.ResetNo;
        retryConfirmPanel = refs.RetryConfirm; retryYes = refs.RetryYes; retryNo = refs.RetryNo;
        phraseButtons = refs.PhraseButtons;
        matchLeft = refs.MatchLeft; matchRight = refs.MatchRight;
        conclusionContinueBtn = refs.ConclusionContinue;
        WireButtons();
        if (showWelcome) ShowIntro();
    }

    private void WireButtons()
    {
        WireBtn(startBtn, StartPractical);
        WireBtn(theoryContinue, () => ElectronicsPracticalManager.Instance?.AdvanceStep());
        WireBtn(nextBtn, OnNextPressed);
        WireBtn(resetBtn, () => { if (resetConfirmPanel != null) resetConfirmPanel.SetActive(true); });
        WireBtn(resetYes, () =>
        {
            resetConfirmPanel?.SetActive(false);
            ElectronicsPracticalManager.Instance?.ResetEntirePractical();
            ShowIntro();
        });
        WireBtn(resetNo, () => resetConfirmPanel?.SetActive(false));
        WireBtn(retryBtn, () => { if (retryConfirmPanel != null) retryConfirmPanel.SetActive(true); });
        WireBtn(retryYes, () =>
        {
            retryConfirmPanel?.SetActive(false);
            ElectronicsPracticalManager.Instance?.RetryExperiment();
        });
        WireBtn(retryNo, () => retryConfirmPanel?.SetActive(false));
        WireBtn(viewProfileBtn, () =>
        {
            string summary = ElectronicsProfileManager.Instance != null ? ElectronicsProfileManager.Instance.GetProfileSummary() : "No profile data.";
            ElectronicsFeedbackManager.Instance?.ShowInstruction(summary);
        });
        WireBtn(viewResultsBtn, () => resultPanel?.SetActive(true));
        WireBtn(questionA, () => AnswerCurrent(0));
        WireBtn(questionB, () => AnswerCurrent(1));
        WireBtn(questionC, () => AnswerCurrent(2));
        WireBtn(questionD, () => AnswerCurrent(3));
        WireBtn(questionContinue, OnQuestionContinue);
        WireBtn(obsGlow, () => AnswerObservation(true));
        WireBtn(obsDark, () => AnswerObservation(false));
        WireBtn(disconnectBtn, () => ElectronicsPracticalManager.Instance?.DisconnectBattery());
        WireBtn(reverseBtn, () => ElectronicsPracticalManager.Instance?.ReverseBattery());
        WireBtn(reconnectBtn, () => ElectronicsPracticalManager.Instance?.ReconnectBattery());
        WireBtn(flipDiodeBtn, () => ElectronicsDiodeController.Instance?.FlipOrientation());
        WireBtn(switchBtn, () => ElectronicsSwitchController.Instance?.ToggleSwitch());
        WireBtn(challengeDiodeBtn, () => ElectronicsMiniChallengeManager.Instance?.SetDiodeCorrect());
        WireBtn(challengeBatteryBtn, () => ElectronicsMiniChallengeManager.Instance?.SetBatteryNormal());
        WireBtn(conclusionContinueBtn, () => ElectronicsPracticalManager.Instance?.CompleteConclusion());

        string[] leftIds = { "ForwardBias", "ReverseBias", "CorrectDirection", "OppositeDirection", "BulbGlows", "BulbDark" };
        if (matchLeft != null)
        {
            for (int i = 0; i < matchLeft.Length && i < leftIds.Length; i++)
            {
                int idx = i;
                WireBtn(matchLeft[i], () => ElectronicsFormulaMatchingManager.Instance?.TapLeft(leftIds[idx]));
            }
        }
        string[] rightIds = { "MatchFlow", "MatchBlocked", "MatchForward", "MatchReverse" };
        if (matchRight != null)
        {
            for (int i = 0; i < matchRight.Length && i < rightIds.Length; i++)
            {
                int idx = i;
                WireBtn(matchRight[i], () => ElectronicsFormulaMatchingManager.Instance?.TapRight(rightIds[idx]));
            }
        }
        if (phraseButtons != null)
        {
            for (int i = 0; i < phraseButtons.Length; i++)
            {
                int idx = i;
                WireBtn(phraseButtons[i], () =>
                {
                    var tmp = phraseButtons[idx] != null ? phraseButtons[idx].GetComponentInChildren<TextMeshProUGUI>() : null;
                    if (tmp != null && !string.IsNullOrEmpty(tmp.text))
                        ElectronicsConclusionManager.Instance?.AddPhrase(tmp.text);
                });
            }
        }
    }

    private void WireBtn(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    public void StartPractical() => ElectronicsPracticalManager.Instance?.StartPractical();

    private void OnNextPressed()
    {
        var step = Current();
        if (step == ElectronicsPracticalStep.Introduction || step == ElectronicsPracticalStep.Theory)
        {
            ElectronicsPracticalManager.Instance?.AdvanceStep();
            return;
        }
        if (step == ElectronicsPracticalStep.EquipmentSelection)
        {
            ElectronicsPracticalManager.Instance?.TryAdvanceFromEquipment();
            return;
        }
        if (step == ElectronicsPracticalStep.Comparison || step == ElectronicsPracticalStep.Questions)
        {
            OnQuestionContinue();
            return;
        }
        if (step == ElectronicsPracticalStep.Conclusion)
        {
            ElectronicsPracticalManager.Instance?.CompleteConclusion();
            return;
        }
        ElectronicsPracticalManager.Instance?.AdvanceStep();
    }

    private void AnswerCurrent(int choice)
    {
        var step = Current();
        if (step == ElectronicsPracticalStep.Comparison)
            ElectronicsComparisonManager.Instance?.Answer(choice);
        else
            ElectronicsQuestionManager.Instance?.Answer(choice);
    }

    private void AnswerObservation(bool glow)
    {
        var step = Current();
        if (step == ElectronicsPracticalStep.ForwardObservation)
            ElectronicsObservationManager.Instance?.AnswerForward(glow);
        else if (step == ElectronicsPracticalStep.ReverseObservation)
            ElectronicsObservationManager.Instance?.AnswerReverse(glow);
    }

    private void OnQuestionContinue()
    {
        var step = Current();
        if (step == ElectronicsPracticalStep.Comparison)
        {
            if (ElectronicsComparisonManager.Instance != null && ElectronicsComparisonManager.Instance.Advance())
                ElectronicsPracticalManager.Instance?.AdvanceStep();
            return;
        }
        if (ElectronicsQuestionManager.Instance != null && ElectronicsQuestionManager.Instance.Advance())
            ElectronicsPracticalManager.Instance?.CompleteQuestions();
    }

    private static ElectronicsPracticalStep Current()
    {
        return ElectronicsPracticalManager.Instance != null
            ? ElectronicsPracticalManager.Instance.CurrentStep
            : ElectronicsPracticalStep.Introduction;
    }

    public void ShowIntro()
    {
        HideAll();
        introPanel?.SetActive(true);
        SetInstruction("In this practical you will investigate how the direction of a diode affects current flow in a simple circuit.");
        SetStepLabel("Introduction");
        SetNextButtonVisible(true);
        if (retryBtn != null) retryBtn.gameObject.SetActive(false);
        ElectronicsProgressManager.Instance?.SetFromPracticalStep(ElectronicsPracticalStep.Introduction);
    }

    public void ShowStep(ElectronicsPracticalStep step)
    {
        HideAll();
        instructionBar?.SetActive(true);
        SetStepLabel(step.ToString());
        ElectronicsProgressManager.Instance?.SetFromPracticalStep(step);
        switch (step)
        {
            case ElectronicsPracticalStep.Introduction:
                introPanel?.SetActive(true);
                SetInstruction("ELECTRONICS — Investigation of Forward Bias and Reverse Bias of a Diode.");
                break;
            case ElectronicsPracticalStep.Theory:
                theoryPanel?.SetActive(true);
                SetInstruction("Read the theory, then press CONTINUE.");
                break;
            case ElectronicsPracticalStep.EquipmentSelection:
                equipmentPanel?.SetActive(true);
                ElectronicsEquipmentSelectionManager.Instance?.EnsureCardsVisible();
                SetInstruction("Select all the equipment required for this practical. Drag items into REQUIRED APPARATUS.");
                break;
            case ElectronicsPracticalStep.CircuitSetup:
                laboratoryPanel?.SetActive(true);
                SetTools(true, false);
                SetInstruction("Select the correct equipment and build the circuit. Drag each part onto the breadboard.");
                break;
            case ElectronicsPracticalStep.ForwardBias:
                laboratoryPanel?.SetActive(true);
                SetTools(true, false);
                SetInstruction("EXPERIMENT 1 — FORWARD BIAS. Check diode direction, then turn ON the switch.");
                break;
            case ElectronicsPracticalStep.ForwardObservation:
                observationPanel?.SetActive(true);
                laboratoryPanel?.SetActive(true);
                ElectronicsObservationManager.Instance?.StartForwardObservation();
                SetInstruction("Record Observation 1 for the forward-bias circuit.");
                break;
            case ElectronicsPracticalStep.BatteryDisconnect:
                laboratoryPanel?.SetActive(true);
                SetTools(true, true);
                SetInstruction("Turn OFF the switch. Disconnect ONLY the battery. Do not rebuild the rest of the circuit.");
                break;
            case ElectronicsPracticalStep.BatteryReverse:
                laboratoryPanel?.SetActive(true);
                SetTools(true, true);
                SetInstruction("Rotate the battery 180° and reconnect it in the opposite direction.");
                break;
            case ElectronicsPracticalStep.ReverseBias:
                laboratoryPanel?.SetActive(true);
                SetTools(true, false);
                SetInstruction("EXPERIMENT 2 — REVERSE BIAS. Turn ON the switch and observe the bulb.");
                break;
            case ElectronicsPracticalStep.ReverseObservation:
                observationPanel?.SetActive(true);
                laboratoryPanel?.SetActive(true);
                ElectronicsObservationManager.Instance?.StartReverseObservation();
                SetInstruction("Record Observation 2 for the reverse-bias circuit.");
                break;
            case ElectronicsPracticalStep.Comparison:
                comparePanel?.SetActive(true);
                questionPanel?.SetActive(true);
                SetQuestionPanelWide(false);
                SetInstruction("Complete the forward vs reverse comparison.");
                break;
            case ElectronicsPracticalStep.Matching:
                matchPanel?.SetActive(true);
                SetInstruction("Match each idea to the correct meaning.");
                break;
            case ElectronicsPracticalStep.Challenge:
                challengePanel?.SetActive(true);
                laboratoryPanel?.SetActive(true);
                SetTools(true, false);
                SetInstruction("MAKE THE BULB GLOW — create a forward-biased circuit.");
                break;
            case ElectronicsPracticalStep.Questions:
                questionPanel?.SetActive(true);
                SetQuestionPanelWide(true);
                SetInstruction("Answer the questions.");
                break;
            case ElectronicsPracticalStep.Conclusion:
                conclusionPanel?.SetActive(true);
                SetInstruction("Arrange the conclusion statements in the correct order.");
                break;
            case ElectronicsPracticalStep.Result:
                resultPanel?.SetActive(true);
                if (retryBtn != null) retryBtn.gameObject.SetActive(true);
                SetInstruction("Practical completed. View results or retry.");
                break;
        }
    }

    private void HideAll()
    {
        introPanel?.SetActive(false);
        theoryPanel?.SetActive(false);
        equipmentPanel?.SetActive(false);
        laboratoryPanel?.SetActive(false);
        observationPanel?.SetActive(false);
        comparePanel?.SetActive(false);
        matchPanel?.SetActive(false);
        challengePanel?.SetActive(false);
        questionPanel?.SetActive(false);
        conclusionPanel?.SetActive(false);
        resultPanel?.SetActive(false);
        questionExplanationPanel?.SetActive(false);
        SetTools(false, false);
        SetDiodeDiagramVisible(false);
    }

    private void SetTools(bool circuit, bool battery)
    {
        if (circuitTools != null) circuitTools.SetActive(circuit);
        if (batteryTools != null) batteryTools.SetActive(battery);
    }

    public void SetInstruction(string text)
    {
        if (instructionText != null) instructionText.text = text;
        instructionBar?.SetActive(true);
    }

    public void SetStepLabel(string text)
    {
        if (stepText != null) stepText.text = text;
    }

    public void UpdateScoreDisplay(int score)
    {
        if (scoreText != null) scoreText.text = $"SCORE: {score}/100";
    }

    public void UpdateAttemptsDisplay(int current, int max)
    {
        if (attemptsText != null) attemptsText.text = $"ATTEMPT: {current}/{max}";
    }

    public void UpdateProgressDisplay(int step, int total, float fill)
    {
        if (progressText != null) progressText.text = $"PROGRESS  Step {step}/{total}";
        if (progressBarFill != null) progressBarFill.fillAmount = fill;
    }

    public void UpdateCircuitStatus(string text)
    {
        if (circuitStatus != null) circuitStatus.text = text;
        if (bulbStatus != null && ElectronicsBulbController.Instance != null)
            bulbStatus.text = ElectronicsBulbController.Instance.IsGlowing ? "BULB STATUS: GLOWING" : "BULB STATUS: NOT GLOWING";
    }

    public void UpdateMatchProgress(int done, int total)
    {
        if (matchProgress != null) matchProgress.text = $"Matched {done}/{total}";
    }

    public void SetNextButtonVisible(bool visible)
    {
        if (nextBtn != null) nextBtn.gameObject.SetActive(visible);
    }

    public void ShowQuestion(int number, int total, string prompt, string a, string b, string c, string d)
    {
        questionPanel?.SetActive(true);
        optionsGroup?.SetActive(true);
        questionExplanationPanel?.SetActive(false);
        if (questionText != null)
            questionText.text = $"QUESTION {number} / {total}\n\n{prompt}";
        SetOpt(questionA, a);
        SetOpt(questionB, b);
        SetOpt(questionC, c);
        SetOpt(questionD, d);
        SetQuestionOptionsVisible(string.IsNullOrEmpty(c) ? 2 : 4);
    }

    public void SetQuestionOptionsVisible(int count)
    {
        if (questionA != null) questionA.gameObject.SetActive(count >= 1);
        if (questionB != null) questionB.gameObject.SetActive(count >= 2);
        if (questionC != null) questionC.gameObject.SetActive(count >= 3);
        if (questionD != null) questionD.gameObject.SetActive(count >= 4);
    }

    public void ShowQuestionExplanation(string explanation, bool correct)
    {
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(true);
        if (questionExplanationText != null)
        {
            questionExplanationText.text = (correct ? "✓  " : "✗  ") + explanation;
            questionExplanationText.color = correct ? new Color(0.08f, 0.45f, 0.22f) : new Color(0.65f, 0.12f, 0.12f);
        }
    }

    public void ShowObservationChoices(string title, string prompt, string a, string b)
    {
        observationPanel?.SetActive(true);
        if (observationTable != null)
            observationTable.text = title + "\n\n" + prompt;
        SetOpt(obsGlow, a);
        SetOpt(obsDark, b);
    }

    public void SetCompareTable(string text)
    {
        if (compareText != null) compareText.text = text;
    }

    public void HighlightCompareRow(int rowIndex)
    {
        if (comparePanel == null) return;
        var table = comparePanel.transform.Find("Table");
        if (table == null) return;
        for (int i = 0; i < 3; i++)
        {
            var row = table.Find("Row" + i);
            if (row == null) continue;
            var img = row.GetComponent<Image>();
            if (img == null) continue;
            bool on = i == rowIndex;
            if (i == 0) img.color = on ? new Color(0.72f, 0.88f, 1f) : new Color(0.86f, 0.93f, 0.98f);
            if (i == 1) img.color = on ? new Color(0.70f, 0.90f, 0.72f) : new Color(0.90f, 0.95f, 0.90f);
            if (i == 2) img.color = on ? new Color(1f, 0.90f, 0.55f) : new Color(1f, 0.96f, 0.82f);
        }
    }

    public void SetQuestionPanelWide(bool wide)
    {
        if (questionPanel == null) return;
        var rt = questionPanel.GetComponent<RectTransform>();
        if (rt == null) return;
        bool narrow = Screen.width < 900 || Screen.width < Screen.height * 0.95f;
        if (wide)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(12, 8);
            rt.offsetMax = new Vector2(-12, -8);
        }
        else if (narrow)
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0.48f);
            rt.offsetMin = new Vector2(8, 6);
            rt.offsetMax = new Vector2(-8, -4);
            if (comparePanel != null)
            {
                var crt = comparePanel.GetComponent<RectTransform>();
                if (crt != null)
                {
                    crt.anchorMin = new Vector2(0f, 0.50f);
                    crt.anchorMax = Vector2.one;
                    crt.offsetMin = new Vector2(8, 4);
                    crt.offsetMax = new Vector2(-8, -6);
                }
            }
        }
        else
        {
            rt.anchorMin = new Vector2(0.47f, 0f);
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(12, 12);
            rt.offsetMax = new Vector2(-16, -12);
        }
    }

    public void SetConclusionPreview(string text)
    {
        if (conclusionPreview != null) conclusionPreview.text = text;
    }

    public void SetDiodeDiagramVisible(bool visible)
    {
        if (diodeDiagram != null) diodeDiagram.SetActive(visible);
    }

    public void HideResult() => resultPanel?.SetActive(false);
    public void ShowResult()
    {
        HideAll();
        resultPanel?.SetActive(true);
        if (retryBtn != null) retryBtn.gameObject.SetActive(true);
    }

    private static void SetOpt(Button btn, string text)
    {
        if (btn == null) return;
        bool show = !string.IsNullOrEmpty(text);
        btn.gameObject.SetActive(show);
        if (!show) return;

        var body = btn.transform.Find("Body")?.GetComponent<TextMeshProUGUI>();
        if (body != null)
        {
            body.text = text;
            return;
        }

        var tmps = btn.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < tmps.Length; i++)
        {
            if (tmps[i] == null) continue;
            if (tmps[i].name == "LetterText" || tmps[i].name == "Letter") continue;
            tmps[i].text = text;
            return;
        }
    }
}
