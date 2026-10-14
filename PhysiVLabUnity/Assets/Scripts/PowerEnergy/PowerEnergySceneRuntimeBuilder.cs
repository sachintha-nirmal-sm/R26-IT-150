using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

[DefaultExecutionOrder(-50)]
public class PowerEnergySceneRuntimeBuilder : MonoBehaviour
{
    private static bool referencesWired;
    private TMP_FontAsset defaultFont;
    private Transform managersRoot;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => referencesWired = false;

    public void BuildScenePersistent() => BuildInternal();
    public bool HasExistingBuild() => transform.Find("Canvas") != null;

    public void WireReferencesOnPlay(bool showWelcome, bool force)
    {
        if (force) referencesWired = false;
        if (referencesWired) return;
        if (!WireReferences(showWelcome))
        {
            Debug.LogWarning("Power & Energy practical: wiring failed.");
            return;
        }
        referencesWired = true;
    }

    private void Awake()
    {
        if (GetComponent<PowerEnergyFailsafeDisplay>() == null)
            gameObject.AddComponent<PowerEnergyFailsafeDisplay>();
        if (!Application.isPlaying) return;
        if (HasExistingBuild()) return;
        BuildInternal();
    }

    private void BuildInternal()
    {
        referencesWired = false;
        try
        {
            var existingCanvas = transform.Find("Canvas");
            if (existingCanvas != null)
                DestroyImmediate(existingCanvas.gameObject);
            LoadFont();
            EnsureEventSystem();
            SetupCamera();
            CreateManagers();
            CreateUI();
            WireReferences(true);
            Debug.Log("Power & Energy practical UI built.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Power & Energy BUILD FAILED: " + ex.Message + "\n" + ex.StackTrace);
        }
    }

    private void LoadFont()
    {
        try
        {
            defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (defaultFont == null) defaultFont = TMP_Settings.defaultFontAsset;
        }
        catch
        {
            defaultFont = null;
        }
    }

    private void EnsureEventSystem()
    {
        try
        {
            var existing = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            EventSystem keep = null;
            foreach (var es in existing)
            {
                if (keep == null)
                {
                    keep = es;
#if ENABLE_INPUT_SYSTEM
                    if (keep.GetComponent<InputSystemUIInputModule>() == null)
                        keep.gameObject.AddComponent<InputSystemUIInputModule>();
                    var stand = keep.GetComponent<StandaloneInputModule>();
                    if (stand != null) DestroyImmediate(stand);
#else
                    if (keep.GetComponent<StandaloneInputModule>() == null)
                        keep.gameObject.AddComponent<StandaloneInputModule>();
#endif
                }
                else DestroyImmediate(es.gameObject);
            }
            if (keep == null)
            {
                var obj = new GameObject("EventSystem");
                keep = obj.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
                keep.gameObject.AddComponent<InputSystemUIInputModule>();
#else
                keep.gameObject.AddComponent<StandaloneInputModule>();
#endif
            }
#if ENABLE_INPUT_SYSTEM
            var module = keep.GetComponent<InputSystemUIInputModule>();
            if (module != null)
            {
                try { module.AssignDefaultActions(); }
                catch { /* already assigned */ }
            }
#endif
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("PowerEnergy EventSystem: " + ex.Message);
        }
    }

