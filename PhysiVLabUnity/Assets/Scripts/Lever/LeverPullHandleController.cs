using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LeverPullHandleController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public static LeverPullHandleController Instance { get; private set; }

    [SerializeField] private RectTransform handle;
    [SerializeField] private float maxPullDown = 280f;
    [SerializeField] private float horizontalPenaltyThreshold = 55f;
    [SerializeField] private float tapForceStep = 2f;

    private Vector2 startAnchored;
    private bool dragging;
    private bool horizontalPenalizedThisDrag;
    private Canvas canvas;
    private Camera eventCamera;

    private void Awake()
    {
        Instance = this;
        if (handle == null) handle = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (handle != null) startAnchored = handle.anchoredPosition;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Bind(RectTransform handleRect)
    {
        handle = handleRect != null ? handleRect : GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (handle != null)
        {
            // Prefer bottom-anchored handle so downward drag is natural.
            startAnchored = handle.anchoredPosition;
        }

        // Ensure the handle can receive UI events.
        var img = GetComponent<Image>();
        if (img != null) img.raycastTarget = true;
    }

    private bool CanPull()
    {
        var step = LeverExperimentManager.Instance?.CurrentStep;
        return step == LeverExperimentStep.PullBalance || step == LeverExperimentStep.ObserveLift;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (handle == null || !CanPull()) return;
        dragging = true;
        horizontalPenalizedThisDrag = false;
        eventCamera = eventData.pressEventCamera;
        NewtonSpringBalanceController.Instance?.StartPull();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || handle == null || canvas == null) return;

        float dx = eventData.delta.x;
        float dy = eventData.delta.y;

        if (!horizontalPenalizedThisDrag && Mathf.Abs(dx) > horizontalPenaltyThreshold && Mathf.Abs(dx) > Mathf.Abs(dy))
        {
            horizontalPenalizedThisDrag = true;
            LeverScoreManager.Instance?.SubtractScore(5);
            LeverGameManager.Instance?.RegisterMistake();
            LeverFeedbackManager.Instance?.ShowInstruction("Pull the spring balance vertically downward.");
        }

        // Convert pointer to local canvas space for reliable touch/mouse pull.
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                handle.parent as RectTransform, eventData.position, eventCamera, out Vector2 localPoint))
        {
            Vector2 pos = handle.anchoredPosition;
            pos.x = startAnchored.x;
            float minY = startAnchored.y - maxPullDown;
            pos.y = Mathf.Clamp(localPoint.y, minY, startAnchored.y);
            handle.anchoredPosition = pos;
        }
        else
        {
            Vector2 pos = handle.anchoredPosition;
            pos.x = startAnchored.x;
            pos.y = Mathf.Clamp(pos.y + dy, startAnchored.y - maxPullDown, startAnchored.y);
            handle.anchoredPosition = pos;
        }

        ApplyForceFromHandle();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;
        NewtonSpringBalanceController.Instance?.StopPull();
        ApplyForceFromHandle();
    }

    /// <summary>Tap the handle to increase force (mobile-friendly backup).</summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (dragging || !CanPull()) return;
        NewtonSpringBalanceController.Instance?.StartPull();
        float current = NewtonSpringBalanceController.Instance != null
            ? NewtonSpringBalanceController.Instance.GetReading()
            : 0f;
        float next = current + tapForceStep;
        NewtonSpringBalanceController.Instance?.SetForceDirect(next);

        // Move handle visually with force.
        if (handle != null && NewtonSpringBalanceController.Instance != null)
        {
            float maxF = Mathf.Max(0.01f, NewtonSpringBalanceController.Instance.maximumForce);
            float t = Mathf.Clamp01(next / maxF);
            Vector2 pos = handle.anchoredPosition;
            pos.x = startAnchored.x;
            pos.y = startAnchored.y - maxPullDown * t;
            handle.anchoredPosition = pos;
        }

        float force = NewtonSpringBalanceController.Instance != null
            ? NewtonSpringBalanceController.Instance.GetReading()
            : 0f;
        LeverLabWorkbench.Instance?.OnPullForceChanged(force);
    }

    public void AddForceStep(float amount = -1f)
    {
        if (!CanPull()) return;
        if (amount < 0f) amount = tapForceStep;
        float current = NewtonSpringBalanceController.Instance != null
            ? NewtonSpringBalanceController.Instance.GetReading()
            : 0f;
        NewtonSpringBalanceController.Instance?.SetForceDirect(current + amount);
        ApplyForceFromHandle(true);
    }

    private void ApplyForceFromHandle(bool fromButton = false)
    {
        if (!fromButton && handle != null)
        {
            float pullDistance = startAnchored.y - handle.anchoredPosition.y;
            NewtonSpringBalanceController.Instance?.UpdateForce(pullDistance);
        }

        float force = NewtonSpringBalanceController.Instance != null
            ? NewtonSpringBalanceController.Instance.GetReading()
            : 0f;
        LeverLabWorkbench.Instance?.OnPullForceChanged(force);
    }

    public void ResetHandle()
    {
        dragging = false;
        horizontalPenalizedThisDrag = false;
        if (handle != null) handle.anchoredPosition = startAnchored;
        NewtonSpringBalanceController.Instance?.ResetBalance();
    }
}
