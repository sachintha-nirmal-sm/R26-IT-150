using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeverLabWorkbench : MonoBehaviour
{
    public static LeverLabWorkbench Instance { get; private set; }

    [Header("Zones")]
    [SerializeField] private LeverUIDropTarget pivotZone;
    [SerializeField] private LeverUIDropTarget stripZone;
    [SerializeField] private LeverUIDropTarget bookZone;
    [SerializeField] private LeverUIDropTarget springBalanceZone;

    [Header("Draggables")]
    [SerializeField] private LeverDraggableUIItem pivotItem;
    [SerializeField] private LeverDraggableUIItem stripItem;
    [SerializeField] private LeverDraggableUIItem bookItem;
    [SerializeField] private LeverDraggableUIItem springBalanceItem;

    [Header("UI")]
    [SerializeField] private GameObject setupTray;
    [SerializeField] private GameObject experimentVisual;
    [SerializeField] private GameObject pullArea;
    [SerializeField] private Button recordBtn;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private TextMeshProUGUI momentLabel;

    private bool pivotPlaced;
    private bool stripPlaced;
    private bool bookPlaced;
    private bool springAttached;
    private bool pullAwarded;
    private bool liftAwarded;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Bind(
        LeverUIDropTarget pivotZ, LeverUIDropTarget stripZ, LeverUIDropTarget bookZ, LeverUIDropTarget springZ,
        LeverDraggableUIItem pivotI, LeverDraggableUIItem stripI, LeverDraggableUIItem bookI, LeverDraggableUIItem springI,
        GameObject tray, GameObject expVis, GameObject pull, Button record, TextMeshProUGUI hint, TextMeshProUGUI moment)
    {
        pivotZone = pivotZ; stripZone = stripZ; bookZone = bookZ; springBalanceZone = springZ;
        pivotItem = pivotI; stripItem = stripI; bookItem = bookI; springBalanceItem = springI;
        setupTray = tray; experimentVisual = expVis; pullArea = pull;
        recordBtn = record; hintText = hint; momentLabel = moment;

        if (recordBtn != null)
        {
            recordBtn.onClick.RemoveAllListeners();
            recordBtn.onClick.AddListener(RecordCurrentReading);
        }

        StoreHomes();
    }

    private void StoreHomes()
    {
        StoreHome(pivotItem);
        StoreHome(stripItem);
        StoreHome(bookItem);
        StoreHome(springBalanceItem);
    }

    private void StoreHome(LeverDraggableUIItem item)
    {
        if (item == null) return;
        item.StoreHome(item.transform.parent, Vector2.zero);
    }

    public void UpdateForStep(LeverExperimentStep step)
    {
        bool labActive = step >= LeverExperimentStep.PlacePivot && step <= LeverExperimentStep.NextXOrCompare;
        if (setupTray != null) setupTray.SetActive(labActive);
        if (experimentVisual != null)
            experimentVisual.SetActive(pivotPlaced || step >= LeverExperimentStep.PlaceWoodenStrip);

        SetZoneVisible(pivotZone, step == LeverExperimentStep.PlacePivot && !pivotPlaced);
        SetZoneVisible(stripZone, step == LeverExperimentStep.PlaceWoodenStrip && !stripPlaced);
        SetZoneVisible(bookZone, step == LeverExperimentStep.PlaceBook && !bookPlaced);
        SetZoneVisible(springBalanceZone, step == LeverExperimentStep.AttachSpringBalance && !springAttached);

        SetDraggable(pivotItem, step == LeverExperimentStep.PlacePivot && !pivotPlaced);
        SetDraggable(stripItem, step == LeverExperimentStep.PlaceWoodenStrip && pivotPlaced && !stripPlaced);
        SetDraggable(bookItem, step == LeverExperimentStep.PlaceBook && stripPlaced && !bookPlaced);
        SetDraggable(springBalanceItem, step == LeverExperimentStep.AttachSpringBalance && bookPlaced && !springAttached);

        bool pullPhase = step == LeverExperimentStep.PullBalance || step == LeverExperimentStep.ObserveLift;
        if (pullArea != null) pullArea.SetActive(springAttached && (pullPhase || step == LeverExperimentStep.RecordReading || step == LeverExperimentStep.SelectDistanceX));
        if (recordBtn != null) recordBtn.gameObject.SetActive(step == LeverExperimentStep.RecordReading || step == LeverExperimentStep.ObserveLift);

        LeverMeasurementManager.Instance?.ShowMeasureAUI(step == LeverExperimentStep.MeasureDistanceA);
        LeverMeasurementManager.Instance?.ShowMeasureXUI(step == LeverExperimentStep.SelectDistanceX);
        if (step == LeverExperimentStep.SelectDistanceX)
        {
            ResetTrialGadgets();
            LeverMeasurementManager.Instance?.PrepareForNextX();
        }

        // Keep measure row visible only during measurement steps.
        if (setupTray != null)
        {
            var measureRow = setupTray.transform.parent?.Find("MeasureRow");
            if (measureRow != null)
            {
                bool showMeasure = step == LeverExperimentStep.MeasureDistanceA || step == LeverExperimentStep.SelectDistanceX;
                measureRow.gameObject.SetActive(showMeasure);
            }
        }

        LeverMeasurementManager.Instance?.UpdateLabels();

        // Bring pull UI to front during pull.
        if (pullArea != null && pullPhase)
            pullArea.transform.SetAsLastSibling();

        RefreshInfo();
        UpdateHint(step);
        UpdateMomentDisplay();
    }

    private void UpdateHint(LeverExperimentStep step)
    {
        if (hintText == null) return;
        switch (step)
        {
            case LeverExperimentStep.PlacePivot: hintText.text = "Place the support P underneath the wooden strip."; break;
            case LeverExperimentStep.PlaceWoodenStrip: hintText.text = "Place the wooden strip on the support P."; break;
            case LeverExperimentStep.PlaceBook: hintText.text = "Place the book on one end of the wooden strip."; break;
            case LeverExperimentStep.MeasureDistanceA: hintText.text = "Tap  a = 20 cm  (or type 20 and Confirm)."; break;
            case LeverExperimentStep.AttachSpringBalance: hintText.text = "Drag Spring Balance to the SPRING BALANCE zone."; break;
            case LeverExperimentStep.SelectDistanceX: hintText.text = "Tap the GREEN x button for this trial."; break;
            case LeverExperimentStep.PullBalance: hintText.text = "Drag handle DOWN, TAP it, or press + FORCE until book lifts."; break;
            case LeverExperimentStep.ObserveLift: hintText.text = "Book lifted! Press RECORD READING."; break;
            case LeverExperimentStep.RecordReading: hintText.text = "Press RECORD READING to save this trial."; break;
            case LeverExperimentStep.NextXOrCompare: hintText.text = "Press NEXT STEP to continue."; break;
            default: hintText.text = "Follow the instruction above."; break;
        }
    }

    public LeverUIDropTarget FindZoneForItem(string itemId)
    {
        switch (itemId)
        {
            case "Pivot":
            case "Support":
            case "SupportPivot":
                return pivotZone;
            case "Strip":
            case "WoodenStrip":
                return stripZone;
            case "Book":
                return bookZone;
            case "SpringBalance":
            case "NewtonSpringBalance":
                return springBalanceZone;
            default:
                return null;
        }
    }

    public void OnItemDropped(string zoneId, LeverDraggableUIItem item)
    {
        var step = LeverExperimentManager.Instance?.CurrentStep ?? LeverExperimentStep.Introduction;
        switch (zoneId)
        {
            case "Pivot":
            case "Support":
            case "SupportPivot":
                if (step != LeverExperimentStep.PlacePivot) { RejectDrop(zoneId, item); return; }
                pivotPlaced = true;
                LeverPivotController.Instance?.MarkPlaced(true);
                LeverScoreManager.Instance?.AddScore(5);
                LeverFeedbackManager.Instance?.ShowInstruction("Support P placed correctly.");
                LeverExperimentManager.Instance?.AdvanceStep();
                break;

            case "Strip":
            case "WoodenStrip":
                if (step != LeverExperimentStep.PlaceWoodenStrip || !pivotPlaced) { RejectDrop(zoneId, item); return; }
                stripPlaced = true;
                LeverPivotController.Instance?.ConfirmSupported();
                LeverScoreManager.Instance?.AddScore(5);
                LeverFeedbackManager.Instance?.ShowInstruction("Wooden strip placed on support P.");
                LeverExperimentManager.Instance?.AdvanceStep();
                break;

            case "Book":
                if (step != LeverExperimentStep.PlaceBook || !stripPlaced) { RejectDrop(zoneId, item); return; }
                bookPlaced = true;
                BookLiftController.Instance?.CaptureRest();
                LeverScoreManager.Instance?.AddScore(5);
                LeverFeedbackManager.Instance?.ShowInstruction("Book (load) placed on the strip.");
                LeverExperimentManager.Instance?.AdvanceStep();
                break;

            case "SpringBalance":
            case "NewtonSpringBalance":
                if (step != LeverExperimentStep.AttachSpringBalance || !bookPlaced) { RejectDrop(zoneId, item); return; }
                springAttached = true;
                LeverScoreManager.Instance?.AddScore(10);
                LeverFeedbackManager.Instance?.ShowInstruction("Newton spring balance attached. +10");
                LeverExperimentManager.Instance?.AdvanceStep();
                break;

            default:
                RejectDrop(zoneId, item);
                break;
        }
    }

    private void RejectDrop(string zoneId, LeverDraggableUIItem item)
    {
        PenalizeWrong();
        item?.ResetItem();
        switch (zoneId)
        {
            case "Pivot":
            case "Support":
            case "SupportPivot":
                pivotZone?.ClearItem(); break;
            case "Strip":
            case "WoodenStrip":
                stripZone?.ClearItem(); break;
            case "Book":
                bookZone?.ClearItem(); break;
            case "SpringBalance":
            case "NewtonSpringBalance":
                springBalanceZone?.ClearItem(); break;
        }
    }

    public void OnPullForceChanged(float force)
    {
        var data = LeverExperimentDataManager.Instance;
        float x = data != null ? data.GetCurrentX() : 10f;
        float required = data != null ? data.GetRequiredEffort(x) : 0f;
        float weight = data != null ? data.bookWeight : 10f;
        float a = data != null ? data.distanceA : 20f;

        LeverUIManager.Instance?.UpdateInfoPanel(weight, a, x, required, force);
        LeverWoodenStripController.Instance?.ApplyForce(force, required);
        UpdateMomentDisplay(force, required);

        var step = LeverExperimentManager.Instance?.CurrentStep;
        if (step != LeverExperimentStep.PullBalance && step != LeverExperimentStep.ObserveLift)
            return;

        if (!pullAwarded && force > 0.5f)
        {
            pullAwarded = true;
            LeverScoreManager.Instance?.AddScore(5);
        }

        bool lifted = BookLiftController.Instance != null &&
                      BookLiftController.Instance.TryLift(force, required);

        if (lifted && !liftAwarded && step == LeverExperimentStep.PullBalance)
        {
            liftAwarded = true;
            LeverScoreManager.Instance?.AddScore(5);
            LeverFeedbackManager.Instance?.ShowInstruction(
                $"✓ Book lifted!\nRequired Effort ≈ {required:0.00} N");
            LeverExperimentManager.Instance?.AdvanceStep();
        }
    }

    public void RecordCurrentReading()
    {
        var step = LeverExperimentManager.Instance?.CurrentStep;
        if (step != LeverExperimentStep.RecordReading && step != LeverExperimentStep.ObserveLift)
        {
            PenalizeWrong();
            return;
        }

        float force = NewtonSpringBalanceController.Instance != null
            ? NewtonSpringBalanceController.Instance.GetReading()
            : 0f;
        bool lifted = BookLiftController.Instance != null && BookLiftController.Instance.IsLifted;

        if (!lifted)
        {
            PenalizeWrong();
            LeverFeedbackManager.Instance?.ShowInstruction("Pull until the book lifts before recording.");
            return;
        }

        LeverExperimentDataManager.Instance?.RecordReadingForCurrentX(force, true);
        LeverScoreManager.Instance?.AddScore(5);
        LeverUIManager.Instance?.UpdateDataTable();
        LeverFeedbackManager.Instance?.ShowInstruction($"Recorded: Effort ≈ {force:0.00} N");

        if (step == LeverExperimentStep.ObserveLift)
            LeverExperimentManager.Instance?.SetStep(LeverExperimentStep.RecordReading);

        LeverExperimentManager.Instance?.AdvanceStep();
        LeverUIManager.Instance?.SetNextButtonVisible(true);
    }

    private void ResetTrialGadgets()
    {
        pullAwarded = false;
        liftAwarded = false;
        BookLiftController.Instance?.ResetPosition();
        LeverWoodenStripController.Instance?.ResetStrip();
        NewtonSpringBalanceController.Instance?.ResetBalance();
        LeverSpringController.Instance?.ResetSpring();
        LeverPullHandleController.Instance?.ResetHandle();
    }

    public void ResetWorkbench()
    {
        pivotPlaced = stripPlaced = bookPlaced = springAttached = false;
        pullAwarded = liftAwarded = false;
        pivotItem?.ResetItem(); stripItem?.ResetItem(); bookItem?.ResetItem(); springBalanceItem?.ResetItem();
        pivotZone?.ClearItem(); stripZone?.ClearItem(); bookZone?.ClearItem(); springBalanceZone?.ClearItem();
        LeverPivotController.Instance?.ResetPivot();
        LeverWoodenStripController.Instance?.ResetStrip();
        BookLiftController.Instance?.ResetPosition();
        NewtonSpringBalanceController.Instance?.ResetBalance();
        LeverSpringController.Instance?.ResetSpring();
        LeverPullHandleController.Instance?.ResetHandle();
        LeverMeasurementManager.Instance?.ResetMeasurement();
        RefreshInfo();
    }

    private void RefreshInfo()
    {
        var data = LeverExperimentDataManager.Instance;
        float weight = data != null ? data.bookWeight : 10f;
        float a = data != null ? data.distanceA : 20f;
        float x = data != null ? data.GetCurrentX() : 10f;
        float required = data != null ? data.GetRequiredEffort(x) : 0f;
        float force = NewtonSpringBalanceController.Instance != null
            ? NewtonSpringBalanceController.Instance.GetReading()
            : 0f;
        LeverUIManager.Instance?.UpdateInfoPanel(weight, a, x, required, force);
    }

    private void UpdateMomentDisplay(float effort = -1f, float required = -1f)
    {
        if (momentLabel == null) return;
        var data = LeverExperimentDataManager.Instance;
        float load = data != null ? data.bookWeight : 10f;
        float a = data != null ? data.distanceA : 20f;
        float x = data != null ? data.GetCurrentX() : 10f;
        if (required < 0f) required = data != null ? data.GetRequiredEffort(x) : 0f;
        if (effort < 0f)
            effort = NewtonSpringBalanceController.Instance != null ? NewtonSpringBalanceController.Instance.GetReading() : 0f;

        float loadMoment = load * a;
        float effortMoment = effort * x;
        momentLabel.text =
            $"Load Moment = {load:0} × {a:0} = {loadMoment:0.0} N·cm\n" +
            $"Effort Moment = {effort:0.00} × {x:0} = {effortMoment:0.0} N·cm\n" +
            (effort >= required * 0.98f ? "Moments are approximately balanced." : "Keep pulling until moments balance.");
    }

    private void SetZoneVisible(LeverUIDropTarget zone, bool visible)
    {
        if (zone != null) zone.gameObject.SetActive(visible);
    }

    private void SetDraggable(LeverDraggableUIItem item, bool value)
    {
        if (item != null) item.SetDraggable(value);
    }

    private void PenalizeWrong()
    {
        LeverScoreManager.Instance?.SubtractScore(5);
        LeverGameManager.Instance?.RegisterMistake();
    }
}
