using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ElecUIDropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string zoneId;
    [SerializeField] private string acceptedItemId;
    [SerializeField] private Vector2 snapPosition;

    private Image background;
    private Color normalColor = new Color(0.8f, 0.88f, 0.95f, 0.55f);
    private Color hoverColor = new Color(0.55f, 0.85f, 0.65f, 0.75f);

    public string ZoneId => zoneId;
    public string AcceptedItemId => acceptedItemId;

    public void Configure(string zone, string acceptedId, Vector2 snap)
    {
        zoneId = zone;
        acceptedItemId = acceptedId;
        snapPosition = snap;
        background = GetComponent<Image>();
        if (background != null) normalColor = background.color;
    }

    public bool CanAccept(ElecDragDrop2D item)
    {
        if (item == null) return false;
        if (string.IsNullOrEmpty(acceptedItemId) || acceptedItemId == "Any") return true;
        return string.Equals(item.ItemId, acceptedItemId);
    }

    public void OnDrop(PointerEventData eventData)
    {
        var item = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<ElecDragDrop2D>() : null;
        if (item == null || !CanAccept(item)) return;
        AcceptDrop(item);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var item = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<ElecDragDrop2D>() : null;
        item?.NotifyHoverTarget(this);
        if (item != null && CanAccept(item) && background != null)
            background.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var item = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<ElecDragDrop2D>() : null;
        if (item != null) item.NotifyHoverTarget(null);
        if (background != null) background.color = normalColor;
    }

    public void AcceptDrop(ElecDragDrop2D item)
    {
        Vector2 pos = snapPosition;
        if (item != null && zoneId == "CircuitBoard")
        {
            var board = transform as RectTransform;
            var eventPos = item.transform.position;
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(board, RectTransformUtility.WorldToScreenPoint(null, eventPos), null, out local);
            pos = local;
        }
        item.SnapTo(transform, pos);
        if (background != null) background.color = normalColor;
        CircuitBuilder.Instance?.OnItemDropped(zoneId, item);
    }
}
