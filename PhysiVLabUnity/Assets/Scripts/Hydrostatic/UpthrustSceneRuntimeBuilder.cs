using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Builds the complete 2D Upthrust / Archimedes lab at edit-time or on Play.
/// Tools → Upthrust Practical → Build Complete Scene, then press Play.
/// </summary>
public class UpthrustSceneRuntimeBuilder : MonoBehaviour
{
    public const int RequiredUiVersion = 6;

    Font uiFont;

    UpthrustScoreManager scoreManager;
    UpthrustProfileManager profileManager;
    UpthrustEquipmentSelector equipmentSelector;
    UpthrustPracticalManager practicalManager;
    UpthrustSpringBalanceGauge springGauge;
    UpthrustObservationTableUI observationTable;
    UpthrustUIManager uiManager;
    UpthrustLabVisuals labVisuals;

    GameObject hudBar, phase1Panel, phase2Panel, phase3Panel, endPanel, stepActionsRoot;
    Text scoreText, instructionText, progressText, feedbackText;
    Text startButtonLabel, liveSpringText, liveBeakerText, liveUpthrustText, liveDisplacedText;
    Text finalScoreText, correctText, mistakeText, gradeText, starsText, tableHintText, wAirText, wEmptyText;
    Button startPhase2Button, restartButton, submitTableButton;
    RectTransform answerTray;

    RectTransform hangingAssembly, cubeRt, overflowRt;
    Image beakerWater, eurekaWater, needleImage;
    GameObject beakerObject, cubeObject;
    Text stageLabel, gaugeReadout;

    static readonly Color ColBg = new Color(0.10f, 0.16f, 0.22f, 1f);
    static readonly Color ColPanel = new Color(0.16f, 0.24f, 0.32f, 0.97f);
    static readonly Color ColCard = new Color(0.24f, 0.32f, 0.40f, 1f);
    static readonly Color ColAccent = new Color(0.25f, 0.72f, 0.82f, 1f);
    static readonly Color ColGood = new Color(0.30f, 0.78f, 0.42f, 1f);
    static readonly Color ColBad = new Color(0.90f, 0.32f, 0.32f, 1f);
    static readonly Color ColText = new Color(0.95f, 0.96f, 0.97f, 1f);
    static readonly Color ColDisabled = new Color(0.35f, 0.38f, 0.42f, 1f);
    static readonly Color ColWater = new Color(0.22f, 0.52f, 0.86f, 0.72f);
    static readonly Color ColBench = new Color(0.42f, 0.30f, 0.18f, 1f);

    struct ItemDef
    {
        public UpthrustApparatusType type;
        public string id;
        public string label;
        public bool correct;
    }

    readonly ItemDef[] trayItems =
    {
        new ItemDef { type = UpthrustApparatusType.MetalCube, id = "cube", label = "Metal Cube", correct = true },
        new ItemDef { type = UpthrustApparatusType.SpringBalance, id = "spring", label = "Spring Balance", correct = true },
        new ItemDef { type = UpthrustApparatusType.EurekaCan, id = "eureka", label = "Eureka Can", correct = true },
        new ItemDef { type = UpthrustApparatusType.SmallBeaker, id = "beaker", label = "Empty Beaker", correct = true },
        new ItemDef { type = UpthrustApparatusType.RetortStand, id = "stand", label = "Retort Stand", correct = true },
        new ItemDef { type = UpthrustApparatusType.Thermometer, id = "thermometer", label = "Thermometer", correct = false },
        new ItemDef { type = UpthrustApparatusType.Voltmeter, id = "voltmeter", label = "Voltmeter", correct = false },
        new ItemDef { type = UpthrustApparatusType.ConvexLens, id = "lens", label = "Convex Lens", correct = false },
        new ItemDef { type = UpthrustApparatusType.FrictionWoodenBlock, id = "block", label = "Wooden Block", correct = false },
    };

    public bool HasExistingBuild() => transform.Find("Canvas") != null;

    public void BuildScenePersistent()
    {
        try
        {
            ClearChildren();
            LoadResources();
            EnsureEventSystem();
            SetupCamera();
            CreateManagers();
            CreateUI();
            Debug.Log("Upthrust Practical: UI built (v" + RequiredUiVersion + "). Press Play.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Upthrust BUILD FAILED: " + ex.Message + "\n" + ex.StackTrace);
        }
    }

    public void WireOnPlay()
    {
        LoadResources();
        EnsureEventSystem();
        CacheManagers();
        CacheUiRefs();
        ApplyFontEverywhere();
        BindManagers();
        ShowPhase1();
        Debug.Log("Upthrust Practical: ready — select 5 correct items, then Start Practical.");
    }

    private void Awake()
    {
        if (!Application.isPlaying) return;
        if (!HasExistingBuild())
            BuildScenePersistent();
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }

