using UnityEngine;
using UnityEngine.UI;

public class ResultantEquipmentManager : MonoBehaviour
{
    public static ResultantEquipmentManager Instance { get; private set; }

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

        MakeItem("Trolley", "Trolley", ResultantIconFactory.GetNamed("trolley"), new Vector2(150, 90));
        MakeItem("Ring", "Ring", ResultantIconFactory.GetNamed("ring"), new Vector2(90, 70));
        MakeItem("Strings", "Strings", ResultantIconFactory.GetNamed("strings"), new Vector2(150, 50));
        MakeItem("Pulley1", "Pulley 1", ResultantIconFactory.GetNamed("pulley"), new Vector2(90, 90));
        MakeItem("Pulley2", "Pulley 2", ResultantIconFactory.GetNamed("pulley"), new Vector2(90, 90));
        MakeItem("BalanceA", "Balance A", ResultantIconFactory.GetNamed("balance"), new Vector2(150, 90));
        MakeItem("BalanceB", "Balance B", ResultantIconFactory.GetNamed("balance"), new Vector2(150, 90));
        MakeItem("BalanceC", "Balance C", ResultantIconFactory.GetNamed("balance"), new Vector2(150, 90));
    }

    public void ResetTray() => BuildTray();

    private ResultantDragDrop2D MakeItem(string id, string label, Sprite sprite, Vector2 size)
    {
        var go = new GameObject(id);
        go.transform.SetParent(tray, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.sprite = sprite != null ? sprite : ResultantIconFactory.White();
        img.preserveAspect = true;
        img.color = Color.white;
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = size.x;
        le.preferredHeight = size.y + 36f;
        le.minHeight = size.y + 28f;
        var drag = go.AddComponent<ResultantDragDrop2D>();
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
        tmp.fontSize = 22;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = new Color(0.12f, 0.16f, 0.22f);
        tmp.raycastTarget = false;
        if (tmp.font == null) tmp.font = TMPro.TMP_Settings.defaultFontAsset;
        return drag;
    }
}
