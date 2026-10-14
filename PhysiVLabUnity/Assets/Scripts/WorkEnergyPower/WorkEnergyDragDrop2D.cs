using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorkEnergyDragDrop2D : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private string itemId;
    [SerializeField] private bool isDraggable = true;
    [SerializeField] private bool lockHorizontal;
    [SerializeField] private bool returnOnInvalid = true;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Transform homeParent;
    private Vector2 homeAnchoredPos;
    private Vector2 dragStartPos;
    private bool dragging;
    private WorkEnergyUIDropTarget hoverTarget;

    public string ItemId => itemId;
    public bool IsPlaced { get; private set; }
    public System.Action<WorkEnergyDragDrop2D> OnCorrectDrop;
    public System.Action<WorkEnergyDragDrop2D> OnIncorrectDrop;
    public System.Action<WorkEnergyDragDrop2D> OnReturned;
    public System.Action<WorkEnergyDragDrop2D, Vector2> OnDragMoved;
    public System.Action<WorkEnergyDragDrop2D> OnClicked;

    public void Configure(string id, bool lockX = false)
    {
        itemId = id;
        lockHorizontal = lockX;
    }

    public void StoreHome(Transform parent, Vector2 anchoredPos)
    {
        homeParent = parent;
        homeAnchoredPos = anchoredPos;
    }

    public void NotifyHoverTarget(WorkEnergyUIDropTarget target) => hoverTarget = target;

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
        dragStartPos = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.92f;
        transform.SetAsLastSibling();
        if (canvas != null) transform.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || rectTransform == null || canvas == null) return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            Vector3 next = localPoint;
            if (lockHorizontal)
                next.x = rectTransform.localPosition.x;
            rectTransform.localPosition = next;
            OnDragMoved?.Invoke(this, rectTransform.anchoredPosition);
        }
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
            OnCorrectDrop?.Invoke(this);
            return;
        }

        if (returnOnInvalid)
        {
            ReturnHome();
            OnIncorrectDrop?.Invoke(this);
            OnReturned?.Invoke(this);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDraggable || IsPlaced || dragging) return;
        OnClicked?.Invoke(this);
        var target = WorkEnergyLabWorkbench.Instance != null ? WorkEnergyLabWorkbench.Instance.FindZoneForItem(itemId) : null;
        if (target != null && target.CanAccept(this))
        {
            target.AcceptDrop(this);
            OnCorrectDrop?.Invoke(this);
        }
    }

    private WorkEnergyUIDropTarget FindDropTarget(PointerEventData eventData)
    {
        if (EventSystem.current == null) return null;
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var hit in results)
        {
            var target = hit.gameObject.GetComponentInParent<WorkEnergyUIDropTarget>();
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

    public Vector2 DragDeltaFromStart()
    {
        if (rectTransform == null) return Vector2.zero;
        return rectTransform.anchoredPosition - dragStartPos;
    }
}
