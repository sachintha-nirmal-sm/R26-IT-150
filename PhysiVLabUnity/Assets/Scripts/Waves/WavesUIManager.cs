using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WavesUIManager : MonoBehaviour
{
    public static WavesUIManager Instance { get; private set; }

    private TextMeshProUGUI titleText, scoreText, progressText, attemptsText, stepText, instructionText;
    private Image progressBarFill;
    private GameObject introPanel, objectivePanel, instructionBar, equipmentPanel, laboratoryPanel;
    private GameObject dataTablePanel, comparePanel, questionPanel, conclusionPanel, resultPanel, variablePanel;
    private TextMeshProUGUI introText, objectiveText, conclusionText, compareText, questionText, dataTableText;
    private TextMeshProUGUI finalScoreText, resultDetailsText, statusText, liveReadingsText, physicsText, conclusionPreview;
    private Button startBtn, nextBtn, resetBtn, retryBtn, viewProfileBtn, viewResultsBtn;
    private Button confirmSetupBtn, shakeSideBtn, shakePushBtn, observeBtn;
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

    public void BindAll(WavesUIRefs refs, bool showWelcome)
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
        shakeSideBtn = refs.ShakeSideBtn; shakePushBtn = refs.ShakePushBtn; observeBtn = refs.ObserveBtn;
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
            WavesExperimentManager.Instance?.ResetExperiment();
            ShowIntro();
        });
        WireBtn(resetNo, () => resetConfirmPanel?.SetActive(false));
        WireBtn(retryBtn, () => { if (retryConfirmPanel != null) retryConfirmPanel.SetActive(true); });
        WireBtn(retryYes, () =>
        {
            retryConfirmPanel?.SetActive(false);
            WavesExperimentManager.Instance?.RetryExperiment();
        });
        WireBtn(retryNo, () => retryConfirmPanel?.SetActive(false));
        WireBtn(viewProfileBtn, () =>
        {
            string summary = WavesProfileManager.Instance != null ? WavesProfileManager.Instance.GetProfileSummary() : "No profile data.";
            WavesFeedbackManager.Instance?.ShowInstruction(summary);
        });
        WireBtn(viewResultsBtn, () => resultPanel?.SetActive(true));
        WireBtn(confirmSetupBtn, () => WavesExperimentManager.Instance?.ConfirmSetup());
        WireBtn(shakeSideBtn, () => WavesExperimentManager.Instance?.ShakeSideToSide());
        WireBtn(shakePushBtn, () => WavesExperimentManager.Instance?.ShakePushPull());
        WireBtn(observeBtn, () => WavesExperimentManager.Instance?.ObserveRibbons());
        WireBtn(questionA, () => WavesQuestionManager.Instance?.Answer(0));
        WireBtn(questionB, () => WavesQuestionManager.Instance?.Answer(1));
        WireBtn(questionC, () => WavesQuestionManager.Instance?.Answer(2));
        WireBtn(questionD, () => WavesQuestionManager.Instance?.Answer(3));
        WireBtn(questionContinue, OnQuestionContinue);
        WireBtn(compareA, () => WavesExperimentManager.Instance?.AnswerMotion(0));
        WireBtn(compareB, () => WavesExperimentManager.Instance?.AnswerMotion(1));
        WireBtn(compareC, () => WavesExperimentManager.Instance?.AnswerMotion(2));
        WireBtn(variableContinueBtn, () => WavesExperimentManager.Instance?.CompleteVariableMatching());
        WireBtn(conclusionContinueBtn, () => WavesExperimentManager.Instance?.CompleteConclusion());
        if (phraseButtons != null)
        {
            for (int i = 0; i < phraseButtons.Length; i++)
            {
                int idx = i;
                WireBtn(phraseButtons[i], () =>
                {
                    var tmp = phraseButtons[idx] != null ? phraseButtons[idx].GetComponentInChildren<TextMeshProUGUI>() : null;
                    if (tmp != null && !string.IsNullOrEmpty(tmp.text))
                        WavesConclusionManager.Instance?.AddPhrase(tmp.text);
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

    public void StartPractical() => WavesExperimentManager.Instance?.StartPractical();

    private void OnNextPressed()
    {
        var step = Current();
        if (step == WavesExperimentStep.Introduction) { StartPractical(); return; }
        if (step == WavesExperimentStep.SelectEquipment)
        {
            WavesExperimentManager.Instance?.TryAdvanceFromEquipment();
            return;
        }
        if (step == WavesExperimentStep.Questions)
        {
            OnQuestionContinue();
            return;
        }
        if (step == WavesExperimentStep.VariableMatching)
        {
            WavesExperimentManager.Instance?.CompleteVariableMatching();
            return;
        }
        if (step == WavesExperimentStep.Conclusion)
        {
            WavesExperimentManager.Instance?.CompleteConclusion();
            return;
        }
        WavesExperimentManager.Instance?.AdvanceStep();
    }

    private void OnQuestionContinue()
    {
        if (WavesQuestionManager.Instance == null)
        {
            WavesExperimentManager.Instance?.CompleteQuestions();
            return;
        }
        if (WavesQuestionManager.Instance.IsFinished)
        {
            WavesExperimentManager.Instance?.CompleteQuestions();
            return;
        }
        if (!WavesQuestionManager.Instance.HasAnswered)
        {
            WavesFeedbackManager.Instance?.ShowInstruction("Select A, B, C or D first. Then press CONTINUE.");
            return;
        }
        bool done = WavesQuestionManager.Instance.Advance();
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(false);
        if (done) WavesExperimentManager.Instance?.CompleteQuestions();
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
                "WAVES AND THEIR APPLICATIONS\n\n" +
                "4.5 — Demonstration of the formation of transverse waves using a slinky\n\n" +
                "Pieces of ribbon are tied at several places along a slinky. The slinky is placed flat on a table. One end is held and shaken from side to side on the plane of the table.\n\n" +
                "You will watch the ribbons as a pulse travels along the slinky.\n\n" +
                "The idea you will test is:\nIn a transverse wave, particles of the medium move perpendicular to the direction of the wave.";
        }
        UpdateAttemptsDisplay(WavesAttemptManager.Instance != null ? WavesAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateScoreDisplay(0);
        UpdateProgress(1, 11);
        if (stepText != null) stepText.text = "Introduction";
        if (titleText != null)
            titleText.text = "WAVES     Transverse slinky";
    }

    public void ShowStep(WavesExperimentStep step)
    {
        HideAllContent();
        if (instructionBar != null) instructionBar.SetActive(step != WavesExperimentStep.Introduction && step != WavesExperimentStep.Complete);
        SetNextButtonVisible(step != WavesExperimentStep.Assembly && step != WavesExperimentStep.GenerateWave && step != WavesExperimentStep.IdentifyMotion);
        SetLabControls(false);

        switch (step)
        {
            case WavesExperimentStep.Introduction:
                ShowIntro();
                return;
            case WavesExperimentStep.Objective:
                objectivePanel?.SetActive(true);
                if (objectiveText != null)
                    objectiveText.text =
                        "OBJECTIVES\n\nBy completing this practical you will:\n\n" +
                        "• Identify the apparatus needed to demonstrate a transverse wave.\n" +
                        "• Tie ribbons at several places along a slinky laid on a table.\n" +
                        "• Shake one end of the slinky from side to side to form a transverse wave.\n" +
                        "• Observe that the ribbons (particles) move perpendicular to the wave direction.\n" +
                        "• Distinguish this from a longitudinal (push-pull) wave.";
                SetInstruction("Read the objectives, then press CONTINUE.");
                SetNextLabel("CONTINUE");
                break;
            case WavesExperimentStep.SelectEquipment:
                equipmentPanel?.SetActive(true);
                WavesEquipmentSelectionManager.Instance?.EnsureCardsVisible();
                SetInstruction("Select all equipment required for this practical. Tap or drag items into the REQUIRED EQUIPMENT AREA.");
                break;
            case WavesExperimentStep.Assembly:
                laboratoryPanel?.SetActive(true);
                SetLabControls(true);
                SetInstruction(WavesAssemblyManager.Instance != null ? WavesAssemblyManager.Instance.NextHint() : "Assemble the apparatus.");
                break;
            case WavesExperimentStep.GenerateWave:
                laboratoryPanel?.SetActive(true);
                SetLabControls(true);
                SetInstruction("Hold one end. Shake SIDE TO SIDE on the table to form a transverse wave. Do not push and pull along the slinky.");
                SetNextButtonVisible(WavesWaveController.Instance != null && WavesWaveController.Instance.HasTransverseWave);
                break;
            case WavesExperimentStep.ObserveRibbons:
                laboratoryPanel?.SetActive(true);
                SetLabControls(true);
                SetInstruction("Watch the pink ribbons as the pulse travels. Then press OBSERVE RIBBONS.");
                SetNextButtonVisible(false);
                break;
            case WavesExperimentStep.IdentifyMotion:
                comparePanel?.SetActive(true);
                FillCompare();
                SetInstruction("How do the ribbons move compared with the direction of the wave?");
                SetNextButtonVisible(false);
                break;
            case WavesExperimentStep.ObservationTable:
                dataTablePanel?.SetActive(true);
                WavesObservationTableManager.Instance?.Refresh();
                SetInstruction("Study the table. Ribbons move perpendicular to the wave that travels along the slinky.");
                break;
            case WavesExperimentStep.Questions:
                questionPanel?.SetActive(true);
                SetNextButtonVisible(true);
                SetNextLabel("CONTINUE");
                SetInstruction("Select an answer, then press CONTINUE.");
                break;
            case WavesExperimentStep.VariableMatching:
                variablePanel?.SetActive(true);
                SetInstruction("Tap Independent, Dependent or Controlled for each quantity. Then press NEXT STEP.");
                SetNextButtonVisible(true);
                SetNextLabel("NEXT STEP");
                if (variableContinueBtn != null) variableContinueBtn.gameObject.SetActive(true);
                break;
            case WavesExperimentStep.Conclusion:
                conclusionPanel?.SetActive(true);
                SetInstruction("Tap the phrases in the correct order, then press NEXT STEP.");
                SetNextButtonVisible(true);
                SetNextLabel("NEXT STEP");
                if (conclusionContinueBtn != null) conclusionContinueBtn.gameObject.SetActive(true);
                break;
            case WavesExperimentStep.Complete:
                ShowResult();
                break;
        }

        UpdateProgress((int)step + 1, 11);
        if (stepText != null) stepText.text = StepTitle(step);
        if (step != WavesExperimentStep.Objective) SetNextLabel("NEXT STEP");
        UpdateLiveReadings();
    }

    private void FillCompare()
    {
        if (compareText == null) return;
        compareText.text =
            "IDENTIFY PARTICLE MOTION\n\n" +
            "The wave travels along the slinky, away from the hand.\n" +
            "The pink ribbons are tied to coils of the slinky.\n\n" +
            "How do the ribbons move?\n\n" +
            "A. They travel along the slinky with the wave, to the far end.\n" +
            "B. They move in the opposite direction to the wave.\n" +
            "C. They move perpendicular to the direction of the wave (across the table).";
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
                "When a slinky is shaken from side to side,\n" +
                "a transverse wave travels along the slinky,\n" +
                "while the ribbons (particles of the medium)\n" +
                "move perpendicular to the direction of the wave.\n\n" +
                "Your sentence:\n" + text;
    }

    public void UpdateLiveReadings()
    {
        var wave = WavesWaveController.Instance;
        var asm = WavesAssemblyManager.Instance;
        bool trans = wave != null && wave.HasTransverseWave;
        int ribbons = asm != null ? asm.RibbonCount : 0;
        if (liveReadingsText != null)
        {
            liveReadingsText.text =
                $"Table:  {(asm != null && asm.TablePlaced ? "placed" : "—")}\n" +
                $"Slinky:  {(asm != null && asm.SlinkyPlaced ? "on table" : "—")}\n" +
                $"Ribbons tied:  {ribbons} / 5\n" +
                $"Shake:  {(trans ? "side to side" : "—")}\n" +
                $"Wave type:  {(trans ? "TRANSVERSE" : "—")}\n\n" +
                (trans
                    ? "Wave direction: along slinky\nRibbon motion: perpendicular"
                    : "Generate the wave to see motion.");
        }
        if (physicsText != null)
        {
            physicsText.text =
                "TRANSVERSE WAVE\nParticles move at right angles to the wave direction.\n\n" +
                "THIS PRACTICAL\nShake the slinky left–right on the table. The pulse travels along the slinky.\n\n" +
                "RIBBONS\nThey mark coils (particles). They do not travel to the far end.";
        }
        if (titleText != null)
            titleText.text = $"WAVES     Attempt: {(WavesAttemptManager.Instance != null ? WavesAttemptManager.Instance.CurrentAttemptNumber : 1)}/3";
        if (stepText != null)
        {
            var cur = Current();
            if (cur == WavesExperimentStep.Assembly) stepText.text = "Assembly";
            else if (cur == WavesExperimentStep.GenerateWave) stepText.text = "Generate wave";
            else if (cur == WavesExperimentStep.ObserveRibbons) stepText.text = "Observe";
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
        bool assembly = Current() == WavesExperimentStep.Assembly;
        bool generate = Current() == WavesExperimentStep.GenerateWave;
        bool observe = Current() == WavesExperimentStep.ObserveRibbons;
        if (confirmSetupBtn != null) confirmSetupBtn.gameObject.SetActive(visible && assembly);
        if (shakeSideBtn != null) shakeSideBtn.gameObject.SetActive(visible && generate);
        if (shakePushBtn != null) shakePushBtn.gameObject.SetActive(visible && generate);
        if (observeBtn != null) observeBtn.gameObject.SetActive(visible && observe);
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

    private static string StepTitle(WavesExperimentStep step)
    {
        switch (step)
        {
            case WavesExperimentStep.Objective: return "Objectives";
            case WavesExperimentStep.SelectEquipment: return "Equipment";
            case WavesExperimentStep.Assembly: return "Assembly";
            case WavesExperimentStep.GenerateWave: return "Generate wave";
            case WavesExperimentStep.ObserveRibbons: return "Observe";
            case WavesExperimentStep.IdentifyMotion: return "Identify";
            case WavesExperimentStep.ObservationTable: return "Observations";
            case WavesExperimentStep.Questions: return "Questions";
            case WavesExperimentStep.VariableMatching: return "Variables";
            case WavesExperimentStep.Conclusion: return "Conclusion";
            case WavesExperimentStep.Complete: return "Results";
            default: return "Introduction";
        }
    }

    private static WavesExperimentStep Current()
    {
        return WavesExperimentManager.Instance != null
            ? WavesExperimentManager.Instance.CurrentStep
            : WavesExperimentStep.Introduction;
    }
}

public class WavesIntroClickToStart : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (WavesUIManager.Instance != null) WavesUIManager.Instance.StartPractical();
    }
}
