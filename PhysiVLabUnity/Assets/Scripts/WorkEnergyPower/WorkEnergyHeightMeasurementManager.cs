using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class WorkEnergyHeightMeasurementManager : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    public static WorkEnergyHeightMeasurementManager Instance { get; private set; }

    [SerializeField] private float currentHeight;
    [SerializeField] private float heightTolerance = 0.02f;
    [SerializeField] private bool heightMeasured;
    [SerializeField] private bool heightSetConfirmed;

    private RectTransform marker;
    private RectTransform scaleRoot;
    private TextMeshProUGUI heightLabel;
    private float clayY;
    private float oneMetreY;
    private float clayNorm = 0.12f;
    private float oneNorm = 0.92f;

    public float CurrentHeight => currentHeight;
    public bool HeightMeasured => heightMeasured;
    public bool HeightSetConfirmed => heightSetConfirmed;

    private void Awake()
    {
        if (Instance != null && Instance != this) return;
        Instance = this;
    }

    public void Bind(RectTransform markerRect, RectTransform scale, TextMeshProUGUI label, float clayNormalized, float oneMetreNormalized)
    {
        marker = markerRect;
        scaleRoot = scale;
        heightLabel = label;
        clayNorm = clayNormalized;
        oneNorm = oneMetreNormalized;
        RefreshScale();
        UpdateLabel();
    }

    private void RefreshScale()
    {
        if (scaleRoot == null) return;
        float h = scaleRoot.rect.height > 1f ? scaleRoot.rect.height : 520f;
        clayY = clayNorm * h;
        oneMetreY = oneNorm * h;
    }

    public void SetDisplayedHeight(float h)
    {
        currentHeight = Mathf.Clamp(h, 0f, 1.05f);
        WorkEnergyReleaseMechanismController.Instance?.SetHeight(currentHeight);
        ApplyMarker();
        UpdateLabel();
    }

    public bool ConfirmSetHeight(float target)
    {
        float h = WorkEnergyReleaseMechanismController.Instance != null
            ? WorkEnergyReleaseMechanismController.Instance.CurrentHeight
            : currentHeight;
        currentHeight = h;
        bool ok = Mathf.Abs(h - target) <= heightTolerance;
        heightSetConfirmed = ok;
        UpdateLabel();
        return ok;
    }

    public bool ConfirmMeasuredHeight(float target)
    {
        bool ok = Mathf.Abs(currentHeight - target) <= heightTolerance;
        heightMeasured = ok;
        return ok;
    }

    public void ResetMeasurement(bool keepHeightValue = false)
    {
        heightMeasured = false;
        heightSetConfirmed = false;
        if (!keepHeightValue) currentHeight = 0.5f;
        UpdateLabel();
    }

    public void OnBeginDrag(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        if (marker == null || scaleRoot == null) return;
        RefreshScale();
        var step = WorkEnergyPowerExperimentManager.Instance != null
            ? WorkEnergyPowerExperimentManager.Instance.CurrentStep
            : WorkEnergyExperimentStep.MeasureHeight;
        if (step != WorkEnergyExperimentStep.SetHeight && step != WorkEnergyExperimentStep.MeasureHeight) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(scaleRoot, eventData.position, eventData.pressEventCamera, out Vector2 local);
        float y = Mathf.Clamp(local.y, Mathf.Min(clayY, oneMetreY), Mathf.Max(clayY, oneMetreY));
        var pos = marker.anchoredPosition;
        pos.y = y;
        marker.anchoredPosition = pos;
        float span = oneMetreY - clayY;
        if (Mathf.Abs(span) > 0.001f)
            currentHeight = Mathf.Clamp((y - clayY) / span, 0f, 1.05f);
        UpdateLabel();
        WorkEnergyLabWorkbench.Instance?.OnHeightMarkerMoved(currentHeight);
    }

    private void ApplyMarker()
    {
        if (marker == null) return;
        RefreshScale();
        float span = oneMetreY - clayY;
        var pos = marker.anchoredPosition;
        pos.y = clayY + currentHeight * span;
        marker.anchoredPosition = pos;
    }

    private void UpdateLabel()
    {
        if (heightLabel != null)
            heightLabel.text = $"HEIGHT  h = {currentHeight:0.00} m";
    }
}
