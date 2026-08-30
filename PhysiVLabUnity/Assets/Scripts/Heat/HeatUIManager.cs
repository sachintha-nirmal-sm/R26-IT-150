using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeatUIManager : MonoBehaviour
{
    public static HeatUIManager Instance { get; private set; }

    private TextMeshProUGUI titleText, scoreText, progressText, attemptsText, stepText, instructionText;
    private Image progressBarFill;
    private GameObject introPanel, objectivePanel, instructionBar, equipmentPanel, laboratoryPanel;
    private GameObject dataTablePanel, comparePanel, questionPanel, conclusionPanel, resultPanel, variablePanel;
    private TextMeshProUGUI introText, objectiveText, conclusionText, compareText, questionText, dataTableText;
    private TextMeshProUGUI finalScoreText, resultDetailsText, statusText, liveReadingsText, physicsText, conclusionPreview;
    private Button startBtn, nextBtn, resetBtn, retryBtn, viewProfileBtn, viewResultsBtn;
    private Button confirmSetupBtn, markABtn, heatBtn, levelsBtn;
    private Button questionA, questionB, questionC, questionD, questionContinue;
    private Button compareA, compareB, compareC;
    private GameObject questionExplanationPanel, resetConfirmPanel, retryConfirmPanel, optionsGroup;
    private TextMeshProUGUI questionExplanationText, optAText, optBText, optCText, optDText;
    private Button resetYes, resetNo, retryYes, retryNo;
    private Button[] phraseButtons;
    private Button variableContinueBtn, conclusionContinueBtn;

    private void Awake() => Instance = this;

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void BindAll(HeatUIRefs refs, bool showWelcome)
    {
        titleText = refs.Title; scoreText = refs.Score; progressText = refs.Progress;
        attemptsText = refs.Attempts; stepText = refs.StepLabel;
        progressBarFill = refs.ProgressBar;
        instructionText = refs.Instruction;
        introPanel = refs.IntroPanel; objectivePanel = refs.ObjectivePanel;
        instructionBar = refs.InstructionBar; equipmentPanel = refs.EquipmentPanel;
        laboratoryPanel = refs.LaboratoryPanel;
        dataTablePanel = refs.DataTablePanel; comparePanel = refs.ComparePanel;
        questionPanel = refs.QuestionPanel;
        conclusionPanel = refs.ConclusionPanel; resultPanel = refs.ResultPanel;
        variablePanel = refs.VariablePanel;
        introText = refs.IntroText; objectiveText = refs.ObjectiveText;
        conclusionText = refs.ConclusionText; compareText = refs.CompareText;
        questionText = refs.QuestionText; dataTableText = refs.DataTableText;
        finalScoreText = refs.FinalScore; resultDetailsText = refs.ResultDetails; statusText = refs.StatusText;
        liveReadingsText = refs.LiveReadings; physicsText = refs.PhysicsText;
        conclusionPreview = refs.ConclusionPreview;
        startBtn = refs.StartBtn; nextBtn = refs.Next; resetBtn = refs.Reset;
        retryBtn = refs.Retry; viewProfileBtn = refs.ViewProfileBtn; viewResultsBtn = refs.ViewResultsBtn;
        confirmSetupBtn = refs.ConfirmSetupBtn;
        markABtn = refs.MarkABtn; heatBtn = refs.HeatBtn; levelsBtn = refs.LevelsBtn;
        questionA = refs.QuestionA; questionB = refs.QuestionB; questionC = refs.QuestionC; questionD = refs.QuestionD;
        questionContinue = refs.QuestionContinue;
        compareA = refs.CompareA; compareB = refs.CompareB; compareC = refs.CompareC;
        questionExplanationPanel = refs.QuestionExplanationPanel;
        questionExplanationText = refs.QuestionExplanationText;
        optAText = refs.OptAText; optBText = refs.OptBText; optCText = refs.OptCText; optDText = refs.OptDText;
        optionsGroup = refs.OptionsGroup;
        resetConfirmPanel = refs.ResetConfirm; resetYes = refs.ResetYes; resetNo = refs.ResetNo;
        retryConfirmPanel = refs.RetryConfirm; retryYes = refs.RetryYes; retryNo = refs.RetryNo;
        phraseButtons = refs.PhraseButtons;
        variableContinueBtn = refs.VariableContinue;
        conclusionContinueBtn = refs.ConclusionContinue;
        WireButtons();
        if (showWelcome) ShowIntro();
    }

    private void WireButtons()
    {
        WireBtn(startBtn, StartPractical);
        WireBtn(nextBtn, OnNextPressed);
        WireBtn(resetBtn, () => { if (resetConfirmPanel != null) resetConfirmPanel.SetActive(true); });
        WireBtn(resetYes, () =>
        {
            resetConfirmPanel?.SetActive(false);
            HeatExperimentManager.Instance?.ResetExperiment();
            ShowIntro();
        });
        WireBtn(resetNo, () => resetConfirmPanel?.SetActive(false));
        WireBtn(retryBtn, () => { if (retryConfirmPanel != null) retryConfirmPanel.SetActive(true); });
        WireBtn(retryYes, () =>
        {
            retryConfirmPanel?.SetActive(false);
            HeatExperimentManager.Instance?.RetryExperiment();
        });
        WireBtn(retryNo, () => retryConfirmPanel?.SetActive(false));
        WireBtn(viewProfileBtn, () =>
        {
            string summary = HeatProfileManager.Instance != null ? HeatProfileManager.Instance.GetProfileSummary() : "No profile data.";
            HeatFeedbackManager.Instance?.ShowInstruction(summary);
        });
        WireBtn(viewResultsBtn, () => resultPanel?.SetActive(true));
        WireBtn(confirmSetupBtn, () => HeatExperimentManager.Instance?.ConfirmSetup());
        WireBtn(markABtn, () => HeatExperimentManager.Instance?.MarkLevelA());
        WireBtn(heatBtn, () => HeatExperimentManager.Instance?.StartHeating());
        WireBtn(levelsBtn, () => HeatExperimentManager.Instance?.ConfirmLevelsObserved());
        WireBtn(questionA, () => HeatQuestionManager.Instance?.Answer(0));
        WireBtn(questionB, () => HeatQuestionManager.Instance?.Answer(1));
        WireBtn(questionC, () => HeatQuestionManager.Instance?.Answer(2));
        WireBtn(questionD, () => HeatQuestionManager.Instance?.Answer(3));
        WireBtn(questionContinue, OnQuestionContinue);
        WireBtn(compareA, () => HeatExperimentManager.Instance?.AnswerLevels(0));
        WireBtn(compareB, () => HeatExperimentManager.Instance?.AnswerLevels(1));
        WireBtn(compareC, () => HeatExperimentManager.Instance?.AnswerLevels(2));
        WireBtn(variableContinueBtn, () => HeatExperimentManager.Instance?.CompleteVariableMatching());
        WireBtn(conclusionContinueBtn, () => HeatExperimentManager.Instance?.CompleteConclusion());
        if (phraseButtons != null)
        {
            for (int i = 0; i < phraseButtons.Length; i++)
            {
                int idx = i;
                WireBtn(phraseButtons[i], () =>
                {
                    var tmp = phraseButtons[idx] != null ? phraseButtons[idx].GetComponentInChildren<TextMeshProUGUI>() : null;
                    if (tmp != null && !string.IsNullOrEmpty(tmp.text))
                        HeatConclusionManager.Instance?.AddPhrase(tmp.text);
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

    public void StartPractical() => HeatExperimentManager.Instance?.StartPractical();

    private void OnNextPressed()
    {
        var step = Current();
        if (step == HeatExperimentStep.Introduction) { StartPractical(); return; }
        if (step == HeatExperimentStep.SelectEquipment)
        {
            HeatExperimentManager.Instance?.TryAdvanceFromEquipment();
            return;
        }
        if (step == HeatExperimentStep.Questions)
        {
            OnQuestionContinue();
            return;
        }
        if (step == HeatExperimentStep.VariableMatching)
        {
            HeatExperimentManager.Instance?.CompleteVariableMatching();
            return;
        }
        if (step == HeatExperimentStep.Conclusion)
        {
            HeatExperimentManager.Instance?.CompleteConclusion();
            return;
        }
        HeatExperimentManager.Instance?.AdvanceStep();
    }

    private void OnQuestionContinue()
    {
        if (HeatQuestionManager.Instance == null)
        {
            HeatExperimentManager.Instance?.CompleteQuestions();
            return;
        }
        if (HeatQuestionManager.Instance.IsFinished)
        {
            HeatExperimentManager.Instance?.CompleteQuestions();
            return;
        }
        if (!HeatQuestionManager.Instance.HasAnswered)
        {
            HeatFeedbackManager.Instance?.ShowInstruction("Select A, B, C or D first. Then press CONTINUE.");
            return;
        }
        bool done = HeatQuestionManager.Instance.Advance();
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(false);
        if (done) HeatExperimentManager.Instance?.CompleteQuestions();
    }

    public void ShowIntro()
    {
        HideAllContent();
        if (introPanel != null) introPanel.SetActive(true);
        if (instructionBar != null) instructionBar.SetActive(false);
        SetNextButtonVisible(false);
        if (introText != null)
        {
            introText.text =
                "HEAT\n\n" +
                "Figure 9.22 — Illustrating expansion of liquids\n\n" +
                "Fill a test tube with coloured water. Fit a rubber stopper with a thin glass tube through it. The liquid rises a little into the thin tube. Mark this height as A.\n\n" +
                "Hold the test tube in a beaker of water and heat the beaker. The liquid first falls slightly to B, then rises past A to C.\n\n" +
                "The idea you will test is:\n" +
                "Both the glass container and the liquid expand when heated, but the liquid expands more. The brief fall to B happens because the glass expands first.";
        }
        UpdateAttemptsDisplay(HeatAttemptManager.Instance != null ? HeatAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateScoreDisplay(0);
        UpdateProgress(1, 11);
        if (stepText != null) stepText.text = "Introduction";
        if (titleText != null)
            titleText.text = "HEAT     Expansion of liquids";
    }

    public void ShowStep(HeatExperimentStep step)
    {
        HideAllContent();
        if (instructionBar != null) instructionBar.SetActive(step != HeatExperimentStep.Introduction && step != HeatExperimentStep.Complete);
        SetNextButtonVisible(step != HeatExperimentStep.Assembly && step != HeatExperimentStep.HeatObserve && step != HeatExperimentStep.IdentifyLevels);
        SetLabControls(false);

        switch (step)
        {
            case HeatExperimentStep.Introduction:
                ShowIntro();
                return;
            case HeatExperimentStep.Objective:
                objectivePanel?.SetActive(true);
                if (objectiveText != null)
                    objectiveText.text =
                        "OBJECTIVES\n\nBy completing this practical you will:\n\n" +
                        "• Identify the apparatus needed to show expansion of a liquid.\n" +
                        "• Assemble a test tube of coloured water with a stopper and thin glass tube, and mark level A.\n" +
                        "• Heat the test tube in a water bath.\n" +
                        "• Observe the liquid fall from A to B, then rise to C.\n" +
                        "• Explain that the glass expands first, then the liquid expands more than the glass.";
                SetInstruction("Read the objectives, then press CONTINUE.");
                SetNextLabel("CONTINUE");
                break;
            case HeatExperimentStep.SelectEquipment:
                equipmentPanel?.SetActive(true);
                HeatEquipmentSelectionManager.Instance?.EnsureCardsVisible();
                SetInstruction("Select all equipment required for this practical. Tap or drag items into the REQUIRED EQUIPMENT AREA.");
                break;
            case HeatExperimentStep.Assembly:
                laboratoryPanel?.SetActive(true);
                SetLabControls(true);
                SetInstruction(HeatAssemblyManager.Instance != null ? HeatAssemblyManager.Instance.NextHint() : "Assemble the apparatus.");
                break;
            case HeatExperimentStep.HeatObserve:
                laboratoryPanel?.SetActive(true);
                SetLabControls(true);
                SetInstruction("Press START HEATING. Watch the liquid fall from A to B, then rise to C. Then press LEVELS REACHED C.");
                SetNextButtonVisible(false);
                break;
            case HeatExperimentStep.IdentifyLevels:
                comparePanel?.SetActive(true);
                FillCompare();
                SetInstruction("Why did the liquid first fall from A to B, then rise to C?");
                SetNextButtonVisible(false);
                break;
            case HeatExperimentStep.ObservationTable:
                dataTablePanel?.SetActive(true);
                HeatObservationTableManager.Instance?.Refresh();
                SetInstruction("Study the table. Glass expands first (A→B). The liquid then expands more than glass (B→C).");
                break;
            case HeatExperimentStep.Questions:
                questionPanel?.SetActive(true);
                SetNextButtonVisible(true);
                SetNextLabel("CONTINUE");
                SetInstruction("Select an answer, then press CONTINUE.");
                break;
            case HeatExperimentStep.VariableMatching:
                variablePanel?.SetActive(true);
                SetInstruction("Tap Independent, Dependent or Controlled for each quantity. Then press NEXT STEP.");
                SetNextButtonVisible(true);
                SetNextLabel("NEXT STEP");
                if (variableContinueBtn != null) variableContinueBtn.gameObject.SetActive(true);
                break;
            case HeatExperimentStep.Conclusion:
                conclusionPanel?.SetActive(true);
                SetInstruction("Tap the phrases in the correct order, then press NEXT STEP.");
                SetNextButtonVisible(true);
                SetNextLabel("NEXT STEP");
                if (conclusionContinueBtn != null) conclusionContinueBtn.gameObject.SetActive(true);
                break;
            case HeatExperimentStep.Complete:
                ShowResult();
                break;
        }

        UpdateProgress((int)step + 1, 11);
        if (stepText != null) stepText.text = StepTitle(step);
        if (step != HeatExperimentStep.Objective) SetNextLabel("NEXT STEP");
        UpdateLiveReadings();
    }

    private void FillCompare()
    {
        if (compareText == null) return;
        compareText.text =
            "IDENTIFY LEVELS A, B AND C\n\n" +
            "A is the starting height of the coloured liquid in the thin tube.\n" +
            "When the beaker is heated, the level first falls a little to B, then rises past A to C.\n\n" +
            "Why does this happen?\n\n" +
            "A. The liquid leaked, then more liquid was added.\n" +
            "B. The glass expands first (level falls A→B). Then the liquid expands more than the glass (level rises to C).\n" +
            "C. Liquids contract when heated, then the glass shrinks.";
    }

    public void ShowQuestion(int number, int total, string prompt, string a, string b, string c, string d)
    {
        questionPanel?.SetActive(true);
        optionsGroup?.SetActive(true);
        if (questionText != null) questionText.text = $"QUESTION {number} / {total}\n\n{prompt}";
        if (optAText != null) optAText.text = a;
        if (optBText != null) optBText.text = b;
        if (optCText != null) optCText.text = c;
        if (optDText != null) optDText.text = d;
        questionExplanationPanel?.SetActive(false);
        if (questionContinue != null)
        {
            questionContinue.gameObject.SetActive(true);
            questionContinue.interactable = true;
            questionContinue.transform.SetAsLastSibling();
        }
        SetNextButtonVisible(true);
        SetNextLabel("CONTINUE");
    }

    public void ShowQuestionExplanation(string explanation, bool correct)
    {
        if (questionExplanationPanel != null)
        {
            questionExplanationPanel.SetActive(true);
            questionExplanationPanel.transform.SetAsLastSibling();
        }
        if (questionContinue != null)
        {
            questionContinue.gameObject.SetActive(true);
            questionContinue.transform.SetAsLastSibling();
            questionContinue.interactable = true;
        }
        SetNextButtonVisible(true);
        SetNextLabel("CONTINUE");
        if (questionExplanationText != null)
        {
            questionExplanationText.text = (correct ? "✓ CORRECT\n" : "✗ INCORRECT\n") + explanation + "\n\nPress CONTINUE.";
            questionExplanationText.color = correct ? new Color(0.08f, 0.52f, 0.22f) : new Color(0.75f, 0.12f, 0.12f);
        }
    }

    public void SetConclusionPreview(string text)
    {
        if (conclusionPreview != null) conclusionPreview.text = text;
        if (conclusionText != null)
            conclusionText.text =
                "CONCLUSION\n\nTap the phrases in order:\n\n" +
                "When heat is applied, the glass container expands first,\n" +
                "so the liquid level falls slightly from A to B.\n" +
                "Then the liquid expands more than the glass,\n" +
                "so the level rises from B, past A, to C.\n\n" +
                "Your sentence:\n" + text;
    }

    public void UpdateLiveReadings()
    {
        var vis = HeatVisualController.Instance;
        var asm = HeatAssemblyManager.Instance;
        if (liveReadingsText != null)
        {
            liveReadingsText.text =
                $"Test tube:  {(asm != null && asm.TestTubePlaced ? "placed" : "—")}\n" +
                $"Coloured water:  {(asm != null && asm.WaterFilled ? "filled" : "—")}\n" +
                $"Stopper + thin tube:  {(asm != null && asm.ThinTubePlaced ? "fitted" : "—")}\n" +
                $"Level A:  {(asm != null && asm.LevelAMarked ? "marked" : "—")}\n" +
                $"Water bath:  {(asm != null && asm.BeakerPlaced ? "beaker on tripod" : "—")}\n" +
                $"Burner:  {(asm != null && asm.BurnerPlaced ? "under tripod" : "—")}\n" +
                $"Clamp:  {(asm != null && asm.StandPlaced ? "tube in bath" : "—")}\n" +
                $"Heating:  {(vis != null && vis.IsHeating ? "on" : vis != null && vis.ReachedLevelC ? "done" : "off")}\n\n" +
                (vis != null && vis.ReachedLevelC
                    ? "A → B (glass first) → C (liquid expands more)"
                    : vis != null && vis.ReachedLevelB
                        ? "Level at B — liquid will now expand."
                        : "Mark A, then heat and watch the thin tube.");
        }
        if (physicsText != null)
        {
            physicsText.text =
                "GLASS EXPANDS FIRST\nHeat reaches the test tube wall before the liquid. Volume of the container increases → level falls A to B.\n\n" +
                "LIQUID EXPANDS MORE\nLiquids expand more than solids for the same temperature rise → level rises to C.\n\n" +
                "THIN TUBE\nA small volume change becomes a large, visible height change.";
        }
        if (titleText != null)
            titleText.text = $"HEAT     Attempt: {(HeatAttemptManager.Instance != null ? HeatAttemptManager.Instance.CurrentAttemptNumber : 1)}/3";
        if (stepText != null)
        {
            var cur = Current();
            if (cur == HeatExperimentStep.Assembly) stepText.text = "Assembly";
            else if (cur == HeatExperimentStep.HeatObserve) stepText.text = "Heat and observe";
        }
    }

    public void UpdateScoreDisplay(int score)
    {
        if (scoreText != null) scoreText.text = $"SCORE: {score}/100";
    }

    public void UpdateAttemptsDisplay(int remaining)
    {
        if (attemptsText != null) attemptsText.text = $"ATTEMPTS REMAINING: {remaining}";
    }

    public void UpdateProgress(int current, int total)
    {
        if (progressText != null) progressText.text = $"Step: {current} / {total}";
        if (progressBarFill != null) progressBarFill.fillAmount = total > 0 ? current / (float)total : 0f;
    }

    public void SetInstruction(string text)
    {
        if (instructionText != null) instructionText.text = text;
    }

    public void SetNextButtonVisible(bool visible)
    {
        if (nextBtn != null) nextBtn.gameObject.SetActive(visible);
    }

    private void SetNextLabel(string label)
    {
        if (nextBtn == null) return;
        var tmp = nextBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = label;
    }

    public void ShowResult()
    {
        HideAllContent();
        resultPanel?.SetActive(true);
        if (retryBtn != null) retryBtn.gameObject.SetActive(true);
        SetNextButtonVisible(false);
        if (instructionBar != null) instructionBar.SetActive(false);
    }

    public void HideResult()
    {
        resultPanel?.SetActive(false);
        if (retryBtn != null) retryBtn.gameObject.SetActive(false);
    }

    private void SetLabControls(bool visible)
    {
        bool assembly = Current() == HeatExperimentStep.Assembly;
        bool heat = Current() == HeatExperimentStep.HeatObserve;
        if (confirmSetupBtn != null) confirmSetupBtn.gameObject.SetActive(visible && assembly);
        if (markABtn != null) markABtn.gameObject.SetActive(visible && assembly);
        if (heatBtn != null) heatBtn.gameObject.SetActive(visible && heat);
        if (levelsBtn != null) levelsBtn.gameObject.SetActive(visible && heat);
    }

    private void HideAllContent()
    {
        introPanel?.SetActive(false);
        objectivePanel?.SetActive(false);
        equipmentPanel?.SetActive(false);
        laboratoryPanel?.SetActive(false);
        dataTablePanel?.SetActive(false);
        comparePanel?.SetActive(false);
        questionPanel?.SetActive(false);
        conclusionPanel?.SetActive(false);
        resultPanel?.SetActive(false);
        variablePanel?.SetActive(false);
        if (retryBtn != null) retryBtn.gameObject.SetActive(false);
    }

    private static string StepTitle(HeatExperimentStep step)
    {
        switch (step)
        {
            case HeatExperimentStep.Objective: return "Objectives";
            case HeatExperimentStep.SelectEquipment: return "Equipment";
            case HeatExperimentStep.Assembly: return "Assembly";
            case HeatExperimentStep.HeatObserve: return "Heat and observe";
            case HeatExperimentStep.IdentifyLevels: return "Identify levels";
            case HeatExperimentStep.ObservationTable: return "Observations";
            case HeatExperimentStep.Questions: return "Questions";
            case HeatExperimentStep.VariableMatching: return "Variables";
            case HeatExperimentStep.Conclusion: return "Conclusion";
            case HeatExperimentStep.Complete: return "Results";
            default: return "Introduction";
        }
    }

    private static HeatExperimentStep Current()
    {
        return HeatExperimentManager.Instance != null
            ? HeatExperimentManager.Instance.CurrentStep
            : HeatExperimentStep.Introduction;
    }
}

public class HeatIntroClickToStart : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (HeatUIManager.Instance != null) HeatUIManager.Instance.StartPractical();
    }
}
