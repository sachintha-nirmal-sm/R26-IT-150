using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrictionGraphController : MonoBehaviour
{
    public static FrictionGraphController Instance { get; private set; }

    [SerializeField] private RectTransform areaGraph;
    [SerializeField] private RectTransform forceGraph;
    [SerializeField] private GameObject dotPrefab;
    private readonly List<GameObject> spawned = new List<GameObject>();
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
        DrawAreaGraph();
        DrawForceTimeGraph();
        if (!graphBonusAwarded)
        {
            graphBonusAwarded = true;
            FrictionScoreManager.Instance?.AddScore(5, false);
        }
        FrictionUIManager.Instance?.SetNextButtonVisible(true);
    }

    public void ResetScoring() => graphBonusAwarded = false;

    private void DrawAreaGraph()
    {
        ClearArea(areaGraph);
        if (areaGraph == null || FrictionTrialManager.Instance == null) return;
        var trials = FrictionTrialManager.Instance.GetAllTrials();
        float maxX = 700f;
        float maxY = 25f;
        float w = areaGraph.rect.width > 8f ? areaGraph.rect.width : 520f;
        float h = areaGraph.rect.height > 8f ? areaGraph.rect.height : 180f;
        Color color = new Color(0.15f, 0.45f, 0.8f);
        Vector2 prev = Vector2.zero;
        bool hasPrev = false;
        foreach (var t in trials)
        {
            if (t == null || !t.completed) continue;
            var pos = Point(t.contactArea, t.limitingFriction, maxX, maxY, w, h);
            PlaceDot(areaGraph, pos, color);
            PlaceLabel(areaGraph, pos + new Vector2(0f, 16f), $"S{t.surfaceName} {t.limitingFriction:0.0} N");
            if (hasPrev) PlaceLine(areaGraph, prev, pos, color);
            prev = pos;
            hasPrev = true;
        }
        PlaceLabel(areaGraph, new Vector2(w * 0.5f, 8f), "Contact Area (cm²)");
        PlaceLabel(areaGraph, new Vector2(18f, h - 12f), "Limiting Friction (N)");
    }

    private void DrawForceTimeGraph()
    {
        ClearArea(forceGraph);
        if (forceGraph == null) return;
        var samples = FrictionMeasurementManager.Instance != null ? FrictionMeasurementManager.Instance.Samples : null;
        float w = forceGraph.rect.width > 8f ? forceGraph.rect.width : 520f;
        float h = forceGraph.rect.height > 8f ? forceGraph.rect.height : 180f;
        if (samples == null || samples.Count < 2)
        {
            PlaceLabel(forceGraph, new Vector2(w * 0.5f, h * 0.5f), "Pull the block to record applied force vs time.");
            return;
        }
        float maxX = 0.5f;
        float maxY = 20f;
        foreach (var s in samples)
        {
            if (s.time > maxX) maxX = s.time;
            if (s.appliedForce > maxY) maxY = s.appliedForce + 2f;
        }
        Color applied = new Color(0.75f, 0.28f, 0.12f);
        Vector2 prev = Vector2.zero;
        bool hasPrev = false;
        foreach (var s in samples)
        {
            var pos = Point(s.time, s.appliedForce, maxX, maxY, w, h);
            if (hasPrev) PlaceLine(forceGraph, prev, pos, applied);
            prev = pos;
            hasPrev = true;
        }
        float limit = LimitingFrictionDetector.Instance != null ? LimitingFrictionDetector.Instance.DetectedReading : 18f;
        if (limit > 0f)
        {
            var a = Point(0f, limit, maxX, maxY, w, h);
            var b = Point(maxX, limit, maxX, maxY, w, h);
            PlaceLine(forceGraph, a, b, new Color(0.12f, 0.55f, 0.28f));
            PlaceLabel(forceGraph, b + new Vector2(-80f, 12f), $"Limiting ≈ {limit:0.0} N");
        }
        PlaceLabel(forceGraph, new Vector2(w * 0.5f, 8f), "Time (s)");
        PlaceLabel(forceGraph, new Vector2(90f, h - 12f), "Applied Force (N)");
    }

    private Vector2 Point(float x, float y, float maxX, float maxY, float w, float h)
    {
        float px = 28f + (x / Mathf.Max(0.01f, maxX)) * (w - 48f);
        float py = 28f + (y / Mathf.Max(0.01f, maxY)) * (h - 48f);
        return new Vector2(px, py);
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
        img.sprite = FrictionIconFactory.White();
        img.color = color;
        img.raycastTarget = false;
        spawned.Add(go);
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
        img.sprite = FrictionIconFactory.White();
        img.color = color;
        img.raycastTarget = false;
        spawned.Add(go);
    }

    private void PlaceLabel(RectTransform area, Vector2 pos, string text)
    {
        var go = new GameObject("Lbl");
        go.transform.SetParent(area, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(220f, 24f);
        var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 16;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = new Color(0.15f, 0.18f, 0.22f);
        tmp.raycastTarget = false;
        if (tmp.font == null) tmp.font = TMPro.TMP_Settings.defaultFontAsset;
        spawned.Add(go);
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
