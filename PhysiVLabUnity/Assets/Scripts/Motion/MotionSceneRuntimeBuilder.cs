using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

[DefaultExecutionOrder(-50)]
public class MotionSceneRuntimeBuilder : MonoBehaviour
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
            Debug.LogWarning("Motion: wiring failed.");
            return;
        }
        referencesWired = true;
    }

    private void Awake()
    {
        if (GetComponent<MotionFailsafeDisplay>() == null)
            gameObject.AddComponent<MotionFailsafeDisplay>();
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
            Debug.Log("Motion practical UI built.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Motion BUILD FAILED: " + ex.Message + "\n" + ex.StackTrace);
        }
    }

    private void LoadFont()
    {
        try
        {
            defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (defaultFont == null) defaultFont = TMP_Settings.defaultFontAsset;
#if UNITY_EDITOR
            if (defaultFont == null)
                defaultFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
#endif
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Motion: font load skipped — " + ex.Message);
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
            Debug.LogWarning("Motion EventSystem: " + ex.Message);
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
        cam.backgroundColor = new Color(0.90f, 0.93f, 0.97f);
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
        AddMgr<MotionExperimentManager>("MotionExperimentManager");
        AddMgr<MotionUIManager>("MotionUIManager");
        AddMgr<MotionScoreManager>("MotionScoreManager");
        AddMgr<MotionFeedbackManager>("MotionFeedbackManager");
        AddMgr<MotionAttemptManager>("MotionAttemptManager");
        AddMgr<MotionSaveManager>("MotionSaveManager");
        AddMgr<MotionProfileManager>("MotionProfileManager");
        AddMgr<MotionDataManager>("MotionDataManager");
        AddMgr<MotionEquipmentSelectionManager>("MotionEquipmentSelectionManager");
        AddMgr<MotionTrackController>("MotionTrackController");
        AddMgr<MotionPositionController>("MotionPositionController");
        AddMgr<ToyCarController>("ToyCarController");
        AddMgr<StopwatchController>("StopwatchController");
        AddMgr<DistanceCalculator>("DistanceCalculator");
        AddMgr<DisplacementCalculator>("DisplacementCalculator");
        AddMgr<SpeedCalculator>("SpeedCalculator");
        AddMgr<VelocityCalculator>("VelocityCalculator");
        AddMgr<AccelerationCalculator>("AccelerationCalculator");
        AddMgr<DecelerationController>("DecelerationController");
        AddMgr<MotionMeasurementManager>("MotionMeasurementManager");
        AddMgr<MotionTrialManager>("MotionTrialManager");
        AddMgr<AccelerationExperimentManager>("AccelerationExperimentManager");
        AddMgr<MotionGraphController>("MotionGraphController");
        AddMgr<MotionObservationTableManager>("MotionObservationTableManager");
        AddMgr<MotionQuestionManager>("MotionQuestionManager");
        AddMgr<MotionResultManager>("MotionResultManager");
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
        Color header = new Color(0.10f, 0.22f, 0.40f);
        Color accent = new Color(0.15f, 0.48f, 0.78f);
        Color green = new Color(0.12f, 0.62f, 0.35f);
        Color amber = new Color(0.85f, 0.55f, 0.12f);

        var canvasObj = new GameObject("Canvas");
        canvasObj.layer = 5;
        canvasObj.transform.SetParent(transform, false);
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvas.pixelPerfect = false;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.65f;
        canvasObj.AddComponent<GraphicRaycaster>();

        Panel(canvasObj.transform, "ScreenBg", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.93f, 0.96f, 0.99f));

        var headerP = Panel(canvasObj.transform, "Header", new Vector2(0, 0.91f), Vector2.one, Vector2.zero, Vector2.zero, header);
        var title = Text("Title", headerP.transform, "MOTION    Experiment: Motion Investigation", 28, TextAlignmentOptions.MidlineLeft, new Vector2(16, -6), new Vector2(1200, 44));
        title.color = Color.white;
        title.enableAutoSizing = true;
        title.fontSizeMin = 14;
        title.fontSizeMax = 28;
        title.overflowMode = TextOverflowModes.Ellipsis;
        var score = Text("Score", headerP.transform, "SCORE 0/100", 24, TextAlignmentOptions.MidlineRight, new Vector2(-16, -6), new Vector2(340, 44), Vector2.one, Vector2.one);
        score.color = Color.white;
        score.enableAutoSizing = true;
        score.fontSizeMin = 12;
        score.fontSizeMax = 24;
        var progress = Text("Progress", headerP.transform, "Step: 1 / 20", 20, TextAlignmentOptions.MidlineLeft, new Vector2(16, -46), new Vector2(220, 32));
        progress.color = new Color(0.85f, 0.93f, 1f);
        var stepLabel = Text("StepLabel", headerP.transform, "Introduction", 20, TextAlignmentOptions.Center, new Vector2(0, -46), new Vector2(280, 32), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        stepLabel.color = new Color(0.85f, 0.93f, 1f);
        stepLabel.enableAutoSizing = true;
        stepLabel.fontSizeMin = 12;
        stepLabel.fontSizeMax = 20;
        stepLabel.overflowMode = TextOverflowModes.Ellipsis;
        var attempts = Text("Attempts", headerP.transform, "Attempts Remaining: 3", 20, TextAlignmentOptions.MidlineRight, new Vector2(-16, -46), new Vector2(340, 32), Vector2.one, Vector2.one);
        attempts.color = new Color(0.85f, 0.93f, 1f);
        attempts.enableAutoSizing = true;
        attempts.fontSizeMin = 12;
        attempts.fontSizeMax = 20;
        var progressBarBg = Panel(headerP.transform, "ProgressBarBg", new Vector2(0.30f, 0.06f), new Vector2(0.70f, 0.18f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.16f, 0.24f, 0.5f));
        var progressBarFill = Panel(progressBarBg.transform, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.32f, 0.85f, 0.48f));
        var fillImg = progressBarFill.GetComponent<Image>();
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 0.05f;

        var instructionBar = Panel(canvasObj.transform, "InstructionBar", new Vector2(0, 0.83f), new Vector2(1, 0.91f), Vector2.zero, Vector2.zero, new Color(1f, 0.96f, 0.78f));
        instructionBar.SetActive(false);
        var instruction = StretchText("Instruction", instructionBar.transform, "Follow the instructions.", 24, TextAlignmentOptions.MidlineLeft, new Color(0.12f, 0.16f, 0.22f), 14);
        instruction.enableAutoSizing = true;
        instruction.fontSizeMin = 14;
        instruction.fontSizeMax = 26;
        instruction.overflowMode = TextOverflowModes.Ellipsis;

        var bottom = Panel(canvasObj.transform, "BottomBar", Vector2.zero, new Vector2(1, 0.11f), Vector2.zero, Vector2.zero, new Color(0.16f, 0.22f, 0.28f));
        var bottomLayout = bottom.AddComponent<HorizontalLayoutGroup>();
        bottomLayout.padding = new RectOffset(16, 16, 10, 10);
        bottomLayout.spacing = 12;
        bottomLayout.childAlignment = TextAnchor.MiddleCenter;
        bottomLayout.childForceExpandWidth = false;
        bottomLayout.childForceExpandHeight = false;
        bottomLayout.childControlWidth = false;
        bottomLayout.childControlHeight = false;
        var nextBtn = Btn("Next", bottom.transform, "NEXT STEP", accent, 240, 64);
        var resetBtn = Btn("Reset", bottom.transform, "Reset", new Color(0.45f, 0.48f, 0.52f), 150, 64);
        var retryBtn = Btn("Retry", bottom.transform, "RETRY", accent, 150, 64);
        retryBtn.gameObject.SetActive(false);

        var main = Panel(canvasObj.transform, "MainArea", new Vector2(0, 0.11f), new Vector2(1, 0.83f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);

        var introPanel = Panel(main.transform, "IntroPanel", Vector2.zero, Vector2.one, new Vector2(36, 24), new Vector2(-36, -24), Color.white);
        var introText = StretchText("IntroText", introPanel.transform, "", 32, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 28);
        var startBtn = BigBtn("StartBtn", introPanel.transform, "START PRACTICAL", new Vector2(0.5f, 0.08f), new Vector2(0, 0), new Vector2(480, 96), green);
        introPanel.AddComponent<MotionIntroClickToStart>();

        var objectivePanel = Panel(main.transform, "ObjectivePanel", Vector2.zero, Vector2.one, new Vector2(36, 24), new Vector2(-36, -24), Color.white);
        objectivePanel.SetActive(false);
        var objectiveText = StretchText("ObjectiveText", objectivePanel.transform, "", 32, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 28);

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
        equipHint.color = new Color(0.22f, 0.32f, 0.46f);
        equipHint.enableAutoSizing = true;
        equipHint.fontSizeMin = 12;
        equipHint.fontSizeMax = 20;
        equipHint.overflowMode = TextOverflowModes.Ellipsis;

        var requiredAreaHost = Panel(equipPanel.transform, "RequiredArea", new Vector2(0, 0), new Vector2(1, 0.24f), new Vector2(8, 6), new Vector2(-8, -4), new Color(0.86f, 0.94f, 1f));
        var reqLabel = StretchText("ReqLabel", requiredAreaHost.transform, "REQUIRED EQUIPMENT", 16, TextAlignmentOptions.MidlineLeft, new Color(0.16f, 0.28f, 0.42f), 6);
        var reqLabelRt = reqLabel.rectTransform;
        reqLabelRt.anchorMin = new Vector2(0f, 0.72f);
        reqLabelRt.anchorMax = Vector2.one;
        reqLabelRt.offsetMin = new Vector2(8, 0);
        reqLabelRt.offsetMax = new Vector2(-8, -2);
        var requiredCards = Panel(requiredAreaHost.transform, "RequiredCards", new Vector2(0, 0), new Vector2(1, 0.72f), new Vector2(6, 4), new Vector2(-6, -2), new Color(0, 0, 0, 0), false);
        var reqLayout = requiredCards.AddComponent<HorizontalLayoutGroup>();
        reqLayout.spacing = 6;
        reqLayout.padding = new RectOffset(4, 4, 2, 2);
        reqLayout.childAlignment = TextAnchor.MiddleCenter;
        reqLayout.childControlWidth = true;
        reqLayout.childControlHeight = true;
        reqLayout.childForceExpandWidth = true;
        reqLayout.childForceExpandHeight = true;
        requiredCards.AddComponent<MotionUIDropTarget>().Configure("RequiredEquipment", "Any", Vector2.zero);

        var scroll = CreateScrollAnchored(equipPanel.transform, new Vector2(0f, 0.25f), new Vector2(1f, 0.85f));
        var scrollRt = scroll.GetComponent<RectTransform>();
        scrollRt.offsetMin = new Vector2(8, 4);
        scrollRt.offsetMax = new Vector2(-8, -4);
        var cardPrefab = CreateCardPrefab();
        var equipContinue = BigBtn("EquipContinueBtn", equipPanel.transform, "NEXT STEP", new Vector2(1f, 0f), new Vector2(-160, 18), new Vector2(240, 48), green);
        equipContinue.gameObject.SetActive(false);

        var labPanel = BuildLaboratory(main.transform, accent, green, amber);
        var dataTablePanel = MakeTextPanel(main.transform, "DataTablePanel", "TableTitle", "OBSERVATION TABLE", "DataTableText");
        var comparePanel = BuildComparePanel(main.transform, green);
        var graphPanel = BuildGraphPanel(main.transform, accent);
        var conclusionPanel = MakeTextPanel(main.transform, "ConclusionPanel", "ConcTitle", "CONCLUSION", "ConclusionText");
        var questionBits = BuildQuestionPanel(main.transform, header);
        var resultBits = BuildResultPanel(main.transform, accent);

        var feedbackPanel = Panel(canvasObj.transform, "FeedbackPanel", new Vector2(0.18f, 0.38f), new Vector2(0.82f, 0.64f), Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.97f));
        feedbackPanel.SetActive(false);
        var feedbackGroup = feedbackPanel.AddComponent<CanvasGroup>();
        var feedbackText = StretchText("FeedbackText", feedbackPanel.transform, "", 22, TextAlignmentOptions.Center, new Color(0.14f, 0.32f, 0.6f), 14);
        var scoreChangeText = Text("ScoreChange", feedbackPanel.transform, "", 20, TextAlignmentOptions.Bottom, new Vector2(0, 8), new Vector2(220, 28), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));

        var resetConfirm = Panel(canvasObj.transform, "ResetConfirm", new Vector2(0.28f, 0.38f), new Vector2(0.72f, 0.62f), Vector2.zero, Vector2.zero, Color.white);
        resetConfirm.SetActive(false);
        StretchText("ResetMsg", resetConfirm.transform, "Are you sure you want to restart the practical?", 24, TextAlignmentOptions.Center, new Color(0.18f, 0.2f, 0.25f), 16);
        var resetRow = Panel(resetConfirm.transform, "Btns", new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.38f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var resetRowLayout = resetRow.AddComponent<HorizontalLayoutGroup>();
        resetRowLayout.spacing = 18;
        resetRowLayout.childForceExpandWidth = true;
        var resetYes = Btn("Yes", resetRow.transform, "YES", new Color(0.75f, 0.2f, 0.2f), 0, 50);
        var resetNo = Btn("No", resetRow.transform, "NO", accent, 0, 50);

        var refs = canvasObj.AddComponent<MotionUIRefs>();
        refs.UiVersion = 6;
        refs.Title = title; refs.Score = score; refs.Progress = progress; refs.Attempts = attempts;
        refs.StepLabel = stepLabel; refs.ProgressBar = fillImg; refs.Instruction = instruction;
        refs.IntroPanel = introPanel; refs.IntroText = introText; refs.StartBtn = startBtn;
        refs.ObjectivePanel = objectivePanel; refs.ObjectiveText = objectiveText;
        refs.InstructionBar = instructionBar; refs.EquipmentPanel = equipPanel;
        refs.LaboratoryPanel = labPanel.panel;
        refs.DataTablePanel = dataTablePanel;
        refs.DataTableText = dataTablePanel.transform.Find("DataTableText")?.GetComponent<TextMeshProUGUI>();
        refs.ComparePanel = comparePanel.panel;
        refs.CompareText = comparePanel.text;
        refs.CompareDistanceInput = comparePanel.distanceInput;
        refs.CompareDisplacementInput = comparePanel.dispInput;
        refs.CheckCompareBtn = comparePanel.checkBtn;
        refs.GraphPanel = graphPanel.panel;
        refs.DistanceGraphArea = graphPanel.dArea;
        refs.VelocityGraphArea = graphPanel.vArea;
        refs.AccelerationGraphArea = graphPanel.aArea;
        refs.DotPrefab = graphPanel.dot;
        refs.QuestionPanel = questionBits.panel;
        refs.QuestionText = questionBits.questionText;
        refs.QuestionA = questionBits.a; refs.QuestionB = questionBits.b; refs.QuestionC = questionBits.c; refs.QuestionD = questionBits.d;
        refs.QuestionContinue = questionBits.cont;
        refs.QuestionExplanationPanel = questionBits.explain;
        refs.QuestionExplanationText = questionBits.explainText;
        refs.OptAText = questionBits.optA; refs.OptBText = questionBits.optB; refs.OptCText = questionBits.optC; refs.OptDText = questionBits.optD;
        refs.NumericGroup = questionBits.numericGroup;
        refs.OptionsGroup = questionBits.optionsGroup;
        refs.NumericInput = questionBits.numericInput;
        refs.NumericSubmit = questionBits.numericSubmit;
        refs.ConclusionPanel = conclusionPanel;
        refs.ConclusionText = conclusionPanel.transform.Find("ConclusionText")?.GetComponent<TextMeshProUGUI>();
        refs.ResultPanel = resultBits.panel; refs.FinalScore = resultBits.score; refs.ResultDetails = resultBits.details; refs.StatusText = resultBits.status;
        refs.ViewProfileBtn = resultBits.profile; refs.ViewResultsBtn = resultBits.results;
        refs.Next = nextBtn; refs.Reset = resetBtn; refs.Retry = retryBtn;
        refs.ResetConfirm = resetConfirm; refs.ResetYes = resetYes; refs.ResetNo = resetNo;
        refs.FeedbackPanel = feedbackPanel; refs.FeedbackText = feedbackText; refs.ScoreChangeText = scoreChangeText;
        refs.FeedbackGroup = feedbackGroup;
        refs.CardContainer = scroll.content; refs.RequiredArea = requiredCards.transform; refs.CardPrefab = cardPrefab;
        refs.LiveReadings = labPanel.live; refs.StopwatchText = labPanel.stopwatch;
        refs.StartExpBtn = labPanel.startExp; refs.StopExpBtn = labPanel.stopExp;
        refs.ResetRunBtn = labPanel.resetRun; refs.RecordBtn = labPanel.record; refs.DirectionBtn = labPanel.direction;
        refs.Target1 = labPanel.t1; refs.Target2 = labPanel.t2; refs.Target3 = labPanel.t3; refs.Target4 = labPanel.t4; refs.Target5 = labPanel.t5;
        refs.LowSpeedBtn = labPanel.low; refs.MedSpeedBtn = labPanel.med; refs.HighSpeedBtn = labPanel.high;
        refs.CalcPanel = labPanel.calcPanel; refs.CalcPrompt = labPanel.calcPrompt; refs.CalcInput = labPanel.calcInput; refs.CheckCalcBtn = labPanel.checkCalc;
        refs.AccelPanel = labPanel.accelPanel;
        refs.ActionRow = labPanel.actionRow;
        refs.TargetRow = labPanel.targetRow;
        refs.TrackArea = labPanel.track; refs.EquipmentTray = labPanel.tray; refs.CarRect = labPanel.car;

        instructionBar.transform.SetSiblingIndex(headerP.transform.GetSiblingIndex() + 1);
        canvasObj.transform.Find("BottomBar")?.SetAsLastSibling();
        feedbackPanel.transform.SetAsLastSibling();
        resetConfirm.transform.SetAsLastSibling();
    }

    private bool WireReferences(bool showWelcome)
    {
        var refs = Object.FindAnyObjectByType<MotionUIRefs>();
        if (refs == null || MotionUIManager.Instance == null) return false;
        MotionFeedbackManager.Instance?.BindUI(refs.FeedbackPanel, refs.FeedbackText, refs.ScoreChangeText, refs.FeedbackGroup);
        MotionUIManager.Instance.BindAll(refs, showWelcome);
        MotionEquipmentSelectionManager.Instance?.SetupUI(refs.CardContainer, refs.RequiredArea, refs.CardPrefab);
        MotionObservationTableManager.Instance?.Bind(refs.DataTableText);
        MotionGraphController.Instance?.Bind(refs.DistanceGraphArea, refs.VelocityGraphArea, refs.AccelerationGraphArea, refs.DotPrefab);
        MotionResultManager.Instance?.Bind(refs.FinalScore, refs.ResultDetails, refs.StatusText);
        MotionTrackController.Instance?.Bind(refs.TrackArea, refs.EquipmentTray, refs.CarRect, defaultFont);
        MotionFailsafeDisplay.Hide();
        return refs.StartBtn != null;
    }

    private LabBits BuildLaboratory(Transform parent, Color accent, Color green, Color amber)
    {
        var labPanel = Panel(parent, "LaboratoryPanel", Vector2.zero, Vector2.one, new Vector2(8, 4), new Vector2(-8, -4), new Color(0, 0, 0, 0), false);
        labPanel.SetActive(false);

        var tray = Panel(labPanel.transform, "EquipmentTray", new Vector2(0.008f, 0.01f), new Vector2(0.20f, 0.99f), Vector2.zero, Vector2.zero, new Color(0.93f, 0.95f, 0.99f, 1f));
        var trayHeader = Panel(tray.transform, "TrayHeader", new Vector2(0f, 0.90f), Vector2.one, Vector2.zero, Vector2.zero, new Color(0.12f, 0.28f, 0.48f));
        StretchText("TrayLabel", trayHeader.transform, "EQUIPMENT\nDrag onto the track", 20, TextAlignmentOptions.Center, Color.white, 6).fontStyle = FontStyles.Bold;
        var scrollObj = Panel(tray.transform, "Scroll", new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.88f), Vector2.zero, Vector2.zero, new Color(0.93f, 0.95f, 0.99f, 0.2f));
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
        trayLayout.childControlHeight = false;
        trayLayout.childControlWidth = true;
        trayInner.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.content = trayInnerRt;
        scroll.vertical = true;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        var trackHost = Panel(labPanel.transform, "TrackHost", new Vector2(0.21f, 0.16f), new Vector2(0.735f, 0.99f), Vector2.zero, Vector2.zero, new Color(0.86f, 0.91f, 0.96f));
        var trackTitle = Text("TrackTitle", trackHost.transform, "EXPERIMENT AREA", 20, TextAlignmentOptions.MidlineLeft, new Vector2(10, -6), new Vector2(420, 28));
        trackTitle.enableAutoSizing = true;
        trackTitle.fontSizeMin = 12;
        trackTitle.fontSizeMax = 20;
        trackTitle.overflowMode = TextOverflowModes.Ellipsis;
        var arrow = Panel(trackHost.transform, "DirectionArrow", new Vector2(0.42f, 0.90f), new Vector2(0.97f, 0.985f), Vector2.zero, Vector2.zero, new Color(0.12f, 0.42f, 0.78f, 0.95f));
        StretchText("ArrowText", arrow.transform, "START  →  FINISH     Positive direction: →", 16, TextAlignmentOptions.Center, Color.white, 4).fontStyle = FontStyles.Bold;
        var track = Panel(trackHost.transform, "Track", new Vector2(0.03f, 0.22f), new Vector2(0.97f, 0.86f), Vector2.zero, Vector2.zero, new Color(0.55f, 0.60f, 0.66f, 0.9f));
        track.AddComponent<MotionUIDropTarget>().Configure("Track", "Any", Vector2.zero, 2.5f);
        var trackVisual = Panel(track.transform, "TrackVisual", new Vector2(0f, 0.28f), new Vector2(1f, 0.72f), Vector2.zero, Vector2.zero, new Color(0.32f, 0.35f, 0.40f));
        trackVisual.SetActive(false);
        var lane = Panel(trackVisual.transform, "Lane", new Vector2(0.02f, 0.42f), new Vector2(0.98f, 0.58f), Vector2.zero, Vector2.zero, new Color(0.95f, 0.86f, 0.22f));
        lane.GetComponent<Image>().raycastTarget = false;
        var rulerVisual = Panel(track.transform, "RulerVisual", new Vector2(0f, 0f), new Vector2(1f, 0.22f), Vector2.zero, Vector2.zero, new Color(0.95f, 0.85f, 0.45f));
        rulerVisual.SetActive(false);
        rulerVisual.AddComponent<MotionUIDropTarget>().Configure("Ruler", "Ruler", new Vector2(0, -20));
        var startZone = Panel(track.transform, "StartZone", new Vector2(0f, 0.18f), new Vector2(0.10f, 1f), Vector2.zero, Vector2.zero, new Color(0.2f, 0.72f, 0.35f, 0.40f));
        startZone.AddComponent<MotionUIDropTarget>().Configure("Start", "Any", Vector2.zero, 0f);
        for (int m = 1; m <= 5; m++)
        {
            float a = Mathf.Max(0f, m / 5f - 0.07f);
            float b = Mathf.Min(1f, m / 5f + 0.07f);
            var zone = Panel(track.transform, "MarkerZone" + m, new Vector2(a, 0.18f), new Vector2(b, 1f), Vector2.zero, Vector2.zero, new Color(0.2f, 0.45f, 0.85f, 0.18f));
            zone.AddComponent<MotionUIDropTarget>().Configure("Marker", "Marker", Vector2.zero, m);
            Text("L" + m, trackHost.transform, m + " m", 22, TextAlignmentOptions.Center,
                Vector2.zero, new Vector2(70, 28),
                new Vector2(0.03f + m / 5f * 0.94f, 0.08f), new Vector2(0.03f + m / 5f * 0.94f, 0.08f), new Vector2(0.5f, 0.5f));
        }
        Text("L0", trackHost.transform, "0 m", 22, TextAlignmentOptions.Center, Vector2.zero, new Vector2(70, 28), new Vector2(0.03f, 0.08f), new Vector2(0.03f, 0.08f), new Vector2(0.5f, 0.5f));
        var car = Panel(track.transform, "ToyCar", new Vector2(0f, 0.52f), new Vector2(0f, 0.52f), Vector2.zero, new Vector2(110, 66), Color.white);
        car.GetComponent<Image>().sprite = MotionIconFactory.GetNamed("car");
        car.GetComponent<Image>().preserveAspect = true;
        car.GetComponent<Image>().raycastTarget = true;
        car.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        car.AddComponent<MotionConfirmStartClick>();
        car.SetActive(true);

        var side = Panel(labPanel.transform, "SidePanel", new Vector2(0.745f, 0.16f), new Vector2(0.992f, 0.99f), Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.98f));
        var swDrop = Panel(side.transform, "StopwatchDrop", new Vector2(0.06f, 0.80f), new Vector2(0.94f, 0.97f), Vector2.zero, Vector2.zero, new Color(0.88f, 0.93f, 1f));
        swDrop.AddComponent<MotionUIDropTarget>().Configure("Stopwatch", "Stopwatch", Vector2.zero);
        var swText = StretchText("StopwatchText", swDrop.transform, "00.00 s", 32, TextAlignmentOptions.Center, new Color(0.10f, 0.14f, 0.22f), 6);
        swText.fontStyle = FontStyles.Bold;
        swText.enableAutoSizing = true;
        swText.fontSizeMin = 16;
        swText.fontSizeMax = 32;
        var live = StretchText("LiveReadings", side.transform, "TIME: 0.00 s", 18, TextAlignmentOptions.TopLeft, new Color(0.12f, 0.16f, 0.22f), 10);
        live.fontStyle = FontStyles.Bold;
        live.enableAutoSizing = true;
        live.fontSizeMin = 12;
        live.fontSizeMax = 20;
        var liveRt = live.rectTransform;
        liveRt.anchorMin = new Vector2(0.06f, 0.03f);
        liveRt.anchorMax = new Vector2(0.94f, 0.76f);

        var actionRow = Panel(labPanel.transform, "ActionRow", new Vector2(0.21f, 0.075f), new Vector2(0.992f, 0.15f), Vector2.zero, Vector2.zero, new Color(0.16f, 0.22f, 0.28f));
        actionRow.SetActive(false);
        var aLayout = actionRow.AddComponent<GridLayoutGroup>();
        aLayout.cellSize = new Vector2(140, 36);
        aLayout.spacing = new Vector2(8, 6);
        aLayout.padding = new RectOffset(8, 8, 6, 6);
        aLayout.constraint = GridLayoutGroup.Constraint.Flexible;
        aLayout.childAlignment = TextAnchor.MiddleCenter;
        var startExp = Btn("StartExp", actionRow.transform, "START", green, 0, 36);
        var stopExp = Btn("StopExp", actionRow.transform, "STOP", new Color(0.75f, 0.25f, 0.2f), 0, 36);
        var record = Btn("Record", actionRow.transform, "RECORD", accent, 0, 36);
        var direction = Btn("Direction", actionRow.transform, "CONFIRM →", amber, 0, 36);

        var targetRow = Panel(labPanel.transform, "TargetRow", new Vector2(0.21f, 0.01f), new Vector2(0.73f, 0.07f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        targetRow.SetActive(false);
        var tLayout = targetRow.AddComponent<HorizontalLayoutGroup>();
        tLayout.spacing = 8;
        tLayout.childForceExpandWidth = true;
        tLayout.childControlWidth = true;
        var t1 = Btn("T1", targetRow.transform, "1 m", accent, 0, 40);
        var t2 = Btn("T2", targetRow.transform, "2 m", accent, 0, 40);
        var t3 = Btn("T3", targetRow.transform, "3 m", accent, 0, 40);
        var t4 = Btn("T4", targetRow.transform, "4 m", accent, 0, 40);
        var t5 = Btn("T5", targetRow.transform, "5 m", accent, 0, 40);

        var accelPanel = Panel(labPanel.transform, "AccelPanel", new Vector2(0.74f, 0.01f), new Vector2(0.992f, 0.07f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        accelPanel.SetActive(false);
        var acLayout = accelPanel.AddComponent<HorizontalLayoutGroup>();
        acLayout.spacing = 6;
        acLayout.childForceExpandWidth = true;
        var low = Btn("Low", accelPanel.transform, "LOW", new Color(0.3f, 0.55f, 0.75f), 0, 40);
        var med = Btn("Med", accelPanel.transform, "MEDIUM", amber, 0, 40);
        var high = Btn("High", accelPanel.transform, "HIGH", new Color(0.75f, 0.3f, 0.25f), 0, 40);

        var calcPanel = Panel(labPanel.transform, "CalcPanel", new Vector2(0.21f, 0.01f), new Vector2(0.992f, 0.15f), Vector2.zero, Vector2.zero, new Color(0.97f, 0.98f, 1f));
        calcPanel.SetActive(false);
        var calcPrompt = Text("CalcPrompt", calcPanel.transform, "Enter your calculated value", 20, TextAlignmentOptions.MidlineLeft, new Vector2(12, 8), new Vector2(520, 40), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        var calcInput = CreateInput(calcPanel.transform, "CalcInput", new Vector2(0.48f, 0.18f), new Vector2(0.72f, 0.82f), "value");
        var checkCalc = Btn("CheckCalc", calcPanel.transform, "CHECK", green, 160, 44);
        var checkRt = checkCalc.GetComponent<RectTransform>();
        checkRt.anchorMin = checkRt.anchorMax = new Vector2(0.88f, 0.5f);
        checkRt.pivot = new Vector2(0.5f, 0.5f);
        checkRt.sizeDelta = new Vector2(150, 44);
        Object.Destroy(checkCalc.GetComponent<LayoutElement>());

        labPanel.AddComponent<MotionAdaptiveLab>().Bind(
            tray.GetComponent<RectTransform>(),
            scroll,
            trayInnerRt,
            trackHost.GetComponent<RectTransform>(),
            side.GetComponent<RectTransform>(),
            actionRow.GetComponent<RectTransform>(),
            targetRow.GetComponent<RectTransform>(),
            accelPanel.GetComponent<RectTransform>(),
            calcPanel.GetComponent<RectTransform>());

        return new LabBits
        {
            panel = labPanel, live = live, stopwatch = swText,
            startExp = startExp, stopExp = stopExp, resetRun = null, record = record, direction = direction,
            t1 = t1, t2 = t2, t3 = t3, t4 = t4, t5 = t5,
            low = low, med = med, high = high,
            calcPanel = calcPanel, calcPrompt = calcPrompt, calcInput = calcInput, checkCalc = checkCalc,
            accelPanel = accelPanel, actionRow = actionRow, targetRow = targetRow,
            track = track.GetComponent<RectTransform>(),
            tray = trayInnerRt, car = car.GetComponent<RectTransform>()
        };
    }

    private CompareBits BuildComparePanel(Transform parent, Color green)
    {
        var panel = Panel(parent, "ComparePanel", Vector2.zero, Vector2.one, new Vector2(24, 16), new Vector2(-24, -16), new Color(0.97f, 0.98f, 1f));
        panel.SetActive(false);
        var text = StretchText("CompareText", panel.transform, "", 26, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 22);
        var tr = text.rectTransform;
        tr.anchorMin = new Vector2(0.04f, 0.38f);
        tr.anchorMax = new Vector2(0.96f, 0.96f);
        var dIn = CreateInput(panel.transform, "DistanceInput", new Vector2(0.08f, 0.22f), new Vector2(0.45f, 0.34f), "Distance (m)");
        var sIn = CreateInput(panel.transform, "DisplacementInput", new Vector2(0.55f, 0.22f), new Vector2(0.92f, 0.34f), "Displacement (m)");
        var check = BigBtn("CheckCompare", panel.transform, "CHECK ANSWERS", new Vector2(0.5f, 0.10f), Vector2.zero, new Vector2(360, 64), green);
        return new CompareBits { panel = panel, text = text, distanceInput = dIn, dispInput = sIn, checkBtn = check };
    }

    private GraphBits BuildGraphPanel(Transform parent, Color accent)
    {
        var graphPanel = Panel(parent, "GraphPanel", Vector2.zero, Vector2.one, new Vector2(20, 12), new Vector2(-20, -12), new Color(0.97f, 0.98f, 1f));
        graphPanel.SetActive(false);
        Text("GraphTitle", graphPanel.transform, "GRAPHS  —  generated from your experiment data", 24, TextAlignmentOptions.MidlineLeft, new Vector2(20, -10), new Vector2(900, 36));
        var dArea = Panel(graphPanel.transform, "DistanceGraph", new Vector2(0.03f, 0.68f), new Vector2(0.97f, 0.92f), Vector2.zero, Vector2.zero, new Color(0.92f, 0.95f, 1f));
        Text("DLabel", dArea.transform, "Time (s) →     Distance (m) ↑", 14, TextAlignmentOptions.Bottom, new Vector2(0, 4), new Vector2(700, 20), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        var vArea = Panel(graphPanel.transform, "VelocityGraph", new Vector2(0.03f, 0.38f), new Vector2(0.97f, 0.66f), Vector2.zero, Vector2.zero, new Color(1f, 0.95f, 0.90f));
        Text("VLabel", vArea.transform, "Time (s) →     Velocity (m/s) ↑", 14, TextAlignmentOptions.Bottom, new Vector2(0, 4), new Vector2(700, 20), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        var aArea = Panel(graphPanel.transform, "AccelerationGraph", new Vector2(0.03f, 0.06f), new Vector2(0.97f, 0.36f), Vector2.zero, Vector2.zero, new Color(0.95f, 0.92f, 1f));
        Text("ALabel", aArea.transform, "Time (s) →     Acceleration (m/s²) ↑", 14, TextAlignmentOptions.Bottom, new Vector2(0, 4), new Vector2(700, 20), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        var dotPrefab = Panel(dArea.transform, "DotPrefab", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-6, -6), new Vector2(6, 6), accent);
        dotPrefab.SetActive(false);
        return new GraphBits
        {
            panel = graphPanel,
            dArea = dArea.GetComponent<RectTransform>(),
            vArea = vArea.GetComponent<RectTransform>(),
            aArea = aArea.GetComponent<RectTransform>(),
            dot = dotPrefab
        };
    }

    private QuestionBits BuildQuestionPanel(Transform parent, Color header)
    {
        var questionPanel = Panel(parent, "QuestionPanel", Vector2.zero, Vector2.one, new Vector2(18, 10), new Vector2(-18, -10), new Color(0.96f, 0.98f, 1f));
        questionPanel.SetActive(false);
        var qHeader = Panel(questionPanel.transform, "Header", new Vector2(0f, 0.78f), new Vector2(1f, 1f), new Vector2(12, 8), new Vector2(-12, -8), header);
        var questionText = StretchText("Question", qHeader.transform, "Question", 24, TextAlignmentOptions.MidlineLeft, Color.white, 14);
        var qOptions = Panel(questionPanel.transform, "Options", new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.76f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var vLayout = qOptions.AddComponent<VerticalLayoutGroup>();
        vLayout.spacing = 10; vLayout.padding = new RectOffset(8, 8, 8, 8);
        vLayout.childControlHeight = true; vLayout.childForceExpandHeight = true;
        vLayout.childControlWidth = true; vLayout.childForceExpandWidth = true;
        var optA = ChoiceBtn("OptA", qOptions.transform, "A", "");
        var optB = ChoiceBtn("OptB", qOptions.transform, "B", "");
        var optC = ChoiceBtn("OptC", qOptions.transform, "C", "");
        var optD = ChoiceBtn("OptD", qOptions.transform, "D", "");
        var numericGroup = Panel(questionPanel.transform, "NumericGroup", new Vector2(0.2f, 0.22f), new Vector2(0.8f, 0.55f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        numericGroup.SetActive(false);
        var numLayout = numericGroup.AddComponent<VerticalLayoutGroup>();
        numLayout.spacing = 10;
        numLayout.childAlignment = TextAnchor.MiddleCenter;
        numLayout.childForceExpandHeight = false;
        numLayout.childForceExpandWidth = true;
        var numericInput = CreateInput(numericGroup.transform, "NumericInput", new Vector2(0f, 0.55f), new Vector2(1f, 1f), "Enter number");
        var numericSubmit = Btn("NumericSubmit", numericGroup.transform, "SUBMIT", new Color(0.12f, 0.62f, 0.35f), 220, 52);
        var explanationPanel = Panel(questionPanel.transform, "ExplanationPanel", new Vector2(0.08f, 0.10f), new Vector2(0.92f, 0.48f), Vector2.zero, Vector2.zero, new Color(0.12f, 0.46f, 0.32f));
        explanationPanel.SetActive(false);
        var explanationText = StretchText("ExplanationText", explanationPanel.transform, "", 22, TextAlignmentOptions.Top, Color.white, 16);
        var continueObj = Panel(explanationPanel.transform, "ContinueBtn", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-120, 12), new Vector2(120, 58), Color.white);
        var continueBtn = continueObj.AddComponent<Button>();
        StretchText("Text", continueObj.transform, "Continue ▶", 22, TextAlignmentOptions.Center, new Color(0.1f, 0.4f, 0.25f), 4).fontStyle = FontStyles.Bold;
        return new QuestionBits
        {
            panel = questionPanel, questionText = questionText,
            a = optA, b = optB, c = optC, d = optD, cont = continueBtn,
            explain = explanationPanel, explainText = explanationText,
            optA = optA.transform.Find("Body")?.GetComponent<TextMeshProUGUI>(),
            optB = optB.transform.Find("Body")?.GetComponent<TextMeshProUGUI>(),
            optC = optC.transform.Find("Body")?.GetComponent<TextMeshProUGUI>(),
            optD = optD.transform.Find("Body")?.GetComponent<TextMeshProUGUI>(),
            numericGroup = numericGroup, optionsGroup = qOptions,
            numericInput = numericInput, numericSubmit = numericSubmit
        };
    }

    private ResultBits BuildResultPanel(Transform parent, Color accent)
    {
        var resultPanel = Panel(parent, "ResultPanel", Vector2.zero, Vector2.one, new Vector2(28, 16), new Vector2(-28, -16), Color.white);
        resultPanel.SetActive(false);
        var finalScore = StretchText("FinalScore", resultPanel.transform, "", 22, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 16);
        var fsRt = finalScore.rectTransform;
        fsRt.anchorMin = new Vector2(0f, 0.62f);
        fsRt.anchorMax = Vector2.one;
        var resultDetails = StretchText("ResultDetails", resultPanel.transform, "", 18, TextAlignmentOptions.TopLeft, new Color(0.18f, 0.22f, 0.28f), 16);
        var rdRt = resultDetails.rectTransform;
        rdRt.anchorMin = new Vector2(0f, 0.16f);
        rdRt.anchorMax = new Vector2(1f, 0.62f);
        var statusText = Text("Status", resultPanel.transform, "STATUS: COMPLETED", 26, TextAlignmentOptions.MidlineLeft, new Vector2(20, 70), new Vector2(520, 40));
        var resultBtnRow = Panel(resultPanel.transform, "ResultBtns", new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.14f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var resultBtnLayout = resultBtnRow.AddComponent<HorizontalLayoutGroup>();
        resultBtnLayout.spacing = 12;
        resultBtnLayout.childForceExpandWidth = true;
        var viewResultsBtn = Btn("ViewResults", resultBtnRow.transform, "VIEW RESULTS", accent, 0, 52);
        var viewProfileBtn = Btn("ViewProfile", resultBtnRow.transform, "VIEW PROFILE", accent, 0, 52);
        Btn("BackPracticals", resultBtnRow.transform, "BACK TO PRACTICALS", new Color(0.4f, 0.45f, 0.5f), 0, 52);
        return new ResultBits { panel = resultPanel, score = finalScore, details = resultDetails, status = statusText, results = viewResultsBtn, profile = viewProfileBtn };
    }

    private GameObject MakeTextPanel(Transform parent, string name, string titleName, string title, string bodyName)
    {
        var panel = Panel(parent, name, Vector2.zero, Vector2.one, new Vector2(24, 16), new Vector2(-24, -16), new Color(0.97f, 0.98f, 1f));
        panel.SetActive(false);
        Text(titleName, panel.transform, title, 34, TextAlignmentOptions.MidlineLeft, new Vector2(20, -12), new Vector2(1100, 48));
        StretchText(bodyName, panel.transform, "", 22, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 22);
        var body = panel.transform.Find(bodyName) as RectTransform;
        if (body != null)
        {
            body.anchorMin = new Vector2(0.04f, 0.08f);
            body.anchorMax = new Vector2(0.96f, 0.86f);
        }
        return panel;
    }

    private TMP_InputField CreateInput(Transform parent, string name, Vector2 aMin, Vector2 aMax, string placeholder)
    {
        var obj = Panel(parent, name, aMin, aMax, Vector2.zero, Vector2.zero, new Color(0.93f, 0.96f, 1f));
        var input = obj.AddComponent<TMP_InputField>();
        var text = StretchText("Text", obj.transform, "", 22, TextAlignmentOptions.MidlineLeft, new Color(0.1f, 0.15f, 0.2f), 8);
        text.raycastTarget = true;
        var ph = StretchText("Placeholder", obj.transform, placeholder, 20, TextAlignmentOptions.MidlineLeft, new Color(0.45f, 0.5f, 0.55f), 8);
        ph.raycastTarget = false;
        input.textComponent = text;
        input.placeholder = ph;
        input.contentType = TMP_InputField.ContentType.Standard;
        input.textViewport = obj.GetComponent<RectTransform>();
        return input;
    }

    private GameObject Panel(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax, Color color, bool raycast = true)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = offMin; rt.offsetMax = offMax;
        var img = obj.AddComponent<Image>();
        img.sprite = MotionIconFactory.White();
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
        tmp.color = new Color(0.12f, 0.15f, 0.2f); tmp.raycastTarget = false;
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
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        tmp.raycastTarget = false;
        return tmp;
    }

    private Button ChoiceBtn(string name, Transform parent, string letter, string text)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var le = obj.AddComponent<LayoutElement>();
        le.preferredHeight = 70; le.minHeight = 60;
        obj.AddComponent<Image>().color = Color.white;
        var btn = obj.AddComponent<Button>();
        var letterBg = Panel(obj.transform, "Letter", new Vector2(0f, 0.12f), new Vector2(0f, 0.88f), new Vector2(10, 0), new Vector2(68, 0), new Color(0.15f, 0.45f, 0.75f), false);
        StretchText("LetterText", letterBg.transform, letter, 26, TextAlignmentOptions.Center, Color.white, 0).fontStyle = FontStyles.Bold;
        var bodyObj = new GameObject("Body");
        bodyObj.transform.SetParent(obj.transform, false);
        var bodyRt = bodyObj.AddComponent<RectTransform>();
        bodyRt.anchorMin = Vector2.zero; bodyRt.anchorMax = Vector2.one;
        bodyRt.offsetMin = new Vector2(82, 8); bodyRt.offsetMax = new Vector2(-16, -8);
        var body = bodyObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) body.font = defaultFont;
        body.text = text; body.fontSize = 22; body.fontStyle = FontStyles.Bold;
        body.alignment = TextAlignmentOptions.MidlineLeft;
        body.color = new Color(0.12f, 0.16f, 0.22f);
        body.raycastTarget = false;
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
        img.sprite = MotionIconFactory.White();
        img.color = bg;
        var btn = obj.AddComponent<Button>();
        var txt = StretchText("Text", obj.transform, label, 18, TextAlignmentOptions.Center, Color.white, 2);
        txt.fontStyle = FontStyles.Bold;
        txt.enableAutoSizing = true;
        txt.fontSizeMin = 10;
        txt.fontSizeMax = 18;
        txt.overflowMode = TextOverflowModes.Ellipsis;
        return btn;
    }

    private Button BigBtn(string name, Transform parent, string label, Vector2 anchor, Vector2 pos, Vector2 size, Color bg)
    {
        var obj = Panel(parent, name, anchor, anchor, Vector2.zero, size, bg);
        obj.GetComponent<RectTransform>().anchoredPosition = pos;
        obj.GetComponent<RectTransform>().sizeDelta = size;
        var btn = obj.AddComponent<Button>();
        StretchText("Text", obj.transform, label, 28, TextAlignmentOptions.Center, Color.white, 8).fontStyle = FontStyles.Bold;
        return btn;
    }

    private ScrollRect CreateScrollAnchored(Transform parent, Vector2 aMin, Vector2 aMax)
    {
        var scrollObj = Panel(parent, "Scroll", aMin, aMax, Vector2.zero, Vector2.zero, new Color(0.96f, 0.98f, 1f));
        var scroll = scrollObj.AddComponent<ScrollRect>();
        var viewport = Panel(scrollObj.transform, "Viewport", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.01f));
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        var content = Panel(viewport.transform, "Content", new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.01f));
        var contentRt = content.GetComponent<RectTransform>();
        contentRt.pivot = new Vector2(0.5f, 1f);
        var grid = content.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(200, 156);
        grid.spacing = new Vector2(10, 10);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        grid.padding = new RectOffset(10, 10, 10, 10);
        grid.childAlignment = TextAnchor.UpperCenter;
        var fitter = content.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        content.AddComponent<MotionAdaptiveGrid>();
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
        card.AddComponent<RectTransform>().sizeDelta = new Vector2(200, 156);
        card.AddComponent<Image>().color = new Color(0.92f, 0.96f, 1f);
        card.AddComponent<MotionEquipmentCardUI>();
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
        public GameObject panel, calcPanel, accelPanel, actionRow, targetRow;
        public TextMeshProUGUI live, stopwatch, calcPrompt;
        public Button startExp, stopExp, resetRun, record, direction;
        public Button t1, t2, t3, t4, t5, low, med, high, checkCalc;
        public TMP_InputField calcInput;
        public RectTransform track, tray, car;
    }

    private struct CompareBits
    {
        public GameObject panel;
        public TextMeshProUGUI text;
        public TMP_InputField distanceInput, dispInput;
        public Button checkBtn;
    }

    private struct GraphBits
    {
        public GameObject panel, dot;
        public RectTransform dArea, vArea, aArea;
    }

    private struct QuestionBits
    {
        public GameObject panel, explain, numericGroup, optionsGroup;
        public TextMeshProUGUI questionText, explainText, optA, optB, optC, optD;
        public Button a, b, c, d, cont, numericSubmit;
        public TMP_InputField numericInput;
    }

    private struct ResultBits
    {
        public GameObject panel;
        public TextMeshProUGUI score, details, status;
        public Button results, profile;
    }
}

public class MotionUIRefs : MonoBehaviour
{
    public int UiVersion;
    public TextMeshProUGUI Title, Score, Progress, Attempts, StepLabel, Instruction;
    public TextMeshProUGUI IntroText, ObjectiveText, ConclusionText, CompareText, QuestionText, DataTableText;
    public TextMeshProUGUI FinalScore, ResultDetails, StatusText, FeedbackText, ScoreChangeText;
    public TextMeshProUGUI LiveReadings, StopwatchText, CalcPrompt;
    public TextMeshProUGUI QuestionExplanationText, OptAText, OptBText, OptCText, OptDText;
    public Image ProgressBar;
    public GameObject IntroPanel, ObjectivePanel, InstructionBar, EquipmentPanel, LaboratoryPanel;
    public GameObject DataTablePanel, ComparePanel, GraphPanel, QuestionPanel, ConclusionPanel, ResultPanel;
    public GameObject CalcPanel, AccelPanel, ActionRow, TargetRow, ResetConfirm, FeedbackPanel, CardPrefab, DotPrefab, QuestionExplanationPanel;
    public GameObject NumericGroup, OptionsGroup;
    public Transform CardContainer, RequiredArea;
    public RectTransform DistanceGraphArea, VelocityGraphArea, AccelerationGraphArea, TrackArea, EquipmentTray, CarRect;
    public Button StartBtn, Next, Reset, Retry, ResetYes, ResetNo, ViewProfileBtn, ViewResultsBtn;
    public Button StartExpBtn, StopExpBtn, ResetRunBtn, RecordBtn, DirectionBtn;
    public Button Target1, Target2, Target3, Target4, Target5;
    public Button LowSpeedBtn, MedSpeedBtn, HighSpeedBtn, CheckCalcBtn, CheckCompareBtn;
    public Button QuestionA, QuestionB, QuestionC, QuestionD, QuestionContinue, NumericSubmit;
    public TMP_InputField NumericInput, CalcInput, CompareDistanceInput, CompareDisplacementInput;
    public CanvasGroup FeedbackGroup;
}

public class MotionAdaptiveGrid : MonoBehaviour
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

public class MotionAdaptiveLab : MonoBehaviour
{
    private RectTransform tray;
    private ScrollRect trayScroll;
    private RectTransform trayItems;
    private RectTransform trackHost;
    private RectTransform side;
    private RectTransform action;
    private RectTransform target;
    private RectTransform accel;
    private RectTransform calc;
    private GridLayoutGroup actionGrid;
    private int lastW;
    private int lastH;

    public void Bind(
        RectTransform trayRt,
        ScrollRect scroll,
        RectTransform items,
        RectTransform trackRt,
        RectTransform sideRt,
        RectTransform actionRt,
        RectTransform targetRt,
        RectTransform accelRt,
        RectTransform calcRt)
    {
        tray = trayRt;
        trayScroll = scroll;
        trayItems = items;
        trackHost = trackRt;
        side = sideRt;
        action = actionRt;
        target = targetRt;
        accel = accelRt;
        calc = calcRt;
        if (action != null) actionGrid = action.GetComponent<GridLayoutGroup>();
    }

    private void LateUpdate() => Apply();

    private void Apply()
    {
        var host = transform as RectTransform;
        if (host == null) return;
        int w = Mathf.RoundToInt(host.rect.width);
        int h = Mathf.RoundToInt(host.rect.height);
        if (w < 80 || (w == lastW && h == lastH)) return;
        lastW = w;
        lastH = h;

        bool narrow = w < 900 || w < h * 0.95f;
        if (narrow)
        {
            Set(trackHost, 0.01f, 0.42f, 0.99f, 0.99f);
            Set(side, 0.52f, 0.18f, 0.99f, 0.41f);
            Set(tray, 0.01f, 0.01f, 0.99f, 0.17f);
            Set(action, 0.01f, 0.17f, 0.51f, 0.41f);
            Set(target, 0.01f, 0.17f, 0.51f, 0.25f);
            Set(accel, 0.52f, 0.18f, 0.99f, 0.26f);
            Set(calc, 0.01f, 0.17f, 0.99f, 0.41f);
            SetTrayHorizontal(true);
        }
        else
        {
            Set(tray, 0.008f, 0.01f, 0.20f, 0.99f);
            Set(trackHost, 0.21f, 0.16f, 0.735f, 0.99f);
            Set(side, 0.745f, 0.16f, 0.992f, 0.99f);
            Set(action, 0.21f, 0.075f, 0.992f, 0.15f);
            Set(target, 0.21f, 0.01f, 0.73f, 0.07f);
            Set(accel, 0.74f, 0.01f, 0.992f, 0.07f);
            Set(calc, 0.21f, 0.01f, 0.992f, 0.15f);
            SetTrayHorizontal(false);
        }

        if (actionGrid == null || action == null) return;
        float aw = action.rect.width;
        int cols = aw < 280 ? 2 : 4;
        float pad = actionGrid.padding.left + actionGrid.padding.right;
        float space = actionGrid.spacing.x * (cols - 1);
        float cellW = Mathf.Max(70f, (aw - pad - space) / cols);
        float cellH = Mathf.Clamp(action.rect.height - actionGrid.padding.top - actionGrid.padding.bottom, 28f, 40f);
        actionGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        actionGrid.constraintCount = cols;
        actionGrid.cellSize = new Vector2(cellW, cellH);
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
