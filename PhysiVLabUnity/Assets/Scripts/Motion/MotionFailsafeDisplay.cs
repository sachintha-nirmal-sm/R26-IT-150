using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public class MotionFailsafeDisplay : MonoBehaviour
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
        if (MotionUIManager.Instance != null) yield break;
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
        var leftover = GameObject.Find("Motion_FailsafeRoot");
        if (leftover != null) Destroy(leftover);
    }

    private static void CreateOverlay()
    {
        if (overlay != null) return;
        overlay = new GameObject("Motion_FailsafeRoot");
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

        var header = CreateUiPanel(panel.transform, "Header", new Color(0.10f, 0.22f, 0.40f));
        var hrt = header.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0f, 0.82f);
        hrt.anchorMax = Vector2.one;
        hrt.offsetMin = hrt.offsetMax = Vector2.zero;
        CreateUiText(header.transform, "MOTION", 42, Color.white, TextAnchor.MiddleCenter);

        CreateUiText(panel.transform, "PRACTICAL\n\nInvestigating Distance, Displacement, Speed, Velocity and Acceleration\n\nClick anywhere or press START", 32, new Color(0.12f, 0.16f, 0.22f), TextAnchor.MiddleCenter);

        var btnGo = CreateUiPanel(panel.transform, "StartBtn", new Color(0.12f, 0.62f, 0.35f));
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

        EnsureEventSystemSafe();
        Debug.Log("Motion emergency start overlay created.");
    }

    private static void Begin()
    {
        Hide();
        if (MotionUIManager.Instance != null)
            MotionUIManager.Instance.StartPractical();
        else
            Debug.LogWarning("Failsafe: MotionUIManager not ready. Stop Play and press Play again.");
    }

    private static void EnsureEventSystemSafe()
    {
        try
        {
            var es = Object.FindAnyObjectByType<EventSystem>();
            if (es == null)
            {
                var go = new GameObject("EventSystem");
                es = go.AddComponent<EventSystem>();
            }
#if ENABLE_INPUT_SYSTEM
            if (es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
                es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            if (es.GetComponent<StandaloneInputModule>() == null)
                es.gameObject.AddComponent<StandaloneInputModule>();
#endif
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Failsafe EventSystem: " + ex.Message);
        }
    }

    private static GameObject CreateUiPanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = true;
        return go;
    }

    private static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private static Text CreateUiText(Transform parent, string content, int size, Color color, TextAnchor align)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(24f, 24f);
        rt.offsetMax = new Vector2(-24f, -24f);
        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null) text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = content;
        text.fontSize = size;
        text.color = color;
        text.alignment = align;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }
}
