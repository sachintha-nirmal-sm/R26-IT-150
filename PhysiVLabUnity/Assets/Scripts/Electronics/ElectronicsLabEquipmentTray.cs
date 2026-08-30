using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElectronicsLabEquipmentTray : MonoBehaviour
{
    public static ElectronicsLabEquipmentTray Instance { get; private set; }

    private Transform tray;
    private readonly List<GameObject> items = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(Transform trayRoot)
    {
        tray = trayRoot;
        ResetTray();
    }

    public void ResetTray()
    {
        foreach (var obj in items)
            if (obj != null) Destroy(obj);
        items.Clear();
        if (tray == null) return;
        Spawn("Breadboard", "Circuit board", ElectronicsEquipmentType.Breadboard);
        Spawn("Battery", "Battery 3V", ElectronicsEquipmentType.DryCells);
        Spawn("Switch", "Switch", ElectronicsEquipmentType.Switch);
        Spawn("Diode", "IN4001", ElectronicsEquipmentType.Diode);
        Spawn("Bulb", "Torch bulb", ElectronicsEquipmentType.Bulb);
        Spawn("Wire", "Wire 1", ElectronicsEquipmentType.Wires);
        Spawn("Wire", "Wire 2", ElectronicsEquipmentType.Wires);
        Spawn("Wire", "Wire 3", ElectronicsEquipmentType.Wires);
        Spawn("Wire", "Wire 4", ElectronicsEquipmentType.Wires);
    }

    private void Spawn(string id, string label, ElectronicsEquipmentType type)
    {
        var obj = new GameObject(id);
        obj.transform.SetParent(tray, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(170, 92);
        var img = obj.AddComponent<Image>();
        img.sprite = ElectronicsIconFactory.White();
        img.color = Color.Lerp(ElectronicsIconFactory.GetColor(type), Color.white, 0.35f);
        var le = obj.AddComponent<LayoutElement>();
        le.preferredHeight = 96;
        le.minHeight = 88;
        le.preferredWidth = 170;

        var icon = new GameObject("Icon");
        icon.transform.SetParent(obj.transform, false);
        var irt = icon.AddComponent<RectTransform>();
        irt.anchorMin = new Vector2(0.06f, 0.32f);
        irt.anchorMax = new Vector2(0.38f, 0.94f);
        irt.offsetMin = irt.offsetMax = Vector2.zero;
        var iimg = icon.AddComponent<Image>();
        iimg.sprite = ElectronicsIconFactory.GetSprite(type);
        iimg.preserveAspect = true;
        iimg.raycastTarget = false;

        var textObj = new GameObject("Label");
        textObj.transform.SetParent(obj.transform, false);
        var trt = textObj.AddComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.40f, 0.08f);
        trt.anchorMax = new Vector2(0.96f, 0.92f);
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        var tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.fontStyle = TMPro.FontStyles.Bold;
        tmp.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        tmp.color = new Color(0.10f, 0.16f, 0.26f);
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 12;
        tmp.fontSizeMax = 16;
        tmp.overflowMode = TMPro.TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        if (tmp.font == null) tmp.font = TMPro.TMP_Settings.defaultFontAsset;

        var drag = obj.AddComponent<ElectronicsDragDrop2D>();
        drag.Configure(id);
        drag.StoreHome(tray, Vector2.zero);
        var comp = obj.AddComponent<ElectronicsCircuitComponent>();
        comp.Configure(id, "In", "Out");
        items.Add(obj);
    }
}
