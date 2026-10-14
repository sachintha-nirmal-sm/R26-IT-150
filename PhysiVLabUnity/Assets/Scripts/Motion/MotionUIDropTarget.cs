using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MotionUIDropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string zoneId;
    [SerializeField] private string acceptedItemId;
    [SerializeField] private Vector2 snapPosition;
    [SerializeField] private float meterValue;

    private Image background;
    private Color normalColor = new Color(0.8f, 0.88f, 0.95f, 0.55f);
    private Color hoverColor = new Color(0.55f, 0.85f, 0.65f, 0.75f);

    public string ZoneId => zoneId;
    public string AcceptedItemId => acceptedItemId;
    public float MeterValue => meterValue;

    public void Configure(string zone, string acceptedId, Vector2 snap, float meters = 0f)
    {
        zoneId = zone;
        acceptedItemId = acceptedId;
        snapPosition = snap;
        meterValue = meters;
        background = GetComponent<Image>();
        if (background != null) normalColor = background.color;
    }

    public bool CanAccept(MotionDragDrop2D item)
    {
        if (item == null) return false;
        if (string.IsNullOrEmpty(acceptedItemId) || acceptedItemId == "Any") return true;
        if (acceptedItemId == "Marker")
            return item.ItemId != null && item.ItemId.StartsWith("Marker");
        return string.Equals(item.ItemId, acceptedItemId);
    }

    public void OnDrop(PointerEventData eventData)
    {
        var item = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<MotionDragDrop2D>() : null;
        if (item == null || !CanAccept(item)) return;
        AcceptDrop(item);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var item = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<MotionDragDrop2D>() : null;
        item?.NotifyHoverTarget(this);
        if (item != null && CanAccept(item) && background != null)
            background.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var item = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<MotionDragDrop2D>() : null;
        if (item != null) item.NotifyHoverTarget(null);
        if (background != null) background.color = normalColor;
    }

    public void AcceptDrop(MotionDragDrop2D item)
    {
        Vector2 pos = snapPosition;
        if (zoneId == "Track" || zoneId == "Ruler" || zoneId == "Start" || zoneId == "Marker")
        {
            if (MotionPositionController.Instance != null)
            {
                float meters = zoneId == "Start" ? 0f : (zoneId == "Marker" ? meterValue : 2.5f);
                pos = MotionPositionController.Instance.AnchoredPositionForMeters(meters);
                if (zoneId == "Ruler") pos.y = -36f;
                else if (zoneId == "Track") pos.y = 0f;
                else pos.y = 18f;
            }
        }
        item.SnapTo(transform, pos);
        if (background != null) background.color = normalColor;
        MotionTrackController.Instance?.OnItemDropped(this, item);
    }
}
