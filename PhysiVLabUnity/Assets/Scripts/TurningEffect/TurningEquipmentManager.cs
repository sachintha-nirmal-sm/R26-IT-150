using UnityEngine;
using UnityEngine.UI;

public class TurningEquipmentManager : MonoBehaviour
{
    public static TurningEquipmentManager Instance { get; private set; }

    [SerializeField] private Transform tray;

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

    public void BuildTray()
    {
        if (tray == null) return;
        for (int i = tray.childCount - 1; i >= 0; i--)
            Destroy(tray.GetChild(i).gameObject);

        MakeItem("Table", "Table", TurningIconFactory.GetNamed("table"), new Vector2(150, 90));
        MakeItem("Stick", "Calibrated stick", TurningIconFactory.GetNamed("stick"), new Vector2(170, 50));
        MakeItem("Drill", "Drill", TurningIconFactory.GetNamed("drill"), new Vector2(110, 90));
        MakeItem("Washer1", "Washer 1", TurningIconFactory.GetNamed("washer"), new Vector2(80, 70));
        MakeItem("ScrewNail", "Screw nail", TurningIconFactory.GetNamed("screw"), new Vector2(70, 90));
        MakeItem("Washer2", "Washer 2", TurningIconFactory.GetNamed("washer"), new Vector2(80, 70));
        MakeItem("Wire", "Wire loops", TurningIconFactory.GetNamed("wire"), new Vector2(120, 70));
        MakeItem("NewtonBalance", "Newton balance", TurningIconFactory.GetNamed("balance"), new Vector2(150, 90));
    }

    public void ResetTray() => BuildTray();

    private TurningDragDrop2D MakeItem(string id, string label, Sprite sprite, Vector2 size)
    {
        var go = new GameObject(id);
        go.transform.SetParent(tray, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.sprite = sprite != null ? sprite : TurningIconFactory.White();
        img.preserveAspect = true;
        img.color = Color.white;
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = size.x;
        le.preferredHeight = size.y + 36f;
        le.minHeight = size.y + 28f;
        var drag = go.AddComponent<TurningDragDrop2D>();
        drag.Configure(id);
        drag.StoreHome(tray, Vector2.zero);

        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(go.transform, false);
        var lrt = labelObj.AddComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0f, 0f);
        lrt.anchorMax = new Vector2(1f, 0.28f);
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var tmp = labelObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = new Color(0.12f, 0.16f, 0.22f);
        tmp.raycastTarget = false;
        if (tmp.font == null) tmp.font = TMPro.TMP_Settings.defaultFontAsset;
        return drag;
    }
}
