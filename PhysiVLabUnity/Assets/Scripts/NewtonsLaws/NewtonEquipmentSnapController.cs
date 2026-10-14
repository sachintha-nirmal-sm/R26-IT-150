using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewtonEquipmentSnapController : MonoBehaviour
{
    public static NewtonEquipmentSnapController Instance { get; private set; }

    private RectTransform trackArea;
    private RectTransform tray;
    private GameObject trackVisual;
    private GameObject rulerVisual;

    public bool TrackVisible { get; private set; }
    public bool RulerVisible { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(RectTransform track, RectTransform trayArea, GameObject trackVis, GameObject rulerVis)
    {
        trackArea = track;
        tray = trayArea;
        trackVisual = trackVis;
        rulerVisual = rulerVis;
        var step = NewtonsLawsExperimentManager.Instance != null
            ? NewtonsLawsExperimentManager.Instance.CurrentStep
            : NewtonExperimentStep.FirstLawSetup;
        RefreshForStep(step);
    }

    public void RefreshForStep(NewtonExperimentStep step)
    {
        if (tray == null) return;
        ClearTray();

        switch (step)
        {
            case NewtonExperimentStep.FirstLawSetup:
                if (!TrackVisible) CreateTrayItem("Track", "track", "TRACK", new Vector2(168, 72));
                if (!RulerVisible) CreateTrayItem("Ruler", "ruler", "RULER", new Vector2(168, 72));
                if (FirstLawExperimentManager.Instance == null || !FirstLawExperimentManager.Instance.TrolleyPlaced)
                    CreateTrayItem("Trolley", "trolley", "TROLLEY", new Vector2(168, 80));
                break;
            case NewtonExperimentStep.FirstLawStationary:
            case NewtonExperimentStep.FirstLawMoving:
            case NewtonExperimentStep.FirstLawFriction:
            case NewtonExperimentStep.FirstLawObservation:
            case NewtonExperimentStep.Introduction:
            case NewtonExperimentStep.Objective:
            case NewtonExperimentStep.SelectEquipment:
                break;
            case NewtonExperimentStep.SecondLawSetup:
            case NewtonExperimentStep.SecondLawConstantMass:
            case NewtonExperimentStep.SecondLawConstantForce:
                if (SecondLawExperimentManager.Instance == null || !SecondLawExperimentManager.Instance.TrolleyReady)
                    CreateTrayItem("Trolley", "trolley", "TROLLEY", new Vector2(168, 80));
                if (PulleyController.Instance == null || !PulleyController.Instance.Placed)
                    CreateTrayItem("Pulley", "pulley", "PULLEY", new Vector2(168, 80));
                if (StringConnectionController.Instance == null || !StringConnectionController.Instance.StringPlaced)
                    CreateTrayItem("String", "string", "STRING", new Vector2(168, 64));
                if (StringConnectionController.Instance == null || !StringConnectionController.Instance.HangerAttached)
                    CreateTrayItem("Hanger", "hanger", "HANGER", new Vector2(168, 80));
                break;
            case NewtonExperimentStep.ThirdLawSetup:
            case NewtonExperimentStep.ThirdLawExperiment:
                if (ThirdLawExperimentManager.Instance == null || !ThirdLawExperimentManager.Instance.StringReady)
                    CreateTrayItem("String", "string", "STRING", new Vector2(168, 64));
                if (ThirdLawExperimentManager.Instance == null || !ThirdLawExperimentManager.Instance.StrawReady)
                    CreateTrayItem("Straw", "straw", "STRAW", new Vector2(168, 64));
                if (ThirdLawExperimentManager.Instance == null || !ThirdLawExperimentManager.Instance.BalloonReady)
                    CreateTrayItem("Balloon", "balloon", "BALLOON", new Vector2(168, 80));
                break;
            case NewtonExperimentStep.WeightExperiment:
                CreateTrayItem("SpringBalance", "spring", "SPRING", new Vector2(168, 100));
                CreateTrayItem("Mass05", "mass", "0.5 kg", new Vector2(168, 72));
                CreateTrayItem("Mass10", "mass", "1.0 kg", new Vector2(168, 72));
                CreateTrayItem("Mass20", "mass", "2.0 kg", new Vector2(168, 72));
                break;
            default:
                CreateTrayItem("Trolley", "trolley", "TROLLEY", new Vector2(168, 80));
                CreateTrayItem("Pulley", "pulley", "PULLEY", new Vector2(168, 80));
                CreateTrayItem("String", "string", "STRING", new Vector2(168, 64));
                CreateTrayItem("Hanger", "hanger", "HANGER", new Vector2(168, 80));
                break;
        }

        if (tray != null) LayoutRebuilder.ForceRebuildLayoutImmediate(tray);
    }

    public void SpawnTrayItems() => RefreshForStep(
        NewtonsLawsExperimentManager.Instance != null
            ? NewtonsLawsExperimentManager.Instance.CurrentStep
            : NewtonExperimentStep.FirstLawSetup);

    private void ClearTray()
    {
        if (tray == null) return;
        for (int i = tray.childCount - 1; i >= 0; i--)
        {
            var child = tray.GetChild(i).gameObject;
            if (Application.isPlaying) Object.DestroyImmediate(child);
            else Object.DestroyImmediate(child);
        }
    }

    private void CreateTrayItem(string id, string spriteKey, string label, Vector2 size)
    {
        var go = new GameObject(id);
        go.transform.SetParent(tray, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = size.x;
        le.minWidth = size.x;
        le.preferredHeight = size.y;
        le.minHeight = size.y;
        var img = go.AddComponent<Image>();
        img.sprite = NewtonsLawsIconFactory.White();
        img.color = new Color(0.14f, 0.36f, 0.56f, 1f);

        var icon = new GameObject("Icon");
        icon.transform.SetParent(go.transform, false);
        var iconRt = icon.AddComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.04f, 0.28f);
        iconRt.anchorMax = new Vector2(0.96f, 0.96f);
        iconRt.offsetMin = iconRt.offsetMax = Vector2.zero;
        var iconImg = icon.AddComponent<Image>();
        iconImg.sprite = NewtonsLawsIconFactory.GetNamed(spriteKey);
        iconImg.preserveAspect = true;
        iconImg.raycastTarget = false;

        var textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.04f, 0.02f);
        textRt.anchorMax = new Vector2(0.96f, 0.30f);
        textRt.offsetMin = textRt.offsetMax = Vector2.zero;
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
        tmp.text = label;
        tmp.fontSize = 16;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        var drag = go.AddComponent<NewtonDragDrop2D>();
        drag.Configure(id);
        drag.StoreHome(tray, Vector2.zero);
        drag.OnIncorrectDrop += _ =>
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowMessage("✗ Place this equipment in the correct experimental position.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
        };
    }

    public void PlaceFromClick(NewtonDragDrop2D item)
    {
        if (item == null) return;
        PlaceById(item.ItemId);
    }

    public bool PlaceById(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return false;
        bool ok = AcceptItem(itemId, true);
        if (ok && itemId != "Mass05" && itemId != "Mass10" && itemId != "Mass20")
            HideTrayItem(itemId);
        return ok;
    }

    private void HideTrayItem(string itemId)
    {
        if (tray == null) return;
        for (int i = 0; i < tray.childCount; i++)
        {
            var child = tray.GetChild(i);
            if (child != null && child.name == itemId) child.gameObject.SetActive(false);
        }
    }

    public void OnItemDropped(NewtonUIDropTarget zone, NewtonDragDrop2D item)
    {
        if (zone == null || item == null) return;
        bool nearStart = zone.ZoneId == "Start" || zone.MeterValue <= 0.2f;
        bool ok = AcceptItem(item.ItemId, nearStart || zone.ZoneId == "Track" || zone.ZoneId == "Any" || zone.AcceptedItemId == item.ItemId);
        if (!ok)
        {
            item.ReturnHome();
            NewtonScoreManager.Instance?.SubtractScore(5);
        }
        else
        {
            item.gameObject.SetActive(false);
        }
    }

    public bool AcceptItem(string itemId, bool atStart)
    {
        var step = NewtonsLawsExperimentManager.Instance != null
            ? NewtonsLawsExperimentManager.Instance.CurrentStep
            : NewtonExperimentStep.FirstLawSetup;

        if (!IsExpectedNow(itemId, step))
        {
            NewtonFeedbackManager.Instance?.ShowInstruction("Use the PLACE buttons shown for this step.");
            return false;
        }

        switch (itemId)
        {
            case "Track":
                TrackVisible = true;
                if (trackVisual != null) trackVisual.SetActive(true);
                FirstLawExperimentManager.Instance?.NotifyTrackPlaced();
                return true;
            case "Ruler":
                RulerVisible = true;
                if (rulerVisual != null) rulerVisual.SetActive(true);
                FirstLawExperimentManager.Instance?.NotifyRulerPlaced();
                return true;
            case "Trolley":
                FirstLawExperimentManager.Instance?.NotifyTrolleyPlaced(atStart || step != NewtonExperimentStep.FirstLawSetup);
                SecondLawExperimentManager.Instance?.NotifyTrolleyPlaced();
                TrolleyController.Instance?.SetPosition(0f);
                return true;
            case "Pulley":
                PulleyController.Instance?.Place();
                SecondLawExperimentManager.Instance?.CheckSetup();
                NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
                return true;
            case "String":
                if (step >= NewtonExperimentStep.ThirdLawSetup && step <= NewtonExperimentStep.ThirdLawObservation)
                {
                    ThirdLawExperimentManager.Instance?.PlaceString();
                    return true;
                }
                StringConnectionController.Instance?.PlaceString();
                SecondLawExperimentManager.Instance?.CheckSetup();
                NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
                return true;
            case "Hanger":
            case "Mass":
                if (step == NewtonExperimentStep.WeightExperiment)
                {
                    WeightExperimentManager.Instance?.SelectObjectIndex(1);
                    return true;
                }
                StringConnectionController.Instance?.AttachHanger();
                SecondLawExperimentManager.Instance?.CheckSetup();
                NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
                return true;
            case "Mass05":
                WeightExperimentManager.Instance?.SelectObjectIndex(0);
                return true;
            case "Mass10":
                WeightExperimentManager.Instance?.SelectObjectIndex(1);
                return true;
            case "Mass20":
                WeightExperimentManager.Instance?.SelectObjectIndex(2);
                return true;
            case "Balloon":
                ThirdLawExperimentManager.Instance?.AttachBalloon();
                return true;
            case "Straw":
                ThirdLawExperimentManager.Instance?.PlaceStraw();
                return true;
            case "SpringBalance":
                SpringBalanceController.Instance?.ResetBalance();
                NewtonFeedbackManager.Instance?.ShowInstruction("Spring balance ready. Select an object and hang it.");
                return true;
            case "Stopwatch":
                NewtonFeedbackManager.Instance?.ShowInstruction("Stopwatch is ready to measure time.");
                return true;
            default:
                return false;
        }
    }

    private static bool IsExpectedNow(string itemId, NewtonExperimentStep step)
    {
        switch (step)
        {
            case NewtonExperimentStep.FirstLawSetup:
                return itemId == "Track" || itemId == "Ruler" || itemId == "Trolley";
            case NewtonExperimentStep.SecondLawSetup:
            case NewtonExperimentStep.SecondLawConstantMass:
            case NewtonExperimentStep.SecondLawConstantForce:
                return itemId == "Trolley" || itemId == "Pulley" || itemId == "String" || itemId == "Hanger" || itemId == "Mass";
            case NewtonExperimentStep.ThirdLawSetup:
            case NewtonExperimentStep.ThirdLawExperiment:
                return itemId == "String" || itemId == "Straw" || itemId == "Balloon";
            case NewtonExperimentStep.WeightExperiment:
                return itemId == "SpringBalance" || itemId == "Mass" || itemId == "Hanger"
                    || itemId == "Mass05" || itemId == "Mass10" || itemId == "Mass20";
            default:
                return true;
        }
    }

    public void ResetVisuals()
    {
        TrackVisible = false;
        RulerVisible = false;
        if (trackVisual != null) trackVisual.SetActive(false);
        if (rulerVisual != null) rulerVisual.SetActive(false);
        SpawnTrayItems();
    }
}
