using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewtonGraphController : MonoBehaviour
{
    public static NewtonGraphController Instance { get; private set; }

    [SerializeField] private RectTransform forceGraphArea;
    [SerializeField] private RectTransform massGraphArea;
    [SerializeField] private GameObject dotPrefab;

    private readonly List<GameObject> spawned = new List<GameObject>();
    private bool graphBonusAwarded;

    private void Awake() => Instance = this;

    public void Bind(RectTransform forceArea, RectTransform massArea, GameObject dot)
    {
        forceGraphArea = forceArea;
        massGraphArea = massArea;
        dotPrefab = dot;
        if (dotPrefab != null) dotPrefab.SetActive(false);
        graphBonusAwarded = false;
    }

    public void ShowGraphs() => StartCoroutine(ShowWhenReady());

    private IEnumerator ShowWhenReady()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        var data = NewtonDataManager.Instance;
        DrawForceAcceleration(forceGraphArea, data != null ? data.ForceSeries : null);
        DrawMassAcceleration(massGraphArea, data != null ? data.MassSeries : null);
        if (!graphBonusAwarded)
        {
            graphBonusAwarded = true;
            NewtonScoreManager.Instance?.AddScore(5, false);
        }
        NewtonUIManager.Instance?.SetNextButtonVisible(true);
    }

    private void DrawForceAcceleration(RectTransform area, IReadOnlyList<NewtonLawTrialData> trials)
    {
        ClearArea(area);
        if (area == null) return;
        float maxX = 5f, maxY = 5f;
        if (trials != null)
        {
            foreach (var t in trials)
            {
                if (t == null) continue;
                if (t.force > maxX) maxX = t.force + 0.5f;
                if (t.acceleration > maxY) maxY = t.acceleration + 0.5f;
            }
        }
        DrawSeries(area, trials, maxX, maxY, t => t.force, t => t.acceleration, new Color(0.15f, 0.45f, 0.8f));
    }

    private void DrawMassAcceleration(RectTransform area, IReadOnlyList<NewtonLawTrialData> trials)
    {
        ClearArea(area);
        if (area == null) return;
        float maxX = 4f, maxY = 8f;
        if (trials != null)
        {
            foreach (var t in trials)
            {
                if (t == null) continue;
                if (t.mass > maxX) maxX = t.mass + 0.5f;
                if (t.acceleration > maxY) maxY = t.acceleration + 0.5f;
            }
        }
        DrawSeries(area, trials, maxX, maxY, t => t.mass, t => t.acceleration, new Color(0.75f, 0.32f, 0.12f));
    }

    private void DrawSeries(RectTransform area, IReadOnlyList<NewtonLawTrialData> trials, float maxX, float maxY,
        System.Func<NewtonLawTrialData, float> xSel, System.Func<NewtonLawTrialData, float> ySel, Color color)
    {
        float w = area.rect.width > 8f ? area.rect.width : 520f;
        float h = area.rect.height > 8f ? area.rect.height : 180f;
        var origin = Point(0f, 0f, maxX, maxY, w, h);
        PlaceDot(area, origin, color);
        Vector2 prev = origin;
        bool hasPrev = true;
        if (trials == null) return;
        var sorted = new List<NewtonLawTrialData>();
        foreach (var t in trials) if (t != null) sorted.Add(t);
        sorted.Sort((a, b) => xSel(a).CompareTo(xSel(b)));
        foreach (var t in sorted)
        {
            var pos = Point(xSel(t), ySel(t), maxX, maxY, w, h);
            PlaceDot(area, pos, color);
            if (hasPrev) PlaceLine(area, prev, pos, color);
            prev = pos;
            hasPrev = true;
        }
    }

    private Vector2 Point(float x, float y, float maxX, float maxY, float w, float h)
    {
        float px = 24f + (x / Mathf.Max(0.01f, maxX)) * (w - 48f);
        float py = 20f + (y / Mathf.Max(0.01f, maxY)) * (h - 40f);
        return new Vector2(px, py);
    }

    private void PlaceDot(RectTransform area, Vector2 pos, Color color)
    {
        var go = dotPrefab != null ? Object.Instantiate(dotPrefab, area) : new GameObject("Dot", typeof(RectTransform), typeof(Image));
        go.SetActive(true);
        go.transform.SetParent(area, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(14f, 14f);
        rt.anchoredPosition = pos;
        var img = go.GetComponent<Image>();
        if (img != null) img.color = color;
        spawned.Add(go);
    }

    private void PlaceLine(RectTransform area, Vector2 a, Vector2 b, Color color)
    {
        var go = new GameObject("Line", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(area, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0f, 0.5f);
        Vector2 d = b - a;
        float len = d.magnitude;
        rt.sizeDelta = new Vector2(len, 4f);
        rt.anchoredPosition = a;
        float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        rt.localRotation = Quaternion.Euler(0f, 0f, ang);
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        spawned.Add(go);
    }

    private void ClearArea(RectTransform area)
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] != null && (area == null || spawned[i].transform.parent == area))
            {
                Object.Destroy(spawned[i]);
                spawned.RemoveAt(i);
            }
        }
        if (area == null) return;
        for (int i = area.childCount - 1; i >= 0; i--)
        {
            var ch = area.GetChild(i);
            if (ch != null && ch.name != "DotPrefab" && !ch.name.Contains("Label"))
                Object.Destroy(ch.gameObject);
        }
    }

    public void ResetBonus() => graphBonusAwarded = false;
}
