using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElecGraphController : MonoBehaviour
{
    public static ElecGraphController Instance { get; private set; }

    [SerializeField] private RectTransform voltageGraphArea;
    [SerializeField] private RectTransform currentGraphArea;
    [SerializeField] private GameObject dotPrefab;

    private readonly List<GameObject> spawned = new List<GameObject>();
    private bool graphBonusAwarded;

    private void Awake() => Instance = this;

    public void Bind(RectTransform vArea, RectTransform iArea, GameObject dot)
    {
        voltageGraphArea = vArea;
        currentGraphArea = iArea;
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
        var readings = ElecExperimentDataManager.Instance != null ? ElecExperimentDataManager.Instance.Readings : null;
        Draw(voltageGraphArea, readings, true);
        Draw(currentGraphArea, readings, false);
        if (!graphBonusAwarded)
        {
            graphBonusAwarded = true;
            ElecScoreManager.Instance?.AddScore(5);
        }
        ElecUIManager.Instance?.SetNextButtonVisible(true);
    }

    private void Draw(RectTransform area, IReadOnlyList<CircuitReading> readings, bool voltageGraph)
    {
        ClearArea(area);
        if (area == null || readings == null) return;

        float maxY = voltageGraph ? 3.2f : 0.4f;
        Vector2 prev = Vector2.zero;
        bool hasPrev = false;
        float w = area.rect.width > 8f ? area.rect.width : 520f;
        float h = area.rect.height > 8f ? area.rect.height : 220f;

        int plotted = 0;
        foreach (var r in readings)
        {
            if (r == null || r.connectionNumber < 1) continue;
            plotted++;
            float xNorm = (r.connectionNumber - 0.5f) / 3f;
            float yVal = voltageGraph ? r.voltage : r.current;
            float yNorm = Mathf.Clamp01(yVal / maxY);
            var pos = new Vector2(xNorm * w - w * 0.5f, yNorm * h - h * 0.5f);

            if (dotPrefab != null)
            {
                var dot = Object.Instantiate(dotPrefab, area);
                dot.SetActive(true);
                var rt = dot.GetComponent<RectTransform>();
                rt.anchoredPosition = pos;
                spawned.Add(dot);
            }

            if (hasPrev)
            {
                var line = new GameObject("Line");
                line.transform.SetParent(area, false);
                var img = line.AddComponent<Image>();
                img.color = voltageGraph ? new Color(0.15f, 0.45f, 0.8f) : new Color(0.75f, 0.35f, 0.12f);
                img.raycastTarget = false;
                var lr = line.GetComponent<RectTransform>();
                Vector2 dir = pos - prev;
                lr.sizeDelta = new Vector2(Mathf.Max(4f, dir.magnitude), 4f);
                lr.anchoredPosition = (pos + prev) * 0.5f;
                lr.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
                spawned.Add(line);
            }
            prev = pos;
            hasPrev = true;
        }

        if (plotted == 0) return;
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
