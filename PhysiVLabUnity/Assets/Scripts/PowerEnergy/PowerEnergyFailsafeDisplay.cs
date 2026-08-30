using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public class PowerEnergyFailsafeDisplay : MonoBehaviour
{
    private static GameObject overlay;
    private static bool practicalStarted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        overlay = null;
        practicalStarted = false;
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);
        if (practicalStarted) yield break;
        if (PowerEnergyUIManager.Instance != null) yield break;
        CreateOverlay();
    }

    public static void Hide()
    {
        practicalStarted = true;
        if (overlay != null)
        {
            Destroy(overlay);
            overlay = null;
        }
        var leftover = GameObject.Find("PowerEnergy_FailsafeRoot");
        if (leftover != null) Destroy(leftover);
    }

    private static void CreateOverlay()
    {
        if (overlay != null) return;
        overlay = new GameObject("PowerEnergy_FailsafeRoot");
        Object.DontDestroyOnLoad(overlay);

        var canvasGo = new GameObject("FailsafeCanvas");
        canvasGo.transform.SetParent(overlay.transform, false);
        canvasGo.layer = 5;
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        var panel = CreateUiPanel(canvasGo.transform, "Panel", Color.white);
        Stretch(panel);

        var header = CreateUiPanel(panel.transform, "Header", new Color(0.08f, 0.28f, 0.48f));
        var hrt = header.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0f, 0.82f);
        hrt.anchorMax = Vector2.one;
        hrt.offsetMin = hrt.offsetMax = Vector2.zero;
        CreateUiText(header.transform, "POWER AND ENERGY", 40, Color.white, TextAnchor.MiddleCenter);

        CreateUiText(panel.transform, "PRACTICAL\n\nPower and Energy of Electric Appliances\nP = VI     E = Pt     1 kWh = 3,600,000 J\n\nClick anywhere or press START", 30, new Color(0.10f, 0.18f, 0.28f), TextAnchor.MiddleCenter);

        var btnGo = CreateUiPanel(panel.transform, "StartBtn", new Color(0.12f, 0.62f, 0.42f));
        var brt = btnGo.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.5f, 0.12f);
        brt.anchorMax = new Vector2(0.5f, 0.12f);
        brt.pivot = new Vector2(0.5f, 0.5f);
        brt.sizeDelta = new Vector2(520f, 110f);
        CreateUiText(btnGo.transform, "START PRACTICAL", 36, Color.white, TextAnchor.MiddleCenter);

        var btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = btnGo.GetComponent<Image>();
        btn.onClick.AddListener(Begin);

        var panelBtn = panel.AddComponent<Button>();
        panelBtn.targetGraphic = panel.GetComponent<Image>();
        panelBtn.onClick.AddListener(Begin);
    }

    private static void Begin()
    {
        Hide();
        if (PowerEnergyExperimentManager.Instance != null)
            PowerEnergyExperimentManager.Instance.StartPractical();
        else if (PowerEnergyUIManager.Instance != null)
            PowerEnergyUIManager.Instance.StartPractical();
    }

    private static GameObject CreateUiPanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    private static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private static void CreateUiText(Transform parent, string text, int size, Color color, TextAnchor align)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(24, 24);
        rt.offsetMax = new Vector2(-24, -24);
        var t = go.AddComponent<Text>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.raycastTarget = false;
    }
}
