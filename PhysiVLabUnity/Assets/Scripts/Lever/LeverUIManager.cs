using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeverUIManager : MonoBehaviour
{
    public static LeverUIManager Instance { get; private set; }

    [Header("Header")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI attemptsText;
    [SerializeField] private Image progressBarFill;

    [Header("Panels")]
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private GameObject instructionBar;
    [SerializeField] private GameObject equipmentPanel;
    [SerializeField] private GameObject experimentPanel;
    [SerializeField] private GameObject dataTablePanel;
    [SerializeField] private GameObject comparePanel;
    [SerializeField] private GameObject conclusionPanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI infoPanelText;
    [SerializeField] private TextMeshProUGUI dataTableText;
    [SerializeField] private TextMeshProUGUI compareText;
    [SerializeField] private TextMeshProUGUI objectiveText;

    [Header("Result")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI resultDetailsText;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Buttons")]
    [SerializeField] private Button startBtn;
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button resetBtn;
    [SerializeField] private Button retryBtn;
    [SerializeField] private Button viewProfileBtn;
    [SerializeField] private GameObject resetConfirmPanel;
    [SerializeField] private Button resetYes;
    [SerializeField] private Button resetNo;
    [SerializeField] private Button equipContinueBtn;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void EnsureBoundFromScene(bool showWelcome = false, bool force = false)
    {
        var builder = Object.FindAnyObjectByType<LeverSceneRuntimeBuilder>();
        if (builder != null)
        {
            builder.WireReferencesOnPlay(showWelcome, force);
            return;
        }
        WireFromRefs(showWelcome);
    }

    private void WireFromRefs(bool showWelcome)
    {
        var refs = Object.FindAnyObjectByType<LeverUIRefsHolder>();
        if (refs == null) return;
        BindAll(refs, showWelcome);
    }

    public void BindAll(LeverUIRefsHolder refs, bool showWelcome)
    {
        if (refs == null) return;

        titleText = refs.Title;
        scoreText = refs.Score;
        progressText = refs.Progress;
        attemptsText = refs.Attempts;
        progressBarFill = refs.ProgressBar;
        instructionText = refs.Instruction;
        objectivePanel = refs.ObjectivePanel;
        instructionBar = refs.InstructionBar;
        equipmentPanel = refs.EquipmentPanel;
        experimentPanel = refs.ExperimentPanel;
        dataTablePanel = refs.DataTablePanel;
        comparePanel = refs.ComparePanel;
        conclusionPanel = refs.ConclusionPanel;
        resultPanel = refs.ResultPanel;
        infoPanelText = refs.InfoText;
        dataTableText = refs.DataTableText;
        compareText = refs.CompareText;
        objectiveText = refs.ObjectiveText;
        finalScoreText = refs.FinalScore;
        resultDetailsText = refs.ResultDetails;
        statusText = refs.StatusText;
        startBtn = refs.StartBtn;
        nextBtn = refs.Next;
        resetBtn = refs.Reset;
        retryBtn = refs.Retry;
        viewProfileBtn = refs.ViewProfileBtn;
        equipContinueBtn = refs.EquipContinueBtn;
        resetConfirmPanel = refs.ResetConfirm;
        resetYes = refs.ResetYes;
        resetNo = refs.ResetNo;

        WireButtons();

        LeverConclusionManager.Instance?.Bind(
            refs.ConclusionA, refs.ConclusionB, refs.ConclusionC, refs.ConclusionD,
            refs.ConclusionExplanationPanel, refs.ConclusionExplanationText,
            refs.ConclusionResultsReminder, refs.ConclusionContinueBtn,
            refs.ConclusionQuestionText);

        NewtonSpringBalanceController.Instance?.Bind(refs.ForceLabel, refs.SpringVisual, LeverSpringController.Instance);
        LeverSpringController.Instance?.Bind(refs.SpringVisual);
        LeverPullHandleController.Instance?.Bind(refs.PullHandle);
        BookLiftController.Instance?.Bind(refs.BookVisual);
        LeverWoodenStripController.Instance?.Bind(refs.StripVisual);
        LeverPivotController.Instance?.Bind(refs.PivotVisual);
        LeverMeasurementManager.Instance?.Bind(
            refs.MeasureAInput, refs.ConfirmDistanceABtn, refs.XSelectionButtonsContainer,
            refs.MeasureALabel, refs.MeasureXLabel, refs.PivotLabel, refs.XSelectionButtons);
        LeverGraphView.Instance?.Bind(refs.GraphArea, refs.DotPrefab, refs.LineImage);

        if (refs.RecordBtn != null)
        {
            refs.RecordBtn.onClick.RemoveAllListeners();
            refs.RecordBtn.onClick.AddListener(() => LeverLabWorkbench.Instance?.RecordCurrentReading());
        }

        if (showWelcome) ShowObjective();
    }

    private void WireButtons()
    {
        WireBtn(startBtn, () => StartPractical());
        WireBtn(nextBtn, () => OnNextPressed());
        WireBtn(equipContinueBtn, () => GoNextFromEquipment());
        WireBtn(resetBtn, () => { if (resetConfirmPanel != null) resetConfirmPanel.SetActive(true); });
        WireBtn(resetYes, () =>
        {
            resetConfirmPanel?.SetActive(false);
            LeverGameManager.Instance?.ResetExperiment();
            ShowObjective();
        });
        WireBtn(resetNo, () => resetConfirmPanel?.SetActive(false));
        WireBtn(retryBtn, () => LeverGameManager.Instance?.RetryExperiment());
        WireBtn(viewProfileBtn, () =>
        {
            string summary = LeverProfileManager.Instance?.GetProfileSummary() ?? "No profile data.";
            LeverFeedbackManager.Instance?.ShowInstruction(summary);
        });
    }

    public void GoNextFromEquipment()
    {
        if (LeverEquipmentSelectionManager.Instance == null ||
            !LeverEquipmentSelectionManager.Instance.IsCompleteCheck())
        {
            LeverFeedbackManager.Instance?.ShowInstruction("Select all 5 required equipment items first.");
            return;
        }

        if (LeverExperimentManager.Instance == null)
        {
            Debug.LogError("LeverExperimentManager missing");
            return;
        }

        if (LeverExperimentManager.Instance.CurrentStep != LeverExperimentStep.SelectEquipment)
            LeverExperimentManager.Instance.SetStep(LeverExperimentStep.SelectEquipment);

        LeverExperimentManager.Instance.TryAdvanceFromEquipment();
        if (equipContinueBtn != null) equipContinueBtn.gameObject.SetActive(false);
    }

    public void SetEquipContinueVisible(bool visible)
    {
        if (equipContinueBtn != null) equipContinueBtn.gameObject.SetActive(visible);
        SetNextButtonVisible(visible);
    }

    private void OnNextPressed()
    {
        var step = LeverExperimentManager.Instance != null
            ? LeverExperimentManager.Instance.CurrentStep
            : LeverExperimentStep.SelectEquipment;

        if (step == LeverExperimentStep.CompareResults || step == LeverExperimentStep.NextXOrCompare)
        {
            LeverExperimentManager.Instance?.AdvanceStep();
            return;
        }

        if (step == LeverExperimentStep.SelectEquipment || step == LeverExperimentStep.Introduction)
        {
            if (LeverEquipmentSelectionManager.Instance != null &&
                LeverEquipmentSelectionManager.Instance.IsCompleteCheck())
            {
                LeverExperimentManager.Instance?.TryAdvanceFromEquipment();
            }
            else
            {
                LeverScoreManager.Instance?.SubtractScore(5);
                LeverFeedbackManager.Instance?.ShowInstruction("Select all 5 required equipment items first.");
            }
        }
    }

    private void WireBtn(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    private void ShowObjective()
    {
        if (objectivePanel != null) objectivePanel.SetActive(true);
        if (instructionBar != null) instructionBar.SetActive(false);
        if (equipmentPanel != null) equipmentPanel.SetActive(false);
        if (experimentPanel != null) experimentPanel.SetActive(false);
        if (dataTablePanel != null) dataTablePanel.SetActive(false);
        if (comparePanel != null) comparePanel.SetActive(false);
        if (conclusionPanel != null) conclusionPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);
        SetNextButtonVisible(false);

        if (objectiveText != null)
        {
            objectiveText.text =
                "ACTIVITY 15.1\n\n" +
                "LEVER\n\n" +
                "Investigate a lever using a book, a wooden strip and a Newton spring balance.\n\n" +
                "Place the book on one end of the wooden strip, rest the strip on support P,\n" +
                "and attach the spring balance on the other side.\n\n" +
                "Measure distance a (pivot to book) and change distance x (pivot to effort).\n" +
                "Record the effort needed to lift the book for different values of x.\n\n" +
                "Observe how effort changes as x increases.\n\n" +
                "Follow each step carefully.";
        }

        UpdateAttemptsDisplay(LeverAttemptManager.Instance?.AttemptsRemaining ?? 3);
    }

    public void StartPractical()
    {
        EnsureBoundFromScene(false, true);

        if (resultPanel != null) resultPanel.SetActive(false);
        if (objectivePanel != null) objectivePanel.SetActive(false);
        if (comparePanel != null) comparePanel.SetActive(false);
        if (conclusionPanel != null) conclusionPanel.SetActive(false);
        if (experimentPanel != null) experimentPanel.SetActive(false);
        if (instructionBar != null) instructionBar.SetActive(true);

        if (LeverGameManager.Instance != null)
            LeverGameManager.Instance.StartPractical();
        else
            Debug.LogError("LeverGameManager missing — rebuild scene: Tools → Lever Practical → Build Complete Scene");

        if (equipmentPanel != null) equipmentPanel.SetActive(true);
        LeverEquipmentSelectionManager.Instance?.EnsureCardsVisible();
        LeverEquipmentSelectionManager.Instance?.ResetSelection();
        UpdateInstruction("Step 1: Tap the five required equipment cards. Then press NEXT STEP.");
        UpdateProgress(LeverExperimentStep.SelectEquipment, 12);
        SetNextButtonVisible(false);
    }

    public void UpdateScoreDisplay(int score)
    {
        if (scoreText != null) scoreText.text = $"Score: {score} / 100";
    }

    public void UpdateAttemptsDisplay(int remaining)
    {
        if (attemptsText != null) attemptsText.text = $"Attempts: {remaining}";
    }

    public void UpdateProgress(LeverExperimentStep step, int total)
    {
        int n = GetDisplayStepNumber(step);
        int displayTotal = total > 0 ? Mathf.Min(total, 12) : 12;
        if (displayTotal < 12) displayTotal = 12;
        if (progressText != null) progressText.text = $"Step {n} / {displayTotal}";
        if (progressBarFill != null) progressBarFill.fillAmount = n / (float)displayTotal;
    }

    private int GetDisplayStepNumber(LeverExperimentStep step)
    {
        switch (step)
        {
            case LeverExperimentStep.Introduction:
            case LeverExperimentStep.SelectEquipment:
                return 1;
            case LeverExperimentStep.PlacePivot:
                return 2;
            case LeverExperimentStep.PlaceWoodenStrip:
                return 3;
            case LeverExperimentStep.PlaceBook:
                return 4;
            case LeverExperimentStep.MeasureDistanceA:
                return 5;
            case LeverExperimentStep.AttachSpringBalance:
                return 6;
            case LeverExperimentStep.SelectDistanceX:
                return 7;
            case LeverExperimentStep.PullBalance:
            case LeverExperimentStep.ObserveLift:
                return 8;
            case LeverExperimentStep.RecordReading:
            case LeverExperimentStep.NextXOrCompare:
                return 9;
            case LeverExperimentStep.CompareResults:
                return 10;
            case LeverExperimentStep.Conclusion:
            case LeverExperimentStep.Conclusion2:
            case LeverExperimentStep.Challenge:
                return 11;
            case LeverExperimentStep.Complete:
                return 12;
            default:
                return 1;
        }
    }

    public void UpdateInstruction(string text)
    {
        if (instructionText != null) instructionText.text = text;
    }

    public void UpdateInfoPanel(float bookWeight, float a, float x, float requiredEffort, float currentForce)
    {
        if (infoPanelText == null) return;
        infoPanelText.text =
            "Experiment Information\n\n" +
            $"Book Weight (Load): {bookWeight:0.0} N\n" +
            $"Distance a: {a:0} cm\n" +
            $"Distance x: {x:0} cm\n" +
            $"Required Effort: {requiredEffort:0.0} N\n" +
            $"Current Force: {currentForce:0.0} N\n\n" +
            "Effort = Load × a / x\n" +
            "Moment = Force × Distance";
    }

    public void ShowStagePanels(LeverExperimentStep step)
    {
        if (instructionBar != null) instructionBar.SetActive(step != LeverExperimentStep.Complete);
        if (objectivePanel != null) objectivePanel.SetActive(false);
        if (equipmentPanel != null) equipmentPanel.SetActive(step == LeverExperimentStep.SelectEquipment);

        bool lab = step >= LeverExperimentStep.PlacePivot && step <= LeverExperimentStep.NextXOrCompare;
        if (experimentPanel != null) experimentPanel.SetActive(lab);

        bool showTable = step >= LeverExperimentStep.RecordReading && step <= LeverExperimentStep.NextXOrCompare;
        if (dataTablePanel != null) dataTablePanel.SetActive(showTable);

        if (comparePanel != null) comparePanel.SetActive(step == LeverExperimentStep.CompareResults);

        bool conclusion =
            step == LeverExperimentStep.Conclusion ||
            step == LeverExperimentStep.Conclusion2 ||
            step == LeverExperimentStep.Challenge;
        if (conclusionPanel != null) conclusionPanel.SetActive(conclusion);

        if (resultPanel != null) resultPanel.SetActive(step == LeverExperimentStep.Complete);

        SetNextButtonVisible(
            (step == LeverExperimentStep.SelectEquipment &&
             LeverEquipmentSelectionManager.Instance != null &&
             LeverEquipmentSelectionManager.Instance.IsCompleteCheck()) ||
            step == LeverExperimentStep.CompareResults ||
            step == LeverExperimentStep.NextXOrCompare);

        if (step >= LeverExperimentStep.RecordReading) UpdateDataTable();
    }

    public void UpdateDataTable()
    {
        if (dataTableText == null) return;
        var data = LeverExperimentDataManager.Instance;
        var readings = data?.Readings;
        int max = data != null ? Mathf.Max(1, data.MaxInstances) : 4;

        string table = "Table 15.1 – Lever Readings\n\n";
        table += "Inst | a(cm) | x(cm) | Load(N) | Effort(N) | Lifted\n";
        table += "-----|-------|-------|---------|-----------|-------\n";

        for (int i = 1; i <= max; i++)
        {
            string a = "—";
            string x = "—";
            string load = "—";
            string effort = "—";
            string lifted = "—";

            if (readings != null)
            {
                foreach (var r in readings)
                {
                    if (r.instance != i) continue;
                    a = r.distanceA.ToString("0");
                    x = r.distanceX.ToString("0");
                    load = r.bookWeight.ToString("0.0");
                    float e = r.measuredEffort > 0f ? r.measuredEffort : r.requiredEffort;
                    effort = e.ToString("0.0");
                    lifted = r.bookLifted ? "Yes" : "No";
                    break;
                }
            }

            table += $"{i:00}   | {a,5} | {x,5} | {load,7} | {effort,9} | {lifted}\n";
        }

        dataTableText.text = table;
    }

    public void ShowCompareResults()
    {
        if (compareText == null) return;
        var readings = LeverExperimentDataManager.Instance?.Readings;
        string text = "Compare how effort changed with distance x:\n\n";
        if (readings != null)
        {
            foreach (var r in readings)
            {
                float effort = r.measuredEffort > 0f ? r.measuredEffort : r.requiredEffort;
                text += $"• x = {r.distanceX:0} cm  →  Effort = {effort:0.0} N" +
                        (r.bookLifted ? "  (lifted)\n" : "\n");
            }
        }
        text += "\nTrend: As x increases, required effort decreases";
        compareText.text = text;
        LeverGraphView.Instance?.UpdateGraph(readings);
        UpdateInstruction("STEP 10: Compare results, then press NEXT STEP for the conclusion questions.");
    }

    public void ShowResult(int score, bool passed, int mistakes, LeverAttemptRecord attempt)
    {
        if (resultPanel != null) resultPanel.SetActive(true);
        if (experimentPanel != null) experimentPanel.SetActive(false);
        if (conclusionPanel != null) conclusionPanel.SetActive(false);
        if (comparePanel != null) comparePanel.SetActive(false);
        if (equipmentPanel != null) equipmentPanel.SetActive(false);
        if (dataTablePanel != null) dataTablePanel.SetActive(false);

        string perf = LeverProfileManager.Instance?.GetPerformanceLabel(score) ?? "";
        if (finalScoreText != null)
            finalScoreText.text =
                $"Score: {score} / 100\nPerformance: {perf}\nMistakes: {mistakes}\nAttempt: {attempt.attemptNumber}";

        if (resultDetailsText != null)
        {
            string details = "LEVER READINGS\n\n";
            if (attempt.readings != null)
            {
                foreach (var r in attempt.readings)
                {
                    float effort = r.measuredEffort > 0f ? r.measuredEffort : r.requiredEffort;
                    details +=
                        $"Inst {r.instance:00}: a={r.distanceA:0} cm, x={r.distanceX:0} cm, " +
                        $"Load={r.bookWeight:0.0} N, Effort={effort:0.0} N, " +
                        $"Lifted={(r.bookLifted ? "Yes" : "No")}\n";
                }
            }

            details +=
                "\nCONCLUSION\n" +
                "As distance x increases, the required effort decreases.\n" +
                "A longer effort arm makes it easier to lift the load.\n" +
                "Effort = Load × a / x";
            resultDetailsText.text = details;
        }

        if (statusText != null)
        {
            statusText.text = passed ? "STATUS: PASSED" : "STATUS: TRY AGAIN";
            statusText.color = passed ? new Color(0.1f, 0.55f, 0.2f) : new Color(0.75f, 0.15f, 0.15f);
        }

        if (retryBtn != null)
            retryBtn.gameObject.SetActive(LeverAttemptManager.Instance != null && LeverAttemptManager.Instance.CanRetry());
    }

    public void SetNextButtonVisible(bool visible)
    {
        if (nextBtn != null) nextBtn.gameObject.SetActive(visible);
    }

    public void HideResult()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
    }
}
