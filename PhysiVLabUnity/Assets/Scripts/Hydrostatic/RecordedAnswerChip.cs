using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Draggable recorded reading (e.g. 1.2 N) for the Phase 3 observation table.
/// The chip stays in the tray; a ghost follows the pointer.
/// </summary>
public class RecordedAnswerChip : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private float valueN;
    [SerializeField] private Text label;

    Canvas rootCanvas;
    RectTransform rect;
    CanvasGroup canvasGroup;
    Transform trayParent;
    Vector3 trayLocalPos;
    GameObject ghost;

    public float ValueN => valueN;

    public void Configure(float value, Text text, Color chipColor)
    {
        valueN = value;
        label = text;
        if (label != null)
            label.text = UpthrustPracticalData.FormatNewton(value) + " N";

        var img = GetComponent<Image>();
        if (img != null)
        {
            img.color = chipColor;
            img.raycastTarget = true;
        }
    }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        trayParent = transform.parent;
        trayLocalPos = transform.localPosition;
        rootCanvas = GetComponentInParent<Canvas>();

        ghost = Instantiate(gameObject, rootCanvas != null ? rootCanvas.transform : transform.root);
        ghost.name = "ChipGhost";
        var ghostChip = ghost.GetComponent<RecordedAnswerChip>();
        if (ghostChip != null)
            Destroy(ghostChip);

        foreach (var g in ghost.GetComponentsInChildren<Graphic>(true))
            g.raycastTarget = false;

        var ghostCg = ghost.GetComponent<CanvasGroup>();
        if (ghostCg == null) ghostCg = ghost.AddComponent<CanvasGroup>();
        ghostCg.blocksRaycasts = false;
        ghostCg.alpha = 0.9f;

        canvasGroup.alpha = 0.45f;
        canvasGroup.blocksRaycasts = false;
        Follow(eventData, ghost.GetComponent<RectTransform>());
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghost != null)
            Follow(eventData, ghost.GetComponent<RectTransform>());
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        UpthrustObservationDropCell drop = FindDropCell(eventData);
        if (drop != null)
            drop.ReceiveDrop(valueN);

        if (ghost != null)
            Destroy(ghost);

        transform.SetParent(trayParent, false);
        transform.localPosition = trayLocalPos;
    }

    private void Follow(PointerEventData eventData, RectTransform target)
    {
        if (target == null || rootCanvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 local);

        target.SetParent(rootCanvas.transform, false);
        target.localPosition = local;
    }

    private static UpthrustObservationDropCell FindDropCell(PointerEventData eventData)
    {
        if (EventSystem.current == null) return null;

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var hit in results)
        {
            var cell = hit.gameObject.GetComponent<UpthrustObservationDropCell>();
            if (cell == null)
                cell = hit.gameObject.GetComponentInParent<UpthrustObservationDropCell>();
            if (cell != null) return cell;
        }

        return null;
    }
}
