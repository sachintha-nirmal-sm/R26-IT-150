using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpticsVisualController : MonoBehaviour
{
    public static OpticsVisualController Instance { get; private set; }

    public const float TrueFocalLengthCm = 20f;
    public const float MinDistanceCm = 8f;
    public const float MaxDistanceCm = 36f;
    public const float FocusToleranceCm = 1.2f;

    private GameObject closedWindow;
    private GameObject openWindow;
    private GameObject outdoorScene;
    private GameObject mirrorVisual;
    private GameObject screenVisual;
    private GameObject rulerVisual;
    private GameObject raysVisual;
    private RectTransform screenRt;
    private Image screenImage;
    private Image outdoorImage;
    private TextMeshProUGUI distanceLabel;
    private TextMeshProUGUI imageStatusLabel;
    private Slider distanceSlider;
    private float screenDistanceCm = 12f;

    public float ScreenDistanceCm => screenDistanceCm;
    public bool IsInFocus => Mathf.Abs(screenDistanceCm - TrueFocalLengthCm) <= FocusToleranceCm;
    public float ImageSharpness
    {
        get
        {
            float error = Mathf.Abs(screenDistanceCm - TrueFocalLengthCm);
            return Mathf.Clamp01(1f - error / 10f);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(
        GameObject closed, GameObject open, GameObject scene,
        GameObject mirror, GameObject screen, GameObject ruler, GameObject rays,
        TextMeshProUGUI distLabel, TextMeshProUGUI statusLabel, Slider slider)
    {
        closedWindow = closed;
        openWindow = open;
        outdoorScene = scene;
        mirrorVisual = mirror;
        screenVisual = screen;
        rulerVisual = ruler;
        raysVisual = rays;
        distanceLabel = distLabel;
        imageStatusLabel = statusLabel;
        distanceSlider = slider;
        if (screen != null)
        {
            screenRt = screen.GetComponent<RectTransform>();
            var imgT = screen.transform.Find("SceneImage");
            if (imgT != null) screenImage = imgT.GetComponent<Image>();
        }
        if (scene != null) outdoorImage = scene.GetComponent<Image>();
        if (distanceSlider != null)
        {
            distanceSlider.minValue = MinDistanceCm;
            distanceSlider.maxValue = MaxDistanceCm;
            distanceSlider.wholeNumbers = false;
            distanceSlider.onValueChanged.RemoveAllListeners();
            distanceSlider.onValueChanged.AddListener(SetScreenDistance);
            screenDistanceCm = 12f;
            distanceSlider.SetValueWithoutNotify(12f);
        }
        ResetVisuals();
    }

    public void ResetVisuals()
    {
        screenDistanceCm = 12f;
        if (distanceSlider != null)
            distanceSlider.SetValueWithoutNotify(12f);
        ShowWindowOpen(false);
        ShowMirror(false);
        ShowScreen(false);
        ShowRuler(false);
        if (raysVisual != null) raysVisual.SetActive(false);
        ApplyScreenPosition();
        RefreshImage();
    }

    public void ShowWindowOpen(bool open)
    {
        if (closedWindow != null) closedWindow.SetActive(!open);
        if (openWindow != null) openWindow.SetActive(open);
        if (outdoorScene != null) outdoorScene.SetActive(open);
        RefreshRays();
    }

    public void ShowMirror(bool show)
    {
        if (mirrorVisual != null) mirrorVisual.SetActive(show);
        RefreshRays();
    }

    public void ShowScreen(bool show)
    {
        if (screenVisual != null) screenVisual.SetActive(show);
        ApplyScreenPosition();
        RefreshImage();
        RefreshRays();
    }

    public void ShowRuler(bool show)
    {
        if (rulerVisual != null) rulerVisual.SetActive(show);
    }

    public void SetScreenDistance(float cm)
    {
        screenDistanceCm = Mathf.Clamp(cm, MinDistanceCm, MaxDistanceCm);
        ApplyScreenPosition();
        RefreshImage();
        OpticsUIManager.Instance?.UpdateLiveReadings();
    }

    public bool TryConfirmFocus()
    {
        if (OpticsAssemblyManager.Instance == null || !OpticsAssemblyManager.Instance.SetupConfirmed)
        {
            OpticsFeedbackManager.Instance?.ShowInstruction("Confirm the setup before adjusting the screen.");
            return false;
        }
        if (!IsInFocus)
        {
            OpticsScoreManager.Instance?.SubtractScore(5);
            OpticsFeedbackManager.Instance?.ShowMessage(
                "✗ NOT SHARP YET\nMove the screen until the upside-down image of the outdoor scene is very clear.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            return false;
        }
        return true;
    }

    private void ApplyScreenPosition()
    {
        if (screenRt == null) return;
        float t = Mathf.InverseLerp(MinDistanceCm, MaxDistanceCm, screenDistanceCm);
        float x = Mathf.Lerp(0.58f, 0.22f, t);
        screenRt.anchorMin = new Vector2(x - 0.055f, 0.18f);
        screenRt.anchorMax = new Vector2(x + 0.055f, 0.78f);
        screenRt.offsetMin = screenRt.offsetMax = Vector2.zero;
    }

    private void RefreshImage()
    {
        bool ready = screenVisual != null && screenVisual.activeInHierarchy &&
                     mirrorVisual != null && mirrorVisual.activeInHierarchy;
        float sharp = ready ? ImageSharpness : 0f;
        if (screenImage != null)
        {
            screenImage.enabled = true;
            screenImage.rectTransform.localScale = new Vector3(1f, -1f, 1f);
            float a = ready ? Mathf.Lerp(0.18f, 1f, sharp) : 0f;
            screenImage.color = new Color(1f, 1f, 1f, a);
        }

        if (distanceLabel != null)
            distanceLabel.text = ready ? $"Mirror–screen distance:  {screenDistanceCm:0.0} cm" : "Mirror–screen distance:  —";

        if (imageStatusLabel != null)
        {
            if (!ready) imageStatusLabel.text = "Image on screen:  —";
            else if (IsInFocus) imageStatusLabel.text = "Image on screen:  CLEAR, UPSIDE DOWN (real image)";
            else if (sharp > 0.55f) imageStatusLabel.text = "Image on screen:  almost sharp, still slightly blurred";
            else imageStatusLabel.text = "Image on screen:  blurred  —  move the screen";
        }
    }

    private void RefreshRays()
    {
        bool show = outdoorScene != null && outdoorScene.activeSelf &&
                    mirrorVisual != null && mirrorVisual.activeSelf;
        if (raysVisual != null) raysVisual.SetActive(show);
    }
}
