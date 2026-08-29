using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EquilibriumGraphController : MonoBehaviour
{
    public static EquilibriumGraphController Instance { get; private set; }

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
        DrawSumLine();
        if (!graphBonusAwarded)
        {
            graphBonusAwarded = true;
            EquilibriumScoreManager.Instance?.AddScore(5, false);
        }
        EquilibriumUIManager.Instance?.SetNextButtonVisible(true);
    }

    public void ResetScoring() => graphBonusAwarded = false;

    private void DrawForceBars()
    {
        ClearArea(areaGraph);
        if (areaGraph == null || EquilibriumTrialManager.Instance == null) return;
        var trials = EquilibriumTrialManager.Instance.GetAllTrials();
        float w = areaGraph.rect.width > 8f ? areaGraph.rect.width : 520f;
        float h = areaGraph.rect.height > 8f ? areaGraph.rect.height : 180f;
        PlaceLabel(areaGraph, new Vector2(w * 0.5f, h - 14f), "F1 and F2 for each trial  (upward forces)");
        float maxY = 1.5f;
        for (int i = 0; i < trials.Count; i++)
        {
            var t = trials[i];
            if (t == null || !t.completed) continue;
            float x0 = 90f + i * (w - 120f) / 3f;
            PlaceBar(areaGraph, x0 - 16f, t.force1N, maxY, h, new Color(0.18f, 0.48f, 0.78f), $"F1 {t.force1N:0.00}");
            PlaceBar(areaGraph, x0 + 16f, t.force2N, maxY, h, new Color(0.12f, 0.58f, 0.38f), $"F2 {t.force2N:0.00}");
            PlaceLabel(areaGraph, new Vector2(x0, 14f), $"Trial {t.trialNumber}");
        }
        PlaceLabel(areaGraph, new Vector2(36f, h - 36f), "Force (N)");
    }

    private void DrawSumLine()
    {
        ClearArea(forceGraph);
        if (forceGraph == null || EquilibriumTrialManager.Instance == null) return;
        var trials = EquilibriumTrialManager.Instance.GetAllTrials();
        float w = forceGraph.rect.width > 8f ? forceGraph.rect.width : 520f;
        float h = forceGraph.rect.height > 8f ? forceGraph.rect.height : 180f;
        PlaceLabel(forceGraph, new Vector2(w * 0.5f, h - 14f), "F1 + F2 compared with W  (should be equal)");
        float max = 2.0f;
        float wN = EquilibriumForceController.TrueWeightN;
        float yW = 28f + (wN / max) * (h - 56f);
        PlaceLine(forceGraph, new Vector2(40f, yW), new Vector2(w - 30f, yW), new Color(0.75f, 0.22f, 0.18f));
        PlaceLabel(forceGraph, new Vector2(w - 80f, yW + 14f), $"W = {wN:0.00} N");
        Vector2 prev = Vector2.zero;
        bool hasPrev = false;
        foreach (var t in trials)
        {
            if (t == null || !t.completed) continue;
            float x = 50f + (t.trialNumber / 3.2f) * (w - 90f);
            var pos = new Vector2(x, 28f + (t.sumN / max) * (h - 56f));
            PlaceDot(forceGraph, pos, new Color(0.12f, 0.45f, 0.78f));
            PlaceLabel(forceGraph, pos + new Vector2(0f, 16f), $"{t.sumN:0.00} N");
            if (hasPrev) PlaceLine(forceGraph, prev, pos, new Color(0.12f, 0.45f, 0.78f));
            prev = pos;
            hasPrev = true;
        }
        PlaceLabel(forceGraph, new Vector2(w * 0.5f, 10f), "Trial     F1 + F2 stays equal to W when the ruler is in equilibrium");
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
        rt.sizeDelta = new Vector2(28f, bh);
        var img = go.AddComponent<Image>();
        img.sprite = EquilibriumIconFactory.White();
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
        img.sprite = EquilibriumIconFactory.White();
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
        img.sprite = EquilibriumIconFactory.White();
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