    private void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.90f, 0.94f, 0.98f);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        cam.orthographicSize = 5f;
    }

    private void CreateManagers()
    {
        var existing = transform.Find("Managers");
        if (existing != null) DestroyImmediate(existing.gameObject);
        managersRoot = new GameObject("Managers").transform;
        managersRoot.SetParent(transform, false);
        AddMgr<PowerEnergyExperimentManager>("PowerEnergyExperimentManager");
        AddMgr<PowerEnergyUIManager>("PowerEnergyUIManager");
        AddMgr<PowerEnergyScoreManager>("PowerEnergyScoreManager");
        AddMgr<PowerEnergyFeedbackManager>("PowerEnergyFeedbackManager");
        AddMgr<PowerEnergyAttemptManager>("PowerEnergyAttemptManager");
        AddMgr<PowerEnergySaveManager>("PowerEnergySaveManager");
        AddMgr<PowerEnergyProfileManager>("PowerEnergyProfileManager");
        AddMgr<PowerEnergyEquipmentSelectionManager>("PowerEnergyEquipmentSelectionManager");
        AddMgr<PowerEnergyEquipmentSnapController>("PowerEnergyEquipmentSnapController");
        AddMgr<PowerEnergyElectricalEquipmentManager>("PowerEnergyElectricalEquipmentManager");
        AddMgr<PowerEnergyCircuitConnectionManager>("PowerEnergyCircuitConnectionManager");
        AddMgr<PowerEnergyWireController>("PowerEnergyWireController");
        AddMgr<PowerEnergyApplianceController>("PowerEnergyApplianceController");
        AddMgr<PowerEnergyVoltmeterController>("PowerEnergyVoltmeterController");
        AddMgr<PowerEnergyAmmeterController>("PowerEnergyAmmeterController");
        AddMgr<PowerEnergyPowerCalculator>("PowerEnergyPowerCalculator");
        AddMgr<PowerEnergyEnergyCalculator>("PowerEnergyEnergyCalculator");
        AddMgr<PowerEnergyKwhConverter>("PowerEnergyKwhConverter");
        AddMgr<PowerEnergyTimerController>("PowerEnergyTimerController");
        AddMgr<PowerEnergyObservationTableManager>("PowerEnergyObservationTableManager");
        AddMgr<PowerEnergyComparisonManager>("PowerEnergyComparisonManager");
        AddMgr<PowerEnergyGraphController>("PowerEnergyGraphController");
        AddMgr<PowerEnergyQuestionManager>("PowerEnergyQuestionManager");
        AddMgr<PowerEnergyFormulaMatchingManager>("PowerEnergyFormulaMatchingManager");
        AddMgr<PowerEnergyConclusionManager>("PowerEnergyConclusionManager");
        AddMgr<PowerEnergyResultManager>("PowerEnergyResultManager");
    }

    private T AddMgr<T>(string name) where T : Component
    {
        var existing = managersRoot.Find(name);
        if (existing != null) return existing.GetComponent<T>();
        var obj = new GameObject(name);
        obj.transform.SetParent(managersRoot, false);
        return obj.AddComponent<T>();
    }

    private void CreateUI()
    {
        Color header = new Color(0.08f, 0.28f, 0.48f);
        Color accent = new Color(0.12f, 0.52f, 0.72f);
        Color green = new Color(0.12f, 0.62f, 0.42f);

        var canvasObj = new GameObject("Canvas");
        canvasObj.layer = 5;
        canvasObj.transform.SetParent(transform, false);
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.65f;
        canvasObj.AddComponent<GraphicRaycaster>();

        Panel(canvasObj.transform, "ScreenBg", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.91f, 0.95f, 0.98f));

        var headerP = Panel(canvasObj.transform, "Header", new Vector2(0, 0.91f), Vector2.one, Vector2.zero, Vector2.zero, header);
        var title = Text("Title", headerP.transform, "POWER & ENERGY", 28, TextAlignmentOptions.MidlineLeft, Vector2.zero, Vector2.zero, new Vector2(0.02f, 0.50f), new Vector2(0.38f, 1f), new Vector2(0.5f, 0.5f));
        StretchFill(title.rectTransform);
        title.color = Color.white;
        title.enableAutoSizing = true;
        title.fontSizeMin = 12;
        title.fontSizeMax = 28;
        title.overflowMode = TextOverflowModes.Ellipsis;
        var applianceHeader = Text("ApplianceHeader", headerP.transform, "Appliance: —", 22, TextAlignmentOptions.Center, Vector2.zero, Vector2.zero, new Vector2(0.38f, 0.50f), new Vector2(0.68f, 1f), new Vector2(0.5f, 0.5f));
        StretchFill(applianceHeader.rectTransform);
        applianceHeader.color = new Color(0.80f, 0.92f, 1f);
        applianceHeader.enableAutoSizing = true;
        applianceHeader.fontSizeMin = 10;
        applianceHeader.fontSizeMax = 22;
        applianceHeader.overflowMode = TextOverflowModes.Ellipsis;
        var score = Text("Score", headerP.transform, "SCORE: 0/100", 24, TextAlignmentOptions.MidlineRight, Vector2.zero, Vector2.zero, new Vector2(0.68f, 0.50f), new Vector2(0.98f, 1f), new Vector2(0.5f, 0.5f));
        StretchFill(score.rectTransform);
        score.color = Color.white;
        score.enableAutoSizing = true;
        score.fontSizeMin = 12;
        score.fontSizeMax = 24;
        var progress = Text("Progress", headerP.transform, "Step: 1 / 13", 20, TextAlignmentOptions.MidlineLeft, Vector2.zero, Vector2.zero, new Vector2(0.02f, 0.08f), new Vector2(0.28f, 0.50f), new Vector2(0.5f, 0.5f));
        StretchFill(progress.rectTransform);
        progress.color = new Color(0.75f, 0.90f, 1f);
        progress.enableAutoSizing = true;
        progress.fontSizeMin = 10;
        progress.fontSizeMax = 20;
        progress.overflowMode = TextOverflowModes.Ellipsis;
        var stepLabel = Text("StepLabel", headerP.transform, "Introduction", 20, TextAlignmentOptions.Center, Vector2.zero, Vector2.zero, new Vector2(0.28f, 0.08f), new Vector2(0.70f, 0.50f), new Vector2(0.5f, 0.5f));
        StretchFill(stepLabel.rectTransform);
        stepLabel.color = new Color(0.75f, 0.90f, 1f);
        stepLabel.enableAutoSizing = true;
        stepLabel.fontSizeMin = 10;
        stepLabel.fontSizeMax = 20;
        stepLabel.overflowMode = TextOverflowModes.Ellipsis;
        var attempts = Text("Attempts", headerP.transform, "ATTEMPT: 1/3", 20, TextAlignmentOptions.MidlineRight, Vector2.zero, Vector2.zero, new Vector2(0.70f, 0.08f), new Vector2(0.98f, 0.50f), new Vector2(0.5f, 0.5f));
        StretchFill(attempts.rectTransform);
        attempts.color = new Color(0.75f, 0.90f, 1f);
        attempts.enableAutoSizing = true;
        attempts.fontSizeMin = 10;
        attempts.fontSizeMax = 20;
        attempts.overflowMode = TextOverflowModes.Ellipsis;
        var progressBarBg = Panel(headerP.transform, "ProgressBarBg", new Vector2(0.02f, 0.00f), new Vector2(0.98f, 0.08f), Vector2.zero, Vector2.zero, new Color(0.05f, 0.12f, 0.22f, 0.5f));
        var progressBarFill = Panel(progressBarBg.transform, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.20f, 0.78f, 0.62f));
        var fillImg = progressBarFill.GetComponent<Image>();
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 0.05f;

        var instructionBar = Panel(canvasObj.transform, "InstructionBar", new Vector2(0, 0.83f), new Vector2(1, 0.91f), Vector2.zero, Vector2.zero, new Color(0.86f, 0.93f, 0.98f));
        instructionBar.SetActive(false);
        var instruction = StretchText("Instruction", instructionBar.transform, "Follow the instructions.", 24, TextAlignmentOptions.MidlineLeft, new Color(0.10f, 0.22f, 0.36f), 14);
        instruction.enableAutoSizing = true;
        instruction.fontSizeMin = 14;
        instruction.fontSizeMax = 26;
        instruction.overflowMode = TextOverflowModes.Ellipsis;

        var bottom = Panel(canvasObj.transform, "BottomBar", Vector2.zero, new Vector2(1, 0.11f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.22f, 0.38f));
        var bottomLayout = bottom.AddComponent<HorizontalLayoutGroup>();
        bottomLayout.padding = new RectOffset(16, 16, 10, 10);
        bottomLayout.spacing = 12;
        bottomLayout.childAlignment = TextAnchor.MiddleCenter;
        bottomLayout.childForceExpandWidth = false;
        bottomLayout.childForceExpandHeight = false;
        bottomLayout.childControlWidth = false;
        bottomLayout.childControlHeight = false;
        var nextBtn = Btn("Next", bottom.transform, "NEXT STEP", accent, 240, 64);
        var resetBtn = Btn("Reset", bottom.transform, "Reset", new Color(0.45f, 0.50f, 0.56f), 150, 64);
        var retryBtn = Btn("Retry", bottom.transform, "RETRY", accent, 150, 64);
        retryBtn.gameObject.SetActive(false);

        var main = Panel(canvasObj.transform, "MainArea", new Vector2(0, 0.11f), new Vector2(1, 0.83f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);

        var introPanel = Panel(main.transform, "IntroPanel", Vector2.zero, Vector2.one, new Vector2(16, 12), new Vector2(-16, -12), Color.white);
        var introText = StretchText("IntroText", introPanel.transform, "", 24, TextAlignmentOptions.TopLeft, new Color(0.10f, 0.18f, 0.28f), 12);
        introText.enableAutoSizing = true;
        introText.fontSizeMin = 14;
        introText.fontSizeMax = 26;
        var introRt = introText.rectTransform;
        introRt.anchorMin = new Vector2(0.02f, 0.46f);
        introRt.anchorMax = new Vector2(0.98f, 0.98f);
        var formulaCard = Panel(introPanel.transform, "FormulaCard", new Vector2(0.12f, 0.18f), new Vector2(0.88f, 0.44f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.28f, 0.48f));
        StretchText("F", formulaCard.transform, "P = VI\n\nE = Pt\n\n1 kWh = 3,600,000 J", 28, TextAlignmentOptions.Center, Color.white, 10);
        var startBtn = BigBtn("StartBtn", introPanel.transform, "START PRACTICAL", new Vector2(0.5f, 0.08f), new Vector2(0, 0), new Vector2(480, 96), green);
        introPanel.AddComponent<PowerEnergyIntroClickToStart>();

        var objectivePanel = Panel(main.transform, "ObjectivePanel", Vector2.zero, Vector2.one, new Vector2(36, 24), new Vector2(-36, -24), Color.white);
        objectivePanel.SetActive(false);
        var objectiveText = StretchText("ObjectiveText", objectivePanel.transform, "", 34, TextAlignmentOptions.TopLeft, new Color(0.10f, 0.18f, 0.28f), 28);

        var equipPanel = Panel(main.transform, "EquipmentPanel", Vector2.zero, Vector2.one, new Vector2(10, 8), new Vector2(-10, -8), Color.white);
        equipPanel.SetActive(false);
        var equipTitle = Text("EquipTitle", equipPanel.transform, "STEP 1 — Tap the equipment needed for this practical.", 26, TextAlignmentOptions.MidlineLeft, Vector2.zero, Vector2.zero, new Vector2(0.02f, 0.91f), new Vector2(0.98f, 1f), new Vector2(0.5f, 0.5f));
        StretchFill(equipTitle.rectTransform);
        equipTitle.enableAutoSizing = true;
        equipTitle.fontSizeMin = 14;
        equipTitle.fontSizeMax = 26;
        equipTitle.overflowMode = TextOverflowModes.Ellipsis;
        var equipHint = Text("EquipHint", equipPanel.transform, "Tap the correct items, then press NEXT STEP at the bottom.", 20, TextAlignmentOptions.MidlineLeft, Vector2.zero, Vector2.zero, new Vector2(0.02f, 0.85f), new Vector2(0.98f, 0.91f), new Vector2(0.5f, 0.5f));
        StretchFill(equipHint.rectTransform);
        equipHint.color = new Color(0.12f, 0.32f, 0.48f);
        equipHint.enableAutoSizing = true;
        equipHint.fontSizeMin = 12;
        equipHint.fontSizeMax = 20;
        equipHint.overflowMode = TextOverflowModes.Ellipsis;

        var requiredAreaHost = Panel(equipPanel.transform, "RequiredArea", new Vector2(0, 0), new Vector2(1, 0.24f), new Vector2(8, 6), new Vector2(-8, -4), new Color(0.92f, 0.96f, 1f));
        var reqLabel = StretchText("ReqLabel", requiredAreaHost.transform, "REQUIRED EQUIPMENT", 16, TextAlignmentOptions.MidlineLeft, new Color(0.08f, 0.28f, 0.48f), 6);
        var reqLabelRt = reqLabel.rectTransform;
        reqLabelRt.anchorMin = new Vector2(0f, 0.72f);
        reqLabelRt.anchorMax = Vector2.one;
        reqLabelRt.offsetMin = new Vector2(8, 0);
        reqLabelRt.offsetMax = new Vector2(-8, -2);
        var requiredArea = Panel(requiredAreaHost.transform, "RequiredCards", new Vector2(0, 0), new Vector2(1, 0.72f), new Vector2(6, 4), new Vector2(-6, -2), new Color(0, 0, 0, 0), false);
        var reqLayout = requiredArea.AddComponent<HorizontalLayoutGroup>();
        reqLayout.spacing = 6;
        reqLayout.padding = new RectOffset(4, 4, 2, 2);
        reqLayout.childAlignment = TextAnchor.MiddleCenter;
        reqLayout.childControlWidth = true;
        reqLayout.childControlHeight = true;
        reqLayout.childForceExpandWidth = true;
        reqLayout.childForceExpandHeight = true;
        requiredArea.AddComponent<PowerEnergyUIDropTarget>().Configure("RequiredEquipment", "Any", Vector2.zero);

        var scroll = CreateScrollAnchored(equipPanel.transform, new Vector2(0f, 0.25f), new Vector2(1f, 0.85f));
        var scrollRt = scroll.GetComponent<RectTransform>();
        scrollRt.offsetMin = new Vector2(8, 4);
        scrollRt.offsetMax = new Vector2(-8, -4);
        var cardPrefab = CreateCardPrefab();
        var equipContinue = BigBtn("EquipContinueBtn", equipPanel.transform, "NEXT STEP", new Vector2(1f, 0f), new Vector2(-160, 18), new Vector2(240, 48), green);
        equipContinue.gameObject.SetActive(false);

        var lab = BuildLaboratory(main.transform, accent, green);
        var exp = BuildExperiment(main.transform, accent, green);
        var dataTablePanel = MakeTextPanel(main.transform, "DataTablePanel", "TableTitle", "OBSERVATION TABLE", "DataTableText");
        var graphBits = BuildGraphPanel(main.transform, green);
        var formulaBits = BuildFormulaPanel(main.transform, green);
        var conclusionBits = BuildConclusionPanel(main.transform, green);
        var questionBits = BuildQuestionPanel(main.transform, header);
        var resultBits = BuildResultPanel(main.transform, accent);

        var feedbackPanel = Panel(canvasObj.transform, "FeedbackPanel", new Vector2(0.16f, 0.36f), new Vector2(0.84f, 0.66f), Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.97f));
        feedbackPanel.SetActive(false);
        var feedbackGroup = feedbackPanel.AddComponent<CanvasGroup>();
        var feedbackText = StretchText("FeedbackText", feedbackPanel.transform, "", 22, TextAlignmentOptions.Center, new Color(0.10f, 0.28f, 0.48f), 14);
        var scoreChangeText = Text("ScoreChange", feedbackPanel.transform, "", 20, TextAlignmentOptions.Bottom, new Vector2(0, 8), new Vector2(220, 28), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));

        var resetConfirm = ConfirmDialog(canvasObj.transform, "ResetConfirm", "Are you sure you want to restart the practical?");
        var retryConfirm = ConfirmDialog(canvasObj.transform, "RetryConfirm", "Are you sure you want to retry this practical?");

        var refs = canvasObj.AddComponent<PowerEnergyUIRefs>();
        refs.UiVersion = 2;
        refs.Title = title; refs.Score = score; refs.Progress = progress; refs.Attempts = attempts;
        refs.StepLabel = stepLabel; refs.ApplianceHeader = applianceHeader; refs.ProgressBar = fillImg; refs.Instruction = instruction;
        refs.IntroPanel = introPanel; refs.IntroText = introText; refs.StartBtn = startBtn;
        refs.ObjectivePanel = objectivePanel; refs.ObjectiveText = objectiveText;
        refs.InstructionBar = instructionBar; refs.EquipmentPanel = equipPanel;
        refs.LaboratoryPanel = lab.panel;
        refs.ExperimentPanel = exp.panel;
        refs.DataTablePanel = dataTablePanel;
        refs.DataTableText = dataTablePanel.transform.Find("DataTableText")?.GetComponent<TextMeshProUGUI>();
        refs.ComparePanel = questionBits.panel;
        refs.CompareText = questionBits.questionText;
        refs.QuestionPanel = questionBits.panel;
        refs.QuestionText = questionBits.questionText;
        refs.QuestionA = questionBits.a; refs.QuestionB = questionBits.b; refs.QuestionC = questionBits.c; refs.QuestionD = questionBits.d;
        refs.QuestionContinue = questionBits.cont;
        refs.QuestionExplanationPanel = questionBits.explain;
        refs.QuestionExplanationText = questionBits.explainText;
        refs.OptAText = questionBits.optA; refs.OptBText = questionBits.optB; refs.OptCText = questionBits.optC; refs.OptDText = questionBits.optD;
        refs.OptionsGroup = questionBits.optionsGroup;
        refs.NumericGroup = questionBits.numericGroup;
        refs.NumericInput = questionBits.numericInput;
        refs.NumericCheck = questionBits.numericCheck;
        refs.FormulaPanel = formulaBits.panel;
        refs.FormulaHelp = formulaBits.help;
        refs.GraphPanel = graphBits.panel;
        refs.GraphTitle = graphBits.title;
        refs.GraphPowerBtn = graphBits.powerBtn;
        refs.GraphEnergyBtn = graphBits.energyBtn;
        refs.ConclusionPanel = conclusionBits.panel;
        refs.ConclusionText = conclusionBits.body;
        refs.ConclusionPreview = conclusionBits.preview;
        refs.PhraseButtons = conclusionBits.phrases;
        refs.VariableContinue = questionBits.cont;
        refs.ConclusionContinue = conclusionBits.continueBtn;
        refs.ResultPanel = resultBits.panel; refs.FinalScore = resultBits.score; refs.ResultDetails = resultBits.details; refs.StatusText = resultBits.status;
        refs.ViewProfileBtn = resultBits.profile; refs.ViewResultsBtn = resultBits.results;
        refs.Next = nextBtn; refs.Reset = resetBtn; refs.Retry = retryBtn;
        refs.ResetConfirm = resetConfirm.panel; refs.ResetYes = resetConfirm.yes; refs.ResetNo = resetConfirm.no;
        refs.RetryConfirm = retryConfirm.panel; refs.RetryYes = retryConfirm.yes; refs.RetryNo = retryConfirm.no;
        refs.FeedbackPanel = feedbackPanel; refs.FeedbackText = feedbackText; refs.ScoreChangeText = scoreChangeText;
        refs.FeedbackGroup = feedbackGroup;
        refs.CardContainer = scroll.content; refs.RequiredArea = requiredArea.transform; refs.CardPrefab = cardPrefab;
        refs.LiveReadings = lab.live; refs.PhysicsText = lab.physics;
        refs.Tray = lab.tray;
        refs.SelectApplianceGroup = exp.selectGroup;
        refs.ReadingsGroup = exp.readingsGroup;
        refs.CalcGroup = exp.calcGroup;
        refs.TimeGroup = exp.timeGroup;
        refs.TimerGroup = exp.timerGroup;
        refs.ApplianceButtons = exp.applianceButtons;
        refs.SwitchBtn = exp.switchBtn;
        refs.TakeVoltageBtn = exp.takeV;
        refs.TakeCurrentBtn = exp.takeI;
        refs.Time10 = exp.t10; refs.Time30 = exp.t30; refs.Time60 = exp.t60;
        refs.TimerStart = exp.start; refs.TimerStop = exp.stop; refs.TimerReset = exp.reset;
        refs.AnotherBtn = exp.another;
        refs.CalcPrompt = exp.calcPrompt;
        refs.CalcHint = exp.calcHint;
        refs.CalcInput = exp.calcInput;
        refs.CalcSubmit = exp.calcSubmit;
        refs.VoltNeedle = lab.voltNeedle;
        refs.AmpNeedle = lab.ampNeedle;
        refs.VoltReading = lab.voltReading;
        refs.AmpReading = lab.ampReading;
        refs.TimerText = exp.timerText;
        refs.ApplianceVisual = exp.applianceVisual;
        refs.ApplianceName = exp.applianceName;
        refs.ApplianceStatus = exp.applianceStatus;
        refs.SupplyZone = lab.supplyZone;
        refs.AmmeterZone = lab.ammeterZone;
        refs.ApplianceZone = lab.applianceZone;
        refs.VoltmeterZone = lab.voltmeterZone;
        refs.WrongSeriesZone = lab.wrongSeries;
        refs.WrongParallelZone = lab.wrongParallel;
        refs.SwitchZone = lab.switchZone;
        refs.WireZone = lab.wireZone;
        refs.SeriesWire = lab.seriesWire;
        refs.ParallelWire = lab.parallelWire;
        refs.CircuitStatus = lab.circuitStatus;
        refs.PowerBars = graphBits.powerBars;
        refs.EnergyBars = graphBits.energyBars;

        instructionBar.transform.SetSiblingIndex(headerP.transform.GetSiblingIndex() + 1);
        canvasObj.transform.Find("BottomBar")?.SetAsLastSibling();
        feedbackPanel.transform.SetAsLastSibling();
        resetConfirm.panel.transform.SetAsLastSibling();
        retryConfirm.panel.transform.SetAsLastSibling();
    }

    public bool WireReferences(bool showWelcome)
    {
        var refs = Object.FindAnyObjectByType<PowerEnergyUIRefs>();
        if (refs == null || PowerEnergyUIManager.Instance == null) return false;
        PowerEnergyFeedbackManager.Instance?.BindUI(refs.FeedbackPanel, refs.FeedbackText, refs.ScoreChangeText, refs.FeedbackGroup);
        PowerEnergyUIManager.Instance.BindAll(refs, showWelcome);
        PowerEnergyEquipmentSelectionManager.Instance?.SetupUI(refs.CardContainer, refs.RequiredArea, refs.CardPrefab);
        PowerEnergyObservationTableManager.Instance?.Bind(refs.DataTableText);
        PowerEnergyResultManager.Instance?.Bind(refs.FinalScore, refs.ResultDetails, refs.StatusText);
        PowerEnergyElectricalEquipmentManager.Instance?.Bind(refs.Tray);
        PowerEnergyComparisonManager.Instance?.Bind(refs.CompareText);
        PowerEnergyGraphController.Instance?.Bind(refs.PowerBars, refs.EnergyBars, refs.GraphTitle);
        PowerEnergyConclusionManager.Instance?.BindPhrases(refs.PhraseButtons);
        PowerEnergyVoltmeterController.Instance?.Bind(refs.VoltReading, refs.VoltNeedle);
        PowerEnergyAmmeterController.Instance?.Bind(refs.AmpReading, refs.AmpNeedle);
        PowerEnergyTimerController.Instance?.Bind(refs.TimerText);
        PowerEnergyApplianceController.Instance?.Bind(
            refs.ApplianceVisual != null ? refs.ApplianceVisual.GetComponent<Image>() : null,
            refs.ApplianceName,
            refs.ApplianceStatus);
        PowerEnergyCircuitConnectionManager.Instance?.Bind(
            refs.SupplyZone, refs.AmmeterZone, refs.ApplianceZone, refs.VoltmeterZone,
            refs.WrongSeriesZone, refs.WrongParallelZone, refs.SwitchZone, refs.WireZone,
            refs.SeriesWire, refs.ParallelWire, refs.CircuitStatus);
        PowerEnergyFailsafeDisplay.Hide();
        return refs.StartBtn != null;
    }

    private LabBits BuildLaboratory(Transform parent, Color accent, Color green)
    {
        var labPanel = Panel(parent, "LaboratoryPanel", Vector2.zero, Vector2.one, new Vector2(8, 4), new Vector2(-8, -4), new Color(0, 0, 0, 0), false);
        labPanel.SetActive(false);

        var tray = Panel(labPanel.transform, "EquipmentTray", new Vector2(0.008f, 0.16f), new Vector2(0.20f, 0.99f), Vector2.zero, Vector2.zero, new Color(0.94f, 0.97f, 1f, 1f));
        var trayHeader = Panel(tray.transform, "TrayHeader", new Vector2(0f, 0.90f), Vector2.one, Vector2.zero, Vector2.zero, new Color(0.08f, 0.28f, 0.48f));
        StretchText("TrayLabel", trayHeader.transform, "EQUIPMENT\nDrag onto the circuit", 22, TextAlignmentOptions.Center, Color.white, 6).fontStyle = FontStyles.Bold;
        var scrollObj = Panel(tray.transform, "Scroll", new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.88f), Vector2.zero, Vector2.zero, new Color(0.94f, 0.97f, 1f, 0.2f));
        var scroll = scrollObj.AddComponent<ScrollRect>();
        var viewport = Panel(scrollObj.transform, "Viewport", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.01f));
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        var trayInner = Panel(viewport.transform, "Items", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var trayInnerRt = trayInner.GetComponent<RectTransform>();
        trayInnerRt.pivot = new Vector2(0.5f, 1f);
        var trayLayout = trayInner.AddComponent<VerticalLayoutGroup>();
        trayLayout.spacing = 10;
        trayLayout.padding = new RectOffset(4, 4, 8, 8);
        trayLayout.childAlignment = TextAnchor.UpperCenter;
        trayLayout.childForceExpandHeight = false;
        trayLayout.childForceExpandWidth = true;
        trayInner.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.content = trayInnerRt;
        scroll.vertical = true;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        var host = Panel(labPanel.transform, "ExperimentHost", new Vector2(0.21f, 0.16f), new Vector2(0.735f, 0.99f), Vector2.zero, Vector2.zero, new Color(0.88f, 0.92f, 0.96f));
        var titleBar = Panel(host.transform, "ExpTitleBar", new Vector2(0.02f, 0.90f), new Vector2(0.98f, 0.985f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.28f, 0.48f), false);
        StretchText("ExpTitle", titleBar.transform, "SIMPLE CIRCUIT   •   Ammeter in SERIES   •   Voltmeter in PARALLEL", 20, TextAlignmentOptions.Center, Color.white, 4).fontStyle = FontStyles.Bold;

        var supplyZone = DropZone(host.transform, "SupplyZone", "POWER SUPPLY", "PowerSupply", new Vector2(0.30f, 0.74f), new Vector2(0.70f, 0.88f), PowerEnergyEquipmentType.PowerSupply);
        var ammeterZone = DropZone(host.transform, "AmmeterZone", "AMMETER (series)", "Ammeter", new Vector2(0.04f, 0.40f), new Vector2(0.32f, 0.70f), PowerEnergyEquipmentType.Ammeter);
        var applianceZone = DropZone(host.transform, "ApplianceZone", "APPLIANCE", "Appliance|Bulb|Fan|Iron|Kettle", new Vector2(0.36f, 0.40f), new Vector2(0.64f, 0.70f), PowerEnergyEquipmentType.ElectricalAppliance);
        var voltmeterZone = DropZone(host.transform, "VoltmeterZone", "VOLTMETER (parallel)", "Voltmeter", new Vector2(0.68f, 0.40f), new Vector2(0.96f, 0.70f), PowerEnergyEquipmentType.Voltmeter);
        var switchZone = DropZone(host.transform, "SwitchZone", "SWITCH", "Switch", new Vector2(0.12f, 0.16f), new Vector2(0.42f, 0.36f), PowerEnergyEquipmentType.Switch);
        var wireZone = DropZone(host.transform, "WireZone", "WIRES", "Wire", new Vector2(0.48f, 0.16f), new Vector2(0.78f, 0.36f), PowerEnergyEquipmentType.Wire);
        var wrongSeries = HiddenDropZone(host.transform, "WrongVoltmeterSeries", "None", new Vector2(0.04f, 0.40f), new Vector2(0.32f, 0.70f));
        var wrongParallel = HiddenDropZone(host.transform, "WrongAmmeterParallel", "None", new Vector2(0.68f, 0.40f), new Vector2(0.96f, 0.70f));

        var seriesWire = Panel(host.transform, "SeriesWire", new Vector2(0.18f, 0.36f), new Vector2(0.82f, 0.38f), Vector2.zero, Vector2.zero, new Color(0.95f, 0.72f, 0.18f));
        seriesWire.GetComponent<Image>().enabled = false;
        seriesWire.transform.SetAsFirstSibling();
        var parallelWire = Panel(host.transform, "ParallelWire", new Vector2(0.64f, 0.40f), new Vector2(0.68f, 0.70f), Vector2.zero, Vector2.zero, new Color(0.82f, 0.22f, 0.22f));
        parallelWire.GetComponent<Image>().enabled = false;
        parallelWire.transform.SetAsFirstSibling();

        var statusBar = Panel(host.transform, "StatusBar", new Vector2(0.02f, 0.015f), new Vector2(0.98f, 0.13f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.28f, 0.48f), false);
        var circuitStatus = StretchText("CircuitStatus", statusBar.transform, "Build the circuit.", 22, TextAlignmentOptions.Center, Color.white, 8);
        circuitStatus.enableAutoSizing = true;
        circuitStatus.fontSizeMin = 16;
        circuitStatus.fontSizeMax = 24;

        var side = Panel(labPanel.transform, "SidePanel", new Vector2(0.745f, 0.16f), new Vector2(0.992f, 0.99f), Vector2.zero, Vector2.zero, Color.white);
        var readHead = Panel(side.transform, "ReadHead", new Vector2(0f, 0.90f), Vector2.one, Vector2.zero, Vector2.zero, new Color(0.08f, 0.28f, 0.48f), false);
        StretchText("ReadTitle", readHead.transform, "READINGS", 22, TextAlignmentOptions.Center, Color.white, 4).fontStyle = FontStyles.Bold;
        var live = StretchText("LiveReadings", side.transform, "", 24, TextAlignmentOptions.TopLeft, new Color(0.10f, 0.18f, 0.28f), 10);
        live.rectTransform.anchorMin = new Vector2(0f, 0.50f);
        live.rectTransform.anchorMax = new Vector2(1f, 0.90f);
        live.rectTransform.offsetMin = new Vector2(12, 8);
        live.rectTransform.offsetMax = new Vector2(-12, -8);
        live.enableAutoSizing = true;
        live.fontSizeMin = 14;
        live.fontSizeMax = 22;
        var formHead = Panel(side.transform, "FormHead", new Vector2(0f, 0.40f), new Vector2(1f, 0.50f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.28f, 0.48f), false);
        StretchText("FormTitle", formHead.transform, "FORMULAS", 22, TextAlignmentOptions.Center, Color.white, 4).fontStyle = FontStyles.Bold;
        var physics = StretchText("PhysicsText", side.transform, "", 22, TextAlignmentOptions.TopLeft, new Color(0.12f, 0.32f, 0.48f), 10);
        physics.rectTransform.anchorMin = Vector2.zero;
        physics.rectTransform.anchorMax = new Vector2(1f, 0.40f);
        physics.rectTransform.offsetMin = new Vector2(12, 10);
        physics.rectTransform.offsetMax = new Vector2(-12, -8);
        physics.enableAutoSizing = true;
        physics.fontSizeMin = 12;
        physics.fontSizeMax = 18;

        var meters = Panel(labPanel.transform, "MetersBar", new Vector2(0.21f, 0.00f), new Vector2(0.735f, 0.15f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.22f, 0.38f));
        var volt = BuildMeterFace(meters.transform, "VoltMeter", "V", new Color(0.78f, 0.22f, 0.22f), new Vector2(0.02f, 0.08f), new Vector2(0.48f, 0.92f));
        var amp = BuildMeterFace(meters.transform, "AmpMeter", "A", new Color(0.15f, 0.48f, 0.78f), new Vector2(0.52f, 0.08f), new Vector2(0.98f, 0.92f));

        var adaptive = labPanel.AddComponent<PowerEnergyAdaptiveLab>();
        adaptive.Bind(
            tray.GetComponent<RectTransform>(),
            scroll,
            trayInnerRt,
            host.GetComponent<RectTransform>(),
            side.GetComponent<RectTransform>(),
            meters.GetComponent<RectTransform>());

        return new LabBits
        {
            panel = labPanel,
            tray = trayInner.transform,
            live = live,
            physics = physics,
            supplyZone = supplyZone,
            ammeterZone = ammeterZone,
            applianceZone = applianceZone,
            voltmeterZone = voltmeterZone,
            wrongSeries = wrongSeries,
            wrongParallel = wrongParallel,
            switchZone = switchZone,
            wireZone = wireZone,
            seriesWire = seriesWire.GetComponent<Image>(),
            parallelWire = parallelWire.GetComponent<Image>(),
            circuitStatus = circuitStatus,
            voltNeedle = volt.needle,
            ampNeedle = amp.needle,
            voltReading = volt.reading,
            ampReading = amp.reading
        };
    }

    private ExpBits BuildExperiment(Transform parent, Color accent, Color green)
    {
        var panel = Panel(parent, "ExperimentPanel", Vector2.zero, Vector2.one, new Vector2(10, 6), new Vector2(-10, -6), new Color(0, 0, 0, 0), false);
        panel.SetActive(false);

        var visual = Panel(panel.transform, "ApplianceVisual", new Vector2(0.72f, 0.55f), new Vector2(0.97f, 0.97f), Vector2.zero, Vector2.zero, Color.white);
        visual.GetComponent<Image>().sprite = PowerEnergyIconFactory.GetSprite(PowerEnergyEquipmentType.Bulb);
        visual.GetComponent<Image>().preserveAspect = true;
        var name = StretchText("ApplianceName", visual.transform, "Select an appliance", 20, TextAlignmentOptions.Bottom, new Color(0.10f, 0.18f, 0.28f), 6);
        var status = Text("ApplianceStatus", visual.transform, "OFF", 22, TextAlignmentOptions.Top, new Vector2(0, -8), new Vector2(160, 32), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));

        var selectGroup = Panel(panel.transform, "SelectApplianceGroup", new Vector2(0.02f, 0.08f), new Vector2(0.70f, 0.96f), Vector2.zero, Vector2.zero, Color.white);
        StretchText("SelTitle", selectGroup.transform, "SELECT AN APPLIANCE", 28, TextAlignmentOptions.Top, new Color(0.08f, 0.28f, 0.48f), 10).fontStyle = FontStyles.Bold;
        var grid = Panel(selectGroup.transform, "Cards", new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.82f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var gl = grid.AddComponent<GridLayoutGroup>();
        gl.cellSize = new Vector2(210, 148);
        gl.spacing = new Vector2(10, 10);
        gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gl.constraintCount = 2;
        grid.AddComponent<PowerEnergyAdaptiveGrid>();
        string[] names = { "Electric Bulb\n≈ 10 W", "Electric Fan\n≈ 80 W", "Electric Iron\n≈ 1000 W", "Electric Kettle\n≈ 2000 W" };
        var types = new[] { PowerEnergyEquipmentType.Bulb, PowerEnergyEquipmentType.Fan, PowerEnergyEquipmentType.Iron, PowerEnergyEquipmentType.Kettle };
        var buttons = new Button[4];
        for (int i = 0; i < 4; i++)
            buttons[i] = ApplianceCard(grid.transform, "App" + i, names[i], types[i], green);

        var readingsGroup = Panel(panel.transform, "ReadingsGroup", new Vector2(0.02f, 0.08f), new Vector2(0.70f, 0.96f), Vector2.zero, Vector2.zero, Color.white);
        readingsGroup.SetActive(false);
        StretchText("RTitle", readingsGroup.transform, "READ VOLTAGE AND CURRENT", 26, TextAlignmentOptions.Top, new Color(0.08f, 0.28f, 0.48f), 10).fontStyle = FontStyles.Bold;
        var switchBtn = BigBtn("SwitchBtn", readingsGroup.transform, "TURN ON", new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(240, 56), new Color(0.90f, 0.55f, 0.16f));
        var takeV = BigBtn("TakeV", readingsGroup.transform, "TAKE VOLTAGE READING", new Vector2(0.28f, 0.22f), Vector2.zero, new Vector2(240, 56), new Color(0.78f, 0.22f, 0.22f));
        var takeI = BigBtn("TakeI", readingsGroup.transform, "TAKE CURRENT READING", new Vector2(0.72f, 0.22f), Vector2.zero, new Vector2(240, 56), accent);

        var calcGroup = Panel(panel.transform, "CalcGroup", new Vector2(0.02f, 0.08f), new Vector2(0.70f, 0.96f), Vector2.zero, Vector2.zero, Color.white);
        calcGroup.SetActive(false);
        var calcPrompt = StretchText("CalcPrompt", calcGroup.transform, "CALCULATE", 28, TextAlignmentOptions.Top, new Color(0.08f, 0.28f, 0.48f), 12);
        calcPrompt.fontStyle = FontStyles.Bold;
        var calcHint = StretchText("CalcHint", calcGroup.transform, "", 22, TextAlignmentOptions.Center, new Color(0.16f, 0.28f, 0.40f), 12);
        calcHint.rectTransform.anchorMin = new Vector2(0.06f, 0.42f);
        calcHint.rectTransform.anchorMax = new Vector2(0.94f, 0.72f);
        var calcInput = MakeInput(calcGroup.transform, "CalcInput", new Vector2(0.18f, 0.22f), new Vector2(0.62f, 0.38f));
        var calcSubmit = BigBtn("CalcSubmit", calcGroup.transform, "CHECK", new Vector2(0.80f, 0.30f), Vector2.zero, new Vector2(160, 52), green);

        var timeGroup = Panel(panel.transform, "TimeGroup", new Vector2(0.02f, 0.08f), new Vector2(0.70f, 0.96f), Vector2.zero, Vector2.zero, Color.white);
        timeGroup.SetActive(false);
        StretchText("TTitle", timeGroup.transform, "ENERGY CONSUMPTION\nChoose how long the appliance operates.", 26, TextAlignmentOptions.Top, new Color(0.08f, 0.28f, 0.48f), 12);
        var t10 = BigBtn("T10", timeGroup.transform, "10 seconds", new Vector2(0.22f, 0.32f), Vector2.zero, new Vector2(200, 56), accent);
        var t30 = BigBtn("T30", timeGroup.transform, "30 seconds", new Vector2(0.50f, 0.32f), Vector2.zero, new Vector2(200, 56), accent);
        var t60 = BigBtn("T60", timeGroup.transform, "60 seconds", new Vector2(0.78f, 0.32f), Vector2.zero, new Vector2(200, 56), accent);

        var timerGroup = Panel(panel.transform, "TimerGroup", new Vector2(0.02f, 0.08f), new Vector2(0.70f, 0.96f), Vector2.zero, Vector2.zero, Color.white);
        timerGroup.SetActive(false);
        var timerText = StretchText("TimerText", timerGroup.transform, "00:00", 72, TextAlignmentOptions.Center, new Color(0.08f, 0.28f, 0.48f), 8);
        timerText.rectTransform.anchorMin = new Vector2(0.2f, 0.45f);
        timerText.rectTransform.anchorMax = new Vector2(0.8f, 0.85f);
        timerText.enableAutoSizing = true;
        timerText.fontSizeMin = 28;
        timerText.fontSizeMax = 72;
        var start = BigBtn("TStart", timerGroup.transform, "START", new Vector2(0.25f, 0.22f), Vector2.zero, new Vector2(150, 48), green);
        var stop = BigBtn("TStop", timerGroup.transform, "STOP", new Vector2(0.50f, 0.22f), Vector2.zero, new Vector2(150, 48), new Color(0.78f, 0.28f, 0.22f));
        var reset = BigBtn("TReset", timerGroup.transform, "RESET", new Vector2(0.75f, 0.22f), Vector2.zero, new Vector2(150, 48), new Color(0.45f, 0.50f, 0.56f));

        var another = BigBtn("Another", panel.transform, "INVESTIGATE ANOTHER APPLIANCE", new Vector2(0.36f, 0.06f), Vector2.zero, new Vector2(340, 48), green);
        another.gameObject.SetActive(false);

        return new ExpBits
        {
            panel = panel,
            selectGroup = selectGroup,
            readingsGroup = readingsGroup,
            calcGroup = calcGroup,
            timeGroup = timeGroup,
            timerGroup = timerGroup,
            applianceButtons = buttons,
            switchBtn = switchBtn,
            takeV = takeV,
            takeI = takeI,
            t10 = t10, t30 = t30, t60 = t60,
            start = start, stop = stop, reset = reset,
            another = another,
            calcPrompt = calcPrompt,
            calcHint = calcHint,
            calcInput = calcInput,
            calcSubmit = calcSubmit,
            timerText = timerText,
            applianceVisual = visual,
            applianceName = name,
            applianceStatus = status
        };
    }

    private GraphBits BuildGraphPanel(Transform parent, Color green)
    {
        var panel = Panel(parent, "GraphPanel", Vector2.zero, Vector2.one, new Vector2(24, 16), new Vector2(-24, -16), Color.white);
        panel.SetActive(false);
        var title = StretchText("GraphTitle", panel.transform, "APPLIANCE vs POWER (W)", 28, TextAlignmentOptions.Top, new Color(0.08f, 0.28f, 0.48f), 10);
        title.fontStyle = FontStyles.Bold;
        var powerBars = BuildBars(panel.transform, "PowerBars");
        var energyBars = BuildBars(panel.transform, "EnergyBars");
        energyBars.gameObject.SetActive(false);
        var powerBtn = BigBtn("GraphPower", panel.transform, "POWER GRAPH", new Vector2(0.35f, 0.08f), Vector2.zero, new Vector2(260, 64), new Color(0.12f, 0.52f, 0.72f));
        var energyBtn = BigBtn("GraphEnergy", panel.transform, "ENERGY GRAPH", new Vector2(0.65f, 0.08f), Vector2.zero, new Vector2(260, 64), green);
        return new GraphBits { panel = panel, title = title, powerBars = powerBars, energyBars = energyBars, powerBtn = powerBtn, energyBtn = energyBtn };
    }

    private Transform BuildBars(Transform parent, string name)
    {
        var root = Panel(parent, name, new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.82f), Vector2.zero, Vector2.zero, new Color(0.94f, 0.97f, 1f), false);
        var layout = root.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 14;
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.childForceExpandHeight = true;
        string[] labels = { "Bulb", "Fan", "Iron", "Kettle" };
        for (int i = 0; i < 4; i++)
        {
            var row = Panel(root.transform, labels[i], Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 70;
            var label = StretchText("Label", row.transform, labels[i], 22, TextAlignmentOptions.MidlineLeft, new Color(0.10f, 0.18f, 0.28f), 4);
            label.rectTransform.anchorMax = new Vector2(0.18f, 1f);
            var track = Panel(row.transform, "Track", new Vector2(0.20f, 0.25f), new Vector2(0.78f, 0.75f), Vector2.zero, Vector2.zero, new Color(0.82f, 0.88f, 0.94f));
            var fill = Panel(track.transform, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.12f, 0.52f, 0.72f));
            var fillImg = fill.GetComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 0f;
            var value = StretchText("Value", row.transform, "—", 20, TextAlignmentOptions.MidlineLeft, new Color(0.10f, 0.18f, 0.28f), 4);
            value.rectTransform.anchorMin = new Vector2(0.80f, 0f);
        }
        return root.transform;
    }

    private FormulaBits BuildFormulaPanel(Transform parent, Color green)
    {
        var panel = Panel(parent, "FormulaPanel", Vector2.zero, Vector2.one, new Vector2(24, 16), new Vector2(-24, -16), Color.white);
        panel.SetActive(false);
        var help = StretchText("Help", panel.transform, "FORMULA MATCHING\nTap the matching formula for each idea.", 24, TextAlignmentOptions.Top, new Color(0.08f, 0.28f, 0.48f), 10);
        var row = Panel(panel.transform, "Rows", new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.78f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var layout = row.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 16;
        layout.childForceExpandHeight = true;
        MakeFormulaRow(row.transform, "Power", "Power", "P = VI", "FormulaPower");
        MakeFormulaRow(row.transform, "Energy", "Electrical Energy", "E = Pt", "FormulaEnergy");
        MakeFormulaRow(row.transform, "Kwh", "kWh conversion", "E(J) / 3,600,000", "FormulaKwh");
        return new FormulaBits { panel = panel, help = help };
    }

    private void MakeFormulaRow(Transform parent, string id, string label, string formula, string zone)
    {
        var row = Panel(parent, id, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.94f, 0.97f, 1f));
        var le = row.AddComponent<LayoutElement>();
        le.preferredHeight = 90;
        var left = StretchText("L", row.transform, label, 24, TextAlignmentOptions.MidlineLeft, new Color(0.10f, 0.18f, 0.28f), 10);
        left.rectTransform.anchorMax = new Vector2(0.42f, 1f);
        var btn = BigBtn("Match", row.transform, formula, new Vector2(0.72f, 0.5f), Vector2.zero, new Vector2(360, 64), new Color(0.12f, 0.52f, 0.72f));
        var choice = btn.gameObject.AddComponent<PowerEnergyFormulaChoiceButton>();
        choice.Configure(id, zone);
        row.AddComponent<PowerEnergyUIDropTarget>().Configure(zone, id, Vector2.zero);
    }

    private ConclusionBits BuildConclusionPanel(Transform parent, Color green)
    {
        var panel = Panel(parent, "ConclusionPanel", Vector2.zero, Vector2.one, new Vector2(28, 16), new Vector2(-28, -16), Color.white);
        panel.SetActive(false);
        var titleBar = Panel(panel.transform, "TitleBar", new Vector2(0.03f, 0.88f), new Vector2(0.97f, 0.98f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.28f, 0.48f), false);
        var body = StretchText("ConclusionText", titleBar.transform, "CONCLUSION  —  tap the three sentences in the correct order", 26, TextAlignmentOptions.Center, Color.white, 8);
        body.fontStyle = FontStyles.Bold;
        var previewBox = Panel(panel.transform, "PreviewBox", new Vector2(0.03f, 0.52f), new Vector2(0.97f, 0.86f), Vector2.zero, Vector2.zero, new Color(0.92f, 0.97f, 0.94f));
        var preview = StretchText("Preview", previewBox.transform, "Your conclusion will appear here.", 24, TextAlignmentOptions.TopLeft, new Color(0.08f, 0.36f, 0.28f), 16);
        preview.enableAutoSizing = true;
        preview.fontSizeMin = 20;
        preview.fontSizeMax = 28;
        var phrasesHost = Panel(panel.transform, "Phrases", new Vector2(0.03f, 0.04f), new Vector2(0.97f, 0.50f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var layout = phrasesHost.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12;
        layout.padding = new RectOffset(4, 4, 4, 4);
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = true;
        var phrases = new Button[3];
        string[] texts =
        {
            "The electrical power of an appliance tells us the rate at which it consumes electrical energy.",
            "Electrical energy consumed increases when the power or operating time increases.",
            "Power can be calculated using P = VI and energy can be calculated using E = Pt."
        };
        for (int i = 0; i < 3; i++)
        {
            phrases[i] = Btn("P" + i, phrasesHost.transform, texts[i], new Color(0.12f, 0.52f, 0.72f), 0, 78);
            var tmp = phrases[i].GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 18;
                tmp.fontSizeMax = 24;
            }
        }
        Button cont = null;
        return new ConclusionBits { panel = panel, body = body, preview = preview, phrases = phrases, continueBtn = cont };
    }

    private QuestionBits BuildQuestionPanel(Transform parent, Color header)
    {
        var panel = Panel(parent, "QuestionPanel", Vector2.zero, Vector2.one, new Vector2(24, 10), new Vector2(-24, -10), Color.white);
        panel.SetActive(false);
        var questionText = StretchText("QuestionText", panel.transform, "", 32, TextAlignmentOptions.TopLeft, new Color(0.10f, 0.18f, 0.28f), 16);
        questionText.rectTransform.anchorMin = new Vector2(0.04f, 0.70f);
        questionText.rectTransform.anchorMax = new Vector2(0.96f, 0.97f);
        questionText.enableAutoSizing = true;
        questionText.fontSizeMin = 24;
        questionText.fontSizeMax = 36;
        var options = Panel(panel.transform, "Options", new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.68f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var ol = options.AddComponent<VerticalLayoutGroup>();
        ol.spacing = 10;
        ol.childForceExpandHeight = false;
        var a = ChoiceBtn("A", options.transform, "A", "");
        var b = ChoiceBtn("B", options.transform, "B", "");
        var c = ChoiceBtn("C", options.transform, "C", "");
        var d = ChoiceBtn("D", options.transform, "D", "");
        var numeric = Panel(panel.transform, "NumericGroup", new Vector2(0.10f, 0.18f), new Vector2(0.90f, 0.62f), Vector2.zero, Vector2.zero, new Color(0.94f, 0.97f, 1f));
        numeric.SetActive(false);
        StretchText("NHint", numeric.transform, "Type your answer, then press CHECK.", 24, TextAlignmentOptions.Top, new Color(0.16f, 0.28f, 0.40f), 8);
        var numericInput = MakeInput(numeric.transform, "NumericInput", new Vector2(0.08f, 0.28f), new Vector2(0.62f, 0.62f));
        var numericCheck = BigBtn("NumericCheck", numeric.transform, "CHECK", new Vector2(0.80f, 0.45f), Vector2.zero, new Vector2(180, 64), new Color(0.12f, 0.62f, 0.42f));
        var explain = Panel(panel.transform, "Explain", new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.36f), Vector2.zero, Vector2.zero, new Color(0.95f, 0.98f, 0.95f));
        explain.SetActive(false);
        var explainText = StretchText("ExplainText", explain.transform, "", 24, TextAlignmentOptions.Center, new Color(0.08f, 0.42f, 0.28f), 10);
        explainText.enableAutoSizing = true;
        explainText.fontSizeMin = 18;
        explainText.fontSizeMax = 26;
        Button cont = null;
        return new QuestionBits
        {
            panel = panel,
            questionText = questionText,
            a = a, b = b, c = c, d = d,
            optA = a.transform.Find("Body")?.GetComponent<TextMeshProUGUI>(),
            optB = b.transform.Find("Body")?.GetComponent<TextMeshProUGUI>(),
            optC = c.transform.Find("Body")?.GetComponent<TextMeshProUGUI>(),
            optD = d.transform.Find("Body")?.GetComponent<TextMeshProUGUI>(),
            optionsGroup = options,
            numericGroup = numeric,
            numericInput = numericInput,
            numericCheck = numericCheck,
            explain = explain,
            explainText = explainText,
            cont = cont
        };
    }

    private ResultBits BuildResultPanel(Transform parent, Color accent)
    {
        var panel = Panel(parent, "ResultPanel", Vector2.zero, Vector2.one, new Vector2(20, 10), new Vector2(-20, -10), Color.white);
        panel.SetActive(false);
        var left = Panel(panel.transform, "LeftCol", new Vector2(0.02f, 0.14f), new Vector2(0.48f, 0.97f), Vector2.zero, Vector2.zero, new Color(0.94f, 0.97f, 1f));
        var score = StretchText("FinalScore", left.transform, "", 26, TextAlignmentOptions.TopLeft, new Color(0.08f, 0.28f, 0.48f), 16);
        score.enableAutoSizing = true;
        score.fontSizeMin = 20;
        score.fontSizeMax = 28;
        var right = Panel(panel.transform, "RightCol", new Vector2(0.50f, 0.14f), new Vector2(0.98f, 0.97f), Vector2.zero, Vector2.zero, new Color(0.96f, 0.98f, 1f));
        var details = StretchText("ResultDetails", right.transform, "", 22, TextAlignmentOptions.TopLeft, new Color(0.10f, 0.18f, 0.28f), 14);
        details.enableAutoSizing = true;
        details.fontSizeMin = 16;
        details.fontSizeMax = 22;
        var status = StretchText("StatusText", panel.transform, "", 24, TextAlignmentOptions.MidlineLeft, new Color(0.12f, 0.52f, 0.32f), 8);
        status.rectTransform.anchorMin = new Vector2(0.03f, 0.03f);
        status.rectTransform.anchorMax = new Vector2(0.48f, 0.12f);
        status.fontStyle = FontStyles.Bold;
        Button results = null;
        var profile = BigBtn("ViewProfile", panel.transform, "BACK TO PROFILE", new Vector2(0.82f, 0.07f), Vector2.zero, new Vector2(280, 64), new Color(0.08f, 0.28f, 0.48f));
        return new ResultBits { panel = panel, score = score, details = details, status = status, results = results, profile = profile };
    }

    private GameObject MakeTextPanel(Transform parent, string name, string titleName, string title, string bodyName)
    {
        var panel = Panel(parent, name, Vector2.zero, Vector2.one, new Vector2(24, 12), new Vector2(-24, -12), Color.white);
        panel.SetActive(false);
        StretchText(titleName, panel.transform, title, 26, TextAlignmentOptions.Top, new Color(0.08f, 0.28f, 0.48f), 10).fontStyle = FontStyles.Bold;
        var body = StretchText(bodyName, panel.transform, "", 24, TextAlignmentOptions.TopLeft, new Color(0.10f, 0.18f, 0.28f), 16);
        body.rectTransform.anchorMin = new Vector2(0.03f, 0.04f);
        body.rectTransform.anchorMax = new Vector2(0.97f, 0.88f);
        body.enableAutoSizing = true;
        body.fontSizeMin = 18;
        body.fontSizeMax = 26;
        return panel;
    }

    private PowerEnergyUIDropTarget DropZone(Transform parent, string name, string label, string accepted, Vector2 aMin, Vector2 aMax, PowerEnergyEquipmentType icon)
    {
        var zone = Panel(parent, name, aMin, aMax, Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.92f));
        var drop = zone.AddComponent<PowerEnergyUIDropTarget>();
        drop.Configure(name, accepted, Vector2.zero);
        var iconObj = Panel(zone.transform, "Icon", new Vector2(0.12f, 0.32f), new Vector2(0.88f, 0.92f), Vector2.zero, Vector2.zero, Color.white, false);
        iconObj.GetComponent<Image>().sprite = PowerEnergyIconFactory.GetSprite(icon);
        iconObj.GetComponent<Image>().preserveAspect = true;
        var hint = StretchText("Hint", zone.transform, label, 18, TextAlignmentOptions.Bottom, new Color(0.10f, 0.22f, 0.36f), 4);
        hint.enableAutoSizing = true;
        hint.fontSizeMin = 14;
        hint.fontSizeMax = 20;
        hint.fontStyle = FontStyles.Bold;
        return drop;
    }

    private PowerEnergyUIDropTarget HiddenDropZone(Transform parent, string name, string accepted, Vector2 aMin, Vector2 aMax)
    {
        var zone = Panel(parent, name, aMin, aMax, Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.01f), false);
        var drop = zone.AddComponent<PowerEnergyUIDropTarget>();
        drop.Configure(name, accepted, Vector2.zero);
        zone.GetComponent<Image>().raycastTarget = false;
        return drop;
    }

    private MeterBits BuildMeterFace(Transform parent, string name, string unit, Color ring, Vector2 aMin, Vector2 aMax)
    {
        var face = Panel(parent, name, aMin, aMax, Vector2.zero, Vector2.zero, new Color(0.12f, 0.18f, 0.28f));
        var dial = Panel(face.transform, "Dial", new Vector2(0.05f, 0.12f), new Vector2(0.42f, 0.92f), Vector2.zero, Vector2.zero, Color.white, false);
        dial.GetComponent<Image>().sprite = PowerEnergyIconFactory.GetSprite(unit == "V" ? PowerEnergyEquipmentType.Voltmeter : PowerEnergyEquipmentType.Ammeter);
        dial.GetComponent<Image>().preserveAspect = true;
        var needle = Panel(dial.transform, "Needle", new Vector2(0.46f, 0.18f), new Vector2(0.54f, 0.78f), Vector2.zero, Vector2.zero, ring, false);
        needle.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
        var reading = StretchText("Reading", face.transform, unit == "V" ? "--- V" : "--- A", 32, TextAlignmentOptions.MidlineLeft, Color.white, 8);
        reading.rectTransform.anchorMin = new Vector2(0.46f, 0.15f);
        return new MeterBits { needle = needle.GetComponent<RectTransform>(), reading = reading };
    }

    private Button ApplianceCard(Transform parent, string name, string label, PowerEnergyEquipmentType type, Color green)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<Image>().color = Color.Lerp(PowerEnergyIconFactory.GetColor(type), Color.white, 0.35f);
        var btn = obj.AddComponent<Button>();
        var icon = Panel(obj.transform, "Icon", new Vector2(0.18f, 0.38f), new Vector2(0.82f, 0.92f), Vector2.zero, Vector2.zero, Color.white, false);
        icon.GetComponent<Image>().sprite = PowerEnergyIconFactory.GetSprite(type);
        icon.GetComponent<Image>().preserveAspect = true;
        StretchText("L", obj.transform, label, 20, TextAlignmentOptions.Bottom, new Color(0.10f, 0.18f, 0.28f), 6);
        return btn;
    }

    private ConfirmBits ConfirmDialog(Transform parent, string name, string message)
    {
        var panel = Panel(parent, name, new Vector2(0.28f, 0.38f), new Vector2(0.72f, 0.62f), Vector2.zero, Vector2.zero, Color.white);
        panel.SetActive(false);
        StretchText("Msg", panel.transform, message, 24, TextAlignmentOptions.Center, new Color(0.10f, 0.18f, 0.28f), 16);
        var row = Panel(panel.transform, "Btns", new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.38f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 18;
        layout.childForceExpandWidth = true;
        var yes = Btn("Yes", row.transform, "YES", new Color(0.75f, 0.2f, 0.2f), 0, 50);
        var no = Btn("No", row.transform, "NO", new Color(0.15f, 0.48f, 0.78f), 0, 50);
        return new ConfirmBits { panel = panel, yes = yes, no = no };
    }

    private GameObject Panel(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax, Color color, bool raycast = true)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = offMin; rt.offsetMax = offMax;
        var img = obj.AddComponent<Image>();
        img.sprite = PowerEnergyIconFactory.White();
        img.color = color;
        img.raycastTarget = raycast && color.a > 0.01f;
        return obj;
    }

    private TextMeshProUGUI Text(string name, Transform parent, string content, float size, TextAlignmentOptions align, Vector2 pos, Vector2 sizeDelta, Vector2 aMin = default, Vector2 aMax = default, Vector2 pivot = default)
    {
        if (aMin == default) { aMin = new Vector2(0, 1); aMax = aMin; pivot = aMin; }
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot; rt.anchoredPosition = pos; rt.sizeDelta = sizeDelta;
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) tmp.font = defaultFont;
        tmp.text = content; tmp.fontSize = size; tmp.alignment = align;
        tmp.color = new Color(0.10f, 0.18f, 0.28f); tmp.raycastTarget = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
