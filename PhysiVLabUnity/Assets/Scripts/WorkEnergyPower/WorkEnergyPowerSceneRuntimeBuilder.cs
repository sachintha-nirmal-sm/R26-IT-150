using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

[DefaultExecutionOrder(-50)]
public class WorkEnergyPowerSceneRuntimeBuilder : MonoBehaviour
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
            Debug.LogWarning("WorkEnergyPower: wiring failed.");
            return;
        }
        referencesWired = true;
    }

    private void Awake()
    {
        if (GetComponent<WorkEnergyFailsafeDisplay>() == null)
            gameObject.AddComponent<WorkEnergyFailsafeDisplay>();
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
            Debug.Log("Work Energy Power practical UI built.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Work Energy Power BUILD FAILED: " + ex.Message + "\n" + ex.StackTrace);
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
            Debug.LogWarning("WorkEnergyPower: font load skipped — " + ex.Message);
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
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("WorkEnergyPower EventSystem: " + ex.Message);
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
        AddMgr<WorkEnergyPowerExperimentManager>("WorkEnergyPowerExperimentManager");
        AddMgr<WorkEnergyUIManager>("WorkEnergyUIManager");
        AddMgr<WorkEnergyScoreManager>("WorkEnergyScoreManager");
        AddMgr<WorkEnergyFeedbackManager>("WorkEnergyFeedbackManager");
        AddMgr<WorkEnergyAttemptManager>("WorkEnergyAttemptManager");
        AddMgr<WorkEnergySaveManager>("WorkEnergySaveManager");
        AddMgr<WorkEnergyProfileManager>("WorkEnergyProfileManager");
        AddMgr<WorkEnergyExperimentDataManager>("WorkEnergyExperimentDataManager");
        AddMgr<WorkEnergyEquipmentSelectionManager>("WorkEnergyEquipmentSelectionManager");
        AddMgr<WorkEnergyPotentialEnergyCalculator>("WorkEnergyPotentialEnergyCalculator");
        AddMgr<WorkEnergyClayController>("WorkEnergyClayController");
        AddMgr<WorkEnergyClaySurfaceController>("WorkEnergyClaySurfaceController");
        AddMgr<WorkEnergyHeavyWeightController>("WorkEnergyHeavyWeightController");
        AddMgr<WorkEnergyReleaseStandController>("WorkEnergyReleaseStandController");
        AddMgr<WorkEnergyReleaseMechanismController>("WorkEnergyReleaseMechanismController");
        AddMgr<WorkEnergyFallingWeightController>("WorkEnergyFallingWeightController");
        AddMgr<WorkEnergyImpactController>("WorkEnergyImpactController");
        AddMgr<WorkEnergyDepressionController>("WorkEnergyDepressionController");
        AddMgr<WorkEnergyGraphController>("WorkEnergyGraphController");
        AddMgr<WorkEnergyConclusionManager>("WorkEnergyConclusionManager");
        AddMgr<WorkEnergyResultManager>("WorkEnergyResultManager");
        AddMgr<WorkEnergyPowerChallengeManager>("WorkEnergyPowerChallengeManager");
        AddMgr<WorkEnergyLabWorkbench>("WorkEnergyLabWorkbench");
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
        Color header = new Color(0.12f, 0.36f, 0.52f);
        Color accent = new Color(0.12f, 0.52f, 0.78f);
        Color green = new Color(0.12f, 0.62f, 0.35f);

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
        var title = Text("Title", headerP.transform, "WORK, ENERGY AND POWER    Activity: Potential Energy and Height", 28, TextAlignmentOptions.MidlineLeft, new Vector2(16, -6), new Vector2(1200, 44));
        title.color = Color.white;
        title.enableAutoSizing = true;
        title.fontSizeMin = 16;
        title.fontSizeMax = 28;
        title.overflowMode = TextOverflowModes.Ellipsis;
        var score = Text("Score", headerP.transform, "Score: 0 / 100", 24, TextAlignmentOptions.MidlineRight, new Vector2(-16, -6), new Vector2(340, 44), Vector2.one, Vector2.one);
        score.color = Color.white;
        var progress = Text("Progress", headerP.transform, "Step: 1 / 15", 20, TextAlignmentOptions.MidlineLeft, new Vector2(16, -46), new Vector2(280, 32));
        progress.color = new Color(0.85f, 0.93f, 1f);
        var attempts = Text("Attempts", headerP.transform, "Attempts Remaining: 3", 20, TextAlignmentOptions.MidlineRight, new Vector2(-16, -46), new Vector2(380, 32), Vector2.one, Vector2.one);
        attempts.color = new Color(0.85f, 0.93f, 1f);
        var progressBarBg = Panel(headerP.transform, "ProgressBarBg", new Vector2(0.30f, 0.08f), new Vector2(0.70f, 0.22f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.16f, 0.24f, 0.5f));
        var progressBarFill = Panel(progressBarBg.transform, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.32f, 0.85f, 0.48f));
        var fillImg = progressBarFill.GetComponent<Image>();
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 0.06f;

        var instructionBar = Panel(canvasObj.transform, "InstructionBar", new Vector2(0, 0.83f), new Vector2(1, 0.91f), Vector2.zero, Vector2.zero, new Color(1f, 0.96f, 0.78f));
        instructionBar.SetActive(false);
        var instruction = StretchText("Instruction", instructionBar.transform, "Follow the instructions.", 24, TextAlignmentOptions.MidlineLeft, new Color(0.12f, 0.16f, 0.22f), 14);
        instruction.enableAutoSizing = true;
        instruction.fontSizeMin = 16;
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

        var objectivePanel = Panel(main.transform, "ObjectivePanel", Vector2.zero, Vector2.one, new Vector2(36, 24), new Vector2(-36, -24), Color.white);
        var objectiveText = StretchText("ObjectiveText", objectivePanel.transform, "", 28, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 24);
        var startBtn = BigBtn("StartBtn", objectivePanel.transform, "START PRACTICAL", new Vector2(0.5f, 0.08f), new Vector2(0, 0), new Vector2(480, 96), green);

        var equipPanel = Panel(main.transform, "EquipmentPanel", Vector2.zero, Vector2.one, new Vector2(10, 8), new Vector2(-10, -8), Color.white);
        equipPanel.SetActive(false);
        var equipTitle = Text("EquipTitle", equipPanel.transform, "STEP 1 — Tap the equipment needed for this practical.", 26, TextAlignmentOptions.MidlineLeft, Vector2.zero, Vector2.zero, new Vector2(0.02f, 0.91f), new Vector2(0.98f, 1f), new Vector2(0.5f, 0.5f));
        StretchFill(equipTitle.rectTransform);
        equipTitle.enableAutoSizing = true;
        equipTitle.fontSizeMin = 16;
        equipTitle.fontSizeMax = 26;
        var equipHint = Text("EquipHint", equipPanel.transform, "Tap the correct items, then press NEXT STEP at the bottom.", 20, TextAlignmentOptions.MidlineLeft, Vector2.zero, Vector2.zero, new Vector2(0.02f, 0.85f), new Vector2(0.98f, 0.91f), new Vector2(0.5f, 0.5f));
        StretchFill(equipHint.rectTransform);
        equipHint.color = new Color(0.22f, 0.32f, 0.46f);
        equipHint.enableAutoSizing = true;
        equipHint.fontSizeMin = 14;
        equipHint.fontSizeMax = 20;

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
        requiredCards.AddComponent<WorkEnergyUIDropTarget>().Configure("RequiredEquipment", "Any", Vector2.zero);

        var scroll = CreateScroll(equipPanel.transform, Vector2.zero, Vector2.zero);
        var scrollRt = scroll.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0f, 0.25f);
        scrollRt.anchorMax = new Vector2(1f, 0.85f);
        scrollRt.offsetMin = new Vector2(8, 4);
        scrollRt.offsetMax = new Vector2(-8, -4);
        var cardPrefab = CreateCardPrefab();
        var equipContinue = BigBtn("EquipContinueBtn", equipPanel.transform, "NEXT STEP", new Vector2(1f, 0f), new Vector2(-160, 18), new Vector2(240, 48), green);
        equipContinue.gameObject.SetActive(false);

        var experimentPanel = Panel(main.transform, "ExperimentPanel", Vector2.zero, Vector2.one, new Vector2(12, 6), new Vector2(-12, -6), new Color(0, 0, 0, 0), false);
        experimentPanel.SetActive(false);

        var infoCard = Panel(experimentPanel.transform, "InfoCard", new Vector2(0.68f, 0.52f), new Vector2(0.99f, 0.99f), Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.94f));
        var infoText = StretchText("InfoText", infoCard.transform, "Experiment Information", 22, TextAlignmentOptions.TopLeft, new Color(0.16f, 0.2f, 0.26f), 14);

        var dataTablePanel = Panel(experimentPanel.transform, "DataTablePanel", new Vector2(0.68f, 0.06f), new Vector2(0.99f, 0.50f), Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.94f));
        var dataTableText = StretchText("DataTableText", dataTablePanel.transform, "DATA TABLE", 20, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 12);

        var workbench = Panel(experimentPanel.transform, "Workbench", new Vector2(0.01f, 0.06f), new Vector2(0.67f, 0.99f), Vector2.zero, Vector2.zero, new Color(0.88f, 0.93f, 0.98f));

        var labBg = Panel(workbench.transform, "ExperimentVisual", new Vector2(0.02f, 0.34f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero, new Color(0.94f, 0.97f, 1f));
        var scaleRoot = labBg.GetComponent<RectTransform>();

        var standPole = Panel(labBg.transform, "StandPole", new Vector2(0.22f, 0.12f), new Vector2(0.25f, 0.96f), Vector2.zero, Vector2.zero, new Color(0.45f, 0.48f, 0.52f), false);
        var standBase = Panel(labBg.transform, "StandBase", new Vector2(0.12f, 0.08f), new Vector2(0.36f, 0.14f), Vector2.zero, Vector2.zero, new Color(0.38f, 0.4f, 0.44f), false);

        var heightScale = Panel(labBg.transform, "HeightScale", new Vector2(0.78f, 0.12f), new Vector2(0.96f, 0.96f), Vector2.zero, Vector2.zero, new Color(0.98f, 0.94f, 0.72f, 0.95f), false);
        BuildHeightTicks(heightScale.transform);

        var clayTray = Panel(labBg.transform, "ClayTray", new Vector2(0.30f, 0.06f), new Vector2(0.74f, 0.20f), Vector2.zero, Vector2.zero, new Color(0.50f, 0.36f, 0.22f), false);
        var claySurf = Panel(clayTray.transform, "ClaySurface", new Vector2(0.04f, 0.28f), new Vector2(0.96f, 0.95f), Vector2.zero, Vector2.zero, new Color(0.72f, 0.48f, 0.28f), false);
        claySurf.GetComponent<Image>().sprite = WorkEnergyIconFactory.ClaySprite();
        claySurf.GetComponent<Image>().preserveAspect = false;
        var clayCol = claySurf.AddComponent<BoxCollider2D>();
        clayCol.isTrigger = true;
        var depression = Panel(claySurf.transform, "Depression", new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), new Vector2(-36, -28), new Vector2(36, 0), new Color(0.35f, 0.2f, 0.1f), false);
        depression.SetActive(false);
        var clayLabel = Text("ClayLabel", clayTray.transform, "CLAY  thickness = 3 cm", 18, TextAlignmentOptions.Center, Vector2.zero, new Vector2(260, 24), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        clayLabel.color = Color.white;

        var impactPoint = Panel(labBg.transform, "ImpactPoint", new Vector2(0.50f, 0.20f), new Vector2(0.50f, 0.20f), new Vector2(-12, -8), new Vector2(12, 8), new Color(0.85f, 0.2f, 0.2f), false);
        Text("ImpactLabel", labBg.transform, "IMPACT POINT", 16, TextAlignmentOptions.Center, new Vector2(0, 10), new Vector2(180, 24), new Vector2(0.5f, 0.20f), new Vector2(0.5f, 0.20f), new Vector2(0.5f, 0));

        var holder = Panel(labBg.transform, "ReleaseMechanism", new Vector2(0.50f, 0.62f), new Vector2(0.50f, 0.62f), new Vector2(-28, -10), new Vector2(28, 10), new Color(0.25f, 0.45f, 0.7f), false);
        var weightVisual = Panel(labBg.transform, "HeavyWeight", new Vector2(0.50f, 0.58f), new Vector2(0.50f, 0.58f), new Vector2(-26, -26), new Vector2(26, 26), Color.white, false);
        weightVisual.GetComponent<Image>().sprite = WorkEnergyIconFactory.BallSprite();
        weightVisual.GetComponent<Image>().preserveAspect = true;
        var weightCol = weightVisual.AddComponent<CircleCollider2D>();
        weightCol.isTrigger = true;
        weightCol.radius = 0.4f;
        Text("WeightTag", weightVisual.transform, "WEIGHT", 16, TextAlignmentOptions.Center, new Vector2(0, -20), new Vector2(100, 20), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));

        var heightMarker = Panel(labBg.transform, "HeightMarker", new Vector2(0.74f, 0.62f), new Vector2(0.74f, 0.62f), new Vector2(-12, -10), new Vector2(20, 10), new Color(0.15f, 0.45f, 0.85f));
        heightMarker.AddComponent<WorkEnergyHeightMeasurementManager>();
        Text("HArrow", heightMarker.transform, "h", 18, TextAlignmentOptions.Center, Vector2.zero, new Vector2(24, 20), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).color = Color.white;

        var depthMarker = Panel(claySurf.transform, "DepthMarker", new Vector2(0.88f, 0.9f), new Vector2(0.88f, 0.9f), new Vector2(-8, -8), new Vector2(8, 8), new Color(0.15f, 0.25f, 0.7f));
        depthMarker.AddComponent<WorkEnergyDepthMeasurementManager>();

        var heightLabel = Text("HeightLabel", labBg.transform, "HEIGHT  h = 0.50 m", 22, TextAlignmentOptions.MidlineLeft, new Vector2(12, -10), new Vector2(280, 36), new Vector2(0, 1), new Vector2(0, 1));
        heightLabel.overflowMode = TextOverflowModes.Truncate;
        var massLabel = Text("MassLabel", labBg.transform, "WEIGHT  1.00 kg", 22, TextAlignmentOptions.MidlineLeft, new Vector2(300, -10), new Vector2(240, 36), new Vector2(0, 1), new Vector2(0, 1));
        massLabel.overflowMode = TextOverflowModes.Truncate;
        var energyLabel = StretchText("EnergyLabel", infoCard.transform, "PE = mgh", 20, TextAlignmentOptions.BottomLeft, new Color(0.12f, 0.4f, 0.22f), 12);
        var energyRt = energyLabel.rectTransform;
        energyRt.anchorMin = new Vector2(0, 0);
        energyRt.anchorMax = new Vector2(1, 0.38f);

        var setupTray = Panel(workbench.transform, "SetupTray", new Vector2(0.02f, 0.36f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var clayZone = CreateDropZone(setupTray.transform, "ClayZone", "Clay", "Clay", new Vector2(0.36f, 0.35f), new Vector2(0.68f, 0.95f), "CLAY TRAY");
        var standZone = CreateDropZone(setupTray.transform, "StandZone", "Stand", "Stand", new Vector2(0.04f, 0.35f), new Vector2(0.28f, 0.95f), "STAND");
        var weightZone = CreateDropZone(setupTray.transform, "WeightZone", "Weight", "Weight", new Vector2(0.42f, 0.55f), new Vector2(0.62f, 0.95f), "RELEASE");
        var balanceZone = CreateDropZone(setupTray.transform, "BalanceZone", "Balance", "Balance", new Vector2(0.74f, 0.35f), new Vector2(0.96f, 0.95f), "BALANCE");

        var actionRow = Panel(workbench.transform, "ActionRow", new Vector2(0.02f, 0.27f), new Vector2(0.98f, 0.35f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var actionLayout = actionRow.AddComponent<HorizontalLayoutGroup>();
        actionLayout.spacing = 6;
        actionLayout.childAlignment = TextAnchor.MiddleCenter;
        actionLayout.childControlWidth = true;
        actionLayout.childForceExpandWidth = true;
        var confirmHeightBtn = Btn("ConfirmHeight", actionRow.transform, "Confirm Height", accent, 0, 44);
        var confirmMeasureHeightBtn = Btn("ConfirmMeasureH", actionRow.transform, "Confirm h", accent, 0, 44);
        var releaseBtn = Btn("ReleaseBtn", actionRow.transform, "RELEASE WEIGHT", new Color(0.75f, 0.22f, 0.18f), 0, 44);
        var confirmDepthBtn = Btn("ConfirmDepth", actionRow.transform, "Confirm Depth", accent, 0, 44);
        var recordBtn = Btn("RecordBtn", actionRow.transform, "RECORD READING", green, 0, 44);
        var resetWeightBtn = Btn("ResetWeight", actionRow.transform, "Reset Weight", new Color(0.5f, 0.45f, 0.2f), 0, 44);
        var measureMassBtn = Btn("MeasureMass", actionRow.transform, "Measure Mass", accent, 0, 44);
        var skipMassBtn = Btn("SkipMass", actionRow.transform, "Skip Mass", new Color(0.45f, 0.48f, 0.52f), 0, 44);
        var changeMassBtn = Btn("ChangeMass", actionRow.transform, "Change Mass", new Color(0.55f, 0.35f, 0.35f), 0, 44);
        confirmHeightBtn.gameObject.SetActive(false);
        confirmMeasureHeightBtn.gameObject.SetActive(false);
        releaseBtn.gameObject.SetActive(false);
        confirmDepthBtn.gameObject.SetActive(false);
        recordBtn.gameObject.SetActive(false);
        resetWeightBtn.gameObject.SetActive(false);
        measureMassBtn.gameObject.SetActive(false);
        skipMassBtn.gameObject.SetActive(false);

        var sliderRow = Panel(workbench.transform, "SliderRow", new Vector2(0.02f, 0.20f), new Vector2(0.98f, 0.27f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var heightSlider = CreateSlider(sliderRow.transform, "HeightSlider", 0.10f, 1.00f, 0.35f);
        var depthSlider = CreateSlider(sliderRow.transform, "DepthSlider", 0.1f, 3.0f, 0.6f);
        depthSlider.gameObject.SetActive(false);
        var depthLabel = Text("DepthLabel", sliderRow.transform, "Depression Depth = 0.0 cm", 22, TextAlignmentOptions.MidlineRight, new Vector2(-8, 0), new Vector2(360, 32), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));

        var palette = Panel(workbench.transform, "Palette", new Vector2(0.02f, 0.01f), new Vector2(0.98f, 0.19f), Vector2.zero, Vector2.zero, new Color(0.95f, 0.97f, 1f, 0.96f));
        var paletteLayout = palette.AddComponent<HorizontalLayoutGroup>();
        paletteLayout.spacing = 14;
        paletteLayout.padding = new RectOffset(10, 10, 8, 8);
        paletteLayout.childAlignment = TextAnchor.MiddleCenter;
        var clayItem = CreateDragItem(palette.transform, "ClayItem", "Clay", WorkEnergyEquipmentType.Clay, "Clay", 150, 118);
        var standItem = CreateDragItem(palette.transform, "StandItem", "Stand", WorkEnergyEquipmentType.ReleaseStand, "Stand", 150, 118);
        var weightItem = CreateDragItem(palette.transform, "WeightItem", "Weight", WorkEnergyEquipmentType.HeavyWeight, "Weight", 150, 118);
        var balanceItem = CreateDragItem(palette.transform, "BalanceItem", "Balance", WorkEnergyEquipmentType.Balance, "Balance", 150, 118);

        var hintText = Text("Hint", workbench.transform, "Press PLACE CLAY at the bottom, or drag clay onto the tray.", 22, TextAlignmentOptions.Center, new Vector2(0, 4), new Vector2(900, 28), new Vector2(0.5f, 0.19f), new Vector2(0.5f, 0.19f), new Vector2(0.5f, 0));
        hintText.color = new Color(0.2f, 0.32f, 0.45f);

        var comparePanel = MakeTextPanel(main.transform, "ComparePanel", "CompareTitle", "OBSERVATIONS", "CompareText");
        var graphPanel = Panel(main.transform, "GraphPanel", Vector2.zero, Vector2.one, new Vector2(20, 12), new Vector2(-20, -12), new Color(0.97f, 0.98f, 1f));
        graphPanel.SetActive(false);
        Text("GraphTitle", graphPanel.transform, "GRAPHS  —  generated from your readings", 26, TextAlignmentOptions.MidlineLeft, new Vector2(20, -12), new Vector2(900, 40));
        var peArea = Panel(graphPanel.transform, "PeGraph", new Vector2(0.04f, 0.52f), new Vector2(0.96f, 0.90f), Vector2.zero, Vector2.zero, new Color(0.92f, 0.95f, 1f));
        Text("PeLabel", peArea.transform, "Height h (m) →     Potential Energy PE (J) ↑     PE ∝ h", 16, TextAlignmentOptions.Bottom, new Vector2(0, 6), new Vector2(700, 24), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        var depthArea = Panel(graphPanel.transform, "DepthGraph", new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.48f), Vector2.zero, Vector2.zero, new Color(1f, 0.95f, 0.90f));
        Text("DepthGLabel", depthArea.transform, "Height h (m) →     Depression Depth (cm) ↑", 16, TextAlignmentOptions.Bottom, new Vector2(0, 6), new Vector2(700, 24), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        var dotPrefab = Panel(peArea.transform, "DotPrefab", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-7, -7), new Vector2(7, 7), accent);
        dotPrefab.SetActive(false);

        var workEnergyPanel = MakeTextPanel(main.transform, "WorkEnergyPanel", "WorkTitle", "WORK, ENERGY AND POWER", "WorkEnergyText");
        var powerPanel = Panel(main.transform, "PowerPanel", Vector2.zero, Vector2.one, new Vector2(40, 30), new Vector2(-40, -30), Color.white);
        powerPanel.SetActive(false);
        var powerPrompt = StretchText("PowerPrompt", powerPanel.transform, "", 24, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 20);
        var inputObj = Panel(powerPanel.transform, "AnswerInput", new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), new Vector2(-140, -28), new Vector2(140, 28), new Color(0.93f, 0.96f, 1f));
        var input = inputObj.AddComponent<TMP_InputField>();
        var inputText = StretchText("Text", inputObj.transform, "", 28, TextAlignmentOptions.Center, new Color(0.1f, 0.15f, 0.2f), 4);
        inputText.raycastTarget = true;
        input.textComponent = inputText;
        input.contentType = TMP_InputField.ContentType.DecimalNumber;
        var powerBtnRow = Panel(powerPanel.transform, "PowerBtns", new Vector2(0.2f, 0.08f), new Vector2(0.8f, 0.20f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var pLayout = powerBtnRow.AddComponent<HorizontalLayoutGroup>();
        pLayout.spacing = 16;
        pLayout.childForceExpandWidth = true;
        var powerSubmit = Btn("PowerSubmit", powerBtnRow.transform, "Check Answer", green, 0, 54);
        var powerSkip = Btn("PowerSkip", powerBtnRow.transform, "Skip Challenge", new Color(0.5f, 0.52f, 0.55f), 0, 54);

        var conclusionPanel = Panel(main.transform, "ConclusionPanel", Vector2.zero, Vector2.one, new Vector2(18, 10), new Vector2(-18, -10), new Color(0.96f, 0.98f, 1f));
        conclusionPanel.SetActive(false);
        var concHeader = Panel(conclusionPanel.transform, "Header", new Vector2(0f, 0.82f), new Vector2(1f, 1f), new Vector2(12, 8), new Vector2(-12, -8), header);
        var concQ = StretchText("Question", concHeader.transform, "Conclusion question", 26, TextAlignmentOptions.MidlineLeft, Color.white, 14);
        var concOptions = Panel(conclusionPanel.transform, "Options", new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.80f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var vLayout = concOptions.AddComponent<VerticalLayoutGroup>();
        vLayout.spacing = 12;
        vLayout.padding = new RectOffset(8, 8, 8, 8);
        vLayout.childControlHeight = true;
        vLayout.childForceExpandHeight = true;
        vLayout.childControlWidth = true;
        vLayout.childForceExpandWidth = true;
        var optA = ChoiceBtn("OptA", concOptions.transform, "A", "");
        var optB = ChoiceBtn("OptB", concOptions.transform, "B", "");
        var optC = ChoiceBtn("OptC", concOptions.transform, "C", "");
        var optD = ChoiceBtn("OptD", concOptions.transform, "D", "");
        var explanationPanel = Panel(conclusionPanel.transform, "ExplanationPanel", new Vector2(0.08f, 0.10f), new Vector2(0.92f, 0.55f), Vector2.zero, Vector2.zero, new Color(0.12f, 0.46f, 0.32f));
        explanationPanel.SetActive(false);
        var explanationText = StretchText("ExplanationText", explanationPanel.transform, "", 22, TextAlignmentOptions.Top, Color.white, 16);
        var continueObj = Panel(explanationPanel.transform, "ContinueBtn", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-120, 12), new Vector2(120, 58), Color.white);
        var continueBtn = continueObj.AddComponent<Button>();
        StretchText("Text", continueObj.transform, "Continue ▶", 22, TextAlignmentOptions.Center, new Color(0.1f, 0.4f, 0.25f), 4).fontStyle = FontStyles.Bold;

        var resultPanel = Panel(main.transform, "ResultPanel", Vector2.zero, Vector2.one, new Vector2(32, 20), new Vector2(-32, -20), Color.white);
        resultPanel.SetActive(false);
        var finalScore = StretchText("FinalScore", resultPanel.transform, "", 24, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 18);
        var resultDetails = StretchText("ResultDetails", resultPanel.transform, "", 20, TextAlignmentOptions.TopLeft, new Color(0.18f, 0.22f, 0.28f), 18);
        var rtDet = resultDetails.rectTransform;
        rtDet.anchorMin = new Vector2(0, 0.18f);
        rtDet.anchorMax = new Vector2(1, 0.62f);
        var statusText = Text("Status", resultPanel.transform, "STATUS: PASSED", 30, TextAlignmentOptions.MidlineLeft, new Vector2(20, 16), new Vector2(520, 48));
        var resultBtnRow = Panel(resultPanel.transform, "ResultBtns", new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.14f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var resultBtnLayout = resultBtnRow.AddComponent<HorizontalLayoutGroup>();
        resultBtnLayout.spacing = 14;
        resultBtnLayout.childForceExpandWidth = true;
        var viewProfileBtn = Btn("ViewProfile", resultBtnRow.transform, "VIEW PROFILE", accent, 0, 52);
        Btn("BackPracticals", resultBtnRow.transform, "BACK TO PRACTICALS", new Color(0.4f, 0.45f, 0.5f), 0, 52);

        var feedbackPanel = Panel(canvasObj.transform, "FeedbackPanel", new Vector2(0.22f, 0.42f), new Vector2(0.78f, 0.58f), Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.97f));
        feedbackPanel.SetActive(false);
        var feedbackGroup = feedbackPanel.AddComponent<CanvasGroup>();
        var feedbackText = StretchText("FeedbackText", feedbackPanel.transform, "", 24, TextAlignmentOptions.Center, new Color(0.14f, 0.32f, 0.6f), 14);
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

        var refs = canvasObj.AddComponent<WorkEnergyUIRefsHolder>();
        refs.UiVersion = 6;
        refs.Title = title; refs.Score = score; refs.Progress = progress; refs.Attempts = attempts;
        refs.ProgressBar = fillImg; refs.Instruction = instruction;
        refs.InstructionBar = instructionBar; refs.ObjectivePanel = objectivePanel; refs.ObjectiveText = objectiveText;
        refs.StartBtn = startBtn; refs.EquipmentPanel = equipPanel; refs.ExperimentPanel = experimentPanel;
        refs.EquipContinueBtn = equipContinue;
        refs.DataTablePanel = dataTablePanel; refs.DataTableText = dataTableText; refs.InfoText = infoText;
        refs.ComparePanel = comparePanel; refs.CompareText = comparePanel.transform.Find("CompareText")?.GetComponent<TextMeshProUGUI>();
        refs.GraphPanel = graphPanel; refs.WorkEnergyPanel = workEnergyPanel;
        refs.WorkEnergyText = workEnergyPanel.transform.Find("WorkEnergyText")?.GetComponent<TextMeshProUGUI>();
        refs.PowerPanel = powerPanel; refs.ConclusionPanel = conclusionPanel; refs.ResultPanel = resultPanel;
        refs.FinalScore = finalScore; refs.ResultDetails = resultDetails; refs.StatusText = statusText;
        refs.Next = nextBtn; refs.Reset = resetBtn; refs.Retry = retryBtn; refs.ViewProfileBtn = viewProfileBtn;
        refs.ResetConfirm = resetConfirm; refs.ResetYes = resetYes; refs.ResetNo = resetNo;
        refs.FeedbackPanel = feedbackPanel; refs.FeedbackText = feedbackText; refs.ScoreChangeText = scoreChangeText;
        refs.FeedbackGroup = feedbackGroup;
        refs.CardContainer = scroll.content; refs.RequiredArea = requiredCards.transform; refs.CardPrefab = cardPrefab;
        refs.PeGraphArea = peArea.GetComponent<RectTransform>();
        refs.DepthGraphArea = depthArea.GetComponent<RectTransform>();
        refs.DotPrefab = dotPrefab;
        refs.ConclusionQuestion = concQ;
        refs.ConclusionA = optA; refs.ConclusionB = optB; refs.ConclusionC = optC; refs.ConclusionD = optD;
        refs.ConclusionExplanationPanel = explanationPanel;
        refs.ConclusionExplanationText = explanationText;
        refs.ConclusionContinueBtn = continueBtn;
        refs.PowerPrompt = powerPrompt; refs.PowerInput = input; refs.PowerSubmit = powerSubmit; refs.PowerSkip = powerSkip;
        refs.ClayZone = clayZone; refs.StandZone = standZone; refs.WeightZone = weightZone; refs.BalanceZone = balanceZone;
        refs.ClayItem = clayItem; refs.StandItem = standItem; refs.WeightItem = weightItem; refs.BalanceItem = balanceItem;
        refs.SetupTray = setupTray; refs.ExperimentVisual = labBg;
        refs.Holder = holder.GetComponent<RectTransform>();
        refs.WeightVisual = weightVisual.GetComponent<RectTransform>();
        refs.ScaleRoot = scaleRoot;
        refs.ImpactPoint = impactPoint.GetComponent<RectTransform>();
        refs.ClayImage = claySurf.GetComponent<Image>();
        refs.DepressionImage = depression.GetComponent<Image>();
        refs.HeightMarker = heightMarker.GetComponent<RectTransform>();
        refs.DepthMarker = depthMarker.GetComponent<RectTransform>();
        refs.HintText = hintText; refs.HeightLabel = heightLabel; refs.EnergyLabel = energyLabel;
        refs.DepthLabel = depthLabel; refs.MassLabel = massLabel;
        refs.HeightSlider = heightSlider; refs.DepthSlider = depthSlider;
        refs.ConfirmHeightBtn = confirmHeightBtn; refs.ConfirmMeasureHeightBtn = confirmMeasureHeightBtn;
        refs.ReleaseBtn = releaseBtn; refs.ConfirmDepthBtn = confirmDepthBtn;
        refs.RecordBtn = recordBtn; refs.ResetWeightBtn = resetWeightBtn;
        refs.MeasureMassBtn = measureMassBtn; refs.SkipMassBtn = skipMassBtn; refs.ChangeMassBtn = changeMassBtn;
        instructionBar.transform.SetAsLastSibling();
        canvasObj.transform.Find("BottomBar")?.SetAsLastSibling();
    }

    private bool WireReferences(bool showWelcome)
    {
        var refs = Object.FindAnyObjectByType<WorkEnergyUIRefsHolder>();
        if (refs == null || WorkEnergyUIManager.Instance == null) return false;

        WorkEnergyFeedbackManager.Instance?.BindUI(refs.FeedbackPanel, refs.FeedbackText, refs.ScoreChangeText, refs.FeedbackGroup);
        WorkEnergyUIManager.Instance.BindAll(refs, showWelcome);
        if (refs.StartBtn != null)
        {
            refs.StartBtn.onClick.RemoveAllListeners();
            refs.StartBtn.onClick.AddListener(() => WorkEnergyUIManager.Instance?.StartPractical());
        }
        if (refs.EquipContinueBtn != null)
        {
            refs.EquipContinueBtn.onClick.RemoveAllListeners();
            refs.EquipContinueBtn.onClick.AddListener(() => WorkEnergyUIManager.Instance?.GoNextFromEquipment());
        }
        WorkEnergyEquipmentSelectionManager.Instance?.SetupUI(refs.CardContainer, refs.RequiredArea, refs.CardPrefab);
        WorkEnergyConclusionManager.Instance?.Bind(refs.ConclusionQuestion, refs.ConclusionA, refs.ConclusionB, refs.ConclusionC, refs.ConclusionD,
            refs.ConclusionExplanationPanel, refs.ConclusionExplanationText, refs.ConclusionContinueBtn);
        WorkEnergyGraphController.Instance?.Bind(refs.PeGraphArea, refs.DepthGraphArea, refs.DotPrefab);
        WorkEnergyResultManager.Instance?.Bind(refs.FinalScore, refs.ResultDetails, refs.StatusText);
        WorkEnergyPowerChallengeManager.Instance?.Bind(refs.PowerPanel, refs.PowerInput, refs.PowerSubmit, refs.PowerSkip, refs.PowerPrompt);
        WorkEnergyLabWorkbench.Instance?.Bind(refs);
        WorkEnergyFailsafeDisplay.Hide();
        return refs.StartBtn != null;
    }

    private GameObject MakeTextPanel(Transform parent, string name, string titleName, string title, string bodyName)
    {
        var panel = Panel(parent, name, Vector2.zero, Vector2.one, new Vector2(24, 16), new Vector2(-24, -16), new Color(0.97f, 0.98f, 1f));
        panel.SetActive(false);
        Text(titleName, panel.transform, title, 32, TextAlignmentOptions.MidlineLeft, new Vector2(20, -12), new Vector2(900, 44));
        StretchText(bodyName, panel.transform, "", 24, TextAlignmentOptions.TopLeft, new Color(0.14f, 0.18f, 0.24f), 20);
        var body = panel.transform.Find(bodyName) as RectTransform;
        if (body != null)
        {
            body.anchorMin = new Vector2(0.04f, 0.08f);
            body.anchorMax = new Vector2(0.96f, 0.86f);
        }
        return panel;
    }

    private void BuildHeightTicks(Transform parent)
    {
        string[] labels = { "1.0 m", "0.8 m", "0.6 m", "0.5 m", "0.4 m", "0.3 m", "0.2 m", "0.0 m" };
        float[] t = { 1f, 0.8f, 0.6f, 0.5f, 0.4f, 0.3f, 0.2f, 0f };
        for (int i = 0; i < labels.Length; i++)
        {
            float y = 0.08f + t[i] * 0.84f;
            var tick = Panel(parent, "Tick" + i, new Vector2(0f, y), new Vector2(0.22f, y), new Vector2(0, -1), new Vector2(0, 1), new Color(0.25f, 0.25f, 0.2f), false);
            Text("L" + i, parent, labels[i], 18, TextAlignmentOptions.MidlineLeft, new Vector2(8, 0), new Vector2(86, 22), new Vector2(0.24f, y), new Vector2(0.24f, y), new Vector2(0f, 0.5f));
        }
    }

    private Slider CreateSlider(Transform parent, string name, float min, float max, float value)
    {
        var obj = Panel(parent, name, new Vector2(0.02f, 0.15f), new Vector2(0.62f, 0.85f), Vector2.zero, Vector2.zero, new Color(0.75f, 0.82f, 0.90f));
        var slider = obj.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = false;
        slider.direction = Slider.Direction.LeftToRight;

        var fillArea = Panel(obj.transform, "Fill Area", new Vector2(0.04f, 0.25f), new Vector2(0.96f, 0.75f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), false);
        var fill = Panel(fillArea.transform, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.18f, 0.55f, 0.85f), false);
        var handleArea = Panel(obj.transform, "Handle Slide Area", Vector2.zero, Vector2.one, new Vector2(12, 0), new Vector2(-12, 0), new Color(0, 0, 0, 0), false);
        var handle = Panel(handleArea.transform, "Handle", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-14, -18), new Vector2(14, 18), Color.white);

        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.value = value;
        return slider;
    }

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
        tmp.overflowMode = TextOverflowModes.Ellipsis;
#pragma warning disable CS0618
        tmp.enableWordWrapping = name == "Instruction" || name.Contains("Hint") || name.Contains("Objective") || name == "EquipTitle" || name == "Title";
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

    private static void StretchFill(RectTransform rt)
    {
        if (rt == null) return;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
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
        body.text = text; body.fontSize = 24; body.fontStyle = FontStyles.Bold;
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
        obj.AddComponent<Image>().color = bg;
        var btn = obj.AddComponent<Button>();
        var txt = StretchText("Text", obj.transform, label, 22, TextAlignmentOptions.Center, Color.white, 4);
        txt.fontStyle = FontStyles.Bold;
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

    private ScrollRect CreateScroll(Transform parent, Vector2 offMin, Vector2 offMax)
    {
        var scrollObj = Panel(parent, "Scroll", Vector2.zero, Vector2.one, offMin, offMax, new Color(0.96f, 0.98f, 1f));
        var scroll = scrollObj.AddComponent<ScrollRect>();
        var viewport = Panel(scrollObj.transform, "Viewport", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.01f));
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        var content = Panel(viewport.transform, "Content", new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero, new Color(1, 1, 1, 0.01f));
        var contentRt = content.GetComponent<RectTransform>();
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.sizeDelta = new Vector2(0f, 1100f);
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
        content.AddComponent<WorkEnergyAdaptiveGrid>();
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
        card.AddComponent<WorkEnergyEquipmentCardUI>();
        card.AddComponent<Button>();
        card.SetActive(false);
        return card;
    }

    private WorkEnergyUIDropTarget CreateDropZone(Transform parent, string name, string zoneId, string acceptedId, Vector2 aMin, Vector2 aMax, string label)
    {
        var zone = Panel(parent, name, aMin, aMax, Vector2.zero, Vector2.zero, new Color(0.8f, 0.88f, 0.95f, 0.55f));
        StretchText("Label", zone.transform, label, 20, TextAlignmentOptions.Center, new Color(0.18f, 0.28f, 0.38f), 6);
        var target = zone.AddComponent<WorkEnergyUIDropTarget>();
        target.Configure(zoneId, acceptedId, Vector2.zero);
        return target;
    }

    private WorkEnergyDragDrop2D CreateDragItem(Transform parent, string name, string id, WorkEnergyEquipmentType type, string label, float w, float h)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var le = obj.AddComponent<LayoutElement>();
        le.preferredWidth = w; le.preferredHeight = h;
        le.minWidth = w; le.minHeight = h;
        var bg = obj.AddComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0.95f);
        bg.raycastTarget = true;
        var iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(obj.transform, false);
        var iconRt = iconObj.AddComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.08f, 0.28f);
        iconRt.anchorMax = new Vector2(0.92f, 0.95f);
        iconRt.offsetMin = iconRt.offsetMax = Vector2.zero;
        var icon = iconObj.AddComponent<Image>();
        icon.sprite = WorkEnergyIconFactory.GetSprite(type);
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
        labelTmp.fontSize = 20;
        labelTmp.alignment = TextAlignmentOptions.Center;
        labelTmp.color = new Color(0.15f, 0.2f, 0.25f);
        labelTmp.raycastTarget = false;
        var drag = obj.AddComponent<WorkEnergyDragDrop2D>();
        drag.Configure(id);
        obj.AddComponent<CanvasGroup>();
        return drag;
    }
}

