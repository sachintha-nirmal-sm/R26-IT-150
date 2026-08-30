using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TurningUIManager : MonoBehaviour
{
    public static TurningUIManager Instance { get; private set; }

    private TextMeshProUGUI titleText, scoreText, progressText, attemptsText, stepText, instructionText;
    private Image progressBarFill;
    private GameObject introPanel, objectivePanel, instructionBar, equipmentPanel, laboratoryPanel;
    private GameObject dataTablePanel, comparePanel, graphPanel, questionPanel, conclusionPanel, resultPanel, variablePanel;
    private TextMeshProUGUI introText, objectiveText, conclusionText, compareText, questionText, dataTableText;
    private TextMeshProUGUI finalScoreText, resultDetailsText, statusText, liveReadingsText, physicsText, conclusionPreview;
    private Button startBtn, nextBtn, resetBtn, retryBtn, viewProfileBtn, viewResultsBtn;
    private Button recordBtn, confirmSetupBtn, resetRunBtn, tightenBtn;
    private Button forcePlus, forceMinus, anglePlus, angleMinus;
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

    public void BindAll(TurningUIRefs refs, bool showWelcome)
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
        tightenBtn = refs.TightenBtn;
        forcePlus = refs.ForcePlus; forceMinus = refs.ForceMinus;
        anglePlus = refs.AnglePlus; angleMinus = refs.AngleMinus;
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
            TurningExperimentManager.Instance?.ResetExperiment();
            ShowIntro();
        });
        WireBtn(resetNo, () => resetConfirmPanel?.SetActive(false));
        WireBtn(retryBtn, () => { if (retryConfirmPanel != null) retryConfirmPanel.SetActive(true); });
        WireBtn(retryYes, () =>
        {
            retryConfirmPanel?.SetActive(false);
            TurningExperimentManager.Instance?.RetryExperiment();
        });
        WireBtn(retryNo, () => retryConfirmPanel?.SetActive(false));
        WireBtn(viewProfileBtn, () =>
        {
            string summary = TurningProfileManager.Instance != null ? TurningProfileManager.Instance.GetProfileSummary() : "No profile data.";
            TurningFeedbackManager.Instance?.ShowInstruction(summary);
        });
        WireBtn(viewResultsBtn, () => resultPanel?.SetActive(true));
        WireBtn(recordBtn, () => TurningExperimentManager.Instance?.RecordReading());
        WireBtn(confirmSetupBtn, () => TurningExperimentManager.Instance?.ConfirmSetup());
        WireBtn(resetRunBtn, () => TurningExperimentManager.Instance?.ResetCurrentTrial());
        WireBtn(tightenBtn, () => TurningExperimentManager.Instance?.TightenScrew());
        WireBtn(forcePlus, () => TurningExperimentManager.Instance?.ChangeForce(1f));
        WireBtn(forceMinus, () => TurningExperimentManager.Instance?.ChangeForce(-1f));
        WireBtn(anglePlus, () => TurningExperimentManager.Instance?.ChangeAngle(1f));
        WireBtn(angleMinus, () => TurningExperimentManager.Instance?.ChangeAngle(-1f));
        WireBtn(questionA, () => TurningQuestionManager.Instance?.Answer(0));
        WireBtn(questionB, () => TurningQuestionManager.Instance?.Answer(1));
        WireBtn(questionC, () => TurningQuestionManager.Instance?.Answer(2));
        WireBtn(questionD, () => TurningQuestionManager.Instance?.Answer(3));
        WireBtn(questionContinue, OnQuestionContinue);
        WireBtn(compareA, () => TurningExperimentManager.Instance?.AnswerCompare(0));
        WireBtn(compareB, () => TurningExperimentManager.Instance?.AnswerCompare(1));
        WireBtn(compareC, () => TurningExperimentManager.Instance?.AnswerCompare(2));
        WireBtn(variableContinueBtn, () => TurningExperimentManager.Instance?.CompleteVariableMatching());
        WireBtn(conclusionContinueBtn, () => TurningExperimentManager.Instance?.CompleteConclusion());
        if (phraseButtons != null)
        {
            for (int i = 0; i < phraseButtons.Length; i++)
            {
                int idx = i;
                WireBtn(phraseButtons[i], () =>
                {
                    var tmp = phraseButtons[idx] != null ? phraseButtons[idx].GetComponentInChildren<TextMeshProUGUI>() : null;
                    if (tmp != null && !string.IsNullOrEmpty(tmp.text))
                        TurningConclusionManager.Instance?.AddPhrase(tmp.text);
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

    public void StartPractical() => TurningExperimentManager.Instance?.StartPractical();

    private void OnNextPressed()
    {
        var step = Current();
        if (step == TurningExperimentStep.Introduction) { StartPractical(); return; }
        if (step == TurningExperimentStep.SelectEquipment)
        {
            TurningExperimentManager.Instance?.TryAdvanceFromEquipment();
            return;
        }
        if (step == TurningExperimentStep.Questions)
        {
            OnQuestionContinue();
            return;
        }
        if (step == TurningExperimentStep.VariableMatching)
        {
            TurningExperimentManager.Instance?.CompleteVariableMatching();
            return;
        }
        if (step == TurningExperimentStep.Conclusion)
        {
            TurningExperimentManager.Instance?.CompleteConclusion();
            return;
        }
        TurningExperimentManager.Instance?.AdvanceStep();
    }

    private void OnQuestionContinue()
    {
        if (TurningQuestionManager.Instance == null)
        {
            TurningExperimentManager.Instance?.CompleteQuestions();
            return;
        }
        if (TurningQuestionManager.Instance.IsFinished)
        {
            TurningExperimentManager.Instance?.CompleteQuestions();
            return;
        }
        if (!TurningQuestionManager.Instance.HasAnswered)
        {
            TurningFeedbackManager.Instance?.ShowInstruction("Select A, B, C or D first. Then press CONTINUE.");
            return;
        }
        bool done = TurningQuestionManager.Instance.Advance();
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(false);
        if (done) TurningExperimentManager.Instance?.CompleteQuestions();
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
                "TURNING EFFECT OF A FORCE\n\n" +
                "Activity 2 — Investigating the turning effect of a force\n\n" +
                "A fairly long calibrated stick is clamped to a table at point O with a screw nail and two rubber washers. Holes A, B, C and D are drilled 15 cm apart. Wire loops are attached so a Newton balance can be hooked on.\n\n" +
                "You will pull the Newton balance at D, perpendicular to the stick, and find the minimum force that just starts the stick turning. Then you tighten the screw half a turn and repeat.\n\n" +
                "The rule you will test is:\nMoment (turning effect)  =  Force  ×  perpendicular distance from the pivot";
        }
        UpdateAttemptsDisplay(TurningAttemptManager.Instance != null ? TurningAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateScoreDisplay(0);
        UpdateProgress(1, 12);
        if (stepText != null) stepText.text = "Introduction";
        if (titleText != null)
            titleText.text = "TURNING EFFECT OF A FORCE     Activity 2";
    }

    public void ShowStep(TurningExperimentStep step)
    {
        HideAllContent();
        if (instructionBar != null) instructionBar.SetActive(step != TurningExperimentStep.Introduction && step != TurningExperimentStep.Complete);
        SetNextButtonVisible(step != TurningExperimentStep.Assembly && step != TurningExperimentStep.ApplyForce);
        SetLabControls(false);

        switch (step)
        {
            case TurningExperimentStep.Introduction:
                ShowIntro();
                return;
            case TurningExperimentStep.Objective:
                objectivePanel?.SetActive(true);
                if (objectiveText != null)
                    objectiveText.text =
                        "OBJECTIVES\n\nBy completing this practical you will:\n\n" +
                        "• Identify the apparatus needed to investigate turning effect.\n" +
                        "• Drill holes at O, A, B, C and D, 15 cm apart.\n" +
                        "• Clamp the stick at O with a screw nail and two rubber washers.\n" +
                        "• Attach wire loops and pull a Newton balance perpendicular to the stick at D.\n" +
                        "• Measure the minimum force that just turns the stick at three tightness values.\n" +
                        "• Show that moment = force × perpendicular distance, and that tightening the pivot increases the force needed.";
                SetInstruction("Read the objectives, then press CONTINUE.");
                SetNextLabel("CONTINUE");
                break;
            case TurningExperimentStep.SelectEquipment:
                equipmentPanel?.SetActive(true);
                TurningEquipmentSelectionManager.Instance?.EnsureCardsVisible();
                SetInstruction("Select all equipment required for this practical. Drag or tap items into the REQUIRED EQUIPMENT AREA.");
                break;
            case TurningExperimentStep.Assembly:
                laboratoryPanel?.SetActive(true);
                SetLabControls(true);
                SetInstruction(TurningAssemblyManager.Instance != null ? TurningAssemblyManager.Instance.NextHint() : "Assemble the apparatus.");
                break;
            case TurningExperimentStep.ApplyForce:
                laboratoryPanel?.SetActive(true);
                SetLabControls(true);
                SetInstruction(ApplyInstruction());
                SetNextButtonVisible(false);
                break;
            case TurningExperimentStep.ObservationTable:
                dataTablePanel?.SetActive(true);
                TurningObservationTableManager.Instance?.Refresh();
                SetInstruction("Study the observation table. The force at D should increase as the screw is tightened.");
                break;
            case TurningExperimentStep.CompareResults:
                comparePanel?.SetActive(true);
                FillCompare();
                SetInstruction("What happens to the force needed when the screw is tightened?");
                SetNextButtonVisible(false);
                break;
            case TurningExperimentStep.Graph:
                graphPanel?.SetActive(true);
                TurningGraphController.Instance?.ShowGraphs();
                SetInstruction("The graphs use your recorded values. Force and moment increase with tightness.");
                break;
            case TurningExperimentStep.Questions:
                questionPanel?.SetActive(true);
                SetNextButtonVisible(true);
                SetNextLabel("CONTINUE");
                SetInstruction("Select an answer, then press CONTINUE.");
                break;
            case TurningExperimentStep.VariableMatching:
                variablePanel?.SetActive(true);
                SetInstruction("Tap Independent, Dependent or Controlled for each quantity. Then press NEXT STEP.");
                SetNextButtonVisible(true);
                SetNextLabel("NEXT STEP");
                if (variableContinueBtn != null) variableContinueBtn.gameObject.SetActive(true);
                break;
            case TurningExperimentStep.Conclusion:
                conclusionPanel?.SetActive(true);
                SetInstruction("Tap the phrases in the correct order, then press NEXT STEP.");
                SetNextButtonVisible(true);
                SetNextLabel("NEXT STEP");
                if (conclusionContinueBtn != null) conclusionContinueBtn.gameObject.SetActive(true);
                break;
            case TurningExperimentStep.Complete:
                ShowResult();
                break;
        }

        UpdateProgress((int)step + 1, 12);
        if (stepText != null) stepText.text = StepTitle(step);
        if (step != TurningExperimentStep.Objective) SetNextLabel("NEXT STEP");
        RefreshTrialLabels();
        UpdateLiveReadings();
    }

    private string ApplyInstruction()
    {
        int trial = TurningTrialManager.Instance != null ? TurningTrialManager.Instance.CurrentTrial : 1;
        float target = TurningTrialManager.Instance != null ? TurningTrialManager.Instance.TargetForceN : 1.5f;
        if (trial == 1)
            return "TRIAL 1 — Hook the Newton balance at D. Set the angle to 90°. Increase the force until the stick just turns, then RECORD READING.";
        return $"TRIAL {trial} — Tighten the screw half a turn, then pull perpendicular at D until the stick just turns (about {target:0.0} N). RECORD READING.";
    }

    private void FillCompare()
    {
        if (compareText == null) return;
        var tm = TurningTrialManager.Instance;
        string Line(int n)
        {
            var t = tm != null ? tm.GetTrial(n) : null;
            if (t == null || !t.completed) return $"Trial {n}:  tightness {n}    F = — N    moment = — N m";
            return $"Trial {n}:  tightness {t.tightnessLevel}    F = {t.forceN:0.0} N    angle = {t.angleDeg:0}°    moment = {t.momentNm:0.00} N m";
        }
        compareText.text =
            "COMPARE RESULTS\n\n" +
            Line(1) + "\n" + Line(2) + "\n" + Line(3) + "\n\n" +
            "What happens to the force needed to turn the stick when the screw nail is tightened?\n\n" +
            "A. The force becomes smaller.\n" +
            "B. The force stays the same.\n" +
            "C. The force becomes larger because friction at the pivot increases.";
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
                "The turning effect of a force\n" +
                "is the moment, equal to force\n" +
                "times the perpendicular distance\n" +
                "from the pivot.\n\n" +
                "Your sentence:\n" + text;
    }

    public void UpdateLiveReadings()
    {
        var mom = TurningMomentController.Instance;
        float f = mom != null ? mom.ForceN : 0f;
        float ang = mom != null ? mom.AngleDeg : 90f;
        float m = mom != null ? mom.MomentNm : 0f;
        string point = mom != null && mom.BalanceAttached ? mom.AttachedPoint : "—";
        int tightness = mom != null ? mom.TightnessLevel : 1;
        int trial = TurningTrialManager.Instance != null ? TurningTrialManager.Instance.CurrentTrial : 1;
        bool moving = mom != null && mom.StickJustMoves;
        if (liveReadingsText != null)
        {
            liveReadingsText.text =
                $"Newton balance:  {f:0.0} N\n" +
                $"Angle to stick:  {ang:0}°\n" +
                $"Hooked at:  {point}  ({(mom != null ? mom.DistanceCm : 0f):0} cm)\n" +
                $"Tightness:  {tightness}\n\n" +
                $"Moment = F⊥ × d\n" +
                $"= {m:0.00} N m\n\n" +
                (moving ? "Stick: JUST TURNING" : "Stick: not yet turning") +
                $"\n\nTrial {trial} / 3";
        }
        if (physicsText != null)
        {
            physicsText.text =
                "TURNING EFFECT (MOMENT)\nMoment = Force × perpendicular distance from the pivot.\n\n" +
                "PERPENDICULAR PULL\nOnly the component of force at 90° to the stick produces a turning effect.\n\n" +
                "TIGHTENING THE SCREW\nA tighter screw increases friction at O. A larger moment is then needed to start the stick turning.";
        }
        if (titleText != null)
            titleText.text = $"TURNING EFFECT OF A FORCE     Activity 2     Attempt: {(TurningAttemptManager.Instance != null ? TurningAttemptManager.Instance.CurrentAttemptNumber : 1)}/3";
        if (stepText != null && (Current() == TurningExperimentStep.Assembly || Current() == TurningExperimentStep.ApplyForce))
            stepText.text = Current() == TurningExperimentStep.Assembly ? "Assembly" : $"Trial {trial} / 3";
        TurningVisualController.Instance?.RefreshReadings();
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
        bool assembly = Current() == TurningExperimentStep.Assembly;
        bool apply = Current() == TurningExperimentStep.ApplyForce;
        if (confirmSetupBtn != null) confirmSetupBtn.gameObject.SetActive(visible && assembly);
        if (recordBtn != null) recordBtn.gameObject.SetActive(visible && apply);
        if (resetRunBtn != null) resetRunBtn.gameObject.SetActive(visible && apply);
        if (tightenBtn != null) tightenBtn.gameObject.SetActive(visible && apply);
        if (forcePlus != null) forcePlus.gameObject.SetActive(visible && apply);
        if (forceMinus != null) forceMinus.gameObject.SetActive(visible && apply);
        if (anglePlus != null) anglePlus.gameObject.SetActive(visible && apply);
        if (angleMinus != null) angleMinus.gameObject.SetActive(visible && apply);
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

    private static string StepTitle(TurningExperimentStep step)
    {
        switch (step)
        {
            case TurningExperimentStep.Objective: return "Objectives";
            case TurningExperimentStep.SelectEquipment: return "Equipment";
            case TurningExperimentStep.Assembly: return "Assembly";
            case TurningExperimentStep.ApplyForce: return "Apply Force";
            case TurningExperimentStep.ObservationTable: return "Observations";
            case TurningExperimentStep.CompareResults: return "Compare";
            case TurningExperimentStep.Graph: return "Graphs";
            case TurningExperimentStep.Questions: return "Questions";
            case TurningExperimentStep.VariableMatching: return "Variables";
            case TurningExperimentStep.Conclusion: return "Conclusion";
            case TurningExperimentStep.Complete: return "Results";
            default: return "Introduction";
        }
    }

    private static TurningExperimentStep Current()
    {
        return TurningExperimentManager.Instance != null
            ? TurningExperimentManager.Instance.CurrentStep
            : TurningExperimentStep.Introduction;
    }
}

public class TurningIntroClickToStart : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (TurningUIManager.Instance != null) TurningUIManager.Instance.StartPractical();
    }
}
