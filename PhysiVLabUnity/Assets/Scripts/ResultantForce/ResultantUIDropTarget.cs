using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResultantUIDropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
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

    public bool CanAccept(ResultantDragDrop2D item)
    {
        if (item == null) return false;
        if (string.IsNullOrEmpty(acceptedItemId) || acceptedItemId == "Any") return true;
        if (acceptedItemId.Contains("|"))
        {
            var parts = acceptedItemId.Split('|');
            foreach (var p in parts)
                if (string.Equals(item.ItemId, p.Trim())) return true;
            return false;
        }
        return string.Equals(item.ItemId, acceptedItemId);
    }

    public void OnDrop(PointerEventData eventData)
    {
        var item = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<ResultantDragDrop2D>() : null;
        if (item == null || !CanAccept(item)) return;
        AcceptDrop(item);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var item = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<ResultantDragDrop2D>() : null;
        item?.NotifyHoverTarget(this);
        if (item != null && CanAccept(item) && background != null)
            background.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var item = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<ResultantDragDrop2D>() : null;
        if (item != null) item.NotifyHoverTarget(null);
        if (background != null) background.color = normalColor;
    }

    public void AcceptDrop(ResultantDragDrop2D item)
    {
        item.SnapTo(transform, snapPosition);
        if (background != null) background.color = normalColor;
        ResultantEquipmentSnapController.Instance?.OnItemDropped(this, item);
        ResultantVariableMatchingManager.Instance?.OnItemDropped(this, item);
    }
}
