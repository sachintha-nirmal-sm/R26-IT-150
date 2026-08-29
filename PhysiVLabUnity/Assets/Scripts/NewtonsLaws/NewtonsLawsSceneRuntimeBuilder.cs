using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

[DefaultExecutionOrder(-50)]
public class NewtonsLawsSceneRuntimeBuilder : MonoBehaviour
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
            Debug.LogWarning("Newton's Laws: wiring failed.");
            return;
        }
        referencesWired = true;
    }

    private void Awake()
    {
        if (GetComponent<NewtonsLawsFailsafeDisplay>() == null)
            gameObject.AddComponent<NewtonsLawsFailsafeDisplay>();
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
            Debug.Log("Newton's Laws practical UI built.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Newton's Laws BUILD FAILED: " + ex.Message + "\n" + ex.StackTrace);
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
            Debug.LogWarning("Newton's Laws: font load skipped — " + ex.Message);
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
            Debug.LogWarning("Newton EventSystem: " + ex.Message);
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
        AddMgr<NewtonsLawsExperimentManager>("NewtonsLawsExperimentManager");
        AddMgr<NewtonUIManager>("NewtonUIManager");
        AddMgr<NewtonScoreManager>("NewtonScoreManager");
        AddMgr<NewtonFeedbackManager>("NewtonFeedbackManager");
        AddMgr<NewtonAttemptManager>("NewtonAttemptManager");
        AddMgr<NewtonSaveManager>("NewtonSaveManager");
        AddMgr<NewtonProfileManager>("NewtonProfileManager");
        AddMgr<NewtonDataManager>("NewtonDataManager");
        AddMgr<NewtonEquipmentSelectionManager>("NewtonEquipmentSelectionManager");
        AddMgr<NewtonEquipmentSnapController>("NewtonEquipmentSnapController");
        AddMgr<FirstLawExperimentManager>("FirstLawExperimentManager");
        AddMgr<FirstLawMotionController>("FirstLawMotionController");
        AddMgr<NewtonFrictionController>("NewtonFrictionController");
        AddMgr<SecondLawExperimentManager>("SecondLawExperimentManager");
        AddMgr<NewtonForceController>("NewtonForceController");
        AddMgr<NewtonMassController>("NewtonMassController");
        AddMgr<TrolleyController>("TrolleyController");
        AddMgr<PulleyController>("PulleyController");
        AddMgr<StringConnectionController>("StringConnectionController");
        AddMgr<ThirdLawExperimentManager>("ThirdLawExperimentManager");
        AddMgr<BalloonController>("BalloonController");
        AddMgr<ActionReactionController>("ActionReactionController");
        AddMgr<WeightExperimentManager>("WeightExperimentManager");
        AddMgr<SpringBalanceController>("SpringBalanceController");
        AddMgr<NewtonAccelerationCalculator>("NewtonAccelerationCalculator");
        AddMgr<NewtonForceCalculator>("NewtonForceCalculator");
        AddMgr<NewtonObservationTableManager>("NewtonObservationTableManager");
        AddMgr<NewtonGraphController>("NewtonGraphController");
        AddMgr<NewtonQuestionManager>("NewtonQuestionManager");
        AddMgr<ConceptMatchingManager>("ConceptMatchingManager");
        AddMgr<NewtonResultManager>("NewtonResultManager");
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
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.65f;
        canvasObj.AddComponent<GraphicRaycaster>();

        Panel(canvasObj.transform, "ScreenBg", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.93f, 0.96f, 0.99f));

        var headerP = Panel(canvasObj.transform, "Header", new Vector2(0, 0.91f), Vector2.one, Vector2.zero, Vector2.zero, header);
        var title = Text("Title", headerP.transform, "NEWTON'S LAWS    Newton's Laws Investigation Lab", 28, TextAlignmentOptions.MidlineLeft, new Vector2(16, -6), new Vector2(1200, 44));
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
        var progress = Text("Progress", headerP.transform, "Step: 1 / 21", 20, TextAlignmentOptions.MidlineLeft, new Vector2(16, -46), new Vector2(220, 32));
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
        introPanel.AddComponent<NewtonIntroClickToStart>();

        var objectivePanel = Panel(main.transform, "ObjectivePanel", Vector2.zero, Vector2.one, new Vector2(36, 24), new Vector2(-36, -24), Color.white);
        objectivePanel.SetActive(false);
        var objectiveText = StretchText("ObjectiveText", objectivePanel.transform, "", 30, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 28);

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
        var requiredArea = Panel(requiredAreaHost.transform, "RequiredCards", new Vector2(0, 0), new Vector2(1, 0.72f), new Vector2(6, 4), new Vector2(-6, -2), new Color(0, 0, 0, 0), false);
        var reqLayout = requiredArea.AddComponent<HorizontalLayoutGroup>();
        reqLayout.spacing = 6;
        reqLayout.padding = new RectOffset(4, 4, 2, 2);
        reqLayout.childAlignment = TextAnchor.MiddleCenter;
        reqLayout.childControlWidth = true;
        reqLayout.childControlHeight = true;
        reqLayout.childForceExpandWidth = true;
        reqLayout.childForceExpandHeight = true;
        requiredArea.AddComponent<NewtonUIDropTarget>().Configure("RequiredEquipment", "Any", Vector2.zero);

        var scroll = CreateScrollAnchored(equipPanel.transform, new Vector2(0f, 0.25f), new Vector2(1f, 0.85f));
        var scrollRt = scroll.GetComponent<RectTransform>();
        scrollRt.offsetMin = new Vector2(8, 4);
        scrollRt.offsetMax = new Vector2(-8, -4);
        var cardPrefab = CreateCardPrefab();
        var equipContinue = BigBtn("EquipContinueBtn", equipPanel.transform, "NEXT STEP", new Vector2(1f, 0f), new Vector2(-160, 18), new Vector2(240, 48), green);
        equipContinue.gameObject.SetActive(false);

        var lab = BuildLaboratory(main.transform, accent, green, amber);
        var dataTablePanel = MakeTextPanel(main.transform, "DataTablePanel", "TableTitle", "OBSERVATION TABLES", "DataTableText");
        var graphPanel = BuildGraphPanel(main.transform, accent);
        var matching = BuildMatchingPanel(main.transform, accent, green);
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
        StretchText("ResetMsg", resetConfirm.transform, "Do you want to retry this practical?", 24, TextAlignmentOptions.Center, new Color(0.18f, 0.2f, 0.25f), 16);
        var resetRow = Panel(resetConfirm.transform, "Btns", new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.38f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var resetRowLayout = resetRow.AddComponent<HorizontalLayoutGroup>();
        resetRowLayout.spacing = 18;
        resetRowLayout.childForceExpandWidth = true;
        var resetYes = Btn("Yes", resetRow.transform, "YES", new Color(0.75f, 0.2f, 0.2f), 0, 50);
        var resetNo = Btn("No", resetRow.transform, "NO", accent, 0, 50);

        var refs = canvasObj.AddComponent<NewtonUIRefs>();
        refs.UiVersion = 11;
        refs.Title = title; refs.Score = score; refs.Progress = progress; refs.Attempts = attempts;
        refs.StepLabel = stepLabel; refs.ProgressBar = fillImg; refs.Instruction = instruction;
        refs.IntroPanel = introPanel; refs.IntroText = introText; refs.StartBtn = startBtn;
        refs.ObjectivePanel = objectivePanel; refs.ObjectiveText = objectiveText;
        refs.InstructionBar = instructionBar; refs.EquipmentPanel = equipPanel;
        refs.LaboratoryPanel = lab.panel;
        refs.DataTablePanel = dataTablePanel;
        refs.DataTableText = dataTablePanel.transform.Find("DataTableText")?.GetComponent<TextMeshProUGUI>();
        refs.GraphPanel = graphPanel.panel;
        refs.ForceGraphArea = graphPanel.forceArea;
        refs.MassGraphArea = graphPanel.massArea;
        refs.DotPrefab = graphPanel.dot;
        refs.MatchingPanel = matching.panel;
        refs.MatchingText = matching.text;
        refs.MatchC1 = matching.c1; refs.MatchC2 = matching.c2; refs.MatchC3 = matching.c3; refs.MatchC4 = matching.c4;
        refs.MatchM1 = matching.m1; refs.MatchM2 = matching.m2; refs.MatchM3 = matching.m3; refs.MatchM4 = matching.m4;
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
        refs.CardContainer = scroll.content; refs.RequiredArea = requiredArea.transform; refs.CardPrefab = cardPrefab;
        refs.LiveReadings = lab.live; refs.StopwatchText = lab.stopwatch; refs.FormulaText = lab.formula; refs.SpringText = lab.springText;
        refs.StartExpBtn = lab.startExp; refs.StopExpBtn = lab.stopExp;
        refs.ResetRunBtn = lab.resetRun; refs.RecordBtn = lab.record; refs.ActionBtn = lab.action;
        refs.Force1 = lab.f1; refs.Force2 = lab.f2; refs.Force3 = lab.f3; refs.Force4 = lab.f4; refs.Force5 = lab.f5;
        refs.Mass05 = lab.m05; refs.Mass10 = lab.m10; refs.Mass15 = lab.m15; refs.Mass20 = lab.m20; refs.Mass40 = lab.m40;
        refs.FrictionLow = lab.lowF; refs.FrictionHigh = lab.highF;
        refs.ObsRest = lab.obsRest; refs.ObsMove = lab.obsMove; refs.WrongObs = lab.wrongObs; refs.ExplainBtn = lab.explain;
        refs.InflateBtn = lab.inflate; refs.HangBtn = lab.hang;
        refs.SetupA = lab.setupA; refs.SetupB = lab.setupB; refs.SetupC = lab.setupC; refs.SetupD = lab.setupD;
        refs.Weight05 = lab.w05; refs.Weight10 = lab.w10; refs.Weight20 = lab.w20; refs.RecordWeightBtn = lab.recordW;
        refs.CalcPanel = lab.calcPanel; refs.CalcPrompt = lab.calcPrompt; refs.CalcInput = lab.calcInput; refs.CheckCalcBtn = lab.checkCalc;
        refs.ActionRow = lab.actionRow; refs.ForceRow = lab.forceRow; refs.MassRow = lab.massRow; refs.FrictionRow = lab.frictionRow;
        refs.TrackArea = lab.track; refs.EquipmentTray = lab.tray; refs.CarRect = lab.trolley;
        refs.TrackVisual = lab.trackVisual; refs.RulerVisual = lab.rulerVisual;
        refs.PulleyVisual = lab.pulley; refs.StringVisual = lab.stringVis; refs.HangerVisual = lab.hanger;
        refs.BalloonObj = lab.balloon; refs.StrawVisual = lab.straw;
        refs.ActionArrow = lab.actionArrow; refs.ReactionArrow = lab.reactionArrow;
        refs.AppliedArrow = lab.appliedArrow; refs.NetArrow = lab.netArrow; refs.ResistArrow = lab.resistArrow;
        refs.SpringVisual = lab.spring; refs.Pointer = lab.pointer;
        refs.ResultBanner = lab.resultBanner; refs.ResultBannerText = lab.resultBannerText;
        refs.WeightStage = lab.weightStage; refs.WeightSpring = lab.weightSpring;
        refs.WeightObjectVisual = lab.weightObject; refs.WeightReadingText = lab.weightReading;

        instructionBar.transform.SetSiblingIndex(headerP.transform.GetSiblingIndex() + 1);
        canvasObj.transform.Find("BottomBar")?.SetAsLastSibling();
        feedbackPanel.transform.SetAsLastSibling();
        resetConfirm.transform.SetAsLastSibling();
    }

    private bool WireReferences(bool showWelcome)
    {
        var refs = Object.FindAnyObjectByType<NewtonUIRefs>();
        if (refs == null || NewtonUIManager.Instance == null) return false;
        NewtonFeedbackManager.Instance?.BindUI(refs.FeedbackPanel, refs.FeedbackText, refs.ScoreChangeText, refs.FeedbackGroup);
        NewtonUIManager.Instance.BindAll(refs, showWelcome);
        NewtonEquipmentSelectionManager.Instance?.SetupUI(refs.CardContainer, refs.RequiredArea, refs.CardPrefab);
        NewtonObservationTableManager.Instance?.Bind(refs.DataTableText);
        NewtonGraphController.Instance?.Bind(refs.ForceGraphArea, refs.MassGraphArea, refs.DotPrefab);
        NewtonResultManager.Instance?.Bind(refs.FinalScore, refs.ResultDetails, refs.StatusText);
        TrolleyController.Instance?.Bind(refs.CarRect, refs.TrackArea);
        NewtonEquipmentSnapController.Instance?.Bind(refs.TrackArea, refs.EquipmentTray, refs.TrackVisual, refs.RulerVisual);
        PulleyController.Instance?.Bind(refs.PulleyVisual);
        StringConnectionController.Instance?.Bind(refs.StringVisual, refs.HangerVisual);
        BalloonController.Instance?.Bind(refs.BalloonObj != null ? refs.BalloonObj.GetComponent<RectTransform>() : null);
        ActionReactionController.Instance?.Bind(refs.ActionArrow, refs.ReactionArrow);
        SpringBalanceController.Instance?.Bind(refs.SpringVisual, refs.Pointer);
        NewtonsLawsFailsafeDisplay.Hide();
        return refs.StartBtn != null;
    }

    private LabBits BuildLaboratory(Transform parent, Color accent, Color green, Color amber)
    {
        var labPanel = Panel(parent, "LaboratoryPanel", Vector2.zero, Vector2.one, new Vector2(8, 4), new Vector2(-8, -4), new Color(0, 0, 0, 0), false);
        labPanel.SetActive(false);

        var tray = Panel(labPanel.transform, "EquipmentTray", new Vector2(0.008f, 0.01f), new Vector2(0.20f, 0.99f), Vector2.zero, Vector2.zero, new Color(0.93f, 0.95f, 0.99f, 1f));
        var trayHeader = Panel(tray.transform, "TrayHeader", new Vector2(0f, 0.90f), Vector2.one, Vector2.zero, Vector2.zero, new Color(0.12f, 0.28f, 0.48f));
        StretchText("TrayLabel", trayHeader.transform, "EQUIPMENT\nDrag onto the setup", 20, TextAlignmentOptions.Center, Color.white, 6).fontStyle = FontStyles.Bold;
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

        var trackHost = Panel(labPanel.transform, "TrackHost", new Vector2(0.21f, 0.23f), new Vector2(0.735f, 0.99f), Vector2.zero, Vector2.zero, new Color(0.86f, 0.91f, 0.96f));
        Text("TrackTitle", trackHost.transform, "EXPERIMENT AREA", 22, TextAlignmentOptions.MidlineLeft, new Vector2(14, -8), new Vector2(420, 32));
        var resultBanner = Panel(trackHost.transform, "ResultBanner", new Vector2(0.04f, 0.84f), new Vector2(0.96f, 0.98f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.48f, 0.22f));
        resultBanner.GetComponent<Image>().raycastTarget = false;
        resultBanner.SetActive(false);
        var resultBannerText = StretchText("ResultText", resultBanner.transform, "Press START to run the experiment.", 20, TextAlignmentOptions.Center, Color.white, 6);
        resultBannerText.fontStyle = FontStyles.Bold;
        resultBannerText.enableAutoSizing = true;
        resultBannerText.fontSizeMin = 16;
        resultBannerText.fontSizeMax = 22;
        resultBannerText.raycastTarget = false;
        var track = Panel(trackHost.transform, "Track", new Vector2(0.03f, 0.18f), new Vector2(0.97f, 0.82f), Vector2.zero, Vector2.zero, new Color(0.55f, 0.60f, 0.66f, 0.35f));
        track.AddComponent<NewtonUIDropTarget>().Configure("Track", "Any", Vector2.zero, 2.5f);
        var trackVisual = Panel(track.transform, "TrackVisual", new Vector2(0f, 0.28f), new Vector2(1f, 0.72f), Vector2.zero, Vector2.zero, new Color(0.32f, 0.35f, 0.40f));
        trackVisual.SetActive(false);
        var lane = Panel(trackVisual.transform, "Lane", new Vector2(0.02f, 0.42f), new Vector2(0.98f, 0.58f), Vector2.zero, Vector2.zero, new Color(0.95f, 0.86f, 0.22f));
        lane.GetComponent<Image>().raycastTarget = false;
        var rulerVisual = Panel(track.transform, "RulerVisual", new Vector2(0f, 0f), new Vector2(1f, 0.22f), Vector2.zero, Vector2.zero, new Color(0.95f, 0.85f, 0.45f));
        rulerVisual.SetActive(false);
        var startZone = Panel(track.transform, "StartZone", new Vector2(0f, 0.18f), new Vector2(0.14f, 1f), Vector2.zero, Vector2.zero, new Color(0.2f, 0.72f, 0.35f, 0.40f));
        startZone.AddComponent<NewtonUIDropTarget>().Configure("Start", "Any", Vector2.zero, 0f);
        Text("L0", trackHost.transform, "0 m", 20, TextAlignmentOptions.Center, Vector2.zero, new Vector2(70, 28), new Vector2(0.03f, 0.06f), new Vector2(0.03f, 0.06f), new Vector2(0.5f, 0.5f));
        Text("L5", trackHost.transform, "5 m", 20, TextAlignmentOptions.Center, Vector2.zero, new Vector2(70, 28), new Vector2(0.97f, 0.06f), new Vector2(0.97f, 0.06f), new Vector2(0.5f, 0.5f));

        var trolley = Panel(track.transform, "Trolley", new Vector2(0f, 0.52f), new Vector2(0f, 0.52f), Vector2.zero, new Vector2(160, 90), Color.white);
        trolley.GetComponent<Image>().sprite = NewtonsLawsIconFactory.GetNamed("trolley");
        trolley.GetComponent<Image>().preserveAspect = true;
        trolley.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

        var pulley = Panel(trackHost.transform, "Pulley", new Vector2(0.90f, 0.78f), new Vector2(0.99f, 0.98f), Vector2.zero, Vector2.zero, Color.white);
        pulley.GetComponent<Image>().sprite = NewtonsLawsIconFactory.GetNamed("pulley");
        pulley.GetComponent<Image>().preserveAspect = true;
        pulley.SetActive(false);
        var stringVis = Panel(trackHost.transform, "StringVis", new Vector2(0.20f, 0.70f), new Vector2(0.92f, 0.74f), Vector2.zero, Vector2.zero, new Color(0.45f, 0.32f, 0.16f));
        stringVis.SetActive(false);
        var hanger = Panel(trackHost.transform, "Hanger", new Vector2(0.90f, 0.20f), new Vector2(0.99f, 0.55f), Vector2.zero, Vector2.zero, Color.white);
        hanger.GetComponent<Image>().sprite = NewtonsLawsIconFactory.GetNamed("hanger");
        hanger.GetComponent<Image>().preserveAspect = true;
        hanger.SetActive(false);

        var straw = Panel(trackHost.transform, "Straw", new Vector2(0.12f, 0.86f), new Vector2(0.88f, 0.92f), Vector2.zero, Vector2.zero, new Color(0.9f, 0.92f, 0.95f));
        straw.SetActive(false);
        var balloon = Panel(trackHost.transform, "Balloon", new Vector2(0.18f, 0.55f), new Vector2(0.18f, 0.55f), Vector2.zero, new Vector2(90, 110), Color.white);
        balloon.GetComponent<Image>().sprite = NewtonsLawsIconFactory.GetNamed("balloon");
        balloon.GetComponent<Image>().preserveAspect = true;
        balloon.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        balloon.SetActive(false);

        var actionArrow = Panel(trackHost.transform, "ActionArrow", new Vector2(0.08f, 0.78f), new Vector2(0.28f, 0.88f), Vector2.zero, Vector2.zero, Color.white);
        actionArrow.GetComponent<Image>().sprite = NewtonsLawsIconFactory.GetNamed("arrowLeft");
        StretchText("ALbl", actionArrow.transform, "AIR ←", 16, TextAlignmentOptions.Center, Color.white, 2).fontStyle = FontStyles.Bold;
        actionArrow.SetActive(false);
        var reactionArrow = Panel(trackHost.transform, "ReactionArrow", new Vector2(0.55f, 0.78f), new Vector2(0.78f, 0.88f), Vector2.zero, Vector2.zero, Color.white);
        reactionArrow.GetComponent<Image>().sprite = NewtonsLawsIconFactory.GetNamed("arrowRight");
        StretchText("RLbl", reactionArrow.transform, "BALLOON →", 16, TextAlignmentOptions.Center, Color.white, 2).fontStyle = FontStyles.Bold;
        reactionArrow.SetActive(false);
        var appliedArrow = Panel(trackHost.transform, "AppliedArrow", new Vector2(0.40f, 0.88f), new Vector2(0.68f, 0.97f), Vector2.zero, Vector2.zero, new Color(0.12f, 0.55f, 0.28f));
        StretchText("Ap", appliedArrow.transform, "Applied Force →", 16, TextAlignmentOptions.Center, Color.white, 2);
        appliedArrow.SetActive(false);
        var resistArrow = Panel(trackHost.transform, "ResistArrow", new Vector2(0.12f, 0.88f), new Vector2(0.38f, 0.97f), Vector2.zero, Vector2.zero, new Color(0.75f, 0.25f, 0.2f));
        StretchText("Rs", resistArrow.transform, "← Resistance", 16, TextAlignmentOptions.Center, Color.white, 2);
        resistArrow.SetActive(false);
        var netArrow = Panel(trackHost.transform, "NetArrow", new Vector2(0.70f, 0.88f), new Vector2(0.96f, 0.97f), Vector2.zero, Vector2.zero, new Color(0.15f, 0.35f, 0.75f));
        StretchText("Nt", netArrow.transform, "Net Force →", 16, TextAlignmentOptions.Center, Color.white, 2);
        netArrow.SetActive(false);

        var weightStage = Panel(trackHost.transform, "WeightStage", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.90f, 0.94f, 0.98f));
        weightStage.SetActive(false);
        Text("WeightTitle", weightStage.transform, "SPRING BALANCE", 26, TextAlignmentOptions.Center, Vector2.zero, new Vector2(420, 36), new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.5f));
        var support = Panel(weightStage.transform, "Support", new Vector2(0.18f, 0.86f), new Vector2(0.82f, 0.90f), Vector2.zero, Vector2.zero, new Color(0.35f, 0.38f, 0.42f));
        support.GetComponent<Image>().raycastTarget = false;
        var weightSpring = Panel(weightStage.transform, "WeightSpring", new Vector2(0.42f, 0.38f), new Vector2(0.58f, 0.86f), Vector2.zero, Vector2.zero, Color.white);
        weightSpring.GetComponent<Image>().sprite = NewtonsLawsIconFactory.GetNamed("spring");
        weightSpring.GetComponent<Image>().preserveAspect = true;
        weightSpring.GetComponent<Image>().raycastTarget = false;
        var weightObject = Panel(weightStage.transform, "WeightObject", new Vector2(0.38f, 0.12f), new Vector2(0.62f, 0.36f), Vector2.zero, Vector2.zero, new Color(0.20f, 0.48f, 0.78f));
        weightObject.SetActive(false);
        StretchText("MassLbl", weightObject.transform, "0.5 kg", 22, TextAlignmentOptions.Center, Color.white, 4).fontStyle = FontStyles.Bold;
        var weightReading = StretchText("WeightReading", weightStage.transform, "Reading: 0.00 N", 24, TextAlignmentOptions.Center, new Color(0.10f, 0.22f, 0.40f), 6);
        var wrRt = weightReading.rectTransform;
        wrRt.anchorMin = new Vector2(0.10f, 0.02f);
        wrRt.anchorMax = new Vector2(0.90f, 0.11f);
        wrRt.offsetMin = wrRt.offsetMax = Vector2.zero;
        weightReading.fontStyle = FontStyles.Bold;

        var side = Panel(labPanel.transform, "SidePanel", new Vector2(0.745f, 0.23f), new Vector2(0.992f, 0.99f), Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.98f));
        var swDrop = Panel(side.transform, "StopwatchDrop", new Vector2(0.06f, 0.88f), new Vector2(0.94f, 0.98f), Vector2.zero, Vector2.zero, new Color(0.88f, 0.93f, 1f));
        swDrop.AddComponent<NewtonUIDropTarget>().Configure("Stopwatch", "Stopwatch", Vector2.zero);
        var swText = StretchText("StopwatchText", swDrop.transform, "00.00 s", 28, TextAlignmentOptions.Center, new Color(0.10f, 0.14f, 0.22f), 4);
        swText.fontStyle = FontStyles.Bold;
        var springText = StretchText("SpringText", side.transform, "Force: 0.00 N", 18, TextAlignmentOptions.MidlineLeft, new Color(0.12f, 0.16f, 0.22f), 6);
        var stRt = springText.rectTransform;
        stRt.anchorMin = new Vector2(0.06f, 0.82f);
        stRt.anchorMax = new Vector2(0.94f, 0.87f);
        var spring = Panel(side.transform, "SpringVisual", new Vector2(0.38f, 0.70f), new Vector2(0.62f, 0.81f), Vector2.zero, Vector2.zero, Color.white);
        spring.GetComponent<Image>().sprite = NewtonsLawsIconFactory.GetNamed("spring");
        spring.GetComponent<Image>().preserveAspect = true;
        spring.SetActive(false);
        var pointer = Panel(spring.transform, "Pointer", new Vector2(0.7f, 0.8f), new Vector2(1.1f, 0.9f), Vector2.zero, Vector2.zero, new Color(0.8f, 0.15f, 0.15f));
        var liveBox = Panel(side.transform, "ReadingsBox", new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.81f), Vector2.zero, Vector2.zero, new Color(0.94f, 0.97f, 1f));
        var live = StretchText("LiveReadings", liveBox.transform, "LIVE READINGS", 18, TextAlignmentOptions.TopLeft, new Color(0.12f, 0.16f, 0.22f), 10);
        live.fontStyle = FontStyles.Bold;
        live.enableAutoSizing = true;
        live.fontSizeMin = 14;
        live.fontSizeMax = 20;
        live.overflowMode = TextOverflowModes.Truncate;
        live.lineSpacing = 8f;
        var formulaBox = Panel(side.transform, "FormulaBox", new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.26f), Vector2.zero, Vector2.zero, new Color(0.10f, 0.22f, 0.40f));
        var formula = StretchText("FormulaText", formulaBox.transform, "FORMULAS", 16, TextAlignmentOptions.TopLeft, Color.white, 10);
        formula.fontStyle = FontStyles.Bold;
        formula.enableAutoSizing = true;
        formula.fontSizeMin = 13;
        formula.fontSizeMax = 17;
        formula.overflowMode = TextOverflowModes.Truncate;
        formula.lineSpacing = 4f;

        var actionRow = Panel(labPanel.transform, "ActionRow", new Vector2(0.21f, 0.075f), new Vector2(0.992f, 0.145f), Vector2.zero, Vector2.zero, new Color(0.16f, 0.22f, 0.28f));
        actionRow.SetActive(false);
        var aLayout = actionRow.AddComponent<GridLayoutGroup>();
        aLayout.cellSize = new Vector2(140, 36);
        aLayout.spacing = new Vector2(6, 4);
        aLayout.padding = new RectOffset(6, 6, 4, 4);
        aLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        aLayout.constraintCount = 4;
        aLayout.childAlignment = TextAnchor.MiddleCenter;
        var startExp = Btn("StartExp", actionRow.transform, "START", green, 0, 36);
        startExp.gameObject.AddComponent<NewtonLabStartClick>();
        var stopExp = Btn("StopExp", actionRow.transform, "STOP", new Color(0.75f, 0.25f, 0.2f), 0, 36);
        var record = Btn("Record", actionRow.transform, "RECORD", accent, 0, 36);
        var resetRun = Btn("ResetRun", actionRow.transform, "RESET", new Color(0.45f, 0.48f, 0.52f), 0, 36);
        var action = Btn("Action", actionRow.transform, "PUSH", amber, 0, 36);
        var inflate = Btn("Inflate", actionRow.transform, "INFLATE", amber, 0, 36);
        var hang = Btn("Hang", actionRow.transform, "HANG OBJECT", green, 0, 36);
        var setupA = Btn("SetupA", actionRow.transform, "PLACE TROLLEY", green, 0, 36);
        var setupB = Btn("SetupB", actionRow.transform, "PLACE PULLEY", accent, 0, 36);
        var setupC = Btn("SetupC", actionRow.transform, "PLACE STRING", amber, 0, 36);
        var setupD = Btn("SetupD", actionRow.transform, "PLACE HANGER", green, 0, 36);
        setupA.gameObject.SetActive(false);
        setupB.gameObject.SetActive(false);
        setupC.gameObject.SetActive(false);
        setupD.gameObject.SetActive(false);

        var obsRow = Panel(labPanel.transform, "ObsRow", new Vector2(0.21f, 0.008f), new Vector2(0.992f, 0.068f), Vector2.zero, Vector2.zero, new Color(0.20f, 0.26f, 0.32f));
        var oLayout = obsRow.AddComponent<GridLayoutGroup>();
        oLayout.cellSize = new Vector2(140, 32);
        oLayout.spacing = new Vector2(6, 4);
        oLayout.padding = new RectOffset(6, 6, 4, 4);
        oLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        oLayout.constraintCount = 4;
        oLayout.childAlignment = TextAnchor.MiddleCenter;
        var obsRest = Btn("ObsRest", obsRow.transform, "Remains at rest", green, 0, 32);
        var obsMove = Btn("ObsMove", obsRow.transform, "Uniform velocity", green, 0, 32);
        var wrongObs = Btn("WrongObs", obsRow.transform, "Stops immediately", new Color(0.55f, 0.55f, 0.58f), 0, 32);
        var explain = Btn("Explain", obsRow.transform, "CONFIRM EXPLANATION", accent, 0, 32);
        var w05 = Btn("W05", obsRow.transform, "0.5 kg", accent, 0, 32);
        var w10 = Btn("W10", obsRow.transform, "1.0 kg", accent, 0, 32);
        var w20 = Btn("W20", obsRow.transform, "2.0 kg", accent, 0, 32);
        var recordW = Btn("RecordW", obsRow.transform, "RECORD WEIGHT", green, 0, 32);
        obsRest.gameObject.SetActive(false);
        obsMove.gameObject.SetActive(false);
        wrongObs.gameObject.SetActive(false);
        explain.gameObject.SetActive(false);
        w05.gameObject.SetActive(false);
        w10.gameObject.SetActive(false);
        w20.gameObject.SetActive(false);
        recordW.gameObject.SetActive(false);

        var forceRow = Panel(labPanel.transform, "ForceRow", new Vector2(0.21f, 0.155f), new Vector2(0.73f, 0.22f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        forceRow.SetActive(false);
        var fLayout = forceRow.AddComponent<HorizontalLayoutGroup>();
        fLayout.spacing = 8; fLayout.childForceExpandWidth = true; fLayout.childControlWidth = true;
        var f1 = Btn("F1", forceRow.transform, "1 N", accent, 0, 40);
        var f2 = Btn("F2", forceRow.transform, "2 N", accent, 0, 40);
        var f3 = Btn("F3", forceRow.transform, "3 N", accent, 0, 40);
        var f4 = Btn("F4", forceRow.transform, "4 N", accent, 0, 40);
        var f5 = Btn("F5", forceRow.transform, "5 N", accent, 0, 40);

        var massRow = Panel(labPanel.transform, "MassRow", new Vector2(0.74f, 0.155f), new Vector2(0.992f, 0.22f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        massRow.SetActive(false);
        var mLayout = massRow.AddComponent<HorizontalLayoutGroup>();
        mLayout.spacing = 4; mLayout.childForceExpandWidth = true;
        var m05 = Btn("M05", massRow.transform, "0.5 kg", amber, 0, 40);
        var m10 = Btn("M10", massRow.transform, "1 kg", amber, 0, 40);
        var m15 = Btn("M15", massRow.transform, "1.5 kg", amber, 0, 40);
        var m20 = Btn("M20", massRow.transform, "2 kg", amber, 0, 40);
        var m40 = Btn("M40", massRow.transform, "4 kg", amber, 0, 40);

        var frictionRow = Panel(labPanel.transform, "FrictionRow", new Vector2(0.008f, 0.155f), new Vector2(0.20f, 0.22f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        frictionRow.SetActive(false);
        var frLayout = frictionRow.AddComponent<HorizontalLayoutGroup>();
        frLayout.spacing = 6; frLayout.childForceExpandWidth = true;
        var lowF = Btn("LowF", frictionRow.transform, "LOW FRICTION", green, 0, 40);
        var highF = Btn("HighF", frictionRow.transform, "HIGH FRICTION", new Color(0.75f, 0.3f, 0.25f), 0, 40);

        var calcPanel = Panel(labPanel.transform, "CalcPanel", new Vector2(0.21f, 0.01f), new Vector2(0.992f, 0.09f), Vector2.zero, Vector2.zero, new Color(0.97f, 0.98f, 1f));
        calcPanel.SetActive(false);
        var calcPrompt = Text("CalcPrompt", calcPanel.transform, "Enter your calculated value", 18, TextAlignmentOptions.MidlineLeft, new Vector2(12, 0), new Vector2(520, 36), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        var calcInput = CreateInput(calcPanel.transform, "CalcInput", new Vector2(0.48f, 0.18f), new Vector2(0.72f, 0.82f), "value");
        var checkCalc = Btn("CheckCalc", calcPanel.transform, "CHECK", green, 160, 40);
        var checkRt = checkCalc.GetComponent<RectTransform>();
        checkRt.anchorMin = checkRt.anchorMax = new Vector2(0.88f, 0.5f);
        checkRt.pivot = new Vector2(0.5f, 0.5f);
        checkRt.sizeDelta = new Vector2(150, 40);
        Object.Destroy(checkCalc.GetComponent<LayoutElement>());

        actionRow.transform.SetAsLastSibling();

        var adaptive = labPanel.AddComponent<NewtonAdaptiveLab>();
        adaptive.Bind(
            tray.GetComponent<RectTransform>(),
            scroll,
            trayInnerRt,
            trackHost.GetComponent<RectTransform>(),
            side.GetComponent<RectTransform>(),
            actionRow.GetComponent<RectTransform>(),
            obsRow.GetComponent<RectTransform>(),
            forceRow.GetComponent<RectTransform>(),
            massRow.GetComponent<RectTransform>(),
            frictionRow.GetComponent<RectTransform>(),
            calcPanel.GetComponent<RectTransform>());

        return new LabBits
        {
            panel = labPanel, live = live, stopwatch = swText, formula = formula, springText = springText,
            resultBanner = resultBanner, resultBannerText = resultBannerText,
            startExp = startExp, stopExp = stopExp, resetRun = resetRun, record = record, action = action,
            inflate = inflate, hang = hang, setupA = setupA, setupB = setupB, setupC = setupC, setupD = setupD,
            obsRest = obsRest, obsMove = obsMove, wrongObs = wrongObs, explain = explain,
            w05 = w05, w10 = w10, w20 = w20, recordW = recordW,
            f1 = f1, f2 = f2, f3 = f3, f4 = f4, f5 = f5,
            m05 = m05, m10 = m10, m15 = m15, m20 = m20, m40 = m40,
            lowF = lowF, highF = highF,
            calcPanel = calcPanel, calcPrompt = calcPrompt, calcInput = calcInput, checkCalc = checkCalc,
            actionRow = actionRow, forceRow = forceRow, massRow = massRow, frictionRow = frictionRow,
            track = track.GetComponent<RectTransform>(), tray = trayInnerRt, trolley = trolley.GetComponent<RectTransform>(),
            trackVisual = trackVisual, rulerVisual = rulerVisual, pulley = pulley, stringVis = stringVis, hanger = hanger,
            balloon = balloon, straw = straw,
            actionArrow = actionArrow, reactionArrow = reactionArrow, appliedArrow = appliedArrow, netArrow = netArrow, resistArrow = resistArrow,
            spring = spring, pointer = pointer,
            weightStage = weightStage, weightSpring = weightSpring, weightObject = weightObject, weightReading = weightReading
        };
    }

    private GraphBits BuildGraphPanel(Transform parent, Color accent)
    {
        var graphPanel = Panel(parent, "GraphPanel", Vector2.zero, Vector2.one, new Vector2(20, 12), new Vector2(-20, -12), new Color(0.97f, 0.98f, 1f));
        graphPanel.SetActive(false);
        Text("GraphTitle", graphPanel.transform, "GRAPHS  —  generated from your experiment data", 24, TextAlignmentOptions.MidlineLeft, new Vector2(20, -10), new Vector2(900, 36));
        var fArea = Panel(graphPanel.transform, "ForceGraph", new Vector2(0.03f, 0.52f), new Vector2(0.97f, 0.92f), Vector2.zero, Vector2.zero, new Color(0.92f, 0.95f, 1f));
        Text("FLabel", fArea.transform, "Force (N) →     Acceleration (m/s²) ↑", 16, TextAlignmentOptions.Bottom, new Vector2(0, 4), new Vector2(700, 22), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        var mArea = Panel(graphPanel.transform, "MassGraph", new Vector2(0.03f, 0.08f), new Vector2(0.97f, 0.48f), Vector2.zero, Vector2.zero, new Color(1f, 0.95f, 0.90f));
        Text("MLabel", mArea.transform, "Mass (kg) →     Acceleration (m/s²) ↑", 16, TextAlignmentOptions.Bottom, new Vector2(0, 4), new Vector2(700, 22), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        var dotPrefab = Panel(fArea.transform, "DotPrefab", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-6, -6), new Vector2(6, 6), accent);
        dotPrefab.SetActive(false);
        return new GraphBits { panel = graphPanel, forceArea = fArea.GetComponent<RectTransform>(), massArea = mArea.GetComponent<RectTransform>(), dot = dotPrefab };
    }

    private MatchBits BuildMatchingPanel(Transform parent, Color accent, Color green)
    {
        var panel = Panel(parent, "MatchingPanel", Vector2.zero, Vector2.one, new Vector2(24, 16), new Vector2(-24, -16), new Color(0.97f, 0.98f, 1f));
        panel.SetActive(false);
        var text = StretchText("MatchingText", panel.transform, "FINAL CONCEPT MATCHING", 26, TextAlignmentOptions.Top, new Color(0.14f, 0.18f, 0.24f), 16);
        var tr = text.rectTransform;
        tr.anchorMin = new Vector2(0.04f, 0.86f);
        tr.anchorMax = new Vector2(0.96f, 0.98f);
        var left = Panel(panel.transform, "Concepts", new Vector2(0.04f, 0.08f), new Vector2(0.48f, 0.84f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var right = Panel(panel.transform, "Meanings", new Vector2(0.52f, 0.08f), new Vector2(0.96f, 0.84f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var l = left.AddComponent<VerticalLayoutGroup>(); l.spacing = 12; l.childForceExpandHeight = true; l.childControlHeight = true; l.childForceExpandWidth = true;
        var r = right.AddComponent<VerticalLayoutGroup>(); r.spacing = 12; r.childForceExpandHeight = true; r.childControlHeight = true; r.childForceExpandWidth = true;
        var c1 = Btn("C1", left.transform, "Newton's First Law", accent, 0, 70);
        var c2 = Btn("C2", left.transform, "Newton's Second Law", accent, 0, 70);
        var c3 = Btn("C3", left.transform, "Newton's Third Law", accent, 0, 70);
        var c4 = Btn("C4", left.transform, "Weight", accent, 0, 70);
        var m1 = Btn("M1", right.transform, "Inertia / uniform motion", green, 0, 70);
        var m2 = Btn("M2", right.transform, "F = ma", green, 0, 70);
        var m3 = Btn("M3", right.transform, "Equal and opposite action-reaction", green, 0, 70);
        var m4 = Btn("M4", right.transform, "W = mg", green, 0, 70);
        return new MatchBits { panel = panel, text = text, c1 = c1, c2 = c2, c3 = c3, c4 = c4, m1 = m1, m2 = m2, m3 = m3, m4 = m4 };
    }

    private QuestionBits BuildQuestionPanel(Transform parent, Color header)
    {
        var questionPanel = Panel(parent, "QuestionPanel", Vector2.zero, Vector2.one, new Vector2(18, 10), new Vector2(-18, -10), new Color(0.96f, 0.98f, 1f));
        questionPanel.SetActive(false);
        var qHeader = Panel(questionPanel.transform, "Header", new Vector2(0f, 0.78f), new Vector2(1f, 1f), new Vector2(12, 8), new Vector2(-12, -8), header);
        var questionText = StretchText("Question", qHeader.transform, "Question", 24, TextAlignmentOptions.MidlineLeft, Color.white, 14);
        var qOptions = Panel(questionPanel.transform, "Options", new Vector2(0.04f, 0.14f), new Vector2(0.96f, 0.76f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var vLayout = qOptions.AddComponent<VerticalLayoutGroup>();
        vLayout.spacing = 10; vLayout.padding = new RectOffset(8, 8, 8, 8);
        vLayout.childControlHeight = true; vLayout.childForceExpandHeight = true;
        vLayout.childControlWidth = true; vLayout.childForceExpandWidth = true;
        var optA = ChoiceBtn("OptA", qOptions.transform, "A", "");
        var optB = ChoiceBtn("OptB", qOptions.transform, "B", "");
        var optC = ChoiceBtn("OptC", qOptions.transform, "C", "");
        var optD = ChoiceBtn("OptD", qOptions.transform, "D", "");
        var numericGroup = Panel(questionPanel.transform, "NumericGroup", new Vector2(0.14f, 0.16f), new Vector2(0.86f, 0.72f), Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 1f));
        numericGroup.SetActive(false);
        StretchText("NumericPrompt", numericGroup.transform, "Type the number, then press SUBMIT.", 22, TextAlignmentOptions.Center, new Color(0.12f, 0.18f, 0.26f), 10);
        var promptRt = numericGroup.transform.Find("NumericPrompt") as RectTransform;
        if (promptRt != null)
        {
            promptRt.anchorMin = new Vector2(0.06f, 0.78f);
            promptRt.anchorMax = new Vector2(0.94f, 0.96f);
            promptRt.offsetMin = promptRt.offsetMax = Vector2.zero;
        }
        var numericInput = CreateInput(numericGroup.transform, "NumericInput", new Vector2(0.08f, 0.46f), new Vector2(0.92f, 0.74f), "e.g. 19.6");
        numericInput.contentType = TMP_InputField.ContentType.DecimalNumber;
        numericInput.lineType = TMP_InputField.LineType.SingleLine;
        if (numericInput.textComponent != null)
        {
            numericInput.textComponent.enableWordWrapping = false;
            numericInput.textComponent.fontSize = 32;
            numericInput.textComponent.alignment = TextAlignmentOptions.MidlineLeft;
        }
        if (numericInput.placeholder is TextMeshProUGUI phTmp)
        {
            phTmp.enableWordWrapping = false;
            phTmp.fontSize = 26;
            phTmp.overflowMode = TextOverflowModes.Overflow;
        }
        var numericSubmit = Panel(numericGroup.transform, "NumericSubmit", new Vector2(0.28f, 0.12f), new Vector2(0.72f, 0.38f), Vector2.zero, Vector2.zero, new Color(0.12f, 0.62f, 0.35f));
        var numericSubmitBtn = numericSubmit.AddComponent<Button>();
        numericSubmitBtn.targetGraphic = numericSubmit.GetComponent<Image>();
        StretchText("Text", numericSubmit.transform, "SUBMIT", 28, TextAlignmentOptions.Center, Color.white, 4).fontStyle = FontStyles.Bold;
        var explanationPanel = Panel(questionPanel.transform, "ExplanationPanel", new Vector2(0.08f, 0.14f), new Vector2(0.92f, 0.48f), Vector2.zero, Vector2.zero, new Color(0.12f, 0.46f, 0.32f));
        explanationPanel.SetActive(false);
        var explanationText = StretchText("ExplanationText", explanationPanel.transform, "", 22, TextAlignmentOptions.Center, Color.white, 16);
        var continueObj = Panel(questionPanel.transform, "ContinueBtn", new Vector2(0.28f, 0.02f), new Vector2(0.72f, 0.12f), Vector2.zero, Vector2.zero, new Color(0.12f, 0.62f, 0.35f));
        var continueBtn = continueObj.AddComponent<Button>();
        StretchText("Text", continueObj.transform, "CONTINUE", 28, TextAlignmentOptions.Center, Color.white, 4).fontStyle = FontStyles.Bold;
        continueObj.SetActive(false);
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
            numericInput = numericInput, numericSubmit = numericSubmitBtn
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
        var viewProfileBtn = Btn("ViewProfile", resultBtnRow.transform, "BACK TO PROFILE", accent, 0, 52);
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
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.textViewport = obj.GetComponent<RectTransform>();
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        ph.enableWordWrapping = false;
        ph.overflowMode = TextOverflowModes.Overflow;
        return input;
    }

    private GameObject Panel(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax, Color color, bool raycast = true)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = offMin; rt.offsetMax = offMax;
        var img = obj.AddComponent<Image>();
        img.sprite = NewtonsLawsIconFactory.White();
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
        tmp.overflowMode = TextOverflowModes.Overflow;
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
        img.sprite = NewtonsLawsIconFactory.White();
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
        content.AddComponent<NewtonAdaptiveGrid>();
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
        card.AddComponent<NewtonEquipmentCardUI>();
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
        public GameObject panel, calcPanel, actionRow, forceRow, massRow, frictionRow, resultBanner;
        public GameObject trackVisual, rulerVisual, pulley, stringVis, hanger, balloon, straw;
        public GameObject actionArrow, reactionArrow, appliedArrow, netArrow, resistArrow, spring, pointer;
        public GameObject weightStage, weightSpring, weightObject;
        public TextMeshProUGUI live, stopwatch, calcPrompt, formula, springText, resultBannerText, weightReading;
        public Button startExp, stopExp, resetRun, record, action, inflate, hang;
        public Button setupA, setupB, setupC, setupD;
        public Button obsRest, obsMove, wrongObs, explain, w05, w10, w20, recordW;
        public Button f1, f2, f3, f4, f5, m05, m10, m15, m20, m40, lowF, highF, checkCalc;
        public TMP_InputField calcInput;
        public RectTransform track, tray, trolley;
    }

    private struct GraphBits
    {
        public GameObject panel, dot;
        public RectTransform forceArea, massArea;
    }

    private struct MatchBits
    {
        public GameObject panel;
        public TextMeshProUGUI text;
        public Button c1, c2, c3, c4, m1, m2, m3, m4;
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

public class NewtonUIRefs : MonoBehaviour
{
    public int UiVersion;
    public TextMeshProUGUI Title, Score, Progress, Attempts, StepLabel, Instruction;
    public TextMeshProUGUI IntroText, ObjectiveText, ConclusionText, QuestionText, DataTableText, MatchingText;
    public TextMeshProUGUI FinalScore, ResultDetails, StatusText, FeedbackText, ScoreChangeText;
    public TextMeshProUGUI LiveReadings, StopwatchText, CalcPrompt, FormulaText, SpringText;
    public TextMeshProUGUI QuestionExplanationText, OptAText, OptBText, OptCText, OptDText;
    public Image ProgressBar;
    public GameObject IntroPanel, ObjectivePanel, InstructionBar, EquipmentPanel, LaboratoryPanel;
    public GameObject DataTablePanel, GraphPanel, QuestionPanel, ConclusionPanel, ResultPanel, MatchingPanel;
    public GameObject CalcPanel, ActionRow, ForceRow, MassRow, FrictionRow, ThirdObsPanel;
    public GameObject ResetConfirm, FeedbackPanel, CardPrefab, DotPrefab, QuestionExplanationPanel;
    public GameObject NumericGroup, OptionsGroup;
    public GameObject TrackVisual, RulerVisual, PulleyVisual, StringVisual, HangerVisual, BalloonObj, StrawVisual;
    public GameObject ActionArrow, ReactionArrow, AppliedArrow, NetArrow, ResistArrow, SpringVisual, Pointer;
    public Transform CardContainer, RequiredArea;
    public RectTransform ForceGraphArea, MassGraphArea, TrackArea, EquipmentTray, CarRect;
    public Button StartBtn, Next, Reset, Retry, ResetYes, ResetNo, ViewProfileBtn, ViewResultsBtn;
    public Button StartExpBtn, StopExpBtn, ResetRunBtn, RecordBtn, ActionBtn;
    public Button Force1, Force2, Force3, Force4, Force5;
    public Button Mass05, Mass10, Mass15, Mass20, Mass40;
    public Button FrictionLow, FrictionHigh, InflateBtn, HangBtn;
    public Button SetupA, SetupB, SetupC, SetupD;
    public Button ObsRest, ObsMove, WrongObs, ExplainBtn;
    public Button Weight05, Weight10, Weight20, RecordWeightBtn, CheckCalcBtn;
    public Button QuestionA, QuestionB, QuestionC, QuestionD, QuestionContinue, NumericSubmit;
    public Button MatchC1, MatchC2, MatchC3, MatchC4, MatchM1, MatchM2, MatchM3, MatchM4;
    public TMP_InputField NumericInput, CalcInput;
    public CanvasGroup FeedbackGroup;
    public GameObject ResultBanner;
    public TextMeshProUGUI ResultBannerText;
    public GameObject WeightStage, WeightSpring, WeightObjectVisual;
    public TextMeshProUGUI WeightReadingText;
}

public class NewtonAdaptiveGrid : MonoBehaviour
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

public class NewtonAdaptiveLab : MonoBehaviour
{
    private RectTransform tray;
    private ScrollRect trayScroll;
    private RectTransform trayItems;
    private RectTransform trackHost;
    private RectTransform side;
    private RectTransform action;
    private RectTransform obs;
    private RectTransform force;
    private RectTransform mass;
    private RectTransform friction;
    private RectTransform calc;
    private GridLayoutGroup actionGrid;
    private GridLayoutGroup obsGrid;
    private int lastW;
    private int lastH;

    public void Bind(
        RectTransform trayRt,
        ScrollRect scroll,
        RectTransform items,
        RectTransform trackRt,
        RectTransform sideRt,
        RectTransform actionRt,
        RectTransform obsRt,
        RectTransform forceRt,
        RectTransform massRt,
        RectTransform frictionRt,
        RectTransform calcRt)
    {
        tray = trayRt;
        trayScroll = scroll;
        trayItems = items;
        trackHost = trackRt;
        side = sideRt;
        action = actionRt;
        obs = obsRt;
        force = forceRt;
        mass = massRt;
        friction = frictionRt;
        calc = calcRt;
        if (action != null) actionGrid = action.GetComponent<GridLayoutGroup>();
        if (obs != null) obsGrid = obs.GetComponent<GridLayoutGroup>();
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
            Set(trackHost, 0.01f, 0.46f, 0.99f, 0.99f);
            Set(side, 0.01f, 0.30f, 0.99f, 0.45f);
            Set(action, 0.01f, 0.17f, 0.99f, 0.29f);
            Set(obs, 0.01f, 0.17f, 0.99f, 0.29f);
            Set(force, 0.01f, 0.17f, 0.66f, 0.29f);
            Set(mass, 0.67f, 0.17f, 0.99f, 0.29f);
            Set(friction, 0.01f, 0.17f, 0.40f, 0.29f);
            Set(calc, 0.01f, 0.17f, 0.99f, 0.29f);
            Set(tray, 0.01f, 0.01f, 0.99f, 0.16f);
            SetTrayHorizontal(true);
        }
        else
        {
            Set(tray, 0.008f, 0.01f, 0.20f, 0.99f);
            Set(trackHost, 0.21f, 0.23f, 0.735f, 0.99f);
            Set(side, 0.745f, 0.23f, 0.992f, 0.99f);
            Set(action, 0.21f, 0.075f, 0.992f, 0.145f);
            Set(obs, 0.21f, 0.008f, 0.992f, 0.068f);
            Set(force, 0.21f, 0.155f, 0.73f, 0.22f);
            Set(mass, 0.74f, 0.155f, 0.992f, 0.22f);
            Set(friction, 0.008f, 0.155f, 0.20f, 0.22f);
            Set(calc, 0.21f, 0.01f, 0.992f, 0.09f);
            SetTrayHorizontal(false);
        }

        FitGrid(actionGrid, action, 70f, 28f, 40f);
        FitGrid(obsGrid, obs, 70f, 26f, 36f);
    }

    private static void FitGrid(GridLayoutGroup grid, RectTransform host, float minCellW, float minH, float maxH)
    {
        if (grid == null || host == null) return;
        float aw = host.rect.width;
        int cols = aw < 280 ? 2 : aw < 520 ? 3 : 4;
        float pad = grid.padding.left + grid.padding.right;
        float space = grid.spacing.x * (cols - 1);
        float cellW = Mathf.Max(minCellW, (aw - pad - space) / cols);
        float cellH = Mathf.Clamp(host.rect.height - grid.padding.top - grid.padding.bottom, minH, maxH);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = cols;
        grid.cellSize = new Vector2(cellW, cellH);
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