    private void LoadResources()
    {
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null) uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (uiFont == null)
            uiFont = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial", "Helvetica" }, 24);
    }

    private void ApplyFontEverywhere()
    {
        if (uiFont == null) return;
        var canvas = transform.Find("Canvas");
        if (canvas == null) return;
        foreach (var t in canvas.GetComponentsInChildren<Text>(true))
        {
            t.font = uiFont;
            if (t.fontSize < 14) t.fontSize = 16;
        }
    }

    private void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;
        cam.orthographic = true;
        cam.backgroundColor = ColBg;
    }

    private void CreateManagers()
    {
        var existing = transform.Find("Managers");
        if (existing != null)
            DestroyImmediate(existing.gameObject);

        var root = new GameObject("Managers").transform;
        root.SetParent(transform, false);

        scoreManager = NewMgr<UpthrustScoreManager>(root, "UpthrustScoreManager");
        profileManager = NewMgr<UpthrustProfileManager>(root, "UpthrustProfileManager");
        equipmentSelector = NewMgr<UpthrustEquipmentSelector>(root, "UpthrustEquipmentSelector");
        practicalManager = NewMgr<UpthrustPracticalManager>(root, "UpthrustPracticalManager");
        springGauge = NewMgr<UpthrustSpringBalanceGauge>(root, "UpthrustSpringBalanceGauge");
        observationTable = NewMgr<UpthrustObservationTableUI>(root, "UpthrustObservationTableUI");
        uiManager = NewMgr<UpthrustUIManager>(root, "UpthrustUIManager");
        labVisuals = NewMgr<UpthrustLabVisuals>(root, "UpthrustLabVisuals");
    }

    private static T NewMgr<T>(Transform parent, string name) where T : Component
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<T>();
    }

    private void CacheManagers()
    {
        scoreManager = UpthrustScoreManager.Instance;
        if (scoreManager == null) scoreManager = GetComponentInChildren<UpthrustScoreManager>(true);
        profileManager = UpthrustProfileManager.Instance;
        if (profileManager == null) profileManager = GetComponentInChildren<UpthrustProfileManager>(true);
        equipmentSelector = UpthrustEquipmentSelector.Instance;
        if (equipmentSelector == null) equipmentSelector = GetComponentInChildren<UpthrustEquipmentSelector>(true);
        practicalManager = UpthrustPracticalManager.Instance;
        if (practicalManager == null) practicalManager = GetComponentInChildren<UpthrustPracticalManager>(true);
        springGauge = UpthrustSpringBalanceGauge.Instance;
        if (springGauge == null) springGauge = GetComponentInChildren<UpthrustSpringBalanceGauge>(true);
        observationTable = UpthrustObservationTableUI.Instance;
        if (observationTable == null) observationTable = GetComponentInChildren<UpthrustObservationTableUI>(true);
        uiManager = UpthrustUIManager.Instance;
        if (uiManager == null) uiManager = GetComponentInChildren<UpthrustUIManager>(true);
        labVisuals = UpthrustLabVisuals.Instance;
        if (labVisuals == null) labVisuals = GetComponentInChildren<UpthrustLabVisuals>(true);

        if (scoreManager == null) scoreManager = NewMgr<UpthrustScoreManager>(transform, "UpthrustScoreManager");
        if (profileManager == null) profileManager = NewMgr<UpthrustProfileManager>(transform, "UpthrustProfileManager");
        if (equipmentSelector == null) equipmentSelector = NewMgr<UpthrustEquipmentSelector>(transform, "UpthrustEquipmentSelector");
        if (practicalManager == null) practicalManager = NewMgr<UpthrustPracticalManager>(transform, "UpthrustPracticalManager");
        if (springGauge == null) springGauge = NewMgr<UpthrustSpringBalanceGauge>(transform, "UpthrustSpringBalanceGauge");
        if (observationTable == null) observationTable = NewMgr<UpthrustObservationTableUI>(transform, "UpthrustObservationTableUI");
        if (uiManager == null) uiManager = NewMgr<UpthrustUIManager>(transform, "UpthrustUIManager");
        if (labVisuals == null) labVisuals = NewMgr<UpthrustLabVisuals>(transform, "UpthrustLabVisuals");
    }

    private void CreateUI()
    {
        var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var canvasRt = canvasGo.GetComponent<RectTransform>();
        var bg = MakeImage(canvasRt, "Background", ColBg, true);
        bg.raycastTarget = false;
        BuildHud(canvasRt);
        BuildPhase1(canvasRt);
        BuildPhase2(canvasRt);
        BuildPhase3(canvasRt);
        BuildEndScreen(canvasRt);

        var holder = canvasGo.GetComponent<UpthrustUIRefsHolder>() ?? canvasGo.AddComponent<UpthrustUIRefsHolder>();
        holder.UiVersion = RequiredUiVersion;

        if (hudBar != null)
            hudBar.transform.SetAsLastSibling();

        ApplyFontEverywhere();
    }

    private void BuildHud(RectTransform parent)
    {
        hudBar = MakePanel(parent, "HUD", new Color(0.07f, 0.11f, 0.15f, 1f),
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 150));
        var hudRt = hudBar.GetComponent<RectTransform>();
        hudRt.offsetMin = new Vector2(0, -150);
        hudRt.offsetMax = Vector2.zero;
        var hudImg = hudBar.GetComponent<Image>();
        if (hudImg != null) hudImg.raycastTarget = false;

        MakeText(hudRt, "Title", "Upthrust Practical — Archimedes' Principle", 24, TextAnchor.MiddleLeft,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(28, -28), new Vector2(820, 40), ColAccent);

        scoreText = MakeText(hudRt, "Score", "Score: 0 / 100", 28, TextAnchor.MiddleRight,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-28, -28), new Vector2(360, 40), ColText);
        scoreText.fontStyle = FontStyle.Bold;

        instructionText = MakeText(hudRt, "Instruction", "Select the correct apparatus.", 28, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -12), new Vector2(1760, 52), ColText);
        instructionText.fontStyle = FontStyle.Bold;
        instructionText.horizontalOverflow = HorizontalWrapMode.Wrap;
        instructionText.verticalOverflow = VerticalWrapMode.Truncate;
        instructionText.resizeTextForBestFit = true;
        instructionText.resizeTextMinSize = 20;
        instructionText.resizeTextMaxSize = 30;

        progressText = MakeText(hudRt, "Progress", "", 20, TextAnchor.LowerCenter,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(1200, 28), new Color(0.75f, 0.85f, 0.9f));

        feedbackText = MakeText(parent, "Feedback", "", 24, TextAnchor.LowerCenter,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 18), new Vector2(1400, 40), ColGood);
        feedbackText.fontStyle = FontStyle.Bold;
    }

    private void BuildPhase1(RectTransform parent)
    {
        phase1Panel = MakePanel(parent, "Phase1_Selection", ColPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -50), new Vector2(1700, 700));
        var p1 = phase1Panel.GetComponent<RectTransform>();

        MakeText(p1, "P1Title", "PHASE 1 — Select the Correct Apparatus", 32, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(1200, 44), ColAccent);
        MakeText(p1, "P1Hint", "Tap the 5 items needed to measure upthrust with a Eureka can. Wrong items lose 5 marks.", 20, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(1400, 32), new Color(0.8f, 0.85f, 0.9f));

        var grid = new GameObject("TrayGrid", typeof(RectTransform), typeof(GridLayoutGroup));
        grid.transform.SetParent(p1, false);
        var gridRt = grid.GetComponent<RectTransform>();
        SetAnchored(gridRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 16), new Vector2(1480, 500));
        var glg = grid.GetComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(250, 230);
        glg.spacing = new Vector2(18, 16);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 5;
        glg.childAlignment = TextAnchor.MiddleCenter;

        UpthrustIconFactory.ClearCache();
        foreach (var item in trayItems)
            CreateApparatusCard(gridRt, item);

        startPhase2Button = MakeButton(p1, "StartPhase2", "Select 5 correct items first...", new Vector2(0, -318), new Vector2(560, 68), ColDisabled);
        startButtonLabel = startPhase2Button.GetComponentInChildren<Text>(true);
        startPhase2Button.interactable = false;
    }

    private void CreateApparatusCard(RectTransform parent, ItemDef item)
    {
        var card = MakePanel(parent, "Card_" + item.id, ColCard,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(250, 230));
        var cardRt = card.GetComponent<RectTransform>();
        var cardImg = card.GetComponent<Image>();
        cardImg.raycastTarget = true;

        var icon = MakeImage(cardRt, "Icon", new Color(1f, 1f, 1f, 0.04f),
            new Vector2(0.5f, 0.64f), new Vector2(0.5f, 0.64f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120, 120));
        icon.sprite = UpthrustIconFactory.WhiteSprite;
        icon.raycastTarget = false;
        UpthrustIconFactory.BuildIcon(icon.rectTransform, item.id);

        var labelBox = MakePanel(cardRt, "LabelBox", new Color(0.12f, 0.16f, 0.20f, 0.55f),
            new Vector2(0.06f, 0.05f), new Vector2(0.94f, 0.34f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        var labelBoxImg = labelBox.GetComponent<Image>();
        labelBoxImg.raycastTarget = false;
        var labelRt = labelBox.GetComponent<RectTransform>();
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;

        var nameText = MakeText(labelRt, "Label", item.label, 20, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200, 56), ColText);
        nameText.fontStyle = FontStyle.Bold;
        nameText.resizeTextForBestFit = true;
        nameText.resizeTextMinSize = 14;
        nameText.resizeTextMaxSize = 22;
        nameText.horizontalOverflow = HorizontalWrapMode.Wrap;
        nameText.verticalOverflow = VerticalWrapMode.Truncate;
        nameText.raycastTarget = false;
        var nameRt = nameText.GetComponent<RectTransform>();
        nameRt.anchorMin = Vector2.zero;
        nameRt.anchorMax = Vector2.one;
        nameRt.offsetMin = new Vector2(8, 4);
        nameRt.offsetMax = new Vector2(-8, -4);

        var check = MakeText(cardRt, "Check", "", 26, TextAnchor.UpperRight,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-10, -6), new Vector2(36, 36), ColGood);
        check.fontStyle = FontStyle.Bold;
        check.raycastTarget = false;

        var itemComp = card.AddComponent<UpthrustApparatusItem>();
        itemComp.Configure(item.type, item.correct, cardImg, icon, check, ColCard);
    }

    private void BuildPhase2(RectTransform parent)
    {
        phase2Panel = MakePanel(parent, "Phase2_Lab", new Color(0.13f, 0.19f, 0.25f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(1760, 740));
        phase2Panel.SetActive(false);
        var p2 = phase2Panel.GetComponent<RectTransform>();

        MakeImage(p2, "Wall", new Color(0.20f, 0.30f, 0.38f),
            new Vector2(0, 0.38f), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        MakeImage(p2, "Bench", ColBench,
            new Vector2(0.04f, 0.16f), new Vector2(0.96f, 0.40f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        MakeImage(p2, "BenchTop", new Color(0.52f, 0.38f, 0.22f),
            new Vector2(0.04f, 0.38f), new Vector2(0.96f, 0.44f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        BuildStandAndBalance(p2);
        BuildEurekaCan(p2);
        BuildBeaker(p2);
        BuildReadingsCard(p2);

        stageLabel = MakeText(p2, "StageLabel", "Ready", 26, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.90f), new Vector2(0.5f, 0.90f), Vector2.zero, new Vector2(1100, 40), ColAccent);
        stageLabel.fontStyle = FontStyle.Bold;

        stepActionsRoot = MakePanel(p2, "StepActions", new Color(0.09f, 0.12f, 0.16f, 0.97f),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 12), new Vector2(1680, 168));
        var actionsRt = stepActionsRoot.GetComponent<RectTransform>();
        MakeText(actionsRt, "ActionsHint", "Choose the correct action for this step:", 18, TextAnchor.UpperLeft,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(16, -6), new Vector2(800, 28), new Color(0.85f, 0.92f, 0.96f));

        var row = new GameObject("ChoiceRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(actionsRt, false);
        var rowRt = row.GetComponent<RectTransform>();
        SetAnchored(rowRt, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        rowRt.offsetMin = new Vector2(14, 10);
        rowRt.offsetMax = new Vector2(-14, -32);
        var layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 14;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = true;
    }

    private void BuildStandAndBalance(RectTransform p2)
    {
        MakeImage(p2, "StandBase", new Color(0.28f, 0.30f, 0.34f),
            new Vector2(0.10f, 0.18f), new Vector2(0.10f, 0.18f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(140, 18));
        MakeImage(p2, "StandPole", new Color(0.32f, 0.34f, 0.38f),
            new Vector2(0.07f, 0.22f), new Vector2(0.07f, 0.88f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(16, 0));
        MakeImage(p2, "StandArm", new Color(0.32f, 0.34f, 0.38f),
            new Vector2(0.07f, 0.86f), new Vector2(0.22f, 0.88f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        var gauge = MakePanel(p2, "GaugeBody", new Color(0.90f, 0.78f, 0.28f),
            new Vector2(0.20f, 0.78f), new Vector2(0.20f, 0.78f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150, 120));
        var gaugeRt = gauge.GetComponent<RectTransform>();
        MakeText(gaugeRt, "ScaleCaption", "0 – 5 N", 20, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -6), new Vector2(140, 26), Color.black);

        needleImage = MakeImage(gaugeRt, "Needle", new Color(0.15f, 0.15f, 0.15f),
            new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(8, 42));
        gaugeReadout = MakeText(gaugeRt, "Readout", "0.0 N", 28, TextAnchor.LowerCenter,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 8), new Vector2(140, 36), Color.black);
        gaugeReadout.fontStyle = FontStyle.Bold;

        hangingAssembly = new GameObject("HangingAssembly", typeof(RectTransform)).GetComponent<RectTransform>();
        hangingAssembly.SetParent(p2, false);
        SetAnchored(hangingAssembly, new Vector2(0.20f, 0.62f), new Vector2(0.20f, 0.62f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(80, 220));

        MakeImage(hangingAssembly, "String", new Color(0.75f, 0.76f, 0.78f),
            new Vector2(0.5f, 1), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 1), Vector2.zero, new Vector2(4, 0));

        cubeObject = MakePanel(hangingAssembly, "MetalCube", new Color(0.62f, 0.66f, 0.72f),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(70, 70));
        cubeRt = cubeObject.GetComponent<RectTransform>();
        MakeImage(cubeRt, "CentreLine", new Color(0.12f, 0.12f, 0.14f),
            new Vector2(0, 0.48f), new Vector2(1, 0.54f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        cubeObject.SetActive(false);
    }

    private void BuildEurekaCan(RectTransform p2)
    {
        var can = MakePanel(p2, "EurekaCan", new Color(0.70f, 0.82f, 0.88f, 0.95f),
            new Vector2(0.46f, 0.42f), new Vector2(0.46f, 0.42f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(170, 210));
        var canRt = can.GetComponent<RectTransform>();

        eurekaWater = MakeImage(canRt, "Water", ColWater,
            new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.78f), new Vector2(0.5f, 0), Vector2.zero, Vector2.zero);
        eurekaWater.type = Image.Type.Filled;
        eurekaWater.fillMethod = Image.FillMethod.Vertical;
        eurekaWater.fillOrigin = 0;
        eurekaWater.fillAmount = 1f;

        MakeImage(canRt, "Spout", new Color(0.60f, 0.72f, 0.78f),
            new Vector2(1f, 0.72f), new Vector2(1f, 0.72f), new Vector2(0f, 0.5f), new Vector2(28, 0), new Vector2(46, 16));

        overflowRt = MakeImage(p2, "Overflow", new Color(0.30f, 0.62f, 0.95f, 0.85f),
            new Vector2(0.56f, 0.50f), new Vector2(0.56f, 0.50f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(10, 70)).rectTransform;
        overflowRt.gameObject.SetActive(false);

        MakeText(canRt, "CanLabel", "Eureka Can", 16, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, 18), new Vector2(160, 24), ColText);
    }

    private void BuildBeaker(RectTransform p2)
    {
        beakerObject = MakePanel(p2, "Beaker", new Color(0.78f, 0.90f, 0.94f, 0.55f),
            new Vector2(0.64f, 0.28f), new Vector2(0.64f, 0.28f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100, 110));
        var bRt = beakerObject.GetComponent<RectTransform>();
        beakerWater = MakeImage(bRt, "Water", ColWater,
            new Vector2(0.1f, 0.08f), new Vector2(0.9f, 0.9f), new Vector2(0.5f, 0), Vector2.zero, Vector2.zero);
        beakerWater.type = Image.Type.Filled;
        beakerWater.fillMethod = Image.FillMethod.Vertical;
        beakerWater.fillOrigin = 0;
        beakerWater.fillAmount = 0.06f;
        MakeText(bRt, "BeakerLabel", "Beaker", 16, TextAnchor.LowerCenter,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, -22), new Vector2(100, 22), ColText);
        beakerObject.SetActive(false);
    }

    private void BuildReadingsCard(RectTransform p2)
    {
        var card = MakePanel(p2, "ReadingsCard", new Color(0.10f, 0.16f, 0.22f, 0.92f),
            new Vector2(0.86f, 0.62f), new Vector2(0.86f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360, 280));
        var rt = card.GetComponent<RectTransform>();
        MakeText(rt, "ReadTitle", "Live Readings", 24, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(320, 34), ColAccent);

        liveSpringText = MakeText(rt, "LiveSpring", "Spring balance: — N", 24, TextAnchor.MiddleLeft,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(10, 50), new Vector2(320, 36), ColText);
        liveBeakerText = MakeText(rt, "LiveBeaker", "Beaker: — N", 24, TextAnchor.MiddleLeft,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(10, 12), new Vector2(320, 36), ColText);
        liveUpthrustText = MakeText(rt, "LiveUpthrust", "Upthrust: — N", 24, TextAnchor.MiddleLeft,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(10, -26), new Vector2(320, 36), ColGood);
        liveDisplacedText = MakeText(rt, "LiveDisplaced", "Displaced water: — N", 24, TextAnchor.MiddleLeft,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(10, -64), new Vector2(320, 36), ColGood);
    }

    private void BuildPhase3(RectTransform parent)
    {
        phase3Panel = MakePanel(parent, "Phase3_Table", ColPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(1680, 740));
        phase3Panel.SetActive(false);
        var p3 = phase3Panel.GetComponent<RectTransform>();

        MakeText(p3, "P3Title", "PHASE 3 — Observation Table", 28, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(900, 36), ColAccent);

        wAirText = MakeText(p3, "WAir", "W (air) = 1.2 N", 18, TextAnchor.MiddleLeft,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -58), new Vector2(360, 26), ColText);
        wEmptyText = MakeText(p3, "WEmpty", "W (empty beaker) = 1.3 N", 18, TextAnchor.MiddleLeft,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(420, -58), new Vector2(420, 26), ColText);

        tableHintText = MakeText(p3, "TableHint",
            "Drag recorded answers into the correct cells.   Upthrust = W_air − W_apparent",
            18, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -88), new Vector2(1400, 26), new Color(0.8f, 0.88f, 0.92f));

        string[] headers = { "Stage", "Spring balance (N)", "Beaker + water (N)", "Upthrust (N)", "Displaced water (N)" };
        float[] xs = { -640, -320, 0, 320, 640 };
        for (int i = 0; i < headers.Length; i++)
        {
            MakeText(p3, "H" + i, headers[i], 20, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(xs[i], 210), new Vector2(280, 36), ColAccent);
        }

        var cells = new UpthrustObservationTableUI.TableCell[16];
        string[] stageNames = { "(a) Near surface", "(b) Half submerged", "(c) Fully immersed", "(d) Deeper" };
        int cellIndex = 0;
        for (int row = 0; row < 4; row++)
        {
            float y = 150 - row * 70;
            MakeText(p3, "Stage" + row, stageNames[row], 20, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(xs[0], y), new Vector2(260, 52), ColText);

            for (int col = 0; col < 4; col++)
            {
                var drop = MakeDropCell(p3, $"Cell_{row}_{col}", new Vector2(xs[col + 1], y), new Vector2(240, 56));
                cells[cellIndex] = new UpthrustObservationTableUI.TableCell
                {
                    stageIndex = row,
                    column = (UpthrustObservationTableUI.Column)col,
                    valueLabel = drop.label,
                    background = drop.background,
                    dropCell = drop.cell
                };
                cellIndex++;
            }
        }

        MakeText(p3, "TrayTitle", "Recorded answers — drag into the table", 22, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -150), new Vector2(1000, 32), ColAccent);

        var trayGo = new GameObject("AnswerTray", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        trayGo.transform.SetParent(p3, false);
        answerTray = trayGo.GetComponent<RectTransform>();
        SetAnchored(answerTray, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -210), new Vector2(1500, 70));
        var h = trayGo.GetComponent<HorizontalLayoutGroup>();
        h.spacing = 12;
        h.childAlignment = TextAnchor.MiddleCenter;
        h.childControlWidth = false;
        h.childControlHeight = false;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = false;
        var fitter = trayGo.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        submitTableButton = MakeButton(p3, "SubmitTable", "Submit Observation Table", new Vector2(0, -300), new Vector2(420, 58), ColDisabled);
        submitTableButton.interactable = false;

        var stash = phase3Panel.GetComponent<UpthrustTableCellHolder>() ?? phase3Panel.AddComponent<UpthrustTableCellHolder>();
        stash.Cells = cells;
    }

    private void BuildEndScreen(RectTransform parent)
    {
        endPanel = MakePanel(parent, "EndScreen", new Color(0.06f, 0.08f, 0.12f, 0.94f), true);
        endPanel.SetActive(false);
        var endRt = endPanel.GetComponent<RectTransform>();

        var box = MakePanel(endRt, "ResultBox", ColPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1760, 920));
        var boxRt = box.GetComponent<RectTransform>();

        MakeText(boxRt, "EndTitle", "Results — Your answers vs Correct answers", 32, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -16), new Vector2(1400, 44), ColAccent);

        BuildReviewTable(boxRt, "YourTable", "Your answers", -430, true);
        BuildReviewTable(boxRt, "CorrectTable", "Correct answers", 430, false);

        finalScoreText = MakeText(boxRt, "FinalScore", "Total Score: 0 / 100", 32, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 200), new Vector2(700, 42), ColText);
        finalScoreText.fontStyle = FontStyle.Bold;

        correctText = MakeText(boxRt, "Correct", "Correct Choices: 0", 22, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-280, 158), new Vector2(360, 32), ColGood);
        mistakeText = MakeText(boxRt, "Mistakes", "Incorrect Mistakes: 0", 22, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(280, 158), new Vector2(360, 32), ColBad);
        gradeText = MakeText(boxRt, "Grade", "Grade: —", 24, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 120), new Vector2(520, 32), ColAccent);
        starsText = MakeText(boxRt, "StarsText", "☆ ☆ ☆", 40, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 80), new Vector2(400, 40), new Color(1f, 0.85f, 0.2f));

        restartButton = MakeButton(boxRt, "Restart", "Restart Practical", new Vector2(0, 0), new Vector2(300, 56), ColAccent);
        var restartRt = restartButton.GetComponent<RectTransform>();
        restartRt.anchorMin = new Vector2(0.5f, 0);
        restartRt.anchorMax = new Vector2(0.5f, 0);
        restartRt.anchoredPosition = new Vector2(0, 32);
    }

    private void BuildReviewTable(RectTransform parent, string name, string title, float x, bool studentSide)
    {
        var table = MakePanel(parent, name, new Color(0.12f, 0.18f, 0.24f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 70), new Vector2(820, 520));
        var rt = table.GetComponent<RectTransform>();

        MakeText(rt, name + "Title", title, 24, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(700, 36), ColAccent);

        string[] headers = { "Stage", "Spring", "Beaker", "Upthrust", "Displaced" };
        float[] xs = { -320, -160, -20, 140, 300 };
        for (int i = 0; i < headers.Length; i++)
        {
            MakeText(rt, name + "H" + i, headers[i], 18, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(xs[i], 190), new Vector2(140, 28), ColAccent);
        }

        string[] stages = { "(a)", "(b)", "(c)", "(d)" };
        string prefix = studentSide ? "Your" : "Correct";
        for (int row = 0; row < 4; row++)
        {
            float y = 130 - row * 80;
            MakeText(rt, name + "S" + row, stages[row], 20, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(xs[0], y), new Vector2(120, 48), ColText);

            for (int col = 0; col < 4; col++)
            {
                var cell = MakePanel(rt, prefix + "_" + row + "_" + col + "_bg",
                    studentSide ? new Color(0.22f, 0.30f, 0.38f) : CellGoodPreview(),
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(xs[col + 1], y), new Vector2(130, 52));
                var label = MakeText(cell.GetComponent<RectTransform>(), prefix + "_" + row + "_" + col,
                    studentSide ? "—" : UpthrustPracticalData.FormatNewton(UpthrustObservationTableUI.ExpectedValue(row, (UpthrustObservationTableUI.Column)col)) + " N",
                    22, TextAnchor.MiddleCenter,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120, 44), Color.white);
                var lrt = label.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = new Vector2(4, 2);
                lrt.offsetMax = new Vector2(-4, -2);
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 16;
                label.resizeTextMaxSize = 24;
            }
        }
    }

    private static Color CellGoodPreview() => new Color(0.16f, 0.42f, 0.26f, 1f);

    private void CacheUiRefs()
    {
        var canvas = transform.Find("Canvas");
        if (canvas == null) return;

        if (hudBar == null) hudBar = canvas.Find("HUD")?.gameObject;
        if (phase1Panel == null) phase1Panel = canvas.Find("Phase1_Selection")?.gameObject;
        if (phase2Panel == null) phase2Panel = canvas.Find("Phase2_Lab")?.gameObject;
        if (phase3Panel == null) phase3Panel = canvas.Find("Phase3_Table")?.gameObject;
        if (endPanel == null) endPanel = canvas.Find("EndScreen")?.gameObject;

        if (scoreText == null) scoreText = FindText(canvas, "Score");
        if (instructionText == null) instructionText = FindText(canvas, "Instruction");
        if (progressText == null) progressText = FindText(canvas, "Progress");
        if (feedbackText == null) feedbackText = FindText(canvas, "Feedback");

        if (startPhase2Button == null && phase1Panel != null)
            startPhase2Button = phase1Panel.transform.Find("StartPhase2")?.GetComponent<Button>();
        if (startButtonLabel == null && startPhase2Button != null)
            startButtonLabel = startPhase2Button.GetComponentInChildren<Text>(true);

        if (phase2Panel != null)
        {
            var p2 = phase2Panel.transform;
            if (stepActionsRoot == null) stepActionsRoot = p2.Find("StepActions")?.gameObject;
            if (hangingAssembly == null) hangingAssembly = p2.Find("HangingAssembly") as RectTransform;
            if (cubeObject == null) cubeObject = p2.Find("HangingAssembly/MetalCube")?.gameObject;
            if (cubeRt == null && cubeObject != null) cubeRt = cubeObject.GetComponent<RectTransform>();
            if (beakerObject == null) beakerObject = p2.Find("Beaker")?.gameObject;
            if (beakerWater == null) beakerWater = p2.Find("Beaker/Water")?.GetComponent<Image>();
            if (eurekaWater == null) eurekaWater = p2.Find("EurekaCan/Water")?.GetComponent<Image>();
            if (overflowRt == null) overflowRt = p2.Find("Overflow") as RectTransform;
            if (stageLabel == null) stageLabel = FindText(p2, "StageLabel");
            if (liveSpringText == null) liveSpringText = FindText(p2, "LiveSpring");
            if (liveBeakerText == null) liveBeakerText = FindText(p2, "LiveBeaker");
            if (liveUpthrustText == null) liveUpthrustText = FindText(p2, "LiveUpthrust");
            if (liveDisplacedText == null) liveDisplacedText = FindText(p2, "LiveDisplaced");
            if (needleImage == null) needleImage = p2.Find("GaugeBody/Needle")?.GetComponent<Image>();
            if (gaugeReadout == null) gaugeReadout = FindText(p2, "Readout");
        }

        if (phase3Panel != null)
        {
            if (submitTableButton == null)
                submitTableButton = phase3Panel.transform.Find("SubmitTable")?.GetComponent<Button>();
            if (tableHintText == null) tableHintText = FindText(phase3Panel.transform, "TableHint");
            if (wAirText == null) wAirText = FindText(phase3Panel.transform, "WAir");
            if (wEmptyText == null) wEmptyText = FindText(phase3Panel.transform, "WEmpty");
            if (answerTray == null)
                answerTray = phase3Panel.transform.Find("AnswerTray") as RectTransform;
        }

        if (endPanel != null)
        {
            if (restartButton == null)
                restartButton = endPanel.transform.Find("ResultBox/Restart")?.GetComponent<Button>();
            if (finalScoreText == null) finalScoreText = FindText(endPanel.transform, "FinalScore");
            if (correctText == null) correctText = FindText(endPanel.transform, "Correct");
            if (mistakeText == null) mistakeText = FindText(endPanel.transform, "Mistakes");
            if (gradeText == null) gradeText = FindText(endPanel.transform, "Grade");
            if (starsText == null) starsText = FindText(endPanel.transform, "StarsText");
        }
    }

    private void BindManagers()
    {
        uiManager.Configure(scoreText, instructionText, progressText, feedbackText,
            endPanel, finalScoreText, correctText, mistakeText, gradeText, starsText, restartButton);

        springGauge.Configure(needleImage != null ? needleImage.rectTransform : null, gaugeReadout, null, null);
        springGauge.SetReading(0f, true);

        labVisuals.Configure(cubeRt, hangingAssembly, beakerWater, eurekaWater, overflowRt,
            beakerObject, cubeObject, stageLabel);

        var items = new List<UpthrustApparatusItem>();
        if (phase1Panel != null)
        {
            var phase1Img = phase1Panel.GetComponent<Image>();
            if (phase1Img != null) phase1Img.raycastTarget = false;

            items.AddRange(phase1Panel.GetComponentsInChildren<UpthrustApparatusItem>(true));
            foreach (var item in items)
            {
                if (item != null)
                    item.EnsureClickable();
            }
        }

        equipmentSelector.Configure(phase1Panel, null, startPhase2Button, startButtonLabel, phase2Panel, items);

        UpthrustObservationTableUI.TableCell[] cells = null;
        if (phase3Panel != null)
        {
            var stash = phase3Panel.GetComponent<UpthrustTableCellHolder>();
            cells = stash != null ? stash.Cells : null;
        }

        observationTable.Configure(phase3Panel, submitTableButton, tableHintText, wAirText, wEmptyText, cells, answerTray);

        practicalManager.Configure(springGauge, labVisuals, observationTable, phase2Panel, stepActionsRoot,
            liveSpringText, liveBeakerText, liveUpthrustText, liveDisplacedText);

        practicalManager.OnStepChanged -= HandleStepChanged;
        practicalManager.OnStepChanged += HandleStepChanged;

        scoreManager.ResetScore();
    }

    private void ShowPhase1()
    {
        if (phase1Panel != null) phase1Panel.SetActive(true);
        if (phase2Panel != null) phase2Panel.SetActive(false);
        if (phase3Panel != null) phase3Panel.SetActive(false);
        if (endPanel != null) endPanel.SetActive(false);

        uiManager.ShowStepInstruction(1, 4, "Phase 1: Select the 5 correct apparatus items for measuring upthrust.");
        uiManager.UpdateSelectionProgress(0, UpthrustPracticalData.CorrectApparatusCount);
        if (hudBar != null) hudBar.transform.SetAsLastSibling();
    }

    private void HandleStepChanged(int index, string instruction)
    {
        BuildStepChoices(index);
        if (progressText != null)
            progressText.text = $"Step {index + 1} of {UpthrustPracticalData.PracticalStepCount} — tap the correct action";
    }

    private void BuildStepChoices(int step)
    {
        if (stepActionsRoot == null) return;
        var row = stepActionsRoot.transform.Find("ChoiceRow") as RectTransform;
        if (row == null) return;

        for (int i = row.childCount - 1; i >= 0; i--)
            Destroy(row.GetChild(i).gameObject);

        var choices = GetChoicesForStep(step);
        Shuffle(choices);
        foreach (var choice in choices)
            CreateStepChoice(row, choice);
    }

    struct StepChoice
    {
        public string label;
        public string iconId;
        public bool correct;
    }

    private static List<StepChoice> GetChoicesForStep(int step)
    {
        switch (step)
        {
            case 0:
                return new List<StepChoice>
                {
                    new StepChoice { label = "Hang metal cube on spring balance", iconId = "cube", correct = true },
                    new StepChoice { label = "Hang the thermometer", iconId = "thermometer", correct = false },
                    new StepChoice { label = "Connect the voltmeter", iconId = "voltmeter", correct = false },
                };
            case 1:
                return new List<StepChoice>
                {
                    new StepChoice { label = "Place empty beaker under spout", iconId = "beaker", correct = true },
                    new StepChoice { label = "Put beaker on the cube", iconId = "beaker", correct = false },
                    new StepChoice { label = "Place wooden block under spout", iconId = "block", correct = false },
                };
            case 2:
                return new List<StepChoice>
                {
                    new StepChoice { label = "Lower cube to Stage (a) near surface", iconId = "cube", correct = true },
                    new StepChoice { label = "Push cube to the bottom now", iconId = "cube", correct = false },
                    new StepChoice { label = "Remove the Eureka can", iconId = "eureka", correct = false },
                };
            case 3:
                return new List<StepChoice>
                {
                    new StepChoice { label = "Lower cube to Stage (b) half submerged", iconId = "cube", correct = true },
                    new StepChoice { label = "Lift cube back into air", iconId = "spring", correct = false },
                    new StepChoice { label = "Heat the water with a flame", iconId = "thermometer", correct = false },
                };
            case 4:
                return new List<StepChoice>
                {
                    new StepChoice { label = "Fully immerse cube — Stage (c)", iconId = "cube", correct = true },
                    new StepChoice { label = "Empty the beaker", iconId = "beaker", correct = false },
                    new StepChoice { label = "Replace cube with wooden block", iconId = "block", correct = false },
                };
            default:
                return new List<StepChoice>
                {
                    new StepChoice { label = "Lower cube deeper — Stage (d)", iconId = "cube", correct = true },
                    new StepChoice { label = "Expect a larger upthrust deeper", iconId = "spring", correct = false },
                    new StepChoice { label = "Raise cube completely out of water", iconId = "cube", correct = false },
                };
        }
    }

    private void CreateStepChoice(RectTransform row, StepChoice choice)
    {
        Color bg = ColCard;
        var card = MakePanel(row, "Choice_" + choice.iconId, bg,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        var cardImg = card.GetComponent<Image>();
        cardImg.raycastTarget = true;

        var le = card.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        le.minWidth = 220;
        le.minHeight = 100;

        var cardRt = card.GetComponent<RectTransform>();
        var icon = MakeImage(cardRt, "Icon", new Color(1f, 1f, 1f, 0.04f),
            new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(64, 64));
        icon.sprite = UpthrustIconFactory.WhiteSprite;
        icon.raycastTarget = false;
        UpthrustIconFactory.BuildIcon(icon.rectTransform, choice.iconId);

        var label = MakeText(cardRt, "Label", choice.label, 22, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 28), new Vector2(0, 70), Color.white);
        var labelRt = label.GetComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0.06f, 0.06f);
        labelRt.anchorMax = new Vector2(0.94f, 0.48f);
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;
        label.fontStyle = FontStyle.Bold;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 16;
        label.resizeTextMaxSize = 24;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.raycastTarget = false;

        var btn = card.AddComponent<Button>();
        btn.targetGraphic = cardImg;
        bool isCorrect = choice.correct;
        btn.onClick.AddListener(() => practicalManager.TryPerformCurrentStepAction(isCorrect));
    }

    private void EnsureEventSystem()
    {
        var existing = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        EventSystem keep = null;
        foreach (var es in existing)
        {
            if (keep == null) keep = es;
            else
            {
                if (Application.isPlaying) Destroy(es.gameObject);
                else DestroyImmediate(es.gameObject);
            }
        }

        if (keep == null)
        {
            var obj = new GameObject("EventSystem");
            keep = obj.AddComponent<EventSystem>();
        }

#if ENABLE_INPUT_SYSTEM
        if (keep.GetComponent<InputSystemUIInputModule>() == null)
            keep.gameObject.AddComponent<InputSystemUIInputModule>();
#else
        if (keep.GetComponent<StandaloneInputModule>() == null)
            keep.gameObject.AddComponent<StandaloneInputModule>();
#endif
    }

    private (UpthrustObservationDropCell cell, Text label, Image background) MakeDropCell(RectTransform parent, string name, Vector2 pos, Vector2 size)
    {
        var go = MakePanel(parent, name, new Color(0.22f, 0.30f, 0.38f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
        var img = go.GetComponent<Image>();
        img.raycastTarget = true;

        var label = MakeText(go.GetComponent<RectTransform>(), "Value", "drop here", 26, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size - new Vector2(8, 6), new Color(1f, 1f, 1f, 0.45f));
        var labelRt = label.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = new Vector2(6, 4);
        labelRt.offsetMax = new Vector2(-6, -4);
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 18;
        label.resizeTextMaxSize = 28;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.raycastTarget = false;

        var drop = go.AddComponent<UpthrustObservationDropCell>();
        drop.Configure(0, UpthrustObservationTableUI.Column.SpringBalance, label, img);
        return (drop, label, img);
    }

    private InputField MakeInput(RectTransform parent, string name, Vector2 pos, Vector2 size)
    {
        var go = MakePanel(parent, name, new Color(0.22f, 0.30f, 0.38f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
        var img = go.GetComponent<Image>();
        img.raycastTarget = true;

        var text = MakeText(go.GetComponent<RectTransform>(), "Text", "", 22, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size - new Vector2(12, 8), ColText);
        text.raycastTarget = false;
        text.supportRichText = false;

        var placeholder = MakeText(go.GetComponent<RectTransform>(), "Placeholder", "N", 18, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size - new Vector2(12, 8), new Color(1, 1, 1, 0.35f));
        placeholder.raycastTarget = false;

        var input = go.AddComponent<InputField>();
        input.textComponent = text;
        input.placeholder = placeholder;
        input.contentType = InputField.ContentType.DecimalNumber;
        input.lineType = InputField.LineType.SingleLine;
        input.targetGraphic = img;
        return input;
    }

    private static Text FindText(Transform root, string name)
    {
        if (root == null) return null;
        foreach (var t in root.GetComponentsInChildren<Text>(true))
            if (t.gameObject.name == name) return t;
        return null;
    }

    private GameObject MakePanel(RectTransform parent, string name, Color color, bool stretch = false)
    {
        return MakePanel(parent, name, color, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, stretch);
    }

    private GameObject MakePanel(RectTransform parent, string name, Color color,
        Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size, bool stretch = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = true;
        var rt = go.GetComponent<RectTransform>();
        if (stretch)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        else SetAnchored(rt, aMin, aMax, pivot, pos, size);
        return go;
    }

    private Image MakeImage(RectTransform parent, string name, Color color, bool stretch = false)
    {
        return MakeImage(parent, name, color, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, stretch);
    }

    private Image MakeImage(RectTransform parent, string name, Color color,
        Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size, bool stretch = false)
    {
        return MakePanel(parent, name, color, aMin, aMax, pivot, pos, size, stretch).GetComponent<Image>();
    }

    private Text MakeText(RectTransform parent, string name, string content, int fontSize, TextAnchor align,
        Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Color color)
    {
        if (uiFont == null) LoadResources();

        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.text = content;
        t.font = uiFont;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        t.supportRichText = true;
        SetAnchored(go.GetComponent<RectTransform>(), aMin, aMax, new Vector2(0.5f, 0.5f), pos, size);
        return t;
    }

    private Button MakeButton(RectTransform parent, string name, string label, Vector2 pos, Vector2 size, Color color)
    {
        var go = MakePanel(parent, name, color,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
        var img = go.GetComponent<Image>();
        img.raycastTarget = true;
        var t = MakeText(go.GetComponent<RectTransform>(), "Label", label, 22, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size - new Vector2(12, 12), Color.black);
        t.fontStyle = FontStyle.Bold;
        t.resizeTextForBestFit = true;
        t.resizeTextMinSize = 16;
        t.resizeTextMaxSize = 26;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        return btn;
    }

    private static void SetAnchored(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}

public class UpthrustUIRefsHolder : MonoBehaviour
{
    public int UiVersion;
}

public class UpthrustTableCellHolder : MonoBehaviour
{
    public UpthrustObservationTableUI.TableCell[] Cells;
}
