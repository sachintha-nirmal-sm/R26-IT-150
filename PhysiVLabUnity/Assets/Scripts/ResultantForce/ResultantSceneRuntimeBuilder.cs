using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

[DefaultExecutionOrder(-50)]
public class ResultantSceneRuntimeBuilder : MonoBehaviour
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
            Debug.LogWarning("Resultant Force: wiring failed.");
            return;
        }
        referencesWired = true;
    }

    private void Awake()
    {
        if (GetComponent<ResultantFailsafeDisplay>() == null)
            gameObject.AddComponent<ResultantFailsafeDisplay>();
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
            Debug.Log("Resultant Force practical UI built.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Resultant Force BUILD FAILED: " + ex.Message + "\n" + ex.StackTrace);
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
            Debug.LogWarning("Resultant Force: font load skipped — " + ex.Message);
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
            Debug.LogWarning("Resultant Force EventSystem: " + ex.Message);
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
        cam.backgroundColor = new Color(0.88f, 0.92f, 0.97f);
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
        AddMgr<ResultantExperimentManager>("ResultantExperimentManager");
        AddMgr<ResultantUIManager>("ResultantUIManager");
        AddMgr<ResultantScoreManager>("ResultantScoreManager");
        AddMgr<ResultantFeedbackManager>("ResultantFeedbackManager");
        AddMgr<ResultantAttemptManager>("ResultantAttemptManager");
        AddMgr<ResultantSaveManager>("ResultantSaveManager");
        AddMgr<ResultantProfileManager>("ResultantProfileManager");
        AddMgr<ResultantEquipmentSelectionManager>("ResultantEquipmentSelectionManager");
        AddMgr<ResultantEquipmentSnapController>("ResultantEquipmentSnapController");
        AddMgr<ResultantEquipmentManager>("ResultantEquipmentManager");
        AddMgr<ResultantAssemblyManager>("ResultantAssemblyManager");
        AddMgr<ResultantForceController>("ResultantForceController");
        AddMgr<ResultantVisualController>("ResultantVisualController");
        AddMgr<ResultantTrialManager>("ResultantTrialManager");
        AddMgr<ResultantObservationTableManager>("ResultantObservationTableManager");
        AddMgr<ResultantGraphController>("ResultantGraphController");
        AddMgr<ResultantQuestionManager>("ResultantQuestionManager");
        AddMgr<ResultantConclusionManager>("ResultantConclusionManager");
        AddMgr<ResultantVariableMatchingManager>("ResultantVariableMatchingManager");
        AddMgr<ResultantResultManager>("ResultantResultManager");
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
        Color header = new Color(0.10f, 0.22f, 0.38f);
        Color accent = new Color(0.18f, 0.48f, 0.72f);
        Color green = new Color(0.12f, 0.62f, 0.35f);

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

        Panel(canvasObj.transform, "ScreenBg", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.93f, 0.95f, 0.98f));

        var headerP = Panel(canvasObj.transform, "Header", new Vector2(0, 0.91f), Vector2.one, Vector2.zero, Vector2.zero, header);
        var title = Text("Title", headerP.transform, "RESULTANT FORCE    Activity 1: Two forces in the same direction", 28, TextAlignmentOptions.MidlineLeft, new Vector2(16, -6), new Vector2(1200, 44));
        title.color = Color.white;
        title.enableAutoSizing = true;
        title.fontSizeMin = 14;
        title.fontSizeMax = 28;
        title.overflowMode = TextOverflowModes.Ellipsis;
        var score = Text("Score", headerP.transform, "SCORE: 0/100", 24, TextAlignmentOptions.MidlineRight, new Vector2(-16, -6), new Vector2(340, 44), Vector2.one, Vector2.one);
        score.color = Color.white;
        score.enableAutoSizing = true;
        score.fontSizeMin = 12;
        score.fontSizeMax = 24;
        var progress = Text("Progress", headerP.transform, "Step: 1 / 12", 20, TextAlignmentOptions.MidlineLeft, new Vector2(16, -46), new Vector2(220, 32));
        progress.color = new Color(0.75f, 0.88f, 1f);
        var stepLabel = Text("StepLabel", headerP.transform, "Introduction", 20, TextAlignmentOptions.Center, new Vector2(0, -46), new Vector2(280, 32), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        stepLabel.color = new Color(0.75f, 0.88f, 1f);
        stepLabel.enableAutoSizing = true;
        stepLabel.fontSizeMin = 12;
        stepLabel.fontSizeMax = 20;
        stepLabel.overflowMode = TextOverflowModes.Ellipsis;
        var attempts = Text("Attempts", headerP.transform, "ATTEMPTS REMAINING: 3", 20, TextAlignmentOptions.MidlineRight, new Vector2(-16, -46), new Vector2(340, 32), Vector2.one, Vector2.one);
        attempts.color = new Color(0.75f, 0.88f, 1f);
        attempts.enableAutoSizing = true;
        attempts.fontSizeMin = 12;
        attempts.fontSizeMax = 20;
        var progressBarBg = Panel(headerP.transform, "ProgressBarBg", new Vector2(0.30f, 0.06f), new Vector2(0.70f, 0.18f), Vector2.zero, Vector2.zero, new Color(0.05f, 0.10f, 0.18f, 0.5f));
        var progressBarFill = Panel(progressBarBg.transform, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.35f, 0.72f, 0.95f));
        var fillImg = progressBarFill.GetComponent<Image>();
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 0.05f;

        var instructionBar = Panel(canvasObj.transform, "InstructionBar", new Vector2(0, 0.83f), new Vector2(1, 0.91f), Vector2.zero, Vector2.zero, new Color(0.88f, 0.95f, 1f));
        instructionBar.SetActive(false);
        var instruction = StretchText("Instruction", instructionBar.transform, "Follow the instructions.", 24, TextAlignmentOptions.MidlineLeft, new Color(0.12f, 0.16f, 0.22f), 14);
        instruction.enableAutoSizing = true;
        instruction.fontSizeMin = 14;
        instruction.fontSizeMax = 26;
        instruction.overflowMode = TextOverflowModes.Ellipsis;

        var bottom = Panel(canvasObj.transform, "BottomBar", Vector2.zero, new Vector2(1, 0.11f), Vector2.zero, Vector2.zero, new Color(0.10f, 0.18f, 0.30f));
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
        var introText = StretchText("IntroText", introPanel.transform, "", 34, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 28);
        var startBtn = BigBtn("StartBtn", introPanel.transform, "START PRACTICAL", new Vector2(0.5f, 0.08f), new Vector2(0, 0), new Vector2(480, 96), green);
        introPanel.AddComponent<ResultantIntroClickToStart>();

        var objectivePanel = Panel(main.transform, "ObjectivePanel", Vector2.zero, Vector2.one, new Vector2(36, 24), new Vector2(-36, -24), Color.white);
        objectivePanel.SetActive(false);
        var objectiveText = StretchText("ObjectiveText", objectivePanel.transform, "", 34, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 28);

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
        equipHint.color = new Color(0.18f, 0.28f, 0.42f);
        equipHint.enableAutoSizing = true;
        equipHint.fontSizeMin = 12;
        equipHint.fontSizeMax = 20;
        equipHint.overflowMode = TextOverflowModes.Ellipsis;

        var requiredAreaHost = Panel(equipPanel.transform, "RequiredArea", new Vector2(0, 0), new Vector2(1, 0.24f), new Vector2(8, 6), new Vector2(-8, -4), new Color(0.90f, 0.95f, 1f));
        var reqLabel = StretchText("ReqLabel", requiredAreaHost.transform, "REQUIRED EQUIPMENT", 16, TextAlignmentOptions.MidlineLeft, new Color(0.10f, 0.22f, 0.38f), 6);
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
        requiredArea.AddComponent<ResultantUIDropTarget>().Configure("RequiredEquipment", "Any", Vector2.zero);

        var scroll = CreateScrollAnchored(equipPanel.transform, new Vector2(0f, 0.25f), new Vector2(1f, 0.85f));
        var scrollRt = scroll.GetComponent<RectTransform>();
        scrollRt.offsetMin = new Vector2(8, 4);
        scrollRt.offsetMax = new Vector2(-8, -4);
        var cardPrefab = CreateCardPrefab();
        var equipContinue = BigBtn("EquipContinueBtn", equipPanel.transform, "NEXT STEP", new Vector2(1f, 0f), new Vector2(-160, 18), new Vector2(240, 48), green);
        equipContinue.gameObject.SetActive(false);

        var lab = BuildLaboratory(main.transform, accent, green);
        var dataTablePanel = MakeTextPanel(main.transform, "DataTablePanel", "TableTitle", "OBSERVATION TABLE", "DataTableText");
        var compareBits = BuildComparePanel(main.transform, green);
        var graphPanel = BuildGraphPanel(main.transform, accent);
        var conclusionBits = BuildConclusionPanel(main.transform, green);
        var questionBits = BuildQuestionPanel(main.transform, header);
        var variableBits = BuildVariablePanel(main.transform, green);
        var resultBits = BuildResultPanel(main.transform, accent);

        var feedbackPanel = Panel(canvasObj.transform, "FeedbackPanel", new Vector2(0.18f, 0.38f), new Vector2(0.82f, 0.64f), Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.97f));
        feedbackPanel.SetActive(false);
        var feedbackGroup = feedbackPanel.AddComponent<CanvasGroup>();
        var feedbackText = StretchText("FeedbackText", feedbackPanel.transform, "", 22, TextAlignmentOptions.Center, new Color(0.14f, 0.32f, 0.6f), 14);
        var scoreChangeText = Text("ScoreChange", feedbackPanel.transform, "", 20, TextAlignmentOptions.Bottom, new Vector2(0, 8), new Vector2(220, 28), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));

        var resetConfirm = ConfirmDialog(canvasObj.transform, "ResetConfirm", "Are you sure you want to restart the practical?");
        var retryConfirm = ConfirmDialog(canvasObj.transform, "RetryConfirm", "Are you sure you want to retry this practical?");

        var refs = canvasObj.AddComponent<ResultantUIRefs>();
        refs.UiVersion = 2;
        refs.Title = title; refs.Score = score; refs.Progress = progress; refs.Attempts = attempts;
        refs.StepLabel = stepLabel; refs.ProgressBar = fillImg; refs.Instruction = instruction;
        refs.IntroPanel = introPanel; refs.IntroText = introText; refs.StartBtn = startBtn;
        refs.ObjectivePanel = objectivePanel; refs.ObjectiveText = objectiveText;
        refs.InstructionBar = instructionBar; refs.EquipmentPanel = equipPanel;
        refs.LaboratoryPanel = lab.panel;
        refs.DataTablePanel = dataTablePanel;
        refs.DataTableText = dataTablePanel.transform.Find("DataTableText")?.GetComponent<TextMeshProUGUI>();
        refs.ComparePanel = compareBits.panel;
        refs.CompareText = compareBits.text;
        refs.CompareA = compareBits.a; refs.CompareB = compareBits.b; refs.CompareC = compareBits.c;
        refs.GraphPanel = graphPanel.panel;
        refs.AreaGraph = graphPanel.area;
        refs.ForceGraph = graphPanel.force;
        refs.DotPrefab = graphPanel.dot;
        refs.QuestionPanel = questionBits.panel;
        refs.QuestionText = questionBits.questionText;
        refs.QuestionA = questionBits.a; refs.QuestionB = questionBits.b; refs.QuestionC = questionBits.c; refs.QuestionD = questionBits.d;
        refs.QuestionContinue = questionBits.cont;
        refs.QuestionExplanationPanel = questionBits.explain;
        refs.QuestionExplanationText = questionBits.explainText;
        refs.OptAText = questionBits.optA; refs.OptBText = questionBits.optB; refs.OptCText = questionBits.optC; refs.OptDText = questionBits.optD;
        refs.OptionsGroup = questionBits.optionsGroup;
        refs.ConclusionPanel = conclusionBits.panel;
        refs.ConclusionText = conclusionBits.body;
        refs.ConclusionPreview = conclusionBits.preview;
        refs.PhraseButtons = conclusionBits.phrases;
        refs.VariablePanel = variableBits.panel;
        refs.VariableContinue = variableBits.continueBtn;
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
        refs.RecordBtn = lab.record; refs.ConfirmSetupBtn = lab.confirm; refs.ResetRunBtn = lab.resetRun;
        refs.ForceBPlus = lab.bPlus; refs.ForceBMinus = lab.bMinus;
        refs.ForceCPlus = lab.cPlus; refs.ForceCMinus = lab.cMinus;
        refs.Tray = lab.tray;
        refs.TrolleyVisual = lab.trolley; refs.RingVisual = lab.ring; refs.StringsVisual = lab.strings;
        refs.Pulley1Visual = lab.pulley1; refs.Pulley2Visual = lab.pulley2;
        refs.BalanceAVisual = lab.balanceA; refs.BalanceBVisual = lab.balanceB; refs.BalanceCVisual = lab.balanceC;
        refs.WallVisual = lab.wall;
        refs.ReadingA = lab.readingA; refs.ReadingB = lab.readingB; refs.ReadingC = lab.readingC;
        refs.ArrowA = lab.arrowA; refs.ArrowB = lab.arrowB; refs.ArrowC = lab.arrowC;
        refs.FillA = lab.fillA; refs.FillB = lab.fillB; refs.FillC = lab.fillC;

        instructionBar.transform.SetSiblingIndex(headerP.transform.GetSiblingIndex() + 1);
        canvasObj.transform.Find("BottomBar")?.SetAsLastSibling();
        feedbackPanel.transform.SetAsLastSibling();
        resetConfirm.panel.transform.SetAsLastSibling();
        retryConfirm.panel.transform.SetAsLastSibling();
    }

    private bool WireReferences(bool showWelcome)
    {
        var refs = Object.FindAnyObjectByType<ResultantUIRefs>();
        if (refs == null || ResultantUIManager.Instance == null) return false;
        ResultantFeedbackManager.Instance?.BindUI(refs.FeedbackPanel, refs.FeedbackText, refs.ScoreChangeText, refs.FeedbackGroup);
        ResultantUIManager.Instance.BindAll(refs, showWelcome);
        ResultantEquipmentSelectionManager.Instance?.SetupUI(refs.CardContainer, refs.RequiredArea, refs.CardPrefab);
        ResultantObservationTableManager.Instance?.Bind(refs.DataTableText);
        ResultantGraphController.Instance?.Bind(refs.AreaGraph, refs.ForceGraph, refs.DotPrefab);
        ResultantResultManager.Instance?.Bind(refs.FinalScore, refs.ResultDetails, refs.StatusText);
        ResultantEquipmentManager.Instance?.Bind(refs.Tray);
        ResultantVariableMatchingManager.Instance?.Bind(refs.VariablePanel != null ? refs.VariablePanel.transform : null);
        ResultantConclusionManager.Instance?.BindPhrases(refs.PhraseButtons);
        ResultantVisualController.Instance?.Bind(
            refs.TrolleyVisual, refs.RingVisual, refs.StringsVisual,
            refs.Pulley1Visual, refs.Pulley2Visual,
            refs.BalanceAVisual, refs.BalanceBVisual, refs.BalanceCVisual, refs.WallVisual,
            refs.ReadingA, refs.ReadingB, refs.ReadingC,
            refs.ArrowA, refs.ArrowB, refs.ArrowC,
            refs.FillA, refs.FillB, refs.FillC);
        ResultantFailsafeDisplay.Hide();
        return refs.StartBtn != null;
    }

    private LabBits BuildLaboratory(Transform parent, Color accent, Color green)
    {
        var labPanel = Panel(parent, "LaboratoryPanel", Vector2.zero, Vector2.one, new Vector2(8, 4), new Vector2(-8, -4), new Color(0, 0, 0, 0), false);
        labPanel.SetActive(false);

        var tray = Panel(labPanel.transform, "EquipmentTray", new Vector2(0.008f, 0.16f), new Vector2(0.20f, 0.99f), Vector2.zero, Vector2.zero, new Color(0.93f, 0.95f, 0.98f, 1f));
        var trayHeader = Panel(tray.transform, "TrayHeader", new Vector2(0f, 0.90f), Vector2.one, Vector2.zero, Vector2.zero, new Color(0.10f, 0.22f, 0.38f));
        StretchText("TrayLabel", trayHeader.transform, "EQUIPMENT\nDrag onto the table", 24, TextAlignmentOptions.Center, Color.white, 6).fontStyle = FontStyles.Bold;
        var scrollObj = Panel(tray.transform, "Scroll", new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.88f), Vector2.zero, Vector2.zero, new Color(0.93f, 0.95f, 0.98f, 0.2f));
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

        var host = Panel(labPanel.transform, "ExperimentHost", new Vector2(0.21f, 0.16f), new Vector2(0.735f, 0.99f), Vector2.zero, Vector2.zero, new Color(0.86f, 0.91f, 0.96f));
        Text("ExpTitle", host.transform, "EXPERIMENT AREA  —  two forces in the same direction", 22, TextAlignmentOptions.MidlineLeft, new Vector2(14, -8), new Vector2(700, 36));

        var table = Panel(host.transform, "Table", new Vector2(0.04f, 0.38f), new Vector2(0.96f, 0.52f), Vector2.zero, Vector2.zero, new Color(0.55f, 0.40f, 0.24f));
        table.AddComponent<ResultantUIDropTarget>().Configure("Table", "Trolley", Vector2.zero);
        StretchText("TableLabel", table.transform, "TABLE", 22, TextAlignmentOptions.Bottom, Color.white, 4);

        var wall = Panel(host.transform, "Wall", new Vector2(0.02f, 0.38f), new Vector2(0.08f, 0.88f), Vector2.zero, Vector2.zero, new Color(0.72f, 0.74f, 0.78f));
        StretchText("WallLbl", wall.transform, "WALL", 16, TextAlignmentOptions.Center, new Color(0.2f, 0.22f, 0.26f), 2);

        var aZone = Panel(host.transform, "BalanceAZone", new Vector2(0.08f, 0.52f), new Vector2(0.26f, 0.78f), Vector2.zero, Vector2.zero, new Color(0.62f, 0.48f, 0.28f, 0.35f));
        aZone.AddComponent<ResultantUIDropTarget>().Configure("BalanceAZone", "BalanceA", Vector2.zero);
        var balanceA = MakeBalanceVisual(aZone.transform, "BalanceAVisual", "A");
        balanceA.SetActive(false);

        var trolleyZone = Panel(host.transform, "TrolleyZone", new Vector2(0.26f, 0.50f), new Vector2(0.58f, 0.78f), Vector2.zero, Vector2.zero, new Color(0.22f, 0.48f, 0.82f, 0.25f));
        trolleyZone.AddComponent<ResultantUIDropTarget>().Configure("TrolleyZone", "Trolley", Vector2.zero);
        var trolley = Panel(trolleyZone.transform, "TrolleyVisual", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
        trolley.GetComponent<Image>().sprite = ResultantIconFactory.GetNamed("trolley");
        trolley.GetComponent<Image>().preserveAspect = true;
        trolley.SetActive(false);

        var ringZone = Panel(host.transform, "RingZone", new Vector2(0.56f, 0.58f), new Vector2(0.66f, 0.74f), Vector2.zero, Vector2.zero, new Color(0.72f, 0.74f, 0.78f, 0.4f));
        ringZone.AddComponent<ResultantUIDropTarget>().Configure("RingZone", "Ring|Strings", Vector2.zero);
        var ring = Panel(ringZone.transform, "RingVisual", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
        ring.GetComponent<Image>().sprite = ResultantIconFactory.GetNamed("ring");
        ring.GetComponent<Image>().preserveAspect = true;
        ring.SetActive(false);

        var stringZone = Panel(host.transform, "StringZone", new Vector2(0.64f, 0.62f), new Vector2(0.86f, 0.72f), Vector2.zero, Vector2.zero, new Color(0.85f, 0.72f, 0.35f, 0.35f));
        stringZone.AddComponent<ResultantUIDropTarget>().Configure("StringZone", "Strings", Vector2.zero);
        var strings = Panel(stringZone.transform, "StringsVisual", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
        strings.GetComponent<Image>().sprite = ResultantIconFactory.GetNamed("strings");
        strings.GetComponent<Image>().preserveAspect = true;
        strings.SetActive(false);

        var p1 = Panel(host.transform, "PulleyZone1", new Vector2(0.78f, 0.58f), new Vector2(0.90f, 0.78f), Vector2.zero, Vector2.zero, new Color(0.18f, 0.62f, 0.38f, 0.3f));
        p1.AddComponent<ResultantUIDropTarget>().Configure("PulleyZone1", "Pulley1|BalanceB", Vector2.zero);
        var pulley1 = Panel(p1.transform, "Pulley1Visual", new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.95f), Vector2.zero, Vector2.zero, Color.white);
        pulley1.GetComponent<Image>().sprite = ResultantIconFactory.GetNamed("pulley");
        pulley1.GetComponent<Image>().preserveAspect = true;
        pulley1.SetActive(false);

        var p2 = Panel(host.transform, "PulleyZone2", new Vector2(0.86f, 0.42f), new Vector2(0.98f, 0.62f), Vector2.zero, Vector2.zero, new Color(0.18f, 0.62f, 0.38f, 0.3f));
        p2.AddComponent<ResultantUIDropTarget>().Configure("PulleyZone2", "Pulley2|BalanceC", Vector2.zero);
        var pulley2 = Panel(p2.transform, "Pulley2Visual", new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.95f), Vector2.zero, Vector2.zero, Color.white);
        pulley2.GetComponent<Image>().sprite = ResultantIconFactory.GetNamed("pulley");
        pulley2.GetComponent<Image>().preserveAspect = true;
        pulley2.SetActive(false);

        var hangB = Panel(host.transform, "HangB", new Vector2(0.78f, 0.08f), new Vector2(0.90f, 0.48f), Vector2.zero, Vector2.zero, new Color(0.62f, 0.48f, 0.28f, 0.25f));
        hangB.AddComponent<ResultantUIDropTarget>().Configure("HangB", "BalanceB", Vector2.zero);
        var balanceB = MakeBalanceVisual(hangB.transform, "BalanceBVisual", "B");
        balanceB.SetActive(false);

        var hangC = Panel(host.transform, "HangC", new Vector2(0.86f, 0.02f), new Vector2(0.98f, 0.40f), Vector2.zero, Vector2.zero, new Color(0.62f, 0.48f, 0.28f, 0.25f));
        hangC.AddComponent<ResultantUIDropTarget>().Configure("HangC", "BalanceC", Vector2.zero);
        var balanceC = MakeBalanceVisual(hangC.transform, "BalanceCVisual", "C");
        balanceC.SetActive(false);

        var arrowA = Panel(host.transform, "ArrowA", new Vector2(0.18f, 0.82f), new Vector2(0.18f, 0.82f), Vector2.zero, new Vector2(110, 26), new Color(0.75f, 0.22f, 0.18f));
        arrowA.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);
        StretchText("ATxt", arrowA.transform, "← A", 16, TextAlignmentOptions.Center, Color.white, 2);
        var arrowB = Panel(host.transform, "ArrowB", new Vector2(0.62f, 0.86f), new Vector2(0.62f, 0.86f), Vector2.zero, new Vector2(90, 22), new Color(0.18f, 0.48f, 0.82f));
        arrowB.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
        StretchText("BTxt", arrowB.transform, "B →", 16, TextAlignmentOptions.Center, Color.white, 2);
        var arrowC = Panel(host.transform, "ArrowC", new Vector2(0.62f, 0.80f), new Vector2(0.62f, 0.80f), Vector2.zero, new Vector2(90, 22), new Color(0.12f, 0.62f, 0.35f));
        arrowC.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
        StretchText("CTxt", arrowC.transform, "C →", 16, TextAlignmentOptions.Center, Color.white, 2);

        var readingA = balanceA.transform.Find("Reading")?.GetComponent<TextMeshProUGUI>();
        var readingB = balanceB.transform.Find("Reading")?.GetComponent<TextMeshProUGUI>();
        var readingC = balanceC.transform.Find("Reading")?.GetComponent<TextMeshProUGUI>();
        var fillA = balanceA.transform.Find("ScaleBg/Fill")?.GetComponent<Image>();
        var fillB = balanceB.transform.Find("ScaleBg/Fill")?.GetComponent<Image>();
        var fillC = balanceC.transform.Find("ScaleBg/Fill")?.GetComponent<Image>();

        var side = Panel(labPanel.transform, "SidePanel", new Vector2(0.745f, 0.16f), new Vector2(0.992f, 0.99f), Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.98f));
        var live = StretchText("LiveReadings", side.transform, "Balance A: 0.0 N", 26, TextAlignmentOptions.TopLeft, new Color(0.12f, 0.16f, 0.22f), 10);
        live.fontStyle = FontStyles.Bold;
        live.enableAutoSizing = true;
        live.fontSizeMin = 14;
        live.fontSizeMax = 24;
        var liveRt = live.rectTransform;
        liveRt.anchorMin = new Vector2(0.06f, 0.50f);
        liveRt.anchorMax = new Vector2(0.94f, 0.98f);
        var physics = StretchText("PhysicsText", side.transform, "", 20, TextAlignmentOptions.TopLeft, new Color(0.18f, 0.22f, 0.28f), 10);
        physics.enableAutoSizing = true;
        physics.fontSizeMin = 12;
        physics.fontSizeMax = 18;
        var pRt = physics.rectTransform;
        pRt.anchorMin = new Vector2(0.06f, 0.02f);
        pRt.anchorMax = new Vector2(0.94f, 0.48f);

        var actionRow = Panel(labPanel.transform, "ActionRow", new Vector2(0.21f, 0.01f), new Vector2(0.992f, 0.15f), Vector2.zero, Vector2.zero, new Color(0.10f, 0.18f, 0.30f));
        var aLayout = actionRow.AddComponent<GridLayoutGroup>();
        aLayout.cellSize = new Vector2(140, 36);
        aLayout.spacing = new Vector2(6, 4);
        aLayout.padding = new RectOffset(6, 6, 4, 4);
        aLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        aLayout.constraintCount = 4;
        aLayout.childAlignment = TextAnchor.MiddleCenter;
        var confirm = Btn("Confirm", actionRow.transform, "CONFIRM SETUP", green, 0, 36);
        var bMinus = Btn("BMinus", actionRow.transform, "B −", accent, 0, 36);
        var bPlus = Btn("BPlus", actionRow.transform, "B +", accent, 0, 36);
        var cMinus = Btn("CMinus", actionRow.transform, "C −", green, 0, 36);
        var cPlus = Btn("CPlus", actionRow.transform, "C +", green, 0, 36);
        var record = Btn("Record", actionRow.transform, "RECORD READING", green, 0, 36);
        var resetRun = Btn("ResetRun", actionRow.transform, "RESET TRIAL", new Color(0.45f, 0.48f, 0.52f), 0, 36);

        var adaptive = labPanel.AddComponent<ResultantAdaptiveLab>();
        adaptive.Bind(
            tray.GetComponent<RectTransform>(),
            scroll,
            trayInnerRt,
            host.GetComponent<RectTransform>(),
            side.GetComponent<RectTransform>(),
            actionRow.GetComponent<RectTransform>());

        return new LabBits
        {
            panel = labPanel,
            live = live,
            physics = physics,
            record = record,
            confirm = confirm,
            resetRun = resetRun,
            bPlus = bPlus,
            bMinus = bMinus,
            cPlus = cPlus,
            cMinus = cMinus,
            tray = trayInner.transform,
            trolley = trolley,
            ring = ring,
            strings = strings,
            pulley1 = pulley1,
            pulley2 = pulley2,
            balanceA = balanceA,
            balanceB = balanceB,
            balanceC = balanceC,
            wall = wall,
            readingA = readingA,
            readingB = readingB,
            readingC = readingC,
            arrowA = arrowA.GetComponent<RectTransform>(),
            arrowB = arrowB.GetComponent<RectTransform>(),
            arrowC = arrowC.GetComponent<RectTransform>(),
            fillA = fillA,
            fillB = fillB,
            fillC = fillC
        };
    }

    private GameObject MakeBalanceVisual(Transform parent, string name, string letter)
    {
        var balance = Panel(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
        balance.GetComponent<Image>().sprite = ResultantIconFactory.GetNamed("balance");
        balance.GetComponent<Image>().preserveAspect = true;
        var scaleBg = Panel(balance.transform, "ScaleBg", new Vector2(0.55f, 0.15f), new Vector2(0.92f, 0.70f), Vector2.zero, Vector2.zero, new Color(0.92f, 0.94f, 0.96f));
        var scaleFill = Panel(scaleBg.transform, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.85f, 0.45f, 0.15f));
        var fill = scaleFill.GetComponent<Image>();
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Vertical;
        fill.fillAmount = 0f;
        StretchText("Reading", balance.transform, letter + " 0.0 N", 18, TextAlignmentOptions.Top, new Color(0.12f, 0.16f, 0.22f), 2);
        return balance;
    }

    private CompareBits BuildComparePanel(Transform parent, Color green)
    {
        var panel = Panel(parent, "ComparePanel", Vector2.zero, Vector2.one, new Vector2(36, 24), new Vector2(-36, -24), Color.white);
        panel.SetActive(false);
        var compareText = StretchText("CompareText", panel.transform, "", 30, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 20);
        var row = Panel(panel.transform, "Choices", new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.22f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 16;
        layout.childForceExpandWidth = true;
        var a = Btn("A", row.transform, "A", new Color(0.75f, 0.25f, 0.2f), 0, 56);
        var b = Btn("B", row.transform, "B", new Color(0.75f, 0.45f, 0.15f), 0, 56);
        var c = Btn("C", row.transform, "C", green, 0, 56);
        return new CompareBits { panel = panel, text = compareText, a = a, b = b, c = c };
    }

    private GraphBits BuildGraphPanel(Transform parent, Color accent)
    {
        var panel = Panel(parent, "GraphPanel", Vector2.zero, Vector2.one, new Vector2(20, 12), new Vector2(-20, -12), Color.white);
        panel.SetActive(false);
        StretchText("GTitle", panel.transform, "FORCE A, B, C PER TRIAL     •     A compared with B + C", 22, TextAlignmentOptions.Top, new Color(0.12f, 0.18f, 0.28f), 8).fontStyle = FontStyles.Bold;
        var area = Panel(panel.transform, "AreaGraph", new Vector2(0.03f, 0.52f), new Vector2(0.97f, 0.92f), Vector2.zero, Vector2.zero, new Color(0.95f, 0.96f, 0.98f));
        var force = Panel(panel.transform, "ForceGraph", new Vector2(0.03f, 0.06f), new Vector2(0.97f, 0.48f), Vector2.zero, Vector2.zero, new Color(0.95f, 0.96f, 0.98f));
        var dot = Panel(panel.transform, "DotPrefab", Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(10, 10), accent);
        dot.SetActive(false);
        return new GraphBits { panel = panel, area = area.GetComponent<RectTransform>(), force = force.GetComponent<RectTransform>(), dot = dot };
    }

    private ConclusionBits BuildConclusionPanel(Transform parent, Color green)
    {
        var panel = Panel(parent, "ConclusionPanel", Vector2.zero, Vector2.one, new Vector2(36, 16), new Vector2(-36, -16), Color.white);
        panel.SetActive(false);
        var body = StretchText("ConclusionText", panel.transform, "", 24, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 16);
        var preview = Text("Preview", panel.transform, "", 18, TextAlignmentOptions.Center, Vector2.zero, new Vector2(900, 48), new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.5f));
        preview.enableAutoSizing = true;
        preview.fontSizeMin = 12;
        preview.fontSizeMax = 18;
        preview.overflowMode = TextOverflowModes.Ellipsis;
        var row = Panel(panel.transform, "Phrases", new Vector2(0.04f, 0.16f), new Vector2(0.96f, 0.38f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var layout = row.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(320, 48);
        layout.spacing = new Vector2(8, 8);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 2;
        string[] phrases =
        {
            "the two forces acting",
            "in the same direction.",
            "The resultant force",
            "is equal to the sum of"
        };
        var buttons = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            buttons[i] = Btn("P" + i, row.transform, phrases[i], green, 0, 56);
            var choice = buttons[i].gameObject.AddComponent<ResultantPhraseChoiceButton>();
            choice.Configure(phrases[i]);
        }
        var cont = BigBtn("ConclusionContinue", panel.transform, "NEXT STEP", new Vector2(0.5f, 0.06f), Vector2.zero, new Vector2(420, 78), green);
        return new ConclusionBits { panel = panel, body = body, preview = preview, phrases = buttons, continueBtn = cont };
    }

    private VariableBits BuildVariablePanel(Transform parent, Color green)
    {
        var panel = Panel(parent, "VariablePanel", Vector2.zero, Vector2.one, new Vector2(24, 8), new Vector2(-24, -8), Color.white);
        panel.SetActive(false);
        var title = Text("VTitle", panel.transform, "VARIABLE IDENTIFICATION\nTap Independent, Dependent or Controlled, then press NEXT STEP.", 26, TextAlignmentOptions.Top, new Vector2(16, -8), new Vector2(1400, 70), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f));
        title.fontStyle = FontStyles.Bold;
        title.enableAutoSizing = true;
        title.fontSizeMin = 22;
        title.fontSizeMax = 28;

        var list = Panel(panel.transform, "Rows", new Vector2(0.03f, 0.16f), new Vector2(0.97f, 0.86f), Vector2.zero, Vector2.zero, new Color(0.95f, 0.97f, 1f));
        var layout = list.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8;
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        MakeVarRow(list.transform, "ForceB", "Force B");
        MakeVarRow(list.transform, "ForceC", "Force C");
        MakeVarRow(list.transform, "ResultantA", "Resultant force A");
        MakeVarRow(list.transform, "Direction", "Direction of the two forces");
        MakeVarRow(list.transform, "Trolley", "Trolley");
        MakeVarRow(list.transform, "PulleySetup", "Pulley arrangement");
        var cont = BigBtn("VariableContinue", panel.transform, "NEXT STEP", new Vector2(0.5f, 0.06f), Vector2.zero, new Vector2(420, 78), green);
        return new VariableBits { panel = panel, continueBtn = cont };
    }

    private void MakeVarRow(Transform parent, string id, string label)
    {
        var row = new GameObject(id);
        row.transform.SetParent(parent, false);
        row.AddComponent<Image>().color = new Color(0.98f, 0.99f, 1f);
        var le = row.AddComponent<LayoutElement>();
        le.preferredHeight = 78;
        le.minHeight = 70;
        var h = row.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 8;
        h.padding = new RectOffset(10, 10, 8, 8);
        h.childAlignment = TextAnchor.MiddleLeft;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = true;
        h.childControlWidth = true;
        h.childControlHeight = true;
        var nameObj = new GameObject("Name");
        nameObj.transform.SetParent(row.transform, false);
        var nle = nameObj.AddComponent<LayoutElement>();
        nle.preferredWidth = 380;
        nle.minWidth = 220;
        nle.flexibleWidth = 1;
        var tmp = nameObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) tmp.font = defaultFont;
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(0.12f, 0.14f, 0.18f);
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;
        MakeChoiceBtn(row.transform, "Ind", "Independent", new Color(0.15f, 0.48f, 0.78f), id, "Independent");
        MakeChoiceBtn(row.transform, "Dep", "Dependent", new Color(0.72f, 0.42f, 0.16f), id, "Dependent");
        MakeChoiceBtn(row.transform, "Ctrl", "Controlled", new Color(0.12f, 0.55f, 0.32f), id, "Controlled");
    }

    private Button MakeChoiceBtn(Transform parent, string name, string label, Color color, string itemId, string zone)
    {
        var btn = Btn(name, parent, label, color, 190, 56);
        var le = btn.GetComponent<LayoutElement>();
        if (le != null)
        {
            le.minWidth = 160;
            le.preferredWidth = 190;
        }
        var choice = btn.gameObject.AddComponent<ResultantVariableChoiceButton>();
        choice.Configure(itemId, zone);
        return btn;
    }

    private QuestionBits BuildQuestionPanel(Transform parent, Color header)
    {
        var panel = Panel(parent, "QuestionPanel", Vector2.zero, Vector2.one, new Vector2(28, 10), new Vector2(-28, -10), Color.white);
        panel.SetActive(false);
        var questionText = StretchText("QuestionText", panel.transform, "", 32, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 16);
        var qRt = questionText.rectTransform;
        qRt.anchorMin = new Vector2(0.04f, 0.68f);
        qRt.anchorMax = new Vector2(0.96f, 0.97f);
        var options = Panel(panel.transform, "Options", new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.66f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var layout = options.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8;
        layout.childForceExpandHeight = false;
        var a = ChoiceBtn("A", options.transform, "A", "");
        var b = ChoiceBtn("B", options.transform, "B", "");
        var c = ChoiceBtn("C", options.transform, "C", "");
        var d = ChoiceBtn("D", options.transform, "D", "");
        var optA = a.transform.Find("Body")?.GetComponent<TextMeshProUGUI>();
        var optB = b.transform.Find("Body")?.GetComponent<TextMeshProUGUI>();
        var optC = c.transform.Find("Body")?.GetComponent<TextMeshProUGUI>();
        var optD = d.transform.Find("Body")?.GetComponent<TextMeshProUGUI>();
        var explain = Panel(panel.transform, "Explain", new Vector2(0.06f, 0.14f), new Vector2(0.94f, 0.27f), Vector2.zero, Vector2.zero, new Color(0.95f, 0.97f, 1f));
        explain.GetComponent<Image>().raycastTarget = false;
        explain.SetActive(false);
        var explainText = StretchText("ExplainText", explain.transform, "", 22, TextAlignmentOptions.Center, new Color(0.12f, 0.32f, 0.2f), 8);
        explainText.raycastTarget = false;
        var cont = BigBtn("Continue", panel.transform, "CONTINUE", new Vector2(0.5f, 0.06f), Vector2.zero, new Vector2(420, 78), header);
        cont.transform.SetAsLastSibling();
        return new QuestionBits
        {
            panel = panel, questionText = questionText, a = a, b = b, c = c, d = d, cont = cont,
            explain = explain, explainText = explainText, optA = optA, optB = optB, optC = optC, optD = optD,
            optionsGroup = options
        };
    }

    private ResultBits BuildResultPanel(Transform parent, Color accent)
    {
        var panel = Panel(parent, "ResultPanel", Vector2.zero, Vector2.one, new Vector2(28, 12), new Vector2(-28, -12), Color.white);
        panel.SetActive(false);
        var score = StretchText("FinalScore", panel.transform, "", 26, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 16);
        var sRt = score.rectTransform;
        sRt.anchorMin = new Vector2(0.04f, 0.55f);
        sRt.anchorMax = new Vector2(0.96f, 0.96f);
        var details = StretchText("ResultDetails", panel.transform, "", 20, TextAlignmentOptions.TopLeft, new Color(0.16f, 0.18f, 0.22f), 16);
        var dRt = details.rectTransform;
        dRt.anchorMin = new Vector2(0.04f, 0.16f);
        dRt.anchorMax = new Vector2(0.96f, 0.54f);
        var status = Text("Status", panel.transform, "", 24, TextAlignmentOptions.Center, Vector2.zero, new Vector2(500, 40), new Vector2(0.5f, 0.10f), new Vector2(0.5f, 0.10f), new Vector2(0.5f, 0.5f));
        var row = Panel(panel.transform, "Btns", new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.10f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 16;
        layout.childForceExpandWidth = true;
        var results = Btn("ViewResults", row.transform, "VIEW RESULTS", accent, 0, 48);
        var profile = Btn("ViewProfile", row.transform, "BACK TO PROFILE", new Color(0.32f, 0.42f, 0.55f), 0, 48);
        return new ResultBits { panel = panel, score = score, details = details, status = status, results = results, profile = profile };
    }

    private GameObject MakeTextPanel(Transform parent, string name, string titleName, string title, string bodyName)
    {
        var panel = Panel(parent, name, Vector2.zero, Vector2.one, new Vector2(28, 16), new Vector2(-28, -16), Color.white);
        panel.SetActive(false);
        Text(titleName, panel.transform, title, 28, TextAlignmentOptions.TopLeft, new Vector2(16, -10), new Vector2(800, 40)).fontStyle = FontStyles.Bold;
        var body = StretchText(bodyName, panel.transform, "", 26, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 16);
        var rt = body.rectTransform;
        rt.anchorMin = new Vector2(0.03f, 0.04f);
        rt.anchorMax = new Vector2(0.97f, 0.90f);
        return panel;
    }

    private ConfirmBits ConfirmDialog(Transform parent, string name, string message)
    {
        var panel = Panel(parent, name, new Vector2(0.28f, 0.38f), new Vector2(0.72f, 0.62f), Vector2.zero, Vector2.zero, Color.white);
        panel.SetActive(false);
        StretchText("Msg", panel.transform, message, 24, TextAlignmentOptions.Center, new Color(0.18f, 0.2f, 0.25f), 16);
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
        img.sprite = ResultantIconFactory.White();
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
        le.preferredHeight = 78; le.minHeight = 70;
        obj.AddComponent<Image>().color = Color.white;
        var btn = obj.AddComponent<Button>();
        var letterBg = Panel(obj.transform, "Letter", new Vector2(0f, 0.12f), new Vector2(0f, 0.88f), new Vector2(10, 0), new Vector2(68, 0), new Color(0.18f, 0.48f, 0.72f), false);
        StretchText("LetterText", letterBg.transform, letter, 30, TextAlignmentOptions.Center, Color.white, 0).fontStyle = FontStyles.Bold;
        var bodyObj = new GameObject("Body");
        bodyObj.transform.SetParent(obj.transform, false);
        var bodyRt = bodyObj.AddComponent<RectTransform>();
        bodyRt.anchorMin = Vector2.zero; bodyRt.anchorMax = Vector2.one;
        bodyRt.offsetMin = new Vector2(82, 8); bodyRt.offsetMax = new Vector2(-16, -8);
        var body = bodyObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) body.font = defaultFont;
        body.text = text; body.fontSize = 26; body.fontStyle = FontStyles.Bold;
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
        img.sprite = ResultantIconFactory.White();
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
        StretchText("Text", obj.transform, label, 28, TextAlignmentOptions.Center, Color.white, 8).fontStyle = FontStyles.Bold;
        return btn;
    }

    private ScrollRect CreateScrollAnchored(Transform parent, Vector2 aMin, Vector2 aMax)
    {
        var scrollObj = Panel(parent, "Scroll", aMin, aMax, Vector2.zero, Vector2.zero, new Color(0.95f, 0.97f, 1f));
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
        content.AddComponent<ResultantAdaptiveGrid>();
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
        card.AddComponent<Image>().color = new Color(0.92f, 0.95f, 1f);
        card.AddComponent<ResultantEquipmentCardUI>();
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
        public GameObject panel, trolley, ring, strings, pulley1, pulley2, balanceA, balanceB, balanceC, wall;
        public TextMeshProUGUI live, physics, readingA, readingB, readingC;
        public Button record, confirm, resetRun, bPlus, bMinus, cPlus, cMinus;
        public RectTransform arrowA, arrowB, arrowC;
        public Image fillA, fillB, fillC;
        public Transform tray;
    }

    private struct CompareBits
    {
        public GameObject panel;
        public TextMeshProUGUI text;
        public Button a, b, c;
    }

    private struct GraphBits
    {
        public GameObject panel, dot;
        public RectTransform area, force;
    }

    private struct ConclusionBits
    {
        public GameObject panel;
        public TextMeshProUGUI body, preview;
        public Button[] phrases;
        public Button continueBtn;
    }

    private struct VariableBits
    {
        public GameObject panel;
        public Button continueBtn;
    }

    private struct QuestionBits
    {
        public GameObject panel, explain, optionsGroup;
        public TextMeshProUGUI questionText, explainText, optA, optB, optC, optD;
        public Button a, b, c, d, cont;
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
}

public class ResultantUIRefs : MonoBehaviour
{
    public int UiVersion;
    public TextMeshProUGUI Title, Score, Progress, Attempts, StepLabel, Instruction;
    public TextMeshProUGUI IntroText, ObjectiveText, ConclusionText, ConclusionPreview, CompareText, QuestionText, DataTableText;
    public TextMeshProUGUI FinalScore, ResultDetails, StatusText, FeedbackText, ScoreChangeText;
    public TextMeshProUGUI LiveReadings, PhysicsText, QuestionExplanationText, OptAText, OptBText, OptCText, OptDText;
    public TextMeshProUGUI ReadingA, ReadingB, ReadingC;
    public Image ProgressBar, FillA, FillB, FillC;
    public GameObject IntroPanel, ObjectivePanel, InstructionBar, EquipmentPanel, LaboratoryPanel;
    public GameObject DataTablePanel, ComparePanel, GraphPanel, QuestionPanel, ConclusionPanel, ResultPanel, VariablePanel;
    public GameObject ResetConfirm, RetryConfirm, FeedbackPanel, CardPrefab, DotPrefab, QuestionExplanationPanel, OptionsGroup;
    public GameObject TrolleyVisual, RingVisual, StringsVisual, Pulley1Visual, Pulley2Visual;
    public GameObject BalanceAVisual, BalanceBVisual, BalanceCVisual, WallVisual;
    public Transform CardContainer, RequiredArea, Tray;
    public RectTransform AreaGraph, ForceGraph, ArrowA, ArrowB, ArrowC;
    public Button StartBtn, Next, Reset, Retry, ResetYes, ResetNo, RetryYes, RetryNo, ViewProfileBtn, ViewResultsBtn;
    public Button RecordBtn, ConfirmSetupBtn, ResetRunBtn, ForceBPlus, ForceBMinus, ForceCPlus, ForceCMinus;
    public Button CompareA, CompareB, CompareC;
    public Button QuestionA, QuestionB, QuestionC, QuestionD, QuestionContinue;
    public Button VariableContinue, ConclusionContinue;
    public Button[] PhraseButtons;
    public CanvasGroup FeedbackGroup;
}

public class ResultantAdaptiveGrid : MonoBehaviour
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

public class ResultantAdaptiveLab : MonoBehaviour
{
    private RectTransform tray;
    private ScrollRect trayScroll;
    private RectTransform trayItems;
    private RectTransform host;
    private RectTransform side;
    private RectTransform action;
    private GridLayoutGroup actionGrid;
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
        if (action != null) actionGrid = action.GetComponent<GridLayoutGroup>();
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
            Set(action, 0.21f, 0.01f, 0.992f, 0.15f);
            SetTrayHorizontal(false);
        }

        if (actionGrid == null || action == null) return;
        float aw = action.rect.width;
        int cols = aw < 280 ? 2 : aw < 520 ? 3 : 4;
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
