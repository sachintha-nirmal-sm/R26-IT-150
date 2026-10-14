using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MotionTrackController : MonoBehaviour
{
    public static MotionTrackController Instance { get; private set; }

    [SerializeField] private float trackLengthMeters = 5f;
    [SerializeField] private bool trackPlaced;
    [SerializeField] private bool rulerPlaced;
    [SerializeField] private bool carPlaced;
    [SerializeField] private bool stopwatchPlaced;
    [SerializeField] private bool directionSet;

    private readonly HashSet<int> placedMarkers = new HashSet<int>();
    private RectTransform trackArea;
    private RectTransform tray;
    private RectTransform carRect;
    private readonly List<MotionDragDrop2D> trayItems = new List<MotionDragDrop2D>();
    private TMP_FontAsset font;

    public bool TrackPlaced => trackPlaced;
    public bool RulerPlaced => rulerPlaced;
    public bool CarPlaced => carPlaced;
    public bool StopwatchPlaced => stopwatchPlaced;
    public bool DirectionSet => directionSet;
    public int MarkersPlaced => placedMarkers.Count;
    public bool SetupComplete => trackPlaced && rulerPlaced && carPlaced && stopwatchPlaced && directionSet && placedMarkers.Count >= 5;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Configure(float length) => trackLengthMeters = Mathf.Max(1f, length);

    public void Bind(RectTransform track, RectTransform equipmentTray, RectTransform car, TMP_FontAsset defaultFont)
    {
        trackArea = track;
        tray = equipmentTray;
        carRect = car;
        font = defaultFont;
        MotionPositionController.Instance?.Bind(track, car);
        MotionPositionController.Instance?.Configure(trackLengthMeters);
        ToyCarController.Instance?.ConfigureLimits(0f, trackLengthMeters);
    }

    public void BuildTrayItems()
    {
        ClearTray();
        if (tray == null) return;
        CreateTrayItem("Track", "Straight Track", MotionEquipmentType.StraightTrack);
        CreateTrayItem("Ruler", "Metre Ruler", MotionEquipmentType.MetreRuler);
        CreateTrayItem("ToyCar", "Toy Car", MotionEquipmentType.ToyCar);
        CreateTrayItem("Stopwatch", "Stopwatch", MotionEquipmentType.Stopwatch);
        CreateTrayItem("StartMarker", "Start Marker", MotionEquipmentType.StartingMarker);
        for (int m = 1; m <= 5; m++)
            CreateTrayItem("Marker" + m, m + " m Marker", MotionEquipmentType.DistanceMarkers, m);
    }

    public void PlaceFromClick(MotionDragDrop2D item)
    {
        if (item == null || trackArea == null) return;
        var target = FindTargetFor(item);
        if (target != null && target.CanAccept(item))
            target.AcceptDrop(item);
        else
        {
            MotionScoreManager.Instance?.SubtractScore(5);
            MotionFeedbackManager.Instance?.ShowMessage("✗ Place this item on the matching snap area on the track.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
        }
    }

    public void OnItemDropped(MotionUIDropTarget zone, MotionDragDrop2D item)
    {
        if (zone == null || item == null) return;
        string id = item.ItemId ?? "";
        if (zone.ZoneId == "Track" && id == "Track")
        {
            AwardOnce(ref trackPlaced, 5, "✓ Track placed correctly along the laboratory bench.");
            HideItem(item);
            ShowTrackVisual(true);
        }
        else if ((zone.ZoneId == "Ruler" || zone.ZoneId == "Track") && id == "Ruler")
        {
            if (!trackPlaced)
            {
                item.ReturnHome();
                MotionScoreManager.Instance?.SubtractScore(5);
                MotionFeedbackManager.Instance?.ShowMessage("✗ Place the straight track first.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
                return;
            }
            AwardOnce(ref rulerPlaced, 5, "✓ The metre scale is aligned with the track.");
            HideItem(item);
            ShowRulerVisual(true);
        }
        else if ((zone.ZoneId == "Start" || zone.ZoneId == "Track") && (id == "ToyCar" || id == "StartMarker"))
        {
            if (!trackPlaced)
            {
                item.ReturnHome();
                MotionScoreManager.Instance?.SubtractScore(5);
                MotionFeedbackManager.Instance?.ShowMessage("✗ Place the track before the car.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
                return;
            }
            float meters = MotionPositionController.Instance != null ? MotionPositionController.Instance.GetPositionMeters() : 0f;
            if (id == "ToyCar")
            {
                MotionPositionController.Instance?.SetPositionMeters(0f);
                if (carRect != null) carRect.gameObject.SetActive(true);
                AwardOnce(ref carPlaced, 5, "✓ Starting position correct. The car is at 0 m.");
                HideItem(item);
            }
            else
            {
                HideItem(item);
                MotionScoreManager.Instance?.AddScore(5, false);
                MotionFeedbackManager.Instance?.ShowMessage("✓ Starting marker placed at 0 m.", "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
            }
            if (meters > 0.35f && id == "ToyCar")
            {
                MotionScoreManager.Instance?.SubtractScore(5);
                MotionFeedbackManager.Instance?.ShowMessage("✗ The car must start at 0 m.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
            }
        }
        else if ((zone.ZoneId == "Stopwatch" || zone.ZoneId == "Track") && id == "Stopwatch")
        {
            AwardOnce(ref stopwatchPlaced, 5, "✓ A stopwatch is required to measure the time taken by the car.");
            HideItem(item);
        }
        else if (zone.ZoneId == "Marker" && id.StartsWith("Marker"))
        {
            int expected = Mathf.RoundToInt(zone.MeterValue);
            int actual = Mathf.RoundToInt(item.MeterValue);
            if (expected == actual && expected >= 1 && expected <= 5)
            {
                if (placedMarkers.Add(expected))
                {
                    MotionScoreManager.Instance?.AddScore(3, false);
                    MotionFeedbackManager.Instance?.ShowMessage($"✓ Marker placed at {expected} m.", "+3 Marks", new Color(0.08f, 0.52f, 0.22f));
                    HideItem(item);
                }
            }
            else
            {
                item.ReturnHome();
                MotionScoreManager.Instance?.SubtractScore(3);
                MotionFeedbackManager.Instance?.ShowMessage($"✗ That marker belongs at {actual} m, not {expected} m.", "-3 Marks", new Color(0.75f, 0.12f, 0.12f));
            }
        }
        else
        {
            item.ReturnHome();
            MotionScoreManager.Instance?.SubtractScore(5);
            MotionFeedbackManager.Instance?.ShowMessage("✗ That item does not belong in this position.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
        }

        MotionExperimentManager.Instance?.NotifySetupChanged();
    }

    public void ConfirmCarStart()
    {
        if (carRect != null)
        {
            carRect.gameObject.SetActive(true);
            MotionPositionController.Instance?.SetPositionMeters(0f);
        }
        AwardOnce(ref carPlaced, 5, "✓ Starting position correct. The car is at 0 m. Press NEXT STEP.");
        MotionExperimentManager.Instance?.NotifySetupChanged();
    }

    public void EnsureCarVisibleAtStart()
    {
        if (carRect != null)
        {
            carRect.gameObject.SetActive(true);
            MotionPositionController.Instance?.SetPositionMeters(0f);
        }
        if (carPlaced)
            MotionExperimentManager.Instance?.NotifySetupChanged();
    }

    public void ConfirmDirection()
    {
        if (directionSet) return;
        directionSet = true;
        MotionScoreManager.Instance?.AddScore(5, false);
        MotionFeedbackManager.Instance?.ShowMessage("✓ Positive direction: START → FINISH.", "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
        MotionExperimentManager.Instance?.NotifySetupChanged();
    }

    public void ResetSetupKeepScore()
    {
        ToyCarController.Instance?.ResetPosition();
        StopwatchController.Instance?.ResetTimer();
        if (carRect != null) carRect.gameObject.SetActive(carPlaced);
        MotionMeasurementManager.Instance?.RefreshLivePanel();
    }

    public void FullReset()
    {
        trackPlaced = rulerPlaced = carPlaced = stopwatchPlaced = directionSet = false;
        placedMarkers.Clear();
        ToyCarController.Instance?.ResetPosition();
        StopwatchController.Instance?.ResetTimer();
        if (carRect != null) carRect.gameObject.SetActive(false);
        ShowTrackVisual(false);
        ShowRulerVisual(false);
        BuildTrayItems();
    }

    private void AwardOnce(ref bool flag, int marks, string message)
    {
        if (flag) return;
        flag = true;
        MotionScoreManager.Instance?.AddScore(marks, false);
        MotionFeedbackManager.Instance?.ShowMessage(message, $"+{marks} Marks", new Color(0.08f, 0.52f, 0.22f));
    }

    private void HideItem(MotionDragDrop2D item)
    {
        if (item != null) item.gameObject.SetActive(false);
    }

    private void ShowTrackVisual(bool on)
    {
        var visual = trackArea != null ? trackArea.Find("TrackVisual") : null;
        if (visual != null) visual.gameObject.SetActive(on);
    }

    private void ShowRulerVisual(bool on)
    {
        var visual = trackArea != null ? trackArea.Find("RulerVisual") : null;
        if (visual != null) visual.gameObject.SetActive(on);
    }

    private MotionUIDropTarget FindTargetFor(MotionDragDrop2D item)
    {
        if (trackArea == null) return null;
        var targets = trackArea.GetComponentsInChildren<MotionUIDropTarget>(true);
        foreach (var t in targets)
            if (t.CanAccept(item)) return t;
        return null;
    }

    private void CreateTrayItem(string id, string label, MotionEquipmentType type, float meters = 0f)
    {
        var obj = new GameObject(id);
        obj.transform.SetParent(tray, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(118, 80);
        var bg = obj.AddComponent<Image>();
        bg.color = new Color(0.97f, 0.98f, 1f, 1f);
        var le = obj.AddComponent<LayoutElement>();
        le.preferredWidth = 118;
        le.preferredHeight = 80;
        le.minWidth = 88;
        le.minHeight = 64;
        le.flexibleWidth = 1f;

        var iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(obj.transform, false);
        var irt = iconObj.AddComponent<RectTransform>();
        irt.anchorMin = new Vector2(0.10f, 0.36f);
        irt.anchorMax = new Vector2(0.90f, 0.96f);
        irt.offsetMin = irt.offsetMax = Vector2.zero;
        var icon = iconObj.AddComponent<Image>();
        icon.sprite = MotionIconFactory.GetSprite(type);
        icon.preserveAspect = true;
        icon.color = Color.white;
        icon.raycastTarget = false;

        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(obj.transform, false);
        var lrt = labelObj.AddComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0.04f, 0.02f);
        lrt.anchorMax = new Vector2(0.96f, 0.34f);
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var tmp = labelObj.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 16;
        tmp.fontSizeMax = 22;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.08f, 0.12f, 0.18f);
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;

        var drag = obj.AddComponent<MotionDragDrop2D>();
        drag.Configure(id, meters);
        drag.StoreHome(tray, Vector2.zero);
        trayItems.Add(drag);
    }

    private void ClearTray()
    {
        foreach (var item in trayItems)
            if (item != null) Destroy(item.gameObject);
        trayItems.Clear();
        if (tray == null) return;
        for (int i = tray.childCount - 1; i >= 0; i--)
            Destroy(tray.GetChild(i).gameObject);
    }
}
