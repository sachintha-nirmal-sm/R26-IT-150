using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeverGraphView : MonoBehaviour
{
    public static LeverGraphView Instance { get; private set; }

    [SerializeField] private RectTransform graphArea;
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private Image lineImage;

    private readonly List<GameObject> dots = new List<GameObject>();

    private void Awake() => Instance = this;

    public void Bind(RectTransform area, GameObject dot, Image line)
    {
        graphArea = area;
        dotPrefab = dot;
        lineImage = line;
    }

    public void UpdateGraph(IReadOnlyList<LeverReading> readings)
    {
        foreach (var d in dots) if (d != null) Destroy(d);
        dots.Clear();
        if (graphArea == null || readings == null || readings.Count == 0) return;

        float maxX = 40f;
        float maxEffort = 40f;
        foreach (var r in readings)
        {
            maxX = Mathf.Max(maxX, r.distanceX);
            maxEffort = Mathf.Max(maxEffort, Mathf.Max(r.measuredEffort, r.requiredEffort));
        }

        Vector2 prev = Vector2.zero;
        bool hasPrev = false;

        // Plot X = distanceX, Y = effort (measured preferred, else required).
        var ordered = new List<LeverReading>(readings);
        ordered.Sort((a, b) => a.distanceX.CompareTo(b.distanceX));

        foreach (var r in ordered)
        {
            float effort = r.measuredEffort > 0f ? r.measuredEffort : r.requiredEffort;
            float xNorm = r.distanceX / maxX;
            float yNorm = 1f - (effort / maxEffort);
            var pos = new Vector2(xNorm * graphArea.rect.width - graphArea.rect.width * 0.5f,
                yNorm * graphArea.rect.height - graphArea.rect.height * 0.5f);

            if (dotPrefab != null)
            {
                var dot = Instantiate(dotPrefab, graphArea);
                dot.GetComponent<RectTransform>().anchoredPosition = pos;
                dots.Add(dot);
            }

            if (hasPrev && lineImage != null)
            {
                var lr = lineImage.rectTransform;
                Vector2 dir = pos - prev;
                lr.sizeDelta = new Vector2(dir.magnitude, 3f);
                lr.anchoredPosition = (pos + prev) * 0.5f;
                lr.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            }
            prev = pos;
            hasPrev = true;
        }
    }
}
