using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Drag helper for Phase 2 apparatus.
/// Works as UI drag (IBeginDragHandler) and as 3D mouse drag (Input System + Collider).
/// </summary>
public class UpthrustDraggableEquipment : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private UpthrustApparatusType apparatusType;
    [SerializeField] private bool lockedAfterSnap = true;
    [SerializeField] private float dragHeightOffset = 0.05f;

    private Camera mainCam;
    private bool isDragging;
    private bool isSnapped;
    private bool isUi;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 startLocalPosition;
    private Transform startParent;
    private Collider cachedCollider;
    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private CanvasGroup canvasGroup;

    public UpthrustApparatusType Type => apparatusType;
    public bool IsSnapped => isSnapped;

    private void Awake()
    {
        mainCam = Camera.main;
        startPosition = transform.position;
        startRotation = transform.rotation;
        startLocalPosition = transform.localPosition;
        startParent = transform.parent;
        cachedCollider = GetComponent<Collider>();
        rectTransform = GetComponent<RectTransform>();
        isUi = rectTransform != null;
        parentCanvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (isUi && canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void Configure(UpthrustApparatusType type)
    {
        apparatusType = type;
    }

    private void Update()
    {
        if (isUi) return;
        if (isSnapped && lockedAfterSnap) return;
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
            TryBeginDrag3D(mouse.position.ReadValue());

        if (isDragging && mouse.leftButton.isPressed)
            DragTo3D(mouse.position.ReadValue());

        if (isDragging && mouse.leftButton.wasReleasedThisFrame)
            isDragging = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isUi || (isSnapped && lockedAfterSnap)) return;
        if (UpthrustPracticalManager.Instance != null && !UpthrustPracticalManager.Instance.IsInteractionAllowed)
            return;

        isDragging = true;
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.8f;
        }

        if (parentCanvas != null)
            transform.SetParent(parentCanvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isUi || !isDragging || parentCanvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        rectTransform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isUi) return;

        isDragging = false;
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        UpthrustSnapZone hitZone = FindZoneUnderPointer(eventData);
        if (hitZone != null && hitZone.TryAccept(this))
            return;

        ReturnToStart();
    }

    private static UpthrustSnapZone FindZoneUnderPointer(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current?.RaycastAll(eventData, results);
        foreach (var hit in results)
        {
            UpthrustSnapZone zone = hit.gameObject.GetComponent<UpthrustSnapZone>();
            if (zone == null)
                zone = hit.gameObject.GetComponentInParent<UpthrustSnapZone>();
            if (zone != null) return zone;
        }

        return null;
    }

    private void TryBeginDrag3D(Vector2 screenPos)
    {
        if (UpthrustPracticalManager.Instance != null &&
            !UpthrustPracticalManager.Instance.IsInteractionAllowed) return;

        Ray ray = mainCam.ScreenPointToRay(screenPos);
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f)) return;
        if (hit.collider != cachedCollider && hit.transform != transform &&
            !hit.transform.IsChildOf(transform)) return;

        isDragging = true;
    }

    private void DragTo3D(Vector2 screenPos)
    {
        Ray ray = mainCam.ScreenPointToRay(screenPos);
        Plane tablePlane = new Plane(Vector3.up, startPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (hit.collider == cachedCollider || hit.transform.IsChildOf(transform))
            {
                if (tablePlane.Raycast(ray, out float enterSelf))
                {
                    Vector3 p = ray.GetPoint(enterSelf);
                    p.y = startPosition.y + dragHeightOffset;
                    transform.position = p;
                }
                return;
            }

            Vector3 point = hit.point;
            point.y += dragHeightOffset;
            transform.position = point;
        }
        else if (tablePlane.Raycast(ray, out float enter))
        {
            Vector3 p = ray.GetPoint(enter);
            p.y = startPosition.y + dragHeightOffset;
            transform.position = p;
        }
    }

    public void SnapTo(Transform target)
    {
        isDragging = false;
        isSnapped = true;
        transform.SetParent(target, true);
        transform.position = target.position;
        transform.rotation = target.rotation;

        if (cachedCollider != null)
            cachedCollider.enabled = false;
    }

    public void ResetEquipment()
    {
        isSnapped = false;
        isDragging = false;
        ReturnToStart();

        if (cachedCollider != null)
            cachedCollider.enabled = true;
    }

    private void ReturnToStart()
    {
        transform.SetParent(startParent, false);
        if (isUi)
            transform.localPosition = startLocalPosition;
        else
        {
            transform.position = startPosition;
            transform.rotation = startRotation;
        }
    }
}
