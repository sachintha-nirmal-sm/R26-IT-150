using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

[DefaultExecutionOrder(-50)]
public class ElectronicsSceneRuntimeBuilder : MonoBehaviour
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
            Debug.LogWarning("Electronics diode practical: wiring failed.");
            return;
        }
        referencesWired = true;
    }

    private void Awake()
    {
        if (GetComponent<ElectronicsFailsafeDisplay>() == null)
            gameObject.AddComponent<ElectronicsFailsafeDisplay>();
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
            Debug.Log("Electronics diode practical UI built.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Electronics BUILD FAILED: " + ex.Message + "\n" + ex.StackTrace);
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
            Debug.LogWarning("Electronics EventSystem: " + ex.Message);
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
        AddMgr<ElectronicsPracticalManager>("ElectronicsPracticalManager");
        AddMgr<ElectronicsUIManager>("ElectronicsUIManager");
        AddMgr<ElectronicsScoreManager>("ElectronicsScoreManager");
        AddMgr<ElectronicsFeedbackManager>("ElectronicsFeedbackManager");
        AddMgr<ElectronicsAttemptManager>("ElectronicsAttemptManager");
        AddMgr<ElectronicsProgressManager>("ElectronicsProgressManager");
        AddMgr<ElectronicsSaveManager>("ElectronicsSaveManager");
        AddMgr<ElectronicsProfileManager>("ElectronicsProfileManager");
        AddMgr<ElectronicsEquipmentSelectionManager>("ElectronicsEquipmentSelectionManager");
        AddMgr<ElectronicsEquipmentSnapController>("ElectronicsEquipmentSnapController");
        AddMgr<ElectronicsLabEquipmentTray>("ElectronicsLabEquipmentTray");
        AddMgr<ElectronicsCircuitBoardManager>("ElectronicsCircuitBoardManager");
        AddMgr<ElectronicsCircuitConnectionManager>("ElectronicsCircuitConnectionManager");
        AddMgr<ElectronicsWireController>("ElectronicsWireController");
        AddMgr<ElectronicsBatteryController>("ElectronicsBatteryController");
        AddMgr<ElectronicsDiodeController>("ElectronicsDiodeController");
        AddMgr<ElectronicsBulbController>("ElectronicsBulbController");
        AddMgr<ElectronicsSwitchController>("ElectronicsSwitchController");
        AddMgr<ElectronicsForwardBiasController>("ElectronicsForwardBiasController");
        AddMgr<ElectronicsReverseBiasController>("ElectronicsReverseBiasController");
        AddMgr<ElectronicsObservationManager>("ElectronicsObservationManager");
        AddMgr<ElectronicsComparisonManager>("ElectronicsComparisonManager");
        AddMgr<ElectronicsQuestionManager>("ElectronicsQuestionManager");
        AddMgr<ElectronicsFormulaMatchingManager>("ElectronicsFormulaMatchingManager");
        AddMgr<ElectronicsMiniChallengeManager>("ElectronicsMiniChallengeManager");
        AddMgr<ElectronicsConclusionManager>("ElectronicsConclusionManager");
        AddMgr<ElectronicsResultManager>("ElectronicsResultManager");
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
        Color header = new Color(0.08f, 0.28f, 0.42f);
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
        var title = Text("Title", headerP.transform, "ELECTRONICS", 26, TextAlignmentOptions.MidlineLeft, Vector2.zero, Vector2.zero, new Vector2(0.02f, 0.50f), new Vector2(0.62f, 1f), new Vector2(0.5f, 0.5f));
        StretchFill(title.rectTransform);
        title.color = Color.white;
        title.enableAutoSizing = true;
        title.fontSizeMin = 12;
        title.fontSizeMax = 26;
        title.overflowMode = TextOverflowModes.Ellipsis;
        var score = Text("Score", headerP.transform, "SCORE: 0/100", 24, TextAlignmentOptions.MidlineRight, Vector2.zero, Vector2.zero, new Vector2(0.62f, 0.50f), new Vector2(0.98f, 1f), new Vector2(0.5f, 0.5f));
        StretchFill(score.rectTransform);
        score.color = Color.white;
        score.enableAutoSizing = true;
        score.fontSizeMin = 12;
        score.fontSizeMax = 24;
        var progress = Text("Progress", headerP.transform, "Step: 1 / 16", 20, TextAlignmentOptions.MidlineLeft, Vector2.zero, Vector2.zero, new Vector2(0.02f, 0.08f), new Vector2(0.28f, 0.50f), new Vector2(0.5f, 0.5f));
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
        Btn("Next", bottom.transform, "NEXT STEP", accent, 240, 64);
        Btn("Reset", bottom.transform, "Reset", new Color(0.45f, 0.50f, 0.56f), 150, 64);
        var retryBtn = Btn("Retry", bottom.transform, "RETRY", accent, 150, 64);
        retryBtn.gameObject.SetActive(false);

        var main = Panel(canvasObj.transform, "MainArea", new Vector2(0, 0.11f), new Vector2(1, 0.83f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);

        CreateIntro(main.transform, green);
        CreateTheory(main.transform, green);
        CreateEquipment(main.transform);
        CreateLaboratory(main.transform, accent, green);
        CreateObservation(main.transform, green);
        CreateCompare(main.transform);
        CreateMatching(main.transform);
        CreateChallenge(main.transform, green);
        CreateQuestions(main.transform);
        CreateConclusion(main.transform, green);
        CreateResult(main.transform, green);
        CreateFeedback(canvasObj.transform);
        ConfirmDialog(canvasObj.transform, "ResetConfirm", "Reset the practical?");
        ConfirmDialog(canvasObj.transform, "RetryConfirm", "Start a new attempt? Best score is kept.");
        CreateCardPrefab();
        var refs = canvasObj.AddComponent<ElectronicsUIRefs>();
        refs.UiVersion = 2;
    }

    private void CreateIntro(Transform main, Color green)
    {
        var introPanel = Panel(main, "IntroPanel", Vector2.zero, Vector2.one, new Vector2(16, 12), new Vector2(-16, -12), Color.white);
        var introText = StretchText("IntroText", introPanel.transform,
            "ELECTRONICS\n\nInvestigation of Forward Bias and Reverse Bias of a Diode\n\nIn this practical you will investigate how the direction of a diode affects current flow in a simple circuit.\n\nFORWARD BIAS — diode allows current to flow.\nREVERSE BIAS — diode blocks current flow.",
            24, TextAlignmentOptions.TopLeft, new Color(0.10f, 0.18f, 0.28f), 12);
        introText.enableAutoSizing = true;
        introText.fontSizeMin = 14;
        introText.fontSizeMax = 26;
        var introRt = introText.rectTransform;
        introRt.anchorMin = new Vector2(0.02f, 0.46f);
        introRt.anchorMax = new Vector2(0.98f, 0.98f);
        var card = Panel(introPanel.transform, "BiasCard", new Vector2(0.12f, 0.18f), new Vector2(0.88f, 0.44f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.28f, 0.42f));
        StretchText("F", card.transform, "Anode → |>| → Cathode\nForward: current flows    Reverse: current blocked", 22, TextAlignmentOptions.Center, Color.white, 10);
        BigBtn("StartBtn", introPanel.transform, "START PRACTICAL", new Vector2(0.5f, 0.08f), Vector2.zero, new Vector2(320, 56), green);
        introPanel.AddComponent<ElectronicsIntroClickToStart>();
    }

    private void CreateTheory(Transform main, Color green)
    {
        var panel = Panel(main, "TheoryPanel", Vector2.zero, Vector2.one, new Vector2(16, 12), new Vector2(-16, -12), Color.white);
        panel.SetActive(false);
        var theoryText = StretchText("TheoryText", panel.transform,
            "DIODE\nA diode is an electronic component that allows electric current to flow mainly in one direction.\n\nFORWARD BIAS\nWhen the diode is connected in the correct direction, current can flow through the circuit.\nBattery → Diode → Bulb → Switch → Battery\n\nREVERSE BIAS\nWhen the diode is connected in the opposite direction, current is blocked.\n\nSymbol:  Anode  →  |>|  →  Cathode",
            24, TextAlignmentOptions.TopLeft, new Color(0.10f, 0.18f, 0.28f), 12);
        theoryText.enableAutoSizing = true;
        theoryText.fontSizeMin = 14;
        theoryText.fontSizeMax = 24;
        theoryText.rectTransform.anchorMin = new Vector2(0.02f, 0.16f);
        theoryText.rectTransform.anchorMax = new Vector2(0.98f, 0.98f);
        BigBtn("TheoryContinue", panel.transform, "CONTINUE", new Vector2(0.5f, 0.08f), Vector2.zero, new Vector2(240, 52), green);
    }

    private void CreateEquipment(Transform main)
    {
        var panel = Panel(main, "EquipmentPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        panel.SetActive(false);
        var equipTitle = Text("EquipTitle", panel.transform, "STEP 1 — Tap the equipment needed for this practical.", 26, TextAlignmentOptions.MidlineLeft, Vector2.zero, Vector2.zero, new Vector2(0.02f, 0.91f), new Vector2(0.98f, 1f), new Vector2(0.5f, 0.5f));
        StretchFill(equipTitle.rectTransform);
        equipTitle.enableAutoSizing = true;
        equipTitle.fontSizeMin = 14;
        equipTitle.fontSizeMax = 26;
        equipTitle.overflowMode = TextOverflowModes.Ellipsis;
        var equipHint = Text("EquipHint", panel.transform, "Tap the correct items, then press NEXT STEP at the bottom.", 20, TextAlignmentOptions.MidlineLeft, Vector2.zero, Vector2.zero, new Vector2(0.02f, 0.85f), new Vector2(0.98f, 0.91f), new Vector2(0.5f, 0.5f));
        StretchFill(equipHint.rectTransform);
        equipHint.color = new Color(0.12f, 0.32f, 0.48f);
        equipHint.enableAutoSizing = true;
        equipHint.fontSizeMin = 12;
        equipHint.fontSizeMax = 20;
        equipHint.overflowMode = TextOverflowModes.Ellipsis;

        var required = Panel(panel.transform, "RequiredArea", new Vector2(0f, 0f), new Vector2(1f, 0.24f), new Vector2(8, 6), new Vector2(-8, -4), new Color(0.88f, 0.94f, 0.90f));
        required.AddComponent<ElectronicsUIDropTarget>().Configure("RequiredEquipment", "Any", Vector2.zero);
        StretchText("ReqTitle", required.transform, "REQUIRED APPARATUS", 16, TextAlignmentOptions.MidlineLeft, new Color(0.10f, 0.32f, 0.22f), 8).rectTransform.anchorMin = new Vector2(0f, 0.72f);
        var reqContent = Panel(required.transform, "Content", new Vector2(0.02f, 0.04f), new Vector2(0.98f, 0.70f), Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.2f), false);
        var gridR = reqContent.AddComponent<GridLayoutGroup>();
        gridR.cellSize = new Vector2(140, 88);
        gridR.spacing = new Vector2(6, 6);
        gridR.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridR.constraintCount = 3;
        CreateScrollAnchored(panel.transform, new Vector2(0f, 0.25f), new Vector2(1f, 0.85f));
    }

    private void CreateLaboratory(Transform main, Color accent, Color green)
    {
        var panel = Panel(main, "LaboratoryPanel", Vector2.zero, Vector2.one, new Vector2(12, 8), new Vector2(-12, -8), new Color(0, 0, 0, 0), false);
        panel.SetActive(false);

        var trayPanel = Panel(panel.transform, "Tray", Vector2.zero, new Vector2(0.20f, 1f), Vector2.zero, Vector2.zero, new Color(0.93f, 0.96f, 1f));
        var trayHeader = Panel(trayPanel.transform, "TrayHeader", new Vector2(0f, 0.90f), Vector2.one, Vector2.zero, Vector2.zero, new Color(0.08f, 0.28f, 0.42f), false);
        StretchText("TrayTitle", trayHeader.transform, "APPARATUS", 16, TextAlignmentOptions.Center, Color.white, 4).fontStyle = FontStyles.Bold;
        var scrollObj = Panel(trayPanel.transform, "Scroll", new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.88f), Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.15f));
        var trayScroll = scrollObj.AddComponent<ScrollRect>();
        var viewport = Panel(scrollObj.transform, "Viewport", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.01f));
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        var trayContent = Panel(viewport.transform, "Content", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.15f), false);
        var trayInnerRt = trayContent.GetComponent<RectTransform>();
        trayInnerRt.pivot = new Vector2(0.5f, 1f);
        var v = trayContent.AddComponent<VerticalLayoutGroup>();
        v.spacing = 8;
        v.padding = new RectOffset(6, 6, 6, 6);
        v.childForceExpandHeight = false;
        v.childForceExpandWidth = true;
        v.childControlHeight = true;
        v.childControlWidth = true;
        trayContent.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        trayScroll.viewport = viewport.GetComponent<RectTransform>();
        trayScroll.content = trayInnerRt;
        trayScroll.vertical = true;
        trayScroll.horizontal = false;
        trayScroll.movementType = ScrollRect.MovementType.Clamped;

        var boardArea = Panel(panel.transform, "BreadboardArea", new Vector2(0.21f, 0.18f), Vector2.one, Vector2.zero, Vector2.zero, new Color(0.97f, 0.93f, 0.80f));
        StretchText("BoardTitle", boardArea.transform, "CIRCUIT  •  Battery → Switch → Diode → Bulb → Battery", 18, TextAlignmentOptions.TopLeft, new Color(0.28f, 0.18f, 0.08f), 8);
        Panel(boardArea.transform, "Glow", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear, false);

        var boardZone = DropZone(boardArea.transform, "BoardZone", "Breadboard", new Vector2(0.02f, 0.06f), new Vector2(0.20f, 0.34f), "BOARD");
        var batteryZone = DropZone(boardArea.transform, "BatteryZone", "Battery", new Vector2(0.02f, 0.58f), new Vector2(0.24f, 0.94f), "BATTERY 3V");
        var switchZone = DropZone(boardArea.transform, "SwitchZone", "Switch", new Vector2(0.32f, 0.58f), new Vector2(0.50f, 0.94f), "SWITCH");
        var diodeZone = DropZone(boardArea.transform, "DiodeZone", "Diode", new Vector2(0.58f, 0.58f), new Vector2(0.76f, 0.94f), "DIODE  |>|");
        var bulbZone = DropZone(boardArea.transform, "BulbZone", "Bulb", new Vector2(0.84f, 0.50f), new Vector2(0.98f, 0.94f), "BULB");

        DropZone(boardArea.transform, "WireBatterySwitch", "Wire", new Vector2(0.245f, 0.70f), new Vector2(0.315f, 0.82f), "wire");
        DropZone(boardArea.transform, "WireSwitchDiode", "Wire", new Vector2(0.505f, 0.70f), new Vector2(0.575f, 0.82f), "wire");
        DropZone(boardArea.transform, "WireDiodeBulb", "Wire", new Vector2(0.765f, 0.70f), new Vector2(0.835f, 0.82f), "wire");
        DropZone(boardArea.transform, "WireBulbBattery", "Wire", new Vector2(0.40f, 0.38f), new Vector2(0.60f, 0.48f), "return wire");

        CreateWirePath(boardArea.transform, "W1",
            (new Vector2(0.24f, 0.73f), new Vector2(0.32f, 0.77f)));
        CreateWirePath(boardArea.transform, "W2",
            (new Vector2(0.50f, 0.73f), new Vector2(0.58f, 0.77f)));
        CreateWirePath(boardArea.transform, "W3",
            (new Vector2(0.76f, 0.73f), new Vector2(0.84f, 0.77f)));
        CreateWirePath(boardArea.transform, "W4",
            (new Vector2(0.90f, 0.48f), new Vector2(0.94f, 0.52f)),
            (new Vector2(0.08f, 0.40f), new Vector2(0.94f, 0.44f)),
            (new Vector2(0.08f, 0.44f), new Vector2(0.12f, 0.58f)));

        Terminal(boardArea.transform, "Battery+", new Vector2(0.04f, 0.86f), new Vector2(0.11f, 0.94f), new Color(0.85f, 0.18f, 0.16f), "+");
        Terminal(boardArea.transform, "Battery-", new Vector2(0.15f, 0.86f), new Vector2(0.22f, 0.94f), new Color(0.12f, 0.12f, 0.16f), "−");
        Terminal(boardArea.transform, "SwitchIn", new Vector2(0.33f, 0.86f), new Vector2(0.39f, 0.94f), accent, "In");
        Terminal(boardArea.transform, "SwitchOut", new Vector2(0.43f, 0.86f), new Vector2(0.49f, 0.94f), accent, "Out");
        Terminal(boardArea.transform, "DiodeAnode", new Vector2(0.59f, 0.86f), new Vector2(0.65f, 0.94f), new Color(0.20f, 0.62f, 0.72f), "A");
        Terminal(boardArea.transform, "DiodeCathode", new Vector2(0.69f, 0.86f), new Vector2(0.75f, 0.94f), new Color(0.72f, 0.62f, 0.18f), "K");
        Terminal(boardArea.transform, "BulbIn", new Vector2(0.85f, 0.86f), new Vector2(0.91f, 0.94f), new Color(0.92f, 0.72f, 0.18f), "In");
        Terminal(boardArea.transform, "BulbOut", new Vector2(0.91f, 0.50f), new Vector2(0.97f, 0.58f), new Color(0.92f, 0.72f, 0.18f), "Out");

        var tools = Panel(panel.transform, "CircuitTools", new Vector2(0.21f, 0f), new Vector2(1f, 0.17f), Vector2.zero, Vector2.zero, new Color(0.12f, 0.22f, 0.34f));
        var row = tools.AddComponent<HorizontalLayoutGroup>();
        row.padding = new RectOffset(10, 10, 8, 8);
        row.spacing = 10;
        row.childForceExpandWidth = true;
        row.childControlHeight = true;
        Btn("SwitchBtn", tools.transform, "TURN ON", green, 0, 56);
        Btn("FlipDiodeBtn", tools.transform, "FLIP DIODE", accent, 0, 56);
        var battTools = Panel(tools.transform, "BatteryTools", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var bLayout = battTools.AddComponent<HorizontalLayoutGroup>();
        bLayout.spacing = 8;
        bLayout.childForceExpandWidth = true;
        Btn("DisconnectBtn", battTools.transform, "DISCONNECT BATTERY", new Color(0.75f, 0.35f, 0.12f), 0, 56);
        Btn("ReverseBtn", battTools.transform, "REVERSE BATTERY", new Color(0.72f, 0.22f, 0.22f), 0, 56);
        Btn("ReconnectBtn", battTools.transform, "RECONNECT", green, 0, 56);
        battTools.SetActive(false);

        var status = StretchText("CircuitStatus", boardArea.transform, "Build the circuit.", 18, TextAlignmentOptions.BottomLeft, new Color(0.18f, 0.22f, 0.30f), 10);
        status.rectTransform.anchorMin = new Vector2(0.22f, 0.02f);
        status.rectTransform.anchorMax = new Vector2(0.78f, 0.16f);
        var bulbStatus = StretchText("BulbStatus", boardArea.transform, "BULB STATUS: NOT GLOWING", 16, TextAlignmentOptions.BottomRight, new Color(0.18f, 0.22f, 0.30f), 8);
        bulbStatus.rectTransform.anchorMin = new Vector2(0.70f, 0.02f);
        bulbStatus.rectTransform.anchorMax = new Vector2(0.98f, 0.14f);

        var diodeIcon = Panel(diodeZone.transform, "DiodeIcon", new Vector2(0.1f, 0.18f), new Vector2(0.9f, 0.72f), Vector2.zero, Vector2.zero, Color.white, false);
        diodeIcon.GetComponent<Image>().sprite = ElectronicsIconFactory.GetNamed("diode");
        diodeIcon.GetComponent<Image>().preserveAspect = true;
        diodeIcon.GetComponent<Image>().raycastTarget = false;
        StretchText("AnodeLabel", diodeZone.transform, "Anode  >|", 13, TextAlignmentOptions.BottomLeft, new Color(0.10f, 0.18f, 0.28f), 4);
        StretchText("CathodeLabel", diodeZone.transform, "|<  Cathode", 13, TextAlignmentOptions.BottomRight, new Color(0.10f, 0.18f, 0.28f), 4);
        StretchText("DiodeStatus", diodeZone.transform, "IN4001", 13, TextAlignmentOptions.Top, new Color(0.10f, 0.18f, 0.28f), 2);

        var battIcon = Panel(batteryZone.transform, "BatteryIcon", new Vector2(0.08f, 0.16f), new Vector2(0.92f, 0.70f), Vector2.zero, Vector2.zero, Color.white, false);
        battIcon.GetComponent<Image>().sprite = ElectronicsIconFactory.GetNamed("battery");
        battIcon.GetComponent<Image>().preserveAspect = true;
        battIcon.GetComponent<Image>().raycastTarget = false;
        StretchText("PolarityLabel", batteryZone.transform, "+  toward switch", 13, TextAlignmentOptions.Bottom, new Color(0.10f, 0.18f, 0.28f), 2);
        StretchText("VoltageLabel", batteryZone.transform, "1.5 V + 1.5 V = 3 V", 13, TextAlignmentOptions.Top, new Color(0.10f, 0.18f, 0.28f), 2);

        var bulbIcon = Panel(bulbZone.transform, "BulbIcon", new Vector2(0.12f, 0.26f), new Vector2(0.88f, 0.84f), Vector2.zero, Vector2.zero, Color.white, false);
        bulbIcon.GetComponent<Image>().sprite = ElectronicsIconFactory.GetNamed("bulb-off");
        bulbIcon.GetComponent<Image>().preserveAspect = true;
        bulbIcon.GetComponent<Image>().raycastTarget = false;
        Panel(bulbZone.transform, "BulbGlow", new Vector2(0.05f, 0.20f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero, Color.clear, false);
        var raysImg = Panel(bulbZone.transform, "BulbRays", new Vector2(0f, 0.15f), Vector2.one, Vector2.zero, Vector2.zero, Color.clear, false);
        raysImg.GetComponent<Image>().sprite = ElectronicsIconFactory.GetNamed("rays");
        StretchText("LocalBulbStatus", bulbZone.transform, "OFF", 13, TextAlignmentOptions.Bottom, new Color(0.10f, 0.18f, 0.28f), 2);

        var swIcon = Panel(switchZone.transform, "SwitchIcon", new Vector2(0.12f, 0.20f), new Vector2(0.88f, 0.76f), Vector2.zero, Vector2.zero, Color.white, false);
        swIcon.GetComponent<Image>().sprite = ElectronicsIconFactory.GetNamed("switch-off");
        swIcon.GetComponent<Image>().preserveAspect = true;
        swIcon.GetComponent<Image>().raycastTarget = false;
        StretchText("SwitchLabel", switchZone.transform, "SWITCH: OFF", 13, TextAlignmentOptions.Bottom, new Color(0.10f, 0.18f, 0.28f), 2);

        _ = boardZone;

        var adaptive = panel.AddComponent<ElectronicsAdaptiveLab>();
        adaptive.Bind(
            trayPanel.GetComponent<RectTransform>(),
            trayScroll,
            trayInnerRt,
            boardArea.GetComponent<RectTransform>(),
            tools.GetComponent<RectTransform>());
    }

    private void CreateObservation(Transform main, Color green)
    {
        var panel = Panel(main, "ObservationPanel", new Vector2(0f, 0f), new Vector2(1f, 0.28f), Vector2.zero, Vector2.zero, Color.white);
        panel.SetActive(false);
        StretchText("ObservationTable", panel.transform, "OBSERVATION TABLE", 20, TextAlignmentOptions.TopLeft, new Color(0.10f, 0.18f, 0.28f), 12);
        var row = Panel(panel.transform, "ObsBtns", new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.28f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var h = row.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 12;
        h.childForceExpandWidth = true;
        Btn("ObsGlow", row.transform, "Bulb glows", green, 0, 64);
        Btn("ObsDark", row.transform, "Bulb does not glow", new Color(0.45f, 0.48f, 0.55f), 0, 64);
    }

    private void CreateCompare(Transform main)
    {
        var panel = Panel(main, "ComparePanel", Vector2.zero, new Vector2(0.46f, 1f), new Vector2(16, 12), new Vector2(-8, -12), Color.white);
        panel.SetActive(false);
        StretchText("CompareTitle", panel.transform, "FORWARD BIAS  vs  REVERSE BIAS", 22, TextAlignmentOptions.Top, new Color(0.08f, 0.22f, 0.38f), 12)
            .rectTransform.anchorMax = new Vector2(1f, 0.98f);
        StretchText("CompareText", panel.transform, "Use this table, then answer the questions.", 16, TextAlignmentOptions.Top, new Color(0.28f, 0.34f, 0.42f), 10)
            .rectTransform.anchorMin = new Vector2(0f, 0.88f);
        var table = Panel(panel.transform, "Table", new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.86f), Vector2.zero, Vector2.zero, new Color(0.93f, 0.96f, 1f));
        var grid = table.AddComponent<VerticalLayoutGroup>();
        grid.spacing = 8;
        grid.padding = new RectOffset(10, 10, 10, 10);
        grid.childForceExpandHeight = true;
        grid.childForceExpandWidth = true;
        CreateCompareRow(table.transform, "Header", "Property", "Forward Bias", "Reverse Bias", new Color(0.08f, 0.32f, 0.48f), Color.white, false);
        CreateCompareRow(table.transform, "Row0", "Diode direction", "Correct\n(current can flow)", "Reversed\n(current blocked)", new Color(0.86f, 0.93f, 0.98f), new Color(0.10f, 0.18f, 0.28f), true);
        CreateCompareRow(table.transform, "Row1", "Current", "Flows", "Blocked", new Color(0.90f, 0.95f, 0.90f), new Color(0.10f, 0.18f, 0.28f), true);
        CreateCompareRow(table.transform, "Row2", "Bulb", "Glows", "Does not glow", new Color(1f, 0.96f, 0.82f), new Color(0.10f, 0.18f, 0.28f), true);
    }

    private void CreateCompareRow(Transform parent, string name, string a, string b, string c, Color bg, Color fg, bool wrap)
    {
        var row = new GameObject(name);
        row.transform.SetParent(parent, false);
        row.AddComponent<LayoutElement>().preferredHeight = 92;
        var img = row.AddComponent<Image>();
        img.sprite = ElectronicsIconFactory.White();
        img.color = bg;
        var h = row.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 6;
        h.padding = new RectOffset(8, 8, 8, 8);
        h.childForceExpandWidth = true;
        h.childForceExpandHeight = true;
        CompareCell(row.transform, "C0", a, fg, wrap, 0.28f);
        CompareCell(row.transform, "C1", b, fg, wrap, 0.36f);
        CompareCell(row.transform, "C2", c, fg, wrap, 0.36f);
    }

    private void CompareCell(Transform parent, string name, string text, Color color, bool wrap, float flex)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var le = obj.AddComponent<LayoutElement>();
        le.flexibleWidth = flex;
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) tmp.font = defaultFont;
        tmp.text = text;
        tmp.fontSize = 20;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 14;
        tmp.fontSizeMax = 22;