#pragma warning disable CS0618
        tmp.enableWordWrapping = name == "Instruction" || name.Contains("Objective") || name.Contains("Intro");
#pragma warning restore CS0618
        return tmp;
    }

    private TextMeshProUGUI StretchText(string name, Transform parent, string content, float size, TextAlignmentOptions align, Color color, float pad)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(pad, pad); rt.offsetMax = new Vector2(-pad, -pad);
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) tmp.font = defaultFont;
        tmp.text = content; tmp.fontSize = size; tmp.alignment = align; tmp.color = color;
#pragma warning disable CS0618
        tmp.enableWordWrapping = true;
#pragma warning restore CS0618
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        return tmp;
    }

    private TMP_InputField MakeInput(Transform parent, string name, Vector2 aMin, Vector2 aMax)
    {
        var obj = Panel(parent, name, aMin, aMax, Vector2.zero, Vector2.zero, new Color(0.94f, 0.97f, 1f));
        var input = obj.AddComponent<TMP_InputField>();
        var text = StretchText("Text", obj.transform, "", 32, TextAlignmentOptions.MidlineLeft, new Color(0.08f, 0.18f, 0.28f), 10);
        text.raycastTarget = true;
        var placeholder = StretchText("Placeholder", obj.transform, "Enter answer", 24, TextAlignmentOptions.MidlineLeft, new Color(0.45f, 0.52f, 0.60f), 10);
        input.textViewport = text.rectTransform;
        input.textComponent = text;
        input.placeholder = placeholder;
        input.contentType = TMP_InputField.ContentType.DecimalNumber;
        input.fontAsset = defaultFont;
        return input;
    }

    private Button ChoiceBtn(string name, Transform parent, string letter, string text)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var le = obj.AddComponent<LayoutElement>();
        le.preferredHeight = 88; le.minHeight = 80;
        obj.AddComponent<Image>().color = Color.white;
        var btn = obj.AddComponent<Button>();
        var letterBg = Panel(obj.transform, "Letter", new Vector2(0f, 0.12f), new Vector2(0f, 0.88f), new Vector2(10, 0), new Vector2(68, 0), new Color(0.12f, 0.52f, 0.72f), false);
        StretchText("LetterText", letterBg.transform, letter, 28, TextAlignmentOptions.Center, Color.white, 0).fontStyle = FontStyles.Bold;
        var bodyObj = new GameObject("Body");
        bodyObj.transform.SetParent(obj.transform, false);
        var bodyRt = bodyObj.AddComponent<RectTransform>();
        bodyRt.anchorMin = Vector2.zero; bodyRt.anchorMax = Vector2.one;
        bodyRt.offsetMin = new Vector2(82, 8); bodyRt.offsetMax = new Vector2(-16, -8);
        var body = bodyObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) body.font = defaultFont;
        body.text = text;         body.fontSize = 26; body.fontStyle = FontStyles.Bold;
        body.alignment = TextAlignmentOptions.MidlineLeft;
        body.color = new Color(0.10f, 0.18f, 0.28f);
        body.raycastTarget = false;
        body.enableAutoSizing = true;
        body.fontSizeMin = 20;
        body.fontSizeMax = 28;
        return btn;
    }

    private Button Btn(string name, Transform parent, string label, Color bg, float w, float h)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var le = obj.AddComponent<LayoutElement>();
        if (w > 0) le.preferredWidth = w;
        le.preferredHeight = h; le.minHeight = h;
        var img = obj.AddComponent<Image>();
        img.sprite = PowerEnergyIconFactory.White();
        img.color = bg;
        var btn = obj.AddComponent<Button>();
        var txt = StretchText("Text", obj.transform, label, 24, TextAlignmentOptions.Center, Color.white, 4);
        txt.fontStyle = FontStyles.Bold;
        txt.enableAutoSizing = true;
        txt.fontSizeMin = 11;
        txt.fontSizeMax = 18;
        return btn;
    }

    private Button BigBtn(string name, Transform parent, string label, Vector2 anchor, Vector2 pos, Vector2 size, Color bg)
    {
        var obj = Panel(parent, name, anchor, anchor, Vector2.zero, size, bg);
        obj.GetComponent<RectTransform>().anchoredPosition = pos;
        obj.GetComponent<RectTransform>().sizeDelta = size;
        var btn = obj.AddComponent<Button>();
        var txt = StretchText("Text", obj.transform, label, 22, TextAlignmentOptions.Center, Color.white, 6);
        txt.fontStyle = FontStyles.Bold;
        txt.enableAutoSizing = true;
        txt.fontSizeMin = 11;
        txt.fontSizeMax = 22;
        return btn;
    }

    private ScrollRect CreateScrollAnchored(Transform parent, Vector2 aMin, Vector2 aMax)
    {
        var scrollObj = Panel(parent, "Scroll", aMin, aMax, Vector2.zero, Vector2.zero, new Color(0.94f, 0.97f, 1f));
        var scroll = scrollObj.AddComponent<ScrollRect>();
        var viewport = Panel(scrollObj.transform, "Viewport", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.01f));
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        var content = Panel(viewport.transform, "Content", new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.01f));
        var contentRt = content.GetComponent<RectTransform>();
        contentRt.pivot = new Vector2(0.5f, 1f);
        var grid = content.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(210, 148);
        grid.spacing = new Vector2(10, 10);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.padding = new RectOffset(10, 10, 10, 10);
        content.AddComponent<PowerEnergyAdaptiveGrid>();
        var fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.content = contentRt;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        return scroll;
    }

    private GameObject CreateCardPrefab()
    {
        var card = new GameObject("EquipmentCardPrefab");
        card.transform.SetParent(transform, false);
        card.AddComponent<RectTransform>().sizeDelta = new Vector2(210, 148);
        card.AddComponent<Image>().color = new Color(0.94f, 0.97f, 1f);
        card.AddComponent<PowerEnergyEquipmentCardUI>();
        card.AddComponent<Button>();
        card.SetActive(false);
        return card;
    }

    private static void StretchFill(RectTransform rt)
    {
        if (rt == null) return;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }

    private struct LabBits
    {
        public GameObject panel;
        public Transform tray;
        public TextMeshProUGUI live, physics, circuitStatus, voltReading, ampReading;
        public PowerEnergyUIDropTarget supplyZone, ammeterZone, applianceZone, voltmeterZone, wrongSeries, wrongParallel, switchZone, wireZone;
        public Image seriesWire, parallelWire;
        public RectTransform voltNeedle, ampNeedle;
    }

    private struct ExpBits
    {
        public GameObject panel, selectGroup, readingsGroup, calcGroup, timeGroup, timerGroup, applianceVisual;
        public Button[] applianceButtons;
        public Button switchBtn, takeV, takeI, t10, t30, t60, start, stop, reset, another, calcSubmit;
        public TextMeshProUGUI calcPrompt, calcHint, timerText, applianceName, applianceStatus;
        public TMP_InputField calcInput;
    }

    private struct GraphBits
    {
        public GameObject panel;
        public TextMeshProUGUI title;
        public Transform powerBars, energyBars;
        public Button powerBtn, energyBtn;
    }

    private struct FormulaBits
    {
        public GameObject panel;
        public TextMeshProUGUI help;
    }

    private struct ConclusionBits
    {
        public GameObject panel;
        public TextMeshProUGUI body, preview;
        public Button[] phrases;
        public Button continueBtn;
    }

    private struct QuestionBits
    {
        public GameObject panel, explain, optionsGroup, numericGroup;
        public TextMeshProUGUI questionText, explainText, optA, optB, optC, optD;
        public Button a, b, c, d, cont, numericCheck;
        public TMP_InputField numericInput;
    }

    private struct ResultBits
    {
        public GameObject panel;
        public TextMeshProUGUI score, details, status;
        public Button results, profile;
    }

    private struct ConfirmBits
    {
        public GameObject panel;
        public Button yes, no;
    }

    private struct MeterBits
    {
        public RectTransform needle;
        public TextMeshProUGUI reading;
    }
}

