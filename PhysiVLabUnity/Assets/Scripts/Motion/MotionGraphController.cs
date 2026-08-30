using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MotionGraphController : MonoBehaviour
{
    public static MotionGraphController Instance { get; private set; }

    [SerializeField] private RectTransform distanceGraphArea;
    [SerializeField] private RectTransform velocityGraphArea;
    [SerializeField] private RectTransform accelerationGraphArea;
    [SerializeField] private GameObject dotPrefab;

    private readonly List<GameObject> spawned = new List<GameObject>();
    private bool graphBonusAwarded;

    private void Awake() => Instance = this;

    public void Bind(RectTransform dArea, RectTransform vArea, RectTransform aArea, GameObject dot)
    {
        distanceGraphArea = dArea;
        velocityGraphArea = vArea;
        accelerationGraphArea = aArea;
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
        var samples = MotionDataManager.Instance != null ? MotionDataManager.Instance.Samples : null;
        var trials = MotionDataManager.Instance != null ? MotionDataManager.Instance.Trials : null;
        DrawFromTrials(distanceGraphArea, trials, 0);
        DrawFromTrials(velocityGraphArea, trials, 1);
        DrawAcceleration(accelerationGraphArea, MotionDataManager.Instance != null ? MotionDataManager.Instance.AccelerationTrials : null, samples);
        if (!graphBonusAwarded)
        {
            graphBonusAwarded = true;
            MotionScoreManager.Instance?.AddScore(5, false);
        }
        MotionUIManager.Instance?.SetNextButtonVisible(true);
    }

    private void DrawFromTrials(RectTransform area, IReadOnlyList<MotionTrialData> trials, int kind)
    {
        ClearArea(area);
        if (area == null || trials == null) return;
        float maxX = 0.5f;
        float maxY = kind == 0 ? 5.5f : 3f;
        foreach (var t in trials)
        {
            if (t == null || t.time <= 0f) continue;
            if (t.time > maxX) maxX = t.time;
            if (kind == 0 && t.distance > maxY) maxY = t.distance + 0.5f;
            if (kind == 1 && Mathf.Abs(t.velocity) > maxY) maxY = Mathf.Abs(t.velocity) + 0.5f;
        }
        float w = area.rect.width > 8f ? area.rect.width : 520f;
        float h = area.rect.height > 8f ? area.rect.height : 160f;
        Color color = kind == 0 ? new Color(0.15f, 0.45f, 0.8f) : new Color(0.75f, 0.35f, 0.12f);
        Vector2 prev = Vector2.zero;
        bool hasPrev = false;
        var origin = Point(0f, 0f, maxX, maxY, w, h);
        PlaceDot(area, origin, color);
        prev = origin;
        hasPrev = true;
        foreach (var t in trials)
        {
            if (t == null || t.time <= 0f) continue;
            float yVal = kind == 0 ? t.distance : t.velocity;
            var pos = Point(t.time, yVal, maxX, maxY, w, h);
            PlaceDot(area, pos, color);
            if (hasPrev) PlaceLine(area, prev, pos, color);
            prev = pos;
        }
    }

    private void DrawAcceleration(RectTransform area, IReadOnlyList<AccelerationTrialData> accelTrials, IReadOnlyList<MotionGraphSample> samples)
    {
        ClearArea(area);
        if (area == null) return;
        float maxX = 2.5f;
        float maxY = 2.5f;
        float w = area.rect.width > 8f ? area.rect.width : 520f;
        float h = area.rect.height > 8f ? area.rect.height : 160f;
        Color color = new Color(0.45f, 0.22f, 0.62f);
        if (samples != null && samples.Count > 1)
        {
            Vector2 prev = Vector2.zero;
            bool hasPrev = false;
            foreach (var s in samples)
            {
                if (s == null) continue;
                if (s.time > maxX) maxX = s.time;
                if (Mathf.Abs(s.acceleration) > maxY) maxY = Mathf.Abs(s.acceleration) + 0.4f;
            }
            foreach (var s in samples)
            {
                if (s == null) continue;
                var pos = Point(s.time, s.acceleration, maxX, maxY, w, h);
                PlaceDot(area, pos, color);
                if (hasPrev) PlaceLine(area, prev, pos, color);
                prev = pos;
                hasPrev = true;
            }
            return;
        }
        if (accelTrials == null) return;
        Vector2 p0 = Point(0f, 0f, maxX, maxY, w, h);
        PlaceDot(area, p0, color);
        foreach (var a in accelTrials)
        {
            if (a == null) continue;
            var pos = Point(a.time, a.acceleration, Mathf.Max(maxX, a.time), Mathf.Max(maxY, Mathf.Abs(a.acceleration) + 0.4f), w, h);
            PlaceDot(area, pos, color);
            PlaceLine(area, p0, pos, color);
        }
    }

    private void Draw(RectTransform area, IReadOnlyList<MotionGraphSample> samples, IReadOnlyList<MotionTrialData> trials, int kind)
    {
        if (kind == 2)
        {
            DrawAcceleration(area, MotionDataManager.Instance != null ? MotionDataManager.Instance.AccelerationTrials : null, samples);
            return;
        }
        DrawFromTrials(area, trials, kind);
    }

    private static Vector2 Point(float x, float y, float maxX, float maxY, float w, float h)
    {
        float xNorm = Mathf.Clamp01(x / maxX);
        float yNorm = Mathf.Clamp01((y + (maxY * 0.05f)) / maxY);
        return new Vector2(xNorm * w - w * 0.5f, yNorm * h - h * 0.5f);
    }

    private void PlaceDot(RectTransform area, Vector2 pos, Color color)
    {
        if (dotPrefab == null) return;
        var dot = Object.Instantiate(dotPrefab, area);
        dot.SetActive(true);
        var img = dot.GetComponent<Image>();
        if (img != null) img.color = color;
        var rt = dot.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        spawned.Add(dot);
    }

    private void PlaceLine(RectTransform area, Vector2 a, Vector2 b, Color color)
    {
        var line = new GameObject("Line");
        line.transform.SetParent(area, false);
        var img = line.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        var lr = line.GetComponent<RectTransform>();
        Vector2 dir = b - a;
        lr.sizeDelta = new Vector2(Mathf.Max(4f, dir.magnitude), 4f);
        lr.anchoredPosition = (a + b) * 0.5f;
        lr.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        spawned.Add(line);
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
            var child = area.GetChild(i);
            if (child.name == "DotPrefab" || child.name == "GraphLabel" || child.name == "AxisX" || child.name == "AxisY") continue;
            if (child.name.StartsWith("Line") || child.name.Contains("Dot"))
                Object.Destroy(child.gameObject);
        }
    }
}
