using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NewtonUIDropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
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

    public bool CanAccept(NewtonDragDrop2D item)
    {
        if (item == null) return false;
        if (string.IsNullOrEmpty(acceptedItemId) || acceptedItemId == "Any") return true;
        if (acceptedItemId == "Marker")
            return item.ItemId != null && item.ItemId.StartsWith("Marker");
        return string.Equals(item.ItemId, acceptedItemId);
    }

    public void OnDrop(PointerEventData eventData)
    {
        var item = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<NewtonDragDrop2D>() : null;
        if (item == null || !CanAccept(item)) return;
        AcceptDrop(item);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var item = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<NewtonDragDrop2D>() : null;
        item?.NotifyHoverTarget(this);
        if (item != null && CanAccept(item) && background != null)
            background.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var item = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<NewtonDragDrop2D>() : null;
        if (item != null) item.NotifyHoverTarget(null);
        if (background != null) background.color = normalColor;
    }

    public void AcceptDrop(NewtonDragDrop2D item)
    {
        Vector2 pos = snapPosition;
        item.SnapTo(transform, pos);
        if (background != null) background.color = normalColor;
        NewtonEquipmentSnapController.Instance?.OnItemDropped(this, item);
    }
}