public class PowerEnergyUIRefs : MonoBehaviour
{
    public int UiVersion;
    public TextMeshProUGUI Title, Score, Progress, Attempts, StepLabel, ApplianceHeader, Instruction;
    public TextMeshProUGUI IntroText, ObjectiveText, ConclusionText, ConclusionPreview, CompareText, QuestionText, DataTableText;
    public TextMeshProUGUI FinalScore, ResultDetails, StatusText, FeedbackText, ScoreChangeText;
    public TextMeshProUGUI LiveReadings, PhysicsText, QuestionExplanationText, OptAText, OptBText, OptCText, OptDText;
    public TextMeshProUGUI GraphTitle, FormulaHelp, CalcPrompt, CalcHint, TimerText, ApplianceName, ApplianceStatus;
    public TextMeshProUGUI VoltReading, AmpReading, CircuitStatus;
    public Image ProgressBar, SeriesWire, ParallelWire;
    public RectTransform VoltNeedle, AmpNeedle;
    public GameObject IntroPanel, ObjectivePanel, InstructionBar, EquipmentPanel, LaboratoryPanel, ExperimentPanel;
    public GameObject DataTablePanel, ComparePanel, QuestionPanel, ConclusionPanel, ResultPanel, FormulaPanel, GraphPanel;
    public GameObject ResetConfirm, RetryConfirm, FeedbackPanel, CardPrefab, QuestionExplanationPanel, OptionsGroup, NumericGroup;
    public GameObject SelectApplianceGroup, ReadingsGroup, CalcGroup, TimeGroup, TimerGroup, ApplianceVisual;
    public Transform CardContainer, RequiredArea, Tray, PowerBars, EnergyBars;
    public Button StartBtn, Next, Reset, Retry, ResetYes, ResetNo, RetryYes, RetryNo, ViewProfileBtn, ViewResultsBtn;
    public Button QuestionA, QuestionB, QuestionC, QuestionD, QuestionContinue, CalcSubmit;
    public Button SwitchBtn, TakeVoltageBtn, TakeCurrentBtn, Time10, Time30, Time60, TimerStart, TimerStop, TimerReset, AnotherBtn;
    public Button GraphPowerBtn, GraphEnergyBtn, VariableContinue, ConclusionContinue;
    public Button[] PhraseButtons, ApplianceButtons;
    public TMP_InputField CalcInput, NumericInput;
    public Button NumericCheck;
    public CanvasGroup FeedbackGroup;
    public PowerEnergyUIDropTarget SupplyZone, AmmeterZone, ApplianceZone, VoltmeterZone, WrongSeriesZone, WrongParallelZone, SwitchZone, WireZone;
}

