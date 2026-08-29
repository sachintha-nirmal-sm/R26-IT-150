using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class LeverSceneRuntimeBuilder : MonoBehaviour
{
    private static bool referencesWired;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => referencesWired = false;

    private TMP_FontAsset defaultFont;
    private Transform managersRoot;

    public void BuildScenePersistent() => BuildInternal();
    public bool HasExistingBuild() => transform.Find("Canvas") != null;

    private void Awake()
    {
        if (!Application.isPlaying) return;
        if (HasExistingBuild()) return;
        BuildInternal();
    }

    private void Start()
    {
        // Wiring deferred to LeverRuntimeBootstrap (order 250) after all manager Awakes.
    }

    public void WireReferencesOnPlay(bool showWelcome, bool force)
    {
        if (force) referencesWired = false;
        if (referencesWired) return;
        if (!WireReferences(showWelcome))
        {
            Debug.LogWarning("LeverSceneRuntimeBuilder: wiring failed — Canvas/UIRefs missing. Use Tools → Lever Practical → Build Complete Scene.");
            return;
        }
        referencesWired = true;
    }

    private void BuildInternal()
    {
        referencesWired = false;
        try
        {
            var existingCanvas = transform.Find("Canvas");
            if (existingCanvas != null)
            {
                if (Application.isPlaying) Destroy(existingCanvas.gameObject);
                else DestroyImmediate(existingCanvas.gameObject);
            }

            LoadFont();
            EnsureEventSystem();
            SetupCamera();
            CreateManagers();
            CreateUI();
            WireReferences(true);
            Debug.Log("Lever Practical: UI built successfully.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Lever Practical BUILD FAILED: " + ex.Message + "\n" + ex.StackTrace);
        }
    }

    private void LoadFont()
    {
        defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF") ?? TMP_Settings.defaultFontAsset;
#if UNITY_EDITOR
        if (defaultFont == null)
            defaultFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
#endif
    }

    private void EnsureEventSystem()
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
            else
            {
                if (Application.isPlaying) Object.Destroy(es.gameObject);
                else Object.DestroyImmediate(es.gameObject);
            }
        }

        if (keep != null) return;

        var obj = new GameObject("EventSystem");
        obj.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        obj.AddComponent<InputSystemUIInputModule>();
#else
        obj.AddComponent<StandaloneInputModule>();
