using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class WorkEnergyDepthMeasurementManager : MonoBehaviour, IDragHandler
{
    public static WorkEnergyDepthMeasurementManager Instance { get; private set; }

    [SerializeField] private float measuredDepth;
    [SerializeField] private float depthTolerance = 0.1f;
    [SerializeField] private bool depthMeasured;

    private RectTransform marker;
    private TextMeshProUGUI depthLabel;
    private float maxVisual = 52f;

    public float MeasuredDepth => measuredDepth;
    public bool DepthMeasured => depthMeasured;

    private void Awake()
    {
        if (Instance != null && Instance != this) return;
        Instance = this;
    }

    public void Bind(RectTransform markerRect, TextMeshProUGUI label)
    {
        marker = markerRect;
        depthLabel = label;
        UpdateLabel();
    }

    public void ResetMeasurement()
    {
        depthMeasured = false;
        measuredDepth = 0f;
        UpdateLabel();
    }

    public bool ConfirmDepth(float target)
    {
        bool ok = Mathf.Abs(measuredDepth - target) <= depthTolerance;
        depthMeasured = ok;
        return ok;
    }

    public void SetDepthFromSlider(float depthCm)
    {
        measuredDepth = Mathf.Clamp(depthCm, 0f, 3.5f);
        measuredDepth = Mathf.Round(measuredDepth * 10f) / 10f;
        UpdateLabel();
        if (marker != null)
        {
            var pos = marker.anchoredPosition;
            float max = WorkEnergyDepressionController.Instance != null ? WorkEnergyDepressionController.Instance.MaximumDepth : 3f;
            pos.y = -Mathf.Lerp(4f, maxVisual, max > 0f ? measuredDepth / max : 0f);
            marker.anchoredPosition = pos;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        var step = WorkEnergyPowerExperimentManager.Instance != null
            ? WorkEnergyPowerExperimentManager.Instance.CurrentStep
            : WorkEnergyExperimentStep.MeasureDepression;
        if (step != WorkEnergyExperimentStep.MeasureDepression) return;
        if (marker == null) return;
        marker.anchoredPosition += new Vector2(0f, eventData.delta.y * 0.45f);
        float y = Mathf.Abs(marker.anchoredPosition.y);
        float max = WorkEnergyDepressionController.Instance != null ? WorkEnergyDepressionController.Instance.MaximumDepth : 3f;
        measuredDepth = Mathf.Clamp((y / maxVisual) * max, 0f, max);
        measuredDepth = Mathf.Round(measuredDepth * 10f) / 10f;
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (depthLabel != null)
            depthLabel.text = $"Depression Depth = {measuredDepth:0.0} cm";
    }
}
