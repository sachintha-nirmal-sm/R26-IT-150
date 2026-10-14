using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

[DefaultExecutionOrder(-50)]
public class CurrentElectricitySceneRuntimeBuilder : MonoBehaviour
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
            Debug.LogWarning("CurrentElectricity: wiring failed.");
            return;
        }
        referencesWired = true;
    }

    private void Awake()
    {
        if (GetComponent<ElecFailsafeDisplay>() == null)
            gameObject.AddComponent<ElecFailsafeDisplay>();
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
            Debug.Log("Current Electricity practical UI built.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Current Electricity BUILD FAILED: " + ex.Message + "\n" + ex.StackTrace);
        }
    }

    private void LoadFont()
    {
        try
        {
            defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (defaultFont == null)
                defaultFont = TMP_Settings.defaultFontAsset;
#if UNITY_EDITOR
            if (defaultFont == null)
                defaultFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
#endif
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("CurrentElectricity: font load skipped — " + ex.Message);
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
                else
                    DestroyImmediate(es.gameObject);
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
            Debug.LogWarning("CurrentElectricity EventSystem: " + ex.Message);
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
        if (existing != null)
            DestroyImmediate(existing.gameObject);
        managersRoot = new GameObject("Managers").transform;
        managersRoot.SetParent(transform, false);
        AddMgr<CurrentElectricityExperimentManager>("CurrentElectricityExperimentManager");
        AddMgr<ElecUIManager>("ElecUIManager");
        AddMgr<ElecScoreManager>("ElecScoreManager");
        AddMgr<ElecFeedbackManager>("ElecFeedbackManager");
        AddMgr<ElecAttemptManager>("ElecAttemptManager");
        AddMgr<ElecSaveManager>("ElecSaveManager");
        AddMgr<ElecProfileManager>("ElecProfileManager");
        AddMgr<ElecExperimentDataManager>("ElecExperimentDataManager");
        AddMgr<ElecEquipmentSelectionManager>("ElecEquipmentSelectionManager");
        AddMgr<CircuitBuilder>("CircuitBuilder");
        AddMgr<CircuitValidator>("CircuitValidator");
        AddMgr<CircuitCalculationManager>("CircuitCalculationManager");
        AddMgr<VoltageMeasurementManager>("VoltageMeasurementManager");
        AddMgr<CurrentMeasurementManager>("CurrentMeasurementManager");
        AddMgr<PowerCalculationManager>("PowerCalculationManager");
        AddMgr<WireDragController>("WireDragController");
        AddMgr<Connection1Manager>("Connection1Manager");
        AddMgr<Connection2Manager>("Connection2Manager");
        AddMgr<Connection3Manager>("Connection3Manager");
        AddMgr<ElecObservationTableManager>("ElecObservationTableManager");
        AddMgr<ElecGraphController>("ElecGraphController");
        AddMgr<ElecQuestionManager>("ElecQuestionManager");
        AddMgr<ElecResultManager>("ElecResultManager");
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
        Color accent = new Color(0.12f, 0.52f, 0.78f);
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
        var title = Text("Title", headerP.transform, "CURRENT ELECTRICITY    Activity: Two Dry Cells", 28, TextAlignmentOptions.MidlineLeft, new Vector2(16, -6), new Vector2(1200, 44));
        title.color = Color.white;
        title.enableAutoSizing = true;
        title.fontSizeMin = 14;
        title.fontSizeMax = 28;
        title.overflowMode = TextOverflowModes.Ellipsis;
        var score = Text("Score", headerP.transform, "Score: 0 / 100", 24, TextAlignmentOptions.MidlineRight, new Vector2(-16, -6), new Vector2(340, 44), Vector2.one, Vector2.one);
        score.color = Color.white;
        score.enableAutoSizing = true;
        score.fontSizeMin = 12;
        score.fontSizeMax = 24;
        var progress = Text("Progress", headerP.transform, "Step: 1 / 14", 20, TextAlignmentOptions.MidlineLeft, new Vector2(16, -46), new Vector2(220, 32));
        progress.color = new Color(0.85f, 0.93f, 1f);
        var connection = Text("ConnectionLabel", headerP.transform, "Connection: —", 20, TextAlignmentOptions.Center, new Vector2(0, -46), new Vector2(220, 32), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        connection.color = new Color(0.85f, 0.93f, 1f);
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
        fillImg.fillAmount = 0.06f;

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
        bottomLayout.childControlWidth = false;
        bottomLayout.childForceExpandWidth = false;
        var nextBtn = Btn("Next", bottom.transform, "NEXT STEP", accent, 240, 64);
        nextBtn.gameObject.SetActive(true);
        var resetBtn = Btn("Reset", bottom.transform, "Reset", new Color(0.45f, 0.48f, 0.52f), 150, 64);
        var retryBtn = Btn("Retry", bottom.transform, "RETRY", accent, 150, 64);
        retryBtn.gameObject.SetActive(false);

        var main = Panel(canvasObj.transform, "MainArea", new Vector2(0, 0.11f), new Vector2(1, 0.83f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);

        var introPanel = Panel(main.transform, "IntroPanel", Vector2.zero, Vector2.one, new Vector2(36, 24), new Vector2(-36, -24), Color.white);
        var introText = StretchText("IntroText", introPanel.transform, "", 32, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 28);
        var startBtn = BigBtn("StartBtn", introPanel.transform, "START PRACTICAL", new Vector2(0.5f, 0.08f), new Vector2(0, 0), new Vector2(480, 96), green);
        introPanel.AddComponent<ElecIntroClickToStart>();

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

        var requiredArea = Panel(equipPanel.transform, "RequiredArea", new Vector2(0, 0), new Vector2(1, 0.24f), new Vector2(8, 6), new Vector2(-8, -4), new Color(0.86f, 0.94f, 1f));
        var reqLabel = StretchText("ReqLabel", requiredArea.transform, "REQUIRED EQUIPMENT", 16, TextAlignmentOptions.MidlineLeft, new Color(0.16f, 0.28f, 0.42f), 6);
        var reqLabelRt = reqLabel.rectTransform;
        reqLabelRt.anchorMin = new Vector2(0f, 0.72f);
        reqLabelRt.anchorMax = Vector2.one;
        reqLabelRt.offsetMin = new Vector2(8, 0);
        reqLabelRt.offsetMax = new Vector2(-8, -2);
        var requiredCards = Panel(requiredArea.transform, "RequiredCards", new Vector2(0, 0), new Vector2(1, 0.72f), new Vector2(6, 4), new Vector2(-6, -2), new Color(0, 0, 0, 0), false);
        var reqLayout = requiredCards.AddComponent<HorizontalLayoutGroup>();
        reqLayout.spacing = 6;
        reqLayout.padding = new RectOffset(4, 4, 2, 2);
        reqLayout.childAlignment = TextAnchor.MiddleCenter;
        reqLayout.childControlWidth = true;
        reqLayout.childControlHeight = true;
        reqLayout.childForceExpandWidth = true;
        reqLayout.childForceExpandHeight = true;
        requiredCards.AddComponent<ElecUIDropTarget>().Configure("RequiredEquipment", "Any", Vector2.zero);

        var scroll = CreateScrollAnchored(equipPanel.transform, new Vector2(0f, 0.25f), new Vector2(1f, 0.85f));
        var scrollRt = scroll.GetComponent<RectTransform>();
        scrollRt.offsetMin = new Vector2(8, 4);
        scrollRt.offsetMax = new Vector2(-8, -4);
        var cardPrefab = CreateCardPrefab();
        var equipContinue = BigBtn("EquipContinueBtn", equipPanel.transform, "NEXT STEP", new Vector2(1f, 0f), new Vector2(-160, 18), new Vector2(240, 48), green);
        equipContinue.gameObject.SetActive(false);

        var tutorialPanel = MakeTextPanel(main.transform, "TutorialPanel", "TutTitle", "CIRCUIT CONSTRUCTION TUTORIAL", "TutorialText");

        var labPanel = Panel(main.transform, "LaboratoryPanel", Vector2.zero, Vector2.one, new Vector2(10, 6), new Vector2(-10, -6), new Color(0, 0, 0, 0), false);
        labPanel.SetActive(false);

        var board = Panel(labPanel.transform, "CircuitBoard", new Vector2(0.01f, 0.32f), new Vector2(0.68f, 0.99f), Vector2.zero, Vector2.zero, new Color(0.22f, 0.46f, 0.32f));
        board.GetComponent<Image>().sprite = ElecIconFactory.GetComponentSprite(ElectricalComponentType.CircuitBoard);
        board.GetComponent<Image>().preserveAspect = false;
        board.GetComponent<Image>().color = new Color(0.85f, 0.95f, 0.88f, 1f);
        var boardTarget = board.AddComponent<ElecUIDropTarget>();
        boardTarget.Configure("CircuitBoard", "Any", Vector2.zero);
        var boardLabel = Text("BoardLabel", board.transform, "CIRCUIT BOARD  —  drag parts here, then tap two terminals to join them with a wire", 18, TextAlignmentOptions.TopLeft, new Vector2(10, -6), new Vector2(820, 26));
        boardLabel.color = new Color(0.12f, 0.28f, 0.18f);
        boardLabel.enableAutoSizing = true;
        boardLabel.fontSizeMin = 11;
        boardLabel.fontSizeMax = 18;
        boardLabel.overflowMode = TextOverflowModes.Ellipsis;

        var side = Panel(labPanel.transform, "SidePanel", new Vector2(0.69f, 0.01f), new Vector2(0.99f, 0.99f), Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.96f));
        var meterInfo = StretchText("MeterInfo", side.transform, "Build the circuit.", 20, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 14);
        var meterRt = meterInfo.rectTransform;
        meterRt.anchorMin = new Vector2(0f, 0.42f);
        meterRt.anchorMax = new Vector2(1f, 1f);

        var vInput = CreateInput(side.transform, "VoltageInput", new Vector2(0.06f, 0.30f), new Vector2(0.94f, 0.40f), "Voltage (V)");
        var iInput = CreateInput(side.transform, "CurrentInput", new Vector2(0.06f, 0.18f), new Vector2(0.94f, 0.28f), "Current (A)");

        var brightRow = Panel(side.transform, "BrightRow", new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.16f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var bLayout = brightRow.AddComponent<HorizontalLayoutGroup>();
        bLayout.spacing = 6;
        bLayout.childForceExpandWidth = true;
        var brightHigh = Btn("BrightHigh", brightRow.transform, "High", amber, 0, 40);
        var brightMed = Btn("BrightMed", brightRow.transform, "Medium", new Color(0.75f, 0.65f, 0.2f), 0, 40);
        var brightOff = Btn("BrightOff", brightRow.transform, "OFF", new Color(0.45f, 0.48f, 0.52f), 0, 40);

        var tray = Panel(labPanel.transform, "EquipmentTray", new Vector2(0.01f, 0.01f), new Vector2(0.68f, 0.18f), Vector2.zero, Vector2.zero, new Color(0.95f, 0.97f, 1f, 0.96f));
        var trayLabel = Text("TrayLabel", tray.transform, "EQUIPMENT TRAY  —  drag these onto the board", 16, TextAlignmentOptions.TopLeft, new Vector2(10, -4), new Vector2(640, 22));
        trayLabel.enableAutoSizing = true;
        trayLabel.fontSizeMin = 11;
        trayLabel.fontSizeMax = 16;
        trayLabel.overflowMode = TextOverflowModes.Ellipsis;
        var trayInner = Panel(tray.transform, "Items", new Vector2(0f, 0f), new Vector2(1f, 0.78f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        trayInner.AddComponent<HorizontalLayoutGroup>();

        var actionRow = Panel(labPanel.transform, "ActionRow", new Vector2(0.01f, 0.18f), new Vector2(0.68f, 0.32f), Vector2.zero, Vector2.zero, new Color(0.16f, 0.22f, 0.28f));
        var aLayout = actionRow.AddComponent<GridLayoutGroup>();
        aLayout.cellSize = new Vector2(150, 34);
        aLayout.spacing = new Vector2(6, 6);
        aLayout.padding = new RectOffset(8, 8, 6, 6);
        aLayout.constraint = GridLayoutGroup.Constraint.Flexible;
        aLayout.childAlignment = TextAnchor.MiddleCenter;
        var checkBtn = Btn("CheckCircuit", actionRow.transform, "CHECK CIRCUIT", accent, 0, 34);
        var testBtn = Btn("TestCircuit", actionRow.transform, "TEST CIRCUIT", green, 0, 34);
        var measureVBtn = Btn("MeasureV", actionRow.transform, "MEASURE VOLTAGE", accent, 0, 34);
        var measureIBtn = Btn("MeasureI", actionRow.transform, "MEASURE CURRENT", accent, 0, 34);
        var recordBtn = Btn("Record", actionRow.transform, "RECORD READING", green, 0, 34);
        var undoBtn = Btn("UndoWire", actionRow.transform, "UNDO WIRE", new Color(0.5f, 0.45f, 0.2f), 0, 34);
        var rotateBtn = Btn("RotateCell", actionRow.transform, "ROTATE CELL", new Color(0.35f, 0.4f, 0.55f), 0, 34);
        testBtn.gameObject.SetActive(false);
        measureVBtn.gameObject.SetActive(false);
        measureIBtn.gameObject.SetActive(false);
        recordBtn.gameObject.SetActive(false);

        var hint = Text("Hint", labPanel.transform, "", 18, TextAlignmentOptions.MidlineLeft, new Vector2(8, 2), new Vector2(980, 22), new Vector2(0.01f, 0.32f), new Vector2(0.01f, 0.32f), new Vector2(0f, 0f));
        hint.color = new Color(0.15f, 0.28f, 0.22f);
        hint.enableAutoSizing = true;
        hint.fontSizeMin = 11;
        hint.fontSizeMax = 18;
        hint.overflowMode = TextOverflowModes.Ellipsis;
        labPanel.AddComponent<ElecAdaptiveLab>().Bind(
            board.GetComponent<RectTransform>(),
            side.GetComponent<RectTransform>(),
            tray.GetComponent<RectTransform>(),
            actionRow.GetComponent<RectTransform>());

        var dataTablePanel = MakeTextPanel(main.transform, "DataTablePanel", "TableTitle", "CONNECTION DATA TABLE", "DataTableText");
        var comparePanel = MakeTextPanel(main.transform, "ComparePanel", "CompareTitle", "COMPARE THE RESULTS", "CompareText");

        var graphPanel = Panel(main.transform, "GraphPanel", Vector2.zero, Vector2.one, new Vector2(20, 12), new Vector2(-20, -12), new Color(0.97f, 0.98f, 1f));
        graphPanel.SetActive(false);
        Text("GraphTitle", graphPanel.transform, "GRAPHS  —  generated from your experiment data", 26, TextAlignmentOptions.MidlineLeft, new Vector2(20, -12), new Vector2(900, 40));
        var vArea = Panel(graphPanel.transform, "VoltageGraph", new Vector2(0.04f, 0.52f), new Vector2(0.96f, 0.90f), Vector2.zero, Vector2.zero, new Color(0.92f, 0.95f, 1f));
        Text("VLabel", vArea.transform, "Connection →     Potential Difference (V) ↑", 16, TextAlignmentOptions.Bottom, new Vector2(0, 6), new Vector2(700, 24), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        var iArea = Panel(graphPanel.transform, "CurrentGraph", new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.48f), Vector2.zero, Vector2.zero, new Color(1f, 0.95f, 0.90f));
        Text("ILabel", iArea.transform, "Connection →     Current (A) ↑", 16, TextAlignmentOptions.Bottom, new Vector2(0, 6), new Vector2(700, 24), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        var dotPrefab = Panel(vArea.transform, "DotPrefab", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-7, -7), new Vector2(7, 7), accent);
        dotPrefab.SetActive(false);

        var educationPanel = MakeTextPanel(main.transform, "EducationPanel", "EduTitle", "EXPLANATION", "EducationText");
        var conclusionPanel = MakeTextPanel(main.transform, "ConclusionPanel", "ConcTitle", "CONCLUSION", "ConclusionText");

        var questionPanel = Panel(main.transform, "QuestionPanel", Vector2.zero, Vector2.one, new Vector2(18, 10), new Vector2(-18, -10), new Color(0.96f, 0.98f, 1f));
        questionPanel.SetActive(false);
        var qHeader = Panel(questionPanel.transform, "Header", new Vector2(0f, 0.78f), new Vector2(1f, 1f), new Vector2(12, 8), new Vector2(-12, -8), header);
        var questionText = StretchText("Question", qHeader.transform, "Question", 24, TextAlignmentOptions.MidlineLeft, Color.white, 14);
        var qOptions = Panel(questionPanel.transform, "Options", new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.76f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var vLayout = qOptions.AddComponent<VerticalLayoutGroup>();
        vLayout.spacing = 10;
        vLayout.padding = new RectOffset(8, 8, 8, 8);
        vLayout.childControlHeight = true;
        vLayout.childForceExpandHeight = true;
        vLayout.childControlWidth = true;
        vLayout.childForceExpandWidth = true;
        var optA = ChoiceBtn("OptA", qOptions.transform, "A", "");
        var optB = ChoiceBtn("OptB", qOptions.transform, "B", "");
        var optC = ChoiceBtn("OptC", qOptions.transform, "C", "");
        var optD = ChoiceBtn("OptD", qOptions.transform, "D", "");
        var explanationPanel = Panel(questionPanel.transform, "ExplanationPanel", new Vector2(0.08f, 0.10f), new Vector2(0.92f, 0.48f), Vector2.zero, Vector2.zero, new Color(0.12f, 0.46f, 0.32f));
        explanationPanel.SetActive(false);
        var explanationText = StretchText("ExplanationText", explanationPanel.transform, "", 22, TextAlignmentOptions.Top, Color.white, 16);
        var continueObj = Panel(explanationPanel.transform, "ContinueBtn", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-120, 12), new Vector2(120, 58), Color.white);
        var continueBtn = continueObj.AddComponent<Button>();
        StretchText("Text", continueObj.transform, "Continue ▶", 22, TextAlignmentOptions.Center, new Color(0.1f, 0.4f, 0.25f), 4).fontStyle = FontStyles.Bold;

        var resultPanel = Panel(main.transform, "ResultPanel", Vector2.zero, Vector2.one, new Vector2(28, 16), new Vector2(-28, -16), Color.white);
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

        var feedbackPanel = Panel(canvasObj.transform, "FeedbackPanel", new Vector2(0.20f, 0.40f), new Vector2(0.80f, 0.62f), Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.97f));
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

        var refs = canvasObj.AddComponent<ElecUIRefsHolder>();
        refs.UiVersion = 6;
        refs.Title = title; refs.Score = score; refs.Progress = progress; refs.Attempts = attempts;
        refs.ConnectionLabel = connection; refs.ProgressBar = fillImg; refs.Instruction = instruction;
        refs.IntroPanel = introPanel; refs.IntroText = introText; refs.StartBtn = startBtn;
        refs.ObjectivePanel = objectivePanel; refs.ObjectiveText = objectiveText;
        refs.InstructionBar = instructionBar; refs.EquipmentPanel = equipPanel;
        refs.TutorialPanel = tutorialPanel;
        refs.TutorialText = tutorialPanel.transform.Find("TutorialText")?.GetComponent<TextMeshProUGUI>();
        refs.LaboratoryPanel = labPanel;
        refs.DataTablePanel = dataTablePanel;
        refs.DataTableText = dataTablePanel.transform.Find("DataTableText")?.GetComponent<TextMeshProUGUI>();
        refs.ComparePanel = comparePanel;
        refs.CompareText = comparePanel.transform.Find("CompareText")?.GetComponent<TextMeshProUGUI>();
        refs.GraphPanel = graphPanel; refs.EducationPanel = educationPanel;
        refs.EducationText = educationPanel.transform.Find("EducationText")?.GetComponent<TextMeshProUGUI>();
        refs.QuestionPanel = questionPanel; refs.QuestionText = questionText;
        refs.ConclusionPanel = conclusionPanel;
        refs.ConclusionText = conclusionPanel.transform.Find("ConclusionText")?.GetComponent<TextMeshProUGUI>();
        refs.ResultPanel = resultPanel; refs.FinalScore = finalScore; refs.ResultDetails = resultDetails; refs.StatusText = statusText;
        refs.Next = nextBtn; refs.Reset = resetBtn; refs.Retry = retryBtn;
        refs.ViewProfileBtn = viewProfileBtn; refs.ViewResultsBtn = viewResultsBtn;
        refs.EquipContinueBtn = equipContinue;
        refs.ResetConfirm = resetConfirm; refs.ResetYes = resetYes; refs.ResetNo = resetNo;
        refs.FeedbackPanel = feedbackPanel; refs.FeedbackText = feedbackText; refs.ScoreChangeText = scoreChangeText;
        refs.FeedbackGroup = feedbackGroup;
        refs.CardContainer = scroll.content; refs.RequiredArea = requiredCards.transform; refs.CardPrefab = cardPrefab;
        refs.VoltageGraphArea = vArea.GetComponent<RectTransform>();
        refs.CurrentGraphArea = iArea.GetComponent<RectTransform>();
        refs.DotPrefab = dotPrefab;
        refs.QuestionA = optA; refs.QuestionB = optB; refs.QuestionC = optC; refs.QuestionD = optD;
        refs.QuestionContinue = continueBtn;
        refs.QuestionExplanationPanel = explanationPanel;
        refs.QuestionExplanationText = explanationText;
        refs.OptAText = optA.transform.Find("Body")?.GetComponent<TextMeshProUGUI>();
        refs.OptBText = optB.transform.Find("Body")?.GetComponent<TextMeshProUGUI>();
        refs.OptCText = optC.transform.Find("Body")?.GetComponent<TextMeshProUGUI>();
        refs.OptDText = optD.transform.Find("Body")?.GetComponent<TextMeshProUGUI>();
        refs.CheckCircuitBtn = checkBtn; refs.TestCircuitBtn = testBtn;
        refs.MeasureVBtn = measureVBtn; refs.MeasureIBtn = measureIBtn; refs.RecordBtn = recordBtn;
        refs.UndoWireBtn = undoBtn; refs.RotateBtn = rotateBtn;
        refs.BrightHigh = brightHigh; refs.BrightMed = brightMed; refs.BrightOff = brightOff;
        refs.CircuitBoard = board.GetComponent<RectTransform>();
        refs.EquipmentTray = trayInner.GetComponent<RectTransform>();
        refs.HintText = hint; refs.MeterInfoText = meterInfo;
        refs.VoltageInput = vInput; refs.CurrentInput = iInput;

        instructionBar.transform.SetSiblingIndex(headerP.transform.GetSiblingIndex() + 1);
        canvasObj.transform.Find("BottomBar")?.SetAsLastSibling();
        feedbackPanel.transform.SetAsLastSibling();
        resetConfirm.transform.SetAsLastSibling();
    }

    private bool WireReferences(bool showWelcome)
    {
        var refs = Object.FindAnyObjectByType<ElecUIRefsHolder>();
        if (refs == null || ElecUIManager.Instance == null) return false;

        ElecFeedbackManager.Instance?.BindUI(refs.FeedbackPanel, refs.FeedbackText, refs.ScoreChangeText, refs.FeedbackGroup);
        ElecUIManager.Instance.BindAll(refs, showWelcome);
        ElecEquipmentSelectionManager.Instance?.SetupUI(refs.CardContainer, refs.RequiredArea, refs.CardPrefab);
        ElecObservationTableManager.Instance?.Bind(refs.DataTableText);
        ElecGraphController.Instance?.Bind(refs.VoltageGraphArea, refs.CurrentGraphArea, refs.DotPrefab);
        ElecResultManager.Instance?.Bind(refs.FinalScore, refs.ResultDetails, refs.StatusText);
        CircuitBuilder.Instance?.Bind(refs.CircuitBoard, refs.EquipmentTray, refs.HintText, refs.MeterInfoText, defaultFont);
        CircuitBuilder.Instance?.BindRecordFields(refs.VoltageInput, refs.CurrentInput);
        ElecFailsafeDisplay.Hide();
        return refs.StartBtn != null;
    }

    private GameObject MakeTextPanel(Transform parent, string name, string titleName, string title, string bodyName)
    {
        var panel = Panel(parent, name, Vector2.zero, Vector2.one, new Vector2(24, 16), new Vector2(-24, -16), new Color(0.97f, 0.98f, 1f));
        panel.SetActive(false);
        Text(titleName, panel.transform, title, 34, TextAlignmentOptions.MidlineLeft, new Vector2(20, -12), new Vector2(1100, 48));
        StretchText(bodyName, panel.transform, "", 26, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 22);
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
        input.contentType = TMP_InputField.ContentType.DecimalNumber;
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
        img.sprite = ElecIconFactory.White();
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
        tmp.enableWordWrapping = name == "Instruction" || name.Contains("Hint") || name.Contains("Objective") || name.Contains("Intro");
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
        le.preferredHeight = 70;
        le.minHeight = 60;
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
        le.preferredHeight = h;
        le.minHeight = h;
        var img = obj.AddComponent<Image>();
        img.sprite = ElecIconFactory.White();
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
        contentRt.sizeDelta = new Vector2(0f, 1600f);
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
        content.AddComponent<ElecAdaptiveGrid>();
        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.content = contentRt;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        return scroll;
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
        contentRt.sizeDelta = new Vector2(0f, 1600f);
        var grid = content.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(300, 210);
        grid.spacing = new Vector2(16, 16);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        grid.padding = new RectOffset(16, 16, 16, 16);
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
        card.AddComponent<ElecEquipmentCardUI>();
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

public class ElecUIRefsHolder : MonoBehaviour
{
    public int UiVersion;
    public TextMeshProUGUI Title, Score, Progress, Attempts, ConnectionLabel, Instruction;
    public TextMeshProUGUI IntroText, ObjectiveText, TutorialText, EducationText, ConclusionText, CompareText, QuestionText, DataTableText;
    public TextMeshProUGUI FinalScore, ResultDetails, StatusText, FeedbackText, ScoreChangeText, HintText, MeterInfoText;
    public TextMeshProUGUI QuestionExplanationText, OptAText, OptBText, OptCText, OptDText;
    public Image ProgressBar;
    public GameObject IntroPanel, ObjectivePanel, InstructionBar, EquipmentPanel, TutorialPanel, LaboratoryPanel;
    public GameObject DataTablePanel, ComparePanel, GraphPanel, EducationPanel, QuestionPanel, ConclusionPanel, ResultPanel;
    public GameObject ResetConfirm, FeedbackPanel, CardPrefab, DotPrefab, QuestionExplanationPanel;
    public Transform CardContainer, RequiredArea;
    public RectTransform VoltageGraphArea, CurrentGraphArea, CircuitBoard, EquipmentTray;
    public Button StartBtn, Next, Reset, Retry, ResetYes, ResetNo, ViewProfileBtn, ViewResultsBtn, EquipContinueBtn;
    public Button CheckCircuitBtn, TestCircuitBtn, MeasureVBtn, MeasureIBtn, RecordBtn, UndoWireBtn, RotateBtn;
    public Button BrightHigh, BrightMed, BrightOff;
    public Button QuestionA, QuestionB, QuestionC, QuestionD, QuestionContinue;
    public TMP_InputField VoltageInput, CurrentInput;
    public CanvasGroup FeedbackGroup;
}

public class ElecAdaptiveGrid : MonoBehaviour
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

public class ElecAdaptiveLab : MonoBehaviour
{
    private RectTransform board;
    private RectTransform side;
    private RectTransform tray;
    private RectTransform action;
    private GridLayoutGroup actionGrid;
    private int lastW;
    private int lastH;

    public void Bind(RectTransform boardRt, RectTransform sideRt, RectTransform trayRt, RectTransform actionRt)
    {
        board = boardRt;
        side = sideRt;
        tray = trayRt;
        action = actionRt;
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
            Set(board, 0.01f, 0.40f, 0.99f, 0.99f);
            Set(side, 0.52f, 0.01f, 0.99f, 0.39f);
            Set(tray, 0.01f, 0.01f, 0.51f, 0.18f);
            Set(action, 0.01f, 0.18f, 0.51f, 0.39f);
        }
        else
        {
            Set(board, 0.01f, 0.32f, 0.68f, 0.99f);
            Set(side, 0.69f, 0.01f, 0.99f, 0.99f);
            Set(tray, 0.01f, 0.01f, 0.68f, 0.18f);
            Set(action, 0.01f, 0.18f, 0.68f, 0.32f);
        }

        if (actionGrid == null || action == null) return;
        float aw = action.rect.width;
        int cols = aw < 360 ? 2 : aw < 560 ? 3 : 4;
        float pad = actionGrid.padding.left + actionGrid.padding.right;
        float space = actionGrid.spacing.x * (cols - 1);
        float cellW = Mathf.Max(90f, (aw - pad - space) / cols);
        float cellH = Mathf.Clamp((action.rect.height - actionGrid.padding.top - actionGrid.padding.bottom - actionGrid.spacing.y) / 2f, 28f, 40f);
        actionGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        actionGrid.constraintCount = cols;
        actionGrid.cellSize = new Vector2(cellW, cellH);
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