#endif
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
        cam.backgroundColor = new Color(0.88f, 0.92f, 0.96f);
    }

    private void CreateManagers()
    {
        var existing = transform.Find("Managers");
        if (existing != null)
        {
            if (Application.isPlaying) Destroy(existing.gameObject);
            else DestroyImmediate(existing.gameObject);
        }

        managersRoot = new GameObject("Managers").transform;
        managersRoot.SetParent(transform, false);
        AddMgr<LeverGameManager>("LeverGameManager");
        AddMgr<LeverExperimentManager>("LeverExperimentManager");
        AddMgr<LeverUIManager>("LeverUIManager");
        AddMgr<LeverScoreManager>("LeverScoreManager");
        AddMgr<LeverFeedbackManager>("LeverFeedbackManager");
        AddMgr<LeverAttemptManager>("LeverAttemptManager");
        AddMgr<LeverSaveManager>("LeverSaveManager");
        AddMgr<LeverProfileManager>("LeverProfileManager");
        AddMgr<LeverExperimentDataManager>("LeverExperimentDataManager");
        AddMgr<LeverEquipmentSelectionManager>("LeverEquipmentSelectionManager");
        AddMgr<LeverConclusionManager>("LeverConclusionManager");
        AddMgr<LeverLabWorkbench>("LeverLabWorkbench");
        AddMgr<LeverGraphView>("LeverGraphView");
        AddMgr<LeverPhysicsController>("LeverPhysicsController");
        AddMgr<NewtonSpringBalanceController>("NewtonSpringBalanceController");
        AddMgr<LeverSpringController>("LeverSpringController");
        AddMgr<LeverPullHandleController>("LeverPullHandleController");
        AddMgr<BookLiftController>("BookLiftController");
        AddMgr<LeverWoodenStripController>("LeverWoodenStripController");
        AddMgr<LeverPivotController>("LeverPivotController");
        AddMgr<LeverMeasurementManager>("LeverMeasurementManager");
        AddMgr<LeverResultManager>("LeverResultManager");
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
        Color header = new Color(0.15f, 0.38f, 0.58f);
        Color accent = new Color(0.15f, 0.52f, 0.82f);

        var canvasObj = new GameObject("Canvas");
        canvasObj.transform.SetParent(transform, false);
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        Panel(canvasObj.transform, "ScreenBg", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.93f, 0.96f, 0.99f));

        var headerP = Panel(canvasObj.transform, "Header", new Vector2(0, 0.88f), Vector2.one, Vector2.zero, Vector2.zero, header);
        var title = Text("Title", headerP.transform, "LEVER – ACTIVITY 15.1", 42, TextAlignmentOptions.MidlineLeft, new Vector2(24, -8), new Vector2(780, 54));
        title.color = Color.white;
        var score = Text("Score", headerP.transform, "Score: 0 / 100", 30, TextAlignmentOptions.MidlineRight, new Vector2(-24, -8), new Vector2(420, 54), Vector2.one, Vector2.one);
        score.color = Color.white;
        var progress = Text("Progress", headerP.transform, "Step 1 / 12", 26, TextAlignmentOptions.MidlineLeft, new Vector2(24, -52), new Vector2(320, 42));
        progress.color = new Color(0.85f, 0.92f, 1f);
        var attempts = Text("Attempts", headerP.transform, "Attempts: 3", 26, TextAlignmentOptions.MidlineRight, new Vector2(-24, -52), new Vector2(320, 42), Vector2.one, Vector2.one);
        attempts.color = new Color(0.85f, 0.92f, 1f);
        var progressBarBg = Panel(headerP.transform, "ProgressBarBg", new Vector2(0.25f, 0), new Vector2(0.75f, 0), new Vector2(0, 6), new Vector2(0, 18), new Color(0.1f, 0.2f, 0.3f, 0.5f));
        var progressBarFill = Panel(progressBarBg.transform, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.3f, 0.85f, 0.45f));
        progressBarFill.GetComponent<Image>().type = Image.Type.Filled;
        progressBarFill.GetComponent<Image>().fillMethod = Image.FillMethod.Horizontal;
        progressBarFill.GetComponent<Image>().fillAmount = 0.1f;

        var instructionBar = Panel(canvasObj.transform, "InstructionBar", new Vector2(0, 0.80f), new Vector2(1, 0.88f), Vector2.zero, Vector2.zero, new Color(1f, 0.97f, 0.85f));
        instructionBar.SetActive(false);
        var instruction = StretchText("Instruction", instructionBar.transform, "Follow the instructions.", 28, TextAlignmentOptions.MidlineLeft, new Color(0.2f, 0.2f, 0.25f), 16);

        var bottom = Panel(canvasObj.transform, "BottomBar", Vector2.zero, new Vector2(1, 0.12f), Vector2.zero, Vector2.zero, new Color(0.18f, 0.22f, 0.28f));
        var bottomLayout = bottom.AddComponent<HorizontalLayoutGroup>();
        bottomLayout.padding = new RectOffset(20, 20, 10, 10);
        bottomLayout.spacing = 16;
        bottomLayout.childAlignment = TextAnchor.MiddleCenter;
        var nextBtn = Btn("Next", bottom.transform, "Next Step", accent, 200, 64);
        nextBtn.gameObject.SetActive(false);
        var resetBtn = Btn("Reset", bottom.transform, "Reset", new Color(0.45f, 0.48f, 0.52f), 160, 64);
        var retryBtn = Btn("Retry", bottom.transform, "Retry", accent, 160, 64);
        retryBtn.gameObject.SetActive(false);

        var main = Panel(canvasObj.transform, "MainArea", new Vector2(0, 0.12f), new Vector2(1, 0.80f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0));

        // Objective
        var objectivePanel = Panel(main.transform, "ObjectivePanel", Vector2.zero, Vector2.one, new Vector2(40, 40), new Vector2(-40, -40), Color.white);
        var objectiveText = StretchText("ObjectiveText", objectivePanel.transform, "", 32, TextAlignmentOptions.TopLeft, new Color(0.15f, 0.18f, 0.25f), 24);
        var startBtn = BigBtn("StartBtn", objectivePanel.transform, "START PRACTICAL", new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(380, 80), new Color(0.12f, 0.62f, 0.35f));

        // Equipment
        var equipPanel = Panel(main.transform, "EquipmentPanel", Vector2.zero, Vector2.one, new Vector2(24, 16), new Vector2(-24, -16), Color.white);
        equipPanel.SetActive(false);
        Text("EquipTitle", equipPanel.transform, "Step 1 — Select the 5 required items for the lever experiment", 32, TextAlignmentOptions.MidlineLeft, new Vector2(20, -16), new Vector2(1300, 48));
        var equipHint = Text("EquipHint", equipPanel.transform, "Tap the 5 correct items. Wrong items give -5 marks.", 24, TextAlignmentOptions.MidlineLeft, new Vector2(20, -60), new Vector2(1000, 36));
        equipHint.color = new Color(0.25f, 0.35f, 0.5f);
        var requiredArea = Panel(equipPanel.transform, "RequiredArea", new Vector2(0, 0), new Vector2(1, 0), new Vector2(16, 70), new Vector2(-16, 150), new Color(0.88f, 0.94f, 1f));
        var reqLayout = requiredArea.AddComponent<HorizontalLayoutGroup>();
        reqLayout.spacing = 10;
        reqLayout.padding = new RectOffset(8, 8, 8, 8);
        reqLayout.childAlignment = TextAnchor.MiddleLeft;
        var scroll = CreateScroll(equipPanel.transform, new Vector2(12, 160), new Vector2(-12, 100));
        var cardPrefab = CreateCardPrefab();
        var equipContinue = BigBtn("EquipContinueBtn", equipPanel.transform, "NEXT STEP", new Vector2(1f, 0f), new Vector2(-180, 28), new Vector2(260, 56), new Color(0.12f, 0.62f, 0.35f));
        equipContinue.gameObject.SetActive(false);

        // Experiment panel
        var experimentPanel = Panel(main.transform, "ExperimentPanel", Vector2.zero, Vector2.one, new Vector2(16, 8), new Vector2(-16, -8), new Color(0, 0, 0, 0));
        experimentPanel.SetActive(false);

        var infoCard = Panel(experimentPanel.transform, "InfoCard", new Vector2(0.68f, 0.55f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.92f));
        var infoText = StretchText("InfoText", infoCard.transform, "Experiment Information", 24, TextAlignmentOptions.TopLeft, new Color(0.2f, 0.25f, 0.3f), 12);

        var dataTablePanel = Panel(experimentPanel.transform, "DataTablePanel", new Vector2(0.68f, 0.08f), new Vector2(0.98f, 0.52f), Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.92f));
        var dataTableText = StretchText("DataTableText", dataTablePanel.transform, "Table 15.1", 22, TextAlignmentOptions.TopLeft, new Color(0.15f, 0.2f, 0.25f), 10);

        var workbench = Panel(experimentPanel.transform, "Workbench", new Vector2(0.02f, 0.08f), new Vector2(0.66f, 0.98f), Vector2.zero, Vector2.zero, new Color(0.9f, 0.94f, 0.99f));

        // Drop zones in upper tray — must NOT overlap the palette.
        var setupTray = Panel(workbench.transform, "SetupTray", new Vector2(0.02f, 0.42f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);

        var pivotZone = CreateDropZone(setupTray.transform, "PivotZone", "SupportPivot", "Pivot", new Vector2(0.38f, 0.08f), new Vector2(0.62f, 0.38f), "PIVOT");
        var stripZone = CreateDropZone(setupTray.transform, "StripZone", "WoodenStrip", "WoodenStrip", new Vector2(0.12f, 0.40f), new Vector2(0.88f, 0.58f), "WOODEN STRIP");
        var bookZone = CreateDropZone(setupTray.transform, "BookZone", "Book", "Book", new Vector2(0.08f, 0.62f), new Vector2(0.32f, 0.95f), "BOOK");
        var springBalanceZone = CreateDropZone(setupTray.transform, "SpringBalanceZone", "NewtonSpringBalance", "NewtonSpringBalance", new Vector2(0.68f, 0.62f), new Vector2(0.92f, 0.95f), "SPRING BALANCE");

        var experimentVisual = Panel(workbench.transform, "ExperimentVisual", new Vector2(0.08f, 0.48f), new Vector2(0.92f, 0.96f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        experimentVisual.SetActive(false);

        var pivotVisual = Panel(experimentVisual.transform, "PivotVisual", new Vector2(0.42f, 0.02f), new Vector2(0.58f, 0.28f), Vector2.zero, Vector2.zero, new Color(0.45f, 0.48f, 0.52f));
        pivotVisual.GetComponent<Image>().raycastTarget = false;
        // Triangle-ish look via stretched icon if available
        var pivotIcon = Icon(pivotVisual.transform, "PivotIcon", LeverEquipmentType.SupportPivot, Vector2.zero, Vector2.one);
        pivotIcon.GetComponent<Image>().color = Color.white;

        var stripVisual = Panel(experimentVisual.transform, "StripVisual", new Vector2(0.08f, 0.30f), new Vector2(0.92f, 0.42f), Vector2.zero, Vector2.zero, new Color(0.72f, 0.52f, 0.28f));
        stripVisual.GetComponent<Image>().raycastTarget = false;
        var stripIcon = Icon(stripVisual.transform, "StripIcon", LeverEquipmentType.WoodenStrip, Vector2.zero, Vector2.one);
        stripIcon.GetComponent<Image>().color = Color.white;

        var bookVisual = Icon(experimentVisual.transform, "BookVisual", LeverEquipmentType.Book, new Vector2(0.08f, 0.44f), new Vector2(0.28f, 0.78f));
        var springBalanceVisual = Icon(experimentVisual.transform, "SpringBalanceVisual", LeverEquipmentType.NewtonSpringBalance, new Vector2(0.72f, 0.44f), new Vector2(0.92f, 0.78f));

        // PullArea — large, visible, on top for drag
        var pullArea = Panel(workbench.transform, "PullArea", new Vector2(0.58f, 0.48f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero, new Color(1f, 0.95f, 0.9f, 0.92f));
        pullArea.SetActive(false);
        StretchText("PullTitle", pullArea.transform, "PULL DOWN\nor TAP handle", 20, TextAlignmentOptions.Top, new Color(0.55f, 0.2f, 0.1f), 6);

        var springVisual = Panel(pullArea.transform, "SpringVisual", new Vector2(0.35f, 0.35f), new Vector2(0.65f, 0.92f), Vector2.zero, Vector2.zero, new Color(0.55f, 0.58f, 0.62f));
        springVisual.GetComponent<Image>().raycastTarget = false;
        var springRt = springVisual.GetComponent<RectTransform>();
        springRt.anchorMin = new Vector2(0.42f, 0.42f);
        springRt.anchorMax = new Vector2(0.58f, 0.88f);

        var pullHandle = Panel(pullArea.transform, "PullHandle", new Vector2(0.18f, 0.06f), new Vector2(0.82f, 0.38f), Vector2.zero, Vector2.zero, new Color(0.9f, 0.3f, 0.15f));
        pullHandle.GetComponent<Image>().raycastTarget = true;
        StretchText("HandleLabel", pullHandle.transform, "⬇ PULL / TAP", 22, TextAlignmentOptions.Center, Color.white, 4).fontStyle = FontStyles.Bold;

        var pullMoreBtn = BigBtn("PullMoreBtn", pullArea.transform, "+ FORCE", new Vector2(0.5f, 0f), new Vector2(0, 8), new Vector2(150, 44), new Color(0.75f, 0.25f, 0.15f));
        pullMoreBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.15f, 0f);
        pullMoreBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.85f, 0f);
        pullMoreBtn.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
        pullMoreBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 6);
        pullMoreBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 44);

        // Measurement panels (shown per step)
        var measureRow = Panel(workbench.transform, "MeasureRow", new Vector2(0.02f, 0.28f), new Vector2(0.98f, 0.46f), Vector2.zero, Vector2.zero, new Color(0.93f, 0.96f, 1f, 0.98f));

        var measureAPanel = Panel(measureRow.transform, "MeasureAPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        measureAPanel.SetActive(false);
        StretchText("AHint", measureAPanel.transform, "Distance a (book → pivot). Tap the correct value:", 20, TextAlignmentOptions.TopLeft, new Color(0.15f, 0.25f, 0.4f), 8);
        var measureAInput = CreateTMPInput(measureAPanel.transform, "MeasureAInput", new Vector2(0.02f, 0.12f), new Vector2(0.22f, 0.72f), "a cm");
        var confirmDistanceABtn = BigBtn("ConfirmDistanceABtn", measureAPanel.transform, "CONFIRM", new Vector2(0.35f, 0.42f), new Vector2(0, 0), new Vector2(130, 48), accent);
        confirmDistanceABtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.24f, 0.15f);
        confirmDistanceABtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.40f, 0.75f);
        confirmDistanceABtn.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        confirmDistanceABtn.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        confirmDistanceABtn.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        confirmDistanceABtn.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        var aQuick20 = BigBtn("AQuick20", measureAPanel.transform, "a = 20 cm", new Vector2(0.62f, 0.42f), new Vector2(0, 0), new Vector2(180, 52), new Color(0.12f, 0.62f, 0.35f));
        aQuick20.GetComponent<RectTransform>().anchorMin = new Vector2(0.45f, 0.12f);
        aQuick20.GetComponent<RectTransform>().anchorMax = new Vector2(0.72f, 0.78f);
        aQuick20.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        aQuick20.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        aQuick20.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        aQuick20.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        var measureALabel = Text("MeasureALabel", measureAPanel.transform, "a = 20 cm", 22, TextAlignmentOptions.MidlineLeft, new Vector2(8, 8), new Vector2(160, 32), new Vector2(0.75f, 0f), new Vector2(0.75f, 0f), new Vector2(0, 0));

        var measureXPanel = Panel(measureRow.transform, "MeasureXPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        measureXPanel.SetActive(false);
        var measureXLabel = Text("MeasureXLabel", measureXPanel.transform, "Select x = 10 cm", 22, TextAlignmentOptions.MidlineLeft, new Vector2(12, -8), new Vector2(280, 36));
        measureXLabel.fontStyle = FontStyles.Bold;
        var pivotLabel = Text("PivotLabel", measureXPanel.transform, "P", 26, TextAlignmentOptions.Center, new Vector2(-16, -8), new Vector2(40, 40), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
        pivotLabel.fontStyle = FontStyles.Bold;
        pivotLabel.color = new Color(0.15f, 0.4f, 0.7f);

        var xSelContainer = Panel(measureXPanel.transform, "XSelectionButtonsContainer", new Vector2(0.02f, 0.08f), new Vector2(0.98f, 0.70f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var xLayout = xSelContainer.AddComponent<HorizontalLayoutGroup>();
        xLayout.spacing = 10;
        xLayout.padding = new RectOffset(6, 6, 4, 4);
        xLayout.childAlignment = TextAnchor.MiddleCenter;
        xLayout.childControlWidth = false;
        xLayout.childForceExpandWidth = false;
        xLayout.childControlHeight = true;
        xLayout.childForceExpandHeight = true;

        var actionRow = Panel(workbench.transform, "ActionRow", new Vector2(0.02f, 0.20f), new Vector2(0.98f, 0.28f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var actionLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
        actionLayout.spacing = 10;
        actionLayout.childAlignment = TextAnchor.MiddleCenter;
        actionLayout.childControlWidth = true;
        actionLayout.childForceExpandWidth = true;
        var recordBtn = Btn("RecordBtn", actionRow.transform, "RECORD READING", new Color(0.12f, 0.58f, 0.35f), 0, 56);
        recordBtn.gameObject.SetActive(false);

        var palette = Panel(workbench.transform, "Palette", new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.19f), Vector2.zero, Vector2.zero, new Color(0.95f, 0.97f, 1f, 0.95f));
        var paletteLayout = palette.AddComponent<HorizontalLayoutGroup>();
        paletteLayout.spacing = 8;
        paletteLayout.padding = new RectOffset(8, 8, 8, 8);
        paletteLayout.childAlignment = TextAnchor.MiddleCenter;
        paletteLayout.childControlWidth = false;
        paletteLayout.childForceExpandWidth = false;

        var pivotItem = CreateDragItem(palette.transform, "PivotItem", "Pivot", LeverEquipmentType.SupportPivot, "Pivot", 120, 95);
        var stripItem = CreateDragItem(palette.transform, "StripItem", "WoodenStrip", LeverEquipmentType.WoodenStrip, "Wooden Strip", 150, 75);
        var bookItem = CreateDragItem(palette.transform, "BookItem", "Book", LeverEquipmentType.Book, "Book", 110, 95);
        var springBalanceItem = CreateDragItem(palette.transform, "SpringBalanceItem", "NewtonSpringBalance", LeverEquipmentType.NewtonSpringBalance, "Spring Balance", 130, 95);

        var forceLabel = Text("ForceLabel", workbench.transform, "Force: 0.0 N", 26, TextAlignmentOptions.MidlineLeft, new Vector2(12, -6), new Vector2(220, 36), new Vector2(0, 1), new Vector2(0, 1));
        forceLabel.fontStyle = FontStyles.Bold;
        var momentLabel = Text("MomentLabel", workbench.transform, "Moments", 18, TextAlignmentOptions.TopLeft, new Vector2(230, -6), new Vector2(400, 78), new Vector2(0, 1), new Vector2(0, 1));
#pragma warning disable CS0618
        momentLabel.enableWordWrapping = true;
#pragma warning restore CS0618
        var hintText = StretchText("Hint", workbench.transform, "Tap or drag items from the bottom tray into the zones.", 22, TextAlignmentOptions.Center, new Color(0.2f, 0.3f, 0.4f), 4);
        var hintRt = hintText.rectTransform;
        hintRt.anchorMin = new Vector2(0.02f, 0.46f);
        hintRt.anchorMax = new Vector2(0.98f, 0.52f);
        hintRt.offsetMin = Vector2.zero;
        hintRt.offsetMax = Vector2.zero;

        // Compare & Conclusion
        var comparePanel = Panel(main.transform, "ComparePanel", Vector2.zero, Vector2.one, new Vector2(24, 16), new Vector2(-24, -16), new Color(0.97f, 0.98f, 1f));
        comparePanel.SetActive(false);
        var compareTitle = Text("CompareTitle", comparePanel.transform, "STEP — Compare Your Results", 30, TextAlignmentOptions.MidlineLeft, new Vector2(24, -16), new Vector2(900, 44));
        compareTitle.fontStyle = FontStyles.Bold;
        var compareText = StretchText("CompareText", comparePanel.transform, "", 24, TextAlignmentOptions.TopLeft, new Color(0.15f, 0.18f, 0.25f), 24);
        var compareRt = compareText.rectTransform;
        compareRt.anchorMin = new Vector2(0.04f, 0.48f);
        compareRt.anchorMax = new Vector2(0.96f, 0.88f);
        compareRt.offsetMin = Vector2.zero;
        compareRt.offsetMax = Vector2.zero;
        var graphArea = Panel(comparePanel.transform, "GraphArea", new Vector2(0.05f, 0.14f), new Vector2(0.95f, 0.46f), Vector2.zero, Vector2.zero, new Color(0.92f, 0.95f, 1f));
        Text("GraphLabel", graphArea.transform, "Distance x (cm) →   |   Effort (N) ↑", 18, TextAlignmentOptions.Bottom, new Vector2(0, 8), new Vector2(400, 28), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        var dotPrefab = Panel(graphArea.transform, "DotPrefab", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-8, -8), new Vector2(8, 8), accent);
        var lineImg = Panel(graphArea.transform, "Line", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100, 4), accent);
        var compareHint = Text("CompareHint", comparePanel.transform, "Look at the trend, then press NEXT STEP to answer the conclusion questions.", 20, TextAlignmentOptions.Center, new Vector2(0, 18), new Vector2(1100, 36), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        compareHint.color = new Color(0.15f, 0.4f, 0.65f);

        var conclusionPanel = Panel(main.transform, "ConclusionPanel", Vector2.zero, Vector2.one, new Vector2(20, 12), new Vector2(-20, -12), new Color(0.96f, 0.98f, 1f));
        conclusionPanel.SetActive(false);

        var concHeader = Panel(conclusionPanel.transform, "Header", new Vector2(0f, 0.78f), new Vector2(1f, 1f), new Vector2(16, 8), new Vector2(-16, -8), new Color(0.12f, 0.42f, 0.68f));
        StretchText("StepLabel", concHeader.transform, "CONCLUSION", 20, TextAlignmentOptions.TopLeft, new Color(0.85f, 0.93f, 1f), 14);
        var questionText = StretchText("QuestionText", concHeader.transform, "What can be concluded from this experiment?", 28, TextAlignmentOptions.MidlineLeft, Color.white, 14);
        var questionRt = questionText.rectTransform;
        questionRt.offsetMin = new Vector2(14, 8);
        questionRt.offsetMax = new Vector2(-14, -28);
        questionText.fontStyle = FontStyles.Bold;

        var resultsStrip = Panel(conclusionPanel.transform, "ResultsStrip", new Vector2(0.03f, 0.66f), new Vector2(0.97f, 0.76f), Vector2.zero, Vector2.zero, new Color(1f, 0.97f, 0.86f));
        var resultsReminder = StretchText("ResultsReminder", resultsStrip.transform, "Your results: larger x → smaller effort", 22, TextAlignmentOptions.Center, new Color(0.35f, 0.25f, 0.05f), 8);
        resultsReminder.fontStyle = FontStyles.Bold;

        var tip = StretchText("Tip", conclusionPanel.transform, "Tap the correct answer below.", 20, TextAlignmentOptions.Center, new Color(0.2f, 0.35f, 0.5f), 4);
        var tipRt = tip.rectTransform;
        tipRt.anchorMin = new Vector2(0.05f, 0.60f);
        tipRt.anchorMax = new Vector2(0.95f, 0.66f);
        tipRt.offsetMin = tipRt.offsetMax = Vector2.zero;

        var concLayout = Panel(conclusionPanel.transform, "Options", new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.59f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var vLayout = concLayout.AddComponent<VerticalLayoutGroup>();
        vLayout.spacing = 14;
        vLayout.padding = new RectOffset(8, 8, 8, 8);
        vLayout.childControlHeight = true;
        vLayout.childForceExpandHeight = true;
        vLayout.childControlWidth = true;
        vLayout.childForceExpandWidth = true;
        var optA = ChoiceBtn("OptA", concLayout.transform, "A", "Increasing distance x increases the effort needed.");
        var optB = ChoiceBtn("OptB", concLayout.transform, "B", "Increasing distance x decreases the effort needed.");
        var optC = ChoiceBtn("OptC", concLayout.transform, "C", "Distance x has no effect on the effort.");
        var optD = ChoiceBtn("OptD", concLayout.transform, "D", "Effort is always equal to the load.");

        var explanationPanel = Panel(conclusionPanel.transform, "ExplanationPanel", new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.78f), Vector2.zero, Vector2.zero, new Color(0.12f, 0.48f, 0.32f));
        explanationPanel.SetActive(false);
        var explanationText = StretchText("ExplanationText", explanationPanel.transform, "", 24, TextAlignmentOptions.Top, Color.white, 20);
        var explanationRt = explanationText.rectTransform;
        explanationRt.offsetMin = new Vector2(20, 70);
        explanationRt.offsetMax = new Vector2(-20, -20);
        var continueObj = Panel(explanationPanel.transform, "ContinueBtn", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-120, 14), new Vector2(120, 68), Color.white);
        var continueRt = continueObj.GetComponent<RectTransform>();
        continueRt.anchoredPosition = new Vector2(0f, 16f);
        continueRt.sizeDelta = new Vector2(240f, 54f);
        var continueBtn = continueObj.AddComponent<Button>();
        var continueTxt = StretchText("Text", continueObj.transform, "Continue ▶", 22, TextAlignmentOptions.Center, new Color(0.1f, 0.4f, 0.25f), 4);
        continueTxt.fontStyle = FontStyles.Bold;

        // Result
        var resultPanel = Panel(main.transform, "ResultPanel", Vector2.zero, Vector2.one, new Vector2(40, 40), new Vector2(-40, -40), Color.white);
        resultPanel.SetActive(false);
        var finalScore = StretchText("FinalScore", resultPanel.transform, "", 28, TextAlignmentOptions.TopLeft, new Color(0.15f, 0.18f, 0.25f), 24);
        var resultDetails = StretchText("ResultDetails", resultPanel.transform, "", 22, TextAlignmentOptions.TopLeft, new Color(0.2f, 0.25f, 0.3f), 24);
        var rtDet = resultDetails.rectTransform;
        rtDet.anchorMin = new Vector2(0, 0.2f);
        rtDet.anchorMax = new Vector2(1, 0.7f);
        var statusText = Text("Status", resultPanel.transform, "STATUS: PASSED", 32, TextAlignmentOptions.MidlineLeft, new Vector2(24, 24), new Vector2(500, 50));
        var resultBtnRow = Panel(resultPanel.transform, "ResultBtns", new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.14f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0));
        var resultBtnLayout = resultBtnRow.AddComponent<HorizontalLayoutGroup>();
        resultBtnLayout.spacing = 16;
        resultBtnLayout.childForceExpandWidth = true;
        var viewProfileBtn = Btn("ViewProfile", resultBtnRow.transform, "View Profile", accent, 0, 52);

        // Feedback
        var feedbackPanel = Panel(canvasObj.transform, "FeedbackPanel", new Vector2(0.25f, 0.45f), new Vector2(0.75f, 0.55f), Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.95f));
        feedbackPanel.SetActive(false);
        var feedbackGroup = feedbackPanel.AddComponent<CanvasGroup>();
        var feedbackText = StretchText("FeedbackText", feedbackPanel.transform, "", 26, TextAlignmentOptions.Center, new Color(0.15f, 0.35f, 0.65f), 16);
        var scoreChangeText = Text("ScoreChange", feedbackPanel.transform, "", 22, TextAlignmentOptions.Bottom, new Vector2(0, 8), new Vector2(200, 30), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));

        // Reset confirm
        var resetConfirm = Panel(canvasObj.transform, "ResetConfirm", new Vector2(0.3f, 0.4f), new Vector2(0.7f, 0.6f), Vector2.zero, Vector2.zero, Color.white);
        resetConfirm.SetActive(false);
        StretchText("ResetMsg", resetConfirm.transform, "Are you sure you want to restart?", 26, TextAlignmentOptions.Center, new Color(0.2f, 0.2f, 0.25f), 16);
        var resetRow = Panel(resetConfirm.transform, "Btns", new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.35f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0));
        var resetRowLayout = resetRow.AddComponent<HorizontalLayoutGroup>();
        resetRowLayout.spacing = 20;
        resetRowLayout.childForceExpandWidth = true;
        var resetYes = Btn("Yes", resetRow.transform, "YES", new Color(0.75f, 0.2f, 0.2f), 0, 50);
        var resetNo = Btn("No", resetRow.transform, "NO", accent, 0, 50);

        // UI Refs — use existing LeverUIRefsHolder (do not redefine)
        var refs = canvasObj.AddComponent<LeverUIRefsHolder>();
        refs.UiVersion = 2;
        refs.Title = title;
        refs.Score = score;
        refs.Progress = progress;
        refs.Attempts = attempts;
        refs.ProgressBar = progressBarFill.GetComponent<Image>();
        refs.Instruction = instruction;
        refs.InstructionBar = instructionBar;
        refs.ObjectivePanel = objectivePanel;
        refs.ObjectiveText = objectiveText;
        refs.StartBtn = startBtn;
        refs.EquipmentPanel = equipPanel;
        refs.ExperimentPanel = experimentPanel;
        refs.EquipContinueBtn = equipContinue;
        refs.DataTablePanel = dataTablePanel;
        refs.DataTableText = dataTableText;
        refs.InfoText = infoText;
        refs.ComparePanel = comparePanel;
        refs.CompareText = compareText;
        refs.ConclusionPanel = conclusionPanel;
        refs.ResultPanel = resultPanel;
        refs.FinalScore = finalScore;
        refs.ResultDetails = resultDetails;
        refs.StatusText = statusText;
        refs.Next = nextBtn;
        refs.Reset = resetBtn;
        refs.Retry = retryBtn;
        refs.ViewProfileBtn = viewProfileBtn;
        refs.ResetConfirm = resetConfirm;
        refs.ResetYes = resetYes;
        refs.ResetNo = resetNo;
        refs.FeedbackPanel = feedbackPanel;
        refs.FeedbackText = feedbackText;
        refs.ScoreChangeText = scoreChangeText;
        refs.FeedbackGroup = feedbackGroup;
        refs.CardContainer = scroll.content;
        refs.RequiredArea = requiredArea.transform;
        refs.CardPrefab = cardPrefab;
        refs.RecordBtn = recordBtn;
        refs.ConfirmDistanceABtn = confirmDistanceABtn;
        refs.ForceLabel = forceLabel;
        refs.MomentLabel = momentLabel;
        refs.HintText = hintText;
        refs.MeasureALabel = measureALabel;
        refs.MeasureXLabel = measureXLabel;
        refs.PivotLabel = pivotLabel;
        refs.MeasureAInput = measureAInput;
        refs.XSelectionButtonsContainer = xSelContainer.transform;
        refs.XSelectionButtons = null;
        refs.PivotZone = pivotZone;
        refs.StripZone = stripZone;
        refs.BookZone = bookZone;
        refs.SpringBalanceZone = springBalanceZone;
        refs.PivotItem = pivotItem;
        refs.StripItem = stripItem;
        refs.BookItem = bookItem;
        refs.SpringBalanceItem = springBalanceItem;
        refs.SetupTray = setupTray;
        refs.ExperimentVisual = experimentVisual;
        refs.PullHandle = pullHandle.GetComponent<RectTransform>();
        refs.SpringVisual = springVisual.GetComponent<RectTransform>();
        refs.StripVisual = stripVisual.GetComponent<RectTransform>();
        refs.BookVisual = bookVisual.GetComponent<RectTransform>();
        refs.PivotVisual = pivotVisual.GetComponent<RectTransform>();
        refs.SpringBalanceVisual = springBalanceVisual.GetComponent<RectTransform>();
        refs.GraphArea = graphArea.GetComponent<RectTransform>();
        refs.DotPrefab = dotPrefab;
        refs.LineImage = lineImg.GetComponent<Image>();
        refs.ConclusionA = optA;
        refs.ConclusionB = optB;
        refs.ConclusionC = optC;
        refs.ConclusionD = optD;
        refs.ConclusionExplanationPanel = explanationPanel;
        refs.ConclusionExplanationText = explanationText;
        refs.ConclusionResultsReminder = resultsReminder;
        refs.ConclusionQuestionText = questionText;
        refs.ConclusionContinueBtn = continueBtn;
    }

    private bool WireReferences(bool showWelcome)
    {
        var refs = Object.FindAnyObjectByType<LeverUIRefsHolder>();
        if (refs == null || LeverUIManager.Instance == null) return false;

        if (managersRoot == null)
            managersRoot = transform.Find("Managers");

        LeverFeedbackManager.Instance?.BindUI(refs.FeedbackPanel, refs.FeedbackText, refs.ScoreChangeText, refs.FeedbackGroup);
        LeverUIManager.Instance.BindAll(refs, showWelcome);

        // Hard-wire Start so intro always works even if BindAll missed something.
        if (refs.StartBtn != null)
        {
            refs.StartBtn.onClick.RemoveAllListeners();
            refs.StartBtn.onClick.AddListener(() => LeverUIManager.Instance?.StartPractical());
        }
        if (refs.EquipContinueBtn != null)
        {
            refs.EquipContinueBtn.onClick.RemoveAllListeners();
            refs.EquipContinueBtn.onClick.AddListener(() => LeverUIManager.Instance?.GoNextFromEquipment());
        }

        LeverEquipmentSelectionManager.Instance?.SetupUI(refs.CardContainer, refs.RequiredArea, refs.CardPrefab);
        LeverConclusionManager.Instance?.Bind(
            refs.ConclusionA, refs.ConclusionB, refs.ConclusionC, refs.ConclusionD,
            refs.ConclusionExplanationPanel, refs.ConclusionExplanationText, refs.ConclusionResultsReminder,
            refs.ConclusionContinueBtn, refs.ConclusionQuestionText);
        LeverGraphView.Instance?.Bind(refs.GraphArea, refs.DotPrefab, refs.LineImage);

        GameObject pullArea = null;
        if (refs.ExperimentPanel != null)
        {
            var wb = refs.ExperimentPanel.transform.Find("Workbench");
            pullArea = wb != null ? wb.Find("PullArea")?.gameObject : null;
        }

        LeverLabWorkbench.Instance?.Bind(
            refs.PivotZone, refs.StripZone, refs.BookZone, refs.SpringBalanceZone,
            refs.PivotItem, refs.StripItem, refs.BookItem, refs.SpringBalanceItem,
            refs.SetupTray, refs.ExperimentVisual, pullArea,
            refs.RecordBtn, refs.HintText, refs.MomentLabel);

        // PullHandle must host the drag handler (IBeginDrag/IDrag). Relocate from Managers if needed.
        var pullCtrl = EnsureControllerOnRect<LeverPullHandleController>(refs.PullHandle, "LeverPullHandleController");
        pullCtrl?.Bind(refs.PullHandle);

        EnsureControllerOnRect<LeverSpringController>(refs.SpringVisual, "LeverSpringController")?.Bind(refs.SpringVisual);
        EnsureControllerOnRect<BookLiftController>(refs.BookVisual, "BookLiftController")?.Bind(refs.BookVisual);
        EnsureControllerOnRect<LeverWoodenStripController>(refs.StripVisual, "LeverWoodenStripController")?.Bind(refs.StripVisual);
        EnsureControllerOnRect<LeverPivotController>(refs.PivotVisual, "LeverPivotController")?.Bind(refs.PivotVisual);

        NewtonSpringBalanceController.Instance?.Bind(refs.ForceLabel, refs.SpringVisual, LeverSpringController.Instance);

        GameObject measureAPanel = null;
        GameObject measureXPanel = null;
        Button aQuick20 = null;
        Button pullMoreBtn = null;
        if (refs.ExperimentPanel != null)
        {
            var wb = refs.ExperimentPanel.transform.Find("Workbench");
            var measureRow = wb != null ? wb.Find("MeasureRow") : null;
            measureAPanel = measureRow != null ? measureRow.Find("MeasureAPanel")?.gameObject : null;
            measureXPanel = measureRow != null ? measureRow.Find("MeasureXPanel")?.gameObject : null;
            if (measureAPanel != null)
            {
                var quick = measureAPanel.transform.Find("AQuick20");
                if (quick != null) aQuick20 = quick.GetComponent<Button>();
            }
            var pullAreaTr = wb != null ? wb.Find("PullArea") : null;
            if (pullAreaTr != null)
            {
                var pm = pullAreaTr.Find("PullMoreBtn");
                if (pm != null) pullMoreBtn = pm.GetComponent<Button>();
            }
        }

        LeverMeasurementManager.Instance?.Bind(
            refs.MeasureAInput, refs.ConfirmDistanceABtn, refs.XSelectionButtonsContainer,
            refs.MeasureALabel, refs.MeasureXLabel, refs.PivotLabel, refs.XSelectionButtons,
            measureAPanel, measureXPanel);

        if (aQuick20 != null)
        {
            aQuick20.onClick.RemoveAllListeners();
            float aVal = LeverExperimentDataManager.Instance != null
                ? LeverExperimentDataManager.Instance.distanceA
                : 20f;
            aQuick20.onClick.AddListener(() => LeverMeasurementManager.Instance?.SelectDistanceA(aVal));
        }

        if (pullMoreBtn != null)
        {
            pullMoreBtn.onClick.RemoveAllListeners();
            pullMoreBtn.onClick.AddListener(() => LeverPullHandleController.Instance?.AddForceStep(2f));
        }

        return refs.StartBtn != null;
    }

    /// <summary>
    /// Ensures component T lives on the visual RectTransform (needed for drag handlers / Bind targets).
    /// Removes the Managers-root duplicate of the same name so the singleton Awake can claim Instance.
    /// </summary>
    private T EnsureControllerOnRect<T>(RectTransform target, string managerName) where T : Component
    {
        if (target == null) return Object.FindAnyObjectByType<T>();

        var onTarget = target.GetComponent<T>();
        if (onTarget != null) return onTarget;

        if (managersRoot == null)
            managersRoot = transform.Find("Managers");

        if (managersRoot != null)
        {
            var mgr = managersRoot.Find(managerName);
            if (mgr != null)
            {
                // Must be immediate — deferred Destroy leaves old Instance alive and
                // the new component's Awake would destroy itself.
                Object.DestroyImmediate(mgr.gameObject);
            }
        }

        foreach (var other in Object.FindObjectsByType<T>(FindObjectsSortMode.None))
        {
            if (other == null) continue;
            if (other.transform == target || other.GetComponent<RectTransform>() == target) continue;
            if (managersRoot != null && other.transform.parent == managersRoot)
                Object.DestroyImmediate(other.gameObject);
        }

        return target.gameObject.AddComponent<T>();
    }

    #region Helpers

    private GameObject Panel(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax, Color color, bool raycast = true)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = offMin; rt.offsetMax = offMax;
        var img = obj.AddComponent<Image>();
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
        tmp.raycastTarget = false;
        return tmp;
    }

    private Button ChoiceBtn(string name, Transform parent, string letter, string text)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var le = obj.AddComponent<LayoutElement>();
        le.preferredHeight = 84;
        le.minHeight = 72;
        var img = obj.AddComponent<Image>();
        img.color = Color.white;
        var outline = obj.AddComponent<Outline>();
        outline.effectColor = new Color(0.15f, 0.45f, 0.75f, 0.35f);
        outline.effectDistance = new Vector2(2f, -2f);
        var btn = obj.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.88f, 0.94f, 1f);
        colors.pressedColor = new Color(0.75f, 0.88f, 1f);
        colors.selectedColor = new Color(0.88f, 0.94f, 1f);
        btn.colors = colors;

        var letterBg = new GameObject("Letter");
        letterBg.transform.SetParent(obj.transform, false);
        var letterRt = letterBg.AddComponent<RectTransform>();
        letterRt.anchorMin = new Vector2(0f, 0.12f);
        letterRt.anchorMax = new Vector2(0f, 0.88f);
        letterRt.offsetMin = new Vector2(10, 0);
        letterRt.offsetMax = new Vector2(68, 0);
        var letterImg = letterBg.AddComponent<Image>();
        letterImg.color = new Color(0.15f, 0.45f, 0.75f);
        letterImg.raycastTarget = false;
        var letterTxtObj = new GameObject("LetterText");
        letterTxtObj.transform.SetParent(letterBg.transform, false);
        var letterTxtRt = letterTxtObj.AddComponent<RectTransform>();
        letterTxtRt.anchorMin = Vector2.zero;
        letterTxtRt.anchorMax = Vector2.one;
        letterTxtRt.offsetMin = letterTxtRt.offsetMax = Vector2.zero;
        var letterTxt = letterTxtObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) letterTxt.font = defaultFont;
        letterTxt.text = letter;
        letterTxt.fontSize = 32;
        letterTxt.fontStyle = FontStyles.Bold;
        letterTxt.alignment = TextAlignmentOptions.Center;
        letterTxt.color = Color.white;
        letterTxt.raycastTarget = false;

        var bodyObj = new GameObject("Body");
        bodyObj.transform.SetParent(obj.transform, false);
        var bodyRt = bodyObj.AddComponent<RectTransform>();
        bodyRt.anchorMin = Vector2.zero;
        bodyRt.anchorMax = Vector2.one;
        bodyRt.offsetMin = new Vector2(82, 8);
        bodyRt.offsetMax = new Vector2(-16, -8);
        var body = bodyObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) body.font = defaultFont;
        body.text = text;
        body.fontSize = 24;
        body.fontStyle = FontStyles.Bold;
        body.alignment = TextAlignmentOptions.MidlineLeft;
        body.color = new Color(0.12f, 0.16f, 0.22f);
        body.raycastTarget = false;
