using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LeverDraggableUIItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private string itemId;
    [SerializeField] private bool isDraggable = true;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Transform homeParent;
    private Vector2 homeAnchoredPos;
    private bool dragging;
    private LeverUIDropTarget hoverTarget;

    public string ItemId => itemId;
    public bool IsPlaced { get; private set; }

    public void Configure(string id) => itemId = id;

    public void StoreHome(Transform parent, Vector2 anchoredPos)
    {
        homeParent = parent;
        homeAnchoredPos = anchoredPos;
    }

    // Kept for compatibility with drop-zone hover feedback.
    public void NotifyHoverTarget(LeverUIDropTarget target) => hoverTarget = target;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
    }

    public void SetDraggable(bool value)
    {
        isDraggable = value;
        var img = GetComponent<Image>();
        if (img != null) img.raycastTarget = true;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = (value || IsPlaced) ? 1f : 0.45f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable || IsPlaced) return;
        dragging = true;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.9f;
        transform.SetAsLastSibling();
        if (canvas != null) transform.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || rectTransform == null || canvas == null) return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            rectTransform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;
        canvasGroup.blocksRaycasts = true;

        var target = hoverTarget != null ? hoverTarget : FindDropTarget(eventData);
        hoverTarget = null;
        if (target != null && target.CanAccept(this))
        {
            target.AcceptDrop(this);
            return;
        }

        ReturnHome();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDraggable || IsPlaced || dragging) return;
        var target = LeverLabWorkbench.Instance?.FindZoneForItem(itemId);
        if (target != null && target.CanAccept(this))
            target.AcceptDrop(this);
    }

    private LeverUIDropTarget FindDropTarget(PointerEventData eventData)
    {
        if (EventSystem.current == null) return null;
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var hit in results)
        {
            var target = hit.gameObject.GetComponentInParent<LeverUIDropTarget>();
            if (target != null) return target;
        }
        return null;
    }

    public void SnapTo(Transform parent, Vector2 anchoredPos)
    {
        transform.SetParent(parent, false);
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPos;
        rectTransform.localScale = Vector3.one;
        IsPlaced = true;
        isDraggable = false;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void ReturnHome()
    {
        if (homeParent != null) transform.SetParent(homeParent, false);
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = homeAnchoredPos;
            rectTransform.localScale = Vector3.one;
        }
        IsPlaced = false;
        isDraggable = true;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void ResetItem()
    {
        IsPlaced = false;
        isDraggable = true;
        ReturnHome();
    }
}
