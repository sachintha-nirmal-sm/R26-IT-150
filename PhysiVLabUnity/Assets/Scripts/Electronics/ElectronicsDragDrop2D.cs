using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ElectronicsDragDrop2D : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private string itemId;
    [SerializeField] private bool isDraggable = true;
    [SerializeField] private bool returnOnInvalid = true;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Transform homeParent;
    private Vector2 homeAnchoredPos;
    private Vector2 homeSize = new Vector2(170, 92);
    private bool dragging;
    private ElectronicsUIDropTarget hoverTarget;

    public string ItemId => itemId;
    public bool IsPlaced { get; private set; }
    public System.Action<ElectronicsDragDrop2D> OnCorrectDrop;
    public System.Action<ElectronicsDragDrop2D> OnIncorrectDrop;
    public System.Action<ElectronicsDragDrop2D> OnClicked;

    public void Configure(string id)
    {
        itemId = id;
    }

    public void StoreHome(Transform parent, Vector2 anchoredPos)
    {
        homeParent = parent;
        homeAnchoredPos = anchoredPos;
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null) homeSize = rectTransform.sizeDelta;
    }

    public void NotifyHoverTarget(ElectronicsUIDropTarget target) => hoverTarget = target;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        if (GetComponent<Collider2D>() == null)
        {
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1.6f, 0.9f);
        }
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
        dragging = true;
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.92f;
        }
        transform.SetAsLastSibling();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas != null) transform.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || rectTransform == null) return;
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            rectTransform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        var target = hoverTarget != null ? hoverTarget : FindDropTarget(eventData);
        hoverTarget = null;
        if (target != null && target.CanAccept(this))
        {
            target.AcceptDrop(this);
            OnCorrectDrop?.Invoke(this);
            return;
        }

        if (target != null && !target.CanAccept(this))
        {
            ElectronicsCircuitConnectionManager.Instance?.NotifyWrongDrop(this, target);
            if (returnOnInvalid && !IsPlaced)
            {
                ReturnHome();
                OnIncorrectDrop?.Invoke(this);
            }
            return;
        }

        if (returnOnInvalid && !IsPlaced)
        {
            ReturnHome();
            ElectronicsFeedbackManager.Instance?.ShowInstruction("Incorrect position.");
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            OnIncorrectDrop?.Invoke(this);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDraggable || dragging) return;
        OnClicked?.Invoke(this);
        if (!IsPlaced) ElectronicsEquipmentSnapController.Instance?.PlaceFromClick(this);
    }

    private ElectronicsUIDropTarget FindDropTarget(PointerEventData eventData)
    {
        if (EventSystem.current == null) return null;
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var hit in results)
        {
            var target = hit.gameObject.GetComponentInParent<ElectronicsUIDropTarget>();
            if (target != null) return target;
        }
        return null;
    }

    public void SnapTo(Transform parent, Vector2 anchoredPos)
    {
        transform.SetParent(parent, false);
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = new Vector2(6, 6);
        rectTransform.offsetMax = new Vector2(-6, -6);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        IsPlaced = true;
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
            rectTransform.sizeDelta = homeSize;
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
