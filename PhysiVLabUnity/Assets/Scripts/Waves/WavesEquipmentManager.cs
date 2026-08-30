using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WavesEquipmentManager : MonoBehaviour
{
    public static WavesEquipmentManager Instance { get; private set; }

    private Transform tray;
    private readonly System.Collections.Generic.List<WavesDragDrop2D> items = new System.Collections.Generic.List<WavesDragDrop2D>();

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

        AddItem("Table", "Table", WavesEquipmentType.Table);
        AddItem("Slinky", "Slinky", WavesEquipmentType.Slinky);
        for (int i = 0; i < 5; i++)
            AddItem("Ribbon" + i, "Ribbon " + (i + 1), WavesEquipmentType.Ribbons);
    }

    private void AddItem(string id, string label, WavesEquipmentType type)
    {
        var obj = new GameObject(id);
        obj.transform.SetParent(tray, false);
        var le = obj.AddComponent<LayoutElement>();
        le.preferredHeight = 92;
        le.minHeight = 88;
        var img = obj.AddComponent<Image>();
        img.color = Color.Lerp(WavesIconFactory.GetColor(type), Color.white, 0.55f);
        var drag = obj.AddComponent<WavesDragDrop2D>();
        drag.Configure(id);

        var iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(obj.transform, false);
        var irt = iconObj.AddComponent<RectTransform>();
        irt.anchorMin = new Vector2(0.08f, 0.38f);
        irt.anchorMax = new Vector2(0.92f, 0.96f);
        irt.offsetMin = irt.offsetMax = Vector2.zero;
        var icon = iconObj.AddComponent<Image>();
        icon.sprite = WavesIconFactory.GetSprite(type);
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
