using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorkEnergyUIManager : MonoBehaviour
{
    public static WorkEnergyUIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI attemptsText;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private GameObject instructionBar;
    [SerializeField] private GameObject equipmentPanel;
    [SerializeField] private GameObject experimentPanel;
    [SerializeField] private GameObject dataTablePanel;
    [SerializeField] private GameObject comparePanel;
    [SerializeField] private GameObject graphPanel;
    [SerializeField] private GameObject workEnergyPanel;
    [SerializeField] private GameObject powerPanel;
    [SerializeField] private GameObject conclusionPanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI infoPanelText;
    [SerializeField] private TextMeshProUGUI dataTableText;
    [SerializeField] private TextMeshProUGUI compareText;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI workEnergyText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI resultDetailsText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button startBtn;
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button resetBtn;
    [SerializeField] private Button retryBtn;
    [SerializeField] private Button viewProfileBtn;
    [SerializeField] private GameObject resetConfirmPanel;
    [SerializeField] private Button resetYes;
    [SerializeField] private Button resetNo;
    [SerializeField] private Button equipContinueBtn;

    private bool tableBonusAwarded;
    private bool graphBonusAwarded;
    private bool practicalRunning;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void BindAll(WorkEnergyUIRefsHolder refs, bool showWelcome)
    {
        titleText = refs.Title; scoreText = refs.Score; progressText = refs.Progress;
        attemptsText = refs.Attempts; progressBarFill = refs.ProgressBar;
        instructionText = refs.Instruction; objectivePanel = refs.ObjectivePanel;
        instructionBar = refs.InstructionBar; equipmentPanel = refs.EquipmentPanel;
        experimentPanel = refs.ExperimentPanel; dataTablePanel = refs.DataTablePanel;
        comparePanel = refs.ComparePanel; graphPanel = refs.GraphPanel;
        workEnergyPanel = refs.WorkEnergyPanel; powerPanel = refs.PowerPanel;
        conclusionPanel = refs.ConclusionPanel; resultPanel = refs.ResultPanel;
        infoPanelText = refs.InfoText; dataTableText = refs.DataTableText;
        compareText = refs.CompareText; objectiveText = refs.ObjectiveText;
        workEnergyText = refs.WorkEnergyText;
        finalScoreText = refs.FinalScore; resultDetailsText = refs.ResultDetails;
        statusText = refs.StatusText;
        startBtn = refs.StartBtn; nextBtn = refs.Next; resetBtn = refs.Reset;
        retryBtn = refs.Retry; viewProfileBtn = refs.ViewProfileBtn;
        equipContinueBtn = refs.EquipContinueBtn;
        resetConfirmPanel = refs.ResetConfirm;
        resetYes = refs.ResetYes; resetNo = refs.ResetNo;
        WireButtons();
        AttachIntroClick();
        if (showWelcome) ShowObjective();
    }

    private void AttachIntroClick()
    {
        if (objectivePanel == null) return;
        var catcher = objectivePanel.GetComponent<IntroClickToStart>() ?? objectivePanel.AddComponent<IntroClickToStart>();
        catcher.enabled = true;
        var panelBtn = objectivePanel.GetComponent<Button>() ?? objectivePanel.AddComponent<Button>();
        panelBtn.transition = Selectable.Transition.None;
        panelBtn.onClick.RemoveAllListeners();
        panelBtn.onClick.AddListener(StartPractical);
        if (startBtn != null)
        {
            startBtn.onClick.RemoveAllListeners();
            startBtn.onClick.AddListener(StartPractical);
            startBtn.gameObject.SetActive(true);
        }
    }

    private void WireButtons()
    {
        WireBtn(startBtn, StartPractical);
        WireBtn(nextBtn, OnNextPressed);
        WireBtn(equipContinueBtn, GoNextFromEquipment);
        WireBtn(resetBtn, () => { if (resetConfirmPanel != null) resetConfirmPanel.SetActive(true); });
        WireBtn(resetYes, () =>
        {
            resetConfirmPanel?.SetActive(false);
            WorkEnergyPowerExperimentManager.Instance?.ResetExperiment();
            ShowObjective();
        });
        WireBtn(resetNo, () => resetConfirmPanel?.SetActive(false));
        WireBtn(retryBtn, () => WorkEnergyPowerExperimentManager.Instance?.RetryExperiment());
        WireBtn(viewProfileBtn, () =>
        {
            string summary = WorkEnergyProfileManager.Instance != null ? WorkEnergyProfileManager.Instance.GetProfileSummary() : "No profile data.";
            WorkEnergyFeedbackManager.Instance?.ShowInstruction(summary);
        });
    }

    private void WireBtn(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    private void ShowObjective()
    {
        practicalRunning = false;
        HideAllContent();
        if (objectivePanel != null) objectivePanel.SetActive(true);
        if (instructionBar != null) instructionBar.SetActive(false);
        SetNextButtonVisible(false);
        if (objectiveText != null)
        {
            objectiveText.text =
                "ACTIVITY\n\n" +
                "VARIATION OF POTENTIAL ENERGY WITH HEIGHT\n\n" +
                "WORK, ENERGY AND POWER\n\n" +
                "OBJECTIVE\n" +
                "To investigate how the potential energy of an object varies with its height above the ground.\n\n" +
                "Potential Energy:\nPE = mgh\n\n" +
                "When an object is raised to a greater height, its gravitational potential energy increases.\n\n" +
                "In this experiment you will investigate how the height from which an object is released affects the depression produced on a clay surface.\n\n" +
                "Raise the object to different heights and release it from rest. Observe and measure the depth of the depression produced in the clay.\n\n" +
                "The depression depth is an experimental indicator of the effect of the impact. It is not equal to energy in joules.";
        }
        UpdateAttemptsDisplay(WorkEnergyAttemptManager.Instance != null ? WorkEnergyAttemptManager.Instance.AttemptsRemaining : 3);
        if (startBtn != null) startBtn.gameObject.SetActive(true);
        AttachIntroClick();
    }

    public bool IsOnIntroScreen =>
        !practicalRunning && objectivePanel != null && objectivePanel.activeInHierarchy;

    private void Update()
    {
        if (!IsOnIntroScreen) return;
        if (ConfirmPressedThisFrame())
            StartPractical();
    }

    private static bool ConfirmPressedThisFrame()
    {
        try
        {
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) return true;
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame))
                return true;
        }
        catch { /* ignore */ }
        return false;
    }

    public void StartPractical()
    {
        if (practicalRunning) return;
        practicalRunning = true;
        try
        {
            WorkEnergyFailsafeDisplay.Hide();
            if (objectivePanel != null)
            {
                var catcher = objectivePanel.GetComponent<IntroClickToStart>();
                if (catcher != null) catcher.enabled = false;
            }

            tableBonusAwarded = false;
            graphBonusAwarded = false;
            HideAllContent();
            if (instructionBar != null) instructionBar.SetActive(true);
            if (WorkEnergyPowerExperimentManager.Instance != null)
                WorkEnergyPowerExperimentManager.Instance.StartPractical();
            if (equipmentPanel != null) equipmentPanel.SetActive(true);
            WorkEnergyEquipmentSelectionManager.Instance?.EnsureCardsVisible();
            UpdateInstruction("STEP 1: Tap the required equipment cards. Then press NEXT STEP at the bottom.");
            SetNextButtonVisible(true);
            SetNextButtonLabel("NEXT STEP");
            if (equipContinueBtn != null) equipContinueBtn.gameObject.SetActive(false);
            Debug.Log("Work Energy Power: practical started.");
        }
        catch (System.Exception ex)
        {
            practicalRunning = false;
            Debug.LogError("StartPractical failed: " + ex);
        }
    }

    public void RestartPractical()
    {
        practicalRunning = false;
        StartPractical();
    }

    public void GoNextFromEquipment()
    {
        if (WorkEnergyEquipmentSelectionManager.Instance == null)
        {
            WorkEnergyPowerExperimentManager.Instance?.SetStep(WorkEnergyExperimentStep.PrepareClay);
            return;
        }

        if (!WorkEnergyEquipmentSelectionManager.Instance.IsCompleteCheck())
            WorkEnergyEquipmentSelectionManager.Instance.SelectRemainingRequired();

        if (WorkEnergyEquipmentSelectionManager.Instance.IsCompleteCheck())
        {
            WorkEnergyPowerExperimentManager.Instance?.TryAdvanceFromEquipment();
            if (equipContinueBtn != null) equipContinueBtn.gameObject.SetActive(false);
        }
        else
        {
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("Tap the required equipment, then press NEXT STEP.");
        }
    }

    public void SetEquipContinueVisible(bool visible)
    {
        if (equipContinueBtn != null) equipContinueBtn.gameObject.SetActive(false);
        SetNextButtonVisible(true);
    }

    private void OnNextPressed()
    {
        var step = WorkEnergyPowerExperimentManager.Instance != null
            ? WorkEnergyPowerExperimentManager.Instance.CurrentStep
            : WorkEnergyExperimentStep.SelectEquipment;

        if (step == WorkEnergyExperimentStep.Introduction)
        {
            StartPractical();
            return;
        }

        if (step == WorkEnergyExperimentStep.SelectEquipment)
        {
            GoNextFromEquipment();
            return;
        }

        if (step == WorkEnergyExperimentStep.ConclusionQ1 || step == WorkEnergyExperimentStep.ConclusionQ2 || step == WorkEnergyExperimentStep.ConclusionQ3)
        {
            WorkEnergyPowerExperimentManager.Instance?.AdvanceStep();
            return;
        }

        if (step == WorkEnergyExperimentStep.CompareResults || step == WorkEnergyExperimentStep.ViewGraph ||
            step == WorkEnergyExperimentStep.WorkEnergy || step == WorkEnergyExperimentStep.Conclusion ||
            step == WorkEnergyExperimentStep.PowerChallenge)
        {
            WorkEnergyPowerExperimentManager.Instance?.AdvanceStep();
            return;
        }

        WorkEnergyLabWorkbench.Instance?.DoGuidedAction();
    }

    public void UpdateScoreDisplay(int score)
    {
        if (scoreText != null) scoreText.text = $"Score: {score} / 100";
    }

    public void UpdateAttemptsDisplay(int remaining)
    {
        if (attemptsText != null) attemptsText.text = $"Attempts Remaining: {remaining}";
    }

    public void UpdateProgress(WorkEnergyExperimentStep step, int total)
    {
        int n = GetDisplayStepNumber(step);
        if (progressText != null) progressText.text = $"Step: {n} / {total}";
        if (progressBarFill != null) progressBarFill.fillAmount = n / (float)Mathf.Max(1, total);
    }

    private int GetDisplayStepNumber(WorkEnergyExperimentStep step)
    {
        switch (step)
        {
            case WorkEnergyExperimentStep.SelectEquipment: return 1;
            case WorkEnergyExperimentStep.PrepareClay: return 2;
            case WorkEnergyExperimentStep.PlaceStand:
            case WorkEnergyExperimentStep.PlaceWeight: return 3;
            case WorkEnergyExperimentStep.MeasureMass: return 4;
            case WorkEnergyExperimentStep.SetHeight:
            case WorkEnergyExperimentStep.MeasureHeight: return 5;
            case WorkEnergyExperimentStep.ReleaseWeight:
            case WorkEnergyExperimentStep.ObserveImpact: return 6;
            case WorkEnergyExperimentStep.MeasureDepression:
            case WorkEnergyExperimentStep.RecordResult: return 7;
            case WorkEnergyExperimentStep.CompareResults: return 9;
            case WorkEnergyExperimentStep.ViewGraph: return 10;
            case WorkEnergyExperimentStep.WorkEnergy: return 11;
            case WorkEnergyExperimentStep.PowerChallenge: return 12;
            case WorkEnergyExperimentStep.ConclusionQ1:
            case WorkEnergyExperimentStep.ConclusionQ2:
            case WorkEnergyExperimentStep.ConclusionQ3: return 13;
            case WorkEnergyExperimentStep.Conclusion: return 14;
            case WorkEnergyExperimentStep.Complete: return 15;
            default: return 1;
        }
    }

    public void UpdateInstruction(string text)
    {
        if (instructionText != null) instructionText.text = text;
    }

    public void ShowStagePanels(WorkEnergyExperimentStep step)
    {
        if (instructionBar != null) instructionBar.SetActive(step != WorkEnergyExperimentStep.Complete && step != WorkEnergyExperimentStep.Introduction);
        if (objectivePanel != null) objectivePanel.SetActive(false);
        if (equipmentPanel != null) equipmentPanel.SetActive(step == WorkEnergyExperimentStep.SelectEquipment);
        bool lab = step >= WorkEnergyExperimentStep.PrepareClay && step <= WorkEnergyExperimentStep.RecordResult;
        if (experimentPanel != null) experimentPanel.SetActive(lab);
        if (dataTablePanel != null) dataTablePanel.SetActive(lab && step >= WorkEnergyExperimentStep.RecordResult);
        if (comparePanel != null) comparePanel.SetActive(step == WorkEnergyExperimentStep.CompareResults);
        if (graphPanel != null) graphPanel.SetActive(step == WorkEnergyExperimentStep.ViewGraph);
        if (workEnergyPanel != null) workEnergyPanel.SetActive(step == WorkEnergyExperimentStep.WorkEnergy);
        if (powerPanel != null) powerPanel.SetActive(step == WorkEnergyExperimentStep.PowerChallenge);
        if (conclusionPanel != null) conclusionPanel.SetActive(
            step == WorkEnergyExperimentStep.ConclusionQ1 || step == WorkEnergyExperimentStep.ConclusionQ2 ||
            step == WorkEnergyExperimentStep.ConclusionQ3 || step == WorkEnergyExperimentStep.Conclusion);
        if (resultPanel != null) resultPanel.SetActive(step == WorkEnergyExperimentStep.Complete);

        bool showNext = step != WorkEnergyExperimentStep.Complete && step != WorkEnergyExperimentStep.Introduction;
        SetNextButtonVisible(showNext);
        SetNextButtonLabel(GetNextLabel(step));

        if (step >= WorkEnergyExperimentStep.RecordResult) UpdateDataTable();
        UpdateInfo();
    }

    private string GetNextLabel(WorkEnergyExperimentStep step)
    {
        switch (step)
        {
            case WorkEnergyExperimentStep.SelectEquipment: return "NEXT STEP";
            case WorkEnergyExperimentStep.PrepareClay: return "PLACE CLAY";
            case WorkEnergyExperimentStep.PlaceStand: return "PLACE STAND";
            case WorkEnergyExperimentStep.PlaceWeight: return "PLACE WEIGHT";
            case WorkEnergyExperimentStep.MeasureMass: return "MEASURE MASS";
            case WorkEnergyExperimentStep.SetHeight: return "SET HEIGHT";
            case WorkEnergyExperimentStep.MeasureHeight: return "CONFIRM HEIGHT";
            case WorkEnergyExperimentStep.ReleaseWeight: return "RELEASE WEIGHT";
            case WorkEnergyExperimentStep.ObserveImpact: return "CONTINUE";
            case WorkEnergyExperimentStep.MeasureDepression: return "CONFIRM DEPTH";
            case WorkEnergyExperimentStep.RecordResult: return "RECORD READING";
            case WorkEnergyExperimentStep.PowerChallenge: return "SKIP / NEXT";
            case WorkEnergyExperimentStep.ConclusionQ1:
            case WorkEnergyExperimentStep.ConclusionQ2:
            case WorkEnergyExperimentStep.ConclusionQ3: return "SKIP QUESTION";
            case WorkEnergyExperimentStep.Conclusion: return "FINISH";
            default: return "NEXT STEP";
        }
    }

    private void HideAllContent()
    {
        if (objectivePanel != null) objectivePanel.SetActive(false);
        if (equipmentPanel != null) equipmentPanel.SetActive(false);
        if (experimentPanel != null) experimentPanel.SetActive(false);
        if (comparePanel != null) comparePanel.SetActive(false);
        if (graphPanel != null) graphPanel.SetActive(false);
        if (workEnergyPanel != null) workEnergyPanel.SetActive(false);
        if (powerPanel != null) powerPanel.SetActive(false);
        if (conclusionPanel != null) conclusionPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    private void UpdateInfo()
    {
        if (infoPanelText == null) return;
        float mass = WorkEnergyExperimentDataManager.Instance != null ? WorkEnergyExperimentDataManager.Instance.StoredMass : 1f;
        float g = WorkEnergyPotentialEnergyCalculator.Instance != null ? WorkEnergyPotentialEnergyCalculator.Instance.Gravity : 9.8f;
        float h = WorkEnergyReleaseMechanismController.Instance != null ? WorkEnergyReleaseMechanismController.Instance.CurrentHeight : 0f;
        float pe = mass * g * h;
        infoPanelText.text =
            "EXPERIMENT INFO\n\n" +
            $"Mass m = {mass:0.00} kg\n" +
            $"g = {g:0.0} m/s²\n" +
            $"Height h = {h:0.00} m\n" +
            $"PE = mgh = {pe:0.00} J\n\n" +
            "Clay thickness = 3 cm\n" +
            "Use the same weight.\n" +
            "Hit the same impact point.";
    }

    public void UpdateDataTable()
    {
        if (dataTableText == null) return;
        var readings = WorkEnergyExperimentDataManager.Instance != null ? WorkEnergyExperimentDataManager.Instance.Readings : null;
        string table = "DATA TABLE\n\n";
        table += "Inst | Mass(kg) | Height(m) | PE(J) | Depth(cm)\n";
        table += "-----|----------|-----------|-------|----------\n";
        if (readings != null)
        {
            foreach (var r in readings)
                table += $"{r.instance:00}   | {r.mass,8:0.00} | {r.height,9:0.00} | {r.potentialEnergy,5:0.00} | {r.depressionDepth,8:0.0}\n";
        }
        if (readings == null || readings.Count == 0)
            table += "(No readings yet)\n";
        dataTableText.text = table;
    }

    public void ShowCompareResults()
    {
        if (compareText == null) return;
        var readings = WorkEnergyExperimentDataManager.Instance != null ? WorkEnergyExperimentDataManager.Instance.Readings : null;
        string text =
            "OBSERVATIONS\n\n" +
            "The depth of the depression generally increases as the height from which the weight is released increases.\n\n" +
            "The gravitational potential energy of the weight increases with height.\n\n";
        if (readings != null)
        {
            foreach (var r in readings)
                text += $"• h = {r.height:0.00} m  →  PE = {r.potentialEnergy:0.00} J  →  depth = {r.depressionDepth:0.0} cm\n";
        }
        text +=
            "\nHeight increases → Potential energy increases → Impact effect increases → Clay depression generally becomes deeper.\n\n" +
            "Depression depth is an indicator of the impact effect, not a direct energy reading in joules.";
        compareText.text = text;
        if (!tableBonusAwarded)
        {
            tableBonusAwarded = true;
            WorkEnergyScoreManager.Instance?.AddScore(5);
        }
        UpdateInstruction("Compare results, then press NEXT STEP.");
        SetNextButtonVisible(true);
    }

    public void ShowWorkEnergy()
    {
        if (workEnergyText == null) return;
        workEnergyText.text =
            "WORK AND ENERGY\n\n" +
            "When the weight is raised, work is done against gravity and gravitational potential energy is stored.\n\n" +
            "Work done against gravity:\nW = mgh\n\n" +
            "This stored potential energy is converted into kinetic energy as the weight falls.\n\n" +
            "When the weight strikes the clay, energy is transferred to the clay and produces a depression.\n\n" +
            "Not all of the gravitational potential energy becomes useful deformation energy in the clay. Some energy is transferred to sound, heat, motion and deformation of the object and clay.\n\n" +
            "POWER\nPower is the rate at which work is done.\nP = W / t";
        SetNextButtonVisible(true);
    }

    public void ShowResult(int score, bool passed, int mistakes, WorkEnergyAttemptRecord attempt)
    {
        HideAllContent();
        if (resultPanel != null) resultPanel.SetActive(true);
        WorkEnergyResultManager.Instance?.ShowResult(score, passed, mistakes, attempt);
        if (retryBtn != null)
            retryBtn.gameObject.SetActive(WorkEnergyAttemptManager.Instance != null && WorkEnergyAttemptManager.Instance.CanRetry());
    }

    public void SetNextButtonVisible(bool visible)
    {
        if (nextBtn != null) nextBtn.gameObject.SetActive(visible);
    }

    public void SetNextButtonLabel(string label)
    {
        if (nextBtn == null) return;
        var txt = nextBtn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (txt != null) txt.text = label;
    }

    public void HideResult()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
    }
}

public class IntroClickToStart : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        WorkEnergyUIManager.Instance?.StartPractical();
    }
}