public class PowerEnergyAdaptiveGrid : MonoBehaviour
{
    private GridLayoutGroup grid;
    private RectTransform viewport;
    private int lastWidth;

    private void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        var scroll = GetComponentInParent<ScrollRect>();
        viewport = scroll != null ? scroll.viewport : transform.parent as RectTransform;
    }

    private void OnRectTransformDimensionsChange() => Apply();
    private void LateUpdate() => Apply();

    private void Apply()
    {
        if (grid == null) return;
        var host = viewport != null ? viewport : transform.parent as RectTransform;
        if (host == null) return;
        int width = Mathf.RoundToInt(host.rect.width);
        if (width < 80 || width == lastWidth) return;
        lastWidth = width;

        int cols = width < 720 ? 2 : width < 1100 ? 3 : width < 1500 ? 4 : 5;
        float pad = grid.padding.left + grid.padding.right;
        float space = grid.spacing.x * (cols - 1);
        float cellW = Mathf.Clamp((width - pad - space) / cols, 120f, 220f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = cols;
        grid.cellSize = new Vector2(cellW, cellW * 0.78f);
    }
}

public class PowerEnergyAdaptiveLab : MonoBehaviour
{
    private RectTransform tray;
    private ScrollRect trayScroll;
    private RectTransform trayItems;
    private RectTransform host;
    private RectTransform side;
    private RectTransform action;
    private int lastW;
    private int lastH;