#pragma warning disable CS0618
        tmp.enableWordWrapping = wrap;
#pragma warning restore CS0618
        tmp.raycastTarget = false;
    }

    private void CreateMatching(Transform main)
    {
        var panel = Panel(main, "MatchPanel", Vector2.zero, Vector2.one, new Vector2(24, 16), new Vector2(-24, -16), Color.white);
        panel.SetActive(false);
        StretchText("MatchProgress", panel.transform, "Matched 0/6", 22, TextAlignmentOptions.Top, new Color(0.10f, 0.18f, 0.28f), 10);
        var left = Panel(panel.transform, "LeftCol", new Vector2(0.04f, 0.08f), new Vector2(0.48f, 0.88f), Vector2.zero, Vector2.zero, new Color(0.94f, 0.97f, 1f));
        var right = Panel(panel.transform, "RightCol", new Vector2(0.52f, 0.08f), new Vector2(0.96f, 0.88f), Vector2.zero, Vector2.zero, new Color(0.94f, 1f, 0.96f));
        var l = left.AddComponent<VerticalLayoutGroup>(); l.spacing = 8; l.padding = new RectOffset(10, 10, 10, 10); l.childForceExpandHeight = true;
        var r = right.AddComponent<VerticalLayoutGroup>(); r.spacing = 8; r.padding = new RectOffset(10, 10, 10, 10); r.childForceExpandHeight = true;
        string[] leftLabels = { "Forward Bias", "Reverse Bias", "Correct diode direction", "Opposite diode direction", "Bulb glows", "Bulb does not glow" };
        string[] rightLabels = { "Current can flow", "Current is blocked", "Forward bias", "Reverse bias" };
        for (int i = 0; i < leftLabels.Length; i++)
            Btn("MatchL" + i, left.transform, leftLabels[i], new Color(0.16f, 0.42f, 0.62f), 0, 56);
        for (int i = 0; i < rightLabels.Length; i++)
            Btn("MatchR" + i, right.transform, rightLabels[i], new Color(0.16f, 0.55f, 0.42f), 0, 70);
    }

    private void CreateChallenge(Transform main, Color green)
    {
        var panel = Panel(main, "ChallengePanel", new Vector2(0f, 0f), new Vector2(1f, 0.16f), Vector2.zero, Vector2.zero, new Color(0.10f, 0.22f, 0.34f));
        panel.SetActive(false);
        var h = panel.AddComponent<HorizontalLayoutGroup>();
        h.padding = new RectOffset(12, 12, 10, 10);
        h.spacing = 12;
        h.childForceExpandWidth = true;
        Btn("ChallengeDiodeBtn", panel.transform, "SET DIODE FORWARD", green, 0, 56);
        Btn("ChallengeBatteryBtn", panel.transform, "SET BATTERY NORMAL", new Color(0.12f, 0.52f, 0.72f), 0, 56);
    }

    private void CreateQuestions(Transform main)
    {
        var panel = Panel(main, "QuestionPanel", new Vector2(0.48f, 0f), Vector2.one, new Vector2(12, 12), new Vector2(-16, -12), Color.white);
        panel.SetActive(false);
        var qText = StretchText("QuestionText", panel.transform, "QUESTION", 26, TextAlignmentOptions.TopLeft, new Color(0.10f, 0.18f, 0.28f), 18);
        qText.rectTransform.anchorMin = new Vector2(0f, 0.78f);
        qText.rectTransform.anchorMax = new Vector2(1f, 1f);
        var diagram = Panel(panel.transform, "DiodeDiagram", new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.78f), Vector2.zero, Vector2.zero, new Color(0.10f, 0.22f, 0.34f));
        diagram.SetActive(false);
        StretchText("Diag", diagram.transform, "Anode  →  |>|  →  Cathode     (IN4001 band = cathode)", 20, TextAlignmentOptions.Center, Color.white, 8);
        var options = Panel(panel.transform, "OptionsGroup", new Vector2(0.04f, 0.16f), new Vector2(0.96f, 0.76f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var v = options.AddComponent<VerticalLayoutGroup>();
        v.spacing = 10;
        v.childForceExpandHeight = false;
        v.childControlHeight = true;
        v.childAlignment = TextAnchor.UpperCenter;
        ChoiceBtn("QuestionA", options.transform, "A", "");
        ChoiceBtn("QuestionB", options.transform, "B", "");
        ChoiceBtn("QuestionC", options.transform, "C", "");
        ChoiceBtn("QuestionD", options.transform, "D", "");
        var explain = Panel(panel.transform, "Explain", new Vector2(0.04f, 0.02f), new Vector2(0.68f, 0.14f), Vector2.zero, Vector2.zero, new Color(0.94f, 0.97f, 1f));
        explain.SetActive(false);
        StretchText("ExplainText", explain.transform, "", 18, TextAlignmentOptions.MidlineLeft, new Color(0.10f, 0.18f, 0.28f), 8);
        BigBtn("QuestionContinue", panel.transform, "CONTINUE", new Vector2(0.86f, 0.08f), Vector2.zero, new Vector2(160, 48), new Color(0.12f, 0.52f, 0.72f));
    }

    private void CreateConclusion(Transform main, Color green)
    {
        var panel = Panel(main, "ConclusionPanel", Vector2.zero, Vector2.one, new Vector2(28, 16), new Vector2(-28, -16), Color.white);
        panel.SetActive(false);
        var previewBox = Panel(panel.transform, "PreviewBox", new Vector2(0.04f, 0.58f), new Vector2(0.96f, 0.96f), Vector2.zero, Vector2.zero, new Color(0.93f, 0.97f, 0.94f));
        StretchText("PreviewTitle", previewBox.transform, "YOUR CONCLUSION", 18, TextAlignmentOptions.TopLeft, new Color(0.08f, 0.38f, 0.28f), 10)
            .rectTransform.anchorMax = new Vector2(1f, 1f);
        var preview = StretchText("ConclusionPreview", previewBox.transform, "Tap the three statements below in the correct order.", 22, TextAlignmentOptions.TopLeft, new Color(0.10f, 0.18f, 0.28f), 14);
        preview.rectTransform.anchorMin = new Vector2(0f, 0f);
        preview.rectTransform.anchorMax = new Vector2(1f, 0.82f);
        var row = Panel(panel.transform, "Phrases", new Vector2(0.04f, 0.16f), new Vector2(0.96f, 0.55f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var v = row.AddComponent<VerticalLayoutGroup>();
        v.spacing = 12;
        v.childForceExpandHeight = true;
        v.childControlHeight = true;
        v.padding = new RectOffset(0, 0, 4, 4);
        Btn("Phrase0", row.transform, "In reverse bias, the diode blocks current and the bulb does not glow.", new Color(0.16f, 0.42f, 0.62f), 0, 72);
        Btn("Phrase1", row.transform, "A diode allows current to flow mainly in one direction.", new Color(0.16f, 0.42f, 0.62f), 0, 72);
        Btn("Phrase2", row.transform, "In forward bias, current can flow and the bulb glows.", new Color(0.16f, 0.42f, 0.62f), 0, 72);
        BigBtn("ConclusionContinue", panel.transform, "CONTINUE", new Vector2(0.5f, 0.07f), Vector2.zero, new Vector2(280, 70), green);
    }

    private void CreateResult(Transform main, Color green)
    {
        var panel = Panel(main, "ResultPanel", Vector2.zero, Vector2.one, new Vector2(24, 12), new Vector2(-24, -12), Color.white);
        panel.SetActive(false);
        StretchText("FinalScore", panel.transform, "PRACTICAL COMPLETED", 26, TextAlignmentOptions.TopLeft, new Color(0.10f, 0.18f, 0.28f), 16).rectTransform.anchorMax = new Vector2(0.55f, 0.98f);
        StretchText("ResultDetails", panel.transform, "RESULTS", 20, TextAlignmentOptions.TopLeft, new Color(0.10f, 0.18f, 0.28f), 12).rectTransform.anchorMin = new Vector2(0.56f, 0.16f);
        StretchText("StatusText", panel.transform, "STATUS", 22, TextAlignmentOptions.BottomLeft, new Color(0.12f, 0.52f, 0.28f), 12).rectTransform.anchorMax = new Vector2(0.55f, 0.14f);
        var row = Panel(panel.transform, "ResultBtns", new Vector2(0.04f, 0.02f), new Vector2(0.55f, 0.12f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var h = row.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 10; h.childForceExpandWidth = true;
        Btn("ViewResultsBtn", row.transform, "VIEW RESULTS", new Color(0.12f, 0.52f, 0.72f), 0, 56);
        Btn("ViewProfileBtn", row.transform, "BACK TO PROFILE", green, 0, 56);
    }

    private void CreateFeedback(Transform canvas)
    {
        var panel = Panel(canvas, "FeedbackPanel", new Vector2(0.22f, 0.38f), new Vector2(0.78f, 0.68f), Vector2.zero, Vector2.zero, Color.white);
        panel.SetActive(false);
        var cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        StretchText("FeedbackText", panel.transform, "✓ CORRECT", 30, TextAlignmentOptions.Top, new Color(0.08f, 0.52f, 0.22f), 18).rectTransform.anchorMax = new Vector2(1f, 0.92f);
        StretchText("ScoreChangeText", panel.transform, "+5 MARKS", 26, TextAlignmentOptions.Bottom, new Color(0.10f, 0.18f, 0.28f), 16).rectTransform.anchorMin = new Vector2(0f, 0.08f);
    }

    private bool WireReferences(bool showWelcome)
    {
        var canvas = transform.Find("Canvas");
        if (canvas == null) return false;
        var refs = canvas.GetComponent<ElectronicsUIRefs>() ?? canvas.gameObject.AddComponent<ElectronicsUIRefs>();
        Transform c = canvas;
        Transform main = c.Find("MainArea");
        Transform header = c.Find("Header");
        Transform bottom = c.Find("BottomBar");
        Transform lab = main.Find("LaboratoryPanel");
        Transform board = lab != null ? lab.Find("BreadboardArea") : null;
        Transform tools = lab != null ? lab.Find("CircuitTools") : null;
        Transform q = main.Find("QuestionPanel");
        Transform match = main.Find("MatchPanel");
        Transform eq = main.Find("EquipmentPanel");
        Transform conc = main.Find("ConclusionPanel");
        Transform res = main.Find("ResultPanel");
        Transform obs = main.Find("ObservationPanel");
        Transform fb = c.Find("FeedbackPanel");

        refs.Title = Tmp(header, "Title");
        refs.Score = Tmp(header, "Score");
        refs.Progress = Tmp(header, "Progress");
        refs.Attempts = Tmp(header, "Attempts");
        refs.StepLabel = Tmp(header, "StepLabel");
        refs.ProgressBar = header.Find("ProgressBarBg/Fill")?.GetComponent<Image>();
        refs.Instruction = Tmp(c.Find("InstructionBar"), "Instruction");
        refs.InstructionBar = c.Find("InstructionBar")?.gameObject;
        refs.IntroPanel = main.Find("IntroPanel")?.gameObject;
        refs.IntroText = Tmp(main.Find("IntroPanel"), "IntroText");
        refs.StartBtn = main.Find("IntroPanel/StartBtn")?.GetComponent<Button>();
        refs.TheoryPanel = main.Find("TheoryPanel")?.gameObject;
        refs.TheoryText = Tmp(main.Find("TheoryPanel"), "TheoryText");
        refs.TheoryContinue = main.Find("TheoryPanel/TheoryContinue")?.GetComponent<Button>();
        refs.EquipmentPanel = eq?.gameObject;
        refs.CardContainer = eq != null ? eq.Find("Scroll/Viewport/Content") : null;
        refs.RequiredArea = eq != null ? eq.Find("RequiredArea/Content") : null;
        refs.CardPrefab = transform.Find("EquipmentCardPrefab")?.gameObject;
        refs.LaboratoryPanel = lab?.gameObject;
        refs.ObservationPanel = obs?.gameObject;
        refs.ObservationTable = Tmp(obs, "ObservationTable");
        refs.ObsGlow = obs != null ? obs.Find("ObsBtns/ObsGlow")?.GetComponent<Button>() : null;
        refs.ObsDark = obs != null ? obs.Find("ObsBtns/ObsDark")?.GetComponent<Button>() : null;
        refs.ComparePanel = main.Find("ComparePanel")?.gameObject;
        refs.CompareText = Tmp(main.Find("ComparePanel"), "CompareText");
        refs.MatchPanel = match?.gameObject;
        refs.MatchProgress = Tmp(match, "MatchProgress");
        refs.ChallengePanel = main.Find("ChallengePanel")?.gameObject;
        refs.ChallengeDiodeBtn = main.Find("ChallengePanel/ChallengeDiodeBtn")?.GetComponent<Button>();
        refs.ChallengeBatteryBtn = main.Find("ChallengePanel/ChallengeBatteryBtn")?.GetComponent<Button>();
        refs.QuestionPanel = q?.gameObject;
        refs.QuestionText = Tmp(q, "QuestionText");
        refs.QuestionA = q != null ? q.Find("OptionsGroup/QuestionA")?.GetComponent<Button>() : null;
        refs.QuestionB = q != null ? q.Find("OptionsGroup/QuestionB")?.GetComponent<Button>() : null;
        refs.QuestionC = q != null ? q.Find("OptionsGroup/QuestionC")?.GetComponent<Button>() : null;
        refs.QuestionD = q != null ? q.Find("OptionsGroup/QuestionD")?.GetComponent<Button>() : null;
        refs.QuestionContinue = q != null ? q.Find("QuestionContinue")?.GetComponent<Button>() : null;
        refs.QuestionExplanationPanel = q != null ? q.Find("Explain")?.gameObject : null;
        refs.QuestionExplanationText = q != null ? Tmp(q.Find("Explain"), "ExplainText") : null;
        refs.OptionsGroup = q != null ? q.Find("OptionsGroup")?.gameObject : null;
        refs.DiodeDiagram = q != null ? q.Find("DiodeDiagram")?.gameObject : null;
        refs.ConclusionPanel = conc?.gameObject;
        refs.ConclusionPreview = conc != null ? Tmp(conc.Find("PreviewBox"), "ConclusionPreview") : null;
        refs.ConclusionContinue = conc != null ? conc.Find("ConclusionContinue")?.GetComponent<Button>() : null;
        refs.PhraseButtons = new[]
        {
            conc != null ? conc.Find("Phrases/Phrase0")?.GetComponent<Button>() : null,
            conc != null ? conc.Find("Phrases/Phrase1")?.GetComponent<Button>() : null,
            conc != null ? conc.Find("Phrases/Phrase2")?.GetComponent<Button>() : null
        };
        refs.ResultPanel = res?.gameObject;
        refs.FinalScore = Tmp(res, "FinalScore");
        refs.ResultDetails = Tmp(res, "ResultDetails");
        refs.StatusText = Tmp(res, "StatusText");
        refs.ViewResultsBtn = res != null ? res.Find("ResultBtns/ViewResultsBtn")?.GetComponent<Button>() : null;
        refs.ViewProfileBtn = res != null ? res.Find("ResultBtns/ViewProfileBtn")?.GetComponent<Button>() : null;
        refs.Next = bottom.Find("Next")?.GetComponent<Button>();
        refs.Reset = bottom.Find("Reset")?.GetComponent<Button>();
        refs.Retry = bottom.Find("Retry")?.GetComponent<Button>();
        refs.ResetConfirm = c.Find("ResetConfirm")?.gameObject;
        refs.ResetYes = c.Find("ResetConfirm/Btns/Yes")?.GetComponent<Button>();
        refs.ResetNo = c.Find("ResetConfirm/Btns/No")?.GetComponent<Button>();
        refs.RetryConfirm = c.Find("RetryConfirm")?.gameObject;
        refs.RetryYes = c.Find("RetryConfirm/Btns/Yes")?.GetComponent<Button>();
        refs.RetryNo = c.Find("RetryConfirm/Btns/No")?.GetComponent<Button>();
        refs.FeedbackPanel = fb?.gameObject;
        refs.FeedbackText = Tmp(fb, "FeedbackText");
        refs.ScoreChangeText = Tmp(fb, "ScoreChangeText");
        refs.FeedbackGroup = fb != null ? fb.GetComponent<CanvasGroup>() : null;
        refs.CircuitTools = tools?.gameObject;
        refs.BatteryTools = tools != null ? tools.Find("BatteryTools")?.gameObject : null;
        refs.SwitchBtn = tools != null ? tools.Find("SwitchBtn")?.GetComponent<Button>() : null;
        refs.FlipDiodeBtn = tools != null ? tools.Find("FlipDiodeBtn")?.GetComponent<Button>() : null;
        refs.DisconnectBtn = tools != null ? tools.Find("BatteryTools/DisconnectBtn")?.GetComponent<Button>() : null;
        refs.ReverseBtn = tools != null ? tools.Find("BatteryTools/ReverseBtn")?.GetComponent<Button>() : null;
        refs.ReconnectBtn = tools != null ? tools.Find("BatteryTools/ReconnectBtn")?.GetComponent<Button>() : null;
        refs.CircuitStatus = board != null ? Tmp(board, "CircuitStatus") : null;
        refs.BulbStatus = board != null ? Tmp(board, "BulbStatus") : null;
        refs.BoardZone = board != null ? board.Find("BoardZone")?.GetComponent<ElectronicsUIDropTarget>() : null;
        refs.BatteryZone = board != null ? board.Find("BatteryZone")?.GetComponent<ElectronicsUIDropTarget>() : null;
        refs.SwitchZone = board != null ? board.Find("SwitchZone")?.GetComponent<ElectronicsUIDropTarget>() : null;
        refs.DiodeZone = board != null ? board.Find("DiodeZone")?.GetComponent<ElectronicsUIDropTarget>() : null;
        refs.BulbZone = board != null ? board.Find("BulbZone")?.GetComponent<ElectronicsUIDropTarget>() : null;
        refs.Wire1Zone = board != null ? board.Find("WireBatterySwitch")?.GetComponent<ElectronicsUIDropTarget>() : null;
        refs.Wire2Zone = board != null ? board.Find("WireSwitchDiode")?.GetComponent<ElectronicsUIDropTarget>() : null;
        refs.Wire3Zone = board != null ? board.Find("WireDiodeBulb")?.GetComponent<ElectronicsUIDropTarget>() : null;
        refs.Wire4Zone = board != null ? board.Find("WireBulbBattery")?.GetComponent<ElectronicsUIDropTarget>() : null;
        refs.Wire1 = board != null ? board.Find("W1")?.GetComponent<Image>() : null;
        refs.Wire2 = board != null ? board.Find("W2")?.GetComponent<Image>() : null;
        refs.Wire3 = board != null ? board.Find("W3")?.GetComponent<Image>() : null;
        refs.Wire4 = board != null ? board.Find("W4")?.GetComponent<Image>() : null;
        refs.Tray = lab != null ? lab.Find("Tray/Scroll/Viewport/Content") : null;
        refs.DiodeIcon = board != null ? board.Find("DiodeZone/DiodeIcon")?.GetComponent<Image>() : null;
        refs.AnodeLabel = board != null ? Tmp(board.Find("DiodeZone"), "AnodeLabel") : null;
        refs.CathodeLabel = board != null ? Tmp(board.Find("DiodeZone"), "CathodeLabel") : null;
        refs.DiodeStatus = board != null ? Tmp(board.Find("DiodeZone"), "DiodeStatus") : null;
        refs.BatteryIcon = board != null ? board.Find("BatteryZone/BatteryIcon")?.GetComponent<Image>() : null;
        refs.BatteryVisual = board != null ? board.Find("BatteryZone/BatteryIcon") as RectTransform : null;
        refs.PolarityLabel = board != null ? Tmp(board.Find("BatteryZone"), "PolarityLabel") : null;
        refs.VoltageLabel = board != null ? Tmp(board.Find("BatteryZone"), "VoltageLabel") : null;
        refs.BulbIcon = board != null ? board.Find("BulbZone/BulbIcon")?.GetComponent<Image>() : null;
        refs.BulbGlow = board != null ? board.Find("BulbZone/BulbGlow")?.GetComponent<Image>() : null;
        refs.BulbRays = board != null ? board.Find("BulbZone/BulbRays")?.GetComponent<Image>() : null;
        refs.LocalBulbStatus = board != null ? Tmp(board.Find("BulbZone"), "LocalBulbStatus") : null;
        refs.SwitchIcon = board != null ? board.Find("SwitchZone/SwitchIcon")?.GetComponent<Image>() : null;
        refs.SwitchLabel = board != null ? Tmp(board.Find("SwitchZone"), "SwitchLabel") : null;
        refs.BoardGlow = board != null ? board.Find("Glow")?.GetComponent<Image>() : null;

        if (match != null)
        {
            refs.MatchLeft = new Button[6];
            refs.MatchRight = new Button[4];
            for (int i = 0; i < 6; i++)
                refs.MatchLeft[i] = match.Find("LeftCol/MatchL" + i)?.GetComponent<Button>();
            for (int i = 0; i < 4; i++)
                refs.MatchRight[i] = match.Find("RightCol/MatchR" + i)?.GetComponent<Button>();
        }

        ElectronicsUIManager.Instance?.BindAll(refs, showWelcome);
        ElectronicsFeedbackManager.Instance?.BindUI(refs.FeedbackPanel, refs.FeedbackText, refs.ScoreChangeText, refs.FeedbackGroup);
        ElectronicsResultManager.Instance?.Bind(refs.FinalScore, refs.ResultDetails, refs.StatusText);
        ElectronicsEquipmentSelectionManager.Instance?.SetupUI(refs.CardContainer, refs.RequiredArea, refs.CardPrefab);
        ElectronicsLabEquipmentTray.Instance?.Bind(refs.Tray);
        ElectronicsCircuitBoardManager.Instance?.Bind(board?.gameObject, refs.CircuitStatus, refs.BoardGlow);
        ElectronicsCircuitConnectionManager.Instance?.Bind(refs.BoardZone, refs.BatteryZone, refs.SwitchZone, refs.DiodeZone, refs.BulbZone, refs.Wire1Zone, refs.Wire2Zone, refs.Wire3Zone, refs.Wire4Zone, refs.CircuitStatus);
        ElectronicsWireController.Instance?.BindWire(ElectronicsWireController.BatteryToSwitch, refs.Wire1);
        ElectronicsWireController.Instance?.BindWire(ElectronicsWireController.SwitchToDiode, refs.Wire2);
        ElectronicsWireController.Instance?.BindWire(ElectronicsWireController.DiodeToBulb, refs.Wire3);
        ElectronicsWireController.Instance?.BindWire(ElectronicsWireController.BulbToBattery, refs.Wire4);
        ElectronicsBatteryController.Instance?.Bind(refs.BatteryVisual, refs.BatteryIcon, refs.PolarityLabel, refs.VoltageLabel);
        ElectronicsDiodeController.Instance?.Bind(refs.DiodeIcon, refs.AnodeLabel, refs.CathodeLabel, refs.DiodeStatus);
        ElectronicsBulbController.Instance?.Bind(refs.BulbIcon, refs.BulbGlow, refs.BulbRays, refs.LocalBulbStatus);
        ElectronicsSwitchController.Instance?.Bind(refs.SwitchIcon, refs.SwitchLabel, refs.SwitchBtn);
        ElectronicsObservationManager.Instance?.Bind(refs.ObservationTable);
        ElectronicsConclusionManager.Instance?.BindPhrases(refs.PhraseButtons);
        ElectronicsFailsafeDisplay.Hide();
        
        return ElectronicsUIManager.Instance != null;
    }

    private static TextMeshProUGUI Tmp(Transform parent, string child)
    {
        if (parent == null) return null;
        var t = parent.Find(child);
        return t != null ? t.GetComponent<TextMeshProUGUI>() : parent.GetComponent<TextMeshProUGUI>();
    }

    private ElectronicsUIDropTarget DropZone(Transform parent, string name, string accept, Vector2 aMin, Vector2 aMax, string hint)
    {
        var obj = Panel(parent, name, aMin, aMax, Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.92f));
        var zone = obj.AddComponent<ElectronicsUIDropTarget>();
        zone.Configure(name, accept, Vector2.zero);
        StretchText("Hint", obj.transform, hint, 14, TextAlignmentOptions.Center, new Color(0.28f, 0.32f, 0.40f), 6);
        return zone;
    }

    private Image CreateWirePath(Transform parent, string name, params (Vector2 min, Vector2 max)[] segs)
    {
        var root = Panel(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear, false);
        root.SetActive(false);
        var copper = new Color(0.72f, 0.22f, 0.14f, 1f);
        for (int i = 0; i < segs.Length; i++)
        {
            var bar = Panel(root.transform, "Seg" + i, segs[i].min, segs[i].max, Vector2.zero, Vector2.zero, copper, false);
            bar.GetComponent<Image>().raycastTarget = false;
        }
        return root.GetComponent<Image>();
    }

    private void Terminal(Transform parent, string id, Vector2 aMin, Vector2 aMax, Color color, string label)
    {
        var obj = Panel(parent, id, aMin, aMax, Vector2.zero, Vector2.zero, color);
        StretchText("L", obj.transform, label, 12, TextAlignmentOptions.Center, Color.white, 2);
        obj.AddComponent<ElectronicsTerminalTap>().Configure(id == "DiodeAnode" ? "DiodeAnode" : id);
        if (id == "DiodeAnode") obj.GetComponent<ElectronicsTerminalTap>().Configure("DiodeAnode");
        if (id == "DiodeCathode") obj.GetComponent<ElectronicsTerminalTap>().Configure("DiodeCathode");
        if (id == "Battery+") obj.GetComponent<ElectronicsTerminalTap>().Configure("Battery+");
        if (id == "Battery-") obj.GetComponent<ElectronicsTerminalTap>().Configure("Battery-");
    }

    private GameObject Panel(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax, Color color, bool raycast = true)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = offMin; rt.offsetMax = offMax;
        var img = obj.AddComponent<Image>();
        img.sprite = ElectronicsIconFactory.White();
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
        le.preferredHeight = 88; le.minHeight = 80;
        obj.AddComponent<Image>().color = Color.white;
        var btn = obj.AddComponent<Button>();
        var letterBg = Panel(obj.transform, "Letter", new Vector2(0f, 0.12f), new Vector2(0f, 0.88f), new Vector2(10, 0), new Vector2(64, 0), new Color(0.12f, 0.52f, 0.72f), false);
        StretchText("LetterText", letterBg.transform, letter, 24, TextAlignmentOptions.Center, Color.white, 0).fontStyle = FontStyles.Bold;
        var bodyObj = new GameObject("Body");
        bodyObj.transform.SetParent(obj.transform, false);
        var bodyRt = bodyObj.AddComponent<RectTransform>();
        bodyRt.anchorMin = Vector2.zero; bodyRt.anchorMax = Vector2.one;
        bodyRt.offsetMin = new Vector2(78, 6); bodyRt.offsetMax = new Vector2(-12, -6);
        var body = bodyObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) body.font = defaultFont;
        body.text = text; body.fontSize = 22; body.fontStyle = FontStyles.Bold;
        body.alignment = TextAlignmentOptions.MidlineLeft;
        body.color = new Color(0.10f, 0.18f, 0.28f);
        body.raycastTarget = false;
        body.enableAutoSizing = true;
        body.fontSizeMin = 11;
        body.fontSizeMax = 18;
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
        le.preferredHeight = h; le.minHeight = h;
        var img = obj.AddComponent<Image>();
        img.sprite = ElectronicsIconFactory.White();
        img.color = bg;
        var btn = obj.AddComponent<Button>();
        var txt = StretchText("Text", obj.transform, label, 22, TextAlignmentOptions.Center, Color.white, 4);
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

    private void ConfirmDialog(Transform parent, string name, string message)
    {
        var panel = Panel(parent, name, new Vector2(0.28f, 0.38f), new Vector2(0.72f, 0.62f), Vector2.zero, Vector2.zero, Color.white);
        panel.SetActive(false);
        StretchText("Msg", panel.transform, message, 24, TextAlignmentOptions.Center, new Color(0.10f, 0.18f, 0.28f), 16);
        var row = Panel(panel.transform, "Btns", new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.38f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 18;
        layout.childForceExpandWidth = true;
        Btn("Yes", row.transform, "YES", new Color(0.75f, 0.2f, 0.2f), 0, 50);
        Btn("No", row.transform, "NO", new Color(0.15f, 0.48f, 0.78f), 0, 50);
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
        content.AddComponent<ElectronicsAdaptiveGrid>();
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
        card.AddComponent<ElectronicsEquipmentCardUI>();
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
}

public class ElectronicsUIRefs : MonoBehaviour
{
    public TextMeshProUGUI Title, Score, Progress, Attempts, StepLabel, Instruction;
    public TextMeshProUGUI IntroText, TheoryText, ConclusionPreview, CompareText, QuestionText, ObservationTable;
    public TextMeshProUGUI FinalScore, ResultDetails, StatusText, FeedbackText, ScoreChangeText;
    public TextMeshProUGUI QuestionExplanationText, CircuitStatus, MatchProgress, BulbStatus;
    public TextMeshProUGUI AnodeLabel, CathodeLabel, DiodeStatus, PolarityLabel, VoltageLabel, SwitchLabel, LocalBulbStatus;
    public Image ProgressBar, Wire1, Wire2, Wire3, Wire4, DiodeIcon, BatteryIcon, BulbIcon, BulbGlow, BulbRays, SwitchIcon, BoardGlow;
    public RectTransform BatteryVisual;
    public GameObject IntroPanel, TheoryPanel, InstructionBar, EquipmentPanel, LaboratoryPanel;
    public GameObject ObservationPanel, ComparePanel, MatchPanel, ChallengePanel, QuestionPanel, ConclusionPanel, ResultPanel;
    public GameObject ResetConfirm, RetryConfirm, FeedbackPanel, CardPrefab, QuestionExplanationPanel, OptionsGroup, DiodeDiagram;
    public GameObject CircuitTools, BatteryTools;
    public Transform CardContainer, RequiredArea, Tray;
    public Button StartBtn, Next, Reset, Retry, ResetYes, ResetNo, RetryYes, RetryNo, ViewProfileBtn, ViewResultsBtn;
    public Button TheoryContinue, QuestionA, QuestionB, QuestionC, QuestionD, QuestionContinue;
    public Button ObsGlow, ObsDark, DisconnectBtn, ReverseBtn, ReconnectBtn, FlipDiodeBtn, SwitchBtn;
    public Button ChallengeDiodeBtn, ChallengeBatteryBtn, ConclusionContinue;
    public Button[] PhraseButtons, MatchLeft, MatchRight;
    public CanvasGroup FeedbackGroup;
    public ElectronicsUIDropTarget BoardZone, BatteryZone, SwitchZone, DiodeZone, BulbZone, Wire1Zone, Wire2Zone, Wire3Zone, Wire4Zone;
    public int UiVersion;
}

public class ElectronicsAdaptiveGrid : MonoBehaviour
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

public class ElectronicsAdaptiveLab : MonoBehaviour
{
    private RectTransform tray;
    private ScrollRect trayScroll;
    private RectTransform trayItems;
    private RectTransform board;
    private RectTransform tools;
    private int lastW;
    private int lastH;

    public void Bind(RectTransform trayRt, ScrollRect scroll, RectTransform items, RectTransform boardRt, RectTransform toolsRt)
    {
        tray = trayRt;
        trayScroll = scroll;
        trayItems = items;
        board = boardRt;
        tools = toolsRt;
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
            Set(board, 0.01f, 0.34f, 0.99f, 0.99f);
            Set(tools, 0.01f, 0.18f, 0.99f, 0.33f);
            Set(tray, 0.01f, 0.01f, 0.99f, 0.17f);
            SetTrayHorizontal(true);
        }
        else
        {
            Set(tray, 0.00f, 0.00f, 0.20f, 1f);
            Set(board, 0.21f, 0.18f, 1f, 1f);
            Set(tools, 0.21f, 0.00f, 1f, 0.17f);
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
