using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerEnergyUIDropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string zoneId;
    [SerializeField] private string acceptedItemId;
    [SerializeField] private Vector2 snapPosition;

    private Image background;
    private Color normalColor = new Color(0.90f, 0.94f, 0.98f, 0.55f);
    private Color hoverColor = new Color(0.45f, 0.82f, 0.62f, 0.75f);

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

    public bool CanAccept(PowerEnergyDragDrop2D item)
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
        var item = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<PowerEnergyDragDrop2D>() : null;
        if (item == null) return;
        if (!CanAccept(item))
        {
            PowerEnergyCircuitConnectionManager.Instance?.NotifyWrongDrop(item, this);
            return;
        }
        AcceptDrop(item);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var item = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<PowerEnergyDragDrop2D>() : null;
        item?.NotifyHoverTarget(this);
        if (item != null && CanAccept(item) && background != null)
            background.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var item = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<PowerEnergyDragDrop2D>() : null;
        if (item != null) item.NotifyHoverTarget(null);
        if (background != null) background.color = normalColor;
    }

    public void AcceptDrop(PowerEnergyDragDrop2D item)
    {
        item.SnapTo(transform, snapPosition);
        if (background != null) background.color = normalColor;
        PowerEnergyEquipmentSnapController.Instance?.OnItemDropped(this, item);
        PowerEnergyFormulaMatchingManager.Instance?.OnItemDropped(this, item);
    }
}
