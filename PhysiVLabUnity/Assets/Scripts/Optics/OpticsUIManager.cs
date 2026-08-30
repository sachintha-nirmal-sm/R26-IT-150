using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OpticsUIManager : MonoBehaviour
{
    public static OpticsUIManager Instance { get; private set; }

    private TextMeshProUGUI titleText, scoreText, progressText, attemptsText, stepText, instructionText;
    private Image progressBarFill;
    private GameObject introPanel, objectivePanel, instructionBar, equipmentPanel, laboratoryPanel;
    private GameObject dataTablePanel, comparePanel, questionPanel, conclusionPanel, resultPanel, variablePanel;
    private TextMeshProUGUI introText, objectiveText, conclusionText, compareText, questionText, dataTableText;
    private TextMeshProUGUI finalScoreText, resultDetailsText, statusText, liveReadingsText, physicsText, conclusionPreview;
    private Button startBtn, nextBtn, resetBtn, retryBtn, viewProfileBtn, viewResultsBtn;
    private Button confirmSetupBtn, openWindowBtn, sharpImageBtn, recordFBtn;
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

    public void BindAll(OpticsUIRefs refs, bool showWelcome)
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
        openWindowBtn = refs.OpenWindowBtn; sharpImageBtn = refs.SharpImageBtn; recordFBtn = refs.RecordFBtn;
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
            OpticsExperimentManager.Instance?.ResetExperiment();
            ShowIntro();
        });
        WireBtn(resetNo, () => resetConfirmPanel?.SetActive(false));
        WireBtn(retryBtn, () => { if (retryConfirmPanel != null) retryConfirmPanel.SetActive(true); });
        WireBtn(retryYes, () =>
        {
            retryConfirmPanel?.SetActive(false);
            OpticsExperimentManager.Instance?.RetryExperiment();
        });
        WireBtn(retryNo, () => retryConfirmPanel?.SetActive(false));
        WireBtn(viewProfileBtn, () =>
        {
            string summary = OpticsProfileManager.Instance != null ? OpticsProfileManager.Instance.GetProfileSummary() : "No profile data.";
            OpticsFeedbackManager.Instance?.ShowInstruction(summary);
        });
        WireBtn(viewResultsBtn, () => resultPanel?.SetActive(true));
        WireBtn(confirmSetupBtn, () => OpticsExperimentManager.Instance?.ConfirmSetup());
        WireBtn(openWindowBtn, () => OpticsExperimentManager.Instance?.OpenWindow());
        WireBtn(sharpImageBtn, () => OpticsExperimentManager.Instance?.ConfirmSharpImage());
        WireBtn(recordFBtn, () => OpticsExperimentManager.Instance?.RecordMeasurement());
        WireBtn(questionA, () => OpticsQuestionManager.Instance?.Answer(0));
        WireBtn(questionB, () => OpticsQuestionManager.Instance?.Answer(1));
        WireBtn(questionC, () => OpticsQuestionManager.Instance?.Answer(2));
        WireBtn(questionD, () => OpticsQuestionManager.Instance?.Answer(3));
        WireBtn(questionContinue, OnQuestionContinue);
        WireBtn(compareA, () => OpticsExperimentManager.Instance?.AnswerImage(0));
        WireBtn(compareB, () => OpticsExperimentManager.Instance?.AnswerImage(1));
        WireBtn(compareC, () => OpticsExperimentManager.Instance?.AnswerImage(2));
        WireBtn(variableContinueBtn, () => OpticsExperimentManager.Instance?.CompleteVariableMatching());
        WireBtn(conclusionContinueBtn, () => OpticsExperimentManager.Instance?.CompleteConclusion());
        if (phraseButtons != null)
        {
            for (int i = 0; i < phraseButtons.Length; i++)
            {
                int idx = i;
                WireBtn(phraseButtons[i], () =>
                {
                    var tmp = phraseButtons[idx] != null ? phraseButtons[idx].GetComponentInChildren<TextMeshProUGUI>() : null;
                    if (tmp != null && !string.IsNullOrEmpty(tmp.text))
                        OpticsConclusionManager.Instance?.AddPhrase(tmp.text);
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

    public void StartPractical() => OpticsExperimentManager.Instance?.StartPractical();

    private void OnNextPressed()
    {
        var step = Current();
        if (step == OpticsExperimentStep.Introduction) { StartPractical(); return; }
        if (step == OpticsExperimentStep.SelectEquipment)
        {
            OpticsExperimentManager.Instance?.TryAdvanceFromEquipment();
            return;
        }
        if (step == OpticsExperimentStep.Questions)
        {
            OnQuestionContinue();
            return;
        }
        if (step == OpticsExperimentStep.VariableMatching)
        {
            OpticsExperimentManager.Instance?.CompleteVariableMatching();
            return;
        }
        if (step == OpticsExperimentStep.Conclusion)
        {
            OpticsExperimentManager.Instance?.CompleteConclusion();
            return;
        }
        OpticsExperimentManager.Instance?.AdvanceStep();
    }

    private void OnQuestionContinue()
    {
        if (OpticsQuestionManager.Instance == null)
        {
            OpticsExperimentManager.Instance?.CompleteQuestions();
            return;
        }
        if (OpticsQuestionManager.Instance.IsFinished)
        {
            OpticsExperimentManager.Instance?.CompleteQuestions();
            return;
        }
        if (!OpticsQuestionManager.Instance.HasAnswered)
        {
            OpticsFeedbackManager.Instance?.ShowInstruction("Select A, B, C or D first. Then press CONTINUE.");
            return;
        }
        bool done = OpticsQuestionManager.Instance.Advance();
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(false);
        if (done) OpticsExperimentManager.Instance?.CompleteQuestions();
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
                "GEOMETRICAL OPTICS\n\n" +
                "Finding the approximate focal length of a concave mirror using a distant object\n\n" +
                "Open a window. Hold a concave mirror turned towards the window. Hold a white screen in front of the mirror and move it until a clear, upside-down image of the outdoor scene is formed.\n\n" +
                "Because this image is formed on the screen, it is a real image.\n\n" +
                "The idea you will test is:\nLight rays from a far-away object can be treated as parallel, so the mirror–screen distance is approximately the focal length of the concave mirror.";
        }
        UpdateAttemptsDisplay(OpticsAttemptManager.Instance != null ? OpticsAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateScoreDisplay(0);
        UpdateProgress(1, 11);
        if (stepText != null) stepText.text = "Introduction";
        if (titleText != null)
            titleText.text = "GEOMETRICAL OPTICS     Concave mirror";
    }

    public void ShowStep(OpticsExperimentStep step)
    {
        HideAllContent();
        if (instructionBar != null) instructionBar.SetActive(step != OpticsExperimentStep.Introduction && step != OpticsExperimentStep.Complete);
        SetNextButtonVisible(step != OpticsExperimentStep.Assembly && step != OpticsExperimentStep.AdjustFocus && step != OpticsExperimentStep.IdentifyImage && step != OpticsExperimentStep.MeasureFocalLength);
        SetLabControls(false);

        switch (step)
        {
            case OpticsExperimentStep.Introduction:
                ShowIntro();
                return;
            case OpticsExperimentStep.Objective:
                objectivePanel?.SetActive(true);
                if (objectiveText != null)
                    objectiveText.text =
                        "OBJECTIVES\n\nBy completing this practical you will:\n\n" +
                        "• Identify the apparatus needed to find the focal length of a concave mirror.\n" +
                        "• Open a window so that a distant outdoor scene can be used as the object.\n" +
                        "• Hold a concave mirror towards the window and a white screen in front of it.\n" +
                        "• Adjust the screen until a clear, inverted real image is formed.\n" +
                        "• Measure the mirror–screen distance and take it as the approximate focal length.";
                SetInstruction("Read the objectives, then press CONTINUE.");
                SetNextLabel("CONTINUE");
                break;
            case OpticsExperimentStep.SelectEquipment:
                equipmentPanel?.SetActive(true);
                OpticsEquipmentSelectionManager.Instance?.EnsureCardsVisible();
                SetInstruction("Select all equipment required for this practical. Tap or drag items into the REQUIRED EQUIPMENT AREA.");
                break;
            case OpticsExperimentStep.Assembly:
                laboratoryPanel?.SetActive(true);
                SetLabControls(true);
                SetInstruction(OpticsAssemblyManager.Instance != null ? OpticsAssemblyManager.Instance.NextHint() : "Assemble the apparatus.");
                break;
            case OpticsExperimentStep.AdjustFocus:
                laboratoryPanel?.SetActive(true);
                SetLabControls(true);
                SetInstruction("Move the screen until a very clear, upside-down image of the outdoor scene is formed. Then press IMAGE IS SHARP.");
                SetNextButtonVisible(false);
                break;
            case OpticsExperimentStep.IdentifyImage:
                comparePanel?.SetActive(true);
                FillCompare();
                SetInstruction("The image is formed on the screen. What kind of image is it?");
                SetNextButtonVisible(false);
                break;
            case OpticsExperimentStep.MeasureFocalLength:
                laboratoryPanel?.SetActive(true);
                SetLabControls(true);
                SetInstruction("Read the meter ruler. The mirror–screen distance for a sharp image is approximately the focal length f. Press RECORD f.");
                SetNextButtonVisible(false);
                break;
            case OpticsExperimentStep.ObservationTable:
                dataTablePanel?.SetActive(true);
                OpticsObservationTableManager.Instance?.Refresh();
                SetInstruction("Study the table. Parallel rays from a distant object meet at the focus of the concave mirror.");
                break;
            case OpticsExperimentStep.Questions:
                questionPanel?.SetActive(true);
                SetNextButtonVisible(true);
                SetNextLabel("CONTINUE");
                SetInstruction("Select an answer, then press CONTINUE.");
                break;
            case OpticsExperimentStep.VariableMatching:
                variablePanel?.SetActive(true);
                SetInstruction("Tap Independent, Dependent or Controlled for each quantity. Then press NEXT STEP.");
                SetNextButtonVisible(true);
                SetNextLabel("NEXT STEP");
                if (variableContinueBtn != null) variableContinueBtn.gameObject.SetActive(true);
                break;
            case OpticsExperimentStep.Conclusion:
                conclusionPanel?.SetActive(true);
                SetInstruction("Tap the phrases in the correct order, then press NEXT STEP.");
                SetNextButtonVisible(true);
                SetNextLabel("NEXT STEP");
                if (conclusionContinueBtn != null) conclusionContinueBtn.gameObject.SetActive(true);
                break;
            case OpticsExperimentStep.Complete:
                ShowResult();
                break;
        }

        UpdateProgress((int)step + 1, 11);
        if (stepText != null) stepText.text = StepTitle(step);
        if (step != OpticsExperimentStep.Objective) SetNextLabel("NEXT STEP");
        UpdateLiveReadings();
    }

    private void FillCompare()
    {
        if (compareText == null) return;
        compareText.text =
            "IDENTIFY THE IMAGE\n\n" +
            "The outdoor scene is far away, so its rays are treated as parallel.\n" +
            "Those rays reflect from the concave mirror and meet on the white screen.\n\n" +
            "What is true of this image?\n\n" +
            "A. It is virtual and erect (the right way up).\n" +
            "B. It is virtual and inverted, formed behind the mirror.\n" +
            "C. It is real and inverted (upside down), because it is formed on the screen.";
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
                "Because light rays from a far-away object can be considered as parallel,\n" +
                "they meet at the principal focus of the concave mirror,\n" +
                "so the distance from the mirror to the sharp real image on the screen\n" +
                "is approximately equal to the focal length of the mirror.\n\n" +
                "Your sentence:\n" + text;
    }

    public void UpdateLiveReadings()
    {
        var vis = OpticsVisualController.Instance;
        var asm = OpticsAssemblyManager.Instance;
        bool sharp = vis != null && vis.IsInFocus;
        float d = vis != null ? vis.ScreenDistanceCm : 0f;
        if (liveReadingsText != null)
        {
            liveReadingsText.text =
                $"Window:  {(asm != null && asm.WindowOpened ? "open" : "closed")}\n" +
                $"Concave mirror:  {(asm != null && asm.MirrorPlaced ? "facing window" : "—")}\n" +
                $"White screen:  {(asm != null && asm.ScreenPlaced ? "in front of mirror" : "—")}\n" +
                $"Ruler:  {(asm != null && asm.RulerPlaced ? "placed" : "—")}\n" +
                $"Distance:  {(asm != null && asm.ScreenPlaced ? $"{d:0.0} cm" : "—")}\n" +
                $"Image:  {(sharp ? "CLEAR, INVERTED" : asm != null && asm.ScreenPlaced ? "blurred" : "—")}\n\n" +
                (sharp
                    ? $"f ≈ {d:0.0} cm  (parallel rays meet at F)"
                    : "Move the screen until the image is sharp.");
        }
        if (physicsText != null)
        {
            physicsText.text =
                "DISTANT OBJECT\nRays arriving at the mirror are treated as parallel.\n\n" +
                "CONCAVE MIRROR\nParallel rays converge at the principal focus F.\n\n" +
                "FOCAL LENGTH\nMirror–screen distance for a sharp real image ≈ f.";
        }
        if (titleText != null)
            titleText.text = $"GEOMETRICAL OPTICS     Attempt: {(OpticsAttemptManager.Instance != null ? OpticsAttemptManager.Instance.CurrentAttemptNumber : 1)}/3";
        if (stepText != null)
        {
            var cur = Current();
            if (cur == OpticsExperimentStep.Assembly) stepText.text = "Assembly";
            else if (cur == OpticsExperimentStep.AdjustFocus) stepText.text = "Focus image";
            else if (cur == OpticsExperimentStep.MeasureFocalLength) stepText.text = "Measure f";
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
        bool assembly = Current() == OpticsExperimentStep.Assembly;
        bool focus = Current() == OpticsExperimentStep.AdjustFocus;
        bool measure = Current() == OpticsExperimentStep.MeasureFocalLength;
        if (confirmSetupBtn != null) confirmSetupBtn.gameObject.SetActive(visible && assembly);
        if (openWindowBtn != null) openWindowBtn.gameObject.SetActive(visible && assembly);
        if (sharpImageBtn != null) sharpImageBtn.gameObject.SetActive(visible && focus);
        if (recordFBtn != null) recordFBtn.gameObject.SetActive(visible && measure);
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

    private static string StepTitle(OpticsExperimentStep step)
    {
        switch (step)
        {
            case OpticsExperimentStep.Objective: return "Objectives";
            case OpticsExperimentStep.SelectEquipment: return "Equipment";
            case OpticsExperimentStep.Assembly: return "Assembly";
            case OpticsExperimentStep.AdjustFocus: return "Focus image";
            case OpticsExperimentStep.IdentifyImage: return "Identify image";
            case OpticsExperimentStep.MeasureFocalLength: return "Measure f";
            case OpticsExperimentStep.ObservationTable: return "Observations";
            case OpticsExperimentStep.Questions: return "Questions";
            case OpticsExperimentStep.VariableMatching: return "Variables";
            case OpticsExperimentStep.Conclusion: return "Conclusion";
            case OpticsExperimentStep.Complete: return "Results";
            default: return "Introduction";
        }
    }

    private static OpticsExperimentStep Current()
    {
        return OpticsExperimentManager.Instance != null
            ? OpticsExperimentManager.Instance.CurrentStep
            : OpticsExperimentStep.Introduction;
    }
}

public class OpticsIntroClickToStart : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (OpticsUIManager.Instance != null) OpticsUIManager.Instance.StartPractical();
    }
}
