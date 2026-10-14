using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquilibriumUIManager : MonoBehaviour
{
    public static EquilibriumUIManager Instance { get; private set; }

    private TextMeshProUGUI titleText, scoreText, progressText, attemptsText, stepText, instructionText;
    private Image progressBarFill;
    private GameObject introPanel, objectivePanel, instructionBar, equipmentPanel, laboratoryPanel;
    private GameObject dataTablePanel, comparePanel, graphPanel, questionPanel, conclusionPanel, resultPanel, variablePanel;
    private TextMeshProUGUI introText, objectiveText, conclusionText, compareText, questionText, dataTableText;
    private TextMeshProUGUI finalScoreText, resultDetailsText, statusText, liveReadingsText, physicsText, conclusionPreview;
    private Button startBtn, nextBtn, resetBtn, retryBtn, viewProfileBtn, viewResultsBtn;
    private Button recordBtn, confirmSetupBtn, resetRunBtn, weighBtn, hangLeftBtn, hangRightBtn;
    private Button tiltPlus, tiltMinus;
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

    public void BindAll(EquilibriumUIRefs refs, bool showWelcome)
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
        weighBtn = refs.WeighBtn; hangLeftBtn = refs.HangLeftBtn; hangRightBtn = refs.HangRightBtn;
        tiltPlus = refs.TiltPlus; tiltMinus = refs.TiltMinus;
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
            EquilibriumExperimentManager.Instance?.ResetExperiment();
            ShowIntro();
        });
        WireBtn(resetNo, () => resetConfirmPanel?.SetActive(false));
        WireBtn(retryBtn, () => { if (retryConfirmPanel != null) retryConfirmPanel.SetActive(true); });
        WireBtn(retryYes, () =>
        {
            retryConfirmPanel?.SetActive(false);
            EquilibriumExperimentManager.Instance?.RetryExperiment();
        });
        WireBtn(retryNo, () => retryConfirmPanel?.SetActive(false));
        WireBtn(viewProfileBtn, () =>
        {
            string summary = EquilibriumProfileManager.Instance != null ? EquilibriumProfileManager.Instance.GetProfileSummary() : "No profile data.";
            EquilibriumFeedbackManager.Instance?.ShowInstruction(summary);
        });
        WireBtn(viewResultsBtn, () => resultPanel?.SetActive(true));
        WireBtn(recordBtn, () => EquilibriumExperimentManager.Instance?.RecordReading());
        WireBtn(confirmSetupBtn, () => EquilibriumExperimentManager.Instance?.ConfirmSetup());
        WireBtn(resetRunBtn, () => EquilibriumExperimentManager.Instance?.ResetCurrentTrial());
        WireBtn(weighBtn, () => EquilibriumExperimentManager.Instance?.WeighRuler());
        WireBtn(hangLeftBtn, () => EquilibriumExperimentManager.Instance?.HangLeft());
        WireBtn(hangRightBtn, () => EquilibriumExperimentManager.Instance?.HangRight());
        WireBtn(tiltPlus, () => EquilibriumExperimentManager.Instance?.ChangeTilt(2f));
        WireBtn(tiltMinus, () => EquilibriumExperimentManager.Instance?.ChangeTilt(-2f));
        WireBtn(questionA, () => EquilibriumQuestionManager.Instance?.Answer(0));
        WireBtn(questionB, () => EquilibriumQuestionManager.Instance?.Answer(1));
        WireBtn(questionC, () => EquilibriumQuestionManager.Instance?.Answer(2));
        WireBtn(questionD, () => EquilibriumQuestionManager.Instance?.Answer(3));
        WireBtn(questionContinue, OnQuestionContinue);
        WireBtn(compareA, () => EquilibriumExperimentManager.Instance?.AnswerCompare(0));
        WireBtn(compareB, () => EquilibriumExperimentManager.Instance?.AnswerCompare(1));
        WireBtn(compareC, () => EquilibriumExperimentManager.Instance?.AnswerCompare(2));
        WireBtn(variableContinueBtn, () => EquilibriumExperimentManager.Instance?.CompleteVariableMatching());
        WireBtn(conclusionContinueBtn, () => EquilibriumExperimentManager.Instance?.CompleteConclusion());
        if (phraseButtons != null)
        {
            for (int i = 0; i < phraseButtons.Length; i++)
            {
                int idx = i;
                WireBtn(phraseButtons[i], () =>
                {
                    var tmp = phraseButtons[idx] != null ? phraseButtons[idx].GetComponentInChildren<TextMeshProUGUI>() : null;
                    if (tmp != null && !string.IsNullOrEmpty(tmp.text))
                        EquilibriumConclusionManager.Instance?.AddPhrase(tmp.text);
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

    public void StartPractical() => EquilibriumExperimentManager.Instance?.StartPractical();

    private void OnNextPressed()
    {
        var step = Current();
        if (step == EquilibriumExperimentStep.Introduction) { StartPractical(); return; }
        if (step == EquilibriumExperimentStep.SelectEquipment)
        {
            EquilibriumExperimentManager.Instance?.TryAdvanceFromEquipment();
            return;
        }
        if (step == EquilibriumExperimentStep.Questions)
        {
            OnQuestionContinue();
            return;
        }
        if (step == EquilibriumExperimentStep.VariableMatching)
        {
            EquilibriumExperimentManager.Instance?.CompleteVariableMatching();
            return;
        }
        if (step == EquilibriumExperimentStep.Conclusion)
        {
            EquilibriumExperimentManager.Instance?.CompleteConclusion();
            return;
        }
        EquilibriumExperimentManager.Instance?.AdvanceStep();
    }

    private void OnQuestionContinue()
    {
        if (EquilibriumQuestionManager.Instance == null)
        {
            EquilibriumExperimentManager.Instance?.CompleteQuestions();
            return;
        }
        if (EquilibriumQuestionManager.Instance.IsFinished)
        {
            EquilibriumExperimentManager.Instance?.CompleteQuestions();
            return;
        }
        if (!EquilibriumQuestionManager.Instance.HasAnswered)
        {
            EquilibriumFeedbackManager.Instance?.ShowInstruction("Select A, B, C or D first. Then press CONTINUE.");
            return;
        }
        bool done = EquilibriumQuestionManager.Instance.Advance();
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(false);
        if (done) EquilibriumExperimentManager.Instance?.CompleteQuestions();
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
                "EQUILIBRIUM OF FORCES\n\n" +
                "Activity — Equilibrium of a meter ruler under three coplanar parallel forces\n\n" +
                "A meter ruler is suspended at its two ends by rubber bands hooked onto two vertical spring balances F1 and F2. The weight W of the ruler acts downward at its centre of gravity.\n\n" +
                "You will first measure W, then hang the ruler horizontally from both balances and read F1 and F2.\n\n" +
                "The rule you will test is:\nF1 + F2  =  W";
        }
        UpdateAttemptsDisplay(EquilibriumAttemptManager.Instance != null ? EquilibriumAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateScoreDisplay(0);
        UpdateProgress(1, 12);
        if (stepText != null) stepText.text = "Introduction";
        if (titleText != null)
            titleText.text = "EQUILIBRIUM OF FORCES     Activity";
    }

    public void ShowStep(EquilibriumExperimentStep step)
    {
        HideAllContent();
        if (instructionBar != null) instructionBar.SetActive(step != EquilibriumExperimentStep.Introduction && step != EquilibriumExperimentStep.Complete);
        SetNextButtonVisible(step != EquilibriumExperimentStep.Assembly && step != EquilibriumExperimentStep.MeasureWeight && step != EquilibriumExperimentStep.Equilibrium);
        SetLabControls(false);

        switch (step)
        {
            case EquilibriumExperimentStep.Introduction:
                ShowIntro();
                return;
            case EquilibriumExperimentStep.Objective:
                objectivePanel?.SetActive(true);
                if (objectiveText != null)
                    objectiveText.text =
                        "OBJECTIVES\n\nBy completing this practical you will:\n\n" +
                        "• Identify the apparatus needed for equilibrium of a meter ruler.\n" +
                        "• Measure the weight W of the meter ruler with a spring balance.\n" +
                        "• Suspend the ruler at its two ends using rubber bands and two spring balances.\n" +
                        "• Adjust the ruler until it is horizontal (in equilibrium).\n" +
                        "• Read F1 and F2 and show that F1 + F2 = W.\n" +
                        "• Keep the three coplanar parallel forces in the same vertical plane.";
                SetInstruction("Read the objectives, then press CONTINUE.");
                SetNextLabel("CONTINUE");
                break;
            case EquilibriumExperimentStep.SelectEquipment:
                equipmentPanel?.SetActive(true);
                EquilibriumEquipmentSelectionManager.Instance?.EnsureCardsVisible();
                SetInstruction("Select all equipment required for this practical. Drag or tap items into the REQUIRED EQUIPMENT AREA.");
                break;
            case EquilibriumExperimentStep.Assembly:
                laboratoryPanel?.SetActive(true);
                SetLabControls(true);
                SetInstruction(EquilibriumAssemblyManager.Instance != null ? EquilibriumAssemblyManager.Instance.NextHint() : "Assemble the apparatus.");
                break;
            case EquilibriumExperimentStep.MeasureWeight:
                laboratoryPanel?.SetActive(true);
                SetLabControls(true);
                SetInstruction("HANG RULER ON F1 to measure its weight W, then press RECORD READING.");
                SetNextButtonVisible(EquilibriumForceController.Instance != null && EquilibriumForceController.Instance.WeightRecorded);
                break;
            case EquilibriumExperimentStep.Equilibrium:
                laboratoryPanel?.SetActive(true);
                SetLabControls(true);
                SetInstruction(ApplyInstruction());
                SetNextButtonVisible(false);
                break;
            case EquilibriumExperimentStep.ObservationTable:
                dataTablePanel?.SetActive(true);
                EquilibriumObservationTableManager.Instance?.Refresh();
                SetInstruction("Study the observation table. F1 + F2 should equal W when the ruler is horizontal.");
                break;
            case EquilibriumExperimentStep.CompareResults:
                comparePanel?.SetActive(true);
                FillCompare();
                SetInstruction("What is true when the meter ruler is in horizontal equilibrium?");
                SetNextButtonVisible(false);
                break;
            case EquilibriumExperimentStep.Graph:
                graphPanel?.SetActive(true);
                EquilibriumGraphController.Instance?.ShowGraphs();
                SetInstruction("The graphs use your recorded values. F1 + F2 should match W.");
                break;
            case EquilibriumExperimentStep.Questions:
                questionPanel?.SetActive(true);
                SetNextButtonVisible(true);
                SetNextLabel("CONTINUE");
                SetInstruction("Select an answer, then press CONTINUE.");
                break;
            case EquilibriumExperimentStep.VariableMatching:
                variablePanel?.SetActive(true);
                SetInstruction("Tap Independent, Dependent or Controlled for each quantity. Then press NEXT STEP.");
                SetNextButtonVisible(true);
                SetNextLabel("NEXT STEP");
                if (variableContinueBtn != null) variableContinueBtn.gameObject.SetActive(true);
                break;
            case EquilibriumExperimentStep.Conclusion:
                conclusionPanel?.SetActive(true);
                SetInstruction("Tap the phrases in the correct order, then press NEXT STEP.");
                SetNextButtonVisible(true);
                SetNextLabel("NEXT STEP");
                if (conclusionContinueBtn != null) conclusionContinueBtn.gameObject.SetActive(true);
                break;
            case EquilibriumExperimentStep.Complete:
                ShowResult();
                break;
        }

        UpdateProgress((int)step + 1, 12);
        if (stepText != null) stepText.text = StepTitle(step);
        if (step != EquilibriumExperimentStep.Objective) SetNextLabel("NEXT STEP");
        RefreshTrialLabels();
        UpdateLiveReadings();
    }

    private string ApplyInstruction()
    {
        int trial = EquilibriumTrialManager.Instance != null ? EquilibriumTrialManager.Instance.CurrentTrial : 1;
        return $"TRIAL {trial} — Hang both ends, LEVEL the ruler until it is horizontal, then RECORD READING (F1 and F2).";
    }

    private void FillCompare()
    {
        if (compareText == null) return;
        var tm = EquilibriumTrialManager.Instance;
        string Line(int n)
        {
            var t = tm != null ? tm.GetTrial(n) : null;
            if (t == null || !t.completed) return $"Trial {n}:  F1 = — N    F2 = — N    F1+F2 = — N    W = — N";
            return $"Trial {n}:  F1 = {t.force1N:0.00} N    F2 = {t.force2N:0.00} N    F1+F2 = {t.sumN:0.00} N    W = {t.weightN:0.00} N";
        }
        compareText.text =
            "COMPARE RESULTS\n\n" +
            Line(1) + "\n" + Line(2) + "\n" + Line(3) + "\n\n" +
            "When the meter ruler is in horizontal equilibrium, what is true?\n\n" +
            "A. F1 is always much larger than F2.\n" +
            "B. F1 + F2 is less than W because some force is lost.\n" +
            "C. F1 + F2 equals W, because the two upward forces balance the weight.";
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
                "For a meter ruler in equilibrium\n" +
                "under three coplanar parallel forces,\n" +
                "the two upward forces F1 and F2\n" +
                "add up to the weight W of the ruler.\n\n" +
                "Your sentence:\n" + text;
    }

    public void UpdateLiveReadings()
    {
        var force = EquilibriumForceController.Instance;
        float f1 = force != null ? force.Force1N : 0f;
        float f2 = force != null ? force.Force2N : 0f;
        float w = force != null && force.WeightRecorded ? force.MeasuredW : 0f;
        float tilt = force != null ? force.TiltDeg : 0f;
        int trial = EquilibriumTrialManager.Instance != null ? EquilibriumTrialManager.Instance.CurrentTrial : 1;
        bool horizontal = force != null && force.IsHorizontal;
        if (liveReadingsText != null)
        {
            liveReadingsText.text =
                $"Spring balance F1:  {f1:0.00} N\n" +
                $"Spring balance F2:  {f2:0.00} N\n" +
                $"F1 + F2:  {(f1 + f2):0.00} N\n" +
                $"Weight W:  {(w > 0f ? w.ToString("0.00") : "—")} N\n\n" +
                $"Tilt:  {tilt:0}°\n" +
                (horizontal ? "Ruler: HORIZONTAL" : "Ruler: not horizontal") +
                $"\n\nTrial {trial} / 3";
        }
        if (physicsText != null)
        {
            physicsText.text =
                "EQUILIBRIUM RULE\nF1 + F2 = W\n\n" +
                "THREE COPLANAR PARALLEL FORCES\nF1 and F2 act vertically upward at the ends. W acts vertically downward at the centre of gravity.\n\n" +
                "HORIZONTAL POSITION\nThe ruler must stay level so there is no net turning effect.";
        }
        if (titleText != null)
            titleText.text = $"EQUILIBRIUM OF FORCES     Attempt: {(EquilibriumAttemptManager.Instance != null ? EquilibriumAttemptManager.Instance.CurrentAttemptNumber : 1)}/3";
        if (stepText != null)
        {
            var cur = Current();
            if (cur == EquilibriumExperimentStep.Assembly) stepText.text = "Assembly";
            else if (cur == EquilibriumExperimentStep.MeasureWeight) stepText.text = "Measure W";
            else if (cur == EquilibriumExperimentStep.Equilibrium) stepText.text = $"Trial {trial} / 3";
        }
        EquilibriumVisualController.Instance?.RefreshReadings();
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
        bool assembly = Current() == EquilibriumExperimentStep.Assembly;
        bool weigh = Current() == EquilibriumExperimentStep.MeasureWeight;
        bool eq = Current() == EquilibriumExperimentStep.Equilibrium;
        if (confirmSetupBtn != null) confirmSetupBtn.gameObject.SetActive(visible && assembly);
        if (weighBtn != null) weighBtn.gameObject.SetActive(visible && weigh);
        if (recordBtn != null) recordBtn.gameObject.SetActive(visible && (weigh || eq));
        if (resetRunBtn != null) resetRunBtn.gameObject.SetActive(visible && eq);
        if (hangLeftBtn != null) hangLeftBtn.gameObject.SetActive(visible && eq);
        if (hangRightBtn != null) hangRightBtn.gameObject.SetActive(visible && eq);
        if (tiltPlus != null) tiltPlus.gameObject.SetActive(visible && eq);
        if (tiltMinus != null) tiltMinus.gameObject.SetActive(visible && eq);
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

    private static string StepTitle(EquilibriumExperimentStep step)
    {
        switch (step)
        {
            case EquilibriumExperimentStep.Objective: return "Objectives";
            case EquilibriumExperimentStep.SelectEquipment: return "Equipment";
            case EquilibriumExperimentStep.Assembly: return "Assembly";
            case EquilibriumExperimentStep.MeasureWeight: return "Measure W";
            case EquilibriumExperimentStep.Equilibrium: return "Equilibrium";
            case EquilibriumExperimentStep.ObservationTable: return "Observations";
            case EquilibriumExperimentStep.CompareResults: return "Compare";
            case EquilibriumExperimentStep.Graph: return "Graphs";
            case EquilibriumExperimentStep.Questions: return "Questions";
            case EquilibriumExperimentStep.VariableMatching: return "Variables";
            case EquilibriumExperimentStep.Conclusion: return "Conclusion";
            case EquilibriumExperimentStep.Complete: return "Results";
            default: return "Introduction";
        }
    }

    private static EquilibriumExperimentStep Current()
    {
        return EquilibriumExperimentManager.Instance != null
            ? EquilibriumExperimentManager.Instance.CurrentStep
            : EquilibriumExperimentStep.Introduction;
    }
}

public class EquilibriumIntroClickToStart : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (EquilibriumUIManager.Instance != null) EquilibriumUIManager.Instance.StartPractical();
    }
}
