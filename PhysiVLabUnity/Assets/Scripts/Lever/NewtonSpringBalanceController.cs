using TMPro;
using UnityEngine;

public class NewtonSpringBalanceController : MonoBehaviour
{
    public static NewtonSpringBalanceController Instance { get; private set; }

    public float currentForce;
    public float maximumForce = 30f;
    public float forcePerPixel = 0.12f;

    [SerializeField] private TextMeshProUGUI forceDisplay;
    [SerializeField] private RectTransform springVisual;
    [SerializeField] private LeverSpringController springController;

    private bool pulling;

    public bool IsPulling => pulling;
    public float CurrentForce => currentForce;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Bind(TextMeshProUGUI display, RectTransform spring, LeverSpringController springCtrl = null)
    {
        forceDisplay = display;
        springVisual = spring;
        springController = springCtrl;
        if (springController == null && springVisual != null)
            springController = springVisual.GetComponent<LeverSpringController>();
        UpdateDisplay();
    }

    public void StartPull()
    {
        pulling = true;
    }

    public void UpdateForce(float pullDistance)
    {
        float force = Mathf.Max(0f, pullDistance) * forcePerPixel;
        currentForce = Mathf.Clamp(force, 0f, maximumForce);
        ApplyVisuals();
        UpdateDisplay();
    }

    public void SetForceDirect(float force)
    {
        currentForce = Mathf.Clamp(force, 0f, maximumForce);
        ApplyVisuals();
        UpdateDisplay();
    }

    public void StopPull()
    {
        pulling = false;
    }

    public void ResetBalance()
    {
        pulling = false;
        currentForce = 0f;
        ApplyVisuals();
        UpdateDisplay();
    }

    public float GetReading() => currentForce;

    private void ApplyVisuals()
    {
        if (springController != null)
            springController.SetExtension(currentForce, maximumForce);
        else if (springVisual != null)
        {
            float t = maximumForce > 0f ? currentForce / maximumForce : 0f;
            var size = springVisual.sizeDelta;
            size.y = Mathf.Lerp(40f, 160f, t);
            springVisual.sizeDelta = size;
        }
    }

    private void UpdateDisplay()
    {
        if (forceDisplay != null)
            forceDisplay.text = $"{currentForce:0.0} N";
    }
}
