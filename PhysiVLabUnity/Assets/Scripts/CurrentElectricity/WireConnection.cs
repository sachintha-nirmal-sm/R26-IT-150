using UnityEngine;
using UnityEngine.UI;

public class WireConnection : MonoBehaviour
{
    [SerializeField] private ElectricalTerminal startTerminal;
    [SerializeField] private ElectricalTerminal endTerminal;
    [SerializeField] private bool energized;
    [SerializeField] private Color idleColor = new Color(0.18f, 0.22f, 0.28f);
    [SerializeField] private Color liveColor = new Color(0.95f, 0.72f, 0.12f);

    private RectTransform rectTransform;
    private Image image;
    private RectTransform flowDot;
    private RectTransform parentBoard;
    private float flowT;

    public ElectricalTerminal StartTerminal => startTerminal;
    public ElectricalTerminal EndTerminal => endTerminal;
    public bool Energized => energized;

    public void Bind(ElectricalTerminal start, ElectricalTerminal end, RectTransform board)
    {
        startTerminal = start;
        endTerminal = end;
        parentBoard = board;
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        if (image == null) image = gameObject.AddComponent<Image>();
        image.color = idleColor;
        image.raycastTarget = false;
        EnsureFlowDot();
        RefreshLayout();
    }

    public void SetEnergized(bool value)
    {
        energized = value;
        if (image != null) image.color = value ? liveColor : idleColor;
        if (flowDot != null) flowDot.gameObject.SetActive(value);
    }

    public bool Connects(ElectricalTerminal a, ElectricalTerminal b)
    {
        return (startTerminal == a && endTerminal == b) || (startTerminal == b && endTerminal == a);
    }

    public bool Involves(ElectricalTerminal t)
    {
        return startTerminal == t || endTerminal == t;
    }

    public bool Involves(ElectricalComponent component)
    {
        if (component == null) return false;
        return (startTerminal != null && startTerminal.Owner == component) ||
               (endTerminal != null && endTerminal.Owner == component);
    }

    public void RefreshLayout()
    {
        if (startTerminal == null || endTerminal == null || parentBoard == null) return;
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        Vector2 a = startTerminal.LocalPositionIn(parentBoard);
        Vector2 b = endTerminal.LocalPositionIn(parentBoard);
        Vector2 dir = b - a;
        float mag = dir.magnitude;
        if (mag < 1f) mag = 1f;

        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = (a + b) * 0.5f;
        rectTransform.sizeDelta = new Vector2(mag, energized ? 8f : 6f);
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        rectTransform.SetAsFirstSibling();
    }

    private void Update()
    {
        if (!energized || flowDot == null || rectTransform == null) return;
        flowT += Time.deltaTime * 0.7f;
        if (flowT > 1f) flowT -= 1f;
        float x = (flowT - 0.5f) * rectTransform.sizeDelta.x;
        flowDot.anchoredPosition = new Vector2(x, 0f);
    }

    private void EnsureFlowDot()
    {
        if (flowDot != null) return;
        var obj = new GameObject("Flow");
        obj.transform.SetParent(transform, false);
        flowDot = obj.AddComponent<RectTransform>();
        flowDot.anchorMin = flowDot.anchorMax = new Vector2(0.5f, 0.5f);
        flowDot.sizeDelta = new Vector2(12f, 12f);
        var img = obj.AddComponent<Image>();
        img.sprite = ElecIconFactory.White();
        img.color = new Color(1f, 0.92f, 0.25f);
        img.raycastTarget = false;
        obj.SetActive(false);
    }
}
