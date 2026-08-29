using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ElectricalTerminal : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private string terminalId;
    [SerializeField] private string polarity;
    [SerializeField] private ElectricalComponent owner;
    [SerializeField] private float snapRadius = 56f;

    private Image image;
    private RectTransform rectTransform;

    public string TerminalId => terminalId;
    public string Polarity => polarity;
    public ElectricalComponent Owner => owner;
    public float SnapRadius => snapRadius;

    public void Configure(string id, string pole, ElectricalComponent component)
    {
        terminalId = id;
        polarity = pole;
        owner = component;
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = true;
            image.color = pole == "+" ? new Color(0.82f, 0.18f, 0.18f) : (pole == "-" ? new Color(0.12f, 0.12f, 0.14f) : new Color(0.75f, 0.55f, 0.12f));
        }
    }

    public Vector2 ScreenPosition()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        return RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);
    }

    public Vector2 LocalPositionIn(RectTransform parent)
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        Vector3 world = rectTransform.position;
        Vector2 local = parent.InverseTransformPoint(world);
        return local;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        WireDragController.Instance?.BeginWire(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        WireDragController.Instance?.DragWire(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        WireDragController.Instance?.EndWire(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging) return;
        WireDragController.Instance?.HandleTerminalClick(this);
    }

    public void SetHighlight(bool on)
    {
        if (image == null) image = GetComponent<Image>();
        if (image == null) return;
        Color baseColor = polarity == "+" ? new Color(0.82f, 0.18f, 0.18f) : (polarity == "-" ? new Color(0.12f, 0.12f, 0.14f) : new Color(0.75f, 0.55f, 0.12f));
        image.color = on ? Color.Lerp(baseColor, Color.yellow, 0.55f) : baseColor;
        transform.localScale = on ? Vector3.one * 1.18f : Vector3.one;
    }
}
