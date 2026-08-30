using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TurningGraphController : MonoBehaviour
{
    public static TurningGraphController Instance { get; private set; }

    [SerializeField] private RectTransform areaGraph;
    [SerializeField] private RectTransform forceGraph;
    [SerializeField] private GameObject dotPrefab;
    private bool graphBonusAwarded;

    private void Awake() => Instance = this;

    public void Bind(RectTransform area, RectTransform force, GameObject dot)
    {
        areaGraph = area;
        forceGraph = force;
        dotPrefab = dot;
        if (dotPrefab != null) dotPrefab.SetActive(false);
        graphBonusAwarded = false;
    }

    public void ShowGraphs()
    {
        StartCoroutine(ShowWhenReady());
    }

    private IEnumerator ShowWhenReady()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        DrawForceBars();
        DrawMomentLine();
        if (!graphBonusAwarded)
        {
            graphBonusAwarded = true;
            TurningScoreManager.Instance?.AddScore(5, false);
        }
        TurningUIManager.Instance?.SetNextButtonVisible(true);
    }

    public void ResetScoring() => graphBonusAwarded = false;

    private void DrawForceBars()
    {
        ClearArea(areaGraph);
        if (areaGraph == null || TurningTrialManager.Instance == null) return;
        var trials = TurningTrialManager.Instance.GetAllTrials();
        float w = areaGraph.rect.width > 8f ? areaGraph.rect.width : 520f;
        float h = areaGraph.rect.height > 8f ? areaGraph.rect.height : 180f;
        PlaceLabel(areaGraph, new Vector2(w * 0.5f, h - 14f), "Minimum force at D for each tightness");
        float maxY = 8f;
        for (int i = 0; i < trials.Count; i++)
        {
            var t = trials[i];
            if (t == null || !t.completed) continue;
            float x0 = 80f + i * (w - 100f) / 3f;
            PlaceBar(areaGraph, x0, t.forceN, maxY, h, new Color(0.75f, 0.25f, 0.18f), $"{t.forceN:0.0} N");
            PlaceLabel(areaGraph, new Vector2(x0, 14f), $"Tightness {t.tightnessLevel}");
        }
        PlaceLabel(areaGraph, new Vector2(28f, h - 36f), "Force (N)");
    }

    private void DrawMomentLine()
    {
        ClearArea(forceGraph);
        if (forceGraph == null || TurningTrialManager.Instance == null) return;
        var trials = TurningTrialManager.Instance.GetAllTrials();
        float w = forceGraph.rect.width > 8f ? forceGraph.rect.width : 520f;
        float h = forceGraph.rect.height > 8f ? forceGraph.rect.height : 180f;
        PlaceLabel(forceGraph, new Vector2(w * 0.5f, h - 14f), "Moment F × d as the screw is tightened");
        float max = 3.5f;
        Vector2 prev = Vector2.zero;
        bool hasPrev = false;
        foreach (var t in trials)
        {
            if (t == null || !t.completed) continue;
            float x = 50f + (t.trialNumber / 3.2f) * (w - 90f);
            var pos = new Vector2(x, 28f + (t.momentNm / max) * (h - 56f));
            PlaceDot(forceGraph, pos, new Color(0.12f, 0.45f, 0.78f));
            PlaceLabel(forceGraph, pos + new Vector2(0f, 16f), $"{t.momentNm:0.00} N m");
            if (hasPrev) PlaceLine(forceGraph, prev, pos, new Color(0.12f, 0.45f, 0.78f));
            prev = pos;
            hasPrev = true;
        }
        PlaceLabel(forceGraph, new Vector2(w * 0.5f, 10f), "Tightness (trial)     moment increases as friction at O increases");
    }

    private void PlaceBar(RectTransform area, float x, float value, float maxY, float h, Color color, string label)
    {
        float bh = (value / Mathf.Max(0.01f, maxY)) * (h - 50f);
        var go = new GameObject("Bar");
        go.transform.SetParent(area, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(x, 28f);
        rt.sizeDelta = new Vector2(36f, bh);
        var img = go.AddComponent<Image>();
        img.sprite = TurningIconFactory.White();
        img.color = color;
        img.raycastTarget = false;
        PlaceLabel(area, new Vector2(x, 28f + bh + 10f), label);
    }

    private void PlaceDot(RectTransform area, Vector2 pos, Color color)
    {
        var go = new GameObject("Dot");
        go.transform.SetParent(area, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(12f, 12f);
        var img = go.AddComponent<Image>();
        img.sprite = TurningIconFactory.White();
        img.color = color;
        img.raycastTarget = false;
    }

    private void PlaceLine(RectTransform area, Vector2 a, Vector2 b, Color color)
    {
        var go = new GameObject("Line");
        go.transform.SetParent(area, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0f, 0.5f);
        Vector2 delta = b - a;
        rt.anchoredPosition = a;
        rt.sizeDelta = new Vector2(delta.magnitude, 4f);
        rt.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        var img = go.AddComponent<Image>();
        img.sprite = TurningIconFactory.White();
        img.color = color;
        img.raycastTarget = false;
    }

    private void PlaceLabel(RectTransform area, Vector2 pos, string text)
    {
        var go = new GameObject("Lbl");
        go.transform.SetParent(area, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(420f, 24f);
        var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 16;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = new Color(0.15f, 0.18f, 0.22f);
        tmp.raycastTarget = false;
        if (tmp.font == null) tmp.font = TMPro.TMP_Settings.defaultFontAsset;
    }

    private void ClearArea(RectTransform area)
    {
        if (area == null) return;
        for (int i = area.childCount - 1; i >= 0; i--)
        {
            var child = area.GetChild(i);
            if (dotPrefab != null && child.gameObject == dotPrefab) continue;
            Object.Destroy(child.gameObject);
        }
    }
}
