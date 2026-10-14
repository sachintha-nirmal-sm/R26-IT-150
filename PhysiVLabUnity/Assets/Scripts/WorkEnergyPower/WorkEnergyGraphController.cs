using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkEnergyGraphController : MonoBehaviour
{
    public static WorkEnergyGraphController Instance { get; private set; }

    [SerializeField] private RectTransform peGraphArea;
    [SerializeField] private RectTransform depthGraphArea;
    [SerializeField] private GameObject dotPrefab;

    private readonly List<GameObject> spawned = new List<GameObject>();
    private bool graphBonusAwarded;

    private void Awake() => Instance = this;

    public void Bind(RectTransform peArea, RectTransform depthArea, GameObject dot)
    {
        peGraphArea = peArea;
        depthGraphArea = depthArea;
        dotPrefab = dot;
        if (dotPrefab != null) dotPrefab.SetActive(false);
        graphBonusAwarded = false;
    }

    public void ShowGraphs()
    {
        var readings = WorkEnergyExperimentDataManager.Instance != null ? WorkEnergyExperimentDataManager.Instance.Readings : null;
        Draw(peGraphArea, readings, true);
        Draw(depthGraphArea, readings, false);
        if (!graphBonusAwarded)
        {
            graphBonusAwarded = true;
            WorkEnergyScoreManager.Instance?.AddScore(5);
        }
        WorkEnergyUIManager.Instance?.SetNextButtonVisible(true);
    }

    private void Draw(RectTransform area, IReadOnlyList<EnergyHeightReading> readings, bool peGraph)
    {
        ClearArea(area);
        if (area == null || readings == null || readings.Count == 0) return;

        float maxX = 0.7f;
        float maxY = 1f;
        foreach (var r in readings)
        {
            maxX = Mathf.Max(maxX, r.height);
            maxY = Mathf.Max(maxY, peGraph ? r.potentialEnergy : r.depressionDepth);
        }
        if (maxY < 0.1f) maxY = 1f;

        Vector2 prev = Vector2.zero;
        bool hasPrev = false;
        float w = area.rect.width > 8f ? area.rect.width : 520f;
        float h = area.rect.height > 8f ? area.rect.height : 220f;

        foreach (var r in readings)
        {
            float xNorm = r.height / maxX;
            float yVal = peGraph ? r.potentialEnergy : r.depressionDepth;
            float yNorm = yVal / maxY;
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
                img.color = peGraph ? new Color(0.15f, 0.45f, 0.8f) : new Color(0.75f, 0.35f, 0.12f);
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
