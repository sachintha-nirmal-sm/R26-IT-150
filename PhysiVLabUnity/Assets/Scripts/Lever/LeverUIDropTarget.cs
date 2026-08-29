using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LeverUIDropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string zoneId;
    [SerializeField] private string acceptedItemId;
    [SerializeField] private Vector2 snapPosition;

    private LeverDraggableUIItem currentItem;
    private Image background;
    private Color normalColor = new Color(0.8f, 0.88f, 0.95f, 0.6f);
    private Color hoverColor = new Color(0.55f, 0.85f, 0.65f, 0.75f);

    public string ZoneId => zoneId;

    public void Configure(string zone, string acceptedId, Vector2 snap)
    {
        zoneId = zone;
        acceptedItemId = acceptedId;
        snapPosition = snap;
        background = GetComponent<Image>();
        if (background != null) normalColor = background.color;
    }

    public bool CanAccept(LeverDraggableUIItem item)
    {
        if (item == null || item.ItemId != acceptedItemId) return false;
        return currentItem == null || currentItem == item;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var item = eventData.pointerDrag?.GetComponent<LeverDraggableUIItem>();
        if (item == null || !CanAccept(item)) return;
        AcceptDrop(item);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var item = eventData.pointerDrag?.GetComponent<LeverDraggableUIItem>();
        item?.NotifyHoverTarget(this);
        if (item != null && CanAccept(item) && background != null)
            background.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var item = eventData.pointerDrag?.GetComponent<LeverDraggableUIItem>();
        if (item != null) item.NotifyHoverTarget(null);
        if (background != null) background.color = normalColor;
    }

    public void AcceptDrop(LeverDraggableUIItem item)
    {
        currentItem = item;
        item.SnapTo(transform, snapPosition);
        if (background != null) background.color = normalColor;
        LeverLabWorkbench.Instance?.OnItemDropped(zoneId, item);
    }

    public void ClearItem()
    {
        currentItem = null;
    }
}
