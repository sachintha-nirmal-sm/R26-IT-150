using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ElecDragDrop2D : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private string itemId;
    [SerializeField] private bool isDraggable = true;
    [SerializeField] private bool returnOnInvalid = true;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Transform homeParent;
    private Vector2 homeAnchoredPos;
    private bool dragging;
    private ElecUIDropTarget hoverTarget;
    private ElectricalComponent component;

    public string ItemId => itemId;
    public bool IsPlaced { get; private set; }
    public System.Action<ElecDragDrop2D> OnCorrectDrop;
    public System.Action<ElecDragDrop2D> OnIncorrectDrop;
    public System.Action<ElecDragDrop2D> OnClicked;

    public void Configure(string id)
    {
        itemId = id;
        component = GetComponent<ElectricalComponent>();
    }

    public void StoreHome(Transform parent, Vector2 anchoredPos)
    {
        homeParent = parent;
        homeAnchoredPos = anchoredPos;
    }

    public void NotifyHoverTarget(ElecUIDropTarget target) => hoverTarget = target;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        component = GetComponent<ElectricalComponent>();
    }

    public void SetDraggable(bool value)
    {
        isDraggable = value;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = (value || IsPlaced) ? 1f : 0.45f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;
        if (eventData.pointerEnter != null && eventData.pointerEnter.GetComponent<ElectricalTerminal>() != null)
            return;

        dragging = true;
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
            rectTransform.localPosition = localPoint;
        CircuitBuilder.Instance?.RefreshWires();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        var target = hoverTarget != null ? hoverTarget : FindDropTarget(eventData);
        hoverTarget = null;
        if (target != null && target.CanAccept(this))
        {
            target.AcceptDrop(this);
            OnCorrectDrop?.Invoke(this);
            CircuitBuilder.Instance?.RefreshWires();
            return;
        }

        if (IsPlaced && CircuitBuilder.Instance != null && CircuitBuilder.Instance.IsPointOnBoard(eventData.position, eventData.pressEventCamera))
        {
            CircuitBuilder.Instance.PlaceFree(this, eventData);
            CircuitBuilder.Instance.RefreshWires();
            return;
        }

        if (returnOnInvalid && !IsPlaced)
        {
            ReturnHome();
            OnIncorrectDrop?.Invoke(this);
        }
        else if (IsPlaced)
            CircuitBuilder.Instance?.RefreshWires();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDraggable || dragging) return;
        OnClicked?.Invoke(this);
        if (!IsPlaced)
            CircuitBuilder.Instance?.PlaceFromClick(this);
    }

    private ElecUIDropTarget FindDropTarget(PointerEventData eventData)
    {
        if (EventSystem.current == null) return null;
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var hit in results)
        {
            var target = hit.gameObject.GetComponentInParent<ElecUIDropTarget>();
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
        if (component != null) component.MarkPlaced(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
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
        if (component != null) component.MarkPlaced(false);
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