public class WorkEnergyUIRefsHolder : MonoBehaviour
{
    public int UiVersion;
    public TextMeshProUGUI Title, Score, Progress, Attempts, Instruction, ObjectiveText, InfoText, DataTableText;
    public TextMeshProUGUI CompareText, WorkEnergyText, FinalScore, ResultDetails, StatusText, FeedbackText, ScoreChangeText;
    public TextMeshProUGUI HintText, HeightLabel, EnergyLabel, DepthLabel, MassLabel, ConclusionQuestion, PowerPrompt;
    public Image ProgressBar, ClayImage, DepressionImage;
    public GameObject ObjectivePanel, InstructionBar, EquipmentPanel, ExperimentPanel, DataTablePanel;
    public GameObject ComparePanel, GraphPanel, WorkEnergyPanel, PowerPanel, ConclusionPanel, ResultPanel, ResetConfirm, FeedbackPanel;
    public GameObject CardPrefab, SetupTray, ExperimentVisual, DotPrefab, ConclusionExplanationPanel;
    public Transform CardContainer, RequiredArea;
    public RectTransform PeGraphArea, DepthGraphArea, Holder, WeightVisual, ScaleRoot, ImpactPoint, HeightMarker, DepthMarker;
    public Button StartBtn, Next, Reset, Retry, ResetYes, ResetNo, ViewProfileBtn, EquipContinueBtn;
    public Button ConfirmHeightBtn, ConfirmMeasureHeightBtn, ReleaseBtn, ConfirmDepthBtn, RecordBtn, ResetWeightBtn;
    public Button MeasureMassBtn, SkipMassBtn, ChangeMassBtn, ConclusionA, ConclusionB, ConclusionC, ConclusionD, ConclusionContinueBtn;
    public Button PowerSubmit, PowerSkip;
    public TMP_InputField PowerInput;
    public TextMeshProUGUI ConclusionExplanationText;
    public CanvasGroup FeedbackGroup;
    public Slider HeightSlider, DepthSlider;
    public WorkEnergyUIDropTarget ClayZone, StandZone, WeightZone, BalanceZone;
    public WorkEnergyDragDrop2D ClayItem, StandItem, WeightItem, BalanceItem;
}

public class WorkEnergyAdaptiveGrid : MonoBehaviour
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
