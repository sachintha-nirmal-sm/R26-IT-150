using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WireDragController : MonoBehaviour
{
    public static WireDragController Instance { get; private set; }

    [SerializeField] private float snapRadius = 56f;

    private ElectricalTerminal pendingStart;
    private ElectricalTerminal dragStart;
    private RectTransform preview;
    private Image previewImage;
    private RectTransform board;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(RectTransform circuitBoard)
    {
        board = circuitBoard;
        EnsurePreview();
    }

    public void HandleTerminalClick(ElectricalTerminal terminal)
    {
        if (terminal == null) return;
        if (pendingStart == null)
        {
            pendingStart = terminal;
            terminal.SetHighlight(true);
            ElecFeedbackManager.Instance?.ShowInstruction("Tap the second terminal to complete the wire.");
            return;
        }

        if (pendingStart == terminal)
        {
            pendingStart.SetHighlight(false);
            pendingStart = null;
            return;
        }

        pendingStart.SetHighlight(false);
        CircuitBuilder.Instance?.TryConnect(pendingStart, terminal);
        pendingStart = null;
    }

    public void BeginWire(ElectricalTerminal terminal, PointerEventData eventData)
    {
        dragStart = terminal;
        if (pendingStart != null)
        {
            pendingStart.SetHighlight(false);
            pendingStart = null;
        }
        EnsurePreview();
        if (preview != null) preview.gameObject.SetActive(true);
        DragWire(eventData);
    }

    public void DragWire(PointerEventData eventData)
    {
        if (dragStart == null || preview == null || board == null) return;
        Vector2 start = dragStart.LocalPositionIn(board);
        Vector2 local;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(board, eventData.position, eventData.pressEventCamera, out local))
            local = start;
        Vector2 dir = local - start;
        float mag = Mathf.Max(1f, dir.magnitude);
        preview.anchoredPosition = (start + local) * 0.5f;
        preview.sizeDelta = new Vector2(mag, 5f);
        preview.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }

    public void EndWire(PointerEventData eventData)
    {
        if (preview != null) preview.gameObject.SetActive(false);
        if (dragStart == null) return;

        ElectricalTerminal target = FindNearestTerminal(eventData);
        if (target != null && target != dragStart)
            CircuitBuilder.Instance?.TryConnect(dragStart, target);
        else
            ElecFeedbackManager.Instance?.ShowInstruction("Drop the wire onto a terminal. Snap zones are large — you do not need pixel-perfect placement.");

        dragStart = null;
    }

    public void CancelPending()
    {
        if (pendingStart != null) pendingStart.SetHighlight(false);
        pendingStart = null;
        dragStart = null;
        if (preview != null) preview.gameObject.SetActive(false);
    }

    private ElectricalTerminal FindNearestTerminal(PointerEventData eventData)
    {
        var terminals = CircuitBuilder.Instance != null ? CircuitBuilder.Instance.AllTerminals() : null;
        if (terminals == null) return null;

        ElectricalTerminal best = null;
        float bestDist = snapRadius;
        foreach (var t in terminals)
        {
            if (t == null || t == dragStart) continue;
            float d = Vector2.Distance(t.ScreenPosition(), eventData.position);
            float allowed = Mathf.Max(snapRadius, t.SnapRadius);
            if (d < bestDist && d <= allowed)
            {
                bestDist = d;
                best = t;
            }
        }
        return best;
    }

    private void EnsurePreview()
    {
        if (preview != null || board == null) return;
        var obj = new GameObject("WirePreview");
        obj.transform.SetParent(board, false);
        preview = obj.AddComponent<RectTransform>();
        preview.anchorMin = preview.anchorMax = new Vector2(0.5f, 0.5f);
        preview.sizeDelta = new Vector2(20f, 5f);
        previewImage = obj.AddComponent<Image>();
        previewImage.sprite = ElecIconFactory.White();
        previewImage.color = new Color(0.2f, 0.55f, 0.85f, 0.85f);
        previewImage.raycastTarget = false;
        obj.SetActive(false);
    }
}