#pragma warning disable CS0618
        body.enableWordWrapping = true;
#pragma warning restore CS0618
        return btn;
    }

    private Button Btn(string name, Transform parent, string label, Color bg, float w, float h)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var le = obj.AddComponent<LayoutElement>();
        if (w > 0) le.preferredWidth = w;
        le.preferredHeight = h;
        obj.AddComponent<Image>().color = bg;
        var btn = obj.AddComponent<Button>();
        var txt = StretchText("Text", obj.transform, label, 24, TextAlignmentOptions.Center, Color.white, 4);
        txt.fontStyle = FontStyles.Bold;
        return btn;
    }

    private Button BigBtn(string name, Transform parent, string label, Vector2 anchor, Vector2 pos, Vector2 size, Color bg)
    {
        var obj = Panel(parent, name, anchor, anchor, Vector2.zero, size, bg);
        obj.GetComponent<RectTransform>().anchoredPosition = pos;
        var btn = obj.AddComponent<Button>();
        StretchText("Text", obj.transform, label, 24, TextAlignmentOptions.Center, Color.white, 8).fontStyle = FontStyles.Bold;
        return btn;
    }

    private ScrollRect CreateScroll(Transform parent, Vector2 offMin, Vector2 offMax)
    {
        var scrollObj = Panel(parent, "Scroll", Vector2.zero, Vector2.one, offMin, offMax, new Color(0.96f, 0.98f, 1f));
        var scroll = scrollObj.AddComponent<ScrollRect>();
        var viewport = Panel(scrollObj.transform, "Viewport", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.01f));
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        var content = Panel(viewport.transform, "Content", new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.01f));
        var contentRt = content.GetComponent<RectTransform>();
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.sizeDelta = new Vector2(0f, 900f);
        var grid = content.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(260, 200);
        grid.spacing = new Vector2(12, 12);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        grid.padding = new RectOffset(12, 12, 12, 12);
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
        card.AddComponent<RectTransform>().sizeDelta = new Vector2(260, 200);
        card.AddComponent<Image>().color = new Color(0.92f, 0.96f, 1f);
        card.AddComponent<LeverEquipmentCardUI>();
        card.AddComponent<Button>();
        card.SetActive(false);
        return card;
    }

    private LeverUIDropTarget CreateDropZone(Transform parent, string name, string zoneId, string acceptedId, Vector2 aMin, Vector2 aMax, string label)
    {
        var zone = Panel(parent, name, aMin, aMax, Vector2.zero, Vector2.zero, new Color(0.8f, 0.88f, 0.95f, 0.6f));
        StretchText("Label", zone.transform, label, 16, TextAlignmentOptions.Center, new Color(0.2f, 0.3f, 0.4f), 4);
        var target = zone.AddComponent<LeverUIDropTarget>();
        target.Configure(zoneId, acceptedId, Vector2.zero);
        return target;
    }

    private LeverDraggableUIItem CreateDragItem(Transform parent, string name, string id, LeverEquipmentType type, string label, float w, float h)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var le = obj.AddComponent<LayoutElement>();
        le.preferredWidth = w; le.preferredHeight = h;
        le.minWidth = w; le.minHeight = h;
        var bg = obj.AddComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0.92f);
        bg.raycastTarget = true;

        var iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(obj.transform, false);
        var iconRt = iconObj.AddComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.08f, 0.28f);
        iconRt.anchorMax = new Vector2(0.92f, 0.95f);
        iconRt.offsetMin = iconRt.offsetMax = Vector2.zero;
        var icon = iconObj.AddComponent<Image>();
        icon.sprite = LeverIconFactory.GetSprite(type);
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(obj.transform, false);
        var labelRt = labelObj.AddComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0f, 0f);
        labelRt.anchorMax = new Vector2(1f, 0f);
        labelRt.pivot = new Vector2(0.5f, 0f);
        labelRt.offsetMin = new Vector2(2f, 2f);
        labelRt.offsetMax = new Vector2(-2f, 24f);
        var labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) labelTmp.font = defaultFont;
        labelTmp.text = label;
        labelTmp.fontSize = 14;
        labelTmp.alignment = TextAlignmentOptions.Center;
        labelTmp.color = new Color(0.15f, 0.2f, 0.25f);
        labelTmp.raycastTarget = false;

        var drag = obj.AddComponent<LeverDraggableUIItem>();
        drag.Configure(id);
        obj.AddComponent<CanvasGroup>();
        return drag;
    }

    private GameObject Icon(Transform parent, string name, LeverEquipmentType type, Vector2 aMin, Vector2 aMax)
    {
        var obj = Panel(parent, name, aMin, aMax, Vector2.zero, Vector2.zero, Color.white, false);
        obj.GetComponent<Image>().sprite = LeverIconFactory.GetSprite(type);
        obj.GetComponent<Image>().preserveAspect = true;
        obj.GetComponent<Image>().raycastTarget = false;
        return obj;
    }

    private TMP_InputField CreateTMPInput(Transform parent, string name, Vector2 aMin, Vector2 aMax, string placeholder)
    {
        var obj = Panel(parent, name, aMin, aMax, Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 1f));
        var input = obj.AddComponent<TMP_InputField>();

        var textArea = new GameObject("Text Area");
        textArea.transform.SetParent(obj.transform, false);
        var areaRt = textArea.AddComponent<RectTransform>();
        areaRt.anchorMin = Vector2.zero;
        areaRt.anchorMax = Vector2.one;
        areaRt.offsetMin = new Vector2(8, 4);
        areaRt.offsetMax = new Vector2(-8, -4);
        textArea.AddComponent<RectMask2D>();

        var placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(textArea.transform, false);
        var phRt = placeholderObj.AddComponent<RectTransform>();
        phRt.anchorMin = Vector2.zero;
        phRt.anchorMax = Vector2.one;
        phRt.offsetMin = phRt.offsetMax = Vector2.zero;
        var ph = placeholderObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) ph.font = defaultFont;
        ph.text = placeholder;
        ph.fontSize = 22;
        ph.fontStyle = FontStyles.Italic;
        ph.color = new Color(0.5f, 0.55f, 0.6f);
        ph.alignment = TextAlignmentOptions.MidlineLeft;
        ph.raycastTarget = false;

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(textArea.transform, false);
        var textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = textRt.offsetMax = Vector2.zero;
        var text = textObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) text.font = defaultFont;
        text.text = "";
        text.fontSize = 24;
        text.color = new Color(0.12f, 0.15f, 0.2f);
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.raycastTarget = false;

        input.textViewport = areaRt;
        input.textComponent = text;
        input.placeholder = ph;
        input.contentType = TMP_InputField.ContentType.DecimalNumber;
        input.caretColor = new Color(0.15f, 0.4f, 0.7f);
        return input;
    }

    #endregion
}
