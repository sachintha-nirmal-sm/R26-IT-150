using UnityEngine;
using UnityEngine.UI;

public class EquilibriumEquipmentManager : MonoBehaviour
{
    public static EquilibriumEquipmentManager Instance { get; private set; }

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

        MakeItem("Stand", "Retort stand", EquilibriumIconFactory.GetNamed("stand"), new Vector2(140, 110));
        MakeItem("Balance1", "Spring balance F1", EquilibriumIconFactory.GetNamed("balance"), new Vector2(150, 90));
        MakeItem("Balance2", "Spring balance F2", EquilibriumIconFactory.GetNamed("balance"), new Vector2(150, 90));
        MakeItem("Ruler", "Meter ruler", EquilibriumIconFactory.GetNamed("ruler"), new Vector2(170, 50));
        MakeItem("BandLeft", "Rubber band (left)", EquilibriumIconFactory.GetNamed("band"), new Vector2(90, 70));
        MakeItem("BandRight", "Rubber band (right)", EquilibriumIconFactory.GetNamed("band"), new Vector2(90, 70));
    }

    public void ResetTray() => BuildTray();

    private EquilibriumDragDrop2D MakeItem(string id, string label, Sprite sprite, Vector2 size)
    {
        var go = new GameObject(id);
        go.transform.SetParent(tray, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.sprite = sprite != null ? sprite : EquilibriumIconFactory.White();
        img.preserveAspect = true;
        img.color = Color.white;
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = size.x;
        le.preferredHeight = size.y + 36f;
        le.minHeight = size.y + 28f;
        var drag = go.AddComponent<EquilibriumDragDrop2D>();
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
