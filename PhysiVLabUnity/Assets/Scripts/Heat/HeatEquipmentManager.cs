using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeatEquipmentManager : MonoBehaviour
{
    public static HeatEquipmentManager Instance { get; private set; }

    private Transform tray;
    private readonly System.Collections.Generic.List<HeatDragDrop2D> items = new System.Collections.Generic.List<HeatDragDrop2D>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(Transform trayRoot)
    {
        tray = trayRoot;
        BuildTray();
    }

    public void ResetTray()
    {
        BuildTray();
    }

    private void BuildTray()
    {
        if (tray == null) return;
        for (int i = tray.childCount - 1; i >= 0; i--)
            Object.Destroy(tray.GetChild(i).gameObject);
        items.Clear();

        AddItem("TestTube", "Test tube", HeatEquipmentType.TestTube);
        AddItem("ColoredWater", "Coloured water", HeatEquipmentType.ColoredWater);
        AddItem("RubberStopper", "Rubber stopper", HeatEquipmentType.RubberStopper);
        AddItem("ThinGlassTube", "Thin glass tube", HeatEquipmentType.ThinGlassTube);
        AddItem("TripodStand", "Tripod stand", HeatEquipmentType.TripodStand);
        AddItem("Beaker", "Beaker", HeatEquipmentType.Beaker);
        AddItem("BunsenBurner", "Bunsen burner", HeatEquipmentType.BunsenBurner);
        AddItem("RetortStand", "Retort stand", HeatEquipmentType.RetortStand);
    }

    private void AddItem(string id, string label, HeatEquipmentType type)
    {
        var obj = new GameObject(id);
        obj.transform.SetParent(tray, false);
        var le = obj.AddComponent<LayoutElement>();
        le.preferredHeight = 92;
        le.minHeight = 88;
        var img = obj.AddComponent<Image>();
        img.color = Color.Lerp(HeatIconFactory.GetColor(type), Color.white, 0.55f);
        var drag = obj.AddComponent<HeatDragDrop2D>();
        drag.Configure(id);

        var iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(obj.transform, false);
        var irt = iconObj.AddComponent<RectTransform>();
        irt.anchorMin = new Vector2(0.08f, 0.38f);
        irt.anchorMax = new Vector2(0.92f, 0.96f);
        irt.offsetMin = irt.offsetMax = Vector2.zero;
        var icon = iconObj.AddComponent<Image>();
        icon.sprite = HeatIconFactory.GetSprite(type);
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(obj.transform, false);
        var lrt = labelObj.AddComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0.04f, 0.04f);
        lrt.anchorMax = new Vector2(0.96f, 0.38f);
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 18;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.12f, 0.14f, 0.20f);
        tmp.raycastTarget = false;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 14;
        tmp.fontSizeMax = 18;
        if (tmp.font == null) tmp.font = TMP_Settings.defaultFontAsset;

        items.Add(drag);
        Canvas.ForceUpdateCanvases();
        var rt = obj.GetComponent<RectTransform>();
        drag.StoreHome(tray, rt != null ? rt.anchoredPosition : Vector2.zero);
    }
}
