using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FrictionUIManager : MonoBehaviour
{
    public static FrictionUIManager Instance { get; private set; }

    private TextMeshProUGUI titleText, scoreText, progressText, attemptsText, stepText, instructionText;
    private Image progressBarFill;
    private GameObject introPanel, objectivePanel, instructionBar, equipmentPanel, laboratoryPanel;
    private GameObject dataTablePanel, comparePanel, graphPanel, questionPanel, conclusionPanel, resultPanel, variablePanel;
    private TextMeshProUGUI introText, objectiveText, conclusionText, compareText, questionText, dataTableText;
    private TextMeshProUGUI finalScoreText, resultDetailsText, statusText, liveReadingsText, physicsText, conclusionPreview;
    private Button startBtn, nextBtn, resetBtn, retryBtn, viewProfileBtn, viewResultsBtn;
    private Button pullBtn, stopPullBtn, resetRunBtn, recordBtn, confirmSetupBtn;
    private Button surfaceABtn, surfaceBBtn, surfaceCBtn;
    private Button questionA, questionB, questionC, questionD, questionContinue;
    private Button compareA, compareB, compareC;
    private GameObject questionExplanationPanel, resetConfirmPanel, retryConfirmPanel, optionsGroup;
    private TextMeshProUGUI questionExplanationText, optAText, optBText, optCText, optDText;
    private Button resetYes, resetNo, retryYes, retryNo;
    private Button[] phraseButtons;
    private Button variableContinueBtn, conclusionContinueBtn;
    private RectTransform forceArrow, frictionArrow;

    private void Awake() => Instance = this;

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void BindAll(FrictionUIRefs refs, bool showWelcome)
    {
        titleText = refs.Title; scoreText = refs.Score; progressText = refs.Progress;
        attemptsText = refs.Attempts; stepText = refs.StepLabel;
        progressBarFill = refs.ProgressBar;
        instructionText = refs.Instruction;
        introPanel = refs.IntroPanel; objectivePanel = refs.ObjectivePanel;
        instructionBar = refs.InstructionBar; equipmentPanel = refs.EquipmentPanel;
        laboratoryPanel = refs.LaboratoryPanel;
        dataTablePanel = refs.DataTablePanel; comparePanel = refs.ComparePanel;
        graphPanel = refs.GraphPanel; questionPanel = refs.QuestionPanel;
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
        pullBtn = refs.PullBtn; stopPullBtn = refs.StopPullBtn;
        resetRunBtn = refs.ResetRunBtn; recordBtn = refs.RecordBtn; confirmSetupBtn = refs.ConfirmSetupBtn;
        surfaceABtn = refs.SurfaceA; surfaceBBtn = refs.SurfaceB; surfaceCBtn = refs.SurfaceC;
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
        forceArrow = refs.ForceArrow; frictionArrow = refs.FrictionArrow;
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
            FrictionExperimentManager.Instance?.ResetExperiment();
            ShowIntro();
        });
        WireBtn(resetNo, () => resetConfirmPanel?.SetActive(false));
        WireBtn(retryBtn, () => { if (retryConfirmPanel != null) retryConfirmPanel.SetActive(true); });
        WireBtn(retryYes, () =>
        {
            retryConfirmPanel?.SetActive(false);
            FrictionExperimentManager.Instance?.RetryExperiment();
        });
        WireBtn(retryNo, () => retryConfirmPanel?.SetActive(false));
        WireBtn(viewProfileBtn, () =>
        {
            string summary = FrictionProfileManager.Instance != null ? FrictionProfileManager.Instance.GetProfileSummary() : "No profile data.";
            FrictionFeedbackManager.Instance?.ShowInstruction(summary);
        });
        WireBtn(viewResultsBtn, () => resultPanel?.SetActive(true));
        WireHold(pullBtn, () => FrictionExperimentManager.Instance?.StartPulling(), () => FrictionExperimentManager.Instance?.StopPulling());
        WireBtn(stopPullBtn, () => FrictionExperimentManager.Instance?.StopPulling());
        WireBtn(resetRunBtn, () => FrictionExperimentManager.Instance?.ResetCurrentTrial());
        WireBtn(recordBtn, () => FrictionExperimentManager.Instance?.RecordReading());
        WireBtn(confirmSetupBtn, () => FrictionExperimentManager.Instance?.ConfirmSetup());
        WireBtn(surfaceABtn, () => FrictionExperimentManager.Instance?.SelectSurface(0));
        WireBtn(surfaceBBtn, () => FrictionExperimentManager.Instance?.SelectSurface(1));
        WireBtn(surfaceCBtn, () => FrictionExperimentManager.Instance?.SelectSurface(2));
        WireBtn(questionA, () => FrictionQuestionManager.Instance?.Answer(0));
        WireBtn(questionB, () => FrictionQuestionManager.Instance?.Answer(1));
        WireBtn(questionC, () => FrictionQuestionManager.Instance?.Answer(2));
        WireBtn(questionD, () => FrictionQuestionManager.Instance?.Answer(3));
        WireBtn(questionContinue, OnQuestionContinue);
        WireBtn(compareA, () => FrictionExperimentManager.Instance?.AnswerCompare(0));
        WireBtn(compareB, () => FrictionExperimentManager.Instance?.AnswerCompare(1));
        WireBtn(compareC, () => FrictionExperimentManager.Instance?.AnswerCompare(2));
        WireBtn(variableContinueBtn, () => FrictionExperimentManager.Instance?.CompleteVariableMatching());
        WireBtn(conclusionContinueBtn, () => FrictionExperimentManager.Instance?.CompleteConclusion());
        if (phraseButtons != null)
        {
            for (int i = 0; i < phraseButtons.Length; i++)
            {
                int idx = i;
                WireBtn(phraseButtons[i], () =>
                {
                    var tmp = phraseButtons[idx] != null ? phraseButtons[idx].GetComponentInChildren<TextMeshProUGUI>() : null;
                    if (tmp != null && !string.IsNullOrEmpty(tmp.text))
                        ConclusionManager.Instance?.AddPhrase(tmp.text);
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

    private void WireHold(Button btn, System.Action down, System.Action up)
    {
        if (btn == null) return;
        var hold = btn.gameObject.GetComponent<FrictionHoldButton>() ?? btn.gameObject.AddComponent<FrictionHoldButton>();
        hold.Configure(down, up);
    }

    public void StartPractical() => FrictionExperimentManager.Instance?.StartPractical();

    private void OnNextPressed()
    {
        var step = Current();
        if (step == FrictionExperimentStep.Introduction) { StartPractical(); return; }
        if (step == FrictionExperimentStep.SelectEquipment)
        {
            FrictionExperimentManager.Instance?.TryAdvanceFromEquipment();
            return;
        }
        if (step == FrictionExperimentStep.Questions)
        {
            OnQuestionContinue();
            return;
        }
        if (step == FrictionExperimentStep.VariableMatching)
        {
            FrictionExperimentManager.Instance?.CompleteVariableMatching();
            return;
        }
        if (step == FrictionExperimentStep.Conclusion)
        {
            FrictionExperimentManager.Instance?.CompleteConclusion();
            return;
        }
        FrictionExperimentManager.Instance?.AdvanceStep();
    }

    private void OnQuestionContinue()
    {
        if (FrictionQuestionManager.Instance == null)
        {
            FrictionExperimentManager.Instance?.CompleteQuestions();
            return;
        }
        if (FrictionQuestionManager.Instance.IsFinished)
        {
            FrictionExperimentManager.Instance?.CompleteQuestions();
            return;
        }
        if (!FrictionQuestionManager.Instance.HasAnswered)
        {
            FrictionFeedbackManager.Instance?.ShowInstruction("Select A, B, C or D first. Then press CONTINUE.");
            return;
        }
        bool done = FrictionQuestionManager.Instance.Advance();
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(false);
        if (done) FrictionExperimentManager.Instance?.CompleteQuestions();
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
                "FRICTION\n\n" +
                "Investigation of the Influence of Surface Area on Friction\n\n" +
                "In this practical you will investigate whether the limiting frictional force depends on the area of contact between a wooden block and a rough surface.\n\n" +
                "Weight of wooden block:  60 N\n" +
                "Surface condition:  Same sandpaper roughness\n\n" +
                "You will rotate the same wooden block onto three different surfaces, pull it with a Newton balance, and compare the limiting frictional force.";
        }
        UpdateAttemptsDisplay(FrictionAttemptManager.Instance != null ? FrictionAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateScoreDisplay(0);
        UpdateProgress(1, 12);
        if (stepText != null) stepText.text = "Introduction";
    }

    public void ShowStep(FrictionExperimentStep step)
    {
        HideAllContent();
        if (instructionBar != null) instructionBar.SetActive(step != FrictionExperimentStep.Introduction && step != FrictionExperimentStep.Complete);
        SetNextButtonVisible(step != FrictionExperimentStep.Setup && step != FrictionExperimentStep.Pulling);
        SetLabControls(false);

        switch (step)
        {
            case FrictionExperimentStep.Introduction:
                ShowIntro();
                return;
            case FrictionExperimentStep.Objective:
                objectivePanel?.SetActive(true);
                if (objectiveText != null)
                    objectiveText.text =
                        "OBJECTIVES\n\nBy completing this practical you will:\n\n" +
                        "• Understand frictional force.\n" +
                        "• Understand limiting friction.\n" +
                        "• Investigate the effect of contact area on friction.\n" +
                        "• Use a Newton balance to measure force.\n" +
                        "• Compare frictional forces for different contact areas.\n" +
                        "• Record experimental observations.\n" +
                        "• Draw a scientific conclusion from experimental results.";
                SetInstruction("Read the objectives, then press CONTINUE.");
                SetNextLabel("CONTINUE");
                break;
            case FrictionExperimentStep.SelectEquipment:
                equipmentPanel?.SetActive(true);
                FrictionEquipmentSelectionManager.Instance?.EnsureCardsVisible();
                SetInstruction("Select all equipment required for this practical. Drag or tap items into the REQUIRED EQUIPMENT AREA.");
                break;
            case FrictionExperimentStep.Setup:
                laboratoryPanel?.SetActive(true);
                SetLabControls(true);
                SetInstruction(SetupInstruction());
                break;
            case FrictionExperimentStep.Pulling:
                laboratoryPanel?.SetActive(true);
                SetLabControls(true);
                SetInstruction("Slowly increase the pulling force. Record the force reading just when the block begins to move.");
                SetNextButtonVisible(false);
                break;
            case FrictionExperimentStep.ObservationTable:
                dataTablePanel?.SetActive(true);
                FrictionObservationTableManager.Instance?.Refresh();
                SetInstruction("Study TABLE 5.4. The values come from your recorded trials.");
                break;
            case FrictionExperimentStep.CompareResults:
                comparePanel?.SetActive(true);
                FillCompare();
                SetInstruction("Does the limiting friction change significantly when the surface area changes?");
                SetNextButtonVisible(false);
                break;
            case FrictionExperimentStep.Graph:
                graphPanel?.SetActive(true);
                FrictionGraphController.Instance?.ShowGraphs();
                SetInstruction("The graph uses your recorded values. Limiting friction stays nearly constant as area changes.");
                break;
            case FrictionExperimentStep.Questions:
                questionPanel?.SetActive(true);
                SetNextButtonVisible(true);
                SetNextLabel("CONTINUE");
                SetInstruction("Select an answer, then press CONTINUE.");
                break;
            case FrictionExperimentStep.VariableMatching:
                variablePanel?.SetActive(true);
                SetInstruction("Tap Independent, Dependent or Controlled for each quantity. Then press NEXT STEP.");
                SetNextButtonVisible(true);
                SetNextLabel("NEXT STEP");
                if (variableContinueBtn != null) variableContinueBtn.gameObject.SetActive(true);
                break;
            case FrictionExperimentStep.Conclusion:
                conclusionPanel?.SetActive(true);
                SetInstruction("Tap the phrases in the correct order, then press NEXT STEP.");
                SetNextButtonVisible(true);
                SetNextLabel("NEXT STEP");
                if (conclusionContinueBtn != null) conclusionContinueBtn.gameObject.SetActive(true);
                break;
            case FrictionExperimentStep.Complete:
                ShowResult();
                break;
        }

        UpdateProgress((int)step + 1, 12);
        if (stepText != null) stepText.text = StepTitle(step);
        if (step != FrictionExperimentStep.Objective) SetNextLabel("NEXT STEP");
        RefreshTrialLabels();
        UpdateLiveReadings();
    }

    private string SetupInstruction()
    {
        int trial = FrictionTrialManager.Instance != null ? FrictionTrialManager.Instance.CurrentTrial : 1;
        if (trial == 1) return "TRIAL 1 — SURFACE A (30 cm × 20 cm = 600 cm²). Place the largest surface of the block on the sandpaper.";
        if (trial == 2) return "TRIAL 2 — SURFACE B (30 cm × 10 cm = 300 cm²). Rotate the same block onto the medium surface.";
        return "TRIAL 3 — SURFACE C (20 cm × 10 cm = 200 cm²). Rotate the same block onto the smallest surface.";
    }

    private void FillCompare()
    {
        if (compareText == null) return;
        var tm = FrictionTrialManager.Instance;
        string Line(int n)
        {
            var t = tm != null ? tm.GetTrial(n) : null;
            if (t == null) return $"Surface {(char)('A' + n - 1)}: Area = — cm²    Limiting friction = — N";
            return $"Surface {t.surfaceName}: Area = {t.contactArea:0} cm²    Limiting friction = {(t.completed ? t.limitingFriction.ToString("0.0") : "—")} N";
        }
        compareText.text =
            "COMPARE RESULTS\n\n" +
            Line(1) + "\n" + Line(2) + "\n" + Line(3) + "\n\n" +
            "Does the limiting friction change significantly when the surface area changes?\n\n" +
            "A. Yes, it increases greatly.\n" +
            "B. Yes, it decreases greatly.\n" +
            "C. No, it remains approximately the same.";
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

    public void ShowNumericQuestion(int number, int total, string prompt)
    {
        ShowQuestion(number, total, prompt, "", "", "", "");
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
                "The limiting frictional force\n" +
                "does not significantly depend on\n" +
                "the area of contact\n" +
                "when the weight and surface roughness remain constant.\n\n" +
                "Your sentence:\n" + text;
    }

    public void UpdateLiveReadings()
    {
        var force = FrictionAppliedForceController.Instance;
        var friction = FrictionForceController.Instance;
        var block = WoodenBlockController.Instance;
        float applied = force != null ? force.AppliedForce : 0f;
        float f = friction != null ? friction.FrictionForce : 0f;
        bool moving = friction != null && friction.BlockMoving;
        string surface = block != null ? block.GetSurfaceName() : "A";
        string dim = block != null ? block.GetDimensions() : "30 cm × 20 cm";
        float area = block != null ? block.GetContactArea() : 600f;
        if (liveReadingsText != null)
        {
            liveReadingsText.text =
                $"Applied Force:  {applied:0.0} N\n" +
                $"Friction:  {f:0.0} N\n" +
                $"Block Status:  {(moving ? "MOVING" : "STATIONARY")}\n\n" +
                $"Weight:  60 N\n" +
                $"Surface {surface}:  {dim}\n" +
                $"Area:  {area:0} cm²\n" +
                "Roughness:  CONSTANT";
        }
        UpdateArrows(applied, f, moving);
        if (physicsText != null)
        {
            physicsText.text =
                "FRICTION\nFriction is a force that opposes the relative motion or tendency of motion between surfaces in contact.\n\n" +
                "LIMITING FRICTION\nThe limiting frictional force is the maximum frictional force acting just before the object begins to move.\n\n" +
                "Only the contact area is changed. Weight and sandpaper roughness stay constant.";
        }
        int trial = FrictionTrialManager.Instance != null ? FrictionTrialManager.Instance.CurrentTrial : 1;
        if (titleText != null)
            titleText.text = $"FRICTION     Activity: Surface Area Investigation     Attempt: {(FrictionAttemptManager.Instance != null ? FrictionAttemptManager.Instance.CurrentAttemptNumber : 1)}/3";
        if (stepText != null && (Current() == FrictionExperimentStep.Setup || Current() == FrictionExperimentStep.Pulling))
            stepText.text = $"Trial {trial} / 3";
    }

    private void UpdateArrows(float applied, float friction, bool moving)
    {
        if (forceArrow != null)
        {
            float w = Mathf.Lerp(40f, 180f, Mathf.Clamp01(applied / 25f));
            forceArrow.sizeDelta = new Vector2(w, forceArrow.sizeDelta.y);
        }
        if (frictionArrow != null)
        {
            float w = Mathf.Lerp(40f, 180f, Mathf.Clamp01(friction / 25f));
            frictionArrow.sizeDelta = new Vector2(w, frictionArrow.sizeDelta.y);
        }
    }

    public void RefreshTrialLabels() => UpdateLiveReadings();

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
        if (pullBtn != null) pullBtn.gameObject.SetActive(visible);
        if (stopPullBtn != null) stopPullBtn.gameObject.SetActive(visible);
        if (recordBtn != null) recordBtn.gameObject.SetActive(visible);
        if (resetRunBtn != null) resetRunBtn.gameObject.SetActive(visible);
        if (confirmSetupBtn != null) confirmSetupBtn.gameObject.SetActive(visible);
        if (surfaceABtn != null) surfaceABtn.gameObject.SetActive(visible);
        if (surfaceBBtn != null) surfaceBBtn.gameObject.SetActive(visible);
        if (surfaceCBtn != null) surfaceCBtn.gameObject.SetActive(visible);
    }

    private void HideAllContent()
    {
        introPanel?.SetActive(false);
        objectivePanel?.SetActive(false);
        equipmentPanel?.SetActive(false);
        laboratoryPanel?.SetActive(false);
        dataTablePanel?.SetActive(false);
        comparePanel?.SetActive(false);
        graphPanel?.SetActive(false);
        questionPanel?.SetActive(false);
        conclusionPanel?.SetActive(false);
        resultPanel?.SetActive(false);
        variablePanel?.SetActive(false);
        if (retryBtn != null) retryBtn.gameObject.SetActive(false);
    }

    private static string StepTitle(FrictionExperimentStep step)
    {
        switch (step)
        {
            case FrictionExperimentStep.Objective: return "Objectives";
            case FrictionExperimentStep.SelectEquipment: return "Equipment";
            case FrictionExperimentStep.Setup: return "Setup";
            case FrictionExperimentStep.Pulling: return "Pull Trial";
            case FrictionExperimentStep.ObservationTable: return "Table 5.4";
            case FrictionExperimentStep.CompareResults: return "Compare";
            case FrictionExperimentStep.Graph: return "Graphs";
            case FrictionExperimentStep.Questions: return "Questions";
            case FrictionExperimentStep.VariableMatching: return "Variables";
            case FrictionExperimentStep.Conclusion: return "Conclusion";
            case FrictionExperimentStep.Complete: return "Results";
            default: return "Introduction";
        }
    }

    private static FrictionExperimentStep Current()
    {
        return FrictionExperimentManager.Instance != null
            ? FrictionExperimentManager.Instance.CurrentStep
            : FrictionExperimentStep.Introduction;
    }
}

public class FrictionIntroClickToStart : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (FrictionUIManager.Instance != null) FrictionUIManager.Instance.StartPractical();
    }
}

public class FrictionHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private System.Action onDown;
    private System.Action onUp;
    private bool held;

    public void Configure(System.Action down, System.Action up)
    {
        onDown = down;
        onUp = up;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        held = true;
        onDown?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!held) return;
        held = false;
        onUp?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!held) return;
        held = false;
        onUp?.Invoke();
    }
}
