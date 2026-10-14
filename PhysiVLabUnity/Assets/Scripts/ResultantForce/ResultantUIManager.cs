using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResultantUIManager : MonoBehaviour
{
    public static ResultantUIManager Instance { get; private set; }

    private TextMeshProUGUI titleText, scoreText, progressText, attemptsText, stepText, instructionText;
    private Image progressBarFill;
    private GameObject introPanel, objectivePanel, instructionBar, equipmentPanel, laboratoryPanel;
    private GameObject dataTablePanel, comparePanel, graphPanel, questionPanel, conclusionPanel, resultPanel, variablePanel;
    private TextMeshProUGUI introText, objectiveText, conclusionText, compareText, questionText, dataTableText;
    private TextMeshProUGUI finalScoreText, resultDetailsText, statusText, liveReadingsText, physicsText, conclusionPreview;
    private Button startBtn, nextBtn, resetBtn, retryBtn, viewProfileBtn, viewResultsBtn;
    private Button recordBtn, confirmSetupBtn, resetRunBtn;
    private Button forceBPlus, forceBMinus, forceCPlus, forceCMinus;
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

    public void BindAll(ResultantUIRefs refs, bool showWelcome)
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
        recordBtn = refs.RecordBtn; confirmSetupBtn = refs.ConfirmSetupBtn; resetRunBtn = refs.ResetRunBtn;
        forceBPlus = refs.ForceBPlus; forceBMinus = refs.ForceBMinus;
        forceCPlus = refs.ForceCPlus; forceCMinus = refs.ForceCMinus;
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
            ResultantExperimentManager.Instance?.ResetExperiment();
            ShowIntro();
        });
        WireBtn(resetNo, () => resetConfirmPanel?.SetActive(false));
        WireBtn(retryBtn, () => { if (retryConfirmPanel != null) retryConfirmPanel.SetActive(true); });
        WireBtn(retryYes, () =>
        {
            retryConfirmPanel?.SetActive(false);
            ResultantExperimentManager.Instance?.RetryExperiment();
        });
        WireBtn(retryNo, () => retryConfirmPanel?.SetActive(false));
        WireBtn(viewProfileBtn, () =>
        {
            string summary = ResultantProfileManager.Instance != null ? ResultantProfileManager.Instance.GetProfileSummary() : "No profile data.";
            ResultantFeedbackManager.Instance?.ShowInstruction(summary);
        });
        WireBtn(viewResultsBtn, () => resultPanel?.SetActive(true));
        WireBtn(recordBtn, () => ResultantExperimentManager.Instance?.RecordReading());
        WireBtn(confirmSetupBtn, () => ResultantExperimentManager.Instance?.ConfirmSetup());
        WireBtn(resetRunBtn, () => ResultantExperimentManager.Instance?.ResetCurrentTrial());
        WireBtn(forceBPlus, () => ResultantExperimentManager.Instance?.ChangeForceB(1f));
        WireBtn(forceBMinus, () => ResultantExperimentManager.Instance?.ChangeForceB(-1f));
        WireBtn(forceCPlus, () => ResultantExperimentManager.Instance?.ChangeForceC(1f));
        WireBtn(forceCMinus, () => ResultantExperimentManager.Instance?.ChangeForceC(-1f));
        WireBtn(questionA, () => ResultantQuestionManager.Instance?.Answer(0));
        WireBtn(questionB, () => ResultantQuestionManager.Instance?.Answer(1));
        WireBtn(questionC, () => ResultantQuestionManager.Instance?.Answer(2));
        WireBtn(questionD, () => ResultantQuestionManager.Instance?.Answer(3));
        WireBtn(questionContinue, OnQuestionContinue);
        WireBtn(compareA, () => ResultantExperimentManager.Instance?.AnswerCompare(0));
        WireBtn(compareB, () => ResultantExperimentManager.Instance?.AnswerCompare(1));
        WireBtn(compareC, () => ResultantExperimentManager.Instance?.AnswerCompare(2));
        WireBtn(variableContinueBtn, () => ResultantExperimentManager.Instance?.CompleteVariableMatching());
        WireBtn(conclusionContinueBtn, () => ResultantExperimentManager.Instance?.CompleteConclusion());
        if (phraseButtons != null)
        {
            for (int i = 0; i < phraseButtons.Length; i++)
            {
                int idx = i;
                WireBtn(phraseButtons[i], () =>
                {
                    var tmp = phraseButtons[idx] != null ? phraseButtons[idx].GetComponentInChildren<TextMeshProUGUI>() : null;
                    if (tmp != null && !string.IsNullOrEmpty(tmp.text))
                        ResultantConclusionManager.Instance?.AddPhrase(tmp.text);
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

    public void StartPractical() => ResultantExperimentManager.Instance?.StartPractical();

    private void OnNextPressed()
    {
        var step = Current();
        if (step == ResultantExperimentStep.Introduction) { StartPractical(); return; }
        if (step == ResultantExperimentStep.SelectEquipment)
        {
            ResultantExperimentManager.Instance?.TryAdvanceFromEquipment();
            return;
        }
        if (step == ResultantExperimentStep.Questions)
        {
            OnQuestionContinue();
            return;
        }
        if (step == ResultantExperimentStep.VariableMatching)
        {
            ResultantExperimentManager.Instance?.CompleteVariableMatching();
            return;
        }
        if (step == ResultantExperimentStep.Conclusion)
        {
            ResultantExperimentManager.Instance?.CompleteConclusion();
            return;
        }
        ResultantExperimentManager.Instance?.AdvanceStep();
    }

    private void OnQuestionContinue()
    {
        if (ResultantQuestionManager.Instance == null)
        {
            ResultantExperimentManager.Instance?.CompleteQuestions();
            return;
        }
        if (ResultantQuestionManager.Instance.IsFinished)
        {
            ResultantExperimentManager.Instance?.CompleteQuestions();
            return;
        }
        if (!ResultantQuestionManager.Instance.HasAnswered)
        {
            ResultantFeedbackManager.Instance?.ShowInstruction("Select A, B, C or D first. Then press CONTINUE.");
            return;
        }
        bool done = ResultantQuestionManager.Instance.Advance();
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(false);
        if (done) ResultantExperimentManager.Instance?.CompleteQuestions();
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
                "RESULTANT FORCE\n\n" +
                "Activity 1 — Two forces acting on the trolley in the same direction\n\n" +
                "In this practical you will show that when two forces act on a trolley in the same direction, the resultant force is equal to their sum.\n\n" +
                "Apparatus: trolley, ring, two strings, two pulleys, three Newton balances (A, B and C).\n\n" +
                "Newton balances B and C pull the trolley through pulleys. Newton balance A is attached to the wall and measures the resultant force.\n\n" +
                "The rule you will test is:\nForce A  =  Force B  +  Force C";
        }
        UpdateAttemptsDisplay(ResultantAttemptManager.Instance != null ? ResultantAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateScoreDisplay(0);
        UpdateProgress(1, 12);
        if (stepText != null) stepText.text = "Introduction";
        if (titleText != null)
            titleText.text = "RESULTANT FORCE     Activity 1: Two forces in the same direction";
    }

    public void ShowStep(ResultantExperimentStep step)
    {
        HideAllContent();
        if (instructionBar != null) instructionBar.SetActive(step != ResultantExperimentStep.Introduction && step != ResultantExperimentStep.Complete);
        SetNextButtonVisible(step != ResultantExperimentStep.Assembly && step != ResultantExperimentStep.ApplyForces);
        SetLabControls(false);

        switch (step)
        {
            case ResultantExperimentStep.Introduction:
                ShowIntro();
                return;
            case ResultantExperimentStep.Objective:
                objectivePanel?.SetActive(true);
                if (objectiveText != null)
                    objectiveText.text =
                        "OBJECTIVES\n\nBy completing this practical you will:\n\n" +
                        "• Identify the apparatus needed to find a resultant force.\n" +
                        "• Assemble a trolley, ring, strings, pulleys and three Newton balances.\n" +
                        "• Apply two forces in the same direction using balances B and C.\n" +
                        "• Read the resultant force on balance A.\n" +
                        "• Show that Force A = Force B + Force C.\n" +
                        "• Record observations, answer questions and write a conclusion.";
                SetInstruction("Read the objectives, then press CONTINUE.");
                SetNextLabel("CONTINUE");
                break;
            case ResultantExperimentStep.SelectEquipment:
                equipmentPanel?.SetActive(true);
                ResultantEquipmentSelectionManager.Instance?.EnsureCardsVisible();
                SetInstruction("Select all equipment required for this practical. Drag or tap items into the REQUIRED EQUIPMENT AREA.");
                break;
            case ResultantExperimentStep.Assembly:
                laboratoryPanel?.SetActive(true);
                SetLabControls(true);
                SetInstruction(ResultantAssemblyManager.Instance != null ? ResultantAssemblyManager.Instance.NextHint() : "Assemble the apparatus.");
                break;
            case ResultantExperimentStep.ApplyForces:
                laboratoryPanel?.SetActive(true);
                SetLabControls(true);
                SetInstruction(ApplyInstruction());
                SetNextButtonVisible(false);
                break;
            case ResultantExperimentStep.ObservationTable:
                dataTablePanel?.SetActive(true);
                ResultantObservationTableManager.Instance?.Refresh();
                SetInstruction("Study the observation table. Check whether Force A equals Force B + Force C in every trial.");
                break;
            case ResultantExperimentStep.CompareResults:
                comparePanel?.SetActive(true);
                FillCompare();
                SetInstruction("Is Force A equal to Force B + Force C?");
                SetNextButtonVisible(false);
                break;
            case ResultantExperimentStep.Graph:
                graphPanel?.SetActive(true);
                ResultantGraphController.Instance?.ShowGraphs();
                SetInstruction("The graph uses your recorded values. A should match B + C in every trial.");
                break;
            case ResultantExperimentStep.Questions:
                questionPanel?.SetActive(true);
                SetNextButtonVisible(true);
                SetNextLabel("CONTINUE");
                SetInstruction("Select an answer, then press CONTINUE.");
                break;
            case ResultantExperimentStep.VariableMatching:
                variablePanel?.SetActive(true);
                SetInstruction("Tap Independent, Dependent or Controlled for each quantity. Then press NEXT STEP.");
                SetNextButtonVisible(true);
                SetNextLabel("NEXT STEP");
                if (variableContinueBtn != null) variableContinueBtn.gameObject.SetActive(true);
                break;
            case ResultantExperimentStep.Conclusion:
                conclusionPanel?.SetActive(true);
                SetInstruction("Tap the phrases in the correct order, then press NEXT STEP.");
                SetNextButtonVisible(true);
                SetNextLabel("NEXT STEP");
                if (conclusionContinueBtn != null) conclusionContinueBtn.gameObject.SetActive(true);
                break;
            case ResultantExperimentStep.Complete:
                ShowResult();
                break;
        }

        UpdateProgress((int)step + 1, 12);
        if (stepText != null) stepText.text = StepTitle(step);
        if (step != ResultantExperimentStep.Objective) SetNextLabel("NEXT STEP");
        RefreshTrialLabels();
        UpdateLiveReadings();
    }

    private string ApplyInstruction()
    {
        int trial = ResultantTrialManager.Instance != null ? ResultantTrialManager.Instance.CurrentTrial : 1;
        float b = ResultantTrialManager.Instance != null ? ResultantTrialManager.Instance.TargetB : 5f;
        float c = ResultantTrialManager.Instance != null ? ResultantTrialManager.Instance.TargetC : 3f;
        return $"TRIAL {trial} — Increase Force B to about {b:0} N and Force C to about {c:0} N. Then RECORD READING. Balance A should show B + C.";
    }

    private void FillCompare()
    {
        if (compareText == null) return;
        var tm = ResultantTrialManager.Instance;
        string Line(int n)
        {
            var t = tm != null ? tm.GetTrial(n) : null;
            if (t == null || !t.completed) return $"Trial {n}:  A = — N    B = — N    C = — N    B+C = — N";
            return $"Trial {n}:  A = {t.forceA:0.0} N    B = {t.forceB:0.0} N    C = {t.forceC:0.0} N    B+C = {(t.forceB + t.forceC):0.0} N";
        }
        compareText.text =
            "COMPARE RESULTS\n\n" +
            Line(1) + "\n" + Line(2) + "\n" + Line(3) + "\n\n" +
            "Is Force A equal to Force B + Force C?\n\n" +
            "A. No, A is always smaller than B + C.\n" +
            "B. No, A is always larger than B + C.\n" +
            "C. Yes, A = B + C when the two forces act in the same direction.";
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
                "The resultant force\n" +
                "is equal to the sum of\n" +
                "the two forces acting\n" +
                "in the same direction.\n\n" +
                "Your sentence:\n" + text;
    }

    public void UpdateLiveReadings()
    {
        var force = ResultantForceController.Instance;
        float a = force != null ? force.ForceA : 0f;
        float b = force != null ? force.ForceB : 0f;
        float c = force != null ? force.ForceC : 0f;
        int trial = ResultantTrialManager.Instance != null ? ResultantTrialManager.Instance.CurrentTrial : 1;
        float tb = ResultantTrialManager.Instance != null ? ResultantTrialManager.Instance.TargetB : 5f;
        float tc = ResultantTrialManager.Instance != null ? ResultantTrialManager.Instance.TargetC : 3f;
        if (liveReadingsText != null)
        {
            liveReadingsText.text =
                $"Balance A (resultant):  {a:0.0} N\n" +
                $"Balance B:  {b:0.0} N\n" +
                $"Balance C:  {c:0.0} N\n\n" +
                $"B + C = {b + c:0.0} N\n\n" +
                $"Trial {trial} target\n" +
                $"B ≈ {tb:0} N    C ≈ {tc:0} N";
        }
        if (physicsText != null)
        {
            physicsText.text =
                "RESULTANT FORCE\nThe resultant force is the single force that has the same effect as two or more forces acting together.\n\n" +
                "SAME DIRECTION\nWhen two forces act in the same direction, the resultant is their sum:\nA = B + C\n\n" +
                "Balance A is attached to the wall. Balances B and C pull the trolley through pulleys, so both forces act the same way.";
        }
        if (titleText != null)
            titleText.text = $"RESULTANT FORCE     Activity 1     Attempt: {(ResultantAttemptManager.Instance != null ? ResultantAttemptManager.Instance.CurrentAttemptNumber : 1)}/3";
        if (stepText != null && (Current() == ResultantExperimentStep.Assembly || Current() == ResultantExperimentStep.ApplyForces))
            stepText.text = Current() == ResultantExperimentStep.Assembly ? "Assembly" : $"Trial {trial} / 3";
        ResultantVisualController.Instance?.RefreshReadings();
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
        bool assembly = Current() == ResultantExperimentStep.Assembly;
        bool apply = Current() == ResultantExperimentStep.ApplyForces;
        if (confirmSetupBtn != null) confirmSetupBtn.gameObject.SetActive(visible && assembly);
        if (recordBtn != null) recordBtn.gameObject.SetActive(visible && apply);
        if (resetRunBtn != null) resetRunBtn.gameObject.SetActive(visible && apply);
        if (forceBPlus != null) forceBPlus.gameObject.SetActive(visible && apply);
        if (forceBMinus != null) forceBMinus.gameObject.SetActive(visible && apply);
        if (forceCPlus != null) forceCPlus.gameObject.SetActive(visible && apply);
        if (forceCMinus != null) forceCMinus.gameObject.SetActive(visible && apply);
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

    private static string StepTitle(ResultantExperimentStep step)
    {
        switch (step)
        {
            case ResultantExperimentStep.Objective: return "Objectives";
            case ResultantExperimentStep.SelectEquipment: return "Equipment";
            case ResultantExperimentStep.Assembly: return "Assembly";
            case ResultantExperimentStep.ApplyForces: return "Apply Forces";
            case ResultantExperimentStep.ObservationTable: return "Observations";
            case ResultantExperimentStep.CompareResults: return "Compare";
            case ResultantExperimentStep.Graph: return "Graphs";
            case ResultantExperimentStep.Questions: return "Questions";
            case ResultantExperimentStep.VariableMatching: return "Variables";
            case ResultantExperimentStep.Conclusion: return "Conclusion";
            case ResultantExperimentStep.Complete: return "Results";
            default: return "Introduction";
        }
    }

    private static ResultantExperimentStep Current()
    {
        return ResultantExperimentManager.Instance != null
            ? ResultantExperimentManager.Instance.CurrentStep
            : ResultantExperimentStep.Introduction;
    }
}

public class ResultantIntroClickToStart : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (ResultantUIManager.Instance != null) ResultantUIManager.Instance.StartPractical();
    }
}
