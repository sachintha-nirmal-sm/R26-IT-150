using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Grade 9 Reflection: dispersion of white light through a glass prism.
/// Runtime UI only — no leftover ScoreManager / UIManager types.
/// </summary>
public class PrismSceneRuntimeBuilder : MonoBehaviour
{
    public const int RequiredUiVersion = 9;

    Font uiFont;

    [Header("Runtime UI Refs (auto-filled by builder)")]
    [SerializeField] Text scoreText;
    [SerializeField] Text percentText;
    [SerializeField] Text instructionText;
    [SerializeField] Text progressText;
    [SerializeField] Text feedbackText;
    [SerializeField] Text startButtonLabel;
    [SerializeField] Button startPhase2Button;
    [SerializeField] Button restartButton;
    [SerializeField] GameObject hudBar;
    [SerializeField] GameObject phase1Panel;
    [SerializeField] GameObject phase2Panel;
    [SerializeField] GameObject endPanel;
    [SerializeField] GameObject stepActionsRoot;

    Text finalScoreText, correctText, mistakeText, gradeText, timerText;
    GameObject prismObj, screenObj, torchGlow, lightBeam, spectrumBand, opticsObj;
    Image[] starImages = new Image[3];
    bool flutterSent;

    int correctSelected;
    const int RequiredCorrect = 5;
    // Local score out of 100 (does not depend on ScoreManager instance bugs)
    const int PointsPerApparatus = 10; // 5 × 10 = 50
    const int PointsPerStep = 10;      // 5 × 10 = 50
    const int MistakePenalty = 5;
    int stepsCompleted;
    int totalMistakes;
    int finalScore100 = -1;

    readonly HashSet<string> selectedIds = new HashSet<string>();
    readonly HashSet<int> stepMistakes = new HashSet<int>();
    int currentStep;
    bool wired;
    bool phase2Active;

    static readonly Color ColBg = new Color(0.12f, 0.18f, 0.24f, 1f);
    static readonly Color ColPanel = new Color(0.18f, 0.26f, 0.34f, 0.96f);
    static readonly Color ColTable = new Color(0.45f, 0.30f, 0.18f, 1f);
    static readonly Color ColTableTop = new Color(0.55f, 0.38f, 0.22f, 1f);
    static readonly Color ColAccent = new Color(0.20f, 0.72f, 0.78f, 1f);
    static readonly Color ColGood = new Color(0.30f, 0.78f, 0.42f, 1f);
    static readonly Color ColBad = new Color(0.90f, 0.32f, 0.32f, 1f);
    static readonly Color ColCard = new Color(0.26f, 0.34f, 0.42f, 1f);
    static readonly Color ColText = new Color(0.95f, 0.96f, 0.97f, 1f);
    static readonly Color ColDisabled = new Color(0.35f, 0.38f, 0.42f, 1f);

    static readonly Color[] Roygbiv =
    {
        new Color(1f, 0.15f, 0.12f), new Color(1f, 0.55f, 0.05f), new Color(1f, 0.92f, 0.15f),
        new Color(0.2f, 0.8f, 0.25f), new Color(0.15f, 0.45f, 1f), new Color(0.35f, 0.2f, 0.85f),
        new Color(0.6f, 0.25f, 0.9f)
    };

    readonly string[] stepInstructions =
    {
        "Step 1: Place the Glass Prism onto the Wooden Table.",
        "Step 2: Set up the Plane Mirror and Cardboard with slit in front of the Light Source.",
        "Step 3: Turn ON the Light Source to send a thin white beam onto the Prism.",
        "Step 4: Position the White Screen behind the Prism to catch the spectrum.",
        "Step 5: Observe the ROYGBIV spectrum and record your observation."
    };

    struct ItemDef
    {
        public string id, label;
        public bool correct;
        public Color color;
    }