    public void Bind(
        RectTransform trayRt,
        ScrollRect scroll,
        RectTransform items,
        RectTransform hostRt,
        RectTransform sideRt,
        RectTransform actionRt)
    {
        tray = trayRt;
        trayScroll = scroll;
        trayItems = items;
        host = hostRt;
        side = sideRt;
        action = actionRt;
    }

    private void LateUpdate() => Apply();

    private void Apply()
    {
        var panel = transform as RectTransform;
        if (panel == null) return;
        int w = Mathf.RoundToInt(panel.rect.width);
        int h = Mathf.RoundToInt(panel.rect.height);
        if (w < 80 || (w == lastW && h == lastH)) return;
        lastW = w;
        lastH = h;

        bool narrow = w < 900 || w < h * 0.95f;
        if (narrow)
        {
            Set(host, 0.01f, 0.42f, 0.99f, 0.99f);
            Set(side, 0.52f, 0.18f, 0.99f, 0.41f);
            Set(tray, 0.01f, 0.01f, 0.99f, 0.17f);
            Set(action, 0.01f, 0.17f, 0.51f, 0.41f);
            SetTrayHorizontal(true);
        }
        else
        {
            Set(tray, 0.008f, 0.16f, 0.20f, 0.99f);
            Set(host, 0.21f, 0.16f, 0.735f, 0.99f);
            Set(side, 0.745f, 0.16f, 0.992f, 0.99f);
            Set(action, 0.21f, 0.00f, 0.735f, 0.15f);
            SetTrayHorizontal(false);
        }
    }

    private void SetTrayHorizontal(bool horizontal)
    {
        if (trayItems == null) return;
        var vert = trayItems.GetComponent<VerticalLayoutGroup>();
        var hor = trayItems.GetComponent<HorizontalLayoutGroup>();
        if (horizontal)
        {
            if (vert != null) vert.enabled = false;
            if (hor == null) hor = trayItems.gameObject.AddComponent<HorizontalLayoutGroup>();
            hor.enabled = true;
            hor.spacing = 8;
            hor.padding = new RectOffset(6, 6, 4, 4);
            hor.childAlignment = TextAnchor.MiddleLeft;
            hor.childForceExpandWidth = false;
            hor.childForceExpandHeight = true;
            hor.childControlWidth = false;
            hor.childControlHeight = true;
            if (trayScroll != null)
            {
                trayScroll.horizontal = true;
                trayScroll.vertical = false;
            }
        }
        else
        {
            if (hor != null) hor.enabled = false;
            if (vert != null) vert.enabled = true;
            if (trayScroll != null)
            {
                trayScroll.horizontal = false;
                trayScroll.vertical = true;
            }
        }
    }

    private static void Set(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
    {
        if (rt == null) return;
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
