using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeverMeasurementManager : MonoBehaviour
{
    public static LeverMeasurementManager Instance { get; private set; }

    [SerializeField] private TMP_InputField distanceAInput;
    [SerializeField] private Button confirmAButton;
    [SerializeField] private Transform xButtonsContainer;
    [SerializeField] private TextMeshProUGUI labelA;
    [SerializeField] private TextMeshProUGUI labelX;
    [SerializeField] private TextMeshProUGUI labelP;
    [SerializeField] private GameObject measureAPanel;
    [SerializeField] private GameObject measureXPanel;

    private Button[] xButtons;
    private float lastMeasuredA;
    private float lastMeasuredX;
    private bool distanceAValidated;
    private bool distanceXValidated;

    public bool DistanceAValidated => distanceAValidated;
    public bool DistanceXValidated => distanceXValidated;
    public float LastMeasuredA => lastMeasuredA;
    public float LastMeasuredX => lastMeasuredX;

    private void Awake() => Instance = this;

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Bind(
        TMP_InputField aInput,
        Button confirmA,
        Transform xContainer,
        TextMeshProUGUI aLabel,
        TextMeshProUGUI xLabel,
        TextMeshProUGUI pLabel,
        Button[] xSelectionButtons = null,
        GameObject aPanel = null,
        GameObject xPanel = null)
    {
        distanceAInput = aInput;
        confirmAButton = confirmA;
        xButtonsContainer = xContainer;
        labelA = aLabel;
        labelX = xLabel;
        labelP = pLabel;
        xButtons = xSelectionButtons;
        measureAPanel = aPanel;
        measureXPanel = xPanel;

        if (confirmAButton != null)
        {
            confirmAButton.onClick.RemoveAllListeners();
            confirmAButton.onClick.AddListener(MeasureDistanceA);
        }

        UpdateLabels();
        BuildOrWireXButtons();
        ShowMeasureAUI(false);
        ShowMeasureXUI(false);
    }

    public void ShowMeasureAUI(bool visible)
    {
        if (measureAPanel != null) measureAPanel.SetActive(visible);
        if (distanceAInput != null) distanceAInput.gameObject.SetActive(visible);
        if (confirmAButton != null) confirmAButton.gameObject.SetActive(visible);
    }

    public void ShowMeasureXUI(bool visible)
    {
        if (measureXPanel != null) measureXPanel.SetActive(visible);
        if (xButtonsContainer != null) xButtonsContainer.gameObject.SetActive(visible);
        if (xButtons == null) return;
        foreach (var b in xButtons)
            if (b != null) b.gameObject.SetActive(visible);
    }

    public void MeasureDistanceA()
    {
        if (LeverExperimentManager.Instance != null &&
            LeverExperimentManager.Instance.CurrentStep != LeverExperimentStep.MeasureDistanceA)
        {
            LeverFeedbackManager.Instance?.ShowInstruction("Measure distance a when prompted.");
            return;
        }

        float measured = 0f;
        if (distanceAInput != null && !string.IsNullOrWhiteSpace(distanceAInput.text))
        {
            string raw = distanceAInput.text.Trim().Replace(',', '.');
            float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out measured);
        }

        ValidateDistanceA(measured);
    }

    /// <summary>Quick-select button for distance a (e.g. 20 cm).</summary>
    public void SelectDistanceA(float value)
    {
        if (distanceAInput != null)
            distanceAInput.text = value.ToString("0", CultureInfo.InvariantCulture);
        ValidateDistanceA(value);
    }

    public void ValidateDistanceA(float measuredA)
    {
        lastMeasuredA = measuredA;
        var data = LeverExperimentDataManager.Instance;
        float expected = data != null ? data.distanceA : 20f;
        float tolerance = data != null ? Mathf.Max(data.distanceTolerance, 1.5f) : 1.5f;

        if (Mathf.Abs(measuredA - expected) <= tolerance)
        {
            distanceAValidated = true;
            LeverScoreManager.Instance?.AddScore(5);
            LeverFeedbackManager.Instance?.ShowInstruction($"Distance a = {expected:0} cm confirmed. +5 marks");
            ShowMeasureAUI(false);
            LeverExperimentManager.Instance?.AdvanceStep();
        }
        else
        {
            distanceAValidated = false;
            LeverScoreManager.Instance?.SubtractScore(5);
            LeverGameManager.Instance?.RegisterMistake();
            LeverFeedbackManager.Instance?.ShowInstruction(
                $"Incorrect. Distance a from book to pivot P is about {expected:0} cm. Tap {expected:0} cm.");
        }
    }

    public void MeasureDistanceX(float selectedX)
    {
        if (LeverExperimentManager.Instance != null &&
            LeverExperimentManager.Instance.CurrentStep != LeverExperimentStep.SelectDistanceX)
        {
            LeverFeedbackManager.Instance?.ShowInstruction("Select distance x when prompted.");
            return;
        }

        ValidateDistanceX(selectedX);
    }

    public void ValidateDistanceX(float measuredX)
    {
        lastMeasuredX = measuredX;
        var data = LeverExperimentDataManager.Instance;
        float expected = data != null ? data.GetCurrentX() : measuredX;
        float tolerance = data != null ? Mathf.Max(data.distanceTolerance, 0.5f) : 0.5f;

        if (Mathf.Abs(measuredX - expected) <= tolerance)
        {
            distanceXValidated = true;
            LeverScoreManager.Instance?.AddScore(5);
            LeverFeedbackManager.Instance?.ShowInstruction($"Distance x = {expected:0} cm selected. +5 marks");
            ShowMeasureXUI(false);
            LeverExperimentManager.Instance?.AdvanceStep();
        }
        else
        {
            distanceXValidated = false;
            LeverScoreManager.Instance?.SubtractScore(5);
            LeverGameManager.Instance?.RegisterMistake();
            LeverFeedbackManager.Instance?.ShowInstruction(
                $"This trial needs x = {expected:0} cm. Tap the {expected:0} cm button.");
        }
    }

    public bool ValidateDistance(float measured, float expected, float tolerance) =>
        Mathf.Abs(measured - expected) <= tolerance;

    public void ResetMeasurement()
    {
        lastMeasuredA = 0f;
        lastMeasuredX = 0f;
        distanceAValidated = false;
        distanceXValidated = false;
        if (distanceAInput != null) distanceAInput.text = "";
        UpdateLabels();
        BuildOrWireXButtons();
        ShowMeasureAUI(false);
        ShowMeasureXUI(false);
    }

    public void PrepareForNextX()
    {
        distanceXValidated = false;
        lastMeasuredX = 0f;
        UpdateLabels();
        BuildOrWireXButtons();
        ShowMeasureXUI(true);
    }

    public void UpdateLabels()
    {
        var data = LeverExperimentDataManager.Instance;
        float a = data != null ? data.distanceA : 20f;
        float x = data != null ? data.GetCurrentX() : 10f;

        if (labelA != null) labelA.text = $"a = {a:0} cm";
        if (labelX != null) labelX.text = $"Select x = {x:0} cm";
        if (labelP != null) labelP.text = "P";
    }

    private void BuildOrWireXButtons()
    {
        var data = LeverExperimentDataManager.Instance;
        if (data == null || data.distanceXValues == null) return;

        if (xButtonsContainer == null) return;

        for (int i = xButtonsContainer.childCount - 1; i >= 0; i--)
        {
            var child = xButtonsContainer.GetChild(i).gameObject;
            if (Application.isPlaying) Object.Destroy(child);
            else Object.DestroyImmediate(child);
        }

        float targetX = data.GetCurrentX();
        xButtons = new Button[data.distanceXValues.Length];
        for (int i = 0; i < data.distanceXValues.Length; i++)
        {
            float xVal = data.distanceXValues[i];
            bool isTarget = Mathf.Abs(xVal - targetX) < 0.01f;

            var go = new GameObject($"X_{xVal:0}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(xButtonsContainer, false);
            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = 110f;
            le.preferredHeight = 52f;
            le.minWidth = 100f;
            le.minHeight = 48f;
            var img = go.GetComponent<Image>();
            img.color = isTarget
                ? new Color(0.25f, 0.7f, 0.4f, 1f)
                : new Color(0.82f, 0.9f, 1f, 1f);
            var btn = go.GetComponent<Button>();

            var textGo = new GameObject("Label", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = $"{xVal:0} cm";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 22f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = isTarget ? Color.white : Color.black;
            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            WireXButton(btn, xVal);
            xButtons[i] = btn;
        }
    }

    private void WireXButton(Button btn, float xVal)
    {
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        float captured = xVal;
        btn.onClick.AddListener(() => MeasureDistanceX(captured));
    }
}