    readonly ItemDef[] trayItems =
    {
        new ItemDef { id = "prism", label = "Glass Prism\n(60°–60°–60°)", correct = true, color = new Color(0.7f, 0.9f, 1f) },
        new ItemDef { id = "screen", label = "White\nScreen", correct = true, color = Color.white },
        new ItemDef { id = "cardboard", label = "Cardboard\nwith Slit", correct = true, color = new Color(0.55f, 0.35f, 0.2f) },
        new ItemDef { id = "mirror", label = "Plane\nMirror", correct = true, color = new Color(0.75f, 0.8f, 0.85f) },
        new ItemDef { id = "torch", label = "Light Source\n/ Torch", correct = true, color = new Color(1f, 0.9f, 0.35f) },
        new ItemDef { id = "lens", label = "Convex\nLens", correct = false, color = new Color(0.6f, 0.75f, 0.9f) },
        new ItemDef { id = "spring", label = "Spring\nBalance", correct = false, color = new Color(0.85f, 0.65f, 0.3f) },
        new ItemDef { id = "beaker", label = "Beaker", correct = false, color = new Color(0.65f, 0.85f, 0.9f) },
        new ItemDef { id = "tape", label = "Measuring\nTape", correct = false, color = new Color(0.95f, 0.8f, 0.3f) },
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
            wired = false;
            Debug.Log("Prism Practical: UI built (v" + RequiredUiVersion + "). Press Play.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Prism Practical BUILD FAILED: " + ex.Message + "\n" + ex.StackTrace);
        }
    }

    public void WireOnPlay()
    {
        LoadResources();
        EnsureManagersRuntime();
        CacheUiRefs();
        ApplyFontEverywhere();
        WireButtons();
        ResetRuntimeState();
        ShowPhase1();
        RefreshScoreHud();
        wired = true;
        Debug.Log("Prism Practical: ready — select 5 correct items, then Start Practical.");
    }

    private void Awake()
    {
        TimerManager.HideOnGui = true;
        FlutterBridge.EnsureInstance();
        TimerManager.EnsureInstance();
        if (!Application.isPlaying) return;
        if (!HasExistingBuild())
            BuildScenePersistent();
    }

    private void Start()
    {
        if (!HasExistingBuild())
            BuildScenePersistent();
        WireOnPlay();

        TimerManager.OnExpired += CompleteOnTimeout;
        TimerManager.OnTick += UpdateTimerHud;
        int limit = FlutterBridge.Instance != null ? FlutterBridge.Instance.DurationSeconds : 600;
        if (limit <= 0)
        {
            limit = 600;
        }

        TimerManager.EnsureInstance()?.StartTimer(limit);
        UpdateTimerHud(TimerManager.Instance != null ? TimerManager.Instance.RemainingSeconds : limit);
    }

    private void OnDestroy()
    {
        TimerManager.OnExpired -= CompleteOnTimeout;
        TimerManager.OnTick -= UpdateTimerHud;
        TimerManager.HideOnGui = false;
    }

    // ═══════════════ BUILD ═══════════════

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
    }

    private void LoadResources()
    {
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null) uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (uiFont == null)
            uiFont = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial", "Helvetica", "Sans-serif" }, 24);
        if (uiFont == null)
            Debug.LogError("Prism: No UI font available — text will not show.");
    }

    private void ApplyFontEverywhere()
    {
        if (uiFont == null) return;
        var canvas = transform.Find("Canvas");
        if (canvas == null) return;
        foreach (var t in canvas.GetComponentsInChildren<Text>(true))
        {
            t.font = uiFont;
            if (t.fontSize < 16) t.fontSize = 18;
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
    }

    private void EnsureManagersRuntime()
    {
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
        MakeImage(canvasRt, "Background", ColBg, true);
        BuildHud(canvasRt);
        BuildPhase1(canvasRt);
        BuildPhase2(canvasRt);
        BuildEndScreen(canvasRt);

        var holder = canvasGo.GetComponent<PrismUIRefsHolder>() ?? canvasGo.AddComponent<PrismUIRefsHolder>();
        holder.UiVersion = RequiredUiVersion;

        // Keep HUD above Phase1 so Score is always visible
        if (hudBar != null)
            hudBar.transform.SetAsLastSibling();

        ApplyFontEverywhere();
    }

    private void BuildHud(RectTransform parent)
    {
        // Built first; re-parented to end of Canvas later so it stays on top.
        hudBar = MakePanel(parent, "HUD", new Color(0.08f, 0.12f, 0.16f, 1f),
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 96));
        var hudRt = hudBar.GetComponent<RectTransform>();
        hudRt.offsetMin = new Vector2(0, -96);
        hudRt.offsetMax = Vector2.zero;
        var hudImg = hudBar.GetComponent<Image>();
        if (hudImg != null) hudImg.raycastTarget = false;

        MakeText(hudRt, "Title", "Prism Practical — Dispersion of Light", 26, TextAnchor.MiddleLeft,
            new Vector2(0, 0.5f), new Vector2(0.45f, 0.5f), new Vector2(20, 0), new Vector2(700, 50), ColAccent);

        scoreText = MakeText(hudRt, "Score", "Score: 0 / 100", 30, TextAnchor.MiddleRight,
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-24, 12), new Vector2(360, 40), ColText);
        scoreText.fontStyle = FontStyle.Bold;
        percentText = scoreText;
        timerText = MakeText(hudRt, "Timer", "10:00", 24, TextAnchor.MiddleRight,
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-24, -22), new Vector2(360, 32), ColText);

        instructionText = MakeText(parent, "Instruction", "", 26, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -110), new Vector2(1700, 48), ColText);
        instructionText.fontStyle = FontStyle.Bold;
        progressText = MakeText(parent, "Progress", "", 22, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -152), new Vector2(1000, 36), new Color(0.75f, 0.85f, 0.9f));
        feedbackText = MakeText(parent, "Feedback", "", 24, TextAnchor.LowerCenter,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 28), new Vector2(1100, 42), ColGood);
        feedbackText.fontStyle = FontStyle.Bold;
    }

    private void BuildPhase1(RectTransform parent)
    {
        phase1Panel = MakePanel(parent, "Phase1_Selection", ColPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(1700, 720));
        var p1 = phase1Panel.GetComponent<RectTransform>();

        MakeText(p1, "P1Title", "PHASE 1 — Select Correct Apparatus", 34, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -12), new Vector2(1100, 48), ColAccent);
        MakeText(p1, "P1Hint", "Tap ALL 5 correct lab items. Wrong items lose marks (−5).", 22, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -54), new Vector2(1400, 36), new Color(0.8f, 0.85f, 0.9f));

        var grid = new GameObject("TrayGrid", typeof(RectTransform), typeof(GridLayoutGroup));
        grid.transform.SetParent(p1, false);
        var gridRt = grid.GetComponent<RectTransform>();
        SetAnchored(gridRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(1400, 480));
        var glg = grid.GetComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(240, 220);
        glg.spacing = new Vector2(16, 14);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 5;
        glg.childAlignment = TextAnchor.MiddleCenter;

        PrismIconFactory.ClearCache();
        foreach (var item in trayItems)
            CreateApparatusCard(gridRt, item);

        // Keep direct references — Find() was leaving Start button unwired.
        startPhase2Button = MakeButton(p1, "StartPhase2", "Select 5 correct items first...", new Vector2(0, -310), new Vector2(520, 72), ColDisabled);
        ApplyDisabledColors(startPhase2Button);
        startButtonLabel = startPhase2Button.GetComponentInChildren<Text>(true);
        if (startButtonLabel != null) startButtonLabel.fontSize = 26;
        startPhase2Button.interactable = false;
    }

    private void CreateApparatusCard(RectTransform parent, ItemDef item)
    {
        // NO Button component — Button.interactable=false was blocking later clicks.
        var card = MakePanel(parent, "Card_" + item.id, ColCard,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(240, 220));

        var cardRt = card.GetComponent<RectTransform>();
        var cardImg = card.GetComponent<Image>();
        cardImg.raycastTarget = true;

        var outline = card.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.15f);
        outline.effectDistance = new Vector2(2, -2);

        // Real apparatus icon (procedural image)
        var icon = MakeImage(cardRt, "Icon", Color.white,
            new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100, 100));
        icon.sprite = PrismIconFactory.GetSprite(item.id);
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        var nameText = MakeText(cardRt, "Label", item.label, 22, TextAnchor.LowerCenter,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(220, 70), ColText);
        nameText.fontStyle = FontStyle.Bold;
        nameText.resizeTextForBestFit = true;
        nameText.resizeTextMinSize = 16;
        nameText.resizeTextMaxSize = 24;
        nameText.horizontalOverflow = HorizontalWrapMode.Wrap;
        nameText.verticalOverflow = VerticalWrapMode.Truncate;
        nameText.raycastTarget = false;

        // Checkmark badge (hidden until selected)
        var check = MakeText(cardRt, "Check", "", 28, TextAnchor.UpperRight,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-10, -8), new Vector2(40, 40), ColGood);
        check.fontStyle = FontStyle.Bold;
        check.raycastTarget = false;

        var marker = card.AddComponent<PrismApparatusCard>();
        marker.Id = item.id;
        marker.IsCorrect = item.correct;
        marker.CardImage = cardImg;
        marker.CheckLabel = check;
        marker.Outline = outline;
        marker.NormalColor = ColCard;
    }

    private void BuildPhase2(RectTransform parent)
    {
        phase2Panel = MakePanel(parent, "Phase2_Lab", new Color(0.14f, 0.2f, 0.26f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -30), new Vector2(1760, 780));
        phase2Panel.SetActive(false);
        var p2 = phase2Panel.GetComponent<RectTransform>();

        MakeImage(p2, "Wall", new Color(0.22f, 0.32f, 0.40f),
            new Vector2(0, 0.35f), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        MakeImage(p2, "Table", ColTable,
            new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.48f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        MakeImage(p2, "TableTop", ColTableTop,
            new Vector2(0.05f, 0.44f), new Vector2(0.95f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        MakeText(p2, "TableLabel", "Wooden Table", 22, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), Vector2.zero, new Vector2(280, 36), new Color(1, 1, 1, 0.75f));

        // Lab props with icons (hidden until step completes)
        torchGlow = CreateLabProp(p2, "TorchGlow", "torch", "TORCH", new Vector2(0.12f, 0.58f), new Vector2(120, 100));
        torchGlow.SetActive(false);

        opticsObj = CreateLabProp(p2, "Optics", "mirror", "Mirror + Slit", new Vector2(0.28f, 0.58f), new Vector2(140, 100));
        opticsObj.SetActive(false);

        prismObj = CreateLabProp(p2, "Prism", "prism", "PRISM", new Vector2(0.48f, 0.58f), new Vector2(130, 120));
        prismObj.SetActive(false);

        screenObj = CreateLabProp(p2, "Screen", "screen", "SCREEN", new Vector2(0.80f, 0.60f), new Vector2(110, 160));
        screenObj.SetActive(false);

        lightBeam = MakePanel(p2, "WhiteBeam", Color.white,
            new Vector2(0.36f, 0.60f), new Vector2(0.36f, 0.60f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260, 10));
        lightBeam.SetActive(false);

        spectrumBand = new GameObject("SpectrumBand", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        spectrumBand.transform.SetParent(p2, false);
        var specRt = spectrumBand.GetComponent<RectTransform>();
        SetAnchored(specRt, new Vector2(0.74f, 0.60f), new Vector2(0.74f, 0.60f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(32, 160));
        var h = spectrumBand.GetComponent<HorizontalLayoutGroup>();
        h.childForceExpandHeight = true;
        h.childForceExpandWidth = true;
        h.spacing = 0;
        foreach (var c in Roygbiv)
        {
            var strip = new GameObject("Strip", typeof(RectTransform), typeof(Image));
            strip.transform.SetParent(specRt, false);
            strip.GetComponent<Image>().color = c;
        }
        spectrumBand.SetActive(false);

        MakeText(p2, "ObsNote", "", 18, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), Vector2.zero, new Vector2(1000, 40), ColAccent);

        // Step action bar — ONE clear choice row per step
        stepActionsRoot = MakePanel(p2, "StepActions", new Color(0.10f, 0.14f, 0.18f, 0.97f),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(1680, 150));
        var actionsRt = stepActionsRoot.GetComponent<RectTransform>();
        MakeText(actionsRt, "ActionsHint", "Choose the correct action for this step:", 20, TextAnchor.UpperLeft,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(16, -8), new Vector2(800, 30), new Color(0.85f, 0.92f, 0.96f));

        var row = new GameObject("ChoiceRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(actionsRt, false);
        var rowRt = row.GetComponent<RectTransform>();
        SetAnchored(rowRt, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        rowRt.offsetMin = new Vector2(16, 12);
        rowRt.offsetMax = new Vector2(-16, -36);
        var layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 16;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = true;
    }

    private void BuildEndScreen(RectTransform parent)
    {
        endPanel = MakePanel(parent, "EndScreen", new Color(0.08f, 0.1f, 0.14f, 0.94f), true);
        endPanel.SetActive(false);
        var endRt = endPanel.GetComponent<RectTransform>();

        var box = MakePanel(endRt, "ResultBox", ColPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 560));
        var boxRt = box.GetComponent<RectTransform>();

        MakeText(boxRt, "EndTitle", "Practical Complete!", 38, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(680, 54), ColAccent);

        finalScoreText = MakeText(boxRt, "FinalScore", "Total Score: 0 / 100", 36, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 90), new Vector2(620, 50), ColText);
        finalScoreText.fontStyle = FontStyle.Bold;

        correctText = MakeText(boxRt, "Correct", "Correct Choices: 0", 24, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 35), new Vector2(520, 40), ColGood);
        mistakeText = MakeText(boxRt, "Mistakes", "Incorrect Mistakes: 0", 24, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -5), new Vector2(520, 40), ColBad);
        gradeText = MakeText(boxRt, "Grade", "Grade: —", 26, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -50), new Vector2(520, 40), ColAccent);
        gradeText.fontStyle = FontStyle.Bold;

        // Stars as text (no missing sprite squares)
        var starsText = MakeText(boxRt, "StarsText", "☆ ☆ ☆", 44, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -110), new Vector2(400, 56), new Color(1f, 0.85f, 0.2f));
        starsText.fontStyle = FontStyle.Bold;

        MakeButton(boxRt, "Restart", "Restart Practical", new Vector2(0, -210), new Vector2(300, 64), ColAccent);
    }

    // ═══════════════ WIRE / PLAY ═══════════════

    private void CacheUiRefs()
    {
        var canvas = transform.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("Prism: Canvas missing — rebuild via Tools menu.");
            return;
        }

        // Only fill missing refs (BuildPhase1 already assigned Start button + score texts)
        if (scoreText == null) scoreText = FindText(canvas, "Score");
        if (timerText == null) timerText = FindText(canvas, "Timer");
        if (percentText == null) percentText = FindText(canvas, "Percent");
        if (instructionText == null) instructionText = FindText(canvas, "Instruction");
        if (progressText == null) progressText = FindText(canvas, "Progress");
        if (feedbackText == null) feedbackText = FindText(canvas, "Feedback");
        if (hudBar == null) hudBar = canvas.Find("HUD")?.gameObject;

        if (phase1Panel == null) phase1Panel = canvas.Find("Phase1_Selection")?.gameObject;
        if (phase2Panel == null) phase2Panel = canvas.Find("Phase2_Lab")?.gameObject;
        if (endPanel == null) endPanel = canvas.Find("EndScreen")?.gameObject;

        if (phase2Panel != null)
        {
            var p2 = phase2Panel.transform;
            if (prismObj == null) prismObj = p2.Find("Prism")?.gameObject;
            if (screenObj == null) screenObj = p2.Find("Screen")?.gameObject;
            if (torchGlow == null) torchGlow = p2.Find("TorchGlow")?.gameObject;
            if (lightBeam == null) lightBeam = p2.Find("WhiteBeam")?.gameObject;
            if (spectrumBand == null) spectrumBand = p2.Find("SpectrumBand")?.gameObject;
            if (opticsObj == null) opticsObj = p2.Find("Optics")?.gameObject;
            if (stepActionsRoot == null) stepActionsRoot = p2.Find("StepActions")?.gameObject;
        }

        ResolveStartButton();

        if (restartButton == null && endPanel != null)
            restartButton = endPanel.transform.Find("ResultBox/Restart")?.GetComponent<Button>();

        if (finalScoreText == null) finalScoreText = FindText(endPanel?.transform, "FinalScore");
        if (correctText == null) correctText = FindText(endPanel?.transform, "Correct");
        if (mistakeText == null) mistakeText = FindText(endPanel?.transform, "Mistakes");
        if (gradeText == null) gradeText = FindText(endPanel?.transform, "Grade");

        if (endPanel != null)
        {
            var box = endPanel.transform.Find("ResultBox");
            for (int i = 0; i < 3; i++)
                if (starImages[i] == null)
                    starImages[i] = box?.Find("Star" + i)?.GetComponent<Image>();
        }

        if (hudBar != null)
            hudBar.transform.SetAsLastSibling();

        Debug.Log($"Prism refs: startBtn={(startPhase2Button != null)} scoreTxt={(scoreText != null)} phase1={(phase1Panel != null)}");
    }

    private void ResolveStartButton()
    {
        if (startPhase2Button == null && phase1Panel != null)
        {
            var t = phase1Panel.transform.Find("StartPhase2");
            if (t != null) startPhase2Button = t.GetComponent<Button>();
        }

        if (startPhase2Button == null && phase1Panel != null)
        {
            foreach (var b in phase1Panel.GetComponentsInChildren<Button>(true))
            {
                if (b.gameObject.name == "StartPhase2")
                {
                    startPhase2Button = b;
                    break;
                }
            }
        }

        if (startButtonLabel == null && startPhase2Button != null)
            startButtonLabel = startPhase2Button.GetComponentInChildren<Text>(true);
    }

    private void WireButtons()
    {
        EnsureEventSystem();

        int wiredCards = 0;
        if (phase1Panel != null)
        {
            var phase1Bg = phase1Panel.GetComponent<Image>();
            if (phase1Bg != null) phase1Bg.raycastTarget = false;

            foreach (var card in phase1Panel.GetComponentsInChildren<PrismApparatusCard>(true))
            {
                // Ensure only the card root receives clicks (icons/text never steal them)
                card.EnsureClickable();
                card.Bind(OnApparatusClicked);
                wiredCards++;
            }

            var startImg = phase1Panel.transform.Find("StartPhase2")?.GetComponent<Image>();
            if (startImg != null) startImg.raycastTarget = true;
        }

        Debug.Log($"Prism: wired {wiredCards} apparatus cards for selection.");

        if (startPhase2Button != null)
        {
            var startImg = startPhase2Button.GetComponent<Image>();
            if (startImg != null) startImg.raycastTarget = true;

            startPhase2Button.onClick.RemoveAllListeners();
            startPhase2Button.onClick.AddListener(OnStartPhase2);
            ApplyDisabledColors(startPhase2Button);
            UpdateStartButtonState();
            Debug.Log("Prism: Start Practical button wired.");
        }
        else
        {
            Debug.LogError("Prism: StartPhase2 button not found!");
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() =>
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        }
    }

    private void ResetRuntimeState()
    {
        correctSelected = 0;
        selectedIds.Clear();
        stepMistakes.Clear();
        currentStep = 0;
        stepsCompleted = 0;
        totalMistakes = 0;
        finalScore100 = -1;
        flutterSent = false;
        phase2Active = false;
    }

    /// <summary>Always compute display score 0–100 from local counters.</summary>
    private int ComputeScore100()
    {
        if (finalScore100 >= 0) return finalScore100;

        int apparatusPoints = Mathf.Min(selectedIds.Count, RequiredCorrect) * PointsPerApparatus;
        int stepPoints = Mathf.Min(stepsCompleted, 5) * PointsPerStep;
        int penalty = totalMistakes * MistakePenalty;
        return Mathf.Clamp(apparatusPoints + stepPoints - penalty, 0, 100);
    }

    private void ShowPhase1()
    {
        phase2Active = false;
        if (phase1Panel != null) phase1Panel.SetActive(true);
        if (phase2Panel != null) phase2Panel.SetActive(false);
        if (endPanel != null) endPanel.SetActive(false);
        if (instructionText != null)
            instructionText.text = "Phase 1: Select the 5 correct apparatus items.";
        if (progressText != null)
            progressText.text = "Apparatus selected: 0 / 5";
        if (feedbackText != null) feedbackText.text = "";
        UpdateStartButtonState();
        RefreshScoreHud();
    }

    private void RefreshScoreHud()
    {
        if (scoreText == null)
        {
            var canvas = transform.Find("Canvas");
            if (canvas != null) scoreText = FindText(canvas, "Score");
        }
        if (scoreText != null)
        {
            scoreText.font = uiFont;
            scoreText.text = $"Score: {ComputeScore100()} / 100";
        }
    }

    // ═══════════════ PHASE 1 ═══════════════

    /// <summary>Public entry used by PrismApparatusCard click fallback.</summary>
    public void HandleApparatusCardClick(PrismApparatusCard card) => OnApparatusClicked(card);

    private void OnApparatusClicked(PrismApparatusCard card)
    {
        if (phase2Active) return;
        if (card == null) return;

        if (selectedIds.Contains(card.Id))
        {
            Flash("Already selected ✓", true);
            return;
        }

        Debug.Log($"Prism: clicked '{card.Id}' correct={card.IsCorrect}");

        if (card.IsCorrect)
        {
            selectedIds.Add(card.Id);
            correctSelected = selectedIds.Count;
            card.SetSelectedVisual(true);
            RefreshScoreHud();
            Flash($"Correct! +{PointsPerApparatus}   ({correctSelected}/5)   Score {ComputeScore100()}/100", true);
            if (progressText != null)
                progressText.text = $"Apparatus selected: {correctSelected} / {RequiredCorrect}";
            UpdateStartButtonState();

            if (correctSelected >= RequiredCorrect)
                Flash($"All 5 selected! Score {ComputeScore100()}/100 — press START PRACTICAL ▶", true);
        }
        else
        {
            totalMistakes++;
            RefreshScoreHud();
            Flash($"Wrong item! (−{MistakePenalty})   Score {ComputeScore100()}/100", false);
            card.FlashWrong(ColBad, ColCard);
        }
    }

    private void UpdateStartButtonState()
    {
        ResolveStartButton();

        if (startPhase2Button == null)
        {
            Debug.LogWarning("Prism: cannot update Start button — reference missing.");
            return;
        }

        correctSelected = selectedIds.Count;
        bool ready = correctSelected >= RequiredCorrect;

        startPhase2Button.interactable = ready;

        if (startButtonLabel != null)
        {
            startButtonLabel.text = ready
                ? "START PRACTICAL ▶"
                : $"Select {Mathf.Max(0, RequiredCorrect - correctSelected)} more correct item(s)...";
            startButtonLabel.color = ready ? Color.black : new Color(0.15f, 0.15f, 0.15f, 1f);
        }

        var img = startPhase2Button.GetComponent<Image>();
        if (img != null)
        {
            img.raycastTarget = true;
            img.color = ready ? ColGood : ColDisabled;
        }

        // Ensure listener is always present when ready
        if (ready)
        {
            startPhase2Button.onClick.RemoveListener(OnStartPhase2);
            startPhase2Button.onClick.AddListener(OnStartPhase2);
        }
    }

    private void OnStartPhase2()
    {
        correctSelected = selectedIds.Count;
        if (correctSelected < RequiredCorrect)
        {
            Flash($"Still need {RequiredCorrect - correctSelected} more correct item(s)! ({correctSelected}/5)", false);
            UpdateStartButtonState();
            return;
        }

        if (phase1Panel == null || phase2Panel == null)
            CacheUiRefs();

        if (phase1Panel == null || phase2Panel == null)
        {
            Flash("UI error — use Tools → Prism Practical → Build Complete Scene", false);
            return;
        }

        phase1Panel.SetActive(false);
        phase2Panel.SetActive(true);
        phase2Active = true;
        currentStep = 0;
        stepMistakes.Clear();

        SetActiveSafe(prismObj, false);
        SetActiveSafe(screenObj, false);
        SetActiveSafe(torchGlow, false);
        SetActiveSafe(lightBeam, false);
        SetActiveSafe(spectrumBand, false);
        SetActiveSafe(opticsObj, false);

        if (hudBar != null) hudBar.transform.SetAsLastSibling();
        RefreshScoreHud();
        RefreshStepActions();
        Flash("Phase 2 — tap the correct action for each step.", true);
        Debug.Log("Prism: Phase 2 started. Score=" + ComputeScore100());
    }

    // ═══════════════ PHASE 2 — one-click steps ═══════════════

    private void RefreshStepActions()
    {
        LoadResources();

        if (instructionText != null)
        {
            instructionText.font = uiFont;
            instructionText.fontSize = 26;
            instructionText.text = $"[{currentStep + 1}/5] {stepInstructions[currentStep]}";
        }
        if (progressText != null)
        {
            progressText.font = uiFont;
            progressText.fontSize = 22;
            progressText.text = $"Step {currentStep + 1} of 5 — tap the correct action below";
        }

        if (stepActionsRoot == null) return;
        var row = stepActionsRoot.transform.Find("ChoiceRow") as RectTransform;
        if (row == null) return;

        for (int i = row.childCount - 1; i >= 0; i--)
            Destroy(row.GetChild(i).gameObject);

        var choices = GetChoicesForStep(currentStep);
        Shuffle(choices);

        foreach (var choice in choices)
            CreateStepChoiceCard(row, choice);
    }

    private struct StepChoice
    {
        public string label;
        public string iconId;
        public bool correct;
    }

    private void CreateStepChoiceCard(RectTransform row, StepChoice choice)
    {
        Color bg = choice.correct ? new Color(0.15f, 0.42f, 0.48f, 1f) : ColCard;
        var card = MakePanel(row, "Choice_" + choice.iconId + (choice.correct ? "_ok" : "_no"), bg,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0, 0));
        var cardImg = card.GetComponent<Image>();
        cardImg.raycastTarget = true;

        var le = card.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        le.minWidth = 220;
        le.minHeight = 110;
        le.preferredHeight = 110;

        var cardRt = card.GetComponent<RectTransform>();

        var icon = MakeImage(cardRt, "Icon", Color.white,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(56, 56));
        icon.sprite = PrismIconFactory.GetSprite(choice.iconId);
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        var label = MakeText(cardRt, "Label", choice.label, 20, TextAnchor.LowerCenter,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(240, 48), Color.white);
        label.font = uiFont;
        label.fontStyle = FontStyle.Bold;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 14;
        label.resizeTextMaxSize = 22;
        label.raycastTarget = false;

        var btn = card.AddComponent<Button>();
        btn.targetGraphic = cardImg;
        bool isCorrect = choice.correct;
        string lab = choice.label;
        btn.onClick.AddListener(() => OnStepChoice(isCorrect, lab));
    }

    private List<StepChoice> GetChoicesForStep(int step)
    {
        switch (step)
        {
            case 0:
                return new List<StepChoice>
                {
                    new StepChoice { label = "Place Glass Prism", iconId = "prism", correct = true },
                    new StepChoice { label = "Place Convex Lens", iconId = "lens", correct = false },
                    new StepChoice { label = "Place Beaker", iconId = "beaker", correct = false },
                };
            case 1:
                return new List<StepChoice>
                {
                    new StepChoice { label = "Mirror + Cardboard Slit", iconId = "mirror", correct = true },
                    new StepChoice { label = "Measuring Tape", iconId = "tape", correct = false },
                    new StepChoice { label = "Spring Balance", iconId = "spring", correct = false },
                };
            case 2:
                return new List<StepChoice>
                {
                    new StepChoice { label = "Turn ON Light Source", iconId = "torch", correct = true },
                    new StepChoice { label = "Keep Light OFF", iconId = "torch", correct = false },
                    new StepChoice { label = "Remove the Prism", iconId = "prism", correct = false },
                };
            case 3:
                return new List<StepChoice>
                {
                    new StepChoice { label = "Place White Screen", iconId = "screen", correct = true },
                    new StepChoice { label = "Place Beaker", iconId = "beaker", correct = false },
                    new StepChoice { label = "Place Convex Lens", iconId = "lens", correct = false },
                };
            default:
                return new List<StepChoice>
                {
                    new StepChoice { label = "Record ROYGBIV spectrum", iconId = "screen", correct = true },
                    new StepChoice { label = "Light stayed white", iconId = "torch", correct = false },
                    new StepChoice { label = "Skip observation", iconId = "tape", correct = false },
                };
        }
    }

    private void OnStepChoice(bool correct, string label)
    {
        if (!phase2Active) return;

        if (!correct)
        {
            RegisterMistake("Wrong action for this step.");
            return;
        }

        ApplyStepVisual(currentStep);
        CompleteStep();
    }

    private void ApplyStepVisual(int step)
    {
        switch (step)
        {
            case 0:
                SetActiveSafe(prismObj, true);
                break;
            case 1:
                SetActiveSafe(opticsObj, true);
                SetActiveSafe(torchGlow, true);
                break;
            case 2:
                SetActiveSafe(torchGlow, true);
                SetActiveSafe(lightBeam, true);
                break;
            case 3:
                SetActiveSafe(screenObj, true);
                SetActiveSafe(spectrumBand, true);
                break;
            case 4:
                var note = phase2Panel?.transform.Find("ObsNote")?.GetComponent<Text>();
                if (note != null)
                    note.text = "Observation: White light dispersed into ROYGBIV (VIBGYOR) on the screen.";
                break;
        }
    }

    private void CompleteStep()
    {
        stepsCompleted++;
        RefreshScoreHud();
        Flash($"Step {currentStep + 1} complete! +{PointsPerStep}   Score {ComputeScore100()}/100", true);

        currentStep++;
        if (currentStep >= stepInstructions.Length)
            FinishPractical();
        else
            RefreshStepActions();
    }

    private void RegisterMistake(string msg)
    {
        stepMistakes.Add(currentStep);
        totalMistakes++;
        RefreshScoreHud();
        Flash($"{msg} −{MistakePenalty}   Score {ComputeScore100()}/100", false);
    }

    private void FinishPractical()
    {
        if (flutterSent)
        {
            return;
        }

        flutterSent = true;
        phase2Active = false;
        finalScore100 = ComputeScore100();
        TimerManager.Instance?.Stop();

        phase2Panel?.SetActive(false);
        if (endPanel == null) CacheUiRefs();
        endPanel?.SetActive(true);
        if (endPanel != null) endPanel.transform.SetAsLastSibling();

        int correctTotal = selectedIds.Count + stepsCompleted;
        int stars = finalScore100 > 80 ? 3 : finalScore100 >= 50 ? 2 : 1;
        string grade = finalScore100 > 80 ? "Excellent" : finalScore100 >= 50 ? "Good" : "Needs Improvement";
        bool passed = finalScore100 >= 50;

        if (endPanel != null)
        {
            foreach (var t in endPanel.GetComponentsInChildren<Text>(true))
            {
                t.font = uiFont;
                switch (t.gameObject.name)
                {
                    case "FinalScore":
                        t.fontSize = 36;
                        t.fontStyle = FontStyle.Bold;
                        t.text = $"Total Score: {finalScore100} / 100";
                        break;
                    case "Correct":
                        t.fontSize = 24;
                        t.text = $"Correct Choices: {correctTotal}";
                        break;
                    case "Mistakes":
                        t.fontSize = 24;
                        t.text = $"Incorrect Mistakes: {totalMistakes}";
                        break;
                    case "Grade":
                        t.fontSize = 26;
                        t.fontStyle = FontStyle.Bold;
                        t.text = $"Grade: {grade}";
                        break;
                    case "StarsText":
                        t.fontSize = 44;
                        t.text = stars == 3 ? "★ ★ ★" : stars == 2 ? "★ ★ ☆" : "★ ☆ ☆";
                        break;
                }
            }
        }

        if (scoreText != null)
            scoreText.text = $"Score: {finalScore100} / 100";

        if (instructionText != null)
        {
            instructionText.text = Application.isEditor
                ? "Score saved in this Play session. Unity Editor will stop Play in a moment."
                : "Your marks were sent to the app profile. You can leave this lab.";
        }
        if (progressText != null) progressText.text = "";

        int timeUsed = TimerManager.Instance != null ? TimerManager.Instance.TimeUsedSeconds : 0;
        string measurements =
            "{\"apparatus\":" + selectedIds.Count.ToString(CultureInfo.InvariantCulture)
            + ",\"steps\":" + stepsCompleted.ToString(CultureInfo.InvariantCulture)
            + ",\"mistakes\":" + totalMistakes.ToString(CultureInfo.InvariantCulture)
            + "}";
        FlutterBridge.EnsureInstance()?.NotifyCompleted(
            finalScore100,
            passed,
            totalMistakes,
            timeUsed,
            true,
            measurements);

        Debug.Log($"Prism FINISH: {finalScore100}/100 | apparatus={selectedIds.Count} steps={stepsCompleted} mistakes={totalMistakes}");
        StartCoroutine(AfterSubmit());
    }

    public void CompleteOnTimeout()
    {
        FinishPractical();
    }

    private void UpdateTimerHud(int remaining)
    {
        if (timerText == null)
        {
            var canvas = transform.Find("Canvas");
            if (canvas != null) timerText = FindText(canvas, "Timer");
        }

        if (timerText == null)
        {
            return;
        }

        int minutes = Mathf.Max(0, remaining) / 60;
        int seconds = Mathf.Max(0, remaining) % 60;
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        timerText.color = remaining <= 30 ? ColBad : ColText;
    }

    private IEnumerator AfterSubmit()
    {
        yield return new WaitForSecondsRealtime(1.2f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void Flash(string msg, bool good)
    {
        if (feedbackText == null) return;
        feedbackText.text = msg;
        feedbackText.color = good ? ColGood : ColBad;
        CancelInvoke(nameof(ClearFeedback));
        Invoke(nameof(ClearFeedback), 2.4f);
    }

    private void ClearFeedback()
    {
        if (feedbackText != null) feedbackText.text = "";
    }

    private System.Collections.IEnumerator RestoreColor(Image img, Color c, float d)
    {
        yield return new WaitForSeconds(d);
        if (img != null) img.color = c;
    }

    private static void SetActiveSafe(GameObject go, bool on)
    {
        if (go != null) go.SetActive(on);
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static void ApplyDisabledColors(Button btn)
    {
        if (btn == null) return;
        var cb = btn.colors;
        cb.disabledColor = new Color(0.35f, 0.38f, 0.42f, 0.85f);
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(0.9f, 0.95f, 1f);
        cb.pressedColor = new Color(0.75f, 0.8f, 0.85f);
        btn.colors = cb;
    }

    // ═══════════════ HELPERS ═══════════════

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

    private GameObject CreateLabProp(RectTransform parent, string name, string iconId, string caption, Vector2 anchor, Vector2 size)
    {
        var go = MakePanel(parent, name, new Color(0.2f, 0.28f, 0.34f, 0.92f),
            anchor, anchor, new Vector2(0.5f, 0.5f), Vector2.zero, size);
        var rt = go.GetComponent<RectTransform>();

        var icon = MakeImage(rt, "Icon", Color.white,
            new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(size.x * 0.55f, size.y * 0.45f));
        icon.sprite = PrismIconFactory.GetSprite(iconId);
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        var label = MakeText(rt, "Caption", caption, 18, TextAnchor.LowerCenter,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 8), new Vector2(size.x - 8, 36), Color.white);
        label.fontStyle = FontStyle.Bold;
        label.raycastTarget = false;
        return go;
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
        t.font = uiFont;
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
}

public class PrismUIRefsHolder : MonoBehaviour
{
    public int UiVersion;
}

public class PrismApparatusCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public string Id;
    public bool IsCorrect;
    public Image CardImage;
    public Text CheckLabel;
    public Outline Outline;
    public Color NormalColor = new Color(0.26f, 0.34f, 0.42f, 1f);

    System.Action<PrismApparatusCard> onClicked;
    bool selected;
    Coroutine flashRoutine;

    public void Bind(System.Action<PrismApparatusCard> callback) => onClicked = callback;

    public void EnsureClickable()
    {
        if (CardImage == null) CardImage = GetComponent<Image>();
        if (CardImage != null) CardImage.raycastTarget = true;

        // Children must never steal clicks
        foreach (var g in GetComponentsInChildren<Graphic>(true))
        {
            if (g.gameObject == gameObject) continue;
            g.raycastTarget = false;
        }

        // Remove leftover Button components that can block selection after 1–3 picks
        var btn = GetComponent<Button>();
        if (btn != null)
            Destroy(btn);
    }

    public void SetSelectedVisual(bool on)
    {
        selected = on;
        if (CardImage != null)
            CardImage.color = on ? new Color(0.16f, 0.48f, 0.28f, 1f) : NormalColor;
        if (CheckLabel != null)
            CheckLabel.text = on ? "✓" : "";
        if (Outline != null)
        {
            Outline.effectColor = on ? new Color(0.4f, 1f, 0.55f, 1f) : new Color(1f, 1f, 1f, 0.15f);
            Outline.effectDistance = on ? new Vector2(3, -3) : new Vector2(2, -2);
        }
    }

    public void FlashWrong(Color bad, Color normal)
    {
        if (selected) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashWrongCo(bad, normal));
    }

    System.Collections.IEnumerator FlashWrongCo(Color bad, Color normal)
    {
        if (CardImage != null) CardImage.color = bad;
        yield return new WaitForSeconds(0.35f);
        if (CardImage != null && !selected) CardImage.color = normal;
        flashRoutine = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (onClicked != null) onClicked.Invoke(this);
        else FindAnyObjectByType<PrismSceneRuntimeBuilder>()?.HandleApparatusCardClick(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selected || CardImage == null) return;
        CardImage.color = new Color(0.32f, 0.42f, 0.52f, 1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (selected || CardImage == null) return;
        CardImage.color = NormalColor;
    }
}

// Kept for older scene leftovers (harmless if unused)
public class PrismPlaceSlot : MonoBehaviour
{
    public string SlotId;
    public int StepIndex;
}

public class PrismStepItem : MonoBehaviour
{
    public string ItemId;
}
