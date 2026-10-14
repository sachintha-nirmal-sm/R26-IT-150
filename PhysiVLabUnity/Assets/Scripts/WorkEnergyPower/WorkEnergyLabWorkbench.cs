using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkEnergyLabWorkbench : MonoBehaviour
{
    public static WorkEnergyLabWorkbench Instance { get; private set; }

    [SerializeField] private WorkEnergyUIDropTarget clayZone;
    [SerializeField] private WorkEnergyUIDropTarget standZone;
    [SerializeField] private WorkEnergyUIDropTarget weightZone;
    [SerializeField] private WorkEnergyUIDropTarget balanceZone;
    [SerializeField] private WorkEnergyDragDrop2D clayItem;
    [SerializeField] private WorkEnergyDragDrop2D standItem;
    [SerializeField] private WorkEnergyDragDrop2D weightItem;
    [SerializeField] private WorkEnergyDragDrop2D balanceItem;

    [SerializeField] private GameObject setupTray;
    [SerializeField] private GameObject experimentVisual;
    [SerializeField] private RectTransform holder;
    [SerializeField] private RectTransform weightVisual;
    [SerializeField] private RectTransform scaleRoot;
    [SerializeField] private RectTransform impactPoint;
    [SerializeField] private Image clayImage;
    [SerializeField] private Image depressionImage;
    [SerializeField] private RectTransform heightMarker;
    [SerializeField] private RectTransform depthMarker;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private TextMeshProUGUI heightLabel;
    [SerializeField] private TextMeshProUGUI energyLabel;
    [SerializeField] private TextMeshProUGUI depthLabel;
    [SerializeField] private TextMeshProUGUI massLabel;
    [SerializeField] private Slider heightSlider;
    [SerializeField] private Slider depthSlider;

    [SerializeField] private Button confirmHeightBtn;
    [SerializeField] private Button confirmMeasureHeightBtn;
    [SerializeField] private Button releaseBtn;
    [SerializeField] private Button confirmDepthBtn;
    [SerializeField] private Button recordBtn;
    [SerializeField] private Button resetWeightBtn;
    [SerializeField] private Button measureMassBtn;
    [SerializeField] private Button skipMassBtn;
    [SerializeField] private Button changeMassBtn;

    private bool clayPlaced;
    private bool standPlaced;
    private bool weightPlaced;
    private bool massMeasured;
    private bool impactObserved;
    private float targetHeight = 0.20f;
    private float lastSimulatedDepth;
    private float lastPotentialEnergy;
    private bool horizontalPenaltyGiven;

    private const float ClayNorm = 0.12f;
    private const float OneMetreNorm = 0.92f;

    private void Awake() => Instance = this;

    public void Bind(WorkEnergyUIRefsHolder refs)
    {
        clayZone = refs.ClayZone;
        standZone = refs.StandZone;
        weightZone = refs.WeightZone;
        balanceZone = refs.BalanceZone;
        clayItem = refs.ClayItem;
        standItem = refs.StandItem;
        weightItem = refs.WeightItem;
        balanceItem = refs.BalanceItem;
        setupTray = refs.SetupTray;
        experimentVisual = refs.ExperimentVisual;
        holder = refs.Holder;
        weightVisual = refs.WeightVisual;
        scaleRoot = refs.ScaleRoot;
        impactPoint = refs.ImpactPoint;
        clayImage = refs.ClayImage;
        depressionImage = refs.DepressionImage;
        heightMarker = refs.HeightMarker;
        depthMarker = refs.DepthMarker;
        hintText = refs.HintText;
        heightLabel = refs.HeightLabel;
        energyLabel = refs.EnergyLabel;
        depthLabel = refs.DepthLabel;
        massLabel = refs.MassLabel;
        heightSlider = refs.HeightSlider;
        depthSlider = refs.DepthSlider;
        confirmHeightBtn = refs.ConfirmHeightBtn;
        confirmMeasureHeightBtn = refs.ConfirmMeasureHeightBtn;
        releaseBtn = refs.ReleaseBtn;
        confirmDepthBtn = refs.ConfirmDepthBtn;
        recordBtn = refs.RecordBtn;
        resetWeightBtn = refs.ResetWeightBtn;
        measureMassBtn = refs.MeasureMassBtn;
        skipMassBtn = refs.SkipMassBtn;
        changeMassBtn = refs.ChangeMassBtn;

        StoreHome(clayItem);
        StoreHome(standItem);
        StoreHome(weightItem);
        StoreHome(balanceItem);

        WorkEnergyClaySurfaceController.Instance?.Bind(clayImage, depressionImage, clayImage != null ? clayImage.rectTransform : null);
        WorkEnergyReleaseMechanismController.Instance?.Bind(holder, scaleRoot, ClayNorm, OneMetreNorm);
        WorkEnergyHeightMeasurementManager.Instance?.Bind(heightMarker, scaleRoot, heightLabel, ClayNorm, OneMetreNorm);
        WorkEnergyDepthMeasurementManager.Instance?.Bind(depthMarker, depthLabel);
        WorkEnergyFallingWeightController.Instance?.Bind(weightVisual);

        if (heightSlider != null)
        {
            heightSlider.minValue = 0.10f;
            heightSlider.maxValue = 1.00f;
            heightSlider.onValueChanged.RemoveAllListeners();
            heightSlider.onValueChanged.AddListener(OnHeightSlider);
        }
        if (depthSlider != null)
        {
            depthSlider.minValue = 0.1f;
            depthSlider.maxValue = 3.0f;
            depthSlider.onValueChanged.RemoveAllListeners();
            depthSlider.onValueChanged.AddListener(v => WorkEnergyDepthMeasurementManager.Instance?.SetDepthFromSlider(v));
        }

        WireBtn(confirmHeightBtn, TryConfirmHeight);
        WireBtn(confirmMeasureHeightBtn, TryConfirmMeasureHeight);
        WireBtn(releaseBtn, TryRelease);
        WireBtn(confirmDepthBtn, TryConfirmDepth);
        WireBtn(recordBtn, TryRecord);
        WireBtn(resetWeightBtn, TryResetForNextReading);
        WireBtn(measureMassBtn, TryMeasureMass);
        WireBtn(skipMassBtn, SkipMass);
        WireBtn(changeMassBtn, () => WorkEnergyHeavyWeightController.Instance?.TryChangeMass(2f));

        UpdateMassLabel();
        UpdateEnergyLabel();
    }

    private void StoreHome(WorkEnergyDragDrop2D item)
    {
        if (item == null) return;
        item.StoreHome(item.transform.parent, Vector2.zero);
        item.OnClicked = OnPaletteClick;
    }

    private void OnPaletteClick(WorkEnergyDragDrop2D item)
    {
        var zone = FindZoneForItem(item.ItemId);
        if (zone != null && zone.CanAccept(item))
            zone.AcceptDrop(item);
    }

    private void WireBtn(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    public WorkEnergyUIDropTarget FindZoneForItem(string itemId)
    {
        switch (itemId)
        {
            case "Clay": return clayZone;
            case "Stand": return standZone;
            case "Weight": return weightZone;
            case "Balance": return balanceZone;
            default: return null;
        }
    }

    public void OnItemDropped(string zoneId, WorkEnergyDragDrop2D item)
    {
        var step = WorkEnergyPowerExperimentManager.Instance != null
            ? WorkEnergyPowerExperimentManager.Instance.CurrentStep
            : WorkEnergyExperimentStep.PrepareClay;

        if (zoneId == "Clay" && item.ItemId == "Clay")
        {
            if (step != WorkEnergyExperimentStep.PrepareClay)
            {
                WorkEnergyScoreManager.Instance?.SubtractScore(5);
                item.ResetItem();
                clayZone?.ClearItem();
                return;
            }
            clayPlaced = true;
            WorkEnergyClayController.Instance?.TryPrepareClay();
            WorkEnergyClaySurfaceController.Instance?.ShowPrepared();
            WorkEnergyScoreManager.Instance?.AddScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("✓ Clay surface prepared.");
            WorkEnergyPowerExperimentManager.Instance?.AdvanceStep();
            return;
        }

        if (zoneId == "Stand" && item.ItemId == "Stand")
        {
            if (step != WorkEnergyExperimentStep.PlaceStand)
            {
                WorkEnergyScoreManager.Instance?.SubtractScore(5);
                item.ResetItem();
                standZone?.ClearItem();
                return;
            }
            standPlaced = true;
            WorkEnergyReleaseStandController.Instance?.PlaceStand();
            WorkEnergyScoreManager.Instance?.AddScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("✓ Release stand positioned above the impact point.");
            WorkEnergyPowerExperimentManager.Instance?.AdvanceStep();
            return;
        }

        if (zoneId == "Weight" && item.ItemId == "Weight")
        {
            if (step != WorkEnergyExperimentStep.PlaceWeight)
            {
                WorkEnergyScoreManager.Instance?.SubtractScore(5);
                item.ResetItem();
                weightZone?.ClearItem();
                return;
            }
            weightPlaced = true;
            WorkEnergyHeavyWeightController.Instance?.PlaceWeight();
            WorkEnergyReleaseMechanismController.Instance?.SetReady(true);
            WorkEnergyScoreManager.Instance?.AddScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("Weight ready for release.");
            WorkEnergyPowerExperimentManager.Instance?.AdvanceStep();
            return;
        }

        if (zoneId == "Balance" && item.ItemId == "Balance")
        {
            TryMeasureMass();
            return;
        }

        WorkEnergyScoreManager.Instance?.SubtractScore(5);
        item.ResetItem();
    }

    public void OnHeightMarkerMoved(float height)
    {
        WorkEnergyReleaseMechanismController.Instance?.SetHeight(height);
        if (heightSlider != null) heightSlider.SetValueWithoutNotify(height);
        MoveWeightToHeight(height);
        UpdateEnergyLabel();
        CheckHorizontalMove();
    }

    private void OnHeightSlider(float value)
    {
        var step = WorkEnergyPowerExperimentManager.Instance?.CurrentStep;
        if (step != WorkEnergyExperimentStep.SetHeight && step != WorkEnergyExperimentStep.MeasureHeight) return;
        WorkEnergyHeightMeasurementManager.Instance?.SetDisplayedHeight(value);
        MoveWeightToHeight(value);
        UpdateEnergyLabel();
    }

    private void CheckHorizontalMove()
    {
        if (horizontalPenaltyGiven) return;
        if (weightVisual == null || impactPoint == null) return;
        float dx = Mathf.Abs(weightVisual.anchoredPosition.x - impactPoint.anchoredPosition.x);
        if (dx > 40f)
        {
            horizontalPenaltyGiven = true;
            WorkEnergyScoreManager.Instance?.SubtractScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("Keep the impact point the same for every reading.");
            var pos = weightVisual.anchoredPosition;
            pos.x = impactPoint.anchoredPosition.x;
            weightVisual.anchoredPosition = pos;
        }
    }

    public void DoGuidedAction()
    {
        var step = WorkEnergyPowerExperimentManager.Instance != null
            ? WorkEnergyPowerExperimentManager.Instance.CurrentStep
            : WorkEnergyExperimentStep.PrepareClay;

        switch (step)
        {
            case WorkEnergyExperimentStep.PrepareClay:
                PlaceItemGuided(clayItem, clayZone, "Clay");
                break;
            case WorkEnergyExperimentStep.PlaceStand:
                PlaceItemGuided(standItem, standZone, "Stand");
                break;
            case WorkEnergyExperimentStep.PlaceWeight:
                PlaceItemGuided(weightItem, weightZone, "Weight");
                break;
            case WorkEnergyExperimentStep.MeasureMass:
                TryMeasureMass();
                break;
            case WorkEnergyExperimentStep.SetHeight:
                SnapHeightToTarget();
                TryConfirmHeight();
                break;
            case WorkEnergyExperimentStep.MeasureHeight:
                SnapHeightToTarget();
                TryConfirmMeasureHeight();
                break;
            case WorkEnergyExperimentStep.ReleaseWeight:
                EnsureSetupPlaced();
                TryRelease();
                break;
            case WorkEnergyExperimentStep.ObserveImpact:
                if (!impactObserved)
                    WorkEnergyFallingWeightController.Instance?.HandleImpact();
                else
                    WorkEnergyPowerExperimentManager.Instance?.AdvanceStep();
                break;
            case WorkEnergyExperimentStep.MeasureDepression:
                if (!impactObserved)
                    WorkEnergyFallingWeightController.Instance?.HandleImpact();
                WorkEnergyDepthMeasurementManager.Instance?.SetDepthFromSlider(lastSimulatedDepth);
                if (depthSlider != null) depthSlider.SetValueWithoutNotify(lastSimulatedDepth);
                TryConfirmDepth();
                break;
            case WorkEnergyExperimentStep.RecordResult:
                TryRecord();
                if (WorkEnergyExperimentDataManager.Instance != null && !WorkEnergyExperimentDataManager.Instance.AllHeightsRecorded())
                    TryResetForNextReading();
                break;
            default:
                WorkEnergyPowerExperimentManager.Instance?.AdvanceStep();
                break;
        }
    }

    private void PlaceItemGuided(WorkEnergyDragDrop2D item, WorkEnergyUIDropTarget zone, string zoneId)
    {
        if (item != null && zone != null)
        {
            if (!zone.gameObject.activeSelf) zone.gameObject.SetActive(true);
            zone.AcceptDrop(item);
            return;
        }

        if (zoneId == "Clay")
        {
            if (clayPlaced)
            {
                WorkEnergyPowerExperimentManager.Instance?.AdvanceStep();
                return;
            }
            clayPlaced = true;
            WorkEnergyClayController.Instance?.TryPrepareClay();
            WorkEnergyClaySurfaceController.Instance?.ShowPrepared();
            WorkEnergyScoreManager.Instance?.AddScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("✓ Clay surface prepared.");
            WorkEnergyPowerExperimentManager.Instance?.AdvanceStep();
        }
        else if (zoneId == "Stand")
        {
            if (standPlaced)
            {
                WorkEnergyPowerExperimentManager.Instance?.AdvanceStep();
                return;
            }
            standPlaced = true;
            WorkEnergyReleaseStandController.Instance?.PlaceStand();
            WorkEnergyScoreManager.Instance?.AddScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("✓ Release stand positioned above the impact point.");
            WorkEnergyPowerExperimentManager.Instance?.AdvanceStep();
        }
        else if (zoneId == "Weight")
        {
            if (weightPlaced)
            {
                WorkEnergyPowerExperimentManager.Instance?.AdvanceStep();
                return;
            }
            weightPlaced = true;
            WorkEnergyHeavyWeightController.Instance?.PlaceWeight();
            WorkEnergyReleaseMechanismController.Instance?.SetReady(true);
            WorkEnergyScoreManager.Instance?.AddScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("Weight ready for release.");
            WorkEnergyPowerExperimentManager.Instance?.AdvanceStep();
        }
    }

    private void EnsureSetupPlaced()
    {
        if (!clayPlaced)
        {
            clayPlaced = true;
            WorkEnergyClayController.Instance?.TryPrepareClay();
            WorkEnergyClaySurfaceController.Instance?.ShowPrepared();
        }
        if (!standPlaced)
        {
            standPlaced = true;
            WorkEnergyReleaseStandController.Instance?.PlaceStand();
        }
        if (!weightPlaced)
        {
            weightPlaced = true;
            WorkEnergyHeavyWeightController.Instance?.PlaceWeight();
            WorkEnergyReleaseMechanismController.Instance?.SetReady(true);
        }
    }

    private void SnapHeightToTarget()
    {
        if (heightSlider != null) heightSlider.SetValueWithoutNotify(targetHeight);
        WorkEnergyHeightMeasurementManager.Instance?.SetDisplayedHeight(targetHeight);
        MoveWeightToHeight(targetHeight);
        WorkEnergyReleaseMechanismController.Instance?.SetHeight(targetHeight);
    }

    public void UpdateForStep(WorkEnergyExperimentStep step)
    {
        bool lab = step >= WorkEnergyExperimentStep.PrepareClay && step <= WorkEnergyExperimentStep.RecordResult;
        if (setupTray != null) setupTray.SetActive(lab);
        if (experimentVisual != null) experimentVisual.SetActive(lab || clayPlaced);

        SetZone(clayZone, step == WorkEnergyExperimentStep.PrepareClay && !clayPlaced);
        SetZone(standZone, step == WorkEnergyExperimentStep.PlaceStand && !standPlaced);
        SetZone(weightZone, step == WorkEnergyExperimentStep.PlaceWeight && !weightPlaced);
        SetZone(balanceZone, step == WorkEnergyExperimentStep.MeasureMass);

        SetDrag(clayItem, step == WorkEnergyExperimentStep.PrepareClay && !clayPlaced);
        SetDrag(standItem, step == WorkEnergyExperimentStep.PlaceStand && !standPlaced);
        SetDrag(weightItem, step == WorkEnergyExperimentStep.PlaceWeight && !weightPlaced);
        SetDrag(balanceItem, step == WorkEnergyExperimentStep.MeasureMass);

        if (heightSlider != null) heightSlider.gameObject.SetActive(step == WorkEnergyExperimentStep.SetHeight || step == WorkEnergyExperimentStep.MeasureHeight);
        if (depthSlider != null) depthSlider.gameObject.SetActive(step == WorkEnergyExperimentStep.MeasureDepression);
        if (confirmHeightBtn != null) confirmHeightBtn.gameObject.SetActive(step == WorkEnergyExperimentStep.SetHeight);
        if (confirmMeasureHeightBtn != null) confirmMeasureHeightBtn.gameObject.SetActive(step == WorkEnergyExperimentStep.MeasureHeight);
        if (releaseBtn != null) releaseBtn.gameObject.SetActive(step == WorkEnergyExperimentStep.ReleaseWeight);
        if (confirmDepthBtn != null) confirmDepthBtn.gameObject.SetActive(step == WorkEnergyExperimentStep.MeasureDepression);
        if (recordBtn != null) recordBtn.gameObject.SetActive(step == WorkEnergyExperimentStep.RecordResult);
        if (resetWeightBtn != null) resetWeightBtn.gameObject.SetActive(step == WorkEnergyExperimentStep.RecordResult && WorkEnergyExperimentDataManager.Instance != null && !WorkEnergyExperimentDataManager.Instance.AllHeightsRecorded());
        if (measureMassBtn != null) measureMassBtn.gameObject.SetActive(step == WorkEnergyExperimentStep.MeasureMass);
        if (skipMassBtn != null) skipMassBtn.gameObject.SetActive(step == WorkEnergyExperimentStep.MeasureMass);
        if (changeMassBtn != null) changeMassBtn.gameObject.SetActive(step >= WorkEnergyExperimentStep.PlaceWeight && step <= WorkEnergyExperimentStep.RecordResult);

        if (step == WorkEnergyExperimentStep.SetHeight)
        {
            targetHeight = WorkEnergyExperimentDataManager.Instance != null
                ? WorkEnergyExperimentDataManager.Instance.GetNextUnrecordedHeight()
                : 0.20f;
            if (heightSlider != null) heightSlider.value = 0.35f;
            WorkEnergyHeightMeasurementManager.Instance?.SetDisplayedHeight(heightSlider != null ? heightSlider.value : 0.35f);
            MoveWeightToHeight(WorkEnergyReleaseMechanismController.Instance != null ? WorkEnergyReleaseMechanismController.Instance.CurrentHeight : 0.35f);
        }

        UpdateHint(step);
        UpdateMassLabel();
        UpdateEnergyLabel();
        WorkEnergyUIManager.Instance?.UpdateDataTable();
    }

    private void SetZone(WorkEnergyUIDropTarget zone, bool visible)
    {
        if (zone != null) zone.gameObject.SetActive(visible);
    }

    private void SetDrag(WorkEnergyDragDrop2D item, bool on)
    {
        if (item != null) item.SetDraggable(on);
    }

    private void UpdateHint(WorkEnergyExperimentStep step)
    {
        if (hintText == null) return;
        switch (step)
        {
            case WorkEnergyExperimentStep.PrepareClay: hintText.text = "Press PLACE CLAY at the bottom, or drag clay onto the tray (about 3 cm thick)."; break;
            case WorkEnergyExperimentStep.PlaceStand: hintText.text = "Press PLACE STAND at the bottom, or drag the stand above the impact point."; break;
            case WorkEnergyExperimentStep.PlaceWeight: hintText.text = "Press PLACE WEIGHT at the bottom, or drag the weight into the release mechanism."; break;
            case WorkEnergyExperimentStep.MeasureMass: hintText.text = "Press MEASURE MASS at the bottom, or skip. This step is optional."; break;
            case WorkEnergyExperimentStep.SetHeight: hintText.text = $"Press SET HEIGHT to snap to {targetHeight:0.00} m, or drag the slider yourself."; break;
            case WorkEnergyExperimentStep.MeasureHeight: hintText.text = "Press CONFIRM HEIGHT at the bottom to measure h from the clay surface."; break;
            case WorkEnergyExperimentStep.ReleaseWeight: hintText.text = "Press RELEASE WEIGHT at the bottom. The weight falls from rest onto the clay."; break;
            case WorkEnergyExperimentStep.ObserveImpact: hintText.text = "Watch the impact, then press CONTINUE."; break;
            case WorkEnergyExperimentStep.MeasureDepression: hintText.text = "Press CONFIRM DEPTH at the bottom, or drag the depth marker onto the depression."; break;
            case WorkEnergyExperimentStep.RecordResult: hintText.text = "Press RECORD READING. The next height is prepared automatically."; break;
            default: hintText.text = "Follow the instruction above."; break;
        }
    }

    public void MoveWeightToHeight(float heightM)
    {
        if (weightVisual == null || scaleRoot == null) return;
        float h = scaleRoot.rect.height > 1f ? scaleRoot.rect.height : 520f;
        float clayY = ClayNorm * h;
        float oneY = OneMetreNorm * h;
        var pos = weightVisual.anchoredPosition;
        pos.y = clayY + heightM * (oneY - clayY) + 28f;
        if (impactPoint != null) pos.x = impactPoint.anchoredPosition.x;
        weightVisual.anchoredPosition = pos;
        if (holder != null)
        {
            var hp = holder.anchoredPosition;
            hp.y = clayY + heightM * (oneY - clayY);
            if (impactPoint != null) hp.x = impactPoint.anchoredPosition.x;
            holder.anchoredPosition = hp;
        }
        WorkEnergyReleaseMechanismController.Instance?.SetHeight(heightM);
    }

    private void TryConfirmHeight()
    {
        if (WorkEnergyPowerExperimentManager.Instance?.CurrentStep != WorkEnergyExperimentStep.SetHeight)
        {
            WorkEnergyScoreManager.Instance?.SubtractScore(5);
            return;
        }
        float h = WorkEnergyReleaseMechanismController.Instance != null
            ? WorkEnergyReleaseMechanismController.Instance.CurrentHeight
            : (heightSlider != null ? heightSlider.value : 0f);
        WorkEnergyHeightMeasurementManager.Instance?.SetDisplayedHeight(h);
        if (WorkEnergyHeightMeasurementManager.Instance != null && WorkEnergyHeightMeasurementManager.Instance.ConfirmSetHeight(targetHeight))
        {
            WorkEnergyScoreManager.Instance?.AddScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction($"✓ Height set to {targetHeight:0.00} m");
            WorkEnergyPowerExperimentManager.Instance.AdvanceStep();
        }
        else
        {
            WorkEnergyScoreManager.Instance?.SubtractScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction($"Set the height close to {targetHeight:0.00} m.");
        }
    }

    private void TryConfirmMeasureHeight()
    {
        if (WorkEnergyPowerExperimentManager.Instance?.CurrentStep != WorkEnergyExperimentStep.MeasureHeight)
        {
            WorkEnergyScoreManager.Instance?.SubtractScore(5);
            return;
        }
        if (WorkEnergyHeightMeasurementManager.Instance != null && WorkEnergyHeightMeasurementManager.Instance.ConfirmMeasuredHeight(targetHeight))
        {
            WorkEnergyScoreManager.Instance?.AddScore(5);
            UpdateEnergyLabel();
            WorkEnergyPowerExperimentManager.Instance.AdvanceStep();
        }
        else
        {
            WorkEnergyScoreManager.Instance?.SubtractScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("Measure the height from the clay surface to the bottom of the weight.");
        }
    }

    private void TryRelease()
    {
        var step = WorkEnergyPowerExperimentManager.Instance?.CurrentStep;
        if (step != WorkEnergyExperimentStep.ReleaseWeight)
        {
            WorkEnergyScoreManager.Instance?.SubtractScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("Finish the setup and height measurement before releasing.");
            return;
        }
        if (!clayPlaced || !standPlaced || !weightPlaced)
        {
            WorkEnergyScoreManager.Instance?.SubtractScore(5);
            return;
        }

        WorkEnergyScoreManager.Instance?.AddScore(5);
        float h = targetHeight;
        Vector2 hold = weightVisual != null ? weightVisual.anchoredPosition : Vector2.zero;
        Vector2 impact = hold;
        if (scaleRoot != null)
        {
            float rh = scaleRoot.rect.height > 1f ? scaleRoot.rect.height : 520f;
            impact.y = ClayNorm * rh + 36f;
        }
        if (impactPoint != null) impact.x = impactPoint.anchoredPosition.x;
        WorkEnergyFallingWeightController.Instance?.SetHeight(hold, impact);
        WorkEnergyFallingWeightController.Instance?.ReleaseWeight();
        WorkEnergyPowerExperimentManager.Instance.SetStep(WorkEnergyExperimentStep.ObserveImpact);
    }

    public void HandleImpact()
    {
        impactObserved = true;
        float mass = WorkEnergyExperimentDataManager.Instance != null ? WorkEnergyExperimentDataManager.Instance.StoredMass : 1f;
        lastPotentialEnergy = WorkEnergyPotentialEnergyCalculator.Instance != null
            ? WorkEnergyPotentialEnergyCalculator.Instance.CalculatePotentialEnergy(mass, targetHeight)
            : mass * 9.8f * targetHeight;
        lastSimulatedDepth = WorkEnergyDepressionController.Instance != null
            ? WorkEnergyDepressionController.Instance.CalculateDepth(lastPotentialEnergy)
            : 0.6f;
        WorkEnergyDepressionController.Instance?.DisplayDepression(lastSimulatedDepth);
        WorkEnergyScoreManager.Instance?.AddScore(5);
        UpdateEnergyLabel();
        if (depthSlider != null) depthSlider.value = Mathf.Clamp(lastSimulatedDepth * 0.7f, 0.1f, 3f);
        WorkEnergyPowerExperimentManager.Instance?.SetStep(WorkEnergyExperimentStep.MeasureDepression);
        WorkEnergyFeedbackManager.Instance?.ShowInstruction("Observe the depression produced on the clay.");
    }

    private void TryConfirmDepth()
    {
        if (WorkEnergyPowerExperimentManager.Instance?.CurrentStep != WorkEnergyExperimentStep.MeasureDepression)
        {
            WorkEnergyScoreManager.Instance?.SubtractScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("Wait until the weight hits the clay before measuring.");
            return;
        }
        if (!impactObserved)
        {
            WorkEnergyScoreManager.Instance?.SubtractScore(5);
            return;
        }
        if (WorkEnergyDepthMeasurementManager.Instance != null && WorkEnergyDepthMeasurementManager.Instance.ConfirmDepth(lastSimulatedDepth))
        {
            WorkEnergyScoreManager.Instance?.AddScore(5);
            WorkEnergyPowerExperimentManager.Instance.AdvanceStep();
        }
        else
        {
            WorkEnergyScoreManager.Instance?.SubtractScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction($"Read the depression depth on the scale. Target is about {lastSimulatedDepth:0.0} cm.");
        }
    }

    private void TryRecord()
    {
        if (WorkEnergyPowerExperimentManager.Instance?.CurrentStep != WorkEnergyExperimentStep.RecordResult)
        {
            WorkEnergyScoreManager.Instance?.SubtractScore(5);
            return;
        }
        if (WorkEnergyDepthMeasurementManager.Instance == null || !WorkEnergyDepthMeasurementManager.Instance.DepthMeasured)
        {
            WorkEnergyScoreManager.Instance?.SubtractScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("Measure the depression depth before recording.");
            return;
        }
        if (WorkEnergyExperimentDataManager.Instance != null && WorkEnergyExperimentDataManager.Instance.HasReadingForHeight(targetHeight))
        {
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("This height is already recorded. Reset the weight and choose another height.");
            return;
        }

        float mass = WorkEnergyExperimentDataManager.Instance != null ? WorkEnergyExperimentDataManager.Instance.StoredMass : 1f;
        int instance = (WorkEnergyExperimentDataManager.Instance != null ? WorkEnergyExperimentDataManager.Instance.Readings.Count : 0) + 1;
        WorkEnergyExperimentDataManager.Instance?.RecordReading(instance, mass, targetHeight, lastPotentialEnergy, lastSimulatedDepth);
        WorkEnergyScoreManager.Instance?.AddScore(5);
        WorkEnergyUIManager.Instance?.UpdateDataTable();
        WorkEnergyFeedbackManager.Instance?.ShowInstruction($"Reading {instance:00} recorded. Repeat using another height.");

        if (WorkEnergyExperimentDataManager.Instance != null && WorkEnergyExperimentDataManager.Instance.AllHeightsRecorded())
        {
            WorkEnergyScoreManager.Instance?.AddScore(5);
            WorkEnergyPowerExperimentManager.Instance.SetStep(WorkEnergyExperimentStep.CompareResults);
        }
        else
        {
            UpdateHint(WorkEnergyExperimentStep.RecordResult);
        }
    }

    public void TryResetForNextReading()
    {
        if (WorkEnergyExperimentDataManager.Instance != null && WorkEnergyExperimentDataManager.Instance.AllHeightsRecorded())
        {
            WorkEnergyPowerExperimentManager.Instance?.SetStep(WorkEnergyExperimentStep.CompareResults);
            return;
        }

        WorkEnergyFallingWeightController.Instance?.ResetWeight();
        WorkEnergyImpactController.Instance?.ResetImpact();
        WorkEnergyDepressionController.Instance?.ResetDepression();
        WorkEnergyHeightMeasurementManager.Instance?.ResetMeasurement(true);
        WorkEnergyDepthMeasurementManager.Instance?.ResetMeasurement();
        impactObserved = false;
        horizontalPenaltyGiven = false;
        targetHeight = WorkEnergyExperimentDataManager.Instance != null
            ? WorkEnergyExperimentDataManager.Instance.GetNextUnrecordedHeight()
            : 0.30f;
        MoveWeightToHeight(0.35f);
        WorkEnergyPowerExperimentManager.Instance?.SetStep(WorkEnergyExperimentStep.SetHeight);
        WorkEnergyFeedbackManager.Instance?.ShowInstruction("Repeat the experiment using another height.");
    }

    private void TryMeasureMass()
    {
        if (WorkEnergyPowerExperimentManager.Instance?.CurrentStep != WorkEnergyExperimentStep.MeasureMass)
        {
            WorkEnergyScoreManager.Instance?.SubtractScore(5);
            return;
        }
        float mass = WorkEnergyExperimentDataManager.Instance != null ? WorkEnergyExperimentDataManager.Instance.WeightMass : 1f;
        WorkEnergyExperimentDataManager.Instance?.StoreMeasuredMass(mass);
        massMeasured = true;
        WorkEnergyScoreManager.Instance?.AddScore(5);
        UpdateMassLabel();
        WorkEnergyFeedbackManager.Instance?.ShowInstruction($"Mass of Weight = {mass:0.0} kg");
        WorkEnergyPowerExperimentManager.Instance.AdvanceStep();
    }

    private void SkipMass()
    {
        if (WorkEnergyPowerExperimentManager.Instance?.CurrentStep != WorkEnergyExperimentStep.MeasureMass) return;
        WorkEnergyPowerExperimentManager.Instance.AdvanceStep();
    }

    private void UpdateMassLabel()
    {
        float mass = WorkEnergyExperimentDataManager.Instance != null ? WorkEnergyExperimentDataManager.Instance.StoredMass : 1f;
        if (massLabel != null) massLabel.text = $"WEIGHT  {mass:0.00} kg";
    }

    private void UpdateEnergyLabel()
    {
        float mass = WorkEnergyExperimentDataManager.Instance != null ? WorkEnergyExperimentDataManager.Instance.StoredMass : 1f;
        float h = WorkEnergyReleaseMechanismController.Instance != null ? WorkEnergyReleaseMechanismController.Instance.CurrentHeight : targetHeight;
        if (WorkEnergyPotentialEnergyCalculator.Instance != null && energyLabel != null)
            energyLabel.text = WorkEnergyPotentialEnergyCalculator.Instance.FormatCalculation(mass, h);
        float g = WorkEnergyPotentialEnergyCalculator.Instance != null ? WorkEnergyPotentialEnergyCalculator.Instance.Gravity : 9.8f;
        if (heightLabel != null)
            heightLabel.text = $"HEIGHT  h = {h:0.00} m";
    }

    public void ResetWorkbench()
    {
        clayPlaced = false;
        standPlaced = false;
        weightPlaced = false;
        massMeasured = false;
        impactObserved = false;
        horizontalPenaltyGiven = false;
        lastSimulatedDepth = 0f;
        lastPotentialEnergy = 0f;
        clayItem?.ResetItem();
        standItem?.ResetItem();
        weightItem?.ResetItem();
        balanceItem?.ResetItem();
        clayZone?.ClearItem();
        standZone?.ClearItem();
        weightZone?.ClearItem();
        balanceZone?.ClearItem();
        WorkEnergyClayController.Instance?.ResetClayPrep();
        WorkEnergyHeavyWeightController.Instance?.ResetPlacement();
        WorkEnergyReleaseStandController.Instance?.ResetStand();
        WorkEnergyReleaseMechanismController.Instance?.ResetMechanism();
        WorkEnergyFallingWeightController.Instance?.ResetWeight();
        WorkEnergyImpactController.Instance?.ResetImpact();
        WorkEnergyDepressionController.Instance?.ResetDepression();
        WorkEnergyHeightMeasurementManager.Instance?.ResetMeasurement();
        WorkEnergyDepthMeasurementManager.Instance?.ResetMeasurement();
    }

    public string GetCurrentResultsSummary()
    {
        var readings = WorkEnergyExperimentDataManager.Instance != null ? WorkEnergyExperimentDataManager.Instance.Readings : null;
        if (readings == null || readings.Count == 0) return "No readings recorded yet.";
        string s = "";
        foreach (var r in readings)
            s += $"Height = {r.height:0.00} m   PE = {r.potentialEnergy:0.00} J   Depth = {r.depressionDepth:0.0} cm\n";
        return s;
    }
}
